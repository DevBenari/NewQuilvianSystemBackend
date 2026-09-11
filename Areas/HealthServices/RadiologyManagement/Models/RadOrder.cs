using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models
{
    /// <summary>
    /// Pesanan pemeriksaan radiologi.
    ///
    /// Entity ini otoritatif atas keadaan operasional pesanan saja. Tidak ada satu pun kolom
    /// finansial di sini — tidak ada Paid, Settlement, PayerApproval, Void, Refund, maupun
    /// Reversal. <c>RJ-BIL-GATE-DEC-004</c> menyatakan Radiologi tidak memiliki financial status
    /// authority, dan cara paling andal menegakkannya adalah dengan tidak menyediakan kolomnya.
    /// </summary>
    public class RadOrder : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Nomor pesanan yang terbaca manusia. Tidak boleh kembar.
        /// </summary>
        /// <remarks>
        /// <c>RAD-CONF-001</c> bagian 8 butir 2. Inilah yang disebut orang ketika menunjuk
        /// sebuah pesanan — pada label yang menempel di amplop citra, pada lembar permintaan
        /// yang dibawa pasien, dan pada pencarian riwayat. <c>Guid</c> tidak dapat dibacakan
        /// lewat telepon dan tidak dapat diketik ulang petugas.
        /// </remarks>
        [Required]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public Guid EncounterId { get; set; }

        /// <summary>Pemeriksaan yang dipesan dokter.</summary>
        [Required]
        public Guid ProcedureId { get; set; }

        /// <summary>
        /// Perawatan rawat inap yang menaungi pesanan ini. Boleh kosong.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-042</c>, <c>INV-DOK-01</c>. Daftar pesanan radiologi sudah dapat disaring
        /// kunjungan sejak awal dan tidak diubah task ini.
        /// </remarks>
        public Guid? InpEpisodeId { get; set; }

        /// <summary>Modalitas yang diminta. Menentukan aturan keselamatan mana yang berlaku.</summary>
        [Required]
        public Guid ModalityId { get; set; }

        public RadOrderStatus OrderStatus { get; set; } = RadOrderStatus.Requested;

        /// <summary>
        /// Status operasional sebelum pesanan ditahan. Disimpan agar <c>OnHold</c> benar-benar
        /// mempertahankan keadaan sebelumnya dan dapat dilanjutkan tanpa menebak.
        /// </summary>
        public RadOrderStatus? StatusBeforeHold { get; set; }

        public string? ClinicalIndication { get; set; }

        public DateTime? RequestedAt { get; set; }

        public Guid? RequestedByUserId { get; set; }

        public DateTime? ScheduledAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Alasan penolakan atau pembatalan. Diisi bersama perpindahan status, bukan sesudahnya,
        /// supaya tidak ada pembatalan tanpa sebab yang tercatat.
        /// </summary>
        public string? ClosureReason { get; set; }

        /// <summary>
        /// Token konkurensi. Dua petugas yang memindahkan status pesanan yang sama secara
        /// bersamaan tidak boleh sama-sama berhasil.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Penanda cito, ditetapkan dokter pengirim saat memesan — <c>RAD-DEC-013</c>.
        ///
        /// <para>
        /// <b>Bawaannya <c>false</c>, dan itu disengaja.</b> Seluruh pesanan yang sudah ada
        /// sebelum kolom ini lahir menjadi tidak-cito, dan pemanggil yang tidak mengirim
        /// penandanya tetap berhasil membuat pesanan. Kalau bawaannya <c>true</c> — atau kolomnya
        /// wajib diisi — setiap pemanggil lama akan rusak seketika, dan daftar kerja akan penuh
        /// pesanan yang terlihat mendesak padahal tidak ada seorang pun yang pernah menyatakannya
        /// begitu.
        /// </para>
        /// </summary>
        public bool IsUrgent { get; set; }

        /// <summary>
        /// Siapa yang menandai pesanan ini cito. Kosong ketika pesanannya memang tidak cito.
        /// </summary>
        /// <remarks>
        /// Penanda cito mendahulukan seorang pasien di atas pasien lain yang sudah menunggu lebih
        /// lama. Wewenang seperti itu tidak boleh anonim: kalau sebuah unit mendapati hampir
        /// seluruh pesanannya bertanda cito, pertanyaan pertamanya adalah siapa yang menandai —
        /// dan pertanyaan itu harus punya jawaban.
        /// </remarks>
        public Guid? UrgentMarkedByUserId { get; set; }

        /// <summary>Kapan pesanan ini ditandai cito. Kosong ketika tidak cito.</summary>
        public DateTime? UrgentMarkedAt { get; set; }

        public RegPatientEncounter? Encounter { get; set; }

        public MstProcedure? Procedure { get; set; }

        public MstRadModality? Modality { get; set; }

        public ICollection<RadStudy> Studies { get; set; } = new List<RadStudy>();
    }
}
