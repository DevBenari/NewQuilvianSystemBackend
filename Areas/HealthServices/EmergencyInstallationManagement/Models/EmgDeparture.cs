using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models
{
    /// <summary>
    /// Catatan kepergian pasien dari IGD: ke mana ia pergi, kapan berangkat, kapan tiba, dan
    /// bagaimana serah terimanya.
    /// </summary>
    /// <remarks>
    /// <c>BE-IGD-031</c> dan <c>BE-IGD-032</c>. Dulu bernama <c>TrxEmergencyTransfer</c>.
    ///
    /// <para>
    /// <b>Mengapa namanya berganti.</b> <c>IGD-DEC-069</c> mengubah artinya dari "perpindahan
    /// beserta tempat tidur" menjadi "catatan kepergian pasien dari IGD" — urusan tempat tidur
    /// pindah ke Rawat Inap. Nama lama menyesatkan sesudah itu: ia menjanjikan penempatan
    /// tempat tidur yang tabel ini tidak lagi urus.
    /// </para>
    ///
    /// <para>
    /// <b>Dua kolom status, bukan satu.</b> <c>IGD-DEC-070</c> dan <c>IGD-DEC-090</c> memecah
    /// <c>TransferStatus</c> tunggal menjadi keadaan fisik pasien dan keadaan dokumen serah
    /// terima. Keduanya bergerak sendiri-sendiri: pasien dapat sudah tiba sementara dokumennya
    /// belum ditinjau, dan itu <b>keadaan sah</b>, bukan penyimpangan.
    /// </para>
    ///
    /// <para>
    /// Kedua kolom status adalah <b>turunan</b> dari kejadian terakhir yang berlaku pada
    /// <see cref="EmgDepartureEvent"/>, bukan sumber kebenaran tandingan. Setiap
    /// penulisan kejadian memperbarui kolom status dalam transaksi yang sama.
    /// </para>
    /// </remarks>
    [Table("EmgDeparture", Schema = "public")]
    public class EmgDeparture : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmergencyVisitId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DepartureNumber { get; set; } = string.Empty;

        public Guid? FromServiceUnitId { get; set; }

        [Required]
        public Guid ToServiceUnitId { get; set; }

        /// <summary>Keadaan fisik pasien. Lihat <see cref="EmergencyPhysicalStatus"/>.</summary>
        public EmergencyPhysicalStatus PhysicalStatus { get; set; } = EmergencyPhysicalStatus.Prepared;

        /// <summary>Keadaan dokumen serah terima. Lihat <see cref="EmergencyHandoverStatus"/>.</summary>
        public EmergencyHandoverStatus HandoverStatus { get; set; } = EmergencyHandoverStatus.Submitted;

        [Required]
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid RequestedByUserId { get; set; }

        public DateTime? DepartedAt { get; set; }

        public DateTime? ArrivedAt { get; set; }

        public Guid? SendingNurseUserId { get; set; }

        public Guid? ReceivingNurseUserId { get; set; }

        [MaxLength(1000)]
        public string? DepartureReason { get; set; }

        // =========================================================================
        // SBAR - IGD-DEC-079. Empat bagian wajib terisi atau ditandai tidak dapat
        // diisi beserta alasannya. Menyimpannya sebagai empat kolom, bukan satu
        // ringkasan bebas, supaya penolakan dapat menyebut bagian mana yang kurang.
        // =========================================================================

        [MaxLength(2000)]
        public string? SituationSummary { get; set; }

        [MaxLength(2000)]
        public string? BackgroundSummary { get; set; }

        [MaxLength(2000)]
        public string? AssessmentSummary { get; set; }

        [MaxLength(2000)]
        public string? RecommendationSummary { get; set; }

        /// <summary>
        /// Alasan ketika satu atau lebih bagian SBAR ditandai tidak dapat diisi.
        /// <c>IGD-DEC-056</c>, <c>IGD-DEC-079</c>.
        /// </summary>
        [MaxLength(1000)]
        public string? UnavailableSectionReason { get; set; }

        /// <summary>
        /// Bagian SBAR yang ditandai tidak dapat diisi, dipisah koma. Kosong berarti seluruh
        /// bagian terisi.
        /// </summary>
        [MaxLength(250)]
        public string? UnavailableSections { get; set; }

        // =========================================================================
        // Salinan keadaan klinis saat serah terima. Disimpan sebagai salinan, bukan
        // rujukan, supaya dokumen serah terima tetap terbaca apa adanya meski data
        // sumbernya berubah kemudian.
        // =========================================================================

        [MaxLength(1000)]
        public string? AllergySnapshot { get; set; }

        public Guid? LastVitalSignId { get; set; }

        [MaxLength(150)]
        public string? TriageLevelSnapshot { get; set; }

        /// <summary>
        /// Alasan penolakan dokumen serah terima oleh unit penerima. Wajib menyebut bagian
        /// mana yang dianggap kurang — validation bagian 4 aturan 4.
        /// </summary>
        [MaxLength(1000)]
        public string? HandoverRejectionReason { get; set; }

        /// <summary>Alasan pembatalan kepergian. Wajib diisi — <c>IGD-DEC-069</c>.</summary>
        [MaxLength(1000)]
        public string? CancellationReason { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public EmgVisit? EmergencyVisit { get; set; }

        public MstServiceUnit? FromServiceUnit { get; set; }

        public MstServiceUnit? ToServiceUnit { get; set; }

        public ICollection<EmgDepartureEvent> Events { get; set; }
            = new List<EmgDepartureEvent>();

        public ICollection<EmgHandoverOrderItem> OrderItems { get; set; }
            = new List<EmgHandoverOrderItem>();
    }
}
