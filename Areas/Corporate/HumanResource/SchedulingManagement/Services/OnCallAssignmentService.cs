using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    public class OnCallAssignmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OnCallAssignmentService> _logger;

        public OnCallAssignmentService(
            ApplicationDbContext context,
            ILogger<OnCallAssignmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<OnCallAssignmentResponse>> GetPagedAsync(
            int pageNumber = 1,
            int pageSize = 20,
            Guid? rosterPeriodId = null,
            Guid? workforceProfileId = null,
            string? status = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.TrxOnCallAssignments
                .AsNoTracking()
                .Include(o => o.WorkforceProfile)
                .Include(o => o.OnCallType)
                .Where(o => o.IsActive)
                .AsQueryable();

            if (rosterPeriodId.HasValue && rosterPeriodId.Value != Guid.Empty)
            {
                query = query.Where(o => o.RosterPeriodId == rosterPeriodId.Value);
            }

            if (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty)
            {
                query = query.Where(o => o.WorkforceProfileId == workforceProfileId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.AssignmentStatus == status);
            }

            var totalData = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(o => o.StartAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OnCallAssignmentResponse
                {
                    Id = o.Id,
                    RosterPeriodId = o.RosterPeriodId,
                    WorkforceProfileId = o.WorkforceProfileId,
                    WorkforceProfileCode = o.WorkforceProfile != null ? o.WorkforceProfile.ProfileCode : string.Empty,
                    WorkforceDisplayName = o.WorkforceProfile != null ? o.WorkforceProfile.DisplayName : string.Empty,
                    OnCallTypeId = o.OnCallTypeId,
                    OnCallTypeName = o.OnCallType != null ? o.OnCallType.OnCallTypeName : string.Empty,
                    StartAt = o.StartAt,
                    EndAt = o.EndAt,
                    OnCallRole = o.OnCallRole,
                    AssignmentStatus = o.AssignmentStatus,
                    ConfirmedAt = o.ConfirmedAt,
                    ActivatedAt = o.ActivatedAt,
                    Notes = o.Notes
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<OnCallAssignmentResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<SchedulingRequestServiceResult<OnCallAssignmentResponse>> CreateAsync(
            CreateOnCallAssignmentRequest req,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var period = await _context.TrxRosterPeriods
                .FirstOrDefaultAsync(p => p.Id == req.RosterPeriodId && p.IsActive, cancellationToken);

            if (period == null)
            {
                return SchedulingRequestServiceResult<OnCallAssignmentResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Periode roster dengan ID '{req.RosterPeriodId}' tidak ditemukan.");
            }

            var workforce = await _context.MstWorkforceProfiles
                .FirstOrDefaultAsync(w => w.Id == req.WorkforceProfileId && w.IsActive, cancellationToken);

            if (workforce == null)
            {
                return SchedulingRequestServiceResult<OnCallAssignmentResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Pegawai dengan ID '{req.WorkforceProfileId}' tidak ditemukan.");
            }

            var onCallType = await _context.MstOnCallTypes
                .FirstOrDefaultAsync(t => t.Id == req.OnCallTypeId && t.IsActive, cancellationToken);

            if (onCallType == null)
            {
                return SchedulingRequestServiceResult<OnCallAssignmentResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Tipe on-call dengan ID '{req.OnCallTypeId}' tidak ditemukan.");
            }

            var assignment = new TrxOnCallAssignment
            {
                Id = Guid.NewGuid(),
                RosterPeriodId = req.RosterPeriodId,
                WorkforceProfileId = req.WorkforceProfileId,
                OnCallTypeId = req.OnCallTypeId,
                ShiftId = req.ShiftId,
                StartAt = req.StartAt,
                EndAt = req.EndAt,
                OnCallRole = req.OnCallRole.Trim(),
                AssignmentStatus = "Scheduled",
                Notes = req.Notes?.Trim(),
                IsActive = true,
                CreateBy = currentUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _context.TrxOnCallAssignments.Add(assignment);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new OnCallAssignmentResponse
            {
                Id = assignment.Id,
                RosterPeriodId = assignment.RosterPeriodId,
                WorkforceProfileId = assignment.WorkforceProfileId,
                WorkforceProfileCode = workforce.ProfileCode,
                WorkforceDisplayName = workforce.DisplayName,
                OnCallTypeId = assignment.OnCallTypeId,
                OnCallTypeName = onCallType.OnCallTypeName,
                StartAt = assignment.StartAt,
                EndAt = assignment.EndAt,
                OnCallRole = assignment.OnCallRole,
                AssignmentStatus = assignment.AssignmentStatus,
                Notes = assignment.Notes
            };

            return SchedulingRequestServiceResult<OnCallAssignmentResponse>.Ok(
                response,
                "Penugasan siaga (on-call) berhasil dijadwalkan.",
                StatusCodes.Status201Created);
        }

        public async Task<SchedulingRequestServiceResult<OnCallAssignmentResponse>> ActivateAsync(
            Guid id,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var assignment = await _context.TrxOnCallAssignments
                .Include(o => o.WorkforceProfile)
                .Include(o => o.OnCallType)
                .FirstOrDefaultAsync(o => o.Id == id && o.IsActive, cancellationToken);

            if (assignment == null)
            {
                return SchedulingRequestServiceResult<OnCallAssignmentResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Penugasan siaga (on-call) dengan ID '{id}' tidak ditemukan.");
            }

            assignment.AssignmentStatus = "Activated";
            assignment.ActivatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var response = new OnCallAssignmentResponse
            {
                Id = assignment.Id,
                RosterPeriodId = assignment.RosterPeriodId,
                WorkforceProfileId = assignment.WorkforceProfileId,
                WorkforceProfileCode = assignment.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = assignment.WorkforceProfile?.DisplayName ?? string.Empty,
                OnCallTypeId = assignment.OnCallTypeId,
                OnCallTypeName = assignment.OnCallType?.OnCallTypeName ?? string.Empty,
                StartAt = assignment.StartAt,
                EndAt = assignment.EndAt,
                OnCallRole = assignment.OnCallRole,
                AssignmentStatus = assignment.AssignmentStatus,
                ConfirmedAt = assignment.ConfirmedAt,
                ActivatedAt = assignment.ActivatedAt,
                Notes = assignment.Notes
            };

            return SchedulingRequestServiceResult<OnCallAssignmentResponse>.Ok(
                response,
                "Penugasan siaga (on-call) berhasil diaktivasi secara operasional.");
        }
    }
}
