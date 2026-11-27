using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

namespace QuilvianSystemBackend.Tests.Pharmacy.LogisticsReportTests;

/// <summary>
/// Hitung selisih Stock Opname (FIN-LGS-084/087) di atas data uji sintetis pada SQLite dalam
/// memori. Hanya membaca: tidak ada sesi, mutasi, atau perubahan saldo yang boleh terjadi.
/// </summary>
public class StockOpnameTests
{
    private static readonly DateOnly Setahun = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));

    // ------------------------------------------------------------------- data uji

    private static DrugStockService Layanan(StockRequestApprovalHarness h)
    {
        var accessor = new HttpContextAccessor();
        return new DrugStockService(h.CreateContext(), accessor,
            new LoggerService(NullLogger<LoggerService>.Instance, accessor));
    }

    private static async Task<Guid> BatchAsync(StockRequestApprovalHarness h, string nomor, DateOnly kedaluwarsa)
    {
        using var k = h.CreateContext();
        var id = Guid.NewGuid();
        k.Set<PhmDrugBatch>().Add(new PhmDrugBatch
        {
            Id = id, DrugId = h.ObatId, BatchNumber = nomor, ExpiryDate = kedaluwarsa, CreateDateTime = DateTime.UtcNow
        });
        await k.SaveChangesAsync();
        return id;
    }

    private static async Task<Guid> SaldoAsync(StockRequestApprovalHarness h, Guid batch, Guid lokasi,
        DrugStockStatus status, decimal fisik, int versi = 0)
    {
        using var k = h.CreateContext();
        var id = Guid.NewGuid();
        k.PhmDrugStockBalances.Add(new PhmDrugStockBalance
        {
            Id = id, DrugId = h.ObatId, DrugBatchId = batch, StorageLocationId = lokasi, Status = status,
            QuantityOnHand = fisik, Version = versi, CreateDateTime = DateTime.UtcNow
        });
        await k.SaveChangesAsync();
        return id;
    }

    private static async Task MutasiAsync(StockRequestApprovalHarness h, Guid batch, Guid lokasi,
        DrugStockStatus status, decimal perubahan, DateTime pada)
    {
        using var k = h.CreateContext();
        k.PhmDrugStockMutations.Add(new PhmDrugStockMutation
        {
            Id = Guid.NewGuid(), DrugId = h.ObatId, DrugBatchId = batch, StorageLocationId = lokasi,
            Status = status, MutationType = DrugStockMutationType.Adjustment, QuantityChange = perubahan,
            BalanceBefore = 0m, BalanceAfter = perubahan,
            SourceDocumentType = DrugStockSourceDocumentTypes.ManualAdjustment, ActorUserId = h.StafUserId,
            OccurredAt = pada, Source = "UJI:opname", CreateDateTime = DateTime.UtcNow
        });
        await k.SaveChangesAsync();
    }

    private static async Task<Guid> LokasiAsync(StockRequestApprovalHarness h, string kode, bool dihapus = false)
    {
        using var k = h.CreateContext();
        var id = Guid.NewGuid();
        k.MstDrugStorageLocations.Add(new MstDrugStorageLocation
        {
            Id = id, StorageLocationCode = kode, StorageLocationName = $"Lokasi {kode}", IsActive = true,
            IsDelete = dihapus, CreateDateTime = DateTime.UtcNow
        });
        await k.SaveChangesAsync();
        return id;
    }

    private static async Task SatuanStokObatAsync(StockRequestApprovalHarness h)
    {
        using var k = h.CreateContext();
        var obat = await k.MstDrugs.SingleAsync(x => x.Id == h.ObatId);
        obat.StockUnitMeasurementId = h.SatuanId;
        await k.SaveChangesAsync();
    }

    private static StockOpnameCountInput Hitung(Guid saldo, decimal lembar, decimal? fisik, string? catatan = null) =>
        new() { BalanceId = saldo, SnapshotQuantity = lembar, PhysicalQuantity = fisik, Note = catatan };

    private static StockOpnameVariancePreviewRequest Lembar(Guid? lokasi, DateTime? dimuat,
        params StockOpnameCountInput[] baris) =>
        new() { StorageLocationId = lokasi, SnapshotTakenAtUtc = dimuat, Counts = [.. baris] };

    // ------------------------------------------------------------- aturan selisih

    [Theory]
    [InlineData(10, 10, false, 10, StockOpnameVarianceResults.Match, 0)]
    [InlineData(10, 10, false, 12.5, StockOpnameVarianceResults.Surplus, 2.5)]
    [InlineData(10, 10, false, 0, StockOpnameVarianceResults.Shortage, -10)]
    // Toleransi selisih 0 (prinsip 4): selisih terkecil yang tercatat pun bukan "Cocok".
    [InlineData(10, 10, false, 10.001, StockOpnameVarianceResults.Surplus, 0.001)]
    public void Kalkulator_menghitung_selisih_fisik_dikurangi_saldo_saat_ini(int saatIni, int lembar,
        bool bergerak, double fisik, string hasil, double selisih)
    {
        var evaluasi = StockOpnameCalculator.Evaluate(saatIni, lembar, bergerak, (decimal)fisik);
        Assert.Equal(hasil, evaluasi.Result);
        Assert.Equal((decimal?)(decimal)selisih, evaluasi.Variance);
        Assert.False(evaluasi.ChangedSinceSnapshot);
    }

    [Fact]
    public void Kalkulator_tidak_memberi_selisih_untuk_baris_belum_dihitung_atau_yang_saldonya_berubah()
    {
        var belum = StockOpnameCalculator.Evaluate(10m, 10m, false, null);
        Assert.Equal(StockOpnameVarianceResults.NotCounted, belum.Result);
        Assert.Null(belum.Variance);

        // Jumlah di lembar hitung berbeda dengan saldo saat ini.
        var jumlahBerubah = StockOpnameCalculator.Evaluate(8m, 10m, false, 10m);
        Assert.Equal(StockOpnameVarianceResults.NeedsRecount, jumlahBerubah.Result);
        Assert.Null(jumlahBerubah.Variance);
        Assert.True(jumlahBerubah.ChangedSinceSnapshot);

        // Jumlah kembali sama, tetapi ada mutasi sesudah lembar hitung dimuat (keluar lalu masuk).
        var bergerak = StockOpnameCalculator.Evaluate(10m, 10m, true, 10m);
        Assert.Equal(StockOpnameVarianceResults.NeedsRecount, bergerak.Result);
        Assert.Null(bergerak.Variance);

        // Belum dihitung tetap belum dihitung, tetapi perubahannya ditandai.
        var belumBerubah = StockOpnameCalculator.Evaluate(8m, 10m, false, null);
        Assert.Equal(StockOpnameVarianceResults.NotCounted, belumBerubah.Result);
        Assert.True(belumBerubah.ChangedSinceSnapshot);

        Assert.True(StockOpnameCalculator.HasValidScale(1.125m));
        Assert.False(StockOpnameCalculator.HasValidScale(1.1255m));
    }

    // ------------------------------------------------------------- hitung selisih

    [Fact]
    public async Task Hitung_selisih_membandingkan_stok_fisik_dengan_saldo_termasuk_nol_rusak_dan_kedaluwarsa()
    {
        using var h = new StockRequestApprovalHarness();
        await h.SediakanMasterObatAsync();
        await SatuanStokObatAsync(h);

        var b1 = await BatchAsync(h, "UJI-OPN-1", Setahun);
        var b2 = await BatchAsync(h, "UJI-OPN-2", Setahun.AddMonths(6));
        var lewat = await BatchAsync(h, "UJI-OPN-ED", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)));

        var cocok = await SaldoAsync(h, b1, h.GudangId, DrugStockStatus.Available, 10m);
        var lebih = await SaldoAsync(h, b2, h.GudangId, DrugStockStatus.Available, 5m);
        // Status Rusak dihitung sebagai baris tersendiri; fisik nol adalah hasil hitung yang sah.
        var rusak = await SaldoAsync(h, b1, h.GudangId, DrugStockStatus.Damaged, 3m);
        var kedaluwarsa = await SaldoAsync(h, lewat, h.GudangId, DrugStockStatus.Available, 4m);
        // Barang yang sudah dipindah ke status Kedaluwarsa juga dihitung per baris statusnya.
        var statusKedaluwarsa = await SaldoAsync(h, lewat, h.GudangId, DrugStockStatus.Expired, 2m);

        var hasil = await Layanan(h).PreviewOpnameVarianceAsync(Lembar(h.GudangId, DateTime.UtcNow.AddMinutes(-5),
            Hitung(cocok, 10m, 10m),
            Hitung(lebih, 5m, 7m, "  ditemukan di rak belakang  "),
            Hitung(rusak, 3m, 0m),
            Hitung(kedaluwarsa, 4m, null),
            Hitung(statusKedaluwarsa, 2m, 2m)));

        Assert.False(hasil.IsPersisted);
        Assert.Equal(h.GudangId, hasil.StorageLocationId);
        Assert.Equal("Gudang Induk (Uji)", hasil.StorageLocationName);
        Assert.NotEmpty(hasil.Limitations);
        Assert.Equal(5, hasil.Lines.Count);

        var baris = hasil.Lines.ToDictionary(x => x.BalanceId);
        Assert.Equal(StockOpnameVarianceResults.Match, baris[cocok].Result);
        Assert.Equal((decimal?)0m, baris[cocok].Variance);
        Assert.Equal("Obat Uji Satu", baris[cocok].DrugName);
        Assert.Equal("Tablet (Uji)", baris[cocok].StockUnitName);
        Assert.Equal("UJI-OPN-1", baris[cocok].BatchNumber);

        Assert.Equal(StockOpnameVarianceResults.Surplus, baris[lebih].Result);
        Assert.Equal((decimal?)2m, baris[lebih].Variance);
        Assert.Equal("ditemukan di rak belakang", baris[lebih].Note);

        Assert.Equal(StockOpnameVarianceResults.Shortage, baris[rusak].Result);
        Assert.Equal((decimal?)-3m, baris[rusak].Variance);
        Assert.Equal(DrugStockStatus.Damaged, baris[rusak].Status);
        Assert.Equal((decimal?)0m, baris[rusak].PhysicalQuantity);

        Assert.Equal(StockOpnameVarianceResults.NotCounted, baris[kedaluwarsa].Result);
        Assert.Null(baris[kedaluwarsa].Variance);
        Assert.True(baris[kedaluwarsa].IsExpired);
        Assert.False(baris[cocok].IsExpired);

        Assert.Equal(StockOpnameVarianceResults.Match, baris[statusKedaluwarsa].Result);
        Assert.Equal(DrugStockStatus.Expired, baris[statusKedaluwarsa].Status);
        Assert.True(baris[statusKedaluwarsa].IsExpired);

        Assert.Equal(5, hasil.Summary.LineCount);
        Assert.Equal(4, hasil.Summary.CountedLineCount);
        Assert.Equal(1, hasil.Summary.NotCountedLineCount);
        Assert.Equal(2, hasil.Summary.MatchCount);
        Assert.Equal(1, hasil.Summary.SurplusCount);
        Assert.Equal(1, hasil.Summary.ShortageCount);
        Assert.Equal(0, hasil.Summary.NeedsRecountCount);
        Assert.False(hasil.Summary.IsComplete);
    }

    [Fact]
    public async Task Saldo_yang_berubah_sejak_lembar_hitung_dimuat_harus_dihitung_ulang()
    {
        using var h = new StockRequestApprovalHarness();
        await h.SediakanMasterObatAsync();
        var depoLain = await LokasiAsync(h, "UJI-OPN-DEPO");
        var dimuat = DateTime.UtcNow.AddMinutes(-30);

        var b1 = await BatchAsync(h, "UJI-OPN-A", Setahun);
        var b2 = await BatchAsync(h, "UJI-OPN-B", Setahun);
        var b3 = await BatchAsync(h, "UJI-OPN-C", Setahun);
        var b4 = await BatchAsync(h, "UJI-OPN-D", Setahun);

        // Jumlahnya berubah dari 10 menjadi 8 sesudah lembar dicetak.
        var jumlahBerubah = await SaldoAsync(h, b1, h.GudangId, DrugStockStatus.Available, 8m);
        // Keluar 2 lalu masuk 2 sesudah lembar dimuat: jumlah sama, tetapi barang sempat bergerak.
        var bergerak = await SaldoAsync(h, b2, h.GudangId, DrugStockStatus.Available, 6m);
        await MutasiAsync(h, b2, h.GudangId, DrugStockStatus.Available, -2m, dimuat.AddMinutes(5));
        await MutasiAsync(h, b2, h.GudangId, DrugStockStatus.Available, 2m, dimuat.AddMinutes(10));
        // Mutasi sebelum lembar dimuat tidak memengaruhi.
        var tenang = await SaldoAsync(h, b3, h.GudangId, DrugStockStatus.Available, 4m);
        await MutasiAsync(h, b3, h.GudangId, DrugStockStatus.Available, 4m, dimuat.AddHours(-2));
        // Mutasi pada status lain dan di lokasi lain untuk batch yang sama tidak memengaruhi baris ini.
        var statusLain = await SaldoAsync(h, b4, h.GudangId, DrugStockStatus.Available, 9m);
        await MutasiAsync(h, b4, h.GudangId, DrugStockStatus.Quarantine, 1m, dimuat.AddMinutes(1));
        await MutasiAsync(h, b4, depoLain, DrugStockStatus.Available, 1m, dimuat.AddMinutes(1));

        var hasil = await Layanan(h).PreviewOpnameVarianceAsync(Lembar(h.GudangId, dimuat,
            Hitung(jumlahBerubah, 10m, 10m),
            Hitung(bergerak, 6m, 6m),
            Hitung(tenang, 4m, 4m),
            Hitung(statusLain, 9m, 9m)));

        var baris = hasil.Lines.ToDictionary(x => x.BalanceId);
        Assert.Equal(StockOpnameVarianceResults.NeedsRecount, baris[jumlahBerubah].Result);
        Assert.Null(baris[jumlahBerubah].Variance);
        Assert.Equal(10m, baris[jumlahBerubah].SnapshotQuantity);
        Assert.Equal(8m, baris[jumlahBerubah].CurrentQuantity);

        Assert.Equal(StockOpnameVarianceResults.NeedsRecount, baris[bergerak].Result);
        Assert.True(baris[bergerak].ChangedSinceSnapshot);

        Assert.Equal(StockOpnameVarianceResults.Match, baris[tenang].Result);
        Assert.Equal(StockOpnameVarianceResults.Match, baris[statusLain].Result);

        Assert.Equal(2, hasil.Summary.NeedsRecountCount);
        Assert.False(hasil.Summary.IsComplete);

        // Tanpa waktu lembar hitung, hanya jumlah yang dibandingkan.
        var tanpaWaktu = await Layanan(h).PreviewOpnameVarianceAsync(Lembar(h.GudangId, null,
            Hitung(bergerak, 6m, 6m)));
        Assert.Equal(StockOpnameVarianceResults.Match, Assert.Single(tanpaWaktu.Lines).Result);
        Assert.True(tanpaWaktu.Summary.IsComplete);
    }

    [Fact]
    public async Task Isian_lembar_hitung_yang_tidak_sah_ditolak_utuh_dengan_PHM037()
    {
        using var h = new StockRequestApprovalHarness();
        var batch = await BatchAsync(h, "UJI-OPN-V", Setahun);
        var saldo = await SaldoAsync(h, batch, h.GudangId, DrugStockStatus.Available, 10m);

        var tidakSah = new[]
        {
            Lembar(h.GudangId, null),
            Lembar(h.GudangId, null, new StockOpnameCountInput { BalanceId = null, SnapshotQuantity = 1m }),
            Lembar(h.GudangId, null, new StockOpnameCountInput { BalanceId = Guid.Empty, SnapshotQuantity = 1m }),
            Lembar(h.GudangId, null, Hitung(saldo, 10m, 1m), Hitung(saldo, 10m, 2m)),
            Lembar(h.GudangId, null, new StockOpnameCountInput { BalanceId = saldo, PhysicalQuantity = 1m }),
            Lembar(h.GudangId, null, Hitung(saldo, 10m, -1m)),
            Lembar(h.GudangId, null, Hitung(saldo, 10m, 1.0005m)),
            Lembar(h.GudangId, null, Hitung(saldo, 10m, StockOpnameCalculator.MaxQuantity + 1m)),
            new StockOpnameVariancePreviewRequest
            {
                StorageLocationId = h.GudangId,
                Counts = [.. Enumerable.Range(0, StockOpnameCalculator.MaxLines + 1)
                    .Select(_ => Hitung(Guid.NewGuid(), 1m, 1m))]
            }
        };

        foreach (var lembar in tidakSah)
        {
            var galat = await Assert.ThrowsAsync<LogisticsReportUnprocessableException>(() =>
                Layanan(h).PreviewOpnameVarianceAsync(lembar));
            Assert.Equal("PHM037", galat.Code);
        }
    }

    [Fact]
    public async Task Lokasi_wajib_terdaftar_PHM036_dan_saldo_harus_milik_lokasi_itu_PHM038()
    {
        using var h = new StockRequestApprovalHarness();
        var dihapus = await LokasiAsync(h, "UJI-OPN-HAPUS", dihapus: true);
        var depoLain = await LokasiAsync(h, "UJI-OPN-LAIN");
        var batch = await BatchAsync(h, "UJI-OPN-L", Setahun);
        var diGudang = await SaldoAsync(h, batch, h.GudangId, DrugStockStatus.Available, 10m);
        var diDepo = await SaldoAsync(h, batch, depoLain, DrugStockStatus.Available, 3m);

        foreach (var lokasi in new Guid?[] { null, Guid.Empty, Guid.NewGuid(), dihapus })
        {
            var galat = await Assert.ThrowsAsync<LogisticsReportUnprocessableException>(() =>
                Layanan(h).PreviewOpnameVarianceAsync(Lembar(lokasi, null, Hitung(diGudang, 10m, 10m))));
            Assert.Equal("PHM036", galat.Code);
        }

        // Saldo lokasi lain, saldo yang tidak ada, dan saldo yang sudah dihapus tidak dilewati diam-diam.
        using (var k = h.CreateContext())
        {
            var terhapus = new PhmDrugStockBalance
            {
                Id = Guid.NewGuid(), DrugId = h.ObatId, DrugBatchId = batch, StorageLocationId = h.GudangId,
                Status = DrugStockStatus.Quarantine, QuantityOnHand = 1m, IsDelete = true,
                CreateDateTime = DateTime.UtcNow
            };
            k.PhmDrugStockBalances.Add(terhapus);
            await k.SaveChangesAsync();

            foreach (var asing in new[] { diDepo, Guid.NewGuid(), terhapus.Id })
            {
                var galat = await Assert.ThrowsAsync<LogisticsReportUnprocessableException>(() =>
                    Layanan(h).PreviewOpnameVarianceAsync(Lembar(h.GudangId, null,
                        Hitung(diGudang, 10m, 10m), Hitung(asing, 1m, 1m))));
                Assert.Equal("PHM038", galat.Code);
            }
        }
    }

    [Fact]
    public async Task Hitung_selisih_tidak_menyimpan_tidak_menulis_mutasi_dan_tidak_mengubah_saldo()
    {
        using var h = new StockRequestApprovalHarness();
        var batch = await BatchAsync(h, "UJI-OPN-RO", Setahun);
        var saldo = await SaldoAsync(h, batch, h.GudangId, DrugStockStatus.Available, 10m, versi: 4);

        int mutasiSebelum;
        using (var k = h.CreateContext()) mutasiSebelum = await k.PhmDrugStockMutations.CountAsync();

        var hasil = await Layanan(h).PreviewOpnameVarianceAsync(Lembar(h.GudangId, DateTime.UtcNow,
            Hitung(saldo, 10m, 6m)));
        Assert.Equal(StockOpnameVarianceResults.Shortage, Assert.Single(hasil.Lines).Result);
        Assert.False(hasil.IsPersisted);

        using var periksa = h.CreateContext();
        Assert.Equal(mutasiSebelum, await periksa.PhmDrugStockMutations.CountAsync());
        Assert.False(await periksa.PhmDrugStockMutations
            .AnyAsync(x => x.SourceDocumentType == DrugStockSourceDocumentTypes.StockOpname));
        var tersimpan = await periksa.PhmDrugStockBalances.SingleAsync(x => x.Id == saldo);
        Assert.Equal(10m, tersimpan.QuantityOnHand);
        Assert.Equal(4, tersimpan.Version);
    }

    [Fact]
    public async Task Endpoint_hitung_selisih_dijaga_DrugStock_Read_dan_menjawab_422()
    {
        var metode = typeof(DrugStockController).GetMethod(nameof(DrugStockController.PreviewOpnameVariance))!;
        var izin = Assert.Single(metode.GetCustomAttributes<AccessPermissionAttribute>());
        Assert.NotNull(izin.Arguments);
        Assert.Equal("DrugStock", izin.Arguments[0] as string);
        Assert.Equal("Read", izin.Arguments[1] as string);
        Assert.Equal("opname/variance-preview", metode.GetCustomAttribute<HttpPostAttribute>()!.Template);
        Assert.NotNull(typeof(DrugStockController).GetCustomAttribute<AuthorizeAttribute>());

        using var h = new StockRequestApprovalHarness();
        var accessor = new HttpContextAccessor();
        var controller = new DrugStockController(new DrugStockService(h.CreateContext(), accessor,
            new LoggerService(NullLogger<LoggerService>.Instance, accessor)));

        var tanpaLokasi = Assert.IsType<ObjectResult>(await controller.PreviewOpnameVariance(
            Lembar(null, null, Hitung(Guid.NewGuid(), 1m, 1m)), CancellationToken.None));
        Assert.Equal(422, tanpaLokasi.StatusCode);
        Assert.Contains("PHM036", JsonSerializer.Serialize(tanpaLokasi.Value));

        var lembarKosong = Assert.IsType<ObjectResult>(await controller.PreviewOpnameVariance(
            Lembar(h.GudangId, null), CancellationToken.None));
        Assert.Equal(422, lembarKosong.StatusCode);
        Assert.Contains("PHM037", JsonSerializer.Serialize(lembarKosong.Value));
    }
}
