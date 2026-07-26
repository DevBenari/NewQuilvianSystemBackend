using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LearningAndDevelopment
{
    public class TrxTrainingPlanConfiguration : IEntityTypeConfiguration<TrxTrainingPlan>
    {
        public void Configure(EntityTypeBuilder<TrxTrainingPlan> entity)
        {
            entity.ToTable("TrxTrainingPlan", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PlannedStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.PlannedEndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EstimatedCost).HasPrecision(18, 2);
            entity.Property(x => x.ApprovedBudget).HasPrecision(18, 2);
            entity.Property(x => x.PlanningSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.IsMandatory).HasDefaultValue(false);
            entity.Property(x => x.IsExternalTraining).HasDefaultValue(false);
            entity.Property(x => x.PlannedParticipantCount).HasDefaultValue(0);
            entity.Property(x => x.ApprovedParticipantCount).HasDefaultValue(0);
            entity.Property(x => x.EstimatedCost).HasDefaultValue(0m);
            entity.Property(x => x.ApprovedBudget).HasDefaultValue(0m);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.TrainingCatalog)
                .WithMany()
                .HasForeignKey(x => x.TrainingCatalogId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TrainingCategory)
                .WithMany()
                .HasForeignKey(x => x.TrainingCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.MandatoryTrainingRule)
                .WithMany()
                .HasForeignKey(x => x.MandatoryTrainingRuleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LegalEntity)
                .WithMany()
                .HasForeignKey(x => x.LegalEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CostCenter)
                .WithMany()
                .HasForeignKey(x => x.CostCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SubmittedByUser)
                .WithMany()
                .HasForeignKey(x => x.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.TrainingPlanNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.PlanYear, x.PlanStatus });

            entity.HasIndex(x => new { x.DepartmentId, x.PlannedStartDate, x.PlannedEndDate });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxTrainingPlan> entity)
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
