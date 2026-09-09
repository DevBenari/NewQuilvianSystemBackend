using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.BillingTests.Infrastructure;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using Xunit;

namespace QuilvianSystemBackend.BillingTests.ClinicalIntegration
{
    /// <summary>
    /// Bukti acceptance <c>BE-RWI-051</c> yang <b>hanya dapat dibuktikan PostgreSQL sungguhan</b>
    /// — butir Verification "integration test terhadap PostgreSQL untuk percobaan ulang".
    /// </summary>
    /// <remarks>
    /// <para>
    /// Penjagaan percobaan ulang pada tindakan bersandar pada unique index <b>parsial</b>
    /// <c>IdempotencyKey</c> milik <c>TrxPatientProcedure</c>, yang penyaringnya khas PostgreSQL:
    /// <c>"IdempotencyKey" IS NOT NULL AND "IsDelete" = false</c>. SQLite tidak menjalankan
    /// migration PostgreSQL dan provider InMemory tidak menegakkan unique index sama sekali,
    /// sehingga uji pada keduanya hanya membuktikan penjagaan di <b>lapisan aplikasi</b>.
    /// </para>
    /// <para>
    /// Yang dibuktikan di sini adalah lapisan basis datanya: dua permintaan yang tiba
    /// benar-benar bersamaan tidak dapat ditahan oleh pemeriksaan "sudah ada" di dalam
    /// controller, karena keduanya berpeluang sama-sama menjawab belum ada.
    /// </para>
    /// <para>
    /// Yang paling mahal bila salah adalah tagihan ganda: tindakan kedua maupun fakta klinis
    /// kedua sama-sama berujung pada pasien membayar dua kali untuk satu tindakan yang sama.
    /// </para>
    /// </remarks>
    public sealed class PatientProcedureRetryTests
        : IClassFixture<BillingTestDatabaseFixture>, IAsyncLifetime
    {
        private readonly BillingTestDatabaseFixture _fixture;
        private readonly List<EncounterSeed> _seeds = new();
        private readonly List<Guid> _episodeIds = new();
        private readonly List<Guid> _consultationIds = new();
        private readonly List<Guid> _doctorIds = new();
        private readonly List<Guid> _patientClassIds = new();
        private readonly List<Guid> _procedureMasterIds = new();

        public PatientProcedureRetryTests(BillingTestDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        /// <summary>
        /// Membersihkan dari anak ke induk. Tindakan, catatan dokter, jejak keutuhan, dan
        /// perawatan seluruhnya memiliki foreign key Restrict ke kunjungan atau pasien,
        /// sehingga barisnya wajib hilang lebih dulu.
        /// </summary>
        public async Task DisposeAsync()
        {
            var encounterIds = _seeds.Select(x => x.EncounterId).ToList();

            await using (var context = _fixture.CreateContext())
            {
                await context.Set<TrxPatientProcedure>()
                    .Where(x => encounterIds.Contains(x.EncounterId))
                    .ExecuteDeleteAsync();

                await context.Set<MrcClinicalDocumentIntegrity>()
                    .Where(x => encounterIds.Contains(x.EncounterId))
                    .ExecuteDeleteAsync();

                await context.Set<TrxDoctorConsultation>()
                    .Where(x => _consultationIds.Contains(x.Id))
                    .ExecuteDeleteAsync();

                await context.Set<TrxPatientEncounterGuarantor>()
                    .Where(x => encounterIds.Contains(x.EncounterId))
                    .ExecuteDeleteAsync();

                await context.Set<InpEpisode>()
                    .Where(x => _episodeIds.Contains(x.Id))
                    .ExecuteDeleteAsync();
            }

            foreach (var seed in _seeds)
                await _fixture.CleanupEncounterAsync(seed);

            await using (var context = _fixture.CreateContext())
            {
                await context.Set<MstTariff>()
                    .Where(x => x.ProcedureId != null && _procedureMasterIds.Contains(x.ProcedureId.Value))
                    .ExecuteDeleteAsync();

                await context.Set<MstProcedure>()
                    .Where(x => _procedureMasterIds.Contains(x.Id))
                    .ExecuteDeleteAsync();

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
            Guid DoctorId,
            Guid ConsultationId);

        /// <summary>
        /// Menyiapkan satu kunjungan rawat inap lengkap dengan sumber pembayaran, dokter,
        /// perawatan, dan catatan dokter sebagai induk tindakan.
        /// </summary>
        /// <remarks>
        /// Sumber pembayaran wajib ada. Tanpa baris itu seluruh jalur yang menghitung tarif
        /// ditolak "Sumber pembayaran encounter tidak ditemukan", dan penolakan itu menyamarkan
        /// hal yang sedang diuji.
        /// </remarks>
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

            context.Set<TrxPatientEncounterGuarantor>().Add(new TrxPatientEncounterGuarantor
            {
                PaymentSourceNumber = $"BYR{pembeda}",
                EncounterId = seed.EncounterId,
                PatientId = seed.PatientId,
                PaymentType = EncounterPaymentType.Cash,
                PaymentSourceNameSnapshot = "Tunai",
                IsActive = true
            });

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

            var catatan = new TrxDoctorConsultation
            {
                ConsultationNumber = $"CON{pembeda}",
                EncounterId = seed.EncounterId,
                PatientId = seed.PatientId,
                DoctorId = dokter.Id,
                ServiceUnitId = seed.ServiceUnitId,
                InpEpisodeId = episode.Id,
                ConsultationDateTime = DateTime.UtcNow,
                ConsultationStatus = DoctorConsultationStatus.InProgress,
                IsActive = true
            };
            context.Set<TrxDoctorConsultation>().Add(catatan);
            await context.SaveChangesAsync();

            _consultationIds.Add(catatan.Id);

            return new Perawatan(seed, episode.Id, dokter.Id, catatan.Id);
        }

        /// <summary>
        /// Membuat satu tindakan master beserta tarif rumah sakitnya.
        /// </summary>
        private async Task<Guid> BuatTindakanMasterAsync()
        {
            await using var context = _fixture.CreateContext();

            var pembeda = Guid.NewGuid().ToString("N")[..12];

            var master = new MstProcedure
            {
                ProcedureCode = $"TND{pembeda}",
                ProcedureName = $"Perawatan Luka Uji {pembeda}",
                ProcedureType = "DoctorAction",
                IsDoctorAction = true
            };
            context.Set<MstProcedure>().Add(master);

            var kategoriTarif = new MstTariffCategory
            {
                TariffCategoryCode = $"KTF{pembeda}",
                TariffCategoryName = "Tindakan Uji",
                IsProcedure = true
            };
            context.Set<MstTariffCategory>().Add(kategoriTarif);
            await context.SaveChangesAsync();

            _procedureMasterIds.Add(master.Id);

            context.Set<MstTariff>().Add(new MstTariff
            {
                TariffCode = $"TRF{pembeda}",
                TariffName = $"Tarif Uji {pembeda}",
                TariffCategoryId = kategoriTarif.Id,
                ProcedureId = master.Id,
                NormalPrice = 150000m,
                IsActive = true
            });
            await context.SaveChangesAsync();

            return master.Id;
        }

        private static PatientProcedureController Controller(
            ApplicationDbContext context,
            Guid actorUserId)
        {
            var logger = BillingTestDatabaseFixture.CreateLoggerService();

            var controller = new PatientProcedureController(
                context,
                new EncounterInsuranceService(context),
                new InsuranceCoverageService(context, new EncounterInsuranceService(context)),
                new ClinicalMilestoneFactProducer(context, new BillingFolioService(context), logger),
                new ClinicalDocumentIntegrityService(context),
                new InpatientClinicalContextService(context),
                logger);

            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, actorUserId.ToString()),
                    new Claim("sub", actorUserId.ToString())
                },
                authenticationType: "UjiIntegrasiPostgres");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };

            return controller;
        }

        private static CreatePatientProcedureRequest Permintaan(
            Perawatan p,
            Guid procedureId,
            string? kunci) => new()
            {
                EncounterId = p.Seed.EncounterId,
                ConsultationId = p.ConsultationId,
                ProcedureId = procedureId,
                PatientId = p.Seed.PatientId,
                InpEpisodeId = p.EpisodeId,
                IdempotencyKey = kunci,
                Quantity = 1,
                UnitNameSnapshot = "TINDAKAN"
            };

        private static int KodeStatus(IActionResult hasil) => hasil switch
        {
            ObjectResult objek => objek.StatusCode ?? StatusCodes.Status200OK,
            StatusCodeResult kode => kode.StatusCode,
            _ => StatusCodes.Status200OK
        };

        private static T Isi<T>(IActionResult hasil)
        {
            var objek = Assert.IsAssignableFrom<ObjectResult>(hasil);
            var pembungkus = Assert.IsType<ApiResponse<T>>(objek.Value);

            Assert.NotNull(pembungkus.Data);
            return pembungkus.Data!;
        }

        /// <summary>
        /// `BE-RWI-051 AC 2` terhadap PostgreSQL — percobaan ulang berkunci sama tidak
        /// menghasilkan tindakan maupun fakta klinis ganda.
        /// </summary>
        /// <remarks>
        /// Dua sumber tagihan ganda yang berbeda diperiksa sekaligus: baris tindakan yang
        /// berganda, dan fakta klinis yang berganda walaupun tindakannya tunggal.
        /// </remarks>
        [Fact]
        public async Task PercobaanUlangBerkunciSama_TidakMenghasilkanTindakanMaupunFaktaGanda()
        {
            var p = await SiapkanPerawatanAsync();
            var masterId = await BuatTindakanMasterAsync();

            const string kunci = "TOMBOL-TINDAKAN-TERTEKAN-DUA-KALI-POSTGRES";

            await using var context = _fixture.CreateContext();

            var pertama = await Controller(context, p.Seed.ActorUserId)
                .CreateProcedure(Permintaan(p, masterId, kunci));
            var kedua = await Controller(context, p.Seed.ActorUserId)
                .CreateProcedure(Permintaan(p, masterId, kunci));

            Assert.Equal(StatusCodes.Status200OK, KodeStatus(pertama));
            Assert.Equal(StatusCodes.Status200OK, KodeStatus(kedua));

            var idTindakan = Isi<PatientProcedureCreateResponse>(pertama).Id;

            Assert.Equal(idTindakan, Isi<PatientProcedureCreateResponse>(kedua).Id);

            Assert.Equal(1, await context.Set<TrxPatientProcedure>()
                .CountAsync(x => x.IdempotencyKey == kunci));

            // Ditandai dikerjakan dua kali. Fakta klinisnya tetap satu baris, karena penandaan
            // yang berulang tidak menerbitkan fakta kedua.
            await Controller(context, p.Seed.ActorUserId)
                .ExecuteProcedure(idTindakan, new ExecutePatientProcedureRequest());
            await Controller(context, p.Seed.ActorUserId)
                .ExecuteProcedure(idTindakan, new ExecutePatientProcedureRequest());

            await using var verifikasi = _fixture.CreateContext();

            Assert.Equal(1, await verifikasi.Set<TrxPatientProcedure>()
                .CountAsync(x => x.IdempotencyKey == kunci));

            Assert.Equal(1, await verifikasi.CliClinicalMilestoneFacts
                .CountAsync(x => x.SourceAggregateId == idTindakan));
        }

        /// <summary>
        /// `BE-RWI-051 AC 2`, `AC-CAP024-02` — <b>dua permintaan bersamaan</b> berkunci sama
        /// hanya menyisakan satu tindakan.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Keduanya dijalankan pada dua konteks basis data yang berbeda dan dimulai bersamaan,
        /// sehingga pemeriksaan "sudah ada" di dalam controller berpeluang sama-sama menjawab
        /// belum ada. Yang menahan baris kedua dalam keadaan itu adalah unique index parsial
        /// pada database.
        /// </para>
        /// <para>
        /// Permintaan yang kalah boleh dijawab <c>200</c> — ketika pemeriksaan aplikasi masih
        /// sempat menangkapnya — atau <c>409</c> ketika database yang menolaknya. Yang
        /// <b>tidak</b> boleh terjadi adalah dua baris tindakan, karena dua baris berarti dua
        /// tagihan. Keduanya juga tidak boleh berakhir sebagai galat yang tidak tertangani.
        /// </para>
        /// </remarks>
        [Fact]
        public async Task DuaPermintaanBersamaan_KunciSama_HanyaSatuTindakan()
        {
            var p = await SiapkanPerawatanAsync();
            var masterId = await BuatTindakanMasterAsync();

            const string kunci = "TINDAKAN-BERSAMAAN-POSTGRES";

            async Task<IActionResult> Kirim()
            {
                await using var context = _fixture.CreateContext();
                return await Controller(context, p.Seed.ActorUserId)
                    .CreateProcedure(Permintaan(p, masterId, kunci));
            }

            var keduanya = await Task.WhenAll(Kirim(), Kirim());

            Assert.All(keduanya, x => Assert.Contains(
                KodeStatus(x),
                new[] { StatusCodes.Status200OK, StatusCodes.Status409Conflict }));

            Assert.Contains(keduanya, x => KodeStatus(x) == StatusCodes.Status200OK);

            await using var verifikasi = _fixture.CreateContext();

            Assert.Equal(1, await verifikasi.Set<TrxPatientProcedure>()
                .CountAsync(x => x.IdempotencyKey == kunci));
        }

        /// <summary>
        /// `BE-RWI-051 AC 2` — database menolak baris kedua berkunci sama, walaupun ditulis
        /// langsung tanpa melewati pemeriksaan di dalam controller.
        /// </summary>
        /// <remarks>
        /// Baris kedua sengaja memakai tindakan master yang <b>berbeda</b>, sehingga unique
        /// atas pasangan catatan dan tindakan tidak mungkin ikut menolaknya. Yang tersisa
        /// sebagai penolak hanyalah unique parsial pada kunci permintaan, dan itulah yang
        /// sedang dibuktikan.
        /// </remarks>
        [Fact]
        public async Task KunciPermintaanKembar_DitolakDatabase()
        {
            var p = await SiapkanPerawatanAsync();
            var masterPertama = await BuatTindakanMasterAsync();
            var masterKedua = await BuatTindakanMasterAsync();

            const string kunci = "TINDAKAN-KUNCI-KEMBAR-POSTGRES";

            await using (var context = _fixture.CreateContext())
            {
                var hasil = await Controller(context, p.Seed.ActorUserId)
                    .CreateProcedure(Permintaan(p, masterPertama, kunci));

                Assert.Equal(StatusCodes.Status200OK, KodeStatus(hasil));
            }

            await using (var context = _fixture.CreateContext())
            {
                context.Set<TrxPatientProcedure>().Add(new TrxPatientProcedure
                {
                    EncounterId = p.Seed.EncounterId,
                    ConsultationId = p.ConsultationId,
                    PatientId = p.Seed.PatientId,
                    DoctorId = p.DoctorId,
                    ServiceUnitId = p.Seed.ServiceUnitId,
                    InpEpisodeId = p.EpisodeId,
                    IdempotencyKey = kunci,
                    ProcedureId = masterKedua,
                    ProcedureCodeSnapshot = "TND-LAIN",
                    ProcedureNameSnapshot = "Tindakan Uji Lain",
                    ProcedureSource = PatientProcedureSource.DoctorOrder,
                    ProcedureStatus = PatientProcedureStatus.Planned,
                    ProcedureDateTime = DateTime.UtcNow,
                    IsActive = true,
                    CreateBy = p.Seed.ActorUserId
                });

                await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            }

            await using (var context = _fixture.CreateContext())
            {
                Assert.Equal(1, await context.Set<TrxPatientProcedure>()
                    .CountAsync(x => x.IdempotencyKey == kunci));
            }
        }

        /// <summary>
        /// `BE-RWI-051 AC 2` — penyaring index memang parsial: tindakan <b>tanpa</b> kunci
        /// permintaan tetap tersimpan berdampingan.
        /// </summary>
        /// <remarks>
        /// Kunci permintaan bersifat opsional, dan tindakan poliklinik maupun IGD tidak
        /// membawanya. Index yang tidak parsial akan mematikan jalur-jalur itu.
        /// </remarks>
        [Fact]
        public async Task TindakanTanpaKunci_TetapDapatBerdampingan()
        {
            var p = await SiapkanPerawatanAsync();

            await using var context = _fixture.CreateContext();

            for (var i = 0; i < 3; i++)
            {
                var masterId = await BuatTindakanMasterAsync();

                var hasil = await Controller(context, p.Seed.ActorUserId)
                    .CreateProcedure(Permintaan(p, masterId, kunci: null));

                Assert.Equal(StatusCodes.Status200OK, KodeStatus(hasil));
            }

            await using var verifikasi = _fixture.CreateContext();

            Assert.Equal(3, await verifikasi.Set<TrxPatientProcedure>()
                .CountAsync(x => x.ConsultationId == p.ConsultationId && x.IdempotencyKey == null));
        }
    }
}
