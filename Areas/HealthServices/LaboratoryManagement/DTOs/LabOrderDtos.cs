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

        public bool IsCancel { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class LabOrderDetailResponse : LabOrderListResponse
    {
        public DateTime? CancelDateTime { get; set; }

        public Guid? CancelBy { get; set; }
    }
}
