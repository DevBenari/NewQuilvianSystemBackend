using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    /// <summary>
    /// Satu baris tabel perencana MRN beserta pemetaan crosswalk-nya.
    /// </summary>
    /// <remarks>
    /// Seluruh kolom dibaca sebagai teks. Tipe fisik kolom pada skema <c>migration</c> tidak
    /// tercatat di repository ini, sehingga kueri meng-cast ke <c>text</c> dan tipe akhirnya
    /// ditafsirkan di service. Kolom <c>normalized_mrn</c> sengaja tidak dibaca: ia bukti
    /// historis Pilot V1, bukan MRN tujuan (`PAT-DEC-005`).
    /// </remarks>
    public sealed class RsmmcPilotPlanRow
    {
        public string LegacyPid { get; set; } = string.Empty;

        public string? PlanStatus { get; set; }

        public string? FinalMedicalRecordNumber { get; set; }

        public string? QuilvianPatientId { get; set; }
    }

    /// <summary>
    /// Pembaca tabel migrasi RSMMC yang sudah ada di staging. Hanya membaca.
    /// </summary>
    public interface IRsmmcPilotMigrationSource
    {
        /// <summary>
        /// Baris perencana untuk batch ini yang berstatus <c>FINALIZED</c> dan sudah punya
        /// <c>quilvian_patient_id</c> di crosswalk — syarat 1 sampai 4 pada `AC-01`.
        /// </summary>
        Task<IReadOnlyList<RsmmcPilotPlanRow>> GetMappedFinalizedRowsAsync(
            Guid batchId,
            CancellationToken cancellationToken);

        /// <summary>
        /// Baris perencana dan crosswalk untuk satu <c>legacy_pid</c>, tanpa saringan status,
        /// supaya perubahan status atau pemetaan sesudah pemilihan dapat dikenali.
        /// </summary>
        Task<IReadOnlyList<RsmmcPilotPlanRow>> GetRowsForRevalidationAsync(
            Guid batchId,
            string legacyPid,
            CancellationToken cancellationToken);

        Task<int> CountFinalizedPlanRowsAsync(Guid batchId, CancellationToken cancellationToken);

        Task<int> CountBackupRowsAsync(Guid batchId, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Membaca tabel <c>migration.rsmmc_*</c> lewat kueri SQL berparameter.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Tabel-tabel ini infrastruktur migrasi yang sudah ada dan tidak dipetakan ke model EF.
    /// <c>Database.SqlQuery&lt;T&gt;</c> memetakan hasil ke tipe di luar model, sehingga model
    /// aplikasi tidak berubah dan tidak ada EF migration yang dibutuhkan (`PAT-OQ-007`).
    /// </para>
    /// <para>
    /// Kueri dijalankan pada koneksi dan transaksi <see cref="ApplicationDbContext"/> yang sama,
    /// sehingga pemeriksaan ulang per pasien membaca keadaan di dalam transaksi pasien itu.
    /// </para>
    /// </remarks>
    public sealed class RsmmcPilotMigrationSource : IRsmmcPilotMigrationSource
    {
        /// <summary>Teks kueri. Hanya SELECT; seluruh nilai masukan dikirim sebagai parameter.</summary>
        internal const string MappedFinalizedRowsSql =
            @"SELECT p.legacy_pid::text AS ""LegacyPid"",
       p.plan_status::text AS ""PlanStatus"",
       p.final_medical_record_number::text AS ""FinalMedicalRecordNumber"",
       c.quilvian_patient_id::text AS ""QuilvianPatientId""
FROM migration.rsmmc_patient_mrn_plan p
JOIN migration.rsmmc_patient_crosswalk c ON c.legacy_pid = p.legacy_pid
WHERE lower(p.batch_id::text) = {0}
  AND p.plan_status = 'FINALIZED'
  AND c.quilvian_patient_id IS NOT NULL
ORDER BY p.legacy_pid";

        internal const string RevalidationRowsSql =
            @"SELECT p.legacy_pid::text AS ""LegacyPid"",
       p.plan_status::text AS ""PlanStatus"",
       p.final_medical_record_number::text AS ""FinalMedicalRecordNumber"",
       c.quilvian_patient_id::text AS ""QuilvianPatientId""
FROM migration.rsmmc_patient_mrn_plan p
LEFT JOIN migration.rsmmc_patient_crosswalk c ON c.legacy_pid = p.legacy_pid
WHERE lower(p.batch_id::text) = {0}
  AND p.legacy_pid::text = {1}";

        internal const string CountFinalizedPlanRowsSql =
            @"SELECT count(*)::int AS ""Value""
FROM migration.rsmmc_patient_mrn_plan p
WHERE lower(p.batch_id::text) = {0}
  AND p.plan_status = 'FINALIZED'";

        internal const string CountBackupRowsSql =
            @"SELECT count(*)::int AS ""Value""
FROM migration.rsmmc_patient_pilot_reconcile_backup b
WHERE lower(b.batch_id::text) = {0}";

        private readonly ApplicationDbContext _dbContext;

        public RsmmcPilotMigrationSource(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<RsmmcPilotPlanRow>> GetMappedFinalizedRowsAsync(
            Guid batchId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Database
                .SqlQuery<RsmmcPilotPlanRow>(Query(MappedFinalizedRowsSql, BatchText(batchId)))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<RsmmcPilotPlanRow>> GetRowsForRevalidationAsync(
            Guid batchId,
            string legacyPid,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Database
                .SqlQuery<RsmmcPilotPlanRow>(Query(RevalidationRowsSql, BatchText(batchId), legacyPid))
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountFinalizedPlanRowsAsync(Guid batchId, CancellationToken cancellationToken)
        {
            return await _dbContext.Database
                .SqlQuery<int>(Query(CountFinalizedPlanRowsSql, BatchText(batchId)))
                .SingleAsync(cancellationToken);
        }

        public async Task<int> CountBackupRowsAsync(Guid batchId, CancellationToken cancellationToken)
        {
            return await _dbContext.Database
                .SqlQuery<int>(Query(CountBackupRowsSql, BatchText(batchId)))
                .SingleAsync(cancellationToken);
        }

        // Nilai masukan menjadi parameter DbParameter, tidak pernah disambung ke teks SQL.
        private static FormattableString Query(string sql, params object[] arguments) =>
            FormattableStringFactory.Create(sql, arguments);

        private static string BatchText(Guid batchId) => batchId.ToString("D");
    }
}
