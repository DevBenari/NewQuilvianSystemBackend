using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilApHandoffConfiguration : IEntityTypeConfiguration<BilApHandoff>
{
    public void Configure(EntityTypeBuilder<BilApHandoff> entity)
    {
        entity.ToTable("BilApHandoff", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_BilApHandoff_ReadinessStatus",
                "\"ReadinessStatus\" IN ('NOT_READY','READY')");
            table.HasCheckConstraint(
                "CK_BilApHandoff_Status",
                "\"Status\" IN ('CREATED','ACKNOWLEDGED')");
            table.HasCheckConstraint("CK_BilApHandoff_Amount", "\"Amount\" > 0");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ReadinessStatus).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();
        entity.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ReadyAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.AcknowledgedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);
        entity.HasIndex(x => x.HandoffKey).IsUnique();
        entity.HasIndex(x => x.CorrelationId).IsUnique();
        entity.HasIndex(x => new { x.InvoiceId, x.DoctorId }).IsUnique();
        entity.HasOne(x => x.Invoice).WithMany().HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.FinalizationRecord).WithMany().HasForeignKey(x => x.FinalizationRecordId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
