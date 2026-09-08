using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.HealthServices.NutritionManagement;

namespace QuilvianSystemBackend.Tests.HealthServices.PharmacyManagement;

/// <summary>
/// Pengujian permintaan stok barang dan obat.
/// </summary>
/// <remarks>
/// Yang diuji bukan sekadar setiap perintah berhasil, melainkan tiga janji yang menentukan:
/// permintaan yang sudah dikirim tidak dapat diubah, satu obat tidak dapat muncul dua kali,
/// dan nama obat pada riwayat tidak ikut berubah ketika master disunting.
/// </remarks>
public sealed class StockRequestServiceTests
{
    private sealed class Fixture : IAsyncDisposable
    {
        public required ApplicationDbContext Context { get; init; }
        public required StockRequestService Service { get; init; }
        public required Guid ServiceUnitId { get; init; }
        public required Guid StorageLocationId { get; init; }
        public required Guid WorkforceId { get; init; }
        public required Guid DrugAId { get; init; }
        public required Guid DrugBId { get; init; }
        public required Guid MeasurementId { get; init; }

        public async ValueTask DisposeAsync() => await Context.DisposeAsync();
    }

    private static async Task<Fixture> CreateAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"stock-request-{Guid.NewGuid():N}").Options;
        var context = new ApplicationDbContext(options);

        var serviceUnitId = Guid.NewGuid();
        var storageLocationId = Guid.NewGuid();
        var workforceId = Guid.NewGuid();
        var drugAId = Guid.NewGuid();
        var drugBId = Guid.NewGuid();
        var measurementId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        context.Set<MstServiceUnit>().Add(new MstServiceUnit
        {
            Id = serviceUnitId, ServiceUnitCode = "SU1",
            ServiceUnitName = "Depo Farmasi Rawat Inap", IsActive = true
        });

        context.Set<MstDrugStorageLocation>().Add(new MstDrugStorageLocation
        {
            Id = storageLocationId, StorageLocationCode = "GD1",
            StorageLocationName = "Gudang Farmasi Utama", IsActive = true
        });

        context.Set<MstWorkforceProfile>().Add(new MstWorkforceProfile
        {
            Id = workforceId, ProfileCode = "WF1", DisplayName = "Petugas Uji", IsActive = true
        });

        context.Set<MstMeasurement>().Add(new MstMeasurement
        {
            Id = measurementId, MeasurementCode = "TAB", MeasurementName = "Tablet",
            MeasurementType = "Unit", IsActive = true
        });

        context.Set<MstDrugCategory>().Add(new MstDrugCategory
        {
            Id = categoryId, DrugCategoryCode = "K1", DrugCategoryName = "Umum",
            DrugCategoryType = "General", IsActive = true
        });

        context.Set<MstDrug>().AddRange(
            new MstDrug
            {
                Id = drugAId, DrugCategoryId = categoryId, DrugCode = "OBT-001",
                DrugName = "Paracetamol 500 mg", IsActive = true
            },
            new MstDrug
            {
                Id = drugBId, DrugCategoryId = categoryId, DrugCode = "OBT-002",
                DrugName = "Amoksisilin 500 mg", IsActive = true
            });

        await context.SaveChangesAsync();

        var accessor = new MutableHttpContextAccessor();
        accessor.SetUser(userId);

        return new Fixture
        {
            Context = context,
            Service = new StockRequestService(context, accessor,
                new LoggerService(NullLogger<LoggerService>.Instance, accessor)),
            ServiceUnitId = serviceUnitId,
            StorageLocationId = storageLocationId,
            WorkforceId = workforceId,
            DrugAId = drugAId,
            DrugBId = drugBId,
            MeasurementId = measurementId
        };
    }

    private static CreateStockRequestRequest CreateRequest(Fixture f, string key,
        params (Guid DrugId, decimal Qty)[] items) => new()
        {
            RequestingServiceUnitId = f.ServiceUnitId,
            StorageLocationId = f.StorageLocationId,
            RequestedByWorkforceId = f.WorkforceId,
            Priority = StockRequestPriority.Routine,
            Items = [.. items.Select(x => new StockRequestItemInput
            {
                DrugId = x.DrugId, MeasurementId = f.MeasurementId, RequestedQuantity = x.Qty
            })],
            IdempotencyKey = key
        };

    // ------------------------------------------------------------ buat permintaan

    [Fact]
    public async Task Buat_MenghasilkanDraftBesertaItemDanNomor()
    {
        await using var f = await CreateAsync();

        var result = await f.Service.CreateAsync(
            CreateRequest(f, "k1", (f.DrugAId, 10), (f.DrugBId, 5)));

        Assert.Equal(StockRequestStatus.Draft, result.Status);
        Assert.True(result.IsEditable);
        Assert.Equal(2, result.ItemCount);
        Assert.Equal(2, result.Items.Count);
        Assert.StartsWith("REQ-", result.RequestNumber);
        Assert.Equal("Paracetamol 500 mg", result.Items[0].DrugName);
    }

    [Fact]
    public async Task Buat_ObatKembarDalamSatuPermintaan_Ditolak()
    {
        await using var f = await CreateAsync();

        var exception = await Assert.ThrowsAsync<StockRequestUnprocessableException>(
            () => f.Service.CreateAsync(CreateRequest(f, "k1", (f.DrugAId, 10), (f.DrugAId, 5))));

        Assert.Equal("PHM005", exception.Code);
    }

    [Fact]
    public async Task Buat_JumlahNolAtauKurang_Ditolak()
    {
        await using var f = await CreateAsync();

        var exception = await Assert.ThrowsAsync<StockRequestUnprocessableException>(
            () => f.Service.CreateAsync(CreateRequest(f, "k1", (f.DrugAId, 0))));

        Assert.Equal("PHM003", exception.Code);
    }

    [Fact]
    public async Task Buat_ObatTidakDikenal_Ditolak()
    {
        await using var f = await CreateAsync();

        var exception = await Assert.ThrowsAsync<StockRequestUnprocessableException>(
            () => f.Service.CreateAsync(CreateRequest(f, "k1", (Guid.NewGuid(), 10))));

        Assert.Equal("PHM001", exception.Code);
    }

    [Fact]
    public async Task Buat_KunciSamaDikirimDuaKali_TidakMenggandakan()
    {
        await using var f = await CreateAsync();

        var first = await f.Service.CreateAsync(CreateRequest(f, "k1", (f.DrugAId, 10)));
        var second = await f.Service.CreateAsync(CreateRequest(f, "k1", (f.DrugAId, 10)));

        Assert.Equal(first.Id, second.Id);
        Assert.Single(f.Context.PhmStockRequests);
    }

    [Fact]
    public async Task Buat_KunciSamaIsiBerbeda_Ditolak()
    {
        await using var f = await CreateAsync();
        await f.Service.CreateAsync(CreateRequest(f, "k1", (f.DrugAId, 10)));

        var exception = await Assert.ThrowsAsync<StockRequestConflictException>(
            () => f.Service.CreateAsync(CreateRequest(f, "k1", (f.DrugAId, 99))));

        Assert.Equal("PHM013", exception.Code);
    }

    // ------------------------------------------------------------ edit permintaan

    [Fact]
    public async Task Edit_PadaDraft_MenggantiSeluruhItem()
    {
        await using var f = await CreateAsync();
        var created = await f.Service.CreateAsync(CreateRequest(f, "k1", (f.DrugAId, 10)));

        var updated = await f.Service.UpdateAsync(created.Id, new UpdateStockRequestRequest
        {
            StorageLocationId = f.StorageLocationId,
            Priority = StockRequestPriority.Urgent,
            Items = [new StockRequestItemInput
            {
                DrugId = f.DrugBId, MeasurementId = f.MeasurementId, RequestedQuantity = 7
            }],
            ExpectedVersion = created.Version,
            IdempotencyKey = "u1"
        });

        var item = Assert.Single(updated.Items);
        Assert.Equal("Amoksisilin 500 mg", item.DrugName);
        Assert.Equal(7, item.RequestedQuantity);
        Assert.Equal(StockRequestPriority.Urgent, updated.Priority);
        Assert.Equal(1, updated.ItemCount);
    }

    [Fact]
    public async Task Edit_SetelahDikirim_Ditolak()
    {
        await using var f = await CreateAsync();
        var created = await f.Service.CreateAsync(CreateRequest(f, "k1", (f.DrugAId, 10)));
        var submitted = await f.Service.SubmitAsync(created.Id, new SubmitStockRequestRequest
        {
            ExpectedVersion = created.Version, IdempotencyKey = "s1"
        });

        // Inilah janji terpenting: gudang mungkin sudah menyiapkan barangnya.
        var exception = await Assert.ThrowsAsync<StockRequestConflictException>(
            () => f.Service.UpdateAsync(created.Id, new UpdateStockRequestRequest
            {
                StorageLocationId = f.StorageLocationId,
                Priority = StockRequestPriority.Routine,
                Items = [new StockRequestItemInput
                {
                    DrugId = f.DrugAId, MeasurementId = f.MeasurementId, RequestedQuantity = 99
                }],
                ExpectedVersion = submitted.Version,
                IdempotencyKey = "u1"
            }));

        Assert.Equal("PHM004", exception.Code);
    }

    [Fact]
    public async Task Edit_VersiTidakCocok_Ditolak()
    {
        await using var f = await CreateAsync();
        var created = await f.Service.CreateAsync(CreateRequest(f, "k1", (f.DrugAId, 10)));

        var exception = await Assert.ThrowsAsync<StockRequestConflictException>(
            () => f.Service.UpdateAsync(created.Id, new UpdateStockRequestRequest
            {
                StorageLocationId = f.StorageLocationId,
                Priority = StockRequestPriority.Routine,
                Items = [new StockRequestItemInput
                {
                    DrugId = f.DrugAId, MeasurementId = f.MeasurementId, RequestedQuantity = 3
                }],
                ExpectedVersion = created.Version + 5,
                IdempotencyKey = "u1"
            }));

        Assert.Equal("PHM012", exception.Code);
    }

    // ------------------------------------------------------------- kirim dan batal

    [Fact]
    public async Task Kirim_MenguncePermintaanDanMencatatWaktunya()
    {
        await using var f = await CreateAsync();
        var created = await f.Service.CreateAsync(CreateRequest(f, "k1", (f.DrugAId, 10)));

        var submitted = await f.Service.SubmitAsync(created.Id, new SubmitStockRequestRequest
        {
            ExpectedVersion = created.Version, IdempotencyKey = "s1"
        });

        Assert.Equal(StockRequestStatus.Submitted, submitted.Status);
        Assert.False(submitted.IsEditable);
        Assert.NotNull(submitted.SubmittedAt);
    }

    [Fact]
    public async Task Batal_TanpaAlasan_Ditolak()
    {
        await using var f = await CreateAsync();
        var created = await f.Service.CreateAsync(CreateRequest(f, "k1", (f.DrugAId, 10)));

        var exception = await Assert.ThrowsAsync<StockRequestUnprocessableException>(
            () => f.Service.CancelAsync(created.Id, new CancelStockRequestRequest
            {
                Reason = "  ", ExpectedVersion = created.Version, IdempotencyKey = "c1"
            }));

        Assert.Equal("PHM009", exception.Code);
    }

    [Fact]
    public async Task Batal_SetelahDibatalkan_TidakDapatDiubahLagi()
    {
        await using var f = await CreateAsync();
        var created = await f.Service.CreateAsync(CreateRequest(f, "k1", (f.DrugAId, 10)));
        var cancelled = await f.Service.CancelAsync(created.Id, new CancelStockRequestRequest
        {
            Reason = "Salah unit", ExpectedVersion = created.Version, IdempotencyKey = "c1"
        });

        Assert.Equal(StockRequestStatus.Cancelled, cancelled.Status);

        var exception = await Assert.ThrowsAsync<StockRequestConflictException>(
            () => f.Service.SubmitAsync(created.Id, new SubmitStockRequestRequest
            {
                ExpectedVersion = cancelled.Version, IdempotencyKey = "s1"
            }));

        Assert.Equal("PHM004", exception.Code);
    }

    // --------------------------------------------------------- riwayat dan snapshot

    [Fact]
    public async Task Riwayat_DapatDicariLewatNamaObatDiDalamnya()
    {
        await using var f = await CreateAsync();
        await f.Service.CreateAsync(CreateRequest(f, "k1", (f.DrugAId, 10)));
        await f.Service.CreateAsync(CreateRequest(f, "k2", (f.DrugBId, 5)));

        var result = await f.Service.GetPagedAsync(new StockRequestPagedQuery
        {
            Search = "amoksisilin"
        });

        var found = Assert.Single(result.Items);
        Assert.Equal(1, found.ItemCount);
    }

    [Fact]
    public async Task Riwayat_DapatDisaringMenurutObat()
    {
        await using var f = await CreateAsync();
        await f.Service.CreateAsync(CreateRequest(f, "k1", (f.DrugAId, 10)));
        await f.Service.CreateAsync(CreateRequest(f, "k2", (f.DrugBId, 5)));

        var result = await f.Service.GetPagedAsync(new StockRequestPagedQuery
        {
            DrugId = f.DrugAId
        });

        Assert.Single(result.Items);
    }

    [Fact]
    public async Task Riwayat_NamaObatTidakIkutBerubahKetikaMasterDisunting()
    {
        await using var f = await CreateAsync();
        var created = await f.Service.CreateAsync(CreateRequest(f, "k1", (f.DrugAId, 10)));

        var drug = await f.Context.MstDrugs.FirstAsync(x => x.Id == f.DrugAId);
        drug.DrugName = "Paracetamol 500 mg (nama baru)";
        await f.Context.SaveChangesAsync();

        var detail = await f.Service.GetDetailAsync(created.Id);

        // Riwayat permintaan harus menunjukkan apa yang tertulis saat itu.
        Assert.Equal("Paracetamol 500 mg", detail!.Items[0].DrugName);
    }

    [Fact]
    public async Task Detail_MemuatJejakStatusnya()
    {
        await using var f = await CreateAsync();
        var created = await f.Service.CreateAsync(CreateRequest(f, "k1", (f.DrugAId, 10)));
        await f.Service.SubmitAsync(created.Id, new SubmitStockRequestRequest
        {
            ExpectedVersion = created.Version, IdempotencyKey = "s1"
        });

        var detail = await f.Service.GetDetailAsync(created.Id);

        Assert.Equal(2, detail!.Histories.Count);
        Assert.Equal(StockRequestStatus.Submitted, detail.Histories[0].ToStatus);
    }

    // ------------------------------------------------------------- sisi gudang

    /// <summary>Mengantar permintaan sampai berstatus Submitted, siap diputuskan gudang.</summary>
    private static async Task<StockRequestDetailResponse> SubmittedAsync(Fixture f,
        params (Guid DrugId, decimal Qty)[] items)
    {
        var created = await f.Service.CreateAsync(CreateRequest(f, "k1", items));
        return await f.Service.SubmitAsync(created.Id, new SubmitStockRequestRequest
        {
            ExpectedVersion = created.Version, IdempotencyKey = "s1"
        });
    }

    /// <summary>
    /// Permintaan yang siap diserahkan, yaitu yang sudah dikirim ke gudang.
    /// </summary>
    /// <remarks>
    /// Tidak ada langkah persetujuan di antaranya. Gudang tidak memutuskan permintaan;
    /// begitu permintaan dikirim, gudang sudah boleh mencatat penyerahannya.
    /// </remarks>
    private static Task<StockRequestDetailResponse> SiapDiserahkanAsync(Fixture f,
        params (Guid DrugId, decimal Qty)[] items) => SubmittedAsync(f, items);

    [Fact]
    public async Task Serahkan_MencatatJumlahTiapBarisDanMenutupPermintaan()
    {
        await using var f = await CreateAsync();
        var approved = await SiapDiserahkanAsync(f, (f.DrugAId, 10), (f.DrugBId, 4));

        var result = await f.Service.FulfillAsync(approved.Id, new FulfillStockRequestRequest
        {
            Items =
            [
                new() { StockRequestItemId = approved.Items[0].Id, FulfilledQuantity = 10 },
                new() { StockRequestItemId = approved.Items[1].Id, FulfilledQuantity = 4 }
            ],
            ExpectedVersion = approved.Version,
            IdempotencyKey = "f1"
        });

        Assert.Equal(StockRequestStatus.Completed, result.Status);
        Assert.Equal(10m, result.Items[0].FulfilledQuantity);
        Assert.Equal(4m, result.Items[1].FulfilledQuantity);
        Assert.False(result.CanCancel);
    }

    [Fact]
    public async Task Serahkan_Sebagian_TetapMenutupTetapiSelisihnyaTerbaca()
    {
        await using var f = await CreateAsync();
        var approved = await SiapDiserahkanAsync(f, (f.DrugAId, 10));

        var result = await f.Service.FulfillAsync(approved.Id, new FulfillStockRequestRequest
        {
            Items = [new() { StockRequestItemId = approved.Items[0].Id, FulfilledQuantity = 3 }],
            ExpectedVersion = approved.Version,
            IdempotencyKey = "f1"
        });

        Assert.Equal(StockRequestStatus.Completed, result.Status);
        Assert.Equal(10m, result.Items[0].RequestedQuantity);
        Assert.Equal(3m, result.Items[0].FulfilledQuantity);
    }

    [Fact]
    public async Task Serahkan_NolSah_DanBerbedaDariBelumDiserahkan()
    {
        await using var f = await CreateAsync();
        var approved = await SiapDiserahkanAsync(f, (f.DrugAId, 10));

        Assert.Null(approved.Items[0].FulfilledQuantity);

        var result = await f.Service.FulfillAsync(approved.Id, new FulfillStockRequestRequest
        {
            Items = [new() { StockRequestItemId = approved.Items[0].Id, FulfilledQuantity = 0 }],
            ExpectedVersion = approved.Version,
            IdempotencyKey = "f1"
        });

        Assert.Equal(0m, result.Items[0].FulfilledQuantity);
    }

    [Fact]
    public async Task Serahkan_LebihBanyakDariYangDiminta_Ditolak()
    {
        await using var f = await CreateAsync();
        var approved = await SiapDiserahkanAsync(f, (f.DrugAId, 10));

        var exception = await Assert.ThrowsAsync<StockRequestUnprocessableException>(
            () => f.Service.FulfillAsync(approved.Id, new FulfillStockRequestRequest
            {
                Items = [new() { StockRequestItemId = approved.Items[0].Id, FulfilledQuantity = 11 }],
                ExpectedVersion = approved.Version,
                IdempotencyKey = "f1"
            }));

        Assert.Equal("PHM006", exception.Code);
    }

    [Fact]
    public async Task Serahkan_AdaBarisYangTidakDisebut_Ditolak()
    {
        await using var f = await CreateAsync();
        var approved = await SiapDiserahkanAsync(f, (f.DrugAId, 10), (f.DrugBId, 4));

        // Diam bukan berarti nol: baris yang terlewat harus dipersoalkan, bukan diterima.
        var exception = await Assert.ThrowsAsync<StockRequestUnprocessableException>(
            () => f.Service.FulfillAsync(approved.Id, new FulfillStockRequestRequest
            {
                Items = [new() { StockRequestItemId = approved.Items[0].Id, FulfilledQuantity = 10 }],
                ExpectedVersion = approved.Version,
                IdempotencyKey = "f1"
            }));

        Assert.Equal("PHM007", exception.Code);
    }

    [Fact]
    public async Task Serahkan_BarisMilikPermintaanLain_Ditolak()
    {
        await using var f = await CreateAsync();
        var approved = await SiapDiserahkanAsync(f, (f.DrugAId, 10));

        var exception = await Assert.ThrowsAsync<StockRequestUnprocessableException>(
            () => f.Service.FulfillAsync(approved.Id, new FulfillStockRequestRequest
            {
                Items = [new() { StockRequestItemId = Guid.NewGuid(), FulfilledQuantity = 1 }],
                ExpectedVersion = approved.Version,
                IdempotencyKey = "f1"
            }));

        Assert.Equal("PHM007", exception.Code);
    }

    [Fact]
    public async Task Serahkan_PermintaanMasihDraft_Ditolak()
    {
        await using var f = await CreateAsync();

        // Draft belum sampai ke gudang. Menyerahkan barang atas permintaan yang belum
        // dikirim berarti gudang mengerjakan sesuatu yang belum diminta kepadanya.
        var created = await f.Service.CreateAsync(CreateRequest(f, "k1", (f.DrugAId, 10)));

        var exception = await Assert.ThrowsAsync<StockRequestConflictException>(
            () => f.Service.FulfillAsync(created.Id, new FulfillStockRequestRequest
            {
                Items = [new() { StockRequestItemId = created.Items[0].Id, FulfilledQuantity = 1 }],
                ExpectedVersion = created.Version,
                IdempotencyKey = "f1"
            }));

        Assert.Equal("PHM004", exception.Code);
    }

    [Fact]
    public async Task Batal_PermintaanTerkirimYangBelumDiserahkan_MasihBolehDibatalkan()
    {
        await using var f = await CreateAsync();
        var approved = await SiapDiserahkanAsync(f, (f.DrugAId, 10));

        var result = await f.Service.CancelAsync(approved.Id, new CancelStockRequestRequest
        {
            Reason = "Kebutuhan sudah terpenuhi dari unit lain.",
            ExpectedVersion = approved.Version,
            IdempotencyKey = "c1"
        });

        Assert.Equal(StockRequestStatus.Cancelled, result.Status);
    }

    [Fact]
    public async Task Batal_PermintaanYangSudahDiserahkan_Ditolak()
    {
        await using var f = await CreateAsync();
        var approved = await SiapDiserahkanAsync(f, (f.DrugAId, 10));
        var completed = await f.Service.FulfillAsync(approved.Id, new FulfillStockRequestRequest
        {
            Items = [new() { StockRequestItemId = approved.Items[0].Id, FulfilledQuantity = 10 }],
            ExpectedVersion = approved.Version,
            IdempotencyKey = "f1"
        });

        var exception = await Assert.ThrowsAsync<StockRequestConflictException>(
            () => f.Service.CancelAsync(completed.Id, new CancelStockRequestRequest
            {
                Reason = "Terlambat.", ExpectedVersion = completed.Version, IdempotencyKey = "c1"
            }));

        Assert.Equal("PHM004", exception.Code);
    }

    // ------------------------------------------- penyerahan dan persediaan sungguhan

    /// <summary>
    /// Menyiapkan permintaan yang penyerahannya benar-benar menyentuh persediaan.
    /// </summary>
    /// <remarks>
    /// Pengujian lain sengaja tidak memasang layanan persediaan supaya alur permintaan dapat
    /// diperiksa tanpa menyiapkan stok. Di sini justru kaitannya yang diuji.
    /// </remarks>
    private static async Task<(StockRequestService Service, DrugStockService Stock, Guid LocationId)>
        CreateWithStockAsync(Fixture f)
    {
        var accessor = new MutableHttpContextAccessor();
        accessor.SetUser(Guid.NewGuid());
        var logger = new LoggerService(NullLogger<LoggerService>.Instance, accessor);
        var stock = new DrugStockService(f.Context, accessor, logger);

        var location = await f.Context.Set<MstDrugStorageLocation>()
            .FirstAsync(x => x.Id == f.StorageLocationId);
        location.IsAllowDispensing = true;
        await f.Context.SaveChangesAsync();

        return (new StockRequestService(f.Context, accessor, logger, stock), stock, f.StorageLocationId);
    }

    [Fact]
    public async Task Serahkan_SatuBarisGagalStok_TidakMeninggalkanPenguranganSebagian()
    {
        await using var f = await CreateAsync();
        var (service, stock, locationId) = await CreateWithStockAsync(f);

        // Obat A cukup, obat B sengaja tidak cukup.
        await stock.RecordOpeningBalanceAsync(new RecordOpeningBalanceRequest
        {
            Batch = new DrugBatchInput
            {
                DrugId = f.DrugAId, BatchNumber = "BA-1",
                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(90)
            },
            StorageLocationId = locationId, Quantity = 100, IdempotencyKey = "ob-a"
        });

        await stock.RecordOpeningBalanceAsync(new RecordOpeningBalanceRequest
        {
            Batch = new DrugBatchInput
            {
                DrugId = f.DrugBId, BatchNumber = "BB-1",
                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(90)
            },
            StorageLocationId = locationId, Quantity = 1, IdempotencyKey = "ob-b"
        });

        var created = await service.CreateAsync(
            CreateRequest(f, "k1", (f.DrugAId, 10), (f.DrugBId, 5)));

        await service.SubmitAsync(created.Id, new SubmitStockRequestRequest
        {
            ExpectedVersion = created.Version, IdempotencyKey = "sub-1"
        });

        var submitted = (await service.GetDetailAsync(created.Id))!;

        await Assert.ThrowsAsync<StockRequestUnprocessableException>(
            () => service.FulfillAsync(submitted.Id, new FulfillStockRequestRequest
            {
                Items = [.. submitted.Items.Select(i => new FulfillStockRequestItemInput
                {
                    StockRequestItemId = i.Id, FulfilledQuantity = i.RequestedQuantity
                })],
                ExpectedVersion = submitted.Version,
                IdempotencyKey = "ful-1"
            }));

        // Baris pertama sempat berhasil dikeluarkan. Bila pengurangannya tetap tersimpan,
        // sepuluh tablet hilang dari gudang tanpa satu pun dokumen yang menutupinya —
        // selisih yang tidak akan pernah dapat dijelaskan.
        var saldo = await stock.GetBalancesAsync(new DrugStockBalanceQuery());
        Assert.Equal(100m, saldo.Items.Single(x => x.DrugId == f.DrugAId).QuantityOnHand);
        Assert.Equal(1m, saldo.Items.Single(x => x.DrugId == f.DrugBId).QuantityOnHand);

        // Permintaannya pun harus tetap terbuka, bukan tertutup separuh jalan.
        var sesudah = (await service.GetDetailAsync(submitted.Id))!;
        Assert.Equal(StockRequestStatus.Submitted, sesudah.Status);
    }
}
