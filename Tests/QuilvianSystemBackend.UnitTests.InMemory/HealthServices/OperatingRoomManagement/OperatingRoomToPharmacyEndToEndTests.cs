using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.OperatingRoomManagement;

/// <summary>
/// Perjalanan penuh satu kasus operasi terhadap persediaan Farmasi.
/// </summary>
/// <remarks>
/// <para>
/// Berkas ini berbeda maksudnya dari pengujian per service. Di sini tidak ada satu pun baris
/// yang disiapkan langsung ke basis data untuk memotong jalan: pemakaian dicatat lewat service
/// Operasi, pembukuan lewat consumer, dan returnya lewat service Farmasi — persis seperti yang
/// akan terjadi di lapangan. Yang dibuktikan adalah bahwa keempat modul itu benar-benar
/// tersambung, bukan bahwa masing-masing benar sendiri-sendiri.
/// </para>
/// <para>
/// Alurnya: kasus operasi dibuat, material dipakai, outbox dibukukan, stok Farmasi berkurang,
/// sisanya diretur, apoteker memeriksa, dan stok yang layak kembali.
/// </para>
/// </remarks>
public class OperatingRoomToPharmacyEndToEndTests
{
    private static readonly Guid VialUnitId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [Fact]
    public async Task Operasi_PakaiMaterial_LaluRetur_StokBerkurangDanKembaliSetelahDiperiksa()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var world = await SetupAsync(ctx, onHand: 100m);

        var material = BuildMaterialService(ctx);
        var dispatch = BuildDispatchService(ctx);
        var returns = BuildReturnService(ctx);

        // ---------------------------------------------------------- 1. dipakai

        var usage = await material.RecordAsync(ctx.CaseId, new CreateOprMaterialUsageRequest
        {
            ExternalItemId = world.DrugId,
            ItemType = OprMaterialItemType.Consumable,
            Quantity = 10m,
            UnitCode = "VIAL",
            UnitMeasurementId = VialUnitId,
            Outcome = OprMaterialOutcome.Used,
            BatchNumber = "B-E2E",
            IdempotencyKey = "e2e-pakai"
        });

        Assert.True(usage.IsItemResolved);

        // Pemakaian saja belum menyentuh stok; ia baru menaruh pesan di outbox.
        Assert.Equal(100m, await OnHandAsync(ctx, DrugStockStatus.Available));
        var staged = await ctx.Context.OprIntegrationDeliveries
            .SingleAsync(x => x.Destination == OperatingRoomIntegrationService.InventoryDestination);
        Assert.Equal(OprDeliveryStatus.Pending, staged.Status);

        // ------------------------------------------------------- 2. dibukukan

        var posted = await dispatch.DispatchCaseAsync(ctx.CaseId);

        Assert.Equal(1, posted.AcceptedCount);
        Assert.Equal(90m, await OnHandAsync(ctx, DrugStockStatus.Available));

        var mutation = await ctx.Context.PhmDrugStockMutations.SingleAsync();
        Assert.Equal(-10m, mutation.QuantityChange);
        Assert.Equal(world.LocationId, mutation.StorageLocationId);

        // ---------------------------------------------------------- 3. diretur

        await material.RecordAsync(ctx.CaseId, new CreateOprMaterialUsageRequest
        {
            ExternalItemId = world.DrugId,
            ItemType = OprMaterialItemType.Consumable,
            Quantity = 4m,
            UnitCode = "VIAL",
            UnitMeasurementId = VialUnitId,
            Outcome = OprMaterialOutcome.Returned,
            BatchNumber = "B-E2E",
            IdempotencyKey = "e2e-retur"
        });

        var raised = await dispatch.DispatchCaseAsync(ctx.CaseId);
        Assert.Equal(1, raised.AcceptedCount);

        // Retur belum menambah stok apa pun: ia masih menunggu apoteker.
        Assert.Equal(90m, await OnHandAsync(ctx, DrugStockStatus.Available));

        var retur = await ctx.Context.PhmDrugReturns.SingleAsync();
        Assert.Equal(DrugReturnStatus.Draft, retur.Status);

        // Returnya menunjuk catatan pemakaian yang dikembalikan — bukan yang dipakai —
        // sehingga penelusuran dari kamar operasi sampai apoteker tetap utuh.
        var returnedUsage = await ctx.Context.OprMaterialUsages
            .SingleAsync(x => x.Outcome == OprMaterialOutcome.Returned);
        Assert.Equal(returnedUsage.Id, retur.SourceOprMaterialUsageId);
        Assert.NotEqual(usage.Id, retur.SourceOprMaterialUsageId);

        // -------------------------------------------------------- 4. diperiksa

        var submitted = await returns.SubmitAsync(retur.Id, new DrugReturnCommandRequest
        {
            ExpectedVersion = retur.Version,
            IdempotencyKey = "e2e-kirim"
        });
        Assert.Equal(DrugReturnStatus.Submitted, submitted.Status);

