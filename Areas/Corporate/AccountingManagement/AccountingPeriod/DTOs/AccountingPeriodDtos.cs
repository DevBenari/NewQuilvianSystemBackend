using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.DTOs
{
    public class AccountingPeriodPagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;

        public Guid? LegalEntityId { get; set; }

        public int? FiscalYear { get; set; }

        public AccountingPeriodStatus? PeriodStatus { get; set; }

        public string? SortDirection { get; set; }
    }

    public class AccountingPeriodResponse
    {
        public Guid Id { get; set; }

        public Guid LegalEntityId { get; set; }

        /// <summary>Bentuk <c>2027-01</c>, tepat tujuh karakter (`ACC-DEC-013`).</summary>
        public string PeriodCode { get; set; } = string.Empty;

        public int FiscalYear { get; set; }

        public int PeriodMonth { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public AccountingPeriodStatus PeriodStatus { get; set; }

        /// <summary>Nama yang dibaca pengguna, misalnya "September 2026".</summary>
        public string PeriodName { get; set; } = string.Empty;

        public Guid? ClosedBy { get; set; }

        public DateTime? ClosedAt { get; set; }

        public Guid? ReopenedBy { get; set; }

        public DateTime? ReopenedAt { get; set; }

        /// <summary>Alasan penutupan atau pembukaan kembali yang terakhir tercatat.</summary>
        public string? LastReasonNote { get; set; }

        /// <summary>
        /// Jenis jurnal yang masih diterima periode ini, sesuai `ACC-STATE-0.1` bagian 2.2.
        /// Disertakan supaya layar tidak perlu menyalin tabel itu sendiri.
        /// </summary>
        public List<string> AcceptedJournalTypeCodes { get; set; } = new();
    }

    public class GenerateAccountingPeriodRequest
    {
        [Required]
        public Guid LegalEntityId { get; set; }

        /// <summary>Tahun buku, misalnya 2027. Dua belas periode dibangkitkan sekaligus.</summary>
        [Required]
        public int FiscalYear { get; set; }
    }

    /// <summary>
    /// Penutupan periode. <see cref="Permanent"/> membedakan tutup sementara dari tutup permanen.
    /// </summary>
    public class ClosePeriodRequest
    {
        /// <summary>
        /// <c>false</c> menghasilkan <c>SoftClosed</c>, <c>true</c> menghasilkan <c>Closed</c>.
        /// </summary>
        public bool Permanent { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Pembukaan kembali. Alasan **wajib** — `ACC-DEC-027`.
    /// </summary>
    public class ReopenPeriodRequest
    {
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}
