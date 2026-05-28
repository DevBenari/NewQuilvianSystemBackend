using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs
{
    public class PatientEmergencyContactSummaryResponse
    {
        public int TotalEmergencyContact { get; set; }
        public int ActiveEmergencyContact { get; set; }
        public int InactiveEmergencyContact { get; set; }
        public int PrimaryEmergencyContact { get; set; }
        public int ResponsiblePersonContact { get; set; }
        public int SameAddressAsPatientContact { get; set; }
    }

    public class PatientEmergencyContactResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientCode { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string PatientFullName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string? Relationship { get; set; }
        public string? IdentityType { get; set; }
        public string? IdentityNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsResponsiblePerson { get; set; }
        public bool IsSameAddressAsPatient { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PatientEmergencyContactDetailResponse : PatientEmergencyContactResponse
    {
        public string? Notes { get; set; }
    }

    public class PatientEmergencyContactOptionResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientFullName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string? Relationship { get; set; }
        public string? PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsResponsiblePerson { get; set; }
    }

    public class PatientEmergencyContactFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public PatientEmergencyContactDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PatientEmergencyContactSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class PatientEmergencyContactDefaultFilterResponse
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public Guid? PatientId { get; set; }
        public bool? IsPrimary { get; set; }
        public bool? IsResponsiblePerson { get; set; }
        public bool? IsSameAddressAsPatient { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class PatientEmergencyContactSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreatePatientEmergencyContactRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        [Required]
        [MaxLength(200)]
        public string ContactName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Relationship { get; set; }

        [MaxLength(50)]
        public string? IdentityType { get; set; }

        [MaxLength(100)]
        public string? IdentityNumber { get; set; }

        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        [MaxLength(30)]
        public string? WhatsAppNumber { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsResponsiblePerson { get; set; } = false;

        public bool IsSameAddressAsPatient { get; set; } = false;

        [MaxLength(250)]
        public string? Notes { get; set; }
    }

    public class UpdatePatientEmergencyContactRequest : CreatePatientEmergencyContactRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class PatientEmergencyContactCreateResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string ContactName { get; set; } = string.Empty;
        public string? Relationship { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsResponsiblePerson { get; set; }
        public bool IsActive { get; set; }
    }
}