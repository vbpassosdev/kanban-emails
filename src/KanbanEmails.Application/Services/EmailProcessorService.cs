using System.Text;
using System.Text.RegularExpressions;
using KanbanEmails.Application.DTOs;
using KanbanEmails.Application.Interfaces;
using KanbanEmails.Domain.Entities;
using KanbanEmails.Domain.Enums;
using KanbanEmails.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace KanbanEmails.Application.Services;

public class EmailProcessorService(
    IImapEmailReader imapReader,
    IImapConnectionConfigProvider configProvider,
    IEmailKanbanRepository emailRepository,
    IEmailAnexoRepository anexoRepository,
    IEmailRemetenteMonitoradoRepository remetenteRepository,
    IStorageService storage,
    ILogger<EmailProcessorService> logger) : IEmailProcessorService
{
    public async Task<int> ProcessarEmailsAsync(CancellationToken ct = default)
    {
        var configs = await configProvider.ObterConfigsAtivasAsync(ct);

        if (configs.Count == 0)
        {
            logger.LogWarning("Nenhuma configuração IMAP ativa encontrada. Configure o IMAP em Configurações > Meu Perfil.");
            return 0;
        }

        int totalProcessados = 0;

        foreach (var config in configs)
        {
            if (ct.IsCancellationRequested) break;

            logger.LogInformation("Processando caixa IMAP de {Username}.", config.Username);

            IReadOnlyList<EmailImapData> emails;
            try
            {
                emails = await imapReader.LerEmailsAsync(config, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao conectar ao IMAP de {Username}.", config.Username);
                continue;
            }

            logger.LogInformation("{Total} e-mail(s) encontrado(s) para {Username}.", emails.Count, config.Username);

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

                    // Determina categoria e conteúdo antes de criar o e-mail
                    var bugreport = ExtrairInfoBugreport(emailData.Anexos);

                    string? corpoHtmlFinal;
                    string? resumoFinal;
                    string? categoriaFinal;

                    if (bugreport is not null)
                    {
                        corpoHtmlFinal = bugreport.Html;
                        resumoFinal    = bugreport.Resumo;
                        categoriaFinal = bugreport.TemSocket ? "Socket" : "Suporte";
                    }
                    else
                    {
                        corpoHtmlFinal = emailData.CorpoHtml;
                        resumoFinal    = GerarResumo(emailData.CorpoTexto ?? emailData.CorpoHtml);
                        categoriaFinal = InferirCategoria(emailData.Assunto);
                    }

                    // Categoria "Socket" roteia direto para EIdSocketError;
                    // bugreports cuja classe de exceção já foi concluída entram como Corrigido.
                    StatusKanban statusInicial;
                    if (categoriaFinal == "Socket")
                    {
                        statusInicial = StatusKanban.EIdSocketError;
                    }
                    else if (bugreport is not null)
                    {
                        var classe = ExtrairClasseExcecao(resumoFinal);
                        var jaCorrigido = classe is not null
                            && await emailRepository.ExisteErroCorrigidoAsync(classe, ct);
                        statusInicial = jaCorrigido ? StatusKanban.Corrigido : StatusKanban.Novo;
                    }
                    else
                    {
                        statusInicial = StatusKanban.Novo;
                    }

                    var email = EmailKanban.Criar(
                        emailData.MessageId,
                        emailData.Remetente,
                        emailData.Assunto,
                        emailData.CorpoTexto,
                        corpoHtmlFinal,
                        emailData.DataRecebimento,
                        statusInicial);

                    if (resumoFinal is not null)
                        email.DefinirResumo(resumoFinal, categoriaFinal);

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
                    totalProcessados++;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Erro ao processar e-mail {MessageId}.", emailData.MessageId);
                }
            }
        }

        logger.LogInformation("{Processados} e-mail(s) novo(s) processado(s) no total.", totalProcessados);
        return totalProcessados;
    }

    // ─── Marcar como Corrigido os cards Novo cujo erro já foi Concluído ──────

    public async Task<int> MarcarErrosCorrigidosAsync(CancellationToken ct = default)
    {
        var classesCorrigidas = await emailRepository.ObterClassesExcecaoConcluidasAsync(ct);
        if (classesCorrigidas.Count == 0)
        {
            logger.LogInformation("Nenhum erro concluído encontrado para comparação.");
            return 0;
        }

        var filtro = new Domain.Interfaces.FiltroEmailKanban(
            Status: StatusKanban.Novo, TamanhoPagina: 10_000);
        var emailsNovos = await emailRepository.ListarAsync(filtro, ct);

        int marcados = 0;
        foreach (var email in emailsNovos)
        {
            var classe = ExtrairClasseExcecao(email.Resumo);
            if (classe is null || !classesCorrigidas.Contains(classe)) continue;

            email.AlterarStatus(
                StatusKanban.Corrigido,
                "Marcado automaticamente: erro já concluído anteriormente");
            marcados++;
        }

        if (marcados > 0)
            await emailRepository.SalvarAsync(ct);

        logger.LogInformation("{Marcados} e-mail(s) marcado(s) como Corrigido.", marcados);
        return marcados;
    }

    private static string? ExtrairClasseExcecao(string? resumo)
    {
        if (string.IsNullOrWhiteSpace(resumo)) return null;
        var idx = resumo.IndexOf(':');
        return idx > 0 ? resumo[..idx].Trim() : null;
    }

    // ─── Backfill: re-parseia bugreports de e-mails já existentes ───────────

    public async Task<int> ReprocessarBugreportsAsync(CancellationToken ct = default)
    {
        var filtro = new Domain.Interfaces.FiltroEmailKanban(TamanhoPagina: 10_000);
        var emails = await emailRepository.ListarAsync(filtro, ct);

        int atualizados = 0;

        foreach (var email in emails)
        {
            foreach (var anexo in email.Anexos)
            {
                if (!anexo.NomeArquivo.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) continue;
                if (!File.Exists(anexo.CaminhoArquivo)) continue;

                byte[] conteudo;
                try { conteudo = await File.ReadAllBytesAsync(anexo.CaminhoArquivo, ct); }
                catch { continue; }

                string texto;
                try { texto = Encoding.UTF8.GetString(conteudo); }
                catch { texto = Encoding.Latin1.GetString(conteudo); }

                if (!texto.Contains("exception class", StringComparison.OrdinalIgnoreCase)) continue;

                var info = ParseBugreport(texto);
                if (info is null) break;

                email.DefinirCorpoHtml(info.Html);
                email.DefinirResumo(info.Resumo, info.TemSocket ? "Socket" : "Suporte");
                atualizados++;
                break; // um bugreport por e-mail é suficiente
            }
        }

        // Segundo passo: move para EIdSocketError todos com categoria Socket ainda em Novo
        foreach (var email in emails.Where(e =>
            e.Categoria == "Socket" && e.Status == StatusKanban.Novo))
        {
            email.AlterarStatus(StatusKanban.EIdSocketError, "Reprocessamento: categoria Socket");
            atualizados++;
        }

        if (atualizados > 0)
            await emailRepository.SalvarAsync(ct);

        logger.LogInformation("{Atualizados} e-mail(s) reprocessado(s) via backfill.", atualizados);
        return atualizados;
    }

    // ─── Extração do bugreport madExcept ────────────────────────────────────

    private sealed record BugreportInfo(string Html, string Resumo, bool TemSocket);

    private static BugreportInfo? ExtrairInfoBugreport(IReadOnlyList<EmailImapAnexoData> anexos)
    {
        foreach (var anexo in anexos)
        {
            if (!anexo.NomeArquivo.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                continue;

            string texto;
            try
            {
                // madExcept usa codepage do sistema; tenta UTF-8, cai em Latin-1
                texto = Encoding.UTF8.GetString(anexo.Conteudo);
            }
            catch
            {
                texto = Encoding.Latin1.GetString(anexo.Conteudo);
            }

            if (!texto.Contains("exception class", StringComparison.OrdinalIgnoreCase))
                continue;

            var info = ParseBugreport(texto);
            if (info is null) continue;
            return info;
        }
        return null;
    }

    private static BugreportInfo? ParseBugreport(string texto)
    {
        string Campo(string chave)
        {
            var m = Regex.Match(texto, $@"^{Regex.Escape(chave)}\s*:\s*(.+)$",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return m.Success ? m.Groups[1].Value.Trim() : string.Empty;
        }

        // Cabeçalho do sistema
        var dataHora     = Campo("date/time");
        var computador   = Campo("computer name");
        var usuario      = Campo("user name");
        var versao       = Campo("version");
        var os           = Campo("operating system");

        // Todas as exceções principais (podem ter mais de uma por arquivo)
        var excecoes = new List<(string Classe, string Mensagem)>();
        var matchesClasse   = Regex.Matches(texto, @"^exception class\s*:\s*(.+)$",   RegexOptions.IgnoreCase | RegexOptions.Multiline);
        var matchesMensagem = Regex.Matches(texto, @"^exception message\s*:\s*(.+)$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        for (int i = 0; i < matchesClasse.Count; i++)
        {
            var classe   = matchesClasse[i].Groups[1].Value.Trim();
            var mensagem = i < matchesMensagem.Count ? matchesMensagem[i].Groups[1].Value.Trim() : string.Empty;
            excecoes.Add((classe, mensagem));
        }

        if (excecoes.Count == 0) return null;

        // Exceções internas (linhas iniciando com >>)
        var internas = Regex.Matches(texto, @"^>>\s+(.+)$", RegexOptions.Multiline)
            .Cast<Match>()
            .Select(m => m.Groups[1].Value.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        bool temSocket = excecoes.Any(e =>
                e.Classe.Contains("Socket",  StringComparison.OrdinalIgnoreCase) ||
                e.Mensagem.Contains("Socket", StringComparison.OrdinalIgnoreCase)) ||
            internas.Any(i => i.Contains("Socket", StringComparison.OrdinalIgnoreCase));

        var primeiraExcecao = excecoes[0];
        var resumo = $"{primeiraExcecao.Classe}: {primeiraExcecao.Mensagem}".TruncatarEm(300);

        var html = MontarHtml(dataHora, computador, usuario, versao, os, excecoes, internas, temSocket);
        return new BugreportInfo(html, resumo, temSocket);
    }

    private static string MontarHtml(
        string dataHora, string computador, string usuario, string versao, string os,
        List<(string Classe, string Mensagem)> excecoes,
        List<string> internas,
        bool temSocket)
    {
        var sb = new StringBuilder();
        sb.Append("<div style='font-family:monospace;font-size:13px;line-height:1.5'>");

        // Informações do sistema
        sb.Append("<table style='border-collapse:collapse;margin-bottom:12px;font-size:12px'>");
        Linha(sb, "Data/Hora",   dataHora);
        Linha(sb, "Computador",  computador);
        Linha(sb, "Usuário",     usuario);
        Linha(sb, "Versão",      versao);
        Linha(sb, "Sistema",     os);
        sb.Append("</table>");

        // Exceções principais
        sb.Append("<p style='font-weight:bold;margin:8px 0 4px'>Exceções:</p>");
        sb.Append("<table style='border-collapse:collapse;width:100%;margin-bottom:8px'>");
        sb.Append("<tr style='background:#f3f4f6'>");
        sb.Append("<th style='border:1px solid #d1d5db;padding:4px 8px;text-align:left'>#</th>");
        sb.Append("<th style='border:1px solid #d1d5db;padding:4px 8px;text-align:left'>Classe</th>");
        sb.Append("<th style='border:1px solid #d1d5db;padding:4px 8px;text-align:left'>Mensagem</th>");
        sb.Append("</tr>");

        for (int i = 0; i < excecoes.Count; i++)
        {
            var (classe, mensagem) = excecoes[i];
            sb.Append("<tr>");
            sb.Append($"<td style='border:1px solid #d1d5db;padding:4px 8px;color:#6b7280'>{i + 1}</td>");
            sb.Append($"<td style='border:1px solid #d1d5db;padding:4px 8px;font-weight:bold'>{HtmlEncode(classe)}</td>");
            sb.Append($"<td style='border:1px solid #d1d5db;padding:4px 8px'>{HtmlEncode(mensagem)}</td>");
            sb.Append("</tr>");
        }
        sb.Append("</table>");

        // Exceções internas (socket errors aparecem aqui)
        if (internas.Count > 0)
        {
            var corBorda  = temSocket ? "#fca5a5" : "#d1d5db";
            var corFundo  = temSocket ? "#fff1f2" : "#f9fafb";
            var corTitulo = temSocket ? "#b91c1c" : "#374151";

            sb.Append($"<p style='font-weight:bold;margin:8px 0 4px;color:{corTitulo}'>Exceções Internas{(temSocket ? " ⚠ Socket Error detectado" : "")}:</p>");
            sb.Append($"<div style='background:{corFundo};border:1px solid {corBorda};border-radius:4px;padding:8px;font-size:12px'>");

            foreach (var interna in internas)
            {
                var textoInterna = HtmlEncode(interna);
                if (interna.Contains("Socket", StringComparison.OrdinalIgnoreCase))
                    textoInterna = $"<strong style='color:#b91c1c'>{textoInterna}</strong>";
                sb.Append($"<div style='margin-bottom:4px'>&gt;&gt; {textoInterna}</div>");
            }

            sb.Append("</div>");
        }

        sb.Append("</div>");
        return sb.ToString();
    }

    private static void Linha(StringBuilder sb, string chave, string valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return;
        sb.Append("<tr>");
        sb.Append($"<td style='padding:2px 8px 2px 0;color:#6b7280;white-space:nowrap'>{chave}:</td>");
        sb.Append($"<td style='padding:2px 0'>{HtmlEncode(valor)}</td>");
        sb.Append("</tr>");
    }

    private static string HtmlEncode(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

    // ─── Helpers originais ───────────────────────────────────────────────────

    private static string? GerarResumo(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        var limpo = Regex.Replace(texto, "<[^>]+>", " ");
        limpo = Regex.Replace(limpo, @"\s+", " ").Trim();
        return limpo.Length <= 300 ? limpo : limpo[..300] + "…";
    }

    private static string? InferirCategoria(string? assunto)
    {
        if (string.IsNullOrWhiteSpace(assunto)) return null;
        var a = assunto.ToLowerInvariant();
        if (a.Contains("suporte") || a.Contains("problema") || a.Contains("erro") || a.Contains("bug")) return "Suporte";
        if (a.Contains("orçamento") || a.Contains("proposta") || a.Contains("cotação") || a.Contains("comercial")) return "Comercial";
        if (a.Contains("financeiro") || a.Contains("boleto") || a.Contains("pagamento") || a.Contains("fatura")) return "Financeiro";
        if (a.Contains("dúvida") || a.Contains("duvida") || a.Contains("pergunta") || a.Contains("como")) return "Dúvida";
        return null;
    }
}

file static class StringExtensions
{
    internal static string TruncatarEm(this string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}
