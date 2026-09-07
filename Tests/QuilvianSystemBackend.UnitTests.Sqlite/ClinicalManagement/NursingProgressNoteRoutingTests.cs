using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-063</c> — catatan keperawatan tampil pada catatan terpadu
    /// tanpa tabel baru.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Yang sebenarnya diperbaiki task ini.</b> Kolom <c>InpEpisodeId</c> pada catatan terpadu
    /// sudah ada sejak <c>BE-RWI-040</c>, tetapi <b>tidak pernah diisi siapa pun</b>. Akibatnya
    /// lini masa catatan terpadu satu perawatan selalu kosong — catatan perawat maupun catatan
    /// dokter sama-sama tidak pernah sampai ke sana. Yang ditambahkan task ini adalah penurunan
    /// konteksnya, bukan tabel atau kolom.
    /// </para>
    /// <para>
    /// Karena itu berkas ini juga memuat uji regresi catatan dokter: perilaku yang sudah berjalan
    /// tidak boleh bergeser satu langkah pun.
    /// </para>
    /// </remarks>
    public class NursingProgressNoteRoutingTests
    {
        private static PatientIntegratedProgressNoteController BuatController(
            ApplicationDbContext c, Guid actorUserId) =>
            new PatientIntegratedProgressNoteController(
                c,
                ControllerTestHarness.BuatLoggerService(actorUserId),
                new ClinicalDocumentIntegrityService(c),
                new CpptVerificationService(c, new InpatientClinicalContextService(c)),
                new InpatientClinicalContextService(c))
                .DenganPengguna(actorUserId);

        private static T Isi<T>(IActionResult hasil) where T : class
        {
            var objek = Assert.IsAssignableFrom<ObjectResult>(hasil);
            var pembungkus = Assert.IsType<ApiResponse<T>>(objek.Value);
            Assert.NotNull(pembungkus.Data);
            return pembungkus.Data!;
        }

        private static CreatePatientIntegratedProgressNoteRequest Permintaan(
            RawatInapTestData.Konteks k,
            string profesi,
            string catatan) => new()
            {
                PatientId = k.PatientId,
                EncounterId = k.EncounterId,
                ProfessionType = profesi,
                NoteText = catatan
            };

        // =====================================================================
        // Kriteria 1 - catatan keperawatan tampil pada catatan terpadu
        // =====================================================================

        /// <summary>
        /// `BE-RWI-063 AC 1`, `INT-KEP-03` — catatan keperawatan tersimpan dengan
        /// <c>ProfessionType</c> perawat dan tampil pada lini masa catatan terpadu perawatan itu.
        /// </summary>
        [Fact]
        public async Task CatatanKeperawatan_TampilPadaLiniMasaCatatanTerpadu()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var (_, akunPerawat) = RawatInapTestData.BuatPerawat(context);

            var dibuat = await BuatController(context, akunPerawat.Id)
                .CreateProgressNote(Permintaan(k, "Perawat", "Pasien mengeluh nyeri berkurang, mobilisasi bertahap"));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dibuat));

            var response = Isi<PatientIntegratedProgressNoteCreateResponse>(dibuat);

            // Konteks perawatan diturunkan backend dari kunjungan.
            Assert.Equal(k.EpisodeId, response.InpEpisodeId);
            Assert.Equal("Nurse", response.ProfessionType);

            var liniMasa = Isi<PagedResult<PatientIntegratedProgressNoteResponse>>(
                await BuatController(context, akunPerawat.Id).GetByEpisode(k.EpisodeId));

            var baris = Assert.Single(liniMasa.Items);

            Assert.Equal(response.Id, baris.Id);
            Assert.Equal("Nurse", baris.ProfessionType);
            Assert.Equal(k.EpisodeId, baris.InpEpisodeId);
        }

        /// <summary>
        /// `BE-RWI-063 AC 1` — lini masa satu perawatan memuat catatan dua profesi sekaligus,
        /// sehingga seluruh profesi membaca satu perkembangan pasien yang sama.
        /// </summary>
        [Fact]
        public async Task LiniMasaSatuPerawatan_MemuatCatatanDuaProfesi()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var (_, akunPerawat) = RawatInapTestData.BuatPerawat(context);

            await BuatController(context, akunPerawat.Id)
                .CreateProgressNote(Permintaan(k, "Perawat", "Tanda vital stabil, luka bersih"));

            await BuatController(context, k.DokterUserId)
                .CreateProgressNote(Permintaan(k, "Dokter", "Terapi dilanjutkan, rencana pulang besok"));

            var liniMasa = Isi<PagedResult<PatientIntegratedProgressNoteResponse>>(
                await BuatController(context, akunPerawat.Id).GetByEpisode(k.EpisodeId));

            Assert.Equal(2, liniMasa.TotalData);
            Assert.Contains(liniMasa.Items, x => x.ProfessionType == "Nurse");
            Assert.Contains(liniMasa.Items, x => x.ProfessionType == "Doctor");
        }

        // =====================================================================
        // Kriteria 2 - nol tabel dan nol kolom baru
        // =====================================================================

        /// <summary>
        /// `BE-RWI-063 AC 2` — task ini memakai <c>TrxPatientIntegratedProgressNote</c> apa
        /// adanya. Kolom penghubungnya sudah ada dan sudah nullable sejak <c>BE-RWI-040</c>.
        /// </summary>
        /// <remarks>
        /// Uji ini membaca model EF Core yang sama dengan yang dipakai aplikasi. Bukti bahwa
        /// migration task ini benar-benar kosong dicatat pada laporan lewat
        /// <c>dotnet ef migrations has-pending-model-changes</c>.
        /// </remarks>
        [Fact]
        public void KolomPenghubungCatatanTerpadu_SudahAdaDanNullable()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var entity = context.Model.FindEntityType(typeof(TrxPatientIntegratedProgressNote))!;

            foreach (var nama in new[] { "InpEpisodeId", "EncounterId", "QueueId", "ConsultationId", "DoctorId" })
            {
                var properti = entity.FindProperty(nama);

                Assert.True(properti != null, $"Kolom {nama} tidak ditemukan.");
                Assert.True(properti!.IsNullable, $"Kolom {nama} seharusnya nullable.");
            }

            Assert.Equal("TrxPatientIntegratedProgressNote", entity.GetTableName());
        }

        // =====================================================================
        // Kriteria 3 - regresi catatan dokter
        // =====================================================================

        /// <summary>
        /// `BE-RWI-063 AC 3` — perilaku catatan dokter tidak berubah: pembuatannya tetap
        /// dijawab <c>200</c>, profesinya tetap tersimpan sebagai <c>Doctor</c>, pembacaan lewat
        /// jalur lama tetap menemukannya, dan pendaftaran keutuhannya tetap terbentuk.
        /// </summary>
        [Fact]
        public async Task CatatanDokter_PerilakunyaTidakBerubah()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var dibuat = await BuatController(context, k.DokterUserId)
                .CreateProgressNote(Permintaan(k, "Dokter", "Pemeriksaan pagi, kondisi membaik"));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dibuat));

            var response = Isi<PatientIntegratedProgressNoteCreateResponse>(dibuat);

            Assert.Equal("Doctor", response.ProfessionType);

            // Pembacaan lewat jalur lama, yaitu detail per catatan.
            var detail = await BuatController(context, k.DokterUserId).GetById(response.Id);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(detail));

            using var pembaca = database.CreateContext();

            var tersimpan = await pembaca.Set<TrxPatientIntegratedProgressNote>()
                .SingleAsync(x => x.Id == response.Id);

            Assert.Equal("Doctor", tersimpan.ProfessionType);
            Assert.Equal(k.EncounterId, tersimpan.EncounterId);
            Assert.Equal("Pemeriksaan pagi, kondisi membaik", tersimpan.NoteText);

            // Pendaftaran keutuhan tetap terbentuk seperti sebelumnya.
            Assert.Equal(1, await pembaca.Set<MrcClinicalDocumentIntegrity>()
                .CountAsync(x => x.DocumentKind == ClinicalDocumentKind.ProgressNote &&
                                 x.DocumentId == response.Id));
        }

        /// <summary>
        /// `BE-RWI-063 AC 3` — catatan di luar rawat inap tidak ikut terbawa: kunjungan tanpa
        /// perawatan rawat inap tetap menghasilkan catatan berkonteks kosong.
        /// </summary>
        [Fact]
        public async Task CatatanTanpaPerawatanRawatInap_KonteksnyaTetapKosong()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var poliklinik = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter.poli");

            var dibuat = await BuatController(context, penulis.Id)
                .CreateProgressNote(new CreatePatientIntegratedProgressNoteRequest
                {
                    PatientId = poliklinik.PatientId,
                    EncounterId = poliklinik.EncounterId,
                    ProfessionType = "Dokter",
                    NoteText = "Kontrol rutin poliklinik"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dibuat));

            var response = Isi<PatientIntegratedProgressNoteCreateResponse>(dibuat);

            Assert.Null(response.InpEpisodeId);
        }

        // =====================================================================
        // Kriteria 4 - koreksi dua profesi pada catatan terpadu yang sama
        // =====================================================================

        /// <summary>
        /// `BE-RWI-063 AC 4`, `RWI-DEC-091` — satu perawatan dapat memuat koreksi perawat dan
        /// koreksi dokter pada catatan terpadu yang sama, <b>keduanya</b> dalam bentuk addendum
        /// bernomor. Bukan satu versi dan satu addendum.
        /// </summary>
        [Fact]
        public async Task SatuPerawatan_MemuatKoreksiPerawatDanKoreksiDokter_KeduanyaAddendum()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var (_, akunPerawat) = RawatInapTestData.BuatPerawat(context);

            var catatanPerawat = Isi<PatientIntegratedProgressNoteCreateResponse>(
                await BuatController(context, akunPerawat.Id)
                    .CreateProgressNote(Permintaan(k, "Perawat", "Nyeri skala 3")));

            var catatanDokter = Isi<PatientIntegratedProgressNoteCreateResponse>(
                await BuatController(context, k.DokterUserId)
                    .CreateProgressNote(Permintaan(k, "Dokter", "Terapi analgetik dilanjutkan")));

            var keutuhan = new ClinicalDocumentIntegrityService(context);
            var koreksi = new ClinicalNoteAddendumService(context, keutuhan);
            var now = DateTime.UtcNow;

            // Kedua catatan ditandatangani penulisnya masing-masing, seperti pada jalur nyata.
            foreach (var (noteId, penulis) in new[]
                     {
                         (catatanPerawat.Id, akunPerawat.Id),
                         (catatanDokter.Id, k.DokterUserId)
                     })
            {
                var (hasilTtd, _) = await keutuhan.SignAsync(
                    ClinicalDocumentKind.ProgressNote, noteId, penulis, null, null, now);

                Assert.True(hasilTtd.IsAllowed, hasilTtd.ErrorMessage);

                var (hasilKoreksi, addendum) = await koreksi.CreateAsync(
                    ClinicalDocumentKind.ProgressNote,
                    noteId,
                    penulis,
                    actorHasSubstituteAuthority: false,
                    addendumText: "Pembetulan isi catatan",
                    correctionReason: "Salah tulis",
                    deviceInfo: null,
                    ipAddress: null,
                    nowUtc: now.AddMinutes(5));

                Assert.True(hasilKoreksi.IsAllowed, hasilKoreksi.ErrorMessage);
                Assert.NotNull(addendum);

                // Keduanya addendum bernomor, dan nomornya dimulai dari satu pada tiap catatan.
                Assert.Equal(1, addendum!.Sequence);
            }

            using var pembaca = database.CreateContext();

            var seluruhKoreksi = await pembaca.Set<MrcClinicalNoteAddendum>().ToListAsync();

            Assert.Equal(2, seluruhKoreksi.Count);
            Assert.All(seluruhKoreksi, x => Assert.Equal("Salah tulis", x.CorrectionReason));

            // Isi asli kedua catatan tidak berubah sedikit pun.
            var catatan = await pembaca.Set<TrxPatientIntegratedProgressNote>()
                .Where(x => x.InpEpisodeId == k.EpisodeId)
                .OrderBy(x => x.ProfessionType)
                .ToListAsync();

            Assert.Equal(2, catatan.Count);
            Assert.Equal("Terapi analgetik dilanjutkan", catatan[0].NoteText);
            Assert.Equal("Nyeri skala 3", catatan[1].NoteText);
        }
    }
}
