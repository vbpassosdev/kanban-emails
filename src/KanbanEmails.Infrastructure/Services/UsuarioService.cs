using KanbanEmails.Application.DTOs;
using KanbanEmails.Application.Interfaces;
using KanbanEmails.Domain.Entities;
using KanbanEmails.Domain.Interfaces;
using KanbanEmails.Infrastructure.Security;

namespace KanbanEmails.Infrastructure.Services;

public class UsuarioService(IUsuarioRepository repository, CriptografiaService criptografia) : IUsuarioService
{
    public async Task<IEnumerable<UsuarioDto>> ListarAsync(CancellationToken ct = default)
    {
        var usuarios = await repository.ListarAsync(ct);
        return usuarios.Select(MapearDto);
    }

    public async Task<UsuarioDto?> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var usuario = await repository.ObterPorIdAsync(id, ct);
        return usuario is null ? null : MapearDto(usuario);
    }

    public async Task<UsuarioDto> CriarAsync(CriarUsuarioDto dto, CancellationToken ct = default)
    {
        if (await repository.ExisteEmailAsync(dto.Email, ct))
            throw new InvalidOperationException($"Já existe um usuário com o e-mail '{dto.Email}'.");

        var hash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);
        var usuario = Usuario.Criar(dto.Nome, dto.Email, hash);

        await repository.AdicionarAsync(usuario, ct);
        await repository.SalvarAsync(ct);

        return MapearDto(usuario);
    }

    public async Task<UsuarioDto?> AtualizarAsync(int id, AtualizarUsuarioDto dto, CancellationToken ct = default)
    {
        var usuario = await repository.ObterPorIdAsync(id, ct);
        if (usuario is null) return null;

        if (!usuario.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase) &&
            await repository.ExisteEmailAsync(dto.Email, ct))
            throw new InvalidOperationException($"Já existe um usuário com o e-mail '{dto.Email}'.");

        usuario.Atualizar(dto.Nome, dto.Email);
        await repository.SalvarAsync(ct);

        return MapearDto(usuario);
    }

    public async Task AlterarSenhaAsync(int id, AlterarSenhaDto dto, CancellationToken ct = default)
    {
        var usuario = await repository.ObterPorIdAsync(id, ct);
        if (usuario is null) throw new KeyNotFoundException("Usuário não encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(dto.SenhaAtual, usuario.SenhaHash))
            throw new UnauthorizedAccessException("Senha atual incorreta.");

        usuario.AlterarSenha(BCrypt.Net.BCrypt.HashPassword(dto.NovaSenha));
        await repository.SalvarAsync(ct);
    }

    public async Task<UsuarioDto?> SalvarConfiguracaoEmailAsync(int id, SalvarConfiguracaoEmailDto dto, CancellationToken ct = default)
    {
        var usuario = await repository.ObterPorIdAsync(id, ct);
        if (usuario is null) return null;

        var senhaCriptografada = criptografia.Criptografar(dto.Senha);

        if (usuario.ConfiguracaoEmail is null)
        {
            var config = ConfiguracaoEmail.Criar(
                usuario.Id, dto.Host, dto.Porta, dto.UsarSsl,
                dto.EmailUsuario, senhaCriptografada, dto.Pasta, dto.IntervaloMinutos);

            usuario.DefinirConfiguracaoEmail(config);
        }
        else
        {
            usuario.ConfiguracaoEmail.Atualizar(
                dto.Host, dto.Porta, dto.UsarSsl,
                dto.EmailUsuario, senhaCriptografada, dto.Pasta, dto.IntervaloMinutos);
        }

        await repository.SalvarAsync(ct);
        return MapearDto(usuario);
    }

    public async Task DefinirAtivoAsync(int id, bool ativo, CancellationToken ct = default)
    {
        var usuario = await repository.ObterPorIdAsync(id, ct);
        if (usuario is null) throw new KeyNotFoundException("Usuário não encontrado.");

        usuario.DefinirAtivo(ativo);
        await repository.SalvarAsync(ct);
    }

    private static UsuarioDto MapearDto(Usuario u) => new(
        u.Id, u.Nome, u.Email, u.Ativo, u.DataCriacao, u.DataAtualizacao,
        u.ConfiguracaoEmail is null ? null : new ConfiguracaoEmailDto(
            u.ConfiguracaoEmail.Id,
            u.ConfiguracaoEmail.Host,
            u.ConfiguracaoEmail.Porta,
            u.ConfiguracaoEmail.UsarSsl,
            u.ConfiguracaoEmail.EmailUsuario,
            u.ConfiguracaoEmail.Pasta,
            u.ConfiguracaoEmail.IntervaloMinutos,
            u.ConfiguracaoEmail.DataCriacao,
            u.ConfiguracaoEmail.DataAtualizacao));
}
