using KanbanEmails.Application.DTOs;
using KanbanEmails.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KanbanEmails.Api.Controllers;

/// <summary>
/// Operações sobre anexos de e-mails.
/// </summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
public class AnexosController(IEmailAnexoRepository anexoRepository) : ControllerBase
{
    /// <summary>
    /// Lista todos os anexos de um e-mail específico.
    /// </summary>
    /// <param name="emailId">Identificador do e-mail.</param>
    [HttpGet("emails-kanban/{emailId:int}/anexos")]
    [ProducesResponseType(typeof(IEnumerable<EmailAnexoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarAnexos(int emailId, CancellationToken ct)
    {
        var anexos = await anexoRepository.ListarPorEmailAsync(emailId, ct);
        return Ok(anexos.Select(a => new
        {
            a.Id,
            a.EmailKanbanId,
            a.NomeArquivo,
            a.MimeType,
            a.TamanhoBytes,
            a.DataCriacao
        }));
    }

    /// <summary>
    /// Realiza o download de um anexo pelo seu identificador.
    /// </summary>
    /// <param name="id">Identificador do anexo.</param>
    [HttpGet("anexos/{id:int}/download")]
    [Produces("application/octet-stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(int id, CancellationToken ct)
    {
        var anexo = await anexoRepository.ObterPorIdAsync(id, ct);
        if (anexo is null) return NotFound();

        if (!System.IO.File.Exists(anexo.CaminhoArquivo))
            return NotFound(new { mensagem = "Arquivo não encontrado no disco." });

        var stream = System.IO.File.OpenRead(anexo.CaminhoArquivo);
        var mimeType = anexo.MimeType ?? "application/octet-stream";
        return File(stream, mimeType, anexo.NomeArquivo);
    }
}
