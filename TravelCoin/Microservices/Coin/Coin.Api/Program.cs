using Coin.Api.Mapper;
using Coin.Api.Services;
using Coin.Application.Extensions;
using Core.AuditTrail;
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
    x.EnableDetailedErrors = true;
});

builder.Services.AddValidators();
builder.Services.AddGrpcValidation();
builder.Services.AddApplicationDependence(builder.Configuration);
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// ── Uncomment if the service is called from a browser or mobile app ──────────
// builder.Services.AddCors(o =>
//     o.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ── Uncomment if this service issues or validates JWT tokens ─────────────────
// builder.Services.AddJwtDependence(builder.Configuration);

var app = builder.Build();

// app.UseGrpcWeb();                  // enable with gRPC-Web
// app.UseCors("AllowAll");           // enable with CORS
// app.UseAuthentication();           // enable with JWT
// app.UseAuthorization();            // enable with JWT

app.MapGrpcService<CoinService>();
app.UseMonitoringPlatform();
app.MapGet("/", () => $"Coin gRPC Service — {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

app.Run();
