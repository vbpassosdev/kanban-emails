namespace KanbanEmails.Domain.Entities;

public class Usuario
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string SenhaHash { get; private set; } = null!;
    public bool Ativo { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }

    public ConfiguracaoEmail? ConfiguracaoEmail { get; private set; }

    protected Usuario() { }

    public static Usuario Criar(string nome, string email, string senhaHash)
    {
        return new Usuario
        {
            Nome = nome,
            Email = email.ToLowerInvariant(),
            SenhaHash = senhaHash,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };
    }

    public void Atualizar(string nome, string email)
    {
        Nome = nome;
        Email = email.ToLowerInvariant();
        DataAtualizacao = DateTime.UtcNow;
    }

    public void AlterarSenha(string novaSenhaHash)
    {
        SenhaHash = novaSenhaHash;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void DefinirAtivo(bool ativo)
    {
        Ativo = ativo;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void DefinirConfiguracaoEmail(ConfiguracaoEmail config)
    {
        ConfiguracaoEmail = config;
        DataAtualizacao = DateTime.UtcNow;
    }
}
