using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models
{
    public class BilProcessingEffect : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Consumer { get; set; } = string.Empty;

        public string OperationType { get; set; } = string.Empty;

        public string IdempotencyKey { get; set; } = string.Empty;

        public string RequestFingerprint { get; set; } = string.Empty;

        public string SourceContext { get; set; } = string.Empty;

        public Guid MilestoneFactId { get; set; }

        public int MilestoneFactVersion { get; set; }

        public string EffectType { get; set; } = string.Empty;

        public DateTime OccurredAt { get; set; }

        public BillingProcessingOutcome Outcome { get; set; } = BillingProcessingOutcome.Received;

        public Guid? FolioId { get; set; }

        public Guid? ChargeLineId { get; set; }

        public BillingChargeCalculationStatus? CalculationStatus { get; set; }

        public string? ErrorCode { get; set; }

        public string? ErrorMessage { get; set; }

        public Guid? CorrelationId { get; set; }

        public Guid? CausationId { get; set; }

        public DateTime? CompletedAt { get; set; }

        // RJ-E2E-DEC-016: satu efek = satu (fakta, versi), sehingga efek inilah unit yang
        // diteruskan ke invoice canonical — bukan BilChargeLine, yang tidak bertambah saat revisi.

        /// <summary>Efek berasal dari fakta pembatalan klinis.</summary>
        public bool IsClinicalCancellation { get; set; }

        public BillingInvoiceSyncStatus InvoiceSyncStatus { get; set; } =
            BillingInvoiceSyncStatus.NotApplicable;

        /// <summary>Token konkurensi kolom sinkron; tabel ini tidak memiliki kolom Version.</summary>
        public int InvoiceSyncVersion { get; set; }

        public string? InvoiceSourceDomain { get; set; }

        public string? InvoiceSourceDetailId { get; set; }

        public Guid? InvoiceId { get; set; }

        public Guid? InvoiceItemId { get; set; }

        public Guid? InvoiceAdjustmentId { get; set; }

        public int InvoiceSyncAttemptCount { get; set; }

        public DateTime? InvoiceSyncNextAttemptAt { get; set; }

        public DateTime? InvoiceSyncedAt { get; set; }

        public string? InvoiceSyncErrorCode { get; set; }

        public string? InvoiceSyncErrorMessage { get; set; }

        public DateTime? ReconciliationResolvedAt { get; set; }

        public Guid? ReconciliationResolvedByUserId { get; set; }

        /// <summary>Alasan penyelesaian manual oleh petugas Billing. Tidak boleh memuat isi klinis.</summary>
        public string? ReconciliationResolutionNote { get; set; }
    }
}
