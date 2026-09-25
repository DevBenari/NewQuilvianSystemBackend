using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Seeders
{
    /// <summary>
    /// Data master awal modul Hemodialisa (<c>BE-HMD-03</c>, <c>02-backend-architecture.md</c> bagian 9).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Modul dengan master kosong tidak dapat dipakai sama sekali. Seeder ini mengisi: satu unit HD
    /// pada <c>MstServiceUnit</c> (<c>ServiceUnitType = Hemodialysis</c>), satu tindakan
    /// "Hemodialisis" pada <c>MstProcedure</c>, dua belas butir checklist Pra-HD, lima butir
    /// kesiapan unit, dan satu baris <c>HmdSetting</c>.
    /// </para>
    /// <para>
    /// <b>Seluruh dua belas butir checklist bertanda tidak boleh dilewati</b> (<c>HMD-ASM-001</c>).
    /// Itu bukan kelalaian: selama badan klinis belum memutuskan (<c>HMD-GATE-002</c>), dokter tidak
    /// dapat melewati satu butir pun. Badan klinis kelak cukup mengubah penandanya lewat
    /// <c>PATCH .../hemodialysis-checklist-items/{id}/overridable</c>.
    /// </para>
    /// <para>
    /// <b>Idempoten.</b> Baris dicari menurut kodenya, termasuk yang sudah ditandai terhapus, lalu
    /// hanya yang belum ada yang ditambahkan. Nilai yang sudah diubah admin tidak pernah ditimpa.
    /// Mesin dan station sengaja <b>tidak</b> diisi: keduanya inventaris nyata unit dan wajib
    /// didaftarkan admin sesuai label fisiknya.
    /// </para>
    /// </remarks>
    public static class HemodialysisMasterDataSeeder
    {
        public const string ServiceUnitCode = "SU-HMD-001";
        public const string ProcedureCode = "PR-HMD-001";

        private static readonly Guid ServiceUnitId = Guid.Parse("7d1e4c20-0001-4a10-8b01-5e2d9c6f7a01");
        private static readonly Guid ProcedureId = Guid.Parse("7d1e4c20-0002-4a10-8b01-5e2d9c6f7a02");
        private static readonly Guid SettingId = Guid.Parse("7d1e4c20-0003-4a10-8b01-5e2d9c6f7a03");

        private sealed record ButirChecklist(string Id, string Kode, string Nama, HmdChecklistCategory Kategori, int Urutan);

        private sealed record ButirKesiapan(string Id, string Kode, string Nama, HmdReadinessCategory Kategori, bool PunyaMasaBerlaku, int Urutan);

        /// <summary>Dua belas butir checklist Pra-HD — <c>02-backend-architecture.md</c> bagian 9.</summary>
        private static readonly ButirChecklist[] ChecklistBaseline =
        {
            new("8e2f5d30-0001-4b20-9c02-6f3e0d7a8b01", "IDENTITY", "Identitas pasien cocok", HmdChecklistCategory.Identity, 1),
            new("8e2f5d30-0002-4b20-9c02-6f3e0d7a8b02", "ENCOUNTER", "Konteks kunjungan sah", HmdChecklistCategory.Identity, 2),
            new("8e2f5d30-0003-4b20-9c02-6f3e0d7a8b03", "EPISODE", "Episode HD aktif", HmdChecklistCategory.ClinicalContext, 3),
            new("8e2f5d30-0004-4b20-9c02-6f3e0d7a8b04", "PRESCRIPTION", "Resep HD aktif tersedia", HmdChecklistCategory.ClinicalContext, 4),
            new("8e2f5d30-0005-4b20-9c02-6f3e0d7a8b05", "CONSENT", "Persetujuan tindakan sah tersedia", HmdChecklistCategory.ClinicalContext, 5),
            new("8e2f5d30-0006-4b20-9c02-6f3e0d7a8b06", "ALLERGY", "Riwayat alergi ditinjau", HmdChecklistCategory.ClinicalContext, 6),
            new("8e2f5d30-0007-4b20-9c02-6f3e0d7a8b07", "VASCULAR_ACCESS", "Akses vaskular layak dipakai", HmdChecklistCategory.ClinicalContext, 7),
            new("8e2f5d30-0008-4b20-9c02-6f3e0d7a8b08", "ISOLATION", "Kebutuhan isolasi terpenuhi", HmdChecklistCategory.ClinicalContext, 8),
            new("8e2f5d30-0009-4b20-9c02-6f3e0d7a8b09", "MACHINE", "Mesin siap dan sesuai kebutuhan isolasi", HmdChecklistCategory.Resource, 9),
            new("8e2f5d30-0010-4b20-9c02-6f3e0d7a8b10", "STATION", "Station tersedia", HmdChecklistCategory.Resource, 10),
            new("8e2f5d30-0011-4b20-9c02-6f3e0d7a8b11", "WATER", "Pengolahan air dinyatakan siap", HmdChecklistCategory.Resource, 11),
            new("8e2f5d30-0012-4b20-9c02-6f3e0d7a8b12", "SUPPLY", "Obat dan BMHP tersedia", HmdChecklistCategory.Resource, 12)
        };

        /// <summary>Lima butir kesiapan unit — PRD <c>FR-HD-011</c> dan <c>FR-HD-012</c>.</summary>
        private static readonly ButirKesiapan[] ReadinessBaselineList =
        {
            new("9f3a6e40-0001-4c30-8d03-7a4f1e8b9c01", "MACHINE", "Mesin hemodialisa siap dipakai", HmdReadinessCategory.Machine, false, 1),
            new("9f3a6e40-0002-4c30-8d03-7a4f1e8b9c02", "STATION", "Station tersedia dan bersih", HmdReadinessCategory.Station, false, 2),
            new("9f3a6e40-0003-4c30-8d03-7a4f1e8b9c03", "WATER", "Hasil pemeriksaan pengolahan air masih berlaku", HmdReadinessCategory.Water, true, 3),
            new("9f3a6e40-0004-4c30-8d03-7a4f1e8b9c04", "SUPPLY", "Obat dan BMHP tersedia", HmdReadinessCategory.Supply, false, 4),
            new("9f3a6e40-0005-4c30-8d03-7a4f1e8b9c05", "STAFF", "Tenaga dialisis tersedia untuk shift", HmdReadinessCategory.Staff, false, 5)
        };

        public sealed record SeedResult(int ServiceUnit, int Procedure, int ChecklistItem, int ReadinessItem, int Setting);

        public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(HemodialysisMasterDataSeeder));

            var seedEnabled = configuration.GetValue<bool?>("SeedDefaultData:Enabled") ?? true;
            if (!seedEnabled)
            {
                logger.LogInformation("Seeder data master hemodialisa dilewati karena SeedDefaultData dimatikan.");
                return;
            }

            await SeedAsync(dbContext, logger, cancellationToken);
        }

        public static async Task<SeedResult> SeedAsync(ApplicationDbContext dbContext, ILogger logger, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var (serviceUnitId, serviceUnitAdded) = await EnsureServiceUnitAsync(dbContext, now, cancellationToken);
            var (procedureId, procedureAdded) = await EnsureProcedureAsync(dbContext, now, cancellationToken);
            var checklistAdded = await EnsureChecklistItemsAsync(dbContext, now, cancellationToken);
            var readinessAdded = await EnsureReadinessItemsAsync(dbContext, now, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            var settingAdded = await EnsureSettingAsync(dbContext, serviceUnitId, procedureId, now, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            var result = new SeedResult(serviceUnitAdded, procedureAdded, checklistAdded, readinessAdded, settingAdded);

            logger.LogInformation(
                "Seeder data master hemodialisa menambahkan {Unit} unit, {Tindakan} tindakan, {Checklist} butir checklist, " +
                "{Kesiapan} butir kesiapan, dan {Pengaturan} pengaturan unit.",
                result.ServiceUnit, result.Procedure, result.ChecklistItem, result.ReadinessItem, result.Setting);

            var mesinSiap = await dbContext.HmdMachines.AsNoTracking()
                .CountAsync(x => x.ServiceUnitId == serviceUnitId && !x.IsDelete && x.IsActive, cancellationToken);
            if (mesinSiap == 0)
            {
                // Sengaja ditulis setiap kali server menyala: selama belum ada mesin, tidak satu pun
                // sesi dapat dijadwalkan. Mesin adalah inventaris nyata dan diisi admin, bukan seeder.
                logger.LogWarning(
                    "Unit hemodialisa belum punya satu pun mesin aktif. Penjadwalan sesi akan ditolak sampai admin " +
                    "mendaftarkan mesin dan station sesuai label fisiknya.");
            }

            return result;
        }

        private static async Task<(Guid Id, int Added)> EnsureServiceUnitAsync(
            ApplicationDbContext dbContext, DateTime now, CancellationToken cancellationToken)
        {
            var existing = await dbContext.Set<MstServiceUnit>().AsNoTracking()
                .Where(x => !x.IsDelete && (x.ServiceUnitType == ServiceUnitType.Hemodialysis || x.ServiceUnitCode == ServiceUnitCode))
                .OrderBy(x => x.CreateDateTime)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (existing.HasValue)
                return (existing.Value, 0);

            dbContext.Set<MstServiceUnit>().Add(new MstServiceUnit
            {
                Id = ServiceUnitId,
                ServiceUnitCode = ServiceUnitCode,
                ServiceUnitName = "Unit Hemodialisa",
                ServiceUnitType = ServiceUnitType.Hemodialysis,
                ShortName = "HD",
                IsAvailableForRegistration = true,
                IsQueueRequired = false,
                IsDoctorRequired = true,
                Description = "Unit hemodialisa, dibentuk seeder HMD-BP-001. Sesuaikan lokasi dan organisasi lewat master unit layanan.",
                IsActive = true,
                CreateDateTime = now,
                CreateBy = Guid.Empty
            });

            return (ServiceUnitId, 1);
        }

        private static async Task<(Guid Id, int Added)> EnsureProcedureAsync(
            ApplicationDbContext dbContext, DateTime now, CancellationToken cancellationToken)
        {
            var existing = await dbContext.Set<MstProcedure>().AsNoTracking()
                .Where(x => x.ProcedureCode == ProcedureCode)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (existing.HasValue)
                return (existing.Value, 0);

            dbContext.Set<MstProcedure>().Add(new MstProcedure
            {
                Id = ProcedureId,
                ProcedureCode = ProcedureCode,
                ProcedureName = "Hemodialisis",
                ProcedureCategoryName = "Hemodialisa",
                ProcedureGroupName = "Hemodialisa",
                ProcedureType = "Therapy",
                IsDoctorAction = false,
                IsNursingAction = true,
                IsTherapy = true,
                IsNeedDoctor = true,
                IsAvailableForOutpatient = true,
                IsAvailableForInpatient = true,
                IsAvailableForEmergency = true,
                EstimatedDurationMinutes = 240,
                Description = "Tindakan hemodialisis per sesi, dibentuk seeder HMD-BP-001. Tarifnya ditetapkan lewat katalog tarif rumah sakit.",
                IsActive = true,
                CreateDateTime = now,
                CreateBy = Guid.Empty
            });

            return (ProcedureId, 1);
        }

        private static async Task<int> EnsureChecklistItemsAsync(
            ApplicationDbContext dbContext, DateTime now, CancellationToken cancellationToken)
        {
            // Baris yang sudah ditandai terhapus ikut dihitung supaya kodenya tidak diisi ulang.
            var kodeAda = new HashSet<string>(
                await dbContext.HmdChecklistItems.AsNoTracking().Select(x => x.ItemCode).ToListAsync(cancellationToken),
                StringComparer.OrdinalIgnoreCase);

            var ditambah = 0;
            foreach (var butir in ChecklistBaseline.Where(x => !kodeAda.Contains(x.Kode)))
            {
                dbContext.HmdChecklistItems.Add(new HmdChecklistItem
                {
                    Id = Guid.Parse(butir.Id),
                    ItemCode = butir.Kode,
                    ItemName = butir.Nama,
                    Category = butir.Kategori,
                    IsMandatory = true,
                    IsOverridable = false,
                    CheckSequence = butir.Urutan,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = Guid.Empty
                });
                ditambah++;
            }

            return ditambah;
        }

        private static async Task<int> EnsureReadinessItemsAsync(
            ApplicationDbContext dbContext, DateTime now, CancellationToken cancellationToken)
        {
            var kodeAda = new HashSet<string>(
                await dbContext.HmdReadinessItems.AsNoTracking().Select(x => x.ItemCode).ToListAsync(cancellationToken),
                StringComparer.OrdinalIgnoreCase);

            var ditambah = 0;
            foreach (var butir in ReadinessBaselineList.Where(x => !kodeAda.Contains(x.Kode)))
            {
                dbContext.HmdReadinessItems.Add(new HmdReadinessItem
                {
                    Id = Guid.Parse(butir.Id),
                    ItemCode = butir.Kode,
                    ItemName = butir.Nama,
                    Category = butir.Kategori,
                    IsMandatory = true,
                    RequiresResultDate = butir.PunyaMasaBerlaku,
                    CheckSequence = butir.Urutan,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = Guid.Empty
                });
                ditambah++;
            }

            return ditambah;
        }

        /// <summary>
        /// Satu baris pengaturan untuk unit HD dengan nilai awal <c>BE-HMD-03</c>. Pengaturan yang
        /// sudah ada tidak ditimpa; hanya tindakan hemodialisa yang dilengkapi bila masih kosong.
        /// </summary>
        private static async Task<int> EnsureSettingAsync(
            ApplicationDbContext dbContext, Guid serviceUnitId, Guid procedureId, DateTime now, CancellationToken cancellationToken)
        {
            var setting = await dbContext.HmdSettings.FirstOrDefaultAsync(x => x.ServiceUnitId == serviceUnitId && !x.IsDelete, cancellationToken);

            if (setting != null)
            {
                if (!setting.ProcedureId.HasValue)
                {
                    setting.ProcedureId = procedureId;
                    setting.UpdateDateTime = now;
                }
                return 0;
            }

            dbContext.HmdSettings.Add(new HmdSetting
            {
                Id = SettingId,
                ServiceUnitId = serviceUnitId,
                MaxPatientsPerNurse = 3,
                EnforceNurseRatio = false,
                WaterResultValidityHours = 720,
                EnforceCompetencyCheck = false,
                AllowMultipleActiveEpisodePerPatient = false,
                SessionStartGraceMinutes = 60,
                RequireDifferentSigner = true,
                ProcedureId = procedureId,
                CreateDateTime = now,
                CreateBy = Guid.Empty
            });

            return 1;
        }
    }
}
