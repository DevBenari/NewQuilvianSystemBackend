using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BillingManagement.Billing;

/// <summary>
/// Konfigurasi tanda terima event Rawat Inap — kontrak <c>integrasi-billing</c> <c>1.1.0</c>
/// kamus data 6.7 dan DDL 6.8.
/// </summary>
public sealed class BilInpatientEventReceiptConfiguration : IEntityTypeConfiguration<BilInpatientEventReceipt>
{
    public void Configure(EntityTypeBuilder<BilInpatientEventReceipt> entity)
    {
        entity.ToTable("BilInpatientEventReceipt", "public");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.IdempotencyKey).HasMaxLength(255).IsRequired();
        entity.Property(x => x.EventType).HasMaxLength(100).IsRequired();
        entity.Property(x => x.SourceId).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Outcome).HasMaxLength(50).IsRequired();
        entity.Property(x => x.Message).HasMaxLength(500);

        // INV-RWF-06: satu kunci idempotensi hanya boleh berefek sekali.
        entity.HasIndex(x => x.IdempotencyKey)
            .IsUnique()
            .HasDatabaseName("IX_BilInpatientEventReceipt_IdempotencyKey");

        entity.HasIndex(x => x.EncounterId)
            .HasDatabaseName("IX_BilInpatientEventReceipt_EncounterId");

        entity.HasIndex(x => x.EpisodeId)
            .HasDatabaseName("IX_BilInpatientEventReceipt_EpisodeId");

        entity.HasOne(x => x.Invoice)
            .WithMany()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
