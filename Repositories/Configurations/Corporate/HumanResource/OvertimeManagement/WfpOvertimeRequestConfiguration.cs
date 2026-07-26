using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.OvertimeManagement
{
    public class WfpOvertimeRequestConfiguration : IEntityTypeConfiguration<WfpOvertimeRequest>
    {
        public void Configure(EntityTypeBuilder<WfpOvertimeRequest> entity)
        {
            entity.ToTable("WfpOvertimeRequest", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RequestNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.OvertimeDate).HasColumnType("date");
            entity.Property(x => x.PlannedEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.PlannedStartAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PlannedEndAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.RequestedStartTime).HasColumnType("time without time zone").IsRequired(false);
            entity.Property(x => x.RequestedEndTime).HasColumnType("time without time zone").IsRequired(false);
            entity.Property(x => x.EstimatedBaseHourlyRate).HasPrecision(18, 2);
            entity.Property(x => x.EstimatedOvertimeCost).HasPrecision(18, 2);
            entity.Property(x => x.ApprovedEstimatedCost).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(10).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.WorkDescription).HasMaxLength(2000);
            entity.Property(x => x.AttachmentPath).HasMaxLength(500);
            entity.Property(x => x.AttachmentFileName).HasMaxLength(255);
            entity.Property(x => x.AttachmentContentType).HasMaxLength(150);
            entity.Property(x => x.ValidationResultJson).HasColumnType("jsonb");
            entity.Property(x => x.OvertimeRequestStatus).HasMaxLength(40).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.StartedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.WaitingRealizationAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.RealizedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovalNotes).HasMaxLength(2000);
            entity.Property(x => x.PayrollProcessedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ProcessedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationAssignment).WithMany().HasForeignKey(x => x.OrganizationAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CostCenter).WithMany().HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OvertimePolicy).WithMany().HasForeignKey(x => x.OvertimePolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkScheduleAssignment).WithMany().HasForeignKey(x => x.WorkScheduleAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RosterPeriod).WithMany().HasForeignKey(x => x.RosterPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ShiftAssignment).WithMany().HasForeignKey(x => x.ShiftAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkSchedule).WithMany().HasForeignKey(x => x.WorkScheduleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Attendance).WithMany().HasForeignKey(x => x.AttendanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AttendanceDaily).WithMany().HasForeignKey(x => x.AttendanceDailyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestReason).WithMany().HasForeignKey(x => x.RequestReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollPeriod).WithMany().HasForeignKey(x => x.PayrollPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollComponent).WithMany().HasForeignKey(x => x.PayrollComponentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SubmittedByUser).WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectedByUser).WithMany().HasForeignKey(x => x.RejectedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CancelledByUser).WithMany().HasForeignKey(x => x.CancelledByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProcessedByUser).WithMany().HasForeignKey(x => x.ProcessedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.RequestNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.WorkforceProfileId, x.OvertimeDate, x.OvertimeRequestStatus, x.IsDelete });
            entity.HasIndex(x => new { x.DepartmentId, x.OvertimeDate, x.OvertimeRequestStatus, x.IsDelete });
            entity.HasIndex(x => new { x.PayrollPeriodId, x.IsPayrollProcessed, x.IsDelete });
        }
    }
}
