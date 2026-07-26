using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LearningAndDevelopment
{
    public class TrxTrainingResultConfiguration : IEntityTypeConfiguration<TrxTrainingResult>
    {
        public void Configure(EntityTypeBuilder<TrxTrainingResult> entity)
        {
            entity.ToTable("TrxTrainingResult", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ResultDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.PreTestScore).HasPrecision(18, 4);
            entity.Property(x => x.PostTestScore).HasPrecision(18, 4);
            entity.Property(x => x.FinalScore).HasPrecision(18, 4);
            entity.Property(x => x.AttendancePercentage).HasPrecision(5, 2);
            entity.Property(x => x.CreditPointEarned).HasPrecision(18, 2);
            entity.Property(x => x.ResultSummaryJson).HasColumnType("jsonb");
            entity.Property(x => x.AttendancePercentage).HasDefaultValue(0m);
            entity.Property(x => x.IsPassed).HasDefaultValue(false);
            entity.Property(x => x.IsCompleted).HasDefaultValue(false);
            entity.Property(x => x.CreditPointEarned).HasDefaultValue(0m);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.TrainingParticipant)
                .WithMany()
                .HasForeignKey(x => x.TrainingParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TrainingSession)
                .WithMany()
                .HasForeignKey(x => x.TrainingSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TrainingRecord)
                .WithMany()
                .HasForeignKey(x => x.TrainingRecordId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.VerifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.TrainingParticipantId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.ResultDate, x.ResultStatus });

            entity.HasIndex(x => x.TrainingRecordId)
                .IsUnique()
                .HasFilter("\"TrainingRecordId\" IS NOT NULL AND \"IsDelete\" = false");

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxTrainingResult> entity)
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
