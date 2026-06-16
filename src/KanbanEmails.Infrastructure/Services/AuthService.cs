using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KanbanEmails.Application.DTOs;
using KanbanEmails.Application.Interfaces;
using KanbanEmails.Domain.Interfaces;
using KanbanEmails.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace KanbanEmails.Infrastructure.Services;

public class AuthService(IUsuarioRepository repository, IConfiguration configuration) : IAuthService
{
    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var usuario = await repository.ObterPorEmailAsync(dto.Email, ct);

        if (usuario is null || !usuario.Ativo)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
            return null;

        var jwtSettings = configuration.GetSection("Jwt");
        var secret = jwtSettings["Secret"]!;
        var issuer = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;
        var expiracaoHoras = int.Parse(jwtSettings["ExpiracaoHoras"] ?? "8");

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

        var usuarioDto = new UsuarioDto(
            usuario.Id, usuario.Nome, usuario.Email, usuario.Ativo,
            usuario.DataCriacao, usuario.DataAtualizacao,
            usuario.ConfiguracaoEmail is null ? null : new ConfiguracaoEmailDto(
                usuario.ConfiguracaoEmail.Id,
                usuario.ConfiguracaoEmail.Host,
                usuario.ConfiguracaoEmail.Porta,
                usuario.ConfiguracaoEmail.UsarSsl,
                usuario.ConfiguracaoEmail.EmailUsuario,
                usuario.ConfiguracaoEmail.Pasta,
                usuario.ConfiguracaoEmail.IntervaloMinutos,
                usuario.ConfiguracaoEmail.DataCriacao,
                usuario.ConfiguracaoEmail.DataAtualizacao));

        return new LoginResponseDto(tokenString, expiracao, usuarioDto);
    }
}
