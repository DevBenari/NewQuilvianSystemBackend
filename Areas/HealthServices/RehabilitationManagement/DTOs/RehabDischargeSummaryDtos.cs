using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs
{
    // =====================================================================
    // Ringkasan akhir — api-contract v1 grup Rehab Discharge Summary (BE-RHB-015).
    // Satu dokumen per episode; path tetap /by-episode/{episodeId}.
    // =====================================================================

    /// <summary>Body <c>PUT /by-episode/{episodeId}</c>. Membuat draft bila belum ada.</summary>
    public class SaveRehabDischargeSummaryRequest
    {
        /// <summary>Wajib bila ringkasan sudah ada; diabaikan saat membuat.</summary>
        public int ExpectedVersion { get; set; }

        /// <summary>Wajib saat membuat; tidak dapat diganti.</summary>
        public Guid? EncounterId { get; set; }

        public RehabEpisodeClosureReason? ClosureReason { get; set; }
        public string? MainServicesText { get; set; }
        public string? GoalAchievementText { get; set; }
        public string? FinalConditionText { get; set; }
        public RehabFollowUpType FollowUpType { get; set; } = RehabFollowUpType.None;
        public string? FollowUpInstructionText { get; set; }
        public string? FollowUpTargetText { get; set; }
        public DateOnly? FollowUpDueDate { get; set; }
    }

    /// <summary>Satu catatan episode yang belum final (V-DIS-02).</summary>
    public class RehabPendingDocument
    {
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentTypeName { get; set; } = string.Empty;
        public Guid DocumentId { get; set; }
        public string? Reference { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }

    public class RehabDischargeSummaryResponse
    {
        public Guid EpisodeId { get; set; }
        public string EpisodeNumber { get; set; } = string.Empty;
        public RehabEpisodeStatus EpisodeStatus { get; set; }
        public string EpisodeStatusName { get; set; } = string.Empty;

        /// <summary><c>false</c> bila ringkasan belum pernah disimpan.</summary>
        public bool Exists { get; set; }

        public Guid? Id { get; set; }
        public Guid? EncounterId { get; set; }
        public string? EncounterNumber { get; set; }
        public RehabEpisodeClosureReason? ClosureReason { get; set; }
        public string? ClosureReasonName { get; set; }
        public string? MainServicesText { get; set; }
        public string? GoalAchievementText { get; set; }
        public string? FinalConditionText { get; set; }
        public RehabFollowUpType FollowUpType { get; set; }
        public string FollowUpTypeName { get; set; } = string.Empty;
        public string? FollowUpInstructionText { get; set; }
        public string? FollowUpTargetText { get; set; }
        public DateOnly? FollowUpDueDate { get; set; }
        public Guid? AuthorUserId { get; set; }
        public string? AuthorName { get; set; }
        public RehabRecordStatus? RecordStatus { get; set; }
        public string? RecordStatusName { get; set; }
        public DateTime? SignedAt { get; set; }
        public int Version { get; set; }

        public ClinicalDocumentIntegrityStatus? IntegrityStatus { get; set; }
        public string? IntegrityStatusName { get; set; }
        public ClinicalDocumentKind DocumentKind { get; set; } = ClinicalDocumentKind.RehabDischargeSummary;

        /// <summary>Catatan episode yang belum final; tanda tangan ditolak selama daftar ini berisi (V-DIS-02).</summary>
        public List<RehabPendingDocument> PendingDocuments { get; set; } = new();

        /// <summary>
        /// Keputusan reasesmen medis yang menandai episode siap ditutup (<c>BE-RHB-014</c>), terbaru dulu,
        /// sebagai bahan isian alasan penutupan.
        /// </summary>
        public List<string> ReadyToCloseSignals { get; set; } = new();

        /// <summary><c>update</c>/<c>sign</c> untuk penulis draft (atau <c>update</c> bila belum ada); <c>addendum</c> sesudah final.</summary>
        public List<string> AvailableActions { get; set; } = new();
    }
}
