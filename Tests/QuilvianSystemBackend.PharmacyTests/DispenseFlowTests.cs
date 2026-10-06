using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Penyerahan obat yang sebenarnya: stok berkurang, resep berpindah tahap, fakta keluar.
/// </summary>
/// <remarks>
/// <para>
/// Inilah satu-satunya titik pada modul Farmasi yang tidak dapat dibatalkan — obat yang sudah di
/// tangan pasien tidak dapat ditarik kembali, dan stok yang sudah berkurang dua kali tidak dapat
/// dibedakan dari obat yang benar-benar hilang. Karena itu yang dijaga di sini bukan hanya
/// hasilnya, melainkan bahwa efeknya terjadi <b>tepat satu kali</b>.
/// </para>
/// <para>
/// Seluruh uji di berkas ini memakai container DI sungguhan, karena penyerahan menerbitkan fakta
/// klinis ke Billing lewat rantai yang membuka scope-nya sendiri.
/// </para>
/// </remarks>
public class DispenseFlowTests
{
    private static PreparePrescriptionDispensingRequest Penyiapan(PharmacyHarness h,
        decimal jumlah) => new()
        {
            StorageLocationId = h.DepoId,
            PreparedByWorkforceId = h.PetugasId,
            Items =
            [
                new PrescriptionDispensingItemInput
                {
                    PrescriptionItemId = h.ItemResepId,
                    Quantity = jumlah
                }
            ],
            IdempotencyKey = Guid.NewGuid().ToString("N")
        };

    private static PrescriptionDispensingCommandRequest Perintah(int versi = 0,
        string? kunci = null) => new()
        {
            ExpectedVersion = versi,
            IdempotencyKey = kunci ?? Guid.NewGuid().ToString("N")
        };

    /// <summary>Resep siap diserahkan, berstok, dengan satu penyiapan penyerahan berstatus draft.</summary>
    private static async Task<(PrescriptionDispensingService Layanan,
        QuilvianSystemBackend.Repositories.ApplicationDbContext Konteks, Guid UsageId)>
        SiapAsync(PharmacyHarness h, decimal jumlah)
    {
        var (layanan, konteks) = h.Penyerahan();
        await h.SampaiSiapDiserahkanAsync(konteks);
        await h.SediakanStokAsync(konteks);

        await layanan.PrepareAsync(h.ResepId, Penyiapan(h, jumlah));

        var usage = konteks.PhmDrugUsages
            .Where(x => x.PrescriptionId == h.ResepId && !x.IsDelete)
            .OrderByDescending(x => x.CreateDateTime)
            .First();

        return (layanan, konteks, usage.Id);
    }

    // ------------------------------------------------------------- penyerahan penuh

    [Fact]
    public async Task Penyerahan_penuh_menandai_resep_sudah_diserahkan()
    {
        using var h = new PharmacyHarness();
        var (layanan, konteks, usageId) = await SiapAsync(h, 10);

        await layanan.DispenseAsync(h.ResepId, usageId, Perintah());

        var resep = await h.ResepAsync(konteks);
        Assert.Equal(PrescriptionFulfillmentStatus.Dispensed, resep.FulfillmentStatus);
    }

    [Fact]
    public async Task Penyerahan_penuh_mengurangi_stok_tepat_sejumlah_yang_diserahkan()
    {
        using var h = new PharmacyHarness();
        var (layanan, konteks, usageId) = await SiapAsync(h, 10);

        var sebelum = konteks.Set<PhmDrugStockBalance>()
            .Single(x => x.StorageLocationId == h.DepoId);
        var saldoAwal = sebelum.QuantityOnHand;

        await layanan.DispenseAsync(h.ResepId, usageId, Perintah());

        await konteks.Entry(sebelum).ReloadAsync();
        Assert.Equal(saldoAwal - 10, sebelum.QuantityOnHand);
    }

    [Fact]
    public async Task Tahanan_stok_dilepas_bersamaan_dengan_pengurangan_saldo()
    {
        // Tidak boleh ada keadaan antara yang menahan sekaligus sudah mengeluarkan: saldo yang
        // tertahan padahal obatnya sudah keluar akan membuat stok terbaca lebih kecil selamanya.
        using var h = new PharmacyHarness();
        var (layanan, konteks, usageId) = await SiapAsync(h, 10);

        var saldo = konteks.Set<PhmDrugStockBalance>()
            .Single(x => x.StorageLocationId == h.DepoId);
        await konteks.Entry(saldo).ReloadAsync();
        Assert.Equal(10, saldo.QuantityReserved);

        await layanan.DispenseAsync(h.ResepId, usageId, Perintah());

        await konteks.Entry(saldo).ReloadAsync();
        Assert.Equal(0, saldo.QuantityReserved);
    }

