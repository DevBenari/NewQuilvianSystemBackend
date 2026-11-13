using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Services;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;

namespace QuilvianSystemBackend.Tests.PatientManagement;

/// <summary>
/// crm-membership `AT-12` (`INV-03`, `BE-CRM-011`): dua pendaftaran membership bersamaan untuk pasien
/// yang sama di PostgreSQL 15 sungguhan — satu sukses, sisanya 409, tepat satu baris Menunggu
/// Pembayaran, dan nomor member hanya terpakai satu.
/// </summary>
/// <remarks>
/// Memakai basis data sekali pakai <see cref="PostgresSekaliPakai"/>; cara menjalankan sama dengan
/// <c>PendaftaranBayiPostgresSungguhanTests</c> (<c>--filter Kategori=ProviderReal</c>, serial).
/// SQLite tidak dapat membuktikan ini karena <c>AcquirePatientLockAsync</c> hanya berjalan di Npgsql.
/// </remarks>
[Trait("Kategori", "ProviderReal")]
public class MembershipRegisterPostgresSungguhanTests
{
    private static readonly TimeSpan BatasTunggu = TimeSpan.FromSeconds(60);

    private static readonly Guid Aktor = Guid.Parse("33333333-3333-3333-3333-333333333333");

    // 13 Okt 2026 09.00 WIB.
    private static readonly DateTimeOffset Sekarang = new(2026, 10, 13, 9, 0, 0, TimeSpan.FromHours(7));

    private const string DeretNomorMember = "PAT_MEMBERSHIP_NUMBER";

    private const string KunciAdvisoryMenunggu =
        "SELECT count(*)::int AS \"Value\" FROM pg_locks WHERE locktype = 'advisory' AND NOT granted " +
        "AND database = (SELECT oid FROM pg_database WHERE datname = current_database())";

    /// <summary>
    /// Jalur utama `INV-03`: kedua permintaan dibuat antre di kunci per pasien, lalu dilepas bersamaan.
    /// Yang kalah harus ditolak oleh pemeriksaan Menunggu (VAL-05) — bukan oleh index unik — sehingga
    /// nomor member tidak ikut hangus.
    /// </summary>
    [FactPostgresSungguhan]
    public async Task AT12_DuaDaftarBersamaan_SatuSukses_SatuDitolak409_TepatSatuMenunggu()
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        var (pasien, gold) = await SiapkanAsync(pg);
        var port = new PortTagihanAmanParalel(new TiruanPortTagihanIuran(() => Sekarang));

        Task<PatientMembershipCommandResult> a, b;

        await using (var pemegang = await PegangKunciPasienAsync(pg, pasien))
        {
            a = DaftarAsync(pg, port, pasien, gold);
            b = DaftarAsync(pg, port, pasien, gold);

            // Bukti keduanya benar-benar bersamaan: dua sesi menunggu kunci advisory yang sama.
            await TungguMenungguKunciAsync(pg, 2);
        }

        var hasil = await Task.WhenAll(a, b).WaitAsync(BatasTunggu);

        var sukses = Assert.Single(hasil, x => x.StatusCode == StatusCodes.Status200OK);
        var ditolak = Assert.Single(hasil, x => x.StatusCode == StatusCodes.Status409Conflict);
        Assert.Equal("Pasien ini sudah punya pendaftaran yang menunggu pembayaran (MBR-000001).", ditolak.Message);
        Assert.Null(ditolak.PatientMembershipId);

        await using var baca = pg.BuatKonteks();
        var menunggu = await baca.Set<MstPatientMembership>().AsNoTracking()
            .Where(x => x.PatientId == pasien && x.MembershipStatus == MembershipStatus.PendingPayment)
            .ToListAsync();
        var satu = Assert.Single(menunggu);
        Assert.Equal(sukses.PatientMembershipId, satu.Id);
        Assert.Equal("MBR-000001", satu.MemberNumber);

        Assert.Equal(1, await baca.Set<MstPatientMembership>().CountAsync(x => x.PatientId == pasien));
        Assert.Equal(2, await baca.Set<PatMembershipStatusHistory>().CountAsync(x => x.PatientMembershipId == satu.Id));
        Assert.Equal(2, await baca.Set<PatMembershipStatusHistory>().CountAsync());

