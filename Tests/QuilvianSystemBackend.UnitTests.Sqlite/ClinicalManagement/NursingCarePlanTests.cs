using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-059</c> — rencana asuhan keperawatan punya tempat
    /// menyimpan masalah, tujuan, rencana tindakan, dan evaluasinya.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Yang paling menentukan di berkas ini</b> adalah <c>VAL-KEP-16</c>: satu masalah
    /// keperawatan tidak boleh dinyatakan teratasi tanpa satu pun catatan evaluasi. Bila
    /// penjaganya bocor, rekam medis memuat pernyataan "masalah teratasi" yang tidak dapat
    /// menunjukkan dasarnya sama sekali.
    /// </para>
    /// <para>
    /// Uji ini memanggil controller apa adanya, bukan lewat HTTP; keterbatasannya dijelaskan
    /// pada <c>ControllerTestHarness</c>.
    /// </para>
    /// </remarks>
    public class NursingCarePlanTests
    {
        internal static NursingCarePlanController BuatController(
            ApplicationDbContext context,
            Guid actorUserId)
        {
            var actorService = new NursingActorService(context);
            var contextService = new InpatientClinicalContextService(context);
            var carePlanService = new NursingCarePlanService(context, contextService, actorService);

            var controller = new NursingCarePlanController(
                ControllerTestHarness.BuatLoggerService(actorUserId),
                carePlanService);

            return controller.DenganPengguna(actorUserId);
        }

        private static T Isi<T>(IActionResult hasil) where T : class
        {
            var objek = Assert.IsAssignableFrom<ObjectResult>(hasil);
            var pembungkus = Assert.IsType<ApiResponse<T>>(objek.Value);
            Assert.NotNull(pembungkus.Data);
            return pembungkus.Data!;
        }

        /// <summary>
        /// Menyiapkan satu perawatan beserta perawat yang tertaut ke akun penggunanya, lalu
        /// membuka rencana asuhannya.
        /// </summary>
        private static async Task<(RawatInapTestData.Konteks Konteks, Guid PerawatUserId, NursingCarePlanResponse Rencana)>
            SiapkanRencanaAsync(ApplicationDbContext context)
        {
            var konteks = RawatInapTestData.SiapkanPerawatan(context);
            var (_, akunPerawat) = RawatInapTestData.BuatPerawat(context);

            var hasil = await BuatController(context, akunPerawat.Id)
                .CreateCarePlan(new CreateNursingCarePlanRequest
                {
                    EncounterId = konteks.EncounterId,
                    InpEpisodeId = konteks.EpisodeId
                });

            Assert.Equal(201, ControllerTestHarness.KodeStatus(hasil));

            return (konteks, akunPerawat.Id, Isi<NursingCarePlanResponse>(hasil));
        }

        // =====================================================================
        // Kriteria 1 - satu perawatan tepat satu rencana asuhan
        // =====================================================================

        /// <summary>
        /// `BE-RWI-059 AC 1` — perawatan yang sudah memiliki rencana asuhan menolak rencana
        /// kedua dengan <c>409</c>, dan tetap hanya ada satu baris di database.
        /// </summary>
        [Fact]
        public async Task RencanaKedua_PadaPerawatanYangSama_Ditolak409()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, perawatUserId, _) = await SiapkanRencanaAsync(context);

            var kedua = await BuatController(context, perawatUserId)
                .CreateCarePlan(new CreateNursingCarePlanRequest
                {
                    EncounterId = konteks.EncounterId
                });

            Assert.Equal(409, ControllerTestHarness.KodeStatus(kedua));

            using var pembaca = database.CreateContext();

            var jumlah = await pembaca.Set<CliNursingCarePlan>()
                .CountAsync(x => x.InpEpisodeId == konteks.EpisodeId && !x.IsDelete);

            Assert.Equal(1, jumlah);
        }

        /// <summary>
        /// `BE-RWI-059 AC 1` — satu rencana asuhan menampung butir masalah sebanyak yang
        /// dibutuhkan.
        /// </summary>
        [Fact]
        public async Task SatuRencana_MenampungBanyakButirMasalah()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, perawatUserId, rencana) = await SiapkanRencanaAsync(context);

            foreach (var masalah in new[] { "Nyeri akut", "Risiko jatuh", "Defisit nutrisi" })
            {
                var hasil = await BuatController(context, perawatUserId)
                    .CreateCarePlanItem(rencana.Id, new CreateCarePlanItemRequest
                    {
                        ProblemStatement = masalah
                    });

                Assert.Equal(201, ControllerTestHarness.KodeStatus(hasil));
            }

            var dibaca = await BuatController(context, perawatUserId).GetByEpisode(konteks.EpisodeId);
            var isi = Isi<NursingCarePlanResponse>(dibaca);

            Assert.Equal(3, isi.TotalItemCount);
            Assert.Equal(3, isi.ActiveItemCount);
            Assert.Equal(
                new[] { "Nyeri akut", "Risiko jatuh", "Defisit nutrisi" },
                isi.Items.Select(x => x.ProblemStatement));
        }

        // =====================================================================
        // Kriteria 2 - butir memuat masalah, tujuan, rencana tindakan, dan evaluasi
        // =====================================================================

        /// <summary>
        /// `BE-RWI-059 AC 2` — keempat isian butir benar-benar tersimpan dan terbaca kembali.
        /// </summary>
        [Fact]
        public async Task Butir_MenyimpanMasalahTujuanRencanaDanEvaluasi()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, perawatUserId, rencana) = await SiapkanRencanaAsync(context);

            var dibuat = await BuatController(context, perawatUserId)
                .CreateCarePlanItem(rencana.Id, new CreateCarePlanItemRequest
                {
                    ProblemStatement = "Nyeri akut berhubungan dengan luka operasi",
                    GoalStatement = "Nyeri berkurang menjadi skala 3 dalam 2 hari",
                    PlannedIntervention = "Manajemen nyeri, kompres hangat, evaluasi tiap 4 jam"
                });

            var butir = Isi<CarePlanItemResponse>(dibuat);

            var dievaluasi = await BuatController(context, perawatUserId)
                .EvaluateCarePlanItem(butir.Id, new EvaluateCarePlanItemRequest
                {
                    EvaluationNote = "Skala nyeri turun dari 7 menjadi 3 pada hari kedua"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dievaluasi));

            using var pembaca = database.CreateContext();

            var tersimpan = await pembaca.Set<CliNursingCarePlanItem>().SingleAsync(x => x.Id == butir.Id);

            Assert.Equal("Nyeri akut berhubungan dengan luka operasi", tersimpan.ProblemStatement);
            Assert.Equal("Nyeri berkurang menjadi skala 3 dalam 2 hari", tersimpan.GoalStatement);
            Assert.Equal("Manajemen nyeri, kompres hangat, evaluasi tiap 4 jam", tersimpan.PlannedIntervention);
            Assert.Equal("Skala nyeri turun dari 7 menjadi 3 pada hari kedua", tersimpan.EvaluationNote);
            Assert.NotNull(tersimpan.LastEvaluatedAt);
        }

        // =====================================================================
        // Kriteria 3 - VAL-KEP-16, tercapai menuntut evaluasi
        // =====================================================================

        /// <summary>
        /// `BE-RWI-059 AC 3` — menutup butir sebagai teratasi tanpa satu pun evaluasi ditolak
        /// <c>400</c>, dan butirnya <b>tetap</b> <c>Active</c> di database.
        /// </summary>
        [Fact]
        public async Task ButirTanpaEvaluasi_TidakDapatDinyatakanTercapai()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, perawatUserId, rencana) = await SiapkanRencanaAsync(context);

            var dibuat = await BuatController(context, perawatUserId)
                .CreateCarePlanItem(rencana.Id, new CreateCarePlanItemRequest
                {
                    ProblemStatement = "Risiko jatuh tinggi"
                });

            var butir = Isi<CarePlanItemResponse>(dibuat);

            var ditutup = await BuatController(context, perawatUserId)
                .CloseCarePlanItem(butir.Id, new CloseCarePlanItemRequest
                {
                    TargetStatus = NursingCarePlanItemStatus.Resolved,
                    Reason = "Pasien sudah mandiri"
                });

            Assert.Equal(400, ControllerTestHarness.KodeStatus(ditutup));
            Assert.Equal(NursingCarePlanService.PenolakanTanpaEvaluasi, ControllerTestHarness.Pesan(ditutup));

            using var pembaca = database.CreateContext();

            var tersimpan = await pembaca.Set<CliNursingCarePlanItem>().SingleAsync(x => x.Id == butir.Id);

            Assert.Equal(NursingCarePlanItemStatus.Active, tersimpan.ItemStatus);
            Assert.Null(tersimpan.ResolvedAt);
            Assert.Null(tersimpan.CloseReason);
        }

        /// <summary>
        /// `BE-RWI-059 AC 3` — setelah evaluasi tercatat, penutupan sebagai teratasi diterima.
        /// </summary>
        [Fact]
        public async Task ButirDenganEvaluasi_DapatDinyatakanTercapai()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, perawatUserId, rencana) = await SiapkanRencanaAsync(context);

            var butir = Isi<CarePlanItemResponse>(await BuatController(context, perawatUserId)
                .CreateCarePlanItem(rencana.Id, new CreateCarePlanItemRequest
                {
                    ProblemStatement = "Risiko jatuh tinggi"
                }));

            await BuatController(context, perawatUserId)
                .EvaluateCarePlanItem(butir.Id, new EvaluateCarePlanItemRequest
                {
                    EvaluationNote = "Pasien mampu berjalan mandiri tanpa terjatuh selama 3 hari"
                });

            var ditutup = await BuatController(context, perawatUserId)
                .CloseCarePlanItem(butir.Id, new CloseCarePlanItemRequest
                {
                    TargetStatus = NursingCarePlanItemStatus.Resolved,
                    Reason = "Masalah teratasi"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(ditutup));

            using var pembaca = database.CreateContext();

            var tersimpan = await pembaca.Set<CliNursingCarePlanItem>().SingleAsync(x => x.Id == butir.Id);

            Assert.Equal(NursingCarePlanItemStatus.Resolved, tersimpan.ItemStatus);
            Assert.NotNull(tersimpan.ResolvedAt);
            Assert.Equal("Masalah teratasi", tersimpan.CloseReason);
        }

        /// <summary>
        /// `BE-RWI-059 AC 3` — menghentikan butir karena tidak lagi relevan tidak menuntut
        /// evaluasi, tetapi tetap menuntut alasan.
        /// </summary>
        [Fact]
        public async Task ButirTanpaEvaluasi_MasihDapatDihentikanDenganAlasan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, perawatUserId, rencana) = await SiapkanRencanaAsync(context);

            var butir = Isi<CarePlanItemResponse>(await BuatController(context, perawatUserId)
                .CreateCarePlanItem(rencana.Id, new CreateCarePlanItemRequest
                {
                    ProblemStatement = "Defisit perawatan diri"
                }));

            var ditutup = await BuatController(context, perawatUserId)
                .CloseCarePlanItem(butir.Id, new CloseCarePlanItemRequest
                {
                    TargetStatus = NursingCarePlanItemStatus.Discontinued,
                    Reason = "Rencana asuhan diganti mengikuti hasil pengkajian ulang"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(ditutup));

            using var pembaca = database.CreateContext();

            var tersimpan = await pembaca.Set<CliNursingCarePlanItem>().SingleAsync(x => x.Id == butir.Id);

            Assert.Equal(NursingCarePlanItemStatus.Discontinued, tersimpan.ItemStatus);
        }

        // =====================================================================
        // Kriteria 4 - butir dikaitkan ke temuan pengkajian asalnya
        // =====================================================================

        /// <summary>
        /// `BE-RWI-059 AC 4`, `AC-CAP013-01` — butir yang menunjuk pengkajian asal menyimpan
        /// rujukannya, dan pembacaan mengembalikan nomor pengkajian itu.
        /// </summary>
        [Fact]
        public async Task Butir_DapatMerujukPengkajianAsal()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, perawatUserId, rencana) = await SiapkanRencanaAsync(context);

            var pengkajian = new TrxPatientAssessment
            {
                AssessmentNumber = "ASS-UJI-001",
                PatientId = konteks.PatientId,
                EncounterId = konteks.EncounterId,
                ServiceUnitId = konteks.ServiceUnitId,
                InpEpisodeId = konteks.EpisodeId,
                AssessmentType = PatientAssessmentType.Initial,
                AssessmentStatus = PatientAssessmentStatus.Completed
            };
            context.Set<TrxPatientAssessment>().Add(pengkajian);
            await context.SaveChangesAsync();

            var dibuat = await BuatController(context, perawatUserId)
                .CreateCarePlanItem(rencana.Id, new CreateCarePlanItemRequest
                {
                    ProblemStatement = "Risiko jatuh tinggi",
                    SourceAssessmentId = pengkajian.Id
                });

            var butir = Isi<CarePlanItemResponse>(dibuat);

            Assert.Equal(pengkajian.Id, butir.SourceAssessmentId);

            var dibaca = Isi<NursingCarePlanResponse>(
                await BuatController(context, perawatUserId).GetByEpisode(konteks.EpisodeId));

            var barisButir = Assert.Single(dibaca.Items);
            Assert.Equal("ASS-UJI-001", barisButir.SourceAssessmentNumber);
        }

        /// <summary>
        /// `BE-RWI-059 AC 4` — penjaga salah pasien: butir tidak boleh menunjuk pengkajian milik
        /// perawatan lain.
        /// </summary>
        [Fact]
        public async Task Butir_MenolakPengkajianMilikPerawatanLain()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, perawatUserId, rencana) = await SiapkanRencanaAsync(context);

            var perawatanLain = RawatInapTestData.SiapkanPerawatan(context);

            var pengkajianLain = new TrxPatientAssessment
            {
                AssessmentNumber = "ASS-UJI-002",
                PatientId = perawatanLain.PatientId,
                EncounterId = perawatanLain.EncounterId,
                ServiceUnitId = perawatanLain.ServiceUnitId,
                InpEpisodeId = perawatanLain.EpisodeId,
                AssessmentType = PatientAssessmentType.Initial,
                AssessmentStatus = PatientAssessmentStatus.Completed
            };
            context.Set<TrxPatientAssessment>().Add(pengkajianLain);
            await context.SaveChangesAsync();

            var hasil = await BuatController(context, perawatUserId)
                .CreateCarePlanItem(rencana.Id, new CreateCarePlanItemRequest
                {
                    ProblemStatement = "Nyeri akut",
                    SourceAssessmentId = pengkajianLain.Id
                });

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
        }

        // =====================================================================
        // Kriteria 5 - rencana asuhan hanya untuk perawatan Admitted
        // =====================================================================

        /// <summary>
        /// `BE-RWI-059 AC 5` — perawatan yang masih <c>Draft</c> menolak pembukaan rencana
        /// asuhan dengan <c>422</c>.
        /// </summary>
        [Fact]
        public async Task RencanaAsuhan_PadaPerawatanDraft_Ditolak422()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RawatInapTestData.SiapkanPerawatan(context, InpEpisodeStatus.Draft);
            var (_, akunPerawat) = RawatInapTestData.BuatPerawat(context);

            var hasil = await BuatController(context, akunPerawat.Id)
                .CreateCarePlan(new CreateNursingCarePlanRequest
                {
                    EncounterId = konteks.EncounterId
                });

            Assert.Equal(422, ControllerTestHarness.KodeStatus(hasil));
        }

        /// <summary>
        /// `BE-RWI-059 AC 5` — perawatan yang sudah <c>Closed</c> menolak pembukaan rencana
        /// asuhan dengan <c>422</c>.
        /// </summary>
        [Fact]
        public async Task RencanaAsuhan_PadaPerawatanTertutup_Ditolak422()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RawatInapTestData.SiapkanPerawatan(context, InpEpisodeStatus.Closed);
            var (_, akunPerawat) = RawatInapTestData.BuatPerawat(context);

            var hasil = await BuatController(context, akunPerawat.Id)
                .CreateCarePlan(new CreateNursingCarePlanRequest
                {
                    EncounterId = konteks.EncounterId
                });

            Assert.Equal(422, ControllerTestHarness.KodeStatus(hasil));
        }

        // =====================================================================
        // Penjaga tambahan yang menjadi syarat berfungsinya slice ini
        // =====================================================================

        /// <summary>
        /// Akun yang belum tertaut ke data pegawai ditolak <c>400</c> beserta arahan
        /// perbaikannya — dokumentasi keperawatan wajib menyebut siapa perawatnya.
        /// </summary>
        [Fact]
        public async Task AkunTanpaPegawai_TidakDapatMembukaRencanaAsuhan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RawatInapTestData.SiapkanPerawatan(context);
            var akunTanpaPegawai = RekamMedisTestData.BuatPengguna(context, "tanpa.pegawai");

            var hasil = await BuatController(context, akunTanpaPegawai.Id)
                .CreateCarePlan(new CreateNursingCarePlanRequest
                {
                    EncounterId = konteks.EncounterId
                });

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(NursingActorService.PenolakanTanpaPegawai, ControllerTestHarness.Pesan(hasil));
        }

        /// <summary>
        /// Perawatan yang belum punya rencana asuhan dijawab <c>404</c>, bukan rencana kosong
        /// yang seolah sudah ada.
        /// </summary>
        [Fact]
        public async Task PerawatanTanpaRencana_Dijawab404()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RawatInapTestData.SiapkanPerawatan(context);
            var (_, akunPerawat) = RawatInapTestData.BuatPerawat(context);

            var hasil = await BuatController(context, akunPerawat.Id).GetByEpisode(konteks.EpisodeId);

            Assert.Equal(404, ControllerTestHarness.KodeStatus(hasil));
        }
    }
}
