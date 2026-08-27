using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models
{
    /// <summary>
    /// Kebijakan ambang dan kewenangan persetujuan finansial — <c>RJ-BIL-BE-006</c>.
    ///
    /// <c>RJ-BIL-GATE-DEC-006</c>: <i>"Finance memiliki threshold/authority policy yang versioned,
    /// effective-dated, approved, auditable, dan non-destructive."</i> Kelima sifat itu ada di
    /// sini: <see cref="PolicyVersion"/>, <see cref="EffectiveStartDate"/> beserta pasangannya,
    /// <see cref="IsApproved"/>, jejak audit bawaan <c>IdentityModel</c>, dan penggantian yang
    /// dilakukan dengan menerbitkan versi baru alih-alih menimpa baris lama.
    ///
    /// <para><b>Tabel ini sengaja dibiarkan kosong.</b></para>
    ///
    /// Tidak ada satu baris pun yang di-seed. Ini berbeda dari <c>RJ-BIL-BE-007</c>, yang atas
    /// <c>RJ-BIL-DEC-010</c> memakai nilai awal nol. Perbedaannya berasal dari keputusannya
    /// sendiri: <c>RJ-BIL-GATE-DEC-006</c> menyatakan <i>"Invalid/missing approval policy tidak
    /// memakai default approver/threshold"</i>. Mengisi tabel ini dengan angka karangan justru
    /// melanggar keputusan yang sedang dilaksanakannya.
    ///
    /// Akibatnya nyata dan memang dikehendaki: selama Finance belum menjawab
    /// <c>RJ-BIL-OQ-004</c>, tindakan finansial yang bergantung ambang berhenti pada
    /// <c>BlockedByPolicyConfiguration</c>. Uang tidak bergerak sebelum ada yang menetapkan siapa
    /// boleh menyetujui berapa. Tagihan deterministik yang normal tidak terpengaruh sama sekali.
    /// </summary>
    public class MstBillingApprovalPolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public BillingFinancialActionType ActionType { get; set; }

        public string PolicyCode { get; set; } = string.Empty;

        public int PolicyVersion { get; set; } = 1;

        // ---------------------------------------------------------------
        // Rentang berlaku
        // ---------------------------------------------------------------

        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        /// <summary>
        /// Kebijakan yang belum disetujui tidak boleh dipakai.
        ///
        /// <c>RJ-BIL-GATE-DEC-006</c> butir <c>4</c>: <i>"Draft, expired, future, unapproved,
        /// atau unversioned rule tidak boleh menghasilkan final charge."</i>
        /// </summary>
        public bool IsApproved { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        // ---------------------------------------------------------------
        // Ambang
        // ---------------------------------------------------------------

        public decimal? MinimumAmount { get; set; }

        public decimal? MaximumAmount { get; set; }

        public string Currency { get; set; } = "IDR";

        /// <summary>
        /// Apakah rentang nominal ini menuntut maker-checker.
        ///
        /// Perlu dicatat: nilai <c>false</c> di sini <b>tidak pernah</b> melonggarkan tindakan
        /// yang sudah high-risk menurut daftar tetap <c>RJ-BIL-GATE-DEC-006</c>. Kebijakan boleh
        /// menambah kewajiban persetujuan, tidak boleh mencabutnya.
        /// </summary>
        public bool RequiresApproval { get; set; } = true;

        /// <summary>
        /// Batas waktu sebuah permintaan menunggu keputusan sebelum ditandai kedaluwarsa.
        ///
        /// <c>0</c> berarti tidak pernah kedaluwarsa. Kedaluwarsa <b>bukan</b> persetujuan; ia
        /// hanya menutup permintaan yang tidak pernah diputuskan.
        /// </summary>
        public int ApprovalExpiryMinutes { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }
    }
}
