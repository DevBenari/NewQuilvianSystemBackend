using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Satu jenis pemeriksaan yang dipesan (<c>LAB-DEC-024</c>, <c>LAB-DEC-026</c>, BR-20).
    ///
    /// <b>Mengapa entity ini ada.</b> Sebelum pemisahan ini, satu baris <see cref="LabSpecimen"/>
    /// merangkap dua peran sekaligus: wadah fisik yang diambil dari pasien, sekaligus jenis
    /// pemeriksaan yang ditagihkan. Kenyataannya keduanya tidak berpasangan satu-satu — satu
    /// tabung darah ungu menopang hemoglobin, leukosit, dan trombosit sekaligus. Selama
    /// keduanya menempel, satu tabung terpaksa dicatat tiga kali, dan pasien menerima tiga
    /// barcode untuk satu kali tusukan jarum (<c>AC-35</c>).
    ///
    /// Entity ini memisahkan peran kedua. <b>Inilah satuan yang ditagihkan</b>, dan kelak
    /// satuan yang memiliki hasil.
    ///
    /// <b>Yang melekat di sini, bukan di pesanan.</b> Kesegeraan dan penanda duplo tinggal pada
    /// baris pemeriksaan (<c>LAB-DEC-026</c>). Satu pesanan boleh memuat pemeriksaan cito dan
    /// biasa sekaligus; memaksanya ke tingkat pesanan akan membuat seluruh isi pesanan ikut
    /// diperlakukan cito dan menenggelamkan yang benar-benar mendesak (<c>AC-40</c>).
    ///
    /// <b>Yang sengaja tidak ada.</b> Tidak satu pun kolom hasil, dan tidak satu pun kolom
    /// finansial. Kolom hasil menunggu slice hasil yang masih tertahan <c>LAB-SIGN-001</c>.
    /// Akibat finansial sepenuhnya milik Billing sesuai <c>RJ-BIL-GATE-DEC-003</c>; salinan
    /// harga di bawah adalah bukti nilai saat kejadian, <b>bukan</b> tagihan.
    /// </summary>
    public class LabExamination : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Pesanan induk yang memuat pemeriksaan ini.</summary>
        [Required]
        public Guid LabOrderId { get; set; }

        /// <summary>
        /// Wadah fisik yang menopang pemeriksaan ini. Satu wadah menopang satu atau lebih
        /// pemeriksaan; kelayakan periksa melekat pada wadah, bukan di sini.
        /// </summary>
        [Required]
        public Guid SpecimenId { get; set; }

        /// <summary>Jenis pemeriksaan yang dipesan. Wajib berpenanda <c>IsLaboratory</c>.</summary>
        [Required]
        public Guid ProcedureId { get; set; }

        /// <summary>Salinan kode jenis pemeriksaan saat kejadian.</summary>
        public string? ProcedureCodeSnapshot { get; set; }

        /// <summary>Salinan nama jenis pemeriksaan saat kejadian.</summary>
        public string? ProcedureNameSnapshot { get; set; }

        /// <summary>
        /// Tarif yang berlaku saat kejadian. Data induknya milik Master Data, dan ditunjuk
        /// tanpa foreign key — mengikuti pola yang sudah dipakai <see cref="LabSpecimen"/>.
        /// </summary>
        public Guid? TariffId { get; set; }

        /// <summary>Salinan kode tarif saat kejadian.</summary>
        public string? TariffCodeSnapshot { get; set; }

        /// <summary>
        /// Salinan harga satuan saat kejadian. <b>Bukan tagihan.</b> Nilainya disimpan supaya
        /// muatan fakta yang dikirim ke Billing dapat direproduksi persis ketika pengiriman
        /// diulang; keputusan menagih tetap milik Billing.
        /// </summary>
        public decimal? UnitPriceSnapshot { get; set; }

        /// <summary>
        /// Keadaan pemeriksaan. Sebagian besar mengikuti wadah penopangnya — lihat
        /// <see cref="LabExaminationStatus"/>.
        /// </summary>
        public LabExaminationStatus ExaminationStatus { get; set; } = LabExaminationStatus.Ordered;

        /// <summary>
        /// Waktu pemeriksaan ini menjadi sah ditagihkan, yaitu saat wadah penopangnya
        /// dinyatakan layak. Kosong selama belum layak.
        /// </summary>
        public DateTime? ChargeEligibleAt { get; set; }

        /// <summary>
        /// Tingkat kesegeraan pemeriksaan ini — biasa atau cito (<c>LAB-DEC-026</c>).
        /// </summary>
        public LabExaminationUrgency Urgency { get; set; } = LabExaminationUrgency.Routine;

        /// <summary>Waktu pemeriksaan ditandai cito atau dikembalikan menjadi biasa.</summary>
        public DateTime? UrgencyMarkedAt { get; set; }

        /// <summary>Dokter yang menandai kesegeraannya.</summary>
        public Guid? UrgencyMarkedByUserId { get; set; }

        /// <summary>
        /// Pemeriksaan dikerjakan ganda (<c>LAB-DEC-026</c>). Dipakai ketika hasil pertama perlu
        /// dikonfirmasi pada pengerjaan yang sama.
        /// </summary>
        public bool IsDuplo { get; set; }

        /// <summary>
        /// Token konkurensi. Menjaga dua permintaan yang datang bersamaan tidak saling menimpa
        /// diam-diam, mengikuti pola <see cref="LabSpecimen.Version"/>.
        /// </summary>
        public int Version { get; set; }

        public LabOrder? LabOrder { get; set; }

        public LabSpecimen? Specimen { get; set; }

        public MstProcedure? Procedure { get; set; }
    }
}
