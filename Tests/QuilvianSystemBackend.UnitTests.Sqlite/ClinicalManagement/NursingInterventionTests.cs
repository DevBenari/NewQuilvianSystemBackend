using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-061</c> — tindakan keperawatan tercatat sekali walaupun
    /// tombolnya tertekan dua kali.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Yang tidak dapat dibuktikan berkas ini</b> adalah perlombaan dua permintaan yang tiba
    /// benar-benar bersamaan. Itu menuntut PostgreSQL sungguhan, dan buktinya berada pada
    /// <c>Tests/QuilvianSystemBackend.IntegrationTests.Postgres/ClinicalIntegration/NursingInterventionIdempotencyTests.cs</c>.
    /// Yang dibuktikan di sini adalah kiriman ulang berurutan, kedua batas waktu, tindakan tanpa
    /// rencana asuhan, dan bentuk index parsialnya pada model EF Core.
    /// </para>
    /// </remarks>
    public class NursingInterventionTests
    {
        internal static NursingInterventionController BuatController(
            ApplicationDbContext context,
            Guid actorUserId)
        {
            var interventionService = BuatService(context);

            var controller = new NursingInterventionController(
                ControllerTestHarness.BuatLoggerService(actorUserId),
                interventionService);

            return controller.DenganPengguna(actorUserId);
        }

        /// <summary>
        /// Membentuk service tindakan beserta seluruh ketergantungannya, sama seperti yang
        /// dipasang dependency injection aplikasi.
        /// </summary>
        internal static NursingInterventionService BuatService(ApplicationDbContext context) =>
            new(context,
                new InpatientClinicalContextService(context),
                new NursingActorService(context),
                new ClinicalDocumentIntegrityService(context),
                new ClinicalNoteAddendumService(context, new ClinicalDocumentIntegrityService(context)));

        private static T Isi<T>(IActionResult hasil) where T : class
        {
            var objek = Assert.IsAssignableFrom<ObjectResult>(hasil);
            var pembungkus = Assert.IsType<ApiResponse<T>>(objek.Value);
            Assert.NotNull(pembungkus.Data);
            return pembungkus.Data!;
        }

        private static async Task<(RawatInapTestData.Konteks Konteks, Guid PerawatUserId, Guid PerawatEmployeeId)>
            SiapkanAsync(ApplicationDbContext context)
        {
            var konteks = RawatInapTestData.SiapkanPerawatan(context);
            var (pegawai, akun) = RawatInapTestData.BuatPerawat(context);

            await Task.CompletedTask;

            return (konteks, akun.Id, pegawai.Id);
        }

        // =====================================================================
        // Kriteria 1 - apa, kapan, oleh siapa, hasilnya, beserta konteks perawatan
        // =====================================================================

        /// <summary>
        /// `BE-RWI-061 AC 1` — tindakan menyimpan apa, kapan, oleh siapa, hasilnya, dan konteks
        /// perawatannya.
        /// </summary>
        [Fact]
        public async Task Tindakan_MenyimpanApaKapanSiapaDanHasilnya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, perawatUserId, perawatEmployeeId) = await SiapkanAsync(context);

            var waktuTindakan = DateTime.UtcNow.AddHours(-2);

            var hasil = await BuatController(context, perawatUserId)
                .CreateNursingIntervention(new CreateNursingInterventionRequest
                {
                    EncounterId = konteks.EncounterId,
                    InterventionName = "Pemasangan infus",
                    PerformedAt = waktuTindakan,
                    ResultNote = "Infus terpasang di tangan kiri, aliran lancar, tidak ada tanda flebitis"
                });

            Assert.Equal(201, ControllerTestHarness.KodeStatus(hasil));

            var response = Isi<NursingInterventionResponse>(hasil);

            using var pembaca = database.CreateContext();

            var tersimpan = await pembaca.Set<CliNursingIntervention>().SingleAsync(x => x.Id == response.Id);

            Assert.Equal("Pemasangan infus", tersimpan.InterventionName);
            Assert.Equal(waktuTindakan, tersimpan.PerformedAt, TimeSpan.FromSeconds(1));
            Assert.Equal(perawatEmployeeId, tersimpan.PerformedByEmployeeId);
            Assert.Equal(
                "Infus terpasang di tangan kiri, aliran lancar, tidak ada tanda flebitis",
                tersimpan.ResultNote);

            // Konteks perawatan diturunkan backend dari kunjungan, bukan diterima dari klien.
            Assert.Equal(konteks.EpisodeId, tersimpan.InpEpisodeId);
            Assert.Equal(konteks.EncounterId, tersimpan.EncounterId);
            Assert.Equal(konteks.PatientId, tersimpan.PatientId);
            Assert.Equal(NursingInterventionStatus.Recorded, tersimpan.RecordStatus);
        }

        /// <summary>
        /// `BE-RWI-061 AC 1` — daftar per perawatan terurut menurut waktu tindakan, bukan waktu
        /// pencatatan.
        /// </summary>
        [Fact]
        public async Task DaftarTindakan_TerurutWaktuTindakan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, perawatUserId, _) = await SiapkanAsync(context);

            // Sengaja dicatat terbalik: yang paling lama dikerjakan justru dicatat terakhir.
            foreach (var (nama, jamLalu) in new[] { ("Observasi tanda vital", 1), ("Pemasangan infus", 5) })
            {
                var hasil = await BuatController(context, perawatUserId)
                    .CreateNursingIntervention(new CreateNursingInterventionRequest
                    {
                        EncounterId = konteks.EncounterId,
                        InterventionName = nama,
                        PerformedAt = DateTime.UtcNow.AddHours(-jamLalu)
                    });

                Assert.Equal(201, ControllerTestHarness.KodeStatus(hasil));
            }

            var daftar = Isi<PagedResult<NursingInterventionListItem>>(
                await BuatController(context, perawatUserId).GetByEpisode(konteks.EpisodeId));

            Assert.Equal(2, daftar.TotalData);
            Assert.Equal(
                new[] { "Pemasangan infus", "Observasi tanda vital" },
                daftar.Items.Select(x => x.InterventionName));
        }

        // =====================================================================
        // Kriteria 2 - tindakan mendadak tanpa rencana asuhan
        // =====================================================================

        /// <summary>
        /// `BE-RWI-061 AC 2`, `CAP-014` aturan 3 — tindakan mendadak dapat dicatat tanpa rujukan
        /// ke rencana asuhan.
        /// </summary>
        [Fact]
        public async Task TindakanMendadak_DapatDicatatTanpaRencanaAsuhan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, perawatUserId, _) = await SiapkanAsync(context);

            // Tidak ada rencana asuhan sama sekali pada perawatan ini.
            Assert.Equal(0, await context.Set<CliNursingCarePlan>()
                .CountAsync(x => x.InpEpisodeId == konteks.EpisodeId));

            var hasil = await BuatController(context, perawatUserId)
                .CreateNursingIntervention(new CreateNursingInterventionRequest
                {
                    EncounterId = konteks.EncounterId,
                    InterventionName = "Resusitasi cairan darurat"
                });

            Assert.Equal(201, ControllerTestHarness.KodeStatus(hasil));

            var response = Isi<NursingInterventionResponse>(hasil);

            Assert.Null(response.CarePlanItemId);
        }

        // =====================================================================
        // Kriteria 3 - VAL-KEP-15 idempotency
        // =====================================================================

        /// <summary>
        /// `BE-RWI-061 AC 3`, `VAL-KEP-15` — kiriman ulang dengan kunci yang sama menghasilkan
        /// <b>satu</b> baris dan dijawab <c>200</c> beserta baris yang sudah ada.
        /// </summary>
        [Fact]
        public async Task KiriminUlangKunciSama_MenghasilkanSatuBarisDanDijawab200()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, perawatUserId, _) = await SiapkanAsync(context);

            CreateNursingInterventionRequest Permintaan() => new()
            {
                EncounterId = konteks.EncounterId,
                InterventionName = "Pemasangan infus",
                IdempotencyKey = "kunci-uji-001"
            };

            var pertama = await BuatController(context, perawatUserId)
                .CreateNursingIntervention(Permintaan());

            Assert.Equal(201, ControllerTestHarness.KodeStatus(pertama));

            var kedua = await BuatController(context, perawatUserId)
                .CreateNursingIntervention(Permintaan());

            // 200, bukan 201 yang melahirkan baris kedua, dan bukan 409 yang menampilkan galat
            // padahal tindakannya sudah tercatat dengan benar.
            Assert.Equal(200, ControllerTestHarness.KodeStatus(kedua));

            var isiPertama = Isi<NursingInterventionResponse>(pertama);
            var isiKedua = Isi<NursingInterventionResponse>(kedua);

            Assert.Equal(isiPertama.Id, isiKedua.Id);
            Assert.True(isiKedua.IsReplay);

            using var pembaca = database.CreateContext();

            Assert.Equal(1, await pembaca.Set<CliNursingIntervention>()
                .CountAsync(x => x.InpEpisodeId == konteks.EpisodeId && !x.IsDelete));
        }

        /// <summary>
        /// `BE-RWI-061 AC 3` — kunci yang berbeda tetap menghasilkan dua tindakan. Idempotency
        /// tidak boleh menelan tindakan kedua yang memang benar-benar berbeda.
        /// </summary>
        [Fact]
        public async Task KunciBerbeda_MenghasilkanDuaTindakan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, perawatUserId, _) = await SiapkanAsync(context);

            foreach (var kunci in new[] { "kunci-uji-001", "kunci-uji-002" })
            {
                var hasil = await BuatController(context, perawatUserId)
                    .CreateNursingIntervention(new CreateNursingInterventionRequest
                    {
                        EncounterId = konteks.EncounterId,
                        InterventionName = "Observasi tanda vital",
                        IdempotencyKey = kunci
                    });

                Assert.Equal(201, ControllerTestHarness.KodeStatus(hasil));
            }

            using var pembaca = database.CreateContext();

            Assert.Equal(2, await pembaca.Set<CliNursingIntervention>()
                .CountAsync(x => x.InpEpisodeId == konteks.EpisodeId && !x.IsDelete));
        }

        /// <summary>
        /// `BE-RWI-061 AC 6` — index parsial: dua tindakan <b>tanpa</b> kunci tetap boleh
        /// tersimpan berdampingan, karena penyaringnya menyebut kunci yang tidak kosong.
        /// </summary>
        [Fact]
        public async Task TindakanTanpaKunci_TidakSalingMenghalangi()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, perawatUserId, _) = await SiapkanAsync(context);

            for (var i = 0; i < 2; i++)
            {
                var hasil = await BuatController(context, perawatUserId)
                    .CreateNursingIntervention(new CreateNursingInterventionRequest
                    {
                        EncounterId = konteks.EncounterId,
                        InterventionName = $"Perawatan luka ke-{i + 1}"
                    });

                Assert.Equal(201, ControllerTestHarness.KodeStatus(hasil));
            }

            using var pembaca = database.CreateContext();

            Assert.Equal(2, await pembaca.Set<CliNursingIntervention>()
                .CountAsync(x => x.InpEpisodeId == konteks.EpisodeId && x.IdempotencyKey == null));
        }

        // =====================================================================
        // Kriteria 4 dan 5 - dua batas waktu
        // =====================================================================

        /// <summary>
        /// `BE-RWI-061 AC 4`, `VAL-KEP-13` — waktu tindakan di masa depan ditolak <c>400</c>,
        /// dan tidak ada baris yang tersimpan.
        /// </summary>
        [Fact]
        public async Task WaktuTindakanMasaDepan_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, perawatUserId, _) = await SiapkanAsync(context);

            var hasil = await BuatController(context, perawatUserId)
                .CreateNursingIntervention(new CreateNursingInterventionRequest
                {
                    EncounterId = konteks.EncounterId,
                    InterventionName = "Pemberian obat",
                    PerformedAt = DateTime.UtcNow.AddHours(3)
                });

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(NursingInterventionService.PenolakanWaktuMasaDepan, ControllerTestHarness.Pesan(hasil));

            using var pembaca = database.CreateContext();

            Assert.Equal(0, await pembaca.Set<CliNursingIntervention>().CountAsync());
        }

        /// <summary>
        /// `BE-RWI-061 AC 5`, `VAL-KEP-14` — waktu tindakan sebelum pasien masuk kamar ditolak
        /// <c>400</c>.
        /// </summary>
        [Fact]
        public async Task WaktuTindakanSebelumMasukKamar_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, perawatUserId, _) = await SiapkanAsync(context);

            var episode = await context.Set<InpEpisode>().SingleAsync(x => x.Id == konteks.EpisodeId);

            var hasil = await BuatController(context, perawatUserId)
                .CreateNursingIntervention(new CreateNursingInterventionRequest
                {
                    EncounterId = konteks.EncounterId,
                    InterventionName = "Pemberian obat",
                    PerformedAt = episode.AdmittedAt!.Value.AddHours(-1)
                });

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(NursingInterventionService.PenolakanSebelumMasukKamar, ControllerTestHarness.Pesan(hasil));
        }

        // =====================================================================
        // Kriteria 6 - bentuk index parsial pada model EF Core
        // =====================================================================

        /// <summary>
        /// `BE-RWI-061 AC 6` — unique parsial pada kunci permintaan terbentuk dengan penyaring
        /// kunci tidak kosong <b>dan</b> baris belum terhapus.
        /// </summary>
        /// <remarks>
        /// Uji ini membaca model EF Core yang sama dengan yang dipakai aplikasi, sehingga
        /// penyaring yang diperiksa adalah penyaring yang benar-benar ditulis migration. Bukti
        /// bahwa index itu benar-benar menolak dua permintaan bersamaan berada pada uji
        /// PostgreSQL, karena provider InMemory maupun SQLite tidak dapat membuktikan perlombaan.
        /// </remarks>
        [Fact]
        public void IndexKunciPermintaan_UniqueDanParsial()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var entity = context.Model.FindEntityType(typeof(CliNursingIntervention))!;

            var index = entity.GetIndexes()
                .Single(x => x.Properties.Count == 1 &&
                             x.Properties[0].Name == nameof(CliNursingIntervention.IdempotencyKey));

            Assert.True(index.IsUnique);

            var filter = index.GetFilter();

            Assert.NotNull(filter);
            Assert.Contains("\"IdempotencyKey\" IS NOT NULL", filter);
            Assert.Contains("\"IsDelete\" = false", filter);
        }

        /// <summary>
        /// Penjaga tambahan: tindakan yang menunjuk butir rencana asuhan milik perawatan lain
        /// ditolak <c>400</c>.
        /// </summary>
        [Fact]
        public async Task Tindakan_MenolakButirRencanaMilikPerawatanLain()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, perawatUserId, _) = await SiapkanAsync(context);

            // Rencana asuhan dibuat pada perawatan yang berbeda.
            var perawatanLain = RawatInapTestData.SiapkanPerawatan(context);
            var (_, akunLain) = RawatInapTestData.BuatPerawat(context);

            var rencanaLain = Isi<NursingCarePlanResponse>(
                await NursingCarePlanTests.BuatController(context, akunLain.Id)
                    .CreateCarePlan(new CreateNursingCarePlanRequest
                    {
                        EncounterId = perawatanLain.EncounterId
                    }));

            var butirLain = Isi<CarePlanItemResponse>(
                await NursingCarePlanTests.BuatController(context, akunLain.Id)
                    .CreateCarePlanItem(rencanaLain.Id, new CreateCarePlanItemRequest
                    {
                        ProblemStatement = "Nyeri akut"
                    }));

            var hasil = await BuatController(context, perawatUserId)
                .CreateNursingIntervention(new CreateNursingInterventionRequest
                {
                    EncounterId = konteks.EncounterId,
                    InterventionName = "Manajemen nyeri",
                    CarePlanItemId = butirLain.Id
                });

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
        }

        /// <summary>
        /// Tindakan yang dapat ditagih lahir berkeadaan <c>Pending</c>; yang tidak dapat
        /// ditagih lahir <c>NotApplicable</c> — <c>INT-KEP-05</c>.
        /// </summary>
        [Fact]
        public async Task KeadaanTagihanAwal_MengikutiPenandaDapatDitagih()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, perawatUserId, _) = await SiapkanAsync(context);

            var ditagih = Isi<NursingInterventionResponse>(
                await BuatController(context, perawatUserId)
                    .CreateNursingIntervention(new CreateNursingInterventionRequest
                    {
                        EncounterId = konteks.EncounterId,
                        InterventionName = "Perawatan luka besar",
                        IsBillable = true
                    }));

            var tidakDitagih = Isi<NursingInterventionResponse>(
                await BuatController(context, perawatUserId)
                    .CreateNursingIntervention(new CreateNursingInterventionRequest
                    {
                        EncounterId = konteks.EncounterId,
                        InterventionName = "Observasi tanda vital",
                        IsBillable = false
                    }));

            Assert.Equal(NursingBillingDispatchStatus.Pending, ditagih.BillingDispatchStatus);
            Assert.Equal(NursingBillingDispatchStatus.NotApplicable, tidakDitagih.BillingDispatchStatus);
        }
    }
}
