using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.Infrastructure;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-049</c> — kunjungan yang salah catat dapat dibatalkan
    /// tanpa menghilangkan jejaknya.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kenapa pembatalan, bukan penyuntingan.</b> Kejadian visite menyatakan fakta
    /// kedatangan. Menyunting jamnya berarti mengganti satu fakta dengan fakta lain tanpa jejak,
    /// dan auditor kehilangan kemampuan melihat bahwa pernah ada catatan yang keliru. Karena itu
    /// penyuntingan di tempat dilarang <c>RWI-DEC-085</c>: yang benar adalah membatalkan
    /// beralasan lalu mencatat ulang.
    /// </para>
    /// <para>
    /// <b>Contoh yang menjelaskan seluruh berkas ini.</b> dr. Andi visite pukul 07.40 tetapi
    /// mengisi 17.40. Ia membatalkan dengan alasan "salah ketik jam", lalu mencatat kejadian
    /// baru pukul 07.40 yang menunjuk kejadian yang digantikannya. Riwayat menampilkan
    /// <b>dua baris</b> — satu batal beserta alasannya, satu berlaku. Hitungan visite hari itu
    /// tetap <b>satu</b>.
    /// </para>
    /// </remarks>
    public class PhysicianVisitCancellationTests
    {
        private static PhysicianVisitController BuatControllerVisite(
            Repositories.ApplicationDbContext c, Guid actorUserId) =>
            PhysicianVisitRecordingTests.BuatControllerVisite(c, actorUserId);

        private static T Isi<T>(IActionResult hasil) =>
            PhysicianVisitRecordingTests.Isi<T>(hasil);

        private static CreatePhysicianVisitRequest Permintaan(
            RawatInapTestData.Konteks k,
            DateTime? waktu = null,
            string? kunci = null) =>
            PhysicianVisitRecordingTests.Permintaan(k, waktu, kunci);

        // =====================================================================
        // Kriteria 1 sampai 4 — pembatalan beralasan, jejak tetap, hitungan turun
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-049 AC 1</c>, <c>VAL-DOK-28</c> — pembatalan tanpa alasan ditolak
        /// <c>400</c>.
        /// </summary>
        /// <remarks>
        /// Tanpa alasan, riwayat hanya menunjukkan bahwa sesuatu dibatalkan. Auditor yang
        /// membacanya setahun kemudian tidak dapat membedakan salah ketik jam dari pembatalan
        /// yang disengaja untuk menghapus jejak.
        /// </remarks>
        [Fact]
        public async Task PembatalanTanpaAlasan_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var visite = Isi<PhysicianVisitResponse>(
                await BuatControllerVisite(context, k.DokterUserId)
                    .RecordVisit(Permintaan(k, DateTime.UtcNow.AddHours(-2))));

            var hasil = await BuatControllerVisite(context, k.DokterUserId)
                .CancelVisit(visite.Id, new CancelPhysicianVisitRequest { CancelReason = "   " });

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Contains("Alasan pembatalan", ControllerTestHarness.Pesan(hasil)!);

            using var verifikasi = database.CreateContext();

            var tersimpan = verifikasi.Set<CliPhysicianVisit>().Single();
            Assert.Equal(PhysicianVisitStatus.Recorded, tersimpan.VisitStatus);
        }

        /// <summary>
        /// <c>BE-RWI-049 AC 2</c> dan <c>AC 3</c>, <c>INV-DOK-08</c> — kejadian yang dibatalkan
        /// tetap tersimpan, tetap tampil pada riwayat beserta alasannya, dan berhenti ikut
        /// dihitung.
        /// </summary>
        [Fact]
        public async Task KejadianDibatalkan_TetapTampilPadaRiwayatDanTidakIkutDihitung()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var visite = Isi<PhysicianVisitResponse>(
                await BuatControllerVisite(context, k.DokterUserId)
                    .RecordVisit(Permintaan(k, DateTime.UtcNow.AddHours(-2))));

            var batal = await BuatControllerVisite(context, k.DokterUserId)
                .CancelVisit(visite.Id, new CancelPhysicianVisitRequest
                {
                    CancelReason = "Salah ketik jam"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(batal));

            // Barisnya tetap ada. Tidak dihapus, tidak ditandai terhapus.
            using var verifikasi = database.CreateContext();

            var tersimpan = verifikasi.Set<CliPhysicianVisit>().Single();

            Assert.Equal(PhysicianVisitStatus.Cancelled, tersimpan.VisitStatus);
            Assert.False(tersimpan.IsDelete);
            Assert.Equal("Salah ketik jam", tersimpan.CancelReason);
            Assert.NotNull(tersimpan.CancelledAt);
            Assert.Equal(k.DokterUserId, tersimpan.CancelledByUserId);

            // Riwayat tetap menampilkannya beserta alasannya.
            var riwayat = Isi<PagedResult<PhysicianVisitListItemResponse>>(
                await BuatControllerVisite(context, k.DokterUserId).GetByEpisode(k.EpisodeId));

            var baris = Assert.Single(riwayat.Items);
            Assert.Equal(PhysicianVisitStatus.Cancelled, baris.VisitStatus);
            Assert.Equal("Dibatalkan", baris.VisitStatusName);
            Assert.Equal("Salah ketik jam", baris.CancelReason);

            // Hitungan berhenti menghitungnya.
            var rekap = Isi<PhysicianVisitSummaryResponse>(
                await BuatControllerVisite(context, k.DokterUserId)
                    .GetSummary(inpEpisodeId: k.EpisodeId));

            Assert.Equal(0, rekap.RecordedCount);
            Assert.Equal(1, rekap.CancelledCount);
            Assert.Equal(1, rekap.TotalCount);
        }

        /// <summary>
        /// <c>BE-RWI-049 AC 4</c>, <c>VAL-DOK-29</c> — membatalkan kejadian yang sudah batal
        /// ditolak <c>409</c>.
        /// </summary>
        /// <remarks>
        /// <c>Cancelled</c> adalah status terminal. Pembatalan kedua akan menimpa alasan dan
        /// waktu pembatalan pertama, dan itu menghapus jejak yang justru sedang dijaga.
        /// </remarks>
        [Fact]
        public async Task MembatalkanKejadianYangSudahBatal_Ditolak409()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var visite = Isi<PhysicianVisitResponse>(
                await BuatControllerVisite(context, k.DokterUserId)
                    .RecordVisit(Permintaan(k, DateTime.UtcNow.AddHours(-2))));

            await BuatControllerVisite(context, k.DokterUserId)
                .CancelVisit(visite.Id, new CancelPhysicianVisitRequest
                {
                    CancelReason = "Salah ketik jam"
                });

            var kedua = await BuatControllerVisite(context, k.DokterUserId)
                .CancelVisit(visite.Id, new CancelPhysicianVisitRequest
                {
                    CancelReason = "Mencoba membatalkan lagi"
                });

            Assert.Equal(409, ControllerTestHarness.KodeStatus(kedua));

            using var verifikasi = database.CreateContext();

            var tersimpan = verifikasi.Set<CliPhysicianVisit>().Single();
            Assert.Equal("Salah ketik jam", tersimpan.CancelReason);
        }

        // =====================================================================
        // Kriteria 5 — pencatatan ulang menunjuk kejadian yang digantikan
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-049 AC 5</c>, <c>state-transition-matrix.md</c> bagian 5.2 dan 5.3 —
        /// pencatatan ulang setelah pembatalan menunjuk kejadian yang digantikannya, riwayat
        /// memuat dua baris, dan hitungannya tetap satu.
        /// </summary>
        /// <remarks>
        /// Ini contoh dr. Andi pada keterangan kelas, dibuktikan dengan angka.
        /// </remarks>
        [Fact]
        public async Task PencatatanUlangSetelahPembatalan_MenunjukKejadianYangDigantikan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var salah = Isi<PhysicianVisitResponse>(
                await BuatControllerVisite(context, k.DokterUserId)
                    .RecordVisit(Permintaan(k, DateTime.UtcNow.AddHours(-1))));

            await BuatControllerVisite(context, k.DokterUserId)
                .CancelVisit(salah.Id, new CancelPhysicianVisitRequest
                {
                    CancelReason = "Salah ketik jam"
                });

            var waktuBenar = DateTime.UtcNow.AddHours(-9);
            var permintaanUlang = Permintaan(k, waktuBenar);
            permintaanUlang.CorrectsVisitId = salah.Id;

            var benar = await BuatControllerVisite(context, k.DokterUserId)
                .RecordVisit(permintaanUlang);

            Assert.Equal(201, ControllerTestHarness.KodeStatus(benar));

            using var verifikasi = database.CreateContext();

            Assert.Equal(2, verifikasi.Set<CliPhysicianVisit>().Count());

            var pengganti = verifikasi.Set<CliPhysicianVisit>()
                .Single(x => x.VisitStatus == PhysicianVisitStatus.Recorded);

            Assert.Equal(salah.Id, pengganti.CorrectsVisitId);
            Assert.Equal(waktuBenar, pengganti.VisitDateTime, TimeSpan.FromSeconds(1));

            var riwayat = Isi<PagedResult<PhysicianVisitListItemResponse>>(
                await BuatControllerVisite(context, k.DokterUserId).GetByEpisode(k.EpisodeId));

            Assert.Equal(2, riwayat.Items.Count);
            Assert.Contains(riwayat.Items, x => x.VisitStatus == PhysicianVisitStatus.Cancelled);

            var rekap = Isi<PhysicianVisitSummaryResponse>(
                await BuatControllerVisite(context, k.DokterUserId)
                    .GetSummary(inpEpisodeId: k.EpisodeId));

            Assert.Equal(1, rekap.RecordedCount);
            Assert.Equal(1, rekap.CancelledCount);
            Assert.Equal(2, rekap.TotalCount);
        }

        // =====================================================================
        // Kriteria 6 dan 7 — uji arsitektur
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-049 AC 6</c>, <c>RWI-DEC-085</c> — <b>tidak ada satu pun endpoint</b> yang
        /// menyunting waktu maupun peran kejadian visite.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Uji ini memeriksa permukaan API-nya sendiri, bukan satu perilaku. Alasannya:
        /// larangan ini paling mudah dilanggar tanpa sadar di kemudian hari — seseorang
        /// menambahkan <c>PATCH /{id}</c> "supaya dokter bisa membetulkan jam" dengan niat
        /// baik, dan seluruh jejak koreksi hilang tanpa satu uji perilaku pun gagal.
        /// </para>
        /// <para>
        /// Yang diperiksa: setiap parameter badan permintaan pada controller visite selain
        /// pencatatan kejadian baru tidak boleh memuat properti waktu kunjungan maupun peran.
        /// </para>
        /// </remarks>
        [Fact]
        public void TidakAdaEndpointYangMenyuntingWaktuMaupunPeranVisite()
        {
            var metode = typeof(PhysicianVisitController)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(x => !x.IsSpecialName)
                .ToList();

            Assert.NotEmpty(metode);

            var namaTerlarang = new[] { "VisitDateTime", "VisitRole" };

            foreach (var m in metode)
            {
                // Pencatatan kejadian BARU memang menerima waktu dan peran; itulah isinya.
                if (m.Name == nameof(PhysicianVisitController.RecordVisit))
                    continue;

                foreach (var parameter in m.GetParameters())
                {
                    var tipe = parameter.ParameterType;

                    if (!tipe.FullName!.StartsWith(
                            "QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs",
                            StringComparison.Ordinal))
                    {
                        continue;
                    }

                    foreach (var terlarang in namaTerlarang)
                    {
                        Assert.True(
                            tipe.GetProperty(terlarang) == null,
                            $"{m.Name} menerima {tipe.Name} yang memuat {terlarang}. " +
                            "Penyuntingan waktu maupun peran visite dilarang RWI-DEC-085; " +
                            "koreksi dilakukan dengan membatalkan lalu mencatat ulang.");
                    }
                }
            }
        }

        /// <summary>
        /// <c>BE-RWI-049 AC 6</c> — controller visite tidak menyediakan satu pun endpoint
        /// penghapusan.
        /// </summary>
        /// <remarks>
        /// <c>INV-DOK-08</c>. Kejadian yang dibatalkan tetap tersimpan; menyediakan
        /// <c>DELETE</c> berarti menyediakan jalan menghapus fakta.
        /// </remarks>
        [Fact]
        public void ControllerVisiteTidakMenyediakanEndpointPenghapusan()
        {
            var adaDelete = typeof(PhysicianVisitController)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Any(x => x.GetCustomAttributes<HttpDeleteAttribute>().Any());

            Assert.False(
                adaDelete,
                "Controller visite tidak boleh memiliki endpoint DELETE. Kejadian yang " +
                "dibatalkan tetap tersimpan - INV-DOK-08.");
        }

        /// <summary>
        /// <c>BE-RWI-049 AC 7</c>, <c>RWI-AC-156</c> — agregasi tagihan tidak mengubah,
        /// menggabungkan, maupun menghapus kejadian klinis.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Dibuktikan pada tingkat arsitektur: tidak ada satu pun tipe di luar
        /// <c>ClinicalManagement</c> yang menyentuh <c>CliPhysicianVisit</c>. Selama tidak ada
        /// yang dapat menulisnya, Billing tidak dapat menggabungkan dua kejadian menjadi satu
        /// walaupun tagihannya digabung.
        /// </para>
        /// <para>
        /// Batas ini penting karena kebijakan agregasi tarif visite <b>belum ada</b>. Ketika
        /// kelak dibuat, ia harus menggabungkan pada sisi tagihan, bukan pada riwayat klinis.
        /// </para>
        /// </remarks>
        [Fact]
        public void HanyaClinicalManagementYangMenyentuhKejadianVisite()
        {
            var penyentuh = typeof(CliPhysicianVisit).Assembly
                .GetTypes()
                .Where(t => t.Namespace != null
                            && !t.Namespace.StartsWith(
                                "QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement",
                                StringComparison.Ordinal)
                            && !t.Namespace.StartsWith(
                                "QuilvianSystemBackend.Repositories",
                                StringComparison.Ordinal))
                .Where(t => t.GetFields(BindingFlags.Public | BindingFlags.NonPublic
                                        | BindingFlags.Instance | BindingFlags.Static)
                                .Any(f => Menyentuh(f.FieldType))
                            || t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic
                                               | BindingFlags.Instance | BindingFlags.Static)
                                .Any(p => Menyentuh(p.PropertyType)))
                .Select(t => t.FullName)
                .ToList();

            Assert.True(
                penyentuh.Count == 0,
                "Kejadian visite hanya boleh disentuh ClinicalManagement. Ditemukan: " +
                string.Join(", ", penyentuh));
        }

        private static bool Menyentuh(Type tipe)
        {
            if (tipe == typeof(CliPhysicianVisit))
                return true;

            return tipe.IsGenericType
                   && tipe.GetGenericArguments().Any(x => x == typeof(CliPhysicianVisit));
        }
    }
}
