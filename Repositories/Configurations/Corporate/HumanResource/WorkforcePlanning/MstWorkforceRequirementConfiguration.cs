using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class MstWorkforceRequirementConfiguration : IEntityTypeConfiguration<MstWorkforceRequirement>
    {
        public void Configure(EntityTypeBuilder<MstWorkforceRequirement> builder)
        {
            builder.ToTable("MstWorkforceRequirement", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.UserType).HasConversion<int>().IsRequired();
            builder.Property(x => x.RequirementCategory).HasMaxLength(50).IsRequired();
            builder.Property(x => x.RequirementCode).HasMaxLength(100).IsRequired();
            builder.Property(x => x.RequirementName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.TargetEntityName).HasMaxLength(100);
            builder.Property(x => x.RequirementScopeType).HasMaxLength(50).IsRequired();
            builder.Property(x => x.MinimumQuantity).HasPrecision(18, 2);
            builder.Property(x => x.TargetQuantity).HasPrecision(18, 2);
            builder.Property(x => x.MaximumQuantity).HasPrecision(18, 2);
            builder.Property(x => x.MeasurementUnit).HasMaxLength(50);
            builder.Property(x => x.RequiredCompetencyLevel).HasMaxLength(50);
            builder.Property(x => x.EffectiveStartDate).HasColumnType("date");
            builder.Property(x => x.EffectiveEndDate).HasColumnType("date");
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.IsRequired).HasDefaultValue(true);
            builder.Property(x => x.IsMultipleAllowed).HasDefaultValue(false);
            builder.Property(x => x.IsFileRequired).HasDefaultValue(true);
            builder.Property(x => x.IsVerificationRequired).HasDefaultValue(true);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkforceType).WithMany().HasForeignKey(x => x.WorkforceTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmployeeCategory).WithMany().HasForeignKey(x => x.EmployeeCategoryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmploymentType).WithMany().HasForeignKey(x => x.EmploymentTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Profession).WithMany().HasForeignKey(x => x.ProfessionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Specialization).WithMany().HasForeignKey(x => x.SpecializationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmployeeGrade).WithMany().HasForeignKey(x => x.EmployeeGradeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkSchedule).WithMany().HasForeignKey(x => x.WorkScheduleId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Competency).WithMany().HasForeignKey(x => x.CompetencyId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.UserType, x.RequirementCategory, x.RequirementCode, x.TargetEntityName })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.OrganizationUnitId, x.DepartmentId, x.PositionId, x.IsActive, x.IsDelete });
            builder.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate, x.IsActive });

        }
    }
}
