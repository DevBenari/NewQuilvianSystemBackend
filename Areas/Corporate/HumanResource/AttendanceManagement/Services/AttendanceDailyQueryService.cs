using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    public class AttendanceDailyQueryService
    {
        private readonly ApplicationDbContext _dbContext;

        public AttendanceDailyQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public AttendanceDailyFilterMetadataResponse GetMetadata()
        {
            return new AttendanceDailyFilterMetadataResponse
            {
                DefaultFilter = new AttendanceDailyDefaultFilterResponse(),
                CustomPeriods = new List<AttendanceDailyOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                AttendanceStatusOptions = new List<AttendanceDailyOptionResponse>
                {
                    new() { Value = AttendanceValueConstants.AttendanceStatus.Unprocessed, Label = "Belum diproses" },
                    new() { Value = AttendanceValueConstants.AttendanceStatus.Present, Label = "Hadir" },
                    new() { Value = AttendanceValueConstants.AttendanceStatus.Absent, Label = "Tidak hadir" },
                    new() { Value = AttendanceValueConstants.AttendanceStatus.Late, Label = "Terlambat" },
                    new() { Value = AttendanceValueConstants.AttendanceStatus.EarlyLeave, Label = "Pulang lebih awal" },
                    new() { Value = AttendanceValueConstants.AttendanceStatus.Incomplete, Label = "Data belum lengkap" },
                    new() { Value = AttendanceValueConstants.AttendanceStatus.Holiday, Label = "Hari libur" },
                    new() { Value = AttendanceValueConstants.AttendanceStatus.RestDay, Label = "Hari istirahat" },
                    new() { Value = AttendanceValueConstants.AttendanceStatus.Leave, Label = "Cuti/Izin" },
                    new() { Value = AttendanceValueConstants.AttendanceStatus.BusinessTrip, Label = "Perjalanan dinas" },
                    new() { Value = AttendanceValueConstants.AttendanceStatus.Remote, Label = "Kehadiran remote" }
                },
                ProcessingStatusOptions = new List<AttendanceDailyOptionResponse>
                {
                    new() { Value = AttendanceValueConstants.AttendanceProcessingStatus.Pending, Label = "Menunggu diproses" },
                    new() { Value = AttendanceValueConstants.AttendanceProcessingStatus.Processing, Label = "Sedang diproses" },
                    new() { Value = AttendanceValueConstants.AttendanceProcessingStatus.Processed, Label = "Selesai diproses" },
                    new() { Value = AttendanceValueConstants.AttendanceProcessingStatus.ReprocessRequired, Label = "Perlu diproses ulang" },
                    new() { Value = AttendanceValueConstants.AttendanceProcessingStatus.Skipped, Label = "Dilewati" },
                    new() { Value = AttendanceValueConstants.AttendanceProcessingStatus.Error, Label = "Gagal diproses" }
                },
                PayrollInputStatusOptions = new List<AttendanceDailyOptionResponse>
                {
                    new() { Value = AttendanceValueConstants.PayrollInputStatus.Pending, Label = "Menunggu payroll" },
                    new() { Value = AttendanceValueConstants.PayrollInputStatus.Ready, Label = "Siap payroll" },
                    new() { Value = AttendanceValueConstants.PayrollInputStatus.Processed, Label = "Sudah masuk payroll" },
                    new() { Value = AttendanceValueConstants.PayrollInputStatus.Blocked, Label = "Diblokir" },
                    new() { Value = AttendanceValueConstants.PayrollInputStatus.Excluded, Label = "Dikecualikan" }
                },
                ScheduleSourceOptions = new List<AttendanceDailyOptionResponse>
                {
                    new() { Value = AttendanceValueConstants.ScheduleSource.PublishedRoster, Label = "Roster terpublikasi" },
                    new() { Value = AttendanceValueConstants.ScheduleSource.ConfirmedRoster, Label = "Roster terkonfirmasi" },
                    new() { Value = AttendanceValueConstants.ScheduleSource.CompletedRoster, Label = "Roster selesai" },
                    new() { Value = AttendanceValueConstants.ScheduleSource.FixedWorkSchedule, Label = "Jadwal kerja tetap" },
                    new() { Value = AttendanceValueConstants.ScheduleSource.RemoteAttendance, Label = "Kehadiran remote" },
                    new() { Value = AttendanceValueConstants.ScheduleSource.BusinessTrip, Label = "Perjalanan dinas" },
                    new() { Value = AttendanceValueConstants.ScheduleSource.ManualOverride, Label = "Penyesuaian manual" },
                    new() { Value = AttendanceValueConstants.ScheduleSource.Unresolved, Label = "Jadwal belum ditemukan" }
                },
                DueExceptionOptions = new List<AttendanceDailyOptionResponse>
                {
                    new() { Value = "open", Label = "Memiliki exception terbuka" },
                    new() { Value = "payrollBlocking", Label = "Memblokir payroll" },
                    new() { Value = "none", Label = "Tanpa exception terbuka" }
                },
                SortOptions = new List<AttendanceDailyOptionResponse>
                {
                    new() { Value = "attendanceDate", Label = "Tanggal kehadiran" },
                    new() { Value = "workforceDisplayName", Label = "Nama workforce" },
                    new() { Value = "firstCheckInAt", Label = "Waktu check-in" },
                    new() { Value = "lastCheckOutAt", Label = "Waktu check-out" },
                    new() { Value = "lateMinutes", Label = "Menit terlambat" },
                    new() { Value = "payableWorkMinutes", Label = "Menit kerja dibayar" },
                    new() { Value = "overtimeMinutes", Label = "Menit lembur" },
                    new() { Value = "exceptionCount", Label = "Jumlah exception" },
                    new() { Value = "attendanceStatus", Label = "Status kehadiran" },
                    new() { Value = "payrollInputStatus", Label = "Status payroll" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
        }

        public async Task<AttendanceDailySummaryResponse> GetSummaryAsync(
            AttendanceDailyQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = BuildQuery(request);
            return await BuildSummaryAsync(query, cancellationToken);
        }

        public async Task<AttendanceDailyPagedResponse> GetPagedAsync(
            AttendanceDailyQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, 100);
            var query = BuildQuery(request);
            var totalData = await query.CountAsync(cancellationToken);

            var items = await ProjectDaily(ApplySorting(query, request.SortBy, request.SortDirection))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            CompleteDailyPresentation(items);

            return new AttendanceDailyPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<AttendanceDailyDetailResponse?> GetDetailAsync(
            Guid attendanceDailyId,
            CancellationToken cancellationToken = default)
        {
            return await GetDetailInternalAsync(
                attendanceDailyId,
                null,
                null,
                cancellationToken);
        }

        public async Task<List<AttendanceDailySegmentResponse>?> GetSegmentsAsync(
            Guid attendanceDailyId,
            CancellationToken cancellationToken = default)
        {
            if (!await DailyExistsAsync(attendanceDailyId, cancellationToken))
            {
                return null;
            }

            return await BuildSegmentsQuery(attendanceDailyId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<AttendanceDailyExceptionResponse>?> GetExceptionsAsync(
            Guid attendanceDailyId,
            CancellationToken cancellationToken = default)
        {
            if (!await DailyExistsAsync(attendanceDailyId, cancellationToken))
            {
                return null;
            }

            return await BuildExceptionsQuery(attendanceDailyId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<AttendanceDailyRawLogResponse>?> GetRawLogsAsync(
            Guid attendanceDailyId,
            CancellationToken cancellationToken = default)
        {
            if (!await DailyExistsAsync(attendanceDailyId, cancellationToken))
            {
                return null;
            }

            return await BuildRawLogsQuery(attendanceDailyId)
                .ToListAsync(cancellationToken);
        }

        public async Task<AttendancePayrollReadinessPagedResponse> GetPayrollReadinessAsync(
            AttendanceDailyQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, 100);
            var query = BuildQuery(request);
            var totalData = await query.CountAsync(cancellationToken);

            var summary = await BuildPayrollSummaryAsync(query, cancellationToken);

            var items = await ApplySorting(query, request.SortBy, request.SortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AttendancePayrollReadinessResponse
                {
                    AttendanceDailyId = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : null,
                    WorkforceDisplayName = x.WorkforceProfile != null
                        ? x.WorkforceProfile.DisplayName
                        : x.Employee != null
                            ? x.Employee.FullName
                            : x.Doctor != null
                                ? x.Doctor.FullName
                                : x.User != null
                                    ? x.User.DisplayName ?? x.User.UserName ?? x.User.Email ?? x.User.UserCode
                                    : string.Empty,
                    AttendanceDate = x.AttendanceDate,
                    AttendanceStatus = x.AttendanceStatus,
                    ProcessingStatus = x.ProcessingStatus,
                    IsPayrollEligible = x.IsPayrollEligible,
                    PayrollInputStatus = x.PayrollInputStatus,
                    IsLocked = x.IsLocked,
                    PayableWorkMinutes = x.PayableWorkMinutes,
                    OvertimeMinutes = x.OvertimeMinutes,
                    OpenExceptionCount = x.Exceptions.Count(e =>
                        !e.IsDelete &&
                        e.IsActive &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Closed &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Corrected &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Waived),
                    PayrollBlockingExceptionCount = x.Exceptions.Count(e =>
                        !e.IsDelete &&
                        e.IsActive &&
                        e.IsPayrollBlocking &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Closed &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Corrected &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Waived),
                    IsPayrollReady =
                        x.IsPayrollEligible &&
                        x.ProcessingStatus == AttendanceValueConstants.AttendanceProcessingStatus.Processed &&
                        x.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Ready &&
                        !x.Exceptions.Any(e =>
                            !e.IsDelete &&
                            e.IsActive &&
                            e.IsPayrollBlocking &&
                            e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Closed &&
                            e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Corrected &&
                            e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Waived)
                })
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                item.BlockingReasons = BuildPayrollBlockingReasons(item);
            }

            return new AttendancePayrollReadinessPagedResponse
            {
                Summary = summary,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<AttendanceDailyQueryServiceResult<AttendanceDailyPagedResponse>> GetMyHistoryAsync(
            Guid currentUserId,
            DateOnly? startDate,
            DateOnly? endDate,
            string? customPeriod,
            string? attendanceStatus,
            string? processingStatus,
            bool? isLate,
            bool? isEarlyLeave,
            bool? hasMissingPunch,
            string? search,
            string sortBy,
            string sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var self = await ResolveSelfIdentityAsync(currentUserId, cancellationToken);
            if (self == null)
            {
                return AttendanceDailyQueryServiceResult<AttendanceDailyPagedResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Akun workforce user login tidak ditemukan.");
            }

            var request = new AttendanceDailyQueryRequest
            {
                StartDate = startDate,
                EndDate = endDate,
                CustomPeriod = customPeriod,
                AttendanceStatus = attendanceStatus,
                ProcessingStatus = processingStatus,
                IsLate = isLate,
                IsEarlyLeave = isEarlyLeave,
                HasMissingPunch = hasMissingPunch,
                Search = search,
                SortBy = sortBy,
                SortDirection = sortDirection,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
            var normalizedSize = pageSize < 1 ? 25 : Math.Min(pageSize, 100);
            var query = ApplySelfScope(BuildQuery(request), self.UserId, self.WorkforceProfileId);
            var totalData = await query.CountAsync(cancellationToken);

            var items = await ProjectDaily(ApplySorting(query, sortBy, sortDirection))
                .Skip((normalizedPage - 1) * normalizedSize)
                .Take(normalizedSize)
                .ToListAsync(cancellationToken);

            CompleteDailyPresentation(items);

            return AttendanceDailyQueryServiceResult<AttendanceDailyPagedResponse>.Ok(
                new AttendanceDailyPagedResponse
                {
                    PageNumber = normalizedPage,
                    PageSize = normalizedSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)normalizedSize),
                    Items = items
                },
                "Riwayat attendance user login berhasil diambil.");
        }

        public async Task<AttendanceDailyQueryServiceResult<AttendanceSelfServiceSummaryResponse>> GetMySummaryAsync(
            Guid currentUserId,
            DateOnly? startDate,
            DateOnly? endDate,
            string? customPeriod,
            CancellationToken cancellationToken = default)
        {
            var self = await ResolveSelfIdentityAsync(currentUserId, cancellationToken);
            if (self == null)
            {
                return AttendanceDailyQueryServiceResult<AttendanceSelfServiceSummaryResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Akun workforce user login tidak ditemukan.");
            }

            var request = new AttendanceDailyQueryRequest
            {
                StartDate = startDate,
                EndDate = endDate,
                CustomPeriod = customPeriod
            };

            var range = ResolveDateRange(startDate, endDate, customPeriod);
            var query = ApplySelfScope(BuildQuery(request), self.UserId, self.WorkforceProfileId);
            var totalDay = await query.CountAsync(cancellationToken);
            var workingDay = await query.CountAsync(x => !x.IsHoliday && !x.IsRestDay, cancellationToken);
            var presentDay = await query.CountAsync(x => x.IsPresent, cancellationToken);

            var result = new AttendanceSelfServiceSummaryResponse
            {
                StartDate = range.Start,
                EndDate = range.End,
                TotalDay = totalDay,
                PresentDay = presentDay,
                AbsentDay = await query.CountAsync(x => x.IsAbsent, cancellationToken),
                LateDay = await query.CountAsync(x => x.IsLate, cancellationToken),
                EarlyLeaveDay = await query.CountAsync(x => x.IsEarlyLeave, cancellationToken),
                MissingPunchDay = await query.CountAsync(x => x.HasMissingPunch, cancellationToken),
                HolidayDay = await query.CountAsync(x => x.IsHoliday, cancellationToken),
                RestDay = await query.CountAsync(x => x.IsRestDay, cancellationToken),
                CorrectedDay = await query.CountAsync(x => x.IsCorrected, cancellationToken),
                OpenExceptionCount = await query.SumAsync(x => (int?)x.Exceptions.Count(e =>
                    !e.IsDelete &&
                    e.IsActive &&
                    e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Closed &&
                    e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Corrected &&
                    e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Waived), cancellationToken) ?? 0,
                TotalPayableWorkMinutes = await query.SumAsync(x => (long?)x.PayableWorkMinutes, cancellationToken) ?? 0,
                TotalOvertimeMinutes = await query.SumAsync(x => (long?)x.OvertimeMinutes, cancellationToken) ?? 0,
                AttendanceRatePercentage = workingDay <= 0
                    ? 0
                    : Math.Round(presentDay * 100m / workingDay, 2)
            };

            return AttendanceDailyQueryServiceResult<AttendanceSelfServiceSummaryResponse>.Ok(
                result,
                "Ringkasan attendance user login berhasil diambil.");
        }

        public async Task<AttendanceDailyQueryServiceResult<AttendanceDailyDetailResponse>> GetMyDetailAsync(
            Guid attendanceDailyId,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var self = await ResolveSelfIdentityAsync(currentUserId, cancellationToken);
            if (self == null)
            {
                return AttendanceDailyQueryServiceResult<AttendanceDailyDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Akun workforce user login tidak ditemukan.");
            }

            var detail = await GetDetailInternalAsync(
                attendanceDailyId,
                self.UserId,
                self.WorkforceProfileId,
                cancellationToken);

            if (detail == null)
            {
                return AttendanceDailyQueryServiceResult<AttendanceDailyDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance daily tidak ditemukan pada riwayat user login.");
            }

            return AttendanceDailyQueryServiceResult<AttendanceDailyDetailResponse>.Ok(
                detail,
                "Detail attendance user login berhasil diambil.");
        }

        private IQueryable<TrxAttendanceDaily> BuildQuery(AttendanceDailyQueryRequest request)
        {
            var query = _dbContext.Set<TrxAttendanceDaily>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive);

            var range = ResolveDateRange(request.StartDate, request.EndDate, request.CustomPeriod);
            if (range.Start.HasValue)
            {
                query = query.Where(x => x.AttendanceDate >= range.Start.Value);
            }

            if (range.End.HasValue)
            {
                query = query.Where(x => x.AttendanceDate <= range.End.Value);
            }

            if (request.WorkforceProfileId.HasValue && request.WorkforceProfileId.Value != Guid.Empty)
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            if (request.HospitalSiteId.HasValue && request.HospitalSiteId.Value != Guid.Empty)
                query = query.Where(x => x.HospitalSiteId == request.HospitalSiteId.Value);
            if (request.OrganizationUnitId.HasValue && request.OrganizationUnitId.Value != Guid.Empty)
                query = query.Where(x => x.OrganizationUnitId == request.OrganizationUnitId.Value);
            if (request.DepartmentId.HasValue && request.DepartmentId.Value != Guid.Empty)
                query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
            if (request.PositionId.HasValue && request.PositionId.Value != Guid.Empty)
                query = query.Where(x => x.PositionId == request.PositionId.Value);
            if (request.WorkLocationId.HasValue && request.WorkLocationId.Value != Guid.Empty)
                query = query.Where(x => x.WorkLocationId == request.WorkLocationId.Value);
            if (request.WorkScheduleId.HasValue && request.WorkScheduleId.Value != Guid.Empty)
                query = query.Where(x => x.WorkScheduleId == request.WorkScheduleId.Value);
            if (request.ShiftId.HasValue && request.ShiftId.Value != Guid.Empty)
                query = query.Where(x => x.ShiftId == request.ShiftId.Value);

            if (!string.IsNullOrWhiteSpace(request.AttendanceStatus))
                query = query.Where(x => x.AttendanceStatus == request.AttendanceStatus.Trim());
            if (!string.IsNullOrWhiteSpace(request.ProcessingStatus))
                query = query.Where(x => x.ProcessingStatus == request.ProcessingStatus.Trim());
            if (!string.IsNullOrWhiteSpace(request.PayrollInputStatus))
                query = query.Where(x => x.PayrollInputStatus == request.PayrollInputStatus.Trim());
            if (!string.IsNullOrWhiteSpace(request.ScheduleSource))
                query = query.Where(x => x.ScheduleSource == request.ScheduleSource.Trim());

            if (request.IsLate.HasValue) query = query.Where(x => x.IsLate == request.IsLate.Value);
            if (request.IsEarlyLeave.HasValue) query = query.Where(x => x.IsEarlyLeave == request.IsEarlyLeave.Value);
            if (request.HasMissingPunch.HasValue) query = query.Where(x => x.HasMissingPunch == request.HasMissingPunch.Value);
            if (request.IsHoliday.HasValue) query = query.Where(x => x.IsHoliday == request.IsHoliday.Value);
            if (request.IsRestDay.HasValue) query = query.Where(x => x.IsRestDay == request.IsRestDay.Value);
            if (request.IsCorrected.HasValue) query = query.Where(x => x.IsCorrected == request.IsCorrected.Value);
            if (request.IsLocked.HasValue) query = query.Where(x => x.IsLocked == request.IsLocked.Value);
            if (request.IsPayrollEligible.HasValue) query = query.Where(x => x.IsPayrollEligible == request.IsPayrollEligible.Value);

            if (request.HasOpenException.HasValue)
            {
                query = request.HasOpenException.Value
                    ? query.Where(x => x.Exceptions.Any(e =>
                        !e.IsDelete &&
                        e.IsActive &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Closed &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Corrected &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Waived))
                    : query.Where(x => !x.Exceptions.Any(e =>
                        !e.IsDelete &&
                        e.IsActive &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Closed &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Corrected &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Waived));
            }

            if (request.HasPayrollBlockingException.HasValue)
            {
                query = request.HasPayrollBlockingException.Value
                    ? query.Where(x => x.Exceptions.Any(e =>
                        !e.IsDelete &&
                        e.IsActive &&
                        e.IsPayrollBlocking &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Closed &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Corrected &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Waived))
                    : query.Where(x => !x.Exceptions.Any(e =>
                        !e.IsDelete &&
                        e.IsActive &&
                        e.IsPayrollBlocking &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Closed &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Corrected &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Waived));
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    (x.WorkforceProfile != null &&
                        (x.WorkforceProfile.ProfileCode.ToLower().Contains(keyword) ||
                         x.WorkforceProfile.DisplayName.ToLower().Contains(keyword))) ||
                    (x.Employee != null &&
                        (x.Employee.EmployeeCode.ToLower().Contains(keyword) ||
                         x.Employee.EmployeeNumber.ToLower().Contains(keyword) ||
                         x.Employee.FullName.ToLower().Contains(keyword))) ||
                    (x.Doctor != null &&
                        (x.Doctor.DoctorCode.ToLower().Contains(keyword) ||
                         x.Doctor.DoctorNumber.ToLower().Contains(keyword) ||
                         x.Doctor.FullName.ToLower().Contains(keyword))) ||
                    (x.HospitalSite != null && x.HospitalSite.SiteName.ToLower().Contains(keyword)) ||
                    (x.OrganizationUnit != null && x.OrganizationUnit.UnitName.ToLower().Contains(keyword)) ||
                    (x.Department != null && x.Department.DepartmentName.ToLower().Contains(keyword)) ||
                    x.AttendanceStatus.ToLower().Contains(keyword) ||
                    x.ProcessingStatus.ToLower().Contains(keyword) ||
                    x.PayrollInputStatus.ToLower().Contains(keyword));
            }

            return query;
        }

        private static IQueryable<TrxAttendanceDaily> ApplySelfScope(
            IQueryable<TrxAttendanceDaily> query,
            Guid userId,
            Guid? workforceProfileId)
        {
            return workforceProfileId.HasValue
                ? query.Where(x => x.UserId == userId || x.WorkforceProfileId == workforceProfileId.Value)
                : query.Where(x => x.UserId == userId);
        }

        private static IOrderedQueryable<TrxAttendanceDaily> ApplySorting(
            IQueryable<TrxAttendanceDaily> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "attendanceDate").Trim().ToLowerInvariant() switch
            {
                "workforcedisplayname" => desc
                    ? query.OrderByDescending(x => x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty)
                        .ThenByDescending(x => x.AttendanceDate)
                    : query.OrderBy(x => x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty)
                        .ThenBy(x => x.AttendanceDate),
                "firstcheckinat" => desc ? query.OrderByDescending(x => x.FirstCheckInAt) : query.OrderBy(x => x.FirstCheckInAt),
                "lastcheckoutat" => desc ? query.OrderByDescending(x => x.LastCheckOutAt) : query.OrderBy(x => x.LastCheckOutAt),
                "lateminutes" => desc ? query.OrderByDescending(x => x.LateMinutes) : query.OrderBy(x => x.LateMinutes),
                "payableworkminutes" => desc ? query.OrderByDescending(x => x.PayableWorkMinutes) : query.OrderBy(x => x.PayableWorkMinutes),
                "overtimeminutes" => desc ? query.OrderByDescending(x => x.OvertimeMinutes) : query.OrderBy(x => x.OvertimeMinutes),
                "exceptioncount" => desc ? query.OrderByDescending(x => x.ExceptionCount) : query.OrderBy(x => x.ExceptionCount),
                "attendancestatus" => desc ? query.OrderByDescending(x => x.AttendanceStatus) : query.OrderBy(x => x.AttendanceStatus),
                "payrollinputstatus" => desc ? query.OrderByDescending(x => x.PayrollInputStatus) : query.OrderBy(x => x.PayrollInputStatus),
                _ => desc
                    ? query.OrderByDescending(x => x.AttendanceDate)
                        .ThenByDescending(x => x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty)
                    : query.OrderBy(x => x.AttendanceDate)
                        .ThenBy(x => x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty)
            };
        }

        private static IQueryable<AttendanceDailyResponse> ProjectDaily(
            IQueryable<TrxAttendanceDaily> query)
        {
            return query.Select(x => new AttendanceDailyResponse
            {
                Id = x.Id,
                UserId = x.UserId,
                WorkforceProfileId = x.WorkforceProfileId,
                EmployeeId = x.EmployeeId,
                DoctorId = x.DoctorId,
                UserType = x.UserType,
                ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : null,
                WorkforceDisplayName = x.WorkforceProfile != null
                    ? x.WorkforceProfile.DisplayName
                    : x.Employee != null
                        ? x.Employee.FullName
                        : x.Doctor != null
                            ? x.Doctor.FullName
                            : x.User != null
                                ? x.User.DisplayName ?? x.User.UserName ?? x.User.Email ?? x.User.UserCode
                                : string.Empty,
                EmployeeCode = x.Employee != null ? x.Employee.EmployeeCode : null,
                EmployeeNumber = x.Employee != null ? x.Employee.EmployeeNumber : null,
                DoctorCode = x.Doctor != null ? x.Doctor.DoctorCode : null,
                DoctorNumber = x.Doctor != null ? x.Doctor.DoctorNumber : null,
                HospitalSiteId = x.HospitalSiteId,
                HospitalSiteName = x.HospitalSite != null ? x.HospitalSite.SiteName : null,
                OrganizationUnitId = x.OrganizationUnitId,
                OrganizationUnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                PositionId = x.PositionId,
                WorkLocationId = x.WorkLocationId,
                WorkScheduleId = x.WorkScheduleId,
                WorkScheduleName = x.WorkSchedule != null ? x.WorkSchedule.ScheduleName : null,
                ShiftId = x.ShiftId,
                ShiftName = x.Shift != null ? x.Shift.ShiftName : null,
                PrimaryShiftAssignmentId = x.PrimaryShiftAssignmentId,
                AttendanceDate = x.AttendanceDate,
                ScheduleSource = x.ScheduleSource,
                ScheduledCheckInAt = x.ScheduledCheckInAt,
                ScheduledCheckOutAt = x.ScheduledCheckOutAt,
                FirstCheckInAt = x.FirstCheckInAt,
                LastCheckOutAt = x.LastCheckOutAt,
                IsOvernightSchedule = x.IsOvernightSchedule,
                IsHoliday = x.IsHoliday,
                IsRestDay = x.IsRestDay,
                IsPresent = x.IsPresent,
                IsAbsent = x.IsAbsent,
                IsLate = x.IsLate,
                IsEarlyLeave = x.IsEarlyLeave,
                HasMissingPunch = x.HasMissingPunch,
                IsBusinessTrip = x.IsBusinessTrip,
                IsRemoteAttendance = x.IsRemoteAttendance,
                IsCorrected = x.IsCorrected,
                IsLocked = x.IsLocked,
                ScheduledWorkMinutes = x.ScheduledWorkMinutes,
                ActualWorkMinutes = x.ActualWorkMinutes,
                BreakMinutes = x.BreakMinutes,
                PayableWorkMinutes = x.PayableWorkMinutes,
                LateMinutes = x.LateMinutes,
                EarlyLeaveMinutes = x.EarlyLeaveMinutes,
                OvertimeMinutes = x.OvertimeMinutes,
                NightWorkMinutes = x.NightWorkMinutes,
                AttendanceStatus = x.AttendanceStatus,
                ProcessingStatus = x.ProcessingStatus,
                ProcessingVersion = x.ProcessingVersion,
                ProcessedAt = x.ProcessedAt,
                ProcessingMessage = x.ProcessingMessage,
                IsPayrollEligible = x.IsPayrollEligible,
                PayrollInputStatus = x.PayrollInputStatus,
                PayrollProcessedAt = x.PayrollProcessedAt,
                SegmentCount = x.Segments.Count(s => !s.IsDelete && s.IsActive),
                SourceLogCount = x.RawLogs.Count(r => !r.IsDelete && r.IsActive),
                ExceptionCount = x.Exceptions.Count(e => !e.IsDelete && e.IsActive),
                OpenExceptionCount = x.Exceptions.Count(e =>
                    !e.IsDelete &&
                    e.IsActive &&
                    e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Closed &&
                    e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Corrected &&
                    e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Waived),
                PayrollBlockingExceptionCount = x.Exceptions.Count(e =>
                    !e.IsDelete &&
                    e.IsActive &&
                    e.IsPayrollBlocking &&
                    e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Closed &&
                    e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Corrected &&
                    e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Waived),
                CorrectionRequestCount = x.CorrectionRequests.Count(c => !c.IsDelete && c.IsActive),
                IsPayrollReady =
                    x.IsPayrollEligible &&
                    x.ProcessingStatus == AttendanceValueConstants.AttendanceProcessingStatus.Processed &&
                    x.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Ready &&
                    !x.Exceptions.Any(e =>
                        !e.IsDelete &&
                        e.IsActive &&
                        e.IsPayrollBlocking &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Closed &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Corrected &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Waived),
                CreateDateTime = x.CreateDateTime,
                UpdateDateTime = x.UpdateDateTime
            });
        }

        private async Task<AttendanceDailyDetailResponse?> GetDetailInternalAsync(
            Guid attendanceDailyId,
            Guid? scopedUserId,
            Guid? scopedWorkforceProfileId,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<TrxAttendanceDaily>()
                .AsNoTracking()
                .Where(x => x.Id == attendanceDailyId && !x.IsDelete && x.IsActive);

            if (scopedUserId.HasValue)
            {
                query = scopedWorkforceProfileId.HasValue
                    ? query.Where(x => x.UserId == scopedUserId.Value || x.WorkforceProfileId == scopedWorkforceProfileId.Value)
                    : query.Where(x => x.UserId == scopedUserId.Value);
            }

            var detail = await query
                .Select(x => new AttendanceDailyDetailResponse
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    WorkforceProfileId = x.WorkforceProfileId,
                    EmployeeId = x.EmployeeId,
                    DoctorId = x.DoctorId,
                    UserType = x.UserType,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : null,
                    WorkforceDisplayName = x.WorkforceProfile != null
                        ? x.WorkforceProfile.DisplayName
                        : x.Employee != null
                            ? x.Employee.FullName
                            : x.Doctor != null
                                ? x.Doctor.FullName
                                : x.User != null
                                    ? x.User.DisplayName ?? x.User.UserName ?? x.User.Email ?? x.User.UserCode
                                    : string.Empty,
                    EmployeeCode = x.Employee != null ? x.Employee.EmployeeCode : null,
                    EmployeeNumber = x.Employee != null ? x.Employee.EmployeeNumber : null,
                    DoctorCode = x.Doctor != null ? x.Doctor.DoctorCode : null,
                    DoctorNumber = x.Doctor != null ? x.Doctor.DoctorNumber : null,
                    OrganizationAssignmentId = x.OrganizationAssignmentId,
                    HospitalSiteId = x.HospitalSiteId,
                    HospitalSiteName = x.HospitalSite != null ? x.HospitalSite.SiteName : null,
                    OrganizationUnitId = x.OrganizationUnitId,
                    OrganizationUnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                    PositionId = x.PositionId,
                    WorkLocationId = x.WorkLocationId,
                    WorkScheduleId = x.WorkScheduleId,
                    WorkScheduleName = x.WorkSchedule != null ? x.WorkSchedule.ScheduleName : null,
                    WorkScheduleAssignmentId = x.WorkScheduleAssignmentId,
                    PrimaryShiftAssignmentId = x.PrimaryShiftAssignmentId,
                    ShiftId = x.ShiftId,
                    ShiftName = x.Shift != null ? x.Shift.ShiftName : null,
                    AttendancePolicyId = x.AttendancePolicyId,
                    AttendancePolicyName = x.AttendancePolicy != null ? x.AttendancePolicy.AttendancePolicyName : null,
                    GracePeriodPolicyId = x.GracePeriodPolicyId,
                    GracePeriodPolicyName = x.GracePeriodPolicy != null ? x.GracePeriodPolicy.GracePeriodPolicyName : null,
                    PayrollPeriodId = x.PayrollPeriodId,
                    AttendanceDate = x.AttendanceDate,
                    ScheduleSource = x.ScheduleSource,
                    ScheduleResolutionJson = x.ScheduleResolutionJson,
                    ScheduledCheckInAt = x.ScheduledCheckInAt,
                    ScheduledCheckOutAt = x.ScheduledCheckOutAt,
                    FirstCheckInAt = x.FirstCheckInAt,
                    LastCheckOutAt = x.LastCheckOutAt,
                    IsOvernightSchedule = x.IsOvernightSchedule,
                    IsHoliday = x.IsHoliday,
                    IsRestDay = x.IsRestDay,
                    IsPresent = x.IsPresent,
                    IsAbsent = x.IsAbsent,
                    IsLate = x.IsLate,
                    IsEarlyLeave = x.IsEarlyLeave,
                    HasMissingPunch = x.HasMissingPunch,
                    IsBusinessTrip = x.IsBusinessTrip,
                    IsRemoteAttendance = x.IsRemoteAttendance,
                    IsCorrected = x.IsCorrected,
                    IsLocked = x.IsLocked,
                    ScheduledWorkMinutes = x.ScheduledWorkMinutes,
                    ActualWorkMinutes = x.ActualWorkMinutes,
                    BreakMinutes = x.BreakMinutes,
                    PayableWorkMinutes = x.PayableWorkMinutes,
                    LateMinutes = x.LateMinutes,
                    EarlyLeaveMinutes = x.EarlyLeaveMinutes,
                    OvertimeMinutes = x.OvertimeMinutes,
                    NightWorkMinutes = x.NightWorkMinutes,
                    AttendanceStatus = x.AttendanceStatus,
                    ProcessingStatus = x.ProcessingStatus,
                    ProcessingVersion = x.ProcessingVersion,
                    ProcessedAt = x.ProcessedAt,
                    ProcessingMessage = x.ProcessingMessage,
                    IsPayrollEligible = x.IsPayrollEligible,
                    PayrollInputStatus = x.PayrollInputStatus,
                    PayrollProcessedAt = x.PayrollProcessedAt,
                    SegmentCount = x.Segments.Count(s => !s.IsDelete && s.IsActive),
                    SourceLogCount = x.RawLogs.Count(r => !r.IsDelete && r.IsActive),
                    ExceptionCount = x.Exceptions.Count(e => !e.IsDelete && e.IsActive),
                    OpenExceptionCount = x.Exceptions.Count(e =>
                        !e.IsDelete &&
                        e.IsActive &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Closed &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Corrected &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Waived),
                    PayrollBlockingExceptionCount = x.Exceptions.Count(e =>
                        !e.IsDelete &&
                        e.IsActive &&
                        e.IsPayrollBlocking &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Closed &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Corrected &&
                        e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Waived),
                    CorrectionRequestCount = x.CorrectionRequests.Count(c => !c.IsDelete && c.IsActive),
                    IsPayrollReady =
                        x.IsPayrollEligible &&
                        x.ProcessingStatus == AttendanceValueConstants.AttendanceProcessingStatus.Processed &&
                        x.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Ready &&
                        !x.Exceptions.Any(e =>
                            !e.IsDelete &&
                            e.IsActive &&
                            e.IsPayrollBlocking &&
                            e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Closed &&
                            e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Corrected &&
                            e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Waived),
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (detail == null)
            {
                return null;
            }

            detail.UserTypeName = detail.UserType.ToString();
            detail.Segments = await BuildSegmentsQuery(attendanceDailyId).ToListAsync(cancellationToken);
            detail.Exceptions = await BuildExceptionsQuery(attendanceDailyId).ToListAsync(cancellationToken);
            detail.RawLogs = await BuildRawLogsQuery(attendanceDailyId).ToListAsync(cancellationToken);
            detail.CorrectionRequests = await _dbContext.Set<HrdAttendanceCorrectionRequest>()
                .AsNoTracking()
                .Where(x => x.AttendanceDailyId == attendanceDailyId && !x.IsDelete && x.IsActive)
                .OrderByDescending(x => x.CreateDateTime)
                .Select(x => new AttendanceDailyCorrectionRequestResponse
                {
                    Id = x.Id,
                    RequestNumber = x.RequestNumber,
                    CorrectionType = x.CorrectionType,
                    RequestStatus = x.RequestStatus,
                    Reason = x.Reason,
                    WorkflowInstanceId = x.WorkflowInstanceId,
                    SubmittedAt = x.SubmittedAt,
                    ApprovedAt = x.ApprovedAt,
                    RejectedAt = x.RejectedAt,
                    AppliedAt = x.AppliedAt,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            await ApplyCorrectionAvailabilityAsync(detail, cancellationToken);
            return detail;
        }

        private IQueryable<AttendanceDailySegmentResponse> BuildSegmentsQuery(Guid attendanceDailyId)
        {
            return _dbContext.Set<TrxAttendanceDailySegment>()
                .AsNoTracking()
                .Where(x => x.AttendanceDailyId == attendanceDailyId && !x.IsDelete && x.IsActive)
                .OrderBy(x => x.SegmentOrder)
                .ThenBy(x => x.ScheduledStartAt)
                .Select(x => new AttendanceDailySegmentResponse
                {
                    Id = x.Id,
                    AttendanceDailyId = x.AttendanceDailyId,
                    ShiftAssignmentId = x.ShiftAssignmentId,
                    SegmentOrder = x.SegmentOrder,
                    SegmentType = x.SegmentType,
                    SegmentSource = x.SegmentSource,
                    ScheduledStartAt = x.ScheduledStartAt,
                    ScheduledEndAt = x.ScheduledEndAt,
                    ActualStartAt = x.ActualStartAt,
                    ActualEndAt = x.ActualEndAt,
                    StartRawLogId = x.StartRawLogId,
                    EndRawLogId = x.EndRawLogId,
                    ScheduledMinutes = x.ScheduledMinutes,
                    ActualMinutes = x.ActualMinutes,
                    BreakMinutes = x.BreakMinutes,
                    PayableMinutes = x.PayableMinutes,
                    LateMinutes = x.LateMinutes,
                    EarlyLeaveMinutes = x.EarlyLeaveMinutes,
                    OvertimeMinutes = x.OvertimeMinutes,
                    IsOvernight = x.IsOvernight,
                    IsCorrected = x.IsCorrected,
                    SegmentStatus = x.SegmentStatus,
                    Notes = x.Notes
                });
        }

        private IQueryable<AttendanceDailyExceptionResponse> BuildExceptionsQuery(Guid attendanceDailyId)
        {
            return _dbContext.Set<TrxAttendanceException>()
                .AsNoTracking()
                .Where(x => x.AttendanceDailyId == attendanceDailyId && !x.IsDelete && x.IsActive)
                .OrderByDescending(x => x.ExceptionStatus == AttendanceValueConstants.AttendanceExceptionStatus.Open)
                .ThenByDescending(x => x.DetectedAt)
                .Select(x => new AttendanceDailyExceptionResponse
                {
                    Id = x.Id,
                    AttendanceDailyId = x.AttendanceDailyId,
                    CorrectionRequestId = x.CorrectionRequestId,
                    ExceptionCode = x.ExceptionCode,
                    ExceptionType = x.ExceptionType,
                    Severity = x.Severity,
                    ExceptionStatus = x.ExceptionStatus,
                    DetectedAt = x.DetectedAt,
                    ExpectedAt = x.ExpectedAt,
                    ActualAt = x.ActualAt,
                    DifferenceMinutes = x.DifferenceMinutes,
                    IsAutoDetected = x.IsAutoDetected,
                    IsPayrollBlocking = x.IsPayrollBlocking,
                    DetectionRule = x.DetectionRule,
                    Message = x.Message,
                    ResolvedByUserId = x.ResolvedByUserId,
                    ResolvedByUserName = x.ResolvedByUser != null
                        ? x.ResolvedByUser.DisplayName ?? x.ResolvedByUser.UserName ?? x.ResolvedByUser.Email ?? x.ResolvedByUser.UserCode
                        : null,
                    ResolvedAt = x.ResolvedAt,
                    ResolutionNote = x.ResolutionNote
                });
        }

        private IQueryable<AttendanceDailyRawLogResponse> BuildRawLogsQuery(Guid attendanceDailyId)
        {
            return _dbContext.Set<TrxAttendanceRawLog>()
                .AsNoTracking()
                .Where(x => x.ProcessedAttendanceDailyId == attendanceDailyId && !x.IsDelete && x.IsActive)
                .OrderBy(x => x.EventAt)
                .ThenBy(x => x.ReceivedAt)
                .Select(x => new AttendanceDailyRawLogResponse
                {
                    Id = x.Id,
                    AttendanceDeviceId = x.AttendanceDeviceId,
                    AttendanceDeviceName = x.AttendanceDevice != null ? x.AttendanceDevice.AttendanceDeviceName : null,
                    AttendanceLocationId = x.AttendanceLocationId,
                    AttendanceLocationName = x.AttendanceLocation != null ? x.AttendanceLocation.AttendanceLocationName : null,
                    ExternalLogId = x.ExternalLogId,
                    ExternalDeviceId = x.ExternalDeviceId,
                    DeviceUserKey = x.DeviceUserKey,
                    EventAt = x.EventAt,
                    EventType = x.EventType,
                    SourceType = x.SourceType,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    AccuracyMeters = x.AccuracyMeters,
                    DistanceMeters = x.DistanceMeters,
                    ProcessingStatus = x.ProcessingStatus,
                    ReceivedAt = x.ReceivedAt,
                    ProcessedAt = x.ProcessedAt,
                    ProcessingMessage = x.ProcessingMessage
                });
        }

        private async Task<AttendanceDailySummaryResponse> BuildSummaryAsync(
            IQueryable<TrxAttendanceDaily> query,
            CancellationToken cancellationToken)
        {
            var total = await query.CountAsync(cancellationToken);
            var workingDay = await query.CountAsync(x => !x.IsHoliday && !x.IsRestDay, cancellationToken);
            var present = await query.CountAsync(x => x.IsPresent, cancellationToken);
            var late = await query.CountAsync(x => x.IsLate, cancellationToken);

            return new AttendanceDailySummaryResponse
            {
                TotalAttendance = total,
                PresentCount = present,
                AbsentCount = await query.CountAsync(x => x.IsAbsent, cancellationToken),
                LateCount = late,
                EarlyLeaveCount = await query.CountAsync(x => x.IsEarlyLeave, cancellationToken),
                MissingPunchCount = await query.CountAsync(x => x.HasMissingPunch, cancellationToken),
                HolidayCount = await query.CountAsync(x => x.IsHoliday, cancellationToken),
                RestDayCount = await query.CountAsync(x => x.IsRestDay, cancellationToken),
                BusinessTripCount = await query.CountAsync(x => x.IsBusinessTrip, cancellationToken),
                RemoteAttendanceCount = await query.CountAsync(x => x.IsRemoteAttendance, cancellationToken),
                CorrectedCount = await query.CountAsync(x => x.IsCorrected, cancellationToken),
                LockedCount = await query.CountAsync(x => x.IsLocked, cancellationToken),
                OpenExceptionCount = await query.SumAsync(x => (int?)x.Exceptions.Count(e =>
                    !e.IsDelete &&
                    e.IsActive &&
                    e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Closed &&
                    e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Corrected &&
                    e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Waived), cancellationToken) ?? 0,
                PayrollBlockingCount = await query.CountAsync(x => x.Exceptions.Any(e =>
                    !e.IsDelete &&
                    e.IsActive &&
                    e.IsPayrollBlocking &&
                    e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Closed &&
                    e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Corrected &&
                    e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Waived), cancellationToken),
                PayrollReadyCount = await query.CountAsync(x => x.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Ready, cancellationToken),
                PayrollProcessedCount = await query.CountAsync(x => x.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Processed, cancellationToken),
                PayrollPendingCount = await query.CountAsync(x => x.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Pending, cancellationToken),
                TotalScheduledWorkMinutes = await query.SumAsync(x => (long?)x.ScheduledWorkMinutes, cancellationToken) ?? 0,
                TotalActualWorkMinutes = await query.SumAsync(x => (long?)x.ActualWorkMinutes, cancellationToken) ?? 0,
                TotalPayableWorkMinutes = await query.SumAsync(x => (long?)x.PayableWorkMinutes, cancellationToken) ?? 0,
                TotalOvertimeMinutes = await query.SumAsync(x => (long?)x.OvertimeMinutes, cancellationToken) ?? 0,
                AttendanceRatePercentage = workingDay <= 0 ? 0 : Math.Round(present * 100m / workingDay, 2),
                LateRatePercentage = workingDay <= 0 ? 0 : Math.Round(late * 100m / workingDay, 2)
            };
        }

        private async Task<AttendancePayrollReadinessSummaryResponse> BuildPayrollSummaryAsync(
            IQueryable<TrxAttendanceDaily> query,
            CancellationToken cancellationToken)
        {
            return new AttendancePayrollReadinessSummaryResponse
            {
                TotalAttendance = await query.CountAsync(cancellationToken),
                EligibleCount = await query.CountAsync(x => x.IsPayrollEligible, cancellationToken),
                ReadyCount = await query.CountAsync(x => x.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Ready, cancellationToken),
                PendingCount = await query.CountAsync(x => x.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Pending, cancellationToken),
                BlockedCount = await query.CountAsync(x => x.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Blocked, cancellationToken),
                ProcessedCount = await query.CountAsync(x => x.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Processed, cancellationToken),
                ExcludedCount = await query.CountAsync(x => x.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Excluded, cancellationToken),
                LockedCount = await query.CountAsync(x => x.IsLocked, cancellationToken),
                PayrollBlockingExceptionCount = await query.SumAsync(x => (int?)x.Exceptions.Count(e =>
                    !e.IsDelete &&
                    e.IsActive &&
                    e.IsPayrollBlocking &&
                    e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Closed &&
                    e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Corrected &&
                    e.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Waived), cancellationToken) ?? 0,
                TotalPayableWorkMinutes = await query.SumAsync(x => (long?)x.PayableWorkMinutes, cancellationToken) ?? 0,
                TotalOvertimeMinutes = await query.SumAsync(x => (long?)x.OvertimeMinutes, cancellationToken) ?? 0
            };
        }

        private async Task ApplyCorrectionAvailabilityAsync(
            AttendanceDailyDetailResponse detail,
            CancellationToken cancellationToken)
        {
            if (!detail.WorkforceProfileId.HasValue)
            {
                detail.CanRequestCorrection = false;
                detail.CorrectionRestrictionReason = "Attendance belum terhubung ke workforce profile.";
                return;
            }

            if (detail.AttendanceDate > DateOnly.FromDateTime(DateTime.UtcNow))
            {
                detail.CanRequestCorrection = false;
                detail.CorrectionRestrictionReason = "Tanggal attendance belum terjadi.";
                return;
            }

            if (detail.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Processed)
            {
                detail.CanRequestCorrection = false;
                detail.CorrectionRestrictionReason = "Attendance sudah diproses ke payroll.";
                return;
            }

            if (detail.IsLocked)
            {
                detail.CanRequestCorrection = false;
                detail.CorrectionRestrictionReason = "Attendance sedang dikunci.";
                return;
            }

            var policy = detail.AttendancePolicyId.HasValue
                ? await _dbContext.Set<MstAttendancePolicy>()
                    .AsNoTracking()
                    .Where(x => x.Id == detail.AttendancePolicyId.Value && !x.IsDelete)
                    .Select(x => new
                    {
                        x.AllowManualCorrection,
                        x.CorrectionRequestLimitDays
                    })
                    .FirstOrDefaultAsync(cancellationToken)
                : null;

            if (policy != null && !policy.AllowManualCorrection)
            {
                detail.CanRequestCorrection = false;
                detail.CorrectionRestrictionReason = "Policy attendance tidak mengizinkan koreksi manual.";
                return;
            }

            var limitDays = policy?.CorrectionRequestLimitDays ?? 7;
            var ageDays = DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - detail.AttendanceDate.DayNumber;
            if (limitDays >= 0 && ageDays > limitDays)
            {
                detail.CanRequestCorrection = false;
                detail.CorrectionRestrictionReason = $"Batas pengajuan koreksi adalah {limitDays} hari setelah tanggal attendance.";
                return;
            }

            var hasOpenRequest = detail.CorrectionRequests.Any(x =>
                x.RequestStatus == AttendanceValueConstants.CorrectionRequestStatus.Draft ||
                x.RequestStatus == AttendanceValueConstants.CorrectionRequestStatus.Submitted ||
                x.RequestStatus == AttendanceValueConstants.CorrectionRequestStatus.UnderReview ||
                x.RequestStatus == AttendanceValueConstants.CorrectionRequestStatus.NeedRevision ||
                x.RequestStatus == AttendanceValueConstants.CorrectionRequestStatus.Approved);

            if (hasOpenRequest)
            {
                detail.CanRequestCorrection = false;
                detail.CorrectionRestrictionReason = "Masih terdapat pengajuan koreksi aktif untuk attendance ini.";
                return;
            }

            detail.CanRequestCorrection = true;
            detail.CorrectionRestrictionReason = null;
        }

        private async Task<bool> DailyExistsAsync(
            Guid attendanceDailyId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<TrxAttendanceDaily>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == attendanceDailyId && !x.IsDelete && x.IsActive, cancellationToken);
        }

        private async Task<SelfIdentity?> ResolveSelfIdentityAsync(
            Guid currentUserId,
            CancellationToken cancellationToken)
        {
            if (currentUserId == Guid.Empty)
            {
                return null;
            }

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.Id == currentUserId)
                .Select(x => new SelfIdentity
                {
                    UserId = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static List<string> BuildPayrollBlockingReasons(
            AttendancePayrollReadinessResponse item)
        {
            var reasons = new List<string>();

            if (!item.IsPayrollEligible)
                reasons.Add("Attendance tidak eligible untuk payroll.");
            if (item.ProcessingStatus != AttendanceValueConstants.AttendanceProcessingStatus.Processed)
                reasons.Add("Attendance belum selesai diproses.");
            if (item.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Blocked)
                reasons.Add("Status input payroll sedang diblokir.");
            if (item.PayrollBlockingExceptionCount > 0)
                reasons.Add($"Terdapat {item.PayrollBlockingExceptionCount} exception yang memblokir payroll.");
            if (item.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Excluded)
                reasons.Add("Attendance dikecualikan dari payroll.");
            if (item.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Processed)
                reasons.Add("Attendance sudah diproses ke payroll.");

            return reasons;
        }

        private static void CompleteDailyPresentation(IEnumerable<AttendanceDailyResponse> items)
        {
            foreach (var item in items)
            {
                item.UserTypeName = item.UserType.ToString();
            }
        }

        private static (DateOnly? Start, DateOnly? End) ResolveDateRange(
            DateOnly? startDate,
            DateOnly? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue || endDate.HasValue)
            {
                var normalizedStart = startDate;
                var normalizedEnd = endDate;
                if (normalizedStart.HasValue && normalizedEnd.HasValue && normalizedStart.Value > normalizedEnd.Value)
                {
                    (normalizedStart, normalizedEnd) = (normalizedEnd, normalizedStart);
                }

                return (normalizedStart, normalizedEnd);
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return customPeriod?.Trim().ToLowerInvariant() switch
            {
                "today" => (today, today),
                "last7days" => (today.AddDays(-6), today),
                "thismonth" => (new DateOnly(today.Year, today.Month, 1), today),
                "lastmonth" => ResolvePreviousMonth(today),
                _ => (null, null)
            };
        }

        private static (DateOnly Start, DateOnly End) ResolvePreviousMonth(DateOnly today)
        {
            var firstCurrentMonth = new DateOnly(today.Year, today.Month, 1);
            var firstPreviousMonth = firstCurrentMonth.AddMonths(-1);
            return (firstPreviousMonth, firstCurrentMonth.AddDays(-1));
        }

        private sealed class SelfIdentity
        {
            public Guid UserId { get; set; }
            public Guid? WorkforceProfileId { get; set; }
        }
    }
}
