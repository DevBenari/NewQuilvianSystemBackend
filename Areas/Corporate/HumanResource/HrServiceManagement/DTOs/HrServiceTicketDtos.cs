using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs
{
    public class HrServiceTicketDto
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid HrServiceCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public Guid HrServiceTypeId { get; set; }
        public string ServiceTypeName { get; set; } = string.Empty;
        public Guid RequestedByWorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string RequestDescription { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = "Submitted";
        public string PriorityTier { get; set; } = "Standar";
        public int SlaTargetHours { get; set; }
        public DateTime? SlaDeadlineUtc { get; set; }
        public string SlaStatus { get; set; } = "WithinSla";
        public Guid? AssignedToUserId { get; set; }
        public string? AssignedToUserName { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? FirstResponseAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionSummary { get; set; }
        public int TotalAttachments { get; set; }
        public int TotalComments { get; set; }
    }

    public class CreateHrServiceTicketRequestDto
    {
        [Required(ErrorMessage = "Kategori layanan wajib dipilih")]
        public Guid HrServiceCategoryId { get; set; }

        [Required(ErrorMessage = "Jenis layanan wajib dipilih")]
        public Guid HrServiceTypeId { get; set; }

        [Required(ErrorMessage = "Subjek tiket permohonan wajib diisi")]
        [StringLength(300, MinimumLength = 5, ErrorMessage = "Subjek tiket minimal 5 karakter")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rincian penjelasan permohonan wajib diisi")]
        [StringLength(5000, MinimumLength = 10, ErrorMessage = "Deskripsi tiket minimal 10 karakter")]
        public string RequestDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tier prioritas wajib ditentukan (Standar, Khusus, Cito)")]
        [RegularExpression("^(Standar|Khusus|Cito)$", ErrorMessage = "PriorityTier harus salah satu dari: Standar, Khusus, Cito")]
        public string PriorityTier { get; set; } = "Standar";

        [StringLength(100)]
        public string? ExternalReferenceNumber { get; set; }

        public string? RequestPayloadJson { get; set; }

        /// <summary>
        /// Nomor/Nama file disposisi resmi pimpinan jika memilih prioritas Cito (HRS-B1, VR-HRS-02)
        /// </summary>
        [StringLength(255)]
        public string? EmergencyDispositionReference { get; set; }
    }

    public class AssignTicketRequestDto
    {
        [Required(ErrorMessage = "ID Analis / Petugas HR tujuan wajib diisi")]
        public Guid AssignedToUserId { get; set; }

        [StringLength(500)]
        public string? AssignmentNotes { get; set; }
    }

    public class UpdateTicketStatusRequestDto
    {
        [Required(ErrorMessage = "Status tiket baru wajib diisi")]
        [RegularExpression("^(InReview|WaitingStaffResponse|InProgress|Resolved|Closed|Cancelled)$",
            ErrorMessage = "Status tiket baru tidak valid")]
        public string NewStatus { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? ResolutionOrReasonNotes { get; set; }
    }

    public class AddTicketCommentRequestDto
    {
        [Required(ErrorMessage = "Pesan komentar tidak boleh kosong")]
        [StringLength(2000, MinimumLength = 2, ErrorMessage = "Pesan komentar 2-2000 karakter")]
        public string CommentText { get; set; } = string.Empty;

        /// <summary>
        /// Jika true, hanya terlihat oleh sesama staf/analis HR dan disembunyikan dari portal ESS pegawai
        /// </summary>
        public bool IsInternalNote { get; set; } = false;
    }

    public class TicketCommentDto
    {
        public Guid Id { get; set; }
        public Guid RequestId { get; set; }
        public Guid AuthorUserId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string CommentText { get; set; } = string.Empty;
        public bool IsInternalNote { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class TicketAttachmentDto
    {
        public Guid Id { get; set; }
        public Guid RequestId { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string AttachmentType { get; set; } = "SupportingDocument";
        public DateTime UploadedAtUtc { get; set; }
    }
}
