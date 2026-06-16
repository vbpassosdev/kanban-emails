using KanbanEmails.Application.DTOs;
using KanbanEmails.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KanbanEmails.Api.Controllers;

/// <summary>
/// Autenticação de usuários no sistema.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Realiza o login e retorna o token JWT.
    /// </summary>
    [HttpPost("login")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        var resultado = await authService.LoginAsync(dto, ct);
        return resultado is null
            ? Unauthorized(new { mensagem = "E-mail ou senha inválidos." })
            : Ok(resultado);
    }

    /// <summary>
    /// Retorna os dados do usuário autenticado pelo token JWT.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        var id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                  ?? User.FindFirst("email")?.Value;
        var nome = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        return Ok(new { id, nome, email });
    }
}
