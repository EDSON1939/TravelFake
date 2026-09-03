using AccountsAndMovements.Application.Common;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Payments.Commands.ExecuteQrPayment;

/// <summary>
/// Pago de un QR boliviano por parte de un cliente extranjero.
/// </summary>
/// <param name="Amount">Monto en la moneda del cliente (ej: 20).</param>
/// <param name="CurrencyCode">Moneda del cliente (ej: USD). El comercio siempre cobra en BOB.</param>
/// <param name="IdempotencyKey">
/// Clave unica de la operacion (ej: TX-2026-000001). Si llega dos veces, el
/// cliente se cobra una sola vez y se devuelve el resultado de la original.
/// </param>
public record ExecuteQrPaymentCommand(
    long    ClientId,
    string  QrCode,
    decimal Amount,
    string  CurrencyCode,
    string  IdempotencyKey,
    string  Description) : IRequest<BaseResponse<PaymentResponse>>;
