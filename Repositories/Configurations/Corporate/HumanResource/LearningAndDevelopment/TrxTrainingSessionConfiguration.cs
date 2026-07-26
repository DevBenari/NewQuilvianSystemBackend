using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LearningAndDevelopment
{
    public class TrxTrainingSessionConfiguration : IEntityTypeConfiguration<TrxTrainingSession>
    {
        public void Configure(EntityTypeBuilder<TrxTrainingSession> entity)
        {
            entity.ToTable("TrxTrainingSession", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.StartDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EndDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.RegistrationOpenAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.RegistrationCloseAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.MinimumPassingScore).HasPrecision(18, 4);
            entity.Property(x => x.AgendaJson).HasColumnType("jsonb");
            entity.Property(x => x.Sequence).HasDefaultValue(1);
            entity.Property(x => x.Capacity).HasDefaultValue(0);
            entity.Property(x => x.MinimumParticipant).HasDefaultValue(0);
            entity.Property(x => x.RequiresAttendance).HasDefaultValue(true);
            entity.Property(x => x.RequiresPreTest).HasDefaultValue(false);
            entity.Property(x => x.RequiresPostTest).HasDefaultValue(false);
            entity.Property(x => x.GeneratesCertificate).HasDefaultValue(false);
            entity.Property(x => x.MinimumPassingScore).HasDefaultValue(0m);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.TrainingPlan)
                .WithMany(x => x.Sessions)
                .HasForeignKey(x => x.TrainingPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TrainingCatalog)
                .WithMany()
                .HasForeignKey(x => x.TrainingCatalogId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.InstructorUser)
                .WithMany()
                .HasForeignKey(x => x.InstructorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.TrainingPlanId, x.Sequence })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.StartDateTime, x.EndDateTime, x.SessionStatus });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxTrainingSession> entity)
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
