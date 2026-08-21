using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.BillingManagement;

public sealed class BillingInvoiceServiceTests
{
    [Fact]
    public async Task FirstChargeCreatesOneInvoiceAndAdditionalSourceReusesIt()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, categoryId) = await SeedAsync(db);
        var service = CreateService(db);

        var first = await service.UpsertChargeAsync(Request(encounterId, categoryId, "PROC-1"), Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var secondRequest = Request(encounterId, categoryId, "LAB-1", "LABORATORY", "ACCEPTED");
        var second = await service.UpsertChargeAsync(secondRequest, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(2, second.ActiveItemCount);
        Assert.Single(db.BilInvoices);
        Assert.StartsWith("BIL-", first.InvoiceNumber);
    }

    [Fact]
    public async Task IdenticalReplayIsNoOpForSameOrNewIdempotencyKey()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, categoryId) = await SeedAsync(db);
        var service = CreateService(db);
        var request = Request(encounterId, categoryId, "PROC-REPLAY");
        var key = Guid.NewGuid();
        await service.UpsertChargeAsync(request, key, Guid.NewGuid(), CancellationToken.None);

        var sameKey = await service.UpsertChargeAsync(request, key, Guid.NewGuid(), CancellationToken.None);
        var newKey = await service.UpsertChargeAsync(request, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.True(sameKey.IsReplay);
        Assert.True(newKey.IsReplay);
        Assert.Single(db.BilInvoiceItems);
        Assert.Equal(2, db.BilChargeReceipts.Count());
    }

    [Fact]
    public async Task ReusedKeyWithDifferentPayloadAndOutOfOrderVersionAreRejected()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, categoryId) = await SeedAsync(db);
        var service = CreateService(db);
        var original = Request(encounterId, categoryId, "PROC-VERSION");
        original.SourceVersion = 2;
        var key = Guid.NewGuid();
        await service.UpsertChargeAsync(original, key, Guid.NewGuid(), CancellationToken.None);

        var changed = Request(encounterId, categoryId, "PROC-VERSION");
        changed.SourceVersion = 2;
        changed.Quantity = 2;
        await Assert.ThrowsAsync<BillingInvoiceConflictException>(() =>
            service.UpsertChargeAsync(changed, key, Guid.NewGuid(), CancellationToken.None));

        var stale = Request(encounterId, categoryId, "PROC-VERSION");
        stale.SourceVersion = 1;
        await Assert.ThrowsAsync<BillingInvoiceConflictException>(() =>
            service.UpsertChargeAsync(stale, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task SameActiveSourceCannotMoveToAnotherEncounter()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (firstEncounter, categoryId) = await SeedAsync(db);
        var secondEncounter = Guid.NewGuid();
        db.TrxPatientEncounters.Add(Encounter(secondEncounter, "ENC-2"));
        await db.SaveChangesAsync();
        var service = CreateService(db);
        await service.UpsertChargeAsync(Request(firstEncounter, categoryId, "SHARED-SOURCE"), Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        await Assert.ThrowsAsync<BillingInvoiceConflictException>(() => service.UpsertChargeAsync(
            Request(secondEncounter, categoryId, "SHARED-SOURCE"), Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task PharmacyRequiresFinalDispensedQuantity()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, categoryId) = await SeedAsync(db);
        var service = CreateService(db);
        var incomplete = Request(encounterId, categoryId, "DRUG-1", "PHARMACY", "ORDERED");
        var exception = await Assert.ThrowsAsync<BillingInvoiceValidationException>(() =>
            service.UpsertChargeAsync(incomplete, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));
        Assert.Contains("diserahkan belum final", exception.Message);

        var dispensed = Request(encounterId, categoryId, "DRUG-1", "PHARMACY", "DISPENSED");
        dispensed.Quantity = 3.5m;
        var result = await service.UpsertChargeAsync(dispensed, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        Assert.Equal(3.5m, result.Items.Single().Quantity);
    }

    [Fact]
    public void EndpointsRequireExactBillingInvoicePermissions()
    {
        AssertPermission(nameof(BillingInvoicesController.Get), "Read");
        AssertPermission(nameof(BillingInvoicesController.GetDetail), "Read");
        AssertPermission(nameof(BillingInvoicesController.FromSource), "Create");
    }

    private static async Task<(Guid EncounterId, Guid CategoryId)> SeedAsync(Repositories.ApplicationDbContext db)
    {
        var encounterId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        db.TrxPatientEncounters.Add(Encounter(encounterId, "ENC-1"));
        db.MstBillingItemCategories.Add(new MstBillingItemCategory
        {
            Id = categoryId,
            BillingItemCategoryCode = "PROC",
            BillingItemCategoryName = "Procedure",
            IsActive = true
        });
        await db.SaveChangesAsync();
        return (encounterId, categoryId);
    }

    private static TrxPatientEncounter Encounter(Guid id, string number) => new()
    {
        Id = id,
        EncounterNumber = number,
        PatientId = Guid.NewGuid(),
        ServiceUnitId = Guid.NewGuid(),
        EncounterType = EncounterType.Inpatient,
        EncounterStatus = EncounterStatus.Registered,
        IsActive = true
    };

    private static UpsertChargeRequest Request(Guid encounterId, Guid categoryId, string sourceId,
        string domain = "PROCEDURE", string status = "CONFIRMED") => new()
    {
        EncounterId = encounterId,
        SourceDomain = domain,
        SourceDetailId = sourceId,
        SourceVersion = 1,
        SourceStatus = status,
        OccurredAt = DateTimeOffset.UtcNow,
        CategoryId = categoryId,
        DescriptionSnapshot = "Pelayanan uji",
        Quantity = 1,
        UnitPrice = 100_000,
        DoctorShare = 20_000,
        ContractVersion = ContractBillingChargeSourceAdapter.ContractVersion,
        CorrelationId = Guid.NewGuid(),
        CausationId = Guid.NewGuid()
    };

    private static BillingInvoiceService CreateService(Repositories.ApplicationDbContext db)
    {
        var logger = new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor());
        var number = new BillingNumberSeriesService(db, Options.Create(new BillingInvoiceNumberOptions()));
        return new BillingInvoiceService(db, new ContractBillingChargeSourceAdapter(), number, logger);
    }

    private static void AssertPermission(string methodName, string action)
    {
        var attribute = typeof(BillingInvoicesController).GetMethod(methodName)?.GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(attribute);
        var arguments = Assert.IsType<object[]>(attribute!.Arguments);
        Assert.Equal("BillingInvoice", arguments[0]);
        Assert.Equal(action, arguments[1]);
    }
}
