using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums
{
    /// <summary>
    /// Jenis tindakan finansial yang dapat diajukan terhadap tagihan yang sudah terbentuk —
    /// <c>RJ-BIL-GATE-DEC-006</c> butir <c>1</c>.
    ///
    /// Ketiganya yang paling sering dikira sama sengaja dipisah, karena
    /// <c>RJ-BIL-GATE-DEC-006</c> menyatakan <i>"Void, reversal, dan refund berbeda secara
    /// semantik, lifecycle, approval, audit, dan accounting consequence."</i>
    /// <see cref="Void"/> membatalkan tagihan yang belum seharusnya ada;
    /// <see cref="Reversal"/> mengoreksi tagihan yang sah tetapi keliru;
    /// <see cref="Refund"/> mengembalikan uang yang sudah benar-benar diterima.
    /// </summary>
    public enum BillingFinancialActionType
    {
        [Display(Name = "Void")]
        Void = 1,

        [Display(Name = "Adjustment")]
        Adjustment = 2,

        [Display(Name = "Reversal")]
        Reversal = 3,

        [Display(Name = "Refund")]
        Refund = 4,

        /// <summary>
        /// Pembebasan biaya — yang dalam percakapan sehari-hari disebut FOC.
        /// </summary>
        [Display(Name = "Waiver / FOC")]
        Waiver = 5,

        [Display(Name = "Write Off")]
        WriteOff = 6,

        [Display(Name = "Manual Override")]
        ManualOverride = 7,

        [Display(Name = "Folio Reopen")]
        FolioReopen = 8
    }

    /// <summary>
    /// Lifecycle permintaan tindakan finansial.
    ///
    /// Urutan minimumnya dikunci <c>RJ-BIL-GATE-DEC-006</c>:
    /// <c>Draft → Submitted → PendingApproval → Approved</c>, dengan <see cref="Rejected"/>,
    /// <see cref="ReturnedForRevision"/>, <see cref="Cancelled"/>, dan <see cref="Expired"/>
    /// sebagai cabangnya.
    ///
    /// Dua anggota tambahan menerjemahkan kalimat fail-closed pada keputusan yang sama.
    /// <see cref="BlockedByPolicyConfiguration"/> dipakai ketika kebijakan ambang yang sah tidak
    /// dapat ditentukan — permintaan <b>bertahan</b> di sana, tidak digagalkan dan tidak pula
    /// diloloskan. <see cref="RevalidationRequired"/> dipakai ketika keadaan sasaran berubah
    /// setelah persetujuan diberikan, sehingga eksekusi tidak boleh berjalan buta.
    /// </summary>
    public enum BillingFinancialActionStatus
    {
        [Display(Name = "Draft")]
        Draft = 1,

        [Display(Name = "Submitted")]
        Submitted = 2,

        [Display(Name = "Pending Approval")]
        PendingApproval = 3,

        [Display(Name = "Approved")]
        Approved = 4,

        [Display(Name = "Rejected")]
        Rejected = 5,

        [Display(Name = "Returned For Revision")]
        ReturnedForRevision = 6,

        [Display(Name = "Cancelled")]
        Cancelled = 7,

        /// <summary>
        /// Kedaluwarsa. <c>RJ-BIL-GATE-DEC-006</c> menegaskan bahwa expired <b>bukan</b> approved.
        /// </summary>
        [Display(Name = "Expired")]
        Expired = 8,

        /// <summary>
        /// Kebijakan ambang yang sah tidak dapat ditentukan. Permintaan tidak digagalkan dan
        /// tidak pula dianggap disetujui; ia menunggu Finance menetapkan kebijakannya.
        /// </summary>
        [Display(Name = "Blocked By Policy Configuration")]
        BlockedByPolicyConfiguration = 9,

        [Display(Name = "Executed")]
        Executed = 10,

        /// <summary>
        /// Keadaan sasaran berubah setelah persetujuan. Eksekusi berhenti dan menuntut penilaian
        /// ulang, bukan menjalankan keputusan atas keadaan yang sudah tidak berlaku.
        /// </summary>
        [Display(Name = "Revalidation Required")]
        RevalidationRequired = 11
    }

    /// <summary>
    /// Tiga keputusan yang boleh diambil checker, dan hanya tiga.
    /// <c>RJ-BIL-GATE-DEC-006</c>: <i>"Checker hanya approve, reject, atau return for revision."</i>
    /// </summary>
    public enum BillingApprovalDecision
    {
        [Display(Name = "Approve")]
        Approve = 1,

        [Display(Name = "Reject")]
        Reject = 2,

        [Display(Name = "Return For Revision")]
        ReturnForRevision = 3
    }

    /// <summary>
    /// Tingkat risiko sebuah permintaan. Disimpan, bukan dihitung ulang saat dibaca, agar
    /// penilaian yang dipakai saat persetujuan tetap dapat dibuktikan di kemudian hari.
    /// </summary>
    public enum BillingFinancialRiskLevel
    {
        [Display(Name = "Normal")]
        Normal = 1,

        /// <summary>
        /// High-risk <b>tanpa memandang nominal</b>. <c>RJ-BIL-GATE-DEC-006</c> menyebut empat
        /// hal yang selalu masuk kategori ini: void/reversal terhadap tagihan yang sudah
        /// dibayar/diposting/diklaim/diselesaikan, refund atas pembayaran yang sudah settled,
        /// reopen folio yang sudah tertutup, dan koreksi lintas encounter.
        /// </summary>
        [Display(Name = "High Risk")]
        HighRisk = 2
    }

    public enum BillingFolioClosureAction
    {
        [Display(Name = "Close")]
        Close = 1,

        [Display(Name = "Reopen")]
        Reopen = 2
    }
}
