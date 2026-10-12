using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs
{
    /// <summary>
    /// Status satu pasien Pilot pada rekonsiliasi MRN RSMMC (`BE-PAT-MIG-001`).
    /// </summary>
    public static class RsmmcPilotMrnReconciliationStatus
    {
        /// <summary>Simulasi: pasien layak dan akan diubah bila dijalankan sungguhan.</summary>
        public const string Ready = "READY";

        /// <summary>Eksekusi: MRN dan QR pasien sudah dipindahkan ke nomor kanonik.</summary>
        public const string Reconciled = "RECONCILED";

        /// <summary>MRN sudah kanonik dan path QR sesuai. Tidak dihitung dalam limit.</summary>
        public const string AlreadyReconciled = "ALREADY_RECONCILED";

        /// <summary>MRN sudah kanonik tetapi path QR tidak sesuai. Dilaporkan, tidak diperbaiki.</summary>
        public const string QrInconsistent = "QR_INCONSISTENT";

        /// <summary>MRN tujuan dimiliki pasien lain, atau artefak QR tujuan sudah ada.</summary>
        public const string Conflict = "CONFLICT";

        /// <summary>Kegagalan tak terduga. Data pasien tidak berubah.</summary>
        public const string Failed = "FAILED";
    }

    /// <summary>
    /// Jenis konflik, supaya dua sebab <see cref="RsmmcPilotMrnReconciliationStatus.Conflict"/>
    /// tetap dapat dibedakan.
    /// </summary>
    public static class RsmmcPilotMrnConflictType
    {
        public const string MedicalRecordNumberOwnedByAnotherPatient = "MRN_OWNED_BY_ANOTHER_PATIENT";

        public const string QrArtifactAlreadyExists = "QR_ARTIFACT_ALREADY_EXISTS";
    }

    public class RsmmcPilotMrnReconcileRequest
    {
        [Required]
        public Guid? BatchId { get; set; }

        /// <summary>
        /// Bila tidak dikirim, dianggap <c>true</c> (`PAT-OQ-003`). Tidak pernah ada default
        /// implisit <c>false</c>.
        /// </summary>
        public bool? DryRun { get; set; }

        /// <summary>
        /// Jumlah maksimum pasien layak yang diproses. Bawaan 25, rentang 1 sampai 100.
        /// </summary>
        [Range(1, 100)]
        public int? Limit { get; set; }
    }

    public class RsmmcPilotMrnReconcileResponse
    {
        public Guid BatchId { get; set; }

        public bool DryRun { get; set; }

        public int Limit { get; set; }

        public string EnvironmentName { get; set; } = string.Empty;

        public int TotalPilot { get; set; }

        public int AlreadyReconciled { get; set; }

        public int QrInconsistent { get; set; }

        /// <summary>Pasien yang MRN-nya masih berbeda dari MRN kanonik saat panggilan dimulai.</summary>
        public int EligibleBeforeRun { get; set; }

        /// <summary>Pasien layak yang dipilih panggilan ini, paling banyak <see cref="Limit"/>.</summary>
        public int Selected { get; set; }

        public int Ready { get; set; }

        public int Reconciled { get; set; }

        public int Conflict { get; set; }

        public int Failed { get; set; }

        /// <summary>Pasien layak yang belum direkonsiliasi sesudah panggilan ini.</summary>
        public int Remaining { get; set; }

        public bool Stopped { get; set; }

        public string? StopReason { get; set; }

        public List<RsmmcPilotMrnGateCheckResponse> Gates { get; set; } = [];

        public List<RsmmcPilotMrnReconcileItemResponse> Items { get; set; } = [];
    }

    public class RsmmcPilotMrnGateCheckResponse
    {
        public string Code { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Expected { get; set; } = string.Empty;

        public string Actual { get; set; } = string.Empty;

        public bool Passed { get; set; }
    }

    public class RsmmcPilotMrnReconcileItemResponse
    {
        public string LegacyPid { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public string PatientCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string CurrentMedicalRecordNumber { get; set; } = string.Empty;

        public string FinalMedicalRecordNumber { get; set; } = string.Empty;

        public string? CurrentQrCodePath { get; set; }

        public string? PlannedQrCodePath { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? ConflictType { get; set; }

        public string? FailureStage { get; set; }

        public string? Message { get; set; }
    }
}
