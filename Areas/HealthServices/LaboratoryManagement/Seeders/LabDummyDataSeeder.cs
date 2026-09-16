using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Seeders
{
    /// <summary>
    /// Mengisi data induk contoh yang dibutuhkan modul Laboratorium agar seluruh alurnya dapat
    /// dijalankan di lingkungan pengembangan tanpa mengisi delapan layar master satu per satu.
    ///
    /// <b>Mengapa seeder ini ada.</b> Laboratorium tidak memiliki satu pun data induknya
    /// sendiri selain katalog alasan penolakan. Katalog pemeriksaan, tarif, kelompok umur, dan
    /// sumber rujukan semuanya milik Master Data. Akibatnya database yang baru dibuat membuat
    /// modul ini tampak rusak: katalog kosong, pesanan tidak dapat dibuat, dan batas nilai tidak
    /// punya jenis pemeriksaan untuk ditunjuk. Seeder ini menutup jarak itu.
    ///
    /// <b>Data di sini contoh, bukan katalog rumah sakit.</b> Kode, nama, harga, dan batas nilai
    /// di bawah disusun agar masuk akal secara klinis dan memenuhi VAL-22 sampai VAL-29, tetapi
    /// tidak satu pun disahkan pemilik klinis atau pemilik tarif. Karena itu seeder ini menolak
    /// berjalan di produksi, mengikuti batas yang sama pada
    /// <c>InpatientMasterDataSeeder</c>.
    /// </summary>
    /// <remarks>
    /// Tiga batas yang mengikat seeder ini:
    ///
    /// 1. MENOLAK berjalan di lingkungan produksi. Katalog dan tarif produksi ditetapkan pemilik
    ///    proses bisnis lewat layar admin, bukan oleh baris kode yang ikut terbawa setiap kali
    ///    aplikasi dinyalakan.
    /// 2. HANYA MENAMBAH baris yang belum ada, dan tidak pernah menimpa baris yang sudah
    ///    tersimpan. Baris yang sudah ditandai terhapus juga dihormati: kodenya tidak diisi
    ///    ulang, karena penghapusannya adalah keputusan pengguna.
    /// 3. TIDAK MENYENTUH tabel transaksi. Tidak ada pasien, kunjungan, pesanan, sampel, maupun
    ///    pemeriksaan yang dibuat di sini. Ketiganya lahir dari alur yang sedang diuji, dan
    ///    membuatnya lewat seeder akan melewati aturan yang justru ingin dibuktikan.
    ///
    /// Katalog alasan penolakan sampel sengaja tidak diulang di sini; sepuluh baris baselinenya
    /// sudah menjadi tanggung jawab <see cref="LabRejectionReasonSeeder"/>.
    ///
    /// Seluruh Id ditetapkan tetap — bukan dibangkitkan — supaya satu baris contoh memiliki
    /// identitas yang sama di setiap lingkungan dan dapat dirujuk dengan pasti. Awalan
    /// <c>dd0000nn</c> menandai baris buatan seeder ini sehingga mudah dicari dan dibersihkan.
    /// </remarks>
    public static class LabDummyDataSeeder
    {
        /// <summary>Nama lingkungan yang membuat seeder berhenti tanpa menulis apa pun.</summary>
        public const string ProductionEnvironmentName = "Production";

        /// <summary>Jumlah baris contoh per data induk, sesuai permintaan penyiapan lingkungan uji.</summary>
        public const int RowsPerMaster = 10;

        // Awalan Id per tabel. Dua digit terakhir adalah nomor urut baris.
        private const string AgeCategoryIdPrefix = "dd000001-0000-4000-8000-0000000000";
        private const string TariffCategoryIdPrefix = "dd000002-0000-4000-8000-0000000000";
        private const string ProcedureIdPrefix = "dd000003-0000-4000-8000-0000000000";
        private const string TariffIdPrefix = "dd000004-0000-4000-8000-0000000000";
        private const string ReferralInstitutionIdPrefix = "dd000005-0000-4000-8000-0000000000";
        private const string ReferralDoctorIdPrefix = "dd000006-0000-4000-8000-0000000000";
        private const string ValueBoundIdPrefix = "dd000007-0000-4000-8000-0000000000";
        private const string ValueOptionIdPrefix = "dd000008-0000-4000-8000-0000000000";

        // =================================================================
        // Titik masuk
        // =================================================================

        public static async Task<LabDummySeedResult> SeedAsync(
            IServiceProvider serviceProvider,
            string environmentName,
            CancellationToken cancellationToken = default)
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(LabDummyDataSeeder));

            // Pemilik baris contoh dicatat sebagai SuperAdmin bila ada, supaya jejak audit
            // menunjuk akun yang nyata dan bukan Guid kosong. Ketiadaannya tidak menghentikan
            // seeder: data induk contoh tidak bergantung pada identitas pembuatnya.
            var actorUserId = await dbContext.Users
                .AsNoTracking()
                .Where(x =>
                    x.NormalizedUserName == "SUPERADMIN" ||
                    x.NormalizedEmail == "SUPERADMIN@ADMIN.COM")
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var result = await SeedAsync(dbContext, actorUserId, environmentName, cancellationToken);

            if (result.RefusedReason != null)
            {
                logger.LogWarning("Seeder data contoh Laboratorium tidak dijalankan. {Reason}", result.RefusedReason);
                return result;
            }

            logger.LogInformation(
                "Seeder data contoh Laboratorium selesai. Kelompok umur {AgeCategory}, kategori tarif {TariffCategory}, "
                + "jenis pemeriksaan {Procedure}, tarif {Tariff}, instansi perujuk {Institution}, dokter perujuk {Doctor}, "
                + "batas nilai {ValueBound}, pilihan hasil {ValueOption}.",
                result.AgeCategoryInserted,
                result.TariffCategoryInserted,
                result.ProcedureInserted,
                result.TariffInserted,
                result.ReferralInstitutionInserted,
                result.ReferralDoctorInserted,
                result.ValueBoundInserted,
                result.ValueOptionInserted);

            return result;
        }

        /// <summary>
        /// Menjalankan pengisian terhadap satu <see cref="ApplicationDbContext"/> yang sudah ada.
        /// Dipisahkan agar perilakunya dapat diuji tanpa membangun service provider.
        /// </summary>
        public static async Task<LabDummySeedResult> SeedAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            string environmentName,
            CancellationToken ct = default)
        {
            var result = new LabDummySeedResult();

            if (IsProductionEnvironment(environmentName))
            {
                result.RefusedReason =
                    "Seeder data contoh Laboratorium menolak berjalan di lingkungan produksi. "
                    + "Isi katalog pemeriksaan, tarif, kelompok umur, dan sumber rujukan produksi "
                    + "lewat layar admin, bukan lewat seeder.";

                return result;
            }

            var now = DateTime.UtcNow;

            // Urutan menuruti arah ketergantungan: tarif menunjuk kategori dan jenis
            // pemeriksaan, dokter perujuk menunjuk instansinya, batas nilai menunjuk jenis
            // pemeriksaan.
            await SeedAgeCategoriesAsync(db, actorUserId, now, result, ct);
            await SeedTariffCategoriesAsync(db, actorUserId, now, result, ct);
            await SeedProceduresAsync(db, actorUserId, now, result, ct);
            await SeedTariffsAsync(db, actorUserId, now, result, ct);
            await SeedReferralInstitutionsAsync(db, actorUserId, now, result, ct);
            await SeedReferralDoctorsAsync(db, actorUserId, now, result, ct);
            await SeedValueBoundsAsync(db, actorUserId, now, result, ct);

            return result;
        }

        /// <summary>
        /// Menentukan apakah nama lingkungan yang diberikan adalah produksi. Pembandingannya
        /// mengabaikan besar kecil huruf, mengikuti cara ASP.NET Core sendiri membaca
        /// ASPNETCORE_ENVIRONMENT.
        /// </summary>
        public static bool IsProductionEnvironment(string? environmentName)
            => string.Equals(
                environmentName?.Trim(),
                ProductionEnvironmentName,
                StringComparison.OrdinalIgnoreCase);

        // =================================================================
        // Kelompok umur — dipakai batas nilai rujukan (BR-14)
        // =================================================================

        /// <remarks>
        /// Batas hari mengikuti pengelompokan umum Kementerian Kesehatan. Tidak satu pun ditandai
        /// <c>IsDefault</c>: database yang sudah berjalan mungkin sudah punya kelompok bawaannya
        /// sendiri, dan seeder contoh tidak berhak memindahkannya.
        /// </remarks>
        private static readonly AgeCategorySeed[] AgeCategories =
        {
            new("01", "AGE-NEONATUS", "Neonatus", "Neonatus", 0, 28, 10),
            new("02", "AGE-BAYI", "Bayi", "Bayi", 29, 364, 20),
            new("03", "AGE-BALITA", "Balita", "Balita", 365, 1824, 30),
            new("04", "AGE-ANAK", "Anak", "Anak", 1825, 4014, 40),
            new("05", "AGE-REMAJA-AWAL", "Remaja Awal", "Rmj Awal", 4015, 6209, 50),
            new("06", "AGE-REMAJA-AKHIR", "Remaja Akhir", "Rmj Akhir", 6210, 9124, 60),
            new("07", "AGE-DEWASA-AWAL", "Dewasa Awal", "Dws Awal", 9125, 13514, 70),
            new("08", "AGE-DEWASA-AKHIR", "Dewasa Akhir", "Dws Akhir", 13515, 16789, 80),
            new("09", "AGE-LANSIA-AWAL", "Lansia Awal", "Lns Awal", 16790, 23359, 90),
            new("10", "AGE-LANSIA-AKHIR", "Lansia Akhir", "Lns Akhir", 23360, null, 100)
        };

        private static async Task SeedAgeCategoriesAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            DateTime now,
            LabDummySeedResult result,
            CancellationToken ct)
        {
            // Baris yang sudah ditandai terhapus ikut dihitung. Index unik AgeCategoryCode tidak
            // menyaring IsDelete, sehingga menambah baris kedua berkode sama akan menabraknya
            // dan menghentikan aplikasi saat menyala.
            var existingCodes = await db.Set<MstAgeCategory>()
                .AsNoTracking()
                .Select(x => x.AgeCategoryCode)
                .ToListAsync(ct);

            var existing = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

            foreach (var seed in AgeCategories)
            {
                if (existing.Contains(seed.Code))
                {
                    result.AgeCategorySkipped++;
                    continue;
                }

                db.Set<MstAgeCategory>().Add(new MstAgeCategory
                {
                    Id = Guid.Parse(AgeCategoryIdPrefix + seed.Suffix),
                    AgeCategoryCode = seed.Code,
                    AgeCategoryName = seed.Name,
                    AgeCategoryShortName = seed.ShortName,
                    MinAgeDays = seed.MinAgeDays,
                    MaxAgeDays = seed.MaxAgeDays,
                    IsDefault = false,
                    IsSelectableInKiosk = true,
                    IsSelectableInRegistration = true,
                    IsUsedForClinicalRule = true,
                    SortOrder = seed.SortOrder,
                    Description = "Data contoh Laboratorium. Bukan pengelompokan umur yang disahkan pemilik klinis.",
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                result.AgeCategoryInserted++;
            }

            if (result.AgeCategoryInserted > 0)
                await db.SaveChangesAsync(ct);
        }

        // =================================================================
        // Kategori tarif — pengelompokan tarif pemeriksaan laboratorium
        // =================================================================

        private static readonly TariffCategorySeed[] TariffCategories =
        {
            new("01", "LAB-HEMA", "Laboratorium Hematologi", 10),
            new("02", "LAB-KIMIA", "Laboratorium Kimia Klinik", 20),
            new("03", "LAB-ELEKTROLIT", "Laboratorium Elektrolit", 30),
            new("04", "LAB-URINALISIS", "Laboratorium Urinalisis", 40),
            new("05", "LAB-IMUNOSEROLOGI", "Laboratorium Imunoserologi", 50),
            new("06", "LAB-MIKROBIOLOGI", "Laboratorium Mikrobiologi", 60),
            new("07", "LAB-PATOLOGI-ANATOMI", "Laboratorium Patologi Anatomi", 70),
            new("08", "LAB-HEMOSTASIS", "Laboratorium Hemostasis", 80),
            new("09", "LAB-TRANSFUSI", "Laboratorium Pratransfusi", 90),
            new("10", "LAB-MOLEKULER", "Laboratorium Molekuler", 100)
        };

        private static async Task SeedTariffCategoriesAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            DateTime now,
            LabDummySeedResult result,
            CancellationToken ct)
        {
            var existingCodes = await db.Set<MstTariffCategory>()
                .AsNoTracking()
                .Select(x => x.TariffCategoryCode)
                .ToListAsync(ct);

            var existing = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

            foreach (var seed in TariffCategories)
            {
                if (existing.Contains(seed.Code))
                {
                    result.TariffCategorySkipped++;
                    continue;
                }

                db.Set<MstTariffCategory>().Add(new MstTariffCategory
                {
                    Id = Guid.Parse(TariffCategoryIdPrefix + seed.Suffix),
                    TariffCategoryCode = seed.Code,
                    TariffCategoryName = seed.Name,
                    TariffGroupName = "Laboratorium",
                    IsProcedure = false,
                    IsLaboratory = true,
                    IsCoveredByInsuranceDefault = true,
                    SortOrder = seed.SortOrder,
                    Description = "Data contoh Laboratorium. Bukan struktur tarif yang disahkan pemilik tarif.",
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                result.TariffCategoryInserted++;
            }

            if (result.TariffCategoryInserted > 0)
                await db.SaveChangesAsync(ct);
        }

        // =================================================================
        // Jenis pemeriksaan laboratorium
        // =================================================================

        /// <remarks>
        /// Ketiga disiplin pada <see cref="LabDiscipline"/> sengaja terwakili, supaya penyaring
        /// disiplin pada katalog (AC-43) dan larangan pindah disiplin (INV-22) benar-benar dapat
        /// diuji dan bukan hanya dibaca.
        ///
        /// Nomor urut baris di sini adalah kunci yang mengikat tarif dan batas nilai: keduanya
        /// menunjuk jenis pemeriksaan lewat akhiran Id yang sama.
        /// </remarks>
        private static readonly ProcedureSeed[] Procedures =
        {
            new("01", "LAB-HB", "Hemoglobin", "Hematologi", LabDiscipline.ClinicalPathology, 10),
            new("02", "LAB-LEU", "Leukosit", "Hematologi", LabDiscipline.ClinicalPathology, 20),
            new("03", "LAB-TRO", "Trombosit", "Hematologi", LabDiscipline.ClinicalPathology, 30),
            new("04", "LAB-GDS", "Glukosa Darah Sewaktu", "Kimia Klinik", LabDiscipline.ClinicalPathology, 40),
            new("05", "LAB-UREUM", "Ureum", "Kimia Klinik", LabDiscipline.ClinicalPathology, 50),
            new("06", "LAB-KREATININ", "Kreatinin", "Kimia Klinik", LabDiscipline.ClinicalPathology, 60),
            new("07", "LAB-KALIUM", "Kalium", "Elektrolit", LabDiscipline.ClinicalPathology, 70),
            new("08", "LAB-URIN-PROTEIN", "Urinalisis Protein", "Urinalisis", LabDiscipline.ClinicalPathology, 80),
            new("09", "LAB-BTA", "Pewarnaan BTA Sputum", "Mikrobiologi", LabDiscipline.Microbiology, 90),
            new("10", "LAB-SITOLOGI-FNAB", "Sitologi FNAB", "Patologi Anatomi", LabDiscipline.AnatomicalPathology, 100)
        };

        private static async Task SeedProceduresAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            DateTime now,
            LabDummySeedResult result,
            CancellationToken ct)
        {
            var existingCodes = await db.Set<MstProcedure>()
                .AsNoTracking()
                .Select(x => x.ProcedureCode)
                .ToListAsync(ct);

            var existing = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

            foreach (var seed in Procedures)
            {
                if (existing.Contains(seed.Code))
                {
                    result.ProcedureSkipped++;
                    continue;
                }

                db.Set<MstProcedure>().Add(new MstProcedure
                {
                    Id = Guid.Parse(ProcedureIdPrefix + seed.Suffix),
                    ProcedureCode = seed.Code,
                    ProcedureName = seed.Name,
                    ProcedureGroupName = "Laboratorium",
                    ProcedureCategoryName = seed.CategoryName,
                    ProcedureType = "Laboratory",

                    // Penanda inilah yang membuat baris ini muncul di katalog Laboratorium.
                    // Tanpa IsLaboratory, LabCatalogService tidak akan pernah membacanya.
                    IsLaboratory = true,
                    LabDiscipline = seed.Discipline,

                    IsDoctorAction = false,
                    IsNursingAction = false,
                    IsSurgery = false,
                    IsRadiology = false,
                    IsTherapy = false,

                    // Pemeriksaan laboratorium dikerjakan analis; kehadiran dokter tidak menjadi
                    // syarat pengerjaannya.
                    IsNeedDoctor = false,
                    IsNeedApproval = false,

                    IsCoveredByInsuranceDefault = true,
                    IsAvailableForOutpatient = true,
                    IsAvailableForInpatient = true,
                    IsAvailableForEmergency = true,
                    EstimatedDurationMinutes = 30,
                    SortOrder = seed.SortOrder,
                    Description = "Data contoh Laboratorium. Bukan katalog pemeriksaan yang disahkan pemilik klinis.",
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                result.ProcedureInserted++;
            }

            if (result.ProcedureInserted > 0)
                await db.SaveChangesAsync(ct);
        }

        // =================================================================
        // Tarif — satu tarif berlaku per jenis pemeriksaan
        // =================================================================

        /// <remarks>
        /// Satu tarif per jenis pemeriksaan sudah cukup, dan justru itu yang benar untuk data
        /// contoh: <c>LabCatalogService</c> memilih tarif berlaku pada satu titik waktu, sehingga
        /// dua tarif dengan masa berlaku bertumpang tindih akan membuat harga yang muncul di
        /// katalog bergantung pada urutan baris, bukan pada aturan.
        ///
        /// <c>EffectiveStartDate</c> dimundurkan satu tahun supaya tarif sudah berlaku pada saat
        /// seeder dijalankan, dan <c>EffectiveEndDate</c> dikosongkan supaya data contoh tidak
        /// diam-diam kedaluwarsa di tengah pengujian.
        ///
        /// Harga di bawah adalah angka contoh yang wajar untuk laboratorium rumah sakit tipe C,
        /// bukan tarif yang disahkan.
        /// </remarks>
        private static readonly TariffSeed[] Tariffs =
        {
            new("01", "TRF-LAB-HB", "Tarif Hemoglobin", "01", "01", 35_000m, 10),
            new("02", "TRF-LAB-LEU", "Tarif Leukosit", "01", "02", 35_000m, 20),
            new("03", "TRF-LAB-TRO", "Tarif Trombosit", "01", "03", 40_000m, 30),
            new("04", "TRF-LAB-GDS", "Tarif Glukosa Darah Sewaktu", "02", "04", 30_000m, 40),
            new("05", "TRF-LAB-UREUM", "Tarif Ureum", "02", "05", 45_000m, 50),
            new("06", "TRF-LAB-KREATININ", "Tarif Kreatinin", "02", "06", 45_000m, 60),
            new("07", "TRF-LAB-KALIUM", "Tarif Kalium", "03", "07", 60_000m, 70),
            new("08", "TRF-LAB-URIN-PROTEIN", "Tarif Urinalisis Protein", "04", "08", 25_000m, 80),
            new("09", "TRF-LAB-BTA", "Tarif Pewarnaan BTA Sputum", "06", "09", 55_000m, 90),
            new("10", "TRF-LAB-SITOLOGI-FNAB", "Tarif Sitologi FNAB", "07", "10", 350_000m, 100)
        };

        private static async Task SeedTariffsAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            DateTime now,
            LabDummySeedResult result,
            CancellationToken ct)
        {
            var existingCodes = await db.Set<MstTariff>()
                .AsNoTracking()
                .Select(x => x.TariffCode)
                .ToListAsync(ct);

            var existing = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

            foreach (var seed in Tariffs)
            {
                if (existing.Contains(seed.Code))
                {
                    result.TariffSkipped++;
                    continue;
                }

                db.Set<MstTariff>().Add(new MstTariff
                {
                    Id = Guid.Parse(TariffIdPrefix + seed.Suffix),
                    TariffCode = seed.Code,
                    TariffName = seed.Name,
                    TariffCategoryId = Guid.Parse(TariffCategoryIdPrefix + seed.TariffCategorySuffix),
                    ProcedureId = Guid.Parse(ProcedureIdPrefix + seed.ProcedureSuffix),
                    NormalPrice = seed.NormalPrice,
                    EffectiveStartDate = now.Date.AddYears(-1),
                    EffectiveEndDate = null,
                    IsNeedDoctor = false,
                    IsNeedApproval = false,
                    IsTaxable = false,
                    SortOrder = seed.SortOrder,
                    Description = "Data contoh Laboratorium. Bukan tarif yang disahkan pemilik tarif.",
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                result.TariffInserted++;
            }

            if (result.TariffInserted > 0)
                await db.SaveChangesAsync(ct);
        }

        // =================================================================
        // Instansi perujuk
        // =================================================================

        private static readonly ReferralInstitutionSeed[] ReferralInstitutions =
        {
            new("01", "RUJ-PKM-KLATEN", "Puskesmas Klaten Tengah", "Jl. Pemuda No. 12, Klaten", "0272-321001"),
            new("02", "RUJ-PKM-CEPER", "Puskesmas Ceper", "Jl. Raya Ceper No. 5, Klaten", "0272-321002"),
            new("03", "RUJ-PKM-DELANGGU", "Puskesmas Delanggu", "Jl. Solo-Yogya Km 18, Klaten", "0272-321003"),
            new("04", "RUJ-KLINIK-SEHAT", "Klinik Pratama Sehat Sentosa", "Jl. Diponegoro No. 44, Klaten", "0272-321004"),
            new("05", "RUJ-KLINIK-AMANAH", "Klinik Pratama Amanah Medika", "Jl. Veteran No. 8, Klaten", "0272-321005"),
            new("06", "RUJ-KLINIK-BUNDA", "Klinik Utama Bunda Ceria", "Jl. Merbabu No. 21, Klaten", "0272-321006"),
            new("07", "RUJ-RS-PERMATA", "RS Permata Husada", "Jl. Ahmad Yani No. 100, Klaten", "0272-321007"),
            new("08", "RUJ-RS-MITRA", "RS Mitra Sejahtera", "Jl. Sulawesi No. 3, Klaten", "0272-321008"),
            new("09", "RUJ-DPM-WIDODO", "Dokter Praktik Mandiri Widodo", "Jl. Kartini No. 17, Klaten", "0272-321009"),
            new("10", "RUJ-LAB-PRAMITA", "Laboratorium Rujukan Pramita Klaten", "Jl. Pandanaran No. 9, Klaten", "0272-321010")
        };

        private static async Task SeedReferralInstitutionsAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            DateTime now,
            LabDummySeedResult result,
            CancellationToken ct)
        {
            var existingCodes = await db.Set<MstReferralInstitution>()
                .AsNoTracking()
                .Select(x => x.InstitutionCode)
                .ToListAsync(ct);

            var existing = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

            foreach (var seed in ReferralInstitutions)
            {
                if (existing.Contains(seed.Code))
                {
                    result.ReferralInstitutionSkipped++;
                    continue;
                }

                db.Set<MstReferralInstitution>().Add(new MstReferralInstitution
                {
                    Id = Guid.Parse(ReferralInstitutionIdPrefix + seed.Suffix),
                    InstitutionCode = seed.Code,
                    InstitutionName = seed.Name,
                    Address = seed.Address,
                    PhoneNumber = seed.PhoneNumber,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                result.ReferralInstitutionInserted++;
            }

            if (result.ReferralInstitutionInserted > 0)
                await db.SaveChangesAsync(ct);
        }

        // =================================================================
        // Dokter perujuk
        // =================================================================

        /// <remarks>
        /// Satu dokter per instansi. Tabel ini tidak punya kode unik, sehingga idempotensinya
        /// diperiksa lewat pasangan instansi dan nama — pasangan itulah yang membuat seorang
        /// dokter perujuk dapat dikenali kembali.
        /// </remarks>
        private static readonly ReferralDoctorSeed[] ReferralDoctors =
        {
            new("01", "01", "dr. Ratna Kusumawardani"),
            new("02", "02", "dr. Bambang Setiawan"),
            new("03", "03", "dr. Siti Nurhaliza Putri"),
            new("04", "04", "dr. Andi Prasetyo"),
            new("05", "05", "dr. Maria Ulfa Rahmawati"),
            new("06", "06", "dr. Hendra Gunawan, Sp.OG"),
            new("07", "07", "dr. Lestari Wulandari, Sp.PD"),
            new("08", "08", "dr. Yusuf Maulana, Sp.A"),
            new("09", "09", "dr. Widodo Santoso"),
            new("10", "10", "dr. Farah Amelia, Sp.PK")
        };

        private static async Task SeedReferralDoctorsAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            DateTime now,
            LabDummySeedResult result,
            CancellationToken ct)
        {
            var institutionIds = ReferralDoctors
                .Select(x => Guid.Parse(ReferralInstitutionIdPrefix + x.InstitutionSuffix))
                .Distinct()
                .ToList();

            var existingPairs = await db.Set<MstReferralDoctor>()
                .AsNoTracking()
                .Where(x => institutionIds.Contains(x.ReferralInstitutionId))
                .Select(x => new { x.ReferralInstitutionId, x.DoctorName })
                .ToListAsync(ct);

            var existing = new HashSet<string>(
                existingPairs.Select(x => x.ReferralInstitutionId + "|" + x.DoctorName),
                StringComparer.OrdinalIgnoreCase);

            foreach (var seed in ReferralDoctors)
            {
                var institutionId = Guid.Parse(ReferralInstitutionIdPrefix + seed.InstitutionSuffix);

                if (existing.Contains(institutionId + "|" + seed.DoctorName))
                {
                    result.ReferralDoctorSkipped++;
                    continue;
                }

                db.Set<MstReferralDoctor>().Add(new MstReferralDoctor
                {
                    Id = Guid.Parse(ReferralDoctorIdPrefix + seed.Suffix),
                    ReferralInstitutionId = institutionId,
                    DoctorName = seed.DoctorName,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                result.ReferralDoctorInserted++;
            }

            if (result.ReferralDoctorInserted > 0)
                await db.SaveChangesAsync(ct);
        }

        // =================================================================
        // Batas nilai rujukan beserta pilihan hasilnya
        // =================================================================

        /// <remarks>
        /// Sepuluh baris batas untuk sembilan jenis pemeriksaan — bukan sepuluh untuk sepuluh.
        /// Kedua penyimpangan itu disengaja, dan keduanya menggambarkan kenyataan yang harus
        /// dapat ditangani modul ini:
        ///
        /// 1. Hemoglobin mendapat <b>dua</b> baris, pria dan wanita. Inilah contoh BR-14 yang
        ///    membuat batas nilai berdiri sebagai tabel tersendiri dan bukan kolom pada
        ///    <c>MstProcedure</c>; tanpa satu pun jenis pemeriksaan berbatas ganda, bentuk
        ///    tabelnya tidak pernah benar-benar teruji.
        /// 2. Sitologi FNAB sengaja <b>tidak</b> diberi batas nilai. Pemeriksaan yang belum
        ///    punya batas memang lumrah, dan katalog harus tetap dapat menampilkannya. Baris
        ///    ini menjaga agar ketiadaan batas tidak diam-diam dianggap kerusakan.
        ///
        /// Semua angka memenuhi VAL-25 sampai VAL-27: batas kritis bawah selalu lebih rendah
        /// daripada batas normal bawah, dan batas kritis atas selalu lebih tinggi daripada batas
        /// normal atas. Baris berbentuk pilihan tidak membawa satuan maupun angka batas, sesuai
        /// VAL-22 sampai VAL-24.
        /// </remarks>
        private static readonly ValueBoundSeed[] ValueBounds =
        {
            new("01", "01", LabResultForm.Numeric, LabGenderScope.Male, "g/dL", 13.0m, 17.0m, 7.0m, 20.0m, 60),
            new("02", "01", LabResultForm.Numeric, LabGenderScope.Female, "g/dL", 12.0m, 15.0m, 7.0m, 20.0m, 60),
            new("03", "02", LabResultForm.Numeric, LabGenderScope.All, "10^3/uL", 4.0m, 11.0m, 2.0m, 30.0m, 60),
            new("04", "03", LabResultForm.Numeric, LabGenderScope.All, "10^3/uL", 150m, 400m, 50m, 1000m, 60),
            new("05", "04", LabResultForm.Numeric, LabGenderScope.All, "mg/dL", 70m, 140m, 40m, 400m, 30),
            new("06", "05", LabResultForm.Numeric, LabGenderScope.All, "mg/dL", 15m, 40m, 5m, 200m, 60),
            new("07", "06", LabResultForm.Numeric, LabGenderScope.All, "mg/dL", 0.6m, 1.2m, 0.2m, 10.0m, 60),
            new("08", "07", LabResultForm.Numeric, LabGenderScope.All, "mmol/L", 3.5m, 5.1m, 2.5m, 6.5m, 30),
            new("09", "08", LabResultForm.Choice, LabGenderScope.All, null, null, null, null, null, 60),
            new("10", "09", LabResultForm.Choice, LabGenderScope.All, null, null, null, null, null, 120)
        };

        /// <remarks>
        /// Pilihan hasil untuk kedua baris berbentuk pilihan. <c>IsOutOfReference</c> dan
        /// <c>IsCritical</c> sengaja tidak selalu bergerak bersama: protein +1 sudah di luar
        /// rujukan tetapi belum kritis, sedangkan BTA +1 keduanya. Urutan menyatakan tingkatan
        /// skala ordinalnya, dan itu isi bisnis — bukan urutan tampilan.
        /// </remarks>
        private static readonly ValueOptionSeed[] ValueOptions =
        {
            // Urinalisis Protein — batas nilai 09
            new("01", "09", "NEG", "Negatif", false, false, 10),
            new("02", "09", "P1", "+1", true, false, 20),
            new("03", "09", "P2", "+2", true, false, 30),
            new("04", "09", "P3", "+3", true, true, 40),
            new("05", "09", "P4", "+4", true, true, 50),

            // Pewarnaan BTA Sputum — batas nilai 10
            new("06", "10", "NEG", "Negatif", false, false, 10),
            new("07", "10", "SCANTY", "Scanty", true, false, 20),
            new("08", "10", "B1", "+1", true, true, 30),
            new("09", "10", "B2", "+2", true, true, 40),
            new("10", "10", "B3", "+3", true, true, 50)
        };

        private static async Task SeedValueBoundsAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            DateTime now,
            LabDummySeedResult result,
            CancellationToken ct)
        {
            var boundIds = ValueBounds
                .Select(x => Guid.Parse(ValueBoundIdPrefix + x.Suffix))
                .ToList();

            // Idempotensi diperiksa lewat Id, bukan hanya lewat kunci unik tabel. Alasannya:
            // kunci unik mengecualikan baris yang sudah dihapus lunak, sehingga memeriksa lewat
            // kunci saja akan mengisi ulang baris yang sengaja dihapus pengguna.
            var existingBoundIds = await db.LabValueBounds
                .AsNoTracking()
                .Where(x => boundIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync(ct);

            var existingBounds = new HashSet<Guid>(existingBoundIds);

            // Kunci unik tabel juga diperiksa, supaya baris contoh tidak menabrak batas nilai
            // yang sudah dibuat pengguna lewat layar pengelolaan untuk kelompok pasien yang sama.
            var procedureIds = ValueBounds
                .Select(x => Guid.Parse(ProcedureIdPrefix + x.ProcedureSuffix))
                .Distinct()
                .ToList();

            var occupiedPairs = await db.LabValueBounds
                .AsNoTracking()
                .Where(x => procedureIds.Contains(x.ProcedureId) && !x.IsDelete)
                .Select(x => new { x.ProcedureId, x.GenderScope, x.AgeCategoryId })
                .ToListAsync(ct);

            var occupied = new HashSet<string>(
                occupiedPairs.Select(x => x.ProcedureId + "|" + (int)x.GenderScope + "|" + x.AgeCategoryId));

            var seededBoundIds = new List<Guid>();

            foreach (var seed in ValueBounds)
            {
                var boundId = Guid.Parse(ValueBoundIdPrefix + seed.Suffix);
                var procedureId = Guid.Parse(ProcedureIdPrefix + seed.ProcedureSuffix);

                // AgeCategoryId dikosongkan pada seluruh baris contoh: batas di bawah berlaku
                // untuk semua umur. Kelompok umur tetap diisi seeder ini karena layar
                // pengelolaan batas nilai membutuhkan daftarnya untuk membuat batas per umur.
                var pairKey = procedureId + "|" + (int)seed.GenderScope + "|";

                if (existingBounds.Contains(boundId) || occupied.Contains(pairKey))
                {
                    result.ValueBoundSkipped++;
                    continue;
                }

                db.LabValueBounds.Add(new LabValueBound
                {
                    Id = boundId,
                    ProcedureId = procedureId,
                    ResultForm = seed.ResultForm,
                    Unit = seed.Unit,
                    NormalLow = seed.NormalLow,
                    NormalHigh = seed.NormalHigh,
                    CriticalLow = seed.CriticalLow,
                    CriticalHigh = seed.CriticalHigh,
                    GenderScope = seed.GenderScope,
                    AgeCategoryId = null,
                    CitoTurnaroundMinutes = seed.CitoTurnaroundMinutes,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                seededBoundIds.Add(boundId);
                result.ValueBoundInserted++;
            }

            if (result.ValueBoundInserted > 0)
                await db.SaveChangesAsync(ct);

            // Pilihan hasil hanya diisikan untuk batas yang baru saja dibuat seeder ini. Batas
            // yang sudah ada adalah milik pengguna, dan menyisipkan pilihan ke dalamnya berarti
            // mengubah daftar hasil yang sah tanpa diminta.
            if (seededBoundIds.Count == 0)
                return;

            var seededBounds = new HashSet<Guid>(seededBoundIds);

            foreach (var seed in ValueOptions)
            {
                var boundId = Guid.Parse(ValueBoundIdPrefix + seed.ValueBoundSuffix);

                if (!seededBounds.Contains(boundId))
                {
                    result.ValueOptionSkipped++;
                    continue;
                }

                db.LabValueOptions.Add(new LabValueOption
                {
                    Id = Guid.Parse(ValueOptionIdPrefix + seed.Suffix),
                    ValueBoundId = boundId,
                    OptionCode = seed.OptionCode,
                    OptionName = seed.OptionName,
                    IsOutOfReference = seed.IsOutOfReference,
                    IsCritical = seed.IsCritical,
                    SortOrder = seed.SortOrder,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                result.ValueOptionInserted++;
            }

            if (result.ValueOptionInserted > 0)
                await db.SaveChangesAsync(ct);
        }

        // =================================================================
        // Bentuk baris contoh
        // =================================================================

        private sealed record AgeCategorySeed(
            string Suffix,
            string Code,
            string Name,
            string ShortName,
            int MinAgeDays,
            int? MaxAgeDays,
            int SortOrder);

        private sealed record TariffCategorySeed(
            string Suffix,
            string Code,
            string Name,
            int SortOrder);

        private sealed record ProcedureSeed(
            string Suffix,
            string Code,
            string Name,
            string CategoryName,
            LabDiscipline Discipline,
            int SortOrder);

        private sealed record TariffSeed(
            string Suffix,
            string Code,
            string Name,
            string TariffCategorySuffix,
            string ProcedureSuffix,
            decimal NormalPrice,
            int SortOrder);

        private sealed record ReferralInstitutionSeed(
            string Suffix,
            string Code,
            string Name,
            string Address,
            string PhoneNumber);

        private sealed record ReferralDoctorSeed(
            string Suffix,
            string InstitutionSuffix,
            string DoctorName);

        private sealed record ValueBoundSeed(
            string Suffix,
            string ProcedureSuffix,
            LabResultForm ResultForm,
            LabGenderScope GenderScope,
            string? Unit,
            decimal? NormalLow,
            decimal? NormalHigh,
            decimal? CriticalLow,
            decimal? CriticalHigh,
            int? CitoTurnaroundMinutes);

        private sealed record ValueOptionSeed(
            string Suffix,
            string ValueBoundSuffix,
            string OptionCode,
            string OptionName,
            bool IsOutOfReference,
            bool IsCritical,
            int SortOrder);
    }

    /// <summary>
    /// Hasil satu kali penjalanan seeder data contoh Laboratorium. Baris yang dilewati dihitung
    /// terpisah dari baris yang ditambahkan supaya penjalanan ulang dapat dibuktikan tidak
    /// menulis apa pun.
    /// </summary>
    public class LabDummySeedResult
    {
        /// <summary>Terisi bila seeder menolak berjalan. Kosong berarti seeder benar-benar berjalan.</summary>
        public string? RefusedReason { get; set; }

        public int AgeCategoryInserted { get; set; }
        public int AgeCategorySkipped { get; set; }

        public int TariffCategoryInserted { get; set; }
        public int TariffCategorySkipped { get; set; }

        public int ProcedureInserted { get; set; }
        public int ProcedureSkipped { get; set; }

        public int TariffInserted { get; set; }
        public int TariffSkipped { get; set; }

        public int ReferralInstitutionInserted { get; set; }
        public int ReferralInstitutionSkipped { get; set; }

        public int ReferralDoctorInserted { get; set; }
        public int ReferralDoctorSkipped { get; set; }

        public int ValueBoundInserted { get; set; }
        public int ValueBoundSkipped { get; set; }

        public int ValueOptionInserted { get; set; }
        public int ValueOptionSkipped { get; set; }

        public int TotalInserted =>
            AgeCategoryInserted
            + TariffCategoryInserted
            + ProcedureInserted
            + TariffInserted
            + ReferralInstitutionInserted
            + ReferralDoctorInserted
            + ValueBoundInserted
            + ValueOptionInserted;
    }
}
