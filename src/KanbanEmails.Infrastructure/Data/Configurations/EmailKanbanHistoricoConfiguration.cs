using KanbanEmails.Domain.Entities;
using KanbanEmails.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KanbanEmails.Infrastructure.Data.Configurations;

public class EmailKanbanHistoricoConfiguration : IEntityTypeConfiguration<EmailKanbanHistorico>
{
    public void Configure(EntityTypeBuilder<EmailKanbanHistorico> builder)
    {
        builder.ToTable("EmailKanbanHistorico");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.StatusAnterior)
            .HasMaxLength(50)
            .HasConversion(
                v => v.HasValue ? v.Value.ToString() : null,
                v => v != null ? Enum.Parse<StatusKanban>(v) : (StatusKanban?)null);

        builder.Property(e => e.StatusNovo)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<StatusKanban>(v));

        builder.Property(e => e.Observacao).HasMaxLength(1000);
        builder.Property(e => e.Usuario).HasMaxLength(150);

        builder.HasIndex(e => e.EmailKanbanId);
    }
}
