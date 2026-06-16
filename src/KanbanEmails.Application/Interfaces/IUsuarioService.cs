using KanbanEmails.Application.DTOs;

namespace KanbanEmails.Application.Interfaces;

public interface IUsuarioService
{
    Task<IEnumerable<UsuarioDto>> ListarAsync(CancellationToken ct = default);
    Task<UsuarioDto?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<UsuarioDto> CriarAsync(CriarUsuarioDto dto, CancellationToken ct = default);
    Task<UsuarioDto?> AtualizarAsync(int id, AtualizarUsuarioDto dto, CancellationToken ct = default);
    Task AlterarSenhaAsync(int id, AlterarSenhaDto dto, CancellationToken ct = default);
    Task<UsuarioDto?> SalvarConfiguracaoEmailAsync(int id, SalvarConfiguracaoEmailDto dto, CancellationToken ct = default);
    Task DefinirAtivoAsync(int id, bool ativo, CancellationToken ct = default);
}
