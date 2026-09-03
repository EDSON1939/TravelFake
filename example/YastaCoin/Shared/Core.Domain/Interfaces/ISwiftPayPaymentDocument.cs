using Core.Domain.Models;
using Core.Domain.Models.ServicePayment.Requests;
using Core.Domain.Models.ServicePayment.Responses;

namespace Core.Domain.Interfaces
{
    public interface ISwiftPayPaymentDocument
    {
        Task<BaseResponse<PaymentDocumentResponse>> GetPaymentDocument(PaymentDocumentRequest request, CancellationToken cancellationToken = default);
    }
}
