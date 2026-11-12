using QuilvianSystemBackend.Responses;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public class EmployeeSeparationService : IEmployeeSeparationService
    {
        private readonly IEmployeeRelationUnitOfWork _uow;
        private readonly NumberSeriesAllocator _numberAllocator;

        public EmployeeSeparationService(IEmployeeRelationUnitOfWork uow, NumberSeriesAllocator numberAllocator)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _numberAllocator = numberAllocator ?? throw new ArgumentNullException(nameof(numberAllocator));
        }

        public async Task<SeparationResponseDto> InitiateSeparationAsync(InitiateSeparationRequestDto request, string actorEmail, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // VR-EMP-19: Notice resign sukarela minimal 30 hari sebelum effective date
            if (string.Equals(request.SeparationReason, "VoluntaryResign", StringComparison.OrdinalIgnoreCase))
            {
                var noticeDays = (request.EffectiveDate.Date - request.NoticeDate.Date).TotalDays;
                if (noticeDays < 30)
                {
                    throw new InvalidOperationException("Pemberitahuan pengunduran diri sukarela wajib memenuhi ketentuan One Month Notice (minimal 30 hari kalender) sesuai aturan VR-EMP-19.");
                }
            }

            // Nomor urut dari provider bersama, bukan angka acak (QBE-CODE-003). Bentuk: SEP-202611-0001
            var separationNumber = await _numberAllocator.AllocateAsync(new NumberAllocationRequest(
                SequenceKey: EmployeeRelationNumberSeries.Keys.Separation,
                Prefix: EmployeeRelationNumberSeries.Prefixes.Separation,
                ResetPolicy: EmployeeRelationNumberSeries.ResetPolicies.Separation,
                SequenceDigits: EmployeeRelationNumberSeries.SequenceDigits,
                ActorUserId: await ResolveActorUserIdAsync(actorEmail, cancellationToken),
                Instant: DateTimeOffset.UtcNow), cancellationToken);

            var separation = new TrxEmployeeSeparation
            {
                Id = Guid.NewGuid(),
                SeparationNumber = separationNumber,
                EmployeeId = request.EmployeeId,
                WorkforceProfileId = request.EmployeeId,
                SeparationType = request.SeparationReason.Trim(),
                RequestDate = request.NoticeDate,
                NoticeDate = request.NoticeDate,
                EffectiveSeparationDate = request.EffectiveDate,
                ResignationLetterUrl = request.ResignationLetterUrl?.Trim(),
                ApprovalStatus = "Pending",
                SeparationStatus = "Initiated",
                IsExitClearanceCompleted = false,
                IsFinalPayrollCompleted = false,
                CreateDateTime = DateTime.UtcNow
            };

            await _uow.AddSeparationAsync(separation, cancellationToken);

            // Inisiasi Exit Clearance Process otomatis untuk 5 Departemen (VR-EMP-24)
            var clearance = new TrxExitClearanceProcess
            {
                ClearanceProcessId = Guid.NewGuid(),
                SeparationId = separation.Id,
                EmployeeId = request.EmployeeId,
                InitiatedDate = DateTime.UtcNow,
                TargetCompletionDate = request.EffectiveDate,
                OverallClearanceStatus = "InProgress",
                IsFinalPayrollUnlocked = false,
                CreateDateTime = DateTime.UtcNow
            };

            var standardChecklists = new List<TrxExitClearanceChecklist>
            {
                new() { ChecklistId = Guid.NewGuid(), ClearanceProcessId = clearance.ClearanceProcessId, DepartmentType = "WorkUnitHandover", ItemName = "Serah Terima Tugas, Kunci Ruangan & Dokumen Kerja Unit", IsReturned = false, Status = "Pending", CreateDateTime = DateTime.UtcNow },
                new() { ChecklistId = Guid.NewGuid(), ClearanceProcessId = clearance.ClearanceProcessId, DepartmentType = "GeneralAffairsAsset", ItemName = "Pengembalian ID Card, Seragam Kerja & Kendaraan Operasional", IsReturned = false, Status = "Pending", CreateDateTime = DateTime.UtcNow },
                new() { ChecklistId = Guid.NewGuid(), ClearanceProcessId = clearance.ClearanceProcessId, DepartmentType = "PharmacyMedicalDevice", ItemName = "Kliring Resep/Obat Pribadi & Pengembalian Alkes Inventaris", IsReturned = false, Status = "Pending", CreateDateTime = DateTime.UtcNow },
                new() { ChecklistId = Guid.NewGuid(), ClearanceProcessId = clearance.ClearanceProcessId, DepartmentType = "FinanceCooperative", ItemName = "Penyelesaian Sisa Pinjaman Koperasi & Kasbon Karyawan", IsReturned = false, Status = "Pending", CreateDateTime = DateTime.UtcNow },
                new() { ChecklistId = Guid.NewGuid(), ClearanceProcessId = clearance.ClearanceProcessId, DepartmentType = "InformationTechnology", ItemName = "Penonaktifan Akun SSO SIMRS, Email Korporat & Pengembalian Laptop", IsReturned = false, Status = "Pending", CreateDateTime = DateTime.UtcNow }
            };

            await _uow.AddExitClearanceProcessAsync(clearance, cancellationToken);
            await _uow.AddExitClearanceChecklistsRangeAsync(standardChecklists, cancellationToken);

            await _uow.SaveChangesAsync(cancellationToken);

            return MapToDto(separation);
        }

        public async Task<PagedResult<SeparationResponseDto>> GetSeparationsAsync(string? status, string? separationReason, Guid? departmentId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _uow.Separations.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.SeparationStatus == status.Trim());
            }

            if (!string.IsNullOrWhiteSpace(separationReason))
            {
                query = query.Where(x => x.SeparationType == separationReason.Trim());
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var safePage = Math.Max(1, page);
            var safePageSize = Math.Clamp(pageSize, 1, 100);

            var items = await query
                .OrderByDescending(x => x.EffectiveSeparationDate)
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .Select(x => MapToDto(x))
                .ToListAsync(cancellationToken);

            return new PagedResult<SeparationResponseDto>
            {
                Items = items,
                TotalData = totalCount,
                PageNumber = safePage,
                PageSize = safePageSize,
                TotalPage = (int)Math.Ceiling((double)totalCount / Math.Max(1, safePageSize))
            };
        }

        public async Task<SeparationResponseDto?> GetSeparationByIdAsync(Guid separationId, CancellationToken cancellationToken = default)
        {
            var separation = await _uow.Separations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == separationId, cancellationToken);

            return separation == null ? null : MapToDto(separation);
        }

        private static SeparationResponseDto MapToDto(TrxEmployeeSeparation entity)
        {
            return new SeparationResponseDto
            {
                SeparationId = entity.Id,
                SeparationNumber = entity.SeparationNumber,
                EmployeeId = entity.EmployeeId ?? entity.WorkforceProfileId,
                SeparationReason = entity.SeparationType,
                NoticeDate = entity.NoticeDate ?? entity.RequestDate,
                EffectiveDate = entity.EffectiveSeparationDate,
                ResignationLetterUrl = entity.ResignationLetterUrl,
                TerminationDecisionNumber = entity.TerminationDecisionNumber,
                ApprovalStatus = entity.ApprovalStatus,
                Status = entity.SeparationStatus,
                CreatedAt = entity.CreateDateTime
            };
        }

        private async Task<Guid> ResolveActorUserIdAsync(string actorEmail, CancellationToken cancellationToken)
        {
            var userId = await _uow.FindUserIdByEmailAsync(actorEmail, cancellationToken);
            return userId ?? EmployeeRelationNumberSeries.SystemFallbackUserId;
        }
    }
}
