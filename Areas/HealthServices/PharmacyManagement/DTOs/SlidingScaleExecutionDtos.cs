using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs
{
    // =========================================================================
    // Pelaksanaan sliding scale — BE-RWI-123, api-contract keperawatan 0.5.0 bagian 7.12.
    // =========================================================================

    public class PreviewSlidingScaleExecutionRequest
    {
        public Guid OrderId { get; set; }

        public decimal? GlucoseValue { get; set; }

        public BloodGlucoseUnit? GlucoseUnit { get; set; }

        /// <summary>Isi ini <b>atau</b> <c>GlucoseValue</c> + <c>GlucoseUnit</c>.</summary>
        public Guid? BloodGlucoseReadingId { get; set; }
    }

    public class SlidingScalePreviewResponse
    {
        public Guid OrderId { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public Guid OrderVersionId { get; set; }

        public int OrderVersionNumber { get; set; }

        public bool IsOrderAdjusted { get; set; }

        public BloodGlucoseUnit ProtocolGlucoseUnit { get; set; }

        public string ProtocolGlucoseUnitLabel { get; set; } = string.Empty;

        public decimal GlucoseValue { get; set; }

        public BloodGlucoseUnit GlucoseUnit { get; set; }

        public Guid? BloodGlucoseReadingId { get; set; }

        public SlidingScaleRangeResponse MatchedRange { get; set; } = new();

        /// <summary>"250–300".</summary>
        public string MatchedRangeLabel { get; set; } = string.Empty;

        public decimal ComputedDoseUnits { get; set; }

        /// <summary><c>true</c> = rentang 0 unit; penyimpanan mencatat dosis <c>Held</c> beralasan — usulan <c>G-22</c>.</summary>
        public bool IsZeroDose { get; set; }

        public string? InstructionText { get; set; }

        /// <summary>Ditampilkan kepada perawat; sistem tidak mengirim notifikasi ke dokter — gate <c>G-24</c>.</summary>
        public bool RequiresPhysicianNotification { get; set; }

        public bool IsHighAlert { get; set; }

        public Guid PrescriptionItemId { get; set; }

        public string DrugName { get; set; } = string.Empty;

        public string? Route { get; set; }
    }

    public class SlidingScaleNewReadingRequest
    {
        public DateTime? MeasuredAt { get; set; }

        public decimal? GlucoseValue { get; set; }

        public BloodGlucoseUnit? GlucoseUnit { get; set; }
    }

    public class CreateSlidingScaleExecutionRequest
    {
        public Guid OrderId { get; set; }

        /// <summary>Versi order yang dilihat perawat saat pratinjau — <c>VAL-KEP-29c</c>.</summary>
        public int? ExpectedOrderVersionNumber { get; set; }

        /// <summary>GDS baru yang diketik perawat, <b>atau</b> <see cref="BloodGlucoseReadingId"/>.</summary>
        public SlidingScaleNewReadingRequest? NewReading { get; set; }

        public Guid? BloodGlucoseReadingId { get; set; }

        /// <summary>Kosong = sama dengan dosis hitung.</summary>
        public decimal? ActualDoseUnits { get; set; }

        public DateTime? AdministeredAt { get; set; }

        public string? ActualRoute { get; set; }

        /// <summary>Wajib bila dosis aktual berbeda dari dosis hitung — <c>VAL-KEP-29d</c>.</summary>
        public string? ExceptionReason { get; set; }

        /// <summary>Slot dosis <c>Due</c> butir yang sama yang hendak dipakai; kosong = baris dosis baru.</summary>
        public Guid? DoseSlotAdministrationId { get; set; }

        public string? IdempotencyKey { get; set; }
    }

    public class SlidingScaleExecutionListItem
    {
        public Guid Id { get; set; }

        public string ExecutionNumber { get; set; } = string.Empty;

        public Guid OrderId { get; set; }

        public string? OrderNumber { get; set; }

        public int? OrderVersionNumber { get; set; }

        public Guid InpEpisodeId { get; set; }

        public DateTime ExecutedAt { get; set; }

        public decimal GlucoseValueSnapshot { get; set; }

        public BloodGlucoseUnit GlucoseUnitSnapshot { get; set; }

        public string GlucoseUnitLabel { get; set; } = string.Empty;

        public string? MatchedRangeLabel { get; set; }

        public decimal ComputedDoseUnits { get; set; }

        public decimal? ActualDoseUnits { get; set; }

        public bool IsException { get; set; }

        public Guid ExecutedByEmployeeId { get; set; }

        public string? ExecutedByName { get; set; }

        public SlidingScaleExecutionStatus ExecutionStatus { get; set; }

        public string ExecutionStatusLabel { get; set; } = string.Empty;

        /// <summary>GDS rujukan dikoreksi setelah pelaksanaan — <c>RWI-DEC-148</c> (b).</summary>
        public bool ReadingCorrectedAfterExecution { get; set; }

        public Guid MedicationAdministrationId { get; set; }

        public MedicationDoseStatus DoseStatus { get; set; }

        public MedicationDoubleCheckStatus DoubleCheckStatus { get; set; }

        public string DoseStatusLabel { get; set; } = string.Empty;
    }

    public class SlidingScaleExecutionResponse : SlidingScaleExecutionListItem
    {
        public Guid OrderVersionId { get; set; }

        public Guid BloodGlucoseReadingId { get; set; }

        public Guid MatchedRangeId { get; set; }

        public string? InstructionText { get; set; }

        public bool RequiresPhysicianNotification { get; set; }

        public string? ExceptionReason { get; set; }

        public Guid ExecutedByUserId { get; set; }

        public MedicationAdministrationResponse? Administration { get; set; }

        public bool IsReplay { get; set; }
    }
}
