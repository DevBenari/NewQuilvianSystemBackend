using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Performance
{
    public class MstPerformanceTemplateConfiguration : IEntityTypeConfiguration<MstPerformanceTemplate>
    {
        public void Configure(EntityTypeBuilder<MstPerformanceTemplate> entity)
        {
            entity.ToTable("MstPerformanceTemplate", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RatingScaleId).IsRequired();
            entity.Property(x => x.TemplateCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TemplateName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.TemplateType).HasMaxLength(50).HasDefaultValue("EmployeePerformance").IsRequired();
            entity.Property(x => x.TotalWeight).HasPrecision(7, 2).HasDefaultValue(100m);
            entity.Property(x => x.MinimumPassingScore).HasPrecision(10, 2).IsRequired(false);
            entity.Property(x => x.IsSelfAssessmentRequired).HasDefaultValue(true);
            entity.Property(x => x.IsManagerAssessmentRequired).HasDefaultValue(true);
            entity.Property(x => x.IsPeerAssessmentAllowed).HasDefaultValue(false);
            entity.Property(x => x.IsSubordinateAssessmentAllowed).HasDefaultValue(false);
            entity.Property(x => x.IsCalibrationRequired).HasDefaultValue(false);
            entity.Property(x => x.EmployeeInstructions).HasMaxLength(2000);
            entity.Property(x => x.ReviewerInstructions).HasMaxLength(2000);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
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

            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.PerformanceCycle)
                .WithMany(x => x.PerformanceTemplates)
                .HasForeignKey(x => x.PerformanceCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RatingScale)
                .WithMany(x => x.PerformanceTemplates)
                .HasForeignKey(x => x.RatingScaleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.EmployeeCategory).WithMany().HasForeignKey(x => x.EmployeeCategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.EmploymentType).WithMany().HasForeignKey(x => x.EmploymentTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Profession).WithMany().HasForeignKey(x => x.ProfessionId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.TemplateCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.TemplateName);
            entity.HasIndex(x => new { x.PerformanceCycleId, x.RatingScaleId, x.TemplateType });
            entity.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.OrganizationUnitId, x.DepartmentId, x.PositionId });
            entity.HasIndex(x => new { x.EmployeeCategoryId, x.EmploymentTypeId, x.ProfessionId });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.IsDefault, x.TemplateType, x.IsActive, x.IsDelete });
        }
    }
}
