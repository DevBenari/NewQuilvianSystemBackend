using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;
using QuilvianSystemBackend.Tests.PatientManagement.MigrasiMassal.Infrastruktur;

namespace QuilvianSystemBackend.Tests.PatientManagement.MigrasiMassal;

/// <summary>
/// Masukan, gerbang environment, hak akses, dan plumbing controller — `PAT-MIG2-AC-01`, `02`, `21`,
/// `31`, `37`–`40`; kontrak `1.3.0` B2, B7, B8.
/// </summary>
public class ValidasiPermintaanDanIzinTests
{
    private const string RuteDisetujui =
        "api/v1/health-services/patient-management/master-data/patients/admin/migration/rsmmc-bulk-patients/migrate";

    [Fact]
    public async Task TanpaDryRunDanLimit_BawaanSimulasiDanLimit25()
    {
        using var h = new MigrasiMassalHarness();
        var sebelum = h.SnapshotDatabase();

        var hasil = await h.JalankanPermintaanAsync(
            new RsmmcBulkPatientMigrationRequest { BatchId = MigrasiMassalHarness.BatchKanonik, SelectionMode = "CANARY_A" },
            MigrasiMassalHarness.AktorUji);

        Assert.Equal(RsmmcBulkPatientMigrationOutcome.Completed, hasil.Outcome);
        Assert.True(hasil.Data!.DryRun);
        Assert.Equal(25, hasil.Data.Limit);
        Assert.Equal(sebelum, h.SnapshotDatabase());
        Assert.Equal(0, h.Kendali.JumlahInsertCrosswalk);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("WAVE")]
    [InlineData("CANARY_B")]
    [InlineData("canary_a")]
    [InlineData(" CANARY_A")]
    public async Task SelectionModeSelainCanaryA_Ditolak400SebelumMembaca(string? mode)
    {
        using var h = new MigrasiMassalHarness();

        var hasil = await h.JalankanPermintaanAsync(
            new RsmmcBulkPatientMigrationRequest { BatchId = MigrasiMassalHarness.BatchKanonik, SelectionMode = mode, DryRun = true },
            MigrasiMassalHarness.AktorUji);

        Assert.Equal(RsmmcBulkPatientMigrationOutcome.ValidationFailed, hasil.Outcome);
        Assert.Equal("RSMMC_BULK_SELECTION_MODE_NOT_SUPPORTED", hasil.ErrorCode);
        Assert.Equal(0, h.Kendali.JumlahPembacaan);
        Assert.Empty(h.Perintah.Teks);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(-1, false)]
    [InlineData(26, false)]
    [InlineData(100, true)]
    public async Task LimitDiLuar1Sampai25_Ditolak400SebelumMembaca(int limit, bool dryRun)
    {
        using var h = new MigrasiMassalHarness();

        var hasil = await h.JalankanAsync(dryRun, limit);

        Assert.Equal(RsmmcBulkPatientMigrationOutcome.ValidationFailed, hasil.Outcome);
        Assert.Equal("RSMMC_BULK_LIMIT_OUT_OF_RANGE", hasil.ErrorCode);
        Assert.Equal(0, h.Kendali.JumlahPembacaan);
        Assert.Empty(h.Perintah.Teks);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(null)]
    public async Task LimitBatasSahDanBawaan_Diterima(int? limit)
    {
        using var h = new MigrasiMassalHarness();

        var hasil = await h.JalankanAsync(true, limit);

        Assert.Equal(RsmmcBulkPatientMigrationOutcome.Completed, hasil.Outcome);
        Assert.Equal(limit ?? 25, hasil.Data!.Limit);
    }

    [Fact]
    public async Task BatchIdTidakDikirimAtauKosong_Ditolak400()
    {
        using var h = new MigrasiMassalHarness();

        foreach (var batchId in new Guid?[] { null, Guid.Empty })
        {
            var hasil = await h.JalankanPermintaanAsync(
                new RsmmcBulkPatientMigrationRequest { BatchId = batchId, SelectionMode = "CANARY_A" },
                MigrasiMassalHarness.AktorUji);

            Assert.Equal(RsmmcBulkPatientMigrationOutcome.ValidationFailed, hasil.Outcome);
            Assert.Equal("RSMMC_BULK_BATCH_ID_REQUIRED", hasil.ErrorCode);
        }

        Assert.Equal(0, h.Kendali.JumlahPembacaan);
    }

