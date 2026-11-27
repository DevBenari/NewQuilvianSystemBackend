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

    /// <summary>
    /// Opsional. <c>null</c> berarti "tidak dikirim": pada create dipakai bawaan 2, pada update nilai tersimpan
    /// dipertahankan. Dahulu bertipe <c>int</c> dengan bawaan 2, sehingga klien yang tidak mengirim field ini
    /// diam-diam menimpa <c>DecimalPlaces</c> yang sudah ada menjadi 2.
    /// </summary>
    [Range(0, 6)] public int? DecimalPlaces { get; set; }

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

    /// <summary>
    /// Rupiah untuk satu unit mata uang ini (kurs tengah) pada tanggal referensi hari ini. <c>1</c> untuk IDR.
    /// <c>null</c> berarti belum ada kurs yang berlaku — kurs bertanggal masa depan dan kurs bernilai 0 tidak dihitung.
    /// Bukan angka 0 pengganti "belum ada".
    /// </summary>
    public decimal? CurrentRateToIdr { get; set; }

    /// <summary><c>RateDate</c> kurs yang dipakai pada <see cref="CurrentRateToIdr"/>. <c>null</c> untuk IDR atau bila belum ada kurs.</summary>
    public DateOnly? CurrentRateDate { get; set; }

    public string? CurrentRateSource { get; set; }

    /// <summary>Dasar kurs: <see cref="CurrencyRateBases"/>; <c>null</c> bila belum ada kurs.</summary>
    public string? CurrentRateBasis { get; set; }

    /// <summary>Jumlah baris riwayat kurs yang belum terhapus (semua tanggal). Petunjuk tampilan; penegak aturan hapus tetap backend.</summary>
    public int ExchangeRateCount { get; set; }

    /// <summary>Aktivitas terbaru pada master atau kurs; lihat <see cref="LastUpdatedSource"/>.</summary>
    public DateTime LastUpdatedAt { get; set; }

    /// <summary>Nama pelaku aktivitas terbaru dari audit; <c>null</c> bila pengguna tidak ditemukan (bukan nama pengguna yang sedang login).</summary>
    public string? LastUpdatedByName { get; set; }

    /// <summary><see cref="CurrencyLastUpdatedSources"/>: perubahan master, atau pencatatan kurs.</summary>
    public string LastUpdatedSource { get; set; } = CurrencyLastUpdatedSources.Currency;
}

public static class CurrencyRateBases
{
    public const string Identity = "IDENTITY";
    public const string Middle = "MIDDLE";
}

public static class CurrencyLastUpdatedSources
{
    public const string Currency = "CURRENCY";
    public const string ExchangeRate = "EXCHANGE_RATE";
}

public sealed class CurrencyDeleteResponse
{
    public Guid Id { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencyName { get; set; } = string.Empty;
    public bool IsDelete { get; set; }
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
