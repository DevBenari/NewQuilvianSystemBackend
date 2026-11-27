using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using static QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests.Perintah;

namespace QuilvianSystemBackend.Tests.Pharmacy.LogisticsReportTests;

/// <summary>
/// Laporan Logistik berbasis jumlah (FIN-LGS-024) di atas data uji sintetis pada SQLite dalam
/// memori: Persediaan, Mutasi, Dashboard Penerimaan &amp; Pengeluaran, Distribusi, Pemakaian per Unit.
/// </summary>
public class LogisticsReportTests
{
    private static readonly DateTime SejamLalu = DateTime.UtcNow.AddHours(-1);

    // ------------------------------------------------------------------- data uji

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

    private static async Task SaldoAsync(StockRequestApprovalHarness h, Guid batch, Guid lokasi,
        DrugStockStatus status, decimal fisik, decimal ditahan = 0m)
    {
        using var k = h.CreateContext();
        k.PhmDrugStockBalances.Add(new PhmDrugStockBalance
        {
            Id = Guid.NewGuid(), DrugId = h.ObatId, DrugBatchId = batch, StorageLocationId = lokasi,
            Status = status, QuantityOnHand = fisik, QuantityReserved = ditahan, CreateDateTime = DateTime.UtcNow
        });
        await k.SaveChangesAsync();
    }

    private static async Task MutasiAsync(StockRequestApprovalHarness h, Guid batch, DrugStockMutationType jenis,
        decimal perubahan, DateTime pada, string? sumber = null, DrugStockStatus status = DrugStockStatus.Available)
    {
        using var k = h.CreateContext();
        k.PhmDrugStockMutations.Add(new PhmDrugStockMutation
        {
            Id = Guid.NewGuid(), DrugId = h.ObatId, DrugBatchId = batch, StorageLocationId = h.GudangId,
            Status = status, MutationType = jenis, QuantityChange = perubahan, BalanceBefore = 0m,
            BalanceAfter = perubahan, SourceDocumentType = sumber, ActorUserId = h.StafUserId, OccurredAt = pada,
            Source = "UJI:laporan", CreateDateTime = DateTime.UtcNow
        });
        await k.SaveChangesAsync();
    }

