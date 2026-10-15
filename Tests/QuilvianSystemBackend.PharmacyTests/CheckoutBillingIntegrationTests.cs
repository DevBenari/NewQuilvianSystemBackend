using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Rantai checkout klinis → invoice kasir sesudah <c>PLAN-REPAIR-BKC-001</c>
/// (<c>BE-BKC-FIX-015</c> s.d. <c>021</c>).
/// </summary>
/// <remarks>
/// Yang diuji adalah keputusan Billing atas fakta yang sudah tersimpan: tarif dan jumlah yang
/// ditagih, kontrak sumber yang diterima, dan hitungan pelayanan yang belum masuk invoice.
/// Penulisan item invoice sendiri (transaksi serializable PostgreSQL) tidak dijalankan di sini.
/// </remarks>
public class CheckoutBillingIntegrationTests
{
    private static readonly DateTime Waktu = new(2026, 10, 12, 3, 0, 0, DateTimeKind.Utc);

    // ====================================================== kontrak sumber

    [Fact]
    public void Sumber_pemakaian_nonresep_dan_Bank_Darah_diteruskan_ke_invoice()
    {
        Assert.True(BillingSourceContract.IsAllowedEffectType(
            BillingSourceContract.DrugUsageSourceContext, BillingSourceContract.DrugUsageChargeEffectType));
        Assert.True(BillingClinicalChargeBridgeService.IsCandidateSourceContext(BillingSourceContract.DrugUsageSourceContext));
        Assert.True(BillingClinicalChargeBridgeService.IsCandidateSourceContext(BillingSourceContract.BloodBankSourceContext));
    }

    [Theory]
    [InlineData("BLOOD_BANK", "COMPLETED")]
    [InlineData("CONSUMABLE", "USED")]
    public void Domain_baru_diterima_validator_Billing_dan_dianggap_selesai(string domain, string status)
    {
        var adapter = new ContractBillingChargeSourceAdapter();

        var snapshot = adapter.ValidateAndNormalize(Tagihan(domain, status));

        Assert.Equal(domain, snapshot.SourceDomain);
        Assert.True(adapter.IsOrderComplete(new BilInvoiceItem { SourceDomain = domain, SourceStatus = status }));
        Assert.False(adapter.IsNormallyVoidable(domain, status));
    }

    /// <summary>
    /// Tagihan resep tahap 1 tetap diterima validator Billing yang sebenarnya — dipindah dari uji
    /// pengirim langsung yang dihapus <c>BE-BKC-FIX-015</c>; jembatan memakai status dan kontrak
    /// yang sama.
    /// </summary>
    [Fact]
    public void Tagihan_resep_tahap_satu_diterima_pada_kontrak_1_3()
    {
        var snapshot = new ContractBillingChargeSourceAdapter().ValidateAndNormalize(Tagihan("PHARMACY", "PRESCRIBED"));

        Assert.Equal("PRESCRIBED", snapshot.SourceStatus);
    }

    [Theory]
    [InlineData("SUBMITTED", "BIL-INTEGRATION-1.3")]
    [InlineData("PRESCRIBED", "BIL-INTEGRATION-1.2")]
    public void Status_atau_kontrak_resep_lama_ditolak(string status, string kontrak)
    {
        Assert.ThrowsAny<Exception>(() =>
            new ContractBillingChargeSourceAdapter().ValidateAndNormalize(Tagihan("PHARMACY", status, kontrak)));
    }

    // ============================================================ racikan (K-02)

    [Fact]
    public async Task Resep_dengan_racikan_menagih_bahan_kali_tarif_obat()
    {
        using var db = TestDatabase.Create();
        await using var k = db.CreateContext();
        var seed = SeedResep(k, denganRacikan: true, tarifBahan: true);
        await k.SaveChangesAsync();

        var hasil = await new BillingSourceTariffResolver(k).ResolveAsync(
            Permintaan(BillingBridgeSourceDomains.Pharmacy, seed.ResepId), CancellationToken.None);

        // 2 × Rp10.000 (obat reguler) + 3 × Rp2.000 (bahan racikan) = Rp26.000, satu baris per resep.
        Assert.Equal(BillingTariffResolutionKind.Resolved, hasil.Kind);
        Assert.Equal(1m, hasil.Quantity);
        Assert.Equal(26_000m, hasil.UnitPrice);
    }

