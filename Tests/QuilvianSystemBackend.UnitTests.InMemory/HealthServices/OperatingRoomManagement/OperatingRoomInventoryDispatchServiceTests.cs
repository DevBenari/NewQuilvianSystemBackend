using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.OperatingRoomManagement;

/// <summary>
/// Pembukuan pemakaian material operasi ke kartu stok Farmasi.
/// </summary>
/// <remarks>
/// Yang dibuktikan di sini bukan hanya bahwa angkanya berkurang, melainkan bahwa ia berkurang
/// di depo yang benar, dari batch yang benar, sekali saja, dan tidak berkurang sama sekali
/// ketika pesannya gagal.
/// </remarks>
public class OperatingRoomInventoryDispatchServiceTests
{
    [Fact]
    public async Task Dispatch_UsedMaterial_DecrementsMappedDepotAndWritesLedger()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var fixture = await StockFixture.CreateAsync(ctx, onHand: 100m);
        await RecordUsageAsync(ctx, fixture.DrugId, quantity: 12m);

        var result = await Dispatch(ctx).DispatchCaseAsync(ctx.CaseId);

        Assert.Equal(1, result.AcceptedCount);
        Assert.Equal(0, result.FailedCount);

        var balance = await ctx.Context.PhmDrugStockBalances.SingleAsync();
        Assert.Equal(88m, balance.QuantityOnHand);

