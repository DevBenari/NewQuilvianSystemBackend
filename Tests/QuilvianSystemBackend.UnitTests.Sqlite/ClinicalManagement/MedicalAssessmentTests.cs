using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-045</c> — kajian medis awal tersimpan terpisah dari
    /// catatan harian.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Kajian medis dan pengkajian keperawatan berbagi satu tabel — keputusan struktur pada
    /// <c>02-backend-architecture.md</c> bagian 4.2, jalan A. Akibatnya mesin hak akses hanya
    /// melihat <b>satu</b> sumber daya untuk dua jenis dokumen, sehingga pembedaannya wajib
    /// dijaga aturan bisnis. Berkas ini adalah buktinya.
    /// </para>
    /// <para>
    /// <b>Satu acceptance criteria tidak dapat dibuktikan di sini.</b> Kriteria 4 menuntut
    /// penolakan kajian medis yang diselesaikan tanpa diagnosis. <c>TrxPatientAssessment</c>
    /// tidak memiliki kolom diagnosis kerja, pemeriksaan fisik, maupun rencana terapi, dan
    /// <c>data/data-dictionary.md</c> bagian 3 menyatakan sub-modul ini menambahkan <b>nol</b>
    /// kolom pada tabel itu. Yang diuji di bawah adalah mekanismenya beserta bagian yang memang
    /// tersedia; sisanya tercatat sebagai blocker pada laporan task.
    /// </para>
    /// </remarks>
    public class MedicalAssessmentTests
    {
        private static PatientAssessmentController BuatControllerKajian(
            ApplicationDbContext c, Guid actorUserId) =>
            new PatientAssessmentController(
                c,
                ControllerTestHarness.BuatLoggerService(actorUserId),
                new InpatientClinicalContextService(c),
                new ClinicalDocumentIntegrityService(c))
                .DenganPengguna(actorUserId);

        private static DoctorConsultationController BuatControllerCatatan(
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
                        ControllerTestHarness.BuatLoggerService())),
                new InpatientClinicalContextService(c))
                .DenganPengguna(actorUserId);

        private static CreatePatientAssessmentRequest PermintaanKajianMedis(
            RawatInapTestData.Konteks k,
            PatientAssessmentType jenis = PatientAssessmentType.MedicalInitial) => new()
            {
                EncounterId = k.EncounterId,
                InpEpisodeId = k.EpisodeId,
                AssessmentType = jenis,
                ChiefComplaint = "Sesak napas sejak dua hari",
                CurrentIllnessHistory = "Sesak memberat saat berbaring, disertai batuk berdahak"
            };

        private static async Task<TrxPatientAssessment> BuatKajianMedisAsync(
            ApplicationDbContext context,
            RawatInapTestData.Konteks k)
        {
            var hasil = await BuatControllerKajian(context, k.DokterUserId)
                .CreateAssessment(PermintaanKajianMedis(k));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            return await context.Set<TrxPatientAssessment>()
                .SingleAsync(x => x.AssessmentType == PatientAssessmentType.MedicalInitial);
        }

        // =====================================================================
        // Kriteria 1 dan 2 — dua record, dua mesin status
        // =====================================================================

        /// <summary>
        /// `BE-RWI-045 AC 1` — kajian medis dan catatan harian tersimpan sebagai record berbeda
        /// pada tabel berbeda, masing-masing dengan mesin statusnya sendiri.
        /// </summary>
        /// <remarks>
        /// <c>AC-CAP022-02</c>. Yang dibuktikan bukan sekadar dua baris, melainkan bahwa
        /// menyelesaikan salah satunya <b>tidak</b> menggerakkan status yang lain.
        /// </remarks>
        [Fact]
        public async Task KajianMedisDanCatatanHarian_DuaRecordDenganStatusYangBerdiriSendiri()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var kajian = await BuatKajianMedisAsync(context, k);

            var catatanHasil = await BuatControllerCatatan(context, k.DokterUserId)
                .CreateConsultation(new CreateDoctorConsultationRequest
                {
                    EncounterId = k.EncounterId,
                    DoctorId = k.DoctorMasterId,
                    Subjective = "Sesak berkurang"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(catatanHasil));

            var catatan = await context.Set<TrxDoctorConsultation>()
                .SingleAsync(x => x.EncounterId == k.EncounterId);

            Assert.NotEqual(kajian.Id, catatan.Id);
            Assert.Equal(PatientAssessmentStatus.InProgress, kajian.AssessmentStatus);
            Assert.Equal(DoctorConsultationStatus.InProgress, catatan.ConsultationStatus);

            // Menyelesaikan kajian tidak menyentuh status catatan.
            var selesai = await BuatControllerKajian(context, k.DokterUserId)
                .CompleteAssessment(kajian.Id, new CompletePatientAssessmentRequest());

            Assert.Equal(200, ControllerTestHarness.KodeStatus(selesai));

            using var pembaca = database.CreateContext();

            var kajianSesudah = await pembaca.Set<TrxPatientAssessment>().SingleAsync(x => x.Id == kajian.Id);
            var catatanSesudah = await pembaca.Set<TrxDoctorConsultation>().SingleAsync(x => x.Id == catatan.Id);

            Assert.Equal(PatientAssessmentStatus.Completed, kajianSesudah.AssessmentStatus);
            Assert.Equal(DoctorConsultationStatus.InProgress, catatanSesudah.ConsultationStatus);
        }

        /// <summary>
        /// `BE-RWI-045 AC 2` — menulis tiga catatan harian tidak mengubah satu huruf pun isi
        /// kajian medis.
        /// </summary>
        /// <remarks>
        /// Inilah inti pemisahan itu: pada bentuk lama, satu catatan per kunjungan berarti
        /// pemeriksaan menyeluruh pertama akan tertimpa catatan harian berikutnya.
        /// </remarks>
        [Fact]
        public async Task TigaCatatanHarian_TidakMengubahIsiKajianMedis()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var kajian = await BuatKajianMedisAsync(context, k);

            var sebelum = new
            {
                kajian.ChiefComplaint,
                kajian.CurrentIllnessHistory,
                kajian.AssessmentStatus,
                kajian.AssessmentDateTime
            };

            for (var hari = 1; hari <= 3; hari++)
            {
                var hasil = await BuatControllerCatatan(context, k.DokterUserId)
                    .CreateConsultation(new CreateDoctorConsultationRequest
                    {
                        EncounterId = k.EncounterId,
                        DoctorId = k.DoctorMasterId,
                        Subjective = $"Catatan hari ke-{hari}",
                        Objective = $"Pemeriksaan hari ke-{hari}"
                    });

                Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));
            }

            using var pembaca = database.CreateContext();

            var sesudah = await pembaca.Set<TrxPatientAssessment>().SingleAsync(x => x.Id == kajian.Id);

            Assert.Equal(sebelum.ChiefComplaint, sesudah.ChiefComplaint);
            Assert.Equal(sebelum.CurrentIllnessHistory, sesudah.CurrentIllnessHistory);
            Assert.Equal(sebelum.AssessmentStatus, sesudah.AssessmentStatus);
            Assert.Equal(sebelum.AssessmentDateTime, sesudah.AssessmentDateTime);

            Assert.Equal(3, await pembaca.Set<TrxDoctorConsultation>()
                .CountAsync(x => x.EncounterId == k.EncounterId));
        }

        // =====================================================================
        // Kriteria 3 — satu kajian medis awal per perawatan
        // =====================================================================

        /// <summary>
        /// `BE-RWI-045 AC 3` — satu perawatan memiliki paling banyak satu kajian medis awal yang
        /// berlaku.
        /// </summary>
        [Fact]
        public async Task KajianMedisAwalKedua_DitolakPadaPerawatanYangSama()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            await BuatKajianMedisAsync(context, k);

            var kedua = await BuatControllerKajian(context, k.DokterUserId)
                .CreateAssessment(PermintaanKajianMedis(k));

            Assert.Equal(400, ControllerTestHarness.KodeStatus(kedua));
            Assert.Contains("sudah memiliki kajian medis awal", ControllerTestHarness.Pesan(kedua));

            Assert.Equal(1, await context.Set<TrxPatientAssessment>()
                .CountAsync(x => x.AssessmentType == PatientAssessmentType.MedicalInitial));
        }

        /// <summary>
        /// `BE-RWI-045 AC 3` — batasnya berlaku juga setelah kajian pertama diselesaikan, dan
        /// <b>kajian medis ulang</b> tetap boleh dibuat.
        /// </summary>
        [Fact]
        public async Task KajianMedisUlang_TetapBolehSetelahKajianAwalSelesai()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var kajian = await BuatKajianMedisAsync(context, k);

            await BuatControllerKajian(context, k.DokterUserId)
                .CompleteAssessment(kajian.Id, new CompletePatientAssessmentRequest());

            var awalKedua = await BuatControllerKajian(context, k.DokterUserId)
                .CreateAssessment(PermintaanKajianMedis(k));

            Assert.Equal(400, ControllerTestHarness.KodeStatus(awalKedua));

            var ulang = await BuatControllerKajian(context, k.DokterUserId)
                .CreateAssessment(PermintaanKajianMedis(k, PatientAssessmentType.MedicalReassessment));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(ulang));
        }

        /// <summary>
        /// `BE-RWI-045 AC 3` — batasnya dihitung <b>per perawatan</b>. Perawatan lain tidak ikut
        /// terhalang.
        /// </summary>
        [Fact]
        public async Task PerawatanLain_TidakIkutTerhalang()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var pasienA = RawatInapTestData.SiapkanPerawatan(context);
            var pasienB = RawatInapTestData.SiapkanPerawatan(context);

            await BuatKajianMedisAsync(context, pasienA);

            var hasil = await BuatControllerKajian(context, pasienB.DokterUserId)
                .CreateAssessment(PermintaanKajianMedis(pasienB));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));
        }

        /// <summary>
        /// `BE-RWI-045 AC 1` — pengkajian keperawatan yang masih dikerjakan <b>tidak</b> menutup
        /// pembuatan kajian medis, dan sebaliknya. Dua profesi, satu tabel, dua mesin status.
        /// </summary>
        [Fact]
        public async Task PengkajianKeperawatanYangMasihDikerjakan_TidakMenutupKajianMedis()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            context.Set<TrxPatientAssessment>().Add(new TrxPatientAssessment
            {
                AssessmentNumber = $"ASM-{Guid.NewGuid():N}"[..20],
                EncounterId = k.EncounterId,
                PatientId = k.PatientId,
                ServiceUnitId = k.ServiceUnitId,
                InpEpisodeId = k.EpisodeId,
                AssessmentType = PatientAssessmentType.Initial,
                AssessmentStatus = PatientAssessmentStatus.InProgress
            });
            await context.SaveChangesAsync();

            var hasil = await BuatControllerKajian(context, k.DokterUserId)
                .CreateAssessment(PermintaanKajianMedis(k));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(2, await context.Set<TrxPatientAssessment>()
                .CountAsync(x => x.EncounterId == k.EncounterId));
        }

        // =====================================================================
        // Kriteria 4 — penyelesaian yang belum lengkap
        // =====================================================================

        /// <summary>
        /// `BE-RWI-045 AC 4` — <b>sebagian</b>. Menyelesaikan kajian medis yang bagiannya masih
        /// kosong ditolak `400`, dan pesannya menyebut bagian mana saja yang kosong.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>VAL-DOK-10</c>. Bagian yang dapat diperiksa hari ini hanya keluhan utama dan
        /// riwayat penyakit sekarang. Pemeriksaan fisik, rencana terapi, dan diagnosis kerja —
        /// yang justru disebut <c>VAL-DOK-11</c> — <b>tidak punya kolom</b> pada
        /// <c>TrxPatientAssessment</c>, dan kamus data menyatakan sub-modul ini menambahkan nol
        /// kolom pada tabel itu. Kekurangannya dilaporkan sebagai blocker, bukan ditambal.
        /// </para>
        /// </remarks>
        [Fact]
        public async Task KajianMedisYangBagiannyaKosong_DitolakBesertaDaftarBagiannya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var dibuat = await BuatControllerKajian(context, k.DokterUserId)
                .CreateAssessment(new CreatePatientAssessmentRequest
                {
                    EncounterId = k.EncounterId,
                    InpEpisodeId = k.EpisodeId,
                    AssessmentType = PatientAssessmentType.MedicalInitial
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dibuat));

            var kajian = await context.Set<TrxPatientAssessment>()
                .SingleAsync(x => x.AssessmentType == PatientAssessmentType.MedicalInitial);

            var hasil = await BuatControllerKajian(context, k.DokterUserId)
                .CompleteAssessment(kajian.Id, new CompletePatientAssessmentRequest());

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));

            var pesan = ControllerTestHarness.Pesan(hasil);

            Assert.StartsWith("Kajian medis belum dapat diselesaikan. Bagian berikut masih kosong:", pesan);
            Assert.Contains("keluhan utama", pesan);
            Assert.Contains("riwayat penyakit sekarang", pesan);

            using var pembaca = database.CreateContext();
            var sesudah = await pembaca.Set<TrxPatientAssessment>().SingleAsync(x => x.Id == kajian.Id);

            Assert.NotEqual(PatientAssessmentStatus.Completed, sesudah.AssessmentStatus);
        }

        /// <summary>
        /// Aturan kelengkapan itu <b>tidak</b> menyentuh pengkajian keperawatan. Perilaku
        /// poliklinik dan IGD tidak berubah sedikit pun.
        /// </summary>
        [Fact]
        public async Task PengkajianKeperawatanYangKosong_TetapDapatDiselesaikan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var kajian = new TrxPatientAssessment
            {
                AssessmentNumber = $"ASM-{Guid.NewGuid():N}"[..20],
                EncounterId = k.EncounterId,
                PatientId = k.PatientId,
                ServiceUnitId = k.ServiceUnitId,
                AssessmentType = PatientAssessmentType.Initial,
                AssessmentStatus = PatientAssessmentStatus.InProgress
            };

            context.Set<TrxPatientAssessment>().Add(kajian);
            await context.SaveChangesAsync();

            var hasil = await BuatControllerKajian(context, k.DokterUserId)
                .CompleteAssessment(kajian.Id, new CompletePatientAssessmentRequest());

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));
        }

        // =====================================================================
        // Kriteria 5 — kewenangan menulis
        // =====================================================================

        /// <summary>
        /// `BE-RWI-045 AC 5` — pengguna yang tidak terhubung ke data dokter ditolak `403` saat
        /// mencoba membuat kajian medis.
        /// </summary>
        /// <remarks>
        /// <c>VAL-DOK-05</c>. Penolakannya diturunkan dari <b>data</b> — tidak ada satu baris
        /// pun kode yang membaca nama peran, nama jabatan, maupun <c>UserType</c>. Hak akses
        /// memanggil endpoint ini tetap ditentukan admin lewat layar Akses Role; yang dijaga di
        /// sini adalah kewenangan yang melekat pada data, dan itu memang bukan urusan mesin hak
        /// akses — <c>permission-audit-matrix.md</c> bagian 3.
        /// </remarks>
        [Fact]
        public async Task PenggunaYangBukanDokter_Ditolak403SaatMembuatKajianMedis()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var hasil = await BuatControllerKajian(context, perawat.Id)
                .CreateAssessment(PermintaanKajianMedis(k));

            Assert.Equal(403, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal("Catatan ini hanya dapat ditulis dokter.", ControllerTestHarness.Pesan(hasil));
            Assert.Equal(0, await context.Set<TrxPatientAssessment>().CountAsync());
        }

        /// <summary>
        /// `BE-RWI-045 AC 5` — kendali negatif. Pengguna yang sama tetap boleh membuat
        /// <b>pengkajian keperawatan</b>, sehingga penolakan di atas benar-benar tentang jenis
        /// dokumennya dan bukan tentang penutupan jalur pengkajian secara umum.
        /// </summary>
        [Fact]
        public async Task PenggunaYangBukanDokter_TetapBolehMembuatPengkajianKeperawatan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var hasil = await BuatControllerKajian(context, perawat.Id)
                .CreateAssessment(new CreatePatientAssessmentRequest
                {
                    EncounterId = k.EncounterId,
                    ChiefComplaint = "Sesak napas"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            var tersimpan = await context.Set<TrxPatientAssessment>().SingleAsync();
            Assert.Equal(PatientAssessmentType.Initial, tersimpan.AssessmentType);
        }

        /// <summary>
        /// `VAL-DOK-01` — kajian medis hanya lahir di atas perawatan rawat inap yang berjalan.
        /// Kunjungan poliklinik ditolak `422`.
        /// </summary>
        [Fact]
        public async Task KajianMedisPadaKunjunganTanpaPerawatan_Ditolak422()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var rawatInap = RawatInapTestData.SiapkanPerawatan(context);
            var poliklinik = RekamMedisTestData.SiapkanPasienDanKunjungan(context);

            var hasil = await BuatControllerKajian(context, rawatInap.DokterUserId)
                .CreateAssessment(new CreatePatientAssessmentRequest
                {
                    EncounterId = poliklinik.EncounterId,
                    AssessmentType = PatientAssessmentType.MedicalInitial,
                    ChiefComplaint = "Sesak napas"
                });

            Assert.Equal(422, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal("Pasien ini tidak sedang dirawat inap.", ControllerTestHarness.Pesan(hasil));
        }

        // =====================================================================
        // Kriteria 6 — pendaftaran ke mesin keutuhan
        // =====================================================================

        /// <summary>
        /// `BE-RWI-045 AC 6` — kajian medis yang selesai terdaftar pada mesin keutuhan rekam
        /// medis, dalam penyimpanan yang sama dengan penyelesaiannya.
        /// </summary>
        /// <remarks>
        /// <c>RWI-AC-157</c>, <c>api-contract.md</c> bagian 2. Pendaftaran inilah yang kelak
        /// membuat pesan "gunakan koreksi" pada <c>VAL-DOK-33</c> menjadi janji yang benar:
        /// dokumen yang tidak pernah terdaftar tidak dapat dikoreksi lewat addendum.
        /// </remarks>
        [Fact]
        public async Task KajianMedisYangSelesai_TerdaftarPadaMesinKeutuhan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var kajian = await BuatKajianMedisAsync(context, k);

            Assert.Equal(0, await context.Set<MrcClinicalDocumentIntegrity>().CountAsync());

            var hasil = await BuatControllerKajian(context, k.DokterUserId)
                .CompleteAssessment(kajian.Id, new CompletePatientAssessmentRequest());

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            var isi = Assert.IsType<ApiResponse<PatientAssessmentCompleteResponse>>(
                ((Microsoft.AspNetCore.Mvc.ObjectResult)hasil).Value);

            Assert.True(isi.Data!.IsRegisteredToIntegrity);

            using var pembaca = database.CreateContext();

            var keutuhan = await pembaca.Set<MrcClinicalDocumentIntegrity>()
                .SingleAsync(x => x.DocumentId == kajian.Id);

            Assert.Equal(ClinicalDocumentKind.Assessment, keutuhan.DocumentKind);
            Assert.Equal(k.EncounterId, keutuhan.EncounterId);
            Assert.Equal(k.PatientId, keutuhan.PatientId);
        }

        /// <summary>
        /// Pengkajian keperawatan yang selesai <b>tidak</b> didaftarkan dari sini. Pendaftarannya
        /// adalah pekerjaan sub-modul keperawatan; menyalakannya dari sini akan mengubah jalur
        /// poliklinik dan IGD yang tidak diminta task ini.
        /// </summary>
        [Fact]
        public async Task PengkajianKeperawatanYangSelesai_TidakDidaftarkanDariSini()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var kajian = new TrxPatientAssessment
            {
                AssessmentNumber = $"ASM-{Guid.NewGuid():N}"[..20],
                EncounterId = k.EncounterId,
                PatientId = k.PatientId,
                ServiceUnitId = k.ServiceUnitId,
                AssessmentType = PatientAssessmentType.Initial,
                AssessmentStatus = PatientAssessmentStatus.InProgress
            };

            context.Set<TrxPatientAssessment>().Add(kajian);
            await context.SaveChangesAsync();

            await BuatControllerKajian(context, k.DokterUserId)
                .CompleteAssessment(kajian.Id, new CompletePatientAssessmentRequest());

            Assert.Equal(0, await context.Set<MrcClinicalDocumentIntegrity>().CountAsync());
        }

        // =====================================================================
        // Pembacaan kajian per perawatan
        // =====================================================================

        /// <summary>
        /// `api-contract.md` bagian 2 — kajian satu perawatan terbaca dan dapat disaring
        /// jenisnya, sehingga layar dokter dan layar perawat tidak saling menimpa.
        /// </summary>
        [Fact]
        public async Task KajianPerPerawatan_TerbacaDanDapatDisaringJenisnya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            await BuatKajianMedisAsync(context, k);

            context.Set<TrxPatientAssessment>().Add(new TrxPatientAssessment
            {
                AssessmentNumber = $"ASM-{Guid.NewGuid():N}"[..20],
                EncounterId = k.EncounterId,
                PatientId = k.PatientId,
                ServiceUnitId = k.ServiceUnitId,
                InpEpisodeId = k.EpisodeId,
                AssessmentType = PatientAssessmentType.Initial,
                AssessmentStatus = PatientAssessmentStatus.Completed
            });
            await context.SaveChangesAsync();

            var controller = BuatControllerKajian(context, k.DokterUserId);

            var semua = await controller.GetByEpisode(k.EpisodeId);
            var hanyaMedis = await controller.GetByEpisode(k.EpisodeId, PatientAssessmentType.MedicalInitial);

            Assert.Equal(2, JumlahBaris(semua));
            Assert.Equal(1, JumlahBaris(hanyaMedis));
        }

        /// <summary>Perawatan yang tidak ada dijawab `404`, bukan daftar kosong.</summary>
        [Fact]
        public async Task KajianPerPerawatanYangTidakAda_Dijawab404()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var hasil = await BuatControllerKajian(context, k.DokterUserId)
                .GetByEpisode(Guid.NewGuid());

            Assert.Equal(404, ControllerTestHarness.KodeStatus(hasil));
        }

        private static int JumlahBaris(Microsoft.AspNetCore.Mvc.IActionResult hasil)
        {
            var isi = Assert.IsType<ApiResponse<PagedResult<PatientAssessmentResponse>>>(
                ((Microsoft.AspNetCore.Mvc.ObjectResult)hasil).Value);

            return isi.Data!.TotalData;
        }
    }
}
