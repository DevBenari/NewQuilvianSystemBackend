using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
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
    public async Task EligibleVoidRetainsHistoryAndCreatesNewCalculationVersion()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, categoryId) = await SeedAsync(db);
        var service = CreateService(db);
        var invoice = await service.UpsertChargeAsync(
            Request(encounterId, categoryId, "PROC-VOID"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var item = Assert.Single(invoice.Items);
        var originalRowVersion = invoice.RowVersion;

        var result = await service.VoidItemAsync(
            invoice.Id,
            item.Id,
            VoidRequest(invoice.RowVersion),
            Guid.NewGuid(),
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.Equal(0, result.ActiveItemCount);
        Assert.Equal(0, result.RunningGrossAmount);
        Assert.Equal(1, result.CurrentCalculationVersion);
        Assert.NotEqual(originalRowVersion, result.RowVersion);
        var voided = Assert.Single(result.Items);
        Assert.Equal(BillingInvoiceItemStatuses.Voided, voided.Status);
        Assert.Equal("Order dibatalkan sebelum pelayanan", voided.VoidReason);
        Assert.False(db.BilInvoiceItems.Single().IsDelete);
        Assert.Equal(0, db.BilCalculationVersions.Single().GrossAmount);
        Assert.Single(result.CalculationVersions);
    }

    [Theory]
    [InlineData("PROCEDURE", "COMPLETED")]
    [InlineData("PROCEDURE", "PERFORMED")]
    [InlineData("LABORATORY", "COMPLETED")]
    [InlineData("RADIOLOGY", "PERFORMED")]
    [InlineData("PHARMACY", "DISPENSED")]
    [InlineData("CONSUMABLE", "USED")]
    public async Task CompletedOrFinalProducerFactsCannotUseNormalVoid(string domain, string status)
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, categoryId) = await SeedAsync(db);
        var service = CreateService(db);
        var invoice = await service.UpsertChargeAsync(
            Request(encounterId, categoryId, $"{domain}-FINAL", domain, status),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BillingInvoiceValidationException>(() =>
            service.VoidItemAsync(
                invoice.Id,
                invoice.Items.Single().Id,
                VoidRequest(invoice.RowVersion),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(
            "Item tidak dapat dibatalkan karena pelayanan atau pembayaran sudah diproses.",
            exception.Message);
        Assert.Equal(BillingInvoiceItemStatuses.Active, db.BilInvoiceItems.Single().Status);
        Assert.Empty(db.BilCalculationVersions);
    }

    [Fact]
    public async Task LockedFinancialSnapshotAndFinalInvoiceAreImmutable()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, categoryId) = await SeedAsync(db);
        var service = CreateService(db);
        var invoice = await service.UpsertChargeAsync(
            Request(encounterId, categoryId, "PROC-LOCKED"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        db.BilCalculationVersions.Add(new BilCalculationVersion
        {
            InvoiceId = invoice.Id,
            VersionNo = 1,
            GrossAmount = 100_000,
            PatientAmount = 100_000,
            IsLocked = true,
            CalculatedAt = DateTimeOffset.UtcNow,
            Reason = "Snapshot pembayaran",
            BreakdownSnapshot = "{}"
        });
        await db.SaveChangesAsync();

        var locked = await Assert.ThrowsAsync<BillingInvoiceValidationException>(() =>
            service.VoidItemAsync(
                invoice.Id, invoice.Items.Single().Id, VoidRequest(invoice.RowVersion),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));
        Assert.Contains("pembayaran sudah diproses", locked.Message);

        var trackedInvoice = db.BilInvoices.Single();
        trackedInvoice.Status = BillingInvoiceStatuses.Final;
        trackedInvoice.RowVersion = Guid.NewGuid();
        await db.SaveChangesAsync();
        var finalRequest = VoidRequest(trackedInvoice.RowVersion);
        var immutable = await Assert.ThrowsAsync<BillingInvoiceValidationException>(() =>
            service.VoidItemAsync(
                invoice.Id, invoice.Items.Single().Id, finalRequest,
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));
        Assert.Equal("Invoice final tidak dapat diedit; ajukan adjustment.", immutable.Message);
        Assert.Equal(BillingInvoiceItemStatuses.Active, db.BilInvoiceItems.Single().Status);
    }

    [Fact]
    public async Task StaleVoidReturnsConflictWithoutMutation()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, categoryId) = await SeedAsync(db);
        var service = CreateService(db);
        var invoice = await service.UpsertChargeAsync(
            Request(encounterId, categoryId, "PROC-CONFLICT"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var request = VoidRequest(Guid.NewGuid());

        var exception = await Assert.ThrowsAsync<BillingInvoiceConflictException>(() =>
            service.VoidItemAsync(
                invoice.Id, invoice.Items.Single().Id, request,
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("Muat ulang", exception.Message);
        Assert.Equal(BillingInvoiceItemStatuses.Active, db.BilInvoiceItems.Single().Status);
        Assert.Empty(db.BilCalculationVersions);
    }

    [Fact]
    public async Task IdenticalVoidReplayDoesNotCreateAnotherVersion()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, categoryId) = await SeedAsync(db);
        var service = CreateService(db);
        var invoice = await service.UpsertChargeAsync(
            Request(encounterId, categoryId, "PROC-VOID-REPLAY"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var request = VoidRequest(invoice.RowVersion);
        var key = Guid.NewGuid();

        var first = await service.VoidItemAsync(
            invoice.Id, invoice.Items.Single().Id, request,
            key, Guid.NewGuid(), CancellationToken.None);
        var replay = await service.VoidItemAsync(
            invoice.Id, invoice.Items.Single().Id, request,
            key, Guid.NewGuid(), CancellationToken.None);

        Assert.False(first.IsReplay);
        Assert.True(replay.IsReplay);
        Assert.Single(db.BilCalculationVersions);
    }

    [Fact]
    public async Task OpenInvoicePriceChangeProducesASeparateImmutableVersion()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, categoryId) = await SeedAsync(db);
        var logger = new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor());
        var service = CreateService(db, logger);
        var calculation = CreateCalculationService(db, logger);
        var source = Request(encounterId, categoryId, "PROC-REPRICE");
        var invoice = await service.UpsertChargeAsync(
            source, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var first = await calculation.RecalculateAsync(
            invoice.Id,
            new RecalculateInvoiceRequest { ExpectedRowVersion = invoice.RowVersion, Reason = "Harga awal" },
            Guid.NewGuid(), CancellationToken.None);

        source.SourceVersion = 2;
        source.UnitPrice = 125_000;
        source.CorrelationId = Guid.NewGuid();
        source.CausationId = Guid.NewGuid();
        var updated = await service.UpsertChargeAsync(
            source, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var second = await calculation.RecalculateAsync(
            invoice.Id,
            new RecalculateInvoiceRequest { ExpectedRowVersion = updated.RowVersion, Reason = "Harga baru" },
            Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(100_000, first.GrossAmount);
        Assert.Equal(125_000, second.GrossAmount);
        Assert.Equal(1, first.VersionNo);
        Assert.Equal(2, second.VersionNo);
        Assert.Equal(100_000, db.BilCalculationVersions.Single(x => x.VersionNo == 1).GrossAmount);
    }

    [Fact]
    public async Task VoidAuditDoesNotWriteClinicalDescriptionOrSourceReference()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, categoryId) = await SeedAsync(db);
        var capture = new CaptureLogger<LoggerService>();
        var logger = new LoggerService(capture, new HttpContextAccessor());
        var service = CreateService(db, logger);
        var source = Request(encounterId, categoryId, "SENSITIVE-SOURCE-REFERENCE");
        source.DescriptionSnapshot = "SENSITIVE-CLINICAL-DESCRIPTION";
        var invoice = await service.UpsertChargeAsync(
            source, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        await service.VoidItemAsync(
            invoice.Id, invoice.Items.Single().Id, VoidRequest(invoice.RowVersion),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.Contains(capture.Messages, x => x.Contains("BILLINGINVOICE_VOIDITEM", StringComparison.Ordinal));
        Assert.DoesNotContain(capture.Messages, x => x.Contains("SENSITIVE-SOURCE-REFERENCE", StringComparison.Ordinal));
        Assert.DoesNotContain(capture.Messages, x => x.Contains("SENSITIVE-CLINICAL-DESCRIPTION", StringComparison.Ordinal));
    }

    [Fact]
    public void EndpointsRequireExactBillingInvoicePermissions()
    {
        AssertPermission(nameof(BillingInvoicesController.Get), "Read");
        AssertPermission(nameof(BillingInvoicesController.GetDetail), "Read");
        AssertPermission(nameof(BillingInvoicesController.FromSource), "Create");
        AssertPermission(nameof(BillingInvoicesController.Recalculate), "Update");
        AssertPermission(nameof(BillingInvoicesController.VoidItem), "Update");
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

    private static VoidInvoiceItemRequest VoidRequest(Guid rowVersion) => new()
    {
        ExpectedRowVersion = rowVersion,
        SourceVersion = 2,
        SourceStatus = "CANCELLED",
        ContractVersion = ContractBillingChargeSourceAdapter.ContractVersion,
        Reason = "Order dibatalkan sebelum pelayanan",
        CorrelationId = Guid.NewGuid(),
        CausationId = Guid.NewGuid()
    };

    private static BillingInvoiceService CreateService(
        Repositories.ApplicationDbContext db,
        LoggerService? logger = null)
    {
        logger ??= new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor());
        var number = new BillingNumberSeriesService(db, Options.Create(new BillingInvoiceNumberOptions()));
        var calculation = CreateCalculationService(db, logger);
        return new BillingInvoiceService(
            db, new ContractBillingChargeSourceAdapter(), number, calculation, logger);
    }

    private static BillingCalculationService CreateCalculationService(
        Repositories.ApplicationDbContext db,
        LoggerService logger) =>
        new(
            db,
            new RegistrationBillingCoverageAdapter(db),
            new BillingAllocationService(db, logger),
            logger);

    private static void AssertPermission(string methodName, string action)
    {
        var attribute = typeof(BillingInvoicesController).GetMethod(methodName)?.GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(attribute);
        var arguments = Assert.IsType<object[]>(attribute!.Arguments);
        Assert.Equal("BillingInvoice", arguments[0]);
        Assert.Equal(action, arguments[1]);
    }

    private sealed class CaptureLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            Messages.Add(formatter(state, exception));
    }
}
