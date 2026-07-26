using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class TrxCandidateAssessmentConfiguration : IEntityTypeConfiguration<TrxCandidateAssessment>
    {
        public void Configure(EntityTypeBuilder<TrxCandidateAssessment> builder)
        {
            builder.ToTable("TrxCandidateAssessment", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.ScheduledAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.StartedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.AssessmentStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.RawScore).HasPrecision(10, 2);
            builder.Property(x => x.FinalScore).HasPrecision(10, 2);
            builder.Property(x => x.AssessmentResult).HasMaxLength(30);
            builder.Property(x => x.ExternalReferenceNumber).HasMaxLength(200);
            builder.Property(x => x.ResultFilePath).HasMaxLength(500);
            builder.Property(x => x.EvaluatorNotes).HasMaxLength(1500);
            builder.Property(x => x.ResultDetailJson).HasColumnType("jsonb");
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.CandidateApplication).WithMany().HasForeignKey(x => x.CandidateApplicationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AssessmentMethod).WithMany().HasForeignKey(x => x.AssessmentMethodId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RecruitmentStage).WithMany().HasForeignKey(x => x.RecruitmentStageId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EvaluatorUser).WithMany().HasForeignKey(x => x.EvaluatorUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.CandidateApplicationId, x.AssessmentMethodId, x.AssessmentStatus });
        }
    }
}
