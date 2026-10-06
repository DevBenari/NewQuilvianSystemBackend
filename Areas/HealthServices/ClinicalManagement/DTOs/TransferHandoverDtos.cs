using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    /// <summary>Saringan dokumen serah terima transfer (<c>BE-RWI-183</c>, API 11.8).</summary>
    public class TransferHandoverQuery
    {
        public Guid? EpisodeId { get; set; }

        /// <summary>Unit asal atau unit tujuan dokumen.</summary>
        public Guid? ServiceUnitId { get; set; }

        public CliTransferHandoverStatus? Status { get; set; }
    }

    /// <summary>Isian yang diketik pengirim (<c>PUT /{id}/draft</c>).</summary>
    public class SaveTransferHandoverDraftRequest
    {
        [MaxLength(4000)] public string? SoapSummary { get; set; }
        [MaxLength(2000)] public string? HandedItems { get; set; }
        [MaxLength(2000)] public string? SpecialInstructions { get; set; }
        [Range(0, int.MaxValue)] public int ExpectedVersion { get; set; }
    }

    /// <summary>Kirim dokumen; potret klinis dibekukan server (<c>PATCH /{id}/send</c>).</summary>
    public class SendTransferHandoverRequest
    {
        [Range(0, int.MaxValue)] public int ExpectedVersion { get; set; }
    }

    /// <summary>Terima atau tolak beralasan (<c>PATCH /{id}/accept</c>).</summary>
    public class AcceptTransferHandoverRequest
    {
        public bool Accept { get; set; } = true;
        [MaxLength(1000)] public string? RejectionReason { get; set; }
        [Range(0, int.MaxValue)] public int ExpectedVersion { get; set; }
    }

    /// <summary>Potret klinis yang dibekukan saat dikirim, beserta rujukan catatan sumbernya.</summary>
    public class TransferHandoverSnapshot
    {
        public DateTime CapturedAt { get; set; }
        public TransferHandoverVitalSnapshot? Vital { get; set; }
        public TransferHandoverPainSnapshot? Pain { get; set; }
        public TransferHandoverFallRiskSnapshot? FallRisk { get; set; }
        public TransferHandoverFluidBalanceSnapshot? FluidBalance { get; set; }
    }

    public class TransferHandoverVitalSnapshot
    {
        public Guid VitalSignId { get; set; }
        public DateTime RecordedAt { get; set; }
        public int? SystolicBp { get; set; }
        public int? DiastolicBp { get; set; }
        public int? PulseRate { get; set; }
        public int? RespiratoryRate { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? SpO2 { get; set; }
        public int? GcsEye { get; set; }
        public int? GcsVerbal { get; set; }
        public int? GcsMotor { get; set; }
        public int? GcsTotal { get; set; }
        public string? Consciousness { get; set; }
    }

    public class TransferHandoverPainSnapshot
    {
        public int? Score { get; set; }
        public string? State { get; set; }
        public DateTime RecordedAt { get; set; }
        public Guid SourceId { get; set; }
        public string SourceKind { get; set; } = string.Empty;
    }

    public class TransferHandoverFallRiskSnapshot
    {
        public int? Score { get; set; }
        public string? Status { get; set; }
        public DateTime RecordedAt { get; set; }
        public Guid AssessmentId { get; set; }
    }

    /// <summary>Balance cairan 24 jam terakhir sebelum dikirim.</summary>
    public class TransferHandoverFluidBalanceSnapshot
    {
        public DateTime WindowStart { get; set; }
        public DateTime WindowEnd { get; set; }
        public decimal IntakeMl { get; set; }
        public decimal OutputMl { get; set; }
        public decimal BalanceMl { get; set; }
        public int EntryCount { get; set; }
    }

    /// <summary>Dokumen serah terima transfer, sembilan bagian V1 (API 11.8). Tidak memuat rupiah.</summary>
    public class TransferHandoverResponse
    {
        public Guid Id { get; set; }
        public Guid InpEpisodeId { get; set; }
        public string EpisodeNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? MedicalRecordNumber { get; set; }
        public Guid FromPlacementId { get; set; }
        public Guid ToPlacementId { get; set; }
        public Guid FromServiceUnitId { get; set; }
        public string? FromServiceUnitName { get; set; }
        public Guid ToServiceUnitId { get; set; }
        public string? ToServiceUnitName { get; set; }
        public DateTime TransferredAt { get; set; }
        public string? TransferReason { get; set; }
        public CliTransferHandoverStatus Status { get; set; }

        /// <summary>Selama bukan <c>Accepted</c>, kedua unit melihat "Serah terima tertunda".</summary>
        public bool IsPending { get; set; }

        public string? SoapSummary { get; set; }
        public string? HandedItems { get; set; }
        public string? SpecialInstructions { get; set; }
        public TransferHandoverSnapshot? Snapshot { get; set; }
        public string? SentByName { get; set; }
        public DateTime? SentAt { get; set; }
        public string? ReceivedByName { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public string? RejectionReason { get; set; }
        public int Version { get; set; }
    }
}
