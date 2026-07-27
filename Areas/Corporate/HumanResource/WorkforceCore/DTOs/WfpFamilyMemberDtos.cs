using QuilvianSystemBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs
{
    public class WfpFamilyMemberSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int EmergencyContactData { get; set; }
        public int WithDependentData { get; set; }
    }

    public class WfpFamilyMemberResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public Gender? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? IdentityType { get; set; }
        public string? IdentityNumber { get; set; }
        public string? MaritalStatusText { get; set; }
        public string? Occupation { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public bool IsEmergencyContact { get; set; }
        public int DependentCount { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpFamilyMemberOptionResponse
    {
        public Guid Id { get; set; }
        public string Relationship { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public bool IsEmergencyContact { get; set; }
        public bool IsActive { get; set; }
    }

    public class WfpFamilyMemberFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpFamilyMemberDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpFamilyMemberStringOptionResponse> CustomPeriods { get; set; } = new();
        public List<WfpFamilyMemberStringOptionResponse> RelationshipOptions { get; set; } = new();
        public List<WfpFamilyMemberEnumOptionResponse> GenderOptions { get; set; } = new();
        public List<WfpFamilyMemberStringOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpFamilyMemberDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Period { get; set; }
        public string? Relationship { get; set; }
        public Gender? Gender { get; set; }
        public bool? IsEmergencyContact { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "fullName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpFamilyMemberStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpFamilyMemberEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpFamilyMemberRequest
    {
        [Required, MaxLength(100)]
        public string Relationship { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        public Gender? Gender { get; set; }
        public DateTime? BirthDate { get; set; }

        [MaxLength(50)]
        public string? IdentityType { get; set; }

        [MaxLength(100)]
        public string? IdentityNumber { get; set; }

        [MaxLength(100)]
        public string? MaritalStatusText { get; set; }

        [MaxLength(200)]
        public string? Occupation { get; set; }

        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        [MaxLength(200), EmailAddress]
        public string? Email { get; set; }

        public bool IsEmergencyContact { get; set; }
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateWfpFamilyMemberRequest : CreateWfpFamilyMemberRequest { }

    public class UpdateWfpFamilyMemberStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class UpdateWfpFamilyMemberEmergencyContactRequest
    {
        public bool IsEmergencyContact { get; set; }
    }
}
