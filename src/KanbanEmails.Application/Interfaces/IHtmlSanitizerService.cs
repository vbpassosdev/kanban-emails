namespace KanbanEmails.Application.Interfaces;

public interface IHtmlSanitizerService
{
    string Sanitizar(string html);
}