        var mutation = await ctx.Context.PhmDrugStockMutations.SingleAsync();
        Assert.Equal(-12m, mutation.QuantityChange);
        Assert.Equal(100m, mutation.BalanceBefore);
        Assert.Equal(88m, mutation.BalanceAfter);
    }

    /// <summary>
    /// Pemanggilan kedua tidak boleh memotong stok lagi. Inilah yang membuat tombol
    /// "kirim ulang" aman ditekan operator yang ragu apakah pengirimannya berhasil.
    /// </summary>
    [Fact]
    public async Task Dispatch_CalledTwice_DecrementsStockOnlyOnce()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var fixture = await StockFixture.CreateAsync(ctx, onHand: 50m);
        await RecordUsageAsync(ctx, fixture.DrugId, quantity: 5m);

        var service = Dispatch(ctx);
        await service.DispatchCaseAsync(ctx.CaseId);
        var second = await service.DispatchCaseAsync(ctx.CaseId);

        Assert.Equal(0, second.ProcessedCount);
        Assert.Equal(45m, (await ctx.Context.PhmDrugStockBalances.SingleAsync()).QuantityOnHand);
        Assert.Equal(1, await ctx.Context.PhmDrugStockMutations.CountAsync());
    }

    [Fact]
    public async Task Dispatch_WithoutRoomMapping_FailsWithoutTouchingStock()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var fixture = await StockFixture.CreateAsync(ctx, onHand: 30m, mapRoom: false);
        await RecordUsageAsync(ctx, fixture.DrugId, quantity: 4m);

        var result = await Dispatch(ctx).DispatchCaseAsync(ctx.CaseId);

        Assert.Equal(1, result.FailedCount);
        Assert.Equal("OPR-INV-004", result.Results[0].ErrorCode);
        Assert.Equal(30m, (await ctx.Context.PhmDrugStockBalances.SingleAsync()).QuantityOnHand);
        Assert.False(await ctx.Context.PhmDrugStockMutations.AnyAsync());

        var delivery = await ctx.Context.OprIntegrationDeliveries.SingleAsync();
        Assert.Equal(OprDeliveryStatus.Failed, delivery.Status);
        Assert.Equal("OPR-INV-004", delivery.LastErrorCode);
    }

    [Fact]
    public async Task Dispatch_InsufficientStock_FailsAndLeavesBalanceUntouched()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var fixture = await StockFixture.CreateAsync(ctx, onHand: 3m);
        await RecordUsageAsync(ctx, fixture.DrugId, quantity: 10m);

        var result = await Dispatch(ctx).DispatchCaseAsync(ctx.CaseId);

        Assert.Equal(1, result.FailedCount);
        Assert.Equal(3m, (await ctx.Context.PhmDrugStockBalances.SingleAsync()).QuantityOnHand);
        Assert.False(await ctx.Context.PhmDrugStockMutations.AnyAsync());
    }

    /// <summary>
    /// FEFO harus tetap berlaku ketika petugas tidak mencatat nomor batch: batch yang lebih
    /// dahulu kedaluwarsa keluar lebih dahulu.
    /// </summary>
    [Fact]
    public async Task Dispatch_WithoutBatchNumber_ConsumesEarliestExpiryFirst()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var fixture = await StockFixture.CreateAsync(ctx, onHand: 10m,
            batchNumber: "B-LAMBAT", expiry: new DateOnly(2027, 12, 31));
        var soonId = await fixture.AddBatchAsync(ctx, "B-CEPAT", new DateOnly(2026, 11, 30), onHand: 4m);
        await RecordUsageAsync(ctx, fixture.DrugId, quantity: 6m);

        await Dispatch(ctx).DispatchCaseAsync(ctx.CaseId);

        var soon = await ctx.Context.PhmDrugStockBalances.SingleAsync(x => x.DrugBatchId == soonId);
        var later = await ctx.Context.PhmDrugStockBalances.SingleAsync(x => x.DrugBatchId == fixture.BatchId);
        Assert.Equal(0m, soon.QuantityOnHand);
        Assert.Equal(8m, later.QuantityOnHand);
    }

    /// <summary>
    /// Nomor batch yang dicatat di kamar operasi menyebut barang yang benar-benar dipakai
    /// pada pasien itu, sehingga ia mengalahkan FEFO.
    /// </summary>
    [Fact]
    public async Task Dispatch_WithBatchNumber_ConsumesThatBatchEvenWhenAnotherExpiresSooner()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var fixture = await StockFixture.CreateAsync(ctx, onHand: 10m,
            batchNumber: "B-DIPAKAI", expiry: new DateOnly(2027, 12, 31));
        var soonId = await fixture.AddBatchAsync(ctx, "B-CEPAT", new DateOnly(2026, 11, 30), onHand: 9m);
        await RecordUsageAsync(ctx, fixture.DrugId, quantity: 2m, batchNumber: "B-DIPAKAI");

        await Dispatch(ctx).DispatchCaseAsync(ctx.CaseId);

        Assert.Equal(9m, (await ctx.Context.PhmDrugStockBalances
            .SingleAsync(x => x.DrugBatchId == soonId)).QuantityOnHand);
        Assert.Equal(8m, (await ctx.Context.PhmDrugStockBalances
            .SingleAsync(x => x.DrugBatchId == fixture.BatchId)).QuantityOnHand);
    }

    /// <summary>
    /// Barang yang kembali dari kamar operasi tidak menambah stok sama sekali di tahap ini.
    /// Ia menjadi draft Retur Obat, dan apoteker yang memutuskan apakah ia layak kembali.
    /// </summary>
    [Fact]
    public async Task Dispatch_ReturnedMaterial_RaisesPharmacyReturnWithoutChangingStock()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var fixture = await StockFixture.CreateAsync(ctx, onHand: 20m, batchNumber: "B-RETUR");
        await RecordUsageAsync(ctx, fixture.DrugId, quantity: 3m, batchNumber: "B-RETUR",
            outcome: OprMaterialOutcome.Returned);

        var result = await Dispatch(ctx).DispatchCaseAsync(ctx.CaseId);

        Assert.Equal(1, result.AcceptedCount);
        Assert.StartsWith("DrugReturn/", result.Results[0].AcceptedReference);

        // Stok belum berubah: tidak di Available, dan tidak pula di Karantina.
        var balance = await ctx.Context.PhmDrugStockBalances.SingleAsync();
        Assert.Equal(DrugStockStatus.Available, balance.Status);
        Assert.Equal(20m, balance.QuantityOnHand);

        var retur = await ctx.Context.PhmDrugReturns.SingleAsync();
        Assert.Equal(DrugReturnStatus.Draft, retur.Status);
        Assert.Equal(fixture.LocationId, retur.StorageLocationId);
        var item = await ctx.Context.PhmDrugReturnItems.SingleAsync();
        Assert.Equal(3m, item.Quantity);
        Assert.Equal(fixture.BatchId, item.DrugBatchId);
    }

    /// <summary>
    /// Pembukuan yang diulang tidak boleh menghasilkan dua dokumen retur untuk barang yang sama.
    /// </summary>
    [Fact]
    public async Task Dispatch_ReturnedMaterialTwice_RaisesOnlyOneReturn()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var fixture = await StockFixture.CreateAsync(ctx, onHand: 20m, batchNumber: "B-RETUR");
        await RecordUsageAsync(ctx, fixture.DrugId, quantity: 3m, batchNumber: "B-RETUR",
            outcome: OprMaterialOutcome.Returned);

        var service = Dispatch(ctx);
        await service.DispatchCaseAsync(ctx.CaseId);
        await service.DispatchCaseAsync(ctx.CaseId);

        Assert.Equal(1, await ctx.Context.PhmDrugReturns.CountAsync());
    }

    /// <summary>
    /// Koreksi membukukan selisihnya saja. Membukukan jumlah penuh akan memotong barang yang
    /// sama dua kali.
    /// </summary>
    [Fact]
    public async Task Dispatch_Correction_PostsOnlyTheDifference()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var fixture = await StockFixture.CreateAsync(ctx, onHand: 100m);
        var originalId = await RecordUsageAsync(ctx, fixture.DrugId, quantity: 10m);

        var service = Dispatch(ctx);
        await service.DispatchCaseAsync(ctx.CaseId);

        await RecordUsageAsync(ctx, fixture.DrugId, quantity: 14m,
            outcome: OprMaterialOutcome.Corrected, correctionOfUsageId: originalId, revision: 2);
        await service.DispatchCaseAsync(ctx.CaseId);

        // 100 - 10 - (14 - 10) = 86, bukan 100 - 10 - 14.
        Assert.Equal(86m, (await ctx.Context.PhmDrugStockBalances.SingleAsync()).QuantityOnHand);
    }

    /// <summary>
    /// Koreksi yang mengurangi jumlah terpakai berarti barang kembali ke depo, bukan diambil
    /// lagi — arahnya harus berbalik.
    /// </summary>
    [Fact]
    public async Task Dispatch_CorrectionReducingQuantity_ReturnsTheDifference()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var fixture = await StockFixture.CreateAsync(ctx, onHand: 100m, batchNumber: "B-KOREKSI");
        var originalId = await RecordUsageAsync(ctx, fixture.DrugId, quantity: 10m,
            batchNumber: "B-KOREKSI");

        var service = Dispatch(ctx);
        await service.DispatchCaseAsync(ctx.CaseId);

        await RecordUsageAsync(ctx, fixture.DrugId, quantity: 6m, batchNumber: "B-KOREKSI",
            outcome: OprMaterialOutcome.Corrected, correctionOfUsageId: originalId, revision: 2);
        await service.DispatchCaseAsync(ctx.CaseId);

        // Yang terpakai tetap 10 dikurangi; selisih 4 yang kembali menjadi dokumen retur,
        // bukan penambahan stok langsung.
        var balance = await ctx.Context.PhmDrugStockBalances.SingleAsync();
        Assert.Equal(90m, balance.QuantityOnHand);

        var item = await ctx.Context.PhmDrugReturnItems.SingleAsync();
        Assert.Equal(4m, item.Quantity);
    }

    /// <summary>
    /// Satu pesan yang gagal tidak boleh menghentikan pesan lain; kegagalan yang memblokir
    /// antrean hanya akan menyembunyikan sisanya.
    /// </summary>
    [Fact]
    public async Task Dispatch_OneFailingMessage_DoesNotBlockTheOthers()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var fixture = await StockFixture.CreateAsync(ctx, onHand: 40m);
        await RecordUsageAsync(ctx, fixture.DrugId, quantity: 5m);
        await RecordUsageAsync(ctx, fixture.DrugId, quantity: 999m);
        await RecordUsageAsync(ctx, fixture.DrugId, quantity: 7m);

        var result = await Dispatch(ctx).DispatchCaseAsync(ctx.CaseId);

        Assert.Equal(3, result.ProcessedCount);
        Assert.Equal(2, result.AcceptedCount);
        Assert.Equal(1, result.FailedCount);
        Assert.Equal(28m, (await ctx.Context.PhmDrugStockBalances.SingleAsync()).QuantityOnHand);
    }

    /// <summary>
    /// Satu box bukan satu vial. Tanpa konversi, satu box akan mengurangi stok sepuluh kali
    /// lebih sedikit daripada yang sebenarnya terpakai.
    /// </summary>
    [Fact]
    public async Task Dispatch_UnitDifferentFromStockUnit_ConvertsBeforePosting()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var fixture = await StockFixture.CreateAsync(ctx, onHand: 100m, withBoxConversion: true);
        await RecordUsageAsync(ctx, fixture.DrugId, quantity: 2m,
            unitMeasurementId: StockFixture.BoxUnitId);

        var result = await Dispatch(ctx).DispatchCaseAsync(ctx.CaseId);

        Assert.Equal(1, result.AcceptedCount);
        // 2 box = 20 vial, bukan 2.
        Assert.Equal(80m, (await ctx.Context.PhmDrugStockBalances.SingleAsync()).QuantityOnHand);
    }

    /// <summary>
    /// Satuan yang tidak dikenal Farmasi tidak boleh diperlakukan sebagai satuan stok.
    /// Menebaknya salah lebih merugikan daripada menolak pesannya.
    /// </summary>
    [Fact]
    public async Task Dispatch_UnknownUnit_FailsWithoutTouchingStock()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var fixture = await StockFixture.CreateAsync(ctx, onHand: 100m);
        await RecordUsageAsync(ctx, fixture.DrugId, quantity: 2m,
            unitMeasurementId: StockFixture.BoxUnitId);

        var result = await Dispatch(ctx).DispatchCaseAsync(ctx.CaseId);

        Assert.Equal(1, result.FailedCount);
        Assert.Equal("PHM092", result.Results[0].ErrorCode);
        Assert.Equal(100m, (await ctx.Context.PhmDrugStockBalances.SingleAsync()).QuantityOnHand);
    }

    /// <summary>
    /// Baris lama yang tercatat sebelum satuan diwajibkan tidak boleh diam-diam dibukukan
    /// dengan satuan yang ditebak.
    /// </summary>
    [Fact]
    public async Task Dispatch_UsageWithoutUnit_FailsWithoutTouchingStock()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var fixture = await StockFixture.CreateAsync(ctx, onHand: 100m);
        await RecordUsageAsync(ctx, fixture.DrugId, quantity: 2m, unitMeasurementId: Guid.Empty);

        var result = await Dispatch(ctx).DispatchCaseAsync(ctx.CaseId);

        Assert.Equal(1, result.FailedCount);
        Assert.Equal("PHM090", result.Results[0].ErrorCode);
        Assert.Equal(100m, (await ctx.Context.PhmDrugStockBalances.SingleAsync()).QuantityOnHand);
    }

    /// <summary>
    /// Kamar operasi tidak boleh mengambil dari lokasi karantina: isinya justru barang yang
    /// sedang ditahan dari pelayanan.
    /// </summary>
    [Fact]
    public async Task SaveStockSource_QuarantineLocation_IsRejected()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var locationId = Guid.NewGuid();
        ctx.Context.Set<MstDrugStorageLocation>().Add(new MstDrugStorageLocation
        {
            // Sengaja: benderanya TIDAK menyala, hanya jenisnya yang menyebut karantina.
            // Keadaan ini benar-benar ada pada data, dan pemeriksaan yang hanya membaca
            // benderanya akan meloloskan lokasi ini sebagai sumber stok kamar operasi.
            Id = locationId, StorageLocationCode = "KAR-01", StorageLocationName = "Karantina",
            StorageLocationType = "Quarantine", IsActive = true, IsAllowDispensing = true,
            IsQuarantineLocation = false
        });
        await ctx.Context.SaveChangesAsync();

        var service = new OperatingRoomStockSourceService(ctx.Context, ctx.Accessor, ctx.Logger);

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            service.SaveAsync(new SaveOprStockSourceRequest
            {
                RoomId = ctx.RoomId, StorageLocationId = locationId
            }));

        Assert.Equal("OPR-INV-012", exception.Code);
    }

    /// <summary>
    /// Satu kamar hanya boleh punya satu depo aktif; menetapkan yang baru menonaktifkan yang
    /// lama, bukan menambah pesaing.
    /// </summary>
    [Fact]
    public async Task SaveStockSource_Remapping_DeactivatesThePreviousDepot()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var first = await AddDepotAsync(ctx, "DEPO-RI", "Depo Rawat Inap");
        var second = await AddDepotAsync(ctx, "DEPO-OK", "Depo OK");

        var service = new OperatingRoomStockSourceService(ctx.Context, ctx.Accessor, ctx.Logger);
        await service.SaveAsync(new SaveOprStockSourceRequest
        {
            RoomId = ctx.RoomId, StorageLocationId = first
        });
        await service.SaveAsync(new SaveOprStockSourceRequest
        {
            RoomId = ctx.RoomId, StorageLocationId = second
        });

        var active = await ctx.Context.MstOperatingRoomStockSources
            .Where(x => x.RoomId == ctx.RoomId && x.IsActive && !x.IsDelete)
            .ToListAsync();

        Assert.Single(active);
        Assert.Equal(second, active[0].StorageLocationId);
        Assert.Equal(2, await ctx.Context.MstOperatingRoomStockSources.CountAsync());
    }

    // ------------------------------------------------------------------ penyiapan

    private static OperatingRoomInventoryDispatchService Dispatch(OperatingRoomTestContext ctx)
    {
        var stock = new DrugStockService(ctx.Context, ctx.Accessor, ctx.Logger);
        return new OperatingRoomInventoryDispatchService(ctx.Context, stock,
            new DrugUnitConversionResolver(ctx.Context),
            new DrugReturnService(ctx.Context, ctx.Accessor, ctx.Logger, stock),
            ctx.Accessor, ctx.Logger);
    }

    private static async Task<Guid> AddDepotAsync(OperatingRoomTestContext ctx, string code, string name)
    {
        var id = Guid.NewGuid();
        ctx.Context.Set<MstDrugStorageLocation>().Add(new MstDrugStorageLocation
        {
            Id = id, StorageLocationCode = code, StorageLocationName = name,
            StorageLocationType = "Depot", IsActive = true, IsAllowDispensing = true,
            IsAllowReceiving = true, IsAllowTransferIn = true, IsAllowTransferOut = true
        });
        await ctx.Context.SaveChangesAsync();
        return id;
    }

    /// <summary>
    /// Mencatat pemakaian beserta pesan outbox-nya secara langsung.
    /// </summary>
    /// <remarks>
    /// Sengaja tidak lewat <c>OperatingRoomMaterialService</c>: yang diuji berkas ini adalah
    /// consumer-nya, dan menyiapkan seluruh prasyarat kewenangan tim hanya akan mengaburkan
    /// apa yang sedang dibuktikan.
    /// </remarks>
    private static async Task<Guid> RecordUsageAsync(OperatingRoomTestContext ctx, Guid drugId,
        decimal quantity, string? batchNumber = null,
        OprMaterialOutcome outcome = OprMaterialOutcome.Used,
        Guid? correctionOfUsageId = null, int revision = 1, Guid? unitMeasurementId = null)
    {
        var usage = new OprMaterialUsage
        {
            OprCaseId = ctx.CaseId, ExternalItemId = drugId, ItemType = OprMaterialItemType.Consumable,
            Quantity = quantity, UnitCode = "TAB", Outcome = outcome, BatchNumber = batchNumber,
            UnitMeasurementId = unitMeasurementId ?? StockFixture.StockUnitId,
            OccurredAt = DateTime.UtcNow, RecordedBy = ctx.SurgeonUserId, Revision = revision,
            CorrectionOfUsageId = correctionOfUsageId,
            CorrectionReason = correctionOfUsageId.HasValue ? "Salah hitung saat operasi." : null,
            CreateDateTime = DateTime.UtcNow, CreateBy = ctx.SurgeonUserId
        };
        ctx.Context.OprMaterialUsages.Add(usage);
        ctx.Context.OprIntegrationDeliveries.Add(new OprIntegrationDelivery
        {
            OprCaseId = ctx.CaseId,
            Destination = OperatingRoomIntegrationService.InventoryDestination,
            MessageType = OperatingRoomIntegrationService.MaterialMessageType,
            IdempotencyKey = $"{ctx.CaseId:N}:usage:{usage.Id:N}:{revision}",
            CorrelationId = ctx.CaseId.ToString("N"),
            PayloadReference = $"OprMaterialUsage/{usage.Id:N}",
            Status = OprDeliveryStatus.Pending,
            CreateDateTime = DateTime.UtcNow, CreateBy = ctx.SurgeonUserId
        });
        await ctx.Context.SaveChangesAsync();
        return usage.Id;
    }

    private sealed class StockFixture
    {
        /// <summary>Satuan stok obat uji; dipakai juga sebagai satuan pemakaian bawaan.</summary>
        public static Guid StockUnitId { get; } = Guid.Parse("11111111-1111-1111-1111-111111111111");

        /// <summary>Satuan yang lebih besar, dipakai membuktikan konversi.</summary>
        public static Guid BoxUnitId { get; } = Guid.Parse("22222222-2222-2222-2222-222222222222");

        public required Guid DrugId { get; init; }
        public required Guid BatchId { get; init; }
        public required Guid LocationId { get; init; }

        public static async Task<StockFixture> CreateAsync(OperatingRoomTestContext ctx,
            decimal onHand, bool mapRoom = true, string batchNumber = "B-UTAMA",
            DateOnly? expiry = null, bool withBoxConversion = false)
        {
            var drugId = Guid.NewGuid();
            var batchId = Guid.NewGuid();
            var locationId = await AddDepotAsync(ctx, "DEPO-RI", "Depo Rawat Inap");

            // Retur Farmasi menuntut kunjungan pasien yang sah; kasus operasi menunjuk salah
            // satunya, jadi kunjungan itu harus ada agar returnya dapat diajukan.
            if (!await ctx.Context.Set<TrxPatientEncounter>().AnyAsync(x => x.Id == ctx.EncounterId))
            {
                ctx.Context.Set<TrxPatientEncounter>().Add(new TrxPatientEncounter
                {
                    Id = ctx.EncounterId, EncounterNumber = "ENC-OPR-001",
                    PatientId = ctx.PatientId, ServiceUnitId = Guid.NewGuid()
                });
            }

            ctx.Context.Set<MstMeasurement>().Add(new MstMeasurement
            {
                Id = StockUnitId, MeasurementCode = "VIAL", MeasurementName = "Vial",
                MeasurementType = "Quantity", IsForDrug = true
            });
            ctx.Context.Set<MstMeasurement>().Add(new MstMeasurement
            {
                Id = BoxUnitId, MeasurementCode = "BOX", MeasurementName = "Box",
                MeasurementType = "Quantity", IsForDrug = true
            });

            ctx.Context.Set<MstDrug>().Add(new MstDrug
            {
                Id = drugId, DrugCategoryId = Guid.NewGuid(), DrugCode = $"DRG-{drugId:N}"[..12],
                DrugName = "Obat Operasi", IsActive = true, StockUnitMeasurementId = StockUnitId
            });

            if (withBoxConversion)
            {
                ctx.Context.MstDrugUnitConversions.Add(new MstDrugUnitConversion
                {
                    DrugId = drugId, ConversionCode = "BOX-VIAL", ConversionName = "Box ke Vial",
                    FromMeasurementId = BoxUnitId, ToMeasurementId = StockUnitId,
                    FromQuantity = 1m, ToQuantity = 10m, ConversionType = "Stock",
                    IsDefault = true, IsForStock = true, IsActive = true
                });
            }
            ctx.Context.MstDrugBatches.Add(new MstDrugBatch
            {
                Id = batchId, DrugId = drugId, BatchNumber = batchNumber,
                ExpiryDate = expiry ?? new DateOnly(2027, 6, 30)
            });
            ctx.Context.PhmDrugStockBalances.Add(new PhmDrugStockBalance
            {
                DrugId = drugId, DrugBatchId = batchId, StorageLocationId = locationId,
                Status = DrugStockStatus.Available, QuantityOnHand = onHand, QuantityReserved = 0m
            });

            if (mapRoom)
            {
                ctx.Context.MstOperatingRoomStockSources.Add(new MstOperatingRoomStockSource
                {
                    RoomId = ctx.RoomId, StorageLocationId = locationId, IsActive = true
                });
            }

            await ctx.Context.SaveChangesAsync();
            return new StockFixture { DrugId = drugId, BatchId = batchId, LocationId = locationId };
        }

        public async Task<Guid> AddBatchAsync(OperatingRoomTestContext ctx, string batchNumber,
            DateOnly expiry, decimal onHand)
        {
            var batchId = Guid.NewGuid();
            ctx.Context.MstDrugBatches.Add(new MstDrugBatch
            {
                Id = batchId, DrugId = DrugId, BatchNumber = batchNumber, ExpiryDate = expiry
            });
            ctx.Context.PhmDrugStockBalances.Add(new PhmDrugStockBalance
            {
                DrugId = DrugId, DrugBatchId = batchId, StorageLocationId = LocationId,
                Status = DrugStockStatus.Available, QuantityOnHand = onHand, QuantityReserved = 0m
            });
            await ctx.Context.SaveChangesAsync();
            return batchId;
        }
    }
}
