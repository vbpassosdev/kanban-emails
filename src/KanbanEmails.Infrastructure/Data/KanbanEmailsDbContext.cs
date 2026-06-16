using KanbanEmails.Domain.Entities;
using KanbanEmails.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace KanbanEmails.Infrastructure.Data;

public class KanbanEmailsDbContext(DbContextOptions<KanbanEmailsDbContext> options) : DbContext(options)
{
    public DbSet<EmailKanban> EmailsKanban => Set<EmailKanban>();
    public DbSet<EmailAnexo> EmailAnexos => Set<EmailAnexo>();
    public DbSet<EmailKanbanHistorico> EmailKanbanHistoricos => Set<EmailKanbanHistorico>();
    public DbSet<EmailRemetenteMonitorado> EmailRemetentesMonitorados => Set<EmailRemetenteMonitorado>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<ConfiguracaoEmail> ConfiguracoesEmail => Set<ConfiguracaoEmail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EmailKanbanConfiguration());
        modelBuilder.ApplyConfiguration(new EmailAnexoConfiguration());
        modelBuilder.ApplyConfiguration(new EmailKanbanHistoricoConfiguration());
        modelBuilder.ApplyConfiguration(new EmailRemetenteMonitoradoConfiguration());
        modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
        modelBuilder.ApplyConfiguration(new ConfiguracaoEmailConfiguration());
    }
}
