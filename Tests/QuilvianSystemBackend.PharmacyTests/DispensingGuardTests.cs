using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Penjagaan sebelum obat berpindah ke pasien: penyiapan penyerahan dan pembatalannya.
/// </summary>
/// <remarks>
/// <para>
/// Penyerahan adalah titik yang tidak dapat dibatalkan — obat yang sudah di tangan pasien tidak
/// dapat ditarik kembali, dan jalan satu-satunya sesudahnya retur obat. Karena itu seluruh
/// penjagaan berada di depan: tahap resep, izin finansial, depo, petugas, dan jumlah.
/// </para>
/// <para>
/// <c>DispenseAsync</c> sendiri tidak diuji di sini: ia menerbitkan fakta klinis ke Billing dan
/// menuntut container DI yang sebenarnya. Dicatat sebagai bagian yang belum teruji.
/// </para>
/// </remarks>
public class DispensingGuardTests
{
    private static PreparePrescriptionDispensingRequest Penyiapan(PharmacyHarness h,
        decimal jumlah = 10, Guid? itemId = null, Guid? depo = null, Guid? petugas = null,
        string? kunci = null) => new()
        {
            StorageLocationId = depo ?? h.DepoId,
            PreparedByWorkforceId = petugas ?? h.PetugasId,
            Items =
            [
                new PrescriptionDispensingItemInput
                {
                    PrescriptionItemId = itemId ?? h.ItemResepId,
                    Quantity = jumlah
                }
            ],
            IdempotencyKey = kunci ?? Guid.NewGuid().ToString("N")
        };

    // ------------------------------------------------------------- gerbang tahap

    [Fact]
    public async Task Penjaga_tahap_diperiksa_lebih_dulu_daripada_izin_finansial()
    {
        // Urutannya `EnsureDispensable` lalu `EnsureFinancialClearanceAsync`. Resep tahap 2
        // karena itu ditolak dengan alasan tahapnya (`PHM110`), bukan alasan finansial —
        // pesannya memang yang paling berguna bagi petugas: obatnya belum disiapkan.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        var galat = await Assert.ThrowsAsync<PrescriptionDispensingConflictException>(() =>
            h.Penyerahan().Layanan.PrepareAsync(h.ResepId, Penyiapan(h)));

