using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs
{
    public class WfpEmergencyContactSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int PrimaryData { get; set; }
        public int WithWhatsAppData { get; set; }
    }

    public class WfpEmergencyContactResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? WhatsAppNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public int PriorityOrder { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpEmergencyContactOptionResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int PriorityOrder { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
    }

    public class WfpEmergencyContactFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpEmergencyContactDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpEmergencyContactStringOptionResponse> CustomPeriods { get; set; } = new();
        public List<WfpEmergencyContactStringOptionResponse> RelationshipOptions { get; set; } = new();
        public List<WfpEmergencyContactStringOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpEmergencyContactDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Period { get; set; }
        public string? Relationship { get; set; }
        public bool? IsPrimary { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "priorityOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpEmergencyContactStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpEmergencyContactRequest
    {
        [Required, MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Relationship { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? WhatsAppNumber { get; set; }

        [MaxLength(200), EmailAddress]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [Range(1, int.MaxValue)]
        public int PriorityOrder { get; set; } = 1;

        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateWfpEmergencyContactRequest : CreateWfpEmergencyContactRequest { }

    public class UpdateWfpEmergencyContactStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class SetWfpEmergencyContactPrimaryRequest
    {
        public bool IsPrimary { get; set; } = true;
    }
}
