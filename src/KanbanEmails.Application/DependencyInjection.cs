using KanbanEmails.Application.Interfaces;
using KanbanEmails.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KanbanEmails.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEmailKanbanService, EmailKanbanService>();
        services.AddScoped<IRemetenteMonitoradoService, RemetenteMonitoradoService>();
        services.AddScoped<IEmailProcessorService, EmailProcessorService>();
        return services;
    }
}
