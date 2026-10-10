using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>
    /// Parameter penyaringan daftar antrean transfer pasien IGD dan Poliklinik menuju Rawat Inap (BE-RWI-206).
    /// </summary>
    public class AdmissionTransferQuery
    {
        /// <summary>Jenis asal transfer: "Emergency" (IGD) atau "Outpatient" (Poliklinik). Null untuk semua.</summary>
        public string? SourceType { get; set; }

        /// <summary>Status disposisi transfer. Bawaan Confirmed (menunggu kamar).</summary>
        public EmergencyDispositionStatus? Status { get; set; }

        /// <summary>Hanya tampilkan pasien yang waktu tunggunya melebihi 60 menit.</summary>
        public bool? OverdueOnly { get; set; }

        /// <summary>Pencarian berdasarkan Nama Pasien, Nomor Rekam Medis, atau Nomor Kunjungan.</summary>
        [MaxLength(100)]
        public string? Search { get; set; }

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Satu item antrean transfer pasien IGD / Rawat Jalan yang membutuhkan kamar rawat inap (BE-RWI-206).
    /// </summary>
    public class InpatientAdmissionTransferItemResponse
    {
        /// <summary>Identifier disposisi transfer IGD atau rujukan rawat jalan.</summary>
        public Guid TransferId { get; set; }

        /// <summary>"Emergency" atau "Outpatient".</summary>
        public string SourceType { get; set; } = "Emergency";

        /// <summary>ID kunjungan IGD asal.</summary>
        public Guid EmergencyVisitId { get; set; }

        /// <summary>Nomor kunjungan IGD yang terbaca manusia (misal EMG-20261010-0012).</summary>
        public string EmergencyVisitNumber { get; set; } = string.Empty;

        /// <summary>ID encounter kunjungan asal jika tersedia.</summary>
        public Guid? SourceEncounterId { get; set; }

        public Guid PatientId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public string MedicalRecordNumber { get; set; } = string.Empty;

        public string? Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? NationalId { get; set; }

        public string? PhoneNumber { get; set; }

        public Guid? DecidedByDoctorId { get; set; }

        public string? DecidedByDoctorName { get; set; }

        public string? DispositionReason { get; set; }

        public string? PatientCondition { get; set; }

        public string? FollowUpInstruction { get; set; }

        public Guid? RecommendedServiceUnitId { get; set; }

        public string? RecommendedServiceUnitName { get; set; }

        public DateTime DecidedAt { get; set; }

        /// <summary>Lama waktu menunggu dalam menit sejak diputuskan dokter.</summary>
        public int WaitingMinutes { get; set; }

        /// <summary>Benar bila menunggu > 60 menit (RWI-DEC-276).</summary>
        public bool IsOverdue { get; set; }

        public string DispositionStatus { get; set; } = "Confirmed";
    }

    /// <summary>
    /// Permintaan pembukaan episode rawat inap dari pasien transfer IGD / Poliklinik (BE-RWI-207).
    /// </summary>
    public class OpenAdmissionFromTransferRequest
    {
        [Required(ErrorMessage = "TransferDispositionId wajib diisi.")]
        public Guid TransferDispositionId { get; set; }

        [Required(ErrorMessage = "SourceType wajib diisi.")]
        public string SourceType { get; set; } = "Emergency";

        [Required(ErrorMessage = "PatientId wajib diisi.")]
        public Guid PatientId { get; set; }

        [Required(ErrorMessage = "ServiceUnitId wajib diisi.")]
        public Guid ServiceUnitId { get; set; }

        [Required(ErrorMessage = "PatientClassId wajib diisi.")]
        public Guid PatientClassId { get; set; }

        [Required(ErrorMessage = "DoctorId wajib diisi.")]
        public Guid DoctorId { get; set; }

        [Required(ErrorMessage = "BedId wajib diisi.")]
        public Guid BedId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(100)]
        public string? PaymentScheme { get; set; }

        public decimal? DepositAmount { get; set; }
    }

    /// <summary>
    /// Hasil balasan pembukaan episode rawat inap dari transfer IGD (BE-RWI-207).
    /// </summary>
    public class OpenAdmissionFromTransferResponse
    {
        public Guid EpisodeId { get; set; }

        public string EpisodeNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public string MedicalRecordNumber { get; set; } = string.Empty;

        public Guid BedId { get; set; }

        public string BedNumber { get; set; } = string.Empty;

        public string RoomName { get; set; } = string.Empty;

        public string TransferStatus { get; set; } = "Executed";

        public DateTime ExecutedAt { get; set; }
    }
}
