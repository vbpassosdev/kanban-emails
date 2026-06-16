using KanbanEmails.Application.DTOs;
using KanbanEmails.Application.Interfaces;
using KanbanEmails.Domain.Enums;
using KanbanEmails.Domain.Interfaces;

namespace KanbanEmails.Application.Services;

public class EmailKanbanService(
    IEmailKanbanRepository repository,
    IHtmlSanitizerService htmlSanitizer) : IEmailKanbanService
{
    public async Task<IEnumerable<EmailKanbanDto>> ListarAsync(
        string? remetente, string? assunto, string? status,
        DateTime? dataInicio, DateTime? dataFim,
        int pagina, int tamanhoPagina,
        CancellationToken ct = default)
    {
        StatusKanban? statusEnum = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<StatusKanban>(status, true, out var parsed))
            statusEnum = parsed;

        var filtro = new FiltroEmailKanban(remetente, assunto, statusEnum, dataInicio, dataFim, pagina, tamanhoPagina);
        var emails = await repository.ListarAsync(filtro, ct);

        return emails.Select(e => new EmailKanbanDto(
            e.Id,
            e.MessageId,
            e.Remetente,
            e.Assunto,
            e.Resumo,
            e.Categoria,
            e.Status.ToString(),
            e.DataRecebimento,
            e.DataAtualizacao,
            e.Anexos.Count,
            e.Anexos.Select(a => new EmailAnexoResumoDto(a.Id, a.NomeArquivo, a.MimeType, a.TamanhoBytes))
        ));
    }

    public async Task<EmailKanbanDetalheDto?> ObterDetalheAsync(int id, CancellationToken ct = default)
    {
        var email = await repository.ObterPorIdAsync(id, ct);
        if (email is null) return null;

        var corpoHtmlSanitizado = email.CorpoHtml is not null
            ? htmlSanitizer.Sanitizar(email.CorpoHtml)
            : null;

        return new EmailKanbanDetalheDto(
            email.Id,
            email.MessageId,
            email.Remetente,
            email.Assunto,
            email.CorpoTexto,
            corpoHtmlSanitizado,
            email.Resumo,
            email.Categoria,
            email.Status.ToString(),
            email.DataRecebimento,
            email.DataAtualizacao,
            email.Anexos.Select(a => new EmailAnexoDto(a.Id, a.EmailKanbanId, a.NomeArquivo, a.MimeType, a.TamanhoBytes, a.DataCriacao)),
            email.Historico
                .OrderByDescending(h => h.DataMovimento)
                .Select(h => new EmailKanbanHistoricoDto(h.Id, h.StatusAnterior?.ToString(), h.StatusNovo.ToString(), h.Observacao, h.Usuario, h.DataMovimento))
        );
    }

    public async Task AlterarStatusAsync(int id, AlterarStatusDto dto, CancellationToken ct = default)
    {
        var email = await repository.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Email {id} não encontrado.");

        if (!Enum.TryParse<StatusKanban>(dto.Status, true, out var novoStatus))
            throw new ArgumentException($"Status '{dto.Status}' inválido.");

        email.AlterarStatus(novoStatus, dto.Observacao, dto.Usuario);
        await repository.SalvarAsync(ct);
    }
}
