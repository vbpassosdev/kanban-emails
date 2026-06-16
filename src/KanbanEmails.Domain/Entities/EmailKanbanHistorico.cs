using KanbanEmails.Domain.Enums;

namespace KanbanEmails.Domain.Entities;

public class EmailKanbanHistorico
{
    public int Id { get; private set; }
    public int EmailKanbanId { get; private set; }
    public StatusKanban? StatusAnterior { get; private set; }
    public StatusKanban StatusNovo { get; private set; }
    public string? Observacao { get; private set; }
    public string? Usuario { get; private set; }
    public DateTime DataMovimento { get; private set; }

    public EmailKanban? Email { get; private set; }

    protected EmailKanbanHistorico() { }

    public static EmailKanbanHistorico Criar(
        int emailKanbanId,
        StatusKanban? statusAnterior,
        StatusKanban statusNovo,
        string? observacao,
        string? usuario)
    {
        return new EmailKanbanHistorico
        {
            EmailKanbanId = emailKanbanId,
            StatusAnterior = statusAnterior,
            StatusNovo = statusNovo,
            Observacao = observacao,
            Usuario = usuario,
            DataMovimento = DateTime.UtcNow
        };
    }
}
