using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LearningAndDevelopment
{
    public class TrxTrainingAssessmentConfiguration : IEntityTypeConfiguration<TrxTrainingAssessment>
    {
        public void Configure(EntityTypeBuilder<TrxTrainingAssessment> entity)
        {
            entity.ToTable("TrxTrainingAssessment", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.StartedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.Score).HasPrecision(18, 4);
            entity.Property(x => x.MaximumScore).HasPrecision(18, 4);
            entity.Property(x => x.PassingScore).HasPrecision(18, 4);
            entity.Property(x => x.AnswerSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.AssessmentResultJson).HasColumnType("jsonb");
            entity.Property(x => x.AttemptNumber).HasDefaultValue(1);
            entity.Property(x => x.Score).HasDefaultValue(0m);
            entity.Property(x => x.MaximumScore).HasDefaultValue(100m);
            entity.Property(x => x.PassingScore).HasDefaultValue(0m);
            entity.Property(x => x.IsPassed).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.TrainingSession)
                .WithMany(x => x.Assessments)
                .HasForeignKey(x => x.TrainingSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TrainingParticipant)
                .WithMany(x => x.Assessments)
                .HasForeignKey(x => x.TrainingParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Competency)
                .WithMany()
                .HasForeignKey(x => x.CompetencyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AssessedByUser)
                .WithMany()
                .HasForeignKey(x => x.AssessedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.TrainingParticipantId, x.AssessmentType, x.AttemptNumber })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.TrainingSessionId, x.AssessmentType });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxTrainingAssessment> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
