using KanbanEmails.Application.DTOs;
using KanbanEmails.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KanbanEmails.Api.Controllers;

/// <summary>
/// Gerenciamento de usuários e configurações de acesso ao e-mail IMAP.
/// </summary>
[ApiController]
[Route("api/usuarios")]
[Produces("application/json")]
[Authorize]
public class UsuariosController(IUsuarioService service) : ControllerBase
{
    /// <summary>
    /// Lista todos os usuários cadastrados.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var usuarios = await service.ListarAsync(ct);
        return Ok(usuarios);
    }

    /// <summary>
    /// Retorna um usuário pelo identificador.
    /// </summary>
    /// <param name="id">Identificador do usuário.</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var usuario = await service.ObterPorIdAsync(id, ct);
        return usuario is null ? NotFound() : Ok(usuario);
    }

    /// <summary>
    /// Cria um novo usuário.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar([FromBody] CriarUsuarioDto dto, CancellationToken ct)
    {
        try
        {
            var criado = await service.CriarAsync(dto, ct);
            return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza nome e e-mail de um usuário.
    /// </summary>
    /// <param name="id">Identificador do usuário.</param>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarUsuarioDto dto, CancellationToken ct)
    {
        try
        {
            var atualizado = await service.AtualizarAsync(id, dto, ct);
            return atualizado is null ? NotFound() : Ok(atualizado);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }

    /// <summary>
    /// Altera a senha do usuário informando a senha atual para confirmação.
    /// </summary>
    /// <param name="id">Identificador do usuário.</param>
    [HttpPatch("{id:int}/senha")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AlterarSenha(int id, [FromBody] AlterarSenhaDto dto, CancellationToken ct)
    {
        try
        {
            await service.AlterarSenhaAsync(id, dto, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    /// <summary>
    /// Salva ou atualiza a configuração de acesso ao servidor IMAP do usuário.
    /// </summary>
    /// <param name="id">Identificador do usuário.</param>
    [HttpPut("{id:int}/configuracao-email")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SalvarConfiguracaoEmail(int id, [FromBody] SalvarConfiguracaoEmailDto dto, CancellationToken ct)
    {
        var resultado = await service.SalvarConfiguracaoEmailAsync(id, dto, ct);
        return resultado is null ? NotFound() : Ok(resultado);
    }

    /// <summary>
    /// Ativa ou inativa um usuário.
    /// </summary>
    /// <param name="id">Identificador do usuário.</param>
    /// <param name="ativo">true para ativar, false para inativar.</param>
    [HttpPatch("{id:int}/ativo")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DefinirAtivo(int id, [FromQuery] bool ativo, CancellationToken ct)
    {
        try
        {
            await service.DefinirAtivoAsync(id, ativo, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
