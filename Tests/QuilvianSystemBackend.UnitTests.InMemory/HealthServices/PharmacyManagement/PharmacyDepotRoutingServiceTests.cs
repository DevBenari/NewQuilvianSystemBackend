using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.PharmacyManagement;

public class PharmacyDepotRoutingServiceTests
{
    [Fact]
    public async Task ResolveAsync_Outpatient_PrefersClinicMatch()
    {
        await using var dbContext = CreateDbContext();
        var clinicId = Guid.NewGuid();
        var serviceUnitId = Guid.NewGuid();
        var encounter = AddEncounter(dbContext, EncounterType.Outpatient, serviceUnitId, clinicId);
        var clinicDepot = AddLocation(dbContext, serviceUnitId, clinicId, "Clinic");
        AddLocation(dbContext, serviceUnitId, null, "Pharmacy");
        await SaveAndClearAsync(dbContext);

        var result = await CreateService(dbContext).ResolveAsync(encounter.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(clinicDepot.Id, result.StorageLocationId);
    }

    [Fact]
    public async Task ResolveAsync_Outpatient_FallsBackToServiceUnit()
    {
        await using var dbContext = CreateDbContext();
        var serviceUnitId = Guid.NewGuid();
        var encounter = AddEncounter(dbContext, EncounterType.Outpatient, serviceUnitId, Guid.NewGuid());
        var serviceDepot = AddLocation(dbContext, serviceUnitId, null, "Pharmacy");
        await SaveAndClearAsync(dbContext);

        var result = await CreateService(dbContext).ResolveAsync(encounter.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(serviceDepot.Id, result.StorageLocationId);
    }

    [Theory]
    [InlineData(EncounterType.Emergency, "Emergency")]
    [InlineData(EncounterType.Inpatient, "Pharmacy")]
    public async Task ResolveAsync_ServiceType_SelectsMatchingDepot(
        EncounterType encounterType,
        string storageLocationType)
    {
        await using var dbContext = CreateDbContext();
        var serviceUnitId = Guid.NewGuid();
        var encounter = AddEncounter(dbContext, encounterType, serviceUnitId);
        var expected = AddLocation(dbContext, serviceUnitId, null, storageLocationType.ToUpperInvariant());
        AddLocation(dbContext, serviceUnitId, null, "General");
        await SaveAndClearAsync(dbContext);

        var result = await CreateService(dbContext).ResolveAsync(encounter.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected.Id, result.StorageLocationId);
    }

    [Theory]
    [InlineData(EncounterType.Unknown)]
    [InlineData(EncounterType.MedicalCheckup)]
    [InlineData(EncounterType.Telemedicine)]
    public async Task ResolveAsync_UnsupportedEncounterType_ReturnsUnsupported(
        EncounterType encounterType)
    {
        await using var dbContext = CreateDbContext();
        var encounter = AddEncounter(dbContext, encounterType, Guid.NewGuid());
        await SaveAndClearAsync(dbContext);

        var result = await CreateService(dbContext).ResolveAsync(encounter.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("PHA_ROUTE_SERVICE_UNSUPPORTED", result.Code);
        Assert.Null(result.StorageLocationId);
    }

    [Fact]
    public async Task ResolveAsync_NoCandidate_ReturnsNotFound()
    {
        await using var dbContext = CreateDbContext();
        var encounter = AddEncounter(dbContext, EncounterType.Emergency, Guid.NewGuid());
        await SaveAndClearAsync(dbContext);

        var result = await CreateService(dbContext).ResolveAsync(encounter.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("PHA_ROUTE_NOT_FOUND", result.Code);
    }

    [Fact]
    public async Task ResolveAsync_MultipleCandidates_ReturnsAmbiguous()
    {
        await using var dbContext = CreateDbContext();
        var serviceUnitId = Guid.NewGuid();
        var encounter = AddEncounter(dbContext, EncounterType.Emergency, serviceUnitId);
        AddLocation(dbContext, serviceUnitId, null, "Emergency");
        AddLocation(dbContext, serviceUnitId, null, "Emergency");
        await SaveAndClearAsync(dbContext);

        var result = await CreateService(dbContext).ResolveAsync(encounter.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("PHA_ROUTE_AMBIGUOUS", result.Code);
        Assert.Null(result.StorageLocationId);
    }

    [Fact]
    public async Task ResolveAsync_IneligibleLocations_AreExcluded()
    {
        await using var dbContext = CreateDbContext();
        var serviceUnitId = Guid.NewGuid();
        var encounter = AddEncounter(dbContext, EncounterType.Emergency, serviceUnitId);
        AddLocation(dbContext, serviceUnitId, null, "Emergency", isActive: false);
        AddLocation(dbContext, serviceUnitId, null, "Emergency", isDelete: true);
        AddLocation(dbContext, serviceUnitId, null, "Emergency", isCancel: true);
        AddLocation(dbContext, serviceUnitId, null, "Emergency", isPharmacy: false);
        AddLocation(dbContext, serviceUnitId, null, "Emergency", allowDispensing: false);
        AddLocation(dbContext, serviceUnitId, null, "Emergency", isMainWarehouse: true);
        AddLocation(dbContext, serviceUnitId, null, "Emergency", isQuarantine: true);
        await SaveAndClearAsync(dbContext);

        var result = await CreateService(dbContext).ResolveAsync(encounter.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("PHA_ROUTE_NOT_FOUND", result.Code);
    }

    [Fact]
    public async Task ResolveAsync_DoesNotTrackOrMutateEntities()
    {
        await using var dbContext = CreateDbContext();
        var serviceUnitId = Guid.NewGuid();
        var encounter = AddEncounter(dbContext, EncounterType.Inpatient, serviceUnitId);
        AddLocation(dbContext, serviceUnitId, null, "Pharmacy");
        await SaveAndClearAsync(dbContext);

        var result = await CreateService(dbContext).ResolveAsync(encounter.Id);

        Assert.True(result.IsSuccess);
        Assert.Empty(dbContext.ChangeTracker.Entries());
    }

    [Fact]
    public async Task ResolveAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        await using var dbContext = CreateDbContext();
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CreateService(dbContext).ResolveAsync(Guid.NewGuid(), cancellationTokenSource.Token));
    }

    private static PharmacyDepotRoutingService CreateService(ApplicationDbContext dbContext)
        => new(dbContext);

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static TrxPatientEncounter AddEncounter(
        ApplicationDbContext dbContext,
        EncounterType encounterType,
        Guid serviceUnitId,
        Guid? clinicId = null)
    {
        var encounter = new TrxPatientEncounter
        {
            EncounterType = encounterType,
            ServiceUnitId = serviceUnitId,
            ClinicId = clinicId,
            EncounterNumber = $"TEST-{Guid.NewGuid():N}",
            PatientId = Guid.NewGuid(),
            RegisteredByUserId = Guid.NewGuid(),
            IsActive = true
        };

        dbContext.Set<TrxPatientEncounter>().Add(encounter);
        return encounter;
    }

    private static MstDrugStorageLocation AddLocation(
        ApplicationDbContext dbContext,
        Guid serviceUnitId,
        Guid? clinicId,
        string storageLocationType,
        bool isActive = true,
        bool isDelete = false,
        bool isCancel = false,
        bool isPharmacy = true,
        bool allowDispensing = true,
        bool isMainWarehouse = false,
        bool isQuarantine = false)
    {
        var location = new MstDrugStorageLocation
        {
            ServiceUnitId = serviceUnitId,
            ClinicId = clinicId,
            StorageLocationCode = $"TEST-{Guid.NewGuid():N}",
            StorageLocationName = "Test Pharmacy Depot",
            StorageLocationType = storageLocationType,
            IsActive = isActive,
            IsDelete = isDelete,
            IsCancel = isCancel,
            IsPharmacyLocation = isPharmacy,
            IsAllowDispensing = allowDispensing,
            IsMainWarehouse = isMainWarehouse,
            IsQuarantineLocation = isQuarantine
        };

        dbContext.Set<MstDrugStorageLocation>().Add(location);
        return location;
    }

    private static async Task SaveAndClearAsync(ApplicationDbContext dbContext)
    {
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
    }
}
