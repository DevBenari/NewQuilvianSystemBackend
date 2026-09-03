using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    /// <summary>
    /// Catatan satu revisi fakta klinis yang diserahkan ke Billing.
    ///
    /// Satu baris sama dengan satu revisi. Koreksi tidak menimpa baris lama melainkan
    /// menambah baris baru dengan <see cref="MilestoneFactVersion"/> berikutnya dan
    /// <see cref="MilestoneFactId"/> yang sama, sehingga riwayat penyerahan tetap utuh dan
    /// dapat diaudit.
    /// </summary>
    public class TrxClinicalMilestoneFact : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Modul klinis pemilik fakta, misalnya <c>Prescription</c> atau <c>Procedure</c>.</summary>
        public string SourceContext { get; set; } = string.Empty;

        /// <summary>Identitas aggregate klinis, misalnya Id resep atau Id tindakan.</summary>
        public Guid SourceAggregateId { get; set; }

        /// <summary>Identitas item di dalam aggregate bila fakta berlaku per item.</summary>
        public Guid? SourceItemId { get; set; }

        /// <summary>Jenis efek finansial yang diminta dikenali Billing.</summary>
        public string EffectType { get; set; } = string.Empty;

        /// <summary>
        /// Identitas fakta yang stabil lintas revisi. Nilai ini tidak berubah ketika terjadi
        /// koreksi atau pembatalan, sehingga Billing selalu menautkannya ke charge yang sama.
        /// </summary>
        public Guid MilestoneFactId { get; set; }

        /// <summary>Nomor revisi fakta, dimulai dari <c>1</c> dan menaik.</summary>
        public int MilestoneFactVersion { get; set; } = 1;

        public ClinicalMilestoneKind MilestoneKind { get; set; } =
            ClinicalMilestoneKind.ChargeEligibility;

        public Guid EncounterId { get; set; }

        /// <summary>Waktu peristiwa klinis terjadi, bukan waktu pengiriman.</summary>
        public DateTime OccurredAt { get; set; }

        public decimal? Quantity { get; set; }

        public string? Unit { get; set; }

        /// <summary>Snapshot tarif klinis sebagai rujukan; bukan nilai finansial otoritatif.</summary>
        public string? TariffSnapshot { get; set; }

        public string? RuleSnapshot { get; set; }

        /// <summary>
        /// Kunci idempotency yang dikirim ke Billing. Stabil per revisi, sehingga retry
        /// memakai kunci yang sama dan tidak menggandakan charge.
        /// </summary>
        public string IdempotencyKey { get; set; } = string.Empty;

        /// <summary>Sidik jari isi material fakta; dipakai mendeteksi revisi identik.</summary>
        public string PayloadFingerprint { get; set; } = string.Empty;

        public ClinicalFactDispatchStatus DispatchStatus { get; set; } =
            ClinicalFactDispatchStatus.Pending;

        public int DispatchAttemptCount { get; set; }

        public DateTime? DispatchedAt { get; set; }

        public Guid? BillingProcessingEffectId { get; set; }

        public Guid? BillingFolioId { get; set; }

        public Guid? BillingChargeLineId { get; set; }

        public string? BillingOutcomeCode { get; set; }

        public string? BillingOutcomeMessage { get; set; }

        public Guid CorrelationId { get; set; } = Guid.NewGuid();

        public Guid? CausationId { get; set; }

        /// <summary>Aktor klinis yang menyebabkan fakta ini terbit.</summary>
        public Guid ActorUserId { get; set; }

        public int Version { get; set; } = 1;

        public bool IsActive { get; set; } = true;
    }
}
