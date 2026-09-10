using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-042</c> — konteks perawatan pada resep dan pesanan
    /// penunjang — beserta bagian <c>BE-RWI-043</c> yang menyentuh resep.
    /// </summary>
    public class SupportingOrderAndPrescriptionContextTests
    {
        private static LabOrderService LayananPesananLab(ApplicationDbContext context)
        {
            var accessor = new HttpContextAccessor
            {
                HttpContext = ControllerTestHarness.BuatHttpContext(Guid.NewGuid())
            };

            var logger = new LoggerService(NullLogger<LoggerService>.Instance, accessor);

            return new LabOrderService(
                context,
                new LabSpecimenService(
                    context,
                    new ClinicalMilestoneFactProducer(context, new BillingFolioService(context), logger),
                    accessor,
                    logger),
                accessor,
                logger);
        }

        private static MstProcedure BuatProcedureMaster(ApplicationDbContext context)
        {
            var pembeda = Guid.NewGuid().ToString("N")[..8];

            var procedure = new MstProcedure
            {
                ProcedureCode = $"LAB-{pembeda}",
                ProcedureName = "Pemeriksaan Uji",
                ProcedureType = "Laboratory"
            };

            context.Set<MstProcedure>().Add(procedure);
            context.SaveChanges();
            return procedure;
        }

        // =====================================================================
        // BE-RWI-042 AC 4 - daftar pesanan laboratorium dapat disaring kunjungan
        // =====================================================================

        /// <summary>
        /// `BE-RWI-042 AC 4` — daftar pesanan laboratorium dapat disaring kunjungan, dan pesanan
        /// milik kunjungan lain tidak ikut terbaca.
        /// </summary>
        /// <remarks>
        /// Sebelum ini penyaringnya tidak ada, sehingga layar yang hanya membutuhkan pesanan satu
        /// kunjungan terpaksa mengambil seluruh pesanan rumah sakit lalu menyaringnya sendiri.
        /// Uji ini juga membuktikan pemanggil lama yang tidak mengirim penyaring tetap menerima
        /// daftar penuh seperti sebelumnya.
        ///
        /// Sejak digabung dengan <c>BE-LAB-18</c>, penyaringnya tidak lagi berdiri sendiri
        /// sebagai parameter lepas melainkan menjadi ruas <c>EncounterId</c> pada
        /// <see cref="LabOrderPagedQuery"/>. Yang dijaga tetap sama: pesanan kunjungan lain
        /// tidak boleh ikut terbaca.
        /// </remarks>
        [Fact]
        public async Task DaftarPesananLaboratorium_DapatDisaringKunjungan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var perawatanA = RawatInapTestData.SiapkanPerawatan(context);
            var perawatanB = RawatInapTestData.SiapkanPerawatan(context);
            var procedure = BuatProcedureMaster(context);

            context.LabOrders.AddRange(
                new LabOrder
                {
                    EncounterId = perawatanA.EncounterId,
                    InpEpisodeId = perawatanA.EpisodeId,
                    ProcedureId = procedure.Id
                },
                new LabOrder
                {
                    EncounterId = perawatanB.EncounterId,
                    InpEpisodeId = perawatanB.EpisodeId,
                    ProcedureId = procedure.Id
                });
            await context.SaveChangesAsync();

            var service = LayananPesananLab(context);

            var seluruhnya = await service.GetListAsync(new LabOrderPagedQuery());
            var milikA = await service.GetListAsync(
                new LabOrderPagedQuery { EncounterId = perawatanA.EncounterId });
            var milikB = await service.GetListAsync(
                new LabOrderPagedQuery { EncounterId = perawatanB.EncounterId });

            Assert.Equal(2, seluruhnya.TotalData);

            Assert.Single(milikA.Items);
            Assert.Equal(perawatanA.EncounterId, milikA.Items[0].EncounterId);

            Assert.Single(milikB.Items);
            Assert.Equal(perawatanB.EncounterId, milikB.Items[0].EncounterId);

            // Pesanan perawatan A tidak pernah terbaca dari perawatan B.
            Assert.DoesNotContain(milikB.Items, x => x.EncounterId == perawatanA.EncounterId);
        }

        /// <summary>
        /// `BE-RWI-042 AC 1` — konteks perawatan tersimpan pada pesanan laboratorium, sehingga
        /// kepemilikan perawatannya dapat dibuktikan tanpa penelusuran berlapis.
        /// </summary>
        [Fact]
        public async Task PesananLaboratorium_MenyimpanKonteksPerawatan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var procedure = BuatProcedureMaster(context);

            context.LabOrders.Add(new LabOrder
            {
                EncounterId = k.EncounterId,
                InpEpisodeId = k.EpisodeId,
                ProcedureId = procedure.Id
            });
            await context.SaveChangesAsync();

            var tersimpan = await context.LabOrders.SingleAsync(x => x.EncounterId == k.EncounterId);

            Assert.Equal(k.EpisodeId, tersimpan.InpEpisodeId);
        }

        // =====================================================================
        // BE-RWI-043 AC 2 dan 4 - batas satu resep aktif per catatan
        // =====================================================================

        /// <summary>
        /// `BE-RWI-043 AC 2 dan AC 4` — batas satu resep aktif per catatan dilepas bagi resep
        /// yang menempel pada perawatan rawat inap, dan <b>tetap berlaku</b> bagi resep tanpa
        /// konteks perawatan — bentuk rawat jalan dan medical check-up.
        /// </summary>
        /// <remarks>
        /// Dibuktikan pada lapisan penyimpanan. Penjagaan yang selama ini menolak resep kedua
        /// adalah unique index pada catatan dokternya; selama index itu masih berlaku penuh,
        /// pelonggaran di lapisan aplikasi saja hanya akan mengubah penolakan yang rapi menjadi
        /// kegagalan sistem saat penyimpanan.
        /// </remarks>
        [Fact]
        public async Task ResepKedua_DiterimaSaatAdaKonteksPerawatan_DitolakSaatTidakAda()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var aktor = RekamMedisTestData.BuatPengguna(context, "dokter");

            var catatanRawatInap = BuatCatatan(context, k, aktor.Id, k.EpisodeId);
            var catatanRawatJalan = BuatCatatan(context, k, aktor.Id, episodeId: null);

            PhmPrescription Resep(TrxDoctorConsultation catatan, Guid? episodeId) => new()
            {
                PrescriptionNumber = $"RSP-{Guid.NewGuid():N}"[..20],
                EncounterId = k.EncounterId,
                ConsultationId = catatan.Id,
                InpEpisodeId = episodeId,
                PatientId = k.PatientId,
                DoctorId = k.DoctorMasterId,
                ServiceUnitId = k.ServiceUnitId,
                PrescriptionOrderType = PrescriptionOrderType.Daily,
                PrescriptionDateTime = DateTime.UtcNow,
                CreateBy = aktor.Id
            };

            // Resep pertama dan kedua pada satu catatan rawat inap: keduanya tersimpan.
            context.Set<PhmPrescription>().Add(Resep(catatanRawatInap, k.EpisodeId));
            await context.SaveChangesAsync();

            context.Set<PhmPrescription>().Add(Resep(catatanRawatInap, k.EpisodeId));
            await context.SaveChangesAsync();

            Assert.Equal(2, await context.Set<PhmPrescription>()
                .CountAsync(x => x.ConsultationId == catatanRawatInap.Id));

            // Resep tanpa konteks perawatan tetap dibatasi satu per catatan.
            context.Set<PhmPrescription>().Add(Resep(catatanRawatJalan, null));
            await context.SaveChangesAsync();

            context.Set<PhmPrescription>().Add(Resep(catatanRawatJalan, null));

            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }

        /// <summary>
        /// Jenis resep obat pulang tersaring tersendiri menurut jenisnya — `AC-CAP023-03`.
        /// </summary>
        [Fact]
        public async Task ResepObatPulang_TersaringMenurutJenisnya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var aktor = RekamMedisTestData.BuatPengguna(context, "dokter");
            var catatan = BuatCatatan(context, k, aktor.Id, k.EpisodeId);

            foreach (var jenis in new[]
                     {
                         PrescriptionOrderType.Routine,
                         PrescriptionOrderType.Daily,
                         PrescriptionOrderType.Discharge
                     })
            {
                context.Set<PhmPrescription>().Add(new PhmPrescription
                {
                    PrescriptionNumber = $"RSP-{Guid.NewGuid():N}"[..20],
                    EncounterId = k.EncounterId,
                    ConsultationId = catatan.Id,
                    InpEpisodeId = k.EpisodeId,
                    PatientId = k.PatientId,
                    DoctorId = k.DoctorMasterId,
                    ServiceUnitId = k.ServiceUnitId,
                    PrescriptionOrderType = jenis,
                    PrescriptionDateTime = DateTime.UtcNow,
                    CreateBy = aktor.Id
                });
                await context.SaveChangesAsync();
            }

            var obatPulang = await context.Set<PhmPrescription>()
                .Where(x => x.InpEpisodeId == k.EpisodeId &&
                            x.PrescriptionOrderType == PrescriptionOrderType.Discharge)
                .ToListAsync();

            Assert.Single(obatPulang);
            Assert.Equal(3, await context.Set<PhmPrescription>()
                .CountAsync(x => x.InpEpisodeId == k.EpisodeId));
        }

        private static PrescriptionController BuatControllerResep(
            ApplicationDbContext c, Guid actorUserId) =>
            new PrescriptionController(
                c,
                new EncounterInsuranceService(c),
                new PrescriptionNumberService(c),
                new PrescriptionSummaryService(c),
                new PrescriptionWorkflowService(c),
                new ClinicalMilestoneFactProducer(
                    c,
                    new BillingFolioService(c),
                    ControllerTestHarness.BuatLoggerService(actorUserId)),
                ControllerTestHarness.BuatLoggerService(actorUserId))
                .DenganPengguna(actorUserId);

        /// <summary>
        /// `BE-RWI-043 AC 2` — <b>lewat endpoint</b>. Resep kedua sepanjang satu perawatan rawat
        /// inap diterima, sedangkan resep aktif kedua tanpa konteks perawatan tetap ditolak
        /// `400` dengan kalimat yang sama persis seperti sebelum pelonggaran.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Uji inilah yang dulu tidak dapat ditulis.</b> Sampai <c>BE-RWI-050</c> selesai,
        /// jalur pemesanan resep rawat inap belum menyala, sehingga kriteria 2 hanya terbukti
        /// pada aturan aplikasi dan index basis data — bukan lewat permintaan yang benar-benar
        /// masuk. Sekarang keduanya dibuktikan pada lapisan yang sama, oleh controller yang
        /// sama, dalam satu uji.
        /// </para>
        /// <para>
        /// Kalimat penolakannya dibandingkan <b>utuh</b>, bukan sepotong. Pelonggaran ini
        /// menyentuh alur poliklinik yang sedang melayani pasien, dan perubahan satu huruf pun
        /// pada kalimat itu harus menggagalkan uji.
        /// </para>
        /// </remarks>
        [Fact]
        public async Task ResepKedua_DiterimaLewatEndpointRawatInap_DitolakLewatEndpointRawatJalan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var catatanRawatInap = BuatCatatan(context, k, k.DokterUserId, k.EpisodeId);
            var catatanRawatJalan = BuatCatatan(context, k, k.DokterUserId, episodeId: null);

            CreatePrescriptionRequest Permintaan(Guid consultationId) => new()
            {
                EncounterId = k.EncounterId,
                ConsultationId = consultationId,
                PrescriptionOrderType = PrescriptionOrderType.Daily
            };

            // Rawat inap: dua resep berturut-turut pada satu perawatan, keduanya diterima.
            var pertama = await BuatControllerResep(context, k.DokterUserId)
                .CreatePrescription(Permintaan(catatanRawatInap.Id));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(pertama));

            var kedua = await BuatControllerResep(context, k.DokterUserId)
                .CreatePrescription(Permintaan(catatanRawatInap.Id));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(kedua));

            using var pembaca = database.CreateContext();

            Assert.Equal(2, await pembaca.Set<PhmPrescription>()
                .CountAsync(x => x.ConsultationId == catatanRawatInap.Id));

            // Tanpa konteks perawatan: resep aktif kedua tetap ditolak, kalimatnya tak berubah.
            var rawatJalanPertama = await BuatControllerResep(context, k.DokterUserId)
                .CreatePrescription(Permintaan(catatanRawatJalan.Id));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(rawatJalanPertama));

            var rawatJalanKedua = await BuatControllerResep(context, k.DokterUserId)
                .CreatePrescription(Permintaan(catatanRawatJalan.Id));

            Assert.Equal(400, ControllerTestHarness.KodeStatus(rawatJalanKedua));
            Assert.Equal(
                "Konsultasi ini sudah memiliki resep aktif.",
                ControllerTestHarness.Pesan(rawatJalanKedua));

            using var pembacaKedua = database.CreateContext();

            Assert.Equal(1, await pembacaKedua.Set<PhmPrescription>()
                .CountAsync(x => x.ConsultationId == catatanRawatJalan.Id));
        }

        private static TrxDoctorConsultation BuatCatatan(
            ApplicationDbContext context,
            RawatInapTestData.Konteks k,
            Guid aktorUserId,
            Guid? episodeId)
        {
            var catatan = new TrxDoctorConsultation
            {
                ConsultationNumber = $"CON-{Guid.NewGuid():N}"[..20],
                EncounterId = k.EncounterId,
                InpEpisodeId = episodeId,
                PatientId = k.PatientId,
                DoctorId = k.DoctorMasterId,
                ServiceUnitId = k.ServiceUnitId,
                ConsultationDateTime = DateTime.UtcNow,
                ConsultationStatus = DoctorConsultationStatus.InProgress,
                StartedByUserId = aktorUserId,
                CreateBy = aktorUserId
            };

            context.Set<TrxDoctorConsultation>().Add(catatan);
            context.SaveChanges();
            return catatan;
        }
    }
}
