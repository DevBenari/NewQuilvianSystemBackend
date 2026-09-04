using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models
{
    /// <summary>
    /// Satu study atau acquisition radiologi.
    ///
    /// Study adalah satuan yang menentukan uang, bukan pesanan. <c>RJ-BIL-GATE-DEC-004</c>
    /// menyatakan `Requested`, `Accepted`, `Scheduled`, dan pelepasan laporan **bukan** pemicu
    /// tagihan; yang memicu adalah acquisition yang benar-benar dikerjakan dan menghasilkan
    /// citra yang dapat dipakai.
    ///
    /// Pengulangan **tidak pernah** menimpa study aslinya. Study baru dibuat dengan
    /// <see cref="RepeatOfStudyId"/> menunjuk ke study yang diulang beserta sebabnya, sehingga
    /// pertanyaan "berapa kali pasien ini sebenarnya disinari" selalu punya jawaban.
    /// </summary>
    public class RadStudy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RadOrderId { get; set; }

        /// <summary>
        /// Kunjungan disalin dari pesanan agar study dapat dicari tanpa join, dan agar fakta
        /// yang dikirim ke Billing tetap punya konteks kunjungannya sendiri.
        /// </summary>
        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid ProcedureId { get; set; }

        [Required]
        public Guid ModalityId { get; set; }

        /// <summary>Nomor urut study dalam satu pesanan, termasuk pengulangannya.</summary>
        public int StudySequence { get; set; }

        [Required]
        public string StudyNumber { get; set; } = string.Empty;

        public RadStudyStatus StudyStatus { get; set; } = RadStudyStatus.Planned;

        public RadStudyStatus? StatusBeforeHold { get; set; }

        /* ---------------------------------------------------------------- *
         * Gerbang identitas dan keselamatan
         * ---------------------------------------------------------------- */

        public DateTime? PatientVerifiedAt { get; set; }

        public Guid? PatientVerifiedByUserId { get; set; }

        public DateTime? SafetyClearedAt { get; set; }

        public Guid? SafetyClearedByUserId { get; set; }

        /// <summary>
        /// Versi aturan keselamatan yang berlaku saat study ini dinyatakan lolos. Dibekukan
        /// supaya perubahan aturan di kemudian hari tidak menulis ulang penilaian yang sudah
        /// terjadi.
        /// </summary>
        public int? SafetyRuleVersionAtClearance { get; set; }

        /* ---------------------------------------------------------------- *
         * Acquisition
         * ---------------------------------------------------------------- */

        public DateTime? AcquisitionStartedAt { get; set; }

        public Guid? AcquisitionStartedByUserId { get; set; }

        public DateTime? AcquiredAt { get; set; }

        /// <summary>
        /// Apakah citra yang dihasilkan dapat dipakai untuk pembacaan klinis.
        ///
        /// Inilah kolom yang menentukan kelayakan tagih normal. Ia sengaja `bool?`: `null`
        /// berarti belum dinilai, dan belum dinilai **bukan** berarti tidak dapat dipakai.
        /// </summary>
        public bool? IsUsable { get; set; }

        public DateTime? QualityDecidedAt { get; set; }

        public Guid? QualityDecidedByUserId { get; set; }

        public string? QualityNote { get; set; }

        /* ---------------------------------------------------------------- *
         * Penghentian dan pengulangan
         * ---------------------------------------------------------------- */

        public RadAbortCause? AbortCause { get; set; }

        public string? AbortReason { get; set; }

        public DateTime? AbortedAt { get; set; }

        /// <summary>
        /// Bagian pemeriksaan yang sempat dikerjakan sebelum dihentikan, sebagai keterangan
        /// bebas. Billing yang menilai akibat finansialnya, bukan Radiologi.
        /// </summary>
        public string? PerformedPortionNote { get; set; }

        /// <summary>Study yang diulang oleh study ini. Kosong berarti study ini bukan pengulangan.</summary>
        public Guid? RepeatOfStudyId { get; set; }

        public RadRepeatCause? RepeatCause { get; set; }

        public string? RepeatReason { get; set; }

        /// <summary>
        /// Order tambahan yang mengesahkan pengulangan karena kebutuhan klinis baru.
        /// <c>GATE-DEC-004</c> mewajibkan mekanisme order untuk kasus itu.
        /// </summary>
        public Guid? AdditionalOrderId { get; set; }

        public Guid? RepeatAuthorizedByUserId { get; set; }

        /* ---------------------------------------------------------------- *
         * Jejak ke Billing
         * ---------------------------------------------------------------- */

        /// <summary>
        /// Menandai bahwa fakta kelayakan tagih study ini sudah dikirim ke Billing. Mencegah
        /// pengiriman ganda tanpa perlu bertanya ke modul lain.
        /// </summary>
        public bool BillingFactSubmitted { get; set; }

        public DateTime? BillingFactSubmittedAt { get; set; }

        /// <summary>Pengenal study pada RIS/PACS bila kelak tersedia. Tidak dipakai sekarang.</summary>
        public string? ExternalStudyUid { get; set; }

        public string? ClosureReason { get; set; }

        public int Version { get; set; }

        public RadOrder? RadOrder { get; set; }

        public MstProcedure? Procedure { get; set; }

        public MstRadModality? Modality { get; set; }

        public RadStudy? RepeatOfStudy { get; set; }

        public ICollection<RadStudySafetyCheck> SafetyChecks { get; set; } =
            new List<RadStudySafetyCheck>();

        public ICollection<RadAcquisitionConsumption> Consumptions { get; set; } =
            new List<RadAcquisitionConsumption>();
    }
}
