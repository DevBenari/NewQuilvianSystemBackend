using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Seeders
{
    /// <summary>
    /// Data awal 5 pelatihan wajib akreditasi STARKES KPS 8 (BE-HRD-239, VR-TRN-01, HRD-DEC-158).
    /// </summary>
    /// <remarks>
    /// Sebelum 10 November 2026 kelima aturan ini ditempel dari daftar in-memory ber-GUID tetap di
    /// <c>MandatoryTrainingRuleService</c> setiap kali tabel aturan kosong, sehingga layar menampilkan aturan yang tidak
    /// pernah ada di database. Seeder ini menyemai kategori pelatihan, katalog pelatihan, dan aturannya sekali saja;
    /// aturan yang sudah ada (berdasarkan <c>RuleCode</c>) tidak ditimpa supaya suntingan admin tetap terjaga.
    /// </remarks>
    public class MandatoryTrainingRuleSeeder
    {
        private const string CategoryCode = "TRN-CAT-STARKES";

        private sealed record RuleSeed(
            string RuleCode,
            string TrainingCode,
            string RuleName,
            string TrainingName,
            bool IsRequiredForCredentialing,
            string Description);

        private static readonly RuleSeed[] Seeds =
        {
            new("MTR-STARKES-BHD", "TRN-STARKES-BHD",
                "Pelatihan Wajib Bantuan Hidup Dasar (BHD / Basic Life Support)",
                "Bantuan Hidup Dasar (BHD / Basic Life Support)",
                true,
                "Standar KARS STARKES KPS 8: Seluruh staf medis dan non-medis wajib tersertifikasi BHD setiap 2 tahun."),
            new("MTR-STARKES-PPI", "TRN-STARKES-PPI",
                "Pelatihan Wajib Pencegahan & Pengendalian Infeksi (PPI / Hand Hygiene)",
                "Pencegahan & Pengendalian Infeksi (PPI / Hand Hygiene)",
                true,
                "Standar KARS STARKES PPI: Kewaspadaan standar, isolasi, pembuangan limbah medis dan kebersihan tangan 5 momen."),
            new("MTR-STARKES-SKP", "TRN-STARKES-SKP",
                "Pelatihan Wajib 6 Sasaran Keselamatan Pasien (SKP / Patient Safety)",
                "6 Sasaran Keselamatan Pasien (SKP / Patient Safety)",
                true,
                "Standar KARS STARKES SKP: Identifikasi pasien, komunikasi efektif, keamanan obat high-alert, keselamatan operasi, pencegahan infeksi & jatuh."),
            new("MTR-STARKES-K3RS", "TRN-STARKES-K3RS",
                "Pelatihan Wajib K3RS, Keselamatan Kerja & Penanggulangan Kebakaran (Code Red / APAR)",
                "K3RS, Keselamatan Kerja & Penanggulangan Kebakaran (Code Red / APAR)",
                false,
                "Standar KARS STARKES MFK: Tanggap darurat bencana alam, simulasi evakuasi dan penggunaan alat pemadam api ringan (APAR)."),
            new("MTR-STARKES-SBAR", "TRN-STARKES-SBAR",
                "Pelatihan Wajib Komunikasi Efektif Antar-Profesional (SBAR & TBAK)",
                "Komunikasi Efektif Antar-Profesional (SBAR & TBAK)",
                true,
                "Standar KARS STARKES SKP 2: Teknik komunikasi pelaporan klinis verbal SBAR (Situation, Background, Assessment, Recommendation) dan konfirmasi TBAK (Tulis, Baca, Konfirmasi)."),
        };

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var seedEnabled = configuration.GetValue<bool?>("SeedDefaultData:Enabled") ?? true;
            if (!seedEnabled)
            {
                Console.WriteLine("[Seeder] SeedDefaultData disabled.");
                return;
            }

            await EnsureMandatoryTrainingRulesAsync(dbContext);
        }

        private static async Task EnsureMandatoryTrainingRulesAsync(ApplicationDbContext dbContext)
        {
            var now = DateTime.UtcNow;

            var category = await dbContext.MstTrainingCategories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.TrainingCategoryCode == CategoryCode && !x.IsDelete);

            if (category == null)
            {
                category = new MstTrainingCategory
                {
                    Id = Guid.NewGuid(),
                    TrainingCategoryCode = CategoryCode,
                    TrainingCategoryName = "Pelatihan Wajib Akreditasi STARKES KPS 8",
                    IsMandatoryCategory = true,
                    Description = "Lima pelatihan wajib seluruh pegawai rumah sakit dengan siklus maksimal 24 bulan (VR-TRN-01, VR-TRN-02).",
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = Guid.Empty
                };
                dbContext.MstTrainingCategories.Add(category);
            }

            var existingRuleCodes = await dbContext.MstMandatoryTrainingRules
                .IgnoreQueryFilters()
                .Where(x => !x.IsDelete)
                .Select(x => x.RuleCode)
                .ToListAsync();

            var existingCatalogs = await dbContext.MstTrainingCatalogs
                .IgnoreQueryFilters()
                .Where(x => !x.IsDelete)
                .ToDictionaryAsync(x => x.TrainingCode, x => x);

            var seededCount = 0;
            var priority = Seeds.Length;

            foreach (var seed in Seeds)
            {
                if (!existingCatalogs.TryGetValue(seed.TrainingCode, out var catalog))
                {
                    catalog = new MstTrainingCatalog
                    {
                        Id = Guid.NewGuid(),
                        TrainingCategoryId = category.Id,
                        TrainingCode = seed.TrainingCode,
                        TrainingName = seed.TrainingName,
                        TrainingType = "Internal",
                        DeliveryMethod = "Classroom",
                        DefaultProviderName = "Komite Diklat RS",
                        DurationHours = 8,
                        ValidityMonths = 24,
                        IsMandatory = true,
                        RequiresAssessment = true,
                        MinimumPassingScore = 75.00m,
                        IssuesCertificate = true,
                        Description = seed.Description,
                        IsActive = true,
                        CreateDateTime = now,
                        CreateBy = Guid.Empty
                    };
                    dbContext.MstTrainingCatalogs.Add(catalog);
                    existingCatalogs[seed.TrainingCode] = catalog;
                }

                if (!existingRuleCodes.Contains(seed.RuleCode))
                {
                    dbContext.MstMandatoryTrainingRules.Add(new MstMandatoryTrainingRule
                    {
                        Id = Guid.NewGuid(),
                        TrainingCatalogId = catalog.Id,
                        RuleCode = seed.RuleCode,
                        RuleName = seed.RuleName,
                        CompletionDueDaysFromJoin = 90,
                        RecurrenceMonths = 24,
                        GracePeriodDays = 30,
                        IsRequiredBeforeAssignment = false,
                        IsRequiredForCredentialing = seed.IsRequiredForCredentialing,
                        IsRequiredBeforeIndependentPractice = true,
                        RequiresPassingResult = true,
                        MinimumPassingScore = 75.00m,
                        Priority = priority,
                        Description = seed.Description,
                        IsActive = true,
                        CreateDateTime = now,
                        CreateBy = Guid.Empty
                    });
                    seededCount++;
                }

                priority--;
            }

            if (dbContext.ChangeTracker.HasChanges())
            {
                await dbContext.SaveChangesAsync();
                Console.WriteLine($"[Seeder] MandatoryTrainingRuleSeeder: {seededCount} aturan pelatihan wajib STARKES disemai.");
            }
            else
            {
                Console.WriteLine("[Seeder] MandatoryTrainingRuleSeeder: seluruh aturan pelatihan wajib STARKES sudah ada.");
            }
        }
    }
}
