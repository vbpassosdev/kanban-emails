using KanbanEmails.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KanbanEmails.Infrastructure.Data.Configurations;

public class ConfiguracaoEmailConfiguration : IEntityTypeConfiguration<ConfiguracaoEmail>
{
    public void Configure(EntityTypeBuilder<ConfiguracaoEmail> builder)
    {
        builder.ToTable("ConfiguracaoEmail");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Host).IsRequired().HasMaxLength(255);
        builder.Property(c => c.Porta).IsRequired();
        builder.Property(c => c.UsarSsl).IsRequired();
        builder.Property(c => c.EmailUsuario).IsRequired().HasMaxLength(255);
        builder.Property(c => c.SenhaCriptografada).IsRequired().HasMaxLength(1000);
        builder.Property(c => c.Pasta).IsRequired().HasMaxLength(100);
        builder.Property(c => c.IntervaloMinutos).IsRequired();

        builder.HasIndex(c => c.UsuarioId).IsUnique();
    }
}
