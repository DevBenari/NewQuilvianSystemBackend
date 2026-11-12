using System;
using System.Collections.Generic;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs
{
    // =========================================================================
    // 1. SIKLUS KINERJA & KATALOG KPI METRICS (Kontrak v21 - Tag 27.1)
    // =========================================================================

    public class PerformanceCycleDto
    {
        public Guid Id { get; set; }
        public string CycleCode { get; set; } = string.Empty;
        public string CycleName { get; set; } = string.Empty;
        public string CycleType { get; set; } = string.Empty; // SEMESTER, ANNUAL
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GoalSettingDeadline { get; set; }
        public DateTime SelfAssessmentDeadline { get; set; }
        public DateTime ManagerAssessmentDeadline { get; set; }
        public DateTime CalibrationDeadline { get; set; }
        public string Status { get; set; } = string.Empty; // DRAFT, GOAL_SETTING, ACTIVE_ASSESSMENT, CALIBRATION, CLOSED
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreatePerformanceCycleRequestDto
    {
        public string CycleCode { get; set; } = string.Empty;
        public string CycleName { get; set; } = string.Empty;
        public string CycleType { get; set; } = "ANNUAL";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GoalSettingDeadline { get; set; }
        public DateTime SelfAssessmentDeadline { get; set; }
        public DateTime ManagerAssessmentDeadline { get; set; }
        public DateTime CalibrationDeadline { get; set; }
        public string? Description { get; set; }
    }

    public class KpiMetricCatalogDto
    {
        public Guid Id { get; set; }
        public string MetricCode { get; set; } = string.Empty;
        public string MetricName { get; set; } = string.Empty;
        public string ComponentType { get; set; } = string.Empty; // QUANTITATIVE_KPI, CORE_VALUES_BEHAVIOR
        public string Category { get; set; } = string.Empty;
        public string TargetProfessionGroup { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public string TargetPolarity { get; set; } = "POSITIVE";
        public decimal DefaultTargetValue { get; set; }
        public string? ScoringRubricJson { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateKpiMetricCatalogRequestDto
    {
        public string MetricCode { get; set; } = string.Empty;
        public string MetricName { get; set; } = string.Empty;
        public string ComponentType { get; set; } = "QUANTITATIVE_KPI";
        public string Category { get; set; } = string.Empty;
        public string TargetProfessionGroup { get; set; } = "ALL";
        public string UnitOfMeasure { get; set; } = "PERSEN";
        public string TargetPolarity { get; set; } = "POSITIVE";
        public decimal DefaultTargetValue { get; set; }
        public string? ScoringRubricJson { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class PerformanceGoalDto
    {
        public Guid Id { get; set; }
        public Guid PerformanceCycleId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public Guid MetricCatalogId { get; set; }
        public string MetricCode { get; set; } = string.Empty;
        public string MetricName { get; set; } = string.Empty;
        public decimal WeightPercentage { get; set; }
        public decimal TargetValue { get; set; }
        public string TargetPolarity { get; set; } = "POSITIVE";
        public Guid? ApprovedByManagerId { get; set; }
        public bool IsLocked { get; set; }
    }

    public class CascadePerformanceGoalsRequestDto
    {
        public Guid PerformanceCycleId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public List<PerformanceGoalItemDto> Goals { get; set; } = new();
    }

    public class PerformanceGoalItemDto
    {
        public Guid MetricCatalogId { get; set; }
        public decimal WeightPercentage { get; set; }
        public decimal TargetValue { get; set; }
        public string TargetPolarity { get; set; } = "POSITIVE";
    }

    // =========================================================================
    // 2. EVALUASI KINERJA BERJENJANG (Kontrak v21 - Tag 27.2)
    // =========================================================================

    public class PerformanceAppraisalDto
    {
        public Guid Id { get; set; }
        public Guid PerformanceCycleId { get; set; }
        public string CycleName { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // DRAFT, SELF_SUBMITTED, MANAGER_REVIEWED, SECOND_LINE_APPROVED, CALIBRATED, RELEASED, ACKNOWLEDGED, DISPUTED
        public decimal SelfScoreQuantitative { get; set; }
        public decimal SelfScoreBehavioral { get; set; }
        public decimal SelfOverallScore { get; set; }
        public decimal ManagerScoreQuantitative { get; set; }
        public decimal ManagerScoreBehavioral { get; set; }
        public decimal ManagerOverallScore { get; set; }
        public decimal CalibratedScore { get; set; }
        public decimal FinalScore { get; set; }
        public string FinalGrade { get; set; } = string.Empty; // SANGAT_BAIK, BAIK, CUKUP, KURANG, SANGAT_KURANG
        public bool HasDispute { get; set; }
        public string? ManagerFeedbackNotes { get; set; }
        public string? SecondLineReviewNotes { get; set; }
        public string? EmployeeAcknowledgmentNotes { get; set; }
        public List<PerformanceAppraisalDetailDto> Details { get; set; } = new();
    }

    public class PerformanceAppraisalDetailDto
    {
        public Guid Id { get; set; }
        public Guid MetricCatalogId { get; set; }
        public string MetricCode { get; set; } = string.Empty;
        public string MetricName { get; set; } = string.Empty;
        public string ComponentType { get; set; } = string.Empty;
        public decimal WeightPercentage { get; set; }
        public decimal TargetValue { get; set; }
        public decimal ActualValue { get; set; }
        public decimal SelfRatingScore { get; set; }
        public decimal ManagerRatingScore { get; set; }
        public decimal WeightedScore { get; set; }
        public string? EvidenceUrl { get; set; }
        public string? Comments { get; set; }
    }

    public class SubmitSelfAppraisalRequestDto
    {
        public Guid PerformanceCycleId { get; set; }
        public List<SubmitAppraisalDetailDto> Details { get; set; } = new();
        public string? SelfNotes { get; set; }
    }

    public class SubmitAppraisalDetailDto
    {
        public Guid MetricCatalogId { get; set; }
        public decimal ActualValue { get; set; }
        public decimal RatingScore { get; set; }
        public string? EvidenceUrl { get; set; }
        public string? Comments { get; set; }
    }

    public class SubmitManagerAppraisalRequestDto
    {
        public List<SubmitManagerAppraisalDetailDto> Details { get; set; } = new();
        public string? ManagerFeedbackNotes { get; set; }
        public string? RecommendedIdp { get; set; }
    }

    public class SubmitManagerAppraisalDetailDto
    {
        public Guid AppraisalDetailId { get; set; }
        public decimal ManagerRatingScore { get; set; }
        public string? ManagerComments { get; set; }
    }

    public class ProcessSecondLineReviewDto
    {
        public string Decision { get; set; } = "APPROVE"; // APPROVE, REQUEST_REVISION
        public string? SecondLineNotes { get; set; }
    }

    public class AcknowledgeAppraisalRequestDto
    {
        public bool IsAgreed { get; set; } = true;
        public string? EmployeeFeedbackNotes { get; set; }
    }

    // =========================================================================
    // 3. KALIBRASI & RESOLUSI SENGKETA (Kontrak v21 - Tag 27.3)
    // =========================================================================

    public class CalibrationDistributionMatrixDto
    {
        public int TotalStaff { get; set; }
        public int CountGradeA { get; set; }
        public decimal PercentageGradeA { get; set; }
        public int CountGradeB { get; set; }
        public decimal PercentageGradeB { get; set; }
        public int CountGradeC { get; set; }
        public decimal PercentageGradeC { get; set; }
        public int CountGradeD { get; set; }
        public decimal PercentageGradeD { get; set; }
        public int CountGradeE { get; set; }
        public decimal PercentageGradeE { get; set; }
        public bool IsWithinGuidingDistribution { get; set; }
    }

    public class CalibrationSessionDto
    {
        public Guid Id { get; set; }
        public Guid PerformanceCycleId { get; set; }
        public string SessionCode { get; set; } = string.Empty;
        public DateTime SessionDate { get; set; }
        public Guid CommitteeChairpersonId { get; set; }
        public string Status { get; set; } = string.Empty; // SCHEDULED, IN_PROGRESS, FINALIZED
        public string? MinutesOfMeeting { get; set; }
    }

    public class CreateCalibrationSessionRequestDto
    {
        public Guid PerformanceCycleId { get; set; }
        public string SessionCode { get; set; } = string.Empty;
        public DateTime SessionDate { get; set; }
        public Guid CommitteeChairpersonId { get; set; }
        public string? Notes { get; set; }
    }

    public class OverrideCalibrationScoreRequestDto
    {
        public Guid AppraisalId { get; set; }
        public decimal CalibratedScore { get; set; }
        public string OverrideJustificationNotes { get; set; } = string.Empty; // Wajib minimal 50 karakter (VR-PRF-05, VR-PRF-12)
    }

    public class FinalizeCalibrationSessionRequestDto
    {
        public string? MinutesOfMeeting { get; set; }
        public bool ReleaseImmediatelyToEss { get; set; } = true;
    }

    public class SubmitAppraisalDisputeRequestDto
    {
        public Guid AppraisalId { get; set; }
        public string DisputeReason { get; set; } = string.Empty;
        public string? CounterEvidenceUrl { get; set; }
    }

    public class AppraisalDisputeDto
    {
        public Guid Id { get; set; }
        public Guid AppraisalId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string DisputeReason { get; set; } = string.Empty;
        public string? CounterEvidenceUrl { get; set; }
        public string Status { get; set; } = string.Empty; // SUBMITTED, IN_MEDIATION, ADJUDICATED, REJECTED
        public string? InvestigationNotes { get; set; }
        public decimal? FinalResolutionScore { get; set; }
        public Guid? AdjudicatedById { get; set; }
        public DateTime? AdjudicatedDateTime { get; set; }
    }

    public class AdjudicateDisputeRequestDto
    {
        public string ResolutionDecision { get; set; } = "UPHELD"; // UPHELD, REVISED, REJECTED
        public decimal? NewFinalScore { get; set; }
        public string ResolutionNotes { get; set; } = string.Empty;
    }

    // =========================================================================
    // 4. PROGRAM PIP & JENJANG KLINIS NAKES (Kontrak v21 - Tag 27.4)
    // =========================================================================

    public class PipActiveSummaryDto
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public Guid AppraisalId { get; set; }
        public decimal BaselineScore { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid MentorStaffId { get; set; }
        public string Status { get; set; } = string.Empty; // ACTIVE, EXTENDED, PASSED, FAILED
        public int ExtensionCount { get; set; }
    }

    public class IssuePipContractRequestDto
    {
        public Guid AppraisalId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public Guid MentorStaffId { get; set; }
        public string ImprovementObjectives { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class RecordPipWeeklyLogRequestDto
    {
        public int WeekNumber { get; set; } // 1 - 8
        public string ActionItemCompleted { get; set; } = string.Empty;
        public string MentorObservationNotes { get; set; } = string.Empty; // Wajib minimal 20 karakter (VR-PRF-09, VR-PRF-15)
        public string ProgressStatus { get; set; } = "ON_TRACK"; // ON_TRACK, NEEDS_ATTENTION, OFF_TRACK
    }

    public class VerdictPipRequestDto
    {
        public string Verdict { get; set; } = "PASS"; // PASS, EXTEND_30_DAYS, FAIL_ESCALATE_LEGAL
        public string VerdictNotes { get; set; } = string.Empty;
    }

    public class ApplyClinicalLadderRequestDto
    {
        public string CurrentClinicalLevel { get; set; } = string.Empty; // PK_I, PK_II, PK_III, PK_IV, PK_V
        public string RequestedClinicalLevel { get; set; } = string.Empty;
        public string PortfolioDocUrl { get; set; } = string.Empty;
        public string? LogbookProceduresSummary { get; set; }
    }

    public class ClinicalCareerLadderDto
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string CurrentLevel { get; set; } = string.Empty;
        public string RequestedLevel { get; set; } = string.Empty;
        public DateTime SubmissionDate { get; set; }
        public string Status { get; set; } = string.Empty; // SUBMITTED, IN_REVIEW, RECOMMENDED, APPROVED, REJECTED
        public Guid? CommitteeAssessorId { get; set; }
        public string? KomiteRecommendation { get; set; }
        public string? DecreeNumber { get; set; }
        public DateTime? DecreeDate { get; set; }
    }

    public class VerdictClinicalLadderRequestDto
    {
        public string Verdict { get; set; } = "RECOMMENDED"; // RECOMMENDED, NOT_RECOMMENDED, REVISION
        public string AssessorNotes { get; set; } = string.Empty;
        public string? DecreeNumber { get; set; }
    }
}
