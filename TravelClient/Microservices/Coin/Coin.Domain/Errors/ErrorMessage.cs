namespace Coin.Domain.Errors;

public struct ErrorMessage
{
    public const string COIN_NOT_FOUND = "La moneda solicitada no existe.";
    public const string COIN_DUPLICATE = "Ya existe una moneda con ese código.";
    public const string INSERT_FAILED  = "No se pudo registrar la moneda. Intente nuevamente.";
    public const string UPDATE_FAILED  = "No se pudo actualizar la moneda. Intente nuevamente.";
}
