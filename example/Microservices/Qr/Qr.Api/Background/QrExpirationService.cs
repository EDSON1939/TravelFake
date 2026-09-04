using Core.Domain.Models;
using MediatR;
using Qr.Application.Features.Qrs.Commands.ExpireQrs;

namespace Qr.Api.Background;

// Servicio en background que evalua cada hora los códigos QR ACTIVOS y cambia
// a EXPIRED los que ya vencieron su fecha de expiración.
public class QrExpirationService(
    IServiceProvider services,
    IConfiguration configuration,
    ILogger<QrExpirationService> logger) : BackgroundService
{
    private readonly TimeSpan _interval =
        TimeSpan.FromHours(configuration.GetValue<int>("QrBackground:IntervalHours", 1));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Servicio de expiración de QR iniciado. Intervalo: {Interval}", _interval);

        using var timer = new PeriodicTimer(_interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = services.CreateScope();
                var sender  = scope.ServiceProvider.GetRequiredService<ISender>();
                var result  = await sender.Send(new ExpireQrsCommand(), stoppingToken);

                if (result.IsSuccess())
                    logger.LogInformation("Códigos QR expirados: {Count}", result.Data);
                else
                    logger.LogWarning("No se pudieron expirar los QR. Código: {StatusCode} Mensaje: {Message}",
                        result.StatusCode, result.Message);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error al ejecutar la expiración de códigos QR.");
            }
        }
    }
}