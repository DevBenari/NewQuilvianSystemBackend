using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>
    /// Parameter penyaringan daftar antrean transfer pasien IGD dan Poliklinik menuju Rawat Inap (BE-RWI-206).
    /// </summary>
    public class AdmissionTransferQuery
    {
        /// <summary>Jenis asal transfer: "Emergency" (IGD) atau "Outpatient" (Poliklinik). Null untuk semua.</summary>
        [RegularExpression("^(Emergency|Outpatient)$")]
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

        /// <summary>ID kunjungan IGD asal jika transfer bersumber dari IGD.</summary>
        public Guid? EmergencyVisitId { get; set; }

        /// <summary>Nomor kunjungan IGD yang terbaca manusia (misal EMG-20261010-0012).</summary>
        public string EmergencyVisitNumber { get; set; } = string.Empty;

        /// <summary>ID encounter kunjungan asal jika tersedia.</summary>
        public Guid? SourceEncounterId { get; set; }
        public string? SourceEncounterNumber { get; set; }
        public Guid? ResultEpisodeId { get; set; }

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

        /// <summary>Benar bila menunggu > 60 menit (EPS-006/WAIT-60).</summary>
        public bool IsOverdue { get; set; }

        public string DispositionStatus { get; set; } = "Confirmed";

        /// <summary>
        /// Menandakan apakah permintaan transfer berstatus gawat darurat / cito dari IGD (EmgAdmissionRequest.IsCito == true).
        /// </summary>
        public bool IsCito { get; set; }

        /// <summary>
        /// Menandakan apakah pasien memiliki membership aktif yang berhak atas antrean prioritas (PriorityQueue == true).
        /// </summary>
        public bool IsPriorityMember { get; set; }

        /// <summary>
        /// Nama tier keanggotaan aktif pasien (misal "Platinum", "Gold", "Silver"). Bernilai null jika reguler/non-member.
        /// </summary>
        public string? MemberTierName { get; set; }

        /// <summary>
        /// Level prioritas tier keanggotaan (angka lebih kecil = prioritas lebih tinggi). Bernilai null jika reguler/non-member.
        /// </summary>
        public int? MemberPriorityLevel { get; set; }

        // PLAN-REPAIR-EPS-041 FIX-01: kebutuhan klinis dari permintaan rawat inap IGD yang sudah
        // ditandatangani. Sumber Poliklinik tidak memuatnya, sehingga selalu false/null di sana.
        public bool NeedsIsolation { get; set; }
        public bool NeedsIntensiveCare { get; set; }
        public bool NeedsOxygenSupport { get; set; }
        public bool NeedsSpecialMonitoring { get; set; }
        public int? EstimatedLengthOfStayDays { get; set; }

        /// <summary>DPJP rawat inap yang diusulkan IGD, berbeda dari dokter jaga yang memutuskan disposisi.</summary>
        public Guid? ProposedInpatientDoctorId { get; set; }
        public string? ProposedInpatientDoctorName { get; set; }

        /// <summary>Diagnosis surat pengantar rawat inap Poliklinik (FIX-01); null untuk sumber IGD.</summary>
        public string? Diagnosis { get; set; }

        /// <summary>Kelas kamar yang diminta pada surat pengantar Poliklinik (FIX-01); null untuk sumber IGD.</summary>
        public string? RequestedRoomClass { get; set; }
    }

    /// <summary>
    /// Permintaan pembukaan episode rawat inap dari pasien transfer IGD / Poliklinik (BE-RWI-207).
    /// </summary>
    public class OpenAdmissionFromTransferRequest
    {
        public long ExpectedBedVersion { get; set; }
        [Required(ErrorMessage = "TransferDispositionId wajib diisi.")]
        public Guid TransferDispositionId { get; set; }

        [Required(ErrorMessage = "SourceType wajib diisi.")]
        [RegularExpression("^(Emergency|Outpatient)$")]
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
        public EncounterPaymentType PaymentType { get; set; } = EncounterPaymentType.Cash;
        public Guid? PaymentMethodId { get; set; }
        public Guid? PatientInsuranceId { get; set; }
        public Guid? PatientCompanyGuarantorId { get; set; }
        public Guid? DepositPaymentMethodId { get; set; }
        public bool RequiresIsolation { get; set; }
        [MaxLength(500)] public string? IsolationNote { get; set; }
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

    /// <summary>
    /// Permintaan penolakan rujukan transfer rawat inap oleh petugas admisi (BE-IGD-077).
    /// </summary>
    public class RejectAdmissionTransferRequest
    {
        /// <summary>Alasan penolakan rujukan transfer rawat inap (wajib diisi, maksimal 500 karakter).</summary>
        [Required(ErrorMessage = "Alasan penolakan admisi wajib diisi.")]
        [MaxLength(500, ErrorMessage = "Alasan penolakan admisi paling panjang 500 karakter.")]
        public string Reason { get; set; } = string.Empty;
    }
}
