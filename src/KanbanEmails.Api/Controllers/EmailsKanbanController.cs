using KanbanEmails.Application.DTOs;
using KanbanEmails.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KanbanEmails.Api.Controllers;

/// <summary>
/// Gerenciamento dos e-mails no quadro Kanban.
/// </summary>
[ApiController]
[Route("api/emails-kanban")]
[Produces("application/json")]
[Authorize]
public class EmailsKanbanController(
    IEmailKanbanService service,
    IEmailProcessorService processor) : ControllerBase
{
    /// <summary>
    /// Lista os e-mails com filtros e paginação.
    /// </summary>
    /// <param name="remetente">Filtra pelo endereço de e-mail do remetente.</param>
    /// <param name="assunto">Filtra por trecho no assunto (contém).</param>
    /// <param name="status">Filtra pelo status do card: Novo, Em Análise, Desenvolvimento, Aguardando Cliente, Concluído.</param>
    /// <param name="dataInicio">Data mínima de recebimento (inclusive).</param>
    /// <param name="dataFim">Data máxima de recebimento (inclusive).</param>
    /// <param name="pagina">Número da página (começa em 1).</param>
    /// <param name="tamanhoPagina">Quantidade de registros por página (padrão 50).</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmailKanbanDto>), StatusCodes.Status200OK)]
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

    /// <summary>
    /// Retorna o detalhe completo de um e-mail, incluindo corpo, anexos e histórico.
    /// </summary>
    /// <param name="id">Identificador do e-mail.</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EmailKanbanDetalheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterDetalhe(int id, CancellationToken ct)
    {
        var detalhe = await service.ObterDetalheAsync(id, ct);
        return detalhe is null ? NotFound() : Ok(detalhe);
    }

    /// <summary>
    /// Altera o status (coluna) de um card no quadro Kanban.
    /// </summary>
    /// <param name="id">Identificador do e-mail.</param>
    /// <param name="dto">Novo status e observação opcional.</param>
    [HttpPut("{id:int}/status")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Dispara a sincronização manual com a caixa IMAP e retorna a quantidade de novos e-mails processados.
    /// </summary>
    [HttpPost("sync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Sincronizar(CancellationToken ct)
    {
        var processados = await processor.ProcessarEmailsAsync(ct);
        return Ok(new { mensagem = $"{processados} e-mail(s) novo(s) processado(s).", processados });
    }
}
