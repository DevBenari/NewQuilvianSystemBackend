using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using static QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests.Perintah;

namespace QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests;

/// <summary>
/// Pengerasan tanpa keputusan bisnis baru (FIN-LGS-001, FIN-LGS-003, FIN-LGS-004): pengulangan
/// kunci idempotensi, jejak audit pembatalan, validasi nilai enum, dan alasan CITO pemohon.
/// </summary>
public class HardeningTests
{
    // ------------------------------------------------------ pengulangan kunci idempotensi

    [Fact]
    public async Task Kunci_idempotensi_milik_pengguna_lain_ditolak_dan_tidak_membuka_hasilnya()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();
        await h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(id, Setujui("uji-kunci-bersama"));

        var galat = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanCitoUserId).ApproveAsync(id, Setujui("uji-kunci-bersama")));

        Assert.Equal("PHM013", galat.Code);
        Assert.Contains("pengguna lain", galat.Message);
        Assert.Equal(1, await HitungRiwayatAsync(h, id, "ApproveStockRequest"));
    }

    [Fact]
    public async Task Kunci_pembatalan_yang_dipakai_ulang_untuk_permintaan_lain_tidak_dianggap_berhasil()
    {
        using var h = new StockRequestApprovalHarness();
        var (pertama, _) = await h.PermintaanAsync();
        var (kedua, _) = await h.PermintaanAsync();
        var batal = new CancelStockRequestRequest
        {
            Reason = "Kebutuhan sudah terpenuhi", ExpectedVersion = Versi, IdempotencyKey = "uji-batal-sama"
        };

        await h.Layanan(h.CreateContext(), h.PemohonUserId).CancelAsync(pertama, batal);

        // Sidik jari pembatalan hanya memuat alasan; tanpa pemeriksaan id, permintaan kedua
        // dijawab "berhasil" padahal tidak pernah dibatalkan.
        var galat = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(h.CreateContext(), h.PemohonUserId).CancelAsync(kedua, batal));

        Assert.Equal("PHM013", galat.Code);
        await AssertMasihMenungguAsync(h, kedua);
    }

    [Fact]
    public async Task Pengulangan_oleh_pelaku_yang_sama_tetap_idempoten()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();
        var batal = new CancelStockRequestRequest
        {
            Reason = "Salah unit", ExpectedVersion = Versi, IdempotencyKey = "uji-batal-ulang"
        };

        var pertama = await h.Layanan(h.CreateContext(), h.PemohonUserId).CancelAsync(id, batal);
        var kedua = await h.Layanan(h.CreateContext(), h.PemohonUserId).CancelAsync(id, batal);

        Assert.Equal(StockRequestStatus.Cancelled, kedua.Status);
        Assert.Equal(pertama.Version, kedua.Version);
        Assert.Equal(1, await HitungRiwayatAsync(h, id, "CancelStockRequest"));
    }

    // ------------------------------------------------------------------ jejak audit

    [Fact]
    public async Task Pembatalan_permintaan_disetujui_tercatat_di_log_audit_dengan_pelaku_dan_status_asal()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();
        var disetujui = await h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(id, Setujui("uji-audit-setuju"));
        var log = new CatatanLogUji();

        await h.Layanan(h.CreateContext(), h.PemohonUserId, log: log).CancelAsync(id, new CancelStockRequestRequest
        {
            Reason = "Pasien sudah pulang", ExpectedVersion = disetujui.Version, IdempotencyKey = "uji-audit-batal"
        });

        var audit = Assert.Single(log.UntukAksi("StockRequest.Cancel"));
        Assert.Equal(h.PemohonUserId.ToString(), CatatanLogUji.PelakuPada(audit.Pesan));
        Assert.DoesNotContain(log.Entri, x => CatatanLogUji.PelakuPada(x.Pesan) == id.ToString());
    }

    // ------------------------------------------------------------ validasi nilai enum

    [Fact]
    public void Nilai_enum_yang_tidak_dikenal_ditolak_validasi_permintaan_HTTP()
    {
        static bool Sah(object dto) =>
            Validator.TryValidateObject(dto, new ValidationContext(dto), [], validateAllProperties: true);

        Assert.True(Sah(new StockRequestPagedQuery()));
        Assert.True(Sah(new StockRequestPagedQuery { Scope = StockRequestListScope.MyAuthority }));
        Assert.False(Sah(new StockRequestPagedQuery { Scope = (StockRequestListScope)9 }));
        Assert.False(Sah(new StockRequestPagedQuery { Status = (StockRequestStatus)99 }));
        Assert.False(Sah(new StockRequestPagedQuery { Priority = (StockRequestPriority)9 }));
        Assert.False(Sah(new StockRequestPagedQuery { EmergencyReviewState = (StockRequestEmergencyReviewState)7 }));
        Assert.False(Sah(new StockRequestPagedQuery { EmergencyReviewDueState = (StockRequestEmergencyReviewDueState)9 }));

        var buat = new CreateStockRequestRequest
        {
            RequestingServiceUnitId = Guid.NewGuid(), StorageLocationId = Guid.NewGuid(),
            RequestedByWorkforceId = Guid.NewGuid(), IdempotencyKey = "uji-enum",
            Items = [new StockRequestItemInput { DrugId = Guid.NewGuid(), MeasurementId = Guid.NewGuid(), RequestedQuantity = 1 }]
        };
        Assert.True(Sah(buat));
        buat.Priority = (StockRequestPriority)9;
        Assert.False(Sah(buat));
    }

    [Fact]
    public async Task Saringan_enum_tak_dikenal_pada_service_menghasilkan_daftar_kosong_bukan_tafsiran_lain()
    {
        using var h = new StockRequestApprovalHarness();
        await h.PermintaanAsync(unitId: h.UnitId);
        await h.PermintaanAsync(unitId: h.UnitLainId);
        var layanan = h.Layanan(h.CreateContext(), h.AtasanUserId);

        // Dulu nilai tak dikenal ditafsirkan sebagai "tidak memerlukan verifikasi" (2 baris).
        var keadaanAsing = await layanan.GetPagedAsync(new StockRequestPagedQuery
        {
            EmergencyReviewState = (StockRequestEmergencyReviewState)7
        });
        Assert.Equal(0, keadaanAsing.TotalData);

        // Cakupan tak dikenal tidak boleh jatuh ke "semua unit".
        var cakupanAsing = await layanan.GetPagedAsync(new StockRequestPagedQuery { Scope = (StockRequestListScope)9 });
        Assert.Equal(0, cakupanAsing.TotalData);

        var tidakPerlu = await layanan.GetPagedAsync(new StockRequestPagedQuery
        {
            EmergencyReviewState = StockRequestEmergencyReviewState.NotRequired
        });
        Assert.Equal(2, tidakPerlu.TotalData);
    }

    // ------------------------------------------------------------- alasan CITO pemohon

    [Fact]
    public async Task Detail_CITO_menyampaikan_alasan_pemohon_kepada_pengotorisasi()
    {
        using var h = new StockRequestApprovalHarness();
        var (cito, _) = await h.PermintaanAsync(status: StockRequestStatus.Draft, prioritas: StockRequestPriority.Emergency);
        var (rutin, _) = await h.PermintaanAsync();

        await h.Layanan(h.CreateContext(), h.PemohonUserId).SubmitAsync(cito, new SubmitStockRequestRequest
        {
            EmergencyReason = "  Stok adrenalin IGD habis, dua pasien syok anafilaksis  ",
            ExpectedVersion = Versi, IdempotencyKey = "uji-alasan-cito"
        });

        var detail = await h.Layanan(h.CreateContext(), h.PetugasCitoUserId).GetDetailAsync(cito);
        Assert.Equal("Stok adrenalin IGD habis, dua pasien syok anafilaksis", detail!.EmergencyRequestReason);

        Assert.Null((await h.Layanan(h.CreateContext(), h.PetugasCitoUserId).GetDetailAsync(rutin))!.EmergencyRequestReason);

        using var k = h.CreateContext();
        Assert.Equal(StockRequestStatus.Submitted,
            (await k.PhmStockRequests.AsNoTracking().SingleAsync(x => x.Id == cito)).Status);
    }
}
