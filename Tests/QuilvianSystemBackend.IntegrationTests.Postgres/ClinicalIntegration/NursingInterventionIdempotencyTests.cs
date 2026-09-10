using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.BillingTests.Infrastructure;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.BillingTests.ClinicalIntegration
{
    /// <summary>
    /// Bukti acceptance <c>BE-RWI-061</c> yang <b>hanya dapat dibuktikan PostgreSQL sungguhan</b>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provider InMemory tidak menegakkan unique index sama sekali, dan SQLite tidak menjalankan
    /// migration PostgreSQL. Dua hal berikut karena itu tinggal di sini:
    /// </para>
    /// <list type="number">
    ///   <item>
    ///     <c>AC 3</c> dan <c>AC 6</c> — <b>dua permintaan bersamaan</b> berkunci sama hanya
    ///     menghasilkan satu baris, dan yang menolaknya adalah <b>database</b>, bukan pemeriksaan
    ///     di dalam aplikasi yang kebetulan lebih dulu berjalan. Idempotency yang hanya dipasang
    ///     di aplikasi akan bocor begitu ada dua instance berjalan.
    ///   </item>
    ///   <item>
    ///     <c>AC 6</c> — penyaring index memang parsial: banyak tindakan <b>tanpa</b> kunci tetap
    ///     boleh tersimpan berdampingan.
    ///   </item>
    /// </list>
    /// <para>
    /// Fixture-nya bersifat fail-closed: tanpa environment variable
    /// <c>QUILVIAN_BILLING_TEST_DB</c> yang menunjuk database uji tersendiri, test berhenti
    /// sebagai galat konfigurasi dan tidak menyentuh database mana pun.
    /// </para>
    /// </remarks>
    [Collection(PostgresIntegrationTestCollection.Name)]
    public sealed class NursingInterventionIdempotencyTests
        : IAsyncLifetime
    {
        private readonly BillingTestDatabaseFixture _fixture;
        private readonly List<EncounterSeed> _seeds = new();
        private readonly List<Guid> _episodeIds = new();
        private readonly List<Guid> _employeeIds = new();
        private readonly List<Guid> _patientClassIds = new();

        public NursingInterventionIdempotencyTests(BillingTestDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        /// <summary>
        /// Membersihkan dari anak ke induk. Catatan tindakan memiliki foreign key Restrict ke
        /// perawatan, pasien, dan pegawai, sehingga barisnya wajib hilang lebih dulu.
        /// </summary>
        public async Task DisposeAsync()
        {
            await using (var context = _fixture.CreateContext())
            {
                await context.Set<CliNursingIntervention>()
                    .Where(x => _episodeIds.Contains(x.InpEpisodeId!.Value))
                    .ExecuteDeleteAsync();

                await context.Set<InpEpisode>()
                    .Where(x => _episodeIds.Contains(x.Id))
                    .ExecuteDeleteAsync();
            }

            foreach (var seed in _seeds)
                await _fixture.CleanupEncounterAsync(seed);

            await using (var context = _fixture.CreateContext())
            {
                await context.Set<MstEmployee>()
                    .Where(x => _employeeIds.Contains(x.Id))
                    .ExecuteDeleteAsync();

                await context.Set<MstPatientClass>()
                    .Where(x => _patientClassIds.Contains(x.Id))
                    .ExecuteDeleteAsync();
            }
        }

        private sealed record Perawatan(
            EncounterSeed Seed,
            Guid EpisodeId,
            Guid EmployeeId);

        /// <summary>
        /// Menyiapkan satu kunjungan beserta perawatan rawat inap dan perawat pelaksananya.
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
                UserType = QuilvianSystemBackend.Enums.UserType.Employee,
                DisplayName = $"Perawat Uji {pembeda}"
            };
            var departemen = new MstDepartment { DepartmentCode = $"DEP{pembeda}", DepartmentName = "Keperawatan" };
            var jenisTenaga = new MstWorkforceType { WorkforceTypeCode = $"WFT{pembeda}", WorkforceTypeName = "Tenaga Keperawatan" };
            var kategori = new MstEmployeeCategory { EmployeeCategoryCode = $"KAT{pembeda}", EmployeeCategoryName = "Tetap" };
            var jenisKepegawaian = new MstEmploymentType { EmploymentTypeCode = $"EMT{pembeda}", EmploymentTypeName = "Purnawaktu" };
            var statusKepegawaian = new MstEmploymentStatus { EmploymentStatusCode = $"EMS{pembeda}", EmploymentStatusName = "Aktif" };

            context.AddRange(profil, departemen, jenisTenaga, kategori, jenisKepegawaian, statusKepegawaian);

            var patientClass = new MstPatientClass
            {
                PatientClassCode = $"KLS{pembeda}",
                PatientClassName = $"Kelas Uji {pembeda}",
                IsForInpatient = true
            };
            context.Set<MstPatientClass>().Add(patientClass);
            await context.SaveChangesAsync();

            _patientClassIds.Add(patientClass.Id);

            var posisi = new MstPosition
            {
                DepartmentId = departemen.Id,
                PositionCode = $"POS{pembeda}",
                PositionName = "Perawat Pelaksana"
            };
            context.Set<MstPosition>().Add(posisi);
            await context.SaveChangesAsync();

            var pegawai = new MstEmployee
            {
                WorkforceProfileId = profil.Id,
                EmployeeCode = $"PEG{pembeda}",
                EmployeeNumber = $"NIP{pembeda}",
                FullName = $"Ns. Uji {pembeda}",
                BirthDate = new DateTime(1990, 1, 1),
                IdentityType = "KTP",
                IdentityNumber = $"327{pembeda}",
                Email = $"perawat.{pembeda}@contoh.uji",
                PrimaryDepartmentId = departemen.Id,
                PrimaryPositionId = posisi.Id,
                WorkforceTypeId = jenisTenaga.Id,
                EmployeeCategoryId = kategori.Id,
                EmploymentTypeId = jenisKepegawaian.Id,
                EmploymentStatusId = statusKepegawaian.Id,
                JoinDate = new DateTime(2020, 1, 1)
            };
            context.Set<MstEmployee>().Add(pegawai);
            await context.SaveChangesAsync();

            _employeeIds.Add(pegawai.Id);

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

            return new Perawatan(seed, episode.Id, pegawai.Id);
        }

        private static NursingInterventionService Service(ApplicationDbContext context)
        {
            var keutuhan = new ClinicalDocumentIntegrityService(context);

            return new NursingInterventionService(
                context,
                new InpatientClinicalContextService(context),
                new NursingActorService(context),
                keutuhan,
                new ClinicalNoteAddendumService(context, keutuhan));
        }

        private static CreateNursingInterventionRequest Permintaan(Perawatan p, string? kunci) => new()
        {
            EncounterId = p.Seed.EncounterId,
            InpEpisodeId = p.EpisodeId,
            InterventionName = "Pemasangan infus",
            PerformedAt = DateTime.UtcNow.AddHours(-2),
            PerformedByEmployeeId = p.EmployeeId,
            IdempotencyKey = kunci
        };

        /// <summary>
        /// `BE-RWI-061 AC 3` dan `AC 6` — <b>dua permintaan bersamaan</b> berkunci sama hanya
        /// menghasilkan satu baris, dan permintaan yang kalah tetap dijawab berhasil beserta
        /// baris yang sudah ada.
        /// </summary>
        /// <remarks>
        /// Keduanya dijalankan pada dua konteks basis data yang berbeda dan dimulai bersamaan,
        /// sehingga pemeriksaan "sudah ada" di dalam aplikasi berpeluang besar sama-sama menjawab
        /// belum ada. Yang menahannya adalah unique parsial pada database.
        /// </remarks>
        [Fact]
        public async Task DuaPermintaanBersamaan_KunciSama_HanyaSatuBaris()
        {
            var p = await SiapkanPerawatanAsync();
            const string kunci = "kunci-tindakan-postgres-01";

            async Task<NursingInterventionResult> Kirim()
            {
                await using var context = _fixture.CreateContext();
                return await Service(context).RecordAsync(
                    Permintaan(p, kunci), kunci, null, p.Seed.ActorUserId);
            }

            var keduanya = await Task.WhenAll(Kirim(), Kirim());

            Assert.All(keduanya, x => Assert.True(x.IsSuccess, x.ErrorMessage));

            // Keduanya menunjuk baris yang sama, dan tepat satu di antaranya adalah kiriman ulang.
            Assert.Equal(keduanya[0].Intervention!.Id, keduanya[1].Intervention!.Id);
            Assert.Contains(keduanya, x => x.IsReplay);

            await using var pembaca = _fixture.CreateContext();

            Assert.Equal(1, await pembaca.Set<CliNursingIntervention>()
                .CountAsync(x => x.IdempotencyKey == kunci));
        }

        /// <summary>
        /// `BE-RWI-061 AC 6` — database menolak baris kedua berkunci sama, walaupun ditulis
        /// langsung tanpa melewati pemeriksaan di dalam service.
        /// </summary>
        [Fact]
        public async Task KunciKembar_DitolakDatabase()
        {
            var p = await SiapkanPerawatanAsync();
            const string kunci = "kunci-tindakan-postgres-02";

            await using (var context = _fixture.CreateContext())
            {
                var hasil = await Service(context).RecordAsync(
                    Permintaan(p, kunci), kunci, null, p.Seed.ActorUserId);

                Assert.True(hasil.IsSuccess, hasil.ErrorMessage);
            }

            await using (var context = _fixture.CreateContext())
            {
                context.Set<CliNursingIntervention>().Add(new CliNursingIntervention
                {
                    EncounterId = p.Seed.EncounterId,
                    InpEpisodeId = p.EpisodeId,
                    PatientId = p.Seed.PatientId,
                    InterventionName = "Pemasangan infus",
                    PerformedAt = DateTime.UtcNow,
                    PerformedByEmployeeId = p.EmployeeId,
                    RecordStatus = NursingInterventionStatus.Recorded,
                    IdempotencyKey = kunci,
                    CreateBy = p.Seed.ActorUserId
                });

                await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            }

            await using (var context = _fixture.CreateContext())
            {
                Assert.Equal(1, await context.Set<CliNursingIntervention>()
                    .CountAsync(x => x.IdempotencyKey == kunci));
            }
        }

        /// <summary>
        /// `BE-RWI-061 AC 6` — penyaring index memang parsial: tiga tindakan <b>tanpa</b> kunci
        /// tersimpan berdampingan tanpa saling menolak.
        /// </summary>
        [Fact]
        public async Task TigaTindakanTanpaKunci_DiterimaSeluruhnya()
        {
            var p = await SiapkanPerawatanAsync();

            await using var context = _fixture.CreateContext();
            var service = Service(context);

            for (var i = 0; i < 3; i++)
            {
                var hasil = await service.RecordAsync(
                    Permintaan(p, null), null, null, p.Seed.ActorUserId);

                Assert.True(hasil.IsSuccess, hasil.ErrorMessage);
            }

            Assert.Equal(3, await context.Set<CliNursingIntervention>()
                .CountAsync(x => x.InpEpisodeId == p.EpisodeId && x.IdempotencyKey == null));
        }
    }
}
