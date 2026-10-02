using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models
{
    /// <summary>
    /// Sampel yang diambil untuk satu pemeriksaan golongan darah. Entity di dalam
    /// <see cref="BbkBloodGroupExam"/> (<c>BD-DOM-10</c>).
    /// </summary>
    /// <remarks>
    /// <b>Ini sampel Bank Darah, bukan sampel Laboratorium</b> (<c>DEC-BD-018</c>). Perbedaannya
    /// bukan soal penamaan: sampel Laboratorium menimbulkan tagihan Laboratorium, sedangkan
    /// sampel ini tidak. Menyambungkannya ke <c>LabSpecimen</c> akan memunculkan tagihan
    /// pemeriksaan yang tidak pernah dipesan siapa pun.
    ///
    /// Yang disimpan di sini sengaja sempit — identifier, pengambil, waktu — dan bukan
    /// manajemen sampel serba guna.
    /// </remarks>
    [Table("BbkBloodGroupSample", Schema = "public")]
    public class BbkBloodGroupSample : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BloodGroupExamId { get; set; }

        [ForeignKey(nameof(BloodGroupExamId))]
        public BbkBloodGroupExam? BloodGroupExam { get; set; }

        /// <summary>
        /// Identifier sampel internal yang tertulis pada tabung, unik di seluruh tabel.
        /// </summary>
        /// <remarks>
        /// <b>Nilai ini ditulis petugas, bukan diterbitkan server</b> — keputusan pemilik modul
        /// 7 September 2026 saat <c>BE-BD-005</c> dikerjakan. Polanya sama dengan
        /// <c>PmiBagNumber</c> yang datang dari PMI (<c>ASM-BD-003</c>) dan
        /// <c>MstBloodStorageLocation.StorageLocationCode</c> yang ditulis BDRS: ketiganya
        /// penanda benda fisik yang sudah ada di tangan petugas, bukan nomor dokumen yang
        /// diterbitkan sistem.
        ///
        /// <b>Konsekuensi yang disengaja:</b> tidak satu pun field pada slice ini memerlukan
        /// provider number-series, sehingga <c>BE-BD-005</c> tidak tertahan gerbang <c>G4</c>
        /// dan <c>QBE-CODE-002/003</c> tidak tersentuh — tidak ada nomor yang dialokasikan
        /// di mana pun.
        ///
        /// <b>Selisih kontrak yang tercatat:</b> <c>03-domain-architecture.md</c> baris 283
        /// menulis <i>"Identifier sampel terbitan sistem"</i>. Rumusan itu digantikan keputusan
        /// pemilik di atas dan dilaporkan sebagai delta pada laporan task <c>BE-BD-005</c>.
        /// </remarks>
        [Required]
        [MaxLength(50)]
        public string SampleIdentifier { get; set; } = string.Empty;

        /// <summary>Petugas yang mengambil sampel.</summary>
        /// <remarks>
        /// Diturunkan dari pengguna terautentikasi, tidak pernah diterima dari isian pemanggil.
        /// </remarks>
        [Required]
        public Guid TakenByUserId { get; set; }

        [Required]
        public DateTime TakenAt { get; set; }
    }
}
