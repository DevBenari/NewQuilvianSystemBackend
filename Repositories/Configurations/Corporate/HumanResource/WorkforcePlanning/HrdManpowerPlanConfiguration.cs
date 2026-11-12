using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class HrdManpowerPlanConfiguration : IEntityTypeConfiguration<HrdManpowerPlan>
    {
        public void Configure(EntityTypeBuilder<HrdManpowerPlan> builder)
        {
            builder.ToTable("HrdManpowerPlan", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.PlanNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.PeriodStartDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.PeriodEndDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.ReferenceDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.Justification).HasMaxLength(2000);
            builder.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            builder.Property(x => x.TotalEstimatedCost).HasPrecision(18, 2);
            builder.Property(x => x.FinanceAdjustedTotal).HasPrecision(18, 2);
            builder.Property(x => x.FinanceAdjustmentReason).HasMaxLength(1000);
            builder.Property(x => x.FinanceAdjustedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");

            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ParentPlan)
                .WithMany()
                .HasForeignKey(x => x.ParentPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.PlanNumber)
                .IsUnique()
                .HasDatabaseName("IX_HrdManpowerPlan_PlanNumber");

            builder.HasIndex(x => new { x.HospitalSiteId, x.PlanYear })
                .IsUnique()
                .HasFilter("\"PlanKind\" = 1 AND \"PlanStatus\" NOT IN (4, 6) AND \"IsDelete\" = false")
                .HasDatabaseName("IX_HrdManpowerPlan_Site_Year_Annual");

            builder.HasIndex(x => new { x.ParentPlanId, x.AddendumSequence })
                .IsUnique()
                .HasFilter("\"ParentPlanId\" IS NOT NULL")
                .HasDatabaseName("IX_HrdManpowerPlan_Parent_Sequence");

            builder.HasIndex(x => x.PlanStatus);
            builder.HasIndex(x => x.WorkflowInstanceId);
        }
    }
}
