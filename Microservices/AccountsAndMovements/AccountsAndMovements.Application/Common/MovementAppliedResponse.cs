namespace AccountsAndMovements.Application.Common;

/// <summary>
/// Resultado de aplicar un movimiento suelto. Devuelve el saldo resultante para
/// que el llamador no tenga que hacer una lectura extra.
/// </summary>
public record MovementAppliedResponse(
    long    MovementId,
    string  AccountNumber,
    decimal Balance);
