namespace KanbanEmails.Domain.Entities;

public class ConfiguracaoEmail
{
    public int Id { get; private set; }
    public int UsuarioId { get; private set; }
    public string Host { get; private set; } = null!;
    public int Porta { get; private set; }
    public bool UsarSsl { get; private set; }
    public string EmailUsuario { get; private set; } = null!;
    public string SenhaCriptografada { get; private set; } = null!;
    public string Pasta { get; private set; } = "INBOX";
    public int IntervaloMinutos { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }

    public Usuario Usuario { get; private set; } = null!;

    protected ConfiguracaoEmail() { }

    public static ConfiguracaoEmail Criar(
        int usuarioId,
        string host,
        int porta,
        bool usarSsl,
        string emailUsuario,
        string senhaCriptografada,
        string pasta = "INBOX",
        int intervaloMinutos = 5)
    {
        return new ConfiguracaoEmail
        {
            UsuarioId = usuarioId,
            Host = host,
            Porta = porta,
            UsarSsl = usarSsl,
            EmailUsuario = emailUsuario,
            SenhaCriptografada = senhaCriptografada,
            Pasta = pasta,
            IntervaloMinutos = intervaloMinutos,
            DataCriacao = DateTime.UtcNow
        };
    }

    public void Atualizar(
        string host,
        int porta,
        bool usarSsl,
        string emailUsuario,
        string senhaCriptografada,
        string pasta,
        int intervaloMinutos)
    {
        Host = host;
        Porta = porta;
        UsarSsl = usarSsl;
        EmailUsuario = emailUsuario;
        SenhaCriptografada = senhaCriptografada;
        Pasta = pasta;
        IntervaloMinutos = intervaloMinutos;
        DataAtualizacao = DateTime.UtcNow;
    }
}
