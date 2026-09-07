using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-051</c> — tindakan dokter tercatat dan tagihannya tidak
    /// pernah ganda.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Urutan yang tidak boleh dibalik.</b> Catatan klinis disimpan lebih dulu; fakta ke
    /// Billing diterbitkan sesudahnya — <c>INV-DOK-09</c>. Kegagalan sistem keuangan
    /// <b>tidak pernah</b> menghapus bukti bahwa tindakan medis benar-benar terjadi. Membalik
    /// urutannya berarti tindakan yang sudah dikerjakan pada pasien hilang dari rekam medis
    /// hanya karena jaringan ke Billing sedang putus.
    /// </para>
    /// <para>
    /// <b>Yang paling mahal bila salah adalah tagihan ganda.</b> Percobaan ulang karena jaringan
    /// terputus tidak boleh melahirkan tindakan kedua maupun fakta kedua, karena keduanya
    /// berujung pada pasien membayar dua kali untuk satu tindakan.
    /// </para>
    /// </remarks>
    public class InpatientProcedureTests
    {
        private static PatientProcedureController BuatControllerTindakan(
            ApplicationDbContext c,
            Guid actorUserId,
            ApplicationDbContext? konteksBilling = null) =>
            new PatientProcedureController(
                c,
                new EncounterInsuranceService(c),
                new InsuranceCoverageService(c, new EncounterInsuranceService(c)),
                new ClinicalMilestoneFactProducer(
                    c,
                    new BillingFolioService(konteksBilling ?? c),
                    ControllerTestHarness.BuatLoggerService(actorUserId)),
                new ClinicalDocumentIntegrityService(c),
                new InpatientClinicalContextService(c),
                ControllerTestHarness.BuatLoggerService(actorUserId))
                .DenganPengguna(actorUserId);

        private static T Isi<T>(IActionResult hasil)
        {
            var objek = Assert.IsAssignableFrom<ObjectResult>(hasil);
            var pembungkus = Assert.IsType<ApiResponse<T>>(objek.Value);

            Assert.NotNull(pembungkus.Data);
            return pembungkus.Data!;
        }

        private static CreatePatientProcedureRequest Permintaan(
            RawatInapTestData.Konteks k,
            Guid consultationId,
            Guid procedureId,
            Guid? patientId = null,
            Guid? episodeId = null,
            Guid? physicianVisitId = null,
            string? kunci = null,
            bool langsungDikerjakan = false) => new()
            {
                EncounterId = k.EncounterId,
                ConsultationId = consultationId,
                ProcedureId = procedureId,
                PatientId = patientId,
                InpEpisodeId = episodeId,
                PhysicianVisitId = physicianVisitId,
                IdempotencyKey = kunci,
                Quantity = 1,
                UnitNameSnapshot = "TINDAKAN",
                ExecuteImmediately = langsungDikerjakan
            };

        // =====================================================================
        // Kriteria 1 — pasangan pasien dan kunjungan yang tidak cocok
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-051 AC 1</c> — tindakan untuk pasangan pasien dan kunjungan yang tidak
        /// cocok ditolak <c>400</c>.
        /// </summary>
        /// <remarks>
        /// Layar dokter rawat inap membuka satu pasien lalu mengirim tindakan. Bila pasien pada
        /// layar dan pasien pada kunjungan berbeda — misalnya karena dokter berpindah pasien di
        /// tengah pengisian — yang terjadi adalah tindakan tercatat pada rekam medis orang lain.
        /// Penolakannya karena itu bukan kerewelan validasi, melainkan penjaga keselamatan.
        /// </remarks>
        [Fact]
        public async Task PasienDanKunjunganTidakCocok_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var catatan = TindakanTestData.BuatCatatanInduk(
                context, k.EncounterId, k.PatientId, k.DoctorMasterId, k.ServiceUnitId, k.EpisodeId);
            var master = TindakanTestData.BuatTindakanMaster(context);

            var pasienLain = RawatInapTestData.SiapkanPerawatan(context);

            var hasil = await BuatControllerTindakan(context, k.DokterUserId).CreateProcedure(
                Permintaan(k, catatan.Id, master.Id, patientId: pasienLain.PatientId));

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Contains("tidak sesuai dengan pasien", ControllerTestHarness.Pesan(hasil)!);

            using var verifikasi = database.CreateContext();
            Assert.Empty(verifikasi.Set<TrxPatientProcedure>());
        }

        /// <summary>
        /// <c>VAL-DOK-26</c> — penanda perawatan yang tidak cocok dengan kunjungannya ditolak
        /// <c>400</c>.
        /// </summary>
        [Fact]
        public async Task PenandaPerawatanTidakCocok_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawatanLain = RawatInapTestData.SiapkanPerawatan(context);

            var catatan = TindakanTestData.BuatCatatanInduk(
                context, k.EncounterId, k.PatientId, k.DoctorMasterId, k.ServiceUnitId, k.EpisodeId);
            var master = TindakanTestData.BuatTindakanMaster(context);

            var hasil = await BuatControllerTindakan(context, k.DokterUserId).CreateProcedure(
                Permintaan(k, catatan.Id, master.Id, episodeId: perawatanLain.EpisodeId));

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));

            using var verifikasi = database.CreateContext();
            Assert.Empty(verifikasi.Set<TrxPatientProcedure>());
        }

        // =====================================================================
        // Kriteria 2 — percobaan ulang tidak menggandakan apa pun
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-051 AC 2</c> — percobaan ulang dengan kunci yang sama tidak menghasilkan
        /// tindakan maupun fakta klinis ganda.
        /// </summary>
        /// <remarks>
        /// Dua hal diperiksa sekaligus, karena keduanya adalah dua sumber tagihan ganda yang
        /// berbeda: baris tindakan yang berganda, dan fakta klinis yang berganda walaupun
        /// tindakannya tunggal.
        /// </remarks>
        [Fact]
        public async Task PercobaanUlangBerkunciSama_TidakMenghasilkanTindakanMaupunFaktaGanda()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var catatan = TindakanTestData.BuatCatatanInduk(
                context, k.EncounterId, k.PatientId, k.DoctorMasterId, k.ServiceUnitId, k.EpisodeId);
            var master = TindakanTestData.BuatTindakanMaster(context);

            const string kunci = "TOMBOL-TINDAKAN-TERTEKAN-DUA-KALI";

            var pertama = await BuatControllerTindakan(context, k.DokterUserId).CreateProcedure(
                Permintaan(k, catatan.Id, master.Id, episodeId: k.EpisodeId, kunci: kunci));
            var kedua = await BuatControllerTindakan(context, k.DokterUserId).CreateProcedure(
                Permintaan(k, catatan.Id, master.Id, episodeId: k.EpisodeId, kunci: kunci));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(pertama));
            Assert.Equal(200, ControllerTestHarness.KodeStatus(kedua));

            Assert.Equal(
                Isi<PatientProcedureCreateResponse>(pertama).Id,
                Isi<PatientProcedureCreateResponse>(kedua).Id);

            var idTindakan = Isi<PatientProcedureCreateResponse>(pertama).Id;

            Assert.Single(context.Set<TrxPatientProcedure>());

            // Dieksekusi dua kali. Fakta klinisnya tetap satu baris, karena mesin fakta
            // mengenali penerbitan identik sebagai pemutaran ulang.
            await BuatControllerTindakan(context, k.DokterUserId)
                .ExecuteProcedure(idTindakan, new ExecutePatientProcedureRequest());
            await BuatControllerTindakan(context, k.DokterUserId)
                .ExecuteProcedure(idTindakan, new ExecutePatientProcedureRequest());

            using var verifikasi = database.CreateContext();

            Assert.Single(verifikasi.Set<TrxPatientProcedure>());
            Assert.Single(verifikasi.Set<CliClinicalMilestoneFact>()
                .Where(x => x.SourceAggregateId == idTindakan));
        }

        // =====================================================================
        // Kriteria 3 — Billing gagal, catatan klinis tetap tersimpan
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-051 AC 3</c>, <c>INV-DOK-09</c> — saat Billing gagal dihubungi, catatan
        /// tindakan <b>tetap tersimpan</b> dan hasil penerbitannya tercatat gagal.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Kegagalannya dipaksa dengan cara yang paling mendekati kejadian nyata: jalur menuju
        /// Billing diputus, sehingga pemanggilannya melempar. Yang dibuktikan bukan pesan
        /// galatnya, melainkan <b>keadaan sesudahnya</b> — tindakan tetap berstatus selesai,
        /// dan baris fakta menyimpan keadaan pengiriman yang tidak berhasil.
        /// </para>
        /// <para>
        /// Keadaan pengiriman <b>bukan</b> status tindakan. Menyimpannya sebagai status tindakan
        /// akan membuat kegagalan sistem keuangan terlihat seperti tindakan medis yang batal.
        /// </para>
        /// </remarks>
        [Fact]
        public async Task BillingGagalDihubungi_CatatanTindakanTetapTersimpanDanPenerbitanTercatatGagal()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var catatan = TindakanTestData.BuatCatatanInduk(
                context, k.EncounterId, k.PatientId, k.DoctorMasterId, k.ServiceUnitId, k.EpisodeId);

            var tindakan = TindakanTestData.BuatTindakan(
                context, k.EncounterId, catatan.Id, k.PatientId, k.DoctorMasterId,
                k.ServiceUnitId, k.EpisodeId);

            // Jalur menuju Billing diputus.
            var konteksBillingTerputus = database.CreateContext();
            konteksBillingTerputus.Dispose();

            var hasil = await BuatControllerTindakan(context, k.DokterUserId, konteksBillingTerputus)
                .ExecuteProcedure(tindakan.Id, new ExecutePatientProcedureRequest());

            // Permintaannya tetap berhasil bagi dokter: tindakannya memang sudah dikerjakan.
            // Kegagalan Billing bukan kegagalan klinis - INV-DOK-09 - sehingga jawabannya
            // sengaja tetap berhasil, dan yang menyimpan kegagalannya adalah baris fakta.
            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            using var verifikasi = database.CreateContext();

            var sesudah = verifikasi.Set<TrxPatientProcedure>().Single(x => x.Id == tindakan.Id);

            Assert.Equal(PatientProcedureStatus.Completed, sesudah.ProcedureStatus);
            Assert.True(sesudah.IsExecuted);
            Assert.NotNull(sesudah.PerformedAt);

            var fakta = verifikasi.Set<CliClinicalMilestoneFact>()
                .Single(x => x.SourceAggregateId == tindakan.Id);

            Assert.NotEqual(ClinicalFactDispatchStatus.Dispatched, fakta.DispatchStatus);
            Assert.False(string.IsNullOrWhiteSpace(fakta.BillingOutcomeCode));
        }

        // =====================================================================
        // Kriteria 4 dan 5 — dua jalur pencatatan, tautan visite opsional
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-051 AC 4</c> — kedua jalur pencatatan dipertahankan: direncanakan lebih
        /// dulu, atau langsung dicatat dikerjakan.
        /// </summary>
        /// <remarks>
        /// Keduanya nyata di lapangan. Tindakan besar direncanakan dan dijadwalkan; tindakan
        /// kecil di samping tempat tidur dikerjakan lebih dulu lalu dicatat. Memaksa salah satu
        /// jalur akan membuat dokter memalsukan alur demi bisa menyimpan.
        /// </remarks>
        [Fact]
        public async Task KeduaJalurPencatatanTindakan_TetapBerjalan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var catatan = TindakanTestData.BuatCatatanInduk(
                context, k.EncounterId, k.PatientId, k.DoctorMasterId, k.ServiceUnitId, k.EpisodeId);

            var direncanakan = await BuatControllerTindakan(context, k.DokterUserId).CreateProcedure(
                Permintaan(k, catatan.Id, TindakanTestData.BuatTindakanMaster(context).Id,
                           episodeId: k.EpisodeId));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(direncanakan));
            Assert.Equal(
                PatientProcedureStatus.Planned,
                Isi<PatientProcedureCreateResponse>(direncanakan).ProcedureStatus);

            var langsung = await BuatControllerTindakan(context, k.DokterUserId).CreateProcedure(
                Permintaan(k, catatan.Id, TindakanTestData.BuatTindakanMaster(context).Id,
                           episodeId: k.EpisodeId, langsungDikerjakan: true));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(langsung));
            Assert.Equal(
                PatientProcedureStatus.Completed,
                Isi<PatientProcedureCreateResponse>(langsung).ProcedureStatus);

            using var verifikasi = database.CreateContext();
            Assert.Equal(2, verifikasi.Set<TrxPatientProcedure>().Count());
        }

        /// <summary>
        /// <c>BE-RWI-051 AC 5</c> — tautan ke kejadian visite bersifat opsional, dan ketika
        /// dikirim, ia wajib milik kunjungan yang sama.
        /// </summary>
        /// <remarks>
        /// Opsional karena tidak setiap tindakan lahir dari kunjungan dokter yang tercatat.
        /// Dijaga kecocokannya karena tautan ke kejadian milik pasien lain akan membuat riwayat
        /// visite menampilkan tindakan yang tidak pernah terjadi pada kunjungan itu.
        /// </remarks>
        [Fact]
        public async Task TautanKejadianVisite_OpsionalTetapiWajibCocokKetikaDikirim()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var catatan = TindakanTestData.BuatCatatanInduk(
                context, k.EncounterId, k.PatientId, k.DoctorMasterId, k.ServiceUnitId, k.EpisodeId);

            // Tanpa tautan - tetap diterima.
            var tanpaTautan = await BuatControllerTindakan(context, k.DokterUserId).CreateProcedure(
                Permintaan(k, catatan.Id, TindakanTestData.BuatTindakanMaster(context).Id,
                           episodeId: k.EpisodeId));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(tanpaTautan));

            // Dengan tautan milik kunjungan yang sama - diterima, dan tersimpan.
            var visite = Isi<PhysicianVisitResponse>(
                await PhysicianVisitRecordingTests.BuatControllerVisite(context, k.DokterUserId)
                    .RecordVisit(PhysicianVisitRecordingTests.Permintaan(
                        k, DateTime.UtcNow.AddHours(-1))));

            var denganTautan = await BuatControllerTindakan(context, k.DokterUserId).CreateProcedure(
                Permintaan(k, catatan.Id, TindakanTestData.BuatTindakanMaster(context).Id,
                           episodeId: k.EpisodeId, physicianVisitId: visite.Id));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(denganTautan));

            var idTertaut = Isi<PatientProcedureCreateResponse>(denganTautan).Id;

            using var verifikasi = database.CreateContext();

            Assert.Equal(
                visite.Id,
                verifikasi.Set<TrxPatientProcedure>().Single(x => x.Id == idTertaut).PhysicianVisitId);

            // Dengan tautan milik kunjungan lain - ditolak.
            var pasienLain = RawatInapTestData.SiapkanPerawatan(context);

            var visiteLain = Isi<PhysicianVisitResponse>(
                await PhysicianVisitRecordingTests.BuatControllerVisite(context, pasienLain.DokterUserId)
                    .RecordVisit(PhysicianVisitRecordingTests.Permintaan(
                        pasienLain, DateTime.UtcNow.AddHours(-1))));

            var salahTautan = await BuatControllerTindakan(context, k.DokterUserId).CreateProcedure(
                Permintaan(k, catatan.Id, TindakanTestData.BuatTindakanMaster(context).Id,
                           episodeId: k.EpisodeId, physicianVisitId: visiteLain.Id));

            Assert.Equal(400, ControllerTestHarness.KodeStatus(salahTautan));
            Assert.Contains("bukan milik kunjungan yang sama", ControllerTestHarness.Pesan(salahTautan)!);
        }

        /// <summary>
        /// <c>BE-RWI-051</c>, <c>api-contract.md</c> bagian 5 — tindakan satu perawatan terbaca
        /// dari konteks perawatannya.
        /// </summary>
        [Fact]
        public async Task TindakanSatuPerawatan_TerbacaDariKonteksPerawatan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var catatan = TindakanTestData.BuatCatatanInduk(
                context, k.EncounterId, k.PatientId, k.DoctorMasterId, k.ServiceUnitId, k.EpisodeId);

            for (var i = 0; i < 3; i++)
            {
                var dibuat = await BuatControllerTindakan(context, k.DokterUserId).CreateProcedure(
                    Permintaan(k, catatan.Id, TindakanTestData.BuatTindakanMaster(context).Id,
                               episodeId: k.EpisodeId));

                Assert.Equal(200, ControllerTestHarness.KodeStatus(dibuat));
            }

            var daftar = Isi<PagedResult<PatientProcedureResponse>>(
                await BuatControllerTindakan(context, k.DokterUserId).GetByEpisode(k.EpisodeId));

            Assert.Equal(3, daftar.TotalData);
            Assert.All(daftar.Items, x => Assert.Equal(k.EncounterId, x.EncounterId));
        }
    }
}
