namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs
{
    /// <summary>
    /// Satu baris riwayat dokumen untuk layar "Riwayat Pengajuan" dan tab riwayat dokumen
    /// (endpoint <c>GET /{id}/history</c> tiap dokumen). Cuplikan <c>SnapshotJson</c> sengaja
    /// tidak diekspos.
    /// </summary>
    public sealed class DocumentHistoryResponse
    {
        public Guid Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public Guid DocumentId { get; set; }
        public string? DocumentNumber { get; set; }
        public string ActionCode { get; set; } = string.Empty;
        public string? FromStatus { get; set; }
        public string? ToStatus { get; set; }
        public string? Reason { get; set; }
        public Guid ActorUserId { get; set; }
        public string? ActorName { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
    }
}
