using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Seeders;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Tests.Security;

/// <summary>
/// Invarian registry permission (Phase A0).
///
/// Audit Phase A0 menemukan 89 endpoint yang <c>[AccessPermission]</c>-nya menunjuk pasangan
/// resource/action yang tidak pernah didaftarkan seeder, sehingga menolak seluruh pengguna
/// non-SuperAdmin dengan 403 permanen yang tidak dapat diperbaiki dari layar Akses Role.
///
/// Kelas test sebelumnya tidak menangkapnya karena <b>menanam sendiri</b> baris registry-nya
/// memakai konstanta seperti <c>"CashierShift"</c> — nama yang justru tidak pernah didaftarkan di
/// produksi. Test di bawah menurunkan registry dari atribut yang sama dengan yang dibaca seeder,
/// sehingga tidak bisa lulus karena asumsi test sendiri.
/// </summary>
public sealed class PermissionRegistryInvariantTests
{
    private static PermissionRegistryDescriptor.RegistrySnapshot Snapshot() =>
        PermissionRegistryDescriptor.BuildFromAssembly(typeof(AccessPermissionService).Assembly);

    /// <summary>
    /// Test 1 dan 2: setiap endpoint terproteksi menghasilkan baris registry yang dapat dicentang.
    ///
    /// Sejak A0 seeder mendaftarkan pasangan dari <c>[AccessPermission]</c> itu sendiri, sehingga
    /// satu-satunya cara sebuah endpoint menjadi tidak dapat diberikan adalah bila metadata
    /// <c>[AccessAction]</c>-nya tidak ada.
    /// </summary>
    [Fact]
    public void EveryProtectedEndpointIsRegisterableInRoleAccess()
    {
        var snapshot = Snapshot();
        var result = PermissionRegistryValidator.Validate(snapshot);

        Assert.True(
            result.UnregisterableEndpoints.Count == 0,
            $"{result.UnregisterableEndpoints.Count} endpoint terproteksi tidak akan muncul di layar " +
            "Akses Role, sehingga menolak semua pengguna non-SuperAdmin secara permanen:" +
            Environment.NewLine + string.Join(Environment.NewLine, result.UnregisterableEndpoints));
    }

    /// <summary>Test 4: identitas seeder == identitas runtime, dijamin secara struktural.</summary>
    [Fact]
    public void SeederIdentityMatchesRuntimeIdentity()
    {
        var snapshot = Snapshot();

        // Setiap kemampuan yang didaftarkan seeder berasal dari kunci runtime, dan sebaliknya.
        foreach (var action in snapshot.Actions)
        {
            var key = PermissionRegistryDescriptor.RegistrySnapshot.Key(action.ResourceName, action.ActionName);
            Assert.Contains(key, snapshot.DeclaredKeys);
        }

        Assert.Equal(
            snapshot.DeclaredKeys.Count,
            snapshot.Actions
                .Select(x => PermissionRegistryDescriptor.RegistrySnapshot.Key(x.ResourceName, x.ActionName))
                .Distinct(StringComparer.Ordinal)
                .Count());
    }

    [Fact]
    public void RegistrySnapshotIsNotVacuous()
    {
        var snapshot = Snapshot();

        Assert.True(snapshot.DeclaredKeys.Count > 900,
            $"Hanya {snapshot.DeclaredKeys.Count} identitas kanonik terbaca. " +
            "Test invarian menjadi tidak bermakna bila snapshot-nya kosong.");
    }

    /// <summary>Test 3: tidak ada identitas resource ganda antar modul.</summary>
    [Fact]
    public void NoDuplicateCanonicalPermissionIdentity()
    {
        var result = PermissionRegistryValidator.Validate(Snapshot());

        Assert.True(
            result.DuplicateResourceIdentities.Count == 0,
            "Resource permission ganda membuat pencarian registry tidak deterministik:" +
            Environment.NewLine + string.Join(Environment.NewLine, result.DuplicateResourceIdentities));
    }

    /// <summary>AccessType wajib salah satu dari empat kolom layar Akses Role.</summary>
    [Fact]
    public void EveryActionUsesAnAllowedAccessType()
    {
        var result = PermissionRegistryValidator.Validate(Snapshot());

        Assert.True(
            result.InvalidAccessTypes.Count == 0,
            "AccessType di luar Read/Create/Update/Delete membuat kemampuan tidak muncul di layar:" +
            Environment.NewLine + string.Join(Environment.NewLine, result.InvalidAccessTypes));
    }

