using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BenefitManagement
{
    public class TrxEmployeeLoanConfiguration : IEntityTypeConfiguration<TrxEmployeeLoan>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeLoan> entity)
        {
            entity.ToTable("TrxEmployeeLoan", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ApplicationDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ApprovalDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DisbursementDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.InstallmentStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.MaturityDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.PrincipalAmount).HasPrecision(18, 2);
            entity.Property(x => x.InterestRate).HasPrecision(18, 4);
            entity.Property(x => x.InterestAmount).HasPrecision(18, 2);
            entity.Property(x => x.AdministrationFee).HasPrecision(18, 2);
            entity.Property(x => x.TotalPayableAmount).HasPrecision(18, 2);
            entity.Property(x => x.InstallmentAmount).HasPrecision(18, 2);
            entity.Property(x => x.PaidAmount).HasPrecision(18, 2);
            entity.Property(x => x.OutstandingAmount).HasPrecision(18, 2);
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DisbursedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationAssignment)
                .WithMany()
                .HasForeignKey(x => x.OrganizationAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.BenefitPlan)
                .WithMany()
                .HasForeignKey(x => x.BenefitPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.BankAccount)
                .WithMany()
                .HasForeignKey(x => x.BankAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PayrollComponent)
                .WithMany()
                .HasForeignKey(x => x.PayrollComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SubmittedByUser)
                .WithMany()
                .HasForeignKey(x => x.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DisbursedByUser)
                .WithMany()
                .HasForeignKey(x => x.DisbursedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.LoanNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.LoanStatus, x.ApplicationDate });

            entity.HasIndex(x => new { x.MaturityDate, x.LoanStatus });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxEmployeeLoan> entity)
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
