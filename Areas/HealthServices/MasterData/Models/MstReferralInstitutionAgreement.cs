using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Perjanjian kerja sama satu fasilitas perujuk mitra (<c>DEC-FRJ-001</c>).
    /// </summary>
    /// <remarks>
    /// <b>Kenapa tabel terpisah.</b> Satu fasilitas dapat diperpanjang berkali-kali, dan status
    /// "Kontrak Berakhir" maupun kelayakan pada tanggal layanan tertentu hanya dapat dihitung
    /// bila setiap periode perjanjian tetap tersimpan. Menimpa tanggal pada master akan
    /// menghapus histori itu.
    ///
    /// <b>Contoh.</b> Klinik A punya PKS 2025-01-01 s.d. 2025-12-31 lalu diperpanjang
    /// 2026-01-01 s.d. 2026-12-31: dua baris di sini, tidak boleh saling tumpang tindih.
    /// Tanggal disimpan sebagai tanggal kalender WIB (inklusif di kedua ujung).
    /// </remarks>
    [Table("MstReferralInstitutionAgreement", Schema = "public")]
    public class MstReferralInstitutionAgreement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ReferralInstitutionId { get; set; }

        /// <summary>Nomor perjanjian kerja sama (PKS). Unik per fasilitas.</summary>
        [Required]
        [MaxLength(100)]
        public string AgreementNumber { get; set; } = string.Empty;

        /// <summary>Hari pertama perjanjian berlaku (inklusif).</summary>
        public DateOnly StartDate { get; set; }

        /// <summary>Hari terakhir perjanjian berlaku (inklusif).</summary>
        public DateOnly EndDate { get; set; }

        public MstReferralInstitution? ReferralInstitution { get; set; }
    }
}
