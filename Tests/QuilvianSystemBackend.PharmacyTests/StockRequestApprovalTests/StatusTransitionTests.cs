using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using static QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests.Perintah;

namespace QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests;

/// <summary>
/// Transisi status, alasan wajib, keputusan ganda, idempotensi, dan batas aktivasi
/// (<c>LGS-DEC-002</c>, <c>LGS-DEC-006</c>).
/// </summary>
public class StatusTransitionTests
{
    // ------------------------------------------------------------------ alasan wajib

    [Fact]
    public async Task Kasus7_alasan_penolakan_wajib()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();

        var galat = await Assert.ThrowsAsync<StockRequestUnprocessableException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanUserId).RejectAsync(id, Tolak("uji-tolak-kosong", alasan: "  ")));

        Assert.Equal("PHM015", galat.Code);
        await AssertMasihMenungguAsync(h, id);
    }

    [Fact]
    public async Task Kasus8_alasan_CITO_wajib_saat_pengajuan_dan_saat_otorisasi()
    {
        using var h = new StockRequestApprovalHarness();
        var (draf, _) = await h.PermintaanAsync(status: StockRequestStatus.Draft,
            prioritas: StockRequestPriority.Emergency);
        var (menunggu, _) = await h.PermintaanAsync(prioritas: StockRequestPriority.Emergency);

        var ajukan = await Assert.ThrowsAsync<StockRequestUnprocessableException>(() =>
            h.Layanan(h.CreateContext(), h.PemohonUserId).SubmitAsync(draf, new SubmitStockRequestRequest
            {
                EmergencyReason = " ", ExpectedVersion = Versi, IdempotencyKey = "uji-ajukan-cito"
            }));
        Assert.Equal("PHM014", ajukan.Code);

        var otorisasi = await Assert.ThrowsAsync<StockRequestUnprocessableException>(() =>
            h.Layanan(h.CreateContext(), h.PetugasCitoUserId)
                .AuthorizeEmergencyAsync(menunggu, Cito("uji-cito-kosong", alasan: "")));
        Assert.Equal("PHM015", otorisasi.Code);
    }

    [Fact]
    public async Task Jalur_CITO_tidak_dapat_dipakai_untuk_permintaan_rutin()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync(prioritas: StockRequestPriority.Routine);

        var galat = await Assert.ThrowsAsync<StockRequestUnprocessableException>(() =>
            h.Layanan(h.CreateContext(), h.PetugasCitoUserId).AuthorizeEmergencyAsync(id, Cito("uji-cito-rutin")));

        Assert.Equal("PHM017", galat.Code);
    }

    // --------------------------------------------------------------- keputusan ganda

    [Fact]
    public async Task Kasus9_keputusan_kedua_ditolak_409_dan_hanya_ada_satu_keputusan()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync(prioritas: StockRequestPriority.Emergency);

        await h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(id, Setujui("uji-ganda-1"));

        var setujuLagi = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanCitoUserId).ApproveAsync(id, Setujui("uji-ganda-2", versi: Versi + 1)));
        Assert.Equal("PHM018", setujuLagi.Code);

        var tolak = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanCitoUserId).RejectAsync(id, Tolak("uji-ganda-3", versi: Versi + 1)));
        Assert.Equal("PHM018", tolak.Code);

        var cito = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(h.CreateContext(), h.PetugasCitoUserId).AuthorizeEmergencyAsync(id, Cito("uji-ganda-4", versi: Versi + 1)));
        Assert.Equal("PHM018", cito.Code);

        Assert.Equal(1, await HitungRiwayatAsync(h, id, "ApproveStockRequest"));
        Assert.Equal(0, await HitungRiwayatAsync(h, id, "RejectStockRequest"));
        Assert.Equal(0, await HitungRiwayatAsync(h, id, "AuthorizeEmergencyStockRequest"));
    }

    [Fact]
    public async Task Pengulangan_dengan_kunci_sama_tidak_menggandakan_keputusan()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();

        var pertama = await h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(id, Setujui("uji-ulang"));
        var kedua = await h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(id, Setujui("uji-ulang"));

        Assert.Equal(pertama.Version, kedua.Version);
        Assert.Equal(1, await HitungRiwayatAsync(h, id, "ApproveStockRequest"));
    }

    [Fact]
    public async Task Kunci_sama_dengan_isi_berbeda_ditolak()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();

        await h.Layanan(h.CreateContext(), h.AtasanUserId).RejectAsync(id, Tolak("uji-isi", alasan: "Alasan pertama"));

        var galat = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanUserId).RejectAsync(id, Tolak("uji-isi", alasan: "Alasan lain")));

        Assert.Equal("PHM013", galat.Code);
    }

    // ------------------------------------------------------------------- transisi

    [Fact]
    public async Task Kasus12_permintaan_baru_wajib_disetujui_sebelum_diserahkan()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, itemId) = await h.PermintaanAsync();

        var galat = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(h.CreateContext(), h.StafUserId).FulfillAsync(id, Serahkan(itemId, Versi, "uji-serah-dini")));

        Assert.Equal("PHM016", galat.Code);
        await AssertMasihMenungguAsync(h, id);
    }

    [Fact]
    public async Task Disetujui_lalu_diserahkan_menjadi_selesai_dan_penandanya_konsisten()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, itemId) = await h.PermintaanAsync();

        var disetujui = await h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(id, Setujui("uji-alur-1"));
        Assert.True(disetujui.CanFulfill);
        Assert.False(disetujui.CanApprove);

        var selesai = await h.Layanan(h.CreateContext(), h.StafUserId)
            .FulfillAsync(id, Serahkan(itemId, disetujui.Version, "uji-alur-2", jumlah: 6m));

        Assert.Equal(StockRequestStatus.Completed, selesai.Status);
        Assert.True(selesai.IsPartiallyFulfilled);
        Assert.False(selesai.CanFulfill);
        Assert.Equal(StockRequestStatus.Approved,
            Assert.Single(selesai.Histories, x => x.Action == "FulfillStockRequest").FromStatus);
    }

    [Fact]
    public async Task Persetujuan_tidak_mengubah_stok()
    {
        using var h = new StockRequestApprovalHarness();
        await h.SediakanStokAsync();
        var (id, _) = await h.PermintaanAsync();
        var sebelum = await h.PotretStokAsync();

        await h.Layanan(h.CreateContext(), h.AtasanUserId, denganStok: true).ApproveAsync(id, Setujui("uji-stok"));

        Assert.Equal(sebelum, await h.PotretStokAsync());
    }

    [Fact]
    public async Task Ditolak_bersifat_final()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, itemId) = await h.PermintaanAsync();

        var ditolak = await h.Layanan(h.CreateContext(), h.AtasanUserId).RejectAsync(id, Tolak("uji-final-1"));
        var versi = ditolak.Version;
        Assert.False(ditolak.CanFulfill);
        Assert.False(ditolak.CanCancel);

        var serah = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(h.CreateContext(), h.StafUserId).FulfillAsync(id, Serahkan(itemId, versi, "uji-final-2")));
        Assert.Equal("PHM004", serah.Code);

        var batal = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(h.CreateContext(), h.PemohonUserId).CancelAsync(id, new CancelStockRequestRequest
            {
                Reason = "Tidak jadi", ExpectedVersion = versi, IdempotencyKey = "uji-final-3"
            }));
        Assert.Equal("PHM004", batal.Code);
    }

    [Fact]
    public async Task Draft_tidak_dapat_diputus_sebelum_diajukan()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync(status: StockRequestStatus.Draft);

        var galat = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(id, Setujui("uji-draft")));

        Assert.Equal("PHM004", galat.Code);
    }

    // ----------------------------------------------------------- batas aktivasi (integrasi)

    [Fact]
    public async Task Kasus11_permintaan_sebelum_batas_mengikuti_aturan_peralihan()
    {
        using var h = new StockRequestApprovalHarness();
        var batas = DateTime.UtcNow.AddDays(-1).ToString("O");
        var (id, itemId) = await h.PermintaanAsync(diajukanPada: DateTime.UtcNow.AddDays(-6));

        var detail = await h.Layanan(h.CreateContext(), h.StafUserId, batas).GetDetailAsync(id);
        Assert.True(detail!.IsLegacySubmission);
        Assert.True(detail.CanFulfill);

        var selesai = await h.Layanan(h.CreateContext(), h.StafUserId, batas)
            .FulfillAsync(id, Serahkan(itemId, Versi, "uji-peralihan-1"));

        Assert.Equal(StockRequestStatus.Completed, selesai.Status);
        // Jalur peralihan selalu terbaca dari riwayat: Submitted langsung ke Completed.
        Assert.Equal(StockRequestStatus.Submitted,
            Assert.Single(selesai.Histories, x => x.Action == "FulfillStockRequest").FromStatus);
    }

    [Fact]
    public async Task Kasus12_permintaan_sesudah_batas_wajib_disetujui()
    {
        using var h = new StockRequestApprovalHarness();
        var batas = DateTime.UtcNow.AddDays(-1).ToString("O");
        var (id, itemId) = await h.PermintaanAsync(diajukanPada: DateTime.UtcNow.AddHours(-1));

        var galat = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(h.CreateContext(), h.StafUserId, batas).FulfillAsync(id, Serahkan(itemId, Versi, "uji-peralihan-2")));

        Assert.Equal("PHM016", galat.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("bukan-tanggal")]
    public async Task Batas_kosong_atau_tidak_sah_tidak_membuka_jalan_pintas(string? batas)
    {
        using var h = new StockRequestApprovalHarness();
        var (id, itemId) = await h.PermintaanAsync(diajukanPada: DateTime.UtcNow.AddDays(-30));

        var detail = await h.Layanan(h.CreateContext(), h.StafUserId, batas).GetDetailAsync(id);
        Assert.False(detail!.IsLegacySubmission);
        Assert.False(detail.CanFulfill);

        var galat = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(h.CreateContext(), h.StafUserId, batas).FulfillAsync(id, Serahkan(itemId, Versi, "uji-peralihan-3")));
        Assert.Equal("PHM016", galat.Code);
    }

    [Fact]
    public async Task Batas_di_masa_depan_tidak_membuat_permintaan_hari_ini_lolos_tanpa_persetujuan()
    {
        using var h = new StockRequestApprovalHarness();
        var batasDepan = DateTime.UtcNow.AddDays(3).ToString("O");
        var (id, itemId) = await h.PermintaanAsync(diajukanPada: DateTime.UtcNow.AddMinutes(-10));

        var galat = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(h.CreateContext(), h.StafUserId, batasDepan).FulfillAsync(id, Serahkan(itemId, Versi, "uji-peralihan-4")));
        Assert.Equal("PHM016", galat.Code);
    }

    [Fact]
    public async Task Permintaan_tanpa_waktu_pengajuan_tidak_dianggap_permintaan_lama()
    {
        using var h = new StockRequestApprovalHarness();
        var batas = DateTime.UtcNow.AddDays(-1).ToString("O");
        var (id, _) = await h.PermintaanAsync(tanpaWaktuPengajuan: true);

        var detail = await h.Layanan(h.CreateContext(), h.StafUserId, batas).GetDetailAsync(id);
        Assert.False(detail!.IsLegacySubmission);
        Assert.False(detail.CanFulfill);
    }

    [Fact]
    public async Task Antrean_gudang_memuat_yang_disetujui_dan_permintaan_lama_saja()
    {
        using var h = new StockRequestApprovalHarness();
        var batas = DateTime.UtcNow.AddDays(-1).ToString("O");
        var (lama, _) = await h.PermintaanAsync(diajukanPada: DateTime.UtcNow.AddDays(-6));
        var (baru, _) = await h.PermintaanAsync(diajukanPada: DateTime.UtcNow.AddHours(-1));
        var (disetujui, _) = await h.PermintaanAsync(diajukanPada: DateTime.UtcNow.AddHours(-2));
        await h.Layanan(h.CreateContext(), h.AtasanUserId, batas).ApproveAsync(disetujui, Setujui("uji-antrean"));

        var antrean = await h.Layanan(h.CreateContext(), h.StafUserId, batas)
            .GetPagedAsync(new StockRequestPagedQuery { FulfillableOnly = true, OrderByPriority = true });

        var ids = antrean.Items.Select(x => x.Id).ToHashSet();
        Assert.Contains(lama, ids);
        Assert.Contains(disetujui, ids);
        Assert.DoesNotContain(baru, ids);
        Assert.All(antrean.Items, x => Assert.True(x.CanFulfill));
        Assert.True(antrean.Items.Single(x => x.Id == lama).IsLegacySubmission);
    }
}
