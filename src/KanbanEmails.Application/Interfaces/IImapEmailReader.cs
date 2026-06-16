using KanbanEmails.Application.DTOs;

namespace KanbanEmails.Application.Interfaces;

public interface IImapEmailReader
{
    Task<IReadOnlyList<EmailImapData>> LerEmailsAsync(CancellationToken ct = default);
}
