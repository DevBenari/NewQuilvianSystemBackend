using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Collection;

public sealed class FinReceiptConfiguration : IEntityTypeConfiguration<FinReceipt>
{
    public void Configure(EntityTypeBuilder<FinReceipt> entity)
    {
        entity.ToTable("FinReceipt", "public", table =>
        {
            table.HasCheckConstraint("CK_FinReceipt_SourceType", "\"SourceType\" IN ('BILLING_TENDER','AR_COLLECTION','MANUAL')");
            table.HasCheckConstraint("CK_FinReceipt_Status", "\"Status\" IN ('RECEIVED','ALLOCATED','RECONCILED','REVERSED')");
            table.HasCheckConstraint("CK_FinReceipt_Amount", "\"Amount\" > 0");
            table.HasCheckConstraint("CK_FinReceipt_Unallocated", "\"UnallocatedAmount\" >= 0");
            table.HasCheckConstraint("CK_FinReceipt_AllocationBalance", "\"Amount\" = \"AllocatedAmount\" + \"UnallocatedAmount\"");
            // Tender wajib ada untuk penerimaan yang berasal dari Billing (FIN-DES-010) — KECUALI
            // baris pembalik (ReversalOfReceiptId terisi), yang sengaja mengosongkan SourceTenderId
            // supaya identitas idempotensi tender asli (IX_FinReceipt_SourceTenderId) tidak pernah
            // dipakai ulang, persis seperti didokumentasikan FinReceipt.cs sejak awal. Diperbaiki
            // 23 September 2026 — versi sebelumnya (tanpa klausa ReversalOfReceiptId) bertentangan
            // dengan desain itu dan memblokir pembalikan tender sepenuhnya (BE-FIN-016 bagian 1.5).
            table.HasCheckConstraint("CK_FinReceipt_TenderRequired", "\"SourceType\" <> 'BILLING_TENDER' OR \"SourceTenderId\" IS NOT NULL OR \"ReversalOfReceiptId\" IS NOT NULL");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ReceiptNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.SourceType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.AllocatedAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.UnallocatedAmount).HasPrecision(18, 2);
        entity.Property(x => x.KwitansiNumber).HasMaxLength(50);
        entity.Property(x => x.ProviderReference).HasMaxLength(150);
        entity.Property(x => x.ProviderEventId).HasMaxLength(100);
        entity.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.SourceInvoiceStatus).HasMaxLength(30);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(FinReceiptStatuses.Received);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        // Baris pembalik sengaja tidak diberi FK nav — self-reference lewat Id murni, konsisten
        // dengan pola pembalikan lain di modul ini (bukan hubungan agregat berlapis).
        entity.HasOne<FinReceipt>().WithMany().HasForeignKey(x => x.ReversalOfReceiptId).OnDelete(DeleteBehavior.Restrict);

        // Satu tender berhasil = paling banyak satu penerimaan (FIN-DES-010).
        entity.HasIndex(x => x.SourceTenderId).IsUnique()
            .HasFilter("\"SourceTenderId\" IS NOT NULL AND \"IsDelete\" = false")
            .HasDatabaseName("IX_FinReceipt_SourceTenderId");
        entity.HasIndex(x => x.ReceiptNumber).IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_FinReceipt_ReceiptNumber");
        entity.HasIndex(x => new { x.CashierShiftId, x.OccurredAt }).HasDatabaseName("IX_FinReceipt_CashierShift_OccurredAt");
    }
}
