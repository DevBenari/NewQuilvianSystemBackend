using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class TrxAnnualManpowerPlanConfiguration : IEntityTypeConfiguration<TrxAnnualManpowerPlan>
    {
        public void Configure(EntityTypeBuilder<TrxAnnualManpowerPlan> builder)
        {
            builder.ToTable("TrxAnnualManpowerPlan", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.PlanNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.PlanName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.PlanStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            builder.Property(x => x.TotalCurrentHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.TotalTargetHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.TotalRequestedHeadcount).HasPrecision(18, 2);
            builder.Property(x => x.TotalEstimatedAnnualCost).HasPrecision(18, 2);
            builder.Property(x => x.ApprovedBudgetAmount).HasPrecision(18, 2);
            builder.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ClosedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SubmittedByUser).WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.PlanNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.PlanYear, x.LegalEntityId, x.HospitalSiteId, x.PlanStatus });
            builder.HasIndex(x => x.WorkflowInstanceId);

        }
    }
}
