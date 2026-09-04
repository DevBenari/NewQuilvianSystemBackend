using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.BillingManagement;

public sealed class BillingInvoiceServiceTests
{
    [Fact]
    public async Task AdhocChargeFromMenuPembayaranIsAcceptedAndVoidableBeforeLock()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, categoryId) = await SeedAsync(db);
        var service = CreateService(db);

        var created = await service.UpsertChargeAsync(
            Request(encounterId, categoryId, "ADHOC-1", "ADHOC", "ADDED"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.Single(created.Items);
        Assert.Equal("ADHOC", created.Items[0].SourceDomain);

        var voidRequest = VoidRequest(created.RowVersion);
        voidRequest.SourceStatus = "VOIDED";
        var voided = await service.VoidItemAsync(
            created.Id, created.Items[0].Id,
            voidRequest,
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(0, voided.ActiveItemCount);
    }

    [Fact]
    public async Task GetDetailIncludesPatientSummaryForMenuPembayaran()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, categoryId) = await SeedAsync(db);
        var encounter = await db.TrxPatientEncounters.SingleAsync(x => x.Id == encounterId);
        db.MstPatients.Add(new MstPatient
        {
            Id = encounter.PatientId,
            PatientCode = "PAT-0001",
            MedicalRecordNumber = "RM-0001",
            FullName = "Andrea Wijaya",
            Gender = Gender.Female
        });
        await db.SaveChangesAsync();
        var service = CreateService(db);
        var created = await service.UpsertChargeAsync(
            Request(encounterId, categoryId, "PROC-PATIENT-1"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        var detail = await service.GetDetailAsync(created.Id, CancellationToken.None);

        Assert.NotNull(detail.Patient);
        Assert.Equal("RM-0001", detail.Patient!.MedicalRecordNumber);
        Assert.Equal("Andrea Wijaya", detail.Patient.FullName);
        Assert.Equal(encounter.EncounterNumber, detail.Patient.EncounterNumber);
    }

    [Fact]
    public async Task ActiveEncounterOptionsExposeServiceUnitClinicAndPatientClassForTariffFiltering()
    {
        // BKC-DEC-061: FE memakai tiga field ini untuk memfilter katalog tarif per konteks
        // kunjungan (unit layanan/klinik/kelas pasien). Consumer lama yang belum membaca field
        // ini tetap aman karena penambahannya aditif pada response yang sudah ada.
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, _) = await SeedAsync(db);
        var encounter = await db.TrxPatientEncounters.SingleAsync(x => x.Id == encounterId);
        var clinicId = Guid.NewGuid();
        var patientClassId = Guid.NewGuid();
        encounter.ClinicId = clinicId;
        encounter.PatientClassId = patientClassId;
        db.MstPatients.Add(new MstPatient
        {
            Id = encounter.PatientId,
            PatientCode = "PAT-0002",
            MedicalRecordNumber = "RM-0002",
            FullName = "Budi Santoso",
            Gender = Gender.Male
        });
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var options = await service.GetActiveEncounterOptionsAsync(null, 10, CancellationToken.None);

        var option = Assert.Single(options, x => x.Id == encounterId);
        Assert.Equal(encounter.ServiceUnitId, option.ServiceUnitId);
        Assert.Equal(clinicId, option.ClinicId);
        Assert.Equal(patientClassId, option.PatientClassId);
    }

    [Fact]
    public async Task CatalogChargeUsesServerSidePriceCategoryAndTariffIdFromActiveTariff()
    {
        // BIL-AT-025: kasir pilih tarif di dropdown, submit tanpa field harga sama sekali - harga,
        // kategori, dan deskripsi seluruhnya berasal dari MstTariff, bukan dari client.
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, categoryId) = await SeedAsync(db);
        var tariffId = Guid.NewGuid();
        db.MstTariffs.Add(Tariff(tariffId, categoryId, "Konsultasi Spesialis", 150_000m));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.AddCatalogChargeAsync(
            CatalogChargeRequest(encounterId, tariffId), Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal("ADHOC_CATALOG", item.SourceDomain);
        Assert.Equal("Konsultasi Spesialis", item.DescriptionSnapshot);
        Assert.Equal(150_000m, item.UnitPrice);
        Assert.Equal(tariffId, (await db.BilInvoiceItems.SingleAsync(x => x.Id == item.Id)).TariffId);
    }

    [Fact]
    public async Task CatalogChargeRejectsInactiveTariff()
    {
        // BIL-AT-026 (paruh nonaktif): TariffId valid tapi IsActive=false.
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, categoryId) = await SeedAsync(db);
        var tariffId = Guid.NewGuid();
        var tariff = Tariff(tariffId, categoryId, "Tarif Nonaktif", 100_000m);
        tariff.IsActive = false;
        db.MstTariffs.Add(tariff);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        await Assert.ThrowsAsync<BillingInvoiceValidationException>(() =>
            service.AddCatalogChargeAsync(
                CatalogChargeRequest(encounterId, tariffId), Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));
        Assert.Empty(db.BilInvoiceItems);
    }

    [Fact]
    public async Task CatalogChargeRejectsExpiredTariff()
    {
        // BIL-AT-026 (paruh kedaluwarsa): TariffId valid, aktif, tapi EffectiveEndDate sudah lewat.
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, categoryId) = await SeedAsync(db);
        var tariffId = Guid.NewGuid();
        var tariff = Tariff(tariffId, categoryId, "Tarif Kedaluwarsa", 100_000m);
        tariff.EffectiveEndDate = DateTime.UtcNow.AddDays(-1);
        db.MstTariffs.Add(tariff);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        await Assert.ThrowsAsync<BillingInvoiceValidationException>(() =>
            service.AddCatalogChargeAsync(
                CatalogChargeRequest(encounterId, tariffId), Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));
        Assert.Empty(db.BilInvoiceItems);
    }

    [Fact]
    public async Task CatalogChargeIdempotencyReplayIsNoOpForSameKeyButNewKeyAddsAnotherItem()
    {
        // SourceDetailId pada AddCatalogChargeAsync diturunkan dari idempotencyKey (bukan Guid acak
        // per panggilan) supaya retry client dengan Idempotency-Key yang sama benar-benar no-op,
        // bukan salah dianggap konflik oleh UpsertChargeAsync.
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, categoryId) = await SeedAsync(db);
        var tariffId = Guid.NewGuid();
        db.MstTariffs.Add(Tariff(tariffId, categoryId, "Tindakan Ranap", 200_000m));
        await db.SaveChangesAsync();
        var service = CreateService(db);
        var request = CatalogChargeRequest(encounterId, tariffId);
        var key = Guid.NewGuid();

        var first = await service.AddCatalogChargeAsync(request, key, Guid.NewGuid(), CancellationToken.None);
        var replay = await service.AddCatalogChargeAsync(request, key, Guid.NewGuid(), CancellationToken.None);
        var second = await service.AddCatalogChargeAsync(request, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.False(first.IsReplay);
        Assert.True(replay.IsReplay);
        Assert.Single(replay.Items);
        Assert.Equal(2, second.Items.Count);
    }

    [Fact]
    public async Task CatalogChargeCoveragePreviewIsCoveredEvenWhenRuleNeedsApproval()
    {
        // BIL-AT-027: rule dengan IsNeedApproval=true tetap dihitung Covered penuh pada preview -
        // approval hanya informasi, BUKAN status NotCovered/gagal (BKC-DEC-060/062).
        await using var db = IsolatedBillingDbContextFactory.Create();
        var patientId = Guid.NewGuid();
        var encounterId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var patientInsuranceId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var tariffId = Guid.NewGuid();

        db.TrxPatientEncounters.Add(new TrxPatientEncounter
        {
            Id = encounterId,
            EncounterNumber = "ENC-COVERAGE-1",
            PatientId = patientId,
            ServiceUnitId = Guid.NewGuid(),
            EncounterType = EncounterType.Inpatient,
            EncounterStatus = EncounterStatus.Registered,
            PaymentType = EncounterPaymentType.Insurance,
            IsActive = true
        });
        db.TrxPatientEncounterGuarantors.Add(new TrxPatientEncounterGuarantor
        {
            EncounterId = encounterId,
            PatientId = patientId,
            PaymentSourceNumber = "PS-1",
            PaymentType = EncounterPaymentType.Insurance,
            PatientInsuranceId = patientInsuranceId,
            InsuranceProviderId = providerId,
            IsActive = true,
            IsEligible = true
        });
        db.MstInsuranceProviders.Add(new MstInsuranceProvider
        {
            Id = providerId,
            InsuranceProviderCode = "INS-1",
            InsuranceProviderName = "Asuransi Uji",
            IsActive = true
        });
        db.MstPatientInsurances.Add(new MstPatientInsurance
        {
            Id = patientInsuranceId,
            PatientId = patientId,
            InsuranceProviderId = providerId,
            PolicyNumber = "POL-1",
            IsActive = true,
            IsEligible = true
        });
        db.MstTariffCategories.Add(new MstTariffCategory
        {
            Id = categoryId,
            TariffCategoryCode = "CONSULT",
            TariffCategoryName = "Konsultasi",
            IsActive = true
        });
        db.MstTariffs.Add(new MstTariff
        {
            Id = tariffId,
            TariffCode = "TRF-COVER-1",
            TariffName = "Konsultasi Spesialis",
            TariffCategoryId = categoryId,
            NormalPrice = 200_000m,
            IsActive = true
        });
        db.MstInsuranceTariffs.Add(new MstInsuranceTariff
        {
            InsuranceProviderId = providerId,
            TariffId = tariffId,
            InsuranceTariffCode = "INS-TRF-1",
            InsuranceTariffName = "Konsultasi Spesialis (Kontrak)",
            ContractPrice = 180_000m,
            IsUsingContractPrice = true,
            IsActive = true
        });
        db.MstInsuranceCoverageRules.Add(new MstInsuranceCoverageRule
        {
            InsuranceProviderId = providerId,
            RuleCode = "RULE-1",
            RuleName = "Butuh approval",
            ItemType = "Tariff",
            TariffId = tariffId,
            CoverageStatus = "Covered",
            CoveragePercent = 100,
            IsNeedApproval = true,
            IsActive = true
        });
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.GetCatalogChargeCoveragePreviewAsync(
            encounterId, tariffId, 1, CancellationToken.None);

        Assert.NotEqual("NotCovered", result.CoverageStatus);
        Assert.True(result.CoveredAmount > 0);
        Assert.True(result.IsNeedApproval);
        Assert.True(result.IsAdvisory);
    }

    [Fact]
    public async Task CatalogChargeCoveragePreviewRejectsUnknownEncounter()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);

        await Assert.ThrowsAsync<BillingInvoiceValidationException>(() =>
            service.GetCatalogChargeCoveragePreviewAsync(
                Guid.NewGuid(), Guid.NewGuid(), 1, CancellationToken.None));
    }

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
        db.MstTariffCategories.Add(new MstTariffCategory
        {
            Id = categoryId,
            TariffCategoryCode = "PROC",
            TariffCategoryName = "Procedure",
            IsActive = true
        });
        await db.SaveChangesAsync();
        return (encounterId, categoryId);
    }

    private static MstTariff Tariff(Guid id, Guid categoryId, string name, decimal normalPrice) => new()
    {
        Id = id,
        TariffCode = "TRF-" + id.ToString("N")[..8].ToUpperInvariant(),
        TariffName = name,
        TariffCategoryId = categoryId,
        NormalPrice = normalPrice,
        IsActive = true
    };

    private static AddCatalogChargeRequest CatalogChargeRequest(Guid encounterId, Guid tariffId) => new()
    {
        EncounterId = encounterId,
        TariffId = tariffId,
        Quantity = 1,
        CorrelationId = Guid.NewGuid(),
        CausationId = Guid.NewGuid()
    };

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
            db, new ContractBillingChargeSourceAdapter(), number, calculation, logger,
            new InsuranceCoverageService(db, new EncounterInsuranceService(db)));
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
