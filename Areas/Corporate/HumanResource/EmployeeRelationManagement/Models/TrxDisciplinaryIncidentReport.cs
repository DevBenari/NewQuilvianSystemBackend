using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models
{
    [Table("trx_disciplinary_incident_reports", Schema = "public")]
    public class TrxDisciplinaryIncidentReport : IdentityModel
    {
        [Key]
        [Column("incident_report_id")]
        public Guid IncidentReportId { get; set; } = Guid.NewGuid();

        [NotMapped]
        public Guid Id
        {
            get => IncidentReportId;
            set => IncidentReportId = value;
        }

        [Required]
        [MaxLength(64)]
        [Column("incident_number")]
        public string IncidentNumber { get; set; } = string.Empty;

        [Required]
        [Column("employee_id")]
        public Guid EmployeeId { get; set; }

        [Column("incident_date")]
        public DateTime IncidentDate { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("incident_location")]
        public string IncidentLocation { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        [Column("category")]
        public string Category { get; set; } = string.Empty;

        [Required]
        [MaxLength(32)]
        [Column("just_culture_classification")]
        public string JustCultureClassification { get; set; } = "HumanError"; // HumanError, AtRiskBehavior, RecklessMisconduct

        [Required]
        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(32)]
        [Column("reporter_type")]
        public string ReporterType { get; set; } = "Superior"; // Self, Superior, Peer, Committee, PatientGrievance

        [Column("reporter_employee_id")]
        public Guid? ReporterEmployeeId { get; set; }

        [MaxLength(64)]
        [Column("kmkp_audit_reference_number")]
        public string? KmkpAuditReferenceNumber { get; set; }

        [Required]
        [MaxLength(32)]
        [Column("status")]
        public string Status { get; set; } = "Reported"; // Reported, Investigating, ResolvedNoSanction, SanctionRecommended

        public ICollection<TrxDisciplinaryInvestigationRecord> InvestigationRecords { get; set; } = new List<TrxDisciplinaryInvestigationRecord>();
    }
}
