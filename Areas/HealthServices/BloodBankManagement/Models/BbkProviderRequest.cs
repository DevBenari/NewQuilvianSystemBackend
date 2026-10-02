using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models
{
    /// <summary>
    /// Catatan administratif permintaan pasokan darah ke PMI atas nama satu pasien.
    /// Aggregate root <c>BD-AGG-02</c> / <c>BD-DOM-03</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Pengirimannya manual di luar sistem</b> (<c>DEC-BD-002</c>). Quilvian mencatat bahwa
    /// permintaan dibuat dan kantong apa saja yang kemudian datang; stok bertambah <b>hanya</b>
    /// ketika kantong diterima fisik, tidak pernah ketika permintaan dibuat.
    /// </para>
    ///
    /// <para>
    /// <b>Jumlah yang diminta sengaja bukan kolom.</b> Kamus data kontrak <c>v4</c> tidak memberi
    /// kolom jumlah pada tabel ini: jumlahnya <b>diturunkan dari baris order asalnya</b>, per
    /// komponen. Sisa permintaan dihitung dari kantong yang benar-benar diterima, dengan batas
    /// bawah nol (<c>INV-BD-017</c>). Menyimpan jumlah atau sisa sebagai kolom berarti membuka
    /// dua catatan yang dapat berselisih (<c>QBE-ENT-003</c>).
    /// </para>
    ///
    /// <para>
    /// <b>Selalu atas nama satu pasien</b> (<c>DEC-BD-003</c>). <see cref="PatientId"/> disalin
    /// dari order asal saat permintaan dibuat, bukan diterima dari isian permintaan.
    /// </para>
    ///
    /// <para>
    /// <b><see cref="Version"/> menjaga sisa tidak pernah negatif</b> (<c>BD-XINV-03</c>). Setiap
    /// penerimaan menaikkan token ini, sehingga dua penerimaan yang hampir bersamaan tidak dapat
    /// sama-sama merasa masih di dalam kuota.
    /// </para>
    /// </remarks>
    [Table("BbkProviderRequest", Schema = "public")]
    public class BbkProviderRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Nomor bisnis permintaan, unik di seluruh tabel. Diterbitkan
        /// <c>NumberSeriesAllocator</c> pada deret <c>BBK_PROVIDER_REQUEST</c>.
        /// </summary>
        /// <remarks>
        /// Nomor yang sudah terbit <b>tidak pernah dipakai ulang</b>: deret boleh berlubang, satu
        /// nomor tidak boleh menempel pada dua catatan (<c>INV-PLT-001</c>, <c>INV-PLT-002</c>).
        /// </remarks>
        [Required]
        [MaxLength(30)]
        public string RequestNumber { get; set; } = string.Empty;

        /// <summary>Order darah asal. Jumlah yang diminta diturunkan dari baris order ini.</summary>
        [Required]
        public Guid BloodOrderId { get; set; }

        [ForeignKey(nameof(BloodOrderId))]
        public BbkBloodOrder? BloodOrder { get; set; }

        /// <summary>Pasien pemilik kebutuhan. Selalu satu pasien (<c>DEC-BD-003</c>).</summary>
        [Required]
        public Guid PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public MstPatient? Patient { get; set; }

        public BbkProviderRequestStatus RequestStatus { get; set; } = BbkProviderRequestStatus.Requested;

        /// <summary>
        /// Token pencegah tulis-bersamaan. Dipetakan sebagai concurrency token dan dinaikkan pada
        /// setiap penerimaan, pembatalan, dan penutupan (<c>BD-XINV-03</c>).
        /// </summary>
        public int Version { get; set; }

        public ICollection<BbkBloodUnitReceipt> Receipts { get; set; } = new List<BbkBloodUnitReceipt>();

        public ICollection<BbkBloodUnit> Units { get; set; } = new List<BbkBloodUnit>();
    }
}
