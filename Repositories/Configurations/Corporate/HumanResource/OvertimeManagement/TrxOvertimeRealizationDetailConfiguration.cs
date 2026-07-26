using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.OvertimeManagement
{
    public class TrxOvertimeRealizationDetailConfiguration : IEntityTypeConfiguration<TrxOvertimeRealizationDetail>
    {
        public void Configure(EntityTypeBuilder<TrxOvertimeRealizationDetail> entity)
        {
            entity.ToTable("TrxOvertimeRealizationDetail", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OvertimeDate).HasColumnType("date");
            entity.Property(x => x.AttendanceCheckInAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.AttendanceCheckOutAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ActualStartAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ActualEndAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DayType).HasMaxLength(30).HasDefaultValue("Workday").IsRequired();
            entity.Property(x => x.RateBandSnapshot).HasMaxLength(50);
            entity.Property(x => x.CalculationMethodSnapshot).HasMaxLength(50);
            entity.Property(x => x.RateMultiplierSnapshot).HasPrecision(10, 4);
            entity.Property(x => x.FixedAmountSnapshot).HasPrecision(18, 2).IsRequired(false);
            entity.Property(x => x.BaseHourlyRateSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.CalculatedAmount).HasPrecision(18, 2);
            entity.Property(x => x.VerifiedAmount).HasPrecision(18, 2);
            entity.Property(x => x.EvidenceFilePath).HasMaxLength(500);
            entity.Property(x => x.EvidenceFileName).HasMaxLength(255);
            entity.Property(x => x.EvidenceContentType).HasMaxLength(150);
            entity.Property(x => x.EvidenceChecksum).HasMaxLength(128);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.DetailStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.OvertimeRealization).WithMany(x => x.Details).HasForeignKey(x => x.OvertimeRealizationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OvertimeRequestDetail).WithMany(x => x.RealizationDetails).HasForeignKey(x => x.OvertimeRequestDetailId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ShiftAssignment).WithMany().HasForeignKey(x => x.ShiftAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Attendance).WithMany().HasForeignKey(x => x.AttendanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AttendanceDaily).WithMany().HasForeignKey(x => x.AttendanceDailyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OvertimeRate).WithMany().HasForeignKey(x => x.OvertimeRateId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.OvertimeRealizationId, x.SequenceNumber }).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.OvertimeDate, x.AttendanceDailyId, x.DetailStatus, x.IsDelete });
            entity.HasIndex(x => x.EvidenceChecksum);
        }
    }
}
