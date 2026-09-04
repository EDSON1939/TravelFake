using Core.AuditTrail;
using Core.ShareKernel.Grpc;
using Qr.Api.Background;
using Qr.Api.Mapper;
using Qr.Api.Services;
using Qr.Application.Extensions;

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
    x.EnableDetailedErrors = true;
});

builder.Services.AddValidators();
builder.Services.AddGrpcValidation();
builder.Services.AddApplicationDependence(builder.Configuration);
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// Servicio en background que expira los QR vencidos (evaluación cada hora)
builder.Services.AddHostedService<QrExpirationService>();

// ── Descomentar si el servicio es llamado desde browser o app móvil ──────────
// builder.Services.AddCors(o =>
//     o.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ── Descomentar si este servicio emite o valida tokens JWT ────────────────────
// builder.Services.AddJwtDependence(builder.Configuration);

var app = builder.Build();

// app.UseGrpcWeb();                  // activar con gRPC-Web
// app.UseCors("AllowAll");           // activar con CORS
// app.UseAuthentication();           // activar con JWT
// app.UseAuthorization();            // activar con JWT

app.MapGrpcService<QrService>();
app.UseMonitoringPlatform();
app.MapGet("/", () => $"Qr gRPC Service — {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

app.Run();