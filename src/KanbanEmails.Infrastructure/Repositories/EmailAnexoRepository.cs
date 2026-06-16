using KanbanEmails.Domain.Entities;
using KanbanEmails.Domain.Interfaces;
using KanbanEmails.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KanbanEmails.Infrastructure.Repositories;

public class EmailAnexoRepository(KanbanEmailsDbContext context) : IEmailAnexoRepository
{
    public async Task<EmailAnexo?> ObterPorIdAsync(int id, CancellationToken ct = default)
        => await context.EmailAnexos.FindAsync([id], ct);

    public async Task<IEnumerable<EmailAnexo>> ListarPorEmailAsync(int emailKanbanId, CancellationToken ct = default)
        => await context.EmailAnexos
            .Where(a => a.EmailKanbanId == emailKanbanId)
            .ToListAsync(ct);

    public async Task AdicionarAsync(EmailAnexo anexo, CancellationToken ct = default)
        => await context.EmailAnexos.AddAsync(anexo, ct);

    public async Task SalvarAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);
}
