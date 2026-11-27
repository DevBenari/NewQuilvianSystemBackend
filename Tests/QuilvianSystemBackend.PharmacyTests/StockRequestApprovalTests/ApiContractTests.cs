using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using static QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests.Perintah;

namespace QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests;

/// <summary>
/// Integrasi controller dan kontrak data dengan frontend (kasus wajib 15): kode HTTP untuk
/// tidak berwenang/benturan/prasyarat, serta nilai angka dan nama properti yang dibaca layar.
/// </summary>
/// <remarks>
/// Controller dipanggil langsung dengan service sungguhan. Filter <c>[AccessPermission]</c> tidak
/// berjalan di sini; yang diuji adalah pemetaan hasil domain ke HTTP dan bentuk respons.
/// </remarks>
public class ApiContractTests
{
    private static int? KodeHttp(IActionResult hasil) => hasil switch
    {
        ObjectResult objek => objek.StatusCode ?? 200,
        StatusCodeResult kode => kode.StatusCode,
        _ => null
    };

    [Fact]
    public async Task Tidak_berwenang_403_benturan_409_prasyarat_422_berhasil_200()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();

        var tanpaWewenang = await new StockRequestController(h.Layanan(h.CreateContext(), h.StafUserId))
            .Approve(id, Setujui("uji-api-403"), CancellationToken.None);
        Assert.Equal(403, KodeHttp(tanpaWewenang));

        var tanpaAlasan = await new StockRequestController(h.Layanan(h.CreateContext(), h.AtasanUserId))
            .Reject(id, Tolak("uji-api-422", alasan: " "), CancellationToken.None);
        Assert.Equal(422, KodeHttp(tanpaAlasan));

        var berhasil = await new StockRequestController(h.Layanan(h.CreateContext(), h.AtasanUserId))
            .Approve(id, Setujui("uji-api-200"), CancellationToken.None);
        Assert.Equal(200, KodeHttp(berhasil));

