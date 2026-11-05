using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    public class OfficialDutyRequestServiceResult<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static OfficialDutyRequestServiceResult<T> Ok(
            T data,
            string message = "Berhasil.",
            int statusCode = StatusCodes.Status200OK)
        {
            return new OfficialDutyRequestServiceResult<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        public static OfficialDutyRequestServiceResult<T> Fail(
            int statusCode,
            string message)
        {
            return new OfficialDutyRequestServiceResult<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Data = default
            };
        }
    }

    /// <summary>
    /// Service pengelolaan surat tugas dinas luar rumah sakit dan pengesahan status kehadiran OnDuty (BE-HRD-109, HRD-DEC-109).
    /// </summary>
    public class OfficialDutyRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OfficialDutyRequestService> _logger;

        public OfficialDutyRequestService(
            ApplicationDbContext context,
            ILogger<OfficialDutyRequestService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OfficialDutyPagedResponse> GetPagedAsync(
            OfficialDutyQueryRequest query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 20 : (query.PageSize > 100 ? 100 : query.PageSize);

            var dbQuery = _context.Set<HrdOfficialDutyRequest>()
                .Include(d => d.WorkforceProfile)
                .Include(d => d.ApprovedByWorkforce)
                .Where(d => !d.IsDelete)
                .AsNoTracking();

            if (query.WorkforceProfileId.HasValue)
            {
                dbQuery = dbQuery.Where(d => d.WorkforceProfileId == query.WorkforceProfileId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.DutyCategory))
            {
                var category = query.DutyCategory.Trim();
                dbQuery = dbQuery.Where(d => d.DutyCategory == category);
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var status = query.Status.Trim();
                dbQuery = dbQuery.Where(d => d.Status == status);
            }

            if (query.StartDate.HasValue)
            {
                var startDt = query.StartDate.Value.ToDateTime(TimeOnly.MinValue);
                dbQuery = dbQuery.Where(d => d.EndAt >= startDt);
            }

            if (query.EndDate.HasValue)
            {
                var endDt = query.EndDate.Value.ToDateTime(TimeOnly.MaxValue);
                dbQuery = dbQuery.Where(d => d.StartAt <= endDt);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var term = query.SearchTerm.Trim().ToLower();
                dbQuery = dbQuery.Where(d =>
                    (d.WorkforceProfile != null && (d.WorkforceProfile.DisplayName.ToLower().Contains(term) || d.WorkforceProfile.ProfileCode.ToLower().Contains(term))) ||
                    d.AssignmentOrderNumber.ToLower().Contains(term) ||
                    d.Destination.ToLower().Contains(term) ||
                    d.Purpose.ToLower().Contains(term));
            }

            var totalData = await dbQuery.CountAsync(cancellationToken);
            var totalPage = (int)Math.Ceiling(totalData / (double)pageSize);

            var items = await dbQuery
                .OrderByDescending(d => d.StartAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new OfficialDutyResponse
                {
                    Id = d.Id,
                    WorkforceProfileId = d.WorkforceProfileId,
                    EmployeeName = d.WorkforceProfile != null ? d.WorkforceProfile.DisplayName : string.Empty,
                    EmployeeNik = d.WorkforceProfile != null ? d.WorkforceProfile.ProfileCode : string.Empty,
                    DepartmentName = null,
                    DutyCategory = d.DutyCategory,
                    AssignmentOrderNumber = d.AssignmentOrderNumber,
                    StartAt = d.StartAt,
                    EndAt = d.EndAt,
                    Destination = d.Destination,
                    Purpose = d.Purpose,
                    Latitude = d.Latitude,
                    Longitude = d.Longitude,
                    AttachmentUrl = d.AttachmentUrl,
                    Status = d.Status,
                    ApprovedByWorkforceId = d.ApprovedByWorkforceId,
                    ApproverName = d.ApprovedByWorkforce != null ? d.ApprovedByWorkforce.DisplayName : null,
                    ApprovedAt = d.ApprovedAt,
                    RejectionReason = d.RejectionReason,
                    IsActive = d.IsActive,
                    CreateDateTime = d.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            return new OfficialDutyPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = totalPage,
                Items = items
            };
        }

        public async Task<OfficialDutyRequestServiceResult<OfficialDutyResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _context.Set<HrdOfficialDutyRequest>()
                .Include(d => d.WorkforceProfile)
                .Include(d => d.ApprovedByWorkforce)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDelete, cancellationToken);

            if (entity == null)
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Surat penugasan dinas luar tidak ditemukan.");
            }

            var response = new OfficialDutyResponse
            {
                Id = entity.Id,
                WorkforceProfileId = entity.WorkforceProfileId,
                EmployeeName = entity.WorkforceProfile?.DisplayName ?? string.Empty,
                EmployeeNik = entity.WorkforceProfile?.ProfileCode ?? string.Empty,
                DutyCategory = entity.DutyCategory,
                AssignmentOrderNumber = entity.AssignmentOrderNumber,
                StartAt = entity.StartAt,
                EndAt = entity.EndAt,
                Destination = entity.Destination,
                Purpose = entity.Purpose,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                AttachmentUrl = entity.AttachmentUrl,
                Status = entity.Status,
                ApprovedByWorkforceId = entity.ApprovedByWorkforceId,
                ApproverName = entity.ApprovedByWorkforce?.DisplayName,
                ApprovedAt = entity.ApprovedAt,
                RejectionReason = entity.RejectionReason,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime
            };

            return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Ok(response);
        }

        public async Task<OfficialDutyRequestServiceResult<OfficialDutyResponse>> CreateAsync(
            CreateOfficialDutyRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (request.WorkforceProfileId == Guid.Empty)
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "WorkforceProfileId pegawai yang ditugaskan wajib diisi.");
            }

            // Validasi VR-ATT-11: Waktu selesai harus lebih besar dari waktu mulai
            if (request.EndAt <= request.StartAt)
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Waktu selesai dinas (EndAt) harus lebih besar dari waktu mulai dinas (StartAt).");
            }

            // Validasi VR-ATT-12: Nomor surat tugas, tujuan, dan uraian tugas wajib
            if (string.IsNullOrWhiteSpace(request.AssignmentOrderNumber) ||
                string.IsNullOrWhiteSpace(request.Destination) ||
                string.IsNullOrWhiteSpace(request.Purpose))
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nomor surat perintah tugas, tujuan dinas, dan rincian tugas wajib diisi.");
            }

            var workforce = await _context.Set<MstWorkforceProfile>().AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == request.WorkforceProfileId && !w.IsDelete, cancellationToken);

            if (workforce == null)
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Data profil tenaga kerja (WorkforceProfile) tidak ditemukan.");
            }

            var startDateOnly = DateOnly.FromDateTime(request.StartAt);
            var endDateOnly = DateOnly.FromDateTime(request.EndAt);

            // Guard HRD-DEC-028: Periode kehadiran tertutup
            var isPeriodClosed = await _context.Set<HrdAttendancePeriod>().AsNoTracking()
                .AnyAsync(p => p.StartDate <= endDateOnly && p.EndDate >= startDateOnly &&
                               p.PeriodStatus == AttendanceValueConstants.AttendancePeriodStatus.Closed &&
                               !p.IsDelete, cancellationToken);
            if (isPeriodClosed)
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Rentang tanggal penugasan dinas beririsan dengan periode kehadiran yang telah ditutup (Closed). Pengajuan ditolak.");
            }

            var entity = new HrdOfficialDutyRequest
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = request.WorkforceProfileId,
                DutyCategory = request.DutyCategory.Trim(),
                AssignmentOrderNumber = request.AssignmentOrderNumber.Trim(),
                StartAt = request.StartAt,
                EndAt = request.EndAt,
                Destination = request.Destination.Trim(),
                Purpose = request.Purpose.Trim(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                AttachmentUrl = request.AttachmentUrl?.Trim(),
                Status = "Submitted",
                IsActive = true,
                CreateBy = currentUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _context.Set<HrdOfficialDutyRequest>().Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Surat tugas dinas luar diajukan: {DutyId} ({AssignmentNumber}) untuk pegawai {WorkforceProfileId} rentang {StartAt} s.d. {EndAt}",
                entity.Id, entity.AssignmentOrderNumber, entity.WorkforceProfileId, entity.StartAt, entity.EndAt);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<OfficialDutyRequestServiceResult<OfficialDutyResponse>> ApproveAsync(
            Guid id,
            ApproveOfficialDutyRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _context.Set<HrdOfficialDutyRequest>()
                .Include(d => d.WorkforceProfile)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDelete, cancellationToken);

            if (entity == null)
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Surat penugasan dinas luar tidak ditemukan.");
            }

            if (entity.Status != "Submitted" && entity.Status != "Draft")
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Penugasan tidak dapat disetujui karena berstatus '{entity.Status}'.");
            }

            var startDateOnly = DateOnly.FromDateTime(entity.StartAt);
            var endDateOnly = DateOnly.FromDateTime(entity.EndAt);

            // Guard HRD-DEC-028: Periode kehadiran tertutup
            var isPeriodClosed = await _context.Set<HrdAttendancePeriod>().AsNoTracking()
                .AnyAsync(p => p.StartDate <= endDateOnly && p.EndDate >= startDateOnly &&
                               p.PeriodStatus == AttendanceValueConstants.AttendancePeriodStatus.Closed &&
                               !p.IsDelete, cancellationToken);
            if (isPeriodClosed)
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Rentang penugasan dinas luar beririsan dengan periode kehadiran yang telah ditutup (Closed). Persetujuan ditolak.");
            }

            entity.Status = "Approved";
            entity.ApprovedByWorkforceId = request.ApproverWorkforceProfileId;
            entity.ApprovedAt = DateTime.UtcNow;
            entity.UpdateBy = currentUserId;
            entity.UpdateDateTime = DateTime.UtcNow;

            // Auto-update pengakuan status OnDuty pada HrdAttendanceDaily (AC 2 & 3: BE-HRD-109, HRD-DEC-109)
            var targetUserId = await _context.Users.AsNoTracking()
                .Where(u => u.WorkforceProfileId == entity.WorkforceProfileId && u.IsActive)
                .Select(u => u.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var existingDailies = await _context.Set<HrdAttendanceDaily>()
                .Include(d => d.Exceptions)
                .Where(d => d.WorkforceProfileId == entity.WorkforceProfileId &&
                            d.AttendanceDate >= startDateOnly &&
                            d.AttendanceDate <= endDateOnly &&
                            !d.IsDelete)
                .ToListAsync(cancellationToken);

            var dailyByDate = existingDailies.ToDictionary(d => d.AttendanceDate);

            for (var curDate = startDateOnly; curDate <= endDateOnly; curDate = curDate.AddDays(1))
            {
                if (dailyByDate.TryGetValue(curDate, out var daily))
                {
                    daily.OfficialDutyRequestId = entity.Id;
                    daily.AttendanceStatus = AttendanceValueConstants.AttendanceStatus.OnDuty;
                    daily.IsPresent = true;
                    daily.IsAbsent = false;

                    if (daily.PayableWorkMinutes == 0)
                    {
                        daily.PayableWorkMinutes = daily.ScheduledWorkMinutes > 0 ? daily.ScheduledWorkMinutes : 480;
                    }

                    if (daily.Exceptions != null)
                    {
                        var dutyExceptions = daily.Exceptions
                            .Where(e => (e.ExceptionType == "Absent" || e.ExceptionType == "MissingCheckIn" || e.ExceptionType == "MissingCheckOut") &&
                                        e.ExceptionStatus != "Closed" &&
                                        !e.IsDelete)
                            .ToList();

                        foreach (var exc in dutyExceptions)
                        {
                            exc.ExceptionStatus = "Waived";
                            exc.IsPayrollBlocking = false;
                            exc.ResolvedAt = DateTime.UtcNow;
                            exc.ResolvedByUserId = currentUserId;
                            exc.ResolutionNote = $"[OnDuty] Penugasan dinas luar resmi disetujui: {entity.AssignmentOrderNumber} ({entity.Destination})";
                        }
                    }
                }
                else
                {
                    // Belum ada daily: buat rekaman OnDuty resmi
                    var newDaily = new HrdAttendanceDaily
                    {
                        Id = Guid.NewGuid(),
                        UserId = targetUserId,
                        WorkforceProfileId = entity.WorkforceProfileId,
                        AttendanceDate = curDate,
                        AttendanceStatus = AttendanceValueConstants.AttendanceStatus.OnDuty,
                        OfficialDutyRequestId = entity.Id,
                        IsPresent = true,
                        IsAbsent = false,
                        ScheduledWorkMinutes = 480,
                        PayableWorkMinutes = 480,
                        ProcessingStatus = AttendanceValueConstants.AttendanceProcessingStatus.Processed,
                        ProcessingMessage = $"Status dinas luar rumah sakit resmi disetujui (Surat Tugas: {entity.AssignmentOrderNumber})",
                        CreateBy = currentUserId,
                        CreateDateTime = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.Set<HrdAttendanceDaily>().Add(newDaily);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Surat tugas dinas luar {DutyId} ({AssignmentNumber}) disetujui oleh {ApproverId}. Status kehadiran OnDuty diaktifkan.",
                entity.Id, entity.AssignmentOrderNumber, request.ApproverWorkforceProfileId);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<OfficialDutyRequestServiceResult<OfficialDutyResponse>> RejectAsync(
            Guid id,
            RejectOfficialDutyRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _context.Set<HrdOfficialDutyRequest>()
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDelete, cancellationToken);

            if (entity == null)
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Surat penugasan dinas luar tidak ditemukan.");
            }

            if (entity.Status != "Submitted" && entity.Status != "Draft")
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Penugasan tidak dapat ditolak karena berstatus '{entity.Status}'.");
            }

            if (string.IsNullOrWhiteSpace(request.RejectionReason))
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan penolakan surat dinas luar wajib diisi.");
            }

            entity.Status = "Rejected";
            entity.RejectionReason = request.RejectionReason.Trim();
            entity.ApprovedByWorkforceId = request.ApproverWorkforceProfileId;
            entity.ApprovedAt = DateTime.UtcNow;
            entity.UpdateBy = currentUserId;
            entity.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Surat tugas dinas luar {DutyId} ({AssignmentNumber}) ditolak oleh {ApproverId}. Alasan: {Reason}",
                entity.Id, entity.AssignmentOrderNumber, request.ApproverWorkforceProfileId, request.RejectionReason);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }
    }
}
