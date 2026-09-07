using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.BillingTests.Infrastructure;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.BillingTests.ClinicalIntegration
{
    /// <summary>
    /// Bukti acceptance <c>BE-RWI-041</c> yang <b>hanya dapat dibuktikan PostgreSQL sungguhan</b>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provider InMemory tidak menegakkan unique index, dan SQLite tidak menjalankan migration
    /// PostgreSQL. Dua acceptance criteria berikut karena itu tinggal di sini:
    /// </para>
    /// <list type="number">
    ///   <item>
    ///     <c>AC 2</c> — dua baris berkunci permintaan sama ditolak database, bukan sekadar
    ///     ditolak pemeriksaan di dalam aplikasi.
    ///   </item>
    ///   <item>
    ///     <c>AC 4</c> — dua visite dokter yang sama pada tanggal yang sama <b>diterima</b>
    ///     keduanya. Tidak ada unique atas pasangan perawatan, dokter, dan tanggal —
    ///     <c>RWI-DEC-085</c>.
    ///   </item>
    /// </list>
    /// <para>
    /// Fixture-nya bersifat fail-closed: tanpa environment variable
    /// <c>QUILVIAN_BILLING_TEST_DB</c> yang menunjuk database uji tersendiri, test berhenti
    /// sebagai galat konfigurasi dan tidak menyentuh database mana pun.
    /// </para>
    /// </remarks>
    public sealed class PhysicianVisitUniquenessTests
        : IClassFixture<BillingTestDatabaseFixture>, IAsyncLifetime
    {
        private readonly BillingTestDatabaseFixture _fixture;
        private readonly List<EncounterSeed> _seeds = new();
        private readonly List<Guid> _episodeIds = new();
        private readonly List<Guid> _doctorIds = new();
        private readonly List<Guid> _patientClassIds = new();

        public PhysicianVisitUniquenessTests(BillingTestDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        /// <summary>
        /// Membersihkan dari anak ke induk. Kejadian visite memiliki foreign key Restrict ke
        /// perawatan, pasien, dan dokter, sehingga barisnya wajib hilang lebih dulu.
        /// </summary>
        public async Task DisposeAsync()
        {
            await using (var context = _fixture.CreateContext())
            {
                await context.CliPhysicianVisits
                    .Where(x => _episodeIds.Contains(x.InpEpisodeId!.Value))
                    .ExecuteDeleteAsync();

                await context.Set<InpDoctorAssignment>()
                    .Where(x => _episodeIds.Contains(x.EpisodeId))
                    .ExecuteDeleteAsync();

                await context.Set<InpEpisode>()
                    .Where(x => _episodeIds.Contains(x.Id))
                    .ExecuteDeleteAsync();
            }

            foreach (var seed in _seeds)
                await _fixture.CleanupEncounterAsync(seed);

            await using (var context = _fixture.CreateContext())
            {
                await context.Set<MstDoctor>()
                    .Where(x => _doctorIds.Contains(x.Id))
                    .ExecuteDeleteAsync();

                await context.Set<MstPatientClass>()
                    .Where(x => _patientClassIds.Contains(x.Id))
                    .ExecuteDeleteAsync();
            }
        }

        private sealed record Perawatan(
            EncounterSeed Seed,
            Guid EpisodeId,
            Guid DoctorId);

        /// <summary>
        /// Menyiapkan satu kunjungan beserta perawatan rawat inap dan dokternya.
        /// </summary>
        private async Task<Perawatan> SiapkanPerawatanAsync()
        {
            var seed = await _fixture.SeedEncounterAsync();
            _seeds.Add(seed);

            await using var context = _fixture.CreateContext();

            var pembeda = Guid.NewGuid().ToString("N")[..12];

            var profil = new MstWorkforceProfile
            {
                ProfileCode = $"PRF{pembeda}",
                UserType = QuilvianSystemBackend.Enums.UserType.PermanentDoctor,
                DisplayName = $"Dokter Uji {pembeda}"
            };
            var jenisTenaga = new MstWorkforceType { WorkforceTypeCode = $"WFT{pembeda}", WorkforceTypeName = "Tenaga Medis" };
            var kategori = new MstEmployeeCategory { EmployeeCategoryCode = $"KAT{pembeda}", EmployeeCategoryName = "Tetap" };
            var jenisKepegawaian = new MstEmploymentType { EmploymentTypeCode = $"EMT{pembeda}", EmploymentTypeName = "Purnawaktu" };
            var statusKepegawaian = new MstEmploymentStatus { EmploymentStatusCode = $"EMS{pembeda}", EmploymentStatusName = "Aktif" };
            var profesi = new MstProfession { ProfessionCode = $"PRO{pembeda}", ProfessionName = "Dokter Umum", ProfessionGroup = "Medis" };

            context.AddRange(profil, jenisTenaga, kategori, jenisKepegawaian, statusKepegawaian, profesi);

            var patientClass = new MstPatientClass
            {
                PatientClassCode = $"KLS{pembeda}",
                PatientClassName = $"Kelas Uji {pembeda}",
                IsForInpatient = true
            };
            context.Set<MstPatientClass>().Add(patientClass);
            await context.SaveChangesAsync();

            _patientClassIds.Add(patientClass.Id);

            var dokter = new MstDoctor
            {
                WorkforceProfileId = profil.Id,
                DoctorCode = $"DOK{pembeda}",
                DoctorNumber = $"NO{pembeda}",
                FullName = $"Dokter Uji {pembeda}",
                WorkforceTypeId = jenisTenaga.Id,
                EmployeeCategoryId = kategori.Id,
                EmploymentTypeId = jenisKepegawaian.Id,
                EmploymentStatusId = statusKepegawaian.Id,
                ProfessionId = profesi.Id
            };
            context.Set<MstDoctor>().Add(dokter);
            await context.SaveChangesAsync();

            _doctorIds.Add(dokter.Id);

            var episode = new InpEpisode
            {
                EpisodeNumber = $"RI{pembeda}",
                EncounterId = seed.EncounterId,
                PatientId = seed.PatientId,
                ServiceUnitId = seed.ServiceUnitId,
                PatientClassId = patientClass.Id,
                EpisodeStatus = InpEpisodeStatus.Admitted,
                AdmittedAt = DateTime.UtcNow.AddDays(-1),
                CreateBy = seed.ActorUserId
            };
            context.Set<InpEpisode>().Add(episode);
            await context.SaveChangesAsync();

            _episodeIds.Add(episode.Id);

            return new Perawatan(seed, episode.Id, dokter.Id);
        }

        private static PhysicianVisitService Service(ApplicationDbContext context) =>
            new(context, new PhysicianVisitNumberService());

        private static RecordPhysicianVisitCommand Perintah(
            Perawatan p, DateTime waktu, string kunci) => new()
            {
                EncounterId = p.Seed.EncounterId,
                InpEpisodeId = p.EpisodeId,
                PatientId = p.Seed.PatientId,
                DoctorId = p.DoctorId,
                VisitDateTime = waktu,
                IdempotencyKey = kunci
            };

        /// <summary>
        /// `BE-RWI-041 AC 2` — database menolak dua baris berkunci permintaan sama.
        /// </summary>
        /// <remarks>
        /// Baris kedua ditulis lewat konteks yang berbeda dan tanpa melewati pemeriksaan di
        /// dalam service, sehingga yang terbukti memang penjagaan database — bukan penjagaan
        /// aplikasi yang kebetulan lebih dulu berjalan.
        /// </remarks>
        [Fact]
        public async Task KunciPermintaanKembar_DitolakDatabase()
        {
            var p = await SiapkanPerawatanAsync();
            const string kunci = "kunci-uji-postgres-01";

            await using (var context = _fixture.CreateContext())
            {
                var hasil = await Service(context).RecordAsync(
                    Perintah(p, DateTime.UtcNow.AddHours(-2), kunci), p.Seed.ActorUserId);

                Assert.True(hasil.IsSuccess);
            }

            await using (var context = _fixture.CreateContext())
            {
                context.CliPhysicianVisits.Add(new CliPhysicianVisit
                {
                    PhysicianVisitNumber = new PhysicianVisitNumberService().Generate(),
                    EncounterId = p.Seed.EncounterId,
                    InpEpisodeId = p.EpisodeId,
                    PatientId = p.Seed.PatientId,
                    DoctorId = p.DoctorId,
                    VisitDateTime = DateTime.UtcNow,
                    VisitStatus = PhysicianVisitStatus.Recorded,
                    RecordedByUserId = p.Seed.ActorUserId,
                    IdempotencyKey = kunci,
                    CreateBy = p.Seed.ActorUserId
                });

                await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            }

            await using (var context = _fixture.CreateContext())
            {
                Assert.Equal(1, await context.CliPhysicianVisits
                    .CountAsync(x => x.IdempotencyKey == kunci));
            }
        }

        /// <summary>
        /// `BE-RWI-041 AC 4` — dua visite dokter yang sama pada tanggal yang sama diterima
        /// keduanya, dan hitungannya dua.
        /// </summary>
        /// <remarks>
        /// Menolak visite kedua pada hari yang sama dilarang <c>RWI-DEC-085</c>: dokter yang
        /// benar-benar datang dua kali memang datang dua kali.
        /// </remarks>
        [Fact]
        public async Task DuaVisitePadaTanggalSama_DiterimaKeduanya()
        {
            var p = await SiapkanPerawatanAsync();
            var hariIni = DateTime.UtcNow.Date.AddHours(7);

            await using var context = _fixture.CreateContext();
            var service = Service(context);

            var pagi = await service.RecordAsync(
                Perintah(p, hariIni, "kunci-postgres-pagi"), p.Seed.ActorUserId);
            var sore = await service.RecordAsync(
                Perintah(p, hariIni.AddHours(9), "kunci-postgres-sore"), p.Seed.ActorUserId);

            Assert.True(pagi.IsSuccess);
            Assert.True(sore.IsSuccess);
            Assert.NotEqual(pagi.Visit!.Id, sore.Visit!.Id);

            Assert.Equal(2, await service.CountRecordedByEpisodeAsync(p.EpisodeId));

            var nomor = new[] { pagi.Visit.PhysicianVisitNumber, sore.Visit.PhysicianVisitNumber };
            Assert.Equal(2, nomor.Distinct().Count());
        }
    }
}
