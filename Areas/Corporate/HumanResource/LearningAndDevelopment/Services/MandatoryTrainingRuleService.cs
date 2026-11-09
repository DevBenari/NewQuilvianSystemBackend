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
    public class MandatoryTrainingRuleService : IMandatoryTrainingRuleService
    {
        private readonly ApplicationDbContext _context;

        // In-memory catalog seed untuk 5 Pelatihan Mandatori STARKES KPS 8 (VR-TRN-01)
        private static readonly List<MandatoryTrainingRuleDto> _defaultStarkesRules = new()
        {
            new MandatoryTrainingRuleDto
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                RuleCode = "MTR-STARKES-BHD",
                RuleName = "Pelatihan Wajib Bantuan Hidup Dasar (BHD / Basic Life Support)",
                TrainingCategory = "STARKES_MANDATORY",
                TargetAudience = "Seluruh Pegawai RS (100%)",
                RecurrenceMonths = 24,
                GracePeriodDays = 30,
                IsRequiredBeforeIndependentPractice = true,
                IsRequiredForCredentialing = true,
                RequiresPassingResult = true,
                MinimumPassingScore = 75.00m,
                OscePassingScore = 80.00m,
                Description = "Standar KARS STARKES KPS 8: Seluruh staf medis dan non-medis wajib tersertifikasi BHD setiap 2 tahun.",
                IsActive = true
            },
            new MandatoryTrainingRuleDto
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111102"),
                RuleCode = "MTR-STARKES-PPI",
                RuleName = "Pelatihan Wajib Pencegahan & Pengendalian Infeksi (PPI / Hand Hygiene)",
                TrainingCategory = "STARKES_MANDATORY",
                TargetAudience = "Seluruh Pegawai RS (100%)",
                RecurrenceMonths = 24,
                GracePeriodDays = 30,
                IsRequiredBeforeIndependentPractice = true,
                IsRequiredForCredentialing = true,
                RequiresPassingResult = true,
                MinimumPassingScore = 75.00m,
                OscePassingScore = 75.00m,
                Description = "Standar KARS STARKES PPI: Kewaspadaan standar, isolasi, pembuangan limbah medis dan kebersihan tangan 5 momen.",
                IsActive = true
            },
            new MandatoryTrainingRuleDto
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111103"),
                RuleCode = "MTR-STARKES-SKP",
                RuleName = "Pelatihan Wajib 6 Sasaran Keselamatan Pasien (SKP / Patient Safety)",
                TrainingCategory = "STARKES_MANDATORY",
                TargetAudience = "Seluruh Pegawai RS (100%)",
                RecurrenceMonths = 24,
                GracePeriodDays = 30,
                IsRequiredBeforeIndependentPractice = true,
                IsRequiredForCredentialing = true,
                RequiresPassingResult = true,
                MinimumPassingScore = 75.00m,
                OscePassingScore = 75.00m,
                Description = "Standar KARS STARKES SKP: Identifikasi pasien, komunikasi efektif, keamanan obat high-alert, keselamatan operasi, pencegahan infeksi & jatuh.",
                IsActive = true
            },
            new MandatoryTrainingRuleDto
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111104"),
                RuleCode = "MTR-STARKES-K3RS",
                RuleName = "Pelatihan Wajib K3RS, Keselamatan Kerja & Penanggulangan Kebakaran (Code Red / APAR)",
                TrainingCategory = "STARKES_MANDATORY",
                TargetAudience = "Seluruh Pegawai RS (100%)",
                RecurrenceMonths = 24,
                GracePeriodDays = 30,
                IsRequiredBeforeIndependentPractice = true,
                IsRequiredForCredentialing = false,
                RequiresPassingResult = true,
                MinimumPassingScore = 75.00m,
                OscePassingScore = 75.00m,
                Description = "Standar KARS STARKES MFK: Tanggap darurat bencana alam, simulasi evakuasi dan penggunaan alat pemadam api ringan (APAR).",
                IsActive = true
            },
            new MandatoryTrainingRuleDto
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111105"),
                RuleCode = "MTR-STARKES-SBAR",
                RuleName = "Pelatihan Wajib Komunikasi Efektif Antar-Profesional (SBAR & TBAK)",
                TrainingCategory = "STARKES_MANDATORY",
                TargetAudience = "Seluruh Tenaga Medis, Keperawatan & Penunjang",
                RecurrenceMonths = 24,
                GracePeriodDays = 30,
                IsRequiredBeforeIndependentPractice = true,
                IsRequiredForCredentialing = true,
                RequiresPassingResult = true,
                MinimumPassingScore = 75.00m,
                OscePassingScore = 80.00m,
                Description = "Standar KARS STARKES SKP 2: Teknik komunikasi pelaporan klinis verbal SBAR (Situation, Background, Assessment, Recommendation) dan konfirmasi TBAK (Tulis, Baca, Konfirmasi).",
                IsActive = true
            }
        };

        public MandatoryTrainingRuleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MandatoryTrainingRuleDto>> GetMandatoryRulesAsync(CancellationToken cancellationToken = default)
        {
            var dbRules = await _context.MstMandatoryTrainingRules
                .Where(r => !r.IsDelete)
                .OrderByDescending(r => r.Priority)
                .ThenBy(r => r.RuleName)
                .ToListAsync(cancellationToken);

            var result = new List<MandatoryTrainingRuleDto>();

            // Jika ada di database, petakan
            foreach (var r in dbRules)
            {
                result.Add(new MandatoryTrainingRuleDto
                {
                    Id = r.Id,
                    RuleCode = r.RuleCode,
                    RuleName = r.RuleName,
                    TrainingCategory = "STARKES_MANDATORY",
                    TargetAudience = "Tenaga Kerja Rumah Sakit",
                    RecurrenceMonths = r.RecurrenceMonths ?? 24,
                    GracePeriodDays = r.GracePeriodDays,
                    IsRequiredBeforeIndependentPractice = r.IsRequiredBeforeIndependentPractice,
                    IsRequiredForCredentialing = r.IsRequiredForCredentialing,
                    RequiresPassingResult = r.RequiresPassingResult,
                    MinimumPassingScore = r.MinimumPassingScore ?? 75.00m,
                    OscePassingScore = 80.00m,
                    Description = r.Description,
                    IsActive = r.IsActive
                });
            }

            // Gabungkan dengan default STARKES mandatori jika belum ada
            foreach (var def in _defaultStarkesRules)
            {
                if (!result.Any(x => x.RuleCode == def.RuleCode))
                {
                    result.Add(def);
                }
            }

            return result;
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
                MinimumPassingScore = existing.MinimumPassingScore ?? 75.00m,
                OscePassingScore = request.OscePassingScore,
                Description = existing.Description,
                IsActive = existing.IsActive
            };
        }
    }
}
