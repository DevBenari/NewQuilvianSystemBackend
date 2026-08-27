using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public sealed class BillingInvoiceNumberOptions
{
    public const string SectionName = "Billing:InvoiceNumber";
    public string Prefix { get; set; } = "BIL";
    public string ResetPolicy { get; set; } = BillingNumberResetPolicies.Daily;
    public int SequenceDigits { get; set; } = 8;
}

public sealed class BillingDepositAccountNumberOptions
{
    public const string SectionName = "Billing:DepositAccountNumber";
    public string Prefix { get; set; } = "DEP";
    public string ResetPolicy { get; set; } = BillingNumberResetPolicies.Never;
    public int SequenceDigits { get; set; } = 8;
}

public sealed class BillingCashierShiftNumberOptions
{
    public const string SectionName = "Billing:CashierShiftNumber";
    public string Prefix { get; set; } = "CSH";
    public string ResetPolicy { get; set; } = BillingNumberResetPolicies.Daily;
    public int SequenceDigits { get; set; } = 6;
}

public sealed class BillingKwitansiNumberOptions
{
    public const string SectionName = "Billing:KwitansiNumber";
    public string Prefix { get; set; } = "KWS";
    public string ResetPolicy { get; set; } = BillingNumberResetPolicies.Daily;
    public int SequenceDigits { get; set; } = 4;
}

public static class BillingNumberResetPolicies
{
    public const string Never = "NEVER";
    public const string Yearly = "YEARLY";
    public const string Monthly = "MONTHLY";
    public const string Daily = "DAILY";
    public static readonly IReadOnlySet<string> All = new HashSet<string>([Never, Yearly, Monthly, Daily], StringComparer.OrdinalIgnoreCase);
}

public sealed class BillingNumberSeriesService
{
    private const string InvoiceSequenceKey = "BILLING_INVOICE";
    private const string DepositAccountSequenceKey = "BILLING_DEPOSIT_ACCOUNT";
    private const string CashierShiftSequenceKey = "BILLING_CASHIER_SHIFT";
    private const string KwitansiSequenceKey = "BILLING_KWITANSI";
    private readonly ApplicationDbContext _dbContext;
    private readonly BillingInvoiceNumberOptions _invoiceOptions;
    private readonly BillingDepositAccountNumberOptions _depositOptions;
    private readonly BillingCashierShiftNumberOptions _cashierShiftOptions;
    private readonly BillingKwitansiNumberOptions _kwitansiOptions;

    public BillingNumberSeriesService(
        ApplicationDbContext dbContext,
        IOptions<BillingInvoiceNumberOptions> invoiceOptions,
        IOptions<BillingDepositAccountNumberOptions>? depositOptions = null,
        IOptions<BillingCashierShiftNumberOptions>? cashierShiftOptions = null,
        IOptions<BillingKwitansiNumberOptions>? kwitansiOptions = null)
    {
        _dbContext = dbContext;
        _invoiceOptions = invoiceOptions.Value;
        _depositOptions = depositOptions?.Value ?? new BillingDepositAccountNumberOptions();
        _cashierShiftOptions = cashierShiftOptions?.Value ?? new BillingCashierShiftNumberOptions();
        _kwitansiOptions = kwitansiOptions?.Value ?? new BillingKwitansiNumberOptions();
    }

    public Task<string> AllocateInvoiceNumberAsync(
        Guid actorUserId,
        DateTimeOffset instant,
        CancellationToken cancellationToken) =>
        AllocateNumberAsync(
            InvoiceSequenceKey,
            _invoiceOptions.Prefix,
            _invoiceOptions.ResetPolicy,
            _invoiceOptions.SequenceDigits,
            "invoice",
            message => new BillingInvoiceValidationException(message),
            actorUserId,
            instant,
            cancellationToken);

    public Task<string> AllocateDepositAccountNumberAsync(
        Guid actorUserId,
        DateTimeOffset instant,
        CancellationToken cancellationToken) =>
        AllocateNumberAsync(
            DepositAccountSequenceKey,
            _depositOptions.Prefix,
            _depositOptions.ResetPolicy,
            _depositOptions.SequenceDigits,
            "akun deposit",
            message => new BillingDepositValidationException(message),
            actorUserId,
            instant,
            cancellationToken);

