using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs
{
    /// <summary>
    /// Satu baris skala pada permintaan — BE-RWI-102/BE-RWI-103. Batas bawah inklusif, batas atas
    /// eksklusif; kosong berarti terbuka.
    /// </summary>
    public class SlidingScaleRangeRequest
    {
        public decimal? LowerBoundInclusive { get; set; }

        public decimal? UpperBoundExclusive { get; set; }

        public decimal DoseUnits { get; set; }

        [MaxLength(300)]
        public string? InstructionText { get; set; }

        public bool RequiresPhysicianNotification { get; set; }
    }

    public class SlidingScaleRangeResponse
    {
        public Guid Id { get; set; }
        public decimal? LowerBoundInclusive { get; set; }
        public decimal? UpperBoundExclusive { get; set; }
        public decimal DoseUnits { get; set; }
        public string? InstructionText { get; set; }
        public bool RequiresPhysicianNotification { get; set; }
        public int SortOrder { get; set; }
    }

    // =====================================================================
    // Template — BE-RWI-102, api-contract 0.6.0 bagian 12.10
    // =====================================================================

    public class CreateSlidingScaleTemplateRequest
    {
        [MaxLength(50)]
        public string? TemplateCode { get; set; }

        [MaxLength(200)]
        public string? TemplateName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class SaveSlidingScaleVersionRequest
    {
        public BloodGlucoseUnit? GlucoseUnit { get; set; }

        public List<SlidingScaleRangeRequest> Ranges { get; set; } = new();
    }

    public class ApproveSlidingScaleVersionRequest
    {
        [MaxLength(500)]
        public string? ApprovalNote { get; set; }
    }

    public class SlidingScaleVersionResponse
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public int VersionNumber { get; set; }
        public SlidingScaleVersionStatus VersionStatus { get; set; }
        public BloodGlucoseUnit GlucoseUnit { get; set; }
        public string? DefinitionHash { get; set; }
        public Guid LastModifiedByUserId { get; set; }
        public string? LastModifiedByName { get; set; }
        public DateTime LastModifiedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RetiredAt { get; set; }
        public string? ApprovalNote { get; set; }
        public List<SlidingScaleRangeResponse> Ranges { get; set; } = new();

        /// <summary>Bantuan tampilan; setiap aksi tetap diperiksa ulang di backend.</summary>
        public List<string> AvailableActions { get; set; } = new();
    }

    public class SlidingScaleTemplateListItem
    {
        public Guid Id { get; set; }
        public string TemplateCode { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        /// <summary><c>false</c> berarti keadaan layar "Belum ada protokol sliding scale yang disahkan" (VAL-DOK-55e).</summary>
        public bool HasApprovedVersion { get; set; }

        public Guid? ApprovedVersionId { get; set; }
        public int? ApprovedVersionNumber { get; set; }
        public BloodGlucoseUnit? ApprovedGlucoseUnit { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int LatestVersionNumber { get; set; }
    }

    public class SlidingScaleTemplateResponse
    {
        public Guid Id { get; set; }
        public string TemplateCode { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public List<SlidingScaleVersionResponse> Versions { get; set; } = new();
    }

    // =====================================================================
    // Order per pasien — BE-RWI-103, api-contract 0.6.0 bagian 12.11
    // =====================================================================

    public class CreateSlidingScaleOrderRequest
    {
        public Guid PrescriptionItemId { get; set; }

        public Guid TemplateVersionId { get; set; }

        /// <summary>Kosong berarti rentang versi template disalin apa adanya.</summary>
        public List<SlidingScaleRangeRequest>? Ranges { get; set; }

        [MaxLength(500)]
        public string? AdjustmentReason { get; set; }

        [MaxLength(30)]
        public string? CheckFrequencyCode { get; set; }
    }

    public class AdjustSlidingScaleOrderRequest
    {
        /// <summary>Nomor versi order yang dibaca dokter sebelum menyesuaikan — penjaga kiriman basi.</summary>
        public int ExpectedVersionNumber { get; set; }

        public List<SlidingScaleRangeRequest> Ranges { get; set; } = new();

        [MaxLength(500)]
        public string? AdjustmentReason { get; set; }
    }

    public class StopSlidingScaleOrderRequest
    {
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    public class SlidingScaleOrderVersionResponse
    {
        public Guid Id { get; set; }
        public int VersionNumber { get; set; }
        public Guid TemplateVersionId { get; set; }
        public int TemplateVersionNumber { get; set; }
        public BloodGlucoseUnit GlucoseUnit { get; set; }
        public bool IsAdjusted { get; set; }
        public string? AdjustmentReason { get; set; }
        public Guid OrderedByDoctorId { get; set; }
        public string? OrderedByDoctorName { get; set; }
        public Guid OrderedByUserId { get; set; }
        public DateTime OrderedAt { get; set; }
        public List<SlidingScaleRangeResponse> Ranges { get; set; } = new();
    }

    public class SlidingScaleOrderListItem
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid PrescriptionId { get; set; }
        public Guid PrescriptionItemId { get; set; }
        public string? DrugNameSnapshot { get; set; }
        public Guid InpEpisodeId { get; set; }
        public Guid PatientId { get; set; }
        public Guid TemplateId { get; set; }
        public string? TemplateName { get; set; }
        public SlidingScaleOrderStatus OrderStatus { get; set; }
        public int CurrentVersionNumber { get; set; }
        public bool IsCurrentVersionAdjusted { get; set; }
        public string? CheckFrequencyCode { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? StoppedAt { get; set; }
    }

    public class SlidingScaleOrderResponse : SlidingScaleOrderListItem
    {
        public Guid EncounterId { get; set; }
        public Guid? StoppedByUserId { get; set; }
        public string? StopReason { get; set; }
        public List<SlidingScaleOrderVersionResponse> Versions { get; set; } = new();
        public List<string> AvailableActions { get; set; } = new();
    }
}
