using KanbanEmails.Domain.Entities;
using KanbanEmails.Domain.Interfaces;
using KanbanEmails.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KanbanEmails.Infrastructure.Repositories;

public class EmailKanbanRepository(KanbanEmailsDbContext context) : IEmailKanbanRepository
{
    public async Task<EmailKanban?> ObterPorIdAsync(int id, CancellationToken ct = default)
        => await context.EmailsKanban
            .Include(e => e.Anexos)
            .Include(e => e.Historico)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<EmailKanban?> ObterPorMessageIdAsync(string messageId, CancellationToken ct = default)
        => await context.EmailsKanban
            .FirstOrDefaultAsync(e => e.MessageId == messageId, ct);

    public async Task<bool> ExisteMessageIdAsync(string messageId, CancellationToken ct = default)
        => await context.EmailsKanban.AnyAsync(e => e.MessageId == messageId, ct);

    public async Task<IEnumerable<EmailKanban>> ListarAsync(FiltroEmailKanban filtro, CancellationToken ct = default)
    {
        var query = context.EmailsKanban
            .Include(e => e.Anexos)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.Remetente))
            query = query.Where(e => e.Remetente.Contains(filtro.Remetente));

        if (!string.IsNullOrWhiteSpace(filtro.Assunto))
            query = query.Where(e => e.Assunto != null && e.Assunto.Contains(filtro.Assunto));

        if (filtro.Status.HasValue)
            query = query.Where(e => e.Status == filtro.Status.Value);

        if (filtro.DataInicio.HasValue)
            query = query.Where(e => e.DataRecebimento >= filtro.DataInicio.Value);

        if (filtro.DataFim.HasValue)
            query = query.Where(e => e.DataRecebimento <= filtro.DataFim.Value);

        return await query
            .OrderByDescending(e => e.DataRecebimento)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .ToListAsync(ct);
    }

    public async Task AdicionarAsync(EmailKanban email, CancellationToken ct = default)
        => await context.EmailsKanban.AddAsync(email, ct);

    public async Task SalvarAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);
}
