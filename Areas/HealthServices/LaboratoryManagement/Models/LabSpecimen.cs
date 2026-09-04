using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Satu sampel laboratorium, sekaligus satu komponen pemeriksaan yang dapat ditagih.
    ///
    /// Keputusan author <c>RJ-BIL-OQ-008</c> menetapkan kelayakan tagih dinilai per
    /// specimen/komponen pemeriksaan, bukan seluruh pesanan sekaligus. Karena itu tarif
    /// komponen melekat pada sampel melalui <see cref="ProcedureId"/>: satu pesanan berisi tiga
    /// pemeriksaan senilai Rp200.000, Rp150.000, dan Rp100.000 dapat menagih Rp350.000 ketika
    /// dua sampel dinyatakan layak dan satu ditolak.
    ///
    /// Sampel yang ditolak tidak pernah dihapus. Pengambilan ulang membuat baris baru yang
    /// menunjuk sampel sebelumnya melalui <see cref="SupersededSpecimenId"/>.
    /// </summary>
    public class LabSpecimen : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid LabOrderId { get; set; }

        /// <summary>
        /// Procedure komponen pemeriksaan yang diwakili sampel ini, sekaligus dasar tarifnya.
        /// </summary>
        [Required]
        public Guid ProcedureId { get; set; }

        /// <summary>
        /// Barcode operasional yang tidak bermakna, berbentuk <c>LSP-</c> diikuti 32 karakter
        /// heksadesimal. Dibuat server, unik, dan tidak pernah berubah setelah sampel dibuat.
        ///
        /// Nilai ini sengaja tidak memuat nama pasien, nomor rekam medis, NIK, tanggal lahir,
        /// nomor telepon, nomor encounter, penjamin, maupun diagnosis. Barcode juga bukan
        /// kredensial otorisasi dan bukan pengganti relasi database — penelusuran ke pesanan,
        /// encounter, dan pasien tetap melalui foreign key.
        /// </summary>
        [Required]
        public string SpecimenBarcode { get; set; } = string.Empty;

        /// <summary>Nomor urut sampel dalam satu pesanan, untuk keterbacaan operasional.</summary>
        public int SpecimenSequence { get; set; }

        /// <summary>Keterangan operasional bebas, misalnya "Darah vena" atau "Urin pagi".</summary>
        public string? SpecimenDescription { get; set; }

        /// <summary>
        /// Salinan identitas dan tarif komponen pemeriksaan pada saat sampel direncanakan.
        ///
        /// Disalin, bukan dibaca ulang saat penetapan layak, karena fakta yang dikirim ke
        /// Billing harus menghasilkan muatan yang sama persis bila dikirim ulang. Membaca tarif
        /// master pada setiap pengiriman akan mengubah sidik jari permintaan begitu master
        /// tarif berubah, dan pengiriman ulang berhenti bersifat idempotent.
        /// </summary>
        public string? ProcedureCodeSnapshot { get; set; }

        public string? ProcedureNameSnapshot { get; set; }

        public Guid? TariffId { get; set; }

        public string? TariffCodeSnapshot { get; set; }

        /// <summary>
        /// Tarif normal komponen pemeriksaan sebagai rujukan. Bukan nilai finansial otoritatif
        /// dan tidak memuat pembagian penjamin — pembagian itu milik Billing.
        /// </summary>
        public decimal? UnitPriceSnapshot { get; set; }

        public LabSpecimenStatus SpecimenStatus { get; set; } = LabSpecimenStatus.Planned;

        public LabSpecimenStatus? StatusBeforeHold { get; set; }

        public DateTime? CollectedAt { get; set; }

        public Guid? CollectedByUserId { get; set; }

        public DateTime? ReceivedAt { get; set; }

        public Guid? ReceivedByUserId { get; set; }

        /// <summary>Waktu keputusan layak atau tolak. Berbeda dari waktu penerimaan fisik.</summary>
        public DateTime? DecidedAt { get; set; }

        public Guid? DecidedByUserId { get; set; }

        /// <summary>
        /// Rujukan ke katalog alasan penolakan. Disimpan bersama
        /// <see cref="RejectionReasonCode"/> agar alasan yang kelak dinonaktifkan tidak merusak
        /// riwayat yang sudah tersimpan.
        /// </summary>
        public Guid? RejectionReasonId { get; set; }

        public string? RejectionReasonCode { get; set; }

        public string? RejectionNote { get; set; }

        /// <summary>Sampel yang digantikan oleh sampel ini pada pengambilan ulang.</summary>
        public Guid? SupersededSpecimenId { get; set; }

        public LabRecollectionCause? RecollectionCause { get; set; }

        public string? RecollectionReason { get; set; }

        public Guid? RecollectionAuthorizedByUserId { get; set; }

        public DateTime? RecollectionAuthorizedAt { get; set; }

        public int Version { get; set; }

        public LabOrder? LabOrder { get; set; }

        public MstProcedure? Procedure { get; set; }

        public MstLabRejectionReason? RejectionReason { get; set; }

        public LabSpecimen? SupersededSpecimen { get; set; }

        /// <summary>
        /// Pemeriksaan yang ditopang wadah ini. Satu wadah menopang satu atau lebih pemeriksaan
        /// (<c>LAB-DEC-024</c>, <c>AC-35</c>) — satu tabung darah ungu dapat menopang
        /// hemoglobin, leukosit, dan trombosit sekaligus dengan satu barcode.
        /// </summary>
        public ICollection<LabExamination> Examinations { get; set; } = new List<LabExamination>();
    }
}
