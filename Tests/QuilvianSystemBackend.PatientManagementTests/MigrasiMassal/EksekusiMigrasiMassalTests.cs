using System.Globalization;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;
using QuilvianSystemBackend.Tests.PatientManagement.MigrasiMassal.Infrastruktur;

namespace QuilvianSystemBackend.Tests.PatientManagement.MigrasiMassal;

/// <summary>
/// Eksekusi <c>dryRun=false</c>: transaksi per pasien, kunci, idempotensi, berhenti pada kejadian tak
/// terduga, QR, dan pembatalan — `PAT-MIG2-AC-10`–`22`, `24`, `25`, `33`–`35`, `41`, `42`, `44`, `49`;
/// kontrak `1.3.0` B4.3–B6, B9, B10.
/// </summary>
public class EksekusiMigrasiMassalTests
{
    private static readonly string[] TabelTidakBolehBerubah =
    [
        "rsmmc_patient_target_preview|", "rsmmc_patient_mrn_plan|", "rsmmc_patient_batch|", "rsmmc_patient_pilot_reconcile_backup|"
    ];

    [Fact]
    public async Task JalurNormal_25PasienDibuatPersisDariPreview_DenganCrosswalkDanQrBaru()
    {
        using var h = new MigrasiMassalHarness();
        var sebelum = h.SnapshotDatabase();
        var crosswalkLama = h.BacaCrosswalk();
        var historis = h.SnapshotFilesystem();
        var mulai = DateTime.UtcNow.AddSeconds(-1);

        var hasil = await h.JalankanAsync(false, 25);
        var data = hasil.Data!;

        Assert.Equal(RsmmcBulkPatientMigrationOutcome.Completed, hasil.Outcome);
        Assert.Equal(25, data.Migrated);
        Assert.Equal(0, data.Conflict);
        Assert.Equal(0, data.Failed);
        Assert.Equal(0, data.NotProcessed);
        Assert.Equal(0, data.Remaining);
        Assert.Equal(25, data.Selected);
        Assert.False(data.Stopped);
        Assert.All(data.Items, x => Assert.Equal(RsmmcBulkPatientMigrationStatus.Migrated, x.Status));

        for (var i = 0; i < 25; i++)
        {
            var pasien = PeriksaPasienSesuaiPreview(h, i, mulai);
            Assert.Equal(pasien.Id, data.Items[i].PatientId);
            Assert.Equal(RsmmcBulkCrosswalkState.Missing, data.Items[i].CrosswalkStateBefore);
            PeriksaCrosswalkBaru(h, i, pasien.Id);
            PeriksaQrBaru(h, i);
        }

        // Satu transaksi per pasien; kunci dua bagian dalam urutan manifest.
        Assert.Equal(25, h.Transaksi.Dibuka);
        Assert.Equal(25, h.Transaksi.Commit);
        Assert.Equal(
            RsmmcBulkCanaryManifest.CanaryAV1.LegacyPids.Select(x => (RsmmcBulkApprovedBatchProfile.WorkerLockNamespace, (int)x)),
            h.Kendali.KunciDiminta);

        // Hanya INSERT: tabel migrasi lain, crosswalk lama, pasien lama, dan QR historis tidak berubah.
        var sesudah = h.SnapshotDatabase();
        foreach (var awalan in TabelTidakBolehBerubah)
        {
            Assert.Equal(Baris(sebelum, awalan), Baris(sesudah, awalan));
        }

        Assert.All(Baris(sebelum, "MstPatient|"), x => Assert.Contains(x, Baris(sesudah, "MstPatient|")));
        Assert.All(crosswalkLama, x => Assert.Contains(x, h.BacaCrosswalk()));
        Assert.All(historis, x => Assert.Equal(x.Value, h.SnapshotFilesystem()[x.Key]));
        Assert.Equal(h.Profil.ApprovedPatientBaseline + 25, h.JumlahPasien());
        Assert.All(h.Perintah.PerintahTulis, x => Assert.StartsWith("INSERT", x.TrimStart(), StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task IsMemberFalse_DikirimEksplisitPadaInsert_TanpaUpdateSesudahnya()
    {
        using var h = new MigrasiMassalHarness();

        await h.JalankanAsync(false, 1);

        var insert = Assert.Single(h.Perintah.PerintahTulis);
        Assert.Contains("INSERT INTO \"MstPatient\"", insert);
        Assert.Contains("\"IsMember\"", insert);
        Assert.DoesNotContain(h.Perintah.PerintahTulis, x => x.TrimStart().StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase));
        Assert.False(h.PasienDenganKode(MigrasiMassalHarness.KodeUntuk(0))!.IsMember);
    }

    [Fact]
    public async Task Limit5_ContohKontrak_PanggilanKeduaMelewatiYangSudahAda()
    {
        using var h = new MigrasiMassalHarness();

        var pertama = (await h.JalankanAsync(false, 5)).Data!;

        Assert.Equal(5, pertama.Migrated);
        Assert.Equal(5, pertama.Selected);
        Assert.Equal(20, pertama.NotProcessed);
        Assert.Equal(20, pertama.Remaining);
        Assert.False(pertama.Stopped);
        Assert.All(pertama.Items.Take(5), x => Assert.Equal(RsmmcBulkPatientMigrationStatus.Migrated, x.Status));
        Assert.All(pertama.Items.Skip(5), x => Assert.Equal(RsmmcBulkPatientMigrationStatus.NotProcessed, x.Status));

        var kedua = await h.JalankanAsync(false, 5);
        var data = kedua.Data!;

        Assert.Equal(RsmmcBulkPatientMigrationOutcome.Completed, kedua.Outcome);
        Assert.True(data.Gates.Single(x => x.Code == "PATIENT_BASELINE").Passed);
        Assert.Equal($"{h.Profil.ApprovedPatientBaseline} + 5 = {h.Profil.ApprovedPatientBaseline + 5}", data.Gates.Single(x => x.Code == "PATIENT_BASELINE").Expected);
        Assert.Equal(5, data.AlreadyMigrated);
        Assert.Equal(20, data.EligibleBeforeRun);
        Assert.Equal(5, data.Migrated);
        Assert.Equal(15, data.Remaining);
        Assert.All(data.Items.Take(5), x => Assert.Equal(RsmmcBulkPatientMigrationStatus.AlreadyMigrated, x.Status));
        Assert.All(data.Items.Skip(5).Take(5), x => Assert.Equal(RsmmcBulkPatientMigrationStatus.Migrated, x.Status));
        Assert.All(data.Items.Skip(10), x => Assert.Equal(RsmmcBulkPatientMigrationStatus.NotProcessed, x.Status));
        Assert.Equal(h.Profil.ApprovedPatientBaseline + 10, h.JumlahPasien());
    }

    [Fact]
    public async Task DiulangSesudahSelesai_Idempoten_TanpaPasienAtauQrKedua()
    {
        using var h = new MigrasiMassalHarness();
        await h.JalankanAsync(false, 25);
        var database = h.SnapshotDatabase();
        var filesystem = h.SnapshotFilesystem();
        var dirender = h.PayloadDirender.Count;

        var data = (await h.JalankanAsync(false, 25)).Data!;

        Assert.All(data.Items, x => Assert.Equal(RsmmcBulkPatientMigrationStatus.AlreadyMigrated, x.Status));
        Assert.Equal(25, data.AlreadyMigrated);
        Assert.Equal(0, data.EligibleBeforeRun);
        Assert.Equal(0, data.Selected);
        Assert.Equal(0, data.Migrated);
        Assert.Equal(0, data.Remaining);
        Assert.False(data.Stopped);
        Assert.Equal(database, h.SnapshotDatabase());
        Assert.Equal(filesystem, h.SnapshotFilesystem());
        Assert.Equal(dirender, h.PayloadDirender.Count);
    }

    [Fact]
    public async Task KasusG_BerkasDiPathBulk_BerhentiPadaPasienKeempat_ContohKontrak()
    {
        using var h = new MigrasiMassalHarness();
        var mrn = MigrasiMassalHarness.MrnUntuk(3);
        MigrasiMassalHarness.TulisFile(h.PathQrBulkFisik(mrn), "QR-YATIM-PERCOBAAN-LALU");

        var data = (await h.JalankanAsync(false, 25)).Data!;

        Assert.Equal(3, data.Migrated);
        Assert.Equal(1, data.Conflict);
        Assert.Equal(21, data.NotProcessed);
        Assert.True(data.Stopped);
        Assert.Contains(h.PidManifest(3).ToString(CultureInfo.InvariantCulture), data.StopReason);
        Assert.Equal(RsmmcBulkPatientConflictType.QrArtifactAlreadyExists, data.Items[3].ConflictType);
        Assert.Equal("QR-YATIM-PERCOBAAN-LALU", File.ReadAllText(h.PathQrBulkFisik(mrn)));
        Assert.Null(h.PasienDenganKode(MigrasiMassalHarness.KodeUntuk(3)));
        Assert.Null(h.CrosswalkUntuk(h.PidManifest(3)));
        Assert.All(Enumerable.Range(4, 21), i => Assert.Null(h.PasienDenganKode(MigrasiMassalHarness.KodeUntuk(i))));
    }

    [Theory]
    [InlineData("crosswalk-tanpa-target", RsmmcBulkPatientConflictType.CrosswalkExistsWithoutTarget)]
    [InlineData("preview-bukan-pending", RsmmcBulkPatientConflictType.PreviewStateChanged)]
    [InlineData("perencana-mrn-beda", RsmmcBulkPatientConflictType.PlannerMismatch)]
    [InlineData("nama-kosong", RsmmcBulkPatientConflictType.PreviewValueInvalid)]
    [InlineData("is-member-true", RsmmcBulkPatientConflictType.PreviewValueInvalid)]
    [InlineData("gender-bukan-anggota", RsmmcBulkPatientConflictType.PreviewValueInvalid)]
    public async Task KonflikTakTerduga_MenghentikanSisaTanpaPasienPengganti(string skenario, string konflik)
    {
        using var h = new MigrasiMassalHarness();
        SimulasiMigrasiMassalTests.SiapkanKeadaan(h, skenario, 3);
        var crosswalkSebelum = h.CrosswalkUntuk(h.PidManifest(3));

        var data = (await h.JalankanAsync(false, 25)).Data!;

        Assert.Equal(3, data.Migrated);
        Assert.Equal(RsmmcBulkPatientMigrationStatus.Conflict, data.Items[3].Status);
        Assert.Equal(konflik, data.Items[3].ConflictType);
        Assert.Equal(21, data.NotProcessed);
        Assert.True(data.Stopped);
        Assert.Equal(25, data.Items.Count);

        // Baris crosswalk historis tanpa target tidak disentuh; tidak ada pasien di luar manifest yang diproses.
        Assert.Equal(crosswalkSebelum, h.CrosswalkUntuk(h.PidManifest(3)));
        Assert.All(MigrasiMassalHarness.PidDiLuarManifest, x => Assert.Null(h.CrosswalkUntuk(x)));
        Assert.All(Enumerable.Range(100, 3), i => Assert.Null(h.PasienDenganKode(MigrasiMassalHarness.KodeUntuk(i))));
        Assert.Equal(3, h.BacaCrosswalk().Count(x => x.MigrationStatus == "MIGRATED_BULK_WORKER"));
    }

    [Fact]
    public async Task SudahDimigrasiDanQrTidakKonsisten_TidakMenghabiskanLimitDanTidakBerhenti()
    {
        using var h = new MigrasiMassalHarness();
        SimulasiMigrasiMassalTests.SiapkanKeadaan(h, "sudah-dimigrasi", 0);
        SimulasiMigrasiMassalTests.SiapkanKeadaan(h, "qr-path-lama", 1);
        var qrLama = h.PasienDenganKode(MigrasiMassalHarness.KodeUntuk(1))!.QrCodePath;

        var data = (await h.JalankanAsync(false, 3)).Data!;

        Assert.Equal(RsmmcBulkPatientMigrationStatus.AlreadyMigrated, data.Items[0].Status);
        Assert.Equal(RsmmcBulkPatientMigrationStatus.MigratedQrInconsistent, data.Items[1].Status);
        Assert.All(data.Items.Skip(2).Take(3), x => Assert.Equal(RsmmcBulkPatientMigrationStatus.Migrated, x.Status));
        Assert.All(data.Items.Skip(5), x => Assert.Equal(RsmmcBulkPatientMigrationStatus.NotProcessed, x.Status));
        Assert.False(data.Stopped);
        Assert.Equal(3, data.Migrated);
        Assert.Equal(2, data.AlreadyMigrated);
        Assert.Equal(23, data.EligibleBeforeRun);
        Assert.Equal(20, data.Remaining);

        // Dilaporkan, tidak diperbaiki.
        Assert.Equal(qrLama, h.PasienDenganKode(MigrasiMassalHarness.KodeUntuk(1))!.QrCodePath);
    }

    [Fact]
    public async Task KunciDipegangSesiLain_ConcurrentRunTanpaMenunggu_LaluBerhasilSesudahLepas()
    {
        using var h = new MigrasiMassalHarness();
        var pid = h.PidManifest(0);
        h.Kendali.KunciDipegangSesiLain.Add((int)pid);

        var data = (await h.JalankanAsync(false, 25)).Data!;

        Assert.Equal(RsmmcBulkPatientMigrationStatus.Conflict, data.Items[0].Status);
        Assert.Equal(RsmmcBulkPatientConflictType.ConcurrentRun, data.Items[0].ConflictType);
        Assert.True(data.Stopped);
        Assert.Equal(24, data.NotProcessed);
        Assert.Equal((RsmmcBulkApprovedBatchProfile.WorkerLockNamespace, -667992), Assert.Single(h.Kendali.KunciDiminta));
        Assert.Equal(1, h.Transaksi.Dibuka);
        Assert.Equal(0, h.Transaksi.Commit);
        Assert.Null(h.PasienDenganKode(MigrasiMassalHarness.KodeUntuk(0)));

        h.Kendali.KunciDipegangSesiLain.Clear();
        var ulang = (await h.JalankanAsync(false, 1)).Data!;

        Assert.Equal(RsmmcBulkPatientMigrationStatus.Migrated, ulang.Items[0].Status);
    }

    [Fact]
    public async Task KeadaanDibacaUlangSesudahKunci_KodeDipakaiSetelahSnapshot_Konflik()
    {
        using var h = new MigrasiMassalHarness();
        var kunciTarget = (int)h.PidManifest(2);
        h.Kendali.SetelahKunci = (kunci, transaksi) =>
        {
            if (kunci == kunciTarget)
            {
                // Proses lain memakai kode pasien ini sesudah snapshot awal, sebelum kunci didapat.
                h.TambahPasien(MigrasiMassalHarness.KodeUntuk(2), "00-99-99-03", transaksi: transaksi);
            }

            return Task.CompletedTask;
        };

        var data = (await h.JalankanAsync(false, 25)).Data!;

        Assert.Equal(2, data.Migrated);
        Assert.Equal(RsmmcBulkPatientConflictType.PatientCodeAlreadyUsed, data.Items[2].ConflictType);
        Assert.True(data.Stopped);
    }

    [Fact]
    public async Task KeadaanDibacaUlangSesudahKunci_SudahDiselesaikanProsesLain_AlreadyMigratedDanLanjut()
    {
        using var h = new MigrasiMassalHarness();
        var kunciTarget = (int)h.PidManifest(1);
        var mrn = MigrasiMassalHarness.MrnUntuk(1);
        h.Kendali.SetelahKunci = (kunci, transaksi) =>
        {
            if (kunci == kunciTarget)
            {
                var id = h.TambahPasien(MigrasiMassalHarness.KodeUntuk(1), mrn, qrCodePath: MigrasiMassalHarness.PathQrBulkPublik(mrn), transaksi: transaksi);
                h.TambahCrosswalk(h.PidManifest(1), id, "TARGET_CREATED", "MIGRATED_BULK_WORKER", MigrasiMassalHarness.BatchKanonik, transaksi);
                MigrasiMassalHarness.TulisFile(h.PathQrBulkFisik(mrn), "QR-BARU:" + mrn);
            }

            return Task.CompletedTask;
        };

        var data = (await h.JalankanAsync(false, 3)).Data!;

        Assert.Equal(RsmmcBulkPatientMigrationStatus.Migrated, data.Items[0].Status);
        Assert.Equal(RsmmcBulkPatientMigrationStatus.AlreadyMigrated, data.Items[1].Status);
        Assert.Equal(RsmmcBulkPatientMigrationStatus.Migrated, data.Items[2].Status);
        Assert.Equal(RsmmcBulkPatientMigrationStatus.Migrated, data.Items[3].Status);
        Assert.Equal(3, data.Migrated);
        Assert.False(data.Stopped);
    }

    [Fact]
    public async Task GagalInsertPasien_FailedInsertPatient_RollbackTanpaSisa()
    {
        using var h = new MigrasiMassalHarness();

        var data = (await h.JalankanAsync(false, 25, default, new GagalSimpanInterceptor(1))).Data!;

        PeriksaGagalTanpaSisa(h, data, 0, RsmmcBulkPatientFailureStage.InsertPatient);
        Assert.Empty(h.PayloadDirender);
    }

    [Fact]
    public async Task GagalInsertCrosswalk_FailedInsertCrosswalk_PasienIkutDibatalkan()
    {
        using var h = new MigrasiMassalHarness();
        h.Kendali.SebelumInsertCrosswalk = _ => throw new InvalidOperationException("Simulasi kegagalan INSERT crosswalk.");

        var data = (await h.JalankanAsync(false, 25)).Data!;

        PeriksaGagalTanpaSisa(h, data, 0, RsmmcBulkPatientFailureStage.InsertCrosswalk);
        Assert.Empty(h.PayloadDirender);
    }

    [Theory]
    [InlineData("IsMember", true)]
    [InlineData("IsActive", false)]
    [InlineData("FullName", "Nama Pengganti Diam-Diam")]
    [InlineData("Citizenship", 1)]
    public async Task NilaiTersimpanBerbedaDariPreview_FailedVerifyPersisted_Rollback(string kolom, object nilai)
    {
        using var h = new MigrasiMassalHarness();

        var data = (await h.JalankanAsync(false, 25, default, new UbahNilaiTersimpanInterceptor(kolom, nilai))).Data!;

        PeriksaGagalTanpaSisa(h, data, 0, RsmmcBulkPatientFailureStage.VerifyPersisted);
        Assert.Contains(kolom, data.Items[0].Message);
        Assert.DoesNotContain(MigrasiMassalHarness.NamaUntuk(0), data.Items[0].Message);

        // QR dibuat sesudah verifikasi, sehingga tidak ada QR yatim.
        Assert.Empty(h.PayloadDirender);
    }

    [Fact]
    public async Task GagalMembentukQr_FailedCreateQr_RollbackTanpaBerkas()
    {
        using var h = new MigrasiMassalHarness { GagalRender = true };

        var data = (await h.JalankanAsync(false, 25)).Data!;

        PeriksaGagalTanpaSisa(h, data, 0, RsmmcBulkPatientFailureStage.CreateQr);
    }

    [Fact]
    public async Task BerkasQrMunculSaatDibentuk_KonflikRace_TidakDitimpaDanDibatalkan()
    {
        using var h = new MigrasiMassalHarness();
        var mrn = MigrasiMassalHarness.MrnUntuk(0);
        h.SaatRender = _ => MigrasiMassalHarness.TulisFile(h.PathQrBulkFisik(mrn), "ISI-PROSES-LAIN");

        var data = (await h.JalankanAsync(false, 25)).Data!;

        Assert.Equal(RsmmcBulkPatientMigrationStatus.Conflict, data.Items[0].Status);
        Assert.Equal(RsmmcBulkPatientConflictType.QrArtifactAlreadyExists, data.Items[0].ConflictType);
        Assert.True(data.Stopped);
        Assert.Equal("ISI-PROSES-LAIN", File.ReadAllText(h.PathQrBulkFisik(mrn)));
        Assert.Null(h.PasienDenganKode(MigrasiMassalHarness.KodeUntuk(0)));
        Assert.Null(h.CrosswalkUntuk(h.PidManifest(0)));
    }

    [Fact]
    public async Task KasusJ_CommitMelemparSebelumTersimpan_QrDipertahankan_PanggilanBerikutnyaKasusG()
    {
        using var h = new MigrasiMassalHarness();
        var mrn = MigrasiMassalHarness.MrnUntuk(0);

        var data = (await h.JalankanAsync(false, 25, default, new GagalCommitInterceptor(1))).Data!;

        Assert.Equal(RsmmcBulkPatientMigrationStatus.Failed, data.Items[0].Status);
        Assert.Equal(RsmmcBulkPatientFailureStage.CommitOutcomeUncertain, data.Items[0].FailureStage);
        Assert.True(data.Stopped);
        Assert.Equal(24, data.NotProcessed);

        // QR baru tidak pernah dihapus otomatis pada hasil commit yang tidak pasti.
        Assert.True(File.Exists(h.PathQrBulkFisik(mrn)));

        var ulang = (await h.JalankanAsync(false, 25)).Data!;

        Assert.Equal(RsmmcBulkPatientConflictType.QrArtifactAlreadyExists, ulang.Items[0].ConflictType);
        Assert.Null(h.PasienDenganKode(MigrasiMassalHarness.KodeUntuk(0)));
        Assert.Equal(h.Profil.ApprovedPatientBaseline, h.JumlahPasien());
    }

    [Fact]
    public async Task KasusJ_CommitTersimpanTetapiMelempar_PanggilanBerikutnyaAlreadyMigrated_TanpaPasienKedua()
    {
        using var h = new MigrasiMassalHarness();

        var data = (await h.JalankanAsync(false, 25, default, new GagalSesudahCommitInterceptor(1))).Data!;

        Assert.Equal(RsmmcBulkPatientFailureStage.CommitOutcomeUncertain, data.Items[0].FailureStage);
        Assert.True(data.Stopped);

        var ulang = (await h.JalankanAsync(false, 1)).Data!;

        Assert.Equal(RsmmcBulkPatientMigrationStatus.AlreadyMigrated, ulang.Items[0].Status);
        Assert.Equal(RsmmcBulkPatientMigrationStatus.Migrated, ulang.Items[1].Status);
        Assert.Equal(h.Profil.ApprovedPatientBaseline + 2, h.JumlahPasien());
    }

    [Fact]
    public async Task LegacyPidDiLuarInt32_FailedLockKeyOutOfRange_TanpaKunciDanBerhenti()
    {
        using var h = new MigrasiMassalHarness(MigrasiMassalHarness.ManifestUji(1, 3000000000, 888));

        var data = (await h.JalankanAsync(false, 25)).Data!;

        Assert.Equal(RsmmcBulkPatientMigrationStatus.Migrated, data.Items[0].Status);
        Assert.Equal(RsmmcBulkPatientMigrationStatus.Failed, data.Items[1].Status);
        Assert.Equal(RsmmcBulkPatientFailureStage.LockKeyOutOfRange, data.Items[1].FailureStage);
        Assert.Equal("3000000000", data.Items[1].LegacyPid);
        Assert.Equal(RsmmcBulkPatientMigrationStatus.NotProcessed, data.Items[2].Status);
        Assert.True(data.Stopped);
        Assert.Equal((RsmmcBulkApprovedBatchProfile.WorkerLockNamespace, 1), Assert.Single(h.Kendali.KunciDiminta));
        Assert.Equal(1, h.Transaksi.Dibuka);
    }

    [Fact]
    public async Task Pembatalan_SebelumPanggilan_TidakAdaPerubahan()
    {
        using var h = new MigrasiMassalHarness();
        var database = h.SnapshotDatabase();
        using var batal = new CancellationTokenSource();
        batal.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => h.JalankanAsync(false, 25, batal.Token));

        Assert.Equal(database, h.SnapshotDatabase());
        Assert.Equal(0, h.Transaksi.Dibuka);
    }

    [Fact]
    public async Task Pembatalan_SesudahMutasiDimulai_PasienItuTetapSelesai_SisanyaNotProcessed()
    {
        using var h = new MigrasiMassalHarness();
        using var batal = new CancellationTokenSource();
        h.Kendali.SebelumInsertCrosswalk = _ => batal.Cancel();

        var data = (await h.JalankanAsync(false, 25, batal.Token)).Data!;

        Assert.Equal(RsmmcBulkPatientMigrationStatus.Migrated, data.Items[0].Status);
        Assert.All(data.Items.Skip(1), x => Assert.Equal(RsmmcBulkPatientMigrationStatus.NotProcessed, x.Status));
        Assert.Equal(1, data.Migrated);
        Assert.True(data.Stopped);
        Assert.Contains("Dibatalkan", data.StopReason);
        Assert.NotNull(h.PasienDenganKode(MigrasiMassalHarness.KodeUntuk(0)));
        Assert.NotNull(h.CrosswalkUntuk(h.PidManifest(0)));
    }

    [Fact]
    public async Task Pembatalan_SaatMembacaUlangSesudahKunci_TanpaPerubahanDanBerhenti()
    {
        using var h = new MigrasiMassalHarness();
        using var batal = new CancellationTokenSource();
        h.Kendali.SetelahKunci = (_, _) =>
        {
            batal.Cancel();
            return Task.CompletedTask;
        };

        var data = (await h.JalankanAsync(false, 25, batal.Token)).Data!;

        Assert.All(data.Items, x => Assert.Equal(RsmmcBulkPatientMigrationStatus.NotProcessed, x.Status));
        Assert.True(data.Stopped);
        Assert.Equal(0, data.Migrated);
        Assert.Equal(1, h.Transaksi.Rollback);
        Assert.Equal(h.Profil.ApprovedPatientBaseline, h.JumlahPasien());
        Assert.Empty(h.PayloadDirender);
    }

    private static MstPatient PeriksaPasienSesuaiPreview(MigrasiMassalHarness h, int urutan, DateTime mulai)
    {
        var preview = h.PreviewManifest(urutan);
        var mrn = MigrasiMassalHarness.MrnUntuk(urutan);
        var pasien = h.PasienDenganKode(preview.ProposedPatientCode!)!;

        Assert.NotNull(pasien);
        Assert.Equal(mrn, pasien.MedicalRecordNumber);
        Assert.Equal(preview.PatientType, (int)pasien.PatientType);
        Assert.Equal(preview.PatientStatus, (int)pasien.PatientStatus);
        Assert.Equal(preview.RegistrationSource, (int)pasien.RegistrationSource);
        Assert.Equal(preview.FullName, pasien.FullName);
        Assert.Equal(preview.BirthDate!.Value.Date, pasien.BirthDate!.Value.Date);
        Assert.Equal(preview.Gender, pasien.Gender is null ? null : (int)pasien.Gender);
        Assert.Equal(preview.Religion, (int)pasien.Religion);
        Assert.Equal(preview.MaritalStatus, (int)pasien.MaritalStatus);
        Assert.Equal(preview.BloodType, (int)pasien.BloodType);
        Assert.Equal(preview.IdentityType, pasien.IdentityType);
        Assert.Equal(preview.IdentityNumber, pasien.IdentityNumber);
        Assert.Equal(preview.PhoneNumber, pasien.PhoneNumber);
        Assert.Equal(preview.Address, pasien.Address);
        Assert.Equal(preview.IsNewborn, pasien.IsNewborn);
        Assert.Equal(preview.IsDeceased, pasien.IsDeceased);
        Assert.Equal(MigrasiMassalHarness.PathQrBulkPublik(mrn), pasien.QrCodePath);

        // PAT-DEC-075: IsMember tersimpan false; IsActive true.
        Assert.False(pasien.IsMember);
        Assert.True(pasien.IsActive);
        Assert.False(pasien.IsDelete);
        Assert.False(pasien.IsCancel);

        // PAT-DEC-025: aktor dan waktu pembuatan.
        Assert.Equal(MigrasiMassalHarness.AktorUji, pasien.CreateBy);
        Assert.InRange(pasien.CreateDateTime, mulai, DateTime.UtcNow.AddSeconds(1));

        // PAT-DEC-076: kolom tanpa pemetaan legacy tetap NULL; kolom lain tidak dikarang.
        Assert.Null(pasien.Citizenship);
        Assert.Null(pasien.NationalityCountryId);
        Assert.Null(pasien.DeliveryCaseId);
        Assert.Null(pasien.NewbornRegistrationKeyHash);
        Assert.Null(pasien.NewbornRegistrationFingerprint);
        Assert.Null(pasien.NickName);
        Assert.Null(pasien.BirthPlace);
        Assert.Null(pasien.WhatsAppNumber);
        Assert.Null(pasien.Email);
        Assert.Null(pasien.PhotoPath);
        Assert.Null(pasien.DeceasedDate);
        Assert.Null(pasien.Notes);
        Assert.Null(pasien.MotherPatientId);
        Assert.Null(pasien.DefaultMembershipTierId);
        Assert.Null(pasien.ActivePatientMembershipId);

        return pasien;
    }

    private static void PeriksaCrosswalkBaru(MigrasiMassalHarness h, int urutan, Guid patientId)
    {
        var preview = h.PreviewManifest(urutan);
        var baris = h.CrosswalkUntuk(preview.LegacyPid)!;

        Assert.NotNull(baris);
        Assert.Null(baris.CanonicalLegacyPid);
        Assert.Equal(preview.LegacyOldRm, baris.LegacyOldRm);
        Assert.Equal(preview.LegacyNormalizedMrn, baris.NormalizedMrn);
        Assert.Equal(preview.LegacyNormalizedNik, baris.NormalizedNik);
        Assert.Equal(patientId, baris.QuilvianPatientId);
        Assert.Equal("TARGET_CREATED", baris.MatchStatus);
        Assert.Equal("MIGRATED_BULK_WORKER", baris.MigrationStatus);
        Assert.Null(baris.ReviewReason);
        Assert.Equal(MigrasiMassalHarness.BatchKanonik, baris.LastBatchId);
        Assert.NotNull(baris.CreatedAt);
        Assert.Null(baris.UpdatedAt);
    }

    private static void PeriksaQrBaru(MigrasiMassalHarness h, int urutan)
    {
        var mrn = MigrasiMassalHarness.MrnUntuk(urutan);

        Assert.Equal("QR-BARU:" + FormatQrPasien.Payload(mrn), File.ReadAllText(h.PathQrBulkFisik(mrn)));
        Assert.Equal(mrn, FormatQrPasien.Payload(mrn));
    }

    private static void PeriksaGagalTanpaSisa(MigrasiMassalHarness h, RsmmcBulkPatientMigrationResponse data, int urutan, string tahap)
    {
        var mrn = MigrasiMassalHarness.MrnUntuk(urutan);

        Assert.Equal(RsmmcBulkPatientMigrationStatus.Failed, data.Items[urutan].Status);
        Assert.Equal(tahap, data.Items[urutan].FailureStage);
        Assert.Equal(1, data.Failed);
        Assert.True(data.Stopped);
        Assert.NotNull(data.StopReason);
        Assert.All(data.Items.Skip(urutan + 1), x => Assert.Equal(RsmmcBulkPatientMigrationStatus.NotProcessed, x.Status));

        Assert.Null(h.PasienDenganKode(MigrasiMassalHarness.KodeUntuk(urutan)));
        Assert.Null(h.CrosswalkUntuk(h.PidManifest(urutan)));
        Assert.False(File.Exists(h.PathQrBulkFisik(mrn)));
        Assert.Equal(h.Profil.ApprovedPatientBaseline, h.JumlahPasien());
        Assert.Equal(0, h.Transaksi.Commit);
    }

    private static List<string> Baris(string snapshot, string awalan) =>
        snapshot.Split('\n').Where(x => x.StartsWith(awalan, StringComparison.Ordinal)).ToList();
}
