using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;

namespace QuilvianSystemBackend.Tests.PatientManagement;

/// <summary>
/// Eksekusi sungguhan (<c>dryRun=false</c>) pada basis data uji dan folder QR sementara —
/// `AC-07` sampai `AC-26`, `PAT-OQ-005`.
/// </summary>
public class EksekusiRekonsiliasiTests
{
    [Fact]
    public async Task Berhasil_MrnDanQrPindahKeKanonik_IdentitasTetap()
    {
        using var h = new RekonsiliasiHarness();
        var pasien = h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");
        var sebelum = h.BacaPasien(pasien.Id);

        var hasil = await h.JalankanAsync(dryRun: false);

        Assert.Equal(RsmmcPilotMrnReconcileOutcome.Completed, hasil.Outcome);
        var item = Assert.Single(hasil.Data!.Items);
        Assert.Equal(RsmmcPilotMrnReconciliationStatus.Reconciled, item.Status);
        Assert.Equal(1, hasil.Data.Reconciled);
        Assert.Equal(0, hasil.Data.Remaining);
        Assert.False(hasil.Data.Stopped);

        var sesudah = h.BacaPasien(pasien.Id);
        Assert.Equal(pasien.Id, sesudah.Id);
        Assert.Equal(pasien.PatientCode, sesudah.PatientCode);
        Assert.Equal("00-79-70-15", sesudah.MedicalRecordNumber);
        Assert.Equal(RekonsiliasiHarness.PathQrPublik("00-79-70-15"), sesudah.QrCodePath);
        Assert.Equal(RekonsiliasiHarness.AktorUji, sesudah.UpdateBy);
        Assert.NotNull(sesudah.UpdateDateTime);
        Assert.Equal(sebelum.CreateDateTime, sesudah.CreateDateTime);
        Assert.Equal(sebelum.CreateBy, sesudah.CreateBy);

        // Hanya empat kolom yang berubah — seluruh kolom skalar lain dibandingkan.
        var berubah = UjiPasien.KolomBerubah(sebelum, sesudah);
        Assert.All(berubah, kolom => Assert.Contains(kolom, UjiPasien.KolomBolehBerubah));
        Assert.Contains(nameof(sesudah.MedicalRecordNumber), berubah);
        Assert.Contains(nameof(sesudah.QrCodePath), berubah);
    }

    [Fact]
    public async Task QrBaru_IsiDanFolderMemakaiMrnKanonik()
    {
        using var h = new RekonsiliasiHarness();
        h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");

        await h.JalankanAsync(dryRun: false);

        Assert.Equal(["00-79-70-15"], h.PayloadDirender);
        Assert.Equal(FormatQrPasien.Payload("00-79-70-15"), h.PayloadDirender.Single());
        Assert.Equal("QR-BARU:00-79-70-15", File.ReadAllText(h.PathQrFisik("00-79-70-15")));
    }

    [Fact]
    public async Task QrLama_TetapAdaDanTidakBerubah()
    {
        using var h = new RekonsiliasiHarness();
        h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");

        await h.JalankanAsync(dryRun: false);

        Assert.Equal("QR-LAMA:00-00-07-01", File.ReadAllText(h.PathQrFisik("00-00-07-01")));
    }

    [Fact]
    public async Task MrnKanonikDipakaiPasienLain_Conflict_TanpaPerubahanDanBerhenti()
    {
        using var h = new RekonsiliasiHarness();
        var bentrok = h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");
        var berikutnya = h.TambahPilot("1002", "00-00-07-02", "00-79-70-16");
        h.TambahPasienLain("00-79-70-15");
        var sebelum = h.BacaPasien(bentrok.Id);

        var hasil = await h.JalankanAsync(dryRun: false);

        var item = hasil.Data!.Items.Single(x => x.LegacyPid == "1001");
        Assert.Equal(RsmmcPilotMrnReconciliationStatus.Conflict, item.Status);
        Assert.Equal(RsmmcPilotMrnConflictType.MedicalRecordNumberOwnedByAnotherPatient, item.ConflictType);
        Assert.True(hasil.Data.Stopped);
        Assert.DoesNotContain(hasil.Data.Items, x => x.LegacyPid == "1002");
        UjiPasien.SamaPersis(sebelum, h.BacaPasien(bentrok.Id));
        Assert.Equal("00-00-07-02", h.BacaPasien(berikutnya.Id).MedicalRecordNumber);
        Assert.Empty(h.PayloadDirender);
    }

