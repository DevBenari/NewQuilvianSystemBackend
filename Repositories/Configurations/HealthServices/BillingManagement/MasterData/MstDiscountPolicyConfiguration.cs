using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Configurations;

public sealed class MstDiscountPolicyConfiguration : IEntityTypeConfiguration<MstDiscountPolicy>
{
    public void Configure(EntityTypeBuilder<MstDiscountPolicy> entity)
    {
        entity.ToTable("MstDiscountPolicy", "public", table =>
        {
            table.HasCheckConstraint("CK_MstDiscountPolicy_EffectivePeriod", "\"EffectiveTo\" IS NULL OR \"EffectiveTo\" > \"EffectiveFrom\"");
            table.HasCheckConstraint("CK_MstDiscountPolicy_Value", "\"Value\" > 0 AND (\"ValueType\" <> 'PERCENTAGE' OR \"Value\" <= 100)");
            table.HasCheckConstraint("CK_MstDiscountPolicy_ValueType", "\"ValueType\" IN ('PERCENTAGE','FIXED_AMOUNT')");
            table.HasCheckConstraint("CK_MstDiscountPolicy_Limit", "\"Limit\" IS NULL OR \"Limit\" > 0");
            table.HasCheckConstraint("CK_MstDiscountPolicy_TypeTarget", "(\"DiscountType\" = 'PROMO_TOTAL' AND \"TargetComponent\" = 'PATIENT_PORTION') OR (\"DiscountType\" = 'PROMO_ITEM' AND \"TargetComponent\" = 'INVOICE_ITEM') OR (\"DiscountType\" = 'DOCTOR' AND \"TargetComponent\" = 'DOCTOR_SHARE')");
            table.HasCheckConstraint("CK_MstDiscountPolicy_Approval", "(\"DiscountType\" IN ('PROMO_TOTAL','PROMO_ITEM') AND \"RequiresApproval\" = false AND \"ApproverRole\" IS NULL) OR (\"DiscountType\" = 'DOCTOR' AND \"RequiresApproval\" = true AND \"ApproverRole\" = 'DOCTOR')");
        });

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
        entity.Property(x => x.DiscountType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.TargetComponent).HasMaxLength(30).IsRequired();
        entity.Property(x => x.ValueType).HasMaxLength(20).IsRequired();
        entity.Property(x => x.Value).HasPrecision(18, 2);
        entity.Property(x => x.Limit).HasPrecision(18, 2);
        entity.Property(x => x.ApproverRole).HasMaxLength(50);
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
        entity.HasIndex(x => new { x.DiscountType, x.TargetComponent, x.EffectiveFrom, x.EffectiveTo, x.IsActive, x.IsDelete });
    }
}
