using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models
{
    [Table("MstExternalUser", Schema = "public")]
    public class MstExternalUser : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ExternalCode { get; set; } = string.Empty;

        public ExternalUserStatus ExternalUserStatus { get; set; } = ExternalUserStatus.Active;

        // Pengganti ExternalUserType dan ExternalEngagementType legacy.
        [Required]
        public Guid WorkforceTypeId { get; set; }

        public Guid? EmployeeCategoryId { get; set; }

        public Guid? EmploymentTypeId { get; set; }

        public Guid? ContractTypeId { get; set; }

        public Guid? WorkerSourceId { get; set; }

        public Guid? ProfessionId { get; set; }

        public Guid? JobFamilyId { get; set; }

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? CompanyName { get; set; }

        [MaxLength(100)]
        public string? CompanyCode { get; set; }

        [MaxLength(100)]
        public string? JobTitle { get; set; }

        [MaxLength(200)]
        public string? ContactPersonName { get; set; }

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

        [MaxLength(50)]
        public string? IdentityType { get; set; }

        [MaxLength(100)]
        public string? IdentityNumber { get; set; }

        [MaxLength(100)]
        public string? TaxNumber { get; set; }

        [MaxLength(100)]
        public string? BusinessLicenseNumber { get; set; }

        public Guid? PrimaryDepartmentId { get; set; }

        public Guid? PrimaryPositionId { get; set; }

        [MaxLength(50)]
        public string? WorkLocation { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public DateTime? AccessStartDate { get; set; }

        public DateTime? AccessEndDate { get; set; }

        [MaxLength(250)]
        public string? AccessPurpose { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public MstDepartment? PrimaryDepartment { get; set; }

        public MstPosition? PrimaryPosition { get; set; }

        public MstWorkforceType? WorkforceType { get; set; }

        public MstEmployeeCategory? EmployeeCategory { get; set; }

        public MstEmploymentType? EmploymentType { get; set; }

        public MstContractType? ContractType { get; set; }

        public MstWorkerSource? WorkerSource { get; set; }

        public MstProfession? Profession { get; set; }

        public MstJobFamily? JobFamily { get; set; }

        public MstCountry? Country { get; set; }

        public MstProvince? Province { get; set; }

        public MstCity? City { get; set; }

        public MstDistrict? District { get; set; }

        public MstPostalCode? PostalCode { get; set; }
    }
}
