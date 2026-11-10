using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
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

            return MapToDto(setting);
        }

        public async Task<HrdPayrollSettingDto> UpdateSettingAsync(
            UpdateHrdPayrollSettingCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var setting = await GetOrCreateDefaultSettingAsync(cancellationToken);

            setting.SettingName = command.SettingName;
            setting.BpjsKesehatanCapAmount = command.BpjsKesehatanCapAmount;
            setting.BpjsKesehatanEmployeeRate = command.BpjsKesehatanEmployeeRate;
            setting.BpjsKesehatanEmployerRate = command.BpjsKesehatanEmployerRate;
            setting.BpjsKetenagakerjaanJpCapAmount = command.BpjsKetenagakerjaanJpCapAmount;
            setting.BpjsKetenagakerjaanJpEmployeeRate = command.BpjsKetenagakerjaanJpEmployeeRate;
            setting.BpjsKetenagakerjaanJpEmployerRate = command.BpjsKetenagakerjaanJpEmployerRate;
            setting.BpjsKetenagakerjaanJhtEmployeeRate = command.BpjsKetenagakerjaanJhtEmployeeRate;
            setting.BpjsKetenagakerjaanJhtEmployerRate = command.BpjsKetenagakerjaanJhtEmployerRate;
            setting.BpjsKetenagakerjaanJkkRate = command.BpjsKetenagakerjaanJkkRate;
            setting.BpjsKetenagakerjaanJkmRate = command.BpjsKetenagakerjaanJkmRate;
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

            return MapToDto(setting);
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
                    BpjsKesehatanCapAmount = 12000000m,
                    BpjsKesehatanEmployeeRate = 1.00m,
                    BpjsKesehatanEmployerRate = 4.00m,
                    BpjsKetenagakerjaanJpCapAmount = 10042300m,
                    BpjsKetenagakerjaanJpEmployeeRate = 1.00m,
                    BpjsKetenagakerjaanJpEmployerRate = 2.00m,
                    BpjsKetenagakerjaanJhtEmployeeRate = 2.00m,
                    BpjsKetenagakerjaanJhtEmployerRate = 3.70m,
                    BpjsKetenagakerjaanJkkRate = 0.89m,
                    BpjsKetenagakerjaanJkmRate = 0.30m,
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
            BpjsKesehatanCapAmount = entity.BpjsKesehatanCapAmount,
            BpjsKesehatanEmployeeRate = entity.BpjsKesehatanEmployeeRate,
            BpjsKesehatanEmployerRate = entity.BpjsKesehatanEmployerRate,
            BpjsKetenagakerjaanJpCapAmount = entity.BpjsKetenagakerjaanJpCapAmount,
            BpjsKetenagakerjaanJpEmployeeRate = entity.BpjsKetenagakerjaanJpEmployeeRate,
            BpjsKetenagakerjaanJpEmployerRate = entity.BpjsKetenagakerjaanJpEmployerRate,
            BpjsKetenagakerjaanJhtEmployeeRate = entity.BpjsKetenagakerjaanJhtEmployeeRate,
            BpjsKetenagakerjaanJhtEmployerRate = entity.BpjsKetenagakerjaanJhtEmployerRate,
            BpjsKetenagakerjaanJkkRate = entity.BpjsKetenagakerjaanJkkRate,
            BpjsKetenagakerjaanJkmRate = entity.BpjsKetenagakerjaanJkmRate,
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
