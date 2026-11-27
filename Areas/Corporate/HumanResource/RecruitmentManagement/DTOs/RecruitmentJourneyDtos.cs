using System;
using System.Collections.Generic;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs
{
    public static class RecruitmentJourneyStageKeys
    {
        public const string Registration = "registration";
        public const string AdministrationScreening = "administration_screening";
        public const string Evaluation = "evaluation";
        public const string JobOffer = "job_offer";
        public const string PreEmploymentMcu = "pre_employment_mcu";
        public const string HandoffOnboarding = "handoff_onboarding";
    }

    public static class RecruitmentJourneyStageStatuses
    {
        public const string Completed = "COMPLETED";
        public const string InProgress = "IN_PROGRESS";
        public const string Waiting = "WAITING";
        public const string Blocked = "BLOCKED";
        public const string Rejected = "REJECTED";
        public const string NotApplicable = "NOT_APPLICABLE";
        public const string Restricted = "RESTRICTED";
        public const string Unknown = "UNKNOWN";
    }

    public static class RecruitmentJourneyOutcomeStatuses
    {
        public const string Active = "ACTIVE";
        public const string Hired = "HIRED";
        public const string Rejected = "REJECTED";
        public const string Withdrawn = "WITHDRAWN";
        public const string TalentPool = "TALENT_POOL";
        public const string McuUnfitRevoked = "MCU_UNFIT_REVOKED";
        public const string OfferDeclined = "OFFER_DECLINED";
        public const string OfferExpired = "OFFER_EXPIRED";
        public const string CancelledOnboarding = "CANCELLED_ONBOARDING";
        public const string Blocked = "BLOCKED";
    }

    public sealed class RecruitmentJourneyResponse
    {
        public Guid ApplicationId { get; set; }
        public Guid CandidateId { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public DateTime ApplicationDate { get; set; }
        public JourneyCandidateSummaryDto CandidateSummary { get; set; } = new();
        public JourneyVacancySummaryDto VacancySummary { get; set; } = new();
        public string CurrentStage { get; set; } = string.Empty;
        public string CurrentStageKey { get; set; } = string.Empty;
        public string CurrentStageLabel { get; set; } = string.Empty;
        public string StageStatus { get; set; } = string.Empty;
        public string CandidateStatus { get; set; } = string.Empty;
        public string JourneyOutcome { get; set; } = string.Empty;
        public string JourneyOutcomeLabel { get; set; } = string.Empty;
        public bool IsFastTrackFromTalentPool { get; set; }
        public int OtherApplicationsCount { get; set; }
        public List<JourneyStageDto> Stages { get; set; } = new();
        public List<JourneyActionDto> AllowedActions { get; set; } = new();
        public List<JourneyBlockerDto> Blockers { get; set; } = new();
        public JourneyNextActionDto? NextAction { get; set; }
        public List<JourneyHistoryItemDto> History { get; set; } = new();
        public JourneyHandoffRefDto? HandoffReference { get; set; }
        public JourneyOnboardingRefDto? OnboardingCaseReference { get; set; }
        public JourneyTalentPoolRefDto? TalentPoolReference { get; set; }
        public JourneyDataCompletenessDto DataCompleteness { get; set; } = new();
        public JourneyViewerPermissionsDto Permissions { get; set; } = new();
    }

    public sealed class JourneyCandidateSummaryDto
    {
        public Guid CandidateId { get; set; }
        public string CandidateCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string MaskedNationalIdNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsRehire { get; set; }
        public Guid? PreviousWorkforceProfileId { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public sealed class JourneyVacancySummaryDto
    {
        public Guid VacancyId { get; set; }
        public string VacancyNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool IsClinical { get; set; }
        public string Status { get; set; } = string.Empty;
        public int QuotaSeats { get; set; }
        public DateTime? PublishDate { get; set; }
        public DateTime? ClosingDate { get; set; }
        public Guid? RequisitionId { get; set; }
        public string? RequisitionNumber { get; set; }
        public Guid? HeadcountRequestId { get; set; }
        public string? HospitalSiteName { get; set; }
        public string? OrganizationUnitName { get; set; }
        public string? PositionName { get; set; }
    }

    public sealed class JourneyStageDto
    {
        public string StageKey { get; set; } = string.Empty;
        public int StageOrder { get; set; }
        public string StageTitle { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public bool IsCurrentStage { get; set; }
        public string ResponsibleRole { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? ReferenceNumber { get; set; }
        public bool IsRestricted { get; set; }
        public string? RestrictionReason { get; set; }
        public List<JourneyBlockerDto> Blockers { get; set; } = new();
        public List<JourneyActionDto> AllowedActions { get; set; } = new();
        public JourneyRegistrationStageDetailDto? RegistrationDetail { get; set; }
        public JourneyScreeningStageDetailDto? ScreeningDetail { get; set; }
        public JourneyEvaluationStageDetailDto? EvaluationDetail { get; set; }
        public JourneyOfferStageDetailDto? OfferDetail { get; set; }
        public JourneyMcuStageDetailDto? McuDetail { get; set; }
        public JourneyHandoffStageDetailDto? HandoffDetail { get; set; }
    }

    public sealed class JourneyRegistrationStageDetailDto
    {
        public string ApplicationNumber { get; set; } = string.Empty;
        public DateTime ApplicationDate { get; set; }
        public int TotalDocuments { get; set; }
        public int VerifiedDocuments { get; set; }
        public bool HasStrDocument { get; set; }
        public bool HasExpiredStrDocument { get; set; }
        public bool IsFastTrackFromTalentPool { get; set; }
        public string? Notes { get; set; }
    }

    public sealed class JourneyScreeningStageDetailDto
    {
        public Guid ScreeningId { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsStrRequired { get; set; }
        public string ClinicalScreeningResult { get; set; } = string.Empty;
        public string? StrNumber { get; set; }
        public DateOnly? StrExpiryDate { get; set; }
        public DateTime ScreeningDate { get; set; }
        public string? Notes { get; set; }
    }

    public sealed class JourneyEvaluationStageDetailDto
    {
        public bool IsClinicalVacancy { get; set; }
        public bool HrCompleted { get; set; }
        public bool HrPassed { get; set; }
        public bool ClinicalRequired { get; set; }
        public bool ClinicalCompleted { get; set; }
        public bool ClinicalPassed { get; set; }
        public bool HasClinicalVeto { get; set; }
        public bool HasHiddenOtherEvaluatorSessions { get; set; }
        public List<JourneyEvaluationSessionDto> Sessions { get; set; } = new();
    }

    public sealed class JourneyEvaluationSessionDto
    {
        public Guid Id { get; set; }
        public string EvaluationType { get; set; } = string.Empty;
        public Guid EvaluatorUserId { get; set; }
        public string EvaluatorName { get; set; } = string.Empty;
        public string EvaluatorRole { get; set; } = string.Empty;
        public DateTime EvaluationDate { get; set; }
        public bool IsSubmitted { get; set; }
        public decimal? Score { get; set; }
        public string Recommendation { get; set; } = string.Empty;
        public bool IsClinicalVetoApplied { get; set; }
        public string? VetoReason { get; set; }
        public string? Notes { get; set; }
    }

    public sealed class JourneyOfferStageDetailDto
    {
        public Guid OfferId { get; set; }
        public string OfferNumber { get; set; } = string.Empty;
        public string OfferedPositionTitle { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ApprovalTier { get; set; } = string.Empty;
        public string? ContractTypeName { get; set; }
        public int? ContractDurationMonths { get; set; }
        public DateOnly ProposedJoinDate { get; set; }
        public DateTime OfferDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsExpired { get; set; }
        public DateTime? CandidateResponseDate { get; set; }
        public string? CandidateResponseNotes { get; set; }
        public bool IsSalaryMasked { get; set; }
        public decimal? OfferedBasicSalary { get; set; }
        public decimal? MaximumGradeSalary { get; set; }
        public string? SalaryGradeName { get; set; }
        public List<JourneyOfferApprovalStepDto> Approvals { get; set; } = new();
    }

    public sealed class JourneyOfferApprovalStepDto
    {
        public Guid Id { get; set; }
        public int StepNumber { get; set; }
        public string ApproverRole { get; set; } = string.Empty;
        public string? ApproverName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? DecisionDate { get; set; }
        public string? Notes { get; set; }
    }

    public sealed class JourneyMcuStageDetailDto
    {
        public Guid McuClearanceId { get; set; }
        public string ReferralNumber { get; set; } = string.Empty;
        public DateTime ReferralDate { get; set; }
        public DateTime? CheckDate { get; set; }
        public string ClinicOrFacility { get; set; } = string.Empty;
        public string ClearanceResult { get; set; } = string.Empty;
        public string? ExaminingDoctorName { get; set; }
        public bool IsMedicalDetailsMasked { get; set; }
        public string? DoctorNotes { get; set; }
        public string? CertificateFilePath { get; set; }
    }

    public sealed class JourneyHandoffStageDetailDto
    {
        public Guid? HandoffId { get; set; }
        public string? HandoffNumber { get; set; }
        public string? HandoffStatus { get; set; }
        public DateTime? HandoffDate { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public Guid? HeadcountRequestSeatId { get; set; }
        public int? SeatNumber { get; set; }
        public bool IsRehireProfileReused { get; set; }
        public string? Notes { get; set; }
        public bool HasOnboardingCase { get; set; }
        public bool NeedsOnboardingRecovery { get; set; }
    }

    public sealed class JourneyActionDto
    {
        public string ActionCode { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string StageKey { get; set; } = string.Empty;
        public string ActionType { get; set; } = "API";
        public string? Method { get; set; }
        public string? Endpoint { get; set; }
        public string? RoutePath { get; set; }
        public string? RequiredPermission { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public sealed class JourneyBlockerDto
    {
        public string BlockerCode { get; set; } = string.Empty;
        public string StageKey { get; set; } = string.Empty;
        public string Severity { get; set; } = "BLOCKING";
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ResolutionHint { get; set; }
    }

    public sealed class JourneyNextActionDto
    {
        public string ActionCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string StageKey { get; set; } = string.Empty;
        public string ResponsibleRole { get; set; } = string.Empty;
        public string? TargetRoute { get; set; }
    }

    public sealed class JourneyHistoryItemDto
    {
        public DateTime Timestamp { get; set; }
        public string StageKey { get; set; } = string.Empty;
        public string EventCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ActorName { get; set; }
    }

    public sealed class JourneyHandoffRefDto
    {
        public Guid HandoffId { get; set; }
        public string HandoffNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime HandoffDate { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public Guid? HeadcountRequestSeatId { get; set; }
        public int? SeatNumber { get; set; }
    }

    public sealed class JourneyOnboardingRefDto
    {
        public Guid OnboardingCaseId { get; set; }
        public string CaseNumber { get; set; } = string.Empty;
        public string CaseStatus { get; set; } = string.Empty;
        public DateOnly PlannedStartDate { get; set; }
        public bool IsRestricted { get; set; }
    }

    public sealed class JourneyTalentPoolRefDto
    {
        public Guid TalentPoolId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal RankingScore { get; set; }
        public int RankingPosition { get; set; }
        public DateTime EnrolledDate { get; set; }
        public DateTime RetentionExpiryDate { get; set; }
        public Guid? PromotedToOfferId { get; set; }
    }

    public sealed class JourneyDataCompletenessDto
    {
        public string CompletenessStatus { get; set; } = "COMPLETE";
        public List<string> MissingOrRestrictedDomains { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public sealed class JourneyViewerPermissionsDto
    {
        public bool CanUpdateCandidate { get; set; }
        public bool CanReadScreening { get; set; }
        public bool CanEvaluateScreening { get; set; }
        public bool CanReadEvaluation { get; set; }
        public bool CanViewAllEvaluations { get; set; }
        public bool CanEvaluateHr { get; set; }
        public bool CanEvaluateClinical { get; set; }
        public bool CanReadOffer { get; set; }
        public bool CanCreateOffer { get; set; }
        public bool CanUpdateOffer { get; set; }
        public bool CanReadOfferApproval { get; set; }
        public bool CanApproveOffer { get; set; }
        public bool CanReadMcu { get; set; }
        public bool CanCreateMcu { get; set; }
        public bool CanEvaluateMcu { get; set; }
        public bool CanReadHandoff { get; set; }
        public bool CanExecuteHandoff { get; set; }
        public bool CanReadTalentPool { get; set; }
        public bool CanCreateTalentPool { get; set; }
        public bool CanPromoteTalentPool { get; set; }
        public bool CanReadOnboarding { get; set; }
    }
}
