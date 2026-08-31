using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilCalculationVersionConfiguration : IEntityTypeConfiguration<BilCalculationVersion>
{
    public void Configure(EntityTypeBuilder<BilCalculationVersion> entity)
    {
        entity.ToTable("BilCalculationVersion", "public", table =>
        {
            table.HasCheckConstraint("CK_BilCalculationVersion_Version", "\"VersionNo\" > 0");
            table.HasCheckConstraint("CK_BilCalculationVersion_Amounts", "\"GrossAmount\" >= 0 AND \"AdministrationFeeAmount\" >= 0 AND \"RoomChargeAmount\" >= 0 AND \"ItemDiscount\" >= 0 AND \"TotalDiscount\" >= 0 AND \"TaxAmount\" >= 0 AND \"PatientAmount\" >= 0 AND \"PrimaryAmount\" >= 0 AND \"ExcessAmount\" >= 0 AND \"UnresolvedCoverageAmount\" >= 0");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.GrossAmount).HasPrecision(18, 2);
        entity.Property(x => x.AdministrationFeeAmount).HasPrecision(18, 2);
        entity.Property(x => x.RoomChargeAmount).HasPrecision(18, 2);
        entity.Property(x => x.ItemDiscount).HasPrecision(18, 2);
        entity.Property(x => x.TotalDiscount).HasPrecision(18, 2);
        entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
        entity.Property(x => x.PatientAmount).HasPrecision(18, 2);
        entity.Property(x => x.PrimaryAmount).HasPrecision(18, 2);
        entity.Property(x => x.ExcessAmount).HasPrecision(18, 2);
        entity.Property(x => x.UnresolvedCoverageAmount).HasPrecision(18, 2);
        entity.Property(x => x.RoundingAmount).HasPrecision(18, 2);
        entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        entity.Property(x => x.BreakdownSnapshot).HasColumnType("jsonb").IsRequired();
        entity.Property(x => x.CalculatedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.HasIndex(x => new { x.InvoiceId, x.VersionNo }).IsUnique();
        entity.HasOne(x => x.Invoice).WithMany(x => x.CalculationVersions).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
    }
}
