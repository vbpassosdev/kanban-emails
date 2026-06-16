using KanbanEmails.Application.DTOs;
using KanbanEmails.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KanbanEmails.Api.Controllers;

/// <summary>
/// Gerenciamento dos remetentes monitorados para captura automática de e-mails.
/// </summary>
[ApiController]
[Route("api/remetentes-monitorados")]
[Produces("application/json")]
public class RemetentesMonitoradosController(IRemetenteMonitoradoService service) : ControllerBase
{
    /// <summary>
    /// Lista todos os remetentes monitorados cadastrados.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RemetenteMonitoradoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var remetentes = await service.ListarAsync(ct);
        return Ok(remetentes);
    }

    /// <summary>
    /// Cadastra um novo remetente monitorado (e-mail ou domínio).
    /// </summary>
    /// <param name="dto">E-mail ou domínio a monitorar e nome de exibição opcional.</param>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(RemetenteMonitoradoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarRemetenteMonitoradoDto dto, CancellationToken ct)
    {
        var criado = await service.CriarAsync(dto, ct);
        return CreatedAtAction(nameof(Listar), new { id = criado.Id }, criado);
    }

    /// <summary>
    /// Atualiza os dados de um remetente monitorado existente.
    /// </summary>
    /// <param name="id">Identificador do remetente monitorado.</param>
    /// <param name="dto">Novos dados do remetente.</param>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(RemetenteMonitoradoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarRemetenteMonitoradoDto dto, CancellationToken ct)
    {
        var atualizado = await service.AtualizarAsync(id, dto, ct);
        return atualizado is null ? NotFound() : Ok(atualizado);
    }

    /// <summary>
    /// Remove um remetente monitorado pelo identificador.
    /// </summary>
    /// <param name="id">Identificador do remetente monitorado.</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id, CancellationToken ct)
    {
        await service.RemoverAsync(id, ct);
        return NoContent();
    }
}
