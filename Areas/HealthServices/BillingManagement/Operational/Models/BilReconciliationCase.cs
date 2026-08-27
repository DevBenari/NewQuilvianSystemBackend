using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models
{
    /// <summary>
    /// Satu ketidaksesuaian antara fakta klinis kanonik dan keadaan Billing kanonik yang belum
    /// dapat diselesaikan secara deterministik.
    ///
    /// Baris ini adalah bentuk nyata dari kalimat <c>RJ-BIL-GATE-DEC-008</c>: <i>"Non-deterministic
    /// mismatch membuat reconciliation case dengan type, source/version, billing/encounter
    /// reference, impact, status, owner, priority/risk, age/SLA, actions, resolution, dan
    /// audit."</i> Setiap butir pada daftar itu punya kolomnya sendiri di sini.
    ///
    /// Yang tidak ada di sini sama pentingnya. Case ini <b>tidak</b> memiliki kolom pembayaran,
    /// pembatalan, pengembalian dana, maupun penghapusan tagihan. Rekonsiliasi menemukan dan
    /// menampilkan masalah; akibat finansialnya tetap ditentukan <c>RJ-BIL-GATE-DEC-006</c>
    /// melalui jalur persetujuan tersendiri. Menaruh kewenangan finansial di sini akan membuat
    /// petugas rekonsiliasi diam-diam menjadi pemutus uang.
    /// </summary>
    public class BilReconciliationCase : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Nomor case yang dapat dibaca manusia, dipakai saat berkomunikasi antarpetugas.
        /// </summary>
        public string CaseNumber { get; set; } = string.Empty;

        public BillingReconciliationCaseType CaseType { get; set; }

        // ---------------------------------------------------------------
        // Rujukan sumber — fakta klinis mana yang bermasalah
        // ---------------------------------------------------------------

        public string SourceContext { get; set; } = string.Empty;

        public Guid MilestoneFactId { get; set; }

        public int MilestoneFactVersion { get; set; }

        public string EffectType { get; set; } = string.Empty;

        /// <summary>
        /// Efek pemrosesan yang memicu case ini, bila ada. Nullable karena sebagian jenis case —
        /// misalnya <c>MissingFact</c> — justru lahir dari ketiadaan efek.
        /// </summary>
        public Guid? ProcessingEffectId { get; set; }

        // ---------------------------------------------------------------
        // Rujukan Billing dan encounter
        // ---------------------------------------------------------------

        public Guid EncounterId { get; set; }

        public Guid? FolioId { get; set; }

        public Guid? ChargeLineId { get; set; }

        // ---------------------------------------------------------------
        // Dampak
        // ---------------------------------------------------------------

        /// <summary>
        /// Nilai rupiah yang dipertaruhkan case ini. Dipakai gerbang penutupan folio untuk
        /// menilai materialitas terhadap ambang pada <c>MstBillingReconciliationPolicy</c>.
        /// </summary>
        public decimal ImpactAmount { get; set; }

        public string ImpactDescription { get; set; } = string.Empty;

        /// <summary>
        /// Apakah case ini menahan penutupan folio. Disimpan, bukan dihitung saat dibaca, agar
        /// keputusan penahanan dapat ditelusuri kembali sebagaimana adanya pada saat itu —
        /// termasuk bila ambang materialitasnya kemudian diubah admin.
        /// </summary>
        public bool BlocksFolioClosure { get; set; }

        // ---------------------------------------------------------------
        // Status, kepemilikan, prioritas
        // ---------------------------------------------------------------

        public BillingReconciliationCaseStatus CaseStatus { get; set; } =
            BillingReconciliationCaseStatus.Open;

        public BillingReconciliationPriority Priority { get; set; } =
            BillingReconciliationPriority.Normal;

        /// <summary>
        /// Pemilik case. Sengaja boleh kosong: <c>RJ-BIL-GATE-DEC-008</c> mewajibkan case punya
        /// owner, tetapi tidak menetapkan aturan penugasan otomatis. Mengarang aturan penugasan
        /// berarti mengarang SOP, sehingga case lahir tanpa pemilik dan penugasannya menjadi
        /// tindakan sadar yang tercatat.
        /// </summary>
        public Guid? OwnerUserId { get; set; }

        public DateTime? AssignedAt { get; set; }

        // ---------------------------------------------------------------
        // Umur dan SLA
        // ---------------------------------------------------------------

        public DateTime DetectedAt { get; set; }

        public DateTime? SlaDueAt { get; set; }

        /// <summary>
        /// Waktu SLA terlampaui. Menurut <c>RJ-BIL-GATE-DEC-008</c> pelampauan SLA hanya memicu
        /// peringatan, eskalasi, prioritas, dan visibilitas — tidak pernah menyetujui,
        /// menghapus tagihan, membatalkan, atau menyelesaikan case dengan sendirinya.
        /// </summary>
        public DateTime? SlaBreachedAt { get; set; }

        // ---------------------------------------------------------------
        // Tindakan
        // ---------------------------------------------------------------

        public int AttemptCount { get; set; }

        public DateTime? LastAttemptAt { get; set; }

        public string? NextAction { get; set; }

        public string? FailureReason { get; set; }

        // ---------------------------------------------------------------
        // Penyelesaian
        // ---------------------------------------------------------------

        public BillingReconciliationResolutionType? ResolutionType { get; set; }

        public string? ResolutionNote { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public Guid? ResolvedByUserId { get; set; }

        // ---------------------------------------------------------------
        // Jejak dan konkurensi
        // ---------------------------------------------------------------

        public Guid? CorrelationId { get; set; }

        public int Version { get; set; }
    }
}
