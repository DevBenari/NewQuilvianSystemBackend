using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class TrxJobRequisitionConfiguration : IEntityTypeConfiguration<TrxJobRequisition>
    {
        public void Configure(EntityTypeBuilder<TrxJobRequisition> builder)
        {
            builder.ToTable("TrxJobRequisition", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.RequisitionNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.JobTitle).HasMaxLength(200).IsRequired();
            builder.Property(x => x.RequisitionType).HasMaxLength(30).IsRequired();
            builder.Property(x => x.RequiredStartDate).HasColumnType("date");
            builder.Property(x => x.TargetFulfillmentDate).HasColumnType("date");
            builder.Property(x => x.MinimumSalaryBudget).HasPrecision(18, 2);
            builder.Property(x => x.MaximumSalaryBudget).HasPrecision(18, 2);
            builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            builder.Property(x => x.BusinessJustification).HasMaxLength(1500);
            builder.Property(x => x.JobDescription).HasMaxLength(2000);
            builder.Property(x => x.MinimumQualification).HasMaxLength(2000);
            builder.Property(x => x.PriorityLevel).HasMaxLength(30).IsRequired();
            builder.Property(x => x.RequisitionStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ClosedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.AnnualManpowerPlan).WithMany().HasForeignKey(x => x.AnnualManpowerPlanId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ManpowerPlanDetail).WithMany().HasForeignKey(x => x.ManpowerPlanDetailId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.HeadcountRequest).WithMany().HasForeignKey(x => x.HeadcountRequestId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.JobFamily).WithMany().HasForeignKey(x => x.JobFamilyId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.JobLevel).WithMany().HasForeignKey(x => x.JobLevelId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmployeeGrade).WithMany().HasForeignKey(x => x.EmployeeGradeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkforceType).WithMany().HasForeignKey(x => x.WorkforceTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmployeeCategory).WithMany().HasForeignKey(x => x.EmployeeCategoryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmploymentType).WithMany().HasForeignKey(x => x.EmploymentTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ContractType).WithMany().HasForeignKey(x => x.ContractTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkerSource).WithMany().HasForeignKey(x => x.WorkerSourceId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Profession).WithMany().HasForeignKey(x => x.ProfessionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Specialization).WithMany().HasForeignKey(x => x.SpecializationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CostCenter).WithMany().HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkLocation).WithMany().HasForeignKey(x => x.WorkLocationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RequestReason).WithMany().HasForeignKey(x => x.RequestReasonId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RequestedByWorkforceProfile).WithMany().HasForeignKey(x => x.RequestedByWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RequestedByUser).WithMany().HasForeignKey(x => x.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.RequisitionNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.RequisitionStatus, x.PriorityLevel, x.RequiredStartDate });
            builder.HasIndex(x => new { x.OrganizationUnitId, x.DepartmentId, x.PositionId, x.RequisitionStatus });
            builder.HasIndex(x => x.WorkflowInstanceId);
        }
    }
}
