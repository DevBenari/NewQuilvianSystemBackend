using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Performance
{
    public class MstPerformanceCycleConfiguration : IEntityTypeConfiguration<MstPerformanceCycle>
    {
        public void Configure(EntityTypeBuilder<MstPerformanceCycle> entity)
        {
            entity.ToTable("MstPerformanceCycle", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CycleCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CycleName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CycleType).HasMaxLength(50).HasDefaultValue("Annual").IsRequired();
            entity.Property(x => x.PeriodYear).IsRequired(false);
            entity.Property(x => x.PeriodStartDate).HasColumnType("date").IsRequired();
            entity.Property(x => x.PeriodEndDate).HasColumnType("date").IsRequired();
            entity.Property(x => x.GoalSettingStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.GoalSettingEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.MidReviewStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.MidReviewEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.FinalReviewStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.FinalReviewEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.CalibrationStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.CalibrationEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.CycleStatus).HasMaxLength(50).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.IsCurrent).HasDefaultValue(false);
            entity.Property(x => x.IsLocked).HasDefaultValue(false);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

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

            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.LegalEntity)
                .WithMany()
                .HasForeignKey(x => x.LegalEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.CycleCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.CycleName);
            entity.HasIndex(x => new { x.CycleType, x.CycleStatus, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.PeriodStartDate, x.PeriodEndDate });
            entity.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.PeriodYear });
            entity.HasIndex(x => new { x.IsCurrent, x.IsLocked, x.IsActive, x.IsDelete });
        }
    }
}
