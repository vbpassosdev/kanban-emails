namespace KanbanEmails.Application.DTOs;

public record EmailImapData(
    string MessageId,
    string Remetente,
    string? Assunto,
    string? CorpoTexto,
    string? CorpoHtml,
    DateTime DataRecebimento,
    IReadOnlyList<EmailImapAnexoData> Anexos);

public record EmailImapAnexoData(
    string NomeArquivo,
    string? MimeType,
    byte[] Conteudo);
