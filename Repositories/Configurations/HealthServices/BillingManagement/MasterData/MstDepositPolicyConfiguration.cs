using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Configurations;

public sealed class MstDepositPolicyConfiguration : IEntityTypeConfiguration<MstDepositPolicy>
{
    public void Configure(EntityTypeBuilder<MstDepositPolicy> entity)
    {
        entity.ToTable("MstDepositPolicy", "public", table =>
        {
            table.HasCheckConstraint("CK_MstDepositPolicy_MinimumAmount", "\"MinimumAmount\" >= 0");
            table.HasCheckConstraint("CK_MstDepositPolicy_EffectivePeriod", "\"EffectiveTo\" IS NULL OR \"EffectiveTo\" > \"EffectiveFrom\"");
            table.HasCheckConstraint("CK_MstDepositPolicy_FollowUpInterval", "\"FollowUpIntervalDays\" >= 0");
        });

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
        entity.Property(x => x.GuarantorId);
        entity.Property(x => x.PatientClassId);
        entity.Property(x => x.IsRequired).IsRequired().HasDefaultValue(true);
        entity.Property(x => x.MinimumAmount).HasPrecision(18, 2);
        entity.Property(x => x.FollowUpIntervalDays).HasDefaultValue(3);
        entity.Property(x => x.Description).HasMaxLength(500);
        entity.Property(x => x.EffectiveFrom).HasColumnType("timestamp with time zone");
        entity.Property(x => x.EffectiveTo).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsActive).HasDefaultValue(true);
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.Code).IsUnique().HasFilter("\"IsDelete\" = false");
        entity.HasIndex(x => new { x.GuarantorId, x.PatientClassId, x.EffectiveFrom, x.EffectiveTo, x.IsActive, x.IsDelete });
    }
}
