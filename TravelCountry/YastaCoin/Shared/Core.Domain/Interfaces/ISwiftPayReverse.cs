using Core.Domain.Models;
using Core.Domain.Models.ServicePayment.Requests;
using Core.Domain.Models.ServicePayment.Responses;

namespace Core.Domain.Interfaces
{
    public interface ISwiftPayReverse
    {
        Task<BaseResponse<ReverseResponse>> Reverse(ReverseRequest request, CancellationToken cancellationToken = default);
    }
}
