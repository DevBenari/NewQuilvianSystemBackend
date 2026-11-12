using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.AttendanceManagement
{
    public class HrdAttendancePeriodReopenLogConfiguration : IEntityTypeConfiguration<HrdAttendancePeriodReopenLog>
    {
        public void Configure(EntityTypeBuilder<HrdAttendancePeriodReopenLog> builder)
        {
            builder.ToTable("HrdAttendancePeriodReopenLog", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.ReopenedAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.ReopenReason).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.PreviousClosedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsRollbackPayrollExecuted).HasDefaultValue(false);
            builder.Property(x => x.RollbackPayrollNotes).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.AttendancePeriod)
                .WithMany()
                .HasForeignKey(x => x.AttendancePeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReopenedByUser)
                .WithMany()
                .HasForeignKey(x => x.ReopenedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PreviousClosedByUser)
                .WithMany()
                .HasForeignKey(x => x.PreviousClosedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.AttendancePeriodId);
        }
    }
}
