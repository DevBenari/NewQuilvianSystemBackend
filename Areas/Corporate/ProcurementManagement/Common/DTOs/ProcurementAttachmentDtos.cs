using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs
{
    /// <summary>Saringan daftar lampiran satu dokumen.</summary>
    public sealed class ProcurementAttachmentQuery
    {
        [Required, MaxLength(40)] public string DocumentType { get; set; } = string.Empty;
        [Required] public Guid DocumentId { get; set; }

        /// <summary>true = sertakan versi lama dan lampiran yang sudah ditarik.</summary>
        public bool IncludeHistory { get; set; }
    }

    /// <summary>Unggah lampiran (multipart). Isi <see cref="ReplacesAttachmentId"/> untuk membuat versi baru.</summary>
    public sealed class UploadProcurementAttachmentRequest
    {
        [Required, MaxLength(40)] public string DocumentType { get; set; } = string.Empty;
        [Required] public Guid DocumentId { get; set; }
        [MaxLength(50)] public string? AttachmentCategory { get; set; }
        public Guid? ReplacesAttachmentId { get; set; }
        [Required] public IFormFile? File { get; set; }
    }

    public sealed class ProcurementAttachmentResponse
    {
        public Guid Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public Guid DocumentId { get; set; }
        public string? AttachmentCategory { get; set; }
        public Guid AttachmentKey { get; set; }
        public int VersionNo { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsWithdrawn { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public Guid UploadedByUserId { get; set; }
        public string? UploadedByName { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
