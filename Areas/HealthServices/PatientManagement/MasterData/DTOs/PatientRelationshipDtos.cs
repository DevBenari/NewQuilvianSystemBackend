using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs
{
    public class PatientRelationshipSummaryResponse
    {
        public int TotalRelationship { get; set; }
        public int ActiveRelationship { get; set; }
        public int InactiveRelationship { get; set; }
        public int PrimaryRelationship { get; set; }
        public int EmergencyContactRelationship { get; set; }
        public int ResponsiblePersonRelationship { get; set; }
        public int LegalGuardianRelationship { get; set; }
    }

    public class PatientRelationshipResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientCode { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public Guid? RelatedPatientId { get; set; }
        public string? RelatedPatientCode { get; set; }
        public string? RelatedPatientMedicalRecordNumber { get; set; }
        public string? RelatedPatientName { get; set; }
        public PatientRelationshipType RelationshipType { get; set; }
        public string RelationshipTypeName { get; set; } = string.Empty;
        public string? RelatedPersonName { get; set; }
        public string? RelatedPersonIdentityType { get; set; }
        public string? RelatedPersonIdentityNumber { get; set; }
        public string? RelatedPersonPhoneNumber { get; set; }
        public string? RelatedPersonWhatsAppNumber { get; set; }
        public string? RelatedPersonEmail { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsEmergencyContact { get; set; }
        public bool IsResponsiblePerson { get; set; }
        public bool IsLegalGuardian { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PatientRelationshipDetailResponse : PatientRelationshipResponse
    {
        public string? RelatedPersonAddress { get; set; }
        public string? Notes { get; set; }
    }

    public class PatientRelationshipOptionResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public Guid? RelatedPatientId { get; set; }
        public string? RelatedPatientName { get; set; }
        public PatientRelationshipType RelationshipType { get; set; }
        public string RelationshipTypeName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public bool IsEmergencyContact { get; set; }
        public bool IsResponsiblePerson { get; set; }
        public bool IsLegalGuardian { get; set; }
    }

    public class PatientRelationshipEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PatientRelationshipFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public PatientRelationshipDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PatientRelationshipSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<PatientRelationshipEnumOptionResponse> RelationshipTypeOptions { get; set; } = new();
    }

    public class PatientRelationshipDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? PatientId { get; set; }
        public Guid? RelatedPatientId { get; set; }
        public PatientRelationshipType? RelationshipType { get; set; }
        public bool? IsPrimary { get; set; }
        public bool? IsEmergencyContact { get; set; }
        public bool? IsResponsiblePerson { get; set; }
        public bool? IsLegalGuardian { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class PatientRelationshipSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreatePatientRelationshipRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        public Guid? RelatedPatientId { get; set; }

        public PatientRelationshipType RelationshipType { get; set; } = PatientRelationshipType.Unknown;

        [MaxLength(200)]
        public string? RelatedPersonName { get; set; }

        [MaxLength(50)]
        public string? RelatedPersonIdentityType { get; set; }

        [MaxLength(100)]
        public string? RelatedPersonIdentityNumber { get; set; }

        [MaxLength(30)]
        public string? RelatedPersonPhoneNumber { get; set; }

        [MaxLength(30)]
        public string? RelatedPersonWhatsAppNumber { get; set; }

        [MaxLength(200)]
        public string? RelatedPersonEmail { get; set; }

        [MaxLength(500)]
        public string? RelatedPersonAddress { get; set; }

        public bool IsPrimary { get; set; } = false;
        public bool IsEmergencyContact { get; set; } = false;
        public bool IsResponsiblePerson { get; set; } = false;
        public bool IsLegalGuardian { get; set; } = false;

        [MaxLength(250)]
        public string? Notes { get; set; }
    }

    public class UpdatePatientRelationshipRequest : CreatePatientRelationshipRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class PatientRelationshipCreateResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid? RelatedPatientId { get; set; }
        public PatientRelationshipType RelationshipType { get; set; }
        public string? RelatedPersonName { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsEmergencyContact { get; set; }
        public bool IsResponsiblePerson { get; set; }
        public bool IsLegalGuardian { get; set; }
        public bool IsActive { get; set; }
    }
}