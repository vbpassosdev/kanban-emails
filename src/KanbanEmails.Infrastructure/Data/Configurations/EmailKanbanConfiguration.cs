using KanbanEmails.Domain.Entities;
using KanbanEmails.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KanbanEmails.Infrastructure.Data.Configurations;

public class EmailKanbanConfiguration : IEntityTypeConfiguration<EmailKanban>
{
    public void Configure(EntityTypeBuilder<EmailKanban> builder)
    {
        builder.ToTable("EmailKanban");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.MessageId).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Remetente).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Assunto).HasMaxLength(500);
        builder.Property(e => e.CorpoTexto).HasColumnType("varchar(max)");
        builder.Property(e => e.CorpoHtml).HasColumnType("varchar(max)");
        builder.Property(e => e.Resumo).HasColumnType("varchar(max)");
        builder.Property(e => e.Categoria).HasMaxLength(100);
        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<StatusKanban>(v));

        builder.HasIndex(e => e.MessageId).IsUnique();
        builder.HasIndex(e => e.DataRecebimento);

        builder.HasMany(e => e.Anexos)
            .WithOne(a => a.Email)
            .HasForeignKey(a => a.EmailKanbanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Historico)
            .WithOne(h => h.Email)
            .HasForeignKey(h => h.EmailKanbanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
