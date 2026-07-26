using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class TrxPerformanceCheckInConfiguration : IEntityTypeConfiguration<TrxPerformanceCheckIn>
    {
        public void Configure(EntityTypeBuilder<TrxPerformanceCheckIn> entity)
        {
            entity.ToTable("TrxPerformanceCheckIn", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CheckInDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.NextCheckInDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ProgressPercentage).HasPrecision(5, 2);
            entity.Property(x => x.ActionItemsJson).HasColumnType("jsonb");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.PerformanceCycle)
                .WithMany()
                .HasForeignKey(x => x.PerformanceCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ManagerUser)
                .WithMany()
                .HasForeignKey(x => x.ManagerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.EmployeeGoal)
                .WithMany(x => x.CheckIns)
                .HasForeignKey(x => x.EmployeeGoalId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.PerformanceCycleId, x.WorkforceProfileId, x.CheckInDate });

            entity.HasIndex(x => new { x.ManagerUserId, x.CheckInDate });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxPerformanceCheckIn> entity)
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
