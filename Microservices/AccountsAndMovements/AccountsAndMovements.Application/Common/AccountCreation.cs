using AccountsAndMovements.Domain.Entities;
using Core.Domain.Models;

namespace AccountsAndMovements.Application.Common;

/// <summary>
/// Traduce el codigo que devuelve commerce.INSERT_CUENTA. Vive aparte porque
/// el alta de cliente y la de comercio tienen reglas distintas pero comparten
/// desenlace: duplicar la traduccion garantizaria que alguna quede distinta.
/// </summary>
internal static class AccountCreation
{
    public static BaseResponse<long> ToResponse(long id) => id switch
    {
        AccountResult.DUPLICATE => BaseResponse<long>.Error(
            Domain.Errors.ErrorCode.ACCOUNT_DUPLICATE,
            Domain.Errors.ErrorMessage.ACCOUNT_DUPLICATE),

        <= 0 => BaseResponse<long>.Error(
            Domain.Errors.ErrorCode.INSERT_FAILED,
            Domain.Errors.ErrorMessage.INSERT_FAILED),

        _ => BaseResponse<long>.Success(id)
    };
}
