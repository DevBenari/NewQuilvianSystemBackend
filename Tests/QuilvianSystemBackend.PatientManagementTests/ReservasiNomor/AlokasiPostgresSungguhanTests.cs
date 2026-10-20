using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;

namespace QuilvianSystemBackend.Tests.PatientManagement.ReservasiNomor;

/// <summary>
/// Bukti provider-real allocator nomor pasien normal pada PostgreSQL 15 sekali pakai —
/// `PAT-MIG3-AC-13`, `PAT-DEC-065`, kontrak `1.2.1` B15.
/// </summary>
/// <remarks>
/// Cara menjalankan, dengan server PostgreSQL 15 sekali pakai yang disediakan sendiri:
/// <c>QUILVIAN_PG_PROOF_CONNECTION="Host=…;Username=…;Password=…;Database=postgres"</c>,
/// <c>QUILVIAN_PG_PROOF_REQUIRED=1</c>, lalu
/// <c>dotnet test Tests/QuilvianSystemBackend.PatientManagementTests --filter Kategori=ProviderReal
/// -- xUnit.ParallelizeTestCollections=false xUnit.MaxParallelThreads=1</c> — serial, lihat
/// <see cref="PostgresSekaliPakai"/>.
/// Bukti persistensi <c>IsMember</c> (`PAT-MIG2-AC-47`) milik <c>BE-PAT-MIG-002</c>, bukan task ini.
/// </remarks>
[Trait("Kategori", "ProviderReal")]
public class AlokasiPostgresSungguhanTests
{
    // Kunci dua bagian tercatat di pg_locks sebagai classid = kunci pertama, objid = kunci kedua,
    // objsubid = 2. Penghitungan seluruh kunci advisory sesi membuktikan tidak ada kunci lain.
    private const string KunciAdvisoryTerpegang =
        "SELECT count(*)::int AS \"Value\" FROM pg_locks WHERE locktype = 'advisory' AND granted AND pid = pg_backend_pid()";

    private static readonly string KunciAllocatorTerpegang =
        "SELECT count(*)::int AS \"Value\" FROM pg_locks WHERE locktype = 'advisory' AND granted " +
        "AND pid = pg_backend_pid() AND objsubid = 2 " +
        $"AND classid::bigint = {RsmmcIdentifierReservationProfile.AllocationLockNamespace} " +
        $"AND objid::bigint = {RsmmcIdentifierReservationProfile.AllocationLockId}";

    [FactPostgresSungguhan]
    public async Task A_B_StagingAktif_LantaiDanKunciDatabase()
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        await TambahPasienAsync(pg, ("PAT-RSMMC-00742", "00-00-00-27"), ("PAT-RSMMC-705249", "00-89-01-87"));

        await using var konteks = pg.BuatKonteks();
        await using var transaksi = await konteks.Database.BeginTransactionAsync();

        var hasil = await Service(konteks, "Staging", "true").AllocateAsync();
        var kunciSaatAlokasi = await konteks.Database.SqlQueryRaw<int>(KunciAdvisoryTerpegang).SingleAsync();
        var kunciAllocatorSaatAlokasi = await konteks.Database.SqlQueryRaw<int>(KunciAllocatorTerpegang).SingleAsync();

        await transaksi.RollbackAsync();
        var kunciSesudahRollback = await konteks.Database.SqlQueryRaw<int>(KunciAdvisoryTerpegang).SingleAsync();

