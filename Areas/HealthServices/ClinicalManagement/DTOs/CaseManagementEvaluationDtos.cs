using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    /// <summary>Membuat konsep Evaluasi Awal MPP — api-contract 0.5.0 bagian 7.3.</summary>
    public class CreateCaseManagementEvaluationRequest
    {
        [Required]
        public Guid EpisodeId { get; set; }

        /// <summary>Waktu klinis; kosong berarti saat simpan.</summary>
        public DateTime? ClinicalDateTime { get; set; }

        /// <summary>Versi checklist yang dimuat formulir; wajib bila <see cref="Responses"/> berisi.</summary>
        public Guid? InstrumentVersionId { get; set; }

        public Dictionary<string, JsonElement>? Responses { get; set; }

        /// <summary>Alternatif header <c>Idempotency-Key</c>.</summary>
        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    public class UpdateCaseManagementEvaluationRequest
    {
        public DateTime? ClinicalDateTime { get; set; }

        public Guid? InstrumentVersionId { get; set; }

        public Dictionary<string, JsonElement>? Responses { get; set; }

        /// <summary><c>UpdateDateTime</c> — atau <c>CreateDateTime</c> — dari pembacaan terakhir. Wajib.</summary>
        public DateTime? ExpectedUpdateDate { get; set; }
    }

    public class CancelCaseManagementEvaluationRequest
    {
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    public class CaseManagementEvaluationAddendumRequest
    {
        [MaxLength(500)]
        public string? Reason { get; set; }

        public string? Content { get; set; }
    }

    public class CaseManagementEvaluationResponse
    {
        public Guid Id { get; set; }
        public string EvaluationNumber { get; set; } = string.Empty;
        public Guid EncounterId { get; set; }
        public Guid InpEpisodeId { get; set; }
        public Guid PatientId { get; set; }
        public Guid ServiceUnitIdSnapshot { get; set; }
        public CaseManagementEvaluationStatus EvaluationStatus { get; set; }
        public string EvaluationStatusLabel { get; set; } = string.Empty;
        public Guid AuthorEmployeeId { get; set; }
        public string? AuthorName { get; set; }
        public Guid AuthorUserId { get; set; }
        public DateTime ClinicalDateTime { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Guid? CompletedByUserId { get; set; }
        public DateTime? CancelledAt { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public string? CancelReason { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }

        /// <summary>Jawaban delapan bagian checklist beserta versinya.</summary>
        public AssessmentInstrumentResultResponse? Checklist { get; set; }

        /// <summary><c>false</c> selama jenis dokumen <c>14</c> belum tersedia — <c>INT-KEP-12</c>.</summary>
        public bool IsAddendumAvailable { get; set; }

        public string? AddendumUnavailableReason { get; set; }

        /// <summary>Aksi yang tersedia bagi pengguna pada keadaan dokumen saat ini.</summary>
        public List<string> AvailableActions { get; set; } = new();

        public bool IsReplay { get; set; }
    }
}
