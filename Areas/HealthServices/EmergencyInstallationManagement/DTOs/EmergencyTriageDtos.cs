using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs
{
    public class EmergencyTriageResponse
    {
        public Guid Id { get; set; }
        public Guid EmergencyVisitId { get; set; }
        public Guid TriageLevelId { get; set; }

        /// <summary>
        /// Nama dan warna level dibawa langsung oleh balasan.
        ///
        /// Tanpa ini setiap pemakai harus menggabungkan sendiri dengan master level, dan
        /// layar yang membuka riwayat tanpa memuat master lebih dulu akan menampilkan
        /// level sebagai identifier yang tidak berarti bagi perawat.
        /// </summary>
        public string? TriageLevelName { get; set; }
        public string? TriageLevelColorName { get; set; }
        public string? TriageLevelColorHex { get; set; }
        public Guid? PatientVitalSignId { get; set; }
        public int Sequence { get; set; }
        public bool IsRetriage { get; set; }
        public Guid? PreviousTriageId { get; set; }
        public EmergencyTriageSystem TriageSystem { get; set; }
        public EmergencyTriageStatus TriageStatus { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? MaxWaitingMinutesSnapshot { get; set; }
        public DateTime? ResponseDueAt { get; set; }
        public bool ImmediateCareAllowed { get; set; }
        public string? TriageReason { get; set; }
        public string? AirwaySummary { get; set; }
        public string? BreathingSummary { get; set; }
        public string? CirculationSummary { get; set; }
        public string? DisabilitySummary { get; set; }
        public string? ExposureSummary { get; set; }
        public string? RedFlagSummary { get; set; }
        public Guid PerformedByUserId { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateEmergencyTriageRequest
    {
        [Required]
        public Guid EmergencyVisitId { get; set; }

        [Required]
        public Guid TriageLevelId { get; set; }

        public Guid? PatientVitalSignId { get; set; }

        /// <summary>
        /// USANG sejak BE-IGD-047 - tetap diterima, tetapi diabaikan sepenuhnya.
        /// </summary>
        /// <remarks>
        /// Nilai bawaan <c>1</c> pada properti ini adalah akar masalah 23505 pada
        /// <c>IX_EmgTriage_EmergencyVisitId_Sequence</c>: pemanggil yang tidak mengirim
        /// <c>sequence</c> tetap membuat controller membaca <c>1</c>, sehingga cabang
        /// <c>request.Sequence &gt; 0</c> selalu benar dan penghitungan nomor urut di server
        /// tidak pernah berjalan. Penilaian kedua pada satu kunjungan karena itu selalu
        /// menabrak penilaian pertama. Nomor urut kini sepenuhnya ditetapkan server, sesuai
        /// acceptance BE-IGD-004.
        /// </remarks>
        public int Sequence { get; set; }

        /// <summary>
        /// USANG sejak BE-IGD-047 - diabaikan; ditetapkan server dari ada tidaknya penilaian
        /// terdahulu pada kunjungan yang sama.
        /// </summary>
        public bool IsRetriage { get; set; }

        /// <summary>
        /// USANG sejak BE-IGD-047 - diabaikan; ditetapkan server ke penilaian terakhir yang
        /// masih berlaku pada kunjungan yang sama.
        /// </summary>
        public Guid? PreviousTriageId { get; set; }

        public EmergencyTriageSystem TriageSystem { get; set; } = EmergencyTriageSystem.ATS;

        public EmergencyTriageStatus TriageStatus { get; set; } = EmergencyTriageStatus.Draft;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public int? MaxWaitingMinutesSnapshot { get; set; }

        public DateTime? ResponseDueAt { get; set; }

        public bool ImmediateCareAllowed { get; set; }

        [MaxLength(1000)]
        public string? TriageReason { get; set; }

        [MaxLength(1000)]
        public string? AirwaySummary { get; set; }

        [MaxLength(1000)]
        public string? BreathingSummary { get; set; }

        [MaxLength(1000)]
        public string? CirculationSummary { get; set; }

        [MaxLength(1000)]
        public string? DisabilitySummary { get; set; }

        [MaxLength(1000)]
        public string? ExposureSummary { get; set; }

        [MaxLength(1000)]
        public string? RedFlagSummary { get; set; }

        public Guid PerformedByUserId { get; set; }

        public Guid? ReviewedByUserId { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

    }

    public class UpdateEmergencyTriageRequest : CreateEmergencyTriageRequest
    {
    }

    /// <summary>
    /// Permintaan penilaian ulang. Nomor urut, penanda retriage, penunjuk penilaian sebelumnya,
    /// target waktu, dan batas waktu respons ditetapkan server, bukan dikirim pemanggil.
    /// </summary>
    public class RetriageEmergencyTriageRequest
    {
        [Required]
        public Guid TriageLevelId { get; set; }

        public Guid? PatientVitalSignId { get; set; }

        /// <summary>
        /// Waktu penilaian ulang dimulai. Bila tidak diisi, memakai waktu server saat ini.
        /// </summary>
        public DateTime? StartedAt { get; set; }

        [MaxLength(1000)]
        public string? TriageReason { get; set; }

        [MaxLength(1000)]
        public string? AirwaySummary { get; set; }

        [MaxLength(1000)]
        public string? BreathingSummary { get; set; }

        [MaxLength(1000)]
        public string? CirculationSummary { get; set; }

        [MaxLength(1000)]
        public string? DisabilitySummary { get; set; }

        [MaxLength(1000)]
        public string? ExposureSummary { get; set; }

        [MaxLength(1000)]
        public string? RedFlagSummary { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class UpdateEmergencyTriageTriageStatusRequest
    {
        [Required]
        public EmergencyTriageStatus TriageStatus { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Satu baris pada daftar pasien yang melampaui batas waktu respons.
    ///
    /// Delapan kolom ringkasan klinis yang bertanda sensitif pada data dictionary
    /// (TriageReason, AirwaySummary, BreathingSummary, CirculationSummary,
    /// DisabilitySummary, ExposureSummary, RedFlagSummary, dan Notes) sengaja
    /// tidak ikut. Daftar ini dipakai untuk menentukan siapa yang harus didahulukan,
    /// bukan untuk membaca isi penilaiannya.
    /// </summary>
    public class EmergencyTriageSlaBreachResponse
    {
        public Guid Id { get; set; }

        public Guid EmergencyVisitId { get; set; }

        public Guid? PatientId { get; set; }

        /// <summary>
        /// Nama pasien. Untuk pasien yang belum teridentifikasi, diisi alias sementara
        /// kunjungan supaya baris ini tetap dapat ditindaklanjuti petugas.
        /// </summary>
        public string PatientName { get; set; } = string.Empty;

        public string? MedicalRecordNumber { get; set; }

        public bool IsUnknownPatient { get; set; }

        public Guid ServiceUnitId { get; set; }

        public string? ServiceUnitName { get; set; }

        public Guid TriageLevelId { get; set; }

        public string? TriageLevelName { get; set; }

        public string? TriageLevelColorName { get; set; }

        /// <summary>
        /// Warna master apa adanya. Tanpa ini layar terpaksa menebak warna dari namanya
        /// lewat tabel padanan yang ditanam di kode.
        /// </summary>
        public string? TriageLevelColorHex { get; set; }

        public int? TriageLevel { get; set; }

        public int Sequence { get; set; }

        public EmergencyTriageStatus TriageStatus { get; set; }

        public DateTime StartedAt { get; set; }

        public int? MaxWaitingMinutesSnapshot { get; set; }

        public DateTime? ResponseDueAt { get; set; }

        public DateTime? SlaBreachedAt { get; set; }

        /// <summary>
        /// Lama keterlambatan dalam menit, dihitung server saat daftar diambil.
        /// </summary>
        public int OverdueMinutes { get; set; }
    }
}
