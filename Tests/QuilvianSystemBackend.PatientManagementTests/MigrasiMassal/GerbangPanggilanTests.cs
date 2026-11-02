using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;
using QuilvianSystemBackend.Tests.PatientManagement.MigrasiMassal.Infrastruktur;

namespace QuilvianSystemBackend.Tests.PatientManagement.MigrasiMassal;

/// <summary>
/// Gerbang tingkat panggilan kontrak `1.3.0` B4.2 — `PAT-MIG2-AC-03`–`08`, `29`, `46`, `51`;
/// `PAT-DEC-034`, `055`, `056`, `063`, `077`.
/// </summary>
public class GerbangPanggilanTests
{
    private static readonly string[] UrutanGerbang =
    [
        "PREVIEW_SCHEMA_SIGNATURE", "APPROVED_BATCH", "BATCH_ROW_EXISTS", "PLANNER_FINALIZED_ROWS",
        "PREVIEW_ROWS", "PREVIEW_UNIQUE", "DEFERRED_SET", "DEFERRED_RESERVATIONS_UNUSED", "PATIENT_BASELINE",
        "NORMAL_ALLOCATION_GUARD", "PREVIEW_CODE_UNMAPPED", "PREVIEW_MRN_UNMAPPED", "CROSSWALK_DUPLICATE_TARGET",
        "PILOT_CROSSWALK_INTACT", "PILOT_PREVIEW_ROWS", "BACKUP_ROWS", "MANIFEST_INTEGRITY"
    ];

    [Fact]
    public async Task DataBaku_SeluruhGerbangLulus_DalamUrutanKontrak()
    {
        using var h = new MigrasiMassalHarness();

        var hasil = await h.JalankanAsync(true);

        Assert.Equal(UrutanGerbang, hasil.Data!.Gates.Select(x => x.Code));
        Assert.All(hasil.Data.Gates, x => Assert.True(x.Passed, $"{x.Code}: diharapkan {x.Expected}, aktual {x.Actual}"));
    }

    [Theory]
    [InlineData("PREVIEW_SCHEMA_SIGNATURE", "tipe-birth-date")]
    [InlineData("PREVIEW_SCHEMA_SIGNATURE", "kolom-hilang")]
    [InlineData("PREVIEW_SCHEMA_SIGNATURE", "tipe-legacy-pid")]
    [InlineData("BATCH_ROW_EXISTS", "batch-dihapus")]
    [InlineData("PLANNER_FINALIZED_ROWS", "perencana-bukan-finalized")]
    [InlineData("PREVIEW_ROWS", "preview-hilang")]
    [InlineData("PREVIEW_UNIQUE", "kode-ganda")]
    [InlineData("PREVIEW_UNIQUE", "mrn-ganda")]
    [InlineData("DEFERRED_SET", "tertunda-tambahan")]
    [InlineData("DEFERRED_SET", "tertunda-hilang")]
    [InlineData("DEFERRED_RESERVATIONS_UNUSED", "kode-cadangan-dipakai")]
    [InlineData("DEFERRED_RESERVATIONS_UNUSED", "mrn-cadangan-dipakai")]
    [InlineData("PATIENT_BASELINE", "pasien-tambahan")]
    [InlineData("PATIENT_BASELINE", "pasien-terhapus-tetap-dihitung")]
    [InlineData("NORMAL_ALLOCATION_GUARD", "guard-tidak-aktif")]
    [InlineData("NORMAL_ALLOCATION_GUARD", "guard-tidak-dikonfigurasi")]
    [InlineData("PREVIEW_CODE_UNMAPPED", "kode-preview-dipakai")]
    [InlineData("PREVIEW_MRN_UNMAPPED", "mrn-preview-dipakai")]
    [InlineData("CROSSWALK_DUPLICATE_TARGET", "target-ganda")]
    [InlineData("PILOT_CROSSWALK_INTACT", "status-pilot-berubah")]
    [InlineData("PILOT_PREVIEW_ROWS", "preview-pilot-hilang")]
    [InlineData("BACKUP_ROWS", "cadangan-hilang")]
    [InlineData("BACKUP_ROWS", "cadangan-batch-lain")]
    [InlineData("MANIFEST_INTEGRITY", "pasien-manifest-tanpa-preview")]
    [InlineData("MANIFEST_INTEGRITY", "kelompok-sumber-berubah")]
    public async Task GerbangGagal_EksekusiDitolak409TanpaMutasi_SimulasiTetapMelaporkan(string kodeGerbang, string skenario)
    {
        using var h = new MigrasiMassalHarness();
        SiapkanSkenario(h, skenario);

        var database = h.SnapshotDatabase();
        var filesystem = h.SnapshotFilesystem();

        var eksekusi = await h.JalankanAsync(false);

        Assert.Equal(RsmmcBulkPatientMigrationOutcome.GateRejected, eksekusi.Outcome);
        Assert.Equal("RSMMC_BULK_GATE_FAILED", eksekusi.ErrorCode);
        Assert.Contains(eksekusi.Data!.Gates, x => x.Code == kodeGerbang && !x.Passed);
        Assert.Empty(eksekusi.Data.Items);

        // Ditolak sebelum satu pasien pun diproses: tanpa transaksi, kunci, insert, atau folder QR.
        Assert.Equal(0, h.Transaksi.Dibuka);
        Assert.Empty(h.Kendali.KunciDiminta);
        Assert.Equal(0, h.Kendali.JumlahInsertCrosswalk);
        Assert.Empty(h.Perintah.PerintahTulis);
        Assert.Equal(0, h.JumlahResolveStorage);
        Assert.Equal(database, h.SnapshotDatabase());
        Assert.Equal(filesystem, h.SnapshotFilesystem());
        Assert.Contains(h.Log.Entri, x => x.Level == LogLevel.Warning && x.Teks.Contains(kodeGerbang, StringComparison.Ordinal));

        var simulasi = await h.JalankanAsync(true);

        Assert.Equal(RsmmcBulkPatientMigrationOutcome.Completed, simulasi.Outcome);
        Assert.Contains(simulasi.Data!.Gates, x => x.Code == kodeGerbang && !x.Passed);
        Assert.Equal(25, simulasi.Data.Items.Count);
        Assert.Equal(database, h.SnapshotDatabase());
    }

