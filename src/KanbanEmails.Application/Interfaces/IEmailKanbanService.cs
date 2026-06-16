using KanbanEmails.Application.DTOs;
using KanbanEmails.Domain.Enums;

namespace KanbanEmails.Application.Interfaces;

public interface IEmailKanbanService
{
    Task<IEnumerable<EmailKanbanDto>> ListarAsync(
        string? remetente, string? assunto, string? status,
        DateTime? dataInicio, DateTime? dataFim,
        int pagina, int tamanhoPagina,
        CancellationToken ct = default);

    Task<EmailKanbanDetalheDto?> ObterDetalheAsync(int id, CancellationToken ct = default);
    Task AlterarStatusAsync(int id, AlterarStatusDto dto, CancellationToken ct = default);
}
