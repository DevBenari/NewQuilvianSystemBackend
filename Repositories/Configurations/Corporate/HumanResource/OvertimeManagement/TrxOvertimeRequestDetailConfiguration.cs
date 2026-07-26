using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.OvertimeManagement
{
    public class TrxOvertimeRequestDetailConfiguration : IEntityTypeConfiguration<TrxOvertimeRequestDetail>
    {
        public void Configure(EntityTypeBuilder<TrxOvertimeRequestDetail> entity)
        {
            entity.ToTable("TrxOvertimeRequestDetail", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OvertimeDate).HasColumnType("date");
            entity.Property(x => x.PlannedStartAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.PlannedEndAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ApprovedStartAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovedEndAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DayType).HasMaxLength(30).HasDefaultValue("Workday").IsRequired();
            entity.Property(x => x.OvertimeCategory).HasMaxLength(40).HasDefaultValue("AfterShift").IsRequired();
            entity.Property(x => x.RateCodeSnapshot).HasMaxLength(50);
            entity.Property(x => x.RateMultiplierSnapshot).HasPrecision(10, 4);
            entity.Property(x => x.BaseHourlyRateSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.EstimatedCost).HasPrecision(18, 2);
            entity.Property(x => x.ApprovedCost).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(10).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.WorkDescription).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.DetailStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.OvertimeRequest).WithMany(x => x.Details).HasForeignKey(x => x.OvertimeRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkSchedule).WithMany().HasForeignKey(x => x.WorkScheduleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ShiftAssignment).WithMany().HasForeignKey(x => x.ShiftAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Attendance).WithMany().HasForeignKey(x => x.AttendanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AttendanceDaily).WithMany().HasForeignKey(x => x.AttendanceDailyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OvertimeRate).WithMany().HasForeignKey(x => x.OvertimeRateId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.OvertimeRequestId, x.SequenceNumber }).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.OvertimeDate, x.ShiftId, x.DetailStatus, x.IsDelete });
        }
    }
}
