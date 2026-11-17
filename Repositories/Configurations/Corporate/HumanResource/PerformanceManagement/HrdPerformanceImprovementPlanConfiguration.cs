using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class HrdPerformanceImprovementPlanConfiguration : IEntityTypeConfiguration<HrdPerformanceImprovementPlan>
    {
        public void Configure(EntityTypeBuilder<HrdPerformanceImprovementPlan> entity)
        {
            entity.ToTable("HrdPerformanceImprovementPlan", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PipNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.StartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ActionPlanSummary).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue("Draft");
            entity.Property(x => x.FinalVerdict).HasMaxLength(30);

            entity.Property(x => x.ObjectivesJson).HasColumnType("jsonb");
            entity.Property(x => x.ActionPlanJson).HasColumnType("jsonb");
            entity.Property(x => x.SuccessMetricsJson).HasColumnType("jsonb");
            entity.Property(x => x.ProgressPercentage).HasColumnType("decimal(5,2)").HasDefaultValue(0m);
            entity.Property(x => x.Outcome).HasMaxLength(100);
            entity.Property(x => x.OutcomeNote).HasMaxLength(3000);
            entity.Property(x => x.EmployeeAcknowledgedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.PerformanceAppraisal)
                .WithMany()
                .HasForeignKey(x => x.PerformanceAppraisalId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.MentorWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.MentorWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PipNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.PerformanceAppraisalId);
            entity.HasIndex(x => x.WorkforceProfileId);
            entity.HasIndex(x => x.MentorWorkforceProfileId);
            entity.HasIndex(x => x.Status);

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<HrdPerformanceImprovementPlan> entity)
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
