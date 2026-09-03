using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>
    /// Permintaan merencanakan satu sampel sekaligus satu komponen pemeriksaan.
    ///
    /// Barcode sengaja tidak ada di sini. Barcode dibuat server dan tidak pernah diterima dari
    /// client, sesuai keputusan author <c>RJ-BIL-OQ-010</c>.
    /// </summary>
    public class PlanLabSpecimenRequest
    {
        /// <summary>
        /// Procedure komponen pemeriksaan. Bila kosong, dipakai procedure pesanan.
        /// </summary>
        public Guid? ProcedureId { get; set; }

        [MaxLength(200)]
        public string? SpecimenDescription { get; set; }
    }

    public class CollectLabSpecimenRequest
    {
        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    public class ReceiveLabSpecimenRequest
    {
        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    public class AcceptLabSpecimenRequest
    {
        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    /// <summary>
    /// Permintaan menolak sampel. <see cref="ReasonCode"/> wajib dan harus berasal dari katalog
    /// alasan; free-text saja tidak diterima.
    /// </summary>
    public class RejectLabSpecimenRequest
    {
        [Required]
        [MaxLength(50)]
        public string ReasonCode { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    /// <summary>
    /// Permintaan pengambilan ulang. Menghasilkan sampel baru dengan identitas dan barcode
    /// baru yang tetap menunjuk sampel yang ditolak sebagai asal-usulnya.
    /// </summary>
    public class RequestLabRecollectionRequest
    {
        [Required]
        public LabRecollectionCause? Cause { get; set; }

        [MaxLength(1000)]
        public string? Reason { get; set; }
    }

    public class HoldLabRequest
    {
        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class ResumeLabRequest
    {
        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    public class CancelLabSpecimenRequest
    {
        [MaxLength(1000)]
        public string? Reason { get; set; }
    }

    public class LabSpecimenResponse
    {
        public Guid Id { get; set; }

        public Guid LabOrderId { get; set; }

        public Guid ProcedureId { get; set; }

        public string SpecimenBarcode { get; set; } = string.Empty;

        public int SpecimenSequence { get; set; }

        public string? SpecimenDescription { get; set; }

        public string SpecimenStatus { get; set; } = string.Empty;

        public string? ProcedureCode { get; set; }

        public string? ProcedureName { get; set; }

        public decimal? UnitPrice { get; set; }

        public DateTime? CollectedAt { get; set; }

        public DateTime? ReceivedAt { get; set; }

        public DateTime? DecidedAt { get; set; }

        public string? RejectionReasonCode { get; set; }

        public string? RejectionNote { get; set; }

        public Guid? SupersededSpecimenId { get; set; }

        public string? RecollectionCause { get; set; }

        public int Version { get; set; }

        /// <summary>
        /// Ringkasan hasil penyerahan fakta ke Billing. Diisi hanya pada tindakan yang memang
        /// menerbitkan fakta, dan tidak pernah memuat keputusan finansial.
        /// </summary>
        public LabBillingHandoffResponse? BillingHandoff { get; set; }
    }

    /// <summary>
    /// Hasil penyerahan fakta klinis ke Billing.
    ///
    /// Berisi keterangan proses, bukan status pembayaran. Laboratorium tidak pernah menyatakan
    /// sesuatu sudah dibayar, dibatalkan secara finansial, atau disetujui penjamin.
    /// </summary>
    public class LabBillingHandoffResponse
    {
        public string Kind { get; set; } = string.Empty;

        public bool IsClinicallySafe { get; set; }

        public Guid? MilestoneFactId { get; set; }

        public int? MilestoneFactVersion { get; set; }

        public string? Code { get; set; }

        public string? Message { get; set; }
    }

    public class LabTransitionHistoryResponse
    {
        public Guid Id { get; set; }

        public Guid LabOrderId { get; set; }

        public Guid? LabSpecimenId { get; set; }

        public string Scope { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string? FromStatus { get; set; }

        public string ToStatus { get; set; } = string.Empty;

        public string? ReasonCode { get; set; }

        public string? ReasonNote { get; set; }

        public Guid ActorUserId { get; set; }

        public DateTime OccurredAt { get; set; }
    }
}
