using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.Seeders
{
    /// <summary>
    /// Mengisi data master IGD agar modul dapat dipakai. Seeder ini hanya menambah baris yang
    /// belum ada berdasarkan Code, dan tidak pernah menimpa baris yang sudah tersimpan, supaya
    /// nilai yang sudah disesuaikan petugas tidak hilang saat aplikasi dijalankan ulang.
    /// </summary>
    public static class EmergencyMasterDataSeeder
    {
        private const int OutOfQueueScaleLevel = MstEmergencyTriageLevel.OutOfQueueScaleLevel;

        public static async Task<EmergencyMasterDataSeedResult> SeedAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var result = new EmergencyMasterDataSeedResult();
            var now = DateTime.UtcNow;

            await SeedTriageLevelsAsync(db, actorUserId, now, result, ct);
            await SeedTriageIndicatorsAsync(db, actorUserId, now, result, ct);
            await SeedArrivalModesAsync(db, actorUserId, now, result, ct);
            await SeedCaseTypesAsync(db, actorUserId, now, result, ct);
            await SeedDispositionTypesAsync(db, actorUserId, now, result, ct);
            await SeedDefaultSettingAsync(db, actorUserId, now, result, ct);

            return result;
        }


        /// <summary>
        /// Menentukan apakah sebuah master sudah diisi sumber lain.
        /// </summary>
        /// <remarks>
        /// Seeder ini mengisi master yang kosong; ia tidak menggabungkan versinya sendiri ke
        /// dalam master yang sudah dipakai. Bila di tabel ada kode yang tidak dikenal daftar
        /// seeder — misalnya "WALK_IN" sementara seeder mengenal "SELF" — artinya isinya
        /// berasal dari sumber lain. Menambah daftar seeder ke sana menghasilkan dua baris
        /// yang artinya sama dengan kode berbeda, dan laporan yang mengelompokkan menurut
        /// master itu menjadi salah tanpa ada yang menyadarinya.
        ///
        /// Menjalankan seeder dua kali atas datanya sendiri tetap aman: seluruh kode yang ada
        /// dikenal, sehingga tidak ada yang dianggap asing.
        /// </remarks>
        private static bool IsOwnedByAnotherSource(
            IEnumerable<string> existingCodes,
            IEnumerable<string> definitionCodes)
        {
            var known = new HashSet<string>(definitionCodes, StringComparer.OrdinalIgnoreCase);

            return existingCodes.Any(code => !known.Contains(code));
        }

        // ------------------------------------------------------------------
        // Level triase
        // ------------------------------------------------------------------

        /// <remarks>
        /// Target waktu hanya diisi untuk level 1, yaitu 0 menit yang berarti dilayani seketika.
        /// Level 2 sampai 5 sengaja dibiarkan kosong karena SOP triase MMC belum tersedia, dan
        /// menebak angkanya dilarang IGD-DEC-027 serta IGD-DEC-035.
        /// </remarks>
        private static async Task SeedTriageLevelsAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            DateTime now,
            EmergencyMasterDataSeedResult result,
            CancellationToken ct)
        {
            var definitions = new[]
            {
                new TriageLevelDefinition(1, "L1", "Resusitasi", "Merah", "#E53935", 0, true, 10),
                new TriageLevelDefinition(2, "L2", "Emergensi", "Merah", "#E53935", null, true, 20),
                new TriageLevelDefinition(3, "L3", "Urgen", "Kuning", "#FDD835", null, false, 30),
                new TriageLevelDefinition(4, "L4", "Semi-urgen", "Hijau", "#43A047", null, false, 40),
                new TriageLevelDefinition(5, "L5", "Tidak gawat darurat", "Hijau", "#43A047", null, false, 50),

                // Hitam berada di luar skala antrean, karena itu Level-nya 0 dan bukan 1-5.
                // Ia tidak pernah punya target waktu respons: pasien yang meninggal saat tiba
                // tidak sedang menunggu dilayani, sehingga menghitung keterlambatan untuknya
                // tidak punya arti. Penetapannya wajib oleh manusia, tidak pernah oleh aplikasi.
                new TriageLevelDefinition(OutOfQueueScaleLevel, "L0", "Meninggal saat tiba", "Hitam", "#212121", null, false, 60)
            };

            // Idempotensi diperiksa lewat DUA kunci, bukan hanya Code.
            //
            // Tabel ini punya dua index unik: Code, dan pasangan (TriageSystem, Level).
            // Memeriksa Code saja aman hanya bila basis datanya kosong atau diisi seeder ini
            // sendiri. Pada basis data yang sudah diisi sumber lain — misalnya level yang
            // sama tetapi berkode "ATS-L1" — seluruh kode seeder tampak belum ada, sehingga
            // seeder menyisipkan level 1 sampai 5 sekali lagi dan menabrak index
            // (TriageSystem, Level). SaveChanges gagal, dan karena pemanggilnya tidak
            // menangkap exception, aplikasi berhenti sebelum sempat melayani permintaan.
            var existingRows = await db.Set<MstEmergencyTriageLevel>()
                .Select(x => new { x.Code, x.TriageSystem, x.Level })
                .ToListAsync(ct);

            var existing = new HashSet<string>(
                existingRows.Select(x => x.Code),
                StringComparer.OrdinalIgnoreCase);

            var existingSystemLevels = new HashSet<(EmergencyTriageSystem, int)>(
                existingRows.Select(x => (x.TriageSystem, x.Level)));

            foreach (var d in definitions)
            {
                if (existing.Contains(d.Code))
                    continue;

                // Level yang slotnya sudah dipakai sistem triase yang sama dilewati apa
                // adanya. Seeder tidak pernah menimpa, dan juga tidak memaksakan versinya
                // sendiri berdampingan dengan versi yang sudah dipakai petugas.
                if (existingSystemLevels.Contains((EmergencyTriageSystem.ATS, d.Level)))
                {
                    result.TriageLevelSkipped++;
                    continue;
                }

                db.Set<MstEmergencyTriageLevel>().Add(new MstEmergencyTriageLevel
                {
                    Id = Guid.NewGuid(),
                    TriageSystem = EmergencyTriageSystem.ATS,
                    Level = d.Level,
                    Code = d.Code,
                    Name = d.Name,
                    ColorName = d.ColorName,
                    ColorHex = d.ColorHex,
                    MaxWaitingMinutes = d.MaxWaitingMinutes,
                    AllowsTreatmentBeforeRegistration = d.AllowsTreatmentBeforeRegistration,
                    Sequence = d.Sequence,
                    Description = d.Level == OutOfQueueScaleLevel
                        ? "Kategori di luar skala antrean. Tidak memiliki target waktu respons dan tidak boleh ditetapkan otomatis oleh aplikasi."
                        : d.MaxWaitingMinutes.HasValue
                            ? null
                            : "Target waktu respons belum ditetapkan SOP triase MMC.",
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                result.TriageLevelInserted++;
            }

            await db.SaveChangesAsync(ct);
        }

        // ------------------------------------------------------------------
        // Indikator triase
        // ------------------------------------------------------------------

        /// <remarks>
        /// Indikator diisi mengikuti kerangka ABCDE secara umum. Daftar final per level
        /// menunggu SOP triase MMC, sehingga keterangannya menyebutkan hal itu apa adanya.
        /// </remarks>
        private static async Task SeedTriageIndicatorsAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            DateTime now,
            EmergencyMasterDataSeedResult result,
            CancellationToken ct)
        {
            var groups = new[]
            {
                new IndicatorGroupDefinition("A", "Airway", "Penilaian jalan napas", 10),
                new IndicatorGroupDefinition("B", "Breathing", "Penilaian pernapasan", 20),
                new IndicatorGroupDefinition("C", "Circulation", "Penilaian sirkulasi", 30),
                new IndicatorGroupDefinition("D", "Disability", "Penilaian kesadaran dan neurologis", 40),
                new IndicatorGroupDefinition("E", "Exposure", "Penilaian paparan dan pemeriksaan menyeluruh", 50)
            };

            var levels = await db.Set<MstEmergencyTriageLevel>()
                .Where(x => !x.IsDelete)
                .Select(x => new { x.Id, x.Code })
                .ToListAsync(ct);

            if (levels.Count == 0)
            {
                result.TriageIndicatorSkippedReason =
                    "Tidak ada level triase yang dapat dijadikan induk indikator.";
                return;
            }

            var existingCodes = await db.Set<MstEmergencyTriageIndicator>()
                .Where(x => !x.IsDelete)
                .Select(x => x.Code)
                .ToListAsync(ct);

            // Kode indikator dibentuk dari kode level, sehingga daftar kandidatnya harus
            // disusun lebih dulu sebelum dapat dibandingkan dengan isi tabel.
            var candidateCodes = levels
                .SelectMany(level => groups.Select(g => $"TRI-{level.Code}-{g.Key}"))
                .ToList();

            if (IsOwnedByAnotherSource(existingCodes, candidateCodes))
            {
                result.TriageIndicatorSkippedReason =
                    "Master indikator triase sudah diisi sumber lain; seeder tidak menambah " +
                    "apa pun supaya checklist perawat tidak berisi dua set indikator sekaligus.";
                return;
            }

            var existing = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

            foreach (var level in levels)
            {
                foreach (var g in groups)
                {
                    var code = $"TRI-{level.Code}-{g.Key}";

                    if (existing.Contains(code))
                        continue;

                    db.Set<MstEmergencyTriageIndicator>().Add(new MstEmergencyTriageIndicator
                    {
                        Id = Guid.NewGuid(),
                        TriageLevelId = level.Id,
                        Code = code,
                        Name = $"{g.Name} — {g.Description}",
                        IndicatorGroup = g.Name,
                        Sequence = g.Sequence,
                        Description = "Daftar indikator final untuk level ini menunggu SOP triase MMC.",
                        IsActive = true,
                        CreateDateTime = now,
                        CreateBy = actorUserId
                    });

                    result.TriageIndicatorInserted++;
                }
            }

            await db.SaveChangesAsync(ct);
        }

        // ------------------------------------------------------------------
        // Cara kedatangan
        // ------------------------------------------------------------------

        private static async Task SeedArrivalModesAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            DateTime now,
            EmergencyMasterDataSeedResult result,
            CancellationToken ct)
        {
            var definitions = new[]
            {
                new ArrivalModeDefinition("SELF", "Datang sendiri", false, false, 10),
                new ArrivalModeDefinition("FAMILY", "Diantar keluarga", false, false, 20),
                new ArrivalModeDefinition("AMBULANCE", "Ambulans", true, false, 30),
                new ArrivalModeDefinition("POLICE", "Diantar polisi", false, false, 40),
                new ArrivalModeDefinition("REFERRAL", "Rujukan fasilitas kesehatan lain", false, true, 50)
            };

            var existingCodes = await db.Set<MstEmergencyArrivalMode>()
                .Where(x => !x.IsDelete)
                .Select(x => x.Code)
                .ToListAsync(ct);

            if (IsOwnedByAnotherSource(existingCodes, definitions.Select(d => d.Code)))
            {
                result.ArrivalModeSkippedReason =
                    "Master cara kedatangan sudah diisi sumber lain; seeder tidak menambah apa pun " +
                    "supaya tidak ada dua baris yang artinya sama dengan kode berbeda.";
                return;
            }

            var existing = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

            foreach (var d in definitions)
            {
                if (existing.Contains(d.Code))
                    continue;

                db.Set<MstEmergencyArrivalMode>().Add(new MstEmergencyArrivalMode
                {
                    Id = Guid.NewGuid(),
                    Code = d.Code,
                    Name = d.Name,
                    IsAmbulance = d.IsAmbulance,
                    IsReferral = d.IsReferral,
                    Sequence = d.Sequence,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                result.ArrivalModeInserted++;
            }

            await db.SaveChangesAsync(ct);
        }

        // ------------------------------------------------------------------
        // Jenis kasus
        // ------------------------------------------------------------------

        private static async Task SeedCaseTypesAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            DateTime now,
            EmergencyMasterDataSeedResult result,
            CancellationToken ct)
        {
            var definitions = new[]
            {
                new CaseTypeDefinition("TRAUMA", "Trauma", 10),
                new CaseTypeDefinition("NON_TRAUMA", "Non-trauma", 20),
                new CaseTypeDefinition("KLL", "Kecelakaan lalu lintas", 30),
                new CaseTypeDefinition("KERJA", "Kecelakaan kerja", 40),
                new CaseTypeDefinition("KRIMINAL", "Kriminalitas", 50),
                new CaseTypeDefinition("OBSTETRI", "Obstetri", 60),
                new CaseTypeDefinition("RACUN", "Keracunan", 70),
                new CaseTypeDefinition("BENCANA", "Bencana", 80)
            };

            var existingCodes = await db.Set<MstEmergencyCaseType>()
                .Where(x => !x.IsDelete)
                .Select(x => x.Code)
                .ToListAsync(ct);

            if (IsOwnedByAnotherSource(existingCodes, definitions.Select(d => d.Code)))
            {
                result.CaseTypeSkippedReason =
                    "Master jenis kasus sudah diisi sumber lain; seeder tidak menambah apa pun " +
                    "supaya tidak ada dua baris yang artinya sama dengan kode berbeda.";
                return;
            }

            var existing = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

            foreach (var d in definitions)
            {
                if (existing.Contains(d.Code))
                    continue;

                db.Set<MstEmergencyCaseType>().Add(new MstEmergencyCaseType
                {
                    Id = Guid.NewGuid(),
                    Code = d.Code,
                    Name = d.Name,
                    Sequence = d.Sequence,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                result.CaseTypeInserted++;
            }

            await db.SaveChangesAsync(ct);
        }

        // ------------------------------------------------------------------
        // Jenis tindak lanjut
        // ------------------------------------------------------------------

        private static async Task SeedDispositionTypesAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            DateTime now,
            EmergencyMasterDataSeedResult result,
            CancellationToken ct)
        {
            var definitions = new[]
            {
                new DispositionTypeDefinition("PULANG", "Pulang", false, false, 10),
                new DispositionTypeDefinition("RANAP", "Rawat inap", true, false, 20),
                new DispositionTypeDefinition("INTENSIF", "Pindah ICU atau kamar operasi", true, false, 30),
                new DispositionTypeDefinition("RUJUK", "Rujuk ke fasilitas kesehatan lain", false, true, 40),
                new DispositionTypeDefinition("MENINGGAL", "Meninggal", false, false, 50),
                new DispositionTypeDefinition("TOLAK", "Menolak perawatan", false, false, 60),
                new DispositionTypeDefinition("APS", "Pulang atas permintaan sendiri", false, false, 70)
            };

            var existingCodes = await db.Set<MstEmergencyDispositionType>()
                .Where(x => !x.IsDelete)
                .Select(x => x.Code)
                .ToListAsync(ct);

            if (IsOwnedByAnotherSource(existingCodes, definitions.Select(d => d.Code)))
            {
                result.DispositionTypeSkippedReason =
                    "Master jenis tindak lanjut sudah diisi sumber lain; seeder tidak menambah apa pun " +
                    "supaya tidak ada dua baris yang artinya sama dengan kode berbeda.";
                return;
            }

            var existing = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

            foreach (var d in definitions)
            {
                if (existing.Contains(d.Code))
                    continue;

                db.Set<MstEmergencyDispositionType>().Add(new MstEmergencyDispositionType
                {
                    Id = Guid.NewGuid(),
                    Code = d.Code,
                    Name = d.Name,
                    RequiresDestinationServiceUnit = d.RequiresDestinationServiceUnit,
                    RequiresReferralFacility = d.RequiresReferralFacility,
                    ClosesEmergencyVisit = true,
                    Sequence = d.Sequence,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                result.DispositionTypeInserted++;
            }

            await db.SaveChangesAsync(ct);
        }

        // ------------------------------------------------------------------
        // Pengaturan IGD
        // ------------------------------------------------------------------

        /// <remarks>
        /// Pengaturan wajib menunjuk satu unit pelayanan IGD yang sudah terdaftar. Unit itu
        /// tidak boleh dibuat sendiri oleh seeder ini karena unit pelayanan dimiliki modul
        /// Master Data, bukan modul IGD. Bila belum ada unit bertipe Emergency, seeder berhenti
        /// pada bagian ini dan menyebutkan alasannya.
        /// </remarks>
        private static async Task SeedDefaultSettingAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            DateTime now,
            EmergencyMasterDataSeedResult result,
            CancellationToken ct)
        {
            var alreadyExists = await db.Set<MstEmergencySetting>()
                .AnyAsync(x => !x.IsDelete, ct);

            if (alreadyExists)
            {
                result.SettingSkippedReason = "Pengaturan IGD sudah ada, tidak ditambah lagi.";
                return;
            }

            var emergencyUnitId = await db.Set<MstServiceUnit>()
                .Where(x => !x.IsDelete &&
                            x.IsActive &&
                            x.ServiceUnitType == ServiceUnitType.Emergency)
                .OrderBy(x => x.SortOrder)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(ct);

            if (emergencyUnitId == null)
            {
                result.SettingSkippedReason =
                    "Belum ada unit pelayanan bertipe Emergency yang aktif. " +
                    "Daftarkan unit IGD lebih dulu pada master unit pelayanan, lalu jalankan seeder kembali.";
                return;
            }

            db.Set<MstEmergencySetting>().Add(new MstEmergencySetting
            {
                Id = Guid.NewGuid(),
                Code = "DEFAULT",
                Name = "Pengaturan IGD Default",
                DefaultEmergencyServiceUnitId = emergencyUnitId.Value,
                TriageSystem = EmergencyTriageSystem.ATS,
                IsDefault = true,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            });

            await db.SaveChangesAsync(ct);
            result.SettingInserted = 1;
        }

        // ------------------------------------------------------------------
        // Bentuk data internal
        // ------------------------------------------------------------------

        private sealed record TriageLevelDefinition(
            int Level,
            string Code,
            string Name,
            string ColorName,
            string ColorHex,
            int? MaxWaitingMinutes,
            bool AllowsTreatmentBeforeRegistration,
            int Sequence);

        private sealed record IndicatorGroupDefinition(
            string Key,
            string Name,
            string Description,
            int Sequence);

        private sealed record ArrivalModeDefinition(
            string Code,
            string Name,
            bool IsAmbulance,
            bool IsReferral,
            int Sequence);

        private sealed record CaseTypeDefinition(
            string Code,
            string Name,
            int Sequence);

        private sealed record DispositionTypeDefinition(
            string Code,
            string Name,
            bool RequiresDestinationServiceUnit,
            bool RequiresReferralFacility,
            int Sequence);
    }

    /// <summary>
    /// Ringkasan hasil seeder, dipakai untuk pencatatan log agar terlihat berapa baris yang
    /// benar-benar ditambahkan dan bagian mana yang dilewati beserta alasannya.
    /// </summary>
    public class EmergencyMasterDataSeedResult
    {
        public int TriageLevelInserted { get; set; }
        public int TriageIndicatorInserted { get; set; }
        public int ArrivalModeInserted { get; set; }
        public int CaseTypeInserted { get; set; }
        public int DispositionTypeInserted { get; set; }
        public int SettingInserted { get; set; }
        public int TriageLevelSkipped { get; set; }
        public string? TriageIndicatorSkippedReason { get; set; }
        public string? ArrivalModeSkippedReason { get; set; }
        public string? CaseTypeSkippedReason { get; set; }
        public string? DispositionTypeSkippedReason { get; set; }
        public string? SettingSkippedReason { get; set; }

        public int TotalInserted =>
            TriageLevelInserted +
            TriageIndicatorInserted +
            ArrivalModeInserted +
            CaseTypeInserted +
            DispositionTypeInserted +
            SettingInserted;
    }
}
