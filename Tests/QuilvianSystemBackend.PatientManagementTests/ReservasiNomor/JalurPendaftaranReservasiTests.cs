using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;

namespace QuilvianSystemBackend.Tests.PatientManagement.ReservasiNomor;

/// <summary>
/// Ketiga jalur pendaftaran pasien normal memakai allocator yang sama dan tidak dapat
/// melewatinya — `PAT-MIG3-AC-01`, `02`, `09`, `14`; serta endpoint pemeriksa `PAT-MIG3-AC-07`.
/// </summary>
/// <remarks>
/// <c>POST /</c> dan <c>POST /kiosk</c> adalah dua rute pada action yang sama
/// (<c>CreatePatient</c>); <c>POST /admin</c> adalah <c>CreatePatientForAdmin</c> yang meneruskan ke
/// action itu. Uji ini memanggil kedua action secara langsung, di bawah lapisan otorisasi rute,
/// sehingga perlindungannya terbukti tidak bergantung pada policy <c>KioskRead</c> maupun izin
/// <c>Patient : Create</c>.
/// </remarks>
public class JalurPendaftaranReservasiTests
{
    // ---- Peta rute (L, J, K) --------------------------------------------------------------

    [Fact]
    public void KetigaRutePendaftaran_BermuaraPadaDuaActionYangDiketahui()
    {
        var create = Method(nameof(PatientController.CreatePatient));
        var admin = Method(nameof(PatientController.CreatePatientForAdmin));

        var ruteCreate = create.GetCustomAttributes<HttpPostAttribute>().Select(x => x.Template).ToList();
        var ruteAdmin = admin.GetCustomAttributes<HttpPostAttribute>().Select(x => x.Template).ToList();

        Assert.Equal(2, ruteCreate.Count);
        Assert.Contains(null, ruteCreate);
        Assert.Contains("kiosk", ruteCreate);
        Assert.Equal(["admin"], ruteAdmin);

        // Seluruh rute POST lain pada controller bukan pendaftaran pasien umum.
        var postLain = typeof(PatientController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m != create && m != admin && m.GetCustomAttributes<HttpPostAttribute>().Any())
            .Select(m => m.Name)
            .OrderBy(x => x)
            .ToList();
        Assert.Equal([nameof(PatientController.CreateNewbornPatient), nameof(PatientController.ReconcileRsmmcPilotMrn)], postLain);
    }

    [Fact]
    public void Controller_TidakLagiMemilikiPembangkitNomorLama()
    {
        var nama = typeof(PatientController)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Select(m => m.Name)
            .ToList();

        Assert.DoesNotContain("GeneratePatientCodeAsync", nama);
        Assert.DoesNotContain("GenerateRunningCodeAsync", nama);
        Assert.DoesNotContain("GenerateMedicalRecordNumberAsync", nama);
        Assert.DoesNotContain(nama, n => n.StartsWith("Generate", StringComparison.Ordinal) &&
            (n.Contains("Code", StringComparison.Ordinal) || n.Contains("Number", StringComparison.Ordinal)));
    }

    // ---- Staging persis + Enabled = true ---------------------------------------------------

    [Fact]
    public async Task RuteBiasaDanKiosk_StagingAktif_MemakaiLantai()
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        h.TambahPasien("PAT-RSMMC-00742", "00-00-00-27");

        var pertama = await DaftarAsync(h, admin: false);
        var kedua = await DaftarAsync(h, admin: false);

