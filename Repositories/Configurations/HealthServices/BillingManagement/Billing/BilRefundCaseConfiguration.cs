using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilRefundCaseConfiguration : IEntityTypeConfiguration<BilRefundCase>
{
    public void Configure(EntityTypeBuilder<BilRefundCase> entity)
    {
        entity.ToTable("BilRefundCase", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_BilRefundCase_Status",
                "\"Status\" IN ('SUBMITTED','APPROVED','REJECTED','PARTIALLY_EXECUTED','EXECUTED')");
            table.HasCheckConstraint(
                "CK_BilRefundCase_RequestedAmount",
                "\"RequestedAmount\" > 0");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        entity.Property(x => x.PayloadHash).HasMaxLength(64).IsRequired();
        entity.Property(x => x.RequestedAmount).HasPrecision(18, 2);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();
        entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);
        entity.HasIndex(x => x.IdempotencyKey).IsUnique();
        entity.HasIndex(x => x.CorrelationId).IsUnique();
        entity.HasIndex(x => new { x.RefundableCreditId, x.Status });
        entity.HasOne(x => x.Invoice).WithMany().HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.RefundableCredit).WithMany().HasForeignKey(x => x.RefundableCreditId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasMany(x => x.Lines).WithOne(x => x.RefundCase)
            .HasForeignKey(x => x.RefundCaseId).OnDelete(DeleteBehavior.Restrict);
    }
}
