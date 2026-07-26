using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class TrxCalibrationSessionConfiguration : IEntityTypeConfiguration<TrxCalibrationSession>
    {
        public void Configure(EntityTypeBuilder<TrxCalibrationSession> entity)
        {
            entity.ToTable("TrxCalibrationSession", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ScheduledStartAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ScheduledEndAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.FinalizedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ParticipantSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.CalibrationDecisionJson).HasColumnType("jsonb");
            entity.Property(x => x.ParticipantCount).HasDefaultValue(0);
            entity.Property(x => x.EmployeeCount).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.PerformanceCycle)
                .WithMany(x => x.CalibrationSessions)
                .HasForeignKey(x => x.PerformanceCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.FacilitatorUser)
                .WithMany()
                .HasForeignKey(x => x.FacilitatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.FinalizedByUser)
                .WithMany()
                .HasForeignKey(x => x.FinalizedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.CalibrationCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.PerformanceCycleId, x.DepartmentId, x.ScheduledStartAt });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxCalibrationSession> entity)
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
