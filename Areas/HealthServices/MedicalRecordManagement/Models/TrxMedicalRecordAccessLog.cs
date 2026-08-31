using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models
{
    /// <summary>
    /// Satu baris untuk setiap pembukaan berkas rekam medis: siapa, pasien siapa, kapan, dan
    /// untuk keperluan apa.
    ///
    /// TIGA ATURAN YANG BERBEDA DARI TABEL LAIN:
    ///
    /// <list type="number">
    /// <item><b>Baris tidak pernah dihapus</b>, termasuk lewat penandaan IsDelete. Jejak yang
    /// dapat dihapus bukan jejak. Penghapusan hanya lewat pengarsipan resmi sesuai masa simpan
    /// 25 tahun (RM-DEC-024).</item>
    /// <item><b>Tidak memuat isi klinis apa pun.</b> Tabel ini menjawab "siapa membuka apa",
    /// bukan "apa isinya". Menyimpan cuplikan isi akan menjadikannya salinan rekam medis kedua,
    /// yang justru memperluas permukaan kebocoran.</item>
    /// <item><b>Kunci utamanya gabungan</b> antara Id dan AccessedAt. Ini disiapkan untuk
    /// pembagian tabel per tahun — PostgreSQL mensyaratkan kolom pembagi ikut menjadi bagian
    /// kunci utama. Menyiapkannya sejak awal jauh lebih murah daripada mengubah kunci utama
    /// pada tabel yang sudah berisi jutaan baris.</item>
    /// </list>
    /// </summary>
    [Table("TrxMedicalRecordAccessLog", Schema = "public")]
    public class TrxMedicalRecordAccessLog : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Salinan nama pengguna saat itu.
        ///
        /// Ini satu-satunya tempat pada modul rekam medis yang menyalin data milik modul lain,
        /// dan alasannya khusus: jejak akses harus tetap terbaca puluhan tahun kemudian,
        /// sementara akun pengguna bisa berganti nama atau dihapus. Menyimpan UserId saja
        /// membuat jejak lama menjadi tidak terbaca.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string UserDisplayNameSnapshot { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? UserRoleSnapshot { get; set; }

        public MedicalRecordAccessType AccessType { get; set; }

        public MedicalRecordAccessScope AccessScope { get; set; }

        /// <summary>Wajib terisi bila <see cref="AccessType"/> bernilai akses beralasan.</summary>
        public Guid? AccessPurposeId { get; set; }

        /// <summary>
        /// Alasan bebas. SENSITIF — dapat mengungkap keadaan pasien, misalnya "konsultasi
        /// kejiwaan". Tidak boleh masuk ke logger.
        /// </summary>
        [MaxLength(500)]
        public string? AccessReason { get; set; }

        /// <summary>
        /// Hasil penilaian pada saat akses terjadi. Disimpan supaya keputusan sistem dapat
        /// ditelusuri kemudian, walaupun keadaan kunjungan sudah berubah.
        /// </summary>
        public bool HasActiveEncounter { get; set; }

        public bool IsFlaggedForReview { get; set; } = false;

        public DateTime? ReviewedAt { get; set; }

        public Guid? ReviewedByUserId { get; set; }

        [MaxLength(500)]
        public string? ReviewNote { get; set; }

        /// <summary>
        /// Waktu pembukaan. Menjadi kolom pembagi tabel per tahun, sehingga ikut menjadi bagian
        /// kunci utama.
        /// </summary>
        public DateTime AccessedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(64)]
        public string? IpAddress { get; set; }

        [MaxLength(250)]
        public string? ClientInfo { get; set; }

        [MaxLength(250)]
        public string? RequestPath { get; set; }

        public MstPatient? Patient { get; set; }

        public MstMedicalRecordAccessPurpose? AccessPurpose { get; set; }
    }
}
