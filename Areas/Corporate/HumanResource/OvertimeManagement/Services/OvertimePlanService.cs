using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    /// <summary>
    /// Service pengelolaan SPKL pra-dinas terencana H-1 dan penugasan tim medis ruangan (BE-HRD-134, HRD-DEC-116, HRD-ASM-26).
    /// </summary>
    public class OvertimePlanService : IOvertimePlanService
    {
        private readonly ApplicationDbContext _context;
        private readonly IOvertimeQuotaEnforcementGuard _quotaGuard;
        private readonly ILogger<OvertimePlanService> _logger;

        public OvertimePlanService(
            ApplicationDbContext context,
            IOvertimeQuotaEnforcementGuard quotaGuard,
            ILogger<OvertimePlanService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _quotaGuard = quotaGuard ?? throw new ArgumentNullException(nameof(quotaGuard));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PagedResult<OvertimePlanSummaryDto>> GetMyPlansAsync(
            Guid workforceProfileId,
            string? status = null,
            int? year = null,
            int? month = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<TrxOvertimePlan>()
                .Include(x => x.Details)
                .Where(x => !x.IsDelete && x.Details.Any(d => d.WorkforceProfileId == workforceProfileId && !d.IsDelete));

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.PlanStatus == status);
            }

            if (year.HasValue)
            {
                query = query.Where(x => x.PlanStartDate.Year == year.Value);
            }

            if (month.HasValue)
            {
                query = query.Where(x => x.PlanStartDate.Month == month.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.PlanStartDate)
                .ThenByDescending(x => x.CreateDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OvertimePlanSummaryDto
                {
                    Id = x.Id,
                    PlanNumber = x.PlanNumber,
                    PlanTitle = x.PlanTitle,
                    PlanDate = x.PlanStartDate,
                    CompensationType = x.CompensationType,
                    PlanStatus = x.PlanStatus,
                    TotalMinutes = x.Details.Where(d => !d.IsDelete).Sum(d => d.PlannedMinutes),
                    StaffCount = x.Details.Count(d => !d.IsDelete)
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<OvertimePlanSummaryDto>
            {
                Items = items,
                TotalData = totalCount,
                TotalPage = (int)Math.Ceiling((double)totalCount / pageSize),
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<OvertimePlanDetailDto?> GetPlanByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var plan = await _context.Set<TrxOvertimePlan>()
                .Include(x => x.OrganizationUnit)
                .Include(x => x.Details)
                    .ThenInclude(d => d.WorkforceProfile)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (plan == null) return null;

            return new OvertimePlanDetailDto
            {
                Id = plan.Id,
                PlanNumber = plan.PlanNumber,
                PlanTitle = plan.PlanTitle,
                OrganizationUnitId = plan.OrganizationUnitId ?? Guid.Empty,
                OrganizationUnitName = plan.OrganizationUnit?.UnitName ?? string.Empty,
                PlanStartDate = plan.PlanStartDate,
                PlanEndDate = plan.PlanEndDate,
                CompensationType = plan.CompensationType,
                PlanStatus = plan.PlanStatus,
                Reason = plan.Reason,
                Notes = plan.Notes,
                CreatedAt = plan.CreateDateTime,
                StaffList = plan.Details.Where(d => !d.IsDelete).Select(d => new OvertimePlanStaffItemDto
                {
                    WorkforceProfileId = d.WorkforceProfileId,
                    EmployeeName = d.WorkforceProfile != null ? d.WorkforceProfile.DisplayName : string.Empty,
                    JobTitle = string.Empty,
                    IsOvertimeExempt = false,
                    PlannedStartAt = d.PlannedStartAt,
                    PlannedEndAt = d.PlannedEndAt,
                    PlannedMinutes = d.PlannedMinutes,
                    WorkDescription = d.WorkDescription
                }).ToList()
            };
        }

        public async Task<OvertimePlanningServiceResult<OvertimePlanResultDto>> CreatePlanAsync(
            CreateOvertimePlanCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var validator = new CreateOvertimePlanCommandValidator();
            var valResult = validator.Validate(command);
            if (!valResult.IsValid)
            {
                var errorMsg = string.Join("; ", valResult.Errors);
                return OvertimePlanningServiceResult<OvertimePlanResultDto>.Fail(StatusCodes.Status400BadRequest, errorMsg);
            }

            var durationMinutes = (int)(command.PlanEndTime - command.PlanStartTime).TotalMinutes;
            var durationHours = (decimal)durationMinutes / 60m;

            // Evaluasi setiap staf nakes pelaksana
            foreach (var staff in command.StaffList)
            {
                // VR-OT-18: Cek konflik cuti aktif sehari penuh yang disetujui
                var onLeave = await _context.Set<WfpLeaveRequest>()
                    .AnyAsync(x => x.WorkforceProfileId == staff.WorkforceProfileId
                        && x.LeaveRequestStatus == "Approved"
                        && x.StartDate <= command.PlanStartDate
                        && x.EndDate >= command.PlanStartDate
                        && !x.IsDelete, cancellationToken);

                if (onLeave)
                {
                    return OvertimePlanningServiceResult<OvertimePlanResultDto>.Fail(
                        StatusCodes.Status409Conflict,
                        "Pegawai sedang dalam jadwal cuti aktif yang telah disetujui pada tanggal tersebut.");
                }

                // VR-OT-04: Evaluasi batas kuota lembur PP 35/2021
                var quotaEval = await _quotaGuard.EvaluateQuotaAsync(
                    staff.WorkforceProfileId,
                    command.PlanStartDate,
                    durationHours,
                    isEmergencyCito: false,
                    cancellationToken);

                if (!quotaEval.IsAllowed)
                {
                    return OvertimePlanningServiceResult<OvertimePlanResultDto>.Fail(
                        StatusCodes.Status400BadRequest,
                        quotaEval.DenialReason ?? "Pengajuan melebihi kuota maksimal lembur (4 jam/hari atau 18 jam/minggu) sesuai regulasi PP 35/2021.");
                }
            }

            // Generate nomor SPKL otomatis: SPKL/YYYY/MM/XXXX
            var year = command.PlanStartDate.Year;
            var month = command.PlanStartDate.Month;
            var prefix = $"SPKL/{year}/{month:D2}/";
            var countThisMonth = await _context.Set<TrxOvertimePlan>()
                .CountAsync(x => x.PlanNumber.StartsWith(prefix), cancellationToken);
            var planNumber = $"{prefix}{(countThisMonth + 1):D4}";

            var plan = new TrxOvertimePlan
            {
                Id = Guid.NewGuid(),
                PlanNumber = planNumber,
                PlanTitle = command.PlanTitle,
                OrganizationUnitId = command.OrganizationUnitId,
                PlanStartDate = command.PlanStartDate,
                PlanEndDate = command.PlanEndDate,
                Reason = command.Reason,
                Notes = command.Notes,
                PlanStatus = "WaitingApproval",
                CompensationType = command.CompensationType,
                IsEmergencyEscalation = false,
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };

            var seq = 1;
            foreach (var staff in command.StaffList)
            {
                var detail = new TrxOvertimePlanDetail
                {
                    Id = Guid.NewGuid(),
                    OvertimePlanId = plan.Id,
                    SequenceNumber = seq++,
                    WorkforceProfileId = staff.WorkforceProfileId,
                    OvertimeDate = command.PlanStartDate,
                    PlannedStartAt = command.PlanStartTime,
                    PlannedEndAt = command.PlanEndTime,
                    PlannedMinutes = durationMinutes,
                    DayType = "Workday",
                    OvertimeCategory = "AfterShift",
                    WorkDescription = string.IsNullOrWhiteSpace(staff.WorkDescription) ? command.Reason : staff.WorkDescription,
                    DetailStatus = "WaitingApproval",
                    CreateBy = actorUserId,
                    CreateDateTime = DateTime.UtcNow
                };
                plan.Details.Add(detail);
            }

            _context.Set<TrxOvertimePlan>().Add(plan);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "SPKL plan created: {PlanNumber} with {StaffCount} staff for unit {UnitId} on {Date}",
                planNumber, command.StaffList.Count, command.OrganizationUnitId, command.PlanStartDate);

            return OvertimePlanningServiceResult<OvertimePlanResultDto>.Ok(
                new OvertimePlanResultDto
                {
                    Id = plan.Id,
                    PlanNumber = plan.PlanNumber,
                    PlanStatus = plan.PlanStatus,
                    TotalEstimatedHours = durationHours,
                    TotalStaffCount = command.StaffList.Count,
                    Message = $"SPKL {plan.PlanNumber} berhasil diajukan dan sedang menunggu persetujuan atasan."
                },
                "Rencana lembur (SPKL) berhasil dibuat.",
                StatusCodes.Status201Created);
        }

        public async Task<OvertimePlanningServiceResult<OvertimePlanResultDto>> CancelPlanAsync(
            Guid id,
            CancelOvertimePlanCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var plan = await _context.Set<TrxOvertimePlan>()
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (plan == null)
            {
                return OvertimePlanningServiceResult<OvertimePlanResultDto>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rencana lembur tidak ditemukan atau sudah dihapus.");
            }

            if (plan.PlanStatus == "Completed" || plan.PlanStatus == "Closed")
            {
                return OvertimePlanningServiceResult<OvertimePlanResultDto>.Fail(
                    StatusCodes.Status409Conflict,
                    "Rencana lembur yang telah selesai atau ditutup tidak dapat dibatalkan.");
            }

            // VR-OT-23: Pembatalan hanya sebelum jam pelaksanaan dinas dimulai
            var now = DateTime.UtcNow;
            var planStart = plan.PlanStartDate.ToDateTime(TimeOnly.MinValue);
            if (now >= planStart)
            {
                return OvertimePlanningServiceResult<OvertimePlanResultDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pembatalan rencana lembur hanya dapat diajukan sebelum jam pelaksanaan dinas dimulai.");
            }

            plan.PlanStatus = "Cancelled";
            plan.Notes = string.IsNullOrWhiteSpace(plan.Notes)
                ? $"Dibatalkan: {command.Reason}"
                : $"{plan.Notes} | Dibatalkan: {command.Reason}";
            plan.UpdateBy = actorUserId;
            plan.UpdateDateTime = DateTime.UtcNow;

            foreach (var detail in plan.Details.Where(d => !d.IsDelete))
            {
                detail.DetailStatus = "Cancelled";
                detail.UpdateBy = actorUserId;
                detail.UpdateDateTime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("SPKL plan cancelled: {PlanNumber} reason: {Reason}", plan.PlanNumber, command.Reason);

            return OvertimePlanningServiceResult<OvertimePlanResultDto>.Ok(
                new OvertimePlanResultDto
                {
                    Id = plan.Id,
                    PlanNumber = plan.PlanNumber,
                    PlanStatus = plan.PlanStatus,
                    Message = $"SPKL {plan.PlanNumber} berhasil dibatalkan."
                },
                "Rencana lembur berhasil dibatalkan.",
                StatusCodes.Status200OK);
        }
    }
}
