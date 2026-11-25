using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.DTOs
{
    public class IncomeStatementQuery
    {
        public Guid LegalEntityId { get; set; }

        public string? PeriodCode { get; set; }

        public Guid? CostCenterId { get; set; }

        public bool IncludeZeroAccounts { get; set; }
    }

    public class IncomeStatementResponse
    {
        public Guid LegalEntityId { get; set; }

        public string PeriodCode { get; set; } = string.Empty;

        public string PeriodName { get; set; } = string.Empty;

        public int FiscalYear { get; set; }

        public DateTime PeriodStartDate { get; set; }

        public DateTime PeriodEndDate { get; set; }

        public DateTime YearToDateStartDate { get; set; }

        public bool ComparisonAvailable { get; set; }

        public string? ComparisonPeriodCode { get; set; }

        public string? ComparisonPeriodName { get; set; }

        public string? ComparisonUnavailableReason { get; set; }

        public Guid? CostCenterId { get; set; }

        public string? CostCenterCode { get; set; }

        public string? CostCenterName { get; set; }

        public string? CostCenterNotice { get; set; }

        public List<FinancialStatementSectionResponse> Sections { get; set; } = new();

        public FinancialStatementAmountsResponse NetIncome { get; set; } = new();
    }

    public class FinancialStatementSectionResponse
    {
        public AccountType AccountType { get; set; }

        public string Title { get; set; } = string.Empty;

        public List<FinancialStatementRowResponse> Rows { get; set; } = new();

        public FinancialStatementAmountsResponse Total { get; set; } = new();
    }

    public class FinancialStatementRowResponse
    {
        public FinancialStatementRowKind RowKind { get; set; }

        public Guid? AccountId { get; set; }

        public Guid? ParentAccountId { get; set; }

        public string AccountCode { get; set; } = string.Empty;

        public string AccountName { get; set; } = string.Empty;

        public int Depth { get; set; }

        public bool IsActive { get; set; }

        public FinancialStatementAmountsResponse Amounts { get; set; } = new();
    }

    public class FinancialStatementAmountsResponse
    {
        public decimal Current { get; set; }

        public decimal? Comparison { get; set; }

        public decimal? Variance { get; set; }

        public decimal? CurrentYearToDate { get; set; }

        public decimal? ComparisonYearToDate { get; set; }

        public decimal? VarianceYearToDate { get; set; }
    }
}
