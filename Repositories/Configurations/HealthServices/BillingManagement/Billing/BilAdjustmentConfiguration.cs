using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilAdjustmentConfiguration : IEntityTypeConfiguration<BilAdjustment>
{
    public void Configure(EntityTypeBuilder<BilAdjustment> entity)
    {
        entity.ToTable("BilAdjustment", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_BilAdjustment_Direction",
                "\"Direction\" IN ('DEBIT','CREDIT')");
            table.HasCheckConstraint(
                "CK_BilAdjustment_Status",
                "\"Status\" IN ('SUBMITTED','POSTED','REJECTED')");
            table.HasCheckConstraint(
                "CK_BilAdjustment_Amount",
                "\"Amount\" > 0");
            table.HasCheckConstraint(
                "CK_BilAdjustment_ReversesOneSource",
                "NOT (\"ReversesAdjustmentId\" IS NOT NULL AND \"ReversesWriteOffCaseId\" IS NOT NULL)");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Direction).HasMaxLength(10).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        entity.Property(x => x.PayloadHash).HasMaxLength(64).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();
        entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.PostedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);
        entity.HasIndex(x => x.IdempotencyKey).IsUnique();
        entity.HasIndex(x => x.CorrelationId).IsUnique();
        entity.HasIndex(x => new { x.InvoiceId, x.Status });
        entity.HasIndex(x => x.ReversesAdjustmentId).IsUnique().HasFilter("\"ReversesAdjustmentId\" IS NOT NULL");
        entity.HasIndex(x => x.ReversesWriteOffCaseId).IsUnique().HasFilter("\"ReversesWriteOffCaseId\" IS NOT NULL");
        entity.HasOne(x => x.Invoice).WithMany().HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<BilAdjustment>().WithMany().HasForeignKey(x => x.ReversesAdjustmentId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<BilWriteOffCase>().WithMany().HasForeignKey(x => x.ReversesWriteOffCaseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