    [Fact]
    public async Task Penyerahan_mengubah_status_penyiapan_menjadi_belum_tertagih()
    {
        using var h = new PharmacyHarness();
        var (layanan, konteks, usageId) = await SiapAsync(h, 10);

        await layanan.DispenseAsync(h.ResepId, usageId, Perintah());

        var usage = await konteks.PhmDrugUsages.FindAsync(usageId);
        Assert.Equal(DrugUsageStatus.NotBilled, usage!.Status);
        Assert.NotNull(usage.RecordedAt);
    }

    // ------------------------------------------------------------- penyerahan sebagian

    [Fact]
    public async Task Penyerahan_sebagian_menandai_resep_diserahkan_sebagian()
    {
        // Resep uji berisi 10; menyerahkan 4 berarti pasien masih berhak atas 6.
        using var h = new PharmacyHarness();
        var (layanan, konteks, usageId) = await SiapAsync(h, 4);

        await layanan.DispenseAsync(h.ResepId, usageId, Perintah());

        var resep = await h.ResepAsync(konteks);
        Assert.Equal(PrescriptionFulfillmentStatus.PartiallyDispensed, resep.FulfillmentStatus);
    }

    [Fact]
    public async Task Sisa_yang_belum_diserahkan_tetap_tercatat_setelah_penyerahan_sebagian()
    {
        using var h = new PharmacyHarness();
        var (layanan, konteks, usageId) = await SiapAsync(h, 4);

        var ringkasan = await layanan.DispenseAsync(h.ResepId, usageId, Perintah());

        var baris = ringkasan.Items.Single(x => x.PrescriptionItemId == h.ItemResepId);
        Assert.Equal(10, baris.QuantityPrescribed);
        Assert.Equal(4, baris.QuantityDispensed);
    }

    [Fact]
    public async Task Sisa_resep_masih_dapat_diserahkan_pada_penyerahan_berikutnya()
    {
        // Dua penyerahan berurutan atas satu resep: sesudah yang kedua resep menjadi penuh.
        using var h = new PharmacyHarness();
        var (layanan, konteks, usagePertama) = await SiapAsync(h, 4);

        await layanan.DispenseAsync(h.ResepId, usagePertama, Perintah());
        Assert.Equal(PrescriptionFulfillmentStatus.PartiallyDispensed,
            (await h.ResepAsync(konteks)).FulfillmentStatus);

        await layanan.PrepareAsync(h.ResepId, Penyiapan(h, 6));
        var usageKedua = konteks.PhmDrugUsages
            .Where(x => x.PrescriptionId == h.ResepId && !x.IsDelete && x.Id != usagePertama)
            .OrderByDescending(x => x.CreateDateTime)
            .First().Id;

        await layanan.DispenseAsync(h.ResepId, usageKedua, Perintah());

        Assert.Equal(PrescriptionFulfillmentStatus.Dispensed,
            (await h.ResepAsync(konteks)).FulfillmentStatus);
    }

    [Fact]
    public async Task Jumlah_melebihi_sisa_setelah_penyerahan_sebagian_ditolak()
    {
        // `PHM104` dihitung terhadap SISA, bukan terhadap jumlah resep semula.
        using var h = new PharmacyHarness();
        var (layanan, konteks, usageId) = await SiapAsync(h, 4);

        await layanan.DispenseAsync(h.ResepId, usageId, Perintah());

        var galat = await Assert.ThrowsAsync<PrescriptionDispensingUnprocessableException>(() =>
            layanan.PrepareAsync(h.ResepId, Penyiapan(h, 7)));

        Assert.Equal("PHM104", galat.Code);
    }

    // ------------------------------------------------------------- idempotensi

    [Fact]
    public async Task Penyerahan_yang_diulang_tidak_mengurangi_stok_dua_kali()
    {
        // Inilah yang membuat tombol kirim ulang aman ditekan petugas yang ragu apakah
        // permintaannya sudah sampai.
        using var h = new PharmacyHarness();
        var (layanan, konteks, usageId) = await SiapAsync(h, 10);

        var saldo = konteks.Set<PhmDrugStockBalance>()
            .Single(x => x.StorageLocationId == h.DepoId);
        var saldoAwal = saldo.QuantityOnHand;

        await layanan.DispenseAsync(h.ResepId, usageId, Perintah());
        await layanan.DispenseAsync(h.ResepId, usageId, Perintah());

        await konteks.Entry(saldo).ReloadAsync();
        Assert.Equal(saldoAwal - 10, saldo.QuantityOnHand);
    }

