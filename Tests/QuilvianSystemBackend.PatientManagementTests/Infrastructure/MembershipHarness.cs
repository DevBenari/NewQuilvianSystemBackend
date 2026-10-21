using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Services;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Tests.PatientManagement.Infrastructure
{
    /// <summary>
    /// Perlengkapan uji crm-membership: basis data SQLite, jam tetap, service, controller, dan
    /// pembuat data samaran. Deret nomor member tinggal di basis data terpisah karena alokator
    /// membuka transaksinya sendiri (SQLite tidak mendukung transaksi bersarang pada satu koneksi).
    /// </summary>
    public sealed class MembershipHarness : IDisposable
    {
        public static readonly Guid Aktor = Guid.Parse("44444444-4444-4444-4444-444444444444");

        private readonly TestDatabase _dbNomor = TestDatabase.Create();

        public MembershipHarness(DateTimeOffset sekarang)
        {
            Sekarang = sekarang;
            Db = TestDatabase.Create();
        }

        public TestDatabase Db { get; }

        public DateTimeOffset Sekarang { get; set; }

        public Dictionary<string, string?> Konfigurasi { get; } = new();

        public void Dispose()
        {
            Db.Dispose();
            _dbNomor.Dispose();
        }

        public ApplicationDbContext Konteks() => Db.CreateContext();

        public PatientMembershipService Service(ApplicationDbContext konteks)
        {
            var config = Config();
            var jam = new JamTetap(Sekarang);
            return new PatientMembershipService(
                konteks,
                new NumberSeriesAllocator(new PabrikKonteks(_dbNomor)),
                new PatientMembershipStatusResolver(konteks, config, jam),
                Logger(),
                config,
                jam);
        }

        public PatientMembershipController Controller(ApplicationDbContext konteks)
        {
            var resolver = new PatientMembershipStatusResolver(konteks, Config(), new JamTetap(Sekarang));
            var controller = new PatientMembershipController(konteks, Logger(), resolver, Service(konteks));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Aktor.ToString())], "uji"))
                }
            };
            return controller;
        }

        public Guid TambahTier(string nama, MembershipTierType tipe, int priorityLevel, decimal iuran = 2_400_000m, bool aktif = true)
        {
            var id = Guid.NewGuid();
            using var k = Konteks();
            k.Set<MstMembershipTier>().Add(new MstMembershipTier
            {
                Id = id, TierCode = nama.ToUpperInvariant(), TierName = nama, TierType = tipe,
                PriorityLevel = priorityLevel, AnnualFeeAmount = iuran, IsActive = aktif
            });
            k.SaveChanges();
            return id;
        }

        public Guid TambahPasien(bool aktif = true)
        {
            var id = Guid.NewGuid();
            using var k = Konteks();
            k.Set<MstPatient>().Add(new MstPatient
            {
                Id = id, PatientCode = "P-" + id.ToString("N")[..6], MedicalRecordNumber = "RM-" + id.ToString("N")[..6],
                FullName = "Pasien Uji", IsActive = aktif, IsMember = false
            });
            k.SaveChanges();
            return id;
        }

        public MstPatientMembership TambahMembership(
            Guid pasien,
            Guid tier,
            MembershipStatus status,
            DateTime? mulai = null,
            DateTime? akhir = null,
            MembershipRegistrationType jenis = MembershipRegistrationType.New)
        {
            var m = new MstPatientMembership
            {
                Id = Guid.NewGuid(), PatientId = pasien, MembershipTierId = tier,
                MemberNumber = "MBR-UJI-" + Guid.NewGuid().ToString("N")[..8],
                MembershipStatus = status, RegistrationType = jenis,
                JoinDate = new DateTime(2026, 10, 13), PeriodStartDate = mulai, ExpiredDate = akhir,
                IsActive = status == MembershipStatus.Active, IsPrimary = status == MembershipStatus.Active,
                RowVersion = Guid.NewGuid()
            };
            using var k = Konteks();
            k.Set<MstPatientMembership>().Add(m);
            k.SaveChanges();
            return m;
        }

        public MstPatientMembership Baca(Guid id)
        {
            using var k = Konteks();
            return k.Set<MstPatientMembership>().AsNoTracking().Single(x => x.Id == id);
        }

        private IConfiguration Config() => new ConfigurationBuilder().AddInMemoryCollection(Konfigurasi).Build();

        private static LoggerService Logger() => new(NullLogger<LoggerService>.Instance, new HttpContextAccessor());

        public static DateTime Tgl(int y, int m, int d) => new(y, m, d, 0, 0, 0, DateTimeKind.Unspecified);

        public static DateTimeOffset Wib(int y, int m, int d, int jam = 9, int menit = 0) =>
            new(y, m, d, jam, menit, 0, TimeSpan.FromHours(7));

        private sealed class PabrikKonteks(TestDatabase db) : IDbContextFactory<ApplicationDbContext>
        {
            public ApplicationDbContext CreateDbContext() => db.CreateContext();
        }

        private sealed class JamTetap(DateTimeOffset sekarang) : TimeProvider
        {
            public override DateTimeOffset GetUtcNow() => sekarang.ToUniversalTime();
        }
    }
}
