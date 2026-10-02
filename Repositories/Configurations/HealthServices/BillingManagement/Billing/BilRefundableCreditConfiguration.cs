using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilRefundableCreditConfiguration : IEntityTypeConfiguration<BilRefundableCredit>
{
    public void Configure(EntityTypeBuilder<BilRefundableCredit> entity)
    {
        entity.ToTable("BilRefundableCredit", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_BilRefundableCredit_Amounts",
                "\"OriginalAmount\" > 0 AND \"AvailableAmount\" >= 0 AND \"AvailableAmount\" <= \"OriginalAmount\"");
            table.HasCheckConstraint(
                "CK_BilRefundableCredit_SourceType",
                "\"SourceType\" IN ('ALLOCATION_EXCESS','SETTLEMENT')");
            table.HasCheckConstraint(
                "CK_BilRefundableCredit_Status",
                "\"Status\" IN ('AVAILABLE','EXHAUSTED')");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.SourceType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
        entity.Property(x => x.OriginalAmount).HasPrecision(18, 2);
        entity.Property(x => x.AvailableAmount).HasPrecision(18, 2);
        entity.Property(x => x.RecognizedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);
        entity.HasIndex(x => new { x.SourceType, x.SourceId }).IsUnique();
        entity.HasIndex(x => new { x.InvoiceId, x.Status });
        entity.HasOne<BilInvoice>().WithMany().HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
