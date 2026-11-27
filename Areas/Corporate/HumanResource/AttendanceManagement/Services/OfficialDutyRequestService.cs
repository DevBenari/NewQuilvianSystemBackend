using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;

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
                    CreateDateTime = d.CreateDateTime,
                    CancelDateTime = d.CancelDateTime
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
                CreateDateTime = entity.CreateDateTime,
                CancelDateTime = entity.CancelDateTime
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

            if (await IsOwnRequestAsync(currentUserId, entity.WorkforceProfileId, cancellationToken))
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(StatusCodes.Status403Forbidden, SelfDecisionMessage);
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

            // Resolusi identitas profil pejabat pengesah (MstWorkforceProfile)
            Guid? approverWfpId = null;
            if (request.ApproverWorkforceProfileId.HasValue && request.ApproverWorkforceProfileId.Value != Guid.Empty)
            {
                var isExplicitWfpValid = await _context.Set<MstWorkforceProfile>().AsNoTracking()
                    .AnyAsync(w => w.Id == request.ApproverWorkforceProfileId.Value && !w.IsDelete, cancellationToken);
                if (isExplicitWfpValid)
                {
                    approverWfpId = request.ApproverWorkforceProfileId.Value;
                }
            }

            if (!approverWfpId.HasValue)
            {
                var userLinkedWfpId = await _context.Users.AsNoTracking()
                    .Where(u => u.Id == currentUserId && u.WorkforceProfileId.HasValue)
                    .Select(u => u.WorkforceProfileId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (userLinkedWfpId.HasValue &&
                    await _context.Set<MstWorkforceProfile>().AsNoTracking().AnyAsync(w => w.Id == userLinkedWfpId.Value && !w.IsDelete, cancellationToken))
                {
                    approverWfpId = userLinkedWfpId.Value;
                }
            }

            entity.Status = "Approved";
            entity.ApprovedByWorkforceId = approverWfpId;
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

            if (await IsOwnRequestAsync(currentUserId, entity.WorkforceProfileId, cancellationToken))
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(StatusCodes.Status403Forbidden, SelfDecisionMessage);
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

            // Resolusi identitas profil pejabat penolak (MstWorkforceProfile)
            Guid? approverWfpId = null;
            if (request.ApproverWorkforceProfileId.HasValue && request.ApproverWorkforceProfileId.Value != Guid.Empty)
            {
                var isExplicitWfpValid = await _context.Set<MstWorkforceProfile>().AsNoTracking()
                    .AnyAsync(w => w.Id == request.ApproverWorkforceProfileId.Value && !w.IsDelete, cancellationToken);
                if (isExplicitWfpValid)
                {
                    approverWfpId = request.ApproverWorkforceProfileId.Value;
                }
            }

            if (!approverWfpId.HasValue)
            {
                var userLinkedWfpId = await _context.Users.AsNoTracking()
                    .Where(u => u.Id == currentUserId && u.WorkforceProfileId.HasValue)
                    .Select(u => u.WorkforceProfileId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (userLinkedWfpId.HasValue &&
                    await _context.Set<MstWorkforceProfile>().AsNoTracking().AnyAsync(w => w.Id == userLinkedWfpId.Value && !w.IsDelete, cancellationToken))
                {
                    approverWfpId = userLinkedWfpId.Value;
                }
            }

            entity.Status = "Rejected";
            entity.RejectionReason = request.RejectionReason.Trim();
            entity.ApprovedByWorkforceId = approverWfpId;
            entity.ApprovedAt = DateTime.UtcNow;
            entity.UpdateBy = currentUserId;
            entity.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Surat tugas dinas luar {DutyId} ({AssignmentNumber}) ditolak oleh {ApproverId}. Alasan: {Reason}",
                entity.Id, entity.AssignmentOrderNumber, request.ApproverWorkforceProfileId, request.RejectionReason);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        // Layanan Mandiri surat tugas dinas luar (BE-HRD-467, HRD-DEC-252, HRD-DEC-256, kontrak v31 API 37.5). Profil pegawai
        // selalu dari sesi (VR-PK-34); akun tanpa profil aktif dijawab 404.
        public const string SelfAttachmentCategory = "attendance-official-duty";
        private const string SelfProfileMissingMessage = "Akun ini belum terhubung ke profil pegawai.";
        private const string SelfDecisionMessage = "Anda tidak dapat memutuskan pengajuan Anda sendiri.";
        private const string NotFoundMessage = "Surat penugasan dinas luar tidak ditemukan.";
        private const string AttachmentNotFoundMessage = "Lampiran tidak ditemukan.";
        private const int SelfMaxPageSize = 100;
        private const int MaxReasonLength = 500;

        public async Task<OfficialDutyRequestServiceResult<OfficialDutyPagedResponse>> GetPagedForSelfAsync(
            Guid? workforceProfileId,
            DateOnly? fromDate,
            DateOnly? toDate,
            string? status,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (!await IsActiveProfileAsync(workforceProfileId, cancellationToken))
            {
                return OfficialDutyRequestServiceResult<OfficialDutyPagedResponse>.Fail(StatusCodes.Status404NotFound, SelfProfileMissingMessage);
            }

            var page = await GetPagedAsync(new OfficialDutyQueryRequest
            {
                WorkforceProfileId = workforceProfileId!.Value,
                StartDate = fromDate,
                EndDate = toDate,
                Status = status,
                PageNumber = pageNumber,
                PageSize = Math.Clamp(pageSize, 1, SelfMaxPageSize)
            }, cancellationToken);

            return OfficialDutyRequestServiceResult<OfficialDutyPagedResponse>.Ok(page, "Daftar surat tugas dinas luar Anda berhasil diambil.");
        }

        /// <summary>Detail milik sendiri; surat tugas pegawai lain diperlakukan tidak ditemukan (404).</summary>
        public async Task<OfficialDutyRequestServiceResult<OfficialDutyResponse>> GetByIdForSelfAsync(
            Guid id,
            Guid? workforceProfileId,
            CancellationToken cancellationToken = default)
        {
            if (!await IsActiveProfileAsync(workforceProfileId, cancellationToken))
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(StatusCodes.Status404NotFound, SelfProfileMissingMessage);
            }

            var result = await GetByIdAsync(id, cancellationToken);
            return result.Success && result.Data!.WorkforceProfileId != workforceProfileId!.Value
                ? OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(StatusCodes.Status404NotFound, NotFoundMessage)
                : result;
        }

        /// <summary>
        /// Pegawai mengajukan surat tugas dinas luar untuk dirinya. <c>WorkforceProfileId</c> pada <paramref name="request"/> diabaikan
        /// dan diganti profil sesi; kategori wajib salah satu pilihan metadata; lampiran hanya kunci hasil unggah akun ini sendiri.
        /// Selanjutnya aturan <see cref="CreateAsync"/> yang sama dengan layar HR (rentang jam, nomor surat, tujuan, uraian, periode tertutup).
        /// </summary>
        public async Task<OfficialDutyRequestServiceResult<OfficialDutyResponse>> CreateForSelfAsync(
            Guid? workforceProfileId,
            CreateOfficialDutyRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (!await IsActiveProfileAsync(workforceProfileId, cancellationToken))
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(StatusCodes.Status404NotFound, SelfProfileMissingMessage);
            }

            var category = request.DutyCategory?.Trim() ?? string.Empty;
            if (!AttendanceValueConstants.OfficialDutyCategory.All.Contains(category))
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Kategori dinas luar tidak dikenal. Pilih salah satu kategori yang tersedia.");
            }

            if ((request.AssignmentOrderNumber?.Trim().Length ?? 0) > 100 || (request.Destination?.Trim().Length ?? 0) > 255)
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nomor surat tugas paling banyak 100 karakter dan tujuan dinas paling banyak 255 karakter.");
            }

            var attachment = string.IsNullOrWhiteSpace(request.AttachmentUrl) ? null : request.AttachmentUrl.Trim();
            if (attachment != null &&
                (HrAttachmentStorage.CategoryOf(attachment) != SelfAttachmentCategory ||
                 currentUserId == Guid.Empty ||
                 HrAttachmentStorage.OwnerOf(attachment) != currentUserId))
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Lampiran surat tugas harus berkas yang Anda unggah sendiri.");
            }

            return await CreateAsync(new CreateOfficialDutyRequest
            {
                WorkforceProfileId = workforceProfileId!.Value,
                DutyCategory = category,
                AssignmentOrderNumber = request.AssignmentOrderNumber ?? string.Empty,
                StartAt = request.StartAt,
                EndAt = request.EndAt,
                Destination = request.Destination ?? string.Empty,
                Purpose = request.Purpose ?? string.Empty,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                AttachmentUrl = attachment
            }, currentUserId, cancellationToken);
        }

        /// <summary>
        /// Pemohon membatalkan surat tugasnya yang masih Submitted (VR-PK-33). Alasan wajib (1–500 karakter); kontrak v31 tidak
        /// menyediakan kolom maupun ruas log untuk alasan ini, sehingga hanya divalidasi (sama dengan izin pulang cepat).
        /// </summary>
        public async Task<OfficialDutyRequestServiceResult<OfficialDutyResponse>> CancelBySelfAsync(
            Guid id,
            Guid? workforceProfileId,
            string? cancelReason,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (!await IsActiveProfileAsync(workforceProfileId, cancellationToken))
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(StatusCodes.Status404NotFound, SelfProfileMissingMessage);
            }

            var profileId = workforceProfileId!.Value;
            var entity = await _context.Set<HrdOfficialDutyRequest>()
                .FirstOrDefaultAsync(d => d.Id == id && d.WorkforceProfileId == profileId && !d.IsDelete, cancellationToken);

            if (entity == null)
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(StatusCodes.Status404NotFound, NotFoundMessage);
            }

            if (string.IsNullOrWhiteSpace(cancelReason) || cancelReason.Trim().Length > MaxReasonLength)
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan pembatalan wajib diisi.");
            }

            if (entity.Status != "Submitted")
            {
                return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Pengajuan sudah diputuskan dan tidak dapat dibatalkan.");
            }

            // Hanya Submitted yang dapat dibatalkan; rekap harian baru ditautkan saat disetujui, jadi tidak ada yang perlu dilepas.
            var now = DateTime.UtcNow;
            entity.Status = "Cancelled";
            entity.IsCancel = true;
            entity.CancelBy = currentUserId;
            entity.CancelDateTime = now;
            entity.UpdateBy = currentUserId;
            entity.UpdateDateTime = now;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Surat tugas dinas luar {DutyId} dibatalkan pemohon {WorkforceProfileId}.", entity.Id, entity.WorkforceProfileId);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        /// <summary>
        /// Lampiran boleh diunduh lewat Layanan Mandiri hanya bila menjadi lampiran surat tugas milik sendiri (kontrak v31 API 37.5);
        /// selain itu 404 agar keberadaannya tidak bocor.
        /// </summary>
        public async Task<OfficialDutyRequestServiceResult<bool>> EnsureOwnAttachmentForSelfAsync(
            Guid? workforceProfileId,
            string? attachmentKey,
            CancellationToken cancellationToken = default)
        {
            if (!await IsActiveProfileAsync(workforceProfileId, cancellationToken))
            {
                return OfficialDutyRequestServiceResult<bool>.Fail(StatusCodes.Status404NotFound, SelfProfileMissingMessage);
            }

            var key = attachmentKey?.Trim();
            if (string.IsNullOrEmpty(key) || HrAttachmentStorage.CategoryOf(key) != SelfAttachmentCategory)
            {
                return OfficialDutyRequestServiceResult<bool>.Fail(StatusCodes.Status404NotFound, AttachmentNotFoundMessage);
            }

            var profileId = workforceProfileId!.Value;
            var isOwn = await _context.Set<HrdOfficialDutyRequest>().AsNoTracking()
                .AnyAsync(d => d.WorkforceProfileId == profileId && d.AttachmentUrl == key && !d.IsDelete, cancellationToken);

            return isOwn
                ? OfficialDutyRequestServiceResult<bool>.Ok(true)
                : OfficialDutyRequestServiceResult<bool>.Fail(StatusCodes.Status404NotFound, AttachmentNotFoundMessage);
        }

        /// <summary>VR-PK-34 untuk endpoint ESS tanpa data pengajuan (unggah lampiran): akun tanpa profil pegawai aktif → 404.</summary>
        public async Task<OfficialDutyRequestServiceResult<bool>> EnsureSelfProfileAsync(
            Guid? workforceProfileId,
            CancellationToken cancellationToken = default)
        {
            return await IsActiveProfileAsync(workforceProfileId, cancellationToken)
                ? OfficialDutyRequestServiceResult<bool>.Ok(true)
                : OfficialDutyRequestServiceResult<bool>.Fail(StatusCodes.Status404NotFound, SelfProfileMissingMessage);
        }

        private async Task<bool> IsActiveProfileAsync(Guid? workforceProfileId, CancellationToken cancellationToken)
        {
            if (!workforceProfileId.HasValue)
            {
                return false;
            }

            var profileId = workforceProfileId.Value;
            return await _context.Set<MstWorkforceProfile>().AsNoTracking()
                .AnyAsync(w => w.Id == profileId && w.IsActive && !w.IsDelete, cancellationToken);
        }

        // VR-PK-36 (prinsip VR-WF-02): profil pegawai pemutus ditentukan dari akun login, bukan dari body request.
        private async Task<bool> IsOwnRequestAsync(Guid currentUserId, Guid ownerWorkforceProfileId, CancellationToken cancellationToken)
        {
            var profileId = await _context.Users.AsNoTracking()
                .Where(u => u.Id == currentUserId)
                .Select(u => u.WorkforceProfileId)
                .FirstOrDefaultAsync(cancellationToken);
            return profileId.HasValue && profileId.Value == ownerWorkforceProfileId;
        }
    }
}
