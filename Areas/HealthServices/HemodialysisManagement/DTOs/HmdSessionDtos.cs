using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs
{
    // =====================================================================
    // Penjadwalan
    // =====================================================================

    public class HmdStaffAssignmentInput
    {
        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public HmdStaffRole StaffRole { get; set; }
    }

    public class CreateHmdSessionRequest
    {
        [Required]
        public Guid EpisodeId { get; set; }

        /// <summary>Opsional; bila diisi wajib sama dengan pasien pemilik episode.</summary>
        public Guid? PatientId { get; set; }

        [Required]
        public DateOnly ScheduledDate { get; set; }

        [Required]
        public HmdShift Shift { get; set; }

        [Required]
        public DateTime ScheduledStartAt { get; set; }

        [Required]
        public DateTime ScheduledEndAt { get; set; }

        [Required]
        public Guid MachineId { get; set; }

        [Required]
        public Guid StationId { get; set; }

        public Guid? ResponsibleDoctorId { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? InpEpisodeId { get; set; }

        /// <summary>Permintaan HD berstatus diterima yang melahirkan sesi ini.</summary>
        public Guid? OrderId { get; set; }

        public List<HmdStaffAssignmentInput> Staff { get; set; } = new();
    }

    public class RescheduleHmdSessionRequest
    {
        [Required]
        public DateOnly ScheduledDate { get; set; }

        [Required]
        public HmdShift Shift { get; set; }

        [Required]
        public DateTime ScheduledStartAt { get; set; }

        [Required]
        public DateTime ScheduledEndAt { get; set; }

        [Required]
        public Guid MachineId { get; set; }

        [Required]
        public Guid StationId { get; set; }

        public Guid? ResponsibleDoctorId { get; set; }
    }

    public class CancelHmdSessionRequest
    {
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    public class AssignHmdStaffRequest
    {
        /// <summary>Dokter penanggung jawab sesi; kosong berarti tidak diubah.</summary>
        public Guid? ResponsibleDoctorId { get; set; }

        /// <summary>Daftar lengkap petugas sesi. Petugas yang tidak disebut dilepas dari sesi.</summary>
        public List<HmdStaffAssignmentInput> Staff { get; set; } = new();
    }

    public class HmdStaffAssignmentResponse
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceName { get; set; }
        public HmdStaffRole StaffRole { get; set; }
        public string StaffRoleName { get; set; } = string.Empty;
        public Guid AssignedByUserId { get; set; }
        public DateTime AssignedAt { get; set; }
        public HmdCompetencyVerificationStatus CompetencyVerificationStatus { get; set; }
        public string CompetencyVerificationStatusName { get; set; } = string.Empty;
        public DateTime? CompetencyCheckedAt { get; set; }
        public string? CompetencySourceReference { get; set; }
    }

    public class HmdWorklistQuery
    {
        /// <summary>Tanggal jadwal; kosong berarti hari ini menurut waktu server.</summary>
        public DateOnly? Date { get; set; }
        public HmdShift? Shift { get; set; }
        public HmdSessionStatus? SessionStatus { get; set; }
        public Guid? ServiceUnitId { get; set; }

        /// <summary>Hanya sesi yang dokumentasinya belum diajukan atau belum disahkan.</summary>
        public bool? NeedsDocumentation { get; set; }

        /// <summary>Pencarian nama pasien, nomor rekam medis, atau nomor sesi.</summary>
        public string? Search { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Satu baris daftar kerja unit. Sengaja tanpa diagnosis dan tanpa status serologi: layar ini
    /// dilihat banyak orang di ruang terbuka. Isolasi hanya berupa penanda <c>true</c>/<c>false</c>.
    /// </summary>
    public class HmdWorklistItemResponse
    {
        public Guid SessionId { get; set; }
        public string SessionNumber { get; set; } = string.Empty;
        public Guid EpisodeId { get; set; }
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? MedicalRecordNumber { get; set; }
        public DateOnly ScheduledDate { get; set; }
        public HmdShift Shift { get; set; }
        public string ShiftName { get; set; } = string.Empty;
        public DateTime ScheduledStartAt { get; set; }
        public DateTime ScheduledEndAt { get; set; }
        public Guid MachineId { get; set; }
        public string? MachineCode { get; set; }
        public Guid StationId { get; set; }
        public string? StationCode { get; set; }
        public Guid? ResponsibleDoctorId { get; set; }
        public string? ResponsibleDoctorName { get; set; }
        public string? PrimaryNurseName { get; set; }
        public HmdSessionStatus SessionStatus { get; set; }
        public string SessionStatusName { get; set; } = string.Empty;
        public bool IsIsolationRequired { get; set; }
        public bool HasUnverifiedCompetency { get; set; }
        public bool NeedsDocumentation { get; set; }
    }

    public class HmdWorklistSummaryResponse
    {
        public DateOnly Date { get; set; }
        public int TotalSession { get; set; }
        public int ScheduledSession { get; set; }
        public int PreparingSession { get; set; }
        public int InProgressSession { get; set; }
        public int AwaitingDocumentationSession { get; set; }
        public int AwaitingFinalizationSession { get; set; }
        public int FinalizedSession { get; set; }
        public int CancelledSession { get; set; }
    }

    public class HmdWorklistFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public List<HmdOptionItemResponse> ShiftOptions { get; set; } = new();
        public List<HmdOptionItemResponse> SessionStatusOptions { get; set; } = new();
        public List<HmdOptionItemResponse> StaffRoleOptions { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    // =====================================================================
    // Sesi
    // =====================================================================

    public class HmdSessionResponse
    {
        public Guid Id { get; set; }
        public string SessionNumber { get; set; } = string.Empty;
        public Guid EpisodeId { get; set; }
        public string? EpisodeNumber { get; set; }
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? MedicalRecordNumber { get; set; }
        public Guid PrescriptionId { get; set; }
        public Guid? EncounterId { get; set; }
        public string? EncounterNumber { get; set; }
        public Guid? InpEpisodeId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid MachineId { get; set; }
        public string? MachineCode { get; set; }
        public Guid StationId { get; set; }
        public string? StationCode { get; set; }
        public Guid? ResponsibleDoctorId { get; set; }
        public string? ResponsibleDoctorName { get; set; }
        public DateOnly ScheduledDate { get; set; }
        public HmdShift Shift { get; set; }
        public string ShiftName { get; set; } = string.Empty;
        public DateTime ScheduledStartAt { get; set; }
        public DateTime ScheduledEndAt { get; set; }
        public DateTime? CheckedInAt { get; set; }
        public DateTime? ReadyAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public Guid? StartedByUserId { get; set; }
        public DateTime? EndedAt { get; set; }
        public int? ActualDurationMinutes { get; set; }
        public int? ActualUltrafiltrationMl { get; set; }
        public HmdSessionStatus SessionStatus { get; set; }
        public string SessionStatusName { get; set; } = string.Empty;
        public string? HoldReason { get; set; }
        public HmdSessionStopReason? StopReason { get; set; }
        public string? StopReasonName { get; set; }
        public HmdDisposition? Disposition { get; set; }
        public string? DispositionName { get; set; }
        public Guid? DocumentedByUserId { get; set; }
        public DateTime? DocumentedAt { get; set; }
        public Guid? SignedByUserId { get; set; }
        public DateTime? SignedAt { get; set; }
        public string? ReturnReason { get; set; }
        public string? CancelReason { get; set; }
        public Guid? PatientProcedureId { get; set; }
        public HmdBillingHandoffStatus BillingHandoffStatus { get; set; }
        public string BillingHandoffStatusName { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public List<string> Warnings { get; set; } = new();
    }

    public class HmdPrescriptionSnapshotResponse
    {
        public Guid Id { get; set; }
        public string? PrescribingDoctorName { get; set; }
        public int FrequencyPerWeek { get; set; }
        public int TargetDurationMinutes { get; set; }
        public int? TargetUltrafiltrationMl { get; set; }
        public int? BloodFlowRate { get; set; }
        public int? DialysateFlowRate { get; set; }
        public string? DialyzerType { get; set; }
        public string? DialysateComposition { get; set; }
        public string? SodiumBicarbonateProfile { get; set; }
        public decimal? DialysateTemperatureC { get; set; }
        public string? AnticoagulantPlan { get; set; }
        public string? VascularAccessSite { get; set; }
        public HmdPrescriptionStatus PrescriptionStatus { get; set; }
    }

    /// <summary>Konteks lengkap satu sesi. Pembacaannya dicatat logger (<c>permission-audit-matrix.md</c> bagian 5).</summary>
    public class HmdSessionDetailResponse : HmdSessionResponse
    {
        public HmdPatientHeaderResponse Patient { get; set; } = new();
        public HmdPrescriptionSnapshotResponse? Prescription { get; set; }
        public HmdIsolationRequirement IsolationRequirement { get; set; }
        public string IsolationRequirementName { get; set; } = string.Empty;
        public string? StopNote { get; set; }
        public string? DeviationNote { get; set; }
        public string? DispositionNote { get; set; }
        public HmdSessionAssessmentResponse? PreAssessment { get; set; }
        public HmdSessionAssessmentResponse? PostAssessment { get; set; }
        public int ChecklistMandatoryCount { get; set; }
        public int ChecklistSatisfiedCount { get; set; }
        public int ObservationCount { get; set; }
        public int MedicationCount { get; set; }
        public int ComplicationCount { get; set; }
        public List<HmdStaffAssignmentResponse> StaffAssignments { get; set; } = new();
        public List<string> AvailableActions { get; set; } = new();
    }

    public class CheckInHmdSessionRequest
    {
        /// <summary>Kunjungan pasien hari ini; wajib bila sesi belum punya kunjungan.</summary>
        public Guid? EncounterId { get; set; }
    }

    public class HoldHmdSessionRequest
    {
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    public class StartHmdSessionRequest
    {
        /// <summary>Kunci idempotency tombol Mulai; permintaan ulang dengan kunci sama mengembalikan sesi yang sama.</summary>
        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    public class StopHmdSessionRequest
    {
        public HmdSessionStopReason? StopReason { get; set; }

        [MaxLength(1000)]
        public string? StopNote { get; set; }

        [Range(0, 20000)]
        public int? ActualUltrafiltrationMl { get; set; }
    }

    public class CompleteHmdSessionRequest
    {
        [Range(0, 20000)]
        public int? ActualUltrafiltrationMl { get; set; }

        /// <summary>Penyimpangan dari resep beserta alasannya, bila ada.</summary>
        [MaxLength(1000)]
        public string? DeviationNote { get; set; }
    }

    public class ReturnHmdSessionRequest
    {
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    public class FinalizeHmdSessionRequest
    {
        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    // ------------------------------------------------------------ checklist Pra-HD

    public class SaveHmdChecklistItemInput
    {
        [Required]
        public Guid ChecklistItemId { get; set; }

        [Required]
        public HmdChecklistResult Result { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }

    public class SaveHmdChecklistRequest
    {
        [Required, MinLength(1)]
        public List<SaveHmdChecklistItemInput> Items { get; set; } = new();
    }

    public class OverrideHmdChecklistRequest
    {
        [MaxLength(1000)]
        public string? Reason { get; set; }
    }

    public class HmdSessionChecklistResponse
    {
        public Guid? Id { get; set; }
        public Guid ChecklistItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public HmdChecklistCategory Category { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
        public bool IsOverridable { get; set; }
        public int CheckSequence { get; set; }
        public HmdChecklistResult Result { get; set; }
        public string ResultName { get; set; } = string.Empty;
        public Guid? VerifiedByUserId { get; set; }
        public string? VerifiedByName { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? Note { get; set; }
        public bool IsOverridden { get; set; }
        public string? OverrideReason { get; set; }
        public Guid? OverriddenByUserId { get; set; }
        public DateTime? OverriddenAt { get; set; }
        public bool IsSatisfied { get; set; }
    }

    // ------------------------------------------------------------ penilaian Pra/Pasca-HD

    public class HmdVitalSignInput
    {
        [Range(0, 300)]
        public int? SystolicBp { get; set; }

        [Range(0, 200)]
        public int? DiastolicBp { get; set; }

        [Range(0, 300)]
        public int? PulseRate { get; set; }

        [Range(0, 100)]
        public int? RespiratoryRate { get; set; }

        [Range(typeof(decimal), "25", "45")]
        public decimal? TemperatureC { get; set; }

        [Range(0, 100)]
        public int? OxygenSaturation { get; set; }
    }

    public class SaveHmdPreAssessmentRequest : HmdVitalSignInput
    {
        [Range(typeof(decimal), "1", "400")]
        public decimal? BodyWeightKg { get; set; }

        [MaxLength(1000)]
        public string? Complaint { get; set; }

        [MaxLength(500)]
        public string? AccessConditionNote { get; set; }

        [MaxLength(1000)]
        public string? PatientCondition { get; set; }
    }

    public class SaveHmdPostAssessmentRequest : SaveHmdPreAssessmentRequest
    {
        public bool? TargetAchieved { get; set; }

        [Range(0, 20000)]
        public int? ActualUltrafiltrationMl { get; set; }

        public HmdDisposition? Disposition { get; set; }

        [MaxLength(1000)]
        public string? DispositionNote { get; set; }
    }

    public class HmdSessionAssessmentResponse
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public HmdAssessmentPhase Phase { get; set; }
        public DateTime AssessedAt { get; set; }
        public Guid AssessedByUserId { get; set; }
        public decimal? BodyWeightKg { get; set; }
        public Guid? PatientVitalSignId { get; set; }
        public int? SystolicBp { get; set; }
        public int? DiastolicBp { get; set; }
        public int? PulseRate { get; set; }
        public int? RespiratoryRate { get; set; }
        public decimal? TemperatureC { get; set; }
        public decimal? OxygenSaturation { get; set; }
        public string? Complaint { get; set; }
        public string? AccessConditionNote { get; set; }
        public string? PatientCondition { get; set; }
        public bool? TargetAchieved { get; set; }
    }

    // ------------------------------------------------------------ pemantauan

    public class HmdObservationQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 100;
    }

    public class CreateHmdObservationRequest : HmdVitalSignInput
    {
        /// <summary>Waktu pengamatan. Kosong berarti waktu server saat ini.</summary>
        public DateTime? ObservedAt { get; set; }

        [Range(0, 1000)]
        public int? BloodFlowRate { get; set; }

        [Range(0, 2000)]
        public int? DialysateFlowRate { get; set; }

        [Range(-1000, 1000)]
        public int? TransmembranePressure { get; set; }

        [Range(-1000, 1000)]
        public int? VenousPressure { get; set; }

        [Range(-1000, 1000)]
        public int? ArterialPressure { get; set; }

        [Range(0, 20000)]
        public int? UltrafiltrationVolumeMl { get; set; }

        /// <summary>Catatan pengamatan termasuk keluhan subjektif pasien.</summary>
        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    public class HmdObservationResponse
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public int SequenceNumber { get; set; }
        public DateTime ObservedAt { get; set; }
        public Guid RecordedByUserId { get; set; }
        public DateTime RecordedAt { get; set; }
        public int? SystolicBp { get; set; }
        public int? DiastolicBp { get; set; }
        public int? PulseRate { get; set; }
        public int? RespiratoryRate { get; set; }
        public decimal? TemperatureC { get; set; }
        public int? OxygenSaturation { get; set; }
        public int? BloodFlowRate { get; set; }
        public int? DialysateFlowRate { get; set; }
        public int? TransmembranePressure { get; set; }
        public int? VenousPressure { get; set; }
        public int? ArterialPressure { get; set; }
        public int? UltrafiltrationVolumeMl { get; set; }
        public string? Note { get; set; }
    }

    // ------------------------------------------------------------ obat

    public class CreateHmdMedicationRequest
    {
        public Guid? DrugId { get; set; }

        public decimal? Dose { get; set; }

        [MaxLength(50)]
        public string? DoseUnit { get; set; }

        public HmdMedicationRoute? Route { get; set; }

        /// <summary>Waktu pemberian. Kosong berarti waktu server saat ini.</summary>
        public DateTime? AdministeredAt { get; set; }

        public Guid? InstructedByDoctorId { get; set; }

        /// <summary>Lokasi stok Farmasi sumber obat — dibutuhkan untuk meneruskan pemakaian ke Farmasi.</summary>
        public Guid? PharmacyStorageLocationId { get; set; }

        /// <summary>Satuan stok Farmasi. Kosong berarti satuan stok bawaan obat.</summary>
        public Guid? PharmacyMeasurementId { get; set; }

        /// <summary>Jumlah dalam satuan stok Farmasi, misalnya 1 vial.</summary>
        [Range(typeof(decimal), "0.001", "1000000")]
        public decimal? PharmacyQuantity { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }

    public class RetryHmdPharmacyHandoffRequest
    {
        public Guid? PharmacyStorageLocationId { get; set; }

        public Guid? PharmacyMeasurementId { get; set; }

        [Range(typeof(decimal), "0.001", "1000000")]
        public decimal? PharmacyQuantity { get; set; }
    }

    public class HmdMedicationResponse
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public Guid DrugId { get; set; }
        public string? DrugName { get; set; }
        public decimal Dose { get; set; }
        public string DoseUnit { get; set; } = string.Empty;
        public HmdMedicationRoute Route { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public Guid? InstructedByDoctorId { get; set; }
        public string? InstructedByDoctorName { get; set; }
        public Guid AdministeredByUserId { get; set; }
        public DateTime AdministeredAt { get; set; }
        public Guid? DrugUsageId { get; set; }

        /// <summary>Status penerusan ke Farmasi (<c>HandoffStatus</c>).</summary>
        public HmdPharmacyHandoffStatus PharmacySyncStatus { get; set; }
        public string PharmacySyncStatusName { get; set; } = string.Empty;
        public DateTime? HandoffAttemptedAt { get; set; }
        public string? HandoffError { get; set; }
        public string? Note { get; set; }
    }

    // ------------------------------------------------------------ komplikasi

    public class CreateHmdComplicationRequest
    {
        public HmdComplicationType? ComplicationType { get; set; }

        /// <summary>Waktu kejadian. Kosong berarti waktu server saat ini.</summary>
        public DateTime? DetectedAt { get; set; }

        [MaxLength(50)]
        public string? Severity { get; set; }

        [MaxLength(1000)]
        public string? SignsAndSymptoms { get; set; }

        [MaxLength(1000)]
        public string? Intervention { get; set; }

        [MaxLength(1000)]
        public string? ClinicianInstruction { get; set; }

        [Required]
        public HmdComplicationOutcome Outcome { get; set; }

        [Required]
        public HmdComplicationSessionImpact SessionImpact { get; set; }

        [MaxLength(200)]
        public string? TransferDestination { get; set; }
    }

    public class HmdComplicationResponse
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public HmdComplicationType ComplicationType { get; set; }
        public string ComplicationTypeName { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; }
        public Guid DetectedByUserId { get; set; }
        public string? Severity { get; set; }
        public string SignsAndSymptoms { get; set; } = string.Empty;
        public string Intervention { get; set; } = string.Empty;
        public string? ClinicianInstruction { get; set; }
        public HmdComplicationOutcome Outcome { get; set; }
        public string OutcomeName { get; set; } = string.Empty;
        public HmdComplicationSessionImpact SessionImpact { get; set; }
        public string SessionImpactName { get; set; } = string.Empty;
        public string? TransferDestination { get; set; }
    }

    // ------------------------------------------------------------ Billing

    public class HmdBillingHandoffResponse
    {
        public Guid SessionId { get; set; }
        public HmdSessionStatus SessionStatus { get; set; }
        public Guid? PatientProcedureId { get; set; }
        public bool IsBillable { get; set; }
        public HmdBillingHandoffStatus BillingHandoffStatus { get; set; }
        public string BillingHandoffStatusName { get; set; } = string.Empty;
        public DateTime? BillingHandoffAt { get; set; }
        public string? BillingHandoffError { get; set; }
    }
}
