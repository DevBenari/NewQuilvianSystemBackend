using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models
{
    [Table("HrdEmployeeRecognition", Schema = "public")]
    public class HrdEmployeeRecognition : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? CostCenterId { get; set; }

        [Required]
        [MaxLength(60)]
        public string RecognitionNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string RecognitionType { get; set; } = string.Empty;

        public DateTime RecognitionDate { get; set; }

        [Required]
        [MaxLength(250)]
        public string RecognitionTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(2500)]
        public string RecognitionReason { get; set; } = string.Empty;

        [MaxLength(1500)]
        public string? AwardDescription { get; set; }

        public decimal MonetaryValue { get; set; } = 0m;
        [Required]
        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        [Required]
        [MaxLength(40)]
        public string RecognitionStatus { get; set; } = "Draft";

        public bool HasPublicationConsent { get; set; } = false;
        public bool IsConfidential { get; set; } = false;
        [Required]
        [MaxLength(30)]
        public string AccessClassification { get; set; } = "Internal";

        public Guid? NominatedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? PresentedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstDepartment? Department { get; set; }
        public MstCostCenter? CostCenter { get; set; }
        public ApplicationUser? NominatedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
