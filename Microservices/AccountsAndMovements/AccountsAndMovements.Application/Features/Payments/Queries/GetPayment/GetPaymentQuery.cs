using AccountsAndMovements.Application.Common;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Payments.Queries.GetPayment;

/// <summary>
/// Consulta de una operacion por su codigo o por la clave de idempotencia con la
/// que se envio. Lo segundo le sirve al llamador que perdio la respuesta y solo
/// conserva su propia clave.
/// </summary>
public record GetPaymentQuery(string TransactionCode, string IdempotencyKey)
    : IRequest<BaseResponse<PaymentResponse>>;
