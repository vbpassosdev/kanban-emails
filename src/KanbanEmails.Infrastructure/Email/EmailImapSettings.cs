namespace KanbanEmails.Infrastructure.Email;

public class EmailImapSettings
{
    public const string SectionName = "EmailImap";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 993;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int IntervaloMinutos { get; set; } = 5;
    public string PastaAnexos { get; set; } = "storage/email-anexos";
    public int DiasParaBuscar { get; set; } = 90;
}
