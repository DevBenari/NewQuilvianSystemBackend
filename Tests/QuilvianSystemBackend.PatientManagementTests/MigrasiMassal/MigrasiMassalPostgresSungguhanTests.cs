using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using NpgsqlTypes;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;
using QuilvianSystemBackend.Tests.PatientManagement.MigrasiMassal.Infrastruktur;

namespace QuilvianSystemBackend.Tests.PatientManagement.MigrasiMassal;

/// <summary>
/// Bukti provider-real worker migrasi massal pada PostgreSQL 15 sekali pakai — `PAT-MIG2-AC-47`,
/// `PAT-DEC-065`, `PAT-DEC-075`, kontrak `1.3.0` B15.
/// </summary>
/// <remarks>
/// <para>
/// Memakai <see cref="RsmmcBulkMigrationStore"/> produksi — SQL bertipe, kunci dua bagian, INSERT
/// crosswalk — dan konfigurasi EF <c>MstPatient</c> aplikasi yang sebenarnya (<c>EnsureCreated</c>), di
/// basis data acak yang dihapus kembali sesudah setiap uji, juga saat uji gagal. Skema
/// <c>migration.rsmmc_*</c> dibentuk ulang dari `PAT-FACT-079`–`083`; tabel cadangan hanya dihitung.
/// Tidak pernah memakai DEV, STAGING, maupun PRODUCTION.
/// </para>
/// <para>
/// Cara menjalankan, dengan server PostgreSQL 15 sekali pakai yang disediakan sendiri:
/// <c>QUILVIAN_PG_PROOF_CONNECTION="Host=…;Username=…;Password=…;Database=postgres"</c>,
/// <c>QUILVIAN_PG_PROOF_REQUIRED=1</c>, lalu
/// <c>dotnet test Tests/QuilvianSystemBackend.PatientManagementTests --filter Kategori=ProviderReal
/// -- xUnit.ParallelizeTestCollections=false xUnit.MaxParallelThreads=1</c> — serial; server bukti mungkin
/// membutuhkan <c>max_locks_per_transaction = 2048</c> (lihat <see cref="PostgresSekaliPakai"/>).
/// </para>
/// </remarks>
[Trait("Kategori", "ProviderReal")]
public class MigrasiMassalPostgresSungguhanTests
{
    private const string SkemaMigrasi = @"
CREATE SCHEMA migration;
CREATE TABLE migration.rsmmc_patient_batch (batch_id uuid PRIMARY KEY);
CREATE TABLE migration.rsmmc_patient_target_preview (
    batch_id uuid NOT NULL,
    legacy_pid bigint NOT NULL,
    source_group varchar(40) NOT NULL,
    legacy_old_rm text,
    legacy_normalized_mrn text,
    legacy_normalized_nik text,
    proposed_patient_code text NOT NULL,
    proposed_medical_record_number text NOT NULL,
    patient_type integer,
    patient_status integer,
    registration_source integer,
    gender integer,
    religion integer,
    marital_status integer,
    blood_type integer,
    full_name text,
    birth_date date,
    identity_type text,
    identity_number text,
    phone_number text,
    address text,
    is_member boolean,
    is_newborn boolean,
    is_deceased boolean,
    is_active boolean,
    validation_status varchar(40) DEFAULT 'PENDING',
    validation_reason text,
    created_at timestamptz DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (batch_id, legacy_pid));
CREATE TABLE migration.rsmmc_patient_mrn_plan (
    batch_id uuid NOT NULL,
    legacy_pid bigint NOT NULL,
    final_medical_record_number varchar(50),
    target_collision boolean NOT NULL DEFAULT false,
    target_collision_patient_id uuid NULL,
    plan_status varchar(30) NOT NULL DEFAULT 'PENDING',
    created_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamptz NULL,
    PRIMARY KEY (batch_id, legacy_pid));
CREATE TABLE migration.rsmmc_patient_crosswalk (
    legacy_pid bigint NOT NULL PRIMARY KEY,
    canonical_legacy_pid bigint NULL,
    legacy_old_rm text NULL,
    normalized_mrn text NULL,
    normalized_nik text NULL,
    quilvian_patient_id uuid NULL,
    match_status varchar(30) NOT NULL DEFAULT 'UNPROCESSED',
    migration_status varchar(30) NOT NULL DEFAULT 'RAW',
    review_reason text NULL,
    last_batch_id uuid NULL REFERENCES migration.rsmmc_patient_batch (batch_id),
    created_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamptz NULL);
CREATE TABLE migration.rsmmc_patient_pilot_reconcile_backup (batch_id uuid NOT NULL, legacy_pid bigint NOT NULL);";