    [Fact]
    public async Task Bahan_racikan_tanpa_tarif_menahan_seluruh_resep()
    {
        using var db = TestDatabase.Create();
        await using var k = db.CreateContext();
        var seed = SeedResep(k, denganRacikan: true, tarifBahan: false);
        await k.SaveChangesAsync();

        var hasil = await new BillingSourceTariffResolver(k).ResolveAsync(
            Permintaan(BillingBridgeSourceDomains.Pharmacy, seed.ResepId), CancellationToken.None);

        Assert.Equal(BillingTariffResolutionKind.Rejected, hasil.Kind);
        Assert.Equal(BillingBridgeCodes.TariffNotFound, hasil.Code);
    }

    [Fact]
    public async Task Tahap_serah_memakai_jumlah_diserahkan_dan_bahan_racikan_tetap_ditagih()
    {
        using var db = TestDatabase.Create();
        await using var k = db.CreateContext();
        var seed = SeedResep(k, denganRacikan: true, tarifBahan: true);
        await k.SaveChangesAsync();

        var hasil = await new BillingSourceTariffResolver(k).ResolveAsync(
            Permintaan(BillingBridgeSourceDomains.Pharmacy, seed.ResepId,
                dispensed: new Dictionary<Guid, decimal> { [seed.ItemResepId] = 1m }),
            CancellationToken.None);

        // 1 diserahkan × Rp10.000 + bahan racikan 3 × Rp2.000.
        Assert.Equal(16_000m, hasil.UnitPrice);
    }

    // ===================================================== pemakaian nonresep (K-04)

    [Fact]
    public async Task Tiga_spuit_nonresep_ditagih_per_butir_dengan_tarif_alkes()
    {
        using var db = TestDatabase.Create();
        await using var k = db.CreateContext();
        var seed = SeedPemakaian(k, DrugUsageStatus.NotBilled, prescriptionId: null, denganTarif: true);
        await k.SaveChangesAsync();

        var hasil = await new BillingSourceTariffResolver(k).ResolveAsync(
            Permintaan(BillingBridgeSourceDomains.Consumable, seed.UsageId, seed.ItemId, factQuantity: 3m),
            CancellationToken.None);

        Assert.Equal(BillingTariffResolutionKind.Resolved, hasil.Kind);
        Assert.Equal(3m, hasil.Quantity);
        Assert.Equal(5_000m, hasil.UnitPrice);
        Assert.Equal(15_000m, hasil.Quantity * hasil.UnitPrice);
    }

    [Fact]
    public async Task Penyerahan_resep_tidak_ditagih_lagi_sebagai_pemakaian()
    {
        using var db = TestDatabase.Create();
        await using var k = db.CreateContext();
        var seed = SeedPemakaian(k, DrugUsageStatus.NotBilled, prescriptionId: Guid.NewGuid(), denganTarif: true);
        await k.SaveChangesAsync();

        var hasil = await new BillingSourceTariffResolver(k).ResolveAsync(
            Permintaan(BillingBridgeSourceDomains.Consumable, seed.UsageId, seed.ItemId, factQuantity: 3m),
            CancellationToken.None);

        Assert.Equal(BillingTariffResolutionKind.NotBillable, hasil.Kind);
    }

    [Fact]
    public async Task Draft_pemakaian_tidak_pernah_ditagih()
    {
        using var db = TestDatabase.Create();
        await using var k = db.CreateContext();
        var seed = SeedPemakaian(k, DrugUsageStatus.Draft, prescriptionId: null, denganTarif: true);
        await k.SaveChangesAsync();

        var hasil = await new BillingSourceTariffResolver(k).ResolveAsync(
            Permintaan(BillingBridgeSourceDomains.Consumable, seed.UsageId, seed.ItemId, factQuantity: 3m),
            CancellationToken.None);

        Assert.Equal(BillingTariffResolutionKind.Rejected, hasil.Kind);
    }