    [Fact]
    public async Task ArtefakQrTujuanSudahAda_Conflict_TanpaMenimpaDanTanpaMemanggilPembuatQr()
    {
        using var h = new RekonsiliasiHarness();
        var pasien = h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");
        RekonsiliasiHarness.TulisFile(h.PathQrFisik("00-79-70-15"), "QR-MILIK-ORANG-LAIN");
        var sebelum = h.BacaPasien(pasien.Id);

        var hasil = await h.JalankanAsync(dryRun: false);

        var item = Assert.Single(hasil.Data!.Items);
        Assert.Equal(RsmmcPilotMrnReconciliationStatus.Conflict, item.Status);
        Assert.Equal(RsmmcPilotMrnConflictType.QrArtifactAlreadyExists, item.ConflictType);
        Assert.Equal("QR-MILIK-ORANG-LAIN", File.ReadAllText(h.PathQrFisik("00-79-70-15")));
        Assert.Equal("QR-LAMA:00-00-07-01", File.ReadAllText(h.PathQrFisik("00-00-07-01")));
        Assert.Empty(h.PayloadDirender);
        UjiPasien.SamaPersis(sebelum, h.BacaPasien(pasien.Id));
    }

    [Fact]
    public async Task ArtefakQrMunculDiAntaraPemeriksaanDanPenulisan_Conflict_TanpaMenimpa()
    {
        using var h = new RekonsiliasiHarness();
        var pasien = h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");
        var sebelum = h.BacaPasien(pasien.Id);

        // Proses lain menulis file tujuan tepat sesudah pemeriksaan awal lolos.
        h.SaatRender = _ => RekonsiliasiHarness.TulisFile(h.PathQrFisik("00-79-70-15"), "QR-PROSES-LAIN");

        var hasil = await h.JalankanAsync(dryRun: false);

        var item = Assert.Single(hasil.Data!.Items);
        Assert.Equal(RsmmcPilotMrnReconciliationStatus.Conflict, item.Status);
        Assert.Equal(RsmmcPilotMrnConflictType.QrArtifactAlreadyExists, item.ConflictType);
        Assert.Equal("QR-PROSES-LAIN", File.ReadAllText(h.PathQrFisik("00-79-70-15")));
        Assert.Equal("QR-LAMA:00-00-07-01", File.ReadAllText(h.PathQrFisik("00-00-07-01")));
        UjiPasien.SamaPersis(sebelum, h.BacaPasien(pasien.Id));
    }

    [Fact]
    public async Task GagalMembuatQr_Failed_DataPasienTetapDanTidakAdaBerkasBaru()
    {
        using var h = new RekonsiliasiHarness();
        var pasien = h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");
        var sebelum = h.BacaPasien(pasien.Id);
        h.GagalRender = true;

        var hasil = await h.JalankanAsync(dryRun: false);

        var item = Assert.Single(hasil.Data!.Items);
        Assert.Equal(RsmmcPilotMrnReconciliationStatus.Failed, item.Status);
        Assert.Equal("CREATE_QR", item.FailureStage);
        Assert.True(hasil.Data.Stopped);
        UjiPasien.SamaPersis(sebelum, h.BacaPasien(pasien.Id));
        Assert.False(Directory.Exists(Path.GetDirectoryName(h.PathQrFisik("00-79-70-15"))));
        Assert.Equal("QR-LAMA:00-00-07-01", File.ReadAllText(h.PathQrFisik("00-00-07-01")));
    }

    [Fact]
    public async Task GagalSimpanSesudahQrDibuat_HanyaQrBaruDibersihkan_QrLamaTetap()
    {
        using var h = new RekonsiliasiHarness();
        var pasien = h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");
        var sebelum = h.BacaPasien(pasien.Id);
        var gagal = new GagalSimpanInterceptor(gagalPadaPemanggilanKe: 1);

        var hasil = await h.JalankanAsync(dryRun: false, interceptors: gagal);

        var item = Assert.Single(hasil.Data!.Items);
        Assert.Equal(RsmmcPilotMrnReconciliationStatus.Failed, item.Status);
        Assert.Equal("SAVE_CHANGES", item.FailureStage);
        Assert.Equal(1, gagal.JumlahPemanggilan);
        Assert.Equal(["00-79-70-15"], h.PayloadDirender);

        UjiPasien.SamaPersis(sebelum, h.BacaPasien(pasien.Id));
        Assert.False(File.Exists(h.PathQrFisik("00-79-70-15")));
        Assert.False(Directory.Exists(Path.GetDirectoryName(h.PathQrFisik("00-79-70-15"))));
        Assert.Equal("QR-LAMA:00-00-07-01", File.ReadAllText(h.PathQrFisik("00-00-07-01")));
    }