        Assert.Equal("PHM110", galat.Code);
    }

    [Fact]
    public async Task Izin_finansial_yang_dicabut_menahan_penyerahan_walau_tahapnya_siap()
    {
        // Gerbang finansial keempat, diuji pada satu-satunya keadaan yang dapat mencapainya:
        // resep sudah siap diserahkan, lalu izinnya dicabut Billing. Tahapnya sengaja tidak
        // dimundurkan — obat yang sudah diracik tidak dapat dibatalkan secara fisik — sehingga
        // penahanannya memang harus datang dari gerbang ini.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiSiapDiserahkanAsync(k);

        await h.TerimaSuratAsync(k,
            status: PrescriptionClearanceProjectionStatuses.Revoked,
            hasil: null, versi: 2, alasan: "PAYMENT_REVERSED");

        var galat = await Assert.ThrowsAsync<PrescriptionDispensingConflictException>(() =>
            h.Penyerahan().Layanan.PrepareAsync(h.ResepId, Penyiapan(h)));

        Assert.StartsWith("PHA_CLR_", galat.Code);
    }

    [Fact]
    public async Task Penyerahan_tidak_dapat_disiapkan_sebelum_penyiapan_farmasi_selesai()
    {
        // `PHM110`. Obat baru dapat diserahkan setelah diracik dan ditelaah akhir — resep yang
        // baru masuk antrean belum punya apa pun untuk diserahkan.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.TerimaSuratAsync(k);

        var galat = await Assert.ThrowsAsync<PrescriptionDispensingConflictException>(() =>
            h.Penyerahan().Layanan.PrepareAsync(h.ResepId, Penyiapan(h)));

        Assert.Equal("PHM110", galat.Code);
    }

    [Fact]
    public async Task Resep_yang_sudah_dibatalkan_tidak_dapat_diserahkan()
    {
        // `PHM109`.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiSiapDiserahkanAsync(k);

        var resep = await h.ResepAsync(k);
        resep.PrescriptionStatus = PrescriptionStatus.Cancelled;
        await k.SaveChangesAsync();

        var galat = await Assert.ThrowsAsync<PrescriptionDispensingConflictException>(() =>
            h.Penyerahan().Layanan.PrepareAsync(h.ResepId, Penyiapan(h)));

        Assert.Equal("PHM109", galat.Code);
    }

    [Fact]
    public async Task Resep_yang_tidak_ada_tidak_dapat_diserahkan()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            h.Penyerahan().Layanan.PrepareAsync(Guid.NewGuid(), Penyiapan(h)));
    }

    // ------------------------------------------------------------- depo & petugas

    [Fact]
    public async Task Depo_yang_tidak_ada_ditolak()
    {
        // `PHM112`.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiSiapDiserahkanAsync(k);

        var galat = await Assert.ThrowsAsync<PrescriptionDispensingUnprocessableException>(() =>
            h.Penyerahan().Layanan.PrepareAsync(h.ResepId, Penyiapan(h, depo: Guid.NewGuid())));

        Assert.Equal("PHM112", galat.Code);
    }

    [Fact]
    public async Task Depo_yang_tidak_aktif_ditolak()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiSiapDiserahkanAsync(k);

        var galat = await Assert.ThrowsAsync<PrescriptionDispensingUnprocessableException>(() =>
            h.Penyerahan().Layanan.PrepareAsync(h.ResepId,
                Penyiapan(h, depo: h.DepoNonaktifId)));

        Assert.Equal("PHM112", galat.Code);
    }

    [Fact]
    public async Task Depo_yang_tidak_boleh_menyerahkan_obat_ditolak()
    {
        // Gudang induk aktif, tetapi bukan tempat obat diserahkan ke pasien. Membedakan keduanya
        // mencegah stok gudang berkurang seolah-olah diserahkan dari loket.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiSiapDiserahkanAsync(k);

        var galat = await Assert.ThrowsAsync<PrescriptionDispensingUnprocessableException>(() =>
            h.Penyerahan().Layanan.PrepareAsync(h.ResepId,
                Penyiapan(h, depo: h.GudangTanpaPenyerahanId)));

        Assert.Equal("PHM112", galat.Code);
    }

    [Fact]
    public async Task Petugas_penyerah_yang_tidak_aktif_ditolak()
    {
        // `PHM113`.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiSiapDiserahkanAsync(k);

        var galat = await Assert.ThrowsAsync<PrescriptionDispensingUnprocessableException>(() =>
            h.Penyerahan().Layanan.PrepareAsync(h.ResepId,
                Penyiapan(h, petugas: h.PetugasNonaktifId)));

        Assert.Equal("PHM113", galat.Code);
    }

    [Fact]
    public async Task Petugas_penyerah_yang_tidak_ada_ditolak()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiSiapDiserahkanAsync(k);

        var galat = await Assert.ThrowsAsync<PrescriptionDispensingUnprocessableException>(() =>
            h.Penyerahan().Layanan.PrepareAsync(h.ResepId,
                Penyiapan(h, petugas: Guid.NewGuid())));

        Assert.Equal("PHM113", galat.Code);
    }

    // ------------------------------------------------------------- baris & jumlah

    [Fact]
    public async Task Penyiapan_tanpa_baris_ditolak()
    {
        // `PHM101`.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiSiapDiserahkanAsync(k);

        var permintaan = Penyiapan(h);
        permintaan.Items.Clear();

        var galat = await Assert.ThrowsAsync<PrescriptionDispensingUnprocessableException>(() =>
            h.Penyerahan().Layanan.PrepareAsync(h.ResepId, permintaan));

        Assert.Equal("PHM101", galat.Code);
    }

    [Fact]
    public async Task Baris_resep_milik_resep_lain_ditolak()
    {
        // `PHM102`. Tanpa ini, obat milik resep orang lain dapat ikut terserahkan.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiSiapDiserahkanAsync(k);

        var galat = await Assert.ThrowsAsync<PrescriptionDispensingUnprocessableException>(() =>
            h.Penyerahan().Layanan.PrepareAsync(h.ResepId,
                Penyiapan(h, itemId: Guid.NewGuid())));

        Assert.Equal("PHM102", galat.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Jumlah_penyerahan_harus_lebih_dari_nol(int jumlah)
    {
        // `PHM103`.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiSiapDiserahkanAsync(k);

        var galat = await Assert.ThrowsAsync<PrescriptionDispensingUnprocessableException>(() =>
            h.Penyerahan().Layanan.PrepareAsync(h.ResepId, Penyiapan(h, jumlah: jumlah)));

        Assert.Equal("PHM103", galat.Code);
    }

    [Fact]
    public async Task Jumlah_melebihi_sisa_yang_boleh_diserahkan_ditolak()
    {
        // `PHM104`. Resep uji berisi 10; permintaan 11 berarti menyerahkan lebih banyak
        // daripada yang diresepkan dokter.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiSiapDiserahkanAsync(k);

        var galat = await Assert.ThrowsAsync<PrescriptionDispensingUnprocessableException>(() =>
            h.Penyerahan().Layanan.PrepareAsync(h.ResepId, Penyiapan(h, jumlah: 11)));

        Assert.Equal("PHM104", galat.Code);
    }

    [Fact]
    public async Task Baris_resep_tanpa_satuan_serah_ditolak()
    {
        // `PHM115`. Jumlah tanpa satuan tidak dapat dikurangkan dari stok mana pun.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiSiapDiserahkanAsync(k);

        var item = await k.Set<PhmPrescriptionItem>().FindAsync(h.ItemResepId);
        item!.DispenseUnitMeasurementId = null;
        await k.SaveChangesAsync();

        var galat = await Assert.ThrowsAsync<PrescriptionDispensingUnprocessableException>(() =>
            h.Penyerahan().Layanan.PrepareAsync(h.ResepId, Penyiapan(h)));

        Assert.Equal("PHM115", galat.Code);
    }

    [Fact]
    public async Task Kunci_idempotensi_wajib_diisi()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiSiapDiserahkanAsync(k);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            h.Penyerahan().Layanan.PrepareAsync(h.ResepId, Penyiapan(h, kunci: "  ")));
    }

    // ------------------------------------------------------------- pembatalan

    [Fact]
    public async Task Pembatalan_penyiapan_wajib_beralasan()
    {
        // `PHM106`.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiSiapDiserahkanAsync(k);

        var galat = await Assert.ThrowsAsync<PrescriptionDispensingUnprocessableException>(() =>
            h.Penyerahan().Layanan.CancelAsync(h.ResepId, Guid.NewGuid(), new CancelPrescriptionDispensingRequest
            {
                Reason = "   ",
                ExpectedVersion = 0,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));

        Assert.Equal("PHM106", galat.Code);
    }

    [Fact]
    public async Task Pembatalan_penyiapan_yang_tidak_ada_ditolak()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiSiapDiserahkanAsync(k);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            h.Penyerahan().Layanan.CancelAsync(h.ResepId, Guid.NewGuid(), new CancelPrescriptionDispensingRequest
            {
                Reason = "Salah input",
                ExpectedVersion = 0,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));
    }

    // ------------------------------------------------------------- pembacaan

    [Fact]
    public async Task Ringkasan_penyerahan_resep_yang_tidak_ada_mengembalikan_null()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        Assert.Null(await h.Penyerahan().Layanan.GetSummaryAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Ringkasan_penyerahan_memuat_sisa_penuh_sebelum_ada_penyerahan()
    {
        // Sisa yang boleh diserahkan berawal sama dengan jumlah yang diresepkan.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiSiapDiserahkanAsync(k);

        var ringkasan = await h.Penyerahan().Layanan.GetSummaryAsync(h.ResepId);

        Assert.NotNull(ringkasan);
        Assert.Equal(h.ResepId, ringkasan!.PrescriptionId);
    }
}
