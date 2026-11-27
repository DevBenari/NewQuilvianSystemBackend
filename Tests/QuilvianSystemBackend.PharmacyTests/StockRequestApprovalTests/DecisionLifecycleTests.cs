using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using static QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests.Perintah;

namespace QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests;

/// <summary>
/// Regresi FIN-LGS-015: <c>DecidedAt</c> dan <c>DecisionReason</c> adalah keputusan atasan unit
/// (LGS-DEC-002) dan tidak lagi ditimpa penyerahan maupun pembatalan. Waktu penyerahan dan
/// pembatalan dibaca dari riwayat sebagai <c>FulfilledAt</c> dan <c>CancelledAt</c>.
/// </summary>
public class DecisionLifecycleTests
{
    private static async Task<PhmStockRequest> TersimpanAsync(StockRequestApprovalHarness h, Guid id)
    {
        using var k = h.CreateContext();
        return await k.PhmStockRequests.AsNoTracking().SingleAsync(x => x.Id == id);
    }

    private static async Task<DateTime> WaktuRiwayatAsync(StockRequestApprovalHarness h, Guid id, string aksi)
    {
        using var k = h.CreateContext();
        return (await k.PhmStockRequestHistories.AsNoTracking()
            .SingleAsync(x => x.StockRequestId == id && x.Action == aksi)).OccurredAt;
    }

    [Fact]
    public async Task Penyerahan_tidak_menimpa_waktu_dan_catatan_persetujuan()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, itemId) = await h.PermintaanAsync();
        var disetujui = await h.Layanan(h.CreateContext(), h.AtasanUserId)
            .ApproveAsync(id, Setujui("uji-putus-setuju", catatan: "Stok depo menipis"));
        var waktuSetuju = await WaktuRiwayatAsync(h, id, "ApproveStockRequest");

        var selesai = await h.Layanan(h.CreateContext(), h.StafUserId).FulfillAsync(id, new FulfillStockRequestRequest
        {
            Items = [new FulfillStockRequestItemInput { StockRequestItemId = itemId, FulfilledQuantity = 4m }],
            Note = "Diambil petugas depo",
            ExpectedVersion = disetujui.Version,
            IdempotencyKey = "uji-putus-serah"
        });

        var tersimpan = await TersimpanAsync(h, id);
        Assert.Equal((DateTime?)waktuSetuju, tersimpan.DecidedAt);
        Assert.Equal("Stok depo menipis", tersimpan.DecisionReason);

        Assert.Equal((DateTime?)waktuSetuju, selesai.DecidedAt);
        Assert.Equal(selesai.DecisionAt, selesai.DecidedAt);
        Assert.NotNull(selesai.FulfilledAt);
        Assert.True(selesai.FulfilledAt >= selesai.DecidedAt);
        Assert.Null(selesai.CancelledAt);

        // Catatan penyerahan tetap tercatat — di riwayat, bukan di kolom keputusan.
        Assert.Equal("Diambil petugas depo",
            Assert.Single(selesai.Histories, x => x.Action == "FulfillStockRequest").Reason);

        // Angka diminta, diserahkan, dan sisa konsisten.
        var baris = Assert.Single(selesai.Items);
        Assert.Equal(StockRequestApprovalHarness.JumlahDiminta, baris.RequestedQuantity);
        Assert.Equal((decimal?)4m, baris.FulfilledQuantity);
        Assert.Equal(baris.RequestedQuantity - baris.FulfilledQuantity, baris.RemainingQuantity);
    }

    [Fact]
    public async Task Pembatalan_setelah_disetujui_tidak_menimpa_keputusan_atasan()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();
        var disetujui = await h.Layanan(h.CreateContext(), h.AtasanUserId)
            .ApproveAsync(id, Setujui("uji-putus-setuju-batal", catatan: "Disetujui"));
        var waktuSetuju = await WaktuRiwayatAsync(h, id, "ApproveStockRequest");

        var batal = await h.Layanan(h.CreateContext(), h.PemohonUserId).CancelAsync(id, new CancelStockRequestRequest
        {
            Reason = "Pasien sudah pulang", ExpectedVersion = disetujui.Version, IdempotencyKey = "uji-putus-batal"
        });

        var tersimpan = await TersimpanAsync(h, id);
        Assert.Equal(StockRequestStatus.Cancelled, tersimpan.Status);
        Assert.Equal((DateTime?)waktuSetuju, tersimpan.DecidedAt);
        Assert.Equal("Disetujui", tersimpan.DecisionReason);
        Assert.Equal((DateTime?)await WaktuRiwayatAsync(h, id, "CancelStockRequest"), batal.CancelledAt);
        Assert.Equal("Pasien sudah pulang",
            Assert.Single(batal.Histories, x => x.Action == "CancelStockRequest").Reason);
    }

    [Fact]
    public async Task Pembatalan_sebelum_diputus_tidak_mengisi_waktu_keputusan()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();

        var batal = await h.Layanan(h.CreateContext(), h.PemohonUserId).CancelAsync(id, new CancelStockRequestRequest
        {
            Reason = "Salah pilih gudang", ExpectedVersion = Versi, IdempotencyKey = "uji-putus-batal-awal"
        });

        var tersimpan = await TersimpanAsync(h, id);
        Assert.Null(tersimpan.DecidedAt);
        Assert.Null(tersimpan.DecisionReason);
        Assert.Null(batal.DecisionAt);
        Assert.NotNull(batal.CancelledAt);
    }

    [Fact]
    public async Task Penolakan_mengisi_waktu_dan_alasan_keputusan()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();

        var ditolak = await h.Layanan(h.CreateContext(), h.AtasanUserId)
            .RejectAsync(id, Tolak("uji-putus-tolak", alasan: "Stok depo masih cukup"));

        var tersimpan = await TersimpanAsync(h, id);
        Assert.Equal((DateTime?)await WaktuRiwayatAsync(h, id, "RejectStockRequest"), tersimpan.DecidedAt);
        Assert.Equal("Stok depo masih cukup", tersimpan.DecisionReason);
        Assert.Equal(ditolak.DecisionAt, ditolak.DecidedAt);
    }

    [Fact]
    public async Task Penyerahan_lewat_aturan_peralihan_tidak_mengarang_keputusan_atasan()
    {
        using var h = new StockRequestApprovalHarness();
        var batas = DateTime.UtcNow.AddDays(-1).ToString("O");
        var (id, itemId) = await h.PermintaanAsync(diajukanPada: DateTime.UtcNow.AddDays(-6));

        var selesai = await h.Layanan(h.CreateContext(), h.StafUserId, batas)
            .FulfillAsync(id, Serahkan(itemId, Versi, "uji-putus-peralihan"));

        var tersimpan = await TersimpanAsync(h, id);
        Assert.Equal(StockRequestStatus.Completed, tersimpan.Status);
        Assert.Null(tersimpan.DecidedAt);
        Assert.Null(selesai.DecisionAt);
        Assert.NotNull(selesai.FulfilledAt);
        Assert.Equal((decimal?)0m, Assert.Single(selesai.Items).RemainingQuantity);
    }
}
