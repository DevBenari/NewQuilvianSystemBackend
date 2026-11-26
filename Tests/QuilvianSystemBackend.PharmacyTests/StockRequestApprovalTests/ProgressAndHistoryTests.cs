using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using static QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests.Perintah;

namespace QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests;

/// <summary>
/// "Status Pemenuhan" (tahapan progres dan pemenuhan sebagian) dan "Riwayat Permintaan"
/// (linimasa aktivitas) di atas data yang sudah ada (FIN-LGS-011).
/// </summary>
public class ProgressAndHistoryTests
{
    /// <summary>Permintaan yang disetujui lalu diserahkan 6 dari 10.</summary>
    private static async Task<Guid> DiserahkanSebagianAsync(StockRequestApprovalHarness h)
    {
        var (id, itemId) = await h.PermintaanAsync();
        var disetujui = await h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(id, Setujui($"uji-setuju-{id:N}"));
        await h.Layanan(h.CreateContext(), h.StafUserId)
            .FulfillAsync(id, Serahkan(itemId, disetujui.Version, $"uji-serah-{id:N}", jumlah: 6m));
        return id;
    }

    // ------------------------------------------------------------- Status Pemenuhan

    [Fact]
    public async Task Daftar_dan_detail_memuat_tahapan_progres_dari_riwayat()
    {
        using var h = new StockRequestApprovalHarness();
        var sebagian = await DiserahkanSebagianAsync(h);
        var (menunggu, _) = await h.PermintaanAsync();

        var daftar = await h.Layanan(h.CreateContext(), h.StafUserId).GetPagedAsync(new StockRequestPagedQuery());

        var barisSebagian = Assert.Single(daftar.Items, x => x.Id == sebagian);
        Assert.NotNull(barisSebagian.DecisionAt);
        Assert.NotNull(barisSebagian.FulfilledAt);
        Assert.True(barisSebagian.FulfilledAt >= barisSebagian.DecisionAt);
        Assert.True(barisSebagian.IsPartiallyFulfilled);
        Assert.Equal(1, barisSebagian.ShortfallItemCount);

        var barisMenunggu = Assert.Single(daftar.Items, x => x.Id == menunggu);
        Assert.Null(barisMenunggu.DecisionAt);
        Assert.Null(barisMenunggu.FulfilledAt);
        Assert.Equal(0, barisMenunggu.ShortfallItemCount);

        // Detail memakai aturan yang sama.
        var detail = await h.Layanan(h.CreateContext(), h.StafUserId).GetDetailAsync(sebagian);
        Assert.Equal(barisSebagian.DecisionAt, detail!.DecisionAt);
        Assert.Equal(barisSebagian.FulfilledAt, detail.FulfilledAt);
        Assert.Equal(1, detail.ShortfallItemCount);
    }

    [Fact]
    public async Task Saringan_pemenuhan_sebagian_draf_dan_pemohon_bekerja_di_backend()
    {
        using var h = new StockRequestApprovalHarness();
        var sebagian = await DiserahkanSebagianAsync(h);
        var (draf, _) = await h.PermintaanAsync(status: StockRequestStatus.Draft);
        var (milikStaf, _) = await h.PermintaanAsync(pemintaWorkforceId: h.StafWorkforceId);
        var layanan = h.Layanan(h.CreateContext(), h.StafUserId);

        var hanyaSebagian = await layanan.GetPagedAsync(new StockRequestPagedQuery { PartiallyFulfilledOnly = true });
        Assert.Equal(sebagian, Assert.Single(hanyaSebagian.Items).Id);

        var tanpaDraf = await layanan.GetPagedAsync(new StockRequestPagedQuery { ExcludeDraft = true });
        Assert.DoesNotContain(tanpaDraf.Items, x => x.Id == draf);
        Assert.Equal(2, tanpaDraf.TotalData);

        var pemintaStaf = await layanan.GetPagedAsync(new StockRequestPagedQuery
        {
            RequestedByWorkforceId = h.StafWorkforceId
        });
        Assert.Equal(milikStaf, Assert.Single(pemintaStaf.Items).Id);
    }

