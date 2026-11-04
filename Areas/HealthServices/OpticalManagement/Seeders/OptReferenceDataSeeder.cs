using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Seeders
{
    /// <summary>
    /// Data master awal modul Optik (<c>02-backend-architecture.md</c> bagian 8).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>BE-OPT-001</b> mengisi satu unit layanan Optik pada <c>MstServiceUnit</c>
    /// (<c>ServiceUnitType = Optical</c>, <c>OPT-DEC-032</c>, disetujui Master Data <c>OPT-DEC-053</c>).
    /// <b>BE-OPT-004</b> menambah satu baris <see cref="OptSetting"/> untuk unit itu dengan angka bawaan
    /// <c>OPT-DEC-009</c>, <c>OPT-DEC-040</c>, <c>OPT-DEC-041</c>, dan rentang BC/DIA usulan (<c>PROPOSED</c>, dikonfirmasi RO/Sp.M).
    /// </para>
    /// <para>
    /// Kunjungan Optik dibuat Registrasi lewat jalur unit penunjang (<c>EncounterIntakeService</c>, jenis
    /// <c>Outpatient</c>), sehingga unit ini tidak memakai antrean dokter: pemeriksa adalah RO atau Sp.M
    /// (<c>OPT-DEC-005</c>), bukan dokter poli.
    /// </para>
    /// <para>
    /// <b>Idempoten.</b> Unit dicari menurut jenis <c>Optical</c> atau kodenya, termasuk yang sudah ditandai
    /// terhapus; pengaturan dicari menurut unitnya. Hanya yang belum ada yang ditambahkan, dan nilai yang
    /// sudah diubah admin tidak pernah ditimpa.
    /// </para>
    /// <para>
    /// <b>Tabel Optik belum ada bukan alasan mematikan startup.</b> Bawaan <c>Seeders:ContinueOnFailure</c> adalah
    /// <c>false</c>, sehingga seeder yang melempar galat menghentikan seluruh aplikasi. Selama migration
    /// <c>AddOpticalManagement</c> belum diterapkan pada basis data yang dipakai, pengisian pengaturan
    /// dilewati dengan peringatan (PostgreSQL <c>42P01</c>, tabel tidak ada); unit layanan tetap diisi. Galat
    /// lain tetap dilempar.
    /// </para>
    /// </remarks>
    public static class OptReferenceDataSeeder
    {
        public const string ServiceUnitCode = "SU-OPT-001";

        private static readonly Guid ServiceUnitId = Guid.Parse("3c9e1b70-0001-4d40-9e04-8b5f2a9c0d01");
        private static readonly Guid SettingId = Guid.Parse("3c9e1b70-0002-4d40-9e04-8b5f2a9c0d02");

        public sealed record SeedResult(int ServiceUnit, int Setting, bool SettingSkippedMissingTable);

        public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(OptReferenceDataSeeder));

            var seedEnabled = configuration.GetValue<bool?>("SeedDefaultData:Enabled") ?? true;
            if (!seedEnabled)
            {
                logger.LogInformation("Seeder data master Optik dilewati karena SeedDefaultData dimatikan.");
                return;
            }

            await SeedAsync(dbContext, logger, cancellationToken);
        }

        public static async Task<SeedResult> SeedAsync(ApplicationDbContext dbContext, ILogger logger, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var serviceUnitAdded = await EnsureServiceUnitAsync(dbContext, now, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            var settingAdded = 0;
            var settingSkipped = false;
            try
            {
                settingAdded = await EnsureSettingAsync(dbContext, now, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex) when (IsMissingTable(ex))
            {
                settingSkipped = true;
                // Membuang baris pengaturan yang mungkin tertahan agar konteks tetap bersih untuk pemanggil.
                foreach (var entry in dbContext.ChangeTracker.Entries<OptSetting>().ToList())
                    entry.State = EntityState.Detached;

                logger.LogWarning(
                    "Tabel OptSetting belum ada pada basis data ini; pengaturan Optik dilewati. " +
                    "Terapkan migration AddOpticalManagement, lalu nyalakan ulang agar pengaturan bawaan terisi.");
            }

            var result = new SeedResult(serviceUnitAdded, settingAdded, settingSkipped);

            logger.LogInformation(
                "Seeder data master Optik menambahkan {Unit} unit layanan dan {Pengaturan} pengaturan unit.",
                result.ServiceUnit, result.Setting);

            return result;
        }

        private static async Task<int> EnsureServiceUnitAsync(
            ApplicationDbContext dbContext, DateTime now, CancellationToken cancellationToken)
        {
            var exists = await dbContext.Set<MstServiceUnit>().AsNoTracking()
                .AnyAsync(x => x.ServiceUnitType == ServiceUnitType.Optical || x.ServiceUnitCode == ServiceUnitCode, cancellationToken);

            if (exists)
                return 0;

            dbContext.Set<MstServiceUnit>().Add(new MstServiceUnit
            {
                Id = ServiceUnitId,
                ServiceUnitCode = ServiceUnitCode,
                ServiceUnitName = "Unit Optik",
                ServiceUnitType = ServiceUnitType.Optical,
                ShortName = "OPT",
                IsAvailableForRegistration = true,
                IsAvailableForKiosk = false,
                IsAvailableForAppointment = false,
                IsAvailableForBloodOrder = false,
                IsQueueRequired = false,
                IsDoctorRequired = false,
                IsScreeningRequired = false,
                Description = "Unit Optik, dibentuk seeder OPT-BP-001 (BE-OPT-001). Sesuaikan lokasi dan organisasi lewat master unit layanan.",
                IsActive = true,
                CreateDateTime = now,
                CreateBy = Guid.Empty
            });

            return 1;
        }

        /// <summary>
        /// Satu <see cref="OptSetting"/> untuk unit Optik tertua yang belum terhapus. Unit Optik yang belum ada
        /// (misalnya dihapus admin) berarti tidak ada pengaturan yang diisi.
        /// </summary>
        private static async Task<int> EnsureSettingAsync(
            ApplicationDbContext dbContext, DateTime now, CancellationToken cancellationToken)
        {
            var unitId = await dbContext.Set<MstServiceUnit>().AsNoTracking()
                .Where(x => x.ServiceUnitType == ServiceUnitType.Optical && !x.IsDelete)
                .OrderBy(x => x.CreateDateTime)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (unitId is null)
                return 0;

            var exists = await dbContext.OptSettings.AsNoTracking()
                .AnyAsync(x => x.ServiceUnitId == unitId.Value, cancellationToken);

            if (exists)
                return 0;

            dbContext.OptSettings.Add(new OptSetting
            {
                Id = SettingId,
                ServiceUnitId = unitId.Value,
                DownPaymentMinPercent = 50m,
                PickupReminderDays = 30,
                NotCollectedCloseDays = 90,
                PrescriptionValidityMonths = 12,
                WarrantyFitDays = 30,
                WarrantyLensDefectMonths = 6,
                WarrantyFrameDefectMonths = 6,
                ContactLensBaseCurveMin = 7.50m,
                ContactLensBaseCurveMax = 9.50m,
                ContactLensDiameterMin = 13.00m,
                ContactLensDiameterMax = 15.00m,
                CreateDateTime = now,
                CreateBy = Guid.Empty
            });

            return 1;
        }

        private static bool IsMissingTable(Exception ex)
        {
            for (var e = ex; e is not null; e = e.InnerException)
            {
                if (e is PostgresException { SqlState: PostgresErrorCodes.UndefinedTable })
                    return true;
            }

            return false;
        }
    }
}
