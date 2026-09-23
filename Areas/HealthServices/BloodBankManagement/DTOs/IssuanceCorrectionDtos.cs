using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs
{
    /// <summary>
    /// Mengajukan koreksi pencatatan pemberian atas kantong <c>Issued</c> (<c>DEC-BD-030</c>,
    /// <c>DEC-BD-041</c>). Koreksi belum berlaku sampai disetujui (<c>INV-BD-033</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Isian yang disimpan hanya empat</b>, persis kamus data <c>BbkIssuanceCorrection</c>:
    /// <see cref="WhatWasWrong"/>, <see cref="WhatIsCorrect"/>, <see cref="ReasonCode"/>, dan
    /// <see cref="SupportingEvidenceNote"/>. Pelaku, waktu, status, dan kantong ditentukan server.
    /// </para>
    /// <para>
    /// <b>Dua isian penjaga yang tidak pernah disimpan.</b> <see cref="IssuedToPatientId"/> dan
    /// <see cref="AnnulIssuance"/> ada semata supaya percobaan memakai koreksi sebagai jalur lain
    /// <b>ditolak eksplisit</b>, bukan diabaikan diam-diam: pasien tujuan yang berbeda dari pasien
    /// penerima → <c>422 VAL-BD-049</c> (<c>DEC-BD-052</c>), dan permintaan menganulir pemberian →
    /// <c>422 VAL-BD-025</c> (<c>INV-BD-021</c>). Bentuk penjaga ini keputusan pemilik sebelum
    /// implementasi <c>BE-BD-010</c>, 17 September 2026.
    /// </para>
    /// <para>
    /// <b>Sengaja tanpa <c>[Required]</c>/<c>[MaxLength]</c>.</b> Validasi model otomatis ASP.NET
    /// memulangkan <c>ProblemDetails</c> generik sebelum service berjalan, sehingga kode kontrak
    /// seperti <c>VAL-BD-076</c> tidak pernah terkirim — pelajaran runtime <c>BE-BD-009</c>. Service
    /// menegakkan seluruhnya secara fail-closed.
    /// </para>
    /// </remarks>
    public class RequestIssuanceCorrectionRequest
    {
        /// <summary>Apa yang keliru dicatat. Maksimal 500 karakter.</summary>
        public string? WhatWasWrong { get; set; }

        /// <summary>Apa yang benar. Maksimal 500 karakter.</summary>
        public string? WhatIsCorrect { get; set; }

        /// <summary>Kode alasan terkendali berkategori <c>IssuanceCorrection</c>.</summary>
        public string? ReasonCode { get; set; }

        /// <summary>Bukti pendukung berupa keterangan tertulis. Wajib, maksimal 1000 karakter.</summary>
        public string? SupportingEvidenceNote { get; set; }

        /// <summary>
        /// <b>Penjaga, tidak disimpan.</b> Bila diisi dengan pasien selain penerima pemberian,
        /// permintaan ditolak <c>VAL-BD-049</c>.
        /// </summary>
        public Guid? IssuedToPatientId { get; set; }

        /// <summary>
        /// <b>Penjaga, tidak disimpan.</b> <c>true</c> berarti mencoba menganulir pemberian dan
        /// ditolak <c>VAL-BD-025</c>.
        /// </summary>
        public bool? AnnulIssuance { get; set; }

        /// <summary>Token konkurensi kantong yang dipegang layar.</summary>
        public int? Version { get; set; }
    }

    /// <summary>
    /// Memutuskan satu permintaan koreksi — dipakai endpoint <c>approve</c> maupun <c>reject</c>.
    /// </summary>
    /// <remarks>
    /// <b>Keputusannya ditentukan endpoint, bukan body.</b> Tidak ada isian "setuju/tolak" di sini:
    /// menaruhnya di body akan membuat satu endpoint bersaklar, padahal kontrak memisahkan keduanya.
    /// Pemutus diambil dari akun yang login. <see cref="DecisionNote"/> wajib pada penolakan
    /// (<c>VAL-BD-077</c>) dan opsional pada persetujuan.
    /// </remarks>
    public class DecideCorrectionRequest
    {
        /// <summary>Keterangan pemutus. Maksimal 500 karakter.</summary>
        public string? DecisionNote { get; set; }
    }

    /// <summary>Satu koreksi pencatatan pemberian beserta keadaannya.</summary>
    /// <remarks>
    /// Koreksi <c>Requested</c> dan <c>Rejected</c> tetap terbaca di sini; hanya <c>Approved</c> yang
    /// berlaku terhadap angka pemenuhan (<c>INV-BD-033</c>).
    /// </remarks>
    public sealed class IssuanceCorrectionDto
    {
        public Guid Id { get; set; }
        public Guid BloodUnitId { get; set; }

        public string WhatWasWrong { get; set; } = string.Empty;
        public string WhatIsCorrect { get; set; } = string.Empty;
        public string ReasonCode { get; set; } = string.Empty;
        public string SupportingEvidenceNote { get; set; } = string.Empty;

        public BbkCorrectionStatus CorrectionStatus { get; set; }
        public string CorrectionStatusLabel { get; set; } = string.Empty;

        public Guid RequestedByUserId { get; set; }
        public DateTime RequestedAt { get; set; }

        public Guid? DecidedByUserId { get; set; }
        public DateTime? DecidedAt { get; set; }
        public string? DecisionNote { get; set; }

        /// <summary>Keadaan kantong saat dibaca — selalu <c>Issued</c> (<c>DEC-BD-051</c>).</summary>
        public BbkBloodUnitStatus UnitStatus { get; set; }
    }
}