    // ------------------------------------------------------------ Riwayat Permintaan

    [Fact]
    public async Task Linimasa_memuat_setiap_perubahan_terbaru_lebih_dahulu_dengan_nama_petugas()
    {
        using var h = new StockRequestApprovalHarness();
        var id = await DiserahkanSebagianAsync(h);

        var linimasa = await h.Layanan(h.CreateContext(), h.StafUserId)
            .GetHistoryPagedAsync(new StockRequestHistoryPagedQuery { PageSize = 50 });

        // Dibuat, diajukan (harness), disetujui, diserahkan.
        Assert.Equal(4, linimasa.TotalData);
        Assert.Equal(
            ["FulfillStockRequest", "ApproveStockRequest", "SubmitStockRequest", "CreateStockRequest"],
            linimasa.Items.Select(x => x.Action).ToList());
        Assert.All(linimasa.Items, x => Assert.Equal(id, x.StockRequestId));

        var disetujui = Assert.Single(linimasa.Items, x => x.Action == "ApproveStockRequest");
        Assert.Equal(h.AtasanUserId, disetujui.ActorUserId);
        Assert.Equal("UJI-WFP-ATASAN", disetujui.ActorName);
        Assert.Equal(StockRequestStatus.Submitted, disetujui.FromStatus);
        Assert.Equal(StockRequestStatus.Approved, disetujui.ToStatus);
        Assert.False(string.IsNullOrWhiteSpace(disetujui.RequestNumber));
        Assert.Equal("Ruang Melati (Uji)", disetujui.RequestingServiceUnitName);

        // Pemohon dan petugas yang memutus tidak tercampur: pemohon dari profil peminta,
        // petugas dari pelaku kejadian.
        Assert.All(linimasa.Items, x => Assert.Equal("UJI-WFP-PEMOHON", x.RequestedByName));
        Assert.NotEqual(disetujui.RequestedByName, disetujui.ActorName);
        Assert.Equal(h.StafUserId, Assert.Single(linimasa.Items, x => x.Action == "FulfillStockRequest").ActorUserId);

        // Tidak ada kejadian ganda: satu baris per riwayat.
        Assert.Equal(linimasa.Items.Count, linimasa.Items.Select(x => x.Id).Distinct().Count());
    }

    [Fact]
    public async Task Linimasa_tidak_membuka_permintaan_di_luar_yang_terlihat_pada_daftar()
    {
        using var h = new StockRequestApprovalHarness();
        await h.PermintaanAsync(unitId: h.UnitId);
        await h.PermintaanAsync(unitId: h.UnitLainId, prioritas: StockRequestPriority.Emergency);
        await h.PermintaanAsync(unitId: h.UnitLainId);
        await h.PermintaanAsync(status: StockRequestStatus.Draft, unitId: h.UnitLainId,
            prioritas: StockRequestPriority.Emergency);

        foreach (var pengguna in new[] { h.AtasanUserId, h.AtasanLainUserId, h.PetugasCitoUserId, h.StafUserId })
        {
            foreach (var cakupan in new[] { StockRequestListScope.All, StockRequestListScope.MyAuthority })
            {
                var daftar = await h.Layanan(h.CreateContext(), pengguna)
                    .GetPagedAsync(new StockRequestPagedQuery { Scope = cakupan, PageSize = 100 });
                var linimasa = await h.Layanan(h.CreateContext(), pengguna)
                    .GetHistoryPagedAsync(new StockRequestHistoryPagedQuery { Scope = cakupan, PageSize = 100 });

                var terlihat = daftar.Items.Select(x => x.Id).ToHashSet();
                Assert.All(linimasa.Items, x => Assert.Contains(x.StockRequestId, terlihat));
                Assert.Equal(terlihat, linimasa.Items.Select(x => x.StockRequestId).ToHashSet());
            }
        }
    }

