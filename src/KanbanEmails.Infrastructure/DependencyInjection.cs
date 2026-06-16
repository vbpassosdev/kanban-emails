using KanbanEmails.Application.Interfaces;
using KanbanEmails.Domain.Interfaces;
using KanbanEmails.Infrastructure.Data;
using KanbanEmails.Infrastructure.Email;
using KanbanEmails.Infrastructure.HtmlSanitizer;
using KanbanEmails.Infrastructure.Repositories;
using KanbanEmails.Infrastructure.Security;
using KanbanEmails.Infrastructure.Services;
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
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        services.Configure<EmailImapSettings>(configuration.GetSection(EmailImapSettings.SectionName));
        services.AddScoped<IImapEmailReader, ImapEmailReader>();
        services.AddScoped<IStorageService, LocalStorageService>();
        services.AddSingleton<IHtmlSanitizerService, HtmlSanitizerService>();

        var chaveAes = configuration["Security:ChaveAes"]
            ?? throw new InvalidOperationException("Security:ChaveAes não configurado.");
        services.AddSingleton(new CriptografiaService(chaveAes));

        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