        var ganda = await new StockRequestController(h.Layanan(h.CreateContext(), h.AtasanCitoUserId))
            .Approve(id, Setujui("uji-api-409", versi: Versi + 1), CancellationToken.None);
        Assert.Equal(409, KodeHttp(ganda));
    }

    [Fact]
    public async Task Verifikasi_susulan_lewat_controller_memetakan_kode_yang_sama()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _, versi) = await h.CitoTerotorisasiAsync();

        var pengotorisasi = await new StockRequestController(h.Layanan(h.CreateContext(), h.PetugasCitoUserId))
            .VerifyEmergency(id, Verifikasi("uji-api-v403", versi), CancellationToken.None);
        Assert.Equal(403, KodeHttp(pengotorisasi));

        var tanpaCatatan = await new StockRequestController(h.Layanan(h.CreateContext(), h.AtasanUserId))
            .VerifyEmergency(id, Verifikasi("uji-api-v422", versi, StockRequestEmergencyReviewOutcome.Deviation),
                CancellationToken.None);
        Assert.Equal(422, KodeHttp(tanpaCatatan));

        var berhasil = await new StockRequestController(h.Layanan(h.CreateContext(), h.AtasanUserId))
            .VerifyEmergency(id, Verifikasi("uji-api-v200", versi), CancellationToken.None);
        Assert.Equal(200, KodeHttp(berhasil));

        var ulang = await new StockRequestController(h.Layanan(h.CreateContext(), h.AtasanCitoUserId))
            .VerifyEmergency(id, Verifikasi("uji-api-v409", versi + 1), CancellationToken.None);
        Assert.Equal(409, KodeHttp(ulang));
    }

    [Fact]
    public async Task Konfigurasi_tenggat_tidak_sah_tidak_membuat_endpoint_gagal_500()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _, _) = await h.CitoTerotorisasiAsync();

        // "1e300" jam: tanpa batas teknis, TimeSpan.FromHours meluap saat service dibuat dan
        // seluruh endpoint permintaan stok menjawab 500.
        var controller = new StockRequestController(
            h.Layanan(h.CreateContext(), h.AtasanUserId, tenggatJam: "1e300", peringatanJam: "4"));

        var daftar = await controller.GetPaged(new StockRequestPagedQuery
        {
            EmergencyReviewDueState = StockRequestEmergencyReviewDueState.Overdue
        }, CancellationToken.None);
        Assert.Equal(200, KodeHttp(daftar));

        var detail = await controller.GetById(id, CancellationToken.None);
        Assert.Equal(200, KodeHttp(detail));
    }

    [Fact]
    public void Nilai_angka_status_dan_penanda_terkunci_sesuai_konstanta_frontend()
    {
        // Frontend (stock-request-constant.jsx) membaca angka-angka ini apa adanya.
        Assert.Equal(1, (int)StockRequestStatus.Draft);
        Assert.Equal(2, (int)StockRequestStatus.Submitted);
        Assert.Equal(3, (int)StockRequestStatus.Approved);
        Assert.Equal(4, (int)StockRequestStatus.Completed);
        Assert.Equal(5, (int)StockRequestStatus.Rejected);
        Assert.Equal(6, (int)StockRequestStatus.Cancelled);
        Assert.Equal(1, (int)StockRequestPriority.Routine);
        Assert.Equal(2, (int)StockRequestPriority.Urgent);
        Assert.Equal(3, (int)StockRequestPriority.Emergency);

        Assert.Equal(0, (int)StockRequestEmergencyReviewState.NotRequired);
        Assert.Equal(1, (int)StockRequestEmergencyReviewState.Pending);
        Assert.Equal(2, (int)StockRequestEmergencyReviewState.Confirmed);
        Assert.Equal(3, (int)StockRequestEmergencyReviewState.Deviation);

        Assert.Equal(1, (int)StockRequestEmergencyReviewOutcome.Confirmed);
        Assert.Equal(2, (int)StockRequestEmergencyReviewOutcome.Deviation);

        // STOCK_REQUEST_REVIEW_DUE_STATE dan STOCK_REQUEST_LIST_SCOPE di frontend.
        Assert.Equal(0, (int)StockRequestEmergencyReviewDueState.NotApplicable);
        Assert.Equal(1, (int)StockRequestEmergencyReviewDueState.NotConfigured);
        Assert.Equal(2, (int)StockRequestEmergencyReviewDueState.NotDue);
        Assert.Equal(3, (int)StockRequestEmergencyReviewDueState.DueSoon);
        Assert.Equal(4, (int)StockRequestEmergencyReviewDueState.Overdue);

        Assert.Equal(0, (int)StockRequestListScope.All);
        Assert.Equal(1, (int)StockRequestListScope.MyAuthority);
    }

    [Fact]
    public async Task Respons_detail_memakai_nama_properti_yang_dibaca_frontend()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _, _) = await h.CitoTerotorisasiAsync();
        var detail = await h.Layanan(h.CreateContext(), h.AtasanUserId).GetDetailAsync(id);

        // Serializer ASP.NET Core bawaan (tanpa konverter enum teks, lihat Program.cs).
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(detail,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        var akar = json.RootElement;

        Assert.Equal(3, akar.GetProperty("status").GetInt32());
        Assert.Equal(1, akar.GetProperty("emergencyReviewState").GetInt32());
        Assert.True(akar.GetProperty("canVerifyEmergency").GetBoolean());
        Assert.True(akar.GetProperty("canFulfill").GetBoolean());
        Assert.Equal(JsonValueKind.Object, akar.GetProperty("emergencyAuthorization").ValueKind);
        Assert.Equal(JsonValueKind.Null, akar.GetProperty("emergencyReview").ValueKind);

        foreach (var nama in new[]
                 {
                     "hasApproveAuthority",
                     "hasEmergencyAuthority", "verifyEmergencyMessage", "isOwnRequest",
                     "isLegacySubmission", "canApprove", "canAuthorizeEmergency", "version", "histories",
                     "emergencyRequestReason", "emergencyReviewDueState", "emergencyReviewDueAt",
                     "emergencyAuthorizedByName"
                 })
            Assert.True(akar.TryGetProperty(nama, out _), $"properti {nama} harus ada");

        // FIN-LGS-070: penanda atasan unit dihapus dari kontrak; kewenangan hanya dari izin aktif.
        foreach (var nama in new[] { "hasUnitAuthority", "unitAuthorityMessage" })
            Assert.False(akar.TryGetProperty(nama, out _), $"properti {nama} tidak boleh ada lagi");
    }
}
