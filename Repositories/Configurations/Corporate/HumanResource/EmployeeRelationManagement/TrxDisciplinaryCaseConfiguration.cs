using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.EmployeeRelationManagement
{
    public class TrxDisciplinaryCaseConfiguration : IEntityTypeConfiguration<TrxDisciplinaryCase>
    {
        public void Configure(EntityTypeBuilder<TrxDisciplinaryCase> entity)
        {
            entity.ToTable("TrxDisciplinaryCase", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OpenedDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ClosedDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.FinalizedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsConfidential).HasDefaultValue(true);
            entity.Property(x => x.RequiresEnhancedAudit).HasDefaultValue(true);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.SubjectWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.SubjectWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SubjectEmployee)
                .WithMany()
                .HasForeignKey(x => x.SubjectEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.IncidentReport)
                .WithMany(x => x.DisciplinaryCases)
                .HasForeignKey(x => x.IncidentReportId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.EmployeeGrievance)
                .WithMany(x => x.DisciplinaryCases)
                .HasForeignKey(x => x.EmployeeGrievanceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkplaceInvestigation)
                .WithMany(x => x.DisciplinaryCases)
                .HasForeignKey(x => x.WorkplaceInvestigationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationAssignment)
                .WithMany()
                .HasForeignKey(x => x.OrganizationAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.HrOwnerUser)
                .WithMany()
                .HasForeignKey(x => x.HrOwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.CaseNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.SubjectWorkforceProfileId, x.OpenedDate });

            entity.HasIndex(x => new { x.CaseStatus, x.SeverityLevel });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxDisciplinaryCase> entity)
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
