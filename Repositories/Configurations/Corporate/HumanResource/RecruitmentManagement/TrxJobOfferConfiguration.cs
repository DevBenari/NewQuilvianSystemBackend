using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class TrxJobOfferConfiguration : IEntityTypeConfiguration<TrxJobOffer>
    {
        public void Configure(EntityTypeBuilder<TrxJobOffer> builder)
        {
            builder.ToTable("TrxJobOffer", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.OfferNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.BaseSalaryAmount).HasPrecision(18, 2);
            builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            builder.Property(x => x.AllowanceConfigurationJson).HasColumnType("jsonb");
            builder.Property(x => x.BenefitConfigurationJson).HasColumnType("jsonb");
            builder.Property(x => x.ProposedStartDate).HasColumnType("date");
            builder.Property(x => x.ContractEndDate).HasColumnType("date");
            builder.Property(x => x.OfferDate).HasColumnType("date");
            builder.Property(x => x.ValidUntil).HasColumnType("date");
            builder.Property(x => x.OfferStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.SentAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.AcceptedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CandidateResponseNotes).HasMaxLength(1000);
            builder.Property(x => x.OfferDocumentPath).HasMaxLength(500);
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.CandidateApplication).WithMany().HasForeignKey(x => x.CandidateApplicationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.JobRequisition).WithMany().HasForeignKey(x => x.JobRequisitionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmployeeGrade).WithMany().HasForeignKey(x => x.EmployeeGradeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SalaryGrade).WithMany().HasForeignKey(x => x.SalaryGradeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SalaryStructure).WithMany().HasForeignKey(x => x.SalaryStructureId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmploymentType).WithMany().HasForeignKey(x => x.EmploymentTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ContractType).WithMany().HasForeignKey(x => x.ContractTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmployeeCategory).WithMany().HasForeignKey(x => x.EmployeeCategoryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkLocation).WithMany().HasForeignKey(x => x.WorkLocationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CostCenter).WithMany().HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.OfferNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.CandidateApplicationId, x.OfferStatus, x.ValidUntil });
            builder.HasIndex(x => x.WorkflowInstanceId);
        }
    }
}
