using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class MstPositionHeadcountPlanConfiguration : IEntityTypeConfiguration<MstPositionHeadcountPlan>
    {
        public void Configure(EntityTypeBuilder<MstPositionHeadcountPlan> builder)
        {
            builder.ToTable("MstPositionHeadcountPlan", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.PlanCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.PlanName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.CurrentHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.BudgetedHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.TargetHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.PlannedAddition).HasPrecision(18, 2);
            builder.Property(x => x.PlannedReplacement).HasPrecision(18, 2);
            builder.Property(x => x.PlannedReduction).HasPrecision(18, 2);
            builder.Property(x => x.PlannedVacancy).HasPrecision(18, 2);
            builder.Property(x => x.AverageMonthlyCost).HasPrecision(18, 2);
            builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            builder.Property(x => x.PlanStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.EffectiveStartDate).HasColumnType("date");
            builder.Property(x => x.EffectiveEndDate).HasColumnType("date");
            builder.Property(x => x.Justification).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmployeeGrade).WithMany().HasForeignKey(x => x.EmployeeGradeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkforceType).WithMany().HasForeignKey(x => x.WorkforceTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmployeeCategory).WithMany().HasForeignKey(x => x.EmployeeCategoryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmploymentType).WithMany().HasForeignKey(x => x.EmploymentTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Profession).WithMany().HasForeignKey(x => x.ProfessionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Specialization).WithMany().HasForeignKey(x => x.SpecializationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CostCenter).WithMany().HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.PlanCode).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.PlanYear, x.LegalEntityId, x.HospitalSiteId, x.OrganizationUnitId, x.PositionId, x.IsActive });

        }
    }
}
