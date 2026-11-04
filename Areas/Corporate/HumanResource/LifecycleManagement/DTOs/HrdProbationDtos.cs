using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs
{
    public sealed class HrdProbationResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public Guid ContractHistoryId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public DateOnly ReminderFromDate { get; set; }
        public ProbationStatus ProbationStatus { get; set; }
        public string ProbationStatusName => ProbationStatus.ToString();
        public DateTime? ClosedAt { get; set; }
        public Guid? ClosedByUserId { get; set; }
        public Guid? EmployeeSeparationId { get; set; }
        public Guid RowVersion { get; set; }
        public List<ProbationAssessmentResponse> Assessments { get; set; } = new();
    }

    public sealed class HrdProbationQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public ProbationStatus? Status { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public bool? DueOnly { get; set; }
        public DateOnly? EndDateFrom { get; set; }
        public DateOnly? EndDateTo { get; set; }
        public string? Search { get; set; }
    }

    public sealed class ProbationAssessmentResponse
    {
        public Guid Id { get; set; }
        public string AssessmentNumber { get; set; } = string.Empty;
        public Guid ProbationId { get; set; }
        public ProbationResult Result { get; set; }
        public string ResultName => Result.ToString();
        public string ReasonText { get; set; } = string.Empty;
        public string? EvidenceFileName { get; set; }
        public ProbationAssessmentStatus AssessmentStatus { get; set; }
        public string AssessmentStatusName => AssessmentStatus.ToString();
        public Guid SubmittedByUserId { get; set; }
        public string? SubmittedByUserName { get; set; }
        public DateTime SubmittedAt { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public Guid? DecidedByUserId { get; set; }
        public string? DecidedByUserName { get; set; }
        public DateTime? DecidedAt { get; set; }
        public string? DecisionNote { get; set; }
        public Guid RowVersion { get; set; }
    }

    public sealed class SubmitProbationAssessmentRequest
    {
        [Required(ErrorMessage = "Hasil penilaian (Lulus/Tidak Lulus) wajib dipilih.")]
        public ProbationResult Result { get; set; }

        [Required(ErrorMessage = "Alasan atau pertimbangan penilaian wajib diisi.")]
        [MaxLength(2000, ErrorMessage = "Alasan penilaian maksimal 2000 karakter.")]
        public string ReasonText { get; set; } = string.Empty;

        public IFormFile? EvidenceFile { get; set; }

        [Required(ErrorMessage = "RowVersion masa percobaan wajib disertakan untuk pencegahan konflik konkurensi.")]
        public Guid ProbationRowVersion { get; set; }
    }

    public sealed class UpdateProbationAssessmentRequest
    {
        [Required(ErrorMessage = "Hasil penilaian (Lulus/Tidak Lulus) wajib dipilih.")]
        public ProbationResult Result { get; set; }

        [Required(ErrorMessage = "Alasan atau pertimbangan penilaian wajib diisi.")]
        [MaxLength(2000, ErrorMessage = "Alasan penilaian maksimal 2000 karakter.")]
        public string ReasonText { get; set; } = string.Empty;

        public IFormFile? EvidenceFile { get; set; }

        [Required(ErrorMessage = "RowVersion penilaian wajib disertakan.")]
        public Guid RowVersion { get; set; }
    }

    public sealed class WithdrawAssessmentRequest
    {
        [MaxLength(500, ErrorMessage = "Catatan penarikan maksimal 500 karakter.")]
        public string? Reason { get; set; }
    }
}
