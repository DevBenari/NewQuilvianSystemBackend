using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models
{
    /// <summary>
    /// Satu kantong darah fisik yang sudah diterima dari PMI. Aggregate root
    /// <c>BD-AGG-03</c> / <c>BD-DOM-05</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Nomor kantong tidak diterbitkan server.</b> <see cref="PmiBagNumber"/> berasal dari PMI
    /// (<c>ASM-BD-003</c>), diinput petugas saat penerimaan, dan dijaga unik oleh index database.
    /// Nomor ini <b>sensitif</b>: tidak boleh masuk payload log dan tidak boleh dipakai sebagai alat
    /// otorisasi.
    /// </para>
    ///
    /// <para>
    /// <b>Asal kantong tidak pernah putus.</b> <see cref="ProviderRequestId"/> dan
    /// <see cref="ReceiptId"/> terisi sejak kantong lahir dan tidak pernah disunting, sehingga
    /// kantong yang kelak dialihkan, dikembalikan, atau dinyatakan tidak layak tetap dapat
    /// ditelusuri ke permintaan dan penerimaan asalnya.
    /// </para>
    ///
    /// <para>
    /// <b>Satu kolom kamus data sengaja belum ada.</b> <c>CompatibilityEvidenceIdUsed</c> menunjuk
    /// <c>BbkCompatibilityEvidence</c> yang lahir pada <c>BE-BD-007</c>; kolom tanpa FK yang
    /// menggantung lebih buruk daripada kolom yang ditambahkan bersama tabel tujuannya.
    /// <see cref="CurrentPlacementId"/> lahir pada <c>BE-BD-015</c> bersama tabel penempatannya.
    /// </para>
    /// </remarks>
    [Table("BbkBloodUnit", Schema = "public")]
    public class BbkBloodUnit : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nomor kantong dari PMI. Unik. <b>Sensitif.</b></summary>
        [Required]
        [MaxLength(50)]
        public string PmiBagNumber { get; set; } = string.Empty;

        /// <summary>Permintaan asal — tidak pernah putus.</summary>
        [Required]
        public Guid ProviderRequestId { get; set; }

        [ForeignKey(nameof(ProviderRequestId))]
        public BbkProviderRequest? ProviderRequest { get; set; }

        /// <summary>Penerimaan yang melahirkan kantong ini.</summary>
        [Required]
        public Guid ReceiptId { get; set; }

        [ForeignKey(nameof(ReceiptId))]
        public BbkBloodUnitReceipt? Receipt { get; set; }

        /// <summary>Komponen darah kantong ini, dari katalog.</summary>
        [Required]
        public Guid BloodComponentId { get; set; }

        [ForeignKey(nameof(BloodComponentId))]
        public MstBloodComponent? BloodComponent { get; set; }

        /// <summary>
        /// Kantong datang melebihi jumlah yang diminta untuk komponennya (<c>DEC-BD-025</c>).
        /// Ditetapkan saat penerimaan dan tidak pernah disunting sesudahnya.
        /// </summary>
        public bool IsExcess { get; set; }

        /// <summary>Status kantong. Lahir <see cref="BbkBloodUnitStatus.Received"/> (<c>DEC-BD-036</c>).</summary>
        public BbkBloodUnitStatus UnitStatus { get; set; } = BbkBloodUnitStatus.Received;

        /// <summary>
        /// Penunjuk ke penempatan yang sedang berlaku (<c>ARCH-BD-POS-05</c>). Kosong selama
        /// <see cref="BbkBloodUnitStatus.Received"/>.
        /// </summary>
        /// <remarks>
        /// <b>Tidak pernah disunting sendiri.</b> Nilai ini hanya berpindah bersama penambahan
        /// <see cref="BbkBloodUnitPlacement"/>, dalam transaksi yang sama dan dikawal
        /// <see cref="Version"/>. Karena itu tidak ada kolom lokasi langsung pada kantong: kolom
        /// seperti itu dapat disunting tanpa menambah riwayat, sehingga kantong dan riwayatnya bisa
        /// berselisih tanpa ketahuan. Keaktifan lokasi juga tidak disalin ke sini — ia dibaca dari
        /// master saat gerbang dinilai (<c>ARCH-BD-POS-06</c>).
        /// </remarks>
        public Guid? CurrentPlacementId { get; set; }

        public BbkBloodUnitPlacement? CurrentPlacement { get; set; }

        /// <summary>Pasien penerima. Terisi saat kantong diberikan. <b>Sensitif.</b></summary>
        public Guid? IssuedToPatientId { get; set; }

        [ForeignKey(nameof(IssuedToPatientId))]
        public MstPatient? IssuedToPatient { get; set; }

        /// <summary>Waktu pemberian. Terminal dan tidak dapat dibalik.</summary>
        public DateTime? IssuedAt { get; set; }

        /// <summary>Pelaku pemberian.</summary>
        public Guid? IssuedByUserId { get; set; }

        /// <summary>Penanda permanen pemberian lewat jalur darurat.</summary>
        public bool IssuedViaEmergency { get; set; }

        /// <summary>
        /// Token pencegah tulis-bersamaan. Menjaga penempatan tunggal yang berlaku
        /// (<c>BE-BD-015</c>) dan kelak alokasi tunggal aktif.
        /// </summary>
        public int Version { get; set; }
    }
}