        Assert.Equal(PostgresSekaliPakai.VersiMayorDiwajibkan, pg.VersiServer.Major);
        Assert.Equal("PAT-RSMMC-705250", hasil.PatientCode);
        Assert.Equal("00-89-01-88", hasil.MedicalRecordNumber);
        Assert.Equal(1, kunciSaatAlokasi);
        Assert.Equal(1, kunciAllocatorSaatAlokasi);
        Assert.Equal(0, kunciSesudahRollback);
    }

    [FactPostgresSungguhan]
    public async Task E_StagingAktif_CelahDiAtasLantai()
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        await TambahPasienAsync(
            pg,
            ("PAT-RSMMC-705250", "00-89-01-88"),
            ("PAT-RSMMC-705252", "00-89-01-90"),
            ("PAT-RSMMC-0705251", "00890189"));

        var hasil = await AlokasikanAsync(pg, "Staging", "true");

        Assert.Equal("PAT-RSMMC-705253", hasil.PatientCode);
        Assert.Equal("00-89-01-91", hasil.MedicalRecordNumber);
    }

    [FactPostgresSungguhan]
    public async Task C_D_Bersamaan_TidakAdaKodeAtauMrnGanda()
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        const int jumlah = 8;
        var jeda = TimeSpan.FromMilliseconds(150);

        var stopwatch = Stopwatch.StartNew();
        var tersimpan = await Task.WhenAll(Enumerable.Range(0, jumlah).Select(async _ =>
        {
            await using var konteks = pg.BuatKonteks();
            await using var transaksi = await konteks.Database.BeginTransactionAsync();

            var hasil = await Service(konteks, "Staging", "true").AllocateAsync();

            konteks.Set<MstPatient>().Add(new MstPatient
            {
                Id = Guid.NewGuid(),
                PatientCode = hasil.PatientCode!,
                MedicalRecordNumber = hasil.MedicalRecordNumber!,
                FullName = "Pasien Bersamaan Uji",
                CreateDateTime = DateTime.UtcNow
            });

            // Memperlebar jendela balapan: tanpa kunci, pendaftaran lain akan membaca nomor yang sama.
            await Task.Delay(jeda);
            await konteks.SaveChangesAsync();
            await transaksi.CommitAsync();

            return (hasil.PatientCode!, hasil.MedicalRecordNumber!);
        }));
        stopwatch.Stop();

        Assert.Equal(
            Enumerable.Range(0, jumlah).Select(i => RsmmcIdentifierReservationProfile.FormatPatientCode(705250 + i)),
            tersimpan.Select(x => x.Item1).OrderBy(x => x, StringComparer.Ordinal));
        Assert.Equal(
            Enumerable.Range(0, jumlah).Select(i => RsmmcIdentifierReservationProfile.FormatMedicalRecordNumber(890188 + i)),
            tersimpan.Select(x => x.Item2).OrderBy(x => x, StringComparer.Ordinal));

        // Kunci diserialkan: delapan transaksi masing-masing memegang kunci selama jeda.
        Assert.True(stopwatch.Elapsed >= jeda * (jumlah - 1), $"Durasi {stopwatch.Elapsed} terlalu singkat untuk alokasi berurutan.");
    }

    [TheoryPostgresSungguhan]
    [InlineData("false")]
    [InlineData(null)]
    [InlineData("yes")]
    public async Task F_G_H_StagingTidakSah_GagalTertutupTanpaKunci(string? enabled)
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        await TambahPasienAsync(pg, ("PAT-RSMMC-00001", "00-00-00-01"));

        await using var konteks = pg.BuatKonteks();
        await using var transaksi = await konteks.Database.BeginTransactionAsync();

        var hasil = await Service(konteks, "Staging", enabled).AllocateAsync();
        var kunci = await konteks.Database.SqlQueryRaw<int>(KunciAdvisoryTerpegang).SingleAsync();
        await transaksi.RollbackAsync();

        Assert.False(hasil.Succeeded);
        Assert.Equal("PATIENT_RESERVATION_GUARD_CONFIG_INVALID", hasil.ErrorCode);
        Assert.Equal(0, kunci);
        Assert.Equal(1, await JumlahPasienAsync(pg));
    }

    [TheoryPostgresSungguhan]
    [InlineData("Production")]
    [InlineData("Development")]
    public async Task I_BukanStaging_SamaDenganPembangkitLamaDanTanpaKunci(string environmentName)
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        var data = new[] { ("PAT-RSMMC-00001", "00-00-00-01"), ("PAT-RSMMC-00003", "00-00-00-03") };
        await TambahPasienAsync(pg, data);

        await using var konteks = pg.BuatKonteks();
        await using var transaksi = await konteks.Database.BeginTransactionAsync();

        var hasil = await Service(konteks, environmentName, "true").AllocateAsync();
        var kunci = await konteks.Database.SqlQueryRaw<int>(KunciAdvisoryTerpegang).SingleAsync();
        await transaksi.RollbackAsync();

        var acuan = PembangkitLamaAcuan.Hitung(data);
        Assert.Equal(acuan.PatientCode, hasil.PatientCode);
        Assert.Equal(acuan.MedicalRecordNumber, hasil.MedicalRecordNumber);
        Assert.Equal(0, kunci);
    }

    [FactPostgresSungguhan]
    public async Task IndeksUnikTetapJaringTerakhir()
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        await TambahPasienAsync(pg, ("PAT-RSMMC-705250", "00-89-01-88"));

        var kodeGanda = await Assert.ThrowsAsync<DbUpdateException>(() =>
            TambahPasienAsync(pg, ("PAT-RSMMC-705250", "00-89-01-99")));
        var mrnGanda = await Assert.ThrowsAsync<DbUpdateException>(() =>
            TambahPasienAsync(pg, ("PAT-RSMMC-705299", "00-89-01-88")));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, Assert.IsType<PostgresException>(kodeGanda.InnerException).SqlState);
        Assert.Equal(PostgresErrorCodes.UniqueViolation, Assert.IsType<PostgresException>(mrnGanda.InnerException).SqlState);
    }

    [FactPostgresSungguhan]
    public async Task Pemeriksa_BacaSajaPadaPostgres()
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        await TambahPasienAsync(pg, ("PAT-RSMMC-705250", "00-89-01-88"));

        await using var konteks = pg.BuatKonteks();
        var status = await Service(konteks, "Staging", "true").CheckAsync();
        var kunci = await konteks.Database.SqlQueryRaw<int>(KunciAdvisoryTerpegang).SingleAsync();

        Assert.True(status.OverallGuardReady);
        Assert.Equal("PAT-RSMMC-705251", status.NextPatientCodeFormatted);
        Assert.Equal("00-89-01-89", status.NextMrnFormatted);
        Assert.Equal(0, kunci);
        Assert.Equal(1, await JumlahPasienAsync(pg));
    }

    private static PatientIdentifierAllocationService Service(ApplicationDbContext konteks, string environmentName, string? enabled)
    {
        var nilai = new Dictionary<string, string?>();
        if (enabled is not null) nilai[$"{RsmmcMigrationReservationGuardOptions.SectionName}:Enabled"] = enabled;

        return new PatientIdentifierAllocationService(
            konteks,
            new LingkunganWebUji(environmentName, Path.GetTempPath()),
            new ConfigurationBuilder().AddInMemoryCollection(nilai).Build());
    }

    private static async Task<PatientIdentifierAllocationResult> AlokasikanAsync(
        PostgresSekaliPakai pg, string environmentName, string? enabled)
    {
        await using var konteks = pg.BuatKonteks();
        await using var transaksi = await konteks.Database.BeginTransactionAsync();
        var hasil = await Service(konteks, environmentName, enabled).AllocateAsync();
        await transaksi.RollbackAsync();
        return hasil;
    }

    private static async Task TambahPasienAsync(PostgresSekaliPakai pg, params (string Code, string Mrn)[] pasien)
    {
        await using var konteks = pg.BuatKonteks();

        foreach (var (code, mrn) in pasien)
        {
            konteks.Set<MstPatient>().Add(new MstPatient
            {
                Id = Guid.NewGuid(),
                PatientCode = code,
                MedicalRecordNumber = mrn,
                FullName = "Pasien Uji Reservasi",
                CreateDateTime = DateTime.UtcNow
            });
        }

        await konteks.SaveChangesAsync();
    }

    private static async Task<int> JumlahPasienAsync(PostgresSekaliPakai pg)
    {
        await using var konteks = pg.BuatKonteks();
        return await konteks.Set<MstPatient>().IgnoreQueryFilters().CountAsync();
    }
}
