using Auth.Api.Mapper;
using Auth.Api.Services;
using Auth.Application.Extensions;
using Core.AuditTrail;
using Core.Infrastructure.Audit;
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
    // Deja una fila en aud.BITACORA por cada RPC, incluidos los intentos de
    // login fallidos: sin ellos la bitacora no sirve para investigar un abuso.
    x.Interceptors.Add<AuditInterceptor>();
    x.EnableDetailedErrors = true;
});

builder.Services.AddAuditDependence();
builder.Services.AddValidators();
builder.Services.AddGrpcValidation();
builder.Services.AddApplicationDependence(builder.Configuration);
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// ── Descomentar si el servicio es llamado desde browser o app móvil ──────────
// builder.Services.AddCors(o =>
//     o.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ── Este servicio EMITE los tokens; quien los valida es el Gateway ──────────

var app = builder.Build();

// app.UseGrpcWeb();                  // activar con gRPC-Web
// app.UseCors("AllowAll");           // activar con CORS
// app.UseAuthentication();           // activar con JWT
// app.UseAuthorization();            // activar con JWT

app.MapGrpcService<AuthService>();
app.UseMonitoringPlatform();
app.MapGet("/", () => $"Auth gRPC Service — {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

app.Run();
