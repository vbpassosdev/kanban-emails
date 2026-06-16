using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KanbanEmails.Application.DTOs;
using KanbanEmails.Application.Interfaces;
using KanbanEmails.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace KanbanEmails.Infrastructure.Services;

public class AuthService(
    IUsuarioRepository repository,
    IConfiguration configuration,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var usuario = await repository.ObterPorEmailAsync(dto.Email, ct);

        if (usuario is null || !usuario.Ativo)
        {
            logger.LogWarning("Tentativa de login falhou: usuário não encontrado ou inativo — {Email}", dto.Email);
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
        {
            logger.LogWarning("Tentativa de login falhou: senha incorreta — {Email}", dto.Email);
            return null;
        }

        var jwtSettings = configuration.GetSection("Jwt");
        var secret = jwtSettings["Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret não configurado.");
        var issuer = jwtSettings["Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer não configurado.");
        var audience = jwtSettings["Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience não configurado.");
        var expiracaoHoras = int.TryParse(jwtSettings["ExpiracaoHoras"], out var h) ? h : 8;

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
        var expiracao = DateTime.UtcNow.AddHours(expiracaoHoras);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiracao,
            signingCredentials: credenciais);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        logger.LogInformation("Login realizado com sucesso — {Email} (Id: {Id})", usuario.Email, usuario.Id);

        return new LoginResponseDto(
            tokenString,
            expiracao,
            new LoginUsuarioDto(usuario.Id, usuario.Nome, usuario.Email, usuario.Ativo));
    }
}
