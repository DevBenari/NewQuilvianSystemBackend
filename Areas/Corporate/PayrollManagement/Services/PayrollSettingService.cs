using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Options;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    /// <summary>
    /// Layanan konfigurasi master parameter penggajian dan pembekuan snapshot periode (BE-HRD-147, HRD-DEC-122 s.d. 124).
    /// </summary>
    public class PayrollSettingService : IPayrollSettingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PayrollSettingService> _logger;

        public PayrollSettingService(
            ApplicationDbContext context,
            ILogger<PayrollSettingService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HrdPayrollSettingDto> GetSettingAsync(CancellationToken cancellationToken = default)
        {
            var setting = await GetOrCreateDefaultSettingAsync(cancellationToken);

            var dto = MapToDto(setting);
            await ApplyPolicyStatusAsync(dto, cancellationToken);
            return dto;
        }

        /// <summary>Menandai ketersediaan kebijakan tarif PPh yang disahkan dan berlaku hari ini. Murni baca.</summary>
        private async Task ApplyPolicyStatusAsync(HrdPayrollSettingDto dto, CancellationToken cancellationToken)
        {
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            var tax = await _context.Set<PrlTaxPolicy>().AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.PolicyStatus == StatutoryPolicyStatuses.Approved
                    && x.EffectiveStartDate <= today && (x.EffectiveEndDate == null || x.EffectiveEndDate >= today))
                .OrderByDescending(x => x.EffectiveStartDate)
                .Select(x => x.PolicyVersion)
                .FirstOrDefaultAsync(cancellationToken);

            dto.TaxPolicyApproved = tax is not null;
            dto.TaxPolicyVersion = tax;
        }

        public async Task<HrdPayrollSettingDto> UpdateSettingAsync(
            UpdateHrdPayrollSettingCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var setting = await GetOrCreateDefaultSettingAsync(cancellationToken);

            setting.SettingName = command.SettingName;

            // Kolom BPJS pada HrdPayrollSetting (LEGACY_PERSISTED) tidak pernah disentuh: tidak dibaca dari command dan tidak ditimpa,
            // sehingga nilai yang sudah tersimpan tetap utuh sebagai data historis.
            setting.VarianceThresholdPercentage = command.VarianceThresholdPercentage;
            setting.VarianceThresholdAmount = command.VarianceThresholdAmount;
            setting.MonthlyCutoffDay = command.MonthlyCutoffDay;
            setting.DefaultPaymentDay = command.DefaultPaymentDay;
            setting.DefaultNightShiftAllowanceAmount = command.DefaultNightShiftAllowanceAmount;
            setting.MaxLoanDeductionPercentage = command.MaxLoanDeductionPercentage;
            setting.UpdateBy = actorUserId;
            setting.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Parameter master penggajian berhasil diperbarui oleh {UserId}.", actorUserId);

            var updated = MapToDto(setting);
            await ApplyPolicyStatusAsync(updated, cancellationToken);
            return updated;
        }

        public async Task<string> GetSettingSnapshotJsonAsync(CancellationToken cancellationToken = default)
        {
            var setting = await GetOrCreateDefaultSettingAsync(cancellationToken);
            var dto = MapToDto(setting);
            return JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = false });
        }

        private async Task<HrdPayrollSetting> GetOrCreateDefaultSettingAsync(CancellationToken cancellationToken)
        {
            var setting = await _context.Set<HrdPayrollSetting>()
                .FirstOrDefaultAsync(x => !x.IsDelete && x.IsActive, cancellationToken);

            if (setting == null)
            {
                setting = new HrdPayrollSetting
                {
                    Id = Guid.NewGuid(),
                    SettingName = "Standar Penggajian Rumah Sakit",
                    VarianceThresholdPercentage = 20.00m,
                    VarianceThresholdAmount = 2000000m,
                    MonthlyCutoffDay = 20,
                    DefaultPaymentDay = 25,
                    DefaultNightShiftAllowanceAmount = 50000m,
                    MaxLoanDeductionPercentage = 50.00m,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    IsDelete = false
                };

                await _context.Set<HrdPayrollSetting>().AddAsync(setting, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return setting;
        }

        private static HrdPayrollSettingDto MapToDto(HrdPayrollSetting entity) => new()
        {
            Id = entity.Id,
            HospitalSiteId = entity.HospitalSiteId,
            SettingName = entity.SettingName,
            VarianceThresholdPercentage = entity.VarianceThresholdPercentage,
            VarianceThresholdAmount = entity.VarianceThresholdAmount,
            MonthlyCutoffDay = entity.MonthlyCutoffDay,
            DefaultPaymentDay = entity.DefaultPaymentDay,
            DefaultNightShiftAllowanceAmount = entity.DefaultNightShiftAllowanceAmount,
            MaxLoanDeductionPercentage = entity.MaxLoanDeductionPercentage,
            IsActive = entity.IsActive,
            LastUpdatedAt = entity.UpdateDateTime ?? entity.CreateDateTime
        };
    }
}
