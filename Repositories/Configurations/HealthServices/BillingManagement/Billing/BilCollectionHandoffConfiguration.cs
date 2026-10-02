using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilCollectionHandoffConfiguration : IEntityTypeConfiguration<BilCollectionHandoff>
{
    public void Configure(EntityTypeBuilder<BilCollectionHandoff> entity)
    {
        entity.ToTable("BilCollectionHandoff", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_BilCollectionHandoff_TenderStatus",
                "\"TenderStatus\" IN ('SUCCEEDED','REVERSED')");
            table.HasCheckConstraint(
                "CK_BilCollectionHandoff_Status",
                "\"Status\" IN ('CREATED','ACKNOWLEDGED')");
            table.HasCheckConstraint("CK_BilCollectionHandoff_Amount", "\"Amount\" > 0");
        });

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.PaymentAllocationIds).HasMaxLength(1000);
        entity.Property(x => x.KwitansiNumber).HasMaxLength(50);
        entity.Property(x => x.ProviderReference).HasMaxLength(150);
        entity.Property(x => x.ProviderEventId).HasMaxLength(100);
        entity.Property(x => x.SourceInvoiceStatus).HasMaxLength(30).IsRequired();
        entity.Property(x => x.TenderStatus).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(BillingHandoffStatuses.Created);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();
        entity.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.AcknowledgedAt).HasColumnType("timestamp with time zone");

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => new { x.TenderId, x.TenderStatus })
            .IsUnique()
            .HasDatabaseName("IX_BilCollectionHandoff_Tender_Status");
        entity.HasIndex(x => x.HandoffKey)
            .IsUnique()
            .HasDatabaseName("IX_BilCollectionHandoff_HandoffKey");
        entity.HasIndex(x => x.Status)
            .HasDatabaseName("IX_BilCollectionHandoff_Status");
        entity.HasIndex(x => x.InvoiceId)
            .HasDatabaseName("IX_BilCollectionHandoff_Invoice");
        entity.HasIndex(x => x.SettlementId);
        entity.HasIndex(x => x.OccurredAt);
        entity.HasIndex(x => x.CashierShiftId);

        entity.HasOne(x => x.Tender).WithMany().HasForeignKey(x => x.TenderId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Settlement).WithMany().HasForeignKey(x => x.SettlementId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Invoice).WithMany().HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.PaymentMethod).WithMany().HasForeignKey(x => x.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.CashierShift).WithMany().HasForeignKey(x => x.CashierShiftId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