        var item = await ctx.Context.PhmDrugReturnItems.SingleAsync();

        // Apoteker menerima 3 dari 4: satu vial tidak layak kembali.
        var verified = await returns.VerifyAsync(retur.Id, new VerifyDrugReturnRequest
        {
            VerifiedByWorkforceId = ctx.WorkforceIds[0],
            ExpectedVersion = submitted.Version,
            IdempotencyKey = "e2e-periksa",
            Note = "Satu vial segelnya rusak.",
            Items =
            [
                new VerifyDrugReturnItemInput
                {
                    DrugReturnItemId = item.Id,
                    AcceptedQuantity = 3m,
                    AcceptedStatus = DrugStockStatus.Available
                }
            ]
        });

        Assert.Equal(DrugReturnStatus.Verified, verified.Status);

        // ------------------------------------------------------ 5. stok akhir

        // 100 - 10 dipakai + 3 diterima kembali = 93. Satu vial yang ditolak tidak masuk
        // saldo mana pun: ia tidak pernah menjadi stok lagi.
        Assert.Equal(93m, await OnHandAsync(ctx, DrugStockStatus.Available));
        Assert.Equal(0m, await OnHandAsync(ctx, DrugStockStatus.Quarantine));

        // Kartu stok memuat kedua sisinya, dan keduanya menyebut batch yang sama.
        var mutations = await ctx.Context.PhmDrugStockMutations
            .OrderBy(x => x.CreateDateTime).ToListAsync();
        Assert.Equal(2, mutations.Count);
        Assert.Equal(-10m, mutations[0].QuantityChange);
        Assert.Equal(3m, mutations[1].QuantityChange);
        Assert.All(mutations, x => Assert.Equal(world.BatchId, x.DrugBatchId));
    }

    /// <summary>
    /// Barang yang dinyatakan tidak layak sama sekali tidak boleh kembali ke saldo mana pun.
    /// </summary>
    [Fact]
    public async Task Operasi_ReturDitolakSeluruhnya_TidakMenambahStok()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var world = await SetupAsync(ctx, onHand: 50m);

        var material = BuildMaterialService(ctx);
        var dispatch = BuildDispatchService(ctx);
        var returns = BuildReturnService(ctx);

        await material.RecordAsync(ctx.CaseId, UsageRequest(world.DrugId, 8m,
            OprMaterialOutcome.Used, "e2e-tolak-pakai"));
        await dispatch.DispatchCaseAsync(ctx.CaseId);

        await material.RecordAsync(ctx.CaseId, UsageRequest(world.DrugId, 2m,
            OprMaterialOutcome.Returned, "e2e-tolak-retur"));
        await dispatch.DispatchCaseAsync(ctx.CaseId);

        var retur = await ctx.Context.PhmDrugReturns.SingleAsync();
        var submitted = await returns.SubmitAsync(retur.Id, new DrugReturnCommandRequest
        {
            ExpectedVersion = retur.Version,
            IdempotencyKey = "e2e-tolak-kirim"
        });
        var item = await ctx.Context.PhmDrugReturnItems.SingleAsync();

        await returns.VerifyAsync(retur.Id, new VerifyDrugReturnRequest
        {
            VerifiedByWorkforceId = ctx.WorkforceIds[0],
            ExpectedVersion = submitted.Version,
            IdempotencyKey = "e2e-tolak-periksa",
            Note = "Kemasan terbuka di kamar operasi.",
            Items =
            [
                new VerifyDrugReturnItemInput
                {
                    DrugReturnItemId = item.Id,
                    AcceptedQuantity = 0m,
                    AcceptedStatus = DrugStockStatus.Available
                }
            ]
        });

        Assert.Equal(42m, await OnHandAsync(ctx, DrugStockStatus.Available));
        Assert.Equal(0m, await OnHandAsync(ctx, DrugStockStatus.Quarantine));
        Assert.Equal(1, await ctx.Context.PhmDrugStockMutations.CountAsync());
    }

    /// <summary>
    /// Menjalankan kembali seluruh rangkaian tidak boleh menggandakan efeknya di mana pun.
    /// </summary>
    [Fact]
    public async Task Operasi_SeluruhRangkaianDiulang_TidakMenggandakanApaPun()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var world = await SetupAsync(ctx, onHand: 30m);

        var material = BuildMaterialService(ctx);
        var dispatch = BuildDispatchService(ctx);

        var request = UsageRequest(world.DrugId, 5m, OprMaterialOutcome.Used, "e2e-ulang");

        await material.RecordAsync(ctx.CaseId, request);
        await material.RecordAsync(ctx.CaseId, request);
        await dispatch.DispatchCaseAsync(ctx.CaseId);
        await dispatch.DispatchCaseAsync(ctx.CaseId);

        Assert.Equal(1, await ctx.Context.OprMaterialUsages.CountAsync());
        Assert.Equal(1, await ctx.Context.OprIntegrationDeliveries
            .CountAsync(x => x.Destination == OperatingRoomIntegrationService.InventoryDestination));
        Assert.Equal(1, await ctx.Context.PhmDrugStockMutations.CountAsync());
        Assert.Equal(25m, await OnHandAsync(ctx, DrugStockStatus.Available));
    }

    // ------------------------------------------------------------------ penyiapan

    private static CreateOprMaterialUsageRequest UsageRequest(Guid drugId, decimal quantity,
        OprMaterialOutcome outcome, string key) => new()
    {
        ExternalItemId = drugId,
        ItemType = OprMaterialItemType.Consumable,
        Quantity = quantity,
        UnitCode = "VIAL",
        UnitMeasurementId = VialUnitId,
        Outcome = outcome,
        BatchNumber = "B-E2E",
        IdempotencyKey = key
    };

    private static Task<decimal> OnHandAsync(OperatingRoomTestContext ctx, DrugStockStatus status) =>
        ctx.Context.PhmDrugStockBalances.AsNoTracking()
            .Where(x => x.Status == status)
            .SumAsync(x => x.QuantityOnHand);

    private static OperatingRoomMaterialService BuildMaterialService(OperatingRoomTestContext ctx) =>
        new(ctx.Context, ctx.Accessor, ctx.Logger,
            new OperatingRoomIntegrationService(ctx.Context, ctx.Accessor, ctx.Logger),
            OperatingRoomTestContext.StrictRules, new DrugUnitConversionResolver(ctx.Context));

    private static OperatingRoomInventoryDispatchService BuildDispatchService(OperatingRoomTestContext ctx)
    {
        var stock = new DrugStockService(ctx.Context, ctx.Accessor, ctx.Logger);
        return new OperatingRoomInventoryDispatchService(ctx.Context, stock,
            new DrugUnitConversionResolver(ctx.Context),
            new DrugReturnService(ctx.Context, ctx.Accessor, ctx.Logger, stock),
            ctx.Accessor, ctx.Logger);
    }

    private static DrugReturnService BuildReturnService(OperatingRoomTestContext ctx) =>
        new(ctx.Context, ctx.Accessor, ctx.Logger,
            new DrugStockService(ctx.Context, ctx.Accessor, ctx.Logger));

    private sealed record World(Guid DrugId, Guid BatchId, Guid LocationId);

    /// <summary>
    /// Menyiapkan dunia minimum: satu depo yang dipetakan ke kamar operasi, satu obat dengan
    /// satuan stok, satu batch, dan saldonya.
    /// </summary>
    private static async Task<World> SetupAsync(OperatingRoomTestContext ctx, decimal onHand)
    {
        var drugId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        ctx.Context.Set<TrxPatientEncounter>().Add(new TrxPatientEncounter
        {
            Id = ctx.EncounterId, EncounterNumber = "ENC-E2E-001",
            PatientId = ctx.PatientId, ServiceUnitId = Guid.NewGuid()
        });

        ctx.Context.Set<MstMeasurement>().Add(new MstMeasurement
        {
            Id = VialUnitId, MeasurementCode = "VIAL", MeasurementName = "Vial",
            MeasurementType = "Quantity", IsForDrug = true
        });

        ctx.Context.Set<MstDrugStorageLocation>().Add(new MstDrugStorageLocation
        {
            Id = locationId, StorageLocationCode = "DEPO-RI", StorageLocationName = "Depo Rawat Inap",
            StorageLocationType = "Depot", IsActive = true, IsAllowDispensing = true,
            IsAllowReceiving = true, IsAllowTransferIn = true, IsAllowTransferOut = true
        });

        ctx.Context.Set<MstDrug>().Add(new MstDrug
        {
            Id = drugId, DrugCategoryId = Guid.NewGuid(), DrugCode = $"DRG-{drugId:N}"[..12],
            DrugName = "Cefazolin 1 g", IsActive = true, StockUnitMeasurementId = VialUnitId
        });

        ctx.Context.MstDrugBatches.Add(new MstDrugBatch
        {
            Id = batchId, DrugId = drugId, BatchNumber = "B-E2E",
            ExpiryDate = new DateOnly(2027, 8, 31)
        });

        ctx.Context.PhmDrugStockBalances.Add(new PhmDrugStockBalance
        {
            DrugId = drugId, DrugBatchId = batchId, StorageLocationId = locationId,
            Status = DrugStockStatus.Available, QuantityOnHand = onHand, QuantityReserved = 0m
        });

        ctx.Context.MstOperatingRoomStockSources.Add(new MstOperatingRoomStockSource
        {
            RoomId = ctx.RoomId, StorageLocationId = locationId, IsActive = true
        });

        await ctx.Context.SaveChangesAsync();
        ctx.Context.ChangeTracker.Clear();

        return new World(drugId, batchId, locationId);
    }
}
