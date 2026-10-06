using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Etiket obat dan penahanan stok — dua hal yang dibaca petugas dan dapur stok apa adanya.
/// </summary>
/// <remarks>
/// Etiket adalah satu-satunya keterangan yang dibawa pasien pulang, jadi isinya harus berasal
/// dari resep dan bukan dari tebakan. Penahanan stok menentukan apakah obat yang sama dapat
/// dijanjikan dua kali kepada dua pasien.
/// </remarks>
public class LabelAndStockTests
{
    // ------------------------------------------------------------------ etiket

    [Fact]
    public async Task Etiket_resep_yang_tidak_ada_mengembalikan_null()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        Assert.Null(await new PrescriptionLabelService(k).GetAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Etiket_memuat_identitas_resep()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        var etiket = await new PrescriptionLabelService(k).GetAsync(h.ResepId);

        Assert.NotNull(etiket);
        Assert.Equal("UJI-RX-0001", etiket!.PrescriptionNumber);
    }

    [Fact]
    public async Task Etiket_memuat_baris_obat_resep()
    {
        // Etiket tanpa baris obat adalah kertas kosong; pasien tidak tahu apa yang diminumnya.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        var etiket = await new PrescriptionLabelService(k).GetAsync(h.ResepId);

        Assert.NotEmpty(etiket!.Items);
    }

    [Fact]
    public async Task Etiket_tetap_terbit_sebelum_obat_diserahkan()
    {
        // Etiket dicetak saat obat dikemas, bukan sesudah diserahkan — kalau ia menunggu
        // penyerahan, kemasan sampai ke pasien tanpa keterangan apa pun.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        Assert.Equal(PrescriptionFulfillmentStatus.WaitingForPayment,
            (await h.ResepAsync(k)).FulfillmentStatus);

