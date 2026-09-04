using AutoMapper;
using Core.Domain.Grpc;
using Core.Domain.Models.ServicePayment.Models;
using Core.Domain.Models.ServicePayment.Requests;
using Core.Domain.Models.ServicePayment.Responses;

namespace Core.Domain.Models.ServicePayment.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<SearchCustomerRequestPb, CustomerSearchRequest>();
            CreateMap<AdditionalInformationPb, AdditionalInformation>();
            CreateMap<CriteriaPb, Criteria>();
            CreateMap<BaseResponse<CustomerSearchResponse>, SearchCustomerResponsePb>().ForMember(x => x.Exception, y => y.MapFrom(z => z.ExceptionDetail));
            CreateMap<ErrorModel, ErrorPb>().ReverseMap();
            CreateMap<Exception, ExceptionPb>().ReverseMap();
            CreateMap<CustomerSearchResponse, CustomerPb>();
            CreateMap<BillingCriteriaPb, BillingCriteria>().ReverseMap();
            CreateMap<CriteriaPb, CriteriaItem>().ReverseMap();
            CreateMap<SearchCriteria, SearchCriteriaPb>().ReverseMap();
            CreateMap<Item, CriteriaPb>();

            CreateMap<Responses.PaymentOption, PaymentOptionPb>();
            CreateMap<Responses.PendingPaymentItem, PendingPaymentItemPb>();
            CreateMap<PaymentRequestPb, PaymentRequest>();
            CreateMap<BillingInformationPb, BillingInformation>();
            CreateMap<PaymentOptionPaymentPb, Requests.PaymentOption>();
            CreateMap<PaymentItemPb, Requests.PendingPaymentItem>();
            CreateMap<BaseResponse<List<PaymentResponse>>, PaymentResponsePb>().ForMember(x => x.Exception, y => y.MapFrom(z => z.ExceptionDetail));
            CreateMap<PaymentResponse, ExecutedPaymentPb>();

            CreateMap<ReverseRequestPb, ReverseRequest>();
            CreateMap<TransactionPb, SwiftpayTransaction>();
            CreateMap<BaseResponse<ReverseResponse>, ReverseResponsePb>().ForMember(x => x.Exception, y => y.MapFrom(z => z.ExceptionDetail));
            CreateMap<ReverseResponse, ExecutedReversePb>();

            CreateMap<BaseResponse<PaymentDocumentResponse>, PaymentDocumentResponsePb>().ForMember(x => x.Exception, y => y.MapFrom(z => z.ExceptionDetail));
            CreateMap<PaymentDocumentResponse, PaymentDocumentPb>();
        }
    }
}
