using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    public class AttendanceRawLogService
    {
        private const int MaximumBatchItem = 500;

        private static readonly string[] EventTypes =
        {
            AttendanceValueConstants.RawLogEventType.CheckIn,
            AttendanceValueConstants.RawLogEventType.CheckOut,
            AttendanceValueConstants.RawLogEventType.BreakStart,
            AttendanceValueConstants.RawLogEventType.BreakEnd,
            AttendanceValueConstants.RawLogEventType.Unknown
        };

        private static readonly string[] SourceTypes =
        {
            AttendanceValueConstants.RawLogSourceType.Device,
            AttendanceValueConstants.RawLogSourceType.Mobile,
            AttendanceValueConstants.RawLogSourceType.WebLogin,
            AttendanceValueConstants.RawLogSourceType.Import,
            AttendanceValueConstants.RawLogSourceType.Integration,
            AttendanceValueConstants.RawLogSourceType.Manual
        };

        private static readonly string[] ProcessingStatuses =
        {
            AttendanceValueConstants.RawLogProcessingStatus.Pending,
            AttendanceValueConstants.RawLogProcessingStatus.Matched,
            AttendanceValueConstants.RawLogProcessingStatus.Processed,
            AttendanceValueConstants.RawLogProcessingStatus.Duplicate,
            AttendanceValueConstants.RawLogProcessingStatus.Rejected,
            AttendanceValueConstants.RawLogProcessingStatus.Error
        };

        private readonly ApplicationDbContext _dbContext;

        public AttendanceRawLogService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AttendanceRawLogFilterMetadataResponse> GetFilterMetadataAsync()
        {
            var deviceOptions = await _dbContext.Set<MstAttendanceDevice>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .OrderBy(x => x.AttendanceDeviceName)
                .ThenBy(x => x.AttendanceDeviceCode)
                .Select(x => new AttendanceRawLogGuidOptionResponse
                {
                    Id = x.Id,
                    Code = x.AttendanceDeviceCode,
                    Name = x.AttendanceDeviceName
                })
                .ToListAsync();

            var locationOptions = await _dbContext.Set<MstAttendanceLocation>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .OrderBy(x => x.AttendanceLocationName)
                .ThenBy(x => x.AttendanceLocationCode)
                .Select(x => new AttendanceRawLogGuidOptionResponse
                {
                    Id = x.Id,
                    Code = x.AttendanceLocationCode,
                    Name = x.AttendanceLocationName
                })
                .ToListAsync();

            return new AttendanceRawLogFilterMetadataResponse
            {
                MaximumBatchItem = MaximumBatchItem,
                DefaultFilter = new AttendanceRawLogDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                EventTypeOptions = EventTypes
                    .Select(x => new AttendanceRawLogStringOptionResponse
                    {
                        Value = x,
                        Label = BuildEventTypeLabel(x)
                    })
                    .ToList(),
                SourceTypeOptions = SourceTypes
                    .Select(x => new AttendanceRawLogStringOptionResponse
                    {
                        Value = x,
                        Label = BuildSourceTypeLabel(x)
                    })
                    .ToList(),
                ProcessingStatusOptions = ProcessingStatuses
                    .Select(x => new AttendanceRawLogStringOptionResponse
                    {
                        Value = x,
                        Label = BuildProcessingStatusLabel(x)
                    })
                    .ToList(),
                AttendanceDeviceOptions = deviceOptions,
                AttendanceLocationOptions = locationOptions,
                SortOptions = new List<AttendanceRawLogSortOptionResponse>
                {
                    new() { Value = "eventAt", Label = "Waktu kejadian" },
                    new() { Value = "receivedAt", Label = "Waktu diterima" },
                    new() { Value = "workforceDisplayName", Label = "Nama workforce" },
                    new() { Value = "attendanceDeviceName", Label = "Perangkat absensi" },
                    new() { Value = "sourceType", Label = "Sumber" },
                    new() { Value = "processingStatus", Label = "Status pemrosesan" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
        }

        public async Task<AttendanceRawLogSummaryResponse> GetSummaryAsync()
        {
            var query = BuildBaseQuery();
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            return new AttendanceRawLogSummaryResponse
            {
                TotalRawLog = await query.CountAsync(),
                ReceivedToday = await query.CountAsync(x => x.ReceivedAt >= today && x.ReceivedAt < tomorrow),
                Pending = await query.CountAsync(x => x.ProcessingStatus == AttendanceValueConstants.RawLogProcessingStatus.Pending),
                Matched = await query.CountAsync(x => x.ProcessingStatus == AttendanceValueConstants.RawLogProcessingStatus.Matched),
                Processed = await query.CountAsync(x => x.ProcessingStatus == AttendanceValueConstants.RawLogProcessingStatus.Processed),
                Rejected = await query.CountAsync(x => x.ProcessingStatus == AttendanceValueConstants.RawLogProcessingStatus.Rejected),
                Error = await query.CountAsync(x => x.ProcessingStatus == AttendanceValueConstants.RawLogProcessingStatus.Error),
                UnmatchedWorkforce = await query.CountAsync(x => x.WorkforceProfileId == null),
                DeviceSource = await query.CountAsync(x => x.SourceType == AttendanceValueConstants.RawLogSourceType.Device),
                MobileSource = await query.CountAsync(x => x.SourceType == AttendanceValueConstants.RawLogSourceType.Mobile)
            };
        }

        public async Task<PagedResult<AttendanceRawLogResponse>> GetPagedAsync(
            AttendanceRawLogQueryRequest request)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, 100);

            var query = ApplyFilter(BuildBaseQuery(), request);
            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, request.SortBy, request.SortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AttendanceRawLogResponse
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserName = x.User != null
                        ? (x.User.DisplayName != string.Empty
                            ? x.User.DisplayName
                            : x.User.UserName)
                        : null,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : null,
                    WorkforceDisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : null,
                    EmployeeId = x.EmployeeId,
                    EmployeeCode = x.Employee != null ? x.Employee.EmployeeCode : null,
                    DoctorId = x.DoctorId,
                    DoctorCode = x.Doctor != null ? x.Doctor.DoctorCode : null,
                    UserType = x.UserType,
                    AttendanceDeviceId = x.AttendanceDeviceId,
                    AttendanceDeviceCode = x.AttendanceDevice != null ? x.AttendanceDevice.AttendanceDeviceCode : null,
                    AttendanceDeviceName = x.AttendanceDevice != null ? x.AttendanceDevice.AttendanceDeviceName : null,
                    AttendanceLocationId = x.AttendanceLocationId,
                    AttendanceLocationCode = x.AttendanceLocation != null ? x.AttendanceLocation.AttendanceLocationCode : null,
                    AttendanceLocationName = x.AttendanceLocation != null ? x.AttendanceLocation.AttendanceLocationName : null,
                    HospitalSiteId = x.HospitalSiteId,
                    HospitalSiteCode = x.HospitalSite != null ? x.HospitalSite.SiteCode : null,
                    HospitalSiteName = x.HospitalSite != null ? x.HospitalSite.SiteName : null,
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
                    ProcessingMessage = x.ProcessingMessage,
                    ReceivedAt = x.ReceivedAt,
                    ProcessedAt = x.ProcessedAt,
                    ProcessedAttendanceId = x.ProcessedAttendanceId,
                    ProcessedAttendanceDailyId = x.ProcessedAttendanceDailyId,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            return new PagedResult<AttendanceRawLogResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<AttendanceRawLogDetailResponse?> GetDetailAsync(Guid id)
        {
            return await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new AttendanceRawLogDetailResponse
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserName = x.User != null
                        ? (x.User.DisplayName != string.Empty
                            ? x.User.DisplayName
                            : x.User.UserName)
                        : null,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : null,
                    WorkforceDisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : null,
                    EmployeeId = x.EmployeeId,
                    EmployeeCode = x.Employee != null ? x.Employee.EmployeeCode : null,
                    DoctorId = x.DoctorId,
                    DoctorCode = x.Doctor != null ? x.Doctor.DoctorCode : null,
                    UserType = x.UserType,
                    AttendanceDeviceId = x.AttendanceDeviceId,
                    AttendanceDeviceCode = x.AttendanceDevice != null ? x.AttendanceDevice.AttendanceDeviceCode : null,
                    AttendanceDeviceName = x.AttendanceDevice != null ? x.AttendanceDevice.AttendanceDeviceName : null,
                    AttendanceLocationId = x.AttendanceLocationId,
                    AttendanceLocationCode = x.AttendanceLocation != null ? x.AttendanceLocation.AttendanceLocationCode : null,
                    AttendanceLocationName = x.AttendanceLocation != null ? x.AttendanceLocation.AttendanceLocationName : null,
                    HospitalSiteId = x.HospitalSiteId,
                    HospitalSiteCode = x.HospitalSite != null ? x.HospitalSite.SiteCode : null,
                    HospitalSiteName = x.HospitalSite != null ? x.HospitalSite.SiteName : null,
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
                    EventHash = x.EventHash,
                    IpAddress = x.IpAddress,
                    UserAgent = x.UserAgent,
                    RawPayloadJson = x.RawPayloadJson,
                    ProcessingStatus = x.ProcessingStatus,
                    ProcessingMessage = x.ProcessingMessage,
                    ReceivedAt = x.ReceivedAt,
                    ProcessedAt = x.ProcessedAt,
                    ProcessedAttendanceId = x.ProcessedAttendanceId,
                    ProcessedAttendanceDailyId = x.ProcessedAttendanceDailyId,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy
                })
                .FirstOrDefaultAsync();
        }

        public async Task<AttendanceRawLogServiceResult<AttendanceRawLogCreateResponse>> CreateAsync(
            CreateAttendanceRawLogRequest request,
            Guid actorUserId,
            string? ipAddress,
            string? userAgent)
        {
            var resolutionResult = await ResolveInputAsync(request, actorUserId);
            if (!resolutionResult.Success || resolutionResult.Data == null)
            {
                return AttendanceRawLogServiceResult<AttendanceRawLogCreateResponse>.Fail(
                    resolutionResult.StatusCode,
                    resolutionResult.Message);
            }

            var resolved = resolutionResult.Data;
            var duplicate = await FindDuplicateAsync(
                resolved.AttendanceDeviceId,
                resolved.ExternalLogId,
                resolved.EventHash);

            if (duplicate != null)
            {
                return AttendanceRawLogServiceResult<AttendanceRawLogCreateResponse>.Ok(
                    BuildDuplicateResponse(duplicate),
                    "Raw log attendance sudah pernah diterima. Data existing dikembalikan secara idempotent.");
            }

            var now = DateTime.UtcNow;
            var entity = new TrxAttendanceRawLog
            {
                Id = Guid.NewGuid(),
                UserId = resolved.UserId,
                WorkforceProfileId = resolved.WorkforceProfileId,
                EmployeeId = resolved.EmployeeId,
                DoctorId = resolved.DoctorId,
                UserType = resolved.UserType,
                AttendanceDeviceId = resolved.AttendanceDeviceId,
                AttendanceLocationId = resolved.AttendanceLocationId,
                HospitalSiteId = resolved.HospitalSiteId,
                ExternalLogId = resolved.ExternalLogId,
                ExternalDeviceId = resolved.ExternalDeviceId,
                DeviceUserKey = resolved.DeviceUserKey,
                EventAt = resolved.EventAt,
                EventType = resolved.EventType,
                SourceType = resolved.SourceType,
                Latitude = resolved.Latitude,
                Longitude = resolved.Longitude,
                AccuracyMeters = resolved.AccuracyMeters,
                DistanceMeters = resolved.DistanceMeters,
                IpAddress = NormalizeNullableString(ipAddress),
                UserAgent = LimitLength(NormalizeNullableString(userAgent), 500),
                EventHash = resolved.EventHash,
                RawPayloadJson = resolved.RawPayloadJson,
                ProcessingStatus = resolved.ProcessingStatus,
                ProcessingMessage = resolved.ProcessingMessage,
                ReceivedAt = now,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<TrxAttendanceRawLog>().Add(entity);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                _dbContext.Entry(entity).State = EntityState.Detached;

                duplicate = await FindDuplicateAsync(
                    resolved.AttendanceDeviceId,
                    resolved.ExternalLogId,
                    resolved.EventHash);

                if (duplicate != null)
                {
                    return AttendanceRawLogServiceResult<AttendanceRawLogCreateResponse>.Ok(
                        BuildDuplicateResponse(duplicate),
                        "Raw log attendance sudah diterima oleh request lain. Data existing dikembalikan secara idempotent.");
                }

                return AttendanceRawLogServiceResult<AttendanceRawLogCreateResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    "Raw log attendance gagal disimpan ke database.");
            }

            return AttendanceRawLogServiceResult<AttendanceRawLogCreateResponse>.Ok(
                new AttendanceRawLogCreateResponse
                {
                    Id = entity.Id,
                    IsDuplicate = false,
                    ProcessingStatus = entity.ProcessingStatus,
                    ProcessingMessage = entity.ProcessingMessage,
                    UserId = entity.UserId,
                    WorkforceProfileId = entity.WorkforceProfileId,
                    AttendanceDeviceId = entity.AttendanceDeviceId,
                    AttendanceLocationId = entity.AttendanceLocationId,
                    HospitalSiteId = entity.HospitalSiteId,
                    EventAt = entity.EventAt,
                    EventType = entity.EventType,
                    SourceType = entity.SourceType,
                    EventHash = entity.EventHash ?? string.Empty
                },
                entity.ProcessingStatus == AttendanceValueConstants.RawLogProcessingStatus.Matched
                    ? "Raw log attendance berhasil diterima dan identitas workforce berhasil dicocokkan."
                    : "Raw log attendance berhasil diterima dan menunggu pencocokan identitas workforce.");
        }

        public async Task<AttendanceRawLogBatchResponse> CreateBatchAsync(
            CreateAttendanceRawLogBatchRequest request,
            Guid actorUserId,
            string? ipAddress,
            string? userAgent)
        {
            var result = new AttendanceRawLogBatchResponse
            {
                TotalItem = request.Items.Count
            };

            for (var index = 0; index < request.Items.Count; index++)
            {
                var item = request.Items[index];
                var itemResult = await CreateAsync(item, actorUserId, ipAddress, userAgent);

                if (itemResult.Success && itemResult.Data != null)
                {
                    var duplicate = itemResult.Data.IsDuplicate;
                    result.SuccessCount++;
                    if (duplicate)
                    {
                        result.DuplicateCount++;
                    }

                    result.Items.Add(new AttendanceRawLogBatchItemResponse
                    {
                        Index = index,
                        Success = true,
                        Id = itemResult.Data.Id,
                        IsDuplicate = duplicate,
                        ExistingRawLogId = itemResult.Data.ExistingRawLogId,
                        ExternalLogId = item.ExternalLogId,
                        DeviceUserKey = item.DeviceUserKey,
                        ProcessingStatus = itemResult.Data.ProcessingStatus,
                        Message = itemResult.Message
                    });
                }
                else
                {
                    result.FailedCount++;
                    result.Items.Add(new AttendanceRawLogBatchItemResponse
                    {
                        Index = index,
                        Success = false,
                        IsDuplicate = false,
                        ExternalLogId = item.ExternalLogId,
                        DeviceUserKey = item.DeviceUserKey,
                        Message = itemResult.Message
                    });
                }
            }

            return result;
        }

        public async Task<AttendanceRawLogServiceResult<AttendanceRawLogRetryResponse>> RetryAsync(
            Guid id,
            Guid actorUserId)
        {
            var entity = await _dbContext.Set<TrxAttendanceRawLog>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return AttendanceRawLogServiceResult<AttendanceRawLogRetryResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Raw log attendance tidak ditemukan.");
            }

            if (entity.ProcessingStatus == AttendanceValueConstants.RawLogProcessingStatus.Processed ||
                entity.ProcessedAttendanceDailyId.HasValue)
            {
                return AttendanceRawLogServiceResult<AttendanceRawLogRetryResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Raw log attendance sudah diproses menjadi attendance daily dan tidak dapat dicocokkan ulang melalui endpoint retry.");
            }

            var previousStatus = entity.ProcessingStatus;
            var identity = await ResolveIdentityAsync(
                entity.UserId,
                entity.WorkforceProfileId,
                entity.EmployeeId,
                entity.DoctorId,
                entity.DeviceUserKey,
                entity.SourceType,
                actorUserId);

            if (!identity.Success || identity.Data == null)
            {
                entity.ProcessingStatus = AttendanceValueConstants.RawLogProcessingStatus.Error;
                entity.ProcessingMessage = identity.Message;
            }
            else
            {
                entity.UserId = identity.Data.UserId;
                entity.WorkforceProfileId = identity.Data.WorkforceProfileId;
                entity.EmployeeId = identity.Data.EmployeeId;
                entity.DoctorId = identity.Data.DoctorId;
                entity.UserType = identity.Data.UserType;
                entity.ProcessingStatus = identity.Data.WorkforceProfileId.HasValue
                    ? AttendanceValueConstants.RawLogProcessingStatus.Matched
                    : AttendanceValueConstants.RawLogProcessingStatus.Pending;
                entity.ProcessingMessage = identity.Data.Message;
            }

            entity.ProcessedAt = null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return AttendanceRawLogServiceResult<AttendanceRawLogRetryResponse>.Ok(
                new AttendanceRawLogRetryResponse
                {
                    Id = entity.Id,
                    PreviousProcessingStatus = previousStatus,
                    ProcessingStatus = entity.ProcessingStatus,
                    ProcessingMessage = entity.ProcessingMessage,
                    UserId = entity.UserId,
                    WorkforceProfileId = entity.WorkforceProfileId,
                    EmployeeId = entity.EmployeeId,
                    DoctorId = entity.DoctorId
                },
                entity.ProcessingStatus == AttendanceValueConstants.RawLogProcessingStatus.Matched
                    ? "Pencocokan ulang raw log attendance berhasil."
                    : "Pencocokan ulang selesai, tetapi identitas workforce masih belum ditemukan.");
        }

        private IQueryable<TrxAttendanceRawLog> BuildBaseQuery()
        {
            return _dbContext.Set<TrxAttendanceRawLog>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<TrxAttendanceRawLog> ApplyFilter(
            IQueryable<TrxAttendanceRawLog> query,
            AttendanceRawLogQueryRequest request)
        {
            var range = ResolveDateRange(request.StartDate, request.EndDate, request.CustomPeriod);
            if (range.Start.HasValue)
            {
                query = query.Where(x => x.EventAt >= range.Start.Value);
            }

            if (range.EndExclusive.HasValue)
            {
                query = query.Where(x => x.EventAt < range.EndExclusive.Value);
            }

            if (request.AttendanceDeviceId.HasValue && request.AttendanceDeviceId.Value != Guid.Empty)
            {
                query = query.Where(x => x.AttendanceDeviceId == request.AttendanceDeviceId.Value);
            }

            if (request.AttendanceLocationId.HasValue && request.AttendanceLocationId.Value != Guid.Empty)
            {
                query = query.Where(x => x.AttendanceLocationId == request.AttendanceLocationId.Value);
            }

            if (request.HospitalSiteId.HasValue && request.HospitalSiteId.Value != Guid.Empty)
            {
                query = query.Where(x => x.HospitalSiteId == request.HospitalSiteId.Value);
            }

            if (request.WorkforceProfileId.HasValue && request.WorkforceProfileId.Value != Guid.Empty)
            {
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.EventType))
            {
                var value = request.EventType.Trim();
                query = query.Where(x => x.EventType == value);
            }

            if (!string.IsNullOrWhiteSpace(request.SourceType))
            {
                var value = request.SourceType.Trim();
                query = query.Where(x => x.SourceType == value);
            }

            if (!string.IsNullOrWhiteSpace(request.ProcessingStatus))
            {
                var value = request.ProcessingStatus.Trim();
                query = query.Where(x => x.ProcessingStatus == value);
            }

            if (request.IsMatched.HasValue)
            {
                query = request.IsMatched.Value
                    ? query.Where(x => x.WorkforceProfileId != null)
                    : query.Where(x => x.WorkforceProfileId == null);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    (x.ExternalLogId != null && x.ExternalLogId.ToLower().Contains(keyword)) ||
                    (x.ExternalDeviceId != null && x.ExternalDeviceId.ToLower().Contains(keyword)) ||
                    (x.DeviceUserKey != null && x.DeviceUserKey.ToLower().Contains(keyword)) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.ProfileCode.ToLower().Contains(keyword)) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.DisplayName.ToLower().Contains(keyword)) ||
                    (x.Employee != null && x.Employee.EmployeeCode.ToLower().Contains(keyword)) ||
                    (x.Employee != null && x.Employee.EmployeeNumber.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.DoctorCode.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.DoctorNumber.ToLower().Contains(keyword)) ||
                    (x.AttendanceDevice != null && x.AttendanceDevice.AttendanceDeviceCode.ToLower().Contains(keyword)) ||
                    (x.AttendanceDevice != null && x.AttendanceDevice.AttendanceDeviceName.ToLower().Contains(keyword)) ||
                    (x.ProcessingMessage != null && x.ProcessingMessage.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<TrxAttendanceRawLog> ApplySorting(
            IQueryable<TrxAttendanceRawLog> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "eventAt").Trim().ToLowerInvariant() switch
            {
                "receivedat" => descending
                    ? query.OrderByDescending(x => x.ReceivedAt)
                    : query.OrderBy(x => x.ReceivedAt),
                "workforcedisplayname" => descending
                    ? query.OrderByDescending(x => x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty)
                    : query.OrderBy(x => x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty),
                "attendancedevicename" => descending
                    ? query.OrderByDescending(x => x.AttendanceDevice != null ? x.AttendanceDevice.AttendanceDeviceName : string.Empty)
                    : query.OrderBy(x => x.AttendanceDevice != null ? x.AttendanceDevice.AttendanceDeviceName : string.Empty),
                "sourcetype" => descending
                    ? query.OrderByDescending(x => x.SourceType).ThenByDescending(x => x.EventAt)
                    : query.OrderBy(x => x.SourceType).ThenByDescending(x => x.EventAt),
                "processingstatus" => descending
                    ? query.OrderByDescending(x => x.ProcessingStatus).ThenByDescending(x => x.EventAt)
                    : query.OrderBy(x => x.ProcessingStatus).ThenByDescending(x => x.EventAt),
                _ => descending
                    ? query.OrderByDescending(x => x.EventAt).ThenByDescending(x => x.ReceivedAt)
                    : query.OrderBy(x => x.EventAt).ThenBy(x => x.ReceivedAt)
            };
        }

        private async Task<AttendanceRawLogServiceResult<ResolvedRawLogInput>> ResolveInputAsync(
            CreateAttendanceRawLogRequest request,
            Guid actorUserId)
        {
            if (request.EventAt == default)
            {
                return AttendanceRawLogServiceResult<ResolvedRawLogInput>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Waktu kejadian attendance wajib diisi dengan timezone atau UTC offset yang jelas.");
            }

            var eventType = NormalizeConstant(request.EventType, EventTypes);
            if (eventType == null)
            {
                return AttendanceRawLogServiceResult<ResolvedRawLogInput>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Event type attendance tidak valid.");
            }

            var sourceType = NormalizeConstant(request.SourceType, SourceTypes);
            if (sourceType == null)
            {
                return AttendanceRawLogServiceResult<ResolvedRawLogInput>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Source type attendance tidak valid.");
            }

            if (request.Latitude.HasValue != request.Longitude.HasValue)
            {
                return AttendanceRawLogServiceResult<ResolvedRawLogInput>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Latitude dan longitude harus dikirim bersamaan.");
            }

            var deviceResult = await ResolveDeviceAsync(
                request.AttendanceDeviceId,
                request.ExternalDeviceId,
                sourceType);

            if (!deviceResult.Success)
            {
                return AttendanceRawLogServiceResult<ResolvedRawLogInput>.Fail(
                    deviceResult.StatusCode,
                    deviceResult.Message);
            }

            var device = deviceResult.Data;
            var locationResult = await ResolveLocationAsync(
                request.AttendanceLocationId,
                request.HospitalSiteId,
                sourceType,
                device);

            if (!locationResult.Success || locationResult.Data == null)
            {
                return AttendanceRawLogServiceResult<ResolvedRawLogInput>.Fail(
                    locationResult.StatusCode,
                    locationResult.Message);
            }

            var identityResult = await ResolveIdentityAsync(
                request.UserId,
                request.WorkforceProfileId,
                request.EmployeeId,
                request.DoctorId,
                request.DeviceUserKey,
                sourceType,
                actorUserId);

            if (!identityResult.Success || identityResult.Data == null)
            {
                return AttendanceRawLogServiceResult<ResolvedRawLogInput>.Fail(
                    identityResult.StatusCode,
                    identityResult.Message);
            }

            var identity = identityResult.Data;
            var eventAt = request.EventAt.UtcDateTime;
            var rawPayloadJson = SerializeRawPayload(request.RawPayload);
            var distanceMeters = CalculateDistanceMeters(
                request.Latitude,
                request.Longitude,
                locationResult.Data.Latitude,
                locationResult.Data.Longitude);

            var externalLogId = NormalizeNullableString(request.ExternalLogId);
            var externalDeviceId = NormalizeNullableString(request.ExternalDeviceId)
                ?? device?.ExternalDeviceId;
            var deviceUserKey = NormalizeNullableString(request.DeviceUserKey);

            var eventHash = ComputeEventHash(
                sourceType,
                device?.Id,
                externalDeviceId,
                externalLogId,
                deviceUserKey,
                identity.UserId,
                identity.WorkforceProfileId,
                eventAt,
                eventType,
                request.Latitude,
                request.Longitude);

            return AttendanceRawLogServiceResult<ResolvedRawLogInput>.Ok(
                new ResolvedRawLogInput
                {
                    UserId = identity.UserId,
                    WorkforceProfileId = identity.WorkforceProfileId,
                    EmployeeId = identity.EmployeeId,
                    DoctorId = identity.DoctorId,
                    UserType = identity.UserType,
                    AttendanceDeviceId = device?.Id,
                    AttendanceLocationId = locationResult.Data.AttendanceLocationId,
                    HospitalSiteId = locationResult.Data.HospitalSiteId,
                    ExternalLogId = externalLogId,
                    ExternalDeviceId = externalDeviceId,
                    DeviceUserKey = deviceUserKey,
                    EventAt = eventAt,
                    EventType = eventType,
                    SourceType = sourceType,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    AccuracyMeters = request.AccuracyMeters,
                    DistanceMeters = distanceMeters,
                    RawPayloadJson = rawPayloadJson,
                    EventHash = eventHash,
                    ProcessingStatus = identity.WorkforceProfileId.HasValue
                        ? AttendanceValueConstants.RawLogProcessingStatus.Matched
                        : AttendanceValueConstants.RawLogProcessingStatus.Pending,
                    ProcessingMessage = identity.Message
                },
                "Input raw log attendance berhasil divalidasi.");
        }

        private async Task<AttendanceRawLogServiceResult<MstAttendanceDevice?>> ResolveDeviceAsync(
            Guid? attendanceDeviceId,
            string? externalDeviceId,
            string sourceType)
        {
            var requiresDevice = sourceType == AttendanceValueConstants.RawLogSourceType.Device;
            var normalizedExternalDeviceId = NormalizeNullableString(externalDeviceId);

            if (!attendanceDeviceId.HasValue && normalizedExternalDeviceId == null)
            {
                if (requiresDevice)
                {
                    return AttendanceRawLogServiceResult<MstAttendanceDevice?>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Attendance device id atau external device id wajib diisi untuk source Device.");
                }

                return AttendanceRawLogServiceResult<MstAttendanceDevice?>.Ok(
                    null,
                    "Source attendance tidak memerlukan perangkat terdaftar.");
            }

            var query = _dbContext.Set<MstAttendanceDevice>()
                .AsNoTracking()
                .Include(x => x.AttendanceLocation)
                .Where(x => !x.IsDelete && x.IsActive);

            MstAttendanceDevice? device;
            if (attendanceDeviceId.HasValue && attendanceDeviceId.Value != Guid.Empty)
            {
                device = await query.FirstOrDefaultAsync(x => x.Id == attendanceDeviceId.Value);
            }
            else
            {
                var matches = await query
                    .Where(x =>
                        x.ExternalDeviceId == normalizedExternalDeviceId ||
                        x.AttendanceDeviceCode == normalizedExternalDeviceId ||
                        x.SerialNumber == normalizedExternalDeviceId)
                    .Take(2)
                    .ToListAsync();

                if (matches.Count > 1)
                {
                    return AttendanceRawLogServiceResult<MstAttendanceDevice?>.Fail(
                        StatusCodes.Status409Conflict,
                        "External device id terhubung ke lebih dari satu attendance device aktif.");
                }

                device = matches.FirstOrDefault();
            }

            if (device == null)
            {
                return AttendanceRawLogServiceResult<MstAttendanceDevice?>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance device tidak ditemukan atau tidak aktif.");
            }

            if (normalizedExternalDeviceId != null &&
                attendanceDeviceId.HasValue &&
                !string.Equals(device.ExternalDeviceId, normalizedExternalDeviceId, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(device.AttendanceDeviceCode, normalizedExternalDeviceId, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(device.SerialNumber, normalizedExternalDeviceId, StringComparison.OrdinalIgnoreCase))
            {
                return AttendanceRawLogServiceResult<MstAttendanceDevice?>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Attendance device id dan external device id tidak mengarah ke perangkat yang sama.");
            }

            if (requiresDevice &&
                device.AttendanceLocation != null &&
                !device.AttendanceLocation.AllowDeviceAttendance)
            {
                return AttendanceRawLogServiceResult<MstAttendanceDevice?>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Lokasi attendance tidak mengizinkan absensi melalui perangkat.");
            }

            return AttendanceRawLogServiceResult<MstAttendanceDevice?>.Ok(
                device,
                "Attendance device berhasil ditemukan.");
        }

        private async Task<AttendanceRawLogServiceResult<LocationResolution>> ResolveLocationAsync(
            Guid? attendanceLocationId,
            Guid? hospitalSiteId,
            string sourceType,
            MstAttendanceDevice? device)
        {
            var resolvedLocationId = NormalizeGuid(attendanceLocationId) ?? device?.AttendanceLocationId;
            var resolvedHospitalSiteId = NormalizeGuid(hospitalSiteId) ?? device?.HospitalSiteId;

            MstAttendanceLocation? location = null;
            if (resolvedLocationId.HasValue)
            {
                location = await _dbContext.Set<MstAttendanceLocation>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == resolvedLocationId.Value &&
                        !x.IsDelete &&
                        x.IsActive);

                if (location == null)
                {
                    return AttendanceRawLogServiceResult<LocationResolution>.Fail(
                        StatusCodes.Status404NotFound,
                        "Attendance location tidak ditemukan atau tidak aktif.");
                }

                if (device?.AttendanceLocationId.HasValue == true &&
                    device.AttendanceLocationId.Value != location.Id)
                {
                    return AttendanceRawLogServiceResult<LocationResolution>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Attendance location tidak sesuai dengan lokasi perangkat.");
                }

                if (sourceType == AttendanceValueConstants.RawLogSourceType.Mobile &&
                    !location.AllowMobileAttendance)
                {
                    return AttendanceRawLogServiceResult<LocationResolution>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Attendance location tidak mengizinkan absensi melalui perangkat mobile.");
                }

                resolvedHospitalSiteId ??= location.HospitalSiteId;
            }

            if (device?.HospitalSiteId.HasValue == true &&
                resolvedHospitalSiteId.HasValue &&
                device.HospitalSiteId.Value != resolvedHospitalSiteId.Value)
            {
                return AttendanceRawLogServiceResult<LocationResolution>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Hospital site tidak sesuai dengan hospital site perangkat attendance.");
            }

            if (location?.HospitalSiteId.HasValue == true &&
                resolvedHospitalSiteId.HasValue &&
                location.HospitalSiteId.Value != resolvedHospitalSiteId.Value)
            {
                return AttendanceRawLogServiceResult<LocationResolution>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Hospital site tidak sesuai dengan attendance location.");
            }

            return AttendanceRawLogServiceResult<LocationResolution>.Ok(
                new LocationResolution
                {
                    AttendanceLocationId = location?.Id,
                    HospitalSiteId = resolvedHospitalSiteId,
                    Latitude = location?.Latitude,
                    Longitude = location?.Longitude
                },
                "Lokasi raw log attendance berhasil diselesaikan.");
        }

        private async Task<AttendanceRawLogServiceResult<IdentityResolution>> ResolveIdentityAsync(
            Guid? requestUserId,
            Guid? requestWorkforceProfileId,
            Guid? requestEmployeeId,
            Guid? requestDoctorId,
            string? deviceUserKey,
            string sourceType,
            Guid actorUserId)
        {
            var explicitUserId = NormalizeGuid(requestUserId);
            var explicitWorkforceProfileId = NormalizeGuid(requestWorkforceProfileId);
            var explicitEmployeeId = NormalizeGuid(requestEmployeeId);
            var explicitDoctorId = NormalizeGuid(requestDoctorId);

            if (!explicitUserId.HasValue &&
                !explicitWorkforceProfileId.HasValue &&
                !explicitEmployeeId.HasValue &&
                !explicitDoctorId.HasValue &&
                string.IsNullOrWhiteSpace(deviceUserKey) &&
                sourceType != AttendanceValueConstants.RawLogSourceType.Device &&
                actorUserId != Guid.Empty)
            {
                explicitUserId = actorUserId;
            }

            var candidateWorkforceIds = new HashSet<Guid>();
            var candidateUserIds = new HashSet<Guid>();
            Guid? resolvedEmployeeId = null;
            Guid? resolvedDoctorId = null;

            if (explicitWorkforceProfileId.HasValue)
            {
                candidateWorkforceIds.Add(explicitWorkforceProfileId.Value);
            }

            if (explicitUserId.HasValue)
            {
                var user = await _dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == explicitUserId.Value && x.IsActive);

                if (user == null)
                {
                    return AttendanceRawLogServiceResult<IdentityResolution>.Fail(
                        StatusCodes.Status404NotFound,
                        "User attendance tidak ditemukan atau tidak aktif.");
                }

                candidateUserIds.Add(user.Id);
                if (user.WorkforceProfileId.HasValue)
                {
                    candidateWorkforceIds.Add(user.WorkforceProfileId.Value);
                }

                resolvedEmployeeId = user.EmployeeId;
                resolvedDoctorId = user.DoctorId;
            }

            if (explicitEmployeeId.HasValue)
            {
                var employee = await _dbContext.Set<MstEmployee>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == explicitEmployeeId.Value &&
                        !x.IsDelete &&
                        x.IsActive);

                if (employee == null)
                {
                    return AttendanceRawLogServiceResult<IdentityResolution>.Fail(
                        StatusCodes.Status404NotFound,
                        "Employee attendance tidak ditemukan atau tidak aktif.");
                }

                candidateWorkforceIds.Add(employee.WorkforceProfileId);
                resolvedEmployeeId = employee.Id;
            }

            if (explicitDoctorId.HasValue)
            {
                var doctor = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == explicitDoctorId.Value &&
                        !x.IsDelete &&
                        x.IsActive);

                if (doctor == null)
                {
                    return AttendanceRawLogServiceResult<IdentityResolution>.Fail(
                        StatusCodes.Status404NotFound,
                        "Doctor attendance tidak ditemukan atau tidak aktif.");
                }

                candidateWorkforceIds.Add(doctor.WorkforceProfileId);
                resolvedDoctorId = doctor.Id;
            }

            if (candidateWorkforceIds.Count > 1)
            {
                return AttendanceRawLogServiceResult<IdentityResolution>.Fail(
                    StatusCodes.Status400BadRequest,
                    "User, workforce profile, employee, atau doctor yang dikirim tidak saling berelasi.");
            }

            if (candidateWorkforceIds.Count == 0 && !string.IsNullOrWhiteSpace(deviceUserKey))
            {
                var keyResult = await ResolveIdentityByDeviceUserKeyAsync(deviceUserKey);
                if (!keyResult.Success || keyResult.Data == null)
                {
                    return keyResult;
                }

                if (keyResult.Data.WorkforceProfileId.HasValue)
                {
                    candidateWorkforceIds.Add(keyResult.Data.WorkforceProfileId.Value);
                }

                if (keyResult.Data.UserId.HasValue)
                {
                    candidateUserIds.Add(keyResult.Data.UserId.Value);
                }

                resolvedEmployeeId = keyResult.Data.EmployeeId;
                resolvedDoctorId = keyResult.Data.DoctorId;
            }

            var workforceProfileId = candidateWorkforceIds.Count == 1
                ? candidateWorkforceIds.First()
                : (Guid?)null;

            MstWorkforceProfile? profile = null;
            if (workforceProfileId.HasValue)
            {
                profile = await _dbContext.Set<MstWorkforceProfile>()
                    .AsNoTracking()
                    .Include(x => x.UserAccount)
                    .Include(x => x.Employee)
                    .Include(x => x.Doctor)
                    .FirstOrDefaultAsync(x =>
                        x.Id == workforceProfileId.Value &&
                        !x.IsDelete &&
                        x.IsActive);

                if (profile == null)
                {
                    return AttendanceRawLogServiceResult<IdentityResolution>.Fail(
                        StatusCodes.Status404NotFound,
                        "Workforce profile attendance tidak ditemukan atau tidak aktif.");
                }

                if (profile.UserAccount != null && profile.UserAccount.IsActive)
                {
                    candidateUserIds.Add(profile.UserAccount.Id);
                }

                resolvedEmployeeId ??= profile.Employee?.Id;
                resolvedDoctorId ??= profile.Doctor?.Id;
            }

            if (candidateUserIds.Count > 1)
            {
                return AttendanceRawLogServiceResult<IdentityResolution>.Fail(
                    StatusCodes.Status400BadRequest,
                    "User attendance tidak sesuai dengan workforce profile yang ditemukan.");
            }

            var userId = candidateUserIds.Count == 1
                ? candidateUserIds.First()
                : (Guid?)null;

            UserType? userType = profile?.UserType;
            if (!userType.HasValue && userId.HasValue)
            {
                userType = await _dbContext.Users
                    .AsNoTracking()
                    .Where(x => x.Id == userId.Value)
                    .Select(x => (UserType?)x.UserType)
                    .FirstOrDefaultAsync();
            }

            return AttendanceRawLogServiceResult<IdentityResolution>.Ok(
                new IdentityResolution
                {
                    UserId = userId,
                    WorkforceProfileId = profile?.Id,
                    EmployeeId = resolvedEmployeeId,
                    DoctorId = resolvedDoctorId,
                    UserType = userType,
                    Message = profile != null
                        ? "Identitas workforce berhasil dicocokkan."
                        : string.IsNullOrWhiteSpace(deviceUserKey)
                            ? "Raw log diterima tanpa workforce profile dan menunggu pencocokan identitas."
                            : "Device user key belum terhubung ke workforce profile aktif."
                },
                "Identitas raw log attendance berhasil diselesaikan.");
        }

        private async Task<AttendanceRawLogServiceResult<IdentityResolution>> ResolveIdentityByDeviceUserKeyAsync(
            string deviceUserKey)
        {
            var key = deviceUserKey.Trim().ToLower();
            var candidates = new List<IdentityResolution>();

            var users = await _dbContext.Users
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    (x.UserCode.ToLower() == key ||
                     (x.UserName != null && x.UserName.ToLower() == key)))
                .Take(3)
                .Select(x => new IdentityResolution
                {
                    UserId = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    EmployeeId = x.EmployeeId,
                    DoctorId = x.DoctorId,
                    UserType = x.UserType
                })
                .ToListAsync();
            candidates.AddRange(users);

            var profiles = await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.ProfileCode.ToLower() == key)
                .Take(3)
                .Select(x => new IdentityResolution
                {
                    WorkforceProfileId = x.Id,
                    UserType = x.UserType,
                    EmployeeId = x.Employee != null ? x.Employee.Id : null,
                    DoctorId = x.Doctor != null ? x.Doctor.Id : null,
                    UserId = x.UserAccount != null ? x.UserAccount.Id : null
                })
                .ToListAsync();
            candidates.AddRange(profiles);

            var employees = await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    (x.EmployeeCode.ToLower() == key || x.EmployeeNumber.ToLower() == key))
                .Take(3)
                .Select(x => new IdentityResolution
                {
                    WorkforceProfileId = x.WorkforceProfileId,
                    EmployeeId = x.Id,
                    UserType = x.WorkforceProfile != null ? x.WorkforceProfile.UserType : null,
                    UserId = x.WorkforceProfile != null && x.WorkforceProfile.UserAccount != null
                        ? x.WorkforceProfile.UserAccount.Id
                        : null
                })
                .ToListAsync();
            candidates.AddRange(employees);

            var doctors = await _dbContext.Set<MstDoctor>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    (x.DoctorCode.ToLower() == key || x.DoctorNumber.ToLower() == key))
                .Take(3)
                .Select(x => new IdentityResolution
                {
                    WorkforceProfileId = x.WorkforceProfileId,
                    DoctorId = x.Id,
                    UserType = x.WorkforceProfile != null ? x.WorkforceProfile.UserType : null,
                    UserId = x.WorkforceProfile != null && x.WorkforceProfile.UserAccount != null
                        ? x.WorkforceProfile.UserAccount.Id
                        : null
                })
                .ToListAsync();
            candidates.AddRange(doctors);

            var distinctWorkforceIds = candidates
                .Where(x => x.WorkforceProfileId.HasValue)
                .Select(x => x.WorkforceProfileId!.Value)
                .Distinct()
                .ToList();

            if (distinctWorkforceIds.Count > 1)
            {
                return AttendanceRawLogServiceResult<IdentityResolution>.Fail(
                    StatusCodes.Status409Conflict,
                    "Device user key cocok dengan lebih dari satu workforce profile. Perbaiki mapping identitas perangkat.");
            }

            if (distinctWorkforceIds.Count == 0)
            {
                var userOnlyCandidates = candidates
                    .Where(x => x.UserId.HasValue)
                    .GroupBy(x => x.UserId)
                    .Select(x => x.First())
                    .ToList();

                if (userOnlyCandidates.Count > 1)
                {
                    return AttendanceRawLogServiceResult<IdentityResolution>.Fail(
                        StatusCodes.Status409Conflict,
                        "Device user key cocok dengan lebih dari satu user aktif.");
                }

                if (userOnlyCandidates.Count == 1)
                {
                    userOnlyCandidates[0].Message = "User ditemukan, tetapi belum terhubung ke workforce profile.";
                    return AttendanceRawLogServiceResult<IdentityResolution>.Ok(
                        userOnlyCandidates[0],
                        "User ditemukan tanpa workforce profile.");
                }

                return AttendanceRawLogServiceResult<IdentityResolution>.Ok(
                    new IdentityResolution
                    {
                        Message = "Device user key belum terhubung ke workforce profile aktif."
                    },
                    "Device user key belum ditemukan.");
            }

            var workforceId = distinctWorkforceIds[0];
            var matchingCandidates = candidates
                .Where(x => x.WorkforceProfileId == workforceId)
                .ToList();

            var result = new IdentityResolution
            {
                WorkforceProfileId = workforceId,
                UserId = matchingCandidates.Select(x => x.UserId).FirstOrDefault(x => x.HasValue),
                EmployeeId = matchingCandidates.Select(x => x.EmployeeId).FirstOrDefault(x => x.HasValue),
                DoctorId = matchingCandidates.Select(x => x.DoctorId).FirstOrDefault(x => x.HasValue),
                UserType = matchingCandidates.Select(x => x.UserType).FirstOrDefault(x => x.HasValue),
                Message = "Device user key berhasil dicocokkan ke workforce profile."
            };

            return AttendanceRawLogServiceResult<IdentityResolution>.Ok(
                result,
                "Device user key berhasil dicocokkan.");
        }

        private async Task<TrxAttendanceRawLog?> FindDuplicateAsync(
            Guid? attendanceDeviceId,
            string? externalLogId,
            string eventHash)
        {
            var query = _dbContext.Set<TrxAttendanceRawLog>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (attendanceDeviceId.HasValue && !string.IsNullOrWhiteSpace(externalLogId))
            {
                var byExternalLog = await query.FirstOrDefaultAsync(x =>
                    x.AttendanceDeviceId == attendanceDeviceId.Value &&
                    x.ExternalLogId == externalLogId);

                if (byExternalLog != null)
                {
                    return byExternalLog;
                }
            }

            return await query.FirstOrDefaultAsync(x => x.EventHash == eventHash);
        }

        private static AttendanceRawLogCreateResponse BuildDuplicateResponse(
            TrxAttendanceRawLog existing)
        {
            return new AttendanceRawLogCreateResponse
            {
                Id = existing.Id,
                IsDuplicate = true,
                ExistingRawLogId = existing.Id,
                ProcessingStatus = existing.ProcessingStatus,
                ProcessingMessage = existing.ProcessingMessage,
                UserId = existing.UserId,
                WorkforceProfileId = existing.WorkforceProfileId,
                AttendanceDeviceId = existing.AttendanceDeviceId,
                AttendanceLocationId = existing.AttendanceLocationId,
                HospitalSiteId = existing.HospitalSiteId,
                EventAt = existing.EventAt,
                EventType = existing.EventType,
                SourceType = existing.SourceType,
                EventHash = existing.EventHash ?? string.Empty
            };
        }

        private static string ComputeEventHash(
            string sourceType,
            Guid? attendanceDeviceId,
            string? externalDeviceId,
            string? externalLogId,
            string? deviceUserKey,
            Guid? userId,
            Guid? workforceProfileId,
            DateTime eventAt,
            string eventType,
            decimal? latitude,
            decimal? longitude)
        {
            var raw = string.Join("|", new[]
            {
                sourceType.Trim().ToUpperInvariant(),
                attendanceDeviceId?.ToString("N") ?? NormalizeHashPart(externalDeviceId),
                NormalizeHashPart(externalLogId),
                !string.IsNullOrWhiteSpace(deviceUserKey)
                    ? NormalizeHashPart(deviceUserKey)
                    : workforceProfileId?.ToString("N") ?? userId?.ToString("N") ?? string.Empty,
                eventAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                eventType.Trim().ToUpperInvariant(),
                latitude?.ToString("0.#######", CultureInfo.InvariantCulture) ?? string.Empty,
                longitude?.ToString("0.#######", CultureInfo.InvariantCulture) ?? string.Empty
            });

            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(hash);
        }

        private static string? SerializeRawPayload(JsonElement? rawPayload)
        {
            if (!rawPayload.HasValue ||
                rawPayload.Value.ValueKind == JsonValueKind.Null ||
                rawPayload.Value.ValueKind == JsonValueKind.Undefined)
            {
                return null;
            }

            return rawPayload.Value.GetRawText();
        }

        private static decimal? CalculateDistanceMeters(
            decimal? eventLatitude,
            decimal? eventLongitude,
            decimal? locationLatitude,
            decimal? locationLongitude)
        {
            if (!eventLatitude.HasValue ||
                !eventLongitude.HasValue ||
                !locationLatitude.HasValue ||
                !locationLongitude.HasValue)
            {
                return null;
            }

            const double earthRadiusMeters = 6371000d;
            var lat1 = DegreesToRadians((double)eventLatitude.Value);
            var lon1 = DegreesToRadians((double)eventLongitude.Value);
            var lat2 = DegreesToRadians((double)locationLatitude.Value);
            var lon2 = DegreesToRadians((double)locationLongitude.Value);

            var deltaLat = lat2 - lat1;
            var deltaLon = lon2 - lon1;
            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(lat1) * Math.Cos(lat2) *
                    Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return Math.Round((decimal)(earthRadiusMeters * c), 2);
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180d;
        }

        private static string? NormalizeConstant(string? value, IEnumerable<string> options)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return options.FirstOrDefault(x =>
                string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static string NormalizeHashPart(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToUpperInvariant();
        }

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string? LimitLength(string? value, int maximumLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maximumLength)
            {
                return value;
            }

            return value[..maximumLength];
        }

        private static Guid? NormalizeGuid(Guid? value)
        {
            return !value.HasValue || value.Value == Guid.Empty
                ? null
                : value.Value;
        }

        private static (DateTime? Start, DateTime? EndExclusive) ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue || endDate.HasValue)
            {
                return (
                    startDate.HasValue
                        ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc)
                        : null,
                    endDate.HasValue
                        ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc)
                        : null);
            }

            var today = DateTime.UtcNow.Date;
            return customPeriod?.Trim().ToLowerInvariant() switch
            {
                "today" => (today, today.AddDays(1)),
                "last7days" => (today.AddDays(-6), today.AddDays(1)),
                "thismonth" => (
                    new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                "lastmonth" => (
                    new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1),
                    new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc)),
                _ => (null, null)
            };
        }

        private static List<AttendanceRawLogStringOptionResponse> BuildPeriodOptions()
        {
            return new List<AttendanceRawLogStringOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }

        private static string BuildEventTypeLabel(string value)
        {
            return value switch
            {
                AttendanceValueConstants.RawLogEventType.CheckIn => "Check-in",
                AttendanceValueConstants.RawLogEventType.CheckOut => "Check-out",
                AttendanceValueConstants.RawLogEventType.BreakStart => "Mulai istirahat",
                AttendanceValueConstants.RawLogEventType.BreakEnd => "Selesai istirahat",
                _ => "Tidak diketahui"
            };
        }

        private static string BuildSourceTypeLabel(string value)
        {
            return value switch
            {
                AttendanceValueConstants.RawLogSourceType.Device => "Perangkat attendance",
                AttendanceValueConstants.RawLogSourceType.Mobile => "Mobile",
                AttendanceValueConstants.RawLogSourceType.WebLogin => "Web login",
                AttendanceValueConstants.RawLogSourceType.Import => "Import",
                AttendanceValueConstants.RawLogSourceType.Integration => "Integrasi",
                AttendanceValueConstants.RawLogSourceType.Manual => "Manual",
                _ => value
            };
        }

        private static string BuildProcessingStatusLabel(string value)
        {
            return value switch
            {
                AttendanceValueConstants.RawLogProcessingStatus.Pending => "Menunggu pencocokan",
                AttendanceValueConstants.RawLogProcessingStatus.Matched => "Identitas cocok",
                AttendanceValueConstants.RawLogProcessingStatus.Processed => "Sudah diproses",
                AttendanceValueConstants.RawLogProcessingStatus.Duplicate => "Duplikat",
                AttendanceValueConstants.RawLogProcessingStatus.Rejected => "Ditolak",
                AttendanceValueConstants.RawLogProcessingStatus.Error => "Gagal",
                _ => value
            };
        }

        private sealed class ResolvedRawLogInput
        {
            public Guid? UserId { get; set; }
            public Guid? WorkforceProfileId { get; set; }
            public Guid? EmployeeId { get; set; }
            public Guid? DoctorId { get; set; }
            public UserType? UserType { get; set; }
            public Guid? AttendanceDeviceId { get; set; }
            public Guid? AttendanceLocationId { get; set; }
            public Guid? HospitalSiteId { get; set; }
            public string? ExternalLogId { get; set; }
            public string? ExternalDeviceId { get; set; }
            public string? DeviceUserKey { get; set; }
            public DateTime EventAt { get; set; }
            public string EventType { get; set; } = string.Empty;
            public string SourceType { get; set; } = string.Empty;
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public decimal? AccuracyMeters { get; set; }
            public decimal? DistanceMeters { get; set; }
            public string EventHash { get; set; } = string.Empty;
            public string? RawPayloadJson { get; set; }
            public string ProcessingStatus { get; set; } = string.Empty;
            public string? ProcessingMessage { get; set; }
        }

        private sealed class IdentityResolution
        {
            public Guid? UserId { get; set; }
            public Guid? WorkforceProfileId { get; set; }
            public Guid? EmployeeId { get; set; }
            public Guid? DoctorId { get; set; }
            public UserType? UserType { get; set; }
            public string? Message { get; set; }
        }

        private sealed class LocationResolution
        {
            public Guid? AttendanceLocationId { get; set; }
            public Guid? HospitalSiteId { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
        }
    }
}
