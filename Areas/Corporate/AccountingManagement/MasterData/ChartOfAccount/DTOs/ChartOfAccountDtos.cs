using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.DTOs
{
    /// <summary>
    /// Penyaring daftar akun. Seluruh field boleh kosong; yang kosong berarti tidak menyaring.
    /// </summary>
    public class ChartOfAccountPagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;

        public Guid? LegalEntityId { get; set; }

        public AccountType? AccountType { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsPostable { get; set; }

        /// <summary>Dicocokkan ke kode maupun nama akun.</summary>
        public string? Search { get; set; }

        public string? SortBy { get; set; }

        public string? SortDirection { get; set; }
    }

    public class ChartOfAccountListResponse
    {
        public Guid Id { get; set; }

        public Guid LegalEntityId { get; set; }

        public string AccountCode { get; set; } = string.Empty;

        public string AccountName { get; set; } = string.Empty;

        public AccountType AccountType { get; set; }

        public NormalBalance NormalBalance { get; set; }

        public int AccountLevel { get; set; }

        public Guid? ParentAccountId { get; set; }

        public string? ParentAccountCode { get; set; }

        public bool IsPostable { get; set; }

        public bool IsActive { get; set; }
    }

    public class ChartOfAccountDetailResponse : ChartOfAccountListResponse
    {
        public string? ParentAccountName { get; set; }

        public string? Description { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        /// <summary>
        /// Benar bila akun punya turunan. Dipakai layar untuk menyembunyikan pilihan
        /// "menerima transaksi", sehingga <c>ACC-DEC-022</c> terjaga sejak di layar.
        /// </summary>
        public bool HasChildAccounts { get; set; }

        /// <summary>
        /// Benar bila akun sudah dipakai baris jurnal **yang disahkan**. Jurnal `Draft` tidak
        /// dihitung — ia belum menjadi transaksi dan tidak boleh mengunci kode akun.
        /// </summary>
        public bool HasPostedJournalLines { get; set; }

        /// <summary>
        /// Diturunkan dari <c>AccountType == Expense</c> (`ACC-DEC-019`), bukan disimpan.
        /// Roadmap `BE-ACC-003` melarang kolom `RequiresCostCenter` pada entity.
        /// </summary>
        public bool RequiresCostCenter { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class ChartOfAccountTreeResponse
    {
        public Guid Id { get; set; }

        public string AccountCode { get; set; } = string.Empty;

        public string AccountName { get; set; } = string.Empty;

        public AccountType AccountType { get; set; }

        public NormalBalance NormalBalance { get; set; }

        public int AccountLevel { get; set; }

        public bool IsPostable { get; set; }

        public bool IsActive { get; set; }

        public List<ChartOfAccountTreeResponse> Children { get; set; } = new();
    }

    /// <summary>
    /// Isian pilihan pada form jurnal. Hanya memuat akun yang menerima transaksi dan aktif.
    /// </summary>
    public class ChartOfAccountOptionResponse
    {
        public Guid Id { get; set; }

        public string AccountCode { get; set; } = string.Empty;

        public string AccountName { get; set; } = string.Empty;

        public AccountType AccountType { get; set; }

        public NormalBalance NormalBalance { get; set; }

        /// <summary>Diturunkan dari jenis akun, tidak disimpan (`ACC-DEC-019`).</summary>
        public bool RequiresCostCenter { get; set; }
    }

    public class CreateChartOfAccountRequest
    {
        [Required]
        public Guid LegalEntityId { get; set; }

        [Required]
        [MaxLength(20)]
        public string AccountCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string AccountName { get; set; } = string.Empty;

        [Required]
        public AccountType AccountType { get; set; }

        /// <summary>
        /// Berdiri sendiri, tidak diturunkan dari <see cref="AccountType"/>, supaya akun kontra
        /// dapat ditangani.
        /// </summary>
        [Required]
        public NormalBalance NormalBalance { get; set; }

        public Guid? ParentAccountId { get; set; }

        public int AccountLevel { get; set; } = 1;

        public bool IsPostable { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
    }

    /// <summary>
    /// <c>AccountCode</c> ikut dapat diubah selama akun belum dipakai jurnal yang disahkan.
    /// Lihat catatan delta kontrak pada laporan `BE-ACC-007`.
    /// </summary>
    public class UpdateChartOfAccountRequest
    {
        [Required]
        [MaxLength(20)]
        public string AccountCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string AccountName { get; set; } = string.Empty;

        public Guid? ParentAccountId { get; set; }

        public int AccountLevel { get; set; } = 1;

        public bool IsPostable { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
    }

    public class DeactivateChartOfAccountRequest
    {
        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
