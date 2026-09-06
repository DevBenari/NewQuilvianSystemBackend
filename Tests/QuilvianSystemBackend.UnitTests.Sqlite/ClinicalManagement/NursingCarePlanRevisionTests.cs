using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-060</c> — perubahan rencana asuhan menyimpan versi
    /// sebelumnya, lengkap dengan penulis dan waktu aslinya.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kriteria yang paling mudah salah</b> ada di berkas ini: versi lama wajib tetap tercatat
    /// atas nama perawat yang <b>menulisnya</b>, bukan perawat yang mengubahnya. Bila tersalin
    /// atas nama pengubah, rekam medis kehilangan bukti siapa yang menilai pertama kali, dan
    /// kesalahan itu tidak terlihat sama sekali dari layar.
    /// </para>
    /// <para>
    /// Uji ini juga membuktikan hal yang sering tertukar: perubahan rencana asuhan menghasilkan
    /// <b>versi</b>, bukan <b>addendum</b> — <c>RWI-DEC-091</c>, <c>RWI-AC-177</c>.
    /// </para>
    /// </remarks>
    public class NursingCarePlanRevisionTests
    {
        private static T Isi<T>(IActionResult hasil) where T : class
        {
            var objek = Assert.IsAssignableFrom<ObjectResult>(hasil);
            var pembungkus = Assert.IsType<ApiResponse<T>>(objek.Value);
            Assert.NotNull(pembungkus.Data);
            return pembungkus.Data!;
        }

        /// <summary>
        /// Menyiapkan satu perawatan, dua perawat berbeda, rencana asuhan, dan satu butir yang
        /// ditulis perawat pertama.
        /// </summary>
        private static async Task<(RawatInapTestData.Konteks Konteks, Guid SariUserId, Guid SariEmployeeId, Guid DewiUserId, Guid DewiEmployeeId, CarePlanItemResponse Butir)>
            SiapkanButirAsync(ApplicationDbContext context)
        {
            var konteks = RawatInapTestData.SiapkanPerawatan(context);
            var (pegawaiSari, akunSari) = RawatInapTestData.BuatPerawat(context);
            var (pegawaiDewi, akunDewi) = RawatInapTestData.BuatPerawat(context);

            var rencana = Isi<NursingCarePlanResponse>(
                await NursingCarePlanTests.BuatController(context, akunSari.Id)
                    .CreateCarePlan(new CreateNursingCarePlanRequest
                    {
                        EncounterId = konteks.EncounterId
                    }));

            var butir = Isi<CarePlanItemResponse>(
                await NursingCarePlanTests.BuatController(context, akunSari.Id)
                    .CreateCarePlanItem(rencana.Id, new CreateCarePlanItemRequest
                    {
                        ProblemStatement = "Risiko jatuh tinggi",
                        GoalStatement = "Pasien tidak terjatuh selama perawatan"
                    }));

            return (konteks, akunSari.Id, pegawaiSari.Id, akunDewi.Id, pegawaiDewi.Id, butir);
        }

        // =====================================================================
        // Kriteria 1 - versi lama menyimpan penulis dan waktu aslinya
        // =====================================================================

        /// <summary>
        /// `BE-RWI-060 AC 1`, `AC-CAP013-02` — versi lama tercatat atas nama perawat yang
        /// menulisnya, bukan perawat yang memperbaruinya.
        /// </summary>
        [Fact]
        public async Task VersiLama_MenyimpanPenulisAsli_BukanPengubahnya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, _, sariEmployeeId, dewiUserId, dewiEmployeeId, butir) =
                await SiapkanButirAsync(context);

            var waktuAsli = butir.AuthoredAt;

            var diperbarui = await NursingCarePlanTests.BuatController(context, dewiUserId)
                .UpdateCarePlanItem(butir.Id, new UpdateCarePlanItemRequest
                {
                    ProblemStatement = "Risiko jatuh sedang",
                    GoalStatement = "Pasien mampu berpindah dengan pendampingan"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(diperbarui));

            using var pembaca = database.CreateContext();

            var versiLama = await pembaca.Set<CliNursingCarePlanItemRevision>()
                .SingleAsync(x => x.CarePlanItemId == butir.Id);

            // Inti kriteria: penulis dan waktu milik versi yang diarsipkan.
            Assert.Equal(sariEmployeeId, versiLama.OriginalAuthorEmployeeId);
            Assert.Equal(waktuAsli, versiLama.OriginalAuthoredAt);
            Assert.NotEqual(dewiEmployeeId, versiLama.OriginalAuthorEmployeeId);

            Assert.Equal(1, versiLama.VersionNumber);
            Assert.Equal("Risiko jatuh tinggi", versiLama.ProblemStatement);
            Assert.Equal("Pasien tidak terjatuh selama perawatan", versiLama.GoalStatement);

            var butirTerkini = await pembaca.Set<CliNursingCarePlanItem>().SingleAsync(x => x.Id == butir.Id);

            Assert.Equal(2, butirTerkini.VersionNumber);
            Assert.Equal("Risiko jatuh sedang", butirTerkini.ProblemStatement);
            Assert.Equal(dewiEmployeeId, butirTerkini.AuthoredByEmployeeId);
        }

        /// <summary>
        /// `BE-RWI-060 AC 1` — pembaruan berulang menghasilkan riwayat versi berurutan, dan
        /// tidak satu pun isi lama hilang.
        /// </summary>
        [Fact]
        public async Task PembaruanBerulang_MenghasilkanRiwayatVersiBerurutan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, sariUserId, _, _, _, butir) = await SiapkanButirAsync(context);

            foreach (var teks in new[] { "Risiko jatuh sedang", "Risiko jatuh rendah" })
            {
                var hasil = await NursingCarePlanTests.BuatController(context, sariUserId)
                    .UpdateCarePlanItem(butir.Id, new UpdateCarePlanItemRequest
                    {
                        ProblemStatement = teks
                    });

                Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));
            }

            var riwayat = Isi<List<CarePlanItemRevisionResponse>>(
                await NursingCarePlanTests.BuatController(context, sariUserId)
                    .GetCarePlanItemRevisions(butir.Id));

            Assert.Equal(2, riwayat.Count);
            Assert.Equal(new[] { 1, 2 }, riwayat.Select(x => x.VersionNumber));
            Assert.Equal(
                new[] { "Risiko jatuh tinggi", "Risiko jatuh sedang" },
                riwayat.Select(x => x.ProblemStatement));
        }

        /// <summary>
        /// Evaluasi kedua tidak menimpa evaluasi pertama: keadaan lama ikut diarsipkan.
        /// </summary>
        [Fact]
        public async Task EvaluasiKedua_TidakMenimpaEvaluasiPertama()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, sariUserId, _, _, _, butir) = await SiapkanButirAsync(context);

            foreach (var catatan in new[] { "Pasien masih memerlukan pendampingan", "Pasien berjalan mandiri" })
            {
                var hasil = await NursingCarePlanTests.BuatController(context, sariUserId)
                    .EvaluateCarePlanItem(butir.Id, new EvaluateCarePlanItemRequest
                    {
                        EvaluationNote = catatan
                    });

                Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));
            }

            using var pembaca = database.CreateContext();

            var versiLama = await pembaca.Set<CliNursingCarePlanItemRevision>()
                .SingleAsync(x => x.CarePlanItemId == butir.Id);

            Assert.Equal("Pasien masih memerlukan pendampingan", versiLama.EvaluationNote);

            var butirTerkini = await pembaca.Set<CliNursingCarePlanItem>().SingleAsync(x => x.Id == butir.Id);

            Assert.Equal("Pasien berjalan mandiri", butirTerkini.EvaluationNote);
        }

        // =====================================================================
        // Kriteria 2 - versi, bukan addendum
        // =====================================================================

        /// <summary>
        /// `BE-RWI-060 AC 2`, `RWI-AC-177` — memperbarui butir menghasilkan versi baru dan
        /// <b>nol</b> baris addendum. Rencana asuhan tidak diseret ke mesin koreksi.
        /// </summary>
        [Fact]
        public async Task PembaruanButir_TidakMembentukSatuPunAddendum()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, _, _, dewiUserId, _, butir) = await SiapkanButirAsync(context);

            await NursingCarePlanTests.BuatController(context, dewiUserId)
                .UpdateCarePlanItem(butir.Id, new UpdateCarePlanItemRequest
                {
                    ProblemStatement = "Risiko jatuh sedang"
                });

            using var pembaca = database.CreateContext();

            Assert.Equal(1, await pembaca.Set<CliNursingCarePlanItemRevision>()
                .CountAsync(x => x.CarePlanItemId == butir.Id));

            // Nol addendum, dan nol baris keutuhan dokumen. Rencana asuhan memang bukan dokumen
            // yang ditandatangani lalu dikoreksi; ia dokumen hidup yang berkembang.
            Assert.Equal(0, await pembaca.Set<MrcClinicalNoteAddendum>().CountAsync());
        }

        // =====================================================================
        // Kriteria 3 - menutup butir tidak menghapus jejak
        // =====================================================================

        /// <summary>
        /// `BE-RWI-060 AC 3` — menutup butir tidak menghapus evaluasi maupun riwayat versinya.
        /// </summary>
        [Fact]
        public async Task MenutupButir_TidakMenghapusEvaluasiDanRiwayatVersi()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, sariUserId, _, _, _, butir) = await SiapkanButirAsync(context);

            await NursingCarePlanTests.BuatController(context, sariUserId)
                .UpdateCarePlanItem(butir.Id, new UpdateCarePlanItemRequest
                {
                    ProblemStatement = "Risiko jatuh sedang"
                });

            await NursingCarePlanTests.BuatController(context, sariUserId)
                .EvaluateCarePlanItem(butir.Id, new EvaluateCarePlanItemRequest
                {
                    EvaluationNote = "Pasien berjalan mandiri tanpa terjatuh selama 3 hari"
                });

            var ditutup = await NursingCarePlanTests.BuatController(context, sariUserId)
                .CloseCarePlanItem(butir.Id, new CloseCarePlanItemRequest
                {
                    TargetStatus = NursingCarePlanItemStatus.Resolved,
                    Reason = "Masalah teratasi"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(ditutup));

            using var pembaca = database.CreateContext();

            var tersimpan = await pembaca.Set<CliNursingCarePlanItem>().SingleAsync(x => x.Id == butir.Id);

            Assert.Equal(NursingCarePlanItemStatus.Resolved, tersimpan.ItemStatus);
            Assert.Equal("Pasien berjalan mandiri tanpa terjatuh selama 3 hari", tersimpan.EvaluationNote);

            Assert.Equal(1, await pembaca.Set<CliNursingCarePlanItemRevision>()
                .CountAsync(x => x.CarePlanItemId == butir.Id));
        }

        // =====================================================================
        // Kriteria 4 - perawatan tertutup hanya-baca
        // =====================================================================

        /// <summary>
        /// `BE-RWI-060 AC 4`, `AC-CAP013-03` — setelah perawatan ditutup, riwayat asuhan tetap
        /// terbaca lewat <c>GET</c>, sementara setiap penulisan dijawab <c>422</c>.
        /// </summary>
        [Fact]
        public async Task PerawatanTertutup_TetapTerbacaTetapiMenolakPenulisan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, sariUserId, _, _, _, butir) = await SiapkanButirAsync(context);

            await NursingCarePlanTests.BuatController(context, sariUserId)
                .UpdateCarePlanItem(butir.Id, new UpdateCarePlanItemRequest
                {
                    ProblemStatement = "Risiko jatuh sedang"
                });

            var episode = await context.Set<InpEpisode>().SingleAsync(x => x.Id == konteks.EpisodeId);
            episode.EpisodeStatus = InpEpisodeStatus.Closed;
            await context.SaveChangesAsync();

            // Pembacaan tetap berjalan penuh.
            var rencanaDibaca = await NursingCarePlanTests.BuatController(context, sariUserId)
                .GetByEpisode(konteks.EpisodeId);
            Assert.Equal(200, ControllerTestHarness.KodeStatus(rencanaDibaca));

            var riwayat = Isi<List<CarePlanItemRevisionResponse>>(
                await NursingCarePlanTests.BuatController(context, sariUserId)
                    .GetCarePlanItemRevisions(butir.Id));
            Assert.Single(riwayat);

            // Setiap penulisan ditolak 422.
            var rencanaBaru = await NursingCarePlanTests.BuatController(context, sariUserId)
                .CreateCarePlan(new CreateNursingCarePlanRequest { EncounterId = konteks.EncounterId });
            Assert.Equal(422, ControllerTestHarness.KodeStatus(rencanaBaru));

            var butirBaru = await NursingCarePlanTests.BuatController(context, sariUserId)
                .CreateCarePlanItem(butir.CarePlanId, new CreateCarePlanItemRequest
                {
                    ProblemStatement = "Masalah baru setelah pasien pulang"
                });
            Assert.Equal(422, ControllerTestHarness.KodeStatus(butirBaru));

            var pembaruan = await NursingCarePlanTests.BuatController(context, sariUserId)
                .UpdateCarePlanItem(butir.Id, new UpdateCarePlanItemRequest
                {
                    ProblemStatement = "Perubahan setelah pasien pulang"
                });
            Assert.Equal(422, ControllerTestHarness.KodeStatus(pembaruan));

            var evaluasi = await NursingCarePlanTests.BuatController(context, sariUserId)
                .EvaluateCarePlanItem(butir.Id, new EvaluateCarePlanItemRequest
                {
                    EvaluationNote = "Evaluasi setelah pasien pulang"
                });
            Assert.Equal(422, ControllerTestHarness.KodeStatus(evaluasi));
        }
    }
}
