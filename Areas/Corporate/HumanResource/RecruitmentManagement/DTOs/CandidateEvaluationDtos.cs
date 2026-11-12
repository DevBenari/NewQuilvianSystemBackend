using System;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs
{
    public sealed class ScheduleEvaluationRequest
    {
        [Required(ErrorMessage = "ApplicationId wajib diisi.")]
        public Guid ApplicationId { get; set; }

        public EvaluationType EvaluationType { get; set; }

        [Required(ErrorMessage = "EvaluatorUserId wajib diisi.")]
        public Guid EvaluatorUserId { get; set; }

        [Required(ErrorMessage = "Nama penilai wajib diisi.")]
        [MaxLength(150)]
        public string EvaluatorName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string EvaluatorRole { get; set; } = string.Empty;

        public DateTime EvaluationDate { get; set; } = DateTime.UtcNow;

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public sealed class SubmitHrEvaluationRequest
    {
        [Range(0, 100, ErrorMessage = "Skor harus berada antara 0 sampai 100.")]
        public decimal Score { get; set; }

        public EvaluationRecommendation Recommendation { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public sealed class SubmitClinicalEvaluationRequest
    {
        [Range(0, 100, ErrorMessage = "Skor harus berada antara 0 sampai 100.")]
        public decimal Score { get; set; }

        public EvaluationRecommendation Recommendation { get; set; }

        [MaxLength(500)]
        public string? VetoReason { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public sealed class CandidateEvaluationResponse
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public Guid CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public EvaluationType EvaluationType { get; set; }
        public string EvaluationTypeName => EvaluationType.ToString();
        public Guid EvaluatorUserId { get; set; }
        public string EvaluatorName { get; set; } = string.Empty;
        public string EvaluatorRole { get; set; } = string.Empty;
        public DateTime EvaluationDate { get; set; }
        public decimal Score { get; set; }
        public EvaluationRecommendation Recommendation { get; set; }
        public string RecommendationName => Recommendation.ToString();
        public bool IsClinicalVetoApplied { get; set; }
        public string? VetoReason { get; set; }
        public string? Notes { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public sealed class CandidateEvaluationListItemResponse
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public string CandidateName { get; set; } = string.Empty;
        public EvaluationType EvaluationType { get; set; }
        public string EvaluationTypeName => EvaluationType.ToString();
        public string EvaluatorName { get; set; } = string.Empty;
        public DateTime EvaluationDate { get; set; }
        public decimal Score { get; set; }
        public EvaluationRecommendation Recommendation { get; set; }
        public string RecommendationName => Recommendation.ToString();
        public bool IsClinicalVetoApplied { get; set; }
    }

    public sealed class CandidateEvaluationQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public Guid? ApplicationId { get; set; }
        public EvaluationType? EvaluationType { get; set; }
        public EvaluationRecommendation? Recommendation { get; set; }
        public bool? IsClinicalVetoApplied { get; set; }
        public string? Search { get; set; }
    }
}
