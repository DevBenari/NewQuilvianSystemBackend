using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using static QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests.Perintah;

namespace QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests;

/// <summary>
/// Konkurensi pada basis data (kasus wajib 10): dua keputusan yang balapan tidak boleh
/// menghasilkan dua perubahan sah.
/// </summary>
/// <remarks>
/// <para>
/// Cara uji: keputusan A sudah memuat permintaan dan lolos seluruh pemeriksaan, lalu tepat sebelum
/// simpanannya terjadi, keputusan B dijalankan sampai tuntas pada konteks lain. Simpanan A kemudian
/// harus gagal utuh karena token versi, sehingga hanya keputusan B yang tersimpan. Ini balapan yang
/// sebenarnya terjadi bila dua atasan menekan tombol hampir bersamaan.
/// </para>
/// <para>
/// Keterbatasan: SQLite menyerialkan penulisan, sedangkan PostgreSQL menjalankan dua transaksi
/// sungguh-sungguh bersamaan. Penjaganya sama — <c>UPDATE … WHERE "Version" = @lama</c> — dan pada
/// PostgreSQL transaksi kedua menunggu yang pertama lalu mendapati nol baris. Pembuktian pada
/// PostgreSQL membutuhkan basis data sandbox dan belum dijalankan.
/// </para>
/// </remarks>
public class ConcurrencyTests
{
    /// <summary>
    /// Menjalankan <paramref name="pesaing"/> sekali, tepat sebelum simpanan pertama pada
    /// <paramref name="konteks"/>. Pesaing berjalan di thread lain supaya tidak terjebak konteks
    /// sinkronisasi uji; konteks pemanggil menunggu, sehingga koneksi SQLite tidak dipakai bersamaan.
    /// </summary>
    private static void SisipkanPesaing(ApplicationDbContext konteks, Func<Task> pesaing)
    {
        var sudah = 0;
        konteks.SavingChanges += (_, _) =>
        {
            if (Interlocked.Exchange(ref sudah, 1) != 0) return;
            Task.Run(pesaing).GetAwaiter().GetResult();
        };
    }

    [Fact]
    public async Task Dua_persetujuan_bersamaan_hanya_satu_yang_tersimpan()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();

        var konteksA = h.CreateContext();
        SisipkanPesaing(konteksA, () =>
            h.Layanan(h.CreateContext(), h.AtasanCitoUserId).ApproveAsync(id, Setujui("uji-balap-b")));

        var galat = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(konteksA, h.AtasanUserId).ApproveAsync(id, Setujui("uji-balap-a")));

        Assert.Equal("PHM012", galat.Code);
        Assert.Equal(1, await HitungRiwayatAsync(h, id, "ApproveStockRequest"));

        using var k = h.CreateContext();
        var tersimpan = await k.PhmStockRequests.AsNoTracking().SingleAsync(x => x.Id == id);
        Assert.Equal(StockRequestStatus.Approved, tersimpan.Status);
        Assert.Equal(Versi + 1, tersimpan.Version);
        Assert.Equal(h.AtasanCitoUserId, tersimpan.UpdateBy);
    }

    [Fact]
    public async Task Setuju_dan_tolak_bersamaan_hanya_satu_keputusan_yang_sah()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();

        var konteksA = h.CreateContext();
        SisipkanPesaing(konteksA, () =>
            h.Layanan(h.CreateContext(), h.AtasanCitoUserId).RejectAsync(id, Tolak("uji-balap-tolak")));

        var galat = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(konteksA, h.AtasanUserId).ApproveAsync(id, Setujui("uji-balap-setuju")));

        Assert.Equal("PHM012", galat.Code);
        Assert.Equal(0, await HitungRiwayatAsync(h, id, "ApproveStockRequest"));
        Assert.Equal(1, await HitungRiwayatAsync(h, id, "RejectStockRequest"));

        using var k = h.CreateContext();
        Assert.Equal(StockRequestStatus.Rejected,
            (await k.PhmStockRequests.AsNoTracking().SingleAsync(x => x.Id == id)).Status);
    }

    [Fact]
    public async Task Otorisasi_CITO_dan_penolakan_bersamaan_hanya_satu_yang_sah()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync(prioritas: StockRequestPriority.Emergency);

        var konteksA = h.CreateContext();
        SisipkanPesaing(konteksA, () =>
            h.Layanan(h.CreateContext(), h.AtasanUserId).RejectAsync(id, Tolak("uji-balap-cito-tolak")));

        var galat = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(konteksA, h.PetugasCitoUserId).AuthorizeEmergencyAsync(id, Cito("uji-balap-cito")));

        Assert.Equal("PHM012", galat.Code);
        Assert.Equal(0, await HitungRiwayatAsync(h, id, "AuthorizeEmergencyStockRequest"));
        Assert.Equal(1, await HitungRiwayatAsync(h, id, "RejectStockRequest"));
    }

    [Fact]
    public async Task Dua_verifikasi_susulan_bersamaan_hanya_satu_yang_tersimpan()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _, versi) = await h.CitoTerotorisasiAsync();

        var konteksA = h.CreateContext();
        SisipkanPesaing(konteksA, () =>
            h.Layanan(h.CreateContext(), h.AtasanCitoUserId).VerifyEmergencyAsync(id,
                Verifikasi("uji-balap-verif-b", versi, StockRequestEmergencyReviewOutcome.Deviation, "Temuan")));

        var galat = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(konteksA, h.AtasanUserId).VerifyEmergencyAsync(id, Verifikasi("uji-balap-verif-a", versi)));

        Assert.Equal("PHM012", galat.Code);
        Assert.Equal(0, await HitungRiwayatAsync(h, id, "VerifyEmergencyStockRequestConfirmed"));
        Assert.Equal(1, await HitungRiwayatAsync(h, id, "VerifyEmergencyStockRequestDeviation"));
    }

    [Fact]
    public async Task Token_versi_menolak_penyimpanan_dari_salinan_lama()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();

        using var salinanLama = h.CreateContext();
        var basi = await salinanLama.PhmStockRequests.SingleAsync(x => x.Id == id);

        await h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(id, Setujui("uji-token"));

        basi.Status = StockRequestStatus.Rejected;
        basi.Version++;

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => salinanLama.SaveChangesAsync());

        using var k = h.CreateContext();
        Assert.Equal(StockRequestStatus.Approved,
            (await k.PhmStockRequests.AsNoTracking().SingleAsync(x => x.Id == id)).Status);
    }

    [Fact]
    public async Task Versi_yang_tertinggal_ditolak_sebelum_menyentuh_data()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();

        var galat = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(id, Setujui("uji-versi", versi: Versi - 1)));

        Assert.Equal("PHM012", galat.Code);
        await AssertMasihMenungguAsync(h, id);
    }
}
