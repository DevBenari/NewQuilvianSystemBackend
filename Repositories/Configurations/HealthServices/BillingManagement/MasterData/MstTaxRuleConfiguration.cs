using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Configurations;

public sealed class MstTaxRuleConfiguration : IEntityTypeConfiguration<MstTaxRule>
{
    public void Configure(EntityTypeBuilder<MstTaxRule> entity)
    {
        entity.ToTable("MstTaxRule", "public", table =>
        {
            table.HasCheckConstraint("CK_MstTaxRule_EffectivePeriod", "\"EffectiveTo\" IS NULL OR \"EffectiveTo\" > \"EffectiveFrom\"");
            table.HasCheckConstraint("CK_MstTaxRule_Rate", "\"Rate\" > 0 AND \"Rate\" <= 100");
            table.HasCheckConstraint("CK_MstTaxRule_RoundingMode", "\"RoundingMode\" IN ('HALF_UP','HALF_EVEN','UP','DOWN')");
            table.HasCheckConstraint("CK_MstTaxRule_AllocationRule", "\"AllocationRule\" IN ('PROPORTIONAL','PATIENT','GUARANTOR')");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
        entity.Property(x => x.TaxableCategory).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Rate).HasPrecision(18, 6);
        entity.Property(x => x.RoundingMode).HasMaxLength(30).IsRequired();
        entity.Property(x => x.AllocationRule).HasMaxLength(50).IsRequired();
        entity.Property(x => x.EffectiveFrom).HasColumnType("timestamp with time zone");
        entity.Property(x => x.EffectiveTo).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsActive).HasDefaultValue(false);
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);
        entity.HasIndex(x => x.Code).IsUnique().HasFilter("\"IsDelete\" = false");
        entity.HasIndex(x => new { x.TaxableCategory, x.EffectiveFrom, x.EffectiveTo, x.IsActive, x.IsDelete });
    }
}
