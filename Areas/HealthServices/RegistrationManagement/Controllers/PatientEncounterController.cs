using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
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

using ResponsePatientEncounterPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs.PatientEncounterResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/registration/patient-encounters")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_REGISTRATION",
        moduleName: "Health Service Registration",
        displayName: "Patient Encounter",
        AreaName = "HealthServices",
        ControllerName = "PatientEncounter",
        Description = "Transaksi kunjungan pasien rawat jalan",
        SortOrder = 2
    )]
    [Tags("Health Services / Registration / Patient Encounter")]
    public class PatientEncounterController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Registration";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PatientEncounterController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientEncounterCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Patient Encounter", Description = "Membuat transaksi kunjungan pasien", AccessType = AccessTypes.Create, SortOrder = 1)]
        [AccessPermission("PatientEncounter", "Create")]
        public async Task<IActionResult> CreateEncounter([FromBody] PatientEncounterCreateRequest request)
        {
            var validation = await ValidateCreateRequestAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data kunjungan pasien tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var serviceUnit = await _dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .FirstAsync(x => x.Id == request.ServiceUnitId);

            MstClinic? clinic = null;

            if (request.ClinicId.HasValue && request.ClinicId.Value != Guid.Empty)
            {
                clinic = await _dbContext.Set<MstClinic>()
                    .AsNoTracking()
                    .FirstAsync(x => x.Id == request.ClinicId.Value);
            }

            var isScreeningRequired = clinic?.IsScreeningRequired ?? serviceUnit.IsScreeningRequired;
            var isQueueRequired = clinic?.IsQueueRequired ?? serviceUnit.IsQueueRequired;
            var isDoctorRequired = clinic?.IsDoctorRequired ?? serviceUnit.IsDoctorRequired;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var encounter = new TrxPatientEncounter
            {
                Id = Guid.NewGuid(),
                EncounterNumber = await GenerateEncounterNumberAsync(now),
                PatientId = request.PatientId,
                ServiceUnitId = request.ServiceUnitId,
                ClinicId = NormalizeNullableGuid(request.ClinicId),
                DoctorId = NormalizeNullableGuid(request.DoctorId),
                DoctorScheduleId = NormalizeNullableGuid(request.DoctorScheduleId),
                DoctorServiceRuleId = NormalizeNullableGuid(request.DoctorServiceRuleId),
                PatientClassId = NormalizeNullableGuid(request.PatientClassId),
                PaymentMethodId = NormalizeNullableGuid(request.PaymentMethodId),
                PatientInsuranceId = NormalizeNullableGuid(request.PatientInsuranceId),
                InsuranceProviderId = NormalizeNullableGuid(request.InsuranceProviderId),
                CompanyGuarantorId = NormalizeNullableGuid(request.CompanyGuarantorId),
                PatientCompanyGuarantorId = NormalizeNullableGuid(request.PatientCompanyGuarantorId),
                PatientMembershipId = NormalizeNullableGuid(request.PatientMembershipId),
                KioskScanSessionId = NormalizeNullableGuid(request.KioskScanSessionId),
                EncounterDate = now,
                EncounterType = request.EncounterType,
                VisitType = request.VisitType,
                RegistrationSource = request.RegistrationSource,
                PaymentType = request.PaymentType,
                EncounterStatus = isQueueRequired
                    ? EncounterStatus.Queued
                    : EncounterStatus.Registered,
                ChiefComplaint = NormalizeNullableText(request.ChiefComplaint),
                ReferralNumber = NormalizeNullableText(request.ReferralNumber),
                EligibilityReferenceNumber = NormalizeNullableText(request.EligibilityReferenceNumber),
                EligibilityCheckedAt = request.EligibilityCheckedAt,
                IsNewPatient = request.IsNewPatient,
                IsFromKiosk = request.IsFromKiosk,
                IsWalkIn = request.IsWalkIn,
                IsAppointment = request.IsAppointment,
                IsReferral = request.IsReferral,
                IsScreeningRequired = isScreeningRequired,
                IsQueueRequired = isQueueRequired,
                IsDoctorRequired = isDoctorRequired,
                RegisteredAt = now,
                RegisteredByUserId = actorUserId,
                Notes = NormalizeNullableText(request.Notes),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<TrxPatientEncounter>().Add(encounter);

            TrxQueue? queue = null;

            if (isQueueRequired)
            {
                var queueNumber = await GenerateQueueNumberAsync(
                    now,
                    request.ServiceUnitId,
                    request.ClinicId,
                    request.DoctorId
                );

                queue = new TrxQueue
                {
                    Id = Guid.NewGuid(),
                    EncounterId = encounter.Id,
                    PatientId = request.PatientId,
                    ServiceUnitId = request.ServiceUnitId,
                    ClinicId = NormalizeNullableGuid(request.ClinicId),
                    DoctorId = NormalizeNullableGuid(request.DoctorId),
                    DoctorScheduleId = NormalizeNullableGuid(request.DoctorScheduleId),
                    QueueDate = now.Date,
                    QueueNumber = queueNumber,
                    QueueCode = GenerateQueueCode(now, clinic, queueNumber),
                    QueueStatus = isScreeningRequired
                        ? QueueStatus.WaitingForNurse
                        : QueueStatus.WaitingForDoctor,
                    IsFromKiosk = request.IsFromKiosk,
                    IsWalkIn = request.IsWalkIn,
                    IsAppointment = request.IsAppointment,
                    IsScreeningRequired = isScreeningRequired,
                    IsDoctorRequired = isDoctorRequired,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<TrxQueue>().Add(queue);

                encounter.EncounterStatus = isScreeningRequired
                    ? EncounterStatus.WaitingForNurse
                    : EncounterStatus.WaitingForDoctor;
            }

            if (request.KioskScanSessionId.HasValue && request.KioskScanSessionId.Value != Guid.Empty)
            {
                var scanSession = await _dbContext.Set<TrxKioskScanSession>()
                    .FirstOrDefaultAsync(x => x.Id == request.KioskScanSessionId.Value && !x.IsDelete);

                if (scanSession != null)
                {
                    scanSession.IsUsedForRegistration = true;
                    scanSession.UpdateDateTime = now;
                    scanSession.UpdateBy = actorUserId;
                }
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var response = new PatientEncounterCreateResponse
            {
                EncounterId = encounter.Id,
                EncounterNumber = encounter.EncounterNumber,
                EncounterStatus = encounter.EncounterStatus,
                QueueId = queue?.Id,
                QueueCode = queue?.QueueCode,
                QueueNumber = queue?.QueueNumber,
                QueueStatus = queue?.QueueStatus,
                IsQueueCreated = queue != null,
                IsScreeningRequired = isScreeningRequired,
                IsDoctorRequired = isDoctorRequired,
                IsQueueRequired = isQueueRequired
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientEncounter.CreateEncounter",
                "Membuat transaksi kunjungan pasien.",
                response
            );

            return Ok(ApiResponse<PatientEncounterCreateResponse>.Ok(
                response,
                "Transaksi kunjungan pasien berhasil dibuat."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientEncounterPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Encounter", Description = "Melihat data kunjungan pasien", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("PatientEncounter", "Read")]
        public async Task<IActionResult> GetEncounters(
            [FromQuery] string? search,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? doctorId,
            [FromQuery] EncounterStatus? encounterStatus,
            [FromQuery] EncounterType? encounterType,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy = "registeredAt",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<TrxPatientEncounter>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.EncounterNumber.ToLower().Contains(keyword) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Clinic != null && x.Clinic.ClinicName.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.FullName.ToLower().Contains(keyword)));
            }

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
                query = query.Where(x => x.ClinicId == clinicId.Value);

            if (doctorId.HasValue && doctorId.Value != Guid.Empty)
                query = query.Where(x => x.DoctorId == doctorId.Value);

            if (encounterStatus.HasValue)
                query = query.Where(x => x.EncounterStatus == encounterStatus.Value);

            if (encounterType.HasValue)
                query = query.Where(x => x.EncounterType == encounterType.Value);

            if (startDate.HasValue)
                query = query.Where(x => x.EncounterDate >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(x => x.EncounterDate < endDate.Value.Date.AddDays(1));

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PatientEncounterResponse
                {
                    Id = x.Id,
                    EncounterNumber = x.EncounterNumber,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty,
                    ClinicId = x.ClinicId,
                    ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                    DoctorId = x.DoctorId,
                    DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                    DoctorScheduleId = x.DoctorScheduleId,
                    DoctorServiceRuleId = x.DoctorServiceRuleId,
                    PatientClassId = x.PatientClassId,
                    PatientClassName = x.PatientClass != null ? x.PatientClass.PatientClassName : null,
                    PaymentMethodId = x.PaymentMethodId,
                    PaymentMethodName = x.PaymentMethod != null ? x.PaymentMethod.PaymentMethodName : null,
                    EncounterType = x.EncounterType,
                    VisitType = x.VisitType,
                    RegistrationSource = x.RegistrationSource,
                    PaymentType = x.PaymentType,
                    EncounterStatus = x.EncounterStatus,
                    EncounterDate = x.EncounterDate,
                    RegisteredAt = x.RegisteredAt,
                    IsScreeningRequired = x.IsScreeningRequired,
                    IsQueueRequired = x.IsQueueRequired,
                    IsDoctorRequired = x.IsDoctorRequired,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new ResponsePatientEncounterPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePatientEncounterPagedResult>.Ok(
                result,
                "Data kunjungan pasien berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientEncounterDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Encounter", Description = "Melihat detail kunjungan pasien", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("PatientEncounter", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _dbContext.Set<TrxPatientEncounter>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new PatientEncounterDetailResponse
                {
                    Id = x.Id,
                    EncounterNumber = x.EncounterNumber,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty,
                    ClinicId = x.ClinicId,
                    ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                    DoctorId = x.DoctorId,
                    DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                    DoctorScheduleId = x.DoctorScheduleId,
                    DoctorServiceRuleId = x.DoctorServiceRuleId,
                    PatientClassId = x.PatientClassId,
                    PatientClassName = x.PatientClass != null ? x.PatientClass.PatientClassName : null,
                    PaymentMethodId = x.PaymentMethodId,
                    PaymentMethodName = x.PaymentMethod != null ? x.PaymentMethod.PaymentMethodName : null,
                    PatientInsuranceId = x.PatientInsuranceId,
                    InsuranceProviderName = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : null,
                    CompanyGuarantorId = x.CompanyGuarantorId,
                    CompanyGuarantorName = x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : null,
                    PatientMembershipId = x.PatientMembershipId,
                    MemberNumber = x.PatientMembership != null ? x.PatientMembership.MemberNumber : null,
                    KioskScanSessionId = x.KioskScanSessionId,
                    EncounterType = x.EncounterType,
                    VisitType = x.VisitType,
                    RegistrationSource = x.RegistrationSource,
                    PaymentType = x.PaymentType,
                    EncounterStatus = x.EncounterStatus,
                    EncounterDate = x.EncounterDate,
                    RegisteredAt = x.RegisteredAt,
                    ChiefComplaint = x.ChiefComplaint,
                    ReferralNumber = x.ReferralNumber,
                    EligibilityReferenceNumber = x.EligibilityReferenceNumber,
                    EligibilityCheckedAt = x.EligibilityCheckedAt,
                    CheckedInAt = x.CheckedInAt,
                    CompletedAt = x.CompletedAt,
                    Notes = x.Notes,
                    IsScreeningRequired = x.IsScreeningRequired,
                    IsQueueRequired = x.IsQueueRequired,
                    IsDoctorRequired = x.IsDoctorRequired,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kunjungan pasien tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientEncounterDetailResponse>.Ok(
                result,
                "Detail kunjungan pasien berhasil diambil."
            ));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(
            PatientEncounterCreateRequest request)
        {
            var patientExists = await _dbContext.Set<MstPatient>()
                .AnyAsync(x => x.Id == request.PatientId && x.IsActive && !x.IsDelete);

            if (!patientExists)
                return (false, "Pasien tidak valid atau tidak aktif.");

            var serviceUnitExists = await _dbContext.Set<MstServiceUnit>()
                .AnyAsync(x => x.Id == request.ServiceUnitId && x.IsActive && !x.IsDelete && x.IsAvailableForRegistration);

            if (!serviceUnitExists)
                return (false, "Service unit tidak valid, tidak aktif, atau tidak tersedia untuk registrasi.");

            if (request.ClinicId.HasValue && request.ClinicId.Value != Guid.Empty)
            {
                var clinicExists = await _dbContext.Set<MstClinic>()
                    .AnyAsync(x =>
                        x.Id == request.ClinicId.Value &&
                        x.ServiceUnitId == request.ServiceUnitId &&
                        x.IsActive &&
                        !x.IsDelete &&
                        x.IsAvailableForRegistration);

                if (!clinicExists)
                    return (false, "Clinic tidak valid, tidak aktif, atau tidak tersedia untuk registrasi.");
            }

            if (request.DoctorId.HasValue && request.DoctorId.Value != Guid.Empty)
            {
                var doctorExists = await _dbContext.Set<MstDoctor>()
                    .AnyAsync(x => x.Id == request.DoctorId.Value && x.IsActive && !x.IsDelete);

                if (!doctorExists)
                    return (false, "Dokter tidak valid atau tidak aktif.");
            }

            if (request.DoctorScheduleId.HasValue && request.DoctorScheduleId.Value != Guid.Empty)
            {
                var scheduleExists = await _dbContext.Set<MstDoctorSchedule>()
                    .AnyAsync(x =>
                        x.Id == request.DoctorScheduleId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!scheduleExists)
                    return (false, "Jadwal dokter tidak valid atau tidak aktif.");
            }

            if (request.PaymentMethodId.HasValue && request.PaymentMethodId.Value != Guid.Empty)
            {
                var paymentMethodExists = await _dbContext.Set<MstPaymentMethod>()
                    .AnyAsync(x =>
                        x.Id == request.PaymentMethodId.Value &&
                        x.IsActive &&
                        !x.IsDelete &&
                        x.IsAvailableForRegistration);

                if (!paymentMethodExists)
                    return (false, "Metode pembayaran tidak valid atau tidak tersedia untuk registrasi.");
            }

            if (request.KioskScanSessionId.HasValue && request.KioskScanSessionId.Value != Guid.Empty)
            {
                var scanSessionExists = await _dbContext.Set<TrxKioskScanSession>()
                    .AnyAsync(x =>
                        x.Id == request.KioskScanSessionId.Value &&
                        !x.IsDelete &&
                        !x.IsUsedForRegistration);

                if (!scanSessionExists)
                    return (false, "Kiosk scan session tidak valid atau sudah digunakan.");
            }

            return (true, null);
        }

        private async Task<string> GenerateEncounterNumberAsync(DateTime now)
        {
            var prefix = $"ENC-{now:yyyyMMdd}";
            var countToday = await _dbContext.Set<TrxPatientEncounter>()
                .CountAsync(x => x.EncounterNumber.StartsWith(prefix));

            return $"{prefix}-{countToday + 1:D5}";
        }

        private async Task<int> GenerateQueueNumberAsync(
            DateTime now,
            Guid serviceUnitId,
            Guid? clinicId,
            Guid? doctorId)
        {
            return await _dbContext.Set<TrxQueue>()
                .CountAsync(x =>
                    x.QueueDate == now.Date &&
                    x.ServiceUnitId == serviceUnitId &&
                    x.ClinicId == NormalizeNullableGuid(clinicId) &&
                    x.DoctorId == NormalizeNullableGuid(doctorId) &&
                    !x.IsDelete) + 1;
        }

        private static string GenerateQueueCode(DateTime now, MstClinic? clinic, int queueNumber)
        {
            var prefix = !string.IsNullOrWhiteSpace(clinic?.ShortName)
                ? clinic.ShortName.Trim().ToUpperInvariant()
                : "Q";

            return $"{prefix}-{now:yyyyMMdd}-{queueNumber:D3}";
        }

        private static IQueryable<TrxPatientEncounter> ApplySorting(
            IQueryable<TrxPatientEncounter> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "registeredAt").ToLowerInvariant() switch
            {
                "encounternumber" => isDesc ? query.OrderByDescending(x => x.EncounterNumber) : query.OrderBy(x => x.EncounterNumber),
                "encounterdate" => isDesc ? query.OrderByDescending(x => x.EncounterDate) : query.OrderBy(x => x.EncounterDate),
                "encounterstatus" => isDesc ? query.OrderByDescending(x => x.EncounterStatus) : query.OrderBy(x => x.EncounterStatus),
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDesc ? query.OrderByDescending(x => x.RegisteredAt) : query.OrderBy(x => x.RegisteredAt)
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

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