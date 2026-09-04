using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    /// <summary>
    /// Permintaan mencatat satu kejadian visite dokter — <c>BE-RWI-048</c>,
    /// <c>api-contract.md</c> bagian 4.
    /// </summary>
    /// <remarks>
    /// Yang tidak diterima dari layar: pencatat kejadian. Ia selalu diambil dari pengguna yang
    /// sedang masuk, supaya tidak ada yang dapat mencatat visite atas nama orang lain lewat
    /// payload.
    /// </remarks>
    public class CreatePhysicianVisitRequest
    {
        /// <summary>Kunjungan yang menaungi kejadian.</summary>
        [Required(ErrorMessage = "Kunjungan wajib diisi.")]
        public Guid EncounterId { get; set; }

        /// <summary>
        /// Perawatan rawat inap yang menaungi kejadian. Boleh kosong; bila terisi tetapi tidak
        /// cocok dengan perawatan milik kunjungan, permintaan ditolak — <c>VAL-DOK-26</c>.
        /// </summary>
        public Guid? InpEpisodeId { get; set; }

        /// <summary>Pasien yang didatangi. Penjaga salah pasien.</summary>
        [Required(ErrorMessage = "Pasien wajib diisi.")]
        public Guid PatientId { get; set; }

        /// <summary>
        /// Dokter yang datang. Boleh kosong; bila kosong, dokter yang sedang masuk yang dipakai.
        /// </summary>
        public Guid? DoctorId { get; set; }

        /// <summary>
        /// <b>Waktu kedatangan</b>, bukan waktu pencatatan. Boleh mundur, tidak boleh maju —
        /// <c>VAL-DOK-16</c>.
        /// </summary>
        public DateTime? VisitDateTime { get; set; }

        /// <summary>Peran dokter pada kejadian ini.</summary>
        public PhysicianVisitRole VisitRole { get; set; } = PhysicianVisitRole.Dpjp;

        /// <summary>Tautan opsional ke catatan dokter yang lahir dari kunjungan ini.</summary>
        public Guid? ConsultationId { get; set; }

        /// <summary>Tautan opsional ke catatan pada lembar terpadu.</summary>
        public Guid? ProgressNoteId { get; set; }

        /// <summary>Tautan opsional ke tindakan yang dikerjakan saat kunjungan ini.</summary>
        public Guid? PatientProcedureId { get; set; }

        /// <summary>Catatan singkat dokter.</summary>
        [MaxLength(1000, ErrorMessage = "Catatan visite terlalu panjang. Batasnya 1000 huruf.")]
        public string? Note { get; set; }

        /// <summary>
        /// Kunci permintaan. <b>Wajib terisi</b> — <c>VAL-DOK-27</c>. Boleh dikirim lewat header
        /// <c>Idempotency-Key</c>; bila keduanya terisi, isi badan permintaan yang dipakai.
        /// </summary>
        [MaxLength(100, ErrorMessage = "Kunci permintaan terlalu panjang. Batasnya 100 huruf.")]
        public string? IdempotencyKey { get; set; }

        /// <summary>
        /// Kejadian yang digantikan, bila pencatatan ini adalah pencatatan ulang setelah
        /// pembatalan — <c>state-transition-matrix.md</c> bagian 5.2.
        /// </summary>
        public Guid? CorrectsVisitId { get; set; }
    }

    /// <summary>
    /// Permintaan membatalkan satu kejadian visite — <c>BE-RWI-049</c>.
    /// </summary>
    public class CancelPhysicianVisitRequest
    {
        /// <summary>
        /// Alasan pembatalan. Wajib diisi — <c>VAL-DOK-28</c>. Tanpa alasan, riwayat hanya
        /// menunjukkan bahwa sesuatu dibatalkan tanpa menjelaskan mengapa.
        /// </summary>
        [Required(ErrorMessage = "Alasan pembatalan wajib diisi.")]
        [MaxLength(500, ErrorMessage = "Alasan pembatalan terlalu panjang. Batasnya 500 huruf.")]
        public string CancelReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Permintaan menautkan dokumen pada kejadian visite yang sudah tercatat.
    /// </summary>
    /// <remarks>
    /// Tidak memuat waktu maupun peran, dan itu disengaja. Penyuntingan keduanya dilarang
    /// <c>RWI-DEC-085</c>; koreksi dilakukan dengan membatalkan lalu mencatat ulang.
    /// </remarks>
    public class UpdatePhysicianVisitLinksRequest
    {
        public Guid? ConsultationId { get; set; }

        public Guid? ProgressNoteId { get; set; }

        public Guid? PatientProcedureId { get; set; }
    }

    /// <summary>
    /// Satu kejadian visite beserta seluruh isinya.
    /// </summary>
    public class PhysicianVisitResponse
    {
        public Guid Id { get; set; }
        public string PhysicianVisitNumber { get; set; } = string.Empty;
        public Guid EncounterId { get; set; }
        public Guid? InpEpisodeId { get; set; }
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public DateTime VisitDateTime { get; set; }
        public PhysicianVisitRole VisitRole { get; set; }
        public string VisitRoleName { get; set; } = string.Empty;
        public PhysicianVisitStatus VisitStatus { get; set; }
        public string VisitStatusName { get; set; } = string.Empty;
        public Guid? ConsultationId { get; set; }
        public Guid? ProgressNoteId { get; set; }
        public Guid? PatientProcedureId { get; set; }
        public string? Note { get; set; }
        public Guid RecordedByUserId { get; set; }
        public string? RecordedByName { get; set; }

        /// <summary>Waktu pencatatan, terpisah dari waktu kedatangan.</summary>
        public DateTime? RecordedAt { get; set; }

        public DateTime? CancelledAt { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public string? CancelledByName { get; set; }
        public string? CancelReason { get; set; }
        public Guid? CorrectsVisitId { get; set; }

        /// <summary>
        /// Aksi yang sedang boleh dijalankan atas kejadian ini.
        /// </summary>
        /// <remarks>
        /// Bantuan tampilan, <b>bukan</b> pengaman. Setiap endpoint aksi tetap memeriksa ulang
        /// kelayakan status dan kewenangan aktor di backend.
        /// </remarks>
        public List<string> AvailableActions { get; set; } = new();
    }

    /// <summary>
    /// Satu baris riwayat visite.
    /// </summary>
    public class PhysicianVisitListItemResponse
    {
        public Guid Id { get; set; }
        public string PhysicianVisitNumber { get; set; } = string.Empty;
        public Guid? InpEpisodeId { get; set; }
        public Guid EncounterId { get; set; }
        public Guid DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public DateTime VisitDateTime { get; set; }
        public PhysicianVisitRole VisitRole { get; set; }
        public string VisitRoleName { get; set; } = string.Empty;
        public PhysicianVisitStatus VisitStatus { get; set; }
        public string VisitStatusName { get; set; } = string.Empty;
        public Guid RecordedByUserId { get; set; }
        public string? RecordedByName { get; set; }
        public DateTime? RecordedAt { get; set; }
        public Guid? ConsultationId { get; set; }
        public Guid? ProgressNoteId { get; set; }
        public Guid? PatientProcedureId { get; set; }
        public bool HasLinkedDocument { get; set; }
        public string? CancelReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        public Guid? CorrectsVisitId { get; set; }
    }

    /// <summary>
    /// Ringkasan hitungan visite.
    /// </summary>
    /// <remarks>
    /// <c>INV-DOK-07</c>. Angka di sini diturunkan dari <b>kejadian visite</b>, bukan dari
    /// catatan yang ditulis dokter. Tiga catatan tanpa satu pun kejadian menghasilkan nol, dan
    /// dua kunjungan nyata pada hari yang sama menghasilkan dua.
    /// </remarks>
    public class PhysicianVisitSummaryResponse
    {
        /// <summary>Jumlah kejadian yang berlaku; kejadian batal tidak ikut dihitung.</summary>
        public int RecordedCount { get; set; }

        /// <summary>Jumlah kejadian yang dibatalkan; tetap tersimpan dan tetap terbaca.</summary>
        public int CancelledCount { get; set; }

        /// <summary>Jumlah seluruh baris kejadian, berlaku maupun batal.</summary>
        public int TotalCount { get; set; }

        /// <summary>Jumlah dokter berbeda yang tercatat mendatangi pasien.</summary>
        public int DistinctDoctorCount { get; set; }

        /// <summary>Waktu kedatangan terakhir yang berlaku.</summary>
        public DateTime? LastVisitDateTime { get; set; }
    }

    public class PhysicianVisitEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PhysicianVisitSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PhysicianVisitDefaultFilterResponse
    {
        public Guid? InpEpisodeId { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? DoctorId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public bool IncludeCancelled { get; set; } = true;
        public string SortBy { get; set; } = "visitDateTime";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class PhysicianVisitFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public PhysicianVisitDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PhysicianVisitSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<PhysicianVisitEnumOptionResponse> VisitRoleOptions { get; set; } = new();
        public List<PhysicianVisitEnumOptionResponse> VisitStatusOptions { get; set; } = new();
    }
}