        Assert.Equal(StatusCodes.Status200OK, pertama.Status);
        Assert.Equal(StatusCodes.Status200OK, kedua.Status);
        Assert.Equal(["PAT-RSMMC-00742", "PAT-RSMMC-705250", "PAT-RSMMC-705251"], KodeTersimpan(h));
        Assert.Equal(["00-00-00-27", "00-89-01-88", "00-89-01-89"], MrnTersimpan(h));
    }

    [Fact]
    public async Task RuteAdmin_StagingAktif_MemakaiLantai()
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);

        var hasil = await DaftarAsync(h, admin: true);

        Assert.Equal(StatusCodes.Status200OK, hasil.Status);
        Assert.Equal(["PAT-RSMMC-705250"], KodeTersimpan(h));
        Assert.Equal(["00-89-01-88"], MrnTersimpan(h));
    }

    [Fact]
    public async Task StagingAktif_QrNormalDitulisKeFolderMrnBaru_BukanFolderMrnCadangan()
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);

        await DaftarAsync(h, admin: false);

        Assert.True(File.Exists(Path.Combine(h.StorageRoot, "patient-qrcodes", "00-89-01-88", "qrcode.png")));
        Assert.False(Directory.Exists(Path.Combine(h.StorageRoot, "patient-qrcodes", "00-00-00-27")));
    }

    // ---- F, G, H lewat ketiga rute: gagal tertutup sebelum mutasi -------------------------

    [Theory]
    [InlineData(KonfigurasiGuardUji.EnabledFalse, false, "DISABLED")]
    [InlineData(KonfigurasiGuardUji.EnabledFalse, true, "DISABLED")]
    [InlineData(KonfigurasiGuardUji.BagianTidakAda, false, "CONFIG_MISSING")]
    [InlineData(KonfigurasiGuardUji.BagianTidakAda, true, "CONFIG_MISSING")]
    [InlineData(KonfigurasiGuardUji.KunciEnabledTidakAda, false, "CONFIG_MISSING")]
    [InlineData(KonfigurasiGuardUji.EnabledSalahBentuk, false, "CONFIG_MALFORMED")]
    [InlineData(KonfigurasiGuardUji.EnabledSalahBentuk, true, "CONFIG_MALFORMED")]
    public async Task StagingTidakSah_Ditolak503_SebelumInsertQrDanFoto(
        KonfigurasiGuardUji konfigurasi,
        bool admin,
        string keadaan)
    {
        using var h = new ReservasiHarness("Staging", konfigurasi);
        h.TambahPasien("PAT-RSMMC-00001", "00-00-00-01");
        var pasienSebelum = h.JumlahPasien();
        var berkasSebelum = h.JumlahBerkasUnggahan();

        // Foto ikut dikirim untuk membuktikan foto pun tidak ditulis.
        var hasil = await DaftarAsync(h, admin, denganFoto: true);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, hasil.Status);
        Assert.Equal("PATIENT_RESERVATION_GUARD_CONFIG_INVALID", Properti(hasil.Body?.Errors, "code"));
        Assert.Equal(keadaan, Properti(hasil.Body?.Errors, "guardState"));
        Assert.Equal(pasienSebelum, h.JumlahPasien());
        Assert.Equal(berkasSebelum, h.JumlahBerkasUnggahan());
        Assert.False(Directory.Exists(Path.Combine(h.StorageRoot, "patient-qrcodes")));
        Assert.False(Directory.Exists(Path.Combine(h.StorageRoot, "patient-photos")));
    }

    // ---- Di luar Staging persis: perilaku lama -----------------------------------------------

    [Theory]
    [InlineData("Production", false)]
    [InlineData("Production", true)]
    [InlineData("Development", false)]
    public async Task BukanStaging_PenomoranLamaTetap(string environmentName, bool admin)
    {
        using var h = new ReservasiHarness(environmentName, KonfigurasiGuardUji.BagianTidakAda);
        h.TambahPasien("PAT-RSMMC-00001", "00-00-00-01");

        var hasil = await DaftarAsync(h, admin);

        Assert.Equal(StatusCodes.Status200OK, hasil.Status);
        Assert.Equal(["PAT-RSMMC-00001", "PAT-RSMMC-00002"], KodeTersimpan(h));
        Assert.Equal(["00-00-00-01", "00-00-00-02"], MrnTersimpan(h));
    }

    // ---- Endpoint pemeriksa (PAT-MIG3-AC-07) ---------------------------------------------

    [Fact]
    public void Pemeriksa_RuteDanIzinSesuaiKontrak()
    {
        var method = Method(nameof(PatientController.GetRsmmcReservationGuardStatus));

        Assert.Equal(
            "admin/migration/rsmmc-reservation-guard",
            method.GetCustomAttribute<HttpGetAttribute>()?.Template);

        var izin = method.GetCustomAttribute<AccessPermissionAttribute>();
        var aksi = method.GetCustomAttribute<AccessActionAttribute>();
        var controller = typeof(PatientController).GetCustomAttribute<AccessControllerAttribute>();

        Assert.NotNull(izin);
        Assert.NotNull(aksi);
        Assert.NotNull(controller);
        Assert.Equal("Patient", controller!.ControllerName);
        Assert.Equal(controller.ControllerName, (string)izin!.Arguments![0]!);
        Assert.Equal("Update", (string)izin.Arguments[1]!);
        Assert.Equal("Update", aksi!.ActionName);
        Assert.Equal(AccessTypes.Update, aksi.AccessType);
        Assert.Null(method.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    [Fact]
    public async Task Pemeriksa_BukanStaging_Ditolak403()
    {
        using var h = new ReservasiHarness("Production", KonfigurasiGuardUji.EnabledTrue);
        await using var konteks = h.Database.CreateContext();

        var hasil = await h.BuatController(konteks).GetRsmmcReservationGuardStatus(CancellationToken.None);

        var objek = Assert.IsAssignableFrom<ObjectResult>(hasil);
        Assert.Equal(StatusCodes.Status403Forbidden, objek.StatusCode);
        Assert.Equal(
            "RSMMC_RESERVATION_GUARD_ENVIRONMENT_NOT_ALLOWED",
            Properti(((ApiResponse<object>)objek.Value!).Errors, "code"));
    }

    [Fact]
    public async Task Pemeriksa_StagingAktif_SiapTanpaMutasi()
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        h.TambahPasien("PAT-RSMMC-00742", "00-00-00-27");
        var sebelum = h.JumlahPasien();
        await using var konteks = h.Database.CreateContext();

        var hasil = await h.BuatController(konteks).GetRsmmcReservationGuardStatus(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(hasil);
        var data = ((ApiResponse<RsmmcReservationGuardStatusResponse>)ok.Value!).Data!;
        Assert.True(data.OverallGuardReady);
        Assert.Equal("PAT-RSMMC-705250", data.NextPatientCodeFormatted);
        Assert.Equal("00-89-01-88", data.NextMrnFormatted);
        Assert.Equal(sebelum, h.JumlahPasien());
        Assert.Equal(0, h.JumlahBerkasUnggahan());
    }

    [Fact]
    public async Task Pemeriksa_StagingEnabledFalse_BelumSiap()
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledFalse);
        await using var konteks = h.Database.CreateContext();

        var hasil = await h.BuatController(konteks).GetRsmmcReservationGuardStatus(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(hasil);
        var data = ((ApiResponse<RsmmcReservationGuardStatusResponse>)ok.Value!).Data!;
        Assert.Equal("DISABLED", data.GuardState);
        Assert.False(data.GuardConfigurationValid);
        Assert.False(data.OverallGuardReady);
    }

    // ---- Pembantu ---------------------------------------------------------------------

    private sealed record HasilDaftar(int Status, ApiResponse<object>? Body);

    private static async Task<HasilDaftar> DaftarAsync(ReservasiHarness h, bool admin, bool denganFoto = false)
    {
        await using var konteks = h.Database.CreateContext();
        var controller = h.BuatController(konteks);

        var request = new CreatePatientRequest
        {
            FullName = "Pasien Normal Uji",
            BirthDate = new DateTime(1990, 5, 17, 0, 0, 0, DateTimeKind.Utc),
            PhotoBase64 = denganFoto ? FotoPngSatuPiksel : null
        };

        var hasil = admin
            ? await controller.CreatePatientForAdmin(request)
            : await controller.CreatePatient(request);

        var objek = Assert.IsAssignableFrom<ObjectResult>(hasil);

        return new HasilDaftar(objek.StatusCode ?? StatusCodes.Status200OK, objek.Value as ApiResponse<object>);
    }

    // PNG 1x1 piksel yang sah.
    private const string FotoPngSatuPiksel =
        "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==";

    private static List<string> KodeTersimpan(ReservasiHarness h)
    {
        using var konteks = h.Database.CreateContext();

        return konteks.Set<MstPatient>()
            .IgnoreQueryFilters()
            .Select(x => x.PatientCode)
            .AsEnumerable()
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();
    }

    private static List<string> MrnTersimpan(ReservasiHarness h)
    {
        using var konteks = h.Database.CreateContext();

        return konteks.Set<MstPatient>()
            .IgnoreQueryFilters()
            .Select(x => x.MedicalRecordNumber)
            .AsEnumerable()
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();
    }

    private static MethodInfo Method(string nama) =>
        typeof(PatientController).GetMethod(nama, BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Method {nama} tidak ditemukan.");

    private static string? Properti(object? objek, string nama) =>
        objek?.GetType().GetProperty(nama)?.GetValue(objek)?.ToString();
}
