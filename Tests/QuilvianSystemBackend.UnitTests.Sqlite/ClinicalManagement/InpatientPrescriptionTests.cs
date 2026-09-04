using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.Infrastructure;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-050</c> — resep berulang dan obat pulang sepanjang
    /// perawatan.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Masalah yang ditutup.</b> Batas satu resep aktif per catatan masuk akal di poliklinik:
    /// satu kunjungan, satu resep. Pada pasien yang dirawat lima hari, batas yang sama berarti
    /// seluruh terapi lima hari harus muat pada satu resep yang ditulis di hari pertama — dan
    /// itu mustahil.
    /// </para>
    /// <para>
    /// <b>Batas yang tidak boleh dilanggar.</b> Menandai obat sudah diserahkan adalah kewenangan
    /// petugas Farmasi — <c>RUL-DOK-01</c>. Sub-modul ini hanya <b>membaca</b> keadaan
    /// pemenuhan. Uji arsitektur di bawah menjaganya, karena menambahkan jalur tulis kelak akan
    /// terasa seperti melengkapi fitur padahal ia melanggar batas kepemilikan.
    /// </para>
    /// </remarks>
    public class InpatientPrescriptionTests
    {
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

        private static T Isi<T>(IActionResult hasil)
        {
            var objek = Assert.IsAssignableFrom<ObjectResult>(hasil);
            var pembungkus = Assert.IsType<ApiResponse<T>>(objek.Value);

            Assert.NotNull(pembungkus.Data);
            return pembungkus.Data!;
        }

        private static CreatePrescriptionRequest Permintaan(
            RawatInapTestData.Konteks k,
            Guid consultationId,
            PrescriptionOrderType jenis = PrescriptionOrderType.Daily,
            string? kunci = null,
            DateTime? waktu = null) => new()
            {
                EncounterId = k.EncounterId,
                ConsultationId = consultationId,
                PrescriptionOrderType = jenis,
                IdempotencyKey = kunci,
                PrescriptionDateTime = waktu
            };

        // =====================================================================
        // Kriteria 1 dan 2 — resep berulang dan obat pulang
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-050 AC 1</c> dan <c>AC 2</c> — lima resep pada satu perawatan lima hari
        /// tersimpan seluruhnya, dan resep obat pulang tersaring tersendiri menurut jenisnya.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Contoh berangka: pasien dirawat lima hari. Hari 1 sampai 4 dokter menulis resep
        /// harian; hari ke-5 ia menulis obat pulang. Yang tersimpan adalah <b>lima</b> resep.
        /// Menyaring jenis obat pulang menghasilkan <b>satu</b> baris — itulah yang dilihat
        /// petugas farmasi pada layar mereka sendiri, <c>AC-CAP023-03</c>.
        /// </para>
        /// <para>
        /// Jenis obat pulang <b>eksplisit</b>, bukan disimpulkan dari waktu penulisan. Pasien
        /// yang pemulangannya tertunda dua hari akan salah dibaca bila jenisnya ditebak dari
        /// tanggal.
        /// </para>
        /// </remarks>
        [Fact]
        public async Task LimaResepPadaSatuPerawatan_TersimpanSeluruhnyaDanObatPulangTersaring()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var catatan = TindakanTestData.BuatCatatanInduk(
                context, k.EncounterId, k.PatientId, k.DoctorMasterId, k.ServiceUnitId, k.EpisodeId);

            for (var hari = 1; hari <= 4; hari++)
            {
                var harian = await BuatControllerResep(context, k.DokterUserId).CreatePrescription(
                    Permintaan(k, catatan.Id, PrescriptionOrderType.Daily,
                               waktu: DateTime.UtcNow.AddDays(-5 + hari)));

                Assert.Equal(200, ControllerTestHarness.KodeStatus(harian));
            }

            var obatPulang = await BuatControllerResep(context, k.DokterUserId).CreatePrescription(
                Permintaan(k, catatan.Id, PrescriptionOrderType.Discharge));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(obatPulang));

            using var verifikasi = database.CreateContext();
            Assert.Equal(5, verifikasi.Set<TrxPrescription>().Count());

            var seluruhnya = Isi<PagedResult<PrescriptionResponse>>(
                await BuatControllerResep(context, k.DokterUserId).GetByEpisode(k.EpisodeId));

            Assert.Equal(5, seluruhnya.TotalData);

            var hanyaObatPulang = Isi<PagedResult<PrescriptionResponse>>(
                await BuatControllerResep(context, k.DokterUserId)
                    .GetByEpisode(k.EpisodeId, PrescriptionOrderType.Discharge));

            Assert.Equal(1, hanyaObatPulang.TotalData);
            Assert.Equal(
                PrescriptionOrderType.Discharge,
                Assert.Single(hanyaObatPulang.Items).PrescriptionOrderType);
        }

        // =====================================================================
        // Kriteria 3 — kunci permintaan
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-050 AC 3</c>, <c>VAL-DOK-19</c> — pengiriman berulang dengan kunci yang
        /// sama tidak melahirkan resep ganda.
        /// </summary>
        /// <remarks>
        /// Resep ganda bukan sekadar baris ganda: ia menjadi obat ganda yang disiapkan farmasi
        /// dan tagihan ganda yang ditanggung pasien.
        /// </remarks>
        [Fact]
        public async Task PengirimanBerulangBerkunciSama_TidakMelahirkanResepGanda()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var catatan = TindakanTestData.BuatCatatanInduk(
                context, k.EncounterId, k.PatientId, k.DoctorMasterId, k.ServiceUnitId, k.EpisodeId);

            const string kunci = "TOMBOL-RESEP-TERTEKAN-DUA-KALI";

            var pertama = await BuatControllerResep(context, k.DokterUserId)
                .CreatePrescription(Permintaan(k, catatan.Id, kunci: kunci));
            var kedua = await BuatControllerResep(context, k.DokterUserId)
                .CreatePrescription(Permintaan(k, catatan.Id, kunci: kunci));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(pertama));
            Assert.Equal(200, ControllerTestHarness.KodeStatus(kedua));

            Assert.Equal(
                Isi<PrescriptionCreateResponse>(pertama).Id,
                Isi<PrescriptionCreateResponse>(kedua).Id);

            using var verifikasi = database.CreateContext();
            Assert.Single(verifikasi.Set<TrxPrescription>());
        }

        // =====================================================================
        // Kriteria 4 — status pemenuhan dapat dibaca
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-050 AC 4</c> — keadaan pemenuhan resep dapat dibaca kembali dari konteks
        /// perawatan.
        /// </summary>
        /// <remarks>
        /// Dokter perlu tahu obat mana yang sudah sampai ke pasien sebelum menulis resep
        /// berikutnya. Yang dibaca adalah kolom milik <c>PharmacyManagement</c> apa adanya;
        /// Rawat Inap tidak menyimpan salinan status pemenuhan mana pun.
        /// </remarks>
        [Fact]
        public async Task StatusPemenuhanResep_DapatDibacaDariKonteksPerawatan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var catatan = TindakanTestData.BuatCatatanInduk(
                context, k.EncounterId, k.PatientId, k.DoctorMasterId, k.ServiceUnitId, k.EpisodeId);

            var dibuat = await BuatControllerResep(context, k.DokterUserId)
                .CreatePrescription(Permintaan(k, catatan.Id));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dibuat));

            // Petugas farmasi menaikkan keadaan pemenuhan lewat permukaannya sendiri. Di sini
            // keadaan itu hanya ditiru langsung pada baris miliknya, karena yang diuji adalah
            // PEMBACAANNYA dari sisi Rawat Inap.
            var resep = context.Set<TrxPrescription>().Single();
            resep.FulfillmentStatus = PrescriptionFulfillmentStatus.ReadyForPharmacy;
            context.SaveChanges();

            var daftar = Isi<PagedResult<PrescriptionResponse>>(
                await BuatControllerResep(context, k.DokterUserId).GetByEpisode(k.EpisodeId));

            var baris = Assert.Single(daftar.Items);

            Assert.Equal(PrescriptionFulfillmentStatus.ReadyForPharmacy, baris.FulfillmentStatus);
            Assert.True(baris.IsReadyForPharmacy);
            Assert.Equal(k.EpisodeId, baris.InpEpisodeId);
        }

        // =====================================================================
        // Kriteria 5 dan 6 — nol jalur tulis menuju status pemenuhan
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-050 AC 6</c>, <c>RUL-DOK-01</c> — <b>tidak ada satu pun</b> permukaan pada
        /// controller resep yang menerima keadaan pemenuhan dari pengirim.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Uji arsitektur, bukan uji perilaku. Alasannya: larangan ini paling mudah dilanggar
        /// dengan niat baik di kemudian hari — seseorang menambahkan satu ruas
        /// <c>fulfillmentStatus</c> pada permintaan pembaruan "supaya dokter bisa menandai obat
        /// sudah diambil", dan batas kepemilikan Farmasi runtuh tanpa satu uji perilaku pun
        /// gagal.
        /// </para>
        /// <para>
        /// Yang diperiksa: setiap tipe badan permintaan yang diterima controller ini tidak
        /// memiliki properti keadaan pemenuhan.
        /// </para>
        /// </remarks>
        [Fact]
        public void TidakAdaPermintaanResepYangMenerimaKeadaanPemenuhan()
        {
            var metode = typeof(PrescriptionController)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(x => !x.IsSpecialName)
                .ToList();

            Assert.NotEmpty(metode);

            foreach (var m in metode)
            {
                foreach (var parameter in m.GetParameters())
                {
                    var tipe = parameter.ParameterType;

                    if (tipe.FullName == null ||
                        !tipe.FullName.StartsWith(
                            "QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs",
                            StringComparison.Ordinal))
                    {
                        continue;
                    }

                    // Hanya badan permintaan yang diperiksa; tipe balasan memang membacanya.
                    if (!tipe.Name.StartsWith("Create", StringComparison.Ordinal) &&
                        !tipe.Name.StartsWith("Update", StringComparison.Ordinal) &&
                        !tipe.Name.StartsWith("Cancel", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    Assert.True(
                        tipe.GetProperty("FulfillmentStatus") == null,
                        $"{m.Name} menerima {tipe.Name} yang memuat FulfillmentStatus. " +
                        "Menandai obat sudah diserahkan adalah kewenangan petugas Farmasi - " +
                        "RUL-DOK-01.");
                }
            }
        }

        /// <summary>
        /// <c>BE-RWI-050 AC 6</c> — controller resep tidak menyediakan satu pun aksi penyerahan
        /// obat.
        /// </summary>
        /// <remarks>
        /// Dibuktikan dari nama aksinya, bukan dari isinya. Endpoint bernama <c>dispense</c>,
        /// <c>handover</c>, atau <c>fulfill</c> pada controller ini adalah pelanggaran batas
        /// walaupun isinya kebetulan belum menulis apa-apa.
        /// </remarks>
        [Fact]
        public void ControllerResepTidakMenyediakanAksiPenyerahanObat()
        {
            var namaTerlarang = new[] { "dispense", "handover", "fulfill", "deliver", "serah" };

            var pelanggar = typeof(PrescriptionController)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(x => !x.IsSpecialName)
                .Where(x => namaTerlarang.Any(t =>
                    x.Name.Contains(t, StringComparison.OrdinalIgnoreCase)))
                .Select(x => x.Name)
                .ToList();

            Assert.True(
                pelanggar.Count == 0,
                "Controller resep tidak boleh memiliki aksi penyerahan obat. Ditemukan: " +
                string.Join(", ", pelanggar));
        }
    }
}
