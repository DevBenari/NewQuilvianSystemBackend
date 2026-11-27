using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using static QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests.Perintah;

namespace QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests;

/// <summary>
/// Pengulangan kunci idempotensi pada ketujuh perintah (FIN-LGS-009), jejak audit buat/ubah,
/// dan jaminan penyerahan tidak mengurangi stok dua kali (FIN-LGS-010).
/// </summary>
public class IdempotencyReplayTests
{
    private const string KunciBersama = "uji-kunci-bersama";

    private static StockRequestItemInput Baris(StockRequestApprovalHarness h, decimal jumlah) =>
        new() { DrugId = h.ObatId, MeasurementId = h.SatuanId, RequestedQuantity = jumlah };

    private static CreateStockRequestRequest Buat(StockRequestApprovalHarness h, string kunci) => new()
    {
        RequestingServiceUnitId = h.UnitId,
        StorageLocationId = h.GudangId,
        RequestedByWorkforceId = h.PemohonWorkforceId,
        Priority = StockRequestPriority.Routine,
        Items = [Baris(h, 5m)],
        IdempotencyKey = kunci
    };

    private static UpdateStockRequestRequest Ubah(StockRequestApprovalHarness h, string kunci, int versi) => new()
    {
        StorageLocationId = h.GudangId,
        Priority = StockRequestPriority.Routine,
        Items = [Baris(h, 7m)],
        ExpectedVersion = versi,
        IdempotencyKey = kunci
    };

    /// <summary>
    /// Pelaku A menjalankan perintah dengan satu kunci; pelaku B mengirim kunci yang sama. B harus
    /// ditolak (PHM013) tanpa menerima hasil milik A, dan tidak ada riwayat kedua.
    /// </summary>
    [Theory]
    [InlineData("buat")]
    [InlineData("ubah")]
    [InlineData("ajukan")]
    [InlineData("batal")]
    [InlineData("keputusan")]
    [InlineData("verifikasi")]
    [InlineData("serah")]
    public async Task Kunci_idempotensi_tidak_dapat_diulang_pengguna_lain(string perintah)
    {
        using var h = new StockRequestApprovalHarness();
        await h.SediakanMasterObatAsync();

        Func<Guid, Task> jalankan;
        Guid pelakuA;
        Guid pelakuB;

        switch (perintah)
        {
            case "buat":
                (pelakuA, pelakuB) = (h.PemohonUserId, h.StafUserId);
                jalankan = user => h.Layanan(h.CreateContext(), user).CreateAsync(Buat(h, KunciBersama));
                break;

            case "ubah":
            {
                var (draf, _) = await h.PermintaanAsync(status: StockRequestStatus.Draft);
                (pelakuA, pelakuB) = (h.PemohonUserId, h.StafUserId);
                jalankan = user => h.Layanan(h.CreateContext(), user)
                    .UpdateAsync(draf, Ubah(h, KunciBersama, Versi));
                break;
            }

            case "ajukan":
            {
                var (draf, _) = await h.PermintaanAsync(status: StockRequestStatus.Draft);
                (pelakuA, pelakuB) = (h.PemohonUserId, h.StafUserId);
                jalankan = user => h.Layanan(h.CreateContext(), user).SubmitAsync(draf,
                    new SubmitStockRequestRequest { ExpectedVersion = Versi, IdempotencyKey = KunciBersama });
                break;
            }

            case "batal":
            {
                var (id, _) = await h.PermintaanAsync();
                (pelakuA, pelakuB) = (h.PemohonUserId, h.StafUserId);
                jalankan = user => h.Layanan(h.CreateContext(), user).CancelAsync(id,
                    new CancelStockRequestRequest
                    {
                        Reason = "Kebutuhan sudah terpenuhi", ExpectedVersion = Versi, IdempotencyKey = KunciBersama
                    });
                break;
            }

            case "keputusan":
            {
                var (id, _) = await h.PermintaanAsync();
                (pelakuA, pelakuB) = (h.AtasanUserId, h.AtasanCitoUserId);
                jalankan = user => h.Layanan(h.CreateContext(), user).RejectAsync(id, Tolak(KunciBersama));
                break;
            }

            case "verifikasi":
            {
                var (id, _, versi) = await h.CitoTerotorisasiAsync();
                (pelakuA, pelakuB) = (h.AtasanUserId, h.AtasanCitoUserId);
                jalankan = user => h.Layanan(h.CreateContext(), user)
                    .VerifyEmergencyAsync(id, Verifikasi(KunciBersama, versi));
                break;
            }

            case "serah":
            {
                var (id, itemId) = await h.PermintaanAsync();
                var disetujui = await h.Layanan(h.CreateContext(), h.AtasanUserId)
                    .ApproveAsync(id, Setujui("uji-setuju-sebelum-serah"));
                (pelakuA, pelakuB) = (h.StafUserId, h.AtasanLainUserId);
                jalankan = user => h.Layanan(h.CreateContext(), user)
                    .FulfillAsync(id, Serahkan(itemId, disetujui.Version, KunciBersama));
                break;
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(perintah), perintah, "Perintah uji tidak dikenal.");
        }

        await jalankan(pelakuA);

        var galat = await Assert.ThrowsAsync<StockRequestConflictException>(() => jalankan(pelakuB));
        Assert.Equal("PHM013", galat.Code);
        Assert.Contains("pengguna lain", galat.Message);

        using var k = h.CreateContext();
        Assert.Equal(1, await k.PhmStockRequestHistories.CountAsync(x => x.CorrelationId == KunciBersama));
    }

