using System.Globalization;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Services;

public static class AssetValuationErrorCodes
{
    public const string ParameterAmount = "AST-VAL-NLI-001";
    public const string EffectivePeriod = "AST-VAL-NLI-002";
    public const string DuplicatePeriod = "AST-VAL-NLI-003";
    public const string PolicyRange = "AST-VAL-NLI-004";
    public const string GroupNotDepreciable = "AST-VAL-NLI-005";
    public const string ParameterMissing = "AST-VAL-NLI-008";
    public const string OpeningBalanceExhausted = "AST-VAL-NLI-040";
    public const string OpeningAccumulation = "AST-VAL-REG-064";
    public const string Field = "AST-NLI-FIELD";
    public const string NotFound = "AST-NLI-NOT-FOUND";
    public const string EventCode = "AST-NLI-EVENT-CODE";
}

public static class AssetValuationMessages
{
    public const string ParameterAmount = "Batas wajib angka rupiah 0 atau lebih.";
    public const string EffectivePeriodFormat = "Parameter baru berlaku mulai {0}.";
    public const string PolicyEffectivePeriodFormat = "Kebijakan baru berlaku mulai {0}.";
    public const string DuplicateParameterFormat = "Sudah ada parameter yang akan berlaku pada {0}. Ubah bulan depan atau hubungi administrator.";
    public const string DuplicatePolicyFormat = "Sudah ada kebijakan kelompok ini yang akan berlaku pada {0}. Ubah bulan depan atau hubungi administrator.";
    public const string PolicyRange = "Umur manfaat 1–600 bulan dan nilai residu 0–100%.";
    public const string GroupNotDepreciable = "Kelompok ini tidak disusutkan.";
    public const string ParameterMissing = "Parameter nilai belum ditetapkan Akuntansi; aset belum dapat disetujui.";
    public const string OpeningBalanceExhausted = "Umur sudah habis tetapi nilai buku masih tersisa; periksa akumulasi.";
    public const string OpeningAccumulation = "Akumulasi penyusutan melebihi nilai yang dapat disusutkan.";
    public const string GroupNotFound = "Kelompok aset tidak ditemukan atau sudah dihapus.";
    public const string GroupRequired = "Kelompok aset wajib dipilih.";
    public const string EventCodeFormat = "Kode kejadian {0} tidak dikenal modul aset.";
}

public static class AssetValuationPeriod
{
    private static readonly CultureInfo Indonesian = CultureInfo.GetCultureInfo("id-ID");

    public static DateOnly Of(DateOnly date) => new(date.Year, date.Month, 1);

    public static DateOnly Current() => Of(FinanceBusinessDate.Today());

    public static DateOnly Next(DateOnly period) => Of(period).AddMonths(1);

    public static DateOnly OfInstant(DateTime instant) => Of(FinanceBusinessDate.ToDateOnly(instant));

    public static int MonthsBetween(DateOnly fromPeriod, DateOnly toPeriod) =>
        (toPeriod.Year - fromPeriod.Year) * 12 + toPeriod.Month - fromPeriod.Month;

    public static string Format(DateOnly period) => period.ToString("MMMM yyyy", Indonesian);

    public static bool IsFirstOfMonth(DateOnly date) => date.Day == 1;
}

public static class AssetValuationMoney
{
    public static bool HasAtMostTwoDecimals(decimal value) => decimal.Round(value, 2) == value;

    public static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
