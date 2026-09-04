using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-048</c> — kunjungan dokter tercatat sebagai kejadian
    /// tersendiri.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kenapa kejadian visite tidak boleh diturunkan dari catatan.</b> Dokter yang mendatangi
    /// pasien pukul 07.40 lalu terpanggil ke ruangan lain sebelum sempat mengetik apa pun tetap
    /// benar-benar datang. Sebaliknya, dokter yang menulis tiga catatan sepanjang satu kunjungan
    /// tetap datang sekali. Menghitung visite dari catatan karena itu dilarang
    /// <c>INV-DOK-07</c>, dan uji di bawah membuktikan keduanya.
    /// </para>
    /// <para>
    /// <b>Satu bukti sengaja tidak ada di sini.</b> Dua permintaan yang tiba benar-benar
    /// bersamaan hanya dapat dijamin unique index basis data, dan SQLite tidak menirukan
    /// perlombaan itu dengan setia. Buktinya berada pada
    /// <c>PhysicianVisitUniquenessTests</c> di project uji PostgreSQL.
    /// </para>
    /// </remarks>
    public class PhysicianVisitRecordingTests
    {
        internal static PhysicianVisitController BuatControllerVisite(
            ApplicationDbContext c, Guid actorUserId) =>
            new PhysicianVisitController(
                c,
                ControllerTestHarness.BuatLoggerService(actorUserId),
                new PhysicianVisitService(c, new PhysicianVisitNumberService()),
                new InpatientClinicalContextService(c))
                .DenganPengguna(actorUserId);

        internal static DoctorConsultationController BuatControllerCatatan(
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
                    new ClinicalDocumentIntegrityService(c)),
                new InpatientClinicalContextService(c))
                .DenganPengguna(actorUserId);

        internal static CreatePhysicianVisitRequest Permintaan(
            RawatInapTestData.Konteks k,
            DateTime? waktuKedatangan = null,
            string? kunci = null,
            PhysicianVisitRole peran = PhysicianVisitRole.Dpjp) => new()
            {
                EncounterId = k.EncounterId,
                InpEpisodeId = k.EpisodeId,
                PatientId = k.PatientId,
                DoctorId = k.DoctorMasterId,
                VisitDateTime = waktuKedatangan,
                VisitRole = peran,
                IdempotencyKey = kunci ?? Guid.NewGuid().ToString("N")
            };

        internal static T Isi<T>(Microsoft.AspNetCore.Mvc.IActionResult hasil)
        {
            var objek = Assert.IsAssignableFrom<Microsoft.AspNetCore.Mvc.ObjectResult>(hasil);
            var pembungkus = Assert.IsType<ApiResponse<T>>(objek.Value);

            Assert.NotNull(pembungkus.Data);
            return pembungkus.Data!;
        }

        // =====================================================================
        // Kriteria 1 dan 7 — kejadian berdiri sendiri, riwayat memuat isinya
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-048 AC 1</c> dan <c>AC 7</c> — visite pukul 07.40 muncul pada riwayat
        /// walaupun tidak ada satu pun catatan yang ditulis, dan riwayatnya memuat perawatan,
        /// dokter, peran, waktu, serta pencatatnya.
        /// </summary>
        /// <remarks>
        /// Waktu yang tersimpan adalah <b>waktu kedatangan</b>, bukan waktu pencatatan. Keduanya
        /// sengaja diperiksa terpisah: bila keliru disamakan, lini masa visite akan menggambarkan
        /// kapan dokter sempat mengetik, bukan kapan ia benar-benar datang.
        /// </remarks>
        [Fact]
        public async Task VisiteTanpaSatuPunCatatan_TetapMunculPadaRiwayatBesertaIsinya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var waktuKedatangan = DateTime.UtcNow.AddHours(-4);

            var hasil = await BuatControllerVisite(context, k.DokterUserId)
                .RecordVisit(Permintaan(k, waktuKedatangan));

            Assert.Equal(201, ControllerTestHarness.KodeStatus(hasil));

            var riwayat = Isi<PagedResult<PhysicianVisitListItemResponse>>(
                await BuatControllerVisite(context, k.DokterUserId).GetByEpisode(k.EpisodeId));

            var baris = Assert.Single(riwayat.Items);

            Assert.Equal(k.EpisodeId, baris.InpEpisodeId);
            Assert.Equal(k.DoctorMasterId, baris.DoctorId);
            Assert.Equal(PhysicianVisitRole.Dpjp, baris.VisitRole);
            Assert.Equal("DPJP", baris.VisitRoleName);
            Assert.Equal(waktuKedatangan, baris.VisitDateTime, TimeSpan.FromSeconds(1));
            Assert.Equal(k.DokterUserId, baris.RecordedByUserId);
            Assert.NotNull(baris.RecordedByName);
            Assert.False(baris.HasLinkedDocument);

            // Waktu pencatatan berbeda dari waktu kedatangan, dan keduanya tersimpan.
            Assert.NotNull(baris.RecordedAt);
            Assert.True(baris.RecordedAt > baris.VisitDateTime);

            // Nol catatan dokter dibuat oleh jalur ini.
            Assert.Empty(context.Set<TrxDoctorConsultation>());
        }

        /// <summary>
        /// <c>BE-RWI-048 AC 1</c> — kejadian visite tetap terbaca pada waktu kedatangannya
        /// walaupun catatan menyusul kemudian, dan tautan dokumennya bersifat opsional.
        /// </summary>
        [Fact]
        public async Task VisitePukulLebihAwal_TetapTerbacaSaatCatatanMenyusulKemudian()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var waktuKedatangan = DateTime.UtcNow.AddHours(-3);

            var visite = Isi<PhysicianVisitResponse>(
                await BuatControllerVisite(context, k.DokterUserId)
                    .RecordVisit(Permintaan(k, waktuKedatangan)));

            var catatan = await BuatControllerCatatan(context, k.DokterUserId)
                .CreateConsultation(new CreateDoctorConsultationRequest
                {
                    EncounterId = k.EncounterId,
                    DoctorId = k.DoctorMasterId,
                    Subjective = "Keluhan berkurang"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(catatan));

            var idCatatan = context.Set<TrxDoctorConsultation>().Single().Id;

            var tautan = await BuatControllerVisite(context, k.DokterUserId)
                .UpdateLinks(visite.Id, new UpdatePhysicianVisitLinksRequest
                {
                    ConsultationId = idCatatan
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(tautan));

            using var verifikasi = database.CreateContext();

            var tersimpan = verifikasi.Set<CliPhysicianVisit>().Single();

            Assert.Equal(idCatatan, tersimpan.ConsultationId);

            // Waktu kedatangan TIDAK bergeser hanya karena catatannya menyusul.
            Assert.Equal(waktuKedatangan, tersimpan.VisitDateTime, TimeSpan.FromSeconds(1));
        }

        // =====================================================================
        // Kriteria 2 dan 4 — hitungan diturunkan dari kejadian, bukan dari catatan
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-048 AC 2</c>, <c>INV-DOK-07</c>, <c>RWI-AC-151</c> — tiga catatan tanpa satu
        /// pun kejadian visite menghasilkan hitungan <b>nol</b>.
        /// </summary>
        /// <remarks>
        /// Inilah larangan yang paling mudah dilanggar tanpa sadar: menghitung visite dari SOAP
        /// terasa masuk akal dan gratis, tetapi angkanya salah pada dua arah sekaligus.
        /// </remarks>
        [Fact]
        public async Task TigaCatatanTanpaKejadianVisite_MenghasilkanHitunganNol()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            for (var i = 0; i < 3; i++)
            {
                var catatan = await BuatControllerCatatan(context, k.DokterUserId)
                    .CreateConsultation(new CreateDoctorConsultationRequest
                    {
                        EncounterId = k.EncounterId,
                        DoctorId = k.DoctorMasterId,
                        ClinicalDateTime = DateTime.UtcNow.AddHours(-6 + i),
                        Subjective = $"Catatan ke-{i + 1}"
                    });

                Assert.Equal(200, ControllerTestHarness.KodeStatus(catatan));
            }

            Assert.Equal(3, context.Set<TrxDoctorConsultation>().Count());

            var rekap = Isi<PhysicianVisitSummaryResponse>(
                await BuatControllerVisite(context, k.DokterUserId)
                    .GetSummary(inpEpisodeId: k.EpisodeId));

            Assert.Equal(0, rekap.RecordedCount);
            Assert.Equal(0, rekap.TotalCount);
            Assert.Null(rekap.LastVisitDateTime);
        }

        /// <summary>
        /// <c>BE-RWI-048 AC 4</c>, <c>RWI-AC-154</c>, <c>RWI-DEC-085</c> — dua visite nyata pada
        /// tanggal yang sama menghasilkan <b>dua</b> baris dan hitungan <b>dua</b>.
        /// </summary>
        /// <remarks>
        /// Menolak visite kedua pada hari yang sama dilarang. Dokter yang pagi memeriksa lalu
        /// sore dipanggil lagi karena pasien memburuk benar-benar datang dua kali, dan
        /// menghapus kunjungan kedua menghapus fakta klinis.
        /// </remarks>
        [Fact]
        public async Task DuaVisiteNyataPadaTanggalYangSama_MenghasilkanDuaBarisDanHitunganDua()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var pagi = DateTime.UtcNow.Date.AddHours(7).AddMinutes(40);
            var sore = DateTime.UtcNow.Date.AddHours(16).AddMinutes(10);

            // Bila uji berjalan sebelum pukul 16.10 UTC, waktu sore masih di masa depan dan
            // ditolak VAL-DOK-16. Kedua waktu karena itu digeser ke hari sebelumnya.
            if (sore > DateTime.UtcNow)
            {
                pagi = pagi.AddDays(-1);
                sore = sore.AddDays(-1);
            }

            var pertama = await BuatControllerVisite(context, k.DokterUserId)
                .RecordVisit(Permintaan(k, pagi));
            var kedua = await BuatControllerVisite(context, k.DokterUserId)
                .RecordVisit(Permintaan(k, sore));

            Assert.Equal(201, ControllerTestHarness.KodeStatus(pertama));
            Assert.Equal(201, ControllerTestHarness.KodeStatus(kedua));

            using var verifikasi = database.CreateContext();
            Assert.Equal(2, verifikasi.Set<CliPhysicianVisit>().Count());

            var rekap = Isi<PhysicianVisitSummaryResponse>(
                await BuatControllerVisite(context, k.DokterUserId)
                    .GetSummary(inpEpisodeId: k.EpisodeId));

            Assert.Equal(2, rekap.RecordedCount);
            Assert.Equal(1, rekap.DistinctDoctorCount);
        }

        // =====================================================================
        // Kriteria 3 dan 5 — kunci permintaan
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-048 AC 3</c>, <c>VAL-DOK-17</c>, <c>RWI-AC-152</c> — dua pengiriman dengan
        /// kunci yang sama menghasilkan <b>satu</b> kejadian dengan identitas yang sama, dan
        /// permintaan kedua dijawab <c>200</c>, bukan <c>409</c>.
        /// </summary>
        /// <remarks>
        /// Kode <c>200</c>, bukan <c>409</c>. Bagi dokter yang jaringannya terputus lalu menekan
        /// Simpan sekali lagi, hasilnya memang berhasil — kejadiannya sudah tersimpan. Menjawab
        /// <c>409</c> akan membuatnya mengira pencatatannya gagal, lalu ia mencatat ulang dengan
        /// kunci baru dan lahirlah kunjungan kedua yang tidak pernah terjadi.
        /// </remarks>
        [Fact]
        public async Task DuaPengirimanBerkunciSama_MenghasilkanSatuKejadianDanKode200()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var kunci = "TOMBOL-TERTEKAN-DUA-KALI";
            var waktu = DateTime.UtcNow.AddHours(-1);

            var pertama = await BuatControllerVisite(context, k.DokterUserId)
                .RecordVisit(Permintaan(k, waktu, kunci));
            var kedua = await BuatControllerVisite(context, k.DokterUserId)
                .RecordVisit(Permintaan(k, waktu, kunci));

            Assert.Equal(201, ControllerTestHarness.KodeStatus(pertama));
            Assert.Equal(200, ControllerTestHarness.KodeStatus(kedua));

            var identitasPertama = Isi<PhysicianVisitResponse>(pertama).Id;
            var identitasKedua = Isi<PhysicianVisitResponse>(kedua).Id;

            Assert.Equal(identitasPertama, identitasKedua);

            using var verifikasi = database.CreateContext();
            Assert.Single(verifikasi.Set<CliPhysicianVisit>());
        }

        /// <summary>
        /// <c>BE-RWI-048 AC 5</c>, <c>VAL-DOK-27</c> — kunci permintaan kosong ditolak
        /// <c>400</c>.
        /// </summary>
        /// <remarks>
        /// Tanpa kunci, <c>INV-DOK-06</c> tidak dapat dijamin sama sekali: tidak ada cara
        /// membedakan kiriman ulang dari kunjungan kedua yang sungguhan.
        /// </remarks>
        [Fact]
        public async Task KunciPermintaanKosong_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var permintaan = Permintaan(k, DateTime.UtcNow.AddHours(-1));
            permintaan.IdempotencyKey = "   ";

            var hasil = await BuatControllerVisite(context, k.DokterUserId).RecordVisit(permintaan);

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Contains("Kunci permintaan", ControllerTestHarness.Pesan(hasil)!);

            using var verifikasi = database.CreateContext();
            Assert.Empty(verifikasi.Set<CliPhysicianVisit>());
        }

        // =====================================================================
        // Kriteria 6 — hanya dokter yang mencatat visite
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-048 AC 6</c>, <c>VAL-DOK-08</c> — perawat ditolak <c>403</c> saat mencoba
        /// mencatat visite.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Penolakannya diturunkan dari <b>data</b>: pengguna ini tidak tertaut ke satu baris
        /// dokter pun. Tidak ada satu baris kode pun yang menyebut kata "perawat", nama peran,
        /// atau <c>UserType</c> — hardcode semacam itu dilarang, dan lagi pula akan salah pada
        /// rumah sakit yang menamai perannya berbeda.
        /// </para>
        /// <para>
        /// Kebijakan pencatatan visite <b>atas nama</b> dokter belum ada, sehingga bawaan yang
        /// aman berlaku: hanya dokter yang bersangkutan yang mencatat kunjungannya sendiri.
        /// </para>
        /// </remarks>
        [Fact]
        public async Task PerawatMencatatVisite_Ditolak403()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var hasil = await BuatControllerVisite(context, perawat.Id)
                .RecordVisit(Permintaan(k, DateTime.UtcNow.AddHours(-1)));

            Assert.Equal(403, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal("Visite hanya dapat dicatat dokter.", ControllerTestHarness.Pesan(hasil));

            using var verifikasi = database.CreateContext();
            Assert.Empty(verifikasi.Set<CliPhysicianVisit>());
        }

        // =====================================================================
        // Penjaga konteks perawatan
        // =====================================================================

        /// <summary>
        /// <c>VAL-DOK-16</c> — waktu visite yang melewati waktu sekarang ditolak <c>400</c>.
        /// </summary>
        [Fact]
        public async Task WaktuVisiteDiMasaDepan_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var hasil = await BuatControllerVisite(context, k.DokterUserId)
                .RecordVisit(Permintaan(k, DateTime.UtcNow.AddHours(3)));

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Contains("melewati waktu sekarang", ControllerTestHarness.Pesan(hasil)!);
        }

        /// <summary>
        /// <c>VAL-DOK-03</c> — perawatan yang sudah ditutup menolak kejadian visite baru
        /// <c>422</c>.
        /// </summary>
        [Fact]
        public async Task PerawatanTertutup_MenolakKejadianVisiteBaru422()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context, InpEpisodeStatus.Closed);

            var hasil = await BuatControllerVisite(context, k.DokterUserId)
                .RecordVisit(Permintaan(k, DateTime.UtcNow.AddHours(-1)));

            Assert.Equal(422, ControllerTestHarness.KodeStatus(hasil));
        }

        /// <summary>
        /// <c>VAL-DOK-06</c> — dokter yang tidak berwenang atas pasien itu ditolak <c>403</c>.
        /// </summary>
        [Fact]
        public async Task DokterTanpaPenugasanPadaPerawatan_Ditolak403()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context, denganPenugasanDpjp: false);

            var hasil = await BuatControllerVisite(context, k.DokterUserId)
                .RecordVisit(Permintaan(k, DateTime.UtcNow.AddHours(-1)));

            Assert.Equal(403, ControllerTestHarness.KodeStatus(hasil));
        }
    }
}
