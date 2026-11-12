using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class HrdPerformanceCalibrationSessionConfiguration : IEntityTypeConfiguration<HrdPerformanceCalibrationSession>
    {
        public void Configure(EntityTypeBuilder<HrdPerformanceCalibrationSession> entity)
        {
            entity.ToTable("HrdPerformanceCalibrationSession", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.SessionNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.SessionDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.GuidingDistributionJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.ActualDistributionJson).HasColumnType("jsonb");
            entity.Property(x => x.TotalEmployeesCalibrated).HasDefaultValue(0);
            entity.Property(x => x.TotalOverridesApplied).HasDefaultValue(0);
            entity.Property(x => x.MinutesOfMeetingUrl).HasMaxLength(250);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue("Scheduled");
            entity.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");

            entity.HasOne(x => x.PerformanceCycle)
                .WithMany(c => c.PerformanceCalibrationSessions)
                .HasForeignKey(x => x.PerformanceCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CompletedByProfile)
                .WithMany()
                .HasForeignKey(x => x.CompletedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.SessionNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.PerformanceCycleId);
            entity.HasIndex(x => x.Status);

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<HrdPerformanceCalibrationSession> entity)
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
