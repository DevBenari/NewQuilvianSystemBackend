using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;

namespace QuilvianSystemBackend.Tests.PatientManagement;

/// <summary>
/// Validasi masukan, gerbang environment, dan kontrak izin endpoint — `AC-05`, `AC-06`,
/// `PAT-OQ-002`, `PAT-OQ-006`.
/// </summary>
public class ValidasiLingkunganDanIzinTests
{
    private const string RuteDisetujui =
        "api/v1/health-services/patient-management/master-data/patients/admin/migration/rsmmc-pilot-mrn/reconcile";

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task LimitDiBawahSatu_Ditolak(int limit)
    {
        using var h = new RekonsiliasiHarness();
        h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");

        var hasil = await h.JalankanAsync(dryRun: true, limit: limit);

        Assert.Equal(RsmmcPilotMrnReconcileOutcome.ValidationFailed, hasil.Outcome);
        Assert.Equal("RSMMC_PILOT_LIMIT_OUT_OF_RANGE", hasil.ErrorCode);
        Assert.Equal(0, h.Sumber.JumlahPembacaan);
    }

    [Theory]
    [InlineData(101)]
    [InlineData(1000)]
    public async Task LimitDiAtasSeratus_Ditolak(int limit)
    {
        using var h = new RekonsiliasiHarness();
        h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");

        var hasil = await h.JalankanAsync(dryRun: false, limit: limit);

        Assert.Equal(RsmmcPilotMrnReconcileOutcome.ValidationFailed, hasil.Outcome);
        Assert.Equal("RSMMC_PILOT_LIMIT_OUT_OF_RANGE", hasil.ErrorCode);
        Assert.Equal(0, h.Sumber.JumlahPembacaan);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(null)]
    public async Task LimitBatasSahDanBawaan_Diterima(int? limit)
    {
        using var h = new RekonsiliasiHarness();
        h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");

        var hasil = await h.JalankanAsync(dryRun: true, limit: limit);

        Assert.Equal(RsmmcPilotMrnReconcileOutcome.Completed, hasil.Outcome);
        Assert.Equal(limit ?? RsmmcPilotMrnReconciliationService.DefaultLimit, hasil.Data!.Limit);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(25, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void AtributValidasiDto_MembatasiLimit(int limit, bool sah)
    {
        var request = new RsmmcPilotMrnReconcileRequest
        {
            BatchId = Guid.NewGuid(),
            Limit = limit
        };

        var valid = Validator.TryValidateObject(request, new ValidationContext(request), [], validateAllProperties: true);

        Assert.Equal(sah, valid);
    }

    [Fact]
    public void AtributValidasiDto_BatchIdWajib()
    {
        var request = new RsmmcPilotMrnReconcileRequest();

        var valid = Validator.TryValidateObject(request, new ValidationContext(request), [], validateAllProperties: true);

        Assert.False(valid);
    }

    [Fact]
    public async Task BatchIdKosong_Ditolak()
    {
        using var h = new RekonsiliasiHarness();

        var hasil = await h.JalankanAsync(dryRun: true, batchId: Guid.Empty);

        Assert.Equal(RsmmcPilotMrnReconcileOutcome.ValidationFailed, hasil.Outcome);
        Assert.Equal("RSMMC_PILOT_BATCH_ID_REQUIRED", hasil.ErrorCode);
    }

    [Fact]
    public void GerbangLingkungan_HanyaStagingPersis()
    {
        Assert.True(RsmmcPilotEnvironmentGate.IsAllowed("Staging"));

        foreach (var nama in new string?[]
        {
            "staging", "STAGING", "Production", "Development", "", " ", null,
            "Stagging", "Staging-Pilot", " Staging", "Staging "
        })
        {
            Assert.False(RsmmcPilotEnvironmentGate.IsAllowed(nama), $"'{nama}' seharusnya ditolak");
        }
    }

    [Fact]
    public async Task LingkunganStagingPersis_Diterima()
    {
        using var h = new RekonsiliasiHarness { EnvironmentName = "Staging" };
        h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");

        var hasil = await h.JalankanAsync(dryRun: true);

        Assert.Equal(RsmmcPilotMrnReconcileOutcome.Completed, hasil.Outcome);
        Assert.Equal("Staging", hasil.Data!.EnvironmentName);
    }

    [Theory]
    [InlineData("staging", true)]
    [InlineData("staging", false)]
    [InlineData("STAGING", false)]
    [InlineData("Production", true)]
    [InlineData("Production", false)]
    [InlineData("Development", true)]
    [InlineData("Development", false)]
    [InlineData("", false)]
    [InlineData("Stagging", false)]
    [InlineData("Staging-Pilot", true)]
    public async Task LingkunganSelainStagingPersis_DitolakSebelumMembacaApaPun(string namaLingkungan, bool dryRun)
    {
        using var h = new RekonsiliasiHarness { EnvironmentName = namaLingkungan };
        var pasien = h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");
        var sebelum = h.BacaPasien(pasien.Id);
        var filesystem = h.SnapshotFilesystem();

        var hasil = await h.JalankanAsync(dryRun: dryRun);

        Assert.Equal(RsmmcPilotMrnReconcileOutcome.EnvironmentRejected, hasil.Outcome);
        Assert.Equal("RSMMC_PILOT_ENVIRONMENT_NOT_ALLOWED", hasil.ErrorCode);
        Assert.Equal(0, h.Sumber.JumlahPembacaan);
        Assert.Equal(0, h.JumlahResolveStorage);
        UjiPasien.SamaPersis(sebelum, h.BacaPasien(pasien.Id));
        Assert.Equal(filesystem, h.SnapshotFilesystem());
    }

    [Fact]
    public void Endpoint_MenuntutLoginDanIzinPatientUpdateYangSudahAda()
    {
        var controller = typeof(PatientController);
        var aksi = controller.GetMethod(nameof(PatientController.ReconcileRsmmcPilotMrn))!;

        // Login wajib: [Authorize] pada controller, dan tidak ada [AllowAnonymous] di mana pun.
        Assert.NotNull(controller.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Null(controller.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Null(aksi.GetCustomAttribute<AllowAnonymousAttribute>());

        // Rute efektif sama persis dengan kontrak yang disetujui.
        var ruteController = controller.GetCustomAttribute<RouteAttribute>()!.Template;
        var post = aksi.GetCustomAttribute<HttpPostAttribute>()!;
        Assert.Equal(RuteDisetujui, $"{ruteController}/{post.Template}");

        // Izin: Patient : Update, dengan tiga nilai yang wajib cocok huruf demi huruf.
        var akses = controller.GetCustomAttribute<AccessControllerAttribute>()!;
        var action = aksi.GetCustomAttribute<AccessActionAttribute>()!;
        var permission = aksi.GetCustomAttribute<AccessPermissionAttribute>()!;

        Assert.Equal("Patient", akses.ControllerName);
        Assert.Equal(akses.ControllerName, (string)permission.Arguments![0]!);
        Assert.Equal("Update", (string)permission.Arguments[1]!);
        Assert.Equal((string)permission.Arguments[1]!, action.ActionName);
        Assert.Equal(AccessTypes.Update, action.AccessType);
        Assert.True(action.VisibleInRoleAccess);
        Assert.False(action.IsSystemOnly);
    }

    [Fact]
    public void Endpoint_DiletakkanSesudahAksiUpdateLama_SupayaLabelAksesRoleTidakBerubah()
    {
        // Registry izin memakai deskriptor pertama per kunci aksi. Aksi Update lama harus tetap
        // muncul lebih dulu, supaya label "Update Patient" di layar Akses Role tidak berubah.
        var aksiUpdate = typeof(PatientController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => x.GetCustomAttribute<AccessActionAttribute>()?.ActionName == "Update")
            .Select(x => x.Name)
            .ToList();

        Assert.Equal(nameof(PatientController.UpdatePatient), aksiUpdate.First());
        Assert.Equal(nameof(PatientController.ReconcileRsmmcPilotMrn), aksiUpdate.Last());
    }

    [Fact]
    public async Task EksekusiTanpaAktorTerkenali_Ditolak()
    {
        using var h = new RekonsiliasiHarness();
        var pasien = h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");

        var hasil = await h.BuatService().ReconcileAsync(
            new RsmmcPilotMrnReconcileRequest { BatchId = RekonsiliasiHarness.BatchUji, DryRun = false },
            Guid.Empty,
            h.BuatStore(),
            "uji-trace",
            CancellationToken.None);

        Assert.Equal(RsmmcPilotMrnReconcileOutcome.ValidationFailed, hasil.Outcome);
        Assert.Equal("RSMMC_PILOT_ACTOR_UNKNOWN", hasil.ErrorCode);
        Assert.Equal("00-00-07-01", h.BacaPasien(pasien.Id).MedicalRecordNumber);
    }
}
