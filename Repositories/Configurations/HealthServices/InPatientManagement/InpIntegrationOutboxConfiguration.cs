using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    public class InpIntegrationOutboxConfiguration : IEntityTypeConfiguration<InpIntegrationOutbox>
    {
        public void Configure(EntityTypeBuilder<InpIntegrationOutbox> builder)
        {
            builder.ToTable("InpIntegrationOutboxes", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.IdempotencyKey)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.SourceDomain)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.SourceType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.SourceDetailId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.EventType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.PayloadJson)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<int>();

            // Unique index compound idempotency key
            builder.HasIndex(x => x.IdempotencyKey, "UQ_InpIntegrationOutbox_IdempotencyKey")
                .IsUnique();

            // Filtered index untuk worker background outbox
            builder.HasIndex(x => x.Status, "IX_InpIntegrationOutbox_Status_Pending")
                .HasFilter("\"Status\" IN (0, 3)");

            builder.HasIndex(x => x.SourceDomain);
            builder.HasIndex(x => x.SourceType);
            builder.HasIndex(x => x.SourceDetailId);
            builder.HasIndex(x => x.EventType);
            builder.HasIndex(x => x.NextRetryAtUtc);
            builder.HasIndex(x => x.CreatedAtUtc);

            // Kontrak integrasi-billing 1.1.0 kamus data 6.5 — sewa pemrosesan dan putar ulang.
            builder.HasIndex(x => new { x.Status, x.ProcessingStartedAtUtc },
                "IX_InpIntegrationOutbox_Status_ProcessingStartedAtUtc");
            builder.HasIndex(x => x.ReplayBatchId, "IX_InpIntegrationOutbox_ReplayBatchId");
        }
    }
}
