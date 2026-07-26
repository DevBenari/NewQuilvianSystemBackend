using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.ExpenseManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.ExpenseManagement
{
    public class TrxExpenseClaimConfiguration : IEntityTypeConfiguration<TrxExpenseClaim>
    {
        public void Configure(EntityTypeBuilder<TrxExpenseClaim> entity)
        {
            entity.ToTable("TrxExpenseClaim", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ClaimNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ClaimType).HasMaxLength(40).HasDefaultValue("General").IsRequired();
            entity.Property(x => x.ClaimTitle).HasMaxLength(250).IsRequired();
            entity.Property(x => x.ClaimDescription).HasMaxLength(2000);
            entity.Property(x => x.ClaimDate).HasColumnType("date");
            entity.Property(x => x.PeriodStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.PeriodEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.TotalClaimedAmount).HasPrecision(18, 2);
            entity.Property(x => x.TotalEligibleAmount).HasPrecision(18, 2);
            entity.Property(x => x.TotalNonEligibleAmount).HasPrecision(18, 2);
            entity.Property(x => x.TotalApprovedAmount).HasPrecision(18, 2);
            entity.Property(x => x.TotalPaidAmount).HasPrecision(18, 2);
            entity.Property(x => x.TotalReversedAmount).HasPrecision(18, 2);
            entity.Property(x => x.OutstandingAmount).HasPrecision(18, 2);
            entity.Property(x => x.PolicyTransactionLimitSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.PolicyPeriodLimitSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.BenefitLimitSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.PolicyUsedBeforeAmount).HasPrecision(18, 2);
            entity.Property(x => x.BenefitUsedBeforeAmount).HasPrecision(18, 2);
            entity.Property(x => x.ValidationResultJson).HasColumnType("jsonb");
            entity.Property(x => x.ClaimStatus).HasMaxLength(40).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PaymentCompletedAt).HasColumnType("timestamp with time zone").IsRequired(false);
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
            entity.HasOne(x => x.EmployeeGrade).WithMany().HasForeignKey(x => x.EmployeeGradeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CostCenter).WithMany().HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ReimbursementPolicy).WithMany().HasForeignKey(x => x.ReimbursementPolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BenefitPlan).WithMany().HasForeignKey(x => x.BenefitPlanId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PaymentSettlementMethod).WithMany().HasForeignKey(x => x.PaymentSettlementMethodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BusinessTravelRequest).WithMany().HasForeignKey(x => x.BusinessTravelRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestReason).WithMany().HasForeignKey(x => x.RequestReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollPeriod).WithMany().HasForeignKey(x => x.PayrollPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SubmittedByUser).WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectedByUser).WithMany().HasForeignKey(x => x.RejectedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CancelledByUser).WithMany().HasForeignKey(x => x.CancelledByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ClaimNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.WorkforceProfileId, x.ClaimDate, x.ClaimStatus, x.IsDelete });
            entity.HasIndex(x => new { x.EmployeeId, x.ClaimStatus, x.IsDelete });
            entity.HasIndex(x => new { x.CostCenterId, x.ClaimDate, x.ClaimStatus, x.IsDelete });
            entity.HasIndex(x => new { x.ReimbursementPolicyId, x.BenefitPlanId, x.ClaimStatus, x.IsDelete });
            entity.HasIndex(x => new { x.PayrollPeriodId, x.FinancePaymentId, x.GlHeaderId });
            entity.HasIndex(x => new { x.BusinessTravelRequestId, x.ClaimStatus, x.IsDelete });
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxExpenseClaim> entity)
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
