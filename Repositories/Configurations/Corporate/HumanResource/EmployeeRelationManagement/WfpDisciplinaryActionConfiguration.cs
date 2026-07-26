using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.EmployeeRelationManagement
{
    public class WfpDisciplinaryActionConfiguration : IEntityTypeConfiguration<WfpDisciplinaryAction>
    {
        public void Configure(EntityTypeBuilder<WfpDisciplinaryAction> entity)
        {
            entity.ToTable("WfpDisciplinaryAction", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ActionDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.AcknowledgedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsAcknowledged).HasDefaultValue(false);
            entity.Property(x => x.IsAppealed).HasDefaultValue(false);
            entity.Property(x => x.IsConfidential).HasDefaultValue(true);
            entity.Property(x => x.RequiresEnhancedAudit).HasDefaultValue(true);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany(x => x.DisciplinaryActions)
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

            entity.HasOne(x => x.DisciplinaryCase)
                .WithMany(x => x.DisciplinaryActions)
                .HasForeignKey(x => x.DisciplinaryCaseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DisciplinaryDecision)
                .WithMany(x => x.DisciplinaryActions)
                .HasForeignKey(x => x.DisciplinaryDecisionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.IncidentReport)
                .WithMany(x => x.DisciplinaryActions)
                .HasForeignKey(x => x.IncidentReportId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.IssuedByUser)
                .WithMany()
                .HasForeignKey(x => x.IssuedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ActionCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.ActionDate });

            entity.HasIndex(x => new { x.ActionStatus, x.EffectiveEndDate });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<WfpDisciplinaryAction> entity)
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
