using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
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

            var seluruhnya = await service.GetListAsync();
            var milikA = await service.GetListAsync(perawatanA.EncounterId);
            var milikB = await service.GetListAsync(perawatanB.EncounterId);

            Assert.Equal(2, seluruhnya.Count);

            Assert.Single(milikA);
            Assert.Equal(perawatanA.EncounterId, milikA[0].EncounterId);

            Assert.Single(milikB);
            Assert.Equal(perawatanB.EncounterId, milikB[0].EncounterId);

            // Pesanan perawatan A tidak pernah terbaca dari perawatan B.
            Assert.DoesNotContain(milikB, x => x.EncounterId == perawatanA.EncounterId);
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

            TrxPrescription Resep(TrxDoctorConsultation catatan, Guid? episodeId) => new()
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
            context.Set<TrxPrescription>().Add(Resep(catatanRawatInap, k.EpisodeId));
            await context.SaveChangesAsync();

            context.Set<TrxPrescription>().Add(Resep(catatanRawatInap, k.EpisodeId));
            await context.SaveChangesAsync();

            Assert.Equal(2, await context.Set<TrxPrescription>()
                .CountAsync(x => x.ConsultationId == catatanRawatInap.Id));

            // Resep tanpa konteks perawatan tetap dibatasi satu per catatan.
            context.Set<TrxPrescription>().Add(Resep(catatanRawatJalan, null));
            await context.SaveChangesAsync();

            context.Set<TrxPrescription>().Add(Resep(catatanRawatJalan, null));

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
                context.Set<TrxPrescription>().Add(new TrxPrescription
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

            var obatPulang = await context.Set<TrxPrescription>()
                .Where(x => x.InpEpisodeId == k.EpisodeId &&
                            x.PrescriptionOrderType == PrescriptionOrderType.Discharge)
                .ToListAsync();

            Assert.Single(obatPulang);
            Assert.Equal(3, await context.Set<TrxPrescription>()
                .CountAsync(x => x.InpEpisodeId == k.EpisodeId));
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