    [Fact]
    public async Task GagalSimpan_QrBaruDigantiIsiSamaPanjang_TidakDihapusDanDilaporkan()
    {
        using var h = new RekonsiliasiHarness();
        var pasien = h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");
        var sebelum = h.BacaPasien(pasien.Id);
        var pathBaru = h.PathQrFisik("00-79-70-15");

        // Tepat sebelum database gagal, pihak lain mengganti QR baru dengan isi berbeda yang
        // panjangnya sama persis. Pembersihan tidak boleh menganggapnya milik pemanggilan ini.
        var pengganti = "QR-LAIN:99-99-99-99";
        var gagal = new GagalSimpanInterceptor(
            gagalPadaPemanggilanKe: 1,
            sebelumGagal: () =>
            {
                Assert.Equal(new FileInfo(pathBaru).Length, pengganti.Length);
                File.WriteAllText(pathBaru, pengganti);
            });

        var hasil = await h.JalankanAsync(dryRun: false, interceptors: gagal);

        var item = Assert.Single(hasil.Data!.Items);
        Assert.Equal(RsmmcPilotMrnReconciliationStatus.Failed, item.Status);
        Assert.Equal("SAVE_CHANGES", item.FailureStage);
        Assert.Contains(nameof(RsmmcPilotQrCleanupOutcome.OwnershipUnproven), item.Message);
        Assert.Equal(pengganti, File.ReadAllText(pathBaru));
        Assert.Equal("QR-LAMA:00-00-07-01", File.ReadAllText(h.PathQrFisik("00-00-07-01")));
        UjiPasien.SamaPersis(sebelum, h.BacaPasien(pasien.Id));
    }

    [Fact]
    public async Task GagalCommit_HasilTidakPasti_QrBaruDanLamaDipertahankan_ProsesBerhenti()
    {
        using var h = new RekonsiliasiHarness();
        var pertama = h.TambahPilot("1", "00-00-07-01", "00-79-70-11");
        var kedua = h.TambahPilot("2", "00-00-07-02", "00-79-70-12");
        var gagal = new GagalCommitInterceptor(gagalPadaCommitKe: 1);

        var hasil = await h.JalankanAsync(dryRun: false, interceptors: gagal);

        Assert.True(hasil.Data!.Stopped);
        Assert.Equal(0, hasil.Data.Reconciled);
        Assert.Equal(1, hasil.Data.Failed);
        var item = Assert.Single(hasil.Data.Items);
        Assert.Equal("1", item.LegacyPid);
        Assert.Equal(RsmmcPilotMrnReconciliationStatus.Failed, item.Status);
        Assert.Equal(RsmmcPilotMrnReconciliationService.CommitOutcomeUncertainStage, item.FailureStage);

        // QR baru tidak dihapus otomatis, QR lama tetap; keduanya untuk rekonsiliasi operator.
        Assert.Equal("QR-BARU:00-79-70-11", File.ReadAllText(h.PathQrFisik("00-79-70-11")));
        Assert.Equal("QR-LAMA:00-00-07-01", File.ReadAllText(h.PathQrFisik("00-00-07-01")));

        // Tidak dicoba ulang, dan pasien berikutnya tidak diproses.
        Assert.Equal(1, gagal.JumlahCommit);
        Assert.Equal(["00-79-70-11"], h.PayloadDirender);
        Assert.Equal("00-00-07-02", h.BacaPasien(kedua.Id).MedicalRecordNumber);
        Assert.False(File.Exists(h.PathQrFisik("00-79-70-12")));
        Assert.Equal(pertama.Id, item.PatientId);
    }