        Assert.NotNull(await new PrescriptionLabelService(k).GetAsync(h.ResepId));
    }

    // ------------------------------------------------------------------ stok

    [Fact]
    public async Task Penahanan_menaikkan_jumlah_tertahan_tanpa_mengurangi_saldo()
    {
        // Menahan bukan mengeluarkan. Saldo fisik tetap, tetapi jumlah yang dapat dijanjikan
        // ke pasien lain berkurang.
        using var h = new PharmacyHarness();
        var (layanan, konteks, _) = await SiapkanStokAsync(h);

        var saldo = konteks.Set<PhmDrugStockBalance>().Single(x => x.StorageLocationId == h.DepoId);

        await layanan.ReserveBatchAsync(saldo.DrugBatchId, h.DepoId, 30);
        await konteks.SaveChangesAsync();
        await konteks.Entry(saldo).ReloadAsync();

        Assert.Equal(100, saldo.QuantityOnHand);
        Assert.Equal(30, saldo.QuantityReserved);
    }

    [Fact]
    public async Task Penahanan_melebihi_saldo_tersedia_ditolak()
    {
        // Tanpa penjagaan ini obat yang sama dijanjikan ke dua pasien, dan yang kedua baru tahu
        // ketika obatnya tidak ada.
        using var h = new PharmacyHarness();
        var (layanan, konteks, _) = await SiapkanStokAsync(h);

        var saldo = konteks.Set<PhmDrugStockBalance>().Single(x => x.StorageLocationId == h.DepoId);

        await Assert.ThrowsAsync<DrugStockUnprocessableException>(() =>
            layanan.ReserveBatchAsync(saldo.DrugBatchId, h.DepoId, 101));
    }

    [Fact]
    public async Task Penahanan_bertahap_terakumulasi_sampai_batas_saldo()
    {
        using var h = new PharmacyHarness();
        var (layanan, konteks, _) = await SiapkanStokAsync(h);

        var saldo = konteks.Set<PhmDrugStockBalance>().Single(x => x.StorageLocationId == h.DepoId);

        await layanan.ReserveBatchAsync(saldo.DrugBatchId, h.DepoId, 60);
        await konteks.SaveChangesAsync();
        await layanan.ReserveBatchAsync(saldo.DrugBatchId, h.DepoId, 40);
        await konteks.SaveChangesAsync();
        await konteks.Entry(saldo).ReloadAsync();
        Assert.Equal(100, saldo.QuantityReserved);

        // Satu pun lagi tidak boleh: seluruh saldo sudah dijanjikan.
        await Assert.ThrowsAsync<DrugStockUnprocessableException>(() =>
            layanan.ReserveBatchAsync(saldo.DrugBatchId, h.DepoId, 1));
    }

    [Fact]
    public async Task Pelepasan_tahanan_mengembalikan_jumlah_yang_dapat_dijanjikan()
    {
        using var h = new PharmacyHarness();
        var (layanan, konteks, _) = await SiapkanStokAsync(h);

        var saldo = konteks.Set<PhmDrugStockBalance>().Single(x => x.StorageLocationId == h.DepoId);

        await layanan.ReserveBatchAsync(saldo.DrugBatchId, h.DepoId, 30);
        await konteks.SaveChangesAsync();
        await layanan.ReleaseReservationAsync(saldo.DrugBatchId, h.DepoId, 30);
        await konteks.SaveChangesAsync();
        await konteks.Entry(saldo).ReloadAsync();

        Assert.Equal(0, saldo.QuantityReserved);
        Assert.Equal(100, saldo.QuantityOnHand);
    }

    [Fact]
    public async Task Rencana_fefo_mengambil_bets_kedaluwarsa_terdekat_lebih_dulu()
    {
        // First-Expired-First-Out. Mengambil bets yang lebih panjang umurnya lebih dulu membuat
        // bets lama berakhir kedaluwarsa di rak padahal masih dapat dipakai.
        using var h = new PharmacyHarness();
        var (layanan, konteks, _) = await SiapkanStokAsync(h);

        var lama = Guid.NewGuid();
        konteks.Set<PhmDrugBatch>().Add(new PhmDrugBatch
        {
            Id = lama,
            DrugId = h.ObatId,
            BatchNumber = "UJI-PHM-BATCH-LAMA",
            ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)),
            CreateDateTime = DateTime.UtcNow
        });
        konteks.Set<PhmDrugStockBalance>().Add(new PhmDrugStockBalance
        {
            Id = Guid.NewGuid(),
            DrugId = h.ObatId,
            DrugBatchId = lama,
            StorageLocationId = h.DepoId,
            Status = DrugStockStatus.Available,
            QuantityOnHand = 50,
            CreateDateTime = DateTime.UtcNow
        });
        await konteks.SaveChangesAsync();

        var rencana = await layanan.PlanFefoAsync(h.ObatId, h.DepoId, 10);

        Assert.Equal(lama, rencana[0].DrugBatchId);
    }

    [Fact]
    public async Task Rencana_fefo_memecah_permintaan_ke_beberapa_bets()
    {
        // Permintaan yang melebihi satu bets tidak ditolak; ia dipenuhi berurutan.
        using var h = new PharmacyHarness();
        var (layanan, konteks, _) = await SiapkanStokAsync(h);

        var kedua = Guid.NewGuid();
        konteks.Set<PhmDrugBatch>().Add(new PhmDrugBatch
        {
            Id = kedua,
            DrugId = h.ObatId,
            BatchNumber = "UJI-PHM-BATCH-2",
            ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2)),
            CreateDateTime = DateTime.UtcNow
        });
        konteks.Set<PhmDrugStockBalance>().Add(new PhmDrugStockBalance
        {
            Id = Guid.NewGuid(),
            DrugId = h.ObatId,
            DrugBatchId = kedua,
            StorageLocationId = h.DepoId,
            Status = DrugStockStatus.Available,
            QuantityOnHand = 50,
            CreateDateTime = DateTime.UtcNow
        });
        await konteks.SaveChangesAsync();

        var rencana = await layanan.PlanFefoAsync(h.ObatId, h.DepoId, 120);

        Assert.Equal(2, rencana.Count);
        Assert.Equal(120, rencana.Sum(x => x.Quantity));
    }

    [Fact]
    public async Task Rencana_fefo_menolak_permintaan_melebihi_seluruh_saldo()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, _) = await SiapkanStokAsync(h);

        await Assert.ThrowsAsync<DrugStockUnprocessableException>(() =>
            layanan.PlanFefoAsync(h.ObatId, h.DepoId, 500));
    }

    [Fact]
    public async Task Rencana_fefo_pada_depo_tanpa_stok_ditolak()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, _) = await SiapkanStokAsync(h);

        await Assert.ThrowsAsync<DrugStockUnprocessableException>(() =>
            layanan.PlanFefoAsync(h.ObatId, h.GudangTanpaPenyerahanId, 1));
    }

    [Fact]
    public async Task Pengeluaran_mengurangi_saldo_dan_melepas_tahanan_sekaligus()
    {
        using var h = new PharmacyHarness();
        var (layanan, konteks, _) = await SiapkanStokAsync(h);

        var saldo = konteks.Set<PhmDrugStockBalance>().Single(x => x.StorageLocationId == h.DepoId);

        await layanan.ReserveBatchAsync(saldo.DrugBatchId, h.DepoId, 25);
        await konteks.SaveChangesAsync();
        await layanan.IssueBatchAsync(saldo.DrugBatchId, h.DepoId, 25,
            consumeReservation: true, DrugStockSourceDocumentTypes.DrugUsage, Guid.NewGuid(),
            "Uji pengeluaran", $"uji-{Guid.NewGuid():N}");
        await konteks.SaveChangesAsync();

        await konteks.Entry(saldo).ReloadAsync();
        Assert.Equal(75, saldo.QuantityOnHand);
        Assert.Equal(0, saldo.QuantityReserved);
    }

    [Fact]
    public async Task Pengeluaran_melebihi_saldo_ditolak()
    {
        using var h = new PharmacyHarness();
        var (layanan, konteks, _) = await SiapkanStokAsync(h);

        var saldo = konteks.Set<PhmDrugStockBalance>().Single(x => x.StorageLocationId == h.DepoId);

        await Assert.ThrowsAsync<DrugStockUnprocessableException>(() =>
            layanan.IssueBatchAsync(saldo.DrugBatchId, h.DepoId, 200,
                consumeReservation: false, DrugStockSourceDocumentTypes.DrugUsage, Guid.NewGuid(),
                "Uji pengeluaran berlebih", $"uji-{Guid.NewGuid():N}"));
    }

    private static async Task<(DrugStockService Layanan,
        QuilvianSystemBackend.Repositories.ApplicationDbContext Konteks, Guid _)>
        SiapkanStokAsync(PharmacyHarness h)
    {
        var (_, konteks) = h.Penyerahan();
        await h.SediakanStokAsync(konteks);
        return (h.StokService(konteks), konteks, Guid.Empty);
    }
}
