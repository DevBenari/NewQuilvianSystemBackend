using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Configurations;

public sealed class MstRoomChargePolicyConfiguration : IEntityTypeConfiguration<MstRoomChargePolicy>
{
    public void Configure(EntityTypeBuilder<MstRoomChargePolicy> entity)
    {
        entity.ToTable("MstRoomChargePolicy", "public", table =>
        {
            table.HasCheckConstraint("CK_MstRoomChargePolicy_EffectivePeriod", "\"EffectiveTo\" IS NULL OR \"EffectiveTo\" > \"EffectiveFrom\"");
            table.HasCheckConstraint("CK_MstRoomChargePolicy_MinimumMinutes", "\"MinimumMinutes\" >= \"PeriodMinutes\"");
            table.HasCheckConstraint("CK_MstRoomChargePolicy_PeriodMinutes", "\"PeriodMinutes\" > 0");
            table.HasCheckConstraint("CK_MstRoomChargePolicy_RemainderRounding", "\"RemainderRounding\" IN ('CEILING_PERIOD','PROPORTIONAL','WHOLE_PERIODS')");
            table.HasCheckConstraint("CK_MstRoomChargePolicy_TariffMoment", "\"TariffMoment\" IN ('PERIOD_START','OCCUPANCY_START')");
            table.HasCheckConstraint("CK_MstRoomChargePolicy_LeaveRule", "\"LeaveRule\" IN ('INCLUDE_LEAVE','EXCLUDE_LEAVE')");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
        entity.Property(x => x.RemainderRounding).HasMaxLength(30).IsRequired();
        entity.Property(x => x.TariffMoment).HasMaxLength(30).IsRequired();
        entity.Property(x => x.LeaveRule).HasMaxLength(50).IsRequired();
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
        entity.HasIndex(x => new { x.EffectiveFrom, x.EffectiveTo, x.IsActive, x.IsDelete });
    }
}
