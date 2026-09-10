using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.Infrastructure;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-052</c> — pemeriksaan laboratorium dan radiologi
    /// dipesan dan hasilnya dibaca.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Nol tabel salinan hasil, dan itu keputusan keselamatan.</b> <c>RUL-DOK-02</c>
    /// melarang Rawat Inap menyimpan salinan hasil. Alasannya bukan kerapian data: hasil
    /// laboratorium dapat direvisi pemiliknya, dan salinan yang tidak ikut berubah menjadi
    /// angka basi di layar dokter yang tetap terlihat sah. Dokter yang mengambil keputusan
    /// dari angka basi adalah risiko keselamatan pasien, bukan masalah tampilan.
    /// </para>
    /// <para>
    /// <b>Penyaringnya perawatan, bukan pasien.</b> Pasien yang dirawat dua kali dalam sebulan
    /// memiliki dua rangkaian pesanan. Menyaring per pasien akan menampilkan hasil perawatan
    /// lama pada layar perawatan yang sedang berjalan — <c>INV-DOK-12</c>.
    /// </para>
    /// </remarks>
    public class InpatientSupportingOrderTests
    {
        private static LabOrderController BuatControllerLab(
            ApplicationDbContext c, Guid actorUserId)
        {
            var accessor = new HttpContextAccessor
            {
                HttpContext = ControllerTestHarness.BuatHttpContext(actorUserId)
            };

            var service = new LabOrderService(
                c,
                new LabSpecimenService(
                    c,
                    new ClinicalMilestoneFactProducer(
                        c,
                        new BillingFolioService(c),
                        ControllerTestHarness.BuatLoggerService(actorUserId)),
                    accessor,
                    ControllerTestHarness.BuatLoggerService(actorUserId)),
                accessor,
                ControllerTestHarness.BuatLoggerService(actorUserId));

            return new LabOrderController(service).DenganPengguna(actorUserId);
        }

        private static RadOrderController BuatControllerRad(
            ApplicationDbContext c, Guid actorUserId)
        {
            var accessor = new HttpContextAccessor
            {
                HttpContext = ControllerTestHarness.BuatHttpContext(actorUserId)
            };

            var service = new RadOrderService(
                c,
                accessor,
                ControllerTestHarness.BuatLoggerService(actorUserId));

            return new RadOrderController(service).DenganPengguna(actorUserId);
        }

        private static T Isi<T>(IActionResult hasil)
        {
            var objek = Assert.IsAssignableFrom<ObjectResult>(hasil);
            var pembungkus = Assert.IsType<ApiResponse<T>>(objek.Value);

            Assert.NotNull(pembungkus.Data);
            return pembungkus.Data!;
        }

        private static MstProcedure BuatTindakanPenunjang(
            ApplicationDbContext context,
            bool laboratorium)
        {
            var pembeda = Guid.NewGuid().ToString("N")[..8];

            var master = new MstProcedure
            {
                ProcedureCode = $"PNJ-{pembeda}",
                ProcedureName = laboratorium ? "Darah Lengkap Uji" : "Rontgen Dada Uji",
                ProcedureType = laboratorium ? "Laboratory" : "Radiology",
                IsLaboratory = laboratorium,
                IsRadiology = !laboratorium,
                IsDoctorAction = false
            };

            context.Set<MstProcedure>().Add(master);
            context.SaveChanges();

            return master;
        }

        private static MstRadModality BuatModalitas(ApplicationDbContext context)
        {
            var pembeda = Guid.NewGuid().ToString("N")[..8];

            var modalitas = new MstRadModality
            {
                ModalityCode = $"MOD-{pembeda}",
                ModalityName = "Radiografi Uji",
                IsActive = true
            };

            context.Set<MstRadModality>().Add(modalitas);
            context.SaveChanges();

            return modalitas;
        }

        // =====================================================================
        // Kriteria 1 dan 2 — pesanan perawatan A bukan milik perawatan B
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-052 AC 1</c>, <c>VAL-DOK-22</c> — pesanan laboratorium perawatan A tidak
        /// dapat diproses sebagai milik perawatan B; ditolak <c>400</c>.
        /// </summary>
        [Fact]
        public async Task PesananLaboratoriumPerawatanLain_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawatanLain = RawatInapTestData.SiapkanPerawatan(context);
            var master = BuatTindakanPenunjang(context, laboratorium: true);

            var hasil = await BuatControllerLab(context, k.DokterUserId).Create(
                new CreateLabOrderRequest
                {
                    EncounterId = k.EncounterId,
                    ProcedureId = master.Id,
                    InpEpisodeId = perawatanLain.EpisodeId
                });

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Contains("tidak cocok dengan perawatan", ControllerTestHarness.Pesan(hasil)!);

            using var verifikasi = database.CreateContext();
            Assert.Empty(verifikasi.Set<LabOrder>());
        }

        /// <summary>
        /// <c>BE-RWI-052 AC 2</c>, <c>VAL-DOK-22</c> — hal yang sama berlaku untuk pesanan
        /// radiologi.
        /// </summary>
        [Fact]
        public async Task PesananRadiologiPerawatanLain_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawatanLain = RawatInapTestData.SiapkanPerawatan(context);
            var master = BuatTindakanPenunjang(context, laboratorium: false);
            var modalitas = BuatModalitas(context);

            var hasil = await BuatControllerRad(context, k.DokterUserId).Create(
                new CreateRadOrderRequest
                {
                    EncounterId = k.EncounterId,
                    ProcedureId = master.Id,
                    ModalityId = modalitas.Id,
                    InpEpisodeId = perawatanLain.EpisodeId
                });

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Contains("tidak cocok dengan perawatan", ControllerTestHarness.Pesan(hasil)!);

            using var verifikasi = database.CreateContext();
            Assert.Empty(verifikasi.Set<RadOrder>());
        }

        // =====================================================================
        // Kriteria 3, 4, dan 5 — hasil final, penanda belum final, batas perawatan
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-052 AC 3</c>, <c>AC 4</c>, dan <c>AC 5</c> — hasil laboratorium terbaca
        /// dari konteks perawatan tanpa tabel salinan, hasil yang belum final ditandai, dan
        /// hasil milik kunjungan di luar perawatan yang dibuka tidak ikut tampil.
        /// </summary>
        /// <remarks>
        /// Tiga kriteria diuji dalam satu keadaan karena ketiganya adalah tiga sisi dari satu
        /// pembacaan yang sama. Data ujinya: satu pesanan selesai pada perawatan yang dibuka,
        /// satu pesanan masih berjalan pada perawatan yang sama, dan satu pesanan pada
        /// perawatan lain yang <b>tidak boleh</b> muncul.
        /// </remarks>
        [Fact]
        public async Task HasilLaboratorium_TerbacaPerPerawatanBesertaPenandaBelumFinal()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawatanLain = RawatInapTestData.SiapkanPerawatan(context);

            var selesai = Isi<LabOrderDetailResponse>(
                await BuatControllerLab(context, k.DokterUserId).Create(new CreateLabOrderRequest
                {
                    EncounterId = k.EncounterId,
                    ProcedureId = BuatTindakanPenunjang(context, laboratorium: true).Id,
                    InpEpisodeId = k.EpisodeId
                }));

            var berjalan = Isi<LabOrderDetailResponse>(
                await BuatControllerLab(context, k.DokterUserId).Create(new CreateLabOrderRequest
                {
                    EncounterId = k.EncounterId,
                    ProcedureId = BuatTindakanPenunjang(context, laboratorium: true).Id,
                    InpEpisodeId = k.EpisodeId
                }));

            await BuatControllerLab(context, perawatanLain.DokterUserId).Create(
                new CreateLabOrderRequest
                {
                    EncounterId = perawatanLain.EncounterId,
                    ProcedureId = BuatTindakanPenunjang(context, laboratorium: true).Id,
                    InpEpisodeId = perawatanLain.EpisodeId
                });

            // Laboratorium menyelesaikan pesanan pertama lewat permukaannya sendiri. Di sini
            // keadaan itu ditiru langsung pada baris miliknya, karena yang diuji adalah
            // PEMBACAANNYA dari sisi Rawat Inap.
            var pesananSelesai = context.Set<LabOrder>().Single(x => x.Id == selesai.Id);
            pesananSelesai.OrderStatus = LabOrderStatus.Completed;
            pesananSelesai.CompletedAt = DateTime.UtcNow;
            context.SaveChanges();

            var daftar = Isi<List<LabOrderListResponse>>(
                await BuatControllerLab(context, k.DokterUserId).GetByEpisode(k.EpisodeId));

            // Kriteria 5 — pesanan perawatan lain tidak ikut tampil.
            Assert.Equal(2, daftar.Count);
            Assert.All(daftar, x => Assert.Equal(k.EpisodeId, x.InpEpisodeId));

            // Kriteria 3 — hasil final terbaca.
            var barisSelesai = daftar.Single(x => x.Id == selesai.Id);
            Assert.True(barisSelesai.IsResultFinal);
            Assert.Equal("Hasil sudah final.", barisSelesai.ResultAvailabilityNote);

            // Kriteria 4 — hasil belum final ditandai, dan tidak disajikan sebagai hasil sah.
            var barisBerjalan = daftar.Single(x => x.Id == berjalan.Id);
            Assert.False(barisBerjalan.IsResultFinal);
            Assert.Contains("belum final", barisBerjalan.ResultAvailabilityNote);
            Assert.Contains("Jangan dipakai", barisBerjalan.ResultAvailabilityNote);
        }

        /// <summary>
        /// <c>BE-RWI-052 AC 3</c> dan <c>AC 4</c> untuk radiologi — hasil dinyatakan final
        /// hanya ketika pesanannya selesai <b>dan</b> ada study yang mutunya sudah diterima.
        /// </summary>
        /// <remarks>
        /// Pesanan yang berstatus selesai tetapi seluruh study-nya belum lolos mutu bukan hasil
        /// sah. Menampilkannya sebagai final akan membuat dokter membaca gambar yang justru
        /// akan diulang.
        /// </remarks>
        [Fact]
        public async Task HasilRadiologi_FinalHanyaKetikaStudyLolosMutu()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var modalitas = BuatModalitas(context);

            var pesanan = Isi<RadOrderDetailResponse>(
                await BuatControllerRad(context, k.DokterUserId).Create(new CreateRadOrderRequest
                {
                    EncounterId = k.EncounterId,
                    ProcedureId = BuatTindakanPenunjang(context, laboratorium: false).Id,
                    ModalityId = modalitas.Id,
                    InpEpisodeId = k.EpisodeId
                }));

            var baris = context.Set<RadOrder>().Single(x => x.Id == pesanan.Id);
            baris.OrderStatus = RadOrderStatus.Completed;
            baris.CompletedAt = DateTime.UtcNow;
            context.SaveChanges();

            // Selesai, tetapi belum ada satu pun study yang lolos mutu.
            var sebelumStudy = Isi<List<RadOrderListResponse>>(
                await BuatControllerRad(context, k.DokterUserId).GetByEpisode(k.EpisodeId));

            Assert.False(Assert.Single(sebelumStudy).IsResultFinal);

            context.Set<RadStudy>().Add(new RadStudy
            {
                RadOrderId = pesanan.Id,
                EncounterId = baris.EncounterId,
                ProcedureId = baris.ProcedureId,
                ModalityId = modalitas.Id,
                StudySequence = 1,
                StudyNumber = $"STD-{Guid.NewGuid().ToString("N")[..8]}",
                StudyStatus = RadStudyStatus.QualityAccepted
            });
            context.SaveChanges();

            var sesudahStudy = Isi<List<RadOrderListResponse>>(
                await BuatControllerRad(context, k.DokterUserId).GetByEpisode(k.EpisodeId));

            var barisSesudah = Assert.Single(sesudahStudy);
            Assert.True(barisSesudah.IsResultFinal);
            Assert.Equal("Hasil sudah final.", barisSesudah.ResultAvailabilityNote);
            Assert.Equal(k.EpisodeId, barisSesudah.InpEpisodeId);
        }

        // =====================================================================
        // Kriteria 6 — nol jalur tulis hasil, nol tabel salinan
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-052 AC 6</c>, <c>RUL-DOK-02</c> — sub-modul Rawat Inap tidak memiliki satu
        /// pun tabel salinan hasil laboratorium maupun radiologi.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Uji arsitektur. Dibuktikan dari kepemilikan tipe: seluruh entity hasil penunjang
        /// berada di bawah <c>LaboratoryManagement</c> dan <c>RadiologyManagement</c>, dan
        /// tidak satu pun berada di bawah <c>ClinicalManagement</c> maupun
        /// <c>InPatientManagement</c>.
        /// </para>
        /// <para>
        /// Ini larangan yang paling mudah dilanggar dengan alasan kinerja — "supaya layar
        /// dokter tidak perlu memanggil dua modul". Salinan itulah yang kelak menjadi angka
        /// basi ketika Laboratorium merevisi hasilnya.
        /// </para>
        /// </remarks>
        [Fact]
        public void RawatInapTidakMemilikiSatuPunTabelSalinanHasilPenunjang()
        {
            var kataHasil = new[] { "LabResult", "RadResult", "LabValueCopy", "RadReportCopy" };

            var penyalin = typeof(LabOrder).Assembly
                .GetTypes()
                .Where(t => t.Namespace != null
                            && (t.Namespace.StartsWith(
                                    "QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement",
                                    StringComparison.Ordinal)
                                || t.Namespace.StartsWith(
                                    "QuilvianSystemBackend.Areas.HealthServices.InPatientManagement",
                                    StringComparison.Ordinal)))
                .Where(t => kataHasil.Any(kata =>
                    t.Name.Contains(kata, StringComparison.OrdinalIgnoreCase)))
                .Select(t => t.FullName)
                .ToList();

            Assert.True(
                penyalin.Count == 0,
                "Rawat Inap tidak boleh menyimpan salinan hasil penunjang. Ditemukan: " +
                string.Join(", ", penyalin));
        }

        /// <summary>
        /// <c>BE-RWI-052 AC 6</c> — controller pesanan penunjang tidak menerima satu pun isi
        /// hasil dari pengirim.
        /// </summary>
        /// <remarks>
        /// Menulis hasil adalah kewenangan petugas Laboratorium dan Radiologi lewat permukaan
        /// mereka sendiri. Uji ini memeriksa permukaan pemesanan, bukan permukaan pemilik:
        /// yang dijaga adalah agar jalur pemesanan tidak berubah diam-diam menjadi jalur
        /// penulisan hasil.
        /// </remarks>
        [Fact]
        public void PermintaanPemesananPenunjangTidakMenerimaIsiHasil()
        {
            var namaTerlarang = new[] { "ResultValue", "ResultText", "ResultNote", "ReportText" };

            var pelanggar = new List<string>();

            foreach (var controller in new[] { typeof(LabOrderController), typeof(RadOrderController) })
            {
                var metode = controller
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(x => !x.IsSpecialName);

                foreach (var m in metode)
                {
                    foreach (var parameter in m.GetParameters())
                    {
                        var tipe = parameter.ParameterType;

                        if (tipe.FullName == null || !tipe.FullName.Contains(".DTOs.", StringComparison.Ordinal))
                            continue;

                        if (!tipe.Name.StartsWith("Create", StringComparison.Ordinal))
                            continue;

                        foreach (var terlarang in namaTerlarang)
                        {
                            if (tipe.GetProperty(terlarang) != null)
                                pelanggar.Add($"{controller.Name}.{m.Name} -> {tipe.Name}.{terlarang}");
                        }
                    }
                }
            }

            Assert.True(
                pelanggar.Count == 0,
                "Permukaan pemesanan penunjang tidak boleh menerima isi hasil. Ditemukan: " +
                string.Join(", ", pelanggar));
        }
    }
}
