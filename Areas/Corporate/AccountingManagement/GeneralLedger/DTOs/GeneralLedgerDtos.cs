namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.DTOs
{
    public class LedgerMovementQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;

        public Guid LegalEntityId { get; set; }

        public Guid AccountId { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }
    }

    /// <summary>
    /// Satu mutasi buku besar. <see cref="RunningBalance"/> adalah saldo <b>sesudah</b> baris ini.
    /// </summary>
    public class LedgerMovementResponse
    {
        public DateTime AccountingDate { get; set; }

        public string JournalNumber { get; set; } = string.Empty;

        public int LineNumber { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal DebitAmount { get; set; }

        public decimal CreditAmount { get; set; }

        /// <summary>
        /// Saldo berjalan. Positif berarti condong debit — konvensi yang sama dengan
        /// <c>AccChartOfAccountService.HitungSaldoAsync</c>.
        /// </summary>
        public decimal RunningBalance { get; set; }
    }

    public class TrialBalanceQuery
    {
        public Guid LegalEntityId { get; set; }

        /// <summary>Bentuk <c>2026-09</c>.</summary>
        public string PeriodCode { get; set; } = string.Empty;
    }

    public class TrialBalanceResponse
    {
        public string PeriodCode { get; set; } = string.Empty;

        public string PeriodName { get; set; } = string.Empty;

        public List<TrialBalanceRowResponse> Rows { get; set; } = new();

        public decimal TotalDebit { get; set; }

        public decimal TotalCredit { get; set; }

        /// <summary>
        /// <c>true</c> bila total debit sama persis dengan total kredit. Pada data yang sehat ini
        /// selalu <c>true</c>: setiap jurnal yang disahkan wajib seimbang, dan jumlah dari
        /// himpunan yang seluruhnya seimbang pasti seimbang. <c>false</c> berarti ada kerusakan
        /// data, bukan sekadar laporan yang tidak rapi.
        /// </summary>
        public bool IsBalanced { get; set; }
    }

    public class TrialBalanceRowResponse
    {
        public Guid AccountId { get; set; }

        public string AccountCode { get; set; } = string.Empty;

        public string AccountName { get; set; } = string.Empty;

        public decimal OpeningBalance { get; set; }

        public decimal TotalDebit { get; set; }

        public decimal TotalCredit { get; set; }

        public decimal ClosingBalance { get; set; }
    }

    public class AccountBalanceResponse
    {
        public Guid AccountId { get; set; }

        public string AccountCode { get; set; } = string.Empty;

        public string AccountName { get; set; } = string.Empty;

        public string PeriodCode { get; set; } = string.Empty;

        public string PeriodName { get; set; } = string.Empty;

        public decimal OpeningBalance { get; set; }

        public decimal TotalDebit { get; set; }

        public decimal TotalCredit { get; set; }

        public decimal ClosingBalance { get; set; }
    }
}
