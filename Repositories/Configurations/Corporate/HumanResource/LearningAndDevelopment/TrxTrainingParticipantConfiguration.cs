using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LearningAndDevelopment
{
    public class TrxTrainingParticipantConfiguration : IEntityTypeConfiguration<TrxTrainingParticipant>
    {
        public void Configure(EntityTypeBuilder<TrxTrainingParticipant> entity)
        {
            entity.ToTable("TrxTrainingParticipant", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.EnrollmentDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.AttendancePercentage).HasPrecision(5, 2);
            entity.Property(x => x.FinalScore).HasPrecision(18, 4);
            entity.Property(x => x.IsMandatory).HasDefaultValue(false);
            entity.Property(x => x.AttendancePercentage).HasDefaultValue(0m);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.TrainingPlan)
                .WithMany()
                .HasForeignKey(x => x.TrainingPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TrainingSession)
                .WithMany(x => x.Participants)
                .HasForeignKey(x => x.TrainingSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationAssignment)
                .WithMany()
                .HasForeignKey(x => x.OrganizationAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.EnrollmentRequest)
                .WithMany()
                .HasForeignKey(x => x.EnrollmentRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.NominatedByUser)
                .WithMany()
                .HasForeignKey(x => x.NominatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.TrainingSessionId, x.WorkforceProfileId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.ParticipantStatus });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxTrainingParticipant> entity)
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
