using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Seeders
{
    /// <summary>
    /// Mengisi katalog alasan penolakan sampel dengan baseline implementasi.
    ///
    /// <b>Mengapa seeder ini ada.</b> Tabel alasan penolakan yang kosong membuat petugas tidak
    /// dapat menolak sampel sama sekali — dan sampel yang jelas tidak layak pun akhirnya tetap
    /// diperiksa, lalu menghasilkan angka yang menyesatkan dokter. Baris baseline memang sudah
    /// diisi migration <c>20260824091610_AddLaboratorySpecimenLifecycle</c>, tetapi migration
    /// hanya menolong lingkungan yang menjalankannya dari awal. Seeder ini menjadi jaring
    /// pengaman untuk lingkungan baru yang databasenya disiapkan dengan cara lain
    /// (<c>BE-LAB-06</c>, <c>BR-15</c>).
    ///
    /// <b>Yang sengaja tidak dikerjakan seeder ini.</b> Ia hanya menambah kode yang belum ada,
    /// dan tidak pernah menimpa baris yang sudah tersimpan. Nama, keterangan, urutan tampil,
    /// dan status aktif adalah milik kepala instalasi; kedua penanda terkunci adalah milik
    /// administrator sistem. Menimpanya saat aplikasi menyala berarti membatalkan keputusan
    /// mereka diam-diam setiap kali server dinyalakan ulang. Semantik ini sama dengan
    /// <c>ON CONFLICT DO NOTHING</c> yang dipakai migration.
    ///
    /// Baris yang sudah ditandai terhapus juga dihormati: kodenya tidak diisi ulang, karena
    /// penghapusannya adalah keputusan pengguna.
    ///
    /// Daftar bawaan ini baseline implementasi, bukan SOP klinis final. Penanda kesalahan
    /// internal pada baris baseline ditetapkan bersama Billing (<c>LAB-INH-011</c>).
    /// </summary>
    public static class LabRejectionReasonSeeder
    {
        /// <summary>
        /// Baris baseline. <c>Id</c> ditetapkan tetap — bukan dibangkitkan — dan nilainya sama
        /// persis dengan yang dipakai migration, supaya sebuah alasan penolakan memiliki
        /// identitas yang sama di setiap lingkungan dan dapat dirujuk dengan pasti.
        /// </summary>
        private static readonly BaselineReason[] BaselineReasons =
        {
            new("1f2a4c60-0001-4a10-9f01-6b1d0a5e7c01", "IDENTITY_MISMATCH", "Identitas tidak cocok", true, false, 1),
            new("1f2a4c60-0002-4a10-9f01-6b1d0a5e7c02", "LABELING_ISSUE", "Masalah pelabelan", true, false, 2),
            new("1f2a4c60-0003-4a10-9f01-6b1d0a5e7c03", "SPECIMEN_TYPE_OR_CONTAINER_MISMATCH", "Jenis sampel atau wadah tidak sesuai", true, false, 3),
            new("1f2a4c60-0004-4a10-9f01-6b1d0a5e7c04", "INSUFFICIENT_QUANTITY", "Jumlah sampel tidak mencukupi", false, false, 4),
            new("1f2a4c60-0005-4a10-9f01-6b1d0a5e7c05", "SPECIMEN_INTEGRITY_OR_QUALITY_ISSUE", "Mutu atau keutuhan sampel bermasalah", false, false, 5),
            new("1f2a4c60-0006-4a10-9f01-6b1d0a5e7c06", "COLLECTION_ISSUE", "Masalah pada proses pengambilan", true, false, 6),
            new("1f2a4c60-0007-4a10-9f01-6b1d0a5e7c07", "TRANSPORT_OR_STORAGE_ISSUE", "Masalah pengiriman atau penyimpanan", true, false, 7),
            new("1f2a4c60-0008-4a10-9f01-6b1d0a5e7c08", "ORDER_SPECIMEN_MISMATCH", "Sampel tidak sesuai pesanan", true, false, 8),
            new("1f2a4c60-0009-4a10-9f01-6b1d0a5e7c09", "DUPLICATE_OR_NOT_REQUIRED", "Duplikat atau tidak diperlukan", false, false, 9),
            new("1f2a4c60-0010-4a10-9f01-6b1d0a5e7c10", "OTHER", "Lainnya", false, true, 99)
        };

        public static async Task SeedAsync(
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(LabRejectionReasonSeeder));

            var seedEnabled = configuration.GetValue<bool?>("SeedDefaultData:Enabled") ?? true;

            if (!seedEnabled)
            {
                logger.LogInformation("Seeder alasan penolakan sampel dilewati karena SeedDefaultData dimatikan.");
                return;
            }

            await SeedAsync(dbContext, logger, cancellationToken);
        }

        /// <summary>
        /// Menjalankan pengisian terhadap satu <see cref="ApplicationDbContext"/> yang sudah ada.
        /// Dipisahkan agar perilakunya dapat diuji tanpa membangun service provider.
        /// </summary>
        public static async Task<int> SeedAsync(
            ApplicationDbContext dbContext,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            // Baris yang sudah ditandai terhapus ikut dihitung, supaya kodenya tidak diisi ulang.
            var existingCodes = await dbContext.MstLabRejectionReasons
                .AsNoTracking()
                .Select(x => x.ReasonCode)
                .ToListAsync(cancellationToken);

            var existing = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

            var now = DateTime.UtcNow;
            var added = 0;

            foreach (var reason in BaselineReasons)
            {
                if (existing.Contains(reason.Code))
                    continue;

                dbContext.MstLabRejectionReasons.Add(new MstLabRejectionReason
                {
                    Id = Guid.Parse(reason.Id),
                    ReasonCode = reason.Code,
                    ReasonName = reason.Name,
                    Description = null,
                    IsInternalHospitalError = reason.IsInternalHospitalError,
                    RequiresNote = reason.RequiresNote,
                    IsActive = true,
                    SortOrder = reason.SortOrder,
                    CreateDateTime = now,
                    CreateBy = Guid.Empty
                });

                added++;
            }

            if (added == 0)
            {
                logger.LogInformation("Alasan penolakan sampel sudah lengkap; tidak ada baris baru yang ditambahkan.");
                return 0;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Seeder alasan penolakan sampel menambahkan {AddedCount} baris baseline.",
                added);

            return added;
        }

        private sealed record BaselineReason(
            string Id,
            string Code,
            string Name,
            bool IsInternalHospitalError,
            bool RequiresNote,
            int SortOrder);
    }
}
