using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Repositories.Configurations.HealthServices.PharmacyManagement;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

namespace QuilvianSystemBackend.Tests.Pharmacy.StockOpnameTests;

/// <summary>
/// Transaksi Stock Opname (FIN-LGS-091/093) di atas SQLite dalam memori. Skemanya dibentuk dari model
/// EF, jadi uji ini membuktikan logika sesi dan detail perhitungan tanpa migrasi; tabel di basis data
/// sungguhan tetap menunggu migrasi oleh pihak berwenang.
/// </summary>
public class StockOpnameTransactionTests
{
    private static readonly DateOnly Setahun = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));

    // ------------------------------------------------------------------- data uji

    private static StockOpnameService Layanan(StockRequestApprovalHarness h, Guid userId, CatatanLogUji? log = null)
    {
        var k = h.CreateContext();
        var accessor = h.AksesorPengguna(userId);
        var logger = new LoggerService(log ?? (ILogger<LoggerService>)NullLogger<LoggerService>.Instance, accessor);
        return new StockOpnameService(k, accessor, logger, new DrugStockService(k, accessor, logger), h.AlokatorNomor());
    }

    private static async Task<StockRequestApprovalHarness> SiapkanAsync()
    {
        var h = new StockRequestApprovalHarness();
        await h.SediakanMasterObatAsync();
        await h.SediakanIzinStockOpnameAsync();
        using var k = h.CreateContext();
        var obat = await k.MstDrugs.SingleAsync(x => x.Id == h.ObatId);
        obat.StockUnitMeasurementId = h.SatuanId;
        await k.SaveChangesAsync();
        return h;
    }

    private static async Task<Guid> BatchAsync(StockRequestApprovalHarness h, string nomor)
    {
        using var k = h.CreateContext();
        var id = Guid.NewGuid();
        k.Set<PhmDrugBatch>().Add(new PhmDrugBatch
        {
            Id = id, DrugId = h.ObatId, BatchNumber = nomor, ExpiryDate = Setahun, CreateDateTime = DateTime.UtcNow
        });
        await k.SaveChangesAsync();
        return id;
    }

    private static async Task<Guid> SaldoAsync(StockRequestApprovalHarness h, Guid batch, Guid lokasi,
        DrugStockStatus status, decimal fisik, decimal ditahan = 0m)
    {
        using var k = h.CreateContext();
        var id = Guid.NewGuid();
        k.PhmDrugStockBalances.Add(new PhmDrugStockBalance
        {
            Id = id, DrugId = h.ObatId, DrugBatchId = batch, StorageLocationId = lokasi, Status = status,
            QuantityOnHand = fisik, QuantityReserved = ditahan, Version = 1, CreateDateTime = DateTime.UtcNow
        });
        await k.SaveChangesAsync();
        return id;
    }

    /// <summary>Pergerakan stok oleh proses lain sesudah snapshot: saldo berubah dan mutasinya tercatat.</summary>
    private static async Task GerakkanStokAsync(StockRequestApprovalHarness h, Guid saldoId, decimal perubahan)
    {
        using var k = h.CreateContext();
        var saldo = await k.PhmDrugStockBalances.SingleAsync(x => x.Id == saldoId);
        var sebelum = saldo.QuantityOnHand;
        saldo.QuantityOnHand += perubahan;
        saldo.Version++;
        k.PhmDrugStockMutations.Add(new PhmDrugStockMutation
        {
            Id = Guid.NewGuid(), DrugId = saldo.DrugId, DrugBatchId = saldo.DrugBatchId,
            StorageLocationId = saldo.StorageLocationId, Status = saldo.Status,
            MutationType = DrugStockMutationType.Adjustment, QuantityChange = perubahan, BalanceBefore = sebelum,
            BalanceAfter = saldo.QuantityOnHand, SourceDocumentType = DrugStockSourceDocumentTypes.ManualAdjustment,
            ActorUserId = h.StafUserId, OccurredAt = DateTime.UtcNow, Source = "UJI:opname",
            CorrelationId = $"uji-gerak-{Guid.NewGuid():N}", CreateDateTime = DateTime.UtcNow
        });
        await k.SaveChangesAsync();
    }

    private static async Task<Guid> LokasiAsync(StockRequestApprovalHarness h, string kode)
    {
        using var k = h.CreateContext();
        var id = Guid.NewGuid();
        k.MstDrugStorageLocations.Add(new MstDrugStorageLocation
        {
            Id = id, StorageLocationCode = kode, StorageLocationName = $"Lokasi {kode}", IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });
        await k.SaveChangesAsync();
        return id;
    }

    private static CreateStockOpnameRequest Buat(Guid lokasi, DrugStockStatus? cakupan = null, Guid? sebelumnya = null) =>
        new() { StorageLocationId = lokasi, StockStatusScope = cakupan, PreviousStockOpnameId = sebelumnya };

    private static SaveStockOpnameCountsRequest Hitung(StockOpnameDetailResponse sesi,
        params (Guid LineId, decimal? Fisik)[] baris) =>
        new()
        {
            ExpectedVersion = sesi.Version,
            Counts = [.. baris.Select(x => new StockOpnameCountEntry { LineId = x.LineId, PhysicalQuantity = x.Fisik })]
        };

    private static StockOpnameCommandRequest Versi(StockOpnameDetailResponse sesi) => new() { ExpectedVersion = sesi.Version };

    private static DecideStockOpnameRequest Putusan(StockOpnameDetailResponse sesi, string? alasan = null) =>
        new() { ExpectedVersion = sesi.Version, Reason = alasan };

    private static Guid Baris(StockOpnameDetailResponse sesi, Guid saldoId) =>
        sesi.Lines.Single(x => x.DrugStockBalanceId == saldoId).Id;

    /// <summary>Sesi diajukan Staf dengan hasil hitung yang diberikan per saldo.</summary>
    private static async Task<StockOpnameDetailResponse> DiajukanAsync(StockRequestApprovalHarness h,
        params (Guid SaldoId, decimal Fisik)[] hasil)
    {
        var sesi = await Layanan(h, h.StafUserId).CreateAsync(Buat(h.GudangId));
        sesi = await Layanan(h, h.StafUserId).SaveCountsAsync(sesi.Id,
            Hitung(sesi, [.. hasil.Select(x => (Baris(sesi, x.SaldoId), (decimal?)x.Fisik))]));
        return await Layanan(h, h.StafUserId).SubmitAsync(sesi.Id, Versi(sesi));
    }

    private static async Task<(decimal Saldo, int Versi)> SaldoTersimpanAsync(StockRequestApprovalHarness h, Guid saldoId)
    {
        using var k = h.CreateContext();
        var saldo = await k.PhmDrugStockBalances.AsNoTracking().SingleAsync(x => x.Id == saldoId);
        return (saldo.QuantityOnHand, saldo.Version);
    }

    private static async Task<List<PhmDrugStockMutation>> MutasiOpnameAsync(StockRequestApprovalHarness h)
    {
        using var k = h.CreateContext();
        return await k.PhmDrugStockMutations.AsNoTracking()
            .Where(x => x.SourceDocumentType == DrugStockSourceDocumentTypes.StockOpname)
            .ToListAsync();
    }

    // ------------------------------------------------------------------- buat sesi

    [Fact]
    public async Task Membuat_sesi_memotret_seluruh_saldo_lokasi_termasuk_nol_dan_menerbitkan_nomor_bersama()
    {
        using var h = await SiapkanAsync();
        var lain = await LokasiAsync(h, "UJI-OPN-LAIN");
        var b1 = await BatchAsync(h, "OPN-1");
        var siap = await SaldoAsync(h, b1, h.GudangId, DrugStockStatus.Available, 10m);
        var rusakNol = await SaldoAsync(h, b1, h.GudangId, DrugStockStatus.Damaged, 0m);
        await SaldoAsync(h, b1, lain, DrugStockStatus.Available, 99m);

        var sesi = await Layanan(h, h.StafUserId).CreateAsync(Buat(h.GudangId));

        Assert.Equal("SOP-000001", sesi.OpnameNumber);
        Assert.Equal(StockOpnameStatus.Counting, sesi.Status);
        Assert.Equal(1, sesi.Version);
        Assert.True(sesi.IsPersisted);
        Assert.Equal(h.StafUserId, sesi.CreatedByUserId);
        Assert.Equal("Gudang Induk (Uji)", sesi.StorageLocationName);
        Assert.Equal(2, sesi.LineCount);
        Assert.Equal(0, sesi.CountedLineCount);

        var barisSiap = sesi.Lines.Single(x => x.DrugStockBalanceId == siap);
        Assert.Equal(10m, barisSiap.SnapshotQuantity);
        Assert.Equal("Obat Uji Satu", barisSiap.DrugName);
        Assert.Equal("Tablet (Uji)", barisSiap.StockUnitName);
        Assert.Equal("OPN-1", barisSiap.BatchNumber);
        Assert.Equal(StockOpnameVarianceResults.NotCounted, barisSiap.Result);
        Assert.Equal(0m, sesi.Lines.Single(x => x.DrugStockBalanceId == rusakNol).SnapshotQuantity);
        Assert.True(sesi.CanEdit);
        Assert.True(sesi.CanCancel);
        Assert.False(sesi.CanSubmit);

        var cakupan = await Layanan(h, h.StafUserId).CreateAsync(Buat(lain, DrugStockStatus.Available));
        Assert.Equal("SOP-000002", cakupan.OpnameNumber);
        Assert.Single(cakupan.Lines);
    }

    [Fact]
    public async Task Satu_sesi_aktif_per_lokasi_dan_pembatalan_membebaskan_lokasi()
    {
        using var h = await SiapkanAsync();
        await SaldoAsync(h, await BatchAsync(h, "OPN-A"), h.GudangId, DrugStockStatus.Available, 5m);

        var pertama = await Layanan(h, h.StafUserId).CreateAsync(Buat(h.GudangId));
        var galat = await Assert.ThrowsAsync<StockOpnameConflictException>(() =>
            Layanan(h, h.AtasanUserId).CreateAsync(Buat(h.GudangId)));
        Assert.Equal("PHM053", galat.Code);

        var batal = await Layanan(h, h.StafUserId).CancelAsync(pertama.Id, Versi(pertama));
        Assert.Equal(StockOpnameStatus.Cancelled, batal.Status);
        Assert.False(batal.CanEdit);

        var kedua = await Layanan(h, h.StafUserId).CreateAsync(Buat(h.GudangId));
        Assert.Equal(StockOpnameStatus.Counting, kedua.Status);

        foreach (var lokasi in new Guid?[] { null, Guid.Empty, Guid.NewGuid() })
        {
            var salah = await Assert.ThrowsAsync<StockOpnameUnprocessableException>(() =>
                Layanan(h, h.StafUserId).CreateAsync(new CreateStockOpnameRequest { StorageLocationId = lokasi }));
            Assert.Equal("PHM036", salah.Code);
        }
    }

    // ------------------------------------------------------------------- hitung

    [Fact]
    public async Task Hasil_hitung_divalidasi_dan_versi_usang_ditolak()
    {
        using var h = await SiapkanAsync();
        var saldo = await SaldoAsync(h, await BatchAsync(h, "OPN-V"), h.GudangId, DrugStockStatus.Available, 10m);
        var sesi = await Layanan(h, h.StafUserId).CreateAsync(Buat(h.GudangId));
        var baris = Baris(sesi, saldo);

        foreach (var permintaan in new[]
                 {
                     Hitung(sesi, (baris, -1m)),
                     Hitung(sesi, (baris, 1.0005m)),
                     Hitung(sesi, (baris, StockOpnameCalculator.MaxQuantity + 1m)),
                     Hitung(sesi, (Guid.NewGuid(), 1m)),
                     Hitung(sesi, (baris, 1m), (baris, 2m)),
                     new SaveStockOpnameCountsRequest { ExpectedVersion = sesi.Version }
                 })
        {
            var galat = await Assert.ThrowsAsync<StockOpnameUnprocessableException>(() =>
                Layanan(h, h.StafUserId).SaveCountsAsync(sesi.Id, permintaan));
            Assert.Equal("PHM037", galat.Code);
        }

        var tersimpan = await Layanan(h, h.StafUserId).SaveCountsAsync(sesi.Id, Hitung(sesi, (baris, 0m)));
        Assert.Equal(sesi.Version + 1, tersimpan.Version);
        var hasil = Assert.Single(tersimpan.Lines);
        Assert.Equal(0m, hasil.PhysicalQuantity);
        Assert.Equal(-10m, hasil.VarianceQuantity);
        Assert.Equal(StockOpnameVarianceResults.Shortage, hasil.Result);
        Assert.Equal(h.StafUserId, hasil.CountedByUserId);

        // Versi lama (sebelum simpan di atas) dan versi kosong ditolak.
        var usang = await Assert.ThrowsAsync<StockOpnameConflictException>(() =>
            Layanan(h, h.StafUserId).SaveCountsAsync(sesi.Id, Hitung(sesi, (baris, 3m))));
        Assert.Equal("PHM057", usang.Code);
        var tanpaVersi = await Assert.ThrowsAsync<StockOpnameUnprocessableException>(() =>
            Layanan(h, h.StafUserId).SubmitAsync(sesi.Id, new StockOpnameCommandRequest()));
        Assert.Equal("PHM058", tanpaVersi.Code);
    }

    [Fact]
    public async Task Saldo_yang_bergerak_sesudah_snapshot_memblokir_pengajuan_sampai_barisnya_dimuat_ulang()
    {
        using var h = await SiapkanAsync();
        var bergerak = await SaldoAsync(h, await BatchAsync(h, "OPN-G"), h.GudangId, DrugStockStatus.Available, 10m);
        var tenang = await SaldoAsync(h, await BatchAsync(h, "OPN-T"), h.GudangId, DrugStockStatus.Available, 4m);

        var sesi = await Layanan(h, h.StafUserId).CreateAsync(Buat(h.GudangId));
        sesi = await Layanan(h, h.StafUserId).SaveCountsAsync(sesi.Id,
            Hitung(sesi, (Baris(sesi, bergerak), 10m), (Baris(sesi, tenang), 4m)));
        Assert.True(sesi.CanSubmit);

        // Keluar 2 lalu masuk 2: jumlah akhir sama, tetapi barang sempat bergerak.
        await GerakkanStokAsync(h, bergerak, -2m);
        await GerakkanStokAsync(h, bergerak, 2m);

        var detail = (await Layanan(h, h.StafUserId).GetDetailAsync(sesi.Id))!;
        var baris = detail.Lines.Single(x => x.DrugStockBalanceId == bergerak);
        Assert.Equal(StockOpnameVarianceResults.NeedsRecount, baris.Result);
        Assert.True(baris.ChangedSinceSnapshot);
        Assert.Equal(StockOpnameVarianceResults.Match, detail.Lines.Single(x => x.DrugStockBalanceId == tenang).Result);
        Assert.False(detail.CanSubmit);
        Assert.Contains("berubah", detail.SubmitBlockedReason);

        var galat = await Assert.ThrowsAsync<StockOpnameUnprocessableException>(() =>
            Layanan(h, h.StafUserId).SubmitAsync(detail.Id, Versi(detail)));
        Assert.Equal("PHM054", galat.Code);

        var dimuatUlang = await Layanan(h, h.StafUserId).RefreshLineAsync(detail.Id, baris.Id, Versi(detail));
        var barisBaru = dimuatUlang.Lines.Single(x => x.DrugStockBalanceId == bergerak);
        Assert.Null(barisBaru.PhysicalQuantity);
        Assert.Equal(StockOpnameVarianceResults.NotCounted, barisBaru.Result);
        Assert.False(barisBaru.ChangedSinceSnapshot);
        // Baris lain tidak tersentuh.
        Assert.Equal(4m, dimuatUlang.Lines.Single(x => x.DrugStockBalanceId == tenang).PhysicalQuantity);

        var dihitung = await Layanan(h, h.StafUserId).SaveCountsAsync(detail.Id,
            Hitung(dimuatUlang, (barisBaru.Id, 10m)));
        var diajukan = await Layanan(h, h.StafUserId).SubmitAsync(detail.Id, Versi(dihitung));
        Assert.Equal(StockOpnameStatus.Submitted, diajukan.Status);
        Assert.False(diajukan.CanEdit);

        var terkunci = await Assert.ThrowsAsync<StockOpnameConflictException>(() =>
            Layanan(h, h.StafUserId).SaveCountsAsync(detail.Id, Hitung(diajukan, (barisBaru.Id, 9m))));
        Assert.Equal("PHM052", terkunci.Code);
    }

    // ------------------------------------------------------------------- setujui dan posting

    [Fact]
    public async Task Persetujuan_memposting_setiap_selisih_sekali_ke_kartu_stok_bersumber_StockOpname()
    {
        using var h = await SiapkanAsync();
        var kurang = await SaldoAsync(h, await BatchAsync(h, "OPN-K"), h.GudangId, DrugStockStatus.Available, 10m);
        var cocok = await SaldoAsync(h, await BatchAsync(h, "OPN-C"), h.GudangId, DrugStockStatus.Available, 5m);
        var lebih = await SaldoAsync(h, await BatchAsync(h, "OPN-L"), h.GudangId, DrugStockStatus.Damaged, 0m);
        // Toleransi 0: selisih 0,001 tetap diposting.
        var tipis = await SaldoAsync(h, await BatchAsync(h, "OPN-T"), h.GudangId, DrugStockStatus.Available, 4m);

        var diajukan = await DiajukanAsync(h, (kurang, 7m), (cocok, 5m), (lebih, 2m), (tipis, 4.001m));
        Assert.Equal(h.StafUserId, diajukan.SubmittedByUserId);

        var log = new CatatanLogUji();
        var selesai = await Layanan(h, h.AtasanUserId, log).ApproveAsync(diajukan.Id, Putusan(diajukan));

        Assert.Equal(StockOpnameStatus.Completed, selesai.Status);
        Assert.Equal(h.AtasanUserId, selesai.DecidedByUserId);
        Assert.Equal((7m, 2), await SaldoTersimpanAsync(h, kurang));
        Assert.Equal((5m, 1), await SaldoTersimpanAsync(h, cocok));
        Assert.Equal((2m, 2), await SaldoTersimpanAsync(h, lebih));
        Assert.Equal(4.001m, (await SaldoTersimpanAsync(h, tipis)).Saldo);

        var mutasi = await MutasiOpnameAsync(h);
        Assert.Equal(3, mutasi.Count);
        Assert.All(mutasi, x =>
        {
            Assert.Equal(DrugStockMutationType.Adjustment, x.MutationType);
            Assert.Equal(selesai.Id, x.SourceDocumentId);
            Assert.Equal(h.AtasanUserId, x.ActorUserId);
            Assert.StartsWith($"Stock Opname {selesai.OpnameNumber} baris", x.Reason);
        });
        Assert.Equal(new[] { -3m, 0.001m, 2m }.OrderBy(x => x), mutasi.Select(x => x.QuantityChange).OrderBy(x => x));

        foreach (var baris in selesai.Lines)
        {
            if (baris.DrugStockBalanceId == cocok) Assert.Null(baris.AdjustmentMutationId);
            else
            {
                var milik = Assert.Single(mutasi, x => x.Id == baris.AdjustmentMutationId);
                Assert.Equal($"opname-{baris.Id:N}", milik.CorrelationId);
            }
        }

        // Jejak audit memuat pelaku sebenarnya.
        var audit = Assert.Single(log.UntukAksi("StockOpname.Approve"));
        Assert.Equal(h.AtasanUserId.ToString(), CatatanLogUji.PelakuPada(audit.Pesan));

        // Persetujuan kedua tidak memposting apa pun.
        var ulang = await Assert.ThrowsAsync<StockOpnameConflictException>(() =>
            Layanan(h, h.PemohonUserId).ApproveAsync(selesai.Id, Putusan(selesai)));
        Assert.Equal("PHM052", ulang.Code);
        Assert.Equal(3, (await MutasiOpnameAsync(h)).Count);
    }

    [Fact]
    public async Task Mutasi_bersumber_sesi_yang_sudah_ada_mencegah_posting_ganda()
    {
        using var h = await SiapkanAsync();
        var saldo = await SaldoAsync(h, await BatchAsync(h, "OPN-D"), h.GudangId, DrugStockStatus.Available, 10m);
        var diajukan = await DiajukanAsync(h, (saldo, 8m));

        // Lapis kedua: mutasi bersumber sesi ini sudah ada (mis. posting yang sempat lolos), walau status
        // sesi belum berubah.
        using (var k = h.CreateContext())
        {
            k.PhmDrugStockMutations.Add(new PhmDrugStockMutation
            {
                Id = Guid.NewGuid(), DrugId = h.ObatId, DrugBatchId = diajukan.Lines[0].DrugBatchId!.Value,
                StorageLocationId = h.GudangId, Status = DrugStockStatus.Quarantine,
                MutationType = DrugStockMutationType.Adjustment, QuantityChange = 0m,
                SourceDocumentType = DrugStockSourceDocumentTypes.StockOpname, SourceDocumentId = diajukan.Id,
                ActorUserId = h.AtasanUserId, OccurredAt = DateTime.UtcNow.AddDays(-1), Source = "UJI:ganda",
                CorrelationId = "uji-ganda", CreateDateTime = DateTime.UtcNow
            });
            await k.SaveChangesAsync();
        }

        var galat = await Assert.ThrowsAsync<StockOpnameConflictException>(() =>
            Layanan(h, h.AtasanUserId).ApproveAsync(diajukan.Id, Putusan(diajukan)));
        Assert.Equal("PHM052", galat.Code);
        Assert.Equal(10m, (await SaldoTersimpanAsync(h, saldo)).Saldo);

        // Lapis ketiga: indeks unik kartu stok atas (SourceDocumentType, CorrelationId). Model uji SQLite
        // sengaja melepas keunikan indeks bersaring (ModelUjiSqlite), jadi yang diperiksa adalah
        // konfigurasi yang dipakai PostgreSQL; penegakannya baru terbukti sesudah migrasi.
        var indeks = IndeksDariKonfigurasi<PhmDrugStockMutation>(new PhmDrugStockMutationConfiguration(),
            nameof(PhmDrugStockMutation.SourceDocumentType), nameof(PhmDrugStockMutation.CorrelationId));
        Assert.True(indeks.IsUnique);
    }

    [Fact]
    public void Konfigurasi_PostgreSQL_menjaga_satu_sesi_aktif_per_lokasi_dan_satu_baris_per_saldo()
    {
        var aktif = IndeksDariKonfigurasi<PhmStockOpname>(new PhmStockOpnameConfiguration(),
            nameof(PhmStockOpname.StorageLocationId));
        Assert.True(aktif.IsUnique);
        Assert.Equal("\"Status\" IN (1, 2) AND \"IsDelete\" = false", aktif.GetFilter());

        Assert.True(IndeksDariKonfigurasi<PhmStockOpname>(new PhmStockOpnameConfiguration(),
            nameof(PhmStockOpname.OpnameNumber)).IsUnique);
        Assert.True(IndeksDariKonfigurasi<PhmStockOpnameLine>(new PhmStockOpnameLineConfiguration(),
            nameof(PhmStockOpnameLine.StockOpnameId), nameof(PhmStockOpnameLine.DrugStockBalanceId)).IsUnique);
        Assert.True(IndeksDariKonfigurasi<PhmStockOpnameLine>(new PhmStockOpnameLineConfiguration(),
            nameof(PhmStockOpnameLine.AdjustmentMutationId)).IsUnique);
    }

    /// <summary>Indeks seperti yang dikonfigurasi untuk PostgreSQL, tanpa penyesuaian model uji SQLite.</summary>
    private static Microsoft.EntityFrameworkCore.Metadata.IMutableIndex IndeksDariKonfigurasi<T>(
        IEntityTypeConfiguration<T> konfigurasi, params string[] kolom) where T : class
    {
        var builder = new ModelBuilder();
        konfigurasi.Configure(builder.Entity<T>());
        return builder.Model.FindEntityType(typeof(T))!.GetIndexes()
            .Single(x => x.Properties.Select(p => p.Name).SequenceEqual(kolom));
    }

    [Fact]
    public async Task Saldo_berubah_sesudah_diajukan_membatalkan_persetujuan_tanpa_perubahan_lalu_ditolak_dan_dihitung_ulang()
    {
        using var h = await SiapkanAsync();
        var saldo = await SaldoAsync(h, await BatchAsync(h, "OPN-S"), h.GudangId, DrugStockStatus.Available, 10m);
        var diajukan = await DiajukanAsync(h, (saldo, 6m));

        await GerakkanStokAsync(h, saldo, -1m);

        var detail = (await Layanan(h, h.AtasanUserId).GetDetailAsync(diajukan.Id))!;
        Assert.Equal(StockOpnameVarianceResults.NeedsRecount, Assert.Single(detail.Lines).Result);

        var galat = await Assert.ThrowsAsync<StockOpnameConflictException>(() =>
            Layanan(h, h.AtasanUserId).ApproveAsync(diajukan.Id, Putusan(diajukan)));
        Assert.Equal("PHM055", galat.Code);
        Assert.Equal((9m, 2), await SaldoTersimpanAsync(h, saldo));
        Assert.Empty(await MutasiOpnameAsync(h));
        Assert.Equal(StockOpnameStatus.Submitted, (await Layanan(h, h.AtasanUserId).GetDetailAsync(diajukan.Id))!.Status);

        var tanpaAlasan = await Assert.ThrowsAsync<StockOpnameUnprocessableException>(() =>
            Layanan(h, h.AtasanUserId).RejectAsync(diajukan.Id, Putusan(diajukan, "   ")));
        Assert.Equal("PHM058", tanpaAlasan.Code);

        var ditolak = await Layanan(h, h.AtasanUserId).RejectAsync(diajukan.Id,
            Putusan(diajukan, "Saldo berubah saat menunggu persetujuan."));
        Assert.Equal(StockOpnameStatus.Rejected, ditolak.Status);
        Assert.Equal("Saldo berubah saat menunggu persetujuan.", ditolak.DecisionReason);

        // Penolakan final: tidak dapat disetujui lagi; hitung ulang lewat sesi baru yang merujuknya.
        var final = await Assert.ThrowsAsync<StockOpnameConflictException>(() =>
            Layanan(h, h.AtasanUserId).ApproveAsync(ditolak.Id, Putusan(ditolak)));
        Assert.Equal("PHM052", final.Code);

        var ulang = await Layanan(h, h.StafUserId).CreateAsync(Buat(h.GudangId, sebelumnya: ditolak.Id));
        Assert.Equal(ditolak.Id, ulang.PreviousStockOpnameId);
        Assert.Equal(9m, Assert.Single(ulang.Lines).SnapshotQuantity);
    }

    // ------------------------------------------------------------------- wewenang

    [Fact]
    public async Task Self_approval_dilarang_bagi_pembuat_pengaju_dan_penghitung()
    {
        using var h = await SiapkanAsync();
        var saldo = await SaldoAsync(h, await BatchAsync(h, "OPN-X"), h.GudangId, DrugStockStatus.Available, 10m);
        var saldo2 = await SaldoAsync(h, await BatchAsync(h, "OPN-Y"), h.GudangId, DrugStockStatus.Available, 3m);

        // Pembuat dan pengaju: Atasan (memegang izin persetujuan) membuat sendiri.
        var sesi = await Layanan(h, h.AtasanUserId).CreateAsync(Buat(h.GudangId));
        // Penghitung: AtasanCito menghitung satu baris.
        sesi = await Layanan(h, h.AtasanCitoUserId).SaveCountsAsync(sesi.Id, Hitung(sesi, (Baris(sesi, saldo), 9m)));
        sesi = await Layanan(h, h.AtasanUserId).SaveCountsAsync(sesi.Id, Hitung(sesi, (Baris(sesi, saldo2), 3m)));
        var diajukan = await Layanan(h, h.AtasanUserId).SubmitAsync(sesi.Id, Versi(sesi));

        foreach (var pelaku in new[] { h.AtasanUserId, h.AtasanCitoUserId })
        {
            var detail = (await Layanan(h, pelaku).GetDetailAsync(diajukan.Id))!;
            Assert.False(detail.CanDecide);
            Assert.Contains("self-approval", detail.DecisionBlockedReason);

            var setuju = await Assert.ThrowsAsync<StockOpnameForbiddenException>(() =>
                Layanan(h, pelaku).ApproveAsync(diajukan.Id, Putusan(diajukan)));
            Assert.Equal("PHM067", setuju.Code);
            var tolak = await Assert.ThrowsAsync<StockOpnameForbiddenException>(() =>
                Layanan(h, pelaku).RejectAsync(diajukan.Id, Putusan(diajukan, "uji")));
            Assert.Equal("PHM067", tolak.Code);
        }

        Assert.Empty(await MutasiOpnameAsync(h));

        var penyetuju = (await Layanan(h, h.PemohonUserId).GetDetailAsync(diajukan.Id))!;
        Assert.True(penyetuju.CanDecide);
        var selesai = await Layanan(h, h.PemohonUserId).ApproveAsync(diajukan.Id, Putusan(diajukan));
        Assert.Equal(StockOpnameStatus.Completed, selesai.Status);
    }

    [Fact]
    public async Task Persetujuan_wajib_izin_eksplisit_tanpa_jalan_pintas_SuperAdmin()
    {
        using var h = await SiapkanAsync();
        var saldo = await SaldoAsync(h, await BatchAsync(h, "OPN-Z"), h.GudangId, DrugStockStatus.Available, 10m);
        var diajukan = await DiajukanAsync(h, (saldo, 10m));

        foreach (var pelaku in new[] { h.AtasanTanpaIzinUserId, h.SuperAdminUserId })
        {
            var detail = (await Layanan(h, pelaku).GetDetailAsync(diajukan.Id))!;
            Assert.False(detail.CanDecide);
            Assert.Contains("StockOpname:Approve", detail.DecisionBlockedReason);

            var galat = await Assert.ThrowsAsync<StockOpnameForbiddenException>(() =>
                Layanan(h, pelaku).ApproveAsync(diajukan.Id, Putusan(diajukan)));
            Assert.Equal("PHM056", galat.Code);
        }

        // Tanpa selisih: disetujui tanpa satu mutasi pun.
        var selesai = await Layanan(h, h.AtasanLainUserId).ApproveAsync(diajukan.Id, Putusan(diajukan));
        Assert.Equal(StockOpnameStatus.Completed, selesai.Status);
        Assert.Empty(await MutasiOpnameAsync(h));
    }

    [Fact]
    public async Task Posting_mengikuti_aturan_saldo_tidak_di_bawah_yang_ditahan_dan_tidak_menyimpan_apa_pun()
    {
        using var h = await SiapkanAsync();
        var saldo = await SaldoAsync(h, await BatchAsync(h, "OPN-R"), h.GudangId, DrugStockStatus.Available, 10m,
            ditahan: 8m);
        var diajukan = await DiajukanAsync(h, (saldo, 5m));

        var galat = await Assert.ThrowsAsync<DrugStockUnprocessableException>(() =>
            Layanan(h, h.AtasanUserId).ApproveAsync(diajukan.Id, Putusan(diajukan)));
        Assert.Equal("PHM029", galat.Code);

        Assert.Equal((10m, 1), await SaldoTersimpanAsync(h, saldo));
        Assert.Empty(await MutasiOpnameAsync(h));
        var tetap = (await Layanan(h, h.AtasanUserId).GetDetailAsync(diajukan.Id))!;
        Assert.Equal(StockOpnameStatus.Submitted, tetap.Status);
        Assert.Null(Assert.Single(tetap.Lines).AdjustmentMutationId);
    }

    // ------------------------------------------------------------------- temuan dan berita acara

    [Fact]
    public async Task Temuan_tanpa_saldo_dicatat_tidak_menambah_stok_dan_masuk_berita_acara()
    {
        using var h = await SiapkanAsync();
        var batch = await BatchAsync(h, "OPN-F");
        var saldo = await SaldoAsync(h, batch, h.GudangId, DrugStockStatus.Available, 10m);
        var sesi = await Layanan(h, h.StafUserId).CreateAsync(Buat(h.GudangId));

        var temuan = new AddStockOpnameFindingRequest
        {
            ExpectedVersion = sesi.Version, DrugId = h.ObatId, BatchNumber = "TANPA-SALDO-1",
            StockStatus = DrugStockStatus.Available, PhysicalQuantity = 3m, Note = "ditemukan di rak belakang"
        };
        sesi = await Layanan(h, h.StafUserId).AddFindingAsync(sesi.Id, temuan);
        var barisTemuan = Assert.Single(sesi.Lines, x => x.LineType == StockOpnameLineType.Finding);
        Assert.Equal(StockOpnameVarianceResults.Finding, barisTemuan.Result);
        Assert.Null(barisTemuan.DrugStockBalanceId);
        Assert.Equal(1, sesi.FindingCount);

        foreach (var salah in new[]
                 {
                     new AddStockOpnameFindingRequest
                     {
                         ExpectedVersion = sesi.Version, DrugId = h.ObatId, BatchNumber = "TANPA-SALDO-1",
                         StockStatus = DrugStockStatus.Available, PhysicalQuantity = 1m
                     },
                     new AddStockOpnameFindingRequest
                     {
                         ExpectedVersion = sesi.Version, DrugId = h.ObatId, DrugBatchId = batch,
                         StockStatus = DrugStockStatus.Available, PhysicalQuantity = 1m
                     },
                     new AddStockOpnameFindingRequest
                     {
                         ExpectedVersion = sesi.Version, DrugId = h.ObatId, StockStatus = DrugStockStatus.Available,
                         PhysicalQuantity = 0m
                     },
                     new AddStockOpnameFindingRequest
                     {
                         ExpectedVersion = sesi.Version, DrugId = Guid.NewGuid(), StockStatus = DrugStockStatus.Available,
                         PhysicalQuantity = 1m
                     }
                 })
        {
            var galat = await Assert.ThrowsAsync<StockOpnameUnprocessableException>(() =>
                Layanan(h, h.StafUserId).AddFindingAsync(sesi.Id, salah));
            Assert.Equal("PHM059", galat.Code);
        }

        // Berita acara belum ada sebelum sesi selesai.
        var belum = await Assert.ThrowsAsync<StockOpnameConflictException>(() =>
            Layanan(h, h.StafUserId).GetReportAsync(sesi.Id));
        Assert.Equal("PHM052", belum.Code);

        sesi = await Layanan(h, h.StafUserId).SaveCountsAsync(sesi.Id, Hitung(sesi, (Baris(sesi, saldo), 9m)));
        var diajukan = await Layanan(h, h.StafUserId).SubmitAsync(sesi.Id, Versi(sesi));
        var selesai = await Layanan(h, h.AtasanUserId).ApproveAsync(diajukan.Id, Putusan(diajukan, "Sesuai hasil hitung."));

        // Hanya selisih baris saldo yang diposting; temuan tidak.
        var mutasi = Assert.Single(await MutasiOpnameAsync(h));
        Assert.Equal(-1m, mutasi.QuantityChange);
        Assert.Equal(9m, (await SaldoTersimpanAsync(h, saldo)).Saldo);
        Assert.Null(selesai.Lines.Single(x => x.LineType == StockOpnameLineType.Finding).AdjustmentMutationId);

        var berita = await Layanan(h, h.StafUserId).GetReportAsync(selesai.Id);
        Assert.Equal(selesai.OpnameNumber, berita.DocumentNumber);
        Assert.Equal(h.AtasanUserId, berita.ApprovedByUserId);
        Assert.Equal(1, berita.AdjustmentCount);
        Assert.Single(berita.DifferenceLines);
        Assert.Equal("TANPA-SALDO-1", Assert.Single(berita.Findings).BatchNumber);
        Assert.Single(berita.CountedByNames);
        Assert.Equal("Sesuai hasil hitung.", berita.DecisionReason);
    }

    [Fact]
    public async Task Temuan_dapat_dihapus_selama_dihitung()
    {
        using var h = await SiapkanAsync();
        var sesi = await Layanan(h, h.StafUserId).CreateAsync(Buat(h.GudangId));
        sesi = await Layanan(h, h.StafUserId).AddFindingAsync(sesi.Id, new AddStockOpnameFindingRequest
        {
            ExpectedVersion = sesi.Version, DrugId = h.ObatId, StockStatus = DrugStockStatus.Damaged, PhysicalQuantity = 2m
        });
        var temuan = Assert.Single(sesi.Lines);

        var dihapus = await Layanan(h, h.StafUserId).RemoveFindingAsync(sesi.Id, temuan.Id, sesi.Version);
        Assert.Empty(dihapus.Lines);
        Assert.Equal(0, dihapus.FindingCount);
    }

    // ------------------------------------------------------------------- BLOCKED_SCHEMA dan kontrak

    [Fact]
    public async Task Tanpa_tabel_Stock_Opname_layanan_dan_endpoint_menjawab_BLOCKED_SCHEMA()
    {
        using var h = await SiapkanAsync();
        Assert.True((await Layanan(h, h.StafUserId).GetAvailabilityAsync()).IsPersistenceReady);

        using (var k = h.CreateContext())
        {
            // Meniru basis data yang belum dimigrasi.
            await k.Database.ExecuteSqlRawAsync("DROP TABLE \"PhmStockOpnameLine\";");
            await k.Database.ExecuteSqlRawAsync("DROP TABLE \"PhmStockOpname\";");
        }

        var kesiapan = await Layanan(h, h.StafUserId).GetAvailabilityAsync();
        Assert.False(kesiapan.IsPersistenceReady);
        Assert.Equal("BLOCKED_SCHEMA", kesiapan.Status);

        var galat = await Assert.ThrowsAsync<StockOpnameSchemaUnavailableException>(() =>
            Layanan(h, h.StafUserId).GetListAsync(new StockOpnameListQuery()));
        Assert.Equal("PHM039", galat.Code);
        await Assert.ThrowsAsync<StockOpnameSchemaUnavailableException>(() =>
            Layanan(h, h.StafUserId).CreateAsync(Buat(h.GudangId)));

        var controller = new StockOpnameController(Layanan(h, h.StafUserId));
        var jawaban = Assert.IsType<ObjectResult>(await controller.GetList(new StockOpnameListQuery(), CancellationToken.None));
        Assert.Equal(503, jawaban.StatusCode);
        var isi = JsonSerializer.Serialize(jawaban.Value);
        Assert.Contains("PHM039", isi);
        Assert.Contains("BLOCKED_SCHEMA", isi);
    }

    [Fact]
    public void Endpoint_Stock_Opname_dijaga_login_dan_izin_StockOpname()
    {
        var controller = typeof(StockOpnameController);
        Assert.NotNull(controller.GetCustomAttribute<AuthorizeAttribute>());

        var aksi = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => x.GetCustomAttributes<HttpMethodAttribute>().Any())
            .ToList();
        Assert.Equal(13, aksi.Count);

        foreach (var metode in aksi)
        {
            var http = metode.GetCustomAttributes<HttpMethodAttribute>().SelectMany(x => x.HttpMethods).Single();
            var izin = Assert.Single(metode.GetCustomAttributes<AccessPermissionAttribute>());
            Assert.Equal("StockOpname", izin.Arguments![0] as string);
            var tindakan = izin.Arguments[1] as string;
            Assert.Equal(tindakan, metode.GetCustomAttribute<AccessActionAttribute>()!.ActionName);

            var harapan = http == "GET"
                ? "Read"
                : metode.Name is nameof(StockOpnameController.Approve) or nameof(StockOpnameController.Reject)
                    ? "Approve"
                    : "Create";
            Assert.Equal(harapan, tindakan);
        }
    }
}
