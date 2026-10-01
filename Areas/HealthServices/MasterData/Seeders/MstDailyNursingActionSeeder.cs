using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Seeders
{
    /// <summary>
    /// Seeder Inisial Master Data Tindakan Harian Keperawatan Rawat Inap (19 Tindakan Standar RS).
    /// Mengisi 19 butir tindakan awal yang siap digunakan pada checklist harian dan dapat di-CRUD bebas oleh admin RS.
    /// </summary>
    public static class MstDailyNursingActionSeeder
    {
        public static async Task SeedAsync(
            IServiceProvider serviceProvider,
            CancellationToken ct = default)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await SeedAsync(db, Guid.Empty, ct);
        }

        public static async Task SeedAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var initialActions = GetInitialActions(actorUserId, now);

            foreach (var action in initialActions)
            {
                var existing = await db.MstDailyNursingActions
                    .FirstOrDefaultAsync(x => x.ActionCode == action.ActionCode, ct);

                if (existing == null)
                {
                    db.MstDailyNursingActions.Add(action);
                }
            }

            await db.SaveChangesAsync(ct);
        }

        public static List<MstDailyNursingAction> GetInitialActions(Guid actorUserId, DateTime now)
        {
            return new List<MstDailyNursingAction>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ActionCode = "ACT_O2",
                    ActionName = "Memberikan oksigen",
                    Category = "Respirasi & Oksigenasi",
                    DefaultNotes = "Nasal kanul / masker oksigen",
                    SortOrder = 1,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ActionCode = "ACT_SUCTION",
                    ActionName = "Suction",
                    Category = "Respirasi & Oksigenasi",
                    DefaultNotes = "Pengisapan lendir jalan napas",
                    SortOrder = 2,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ActionCode = "ACT_BATUK_EFEKTIF",
                    ActionName = "Latihan batuk efektif",
                    Category = "Respirasi & Oksigenasi",
                    DefaultNotes = "Edukasi & latihan batuk efektif",
                    SortOrder = 3,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ActionCode = "ACT_NEBULIZER",
                    ActionName = "Memasang nebulizer",
                    Category = "Respirasi & Oksigenasi",
                    DefaultNotes = "Inhalasi terapi pernapasan",
                    SortOrder = 4,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ActionCode = "ACT_GANTI_KATETER",
                    ActionName = "Ganti kateter urin",
                    Category = "Eliminasi & Kateterisasi",
                    DefaultNotes = "Perawatan / penggantian kateter urine",
                    SortOrder = 5,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ActionCode = "ACT_IRIGASI_KATETER",
                    ActionName = "Irigasi kateter",
                    Category = "Eliminasi & Kateterisasi",
                    DefaultNotes = "Irigasi spuit cairan steril",
                    SortOrder = 6,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ActionCode = "ACT_PASANG_PAMPERS",
                    ActionName = "Pemasangan pampers",
                    Category = "Eliminasi & Kateterisasi",
                    DefaultNotes = "Penggantian popok / pampers dewasa",
                    SortOrder = 7,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ActionCode = "ACT_GANTI_STOMA_BAG",
                    ActionName = "Mengganti stoma bag",
                    Category = "Eliminasi & Kateterisasi",
                    DefaultNotes = "Perawatan stoma & ganti kantong",
                    SortOrder = 8,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ActionCode = "ACT_GANTI_INFUS",
                    ActionName = "Ganti infus",
                    Category = "Akses Vaskular & Cairan",
                    DefaultNotes = "Ganti botol cairan / set infus",
                    SortOrder = 9,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ActionCode = "ACT_PASANG_NGT",
                    ActionName = "Pasang NGT",
                    Category = "Nutrisi & Saluran Cerna",
                    DefaultNotes = "Pemasangan selang lambung NGT",
                    SortOrder = 10,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ActionCode = "ACT_MONITOR_INTAKE_OUTPUT",
                    ActionName = "Monitor intake output",
                    Category = "Nutrisi & Saluran Cerna",
                    DefaultNotes = "Pencatatan keseimbangan cairan",
                    SortOrder = 11,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ActionCode = "ACT_MAKAN_NGT",
                    ActionName = "Memberi makan via NGT",
                    Category = "Nutrisi & Saluran Cerna",
                    DefaultNotes = "Nutrisi enteral sonde cair",
                    SortOrder = 12,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ActionCode = "ACT_MINUM_ORAL",
                    ActionName = "Pemberian minum oral",
                    Category = "Nutrisi & Saluran Cerna",
                    DefaultNotes = "Membantu minum per oral bertahap",
                    SortOrder = 13,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ActionCode = "ACT_VITAL_SIGN",
                    ActionName = "Cek tanda vital",
                    Category = "Observasi & Monitoring",
                    DefaultNotes = "Pengukuran TTV rutin per shift",
                    SortOrder = 14,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ActionCode = "ACT_OBS_LUKA_OPERASI",
                    ActionName = "Observasi luka operasi",
                    Category = "Perawatan Luka & Kulit",
                    DefaultNotes = "Observasi tanda infeksi / rembesan",
                    SortOrder = 15,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ActionCode = "ACT_ORAL_HYGIENE",
                    ActionName = "Perawatan mulut",
                    Category = "Higiene & Kenyamanan",
                    DefaultNotes = "Oral hygiene antiseptik",
                    SortOrder = 16,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ActionCode = "ACT_MANDIKAN_PASIEN",
                    ActionName = "Memandikan pasien di tempat tidur",
                    Category = "Higiene & Kenyamanan",
                    DefaultNotes = "Seka air hangat & ganti laken",
                    SortOrder = 17,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ActionCode = "ACT_RAWAT_LUKA_STERIL",
                    ActionName = "Perawatan luka steril / ganti balutan",
                    Category = "Perawatan Luka & Kulit",
                    DefaultNotes = "Ganti balutan steril pasca bedah",
                    SortOrder = 18,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ActionCode = "ACT_MOBILISASI_FISIOTERAPI",
                    ActionName = "Fisioterapi dada / mobilisasi bertahap",
                    Category = "Mobilisasi & Keselamatan",
                    DefaultNotes = "Alih baring / duduk di bed / jalan",
                    SortOrder = 19,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false
                }
            };
        }
    }
}
