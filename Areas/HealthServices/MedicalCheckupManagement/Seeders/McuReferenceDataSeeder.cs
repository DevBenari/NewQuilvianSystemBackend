using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Seeders
{
    /// <summary>
    /// Data referensi USULAN modul MCU (<c>BE-MCU-004</c>, <c>02-backend-architecture.md</c> bagian 9):
    /// empat kategori kelayakan dan dua jenis Pemeriksaan MCU beserta isiannya.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Seluruh isinya usulan, bukan keputusan klinis.</b> Daftar kategori kelayakan menunggu
    /// <c>MCU-OQ-003</c> dan formulir pemeriksaan menunggu <c>MCU-OQ-002</c>; keduanya wajib
    /// diratifikasi dokter MCU sebelum go-live. Karena itu setiap baris membawa
    /// <see cref="UnratifiedMarker"/> pada keterangannya — admin yang membuka master melihat
    /// statusnya tanpa perlu membaca dokumen blueprint. Menghapus penanda itu adalah tindakan
    /// ratifikasi, dan dilakukan admin, bukan seeder.
    /// </para>
    /// <para>
    /// <b>Idempoten dan tidak pernah menimpa.</b> Baris dicari menurut kodenya, termasuk yang
    /// sudah ditandai terhapus, lalu hanya yang belum ada yang ditambahkan. Nama, keterangan, dan
    /// status aktif yang sudah diubah admin tidak disentuh; baris yang dihapus admin tidak diisi
    /// ulang — penghapusan adalah keputusan pengguna.
    /// </para>
    /// <para>
    /// <b>Isian hanya ditanam bersama jenisnya.</b> Bila jenis pemeriksaan berkode sama sudah ada,
    /// isiannya dianggap milik admin seluruhnya: menambah butir ke formulir yang sudah disusun
    /// admin bisa bertabrakan dengan nomor butirnya dan mengubah formulir tanpa seorang pun
    /// memutuskannya.
    /// </para>
    /// </remarks>
    public static class McuReferenceDataSeeder
    {
        public const string UnratifiedMarker = "USULAN — belum diratifikasi dokter MCU";

        private sealed record Kategori(string Id, string Kode, string Nama);

        private sealed record Isian(
            string Id,
            int Nomor,
            string Kode,
            string Label,
            McuExaminationFieldType Tipe,
            string? Satuan,
            bool Wajib);

        private sealed record JenisPemeriksaan(string Id, string Kode, string Nama, Isian[] Isian);

        /// <summary>Usulan <c>MCU-OQ-003</c>.</summary>
        private static readonly Kategori[] KategoriBaseline =
        {
            new("c4d1a7e0-0001-4e51-9a01-3b7c2d8e9f01", "FIT", "Fit"),
            new("c4d1a7e0-0002-4e51-9a01-3b7c2d8e9f02", "FIT_DENGAN_CATATAN", "Fit dengan catatan"),
            new("c4d1a7e0-0003-4e51-9a01-3b7c2d8e9f03", "UNFIT_SEMENTARA", "Unfit sementara"),
            new("c4d1a7e0-0004-4e51-9a01-3b7c2d8e9f04", "UNFIT", "Unfit")
        };

        /// <summary>
        /// Usulan <c>MCU-OQ-002</c>. Isian tanda vital mengikuti bagian 9 blueprint (tekanan darah,
        /// nadi, tinggi, berat). Pemeriksaan Fisik sengaja hanya satu isian teks: formulir fisik
        /// dokter MCU belum diterima, dan menebak butir klinisnya lebih berbahaya daripada
        /// memberinya ruang tulis bebas.
        /// </summary>
        private static readonly JenisPemeriksaan[] JenisBaseline =
        {
            new("d5e2b8f0-0001-4f62-8b02-4c8d3e9fa001", "FISIK", "Pemeriksaan Fisik",
            [
                new("d5e2b8f0-0101-4f62-8b02-4c8d3e9fa101", 1, "TEMUAN_FISIK", "Temuan pemeriksaan fisik", McuExaminationFieldType.Text, null, true)
            ]),
            new("d5e2b8f0-0002-4f62-8b02-4c8d3e9fa002", "TANDA_VITAL", "Tanda Vital & Antropometri",
            [
                new("d5e2b8f0-0201-4f62-8b02-4c8d3e9fa201", 1, "TEKANAN_SISTOLIK", "Tekanan darah sistolik", McuExaminationFieldType.Number, "mmHg", true),
                new("d5e2b8f0-0202-4f62-8b02-4c8d3e9fa202", 2, "TEKANAN_DIASTOLIK", "Tekanan darah diastolik", McuExaminationFieldType.Number, "mmHg", true),
                new("d5e2b8f0-0203-4f62-8b02-4c8d3e9fa203", 3, "NADI", "Nadi", McuExaminationFieldType.Number, "x/menit", true),
                new("d5e2b8f0-0204-4f62-8b02-4c8d3e9fa204", 4, "TINGGI_BADAN", "Tinggi badan", McuExaminationFieldType.Number, "cm", true),
                new("d5e2b8f0-0205-4f62-8b02-4c8d3e9fa205", 5, "BERAT_BADAN", "Berat badan", McuExaminationFieldType.Number, "kg", true)
            ])
        };

        public sealed record SeedResult(int FitnessCategory, int ExaminationType, int ExaminationField);

        public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(McuReferenceDataSeeder));

            var seedEnabled = configuration.GetValue<bool?>("SeedDefaultData:Enabled") ?? true;
            if (!seedEnabled)
            {
                logger.LogInformation("Seeder data referensi MCU dilewati karena SeedDefaultData dimatikan.");
                return;
            }

            await SeedAsync(dbContext, logger, cancellationToken);
        }

        /// <summary>
        /// Menjalankan pengisian terhadap satu <see cref="ApplicationDbContext"/> yang sudah ada.
        /// Dipisahkan agar perilakunya dapat diuji tanpa membangun service provider.
        /// </summary>
        public static async Task<SeedResult> SeedAsync(
            ApplicationDbContext dbContext,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var kategoriAda = new HashSet<string>(
                await dbContext.McuFitnessCategories.IgnoreQueryFilters().AsNoTracking()
                    .Select(x => x.FitnessCategoryCode)
                    .ToListAsync(cancellationToken),
                StringComparer.OrdinalIgnoreCase);

            var kategoriBaru = 0;
            foreach (var kategori in KategoriBaseline.Where(x => !kategoriAda.Contains(x.Kode)))
            {
                dbContext.McuFitnessCategories.Add(new McuFitnessCategory
                {
                    Id = Guid.Parse(kategori.Id),
                    FitnessCategoryCode = kategori.Kode,
                    FitnessCategoryName = kategori.Nama,
                    Description = $"{UnratifiedMarker} (MCU-OQ-003).",
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = Guid.Empty
                });
                kategoriBaru++;
            }

            var jenisAda = new HashSet<string>(
                await dbContext.McuExaminationTypes.IgnoreQueryFilters().AsNoTracking()
                    .Select(x => x.ExaminationTypeCode)
                    .ToListAsync(cancellationToken),
                StringComparer.OrdinalIgnoreCase);

            var jenisBaru = 0;
            var isianBaru = 0;
            foreach (var jenis in JenisBaseline.Where(x => !jenisAda.Contains(x.Kode)))
            {
                var jenisId = Guid.Parse(jenis.Id);

                dbContext.McuExaminationTypes.Add(new McuExaminationType
                {
                    Id = jenisId,
                    ExaminationTypeCode = jenis.Kode,
                    ExaminationTypeName = jenis.Nama,
                    Description = $"{UnratifiedMarker} — isian formulir ikut usulan (MCU-OQ-002).",
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = Guid.Empty
                });
                jenisBaru++;

                foreach (var isian in jenis.Isian)
                {
                    dbContext.McuExaminationFields.Add(new McuExaminationField
                    {
                        Id = Guid.Parse(isian.Id),
                        ExaminationTypeId = jenisId,
                        FieldCode = isian.Kode,
                        FieldLabel = isian.Label,
                        FieldType = isian.Tipe,
                        UnitName = isian.Satuan,
                        IsRequired = isian.Wajib,
                        FieldNumber = isian.Nomor,
                        CreateDateTime = now,
                        CreateBy = Guid.Empty
                    });
                    isianBaru++;
                }
            }

            var result = new SeedResult(kategoriBaru, jenisBaru, isianBaru);

            if (kategoriBaru + jenisBaru == 0)
            {
                logger.LogInformation("Data referensi MCU sudah lengkap; tidak ada baris baru yang ditambahkan.");
                return result;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Seeder data referensi MCU menambahkan {Kategori} kategori kelayakan, {Jenis} jenis pemeriksaan, " +
                "dan {Isian} isian formulir. Seluruhnya usulan dan wajib diratifikasi dokter MCU sebelum go-live.",
                result.FitnessCategory, result.ExaminationType, result.ExaminationField);

            return result;
        }
    }
}
