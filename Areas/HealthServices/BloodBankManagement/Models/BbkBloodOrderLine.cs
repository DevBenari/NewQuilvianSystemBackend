using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models
{
    /// <summary>
    /// Satu baris kebutuhan pada order darah: komponen apa, berapa kantong.
    /// Entity di dalam <c>BD-AGG-01</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Komponen selalu dipilih dari katalog</b> <c>MstBloodComponent</c>, tidak pernah
    /// diketik bebas (<c>VAL-BD-003</c>). Nama komponen yang diketik tangan membuat dua baris
    /// yang sebenarnya sama tampak berbeda, dan deteksi order ganda (<c>BD-XINV-01</c>)
    /// bekerja tepat pada kesamaan komponen itu.
    /// </para>
    ///
    /// <para>
    /// <b>Baris ini tidak menyimpan jumlah yang sudah diberikan.</b> Yang tersimpan hanya
    /// <see cref="RequestedQuantity"/>; sisi pemenuhannya dihitung dari pemberian nyata
    /// (<c>BD-DOM-17</c>).
    /// </para>
    /// </remarks>
    [Table("BbkBloodOrderLine", Schema = "public")]
    public class BbkBloodOrderLine : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BloodOrderId { get; set; }

        [ForeignKey(nameof(BloodOrderId))]
        public BbkBloodOrder? BloodOrder { get; set; }

        /// <summary>Komponen darah yang diminta, dari katalog Bank Darah.</summary>
        [Required]
        public Guid BloodComponentId { get; set; }

        [ForeignKey(nameof(BloodComponentId))]
        public MstBloodComponent? BloodComponent { get; set; }

        /// <summary>Jumlah kantong yang diminta. Wajib lebih dari nol (<c>VAL-BD-002</c>).</summary>
        public int RequestedQuantity { get; set; }

        /// <summary>Nomor urut baris di dalam ordernya, dimulai dari 1.</summary>
        /// <remarks>
        /// Urutan tampil milik satu order, bukan pengurutan global — karena itu ia hidup di
        /// baris order dan bukan sebagai <c>SortOrder</c> generik yang dipersistensi pada
        /// master (<c>QBE-CODE-004</c>).
        /// </remarks>
        public int Sequence { get; set; }
    }
}
