using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public class RestitutionSettingService : IRestitutionSettingService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<RestitutionSettingService> _logger;

        public RestitutionSettingService(
            ApplicationDbContext dbContext,
            ILogger<RestitutionSettingService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<RestitutionSettingResponseDto>> GetAllSettingsAsync(
            RestitutionSettingFilterDto? filter = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.HrdRestitutionSettings
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.GradeLevelCode))
                    query = query.Where(x => x.GradeLevelCode == filter.GradeLevelCode);

                if (!string.IsNullOrWhiteSpace(filter.BenefitCategory))
                    query = query.Where(x => x.BenefitCategory == filter.BenefitCategory);

                if (filter.IsActive.HasValue)
                    query = query.Where(x => x.IsActive == filter.IsActive.Value);
            }

            var entities = await query
                .OrderBy(x => x.GradeLevelCode)
                .ThenBy(x => x.BenefitCategory)
                .ToListAsync(cancellationToken);

            return entities.Select(MapToResponseDto).ToList();
        }

        public async Task<RestitutionSettingResponseDto?> GetSettingByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.HrdRestitutionSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            return entity != null ? MapToResponseDto(entity) : null;
        }

        public async Task<RestitutionSettingResponseDto?> GetPolicyForGradeAndCategoryAsync(
            string gradeLevelCode,
            string benefitCategory,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.HrdRestitutionSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.GradeLevelCode == gradeLevelCode &&
                                          x.BenefitCategory == benefitCategory &&
                                          x.IsActive &&
                                          !x.IsDelete, cancellationToken);

            // Fallback ke GRADE-ALL jika spesifik grade tidak ditemukan
            if (entity == null && gradeLevelCode != "GRADE-ALL")
            {
                entity = await _dbContext.HrdRestitutionSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.GradeLevelCode == "GRADE-ALL" &&
                                              x.BenefitCategory == benefitCategory &&
                                              x.IsActive &&
                                              !x.IsDelete, cancellationToken);
            }

            return entity != null ? MapToResponseDto(entity) : null;
        }

        public async Task<decimal> GetMaxDeductionPercentageAsync(
            string? gradeLevelCode = null,
            CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(gradeLevelCode))
            {
                var specificPolicy = await _dbContext.HrdRestitutionSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.GradeLevelCode == gradeLevelCode &&
                                              x.BenefitCategory == "KebijakanUmumPemotongan" &&
                                              x.IsActive &&
                                              !x.IsDelete, cancellationToken);

                if (specificPolicy != null)
                    return specificPolicy.MaxDeductionPercentageOfGross;
            }

            var generalPolicy = await _dbContext.HrdRestitutionSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.GradeLevelCode == "GRADE-ALL" &&
                                          x.BenefitCategory == "KebijakanUmumPemotongan" &&
                                          x.IsActive &&
                                          !x.IsDelete, cancellationToken);

            return generalPolicy?.MaxDeductionPercentageOfGross ?? 50.00m; // Default mutlak 50% PP 36/2021
        }

        public async Task<RestitutionSettingResponseDto> CreateOrUpdateSettingAsync(
            CreateRestitutionSettingRequestDto dto,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var existing = await _dbContext.HrdRestitutionSettings
                .FirstOrDefaultAsync(x => x.GradeLevelCode == dto.GradeLevelCode &&
                                          x.BenefitCategory == dto.BenefitCategory &&
                                          !x.IsDelete, cancellationToken);

            if (existing != null)
            {
                existing.AnnualLimitAmount = dto.AnnualLimitAmount;
                existing.MinimumServiceMonthsRequired = dto.MinimumServiceMonthsRequired;
                existing.MaxDeductionPercentageOfGross = dto.MaxDeductionPercentageOfGross;
                existing.AllowEmergencyCitoCashier = dto.AllowEmergencyCitoCashier;
                existing.IsActive = dto.IsActive;
                existing.UpdateDateTime = DateTime.UtcNow;
                existing.UpdateBy = userId;
            }
            else
            {
                existing = new HrdRestitutionSetting
                {
                    Id = Guid.NewGuid(),
                    GradeLevelCode = dto.GradeLevelCode,
                    BenefitCategory = dto.BenefitCategory,
                    AnnualLimitAmount = dto.AnnualLimitAmount,
                    MinimumServiceMonthsRequired = dto.MinimumServiceMonthsRequired,
                    MaxDeductionPercentageOfGross = dto.MaxDeductionPercentageOfGross,
                    AllowEmergencyCitoCashier = dto.AllowEmergencyCitoCashier,
                    IsActive = dto.IsActive,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userId
                };
                await _dbContext.HrdRestitutionSettings.AddAsync(existing, cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Kebijakan restitusi {Grade}-{Category} berhasil disimpan oleh {User}.",
                dto.GradeLevelCode, dto.BenefitCategory, userId);

            return MapToResponseDto(existing);
        }

        public async Task<int> SeedDefaultHospitalSettingsAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var defaultSettings = new List<HrdRestitutionSetting>
            {
                // Kebijakan Umum
                new() { GradeLevelCode = "GRADE-ALL", BenefitCategory = "KebijakanUmumPemotongan", AnnualLimitAmount = 0m, MinimumServiceMonthsRequired = 0, MaxDeductionPercentageOfGross = 50.00m, AllowEmergencyCitoCashier = false, IsActive = true, CreateBy = userId, CreateDateTime = DateTime.UtcNow },

                // Grade I (Staf Medis / Dokter Spesialis)
                new() { GradeLevelCode = "GRADE-I", BenefitCategory = "KacamataKerja", AnnualLimitAmount = 2500000m, MinimumServiceMonthsRequired = 6, MaxDeductionPercentageOfGross = 50.00m, AllowEmergencyCitoCashier = false, IsActive = true, CreateBy = userId, CreateDateTime = DateTime.UtcNow },
                new() { GradeLevelCode = "GRADE-I", BenefitCategory = "FaskesLuarDarurat", AnnualLimitAmount = 10000000m, MinimumServiceMonthsRequired = 3, MaxDeductionPercentageOfGross = 50.00m, AllowEmergencyCitoCashier = true, IsActive = true, CreateBy = userId, CreateDateTime = DateTime.UtcNow },

                // Grade II (Perawat & Bidan)
                new() { GradeLevelCode = "GRADE-II", BenefitCategory = "KacamataKerja", AnnualLimitAmount = 1500000m, MinimumServiceMonthsRequired = 6, MaxDeductionPercentageOfGross = 50.00m, AllowEmergencyCitoCashier = false, IsActive = true, CreateBy = userId, CreateDateTime = DateTime.UtcNow },
                new() { GradeLevelCode = "GRADE-II", BenefitCategory = "FaskesLuarDarurat", AnnualLimitAmount = 7500000m, MinimumServiceMonthsRequired = 3, MaxDeductionPercentageOfGross = 50.00m, AllowEmergencyCitoCashier = true, IsActive = true, CreateBy = userId, CreateDateTime = DateTime.UtcNow },

                // Grade III (Penunjang Medik & Laboran)
                new() { GradeLevelCode = "GRADE-III", BenefitCategory = "KacamataKerja", AnnualLimitAmount = 1250000m, MinimumServiceMonthsRequired = 6, MaxDeductionPercentageOfGross = 50.00m, AllowEmergencyCitoCashier = false, IsActive = true, CreateBy = userId, CreateDateTime = DateTime.UtcNow },
                new() { GradeLevelCode = "GRADE-III", BenefitCategory = "FaskesLuarDarurat", AnnualLimitAmount = 5000000m, MinimumServiceMonthsRequired = 3, MaxDeductionPercentageOfGross = 50.00m, AllowEmergencyCitoCashier = true, IsActive = true, CreateBy = userId, CreateDateTime = DateTime.UtcNow },

                // Grade IV (Staf Umum & Administrasi)
                new() { GradeLevelCode = "GRADE-IV", BenefitCategory = "KacamataKerja", AnnualLimitAmount = 1000000m, MinimumServiceMonthsRequired = 12, MaxDeductionPercentageOfGross = 50.00m, AllowEmergencyCitoCashier = false, IsActive = true, CreateBy = userId, CreateDateTime = DateTime.UtcNow },
                new() { GradeLevelCode = "GRADE-IV", BenefitCategory = "FaskesLuarDarurat", AnnualLimitAmount = 3000000m, MinimumServiceMonthsRequired = 12, MaxDeductionPercentageOfGross = 50.00m, AllowEmergencyCitoCashier = false, IsActive = true, CreateBy = userId, CreateDateTime = DateTime.UtcNow }
            };

            int insertedCount = 0;
            foreach (var s in defaultSettings)
            {
                var exists = await _dbContext.HrdRestitutionSettings
                    .AnyAsync(x => x.GradeLevelCode == s.GradeLevelCode &&
                                   x.BenefitCategory == s.BenefitCategory &&
                                   !x.IsDelete, cancellationToken);
                if (!exists)
                {
                    await _dbContext.HrdRestitutionSettings.AddAsync(s, cancellationToken);
                    insertedCount++;
                }
            }

            if (insertedCount > 0)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Berhasil menyemai {Count} pengaturan awal restitusi rumah sakit.", insertedCount);
            }

            return insertedCount;
        }

        private static RestitutionSettingResponseDto MapToResponseDto(HrdRestitutionSetting entity)
        {
            return new RestitutionSettingResponseDto
            {
                Id = entity.Id,
                GradeLevelCode = entity.GradeLevelCode,
                BenefitCategory = entity.BenefitCategory,
                AnnualLimitAmount = entity.AnnualLimitAmount,
                MinimumServiceMonthsRequired = entity.MinimumServiceMonthsRequired,
                MaxDeductionPercentageOfGross = entity.MaxDeductionPercentageOfGross,
                AllowEmergencyCitoCashier = entity.AllowEmergencyCitoCashier,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreateDateTime,
                UpdatedAt = entity.UpdateDateTime
            };
        }
    }
}
