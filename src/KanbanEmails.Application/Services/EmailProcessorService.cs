using KanbanEmails.Application.Interfaces;
using KanbanEmails.Domain.Entities;
using KanbanEmails.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace KanbanEmails.Application.Services;

public class EmailProcessorService(
    IImapEmailReader imapReader,
    IEmailKanbanRepository emailRepository,
    IEmailAnexoRepository anexoRepository,
    IEmailRemetenteMonitoradoRepository remetenteRepository,
    IStorageService storage,
    ILogger<EmailProcessorService> logger) : IEmailProcessorService
{
    public async Task<int> ProcessarEmailsAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Iniciando leitura de e-mails via IMAP.");

        IReadOnlyList<DTOs.EmailImapData> emails;
        try
        {
            emails = await imapReader.LerEmailsAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao conectar ao servidor IMAP.");
            return 0;
        }

        logger.LogInformation("{Total} e-mail(s) encontrado(s) no servidor.", emails.Count);

        int processados = 0;

        foreach (var emailData in emails)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                if (!await remetenteRepository.RemetenteMonitoradoAsync(emailData.Remetente, ct))
                {
                    logger.LogDebug("Remetente não monitorado: {Remetente}", emailData.Remetente);
                    continue;
                }

                if (await emailRepository.ExisteMessageIdAsync(emailData.MessageId, ct))
                {
                    logger.LogDebug("MessageId já processado: {MessageId}", emailData.MessageId);
                    continue;
                }

                var email = EmailKanban.Criar(
                    emailData.MessageId,
                    emailData.Remetente,
                    emailData.Assunto,
                    emailData.CorpoTexto,
                    emailData.CorpoHtml,
                    emailData.DataRecebimento);

                await emailRepository.AdicionarAsync(email, ct);
                await emailRepository.SalvarAsync(ct);

                foreach (var anexoData in emailData.Anexos)
                {
                    var caminho = await storage.SalvarAnexoAsync(email.Id, anexoData.NomeArquivo, anexoData.Conteudo, ct);
                    var anexo = EmailAnexo.Criar(email.Id, anexoData.NomeArquivo, anexoData.MimeType, anexoData.Conteudo.Length, caminho);
                    await anexoRepository.AdicionarAsync(anexo, ct);
                }

                if (emailData.Anexos.Count > 0)
                    await anexoRepository.SalvarAsync(ct);

                logger.LogInformation("E-mail processado: {Assunto} de {Remetente}", emailData.Assunto, emailData.Remetente);
                processados++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao processar e-mail {MessageId}.", emailData.MessageId);
            }
        }

        logger.LogInformation("{Processados} e-mail(s) novo(s) processado(s).", processados);
        return processados;
    }
}
