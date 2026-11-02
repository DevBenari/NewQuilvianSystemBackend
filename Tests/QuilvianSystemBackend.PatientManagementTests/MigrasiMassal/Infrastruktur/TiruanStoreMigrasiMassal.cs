using System.Data.Common;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.PatientManagement.MigrasiMassal.Infrastruktur
{
    /// <summary>
    /// Kendali dan catatan bersama untuk <see cref="TiruanStoreMigrasiMassal"/>, hidup sepanjang harness
    /// sehingga beberapa panggilan service dapat dibandingkan.
    /// </summary>
    public sealed class KendaliStoreUji
    {
        /// <summary>Isi <c>information_schema.columns</c> tiruan. Bawaan: skema fisik yang disetujui.</summary>
        public List<RsmmcBulkSchemaColumn> SkemaKolom { get; } = SkemaFisikUji.Disetujui();

        /// <summary>Kunci kedua yang sedang dipegang sesi lain; percobaan atasnya mengembalikan <c>false</c>.</summary>
        public HashSet<int> KunciDipegangSesiLain { get; } = [];

        public List<(int Namespace, int Kunci)> KunciDiminta { get; } = [];

        /// <summary>Seluruh pembacaan, termasuk tanda tangan skema.</summary>
        public int JumlahPembacaan { get; set; }

        /// <summary>Pembacaan tabel migrasi bertipe — seluruh pembacaan selain tanda tangan skema.</summary>
        public int JumlahPembacaanBertipe { get; set; }

        public int JumlahInsertCrosswalk { get; set; }

        /// <summary>Dipanggil sesudah kunci per pasien didapat, dengan transaksi pasien yang aktif.</summary>
        public Func<int, DbTransaction?, Task>? SetelahKunci { get; set; }

        /// <summary>Dipanggil sebelum INSERT crosswalk; melempar galat untuk mensimulasikan kegagalan.</summary>
        public Action<RsmmcBulkCrosswalkInsert>? SebelumInsertCrosswalk { get; set; }
    }

    /// <summary>
    /// Tiruan <see cref="IRsmmcBulkMigrationStore"/> di atas skema SQLite <c>migration</c> yang di-ATTACH
    /// ke koneksi basis data uji. Seluruh perintah memakai koneksi dan transaksi
    /// <see cref="ApplicationDbContext"/> yang sama dengan service, sehingga INSERT crosswalk ikut
    /// di-<i>commit</i> atau dibatalkan bersama insert <c>MstPatient</c> — seperti PostgreSQL.
    /// </summary>
    /// <remarks>
    /// SQL tiruan hanya membaca baris mentah; hitungan dan penyaringan dikerjakan di C# mengikuti
    /// semantik SQL produksi. Bukti SQL bertipe PostgreSQL yang sesungguhnya ada pada uji provider-real.
    /// </remarks>
    public sealed class TiruanStoreMigrasiMassal(ApplicationDbContext konteks, KendaliStoreUji kendali) : IRsmmcBulkMigrationStore
    {
        private static readonly string[] StatusPilot =
        [
            RsmmcBulkApprovedBatchProfile.MigrationStatusPilotAuto,
            RsmmcBulkApprovedBatchProfile.MigrationStatusPilotManual
        ];

        public Task<IReadOnlyList<RsmmcBulkSchemaColumn>> GetSchemaColumnsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            kendali.JumlahPembacaan++;

            IReadOnlyList<RsmmcBulkSchemaColumn> hasil = kendali.SkemaKolom
                .Select(x => new RsmmcBulkSchemaColumn { TableName = x.TableName, ColumnName = x.ColumnName, DataType = x.DataType })
                .ToList();

            return Task.FromResult(hasil);
        }

        public async Task<long> CountBatchRowsAsync(Guid batchId, CancellationToken cancellationToken) =>
            (await BacaAsync("SELECT batch_id FROM migration.rsmmc_patient_batch", r => Guid.Parse(r.GetString(0)), cancellationToken))
            .LongCount(x => x == batchId);

        public async Task<long> CountFinalizedPlannerRowsAsync(Guid batchId, CancellationToken cancellationToken) =>
            (await PerencanaAsync(cancellationToken))
            .LongCount(x => x.BatchId == batchId && x.Baris.PlanStatus == RsmmcBulkApprovedBatchProfile.PlannerFinalizedStatus);

        public async Task<RsmmcBulkPreviewUniqueness> GetPreviewUniquenessAsync(Guid batchId, CancellationToken cancellationToken)
        {
            var preview = (await PreviewAsync(cancellationToken)).Where(x => x.BatchId == batchId).ToList();

            // count(DISTINCT x) PostgreSQL tidak menghitung NULL.
            return new RsmmcBulkPreviewUniqueness
            {
                Rows = preview.Count,
                DistinctLegacyPids = preview.Select(x => x.LegacyPid).Distinct().LongCount(),
                DistinctPatientCodes = preview.Where(x => x.ProposedPatientCode != null).Select(x => x.ProposedPatientCode).Distinct().LongCount(),
                DistinctMedicalRecordNumbers = preview.Where(x => x.ProposedMedicalRecordNumber != null).Select(x => x.ProposedMedicalRecordNumber).Distinct().LongCount()
            };
        }

        public async Task<IReadOnlyList<long>> GetDeferredLegacyPidsAsync(Guid batchId, int maxRows, CancellationToken cancellationToken)
        {
            var preview = (await PreviewAsync(cancellationToken)).Where(x => x.BatchId == batchId).Select(x => x.LegacyPid).ToHashSet();
            var pilot = (await CrosswalkAsync(cancellationToken))
                .Where(x => StatusPilot.Contains(x.MigrationStatus))
                .Select(x => x.LegacyPid)
                .ToHashSet();

            return (await PerencanaAsync(cancellationToken))
                .Where(x => x.BatchId == batchId && x.Baris.PlanStatus == RsmmcBulkApprovedBatchProfile.PlannerFinalizedStatus)
                .Select(x => x.Baris.LegacyPid)
                .Where(x => !preview.Contains(x) && !pilot.Contains(x))
                .OrderBy(x => x)
                .Take(maxRows)
                .ToList();
        }

        public async Task<long> CountWorkerMigratedCrosswalkAsync(Guid batchId, CancellationToken cancellationToken) =>
            (await CrosswalkAsync(cancellationToken))
            .LongCount(x => x.LastBatchId == batchId && x.MigrationStatus == RsmmcBulkApprovedBatchProfile.MigrationStatusBulkWorker);

        public Task<long> CountPreviewCodesUsedByUnmappedPatientsAsync(Guid batchId, CancellationToken cancellationToken) =>
            HitungPemakaianTanpaPemetaanAsync(batchId, x => x.PatientCode, x => x.ProposedPatientCode, cancellationToken);

        public Task<long> CountPreviewMedicalRecordNumbersUsedByUnmappedPatientsAsync(Guid batchId, CancellationToken cancellationToken) =>
            HitungPemakaianTanpaPemetaanAsync(batchId, x => x.MedicalRecordNumber, x => x.ProposedMedicalRecordNumber, cancellationToken);

        public async Task<long> CountDuplicateTargetGroupsAsync(CancellationToken cancellationToken) =>
            (await CrosswalkAsync(cancellationToken))
            .Where(x => x.QuilvianPatientId.HasValue)
            .GroupBy(x => x.QuilvianPatientId)
            .LongCount(g => g.Count() > 1);

        public async Task<RsmmcBulkPilotCrosswalkState> GetPilotCrosswalkStateAsync(CancellationToken cancellationToken)
        {
            var pilot = (await CrosswalkAsync(cancellationToken)).Where(x => StatusPilot.Contains(x.MigrationStatus)).ToList();

            return new RsmmcBulkPilotCrosswalkState
            {
                Targets = pilot.LongCount(x => x.QuilvianPatientId.HasValue),
                AutoMigrated = pilot.LongCount(x => x.MigrationStatus == RsmmcBulkApprovedBatchProfile.MigrationStatusPilotAuto),
                ManualMigrated = pilot.LongCount(x => x.MigrationStatus == RsmmcBulkApprovedBatchProfile.MigrationStatusPilotManual)
            };
        }

        public async Task<long> CountPreviewRowsAsync(Guid batchId, CancellationToken cancellationToken) =>
            (await PreviewAsync(cancellationToken)).LongCount(x => x.BatchId == batchId);

        public async Task<long> CountBackupRowsAsync(CancellationToken cancellationToken) =>
            (await BacaAsync("SELECT legacy_pid FROM migration.rsmmc_patient_pilot_reconcile_backup", r => r.GetInt64(0), cancellationToken))
            .LongCount();

        public async Task<IReadOnlyList<RsmmcBulkPreviewRow>> GetPreviewRowsAsync(
            Guid batchId,
            IReadOnlyCollection<long> legacyPids,
            CancellationToken cancellationToken) =>
            (await PreviewAsync(cancellationToken))
            .Where(x => x.BatchId == batchId && legacyPids.Contains(x.LegacyPid))
            .ToList();

        public async Task<IReadOnlyList<RsmmcBulkPlannerRow>> GetPlannerRowsAsync(
            Guid batchId,
            IReadOnlyCollection<long> legacyPids,
            CancellationToken cancellationToken) =>
            (await PerencanaAsync(cancellationToken))
            .Where(x => x.BatchId == batchId && legacyPids.Contains(x.Baris.LegacyPid))
            .Select(x => x.Baris)
            .ToList();

        public async Task<IReadOnlyList<RsmmcBulkCrosswalkRow>> GetCrosswalkRowsAsync(
            IReadOnlyCollection<long> legacyPids,
            CancellationToken cancellationToken) =>
            (await CrosswalkAsync(cancellationToken)).Where(x => legacyPids.Contains(x.LegacyPid)).ToList();

        public async Task<long> CountCrosswalkRowsForTargetAsync(Guid patientId, CancellationToken cancellationToken) =>
            (await CrosswalkAsync(cancellationToken)).LongCount(x => x.QuilvianPatientId == patientId);

        public async Task<bool> TryAcquirePatientLockAsync(int lockKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var transaksi = WajibTransaksi();

            kendali.KunciDiminta.Add((RsmmcBulkApprovedBatchProfile.WorkerLockNamespace, lockKey));

            if (kendali.KunciDipegangSesiLain.Contains(lockKey))
            {
                return false;
            }

            if (kendali.SetelahKunci is { } setelahKunci)
            {
                await setelahKunci(lockKey, transaksi);
            }

            return true;
        }

        public async Task InsertCrosswalkAsync(RsmmcBulkCrosswalkInsert row, CancellationToken cancellationToken)
        {
            var transaksi = WajibTransaksi();

            kendali.SebelumInsertCrosswalk?.Invoke(row);

            await using var perintah = konteks.Database.GetDbConnection().CreateCommand();
            perintah.Transaction = transaksi;
            perintah.CommandText =
                "INSERT INTO migration.rsmmc_patient_crosswalk " +
                "(legacy_pid, canonical_legacy_pid, legacy_old_rm, normalized_mrn, normalized_nik, quilvian_patient_id, " +
                "match_status, migration_status, review_reason, last_batch_id, created_at, updated_at) " +
                "VALUES ($legacy_pid, NULL, $legacy_old_rm, $normalized_mrn, $normalized_nik, $quilvian_patient_id, " +
                "$match_status, $migration_status, NULL, $last_batch_id, CURRENT_TIMESTAMP, NULL)";

            Parameter(perintah, "$legacy_pid", row.LegacyPid);
            Parameter(perintah, "$legacy_old_rm", row.LegacyOldRm);
            Parameter(perintah, "$normalized_mrn", row.NormalizedMrn);
            Parameter(perintah, "$normalized_nik", row.NormalizedNik);
            Parameter(perintah, "$quilvian_patient_id", row.QuilvianPatientId.ToString("D"));
            Parameter(perintah, "$match_status", RsmmcBulkApprovedBatchProfile.MatchStatusTargetCreated);
            Parameter(perintah, "$migration_status", RsmmcBulkApprovedBatchProfile.MigrationStatusBulkWorker);
            Parameter(perintah, "$last_batch_id", row.BatchId.ToString("D"));

            await perintah.ExecuteNonQueryAsync(cancellationToken);
            kendali.JumlahInsertCrosswalk++;
        }

        // Sama dengan EnsureTransaction store produksi: kunci xact dan insert wajib di transaksi pasien.
        private DbTransaction WajibTransaksi() =>
            konteks.Database.CurrentTransaction?.GetDbTransaction()
            ?? throw new InvalidOperationException("Operasi migrasi massal ini wajib berjalan di dalam transaksi pasien.");

        private async Task<long> HitungPemakaianTanpaPemetaanAsync(
            Guid batchId,
            Func<PasienRingkas, string> kolomPasien,
            Func<RsmmcBulkPreviewRow, string?> kolomPreview,
            CancellationToken cancellationToken)
        {
            var preview = (await PreviewAsync(cancellationToken)).Where(x => x.BatchId == batchId).ToList();
            var crosswalk = (await CrosswalkAsync(cancellationToken)).ToDictionary(x => x.LegacyPid);
            var pasien = await konteks.Set<MstPatient>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Select(x => new PasienRingkas(x.Id, x.PatientCode, x.MedicalRecordNumber))
                .ToListAsync(cancellationToken);

            // JOIN preview; LEFT JOIN crosswalk; WHERE target IS NULL OR target <> m.Id.
            return pasien
                .SelectMany(m => preview.Where(t => kolomPreview(t) == kolomPasien(m)).Select(t => (m, t)))
                .LongCount(x => !crosswalk.TryGetValue(x.t.LegacyPid, out var c) || c.QuilvianPatientId is null || c.QuilvianPatientId != x.m.Id);
        }

        private Task<List<RsmmcBulkPreviewRow>> PreviewAsync(CancellationToken cancellationToken) =>
            BacaAsync(
                "SELECT batch_id, legacy_pid, source_group, legacy_old_rm, legacy_normalized_mrn, legacy_normalized_nik, " +
                "proposed_patient_code, proposed_medical_record_number, patient_type, patient_status, registration_source, " +
                "gender, religion, marital_status, blood_type, full_name, birth_date, identity_type, identity_number, " +
                "phone_number, address, is_member, is_newborn, is_deceased, is_active, validation_status " +
                "FROM migration.rsmmc_patient_target_preview",
                r => new RsmmcBulkPreviewRow
                {
                    BatchId = Guid.Parse(r.GetString(0)),
                    LegacyPid = r.GetInt64(1),
                    SourceGroup = Teks(r, 2),
                    LegacyOldRm = Teks(r, 3),
                    LegacyNormalizedMrn = Teks(r, 4),
                    LegacyNormalizedNik = Teks(r, 5),
                    ProposedPatientCode = Teks(r, 6),
                    ProposedMedicalRecordNumber = Teks(r, 7),
                    PatientType = Bulat(r, 8),
                    PatientStatus = Bulat(r, 9),
                    RegistrationSource = Bulat(r, 10),
                    Gender = Bulat(r, 11),
                    Religion = Bulat(r, 12),
                    MaritalStatus = Bulat(r, 13),
                    BloodType = Bulat(r, 14),
                    FullName = Teks(r, 15),
                    BirthDate = Teks(r, 16) is { } tanggal
                        ? DateTime.ParseExact(tanggal, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                        : null,
                    IdentityType = Teks(r, 17),
                    IdentityNumber = Teks(r, 18),
                    PhoneNumber = Teks(r, 19),
                    Address = Teks(r, 20),
                    IsMember = Benar(r, 21),
                    IsNewborn = Benar(r, 22),
                    IsDeceased = Benar(r, 23),
                    IsActive = Benar(r, 24),
                    ValidationStatus = Teks(r, 25)
                },
                cancellationToken);

        private Task<List<(Guid BatchId, RsmmcBulkPlannerRow Baris)>> PerencanaAsync(CancellationToken cancellationToken) =>
            BacaAsync(
                "SELECT batch_id, legacy_pid, plan_status, final_medical_record_number FROM migration.rsmmc_patient_mrn_plan",
                r => (Guid.Parse(r.GetString(0)), new RsmmcBulkPlannerRow
                {
                    LegacyPid = r.GetInt64(1),
                    PlanStatus = Teks(r, 2),
                    FinalMedicalRecordNumber = Teks(r, 3)
                }),
                cancellationToken);

        private Task<List<RsmmcBulkCrosswalkRow>> CrosswalkAsync(CancellationToken cancellationToken) =>
            BacaAsync(
                "SELECT legacy_pid, quilvian_patient_id, match_status, migration_status, last_batch_id FROM migration.rsmmc_patient_crosswalk",
                r => new RsmmcBulkCrosswalkRow
                {
                    LegacyPid = r.GetInt64(0),
                    QuilvianPatientId = Teks(r, 1) is { } target ? Guid.Parse(target) : null,
                    MatchStatus = Teks(r, 2),
                    MigrationStatus = Teks(r, 3),
                    LastBatchId = Teks(r, 4) is { } batch ? Guid.Parse(batch) : null
                },
                cancellationToken);

        private async Task<List<T>> BacaAsync<T>(string sql, Func<DbDataReader, T> peta, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            kendali.JumlahPembacaan++;
            kendali.JumlahPembacaanBertipe++;

            await using var perintah = konteks.Database.GetDbConnection().CreateCommand();
            perintah.Transaction = konteks.Database.CurrentTransaction?.GetDbTransaction();
            perintah.CommandText = sql;

            await using var pembaca = await perintah.ExecuteReaderAsync(cancellationToken);
            var hasil = new List<T>();

            while (await pembaca.ReadAsync(cancellationToken))
            {
                hasil.Add(peta(pembaca));
            }

            return hasil;
        }

        private static void Parameter(DbCommand perintah, string nama, object? nilai)
        {
            var parameter = perintah.CreateParameter();
            parameter.ParameterName = nama;
            parameter.Value = nilai ?? DBNull.Value;
            perintah.Parameters.Add(parameter);
        }

        private static string? Teks(DbDataReader r, int i) => r.IsDBNull(i) ? null : r.GetString(i);

        private static int? Bulat(DbDataReader r, int i) => r.IsDBNull(i) ? null : r.GetInt32(i);

        private static bool? Benar(DbDataReader r, int i) => r.IsDBNull(i) ? null : r.GetInt64(i) != 0;

        private sealed record PasienRingkas(Guid Id, string PatientCode, string MedicalRecordNumber);
    }

    /// <summary>
    /// Skema fisik tabel migrasi yang disetujui — `PAT-FACT-079`, `082`, `083` — dalam bentuk
    /// <c>information_schema.columns</c> PostgreSQL. Ditulis terpisah dari service, supaya uji
    /// membandingkan tanda tangan service dengan fakta, bukan dengan dirinya sendiri.
    /// </summary>
    public static class SkemaFisikUji
    {
        public const string Preview = "rsmmc_patient_target_preview";
        public const string Perencana = "rsmmc_patient_mrn_plan";
        public const string Crosswalk = "rsmmc_patient_crosswalk";
        public const string Batch = "rsmmc_patient_batch";

        public static List<RsmmcBulkSchemaColumn> Disetujui()
        {
            var kolom = new List<RsmmcBulkSchemaColumn>();

            void Tambah(string tabel, string tipe, params string[] nama)
            {
                foreach (var n in nama)
                {
                    kolom.Add(new RsmmcBulkSchemaColumn { TableName = tabel, ColumnName = n, DataType = tipe });
                }
            }

            Tambah(Preview, "uuid", "batch_id");
            Tambah(Preview, "bigint", "legacy_pid");
            Tambah(Preview, "character varying", "source_group", "validation_status");
            Tambah(Preview, "text", "legacy_old_rm", "legacy_normalized_mrn", "legacy_normalized_nik",
                "proposed_patient_code", "proposed_medical_record_number", "identity_type", "identity_number",
                "phone_number", "address", "validation_reason", "full_name");
            Tambah(Preview, "integer", "patient_type", "patient_status", "registration_source", "gender",
                "religion", "marital_status", "blood_type");
            Tambah(Preview, "boolean", "is_member", "is_newborn", "is_deceased", "is_active");
            Tambah(Preview, "date", "birth_date");
            Tambah(Preview, "timestamp with time zone", "created_at");

            Tambah(Perencana, "uuid", "batch_id", "target_collision_patient_id");
            Tambah(Perencana, "bigint", "legacy_pid");
            Tambah(Perencana, "character varying", "final_medical_record_number", "plan_status");
            Tambah(Perencana, "boolean", "target_collision");
            Tambah(Perencana, "timestamp with time zone", "created_at", "updated_at");

            Tambah(Crosswalk, "bigint", "legacy_pid", "canonical_legacy_pid");
            Tambah(Crosswalk, "text", "legacy_old_rm", "normalized_mrn", "normalized_nik", "review_reason");
            Tambah(Crosswalk, "uuid", "quilvian_patient_id", "last_batch_id");
            Tambah(Crosswalk, "character varying", "match_status", "migration_status");
            Tambah(Crosswalk, "timestamp with time zone", "created_at", "updated_at");

            Tambah(Batch, "uuid", "batch_id");

            return kolom;
        }
    }
}
