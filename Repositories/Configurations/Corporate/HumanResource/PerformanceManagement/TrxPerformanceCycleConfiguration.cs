using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class TrxPerformanceCycleConfiguration : IEntityTypeConfiguration<TrxPerformanceCycle>
    {
        public void Configure(EntityTypeBuilder<TrxPerformanceCycle> entity)
        {
            entity.ToTable("trx_performance_cycles", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CycleCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CycleTitle).HasMaxLength(150).IsRequired();
            entity.Property(x => x.CycleType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.StartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SelfAppraisalDeadline).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ManagerAppraisalDeadline).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CalibrationDeadline).HasColumnType("timestamp with time zone");
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue("Draft");

            entity.Property(x => x.PublishedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ClosedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CycleConfigurationJson).HasColumnType("jsonb");
            entity.Property(x => x.PopulationSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.AllowEmployeeViewFinalResult).HasDefaultValue(false);
            entity.Property(x => x.RequireSelfAssessment).HasDefaultValue(true);
            entity.Property(x => x.EnablePeerFeedback).HasDefaultValue(false);
            entity.Property(x => x.TotalEmployee).HasDefaultValue(0);
            entity.Property(x => x.CompletedEmployee).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasIndex(x => x.CycleCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.EvaluationYear, x.Status });
            entity.HasIndex(x => new { x.StartDate, x.EndDate });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxPerformanceCycle> entity)
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
