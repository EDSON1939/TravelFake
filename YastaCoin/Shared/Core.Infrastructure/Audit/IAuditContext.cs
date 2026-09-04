namespace Core.Infrastructure.Audit
{
    /// <summary>
    /// Datos de la llamada en curso que necesitan los stored procedures para
    /// dejar auditoria: el identificador de la traza y quien la origino.
    ///
    /// Existe porque la auditoria de datos se escribe DENTRO del procedimiento,
    /// y el procedimiento no tiene forma de averiguar por si solo quien esta del
    /// otro lado. Los repositorios lo inyectan y lo pasan como parametro.
    /// </summary>
    public interface IAuditContext
    {
        /// <summary>
        /// TraceId de la actividad actual. Es lo que despues une la fila de
        /// aud.AUDITORIA con la de aud.BITACORA, y las dos con los logs.
        /// </summary>
        string? TraceId { get; }

        /// <summary>Usuario del JWT. Null cuando el token no identifica a nadie.</summary>
        long? UserId { get; }
    }
}
