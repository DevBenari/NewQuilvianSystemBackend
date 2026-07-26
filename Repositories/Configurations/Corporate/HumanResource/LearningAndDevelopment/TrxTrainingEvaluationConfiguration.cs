using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LearningAndDevelopment
{
    public class TrxTrainingEvaluationConfiguration : IEntityTypeConfiguration<TrxTrainingEvaluation>
    {
        public void Configure(EntityTypeBuilder<TrxTrainingEvaluation> entity)
        {
            entity.ToTable("TrxTrainingEvaluation", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.EvaluationDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ContentRating).HasPrecision(5, 2);
            entity.Property(x => x.InstructorRating).HasPrecision(5, 2);
            entity.Property(x => x.FacilityRating).HasPrecision(5, 2);
            entity.Property(x => x.OverallRating).HasPrecision(5, 2);
            entity.Property(x => x.EvaluationJson).HasColumnType("jsonb");
            entity.Property(x => x.WouldRecommend).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.TrainingSession)
                .WithMany(x => x.Evaluations)
                .HasForeignKey(x => x.TrainingSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TrainingParticipant)
                .WithMany(x => x.Evaluations)
                .HasForeignKey(x => x.TrainingParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.TrainingParticipantId, x.EvaluationType })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.TrainingSessionId, x.EvaluationDate });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxTrainingEvaluation> entity)
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
