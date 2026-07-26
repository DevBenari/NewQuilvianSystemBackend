using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BenefitManagement
{
    public class TrxEmployeeBenefitEnrollmentConfiguration : IEntityTypeConfiguration<TrxEmployeeBenefitEnrollment>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeBenefitEnrollment> entity)
        {
            entity.ToTable("TrxEmployeeBenefitEnrollment", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.EnrollmentDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CoverageLimitAmount).HasPrecision(18, 2);
            entity.Property(x => x.UsedAmount).HasPrecision(18, 2);
            entity.Property(x => x.RemainingAmount).HasPrecision(18, 2);
            entity.Property(x => x.EmployerContributionAmount).HasPrecision(18, 2);
            entity.Property(x => x.EmployeeContributionAmount).HasPrecision(18, 2);
            entity.Property(x => x.EligibilityResultJson).HasColumnType("jsonb");
            entity.Property(x => x.PlanSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ActivatedAt).HasColumnType("timestamp with time zone");
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

            entity.HasOne(x => x.BenefitEligibilityRule)
                .WithMany()
                .HasForeignKey(x => x.BenefitEligibilityRuleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PayrollPeriod)
                .WithMany()
                .HasForeignKey(x => x.PayrollPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SubmittedByUser)
                .WithMany()
                .HasForeignKey(x => x.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ActivatedByUser)
                .WithMany()
                .HasForeignKey(x => x.ActivatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.EnrollmentNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.BenefitPlanId, x.EnrollmentStatus });

            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate, x.IsActive });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxEmployeeBenefitEnrollment> entity)
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
