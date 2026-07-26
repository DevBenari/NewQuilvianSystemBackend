using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.CompetencyAndCredential
{
    public class MstMandatoryTrainingRuleConfiguration : IEntityTypeConfiguration<MstMandatoryTrainingRule>
    {
        public void Configure(EntityTypeBuilder<MstMandatoryTrainingRule> entity)
        {
            entity.ToTable("MstMandatoryTrainingRule", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TrainingCatalogId).IsRequired();
            entity.Property(x => x.RuleCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RuleName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CompletionDueDaysFromJoin).HasDefaultValue(0);
            entity.Property(x => x.RecurrenceMonths).IsRequired(false);
            entity.Property(x => x.GracePeriodDays).HasDefaultValue(0);
            entity.Property(x => x.IsRequiredBeforeAssignment).HasDefaultValue(false);
            entity.Property(x => x.IsRequiredForCredentialing).HasDefaultValue(false);
            entity.Property(x => x.IsRequiredBeforeIndependentPractice).HasDefaultValue(false);
            entity.Property(x => x.RequiresPassingResult).HasDefaultValue(false);
            entity.Property(x => x.MinimumPassingScore).HasPrecision(5, 2).IsRequired(false);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.Priority).HasDefaultValue(0);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.TrainingCatalog).WithMany(x => x.MandatoryTrainingRules).HasForeignKey(x => x.TrainingCatalogId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Profession).WithMany().HasForeignKey(x => x.ProfessionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Specialization).WithMany().HasForeignKey(x => x.SpecializationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.EmployeeCategory).WithMany().HasForeignKey(x => x.EmployeeCategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.EmploymentType).WithMany().HasForeignKey(x => x.EmploymentTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.RuleCode).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.TrainingCatalogId, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.OrganizationUnitId, x.PositionId });
            entity.HasIndex(x => new { x.ProfessionId, x.SpecializationId, x.EmployeeCategoryId, x.EmploymentTypeId });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate, x.Priority });
        }
    }
}
