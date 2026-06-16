namespace KanbanEmails.Domain.Entities;

public class EmailRemetenteMonitorado
{
    public int Id { get; private set; }
    public string? Nome { get; private set; }
    public string EmailOuDominio { get; private set; } = null!;
    public bool Ativo { get; private set; }
    public DateTime DataCriacao { get; private set; }

    protected EmailRemetenteMonitorado() { }

    public static EmailRemetenteMonitorado Criar(string emailOuDominio, string? nome = null)
    {
        return new EmailRemetenteMonitorado
        {
            EmailOuDominio = emailOuDominio.ToLowerInvariant(),
            Nome = nome,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };
    }

    public void Atualizar(string emailOuDominio, string? nome, bool ativo)
    {
        EmailOuDominio = emailOuDominio.ToLowerInvariant();
        Nome = nome;
        Ativo = ativo;
    }
}
