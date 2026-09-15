using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.DTOs
{
    /// <summary>
    /// Penyaring daftar aturan posting (<c>GET /posting-rules</c>).
    /// </summary>
    public class PostingRulePagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;

        public Guid? LegalEntityId { get; set; }

        public Guid? EventTypeId { get; set; }

        public Guid? JournalTypeId { get; set; }

        /// <summary>Angka enum <see cref="AccountingEventTreatment"/>: 1 langsung disahkan, 2 buat draft.</summary>
        public AccountingEventTreatment? Treatment { get; set; }

        public bool? IsActive { get; set; }

        /// <summary>Dicocokkan ke kode maupun nama jenis kejadian.</summary>
        public string? Search { get; set; }

        /// <summary><c>eventTypeCode</c> (bawaan), <c>journalTypeCode</c>, atau <c>createDateTime</c>.</summary>
        public string? SortBy { get; set; }

        public string? SortDirection { get; set; }
    }

    /// <summary>
    /// Satu baris daftar aturan posting.
    /// </summary>
    public class PostingRuleListResponse
    {
        public Guid Id { get; set; }

        public Guid LegalEntityId { get; set; }

        public string LegalEntityName { get; set; } = string.Empty;

        public Guid EventTypeId { get; set; }

        public string EventTypeCode { get; set; } = string.Empty;

        public string EventTypeName { get; set; } = string.Empty;

        public Guid JournalTypeId { get; set; }

        public string JournalTypeCode { get; set; } = string.Empty;

        public string JournalTypeName { get; set; } = string.Empty;

        public AccountingEventTreatment Treatment { get; set; }

        public bool IsActive { get; set; }

        public int LineCount { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    /// <summary>
    /// Rincian satu aturan posting beserta seluruh barisnya.
    /// </summary>
    public class PostingRuleDetailResponse : PostingRuleListResponse
    {
        public Guid CreateBy { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        /// <summary><c>Guid.Empty</c> bila belum pernah diubah.</summary>
        public Guid UpdateBy { get; set; }

        public List<PostingRuleLineResponse> Lines { get; set; } = new();
    }

    public class PostingRuleLineResponse
    {
        public Guid Id { get; set; }

        public int LineNumber { get; set; }

        public string ComponentCode { get; set; } = string.Empty;

        public Guid AccountId { get; set; }

        public string AccountCode { get; set; } = string.Empty;

        public string AccountName { get; set; } = string.Empty;

        public AccountType AccountType { get; set; }

        /// <summary>
        /// Penanda saja. Aturan posting <b>boleh</b> menunjuk control account — jalur otomatis
        /// adalah jalur yang sah menuju akun itu (<c>ACC-DEC-064</c>).
        /// </summary>
        public bool IsControlAccount { get; set; }

        public Guid? CostCenterId { get; set; }

        public string? CostCenterName { get; set; }

        /// <summary>Angka enum <see cref="PostingSide"/>: 1 debit, 2 kredit.</summary>
        public PostingSide Side { get; set; }

        public string? Description { get; set; }
    }

    /// <summary>
    /// Menambah aturan posting. Aturan baru selalu lahir aktif (kamus data bagian 12).
    /// </summary>
    public class CreatePostingRuleRequest
    {
        [Required]
        public Guid LegalEntityId { get; set; }

        [Required]
        public Guid EventTypeId { get; set; }

        /// <summary>Jenis jurnal yang dihasilkan aturan ini (<c>ACC-DEC-074</c>).</summary>
        [Required]
        public Guid JournalTypeId { get; set; }

        /// <summary>Bawaan buat draft — yang lebih aman bila tidak diisi.</summary>
        public AccountingEventTreatment Treatment { get; set; } = AccountingEventTreatment.BuatDraft;

        public List<PostingRuleLineRequest> Lines { get; set; } = new();
    }

    /// <summary>
    /// Mengubah jenis jurnal, perlakuan, dan seluruh baris. Badan hukum dan jenis kejadian
    /// <b>tidak</b> dapat diubah: keduanya identitas aturan. Untuk memetakan jenis kejadian lain,
    /// buat aturan baru. Baris dikirim utuh dan menggantikan seluruh baris sebelumnya.
    /// </summary>
    public class UpdatePostingRuleRequest
    {
        [Required]
        public Guid JournalTypeId { get; set; }

        public AccountingEventTreatment Treatment { get; set; } = AccountingEventTreatment.BuatDraft;

        public List<PostingRuleLineRequest> Lines { get; set; } = new();
    }

    public class PostingRuleLineRequest
    {
        public int LineNumber { get; set; }

        /// <summary>
        /// Kode komponen nilai, contoh <c>JASA_MEDIS</c>. Kosong berarti <c>TOTAL</c> — nilai
        /// total kejadian.
        /// </summary>
        [MaxLength(50)]
        public string? ComponentCode { get; set; }

        [Required]
        public Guid AccountId { get; set; }

        /// <summary>Wajib bila akunnya berjenis beban (<c>ACC-DEC-019</c>).</summary>
        public Guid? CostCenterId { get; set; }

        /// <summary>Angka enum <see cref="PostingSide"/>: 1 debit, 2 kredit.</summary>
        public PostingSide Side { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
