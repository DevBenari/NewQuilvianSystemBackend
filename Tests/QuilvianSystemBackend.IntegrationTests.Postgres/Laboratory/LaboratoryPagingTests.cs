using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.BillingTests.Infrastructure;
using Xunit;

namespace QuilvianSystemBackend.BillingTests.Laboratory
{
    /// <summary>
    /// Bukti batas halaman modul laboratorium terhadap PostgreSQL sungguhan.
    ///
    /// <para>
    /// <b>Mengapa uji ini tidak dapat hidup di proyek unit test.</b> Uji unit laboratorium
    /// berjalan di atas penyedia InMemory, yang mempertahankan urutan penyisipan ketika kunci
    /// urutannya seri. Justru pada keadaan itulah cacat yang diperbaiki di sini muncul: SQL
    /// tidak menjanjikan urutan relatif apa pun untuk baris yang seluruh kunci urutannya sama,
    /// dan halaman 1 dengan halaman 2 adalah dua kueri yang berbeda. Satu baris dapat terbaca
    /// di kedua halaman sementara baris lain tidak pernah terbaca di halaman mana pun.
    /// </para>
    ///
    /// <para>
    /// Karena itu setiap skenario di bawah sengaja menyeragamkan kunci urutannya — nama pasien
    /// yang sama persis, nama pemeriksaan yang sama persis — sehingga yang tersisa sebagai
    /// penentu urutan hanyalah pemecah seri yang ditambahkan pada <c>ORDER BY</c>. Bila pemecah
    /// seri itu dihapus, uji ini yang pertama gagal.
    /// </para>
    ///
    /// <para>
    /// Akibat nyata bila dibiarkan: petugas laboratorium yang menelusuri daftar pasien tidak
    /// menemukan pasien yang sebenarnya terdaftar, lalu mendaftarkannya sebagai pasien baru —
    /// dan pasien itu berakhir dengan dua nomor rekam medis serta riwayat yang terbelah.
    /// </para>
    /// </summary>
    [Collection(PostgresIntegrationTestCollection.Name)]
    public sealed class LaboratoryPagingTests : IAsyncLifetime
    {
        private readonly BillingTestDatabaseFixture _fixture;

        private readonly List<Guid> _patientIds = new();
        private readonly List<Guid> _tariffIds = new();
        private readonly List<Guid> _procedureIds = new();

