using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class TrxHeadcountRequestConfiguration : IEntityTypeConfiguration<TrxHeadcountRequest>
    {
        public void Configure(EntityTypeBuilder<TrxHeadcountRequest> builder)
        {
            builder.ToTable("TrxHeadcountRequest", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.RequestNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.RequestType).HasMaxLength(30).IsRequired();
            builder.Property(x => x.RequestedHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.ApprovedHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.RequiredDate).HasColumnType("date");
            builder.Property(x => x.PriorityLevel).HasMaxLength(20).IsRequired();
            builder.Property(x => x.BusinessJustification).HasMaxLength(1000);
            builder.Property(x => x.EstimatedMonthlyCost).HasPrecision(18, 2);
            builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            builder.Property(x => x.RequestStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.FulfilledAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.AnnualManpowerPlan).WithMany().HasForeignKey(x => x.AnnualManpowerPlanId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ManpowerPlanDetail).WithMany().HasForeignKey(x => x.ManpowerPlanDetailId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmployeeGrade).WithMany().HasForeignKey(x => x.EmployeeGradeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkforceType).WithMany().HasForeignKey(x => x.WorkforceTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmployeeCategory).WithMany().HasForeignKey(x => x.EmployeeCategoryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmploymentType).WithMany().HasForeignKey(x => x.EmploymentTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ContractType).WithMany().HasForeignKey(x => x.ContractTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Profession).WithMany().HasForeignKey(x => x.ProfessionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Specialization).WithMany().HasForeignKey(x => x.SpecializationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CostCenter).WithMany().HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RequestReason).WithMany().HasForeignKey(x => x.RequestReasonId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RequestedByWorkforceProfile).WithMany().HasForeignKey(x => x.RequestedByWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RequestedByUser).WithMany().HasForeignKey(x => x.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.RequestNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.RequestStatus, x.RequiredDate, x.PriorityLevel });
            builder.HasIndex(x => new { x.OrganizationUnitId, x.DepartmentId, x.PositionId, x.RequestStatus });
            builder.HasIndex(x => x.WorkflowInstanceId);

        }
    }
}
