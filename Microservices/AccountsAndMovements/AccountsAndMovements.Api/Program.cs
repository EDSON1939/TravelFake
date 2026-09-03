using AccountsAndMovements.Api.Mapper;
using AccountsAndMovements.Api.Services;
using AccountsAndMovements.Application.Extensions;
using Core.AuditTrail;
using Core.AuditTrail.Grpc;
using Core.ShareKernel.Grpc;

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
    x.EnableDetailedErrors = true;
});

builder.Services.AddValidators();
builder.Services.AddGrpcValidation();
builder.Services.AddApplicationDependence(builder.Configuration);
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// ── Descomentar si el servicio es llamado desde browser o app móvil ──────────
// builder.Services.AddCors(o =>
//     o.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ── JWT: el interceptor exige que llegue en cada RPC, pero NO lo verifica ───
// Firma, vigencia y permisos los valida el Gateway, único punto de entrada.

var app = builder.Build();

// app.UseGrpcWeb();                  // activar con gRPC-Web
// app.UseCors("AllowAll");           // activar con CORS
// app.UseAuthentication();           // activar con JWT
// app.UseAuthorization();            // activar con JWT

app.MapGrpcService<AccountsAndMovementsService>();
app.UseMonitoringPlatform();
app.MapGet("/", () => $"AccountsAndMovements gRPC Service — {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

app.Run();