    [Fact]
    public async Task Pemakaian_tanpa_tarif_masuk_rekonsiliasi_bukan_nol()
    {
        using var db = TestDatabase.Create();
        await using var k = db.CreateContext();
        var seed = SeedPemakaian(k, DrugUsageStatus.NotBilled, prescriptionId: null, denganTarif: false);
        await k.SaveChangesAsync();

        var hasil = await new BillingSourceTariffResolver(k).ResolveAsync(
            Permintaan(BillingBridgeSourceDomains.Consumable, seed.UsageId, seed.ItemId, factQuantity: 3m),
            CancellationToken.None);

        Assert.Equal(BillingTariffResolutionKind.Rejected, hasil.Kind);
        Assert.Equal(BillingBridgeCodes.TariffNotFound, hasil.Code);
    }

    // ========================================================== Bank Darah (K-06)

    [Fact]
    public async Task Tindakan_Bank_Darah_satu_baris_per_tindakan_dari_tarif_berlaku()
    {
        using var db = TestDatabase.Create();
        await using var k = db.CreateContext();
        var orderId = Guid.NewGuid();
        var tindakanId = Guid.NewGuid();
        var tarifId = Guid.NewGuid();
        k.Set<MstTariff>().Add(Tarif(tarifId, "UJI-TRF-BD", 350_000m, procedureId: Guid.NewGuid()));
        k.Set<BbkBloodBankProcedure>().Add(new BbkBloodBankProcedure
        {
            Id = tindakanId,
            ProcedureNumber = "UJI-BD-0001",
            BloodOrderId = orderId,
            ServiceUnitId = Guid.NewGuid(),
            BdrsDoctorId = Guid.NewGuid(),
            PerformedByUserId = Guid.NewGuid(),
            PatientClassId = Guid.NewGuid(),
            ProcedureRefId = Guid.NewGuid(),
            TariffId = tarifId,
            ProcedureCodeSnapshot = "UJI-TRANSFUSI",
            ProcedureNameSnapshot = "Transfusi (Uji)",
            TariffAmountSnapshot = 1m,
            ProcedureStatus = BbkProcedureStatus.Completed,
            CreateDateTime = Waktu
        });
        await k.SaveChangesAsync();

        var hasil = await new BillingSourceTariffResolver(k).ResolveAsync(
            Permintaan(BillingBridgeSourceDomains.BloodBank, orderId, tindakanId, factQuantity: 1m),
            CancellationToken.None);

        // Harga dibaca dari MstTariff yang berlaku, bukan dari salinan klinis (Rp1 pada uji ini).
        Assert.Equal(BillingTariffResolutionKind.Resolved, hasil.Kind);
        Assert.Equal(1m, hasil.Quantity);
        Assert.Equal(350_000m, hasil.UnitPrice);
    }

    // ============================================== pelayanan tertunda (FIX-09/11)

