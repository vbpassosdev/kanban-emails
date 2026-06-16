using KanbanEmails.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KanbanEmails.Infrastructure.Data.Configurations;

public class EmailAnexoConfiguration : IEntityTypeConfiguration<EmailAnexo>
{
    public void Configure(EntityTypeBuilder<EmailAnexo> builder)
    {
        builder.ToTable("EmailAnexo");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.NomeArquivo).IsRequired().HasMaxLength(500);
        builder.Property(e => e.MimeType).HasMaxLength(200);
        builder.Property(e => e.CaminhoArquivo).IsRequired().HasMaxLength(1000);

        builder.HasIndex(e => e.EmailKanbanId);
    }
}
