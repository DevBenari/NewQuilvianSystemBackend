using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BenefitManagement
{
    public class HrdBenefitGradeEligibilityConfiguration : IEntityTypeConfiguration<HrdBenefitGradeEligibility>
    {
        public void Configure(EntityTypeBuilder<HrdBenefitGradeEligibility> entity)
        {
            entity.ToTable("HrdBenefitGradeEligibility", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.BenefitPlanId).IsRequired();
            entity.Property(x => x.GradeLevelCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.EmploymentTypeCode).HasMaxLength(50).HasDefaultValue("Permanent").IsRequired();
            entity.Property(x => x.SpecificAnnualLimit).HasPrecision(18, 2);
            entity.Property(x => x.InpatientRoomClass).HasMaxLength(50).HasDefaultValue("Kelas 1").IsRequired();
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);

            entity.HasIndex(x => new { x.BenefitPlanId, x.GradeLevelCode, x.EmploymentTypeCode })
                .IsUnique()
                .HasDatabaseName("uq_benefit_grade_plan");

            entity.HasOne(x => x.BenefitPlan)
                .WithMany(x => x.GradeEligibilities)
                .HasForeignKey(x => x.BenefitPlanId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
