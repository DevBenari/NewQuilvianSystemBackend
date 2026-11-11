using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs
{
    // =========================================================================
    // 1. DTOs: Health Record & Medical Privacy (HRD-DEC-176, VR-OHS-01, VR-OHS-02)
    // =========================================================================

    public class HealthRecordClinicalDetailDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public string Rhesus { get; set; } = string.Empty;
        public string CurrentFitnessStatus { get; set; } = "Fit";
        public string DecryptedClinicalHistory { get; set; } = string.Empty;
        public List<string> DiagnosisIcd10 { get; set; } = new();
        public List<string> SensitiveLabFindings { get; set; } = new();
        public List<string> MedicalAlerts { get; set; } = new();
        public DateTime LastAssessmentDate { get; set; }
        public string AssessedByDoctorName { get; set; } = string.Empty;
    }

    public class EmployeeFitnessSummaryDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string FitnessStatus { get; set; } = "Fit";
        public DateTime ValidUntil { get; set; }
        public bool HasActiveRestriction { get; set; }
        public string? RestrictionDirectives { get; set; }
        public bool IsSchedulingAllowed { get; set; } = true;
        public bool IsClinicalDutyAllowed { get; set; } = true;
        public string AssessedByDoctor { get; set; } = string.Empty;
        public DateTime LastAssessedDate { get; set; }
    }

    public class UpdateFitnessStatusRequestDto
    {
        [Required]
        public string FitnessStatus { get; set; } = "Fit"; // Fit, FitWithRestriction, TemporarilyUnfit, PermanentlyUnfit
        public DateTime ValidUntil { get; set; }
        public string? OperationalRestrictionSummary { get; set; }
        public string? DoctorHiperkesRegNumber { get; set; }
        public string? ClinicalNotesEncrypted { get; set; }
    }

    public class MyHealthProfileDto
    {
        public Guid EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string FitnessStatus { get; set; } = "Fit";
        public DateTime? LastMcuDate { get; set; }
        public string? McuResultSummary { get; set; }
        public List<VaccinationRecordDto> VaccinationHistory { get; set; } = new();
        public bool IsHepatitisBProtected { get; set; }
        public decimal? LastAntiHbsTiter { get; set; }
        public bool HasActiveWorkRestriction { get; set; }
        public string? WorkRestrictionNote { get; set; }
    }

    public class OhsExecutiveDashboardDto
    {
        public int Year { get; set; }
        public int TotalEmployees { get; set; }
        public int McuTargetEmployees { get; set; }
        public int McuCompletedEmployees { get; set; }
        public decimal McuCompliancePercentage { get; set; }
        public int TotalNeedleStickIncidents { get; set; }
        public int ActiveSerialSurveillances { get; set; }
        public int TotalWorkplaceInjuries { get; set; }
        public int LostTimeInjuryCount { get; set; }
        public int TotalLostWorkDays { get; set; }
        public int ActiveWorkRestrictions { get; set; }
        public int StaffRequiringVaccineBooster { get; set; }
        public int RadiationStaffOverThreshold { get; set; }
    }

    // =========================================================================
    // 2. DTOs: Medical Examination & Risk Surveillance (VR-OHS-12, VR-OHS-17)
    // =========================================================================

    public class ScheduleMcuRequestDto
    {
        [Required]
        public Guid EmployeeId { get; set; }
        [Required]
        public string ExamType { get; set; } = "AnnualPeriodic"; // PreEmployment, AnnualPeriodic, SpecialHazard, PostIllness
        public DateTime ExaminationDate { get; set; }
        public string? RiskProfile { get; set; } // Radiation, InfectiousBloodBorne, CytotoxicDrugs, HighNoise, ChemicalHazard, Standard
        public string? ProviderName { get; set; }
        public string? Notes { get; set; }
    }

    public class MedicalExaminationResponseDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string ExaminationNumber { get; set; } = string.Empty;
        public string ExaminationType { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string AdministrativeStatus { get; set; } = "Scheduled";
        public string? FitnessResult { get; set; }
        public string? ProviderName { get; set; }
        public DateTime? NextExaminationDate { get; set; }
        public bool IsClinicalDataRestricted { get; set; } = true;
    }

    public class MedicalExaminationSummaryDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string ExaminationNumber { get; set; } = string.Empty;
        public string ExaminationType { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string AdministrativeStatus { get; set; } = string.Empty;
        public string? FitnessResult { get; set; }
    }

    public class RecordMcuResultsRequestDto
    {
        public string? PhysicalFindingsEncrypted { get; set; }
        public string? LabResultsEncrypted { get; set; }
        public string? ThoraxXrayFindingsEncrypted { get; set; }
        [Required]
        public string FitnessConclusion { get; set; } = "Fit"; // Fit, FitWithRestriction, TemporarilyUnfit, PermanentlyUnfit
        public string? DoctorHiperkesRegNumber { get; set; }
        public DateTime? NextRecommendedMcuDate { get; set; }
        public string? AdministrativeNotes { get; set; }
    }

    public class RecordRadiationDoseRequestDto
    {
        [Required]
        public Guid EmployeeId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        [Range(0, 100)]
        public decimal DoseMsv { get; set; }
        public string? BadgeNumber { get; set; }
        public string? ReaderLaboratoryName { get; set; }
    }

    public class HealthSurveillanceResponseDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string SurveillanceNumber { get; set; } = string.Empty;
        public string SurveillanceProgram { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string SurveillanceStatus { get; set; } = "Scheduled";
        public string? AdministrativeOutcome { get; set; }
        public bool IsCompliant { get; set; }
    }

    public class RadiationSurveillanceRadarDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public decimal QuarterlyDoseMsv { get; set; }
        public decimal CumulativeYearlyDoseMsv { get; set; }
        public decimal MaxPermissibleYearlyDoseMsv { get; set; } = 20.0m;
        public bool IsOverQuarterlyThreshold { get; set; } // > 5 mSv
        public bool IsOverYearlyThreshold { get; set; } // > 20 mSv
        public string RotationRecommendation { get; set; } = "NormalDuty";
    }

    // =========================================================================
    // 3. DTOs: Needle-Stick & Occupational Exposure Emergency (VR-OHS-08..11, INT-HR-91)
    // =========================================================================

    public class ReportNeedleStickCitoRequestDto
    {
        [Required]
        public DateTime IncidentTimestamp { get; set; }
        [Required]
        public string Location { get; set; } = string.Empty;
        [Required]
        public string DeviceType { get; set; } = string.Empty;
        public string? ProcedurePerformed { get; set; }
        public bool IsHollowBoreNeedle { get; set; } = true;
        public bool WasVisibleBloodPresent { get; set; } = true;
        public bool IsSourceKnown { get; set; }
        public string? SourcePatientMrn { get; set; }
        public string? InjuryBodySite { get; set; }
        public string? ImmediateFirstAid { get; set; }
    }

    public class NeedleStickReceiptDto
    {
        public Guid IncidentId { get; set; }
        public string IncidentNumber { get; set; } = string.Empty;
        public DateTime IncidentTimestamp { get; set; }
        public string Status { get; set; } = "CitoReported";
        public int SlaPepCountdownSeconds { get; set; } // 14400 detik (4 jam)
        public string ActionDirective { get; set; } = "Segera menuju IGD / Posko K3RS untuk asesmen risiko & paket PPP < 4 jam.";
        public string EmergencyContactPhone { get; set; } = "Ext-777 (IGD K3RS Cito)";
    }

    public class NeedleStickIncidentDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string IncidentNumber { get; set; } = string.Empty;
        public DateTime IncidentDateTime { get; set; }
        public string? DeviceType { get; set; }
        public string? InjuryBodySite { get; set; }
        public bool PostExposureProphylaxisRecommended { get; set; }
        public bool PostExposureProphylaxisStarted { get; set; }
        public DateTime? PepStartedAt { get; set; }
        public string IncidentStatus { get; set; } = string.Empty;
        public DateTime? FollowUpDate { get; set; }
    }

    public class AdministerPepRequestDto
    {
        [Required]
        public string PepRegimenDescription { get; set; } = string.Empty;
        public DateTime PepAdministeredTime { get; set; }
        public string? DoctorNotes { get; set; }
        public string? DoctorHiperkesRegNumber { get; set; }
        public bool BaselineBloodSampleTaken { get; set; } = true;
    }

    public class RecordSerologyFollowupRequestDto
    {
        [Range(1, 6)]
        public int FollowUpMonth { get; set; } // 1, 3, or 6
        public DateTime TestDate { get; set; }
        [Required]
        public string OutcomeStatus { get; set; } = "NonReactive"; // NonReactive, Reactive, Inconclusive
        public string? LabDocumentUrl { get; set; }
        public string? ClinicalNotesEncrypted { get; set; }
    }

    public class OccupationalExposureDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string ExposureNumber { get; set; } = string.Empty;
        public DateTime ExposureDateTime { get; set; }
        public string ExposureType { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public string ExposureStatus { get; set; } = string.Empty;
        public bool FollowUpRequired { get; set; }
        public DateTime? FollowUpDate { get; set; }
    }

    public class ActiveExposureRadarSummaryDto
    {
        public Guid ExposureId { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime ExposureDate { get; set; }
        public int MonthsElapsed { get; set; }
        public int NextMilestoneMonth { get; set; } // 1, 3, or 6
        public DateTime NextScheduledTestDate { get; set; }
        public string Month1Status { get; set; } = "Pending";
        public string Month3Status { get; set; } = "Pending";
        public string Month6Status { get; set; } = "Pending";
        public string OverallStatus { get; set; } = "UnderSurveillance";
    }

    // =========================================================================
    // 4. DTOs: Staff Vaccination & Immunity Tracking (VR-OHS-14..16)
    // =========================================================================

    public class RecordVaccinationRequestDto
    {
        [Required]
        public Guid EmployeeId { get; set; }
        [Required]
        public string VaccineName { get; set; } = string.Empty;
        [Required]
        public string VaccineType { get; set; } = "HepatitisB"; // HepatitisB, Influenza, Varicella, MMR, Covid19
        public int DoseNumber { get; set; } = 1;
        public DateTime AdministeredDate { get; set; }
        [Required]
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public string? ProviderName { get; set; }
        public string? VaccinatorName { get; set; }
        public DateTime? NextRecommendedDoseDate { get; set; }
    }

    public class VaccinationRecordDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string VaccinationNumber { get; set; } = string.Empty;
        public string VaccineName { get; set; } = string.Empty;
        public string VaccineType { get; set; } = string.Empty;
        public int DoseNumber { get; set; }
        public DateTime? AdministrationDate { get; set; }
        public DateTime? NextDoseDate { get; set; }
        public string? BatchNumber { get; set; }
        public string VaccinationStatus { get; set; } = string.Empty;
        public bool IsCompliant { get; set; }
    }

    public class RecordAntibodyTiterRequestDto
    {
        [Required]
        public Guid EmployeeId { get; set; }
        [Range(0, 10000)]
        public decimal AntibodyTiterValue { get; set; }
        public string TiterUnit { get; set; } = "mIU/mL";
        public DateTime TestDate { get; set; }
        public string? LaboratoryName { get; set; }
        public string? TiterInterpretation { get; set; } // Protective (>=10 mIU/mL), NonProtective (<10 mIU/mL)
    }

    public class NakesVaccinationRecallDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string RecallReason { get; set; } = string.Empty; // LowAntiHbsTiter, BoosterDueDatePassed, IncompletePrimarySeries
        public decimal? LastKnownTiter { get; set; }
        public DateTime? DueDate { get; set; }
        public string UrgencyLevel { get; set; } = "High";
    }

    // =========================================================================
    // 5. DTOs: Work Restriction & Injury BPJS JKK (VR-OHS-04..07, VR-OHS-18..21, INT-HR-89, INT-HR-90)
    // =========================================================================

    public class IssueWorkRestrictionRequestDto
    {
        [Required]
        public Guid EmployeeId { get; set; }
        public Guid? FitnessToWorkId { get; set; }
        [Required]
        public string RestrictionType { get; set; } = "Temporary"; // Temporary, Conditional, PermanentAdjustment
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [Required]
        public string Directives { get; set; } = string.Empty;
        public bool DisallowNightShift { get; set; } = true;
        public bool DisallowOnCall { get; set; } = true;
        public bool DisallowHeavyLifting { get; set; } = true;
        public bool DisallowHighRiskZone { get; set; } = false;
        public int? MaxWorkHoursPerDay { get; set; }
        public string? DoctorHiperkesRegNumber { get; set; }
    }

    public class WorkRestrictionDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string RestrictionNumber { get; set; } = string.Empty;
        public string RestrictionType { get; set; } = string.Empty;
        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string RestrictionStatus { get; set; } = "Active";
        public string? AdministrativeInstruction { get; set; }
        public bool NoNightShift { get; set; }
        public bool NoOnCall { get; set; }
        public bool NoHeavyLifting { get; set; }
        public bool IsSchedulingBlocked { get; set; }
    }

    public class ScheduleValidationResultDto
    {
        public Guid EmployeeId { get; set; }
        public bool IsAllowed { get; set; }
        public string? RejectionReason { get; set; }
        public string? ActiveRestrictionNumber { get; set; }
        public DateTime? RestrictionEndDate { get; set; }
    }

    public class RecordReturnToWorkRequestDto
    {
        public string? PhysicalFindings { get; set; }
        [Required]
        public string ConclusionStatus { get; set; } = "FitForFullDuty"; // FitForFullDuty, FitWithModifiedDuty, RemainUnfit
        public bool IsRestrictionLifted { get; set; } = true;
        public DateTime ActualReturnDate { get; set; }
        public string? DoctorNotes { get; set; }
        public string? DoctorHiperkesRegNumber { get; set; }
    }

    public class ReturnToWorkAssessmentDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string AssessmentNumber { get; set; } = string.Empty;
        public DateTime AssessmentDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }
        public string FitnessStatus { get; set; } = string.Empty;
        public bool IsSchedulingAllowed { get; set; }
        public bool IsClinicalDutyAllowed { get; set; }
        public string AssessmentStatus { get; set; } = "Completed";
    }

    public class ReportEmployeeInjuryRequestDto
    {
        [Required]
        public Guid EmployeeId { get; set; }
        public DateTime Timestamp { get; set; }
        [Required]
        public string Location { get; set; } = string.Empty;
        [Required]
        public string InjuryType { get; set; } = string.Empty;
        public string? BodyPart { get; set; }
        public string SeverityLevel { get; set; } = "Moderate"; // Minor, Moderate, Severe, Fatal
        [Required]
        public string Description { get; set; } = string.Empty;
        public string? ImmediateAction { get; set; }
        public bool IsLostTimeInjury { get; set; }
        public int EstimatedLostWorkDays { get; set; }
        public string? RootCauseAnalysis { get; set; }
        public string? CapaRecommendation { get; set; }
        public List<string> InvestigatorNames { get; set; } = new();
    }

    public class EmployeeInjuryResponseDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string InjuryNumber { get; set; } = string.Empty;
        public DateTime InjuryDateTime { get; set; }
        public string InjuryType { get; set; } = string.Empty;
        public string SeverityLevel { get; set; } = string.Empty;
        public string InjuryStatus { get; set; } = "Reported";
        public bool IsLostTimeInjury { get; set; }
        public int LostWorkDays { get; set; }
        public string? BpjsStage1ClaimNumber { get; set; }
        public DateTime BpjsStage1ReportDeadline { get; set; } // H+48 jam
        public bool BpjsReportedWithin48Hours { get; set; }
    }

    public class BpjsJkkClaimFormDto
    {
        public Guid InjuryId { get; set; }
        public int Stage { get; set; } // 1 or 2
        public string FormTitle { get; set; } = string.Empty;
        public string FormCode { get; set; } = string.Empty; // Formulir 3a BPJS Ketenagakerjaan
        public string EmployeeName { get; set; } = string.Empty;
        public string BpjsKetenagakerjaanNumber { get; set; } = string.Empty;
        public DateTime IncidentDateTime { get; set; }
        public DateTime ReportDeadline { get; set; }
        public string DeadlineStatus { get; set; } = "OnTime"; // OnTime, Warning, Overdue
        public string FormPdfDownloadUrl { get; set; } = string.Empty;
        public decimal? ReimbursementAmountClaimed { get; set; }
        public string ReimbursementStatus { get; set; } = "PendingVerification";
    }
}
