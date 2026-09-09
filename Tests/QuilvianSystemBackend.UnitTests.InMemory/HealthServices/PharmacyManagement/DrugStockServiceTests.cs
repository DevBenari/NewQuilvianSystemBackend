using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.HealthServices.NutritionManagement;

namespace QuilvianSystemBackend.Tests.HealthServices.PharmacyManagement;

/// <summary>
/// Pengujian sumber kebenaran stok obat.
/// </summary>
/// <remarks>
/// Yang diuji adalah janji yang menentukan: stok tidak dapat menjadi negatif, pengeluaran
/// selalu mengambil batch yang paling dekat kedaluwarsa, stok karantina dan kedaluwarsa tidak
/// pernah ikut keluar, reservasi tidak dapat diambil dua kali, dan koreksi tidak pernah
/// menghapus jejak.
/// </remarks>
public sealed class DrugStockServiceTests
{
    private sealed class Fixture : IAsyncDisposable
    {
        public required ApplicationDbContext Context { get; init; }
        public required DrugStockService Service { get; init; }
        public required Guid DrugId { get; init; }
        public required Guid LocationId { get; init; }
        public required Guid OtherLocationId { get; init; }

        public async ValueTask DisposeAsync() => await Context.DisposeAsync();
    }

    private static async Task<Fixture> CreateAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"drug-stock-{Guid.NewGuid():N}").Options;
        var context = new ApplicationDbContext(options);

        var drugId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var otherLocationId = Guid.NewGuid();
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

        context.Set<MstDrugStorageLocation>().AddRange(
            new MstDrugStorageLocation
            {
                Id = locationId, StorageLocationCode = "GD1",
                StorageLocationName = "Gudang Farmasi", IsActive = true
            },
            new MstDrugStorageLocation
            {
                Id = otherLocationId, StorageLocationCode = "DP1",
                StorageLocationName = "Depo IGD", IsActive = true
            });

        await context.SaveChangesAsync();

        var accessor = new MutableHttpContextAccessor();
        accessor.SetUser(Guid.NewGuid());

        return new Fixture
        {
            Context = context,
            Service = new DrugStockService(context, accessor,
                new LoggerService(NullLogger<LoggerService>.Instance, accessor)),
            DrugId = drugId,
            LocationId = locationId,
            OtherLocationId = otherLocationId
        };
    }

    /// <summary>Memasukkan stok pembuka satu batch dan mengembalikan saldonya.</summary>
    private static Task<DrugStockBalanceResponse> IsiAsync(Fixture f, string batchNumber,
        DateOnly expiry, decimal quantity, Guid? locationId = null) =>
        f.Service.RecordOpeningBalanceAsync(new RecordOpeningBalanceRequest
        {
            Batch = new DrugBatchInput
            {
                DrugId = f.DrugId, BatchNumber = batchNumber, ExpiryDate = expiry
            },
            StorageLocationId = locationId ?? f.LocationId,
            Quantity = quantity,
            IdempotencyKey = $"open-{batchNumber}-{locationId ?? f.LocationId:N}"
        });

    private static DateOnly Hari(int selisih) =>
        DateOnly.FromDateTime(DateTime.UtcNow).AddDays(selisih);

    // ------------------------------------------------------------ saldo pembuka

    [Fact]
    public async Task SaldoPembuka_MembuatBatchDanSaldoBesertaKartuStok()
    {
        await using var f = await CreateAsync();

        var saldo = await IsiAsync(f, "B-001", Hari(180), 100);

        Assert.Equal(100m, saldo.QuantityOnHand);
        Assert.Equal(0m, saldo.QuantityReserved);
        Assert.Equal(100m, saldo.QuantityAvailable);

        var kartu = await f.Service.GetMutationsAsync(new DrugStockMutationQuery());
        var baris = Assert.Single(kartu.Items);
        Assert.Equal(DrugStockMutationType.OpeningBalance, baris.MutationType);
        Assert.Equal(0m, baris.BalanceBefore);
        Assert.Equal(100m, baris.BalanceAfter);
    }

    [Fact]
    public async Task SaldoPembuka_TanpaTanggalKedaluwarsa_Ditolak()
    {
        await using var f = await CreateAsync();

        var exception = await Assert.ThrowsAsync<DrugStockUnprocessableException>(
            () => f.Service.RecordOpeningBalanceAsync(new RecordOpeningBalanceRequest
            {
                Batch = new DrugBatchInput
                {
                    DrugId = f.DrugId, BatchNumber = "B-NO-EXP", ExpiryDate = null
                },
                StorageLocationId = f.LocationId,
                Quantity = 10,
                IdempotencyKey = "k1"
            }));

        Assert.Equal("PHM030", exception.Code);
    }

    [Fact]
    public async Task SaldoPembuka_BatchSamaTanggalBerbeda_Ditolak()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(180), 100);

        // Satu nomor batch tidak boleh punya dua tanggal kedaluwarsa; salah satunya pasti
        // salah ketik, dan FEFO akan memilih berdasarkan angka yang keliru.
        var exception = await Assert.ThrowsAsync<DrugStockConflictException>(
            () => f.Service.RecordOpeningBalanceAsync(new RecordOpeningBalanceRequest
            {
                Batch = new DrugBatchInput
                {
                    DrugId = f.DrugId, BatchNumber = "B-001", ExpiryDate = Hari(365)
                },
                StorageLocationId = f.LocationId,
                Quantity = 5,
                IdempotencyKey = "k2"
            }));

        Assert.Equal("PHM032", exception.Code);
    }

    // ------------------------------------------------------------------- FEFO

    [Fact]
    public async Task Keluar_MengambilBatchYangPalingDekatKedaluwarsaLebihDahulu()
    {
        await using var f = await CreateAsync();

        // Sengaja dimasukkan dengan urutan terbalik supaya urutan pengambilan tidak dapat
        // kebetulan benar hanya karena urutan pencatatannya.
        await IsiAsync(f, "B-LAMBAT", Hari(300), 50);
        await IsiAsync(f, "B-CEPAT", Hari(30), 20);
        await IsiAsync(f, "B-TENGAH", Hari(120), 30);

        var hasil = await f.Service.IssueAsync(new IssueStockRequest
        {
            DrugId = f.DrugId,
            StorageLocationId = f.LocationId,
            Quantity = 40,
            IdempotencyKey = "issue-1"
        });

        Assert.Equal(2, hasil.Allocations.Count);
        Assert.Equal("B-CEPAT", hasil.Allocations[0].BatchNumber);
        Assert.Equal(20m, hasil.Allocations[0].Quantity);
        Assert.Equal("B-TENGAH", hasil.Allocations[1].BatchNumber);
        Assert.Equal(20m, hasil.Allocations[1].Quantity);
    }

    [Fact]
    public async Task Keluar_MelebihiStok_Ditolak()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(180), 10);

        var exception = await Assert.ThrowsAsync<DrugStockUnprocessableException>(
            () => f.Service.IssueAsync(new IssueStockRequest
            {
                DrugId = f.DrugId,
                StorageLocationId = f.LocationId,
                Quantity = 11,
                IdempotencyKey = "issue-1"
            }));

        Assert.Equal("PHM028", exception.Code);

        // Penolakan tidak boleh menyisakan pengurangan sebagian.
        var saldo = await f.Service.GetBalancesAsync(new DrugStockBalanceQuery());
        Assert.Equal(10m, Assert.Single(saldo.Items).QuantityOnHand);
    }

    [Fact]
    public async Task Keluar_BatchKedaluwarsa_TidakIkutDiambil()
    {
        await using var f = await CreateAsync();

        await IsiAsync(f, "B-KADALUARSA", Hari(-1), 100);
        await IsiAsync(f, "B-SEGAR", Hari(90), 5);

        // Walaupun batch kedaluwarsa jauh lebih banyak dan lebih dulu kedaluwarsa, ia tidak
        // boleh dipakai sama sekali.
        var exception = await Assert.ThrowsAsync<DrugStockUnprocessableException>(
            () => f.Service.IssueAsync(new IssueStockRequest
            {
                DrugId = f.DrugId,
                StorageLocationId = f.LocationId,
                Quantity = 6,
                IdempotencyKey = "issue-1"
            }));

        Assert.Equal("PHM028", exception.Code);
    }

    [Fact]
    public async Task Keluar_LokasiLain_TidakIkutDihitung()
    {
        await using var f = await CreateAsync();

        await IsiAsync(f, "B-001", Hari(180), 100, f.OtherLocationId);

        var exception = await Assert.ThrowsAsync<DrugStockUnprocessableException>(
            () => f.Service.IssueAsync(new IssueStockRequest
            {
                DrugId = f.DrugId,
                StorageLocationId = f.LocationId,
                Quantity = 1,
                IdempotencyKey = "issue-1"
            }));

        Assert.Equal("PHM028", exception.Code);
    }

    [Fact]
    public async Task Keluar_MenurunkanSaldoDanMenulisKartuStok()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(180), 100);

        await f.Service.IssueAsync(new IssueStockRequest
        {
            DrugId = f.DrugId,
            StorageLocationId = f.LocationId,
            Quantity = 30,
            SourceDocumentType = DrugStockSourceDocumentTypes.StockRequest,
            SourceDocumentId = Guid.NewGuid(),
            IdempotencyKey = "issue-1"
        });

        var saldo = await f.Service.GetBalancesAsync(new DrugStockBalanceQuery());
        Assert.Equal(70m, Assert.Single(saldo.Items).QuantityOnHand);

        var kartu = await f.Service.GetMutationsAsync(new DrugStockMutationQuery
        {
            MutationType = DrugStockMutationType.StockOut
        });
        var baris = Assert.Single(kartu.Items);
        Assert.Equal(-30m, baris.QuantityChange);
        Assert.Equal(100m, baris.BalanceBefore);
        Assert.Equal(70m, baris.BalanceAfter);
        Assert.Equal(DrugStockSourceDocumentTypes.StockRequest, baris.SourceDocumentType);
    }

    // --------------------------------------------------------------- reservasi

    [Fact]
    public async Task Reservasi_MenahanTanpaMengurangiFisik()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(180), 100);

        await f.Service.ReserveAsync(new ReserveStockRequest
        {
            DrugId = f.DrugId, StorageLocationId = f.LocationId,
            Quantity = 40, IdempotencyKey = "res-1"
        });

        var saldo = Assert.Single((await f.Service.GetBalancesAsync(new DrugStockBalanceQuery())).Items);
        Assert.Equal(100m, saldo.QuantityOnHand);
        Assert.Equal(40m, saldo.QuantityReserved);
        Assert.Equal(60m, saldo.QuantityAvailable);
    }

    [Fact]
    public async Task Reservasi_TidakDapatDiambilProsesLain()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(180), 100);

        await f.Service.ReserveAsync(new ReserveStockRequest
        {
            DrugId = f.DrugId, StorageLocationId = f.LocationId,
            Quantity = 90, IdempotencyKey = "res-1"
        });

        // Sisa fisik masih 100, tetapi yang benar-benar tersedia tinggal 10.
        var exception = await Assert.ThrowsAsync<DrugStockUnprocessableException>(
            () => f.Service.IssueAsync(new IssueStockRequest
            {
                DrugId = f.DrugId, StorageLocationId = f.LocationId,
                Quantity = 11, IdempotencyKey = "issue-1"
            }));

        Assert.Equal("PHM028", exception.Code);
    }

    [Fact]
    public async Task Reservasi_DuaKali_TidakMenahanBarangYangSama()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(180), 100);

        await f.Service.ReserveAsync(new ReserveStockRequest
        {
            DrugId = f.DrugId, StorageLocationId = f.LocationId,
            Quantity = 60, IdempotencyKey = "res-1"
        });

        var exception = await Assert.ThrowsAsync<DrugStockUnprocessableException>(
            () => f.Service.ReserveAsync(new ReserveStockRequest
            {
                DrugId = f.DrugId, StorageLocationId = f.LocationId,
                Quantity = 60, IdempotencyKey = "res-2"
            }));

        Assert.Equal("PHM028", exception.Code);
    }

    [Fact]
    public async Task Reservasi_Dilepas_StokKembaliTersedia()
    {
        await using var f = await CreateAsync();
        var saldo = await IsiAsync(f, "B-001", Hari(180), 100);

        await f.Service.ReserveAsync(new ReserveStockRequest
        {
            DrugId = f.DrugId, StorageLocationId = f.LocationId,
            Quantity = 90, IdempotencyKey = "res-1"
        });

        await f.Service.ReleaseReservationAsync(saldo.DrugBatchId, f.LocationId, 90);

        var sesudah = Assert.Single((await f.Service.GetBalancesAsync(new DrugStockBalanceQuery())).Items);
        Assert.Equal(100m, sesudah.QuantityOnHand);
        Assert.Equal(0m, sesudah.QuantityReserved);
    }

    [Fact]
    public async Task Penyerahan_DariReservasi_MengurangiKeduanya()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(180), 100);

        await f.Service.ReserveAsync(new ReserveStockRequest
        {
            DrugId = f.DrugId, StorageLocationId = f.LocationId,
            Quantity = 40, IdempotencyKey = "res-1"
        });

        await f.Service.IssueAsync(new IssueStockRequest
        {
            DrugId = f.DrugId, StorageLocationId = f.LocationId,
            Quantity = 40, ConsumeReservation = true, IdempotencyKey = "issue-1"
        });

        var saldo = Assert.Single((await f.Service.GetBalancesAsync(new DrugStockBalanceQuery())).Items);
        Assert.Equal(60m, saldo.QuantityOnHand);
        Assert.Equal(0m, saldo.QuantityReserved);
    }

    // ---------------------------------------------------------------- karantina

    [Fact]
    public async Task Karantina_MemindahkanSebagianTanpaMengubahTotalFisik()
    {
        await using var f = await CreateAsync();
        var saldo = await IsiAsync(f, "B-001", Hari(180), 100);

        var hasil = await f.Service.ChangeStatusAsync(new ChangeStockStatusRequest
        {
            DrugBatchId = saldo.DrugBatchId,
            StorageLocationId = f.LocationId,
            FromStatus = DrugStockStatus.Available,
            ToStatus = DrugStockStatus.Quarantine,
            Quantity = 25,
            Reason = "Kemasan penyok, menunggu pemeriksaan.",
            ExpectedVersion = saldo.Version,
            IdempotencyKey = "q-1"
        });

        var tersedia = hasil.Single(x => x.Status == DrugStockStatus.Available);
        var karantina = hasil.Single(x => x.Status == DrugStockStatus.Quarantine);

        Assert.Equal(75m, tersedia.QuantityOnHand);
        Assert.Equal(25m, karantina.QuantityOnHand);
    }

    [Fact]
    public async Task Karantina_TidakIkutDikeluarkan()
    {
        await using var f = await CreateAsync();
        var saldo = await IsiAsync(f, "B-001", Hari(180), 100);

        await f.Service.ChangeStatusAsync(new ChangeStockStatusRequest
        {
            DrugBatchId = saldo.DrugBatchId,
            StorageLocationId = f.LocationId,
            FromStatus = DrugStockStatus.Available,
            ToStatus = DrugStockStatus.Quarantine,
            Quantity = 90,
            Reason = "Menunggu pemeriksaan mutu.",
            ExpectedVersion = saldo.Version,
            IdempotencyKey = "q-1"
        });

        // Barangnya masih di gudang, tetapi tidak boleh dipakai.
        var exception = await Assert.ThrowsAsync<DrugStockUnprocessableException>(
            () => f.Service.IssueAsync(new IssueStockRequest
            {
                DrugId = f.DrugId, StorageLocationId = f.LocationId,
                Quantity = 11, IdempotencyKey = "issue-1"
            }));

        Assert.Equal("PHM028", exception.Code);
    }

    [Fact]
    public async Task Karantina_TanpaAlasan_Ditolak()
    {
        await using var f = await CreateAsync();
        var saldo = await IsiAsync(f, "B-001", Hari(180), 100);

        var exception = await Assert.ThrowsAsync<DrugStockUnprocessableException>(
            () => f.Service.ChangeStatusAsync(new ChangeStockStatusRequest
            {
                DrugBatchId = saldo.DrugBatchId,
                StorageLocationId = f.LocationId,
                ToStatus = DrugStockStatus.Quarantine,
                Quantity = 10,
                Reason = "   ",
                ExpectedVersion = saldo.Version,
                IdempotencyKey = "q-1"
            }));

        Assert.Equal("PHM021", exception.Code);
    }

    // ------------------------------------------------------------- penyesuaian

    [Fact]
    public async Task Penyesuaian_TanpaAlasan_Ditolak()
    {
        await using var f = await CreateAsync();
        var saldo = await IsiAsync(f, "B-001", Hari(180), 100);

        var exception = await Assert.ThrowsAsync<DrugStockUnprocessableException>(
            () => f.Service.AdjustAsync(new AdjustStockRequest
            {
                DrugBatchId = saldo.DrugBatchId,
                StorageLocationId = f.LocationId,
                QuantityChange = -5,
                Reason = "",
                ExpectedVersion = saldo.Version,
                IdempotencyKey = "adj-1"
            }));

        Assert.Equal("PHM021", exception.Code);
    }

    [Fact]
    public async Task Penyesuaian_SampaiNegatif_Ditolak()
    {
        await using var f = await CreateAsync();
        var saldo = await IsiAsync(f, "B-001", Hari(180), 10);

        var exception = await Assert.ThrowsAsync<DrugStockUnprocessableException>(
            () => f.Service.AdjustAsync(new AdjustStockRequest
            {
                DrugBatchId = saldo.DrugBatchId,
                StorageLocationId = f.LocationId,
                QuantityChange = -11,
                Reason = "Hasil stock opname.",
                ExpectedVersion = saldo.Version,
                IdempotencyKey = "adj-1"
            }));

        Assert.Equal("PHM028", exception.Code);
    }

    [Fact]
    public async Task Penyesuaian_VersiTertinggal_Ditolak()
    {
        await using var f = await CreateAsync();
        var saldo = await IsiAsync(f, "B-001", Hari(180), 100);

        await f.Service.AdjustAsync(new AdjustStockRequest
        {
            DrugBatchId = saldo.DrugBatchId,
            StorageLocationId = f.LocationId,
            QuantityChange = -5,
            Reason = "Opname pertama.",
            ExpectedVersion = saldo.Version,
            IdempotencyKey = "adj-1"
        });

        var exception = await Assert.ThrowsAsync<DrugStockConflictException>(
            () => f.Service.AdjustAsync(new AdjustStockRequest
            {
                DrugBatchId = saldo.DrugBatchId,
                StorageLocationId = f.LocationId,
                QuantityChange = -5,
                Reason = "Opname kedua.",
                ExpectedVersion = saldo.Version,
                IdempotencyKey = "adj-2"
            }));

        Assert.Equal("PHM033", exception.Code);
    }

    // ---------------------------------------------------------------- koreksi

    [Fact]
    public async Task Koreksi_MenambahBarisBaruDanTidakMengubahYangLama()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(180), 100);

        var keluar = await f.Service.IssueAsync(new IssueStockRequest
        {
            DrugId = f.DrugId, StorageLocationId = f.LocationId,
            Quantity = 30, IdempotencyKey = "issue-1"
        });

        var mutationId = keluar.Allocations[0].MutationId;

        // Ternyata yang benar-benar keluar hanya 20, bukan 30.
        var koreksi = await f.Service.CorrectMutationAsync(new CorrectMutationRequest
        {
            MutationId = mutationId,
            CorrectedQuantityChange = -20,
            Reason = "Salah hitung saat serah terima.",
            IdempotencyKey = "cor-1"
        });

        Assert.Equal(mutationId, koreksi.CorrectionOfMutationId);
        Assert.Equal(10m, koreksi.QuantityChange);

        // Baris asli tetap apa adanya.
        var asli = (await f.Service.GetMutationsAsync(new DrugStockMutationQuery())).Items
            .Single(x => x.Id == mutationId);
        Assert.Equal(-30m, asli.QuantityChange);

        // Saldo menjadi benar tanpa menghapus jejak.
        var saldo = Assert.Single((await f.Service.GetBalancesAsync(new DrugStockBalanceQuery())).Items);
        Assert.Equal(80m, saldo.QuantityOnHand);
    }

    [Fact]
    public async Task Koreksi_DuaKaliPadaBarisYangSama_Ditolak()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(180), 100);

        var keluar = await f.Service.IssueAsync(new IssueStockRequest
        {
            DrugId = f.DrugId, StorageLocationId = f.LocationId,
            Quantity = 30, IdempotencyKey = "issue-1"
        });

        await f.Service.CorrectMutationAsync(new CorrectMutationRequest
        {
            MutationId = keluar.Allocations[0].MutationId,
            CorrectedQuantityChange = -20,
            Reason = "Koreksi pertama.",
            IdempotencyKey = "cor-1"
        });

        var exception = await Assert.ThrowsAsync<DrugStockConflictException>(
            () => f.Service.CorrectMutationAsync(new CorrectMutationRequest
            {
                MutationId = keluar.Allocations[0].MutationId,
                CorrectedQuantityChange = -25,
                Reason = "Koreksi kedua.",
                IdempotencyKey = "cor-2"
            }));

        Assert.Equal("PHM027", exception.Code);
    }

    [Fact]
    public async Task Koreksi_TanpaAlasan_Ditolak()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(180), 100);

        var keluar = await f.Service.IssueAsync(new IssueStockRequest
        {
            DrugId = f.DrugId, StorageLocationId = f.LocationId,
            Quantity = 30, IdempotencyKey = "issue-1"
        });

        var exception = await Assert.ThrowsAsync<DrugStockUnprocessableException>(
            () => f.Service.CorrectMutationAsync(new CorrectMutationRequest
            {
                MutationId = keluar.Allocations[0].MutationId,
                CorrectedQuantityChange = -20,
                Reason = "  ",
                IdempotencyKey = "cor-1"
            }));

        Assert.Equal("PHM021", exception.Code);
    }

    // ------------------------------------------------------- pemantauan kedaluwarsa

    [Fact]
    public async Task Saldo_DapatDisaringMenurutMendekatiKedaluwarsa()
    {
        await using var f = await CreateAsync();

        await IsiAsync(f, "B-DEKAT", Hari(20), 10);
        await IsiAsync(f, "B-JAUH", Hari(300), 10);

        var hasil = await f.Service.GetBalancesAsync(new DrugStockBalanceQuery
        {
            ExpiringWithinDays = 30
        });

        var baris = Assert.Single(hasil.Items);
        Assert.Equal("B-DEKAT", baris.BatchNumber);
        Assert.True(baris.DaysToExpiry <= 30);
    }

    [Fact]
    public async Task Saldo_DapatDisaringMenurutSudahKedaluwarsa()
    {
        await using var f = await CreateAsync();

        await IsiAsync(f, "B-LEWAT", Hari(-5), 10);
        await IsiAsync(f, "B-SEGAR", Hari(300), 10);

        var hasil = await f.Service.GetBalancesAsync(new DrugStockBalanceQuery
        {
            OnlyExpired = true
        });

        var baris = Assert.Single(hasil.Items);
        Assert.Equal("B-LEWAT", baris.BatchNumber);
        Assert.True(baris.DaysToExpiry < 0);
    }
}
