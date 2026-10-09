using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;

namespace QuilvianSystemBackend.Tests.PatientManagement;

/// <summary>
/// Simulasi (<c>dryRun=true</c>), klasifikasi seluruh set Pilot, dan aturan <c>limit</c>
/// — `AC-01` sampai `AC-04`, `AC-14`, `AC-15`, `AC-24`, `PAT-OQ-003`, `PAT-OQ-004`.
/// </summary>
public class SimulasiDanKlasifikasiTests
{
    [Fact]
    public async Task Simulasi_TidakMengubahSatuPunMstPatient()
    {
        using var h = new RekonsiliasiHarness();
        var a = h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");
        var b = h.TambahPilot("1002", "00-00-07-02", "00-79-70-16");
        var sebelumA = h.BacaPasien(a.Id);
        var sebelumB = h.BacaPasien(b.Id);

        var hasil = await h.JalankanAsync(dryRun: true);

        Assert.Equal(RsmmcPilotMrnReconcileOutcome.Completed, hasil.Outcome);
        Assert.Equal(2, hasil.Data!.Ready);
        UjiPasien.SamaPersis(sebelumA, h.BacaPasien(a.Id));
        UjiPasien.SamaPersis(sebelumB, h.BacaPasien(b.Id));
    }

    [Fact]
    public async Task Simulasi_TidakMenulisFolderMaupunFileQr()
    {
        using var h = new RekonsiliasiHarness();
        h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");
        var sebelum = h.SnapshotFilesystem();

        var hasil = await h.JalankanAsync(dryRun: true);

        Assert.Equal(sebelum, h.SnapshotFilesystem());
        Assert.False(Directory.Exists(Path.GetDirectoryName(h.PathQrFisik("00-79-70-15"))));
        // Folder penyimpanan bahkan tidak pernah diminta, karena memintanya membuat folder.
        Assert.Equal(0, h.JumlahResolveStorage);
        Assert.Empty(h.PayloadDirender);
        Assert.Equal(
            RekonsiliasiHarness.PathQrPublik("00-79-70-15"),
            hasil.Data!.Items.Single().PlannedQrCodePath);
    }

    [Fact]
    public async Task DryRunTidakDikirim_DianggapSimulasi()
    {
        using var h = new RekonsiliasiHarness();
        var a = h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");
        var sebelum = h.BacaPasien(a.Id);
        var filesystem = h.SnapshotFilesystem();

        var hasil = await h.JalankanAsync(dryRun: null);

        Assert.True(hasil.Data!.DryRun);
        Assert.Equal(RsmmcPilotMrnReconciliationStatus.Ready, hasil.Data.Items.Single().Status);
        Assert.Equal(0, hasil.Data.Reconciled);
        UjiPasien.SamaPersis(sebelum, h.BacaPasien(a.Id));
        Assert.Equal(filesystem, h.SnapshotFilesystem());
    }

    [Fact]
    public async Task Simulasi_MenampilkanIsiWajibDanUrutLegacyPidNaik()
    {
        using var h = new RekonsiliasiHarness();
        // Disisipkan tidak berurutan; tiruan sumber juga mengembalikannya terbalik.
        h.TambahPilot("1010", "00-00-07-10", "00-79-70-20");
        var kecil = h.TambahPilot("2", "00-00-07-02", "00-79-70-12");
        h.TambahPilot("101", "00-00-07-11", "00-79-70-21");

        var hasil = await h.JalankanAsync(dryRun: true);

        Assert.Equal(["2", "101", "1010"], hasil.Data!.Items.Select(x => x.LegacyPid));

        var item = hasil.Data.Items[0];
        Assert.Equal(kecil.Id, item.PatientId);
        Assert.Equal(kecil.PatientCode, item.PatientCode);
        Assert.False(string.IsNullOrWhiteSpace(item.FullName));
        Assert.Equal("00-00-07-02", item.CurrentMedicalRecordNumber);
        Assert.Equal("00-79-70-12", item.FinalMedicalRecordNumber);
        Assert.Equal(RekonsiliasiHarness.PathQrPublik("00-00-07-02"), item.CurrentQrCodePath);
        Assert.Equal(RekonsiliasiHarness.PathQrPublik("00-79-70-12"), item.PlannedQrCodePath);
        Assert.Equal(RsmmcPilotMrnReconciliationStatus.Ready, item.Status);

        // Dijalankan dua kali, hasilnya sama persis.
        var ulang = await h.JalankanAsync(dryRun: true);
        Assert.Equal(
            hasil.Data.Items.Select(x => (x.LegacyPid, x.Status, x.FinalMedicalRecordNumber)),
            ulang.Data!.Items.Select(x => (x.LegacyPid, x.Status, x.FinalMedicalRecordNumber)));
    }

    [Fact]
    public async Task MrnTujuan_DiambilDariPerencana_BukanNormalizedMrnCrosswalk()
    {
        using var h = new RekonsiliasiHarness();
        h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");
        var normalizedMrn = h.Sumber.Crosswalk.Single().NormalizedMrn;
        Assert.NotEqual("00-79-70-15", normalizedMrn);

        var hasil = await h.JalankanAsync(dryRun: true);

        Assert.Equal("00-79-70-15", hasil.Data!.Items.Single().FinalMedicalRecordNumber);
        Assert.DoesNotContain(hasil.Data.Items, x => x.FinalMedicalRecordNumber == normalizedMrn);
    }

