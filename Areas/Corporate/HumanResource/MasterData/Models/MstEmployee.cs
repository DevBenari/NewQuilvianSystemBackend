using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models
{
    [Table("MstEmployee", Schema = "public")]
    public class MstEmployee : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        [MaxLength(50)]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? NickName { get; set; }        

        [MaxLength(100)]
        public string? BirthPlace { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        public Gender? Gender { get; set; }

        public Religion Religion { get; set; } = Religion.Unknown;

        public MaritalStatus MaritalStatus { get; set; } = MaritalStatus.Unknown;

        public BloodType BloodType { get; set; } = BloodType.Unknown;

        [Required]
        [MaxLength(50)]
        public string IdentityType { get; set; } = string.Empty;

        [Required]
        [MaxLength(16)]
        public string IdentityNumber { get; set; } = string.Empty;

        [MaxLength(13)]
        public string? PhoneNumber { get; set; }

        [MaxLength(13)]
        public string? WhatsAppNumber { get; set; }

        [Required]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Address { get; set; }

        public Guid? CountryId { get; set; }

        public Guid? ProvinceId { get; set; }

        public Guid? CityId { get; set; }

        public Guid? DistrictId { get; set; }

        public Guid? PostalCodeId { get; set; }

        [Required]
        public Guid PrimaryDepartmentId { get; set; }

        [Required]
        public Guid PrimaryPositionId { get; set; }

        public EmployeeStatus EmployeeStatus { get; set; } = EmployeeStatus.Active;

        public EmployeeProfessionType ProfessionType { get; set; } = EmployeeProfessionType.GeneralStaff;

        public EmploymentType EmploymentType { get; set; } = EmploymentType.Contract;

        [MaxLength(50)]
        public string? GradeLevel { get; set; }

        [MaxLength(50)]
        public string? WorkLocation { get; set; }

        [Required]
        public DateTime JoinDate { get; set; }

        public DateTime? ProbationEndDate { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public DateTime? ResignDate { get; set; }

        [MaxLength(250)]
        public string? ResignReason { get; set; }

        [MaxLength(200)]
        public string? EmergencyContactName { get; set; }

        [MaxLength(50)]
        public string? EmergencyContactRelation { get; set; }

        [MaxLength(13)]
        public string? EmergencyContactPhone { get; set; }

        [MaxLength(500)]
        public string? EmergencyContactAddress { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public MstDepartment? PrimaryDepartment { get; set; }

        public MstPosition? PrimaryPosition { get; set; }

        public MstCountry? Country { get; set; }

        public MstProvince? Province { get; set; }

        public MstCity? City { get; set; }

        public MstDistrict? District { get; set; }

        public MstPostalCode? PostalCode { get; set; }
    }
}