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
/// Pengujian perpindahan stok antar lokasi.
/// </summary>
/// <remarks>
/// Yang diuji adalah janji yang menentukan: hanya lokasi yang diizinkan boleh menjadi ujung
/// transfer, persetujuan menahan stok sehingga tidak dapat diambil proses lain, barang keluar
/// dan masuk tercatat sebagai dua pergerakan terpisah pada batch yang sama, kekurangan dalam
/// perjalanan tetap terbaca, dan pembatalan melepas kembali stok yang tertahan.
/// </remarks>
public sealed class StockTransferServiceTests
{
    private sealed class Fixture : IAsyncDisposable
    {
        public required ApplicationDbContext Context { get; init; }
        public required StockTransferService Service { get; init; }
        public required DrugStockService StockService { get; init; }
        public required Guid DrugId { get; init; }
        public required Guid SourceId { get; init; }
        public required Guid DestinationId { get; init; }
        public required Guid NoTransferId { get; init; }
        public required Guid WorkforceId { get; init; }

        public async ValueTask DisposeAsync() => await Context.DisposeAsync();
    }

    private static async Task<Fixture> CreateAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"stock-transfer-{Guid.NewGuid():N}").Options;
        var context = new ApplicationDbContext(options);

        var drugId = Guid.NewGuid();
        var sourceId = Guid.NewGuid();
        var destinationId = Guid.NewGuid();
        var noTransferId = Guid.NewGuid();
        var workforceId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        context.Set<MstDrugCategory>().Add(new MstDrugCategory
        {
            Id = categoryId, DrugCategoryCode = "K1", DrugCategoryName = "Umum",
            DrugCategoryType = "General", IsActive = true
        });

        context.Set<MstDrug>().Add(new MstDrug
        {
            Id = drugId, DrugCategoryId = categoryId, DrugCode = "OBT-001",
            DrugName = "Paracetamol 500 mg", IsActive = true
        });

        context.Set<MstWorkforceProfile>().Add(new MstWorkforceProfile
        {
            Id = workforceId, ProfileCode = "WF1", DisplayName = "Petugas Uji", IsActive = true
        });

        context.Set<MstDrugStorageLocation>().AddRange(
            new MstDrugStorageLocation
            {
                Id = sourceId, StorageLocationCode = "GD1",
                StorageLocationName = "Gudang Farmasi", IsActive = true,
                IsAllowTransferIn = true, IsAllowTransferOut = true
            },
            new MstDrugStorageLocation
            {
                Id = destinationId, StorageLocationCode = "DP1",
                StorageLocationName = "Depo IGD", IsActive = true,
                IsAllowTransferIn = true, IsAllowTransferOut = true
            },
            // Lokasi yang sengaja tidak boleh menjadi ujung transfer, mewakili tempat
            // penyimpanan yang bukan depo dengan saldo sendiri.
            new MstDrugStorageLocation
            {
                Id = noTransferId, StorageLocationCode = "NOP",
                StorageLocationName = "Lemari Tanpa Saldo", IsActive = true,
                IsAllowTransferIn = false, IsAllowTransferOut = false
            });

        await context.SaveChangesAsync();

        var accessor = new MutableHttpContextAccessor();
        accessor.SetUser(Guid.NewGuid());
        var logger = new LoggerService(NullLogger<LoggerService>.Instance, accessor);
        var stockService = new DrugStockService(context, accessor, logger);

        return new Fixture
        {
            Context = context,
            StockService = stockService,
            Service = new StockTransferService(context, accessor, logger, stockService),
            DrugId = drugId,
            SourceId = sourceId,
            DestinationId = destinationId,
            NoTransferId = noTransferId,
            WorkforceId = workforceId
        };
    }

    private static DateOnly Hari(int selisih) =>
        DateOnly.FromDateTime(DateTime.UtcNow).AddDays(selisih);

    private static Task<DrugStockBalanceResponse> IsiAsync(Fixture f, string batch,
        DateOnly expiry, decimal quantity, Guid? locationId = null) =>
        f.StockService.RecordOpeningBalanceAsync(new RecordOpeningBalanceRequest
        {
            Batch = new DrugBatchInput
            {
                DrugId = f.DrugId, BatchNumber = batch, ExpiryDate = expiry
            },
            StorageLocationId = locationId ?? f.SourceId,
            Quantity = quantity,
            IdempotencyKey = $"open-{batch}"
        });

    private static CreateStockTransferRequest Permintaan(Fixture f, decimal quantity,
        string key = "t1") => new()
        {
            SourceStorageLocationId = f.SourceId,
            DestinationStorageLocationId = f.DestinationId,
            RequestedByWorkforceId = f.WorkforceId,
            Items = [new StockTransferItemInput { DrugId = f.DrugId, RequestedQuantity = quantity }],
            IdempotencyKey = key
        };

    /// <summary>Membawa transfer sampai berstatus disetujui, dengan stok sudah tertahan.</summary>
    private static async Task<StockTransferDetailResponse> DisetujuiAsync(Fixture f,
        decimal quantity, string key = "t1")
    {
        var dibuat = await f.Service.CreateAsync(Permintaan(f, quantity, key));

        var diajukan = await f.Service.SubmitAsync(dibuat.Id, new StockTransferCommandRequest
        {
            ExpectedVersion = dibuat.Version, IdempotencyKey = $"sub-{key}"
        });

        return await f.Service.ApproveAsync(diajukan.Id, new StockTransferCommandRequest
        {
            ExpectedVersion = diajukan.Version, IdempotencyKey = $"app-{key}"
        });
    }

    // ------------------------------------------------------------- kelayakan ujung

    [Fact]
    public async Task Buat_LokasiAsalTidakBolehMengirim_Ditolak()
    {
        await using var f = await CreateAsync();

        var permintaan = Permintaan(f, 10);
        permintaan.SourceStorageLocationId = f.NoTransferId;

        var exception = await Assert.ThrowsAsync<DrugStockUnprocessableException>(
            () => f.Service.CreateAsync(permintaan));

        Assert.Equal("PHM034", exception.Code);
    }

    [Fact]
    public async Task Buat_LokasiTujuanTidakBolehMenerima_Ditolak()
    {
        await using var f = await CreateAsync();

        var permintaan = Permintaan(f, 10);
        permintaan.DestinationStorageLocationId = f.NoTransferId;

        var exception = await Assert.ThrowsAsync<DrugStockUnprocessableException>(
            () => f.Service.CreateAsync(permintaan));

        Assert.Equal("PHM034", exception.Code);
    }

    [Fact]
    public async Task Buat_AsalDanTujuanSama_Ditolak()
    {
        await using var f = await CreateAsync();

        var permintaan = Permintaan(f, 10);
        permintaan.DestinationStorageLocationId = f.SourceId;

        var exception = await Assert.ThrowsAsync<StockTransferUnprocessableException>(
            () => f.Service.CreateAsync(permintaan));

        Assert.Equal("PHM040", exception.Code);
    }

    // ------------------------------------------------------------------ persetujuan

    [Fact]
    public async Task Setujui_MenahanStokMenurutKedaluwarsaTerdekat()
    {
        await using var f = await CreateAsync();

        await IsiAsync(f, "B-CEPAT", Hari(30), 20);
        await IsiAsync(f, "B-LAMBAT", Hari(300), 50);

        var disetujui = await DisetujuiAsync(f, 30);

        Assert.Equal(StockTransferStatus.Approved, disetujui.Status);

        var alokasi = Assert.Single(disetujui.Items).Allocations;
        Assert.Equal(2, alokasi.Count);
        Assert.Equal("B-CEPAT", alokasi[0].BatchNumber);
        Assert.Equal(20m, alokasi[0].Quantity);
        Assert.Equal("B-LAMBAT", alokasi[1].BatchNumber);
        Assert.Equal(10m, alokasi[1].Quantity);

        // Stok ditahan, bukan dikurangi: fisiknya masih utuh di lokasi asal.
        var saldo = await f.StockService.GetBalancesAsync(new DrugStockBalanceQuery
        {
            StorageLocationId = f.SourceId
        });
        Assert.Equal(70m, saldo.Items.Sum(x => x.QuantityOnHand));
        Assert.Equal(30m, saldo.Items.Sum(x => x.QuantityReserved));
        Assert.Equal(40m, saldo.Items.Sum(x => x.QuantityAvailable));
    }

    [Fact]
    public async Task Setujui_StokTidakCukup_Ditolak()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(90), 5);

        var exception = await Assert.ThrowsAsync<StockTransferUnprocessableException>(
            () => DisetujuiAsync(f, 10));

        Assert.Equal("PHM028", exception.Code);
        Assert.Contains("Paracetamol", exception.Message);
    }

    [Fact]
    public async Task Setujui_StokYangDitahanTidakDapatDiambilProsesLain()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(90), 100);

        await DisetujuiAsync(f, 90);

        // Fisiknya masih 100, tetapi yang tersedia tinggal 10.
        var exception = await Assert.ThrowsAsync<DrugStockUnprocessableException>(
            () => f.StockService.IssueAsync(new IssueStockRequest
            {
                DrugId = f.DrugId, StorageLocationId = f.SourceId,
                Quantity = 11, IdempotencyKey = "lain-1"
            }));

        Assert.Equal("PHM028", exception.Code);
    }

    // -------------------------------------------------------------- keluar dan terima

    [Fact]
    public async Task Keluar_MengurangiAsalDanBelumMenambahTujuan()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(90), 100);

        var disetujui = await DisetujuiAsync(f, 30);
        var dikeluarkan = await f.Service.IssueAsync(disetujui.Id,
            new StockTransferCommandRequest
            {
                ExpectedVersion = disetujui.Version, IdempotencyKey = "iss-1"
            });

        Assert.Equal(StockTransferStatus.InTransit, dikeluarkan.Status);
        Assert.Equal(30m, Assert.Single(dikeluarkan.Items).IssuedQuantity);

        var asal = await f.StockService.GetBalancesAsync(new DrugStockBalanceQuery
        {
            StorageLocationId = f.SourceId
        });
        Assert.Equal(70m, asal.Items.Sum(x => x.QuantityOnHand));
        Assert.Equal(0m, asal.Items.Sum(x => x.QuantityReserved));

        // Barang sedang di jalan: sudah tidak di asal, belum di tujuan.
        var tujuan = await f.StockService.GetBalancesAsync(new DrugStockBalanceQuery
        {
            StorageLocationId = f.DestinationId
        });
        Assert.Empty(tujuan.Items);
    }

    [Fact]
    public async Task Terima_MenambahTujuanPadaBatchYangSama()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(90), 100);

        var disetujui = await DisetujuiAsync(f, 30);
        var dikeluarkan = await f.Service.IssueAsync(disetujui.Id,
            new StockTransferCommandRequest
            {
                ExpectedVersion = disetujui.Version, IdempotencyKey = "iss-1"
            });

        var selesai = await f.Service.ReceiveAsync(dikeluarkan.Id,
            new ReceiveStockTransferRequest
            {
                Items = [.. dikeluarkan.Items.Select(i => new ReceiveStockTransferItemInput
                {
                    StockTransferItemId = i.Id, ReceivedQuantity = i.IssuedQuantity ?? 0
                })],
                ExpectedVersion = dikeluarkan.Version,
                IdempotencyKey = "rec-1"
            });

        Assert.Equal(StockTransferStatus.Completed, selesai.Status);

        var tujuan = await f.StockService.GetBalancesAsync(new DrugStockBalanceQuery
        {
            StorageLocationId = f.DestinationId
        });
        var baris = Assert.Single(tujuan.Items);
        Assert.Equal(30m, baris.QuantityOnHand);

        // Nomor batch tetap melekat pada barangnya ke mana pun ia berpindah; tanpa itu
        // penarikan obat tidak dapat menelusurinya sampai lokasi tujuan.
        Assert.Equal("B-001", baris.BatchNumber);
    }

    [Fact]
    public async Task Terima_LebihSedikitDariYangDikirim_SelisihnyaTerbaca()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(90), 100);

        var disetujui = await DisetujuiAsync(f, 30);
        var dikeluarkan = await f.Service.IssueAsync(disetujui.Id,
            new StockTransferCommandRequest
            {
                ExpectedVersion = disetujui.Version, IdempotencyKey = "iss-1"
            });

        var selesai = await f.Service.ReceiveAsync(dikeluarkan.Id,
            new ReceiveStockTransferRequest
            {
                Items = [new() { StockTransferItemId = dikeluarkan.Items[0].Id, ReceivedQuantity = 25 }],
                Note = "Lima tablet rusak di perjalanan.",
                ExpectedVersion = dikeluarkan.Version,
                IdempotencyKey = "rec-1"
            });

        var item = Assert.Single(selesai.Items);
        Assert.Equal(30m, item.IssuedQuantity);
        Assert.Equal(25m, item.ReceivedQuantity);

        // Yang hilang di perjalanan tidak muncul di mana pun sebagai stok, dan itu memang
        // yang diinginkan: selisihnya terbaca dari dokumennya sendiri.
        var tujuan = await f.StockService.GetBalancesAsync(new DrugStockBalanceQuery
        {
            StorageLocationId = f.DestinationId
        });
        Assert.Equal(25m, Assert.Single(tujuan.Items).QuantityOnHand);
    }

    [Fact]
    public async Task Terima_LebihBanyakDariYangDikirim_Ditolak()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(90), 100);

        var disetujui = await DisetujuiAsync(f, 30);
        var dikeluarkan = await f.Service.IssueAsync(disetujui.Id,
            new StockTransferCommandRequest
            {
                ExpectedVersion = disetujui.Version, IdempotencyKey = "iss-1"
            });

        var exception = await Assert.ThrowsAsync<StockTransferUnprocessableException>(
            () => f.Service.ReceiveAsync(dikeluarkan.Id, new ReceiveStockTransferRequest
            {
                Items = [new() { StockTransferItemId = dikeluarkan.Items[0].Id, ReceivedQuantity = 31 }],
                ExpectedVersion = dikeluarkan.Version,
                IdempotencyKey = "rec-1"
            }));

        Assert.Equal("PHM042", exception.Code);
    }

    [Fact]
    public async Task Terima_BarisTidakDisebut_Ditolak()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(90), 100);

        var disetujui = await DisetujuiAsync(f, 30);
        var dikeluarkan = await f.Service.IssueAsync(disetujui.Id,
            new StockTransferCommandRequest
            {
                ExpectedVersion = disetujui.Version, IdempotencyKey = "iss-1"
            });

        var exception = await Assert.ThrowsAsync<StockTransferUnprocessableException>(
            () => f.Service.ReceiveAsync(dikeluarkan.Id, new ReceiveStockTransferRequest
            {
                Items = [new() { StockTransferItemId = Guid.NewGuid(), ReceivedQuantity = 1 }],
                ExpectedVersion = dikeluarkan.Version,
                IdempotencyKey = "rec-1"
            }));

        Assert.Equal("PHM048", exception.Code);
    }

    // ------------------------------------------------------------------ kartu stok

    [Fact]
    public async Task Transfer_MenulisDuaPergerakanPadaKartuStok()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(90), 100);

        var disetujui = await DisetujuiAsync(f, 30);
        var dikeluarkan = await f.Service.IssueAsync(disetujui.Id,
            new StockTransferCommandRequest
            {
                ExpectedVersion = disetujui.Version, IdempotencyKey = "iss-1"
            });

        await f.Service.ReceiveAsync(dikeluarkan.Id, new ReceiveStockTransferRequest
        {
            Items = [new() { StockTransferItemId = dikeluarkan.Items[0].Id, ReceivedQuantity = 30 }],
            ExpectedVersion = dikeluarkan.Version,
            IdempotencyKey = "rec-1"
        });

        var kartu = await f.StockService.GetMutationsAsync(new DrugStockMutationQuery
        {
            SourceDocumentType = DrugStockSourceDocumentTypes.StockTransfer
        });

        Assert.Equal(2, kartu.Items.Count);
        Assert.Contains(kartu.Items, x =>
            x.StorageLocationId == f.SourceId && x.QuantityChange == -30m);
        Assert.Contains(kartu.Items, x =>
            x.StorageLocationId == f.DestinationId && x.QuantityChange == 30m);
    }

    // ----------------------------------------------------------------- pembatalan

    [Fact]
    public async Task Batal_MelepasStokYangTertahan()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(90), 100);

        var disetujui = await DisetujuiAsync(f, 40);

        await f.Service.CancelAsync(disetujui.Id, new StockTransferReasonRequest
        {
            Reason = "Kebutuhan sudah terpenuhi dari lokasi lain.",
            ExpectedVersion = disetujui.Version,
            IdempotencyKey = "can-1"
        });

        // Tanpa pelepasan ini, stok tersandera selamanya oleh transfer yang tidak terjadi.
        var saldo = await f.StockService.GetBalancesAsync(new DrugStockBalanceQuery
        {
            StorageLocationId = f.SourceId
        });
        var baris = Assert.Single(saldo.Items);
        Assert.Equal(100m, baris.QuantityOnHand);
        Assert.Equal(0m, baris.QuantityReserved);
    }

    [Fact]
    public async Task Batal_SetelahBarangKeluar_Ditolak()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(90), 100);

        var disetujui = await DisetujuiAsync(f, 30);
        var dikeluarkan = await f.Service.IssueAsync(disetujui.Id,
            new StockTransferCommandRequest
            {
                ExpectedVersion = disetujui.Version, IdempotencyKey = "iss-1"
            });

        var exception = await Assert.ThrowsAsync<StockTransferConflictException>(
            () => f.Service.CancelAsync(dikeluarkan.Id, new StockTransferReasonRequest
            {
                Reason = "Terlambat.",
                ExpectedVersion = dikeluarkan.Version,
                IdempotencyKey = "can-1"
            }));

        Assert.Equal("PHM041", exception.Code);
    }

    // -------------------------------------------------------------------- transisi

    [Fact]
    public async Task Keluar_SebelumDisetujui_Ditolak()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(90), 100);

        var dibuat = await f.Service.CreateAsync(Permintaan(f, 10));

        var exception = await Assert.ThrowsAsync<StockTransferConflictException>(
            () => f.Service.IssueAsync(dibuat.Id, new StockTransferCommandRequest
            {
                ExpectedVersion = dibuat.Version, IdempotencyKey = "iss-1"
            }));

        Assert.Equal("PHM041", exception.Code);
    }

    [Fact]
    public async Task Ubah_SetelahDiajukan_Ditolak()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(90), 100);

        var dibuat = await f.Service.CreateAsync(Permintaan(f, 10));
        var diajukan = await f.Service.SubmitAsync(dibuat.Id, new StockTransferCommandRequest
        {
            ExpectedVersion = dibuat.Version, IdempotencyKey = "sub-1"
        });

        var exception = await Assert.ThrowsAsync<StockTransferConflictException>(
            () => f.Service.UpdateAsync(diajukan.Id, new UpdateStockTransferRequest
            {
                DestinationStorageLocationId = f.DestinationId,
                Items = [new StockTransferItemInput { DrugId = f.DrugId, RequestedQuantity = 5 }],
                ExpectedVersion = diajukan.Version,
                IdempotencyKey = "upd-1"
            }));

        Assert.Equal("PHM041", exception.Code);
    }

    [Fact]
    public async Task Tolak_MenyimpanAlasannyaDanMenutupTransfer()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(90), 100);

        var dibuat = await f.Service.CreateAsync(Permintaan(f, 10));
        var diajukan = await f.Service.SubmitAsync(dibuat.Id, new StockTransferCommandRequest
        {
            ExpectedVersion = dibuat.Version, IdempotencyKey = "sub-1"
        });

        var ditolak = await f.Service.RejectAsync(diajukan.Id, new StockTransferReasonRequest
        {
            Reason = "Stok gudang sedang menipis.",
            ExpectedVersion = diajukan.Version,
            IdempotencyKey = "rej-1"
        });

        Assert.Equal(StockTransferStatus.Rejected, ditolak.Status);
        Assert.Equal("Stok gudang sedang menipis.", ditolak.DecisionReason);

        // Penolakan terjadi sebelum persetujuan, jadi tidak ada stok yang perlu dilepas.
        var saldo = await f.StockService.GetBalancesAsync(new DrugStockBalanceQuery());
        Assert.Equal(0m, Assert.Single(saldo.Items).QuantityReserved);
    }

    [Fact]
    public async Task Buat_ObatKembar_Ditolak()
    {
        await using var f = await CreateAsync();

        var permintaan = Permintaan(f, 10);
        permintaan.Items.Add(new StockTransferItemInput
        {
            DrugId = f.DrugId, RequestedQuantity = 5
        });

        var exception = await Assert.ThrowsAsync<StockTransferUnprocessableException>(
            () => f.Service.CreateAsync(permintaan));

        Assert.Equal("PHM047", exception.Code);
    }

    [Fact]
    public async Task Buat_KunciSamaDikirimDuaKali_TidakMenggandakan()
    {
        await using var f = await CreateAsync();

        var pertama = await f.Service.CreateAsync(Permintaan(f, 10));
        var kedua = await f.Service.CreateAsync(Permintaan(f, 10));

        Assert.Equal(pertama.Id, kedua.Id);
        Assert.Single(f.Context.PhmStockTransfers);
    }
}
