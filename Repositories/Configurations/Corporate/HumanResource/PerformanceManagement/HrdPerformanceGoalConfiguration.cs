using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class HrdPerformanceGoalConfiguration : IEntityTypeConfiguration<HrdPerformanceGoal>
    {
        public void Configure(EntityTypeBuilder<HrdPerformanceGoal> entity)
        {
            entity.ToTable("HrdPerformanceGoal", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.GoalDescription).HasMaxLength(250).IsRequired();
            entity.Property(x => x.WeightPercentage).HasColumnType("decimal(5,2)").IsRequired();
            entity.Property(x => x.TargetValue).HasColumnType("decimal(15,2)").IsRequired();
            entity.Property(x => x.ActualValue).HasColumnType("decimal(15,2)");
            entity.Property(x => x.AchievementPercentage).HasColumnType("decimal(5,2)");
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue("Draft");
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");

            entity.HasOne(x => x.PerformanceCycle)
                .WithMany(c => c.PerformanceGoals)
                .HasForeignKey(x => x.PerformanceCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.KpiMetricCatalog)
                .WithMany()
                .HasForeignKey(x => x.KpiMetricCatalogId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApproverProfile)
                .WithMany()
                .HasForeignKey(x => x.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PerformanceCycleId);
            entity.HasIndex(x => x.WorkforceProfileId);
            entity.HasIndex(x => x.KpiMetricCatalogId);
            entity.HasIndex(x => x.Status);

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<HrdPerformanceGoal> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
