using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models
{
    [Table("trx_disciplinary_investigation_records", Schema = "public")]
    public class TrxDisciplinaryInvestigationRecord : IdentityModel
    {
        [Key]
        [Column("investigation_id")]
        public Guid InvestigationId { get; set; } = Guid.NewGuid();

        [NotMapped]
        public Guid Id
        {
            get => InvestigationId;
            set => InvestigationId = value;
        }

        [Required]
        [Column("incident_report_id")]
        public Guid IncidentReportId { get; set; }

        [Required]
        [MaxLength(64)]
        [Column("bap_number")]
        public string BapNumber { get; set; } = string.Empty;

        [Column("hearing_date")]
        public DateTime HearingDate { get; set; }

        [Required]
        [Column("investigator_names")]
        public string InvestigatorNames { get; set; } = string.Empty;

        [Column("witness_statements")]
        public string? WitnessStatements { get; set; }

        [Column("evidence_document_urls")]
        public string? EvidenceDocumentUrls { get; set; }

        [Column("employee_defense_statement")]
        public string? EmployeeDefenseStatement { get; set; }

        [Required]
        [Column("investigation_findings")]
        public string InvestigationFindings { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        [Column("recommended_action")]
        public string RecommendedAction { get; set; } = string.Empty; // Coaching, VerbalWarning, SP1, SP2, SP3, Suspension, Termination

        [Required]
        [MaxLength(32)]
        [Column("status")]
        public string Status { get; set; } = "Draft"; // Draft, Submitted, ApprovedByHrDirector, Rejected

        public TrxDisciplinaryIncidentReport? IncidentReport { get; set; }
        public ICollection<TrxDisciplinarySanction> Sanctions { get; set; } = new List<TrxDisciplinarySanction>();
        public ICollection<TrxDisciplinarySuspension> Suspensions { get; set; } = new List<TrxDisciplinarySuspension>();
    }
}
