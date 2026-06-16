using KanbanEmails.Application.DTOs;

namespace KanbanEmails.Application.Interfaces;

public interface IRemetenteMonitoradoService
{
    Task<IEnumerable<RemetenteMonitoradoDto>> ListarAsync(CancellationToken ct = default);
    Task<RemetenteMonitoradoDto> CriarAsync(CriarRemetenteMonitoradoDto dto, CancellationToken ct = default);
    Task<RemetenteMonitoradoDto?> AtualizarAsync(int id, AtualizarRemetenteMonitoradoDto dto, CancellationToken ct = default);
    Task RemoverAsync(int id, CancellationToken ct = default);
}
