namespace KanbanEmails.Domain.Entities;

public class EmailAnexo
{
    public int Id { get; private set; }
    public int EmailKanbanId { get; private set; }
    public string NomeArquivo { get; private set; } = null!;
    public string? MimeType { get; private set; }
    public long TamanhoBytes { get; private set; }
    public string CaminhoArquivo { get; private set; } = null!;
    public DateTime DataCriacao { get; private set; }

    public EmailKanban? Email { get; private set; }

    protected EmailAnexo() { }

    public static EmailAnexo Criar(
        int emailKanbanId,
        string nomeArquivo,
        string? mimeType,
        long tamanhoBytes,
        string caminhoArquivo)
    {
        return new EmailAnexo
        {
            EmailKanbanId = emailKanbanId,
            NomeArquivo = nomeArquivo,
            MimeType = mimeType,
            TamanhoBytes = tamanhoBytes,
            CaminhoArquivo = caminhoArquivo,
            DataCriacao = DateTime.UtcNow
        };
    }
}