    [Fact]
    public async Task Linimasa_menyaring_status_unit_pemohon_tanggal_dan_halaman()
    {
        using var h = new StockRequestApprovalHarness();
        await DiserahkanSebagianAsync(h);
        var (unitLain, _) = await h.PermintaanAsync(unitId: h.UnitLainId);
        var (milikStaf, _) = await h.PermintaanAsync(pemintaWorkforceId: h.StafWorkforceId);
        var layanan = h.Layanan(h.CreateContext(), h.StafUserId);

        var disetujui = await layanan.GetHistoryPagedAsync(new StockRequestHistoryPagedQuery
        {
            ToStatus = StockRequestStatus.Approved
        });
        Assert.Equal("ApproveStockRequest", Assert.Single(disetujui.Items).Action);

        var perUnit = await layanan.GetHistoryPagedAsync(new StockRequestHistoryPagedQuery
        {
            RequestingServiceUnitId = h.UnitLainId
        });
        Assert.Equal(2, perUnit.TotalData);
        Assert.All(perUnit.Items, x => Assert.Equal(unitLain, x.StockRequestId));

        var perPemohon = await layanan.GetHistoryPagedAsync(new StockRequestHistoryPagedQuery
        {
            RequestedByWorkforceId = h.StafWorkforceId
        });
        Assert.Equal(2, perPemohon.TotalData);
        Assert.All(perPemohon.Items, x => Assert.Equal(milikStaf, x.StockRequestId));

        // Rentang lebar supaya tidak bergantung pada zona waktu mesin uji.
        var lampau = await layanan.GetHistoryPagedAsync(new StockRequestHistoryPagedQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-10), EndDate = DateTime.UtcNow.AddDays(-3)
        });
        Assert.Equal(0, lampau.TotalData);
        var kini = await layanan.GetHistoryPagedAsync(new StockRequestHistoryPagedQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-1)
        });
        Assert.Equal(8, kini.TotalData);

        var halaman = await layanan.GetHistoryPagedAsync(new StockRequestHistoryPagedQuery { PageSize = 3 });
        Assert.Equal(8, halaman.TotalData);
        Assert.Equal(3, halaman.TotalPage);
        Assert.Equal(3, halaman.Items.Count);
    }

    [Fact]
    public async Task Linimasa_dengan_cakupan_mengikuti_kewenangan_dan_nilai_asing_tidak_ditafsirkan()
    {
        using var h = new StockRequestApprovalHarness();
        var (unitA, _) = await h.PermintaanAsync(unitId: h.UnitId);
        var (unitB, _) = await h.PermintaanAsync(unitId: h.UnitLainId);
        var (draf, _) = await h.PermintaanAsync(status: StockRequestStatus.Draft, unitId: h.UnitId);

        // Pemegang izin persetujuan melihat permintaan seluruh unit yang sudah diajukan (FIN-LGS-070).
        var penyetuju = await h.Layanan(h.CreateContext(), h.AtasanLainUserId)
            .GetHistoryPagedAsync(new StockRequestHistoryPagedQuery { Scope = StockRequestListScope.MyAuthority });
        Assert.Equal(new HashSet<Guid> { unitA, unitB }, penyetuju.Items.Select(x => x.StockRequestId).ToHashSet());
        Assert.DoesNotContain(penyetuju.Items, x => x.StockRequestId == draf);

        var tanpaKewenangan = await h.Layanan(h.CreateContext(), h.StafUserId)
            .GetHistoryPagedAsync(new StockRequestHistoryPagedQuery { Scope = StockRequestListScope.MyAuthority });
        Assert.Equal(0, tanpaKewenangan.TotalData);

        var layanan = h.Layanan(h.CreateContext(), h.StafUserId);
        Assert.Equal(0, (await layanan.GetHistoryPagedAsync(new StockRequestHistoryPagedQuery
        {
            Scope = (StockRequestListScope)9
        })).TotalData);
        Assert.Equal(0, (await layanan.GetHistoryPagedAsync(new StockRequestHistoryPagedQuery
        {
            ToStatus = (StockRequestStatus)99
        })).TotalData);
    }
}