    private static readonly Guid BatchKanonik = RsmmcBulkApprovedBatchProfile.Canonical.BatchId;
    private static readonly Guid BatchPilot = RsmmcBulkApprovedBatchProfile.Canonical.PilotBatchId;
    private static readonly Guid Aktor = Guid.Parse("0b5e7a10-0000-4000-8000-00000000e015");
    private static readonly DateTime WaktuDibuat = new(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc);

    [FactPostgresSungguhan]
    public async Task A_SimulasiPadaPostgres15_SeluruhGerbangLulusDanTanpaPenulisan()
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        using var lingkungan = new LingkunganBukti(pg);
        await lingkungan.SiapkanAsync();
        var pasienSebelum = await lingkungan.HitungAsync("SELECT count(*) FROM \"MstPatient\"");
        var crosswalkSebelum = await lingkungan.HitungAsync("SELECT count(*) FROM migration.rsmmc_patient_crosswalk");

        var hasil = await lingkungan.JalankanAsync(dryRun: true, limit: 25);

        Assert.Equal(PostgresSekaliPakai.VersiMayorDiwajibkan, pg.VersiServer.Major);
        Assert.Equal(RsmmcBulkPatientMigrationOutcome.Completed, hasil.Outcome);
        Assert.All(hasil.Data!.Gates, x => Assert.True(x.Passed, $"{x.Code}: diharapkan {x.Expected}, aktual {x.Actual}"));
        Assert.All(hasil.Data.Items, x => Assert.Equal(RsmmcBulkPatientMigrationStatus.Ready, x.Status));
        Assert.Equal(pasienSebelum, await lingkungan.HitungAsync("SELECT count(*) FROM \"MstPatient\""));
        Assert.Equal(crosswalkSebelum, await lingkungan.HitungAsync("SELECT count(*) FROM migration.rsmmc_patient_crosswalk"));
        Assert.Empty(lingkungan.Perintah.PerintahTulis);
        Assert.False(Directory.Exists(Path.Combine(lingkungan.StorageRoot, "patient-qrcodes", "rsmmc-bulk")));
    }

    [FactPostgresSungguhan]
    public async Task B_Eksekusi_IsMemberFalseTersimpanFisikLewatInsert_IsActiveTrue_SesuaiPreview()
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        using var lingkungan = new LingkunganBukti(pg);
        await lingkungan.SiapkanAsync();

        var hasil = await lingkungan.JalankanAsync(dryRun: false, limit: 1);
        var item = hasil.Data!.Items[0];
        var preview = MigrasiMassalHarness.BuatPreview(BatchKanonik, -667992, 0);

        Assert.Equal("-667992", item.LegacyPid);
        Assert.Equal(RsmmcBulkPatientMigrationStatus.Migrated, item.Status);

        // Nilai fisik dibaca dengan SQL mentah dari sesi lain, bukan dari entity yang disiapkan.
        await using (var koneksi = new NpgsqlConnection(pg.KoneksiDatabase))
        {
            await koneksi.OpenAsync();
            await using var perintah = new NpgsqlCommand(
                "SELECT \"IsMember\", \"IsActive\", \"IsNewborn\", \"IsDeceased\", \"BirthDate\", \"CreateBy\", \"QrCodePath\" " +
                "FROM \"MstPatient\" WHERE \"PatientCode\" = @kode", koneksi);
            perintah.Parameters.Add(new NpgsqlParameter("kode", NpgsqlDbType.Text) { Value = preview.ProposedPatientCode });

            await using var r = await perintah.ExecuteReaderAsync();
            Assert.True(await r.ReadAsync());
            Assert.False(r.GetBoolean(0));
            Assert.True(r.GetBoolean(1));
            Assert.Equal(preview.IsNewborn, r.GetBoolean(2));
            Assert.Equal(preview.IsDeceased, r.GetBoolean(3));
            Assert.Equal(preview.BirthDate!.Value.Date, r.GetDateTime(4).Date);
            Assert.Equal(Aktor, r.GetGuid(5));
            Assert.Equal(MigrasiMassalHarness.PathQrBulkPublik(preview.ProposedMedicalRecordNumber!), r.GetString(6));
            Assert.False(await r.ReadAsync());
        }

        // Pembacaan ulang AsNoTracking cocok dengan preview pada setiap kolom yang diperiksa worker.
        await using (var konteks = pg.BuatKonteks())
        {
            var tersimpan = await konteks.Set<MstPatient>().IgnoreQueryFilters().AsNoTracking()
                .SingleAsync(x => x.PatientCode == preview.ProposedPatientCode);
            var diharapkan = RsmmcBulkPatientMapper.ToPatient(
                preview, preview.ProposedMedicalRecordNumber!, MigrasiMassalHarness.PathQrBulkPublik(preview.ProposedMedicalRecordNumber!), Aktor, DateTime.UtcNow);

            Assert.Empty(RsmmcBulkPatientMapper.FindPersistedDifferences(diharapkan, tersimpan));
            Assert.Equal(item.PatientId, tersimpan.Id);
        }

        // Crosswalk baru persis kolom PAT-DEC-047.
        var crosswalk = await lingkungan.BacaCrosswalkAsync(-667992);
        Assert.Equal(item.PatientId, crosswalk.Target);
        Assert.Equal("TARGET_CREATED", crosswalk.Match);
        Assert.Equal("MIGRATED_BULK_WORKER", crosswalk.Migration);
        Assert.Equal(BatchKanonik, crosswalk.LastBatch);
        Assert.True(crosswalk.AdaCreatedAt);
        Assert.True(crosswalk.KolomKosongTetapKosong);

        // Hanya INSERT; tidak ada UPDATE IsMember sesudahnya.
        Assert.DoesNotContain(lingkungan.Perintah.PerintahTulis, x => x.TrimStart().StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase));

        // Kunci transaksi lepas bersama transaksi pasien.
        Assert.True(await lingkungan.CobaKunciDariSesiLainAsync(RsmmcBulkApprovedBatchProfile.WorkerLockNamespace, -667992));
    }

    [FactPostgresSungguhan]
    public async Task C_D_RollbackTanpaSisa_LaluPercobaanUlangBerhasil_DanIdempoten()
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        using var lingkungan = new LingkunganBukti(pg);
        await lingkungan.SiapkanAsync();
        var pasienAwal = await lingkungan.HitungAsync("SELECT count(*) FROM \"MstPatient\"");
        var kode = MigrasiMassalHarness.KodeUntuk(0);

        // Gagal sesudah INSERT pasien, INSERT crosswalk, dan verifikasi — saat QR dibentuk.
        lingkungan.GagalRender = true;
        var gagal = (await lingkungan.JalankanAsync(dryRun: false, limit: 1)).Data!;

        Assert.Equal(RsmmcBulkPatientFailureStage.CreateQr, gagal.Items[0].FailureStage);
        Assert.Equal(pasienAwal, await lingkungan.HitungAsync("SELECT count(*) FROM \"MstPatient\""));
        Assert.Equal(0, await lingkungan.HitungAsync($"SELECT count(*) FROM \"MstPatient\" WHERE \"PatientCode\" = '{kode}'"));
        Assert.Equal(0, await lingkungan.HitungAsync("SELECT count(*) FROM migration.rsmmc_patient_crosswalk WHERE legacy_pid = -667992"));

        lingkungan.GagalRender = false;
        var ulang = (await lingkungan.JalankanAsync(dryRun: false, limit: 1)).Data!;
        var lagi = (await lingkungan.JalankanAsync(dryRun: false, limit: 1)).Data!;

        Assert.Equal(RsmmcBulkPatientMigrationStatus.Migrated, ulang.Items[0].Status);
        Assert.Equal(RsmmcBulkPatientMigrationStatus.AlreadyMigrated, lagi.Items[0].Status);
        Assert.Equal(RsmmcBulkPatientMigrationStatus.Migrated, lagi.Items[1].Status);
        Assert.Equal(1, await lingkungan.HitungAsync($"SELECT count(*) FROM \"MstPatient\" WHERE \"PatientCode\" = '{kode}'"));
        Assert.Equal(1, await lingkungan.HitungAsync("SELECT count(*) FROM migration.rsmmc_patient_crosswalk WHERE legacy_pid = -667992"));
        Assert.Equal(pasienAwal + 2, await lingkungan.HitungAsync("SELECT count(*) FROM \"MstPatient\""));
    }

    [FactPostgresSungguhan]
    public async Task E_GagalSebelumCommit_TidakAdaPemetaanTersimpan_PanggilanBerikutnyaTanpaPasienKedua()
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        using var lingkungan = new LingkunganBukti(pg);
        await lingkungan.SiapkanAsync();
        var pasienAwal = await lingkungan.HitungAsync("SELECT count(*) FROM \"MstPatient\"");

        var gagal = (await lingkungan.JalankanAsync(dryRun: false, limit: 1, new GagalCommitInterceptor(1))).Data!;

        Assert.Equal(RsmmcBulkPatientFailureStage.CommitOutcomeUncertain, gagal.Items[0].FailureStage);
        Assert.Equal(pasienAwal, await lingkungan.HitungAsync("SELECT count(*) FROM \"MstPatient\""));
        Assert.Equal(0, await lingkungan.HitungAsync("SELECT count(*) FROM migration.rsmmc_patient_crosswalk WHERE legacy_pid = -667992"));

        var ulang = (await lingkungan.JalankanAsync(dryRun: false, limit: 1)).Data!;

        Assert.Equal(RsmmcBulkPatientConflictType.QrArtifactAlreadyExists, ulang.Items[0].ConflictType);
        Assert.Equal(pasienAwal, await lingkungan.HitungAsync("SELECT count(*) FROM \"MstPatient\""));
    }

    [FactPostgresSungguhan]
    public async Task F_KunciDipegangSesiLain_ConcurrentRunTanpaMenunggu_KunciAllocatorNormalTidakBerbagiRuang()
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        using var lingkungan = new LingkunganBukti(pg);
        await lingkungan.SiapkanAsync();

        await using (var pemegang = new NpgsqlConnection(pg.KoneksiDatabase))
        {
            await pemegang.OpenAsync();
            await using var transaksi = await pemegang.BeginTransactionAsync();
            await using (var kunci = new NpgsqlCommand("SELECT pg_advisory_xact_lock(1173286842, -667992)", pemegang, transaksi))
            {
                await kunci.ExecuteNonQueryAsync();
            }

            var waktu = Stopwatch.StartNew();
            var data = (await lingkungan.JalankanAsync(dryRun: false, limit: 1)).Data!;
            waktu.Stop();

            Assert.Equal(RsmmcBulkPatientConflictType.ConcurrentRun, data.Items[0].ConflictType);
            Assert.True(data.Stopped);
            Assert.True(waktu.Elapsed < TimeSpan.FromSeconds(20), $"Worker menunggu kunci {waktu.Elapsed}.");

            await transaksi.RollbackAsync();
        }

        // Kunci allocator pendaftaran normal (BE-PAT-MIG-003) tidak menahan worker.
        await using (var pemegang = new NpgsqlConnection(pg.KoneksiDatabase))
        {
            await pemegang.OpenAsync();
            await using var transaksi = await pemegang.BeginTransactionAsync();
            await using (var kunci = new NpgsqlCommand(
                $"SELECT pg_advisory_xact_lock({RsmmcIdentifierReservationProfile.AllocationLockNamespace}, {RsmmcIdentifierReservationProfile.AllocationLockId})",
                pemegang,
                transaksi))
            {
                await kunci.ExecuteNonQueryAsync();
            }

            var data = (await lingkungan.JalankanAsync(dryRun: false, limit: 1)).Data!;

            Assert.Equal(RsmmcBulkPatientMigrationStatus.Migrated, data.Items[0].Status);
            await transaksi.RollbackAsync();
        }
    }

    [FactPostgresSungguhan]
    public async Task G_TipeFisikBerbeda_TandaTanganSkemaGagal_EksekusiDitolak()
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        using var lingkungan = new LingkunganBukti(pg);
        await lingkungan.SiapkanAsync();
        await lingkungan.JalankanSqlAsync("ALTER TABLE migration.rsmmc_patient_target_preview ALTER COLUMN birth_date TYPE timestamp");

        var simulasi = await lingkungan.JalankanAsync(dryRun: true, limit: 25);
        var eksekusi = await lingkungan.JalankanAsync(dryRun: false, limit: 25);

        Assert.Contains(simulasi.Data!.Gates, x => x.Code == "PREVIEW_SCHEMA_SIGNATURE" && !x.Passed);
        Assert.All(simulasi.Data.Items, x => Assert.Equal(RsmmcBulkPatientMigrationStatus.NotProcessed, x.Status));
        Assert.Equal(RsmmcBulkPatientMigrationOutcome.GateRejected, eksekusi.Outcome);
    }

    /// <summary>Basis data bukti yang sudah berisi skema migrasi dan data baku kecil, serta folder QR sementara.</summary>
    private sealed class LingkunganBukti(PostgresSekaliPakai pg) : IDisposable
    {
        private RsmmcBulkApprovedBatchProfile _profil = RsmmcBulkApprovedBatchProfile.Canonical;

        public string StorageRoot { get; } = Path.Combine(Path.GetTempPath(), "quilvian-pat-mig2-pg", Guid.NewGuid().ToString("N"));

        public PenghitungPerintahDb Perintah { get; } = new();

        public bool GagalRender { get; set; }

        public async Task SiapkanAsync()
        {
            await JalankanSqlAsync(SkemaMigrasi);
            await JalankanSqlAsync("INSERT INTO migration.rsmmc_patient_batch (batch_id) VALUES (@b)", Uuid("b", BatchKanonik));

            var pilot = new List<Guid>();

            await using (var konteks = pg.BuatKonteks())
            {
                foreach (var (kode, mrn) in new[] { ("PAT-RSMMC-00001", "00-00-00-01"), ("PAT-RSMMC-00901", "00-79-70-10") })
                {
                    var id = Guid.NewGuid();
                    konteks.Set<MstPatient>().Add(new MstPatient
                    {
                        Id = id,
                        PatientCode = kode,
                        MedicalRecordNumber = mrn,
                        FullName = "Pasien Lama Uji",
                        CreateDateTime = WaktuDibuat
                    });
                    pilot.Add(id);
                }

                await konteks.SaveChangesAsync();
            }

            // Satu pemetaan Pilot beserta preview Pilot, perencana, dan cadangannya.
            await TambahPerencanaAsync(9001, "00-79-70-10");
            await JalankanSqlAsync(
                "INSERT INTO migration.rsmmc_patient_crosswalk (legacy_pid, quilvian_patient_id, match_status, migration_status, last_batch_id) " +
                "VALUES (9001, @t, 'TARGET_CREATED', 'MIGRATED_API_AUTO', @b)",
                Uuid("t", pilot[1]),
                Uuid("b", BatchKanonik));
            await TambahPreviewAsync(MigrasiMassalHarness.BuatPreview(BatchPilot, 9001, 900));
            await JalankanSqlAsync(
                "INSERT INTO migration.rsmmc_patient_pilot_reconcile_backup (batch_id, legacy_pid) VALUES (@b, 9001)",
                Uuid("b", BatchKanonik));

            // Crosswalk historis tanpa target.
            await JalankanSqlAsync("INSERT INTO migration.rsmmc_patient_crosswalk (legacy_pid) VALUES (600001)");

            var manifest = RsmmcBulkCanaryManifest.CanaryAV1;

            for (var i = 0; i < manifest.LegacyPids.Count; i++)
            {
                var preview = MigrasiMassalHarness.BuatPreview(BatchKanonik, manifest.LegacyPids[i], i);
                await TambahPreviewAsync(preview);
                await TambahPerencanaAsync(preview.LegacyPid, preview.ProposedMedicalRecordNumber!);
            }

            await TambahPerencanaAsync(280143, MigrasiMassalHarness.MrnTertunda[0]);
            await TambahPerencanaAsync(805596, MigrasiMassalHarness.MrnTertunda[1]);

            _profil = RsmmcBulkApprovedBatchProfile.Canonical with
            {
                ExpectedPreviewRows = manifest.LegacyPids.Count,
                ExpectedFinalizedPlannerRows = manifest.LegacyPids.Count + 3,
                ApprovedPatientBaseline = 2,
                ExpectedPilotTargets = 1,
                ExpectedPilotAutoMigrated = 1,
                ExpectedPilotManualMigrated = 0,
                ExpectedPilotPreviewRows = 1,
                ExpectedBackupRows = 1
            };
        }

        public async Task<RsmmcBulkPatientMigrationResult> JalankanAsync(bool dryRun, int limit, params IInterceptor[] interceptors)
        {
            IInterceptor[] semuaInterceptor = [Perintah, .. interceptors];
            await using var konteks = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(pg.KoneksiDatabase)
                .AddInterceptors(semuaInterceptor)
                .Options);
            var environment = new LingkunganUji("Staging");

            var service = new RsmmcBulkPatientMigrationService(
                konteks,
                new RsmmcBulkMigrationStore(konteks),
                environment,
                new LoggerService(new PencatatLogUji(), new HttpContextAccessor()),
                _profil,
                RsmmcBulkCanaryManifest.CanaryAV1,
                new PatientIdentifierAllocationService(konteks, environment, ReservasiHarness.BuatKonfigurasi(KonfigurasiGuardUji.EnabledTrue)));

            return await service.MigrateAsync(
                new RsmmcBulkPatientMigrationRequest { BatchId = BatchKanonik, SelectionMode = "CANARY_A", DryRun = dryRun, Limit = limit },
                Aktor,
                BuatStoreQr(),
                new RsmmcQrDestinationProbe(
                    () => StorageRoot,
                    new RsmmcPilotQrArtifactLayout("/uploads", Path.Combine("patient-qrcodes", "rsmmc-bulk"), "qrcode.png"),
                    new RsmmcPilotQrArtifactLayout("/uploads", "patient-qrcodes", "qrcode.png"),
                    FormatQrPasien.Sanitize),
                "uji-pg-mig2",
                CancellationToken.None);
        }

        public async Task<long> HitungAsync(string sql)
        {
            await using var koneksi = new NpgsqlConnection(pg.KoneksiDatabase);
            await koneksi.OpenAsync();
            await using var perintah = new NpgsqlCommand(sql, koneksi);

            return Convert.ToInt64(await perintah.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
        }

        public async Task<(Guid? Target, string Match, string Migration, Guid? LastBatch, bool AdaCreatedAt, bool KolomKosongTetapKosong)> BacaCrosswalkAsync(long legacyPid)
        {
            await using var koneksi = new NpgsqlConnection(pg.KoneksiDatabase);
            await koneksi.OpenAsync();
            await using var perintah = new NpgsqlCommand(
                "SELECT quilvian_patient_id, match_status, migration_status, last_batch_id, created_at IS NOT NULL, " +
                "canonical_legacy_pid IS NULL AND review_reason IS NULL AND updated_at IS NULL " +
                "FROM migration.rsmmc_patient_crosswalk WHERE legacy_pid = @p",
                koneksi);
            perintah.Parameters.Add(new NpgsqlParameter("p", NpgsqlDbType.Bigint) { Value = legacyPid });

            await using var r = await perintah.ExecuteReaderAsync();
            Assert.True(await r.ReadAsync());

            return (
                r.IsDBNull(0) ? null : r.GetGuid(0),
                r.GetString(1),
                r.GetString(2),
                r.IsDBNull(3) ? null : r.GetGuid(3),
                r.GetBoolean(4),
                r.GetBoolean(5));
        }

        public async Task<bool> CobaKunciDariSesiLainAsync(int namespaceKunci, int kunci)
        {
            await using var koneksi = new NpgsqlConnection(pg.KoneksiDatabase);
            await koneksi.OpenAsync();
            await using var transaksi = await koneksi.BeginTransactionAsync();
            await using var perintah = new NpgsqlCommand($"SELECT pg_try_advisory_xact_lock({namespaceKunci}, {kunci})", koneksi, transaksi);

            var didapat = (bool)(await perintah.ExecuteScalarAsync())!;
            await transaksi.RollbackAsync();
            return didapat;
        }

        public async Task JalankanSqlAsync(string sql, params NpgsqlParameter[] parameter)
        {
            await using var koneksi = new NpgsqlConnection(pg.KoneksiDatabase);
            await koneksi.OpenAsync();
            await using var perintah = new NpgsqlCommand(sql, koneksi);
            perintah.Parameters.AddRange(parameter);
            await perintah.ExecuteNonQueryAsync();
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(StorageRoot))
                {
                    Directory.Delete(StorageRoot, recursive: true);
                }
            }
            catch (IOException)
            {
                // Folder sementara milik uji; sisa penguncian berkas tidak boleh menggagalkan uji.
            }
        }

        private RsmmcPilotQrArtifactStore BuatStoreQr() =>
            new(
                new RsmmcPilotQrArtifactLayout("/uploads", Path.Combine("patient-qrcodes", "rsmmc-bulk"), "qrcode.png"),
                () =>
                {
                    Directory.CreateDirectory(StorageRoot);
                    return (StorageRoot, null);
                },
                new RsmmcPilotQrArtifactFormat(
                    FormatQrPasien.Sanitize,
                    FormatQrPasien.Payload,
                    (payload, _, _) => GagalRender
                        ? throw new InvalidOperationException("Simulasi kegagalan membentuk PNG QR.")
                        : Encoding.UTF8.GetBytes("QR-BARU:" + payload),
                    FormatQrPasien.Combine));

        private Task TambahPerencanaAsync(long legacyPid, string final) =>
            JalankanSqlAsync(
                "INSERT INTO migration.rsmmc_patient_mrn_plan (batch_id, legacy_pid, final_medical_record_number, plan_status) VALUES (@b, @p, @f, 'FINALIZED')",
                Uuid("b", BatchKanonik),
                new NpgsqlParameter("p", NpgsqlDbType.Bigint) { Value = legacyPid },
                new NpgsqlParameter("f", NpgsqlDbType.Varchar) { Value = final });

        private Task TambahPreviewAsync(RsmmcBulkPreviewRow p) =>
            JalankanSqlAsync(
                "INSERT INTO migration.rsmmc_patient_target_preview " +
                "(batch_id, legacy_pid, source_group, legacy_old_rm, legacy_normalized_mrn, legacy_normalized_nik, " +
                "proposed_patient_code, proposed_medical_record_number, patient_type, patient_status, registration_source, " +
                "gender, religion, marital_status, blood_type, full_name, birth_date, identity_type, identity_number, " +
                "phone_number, address, is_member, is_newborn, is_deceased, is_active, validation_status) VALUES " +
                "(@batch_id, @legacy_pid, @source_group, @legacy_old_rm, @legacy_normalized_mrn, @legacy_normalized_nik, " +
                "@proposed_patient_code, @proposed_medical_record_number, @patient_type, @patient_status, @registration_source, " +
                "@gender, @religion, @marital_status, @blood_type, @full_name, @birth_date, @identity_type, @identity_number, " +
                "@phone_number, @address, @is_member, @is_newborn, @is_deceased, @is_active, @validation_status)",
                Uuid("batch_id", p.BatchId),
                new NpgsqlParameter("legacy_pid", NpgsqlDbType.Bigint) { Value = p.LegacyPid },
                Teks("source_group", p.SourceGroup, NpgsqlDbType.Varchar),
                Teks("legacy_old_rm", p.LegacyOldRm),
                Teks("legacy_normalized_mrn", p.LegacyNormalizedMrn),
                Teks("legacy_normalized_nik", p.LegacyNormalizedNik),
                Teks("proposed_patient_code", p.ProposedPatientCode),
                Teks("proposed_medical_record_number", p.ProposedMedicalRecordNumber),
                Bulat("patient_type", p.PatientType),
                Bulat("patient_status", p.PatientStatus),
                Bulat("registration_source", p.RegistrationSource),
                Bulat("gender", p.Gender),
                Bulat("religion", p.Religion),
                Bulat("marital_status", p.MaritalStatus),
                Bulat("blood_type", p.BloodType),
                Teks("full_name", p.FullName),
                new NpgsqlParameter("birth_date", NpgsqlDbType.Date) { Value = (object?)p.BirthDate ?? DBNull.Value },
                Teks("identity_type", p.IdentityType),
                Teks("identity_number", p.IdentityNumber),
                Teks("phone_number", p.PhoneNumber),
                Teks("address", p.Address),
                Benar("is_member", p.IsMember),
                Benar("is_newborn", p.IsNewborn),
                Benar("is_deceased", p.IsDeceased),
                Benar("is_active", p.IsActive),
                Teks("validation_status", p.ValidationStatus, NpgsqlDbType.Varchar));

        private static NpgsqlParameter Uuid(string nama, Guid nilai) => new(nama, NpgsqlDbType.Uuid) { Value = nilai };

        private static NpgsqlParameter Teks(string nama, string? nilai, NpgsqlDbType tipe = NpgsqlDbType.Text) =>
            new(nama, tipe) { Value = (object?)nilai ?? DBNull.Value };

        private static NpgsqlParameter Bulat(string nama, int? nilai) =>
            new(nama, NpgsqlDbType.Integer) { Value = (object?)nilai ?? DBNull.Value };

        private static NpgsqlParameter Benar(string nama, bool? nilai) =>
            new(nama, NpgsqlDbType.Boolean) { Value = (object?)nilai ?? DBNull.Value };
    }
}
