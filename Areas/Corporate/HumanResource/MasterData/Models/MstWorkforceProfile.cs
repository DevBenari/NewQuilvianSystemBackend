using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models
{
    [Table("MstWorkforceProfile", Schema = "public")]
    public class MstWorkforceProfile : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ProfileCode { get; set; } = string.Empty;

        [Required]
        public UserType UserType { get; set; }

        [Required]
        [MaxLength(200)]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        [MaxLength(30)]
        public string? WhatsAppNumber { get; set; }

        public Guid? PrimaryDepartmentId { get; set; }

        public Guid? PrimaryPositionId { get; set; }

        public bool IsActive { get; set; } = true;

        public MstDepartment? PrimaryDepartment { get; set; }

        public MstPosition? PrimaryPosition { get; set; }

        public MstEmployee? Employee { get; set; }

        public MstDoctor? Doctor { get; set; }

        public MstExternalUser? ExternalUser { get; set; }

        public ApplicationUser? UserAccount { get; set; }

        public ICollection<WfpOrganizationAssignment> OrganizationAssignments { get; set; }
            = new List<WfpOrganizationAssignment>();

        public ICollection<WfpBankAccount> BankAccounts { get; set; }
            = new List<WfpBankAccount>();

        public ICollection<WfpDocument> Documents { get; set; }
            = new List<WfpDocument>();

        public ICollection<WfpEducation> Educations { get; set; }
            = new List<WfpEducation>();

        public ICollection<WfpTrainingRecord> TrainingRecords { get; set; }
            = new List<WfpTrainingRecord>();

        public ICollection<WfpCertification> Certifications { get; set; }
            = new List<WfpCertification>();

        public ICollection<WfpCredentialLicense> CredentialLicenses { get; set; }
            = new List<WfpCredentialLicense>();

        public WfpTransportAllowance? TransportAllowance { get; set; }

        public ICollection<WfpTransportAllowanceTransaction> TransportAllowanceTransactions { get; set; } = new List<WfpTransportAllowanceTransaction>();

        public WfpPayroll? Payroll { get; set; }

        public WfpTax? Tax { get; set; }

        public WfpInsurance? Insurance { get; set; }

        public ICollection<WfpWorkScheduleAssignment> WorkScheduleAssignments { get; set; } = new List<WfpWorkScheduleAssignment>();
    }
}