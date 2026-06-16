using KanbanEmails.Domain.Entities;

namespace KanbanEmails.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default);
    Task<IEnumerable<Usuario>> ListarAsync(CancellationToken ct = default);
    Task AdicionarAsync(Usuario usuario, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
}
