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

    public class BalanceSheetQuery
    {
        public Guid LegalEntityId { get; set; }

        public string? PeriodCode { get; set; }

        public bool IncludeZeroAccounts { get; set; }
    }

    public class BalanceSheetResponse
    {
        public Guid LegalEntityId { get; set; }

        public string PeriodCode { get; set; } = string.Empty;

        public string PeriodName { get; set; } = string.Empty;

        public int FiscalYear { get; set; }

        public DateTime AsOfDate { get; set; }

        public bool ComparisonAvailable { get; set; }

        public DateTime? ComparisonAsOfDate { get; set; }

        public string? ComparisonUnavailableReason { get; set; }

        public List<FinancialStatementSectionResponse> Sections { get; set; } = new();

        public FinancialStatementAmountsResponse TotalLiabilitiesAndEquity { get; set; } = new();

        public bool IsBalanced { get; set; }

        public bool? ComparisonIsBalanced { get; set; }

        public decimal Difference { get; set; }

        public decimal? ComparisonDifference { get; set; }

        public List<int> UnclosedFiscalYears { get; set; } = new();

        public int UnclassifiedAccountCount { get; set; }

        public List<FinancialStatementClassificationGapResponse> ClassificationGaps { get; set; } = new();
    }

    public class FinancialRatioQuery
    {
        public Guid LegalEntityId { get; set; }

        public string? PeriodCode { get; set; }
    }

    public class FinancialRatioResponse
    {
        public Guid LegalEntityId { get; set; }

        public string PeriodCode { get; set; } = string.Empty;

        public string PeriodName { get; set; } = string.Empty;

        public int FiscalYear { get; set; }

        public DateTime AsOfDate { get; set; }

        public DateTime YearToDateStartDate { get; set; }

        public bool ComparisonAvailable { get; set; }

        public DateTime? ComparisonAsOfDate { get; set; }

        public string? ComparisonUnavailableReason { get; set; }

        public bool IsBalanced { get; set; }

        public bool? ComparisonIsBalanced { get; set; }

        public List<FinancialStatementClassificationGapResponse> ClassificationGaps { get; set; } = new();

        public List<FinancialStatementClassificationGapResponse> ComparisonClassificationGaps { get; set; } = new();

        public List<FinancialRatioRowResponse> Ratios { get; set; } = new();
    }

    public class FinancialRatioRowResponse
    {
        public FinancialRatioCode Code { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Formula { get; set; } = string.Empty;

        public FinancialRatioUnit Unit { get; set; }

        public FinancialRatioValueResponse Current { get; set; } = new();

        public FinancialRatioValueResponse Comparison { get; set; } = new();
    }

    public class FinancialRatioValueResponse
    {
        public FinancialRatioStatus Status { get; set; }

        public decimal? Value { get; set; }

        public decimal? Numerator { get; set; }

        public decimal? Denominator { get; set; }

        public decimal? DenominatorOpening { get; set; }

        public decimal? DenominatorClosing { get; set; }

        public string? Reason { get; set; }

        public string? Note { get; set; }
    }

    public class FinancialStatementClassificationGapResponse
    {
        public Guid AccountId { get; set; }

        public string AccountCode { get; set; } = string.Empty;

        public string AccountName { get; set; } = string.Empty;

        public AccountType AccountType { get; set; }

        public ClassificationGapKind GapKind { get; set; }
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
