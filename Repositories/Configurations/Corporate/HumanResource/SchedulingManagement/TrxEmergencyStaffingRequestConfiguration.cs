using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.SchedulingManagement
{
    public class TrxEmergencyStaffingRequestConfiguration : IEntityTypeConfiguration<TrxEmergencyStaffingRequest>
    {
        public void Configure(EntityTypeBuilder<TrxEmergencyStaffingRequest> entity)
        {
            entity.ToTable("TrxEmergencyStaffingRequest", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RequestNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RequirementDate).HasColumnType("date");
            entity.Property(x => x.RequiredStartAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.RequiredEndAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Priority).HasMaxLength(20).HasDefaultValue("High").IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.RequestStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.FulfilledAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovalNotes).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
            entity.HasOne(x => x.RosterPeriod).WithMany().HasForeignKey(x => x.RosterPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ShiftAssignment).WithMany().HasForeignKey(x => x.ShiftAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Profession).WithMany().HasForeignKey(x => x.ProfessionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Specialization).WithMany().HasForeignKey(x => x.SpecializationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Competency).WithMany().HasForeignKey(x => x.CompetencyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestReason).WithMany().HasForeignKey(x => x.RequestReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestedByWorkforceProfile).WithMany().HasForeignKey(x => x.RequestedByWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestedByUser).WithMany().HasForeignKey(x => x.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.RequestNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.RequirementDate, x.HospitalSiteId, x.OrganizationUnitId, x.DepartmentId, x.ShiftId });
            entity.HasIndex(x => new { x.RequestStatus, x.Priority, x.IsActive, x.IsDelete });
        }
    }
}