        public LaboratoryPagingTests(BillingTestDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        /// <summary>
        /// Menghapus seluruh baris yang dibuat uji ini, dari anak ke induk. Tarif lebih dulu
        /// karena FK-nya menunjuk pemeriksaan dengan perilaku Restrict.
        /// </summary>
        public async Task DisposeAsync()
        {
            await using var context = _fixture.CreateContext();

            if (_tariffIds.Count > 0)
            {
                await context.Set<MstTariff>()
                    .Where(x => _tariffIds.Contains(x.Id))
                    .ExecuteDeleteAsync();
            }

            if (_procedureIds.Count > 0)
            {
                await context.Set<MstProcedure>()
                    .Where(x => _procedureIds.Contains(x.Id))
                    .ExecuteDeleteAsync();
            }

            if (_patientIds.Count > 0)
            {
                await context.Set<MstPatient>()
                    .Where(x => _patientIds.Contains(x.Id))
                    .ExecuteDeleteAsync();
            }
        }

        // =====================================================================
        // Pencarian pasien
        // =====================================================================

        /// <summary>
        /// Dua puluh empat pasien bernama sama persis tetap terbaca seluruhnya ketika daftarnya
        /// ditelusuri per halaman — jumlah yang sama dengan yang dilaporkan pengguna.
        /// </summary>
        [Fact]
        public async Task PencarianPasien_DuaPuluhEmpatNamaKembar_TerbacaSeluruhnyaLintasHalaman()
        {
            var penanda = $"UjiHalaman{Guid.NewGuid():N}"[..24];

            await SeedPasienKembarAsync(penanda, 24);

            await using var context = _fixture.CreateContext();
            var service = new LabPatientRegistrationService(context, null!);

            var halaman1 = await service.SearchPatientsAsync(
                new LabPatientSearchQuery { Search = penanda, PageNumber = 1, PageSize = 20 });

            var halaman2 = await service.SearchPatientsAsync(
                new LabPatientSearchQuery { Search = penanda, PageNumber = 2, PageSize = 20 });

            Assert.Equal(24, halaman1.TotalData);
            Assert.Equal(2, halaman1.TotalPage);
            Assert.Equal(20, halaman1.Items.Count);
            Assert.Equal(4, halaman2.Items.Count);

            var seluruhnya = halaman1.Items
                .Concat(halaman2.Items)
                .Select(x => x.PatientId)
                .ToList();

            Assert.Equal(24, seluruhnya.Distinct().Count());
        }

        /// <summary>
        /// Halaman yang sama diminta dua kali mengembalikan baris yang sama persis, dalam urutan
        /// yang sama. Tanpa jaminan ini, menyegarkan layar dapat menggeser isi halaman.
        /// </summary>
        [Fact]
        public async Task PencarianPasien_HalamanYangSamaDimintaDuaKali_IsinyaTidakBergeser()
        {
            var penanda = $"UjiUlang{Guid.NewGuid():N}"[..24];

            await SeedPasienKembarAsync(penanda, 12);

            await using var context = _fixture.CreateContext();
            var service = new LabPatientRegistrationService(context, null!);

            var pertama = await service.SearchPatientsAsync(
                new LabPatientSearchQuery { Search = penanda, PageNumber = 2, PageSize = 5 });

            var kedua = await service.SearchPatientsAsync(
                new LabPatientSearchQuery { Search = penanda, PageNumber = 2, PageSize = 5 });

            Assert.Equal(
                pertama.Items.Select(x => x.PatientId),
                kedua.Items.Select(x => x.PatientId));
        }

        // =====================================================================
        // Katalog pemeriksaan
        // =====================================================================

        /// <summary>
        /// Katalog yang memuat beberapa pemeriksaan bernama sama tetap terbaca seluruhnya.
        /// Pemeriksaan yang terlewat tidak akan pernah dapat dipesan lewat layar katalog.
        /// </summary>
        [Fact]
        public async Task Katalog_NamaPemeriksaanKembar_TerbacaSeluruhnyaLintasHalaman()
        {
            var penanda = $"UjiKatalog{Guid.NewGuid():N}"[..24];

            await SeedPemeriksaanKembarAsync(penanda, 5);

            await using var context = _fixture.CreateContext();
            var service = new LabCatalogService(context);

            var halaman1 = await service.GetExaminationsAsync(
                new LabCatalogQuery { Search = penanda, PageNumber = 1, PageSize = 2 });

            var halaman2 = await service.GetExaminationsAsync(
                new LabCatalogQuery { Search = penanda, PageNumber = 2, PageSize = 2 });

            var halaman3 = await service.GetExaminationsAsync(
                new LabCatalogQuery { Search = penanda, PageNumber = 3, PageSize = 2 });

            Assert.Equal(5, halaman1.TotalData);
            Assert.Equal(3, halaman1.TotalPage);
            Assert.Equal(1, halaman3.Items.Count);

            var seluruhnya = halaman1.Items
                .Concat(halaman2.Items)
                .Concat(halaman3.Items)
                .Select(x => x.ProcedureId)
                .ToList();

            Assert.Equal(5, seluruhnya.Distinct().Count());
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        /// <summary>
        /// Sejumlah pasien yang seluruh kunci urutannya seri: namanya sama persis, sehingga
        /// <c>ORDER BY FullName</c> tidak dapat memisahkan satu pun di antara mereka.
        /// </summary>
        private async Task SeedPasienKembarAsync(string penanda, int banyaknya)
        {
            await using var context = _fixture.CreateContext();

            for (var urutan = 0; urutan < banyaknya; urutan++)
            {
                var suffix = Guid.NewGuid().ToString("N")[..12];

                var patient = new MstPatient
                {
                    Id = Guid.NewGuid(),
                    PatientCode = $"PC{suffix}",
                    MedicalRecordNumber = $"MR{suffix}",
                    FullName = penanda,
                    BirthDate = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    IsDelete = false
                };

                context.Set<MstPatient>().Add(patient);
                _patientIds.Add(patient.Id);
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Sejumlah pemeriksaan laboratorium bernama sama persis, masing-masing dengan satu
        /// tarif supaya katalog memperlakukannya sebagai baris yang lengkap.
        /// </summary>
        private async Task SeedPemeriksaanKembarAsync(string penanda, int banyaknya)
        {
            await using var context = _fixture.CreateContext();

            for (var urutan = 0; urutan < banyaknya; urutan++)
            {
                var suffix = Guid.NewGuid().ToString("N")[..12];

                var procedure = new MstProcedure
                {
                    Id = Guid.NewGuid(),
                    ProcedureCode = $"LB{suffix}",
                    ProcedureName = penanda,
                    ProcedureType = "Laboratory",
                    IsLaboratory = true,
                    LabDiscipline = LabDiscipline.ClinicalPathology,
                    IsActive = true,
                    IsDelete = false
                };

                var tariff = new MstTariff
                {
                    Id = Guid.NewGuid(),
                    ProcedureId = procedure.Id,
                    TariffCode = $"TRF{suffix}",
                    TariffName = $"Tarif {penanda}",
                    NormalPrice = 35_000m,
                    IsActive = true,
                    IsDelete = false
                };

                context.Set<MstProcedure>().Add(procedure);
                context.Set<MstTariff>().Add(tariff);

                _procedureIds.Add(procedure.Id);
                _tariffIds.Add(tariff.Id);
            }

            await context.SaveChangesAsync();
        }
    }
}
