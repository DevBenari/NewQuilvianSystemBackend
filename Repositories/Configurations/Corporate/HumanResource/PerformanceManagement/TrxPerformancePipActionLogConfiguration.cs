using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class TrxPerformancePipActionLogConfiguration : IEntityTypeConfiguration<TrxPerformancePipActionLog>
    {
        public void Configure(EntityTypeBuilder<TrxPerformancePipActionLog> entity)
        {
            entity.ToTable("trx_performance_pip_action_logs", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.LogDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ProgressSummary).IsRequired();
            entity.Property(x => x.MentorFeedback).IsRequired();
            entity.Property(x => x.IsTargetOnTrack).HasDefaultValue(true);

            entity.HasOne(x => x.PerformanceImprovementPlan)
                .WithMany(p => p.ActionLogs)
                .HasForeignKey(x => x.PipId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.LoggedByProfile)
                .WithMany()
                .HasForeignKey(x => x.LoggedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PipId);
            entity.HasIndex(x => new { x.PipId, x.WeekNumber });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxPerformancePipActionLog> entity)
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
