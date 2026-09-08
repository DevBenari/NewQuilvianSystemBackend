using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Services;
using QuilvianSystemBackend.Repositories;
using Microsoft.Extensions.Configuration;

namespace QuilvianSystemBackend.Tests.BillingManagement;

/// <summary>
/// BE-BKC-034 — perluasan penomoran voucher Petty Cash pada mekanisme BilNumberSeries yang sudah
/// ada. Menutup BIL-AT-065 seluruhnya dan bagian BIL-AT-066 yang dapat dibuktikan tanpa
/// PostgreSQL; jaminan pg_advisory_xact_lock-nya sendiri hanya dapat dibuktikan pada provider
/// relational — lihat laporan task bagian Verifikasi.
/// </summary>
public sealed class BillingNumberSeriesServiceTests
{
    private static readonly Guid Actor = Guid.NewGuid();

    /// <summary>
    /// 6 September 2026 pukul 22:30 UTC = 7 September 2026 pukul 05:30 Asia/Jakarta.
    /// Instant ini sengaja dipilih agar tanggal UTC dan tanggal bisnis BERBEDA, sehingga test
    /// benar-benar membuktikan scope key memakai tanggal Asia/Jakarta.
    /// </summary>
    private static readonly DateTimeOffset JakartaMorning =
        new(2026, 9, 6, 22, 30, 0, TimeSpan.Zero);

    private static BillingNumberSeriesService CreateService(ApplicationDbContext db) =>
        new(db, Options.Create(new BillingInvoiceNumberOptions()));

