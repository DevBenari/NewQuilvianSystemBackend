using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Reconciliation.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Reconciliation.DTOs
{
    /// <summary>
    /// Penyaring saldo control account.
    /// </summary>
    public class ControlAccountBalanceQuery
    {
        /// <summary>
        /// Badan hukum yang bukunya dibaca. Wajib — saldo dua badan hukum tidak boleh tercampur.
        /// </summary>
        public Guid LegalEntityId { get; set; }

        /// <summary>
        /// Saldo dihitung sampai tanggal ini, termasuk tanggalnya. Kosong berarti seluruh
        /// riwayat.
        /// </summary>
        public DateTime? AsOfDate { get; set; }
    }

    /// <summary>
    /// Saldo buku besar seluruh control account sebuah badan hukum pada satu titik waktu.
    /// </summary>
    /// <remarks>
    /// Ini <b>separuh</b> dari rekonsiliasi: sisi buku besar. Pembandingnya dengan saldo subledger
    /// adalah <c>BE-ACC-P2-014</c>, yang masih terblokir <c>DEC-ACC-P2-011</c> — belum diputuskan
    /// dari mana Accounting memperoleh saldo subledger.
    /// </remarks>
    public class ControlAccountBalanceReportResponse
    {
        public Guid LegalEntityId { get; set; }

        /// <summary>Tanggal batas yang dipakai; kosong berarti seluruh riwayat.</summary>
        public DateTime? AsOfDate { get; set; }

        /// <summary>Waktu perhitungan dijalankan; angkanya tidak pernah disimpan.</summary>
        public DateTime EvaluatedAt { get; set; }

        public int AccountCount { get; set; }

        public decimal TotalDebit { get; set; }

        public decimal TotalCredit { get; set; }

        public List<ControlAccountBalanceResponse> Accounts { get; set; } = new();
    }

    /// <summary>
    /// Saldo satu control account.
    /// </summary>
    public class ControlAccountBalanceResponse
    {
        public Guid AccountId { get; set; }

        public string AccountCode { get; set; } = string.Empty;

        public string AccountName { get; set; } = string.Empty;

        public AccountType AccountType { get; set; }

        public NormalBalance NormalBalance { get; set; }

        public decimal TotalDebit { get; set; }

        public decimal TotalCredit { get; set; }

        /// <summary>
        /// Debit dikurangi kredit. Positif berarti condong debit — bentuk yang sama persis dengan
        /// <c>AccChartOfAccountService.HitungSaldoAsync</c>.
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Saldo menurut saldo normal akunnya, sehingga selalu positif bila akun berperilaku
        /// wajar.
        /// </summary>
        /// <remarks>
        /// Untuk akun bersaldo normal kredit — Hutang, misalnya — <see cref="Balance"/> bernilai
        /// negatif ketika akun itu justru sehat. Menyodorkan angka negatif ke layar rekonsiliasi
        /// membuat pembacanya mengira ada yang salah. Bidang ini membalik tandanya sesuai saldo
        /// normal, dan <see cref="Balance"/> tetap disediakan apa adanya untuk yang membutuhkan
        /// bentuk mentahnya.
        /// </remarks>
        public decimal BalanceInNormalBalance { get; set; }

        /// <summary>Jumlah baris jurnal `Posted` yang membentuk saldo di atas.</summary>
        public int PostedLineCount { get; set; }
    }

    public class SubledgerComparisonQuery
    {
        public Guid AccountingPeriodId { get; set; }
    }

    public class SubledgerComparisonReportResponse
    {
        public Guid LegalEntityId { get; set; }

        public Guid AccountingPeriodId { get; set; }

        public string PeriodCode { get; set; } = string.Empty;

        public string PeriodName { get; set; } = string.Empty;

        public AccountingPeriodStatus PeriodStatus { get; set; }

        public DateTime PeriodEndDate { get; set; }

        public DateTime EvaluatedAt { get; set; }

        public SubledgerReconciliationState ReconciliationState { get; set; }

        public string? StartPeriodCode { get; set; }

        public string StateMessage { get; set; } = string.Empty;

        public int AccountCount { get; set; }

        public int RequiredAccountCount { get; set; }

        public int MatchedCount { get; set; }

        public int DifferenceCount { get; set; }

        public int NotReceivedCount { get; set; }

        public int CutOffMismatchCount { get; set; }

        public int BlockingCount { get; set; }

        public List<SubledgerComparisonAccountResponse> Accounts { get; set; } = new();
    }

    public class SubledgerComparisonAccountResponse
    {
        public Guid AccountId { get; set; }

        public string AccountCode { get; set; } = string.Empty;

        public string AccountName { get; set; } = string.Empty;

        public AccountType AccountType { get; set; }

        public NormalBalance NormalBalance { get; set; }

        public bool IsActive { get; set; }

        public bool IsPostable { get; set; }

        public bool IsRequired { get; set; }

        public decimal GlBalance { get; set; }

        public int PostedLineCount { get; set; }

        public decimal? SubledgerBalance { get; set; }

        public DateTime? SubledgerAsOfDate { get; set; }

        public int? SubledgerVersionNumber { get; set; }

        public Guid? SubledgerAccountingEventId { get; set; }

        public string? SubledgerEventNumber { get; set; }

        public DateTime? SubledgerRecordedAt { get; set; }

        public decimal? Difference { get; set; }

        public SubledgerReconciliationItemStatus ItemStatus { get; set; }

        public bool IsBlocking { get; set; }
    }
}
