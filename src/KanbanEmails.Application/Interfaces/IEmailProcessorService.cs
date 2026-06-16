namespace KanbanEmails.Application.Interfaces;

public interface IEmailProcessorService
{
    Task<int> ProcessarEmailsAsync(CancellationToken ct = default);
}
