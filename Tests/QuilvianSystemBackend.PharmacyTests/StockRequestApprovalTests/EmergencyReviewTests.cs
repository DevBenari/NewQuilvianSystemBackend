using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using static QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests.Perintah;

namespace QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests;

/// <summary>
/// Verifikasi administratif susulan CITO (<c>LGS-DEC-005</c>): oleh atasan unit, tidak mengubah
/// status maupun stok, temuan penyimpangan tercatat tanpa menghapus transaksi, dan hanya sekali.
/// </summary>
public class EmergencyReviewTests
{
    [Fact]
    public async Task Atasan_unit_membenarkan_otorisasi_CITO_tanpa_mengubah_status()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _, versi) = await h.CitoTerotorisasiAsync();

        var hasil = await h.Layanan(h.CreateContext(), h.AtasanUserId)
            .VerifyEmergencyAsync(id, Verifikasi("uji-verif-1", versi, catatan: "Sesuai kondisi pasien"));

        Assert.Equal(StockRequestStatus.Approved, hasil.Status);
        Assert.Equal(versi + 1, hasil.Version);
        Assert.Equal(StockRequestEmergencyReviewState.Confirmed, hasil.EmergencyReviewState);
        Assert.NotNull(hasil.EmergencyReview);
        Assert.Equal(StockRequestEmergencyReviewOutcome.Confirmed, hasil.EmergencyReview!.Outcome);
        Assert.Equal(h.AtasanUserId, hasil.EmergencyReview.ActorUserId);
        Assert.Equal("Sesuai kondisi pasien", hasil.EmergencyReview.Reason);

        var riwayat = Assert.Single(hasil.Histories, x => x.Action == "VerifyEmergencyStockRequestConfirmed");
        Assert.Equal(StockRequestStatus.Approved, riwayat.FromStatus);
        Assert.Equal(StockRequestStatus.Approved, riwayat.ToStatus);
    }

    [Fact]
    public async Task Kasus13_verifikasi_susulan_tidak_mengubah_stok_walau_service_memegang_akses_stok()
    {
        using var h = new StockRequestApprovalHarness();
        await h.SediakanStokAsync();
        var (id, _, versi) = await h.CitoTerotorisasiAsync();
        var sebelum = await h.PotretStokAsync();

        await h.Layanan(h.CreateContext(), h.AtasanUserId, denganStok: true)
            .VerifyEmergencyAsync(id, Verifikasi("uji-verif-stok", versi,
                StockRequestEmergencyReviewOutcome.Deviation, "Bukan kondisi gawat darurat"));

        Assert.Equal(sebelum, await h.PotretStokAsync());
    }

    [Fact]
    public async Task Temuan_penyimpangan_tidak_menghapus_transaksi_fisik_yang_sudah_terjadi()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, itemId, versi) = await h.CitoTerotorisasiAsync();
        var selesai = await h.Layanan(h.CreateContext(), h.StafUserId)
            .FulfillAsync(id, Serahkan(itemId, versi, "uji-serah-cito", jumlah: 8m));

        var hasil = await h.Layanan(h.CreateContext(), h.AtasanUserId)
            .VerifyEmergencyAsync(id, Verifikasi("uji-temuan", selesai.Version,
                StockRequestEmergencyReviewOutcome.Deviation, "Kebutuhan tidak gawat darurat"));

        Assert.Equal(StockRequestStatus.Completed, hasil.Status);
        Assert.Equal(StockRequestEmergencyReviewState.Deviation, hasil.EmergencyReviewState);
        Assert.Equal((decimal?)8m, Assert.Single(hasil.Items).FulfilledQuantity);
        Assert.Single(hasil.Histories, x => x.Action == "FulfillStockRequest");
        Assert.Single(hasil.Histories, x => x.Action == "VerifyEmergencyStockRequestDeviation");
    }

    [Fact]
    public async Task Temuan_penyimpangan_wajib_disertai_catatan()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _, versi) = await h.CitoTerotorisasiAsync();

        var galat = await Assert.ThrowsAsync<StockRequestUnprocessableException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanUserId).VerifyEmergencyAsync(id,
                Verifikasi("uji-temuan-kosong", versi, StockRequestEmergencyReviewOutcome.Deviation, "  ")));

        Assert.Equal("PHM008", galat.Code);
    }

    [Fact]
    public async Task Hasil_verifikasi_yang_tidak_dikenal_ditolak()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _, versi) = await h.CitoTerotorisasiAsync();

        var galat = await Assert.ThrowsAsync<StockRequestUnprocessableException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanUserId).VerifyEmergencyAsync(id,
                Verifikasi("uji-hasil-asing", versi, (StockRequestEmergencyReviewOutcome)99)));

        Assert.Equal("PHM011", galat.Code);
    }

    [Fact]
    public async Task Kasus14_verifikator_yang_tidak_berwenang_ditolak()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _, versi) = await h.CitoTerotorisasiAsync();

        foreach (var (pengguna, kunci) in new[]
                 {
                     (h.PemohonUserId, "uji-verif-pemohon"),
                     (h.PetugasCitoUserId, "uji-verif-pengotorisasi"),
                     (h.AtasanLainUserId, "uji-verif-unit-lain"),
                     (h.AtasanTanpaIzinUserId, "uji-verif-tanpa-izin"),
                     (h.StafUserId, "uji-verif-staf"),
                     (h.SuperAdminUserId, "uji-verif-superadmin")
                 })
        {
            await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
                h.Layanan(h.CreateContext(), pengguna).VerifyEmergencyAsync(id, Verifikasi(kunci, versi)));
        }

        Assert.Equal(0, await HitungRiwayatAsync(h, id, "VerifyEmergencyStockRequestConfirmed"));
    }

    [Fact]
    public async Task Atasan_unit_yang_mengotorisasi_CITO_tidak_boleh_memverifikasi_otorisasinya_sendiri()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _, versi) = await h.CitoTerotorisasiAsync(pengotorisasi: h.AtasanCitoUserId);

        var galat = await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanCitoUserId).VerifyEmergencyAsync(id, Verifikasi("uji-verif-diri", versi)));
        Assert.Contains("otorisasinya sendiri", galat.Message);

        // Atasan unit lain pada unit yang sama tetap dapat memverifikasi.
        var hasil = await h.Layanan(h.CreateContext(), h.AtasanUserId)
            .VerifyEmergencyAsync(id, Verifikasi("uji-verif-rekan", versi));
        Assert.Equal(StockRequestEmergencyReviewState.Confirmed, hasil.EmergencyReviewState);
    }

    [Fact]
    public async Task Verifikasi_ganda_ditolak()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _, versi) = await h.CitoTerotorisasiAsync();

        var pertama = await h.Layanan(h.CreateContext(), h.AtasanUserId)
            .VerifyEmergencyAsync(id, Verifikasi("uji-ganda-1", versi));

        var galat = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanCitoUserId).VerifyEmergencyAsync(id,
                Verifikasi("uji-ganda-2", pertama.Version, StockRequestEmergencyReviewOutcome.Deviation, "Temuan")));

        Assert.Equal("PHM019", galat.Code);
        Assert.Equal(StockRequestEmergencyReviewState.Confirmed,
            (await h.Layanan(h.CreateContext(), h.AtasanUserId).GetDetailAsync(id))!.EmergencyReviewState);
    }

    [Fact]
    public async Task Pengulangan_kunci_yang_sama_tidak_menggandakan_verifikasi()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _, versi) = await h.CitoTerotorisasiAsync();

        var pertama = await h.Layanan(h.CreateContext(), h.AtasanUserId)
            .VerifyEmergencyAsync(id, Verifikasi("uji-ulang-verif", versi));
        var kedua = await h.Layanan(h.CreateContext(), h.AtasanUserId)
            .VerifyEmergencyAsync(id, Verifikasi("uji-ulang-verif", versi));

        Assert.Equal(pertama.Version, kedua.Version);
        Assert.Equal(1, await HitungRiwayatAsync(h, id, "VerifyEmergencyStockRequestConfirmed"));

        // Kunci yang sama dengan hasil berbeda adalah benturan, bukan verifikasi baru.
        var galat = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanUserId).VerifyEmergencyAsync(id,
                Verifikasi("uji-ulang-verif", versi, StockRequestEmergencyReviewOutcome.Deviation, "Temuan")));
        Assert.Equal("PHM013", galat.Code);
    }

    [Fact]
    public async Task Permintaan_yang_disetujui_atasan_biasa_tidak_memerlukan_verifikasi_susulan()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync(prioritas: StockRequestPriority.Emergency);
        var disetujui = await h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(id, Setujui("uji-biasa"));

        Assert.Equal(StockRequestEmergencyReviewState.NotRequired, disetujui.EmergencyReviewState);
        var galat = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanCitoUserId).VerifyEmergencyAsync(id,
                Verifikasi("uji-biasa-verif", disetujui.Version)));
        Assert.Equal("PHM010", galat.Code);
    }

    [Fact]
    public async Task Daftar_menunggu_verifikasi_hanya_memuat_CITO_terotorisasi_yang_belum_diverifikasi()
    {
        using var h = new StockRequestApprovalHarness();
        var (menunggu, _, _) = await h.CitoTerotorisasiAsync();
        var (sudah, _, versiSudah) = await h.CitoTerotorisasiAsync();
        await h.Layanan(h.CreateContext(), h.AtasanUserId).VerifyEmergencyAsync(sudah, Verifikasi("uji-daftar", versiSudah));
        var (rutin, _) = await h.PermintaanAsync();

        var daftar = await h.Layanan(h.CreateContext(), h.AtasanUserId).GetPagedAsync(new StockRequestPagedQuery
        {
            EmergencyReviewState = StockRequestEmergencyReviewState.Pending
        });

        var ids = daftar.Items.Select(x => x.Id).ToList();
        Assert.Contains(menunggu, ids);
        Assert.DoesNotContain(sudah, ids);
        Assert.DoesNotContain(rutin, ids);
        Assert.All(daftar.Items, x => Assert.Equal(StockRequestEmergencyReviewState.Pending, x.EmergencyReviewState));

        var terverifikasi = await h.Layanan(h.CreateContext(), h.AtasanUserId).GetPagedAsync(new StockRequestPagedQuery
        {
            EmergencyReviewState = StockRequestEmergencyReviewState.Confirmed
        });
        Assert.Equal(sudah, Assert.Single(terverifikasi.Items).Id);
    }

    [Fact]
    public async Task Detail_menyampaikan_kewenangan_verifikasi_dan_otorisasi_awal()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _, _) = await h.CitoTerotorisasiAsync();

        var atasan = await h.Layanan(h.CreateContext(), h.AtasanUserId).GetDetailAsync(id);
        Assert.True(atasan!.CanVerifyEmergency);
        Assert.Null(atasan.VerifyEmergencyMessage);
        Assert.Equal(h.PetugasCitoUserId, atasan.EmergencyAuthorization!.ActorUserId);

        var pengotorisasi = await h.Layanan(h.CreateContext(), h.PetugasCitoUserId).GetDetailAsync(id);
        Assert.False(pengotorisasi!.CanVerifyEmergency);
        Assert.False(string.IsNullOrWhiteSpace(pengotorisasi.VerifyEmergencyMessage));
    }
}
