namespace KanbanEmails.Application.Interfaces;

public interface IStorageService
{
    Task<string> SalvarAnexoAsync(int emailId, string nomeArquivo, byte[] conteudo, CancellationToken ct = default);
}
