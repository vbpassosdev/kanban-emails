namespace KanbanEmails.Application.DTOs;

public record AlterarStatusDto(
    string Status,
    string? Observacao = null,
    string? Usuario = null);
