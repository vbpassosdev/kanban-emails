namespace KanbanEmails.Application.DTOs;

public record EmailKanbanDto(
    int Id,
    string MessageId,
    string Remetente,
    string? Assunto,
    string? Resumo,
    string? Categoria,
    string Status,
    DateTime DataRecebimento,
    DateTime? DataAtualizacao,
    int QuantidadeAnexos,
    IEnumerable<EmailAnexoResumoDto> Anexos);

public record EmailKanbanDetalheDto(
    int Id,
    string MessageId,
    string Remetente,
    string? Assunto,
    string? CorpoTexto,
    string? CorpoHtml,
    string? Resumo,
    string? Categoria,
    string Status,
    DateTime DataRecebimento,
    DateTime? DataAtualizacao,
    IEnumerable<EmailAnexoDto> Anexos,
    IEnumerable<EmailKanbanHistoricoDto> Historico);

public record EmailAnexoResumoDto(
    int Id,
    string NomeArquivo,
    string? MimeType,
    long TamanhoBytes);

public record EmailAnexoDto(
    int Id,
    int EmailKanbanId,
    string NomeArquivo,
    string? MimeType,
    long TamanhoBytes,
    DateTime DataCriacao);

public record EmailKanbanHistoricoDto(
    int Id,
    string? StatusAnterior,
    string StatusNovo,
    string? Observacao,
    string? Usuario,
    DateTime DataMovimento);
