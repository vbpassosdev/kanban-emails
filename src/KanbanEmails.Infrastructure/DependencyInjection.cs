using KanbanEmails.Application.Interfaces;
using KanbanEmails.Domain.Interfaces;
using KanbanEmails.Infrastructure.Data;
using KanbanEmails.Infrastructure.Email;
using KanbanEmails.Infrastructure.HtmlSanitizer;
using KanbanEmails.Infrastructure.Repositories;
using KanbanEmails.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KanbanEmails.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<KanbanEmailsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IEmailKanbanRepository, EmailKanbanRepository>();
        services.AddScoped<IEmailAnexoRepository, EmailAnexoRepository>();
        services.AddScoped<IEmailRemetenteMonitoradoRepository, EmailRemetenteMonitoradoRepository>();

        services.Configure<EmailImapSettings>(configuration.GetSection(EmailImapSettings.SectionName));
        services.AddScoped<IImapEmailReader, ImapEmailReader>();
        services.AddScoped<IStorageService, LocalStorageService>();
        services.AddSingleton<IHtmlSanitizerService, HtmlSanitizerService>();

        return services;
    }
}
