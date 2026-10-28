using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    public class ShiftReplacementService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ShiftReplacementService> _logger;

        public ShiftReplacementService(
            ApplicationDbContext context,
            ILogger<ShiftReplacementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<ShiftReplacementResponse>> GetPagedAsync(
            int pageNumber = 1,
            int pageSize = 20,
            Guid? shiftAssignmentId = null,
            string? status = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.TrxShiftReplacements
                .AsNoTracking()
                .Include(r => r.ShiftAssignment)
                    .ThenInclude(s => s!.Shift)
                .Include(r => r.OriginalWorkforceProfile)
                .Include(r => r.ReplacementWorkforceProfile)
                .Where(r => r.IsActive)
                .AsQueryable();

            if (shiftAssignmentId.HasValue && shiftAssignmentId.Value != Guid.Empty)
            {
                query = query.Where(r => r.ShiftAssignmentId == shiftAssignmentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.ReplacementStatus == status);
            }

            var totalData = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(r => r.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ShiftReplacementResponse
                {
                    Id = r.Id,
                    ShiftAssignmentId = r.ShiftAssignmentId,
                    ShiftDate = r.ShiftAssignment != null ? r.ShiftAssignment.ShiftDate : default,
                    ShiftName = r.ShiftAssignment != null && r.ShiftAssignment.Shift != null ? r.ShiftAssignment.Shift.ShiftName : string.Empty,
                    OriginalWorkforceProfileId = r.OriginalWorkforceProfileId,
                    OriginalWorkforceDisplayName = r.OriginalWorkforceProfile != null ? r.OriginalWorkforceProfile.DisplayName : string.Empty,
                    ReplacementWorkforceProfileId = r.ReplacementWorkforceProfileId,
                    ReplacementWorkforceDisplayName = r.ReplacementWorkforceProfile != null ? r.ReplacementWorkforceProfile.DisplayName : string.Empty,
                    ReplacementType = r.ReplacementType,
                    Reason = r.Reason,
                    ReplacementStatus = r.ReplacementStatus,
                    CreateDateTime = r.CreateDateTime,
                    ApprovedAt = r.ApprovedAt,
                    AppliedAt = r.AppliedAt
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<ShiftReplacementResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<SchedulingRequestServiceResult<ShiftReplacementResponse>> CreateReplacementAsync(
            CreateShiftReplacementRequest req,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var shiftAssignment = await _context.TrxShiftAssignments
                .Include(s => s.Shift)
                .FirstOrDefaultAsync(s => s.Id == req.ShiftAssignmentId && s.IsActive, cancellationToken);

            if (shiftAssignment == null)
            {
                return SchedulingRequestServiceResult<ShiftReplacementResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Penugasan shift dengan ID '{req.ShiftAssignmentId}' tidak ditemukan.");
            }

            var origStaff = await _context.MstWorkforceProfiles
                .FirstOrDefaultAsync(w => w.Id == req.OriginalWorkforceProfileId && w.IsActive, cancellationToken);

            if (origStaff == null)
            {
                return SchedulingRequestServiceResult<ShiftReplacementResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Pegawai asal dengan ID '{req.OriginalWorkforceProfileId}' tidak ditemukan.");
            }

            var replStaff = await _context.MstWorkforceProfiles
                .FirstOrDefaultAsync(w => w.Id == req.ReplacementWorkforceProfileId && w.IsActive, cancellationToken);

            if (replStaff == null)
            {
                return SchedulingRequestServiceResult<ShiftReplacementResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Pegawai pengganti dengan ID '{req.ReplacementWorkforceProfileId}' tidak ditemukan.");
            }

            var replacement = new TrxShiftReplacement
            {
                Id = Guid.NewGuid(),
                ShiftAssignmentId = req.ShiftAssignmentId,
                OriginalWorkforceProfileId = req.OriginalWorkforceProfileId,
                ReplacementWorkforceProfileId = req.ReplacementWorkforceProfileId,
                ReplacementType = req.ReplacementType.Trim(),
                Reason = req.Reason.Trim(),
                ReplacementStatus = "Draft",
                IsActive = true,
                CreateBy = currentUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _context.TrxShiftReplacements.Add(replacement);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new ShiftReplacementResponse
            {
                Id = replacement.Id,
                ShiftAssignmentId = replacement.ShiftAssignmentId,
                ShiftDate = shiftAssignment.ShiftDate,
                ShiftName = shiftAssignment.Shift?.ShiftName ?? string.Empty,
                OriginalWorkforceProfileId = replacement.OriginalWorkforceProfileId,
                OriginalWorkforceDisplayName = origStaff.DisplayName,
                ReplacementWorkforceProfileId = replacement.ReplacementWorkforceProfileId,
                ReplacementWorkforceDisplayName = replStaff.DisplayName,
                ReplacementType = replacement.ReplacementType,
                Reason = replacement.Reason,
                ReplacementStatus = replacement.ReplacementStatus,
                CreateDateTime = replacement.CreateDateTime
            };

            return SchedulingRequestServiceResult<ShiftReplacementResponse>.Ok(
                response,
                "Penggantian shift operasional darurat berhasil dicatat.",
                StatusCodes.Status201Created);
        }

        public async Task<SchedulingRequestServiceResult<ShiftReplacementResponse>> ApproveReplacementAsync(
            Guid id,
            Guid currentUserId,
            string? notes,
            CancellationToken cancellationToken = default)
        {
            var replacement = await _context.TrxShiftReplacements
                .Include(r => r.ShiftAssignment)
                    .ThenInclude(s => s!.Shift)
                .Include(r => r.OriginalWorkforceProfile)
                .Include(r => r.ReplacementWorkforceProfile)
                .FirstOrDefaultAsync(r => r.Id == id && r.IsActive, cancellationToken);

            if (replacement == null)
            {
                return SchedulingRequestServiceResult<ShiftReplacementResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Permintaan penggantian shift dengan ID '{id}' tidak ditemukan.");
            }

            if (replacement.ReplacementStatus == "Applied" || replacement.ReplacementStatus == "Approved")
            {
                return SchedulingRequestServiceResult<ShiftReplacementResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Permintaan penggantian shift ini sudah disetujui sebelumnya.");
            }

            replacement.ReplacementStatus = "Applied";
            replacement.ApprovedAt = DateTime.UtcNow;
            replacement.ApprovedByUserId = currentUserId;
            replacement.AppliedAt = DateTime.UtcNow;
            replacement.ApprovalNotes = notes?.Trim();

            // Terapkan perubahan ke TrxShiftAssignment
            if (replacement.ShiftAssignment != null)
            {
                replacement.ShiftAssignment.WorkforceProfileId = replacement.ReplacementWorkforceProfileId;
                replacement.ShiftAssignment.AssignmentSource = replacement.ReplacementType;
                replacement.ShiftAssignment.IsManualOverride = true;
                replacement.ShiftAssignment.OverrideReason = $"Penggantian shift ({replacement.ReplacementType}): {replacement.Reason}";
            }

            await _context.SaveChangesAsync(cancellationToken);

            var response = new ShiftReplacementResponse
            {
                Id = replacement.Id,
                ShiftAssignmentId = replacement.ShiftAssignmentId,
                ShiftDate = replacement.ShiftAssignment != null ? replacement.ShiftAssignment.ShiftDate : default,
                ShiftName = replacement.ShiftAssignment?.Shift?.ShiftName ?? string.Empty,
                OriginalWorkforceProfileId = replacement.OriginalWorkforceProfileId,
                OriginalWorkforceDisplayName = replacement.OriginalWorkforceProfile?.DisplayName ?? string.Empty,
                ReplacementWorkforceProfileId = replacement.ReplacementWorkforceProfileId,
                ReplacementWorkforceDisplayName = replacement.ReplacementWorkforceProfile?.DisplayName ?? string.Empty,
                ReplacementType = replacement.ReplacementType,
                Reason = replacement.Reason,
                ReplacementStatus = replacement.ReplacementStatus,
                CreateDateTime = replacement.CreateDateTime,
                ApprovedAt = replacement.ApprovedAt,
                AppliedAt = replacement.AppliedAt
            };

            return SchedulingRequestServiceResult<ShiftReplacementResponse>.Ok(
                response,
                "Penggantian shift berhasil disetujui dan diterapkan langsung ke jadwal operasional.");
        }
    }
}