    [Fact]
    public async Task Penyerahan_yang_diulang_tidak_mengubah_versi_penyiapan_lagi()
    {
        using var h = new PharmacyHarness();
        var (layanan, konteks, usageId) = await SiapAsync(h, 10);

        await layanan.DispenseAsync(h.ResepId, usageId, Perintah());
        var usage = await konteks.PhmDrugUsages.FindAsync(usageId);
        var versiSesudahPertama = usage!.Version;

        await layanan.DispenseAsync(h.ResepId, usageId, Perintah());
        await konteks.Entry(usage).ReloadAsync();

        Assert.Equal(versiSesudahPertama, usage.Version);
    }

    [Fact]
    public async Task Penyiapan_yang_diulang_dengan_kunci_sama_tidak_menahan_stok_dua_kali()
    {
        using var h = new PharmacyHarness();
        var (layanan, konteks) = h.Penyerahan();
        await h.SampaiSiapDiserahkanAsync(konteks);
        await h.SediakanStokAsync(konteks);

        var permintaan = Penyiapan(h, 10);
        await layanan.PrepareAsync(h.ResepId, permintaan);
        await layanan.PrepareAsync(h.ResepId, permintaan);

        var saldo = konteks.Set<PhmDrugStockBalance>()
            .Single(x => x.StorageLocationId == h.DepoId);
        await konteks.Entry(saldo).ReloadAsync();

        Assert.Equal(10, saldo.QuantityReserved);
        Assert.Single(konteks.PhmDrugUsages.Where(x => x.PrescriptionId == h.ResepId && !x.IsDelete));
    }

    [Fact]
    public async Task Versi_penyiapan_yang_tidak_cocok_ditolak()
    {
        // `PHM114`. Dua petugas yang membuka layar yang sama tidak boleh saling menimpa.
        using var h = new PharmacyHarness();
        var (layanan, _, usageId) = await SiapAsync(h, 10);

        var galat = await Assert.ThrowsAsync<PrescriptionDispensingConflictException>(() =>
            layanan.DispenseAsync(h.ResepId, usageId, Perintah(versi: 7)));

        Assert.Equal("PHM114", galat.Code);
    }

    // ------------------------------------------------------------- gerbang finansial

    [Fact]
    public async Task Izin_finansial_yang_dicabut_menahan_penyerahan_yang_sudah_disiapkan()
    {
        // Penyiapan sudah menahan stok, tetapi izinnya dicabut sebelum obat berpindah. Stok
        // tetap tertahan dan tidak berkurang — yang benar, karena obatnya belum keluar.
        using var h = new PharmacyHarness();
        var (layanan, konteks, usageId) = await SiapAsync(h, 10);

        await h.TerimaSuratAsync(konteks,
            status: PrescriptionClearanceProjectionStatuses.Revoked,
            hasil: null, versi: 2, alasan: "PAYMENT_REVERSED");

        var galat = await Assert.ThrowsAsync<PrescriptionDispensingConflictException>(() =>
            layanan.DispenseAsync(h.ResepId, usageId, Perintah()));

        Assert.StartsWith("PHA_CLR_", galat.Code);

        var saldo = konteks.Set<PhmDrugStockBalance>()
            .Single(x => x.StorageLocationId == h.DepoId);
        await konteks.Entry(saldo).ReloadAsync();
        Assert.Equal(100, saldo.QuantityOnHand);
        Assert.Equal(10, saldo.QuantityReserved);
    }

    [Fact]
    public async Task Resep_yang_dibatalkan_setelah_penyiapan_tidak_dapat_diserahkan()
    {
        using var h = new PharmacyHarness();
        var (layanan, konteks, usageId) = await SiapAsync(h, 10);

        var resep = await h.ResepAsync(konteks);
        resep.PrescriptionStatus = PrescriptionStatus.Cancelled;
        await konteks.SaveChangesAsync();

        var galat = await Assert.ThrowsAsync<PrescriptionDispensingConflictException>(() =>
            layanan.DispenseAsync(h.ResepId, usageId, Perintah()));

        Assert.Equal("PHM109", galat.Code);
    }

