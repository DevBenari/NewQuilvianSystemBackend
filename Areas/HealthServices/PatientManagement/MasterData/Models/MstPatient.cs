using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models
{
    [Table("MstPatient", Schema = "public")]
    public class MstPatient : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string PatientCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string MedicalRecordNumber { get; set; } = string.Empty;

        public PatientType PatientType { get; set; } = PatientType.General;

        public PatientStatus PatientStatus { get; set; } = PatientStatus.Active;

        public PatientRegistrationSource RegistrationSource { get; set; } = PatientRegistrationSource.Unknown;

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? NickName { get; set; }

        [MaxLength(100)]
        public string? BirthPlace { get; set; }

        public DateTime? BirthDate { get; set; }

        public Gender? Gender { get; set; }

        public Religion Religion { get; set; } = Religion.Unknown;

        public MaritalStatus MaritalStatus { get; set; } = MaritalStatus.Unknown;

        public BloodType BloodType { get; set; } = BloodType.Unknown;

        [MaxLength(50)]
        public string? IdentityType { get; set; }

        [MaxLength(50)]
        public string? IdentityNumber { get; set; }

        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        [MaxLength(30)]
        public string? WhatsAppNumber { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public Guid? CountryId { get; set; }

        public Guid? ProvinceId { get; set; }

        public Guid? CityId { get; set; }

        public Guid? DistrictId { get; set; }

        public Guid? PostalCodeId { get; set; }

        [MaxLength(500)]
        public string? PhotoPath { get; set; }

        public bool IsMember { get; set; } = true;

        public Guid? DefaultMembershipTierId { get; set; }

        public Guid? ActivePatientMembershipId { get; set; }

        public bool IsNewborn { get; set; } = false;

        public Guid? MotherPatientId { get; set; }

        public int? BirthOrder { get; set; }

        public decimal? BirthWeightGram { get; set; }

        public decimal? BirthLengthCm { get; set; }

        public TimeSpan? BirthTime { get; set; }

        [MaxLength(100)]
        public string? DeliveryMethod { get; set; }

        public bool IsDeceased { get; set; } = false;

        public DateTime? DeceasedDate { get; set; }

        public Guid? MergedToPatientId { get; set; }

        [MaxLength(250)]
        public string? MergeReason { get; set; }

        [MaxLength(250)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstCountry? Country { get; set; }

        public MstProvince? Province { get; set; }

        public MstCity? City { get; set; }

        public MstDistrict? District { get; set; }

        public MstPostalCode? PostalCode { get; set; }
        public MstMembershipTier? DefaultMembershipTier { get; set; }

        public MstPatient? MotherPatient { get; set; }

        public MstPatient? MergedToPatient { get; set; }
    }
}
