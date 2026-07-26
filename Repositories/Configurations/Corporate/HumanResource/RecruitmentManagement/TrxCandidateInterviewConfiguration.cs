using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class TrxCandidateInterviewConfiguration : IEntityTypeConfiguration<TrxCandidateInterview>
    {
        public void Configure(EntityTypeBuilder<TrxCandidateInterview> builder)
        {
            builder.ToTable("TrxCandidateInterview", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.InterviewType).HasMaxLength(30).IsRequired();
            builder.Property(x => x.InterviewMode).HasMaxLength(20).IsRequired();
            builder.Property(x => x.ScheduledStartAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ScheduledEndAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ActualStartAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ActualEndAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.LocationDescription).HasMaxLength(500);
            builder.Property(x => x.MeetingUrl).HasMaxLength(1000);
            builder.Property(x => x.InterviewStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.FinalScore).HasPrecision(10, 2);
            builder.Property(x => x.FinalRecommendation).HasMaxLength(30);
            builder.Property(x => x.PanelDefinitionJson).HasColumnType("jsonb");
            builder.Property(x => x.EvaluationSummaryJson).HasColumnType("jsonb");
            builder.Property(x => x.FinalNotes).HasMaxLength(1500);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.CandidateApplication).WithMany().HasForeignKey(x => x.CandidateApplicationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.InterviewTemplate).WithMany().HasForeignKey(x => x.InterviewTemplateId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RecruitmentStage).WithMany().HasForeignKey(x => x.RecruitmentStageId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkLocation).WithMany().HasForeignKey(x => x.WorkLocationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PanelLeadUser).WithMany().HasForeignKey(x => x.PanelLeadUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PanelLeadWorkforceProfile).WithMany().HasForeignKey(x => x.PanelLeadWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.CandidateApplicationId, x.InterviewRound, x.InterviewType });
            builder.HasIndex(x => new { x.ScheduledStartAt, x.InterviewStatus, x.PanelLeadUserId });
        }
    }
}
