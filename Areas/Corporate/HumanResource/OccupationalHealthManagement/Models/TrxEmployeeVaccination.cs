using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models
{
    [Table("TrxEmployeeVaccination", Schema = "public")]
    public class TrxEmployeeVaccination : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? HospitalSiteId { get; set; }

        [Required]
        [MaxLength(60)]
        public string VaccinationNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string VaccineType { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string VaccineName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? DiseasePrevented { get; set; }

        public int DoseNumber { get; set; } = 1;
        public DateTime? ScheduledDate { get; set; }
        public DateTime? AdministrationDate { get; set; }
        public DateTime? NextDoseDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        [MaxLength(200)]
        public string? ProviderName { get; set; }
        [MaxLength(100)]
        public string? BatchNumber { get; set; }

        [Required]
        [MaxLength(40)]
        public string VaccinationStatus { get; set; } = "Scheduled";

        public bool IsRelevantForRole { get; set; } = true;
        public bool IsMandatory { get; set; } = false;
        public bool IsCompliant { get; set; } = false;

        [MaxLength(500)]
        public string? CertificateFilePath { get; set; }
        [MaxLength(100)]
        public string? CertificateContentType { get; set; }

        public bool IsVerified { get; set; } = false;
        public Guid? VerifiedByUserId { get; set; }
        public DateTime? VerifiedAt { get; set; }

        [MaxLength(1000)]
        public string? AdministrativeNotes { get; set; }
        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }
    }
}
