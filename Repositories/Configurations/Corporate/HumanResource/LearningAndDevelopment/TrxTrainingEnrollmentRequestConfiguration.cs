using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LearningAndDevelopment
{
    public class TrxTrainingEnrollmentRequestConfiguration : IEntityTypeConfiguration<TrxTrainingEnrollmentRequest>
    {
        public void Configure(EntityTypeBuilder<TrxTrainingEnrollmentRequest> entity)
        {
            entity.ToTable("TrxTrainingEnrollmentRequest", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RequestDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ExternalStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ExternalEndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ManagerActionAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.RequestedCost).HasPrecision(18, 2);
            entity.Property(x => x.RequestedCost).HasDefaultValue(0m);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.TrainingPlan)
                .WithMany(x => x.EnrollmentRequests)
                .HasForeignKey(x => x.TrainingPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TrainingSession)
                .WithMany()
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

            entity.HasOne(x => x.ManagerUser)
                .WithMany()
                .HasForeignKey(x => x.ManagerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SubmittedByUser)
                .WithMany()
                .HasForeignKey(x => x.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ManagerActionByUser)
                .WithMany()
                .HasForeignKey(x => x.ManagerActionByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.RequestNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.RequestStatus, x.RequestDate });

            entity.HasIndex(x => new { x.TrainingSessionId, x.WorkforceProfileId })
                .IsUnique()
                .HasFilter("\"TrainingSessionId\" IS NOT NULL AND \"IsDelete\" = false");

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxTrainingEnrollmentRequest> entity)
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
