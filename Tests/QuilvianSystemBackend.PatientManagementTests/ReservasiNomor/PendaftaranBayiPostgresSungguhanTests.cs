using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;

namespace QuilvianSystemBackend.Tests.PatientManagement.ReservasiNomor;

/// <summary>
/// Bukti provider-real guard reservasi nomor pada jalur pendaftaran bayi, PostgreSQL 15 sekali
/// pakai — `BE-PAT-MIG-003A`, `PAT-DEC-070`, `071`, kontrak `1.3.0` B15.
/// </summary>
/// <remarks>
/// <para>
/// Cara menjalankan sama dengan <see cref="AlokasiPostgresSungguhanTests"/>:
/// <c>QUILVIAN_PG_PROOF_CONNECTION</c>, <c>QUILVIAN_PG_PROOF_REQUIRED=1</c>, lalu
/// <c>dotnet test Tests/QuilvianSystemBackend.PatientManagementTests --filter Kategori=ProviderReal</c>.
/// </para>
/// <para>
/// Fixture: pada basis data sekali pakai, foreign key <c>OprCase</c> ke encounter dan dokter dilepas
/// supaya kasus persalinan uji tidak perlu menyeret master registrasi dan SDM. Jalur bayi tidak
/// membaca keduanya; foreign key ke ibu (<c>MstPatient</c>) tetap ada. Basis data lain di server
/// tidak disentuh.
/// </para>
/// </remarks>
[Trait("Kategori", "ProviderReal")]
public class PendaftaranBayiPostgresSungguhanTests
{
    private static readonly TimeSpan BatasTunggu = TimeSpan.FromSeconds(60);

    // Kunci (PATI, NORM) di basis data uji ini saja — basis data uji lain di server yang sama
    // dapat memegang kunci bernilai sama pada saat yang sama.
    private static readonly string KunciAllocatorDiBasisDataIni =
        "SELECT count(*)::int AS \"Value\" FROM pg_locks WHERE locktype = 'advisory' AND objsubid = 2 " +
        "AND database = (SELECT oid FROM pg_database WHERE datname = current_database()) " +
        $"AND classid::bigint = {RsmmcIdentifierReservationProfile.AllocationLockNamespace} " +
        $"AND objid::bigint = {RsmmcIdentifierReservationProfile.AllocationLockId}";

    private static readonly string KunciAllocatorMenunggu = KunciAllocatorDiBasisDataIni + " AND NOT granted";

    private const string KunciAdvisorySesiIni =
        "SELECT count(*)::int AS \"Value\" FROM pg_locks WHERE locktype = 'advisory' AND granted AND pid = pg_backend_pid()";

    // ---- T1, T2: di luar Staging persis — NumberSeries apa adanya, tanpa kunci allocator ---------

    [TheoryPostgresSungguhan]
    [InlineData("Production")]
    [InlineData("Development")]
    [InlineData("staging")]
    public async Task T1_T2_BukanStagingPersis_NumberSeriesSepertiSebelumnya(string environmentName)
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        var kasus = await SiapkanAsync(pg, ("PAT-RSMMC-00741", "00-00-07-41"), ("PAT-RSMMC-00003", "00-00-00-03"));

        // Sesi lain memegang kunci allocator selama uji. Jalur bayi di luar Staging persis tidak
        // memakainya, sehingga tidak menunggu.
        await using var pemegang = await PemegangKunci.PegangAsync(pg);

        var pertama = await DaftarBayiAsync(pg, kasus, environmentName, "true", "uji-bayi-lama-0001").WaitAsync(BatasTunggu);
        var ulang = await DaftarBayiAsync(pg, kasus, environmentName, "true", "uji-bayi-lama-0001").WaitAsync(BatasTunggu);
        var kedua = await DaftarBayiAsync(pg, kasus, environmentName, "true", "uji-bayi-lama-0002", "RS Uji Kembar").WaitAsync(BatasTunggu);

