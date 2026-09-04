using Core.ShareKernel.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Infrastructure.Audit
{
    /// <summary>
    /// Registro de la auditoria. Un microservicio la enciende con una linea:
    ///
    ///     builder.Services.AddAuditDependence();
    ///     builder.Services.AddGrpc(x => x.Interceptors.Add&lt;AuditInterceptor&gt;());
    ///
    /// Requiere que ya esten registrados AddDatabaseDependence y
    /// AddHttpContextAccessor, que es lo que usan ICommand e ICurrentUser.
    /// </summary>
    public static class AuditDependence
    {
        public static IServiceCollection AddAuditDependence(this IServiceCollection services)
        {
            // TryAdd: si el microservicio ya registro su propia implementacion de
            // ICurrentUser -AccountsAndMovements lo hace, para el atajo de
            // DevAuth- se respeta la suya.
            services.AddScoped<AuditInterceptor>();
            services.AddScoped<IAuditContext, AuditContext>();

            if (services.All(x => x.ServiceType != typeof(ICurrentUser)))
                services.AddScoped<ICurrentUser, CurrentUser>();

            return services;
        }
    }
}
