namespace Qr.Domain.Entities;

public class QrEntity
{
    public long      QrId              { get; set; }
    public string    Codigo            { get; set; } = string.Empty;
    public long      ComercioId        { get; set; }
    public decimal   Monto             { get; set; }
    public string    Tipo              { get; set; } = QrTipo.UNICO;
    public DateTime  FechaExpiracion   { get; set; }
    public string    Estado            { get; set; } = QrEstado.ACTIVE;
    public bool      Activo            { get; set; } = true;
    public DateTime  FechaCreacion     { get; set; }
    public DateTime? FechaActualizacion { get; set; }
}

public static class QrEstado
{
    public const string ACTIVE  = nameof(ACTIVE);
    public const string USED    = nameof(USED);
    public const string EXPIRED = nameof(EXPIRED);
}

public static class QrTipo
{
    public const string UNICO    = nameof(UNICO);
    public const string MULTIPLE = nameof(MULTIPLE);
}