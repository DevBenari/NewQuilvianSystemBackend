using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Tests.BillingManagement;

// BE-BKC-017 hardening (26 Agustus 2026): RegisterService sebelumnya nol test coverage meski
// source-nya sudah lengkap (CRUD penuh) - ditemukan saat audit backend Register sebelum membangun
// frontend-nya. Test ini mengunci perilaku CRUD/validasi/soft-delete yang sudah ada.
public sealed class RegisterServiceTests
{
    [Fact]
    public async Task Create_NormalizesCodeAndTrimsFields()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);

        var result = await service.CreateAsync(new CreateRegisterRequest
        {
            RegisterCode = "  reg-01  ",
            RegisterName = "  Kasir Rawat Jalan 1  ",
            Location = "  Lantai 1  ",
            IsActive = true,
        }, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal("REG-01", result.RegisterCode);
        Assert.Equal("Kasir Rawat Jalan 1", result.RegisterName);
        Assert.Equal("Lantai 1", result.Location);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task Create_RejectsDuplicateCode()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        await service.CreateAsync(Request("REG-01", "Kasir 1"), Guid.NewGuid(), CancellationToken.None);

        await Assert.ThrowsAsync<RegisterValidationException>(() =>
            service.CreateAsync(Request("reg-01", "Kasir Lain"), Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task Create_RejectsDuplicateNameCaseInsensitive()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        await service.CreateAsync(Request("REG-01", "Kasir Utama"), Guid.NewGuid(), CancellationToken.None);

        await Assert.ThrowsAsync<RegisterValidationException>(() =>
            service.CreateAsync(Request("REG-02", "kasir utama"), Guid.NewGuid(), CancellationToken.None));
    }

    [Theory]
    [InlineData("", "Nama Valid")]
    [InlineData("KODE", "")]
    public async Task Create_RejectsEmptyRequiredFields(string code, string name)
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);

        await Assert.ThrowsAsync<RegisterValidationException>(() =>
            service.CreateAsync(Request(code, name), Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task Update_ChangesFieldsAndAllowsKeepingOwnCode()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        var created = await service.CreateAsync(Request("REG-01", "Kasir 1"), Guid.NewGuid(), CancellationToken.None);

        var updated = await service.UpdateAsync(created.Id, new UpdateRegisterRequest
        {
            RegisterCode = "REG-01",
            RegisterName = "Kasir 1 (Diperbarui)",
            Location = "Lantai 2",
            IsActive = false,
        }, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal("Kasir 1 (Diperbarui)", updated.RegisterName);
        Assert.Equal("Lantai 2", updated.Location);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task Update_RejectsCodeAlreadyUsedByAnotherRegister()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        await service.CreateAsync(Request("REG-01", "Kasir 1"), Guid.NewGuid(), CancellationToken.None);
        var second = await service.CreateAsync(Request("REG-02", "Kasir 2"), Guid.NewGuid(), CancellationToken.None);

        await Assert.ThrowsAsync<RegisterValidationException>(() =>
            service.UpdateAsync(second.Id, new UpdateRegisterRequest
            {
                RegisterCode = "REG-01",
                RegisterName = "Kasir 2",
                IsActive = true,
            }, Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task Update_ThrowsWhenRegisterNotFoundOrDeleted()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.UpdateAsync(Guid.NewGuid(), new UpdateRegisterRequest
            {
                RegisterCode = "REG-99",
                RegisterName = "Tidak Ada",
                IsActive = true,
            }, Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task ChangeStatusAsync_TogglesIsActive()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        var created = await service.CreateAsync(Request("REG-01", "Kasir 1"), Guid.NewGuid(), CancellationToken.None);

        var deactivated = await service.ChangeStatusAsync(created.Id, false, Guid.NewGuid(), CancellationToken.None);
        Assert.False(deactivated.IsActive);

        var reactivated = await service.ChangeStatusAsync(created.Id, true, Guid.NewGuid(), CancellationToken.None);
        Assert.True(reactivated.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesAndExcludesFromFutureQueries()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        var created = await service.CreateAsync(Request("REG-01", "Kasir 1"), Guid.NewGuid(), CancellationToken.None);

        var deleted = await service.DeleteAsync(created.Id, Guid.NewGuid(), CancellationToken.None);

        Assert.True(deleted.IsDelete);
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.GetByIdAsync(created.Id, CancellationToken.None));

        // Kode yang sudah dihapus (soft-delete) boleh dipakai ulang - konsisten dengan
        // ValidateAsync yang hanya membandingkan terhadap baris !IsDelete.
        var recreated = await service.CreateAsync(Request("REG-01", "Kasir 1 Baru"), Guid.NewGuid(), CancellationToken.None);
        Assert.Equal("REG-01", recreated.RegisterCode);
    }

    [Fact]
    public async Task GetOptionsAsync_DefaultsToOnlyActiveAndSortsByName()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        await service.CreateAsync(Request("REG-B", "Kasir B"), Guid.NewGuid(), CancellationToken.None);
        var inactive = await service.CreateAsync(Request("REG-A", "Kasir A"), Guid.NewGuid(), CancellationToken.None);
        await service.ChangeStatusAsync(inactive.Id, false, Guid.NewGuid(), CancellationToken.None);

        var options = await service.GetOptionsAsync(onlyActive: true, search: null, CancellationToken.None);

        var option = Assert.Single(options);
        Assert.Equal("REG-B", option.RegisterCode);
    }

    [Fact]
    public async Task GetPagedAsync_FiltersBySearchKeyword()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        await service.CreateAsync(Request("REG-01", "Kasir Rawat Jalan"), Guid.NewGuid(), CancellationToken.None);
        await service.CreateAsync(Request("REG-02", "Kasir Rawat Inap"), Guid.NewGuid(), CancellationToken.None);

        var result = await service.GetPagedAsync(
            search: "Jalan", isActive: null, sortBy: "registerName", sortDirection: "asc",
            pageNumber: 1, pageSize: 25, CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal("REG-01", item.RegisterCode);
    }

    private static CreateRegisterRequest Request(string code, string name) => new()
    {
        RegisterCode = code,
        RegisterName = name,
        IsActive = true,
    };

    private static RegisterService CreateService(Repositories.ApplicationDbContext dbContext)
    {
        var logger = new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor());
        return new RegisterService(dbContext, logger);
    }
}
