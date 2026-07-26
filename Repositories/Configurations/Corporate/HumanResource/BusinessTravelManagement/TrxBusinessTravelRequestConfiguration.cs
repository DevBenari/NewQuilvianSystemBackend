using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BusinessTravelManagement
{
    public class TrxBusinessTravelRequestConfiguration : IEntityTypeConfiguration<TrxBusinessTravelRequest>
    {
        public void Configure(EntityTypeBuilder<TrxBusinessTravelRequest> entity)
        {
            entity.ToTable("TrxBusinessTravelRequest", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TravelRequestNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TravelTitle).HasMaxLength(250).IsRequired();
            entity.Property(x => x.TravelPurpose).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.ActivityDescription).HasMaxLength(2000);
            entity.Property(x => x.Origin).HasMaxLength(250).IsRequired();
            entity.Property(x => x.Destination).HasMaxLength(250).IsRequired();
            entity.Property(x => x.StartDate).HasColumnType("date");
            entity.Property(x => x.EndDate).HasColumnType("date");
            entity.Property(x => x.PlannedDepartureAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PlannedReturnAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.EstimatedTransportationAmount).HasPrecision(18, 2);
            entity.Property(x => x.EstimatedAccommodationAmount).HasPrecision(18, 2);
            entity.Property(x => x.EstimatedAllowanceAmount).HasPrecision(18, 2);
            entity.Property(x => x.EstimatedOtherAmount).HasPrecision(18, 2);
            entity.Property(x => x.EstimatedTotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.ApprovedBudgetAmount).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(10).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.BudgetSourceCode).HasMaxLength(100);
            entity.Property(x => x.BudgetSourceName).HasMaxLength(250);
            entity.Property(x => x.ValidationResultJson).HasColumnType("jsonb");
            entity.Property(x => x.TravelStatus).HasMaxLength(40).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ManagerApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.BudgetApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.HrVerifiedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.FinanceVerifiedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.TravelStartedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.TravelEndedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovalNotes).HasMaxLength(2000);
            entity.Property(x => x.RejectionNotes).HasMaxLength(2000);
            entity.Property(x => x.CancellationReason).HasMaxLength(2000);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationAssignment).WithMany().HasForeignKey(x => x.OrganizationAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CostCenter).WithMany().HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TravelType).WithMany().HasForeignKey(x => x.TravelTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TravelPolicy).WithMany().HasForeignKey(x => x.TravelPolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DestinationZone).WithMany().HasForeignKey(x => x.DestinationZoneId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestReason).WithMany().HasForeignKey(x => x.RequestReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollPeriod).WithMany().HasForeignKey(x => x.PayrollPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SubmittedByUser).WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ManagerApprovedByUser).WithMany().HasForeignKey(x => x.ManagerApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BudgetApprovedByUser).WithMany().HasForeignKey(x => x.BudgetApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HrVerifiedByUser).WithMany().HasForeignKey(x => x.HrVerifiedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.FinanceVerifiedByUser).WithMany().HasForeignKey(x => x.FinanceVerifiedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectedByUser).WithMany().HasForeignKey(x => x.RejectedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CancelledByUser).WithMany().HasForeignKey(x => x.CancelledByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.TravelRequestNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.WorkforceProfileId, x.StartDate, x.EndDate, x.TravelStatus, x.IsDelete });
            entity.HasIndex(x => new { x.DepartmentId, x.StartDate, x.TravelStatus, x.IsDelete });
            entity.HasIndex(x => new { x.CostCenterId, x.TravelStatus, x.IsDelete });
            entity.HasIndex(x => new { x.PayrollPeriodId, x.FinancePaymentId, x.GlHeaderId });
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxBusinessTravelRequest> entity)
        {
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
