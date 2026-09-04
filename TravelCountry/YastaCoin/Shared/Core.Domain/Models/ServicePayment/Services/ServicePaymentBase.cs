using AutoMapper;
using Core.Domain.Interfaces;
using Core.Domain.Models.ServicePayment.Requests;
using Grpc.Core;

namespace Core.Domain.Models.ServicePayment.Services
{
    public class ServicePaymentBase(ISwiftPay swiftPay, IMapper mapper, ISwiftPayReverse swiftPayReverse = default!, ISwiftPayPaymentDocument swiftPayPaymentDocument = default!) : Domain.ServicePayment.ServicePaymentBase
    {
        public override async Task<PaymentResponsePb> Payment(PaymentRequestPb request, ServerCallContext context)
        {
            var paymentRequest = mapper.Map<PaymentRequest>(request);
            var paymentResponse = await swiftPay.ExecutePayment(paymentRequest);
            return mapper.Map<PaymentResponsePb>(paymentResponse);
        }

        public override async Task<SearchCustomerResponsePb> SearchCustomer(SearchCustomerRequestPb request, ServerCallContext context)
        {
            var searchCustomerRequest = mapper.Map<CustomerSearchRequest>(request);
            var searchCustomerResponse = await swiftPay.SearchCustomer(searchCustomerRequest);
            return mapper.Map<SearchCustomerResponsePb>(searchCustomerResponse);
        }

        public override async Task<ReverseResponsePb> Reverse(ReverseRequestPb request, ServerCallContext context)
        {
            var reverseRequest = mapper.Map<ReverseRequest>(request);
            var reverseResponse = await swiftPayReverse.Reverse(reverseRequest);
            return mapper.Map<ReverseResponsePb>(reverseResponse);
        }

        public override async Task<PaymentDocumentResponsePb> GetPaymentDocument(PaymentDocumentRequestPb request, ServerCallContext context)
        {
            var paymentDocumentResponse = await swiftPayPaymentDocument.GetPaymentDocument(new PaymentDocumentRequest { TransactionId = request.TransactionId });
            return mapper.Map<PaymentDocumentResponsePb>(paymentDocumentResponse);
        }
    }
}
