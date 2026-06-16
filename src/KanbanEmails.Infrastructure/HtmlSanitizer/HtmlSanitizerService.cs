using Ganss.Xss;
using KanbanEmails.Application.Interfaces;

namespace KanbanEmails.Infrastructure.HtmlSanitizer;

public class HtmlSanitizerService : IHtmlSanitizerService
{
    private static readonly Ganss.Xss.HtmlSanitizer _sanitizer = new();

    public string Sanitizar(string html) => _sanitizer.Sanitize(html);
}
