using KanbanEmails.Domain.Entities;
using KanbanEmails.Domain.Interfaces;
using KanbanEmails.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KanbanEmails.Infrastructure.Repositories;

public class UsuarioRepository(KanbanEmailsDbContext context) : IUsuarioRepository
{
    public async Task<Usuario?> ObterPorIdAsync(int id, CancellationToken ct = default)
        => await context.Usuarios
            .Include(u => u.ConfiguracaoEmail)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct = default)
        => await context.Usuarios
            .Include(u => u.ConfiguracaoEmail)
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default)
        => await context.Usuarios.AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task<IEnumerable<Usuario>> ListarAsync(CancellationToken ct = default)
        => await context.Usuarios
            .Include(u => u.ConfiguracaoEmail)
            .OrderBy(u => u.Nome)
            .ToListAsync(ct);

    public async Task AdicionarAsync(Usuario usuario, CancellationToken ct = default)
        => await context.Usuarios.AddAsync(usuario, ct);

    public async Task SalvarAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);
}
