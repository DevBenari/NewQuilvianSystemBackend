using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.EmployeeRelationManagement
{
    public class HrdDisciplinaryIncidentReportConfiguration : IEntityTypeConfiguration<HrdDisciplinaryIncidentReport>
    {
        public void Configure(EntityTypeBuilder<HrdDisciplinaryIncidentReport> entity)
        {
            entity.ToTable("HrdDisciplinaryIncidentReport", "public");
            entity.HasKey(x => x.IncidentReportId);

            entity.Property(x => x.IncidentReportId).HasColumnName("incident_report_id");
            entity.Property(x => x.IncidentNumber).HasColumnName("incident_number").HasMaxLength(64).IsRequired();
            entity.Property(x => x.EmployeeId).HasColumnName("employee_id").IsRequired();
            entity.Property(x => x.IncidentDate).HasColumnName("incident_date").HasColumnType("timestamp with time zone");
            entity.Property(x => x.IncidentLocation).HasColumnName("incident_location").HasMaxLength(255).IsRequired();
            entity.Property(x => x.Category).HasColumnName("category").HasMaxLength(64).IsRequired();
            entity.Property(x => x.JustCultureClassification).HasColumnName("just_culture_classification").HasMaxLength(32).HasDefaultValue("HumanError").IsRequired();
            entity.Property(x => x.Description).HasColumnName("description").IsRequired();
            entity.Property(x => x.ReporterType).HasColumnName("reporter_type").HasMaxLength(32).IsRequired();
            entity.Property(x => x.ReporterEmployeeId).HasColumnName("reporter_employee_id");
            entity.Property(x => x.KmkpAuditReferenceNumber).HasColumnName("kmkp_audit_reference_number").HasMaxLength(64);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32).HasDefaultValue("Reported").IsRequired();

            entity.HasIndex(x => x.IncidentNumber).IsUnique();
            entity.HasIndex(x => x.EmployeeId);
            entity.HasIndex(x => x.Status);
        }
    }
}