    [Fact]
    public async Task PalingBanyakLimitPasienDiproses_SisanyaTidakTersentuh()
    {
        using var h = new RekonsiliasiHarness();
        var pasien = Enumerable.Range(1, 5)
            .Select(i => h.TambahPilot($"{i}", $"00-00-07-0{i}", $"00-79-70-1{i}"))
            .ToList();

        var hasil = await h.JalankanAsync(dryRun: false, limit: 2);

        Assert.Equal(2, hasil.Data!.Selected);
        Assert.Equal(2, hasil.Data.Reconciled);
        Assert.Equal(3, hasil.Data.Remaining);
        Assert.Equal(["1", "2"], hasil.Data.Items.Select(x => x.LegacyPid));
        Assert.Equal("00-79-70-11", h.BacaPasien(pasien[0].Id).MedicalRecordNumber);
        Assert.Equal("00-79-70-12", h.BacaPasien(pasien[1].Id).MedicalRecordNumber);

        foreach (var sisa in pasien.Skip(2))
        {
            Assert.Equal(sisa.MrnLama, h.BacaPasien(sisa.Id).MedicalRecordNumber);
            Assert.False(File.Exists(h.PathQrFisik(sisa.MrnKanonik)));
        }
    }

    [Fact]
    public async Task KegagalanPertama_MenghentikanProses_PasienSebelumnyaTetapTersimpan()
    {
        using var h = new RekonsiliasiHarness();
        var pertama = h.TambahPilot("1", "00-00-07-01", "00-79-70-11");
        var kedua = h.TambahPilot("2", "00-00-07-02", "00-79-70-12");
        var ketiga = h.TambahPilot("3", "00-00-07-03", "00-79-70-13");
        var gagal = new GagalSimpanInterceptor(gagalPadaPemanggilanKe: 2);

        var hasil = await h.JalankanAsync(dryRun: false, interceptors: gagal);

        Assert.True(hasil.Data!.Stopped);
        Assert.Equal(1, hasil.Data.Reconciled);
        Assert.Equal(1, hasil.Data.Failed);
        Assert.Equal(2, hasil.Data.Remaining);
        Assert.Equal(
            [RsmmcPilotMrnReconciliationStatus.Reconciled, RsmmcPilotMrnReconciliationStatus.Failed],
            hasil.Data.Items.Select(x => x.Status));

        var gagalItem = hasil.Data.Items[1];
        Assert.Equal("2", gagalItem.LegacyPid);
        Assert.Equal(kedua.Id, gagalItem.PatientId);
        Assert.Equal(kedua.PatientCode, gagalItem.PatientCode);
        Assert.Equal("00-00-07-02", gagalItem.CurrentMedicalRecordNumber);
        Assert.Equal("00-79-70-12", gagalItem.FinalMedicalRecordNumber);
        Assert.Equal("SAVE_CHANGES", gagalItem.FailureStage);

        // Pasien pertama sudah commit dan tidak ikut dibatalkan.
        Assert.Equal("00-79-70-11", h.BacaPasien(pertama.Id).MedicalRecordNumber);
        Assert.True(File.Exists(h.PathQrFisik("00-79-70-11")));

        Assert.Equal("00-00-07-02", h.BacaPasien(kedua.Id).MedicalRecordNumber);
        Assert.False(File.Exists(h.PathQrFisik("00-79-70-12")));

        // Pasien ketiga tidak diproses sama sekali.
        Assert.Equal("00-00-07-03", h.BacaPasien(ketiga.Id).MedicalRecordNumber);
        Assert.False(File.Exists(h.PathQrFisik("00-79-70-13")));
        Assert.Equal(2, gagal.JumlahPemanggilan);
    }

    [Fact]
    public async Task PanggilanUlang_Idempoten_TidakAdaPerubahanKedua()
    {
        using var h = new RekonsiliasiHarness();
        var pasien = h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");

        await h.JalankanAsync(dryRun: false);
        var sesudahPertama = h.BacaPasien(pasien.Id);
        var filesystem = h.SnapshotFilesystem();

        var ulang = await h.JalankanAsync(dryRun: false);

        Assert.Equal(1, ulang.Data!.AlreadyReconciled);
        Assert.Equal(0, ulang.Data.Selected);
        Assert.Equal(0, ulang.Data.Reconciled);
        Assert.Empty(ulang.Data.Items);
        UjiPasien.SamaPersis(sesudahPertama, h.BacaPasien(pasien.Id));
        Assert.Equal(filesystem, h.SnapshotFilesystem());
    }

