using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilHandoffAdjustmentConfiguration : IEntityTypeConfiguration<BilHandoffAdjustment>
{
    public void Configure(EntityTypeBuilder<BilHandoffAdjustment> entity)
    {
        entity.ToTable("BilHandoffAdjustment", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_BilHandoffAdjustment_Direction",
                "\"Direction\" IN ('DEBIT','CREDIT')");
            table.HasCheckConstraint("CK_BilHandoffAdjustment_Amount", "\"Amount\" > 0");
            table.HasCheckConstraint(
                "CK_BilHandoffAdjustment_ExactlyOneTarget",
                "((\"ArHandoffId\" IS NOT NULL AND \"ApHandoffId\" IS NULL) OR (\"ArHandoffId\" IS NULL AND \"ApHandoffId\" IS NOT NULL))");
            table.HasCheckConstraint(
                "CK_BilHandoffAdjustment_ExactlyOneSource",
                "NOT (\"SourceAdjustmentId\" IS NOT NULL AND \"SourceWriteOffCaseId\" IS NOT NULL)");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Direction).HasMaxLength(10).IsRequired();
        entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();
        entity.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);
        entity.HasIndex(x => x.CorrelationId).IsUnique();
        entity.HasIndex(x => x.SourceAdjustmentId).IsUnique().HasFilter("\"SourceAdjustmentId\" IS NOT NULL");
        entity.HasIndex(x => x.SourceWriteOffCaseId).IsUnique().HasFilter("\"SourceWriteOffCaseId\" IS NOT NULL");
        entity.HasOne(x => x.ArHandoff).WithMany().HasForeignKey(x => x.ArHandoffId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.ApHandoff).WithMany().HasForeignKey(x => x.ApHandoffId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
