using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public class PayrollRunInitializationService : IPayrollRunInitializationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPayrollSettingService _settingService;
        private readonly IPayrollPreconditionGateService _preconditionGateService;
        private readonly ILogger<PayrollRunInitializationService> _logger;

        public PayrollRunInitializationService(
            ApplicationDbContext context,
            IPayrollSettingService settingService,
            IPayrollPreconditionGateService preconditionGateService,
            ILogger<PayrollRunInitializationService> logger)
        {
            _context = context;
            _settingService = settingService;
            _preconditionGateService = preconditionGateService;
            _logger = logger;
        }

        public async Task<PayrollServiceResult<PayrollRunOperationResultDto>> InitializePayrollRunAsync(
            CreatePayrollRunCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Memulai inisiasi batch siklus penggajian untuk periode {Year}-{Month:D2} (Tipe: {RunType}). Actor: {ActorId}",
                command.PeriodYear, command.PeriodMonth, command.RunType, actorUserId);

            // 1. Validasi Precondition Gate (Kecuali eksplisit di-bypass dengan otorisasi)
            if (!command.BypassPreconditions)
            {
                var gateCheck = await _preconditionGateService.CheckPreconditionsAsync(
                    command.PeriodYear, command.PeriodMonth, cancellationToken);

                if (!gateCheck.CanInitialize)
                {
                    _logger.LogWarning(
                        "Inisiasi batch penggajian ditolak oleh Precondition Gate: {Message}",
                        gateCheck.Message);

                    return PayrollServiceResult<PayrollRunOperationResultDto>.Fail(
                        StatusCodes.Status422UnprocessableEntity,
                        $"Precondition Gate Gagal: {gateCheck.Message}");
                }
            }

            // 2. Proteksi Anti-Duplikasi Periode Penggajian (VR-PAY-01)
            var existingActiveRun = await _context.TrxPayrollRuns
                .Include(r => r.PayrollPeriod)
                .FirstOrDefaultAsync(r => r.IsActive
                    && r.RunType == command.RunType
                    && r.PayrollPeriod != null
                    && r.PayrollPeriod.FiscalYear == command.PeriodYear
                    && r.PayrollPeriod.PeriodNumber == command.PeriodMonth
                    && r.RunStatus != "Cancelled"
                    && r.RunStatus != "Reversed", cancellationToken);

            if (existingActiveRun != null)
            {
                _logger.LogWarning(
                    "Duplikasi terdeteksi: Batch penggajian untuk periode {Year}-{Month:D2} tipe {RunType} sudah ada dengan nomor {RunNumber} (Status: {RunStatus})",
                    command.PeriodYear, command.PeriodMonth, command.RunType,
                    existingActiveRun.RunNumber, existingActiveRun.RunStatus);

                return PayrollServiceResult<PayrollRunOperationResultDto>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Batch penggajian untuk periode {command.PeriodYear}-{command.PeriodMonth:D2} dengan tipe '{command.RunType}' sudah aktif dengan nomor {existingActiveRun.RunNumber} (Status: {existingActiveRun.RunStatus}). (VR-PAY-01)");
            }

            // 3. Cari atau Inisiasi MstPayrollPeriod untuk Periode Tersebut
            var period = await _context.MstPayrollPeriods
                .FirstOrDefaultAsync(p => p.FiscalYear == command.PeriodYear && p.PeriodNumber == command.PeriodMonth, cancellationToken);

            if (period == null)
            {
                var prevMonthDate = new DateTime(command.PeriodYear, command.PeriodMonth, 1).AddMonths(-1);
                var startDate = new DateTime(prevMonthDate.Year, prevMonthDate.Month, 21);
                var endDate = new DateTime(command.PeriodYear, command.PeriodMonth, 20);
                var paymentDate = new DateTime(command.PeriodYear, command.PeriodMonth, 25);
                var cutoffDate = endDate;

                period = new MstPayrollPeriod
                {
                    Id = Guid.NewGuid(),
                    PayrollPeriodCode = $"PRD-{command.PeriodYear}{command.PeriodMonth:D2}",
                    PayrollPeriodName = $"Periode Penggajian {new DateTime(command.PeriodYear, command.PeriodMonth, 1):MMMM yyyy}",
                    PeriodType = "Monthly",
                    FiscalYear = command.PeriodYear,
                    PeriodNumber = command.PeriodMonth,
                    StartDate = startDate,
                    EndDate = endDate,
                    AttendanceCutoffDate = cutoffDate,
                    VariableInputCutoffDate = cutoffDate,
                    PaymentDate = paymentDate,
                    PayrollPeriodStatus = "Processing",
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = DateTime.UtcNow
                };

                _context.MstPayrollPeriods.Add(period);
            }

            // 4. Ambil Snapshot Pengaturan Penggajian Aktif
            var settingSnapshotJson = await _settingService.GetSettingSnapshotJsonAsync(cancellationToken);

            // 5. Generate Nomor Batch Unik Resmi (Format: PRUN-YYYYMM-XXX)
            var currentMonthRunsCount = await _context.TrxPayrollRuns
                .CountAsync(r => r.CreateDateTime.Year == command.PeriodYear
                              && r.CreateDateTime.Month == command.PeriodMonth, cancellationToken);

            var runNumber = $"PRUN-{command.PeriodYear}{command.PeriodMonth:D2}-{(currentMonthRunsCount + 1):D3}";

            // 6. Buat Entitas Master TrxPayrollRun
            var payrollRun = new TrxPayrollRun
            {
                Id = Guid.NewGuid(),
                PayrollPeriodId = period.Id,
                RunNumber = runNumber,
                RunType = command.RunType,
                RunStatus = "Draft",
                CurrencyCode = "IDR",
                PeriodStartDateSnapshot = period.StartDate,
                PeriodEndDateSnapshot = period.EndDate,
                AttendanceCutoffDateSnapshot = period.AttendanceCutoffDate,
                VariableInputCutoffDateSnapshot = period.VariableInputCutoffDate,
                PaymentDateSnapshot = period.PaymentDate,
                ConfigurationSnapshotJson = settingSnapshotJson,
                Notes = command.Notes,
                IsLocked = false,
                IsActive = true,
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };

            // 7. Populasi Daftar Pegawai Aktif Rumah Sakit
            var activeProfiles = await _context.MstWorkforceProfiles
                .Include(p => p.PrimaryDepartment)
                .Include(p => p.PrimaryPosition)
                .Where(p => p.IsActive)
                .ToListAsync(cancellationToken);

            var profileIds = activeProfiles.Select(p => p.Id).ToList();

            var activeSalaryAssignments = await _context.WfpSalaryAssignments
                .Where(s => profileIds.Contains(s.WorkforceProfileId)
                    && s.IsActive
                    && s.EffectiveStartDate <= period.EndDate
                    && (s.EffectiveEndDate == null || s.EffectiveEndDate >= period.StartDate))
                .OrderByDescending(s => s.EffectiveStartDate)
                .ToListAsync(cancellationToken);

            var salaryMap = activeSalaryAssignments
                .GroupBy(s => s.WorkforceProfileId)
                .ToDictionary(g => g.Key, g => g.First());

            var runEmployees = new List<TrxPayrollRunEmployee>();
            foreach (var profile in activeProfiles)
            {
                salaryMap.TryGetValue(profile.Id, out var salaryAss);

                var runEmp = new TrxPayrollRunEmployee
                {
                    Id = Guid.NewGuid(),
                    PayrollRunId = payrollRun.Id,
                    WorkforceProfileId = profile.Id,
                    EmployeePayrollStatus = "Pending",
                    CurrencyCode = "IDR",
                    EmployeeNumberSnapshot = profile.ProfileCode,
                    EmployeeNameSnapshot = profile.DisplayName,
                    DepartmentNameSnapshot = profile.PrimaryDepartment?.DepartmentName,
                    PositionNameSnapshot = profile.PrimaryPosition?.PositionName,
                    SalaryStructureId = salaryAss?.SalaryStructureId,
                    SalaryGradeId = salaryAss?.SalaryGradeId,
                    SalaryAssignmentId = salaryAss?.Id,
                    BaseSalary = salaryAss?.BaseSalary ?? 0m,
                    TotalRecurringEarning = 0m,
                    GrossPay = salaryAss?.BaseSalary ?? 0m,
                    NetPay = salaryAss?.BaseSalary ?? 0m,
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = DateTime.UtcNow
                };

                runEmployees.Add(runEmp);
            }

            payrollRun.TotalEmployeeCount = runEmployees.Count;
            payrollRun.TotalBaseSalary = runEmployees.Sum(e => e.BaseSalary);
            payrollRun.TotalGrossPay = runEmployees.Sum(e => e.GrossPay);
            payrollRun.TotalNetPay = runEmployees.Sum(e => e.NetPay);

            _context.TrxPayrollRuns.Add(payrollRun);
            _context.TrxPayrollRunEmployees.AddRange(runEmployees);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Berhasil menginisiasi batch penggajian {RunNumber} dengan {Total} pegawai aktif.",
                payrollRun.RunNumber, runEmployees.Count);

            return PayrollServiceResult<PayrollRunOperationResultDto>.Ok(
                new PayrollRunOperationResultDto
                {
                    RunId = payrollRun.Id,
                    RunNumber = payrollRun.RunNumber,
                    RunStatus = payrollRun.RunStatus,
                    TotalEmployees = runEmployees.Count,
                    Message = $"Batch penggajian {payrollRun.RunNumber} berhasil diinisiasi dengan status Draft untuk {runEmployees.Count} pegawai."
                },
                "Batch penggajian berhasil dibuat.",
                StatusCodes.Status201Created);
        }

        public async Task<PayrollRunDetailDto?> GetPayrollRunDetailAsync(
            Guid runId,
            CancellationToken cancellationToken = default)
        {
            var run = await _context.TrxPayrollRuns
                .Include(r => r.PayrollPeriod)
                .Include(r => r.Employees.Where(e => e.IsActive))
                .FirstOrDefaultAsync(r => r.Id == runId && r.IsActive, cancellationToken);

            if (run == null) return null;

            return new PayrollRunDetailDto
            {
                Id = run.Id,
                RunNumber = run.RunNumber,
                PeriodYear = run.PayrollPeriod?.FiscalYear ?? run.CreateDateTime.Year,
                PeriodMonth = run.PayrollPeriod?.PeriodNumber ?? run.CreateDateTime.Month,
                PeriodName = run.PayrollPeriod?.PayrollPeriodName ?? "Periode Gaji",
                RunType = run.RunType,
                RunStatus = run.RunStatus,
                PeriodStartDateSnapshot = run.PeriodStartDateSnapshot,
                PeriodEndDateSnapshot = run.PeriodEndDateSnapshot,
                AttendanceCutoffDateSnapshot = run.AttendanceCutoffDateSnapshot,
                PaymentDateSnapshot = run.PaymentDateSnapshot,
                TotalEmployeeCount = run.TotalEmployeeCount,
                ProcessedEmployeeCount = run.ProcessedEmployeeCount,
                TotalBaseSalary = run.TotalBaseSalary,
                TotalEarning = run.TotalEarning,
                TotalDeduction = run.TotalDeduction,
                TotalGrossPay = run.TotalGrossPay,
                TotalNetPay = run.TotalNetPay,
                IsLocked = run.IsLocked,
                ConfigurationSnapshotJson = run.ConfigurationSnapshotJson,
                Notes = run.Notes,
                CreatedAt = run.CreateDateTime,
                Employees = run.Employees.Select(e => new PayrollRunEmployeeItemDto
                {
                    Id = e.Id,
                    WorkforceProfileId = e.WorkforceProfileId,
                    EmployeeName = e.EmployeeNameSnapshot,
                    DepartmentName = e.DepartmentNameSnapshot ?? "-",
                    PositionName = e.PositionNameSnapshot ?? "-",
                    PayrollStatus = e.EmployeePayrollStatus,
                    BaseSalary = e.BaseSalary,
                    RecurringEarning = e.TotalRecurringEarning,
                    GrossPay = e.GrossPay,
                    TotalDeduction = e.TotalDeduction,
                    NetPay = e.NetPay,
                    HasVarianceAlert = false
                }).ToList()
            };
        }

        public async Task<List<PayrollRunSummaryDto>> GetAllPayrollRunsAsync(
            CancellationToken cancellationToken = default)
        {
            var runs = await _context.TrxPayrollRuns
                .Include(r => r.PayrollPeriod)
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.CreateDateTime)
                .ToListAsync(cancellationToken);

            return runs.Select(r => new PayrollRunSummaryDto
            {
                Id = r.Id,
                RunNumber = r.RunNumber,
                PeriodYear = r.PayrollPeriod?.FiscalYear ?? r.CreateDateTime.Year,
                PeriodMonth = r.PayrollPeriod?.PeriodNumber ?? r.CreateDateTime.Month,
                RunType = r.RunType,
                RunStatus = r.RunStatus,
                TotalEmployeeCount = r.TotalEmployeeCount,
                ProcessedEmployeeCount = r.ProcessedEmployeeCount,
                TotalGrossPay = r.TotalGrossPay,
                TotalDeduction = r.TotalDeduction,
                TotalNetPay = r.TotalNetPay,
                CalculatedAt = r.CalculatedAt,
                ApprovedAt = r.ApprovedAt,
                CreatedAt = r.CreateDateTime
            }).ToList();
        }
    }
}
