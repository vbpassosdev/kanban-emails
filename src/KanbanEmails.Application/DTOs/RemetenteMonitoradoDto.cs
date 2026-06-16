namespace KanbanEmails.Application.DTOs;

public record RemetenteMonitoradoDto(
    int Id,
    string? Nome,
    string EmailOuDominio,
    bool Ativo,
    DateTime DataCriacao);

public record CriarRemetenteMonitoradoDto(
    string EmailOuDominio,
    string? Nome);

public record AtualizarRemetenteMonitoradoDto(
    string EmailOuDominio,
    string? Nome,
    bool Ativo);
