using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    /// <summary>
    /// Workflow persetujuan bertingkat dua tahap lembur ruangan dengan penjaga mutlak anti-self-approval (BE-HRD-137, HRD-DEC-120, VR-OT-03, OT-B5).
    /// </summary>
    public class OvertimeApprovalWorkflowService : IOvertimeApprovalWorkflowService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMedicalDirectorOvertimeEscalationService _medicalDirectorEscalationService;
        private readonly ILogger<OvertimeApprovalWorkflowService> _logger;

        public OvertimeApprovalWorkflowService(
            ApplicationDbContext context,
            IMedicalDirectorOvertimeEscalationService medicalDirectorEscalationService,
            ILogger<OvertimeApprovalWorkflowService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _medicalDirectorEscalationService = medicalDirectorEscalationService ?? throw new ArgumentNullException(nameof(medicalDirectorEscalationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PagedResult<OvertimeApprovalInboxItemDto>> GetInboxAsync(
            Guid approverUserId,
            string? status = null,
            Guid? unitId = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<TrxOvertimePlan>()
                .Include(x => x.OrganizationUnit)
                .Include(x => x.Details)
                    .ThenInclude(d => d.WorkforceProfile)
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.PlanStatus == status);
            }
            else
            {
                query = query.Where(x => x.PlanStatus == "WaitingApproval" || x.PlanStatus == "WaitingUnitApproval" || x.PlanStatus == "WaitingHrVerification");
            }

            if (unitId.HasValue && unitId.Value != Guid.Empty)
            {
                query = query.Where(x => x.OrganizationUnitId == unitId.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.CreateDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OvertimeApprovalInboxItemDto
                {
                    Id = x.Id,
                    PlanNumber = x.PlanNumber,
                    PlanTitle = x.PlanTitle,
                    OrganizationUnitId = x.OrganizationUnitId ?? Guid.Empty,
                    OrganizationUnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : string.Empty,
                    PlanDate = x.PlanStartDate,
                    CompensationType = x.CompensationType,
                    Reason = x.Reason,
                    StaffCount = x.Details.Count(d => !d.IsDelete),
                    TotalEstimatedHours = x.Details.Where(d => !d.IsDelete).Sum(d => (decimal)d.PlannedMinutes / 60m),
                    RequestEmployeeUserId = x.CreateBy,
                    RequestEmployeeName = "Staf Pengaju",
                    CurrentStatus = x.PlanStatus,
                    SubmittedAt = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<OvertimeApprovalInboxItemDto>
            {
                Items = items,
                TotalData = totalCount,
                TotalPage = (int)Math.Ceiling((double)totalCount / pageSize),
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<OvertimePlanningServiceResult<OvertimeApprovalResultDto>> ApprovePlanAsync(
            Guid planId,
            ApproveOvertimeCommand command,
            Guid approverUserId,
            CancellationToken cancellationToken = default)
        {
            var plan = await _context.Set<TrxOvertimePlan>()
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.Id == planId && !x.IsDelete, cancellationToken);

            if (plan == null)
            {
                return OvertimePlanningServiceResult<OvertimeApprovalResultDto>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rencana lembur (SPKL) tidak ditemukan atau sudah dihapus.");
            }

            // VR-OT-03, OT-B5: Pemblokiran mutlak persetujuan lembur sendiri (Anti-Self-Approval)
            if (plan.CreateBy == approverUserId ||
                plan.Details.Any(d => d.CreateBy == approverUserId))
            {
                _logger.LogWarning(
                    "Self-approval blocked for user {UserId} on SPKL {PlanNumber}",
                    approverUserId, plan.PlanNumber);

                return OvertimePlanningServiceResult<OvertimeApprovalResultDto>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Akses ditolak: Pejabat struktural dilarang menyetujui formulir lemburnya sendiri.");
            }

            var prevStatus = plan.PlanStatus;
            var nextStatus = prevStatus == "WaitingApproval" || prevStatus == "WaitingUnitApproval"
                ? "WaitingHrVerification"
                : "ApprovedForWork";

            plan.PlanStatus = nextStatus;
            plan.ValidatedAt = DateTime.UtcNow;
            plan.ValidatedByUserId = approverUserId;
            plan.Notes = string.IsNullOrWhiteSpace(command?.Notes)
                ? plan.Notes
                : $"{plan.Notes} | Approval Note: {command.Notes}";
            plan.UpdateBy = approverUserId;
            plan.UpdateDateTime = DateTime.UtcNow;

            foreach (var detail in plan.Details.Where(d => !d.IsDelete))
            {
                detail.DetailStatus = nextStatus;
                detail.UpdateBy = approverUserId;
                detail.UpdateDateTime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "SPKL {PlanNumber} approved by {ApproverId}: {PrevStatus} -> {NextStatus}",
                plan.PlanNumber, approverUserId, prevStatus, nextStatus);

            return OvertimePlanningServiceResult<OvertimeApprovalResultDto>.Ok(
                new OvertimeApprovalResultDto
                {
                    Id = plan.Id,
                    Number = plan.PlanNumber,
                    PreviousStatus = prevStatus,
                    NewStatus = nextStatus,
                    Message = $"SPKL {plan.PlanNumber} berhasil disetujui ({nextStatus})."
                },
                "Persetujuan lembur berhasil diproses.",
                StatusCodes.Status200OK);
        }

        public async Task<OvertimePlanningServiceResult<OvertimeApprovalResultDto>> RejectPlanAsync(
            Guid planId,
            RejectOvertimeCommand command,
            Guid approverUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(command?.RejectionReason))
            {
                return OvertimePlanningServiceResult<OvertimeApprovalResultDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan penolakan SPKL wajib diisi.");
            }

            var plan = await _context.Set<TrxOvertimePlan>()
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.Id == planId && !x.IsDelete, cancellationToken);

            if (plan == null)
            {
                return OvertimePlanningServiceResult<OvertimeApprovalResultDto>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rencana lembur (SPKL) tidak ditemukan atau sudah dihapus.");
            }

            // VR-OT-03: Anti-Self-Approval
            if (plan.CreateBy == approverUserId ||
                plan.Details.Any(d => d.CreateBy == approverUserId))
            {
                return OvertimePlanningServiceResult<OvertimeApprovalResultDto>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Akses ditolak: Pejabat struktural dilarang menyetujui formulir lemburnya sendiri.");
            }

            var prevStatus = plan.PlanStatus;
            plan.PlanStatus = "Rejected";
            plan.Notes = $"{plan.Notes} | Ditolak: {command.RejectionReason}";
            plan.UpdateBy = approverUserId;
            plan.UpdateDateTime = DateTime.UtcNow;

            foreach (var detail in plan.Details.Where(d => !d.IsDelete))
            {
                detail.DetailStatus = "Rejected";
                detail.UpdateBy = approverUserId;
                detail.UpdateDateTime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "SPKL {PlanNumber} rejected by {ApproverId}: Reason={Reason}",
                plan.PlanNumber, approverUserId, command.RejectionReason);

            return OvertimePlanningServiceResult<OvertimeApprovalResultDto>.Ok(
                new OvertimeApprovalResultDto
                {
                    Id = plan.Id,
                    Number = plan.PlanNumber,
                    PreviousStatus = prevStatus,
                    NewStatus = "Rejected",
                    Message = $"SPKL {plan.PlanNumber} berhasil ditolak."
                },
                "Penolakan lembur berhasil diproses.",
                StatusCodes.Status200OK);
        }

        public async Task<PagedResult<MedicalDirectorOvertimeEscalationDto>> GetMedicalDirectorInboxAsync(
            string? urgency = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            return await _medicalDirectorEscalationService.GetEscalationQueueAsync(urgency, page, pageSize, cancellationToken);
        }

        public async Task<OvertimePlanningServiceResult<OvertimeApprovalResultDto>> ApproveMedicalDirectorEscalationAsync(
            Guid realizationId,
            ApproveMedicalDirectorEscalationCommand command,
            Guid directorUserId,
            CancellationToken cancellationToken = default)
        {
            return await _medicalDirectorEscalationService.ApproveEscalationAsync(realizationId, command, directorUserId, cancellationToken);
        }

        public async Task<OvertimePlanningServiceResult<OvertimeApprovalResultDto>> RejectMedicalDirectorEscalationAsync(
            Guid realizationId,
            string rejectionReason,
            Guid directorUserId,
            CancellationToken cancellationToken = default)
        {
            return await _medicalDirectorEscalationService.RejectEscalationAsync(realizationId, rejectionReason, directorUserId, cancellationToken);
        }
    }
}
