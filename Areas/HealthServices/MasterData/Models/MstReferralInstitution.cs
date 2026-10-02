using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Instansi perujuk — klinik, puskesmas, atau rumah sakit yang mengirim pasien ke sini
    /// (<c>LAB-DEC-035</c>, <c>BE-EXT-02</c>).
    ///
    /// <b>Kenapa ini data induk, bukan teks bebas pada kunjungan.</b> Sebagai teks bebas,
    /// "Klinik Sehat Sentosa", "Kl. Sehat Sentosa", dan "sehat sentosa" terhitung tiga instansi
    /// berbeda. Laporan asal rujukan kemudian tidak pernah dapat dipercaya, dan tidak ada cara
    /// memperbaikinya selain menebak mana yang sebenarnya sama.
    ///
    /// Data induk ini <b>global</b>: Laboratorium, Rawat Jalan, dan IGD sama-sama menerima
    /// pasien rujukan, sehingga pemiliknya Master Data dan bukan salah satu modul pemakainya.
    /// </summary>
    [Table("MstReferralInstitution", Schema = "public")]
    public class MstReferralInstitution : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Kode instansi perujuk. Unik di antara baris yang belum dihapus.</summary>
        [Required]
        [MaxLength(50)]
        public string InstitutionCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string InstitutionName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(50)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Penanda aktif. Instansi yang tidak lagi bekerja sama <b>dinonaktifkan</b>, bukan
        /// dihapus — kunjungan lama yang menunjuk ke sini harus tetap dapat dibaca.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Dokter yang berpraktik pada instansi ini.</summary>
        public ICollection<MstReferralDoctor> Doctors { get; set; } = new List<MstReferralDoctor>();
    }
}