    [Fact]
    public async Task Hanya_pelayanan_eligible_yang_belum_masuk_invoice_yang_dihitung_tertunda()
    {
        using var db = TestDatabase.Create();
        await using var k = db.CreateContext();
        var kunjungan = Guid.NewGuid();
        var folio = new BilFolio { Id = Guid.NewGuid(), EncounterId = kunjungan, CreateDateTime = Waktu };
        k.Set<BilFolio>().Add(folio);
        k.Set<BilProcessingEffect>().AddRange(
            Efek(folio.Id, BillingInvoiceSyncStatus.Pending),
            Efek(folio.Id, BillingInvoiceSyncStatus.Failed),
            Efek(folio.Id, BillingInvoiceSyncStatus.ReconciliationRequired, BillingBridgeCodes.TariffNotFound),
            Efek(folio.Id, BillingInvoiceSyncStatus.ReconciliationRequired, BillingBridgeCodes.SourceRejected),
            // Tidak dihitung: tahap belum eligible/gratis, sudah masuk, atau sudah diselesaikan.
            Efek(folio.Id, BillingInvoiceSyncStatus.NotApplicable),
            Efek(folio.Id, BillingInvoiceSyncStatus.Synced),
            Efek(folio.Id, BillingInvoiceSyncStatus.Resolved));
        k.Set<CliClinicalMilestoneFact>().AddRange(
            Fakta(kunjungan, ClinicalFactDispatchStatus.Pending),
            Fakta(kunjungan, ClinicalFactDispatchStatus.Dispatched),
            Fakta(kunjungan, ClinicalFactDispatchStatus.Pending, resolved: true));
        await k.SaveChangesAsync();

        var semua = await BillingPendingChargeQuery.CountByEncounterAsync(k, [kunjungan], CancellationToken.None);
        var tanpaTarifKosong = await BillingPendingChargeQuery.CountByEncounterAsync(
            k, [kunjungan], CancellationToken.None, excludeTariffNotFound: true);

        // Efek: Pending, Failed, 2 × ReconciliationRequired; fakta: 1 Pending yang belum diselesaikan.
        Assert.Equal(5, semua[kunjungan]);
        Assert.Equal(4, tanpaTarifKosong[kunjungan]);
    }

    // ================================================================ bantuan

    private static UpsertChargeRequest Tagihan(string domain, string status, string? kontrak = null) => new()
    {
        EncounterId = Guid.NewGuid(),
        SourceDomain = domain,
        SourceDetailId = Guid.NewGuid().ToString("D"),
        SourceVersion = 1,
        SourceStatus = status,
        OccurredAt = DateTimeOffset.UtcNow,
        CategoryId = Guid.NewGuid(),
        DescriptionSnapshot = "Tagihan uji",
        Quantity = 1,
        UnitPrice = 50_000m,
        DoctorShare = 0,
        ContractVersion = kontrak ?? ContractBillingChargeSourceAdapter.IntegrationContractVersion13,
        CorrelationId = Guid.NewGuid(),
        CausationId = Guid.NewGuid()
    };

    private static BillingTariffResolutionRequest Permintaan(
        string domain, Guid aggregateId, Guid? itemId = null, decimal? factQuantity = null,
        IReadOnlyDictionary<Guid, decimal>? dispensed = null) =>
        new(domain, aggregateId, itemId, factQuantity, null, null, Waktu, dispensed);

    private static MstTariff Tarif(Guid id, string kode, decimal harga, Guid? drugId = null, Guid? procedureId = null) => new()
    {
        Id = id,
        TariffCode = kode,
        TariffName = $"Tarif {kode}",
        TariffCategoryId = Guid.NewGuid(),
        DrugId = drugId,
        ProcedureId = procedureId,
        NormalPrice = harga,
        IsActive = true,
        CreateDateTime = Waktu
    };

    private sealed record ResepUji(Guid ResepId, Guid ItemResepId);

    private static ResepUji SeedResep(ApplicationDbContext k, bool denganRacikan, bool tarifBahan)
    {
        var resepId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var obatReguler = Guid.NewGuid();
        var obatBahan = Guid.NewGuid();

        k.Set<PhmPrescription>().Add(new PhmPrescription
        {
            Id = resepId,
            PrescriptionNumber = "UJI-RX-RACIK",
            EncounterId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            PrescriptionDateTime = Waktu,
            IsActive = true,
            CreateDateTime = Waktu
        });
        k.Set<PhmPrescriptionItem>().Add(new PhmPrescriptionItem
        {
            Id = itemId,
            PrescriptionId = resepId,
            DrugId = obatReguler,
            DrugNameSnapshot = "Obat Reguler (Uji)",
            Quantity = 2,
            IsActive = true,
            CreateDateTime = Waktu
        });
        k.Set<MstTariff>().Add(Tarif(Guid.NewGuid(), "UJI-TRF-REG", 10_000m, drugId: obatReguler));

        if (denganRacikan)
        {
            var racikanId = Guid.NewGuid();
            k.Set<PhmPrescriptionCompound>().Add(new PhmPrescriptionCompound
            {
                Id = racikanId,
                PrescriptionId = resepId,
                CompoundName = "Puyer Uji",
                IsActive = true,
                CreateDateTime = Waktu
            });
            k.Set<PhmPrescriptionCompoundItem>().Add(new PhmPrescriptionCompoundItem
            {
                Id = Guid.NewGuid(),
                PrescriptionCompoundId = racikanId,
                DrugId = obatBahan,
                DrugCodeSnapshot = "UJI-BAHAN",
                DrugNameSnapshot = "Bahan Racikan (Uji)",
                PricingQuantity = 3,
                IsActive = true,
                CreateDateTime = Waktu
            });
            if (tarifBahan)
                k.Set<MstTariff>().Add(Tarif(Guid.NewGuid(), "UJI-TRF-BAHAN", 2_000m, drugId: obatBahan));
        }

        return new ResepUji(resepId, itemId);
    }

