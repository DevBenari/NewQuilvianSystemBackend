using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models
{
    /// <summary>
    /// Otorisasi darurat yang menggantikan gerbang pemberian normal untuk satu kantong
    /// terhadap satu pasien (BD-DOM-08).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Entity ini <b>tidak pernah diterbitkan sistem</b> dan tidak pernah menjadi keputusan
    /// otomatis (INV-BD-032). Ia merekam keputusan manusia berwenang beserta seluruh
    /// kelengkapannya: alasan terkendali, keterangan kondisi kedaruratan, pelaku, peran yang
    /// dipakainya, waktu, pasien, dan kantong.
    /// </para>
    ///
    /// <para>
    /// <see cref="BypassScope"/> wajib menyatakan gerbang mana yang dilewati (INV-BD-030).
    /// Penanda darurat tidak pernah berdiri tanpa keterangan itu, karena penanda tanpa keterangan
    /// berhenti bermakna bagi pembaca rekam berikutnya.
    /// </para>
    ///
    /// <para>
    /// Barisnya melekat permanen pada kantong. Pemberian bersifat terminal, sehingga otorisasi
    /// ini tidak punya jalur pembatalan; koreksi pencatatan pemberian adalah proses tersendiri
    /// milik BE-BD-010.
    /// </para>
    /// </remarks>
    [Table("BbkEmergencyAuthorization", Schema = "public")]
    public class BbkEmergencyAuthorization : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Kantong yang diberikan lewat jalur darurat.</summary>
        [Required]
        public Guid BloodUnitId { get; set; }

        [ForeignKey(nameof(BloodUnitId))]
        public BbkBloodUnit? BloodUnit { get; set; }

        /// <summary>
        /// Pasien tujuan. Diturunkan dari alokasi aktif kantong, tidak diterima dari client.
        /// </summary>
        [Required]
        public Guid PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public MstPatient? Patient { get; set; }

        /// <summary>Penerbit otorisasi. Diambil dari akun yang login, tidak pernah dari body.</summary>
        [Required]
        public Guid AuthorizedByUserId { get; set; }

        /// <summary>Waktu otorisasi diterbitkan. Diambil dari jam server.</summary>
        [Required]
        public DateTime AuthorizedAt { get; set; }

        /// <summary>
        /// Kode alasan terkendali berkategori <c>Emergency</c> pada <c>MstBloodBankReason</c>.
        /// Tidak pernah diketik bebas (INV-BD-016).
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string ReasonCode { get; set; } = string.Empty;

        /// <summary>
        /// Salinan teks alasan saat otorisasi diterbitkan, supaya riwayat lama tidak berubah
        /// makna ketika master disunting.
        /// </summary>
        [MaxLength(500)]
        public string? ReasonNote { get; set; }

        /// <summary>Gerbang yang dilewati otorisasi ini (INV-BD-030).</summary>
        [Required]
        public BbkEmergencyBypassScope BypassScope { get; set; }

        /// <summary>Peran yang dipakai penerbit (DEC-BD-040, INV-BD-032).</summary>
        [Required]
        public BbkEmergencyAuthorizerRole AuthorizerRole { get; set; }

        /// <summary>
        /// Keterangan keadaan klinis yang membuat pemberian harus dilakukan sekarang.
        /// Wajib, dan berbeda dari <see cref="ReasonCode"/> yang terkendali — ini uraian keadaan,
        /// bukan kategori.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string EmergencyConditionNote { get; set; } = string.Empty;
    }
}