    // ------------------------------------------------------------- fakta klinis

    [Fact]
    public async Task Penyerahan_tanpa_fakta_tahap_satu_tetap_berhasil()
    {
        // `EmitDispensedChargeAsync` hanya MEREVISI fakta tagihan tahap 1 yang sudah ada. Resep
        // dari jalur lama yang tidak pernah menerbitkannya tidak mendapat tagihan baru dari
        // sini, dan ketiadaan fakta itu tidak boleh menggagalkan penyerahan obat.
        using var h = new PharmacyHarness();
        var (layanan, konteks, usageId) = await SiapAsync(h, 10);

        Assert.Empty(konteks.Set<CliClinicalMilestoneFact>());

        await layanan.DispenseAsync(h.ResepId, usageId, Perintah());

        Assert.Equal(PrescriptionFulfillmentStatus.Dispensed,
            (await h.ResepAsync(konteks)).FulfillmentStatus);
    }

    [Fact]
    public async Task Kegagalan_penyerahan_fakta_ke_billing_tidak_membatalkan_penyerahan_obat()
    {
        // Pola source: `EmitDispensedChargeAsync` dibungkus try/catch dan dipanggil SETELAH
        // penyerahan tersimpan. Obat yang sudah di tangan pasien tidak dapat ditarik kembali
        // hanya karena Billing sedang tidak dapat dihubungi.
        //
        // Dibuktikan dengan fakta tahap 1 yang ada tetapi tidak dapat direvisi — rantai
        // producer berjalan sungguhan lewat container DI, dan kegagalannya tertelan.
        using var h = new PharmacyHarness();
        var (layanan, konteks, usageId) = await SiapAsync(h, 10);

        konteks.Set<CliClinicalMilestoneFact>().Add(new CliClinicalMilestoneFact
        {
            Id = Guid.NewGuid(),
            SourceContext = "PRESCRIPTION",
            SourceAggregateId = h.ResepId,
            SourceItemId = null,
            EffectType = "PRESCRIPTION_CHARGE",
            MilestoneFactId = Guid.NewGuid(),
            MilestoneFactVersion = 1,
            MilestoneKind = ClinicalMilestoneKind.ChargeEligibility,
            EncounterId = h.KunjunganId,
            OccurredAt = DateTime.UtcNow,
            PayloadFingerprint = "uji",
            DispatchStatus = ClinicalFactDispatchStatus.Dispatched,
            CorrelationId = Guid.NewGuid(),
            ActorUserId = h.ApotekerId,
            IdempotencyKey = Guid.NewGuid().ToString("N"),
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });
        await konteks.SaveChangesAsync();

        await layanan.DispenseAsync(h.ResepId, usageId, Perintah());

        var resep = await h.ResepAsync(konteks);
        Assert.Equal(PrescriptionFulfillmentStatus.Dispensed, resep.FulfillmentStatus);

        var usage = await konteks.PhmDrugUsages.FindAsync(usageId);
        Assert.Equal(DrugUsageStatus.NotBilled, usage!.Status);
    }

    // ------------------------------------------------------------- data klinis

    [Fact]
    public async Task Penyerahan_tidak_mengubah_data_klinis_resep()
    {
        using var h = new PharmacyHarness();
        var (layanan, konteks, usageId) = await SiapAsync(h, 10);

        var sebelum = await h.ResepAsync(konteks);
        var nomor = sebelum.PrescriptionNumber;
        var item = sebelum.TotalItemCount;
        var harga = sebelum.TotalPrice;
        var dokter = sebelum.DoctorId;

        await layanan.DispenseAsync(h.ResepId, usageId, Perintah());

        var sesudah = await h.ResepAsync(konteks);
        Assert.Equal(nomor, sesudah.PrescriptionNumber);
        Assert.Equal(item, sesudah.TotalItemCount);
        Assert.Equal(harga, sesudah.TotalPrice);
        Assert.Equal(dokter, sesudah.DoctorId);
    }

    [Fact]
    public async Task Penyerahan_yang_tidak_ada_tidak_dapat_diserahkan()
    {
        using var h = new PharmacyHarness();
        var (layanan, konteks) = h.Penyerahan();
        await h.SampaiSiapDiserahkanAsync(konteks);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            layanan.DispenseAsync(h.ResepId, Guid.NewGuid(), Perintah()));
    }
}
