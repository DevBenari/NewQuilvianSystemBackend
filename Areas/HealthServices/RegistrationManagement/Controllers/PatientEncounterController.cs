using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Security.Claims;

using ResponsePatientEncounterPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs.PatientEncounterResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/registration-management/patient-encounters")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_REGISTRATION_MANAGEMENT",
        moduleName: "Health Service Registration Management",
        displayName: "Patient Encounter",
        AreaName = "HealthServices",
        ControllerName = "PatientEncounter",
        Description = "Transaksi kunjungan pasien rawat jalan dan sumber pembayaran",
        SortOrder = 2
    )]
    [Tags("Health Services / Registration Management / Patient Encounter")]
    public class PatientEncounterController : ControllerBase
    {
        private const string LogCategory = "HealthServices.RegistrationManagement";
        private const string KioskReadPolicy = "KioskRead";
        private const string EncounterCodePrefix = "ENC-RSMMC-";
        private const string PaymentSourceCodePrefix = "EGT-RSMMC-";
        private const int CodeNumberLength = 5;

        // Nama kelas dijadikan business key karena kode dan GUID master dapat
        // berbeda antar-environment. Penulisan dibandingkan secara case-insensitive
        // dan mengabaikan spasi berlebih.
        private const string DefaultOutpatientPatientClassName = "RAWAT JALAN";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly QueueRealtimeService _queueRealtimeService;

        public PatientEncounterController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            QueueRealtimeService queueRealtimeService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _queueRealtimeService = queueRealtimeService;
        }

        [HttpGet("admin/filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientEncounterFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessPermission("PatientEncounter", "Read")]
        public async Task<IActionResult> GetFilterMetadataForAdmin()
        {
            return await GetFilterMetadataForKiosk();
        }

        [HttpGet("filters/metadata")]
        [HttpGet("kiosk/filters/metadata")]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<PatientEncounterFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Encounter", Description = "Melihat metadata filter patient encounter", AccessType = AccessTypes.Read, SortOrder = 1)]
        public async Task<IActionResult> GetFilterMetadataForKiosk()
        {
            var result = new PatientEncounterFilterMetadataResponse
            {
                DefaultFilter = new PatientEncounterDefaultFilterResponse(),
                CustomPeriods = new List<PatientEncounterCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                RelationFilters = new List<PatientEncounterRelationFilterResponse>
                {
                    new() { Value = "patientId", Label = "Patient", Endpoint = "/api/v1/health-services/patient-management/master-data/patients/options" },
                    new() { Value = "serviceUnitId", Label = "Service Unit", Endpoint = "/api/v1/health-services/master-data/service-units/options" },
                    new() { Value = "roomId", Label = "Ruangan", Endpoint = "/api/v1/health-services/master-data/rooms/options" }
                },
                SortOptions = new List<PatientEncounterSortOptionResponse>
                {
                    new() { Value = "registeredAt", Label = "Waktu registrasi" },
                    new() { Value = "encounterDate", Label = "Tanggal encounter" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "updateDateTime", Label = "Tanggal diperbarui" },
                    new() { Value = "encounterNumber", Label = "Nomor encounter" },
                    new() { Value = "patientName", Label = "Nama pasien" },
                    new() { Value = "medicalRecordNumber", Label = "Nomor rekam medis" },
                    new() { Value = "serviceUnitName", Label = "Service unit" },
                    new() { Value = "clinicName", Label = "Clinic" },
                    new() { Value = "roomName", Label = "Ruangan" },
                    new() { Value = "doctorName", Label = "Dokter" },
                    new() { Value = "encounterStatus", Label = "Status encounter" },
                    new() { Value = "paymentType", Label = "Tipe pembayaran" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                EncounterTypeOptions = BuildEnumOptions<EncounterType>(),
                VisitTypeOptions = BuildEnumOptions<VisitType>(),
                RegistrationSourceOptions = BuildEnumOptions<EncounterRegistrationSource>(),
                EncounterStatusOptions = BuildEnumOptions<EncounterStatus>(),
                PaymentTypeOptions = BuildEnumOptions<EncounterPaymentType>(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientEncounter.GetFilterMetadataForKiosk",
                "Mengambil metadata filter patient encounter.",
                result);

            return Ok(ApiResponse<PatientEncounterFilterMetadataResponse>.Ok(
                result,
                "Metadata filter patient encounter berhasil diambil."));
        }

        [HttpGet("summary")]
        [HttpGet("admin/summary")]
        [ProducesResponseType(typeof(ApiResponse<PatientEncounterSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Encounter", Description = "Melihat ringkasan patient encounter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientEncounter", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] EncounterStatus? encounterStatus,
            [FromQuery] EncounterType? encounterType,
            [FromQuery] EncounterPaymentType? paymentType,
            [FromQuery] bool? isActive)
        {
            var query = BuildBaseQuery();
            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyRelationFilter(query, patientId, serviceUnitId);
            query = ApplyStandardFilter(
                query,
                encounterStatus,
                encounterType,
                paymentType,
                isReferral: null,
                isActive: isActive,
                search: null);

            var result = new PatientEncounterSummaryResponse
            {
                TotalEncounter = await query.CountAsync(),
                RegisteredEncounter = await query.CountAsync(x => x.EncounterStatus == EncounterStatus.Registered),
                WaitingForNurseEncounter = await query.CountAsync(x => x.EncounterStatus == EncounterStatus.WaitingForNurse),
                WaitingForDoctorEncounter = await query.CountAsync(x => x.EncounterStatus == EncounterStatus.WaitingForDoctor),
                CompletedEncounter = await query.CountAsync(x => x.CompletedAt.HasValue),
                CancelledEncounter = await query.CountAsync(x => x.CancelledAt.HasValue || x.IsCancel),
                NoShowEncounter = await query.CountAsync(x => x.NoShowAt.HasValue),
                CashEncounter = await query.CountAsync(x => x.PaymentType == EncounterPaymentType.Cash),
                InsuranceEncounter = await query.CountAsync(x => x.PaymentType == EncounterPaymentType.Insurance),
                ReferralEncounter = await query.CountAsync(x => x.IsReferral),
                FromKioskEncounter = await query.CountAsync(x => x.IsFromKiosk)
            };

            return Ok(ApiResponse<PatientEncounterSummaryResponse>.Ok(
                result,
                "Ringkasan patient encounter berhasil diambil."));
        }

        [HttpGet]
        [HttpGet("admin")]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientEncounterPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Encounter", Description = "Melihat data kunjungan pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientEncounter", "Read")]
        public async Task<IActionResult> GetEncounters(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? roomId,
            [FromQuery] Guid? doctorId,
            [FromQuery] EncounterStatus? encounterStatus,
            [FromQuery] EncounterType? encounterType,
            [FromQuery] EncounterPaymentType? paymentType,
            [FromQuery] bool? isReferral,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "registeredAt",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();
            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyRelationFilter(query, patientId, serviceUnitId);
            query = ApplyStandardFilter(
                query,
                encounterStatus,
                encounterType,
                paymentType,
                isReferral,
                isActive,
                search);

            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
            {
                query = query.Where(x => x.ClinicId == clinicId.Value);
            }

            if (roomId.HasValue && roomId.Value != Guid.Empty)
            {
                query = query.Where(x => x.RoomId == roomId.Value);
            }

            if (doctorId.HasValue && doctorId.Value != Guid.Empty)
            {
                query = query.Where(x => x.DoctorId == doctorId.Value);
            }

            var totalData = await query.CountAsync();
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities.SelectMany(x => new[]
                {
                    x.CreateBy,
                    x.UpdateBy,
                    x.RegisteredByUserId,
                    x.CancelledByUserId ?? Guid.Empty,
                    x.NoShowByUserId ?? Guid.Empty
                }));

            var items = entities
                .Select(x => MapResponse(x, actorNames))
                .ToList();

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
                "Data kunjungan pasien berhasil diambil."));
        }

        [HttpGet("admin/options")]
        [ProducesResponseType(typeof(ApiResponse<PatientEncounterOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessPermission("PatientEncounter", "Read")]
        public async Task<IActionResult> GetEncounterOptionsForAdmin(
            [FromQuery] Guid? patientId = null,
            [FromQuery] Guid? serviceUnitId = null,
            [FromQuery] EncounterStatus? encounterStatus = null,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            return await GetEncounterOptionsForKiosk(
                patientId,
                serviceUnitId,
                encounterStatus,
                onlyActive,
                search,
                pageNumber,
                pageSize);
        }

        [HttpGet("options")]
        [HttpGet("kiosk/options")]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<PatientEncounterOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Encounter", Description = "Melihat data pilihan patient encounter", AccessType = AccessTypes.Read, SortOrder = 1)]
        public async Task<IActionResult> GetEncounterOptionsForKiosk(
            [FromQuery] Guid? patientId = null,
            [FromQuery] Guid? serviceUnitId = null,
            [FromQuery] EncounterStatus? encounterStatus = null,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();
            query = ApplyRelationFilter(query, patientId, serviceUnitId);

            if (encounterStatus.HasValue) query = query.Where(x => x.EncounterStatus == encounterStatus.Value);
            if (onlyActive) query = query.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.EncounterNumber.ToLower().Contains(keyword) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitName.ToLower().Contains(keyword)) ||
                    (x.Clinic != null && x.Clinic.ClinicName.ToLower().Contains(keyword)) ||
                    (x.Room != null && x.Room.RoomCode.ToLower().Contains(keyword)) ||
                    (x.Room != null && x.Room.RoomName.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.FullName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();
            var entities = await query.OrderByDescending(x => x.RegisteredAt).ThenByDescending(x => x.EncounterNumber).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var result = new PatientEncounterOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(MapOptionResponse).ToList()
            };

            return Ok(ApiResponse<PatientEncounterOptionPagedResponse>.Ok(result, "Data pilihan patient encounter berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [HttpGet("admin/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientEncounterDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Encounter", Description = "Melihat detail kunjungan pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientEncounter", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .Include(x => x.PaymentSource)
                    .ThenInclude(x => x!.PaymentMethod)
                .Include(x => x.PaymentSource)
                    .ThenInclude(x => x!.InsuranceProvider)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kunjungan pasien tidak ditemukan."));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy,
                entity.RegisteredByUserId,
                entity.CancelledByUserId ?? Guid.Empty,
                entity.NoShowByUserId ?? Guid.Empty
            });

            return Ok(ApiResponse<PatientEncounterDetailResponse>.Ok(
                MapDetailResponse(entity, actorNames),
                "Detail kunjungan pasien berhasil diambil."));
        }

        [HttpPost("admin")]
        [ProducesResponseType(typeof(ApiResponse<PatientEncounterCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessPermission("PatientEncounter", "Create")]
        public async Task<IActionResult> CreateEncounterForAdmin([FromBody] PatientEncounterCreateRequest request)
        {
            return await CreateEncounterForKiosk(request);
        }

        [HttpPost]
        [HttpPost("kiosk")]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<PatientEncounterCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Patient Encounter", Description = "Membuat transaksi kunjungan pasien dengan satu sumber pembayaran", AccessType = AccessTypes.Create, SortOrder = 2)]
        public async Task<IActionResult> CreateEncounterForKiosk([FromBody] PatientEncounterCreateRequest request)
        {
            var traceId = HttpContext.TraceIdentifier;
            Response.Headers["X-Trace-Id"] = traceId;

            var now = DateTime.UtcNow;
            var operationalDate = ToUtcDate(AppDateTimeHelper.OperationalDate());

            var targetDateResult = await ResolveTargetEncounterDateAsync(
                request,
                operationalDate);

            if (!targetDateResult.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    targetDateResult.ErrorMessage ?? "Tanggal kunjungan tidak valid."));
            }

            var targetEncounterDate = targetDateResult.TargetDate;

            // Rawat jalan selalu menggunakan kelas pasien RAWAT JALAN yang
            // diselesaikan oleh backend. Dengan demikian kiosk/front desk tidak
            // perlu mengirim GUID kelas dan tarif dapat dicocokkan secara konsisten.
            var patientClassResolution = await ResolvePatientClassAsync(
                request,
                cancellationToken: HttpContext.RequestAborted);

            if (!patientClassResolution.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    patientClassResolution.ErrorMessage
                        ?? "Kelas pasien tidak dapat ditentukan."));
            }

            request.PatientClassId = patientClassResolution.PatientClass?.Id;

            var validation = await ValidateCreateRequestAsync(
                request,
                targetEncounterDate,
                operationalDate);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data kunjungan pasien tidak valid."));
            }

            var roomResolution = await ResolveEncounterRoomAsync(request);
            if (!roomResolution.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    roomResolution.ErrorMessage ?? "Ruangan pelayanan tidak valid."));
            }

            var room = roomResolution.Room;

            MstPatientInsurance? patientInsurance = null;

            if (request.PaymentType == EncounterPaymentType.Insurance)
            {
                var insuranceResult = await LoadValidPatientInsuranceAsync(
                    request.PatientId,
                    request.PatientInsuranceId!.Value,
                    targetEncounterDate);

                if (insuranceResult.Insurance == null)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        insuranceResult.ErrorMessage ?? "Asuransi pasien tidak valid."));
                }

                patientInsurance = insuranceResult.Insurance;
            }

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

            var patient = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .FirstAsync(x =>
                    x.Id == request.PatientId &&
                    x.IsActive &&
                    !x.IsDelete);

            var ageSnapshot = await BuildAgeSnapshotAsync(
                patient.BirthDate,
                targetEncounterDate,
                now);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            var transactionCommitted = false;

            try
            {
                var encounter = new TrxPatientEncounter
                {
                    Id = Guid.NewGuid(),
                    EncounterNumber = await GenerateEncounterNumberAsync(),
                    PatientId = request.PatientId,
                    ServiceUnitId = request.ServiceUnitId,
                    ClinicId = NormalizeNullableGuid(request.ClinicId),
                    RoomId = room?.Id,
                    DoctorId = NormalizeNullableGuid(request.DoctorId),
                    DoctorScheduleId = NormalizeNullableGuid(request.DoctorScheduleId),
                    DoctorServiceRuleId = NormalizeNullableGuid(request.DoctorServiceRuleId),
                    PatientClassId = patientClassResolution.PatientClass?.Id,
                    PaymentMethodId = request.PaymentType == EncounterPaymentType.Cash
                        ? NormalizeNullableGuid(request.PaymentMethodId)
                        : null,
                    KioskScanSessionId = NormalizeNullableGuid(request.KioskScanSessionId),
                    AgeCategoryId = ageSnapshot.AgeCategoryId,
                    AgeYearAtEncounter = ageSnapshot.AgeYear,
                    AgeMonthAtEncounter = ageSnapshot.AgeMonth,
                    AgeDayAtEncounter = ageSnapshot.AgeDay,
                    TotalAgeDaysAtEncounter = ageSnapshot.TotalAgeDays,
                    AgeTextAtEncounter = ageSnapshot.AgeText,
                    AgeCategoryCodeSnapshot = ageSnapshot.AgeCategoryCode,
                    AgeCategoryNameSnapshot = ageSnapshot.AgeCategoryName,
                    AgeReferenceDate = ageSnapshot.AgeReferenceDate,
                    AgeCalculatedAt = ageSnapshot.AgeCalculatedAt,
                    EncounterDate = targetEncounterDate,
                    EncounterType = request.EncounterType,
                    VisitType = request.VisitType,
                    RegistrationSource = request.RegistrationSource,
                    PaymentType = request.PaymentType,
                    EncounterStatus = isQueueRequired
                        ? EncounterStatus.Queued
                        : EncounterStatus.Registered,
                    ChiefComplaint = NormalizeNullableText(request.ChiefComplaint),
                    IsReferral = request.IsReferral,
                    ReferralNumber = NormalizeNullableText(request.ReferralNumber),
                    IsReferralRequired = request.IsReferralRequired,
                    IsReferralVerified = request.IsReferralVerified,
                    IsNewPatient = request.IsNewPatient,
                    IsFromKiosk = request.IsFromKiosk || request.KioskScanSessionId.HasValue,
                    IsWalkIn = request.IsWalkIn,
                    IsAppointment = request.IsAppointment,
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

                var paymentSource = await BuildPaymentSourceAsync(
                    encounter.Id,
                    request,
                    patientInsurance,
                    now,
                    actorUserId);

                ApplyEncounterPaymentSummary(encounter, paymentSource);

                _dbContext.Set<TrxPatientEncounter>().Add(encounter);
                _dbContext.Set<TrxPatientEncounterGuarantor>().Add(paymentSource);

                TrxQueue? queue = null;

                if (isQueueRequired)
                {
                    var queueNumber = await GenerateQueueNumberAsync(
                        targetEncounterDate,
                        request.ServiceUnitId,
                        request.ClinicId);

                    queue = new TrxQueue
                    {
                        Id = Guid.NewGuid(),
                        EncounterId = encounter.Id,
                        PatientId = request.PatientId,
                        ServiceUnitId = request.ServiceUnitId,
                        ClinicId = NormalizeNullableGuid(request.ClinicId),
                        DoctorId = NormalizeNullableGuid(request.DoctorId),
                        DoctorScheduleId = NormalizeNullableGuid(request.DoctorScheduleId),
                        QueueDate = targetEncounterDate,
                        QueueNumber = queueNumber,
                        QueueCode = GenerateQueueCode(clinic, queueNumber),
                        QueueStatus = isScreeningRequired
                            ? QueueStatus.WaitingForNurse
                            : QueueStatus.WaitingForDoctor,
                        IsFromKiosk = encounter.IsFromKiosk,
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

                if (request.KioskScanSessionId.HasValue &&
                    request.KioskScanSessionId.Value != Guid.Empty)
                {
                    var scanSession = await _dbContext.Set<TrxKioskScanSession>()
                        .FirstOrDefaultAsync(x =>
                            x.Id == request.KioskScanSessionId.Value &&
                            !x.IsDelete);

                    if (scanSession != null)
                    {
                        scanSession.IsUsedForRegistration = true;
                        scanSession.UpdateDateTime = now;
                        scanSession.UpdateBy = actorUserId;
                    }
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                transactionCommitted = true;

                // Kegagalan notifikasi realtime tidak boleh mengubah transaksi database
                // yang sudah berhasil commit menjadi response 500.
                if (queue != null)
                {
                    try
                    {
                        await _queueRealtimeService.NotifyQueueCreatedAsync(
                            queue,
                            actorUserId,
                            "Antrean pasien baru dibuat.");
                    }
                    catch (Exception notificationException)
                    {
                        try
                        {
                            await _loggerService.ErrorAsync(
                                LogCategory,
                                "PatientEncounter.CreateEncounterForKiosk.QueueNotification",
                                $"Encounter dan antrean berhasil disimpan, tetapi notifikasi realtime gagal. TraceId={traceId}; EncounterId={encounter.Id}; QueueId={queue.Id}.",
                                notificationException);
                        }
                        catch
                        {
                            // Jangan menggagalkan response utama hanya karena pencatatan
                            // kegagalan notifikasi juga mengalami masalah.
                        }
                    }
                }

                var response = new PatientEncounterCreateResponse
                {
                    EncounterId = encounter.Id,
                    EncounterNumber = encounter.EncounterNumber,
                    PatientClassId = patientClassResolution.PatientClass?.Id,
                    PatientClassCode = patientClassResolution.PatientClass?.PatientClassCode,
                    PatientClassName = patientClassResolution.PatientClass?.PatientClassName,
                    IsPatientClassAssignedAutomatically =
                        patientClassResolution.IsAssignedAutomatically,
                    EncounterStatus = encounter.EncounterStatus,
                    EncounterStatusName = BuildEnumLabel(encounter.EncounterStatus),
                    QueueId = queue?.Id,
                    QueueCode = queue?.QueueCode,
                    QueueNumber = queue?.QueueNumber,
                    QueueStatus = queue?.QueueStatus,
                    QueueStatusName = queue?.QueueStatus != null
                        ? BuildEnumLabel(queue.QueueStatus)
                        : null,
                    EncounterDate = encounter.EncounterDate,
                    QueueDate = queue?.QueueDate,
                    IsFutureVisit = encounter.EncounterDate > operationalDate,
                    IsQueueCreated = queue != null,
                    IsScreeningRequired = isScreeningRequired,
                    IsDoctorRequired = isDoctorRequired,
                    IsQueueRequired = isQueueRequired,
                    RoomId = encounter.RoomId,
                    RoomCode = room?.RoomCode,
                    RoomName = room?.RoomName,
                    RoomNumber = room?.RoomNumber,
                    AgeCategoryId = encounter.AgeCategoryId,
                    AgeCategoryCode = encounter.AgeCategoryCodeSnapshot,
                    AgeCategoryName = encounter.AgeCategoryNameSnapshot,
                    AgeYearAtEncounter = encounter.AgeYearAtEncounter,
                    AgeMonthAtEncounter = encounter.AgeMonthAtEncounter,
                    AgeDayAtEncounter = encounter.AgeDayAtEncounter,
                    TotalAgeDaysAtEncounter = encounter.TotalAgeDaysAtEncounter,
                    AgeTextAtEncounter = encounter.AgeTextAtEncounter,
                    AgeReferenceDate = encounter.AgeReferenceDate,
                    AgeCalculatedAt = encounter.AgeCalculatedAt,
                    Payment = MapPaymentSourceResponse(paymentSource)
                };

                try
                {
                    await _loggerService.InfoAsync(
                        LogCategory,
                        "PatientEncounter.CreateEncounterForKiosk",
                        $"Membuat transaksi kunjungan pasien dengan satu sumber pembayaran. TraceId={traceId}.",
                        response);
                }
                catch
                {
                    // Data sudah commit. Kegagalan audit log tidak boleh mengubah
                    // response transaksi yang sebenarnya berhasil.
                }

                return Ok(ApiResponse<PatientEncounterCreateResponse>.Ok(
                    response,
                    "Transaksi kunjungan pasien berhasil dibuat."));
            }
            catch (DbUpdateException dbException)
            {
                var rollbackError = string.Empty;

                if (!transactionCommitted)
                {
                    try
                    {
                        await transaction.RollbackAsync();
                    }
                    catch (Exception rollbackException)
                    {
                        rollbackError = rollbackException.GetBaseException().Message;
                    }
                }

                var rootException = dbException.GetBaseException();
                var postgresException = rootException as PostgresException;
                var registrationMode = request.IsNewPatient
                    ? "NEW_PATIENT"
                    : "OLD_PATIENT";

                var trackedEntities = string.Join(
                    ", ",
                    _dbContext.ChangeTracker
                        .Entries()
                        .Select(entry =>
                            $"{entry.Metadata.ClrType.Name}:{entry.State}"));

                var databaseErrorMessage =
                    $"TraceId={traceId}; " +
                    $"RegistrationMode={registrationMode}; " +
                    $"PatientId={request.PatientId}; " +
                    $"ServiceUnitId={request.ServiceUnitId}; " +
                    $"ClinicId={request.ClinicId}; " +
                    $"RoomId={request.RoomId}; " +
                    $"DoctorId={request.DoctorId}; " +
                    $"DoctorScheduleId={request.DoctorScheduleId}; " +
                    $"DoctorServiceRuleId={request.DoctorServiceRuleId}; " +
                    $"PaymentType={request.PaymentType}; " +
                    $"PaymentMethodId={request.PaymentMethodId}; " +
                    $"PatientInsuranceId={request.PatientInsuranceId}; " +
                    $"KioskScanSessionId={request.KioskScanSessionId}; " +
                    $"SqlState={postgresException?.SqlState ?? "-"}; " +
                    $"Schema={postgresException?.SchemaName ?? "-"}; " +
                    $"Table={postgresException?.TableName ?? "-"}; " +
                    $"Column={postgresException?.ColumnName ?? "-"}; " +
                    $"Constraint={postgresException?.ConstraintName ?? "-"}; " +
                    $"DatabaseMessage={postgresException?.MessageText ?? rootException.Message}; " +
                    $"Detail={postgresException?.Detail ?? "-"}; " +
                    $"Hint={postgresException?.Hint ?? "-"}; " +
                    $"TrackedEntities={trackedEntities}; " +
                    $"RollbackError={(string.IsNullOrWhiteSpace(rollbackError) ? "-" : rollbackError)}";

                try
                {
                    await _loggerService.ErrorAsync(
                        LogCategory,
                        "PatientEncounter.CreateEncounterForKiosk.Database",
                        databaseErrorMessage,
                        dbException);
                }
                catch
                {
                    // Response tetap harus dikembalikan meskipun logger bermasalah.
                }

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        $"Database gagal menyimpan transaksi kunjungan pasien. Trace ID: {traceId}"));
            }
            catch (Exception ex)
            {
                var rollbackError = string.Empty;

                if (!transactionCommitted)
                {
                    try
                    {
                        await transaction.RollbackAsync();
                    }
                    catch (Exception rollbackException)
                    {
                        rollbackError = rollbackException.GetBaseException().Message;
                    }
                }

                var rootException = ex.GetBaseException();
                var registrationMode = request.IsNewPatient
                    ? "NEW_PATIENT"
                    : "OLD_PATIENT";

                try
                {
                    await _loggerService.ErrorAsync(
                        LogCategory,
                        "PatientEncounter.CreateEncounterForKiosk",
                        $"TraceId={traceId}; " +
                        $"RegistrationMode={registrationMode}; " +
                        $"TransactionCommitted={transactionCommitted}; " +
                        $"ExceptionType={rootException.GetType().FullName}; " +
                        $"Message={rootException.Message}; " +
                        $"RollbackError={(string.IsNullOrWhiteSpace(rollbackError) ? "-" : rollbackError)}",
                        ex);
                }
                catch
                {
                    // Response tetap harus dikembalikan meskipun logger bermasalah.
                }

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        $"Terjadi kesalahan saat membuat transaksi kunjungan pasien. Trace ID: {traceId}"));
            }
        }

        [HttpPatch("{id:guid}/status")]
        [HttpPatch("admin/{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Encounter Status", Description = "Mengubah status patient encounter", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientEncounter", "Update")]
        public async Task<IActionResult> UpdateEncounterStatus(Guid id, [FromBody] PatientEncounterStatusRequest request)
        {
            if (!Enum.IsDefined(typeof(EncounterStatus), request.EncounterStatus))
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Status encounter tidak valid. Gunakan nilai dari endpoint filters/metadata."));
            }

            var entity = await _dbContext.Set<TrxPatientEncounter>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Kunjungan pasien tidak ditemukan."));
            }

            if (entity.IsCancel || entity.CancelledAt.HasValue || entity.CompletedAt.HasValue)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Kunjungan yang sudah batal atau selesai tidak dapat diubah statusnya."));
            }

            entity.EncounterStatus = request.EncounterStatus;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            if (!string.IsNullOrWhiteSpace(request.Reason))
            {
                entity.Notes = request.Reason.Trim();
            }

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Status patient encounter berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/check-in")]
        [HttpPatch("admin/{id:guid}/check-in")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Check In Patient Encounter", Description = "Check-in patient encounter", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientEncounter", "Update")]
        public async Task<IActionResult> CheckInEncounter(Guid id)
        {
            var entity = await _dbContext.Set<TrxPatientEncounter>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Kunjungan pasien tidak ditemukan."));
            }

            if (entity.CancelledAt.HasValue || entity.NoShowAt.HasValue || entity.CompletedAt.HasValue)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Kunjungan yang sudah selesai, no-show, atau batal tidak dapat check-in."));
            }

            entity.CheckedInAt = DateTime.UtcNow;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Patient encounter berhasil check-in."));
        }

        [HttpPatch("{id:guid}/cancel")]
        [HttpPatch("admin/{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Cancel Patient Encounter", Description = "Membatalkan patient encounter", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientEncounter", "Update")]
        public async Task<IActionResult> CancelEncounter(Guid id, [FromBody] PatientEncounterCancelRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientEncounter>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Kunjungan pasien tidak ditemukan."));
            }

            if (entity.CompletedAt.HasValue)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Kunjungan yang sudah selesai tidak dapat dibatalkan."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.CancelledAt = now;
            entity.CancelledByUserId = actorUserId;
            entity.CancelReason = request.CancelReason.Trim();
            entity.IsCancel = true;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            var cancelledQueues = await CancelQueuesByEncounterAsync(entity.Id, now, actorUserId, request.CancelReason.Trim());
            await _dbContext.SaveChangesAsync();

            foreach (var queue in cancelledQueues)
            {
                await _queueRealtimeService.NotifyQueueCancelledAsync(queue, actorUserId, "Patient encounter dibatalkan.");
            }

            return Ok(ApiResponse<object>.Ok(null, "Patient encounter berhasil dibatalkan."));
        }

        [HttpDelete("{id:guid}")]
        [HttpDelete("admin/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Patient Encounter", Description = "Menghapus patient encounter", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("PatientEncounter", "Delete")]
        public async Task<IActionResult> DeleteEncounter(Guid id, [FromBody] DeletePatientEncounterRequest? request = null)
        {
            var entity = await _dbContext.Set<TrxPatientEncounter>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Kunjungan pasien tidak ditemukan."));
            }

            if (entity.CompletedAt.HasValue)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Kunjungan yang sudah selesai tidak dapat dihapus. Gunakan pembatalan atau koreksi data."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (!string.IsNullOrWhiteSpace(request?.DeleteReason))
            {
                entity.Notes = request.DeleteReason.Trim();
            }

            var paymentSource = await _dbContext.Set<TrxPatientEncounterGuarantor>()
                .FirstOrDefaultAsync(x => x.EncounterId == id && !x.IsDelete);

            if (paymentSource != null)
            {
                paymentSource.IsDelete = true;
                paymentSource.IsActive = false;
                paymentSource.DeleteDateTime = now;
                paymentSource.DeleteBy = actorUserId;
                paymentSource.UpdateDateTime = now;
                paymentSource.UpdateBy = actorUserId;
            }

            var cancelledQueues = await CancelQueuesByEncounterAsync(entity.Id, now, actorUserId, request?.DeleteReason ?? "Encounter deleted.");
            await _dbContext.SaveChangesAsync();

            foreach (var queue in cancelledQueues)
            {
                await _queueRealtimeService.NotifyQueueCancelledAsync(queue, actorUserId, "Patient encounter dihapus.");
            }

            return Ok(ApiResponse<object>.Ok(null, "Patient encounter berhasil dihapus."));
        }

        private IQueryable<TrxPatientEncounter> BuildBaseQuery()
        {
            return _dbContext.Set<TrxPatientEncounter>()
                .Include(x => x.Patient)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.Room)
                .Include(x => x.Doctor)
                .Include(x => x.PatientClass)
                .Include(x => x.AgeCategory)
                .Include(x => x.PaymentMethod)
                .Include(x => x.PaymentSource)
                .Include(x => x.RegisteredByUser)
                .Include(x => x.CancelledByUser)
                .Include(x => x.NoShowByUser)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<TrxPatientEncounter> ApplyDateFilter(IQueryable<TrxPatientEncounter> query, DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            if (startDate.HasValue)
            {
                var start = ToUtcDate(startDate.Value);
                query = query.Where(x => x.EncounterDate >= start);
            }

            if (endDate.HasValue)
            {
                var endExclusive = ToUtcDate(endDate.Value).AddDays(1);
                query = query.Where(x => x.EncounterDate < endExclusive);
            }

            if (!startDate.HasValue && !endDate.HasValue && !string.IsNullOrWhiteSpace(customPeriod))
            {
                var today = ToUtcDate(AppDateTimeHelper.OperationalDate());

                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        query = query.Where(x => x.EncounterDate >= today && x.EncounterDate < today.AddDays(1));
                        break;
                    case "last7days":
                        query = query.Where(x => x.EncounterDate >= today.AddDays(-6) && x.EncounterDate < today.AddDays(1));
                        break;
                    case "thismonth":
                        var thisMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        query = query.Where(x => x.EncounterDate >= thisMonthStart && x.EncounterDate < thisMonthStart.AddMonths(1));
                        break;
                    case "lastmonth":
                        var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        var lastMonthStart = currentMonthStart.AddMonths(-1);
                        query = query.Where(x => x.EncounterDate >= lastMonthStart && x.EncounterDate < currentMonthStart);
                        break;
                }
            }

            return query;
        }

        private static IQueryable<TrxPatientEncounter> ApplyRelationFilter(IQueryable<TrxPatientEncounter> query, Guid? patientId, Guid? serviceUnitId)
        {
            var normalizedPatientId = NormalizeNullableGuid(patientId);
            if (normalizedPatientId.HasValue) query = query.Where(x => x.PatientId == normalizedPatientId.Value);

            var normalizedServiceUnitId = NormalizeNullableGuid(serviceUnitId);
            if (normalizedServiceUnitId.HasValue) query = query.Where(x => x.ServiceUnitId == normalizedServiceUnitId.Value);

            return query;
        }

        private static IQueryable<TrxPatientEncounter> ApplyStandardFilter(
            IQueryable<TrxPatientEncounter> query,
            EncounterStatus? encounterStatus,
            EncounterType? encounterType,
            EncounterPaymentType? paymentType,
            bool? isReferral,
            bool? isActive,
            string? search)
        {
            if (encounterStatus.HasValue)
            {
                query = query.Where(x => x.EncounterStatus == encounterStatus.Value);
            }

            if (encounterType.HasValue)
            {
                query = query.Where(x => x.EncounterType == encounterType.Value);
            }

            if (paymentType.HasValue)
            {
                query = query.Where(x => x.PaymentType == paymentType.Value);
            }

            if (isReferral.HasValue)
            {
                query = query.Where(x => x.IsReferral == isReferral.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.EncounterNumber.ToLower().Contains(keyword) ||
                    (x.Patient != null && x.Patient.PatientCode.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitName.ToLower().Contains(keyword)) ||
                    (x.Clinic != null && x.Clinic.ClinicName.ToLower().Contains(keyword)) ||
                    (x.Room != null && x.Room.RoomName.ToLower().Contains(keyword)) ||
                    (x.Room != null && x.Room.RoomNumber != null && x.Room.RoomNumber.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.FullName.ToLower().Contains(keyword)) ||
                    (x.PaymentMethod != null && x.PaymentMethod.PaymentMethodName.ToLower().Contains(keyword)) ||
                    (x.AgeCategoryCodeSnapshot != null && x.AgeCategoryCodeSnapshot.ToLower().Contains(keyword)) ||
                    (x.AgeCategoryNameSnapshot != null && x.AgeCategoryNameSnapshot.ToLower().Contains(keyword)) ||
                    (x.AgeTextAtEncounter != null && x.AgeTextAtEncounter.ToLower().Contains(keyword)) ||
                    (x.PaymentSource != null &&
                     x.PaymentSource.PaymentSourceNameSnapshot != null &&
                     x.PaymentSource.PaymentSourceNameSnapshot.ToLower().Contains(keyword)) ||
                    (x.ReferralNumber != null && x.ReferralNumber.ToLower().Contains(keyword)));
            }

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(
            PatientEncounterCreateRequest request,
            DateTime targetEncounterDate,
            DateTime operationalDate)
        {
            if (request.PatientId == Guid.Empty)
            {
                return (false, "Pasien wajib dipilih.");
            }

            if (request.ServiceUnitId == Guid.Empty)
            {
                return (false, "Service unit wajib dipilih.");
            }

            if (!Enum.IsDefined(typeof(EncounterType), request.EncounterType))
            {
                return (false, "Tipe encounter tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (!Enum.IsDefined(typeof(VisitType), request.VisitType))
            {
                return (false, "Tipe visit tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (!Enum.IsDefined(typeof(EncounterRegistrationSource), request.RegistrationSource))
            {
                return (false, "Sumber registrasi tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (request.PaymentType != EncounterPaymentType.Cash &&
                request.PaymentType != EncounterPaymentType.Insurance)
            {
                return (false, "Tipe pembayaran registrasi hanya mendukung Tunai atau Asuransi.");
            }

            var patientExists = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == request.PatientId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!patientExists)
            {
                return (false, "Pasien tidak valid atau tidak aktif.");
            }

            var serviceUnitExists = await _dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == request.ServiceUnitId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.IsAvailableForRegistration);

            if (!serviceUnitExists)
            {
                return (false, "Service unit tidak valid, tidak aktif, atau tidak tersedia untuk registrasi.");
            }

            if (request.ClinicId.HasValue && request.ClinicId.Value != Guid.Empty)
            {
                var clinicExists = await _dbContext.Set<MstClinic>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == request.ClinicId.Value &&
                        x.ServiceUnitId == request.ServiceUnitId &&
                        x.IsActive &&
                        !x.IsDelete &&
                        x.IsAvailableForRegistration);

                if (!clinicExists)
                {
                    return (false, "Clinic tidak valid, tidak aktif, atau tidak tersedia untuk registrasi.");
                }
            }

            if (request.DoctorId.HasValue && request.DoctorId.Value != Guid.Empty)
            {
                var doctorExists = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == request.DoctorId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!doctorExists)
                {
                    return (false, "Dokter tidak valid atau tidak aktif.");
                }
            }

            if (targetEncounterDate < operationalDate)
            {
                return (false, "Tanggal kunjungan tidak boleh lebih kecil dari tanggal operasional.");
            }

            if (request.IsAppointment &&
                !request.VisitDate.HasValue &&
                request.DoctorScheduleId.HasValue)
            {
                var scheduleType = await _dbContext.Set<MstDoctorSchedule>()
                    .AsNoTracking()
                    .Where(x =>
                        x.Id == request.DoctorScheduleId.Value &&
                        !x.IsDelete)
                    .Select(x => x.ScheduleType)
                    .FirstOrDefaultAsync();

                if (scheduleType == DoctorScheduleType.WeeklyRecurring)
                {
                    return (false, "Tanggal kunjungan wajib diisi untuk booking jadwal dokter mingguan.");
                }
            }

            if (request.DoctorScheduleId.HasValue &&
                request.DoctorScheduleId.Value != Guid.Empty)
            {
                var scheduleValidation = await ValidateDoctorScheduleForEncounterAsync(
                    request,
                    targetEncounterDate);

                if (!scheduleValidation.IsValid)
                {
                    return scheduleValidation;
                }
            }

            if (request.DoctorServiceRuleId.HasValue &&
                request.DoctorServiceRuleId.Value != Guid.Empty)
            {
                var ruleExists = await _dbContext.Set<MstDoctorServiceRule>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == request.DoctorServiceRuleId.Value &&
                        x.IsActive &&
                        !x.IsDelete &&
                        (!request.DoctorId.HasValue || x.DoctorId == request.DoctorId.Value) &&
                        x.ServiceUnitId == request.ServiceUnitId);

                if (!ruleExists)
                {
                    return (false, "Doctor service rule tidak valid, tidak aktif, atau tidak sesuai dokter/service unit.");
                }
            }

            if (request.PatientClassId.HasValue &&
                request.PatientClassId.Value != Guid.Empty)
            {
                var patientClassExists = await _dbContext.Set<MstPatientClass>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == request.PatientClassId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!patientClassExists)
                {
                    return (false, "Patient class tidak valid atau tidak aktif.");
                }
            }

            if (request.PaymentType == EncounterPaymentType.Cash)
            {
                if (request.PatientInsuranceId.HasValue &&
                    request.PatientInsuranceId.Value != Guid.Empty)
                {
                    return (false, "PatientInsuranceId harus kosong untuk pembayaran Tunai.");
                }

                if (request.PaymentMethodId.HasValue &&
                    request.PaymentMethodId.Value != Guid.Empty)
                {
                    var paymentMethodExists = await _dbContext.Set<MstPaymentMethod>()
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.Id == request.PaymentMethodId.Value &&
                            x.IsActive &&
                            !x.IsDelete &&
                            x.IsAvailableForRegistration);

                    if (!paymentMethodExists)
                    {
                        return (false, "Metode pembayaran tidak valid atau tidak tersedia untuk registrasi.");
                    }
                }
            }
            else
            {
                if (request.PaymentMethodId.HasValue &&
                    request.PaymentMethodId.Value != Guid.Empty)
                {
                    return (false, "PaymentMethodId harus kosong untuk pembayaran Asuransi.");
                }

                if (!request.PatientInsuranceId.HasValue ||
                    request.PatientInsuranceId.Value == Guid.Empty)
                {
                    return (false, "PatientInsuranceId wajib diisi untuk pembayaran Asuransi.");
                }

                var insuranceResult = await LoadValidPatientInsuranceAsync(
                    request.PatientId,
                    request.PatientInsuranceId.Value,
                    targetEncounterDate);

                if (insuranceResult.Insurance == null)
                {
                    return (false, insuranceResult.ErrorMessage ?? "Asuransi pasien tidak valid.");
                }
            }

            if (request.KioskScanSessionId.HasValue &&
                request.KioskScanSessionId.Value != Guid.Empty)
            {
                var scanSession = await _dbContext.Set<TrxKioskScanSession>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.KioskScanSessionId.Value &&
                        !x.IsDelete &&
                        !x.IsUsedForRegistration);

                if (scanSession == null)
                {
                    return (false, "Kiosk scan session tidak valid atau sudah digunakan.");
                }

                if (scanSession.PatientId.HasValue &&
                    scanSession.PatientId.Value != request.PatientId)
                {
                    return (false, "Kiosk scan session tidak sesuai dengan pasien yang dipilih.");
                }
            }

            if (request.IsReferralRequired && !request.IsReferral)
            {
                return (false, "Referral wajib ditandai aktif jika rujukan diperlukan.");
            }

            if (request.IsReferral &&
                request.IsReferralVerified &&
                string.IsNullOrWhiteSpace(request.ReferralNumber))
            {
                return (false, "Nomor referral wajib diisi jika referral sudah diverifikasi.");
            }

            return (true, null);
        }

        private async Task<PatientClassResolution> ResolvePatientClassAsync(
            PatientEncounterCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            // Untuk rawat jalan, kelas ditentukan penuh oleh backend. Nilai yang
            // mungkin dikirim frontend sengaja tidak dipakai agar kiosk, front desk,
            // appointment, dan mobile menghasilkan konteks tarif yang sama.
            if (request.EncounterType == EncounterType.Outpatient)
            {
                var activeClasses = await _dbContext.Set<MstPatientClass>()
                    .AsNoTracking()
                    .Where(x => x.IsActive && !x.IsDelete)
                    .OrderBy(x => x.PatientClassName)
                    .ThenBy(x => x.Id)
                    .ToListAsync(cancellationToken);

                var expectedName = NormalizeLookupText(
                    DefaultOutpatientPatientClassName);

                var matches = activeClasses
                    .Where(x => NormalizeLookupText(x.PatientClassName) == expectedName)
                    .ToList();

                if (matches.Count == 0)
                {
                    return PatientClassResolution.Fail(
                        $"Master kelas pasien '{DefaultOutpatientPatientClassName}' " +
                        "tidak ditemukan atau tidak aktif.");
                }

                if (matches.Count > 1)
                {
                    return PatientClassResolution.Fail(
                        $"Ditemukan lebih dari satu master kelas aktif bernama " +
                        $"'{DefaultOutpatientPatientClassName}'. Nonaktifkan atau " +
                        "rapikan data duplikat agar pemilihan tarif tidak ambigu.");
                }

                return PatientClassResolution.Success(
                    matches[0],
                    isAssignedAutomatically: true);
            }

            var requestedPatientClassId =
                NormalizeNullableGuid(request.PatientClassId);

            if (!requestedPatientClassId.HasValue)
            {
                return PatientClassResolution.Success(
                    patientClass: null,
                    isAssignedAutomatically: false);
            }

            var requestedClass = await _dbContext.Set<MstPatientClass>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == requestedPatientClassId.Value &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            if (requestedClass == null)
            {
                return PatientClassResolution.Fail(
                    "Patient class tidak ditemukan, tidak aktif, atau sudah dihapus.");
            }

            return PatientClassResolution.Success(
                requestedClass,
                isAssignedAutomatically: false);
        }

        private async Task<(bool IsValid, string? ErrorMessage, MstRoom? Room)> ResolveEncounterRoomAsync(
            PatientEncounterCreateRequest request)
        {
            var requestedRoomId = NormalizeNullableGuid(request.RoomId);
            Guid? scheduleRoomId = null;

            if (request.DoctorScheduleId.HasValue &&
                request.DoctorScheduleId.Value != Guid.Empty)
            {
                scheduleRoomId = await _dbContext.Set<MstDoctorSchedule>()
                    .AsNoTracking()
                    .Where(x =>
                        x.Id == request.DoctorScheduleId.Value &&
                        !x.IsDelete)
                    .Select(x => x.RoomId)
                    .FirstOrDefaultAsync();
            }

            if (requestedRoomId.HasValue &&
                scheduleRoomId.HasValue &&
                requestedRoomId.Value != scheduleRoomId.Value)
            {
                return (
                    false,
                    "Ruangan yang dipilih tidak sesuai dengan ruangan pada jadwal dokter.",
                    null);
            }

            var resolvedRoomId = scheduleRoomId ?? requestedRoomId;
            if (!resolvedRoomId.HasValue)
            {
                return (true, null, null);
            }

            var room = await _dbContext.Set<MstRoom>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == resolvedRoomId.Value &&
                    x.IsActive &&
                    !x.IsDelete);

            if (room == null)
            {
                return (false, "Ruangan tidak ditemukan, tidak aktif, atau sudah dihapus.", null);
            }

            if (room.ServiceUnitId != request.ServiceUnitId)
            {
                return (false, "Ruangan tidak sesuai dengan service unit pada encounter.", null);
            }

            var patientClassId = NormalizeNullableGuid(request.PatientClassId);
            if (room.PatientClassId.HasValue &&
                patientClassId.HasValue &&
                room.PatientClassId.Value != patientClassId.Value)
            {
                return (false, "Ruangan tidak sesuai dengan patient class yang dipilih.", null);
            }

            return (true, null, room);
        }

        private async Task<(MstPatientInsurance? Insurance, string? ErrorMessage)> LoadValidPatientInsuranceAsync(
            Guid patientId,
            Guid patientInsuranceId,
            DateTime encounterDate)
        {
            if (patientInsuranceId == Guid.Empty)
            {
                return (null, "PatientInsuranceId wajib diisi.");
            }

            var insurance = await _dbContext.Set<MstPatientInsurance>()
                .AsNoTracking()
                .Include(x => x.InsuranceProvider)
                .FirstOrDefaultAsync(x =>
                    x.Id == patientInsuranceId &&
                    !x.IsDelete);

            if (insurance == null)
            {
                return (null, "Asuransi pasien tidak ditemukan.");
            }

            if (insurance.PatientId != patientId)
            {
                return (null, "Asuransi yang dipilih bukan milik pasien pada encounter.");
            }

            if (!insurance.IsActive)
            {
                return (null, "Asuransi pasien tidak aktif.");
            }

            if (!insurance.IsEligible)
            {
                return (null, "Asuransi pasien tidak eligible.");
            }

            if (insurance.InsuranceProvider == null ||
                !insurance.InsuranceProvider.IsActive ||
                insurance.InsuranceProvider.IsDelete)
            {
                return (null, "Provider asuransi tidak valid atau tidak aktif.");
            }

            var encounterDay = ToUtcDate(encounterDate);

            if (insurance.EffectiveStartDate.HasValue &&
                ToUtcDate(insurance.EffectiveStartDate.Value) > encounterDay)
            {
                return (null, "Polis asuransi belum berlaku pada tanggal kunjungan.");
            }

            if (insurance.EffectiveEndDate.HasValue &&
                ToUtcDate(insurance.EffectiveEndDate.Value) < encounterDay)
            {
                return (null, "Polis asuransi sudah kedaluwarsa pada tanggal kunjungan.");
            }

            return (insurance, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage, DateTime TargetDate)> ResolveTargetEncounterDateAsync(
            PatientEncounterCreateRequest request,
            DateTime operationalDate)
        {
            var requestedVisitDate = request.VisitDate;

            if (requestedVisitDate.HasValue)
            {
                return (true, null, ToUtcDate(requestedVisitDate.Value));
            }

            if (request.IsAppointment && request.DoctorScheduleId.HasValue && request.DoctorScheduleId.Value != Guid.Empty)
            {
                var schedule = await _dbContext.Set<MstDoctorSchedule>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.DoctorScheduleId.Value &&
                        !x.IsDelete);

                if (schedule == null)
                {
                    return (false, "Jadwal dokter tidak ditemukan.", operationalDate);
                }

                if (schedule.ScheduleType != DoctorScheduleType.WeeklyRecurring && schedule.PracticeDate.HasValue)
                {
                    return (true, null, ToUtcDate(schedule.PracticeDate.Value));
                }

                return (false, "Tanggal kunjungan wajib diisi untuk booking jadwal dokter mingguan.", operationalDate);
            }

            return (true, null, operationalDate);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateDoctorScheduleForEncounterAsync(
            PatientEncounterCreateRequest request,
            DateTime targetEncounterDate)
        {
            var schedule = await _dbContext.Set<MstDoctorSchedule>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.DoctorScheduleId!.Value &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.ScheduleStatus == DoctorScheduleStatus.Active &&
                    (!request.DoctorId.HasValue || x.DoctorId == request.DoctorId.Value) &&
                    x.ServiceUnitId == request.ServiceUnitId &&
                    (!request.ClinicId.HasValue || x.ClinicId == request.ClinicId.Value));

            if (schedule == null)
            {
                return (false, "Jadwal dokter tidak valid, tidak aktif, tidak berstatus active, atau tidak sesuai dokter/service unit/clinic.");
            }

            if (request.IsFromKiosk && !schedule.IsAllowKioskRegistration)
            {
                return (false, "Jadwal dokter tidak tersedia untuk registrasi kiosk.");
            }

            if (request.IsWalkIn && !schedule.IsAllowWalkIn)
            {
                return (false, "Jadwal dokter tidak menerima pasien walk-in.");
            }

            if (request.IsAppointment && !schedule.IsAllowAppointment)
            {
                return (false, "Jadwal dokter tidak menerima appointment.");
            }

            var visitDate = ToUtcDate(targetEncounterDate);

            if (!IsDoctorScheduleValidForVisitDate(schedule, visitDate))
            {
                return (false, "Jadwal dokter tidak sesuai dengan tanggal kunjungan.");
            }

            var quotaValidation = await ValidateDoctorScheduleQuotaAsync(
                schedule,
                request,
                visitDate);

            if (!quotaValidation.IsValid)
            {
                return quotaValidation;
            }

            return (true, null);
        }

        private static bool IsDoctorScheduleValidForVisitDate(
            MstDoctorSchedule schedule,
            DateTime visitDate)
        {
            var normalizedVisitDate = ToUtcDate(visitDate);

            if (schedule.EffectiveStartDate.HasValue &&
                ToUtcDate(schedule.EffectiveStartDate.Value) > normalizedVisitDate)
            {
                return false;
            }

            if (schedule.EffectiveEndDate.HasValue &&
                ToUtcDate(schedule.EffectiveEndDate.Value) < normalizedVisitDate)
            {
                return false;
            }

            if (schedule.ScheduleType == DoctorScheduleType.WeeklyRecurring)
            {
                return schedule.PracticeDay == normalizedVisitDate.DayOfWeek;
            }

            return schedule.PracticeDate.HasValue &&
                ToUtcDate(schedule.PracticeDate.Value) == normalizedVisitDate;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateDoctorScheduleQuotaAsync(
            MstDoctorSchedule schedule,
            PatientEncounterCreateRequest request,
            DateTime visitDate)
        {
            var normalizedVisitDate = ToUtcDate(visitDate);

            var queueQuery = _dbContext.Set<TrxQueue>()
                .AsNoTracking()
                .Where(x =>
                    x.QueueDate == normalizedVisitDate &&
                    x.DoctorScheduleId == schedule.Id &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    !x.CancelledAt.HasValue);

            if (schedule.MaxPatientQuota > 0)
            {
                var totalQueue = await queueQuery.CountAsync();

                if (totalQueue >= schedule.MaxPatientQuota)
                {
                    return (false, "Kuota pasien pada jadwal dokter sudah penuh.");
                }
            }

            if (request.IsAppointment && schedule.MaxAppointmentQuota > 0)
            {
                var appointmentQueue = await queueQuery.CountAsync(x => x.IsAppointment);

                if (appointmentQueue >= schedule.MaxAppointmentQuota)
                {
                    return (false, "Kuota appointment pada jadwal dokter sudah penuh.");
                }
            }

            if (request.IsWalkIn && schedule.MaxWalkInQuota > 0)
            {
                var walkInQueue = await queueQuery.CountAsync(x => x.IsWalkIn);

                if (walkInQueue >= schedule.MaxWalkInQuota)
                {
                    return (false, "Kuota walk-in pada jadwal dokter sudah penuh.");
                }
            }

            return (true, null);
        }

        private async Task<TrxPatientEncounterGuarantor> BuildPaymentSourceAsync(
            Guid encounterId,
            PatientEncounterCreateRequest request,
            MstPatientInsurance? patientInsurance,
            DateTime now,
            Guid actorUserId)
        {
            var entity = new TrxPatientEncounterGuarantor
            {
                Id = Guid.NewGuid(),
                PaymentSourceNumber = await GeneratePaymentSourceNumberAsync(),
                EncounterId = encounterId,
                PatientId = request.PatientId,
                PaymentType = request.PaymentType,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            if (request.PaymentType == EncounterPaymentType.Cash)
            {
                entity.PaymentMethodId = NormalizeNullableGuid(request.PaymentMethodId);
                entity.PaymentSourceNameSnapshot = "Tunai";
                entity.IsEligible = true;
                entity.IsPolicyActive = false;
                return entity;
            }

            if (patientInsurance == null)
            {
                throw new InvalidOperationException(
                    "Asuransi pasien wajib tersedia untuk membentuk sumber pembayaran Asuransi.");
            }

            entity.PatientInsuranceId = patientInsurance.Id;
            entity.InsuranceProviderId = patientInsurance.InsuranceProviderId;
            entity.PaymentSourceNameSnapshot =
                patientInsurance.InsuranceProvider?.InsuranceProviderName;
            entity.PolicyNumberSnapshot = NormalizeNullableText(patientInsurance.PolicyNumber);
            entity.CardNumberSnapshot = NormalizeNullableText(patientInsurance.CardNumber);
            entity.MemberNumberSnapshot = NormalizeNullableText(patientInsurance.MemberNumber);
            entity.PlanNameSnapshot = NormalizeNullableText(patientInsurance.PlanName);
            entity.ClassNameSnapshot = NormalizeNullableText(patientInsurance.ClassName);
            entity.BenefitPlanCodeSnapshot = NormalizeNullableText(patientInsurance.BenefitPlanCode);
            entity.EffectiveStartDateSnapshot = patientInsurance.EffectiveStartDate;
            entity.EffectiveEndDateSnapshot = patientInsurance.EffectiveEndDate;
            entity.IsEligible = patientInsurance.IsEligible;
            entity.IsPolicyActive = true;

            return entity;
        }





        private static void ApplyEncounterPaymentSummary(
            TrxPatientEncounter encounter,
            TrxPatientEncounterGuarantor paymentSource)
        {
            encounter.PaymentType = paymentSource.PaymentType;
            encounter.PaymentMethodId = paymentSource.PaymentType == EncounterPaymentType.Cash
                ? paymentSource.PaymentMethodId
                : null;
            encounter.PaymentSource = paymentSource;
        }

        private async Task<List<TrxQueue>> CancelQueuesByEncounterAsync(Guid encounterId, DateTime now, Guid actorUserId, string reason)
        {
            var queues = await _dbContext.Set<TrxQueue>().Where(x => x.EncounterId == encounterId && !x.IsDelete && !x.CompletedAt.HasValue && !x.CancelledAt.HasValue).ToListAsync();

            foreach (var queue in queues)
            {
                queue.CancelledAt = now;
                queue.CancelledByUserId = actorUserId;
                queue.CancelReason = reason;
                queue.IsCancel = true;
                queue.CancelDateTime = now;
                queue.CancelBy = actorUserId;
                queue.IsActive = false;
                queue.UpdateDateTime = now;
                queue.UpdateBy = actorUserId;
            }

            return queues;
        }

        private async Task<string> GenerateEncounterNumberAsync()
        {
            return await GenerateRunningCodeAsync<TrxPatientEncounter>(selector: x => x.EncounterNumber, prefix: EncounterCodePrefix);
        }

        private async Task<string> GeneratePaymentSourceNumberAsync()
        {
            // Prefix lama dipertahankan agar penomoran data existing tetap berlanjut.
            return await GenerateRunningCodeAsync<TrxPatientEncounterGuarantor>(
                selector: x => x.PaymentSourceNumber,
                prefix: PaymentSourceCodePrefix);
        }

        private async Task<string> GenerateRunningCodeAsync<TEntity>(Expression<Func<TEntity, string>> selector, string prefix) where TEntity : class
        {
            var existingCodes = await _dbContext.Set<TEntity>().IgnoreQueryFilters().AsNoTracking().Select(selector).Where(x => x.StartsWith(prefix)).ToListAsync();
            var usedNumbers = existingCodes.Select(x => x.Replace(prefix, string.Empty)).Where(x => int.TryParse(x, out _)).Select(int.Parse).Where(x => x > 0).ToHashSet();
            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber)) nextNumber++;

            return prefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
        }

        private async Task<int> GenerateQueueNumberAsync(DateTime operationalDate, Guid serviceUnitId, Guid? clinicId)
        {
            var normalizedClinicId = NormalizeNullableGuid(clinicId);
            var queueDate = ToUtcDate(operationalDate);

            var baseQuery = _dbContext.Set<TrxQueue>()
                .AsNoTracking()
                .Where(x =>
                    x.QueueDate == queueDate &&
                    x.ServiceUnitId == serviceUnitId &&
                    !x.IsDelete);

            if (!normalizedClinicId.HasValue)
            {
                return await GetNextQueueNumberAsync(baseQuery);
            }

            var clusterIds = await _dbContext.Set<MstNurseStationClusterClinic>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.ClinicId == normalizedClinicId.Value)
                .Select(x => x.NurseStationClusterId)
                .Distinct()
                .ToListAsync();

            if (!clusterIds.Any())
            {
                var clinicOnlyQuery = baseQuery.Where(x => x.ClinicId == normalizedClinicId.Value);
                return await GetNextQueueNumberAsync(clinicOnlyQuery);
            }

            var clinicIdsInCluster = await _dbContext.Set<MstNurseStationClusterClinic>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    clusterIds.Contains(x.NurseStationClusterId))
                .Select(x => x.ClinicId)
                .Distinct()
                .ToListAsync();

            if (!clinicIdsInCluster.Any())
            {
                clinicIdsInCluster.Add(normalizedClinicId.Value);
            }

            var clusterQueueQuery = baseQuery.Where(x =>
                x.ClinicId.HasValue &&
                clinicIdsInCluster.Contains(x.ClinicId.Value));

            return await GetNextQueueNumberAsync(clusterQueueQuery);
        }

        private static async Task<int> GetNextQueueNumberAsync(IQueryable<TrxQueue> query)
        {
            var lastQueueNumber = await query
                .Select(x => (int?)x.QueueNumber)
                .MaxAsync();

            return lastQueueNumber.GetValueOrDefault() + 1;
        }

        private static string GenerateQueueCode(MstClinic? clinic, int queueNumber)
        {
            var prefix = BuildQueueCodePrefix(clinic);
            return $"{prefix}{queueNumber:D3}";
        }

        private static string BuildQueueCodePrefix(MstClinic? clinic)
        {
            var source = !string.IsNullOrWhiteSpace(clinic?.ShortName)
                ? clinic.ShortName.Trim()
                : BuildClinicNamePrefixSource(clinic?.ClinicName);

            if (string.IsNullOrWhiteSpace(source))
            {
                return "Q";
            }

            foreach (var character in source.Trim().ToUpperInvariant())
            {
                if (character >= 'A' && character <= 'Z')
                {
                    return character.ToString();
                }
            }

            return "Q";
        }

        private static string BuildClinicNamePrefixSource(string? clinicName)
        {
            if (string.IsNullOrWhiteSpace(clinicName))
            {
                return string.Empty;
            }

            var ignoredWords = new[] { "POLI", "POLIKLINIK", "KLINIK", "CLINIC" };

            var tokens = clinicName
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var token in tokens)
            {
                if (!ignoredWords.Contains(token.ToUpperInvariant()))
                {
                    return token;
                }
            }

            return clinicName.Trim();
        }

        private async Task<PatientEncounterAgeSnapshot> BuildAgeSnapshotAsync(
            DateTime? birthDate,
            DateTime encounterDate,
            DateTime calculatedAt)
        {
            if (!birthDate.HasValue)
            {
                return PatientEncounterAgeSnapshot.Empty;
            }

            var birth = birthDate.Value.Date;
            var referenceDate = encounterDate.Date;

            if (referenceDate < birth)
            {
                return PatientEncounterAgeSnapshot.Empty;
            }

            var ageParts = CalculateAgeParts(birth, referenceDate);
            var ageReferenceDate = DateTime.SpecifyKind(referenceDate, DateTimeKind.Utc);
            var ageCalculatedAt = DateTime.SpecifyKind(calculatedAt, DateTimeKind.Utc);

            var category = await _dbContext.Set<MstAgeCategory>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.MinAgeDays <= ageParts.TotalDays &&
                    (x.MaxAgeDays == null || x.MaxAgeDays >= ageParts.TotalDays) &&
                    (x.EffectiveStartDate == null || x.EffectiveStartDate <= ageReferenceDate) &&
                    (x.EffectiveEndDate == null || x.EffectiveEndDate >= ageReferenceDate))
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.MinAgeDays)
                .FirstOrDefaultAsync();

            return new PatientEncounterAgeSnapshot
            {
                AgeCategoryId = category?.Id,
                AgeCategoryCode = category?.AgeCategoryCode,
                AgeCategoryName = category?.AgeCategoryName,
                AgeYear = ageParts.Years,
                AgeMonth = ageParts.Months,
                AgeDay = ageParts.Days,
                TotalAgeDays = ageParts.TotalDays,
                AgeText = BuildAgeText(ageParts.Years, ageParts.Months, ageParts.Days),
                AgeReferenceDate = ageReferenceDate,
                AgeCalculatedAt = ageCalculatedAt
            };
        }

        private static PatientEncounterAgeParts CalculateAgeParts(DateTime birthDate, DateTime referenceDate)
        {
            var years = referenceDate.Year - birthDate.Year;
            var months = referenceDate.Month - birthDate.Month;
            var days = referenceDate.Day - birthDate.Day;

            if (days < 0)
            {
                var previousMonth = referenceDate.AddMonths(-1);
                days += DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
                months--;
            }

            if (months < 0)
            {
                months += 12;
                years--;
            }

            var totalDays = (referenceDate - birthDate).Days;

            return new PatientEncounterAgeParts
            {
                Years = years,
                Months = months,
                Days = days,
                TotalDays = totalDays
            };
        }

        private static string BuildAgeText(int years, int months, int days)
        {
            var parts = new List<string>();

            if (years > 0)
            {
                parts.Add($"{years} tahun");
            }

            if (months > 0)
            {
                parts.Add($"{months} bulan");
            }

            if (days > 0 || parts.Count == 0)
            {
                parts.Add($"{days} hari");
            }

            return string.Join(" ", parts);
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> actorIds)
        {
            var ids = actorIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (!ids.Any()) return new Dictionary<Guid, string?>();

            return await _dbContext.Users.AsNoTracking().Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode }).ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static PatientEncounterResponse MapResponse(
            TrxPatientEncounter entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new PatientEncounterResponse
            {
                Id = entity.Id,
                EncounterNumber = entity.EncounterNumber,
                PatientId = entity.PatientId,
                PatientCode = entity.Patient?.PatientCode ?? string.Empty,
                PatientName = entity.Patient?.FullName ?? string.Empty,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber ?? string.Empty,
                ServiceUnitId = entity.ServiceUnitId,
                ServiceUnitName = entity.ServiceUnit?.ServiceUnitName ?? string.Empty,
                ClinicId = entity.ClinicId,
                ClinicName = entity.Clinic?.ClinicName,
                RoomId = entity.RoomId,
                RoomCode = entity.Room?.RoomCode,
                RoomName = entity.Room?.RoomName,
                RoomNumber = entity.Room?.RoomNumber,
                RoomLocationName = entity.Room?.LocationName,
                RoomFloorName = entity.Room?.FloorName,
                DoctorId = entity.DoctorId,
                DoctorName = entity.Doctor?.FullName,
                DoctorScheduleId = entity.DoctorScheduleId,
                DoctorServiceRuleId = entity.DoctorServiceRuleId,
                PatientClassId = entity.PatientClassId,
                PatientClassName = entity.PatientClass?.PatientClassName,
                AgeCategoryId = entity.AgeCategoryId,
                AgeCategoryCode = entity.AgeCategoryCodeSnapshot
                    ?? entity.AgeCategory?.AgeCategoryCode,
                AgeCategoryName = entity.AgeCategoryNameSnapshot
                    ?? entity.AgeCategory?.AgeCategoryName,
                AgeYearAtEncounter = entity.AgeYearAtEncounter,
                AgeMonthAtEncounter = entity.AgeMonthAtEncounter,
                AgeDayAtEncounter = entity.AgeDayAtEncounter,
                TotalAgeDaysAtEncounter = entity.TotalAgeDaysAtEncounter,
                AgeTextAtEncounter = entity.AgeTextAtEncounter,
                AgeReferenceDate = entity.AgeReferenceDate,
                AgeCalculatedAt = entity.AgeCalculatedAt,
                PaymentMethodId = entity.PaymentMethodId,
                PaymentMethodName = entity.PaymentMethod?.PaymentMethodName,
                EncounterDate = entity.EncounterDate,
                EncounterType = entity.EncounterType,
                EncounterTypeName = BuildEnumLabel(entity.EncounterType),
                VisitType = entity.VisitType,
                VisitTypeName = BuildEnumLabel(entity.VisitType),
                RegistrationSource = entity.RegistrationSource,
                RegistrationSourceName = BuildEnumLabel(entity.RegistrationSource),
                EncounterStatus = entity.EncounterStatus,
                EncounterStatusName = BuildEnumLabel(entity.EncounterStatus),
                PaymentType = entity.PaymentType,
                PaymentTypeName = BuildEnumLabel(entity.PaymentType),
                PaymentSourceNameSnapshot =
                    entity.PaymentSource?.PaymentSourceNameSnapshot,
                IsReferral = entity.IsReferral,
                IsReferralRequired = entity.IsReferralRequired,
                IsReferralVerified = entity.IsReferralVerified,
                IsNewPatient = entity.IsNewPatient,
                IsFromKiosk = entity.IsFromKiosk,
                IsWalkIn = entity.IsWalkIn,
                IsAppointment = entity.IsAppointment,
                IsScreeningRequired = entity.IsScreeningRequired,
                IsQueueRequired = entity.IsQueueRequired,
                IsDoctorRequired = entity.IsDoctorRequired,
                RegisteredAt = entity.RegisteredAt,
                RegisteredByUserId = entity.RegisteredByUserId,
                RegisteredByUserName = GetActorName(actorNames, entity.RegisteredByUserId),
                CheckedInAt = entity.CheckedInAt,
                CompletedAt = entity.CompletedAt,
                CancelledAt = entity.CancelledAt,
                NoShowAt = entity.NoShowAt,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static PatientEncounterDetailResponse MapDetailResponse(
            TrxPatientEncounter entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = new PatientEncounterDetailResponse
            {
                Id = entity.Id,
                EncounterNumber = entity.EncounterNumber,
                PatientId = entity.PatientId,
                PatientCode = entity.Patient?.PatientCode ?? string.Empty,
                PatientName = entity.Patient?.FullName ?? string.Empty,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber ?? string.Empty,
                ServiceUnitId = entity.ServiceUnitId,
                ServiceUnitName = entity.ServiceUnit?.ServiceUnitName ?? string.Empty,
                ClinicId = entity.ClinicId,
                ClinicName = entity.Clinic?.ClinicName,
                RoomId = entity.RoomId,
                RoomCode = entity.Room?.RoomCode,
                RoomName = entity.Room?.RoomName,
                RoomNumber = entity.Room?.RoomNumber,
                RoomLocationName = entity.Room?.LocationName,
                RoomFloorName = entity.Room?.FloorName,
                DoctorId = entity.DoctorId,
                DoctorName = entity.Doctor?.FullName,
                DoctorScheduleId = entity.DoctorScheduleId,
                DoctorServiceRuleId = entity.DoctorServiceRuleId,
                PatientClassId = entity.PatientClassId,
                PatientClassName = entity.PatientClass?.PatientClassName,
                AgeCategoryId = entity.AgeCategoryId,
                AgeCategoryCode = entity.AgeCategoryCodeSnapshot
                    ?? entity.AgeCategory?.AgeCategoryCode,
                AgeCategoryName = entity.AgeCategoryNameSnapshot
                    ?? entity.AgeCategory?.AgeCategoryName,
                AgeYearAtEncounter = entity.AgeYearAtEncounter,
                AgeMonthAtEncounter = entity.AgeMonthAtEncounter,
                AgeDayAtEncounter = entity.AgeDayAtEncounter,
                TotalAgeDaysAtEncounter = entity.TotalAgeDaysAtEncounter,
                AgeTextAtEncounter = entity.AgeTextAtEncounter,
                AgeReferenceDate = entity.AgeReferenceDate,
                AgeCalculatedAt = entity.AgeCalculatedAt,
                PaymentMethodId = entity.PaymentMethodId,
                PaymentMethodName = entity.PaymentMethod?.PaymentMethodName,
                KioskScanSessionId = entity.KioskScanSessionId,
                EncounterDate = entity.EncounterDate,
                EncounterType = entity.EncounterType,
                EncounterTypeName = BuildEnumLabel(entity.EncounterType),
                VisitType = entity.VisitType,
                VisitTypeName = BuildEnumLabel(entity.VisitType),
                RegistrationSource = entity.RegistrationSource,
                RegistrationSourceName = BuildEnumLabel(entity.RegistrationSource),
                EncounterStatus = entity.EncounterStatus,
                EncounterStatusName = BuildEnumLabel(entity.EncounterStatus),
                PaymentType = entity.PaymentType,
                PaymentTypeName = BuildEnumLabel(entity.PaymentType),
                ChiefComplaint = entity.ChiefComplaint,
                PaymentSourceNameSnapshot =
                    entity.PaymentSource?.PaymentSourceNameSnapshot,
                IsReferral = entity.IsReferral,
                ReferralNumber = entity.ReferralNumber,
                IsReferralRequired = entity.IsReferralRequired,
                IsReferralVerified = entity.IsReferralVerified,
                IsNewPatient = entity.IsNewPatient,
                IsFromKiosk = entity.IsFromKiosk,
                IsWalkIn = entity.IsWalkIn,
                IsAppointment = entity.IsAppointment,
                IsScreeningRequired = entity.IsScreeningRequired,
                IsQueueRequired = entity.IsQueueRequired,
                IsDoctorRequired = entity.IsDoctorRequired,
                RegisteredAt = entity.RegisteredAt,
                RegisteredByUserId = entity.RegisteredByUserId,
                RegisteredByUserName = GetActorName(actorNames, entity.RegisteredByUserId),
                CheckedInAt = entity.CheckedInAt,
                CompletedAt = entity.CompletedAt,
                CancelledAt = entity.CancelledAt,
                CancelledByUserId = entity.CancelledByUserId,
                CancelledByUserName = entity.CancelledByUserId.HasValue
                    ? GetActorName(actorNames, entity.CancelledByUserId.Value)
                    : null,
                CancelReason = entity.CancelReason,
                NoShowAt = entity.NoShowAt,
                NoShowByUserId = entity.NoShowByUserId,
                NoShowByUserName = entity.NoShowByUserId.HasValue
                    ? GetActorName(actorNames, entity.NoShowByUserId.Value)
                    : null,
                NoShowReason = entity.NoShowReason,
                Notes = entity.Notes,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy),
                Payment = entity.PaymentSource == null || entity.PaymentSource.IsDelete
                    ? null
                    : MapPaymentSourceResponse(entity.PaymentSource)
            };

            return response;
        }

        private static PatientEncounterOptionResponse MapOptionResponse(
            TrxPatientEncounter entity)
        {
            return new PatientEncounterOptionResponse
            {
                Id = entity.Id,
                EncounterNumber = entity.EncounterNumber,
                PatientId = entity.PatientId,
                PatientName = entity.Patient?.FullName ?? string.Empty,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber ?? string.Empty,
                ServiceUnitId = entity.ServiceUnitId,
                ServiceUnitName = entity.ServiceUnit?.ServiceUnitName ?? string.Empty,
                ClinicId = entity.ClinicId,
                ClinicName = entity.Clinic?.ClinicName,
                RoomId = entity.RoomId,
                RoomName = entity.Room?.RoomName,
                RoomNumber = entity.Room?.RoomNumber,
                DoctorId = entity.DoctorId,
                DoctorName = entity.Doctor?.FullName,
                AgeCategoryId = entity.AgeCategoryId,
                AgeCategoryName = entity.AgeCategoryNameSnapshot
                    ?? entity.AgeCategory?.AgeCategoryName,
                AgeTextAtEncounter = entity.AgeTextAtEncounter,
                EncounterStatus = entity.EncounterStatus,
                EncounterStatusName = BuildEnumLabel(entity.EncounterStatus),
                PaymentType = entity.PaymentType,
                EncounterDate = entity.EncounterDate,
                RegisteredAt = entity.RegisteredAt
            };
        }

        private static PatientEncounterPaymentResponse MapPaymentSourceResponse(
            TrxPatientEncounterGuarantor entity)
        {
            return new PatientEncounterPaymentResponse
            {
                Id = entity.Id,
                PaymentSourceNumber = entity.PaymentSourceNumber,
                EncounterId = entity.EncounterId,
                PatientId = entity.PatientId,
                PaymentType = entity.PaymentType,
                PaymentTypeName = BuildEnumLabel(entity.PaymentType),
                PaymentMethodId = entity.PaymentMethodId,
                PaymentMethodName = entity.PaymentMethod?.PaymentMethodName,
                PatientInsuranceId = entity.PatientInsuranceId,
                InsuranceProviderId = entity.InsuranceProviderId,
                InsuranceProviderName = entity.PaymentSourceNameSnapshot
                    ?? entity.InsuranceProvider?.InsuranceProviderName,
                PolicyNumberSnapshot = entity.PolicyNumberSnapshot,
                CardNumberSnapshot = entity.CardNumberSnapshot,
                MemberNumberSnapshot = entity.MemberNumberSnapshot,
                PlanNameSnapshot = entity.PlanNameSnapshot,
                ClassNameSnapshot = entity.ClassNameSnapshot,
                BenefitPlanCodeSnapshot = entity.BenefitPlanCodeSnapshot,
                EffectiveStartDateSnapshot = entity.EffectiveStartDateSnapshot,
                EffectiveEndDateSnapshot = entity.EffectiveEndDateSnapshot,
                IsEligible = entity.IsEligible,
                IsPolicyActive = entity.IsPolicyActive,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime
            };
        }



        private static IQueryable<TrxPatientEncounter> ApplySorting(IQueryable<TrxPatientEncounter> query, string? sortBy, string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "registeredAt").Trim().ToLowerInvariant() switch
            {
                "encounternumber" => isDescending ? query.OrderByDescending(x => x.EncounterNumber) : query.OrderBy(x => x.EncounterNumber),
                "encounterdate" => isDescending ? query.OrderByDescending(x => x.EncounterDate) : query.OrderBy(x => x.EncounterDate),
                "patientname" => isDescending ? query.OrderByDescending(x => x.Patient != null ? x.Patient.FullName : string.Empty) : query.OrderBy(x => x.Patient != null ? x.Patient.FullName : string.Empty),
                "medicalrecordnumber" => isDescending ? query.OrderByDescending(x => x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty) : query.OrderBy(x => x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty),
                "serviceunitname" => isDescending ? query.OrderByDescending(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty) : query.OrderBy(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty),
                "clinicname" => isDescending ? query.OrderByDescending(x => x.Clinic != null ? x.Clinic.ClinicName : string.Empty) : query.OrderBy(x => x.Clinic != null ? x.Clinic.ClinicName : string.Empty),
                "roomname" => isDescending ? query.OrderByDescending(x => x.Room != null ? x.Room.RoomName : string.Empty) : query.OrderBy(x => x.Room != null ? x.Room.RoomName : string.Empty),
                "doctorname" => isDescending ? query.OrderByDescending(x => x.Doctor != null ? x.Doctor.FullName : string.Empty) : query.OrderBy(x => x.Doctor != null ? x.Doctor.FullName : string.Empty),
                "encounterstatus" => isDescending ? query.OrderByDescending(x => x.EncounterStatus).ThenByDescending(x => x.RegisteredAt) : query.OrderBy(x => x.EncounterStatus).ThenBy(x => x.RegisteredAt),
                "paymenttype" => isDescending ? query.OrderByDescending(x => x.PaymentType).ThenByDescending(x => x.RegisteredAt) : query.OrderBy(x => x.PaymentType).ThenBy(x => x.RegisteredAt),
                "createdatetime" => isDescending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "updatedatetime" => isDescending ? query.OrderByDescending(x => x.UpdateDateTime).ThenByDescending(x => x.RegisteredAt) : query.OrderBy(x => x.UpdateDateTime).ThenBy(x => x.RegisteredAt),
                _ => isDescending ? query.OrderByDescending(x => x.RegisteredAt).ThenByDescending(x => x.EncounterNumber) : query.OrderBy(x => x.RegisteredAt).ThenBy(x => x.EncounterNumber)
            };
        }

        private static List<PatientEncounterEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Select(x => new PatientEncounterEnumOptionResponse { Value = Convert.ToInt32(x), Name = x.ToString(), Label = BuildEnumLabel(x) }).ToList();
        }

        private static string BuildEnumLabel<TEnum>(TEnum value) where TEnum : Enum
        {
            var memberInfo = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();
            var displayAttribute = memberInfo?
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .OfType<DisplayAttribute>()
                .FirstOrDefault();

            var displayName = displayAttribute?.GetName();

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                return displayName;
            }

            return SplitPascalCase(value.ToString());
        }

        private static string SplitPascalCase(string value)
        {
            return string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> actorNames, Guid actorId)
        {
            if (actorId == Guid.Empty) return null;
            return actorNames.TryGetValue(actorId, out var actorName) ? actorName : null;
        }

        private static DateTime ToUtcDate(DateTime value)
        {
            return DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;
            return (pageNumber, pageSize);
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty) return null;
            return value.Value;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string NormalizeLookupText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return string.Join(
                    " ",
                    value.Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries |
                        StringSplitOptions.TrimEntries))
                .ToUpperInvariant();
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(userIdValue, out var userId) ? userId : Guid.Empty;
        }

        private sealed class PatientClassResolution
        {
            private PatientClassResolution()
            {
            }

            public bool IsValid { get; private init; }
            public string? ErrorMessage { get; private init; }
            public MstPatientClass? PatientClass { get; private init; }
            public bool IsAssignedAutomatically { get; private init; }

            public static PatientClassResolution Success(
                MstPatientClass? patientClass,
                bool isAssignedAutomatically)
            {
                return new PatientClassResolution
                {
                    IsValid = true,
                    PatientClass = patientClass,
                    IsAssignedAutomatically = isAssignedAutomatically
                };
            }

            public static PatientClassResolution Fail(string errorMessage)
            {
                return new PatientClassResolution
                {
                    IsValid = false,
                    ErrorMessage = errorMessage,
                    PatientClass = null,
                    IsAssignedAutomatically = false
                };
            }
        }

        private class PatientEncounterAgeSnapshot
        {
            public static PatientEncounterAgeSnapshot Empty => new();
            public Guid? AgeCategoryId { get; set; }
            public string? AgeCategoryCode { get; set; }
            public string? AgeCategoryName { get; set; }
            public int? AgeYear { get; set; }
            public int? AgeMonth { get; set; }
            public int? AgeDay { get; set; }
            public int? TotalAgeDays { get; set; }
            public string? AgeText { get; set; }
            public DateTime? AgeReferenceDate { get; set; }
            public DateTime? AgeCalculatedAt { get; set; }
        }

        private class PatientEncounterAgeParts
        {
            public int Years { get; set; }
            public int Months { get; set; }
            public int Days { get; set; }
            public int TotalDays { get; set; }
        }

    }
}
