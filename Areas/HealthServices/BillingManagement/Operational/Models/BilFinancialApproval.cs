using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models
{
    /// <summary>
    /// Satu keputusan checker atas satu permintaan tindakan finansial — <c>RJ-BIL-BE-006</c>.
    ///
    /// Baris di sini <b>hanya ditambahkan, tidak pernah diubah</b>. Persetujuan yang dapat
    /// disunting belakangan bukan persetujuan; ia hanya catatan yang kebetulan bernama begitu.
    ///
    /// <c>RJ-BIL-GATE-DEC-006</c> menuntut jejak persetujuan menelusuri request, maker/checker,
    /// nominal diminta dan disetujui, dampak, status sebelum dan sesudah, alasan, versi
    /// kebijakan, rujukan encounter/folio/charge, correlation, serta waktu. Setiap butir itu
    /// punya kolomnya sendiri di bawah ini.
    /// </summary>
    public class BilFinancialApproval : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid RequestId { get; set; }

        public BillingApprovalDecision Decision { get; set; }

        public Guid CheckerUserId { get; set; }

        public DateTime DecidedAt { get; set; } = DateTime.UtcNow;

        public string? DecisionNote { get; set; }

        /// <summary>
        /// Nominal yang benar-benar disetujui checker.
        ///
        /// Dipisahkan dari nominal yang diminta karena keduanya boleh berbeda, dan yang mengikat
        /// saat eksekusi adalah yang disetujui — bukan yang diajukan.
        /// </summary>
        public decimal? ApprovedAmount { get; set; }

        /// <summary>
        /// Sidik isi permintaan pada detik checker memutuskan.
        ///
        /// Disalin ke sini, bukan dibaca ulang dari permintaannya, supaya pertanyaan
        /// <i>"sebenarnya apa yang disetujui orang ini?"</i> punya jawaban yang tidak bergantung
        /// pada keadaan baris lain di kemudian hari.
        /// </summary>
        public string RequestContentHash { get; set; } = string.Empty;

        // ---------------------------------------------------------------
        // Konteks yang dibekukan agar jejaknya berdiri sendiri
        // ---------------------------------------------------------------

        public BillingFinancialActionType ActionType { get; set; }

        public BillingFinancialActionStatus PriorStatus { get; set; }

        public BillingFinancialActionStatus ResultingStatus { get; set; }

        public Guid MakerUserId { get; set; }

        public Guid FolioId { get; set; }

        public Guid EncounterId { get; set; }

        public Guid? ChargeLineId { get; set; }

        public decimal RequestedAmount { get; set; }

        public Guid? ApprovalPolicyId { get; set; }

        public int? ApprovalPolicyVersion { get; set; }

        public Guid? CorrelationId { get; set; }

        public BilFinancialActionRequest Request { get; set; } = null!;
    }
}
