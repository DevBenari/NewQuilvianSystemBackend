using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs
{
    public class EmergencyDepartureResponse
    {
        public Guid Id { get; set; }
        public Guid EmergencyVisitId { get; set; }
        public string DepartureNumber { get; set; } = string.Empty;

        public Guid? FromServiceUnitId { get; set; }

        /// <summary>
        /// Nama unit asal dan unit tujuan, disalin dari master saat balasan dibentuk. Layar
        /// menampilkan nama unit kepada perawat; identifier tidak pernah menjadi label yang
        /// dibaca petugas.
        /// </summary>
        public string? FromServiceUnitName { get; set; }

        public Guid ToServiceUnitId { get; set; }
        public string? ToServiceUnitName { get; set; }

        public EmergencyPhysicalStatus PhysicalStatus { get; set; }
        public EmergencyHandoverStatus HandoverStatus { get; set; }

        public DateTime RequestedAt { get; set; }
        public Guid RequestedByUserId { get; set; }
        public DateTime? DepartedAt { get; set; }
        public DateTime? ArrivedAt { get; set; }
        public Guid? SendingNurseUserId { get; set; }
        public Guid? ReceivingNurseUserId { get; set; }

        public string? DepartureReason { get; set; }

        public string? SituationSummary { get; set; }
        public string? BackgroundSummary { get; set; }
        public string? AssessmentSummary { get; set; }
        public string? RecommendationSummary { get; set; }
        public string? UnavailableSections { get; set; }
        public string? UnavailableSectionReason { get; set; }

        public string? AllergySnapshot { get; set; }
        public Guid? LastVitalSignId { get; set; }
        public string? TriageLevelSnapshot { get; set; }

        public string? HandoverRejectionReason { get; set; }
        public string? CancellationReason { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }

        public List<EmergencyDepartureEventResponse> Events { get; set; } = new();
        public List<EmergencyHandoverOrderItemResponse> OrderItems { get; set; } = new();
    }

    public class EmergencyDepartureEventResponse
    {
        public Guid Id { get; set; }
        public Guid EmergencyDepartureId { get; set; }
        public EmergencyDepartureEventType EventType { get; set; }
        public DateTime OccurredAt { get; set; }
        public DateTime RecordedAt { get; set; }
        public Guid RecordedByUserId { get; set; }
        public string? Reason { get; set; }
        public string? DowntimeReference { get; set; }
        public bool IsEffective { get; set; }
        public Guid? SupersedesEventId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
    }

    public class EmergencyHandoverOrderItemResponse
    {
        public Guid Id { get; set; }
        public Guid EmergencyDepartureId { get; set; }
        public EmergencyOrderKind OrderKind { get; set; }
        public EmergencyOrderSource OrderSource { get; set; }
        public Guid? OrderReferenceId { get; set; }
        public string? ExternalReference { get; set; }
        public string OrderDescription { get; set; } = string.Empty;
        public EmergencyOrderAction Action { get; set; }
        public string? ActionReason { get; set; }
        public Guid ActionByUserId { get; set; }
        public DateTime ActionAt { get; set; }
        public Guid? ToServiceUnitId { get; set; }
        public string? ToServiceUnitName { get; set; }
        public EmergencyOrderAcceptanceStatus AcceptanceStatus { get; set; }
        public Guid? AcceptedByUserId { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public string? RejectionReason { get; set; }
        public bool IsEffective { get; set; }
        public Guid? SupersedesOrderItemId { get; set; }

        /// <summary>
        /// Selalu benar untuk pesanan laboratorium. Layar <b>wajib</b> menyatakan bahwa sikap
        /// ini ditetapkan petugas, bukan dibaca dari sistem laboratorium — validation bagian 5
        /// aturan 5, <c>IGD-DEC-101</c>.
        /// </summary>
        public bool IsActionSetManually { get; set; }
    }

    public class CreateEmergencyDepartureRequest
    {
        [Required]
        public Guid EmergencyVisitId { get; set; }

        [MaxLength(50)]
        public string? DepartureNumber { get; set; }

        public Guid? FromServiceUnitId { get; set; }

        [Required]
        public Guid ToServiceUnitId { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public Guid RequestedByUserId { get; set; }

        public Guid? SendingNurseUserId { get; set; }

        [MaxLength(1000)]
        public string? DepartureReason { get; set; }

        [MaxLength(2000)]
        public string? SituationSummary { get; set; }

        [MaxLength(2000)]
        public string? BackgroundSummary { get; set; }

        [MaxLength(2000)]
        public string? AssessmentSummary { get; set; }

        [MaxLength(2000)]
        public string? RecommendationSummary { get; set; }

        /// <summary>
        /// Bagian SBAR yang ditandai tidak dapat diisi, dipisah koma — misalnya
        /// <c>Background,Assessment</c>. Setiap bagian yang kosong dan tidak disebut di sini
        /// akan ditolak.
        /// </summary>
        [MaxLength(250)]
        public string? UnavailableSections { get; set; }

        [MaxLength(1000)]
        public string? UnavailableSectionReason { get; set; }

        [MaxLength(1000)]
        public string? AllergySnapshot { get; set; }

        public Guid? LastVitalSignId { get; set; }

        [MaxLength(150)]
        public string? TriageLevelSnapshot { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Pesanan yang belum selesai beserta sikapnya. Boleh kosong ketika dokumen dibuat,
        /// dan wajib lengkap sebelum dokumen diajukan — validation bagian 5 aturan 1.
        /// </summary>
        public List<EmergencyHandoverOrderItemInput> OrderItems { get; set; } = new();
    }

    public class UpdateEmergencyDepartureRequest : CreateEmergencyDepartureRequest
    {
    }

    public class EmergencyHandoverOrderItemInput
    {
        [Required]
        public EmergencyOrderKind OrderKind { get; set; }

        [Required]
        public EmergencyOrderSource OrderSource { get; set; } = EmergencyOrderSource.Internal;

        public Guid? OrderReferenceId { get; set; }

        [MaxLength(150)]
        public string? ExternalReference { get; set; }

        [Required]
        [MaxLength(500)]
        public string OrderDescription { get; set; } = string.Empty;

        [Required]
        public EmergencyOrderAction Action { get; set; }

        [MaxLength(1000)]
        public string? ActionReason { get; set; }

        public Guid? ToServiceUnitId { get; set; }

        /// <summary>
        /// Diisi ketika baris ini adalah <b>sikap pengganti</b> atas pesanan yang ditolak unit
        /// penerima — <c>IGD-DEC-102</c> butir (c).
        /// </summary>
        public Guid? SupersedesOrderItemId { get; set; }
    }

    /// <summary>
    /// Mencatat keberangkatan pasien dari IGD.
    /// </summary>
    public class DepartEmergencyDepartureRequest
    {
        /// <summary>
        /// Waktu kejadian sebenarnya. Kosong berarti sekarang. Tidak boleh di masa depan —
        /// validation bagian 4 aturan 10.
        /// </summary>
        public DateTime? OccurredAt { get; set; }

        [MaxLength(250)]
        public string? DowntimeReference { get; set; }

        [MaxLength(1000)]
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Mencatat kedatangan pasien di unit tujuan. Wajib berwenang atas unit tujuan —
    /// validation bagian 4 aturan 5.
    /// </summary>
    public class ArriveEmergencyDepartureRequest
    {
        public DateTime? OccurredAt { get; set; }

        [MaxLength(250)]
        public string? DowntimeReference { get; set; }

        public Guid? ReceivingNurseUserId { get; set; }
    }

    public class UpdateEmergencyHandoverStatusRequest
    {
        [Required]
        public EmergencyHandoverStatus HandoverStatus { get; set; }

        /// <summary>
        /// Wajib ketika status menjadi <c>Rejected</c>, dan wajib menyebut bagian mana yang
        /// dianggap kurang — validation bagian 4 aturan 4.
        /// </summary>
        [MaxLength(1000)]
        public string? RejectionReason { get; set; }

        public DateTime? OccurredAt { get; set; }
    }

    public class CancelEmergencyDepartureRequest
    {
        [Required]
        [MaxLength(1000)]
        public string CancellationReason { get; set; } = string.Empty;

        public DateTime? OccurredAt { get; set; }
    }

    /// <summary>
    /// Koreksi kejadian yang sudah tercatat — <c>IGD-DEC-065</c>.
    /// </summary>
    public class AmendDepartureEventRequest
    {
        [Required]
        public Guid EventId { get; set; }

        [Required]
        public DateTime OccurredAt { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? DowntimeReference { get; set; }
    }

    /// <summary>
    /// Pembalikan kejadian, menuntut persetujuan orang kedua — <c>IGD-DEC-066</c>.
    /// </summary>
    public class ReverseDepartureEventRequest
    {
        [Required]
        public Guid EventId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Pemberi persetujuan. Wajib, dan <b>tidak boleh sama</b> dengan pelaku pembalikan —
        /// validation bagian 4 aturan 11 dan 12.
        /// </summary>
        [Required]
        public Guid ApprovedByUserId { get; set; }
    }

    /// <summary>
    /// Menetapkan sikap atas satu pesanan, atau menggantikan sikap yang ditolak.
    /// </summary>
    public class SetOrderItemActionRequest
    {
        [Required]
        public EmergencyHandoverOrderItemInput Item { get; set; } = new();
    }

    /// <summary>
    /// Penerimaan atau penolakan satu pesanan oleh unit penerima — <c>IGD-DEC-102</c>.
    /// </summary>
    public class SetOrderItemAcceptanceRequest
    {
        [Required]
        public EmergencyOrderAcceptanceStatus AcceptanceStatus { get; set; }

        [MaxLength(1000)]
        public string? RejectionReason { get; set; }
    }
}
