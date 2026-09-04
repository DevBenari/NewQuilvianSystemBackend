using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs
{
    /* ------------------------------------------------------------------ *
     * Permintaan
     * ------------------------------------------------------------------ */

    public class CreateRadOrderRequest
    {
        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid ProcedureId { get; set; }

        [Required]
        public Guid ModalityId { get; set; }

        public string? ClinicalIndication { get; set; }

        /// <summary>
        /// Perawatan rawat inap yang menaungi pesanan. Boleh kosong; bila terisi tetapi tidak
        /// cocok dengan perawatan milik kunjungannya, permintaan ditolak <c>400</c>.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-052</c>, <c>VAL-DOK-22</c>, <c>INV-DOK-12</c>. Inilah yang membuat pesanan
        /// perawatan A tidak dapat diproses sebagai milik perawatan B. Tanpa penanda ini, satu
        /// pasien yang dirawat dua kali dalam sebulan memiliki dua rangkaian pesanan yang
        /// bercampur pada layar dokter.
        /// </remarks>
        public Guid? InpEpisodeId { get; set; }
    }

    public class RadOrderTransitionRequest
    {
        public string? Reason { get; set; }

        public DateTime? ScheduledAt { get; set; }
    }

    public class CreateRadStudyRequest
    {
        /// <summary>
        /// Pemeriksaan study ini. Kosong berarti mengikuti pemeriksaan pesanannya.
        /// </summary>
        public Guid? ProcedureId { get; set; }
    }

    public class RadSafetyCheckDecisionRequest
    {
        [Required]
        public Guid SafetyRequirementId { get; set; }

        [Required]
        public RadSafetyCheckState CheckState { get; set; }

        public string? Note { get; set; }
    }

    public class RadAcquisitionQualityRequest
    {
        /// <summary>
        /// Apakah citra dapat dipakai untuk pembacaan klinis. Inilah yang menentukan kelayakan
        /// tagih normal; nilainya tidak boleh disimpulkan dari status apa pun.
        /// </summary>
        [Required]
        public bool IsUsable { get; set; }

        public string? QualityNote { get; set; }
    }

    public class RadAbortAcquisitionRequest
    {
        [Required]
        public RadAbortCause AbortCause { get; set; }

        [Required]
        public string AbortReason { get; set; } = string.Empty;

        public string? PerformedPortionNote { get; set; }
    }

    public class RadRepeatStudyRequest
    {
        [Required]
        public RadRepeatCause RepeatCause { get; set; }

        [Required]
        public string RepeatReason { get; set; } = string.Empty;

        /// <summary>
        /// Pesanan tambahan yang mengesahkan pengulangan. Wajib ketika sebabnya adalah
        /// kebutuhan klinis baru — <c>RJ-BIL-GATE-DEC-004</c> menuntut order yang sah untuk
        /// kasus itu, bukan sekadar alasan bebas.
        /// </summary>
        public Guid? AdditionalOrderId { get; set; }
    }

    public class RadConsumptionRequest
    {
        [Required]
        public RadConsumptionItemType ItemType { get; set; }

        [Required]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        public string ItemName { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        [Required]
        public string Unit { get; set; } = string.Empty;

        public bool ConsumedDespiteFailure { get; set; }

        public string? Note { get; set; }
    }

    /* ------------------------------------------------------------------ *
     * Balasan
     * ------------------------------------------------------------------ */

    public class RadOrderListResponse
    {
        public Guid Id { get; set; }

        public Guid EncounterId { get; set; }

        /// <summary>Perawatan rawat inap yang menaungi pesanan, bila ada.</summary>
        public Guid? InpEpisodeId { get; set; }

        /// <summary>
        /// Benar ketika hasil pemeriksaan sudah final dan sah dipakai sebagai dasar keputusan
        /// klinis.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-052</c>, <c>VAL-DOK-30</c>. Hasil yang belum final <b>wajib</b> ditandai
        /// dan tidak boleh disajikan sebagai hasil sah. Hasil basi di layar dokter adalah risiko
        /// keselamatan, bukan masalah tampilan.
        /// </remarks>
        public bool IsResultFinal { get; set; }

        /// <summary>Keterangan singkat ketersediaan hasil, siap ditampilkan apa adanya.</summary>
        public string ResultAvailabilityNote { get; set; } = string.Empty;

        public Guid ProcedureId { get; set; }

        public string ProcedureCode { get; set; } = string.Empty;

        public string ProcedureName { get; set; } = string.Empty;

        public Guid ModalityId { get; set; }

        public string ModalityCode { get; set; } = string.Empty;

        public string ModalityName { get; set; } = string.Empty;

        /// <summary>
        /// Status operasional pesanan. Bukan status pembayaran — Radiologi tidak memiliki status
        /// finansial apa pun.
        /// </summary>
        public string OrderStatus { get; set; } = string.Empty;

        public int StudyCount { get; set; }

        /// <summary>
        /// Jumlah study yang benar-benar dikerjakan dan menghasilkan citra yang dapat dipakai,
        /// yaitu jumlah study yang sudah memenuhi milestone kelayakan tagih.
        /// </summary>
        public int UsableStudyCount { get; set; }

        public bool IsCancel { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class RadOrderDetailResponse : RadOrderListResponse
    {
        public string? ClinicalIndication { get; set; }

        public DateTime? RequestedAt { get; set; }

        public DateTime? ScheduledAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string? StatusBeforeHold { get; set; }

        public string? ClosureReason { get; set; }

        public int Version { get; set; }

        public List<RadStudyResponse> Studies { get; set; } = new();
    }

    public class RadStudyResponse
    {
        public Guid Id { get; set; }

        public Guid RadOrderId { get; set; }

        public Guid EncounterId { get; set; }

        public string StudyNumber { get; set; } = string.Empty;

        public int StudySequence { get; set; }

        public Guid ProcedureId { get; set; }

        public string? ProcedureCode { get; set; }

        public string? ProcedureName { get; set; }

        public Guid ModalityId { get; set; }

        public string? ModalityCode { get; set; }

        public string StudyStatus { get; set; } = string.Empty;

        public DateTime? PatientVerifiedAt { get; set; }

        public DateTime? SafetyClearedAt { get; set; }

        public int? SafetyRuleVersionAtClearance { get; set; }

        public DateTime? AcquisitionStartedAt { get; set; }

        public DateTime? AcquiredAt { get; set; }

        /// <summary>Kosong berarti belum dinilai, bukan berarti tidak dapat dipakai.</summary>
        public bool? IsUsable { get; set; }

        public string? QualityNote { get; set; }

        public string? AbortCause { get; set; }

        public string? AbortReason { get; set; }

        public string? PerformedPortionNote { get; set; }

        public Guid? RepeatOfStudyId { get; set; }

        public string? RepeatCause { get; set; }

        public string? RepeatReason { get; set; }

        public Guid? AdditionalOrderId { get; set; }

        public bool BillingFactSubmitted { get; set; }

        public DateTime? BillingFactSubmittedAt { get; set; }

        public int Version { get; set; }

        public List<RadStudySafetyCheckResponse> SafetyChecks { get; set; } = new();

        public List<RadConsumptionResponse> Consumptions { get; set; } = new();
    }

    public class RadStudySafetyCheckResponse
    {
        public Guid Id { get; set; }

        public Guid SafetyRequirementId { get; set; }

        public string RequirementCode { get; set; } = string.Empty;

        public string RequirementName { get; set; } = string.Empty;

        public bool IsMandatory { get; set; }

        public string CheckState { get; set; } = string.Empty;

        public DateTime? DecidedAt { get; set; }

        public string? Note { get; set; }
    }

    public class RadConsumptionResponse
    {
        public Guid Id { get; set; }

        public string ItemType { get; set; } = string.Empty;

        public string ItemCode { get; set; } = string.Empty;

        public string ItemName { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        public string Unit { get; set; } = string.Empty;

        public bool ConsumedDespiteFailure { get; set; }

        public DateTime RecordedAt { get; set; }

        public string? Note { get; set; }
    }

    public class RadTransitionHistoryResponse
    {
        public Guid Id { get; set; }

        public Guid RadOrderId { get; set; }

        public Guid? RadStudyId { get; set; }

        public string Scope { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string? FromStatus { get; set; }

        public string ToStatus { get; set; } = string.Empty;

        public string? ReasonCode { get; set; }

        public string? ReasonNote { get; set; }

        public Guid ActorUserId { get; set; }

        public DateTime OccurredAt { get; set; }
    }

    public class RadModalityResponse
    {
        public Guid Id { get; set; }

        public string ModalityCode { get; set; } = string.Empty;

        public string ModalityName { get; set; } = string.Empty;

        public bool UsesIonisingRadiation { get; set; }

        public bool SupportsContrast { get; set; }

        /// <summary>
        /// Apakah modalitas ini sudah punya aturan keselamatan aktif. Bernilai <c>false</c>
        /// berarti setiap acquisition padanya akan ditolak sampai admin menetapkan aturannya.
        /// </summary>
        public bool HasActiveSafetyRule { get; set; }

        public bool IsActive { get; set; }
    }

    public class RadSafetyRequirementResponse
    {
        public Guid Id { get; set; }

        public string RequirementCode { get; set; } = string.Empty;

        public string RequirementName { get; set; } = string.Empty;

        public string? Category { get; set; }

        public string? Description { get; set; }

        public bool RequiresNote { get; set; }

        public string? SourceNote { get; set; }

        public bool IsActive { get; set; }
    }
}
