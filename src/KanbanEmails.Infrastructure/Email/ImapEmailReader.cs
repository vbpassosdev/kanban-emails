using KanbanEmails.Application.DTOs;
using KanbanEmails.Application.Interfaces;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace KanbanEmails.Infrastructure.Email;

public class ImapEmailReader(
    IOptions<EmailImapSettings> options,
    ILogger<ImapEmailReader> logger) : IImapEmailReader
{
    private readonly EmailImapSettings _settings = options.Value;

    public async Task<IReadOnlyList<EmailImapData>> LerEmailsAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Username) || string.IsNullOrWhiteSpace(_settings.Password))
        {
            logger.LogWarning("Credenciais IMAP não configuradas. Configure EmailImap:Username e EmailImap:Password.");
            return [];
        }

        using var client = new ImapClient();

        var sslOptions = _settings.UseSsl
            ? MailKit.Security.SecureSocketOptions.SslOnConnect
            : MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable;

        logger.LogInformation("Conectando ao IMAP {Host}:{Port}", _settings.Host, _settings.Port);
        await client.ConnectAsync(_settings.Host, _settings.Port, sslOptions, ct);
        await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);

        var inbox = client.Inbox;
        await inbox.OpenAsync(FolderAccess.ReadOnly, ct);

        var cutoff = DateTime.UtcNow.AddDays(-_settings.DiasParaBuscar);
        var uids = await inbox.SearchAsync(SearchQuery.DeliveredAfter(cutoff), ct);

        logger.LogInformation("{Count} mensagem(ns) encontrada(s) desde {Cutoff:dd/MM/yyyy}.", uids.Count, cutoff);

        var result = new List<EmailImapData>();

        foreach (var uid in uids)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                var message = await inbox.GetMessageAsync(uid, ct);
                result.Add(MapToEmailImapData(message));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Erro ao ler mensagem UID {Uid}.", uid);
            }
        }

        await client.DisconnectAsync(true, ct);
        return result;
    }

    private static EmailImapData MapToEmailImapData(MimeMessage message)
    {
        var messageId = message.MessageId ?? Guid.NewGuid().ToString();
        var remetente = message.From.Mailboxes.FirstOrDefault()?.Address ?? "desconhecido@desconhecido.com";
        var assunto = message.Subject;
        var dataRecebimento = message.Date.UtcDateTime;
        var corpoTexto = message.TextBody;
        var corpoHtml = message.HtmlBody;

        var anexos = new List<EmailImapAnexoData>();
        foreach (var part in message.Attachments)
        {
            if (part is MimePart mimePart && mimePart.Content is not null)
            {
                var nomeArquivo = SanitizarNomeArquivo(mimePart.FileName ?? $"anexo_{Guid.NewGuid()}");
                using var ms = new MemoryStream();
                mimePart.Content.DecodeTo(ms);
                anexos.Add(new EmailImapAnexoData(nomeArquivo, mimePart.ContentType.MimeType, ms.ToArray()));
            }
        }

        return new EmailImapData(messageId, remetente, assunto, corpoTexto, corpoHtml, dataRecebimento, anexos);
    }

    private static string SanitizarNomeArquivo(string nome)
    {
        var invalidos = Path.GetInvalidFileNameChars();
        var sanitizado = new string(nome.Select(c => invalidos.Contains(c) ? '_' : c).ToArray());
        return sanitizado.Length > 255 ? sanitizado[..255] : sanitizado;
    }
}