    // ---------------------------------------------------------------------------------------
    // BIL-AT-065 — voucher pertama pada tanggal Asia/Jakarta tertentu
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task PettyCashVoucherNumber_FirstAllocationOfTheDay_UsesPtcJakartaDateAndFourDigits()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);

        var number = await service.AllocatePettyCashVoucherNumberAsync(
            Actor, JakartaMorning, CancellationToken.None);
        await db.SaveChangesAsync();

        Assert.Equal("PTC-20260907-0001", number);
    }

    [Fact]
    public async Task PettyCashVoucherNumber_FirstAllocation_CreatesSingleSeriesRowWithCanonicalKey()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);

        await service.AllocatePettyCashVoucherNumberAsync(Actor, JakartaMorning, CancellationToken.None);
        await db.SaveChangesAsync();

        var series = Assert.Single(await db.BilNumberSeries.ToListAsync());
        Assert.Equal("BILLING_PETTY_CASH_VOUCHER", series.SequenceKey);
        Assert.Equal("20260907", series.ScopeKey);
        Assert.Equal("DAILY", series.ResetPolicy);
        Assert.Equal(1, series.CurrentValue);
    }

    /// <summary>
    /// Jalur gagal yang BIL-AT-065 tuntut TIDAK terjadi: nomor berbasis milidetik epoch seperti
    /// PC-1786239462244 pada rujukan tampilan (QBE-CODE-003).
    /// </summary>
    [Fact]
    public async Task PettyCashVoucherNumber_IsNotEpochMillisecondBased()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);

        var number = await service.AllocatePettyCashVoucherNumberAsync(
            Actor, JakartaMorning, CancellationToken.None);
        await db.SaveChangesAsync();

        Assert.StartsWith("PTC-", number, StringComparison.Ordinal);
        Assert.DoesNotContain(JakartaMorning.ToUnixTimeMilliseconds().ToString(), number, StringComparison.Ordinal);
        var parts = number.Split('-');
        Assert.Equal(3, parts.Length);
        Assert.Equal(8, parts[1].Length);
        Assert.Equal(4, parts[2].Length);
    }

    // ---------------------------------------------------------------------------------------
    // BIL-AT-066 — bagian yang dapat dibuktikan tanpa PostgreSQL
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task PettyCashVoucherNumber_TenAllocationsInSameDay_AreDistinctAndGapless()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);

        var numbers = new List<string>();
        for (var i = 0; i < 10; i++)
        {
            numbers.Add(await service.AllocatePettyCashVoucherNumberAsync(
                Actor, JakartaMorning, CancellationToken.None));
            await db.SaveChangesAsync();
        }

        Assert.Equal(10, numbers.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(
            Enumerable.Range(1, 10).Select(i => $"PTC-20260907-{i:D4}").ToList(),
            numbers);
    }

    [Fact]
    public async Task PettyCashVoucherNumber_NextBusinessDay_ResetsToOneInItsOwnScope()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);

        await service.AllocatePettyCashVoucherNumberAsync(Actor, JakartaMorning, CancellationToken.None);
        await db.SaveChangesAsync();

        var nextDay = await service.AllocatePettyCashVoucherNumberAsync(
            Actor, JakartaMorning.AddDays(1), CancellationToken.None);
        await db.SaveChangesAsync();

        Assert.Equal("PTC-20260908-0001", nextDay);
        var scopes = await db.BilNumberSeries
            .Where(x => x.SequenceKey == "BILLING_PETTY_CASH_VOUCHER")
            .Select(x => x.ScopeKey)
            .OrderBy(x => x)
            .ToListAsync();
        Assert.Equal(["20260907", "20260908"], scopes);
    }

    // ---------------------------------------------------------------------------------------
    // Konfigurasi
    // ---------------------------------------------------------------------------------------

    [Fact]
    public void PettyCashVoucherNumberOptions_DefaultsMatchTheLockedContract()
    {
        var options = new PettyCashVoucherNumberOptions();

        Assert.Equal("PTC", options.Prefix);
        Assert.Equal(BillingNumberResetPolicies.Daily, options.ResetPolicy);
        Assert.Equal(4, options.SequenceDigits);
        Assert.Equal("Billing:PettyCashVoucherNumber", PettyCashVoucherNumberOptions.SectionName);
    }

    [Fact]
    public void AddBillingManagement_RegistersPettyCashVoucherNumberOptions()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(
            new ConfigurationBuilder().Build());

        services.AddScoped(_ => IsolatedBillingDbContextFactory.Create());
        services.AddBillingManagement();

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();

        var options = scope.ServiceProvider
            .GetRequiredService<IOptions<PettyCashVoucherNumberOptions>>().Value;

        Assert.Equal("PTC", options.Prefix);
        Assert.Equal(4, options.SequenceDigits);
    }

    /// <summary>
    /// Konstruktor baru wajib OPSIONAL. Bila tidak, empat pemanggil existing dan seluruh test
    /// yang sudah berjalan akan rusak — risiko yang dicatat eksplisit pada kartu task BE-BKC-034.
    /// </summary>
    [Fact]
    public void Constructor_PettyCashOptionsParameter_IsOptional()
    {
        var constructor = Assert.Single(typeof(BillingNumberSeriesService).GetConstructors());
        var parameter = Assert.Single(
            constructor.GetParameters(),
            p => p.ParameterType == typeof(IOptions<PettyCashVoucherNumberOptions>));

        Assert.True(parameter.IsOptional);
        Assert.Equal(5, parameter.Position);
    }

    [Fact]
    public async Task PettyCashVoucherNumber_InvalidPrefix_ThrowsPettyCashDomainException()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = new BillingNumberSeriesService(
            db,
            Options.Create(new BillingInvoiceNumberOptions()),
            pettyCashVoucherOptions: Options.Create(new PettyCashVoucherNumberOptions { Prefix = "   " }));

        await Assert.ThrowsAsync<PettyCashVoucherValidationException>(() =>
            service.AllocatePettyCashVoucherNumberAsync(Actor, JakartaMorning, CancellationToken.None));
    }

    // ---------------------------------------------------------------------------------------
    // Regresi empat jenis nomor existing — DoD BE-BKC-034
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task ExistingNumberTypes_KeepTheirFormats_AfterPettyCashExtension()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);

        var invoice = await service.AllocateInvoiceNumberAsync(Actor, JakartaMorning, CancellationToken.None);
        await db.SaveChangesAsync();
        var deposit = await service.AllocateDepositAccountNumberAsync(Actor, JakartaMorning, CancellationToken.None);
        await db.SaveChangesAsync();
        var shift = await service.AllocateCashierShiftNumberAsync(Actor, JakartaMorning, CancellationToken.None);
        await db.SaveChangesAsync();
        var kwitansi = await service.AllocateKwitansiNumberAsync(Actor, JakartaMorning, CancellationToken.None);
        await db.SaveChangesAsync();

        Assert.Equal("BIL-20260907-00000001", invoice);
        Assert.Equal("DEP-00000001", deposit);
        Assert.Equal("CSH-20260907-000001", shift);
        Assert.Equal("KWS-20260907-0001", kwitansi);
    }

    /// <summary>
    /// Kelima jenis nomor wajib memakai baris BilNumberSeries yang terpisah, sehingga voucher
    /// kas kecil tidak pernah menggeser urutan nomor invoice, deposit, shift, atau kwitansi.
    /// </summary>
    [Fact]
    public async Task PettyCashVoucherNumber_DoesNotShareSequenceWithExistingNumberTypes()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);

        await service.AllocateInvoiceNumberAsync(Actor, JakartaMorning, CancellationToken.None);
        await db.SaveChangesAsync();
        await service.AllocatePettyCashVoucherNumberAsync(Actor, JakartaMorning, CancellationToken.None);
        await db.SaveChangesAsync();
        var secondInvoice = await service.AllocateInvoiceNumberAsync(Actor, JakartaMorning, CancellationToken.None);
        await db.SaveChangesAsync();

        Assert.Equal("BIL-20260907-00000002", secondInvoice);

        var keys = await db.BilNumberSeries
            .Select(x => x.SequenceKey)
            .OrderBy(x => x)
            .ToListAsync();
        Assert.Equal(["BILLING_INVOICE", "BILLING_PETTY_CASH_VOUCHER"], keys);
    }
}
