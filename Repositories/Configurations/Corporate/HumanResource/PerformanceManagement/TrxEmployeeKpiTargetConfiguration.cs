using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class TrxEmployeeKpiTargetConfiguration : IEntityTypeConfiguration<TrxEmployeeKpiTarget>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeKpiTarget> entity)
        {
            entity.ToTable("TrxEmployeeKpiTarget", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Weight).HasPrecision(8, 4);
            entity.Property(x => x.TargetValue).HasPrecision(18, 4);
            entity.Property(x => x.ActualValue).HasPrecision(18, 4);
            entity.Property(x => x.AchievementPercentage).HasPrecision(8, 2);
            entity.Property(x => x.SelfScore).HasPrecision(18, 4);
            entity.Property(x => x.ManagerScore).HasPrecision(18, 4);
            entity.Property(x => x.FinalScore).HasPrecision(18, 4);
            entity.Property(x => x.EvidenceJson).HasColumnType("jsonb");
            entity.Property(x => x.Weight).HasDefaultValue(0m);
            entity.Property(x => x.AchievementPercentage).HasDefaultValue(0m);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.PerformanceCycle)
                .WithMany(x => x.KpiTargets)
                .HasForeignKey(x => x.PerformanceCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.EmployeeGoal)
                .WithMany(x => x.KpiTargets)
                .HasForeignKey(x => x.EmployeeGoalId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.KpiCatalog)
                .WithMany()
                .HasForeignKey(x => x.KpiCatalogId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.EmployeeGoalId, x.KpiCode })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.PerformanceCycleId, x.WorkforceProfileId, x.TargetStatus });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxEmployeeKpiTarget> entity)
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
