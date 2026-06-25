using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Linq.Expressions;
using System.Security.Claims;

using ResponseKioskScanSessionPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs.KioskScanSessionResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/registration-management/kiosk-scan-sessions")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_REGISTRATION_MANAGEMENT",
        moduleName: "Health Service Registration Management",
        displayName: "Kiosk Scan Session",
        AreaName = "HealthServices",
        ControllerName = "KioskScanSession",
        Description = "Transaksi scan kartu identitas pasien pada proses registrasi",
        SortOrder = 1
    )]
    [Tags("Health Services / Registration Management / Kiosk Scan Session")]
    public class KioskScanSessionController : ControllerBase
    {
        private const string LogCategory = "HealthServices.RegistrationManagement";
        private const string KioskReadPolicy = "KioskRead";
        private const string CodePrefix = "KSC-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public KioskScanSessionController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<KioskScanSessionFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Kiosk Scan Session",
            Description = "Melihat metadata filter kiosk scan session",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new KioskScanSessionFilterMetadataResponse
            {
                DefaultFilter = new KioskScanSessionDefaultFilterResponse(),
                CustomPeriods = new List<KioskScanSessionCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<KioskScanSessionSortOptionResponse>
                {
                    new() { Value = "startedAt", Label = "Waktu mulai" },
                    new() { Value = "completedAt", Label = "Waktu selesai" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "updateDateTime", Label = "Tanggal diperbarui" },
                    new() { Value = "sessionCode", Label = "Kode sesi" },
                    new() { Value = "scanSource", Label = "Sumber scan" },
                    new() { Value = "scanStatus", Label = "Status scan" },
                    new() { Value = "patientName", Label = "Nama pasien" },
                    new() { Value = "medicalRecordNumber", Label = "Nomor rekam medis" },
                    new() { Value = "isPatientFound", Label = "Pasien ditemukan" },
                    new() { Value = "isManualInput", Label = "Input manual" },
                    new() { Value = "isUsedForRegistration", Label = "Dipakai registrasi" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                RelationFilters = new List<KioskScanSessionRelationFilterResponse>
                {
                    new()
                    {
                        Value = "kioskDeviceId",
                        Label = "Kiosk Device",
                        Endpoint = "/api/v1/administrator/master-data/kiosk-devices/options"
                    },
                    new()
                    {
                        Value = "patientId",
                        Label = "Patient",
                        Endpoint = "/api/v1/health-services/patient-management/master-data/patients/options"
                    }
                },
                ScanSourceOptions = BuildEnumOptions<KioskScanSource>(),
                ScanStatusOptions = BuildEnumOptions<KioskScanSessionStatus>(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "KioskScanSession.GetFilterMetadata",
                "Mengambil metadata filter kiosk scan session.",
                result
            );

            return Ok(ApiResponse<KioskScanSessionFilterMetadataResponse>.Ok(
                result,
                "Metadata filter kiosk scan session berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<KioskScanSessionSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Kiosk Scan Session",
            Description = "Melihat ringkasan kiosk scan session",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("KioskScanSession", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? kioskDeviceId,
            [FromQuery] Guid? patientId)
        {
            var query = BuildBaseQuery();

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyRelationFilter(query, kioskDeviceId, patientId);

            var result = new KioskScanSessionSummaryResponse
            {
                TotalSession = await query.CountAsync(),
                StartedSession = await query.CountAsync(x => x.ScanStatus == KioskScanSessionStatus.Started),
                SuccessSession = await query.CountAsync(x => x.ScanStatus == KioskScanSessionStatus.Success),
                FailedSession = await query.CountAsync(x => x.ScanStatus == KioskScanSessionStatus.Failed),
                CancelledSession = await query.CountAsync(x => x.ScanStatus == KioskScanSessionStatus.Cancelled),
                ManualInputSession = await query.CountAsync(x => x.IsManualInput),
                PatientFoundSession = await query.CountAsync(x => x.IsPatientFound),
                PatientNotFoundSession = await query.CountAsync(x => !x.IsPatientFound),
                UsedForRegistrationSession = await query.CountAsync(x => x.IsUsedForRegistration),
                UnusedForRegistrationSession = await query.CountAsync(x => !x.IsUsedForRegistration)
            };

            return Ok(ApiResponse<KioskScanSessionSummaryResponse>.Ok(
                result,
                "Ringkasan kiosk scan session berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseKioskScanSessionPagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Kiosk Scan Session",
            Description = "Melihat data kiosk scan session",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("KioskScanSession", "Read")]
        public async Task<IActionResult> GetSessions(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? kioskDeviceId,
            [FromQuery] Guid? patientId,
            [FromQuery] KioskScanSource? scanSource,
            [FromQuery] KioskScanSessionStatus? scanStatus,
            [FromQuery] bool? isPatientFound,
            [FromQuery] bool? isManualInput,
            [FromQuery] bool? isUsedForRegistration,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "startedAt",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyRelationFilter(query, kioskDeviceId, patientId);
            query = ApplyStandardFilter(
                query,
                scanSource,
                scanStatus,
                isPatientFound,
                isManualInput,
                isUsedForRegistration,
                search
            );

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities
                    .SelectMany(x => new[] { x.CreateBy, x.UpdateBy })
                    .Where(x => x != Guid.Empty)
            );

            var items = entities
                .Select(x => MapResponse(x, actorNames))
                .ToList();

            var result = new ResponseKioskScanSessionPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseKioskScanSessionPagedResult>.Ok(
                result,
                "Data kiosk scan session berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<KioskScanSessionOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Kiosk Scan Session",
            Description = "Melihat data pilihan kiosk scan session",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        public async Task<IActionResult> GetSessionOptions(
            [FromQuery] bool onlyUsableForRegistration = false,
            [FromQuery] Guid? kioskDeviceId = null,
            [FromQuery] Guid? patientId = null,
            [FromQuery] KioskScanSessionStatus? scanStatus = null,
            [FromQuery] bool? isPatientFound = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyRelationFilter(query, kioskDeviceId, patientId);
            query = ApplyStandardFilter(
                query,
                scanSource: null,
                scanStatus: scanStatus,
                isPatientFound: isPatientFound,
                isManualInput: null,
                isUsedForRegistration: onlyUsableForRegistration ? false : null,
                search: search
            );

            if (onlyUsableForRegistration)
            {
                query = query.Where(x =>
                    x.ScanStatus == KioskScanSessionStatus.Success &&
                    !x.IsCancel);
            }

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.StartedAt)
                .ThenByDescending(x => x.SessionCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new KioskScanSessionOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(MapOptionResponse).ToList()
            };

            return Ok(ApiResponse<KioskScanSessionOptionPagedResponse>.Ok(
                result,
                "Data pilihan kiosk scan session berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<KioskScanSessionDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Kiosk Scan Session",
            Description = "Melihat detail kiosk scan session",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("KioskScanSession", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kiosk scan session tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var result = MapDetailResponse(entity, actorNames);

            return Ok(ApiResponse<KioskScanSessionDetailResponse>.Ok(
                result,
                "Detail kiosk scan session berhasil diambil."
            ));
        }

        [HttpPost("scan-result")]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<KioskScanSessionCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Create",
            "Create Kiosk Scan Session",
            Description = "Menerima hasil scan kartu pasien",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        public async Task<IActionResult> CreateFromScanResult([FromBody] CreateKioskScanSessionRequest request)
        {
            var validation = await ValidateRequestAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data scan tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var hasReadableIdentity = HasAnyReadableIdentity(request);

            var patient = hasReadableIdentity
                ? await FindPatientAsync(
                    request.IdentityNumber,
                    request.CardNumber,
                    request.MemberNumber,
                    request.InsuranceCardNumber)
                : null;

            var isPatientFound = patient != null;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var entity = new TrxKioskScanSession
                {
                    Id = Guid.NewGuid(),
                    SessionCode = await GenerateSessionCodeAsync(),
                    ScanSource = request.IsManualInput ? KioskScanSource.ManualInput : request.ScanSource,
                    ScanStatus = hasReadableIdentity
                        ? KioskScanSessionStatus.Success
                        : KioskScanSessionStatus.Failed,
                    KioskDeviceId = NormalizeNullableGuid(request.KioskDeviceId),
                    IdentityScannerProfileId = NormalizeNullableGuid(request.IdentityScannerProfileId),
                    PatientId = patient?.Id,
                    IdentityType = NormalizeNullableText(request.IdentityType),
                    IdentityNumber = NormalizeNullableText(request.IdentityNumber),
                    CardNumber = NormalizeNullableText(request.CardNumber),
                    MemberNumber = NormalizeNullableText(request.MemberNumber),
                    InsuranceCardNumber = NormalizeNullableText(request.InsuranceCardNumber),
                    FullName = NormalizeNullableText(request.FullName),
                    BirthDate = request.BirthDate,
                    GenderText = NormalizeNullableText(request.GenderText),
                    Address = NormalizeNullableText(request.Address),
                    RawScanText = NormalizeNullableText(request.RawScanText),
                    ParsedJson = NormalizeNullableText(request.ParsedJson),
                    ScanImagePath = NormalizeNullableText(request.ScanImagePath),
                    ScanImageContentType = NormalizeNullableText(request.ScanImageContentType),
                    FailureReason = hasReadableIdentity
                        ? null
                        : NormalizeNullableText(request.FailureReason) ?? "Data identitas tidak terbaca.",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers.UserAgent.ToString(),
                    StartedAt = now,
                    CompletedAt = now,
                    IsPatientFound = isPatientFound,
                    IsManualInput = request.IsManualInput,
                    IsUsedForRegistration = false,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<TrxKioskScanSession>().Add(entity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = new KioskScanSessionCreateResponse
                {
                    Id = entity.Id,
                    SessionCode = entity.SessionCode,
                    ScanStatus = entity.ScanStatus,
                    ScanStatusName = BuildEnumLabel(entity.ScanStatus),
                    IsPatientFound = entity.IsPatientFound,
                    PatientId = patient?.Id,
                    MedicalRecordNumber = patient?.MedicalRecordNumber,
                    PatientName = patient?.FullName
                };

                await _loggerService.InfoAsync(
                    LogCategory,
                    "KioskScanSession.CreateFromScanResult",
                    "Menerima hasil scan kartu pasien.",
                    response
                );

                return Ok(ApiResponse<KioskScanSessionCreateResponse>.Ok(
                    response,
                    isPatientFound
                        ? "Hasil scan berhasil diterima dan pasien ditemukan."
                        : "Hasil scan berhasil diterima, pasien belum ditemukan."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "KioskScanSession.CreateFromScanResult",
                    "Gagal menerima hasil scan kartu pasien.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat menyimpan hasil scan."
                    )
                );
            }
        }

        [HttpPatch("{id:guid}/mark-used-for-registration")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Kiosk Scan Session",
            Description = "Menandai hasil scan sudah dipakai untuk registrasi",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("KioskScanSession", "Update")]
        public async Task<IActionResult> MarkUsedForRegistration(
            Guid id,
            [FromBody] MarkKioskScanSessionUsedForRegistrationRequest? request = null)
        {
            var entity = await _dbContext.Set<TrxKioskScanSession>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kiosk scan session tidak ditemukan."
                ));
            }

            if (entity.IsCancel || entity.ScanStatus == KioskScanSessionStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Kiosk scan session yang sudah dibatalkan tidak dapat dipakai untuk registrasi."
                ));
            }

            if (entity.ScanStatus == KioskScanSessionStatus.Failed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Kiosk scan session gagal tidak dapat dipakai untuk registrasi."
                ));
            }

            if (entity.IsUsedForRegistration)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Kiosk scan session sudah pernah dipakai untuk registrasi."
                ));
            }

            var normalizedPatientId = NormalizeNullableGuid(request?.PatientId);

            if (normalizedPatientId.HasValue)
            {
                var patientExists = await _dbContext.Set<MstPatient>()
                    .AnyAsync(x =>
                        x.Id == normalizedPatientId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!patientExists)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Patient tidak valid atau tidak aktif."
                    ));
                }

                entity.PatientId = normalizedPatientId.Value;
                entity.IsPatientFound = true;
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsUsedForRegistration = true;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Kiosk scan session berhasil ditandai sudah dipakai untuk registrasi."
            ));
        }

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Cancel",
            "Cancel Kiosk Scan Session",
            Description = "Membatalkan kiosk scan session",
            AccessType = AccessTypes.Update,
            SortOrder = 4
        )]
        [AccessPermission("KioskScanSession", "Update")]
        public async Task<IActionResult> CancelSession(
            Guid id,
            [FromBody] CancelKioskScanSessionRequest? request = null)
        {
            var entity = await _dbContext.Set<TrxKioskScanSession>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kiosk scan session tidak ditemukan."
                ));
            }

            if (entity.IsUsedForRegistration)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Kiosk scan session yang sudah dipakai untuk registrasi tidak dapat dibatalkan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.ScanStatus = KioskScanSessionStatus.Cancelled;
            entity.IsCancel = true;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.FailureReason = NormalizeNullableText(request?.CancelReason) ?? entity.FailureReason;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Kiosk scan session berhasil dibatalkan."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Kiosk Scan Session",
            Description = "Menghapus kiosk scan session",
            AccessType = AccessTypes.Delete,
            SortOrder = 5
        )]
        [AccessPermission("KioskScanSession", "Delete")]
        public async Task<IActionResult> DeleteSession(
            Guid id,
            [FromBody] DeleteKioskScanSessionRequest? request = null)
        {
            var entity = await _dbContext.Set<TrxKioskScanSession>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kiosk scan session tidak ditemukan."
                ));
            }

            if (entity.IsUsedForRegistration)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Kiosk scan session yang sudah dipakai untuk registrasi tidak dapat dihapus."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.FailureReason = NormalizeNullableText(request?.DeleteReason) ?? entity.FailureReason;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Kiosk scan session berhasil dihapus."
            ));
        }

        private IQueryable<TrxKioskScanSession> BuildBaseQuery()
        {
            return _dbContext.Set<TrxKioskScanSession>()
                .AsNoTracking()
                .Include(x => x.KioskDevice)
                .Include(x => x.IdentityScannerProfile)
                .Include(x => x.Patient)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<TrxKioskScanSession> ApplyDateFilter(
            IQueryable<TrxKioskScanSession> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue)
            {
                var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(x => x.StartedAt >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(x => x.StartedAt < end);
            }

            if (!startDate.HasValue &&
                !endDate.HasValue &&
                !string.IsNullOrWhiteSpace(customPeriod))
            {
                var today = AppDateTimeHelper.OperationalDate();

                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        query = query.Where(x =>
                            x.StartedAt >= today &&
                            x.StartedAt < today.AddDays(1));
                        break;

                    case "last7days":
                        query = query.Where(x =>
                            x.StartedAt >= today.AddDays(-6) &&
                            x.StartedAt < today.AddDays(1));
                        break;

                    case "thismonth":
                        var thisMonthStart = new DateTime(
                            today.Year,
                            today.Month,
                            1,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc
                        );

                        query = query.Where(x =>
                            x.StartedAt >= thisMonthStart &&
                            x.StartedAt < thisMonthStart.AddMonths(1));
                        break;

                    case "lastmonth":
                        var currentMonthStart = new DateTime(
                            today.Year,
                            today.Month,
                            1,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc
                        );

                        var lastMonthStart = currentMonthStart.AddMonths(-1);

                        query = query.Where(x =>
                            x.StartedAt >= lastMonthStart &&
                            x.StartedAt < currentMonthStart);
                        break;
                }
            }

            return query;
        }

        private static IQueryable<TrxKioskScanSession> ApplyRelationFilter(
            IQueryable<TrxKioskScanSession> query,
            Guid? kioskDeviceId,
            Guid? patientId)
        {
            var normalizedKioskDeviceId = NormalizeNullableGuid(kioskDeviceId);

            if (normalizedKioskDeviceId.HasValue)
            {
                query = query.Where(x => x.KioskDeviceId == normalizedKioskDeviceId.Value);
            }

            var normalizedPatientId = NormalizeNullableGuid(patientId);

            if (normalizedPatientId.HasValue)
            {
                query = query.Where(x => x.PatientId == normalizedPatientId.Value);
            }

            return query;
        }

        private static IQueryable<TrxKioskScanSession> ApplyStandardFilter(
            IQueryable<TrxKioskScanSession> query,
            KioskScanSource? scanSource,
            KioskScanSessionStatus? scanStatus,
            bool? isPatientFound,
            bool? isManualInput,
            bool? isUsedForRegistration,
            string? search)
        {
            if (scanSource.HasValue)
            {
                query = query.Where(x => x.ScanSource == scanSource.Value);
            }

            if (scanStatus.HasValue)
            {
                query = query.Where(x => x.ScanStatus == scanStatus.Value);
            }

            if (isPatientFound.HasValue)
            {
                query = query.Where(x => x.IsPatientFound == isPatientFound.Value);
            }

            if (isManualInput.HasValue)
            {
                query = query.Where(x => x.IsManualInput == isManualInput.Value);
            }

            if (isUsedForRegistration.HasValue)
            {
                query = query.Where(x => x.IsUsedForRegistration == isUsedForRegistration.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                var matchedSources = Enum.GetValues<KioskScanSource>()
                    .Where(x =>
                        x.ToString().ToLower().Contains(keyword) ||
                        BuildEnumLabel(x).ToLower().Contains(keyword))
                    .ToList();

                var matchedStatuses = Enum.GetValues<KioskScanSessionStatus>()
                    .Where(x =>
                        x.ToString().ToLower().Contains(keyword) ||
                        BuildEnumLabel(x).ToLower().Contains(keyword))
                    .ToList();

                query = query.Where(x =>
                    x.SessionCode.ToLower().Contains(keyword) ||
                    (x.IdentityType != null && x.IdentityType.ToLower().Contains(keyword)) ||
                    (x.IdentityNumber != null && x.IdentityNumber.ToLower().Contains(keyword)) ||
                    (x.CardNumber != null && x.CardNumber.ToLower().Contains(keyword)) ||
                    (x.MemberNumber != null && x.MemberNumber.ToLower().Contains(keyword)) ||
                    (x.InsuranceCardNumber != null && x.InsuranceCardNumber.ToLower().Contains(keyword)) ||
                    (x.FullName != null && x.FullName.ToLower().Contains(keyword)) ||
                    (x.KioskDevice != null && x.KioskDevice.DeviceName.ToLower().Contains(keyword)) ||
                    (x.IdentityScannerProfile != null && x.IdentityScannerProfile.ProfileName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.PatientCode.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    matchedSources.Contains(x.ScanSource) ||
                    matchedStatuses.Contains(x.ScanStatus));
            }

            return query;
        }

        private async Task<MstPatient?> FindPatientAsync(
            string? identityNumber,
            string? cardNumber,
            string? memberNumber,
            string? insuranceCardNumber)
        {
            var normalizedIdentityNumber = NormalizeNullableText(identityNumber)?.ToLowerInvariant();
            var normalizedCardNumber = NormalizeNullableText(cardNumber)?.ToLowerInvariant();
            var normalizedMemberNumber = NormalizeNullableText(memberNumber)?.ToLowerInvariant();
            var normalizedInsuranceCardNumber = NormalizeNullableText(insuranceCardNumber)?.ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(normalizedIdentityNumber))
            {
                var patient = await _dbContext.Set<MstPatient>()
                    .FirstOrDefaultAsync(x =>
                        !x.IsDelete &&
                        x.IsActive &&
                        x.IdentityNumber != null &&
                        x.IdentityNumber.ToLower() == normalizedIdentityNumber);

                if (patient != null)
                {
                    return patient;
                }

                var patientFromDocument = await _dbContext.Set<MstPatientIdentityDocument>()
                    .Where(x =>
                        !x.IsDelete &&
                        x.IsActive &&
                        x.IdentityNumber.ToLower() == normalizedIdentityNumber &&
                        x.Patient != null &&
                        x.Patient.IsActive &&
                        !x.Patient.IsDelete)
                    .Select(x => x.Patient)
                    .FirstOrDefaultAsync();

                if (patientFromDocument != null)
                {
                    return patientFromDocument;
                }
            }

            if (!string.IsNullOrWhiteSpace(normalizedCardNumber))
            {
                var patientFromInsuranceCard = await _dbContext.Set<MstPatientInsurance>()
                    .Where(x =>
                        !x.IsDelete &&
                        x.IsActive &&
                        x.CardNumber != null &&
                        x.CardNumber.ToLower() == normalizedCardNumber &&
                        x.Patient != null &&
                        x.Patient.IsActive &&
                        !x.Patient.IsDelete)
                    .Select(x => x.Patient)
                    .FirstOrDefaultAsync();

                if (patientFromInsuranceCard != null)
                {
                    return patientFromInsuranceCard;
                }
            }

            if (!string.IsNullOrWhiteSpace(normalizedMemberNumber))
            {
                var patientFromMembership = await _dbContext.Set<MstPatientMembership>()
                    .Where(x =>
                        !x.IsDelete &&
                        x.IsActive &&
                        x.MemberNumber.ToLower() == normalizedMemberNumber &&
                        x.Patient != null &&
                        x.Patient.IsActive &&
                        !x.Patient.IsDelete)
                    .Select(x => x.Patient)
                    .FirstOrDefaultAsync();

                if (patientFromMembership != null)
                {
                    return patientFromMembership;
                }

                var patientFromInsuranceMember = await _dbContext.Set<MstPatientInsurance>()
                    .Where(x =>
                        !x.IsDelete &&
                        x.IsActive &&
                        x.MemberNumber != null &&
                        x.MemberNumber.ToLower() == normalizedMemberNumber &&
                        x.Patient != null &&
                        x.Patient.IsActive &&
                        !x.Patient.IsDelete)
                    .Select(x => x.Patient)
                    .FirstOrDefaultAsync();

                if (patientFromInsuranceMember != null)
                {
                    return patientFromInsuranceMember;
                }
            }

            if (!string.IsNullOrWhiteSpace(normalizedInsuranceCardNumber))
            {
                var patientFromInsurance = await _dbContext.Set<MstPatientInsurance>()
                    .Where(x =>
                        !x.IsDelete &&
                        x.IsActive &&
                        x.CardNumber != null &&
                        x.CardNumber.ToLower() == normalizedInsuranceCardNumber &&
                        x.Patient != null &&
                        x.Patient.IsActive &&
                        !x.Patient.IsDelete)
                    .Select(x => x.Patient)
                    .FirstOrDefaultAsync();

                if (patientFromInsurance != null)
                {
                    return patientFromInsurance;
                }
            }

            return null;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            CreateKioskScanSessionRequest request)
        {
            if (!Enum.IsDefined(typeof(KioskScanSource), request.ScanSource))
            {
                return (false, "Sumber scan tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (request.KioskDeviceId.HasValue && request.KioskDeviceId.Value != Guid.Empty)
            {
                var deviceExists = await _dbContext.Set<MstKioskDevice>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == request.KioskDeviceId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!deviceExists)
                {
                    return (false, "Kiosk device tidak valid atau tidak aktif.");
                }
            }

            if (request.IdentityScannerProfileId.HasValue && request.IdentityScannerProfileId.Value != Guid.Empty)
            {
                var scannerProfileExists = await _dbContext.Set<MstIdentityScannerProfile>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == request.IdentityScannerProfileId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!scannerProfileExists)
                {
                    return (false, "Identity scanner profile tidak valid atau tidak aktif.");
                }
            }

            var hasReadableIdentity = HasAnyReadableIdentity(request);

            if (request.IsManualInput && !hasReadableIdentity)
            {
                return (false, "Input manual wajib memiliki minimal salah satu data: NIK, nomor kartu, nomor member, nomor kartu asuransi, atau raw scan text.");
            }

            if (!request.IsManualInput &&
                request.ScanSource == KioskScanSource.Unknown &&
                !hasReadableIdentity)
            {
                return (false, "Sumber scan wajib dipilih atau minimal ada data hasil scan.");
            }

            return (true, null);
        }

        private static bool HasAnyReadableIdentity(CreateKioskScanSessionRequest request)
        {
            return
                !string.IsNullOrWhiteSpace(request.IdentityNumber) ||
                !string.IsNullOrWhiteSpace(request.CardNumber) ||
                !string.IsNullOrWhiteSpace(request.MemberNumber) ||
                !string.IsNullOrWhiteSpace(request.InsuranceCardNumber) ||
                !string.IsNullOrWhiteSpace(request.FullName) ||
                !string.IsNullOrWhiteSpace(request.RawScanText);
        }

        private async Task<string> GenerateSessionCodeAsync()
        {
            return await GenerateRunningCodeAsync(
                selector: x => x.SessionCode,
                prefix: CodePrefix
            );
        }

        private async Task<string> GenerateRunningCodeAsync(
            Expression<Func<TrxKioskScanSession, string>> selector,
            string prefix)
        {
            var existingCodes = await _dbContext.Set<TrxKioskScanSession>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Select(selector)
                .Where(x => x.StartsWith(prefix))
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(x => x.Replace(prefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return prefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(
            IEnumerable<Guid> actorIds)
        {
            var ids = actorIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (!ids.Any())
            {
                return new Dictionary<Guid, string?>();
            }

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    Name =
                        x.DisplayName ??
                        x.UserName ??
                        x.Email ??
                        x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static KioskScanSessionResponse MapResponse(
            TrxKioskScanSession entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new KioskScanSessionResponse
            {
                Id = entity.Id,
                SessionCode = entity.SessionCode,
                ScanSource = entity.ScanSource,
                ScanSourceName = BuildEnumLabel(entity.ScanSource),
                ScanStatus = entity.ScanStatus,
                ScanStatusName = BuildEnumLabel(entity.ScanStatus),
                KioskDeviceId = entity.KioskDeviceId,
                KioskDeviceName = entity.KioskDevice?.DeviceName,
                IdentityScannerProfileId = entity.IdentityScannerProfileId,
                IdentityScannerProfileName = entity.IdentityScannerProfile?.ProfileName,
                ScanImagePath = entity.ScanImagePath,
                ScanImageContentType = entity.ScanImageContentType,
                PatientId = entity.PatientId,
                PatientCode = entity.Patient?.PatientCode,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber,
                PatientName = entity.Patient?.FullName,
                IdentityType = entity.IdentityType,
                IdentityNumber = entity.IdentityNumber,
                CardNumber = entity.CardNumber,
                MemberNumber = entity.MemberNumber,
                InsuranceCardNumber = entity.InsuranceCardNumber,
                IsPatientFound = entity.IsPatientFound,
                IsManualInput = entity.IsManualInput,
                IsUsedForRegistration = entity.IsUsedForRegistration,
                StartedAt = entity.StartedAt,
                CompletedAt = entity.CompletedAt,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static KioskScanSessionDetailResponse MapDetailResponse(
            TrxKioskScanSession entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new KioskScanSessionDetailResponse
            {
                Id = entity.Id,
                SessionCode = entity.SessionCode,
                ScanSource = entity.ScanSource,
                ScanSourceName = BuildEnumLabel(entity.ScanSource),
                ScanStatus = entity.ScanStatus,
                ScanStatusName = BuildEnumLabel(entity.ScanStatus),
                KioskDeviceId = entity.KioskDeviceId,
                KioskDeviceName = entity.KioskDevice?.DeviceName,
                IdentityScannerProfileId = entity.IdentityScannerProfileId,
                IdentityScannerProfileName = entity.IdentityScannerProfile?.ProfileName,
                ScanImagePath = entity.ScanImagePath,
                ScanImageContentType = entity.ScanImageContentType,
                PatientId = entity.PatientId,
                PatientCode = entity.Patient?.PatientCode,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber,
                PatientName = entity.Patient?.FullName,
                IdentityType = entity.IdentityType,
                IdentityNumber = entity.IdentityNumber,
                CardNumber = entity.CardNumber,
                MemberNumber = entity.MemberNumber,
                InsuranceCardNumber = entity.InsuranceCardNumber,
                FullName = entity.FullName,
                BirthDate = entity.BirthDate,
                GenderText = entity.GenderText,
                Address = entity.Address,
                RawScanText = entity.RawScanText,
                ParsedJson = entity.ParsedJson,
                FailureReason = entity.FailureReason,
                IpAddress = entity.IpAddress,
                UserAgent = entity.UserAgent,
                IsPatientFound = entity.IsPatientFound,
                IsManualInput = entity.IsManualInput,
                IsUsedForRegistration = entity.IsUsedForRegistration,
                StartedAt = entity.StartedAt,
                CompletedAt = entity.CompletedAt,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static KioskScanSessionOptionResponse MapOptionResponse(TrxKioskScanSession entity)
        {
            return new KioskScanSessionOptionResponse
            {
                Id = entity.Id,
                SessionCode = entity.SessionCode,
                ScanSource = entity.ScanSource,
                ScanSourceName = BuildEnumLabel(entity.ScanSource),
                ScanStatus = entity.ScanStatus,
                ScanStatusName = BuildEnumLabel(entity.ScanStatus),
                PatientId = entity.PatientId,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber,
                PatientName = entity.Patient?.FullName,
                IdentityNumber = entity.IdentityNumber,
                CardNumber = entity.CardNumber,
                MemberNumber = entity.MemberNumber,
                IsPatientFound = entity.IsPatientFound,
                IsUsedForRegistration = entity.IsUsedForRegistration,
                StartedAt = entity.StartedAt
            };
        }

        private static IQueryable<TrxKioskScanSession> ApplySorting(
            IQueryable<TrxKioskScanSession> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase
            );

            return (sortBy ?? "startedAt").Trim().ToLowerInvariant() switch
            {
                "sessioncode" => isDescending
                    ? query.OrderByDescending(x => x.SessionCode)
                    : query.OrderBy(x => x.SessionCode),

                "scansource" => isDescending
                    ? query.OrderByDescending(x => x.ScanSource).ThenByDescending(x => x.StartedAt)
                    : query.OrderBy(x => x.ScanSource).ThenBy(x => x.StartedAt),

                "scanstatus" => isDescending
                    ? query.OrderByDescending(x => x.ScanStatus).ThenByDescending(x => x.StartedAt)
                    : query.OrderBy(x => x.ScanStatus).ThenBy(x => x.StartedAt),

                "patientname" => isDescending
                    ? query.OrderByDescending(x => x.Patient != null ? x.Patient.FullName : string.Empty)
                    : query.OrderBy(x => x.Patient != null ? x.Patient.FullName : string.Empty),

                "medicalrecordnumber" => isDescending
                    ? query.OrderByDescending(x => x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty)
                    : query.OrderBy(x => x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty),

                "completedat" => isDescending
                    ? query.OrderByDescending(x => x.CompletedAt).ThenByDescending(x => x.StartedAt)
                    : query.OrderBy(x => x.CompletedAt).ThenBy(x => x.StartedAt),

                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "updatedatetime" => isDescending
                    ? query.OrderByDescending(x => x.UpdateDateTime).ThenByDescending(x => x.StartedAt)
                    : query.OrderBy(x => x.UpdateDateTime).ThenBy(x => x.StartedAt),

                "ispatientfound" => isDescending
                    ? query.OrderByDescending(x => x.IsPatientFound).ThenByDescending(x => x.StartedAt)
                    : query.OrderBy(x => x.IsPatientFound).ThenBy(x => x.StartedAt),

                "ismanualinput" => isDescending
                    ? query.OrderByDescending(x => x.IsManualInput).ThenByDescending(x => x.StartedAt)
                    : query.OrderBy(x => x.IsManualInput).ThenBy(x => x.StartedAt),

                "isusedforregistration" => isDescending
                    ? query.OrderByDescending(x => x.IsUsedForRegistration).ThenByDescending(x => x.StartedAt)
                    : query.OrderBy(x => x.IsUsedForRegistration).ThenBy(x => x.StartedAt),

                _ => isDescending
                    ? query.OrderByDescending(x => x.StartedAt).ThenByDescending(x => x.SessionCode)
                    : query.OrderBy(x => x.StartedAt).ThenBy(x => x.SessionCode)
            };
        }

        private static List<KioskScanSessionEnumOptionResponse> BuildEnumOptions<TEnum>()
            where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new KioskScanSessionEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = BuildEnumLabel(x)
                })
                .ToList();
        }

        private static string BuildEnumLabel<TEnum>(TEnum value)
            where TEnum : Enum
        {
            return SplitPascalCase(value.ToString());
        }

        private static string SplitPascalCase(string value)
        {
            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private static string? GetActorName(
            IReadOnlyDictionary<Guid, string?> actorNames,
            Guid actorId)
        {
            if (actorId == Guid.Empty)
            {
                return null;
            }

            return actorNames.TryGetValue(actorId, out var actorName)
                ? actorName
                : null;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 25;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            return (pageNumber, pageSize);
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
            {
                return null;
            }

            return value.Value;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
