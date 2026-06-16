using KanbanEmails.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KanbanEmails.Infrastructure.Data.Configurations;

public class EmailRemetenteMonitoradoConfiguration : IEntityTypeConfiguration<EmailRemetenteMonitorado>
{
    public void Configure(EntityTypeBuilder<EmailRemetenteMonitorado> builder)
    {
        builder.ToTable("EmailRemetenteMonitorado");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome).HasMaxLength(200);
        builder.Property(e => e.EmailOuDominio).IsRequired().HasMaxLength(255);

        builder.HasIndex(e => e.EmailOuDominio);
    }
}