    /// <summary>
    /// Saldo pembuka +50, masuk +20 (transfer), keluar −15 (permintaan), penyesuaian −2, perubahan
    /// status −5/+5, dan satu barang masuk 40 hari lalu (di luar periode bawaan).
    /// </summary>
    private static async Task<Guid> MutasiStandarAsync(StockRequestApprovalHarness h)
    {
        var batch = await BatchAsync(h, "UJI-MUT-1", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)));
        await MutasiAsync(h, batch, DrugStockMutationType.OpeningBalance, 50m, SejamLalu);
        await MutasiAsync(h, batch, DrugStockMutationType.StockIn, 20m, SejamLalu, DrugStockSourceDocumentTypes.StockTransfer);
        await MutasiAsync(h, batch, DrugStockMutationType.StockOut, -15m, SejamLalu, DrugStockSourceDocumentTypes.StockRequest);
        await MutasiAsync(h, batch, DrugStockMutationType.Adjustment, -2m, SejamLalu, DrugStockSourceDocumentTypes.ManualAdjustment);
        await MutasiAsync(h, batch, DrugStockMutationType.StatusChange, -5m, SejamLalu);
        await MutasiAsync(h, batch, DrugStockMutationType.StatusChange, 5m, SejamLalu, status: DrugStockStatus.Quarantine);
        await MutasiAsync(h, batch, DrugStockMutationType.StockIn, 99m, DateTime.UtcNow.AddDays(-40));
        await SaldoAsync(h, batch, h.GudangId, DrugStockStatus.Available, 48m);
        await SaldoAsync(h, batch, h.GudangId, DrugStockStatus.Quarantine, 5m);
        return batch;
    }

    // ---------------------------------------------------------- Laporan Persediaan

    [Fact]
    public async Task Laporan_persediaan_memisahkan_saldo_per_status_untuk_satu_obat_dan_lokasi()
    {
        using var h = new StockRequestApprovalHarness();
        await h.SediakanMasterObatAsync();
        var kedaluwarsaDekat = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(200));
        var batchA = await BatchAsync(h, "UJI-INV-A", kedaluwarsaDekat);
        var batchB = await BatchAsync(h, "UJI-INV-B", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));
        await SaldoAsync(h, batchA, h.GudangId, DrugStockStatus.Available, 100m, ditahan: 10m);
        await SaldoAsync(h, batchA, h.GudangId, DrugStockStatus.Quarantine, 5m);
        await SaldoAsync(h, batchB, h.GudangId, DrugStockStatus.Damaged, 2m);

        Guid lokasiLain;
        using (var k = h.CreateContext())
        {
            lokasiLain = Guid.NewGuid();
            k.MstDrugStorageLocations.Add(new MstDrugStorageLocation
            {
                Id = lokasiLain, StorageLocationCode = "UJI-GUD-LAIN", StorageLocationName = "Gudang Lain (Uji)",
                IsActive = true, CreateDateTime = DateTime.UtcNow
            });
            await k.SaveChangesAsync();
        }
        await SaldoAsync(h, batchA, lokasiLain, DrugStockStatus.Available, 0m);

        var laporan = await new LogisticsReportQueries(h.CreateContext()).GetInventoryAsync(new InventoryReportQuery());

        var baris = Assert.Single(laporan.Rows.Items);
        Assert.Equal("Obat Uji Satu", baris.DrugName);
        Assert.Equal("Gudang Induk (Uji)", baris.StorageLocationName);
        Assert.Equal(100m, baris.AvailableOnHand);
        Assert.Equal(10m, baris.Reserved);
        Assert.Equal(90m, baris.AvailableToIssue);
        Assert.Equal(5m, baris.Quarantine);
        Assert.Equal(2m, baris.Damaged);
        Assert.Equal(0m, baris.Expired);
        Assert.Equal(107m, baris.TotalOnHand);
        Assert.Equal(2, baris.BatchCount);
        // Kedaluwarsa terdekat hanya dari batch siap pakai; batch rusak tidak dihitung.
        Assert.Equal(kedaluwarsaDekat, baris.NearestExpiryDate);

        Assert.Equal(1, laporan.Summary.RowCount);
        Assert.Equal(1, laporan.Summary.RowsWithQuarantine);
        Assert.Equal(1, laporan.Summary.RowsWithDamaged);
        Assert.Equal(0, laporan.Summary.RowsWithoutAvailable);
        Assert.Contains(laporan.Limitations, x => x.Contains("Nilai persediaan"));

        var termasukKosong = await new LogisticsReportQueries(h.CreateContext())
            .GetInventoryAsync(new InventoryReportQuery { IncludeEmpty = true });
        Assert.Equal(2, termasukKosong.Summary.RowCount);

        var cariKosong = await new LogisticsReportQueries(h.CreateContext())
            .GetInventoryAsync(new InventoryReportQuery { Search = "tidak-ada-obat-ini" });
        Assert.Equal(0, cariKosong.Rows.TotalData);
    }

    // ------------------------------------------------------------- Laporan Mutasi

    [Fact]
    public async Task Laporan_mutasi_merekap_masuk_keluar_dan_penyesuaian_tanpa_perubahan_status()
    {
        using var h = new StockRequestApprovalHarness();
        await h.SediakanMasterObatAsync();
        await MutasiStandarAsync(h);

        var laporan = await new LogisticsReportQueries(h.CreateContext()).GetMutationsAsync(new MutationReportQuery());

        Assert.Equal(LogisticsReportPeriod.DefaultDays, laporan.Period.DayCount);
        var baris = Assert.Single(laporan.Rows.Items);
        Assert.Equal(50m, baris.OpeningRecorded);
        Assert.Equal(20m, baris.QuantityIn);
        Assert.Equal(15m, baris.QuantityOut);
        Assert.Equal(-2m, baris.AdjustmentNet);
        Assert.Equal(53m, baris.NetChange);
        Assert.Equal(4, baris.MutationCount);
        Assert.Equal(53m, baris.CurrentOnHand);

        Assert.Equal(6, laporan.Summary.MutationCount);
        Assert.Equal(2, laporan.Summary.StatusChangeCount);
        Assert.Equal(1, laporan.Summary.InCount);
        Assert.Equal(1, laporan.Summary.OutCount);
        Assert.Contains(laporan.Limitations, x => x.Contains("Saldo Saat Ini"));
    }

    [Fact]
    public async Task Periode_laporan_yang_tidak_sah_ditolak_dengan_PHM035()
    {
        using var h = new StockRequestApprovalHarness();
        var laporan = new LogisticsReportQueries(h.CreateContext());

        var terbalik = await Assert.ThrowsAsync<LogisticsReportUnprocessableException>(() =>
            laporan.GetMutationsAsync(new MutationReportQuery
            {
                StartDate = new DateTime(2026, 11, 10), EndDate = new DateTime(2026, 11, 1)
            }));
        Assert.Equal("PHM035", terbalik.Code);

        var terlaluPanjang = await Assert.ThrowsAsync<LogisticsReportUnprocessableException>(() =>
            laporan.GetStockFlowAsync(new StockFlowSummaryQuery
            {
                StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2026, 3, 1)
            }));
        Assert.Equal("PHM035", terlaluPanjang.Code);

        // Lewat controller: 422.
        var hasil = await new StockRequestController(h.Layanan(h.CreateContext(), h.StafUserId))
            .GetDistributionReport(new DistributionReportQuery
            {
                StartDate = new DateTime(2026, 11, 10), EndDate = new DateTime(2026, 11, 1)
            }, CancellationToken.None);
        Assert.Equal(422, Assert.IsType<ObjectResult>(hasil).StatusCode);
    }

    // ------------------------------------------- Dashboard Penerimaan & Pengeluaran

    [Fact]
    public async Task Dashboard_menghitung_transaksi_per_sumber_dan_hari_tanpa_mengarang_penerimaan_pengadaan()
    {
        using var h = new StockRequestApprovalHarness();
        await h.SediakanMasterObatAsync();
        await MutasiStandarAsync(h);

        var dashboard = await new LogisticsReportQueries(h.CreateContext()).GetStockFlowAsync(new StockFlowSummaryQuery());

        Assert.Equal(1, dashboard.Totals.InMutationCount);
        Assert.Equal(1, dashboard.Totals.OutMutationCount);
        Assert.Equal(1, dashboard.Totals.AdjustmentCount);
        Assert.Equal(1, dashboard.Totals.OpeningCount);
        Assert.Equal(2, dashboard.Totals.StatusChangeCount);

        Assert.Contains(dashboard.BySource, x => x.Direction == StockFlowDirections.In &&
            x.SourceDocumentType == DrugStockSourceDocumentTypes.StockTransfer && x.MutationCount == 1);
        Assert.Contains(dashboard.BySource, x => x.Direction == StockFlowDirections.Out &&
            x.SourceDocumentType == DrugStockSourceDocumentTypes.StockRequest && x.MutationCount == 1);

        Assert.Equal(dashboard.Period.DayCount, dashboard.Daily.Count);
        Assert.Equal(1, dashboard.Daily.Sum(x => x.InCount));
        Assert.Equal(1, dashboard.Daily.Sum(x => x.OutCount));

        Assert.Equal(15m, Assert.Single(dashboard.TopIssuedDrugs).Quantity);
        Assert.Equal(20m, Assert.Single(dashboard.TopReceivedDrugs).Quantity);
        Assert.False(dashboard.HasProcurementReceipts);
        Assert.Contains(dashboard.Limitations, x => x.Contains("Goods Receipt"));
    }

    // ---------------------------------------------------------- Laporan Distribusi

    [Fact]
    public async Task Laporan_distribusi_merekap_penyerahan_per_unit_dan_obat_termasuk_kekurangan()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, itemId) = await h.PermintaanAsync(unitId: h.UnitId);
        var disetujui = await h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(id, Setujui("uji-lap-setuju"));
        await h.Layanan(h.CreateContext(), h.StafUserId)
            .FulfillAsync(id, Serahkan(itemId, disetujui.Version, "uji-lap-serah", jumlah: 6m));
        await h.PermintaanAsync(unitId: h.UnitId); // masih menunggu: bukan distribusi

        var laporan = await h.Layanan(h.CreateContext(), h.StafUserId)
            .GetDistributionReportAsync(new DistributionReportQuery());

        var baris = Assert.Single(laporan.Rows.Items);
        Assert.Equal("Ruang Melati (Uji)", baris.RequestingServiceUnitName);
        Assert.Equal("Obat Uji Satu", baris.DrugName);
        Assert.Equal(h.SatuanId, baris.MeasurementId);
        Assert.Equal(1, baris.RequestCount);
        Assert.Equal(StockRequestApprovalHarness.JumlahDiminta, baris.RequestedQuantity);
        Assert.Equal(6m, baris.FulfilledQuantity);
        Assert.Equal(4m, baris.ShortfallQuantity);
        Assert.Equal(1, baris.ShortfallLineCount);
        Assert.Equal(1, laporan.Summary.CompletedRequestCount);
        Assert.Equal(1, laporan.Summary.PartiallyFulfilledRequestCount);

        // Cakupan sama dengan daftar permintaan (FIN-LGS-070): tanpa izin persetujuan kosong;
        // pemegang izin melihat seluruh unit, termasuk yang bukan unit penempatan HRD-nya.
        var tanpaIzin = await h.Layanan(h.CreateContext(), h.StafUserId)
            .GetDistributionReportAsync(new DistributionReportQuery { Scope = StockRequestListScope.MyAuthority });
        Assert.Equal(0, tanpaIzin.Rows.TotalData);
        var penyetujuUnitLain = await h.Layanan(h.CreateContext(), h.AtasanLainUserId)
            .GetDistributionReportAsync(new DistributionReportQuery { Scope = StockRequestListScope.MyAuthority });
        Assert.Equal(1, penyetujuUnitLain.Rows.TotalData);
    }

    // --------------------------------------------------- Laporan Pemakaian per Unit

    [Fact]
    public async Task Laporan_pemakaian_per_unit_hanya_menghitung_pemakaian_yang_sudah_dicatat()
    {
        using var h = new StockRequestApprovalHarness();
        var kunjunganA = Guid.NewGuid();
        var kunjunganB = Guid.NewGuid();
        var nomor = 0;

        using (var k = h.CreateContext())
        {
            k.RegPatientEncounters.AddRange(
                new RegPatientEncounter
                {
                    Id = kunjunganA, EncounterNumber = "UJI-KJ-A", PatientId = Guid.NewGuid(),
                    ServiceUnitId = h.UnitId, CreateDateTime = DateTime.UtcNow
                },
                new RegPatientEncounter
                {
                    Id = kunjunganB, EncounterNumber = "UJI-KJ-B", PatientId = Guid.NewGuid(),
                    ServiceUnitId = h.UnitLainId, CreateDateTime = DateTime.UtcNow
                });

            void Pemakaian(Guid kunjungan, DrugUsageStatus status, decimal jumlah)
            {
                var usageId = Guid.NewGuid();
                k.PhmDrugUsages.Add(new PhmDrugUsage
                {
                    Id = usageId, UsageNumber = $"UJI-PMK-{++nomor}", EncounterId = kunjungan,
                    StorageLocationId = h.GudangId, RecordedByWorkforceId = h.PemohonWorkforceId,
                    Status = status, UsedAt = SejamLalu, ItemCount = 1, CreateDateTime = DateTime.UtcNow
                });
                k.PhmDrugUsageItems.Add(new PhmDrugUsageItem
                {
                    Id = Guid.NewGuid(), DrugUsageId = usageId, DrugId = h.ObatId, MeasurementId = h.SatuanId,
                    DrugCodeSnapshot = "UJI-OBT-1", DrugNameSnapshot = "Obat Uji Satu", Quantity = jumlah,
                    LineNumber = 1, CreateDateTime = DateTime.UtcNow
                });
            }

            Pemakaian(kunjunganA, DrugUsageStatus.NotBilled, 3m);
            Pemakaian(kunjunganA, DrugUsageStatus.Billed, 2m);
            Pemakaian(kunjunganA, DrugUsageStatus.Draft, 7m);
            Pemakaian(kunjunganA, DrugUsageStatus.Cancelled, 9m);
            Pemakaian(kunjunganB, DrugUsageStatus.NotBilled, 4m);
            await k.SaveChangesAsync();
        }

        var laporan = await new LogisticsReportQueries(h.CreateContext()).GetUsageByUnitAsync(new UsageByUnitReportQuery());

        Assert.Equal(2, laporan.Rows.TotalData);
        var unitA = Assert.Single(laporan.Rows.Items, x => x.ServiceUnitId == h.UnitId);
        Assert.Equal("Ruang Melati (Uji)", unitA.ServiceUnitName);
        Assert.Equal(5m, unitA.Quantity);
        Assert.Equal(2, unitA.UsageDocumentCount);
        Assert.Equal(1, unitA.EncounterCount);
        Assert.Equal(4m, Assert.Single(laporan.Rows.Items, x => x.ServiceUnitId == h.UnitLainId).Quantity);
        Assert.Equal(3, laporan.Summary.UsageDocumentCount);
        Assert.Equal(2, laporan.Summary.UnitCount);

        var hanyaA = await new LogisticsReportQueries(h.CreateContext())
            .GetUsageByUnitAsync(new UsageByUnitReportQuery { ServiceUnitId = h.UnitId });
        Assert.Equal(h.UnitId, Assert.Single(hanyaA.Rows.Items).ServiceUnitId);
    }

    // ------------------------------------------- Pemeriksaan kelengkapan buku mutasi

    [Fact]
    public async Task Pemeriksaan_buku_mutasi_menandai_saldo_yang_tidak_dapat_direkonstruksi()
    {
        using var h = new StockRequestApprovalHarness();
        await h.SediakanMasterObatAsync();
        var kedaluwarsa = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));

        // Cocok: saldo pembuka 50, keluar 15, saldo tersimpan 35.
        var cocok = await BatchAsync(h, "UJI-LED-1", kedaluwarsa);
        await MutasiAsync(h, cocok, DrugStockMutationType.OpeningBalance, 50m, SejamLalu);
        await MutasiAsync(h, cocok, DrugStockMutationType.StockOut, -15m, SejamLalu, DrugStockSourceDocumentTypes.StockRequest);
        await SaldoAsync(h, cocok, h.GudangId, DrugStockStatus.Available, 35m);

        // Selisih: hanya masuk 20 yang tercatat, saldo tersimpan 25.
        var selisih = await BatchAsync(h, "UJI-LED-2", kedaluwarsa);
        await MutasiAsync(h, selisih, DrugStockMutationType.StockIn, 20m, SejamLalu, DrugStockSourceDocumentTypes.StockTransfer);
        await SaldoAsync(h, selisih, h.GudangId, DrugStockStatus.Available, 25m);

        // Tanpa mutasi: saldo 10 yang tidak pernah dicatat di buku mutasi.
        var tanpaMutasi = await BatchAsync(h, "UJI-LED-3", kedaluwarsa);
        await SaldoAsync(h, tanpaMutasi, h.GudangId, DrugStockStatus.Available, 10m);

        // Tanpa saldo: mutasi karantina +5 tanpa baris saldo karantina.
        await MutasiAsync(h, cocok, DrugStockMutationType.StatusChange, 5m, SejamLalu, status: DrugStockStatus.Quarantine);

        var semua = await new LogisticsReportQueries(h.CreateContext())
            .GetLedgerConsistencyAsync(new LedgerConsistencyQuery { StorageLocationId = h.GudangId, OnlyInconsistent = false });

        Assert.Equal("Gudang Induk (Uji)", semua.StorageLocationName);
        Assert.Equal(4, semua.Summary.CheckedKeyCount);
        Assert.Equal(1, semua.Summary.MatchCount);
        Assert.Equal(1, semua.Summary.SumMismatchCount);
        Assert.Equal(1, semua.Summary.NoMutationCount);
        Assert.Equal(1, semua.Summary.NoBalanceCount);
        Assert.Equal(4, semua.Summary.MutationCount);
        Assert.False(semua.Summary.IsConsistent);

        var barisSelisih = Assert.Single(semua.Rows.Items, x => x.Finding == LedgerConsistencyFindings.SumMismatch);
        Assert.Equal("UJI-LED-2", barisSelisih.BatchNumber);
        Assert.Equal(25m, barisSelisih.QuantityOnHand);
        Assert.Equal(20m, barisSelisih.LedgerQuantity);
        Assert.Equal(5m, barisSelisih.Difference);

        var barisCocok = Assert.Single(semua.Rows.Items, x => x.Finding == LedgerConsistencyFindings.Match);
        Assert.Equal(35m, barisCocok.LedgerQuantity);
        Assert.Equal(2, barisCocok.MutationCount);

        var barisTanpaSaldo = Assert.Single(semua.Rows.Items, x => x.Finding == LedgerConsistencyFindings.NoBalance);
        Assert.Equal(DrugStockStatus.Quarantine, barisTanpaSaldo.Status);
        Assert.Equal(-5m, barisTanpaSaldo.Difference);

        // Bawaan: hanya yang tidak cocok, dan tidak ada saldo yang diubah.
        var masalah = await new LogisticsReportQueries(h.CreateContext())
            .GetLedgerConsistencyAsync(new LedgerConsistencyQuery { StorageLocationId = h.GudangId });
        Assert.Equal(3, masalah.Rows.TotalData);
        Assert.DoesNotContain(masalah.Rows.Items, x => x.Finding == LedgerConsistencyFindings.Match);
        // Saldo 35 + 25 + 10 dan empat mutasi tetap sama sesudah pemeriksaan.
        Assert.Equal((70m, 4), await h.PotretStokAsync());
    }

    [Fact]
    public async Task Pemeriksaan_buku_mutasi_mewajibkan_lokasi_terdaftar_dan_memisahkan_lokasi_serta_obat()
    {
        using var h = new StockRequestApprovalHarness();
        await h.SediakanMasterObatAsync();

        var tanpaLokasi = await Assert.ThrowsAsync<LogisticsReportUnprocessableException>(() =>
            new LogisticsReportQueries(h.CreateContext()).GetLedgerConsistencyAsync(new LedgerConsistencyQuery()));
        Assert.Equal(LogisticsReportQueries.LedgerLocationRequiredCode, tanpaLokasi.Code);

        // Lokasi yang tidak terdaftar tidak boleh dilaporkan "konsisten" dengan nol baris.
        var tidakTerdaftar = await Assert.ThrowsAsync<LogisticsReportUnprocessableException>(() =>
            new LogisticsReportQueries(h.CreateContext())
                .GetLedgerConsistencyAsync(new LedgerConsistencyQuery { StorageLocationId = Guid.NewGuid() }));
        Assert.Equal(LogisticsReportQueries.LedgerLocationRequiredCode, tidakTerdaftar.Code);

        // Saldo di gudang uji cocok; saldo tanpa mutasi di lokasi lain tidak ikut diperiksa.
        var batch = await BatchAsync(h, "UJI-LED-4", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)));
        await MutasiAsync(h, batch, DrugStockMutationType.OpeningBalance, 12m, SejamLalu);
        await SaldoAsync(h, batch, h.GudangId, DrugStockStatus.Available, 12m);

        Guid lokasiLain;
        using (var k = h.CreateContext())
        {
            lokasiLain = Guid.NewGuid();
            k.MstDrugStorageLocations.Add(new MstDrugStorageLocation
            {
                Id = lokasiLain, StorageLocationCode = "UJI-GUD-LED", StorageLocationName = "Gudang Periksa (Uji)",
                IsActive = true, CreateDateTime = DateTime.UtcNow
            });
            await k.SaveChangesAsync();
        }
        await SaldoAsync(h, batch, lokasiLain, DrugStockStatus.Available, 7m);

        var hasil = await new LogisticsReportQueries(h.CreateContext())
            .GetLedgerConsistencyAsync(new LedgerConsistencyQuery { StorageLocationId = h.GudangId });
        Assert.True(hasil.Summary.IsConsistent);
        Assert.Equal(1, hasil.Summary.CheckedKeyCount);
        Assert.Equal(0, hasil.Rows.TotalData);

        var lain = await new LogisticsReportQueries(h.CreateContext())
            .GetLedgerConsistencyAsync(new LedgerConsistencyQuery { StorageLocationId = lokasiLain });
        Assert.Equal(LedgerConsistencyFindings.NoMutation, Assert.Single(lain.Rows.Items).Finding);

        // Mutasi yang obatnya tidak sesuai dengan batch tampil sebagai baris tersendiri, tidak
        // tercampur ke saldo obat yang benar.
        var obatLain = Guid.NewGuid();
        using (var k = h.CreateContext())
        {
            k.PhmDrugStockMutations.Add(new PhmDrugStockMutation
            {
                Id = Guid.NewGuid(), DrugId = obatLain, DrugBatchId = batch, StorageLocationId = h.GudangId,
                Status = DrugStockStatus.Available, MutationType = DrugStockMutationType.StockIn, QuantityChange = 3m,
                BalanceBefore = 0m, BalanceAfter = 3m, ActorUserId = h.StafUserId, OccurredAt = SejamLalu,
                Source = "UJI:laporan", CreateDateTime = DateTime.UtcNow
            });
            await k.SaveChangesAsync();
        }

        var campur = await new LogisticsReportQueries(h.CreateContext())
            .GetLedgerConsistencyAsync(new LedgerConsistencyQuery { StorageLocationId = h.GudangId, OnlyInconsistent = false });
        Assert.Equal(2, campur.Summary.CheckedKeyCount);
        Assert.Equal(12m, Assert.Single(campur.Rows.Items, x => x.DrugId == h.ObatId).LedgerQuantity);
        var anomali = Assert.Single(campur.Rows.Items, x => x.DrugId == obatLain);
        Assert.Equal(LedgerConsistencyFindings.NoBalance, anomali.Finding);
        Assert.Equal(3m, anomali.LedgerQuantity);
    }

    [Fact]
    public async Task Pemeriksaan_buku_mutasi_lewat_controller_dijaga_DrugStock_Read_dan_menjawab_422_PHM036()
    {
        var metode = typeof(DrugStockController).GetMethod(nameof(DrugStockController.GetLedgerConsistency))!;
        var izin = Assert.Single(metode.GetCustomAttributes<AccessPermissionAttribute>());
        Assert.NotNull(izin.Arguments);
        Assert.Equal("DrugStock", izin.Arguments[0] as string);
        Assert.Equal("Read", izin.Arguments[1] as string);
        Assert.Equal("reports/ledger-consistency", metode.GetCustomAttribute<HttpGetAttribute>()!.Template);
        Assert.NotNull(typeof(DrugStockController).GetCustomAttribute<AuthorizeAttribute>());

        using var h = new StockRequestApprovalHarness();
        var accessor = new HttpContextAccessor();
        var controller = new DrugStockController(new DrugStockService(h.CreateContext(), accessor,
            new LoggerService(NullLogger<LoggerService>.Instance, accessor)));

        foreach (var lokasi in new Guid?[] { null, Guid.Empty, Guid.NewGuid() })
        {
            var hasil = await controller.GetLedgerConsistency(
                new LedgerConsistencyQuery { StorageLocationId = lokasi }, CancellationToken.None);
            var objek = Assert.IsType<ObjectResult>(hasil);
            Assert.Equal(422, objek.StatusCode);
            Assert.Contains("PHM036", JsonSerializer.Serialize(objek.Value));
        }
    }

    // ------------------------------------------------- penyempurnaan stok (FIN-LGS-081)

    [Fact]
    public async Task Stok_per_unit_menyaring_lokasi_milik_unit_langsung_dan_lewat_induknya()
    {
        using var h = new StockRequestApprovalHarness();
        // Daftar saldo diurutkan menurut nama obat (JOIN wajib ke master obat), jadi obatnya harus ada.
        await h.SediakanMasterObatAsync();
        var depoUnit = await LokasiAsync(h, "UJI-DEPO-A", unitLayanan: h.UnitId);
        var rakUnit = await LokasiAsync(h, "UJI-DEPO-A-RAK", induk: depoUnit);
        var depoLain = await LokasiAsync(h, "UJI-DEPO-B", unitLayanan: h.UnitLainId);
        // Lokasi yang menunjuk unitnya sendiri tidak ikut unit induknya.
        var rakMilikLain = await LokasiAsync(h, "UJI-DEPO-A-RAK-B", unitLayanan: h.UnitLainId, induk: depoUnit);
        var batch = await BatchAsync(h, "UJI-UNIT-1", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)));
        await SaldoAsync(h, batch, depoUnit, DrugStockStatus.Available, 10m);
        await SaldoAsync(h, batch, rakUnit, DrugStockStatus.Available, 7m);
        await SaldoAsync(h, batch, depoLain, DrugStockStatus.Available, 5m);
        await SaldoAsync(h, batch, rakMilikLain, DrugStockStatus.Available, 3m);
        await SaldoAsync(h, batch, h.GudangId, DrugStockStatus.Available, 100m);

        var layanan = LayananStok(h);

        var unitA = await layanan.GetBalancesAsync(new DrugStockBalanceQuery { ServiceUnitId = h.UnitId, PageSize = 50 });
        Assert.Equal(2, unitA.TotalData);
        Assert.Equal(new HashSet<Guid> { depoUnit, rakUnit }, unitA.Items.Select(x => x.StorageLocationId).ToHashSet());

        var unitB = await layanan.GetBalancesAsync(new DrugStockBalanceQuery { ServiceUnitId = h.UnitLainId, PageSize = 50 });
        Assert.Equal(new HashSet<Guid> { depoLain, rakMilikLain }, unitB.Items.Select(x => x.StorageLocationId).ToHashSet());

        // Tanpa saringan unit hasilnya tidak berubah; unit tanpa lokasi menghasilkan daftar kosong.
        var semua = await layanan.GetBalancesAsync(new DrugStockBalanceQuery { PageSize = 50 });
        Assert.Equal(5, semua.TotalData);
        var tanpaLokasi = await layanan.GetBalancesAsync(new DrugStockBalanceQuery { ServiceUnitId = h.UnitTanpaTautanId });
        Assert.Equal(0, tanpaLokasi.TotalData);
    }

    [Fact]
    public async Task Detail_permintaan_menampilkan_ketersediaan_per_baris_dengan_aturan_penyerahan()
    {
        using var h = new StockRequestApprovalHarness();
        await h.SediakanMasterObatAsync();
        await SatuanStokObatAsync(h);
        var depoLain = await LokasiAsync(h, "UJI-DEPO-LAIN");
        var setahun = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));

        // Dapat diserahkan: siap pakai, belum kedaluwarsa, 9 − 3 ditahan = 6.
        var layak = await BatchAsync(h, "UJI-AV-1", setahun);
        await SaldoAsync(h, layak, h.GudangId, DrugStockStatus.Available, 9m, ditahan: 3m);
        await SaldoAsync(h, layak, h.GudangId, DrugStockStatus.Quarantine, 40m);
        await SaldoAsync(h, layak, depoLain, DrugStockStatus.Available, 70m);
        // Tidak dapat diserahkan: sudah lewat kedaluwarsa walau statusnya masih siap pakai, dan
        // batch yang seluruhnya ditahan.
        var lewat = await BatchAsync(h, "UJI-AV-2", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));
        await SaldoAsync(h, lewat, h.GudangId, DrugStockStatus.Available, 50m);
        var ditahanSemua = await BatchAsync(h, "UJI-AV-3", setahun.AddYears(1));
        await SaldoAsync(h, ditahanSemua, h.GudangId, DrugStockStatus.Available, 10m, ditahan: 10m);

        var (disetujui, itemId) = await h.PermintaanAsync(status: StockRequestStatus.Approved);
        var (menunggu, _) = await h.PermintaanAsync();

        var detail = await h.Layanan(h.CreateContext(), h.StafUserId).GetDetailAsync(disetujui);
        Assert.True(detail!.CanFulfill);
        var baris = Assert.Single(detail.Items);
        Assert.Equal((decimal?)6m, baris.AvailableQuantity);
        Assert.Equal("Tablet (Uji)", baris.StockUnitName);

        // Belum dapat diserahkan: tidak dihitung.
        var belum = await h.Layanan(h.CreateContext(), h.StafUserId).GetDetailAsync(menunggu);
        Assert.False(belum!.CanFulfill);
        Assert.Null(Assert.Single(belum.Items).AvailableQuantity);

        // Angka itu hanya informasi: penyerahan 10 tetap ditolak backend, penyerahan 6 diterima.
        var galat = await Assert.ThrowsAsync<StockRequestUnprocessableException>(() =>
            h.Layanan(h.CreateContext(), h.StafUserId, denganStok: true)
                .FulfillAsync(disetujui, Serahkan(itemId, Versi, "uji-av-serah-10")));
        Assert.Equal("PHM028", galat.Code);

        var selesai = await h.Layanan(h.CreateContext(), h.StafUserId, denganStok: true)
            .FulfillAsync(disetujui, Serahkan(itemId, Versi, "uji-av-serah-6", jumlah: 6m));
        Assert.Equal(StockRequestStatus.Completed, selesai.Status);
        Assert.Equal((decimal?)6m, Assert.Single(selesai.Items).FulfilledQuantity);
        // Sesudah selesai tidak ada lagi yang dapat diserahkan, jadi ketersediaan tidak dihitung.
        Assert.Null(Assert.Single(selesai.Items).AvailableQuantity);
    }

    // ------------------------------------------- bantuan data uji tiga fitur (FIN-LGS-055–057)

    private static DrugStockService LayananStok(StockRequestApprovalHarness h, params Guid[] lokasiTerverifikasi)
    {
        var accessor = new HttpContextAccessor();
        var konfigurasi = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [LogisticsLedgerVerification.ConfigKey] = string.Join(",", lokasiTerverifikasi)
            })
            .Build();
        return new DrugStockService(h.CreateContext(), accessor,
            new LoggerService(NullLogger<LoggerService>.Instance, accessor), konfigurasi);
    }

    private static async Task<Guid> LokasiAsync(StockRequestApprovalHarness h, string kode, bool gudangInduk = false,
        Guid? unitLayanan = null, bool aktif = true, Guid? induk = null)
    {
        using var k = h.CreateContext();
        var id = Guid.NewGuid();
        k.MstDrugStorageLocations.Add(new MstDrugStorageLocation
        {
            Id = id, StorageLocationCode = kode, StorageLocationName = $"Lokasi {kode}", IsMainWarehouse = gudangInduk,
            ServiceUnitId = unitLayanan, ParentStorageLocationId = induk, IsActive = aktif,
            CreateDateTime = DateTime.UtcNow
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

    private static async Task KebijakanAsync(StockRequestApprovalHarness h, Guid? lokasi, decimal titikPesanUlang,
        decimal maksimum, Guid? satuan = null, bool aktif = true, DateTime? berakhir = null)
    {
        using var k = h.CreateContext();
        var id = Guid.NewGuid();
        k.MstDrugStockPolicies.Add(new MstDrugStockPolicy
        {
            Id = id, DrugId = h.ObatId, StorageLocationId = lokasi, StockUnitMeasurementId = satuan ?? h.SatuanId,
            StockPolicyCode = $"UJI-KBJ-{id.ToString("N")[..8]}", StockPolicyName = "Kebijakan Uji",
            ReorderPointQuantity = titikPesanUlang, MaximumStockQuantity = maksimum, IsActive = aktif,
            EffectiveEndDate = berakhir, CreateDateTime = DateTime.UtcNow
        });
        await k.SaveChangesAsync();
    }

    // ------------------------------------------------------- Riwayat Mutasi (FIN-LGS-055)

    [Fact]
    public async Task Riwayat_mutasi_menggabungkan_transfer_dan_retur_dari_kartu_stok_tanpa_sumber_lain()
    {
        using var h = new StockRequestApprovalHarness();
        await h.SediakanMasterObatAsync();
        var batch = await BatchAsync(h, "UJI-RWY-1", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)));
        await MutasiAsync(h, batch, DrugStockMutationType.StockOut, -4m, SejamLalu, DrugStockSourceDocumentTypes.StockTransfer);
        await MutasiAsync(h, batch, DrugStockMutationType.StockIn, 2m, SejamLalu, DrugStockSourceDocumentTypes.PrescriptionReturn);
        await MutasiAsync(h, batch, DrugStockMutationType.StockOut, -6m, SejamLalu, DrugStockSourceDocumentTypes.StockRequest);
        await MutasiAsync(h, batch, DrugStockMutationType.StockOut, -1m, SejamLalu, DrugStockSourceDocumentTypes.DrugUsage);

        var gabungan = await LayananStok(h).GetMutationsAsync(new DrugStockMutationQuery
        {
            SourceDocumentTypes = "StockTransfer,PrescriptionReturn"
        });
        Assert.Equal(2, gabungan.TotalData);
        Assert.Equal(
            new HashSet<string> { DrugStockSourceDocumentTypes.StockTransfer, DrugStockSourceDocumentTypes.PrescriptionReturn },
            gabungan.Items.Select(x => x.SourceDocumentType!).ToHashSet());

        // Huruf besar/kecil dan spasi dinormalkan; nilai yang tidak dikenal tidak melebarkan hasil.
        var dinormalkan = await LayananStok(h).GetMutationsAsync(new DrugStockMutationQuery
        {
            SourceDocumentTypes = " stocktransfer , TidakDikenal "
        });
        Assert.Equal(DrugStockSourceDocumentTypes.StockTransfer, Assert.Single(dinormalkan.Items).SourceDocumentType);

        var tidakDikenal = await LayananStok(h).GetMutationsAsync(new DrugStockMutationQuery
        {
            SourceDocumentTypes = "TidakDikenal"
        });
        Assert.Equal(0, tidakDikenal.TotalData);

        // Saringan tunggal (pilihan di layar) dan daftar bawaan harus sama-sama terpenuhi.
        var hanyaRetur = await LayananStok(h).GetMutationsAsync(new DrugStockMutationQuery
        {
            SourceDocumentTypes = "StockTransfer,PrescriptionReturn",
            SourceDocumentType = DrugStockSourceDocumentTypes.PrescriptionReturn
        });
        Assert.Equal(2m, Assert.Single(hanyaRetur.Items).QuantityChange);

        // Tanpa saringan baru, kartu stok tetap memuat semua sumber.
        Assert.Equal(4, (await LayananStok(h).GetMutationsAsync(new DrugStockMutationQuery())).TotalData);
    }

    // ---------------------------------------------- Mutasi Saldo Persediaan (FIN-LGS-056)

    [Fact]
    public async Task Mutasi_saldo_menghitung_saldo_awal_mundur_dan_saldo_akhir_untuk_buku_yang_cocok()
    {
        using var h = new StockRequestApprovalHarness();
        await h.SediakanMasterObatAsync();
        var batch = await BatchAsync(h, "UJI-MSP-1", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)));
        var empatPuluhHariLalu = DateTime.UtcNow.AddDays(-40);

        // Saldo pembuka 50 sebelum periode bawaan; dalam periode: masuk 20, keluar 15, penyesuaian −2,
        // dan 5 dipindah ke karantina. Saldo tersimpan: siap pakai 48, karantina 5.
        await MutasiAsync(h, batch, DrugStockMutationType.OpeningBalance, 50m, empatPuluhHariLalu);
        await MutasiAsync(h, batch, DrugStockMutationType.StockIn, 20m, SejamLalu, DrugStockSourceDocumentTypes.StockTransfer);
        await MutasiAsync(h, batch, DrugStockMutationType.StockOut, -15m, SejamLalu, DrugStockSourceDocumentTypes.StockRequest);
        await MutasiAsync(h, batch, DrugStockMutationType.Adjustment, -2m, SejamLalu, DrugStockSourceDocumentTypes.ManualAdjustment);
        await MutasiAsync(h, batch, DrugStockMutationType.StatusChange, -5m, SejamLalu);
        await MutasiAsync(h, batch, DrugStockMutationType.StatusChange, 5m, SejamLalu, status: DrugStockStatus.Quarantine);
        await SaldoAsync(h, batch, h.GudangId, DrugStockStatus.Available, 48m);
        await SaldoAsync(h, batch, h.GudangId, DrugStockStatus.Quarantine, 5m);

        var valid = await LayananStok(h, h.GudangId).GetBalanceMovementAsync(
            new BalanceMovementQuery { StorageLocationId = h.GudangId });

        Assert.True(valid.IsLocationVerified);
        Assert.True(valid.Summary.IsLedgerConsistent);
        var baris = Assert.Single(valid.Rows.Items);
        Assert.Equal(BalanceMovementStatuses.Valid, baris.Status);
        Assert.Equal((decimal?)50m, baris.OpeningBalance);
        Assert.Equal(20m, baris.QuantityIn);
        Assert.Equal(15m, baris.QuantityOut);
        Assert.Equal(-2m, baris.AdjustmentNet);
        Assert.Equal((decimal?)53m, baris.ClosingBalance);
        Assert.Equal(53m, baris.CurrentOnHand);
        // Perubahan status tidak menjadi masuk/keluar, hanya dihitung.
        Assert.Equal(2, baris.StatusChangeCount);
        var rincian = Assert.Single(baris.Batches);
        Assert.Equal("UJI-MSP-1", rincian.BatchNumber);
        Assert.Equal((decimal?)50m, rincian.OpeningBalance);
        Assert.Equal((decimal?)53m, rincian.ClosingBalance);

        // Periode lampau: saldo awal dihitung mundur melewati mutasi sesudah periode.
        var lampau = await LayananStok(h, h.GudangId).GetBalanceMovementAsync(new BalanceMovementQuery
        {
            StorageLocationId = h.GudangId,
            StartDate = DateTime.Now.AddDays(-45),
            EndDate = DateTime.Now.AddDays(-35)
        });
        var barisLampau = Assert.Single(lampau.Rows.Items);
        Assert.Equal((decimal?)0m, barisLampau.OpeningBalance);
        Assert.Equal(50m, barisLampau.OpeningRecorded);
        Assert.Equal((decimal?)50m, barisLampau.ClosingBalance);

        // Tanpa bukti UAT untuk lokasi ini: angka ada, tetapi tidak dinyatakan valid.
        var belum = await LayananStok(h).GetBalanceMovementAsync(new BalanceMovementQuery { StorageLocationId = h.GudangId });
        Assert.False(belum.IsLocationVerified);
        Assert.Equal(BalanceMovementStatuses.Unverified, Assert.Single(belum.Rows.Items).Status);
        Assert.Contains(belum.Limitations, x => x.Contains(LogisticsLedgerVerification.ConfigKey));
    }

    [Fact]
    public async Task Mutasi_saldo_tidak_menghitung_saldo_historis_untuk_buku_yang_tidak_cocok()
    {
        using var h = new StockRequestApprovalHarness();
        await h.SediakanMasterObatAsync();
        var kedaluwarsa = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));

        // Batch A: saldo 10 tanpa satu pun mutasi. Batch B: masuk 3 tercatat, saldo tersimpan 5.
        var batchA = await BatchAsync(h, "UJI-MSP-A", kedaluwarsa);
        await SaldoAsync(h, batchA, h.GudangId, DrugStockStatus.Available, 10m);
        var batchB = await BatchAsync(h, "UJI-MSP-B", kedaluwarsa);
        await MutasiAsync(h, batchB, DrugStockMutationType.StockIn, 3m, SejamLalu, DrugStockSourceDocumentTypes.StockTransfer);
        await SaldoAsync(h, batchB, h.GudangId, DrugStockStatus.Available, 5m);

        // Walaupun lokasi sudah diverifikasi, buku yang tidak cocok tetap tidak dihitung.
        var hasil = await LayananStok(h, h.GudangId).GetBalanceMovementAsync(
            new BalanceMovementQuery { StorageLocationId = h.GudangId });

        Assert.False(hasil.Summary.IsLedgerConsistent);
        var baris = Assert.Single(hasil.Rows.Items);
        Assert.Equal(BalanceMovementStatuses.NotComputable, baris.Status);
        Assert.Null(baris.OpeningBalance);
        Assert.Null(baris.ClosingBalance);
        Assert.Equal(3m, baris.QuantityIn);
        Assert.Equal(15m, baris.CurrentOnHand);
        Assert.Equal(2, baris.InconsistentKeyCount);
        Assert.All(baris.Batches, x =>
        {
            Assert.Equal(BalanceMovementStatuses.NotComputable, x.Status);
            Assert.Null(x.OpeningBalance);
        });
        Assert.Equal(1, hasil.Summary.NotComputableRowCount);

        var tanpaLokasi = await Assert.ThrowsAsync<LogisticsReportUnprocessableException>(() =>
            LayananStok(h).GetBalanceMovementAsync(new BalanceMovementQuery()));
        Assert.Equal("PHM036", tanpaLokasi.Code);

        var periodePanjang = await Assert.ThrowsAsync<LogisticsReportUnprocessableException>(() =>
            LayananStok(h).GetBalanceMovementAsync(new BalanceMovementQuery
            {
                StorageLocationId = h.GudangId,
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2026, 3, 1)
            }));
        Assert.Equal("PHM035", periodePanjang.Code);
    }

    // ------------------------------------------ Rekomendasi Pengisian Ulang (FIN-LGS-057)

    [Fact]
    public async Task Rekomendasi_pengisian_ulang_memakai_maksimum_dikurangi_siap_pakai_dibulatkan_ke_atas()
    {
        using var h = new StockRequestApprovalHarness();
        await h.SediakanMasterObatAsync();
        await SatuanStokObatAsync(h);
        var batch = await BatchAsync(h, "UJI-RPU-1", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)));

        // Depo unit (dipenuhi dari gudang internal): siap pakai 12,5 (2 ditahan) ≤ titik pesan ulang 20.
        var depo = await LokasiAsync(h, "UJI-DEPO", unitLayanan: h.UnitId);
        await SaldoAsync(h, batch, depo, DrugStockStatus.Available, 12.5m, ditahan: 2m);
        await KebijakanAsync(h, depo, titikPesanUlang: 20m, maksimum: 100m);

        // Gudang induk: siap pakai 5 ≤ 10; diisi lewat pengadaan, tanpa tombol permintaan.
        var induk = await LokasiAsync(h, "UJI-INDUK", gudangInduk: true);
        await SaldoAsync(h, batch, induk, DrugStockStatus.Available, 5m);
        await KebijakanAsync(h, induk, titikPesanUlang: 10m, maksimum: 50m);

        // Masih di atas titik pesan ulang: tidak ditampilkan.
        var aman = await LokasiAsync(h, "UJI-AMAN");
        await SaldoAsync(h, batch, aman, DrugStockStatus.Available, 30m);
        await KebijakanAsync(h, aman, titikPesanUlang: 20m, maksimum: 60m);

        var hasil = await LayananStok(h).GetReplenishmentAsync(new ReplenishmentQuery());

        Assert.Equal(2, hasil.Rows.TotalData);
        Assert.Equal(2, hasil.Summary.RecommendedCount);
        Assert.Equal(1, hasil.Summary.AboveReorderPointCount);
        Assert.Equal(3, hasil.Summary.ActivePolicyCount);

        var barisDepo = Assert.Single(hasil.Rows.Items, x => x.StorageLocationId == depo);
        Assert.Equal(ReplenishmentStatuses.Recommended, barisDepo.Status);
        Assert.Equal(12.5m, barisDepo.QuantityAvailable);
        Assert.Equal((decimal?)88m, barisDepo.RecommendedQuantity); // 100 − 12,5 = 87,5 → 88
        Assert.True(barisDepo.CanCreateRequest);
        Assert.Equal((Guid?)h.UnitId, barisDepo.RequestingServiceUnitId);
        Assert.Equal("Ruang Melati (Uji)", barisDepo.RequestingServiceUnitName);
        Assert.Equal((Guid?)h.SatuanId, barisDepo.StockUnitMeasurementId);

        var barisInduk = Assert.Single(hasil.Rows.Items, x => x.StorageLocationId == induk);
        Assert.Equal((decimal?)45m, barisInduk.RecommendedQuantity);
        Assert.False(barisInduk.CanCreateRequest);
        Assert.True(barisInduk.IsMainWarehouse);

        // Tidak ada permintaan yang dibuat.
        using var k = h.CreateContext();
        Assert.Equal(0, await k.PhmStockRequests.CountAsync());
    }

    [Fact]
    public async Task Rekomendasi_pengisian_ulang_tidak_mengarang_angka_untuk_kebijakan_tidak_lengkap_atau_satuan_keliru()
    {
        using var h = new StockRequestApprovalHarness();
        await h.SediakanMasterObatAsync();
        await SatuanStokObatAsync(h);

        // Satu kebijakan hidup per obat × lokasi, sesuai indeks unik PostgreSQL (DrugId, StorageLocationId).
        var nonaktif = await LokasiAsync(h, "UJI-RPU-N", aktif: false);
        await KebijakanAsync(h, await LokasiAsync(h, "UJI-RPU-1"), titikPesanUlang: 0m, maksimum: 50m);   // titik pesan ulang belum diisi
        await KebijakanAsync(h, await LokasiAsync(h, "UJI-RPU-2"), titikPesanUlang: 20m, maksimum: 20m);  // maksimum tidak di atas titik
        await KebijakanAsync(h, null, titikPesanUlang: 10m, maksimum: 40m);                               // tanpa lokasi
        await KebijakanAsync(h, nonaktif, titikPesanUlang: 10m, maksimum: 40m);                           // lokasi nonaktif
        await KebijakanAsync(h, await LokasiAsync(h, "UJI-RPU-3"), titikPesanUlang: 10m, maksimum: 40m,
            satuan: Guid.NewGuid());                                                                      // satuan keliru
        await KebijakanAsync(h, await LokasiAsync(h, "UJI-RPU-4"), titikPesanUlang: 10m, maksimum: 40m,
            aktif: false);                                                                                // tidak dibaca
        await KebijakanAsync(h, await LokasiAsync(h, "UJI-RPU-5"), titikPesanUlang: 10m, maksimum: 40m,
            berakhir: DateTime.UtcNow.AddDays(-1));                                                       // berakhir

        var hasil = await LayananStok(h).GetReplenishmentAsync(new ReplenishmentQuery());

        Assert.Equal(5, hasil.Summary.ActivePolicyCount);
        Assert.Equal(4, hasil.Summary.PolicyIncompleteCount);
        Assert.Equal(1, hasil.Summary.UnitMismatchCount);
        Assert.Equal(0, hasil.Summary.RecommendedCount);
        Assert.All(hasil.Rows.Items, x =>
        {
            Assert.Null(x.RecommendedQuantity);
            Assert.False(x.CanCreateRequest);
        });
        Assert.Contains(hasil.Rows.Items, x => x.IncompleteReasons.Contains(ReplenishmentIncompleteReasons.ReorderPointNotSet));
        Assert.Contains(hasil.Rows.Items, x => x.IncompleteReasons.Contains(ReplenishmentIncompleteReasons.MaximumNotAboveReorderPoint));
        Assert.Contains(hasil.Rows.Items, x => x.IncompleteReasons.Contains(ReplenishmentIncompleteReasons.NoLocation));
        Assert.Contains(hasil.Rows.Items, x => x.IncompleteReasons.Contains(ReplenishmentIncompleteReasons.LocationInactive));

        var satuanKeliru = await LayananStok(h).GetReplenishmentAsync(
            new ReplenishmentQuery { Status = ReplenishmentStatuses.UnitMismatch });
        Assert.Equal(ReplenishmentStatuses.UnitMismatch, Assert.Single(satuanKeliru.Rows.Items).Status);

        // Ketiga endpoint baru dijaga izin baca stok yang sudah ada.
        foreach (var nama in new[] { nameof(DrugStockController.GetReplenishment), nameof(DrugStockController.GetBalanceMovement) })
        {
            var izin = Assert.Single(typeof(DrugStockController).GetMethod(nama)!.GetCustomAttributes<AccessPermissionAttribute>());
            Assert.NotNull(izin.Arguments);
            Assert.Equal("DrugStock", izin.Arguments[0] as string);
            Assert.Equal("Read", izin.Arguments[1] as string);
        }
    }
}
