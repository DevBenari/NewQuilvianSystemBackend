using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models
{
    /// <summary>
    /// Satu kedatangan fisik kantong dari PMI untuk satu permintaan. Entity di dalam
    /// <c>BD-AGG-02</c> / <c>BD-DOM-04</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Stok bertambah hanya lewat konsep ini</b> (<c>DEC-BD-002</c>, <c>VAL-BD-015</c>). Tidak ada
    /// jalan lain untuk melahirkan <see cref="BbkBloodUnit"/>: kantong lahir dari penerimaan, dan
    /// selalu membawa rujukan penerimaan pelahirnya.
    /// </para>
    ///
    /// <para>
    /// <b>Penerimaan tidak pernah ditolak karena kelebihan</b> (<c>DEC-BD-025</c>). Kantong yang
    /// melebihi jumlah diminta tetap dicatat di sini, ditandai <c>IsExcess</c> pada kantongnya,
    /// dan sisa permintaan berhenti di nol.
    /// </para>
    /// </remarks>
    [Table("BbkBloodUnitReceipt", Schema = "public")]
    public class BbkBloodUnitReceipt : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProviderRequestId { get; set; }

        [ForeignKey(nameof(ProviderRequestId))]
        public BbkProviderRequest? ProviderRequest { get; set; }

        /// <summary>Jumlah kantong pada kedatangan ini, termasuk yang berlebih.</summary>
        public int ReceivedQuantity { get; set; }

        /// <summary>Waktu kantong diterima fisik.</summary>
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Petugas penerima. Diturunkan dari pengguna terautentikasi, tidak pernah dari isian
        /// permintaan.
        /// </summary>
        [Required]
        public Guid ReceivedByUserId { get; set; }

        /// <summary>Urutan kedatangan di dalam permintaannya, dimulai dari 1.</summary>
        public int Sequence { get; set; }

        public ICollection<BbkBloodUnit> Units { get; set; } = new List<BbkBloodUnit>();
    }
}
