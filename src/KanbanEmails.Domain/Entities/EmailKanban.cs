using KanbanEmails.Domain.Enums;

namespace KanbanEmails.Domain.Entities;

public class EmailKanban
{
    public int Id { get; private set; }
    public string MessageId { get; private set; } = null!;
    public string Remetente { get; private set; } = null!;
    public string? Assunto { get; private set; }
    public string? CorpoTexto { get; private set; }
    public string? CorpoHtml { get; private set; }
    public string? Resumo { get; private set; }
    public string? Categoria { get; private set; }
    public StatusKanban Status { get; private set; }
    public DateTime DataRecebimento { get; private set; }
    public DateTime DataProcessamento { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }

    public IReadOnlyCollection<EmailAnexo> Anexos => _anexos.AsReadOnly();
    public IReadOnlyCollection<EmailKanbanHistorico> Historico => _historico.AsReadOnly();

    private readonly List<EmailAnexo> _anexos = [];
    private readonly List<EmailKanbanHistorico> _historico = [];

    protected EmailKanban() { }

    public static EmailKanban Criar(
        string messageId,
        string remetente,
        string? assunto,
        string? corpoTexto,
        string? corpoHtml,
        DateTime dataRecebimento)
    {
        return new EmailKanban
        {
            MessageId = messageId,
            Remetente = remetente,
            Assunto = assunto,
            CorpoTexto = corpoTexto,
            CorpoHtml = corpoHtml,
            Status = StatusKanban.Novo,
            DataRecebimento = dataRecebimento,
            DataProcessamento = DateTime.UtcNow
        };
    }

    public void AlterarStatus(StatusKanban novoStatus, string? observacao = null, string? usuario = null)
    {
        var historico = EmailKanbanHistorico.Criar(Id, Status, novoStatus, observacao, usuario);
        _historico.Add(historico);
        Status = novoStatus;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void DefinirResumo(string resumo, string? categoria = null)
    {
        Resumo = resumo;
        Categoria = categoria;
    }

    public void AdicionarAnexo(EmailAnexo anexo) => _anexos.Add(anexo);
}
