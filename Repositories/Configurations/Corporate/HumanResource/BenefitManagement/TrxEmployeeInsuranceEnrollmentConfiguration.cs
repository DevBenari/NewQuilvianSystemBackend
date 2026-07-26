using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BenefitManagement
{
    public class TrxEmployeeInsuranceEnrollmentConfiguration : IEntityTypeConfiguration<TrxEmployeeInsuranceEnrollment>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeInsuranceEnrollment> entity)
        {
            entity.ToTable("TrxEmployeeInsuranceEnrollment", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.SubmittedDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.PremiumAmount).HasPrecision(18, 2);
            entity.Property(x => x.EmployerContributionAmount).HasPrecision(18, 2);
            entity.Property(x => x.EmployeeContributionAmount).HasPrecision(18, 2);
            entity.Property(x => x.ExternalResponseJson).HasColumnType("jsonb");
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.EmployeeBenefitEnrollment)
                .WithMany(x => x.InsuranceEnrollments)
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

            entity.HasOne(x => x.BenefitPlan)
                .WithMany()
                .HasForeignKey(x => x.BenefitPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.InsuranceProfile)
                .WithMany()
                .HasForeignKey(x => x.InsuranceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.VerifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.InsuranceEnrollmentNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.ProviderName, x.EnrollmentStatus });

            entity.HasIndex(x => new { x.PolicyNumber, x.MemberNumber });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxEmployeeInsuranceEnrollment> entity)
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
