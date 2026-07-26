using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.LeaveAndOvertime
{
    public class MstOvertimeRateConfiguration : IEntityTypeConfiguration<MstOvertimeRate>
    {
        public void Configure(EntityTypeBuilder<MstOvertimeRate> entity)
        {
            entity.ToTable("MstOvertimeRate", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OvertimePolicyId).IsRequired();
            entity.Property(x => x.OvertimeRateCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.OvertimeRateName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.DayType).HasMaxLength(50).HasDefaultValue("Workday").IsRequired();
            entity.Property(x => x.TimeBand).HasMaxLength(50).HasDefaultValue("AllDay").IsRequired();
            entity.Property(x => x.CalculationMethod).HasMaxLength(50).HasDefaultValue("Multiplier").IsRequired();
            entity.Property(x => x.RateMultiplier).HasPrecision(8, 4).HasDefaultValue(1m);
            entity.Property(x => x.FixedAmount).HasPrecision(18, 2).IsRequired(false);
            entity.Property(x => x.StartMinute).HasDefaultValue(0);
            entity.Property(x => x.StartTime).HasColumnType("time without time zone").IsRequired(false);
            entity.Property(x => x.EndTime).HasColumnType("time without time zone").IsRequired(false);
            entity.Property(x => x.MinimumEligibleMinutes).HasDefaultValue(0);
            entity.Property(x => x.Priority).HasDefaultValue(0);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.OvertimePolicy)
                .WithMany(x => x.OvertimeRates)
                .HasForeignKey(x => x.OvertimePolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.OvertimeRateCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.OvertimeRateName);
            entity.HasIndex(x => x.OvertimePolicyId);
            entity.HasIndex(x => new { x.OvertimePolicyId, x.DayType, x.TimeBand, x.Priority, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields<T>(EntityTypeBuilder<T> entity)
            where T : QuilvianSystemBackend.Models.IdentityModel
        {
            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);
        }
    }
}
