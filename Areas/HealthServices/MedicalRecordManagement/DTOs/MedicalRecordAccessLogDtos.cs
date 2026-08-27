using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs
{
    /// <summary>
    /// Satu baris jejak akses sebagaimana ditampilkan pada layar tinjauan.
    ///
    /// PERHATIAN PRIVASI: <see cref="AccessReason"/> bertanda sensitif karena dapat mengungkap
    /// keadaan pasien, misalnya "konsultasi kejiwaan". Hak akses ke daftar ini tidak boleh
    /// diberikan seluas hak baca rekam medis.
    /// </summary>
    public class MedicalRecordAccessLogResponse
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? MedicalRecordNumber { get; set; }

        public Guid UserId { get; set; }

        /// <summary>
        /// Nama pengguna sebagaimana tercatat saat akses terjadi, bukan nama sekarang. Jejak
        /// harus terbaca apa adanya walaupun akun sudah berganti nama atau dihapus.
        /// </summary>
        public string UserDisplayNameSnapshot { get; set; } = string.Empty;

        public MedicalRecordAccessType AccessType { get; set; }
        public string AccessTypeName { get; set; } = string.Empty;

        public MedicalRecordAccessScope AccessScope { get; set; }
        public string AccessScopeName { get; set; } = string.Empty;

        public Guid? AccessPurposeId { get; set; }
        public string? AccessPurposeName { get; set; }

        /// <summary>SENSITIF. Tidak boleh masuk ke logger.</summary>
        public string? AccessReason { get; set; }

        public bool HasActiveEncounter { get; set; }

        public bool IsFlaggedForReview { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public string? ReviewedByName { get; set; }
        public string? ReviewNote { get; set; }

        public DateTime AccessedAt { get; set; }
        public string? IpAddress { get; set; }
        public string? ClientInfo { get; set; }
    }

    public class MarkAccessReviewedRequest
    {
        /// <summary>
        /// Catatan hasil tinjauan. Wajib diisi — tinjauan tanpa catatan tidak dapat dibedakan
        /// dari sekadar membersihkan antrean.
        /// </summary>
        [Required(ErrorMessage = "Catatan tinjauan wajib diisi.")]
        [MaxLength(500, ErrorMessage = "Catatan tinjauan terlalu panjang. Batasnya 500 huruf.")]
        public string ReviewNote { get; set; } = string.Empty;
    }

    /// <summary>
    /// Rekap jumlah akses per jenis dalam satu rentang waktu.
    ///
    /// Angka inilah yang memberi tahu apakah aturan akses bekerja sebagaimana dimaksud: bila
    /// hampir seluruh akses berjenis beralasan, berarti definisi pasien rawatan terlalu sempit
    /// dan menghambat pelayanan.
    /// </summary>
    public class MedicalRecordAccessSummaryResponse
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int TotalAkses { get; set; }
        public int AksesRawatan { get; set; }
        public int AksesBeralasan { get; set; }
        public int AksesCatatanPribadi { get; set; }

        public int PerluDitinjau { get; set; }
        public int SudahDitinjau { get; set; }
        public int BelumDitinjau { get; set; }

        public int JumlahPenggunaBerbeda { get; set; }
        public int JumlahPasienBerbeda { get; set; }
    }
}
