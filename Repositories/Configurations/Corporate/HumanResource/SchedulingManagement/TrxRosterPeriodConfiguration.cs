using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.SchedulingManagement
{
    public class TrxRosterPeriodConfiguration : IEntityTypeConfiguration<TrxRosterPeriod>
    {
        public void Configure(EntityTypeBuilder<TrxRosterPeriod> entity)
        {
            entity.ToTable("TrxRosterPeriod", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RosterPeriodCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RosterPeriodName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PeriodStartDate).HasColumnType("date");
            entity.Property(x => x.PeriodEndDate).HasColumnType("date");
            entity.Property(x => x.SubmissionDeadlineAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PublicationPlannedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.RosterStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.ValidatedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PublishedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.LockedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ValidationSummaryJson).HasColumnType("jsonb");
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
            entity.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RosterPolicy).WithMany().HasForeignKey(x => x.RosterPolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.MinimumRestPolicy).WithMany().HasForeignKey(x => x.MinimumRestPolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ValidatedByUser).WithMany().HasForeignKey(x => x.ValidatedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SubmittedByUser).WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PublishedByUser).WithMany().HasForeignKey(x => x.PublishedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LockedByUser).WithMany().HasForeignKey(x => x.LockedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.RosterPeriodCode).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.HospitalSiteId, x.OrganizationUnitId, x.DepartmentId, x.PeriodStartDate, x.PeriodEndDate });
            entity.HasIndex(x => new { x.RosterStatus, x.IsActive, x.IsDelete });
        }
    }
}
