using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs
{
    public class PatientMembershipSummaryResponse
    {
        public int TotalPatientMembership { get; set; }
        public int ActivePatientMembership { get; set; }
        public int InactivePatientMembership { get; set; }
        public int PrimaryPatientMembership { get; set; }
        public int ExpiredPatientMembership { get; set; }
        public int AutoCreatedPatientMembership { get; set; }
        public int KioskCreatedPatientMembership { get; set; }
        public int AdmissionCreatedPatientMembership { get; set; }
        public int MarketingCreatedPatientMembership { get; set; }
    }

    public class PatientMembershipResponse
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }
        public string PatientCode { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string PatientFullName { get; set; } = string.Empty;

        public Guid MembershipTierId { get; set; }
        public string TierCode { get; set; } = string.Empty;
        public string TierName { get; set; } = string.Empty;
        public MembershipTierType TierType { get; set; }

        public string MemberNumber { get; set; } = string.Empty;
        public MembershipStatus MembershipStatus { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime? ExpiredDate { get; set; }

        public bool IsPrimary { get; set; }
        public bool IsAutoCreated { get; set; }
        public bool IsCreatedFromKiosk { get; set; }
        public bool IsCreatedFromAdmission { get; set; }
        public bool IsCreatedByMarketing { get; set; }

        public int PointBalance { get; set; }
        public decimal TotalSpendAmount { get; set; }

        public DateTime? LastUpgradeDate { get; set; }
        public DateTime? LastDowngradeDate { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PatientMembershipDetailResponse : PatientMembershipResponse
    {
        public string? UpgradeDowngradeReason { get; set; }
        public string? Notes { get; set; }
    }

    public class PatientMembershipOptionResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientFullName { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public Guid MembershipTierId { get; set; }
        public string TierName { get; set; } = string.Empty;
        public string MemberNumber { get; set; } = string.Empty;
        public MembershipStatus MembershipStatus { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
    }

    public class PatientMembershipEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PatientMembershipFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public PatientMembershipDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PatientMembershipSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<PatientMembershipEnumOptionResponse> MembershipStatusOptions { get; set; } = new();
        public List<PatientMembershipEnumOptionResponse> MembershipTierTypeOptions { get; set; } = new();
    }

    public class PatientMembershipDefaultFilterResponse
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public Guid? PatientId { get; set; }
        public Guid? MembershipTierId { get; set; }
        public MembershipStatus? MembershipStatus { get; set; }
        public MembershipTierType? TierType { get; set; }
        public bool? IsPrimary { get; set; }
        public bool? IsAutoCreated { get; set; }
        public bool? IsCreatedFromKiosk { get; set; }
        public bool? IsCreatedFromAdmission { get; set; }
        public bool? IsCreatedByMarketing { get; set; }
        public bool? IsExpired { get; set; }
        public DateTime? JoinDateFrom { get; set; }
        public DateTime? JoinDateTo { get; set; }
        public DateTime? ExpiredDateFrom { get; set; }
        public DateTime? ExpiredDateTo { get; set; }
        public string SortBy { get; set; } = "joinDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class PatientMembershipSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreatePatientMembershipRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid MembershipTierId { get; set; }

        [Required]
        [MaxLength(50)]
        public string MemberNumber { get; set; } = string.Empty;

        public MembershipStatus MembershipStatus { get; set; } = MembershipStatus.Active;

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiredDate { get; set; }

        public bool IsPrimary { get; set; } = true;

        public bool IsAutoCreated { get; set; } = false;

        public bool IsCreatedFromKiosk { get; set; } = false;

        public bool IsCreatedFromAdmission { get; set; } = false;

        public bool IsCreatedByMarketing { get; set; } = false;

        public int PointBalance { get; set; } = 0;

        public decimal TotalSpendAmount { get; set; } = 0;

        public DateTime? LastUpgradeDate { get; set; }

        public DateTime? LastDowngradeDate { get; set; }

        [MaxLength(250)]
        public string? UpgradeDowngradeReason { get; set; }

        [MaxLength(250)]
        public string? Notes { get; set; }
    }

    public class UpdatePatientMembershipRequest : CreatePatientMembershipRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class PatientMembershipCreateResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid MembershipTierId { get; set; }
        public string MemberNumber { get; set; } = string.Empty;
        public MembershipStatus MembershipStatus { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
    }
}
