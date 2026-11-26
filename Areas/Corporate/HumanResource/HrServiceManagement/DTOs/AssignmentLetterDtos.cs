using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs
{
    /// <summary>Status Surat Tugas (BE-HRD-383).</summary>
    public static class AssignmentLetterStatuses
    {
        public const string Draft = "Draft";
        public const string Submitted = "Submitted";
        public const string RevisionNeeded = "RevisionNeeded";
        public const string PendingSignOff = "PendingSignOff";
        public const string Endorsed = "Endorsed";
        public const string Issued = "Issued";
        public const string InExecution = "InExecution";
        public const string Completed = "Completed";
        public const string Rejected = "Rejected";
        public const string Cancelled = "Cancelled";

        public static readonly string[] All =
        {
            Draft, Submitted, RevisionNeeded, PendingSignOff, Endorsed, Issued, InExecution, Completed, Rejected, Cancelled
        };

        public static string Label(string status) => status switch
        {
            Draft => "Draf",
            Submitted => "Diajukan ke HRD",
            RevisionNeeded => "Perlu Revisi",
            PendingSignOff => "Menunggu Pengesahan",
            Endorsed => "Disahkan",
            Issued => "Diterbitkan",
            InExecution => "Sedang Dilaksanakan",
            Completed => "Selesai",
            Rejected => "Ditolak",
            Cancelled => "Dibatalkan",
            _ => status
        };
    }

    public class AssignmentLetterParticipantInput
    {
        [Required]
        public Guid WorkforceProfileId { get; set; }

        [MaxLength(100)]
        public string? ParticipantRole { get; set; }
    }

    public class SaveAssignmentLetterRequest
    {
        [Required, MaxLength(200)]
        public string ActivityName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string DutyCategory { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? BasisDescription { get; set; }

        [MaxLength(500)]
        public string? BasisDocumentReference { get; set; }

        [Required, MaxLength(2000)]
        public string TaskDescription { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Destination { get; set; } = string.Empty;

        [Required]
        public DateTime StartAt { get; set; }

        [Required]
        public DateTime EndAt { get; set; }

        public Guid? RequestingOrganizationUnitId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Surat Tugas wajib memuat minimal satu peserta.")]
        public List<AssignmentLetterParticipantInput> Participants { get; set; } = new();
    }

    public class VerifyAssignmentLetterRequest
    {
        /// <summary><c>Approve</c> (teruskan ke pengesahan), <c>RequestRevision</c>, atau <c>Reject</c>.</summary>
        [Required]
        [RegularExpression("^(Approve|RequestRevision|Reject)$", ErrorMessage = "Keputusan verifikasi harus Approve, RequestRevision, atau Reject.")]
        public string Decision { get; set; } = "Approve";

        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    public class EndorseAssignmentLetterRequest
    {
        /// <summary><c>Endorse</c> atau <c>Reject</c>.</summary>
        [Required]
        [RegularExpression("^(Endorse|Reject)$", ErrorMessage = "Keputusan pengesahan harus Endorse atau Reject.")]
        public string Decision { get; set; } = "Endorse";

        [MaxLength(1000)]
        public string? Note { get; set; }

        /// <summary>Jabatan penandatangan yang dicetak; bila kosong memakai jabatan utama akun pengesah.</summary>
        [MaxLength(200)]
        public string? SignerTitle { get; set; }
    }

    public class CompleteAssignmentLetterRequest
    {
        [Required, MaxLength(2000)]
        public string ResultReport { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ResultEvidenceReference { get; set; }
    }

    public class CancelAssignmentLetterRequest
    {
        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class AssignmentLetterQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class AssignmentLetterParticipantResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? ParticipantRole { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string? ProfileCode { get; set; }
        public string? PositionName { get; set; }
        public string? UnitName { get; set; }
        public Guid? OfficialDutyRequestId { get; set; }
        /// <summary>Peringatan benturan jadwal saat ini (shift/penugasan lain yang tumpang tindih). Tidak memblokir.</summary>
        public List<string> ScheduleConflicts { get; set; } = new();
    }

    public class AssignmentLetterListItemResponse
    {
        public Guid Id { get; set; }
        public string? LetterNumber { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public string DutyCategory { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string LetterStatus { get; set; } = string.Empty;
        public string LetterStatusLabel { get; set; } = string.Empty;
        public int ParticipantCount { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class AssignmentLetterTimelineEntry
    {
        public string Step { get; set; } = string.Empty;
        public Guid? ActorUserId { get; set; }
        public string? ActorName { get; set; }
        public DateTime? At { get; set; }
        public string? Note { get; set; }
    }

    public class AssignmentLetterDetailResponse : AssignmentLetterListItemResponse
    {
        public string? BasisDescription { get; set; }
        public string? BasisDocumentReference { get; set; }
        public string TaskDescription { get; set; } = string.Empty;
        public Guid? RequestingOrganizationUnitId { get; set; }
        public string? SignerName { get; set; }
        public string? SignerTitle { get; set; }
        public string? RevisionNote { get; set; }
        public string? RejectionReason { get; set; }
        public string? CancellationReason { get; set; }
        public string? ResultReport { get; set; }
        public string? ResultEvidenceReference { get; set; }
        public bool HasDocument { get; set; }
        public List<AssignmentLetterParticipantResponse> Participants { get; set; } = new();
        /// <summary>Jejak aktor terpisah: pembuat, pengaju, verifikator, pengesah, penerbit, pelaksana, pembatal.</summary>
        public List<AssignmentLetterTimelineEntry> Timeline { get; set; } = new();
        /// <summary>Aksi yang tersedia bagi pengguna saat ini menurut status dan hak akses.</summary>
        public List<string> AvailableActions { get; set; } = new();
    }
}
