using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    /// <summary>
    /// Saringan Daftar Pasien Rawat Jalan (kontrak <c>RJ-DOC-ENCLIST-001@1.0.0</c>).
    /// </summary>
    public class OutpatientEncounterListQuery
    {
        /// <summary><c>today</c> (bawaan), <c>active</c>, atau <c>range</c>.</summary>
        public string? Mode { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        /// <summary>Hanya kunjungan yang menghalangi pendaftaran dan bertanggal sebelum hari ini.</summary>
        public bool HangingOnly { get; set; }

        public List<int>? EncounterStatus { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? DoctorId { get; set; }

        [MaxLength(100)]
        public string? Search { get; set; }

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;
    }

    public class OutpatientEncounterListItemResponse
    {
        public Guid Id { get; set; }
        public string EncounterNumber { get; set; } = string.Empty;
        public DateTime EncounterDate { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public Guid ClinicId { get; set; }
        public string ClinicName { get; set; } = string.Empty;
        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public string PaymentLabel { get; set; } = string.Empty;
        public int EncounterStatus { get; set; }
        public string EncounterStatusName { get; set; } = string.Empty;
        public bool IsCancelled { get; set; }
        public bool IsHanging { get; set; }
        public bool HasActiveConsultation { get; set; }
        public bool CanCancel { get; set; }
        public string? CancelBlockedReason { get; set; }
    }

    public class OutpatientEncounterSummaryResponse
    {
        /// <summary>Status Draft sampai Menunggu Dokter, belum batal.</summary>
        public int Waiting { get; set; }

        /// <summary>Sedang Konsultasi, belum batal.</summary>
        public int InConsultation { get; set; }

        /// <summary>Konsultasi Selesai dan Proses Billing, belum batal.</summary>
        public int ReadyForBilling { get; set; }

        /// <summary>Batal, Selesai, atau Tidak Hadir.</summary>
        public int Closed { get; set; }

        /// <summary>Menghalangi pendaftaran dan bertanggal sebelum hari ini; selalu lintas tanggal.</summary>
        public int Hanging { get; set; }
    }

    public class OutpatientEncounterScopeResponse
    {
        public bool CanReadAll { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class OutpatientEncounterOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class OutpatientEncounterFilterMetadataResponse
    {
        public OutpatientEncounterScopeResponse Scope { get; set; } = new();
        public List<OutpatientEncounterOptionResponse> StatusOptions { get; set; } = new();
        public List<OutpatientEncounterOptionResponse> ClinicOptions { get; set; } = new();
        public List<OutpatientEncounterOptionResponse> DoctorOptions { get; set; } = new();
    }

    /// <summary>
    /// Isian pembatalan. Panjang alasan diperiksa service (RJDP-VAL-002) supaya pesannya sama
    /// dengan validation matrix, bukan pesan bawaan model binding.
    /// </summary>
    public class OutpatientEncounterCancelRequest
    {
        public string? CancelReason { get; set; }
    }

    public class OutpatientEncounterCancelResponse
    {
        public Guid Id { get; set; }
        public DateTime CancelledAt { get; set; }
        public int CancelledQueueCount { get; set; }
    }
}
