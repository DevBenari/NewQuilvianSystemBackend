using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BenefitManagement
{
    public class TrxBenefitClaimConfiguration : IEntityTypeConfiguration<TrxBenefitClaim>
    {
        public void Configure(EntityTypeBuilder<TrxBenefitClaim> entity)
        {
            entity.ToTable("TrxBenefitClaim", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ClaimDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ServiceStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ServiceEndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ClaimedAmount).HasPrecision(18, 2);
            entity.Property(x => x.EligibleAmount).HasPrecision(18, 2);
            entity.Property(x => x.NonEligibleAmount).HasPrecision(18, 2);
            entity.Property(x => x.ApprovedAmount).HasPrecision(18, 2);
            entity.Property(x => x.PaidAmount).HasPrecision(18, 2);
            entity.Property(x => x.EnrollmentSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.LimitUsageSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.ValidationResultJson).HasColumnType("jsonb");
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.PaidAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.EmployeeBenefitEnrollment)
                .WithMany(x => x.Claims)
                .HasForeignKey(x => x.EmployeeBenefitEnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

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

            entity.HasOne(x => x.BenefitType)
                .WithMany()
                .HasForeignKey(x => x.BenefitTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PayrollPeriod)
                .WithMany()
                .HasForeignKey(x => x.PayrollPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SubmittedByUser)
                .WithMany()
                .HasForeignKey(x => x.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.VerifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PaidByUser)
                .WithMany()
                .HasForeignKey(x => x.PaidByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ClaimNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.ClaimStatus, x.ClaimDate });

            entity.HasIndex(x => new { x.BenefitPlanId, x.ClaimStatus });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxBenefitClaim> entity)
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