    [Fact]
    public async Task Pembuatan_dan_pengubahan_draf_tercatat_di_log_audit_dengan_pelaku_yang_benar()
    {
        using var h = new StockRequestApprovalHarness();
        await h.SediakanMasterObatAsync();
        var log = new CatatanLogUji();

        var dibuat = await h.Layanan(h.CreateContext(), h.PemohonUserId, log: log)
            .CreateAsync(Buat(h, "uji-audit-buat"));
        await h.Layanan(h.CreateContext(), h.PemohonUserId, log: log)
            .UpdateAsync(dibuat.Id, Ubah(h, "uji-audit-ubah", dibuat.Version));

        Assert.Equal(h.PemohonUserId.ToString(),
            CatatanLogUji.PelakuPada(Assert.Single(log.UntukAksi("StockRequest.Create")).Pesan));
        Assert.Equal(h.PemohonUserId.ToString(),
            CatatanLogUji.PelakuPada(Assert.Single(log.UntukAksi("StockRequest.Update")).Pesan));
        Assert.DoesNotContain(log.Entri, x => CatatanLogUji.PelakuPada(x.Pesan) == dibuat.Id.ToString());
    }

    [Fact]
    public async Task Penyerahan_mengurangi_stok_sekali_walau_diulang_atau_dikirim_dua_kali()
    {
        using var h = new StockRequestApprovalHarness();
        await h.SediakanStokAsync();
        var (id, itemId) = await h.PermintaanAsync();
        var disetujui = await h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(id, Setujui("uji-stok-setuju"));
        var awal = await h.PotretStokAsync();

        var serah = Serahkan(itemId, disetujui.Version, "uji-stok-serah");
        await h.Layanan(h.CreateContext(), h.StafUserId, denganStok: true).FulfillAsync(id, serah);

        var sesudah = await h.PotretStokAsync();
        Assert.Equal(awal.Saldo - StockRequestApprovalHarness.JumlahDiminta, sesudah.Saldo);
        Assert.True(sesudah.Mutasi > awal.Mutasi);

        // Pengulangan dengan kunci yang sama oleh pelaku yang sama: hasil sama, stok tetap.
        var ulang = await h.Layanan(h.CreateContext(), h.StafUserId, denganStok: true).FulfillAsync(id, serah);
        Assert.Equal(StockRequestStatus.Completed, ulang.Status);
        Assert.Equal(sesudah, await h.PotretStokAsync());

        // Kiriman kedua dengan kunci baru ditolak karena permintaan sudah selesai; stok tetap.
        var galat = await Assert.ThrowsAsync<StockRequestConflictException>(() =>
            h.Layanan(h.CreateContext(), h.StafUserId, denganStok: true)
                .FulfillAsync(id, Serahkan(itemId, ulang.Version, "uji-stok-serah-kedua")));
        Assert.Equal("PHM004", galat.Code);
        Assert.Equal(sesudah, await h.PotretStokAsync());
        Assert.Equal(1, await HitungRiwayatAsync(h, id, "FulfillStockRequest"));
    }
}
