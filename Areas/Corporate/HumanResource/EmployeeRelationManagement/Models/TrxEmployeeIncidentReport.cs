using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models
{
    [Table("TrxEmployeeIncidentReport", Schema = "public")]
    public class TrxEmployeeIncidentReport : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? ReporterWorkforceProfileId { get; set; }
        public Guid? ReporterUserId { get; set; }
        public Guid? SubjectWorkforceProfileId { get; set; }
        public Guid? SubjectEmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? AssignedInvestigatorUserId { get; set; }

        [Required]
        [MaxLength(60)]
        public string IncidentNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string IncidentType { get; set; } = string.Empty;

        public DateTime IncidentDateTime { get; set; }

        [MaxLength(300)]
        public string? IncidentLocation { get; set; }

        [Required]
        [MaxLength(500)]
        public string IncidentSummary { get; set; } = string.Empty;

        [MaxLength(5000)]
        public string? IncidentDetailsRestricted { get; set; }

        [Required]
        [MaxLength(30)]
        public string SeverityLevel { get; set; } = "Medium";

        [Required]
        [MaxLength(40)]
        public string IncidentStatus { get; set; } = "Draft";

        public bool IsAnonymousReport { get; set; } = false;
        public bool IsReporterIdentityProtected { get; set; } = true;
        public bool IsConfidential { get; set; } = true;
        [Required]
        [MaxLength(30)]
        public string AccessClassification { get; set; } = "HighlyRestricted";
        public bool RequiresEnhancedAudit { get; set; } = true;

        public string? AttachmentMetadataJson { get; set; }

        public DateTime? SubmittedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? ReporterWorkforceProfile { get; set; }
        public ApplicationUser? ReporterUser { get; set; }
        public MstWorkforceProfile? SubjectWorkforceProfile { get; set; }
        public MstEmployee? SubjectEmployee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstDepartment? Department { get; set; }
        public ApplicationUser? AssignedInvestigatorUser { get; set; }

        public ICollection<TrxWorkplaceInvestigation> Investigations { get; set; } = new List<TrxWorkplaceInvestigation>();
        public ICollection<TrxDisciplinaryCase> DisciplinaryCases { get; set; } = new List<TrxDisciplinaryCase>();
        public ICollection<WfpDisciplinaryAction> DisciplinaryActions { get; set; } = new List<WfpDisciplinaryAction>();
    }
}
