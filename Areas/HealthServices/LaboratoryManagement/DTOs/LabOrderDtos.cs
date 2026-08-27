using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    public class CreateLabOrderRequest
    {
        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid ProcedureId { get; set; }
    }

    public class LabOrderListResponse
    {
        public Guid Id { get; set; }

        public Guid EncounterId { get; set; }

        public Guid ProcedureId { get; set; }

        public string ProcedureCode { get; set; } = string.Empty;

        public string ProcedureName { get; set; } = string.Empty;

        /// <summary>
        /// Status operasional pesanan. Bukan status pembayaran — Laboratorium tidak memiliki
        /// status finansial apa pun.
        /// </summary>
        public string OrderStatus { get; set; } = string.Empty;

        public int SpecimenCount { get; set; }

        /// <summary>
        /// Jumlah sampel yang sudah dinyatakan layak, yaitu jumlah komponen pemeriksaan yang
        /// sudah memenuhi milestone kelayakan tagih.
        /// </summary>
        public int AcceptedSpecimenCount { get; set; }

        public bool IsCancel { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class LabOrderDetailResponse : LabOrderListResponse
    {
        public DateTime? RequestedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string? StatusBeforeHold { get; set; }

        public int Version { get; set; }

        public DateTime? CancelDateTime { get; set; }

        public Guid? CancelBy { get; set; }
    }
}
