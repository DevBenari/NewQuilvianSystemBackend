using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models
{
    /// <summary>
    /// Catatan koreksi pencatatan pemberian — <c>BD-DOM-23</c>, entity di dalam batas kantong
    /// <c>BD-AGG-03</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Koreksi menempel, tidak pernah mengganti.</b> Pemberian asal pada <see cref="BbkBloodUnit"/>
    /// tidak dihapus, tidak dibalik, dan tidak disunting oleh baris ini (<c>DEC-BD-030</c>,
    /// <c>INV-BD-021</c>). Kantong tetap <c>Issued</c> sebelum dan sesudah koreksi disetujui, dan
    /// penanganan fisik kantong yang ternyata masih ada berada di luar jalur ini (<c>DEC-BD-051</c>).
    /// </para>
    /// <para>
    /// <b>Dua tahap dengan dua pelaku berbeda</b> (<c>DEC-BD-041</c>). Petugas BDRS mengajukan
    /// (<see cref="RequestedByUserId"/>), Dokter BDRS memutuskan (<see cref="DecidedByUserId"/>), dan
    /// keduanya wajib orang berbeda (<c>VAL-BD-073</c>). Satu pasang kolom pemutus dipakai untuk setuju
    /// maupun tolak, sehingga baris yang punya penyetuju sekaligus penolak tidak dapat ditulis.
    /// </para>
    /// <para>
    /// <b>Bukti pendukung berupa keterangan tertulis</b>; lampiran berkas belum diputuskan
    /// (<c>OQ-BD-016</c>). Pasien tidak disimpan di sini — koreksi tidak pernah memindahkan pemberian
    /// ke pasien lain (<c>VAL-BD-049</c>).
    /// </para>
    /// </remarks>
    [Table("BbkIssuanceCorrection", Schema = "public")]
    public class BbkIssuanceCorrection : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Kantong yang pencatatan pemberiannya dikoreksi.</summary>
        [Required]
        public Guid BloodUnitId { get; set; }

        public BbkBloodUnit? BloodUnit { get; set; }

        /// <summary>Apa yang keliru dicatat.</summary>
        [Required]
        [MaxLength(500)]
        public string WhatWasWrong { get; set; } = string.Empty;

        /// <summary>Apa yang benar.</summary>
        [Required]
        [MaxLength(500)]
        public string WhatIsCorrect { get; set; } = string.Empty;

        /// <summary>Kode alasan terkendali berkategori <c>IssuanceCorrection</c>.</summary>
        [Required]
        [MaxLength(30)]
        public string ReasonCode { get; set; } = string.Empty;

        /// <summary>Bukti pendukung berupa keterangan tertulis (<c>VAL-BD-076</c>).</summary>
        [Required]
        [MaxLength(1000)]
        public string SupportingEvidenceNote { get; set; } = string.Empty;

        /// <summary>Keadaan koreksi. Lahir <see cref="BbkCorrectionStatus.Requested"/>.</summary>
        public BbkCorrectionStatus CorrectionStatus { get; set; } = BbkCorrectionStatus.Requested;

        /// <summary>Petugas BDRS yang mengajukan, dari akun yang login.</summary>
        [Required]
        public Guid RequestedByUserId { get; set; }

        /// <summary>Waktu pengajuan, dari jam server.</summary>
        public DateTime RequestedAt { get; set; }

        /// <summary>Dokter BDRS yang memutuskan. Kosong selama <c>Requested</c>.</summary>
        public Guid? DecidedByUserId { get; set; }

        /// <summary>Waktu keputusan. Kosong selama <c>Requested</c>.</summary>
        public DateTime? DecidedAt { get; set; }

        /// <summary>Keterangan pemutus. Wajib bila <c>Rejected</c> (<c>VAL-BD-077</c>).</summary>
        [MaxLength(500)]
        public string? DecisionNote { get; set; }
    }
}
