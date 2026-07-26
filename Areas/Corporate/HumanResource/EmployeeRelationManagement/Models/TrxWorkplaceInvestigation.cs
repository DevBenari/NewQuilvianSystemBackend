using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models
{
    [Table("TrxWorkplaceInvestigation", Schema = "public")]
    public class TrxWorkplaceInvestigation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? IncidentReportId { get; set; }
        public Guid? EmployeeGrievanceId { get; set; }
        public Guid? LeadInvestigatorUserId { get; set; }

        [Required]
        [MaxLength(60)]
        public string InvestigationNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string InvestigationTitle { get; set; } = string.Empty;

        [MaxLength(3000)]
        public string? InvestigationScope { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Required]
        [MaxLength(40)]
        public string InvestigationStatus { get; set; } = "Open";

        [MaxLength(5000)]
        public string? FindingsRestricted { get; set; }
        [MaxLength(3000)]
        public string? RecommendationRestricted { get; set; }
        [MaxLength(1500)]
        public string? AdministrativeConclusion { get; set; }

        public bool IsConfidential { get; set; } = true;
        [Required]
        [MaxLength(30)]
        public string AccessClassification { get; set; } = "HighlyRestricted";
        public bool RequiresEnhancedAudit { get; set; } = true;

        [MaxLength(500)]
        public string? InvestigationReportFilePath { get; set; }
        [MaxLength(100)]
        public string? InvestigationReportContentType { get; set; }

        public Guid? ClosedByUserId { get; set; }
        public DateTime? ClosedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public TrxEmployeeIncidentReport? IncidentReport { get; set; }
        public TrxEmployeeGrievance? EmployeeGrievance { get; set; }
        public ApplicationUser? LeadInvestigatorUser { get; set; }
        public ApplicationUser? ClosedByUser { get; set; }

        public ICollection<TrxInvestigationEvidence> EvidenceItems { get; set; } = new List<TrxInvestigationEvidence>();
        public ICollection<TrxDisciplinaryCase> DisciplinaryCases { get; set; } = new List<TrxDisciplinaryCase>();
    }
}
