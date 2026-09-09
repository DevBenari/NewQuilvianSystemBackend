using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Tests.BillingManagement;

/// <summary>
/// BE-BKC-035 — CRUD baseline master data kategori Petty Cash, mengikuti TaxRuleService apa
/// adanya. Menutup bagian domain BIL-AT-076 yang dapat dibuktikan tanpa PostgreSQL/HTTP: create,
/// kode duplikat (409), hapus kategori terpakai (400, BIL-VAL-053), dan nonaktivasi.
/// </summary>
public sealed class PettyCashCategoryServiceTests
{
    private static PettyCashCategoryService CreateService(ApplicationDbContext db) =>
        new(db, new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor()));

    private static CreatePettyCashCategoryRequest Request(string code, string name = "Kategori Uji", bool isActive = true) => new()
    {
        CategoryCode = code,
        CategoryName = name,
        Description = "Deskripsi uji",
        IsActive = isActive
    };

    [Fact]
    public async Task Create_PersistsUppercasedCodeAndTrimmedFields()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);

        var created = await service.CreateAsync(Request("  transport  ", "  Transport  "), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal("TRANSPORT", created.CategoryCode);
        Assert.Equal("Transport", created.CategoryName);
        Assert.True(created.IsActive);
    }

    [Fact]
    public async Task Create_DuplicateCategoryCode_ThrowsConflict()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        await service.CreateAsync(Request("ATK"), Guid.NewGuid(), CancellationToken.None);

        await Assert.ThrowsAsync<PettyCashCategoryConflictException>(() =>
            service.CreateAsync(Request("atk", "Alat Tulis Kantor Lain"), Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task Update_DuplicateCategoryCodeFromAnotherRow_ThrowsConflict()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        await service.CreateAsync(Request("ATK"), Guid.NewGuid(), CancellationToken.None);
        var second = await service.CreateAsync(Request("KONSUMSI"), Guid.NewGuid(), CancellationToken.None);

        await Assert.ThrowsAsync<PettyCashCategoryConflictException>(() => service.UpdateAsync(
            second.Id,
            new UpdatePettyCashCategoryRequest { CategoryCode = "ATK", CategoryName = second.CategoryName, IsActive = true },
            Guid.NewGuid(),
            CancellationToken.None));
    }

    [Fact]
    public async Task Update_SameRowKeepingItsOwnCode_Succeeds()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        var created = await service.CreateAsync(Request("ATK"), Guid.NewGuid(), CancellationToken.None);

        var updated = await service.UpdateAsync(
            created.Id,
            new UpdatePettyCashCategoryRequest { CategoryCode = "ATK", CategoryName = "Alat Tulis Kantor", IsActive = true },
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.Equal("Alat Tulis Kantor", updated.CategoryName);
    }

    [Fact]
    public async Task GetById_UnknownId_ThrowsKeyNotFound()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateStatus_TogglesIsActiveWithoutChangingOtherFields()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        var created = await service.CreateAsync(Request("MAINTENANCE"), Guid.NewGuid(), CancellationToken.None);

        var deactivated = await service.UpdateStatusAsync(
            created.Id, new UpdatePettyCashCategoryStatusRequest { IsActive = false }, Guid.NewGuid(), CancellationToken.None);
        Assert.False(deactivated.IsActive);
        Assert.Equal("MAINTENANCE", deactivated.CategoryCode);

        var reactivated = await service.UpdateStatusAsync(
            created.Id, new UpdatePettyCashCategoryStatusRequest { IsActive = true }, Guid.NewGuid(), CancellationToken.None);
        Assert.True(reactivated.IsActive);
    }

    [Fact]
    public async Task Delete_UnusedCategory_MarksSoftDeleted()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        var created = await service.CreateAsync(Request("OPERASIONAL"), Guid.NewGuid(), CancellationToken.None);

        var deleted = await service.DeleteAsync(created.Id, Guid.NewGuid(), CancellationToken.None);

        Assert.True(deleted.IsDelete);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetByIdAsync(created.Id, CancellationToken.None));
    }

    // BIL-VAL-053: kategori yang masih dipakai voucher (baris apa pun, tanpa memandang Status)
    // wajib ditolak dari penghapusan — hanya nonaktivasi yang diperbolehkan.
    [Fact]
    public async Task Delete_CategoryUsedByVoucher_ThrowsInUseException()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        var created = await service.CreateAsync(Request("TRANSPORT"), Guid.NewGuid(), CancellationToken.None);

        db.BilPettyCashVouchers.Add(new BilPettyCashVoucher
        {
            VoucherNumber = "PTC-20260907-0001",
            RecipientName = "Budi",
            CategoryId = created.Id,
            Amount = 50_000m,
            Purpose = "Ongkos kirim dokumen",
            RequestedBy = Guid.NewGuid(),
            SubmittedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(CancellationToken.None);

        var exception = await Assert.ThrowsAsync<PettyCashCategoryInUseException>(
            () => service.DeleteAsync(created.Id, Guid.NewGuid(), CancellationToken.None));
        Assert.Contains("dipakai voucher", exception.Message, StringComparison.OrdinalIgnoreCase);

        var stillFound = await service.GetByIdAsync(created.Id, CancellationToken.None);
        Assert.True(stillFound.IsActive);
    }

    [Fact]
    public async Task GetPagedAsync_FiltersByActiveAndSearch_AndSortsByCategoryNameAscendingByDefault()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        await service.CreateAsync(Request("KONSUMSI", "Konsumsi"), Guid.NewGuid(), CancellationToken.None);
        await service.CreateAsync(Request("ATK", "Alat Tulis Kantor"), Guid.NewGuid(), CancellationToken.None);
        var inactive = await service.CreateAsync(Request("MAINTENANCE", "Maintenance", isActive: false), Guid.NewGuid(), CancellationToken.None);

        var activeOnly = await service.GetPagedAsync(new PettyCashCategoryQuery { IsActive = true }, CancellationToken.None);
        Assert.Equal(2, activeOnly.TotalData);
        Assert.DoesNotContain(activeOnly.Items, x => x.Id == inactive.Id);
        Assert.Equal(["Alat Tulis Kantor", "Konsumsi"], activeOnly.Items.Select(x => x.CategoryName).ToList());

        var searched = await service.GetPagedAsync(new PettyCashCategoryQuery { Search = "konsu" }, CancellationToken.None);
        Assert.Single(searched.Items);
        Assert.Equal("KONSUMSI", searched.Items[0].CategoryCode);
    }

    [Fact]
    public async Task GetOptionsAsync_DefaultsToActiveOnly()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        await service.CreateAsync(Request("ATK", "Alat Tulis Kantor"), Guid.NewGuid(), CancellationToken.None);
        await service.CreateAsync(Request("MAINTENANCE", "Maintenance", isActive: false), Guid.NewGuid(), CancellationToken.None);

        var activeOnly = await service.GetOptionsAsync(onlyActive: true, search: null, CancellationToken.None);
        Assert.Single(activeOnly);

        var all = await service.GetOptionsAsync(onlyActive: false, search: null, CancellationToken.None);
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task GetSummaryAsync_CountsTotalActiveAndInactive()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        await service.CreateAsync(Request("ATK"), Guid.NewGuid(), CancellationToken.None);
        await service.CreateAsync(Request("MAINTENANCE", isActive: false), Guid.NewGuid(), CancellationToken.None);

        var summary = await service.GetSummaryAsync(CancellationToken.None);

        Assert.Equal(2, summary.TotalCategory);
        Assert.Equal(1, summary.ActiveCategory);
        Assert.Equal(1, summary.InactiveCategory);
    }

    [Fact]
    public void AddBillingManagement_RegistersPettyCashCategoryService()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => IsolatedBillingDbContextFactory.Create());
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<ILogger<LoggerService>>(NullLogger<LoggerService>.Instance);
        services.AddScoped<LoggerService>();
        services.AddBillingManagement();

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<PettyCashCategoryService>());
    }
}
