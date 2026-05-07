using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Administrator.UserManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Employee.Models;
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

        public Gender? Gender { get; set; }

        [MaxLength(100)]
        public string? BirthPlace { get; set; }

        public DateTime? BirthDate { get; set; }

        public Religion Religion { get; set; } = Religion.Unknown;

        public MaritalStatus MaritalStatus { get; set; } = MaritalStatus.Unknown;

        public BloodType BloodType { get; set; } = BloodType.Unknown;

        [MaxLength(50)]
        public string? IdentityType { get; set; }

        [MaxLength(16)]
        public string? IdentityNumber { get; set; }

        [MaxLength(13)]
        public string? PhoneNumber { get; set; }

        [MaxLength(13)]
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

        public Guid PrimaryDepartmentId { get; set; }

        public Guid PrimaryPositionId { get; set; }

        public EmployeeStatus EmployeeStatus { get; set; } = EmployeeStatus.Contract;

        public EmployeeProfessionType ProfessionType { get; set; } = EmployeeProfessionType.GeneralStaff;

        [MaxLength(50)]
        public string? EmploymentType { get; set; }

        [MaxLength(50)]
        public string? GradeLevel { get; set; }

        [MaxLength(50)]
        public string? WorkLocation { get; set; }

        public DateTime? JoinDate { get; set; }

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

        public MstDepartment? PrimaryDepartment { get; set; }

        public MstPosition? PrimaryPosition { get; set; }

        public MstCountry? Country { get; set; }

        public MstProvince? Province { get; set; }

        public MstCity? City { get; set; }

        public MstDistrict? District { get; set; }

        public MstPostalCode? PostalCode { get; set; }

        public ICollection<EmpBankAccount> BankAccounts { get; set; } = new List<EmpBankAccount>();

        public EmpPayrollProfile? PayrollProfile { get; set; }

        public EmpTaxProfile? TaxProfile { get; set; }

        public EmpInsuranceProfile? InsuranceProfile { get; set; }

        public ICollection<EmpDocument> Documents { get; set; } = new List<EmpDocument>();

        public EmpTransportAllowanceProfile? TransportAllowanceProfile { get; set; }

        public ICollection<EmpTransportAllowanceTransaction> TransportAllowanceTransactions { get; set; }
            = new List<EmpTransportAllowanceTransaction>();
    }
}