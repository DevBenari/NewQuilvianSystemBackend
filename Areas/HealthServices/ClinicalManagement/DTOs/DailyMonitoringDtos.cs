using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    // =========================================================================
    // Pengawasan Harian — BE-RWI-119, BE-RWI-120, BE-RWI-122; api-contract keperawatan 0.5.0 bagian 7.5–7.9.
    // =========================================================================

    /// <summary>Pembatalan entri terukur (cairan, GDS, observasi) — alasan wajib.</summary>
    public class CancelClinicalMeasurementRequest
    {
        public string? Reason { get; set; }

        public int? ExpectedRevisionNumber { get; set; }
    }

    // ---------------------------------------------------------------- Cairan

    public class FluidBalanceEntryResponse
    {
        public Guid Id { get; set; }

        public Guid EncounterId { get; set; }

        public Guid PatientId { get; set; }

        public Guid InpEpisodeId { get; set; }

        public FluidDirection Direction { get; set; }

        public string DirectionLabel { get; set; } = string.Empty;

        public FluidSourceCategory SourceCategory { get; set; }

        public string SourceCategoryLabel { get; set; } = string.Empty;

        public string? SourceDetail { get; set; }

        public decimal VolumeMl { get; set; }

        public DateTime EntryDateTime { get; set; }

        public Guid RecordedByEmployeeId { get; set; }

        public string? RecordedByName { get; set; }

        public Guid RecordedByUserId { get; set; }

        public Guid? MedicationAdministrationId { get; set; }

        public string? MedicationAdministrationNumber { get; set; }

        public string? MedicationDrugName { get; set; }

        public ClinicalMeasurementStatus EntryStatus { get; set; }

        public string EntryStatusLabel { get; set; } = string.Empty;

        public int RevisionNumber { get; set; }

        public string? CancelReason { get; set; }

        /// <summary>Dosis tertaut dikoreksi menjadi selain diberikan — usulan <c>G-26</c>.</summary>
        public DateTime? DoseCorrectionFlaggedAt { get; set; }

        /// <summary>"Perlu ditinjau" — <c>VAL-KEP-36f</c>. Nilai entri tidak diubah otomatis.</summary>
        public bool NeedsReview { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        public bool IsReplay { get; set; }
    }

    public class CreateFluidBalanceEntryRequest
    {
        public Guid EpisodeId { get; set; }

        public FluidDirection Direction { get; set; }

        public FluidSourceCategory SourceCategory { get; set; }

        /// <summary>"RL 500 ml", "Urin kateter".</summary>
        public string? SourceDetail { get; set; }

        /// <summary>Volume aktual diketik perawat, termasuk pelarut — <c>RWI-DEC-149</c>.</summary>
        public decimal VolumeMl { get; set; }

        public DateTime? EntryDateTime { get; set; }

        /// <summary>Wajib untuk sumber Obat; dilarang untuk sumber lain — <c>VAL-KEP-24d</c>.</summary>
        public Guid? MedicationAdministrationId { get; set; }

        public string? IdempotencyKey { get; set; }
    }

    public class CorrectFluidBalanceEntryRequest
    {
        public decimal VolumeMl { get; set; }

        public DateTime? EntryDateTime { get; set; }

        public FluidSourceCategory SourceCategory { get; set; }

        public string? SourceDetail { get; set; }

        public string? CorrectionReason { get; set; }

        public int? ExpectedRevisionNumber { get; set; }
    }

    public class FluidBalanceEntryRevisionResponse
    {
        public Guid Id { get; set; }

        public int RevisionNumber { get; set; }

        public decimal PreviousVolumeMl { get; set; }

        public DateTime PreviousEntryDateTime { get; set; }

        public FluidSourceCategory PreviousSourceCategory { get; set; }

        public string PreviousSourceCategoryLabel { get; set; } = string.Empty;

        public string? PreviousSourceDetail { get; set; }

        public string CorrectionReason { get; set; } = string.Empty;

        public Guid CorrectedByUserId { get; set; }

        public string? CorrectedByName { get; set; }

        public DateTime CorrectedAt { get; set; }
    }

    public class FluidTotalsResponse
    {
        public Guid EpisodeId { get; set; }

        public DateOnly Date { get; set; }

        public DateTime WindowStartUtc { get; set; }

        public DateTime WindowEndUtc { get; set; }

        /// <summary>Kosong bila unit maupun bawaan tidak punya konfigurasi shift.</summary>
        public List<FluidTotalLine> Shifts { get; set; } = new();

        public FluidTotalLine Day { get; set; } = new();

        /// <summary><c>true</c> = hanya balance 24 jam; tidak ada shift buatan — <c>BE-RWI-120</c> kriteria 3.</summary>
        public bool ShiftConfigurationMissing { get; set; }
    }

    public class FluidTotalLine
    {
        /// <summary><c>null</c> untuk baris 24 jam.</summary>
        public string? ShiftCode { get; set; }

        public string? ShiftName { get; set; }

        public DateTime StartUtc { get; set; }

        public DateTime EndUtc { get; set; }

        public decimal IntakeMl { get; set; }

        public decimal OutputMl { get; set; }

        public decimal BalanceMl { get; set; }
    }

    // ---------------------------------------------------------------- GDS bangsal

    public class BloodGlucoseReadingResponse
    {
        public Guid Id { get; set; }

        public Guid EncounterId { get; set; }

        public Guid PatientId { get; set; }

        public Guid InpEpisodeId { get; set; }

        public DateTime MeasuredAt { get; set; }

        public decimal GlucoseValue { get; set; }

        public BloodGlucoseUnit GlucoseUnit { get; set; }

        public string GlucoseUnitLabel { get; set; } = string.Empty;

        public BloodGlucoseMethod Method { get; set; }

        public Guid RecordedByEmployeeId { get; set; }

        public string? RecordedByName { get; set; }

        public Guid RecordedByUserId { get; set; }

        public ClinicalMeasurementStatus ReadingStatus { get; set; }

        public string ReadingStatusLabel { get; set; } = string.Empty;

        public int RevisionNumber { get; set; }

        public string? CancelReason { get; set; }

        /// <summary>Pelaksanaan sliding scale tercatat yang memakai GDS ini.</summary>
        public Guid? UsedBySlidingScaleExecutionId { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        public bool IsReplay { get; set; }
    }

    public class CreateBloodGlucoseReadingRequest
    {
        public Guid EpisodeId { get; set; }

        public DateTime? MeasuredAt { get; set; }

        public decimal? GlucoseValue { get; set; }

        /// <summary>Wajib dipilih, tanpa bawaan — gate <c>G-25</c>, <c>VAL-KEP-25a</c>.</summary>
        public BloodGlucoseUnit? GlucoseUnit { get; set; }

        public string? IdempotencyKey { get; set; }
    }

    public class CorrectBloodGlucoseReadingRequest
    {
        public decimal? GlucoseValue { get; set; }

        public BloodGlucoseUnit? GlucoseUnit { get; set; }

        public DateTime? MeasuredAt { get; set; }

        public string? CorrectionReason { get; set; }

        public int? ExpectedRevisionNumber { get; set; }
    }

    public class BloodGlucoseReadingRevisionResponse
    {
        public Guid Id { get; set; }

        public int RevisionNumber { get; set; }

        public decimal PreviousValue { get; set; }

        public BloodGlucoseUnit PreviousUnit { get; set; }

        public string PreviousUnitLabel { get; set; } = string.Empty;

        public DateTime PreviousMeasuredAt { get; set; }

        public string CorrectionReason { get; set; } = string.Empty;

        public Guid CorrectedByUserId { get; set; }

        public string? CorrectedByName { get; set; }

        public DateTime CorrectedAt { get; set; }
    }

    // ---------------------------------------------------------------- Observasi harian

    public class DailyObservationResponse
    {
        public Guid Id { get; set; }

        public Guid EncounterId { get; set; }

        public Guid PatientId { get; set; }

        public Guid InpEpisodeId { get; set; }

        public DateTime ObservedAt { get; set; }

        public int? DietIntakePercent { get; set; }

        public string? DietNote { get; set; }

        public MobilizationLevel MobilizationLevel { get; set; }

        public string MobilizationLevelLabel { get; set; } = string.Empty;

        public decimal? AbdominalCircumferenceCm { get; set; }

        /// <summary><c>null</c> = belum dinilai, bukan "tidak agitasi".</summary>
        public bool? IsAgitated { get; set; }

        public string? Note { get; set; }

        public Guid RecordedByEmployeeId { get; set; }

        public string? RecordedByName { get; set; }

        public Guid RecordedByUserId { get; set; }

        public ClinicalMeasurementStatus ObservationStatus { get; set; }

        public string ObservationStatusLabel { get; set; } = string.Empty;

        public int RevisionNumber { get; set; }

        public string? CancelReason { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        public bool IsReplay { get; set; }
    }

    public class CreateDailyObservationRequest
    {
        public Guid EpisodeId { get; set; }

        public DateTime? ObservedAt { get; set; }

        public int? DietIntakePercent { get; set; }

        public string? DietNote { get; set; }

        public MobilizationLevel MobilizationLevel { get; set; } = MobilizationLevel.NotAssessed;

        public decimal? AbdominalCircumferenceCm { get; set; }

        public bool? IsAgitated { get; set; }

        public string? Note { get; set; }

        public string? IdempotencyKey { get; set; }
    }

    public class CorrectDailyObservationRequest
    {
        public DateTime? ObservedAt { get; set; }

        public int? DietIntakePercent { get; set; }

        public string? DietNote { get; set; }

        public MobilizationLevel MobilizationLevel { get; set; } = MobilizationLevel.NotAssessed;

        public decimal? AbdominalCircumferenceCm { get; set; }

        public bool? IsAgitated { get; set; }

        public string? Note { get; set; }

        public string? CorrectionReason { get; set; }

        public int? ExpectedRevisionNumber { get; set; }
    }

    // ---------------------------------------------------------------- Shift

    public class NursingShiftSetResponse
    {
        /// <summary><c>null</c> = shift bawaan rumah sakit.</summary>
        public Guid? ServiceUnitId { get; set; }

        public string? ServiceUnitName { get; set; }

        /// <summary><c>true</c> bila set yang dikembalikan adalah bawaan — unit tidak punya shift sendiri.</summary>
        public bool IsDefault { get; set; }

        /// <summary><c>false</c> bila unit maupun bawaan belum dikonfigurasi.</summary>
        public bool IsConfigured { get; set; }

        public List<NursingShiftResponse> Shifts { get; set; } = new();
    }

    public class NursingShiftResponse
    {
        public Guid Id { get; set; }

        public string ShiftCode { get; set; } = string.Empty;

        public string ShiftName { get; set; } = string.Empty;

        /// <summary>"07:00".</summary>
        public string StartTime { get; set; } = string.Empty;

        /// <summary>"14:00"; lebih kecil dari mulai berarti melewati tengah malam.</summary>
        public string EndTime { get; set; } = string.Empty;
    }

    public class ReplaceNursingShiftSetRequest
    {
        /// <summary><c>null</c> = mengganti shift bawaan.</summary>
        public Guid? ServiceUnitId { get; set; }

        /// <summary>Daftar kosong pada unit menghapus shift unit itu sehingga unit memakai bawaan.</summary>
        public List<NursingShiftRequest> Shifts { get; set; } = new();
    }

    public class NursingShiftRequest
    {
        public string? ShiftCode { get; set; }

        public string? ShiftName { get; set; }

        public string? StartTime { get; set; }

        public string? EndTime { get; set; }

        /// <summary>Diterima demi kontrak, tidak disimpan: urutan shift dibaca dari jam mulainya.</summary>
        public int? SortOrder { get; set; }
    }

    // ---------------------------------------------------------------- Ringkasan

    public class DailyMonitoringSummaryResponse
    {
        public Guid EpisodeId { get; set; }

        public DateOnly Date { get; set; }

        public DateTime WindowStartUtc { get; set; }

        public DateTime WindowEndUtc { get; set; }

        /// <summary>"07:00" bila hari mengikuti shift pertama unit; "00:00" bila tanpa shift.</summary>
        public string DayStartsAt { get; set; } = "00:00";

        public List<PatientVitalSignSeriesItem> VitalSigns { get; set; } = new();

        /// <summary>Nyeri terakhir dari Monitoring Nyeri — bukan dari tanda vital.</summary>
        public DailyMonitoringLatestPain? LatestPain { get; set; }

        public List<BloodGlucoseReadingResponse> GlucoseReadings { get; set; } = new();

        public FluidTotalsResponse FluidTotals { get; set; } = new();

        public List<FluidBalanceEntryResponse> FluidEntries { get; set; } = new();

        public List<DailyObservationResponse> Observations { get; set; } = new();

        /// <summary>Pengingat, bukan kewajiban — <c>FR-KEP-063</c>, usulan <c>G-27</c>.</summary>
        public List<AdministeredDoseWithoutIntakeItem> AdministeredDosesWithoutIntake { get; set; } = new();
    }

    public class DailyMonitoringLatestPain
    {
        public Guid AssessmentId { get; set; }

        public int? PainScale { get; set; }

        public PainAssessmentState PainAssessmentState { get; set; }

        public DateTime ClinicalDateTime { get; set; }

        public DateTime? PainReassessmentDueAt { get; set; }
    }

    public class AdministeredDoseWithoutIntakeItem
    {
        public Guid AdministrationId { get; set; }

        public string AdministrationNumber { get; set; } = string.Empty;

        public string DrugName { get; set; } = string.Empty;

        public decimal? ActualDose { get; set; }

        public string? ActualDoseUnit { get; set; }

        public string? ActualRoute { get; set; }

        public DateTime? AdministeredAt { get; set; }
    }
}
