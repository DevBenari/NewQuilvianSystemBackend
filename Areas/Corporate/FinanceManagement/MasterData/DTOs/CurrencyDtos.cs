using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Dtos;

public sealed class CurrencyQuery
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public string SortBy { get; set; } = "currencyName";
    public string SortDirection { get; set; } = "asc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public class CreateCurrencyRequest
{
    [Required, MaxLength(3)] public string CurrencyCode { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string CurrencyName { get; set; } = string.Empty;
    [MaxLength(10)] public string? Symbol { get; set; }
    [Range(0, 6)] public int DecimalPlaces { get; set; } = 2;
    public bool IsBaseCurrency { get; set; } = false;
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateCurrencyRequest : CreateCurrencyRequest;

public sealed class UpdateCurrencyStatusRequest
{
    public bool IsActive { get; set; }
}

public sealed class CurrencyResponse
{
    public Guid Id { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencyName { get; set; } = string.Empty;
    public string? Symbol { get; set; }
    public int DecimalPlaces { get; set; }
    public bool IsBaseCurrency { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreateDateTime { get; set; }
    public DateTime? UpdateDateTime { get; set; }
}

public sealed class CurrencyOptionResponse
{
    public Guid Id { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencyName { get; set; } = string.Empty;
    public string? Symbol { get; set; }
    public bool IsBaseCurrency { get; set; }
}

public sealed class CurrencySummaryResponse
{
    public int TotalCurrency { get; set; }
    public int ActiveCurrency { get; set; }
    public int InactiveCurrency { get; set; }
}

public sealed class CurrencyDefaultFilterResponse
{
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "currencyName";
    public string SortDirection { get; set; } = "asc";
}

public sealed class CurrencyFilterMetadataResponse
{
    public CurrencyDefaultFilterResponse DefaultFilter { get; set; } = new();
    public List<int> PageSizeOptions { get; set; } = new();
    public List<string> SortableFields { get; set; } = new();
}

public sealed class ExchangeRateQuery
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class CreateExchangeRateRequest
{
    [Required] public DateOnly RateDate { get; set; }
    [Range(0.01, double.MaxValue)] public decimal BuyRate { get; set; }
    [Range(0.01, double.MaxValue)] public decimal SellRate { get; set; }
    [Range(0.01, double.MaxValue)] public decimal MiddleRate { get; set; }
    [MaxLength(100)] public string? Source { get; set; }
}

public sealed class ExchangeRateResponse
{
    public Guid Id { get; set; }
    public Guid CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public DateOnly RateDate { get; set; }
    public decimal BuyRate { get; set; }
    public decimal SellRate { get; set; }
    public decimal MiddleRate { get; set; }
    public string? Source { get; set; }
    public DateTime CreateDateTime { get; set; }
}
