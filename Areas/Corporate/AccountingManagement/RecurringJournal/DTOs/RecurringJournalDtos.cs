using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.DTOs
{
    public class RecurringJournalPagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;

        public Guid? LegalEntityId { get; set; }

        public Guid? JournalTypeId { get; set; }

        public bool? IsActive { get; set; }

        /// <summary>Dicocokkan pada kode dan nama template.</summary>
        public string? Search { get; set; }

        public string? SortBy { get; set; }

        public string? SortDirection { get; set; }
    }

    public class RecurringJournalListResponse
    {
        public Guid Id { get; set; }

        public Guid LegalEntityId { get; set; }

        public string TemplateCode { get; set; } = string.Empty;

        public string TemplateName { get; set; } = string.Empty;

        public Guid JournalTypeId { get; set; }

        public string JournalTypeCode { get; set; } = string.Empty;

        public string JournalTypeName { get; set; } = string.Empty;

        public RecurringFrequency Frequency { get; set; }

        public int DayOfMonth { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; }

        /// <summary>Jumlah baris template. Dihitung saat baca, tidak dipersistensi.</summary>
        public int LineCount { get; set; }

        /// <summary>
        /// SENSITIF. Total debit baris template — sama dengan total kreditnya, karena template
        /// yang timpang tidak dapat disimpan.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Jumlah periode yang sudah diterbitkan template ini. Dipakai layar untuk menandai
        /// template yang belum pernah terbit sama sekali.
        /// </summary>
        public int RunCount { get; set; }
    }

    public class RecurringJournalDetailResponse
    {
        public Guid Id { get; set; }

        public Guid LegalEntityId { get; set; }

        public string TemplateCode { get; set; } = string.Empty;

        public string TemplateName { get; set; } = string.Empty;

        public Guid JournalTypeId { get; set; }

        public string JournalTypeCode { get; set; } = string.Empty;

        public string JournalTypeName { get; set; } = string.Empty;

        public RecurringFrequency Frequency { get; set; }

        public int DayOfMonth { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; }

        /// <summary>SENSITIF. Total debit baris template.</summary>
        public decimal TotalDebit { get; set; }

        /// <summary>SENSITIF. Total kredit baris template.</summary>
        public decimal TotalCredit { get; set; }

        /// <summary>
        /// Selalu <c>true</c> pada template tersimpan — template timpang ditolak sejak disimpan
        /// (<c>ACC-VALIDATION-0.6</c> bagian 3). Disertakan supaya layar tidak perlu
        /// membandingkan dua angka sendiri.
        /// </summary>
        public bool IsBalanced { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        public List<RecurringJournalLineResponse> Lines { get; set; } = new();
    }

    public class RecurringJournalLineResponse
    {
        public Guid Id { get; set; }

        public int LineNumber { get; set; }

        public Guid AccountId { get; set; }

        public string AccountCode { get; set; } = string.Empty;

        public string AccountName { get; set; } = string.Empty;

        public Guid? CostCenterId { get; set; }

        public string? CostCenterName { get; set; }

        public string? Description { get; set; }

        /// <summary>SENSITIF. Nol bila baris ini kredit.</summary>
        public decimal DebitAmount { get; set; }

        /// <summary>SENSITIF. Nol bila baris ini debit.</summary>
        public decimal CreditAmount { get; set; }
    }

    /// <summary>
    /// Satu penerbitan template untuk satu periode, beserta jurnal yang dihasilkannya.
    /// </summary>
    /// <remarks>
    /// Barisnya <b>tidak pernah</b> diubah maupun dihapus: ia bukti bahwa periode itu sudah
    /// diterbitkan, dan bukti itulah yang mencegah penerbitan kedua.
    /// </remarks>
    public class RecurringJournalRunResponse
    {
        public Guid Id { get; set; }

        public Guid TemplateId { get; set; }

        public Guid AccountingPeriodId { get; set; }

        /// <summary>Bentuk <c>2026-09</c>.</summary>
        public string PeriodCode { get; set; } = string.Empty;

        public Guid JournalId { get; set; }

        public string JournalNumber { get; set; } = string.Empty;

        /// <summary>
        /// Status jurnal yang dihasilkan <b>saat ini</b>, bukan saat diterbitkan. Jurnalnya lahir
        /// <c>Draft</c> lalu menempuh daur hidupnya sendiri.
        /// </summary>
        public JournalManagement.Enums.JournalStatus JournalStatus { get; set; }

        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Menambah template beserta seluruh barisnya sekaligus.
    /// </summary>
    /// <remarks>
    /// <b>Template baru selalu lahir tidak aktif</b> (<c>ACC-DEC-050</c>), dan karena itu
    /// permintaan ini <b>tidak punya</b> bidang <c>IsActive</c>. Menyediakannya berarti membuka
    /// jalan template lahir langsung aktif dan menerbitkan jurnal pada siklus penjadwal
    /// berikutnya — sebelum satu orang pun sempat memeriksa barisnya. Pengaktifan punya
    /// endpointnya sendiri.
    /// </remarks>
    public class CreateRecurringJournalRequest
    {
        [Required]
        public Guid LegalEntityId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TemplateCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TemplateName { get; set; } = string.Empty;

        [Required]
        public Guid JournalTypeId { get; set; }

        public RecurringFrequency Frequency { get; set; } = RecurringFrequency.Bulanan;

        /// <summary>
        /// Tanggal terbit setiap bulan, dibatasi 1 sampai 28. Tanggal 29, 30, dan 31 tidak ada di
        /// setiap bulan, sehingga template bertanggal itu akan terlewat pada Februari tanpa
        /// menimbulkan error apa pun.
        /// </summary>
        [Range(1, 28)]
        public int DayOfMonth { get; set; } = 1;

        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>Kosong berarti berlaku tanpa batas waktu.</summary>
        public DateTime? EndDate { get; set; }

        /// <summary>Dikirim <b>utuh</b>. Tidak ada jalur penambahan baris satu per satu.</summary>
        public List<CreateRecurringJournalLineRequest> Lines { get; set; } = new();
    }

    /// <summary>
    /// Sama dengan <see cref="CreateRecurringJournalRequest"/> tanpa <c>LegalEntityId</c>: badan
    /// hukum sebuah template tidak dapat berpindah setelah dibuat, sama seperti jurnal.
    /// </summary>
    /// <remarks>
    /// <c>IsActive</c> juga tidak ada di sini. Mengubah isi template dan mengaktifkannya adalah
    /// dua tindakan yang berbeda beratnya, dan matriks hak akses membedakan keduanya.
    /// </remarks>
    public class UpdateRecurringJournalRequest
    {
        [Required]
        [MaxLength(50)]
        public string TemplateCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TemplateName { get; set; } = string.Empty;

        [Required]
        public Guid JournalTypeId { get; set; }

        public RecurringFrequency Frequency { get; set; } = RecurringFrequency.Bulanan;

        [Range(1, 28)]
        public int DayOfMonth { get; set; } = 1;

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        /// <summary>Menggantikan <b>seluruh</b> baris sebelumnya.</summary>
        public List<CreateRecurringJournalLineRequest> Lines { get; set; } = new();
    }

    public class CreateRecurringJournalLineRequest
    {
        public int LineNumber { get; set; }

        public Guid AccountId { get; set; }

        public Guid? CostCenterId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public decimal DebitAmount { get; set; }

        public decimal CreditAmount { get; set; }
    }

    /// <summary>
    /// Menerbitkan jurnal template untuk satu periode secara manual, di luar jadwal.
    /// Cakupan <c>BE-ACC-P2-008</c>.
    /// </summary>
    /// <remarks>
    /// Periodenya ditentukan dari <see cref="AccountingDate"/>, bukan dikirim sebagai
    /// <c>AccountingPeriodId</c>. Alasannya sama dengan jurnal manual: periode adalah turunan
    /// tanggal akuntansi, dan menerimanya langsung membuka jalan jurnal tercatat di periode yang
    /// tidak sesuai tanggalnya.
    /// </remarks>
    public class GenerateRecurringJournalRequest
    {
        /// <summary>
        /// Tanggal akuntansi jurnal yang diterbitkan. Kosong berarti memakai
        /// <c>DayOfMonth</c> template pada bulan berjalan.
        /// </summary>
        public DateTime? AccountingDate { get; set; }
    }
}