    private sealed record PemakaianUji(Guid UsageId, Guid ItemId);

    private static PemakaianUji SeedPemakaian(ApplicationDbContext k, DrugUsageStatus status, Guid? prescriptionId, bool denganTarif)
    {
        var usageId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var spuit = Guid.NewGuid();

        k.Set<PhmDrugUsage>().Add(new PhmDrugUsage
        {
            Id = usageId,
            UsageNumber = "USG-UJI-0001",
            EncounterId = Guid.NewGuid(),
            PrescriptionId = prescriptionId,
            StorageLocationId = Guid.NewGuid(),
            RecordedByWorkforceId = Guid.NewGuid(),
            Status = status,
            UsedAt = Waktu,
            ItemCount = 1,
            CreateDateTime = Waktu
        });
        k.Set<PhmDrugUsageItem>().Add(new PhmDrugUsageItem
        {
            Id = itemId,
            DrugUsageId = usageId,
            DrugId = spuit,
            MeasurementId = Guid.NewGuid(),
            DrugCodeSnapshot = "UJI-SPUIT",
            DrugNameSnapshot = "Spuit 3 ml (Uji)",
            Quantity = 3,
            LineNumber = 1,
            CreateDateTime = Waktu
        });
        if (denganTarif)
            k.Set<MstTariff>().Add(Tarif(Guid.NewGuid(), "UJI-TRF-SPUIT", 5_000m, drugId: spuit));

        return new PemakaianUji(usageId, itemId);
    }

    private static BilProcessingEffect Efek(Guid folioId, BillingInvoiceSyncStatus status, string? code = null) => new()
    {
        Id = Guid.NewGuid(),
        Consumer = BillingFolioServiceConsumer,
        OperationType = "RecognizeMilestone",
        IdempotencyKey = Guid.NewGuid().ToString("N"),
        RequestFingerprint = "uji",
        SourceContext = BillingSourceContract.ProcedureSourceContext,
        MilestoneFactId = Guid.NewGuid(),
        MilestoneFactVersion = 1,
        EffectType = BillingSourceContract.ProcedureChargeEffectType,
        OccurredAt = Waktu,
        FolioId = folioId,
        InvoiceSyncStatus = status,
        InvoiceSyncErrorCode = code,
        CreateDateTime = Waktu
    };

    private const string BillingFolioServiceConsumer = "ClinicalFactProducer";

    private static CliClinicalMilestoneFact Fakta(Guid encounterId, ClinicalFactDispatchStatus status, bool resolved = false) => new()
    {
        Id = Guid.NewGuid(),
        SourceContext = BillingSourceContract.ProcedureSourceContext,
        SourceAggregateId = Guid.NewGuid(),
        EffectType = BillingSourceContract.ProcedureChargeEffectType,
        MilestoneFactId = Guid.NewGuid(),
        MilestoneFactVersion = 1,
        MilestoneKind = ClinicalMilestoneKind.ChargeEligibility,
        EncounterId = encounterId,
        OccurredAt = Waktu,
        IdempotencyKey = Guid.NewGuid().ToString("N"),
        PayloadFingerprint = "uji",
        DispatchStatus = status,
        ReconciliationResolvedAt = resolved ? Waktu : null,
        CreateDateTime = Waktu
    };
}