        // Yang kalah berhenti sebelum alokasi nomor: deret tetap 1.
        Assert.Equal(1, await NilaiDeretAsync(baca));
        Assert.Single(port.PermintaanTagihan);
    }

    /// <summary>
    /// Beban: delapan permintaan dilepas tanpa penahan. Tetap tepat satu sukses dan satu nomor terpakai.
    /// </summary>
    [FactPostgresSungguhan]
    public async Task AT12_DelapanDaftarParalel_TepatSatuSukses_NomorTidakHangus()
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        var (pasien, gold) = await SiapkanAsync(pg);
        var port = new PortTagihanAmanParalel(new TiruanPortTagihanIuran(() => Sekarang));

        using var mulai = new ManualResetEventSlim(false);
        var tugas = Enumerable.Range(0, 8)
            .Select(_ => Task.Run(async () =>
            {
                mulai.Wait();
                return await DaftarAsync(pg, port, pasien, gold);
            }))
            .ToArray();
        mulai.Set();

        var hasil = await Task.WhenAll(tugas).WaitAsync(BatasTunggu);

        Assert.Single(hasil, x => x.StatusCode == StatusCodes.Status200OK);
        Assert.Equal(7, hasil.Count(x => x.StatusCode == StatusCodes.Status409Conflict));

        await using var baca = pg.BuatKonteks();
        Assert.Equal(1, await baca.Set<MstPatientMembership>()
            .CountAsync(x => x.PatientId == pasien && x.MembershipStatus == MembershipStatus.PendingPayment));
        Assert.Equal(1, await baca.Set<MstPatientMembership>().CountAsync());
        Assert.Equal(1, await NilaiDeretAsync(baca));
    }

    /// <summary>
    /// Kontrol: kunci per pasien tidak menyerialkan pasien yang berbeda — dua pasien yang ditahan
    /// bersamaan tetap sama-sama sukses dengan nomor berbeda.
    /// </summary>
    [FactPostgresSungguhan]
    public async Task AT12_Kontrol_DuaPasienBerbedaBersamaan_KeduanyaSukses()
    {
        await using var pg = await PostgresSekaliPakai.BuatAsync();
        var (pasienA, gold) = await SiapkanAsync(pg);
        var pasienB = await TambahPasienAsync(pg);
        var port = new PortTagihanAmanParalel(new TiruanPortTagihanIuran(() => Sekarang));

        var hasil = await Task.WhenAll(
            DaftarAsync(pg, port, pasienA, gold),
            DaftarAsync(pg, port, pasienB, gold)).WaitAsync(BatasTunggu);

        Assert.All(hasil, x => Assert.Equal(StatusCodes.Status200OK, x.StatusCode));

        await using var baca = pg.BuatKonteks();
        var nomor = await baca.Set<MstPatientMembership>().AsNoTracking()
            .OrderBy(x => x.MemberNumber).Select(x => x.MemberNumber).ToListAsync();
        Assert.Equal(["MBR-000001", "MBR-000002"], nomor);
    }

    // ---------- bantuan ----------

    private static async Task<PatientMembershipCommandResult> DaftarAsync(
        PostgresSekaliPakai pg, IMembershipFeeChargePort port, Guid pasien, Guid tier)
    {
        await using var konteks = pg.BuatKonteks();
        var jam = new JamTetap(Sekarang);
        var konfigurasi = new ConfigurationBuilder().Build();
        var service = new PatientMembershipService(
            konteks,
            new NumberSeriesAllocator(new PabrikKonteks(pg)),
            new PatientMembershipStatusResolver(konteks, konfigurasi, jam),
            new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor()),
            port,
            konfigurasi,
            jam);

        return await service.RegisterAsync(
            new RegisterPatientMembershipRequest { PatientId = pasien, MembershipTierId = tier }, Aktor);
    }

    private static async Task<(Guid Pasien, Guid Gold)> SiapkanAsync(PostgresSekaliPakai pg)
    {
        var gold = Guid.NewGuid();
        await using (var k = pg.BuatKonteks())
        {
            k.Set<MstMembershipTier>().Add(new MstMembershipTier
            {
                Id = gold, TierCode = "GOLD", TierName = "Gold", TierType = MembershipTierType.Gold,
                IsActive = true, AnnualFeeAmount = 2_400_000m
            });
            await k.SaveChangesAsync();
        }

        return (await TambahPasienAsync(pg), gold);
    }

    private static async Task<Guid> TambahPasienAsync(PostgresSekaliPakai pg)
    {
        var id = Guid.NewGuid();
        await using var k = pg.BuatKonteks();
        k.Set<MstPatient>().Add(new MstPatient
        {
            Id = id, PatientCode = "P-" + id.ToString("N")[..6], MedicalRecordNumber = "RM-" + id.ToString("N")[..6],
            FullName = "Pasien Uji", IsActive = true, IsMember = false
        });
        await k.SaveChangesAsync();
        return id;
    }

    private static async Task<long> NilaiDeretAsync(ApplicationDbContext k) =>
        await k.Set<NumNumberSeries>().AsNoTracking()
            .Where(x => x.SequenceKey == DeretNomorMember)
            .Select(x => x.CurrentValue)
            .SingleAsync();

    private static async Task TungguMenungguKunciAsync(PostgresSekaliPakai pg, int jumlah)
    {
        await using var k = pg.BuatKonteks();
        var batas = DateTime.UtcNow + BatasTunggu;

        while (true)
        {
            var menunggu = await k.Database.SqlQueryRaw<int>(KunciAdvisoryMenunggu).SingleAsync();
            if (menunggu == jumlah)
                return;

            if (DateTime.UtcNow > batas)
                Assert.Fail($"Menunggu {jumlah} sesi antre di kunci pasien, yang terlihat {menunggu}.");

            await Task.Delay(50);
        }
    }

    // Kunci yang sama dengan PatientMembershipService.AcquirePatientLockAsync.
    private static async Task<PemegangKunci> PegangKunciPasienAsync(PostgresSekaliPakai pg, Guid pasien)
    {
        var koneksi = new NpgsqlConnection(pg.KoneksiDatabase);
        await koneksi.OpenAsync();
        var transaksi = await koneksi.BeginTransactionAsync();

        await using (var perintah = new NpgsqlCommand("SELECT pg_advisory_xact_lock(hashtext(@k));", koneksi, transaksi))
        {
            perintah.Parameters.AddWithValue("k", $"PAT_MEMBERSHIP_{pasien:N}");
            await perintah.ExecuteNonQueryAsync();
        }

        return new PemegangKunci(koneksi, transaksi);
    }

    private sealed class PemegangKunci(NpgsqlConnection koneksi, NpgsqlTransaction transaksi) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await transaksi.RollbackAsync();
            await transaksi.DisposeAsync();
            await koneksi.DisposeAsync();
        }
    }

    /// <summary>Tiruan port tagihan dibungkus kunci, karena dipanggil dari beberapa thread sekaligus.</summary>
    private sealed class PortTagihanAmanParalel(TiruanPortTagihanIuran dalam) : IMembershipFeeChargePort
    {
        private readonly object _kunci = new();

        public IReadOnlyList<MembershipFeeChargeRequest> PermintaanTagihan
        {
            get { lock (_kunci) return [.. dalam.PermintaanTagihan]; }
        }

        public Task<MembershipFeeChargeRef> EnsureChargeAsync(MembershipFeeChargeRequest request, CancellationToken cancellationToken)
        {
            lock (_kunci) return dalam.EnsureChargeAsync(request, cancellationToken);
        }

        public Task<MembershipFeeCancelOutcome> CancelChargeAsync(Guid membershipId, string reason, Guid actorUserId, CancellationToken cancellationToken)
        {
            lock (_kunci) return dalam.CancelChargeAsync(membershipId, reason, actorUserId, cancellationToken);
        }

        public Task<IReadOnlyDictionary<Guid, MembershipFeeChargeSummary>> GetSummariesAsync(IReadOnlyCollection<Guid> chargeIds, CancellationToken cancellationToken)
        {
            lock (_kunci) return dalam.GetSummariesAsync(chargeIds, cancellationToken);
        }

        public Task<int> RetryUnconfirmedAsync(CancellationToken cancellationToken) => dalam.RetryUnconfirmedAsync(cancellationToken);
    }

    private sealed class PabrikKonteks(PostgresSekaliPakai pg) : IDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext() => pg.BuatKonteks();
    }

    private sealed class JamTetap(DateTimeOffset sekarang) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => sekarang.ToUniversalTime();
    }
}