    /// <summary>
    /// Test 5 dan 14: seeder hanya mengelola registry dan tidak pernah membuat
    /// <c>SysAccessPolicy</c>. Kemampuan sensitif tetap fail closed sampai admin memberikannya.
    /// </summary>
    [Fact]
    public async Task ReconcileNeverCreatesAccessPolicy()
    {
        await using var dbContext = NewContext();

        var snapshot = Snapshot();
        var result = await AccessMenuSeeder.ReconcileAsync(dbContext, snapshot);

        Assert.True(result.ActionsUpserted > 0, "Seeder tidak mendaftarkan satu pun action.");
        Assert.Equal(0, await dbContext.SysAccessPolicies.CountAsync());

        // Kemampuan finansial dan klinis yang sebelumnya 403 permanen kini terdaftar, tetapi tidak
        // satu pun otomatis diberikan kepada Departemen x Posisi mana pun.
        foreach (var (resource, action) in new[]
                 {
                     ("CashierShift", "Close"),
                     ("CashierShift", "Reopen"),
                     ("BillingWriteOff", "Approve"),
                     ("BillingRefund", "Approve"),
                     ("BillingPayment", "Create"),
                     ("LabSpecimen", "Accept"),
                     ("LabSpecimen", "Collect"),
                     ("InpatientDischarge", "Sign"),
                     ("OperatingRoomAnesthesia", "Update"),
                 })
        {
            var registered = await dbContext.SysActionAccesses
                .AnyAsync(x => x.ActionName == action &&
                               x.ControllerAccess!.ControllerName == resource);

            Assert.True(registered, $"{resource}.{action} tidak terdaftar di registry.");
        }

        Assert.Equal(0, await dbContext.SysAccessPolicies.CountAsync());
    }

    /// <summary>
    /// Rekonsiliasi generik menutup baris registry yang tidak lagi punya deskriptor di source,
    /// memakai lifecycle repository dan bukan hard delete.
    /// </summary>
    [Fact]
    public async Task ReconcileClosesStaleRegistryRowsWithoutHardDelete()
    {
        await using var dbContext = NewContext();

        var snapshot = Snapshot();
        await AccessMenuSeeder.ReconcileAsync(dbContext, snapshot);

        var module = await dbContext.SysApplicationModules.FirstAsync();
        var staleController = new QuilvianSystemBackend.Models.SysControllerAccess
        {
            Id = Guid.NewGuid(),
            ModuleId = module.Id,
            ControllerName = "ResourceYangSudahDihapusDariSource",
            DisplayName = "Peninggalan versi lama",
            IsActive = true,
            VisibleInRoleAccess = true,
            IsDelete = false
        };
        dbContext.SysControllerAccesses.Add(staleController);

        dbContext.SysActionAccesses.Add(new QuilvianSystemBackend.Models.SysActionAccess
        {
            Id = Guid.NewGuid(),
            ControllerAccessId = staleController.Id,
            ActionName = "Read",
            DisplayName = "Read peninggalan",
            AccessType = "Read",
            IsActive = true,
            VisibleInRoleAccess = true,
            IsDelete = false
        });
        await dbContext.SaveChangesAsync();

        var second = await AccessMenuSeeder.ReconcileAsync(dbContext, snapshot);

        Assert.True(second.ControllersClosed >= 1);
        Assert.True(second.ActionsClosed >= 1);

        var closed = await dbContext.SysControllerAccesses
            .FirstAsync(x => x.ControllerName == "ResourceYangSudahDihapusDariSource");

        Assert.False(closed.IsActive);
        Assert.True(closed.IsDelete);
        Assert.False(closed.VisibleInRoleAccess);

        // Bukan hard delete: barisnya tetap ada supaya sejarah dan referensi policy lama utuh.
        Assert.NotNull(closed);
    }

    /// <summary>Rekonsiliasi bersifat idempoten: menjalankannya dua kali tidak menutup apa pun.</summary>
    [Fact]
    public async Task ReconcileIsIdempotent()
    {
        await using var dbContext = NewContext();

        var snapshot = Snapshot();
        await AccessMenuSeeder.ReconcileAsync(dbContext, snapshot);
        var second = await AccessMenuSeeder.ReconcileAsync(dbContext, snapshot);

        Assert.Equal(0, second.ModulesClosed);
        Assert.Equal(0, second.ControllersClosed);
        Assert.Equal(0, second.ActionsClosed);
    }

    private static ApplicationDbContext NewContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"registry-{Guid.NewGuid()}")
            .Options);
}
