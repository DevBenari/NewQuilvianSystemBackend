using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>
    /// Bentuk permintaan membuka admisi rawat inap. Satu pasien terdaftar dijadikan pasien
    /// rawat inap, dan episodenya lahir berstatus <c>Draft</c> dengan nomor yang terbaca
    /// manusia.
    /// </summary>
    /// <remarks>
    /// <b>Kenapa <c>EncounterId</c> boleh kosong.</b> Ada dua jalur masuk pada revisi ini,
    /// dan keduanya berakhir pada bentuk data yang sama:
    ///
    /// <list type="number">
    /// <item><description>
    /// Petugas menunjuk kunjungan bertipe rawat inap yang sudah ada. Kunjungan itu dipakai
    /// apa adanya sebagai jangkar episode.
    /// </description></item>
    /// <item><description>
    /// Petugas tidak menunjuk kunjungan apa pun — inilah jalur pasien datang langsung.
    /// Sistem membuat kunjungan bertipe rawat inap sendiri, di dalam proses admisi yang sama.
    /// Petugas tidak diminta mengisi form kedua (<c>RWI-AC-009</c>).
    /// </description></item>
    /// </list>
    ///
    /// Tidak ada jalur ketiga yang menghasilkan episode tanpa kunjungan. <c>RWI-AC-010</c>
    /// dijaga index unik <c>IX_InpEpisode_EncounterId</c> ditambah pemeriksaan service.
    /// </remarks>
    public class OpenAdmissionRequest
    {
        /// <summary>Pasien yang akan dirawat. Wajib, dan wajib benar-benar ada.</summary>
        public Guid PatientId { get; set; }

        /// <summary>
        /// Kunjungan yang dipakai sebagai jangkar episode. Dikosongkan untuk pasien yang
        /// datang langsung; sistem yang membuatkan kunjungannya.
        /// </summary>
        public Guid? EncounterId { get; set; }

        /// <summary>Unit layanan rawat inap tujuan. Wajib bertipe <c>Inpatient</c>.</summary>
        public Guid ServiceUnitId { get; set; }

        /// <summary>Kelas perawatan yang ditagihkan. Wajib berlaku untuk rawat inap.</summary>
        public Guid PatientClassId { get; set; }

        /// <summary>
        /// Dokter penanggung jawab pelayanan pertama. Wajib sejak detik pertama — episode
        /// tanpa DPJP tidak boleh pernah tersimpan (<c>INV-INP-03</c>).
        /// </summary>
        public Guid DoctorId { get; set; }

        /// <summary>Catatan admisi. Kolom sensitif; tidak boleh masuk payload logger.</summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Bentuk permintaan membetulkan isian admisi yang salah. Hanya berlaku selagi episode
    /// masih <c>Draft</c>.
    /// </summary>
    /// <remarks>
    /// <b>Yang sengaja tidak ada di sini.</b> Pasien dan kunjungan adalah jangkar episode dan
    /// tidak dapat ditukar lewat endpoint ini: menukarnya berarti episode yang lain, bukan
    /// koreksi isian. Salah pilih pasien dibetulkan dengan membatalkan admisi lalu membuka
    /// admisi baru — persis contoh pada <c>RWI-RULE-004</c>. Pengalihan DPJP juga tidak di
    /// sini; ia punya endpoint bermakna sendiri dan meninggalkan riwayat penugasan.
    /// </remarks>
    public class UpdateAdmissionRequest
    {
        /// <summary>Unit layanan rawat inap tujuan. Wajib bertipe <c>Inpatient</c>.</summary>
        public Guid ServiceUnitId { get; set; }

        /// <summary>Kelas perawatan yang ditagihkan. Wajib berlaku untuk rawat inap.</summary>
        public Guid PatientClassId { get; set; }

        /// <summary>Catatan admisi. Kolom sensitif; tidak boleh masuk payload logger.</summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Bentuk permintaan membatalkan admisi yang tidak jadi berjalan.
    /// </summary>
    /// <remarks>
    /// Alasan wajib diisi orang, dan alasan yang hanya berisi tanda baca ditolak
    /// (<c>RWI-AC-008</c>). Pembatalan menyimpan tiga hal sekaligus: siapa, kapan, dan apa
    /// alasannya. Barisnya tidak dihapus, hanya ditandai batal, sehingga tetap dapat
    /// ditelusuri saat diaudit.
    /// </remarks>
    public class CancelAdmissionRequest
    {
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Bentuk balasan satu episode rawat inap.
    /// </summary>
    /// <remarks>
    /// <b>Lokasi terkini sengaja belum ada di sini.</b> Lokasi pasien selalu dibaca dari
    /// <c>InpBedPlacement</c>, bukan dari kolom pada episode, dan penempatan baru lahir pada
    /// <c>BE-RWI-011</c>. Menambahkan kolom "lokasi terakhir" pada episode dilarang
    /// arsitektur walaupun query-nya lebih murah.
    /// </remarks>
    public class InpatientEpisodeDetailResponse
    {
        public Guid Id { get; set; }

        public string EpisodeNumber { get; set; } = string.Empty;

        public Guid EncounterId { get; set; }

        public string? EncounterNumber { get; set; }

        /// <summary>
        /// Benar bila kunjungan jangkarnya dibuat sendiri oleh proses admisi, yaitu jalur
        /// pasien datang langsung. Kunjungan itulah yang ikut dibatalkan ketika admisinya
        /// batal atau gugur.
        /// </summary>
        public bool IsEncounterCreatedByAdmission { get; set; }

        public Guid PatientId { get; set; }

        public string? PatientName { get; set; }

        public string? MedicalRecordNumber { get; set; }

        public Guid ServiceUnitId { get; set; }

        public string? ServiceUnitName { get; set; }

        public Guid PatientClassId { get; set; }

        public string? PatientClassName { get; set; }

        public int EpisodeStatus { get; set; }

        public string EpisodeStatusName { get; set; } = string.Empty;

        public DateTime? AdmittedAt { get; set; }

        public DateTime? DischargeDecidedAt { get; set; }

        public DateTime? PhysicallyLeftAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        public bool RequiresIsolation { get; set; }

        public string? CancelReason { get; set; }

        /// <summary>Catatan admisi. Kolom sensitif; tidak boleh masuk payload logger.</summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Keterangan kebutuhan isolasi. Kolom sensitif; hanya muncul pada detail, tidak
        /// pernah pada daftar mana pun, dan tidak boleh masuk payload logger.
        /// </summary>
        public string? IsolationNote { get; set; }

        public int? IsolationSource { get; set; }

        public string? IsolationSourceName { get; set; }

        public Guid? IsolationSetByUserId { get; set; }

        public Guid? IsolationSetByDoctorId { get; set; }

        public DateTime? IsolationSetAt { get; set; }

        public int DischargeType { get; set; }

        public string DischargeTypeName { get; set; } = string.Empty;

        public InpatientEpisodeActiveDoctorResponse? ActiveDoctor { get; set; }

        /// <summary>Perawat penanggung jawab yang sedang berlaku, bila memang sudah ada.</summary>
        public InpatientEpisodeActiveNurseResponse? ActiveNurse { get; set; }

        /// <summary>
        /// Lokasi pasien saat ini, selalu dibaca dari baris <c>InpBedPlacement</c> yang masih
        /// aktif — bukan dari kolom pada episode.
        /// </summary>
        public InpatientEpisodeCurrentLocationResponse? CurrentLocation { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        /// <summary>
        /// Peringatan yang tidak menghalangi permintaan. Dipakai untuk keadaan yang perlu
        /// diketahui petugas tetapi bukan alasan menolak, misalnya pasien yang sudah punya
        /// admisi lain yang masih disiapkan.
        /// </summary>
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Ringkasan DPJP yang sedang berlaku pada satu episode. Bentuk riwayat penugasan yang
    /// utuh adalah milik endpoint <c>/doctor-assignments</c> dan belum dibuka pada revisi ini.
    /// </summary>
    public class InpatientEpisodeActiveDoctorResponse
    {
        public Guid AssignmentId { get; set; }

        public Guid DoctorId { get; set; }

        public string? DoctorName { get; set; }

        public int SequenceNumber { get; set; }

        public DateTime StartDateTime { get; set; }
    }
}
