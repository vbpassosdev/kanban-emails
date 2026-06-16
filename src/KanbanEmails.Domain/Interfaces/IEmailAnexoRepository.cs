using KanbanEmails.Domain.Entities;

namespace KanbanEmails.Domain.Interfaces;

public interface IEmailAnexoRepository
{
    Task<EmailAnexo?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<EmailAnexo>> ListarPorEmailAsync(int emailKanbanId, CancellationToken ct = default);
    Task AdicionarAsync(EmailAnexo anexo, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
}
