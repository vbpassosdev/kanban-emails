using KanbanEmails.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KanbanEmails.Api.Controllers;

[ApiController]
[Route("api")]
public class AnexosController(IEmailAnexoRepository anexoRepository) : ControllerBase
{
    [HttpGet("emails-kanban/{emailId:int}/anexos")]
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

    [HttpGet("anexos/{id:int}/download")]
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
