using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class TrxPerformanceCycleConfiguration : IEntityTypeConfiguration<TrxPerformanceCycle>
    {
        public void Configure(EntityTypeBuilder<TrxPerformanceCycle> entity)
        {
            entity.ToTable("TrxPerformanceCycle", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CycleStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CycleEndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.GoalSettingDeadline).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SelfAssessmentDeadline).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ManagerAssessmentDeadline).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CalibrationDeadline).HasColumnType("timestamp with time zone");
            entity.Property(x => x.PublishedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ClosedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CycleConfigurationJson).HasColumnType("jsonb");
            entity.Property(x => x.PopulationSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.AllowEmployeeViewFinalResult).HasDefaultValue(false);
            entity.Property(x => x.RequireSelfAssessment).HasDefaultValue(true);
            entity.Property(x => x.EnablePeerFeedback).HasDefaultValue(false);
            entity.Property(x => x.TotalEmployee).HasDefaultValue(0);
            entity.Property(x => x.CompletedEmployee).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.MasterPerformanceCycle)
                .WithMany()
                .HasForeignKey(x => x.MasterPerformanceCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PerformanceTemplate)
                .WithMany()
                .HasForeignKey(x => x.PerformanceTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RatingScale)
                .WithMany()
                .HasForeignKey(x => x.RatingScaleId)
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

            entity.HasOne(x => x.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PublishedByUser)
                .WithMany()
                .HasForeignKey(x => x.PublishedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ClosedByUser)
                .WithMany()
                .HasForeignKey(x => x.ClosedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.CycleCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.CycleYear, x.CycleStatus });

            entity.HasIndex(x => new { x.CycleStartDate, x.CycleEndDate });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxPerformanceCycle> entity)
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
