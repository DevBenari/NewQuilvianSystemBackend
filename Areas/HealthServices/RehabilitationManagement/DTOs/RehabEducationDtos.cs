using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs
{
    // =====================================================================
    // Edukasi dan persetujuan — api-contract v1 grup Rehab Education (BE-RHB-010).
    // =====================================================================

    /// <summary>Saringan <c>GET /by-episode/{episodeId}</c>.</summary>
    public class RehabEducationQuery
    {
        public Guid? ProgramId { get; set; }
    }

    /// <summary>Isian edukasi. Dipakai <c>POST /by-episode/{episodeId}</c> dan <c>PUT /{id}</c>.</summary>
    /// <remarks>Pemberi edukasi selalu pengguna yang mencatat; tidak dikirim klien.</remarks>
    public class SaveRehabEducationRequest
    {
        /// <summary>Wajib untuk <c>PUT</c>; diabaikan saat membuat.</summary>
        public int ExpectedVersion { get; set; }

        /// <summary>Program yang dijelaskan. Kosong = edukasi rencana episode secara umum.</summary>
        public Guid? ProgramId { get; set; }

        /// <summary>Wajib saat membuat; tidak dapat diganti sesudahnya.</summary>
        public Guid? EncounterId { get; set; }

        /// <summary>Waktu edukasi. Kosong = sekarang.</summary>
        public DateTime? EducatedAt { get; set; }

        public string? MaterialText { get; set; }
        public string? RecipientName { get; set; }
        public string? RecipientRelation { get; set; }
        public string? MethodText { get; set; }

        /// <summary>Kosong = <c>NotAssessed</c>. <c>NotAssessed</c> ditolak saat tanda tangan.</summary>
        public RehabUnderstandingLevel? UnderstandingLevel { get; set; }

        public string? BarrierText { get; set; }
        public RehabConsentStatus? ConsentStatus { get; set; }

        /// <summary>Wajib bila <c>Refused</c> atau <c>PartiallyAgreed</c>.</summary>
        public string? ConsentNote { get; set; }
    }

    public class RehabEducationItem
    {
        public Guid Id { get; set; }
        public Guid EpisodeId { get; set; }
        public Guid? ProgramId { get; set; }

        /// <summary>Contoh: "Fisioterapi v2". Kosong bila edukasi umum.</summary>
        public string? ProgramLabel { get; set; }

        public Guid EncounterId { get; set; }
        public string? EncounterNumber { get; set; }
        public DateTime EducatedAt { get; set; }

        public string MaterialText { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string RecipientRelation { get; set; } = string.Empty;
        public string MethodText { get; set; } = string.Empty;
        public RehabUnderstandingLevel UnderstandingLevel { get; set; }
        public string UnderstandingLevelName { get; set; } = string.Empty;
        public string? BarrierText { get; set; }
        public RehabConsentStatus ConsentStatus { get; set; }
        public string ConsentStatusName { get; set; } = string.Empty;
        public string? ConsentNote { get; set; }

        public Guid EducatorUserId { get; set; }
        public string? EducatorName { get; set; }
        public RehabRecordStatus RecordStatus { get; set; }
        public string RecordStatusName { get; set; } = string.Empty;
        public DateTime? SignedAt { get; set; }
        public int Version { get; set; }

        public ClinicalDocumentIntegrityStatus? IntegrityStatus { get; set; }
        public string? IntegrityStatusName { get; set; }

        /// <summary>Jenis dokumen RM (<c>RehabEducation</c> = 52) untuk addendum sesudah final.</summary>
        public ClinicalDocumentKind DocumentKind { get; set; } = ClinicalDocumentKind.RehabEducation;

        /// <summary><c>update</c>/<c>sign</c> untuk pemberi edukasi selama draft; <c>addendum</c> sesudah final.</summary>
        public List<string> AvailableActions { get; set; } = new();
    }
}
