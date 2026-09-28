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

    public enum BillingProcessingOutcome
    {
        [Display(Name = "Received")]
        Received = 1,

        [Display(Name = "In Progress")]
        InProgress = 2,

        [Display(Name = "Succeeded")]
        Succeeded = 3,

        [Display(Name = "Failed Before Effect")]
        FailedBeforeEffect = 4,

        [Display(Name = "Partial Outcome")]
        PartialOutcome = 5,

        [Display(Name = "Outcome Unknown")]
        OutcomeUnknown = 6
    }

    /// <summary>
    /// Keadaan penerusan satu efek folio ke invoice canonical (<c>RJ-E2E-DEC-003</c>,
    /// <c>RJ-E2E-DEC-016</c>). Nilai bawaan <see cref="NotApplicable"/> sengaja bernilai nol
    /// supaya baris yang terbentuk sebelum jembatan ada tidak pernah terbaca sebagai antrean kirim.
    /// </summary>
    public enum BillingInvoiceSyncStatus
    {
        [Display(Name = "Not Applicable")]
        NotApplicable = 0,

        [Display(Name = "Pending")]
        Pending = 1,

        [Display(Name = "Synced")]
        Synced = 2,

        [Display(Name = "Failed")]
        Failed = 3,

        [Display(Name = "Reconciliation Required")]
        ReconciliationRequired = 4,

        [Display(Name = "Resolved")]
        Resolved = 5
    }
}
