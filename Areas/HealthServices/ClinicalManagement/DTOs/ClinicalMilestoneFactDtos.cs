using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    /// <summary>
    /// Permintaan penerbitan satu fakta klinis ke Billing.
    ///
    /// Tidak memuat status pembayaran, nominal yang dianggap final, maupun keputusan finansial
    /// apa pun. Modul klinis hanya menyatakan bahwa suatu peristiwa klinis benar terjadi.
    /// </summary>
    public sealed class ClinicalMilestoneFactRequest
    {
        public string SourceContext { get; set; } = string.Empty;

        public Guid SourceAggregateId { get; set; }

        public Guid? SourceItemId { get; set; }

        public string EffectType { get; set; } = string.Empty;

        public Guid EncounterId { get; set; }

        public DateTime OccurredAt { get; set; }

        public decimal? Quantity { get; set; }

        public string? Unit { get; set; }

        /// <summary>Snapshot tarif klinis sebagai rujukan, bukan nilai finansial otoritatif.</summary>
        public string? TariffSnapshot { get; set; }

        public string? RuleSnapshot { get; set; }

        public Guid? CorrelationId { get; set; }

        public Guid? CausationId { get; set; }
    }

    public enum ClinicalFactEmissionKind
    {
        /// <summary>Revisi baru berhasil diterbitkan dan diterima Billing.</summary>
        Emitted = 1,

        /// <summary>Fakta identik sudah pernah diterbitkan; hasil canonical dikembalikan.</summary>
        Replayed = 2,

        /// <summary>Pembatalan klinis terjadi sebelum charge terbentuk; tidak ada koreksi finansial.</summary>
        SuppressedNoPriorCharge = 3,

        /// <summary>Revisi sebelumnya berstatus tidak diketahui; wajib rekonsiliasi lebih dulu.</summary>
        ReconciliationRequired = 4,

        /// <summary>Permintaan tidak memenuhi kontrak fact.</summary>
        Invalid = 5,

        /// <summary>Billing menolak fakta dengan alasan kontrak.</summary>
        RejectedByBilling = 6,

        /// <summary>Pengiriman tidak dapat dipastikan hasilnya.</summary>
        OutcomeUnknown = 7
    }

    public sealed class ClinicalFactEmissionResult
    {
        private ClinicalFactEmissionResult(ClinicalFactEmissionKind kind)
        {
            Kind = kind;
        }

        public ClinicalFactEmissionKind Kind { get; private init; }

        public Guid? ClinicalMilestoneFactId { get; private init; }

        public Guid? MilestoneFactId { get; private init; }

        public int? MilestoneFactVersion { get; private init; }

        public ClinicalFactDispatchStatus? DispatchStatus { get; private init; }

        public Guid? BillingFolioId { get; private init; }

        public Guid? BillingChargeLineId { get; private init; }

        public string? Code { get; private init; }

        public string? Message { get; private init; }

        /// <summary>
        /// Benar bila peristiwa klinis sudah tercatat dengan aman, termasuk ketika Billing
        /// belum dapat dihubungi. Keadaan klinis tidak boleh dibatalkan hanya karena Billing
        /// sedang tidak tersedia.
        /// </summary>
        public bool IsClinicallySafe =>
            Kind is ClinicalFactEmissionKind.Emitted
                or ClinicalFactEmissionKind.Replayed
                or ClinicalFactEmissionKind.SuppressedNoPriorCharge
                or ClinicalFactEmissionKind.OutcomeUnknown;

        public static ClinicalFactEmissionResult Emitted(
            ClinicalFactEmissionKind kind,
            Guid clinicalMilestoneFactId,
            Guid milestoneFactId,
            int milestoneFactVersion,
            ClinicalFactDispatchStatus dispatchStatus,
            Guid? billingFolioId,
            Guid? billingChargeLineId,
            string? code = null,
            string? message = null) =>
            new(kind)
            {
                ClinicalMilestoneFactId = clinicalMilestoneFactId,
                MilestoneFactId = milestoneFactId,
                MilestoneFactVersion = milestoneFactVersion,
                DispatchStatus = dispatchStatus,
                BillingFolioId = billingFolioId,
                BillingChargeLineId = billingChargeLineId,
                Code = code,
                Message = message
            };

        public static ClinicalFactEmissionResult Failure(
            ClinicalFactEmissionKind kind,
            string code,
            string message,
            Guid? clinicalMilestoneFactId = null,
            Guid? milestoneFactId = null,
            int? milestoneFactVersion = null) =>
            new(kind)
            {
                Code = code,
                Message = message,
                ClinicalMilestoneFactId = clinicalMilestoneFactId,
                MilestoneFactId = milestoneFactId,
                MilestoneFactVersion = milestoneFactVersion
            };
    }
}
