using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.DTOs
{
    public class JournalPagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;

        public Guid? LegalEntityId { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public Guid? JournalTypeId { get; set; }

        public JournalStatus? JournalStatus { get; set; }

        /// <summary>Dicocokkan pada nomor jurnal dan keterangan.</summary>
        public string? Search { get; set; }

        public string? SortBy { get; set; }

        public string? SortDirection { get; set; }
    }

    public class JournalListResponse
    {
        public Guid Id { get; set; }

        public string JournalNumber { get; set; } = string.Empty;

        public DateTime AccountingDate { get; set; }

        public Guid JournalTypeId { get; set; }

        public string JournalTypeName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public JournalStatus JournalStatus { get; set; }

        public decimal TotalDebit { get; set; }

        public decimal TotalCredit { get; set; }
    }

    public class JournalDetailResponse
    {
        public Guid Id { get; set; }

        public Guid LegalEntityId { get; set; }

        public string JournalNumber { get; set; } = string.Empty;

        public Guid JournalTypeId { get; set; }

        public string JournalTypeCode { get; set; } = string.Empty;

        public string JournalTypeName { get; set; } = string.Empty;

        public Guid AccountingPeriodId { get; set; }

        /// <summary>Bentuk <c>2026-09</c>. Diturunkan dari periode, bukan dikirim pengguna.</summary>
        public string PeriodCode { get; set; } = string.Empty;

        public string? DocumentNumber { get; set; }

        public DateTime? DocumentDate { get; set; }

        public DateTime AccountingDate { get; set; }

        public string Description { get; set; } = string.Empty;

        public JournalStatus JournalStatus { get; set; }

        public decimal TotalDebit { get; set; }

        public decimal TotalCredit { get; set; }

        /// <summary>
        /// <c>false</c> berarti jurnal masih timpang. Draft timpang tetap boleh disimpan
        /// (<c>ACC-DEC-025</c>); keseimbangan baru menggigit saat pengajuan pada `BE-ACC-011`.
        /// </summary>
        public bool IsBalanced { get; set; }

        public Guid? SubmittedBy { get; set; }

        /// <summary>Nama aktor, diisi saat baca. Tidak dipersistensi (QBE-ENT-003).</summary>
        public string? SubmittedByName { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public Guid? ApprovedBy { get; set; }

        /// <summary>Nama aktor, diisi saat baca. Tidak dipersistensi (QBE-ENT-003).</summary>
        public string? ApprovedByName { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public Guid? PostedBy { get; set; }

        /// <summary>Nama aktor, diisi saat baca. Tidak dipersistensi (QBE-ENT-003).</summary>
        public string? PostedByName { get; set; }

        public DateTime? PostedAt { get; set; }

        public string? RejectionReason { get; set; }

        public Guid? ReversalOfJournalId { get; set; }

        public string? ReversalOfJournalNumber { get; set; }

        public JournalCorrectionType? CorrectionType { get; set; }

        public DateTime CreateDateTime { get; set; }

        public Guid CreateBy { get; set; }

        public List<JournalLineResponse> Lines { get; set; } = new();

        public List<JournalApprovalResponse> Approvals { get; set; } = new();

        /// <summary>
        /// Tindakan yang masih mungkin atas jurnal ini.
        /// </summary>
        /// <remarks>
        /// Dihitung backend dari <b>tiga</b> hal sekaligus (`BE-ACC-011` acceptance 6): status
        /// jurnal, hak akses pengguna, dan aturan pembuat-bukan-penyetuju (`ACC-DEC-016`).
        /// Nilainya salah satu dari <c>update</c>, <c>delete</c>, <c>submit</c>, <c>approve</c>,
        /// <c>reject</c>, <c>post</c>, dan <c>reverse</c>.
        ///
        /// Frontend menampilkan tombol berdasarkan daftar ini dan <b>tidak boleh</b> menghitung
        /// sendiri. Backend tetap memeriksa ulang saat tindakannya benar-benar dijalankan —
        /// daftar ini mempercepat layar, bukan menggantikan penegakan.
        /// </remarks>
        public List<string> AvailableActions { get; set; } = new();
    }

    public class JournalLineResponse
    {
        public Guid Id { get; set; }

        public int LineNumber { get; set; }

        public Guid AccountId { get; set; }

        public string AccountCode { get; set; } = string.Empty;

        public string AccountName { get; set; } = string.Empty;

        public Guid? CostCenterId { get; set; }

        public string? CostCenterName { get; set; }

        public string? Description { get; set; }

        public decimal DebitAmount { get; set; }

        public decimal CreditAmount { get; set; }
    }

    public class JournalApprovalResponse
    {
        public JournalApprovalAction ApprovalAction { get; set; }

        public Guid ActionBy { get; set; }

        /// <summary>
        /// Nama aktor, diisi saat baca dari <c>AspNetUsers</c>. Tidak dipersistensi:
        /// menyimpan nama pada baris riwayat melanggar QBE-ENT-003 dan akan basi begitu
        /// pengguna berganti nama.
        /// </summary>
        public string? ActionByName { get; set; }

        public DateTime ActionAt { get; set; }

        public string? Reason { get; set; }
    }

    public class CreateJournalRequest
    {
        [Required]
        public Guid LegalEntityId { get; set; }

        [Required]
        public Guid JournalTypeId { get; set; }

        [MaxLength(50)]
        public string? DocumentNumber { get; set; }

        public DateTime? DocumentDate { get; set; }

        /// <summary>
        /// Menentukan periode dan bulan penomoran. Periode <b>tidak</b> dikirim pengguna.
        /// </summary>
        [Required]
        public DateTime AccountingDate { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Dikirim <b>utuh</b>. Tidak ada jalur penambahan atau penghapusan baris satu per satu.
        /// </summary>
        public List<CreateJournalLineRequest> Lines { get; set; } = new();
    }

    /// <summary>
    /// Sama dengan <see cref="CreateJournalRequest"/> tanpa <c>LegalEntityId</c>: badan hukum
    /// sebuah jurnal tidak dapat berpindah setelah dibuat.
    /// </summary>
    public class UpdateJournalRequest
    {
        [Required]
        public Guid JournalTypeId { get; set; }

        [MaxLength(50)]
        public string? DocumentNumber { get; set; }

        public DateTime? DocumentDate { get; set; }

        [Required]
        public DateTime AccountingDate { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Menggantikan <b>seluruh</b> baris sebelumnya.
        /// </summary>
        public List<CreateJournalLineRequest> Lines { get; set; } = new();
    }

    /// <summary>
    /// Penolakan jurnal. Alasan <b>wajib</b> — `ACC-STATE-0.1` bagian 1.1 dan
    /// `ACC-VALIDATION-0.2` bagian 4.
    /// </summary>
    public class RejectJournalRequest
    {
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Pembalikan atau penyesuaian jurnal yang sudah disahkan. Cakupan `BE-ACC-013`.
    /// </summary>
    public class ReverseJournalRequest
    {
        /// <summary>
        /// Wajib dipilih. <c>FullReversal</c> membalik seluruh baris; <c>Adjustment</c> mencatat
        /// selisihnya saja.
        /// </summary>
        [Required]
        public JournalCorrectionType? CorrectionType { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Tanggal akuntansi jurnal koreksi. Kosong berarti mengikuti tanggal jurnal asal.
        /// </summary>
        public DateTime? AccountingDate { get; set; }

        /// <summary>
        /// Wajib diisi dan harus seimbang bila <see cref="CorrectionType"/> adalah
        /// <c>Adjustment</c>. Diabaikan pada pembalikan penuh, karena barisnya diturunkan dari
        /// jurnal asal.
        /// </summary>
        public List<CreateJournalLineRequest> AdjustmentLines { get; set; } = new();
    }

    /// <summary>
    /// Hak akses pengguna yang sedang masuk atas sebuah jurnal, dinilai lapisan infrastruktur
    /// lalu diserahkan ke service.
    /// </summary>
    /// <remarks>
    /// Penilaian hak akses adalah urusan <c>AccessPermissionService</c>, bukan aturan bisnis
    /// Accounting, sehingga ia tetap di controller. Yang dikerjakan service adalah
    /// menggabungkannya dengan status jurnal dan aturan pembuat-bukan-penyetuju
    /// (<c>ACC-DEC-016</c>) — dan aturan terakhir itu memang wajib berada di service, sesuai
    /// `ACC-PERMISSION-0.3` bagian 5.
    /// </remarks>
    public sealed record JournalActorPermissions(
        bool CanUpdate,
        bool CanDelete,
        bool CanSubmit,
        bool CanApprove,
        bool CanPost,
        bool CanReverse)
    {
        /// <summary>Dipakai jalur yang belum menilai hak akses; menghasilkan daftar tindakan kosong.</summary>
        public static readonly JournalActorPermissions Kosong =
            new(false, false, false, false, false, false);
    }

    public class CreateJournalLineRequest
    {
        public int LineNumber { get; set; }

        public Guid AccountId { get; set; }

        public Guid? CostCenterId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public decimal DebitAmount { get; set; }

        public decimal CreditAmount { get; set; }
    }
}
