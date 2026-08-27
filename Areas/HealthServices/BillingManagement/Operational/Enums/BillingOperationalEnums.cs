using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums
{
    public enum BillingFolioStatus
    {
        [Display(Name = "Open")]
        Open = 1,

        [Display(Name = "Review Required")]
        ReviewRequired = 2,

        [Display(Name = "Ready to Close")]
        ReadyToClose = 3,

        [Display(Name = "Closed")]
        Closed = 4
    }

    public enum BillingChargeCalculationStatus
    {
        [Display(Name = "Received")]
        Received = 1,

        [Display(Name = "Evaluating")]
        Evaluating = 2,

        [Display(Name = "Pending Financial Review")]
        PendingFinancialReview = 3,

        [Display(Name = "Recognized")]
        Recognized = 4,

        [Display(Name = "Superseded")]
        Superseded = 5,

        [Display(Name = "Voided")]
        Voided = 6,

        [Display(Name = "Reversed")]
        Reversed = 7
    }

    /// <summary>
    /// Hasil pemrosesan sebuah fakta klinis oleh Billing, terpisah dari status finansialnya.
    ///
    /// Kosakata ini dituntut <c>RJ-BIL-GATE-DEC-008</c>. Lima anggota pertama sudah ada sejak
    /// <c>RJ-BIL-BE-002</c> dengan nama yang setara; yang ditambahkan <c>RJ-BIL-BE-007</c>
    /// adalah pembedaan tiga jenis kegagalan beserta dua status rekonsiliasi.
    ///
    /// Pembedaan tiga jenis kegagalan itu bukan kerapian penamaan. Ketiganya menuntut tindakan
    /// yang berbeda: <see cref="RejectedValidation"/> bersifat final untuk versi tersebut dan
    /// hanya dapat diperbaiki melalui versi baru; <see cref="TransientFailure"/> boleh dicoba
    /// ulang otomatis dengan kunci yang sama; <see cref="PermanentFailure"/> berhenti dari
    /// percobaan ulang dan masuk telaah terkendali. Menyatukan ketiganya menjadi satu nilai
    /// membuat sistem tidak dapat memutuskan mana yang boleh diulang.
    /// </summary>
    public enum BillingProcessingOutcome
    {
        [Display(Name = "Received")]
        Received = 1,

        [Display(Name = "In Progress")]
        InProgress = 2,

        [Display(Name = "Succeeded")]
        Succeeded = 3,

        /// <summary>
        /// Peninggalan <c>RJ-BIL-BE-002</c>: kegagalan sebelum efek finansial terbentuk, tanpa
        /// membedakan sebabnya. Nilai ini <b>tidak dipakai lagi</b> untuk baris baru, tetapi
        /// sengaja dipertahankan karena baris lama di database sudah memuatnya. Menghapusnya
        /// akan membuat baris tersebut tidak dapat dibaca kembali sebagai status mana pun.
        /// </summary>
        [Display(Name = "Failed Before Effect (legacy)")]
        FailedBeforeEffect = 4,

        [Display(Name = "Partial Outcome")]
        PartialOutcome = 5,

        [Display(Name = "Outcome Unknown")]
        OutcomeUnknown = 6,

        /// <summary>
        /// Payload ditolak validasi. Final untuk versi fakta tersebut; perbaikannya adalah
        /// versi fakta baru, bukan percobaan ulang.
        /// </summary>
        [Display(Name = "Rejected Validation")]
        RejectedValidation = 7,

        /// <summary>
        /// Gangguan sementara. Boleh dicoba ulang otomatis dengan kunci idempotensi yang sama.
        /// </summary>
        [Display(Name = "Transient Failure")]
        TransientFailure = 8,

        /// <summary>
        /// Gangguan menetap. Berhenti dari percobaan ulang otomatis dan masuk dead-letter untuk
        /// telaah terkendali. Dead letter bukan penyelesaian.
        /// </summary>
        [Display(Name = "Permanent Failure")]
        PermanentFailure = 9,

        /// <summary>
        /// Menunggu rekonsiliasi. Hasilnya belum dapat dipastikan dan sudah memiliki case.
        /// </summary>
        [Display(Name = "Pending Reconciliation")]
        PendingReconciliation = 10,

        /// <summary>
        /// Sudah direkonsiliasi dan hasil akhirnya diketahui.
        /// </summary>
        [Display(Name = "Reconciled")]
        Reconciled = 11
    }

    /// <summary>
    /// Jenis ketidaksesuaian yang dibandingkan rekonsiliasi antara fakta klinis kanonik dengan
    /// keadaan Billing kanonik, sesuai daftar pada <c>RJ-BIL-GATE-DEC-008</c>.
    /// </summary>
    public enum BillingReconciliationCaseType
    {
        [Display(Name = "Outcome Unknown")]
        OutcomeUnknown = 1,

        [Display(Name = "Partial Component Failure")]
        PartialComponentFailure = 2,

        [Display(Name = "Permanent Failure")]
        PermanentFailure = 3,

        [Display(Name = "Duplicate Charge")]
        DuplicateCharge = 4,

        [Display(Name = "Missing Fact")]
        MissingFact = 5,

        [Display(Name = "Orphan Charge")]
        OrphanCharge = 6,

        [Display(Name = "Amount Mismatch")]
        AmountMismatch = 7,

        [Display(Name = "Version Mismatch")]
        VersionMismatch = 8,

        [Display(Name = "Stale Projection")]
        StaleProjection = 9,

        [Display(Name = "Unresolved Exception")]
        UnresolvedException = 10
    }

    public enum BillingReconciliationCaseStatus
    {
        [Display(Name = "Open")]
        Open = 1,

        [Display(Name = "In Progress")]
        InProgress = 2,

        [Display(Name = "Escalated")]
        Escalated = 3,

        [Display(Name = "Resolved")]
        Resolved = 4,

        [Display(Name = "Auto Resolved")]
        AutoResolved = 5
    }

    public enum BillingReconciliationPriority
    {
        [Display(Name = "Low")]
        Low = 1,

        [Display(Name = "Normal")]
        Normal = 2,

        [Display(Name = "High")]
        High = 3,

        [Display(Name = "Critical")]
        Critical = 4
    }

    /// <summary>
    /// Cara sebuah case diselesaikan. Tidak satu pun anggota di sini menetapkan akibat
    /// finansial: <see cref="ManualFinancialAction"/> justru menyatakan bahwa keputusan
    /// finansialnya diserahkan kepada <c>RJ-BIL-GATE-DEC-006</c>, yaitu ranah
    /// <c>RJ-BIL-BE-006</c>.
    /// </summary>
    public enum BillingReconciliationResolutionType
    {
        [Display(Name = "Confirmed Applied")]
        ConfirmedApplied = 1,

        [Display(Name = "Confirmed Not Applied")]
        ConfirmedNotApplied = 2,

        [Display(Name = "Reprocessed")]
        Reprocessed = 3,

        [Display(Name = "Deterministic Duplicate")]
        DeterministicDuplicate = 4,

        [Display(Name = "Manual Financial Action Required")]
        ManualFinancialAction = 5,

        [Display(Name = "No Financial Impact")]
        NoFinancialImpact = 6
    }
}
