using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public sealed class BillingInvoiceNumberOptions
{
    public const string SectionName = "Billing:InvoiceNumber";
    public string Prefix { get; set; } = "BIL";
    public string ResetPolicy { get; set; } = BillingNumberResetPolicies.Daily;
    public int SequenceDigits { get; set; } = 8;
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
    private readonly ApplicationDbContext _dbContext;
    private readonly BillingInvoiceNumberOptions _options;

    public BillingNumberSeriesService(ApplicationDbContext dbContext, IOptions<BillingInvoiceNumberOptions> options)
    {
        _dbContext = dbContext;
        _options = options.Value;
    }

    public async Task<string> AllocateInvoiceNumberAsync(Guid actorUserId, DateTimeOffset instant, CancellationToken cancellationToken)
    {
        var prefix = Required(_options.Prefix, "Prefix").ToUpperInvariant();
        if (prefix.Length > 15) throw new BillingInvoiceValidationException("Prefix nomor invoice maksimal 15 karakter.");
        var resetPolicy = Required(_options.ResetPolicy, "ResetPolicy").ToUpperInvariant();
        if (!BillingNumberResetPolicies.All.Contains(resetPolicy))
            throw new BillingInvoiceValidationException("ResetPolicy nomor invoice harus NEVER, YEARLY, MONTHLY, atau DAILY.");
        if (_options.SequenceDigits is < 4 or > 12)
            throw new BillingInvoiceValidationException("SequenceDigits nomor invoice harus antara 4 dan 12.");

        var local = TimeZoneInfo.ConvertTime(instant, ResolveBusinessTimeZone());
        var scopeKey = resetPolicy switch
        {
            BillingNumberResetPolicies.Never => "GLOBAL",
            BillingNumberResetPolicies.Yearly => local.ToString("yyyy"),
            BillingNumberResetPolicies.Monthly => local.ToString("yyyyMM"),
            BillingNumberResetPolicies.Daily => local.ToString("yyyyMMdd"),
            _ => throw new BillingInvoiceValidationException("ResetPolicy nomor invoice tidak didukung.")
        };

        if (_dbContext.Database.IsRelational())
        {
            if (_dbContext.Database.CurrentTransaction is null)
                throw new InvalidOperationException("Alokasi nomor invoice relational wajib berada di dalam transaction.");
            var lockKey = $"BIL_NUMBER_{InvoiceSequenceKey}_{scopeKey}";
            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));", [lockKey], cancellationToken);
        }

        var series = await _dbContext.BilNumberSeries.SingleOrDefaultAsync(
            x => x.SequenceKey == InvoiceSequenceKey && x.ScopeKey == scopeKey, cancellationToken);
        if (series is null)
        {
            series = new BilNumberSeries
            {
                SequenceKey = InvoiceSequenceKey,
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

        var sequence = series.CurrentValue.ToString($"D{_options.SequenceDigits}");
        return scopeKey == "GLOBAL" ? $"{prefix}-{sequence}" : $"{prefix}-{scopeKey}-{sequence}";
    }

    private static TimeZoneInfo ResolveBusinessTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
    }
    private static string Required(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new BillingInvoiceValidationException($"Konfigurasi {field} nomor invoice wajib diisi.");
        return value.Trim();
    }
}
