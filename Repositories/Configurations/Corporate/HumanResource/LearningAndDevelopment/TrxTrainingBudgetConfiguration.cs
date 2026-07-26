using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LearningAndDevelopment
{
    public class TrxTrainingBudgetConfiguration : IEntityTypeConfiguration<TrxTrainingBudget>
    {
        public void Configure(EntityTypeBuilder<TrxTrainingBudget> entity)
        {
            entity.ToTable("TrxTrainingBudget", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.AllocatedAmount).HasPrecision(18, 2);
            entity.Property(x => x.CommittedAmount).HasPrecision(18, 2);
            entity.Property(x => x.UsedAmount).HasPrecision(18, 2);
            entity.Property(x => x.RemainingAmount).HasPrecision(18, 2);
            entity.Property(x => x.AllocatedAmount).HasDefaultValue(0m);
            entity.Property(x => x.CommittedAmount).HasDefaultValue(0m);
            entity.Property(x => x.UsedAmount).HasDefaultValue(0m);
            entity.Property(x => x.RemainingAmount).HasDefaultValue(0m);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.TrainingPlan)
                .WithMany(x => x.Budgets)
                .HasForeignKey(x => x.TrainingPlanId)
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

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.TrainingPlanId, x.CostCenterId, x.FiscalYear })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.FiscalYear, x.BudgetStatus });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxTrainingBudget> entity)
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
