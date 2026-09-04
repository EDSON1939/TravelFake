using Coin.Domain.Entities;
using FluentValidation;
using System.Globalization;

namespace Coin.Application.Common;

/// <summary>
/// Monetary values travel as text in the gRPC contract (protobuf has no decimal
/// type and double would lose precision on money). Invariant-culture parsing and
/// the reusable validation rule live here.
/// </summary>
public static class MoneyRules
{
    /// <summary>Ceiling aligned with DECIMAL(18,2): 16 integer digits + 2 decimals.</summary>
    public const decimal MaxValue = 9_999_999_999_999_999.99m;

    public static bool TryParse(string? value, out decimal result)
        => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);

    /// <summary>Internal parsing, for use once the validator has approved the value.</summary>
    public static decimal Parse(string value) => decimal.Parse(value, NumberStyles.Number, CultureInfo.InvariantCulture);

    public static string Format(decimal value) => value.ToString("F2", CultureInfo.InvariantCulture);

    /// <summary>
    /// Requires a positive decimal, using a dot as the decimal separator and at
    /// most 2 decimals. Always pair it with <c>.Cascade(CascadeMode.Stop)</c> so an
    /// empty value does not also trigger the format messages.
    /// </summary>
    public static IRuleBuilderOptions<T, string> MustBeMoney<T>(
        this IRuleBuilder<T, string> rule, string fieldLabel)
        => rule
            .NotEmpty().WithMessage($"{fieldLabel} es requerido.")
            .Must(v => TryParse(v, out _))
                .WithMessage($"{fieldLabel} debe ser un número decimal válido; use punto como separador (ej. 6.86).")
            .Must(v => TryParse(v, out var d) && d > 0)
                .WithMessage($"{fieldLabel} debe ser mayor a cero.")
            .Must(v => TryParse(v, out var d) && d <= MaxValue)
                .WithMessage($"{fieldLabel} supera el máximo permitido.")
            .Must(v => TryParse(v, out var d) && decimal.Round(d, Money.Decimals) == d)
                .WithMessage($"{fieldLabel} no puede tener más de {Money.Decimals} decimales.");
}
