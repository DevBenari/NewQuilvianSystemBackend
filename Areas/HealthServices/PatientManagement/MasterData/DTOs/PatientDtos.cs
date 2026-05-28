using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs
{
    public class PatientSummaryResponse
    {
        public int TotalPatient { get; set; }
        public int ActivePatient { get; set; }
        public int InactivePatient { get; set; }
        public int GeneralPatient { get; set; }
        public int NewbornPatient { get; set; }
        public int MemberPatient { get; set; }
        public int DeceasedPatient { get; set; }
        public int MergedPatient { get; set; }
    }

    public class PatientResponse
    {
        public Guid Id { get; set; }
        public string PatientCode { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public PatientType PatientType { get; set; }
        public PatientStatus PatientStatus { get; set; }
        public PatientRegistrationSource RegistrationSource { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? NickName { get; set; }
        public string? BirthPlace { get; set; }
        public DateTime? BirthDate { get; set; }
        public Gender? Gender { get; set; }
        public Religion Religion { get; set; }
        public MaritalStatus MaritalStatus { get; set; }
        public BloodType BloodType { get; set; }
        public string? IdentityType { get; set; }
        public string? IdentityNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? Email { get; set; }
        public Guid? CountryId { get; set; }
        public string? CountryName { get; set; }
        public Guid? ProvinceId { get; set; }
        public string? ProvinceName { get; set; }
        public Guid? CityId { get; set; }
        public string? CityName { get; set; }
        public Guid? DistrictId { get; set; }
        public string? DistrictName { get; set; }
        public Guid? PostalCodeId { get; set; }
        public string? PostalCode { get; set; }
        public string? PhotoPath { get; set; }
        public bool IsMember { get; set; }
        public Guid? DefaultMembershipTierId { get; set; }
        public string? DefaultMembershipTierName { get; set; }
        public Guid? ActivePatientMembershipId { get; set; }
        public bool IsNewborn { get; set; }
        public Guid? MotherPatientId { get; set; }
        public string? MotherMedicalRecordNumber { get; set; }
        public string? MotherPatientName { get; set; }
        public int? BirthOrder { get; set; }
        public bool IsDeceased { get; set; }
        public DateTime? DeceasedDate { get; set; }
        public Guid? MergedToPatientId { get; set; }
        public string? MergedToMedicalRecordNumber { get; set; }
        public string? MergedToPatientName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PatientDetailResponse : PatientResponse
    {
        public string? Address { get; set; }
        public decimal? BirthWeightGram { get; set; }
        public decimal? BirthLengthCm { get; set; }
        public TimeSpan? BirthTime { get; set; }
        public string? DeliveryMethod { get; set; }
        public string? MergeReason { get; set; }
        public string? Notes { get; set; }
    }

    public class PatientOptionResponse
    {
        public Guid Id { get; set; }
        public string PatientCode { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public Gender? Gender { get; set; }
        public string? IdentityType { get; set; }
        public string? IdentityNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public PatientType PatientType { get; set; }
        public PatientStatus PatientStatus { get; set; }
        public bool IsNewborn { get; set; }
        public bool IsMember { get; set; }
    }

    public class PatientEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PatientFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public PatientDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PatientSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<PatientEnumOptionResponse> PatientTypeOptions { get; set; } = new();
        public List<PatientEnumOptionResponse> PatientStatusOptions { get; set; } = new();
        public List<PatientEnumOptionResponse> RegistrationSourceOptions { get; set; } = new();
        public List<PatientEnumOptionResponse> GenderOptions { get; set; } = new();
        public List<PatientEnumOptionResponse> ReligionOptions { get; set; } = new();
        public List<PatientEnumOptionResponse> MaritalStatusOptions { get; set; } = new();
        public List<PatientEnumOptionResponse> BloodTypeOptions { get; set; } = new();
    }

    public class PatientDefaultFilterResponse
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public PatientType? PatientType { get; set; }
        public PatientStatus? PatientStatus { get; set; }
        public PatientRegistrationSource? RegistrationSource { get; set; }
        public Gender? Gender { get; set; }
        public bool? IsMember { get; set; }
        public bool? IsNewborn { get; set; }
        public bool? IsDeceased { get; set; }
        public bool? IsMerged { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? CityId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? DefaultMembershipTierId { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class PatientSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreatePatientRequest
    {
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

        [MaxLength(250)]
        public string? Notes { get; set; }
    }

    public class UpdatePatientRequest : CreatePatientRequest
    {
        public Guid? MergedToPatientId { get; set; }

        [MaxLength(250)]
        public string? MergeReason { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class PatientCreateResponse
    {
        public Guid Id { get; set; }
        public string PatientCode { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public PatientType PatientType { get; set; }
        public PatientStatus PatientStatus { get; set; }
        public bool IsNewborn { get; set; }
        public bool IsMember { get; set; }
        public bool IsActive { get; set; }
    }
}