using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs
{


    // =========================================================================
    // TAG 1: EMPLOYEE DISCIPLINARY & JUST CULTURE
    // =========================================================================
    public class CreateIncidentReportRequestDto
    {
        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public DateTime IncidentDate { get; set; }

        [Required]
        [MaxLength(255)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        public string Category { get; set; } = string.Empty; // Attendance, Behavioral, ClinicalSafety, Ethics, SOPViolation

        [Required]
        [MaxLength(32)]
        public string JustCultureClassification { get; set; } = "HumanError"; // HumanError, AtRiskBehavior, RecklessMisconduct

        [Required]
        [MinLength(50, ErrorMessage = "Deskripsi insiden minimal 50 karakter sesuai aturan VR-EMP-01")]
        public string Description { get; set; } = string.Empty;

        public string? ReporterType { get; set; } = "Superior";
        public Guid? ReporterEmployeeId { get; set; }
        public string? KmkpAuditRef { get; set; }
    }

    public class IncidentReportResponseDto
    {
        public Guid IncidentReportId { get; set; }
        public string IncidentNumber { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public DateTime IncidentDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string JustCultureClassification { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? KmkpAuditRef { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class IncidentReportDetailDto : IncidentReportResponseDto
    {
        public string ReporterType { get; set; } = string.Empty;
        public Guid? ReporterEmployeeId { get; set; }
        public List<InvestigationRecordResponseDto> Investigations { get; set; } = new();
    }

    public class InitiateInvestigationRequestDto
    {
        [Required]
        public Guid IncidentReportId { get; set; }

        [Required]
        public DateTime HearingDate { get; set; }

        [Required]
        public string InvestigatorNames { get; set; } = string.Empty; // Minimal 2 pemeriksa sesuai VR-EMP-04

        public string? WitnessStatements { get; set; }
        public string? EvidenceDocumentUrls { get; set; }
    }

    public class RecordBapFindingsRequestDto
    {
        [Required]
        public string EmployeeDefense { get; set; } = string.Empty; // Wajib terisi sesuai VR-EMP-05

        [Required]
        public string Findings { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        public string RecommendedAction { get; set; } = "Coaching"; // Coaching, VerbalWarning, SP1, SP2, SP3, Suspension, Termination
    }

    public class InvestigationRecordResponseDto
    {
        public Guid InvestigationId { get; set; }
        public Guid IncidentReportId { get; set; }
        public string BapNumber { get; set; } = string.Empty;
        public DateTime HearingDate { get; set; }
        public string InvestigatorNames { get; set; } = string.Empty;
        public string? WitnessStatements { get; set; }
        public string? EvidenceDocumentUrls { get; set; }
        public string? EmployeeDefense { get; set; }
        public string Findings { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class IssueSanctionRequestDto
    {
        public Guid? InvestigationId { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        [MaxLength(32)]
        public string SanctionLevel { get; set; } = "SP1"; // VerbalWarning, SP1, SP2, SP3

        [Required]
        public DateTime IssuedDate { get; set; }

        [Required]
        public string ViolationDescription { get; set; } = string.Empty;

        public bool IsDirectorApprovedBypass { get; set; } = false; // Bypass langsung SP3 sesuai VR-EMP-09
    }

    public class SanctionResponseDto
    {
        public Guid SanctionId { get; set; }
        public Guid? InvestigationId { get; set; }
        public Guid EmployeeId { get; set; }
        public string SanctionNumber { get; set; } = string.Empty;
        public string SanctionLevel { get; set; } = string.Empty;
        public DateTime IssuedDate { get; set; }
        public DateTime ValidUntil { get; set; } // IssuedDate + 6 bulan kalender (PP 35/2021)
        public string ViolationDescription { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsAppealed { get; set; }
        public string? AppealResolution { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AppealSanctionRequestDto
    {
        [Required]
        public string AppealStatement { get; set; } = string.Empty;

        public string? SupportingEvidenceUrls { get; set; }
    }

    public class SanctionAppealResponseDto
    {
        public Guid SanctionId { get; set; }
        public string SanctionNumber { get; set; } = string.Empty;
        public bool IsAppealed { get; set; }
        public DateTime AppealedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class IssueSuspensionRequestDto
    {
        [Required]
        public Guid InvestigationId { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime ExpectedEndDate { get; set; } // Maksimal 180 hari / 6 bulan
    }

    public class ReinstateEmployeeRequestDto
    {
        [Required]
        public DateTime ActualEndDate { get; set; }

        [Required]
        public string ReinstatementNotes { get; set; } = string.Empty;
    }

    public class SuspensionResponseDto
    {
        public Guid SuspensionId { get; set; }
        public Guid InvestigationId { get; set; }
        public Guid EmployeeId { get; set; }
        public string SuspensionNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime ExpectedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public string WagePaymentStatus { get; set; } = "PaidFull";
        public bool IsSystemAccessRevoked { get; set; } = true;
        public bool IsPhysicalAccessRevoked { get; set; } = true;
        public string Status { get; set; } = string.Empty;
        public string? ReinstatementNotes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // =========================================================================
    // TAG 2: GRIEVANCE & CONFIDENTIAL WHISTLEBLOWING
    // =========================================================================
    public class SubmitGrievanceRequestDto
    {
        [Required]
        [MaxLength(64)]
        public string Category { get; set; } = string.Empty; // InterpersonalConflict, ShiftDispute, Workload, etc.

        [MaxLength(128)]
        public string? TargetUnit { get; set; }

        [MaxLength(32)]
        public string SeverityLevel { get; set; } = "Medium";

        [Required]
        public string ReportDescription { get; set; } = string.Empty;

        public Guid? ComplainantEmployeeId { get; set; }
    }

    public class SubmitWhistleblowerRequestDto
    {
        public bool IsAnonymous { get; set; } = true;

        [Required]
        public string EncryptedPayload { get; set; } = string.Empty; // Payload terenkripsi asimetris VR-EMP-15

        [Required]
        [MaxLength(64)]
        public string Category { get; set; } = string.Empty; // Fraud, Harassment, Bribery, MedicalRecordAlteration

        [MaxLength(128)]
        public string? TargetUnit { get; set; }

        [MaxLength(32)]
        public string SeverityLevel { get; set; } = "Critical";

        public Guid? ComplainantEmployeeId { get; set; }
    }

    public class GrievanceTicketResponseDto
    {
        public Guid GrievanceId { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public Guid? ComplainantEmployeeId { get; set; }
        public bool IsWhistleblowerCase { get; set; }
        public bool IsAnonymous { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? TargetUnit { get; set; }
        public string SeverityLevel { get; set; } = string.Empty;
        public DateTime TargetResolutionDate { get; set; } // Maksimal 14 hari kerja VR-EMP-14
        public string Status { get; set; } = string.Empty;
        public string? ResolutionNotes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GrievanceTicketSummaryDto
    {
        public Guid GrievanceId { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string SeverityLevel { get; set; } = string.Empty;
        public DateTime TargetResolutionDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class WhistleblowerTicketReceiptDto
    {
        public string TicketNumber { get; set; } = string.Empty;
        public string AccessKeyHash { get; set; } = string.Empty;
        public string Message { get; set; } = "Laporan whistleblower berhasil diterima dan diamankan dengan enkripsi.";
        public DateTime SubmittedAt { get; set; }
    }

    public class WhistleblowerCaseDto
    {
        public Guid GrievanceId { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? TargetUnit { get; set; }
        public string SeverityLevel { get; set; } = string.Empty;
        public string EncryptedPayload { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime TargetResolutionDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RecordMediationResolutionRequestDto
    {
        [Required]
        public Guid MediatorEmployeeId { get; set; }

        [Required]
        public string ResolutionNotes { get; set; } = string.Empty;

        public bool IsResolved { get; set; } = true;
    }

    // =========================================================================
    // TAG 3: BIPARTITE NEGOTIATION & LABOR RELATIONS
    // =========================================================================
    public class ScheduleBipartiteMeetingRequestDto
    {
        [Required]
        public DateTime MeetingDate { get; set; }

        [Required]
        [MaxLength(64)]
        public string DisputeType { get; set; } = "RightsDispute"; // RightsDispute, InterestDispute, TerminationDispute, UnionDispute

        [Required]
        public string ManagementReps { get; set; } = string.Empty; // Minimal 1 delegasi VR-EMP-17

        [Required]
        public string UnionReps { get; set; } = string.Empty; // Minimal 1 delegasi VR-EMP-17

        [Required]
        public string Agenda { get; set; } = string.Empty;
    }

    public class RecordMinutesRequestDto
    {
        [Required]
        public string MinutesOfMeeting { get; set; } = string.Empty;

        [Required]
        public string AttendeesList { get; set; } = string.Empty;

        public string? DeadlockPoints { get; set; }
    }

    public class UploadJointAgreementRequestDto
    {
        [Required]
        public string JointAgreementDocUrl { get; set; } = string.Empty; // Wajib .pdf sesuai VR-EMP-18

        public string? RegistrationDisnakerNumber { get; set; }
    }

    public class BipartiteForumResponseDto
    {
        public Guid ForumId { get; set; }
        public string RegisterNumber { get; set; } = string.Empty;
        public DateTime MeetingDate { get; set; }
        public string DisputeType { get; set; } = string.Empty;
        public string ManagementRepresentatives { get; set; } = string.Empty;
        public string UnionRepresentatives { get; set; } = string.Empty;
        public string AgendaDescription { get; set; } = string.Empty;
        public string MinutesOfMeeting { get; set; } = string.Empty;
        public string? JointAgreementDocumentUrl { get; set; }
        public string? RegistrationDisnakerNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // =========================================================================
    // TAG 4: EMPLOYEE SEPARATION & PP35 SEVERANCE
    // =========================================================================
    public class InitiateSeparationRequestDto
    {
        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        [MaxLength(64)]
        public string SeparationReason { get; set; } = "VoluntaryResign"; // VoluntaryResign, ContractEnd, Retirement, DisciplinarySP3, SevereMisconduct, Deceased

        [Required]
        public DateTime NoticeDate { get; set; }

        [Required]
        public DateTime EffectiveDate { get; set; } // NoticeDate minimal 30 hari sebelum EffectiveDate jika VoluntaryResign (VR-EMP-19)

        public string? ResignationLetterUrl { get; set; }
    }

    public class SeparationResponseDto
    {
        public Guid SeparationId { get; set; }
        public string SeparationNumber { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string SeparationReason { get; set; } = string.Empty;
        public DateTime NoticeDate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string? ResignationLetterUrl { get; set; }
        public string? TerminationDecisionNumber { get; set; }
        public string ApprovalStatus { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CalculateSeveranceRequestDto
    {
        [Required]
        public Guid SeparationId { get; set; }

        [Required]
        [Range(0, 1000000000)]
        public decimal BaseMonthlyWage { get; set; }

        [Range(0, 365)]
        public int UnusedLeaveDays { get; set; } = 0;
    }

    public class SeveranceCalculationResponseDto
    {
        public Guid CalculationId { get; set; }
        public Guid SeparationId { get; set; }
        public decimal BaseMonthlyWage { get; set; }
        public int ServiceYears { get; set; }
        public int ServiceMonths { get; set; }
        public decimal SeverancePayAmount { get; set; } // UP
        public decimal ServiceAwardAmount { get; set; } // UPMK
        public decimal RightsCompensationAmount { get; set; } // UPH
        public decimal SeparationAllowanceAmount { get; set; } // Uang Pisah
        public decimal TotalSeveranceGross { get; set; }
        public decimal TaxDeduction { get; set; } // PPh 21 Final
        public decimal TotalSeveranceNet { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsApprovedByFinanceDirector { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SeveranceCalculationDetailDto : SeveranceCalculationResponseDto
    {
        public string SeparationReason { get; set; } = string.Empty;
        public string LegalReference { get; set; } = "PP No. 35 Tahun 2021 Pasal 40 s.d. 59";
        public string CalculationFormulaNotes { get; set; } = string.Empty;
        public string? FinanceApprovalNotes { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }

    public class ApproveSeveranceRequestDto
    {
        public string? Notes { get; set; }

        [Required]
        public bool IsApproved { get; set; } = true;
    }

    // =========================================================================
    // TAG 5: MULTI-DEPARTMENT EXIT CLEARANCE & ASSET HANDOVER
    // =========================================================================
    public class ExitClearanceChecklistDto
    {
        public Guid ChecklistId { get; set; }
        public Guid ClearanceProcessId { get; set; }
        public string DepartmentType { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public bool IsReturned { get; set; }
        public decimal OutstandingLiabilityAmount { get; set; }
        public string? ApproverNotes { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ExitClearanceProcessDetailDto
    {
        public Guid ClearanceProcessId { get; set; }
        public Guid SeparationId { get; set; }
        public Guid EmployeeId { get; set; }
        public DateTime InitiatedDate { get; set; }
        public DateTime TargetCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public string OverallClearanceStatus { get; set; } = string.Empty;
        public bool IsFinalPayrollUnlocked { get; set; }
        public string? FinalizationNotes { get; set; }
        public List<ExitClearanceChecklistDto> Checklists { get; set; } = new();
    }

    public class SignoffChecklistRequestDto
    {
        [Required]
        public bool IsReturned { get; set; } = true;

        public decimal OutstandingLiabilityAmount { get; set; } = 0m;

        public string? ApproverNotes { get; set; }

        public Guid? ApproverUserId { get; set; }
    }

    public class FinalizeExitClearanceRequestDto
    {
        public string? Notes { get; set; }
    }

    public class PayrollLockStatusResponseDto
    {
        public Guid EmployeeId { get; set; }
        public bool IsFinalPayrollUnlocked { get; set; }
        public string OverallStatus { get; set; } = string.Empty;
        public List<string> PendingDepartments { get; set; } = new();
        public decimal TotalOutstandingLiability { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Identitas pelapor keluhan yang diturunkan dari token pengguna login (remediasi kelompok G, 10 November 2026).
    /// </summary>
    /// <param name="WorkforceProfileId">Profil tenaga kerja pelapor (klaim workforce_profile_id, dengan cadangan employee_id lalu ID akun).</param>
    /// <param name="EmployeeId">Data karyawan pelapor bila ada (klaim employee_id).</param>
    /// <param name="UserId">Akun pelapor.</param>
    public sealed record GrievanceComplainantIdentity(Guid WorkforceProfileId, Guid? EmployeeId, Guid UserId);
}
