using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs
{
    #region METRIC CATALOG DTOS
    public class OppeMetricCatalogDto
    {
        public Guid Id { get; set; }
        public string MetricCode { get; set; } = string.Empty;
        public string MetricName { get; set; } = string.Empty;
        public string CompetencyDomain { get; set; } = string.Empty;
        public string SpecialtyGroup { get; set; } = string.Empty;
        public string DataSourceType { get; set; } = string.Empty;
        public decimal TargetStandardValue { get; set; }
        public decimal MetricWeightPercentage { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateOppeMetricCatalogRequestDto
    {
        [Required]
        [StringLength(30)]
        public string MetricCode { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string MetricName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string CompetencyDomain { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string SpecialtyGroup { get; set; } = "ALL";

        [Required]
        [StringLength(30)]
        public string DataSourceType { get; set; } = "SIMRS_VOLUME";

        [Range(0.0, 100000.0)]
        public decimal TargetStandardValue { get; set; } = 85.00m;

        [Range(0.1, 100.0)]
        public decimal MetricWeightPercentage { get; set; } = 10.00m;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateOppeMetricCatalogRequestDto
    {
        [Required]
        [StringLength(150)]
        public string MetricName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string CompetencyDomain { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string SpecialtyGroup { get; set; } = "ALL";

        [Required]
        [StringLength(30)]
        public string DataSourceType { get; set; } = "SIMRS_VOLUME";

        [Range(0.0, 100000.0)]
        public decimal TargetStandardValue { get; set; }

        [Range(0.1, 100.0)]
        public decimal MetricWeightPercentage { get; set; }

        public bool IsActive { get; set; }
    }
    #endregion

    #region EVALUATION PERIOD DTOS
    public class OppeEvaluationPeriodDto
    {
        public Guid Id { get; set; }
        public string PeriodCode { get; set; } = string.Empty;
        public string PeriodName { get; set; } = string.Empty;
        public int EvaluationYear { get; set; }
        public int Semester { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal QuantitativeWeightPercentage { get; set; }
        public decimal QualitativeWeightPercentage { get; set; }
        public int TotalStaffCount { get; set; }
    }

    public class CreateOppeEvaluationPeriodRequestDto
    {
        [Required]
        [StringLength(50)]
        public string PeriodCode { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string PeriodName { get; set; } = string.Empty;

        [Range(2020, 2050)]
        public int EvaluationYear { get; set; }

        [Range(0, 2)]
        public int Semester { get; set; } = 1;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Range(10.0, 90.0)]
        public decimal QuantitativeWeightPercentage { get; set; } = 60.00m;

        [Range(10.0, 90.0)]
        public decimal QualitativeWeightPercentage { get; set; } = 40.00m;
    }
    #endregion

    #region STAFF EVALUATION DTOS
    public class OppeStaffEvaluationDto
    {
        public Guid Id { get; set; }
        public Guid PeriodId { get; set; }
        public string PeriodName { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public Guid? CommitteeId { get; set; }
        public string CommitteeName { get; set; } = string.Empty;
        public decimal QuantitativeScore { get; set; }
        public decimal QualitativeScore { get; set; }
        public decimal FinalCompositeScore { get; set; }
        public string PerformanceGrade { get; set; } = string.Empty;
        public Guid? EvaluatorKsmHeadId { get; set; }
        public string? EvaluatorKsmHeadName { get; set; }
        public string? EvaluatorRecommendationNotes { get; set; }
        public bool IsAcknowledgedByStaff { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<OppeStaffEvaluationDetailDto> Details { get; set; } = new List<OppeStaffEvaluationDetailDto>();
    }

    public class OppeStaffEvaluationDetailDto
    {
        public Guid Id { get; set; }
        public Guid MetricCatalogId { get; set; }
        public string MetricCode { get; set; } = string.Empty;
        public string MetricName { get; set; } = string.Empty;
        public string CompetencyDomain { get; set; } = string.Empty;
        public string DataSourceType { get; set; } = string.Empty;
        public decimal TargetValue { get; set; }
        public decimal ActualValue { get; set; }
        public decimal ComputedScore { get; set; }
        public string? EvaluatorNotes { get; set; }
    }

    public class FinalizeOppeStaffEvaluationDto
    {
        public Guid EvaluatorKsmHeadId { get; set; }
        [Required]
        public string EvaluatorRecommendationNotes { get; set; } = string.Empty;
    }

    public class AcknowledgeOppeStaffEvaluationDto
    {
        public Guid WorkforceProfileId { get; set; }
        public string? ClarificationNotes { get; set; }
    }
    #endregion

    #region PEER REVIEW DTOS
    public class AssignedPeerReviewDto
    {
        public Guid StaffEvaluationId { get; set; }
        public Guid EvaluateeWorkforceProfileId { get; set; }
        public string EvaluateeName { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public string PeriodName { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class SubmitPeerReviewEvaluationRequestDto
    {
        [Required]
        public Guid EvaluatorWorkforceProfileId { get; set; }

        [Required]
        public List<PeerReviewScoreItemDto> Scores { get; set; } = new List<PeerReviewScoreItemDto>();

        public string? OverallClinicalImpression { get; set; }
    }

    public class PeerReviewScoreItemDto
    {
        [Required]
        public Guid MetricCatalogId { get; set; }

        /// <summary>
        /// Nilai Likert 1 s.d. 5
        /// </summary>
        [Range(1, 5)]
        public int Score { get; set; }

        /// <summary>
        /// Wajib diisi jika skor <= 2 (VR-OPP-04)
        /// </summary>
        public string? QualitativeComment { get; set; }
    }
    #endregion

    #region READ AUDIT DTOS
    public class LogPeerReviewAccessRequestDto
    {
        [Required]
        public Guid AccessedWorkforceProfileId { get; set; }

        [Required]
        public string RecordType { get; set; } = "OPPE_REPORT";

        [Required]
        public Guid RecordId { get; set; }

        [Required]
        public Guid AccessedByUserId { get; set; }

        [Required]
        [StringLength(250)]
        public string AccessReason { get; set; } = string.Empty;

        public string IpAddress { get; set; } = "127.0.0.1";
    }

    public class ClinicalPeerReviewAuditLogDto
    {
        public Guid Id { get; set; }
        public Guid AccessedWorkforceProfileId { get; set; }
        public string AccessedDoctorName { get; set; } = string.Empty;
        public string RecordType { get; set; } = string.Empty;
        public Guid RecordId { get; set; }
        public Guid AccessedByUserId { get; set; }
        public string AccessedByUserName { get; set; } = string.Empty;
        public string AccessReason { get; set; } = string.Empty;
        public DateTime AccessedAt { get; set; }
        public string IpAddress { get; set; } = string.Empty;
    }
    #endregion
}
