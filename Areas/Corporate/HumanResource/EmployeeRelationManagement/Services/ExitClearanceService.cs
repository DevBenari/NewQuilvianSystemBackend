using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public class ExitClearanceService : IExitClearanceService, IExitClearanceStatusProvider
    {
        private readonly IEmployeeRelationUnitOfWork _uow;

        public ExitClearanceService(IEmployeeRelationUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<ExitClearanceProcessDetailDto?> GetClearanceBySeparationIdAsync(Guid separationId, CancellationToken cancellationToken = default)
        {
            var process = await _uow.ExitClearances
                .AsNoTracking()
                .Include(x => x.Checklists)
                .FirstOrDefaultAsync(x => x.SeparationId == separationId, cancellationToken);

            return process == null ? null : MapToDetailDto(process);
        }

        public async Task<ExitClearanceChecklistDto> SignoffChecklistAsync(Guid checklistId, SignoffChecklistRequestDto request, string actorEmail, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var item = await _uow.ExitClearanceChecklists
                .Include(x => x.ClearanceProcess)
                .FirstOrDefaultAsync(x => x.ChecklistId == checklistId, cancellationToken);

            if (item == null)
            {
                throw new KeyNotFoundException($"Item checklist dengan ID {checklistId} tidak ditemukan.");
            }

            item.IsReturned = request.IsReturned;
            item.OutstandingLiabilityAmount = request.OutstandingLiabilityAmount;
            item.ApproverNotes = request.ApproverNotes?.Trim();
            item.ApprovedByUserId = request.ApproverUserId;
            item.ApprovedAt = DateTime.UtcNow;

            if (request.IsReturned && request.OutstandingLiabilityAmount <= 0)
            {
                item.Status = "Approved";
            }
            else
            {
                item.Status = "RejectedOutstanding";
            }
            item.UpdateDateTime = DateTime.UtcNow;

            await _uow.SaveChangesAsync(cancellationToken);

            return MapChecklistDto(item);
        }

        public async Task<ExitClearanceProcessDetailDto> FinalizeExitClearanceAsync(Guid clearanceProcessId, FinalizeExitClearanceRequestDto request, string actorEmail, CancellationToken cancellationToken = default)
        {
            var process = await _uow.ExitClearances
                .Include(x => x.Checklists)
                .FirstOrDefaultAsync(x => x.ClearanceProcessId == clearanceProcessId, cancellationToken);

            if (process == null)
            {
                throw new KeyNotFoundException($"Berkas exit clearance dengan ID {clearanceProcessId} tidak ditemukan.");
            }

            // VR-EMP-24: Seluruh item checklist dari 5 departemen wajib berstatus Approved
            var pendingOrRejected = process.Checklists.Where(x => x.Status != "Approved").ToList();
            if (pendingOrRejected.Any())
            {
                var pendingDeps = string.Join(", ", pendingOrRejected.Select(x => x.DepartmentType).Distinct());
                throw new InvalidOperationException($"Proses exit clearance belum dapat diselesaikan karena masih ada checklist yang belum disetujui pada departemen: {pendingDeps} (VR-EMP-24).");
            }

            // Validasi sisa tanggungan ganti rugi
            var totalLiability = process.Checklists.Sum(x => x.OutstandingLiabilityAmount);
            if (totalLiability > 0)
            {
                throw new InvalidOperationException($"Proses exit clearance belum dapat diselesaikan karena masih ada tanggungan biaya sebesar Rp {totalLiability:N0} (VR-EMP-24).");
            }

            process.OverallClearanceStatus = "Completed";
            process.ActualCompletionDate = DateTime.UtcNow;
            // VR-EMP-25: Buka gembok rilis gaji akhir (INT-HR-87)
            process.IsFinalPayrollUnlocked = true;
            process.FinalizationNotes = request?.Notes?.Trim() ?? "Seluruh 5 unit kerja telah menyetujui serah terima inventaris tanpa sisa tanggungan.";
            process.UpdateDateTime = DateTime.UtcNow;

            // Update status separation
            var separation = await _uow.Separations.FirstOrDefaultAsync(x => x.Id == process.SeparationId, cancellationToken);
            if (separation != null)
            {
                separation.IsExitClearanceCompleted = true;
                separation.SeparationStatus = "FinalizedCompleted";
                separation.UpdateDateTime = DateTime.UtcNow;
            }

            await _uow.SaveChangesAsync(cancellationToken);
            return MapToDetailDto(process);
        }

        public async Task<PayrollLockStatusResponseDto> GetPayrollLockStatusAsync(Guid employeeId, CancellationToken cancellationToken = default)
        {
            var process = await _uow.ExitClearances
                .AsNoTracking()
                .Include(x => x.Checklists)
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.InitiatedDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (process == null)
            {
                return new PayrollLockStatusResponseDto
                {
                    EmployeeId = employeeId,
                    IsFinalPayrollUnlocked = true, // Pegawai aktif normal tidak memiliki gembok pemutusan kerja
                    OverallStatus = "ActiveRegular",
                    Message = "Pegawai tidak berada dalam proses terminasi kerja."
                };
            }

            var pendingDeps = process.Checklists
                .Where(x => x.Status != "Approved")
                .Select(x => x.DepartmentType)
                .Distinct()
                .ToList();

            var totalOutstanding = process.Checklists.Sum(x => x.OutstandingLiabilityAmount);

            return new PayrollLockStatusResponseDto
            {
                EmployeeId = employeeId,
                IsFinalPayrollUnlocked = process.IsFinalPayrollUnlocked,
                OverallStatus = process.OverallClearanceStatus,
                PendingDepartments = pendingDeps,
                TotalOutstandingLiability = totalOutstanding,
                Message = process.IsFinalPayrollUnlocked
                    ? "Exit clearance selesai 100%. Gembok rilis payroll akhir telah dibuka."
                    : $"Hard-Block Keamanan INT-HR-87: Payroll akhir terkunci. Masih ada {pendingDeps.Count} departemen pending/ganti rugi Rp {totalOutstanding:N0}."
            };
        }

        public async Task<bool> IsFinalPayrollUnlockedAsync(Guid employeeId, CancellationToken cancellationToken = default)
        {
            var status = await GetPayrollLockStatusAsync(employeeId, cancellationToken);
            return status.IsFinalPayrollUnlocked;
        }

        public async Task<List<string>> GetPendingClearanceDepartmentsAsync(Guid employeeId, CancellationToken cancellationToken = default)
        {
            var status = await GetPayrollLockStatusAsync(employeeId, cancellationToken);
            return status.PendingDepartments;
        }

        private static ExitClearanceProcessDetailDto MapToDetailDto(TrxExitClearanceProcess entity)
        {
            return new ExitClearanceProcessDetailDto
            {
                ClearanceProcessId = entity.ClearanceProcessId,
                SeparationId = entity.SeparationId,
                EmployeeId = entity.EmployeeId,
                InitiatedDate = entity.InitiatedDate,
                TargetCompletionDate = entity.TargetCompletionDate,
                ActualCompletionDate = entity.ActualCompletionDate,
                OverallClearanceStatus = entity.OverallClearanceStatus,
                IsFinalPayrollUnlocked = entity.IsFinalPayrollUnlocked,
                FinalizationNotes = entity.FinalizationNotes,
                Checklists = entity.Checklists.Select(c => MapChecklistDto(c)).ToList()
            };
        }

        private static ExitClearanceChecklistDto MapChecklistDto(TrxExitClearanceChecklist entity)
        {
            return new ExitClearanceChecklistDto
            {
                ChecklistId = entity.ChecklistId,
                ClearanceProcessId = entity.ClearanceProcessId,
                DepartmentType = entity.DepartmentType,
                ItemName = entity.ItemName,
                IsReturned = entity.IsReturned,
                OutstandingLiabilityAmount = entity.OutstandingLiabilityAmount,
                ApproverNotes = entity.ApproverNotes,
                ApprovedByUserId = entity.ApprovedByUserId,
                ApprovedAt = entity.ApprovedAt,
                Status = entity.Status
            };
        }
    }
}
