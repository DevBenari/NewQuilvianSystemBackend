using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class HrdPerformanceAppraisalConfiguration : IEntityTypeConfiguration<HrdPerformanceAppraisal>
    {
        public void Configure(EntityTypeBuilder<HrdPerformanceAppraisal> entity)
        {
            entity.ToTable("HrdPerformanceAppraisal", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.KpiScore).HasColumnType("decimal(5,2)");
            entity.Property(x => x.BehaviorScore).HasColumnType("decimal(5,2)");
            entity.Property(x => x.PreCalibrationScore).HasColumnType("decimal(5,2)");
            entity.Property(x => x.FinalCompositeScore).HasColumnType("decimal(5,2)");
            entity.Property(x => x.PerformanceBand).HasMaxLength(20);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue("Draft");
            entity.Property(x => x.EmployeeAcknowledgedAt).HasColumnType("timestamp with time zone");

            entity.HasOne(x => x.PerformanceCycle)
                .WithMany(c => c.PerformanceAppraisals)
                .HasForeignKey(x => x.PerformanceCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.FirstLineManager)
                .WithMany()
                .HasForeignKey(x => x.FirstLineManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SecondLineManager)
                .WithMany()
                .HasForeignKey(x => x.SecondLineManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Composite Unique Index: Satu pegawai hanya memiliki tepat satu lembar evaluasi per siklus kinerja
            entity.HasIndex(x => new { x.PerformanceCycleId, x.WorkforceProfileId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.FirstLineManagerId);
            entity.HasIndex(x => x.SecondLineManagerId);
            entity.HasIndex(x => x.FinalCompositeScore);
            entity.HasIndex(x => x.PerformanceBand);
            entity.HasIndex(x => x.Status);

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<HrdPerformanceAppraisal> entity)
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
