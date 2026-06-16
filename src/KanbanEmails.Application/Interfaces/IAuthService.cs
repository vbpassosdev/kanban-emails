using KanbanEmails.Application.DTOs;

namespace KanbanEmails.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto dto, CancellationToken ct = default);
}
