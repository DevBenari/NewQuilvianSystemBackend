using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Enums;
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

        // Hitungan per status siklus hidup crm-membership (BE-CRM-023).
        public int PendingPaymentPatientMembership { get; set; }
        public int ScheduledPatientMembership { get; set; }
        public int NeedsVerificationPatientMembership { get; set; }
        public int CancelledPatientMembership { get; set; }

        /// <summary>Aktif yang berakhir dalam 30 hari ke depan (pengganti pengingat WA selama MVP).</summary>
        public int ExpiringIn30DaysPatientMembership { get; set; }
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
        public string TierTypeName { get; set; } = string.Empty;

        public string MemberNumber { get; set; } = string.Empty;
        public MembershipStatus MembershipStatus { get; set; }
        public string MembershipStatusName { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }
        public DateTime? ExpiredDate { get; set; }

        public MembershipRegistrationType RegistrationType { get; set; }
        public string RegistrationTypeName { get; set; } = string.Empty;
        public DateTime? PeriodStartDate { get; set; }
        public DateTimeOffset? PaymentDueAt { get; set; }
        public DateTimeOffset? ActivatedAt { get; set; }
        public decimal FeeAmountSnapshot { get; set; }
        public Guid? PreviousPatientMembershipId { get; set; }
        public MembershipCancellationCategory? CancellationCategory { get; set; }
        public string? CancellationCategoryName { get; set; }
        public Guid RowVersion { get; set; }

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
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class PatientMembershipDetailResponse : PatientMembershipResponse
    {
        public string? UpgradeDowngradeReason { get; set; }
        public string? Notes { get; set; }

        /// <summary>Hanya di detail, tidak di daftar (data sensitif).</summary>
        public string? CancellationReason { get; set; }

        /// <summary>Referensi tagihan iuran di Billing/Kasir; kosong untuk data lama atau impor.</summary>
        public Guid? FeeChargeReferenceId { get; set; }

        /// <summary>
        /// Ringkasan tagihan iuran dari Billing (baca-saja). Status membership tetap ditentukan CRM; bila tagihan
        /// sudah lunas tetapi membership masih Menunggu Pembayaran, <c>NeedsSync</c> bernilai true.
        /// </summary>
        public QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Services.MembershipFeeChargeSummary? FeeCharge { get; set; }
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
        public string MembershipStatusName { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
    }

    public class PatientMembershipOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<PatientMembershipOptionResponse> Items { get; set; } = new();
    }

    public class PatientMembershipFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public PatientMembershipDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PatientMembershipCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<PatientMembershipSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<PatientMembershipRelationFilterResponse> RelationFilters { get; set; } = new();
        public List<PatientMembershipEnumOptionResponse> MembershipStatusOptions { get; set; } = new();
        public List<PatientMembershipEnumOptionResponse> MembershipTierTypeOptions { get; set; } = new();
        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class PatientMembershipDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? PatientId { get; set; }
        public Guid? MembershipTierId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class PatientMembershipRelationFilterResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
    }

    public class PatientMembershipEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PatientMembershipCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PatientMembershipSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>
    /// Pendaftaran membership baru oleh CRM (BE-CRM-011). Status, tanggal, dan nomor member
    /// diisi sistem; field lain di body diabaikan (CRM-DEC-022, CRM-DEC-023).
    /// </summary>
    public class RegisterPatientMembershipRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid MembershipTierId { get; set; }

        [MaxLength(250)]
        public string? Notes { get; set; }
    }

    /// <summary>Perpanjangan membership Aktif dengan tingkat yang sama (BE-CRM-012, CRM-DEC-024).</summary>
    public class RenewPatientMembershipRequest
    {
        [MaxLength(250)]
        public string? Notes { get; set; }

        /// <summary>RowVersion membership Aktif yang sedang ditampilkan (VAL-17).</summary>
        [Required]
        public Guid ExpectedRowVersion { get; set; }
    }

    /// <summary>Upgrade membership Aktif ke tingkat yang lebih tinggi (BE-CRM-012, CRM-DEC-012).</summary>
    public class UpgradePatientMembershipRequest
    {
        [Required]
        public Guid MembershipTierId { get; set; }

        [MaxLength(250)]
        public string? Notes { get; set; }

        [Required]
        public Guid ExpectedRowVersion { get; set; }
    }

    /// <summary>Pembatalan oleh petugas CRM (BE-CRM-013, CRM-DEC-013).</summary>
    public class CancelPatientMembershipRequest
    {
        [Required]
        public MembershipCancellationCategory Category { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public Guid ExpectedRowVersion { get; set; }
    }

    /// <summary>Keputusan atas membership lama berstatus Perlu Verifikasi (BE-CRM-013, CRM-DEC-021).</summary>
    public class VerifyLegacyPatientMembershipRequest
    {
        [Required]
        public MembershipVerificationDecision Decision { get; set; }

        /// <summary>Wajib bila diakui. Tanggal kalender WIB.</summary>
        public DateTime? PeriodStartDate { get; set; }

        /// <summary>Wajib bila diakui. Tanggal akhir periode, inklusif.</summary>
        public DateTime? PeriodEndDate { get; set; }

        /// <summary>Wajib (10–500 karakter) bila ditolak.</summary>
        [MaxLength(500)]
        public string? Reason { get; set; }

        [Required]
        public Guid ExpectedRowVersion { get; set; }
    }

    /// <summary>Ubah catatan — satu-satunya ubah setelah tersimpan (BE-CRM-013, CRM-DEC-022).</summary>
    public class UpdatePatientMembershipNotesRequest
    {
        [MaxLength(250)]
        public string? Notes { get; set; }

        [Required]
        public Guid ExpectedRowVersion { get; set; }
    }

    public class PatientMembershipStatusHistoryResponse
    {
        public Guid Id { get; set; }
        public MembershipStatus? FromStatus { get; set; }
        public MembershipStatus ToStatus { get; set; }
        public string ActionCode { get; set; } = string.Empty;
        public MembershipChangeSource Source { get; set; }
        public Guid? ActorUserId { get; set; }
        public string? ActorName { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
        public string? Reason { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? OldNotes { get; set; }
        public string? NewNotes { get; set; }
    }

    /// <summary>Pencarian pasien untuk pendaftaran membership (BE-CRM-014, CRM-DEC-010).</summary>
    public class PatientMembershipLookupQuery
    {
        /// <summary>No. RM, NIK, atau No. Telp; minimal 3 karakter.</summary>
        public string? Keyword { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public class PatientMembershipActiveSummaryResponse
    {
        public Guid PatientMembershipId { get; set; }
        public string MemberNumber { get; set; } = string.Empty;
        public Guid MembershipTierId { get; set; }
        public string TierCode { get; set; } = string.Empty;
        public string TierName { get; set; } = string.Empty;
        public MembershipTierType TierType { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
    }

    public class PatientMembershipLookupResponse
    {
        public Guid PatientId { get; set; }
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }

        /// <summary>NIK tersamar, contoh <c>3174********0001</c>.</summary>
        public string? IdentityNumberMasked { get; set; }

        /// <summary>No. Telp tersamar, contoh <c>0812*****123</c>.</summary>
        public string? PhoneNumberMasked { get; set; }

        public bool IsMember { get; set; }
        public PatientMembershipActiveSummaryResponse? ActiveMembership { get; set; }
        public string? PendingMemberNumber { get; set; }
        public DateTimeOffset? PendingPaymentDueAt { get; set; }
    }

    public class PatientActiveMembershipResponse
    {
        public Guid PatientId { get; set; }

        /// <summary>Tanggal kalender WIB yang ditanyakan.</summary>
        public DateTime Date { get; set; }

        public bool IsMember { get; set; }
        public PatientMembershipActiveSummaryResponse? ActiveMembership { get; set; }
    }
}
