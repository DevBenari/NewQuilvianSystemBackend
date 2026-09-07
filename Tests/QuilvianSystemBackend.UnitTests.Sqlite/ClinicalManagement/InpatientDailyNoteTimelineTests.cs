using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-046</c> — catatan harian terbaca menurut waktu
    /// pemeriksaan yang sebenarnya.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Visite pukul 07.40 yang baru sempat diketik pukul 11.00 wajib terbaca pada urutan 07.40.
    /// Memaksa waktu pemeriksaan sama dengan waktu penulisan membuat lini masa perkembangan
    /// pasien menyesatkan — dan lini masa yang menyesatkan adalah dasar keputusan terapi yang
    /// salah.
    /// </para>
    /// <para>
    /// Yang dijaga hanya dua ujung: waktu pemeriksaan tidak boleh berada di masa depan
    /// (<c>VAL-DOK-13</c>), dan tidak boleh mendahului saat pasien masuk kamar
    /// (<c>VAL-DOK-14</c>). Di antara keduanya, pengisian mundur memang <b>wajib</b> boleh.
    /// </para>
    /// </remarks>
    public class InpatientDailyNoteTimelineTests
    {
        private static DoctorConsultationController BuatController(
            ApplicationDbContext c, Guid actorUserId) =>
            new DoctorConsultationController(
                c,
                ControllerTestHarness.BuatLoggerService(actorUserId),
                new ConsultationValidationService(c, new PrescriptionValidationService(c)),
                new ConsultationFinalizationService(
                    c,
                    new ConsultationValidationService(c, new PrescriptionValidationService(c)),
                    new PrescriptionAggregateService(c, new PrescriptionSummaryService(c)),
                    new PrescriptionWorkflowService(c),
                    new ClinicalMilestoneFactProducer(
                        c,
                        new BillingFolioService(c),
                        ControllerTestHarness.BuatLoggerService()),
                    // BE-RWI-038. Finalisasi kini sekaligus mendaftarkan catatan ke mesin
                    // keutuhan rekam medis.
                    new ClinicalDocumentIntegrityService(c)),
                new InpatientClinicalContextService(c))
                .DenganPengguna(actorUserId);

        private static CreateDoctorConsultationRequest Permintaan(
            RawatInapTestData.Konteks k,
            DateTime? waktuPemeriksaan,
            string isi) => new()
            {
                EncounterId = k.EncounterId,
                DoctorId = k.DoctorMasterId,
                ClinicalDateTime = waktuPemeriksaan,
                Subjective = isi
            };

        private static SoapTimelineResponse LiniMasa(IActionResult hasil)
        {
            var isi = Assert.IsType<ApiResponse<SoapTimelineResponse>>(
                ((ObjectResult)hasil).Value);

            return isi.Data!;
        }

        // =====================================================================
        // Kriteria 1 dan 4 — urutan lini masa
        // =====================================================================

        /// <summary>
        /// `BE-RWI-046 AC 1` — catatan yang ditulis pukul 11.00 untuk pemeriksaan pukul 07.40
        /// menempati urutan pukul 07.40, bukan urutan penulisannya.
        /// </summary>
        /// <remarks>
        /// Dibuktikan dengan dua catatan yang urutan penulisannya <b>terbalik</b> dari urutan
        /// pemeriksaannya. Bila lini masa diam-diam kembali memakai waktu penulisan, uji ini
        /// gagal — dan itu memang tujuannya.
        /// </remarks>
        [Fact]
        public async Task CatatanYangDitulisBelakangan_MenempatiUrutanWaktuPemeriksaannya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var controller = BuatController(context, k.DokterUserId);

            var pukul1100 = DateTime.UtcNow.AddHours(-1);
            var pukul0740 = DateTime.UtcNow.AddHours(-4);

            // Ditulis lebih dulu, tetapi pemeriksaannya justru yang paling akhir.
            await controller.CreateConsultation(Permintaan(k, pukul1100, "Pemeriksaan siang"));
            await controller.CreateConsultation(Permintaan(k, pukul0740, "Pemeriksaan pagi"));

            var liniMasa = LiniMasa(await controller.GetSoapTimelineByEpisode(k.EpisodeId));

            Assert.Equal(2, liniMasa.TotalCount);
            Assert.Equal("Pemeriksaan pagi", liniMasa.Items[0].Subjective);
            Assert.Equal("Pemeriksaan siang", liniMasa.Items[1].Subjective);
            Assert.True(liniMasa.Items[0].TimelineDateTime < liniMasa.Items[1].TimelineDateTime);
            Assert.True(liniMasa.Items[0].IsBackdated);
        }

        /// <summary>
        /// `BE-RWI-046 AC 4` — beberapa catatan sepanjang perawatan terbaca sebagai lini masa
        /// yang terurut.
        /// </summary>
        [Fact]
        public async Task BeberapaCatatanSepanjangPerawatan_TerbacaTerurut()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var controller = BuatController(context, k.DokterUserId);

            var urutanPenulisan = new[] { -6, -30, -18 };

            foreach (var jamLalu in urutanPenulisan)
            {
                var hasil = await controller.CreateConsultation(
                    Permintaan(k, DateTime.UtcNow.AddHours(jamLalu), $"Catatan {jamLalu}"));

                Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));
            }

            var liniMasa = LiniMasa(await controller.GetSoapTimelineByEpisode(k.EpisodeId));

            Assert.Equal(3, liniMasa.TotalCount);
            Assert.Equal(
                new[] { "Catatan -30", "Catatan -18", "Catatan -6" },
                liniMasa.Items.Select(x => x.Subjective).ToArray());
        }

        /// <summary>
        /// Catatan yang tidak punya waktu pemeriksaan tetap muncul pada lini masa, memakai waktu
        /// penulisannya sendiri. Tidak ada catatan yang hilang hanya karena kolomnya kosong.
        /// </summary>
        [Fact]
        public async Task CatatanTanpaWaktuPemeriksaan_TetapMunculMemakaiWaktuPenulisan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var controller = BuatController(context, k.DokterUserId);

            await controller.CreateConsultation(Permintaan(k, null, "Tanpa waktu pemeriksaan"));

            var liniMasa = LiniMasa(await controller.GetSoapTimelineByEpisode(k.EpisodeId));

            var butir = Assert.Single(liniMasa.Items);

            Assert.Null(butir.ClinicalDateTime);
            Assert.Equal(butir.ConsultationDateTime, butir.TimelineDateTime);
            Assert.False(butir.IsBackdated);
        }

        /// <summary>
        /// Penyaring waktu membandingkan waktu yang sama dengan yang dipakai mengurutkan.
        /// </summary>
        [Fact]
        public async Task PenyaringWaktu_MemotongMemakaiWaktuPemeriksaan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var controller = BuatController(context, k.DokterUserId);

            await controller.CreateConsultation(
                Permintaan(k, DateTime.UtcNow.AddHours(-30), "Kemarin"));
            await controller.CreateConsultation(
                Permintaan(k, DateTime.UtcNow.AddHours(-2), "Hari ini"));

            var liniMasa = LiniMasa(await controller.GetSoapTimelineByEpisode(
                k.EpisodeId,
                from: DateTime.UtcNow.AddHours(-6)));

            var butir = Assert.Single(liniMasa.Items);
            Assert.Equal("Hari ini", butir.Subjective);
        }

        /// <summary>
        /// Lini masa hanya memuat catatan milik perawatan yang diminta — <c>INV-DOK-12</c>.
        /// Membaca catatan pasien lain adalah cara paling langsung menghasilkan keputusan terapi
        /// yang salah.
        /// </summary>
        [Fact]
        public async Task LiniMasa_TidakMemuatCatatanPerawatanLain()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var pasienA = RawatInapTestData.SiapkanPerawatan(context);
            var pasienB = RawatInapTestData.SiapkanPerawatan(context);

            await BuatController(context, pasienA.DokterUserId)
                .CreateConsultation(Permintaan(pasienA, null, "Milik pasien A"));
            await BuatController(context, pasienB.DokterUserId)
                .CreateConsultation(Permintaan(pasienB, null, "Milik pasien B"));

            var liniMasa = LiniMasa(await BuatController(context, pasienA.DokterUserId)
                .GetSoapTimelineByEpisode(pasienA.EpisodeId));

            var butir = Assert.Single(liniMasa.Items);
            Assert.Equal("Milik pasien A", butir.Subjective);
        }

        /// <summary>Perawatan yang tidak ada dijawab `404`, bukan lini masa kosong.</summary>
        [Fact]
        public async Task LiniMasaPerawatanYangTidakAda_Dijawab404()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var hasil = await BuatController(context, k.DokterUserId)
                .GetSoapTimelineByEpisode(Guid.NewGuid());

            Assert.Equal(404, ControllerTestHarness.KodeStatus(hasil));
        }

        // =====================================================================
        // Kriteria 2 dan 3 — batas waktu
        // =====================================================================

        /// <summary>
        /// `BE-RWI-046 AC 2` — waktu pemeriksaan yang melewati waktu sekarang ditolak `400`.
        /// </summary>
        [Fact]
        public async Task WaktuPemeriksaanDiMasaDepan_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var hasil = await BuatController(context, k.DokterUserId)
                .CreateConsultation(Permintaan(k, DateTime.UtcNow.AddHours(2), "Isi uji"));

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(
                "Waktu pemeriksaan tidak boleh melewati waktu sekarang.",
                ControllerTestHarness.Pesan(hasil));
            Assert.Equal(0, await context.Set<TrxDoctorConsultation>().CountAsync());
        }

        /// <summary>
        /// `BE-RWI-046 AC 3` — waktu pemeriksaan sebelum pasien masuk kamar ditolak `400`.
        /// </summary>
        /// <remarks>
        /// Data uji menempatkan saat masuk kamar dua hari lalu; permintaan memakai tiga hari
        /// lalu, yaitu saat pasien belum berada di kamar mana pun.
        /// </remarks>
        [Fact]
        public async Task WaktuPemeriksaanSebelumMasukKamar_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var masukKamar = await context.Set<InpEpisode>()
                .Where(x => x.Id == k.EpisodeId)
                .Select(x => x.AdmittedAt)
                .SingleAsync();

            Assert.NotNull(masukKamar);

            var hasil = await BuatController(context, k.DokterUserId)
                .CreateConsultation(Permintaan(k, masukKamar!.Value.AddHours(-3), "Isi uji"));

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(
                "Waktu pemeriksaan sebelum pasien masuk kamar. Periksa kembali.",
                ControllerTestHarness.Pesan(hasil));
        }

        /// <summary>
        /// Kendali positif. Pengisian mundur di dalam masa perawatan tetap diterima — itu justru
        /// alasan kolom waktu pemeriksaan ada.
        /// </summary>
        [Fact]
        public async Task WaktuPemeriksaanMundurDiDalamMasaPerawatan_Diterima()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var hasil = await BuatController(context, k.DokterUserId)
                .CreateConsultation(Permintaan(k, DateTime.UtcNow.AddHours(-20), "Isi uji"));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            var tersimpan = await context.Set<TrxDoctorConsultation>()
                .SingleAsync(x => x.EncounterId == k.EncounterId);

            Assert.NotNull(tersimpan.ClinicalDateTime);
            Assert.True(tersimpan.ClinicalDateTime!.Value < tersimpan.ConsultationDateTime);
        }

        // =====================================================================
        // Kriteria 5 — penyelesaian catatan harian
        // =====================================================================

        /// <summary>
        /// `BE-RWI-046 AC 5` — menyelesaikan catatan dengan keempat bagian S/O/A/P kosong
        /// ditolak `400`.
        /// </summary>
        /// <remarks>
        /// <c>VAL-DOK-12</c>. Kalimatnya diambil apa adanya dari validation matrix.
        /// </remarks>
        [Fact]
        public async Task CatatanHarianDenganKeempatBagianKosong_DitolakSaatDiselesaikan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var controller = BuatController(context, k.DokterUserId);

            await controller.CreateConsultation(Permintaan(k, null, string.Empty));

            var catatan = await context.Set<TrxDoctorConsultation>()
                .SingleAsync(x => x.EncounterId == k.EncounterId);

            var hasil = await controller.CompleteConsultation(
                catatan.Id, new FinalizeDoctorConsultationRequest());

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));

            var isi = Assert.IsType<ApiResponse<ConsultationFinalizationValidationResponse>>(
                ((ObjectResult)hasil).Value);

            var masalah = isi.Data!.Sections.SelectMany(x => x.Issues).ToList();

            Assert.Contains(masalah, x => x.Code == "EMPTY_INPATIENT_NOTE");
            Assert.Contains(masalah, x => x.Message == "Catatan masih kosong.");
        }

        /// <summary>
        /// `BE-RWI-046 AC 5` — kendali positif dan inti <c>VAL-DOK-12</c>: <b>cukup satu bagian
        /// terisi</b>. Catatan harian tidak menuntut keempat bagian, dan tidak menuntut
        /// diagnosis utama.
        /// </summary>
        /// <remarks>
        /// Menuntut keempatnya pada setiap catatan harian akan membuat dokter menulis kalimat
        /// kosong demi lolos validasi, dan diagnosis kerja pasien rawat inap hidup pada kajian
        /// medis — bukan diulang setiap hari pada catatan perkembangan.
        /// </remarks>
        [Fact]
        public async Task CatatanHarianDenganSatuBagianTerisi_DapatDiselesaikan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var controller = BuatController(context, k.DokterUserId);

            await controller.CreateConsultation(Permintaan(k, null, "Sesak berkurang"));

            var catatan = await context.Set<TrxDoctorConsultation>()
                .SingleAsync(x => x.EncounterId == k.EncounterId);

            var hasil = await controller.CompleteConsultation(
                catatan.Id, new FinalizeDoctorConsultationRequest());

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            using var pembaca = database.CreateContext();
            var sesudah = await pembaca.Set<TrxDoctorConsultation>().SingleAsync(x => x.Id == catatan.Id);

            Assert.Equal(DoctorConsultationStatus.Completed, sesudah.ConsultationStatus);
        }

        /// <summary>
        /// Regresi poliklinik. Catatan <b>tanpa</b> konteks perawatan tetap tunduk pada aturan
        /// lama: keempat bagian SOAP dan diagnosis utama tetap diwajibkan.
        /// </summary>
        /// <remarks>
        /// <c>RWI-AC-143</c>. Penyaring pelonggaran adalah keberadaan konteks perawatan pada
        /// catatan itu sendiri, bukan tipe kunjungannya, sehingga catatan poliklinik tidak
        /// pernah ikut terlonggarkan.
        /// </remarks>
        [Fact]
        public async Task CatatanTanpaKonteksPerawatan_TetapMenuntutSoapLengkapDanDiagnosis()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokterMaster = RawatInapTestData.BuatDokterMaster(context);
            var aktor = RekamMedisTestData.BuatPengguna(context, "dokter");

            var catatan = new TrxDoctorConsultation
            {
                ConsultationNumber = $"CON-{Guid.NewGuid():N}"[..20],
                EncounterId = konteks.EncounterId,
                PatientId = konteks.PatientId,
                DoctorId = dokterMaster.Id,
                ServiceUnitId = konteks.ServiceUnitId,
                ConsultationDateTime = DateTime.UtcNow,
                ConsultationStatus = DoctorConsultationStatus.InProgress,
                Subjective = "Hanya satu bagian yang terisi",
                StartedByUserId = aktor.Id,
                CreateBy = aktor.Id
            };

            context.Set<TrxDoctorConsultation>().Add(catatan);
            await context.SaveChangesAsync();

            var hasil = await BuatController(context, aktor.Id)
                .CompleteConsultation(catatan.Id, new FinalizeDoctorConsultationRequest());

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));

            var isi = Assert.IsType<ApiResponse<ConsultationFinalizationValidationResponse>>(
                ((ObjectResult)hasil).Value);

            var kode = isi.Data!.Sections.SelectMany(x => x.Issues).Select(x => x.Code).ToList();

            Assert.Contains("MISSING_OBJECTIVE", kode);
            Assert.Contains("MISSING_ASSESSMENT", kode);
            Assert.Contains("MISSING_PLAN", kode);
            Assert.Contains("MISSING_PRIMARY_DIAGNOSIS", kode);
            Assert.DoesNotContain("EMPTY_INPATIENT_NOTE", kode);
        }
    }
}
