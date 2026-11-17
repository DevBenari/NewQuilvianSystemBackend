namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs
{
    /// <summary>
    /// Langkah dan keputusan persetujuan <c>Wfl</c> satu dokumen Pengadaan untuk tab Persetujuan
    /// (<c>GET /{id}/approval</c> tiap dokumen). Dibaca lewat izin baca dokumen itu sendiri, sehingga
    /// pengusul tidak memerlukan <c>WorkflowInstance : Read</c> yang membuka seluruh instance <c>Wfl</c>.
    /// Konteks permintaan dan cuplikan definisi sengaja tidak diekspos.
    /// </summary>
    public sealed class ProcurementApprovalTimelineResponse
    {
        public Guid WorkflowInstanceId { get; set; }
        public string? RequestNumber { get; set; }
        public string? WorkflowName { get; set; }
        public string WorkflowStatus { get; set; } = string.Empty;
        public DateTime? SubmittedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<ProcurementApprovalStepResponse> Steps { get; set; } = new();
    }

    public sealed class ProcurementApprovalStepResponse
    {
        public Guid Id { get; set; }
        public int StepOrder { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string StepStatus { get; set; } = string.Empty;
        public DateTime? DueAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<ProcurementApprovalActionResponse> Actions { get; set; } = new();
    }

    public sealed class ProcurementApprovalActionResponse
    {
        public Guid Id { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public DateTime ActionAt { get; set; }
        public string? ActorName { get; set; }
        public bool IsSystemAction { get; set; }
        public string? Comment { get; set; }
        public string? ReasonName { get; set; }
    }
}
