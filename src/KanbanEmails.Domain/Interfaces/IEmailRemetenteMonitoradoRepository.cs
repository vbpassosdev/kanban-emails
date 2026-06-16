using KanbanEmails.Domain.Entities;

namespace KanbanEmails.Domain.Interfaces;

public interface IEmailRemetenteMonitoradoRepository
{
    Task<EmailRemetenteMonitorado?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<EmailRemetenteMonitorado>> ListarAtivosAsync(CancellationToken ct = default);
    Task<IEnumerable<EmailRemetenteMonitorado>> ListarTodosAsync(CancellationToken ct = default);
    Task<bool> RemetenteMonitoradoAsync(string email, CancellationToken ct = default);
    Task AdicionarAsync(EmailRemetenteMonitorado remetente, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
    Task RemoverAsync(int id, CancellationToken ct = default);
}
