using System;
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

        public ClinicalScreeningResult? ManualClinicalResult { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public sealed class CandidateScreeningResponse
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
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
        public string? StrNumber { get; set; }
        public DateOnly? StrExpiryDate { get; set; }
        public string? Notes { get; set; }
        public Guid EvaluatedByUserId { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public sealed class CandidateScreeningListItemResponse
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public string CandidateName { get; set; } = string.Empty;
        public string VacancyTitle { get; set; } = string.Empty;
        public bool IsClinicalVacancy { get; set; }
        public ScreeningStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public ClinicalScreeningResult ClinicalScreeningResult { get; set; }
        public string ClinicalScreeningResultName => ClinicalScreeningResult.ToString();
        public DateOnly? StrExpiryDate { get; set; }
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
}
