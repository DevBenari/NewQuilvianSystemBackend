using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    /// <summary>
    /// Implementasi Layanan Master Aturan Pelatihan Wajib STARKES KPS 8 (VR-TRN-01, VR-TRN-02)
    /// </summary>
    /// <remarks>
    /// Remediasi 10 November 2026 (BE-HRD-239): daftar aturan dibaca hanya dari tabel <c>MstMandatoryTrainingRule</c>.
    /// Lima aturan bawaan STARKES yang dulu ditempel dari daftar in-memory ber-GUID tetap kini disemai ke database oleh
    /// <c>Seeders/MandatoryTrainingRuleSeeder</c> saat aplikasi mulai, sehingga layar menampilkan data yang benar-benar ada.
    /// </remarks>
    public class MandatoryTrainingRuleService : IMandatoryTrainingRuleService
    {
        private const string DefaultTrainingCategory = "STARKES_MANDATORY";
        private const decimal DefaultMinimumPassingScore = 75.00m;

        private readonly ApplicationDbContext _context;

        public MandatoryTrainingRuleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MandatoryTrainingRuleDto>> GetMandatoryRulesAsync(CancellationToken cancellationToken = default)
        {
            var dbRules = await _context.MstMandatoryTrainingRules
                .AsNoTracking()
                .Include(r => r.TrainingCatalog)
                    .ThenInclude(c => c!.TrainingCategory)
                .Where(r => !r.IsDelete)
                .OrderByDescending(r => r.Priority)
                .ThenBy(r => r.RuleName)
                .ToListAsync(cancellationToken);

            return dbRules.Select(r => new MandatoryTrainingRuleDto
            {
                Id = r.Id,
                RuleCode = r.RuleCode,
                RuleName = r.RuleName,
                TrainingCategory = r.TrainingCatalog?.TrainingCategory?.TrainingCategoryCode ?? DefaultTrainingCategory,
                TargetAudience = ResolveTargetAudience(r),
                RecurrenceMonths = r.RecurrenceMonths ?? 24,
                GracePeriodDays = r.GracePeriodDays,
                IsRequiredBeforeIndependentPractice = r.IsRequiredBeforeIndependentPractice,
                IsRequiredForCredentialing = r.IsRequiredForCredentialing,
                RequiresPassingResult = r.RequiresPassingResult,
                MinimumPassingScore = r.MinimumPassingScore ?? DefaultMinimumPassingScore,
                // Tabel aturan belum menyimpan ambang OSCE terpisah; dipakai ambang kelulusan yang tersimpan, bukan konstanta karangan.
                OscePassingScore = r.MinimumPassingScore ?? DefaultMinimumPassingScore,
                Description = r.Description,
                IsActive = r.IsActive
            }).ToList();
        }

        public async Task<MandatoryTrainingRuleDto> CreateOrUpdateRuleAsync(
            CreateMandatoryTrainingRuleRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Validasi VR-TRN-02: Masa berlaku maksimal 24 bulan
            if (request.RecurrenceMonths > 24)
            {
                throw new InvalidOperationException("Masa berlaku pelatihan wajib rumah sakit dibatasi maksimal 24 bulan sesuai regulasi akreditasi (VR-TRN-02).");
            }

            if (string.IsNullOrWhiteSpace(request.RuleName))
            {
                throw new ArgumentException("Nama aturan pelatihan wajib diisi.");
            }

            // Cek apakah ada record yang cocok di database
            var existing = await _context.MstMandatoryTrainingRules
                .FirstOrDefaultAsync(r => r.RuleCode == request.RuleCode && !r.IsDelete, cancellationToken);

            if (existing == null)
            {
                existing = new MstMandatoryTrainingRule
                {
                    Id = Guid.NewGuid(),
                    TrainingCatalogId = await ResolveTrainingCatalogIdAsync(request, cancellationToken),
                    RuleCode = request.RuleCode,
                    RuleName = request.RuleName.Trim(),
                    RecurrenceMonths = request.RecurrenceMonths,
                    GracePeriodDays = request.GracePeriodDays,
                    IsRequiredBeforeIndependentPractice = request.IsRequiredBeforeIndependentPractice,
                    IsRequiredForCredentialing = request.IsRequiredForCredentialing,
                    RequiresPassingResult = request.RequiresPassingResult,
                    MinimumPassingScore = request.MinimumPassingScore,
                    Description = request.Description,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow
                };
                _context.MstMandatoryTrainingRules.Add(existing);
            }
            else
            {
                existing.RuleName = request.RuleName.Trim();
                existing.RecurrenceMonths = request.RecurrenceMonths;
                existing.GracePeriodDays = request.GracePeriodDays;
                existing.IsRequiredBeforeIndependentPractice = request.IsRequiredBeforeIndependentPractice;
                existing.IsRequiredForCredentialing = request.IsRequiredForCredentialing;
                existing.RequiresPassingResult = request.RequiresPassingResult;
                existing.MinimumPassingScore = request.MinimumPassingScore;
                existing.Description = request.Description;
                existing.UpdateDateTime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new MandatoryTrainingRuleDto
            {
                Id = existing.Id,
                RuleCode = existing.RuleCode,
                RuleName = existing.RuleName,
                TrainingCategory = request.TrainingCategory,
                TargetAudience = request.TargetAudience,
                RecurrenceMonths = existing.RecurrenceMonths ?? 24,
                GracePeriodDays = existing.GracePeriodDays,
                IsRequiredBeforeIndependentPractice = existing.IsRequiredBeforeIndependentPractice,
                IsRequiredForCredentialing = existing.IsRequiredForCredentialing,
                RequiresPassingResult = existing.RequiresPassingResult,
                MinimumPassingScore = existing.MinimumPassingScore ?? DefaultMinimumPassingScore,
                OscePassingScore = request.OscePassingScore,
                Description = existing.Description,
                IsActive = existing.IsActive
            };
        }

        /// <summary>
        /// Katalog wajib ada karena aturan merujuknya. Bila pemanggil tidak menyebutnya, dipakai katalog bernama sama
        /// atau dibuat katalog baru pada kategori yang diminta.
        /// </summary>
        private async Task<Guid> ResolveTrainingCatalogIdAsync(CreateMandatoryTrainingRuleRequestDto request, CancellationToken cancellationToken)
        {
            if (request.TrainingCatalogId.HasValue)
            {
                var exists = await _context.MstTrainingCatalogs.AsNoTracking()
                    .AnyAsync(c => c.Id == request.TrainingCatalogId.Value && !c.IsDelete, cancellationToken);
                if (!exists) throw new ArgumentException("Katalog pelatihan tidak ditemukan.");
                return request.TrainingCatalogId.Value;
            }

            var name = request.RuleName.Trim();
            var matched = await _context.MstTrainingCatalogs.AsNoTracking()
                .Where(c => !c.IsDelete && c.TrainingName == name)
                .Select(c => (Guid?)c.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (matched.HasValue) return matched.Value;

            var categoryText = request.TrainingCategory?.Trim();
            var category = await _context.MstTrainingCategories
                .Where(c => !c.IsDelete && (c.TrainingCategoryCode == categoryText || c.TrainingCategoryName == categoryText))
                .FirstOrDefaultAsync(cancellationToken)
                ?? await _context.MstTrainingCategories
                    .Where(c => !c.IsDelete && c.IsMandatoryCategory)
                    .OrderBy(c => c.TrainingCategoryCode)
                    .FirstOrDefaultAsync(cancellationToken)
                ?? throw new ArgumentException("Kategori pelatihan belum tersedia. Buat kategori pelatihan lebih dulu.");

            var now = DateTime.UtcNow;
            var catalog = new MstTrainingCatalog
            {
                Id = Guid.NewGuid(),
                TrainingCategoryId = category.Id,
                TrainingCode = request.RuleCode.Trim(),
                TrainingName = name,
                TrainingType = "Internal",
                DeliveryMethod = "Classroom",
                ValidityMonths = request.RecurrenceMonths,
                IsMandatory = true,
                RequiresAssessment = request.RequiresPassingResult,
                MinimumPassingScore = request.MinimumPassingScore,
                Description = request.Description,
                IsActive = true,
                CreateDateTime = now
            };
            _context.MstTrainingCatalogs.Add(catalog);
            return catalog.Id;
        }

        /// <summary>
        /// Sasaran aturan diturunkan dari cakupan yang benar-benar tersimpan: aturan tanpa pembatas profesi, jabatan,
        /// kategori, atau unit berlaku untuk seluruh pegawai.
        /// </summary>
        private static string ResolveTargetAudience(MstMandatoryTrainingRule rule)
        {
            var hasScope = rule.ProfessionId.HasValue || rule.PositionId.HasValue || rule.EmployeeCategoryId.HasValue
                || rule.EmploymentTypeId.HasValue || rule.OrganizationUnitId.HasValue || rule.SpecializationId.HasValue;

            return hasScope ? "Pegawai pada cakupan aturan" : "Seluruh Pegawai RS (100%)";
        }
    }
}
