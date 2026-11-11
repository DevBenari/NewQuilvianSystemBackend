using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.EmployeeRelationManagement
{
    public class TrxDisciplinaryInvestigationRecordConfiguration : IEntityTypeConfiguration<TrxDisciplinaryInvestigationRecord>
    {
        public void Configure(EntityTypeBuilder<TrxDisciplinaryInvestigationRecord> entity)
        {
            entity.ToTable("trx_disciplinary_investigation_records", "public");
            entity.HasKey(x => x.InvestigationId);

            entity.Property(x => x.InvestigationId).HasColumnName("investigation_id");
            entity.Property(x => x.IncidentReportId).HasColumnName("incident_report_id").IsRequired();
            entity.Property(x => x.BapNumber).HasColumnName("bap_number").HasMaxLength(64).IsRequired();
            entity.Property(x => x.HearingDate).HasColumnName("hearing_date").HasColumnType("timestamp with time zone");
            entity.Property(x => x.InvestigatorNames).HasColumnName("investigator_names").IsRequired();
            entity.Property(x => x.WitnessStatements).HasColumnName("witness_statements");
            entity.Property(x => x.EvidenceDocumentUrls).HasColumnName("evidence_document_urls");
            entity.Property(x => x.EmployeeDefenseStatement).HasColumnName("employee_defense_statement");
            entity.Property(x => x.InvestigationFindings).HasColumnName("investigation_findings").IsRequired();
            entity.Property(x => x.RecommendedAction).HasColumnName("recommended_action").HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32).HasDefaultValue("Draft").IsRequired();

            entity.HasIndex(x => x.BapNumber).IsUnique();
            entity.HasIndex(x => x.IncidentReportId);

            entity.HasOne(x => x.IncidentReport)
                .WithMany(x => x.InvestigationRecords)
                .HasForeignKey(x => x.IncidentReportId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
