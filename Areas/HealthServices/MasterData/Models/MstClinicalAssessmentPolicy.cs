using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Kebijakan batas waktu penyelesaian pengkajian, per jenis pengkajian dan per jenis
    /// pelayanan, <b>berversi</b> lewat periode berlaku.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>BE-RWI-055</c>, <c>FR-KEP-010</c>, <c>FR-KEP-011</c>, PRD 16.2 aturan 11. Angka batas
    /// waktu klinis adalah <b>konfigurasi</b>, bukan ketetapan yang ditanam di kode:
    /// <c>RWI-RULE-021</c> belum final karena pemilik klinisnya belum ditunjuk, dan justru itu
    /// alasan tabel ini ada. Clinical governance mengisi angkanya sendiri lewat layar master,
    /// tanpa satu baris kode pun berubah.
    /// </para>
    /// <para>
    /// <b>Kenapa berversi, bukan ditimpa.</b> Bayangkan batas pengkajian awal semula 24 jam.
    /// Pengkajian Tn. Budi selesai pada jam ke-20 dan dinilai tepat waktu. Bila kelak batasnya
    /// diperketat menjadi 8 jam dan penilaian memakai angka terbaru, pengkajian Tn. Budi
    /// tiba-tiba terbaca terlambat - padahal perawatnya tidak melanggar apa pun. Karena itu
    /// setiap kebijakan punya <see cref="EffectiveFrom"/> dan <see cref="EffectiveTo"/>, dan
    /// pengkajian menyimpan penunjuk ke kebijakan yang berlaku <b>saat ia dibuat</b>.
    /// </para>
    /// <para>
    /// <b>Master kosong bukan kesalahan.</b> Selama tabel ini kosong, tidak satu pun pengkajian
    /// memperoleh tenggat dan tidak satu pun dinyatakan terlambat - <c>VAL-KEP-17</c>.
    /// Pencatatan berjalan penuh. Itu perilaku yang dirancang, bukan celah.
    /// </para>
    /// </remarks>
    [Table("MstClinicalAssessmentPolicy", Schema = "public")]
    public class MstClinicalAssessmentPolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Kode kebijakan, unik di seluruh tabel. Contoh <c>KEP-AWAL-RI-2026</c>.</summary>
        [Required]
        [MaxLength(50)]
        public string PolicyCode { get; set; } = string.Empty;

        /// <summary>Nama kebijakan yang dibaca manusia pada layar master.</summary>
        [Required]
        [MaxLength(150)]
        public string PolicyName { get; set; } = string.Empty;

        /// <summary>Jenis pengkajian yang diatur kebijakan ini.</summary>
        public PatientAssessmentType AssessmentType { get; set; } = PatientAssessmentType.Initial;

        /// <summary>
        /// Jenis pelayanan yang diatur. Kosong berarti kebijakan ini berlaku untuk seluruh jenis
        /// pelayanan.
        /// </summary>
        /// <remarks>
        /// Kamus data menuliskannya sebagai <c>ServiceUnitTypeId uuid?</c>, yaitu penunjuk ke
        /// tabel jenis pelayanan. Source tidak memiliki tabel itu: jenis pelayanan adalah enum
        /// <see cref="ServiceUnitType"/> yang melekat pada <c>MstServiceUnit</c>. Bentuk yang
        /// dipakai di sini mengikuti source, dan selisihnya dicatat pada laporan task.
        /// </remarks>
        public ServiceUnitType? ServiceUnitType { get; set; }

        /// <summary>
        /// Batas waktu penyelesaian, dihitung dalam menit sejak pengkajian dibuat.
        /// </summary>
        /// <remarks>
        /// Menit, bukan jam, supaya kebijakan yang lebih ketat daripada satu jam tetap dapat
        /// dinyatakan tanpa mengubah bentuk kolom.
        /// </remarks>
        public int DueWithinMinutes { get; set; }

        /// <summary>Awal masa berlaku kebijakan.</summary>
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Akhir masa berlaku. Kosong berarti kebijakan ini masih berlaku sampai digantikan.
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
