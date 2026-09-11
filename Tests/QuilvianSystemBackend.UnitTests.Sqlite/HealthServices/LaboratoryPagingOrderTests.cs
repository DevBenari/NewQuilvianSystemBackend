using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using Microsoft.AspNetCore.Http;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices
{
    /// <summary>
    /// Penjaga batas halaman modul laboratorium, diperiksa dari SQL yang benar-benar
    /// dibangkitkan EF Core — bukan dari perilakunya.
    ///
    /// <para>
    /// <b>Kenapa memeriksa teks SQL, bukan hasilnya.</b> Cacat yang dijaga di sini adalah
    /// <c>ORDER BY</c> tanpa pemecah seri yang pasti. SQL tidak menjanjikan urutan relatif apa
    /// pun bagi baris yang seluruh kunci urutannya sama, dan halaman 1 dengan halaman 2 adalah
    /// dua kueri terpisah — sehingga satu baris dapat terbaca di kedua halaman sementara baris
    /// lain tidak pernah terbaca sama sekali.
    /// </para>
    ///
    /// <para>
    /// Uji berbasis perilaku tidak dapat menangkapnya. Penyedia InMemory maupun SQLite
    /// mempertahankan urutan penyisipan bagi baris yang seri, sehingga cacatnya tidak pernah
    /// muncul di sana; yang membebaskan urutannya adalah PostgreSQL di lingkungan sungguhan.
    /// Karena itu yang diperiksa di sini adalah bentuk kueri yang dikirim ke database, dan
    /// SQLite hanya dipakai sebagai mesin yang membangkitkan SQL-nya tanpa perlu server.
    /// </para>
    ///
    /// <para>
    /// Akibat nyata bila penjagaan ini hilang: pasien yang terdaftar tidak muncul di halaman
    /// mana pun pada layar pencarian, petugas menyimpulkan ia belum pernah terdaftar, lalu
    /// mendaftarkannya sebagai pasien baru — dan pasien itu berakhir dengan dua nomor rekam
    /// medis serta riwayat pemeriksaan yang terbelah.
    /// </para>
    /// </summary>
    public class LaboratoryPagingOrderTests
    {
        /// <summary>
        /// Menjalankan satu pembacaan berhalaman lalu mengembalikan potongan <c>ORDER BY</c>
        /// dari perintah SQL terakhir yang memuatnya.
        ///
        /// Penyaring pencarian sengaja tidak diisi pada setiap pemanggilan: beberapa service
        /// memakai <c>ILike</c> yang khas PostgreSQL dan tidak dapat diterjemahkan SQLite.
        /// Yang sedang diperiksa adalah urutannya, dan urutan itu tidak bergantung pada
        /// penyaring apa pun.
        /// </summary>
        private static async Task<string> BacaOrderByAsync(
            Func<ApplicationDbContext, Task> pembacaan)
        {
            using var database = TestDatabase.Create();

            var perintah = new List<string>();

            await using (var context = database.CreateContext(baris => perintah.Add(baris)))
            {
                await pembacaan(context);
            }

            var orderBy = perintah
                .Select(AmbilOrderBy)
                .LastOrDefault(x => !string.IsNullOrWhiteSpace(x));

            Assert.False(
                string.IsNullOrWhiteSpace(orderBy),
                "Tidak ada perintah SQL ber-ORDER BY yang tercatat. Kueri berhalaman tanpa " +
                "ORDER BY sama sekali berarti batas halamannya sepenuhnya tidak pasti.");

            return orderBy!;
        }

        /// <summary>Accessor beridentitas petugas, syarat konstruktor beberapa service.</summary>
        private static IHttpContextAccessor BuatAccessor() =>
            new HttpContextAccessor
            {
                HttpContext = ControllerTestHarness.BuatHttpContext(Guid.NewGuid())
            };

        /// <summary>Mengambil isi klausa ORDER BY, berhenti sebelum LIMIT/OFFSET.</summary>
        private static string AmbilOrderBy(string perintah)
        {
            var cocok = Regex.Match(
                perintah,
                @"ORDER BY (?<isi>.*?)(?=\s+LIMIT\b|\s+OFFSET\b|$)",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            return cocok.Success ? cocok.Groups["isi"].Value.Trim() : string.Empty;
        }

        /// <summary>
        /// Kunci urutan terakhir wajib berupa kolom yang unik per baris.
        ///
        /// Diperiksa pada kunci <b>terakhir</b>, bukan sekadar "ada di suatu tempat": pemecah
        /// seri yang berada di tengah tidak memutus seri pada kunci-kunci sesudahnya.
        /// </summary>
        private static void PastikanBerakhirPadaKolomUnik(string orderBy, string namaLayar)
        {
            var kunciTerakhir = orderBy.Split(',').Last().Trim();

            Assert.True(
                Regex.IsMatch(kunciTerakhir, @"""Id""|""ExaminationId""|""ReasonCode""", RegexOptions.IgnoreCase),
                $"{namaLayar}: ORDER BY berakhir pada \"{kunciTerakhir}\", yang belum tentu unik " +
                $"per baris. Batas antar halaman karena itu tidak pasti, dan satu baris dapat " +
                $"terbaca dua kali sementara baris lain tidak pernah tampil. " +
                $"ORDER BY selengkapnya: {orderBy}");
        }

        // =====================================================================
        // Pencarian pasien
        // =====================================================================

        [Fact]
        public async Task PencarianPasien_UrutannyaBerakhirPadaKolomUnik()
        {
            var orderBy = await BacaOrderByAsync(async context =>
            {
                var service = new LabPatientRegistrationService(context, null!);
                await service.SearchPatientsAsync(new LabPatientSearchQuery());
            });

            PastikanBerakhirPadaKolomUnik(orderBy, "Pencarian pasien laboratorium");
        }

        // =====================================================================
        // Katalog
        // =====================================================================

        [Fact]
        public async Task KatalogPemeriksaan_UrutannyaBerakhirPadaKolomUnik()
        {
            var orderBy = await BacaOrderByAsync(async context =>
            {
                var service = new LabCatalogService(context);
                await service.GetExaminationsAsync(new LabCatalogQuery());
            });

            PastikanBerakhirPadaKolomUnik(orderBy, "Katalog pemeriksaan");
        }

        [Fact]
        public async Task DaftarTarif_UrutannyaBerakhirPadaKolomUnik()
        {
            var orderBy = await BacaOrderByAsync(async context =>
            {
                var service = new LabCatalogService(context);
                await service.GetTariffsAsync(new LabTariffQuery());
            });

            PastikanBerakhirPadaKolomUnik(orderBy, "Daftar tarif laboratorium");
        }

        // =====================================================================
        // Daftar kerja
        // =====================================================================

        [Fact]
        public async Task DaftarKerja_UrutannyaBerakhirPadaKolomUnik()
        {
            var orderBy = await BacaOrderByAsync(async context =>
            {
                var service = new LabWorklistService(context);
                await service.GetPendingAsync(new LabWorklistPagedQuery());
            });

            PastikanBerakhirPadaKolomUnik(orderBy, "Daftar kerja laboratorium");
        }

        // =====================================================================
        // Monitoring
        // =====================================================================

        [Fact]
        public async Task DaftarPantau_UrutannyaBerakhirPadaKolomUnik()
        {
            var orderBy = await BacaOrderByAsync(async context =>
            {
                var service = new LabMonitoringService(context);

                await service.GetByDisciplineAsync(
                    LabDiscipline.ClinicalPathology, new LabMonitoringQuery());
            });

            PastikanBerakhirPadaKolomUnik(orderBy, "Daftar pantau laboratorium");
        }

        // =====================================================================
        // Batas nilai
        // =====================================================================

        [Fact]
        public async Task DaftarBatasNilai_UrutannyaBerakhirPadaKolomUnik()
        {
            var orderBy = await BacaOrderByAsync(async context =>
            {
                var service = new LabValueBoundService(
                    context,
                    BuatAccessor(),
                    ControllerTestHarness.BuatLoggerService());

                await service.GetListAsync(new LabValueBoundPagedQuery());
            });

            PastikanBerakhirPadaKolomUnik(orderBy, "Daftar batas nilai laboratorium");
        }

        // =====================================================================
        // Alasan penolakan
        // =====================================================================

        /// <summary>
        /// Alasan penolakan berakhir pada <c>ReasonCode</c>, bukan <c>Id</c>. Itu tetap sah
        /// karena kolom tersebut punya unique index tersaring pada baris yang belum terhapus —
        /// dan kueri ini memang hanya membaca baris yang belum terhapus.
        /// </summary>
        [Fact]
        public async Task AlasanPenolakan_UrutannyaBerakhirPadaKolomUnik()
        {
            var orderBy = await BacaOrderByAsync(async context =>
            {
                var service = new LabRejectionReasonService(
                    context,
                    BuatAccessor(),
                    ControllerTestHarness.BuatLoggerService());

                await service.GetListAsync(new LabRejectionReasonPagedQuery());
            });

            PastikanBerakhirPadaKolomUnik(orderBy, "Alasan penolakan sampel");
        }

        // =====================================================================
        // Daftar pesanan
        // =====================================================================

        /// <summary>
        /// Daftar pesanan punya empat cabang urutan — dua kolom pengurutan dikali dua arah.
        /// Keempatnya diperiksa, karena pemecah seri yang hanya dipasang pada cabang bawaan
        /// meninggalkan tiga cabang lain tetap rawan.
        /// </summary>
        [Theory]
        [InlineData(null, "desc")]
        [InlineData(null, "asc")]
        [InlineData("orderstatus", "desc")]
        [InlineData("orderstatus", "asc")]
        public async Task DaftarPesanan_SetiapCabangUrutanBerakhirPadaKolomUnik(
            string? sortBy,
            string sortDirection)
        {
            var orderBy = await BacaOrderByAsync(async context =>
            {
                var service = new LabOrderService(
                    context, null!, BuatAccessor(), ControllerTestHarness.BuatLoggerService());

                await service.GetListAsync(new LabOrderPagedQuery
                {
                    SortBy = sortBy,
                    SortDirection = sortDirection
                });
            });

            PastikanBerakhirPadaKolomUnik(
                orderBy, $"Daftar pesanan (sortBy={sortBy ?? "bawaan"}, arah={sortDirection})");
        }
    }
}