    [Theory]
    [InlineData("staging", true)]
    [InlineData("staging", false)]
    [InlineData("STAGING", false)]
    [InlineData("Production", true)]
    [InlineData("Production", false)]
    [InlineData("Development", true)]
    [InlineData("Development", false)]
    [InlineData("", true)]
    [InlineData(" Staging", false)]
    [InlineData("Staging ", true)]
    [InlineData("Staging-Canary", false)]
    public async Task LingkunganSelainStagingPersis_Ditolak403SebelumSatuTabelPunDibaca(string lingkungan, bool dryRun)
    {
        using var h = new MigrasiMassalHarness { EnvironmentName = lingkungan };
        var database = h.SnapshotDatabase();
        var filesystem = h.SnapshotFilesystem();

        var hasil = await h.JalankanAsync(dryRun);

        Assert.Equal(RsmmcBulkPatientMigrationOutcome.EnvironmentRejected, hasil.Outcome);
        Assert.Equal("RSMMC_BULK_ENVIRONMENT_NOT_ALLOWED", hasil.ErrorCode);
        Assert.Null(hasil.Data);

        // Tidak ada pembacaan tabel migrasi, tidak ada perintah EF (termasuk pemeriksa guard), tidak ada
        // folder penyimpanan yang diminta.
        Assert.Equal(0, h.Kendali.JumlahPembacaan);
        Assert.Empty(h.Perintah.Teks);
        Assert.Equal(0, h.JumlahResolveStorage);
        Assert.Equal(database, h.SnapshotDatabase());
        Assert.Equal(filesystem, h.SnapshotFilesystem());
        Assert.Contains(h.Log.Entri, x => x.Level == Microsoft.Extensions.Logging.LogLevel.Warning);
    }

    [Fact]
    public async Task LingkunganStagingPersis_Diterima()
    {
        using var h = new MigrasiMassalHarness { EnvironmentName = "Staging" };

        var hasil = await h.JalankanAsync(true);

        Assert.Equal(RsmmcBulkPatientMigrationOutcome.Completed, hasil.Outcome);
        Assert.Equal("Staging", hasil.Data!.EnvironmentName);
    }

    [Fact]
    public async Task EksekusiTanpaAktorTerkenali_Ditolak400TanpaPerubahan()
    {
        using var h = new MigrasiMassalHarness();
        var sebelum = h.SnapshotDatabase();

        var hasil = await h.JalankanPermintaanAsync(
            new RsmmcBulkPatientMigrationRequest { BatchId = MigrasiMassalHarness.BatchKanonik, SelectionMode = "CANARY_A", DryRun = false },
            Guid.Empty);

        Assert.Equal(RsmmcBulkPatientMigrationOutcome.ValidationFailed, hasil.Outcome);
        Assert.Equal("RSMMC_BULK_ACTOR_UNKNOWN", hasil.ErrorCode);
        Assert.Equal(0, h.Kendali.JumlahPembacaan);
        Assert.Equal(sebelum, h.SnapshotDatabase());
    }

    [Fact]
    public async Task SimulasiTanpaAktorTerkenali_TetapBerjalan()
    {
        using var h = new MigrasiMassalHarness();

        var hasil = await h.JalankanPermintaanAsync(
            new RsmmcBulkPatientMigrationRequest { BatchId = MigrasiMassalHarness.BatchKanonik, SelectionMode = "CANARY_A" },
            Guid.Empty);

        Assert.Equal(RsmmcBulkPatientMigrationOutcome.Completed, hasil.Outcome);
    }

