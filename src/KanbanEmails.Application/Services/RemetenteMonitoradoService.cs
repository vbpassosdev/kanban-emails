using KanbanEmails.Application.DTOs;
using KanbanEmails.Application.Interfaces;
using KanbanEmails.Domain.Entities;
using KanbanEmails.Domain.Interfaces;

namespace KanbanEmails.Application.Services;

public class RemetenteMonitoradoService(IEmailRemetenteMonitoradoRepository repository) : IRemetenteMonitoradoService
{
    public async Task<IEnumerable<RemetenteMonitoradoDto>> ListarAsync(CancellationToken ct = default)
    {
        var remetentes = await repository.ListarTodosAsync(ct);
        return remetentes.Select(ToDto);
    }

    public async Task<RemetenteMonitoradoDto> CriarAsync(CriarRemetenteMonitoradoDto dto, CancellationToken ct = default)
    {
        var remetente = EmailRemetenteMonitorado.Criar(dto.EmailOuDominio, dto.Nome);
        await repository.AdicionarAsync(remetente, ct);
        await repository.SalvarAsync(ct);
        return ToDto(remetente);
    }

    public async Task<RemetenteMonitoradoDto?> AtualizarAsync(int id, AtualizarRemetenteMonitoradoDto dto, CancellationToken ct = default)
    {
        var remetente = await repository.ObterPorIdAsync(id, ct);
        if (remetente is null) return null;

        remetente.Atualizar(dto.EmailOuDominio, dto.Nome, dto.Ativo);
        await repository.SalvarAsync(ct);
        return ToDto(remetente);
    }

    public async Task RemoverAsync(int id, CancellationToken ct = default)
        => await repository.RemoverAsync(id, ct);

    private static RemetenteMonitoradoDto ToDto(EmailRemetenteMonitorado r)
        => new(r.Id, r.Nome, r.EmailOuDominio, r.Ativo, r.DataCriacao);
}