    [Fact]
    public async Task TandaTanganSkemaGagal_KueriBertipeTidakDijalankan_PasienTidakDiklasifikasikan()
    {
        using var h = new MigrasiMassalHarness();
        SiapkanSkenario(h, "tipe-birth-date");

        var hasil = await h.JalankanAsync(true);
        var data = hasil.Data!;

        Assert.Equal(RsmmcBulkPatientMigrationOutcome.Completed, hasil.Outcome);
        Assert.Equal(0, h.Kendali.JumlahPembacaanBertipe);

        var skema = data.Gates.Single(x => x.Code == "PREVIEW_SCHEMA_SIGNATURE");
        Assert.False(skema.Passed);
        Assert.Contains("rsmmc_patient_target_preview.birth_date", skema.Actual);

        // Gerbang yang membutuhkan kueri bertipe tidak dievaluasi.
        foreach (var kode in UrutanGerbang.Except(["PREVIEW_SCHEMA_SIGNATURE", "APPROVED_BATCH", "NORMAL_ALLOCATION_GUARD", "MANIFEST_INTEGRITY"]))
        {
            var gerbang = data.Gates.Single(x => x.Code == kode);
            Assert.False(gerbang.Passed);
            Assert.Equal(RsmmcBulkPatientMigrationService.NotEvaluated, gerbang.Actual);
        }

        Assert.All(data.Items, x => Assert.Equal(RsmmcBulkPatientMigrationStatus.NotProcessed, x.Status));
        Assert.Equal(25, data.NotProcessed);
        Assert.Equal(0, data.Ready);
    }

    [Fact]
    public async Task TandaTanganSkema_FullNameVarcharJugaDiterima()
    {
        using var h = new MigrasiMassalHarness();
        h.Kendali.SkemaKolom.Single(x => x.TableName == SkemaFisikUji.Preview && x.ColumnName == "full_name").DataType = "character varying";

        var hasil = await h.JalankanAsync(true);

        Assert.True(hasil.Data!.Gates.Single(x => x.Code == "PREVIEW_SCHEMA_SIGNATURE").Passed);
    }

    [Fact]
    public async Task BatchSelainKanonik_EksekusiDitolak_SimulasiMelaporkanPreviewBerubah()
    {
        using var h = new MigrasiMassalHarness();
        var batchLain = Guid.Parse("11111111-2222-3333-4444-555555555555");

        var eksekusi = await h.JalankanPermintaanAsync(
            new RsmmcBulkPatientMigrationRequest { BatchId = batchLain, SelectionMode = "CANARY_A", DryRun = false },
            MigrasiMassalHarness.AktorUji);

        Assert.Equal(RsmmcBulkPatientMigrationOutcome.GateRejected, eksekusi.Outcome);
        Assert.Contains(eksekusi.Data!.Gates, x => x.Code == "APPROVED_BATCH" && !x.Passed);
        Assert.Equal(0, h.Transaksi.Dibuka);

        var simulasi = await h.JalankanPermintaanAsync(
            new RsmmcBulkPatientMigrationRequest { BatchId = batchLain, SelectionMode = "CANARY_A" },
            MigrasiMassalHarness.AktorUji);

        Assert.All(simulasi.Data!.Items, x => Assert.Equal(RsmmcBulkPatientConflictType.PreviewStateChanged, x.ConflictType));
    }