    // BKC-DEC-054: nomor Kwitansi memakai mekanisme sequence generik yang sama dengan Invoice
    // Number/Cashier Shift Number (BilNumberSeries, reset harian) - bukan tabel sequence terpisah.
    // Caller (BillingInvoiceService) bertanggung jawab memanggil ini HANYA SEKALI per invoice
    // (saat BilInvoice.KwitansiNumber masih null) supaya reprint tidak mengonsumsi nomor baru.
    public Task<string> AllocateKwitansiNumberAsync(
        Guid actorUserId,
        DateTimeOffset instant,
        CancellationToken cancellationToken) =>
        AllocateNumberAsync(
            KwitansiSequenceKey,
            _kwitansiOptions.Prefix,
            _kwitansiOptions.ResetPolicy,
            _kwitansiOptions.SequenceDigits,
            "kwitansi",
            message => new BillingInvoiceValidationException(message),
            actorUserId,
            instant,
            cancellationToken);

    public Task<string> AllocateCashierShiftNumberAsync(
        Guid actorUserId,
        DateTimeOffset instant,
        CancellationToken cancellationToken) =>
        AllocateNumberAsync(
            CashierShiftSequenceKey,
            _cashierShiftOptions.Prefix,
            _cashierShiftOptions.ResetPolicy,
            _cashierShiftOptions.SequenceDigits,
            "shift kasir",
            message => new CashierShiftValidationException(message),
            actorUserId,
            instant,
            cancellationToken);

    private async Task<string> AllocateNumberAsync(
        string sequenceKey,
        string? configuredPrefix,
        string? configuredResetPolicy,
        int sequenceDigits,
        string numberName,
        Func<string, Exception> validationException,
        Guid actorUserId,
        DateTimeOffset instant,
        CancellationToken cancellationToken)
    {
        var prefix = Required(configuredPrefix, "Prefix", numberName, validationException).ToUpperInvariant();
        if (prefix.Length > 15) throw validationException($"Prefix nomor {numberName} maksimal 15 karakter.");
        var resetPolicy = Required(configuredResetPolicy, "ResetPolicy", numberName, validationException).ToUpperInvariant();
        if (!BillingNumberResetPolicies.All.Contains(resetPolicy))
            throw validationException($"ResetPolicy nomor {numberName} harus NEVER, YEARLY, MONTHLY, atau DAILY.");
        if (sequenceDigits is < 4 or > 12)
            throw validationException($"SequenceDigits nomor {numberName} harus antara 4 dan 12.");

        var local = TimeZoneInfo.ConvertTime(instant, ResolveBusinessTimeZone());
        var scopeKey = resetPolicy switch
        {
            BillingNumberResetPolicies.Never => "GLOBAL",
            BillingNumberResetPolicies.Yearly => local.ToString("yyyy"),
            BillingNumberResetPolicies.Monthly => local.ToString("yyyyMM"),
            BillingNumberResetPolicies.Daily => local.ToString("yyyyMMdd"),
            _ => throw validationException($"ResetPolicy nomor {numberName} tidak didukung.")
        };

        if (_dbContext.Database.IsRelational())
        {
            if (_dbContext.Database.CurrentTransaction is null)
                throw new InvalidOperationException($"Alokasi nomor {numberName} relational wajib berada di dalam transaction.");
            var lockKey = $"BIL_NUMBER_{sequenceKey}_{scopeKey}";
            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));", [lockKey], cancellationToken);
        }

        var series = await _dbContext.BilNumberSeries.SingleOrDefaultAsync(
            x => x.SequenceKey == sequenceKey && x.ScopeKey == scopeKey, cancellationToken);
        if (series is null)
        {
            series = new BilNumberSeries
            {
                SequenceKey = sequenceKey,
                ScopeKey = scopeKey,
                ResetPolicy = resetPolicy,
                CurrentValue = 1,
                LastAllocatedAt = instant,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.BilNumberSeries.Add(series);
        }
        else
        {
            checked { series.CurrentValue++; }
            series.LastAllocatedAt = instant;
            series.UpdateDateTime = DateTime.UtcNow;
            series.UpdateBy = actorUserId;
        }

        var sequence = series.CurrentValue.ToString($"D{sequenceDigits}");
        return scopeKey == "GLOBAL" ? $"{prefix}-{sequence}" : $"{prefix}-{scopeKey}-{sequence}";
    }

    private static TimeZoneInfo ResolveBusinessTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
    }
    private static string Required(
        string? value,
        string field,
        string numberName,
        Func<string, Exception> validationException)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw validationException($"Konfigurasi {field} nomor {numberName} wajib diisi.");
        return value.Trim();
    }
}
