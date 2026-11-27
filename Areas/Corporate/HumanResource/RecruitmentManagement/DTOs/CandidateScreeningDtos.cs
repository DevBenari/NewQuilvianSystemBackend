using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs
{
    public sealed class InitiateScreeningRequest
    {
        [Required(ErrorMessage = "ApplicationId wajib diisi.")]
        public Guid ApplicationId { get; set; }

        public bool IsStrRequired { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public sealed class EvaluateScreeningRequest
    {
        [Required(ErrorMessage = "ApplicationId wajib diisi.")]
        public Guid ApplicationId { get; set; }

        /// <summary>
        /// Keputusan opsional (<c>Pending</c> = simpan catatan/minta kelengkapan sementara tanpa ubah schema enum,
        /// <c>Passed</c> = nyatakan lulus bila semua dokumen wajib dan STR terpenuhi, <c>Rejected</c> = gugurkan administrasi).
        /// </summary>
        public ScreeningStatus? Decision { get; set; }

        public ClinicalScreeningResult? ManualClinicalResult { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public sealed class CandidateScreeningResponse
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public string? ApplicationStageStatus { get; set; }
        public Guid CandidateId { get; set; }
        public string CandidateCode { get; set; } = string.Empty;
        public string CandidateName { get; set; } = string.Empty;
        public Guid VacancyId { get; set; }
        public string VacancyTitle { get; set; } = string.Empty;
        public bool IsClinicalVacancy { get; set; }
        public DateTime ScreeningDate { get; set; }
        public ScreeningStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public bool IsStrRequired { get; set; }
        public ClinicalScreeningResult ClinicalScreeningResult { get; set; }
        public string ClinicalScreeningResultName => ClinicalScreeningResult.ToString();
        public string ClinicalScreeningResultLabel { get; set; } = string.Empty;
        public string? StrNumber { get; set; }
        public DateOnly? StrExpiryDate { get; set; }
        public int DocumentsCount { get; set; }
        public bool HasStrDocument { get; set; }
        public bool HasExpiredStr { get; set; }
        public List<CandidateDocumentResponse> Documents { get; set; } = new();
        public string? Notes { get; set; }
        public Guid EvaluatedByUserId { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public sealed class CandidateScreeningListItemResponse
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public string? ApplicationStageStatus { get; set; }
        public Guid CandidateId { get; set; }
        public string CandidateCode { get; set; } = string.Empty;
        public string CandidateName { get; set; } = string.Empty;
        public string VacancyTitle { get; set; } = string.Empty;
        public bool IsClinicalVacancy { get; set; }
        public ScreeningStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public ClinicalScreeningResult ClinicalScreeningResult { get; set; }
        public string ClinicalScreeningResultName => ClinicalScreeningResult.ToString();
        public string ClinicalScreeningResultLabel { get; set; } = string.Empty;
        public DateOnly? StrExpiryDate { get; set; }
        public int DocumentsCount { get; set; }
        public bool HasStrDocument { get; set; }
        public bool HasExpiredStr { get; set; }
        public DateTime ScreeningDate { get; set; }
    }

    public sealed class CandidateScreeningQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public Guid? VacancyId { get; set; }
        public ScreeningStatus? Status { get; set; }
        public ClinicalScreeningResult? ClinicalResult { get; set; }
        public string? Search { get; set; }
    }

    public sealed class ScreeningSummaryResponse
    {
        public int TotalScreenings { get; set; }
        public int PendingCount { get; set; }
        public int PassedCount { get; set; }
        public int RejectedCount { get; set; }
        public int ClinicalPendingCount { get; set; }
        public int MissingScreeningEligibleCount { get; set; }
    }

    public sealed class ScreeningReconcileItemDto
    {
        public Guid ApplicationId { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public Guid CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string VacancyTitle { get; set; } = string.Empty;
        public bool IsClinical { get; set; }
        public string CurrentStage { get; set; } = string.Empty;
        public string StageStatus { get; set; } = string.Empty;
        public bool Eligible { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public sealed class ScreeningReconcilePreviewResponse
    {
        public int EligibleCount { get; set; }
        public int SkippedCount { get; set; }
        public List<ScreeningReconcileItemDto> EligibleItems { get; set; } = new();
        public List<ScreeningReconcileItemDto> SkippedItems { get; set; } = new();
    }

    public sealed class ScreeningReconcileResultResponse
    {
        public int CreatedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<Guid> CreatedScreeningIds { get; set; } = new();
        public List<ScreeningReconcileItemDto> SkippedItems { get; set; } = new();
    }
}