    [Fact]
    public async Task PatientBaseline_RumusTetapDitambahCrosswalkWorkerBatchKanonikSaja()
    {
        using var h = new MigrasiMassalHarness();
        var baseline = h.Profil.ApprovedPatientBaseline;

        // Pasien bulk yang sudah dipetakan worker pada batch kanonik: jumlah pasien naik satu, rumus ikut naik.
        var id = h.TambahPasien(MigrasiMassalHarness.KodeUntuk(100), MigrasiMassalHarness.MrnUntuk(100),
            qrCodePath: MigrasiMassalHarness.PathQrBulkPublik(MigrasiMassalHarness.MrnUntuk(100)));
        h.TambahCrosswalk(MigrasiMassalHarness.PidDiLuarManifest[0], id, "TARGET_CREATED", "MIGRATED_BULK_WORKER", MigrasiMassalHarness.BatchKanonik);

        // Crosswalk worker milik batch lain tidak dihitung.
        h.TambahCrosswalk(700002, Guid.NewGuid(), "TARGET_CREATED", "MIGRATED_BULK_WORKER", Guid.Parse("11111111-2222-3333-4444-555555555555"));

        var hasil = await h.JalankanAsync(true);
        var gerbang = hasil.Data!.Gates.Single(x => x.Code == "PATIENT_BASELINE");

        Assert.True(gerbang.Passed, gerbang.Actual);
        Assert.Equal($"{baseline} + 1 = {baseline + 1}", gerbang.Expected);
        Assert.Equal((baseline + 1).ToString(), gerbang.Actual);
        Assert.True(hasil.Data.Gates.Single(x => x.Code == "PREVIEW_CODE_UNMAPPED").Passed);
        Assert.True(hasil.Data.Gates.Single(x => x.Code == "PREVIEW_MRN_UNMAPPED").Passed);
    }

    [Fact]
    public async Task PatientBaseline_NilaiTidakDiambilDariKonfigurasiAtauPermintaan()
    {
        // Profil adalah satu-satunya sumber; permintaan tidak punya field baseline (lihat uji DTO).
        using var h = new MigrasiMassalHarness();
        h.Profil = h.Profil with { ApprovedPatientBaseline = h.Profil.ApprovedPatientBaseline - 1 };

        var hasil = await h.JalankanAsync(false);

        Assert.Equal(RsmmcBulkPatientMigrationOutcome.GateRejected, hasil.Outcome);
        Assert.Contains(hasil.Data!.Gates, x => x.Code == "PATIENT_BASELINE" && !x.Passed);
    }

    [Fact]
    public async Task ManifestUrutanBerbedaDariSidikJariDisetujui_Ditolak()
    {
        var urutan = RsmmcBulkCanaryManifest.CanaryAV1.LegacyPids.Reverse().ToArray();
        var manifest = new RsmmcBulkCanaryManifest(
            "RSMMC-CANARY-A",
            1,
            urutan,
            RsmmcBulkCanaryManifest.CanaryAV1OrderedSha256,
            RsmmcBulkCanaryManifest.CanaryAV1.ExpectedSourceGroupCounts.ToDictionary(x => x.Key, x => x.Value));

        using var h = new MigrasiMassalHarness(manifest);

        var hasil = await h.JalankanAsync(false);

        Assert.Equal(RsmmcBulkPatientMigrationOutcome.GateRejected, hasil.Outcome);
        Assert.Contains(hasil.Data!.Gates, x => x.Code == "MANIFEST_INTEGRITY" && !x.Passed);
    }

    [Fact]
    public async Task ManifestBerisiNilaiGanda_Ditolak()
    {
        using var h = new MigrasiMassalHarness(MigrasiMassalHarness.ManifestUji(1, 888, 1));

        var hasil = await h.JalankanAsync(false);

        Assert.Equal(RsmmcBulkPatientMigrationOutcome.GateRejected, hasil.Outcome);
        Assert.Contains(hasil.Data!.Gates, x => x.Code == "MANIFEST_INTEGRITY" && !x.Passed);
    }

    [Fact]
    public async Task GuardNormal_MemakaiPemeriksaCheckAsyncApaAdanya()
    {
        using var h = new MigrasiMassalHarness();

        var hasil = await h.JalankanAsync(true);
        var gerbang = hasil.Data!.Gates.Single(x => x.Code == "NORMAL_ALLOCATION_GUARD");

        Assert.True(gerbang.Passed);
        Assert.Contains("overallGuardReady = true", gerbang.Actual);
        Assert.Contains("nextPatientCode = PAT-RSMMC-705250", gerbang.Actual);
        Assert.Contains("nextMrn = 00-89-01-88", gerbang.Actual);
    }

