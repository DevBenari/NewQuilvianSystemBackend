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
        Assert.Single(f.Context.TrxStockRequests);
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
}
