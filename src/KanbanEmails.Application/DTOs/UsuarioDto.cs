namespace KanbanEmails.Application.DTOs;

public record UsuarioDto(
    int Id,
    string Nome,
    string Email,
    bool Ativo,
    DateTime DataCriacao,
    DateTime? DataAtualizacao,
    ConfiguracaoEmailDto? ConfiguracaoEmail);

public record ConfiguracaoEmailDto(
    int Id,
    string Host,
    int Porta,
    bool UsarSsl,
    string EmailUsuario,
    string Pasta,
    int IntervaloMinutos,
    DateTime DataCriacao,
    DateTime? DataAtualizacao);

public record CriarUsuarioDto(
    string Nome,
    string Email,
    string Senha);

public record AtualizarUsuarioDto(
    string Nome,
    string Email);

public record AlterarSenhaDto(
    string SenhaAtual,
    string NovaSenha);

public record SalvarConfiguracaoEmailDto(
    string Host,
    int Porta,
    bool UsarSsl,
    string EmailUsuario,
    string Senha,
    string Pasta = "INBOX",
    int IntervaloMinutos = 5);

public record LoginDto(
    string Email,
    string Senha);

public record LoginResponseDto(
    string Token,
    DateTime Expiracao,
    UsuarioDto Usuario);
