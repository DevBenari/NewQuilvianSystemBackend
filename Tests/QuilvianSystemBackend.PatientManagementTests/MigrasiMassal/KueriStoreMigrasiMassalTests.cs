using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.PatientManagement.MigrasiMassal;

/// <summary>
/// SQL store PostgreSQL worker: bertipe, tanpa <c>::text</c>, satu-satunya penulisan INSERT crosswalk, dan
/// kunci dua bagian tanpa menunggu — `PAT-MIG2-AC-10`, `29`, `32`, `34`, `42`, `51`; `PAT-DEC-047`,
/// `048`, `063`. Bukti eksekusi pada PostgreSQL 15 ada pada uji provider-real.
/// </summary>
public class KueriStoreMigrasiMassalTests
{
    private static readonly Regex KataTulisLain = new(
        @"\b(UPDATE|DELETE|TRUNCATE|DROP|ALTER|CREATE|MERGE|GRANT|REVOKE|COPY)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static IEnumerable<object[]> SeluruhSql() =>
        typeof(RsmmcBulkMigrationStore)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Static)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string) && f.Name.EndsWith("Sql", StringComparison.Ordinal))
            .Select(f => new object[] { f.Name });

    [Theory]
    [MemberData(nameof(SeluruhSql))]
    public void SetiapSql_TanpaCastTeks_TanpaPerintahUbahSelainInsertCrosswalk(string nama)
    {
        var sql = Sql(nama);

        Assert.DoesNotContain("::", sql);
        Assert.DoesNotContain("lower(", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch(KataTulisLain, sql);

        if (nama != "InsertCrosswalkSql")
        {
            Assert.DoesNotContain("INSERT", sql, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void SeluruhSqlDitemukan()
    {
        Assert.Equal(18, SeluruhSql().Count());
    }

    [Fact]
    public void InsertCrosswalk_KolomDanNilaiPersisKeputusan047()
    {
        var sql = Normal(Sql("InsertCrosswalkSql"));
        var cocok = Regex.Match(sql, @"^INSERT INTO migration\.rsmmc_patient_crosswalk \((?<kolom>[^)]*)\) VALUES \((?<nilai>[^)]*)\)$");

        Assert.True(cocok.Success, sql);
        Assert.Equal(
            ["legacy_pid", "canonical_legacy_pid", "legacy_old_rm", "normalized_mrn", "normalized_nik", "quilvian_patient_id",
             "match_status", "migration_status", "review_reason", "last_batch_id", "created_at", "updated_at"],
            Pisah(cocok.Groups["kolom"].Value));
        Assert.Equal(
            ["@legacy_pid", "NULL", "@legacy_old_rm", "@normalized_mrn", "@normalized_nik", "@quilvian_patient_id",
             "@match_status", "@migration_status", "NULL", "@last_batch_id", "CURRENT_TIMESTAMP", "NULL"],
            Pisah(cocok.Groups["nilai"].Value));
    }

    [Fact]
    public void Kunci_DuaBagianTryLock_TanpaMenungguDanTanpaHashtext()
    {
        var sql = Normal(Sql("TryAcquirePatientLockSql"));

        Assert.Equal("SELECT pg_try_advisory_xact_lock(@lock_namespace, @lock_key) AS \"Value\"", sql);
        Assert.DoesNotContain("hashtext", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PembacaanPasien_MemakaiParameterBertipeBatchDanLegacyPid()
    {
        Assert.Contains("t.batch_id = @batch_id", Sql("PreviewRowsSql"));
        Assert.Contains("t.legacy_pid = ANY(@legacy_pids)", Sql("PreviewRowsSql"));
        Assert.Contains("p.batch_id = @batch_id", Sql("PlannerRowsSql"));
        Assert.Contains("p.legacy_pid = ANY(@legacy_pids)", Sql("PlannerRowsSql"));
        Assert.Contains("c.legacy_pid = ANY(@legacy_pids)", Sql("CrosswalkRowsSql"));
        Assert.Contains("c.quilvian_patient_id = @patient_id", Sql("CountCrosswalkRowsForTargetSql"));
    }

    [Fact]
    public void PatientBaseline_MenghitungMigrationStatusWorkerPadaBatchKanonik_BukanMatchStatus()
    {
        var sql = Sql("CountWorkerMigratedCrosswalkSql");

        Assert.Contains("c.last_batch_id = @batch_id", sql);
        Assert.Contains("c.migration_status = @migration_status", sql);
        Assert.DoesNotContain("match_status", sql);
    }

    [Fact]
    public void TandaTanganSkema_LiteralTanpaParameter_DanCadanganDihitungSeluruhTabel()
    {
        var skema = Sql("SchemaColumnsSql");

        Assert.Contains("FROM information_schema.columns", skema);
        Assert.Contains("c.table_schema = 'migration'", skema);
        Assert.DoesNotContain("@", skema);
        Assert.DoesNotContain("WHERE", Sql("CountBackupRowsSql"));
    }

    [Fact]
    public void ParameterNpgsql_BertipeSesuaiSkemaFisik()
    {
        var id = Guid.NewGuid();

        Assert.Equal((NpgsqlDbType.Uuid, (object)id), Parameter("Uuid", "batch_id", id));
        Assert.Equal((NpgsqlDbType.Bigint, (object)-667992L), Parameter("Bigint", "legacy_pid", -667992L));
        Assert.Equal((NpgsqlDbType.Integer, (object)1173286842), Parameter("Integer", "lock_namespace", 1173286842));
        Assert.Equal((NpgsqlDbType.Text, (object)DBNull.Value), Parameter("Text", "legacy_old_rm", null));

        var larik = Parameter("BigintArray", "legacy_pids", (IReadOnlyCollection<long>)new long[] { -667992, 0, 797554 });
        Assert.Equal(NpgsqlDbType.Array | NpgsqlDbType.Bigint, larik.Tipe);
        Assert.Equal(new long[] { -667992, 0, 797554 }, (long[])larik.Nilai);

        var pilot = (NpgsqlParameter)typeof(RsmmcBulkMigrationStore)
            .GetMethod("PilotStatuses", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, null)!;
        Assert.Equal(NpgsqlDbType.Array | NpgsqlDbType.Text, pilot.NpgsqlDbType);
        Assert.Equal(new[] { "MIGRATED_API_AUTO", "MIGRATED_API_PILOT" }, (string[])pilot.Value!);
    }

    [Fact]
    public async Task KunciDanInsert_WajibDiDalamTransaksi_DitolakSebelumMenyentuhDatabase()
    {
        // Host .invalid tidak pernah dapat dihubungi: bila store mencoba membuka koneksi, galatnya bukan
        // InvalidOperationException dengan pesan transaksi.
        await using var konteks = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=pat-mig2-uji.invalid;Database=tidak_ada;Username=tidak_ada;Timeout=1")
            .Options);
        var store = new RsmmcBulkMigrationStore(konteks);

        var kunci = await Assert.ThrowsAsync<InvalidOperationException>(() => store.TryAcquirePatientLockAsync(1, CancellationToken.None));
        var insert = await Assert.ThrowsAsync<InvalidOperationException>(() => store.InsertCrosswalkAsync(
            new RsmmcBulkCrosswalkInsert(1, null, null, null, Guid.NewGuid(), RsmmcBulkApprovedBatchProfile.Canonical.BatchId),
            CancellationToken.None));

        Assert.Contains("transaksi pasien", kunci.Message);
        Assert.Contains("transaksi pasien", insert.Message);
    }

    [Fact]
    public void AntarmukaStore_HanyaSatuOperasiTulis()
    {
        var tulis = typeof(IRsmmcBulkMigrationStore)
            .GetMethods()
            .Where(m => Regex.IsMatch(m.Name, "^(Insert|Update|Delete|Upsert|Set|Write|Remove|Save)"))
            .Select(m => m.Name)
            .ToList();

        Assert.Equal(["InsertCrosswalkAsync"], tulis);
    }

    private static string Sql(string nama) =>
        (string)typeof(RsmmcBulkMigrationStore).GetField(nama, BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;

    private static string Normal(string sql) => Regex.Replace(sql, @"\s+", " ").Trim();

    private static List<string> Pisah(string daftar) => daftar.Split(',').Select(x => x.Trim()).ToList();

    private static (NpgsqlDbType Tipe, object Nilai) Parameter(string metode, string nama, object? nilai)
    {
        var parameter = (NpgsqlParameter)typeof(RsmmcBulkMigrationStore)
            .GetMethod(metode, BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, [nama, nilai])!;

        Assert.Equal(nama, parameter.ParameterName);
        return (parameter.NpgsqlDbType, parameter.Value!);
    }
}
