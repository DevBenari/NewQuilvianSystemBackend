using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    public class PreviewOvertimeRealizationRequest
    {
        public bool AllowUnprocessedAttendance { get; set; } = false;
        public bool IncludeRateBreakdown { get; set; } = true;
    }

    public class CalculateOvertimeRealizationRequest
    {
        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool SubmitForVerification { get; set; } = true;
        public bool ForceNewVersion { get; set; } = false;

        [MaxLength(120)]
        public string? IdempotencyKey { get; set; }
    }

    public class SubmitOvertimeRealizationRequest
    {
        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class CancelOvertimeRealizationRequest
    {
        [Required, MaxLength(2000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class OvertimeRealizationIssueResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Severity { get; set; } = "Warning";
        public string Message { get; set; } = string.Empty;
        public bool IsBlocking { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? Field { get; set; }
    }

    public class OvertimeAttendanceEvidenceResponse
    {
        public Guid AttendanceDailyId { get; set; }
        public Guid? AttendanceId { get; set; }
        public DateOnly AttendanceDate { get; set; }
        public string AttendanceStatus { get; set; } = string.Empty;
        public string ProcessingStatus { get; set; } = string.Empty;
        public int ProcessingVersion { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? FirstCheckInAt { get; set; }
        public DateTime? LastCheckOutAt { get; set; }
        public bool HasMissingPunch { get; set; }
        public bool IsCorrected { get; set; }
        public bool IsLocked { get; set; }
        public List<Guid> SegmentIds { get; set; } = new();
        public List<Guid> RawLogIds { get; set; } = new();
    }

    public class OvertimeMatchedIntervalResponse
    {
        public Guid OvertimeRequestDetailId { get; set; }
        public Guid? AttendanceDailyId { get; set; }
        public Guid? AttendanceId { get; set; }
        public Guid? AttendanceSegmentId { get; set; }
        public Guid? ShiftAssignmentId { get; set; }
        public Guid? StartRawLogId { get; set; }
        public Guid? EndRawLogId { get; set; }
        public string SegmentType { get; set; } = string.Empty;
        public string SegmentSource { get; set; } = string.Empty;
        public DateTime SourceStartAt { get; set; }
        public DateTime SourceEndAt { get; set; }
        public DateTime MatchedStartAt { get; set; }
        public DateTime MatchedEndAt { get; set; }
        public int MatchedMinutes { get; set; }
        public bool IsCorrected { get; set; }
        public bool IsFallbackFromDaily { get; set; }
    }

    public class OvertimeRateBreakdownResponse
    {
        public Guid OvertimeRequestDetailId { get; set; }
        public Guid? AttendanceDailyId { get; set; }
        public Guid? AttendanceId { get; set; }
        public Guid? ShiftAssignmentId { get; set; }
        public DateOnly OvertimeDate { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public int Minutes { get; set; }
        public int MinutePositionStart { get; set; }
        public string DayType { get; set; } = string.Empty;
        public Guid OvertimeRateId { get; set; }
        public string OvertimeRateCode { get; set; } = string.Empty;
        public string OvertimeRateName { get; set; } = string.Empty;
        public string TimeBand { get; set; } = string.Empty;
        public string CalculationMethod { get; set; } = string.Empty;
        public decimal RateMultiplier { get; set; }
        public decimal? FixedAmount { get; set; }
        public bool NominalCalculationDeferredToPayroll { get; set; } = true;
    }

    public class OvertimeRequestDetailCalculationPreviewResponse
    {
        public Guid OvertimeRequestDetailId { get; set; }
        public int SequenceNumber { get; set; }
        public DateOnly OvertimeDate { get; set; }
        public DateTime ApprovedStartAt { get; set; }
        public DateTime ApprovedEndAt { get; set; }
        public int ApprovedMinutes { get; set; }
        public string DayType { get; set; } = string.Empty;
        public string OvertimeCategory { get; set; } = string.Empty;
        public string MatchStatus { get; set; } = string.Empty;
        public int RawMatchedMinutes { get; set; }
        public int ObservedBreakMinutes { get; set; }
        public int AppliedBreakMinutes { get; set; }
        public int ThresholdMinutes { get; set; }
        public int NetMinutesBeforeRounding { get; set; }
        public int RoundedMinutes { get; set; }
        public int EligibleMinutes { get; set; }
        public int VarianceFromApprovedMinutes { get; set; }
        public List<OvertimeAttendanceEvidenceResponse> AttendanceEvidence { get; set; } = new();
        public List<OvertimeMatchedIntervalResponse> MatchedIntervals { get; set; } = new();
        public List<OvertimeRateBreakdownResponse> RateBreakdown { get; set; } = new();
        public List<OvertimeRealizationIssueResponse> Issues { get; set; } = new();
    }

    public class OvertimeRealizationPreviewResponse
    {
        public Guid OvertimeRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid OvertimePolicyId { get; set; }
        public string OvertimePolicyCode { get; set; } = string.Empty;
        public string OvertimePolicyName { get; set; } = string.Empty;
        public bool RequireAttendanceMatch { get; set; }
        public bool RequirePostVerification { get; set; }
        public string InputFingerprint { get; set; } = string.Empty;
        public int RequestedMinutes { get; set; }
        public int ApprovedMinutes { get; set; }
        public int ActualMinutes { get; set; }
        public int BreakMinutes { get; set; }
        public int EligibleMinutes { get; set; }
        public int VarianceMinutes { get; set; }
        public bool CanCalculate { get; set; }
        public List<OvertimeRequestDetailCalculationPreviewResponse> Details { get; set; } = new();
        public List<OvertimeRealizationIssueResponse> Issues { get; set; } = new();
    }

    public class OvertimeRealizationMutationResponse
    {
        public Guid OvertimeRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public Guid OvertimeRealizationId { get; set; }
        public string RealizationNumber { get; set; } = string.Empty;
        public int RealizationVersion { get; set; }
        public string RealizationStatus { get; set; } = string.Empty;
        public string InputFingerprint { get; set; } = string.Empty;
        public int ActualMinutes { get; set; }
        public int BreakMinutes { get; set; }
        public int EligibleMinutes { get; set; }
        public int VarianceMinutes { get; set; }
        public bool IsIdempotentResult { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }
}
