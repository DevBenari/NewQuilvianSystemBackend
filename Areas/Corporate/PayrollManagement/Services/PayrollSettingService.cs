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
        private readonly PayrollPolicyOptions _policy;

        public PayrollSettingService(
            ApplicationDbContext context,
            ILogger<PayrollSettingService> logger,
            IOptions<PayrollPolicyOptions> policyOptions)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _policy = policyOptions?.Value ?? new PayrollPolicyOptions();
        }

        public async Task<HrdPayrollSettingDto> GetSettingAsync(CancellationToken cancellationToken = default)
        {
            var setting = await GetOrCreateDefaultSettingAsync(cancellationToken);

            var dto = MapToDto(setting);
            await ApplyPolicyStatusAsync(dto, cancellationToken);
            return dto;
        }

        /// <summary>
        /// Menandai status verifikasi nilai BPJS: tanpa kebijakan iuran BPJS yang disahkan dan berlaku hari ini, nilai pada pengaturan
        /// ini berstatus "Menunggu Verifikasi Kebijakan" dan tidak dipakai payroll. Murni baca.
        /// </summary>
        private async Task ApplyPolicyStatusAsync(HrdPayrollSettingDto dto, CancellationToken cancellationToken)
        {
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            var bpjs = await _context.Set<PrlBpjsPolicy>().AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.PolicyStatus == StatutoryPolicyStatuses.Approved
                    && x.EffectiveStartDate <= today && (x.EffectiveEndDate == null || x.EffectiveEndDate >= today))
                .OrderByDescending(x => x.EffectiveStartDate)
                .Select(x => new { x.Id, x.PolicyVersion })
                .FirstOrDefaultAsync(cancellationToken);
            var tax = await _context.Set<PrlTaxPolicy>().AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.PolicyStatus == StatutoryPolicyStatuses.Approved
                    && x.EffectiveStartDate <= today && (x.EffectiveEndDate == null || x.EffectiveEndDate >= today))
                .OrderByDescending(x => x.EffectiveStartDate)
                .Select(x => x.PolicyVersion)
                .FirstOrDefaultAsync(cancellationToken);

            dto.BpjsPolicyApproved = bpjs is not null;
            dto.BpjsPolicyId = bpjs?.Id;
            dto.BpjsPolicyVersion = bpjs?.PolicyVersion;
            if (!_policy.EnableBpjsInPayroll)
            {
                dto.BpjsValuesStatus = PayrollConfigurationStatuses.TestingModeDisabled;
                dto.BpjsValuesStatusLabel = StatutoryPolicyLabels.BpjsTestingModeDisabled;
            }
            else
            {
                dto.BpjsValuesStatus = bpjs is null ? "PENDING_VERIFICATION" : "REPLACED_BY_APPROVED_POLICY";
                dto.BpjsValuesStatusLabel = bpjs is null
                    ? StatutoryPolicyLabels.AwaitingVerification
                    : $"Tidak dipakai payroll; tarif resmi mengikuti kebijakan {bpjs.PolicyVersion}";
            }
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

            // Parameter BPJS hanya diubah saat fitur BPJS aktif. Saat nonaktif, Frontend tidak lagi menampilkan atau mengirimnya, sehingga
            // command membawa nilai bawaan DTO; menimpa kolom dengan nilai itu akan mereset tarif dan plafon yang sudah tersimpan. Nilai
            // tersimpan dipertahankan apa adanya (non-destruktif) dan dipakai kembali bila BPJS diaktifkan lagi.
            if (_policy.EnableBpjsInPayroll)
            {
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
            }
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
