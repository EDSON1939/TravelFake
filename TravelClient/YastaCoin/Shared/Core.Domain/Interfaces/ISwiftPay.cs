using Core.Domain.Models;
using Core.Domain.Models.ServicePayment.Requests;
using Core.Domain.Models.ServicePayment.Responses;

namespace Core.Domain.Interfaces
{
    public interface ISwiftPay
    {
        Task<BaseResponse<CustomerSearchResponse>> SearchCustomer(CustomerSearchRequest request, CancellationToken cancellationToken = default);

        Task<BaseResponse<List<PaymentResponse>>> ExecutePayment(PaymentRequest request, CancellationToken cancellationToken = default);
    }
}
