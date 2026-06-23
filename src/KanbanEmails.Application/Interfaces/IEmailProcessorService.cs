namespace KanbanEmails.Application.Interfaces;

public interface IEmailProcessorService
{
    Task<int> ProcessarEmailsAsync(CancellationToken ct = default);
    Task<int> ReprocessarBugreportsAsync(CancellationToken ct = default);
    Task<int> MarcarErrosCorrigidosAsync(CancellationToken ct = default);
}
