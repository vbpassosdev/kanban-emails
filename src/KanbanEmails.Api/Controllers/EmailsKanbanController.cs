using KanbanEmails.Application.DTOs;
using KanbanEmails.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KanbanEmails.Api.Controllers;

[ApiController]
[Route("api/emails-kanban")]
public class EmailsKanbanController(
    IEmailKanbanService service,
    IEmailProcessorService processor) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? remetente,
        [FromQuery] string? assunto,
        [FromQuery] string? status,
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 50,
        CancellationToken ct = default)
    {
        var resultado = await service.ListarAsync(remetente, assunto, status, dataInicio, dataFim, pagina, tamanhoPagina, ct);
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterDetalhe(int id, CancellationToken ct)
    {
        var detalhe = await service.ObterDetalheAsync(id, ct);
        return detalhe is null ? NotFound() : Ok(detalhe);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> AlterarStatus(int id, [FromBody] AlterarStatusDto dto, CancellationToken ct)
    {
        try
        {
            await service.AlterarStatusAsync(id, dto, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sincronizar(CancellationToken ct)
    {
        var processados = await processor.ProcessarEmailsAsync(ct);
        return Ok(new { mensagem = $"{processados} e-mail(s) novo(s) processado(s).", processados });
    }
}
