namespace Coin.Domain.Errors;

// End-user facing text stays in Spanish, consistent with Core.Domain.ErrorMessage
// and with the Country and Client microservices.
public struct ErrorMessage
{
    public const string COIN_NOT_FOUND        = "La moneda solicitada no existe.";
    public const string COIN_INACTIVE         = "La moneda está inactiva y no puede usarse para convertir montos.";
    public const string INVALID_EXCHANGE_RATE = "La moneda no tiene un tipo de cambio de compra válido.";
    public const string INSERT_FAILED         = "No se pudo registrar la moneda. Intente nuevamente.";
    public const string UPDATE_FAILED         = "No se pudo actualizar la moneda. Intente nuevamente.";
    public const string DELETE_FAILED         = "No se pudo eliminar la moneda. Intente nuevamente.";
}
