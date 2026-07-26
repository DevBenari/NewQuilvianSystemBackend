using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.EmployeeRelationManagement
{
    public class TrxEmployeeIncidentReportConfiguration : IEntityTypeConfiguration<TrxEmployeeIncidentReport>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeIncidentReport> entity)
        {
            entity.ToTable("TrxEmployeeIncidentReport", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.IncidentDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ResolvedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.AttachmentMetadataJson).HasColumnType("jsonb");
            entity.Property(x => x.IsAnonymousReport).HasDefaultValue(false);
            entity.Property(x => x.IsReporterIdentityProtected).HasDefaultValue(true);
            entity.Property(x => x.IsConfidential).HasDefaultValue(true);
            entity.Property(x => x.RequiresEnhancedAudit).HasDefaultValue(true);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.ReporterWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.ReporterWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ReporterUser)
                .WithMany()
                .HasForeignKey(x => x.ReporterUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SubjectWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.SubjectWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SubjectEmployee)
                .WithMany()
                .HasForeignKey(x => x.SubjectEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationAssignment)
                .WithMany()
                .HasForeignKey(x => x.OrganizationAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AssignedInvestigatorUser)
                .WithMany()
                .HasForeignKey(x => x.AssignedInvestigatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.IncidentNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.SubjectWorkforceProfileId, x.IncidentDateTime });

            entity.HasIndex(x => new { x.IncidentStatus, x.SeverityLevel });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxEmployeeIncidentReport> entity)
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
