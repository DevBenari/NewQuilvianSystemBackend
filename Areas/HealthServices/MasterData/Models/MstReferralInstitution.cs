using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
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

        /// <summary>
        /// Bermitra dengan rumah sakit (<c>RJ-DOC-DEC-073</c>). Bila benar, layar pendaftaran
        /// rujukan menampilkan alert "Fasilitas Perujuk Bermitra dengan Rumah Sakit".
        /// </summary>
        /// <remarks>
        /// Sejak <c>DEC-FRJ-001</c> setiap master baru selalu mitra. Nilai <c>false</c> hanya
        /// dimiliki data lama yang dipertahankan untuk histori. Penanda ini <b>bukan</b> status
        /// kerja sama: kelayakan dihitung dari perjanjian yang berlaku (lihat
        /// <see cref="Agreements"/>).
        /// </remarks>
        public bool IsPartner { get; set; } = false;

        // ---------- DEC-FRJ-001: identitas, lokasi, dan kontak mitra ----------
        // Kolom-kolom di bawah nullable di database supaya baris lama tetap sah; service
        // mewajibkannya setiap kali master dibuat atau diubah.

        public ReferralInstitutionType InstitutionType { get; set; } = ReferralInstitutionType.Unknown;

        public Guid? ProvinceId { get; set; }

        /// <summary>Kabupaten/Kota; wajib berada di <see cref="ProvinceId"/>.</summary>
        public Guid? CityId { get; set; }

        [MaxLength(150)]
        public string? Email { get; set; }

        /// <summary>Nama penanggung jawab (PIC) di pihak mitra.</summary>
        [MaxLength(150)]
        public string? PicName { get; set; }

        /// <summary>Kode faskes dari sistem eksternal, misalnya kode faskes BPJS.</summary>
        [MaxLength(50)]
        public string? ExternalFacilityCode { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Token konkurensi; diganti setiap kali master atau perjanjiannya berubah, sehingga
        /// dua penyuntingan atau dua perpanjangan serentak tidak saling menimpa.
        /// </summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public MstProvince? Province { get; set; }

        public MstCity? City { get; set; }

        /// <summary>Dokter yang berpraktik pada instansi ini.</summary>
        public ICollection<MstReferralDoctor> Doctors { get; set; } = new List<MstReferralDoctor>();

        /// <summary>Histori perjanjian kerja sama (<c>DEC-FRJ-001</c>).</summary>
        public ICollection<MstReferralInstitutionAgreement> Agreements { get; set; } =
            new List<MstReferralInstitutionAgreement>();
    }
}
