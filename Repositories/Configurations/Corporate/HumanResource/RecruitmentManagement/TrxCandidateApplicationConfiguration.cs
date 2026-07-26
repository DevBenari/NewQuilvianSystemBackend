using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class TrxCandidateApplicationConfiguration : IEntityTypeConfiguration<TrxCandidateApplication>
    {
        public void Configure(EntityTypeBuilder<TrxCandidateApplication> builder)
        {
            builder.ToTable("TrxCandidateApplication", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.ApplicationNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.AppliedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ApplicationStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.OverallScore).HasPrecision(10, 2);
            builder.Property(x => x.LastStageChangedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.WithdrawnAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.StatusNotes).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.Candidate).WithMany().HasForeignKey(x => x.CandidateId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.JobVacancy).WithMany().HasForeignKey(x => x.JobVacancyId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.JobRequisition).WithMany().HasForeignKey(x => x.JobRequisitionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RecruitmentSource).WithMany().HasForeignKey(x => x.RecruitmentSourceId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CurrentStage).WithMany().HasForeignKey(x => x.CurrentStageId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CandidateStatus).WithMany().HasForeignKey(x => x.CandidateStatusId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AssignedRecruiterUser).WithMany().HasForeignKey(x => x.AssignedRecruiterUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WithdrawalReason).WithMany().HasForeignKey(x => x.WithdrawalReasonId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.ApplicationNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.CandidateId, x.JobVacancyId }).IsUnique().HasFilter("\"IsDelete\" = false AND \"ApplicationStatus\" <> 'Withdrawn'");
            builder.HasIndex(x => new { x.ApplicationStatus, x.CurrentStageId, x.AssignedRecruiterUserId });
            builder.HasIndex(x => x.WorkflowInstanceId);
        }
    }
}