    [Fact]
    public async Task MrnSudahKanonikDanQrBenar_AlreadyReconciled_TanpaPerubahan()
    {
        using var h = new RekonsiliasiHarness();
        var sudah = h.TambahPilot("1001", "00-79-70-15", "00-79-70-15");
        var sebelum = h.BacaPasien(sudah.Id);

        var simulasi = await h.JalankanAsync(dryRun: true);
        var eksekusi = await h.JalankanAsync(dryRun: false);

        foreach (var hasil in new[] { simulasi, eksekusi })
        {
            Assert.Equal(1, hasil.Data!.AlreadyReconciled);
            Assert.Equal(0, hasil.Data.Selected);
            Assert.Equal(0, hasil.Data.Remaining);
        }

        Assert.Equal(0, h.JumlahResolveStorage);
        UjiPasien.SamaPersis(sebelum, h.BacaPasien(sudah.Id));
    }

    [Fact]
    public async Task MrnKanonikTetapiQrSalah_QrInconsistent_DilaporkanTanpaDiperbaiki()
    {
        using var h = new RekonsiliasiHarness();
        var salahQr = h.TambahPilot(
            "1001",
            "00-79-70-15",
            "00-79-70-15",
            qrCodePath: RekonsiliasiHarness.PathQrPublik("00-00-07-01"));
        var sebelum = h.BacaPasien(salahQr.Id);

        var hasil = await h.JalankanAsync(dryRun: false);

        Assert.Equal(1, hasil.Data!.QrInconsistent);
        Assert.Equal(0, hasil.Data.AlreadyReconciled);
        var item = Assert.Single(hasil.Data.Items);
        Assert.Equal(RsmmcPilotMrnReconciliationStatus.QrInconsistent, item.Status);
        Assert.Equal(0, h.JumlahResolveStorage);
        UjiPasien.SamaPersis(sebelum, h.BacaPasien(salahQr.Id));
    }

    [Fact]
    public async Task PasienKanonikDanQrInconsistent_TidakMenghabiskanLimit()
    {
        using var h = new RekonsiliasiHarness();
        // Legacy pid terkecil justru yang sudah kanonik, supaya bila mereka dihitung dalam limit,
        // jatah limit habis sebelum pasien layak tersentuh.
        h.TambahPilot("1", "00-79-70-01", "00-79-70-01");
        h.TambahPilot("2", "00-79-70-02", "00-79-70-02");
        h.TambahPilot("3", "00-79-70-03", "00-79-70-03", qrCodePath: "/uploads/patient-qrcodes/00-00-00-03/qrcode.png");
        var layak1 = h.TambahPilot("4", "00-00-07-04", "00-79-70-04");
        var layak2 = h.TambahPilot("5", "00-00-07-05", "00-79-70-05");
        var layak3 = h.TambahPilot("6", "00-00-07-06", "00-79-70-06");

        var simulasi = await h.JalankanAsync(dryRun: true, limit: 2);

        Assert.Equal(2, simulasi.Data!.AlreadyReconciled);
        Assert.Equal(1, simulasi.Data.QrInconsistent);
        Assert.Equal(3, simulasi.Data.EligibleBeforeRun);
        Assert.Equal(2, simulasi.Data.Selected);
        Assert.Equal(
            ["4", "5"],
            simulasi.Data.Items.Where(x => x.Status == RsmmcPilotMrnReconciliationStatus.Ready).Select(x => x.LegacyPid));

        var eksekusi = await h.JalankanAsync(dryRun: false, limit: 2);

        Assert.Equal(2, eksekusi.Data!.Reconciled);
        Assert.Equal(1, eksekusi.Data.Remaining);
        Assert.Equal("00-79-70-04", h.BacaPasien(layak1.Id).MedicalRecordNumber);
        Assert.Equal("00-79-70-05", h.BacaPasien(layak2.Id).MedicalRecordNumber);
        Assert.Equal("00-00-07-06", h.BacaPasien(layak3.Id).MedicalRecordNumber);
    }

    [Fact]
    public async Task Simulasi_MelaporkanKonflikMrnTanpaMengubahData()
    {
        using var h = new RekonsiliasiHarness();
        var pilot = h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");
        h.TambahPasienLain("00-79-70-15");

        var hasil = await h.JalankanAsync(dryRun: true);

        var item = Assert.Single(hasil.Data!.Items);
        Assert.Equal(RsmmcPilotMrnReconciliationStatus.Conflict, item.Status);
        Assert.Equal(RsmmcPilotMrnConflictType.MedicalRecordNumberOwnedByAnotherPatient, item.ConflictType);
        Assert.Equal("00-00-07-01", h.BacaPasien(pilot.Id).MedicalRecordNumber);
    }

    [Fact]
    public async Task Simulasi_BatchLain_MelaporkanGerbangGagalTanpaMenolak()
    {
        using var h = new RekonsiliasiHarness();
        h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");
        h.ProfilKhusus = RsmmcPilotApprovedBatchProfile.Canonical;

        var hasil = await h.JalankanAsync(dryRun: true);

        Assert.Equal(RsmmcPilotMrnReconcileOutcome.Completed, hasil.Outcome);
        Assert.Contains(hasil.Data!.Gates, x => x.Code == "APPROVED_BATCH" && !x.Passed);
    }
}
