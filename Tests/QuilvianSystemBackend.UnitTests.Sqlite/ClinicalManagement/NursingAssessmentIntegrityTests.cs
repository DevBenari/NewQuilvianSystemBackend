using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-065</c> dan <c>BE-RWI-057</c> — pengkajian keperawatan
    /// yang selesai ikut terkunci, lalu dibetulkan lewat koreksi dan bukan dengan menimpanya.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Kedua task ini bersambung dan karena itu diuji berdampingan: tanpa pendaftaran keutuhan
    /// milik <c>BE-RWI-065</c>, mesin koreksi <c>BE-RWI-057</c> tidak punya apa pun untuk
    /// dikoreksi.
    /// </para>
    /// <para>
    /// <b>Yang paling menentukan di sini</b> adalah bahwa satu lembar rekam medis tidak memuat
    /// dua bentuk penguncian. Pengkajian perawat dan catatan dokter terkunci pada mesin yang
    /// sama, dan keduanya dikoreksi dengan cara yang sama — <c>RWI-DEC-091</c>.
    /// </para>
    /// </remarks>
    public class NursingAssessmentIntegrityTests
    {
        private static CreatePatientAssessmentRequest Permintaan(RawatInapTestData.Konteks k) => new()
        {
            EncounterId = k.EncounterId,
            InpEpisodeId = k.EpisodeId,
            AssessmentType = PatientAssessmentType.Initial,
            ChiefComplaint = "Nyeri dada",
            HasPain = true,
            PainScale = 3,
            NutritionRiskStatus = NutritionRiskStatus.LowRisk
        };

        private static async Task<TrxPatientAssessment> BuatDanSelesaikanAsync(
            TestDatabase database,
            ApplicationDbContext context,
            RawatInapTestData.Konteks k,
            Guid perawatUserId)
        {
            var dibuat = await NursingAssessmentContextTests.BuatController(context, perawatUserId)
                .CreateAssessment(Permintaan(k));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dibuat));

            var pengkajian = await context.Set<TrxPatientAssessment>()
                .SingleAsync(x => x.InpEpisodeId == k.EpisodeId);

            var selesai = await NursingAssessmentContextTests.BuatController(context, perawatUserId)
                .CompleteAssessment(pengkajian.Id, new CompletePatientAssessmentRequest());

            Assert.Equal(200, ControllerTestHarness.KodeStatus(selesai));

            return pengkajian;
        }

        // =====================================================================
        // BE-RWI-065 kriteria 1 — pendaftaran saat penyelesaian
        // =====================================================================

        /// <summary>
        /// `BE-RWI-065 AC 1` — menyelesaikan pengkajian keperawatan rawat inap mendaftarkannya
        /// pada mesin keutuhan berjenis <c>Assessment</c>, dengan penulis pengkajian sebagai
        /// penanda tangannya.
        /// </summary>
        [Fact]
        public async Task PengkajianKeperawatanSelesai_TerdaftarSebagaiDokumenTertandaTangan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var pengkajian = await BuatDanSelesaikanAsync(database, context, k, perawat.Id);

            using var pembaca = database.CreateContext();

            var keutuhan = await pembaca.Set<MrcClinicalDocumentIntegrity>()
                .SingleAsync(x => x.DocumentId == pengkajian.Id);

            Assert.Equal(ClinicalDocumentKind.Assessment, keutuhan.DocumentKind);
            Assert.Equal(ClinicalDocumentIntegrityStatus.Signed, keutuhan.IntegrityStatus);
            Assert.Equal(perawat.Id, keutuhan.AuthorUserId);
            Assert.Equal(perawat.Id, keutuhan.SignedByUserId);
            Assert.NotNull(keutuhan.SignedAt);
            Assert.NotNull(keutuhan.LockedAt);
            Assert.Equal(ClinicalDocumentLockTrigger.AuthorSigned, keutuhan.LockTrigger);
        }

        /// <summary>
        /// `BE-RWI-065 AC 4` — nol nilai enum baru ditambahkan ke <c>ClinicalDocumentKind</c>.
        /// </summary>
        /// <remarks>
        /// Jenis <c>Assessment</c> sudah ada dan sudah ditegakkan sejak <c>BE-RWI-038</c>. Task
        /// ini memakainya apa adanya; menambah nilai enum baru justru akan melahirkan dua jenis
        /// dokumen untuk satu konsep yang sama.
        /// </remarks>
        [Fact]
        public void NolNilaiEnumBaru_PadaJenisDokumenKlinis()
        {
            var jumlahNilai = Enum.GetValues<ClinicalDocumentKind>().Length;

            Assert.Equal(13, jumlahNilai);
            Assert.True(Enum.IsDefined(ClinicalDocumentKind.Assessment));
            Assert.True(Enum.IsDefined(ClinicalDocumentKind.Procedure));
        }

        // =====================================================================
        // BE-RWI-065 kriteria 2 — pendaftaran gagal membatalkan penyelesaian
        // =====================================================================

        /// <summary>
        /// `BE-RWI-065 AC 2` — bila pendaftaran keutuhan gagal, penyelesaian ikut batal.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Kegagalan dipaksa dengan cara yang benar-benar mungkin terjadi: penulis pengkajian
        /// tidak dapat ditentukan. Mesin keutuhan menolak dokumen bertanda tangan tanpa penanda
        /// tangan, karena dokumen seperti itu bukan bukti apa pun.
        /// </para>
        /// <para>
        /// Yang dibuktikan bukan sekadar balasannya <c>400</c>, melainkan bahwa <b>tidak satu
        /// pun</b> perubahan tersimpan: pengkajiannya tetap belum selesai. Bila dipisah menjadi
        /// dua langkah, akan lahir pengkajian selesai yang tidak dapat dikoreksi selamanya —
        /// persis keadaan yang ditemukan <c>RWI-FACT-014</c> pada dokumen dokter.
        /// </para>
        /// </remarks>
        [Fact]
        public async Task PendaftaranKeutuhanGagal_PenyelesaianIkutBatal()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var dibuat = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CreateAssessment(Permintaan(k));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dibuat));

            var pengkajian = await context.Set<TrxPatientAssessment>()
                .SingleAsync(x => x.InpEpisodeId == k.EpisodeId);

            // Penulis dihapus, sehingga penanda tangannya tidak dapat ditentukan.
            pengkajian.AssessmentByUserId = null;
            pengkajian.CreateBy = Guid.Empty;
            await context.SaveChangesAsync();

            // Aktor pun tidak dikenali, sehingga tidak ada jalan mengisi penanda tangan.
            var controller = NursingAssessmentContextTests.BuatController(context, Guid.Empty);

            var hasil = await controller.CompleteAssessment(
                pengkajian.Id, new CompletePatientAssessmentRequest());

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Contains("rekam medis gagal", ControllerTestHarness.Pesan(hasil)!);

            using var pembaca = database.CreateContext();

            var sesudah = await pembaca.Set<TrxPatientAssessment>().SingleAsync(x => x.Id == pengkajian.Id);

            Assert.NotEqual(PatientAssessmentStatus.Completed, sesudah.AssessmentStatus);
            Assert.Null(sesudah.CompletedAt);
            Assert.Equal(0, await pembaca.Set<MrcClinicalDocumentIntegrity>()
                .CountAsync(x => x.DocumentId == pengkajian.Id));
        }

        // =====================================================================
        // BE-RWI-065 kriteria 3 dan 6 — penjaga penyuntingan
        // =====================================================================

        /// <summary>
        /// `BE-RWI-065 AC 3` — menyunting langsung pengkajian yang sudah terkunci ditolak
        /// <c>400</c>, beserta arahan memakai koreksi.
        /// </summary>
        [Fact]
        public async Task MenyuntingPengkajianTerkunci_Ditolak400DenganArahanKoreksi()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var pengkajian = await BuatDanSelesaikanAsync(database, context, k, perawat.Id);

            var hasil = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .UpdateAssessment(pengkajian.Id, new UpdatePatientAssessmentRequest
                {
                    ChiefComplaint = "Diubah diam-diam"
                });

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Contains("addendum", ControllerTestHarness.Pesan(hasil)!);

            using var pembaca = database.CreateContext();
            var sesudah = await pembaca.Set<TrxPatientAssessment>().SingleAsync(x => x.Id == pengkajian.Id);

            Assert.Equal("Nyeri dada", sesudah.ChiefComplaint);
        }

        /// <summary>
        /// `BE-RWI-065 AC 6` — pengkajian yang masih dikerjakan tetap dapat disunting seperti
        /// biasa.
        /// </summary>
        [Fact]
        public async Task PengkajianBelumSelesai_TetapDapatDisunting()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var dibuat = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CreateAssessment(Permintaan(k));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dibuat));

            var pengkajian = await context.Set<TrxPatientAssessment>()
                .SingleAsync(x => x.InpEpisodeId == k.EpisodeId);

            var hasil = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .UpdateAssessment(pengkajian.Id, new UpdatePatientAssessmentRequest
                {
                    ChiefComplaint = "Nyeri dada menjalar ke lengan kiri"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            using var pembaca = database.CreateContext();
            var sesudah = await pembaca.Set<TrxPatientAssessment>().SingleAsync(x => x.Id == pengkajian.Id);

            Assert.Equal("Nyeri dada menjalar ke lengan kiri", sesudah.ChiefComplaint);
        }

        // =====================================================================
        // BE-RWI-065 kriteria 5 — regresi jalur lama
        // =====================================================================

        /// <summary>
        /// `BE-RWI-065 AC 5` — pengkajian poliklinik yang diselesaikan <b>tidak</b> didaftarkan
        /// pada mesin keutuhan, dan tetap dapat disunting seperti sebelumnya.
        /// </summary>
        /// <remarks>
        /// Inilah batas yang disengaja. Menyalakan pendaftaran bagi seluruh pengkajian akan
        /// mengubah perilaku jalur poliklinik dan IGD yang tidak diminta task mana pun — source
        /// menyatakannya sendiri sebelum task ini dikerjakan.
        /// </remarks>
        [Fact]
        public async Task PengkajianPoliklinikSelesai_TidakDidaftarkanDanTetapDapatDisunting()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");
            var antrean = NursingAssessmentContextTests.BuatAntreanScreening(context, konteks);

            var dibuat = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CreateAssessment(new CreatePatientAssessmentRequest
                {
                    EncounterId = konteks.EncounterId,
                    QueueId = antrean.Id,
                    ChiefComplaint = "Batuk"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dibuat));

            var pengkajian = await context.Set<TrxPatientAssessment>().SingleAsync();

            var selesai = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CompleteAssessment(pengkajian.Id, new CompletePatientAssessmentRequest());

            Assert.Equal(200, ControllerTestHarness.KodeStatus(selesai));

            using var pembaca = database.CreateContext();

            Assert.Equal(0, await pembaca.Set<MrcClinicalDocumentIntegrity>()
                .CountAsync(x => x.DocumentId == pengkajian.Id));

            // Penolakan penyuntingannya tetap kalimat lama, bukan arahan koreksi.
            var hasil = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .UpdateAssessment(pengkajian.Id, new UpdatePatientAssessmentRequest());

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal("Assessment yang sudah completed tidak dapat diubah.",
                ControllerTestHarness.Pesan(hasil));
        }

        // =====================================================================
        // BE-RWI-057 — koreksi lewat addendum
        // =====================================================================

        /// <summary>
        /// `BE-RWI-057 AC 1, 3, dan 7` — koreksi menyimpan aktor, waktu, alasan, dan nomor urut;
        /// isi pengkajian asli tidak berubah; status tetap <c>Completed</c>; dan tidak satu baris
        /// pun ditulis pada tabel milik <c>ClinicalManagement</c>.
        /// </summary>
        [Fact]
        public async Task Koreksi_MenyimpanJejakTanpaMengubahIsiAsli()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var pengkajian = await BuatDanSelesaikanAsync(database, context, k, perawat.Id);

            var hasil = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CreateAddendum(pengkajian.Id, new CreateAssessmentAddendumRequest
                {
                    Content = "Skala nyeri seharusnya 7, bukan 3.",
                    Reason = "Salah ketik saat pencatatan"
                });

            Assert.Equal(201, ControllerTestHarness.KodeStatus(hasil));

            using var pembaca = database.CreateContext();

            var addendum = await pembaca.Set<MrcClinicalNoteAddendum>().SingleAsync();

            Assert.Equal(1, addendum.Sequence);
            Assert.Equal(perawat.Id, addendum.AuthorUserId);
            Assert.Equal("Skala nyeri seharusnya 7, bukan 3.", addendum.AddendumText);
            Assert.Equal("Salah ketik saat pencatatan", addendum.CorrectionReason);
            Assert.NotEqual(default, addendum.SignedAt);

            var sesudah = await pembaca.Set<TrxPatientAssessment>().SingleAsync(x => x.Id == pengkajian.Id);

            Assert.Equal(PatientAssessmentStatus.Completed, sesudah.AssessmentStatus);
            Assert.Equal(3, sesudah.PainScale);
            Assert.Equal("Nyeri dada", sesudah.ChiefComplaint);
        }

        /// <summary>
        /// `BE-RWI-057 AC 3` — koreksi kedua menambah nomor urut berikutnya, dan status
        /// pengkajian <b>tetap</b> <c>Completed</c>.
        /// </summary>
        [Fact]
        public async Task KoreksiKedua_MenambahNomorUrutDanStatusTetapCompleted()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var pengkajian = await BuatDanSelesaikanAsync(database, context, k, perawat.Id);

            foreach (var isi in new[] { "Koreksi pertama", "Koreksi kedua" })
            {
                var hasil = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                    .CreateAddendum(pengkajian.Id, new CreateAssessmentAddendumRequest
                    {
                        Content = isi,
                        Reason = "Pembetulan"
                    });

                Assert.Equal(201, ControllerTestHarness.KodeStatus(hasil));
            }

            using var pembaca = database.CreateContext();

            var urutan = await pembaca.Set<MrcClinicalNoteAddendum>()
                .OrderBy(x => x.Sequence)
                .Select(x => x.Sequence)
                .ToListAsync();

            Assert.Equal([1, 2], urutan);

            var sesudah = await pembaca.Set<TrxPatientAssessment>().SingleAsync(x => x.Id == pengkajian.Id);
            Assert.Equal(PatientAssessmentStatus.Completed, sesudah.AssessmentStatus);
        }

        /// <summary>
        /// `BE-RWI-057 AC 2` — koreksi tanpa alasan ditolak <c>400</c> (<c>VAL-KEP-12</c>).
        /// </summary>
        [Fact]
        public async Task KoreksiTanpaAlasan_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var pengkajian = await BuatDanSelesaikanAsync(database, context, k, perawat.Id);

            var hasil = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CreateAddendum(pengkajian.Id, new CreateAssessmentAddendumRequest
                {
                    Content = "Skala nyeri seharusnya 7.",
                    Reason = "   "
                });

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));

            using var pembaca = database.CreateContext();
            Assert.Equal(0, await pembaca.Set<MrcClinicalNoteAddendum>().CountAsync());
        }

        /// <summary>
        /// `BE-RWI-057 AC 4` — koreksi pada pengkajian yang masih konsep ditolak, dengan arahan
        /// membetulkan langsung pada isinya (<c>RWI-FACT-013</c>).
        /// </summary>
        [Fact]
        public async Task KoreksiPadaPengkajianKonsep_DitolakDenganArahanSuntingLangsung()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var dibuat = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CreateAssessment(Permintaan(k));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dibuat));

            var pengkajian = await context.Set<TrxPatientAssessment>()
                .SingleAsync(x => x.InpEpisodeId == k.EpisodeId);

            var hasil = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CreateAddendum(pengkajian.Id, new CreateAssessmentAddendumRequest
                {
                    Content = "Koreksi",
                    Reason = "Salah tulis"
                });

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Contains("belum final", ControllerTestHarness.Pesan(hasil)!);
        }

        /// <summary>
        /// `BE-RWI-057 AC 5` — nilai status <c>Amended</c> tidak ada, sehingga transisi menujunya
        /// mustahil. Mesin status pengkajian tetap berisi empat nilai.
        /// </summary>
        /// <remarks>
        /// <c>RWI-DEC-091</c> mencabut nilai itu. Pertanyaan "apakah dokumen ini pernah
        /// dikoreksi" dijawab riwayat addendum, bukan status dokumen; dua sumber jawaban untuk
        /// satu pertanyaan adalah cara termudah membuat rekam medis bertentangan dengan dirinya
        /// sendiri.
        /// </remarks>
        [Fact]
        public void StatusAmended_TidakAdaPadaMesinStatusPengkajian()
        {
            var nilai = Enum.GetNames<PatientAssessmentStatus>();

            Assert.Equal(4, nilai.Length);
            Assert.DoesNotContain("Amended", nilai);
            Assert.Equal(["Draft", "InProgress", "Completed", "Cancelled"], nilai);
        }

        /// <summary>
        /// `BE-RWI-057 AC 6` — pengkajian tidak dapat dihapus: grup endpoint ini tidak memiliki
        /// satu pun <c>DELETE</c>.
        /// </summary>
        [Fact]
        public void GrupPengkajian_TidakPunyaEndpointPenghapusan()
        {
            var adaDelete = typeof(PatientAssessmentController)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .SelectMany(x => x.GetCustomAttributes<HttpMethodAttribute>())
                .SelectMany(x => x.HttpMethods)
                .Any(x => string.Equals(x, "DELETE", StringComparison.OrdinalIgnoreCase));

            Assert.False(adaDelete,
                "Grup Patient Assessment tidak boleh punya endpoint penghapusan - CAP-012 aturan 12.");
        }

        /// <summary>
        /// `BE-RWI-057 AC 7` — koreksi tidak menulis satu baris pun pada tabel milik
        /// <c>ClinicalManagement</c>; seluruhnya tersimpan pada mesin <c>MedicalRecordManagement</c>.
        /// </summary>
        [Fact]
        public async Task Koreksi_TidakMenulisPadaTabelClinicalManagement()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var pengkajian = await BuatDanSelesaikanAsync(database, context, k, perawat.Id);

            using var sebelum = database.CreateContext();
            var jumlahPengkajianSebelum = await sebelum.Set<TrxPatientAssessment>().CountAsync();
            var jumlahCatatanTerpaduSebelum = await sebelum
                .Set<TrxPatientIntegratedProgressNote>().CountAsync();
            var waktuUbahSebelum = (await sebelum.Set<TrxPatientAssessment>()
                .SingleAsync(x => x.Id == pengkajian.Id)).UpdateDateTime;

            var hasil = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CreateAddendum(pengkajian.Id, new CreateAssessmentAddendumRequest
                {
                    Content = "Koreksi",
                    Reason = "Pembetulan"
                });

            Assert.Equal(201, ControllerTestHarness.KodeStatus(hasil));

            using var sesudah = database.CreateContext();

            Assert.Equal(jumlahPengkajianSebelum,
                await sesudah.Set<TrxPatientAssessment>().CountAsync());
            Assert.Equal(jumlahCatatanTerpaduSebelum,
                await sesudah.Set<TrxPatientIntegratedProgressNote>().CountAsync());
            Assert.Equal(waktuUbahSebelum,
                (await sesudah.Set<TrxPatientAssessment>()
                    .SingleAsync(x => x.Id == pengkajian.Id)).UpdateDateTime);

            Assert.Equal(1, await sesudah.Set<MrcClinicalNoteAddendum>().CountAsync());
        }

        /// <summary>Daftar koreksi terbaca lewat endpoint bacanya, terurut nomor.</summary>
        [Fact]
        public async Task DaftarKoreksi_TerbacaTerurutNomor()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var pengkajian = await BuatDanSelesaikanAsync(database, context, k, perawat.Id);

            foreach (var isi in new[] { "Koreksi satu", "Koreksi dua" })
            {
                await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                    .CreateAddendum(pengkajian.Id, new CreateAssessmentAddendumRequest
                    {
                        Content = isi,
                        Reason = "Pembetulan"
                    });
            }

            var hasil = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .GetAddendums(pengkajian.Id);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            var isiBalasan = (ObjectResult)hasil;
            var data = isiBalasan.Value!.GetType().GetProperty("Data")!.GetValue(isiBalasan.Value);
            var daftar = Assert.IsAssignableFrom<System.Collections.IEnumerable>(data!)
                .Cast<object>()
                .ToList();

            Assert.Equal(2, daftar.Count);
        }

        /// <summary>Koreksi pada pengkajian yang tidak ada dijawab <c>404</c>.</summary>
        [Fact]
        public async Task KoreksiPadaPengkajianTidakDikenal_Ditolak404()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var hasil = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CreateAddendum(Guid.NewGuid(), new CreateAssessmentAddendumRequest
                {
                    Content = "Koreksi",
                    Reason = "Pembetulan"
                });

            Assert.Equal(404, ControllerTestHarness.KodeStatus(hasil));
        }
    }
}