        // Perilaku sebelum task: deret belum ada → batas bawah = digit tertinggi yang ada (741);
        // deret sudah ada → lanjut dari pencacah tersimpan.
        Assert.Equal(NewbornRegistrationStatus.Success, pertama.Status);
        Assert.Equal("PAT-RSMMC-00742", pertama.Data!.PatientCode);
        Assert.Equal("00-00-07-42", pertama.Data.MedicalRecordNumber);
        Assert.True(ulang.Data!.WasExisting);
        Assert.Equal(pertama.Data.Id, ulang.Data.Id);
        Assert.Equal("PAT-RSMMC-00743", kedua.Data!.PatientCode);
        Assert.Equal("00-00-07-43", kedua.Data.MedicalRecordNumber);
        Assert.Equal(2, kedua.Data.BirthOrder);

        await using var baca = pg.BuatKonteks();
        Assert.Equal(743, await BayiUji.NilaiDeretAsync(baca, "PATIENT_CODE"));
        Assert.Equal(743, await BayiUji.NilaiDeretAsync(baca, "PATIENT_MEDICAL_RECORD"));
    }

    // ---- T3, T4, T5: Staging persis tanpa guard sah ------------------------------------------

    [TheoryPostgresSungguhan]
    [InlineData("false")]
    [InlineData(null)]
    [InlineData("yes")]
    public async Task T3_T4_T5_StagingTidakSah_GagalTertutupTanpaMutasiDanTanpaKunci(string? enabled)
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        var kasus = await SiapkanAsync(pg, ("PAT-RSMMC-00741", "00-00-07-41"));

        var hasil = await DaftarBayiAsync(pg, kasus, "Staging", enabled, "uji-bayi-tolak-0001");

        Assert.Equal(NewbornRegistrationStatus.ReservationGuardRejected, hasil.Status);
        Assert.Null(hasil.Data);

        await using var baca = pg.BuatKonteks();
        Assert.Equal(1, await baca.Set<MstPatient>().IgnoreQueryFilters().CountAsync());
        Assert.Equal(0, await BayiUji.JumlahDeretAsync(baca));
        Assert.Equal(0, await BayiUji.JumlahUrutanKelahiranAsync(baca));
        Assert.Equal(0, await baca.Database.SqlQueryRaw<int>(KunciAllocatorDiBasisDataIni).SingleAsync());
    }

    // ---- T6: Staging persis, Enabled = true --------------------------------------------------

    [FactPostgresSungguhan]
    public async Task T6_StagingAktif_NomorBayiDiAtasLantaiTanpaNumberSeries()
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        var kasus = await SiapkanAsync(
            pg,
            ("PAT-RSMMC-00741", "00-00-07-41"),
            ("PAT-RSMMC-705249", "00-89-01-87"),
            ("PAT-RSMMC-00742", "00-00-00-27"));

        var hasil = await DaftarBayiAsync(pg, kasus, "Staging", "true", "uji-bayi-lantai-0001");

        Assert.Equal(PostgresSekaliPakai.VersiMayorDiwajibkan, pg.VersiServer.Major);
        Assert.Equal(NewbornRegistrationStatus.Success, hasil.Status);
        Assert.Equal("PAT-RSMMC-705250", hasil.Data!.PatientCode);
        Assert.Equal("00-89-01-88", hasil.Data.MedicalRecordNumber);

        await using var baca = pg.BuatKonteks();
        Assert.Equal(0, await BayiUji.JumlahDeretAsync(baca));
        var bayi = await baca.Set<MstPatient>().AsNoTracking().SingleAsync(x => x.Id == hasil.Data.Id);
        Assert.False(bayi.IsMember);
        Assert.True(bayi.IsNewborn);
        Assert.Equal(1, bayi.BirthOrder);
    }

    // ---- T10: kunci yang disetujui, bentuk transaksi -----------------------------------------

    [FactPostgresSungguhan]
    public async Task T10_StagingAktif_BayiMenungguKunciPatiNormLaluMelepasnyaSaatCommit()
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        var kasus = await SiapkanAsync(pg, ("PAT-RSMMC-00741", "00-00-07-41"));

        var pemegang = await PemegangKunci.PegangAsync(pg);
        var tugas = DaftarBayiAsync(pg, kasus, "Staging", "true", "uji-bayi-kunci-0001");

        // Jalur bayi meminta tepat kunci (1346458697, 1313821261) dan menunggu pemegangnya.
        await TungguSampaiAsync(async () => await HitungAsync(pg, KunciAllocatorMenunggu) == 1);
        Assert.False(tugas.IsCompleted);

        await pemegang.DisposeAsync();
        var hasil = await tugas.WaitAsync(BatasTunggu);

        Assert.Equal(NewbornRegistrationStatus.Success, hasil.Status);
        Assert.Equal("PAT-RSMMC-705250", hasil.Data!.PatientCode);
        // Kunci transaksi lepas sendiri saat transaksi pendaftaran bayi selesai.
        Assert.Equal(0, await HitungAsync(pg, KunciAllocatorDiBasisDataIni));
    }

    // ---- T8: bayi ↔ bayi bersamaan -----------------------------------------------------------

    [FactPostgresSungguhan]
    public async Task T8_BayiBersamaan_KasusBerbeda_TidakAdaNomorGanda()
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        const int jumlah = 6;
        var jeda = TimeSpan.FromMilliseconds(150);
        var kasus = await SiapkanKasusAsync(pg, jumlah);

        var stopwatch = Stopwatch.StartNew();
        var hasil = await Task.WhenAll(kasus.Select((k, i) =>
            DaftarBayiAsync(pg, k, "Staging", "true", $"uji-bayi-bersama-{i:0000}", jeda: jeda)));
        stopwatch.Stop();

        Assert.All(hasil, x => Assert.Equal(NewbornRegistrationStatus.Success, x.Status));
        AssertNomorBerurutanDariLantai(hasil.Select(x => (x.Data!.PatientCode, x.Data.MedicalRecordNumber)), jumlah);
        Assert.True(stopwatch.Elapsed >= jeda * (jumlah - 1), $"Durasi {stopwatch.Elapsed} terlalu singkat untuk alokasi berurutan.");
    }

    // ---- T7, T9: normal ↔ bayi bersamaan -----------------------------------------------------

    [FactPostgresSungguhan]
    public async Task T7_T9_NormalDanBayiBersamaan_TidakAdaNomorGandaDanTidakDiBawahLantai()
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        const int jumlahPerJalur = 4;
        var jeda = TimeSpan.FromMilliseconds(150);
        var kasus = await SiapkanKasusAsync(pg, jumlahPerJalur);

        var stopwatch = Stopwatch.StartNew();
        var bayi = kasus.Select(async (k, i) =>
        {
            var hasil = await DaftarBayiAsync(pg, k, "Staging", "true", $"uji-bayi-campur-{i:0000}", jeda: jeda);
            Assert.Equal(NewbornRegistrationStatus.Success, hasil.Status);
            return (hasil.Data!.PatientCode, hasil.Data.MedicalRecordNumber);
        });
        var normal = Enumerable.Range(0, jumlahPerJalur).Select(_ => DaftarNormalAsync(pg, jeda));
        var tersimpan = await Task.WhenAll(bayi.Concat(normal));
        stopwatch.Stop();

        AssertNomorBerurutanDariLantai(tersimpan, jumlahPerJalur * 2);
        Assert.True(
            stopwatch.Elapsed >= jeda * (jumlahPerJalur * 2 - 1),
            $"Durasi {stopwatch.Elapsed} terlalu singkat: pendaftaran normal dan bayi tidak diserialkan kunci yang sama.");
    }

    // ---- T15, T16: idempotency dan urutan kelahiran tetap ------------------------------------

    [FactPostgresSungguhan]
    public async Task T15_StagingAktif_KunciIdempotencySamaBersamaan_SatuPasien()
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        var kasus = await SiapkanAsync(pg, ("PAT-RSMMC-00741", "00-00-07-41"));

        var hasil = await Task.WhenAll(
            DaftarBayiAsync(pg, kasus, "Staging", "true", "uji-bayi-ganda-0001", jeda: TimeSpan.FromMilliseconds(150)),
            DaftarBayiAsync(pg, kasus, "Staging", "true", "uji-bayi-ganda-0001", jeda: TimeSpan.FromMilliseconds(150)));
        var berbeda = await DaftarBayiAsync(pg, kasus, "Staging", "true", "uji-bayi-ganda-0001", "RS Lain");

        Assert.All(hasil, x => Assert.Equal(NewbornRegistrationStatus.Success, x.Status));
        Assert.Single(hasil, x => !x.Data!.WasExisting);
        Assert.Single(hasil, x => x.Data!.WasExisting);
        Assert.Equal(hasil[0].Data!.Id, hasil[1].Data!.Id);
        Assert.Equal(NewbornRegistrationStatus.Conflict, berbeda.Status);

        await using var baca = pg.BuatKonteks();
        Assert.Equal(2, await baca.Set<MstPatient>().IgnoreQueryFilters().CountAsync());
    }

    [FactPostgresSungguhan]
    public async Task T16_StagingAktif_KembarBersamaanSatuKasus_UrutanSatuDanDua()
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        var kasus = await SiapkanAsync(pg, ("PAT-RSMMC-00741", "00-00-07-41"));

        var hasil = await Task.WhenAll(
            DaftarBayiAsync(pg, kasus, "Staging", "true", "uji-bayi-kembar-0001", jeda: TimeSpan.FromMilliseconds(150)),
            DaftarBayiAsync(pg, kasus, "Staging", "true", "uji-bayi-kembar-0002", "RS Uji Kembar", TimeSpan.FromMilliseconds(150)));

        Assert.Equal([1, 2], hasil.Select(x => x.Data!.BirthOrder).Order());
        AssertNomorBerurutanDariLantai(hasil.Select(x => (x.Data!.PatientCode, x.Data.MedicalRecordNumber)), 2);

        await using var baca = pg.BuatKonteks();
        var urutan = await baca.PatNewbornBirthSequences.AsNoTracking().SingleAsync();
        Assert.Equal(2, urutan.LastAllocatedBirthOrder);
    }

    // ---- T11, T12, T14: pemeriksa ------------------------------------------------------------

    [FactPostgresSungguhan]
    public async Task T11_T12_T14_Pemeriksa_KeduaJalurTerlindungiBacaSaja()
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        await SiapkanAsync(pg, ("PAT-RSMMC-705250", "00-89-01-88"));

        await using var konteks = pg.BuatKonteks();
        var status = await Alokasi(konteks, "Staging", "true").CheckAsync();
        var kunci = await konteks.Database.SqlQueryRaw<int>(KunciAdvisorySesiIni).SingleAsync();

        Assert.True(status.NormalPatientCreateProtected);
        Assert.True(status.NewbornPatientCreateProtected);
        Assert.True(status.OverallGuardReady);
        Assert.Equal("PAT-RSMMC-705251", status.NextPatientCodeFormatted);
        Assert.Equal("00-89-01-89", status.NextMrnFormatted);
        Assert.Equal(0, kunci);

        await using var baca = pg.BuatKonteks();
        Assert.Equal(1, await baca.Set<MstPatient>().IgnoreQueryFilters().CountAsync());
        Assert.Equal(0, await BayiUji.JumlahDeretAsync(baca));
    }

    // ---- Pembantu -------------------------------------------------------------------------

    private static async Task<KasusPersalinanUji> SiapkanAsync(
        PostgresSekaliPakai pg,
        (string Code, string Mrn) ibu,
        params (string Code, string Mrn)[] pasienLain)
    {
        await LepasForeignKeyKasusSelainIbuAsync(pg);

        await using var konteks = pg.BuatKonteks();

        foreach (var (code, mrn) in pasienLain)
        {
            konteks.Set<MstPatient>().Add(new MstPatient
            {
                Id = Guid.NewGuid(),
                PatientCode = code,
                MedicalRecordNumber = mrn,
                FullName = "Pasien Uji Reservasi",
                CreateDateTime = BayiUji.WaktuDibuat
            });
        }

        await konteks.SaveChangesAsync();

        return await BayiUji.TambahIbuDanKasusAsync(konteks, ibu.Code, ibu.Mrn);
    }

    private static async Task<List<KasusPersalinanUji>> SiapkanKasusAsync(PostgresSekaliPakai pg, int jumlah)
    {
        var pertama = await SiapkanAsync(pg, ("PAT-RSMMC-00741", "00-00-07-41"));
        var daftar = new List<KasusPersalinanUji> { pertama };

        await using var konteks = pg.BuatKonteks();
        for (var i = 1; i < jumlah; i++)
        {
            daftar.Add(await BayiUji.TambahKasusAsync(konteks, pertama.IbuId));
        }

        return daftar;
    }

    private static async Task LepasForeignKeyKasusSelainIbuAsync(PostgresSekaliPakai pg)
    {
        await using var konteks = pg.BuatKonteks();
        var kasus = konteks.Model.FindEntityType(typeof(OprCase))!;

        foreach (var foreignKey in kasus.GetForeignKeys().Where(x => x.PrincipalEntityType.ClrType != typeof(MstPatient)))
        {
            var sql = "ALTER TABLE \"OprCase\" DROP CONSTRAINT IF EXISTS \"" + foreignKey.GetConstraintName() + "\";";
            await konteks.Database.ExecuteSqlRawAsync(sql);
        }
    }

    private static async Task<NewbornRegistrationResult> DaftarBayiAsync(
        PostgresSekaliPakai pg,
        KasusPersalinanUji kasus,
        string environmentName,
        string? enabled,
        string kunci,
        string tempatLahir = "RS Uji",
        TimeSpan? jeda = null)
    {
        await using var konteks = Konteks(pg, jeda);
        var layanan = BayiUji.BuatLayanan(konteks, Alokasi(konteks, environmentName, enabled), new PabrikKonteks(pg));

        return await layanan.RegisterAsync(BayiUji.Permintaan(kasus, tempatLahir), kunci, ReservasiHarness.AktorUji);
    }

    private static async Task<(string PatientCode, string MedicalRecordNumber)> DaftarNormalAsync(
        PostgresSekaliPakai pg,
        TimeSpan jeda)
    {
        await using var konteks = Konteks(pg, jeda);
        await using var transaksi = await konteks.Database.BeginTransactionAsync();

        var hasil = await Alokasi(konteks, "Staging", "true").AllocateAsync();

        konteks.Set<MstPatient>().Add(new MstPatient
        {
            Id = Guid.NewGuid(),
            PatientCode = hasil.PatientCode!,
            MedicalRecordNumber = hasil.MedicalRecordNumber!,
            FullName = "Pasien Normal Bersamaan Uji",
            CreateDateTime = DateTime.UtcNow
        });

        await konteks.SaveChangesAsync();
        await transaksi.CommitAsync();

        return (hasil.PatientCode!, hasil.MedicalRecordNumber!);
    }

    private static void AssertNomorBerurutanDariLantai(
        IEnumerable<(string PatientCode, string MedicalRecordNumber)> tersimpan,
        int jumlah)
    {
        var daftar = tersimpan.ToList();

        Assert.Equal(
            Enumerable.Range(0, jumlah).Select(i => RsmmcIdentifierReservationProfile.FormatPatientCode(705250 + i)),
            daftar.Select(x => x.PatientCode).OrderBy(x => x, StringComparer.Ordinal));
        Assert.Equal(
            Enumerable.Range(0, jumlah).Select(i => RsmmcIdentifierReservationProfile.FormatMedicalRecordNumber(890188 + i)),
            daftar.Select(x => x.MedicalRecordNumber).OrderBy(x => x, StringComparer.Ordinal));
    }

    private static ApplicationDbContext Konteks(PostgresSekaliPakai pg, TimeSpan? jeda)
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(pg.KoneksiDatabase);

        if (jeda is { } nilai) builder.AddInterceptors(new JedaSebelumInsertPasien(nilai));

        return new ApplicationDbContext(builder.Options);
    }

    private static PatientIdentifierAllocationService Alokasi(ApplicationDbContext konteks, string environmentName, string? enabled)
    {
        var nilai = new Dictionary<string, string?>();
        if (enabled is not null) nilai[$"{RsmmcMigrationReservationGuardOptions.SectionName}:Enabled"] = enabled;

        return new PatientIdentifierAllocationService(
            konteks,
            new LingkunganWebUji(environmentName, Path.GetTempPath()),
            new ConfigurationBuilder().AddInMemoryCollection(nilai).Build());
    }

    private static async Task<int> HitungAsync(PostgresSekaliPakai pg, string sql)
    {
        await using var konteks = pg.BuatKonteks();
        return await konteks.Database.SqlQueryRaw<int>(sql).SingleAsync();
    }

    private static async Task TungguSampaiAsync(Func<Task<bool>> kondisi)
    {
        var batas = Stopwatch.StartNew();

        while (!await kondisi())
        {
            if (batas.Elapsed > BatasTunggu)
            {
                throw new TimeoutException("Kondisi kunci yang ditunggu tidak tercapai.");
            }

            await Task.Delay(50);
        }
    }

    /// <summary>Konteks <see cref="NumberSeriesAllocator"/> pada basis data uji yang sama.</summary>
    private sealed class PabrikKonteks(PostgresSekaliPakai pg) : IDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext() => pg.BuatKonteks();
    }

    /// <summary>
    /// Memperlebar jendela balapan: insert pasien ditunda sesudah nomor dialokasikan. Tanpa kunci
    /// bersama, pendaftaran lain akan membaca nomor yang sama selama jeda ini.
    /// </summary>
    private sealed class JedaSebelumInsertPasien(TimeSpan jeda) : DbCommandInterceptor
    {
        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            if (InsertPasien(command)) await Task.Delay(jeda, cancellationToken);
            return result;
        }

        public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (InsertPasien(command)) await Task.Delay(jeda, cancellationToken);
            return result;
        }

        private static bool InsertPasien(DbCommand command) =>
            command.CommandText.Contains("INSERT INTO", StringComparison.Ordinal) &&
            command.CommandText.Contains("\"MstPatient\"", StringComparison.Ordinal);
    }

    /// <summary>Sesi PostgreSQL terpisah yang memegang kunci allocator sampai dibuang.</summary>
    private sealed class PemegangKunci : IAsyncDisposable
    {
        private readonly NpgsqlConnection _koneksi;
        private readonly NpgsqlTransaction _transaksi;
        private bool _dilepas;

        private PemegangKunci(NpgsqlConnection koneksi, NpgsqlTransaction transaksi)
        {
            _koneksi = koneksi;
            _transaksi = transaksi;
        }

        public static async Task<PemegangKunci> PegangAsync(PostgresSekaliPakai pg)
        {
            var koneksi = new NpgsqlConnection(pg.KoneksiDatabase);
            await koneksi.OpenAsync();
            var transaksi = await koneksi.BeginTransactionAsync();

            await using (var perintah = new NpgsqlCommand("SELECT pg_advisory_xact_lock(@a, @b);", koneksi, transaksi))
            {
                perintah.Parameters.AddWithValue("a", RsmmcIdentifierReservationProfile.AllocationLockNamespace);
                perintah.Parameters.AddWithValue("b", RsmmcIdentifierReservationProfile.AllocationLockId);
                await perintah.ExecuteNonQueryAsync();
            }

            return new PemegangKunci(koneksi, transaksi);
        }

        public async ValueTask DisposeAsync()
        {
            if (_dilepas) return;
            _dilepas = true;

            await _transaksi.RollbackAsync();
            await _transaksi.DisposeAsync();
            await _koneksi.DisposeAsync();
        }
    }
}