    private static void SiapkanSkenario(MigrasiMassalHarness h, string skenario)
    {
        var pidManifest = h.PidManifest(3);
        var pidLuar = MigrasiMassalHarness.PidDiLuarManifest;

        switch (skenario)
        {
            case "tipe-birth-date":
                h.Kendali.SkemaKolom.Single(x => x.TableName == SkemaFisikUji.Preview && x.ColumnName == "birth_date").DataType = "timestamp without time zone";
                break;
            case "kolom-hilang":
                h.Kendali.SkemaKolom.RemoveAll(x => x.TableName == SkemaFisikUji.Crosswalk && x.ColumnName == "last_batch_id");
                break;
            case "tipe-legacy-pid":
                h.Kendali.SkemaKolom.Single(x => x.TableName == SkemaFisikUji.Perencana && x.ColumnName == "legacy_pid").DataType = "text";
                break;
            case "batch-dihapus":
                h.Jalankan("DELETE FROM migration.rsmmc_patient_batch");
                break;
            case "perencana-bukan-finalized":
                h.UbahPerencana(pidLuar[0], "plan_status", "PENDING");
                break;
            case "preview-hilang":
                h.Jalankan("DELETE FROM migration.rsmmc_patient_target_preview WHERE legacy_pid = $p", ("$p", pidLuar[0]));
                break;
            case "kode-ganda":
                h.UbahPreview(pidLuar[1], "proposed_patient_code", MigrasiMassalHarness.KodeUntuk(100));
                break;
            case "mrn-ganda":
                h.UbahPreview(pidLuar[1], "proposed_medical_record_number", MigrasiMassalHarness.MrnUntuk(100));
                break;
            case "tertunda-tambahan":
                h.TambahPerencana(700001, "00-88-00-09");
                break;
            case "tertunda-hilang":
                h.Jalankan("DELETE FROM migration.rsmmc_patient_mrn_plan WHERE legacy_pid = 280143");
                h.Profil = h.Profil with { ExpectedFinalizedPlannerRows = h.Profil.ExpectedFinalizedPlannerRows - 1 };
                break;
            case "kode-cadangan-dipakai":
                h.TambahPasien("PAT-RSMMC-101416", "00-99-99-01");
                break;
            case "mrn-cadangan-dipakai":
                h.TambahPasien("PAT-LAIN-00001", MigrasiMassalHarness.MrnTertunda[1]);
                break;
            case "pasien-tambahan":
                // Pendaftaran Staging yang melanggar pembekuan: guard memberi nomor di atas lantai.
                h.TambahPasien("PAT-RSMMC-705250", "00-89-01-88");
                break;
            case "pasien-terhapus-tetap-dihitung":
                h.TambahPasien("PAT-RSMMC-705250", "00-89-01-88", terhapus: true);
                break;
            case "guard-tidak-aktif":
                h.KonfigurasiGuard = KonfigurasiGuardUji.EnabledFalse;
                break;
            case "guard-tidak-dikonfigurasi":
                h.KonfigurasiGuard = KonfigurasiGuardUji.BagianTidakAda;
                break;
            case "kode-preview-dipakai":
                h.TambahPasien(MigrasiMassalHarness.KodeUntuk(100), "00-99-99-02");
                break;
            case "mrn-preview-dipakai":
                h.TambahPasien("PAT-LAIN-00002", MigrasiMassalHarness.MrnUntuk(101));
                break;
            case "target-ganda":
                h.TambahCrosswalk(700003, h.PasienDenganKode("PAT-RSMMC-00901")!.Id, "TARGET_CREATED", "RAW", null);
                break;
            case "status-pilot-berubah":
                h.Jalankan("UPDATE migration.rsmmc_patient_crosswalk SET migration_status = 'RAW' WHERE legacy_pid = 9001");
                break;
            case "preview-pilot-hilang":
                h.Jalankan("DELETE FROM migration.rsmmc_patient_target_preview WHERE legacy_pid = 9001");
                break;
            case "cadangan-hilang":
                h.Jalankan("DELETE FROM migration.rsmmc_patient_pilot_reconcile_backup WHERE legacy_pid = 9002");
                break;
            case "cadangan-batch-lain":
                h.Jalankan(
                    "INSERT INTO migration.rsmmc_patient_pilot_reconcile_backup (batch_id, legacy_pid) VALUES ('11111111-2222-3333-4444-555555555555', 9999)");
                break;
            case "pasien-manifest-tanpa-preview":
                h.Jalankan("DELETE FROM migration.rsmmc_patient_target_preview WHERE legacy_pid = $p", ("$p", pidManifest));
                break;
            case "kelompok-sumber-berubah":
                h.UbahPreview(h.PidManifest(0), "source_group", "REG_WITH_RM");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(skenario), skenario, null);
        }
    }
}
