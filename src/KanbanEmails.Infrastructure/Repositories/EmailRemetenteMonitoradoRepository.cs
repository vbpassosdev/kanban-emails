using KanbanEmails.Domain.Entities;
using KanbanEmails.Domain.Interfaces;
using KanbanEmails.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KanbanEmails.Infrastructure.Repositories;

public class EmailRemetenteMonitoradoRepository(KanbanEmailsDbContext context) : IEmailRemetenteMonitoradoRepository
{
    public async Task<EmailRemetenteMonitorado?> ObterPorIdAsync(int id, CancellationToken ct = default)
        => await context.EmailRemetentesMonitorados.FindAsync([id], ct);

    public async Task<IEnumerable<EmailRemetenteMonitorado>> ListarAtivosAsync(CancellationToken ct = default)
        => await context.EmailRemetentesMonitorados
            .Where(r => r.Ativo)
            .OrderBy(r => r.EmailOuDominio)
            .ToListAsync(ct);

    public async Task<IEnumerable<EmailRemetenteMonitorado>> ListarTodosAsync(CancellationToken ct = default)
        => await context.EmailRemetentesMonitorados
            .OrderBy(r => r.EmailOuDominio)
            .ToListAsync(ct);

    public async Task<bool> RemetenteMonitoradoAsync(string email, CancellationToken ct = default)
    {
        var emailLower = email.ToLowerInvariant();
        var dominio = emailLower.Contains('@') ? emailLower.Split('@')[1] : emailLower;

        return await context.EmailRemetentesMonitorados
            .Where(r => r.Ativo)
            .AnyAsync(r => r.EmailOuDominio == emailLower || r.EmailOuDominio == dominio, ct);
    }

    public async Task AdicionarAsync(EmailRemetenteMonitorado remetente, CancellationToken ct = default)
        => await context.EmailRemetentesMonitorados.AddAsync(remetente, ct);

    public async Task SalvarAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);

    public async Task RemoverAsync(int id, CancellationToken ct = default)
    {
        var remetente = await ObterPorIdAsync(id, ct);
        if (remetente is not null)
            context.EmailRemetentesMonitorados.Remove(remetente);
        await context.SaveChangesAsync(ct);
    }
}
