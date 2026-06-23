using KanbanEmails.Domain.Entities;
using KanbanEmails.Domain.Enums;

namespace KanbanEmails.Domain.Interfaces;

public interface IEmailKanbanRepository
{
    Task<EmailKanban?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<EmailKanban?> ObterPorMessageIdAsync(string messageId, CancellationToken ct = default);
    Task<bool> ExisteMessageIdAsync(string messageId, CancellationToken ct = default);
    Task<IEnumerable<EmailKanban>> ListarAsync(FiltroEmailKanban filtro, CancellationToken ct = default);
    Task AdicionarAsync(EmailKanban email, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
    Task<bool> ExisteErroCorrigidoAsync(string classeExcecao, CancellationToken ct = default);
    Task<HashSet<string>> ObterClassesExcecaoConcluidasAsync(CancellationToken ct = default);
}

public record FiltroEmailKanban(
    string? Remetente = null,
    string? Assunto = null,
    StatusKanban? Status = null,
    DateTime? DataInicio = null,
    DateTime? DataFim = null,
    int Pagina = 1,
    int TamanhoPagina = 50);
