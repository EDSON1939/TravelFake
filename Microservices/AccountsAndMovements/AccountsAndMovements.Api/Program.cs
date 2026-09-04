using AccountsAndMovements.Api.Mapper;
using AccountsAndMovements.Api.Security;
using AccountsAndMovements.Api.Services;
using AccountsAndMovements.Application.Extensions;
using Core.AuditTrail;
using Core.AuditTrail.Grpc;
using Core.Infrastructure.Audit;
using Core.ShareKernel.Grpc;
using Core.ShareKernel.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var logger = builder.AddMonitoringPlatform();

builder.Services.AddHttpContextAccessor();
builder.Services.AddGrpc(x =>
{
    x.EnableRequestValidation();
    x.AddLogInterceptor(logger);
    x.Interceptors.Add<JwtAuthorizationInterceptor>();
    // Deja una fila en aud.BITACORA por cada RPC. Va despues del de JWT para
    // que la identidad ya este resuelta cuando se escribe la fila.
    x.Interceptors.Add<AuditInterceptor>();
    x.EnableDetailedErrors = true;
});

// ── Identidad del llamador ───────────────────────────────────────────────────
// El id del cliente sale del claim client_id del JWT, no del request: si viajara
// en el body, cualquiera podria pagar desde la cuenta de otro.
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddScoped<ICurrentUser>(sp =>
{
    var currentUser = sp.GetRequiredService<CurrentUser>();

    // Red de pruebas: mientras no exista el microservicio de Clientes ni un
    // login real, DevAuth:ClientId completa el claim que el token no trae.
    // Solo en Development, y solo cuando el token no identifica a nadie.
    var environment   = sp.GetRequiredService<IHostEnvironment>();
    var configuration = sp.GetRequiredService<IConfiguration>();

    if (!EsEntornoLocal(environment) || !configuration.GetValue<bool>("DevAuth:Enabled"))
        return currentUser;

    var fallbackClientId = configuration.GetValue<long?>("DevAuth:ClientId");

    return fallbackClientId.HasValue
        ? new DevelopmentCurrentUser(currentUser, fallbackClientId.Value)
        : currentUser;
});

builder.Services.AddAuditDependence();
builder.Services.AddValidators();
builder.Services.AddGrpcValidation();
builder.Services.AddApplicationDependence(builder.Configuration);
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// ── Descomentar si el servicio es llamado desde browser o app móvil ──────────
// builder.Services.AddCors(o =>
//     o.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ── JWT: el interceptor exige que llegue en cada RPC, pero NO lo verifica ───
// Firma, vigencia y permisos los valida el Gateway, único punto de entrada.
// La identidad, en cambio, sí se resuelve acá: ver ICurrentUser.

var app = builder.Build();

if (EsEntornoLocal(app.Environment) && app.Configuration.GetValue<bool>("DevAuth:Enabled"))
    app.Logger.LogWarning(
        "DevAuth ACTIVO: los tokens sin claim client_id se resuelven como cliente {ClientId}. Solo para pruebas locales.",
        app.Configuration.GetValue<long?>("DevAuth:ClientId"));

// app.UseGrpcWeb();                  // activar con gRPC-Web
// app.UseCors("AllowAll");           // activar con CORS
// app.UseAuthentication();           // activar con JWT
// app.UseAuthorization();            // activar con JWT

app.MapGrpcService<AccountsAndMovementsService>();
app.UseMonitoringPlatform();
app.MapGet("/", () => $"AccountsAndMovements gRPC Service — {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

app.Run();

/// <summary>
/// Entornos donde se permite el atajo de DevAuth: la maquina del desarrollador y
/// el compose local. Production no entra nunca, aunque el appsettings traiga la
/// seccion puesta por error.
/// </summary>
static bool EsEntornoLocal(IHostEnvironment environment)
    => environment.IsDevelopment() || environment.IsEnvironment("Docker");
