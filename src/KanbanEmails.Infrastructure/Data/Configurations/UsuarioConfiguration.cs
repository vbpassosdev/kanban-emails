using KanbanEmails.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KanbanEmails.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuario");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nome).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(255);
        builder.Property(u => u.SenhaHash).IsRequired().HasMaxLength(500);
        builder.Property(u => u.Ativo).IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasOne(u => u.ConfiguracaoEmail)
            .WithOne(c => c.Usuario)
            .HasForeignKey<ConfiguracaoEmail>(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
