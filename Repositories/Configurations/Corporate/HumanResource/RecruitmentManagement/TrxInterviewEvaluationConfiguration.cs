using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class TrxInterviewEvaluationConfiguration : IEntityTypeConfiguration<TrxInterviewEvaluation>
    {
        public void Configure(EntityTypeBuilder<TrxInterviewEvaluation> builder)
        {
            builder.ToTable("TrxInterviewEvaluation", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.TechnicalScore).HasPrecision(10, 2);
            builder.Property(x => x.BehavioralScore).HasPrecision(10, 2);
            builder.Property(x => x.CultureFitScore).HasPrecision(10, 2);
            builder.Property(x => x.OverallScore).HasPrecision(10, 2);
            builder.Property(x => x.Recommendation).HasMaxLength(30).IsRequired();
            builder.Property(x => x.Strengths).HasMaxLength(2000);
            builder.Property(x => x.Concerns).HasMaxLength(2000);
            builder.Property(x => x.Comments).HasMaxLength(2000);
            builder.Property(x => x.CriteriaResultJson).HasColumnType("jsonb");
            builder.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.CandidateInterview).WithMany().HasForeignKey(x => x.CandidateInterviewId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EvaluatorUser).WithMany().HasForeignKey(x => x.EvaluatorUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EvaluatorWorkforceProfile).WithMany().HasForeignKey(x => x.EvaluatorWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RatingScale).WithMany().HasForeignKey(x => x.RatingScaleId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.CandidateInterviewId, x.EvaluatorUserId }).IsUnique().HasFilter("\"EvaluatorUserId\" IS NOT NULL AND \"IsDelete\" = false");
            builder.HasIndex(x => new { x.CandidateInterviewId, x.IsSubmitted, x.Recommendation });
        }
    }
}
