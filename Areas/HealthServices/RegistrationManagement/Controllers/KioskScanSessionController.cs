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
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseKioskScanSessionPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs.KioskScanSessionResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/registration/kiosk-scan-sessions")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_REGISTRATION",
        moduleName: "Health Service Registration",
        displayName: "Kiosk Scan Session",
        AreaName = "HealthServices",
        ControllerName = "KioskScanSession",
        Description = "Transaksi scan kartu identitas pasien pada proses registrasi",
        SortOrder = 1
    )]
    [Tags("Health Services / Registration / Kiosk Scan Session")]
    public class KioskScanSessionController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Registration";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public KioskScanSessionController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<KioskScanSessionSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Kiosk Scan Session", Description = "Melihat ringkasan kiosk scan session", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("KioskScanSession", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<TrxKioskScanSession>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new KioskScanSessionSummaryResponse
            {
                TotalSession = await query.CountAsync(),
                SuccessSession = await query.CountAsync(x => x.ScanStatus == KioskScanSessionStatus.Success),
                FailedSession = await query.CountAsync(x => x.ScanStatus == KioskScanSessionStatus.Failed),
                ManualInputSession = await query.CountAsync(x => x.IsManualInput),
                PatientFoundSession = await query.CountAsync(x => x.IsPatientFound),
                UsedForRegistrationSession = await query.CountAsync(x => x.IsUsedForRegistration)
            };

            return Ok(ApiResponse<KioskScanSessionSummaryResponse>.Ok(
                result,
                "Ringkasan kiosk scan session berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseKioskScanSessionPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Kiosk Scan Session", Description = "Melihat data kiosk scan session", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("KioskScanSession", "Read")]
        public async Task<IActionResult> GetSessions(
            [FromQuery] string? search,
            [FromQuery] KioskScanSource? scanSource,
            [FromQuery] KioskScanSessionStatus? scanStatus,
            [FromQuery] Guid? kioskDeviceId,
            [FromQuery] Guid? identityScannerProfileId,
            [FromQuery] Guid? patientId,
            [FromQuery] bool? isPatientFound,
            [FromQuery] bool? isManualInput,
            [FromQuery] bool? isUsedForRegistration,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy = "startedAt",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<TrxKioskScanSession>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.SessionCode.ToLower().Contains(keyword) ||
                    (x.IdentityNumber != null && x.IdentityNumber.ToLower().Contains(keyword)) ||
                    (x.CardNumber != null && x.CardNumber.ToLower().Contains(keyword)) ||
                    (x.MemberNumber != null && x.MemberNumber.ToLower().Contains(keyword)) ||
                    (x.InsuranceCardNumber != null && x.InsuranceCardNumber.ToLower().Contains(keyword)) ||
                    (x.FullName != null && x.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)));
            }

            if (scanSource.HasValue)
                query = query.Where(x => x.ScanSource == scanSource.Value);

            if (scanStatus.HasValue)
                query = query.Where(x => x.ScanStatus == scanStatus.Value);

            if (kioskDeviceId.HasValue && kioskDeviceId.Value != Guid.Empty)
                query = query.Where(x => x.KioskDeviceId == kioskDeviceId.Value);

            if (identityScannerProfileId.HasValue && identityScannerProfileId.Value != Guid.Empty)
                query = query.Where(x => x.IdentityScannerProfileId == identityScannerProfileId.Value);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (isPatientFound.HasValue)
                query = query.Where(x => x.IsPatientFound == isPatientFound.Value);

            if (isManualInput.HasValue)
                query = query.Where(x => x.IsManualInput == isManualInput.Value);

            if (isUsedForRegistration.HasValue)
                query = query.Where(x => x.IsUsedForRegistration == isUsedForRegistration.Value);

            if (startDate.HasValue)
                query = query.Where(x => x.StartedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.StartedAt < endDate.Value.AddDays(1));

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new KioskScanSessionResponse
                {
                    Id = x.Id,
                    SessionCode = x.SessionCode,
                    ScanSource = x.ScanSource,
                    ScanStatus = x.ScanStatus,
                    KioskDeviceId = x.KioskDeviceId,
                    KioskDeviceName = x.KioskDevice != null ? x.KioskDevice.DeviceName : null,
                    IdentityScannerProfileId = x.IdentityScannerProfileId,
                    IdentityScannerProfileName = x.IdentityScannerProfile != null ? x.IdentityScannerProfile.ProfileName : null,
                    PatientId = x.PatientId,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                    PatientName = x.Patient != null ? x.Patient.FullName : null,
                    IdentityType = x.IdentityType,
                    IdentityNumber = x.IdentityNumber,
                    CardNumber = x.CardNumber,
                    MemberNumber = x.MemberNumber,
                    InsuranceCardNumber = x.InsuranceCardNumber,
                    IsPatientFound = x.IsPatientFound,
                    IsManualInput = x.IsManualInput,
                    IsUsedForRegistration = x.IsUsedForRegistration,
                    StartedAt = x.StartedAt,
                    CompletedAt = x.CompletedAt,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

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

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<KioskScanSessionDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Kiosk Scan Session", Description = "Melihat detail kiosk scan session", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("KioskScanSession", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _dbContext.Set<TrxKioskScanSession>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new KioskScanSessionDetailResponse
                {
                    Id = x.Id,
                    SessionCode = x.SessionCode,
                    ScanSource = x.ScanSource,
                    ScanStatus = x.ScanStatus,
                    KioskDeviceId = x.KioskDeviceId,
                    KioskDeviceName = x.KioskDevice != null ? x.KioskDevice.DeviceName : null,
                    IdentityScannerProfileId = x.IdentityScannerProfileId,
                    IdentityScannerProfileName = x.IdentityScannerProfile != null ? x.IdentityScannerProfile.ProfileName : null,
                    PatientId = x.PatientId,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                    PatientName = x.Patient != null ? x.Patient.FullName : null,
                    IdentityType = x.IdentityType,
                    IdentityNumber = x.IdentityNumber,
                    CardNumber = x.CardNumber,
                    MemberNumber = x.MemberNumber,
                    InsuranceCardNumber = x.InsuranceCardNumber,
                    FullName = x.FullName,
                    BirthDate = x.BirthDate,
                    GenderText = x.GenderText,
                    Address = x.Address,
                    RawScanText = x.RawScanText,
                    ParsedJson = x.ParsedJson,
                    FailureReason = x.FailureReason,
                    IpAddress = x.IpAddress,
                    UserAgent = x.UserAgent,
                    IsPatientFound = x.IsPatientFound,
                    IsManualInput = x.IsManualInput,
                    IsUsedForRegistration = x.IsUsedForRegistration,
                    StartedAt = x.StartedAt,
                    CompletedAt = x.CompletedAt,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kiosk scan session tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<KioskScanSessionDetailResponse>.Ok(
                result,
                "Detail kiosk scan session berhasil diambil."
            ));
        }

        [HttpPost("scan-result")]
        [ProducesResponseType(typeof(ApiResponse<KioskScanSessionCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Kiosk Scan Session", Description = "Menerima hasil scan kartu pasien", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("KioskScanSession", "Create")]
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

            var patient = await FindPatientAsync(
                request.IdentityNumber,
                request.CardNumber,
                request.MemberNumber,
                request.InsuranceCardNumber
            );

            var isPatientFound = patient != null;

            var entity = new TrxKioskScanSession
            {
                Id = Guid.NewGuid(),
                SessionCode = await GenerateSessionCodeAsync(now),
                ScanSource = request.IsManualInput ? KioskScanSource.ManualInput : request.ScanSource,
                ScanStatus = isPatientFound || HasAnyReadableIdentity(request)
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
                FailureReason = isPatientFound || HasAnyReadableIdentity(request)
                    ? null
                    : "Data identitas tidak terbaca.",
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

            var response = new KioskScanSessionCreateResponse
            {
                Id = entity.Id,
                SessionCode = entity.SessionCode,
                ScanStatus = entity.ScanStatus,
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

        [HttpPatch("{id:guid}/mark-used-for-registration")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Kiosk Scan Session", Description = "Menandai hasil scan sudah dipakai untuk registrasi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("KioskScanSession", "Update")]
        public async Task<IActionResult> MarkUsedForRegistration(Guid id)
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

            entity.IsUsedForRegistration = true;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Kiosk scan session berhasil ditandai sudah dipakai untuk registrasi."
            ));
        }

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Cancel", "Cancel Kiosk Scan Session", Description = "Membatalkan kiosk scan session", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("KioskScanSession", "Update")]
        public async Task<IActionResult> CancelSession(Guid id)
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

            entity.ScanStatus = KioskScanSessionStatus.Cancelled;
            entity.IsCancel = true;
            entity.CancelDateTime = DateTime.UtcNow;
            entity.CancelBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Kiosk scan session berhasil dibatalkan."
            ));
        }

        private async Task<MstPatient?> FindPatientAsync(
            string? identityNumber,
            string? cardNumber,
            string? memberNumber,
            string? insuranceCardNumber)
        {
            var normalizedIdentityNumber = NormalizeNullableText(identityNumber);
            var normalizedCardNumber = NormalizeNullableText(cardNumber);
            var normalizedMemberNumber = NormalizeNullableText(memberNumber);
            var normalizedInsuranceCardNumber = NormalizeNullableText(insuranceCardNumber);

            if (!string.IsNullOrWhiteSpace(normalizedIdentityNumber))
            {
                var patient = await _dbContext.Set<MstPatient>()
                    .FirstOrDefaultAsync(x =>
                        !x.IsDelete &&
                        x.IsActive &&
                        x.IdentityNumber != null &&
                        x.IdentityNumber == normalizedIdentityNumber);

                if (patient != null)
                    return patient;

                var patientFromDocument = await _dbContext.Set<MstPatientIdentityDocument>()
                    .Where(x =>
                        !x.IsDelete &&
                        x.IsActive &&
                        x.IdentityNumber == normalizedIdentityNumber)
                    .Select(x => x.Patient)
                    .FirstOrDefaultAsync();

                if (patientFromDocument != null)
                    return patientFromDocument;
            }

            if (!string.IsNullOrWhiteSpace(normalizedCardNumber))
            {
                var patientFromInsuranceCard = await _dbContext.Set<MstPatientInsurance>()
                    .Where(x =>
                        !x.IsDelete &&
                        x.IsActive &&
                        x.CardNumber != null &&
                        x.CardNumber == normalizedCardNumber)
                    .Select(x => x.Patient)
                    .FirstOrDefaultAsync();

                if (patientFromInsuranceCard != null)
                    return patientFromInsuranceCard;
            }

            if (!string.IsNullOrWhiteSpace(normalizedMemberNumber))
            {
                var patientFromMembership = await _dbContext.Set<MstPatientMembership>()
                    .Where(x =>
                        !x.IsDelete &&
                        x.IsActive &&
                        x.MemberNumber == normalizedMemberNumber)
                    .Select(x => x.Patient)
                    .FirstOrDefaultAsync();

                if (patientFromMembership != null)
                    return patientFromMembership;

                var patientFromInsuranceMember = await _dbContext.Set<MstPatientInsurance>()
                    .Where(x =>
                        !x.IsDelete &&
                        x.IsActive &&
                        x.MemberNumber != null &&
                        x.MemberNumber == normalizedMemberNumber)
                    .Select(x => x.Patient)
                    .FirstOrDefaultAsync();

                if (patientFromInsuranceMember != null)
                    return patientFromInsuranceMember;
            }

            if (!string.IsNullOrWhiteSpace(normalizedInsuranceCardNumber))
            {
                var patientFromInsurance = await _dbContext.Set<MstPatientInsurance>()
                    .Where(x =>
                        !x.IsDelete &&
                        x.IsActive &&
                        x.CardNumber != null &&
                        x.CardNumber == normalizedInsuranceCardNumber)
                    .Select(x => x.Patient)
                    .FirstOrDefaultAsync();

                if (patientFromInsurance != null)
                    return patientFromInsurance;
            }

            return null;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            CreateKioskScanSessionRequest request)
        {
            if (request.KioskDeviceId.HasValue && request.KioskDeviceId.Value != Guid.Empty)
            {
                var deviceExists = await _dbContext.Set<MstKioskDevice>()
                    .AnyAsync(x => x.Id == request.KioskDeviceId.Value && x.IsActive && !x.IsDelete);

                if (!deviceExists)
                    return (false, "Kiosk device tidak valid atau tidak aktif.");
            }

            if (request.IdentityScannerProfileId.HasValue && request.IdentityScannerProfileId.Value != Guid.Empty)
            {
                var scannerProfileExists = await _dbContext.Set<MstIdentityScannerProfile>()
                    .AnyAsync(x => x.Id == request.IdentityScannerProfileId.Value && x.IsActive && !x.IsDelete);

                if (!scannerProfileExists)
                    return (false, "Identity scanner profile tidak valid atau tidak aktif.");
            }

            if (!HasAnyReadableIdentity(request))
                return (false, "Minimal salah satu data wajib tersedia: NIK, nomor kartu, nomor member, nomor kartu asuransi, atau raw scan text.");

            return (true, null);
        }

        private static bool HasAnyReadableIdentity(CreateKioskScanSessionRequest request)
        {
            return
                !string.IsNullOrWhiteSpace(request.IdentityNumber) ||
                !string.IsNullOrWhiteSpace(request.CardNumber) ||
                !string.IsNullOrWhiteSpace(request.MemberNumber) ||
                !string.IsNullOrWhiteSpace(request.InsuranceCardNumber) ||
                !string.IsNullOrWhiteSpace(request.RawScanText);
        }

        private async Task<string> GenerateSessionCodeAsync(DateTime now)
        {
            var prefix = $"KSS-{now:yyyyMMdd}";
            var countToday = await _dbContext.Set<TrxKioskScanSession>()
                .CountAsync(x => x.SessionCode.StartsWith(prefix));

            return $"{prefix}-{countToday + 1:D5}";
        }

        private static IQueryable<TrxKioskScanSession> ApplySorting(
            IQueryable<TrxKioskScanSession> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "startedAt").ToLowerInvariant() switch
            {
                "sessioncode" => isDesc
                    ? query.OrderByDescending(x => x.SessionCode)
                    : query.OrderBy(x => x.SessionCode),

                "scansource" => isDesc
                    ? query.OrderByDescending(x => x.ScanSource)
                    : query.OrderBy(x => x.ScanSource),

                "scanstatus" => isDesc
                    ? query.OrderByDescending(x => x.ScanStatus)
                    : query.OrderBy(x => x.ScanStatus),

                "completedat" => isDesc
                    ? query.OrderByDescending(x => x.CompletedAt)
                    : query.OrderBy(x => x.CompletedAt),

                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                _ => isDesc
                    ? query.OrderByDescending(x => x.StartedAt)
                    : query.OrderBy(x => x.StartedAt)
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
                pageNumber = 1;

            if (pageSize <= 0)
                pageSize = 25;

            if (pageSize > 100)
                pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
                return null;

            return value.Value;
        }

        private static string? NormalizeNullableText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value.Trim();
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userId, out var id)
                ? id
                : Guid.Empty;
        }
    }
}