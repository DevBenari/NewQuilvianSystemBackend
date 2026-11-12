using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    public class EarlyLeavePermissionServiceResult<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static EarlyLeavePermissionServiceResult<T> Ok(
            T data,
            string message = "Berhasil.",
            int statusCode = StatusCodes.Status200OK)
        {
            return new EarlyLeavePermissionServiceResult<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        public static EarlyLeavePermissionServiceResult<T> Fail(
            int statusCode,
            string message)
        {
            return new EarlyLeavePermissionServiceResult<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Data = default
            };
        }
    }

    /// <summary>
    /// Service pengelolaan permohonan izin pulang cepat administratif non-deductible dan waiver pinalti presensi (BE-HRD-108, HRD-DEC-107, menutup HRD-Q-47).
    /// </summary>
    public class EarlyLeavePermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EarlyLeavePermissionService> _logger;

        public EarlyLeavePermissionService(
            ApplicationDbContext context,
            ILogger<EarlyLeavePermissionService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<EarlyLeavePermissionPagedResponse> GetPagedAsync(
            EarlyLeavePermissionQueryRequest query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 20 : (query.PageSize > 100 ? 100 : query.PageSize);

            var dbQuery = _context.Set<HrdEarlyLeavePermission>()
                .Include(p => p.WorkforceProfile)
                .Include(p => p.AttendanceDaily)
                .Include(p => p.ApprovedByWorkforce)
                .Where(p => !p.IsDelete)
                .AsNoTracking();

            if (query.WorkforceProfileId.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.WorkforceProfileId == query.WorkforceProfileId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var trimmedStatus = query.Status.Trim();
                dbQuery = dbQuery.Where(p => p.Status == trimmedStatus);
            }

            if (query.StartDate.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.AttendanceDaily != null && p.AttendanceDaily.AttendanceDate >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.AttendanceDaily != null && p.AttendanceDaily.AttendanceDate <= query.EndDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var term = query.SearchTerm.Trim().ToLower();
                dbQuery = dbQuery.Where(p =>
                    (p.WorkforceProfile != null && (p.WorkforceProfile.DisplayName.ToLower().Contains(term) || p.WorkforceProfile.ProfileCode.ToLower().Contains(term))) ||
                    p.Reason.ToLower().Contains(term));
            }

            var totalData = await dbQuery.CountAsync(cancellationToken);
            var totalPage = (int)Math.Ceiling(totalData / (double)pageSize);

            var items = await dbQuery
                .OrderByDescending(p => p.RequestedEarlyLeaveAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new EarlyLeavePermissionResponse
                {
                    Id = p.Id,
                    AttendanceDailyId = p.AttendanceDailyId,
                    WorkforceProfileId = p.WorkforceProfileId,
                    EmployeeName = p.WorkforceProfile != null ? p.WorkforceProfile.DisplayName : string.Empty,
                    EmployeeNik = p.WorkforceProfile != null ? p.WorkforceProfile.ProfileCode : string.Empty,
                    DepartmentName = p.AttendanceDaily != null && p.AttendanceDaily.Department != null ? p.AttendanceDaily.Department.DepartmentName : null,
                    AttendanceDate = p.AttendanceDaily != null ? p.AttendanceDaily.AttendanceDate : default,
                    ScheduledCheckInAt = p.AttendanceDaily != null ? p.AttendanceDaily.ScheduledCheckInAt : null,
                    ScheduledCheckOutAt = p.AttendanceDaily != null ? p.AttendanceDaily.ScheduledCheckOutAt : null,
                    RequestedEarlyLeaveAt = p.RequestedEarlyLeaveAt,
                    Reason = p.Reason,
                    Status = p.Status,
                    ApprovedByWorkforceId = p.ApprovedByWorkforceId,
                    ApproverName = p.ApprovedByWorkforce != null ? p.ApprovedByWorkforce.DisplayName : null,
                    ApprovedAt = p.ApprovedAt,
                    RejectionReason = p.RejectionReason,
                    IsDeductible = p.IsDeductible,
                    IsActive = p.IsActive,
                    CreateDateTime = p.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            return new EarlyLeavePermissionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = totalPage,
                Items = items
            };
        }

        public async Task<EarlyLeavePermissionServiceResult<EarlyLeavePermissionResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _context.Set<HrdEarlyLeavePermission>()
                .Include(p => p.WorkforceProfile)
                .Include(p => p.AttendanceDaily)
                    .ThenInclude(d => d!.Department)
                .Include(p => p.ApprovedByWorkforce)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDelete, cancellationToken);

            if (entity == null)
            {
                return EarlyLeavePermissionServiceResult<EarlyLeavePermissionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Permohonan izin pulang cepat tidak ditemukan.");
            }

            var response = new EarlyLeavePermissionResponse
            {
                Id = entity.Id,
                AttendanceDailyId = entity.AttendanceDailyId,
                WorkforceProfileId = entity.WorkforceProfileId,
                EmployeeName = entity.WorkforceProfile?.DisplayName ?? string.Empty,
                EmployeeNik = entity.WorkforceProfile?.ProfileCode ?? string.Empty,
                DepartmentName = entity.AttendanceDaily?.Department?.DepartmentName,
                AttendanceDate = entity.AttendanceDaily?.AttendanceDate ?? default,
                ScheduledCheckInAt = entity.AttendanceDaily?.ScheduledCheckInAt,
                ScheduledCheckOutAt = entity.AttendanceDaily?.ScheduledCheckOutAt,
                RequestedEarlyLeaveAt = entity.RequestedEarlyLeaveAt,
                Reason = entity.Reason,
                Status = entity.Status,
                ApprovedByWorkforceId = entity.ApprovedByWorkforceId,
                ApproverName = entity.ApprovedByWorkforce?.DisplayName,
                ApprovedAt = entity.ApprovedAt,
                RejectionReason = entity.RejectionReason,
                IsDeductible = entity.IsDeductible,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime
            };

            return EarlyLeavePermissionServiceResult<EarlyLeavePermissionResponse>.Ok(response);
        }

        public async Task<EarlyLeavePermissionServiceResult<EarlyLeavePermissionResponse>> CreateAsync(
            CreateEarlyLeavePermissionRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (request.AttendanceDailyId == Guid.Empty || request.WorkforceProfileId == Guid.Empty)
            {
                return EarlyLeavePermissionServiceResult<EarlyLeavePermissionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "AttendanceDailyId dan WorkforceProfileId wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return EarlyLeavePermissionServiceResult<EarlyLeavePermissionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan permohonan izin pulang cepat wajib diisi.");
            }

            var daily = await _context.Set<HrdAttendanceDaily>()
                .Include(d => d.WorkforceProfile)
                .FirstOrDefaultAsync(d => d.Id == request.AttendanceDailyId && !d.IsDelete, cancellationToken);

            if (daily == null)
            {
                return EarlyLeavePermissionServiceResult<EarlyLeavePermissionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rekaman kehadiran harian (AttendanceDaily) tidak ditemukan.");
            }

            if (daily.WorkforceProfileId != request.WorkforceProfileId)
            {
                return EarlyLeavePermissionServiceResult<EarlyLeavePermissionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "WorkforceProfileId tidak cocok dengan data kehadiran harian yang dipilih.");
            }

            // Guard HRD-DEC-028: Periode kehadiran tertutup
            var isPeriodClosed = await _context.Set<HrdAttendancePeriod>().AsNoTracking()
                .AnyAsync(p => p.StartDate <= daily.AttendanceDate && p.EndDate >= daily.AttendanceDate &&
                               p.PeriodStatus == AttendanceValueConstants.AttendancePeriodStatus.Closed &&
                               !p.IsDelete, cancellationToken);
            if (isPeriodClosed)
            {
                return EarlyLeavePermissionServiceResult<EarlyLeavePermissionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Periode kehadiran untuk tanggal {daily.AttendanceDate:yyyy-MM-dd} telah ditutup (Closed). Permohonan izin pulang cepat ditolak.");
            }

            // Validasi jadwal
            if (daily.ScheduledCheckInAt.HasValue && request.RequestedEarlyLeaveAt < daily.ScheduledCheckInAt.Value)
            {
                return EarlyLeavePermissionServiceResult<EarlyLeavePermissionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Jam izin pulang cepat tidak boleh lebih awal dari jam mulai kerja yang dijadwalkan.");
            }

            // Guard duplicate active request
            var existingActive = await _context.Set<HrdEarlyLeavePermission>().AsNoTracking()
                .AnyAsync(p => p.AttendanceDailyId == daily.Id &&
                               p.Status != "Rejected" &&
                               p.Status != "Cancelled" &&
                               !p.IsDelete, cancellationToken);
            if (existingActive)
            {
                return EarlyLeavePermissionServiceResult<EarlyLeavePermissionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Sudah ada permohonan izin pulang cepat yang sedang aktif atau telah disetujui untuk hari ini.");
            }

            var entity = new HrdEarlyLeavePermission
            {
                Id = Guid.NewGuid(),
                AttendanceDailyId = daily.Id,
                WorkforceProfileId = request.WorkforceProfileId,
                RequestedEarlyLeaveAt = request.RequestedEarlyLeaveAt,
                Reason = request.Reason.Trim(),
                Status = "Submitted",
                IsDeductible = false, // Menutup HRD-Q-47 & HRD-DEC-107: Tidak memotong saldo cuti tahunan
                IsActive = true,
                CreateBy = currentUserId,
                CreateDateTime = DateTime.UtcNow
            };

            daily.EarlyLeavePermissionId = entity.Id;

            _context.Set<HrdEarlyLeavePermission>().Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Permohonan izin pulang cepat non-deductible diajukan: {PermissionId} untuk pegawai {WorkforceProfileId} pada tanggal {AttendanceDate}",
                entity.Id, entity.WorkforceProfileId, daily.AttendanceDate);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<EarlyLeavePermissionServiceResult<EarlyLeavePermissionResponse>> ApproveAsync(
            Guid id,
            ApproveEarlyLeavePermissionRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _context.Set<HrdEarlyLeavePermission>()
                .Include(p => p.AttendanceDaily)
                    .ThenInclude(d => d!.Exceptions)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDelete, cancellationToken);

            if (entity == null)
            {
                return EarlyLeavePermissionServiceResult<EarlyLeavePermissionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Permohonan izin pulang cepat tidak ditemukan.");
            }

            if (entity.Status != "Submitted" && entity.Status != "Draft")
            {
                return EarlyLeavePermissionServiceResult<EarlyLeavePermissionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Permohonan tidak dapat disetujui karena berstatus '{entity.Status}'.");
            }

            var daily = entity.AttendanceDaily;
            if (daily != null)
            {
                // Guard HRD-DEC-028: Periode kehadiran tertutup
                var isPeriodClosed = await _context.Set<HrdAttendancePeriod>().AsNoTracking()
                    .AnyAsync(p => p.StartDate <= daily.AttendanceDate && p.EndDate >= daily.AttendanceDate &&
                                   p.PeriodStatus == AttendanceValueConstants.AttendancePeriodStatus.Closed &&
                                   !p.IsDelete, cancellationToken);
                if (isPeriodClosed)
                {
                    return EarlyLeavePermissionServiceResult<EarlyLeavePermissionResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        $"Periode kehadiran untuk tanggal {daily.AttendanceDate:yyyy-MM-dd} telah ditutup (Closed). Persetujuan izin pulang cepat ditolak.");
                }

                // Tandai flag pada HrdAttendanceDaily
                daily.IsEarlyLeave = true;
                daily.EarlyLeavePermissionId = entity.Id;

                // Acceptance criteria 3: Mengaktifkan flag waiver penalti presensi (Waived) pada exceptions terkait
                if (daily.Exceptions != null)
                {
                    var earlyExceptions = daily.Exceptions
                        .Where(e => (e.ExceptionType == "EarlyLeave" || e.ExceptionType == "MissingCheckOut") &&
                                    e.ExceptionStatus != "Closed" &&
                                    !e.IsDelete)
                        .ToList();

                    foreach (var exc in earlyExceptions)
                    {
                        exc.ExceptionStatus = "Waived";
                        exc.IsPayrollBlocking = false;
                        exc.ResolvedAt = DateTime.UtcNow;
                        exc.ResolvedByUserId = currentUserId;
                        exc.ResolutionNote = $"[Waiver] Izin pulang cepat non-deductible disetujui (HRD-DEC-107). Catatan: {request.ApprovalNote ?? entity.Reason}";
                    }
                }
            }

            entity.Status = "Approved";
            entity.ApprovedByWorkforceId = request.ApproverWorkforceProfileId;
            entity.ApprovedAt = DateTime.UtcNow;
            entity.UpdateBy = currentUserId;
            entity.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Permohonan izin pulang cepat {PermissionId} disetujui oleh {ApproverId} dengan waiver penalti presensi.",
                entity.Id, request.ApproverWorkforceProfileId);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<EarlyLeavePermissionServiceResult<EarlyLeavePermissionResponse>> RejectAsync(
            Guid id,
            RejectEarlyLeavePermissionRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _context.Set<HrdEarlyLeavePermission>()
                .Include(p => p.AttendanceDaily)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDelete, cancellationToken);

            if (entity == null)
            {
                return EarlyLeavePermissionServiceResult<EarlyLeavePermissionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Permohonan izin pulang cepat tidak ditemukan.");
            }

            if (entity.Status != "Submitted" && entity.Status != "Draft")
            {
                return EarlyLeavePermissionServiceResult<EarlyLeavePermissionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Permohonan tidak dapat ditolak karena berstatus '{entity.Status}'.");
            }

            if (string.IsNullOrWhiteSpace(request.RejectionReason))
            {
                return EarlyLeavePermissionServiceResult<EarlyLeavePermissionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan penolakan izin pulang cepat wajib diisi.");
            }

            if (entity.AttendanceDaily != null && entity.AttendanceDaily.EarlyLeavePermissionId == entity.Id)
            {
                entity.AttendanceDaily.EarlyLeavePermissionId = null;
            }

            entity.Status = "Rejected";
            entity.RejectionReason = request.RejectionReason.Trim();
            entity.ApprovedByWorkforceId = request.ApproverWorkforceProfileId;
            entity.ApprovedAt = DateTime.UtcNow;
            entity.UpdateBy = currentUserId;
            entity.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Permohonan izin pulang cepat {PermissionId} ditolak oleh {ApproverId}. Alasan: {Reason}",
                entity.Id, request.ApproverWorkforceProfileId, request.RejectionReason);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }
    }
}
