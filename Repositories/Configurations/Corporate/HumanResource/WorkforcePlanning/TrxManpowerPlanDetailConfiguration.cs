using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class TrxManpowerPlanDetailConfiguration : IEntityTypeConfiguration<TrxManpowerPlanDetail>
    {
        public void Configure(EntityTypeBuilder<TrxManpowerPlanDetail> builder)
        {
            builder.ToTable("TrxManpowerPlanDetail", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.CurrentHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.BudgetedHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.TargetHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.RequestedAddition).HasPrecision(18, 2);
            builder.Property(x => x.RequestedReplacement).HasPrecision(18, 2);
            builder.Property(x => x.PlannedReduction).HasPrecision(18, 2);
            builder.Property(x => x.ApprovedHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.AverageMonthlyCost).HasPrecision(18, 2);
            builder.Property(x => x.EstimatedAnnualCost).HasPrecision(18, 2);
            builder.Property(x => x.PriorityLevel).HasMaxLength(20).IsRequired();
            builder.Property(x => x.DetailStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.Justification).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.AnnualManpowerPlan).WithMany(x => x.Details).HasForeignKey(x => x.AnnualManpowerPlanId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PositionHeadcountPlan).WithMany().HasForeignKey(x => x.PositionHeadcountPlanId).OnDelete(DeleteBehavior.Restrict);
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

            builder.HasIndex(x => new { x.AnnualManpowerPlanId, x.OrganizationUnitId, x.DepartmentId, x.PositionId });
            builder.HasIndex(x => new { x.DetailStatus, x.PriorityLevel, x.IsActive });

        }
    }
}
