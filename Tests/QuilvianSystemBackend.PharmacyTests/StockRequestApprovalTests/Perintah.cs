using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

namespace QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests;

/// <summary>Pembentuk perintah dan pemeriksaan bersama untuk uji persetujuan permintaan stok.</summary>
internal static class Perintah
{
    public const int Versi = StockRequestApprovalHarness.VersiAwal;

    public static ApproveStockRequestRequest Setujui(string kunci, int versi = Versi, string? catatan = null) =>
        new() { Note = catatan, ExpectedVersion = versi, IdempotencyKey = kunci };

    public static RejectStockRequestRequest Tolak(string kunci, string alasan = "Stok depo masih cukup",
        int versi = Versi) =>
        new() { Reason = alasan, ExpectedVersion = versi, IdempotencyKey = kunci };

    public static AuthorizeEmergencyStockRequestRequest Cito(string kunci,
        string alasan = "Pasien IGD, stok depo habis", int versi = Versi) =>
        new() { Reason = alasan, ExpectedVersion = versi, IdempotencyKey = kunci };

    public static VerifyEmergencyStockRequestRequest Verifikasi(string kunci, int versi,
        StockRequestEmergencyReviewOutcome hasil = StockRequestEmergencyReviewOutcome.Confirmed,
        string? catatan = null) =>
        new() { Outcome = hasil, Note = catatan, ExpectedVersion = versi, IdempotencyKey = kunci };

    public static FulfillStockRequestRequest Serahkan(Guid itemId, int versi, string kunci,
        decimal jumlah = StockRequestApprovalHarness.JumlahDiminta) => new()
        {
            Items = [new FulfillStockRequestItemInput { StockRequestItemId = itemId, FulfilledQuantity = jumlah }],
            ExpectedVersion = versi,
            IdempotencyKey = kunci
        };

    /// <summary>Permintaan masih Menunggu Persetujuan dengan versi awal: tidak ada yang berubah.</summary>
    public static async Task AssertMasihMenungguAsync(StockRequestApprovalHarness h, Guid id)
    {
        using var k = h.CreateContext();
        var tersimpan = await k.PhmStockRequests.AsNoTracking().SingleAsync(x => x.Id == id);
        Assert.Equal(StockRequestStatus.Submitted, tersimpan.Status);
        Assert.Equal(Versi, tersimpan.Version);
    }

    public static async Task<int> HitungRiwayatAsync(StockRequestApprovalHarness h, Guid id, string aksi)
    {
        using var k = h.CreateContext();
        return await k.PhmStockRequestHistories.CountAsync(x => x.StockRequestId == id && x.Action == aksi);
    }
}
