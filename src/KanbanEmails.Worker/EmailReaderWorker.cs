using KanbanEmails.Application.Interfaces;

namespace KanbanEmails.Worker;

public class EmailReaderWorker(
    ILogger<EmailReaderWorker> logger,
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervaloMinutos = configuration.GetValue<int>("EmailImap:IntervaloMinutos", 5);
        logger.LogInformation("EmailReaderWorker iniciado. Intervalo: {Intervalo} minutos.", intervaloMinutos);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessarAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(intervaloMinutos), stoppingToken);
        }
    }

    private async Task ProcessarAsync(CancellationToken ct)
    {
        try
        {
            // IEmailProcessorService is Scoped, Worker is Singleton — use a scope
            using var scope = scopeFactory.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IEmailProcessorService>();
            var processados = await processor.ProcessarEmailsAsync(ct);
            logger.LogInformation("Ciclo concluído. {Processados} e-mail(s) novo(s) processado(s).", processados);
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro durante ciclo de leitura de e-mails.");
        }
    }
}
