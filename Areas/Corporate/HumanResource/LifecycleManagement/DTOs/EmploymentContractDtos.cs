using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs
{
    public sealed class EmploymentContractResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string ContractNumber { get; set; } = string.Empty;
        public string HistoryType { get; set; } = string.Empty;
        public string ContractStatus { get; set; } = string.Empty;
        public Guid? ContractTypeId { get; set; }
        public string? ContractTypeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? SignedDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public bool IsCurrent { get; set; }
        public string? SourceDocumentType { get; set; }
        public Guid? SourceDocumentId { get; set; }
        public string? DocumentPath { get; set; }
        public string? DocumentFileName { get; set; }
        public DateTime? FixedTermSeriesStartDate { get; set; }
        public Guid RowVersion { get; set; }
    }

    public sealed class EmploymentContractPrintDataResponse
    {
        public Guid ContractId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string HospitalName { get; set; } = string.Empty;
        public string EmployeeFullName { get; set; } = string.Empty;
        public string EmployeeNationalId { get; set; } = string.Empty;
        public string PositionTitle { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string ContractTypeName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? BasicSalary { get; set; }
        public bool IsSalaryMasked { get; set; }
        public string NoticeText => "Perjanjian Kerja ini dibuat dan ditandatangani oleh Para Pihak dalam keadaan sadar dan tanpa paksaan.";
    }

    public sealed class RecordContractSignedRequest
    {
        [Required(ErrorMessage = "Tanggal penandatanganan wajib diisi.")]
        public DateTime SignedDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Berkas scan kontrak bertanda tangan wajib dilampirkan (VR-ONB-30).")]
        public IFormFile SignedScanFile { get; set; } = null!;
    }

    public sealed class EmploymentContractQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? ContractStatus { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public string? Search { get; set; }
        public int? ExpiringInDays { get; set; }
    }

    // --- BE-HRD-080: Keputusan Kontrak T16 & Batas 5 Tahun PKWT ---

    public sealed class FixedTermCheckResponse
    {
        public Guid ContractHistoryId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public DateOnly FixedTermSeriesStartDate { get; set; }
        public int TotalUsedMonths { get; set; }
        public int RemainingMonths { get; set; }
        public bool CanExtend { get; set; }
        public int MaxExtensionMonths { get; set; }
        public string Message { get; set; } = string.Empty;

        // UU 27/2026 versioned policy fields (FIN-HRD-08-BE-001)
        public string RuleVersion { get; set; } = "UU-27-2026";
        public string LegalBasis { get; set; } = "Undang-Undang Nomor 27 Tahun 2026 tentang Pelindungan Ketenagakerjaan (Pasal 82-87)";
        public int RenewalSequence { get; set; }
        public int MaxRenewalCount { get; set; } = 2;
        public int MaxMonthsPerRenewal { get; set; } = 12;
        public DateOnly? RequiredNoticeDate { get; set; }
        public string? LegalTransitionStatus { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();
    }

    public sealed class SubmitContractDecisionRequest
    {
        [Required]
        public ContractDecisionType DecisionType { get; set; }

        public int? ExtensionMonths { get; set; }

        [Required]
        public DateOnly ProposedStartDate { get; set; }

        public DateOnly? ProposedEndDate { get; set; }

        public string? ReasonText { get; set; }
    }

    public sealed class ContractDecisionQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public ContractDecisionType? DecisionType { get; set; }
        public ContractDecisionStatus? DecisionStatus { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public string? Search { get; set; }
    }

    public sealed class ContractDecisionResponse
    {
        public Guid Id { get; set; }
        public string DecisionNumber { get; set; } = string.Empty;
        public Guid ContractHistoryId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public ContractDecisionType DecisionType { get; set; }
        public string DecisionTypeName => DecisionType.ToString();
        public int? ExtensionMonths { get; set; }
        public DateOnly ProposedStartDate { get; set; }
        public DateOnly? ProposedEndDate { get; set; }
        public string? ReasonText { get; set; }
        public ContractDecisionStatus DecisionStatus { get; set; }
        public string DecisionStatusName => DecisionStatus.ToString();
        public Guid SubmittedByUserId { get; set; }
        public string SubmittedByUserName { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public Guid? DecidedByUserId { get; set; }
        public string? DecidedByUserName { get; set; }
        public DateTime? DecidedAt { get; set; }
        public string? DecisionNote { get; set; }
        public Guid? FollowUpContractHistoryId { get; set; }
        public DateOnly? EmployeeNotifiedOn { get; set; }
        public Guid? EmployeeSeparationId { get; set; }

        // Evidensi & Kepatuhan Pemberitahuan Tertulis (FIN-HRD-08-BE-004)
        public string? NotificationChannel { get; set; }
        public string? NotificationEvidenceNotes { get; set; }
        public string NotificationComplianceStatus { get; set; } = "NOT_APPLICABLE"; // ON_TIME, LATE, MISSING, NOT_APPLICABLE
        public DateOnly? RequiredNoticeDate { get; set; }
    }

    public sealed class RecordEmployeeNotifiedRequest
    {
        [Required]
        public DateOnly NotifiedOn { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        [MaxLength(100)]
        public string? DeliveryChannel { get; set; } = "Surat Tertulis";

        [MaxLength(1000)]
        public string? EvidenceNotes { get; set; }
    }

    // --- Ringkasan Dashboard Menu 08 Submenu 1, 2, 4 ---

    public sealed class EmploymentContractSummaryResponse
    {
        public int ActiveContractsCount { get; set; }
        public int PendingSignedCount { get; set; }
        public int ExpiringSoonCount { get; set; }
        public int NeedsReviewCount { get; set; }
    }

    public sealed class ContractRenewalSummaryResponse
    {
        public int ReminderH60Count { get; set; }
        public int EscalationH14Count { get; set; }
        public int NoticeDueH7Count { get; set; }
        public int PendingDecisionCount { get; set; }
    }

    public sealed class EmploymentStatusChangeSummaryResponse
    {
        public int SubmittedCount { get; set; }
        public int PendingSignedCount { get; set; }
        public int ScheduledOrEffectiveCount { get; set; }
    }

    // --- BE-HRD-081: Koreksi Kontrak T17 ---

    public sealed class SubmitContractCorrectionRequest
    {
        [Required]
        [MaxLength(100)]
        public string AfterContractNumber { get; set; } = string.Empty;

        [Required]
        public Guid AfterContractTypeId { get; set; }

        [Required]
        public DateOnly AfterStartDate { get; set; }

        public DateOnly? AfterEndDate { get; set; }

        [Required]
        [MaxLength(2000)]
        public string ReasonText { get; set; } = string.Empty;

        [Required(ErrorMessage = "Berkas bukti dokumen kontrak perbaikan wajib dilampirkan (VR-ONB-43).")]
        public IFormFile EvidenceFile { get; set; } = null!;
    }

    public sealed class ContractCorrectionResponse
    {
        public Guid Id { get; set; }
        public string CorrectionNumber { get; set; } = string.Empty;
        public Guid ContractHistoryId { get; set; }
        public string BeforeContractNumber { get; set; } = string.Empty;
        public Guid BeforeContractTypeId { get; set; }
        public string BeforeContractTypeName { get; set; } = string.Empty;
        public DateOnly BeforeStartDate { get; set; }
        public DateOnly? BeforeEndDate { get; set; }
        public string AfterContractNumber { get; set; } = string.Empty;
        public Guid AfterContractTypeId { get; set; }
        public string AfterContractTypeName { get; set; } = string.Empty;
        public DateOnly AfterStartDate { get; set; }
        public DateOnly? AfterEndDate { get; set; }
        public string ReasonText { get; set; } = string.Empty;
        public string EvidenceFileName { get; set; } = string.Empty;
        public ContractCorrectionStatus CorrectionStatus { get; set; }
        public string CorrectionStatusName => CorrectionStatus.ToString();
        public Guid SubmittedByUserId { get; set; }
        public string SubmittedByUserName { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public Guid? DecidedByUserId { get; set; }
        public string? DecidedByUserName { get; set; }
        public DateTime? DecidedAt { get; set; }
        public string? DecisionNote { get; set; }
        public DateTime? AppliedAt { get; set; }
    }

    // --- Scan Dokumen Susulan untuk Kontrak Data Awal (VR-ONB-45) ---

    public sealed class UploadLegacyContractDocumentRequest
    {
        [Required(ErrorMessage = "Berkas scan dokumen kontrak lama wajib dilampirkan.")]
        public IFormFile DocumentFile { get; set; } = null!;
    }
}
