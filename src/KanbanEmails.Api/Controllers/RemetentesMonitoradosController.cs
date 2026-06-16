using KanbanEmails.Application.DTOs;
using KanbanEmails.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KanbanEmails.Api.Controllers;

[ApiController]
[Route("api/remetentes-monitorados")]
public class RemetentesMonitoradosController(IRemetenteMonitoradoService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var remetentes = await service.ListarAsync(ct);
        return Ok(remetentes);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarRemetenteMonitoradoDto dto, CancellationToken ct)
    {
        var criado = await service.CriarAsync(dto, ct);
        return CreatedAtAction(nameof(Listar), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarRemetenteMonitoradoDto dto, CancellationToken ct)
    {
        var atualizado = await service.AtualizarAsync(id, dto, ct);
        return atualizado is null ? NotFound() : Ok(atualizado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id, CancellationToken ct)
    {
        await service.RemoverAsync(id, ct);
        return NoContent();
    }
}