    [Fact]
    public async Task TidakMemakaiPembangkitMrnMaupunPatientCodeNormal_TidakMenambahPasien()
    {
        using var h = new RekonsiliasiHarness();
        // Pembangkit normal memilih nomor terkecil yang belum dipakai, misalnya 00-00-00-01.
        // MRN kanonik di sini sengaja jauh dari itu.
        var pasien = h.TambahPilot("1001", "00-00-07-01", "99-00-00-01");
        var jumlahSebelum = h.JumlahPasien();
        var kodeSebelum = h.SemuaPatientCode();

        await h.JalankanAsync(dryRun: false);

        var sesudah = h.BacaPasien(pasien.Id);
        Assert.Equal("99-00-00-01", sesudah.MedicalRecordNumber);
        Assert.Equal(pasien.PatientCode, sesudah.PatientCode);
        Assert.Equal(jumlahSebelum, h.JumlahPasien());
        Assert.Equal(kodeSebelum, h.SemuaPatientCode());
    }

    [Fact]
    public async Task CrosswalkPerencanaDanCadangan_TidakBerubah()
    {
        using var h = new RekonsiliasiHarness();
        h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");
        h.TambahPilot("1002", "00-00-07-02", "00-79-70-16");
        var sebelum = h.Sumber.Snapshot();

        await h.JalankanAsync(dryRun: true);
        await h.JalankanAsync(dryRun: false);

        Assert.Equal(sebelum, h.Sumber.Snapshot());
    }

    [Fact]
    public async Task GerbangBatchGagal_EksekusiDitolakTanpaPerubahan()
    {
        using var h = new RekonsiliasiHarness();
        var pasien = h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");
        h.ProfilKhusus = new RsmmcPilotApprovedBatchProfile(
            RekonsiliasiHarness.BatchUji,
            ExpectedFinalizedPlannerRows: 1,
            ExpectedPilotRows: 1,
            ExpectedBackupRows: 715);
        var sebelum = h.BacaPasien(pasien.Id);

        var hasil = await h.JalankanAsync(dryRun: false);

        Assert.Equal(RsmmcPilotMrnReconcileOutcome.GateRejected, hasil.Outcome);
        Assert.Contains(hasil.Data!.Gates, x => x.Code == "BACKUP_ROWS" && !x.Passed);
        Assert.Equal(0, h.JumlahResolveStorage);
        UjiPasien.SamaPersis(sebelum, h.BacaPasien(pasien.Id));
    }

    [Fact]
    public async Task BatchSelainBatchKanonik_EksekusiDitolak()
    {
        using var h = new RekonsiliasiHarness();
        var pasien = h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");
        h.ProfilKhusus = RsmmcPilotApprovedBatchProfile.Canonical;

        var hasil = await h.JalankanAsync(dryRun: false);

        Assert.Equal(RsmmcPilotMrnReconcileOutcome.GateRejected, hasil.Outcome);
        Assert.Contains(hasil.Data!.Gates, x => x.Code == "APPROVED_BATCH" && !x.Passed);
        Assert.Equal("00-00-07-01", h.BacaPasien(pasien.Id).MedicalRecordNumber);
    }

    [Fact]
    public async Task PerencanaBerubahSesudahPemilihan_Failed_TanpaPerubahan()
    {
        using var h = new RekonsiliasiHarness();
        var pasien = h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");
        var sebelum = h.BacaPasien(pasien.Id);
        h.Sumber.SebelumPembacaanUlang = _ => h.Sumber.Perencana.Single().PlanStatus = "DRAFT";

        var hasil = await h.JalankanAsync(dryRun: false);

        var item = Assert.Single(hasil.Data!.Items);
        Assert.Equal(RsmmcPilotMrnReconciliationStatus.Failed, item.Status);
        Assert.Equal("REVALIDATE_PLAN", item.FailureStage);
        Assert.Empty(h.PayloadDirender);
        UjiPasien.SamaPersis(sebelum, h.BacaPasien(pasien.Id));
    }

    [Fact]
    public async Task PemetaanCrosswalkBerubahSesudahPemilihan_Failed_TanpaPerubahan()
    {
        using var h = new RekonsiliasiHarness();
        var pasien = h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");
        h.Sumber.SebelumPembacaanUlang = _ =>
            h.Sumber.Crosswalk.Single().QuilvianPatientId = Guid.NewGuid().ToString("D");

        var hasil = await h.JalankanAsync(dryRun: false);

        var item = Assert.Single(hasil.Data!.Items);
        Assert.Equal(RsmmcPilotMrnReconciliationStatus.Failed, item.Status);
        Assert.Equal("REVALIDATE_PLAN", item.FailureStage);
        Assert.Equal("00-00-07-01", h.BacaPasien(pasien.Id).MedicalRecordNumber);
    }
}
