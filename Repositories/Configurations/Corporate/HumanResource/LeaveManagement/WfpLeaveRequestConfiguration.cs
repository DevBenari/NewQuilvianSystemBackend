using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LeaveManagement
{
    public class WfpLeaveRequestConfiguration : IEntityTypeConfiguration<WfpLeaveRequest>
    {
        public void Configure(EntityTypeBuilder<WfpLeaveRequest> entity)
        {
            entity.ToTable("WfpLeaveRequest", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RequestNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.StartDate).HasColumnType("date");
            entity.Property(x => x.EndDate).HasColumnType("date");
            entity.Property(x => x.StartTime).HasColumnType("time without time zone").IsRequired(false);
            entity.Property(x => x.EndTime).HasColumnType("time without time zone").IsRequired(false);
            entity.Property(x => x.HalfDayPeriod).HasMaxLength(20);
            entity.Property(x => x.RequestedDays).HasPrecision(10, 2);
            entity.Property(x => x.CalculatedWorkingDays).HasPrecision(10, 2);
            entity.Property(x => x.ExcludedHolidayDays).HasPrecision(10, 2);
            entity.Property(x => x.ExcludedWeeklyOffDays).HasPrecision(10, 2);
            entity.Property(x => x.BalanceBeforeRequest).HasPrecision(10, 2);
            entity.Property(x => x.EstimatedBalanceDeduction).HasPrecision(10, 2);
            entity.Property(x => x.EstimatedBalanceAfterRequest).HasPrecision(10, 2);
            entity.Property(x => x.ActualBalanceDeduction).HasPrecision(10, 2);
            entity.Property(x => x.Reason).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.ContactAddressDuringLeave).HasMaxLength(500);
            entity.Property(x => x.ContactNumberDuringLeave).HasMaxLength(50);
            entity.Property(x => x.HandoverNotes).HasMaxLength(2000);
            entity.Property(x => x.BalanceSimulationJson).HasColumnType("jsonb");
            entity.Property(x => x.RosterImpactJson).HasColumnType("jsonb");
            entity.Property(x => x.ValidationResultJson).HasColumnType("jsonb");
            entity.Property(x => x.LeaveRequestStatus).HasMaxLength(40).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.SupervisorApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ManagerApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.HrVerifiedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.TakenAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.RecalledAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ExpiredAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovalNotes).HasMaxLength(2000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeavePolicy).WithMany().HasForeignKey(x => x.LeavePolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveBalance).WithMany(x => x.LeaveRequests).HasForeignKey(x => x.LeaveBalanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationAssignment).WithMany().HasForeignKey(x => x.OrganizationAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ReplacementWorkforceProfile).WithMany().HasForeignKey(x => x.ReplacementWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestReason).WithMany().HasForeignKey(x => x.RequestReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SubmittedByUser).WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectedByUser).WithMany().HasForeignKey(x => x.RejectedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CancelledByUser).WithMany().HasForeignKey(x => x.CancelledByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.RequestNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.WorkforceProfileId, x.StartDate, x.EndDate, x.LeaveRequestStatus, x.IsDelete });
            entity.HasIndex(x => new { x.DepartmentId, x.StartDate, x.EndDate, x.LeaveRequestStatus, x.IsDelete });
            entity.HasIndex(x => new { x.ReplacementWorkforceProfileId, x.StartDate, x.EndDate, x.IsDelete });
        }
    }
}
