using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilArHandoffConfiguration : IEntityTypeConfiguration<BilArHandoff>
{
    public void Configure(EntityTypeBuilder<BilArHandoff> entity)
    {
        entity.ToTable("BilArHandoff", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_BilArHandoff_DebtorType",
                "\"DebtorType\" IN ('PATIENT_GUARANTOR','PAYER')");
            table.HasCheckConstraint(
                "CK_BilArHandoff_Status",
                "\"Status\" IN ('CREATED','ACKNOWLEDGED')");
            table.HasCheckConstraint("CK_BilArHandoff_Amount", "\"Amount\" > 0");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.DebtorType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();
        entity.Property(x => x.DueDate).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.AcknowledgedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);
        entity.HasIndex(x => x.HandoffKey).IsUnique();
        entity.HasIndex(x => x.CorrelationId).IsUnique();
        entity.HasIndex(x => new { x.InvoiceId, x.DebtorType }).IsUnique();
        entity.HasOne(x => x.Invoice).WithMany().HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.FinalizationRecord).WithMany().HasForeignKey(x => x.FinalizationRecordId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
