using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    public class TrainingBudgetControlService : ITrainingBudgetControlService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TrainingBudgetControlService> _logger;

        public TrainingBudgetControlService(
            ApplicationDbContext context,
            ILogger<TrainingBudgetControlService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TrainingBudgetSummaryDto?> GetDepartmentBudgetStatusAsync(
            Guid departmentId,
            int? fiscalYear,
            CancellationToken cancellationToken = default)
        {
            var year = fiscalYear ?? DateTime.UtcNow.Year;

            var budget = await _context.TrxTrainingBudgets
                .AsNoTracking()
                .Include(b => b.Department)
                .Include(b => b.TrainingPlan)
                .FirstOrDefaultAsync(b => b.DepartmentId == departmentId && b.FiscalYear == year && !b.IsDelete, cancellationToken);

            return budget == null ? null : MapToDto(budget);
        }

        public async Task<List<TrainingBudgetSummaryDto>> GetPlanBudgetsAsync(
            Guid trainingPlanId,
            CancellationToken cancellationToken = default)
        {
            var budgets = await _context.TrxTrainingBudgets
                .AsNoTracking()
                .Include(b => b.Department)
                .Include(b => b.TrainingPlan)
                .Where(b => b.TrainingPlanId == trainingPlanId && !b.IsDelete)
                .OrderBy(b => b.FiscalYear)
                .ToListAsync(cancellationToken);

            return budgets.Select(MapToDto).ToList();
        }

        public async Task<TrainingBudgetSummaryDto> AllocateBudgetAsync(
            AllocateDepartmentBudgetRequestDto request,
            string allocatedBy,
            CancellationToken cancellationToken = default)
        {
            var existingBudget = await _context.TrxTrainingBudgets
                .FirstOrDefaultAsync(b =>
                    b.TrainingPlanId == request.TrainingPlanId &&
                    b.DepartmentId == request.DepartmentId &&
                    b.FiscalYear == request.FiscalYear &&
                    !b.IsDelete, cancellationToken);

            if (existingBudget != null)
            {
                // Perbarui alokasi yang sudah ada
                existingBudget.AllocatedAmount = request.AllocatedAmount;
                existingBudget.RemainingAmount = existingBudget.AllocatedAmount - existingBudget.CommittedAmount - existingBudget.UsedAmount;
                existingBudget.BudgetNote = request.BudgetNote ?? existingBudget.BudgetNote;
                existingBudget.UpdateDateTime = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);
                return MapToDto(existingBudget);
            }

            var entity = new TrxTrainingBudget
            {
                Id = Guid.NewGuid(),
                TrainingPlanId = request.TrainingPlanId,
                DepartmentId = request.DepartmentId,
                LegalEntityId = request.LegalEntityId,
                HospitalSiteId = request.HospitalSiteId,
                CostCenterId = request.CostCenterId,
                FiscalYear = request.FiscalYear,
                BudgetType = "TrainingPlan",
                BudgetStatus = "Approved",
                CurrencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode) ? "IDR" : request.CurrencyCode,
                AllocatedAmount = request.AllocatedAmount,
                CommittedAmount = 0m,
                UsedAmount = 0m,
                RemainingAmount = request.AllocatedAmount,
                ApprovedAt = DateTime.UtcNow,
                BudgetNote = request.BudgetNote,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow
            };

            _context.TrxTrainingBudgets.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Pagu anggaran diklat dialokasikan untuk Departemen {DepartmentId} tahun {FiscalYear}: Rp {AllocatedAmount:N0} oleh {AllocatedBy}",
                request.DepartmentId, request.FiscalYear, request.AllocatedAmount, allocatedBy);

            return MapToDto(entity);
        }

        public async Task<DepartmentBudgetValidationResultDto> ValidateAndCommitBudgetAsync(
            CommitTrainingBudgetRequestDto request,
            string committedBy,
            CancellationToken cancellationToken = default)
        {
            var currentYear = DateTime.UtcNow.Year;

            // Cari pagu anggaran departemen untuk tahun fiskal berjalan
            var budget = await _context.TrxTrainingBudgets
                .FirstOrDefaultAsync(b =>
                    b.DepartmentId == request.DepartmentId &&
                    b.FiscalYear == currentYear &&
                    !b.IsDelete, cancellationToken);

            if (budget == null)
            {
                // Jika belum ada pagu terinci departemen, periksa pagu master rencana
                budget = await _context.TrxTrainingBudgets
                    .FirstOrDefaultAsync(b => b.TrainingPlanId == request.TrainingPlanId && !b.IsDelete, cancellationToken);
            }

            if (budget == null)
            {
                throw new KeyNotFoundException($"Pagu anggaran diklat untuk Departemen {request.DepartmentId} tahun {currentYear} belum dialokasikan.");
            }

            // Validasi VR-TRN-14: Pagu tidak boleh defisit
            if (budget.RemainingAmount < request.CommitmentAmount)
            {
                _logger.LogWarning(
                    "Pendaftaran diklat ditolak: Sisa pagu anggaran Departemen {DepartmentId} (Rp {Remaining:N0}) tidak mencukupi komitmen Rp {Requested:N0} (VR-TRN-14).",
                    request.DepartmentId, budget.RemainingAmount, request.CommitmentAmount);

                return new DepartmentBudgetValidationResultDto
                {
                    IsEligible = false,
                    AllocatedAmount = budget.AllocatedAmount,
                    CommittedAmount = budget.CommittedAmount,
                    RemainingAmount = budget.RemainingAmount,
                    RequestedAmount = request.CommitmentAmount,
                    ProjectedRemainingAmount = budget.RemainingAmount - request.CommitmentAmount,
                    StatusMessage = $"Sisa pagu anggaran diklat departemen tidak mencukupi untuk membiayai pelatihan yang diajukan (VR-TRN-14). Sisa pagu: Rp {budget.RemainingAmount:N0}, dibutuhkan: Rp {request.CommitmentAmount:N0}."
                };
            }

            // Validasi VR-TRN-15: Pemindahan saldo dari Remaining ke Committed
            budget.CommittedAmount += request.CommitmentAmount;
            budget.RemainingAmount -= request.CommitmentAmount;
            budget.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Komitmen pagu anggaran diklat berhasil dibukukan: Rp {Amount:N0} untuk permohonan {EnrollmentId}. Sisa pagu: Rp {Remaining:N0} (VR-TRN-15).",
                request.CommitmentAmount, request.EnrollmentRequestId, budget.RemainingAmount);

            return new DepartmentBudgetValidationResultDto
            {
                IsEligible = true,
                AllocatedAmount = budget.AllocatedAmount,
                CommittedAmount = budget.CommittedAmount,
                RemainingAmount = budget.RemainingAmount,
                RequestedAmount = request.CommitmentAmount,
                ProjectedRemainingAmount = budget.RemainingAmount,
                StatusMessage = "Pagu anggaran mencukupi dan komitmen biaya berhasil dialokasikan secara otomatis (VR-TRN-15)."
            };
        }

        public async Task<TrainingBudgetSummaryDto> RealizeBudgetAsync(
            RealizeTrainingBudgetRequestDto request,
            string realizedBy,
            CancellationToken cancellationToken = default)
        {
            var budget = await _context.TrxTrainingBudgets
                .Include(b => b.Department)
                .Include(b => b.TrainingPlan)
                .FirstOrDefaultAsync(b => b.Id == request.BudgetId && !b.IsDelete, cancellationToken);

            if (budget == null)
            {
                throw new KeyNotFoundException($"Data anggaran dengan ID {request.BudgetId} tidak ditemukan.");
            }

            var releasedCommitment = request.ReleasedCommitmentAmount > 0 ? request.ReleasedCommitmentAmount : request.RealizedAmount;

            budget.CommittedAmount = Math.Max(0m, budget.CommittedAmount - releasedCommitment);
            budget.UsedAmount += request.RealizedAmount;
            budget.RemainingAmount = Math.Max(0m, budget.AllocatedAmount - budget.CommittedAmount - budget.UsedAmount);

            if (!string.IsNullOrWhiteSpace(request.RealizationNote))
            {
                budget.BudgetNote = string.IsNullOrWhiteSpace(budget.BudgetNote)
                    ? $"Realisasi: {request.RealizationNote}"
                    : $"{budget.BudgetNote} | Realisasi: {request.RealizationNote}";
            }

            budget.UpdateDateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Realisasi biaya diklat dicairkan: Rp {Realized:N0} oleh {RealizedBy}. Total realisasi: Rp {Used:N0}, Sisa pagu: Rp {Remaining:N0}.",
                request.RealizedAmount, realizedBy, budget.UsedAmount, budget.RemainingAmount);

            return MapToDto(budget);
        }

        private static TrainingBudgetSummaryDto MapToDto(TrxTrainingBudget entity)
        {
            return new TrainingBudgetSummaryDto
            {
                Id = entity.Id,
                TrainingPlanId = entity.TrainingPlanId,
                TrainingPlanName = entity.TrainingPlan?.TrainingPlanName,
                DepartmentId = entity.DepartmentId,
                DepartmentName = entity.Department?.DepartmentName,
                FiscalYear = entity.FiscalYear,
                BudgetType = entity.BudgetType,
                BudgetStatus = entity.BudgetStatus,
                CurrencyCode = entity.CurrencyCode,
                AllocatedAmount = entity.AllocatedAmount,
                CommittedAmount = entity.CommittedAmount,
                UsedAmount = entity.UsedAmount,
                RemainingAmount = entity.RemainingAmount,
                ApprovedAt = entity.ApprovedAt,
                BudgetNote = entity.BudgetNote,
                IsActive = entity.IsActive
            };
        }
    }
}
