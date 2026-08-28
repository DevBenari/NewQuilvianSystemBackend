using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models
{
    /// <summary>
    /// Satu permintaan tindakan finansial terhadap tagihan yang sudah terbentuk —
    /// <c>RJ-BIL-BE-006</c>, melaksanakan <c>RJ-BIL-GATE-DEC-006</c>.
    ///
    /// <para><b>Baris ini adalah pengajuan, bukan pelaksanaan.</b></para>
    ///
    /// Keberadaannya tidak mengubah satu angka pun pada tagihan. <c>RJ-BIL-GATE-DEC-006</c>
    /// menyatakan <i>"Approval terjadi sebelum financial mutation efektif. Pending approval tidak
    /// mengubah canonical financial state."</i> Karena itu efek finansial baru muncul pada
    /// <see cref="ExecutedAt"/>, dan tidak pernah lebih awal.
    ///
    /// <para><b>Mengapa Billing memiliki tabel ini sendiri.</b></para>
    ///
    /// Sistem sudah punya mesin persetujuan yang lengkap di
    /// <c>Areas/Corporate/HumanResource/WorkflowManagement/</c>. Mesin itu <b>tidak</b> dipakai,
    /// atas <c>RJ-BIL-DEC-011</c>, karena dua invariant keputusan ini tidak dapat ditegakkan di
    /// sana: larangan self-approval di mesin itu adalah <c>bool</c> per step yang dapat
    /// dinyalakan lewat layar konfigurasi modul lain, dan penyaringan maker hanya terjadi sekali
    /// saat assignment dibuat sehingga delegasi dapat mengembalikan persetujuan kepada
    /// pengajunya. Untuk uang, kedua hal itu tidak boleh bergantung pada konfigurasi milik siapa
    /// pun.
    ///
    /// <para><b>Yang sengaja tidak ada di sini.</b></para>
    ///
    /// Tidak ada kolom nomor rekening, kanal pembayaran, atau bukti transfer. Permintaan ini
    /// mencatat <i>kewenangan</i> untuk sebuah tindakan finansial; perpindahan uang yang
    /// sesungguhnya bukan cakupan <c>RJ-BIL-BE-006</c> dan tetap tertutup selama
    /// <c>RJ-BIL-DEP-009</c> berstatus inactive.
    /// </summary>
    public class BilFinancialActionRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Nomor permintaan yang dapat dibaca dan disebut manusia saat berkomunikasi.
        /// </summary>
        public string RequestNumber { get; set; } = string.Empty;

        public BillingFinancialActionType ActionType { get; set; }

        // ---------------------------------------------------------------
        // Sasaran — tagihan mana yang hendak diubah
        // ---------------------------------------------------------------

        public Guid FolioId { get; set; }

        public Guid EncounterId { get; set; }

        public Guid? ChargeLineId { get; set; }

        public Guid? ChargeComponentId { get; set; }

        /// <summary>
        /// Encounter tujuan pada koreksi lintas encounter.
        ///
        /// <c>RJ-BIL-GATE-DEC-006</c> melarang mengubah <c>EncounterId</c> pada charge asli.
        /// Karena itu koreksi lintas encounter dicatat di sini sebagai <i>tujuan</i>, sementara
        /// <see cref="EncounterId"/> tetap merekam encounter asalnya. Keduanya disimpan agar
        /// penelusuran asal-tujuan tidak pernah kehilangan salah satu sisinya.
        /// </summary>
        public Guid? TargetEncounterId { get; set; }

        /// <summary>
        /// Versi baris tagihan sasaran pada saat permintaan diajukan.
        ///
        /// Dipakai saat eksekusi untuk menilai ulang keadaan sasaran. Bila versinya sudah
        /// berubah, permintaan masuk <see cref="BillingFinancialActionStatus.RevalidationRequired"/>
        /// alih-alih dijalankan atas keadaan yang sudah tidak berlaku.
        /// </summary>
        public int? TargetVersionAtSubmission { get; set; }

        // ---------------------------------------------------------------
        // Nilai dan alasan
        // ---------------------------------------------------------------

        public decimal RequestedAmount { get; set; }

        public string Currency { get; set; } = "IDR";

        public string ReasonCode { get; set; } = string.Empty;

        public string? ReasonNote { get; set; }

        // ---------------------------------------------------------------
        // Risiko, kebijakan, dan lifecycle
        // ---------------------------------------------------------------

        public BillingFinancialRiskLevel RiskLevel { get; set; } =
            BillingFinancialRiskLevel.Normal;

        /// <summary>
        /// Disimpan, bukan disimpulkan ulang saat dibaca. Alasannya sama dengan
        /// <see cref="RiskLevel"/>: yang harus dapat dibuktikan adalah penilaian yang berlaku
        /// <i>saat</i> permintaan diputuskan, bukan penilaian hari ini.
        /// </summary>
        public bool RequiresApproval { get; set; }

        public BillingFinancialActionStatus Status { get; set; } =
            BillingFinancialActionStatus.Draft;

        /// <summary>
        /// Kebijakan ambang yang dipakai, beserta versinya.
        ///
        /// <c>RJ-BIL-GATE-DEC-006</c>: <i>"Historical request tetap mereferensikan policy version
        /// asal."</i> Karena itu keduanya disalin ke sini dan tidak pernah diperbarui, walaupun
        /// kebijakannya kelak diganti.
        /// </summary>
        public Guid? ApprovalPolicyId { get; set; }

        public int? ApprovalPolicyVersion { get; set; }

        /// <summary>
        /// Alasan mengapa kebijakan tidak dapat ditentukan, bila statusnya
        /// <see cref="BillingFinancialActionStatus.BlockedByPolicyConfiguration"/>.
        /// </summary>
        public string? PolicyBlockReason { get; set; }

        // ---------------------------------------------------------------
        // Maker, revisi, dan keutuhan isi
        // ---------------------------------------------------------------

        public Guid MakerUserId { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public int RevisionNumber { get; set; } = 1;

        /// <summary>
        /// Permintaan yang digantikan oleh baris ini.
        ///
        /// <c>RJ-BIL-GATE-DEC-006</c>: <i>"Material change menghasilkan maker revision baru;
        /// request lama immutable."</i> Perubahan tidak pernah menimpa baris lama; ia melahirkan
        /// baris baru yang menunjuk ke pendahulunya.
        /// </summary>
        public Guid? SupersedesRequestId { get; set; }

        /// <summary>
        /// Sidik isi permintaan. Dihitung dari seluruh field yang menentukan akibat finansial,
        /// lalu disalin ke setiap keputusan checker.
        ///
        /// Inilah yang membuat kalimat <i>"checker material edit tidak dapat disetujui sebagai
        /// request lama"</i> dapat dibuktikan, bukan sekadar dijanjikan: persetujuan menyimpan
        /// sidik isi yang benar-benar dilihat checker pada saat ia memutuskan.
        /// </summary>
        public string ContentHash { get; set; } = string.Empty;

        /// <summary>
        /// Kunci idempotensi pengajuan. Pengiriman ulang dengan kunci yang sama tidak melahirkan
        /// permintaan kedua.
        /// </summary>
        public string? IdempotencyKey { get; set; }

        // ---------------------------------------------------------------
        // Pelaksanaan
        // ---------------------------------------------------------------

        public DateTime? ExecutedAt { get; set; }

        public Guid? ExecutedByUserId { get; set; }

        /// <summary>
        /// Nilai yang benar-benar diterapkan saat eksekusi. Dapat berbeda dari
        /// <see cref="RequestedAmount"/> bila checker menyetujui nominal yang lebih kecil.
        /// </summary>
        public decimal? ExecutedAmount { get; set; }

        public string? ExecutionNote { get; set; }

        public Guid? CorrelationId { get; set; }

        public int Version { get; set; } = 1;

        public ICollection<BilFinancialApproval> Approvals { get; set; } =
            new List<BilFinancialApproval>();
    }
}
