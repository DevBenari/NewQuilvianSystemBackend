using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilRefundLineConfiguration : IEntityTypeConfiguration<BilRefundLine>
{
    public void Configure(EntityTypeBuilder<BilRefundLine> entity)
    {
        entity.ToTable("BilRefundLine", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_BilRefundLine_Status",
                "\"Status\" IN ('PENDING','SUCCEEDED','FAILED')");
            table.HasCheckConstraint(
                "CK_BilRefundLine_Amount",
                "\"Amount\" > 0");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
        entity.Property(x => x.ProviderReference).HasMaxLength(150);
        entity.Property(x => x.ProviderStatusCode).HasMaxLength(50);
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.AttemptedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.SettledAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);
        entity.HasIndex(x => new { x.RefundCaseId, x.OriginalTenderId }).IsUnique();
        entity.HasOne(x => x.RefundCase).WithMany(x => x.Lines).HasForeignKey(x => x.RefundCaseId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.OriginalTender).WithMany().HasForeignKey(x => x.OriginalTenderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