    [Fact]
    public void Dto_TanpaAtributValidasi_DanTanpaDaftarLegacyPidDariKlien()
    {
        var properti = typeof(RsmmcBulkPatientMigrationRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Validasi berada di service supaya kode galat B2.3 selalu dikembalikan, bukan ModelState bawaan.
        Assert.All(properti, p => Assert.Empty(p.GetCustomAttributes<ValidationAttribute>()));
        Assert.Equal(
            ["BatchId", "DryRun", "Limit", "SelectionMode"],
            properti.Select(p => p.Name).OrderBy(x => x, StringComparer.Ordinal));
    }

    /// <summary>
    /// Respons publik hanya memuat field yang dikunci kontrak `1.3.0` B2.2 — tanpa field ringkasan tambahan.
    /// Daftar disalin dari kontrak, bukan dari DTO.
    /// </summary>
    [Fact]
    public void Respons_FieldPersisKontrakB22_TanpaTambahan()
    {
        string[] ringkasan =
        [
            "batchId", "selectionMode", "dryRun", "limit", "environmentName",
            "manifestId", "manifestVersion", "manifestSha256", "manifestSize",
            "totalInManifest", "alreadyMigrated", "eligibleBeforeRun", "selected",
            "ready", "migrated", "conflict", "failed", "notProcessed", "remaining",
            "stopped", "stopReason", "gates", "items"
        ];
        string[] gerbang = ["code", "description", "expected", "actual", "passed"];
        string[] baris =
        [
            "legacyPid", "sourceGroup", "crosswalkStateBefore", "patientId", "patientCode", "medicalRecordNumber",
            "plannedQrCodePath", "fullName", "hasIdentityNumber", "hasPhoneNumber", "status",
            "historicalQrAtDefaultPath", "historicalQrLength", "historicalQrSha256", "plannedQrExists",
            "conflictType", "failureStage", "message"
        ];

        Assert.Equal(ringkasan.Order(StringComparer.Ordinal), NamaField<RsmmcBulkPatientMigrationResponse>());
        Assert.Equal(gerbang.Order(StringComparer.Ordinal), NamaField<RsmmcBulkGateCheckResponse>());
        Assert.Equal(baris.Order(StringComparer.Ordinal), NamaField<RsmmcBulkPatientMigrationItemResponse>());
    }

    [Fact]
    public void Endpoint_MenuntutLoginDanIzinPatientUpdate()
    {
        var controller = typeof(PatientController);
        var aksi = controller.GetMethod(nameof(PatientController.MigrateRsmmcBulkPatients))!;

        Assert.NotNull(controller.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Null(controller.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Null(aksi.GetCustomAttribute<AllowAnonymousAttribute>());

        var ruteController = controller.GetCustomAttribute<RouteAttribute>()!.Template;
        var post = aksi.GetCustomAttribute<HttpPostAttribute>()!;
        Assert.Equal(RuteDisetujui, $"{ruteController}/{post.Template}");

        var akses = controller.GetCustomAttribute<AccessControllerAttribute>()!;
        var action = aksi.GetCustomAttribute<AccessActionAttribute>()!;
        var permission = aksi.GetCustomAttribute<AccessPermissionAttribute>()!;

        Assert.Equal("Patient", akses.ControllerName);
        Assert.Equal(akses.ControllerName, (string)permission.Arguments![0]!);
        Assert.Equal("Update", (string)permission.Arguments[1]!);
        Assert.Equal((string)permission.Arguments[1]!, action.ActionName);
        Assert.Equal("Migrate RSMMC Bulk Patients", action.DisplayName);
        Assert.Equal(AccessTypes.Update, action.AccessType);
        Assert.True(action.VisibleInRoleAccess);
        Assert.False(action.IsSystemOnly);
    }

    [Fact]
    public void Endpoint_SesudahUpdatePatient_SupayaLabelAksesRoleTidakBerubah()
    {
        var aksiUpdate = typeof(PatientController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => x.GetCustomAttribute<AccessActionAttribute>()?.ActionName == "Update")
            .Select(x => x.Name)
            .ToList();

        Assert.Equal(nameof(PatientController.UpdatePatient), aksiUpdate.First());
        Assert.True(aksiUpdate.IndexOf(nameof(PatientController.MigrateRsmmcBulkPatients)) > 0);
        Assert.Equal(nameof(PatientController.ReconcileRsmmcPilotMrn), aksiUpdate.Last());
    }

    [Fact]
    public async Task Controller_LingkunganDitolak_403DenganKode()
    {
        using var h = new MigrasiMassalHarness { EnvironmentName = "Production" };

        var hasil = await PanggilController(h, new RsmmcBulkPatientMigrationRequest
        {
            BatchId = MigrasiMassalHarness.BatchKanonik,
            SelectionMode = "CANARY_A"
        });

        var objek = Assert.IsType<ObjectResult>(hasil);
        Assert.Equal(StatusCodes.Status403Forbidden, objek.StatusCode);
        var respons = Assert.IsType<ApiResponse<object>>(objek.Value);
        Assert.False(respons.Success);
        Assert.Contains("RSMMC_BULK_ENVIRONMENT_NOT_ALLOWED", respons.Errors!.ToString());
    }

    [Fact]
    public async Task Controller_SelectionModeTidakDidukung_400DenganKode()
    {
        using var h = new MigrasiMassalHarness();

        var hasil = await PanggilController(h, new RsmmcBulkPatientMigrationRequest
        {
            BatchId = MigrasiMassalHarness.BatchKanonik,
            SelectionMode = "WAVE"
        });

        var objek = Assert.IsType<BadRequestObjectResult>(hasil);
        var respons = Assert.IsType<ApiResponse<object>>(objek.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, respons.StatusCode);
        Assert.Contains("RSMMC_BULK_SELECTION_MODE_NOT_SUPPORTED", respons.Errors!.ToString());
    }

    [Fact]
    public async Task Controller_GerbangGagalPadaEksekusi_409DenganDaftarGerbang()
    {
        using var h = new MigrasiMassalHarness();
        h.Profil = h.Profil with { ApprovedPatientBaseline = h.Profil.ApprovedPatientBaseline + 1 };

        var hasil = await PanggilController(h, new RsmmcBulkPatientMigrationRequest
        {
            BatchId = MigrasiMassalHarness.BatchKanonik,
            SelectionMode = "CANARY_A",
            DryRun = false
        });

        var objek = Assert.IsType<ObjectResult>(hasil);
        Assert.Equal(StatusCodes.Status409Conflict, objek.StatusCode);
        var respons = Assert.IsType<ApiResponse<object>>(objek.Value);
        var errors = respons.Errors!;
        Assert.Equal("RSMMC_BULK_GATE_FAILED", errors.GetType().GetProperty("code")!.GetValue(errors));
        var gates = Assert.IsAssignableFrom<IEnumerable<RsmmcBulkGateCheckResponse>>(errors.GetType().GetProperty("gates")!.GetValue(errors));
        Assert.Contains(gates, x => x.Code == "PATIENT_BASELINE" && !x.Passed);
        Assert.Equal(0, h.Kendali.JumlahInsertCrosswalk);
    }

    [Fact]
    public async Task Controller_Simulasi_200DenganRespons()
    {
        using var h = new MigrasiMassalHarness();

        var hasil = await PanggilController(h, new RsmmcBulkPatientMigrationRequest
        {
            BatchId = MigrasiMassalHarness.BatchKanonik,
            SelectionMode = "CANARY_A"
        });

        var objek = Assert.IsType<OkObjectResult>(hasil);
        var respons = Assert.IsType<ApiResponse<RsmmcBulkPatientMigrationResponse>>(objek.Value);
        Assert.True(respons.Success);
        Assert.Equal(25, respons.Data!.Items.Count);
    }

    [Fact]
    public void Controller_PenulisQrBulk_MemakaiNamespaceRsmmcBulk()
    {
        using var r = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        using var konteks = r.Database.CreateContext();
        var controller = r.BuatController(konteks);

        var store = (RsmmcPilotQrArtifactStore)Privat(controller, "CreateRsmmcBulkQrArtifactStore")!;
        var storePilot = (RsmmcPilotQrArtifactStore)Privat(controller, "CreateRsmmcPilotQrArtifactStore")!;

        Assert.Equal("/uploads/patient-qrcodes/rsmmc-bulk/00-12-34-56/qrcode.png", store.BuildPublicPath("00-12-34-56"));

        // Penulis Pilot tetap memakai path bawaan.
        Assert.Equal("/uploads/patient-qrcodes/00-12-34-56/qrcode.png", storePilot.BuildPublicPath("00-12-34-56"));
    }

    [Fact]
    public void Controller_PemeriksaQr_TidakMembuatFolder_SedangkanPenyimpananBiasaTetapMembuat()
    {
        using var r = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        var akarBelumAda = Path.Combine(r.StorageRoot, "belum-ada");
        r.Configuration = ReservasiHarness.BuatKonfigurasi(KonfigurasiGuardUji.EnabledTrue, storageRoot: akarBelumAda);

        using var konteks = r.Database.CreateContext();
        var controller = r.BuatController(konteks);

        var probe = (RsmmcQrDestinationProbe)Privat(controller, "CreateRsmmcQrDestinationProbe")!;
        var hasil = probe.Probe("00-12-34-56");

        Assert.False(hasil.PlannedExists);
        Assert.False(hasil.HistoricalExists);
        Assert.False(Directory.Exists(akarBelumAda));

        // Pemisahan ResolveFileStorageRootPath tidak mengubah GetFileStoragePaths: tetap membuat folder.
        var lokasi = ((string RootPath, string PublicRequestPath))Privat(controller, "GetFileStoragePaths")!;
        Assert.Equal(akarBelumAda, lokasi.RootPath);
        Assert.Equal("/uploads", lokasi.PublicRequestPath);
        Assert.True(Directory.Exists(akarBelumAda));
    }

    private static async Task<IActionResult> PanggilController(MigrasiMassalHarness h, RsmmcBulkPatientMigrationRequest permintaan)
    {
        using var r = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        using var konteks = r.Database.CreateContext();
        var controller = r.BuatController(konteks);

        return await controller.MigrateRsmmcBulkPatients(permintaan, h.BuatService(), CancellationToken.None);
    }

    // Nama field JSON bawaan ASP.NET Core: camelCase dari nama properti publik.
    private static IEnumerable<string> NamaField<T>() =>
        typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => char.ToLowerInvariant(p.Name[0]) + p.Name[1..])
            .Order(StringComparer.Ordinal);

    private static object? Privat(object target, string nama) =>
        target.GetType().GetMethod(nama, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(target, null);
}
