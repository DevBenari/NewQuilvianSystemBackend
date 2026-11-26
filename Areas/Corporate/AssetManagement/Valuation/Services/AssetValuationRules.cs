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
    public const string RunFuturePeriod = "AST-VAL-NLI-010";
    public const string RunAlreadyProcessed = "AST-VAL-NLI-011";
    public const string RunSequence = "AST-VAL-NLI-012";
    public const string RunPreviewErrors = "AST-VAL-NLI-013";
    public const string RunCostCenterUnclear = "AST-VAL-NLI-014";
    public const string RunGroupWithoutPolicy = "AST-VAL-NLI-015";
    public const string RunGroupWithoutAccount = "AST-VAL-NLI-016";
    public const string AccountCode = "AST-VAL-NLI-017";
    public const string CostCenterInactive = "AST-VAL-NLI-018";
    public const string Concurrent = "AST-VAL-NLI-028";
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
    public const string AccountRequired = "Kode akun kontrol harga perolehan dan akumulasi penyusutan wajib diisi.";
    public const string AccountLengthFormat = "Kode akun paling panjang {0} karakter.";
    public const string AccountCodeFormat = "Kode akun {0} tidak ada di daftar akun Akuntansi, tidak aktif, atau bukan akun yang dapat diposting.";
    public const string AccountUnknownFormat = "Akun {0} tidak lagi ada atau aktif di daftar akun Akuntansi.";
    public const string AccountNotControlFormat = "Akun {0} belum ditandai akun kontrol di Akuntansi.";
    public const string GroupWithoutAccountFormat = "Saldo kelompok {0} tidak akan dikirim untuk rekonsiliasi.";
    public const string UnitNotFound = "Unit tidak ditemukan atau tidak aktif.";
    public const string CostCenterRequired = "Pusat biaya wajib dipilih.";
    public const string CostCenterNotFound = "Pusat biaya tidak ditemukan.";
    public const string CostCenterInactive = "Pusat biaya ini tidak aktif.";
    public const string MappingNotFound = "Unit ini belum punya pemetaan pusat biaya.";
    public const string MappedCostCenterInactiveFormat = "Pusat biaya pemetaan {0} tidak aktif lagi.";
    public const string NoActiveCostCenter = "Unit ini belum punya pusat biaya aktif.";
    public const string ManyActiveCostCentersFormat = "Unit ini punya {0} pusat biaya aktif; tetapkan salah satu.";
    public const string Concurrent = "Data ini baru saja diubah orang lain. Muat ulang lalu coba lagi.";
    public const string RunFuturePeriodFormat = "Penyusutan {0} belum dapat diproses.";
    public const string RunAlreadyProcessedFormat = "Penyusutan {0} sudah diproses pada {1} oleh {2}.";
    public const string RunSequenceFormat = "Proses penyusutan {0} terlebih dahulu.";
    public const string RunBeforeFirstFormat = "Penyusutan pertama adalah {0}, bulan berlakunya parameter nilai pertama.";
    public const string RunPreviewErrorsFormat = "Masih ada {0} galat pada pratinjau. Selesaikan sebelum memproses.";
    public const string RunCostCenterUnclearFormat = "Unit {0} belum jelas pusat biayanya. Petakan di Parameter Nilai.";
    public const string RunUnplacedFormat = "{0} aset belum ditempatkan pada akhir {1} sehingga pusat biayanya tidak dapat ditentukan: {2}.";
    public const string RunGroupWithoutPolicyFormat = "Kelompok {0} belum punya umur manfaat.";
    public const string RunPeriodFormat = "Periode wajib berformat YYYY-MM.";
    public const string RunNotFound = "Proses penyusutan tidak ditemukan.";
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
