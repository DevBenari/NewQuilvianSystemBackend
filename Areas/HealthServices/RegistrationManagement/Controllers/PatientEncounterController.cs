using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
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
        Description = "Transaksi kunjungan pasien rawat jalan dan penjamin kunjungan",
        SortOrder = 2
    )]
    [Tags("Health Services / Registration Management / Patient Encounter")]
    public class PatientEncounterController : ControllerBase
    {
        private const string LogCategory = "HealthServices.RegistrationManagement";
        private const string KioskReadPolicy = "KioskRead";
        private const string EncounterCodePrefix = "ENC-RSMMC-";
        private const string EncounterGuarantorCodePrefix = "EGT-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PatientEncounterController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientEncounterFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Encounter", Description = "Melihat metadata filter patient encounter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientEncounter", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
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
                    new() { Value = "serviceUnitId", Label = "Service Unit", Endpoint = "/api/v1/health-services/master-data/service-units/options" }
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
                GuarantorTypeOptions = BuildEnumOptions<PatientEncounterGuarantorType>(),
                GuarantorStatusOptions = BuildEnumOptions<PatientEncounterGuarantorStatus>(),
                GuarantorRoleOptions = BuildEnumOptions<PatientEncounterGuarantorRole>(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(LogCategory, "PatientEncounter.GetFilterMetadata", "Mengambil metadata filter patient encounter.", result);
            return Ok(ApiResponse<PatientEncounterFilterMetadataResponse>.Ok(result, "Metadata filter patient encounter berhasil diambil."));
        }

        [HttpGet("summary")]
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
            query = ApplyStandardFilter(query, encounterStatus, encounterType, paymentType, null, null, null, null, null, isActive, null);

            var result = new PatientEncounterSummaryResponse
            {
                TotalEncounter = await query.CountAsync(),
                RegisteredEncounter = await query.CountAsync(x => x.EncounterStatus == EncounterStatus.Registered),
                WaitingForNurseEncounter = await query.CountAsync(x => x.EncounterStatus == EncounterStatus.WaitingForNurse),
                WaitingForDoctorEncounter = await query.CountAsync(x => x.EncounterStatus == EncounterStatus.WaitingForDoctor),
                CompletedEncounter = await query.CountAsync(x => x.CompletedAt.HasValue),
                CancelledEncounter = await query.CountAsync(x => x.CancelledAt.HasValue || x.IsCancel),
                NoShowEncounter = await query.CountAsync(x => x.NoShowAt.HasValue),
                InsuranceEncounter = await query.CountAsync(x => x.IsInsurancePatient),
                CompanyEncounter = await query.CountAsync(x => x.IsCompanyPatient),
                MembershipEncounter = await query.CountAsync(x => x.IsMembershipPatient),
                MixedPaymentEncounter = await query.CountAsync(x => x.IsMixedPayment),
                ReferralEncounter = await query.CountAsync(x => x.IsReferral),
                EligibilityRequiredEncounter = await query.CountAsync(x => x.IsEligibilityRequired),
                EligibilityCompletedEncounter = await query.CountAsync(x => x.IsEligibilityCompleted),
                FromKioskEncounter = await query.CountAsync(x => x.IsFromKiosk)
            };

            return Ok(ApiResponse<PatientEncounterSummaryResponse>.Ok(result, "Ringkasan patient encounter berhasil diambil."));
        }

        [HttpGet]
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
            [FromQuery] Guid? doctorId,
            [FromQuery] EncounterStatus? encounterStatus,
            [FromQuery] EncounterType? encounterType,
            [FromQuery] EncounterPaymentType? paymentType,
            [FromQuery] bool? isInsurancePatient,
            [FromQuery] bool? isCompanyPatient,
            [FromQuery] bool? isEligibilityRequired,
            [FromQuery] bool? isEligibilityCompleted,
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
            query = ApplyStandardFilter(query, encounterStatus, encounterType, paymentType, isInsurancePatient, isCompanyPatient, isEligibilityRequired, isEligibilityCompleted, isReferral, isActive, search);

            if (clinicId.HasValue && clinicId.Value != Guid.Empty) query = query.Where(x => x.ClinicId == clinicId.Value);
            if (doctorId.HasValue && doctorId.Value != Guid.Empty) query = query.Where(x => x.DoctorId == doctorId.Value);

            var totalData = await query.CountAsync();
            var entities = await ApplySorting(query, sortBy, sortDirection).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var actorNames = await GetActorNameMapAsync(entities.SelectMany(x => new[] { x.CreateBy, x.UpdateBy, x.RegisteredByUserId, x.CancelledByUserId ?? Guid.Empty, x.NoShowByUserId ?? Guid.Empty }));
            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

            var result = new ResponsePatientEncounterPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePatientEncounterPagedResult>.Ok(result, "Data kunjungan pasien berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<PatientEncounterOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Encounter", Description = "Melihat data pilihan patient encounter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientEncounter", "Read")]
        public async Task<IActionResult> GetEncounterOptions(
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
        [ProducesResponseType(typeof(ApiResponse<PatientEncounterDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Encounter", Description = "Melihat detail kunjungan pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientEncounter", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .Include(x => x.EncounterGuarantors.Where(g => !g.IsDelete))
                    .ThenInclude(x => x.PaymentMethod)
                .Include(x => x.EncounterGuarantors.Where(g => !g.IsDelete))
                    .ThenInclude(x => x.InsuranceProvider)
                .Include(x => x.EncounterGuarantors.Where(g => !g.IsDelete))
                    .ThenInclude(x => x.CompanyGuarantor)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Kunjungan pasien tidak ditemukan."));
            }

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy, entity.RegisteredByUserId, entity.CancelledByUserId ?? Guid.Empty, entity.NoShowByUserId ?? Guid.Empty });

            return Ok(ApiResponse<PatientEncounterDetailResponse>.Ok(MapDetailResponse(entity, actorNames), "Detail kunjungan pasien berhasil diambil."));
        }

        [HttpPost]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<PatientEncounterCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Patient Encounter", Description = "Membuat transaksi kunjungan pasien beserta penjamin", AccessType = AccessTypes.Create, SortOrder = 2)]        
        public async Task<IActionResult> CreateEncounter([FromBody] PatientEncounterCreateRequest request)
        {
            var validation = await ValidateCreateRequestAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data kunjungan pasien tidak valid."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var serviceUnit = await _dbContext.Set<MstServiceUnit>().AsNoTracking().FirstAsync(x => x.Id == request.ServiceUnitId);
            MstClinic? clinic = null;

            if (request.ClinicId.HasValue && request.ClinicId.Value != Guid.Empty)
            {
                clinic = await _dbContext.Set<MstClinic>().AsNoTracking().FirstAsync(x => x.Id == request.ClinicId.Value);
            }

            var isScreeningRequired = clinic?.IsScreeningRequired ?? serviceUnit.IsScreeningRequired;
            var isQueueRequired = clinic?.IsQueueRequired ?? serviceUnit.IsQueueRequired;
            var isDoctorRequired = clinic?.IsDoctorRequired ?? serviceUnit.IsDoctorRequired;
            var guarantorRequests = BuildDefaultGuarantorsIfEmpty(request);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var encounter = new TrxPatientEncounter
                {
                    Id = Guid.NewGuid(),
                    EncounterNumber = await GenerateEncounterNumberAsync(),
                    PatientId = request.PatientId,
                    ServiceUnitId = request.ServiceUnitId,
                    ClinicId = NormalizeNullableGuid(request.ClinicId),
                    DoctorId = NormalizeNullableGuid(request.DoctorId),
                    DoctorScheduleId = NormalizeNullableGuid(request.DoctorScheduleId),
                    DoctorServiceRuleId = NormalizeNullableGuid(request.DoctorServiceRuleId),
                    PatientClassId = NormalizeNullableGuid(request.PatientClassId),
                    PaymentMethodId = NormalizeNullableGuid(request.PaymentMethodId),
                    KioskScanSessionId = NormalizeNullableGuid(request.KioskScanSessionId),
                    EncounterDate = now,
                    EncounterType = request.EncounterType,
                    VisitType = request.VisitType,
                    RegistrationSource = request.RegistrationSource,
                    PaymentType = request.PaymentType,
                    EncounterStatus = isQueueRequired ? EncounterStatus.Queued : EncounterStatus.Registered,
                    ChiefComplaint = NormalizeNullableText(request.ChiefComplaint),
                    EligibilityReferenceNumber = NormalizeNullableText(request.EligibilityReferenceNumber),
                    EligibilityCheckedAt = request.EligibilityCheckedAt,
                    IsEligibilityRequired = request.IsEligibilityRequired,
                    IsEligibilityCompleted = request.IsEligibilityCompleted,
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

                _dbContext.Set<TrxPatientEncounter>().Add(encounter);

                var guarantors = new List<TrxPatientEncounterGuarantor>();

                foreach (var guarantorRequest in guarantorRequests.OrderBy(x => x.CoveragePriority))
                {
                    var guarantor = await BuildGuarantorEntityAsync(encounter.Id, request.PatientId, guarantorRequest, now, actorUserId);
                    guarantors.Add(guarantor);
                    _dbContext.Set<TrxPatientEncounterGuarantor>().Add(guarantor);
                }

                ApplyEncounterPaymentSummary(encounter, guarantors, now, actorUserId);

                TrxQueue? queue = null;

                if (isQueueRequired)
                {
                    var queueNumber = await GenerateQueueNumberAsync(now, request.ServiceUnitId, request.ClinicId, request.DoctorId);

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
                        QueueStatus = isScreeningRequired ? QueueStatus.WaitingForNurse : QueueStatus.WaitingForDoctor,
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
                    encounter.EncounterStatus = isScreeningRequired ? EncounterStatus.WaitingForNurse : EncounterStatus.WaitingForDoctor;
                }

                if (request.KioskScanSessionId.HasValue && request.KioskScanSessionId.Value != Guid.Empty)
                {
                    var scanSession = await _dbContext.Set<TrxKioskScanSession>().FirstOrDefaultAsync(x => x.Id == request.KioskScanSessionId.Value && !x.IsDelete);

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
                    EncounterStatusName = BuildEnumLabel(encounter.EncounterStatus),
                    QueueId = queue?.Id,
                    QueueCode = queue?.QueueCode,
                    QueueNumber = queue?.QueueNumber,
                    QueueStatus = queue?.QueueStatus,
                    QueueStatusName = queue?.QueueStatus != null ? BuildEnumLabel(queue.QueueStatus) : null,
                    IsQueueCreated = queue != null,
                    IsScreeningRequired = isScreeningRequired,
                    IsDoctorRequired = isDoctorRequired,
                    IsQueueRequired = isQueueRequired,
                    GuarantorCount = guarantors.Count,
                    Guarantors = guarantors.Select(MapGuarantorCreateResponse).ToList()
                };

                await _loggerService.InfoAsync(LogCategory, "PatientEncounter.CreateEncounter", "Membuat transaksi kunjungan pasien beserta penjamin.", response);

                return Ok(ApiResponse<PatientEncounterCreateResponse>.Ok(response, "Transaksi kunjungan pasien berhasil dibuat."));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggerService.ErrorAsync(LogCategory, "PatientEncounter.CreateEncounter", "Gagal membuat transaksi kunjungan pasien.", ex);

                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail(StatusCodes.Status500InternalServerError, "Terjadi kesalahan saat membuat transaksi kunjungan pasien."));
            }
        }

        [HttpPatch("{id:guid}/status")]
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

            await CancelQueuesByEncounterAsync(entity.Id, now, actorUserId, request.CancelReason.Trim());
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Patient encounter berhasil dibatalkan."));
        }

        [HttpDelete("{id:guid}")]
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

            var guarantors = await _dbContext.Set<TrxPatientEncounterGuarantor>().Where(x => x.EncounterId == id && !x.IsDelete).ToListAsync();

            foreach (var guarantor in guarantors)
            {
                guarantor.IsDelete = true;
                guarantor.IsActive = false;
                guarantor.DeleteDateTime = now;
                guarantor.DeleteBy = actorUserId;
                guarantor.UpdateDateTime = now;
                guarantor.UpdateBy = actorUserId;
            }

            await CancelQueuesByEncounterAsync(entity.Id, now, actorUserId, request?.DeleteReason ?? "Encounter deleted.");
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Patient encounter berhasil dihapus."));
        }

        private IQueryable<TrxPatientEncounter> BuildBaseQuery()
        {
            return _dbContext.Set<TrxPatientEncounter>()
                .Include(x => x.Patient)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.Doctor)
                .Include(x => x.PatientClass)
                .Include(x => x.PaymentMethod)
                .Include(x => x.RegisteredByUser)
                .Include(x => x.CancelledByUser)
                .Include(x => x.NoShowByUser)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<TrxPatientEncounter> ApplyDateFilter(IQueryable<TrxPatientEncounter> query, DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            if (startDate.HasValue)
            {
                query = query.Where(x => x.EncounterDate >= DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc));
            }

            if (endDate.HasValue)
            {
                query = query.Where(x => x.EncounterDate < DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc));
            }

            if (!startDate.HasValue && !endDate.HasValue && !string.IsNullOrWhiteSpace(customPeriod))
            {
                var today = DateTime.UtcNow.Date;

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
            bool? isInsurancePatient,
            bool? isCompanyPatient,
            bool? isEligibilityRequired,
            bool? isEligibilityCompleted,
            bool? isReferral,
            bool? isActive,
            string? search)
        {
            if (encounterStatus.HasValue) query = query.Where(x => x.EncounterStatus == encounterStatus.Value);
            if (encounterType.HasValue) query = query.Where(x => x.EncounterType == encounterType.Value);
            if (paymentType.HasValue) query = query.Where(x => x.PaymentType == paymentType.Value);
            if (isInsurancePatient.HasValue) query = query.Where(x => x.IsInsurancePatient == isInsurancePatient.Value);
            if (isCompanyPatient.HasValue) query = query.Where(x => x.IsCompanyPatient == isCompanyPatient.Value);
            if (isEligibilityRequired.HasValue) query = query.Where(x => x.IsEligibilityRequired == isEligibilityRequired.Value);
            if (isEligibilityCompleted.HasValue) query = query.Where(x => x.IsEligibilityCompleted == isEligibilityCompleted.Value);
            if (isReferral.HasValue) query = query.Where(x => x.IsReferral == isReferral.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

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
                    (x.Doctor != null && x.Doctor.FullName.ToLower().Contains(keyword)) ||
                    (x.PaymentMethod != null && x.PaymentMethod.PaymentMethodName.ToLower().Contains(keyword)) ||
                    (x.PrimaryGuarantorNameSnapshot != null && x.PrimaryGuarantorNameSnapshot.ToLower().Contains(keyword)) ||
                    (x.ReferralNumber != null && x.ReferralNumber.ToLower().Contains(keyword)) ||
                    (x.EligibilityReferenceNumber != null && x.EligibilityReferenceNumber.ToLower().Contains(keyword)));
            }

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(PatientEncounterCreateRequest request)
        {
            if (request.PatientId == Guid.Empty) return (false, "Pasien wajib dipilih.");
            if (request.ServiceUnitId == Guid.Empty) return (false, "Service unit wajib dipilih.");

            if (!Enum.IsDefined(typeof(EncounterType), request.EncounterType)) return (false, "Tipe encounter tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            if (!Enum.IsDefined(typeof(VisitType), request.VisitType)) return (false, "Tipe visit tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            if (!Enum.IsDefined(typeof(EncounterRegistrationSource), request.RegistrationSource)) return (false, "Sumber registrasi tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            if (!Enum.IsDefined(typeof(EncounterPaymentType), request.PaymentType)) return (false, "Tipe pembayaran tidak valid. Gunakan nilai dari endpoint filters/metadata.");

            var patientExists = await _dbContext.Set<MstPatient>().AsNoTracking().AnyAsync(x => x.Id == request.PatientId && x.IsActive && !x.IsDelete);
            if (!patientExists) return (false, "Pasien tidak valid atau tidak aktif.");

            var serviceUnitExists = await _dbContext.Set<MstServiceUnit>().AsNoTracking().AnyAsync(x => x.Id == request.ServiceUnitId && x.IsActive && !x.IsDelete && x.IsAvailableForRegistration);
            if (!serviceUnitExists) return (false, "Service unit tidak valid, tidak aktif, atau tidak tersedia untuk registrasi.");

            if (request.ClinicId.HasValue && request.ClinicId.Value != Guid.Empty)
            {
                var clinicExists = await _dbContext.Set<MstClinic>().AsNoTracking().AnyAsync(x => x.Id == request.ClinicId.Value && x.ServiceUnitId == request.ServiceUnitId && x.IsActive && !x.IsDelete && x.IsAvailableForRegistration);
                if (!clinicExists) return (false, "Clinic tidak valid, tidak aktif, atau tidak tersedia untuk registrasi.");
            }

            if (request.DoctorId.HasValue && request.DoctorId.Value != Guid.Empty)
            {
                var doctorExists = await _dbContext.Set<MstDoctor>().AsNoTracking().AnyAsync(x => x.Id == request.DoctorId.Value && x.IsActive && !x.IsDelete);
                if (!doctorExists) return (false, "Dokter tidak valid atau tidak aktif.");
            }

            if (request.DoctorScheduleId.HasValue && request.DoctorScheduleId.Value != Guid.Empty)
            {
                var scheduleExists = await _dbContext.Set<MstDoctorSchedule>().AsNoTracking().AnyAsync(x =>
                    x.Id == request.DoctorScheduleId.Value &&
                    x.IsActive &&
                    !x.IsDelete &&
                    (!request.DoctorId.HasValue || x.DoctorId == request.DoctorId.Value) &&
                    x.ServiceUnitId == request.ServiceUnitId &&
                    (!request.ClinicId.HasValue || x.ClinicId == request.ClinicId.Value));

                if (!scheduleExists) return (false, "Jadwal dokter tidak valid, tidak aktif, atau tidak sesuai dokter/service unit/clinic.");
            }

            if (request.DoctorServiceRuleId.HasValue && request.DoctorServiceRuleId.Value != Guid.Empty)
            {
                var ruleExists = await _dbContext.Set<MstDoctorServiceRule>().AsNoTracking().AnyAsync(x =>
                    x.Id == request.DoctorServiceRuleId.Value &&
                    x.IsActive &&
                    !x.IsDelete &&
                    (!request.DoctorId.HasValue || x.DoctorId == request.DoctorId.Value) &&
                    x.ServiceUnitId == request.ServiceUnitId);

                if (!ruleExists) return (false, "Doctor service rule tidak valid, tidak aktif, atau tidak sesuai dokter/service unit.");
            }

            if (request.PatientClassId.HasValue && request.PatientClassId.Value != Guid.Empty)
            {
                var patientClassExists = await _dbContext.Set<MstPatientClass>().AsNoTracking().AnyAsync(x => x.Id == request.PatientClassId.Value && x.IsActive && !x.IsDelete);
                if (!patientClassExists) return (false, "Patient class tidak valid atau tidak aktif.");
            }

            if (request.PaymentMethodId.HasValue && request.PaymentMethodId.Value != Guid.Empty)
            {
                var paymentMethodExists = await _dbContext.Set<MstPaymentMethod>().AsNoTracking().AnyAsync(x => x.Id == request.PaymentMethodId.Value && x.IsActive && !x.IsDelete && x.IsAvailableForRegistration);
                if (!paymentMethodExists) return (false, "Metode pembayaran tidak valid atau tidak tersedia untuk registrasi.");
            }

            if (request.KioskScanSessionId.HasValue && request.KioskScanSessionId.Value != Guid.Empty)
            {
                var scanSession = await _dbContext.Set<TrxKioskScanSession>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.KioskScanSessionId.Value && !x.IsDelete && !x.IsUsedForRegistration);
                if (scanSession == null) return (false, "Kiosk scan session tidak valid atau sudah digunakan.");
                if (scanSession.PatientId.HasValue && scanSession.PatientId.Value != request.PatientId) return (false, "Kiosk scan session tidak sesuai dengan pasien yang dipilih.");
            }

            if (request.IsReferralRequired && !request.IsReferral) return (false, "Referral wajib ditandai aktif jika rujukan diperlukan.");
            if (request.IsReferral && request.IsReferralVerified && string.IsNullOrWhiteSpace(request.ReferralNumber)) return (false, "Nomor referral wajib diisi jika referral sudah diverifikasi.");

            var guarantors = BuildDefaultGuarantorsIfEmpty(request);
            if (!guarantors.Any()) return (false, "Minimal satu penjamin kunjungan wajib diisi.");
            if (guarantors.Count(x => x.IsPrimary) != 1) return (false, "Harus ada tepat satu penjamin utama.");
            if (guarantors.GroupBy(x => x.CoveragePriority).Any(x => x.Count() > 1)) return (false, "Coverage priority penjamin tidak boleh duplikat.");

            foreach (var guarantor in guarantors)
            {
                var guarantorValidation = await ValidateGuarantorRequestAsync(request.PatientId, guarantor);
                if (!guarantorValidation.IsValid) return guarantorValidation;
            }

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateGuarantorRequestAsync(Guid patientId, PatientEncounterGuarantorRequest request)
        {
            if (!Enum.IsDefined(typeof(PatientEncounterGuarantorType), request.GuarantorType)) return (false, "Tipe penjamin tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            if (!Enum.IsDefined(typeof(PatientEncounterGuarantorRole), request.GuarantorRole)) return (false, "Role penjamin tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            if (!Enum.IsDefined(typeof(PatientEncounterGuarantorStatus), request.GuarantorStatus)) return (false, "Status penjamin tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            if (request.CoveragePriority <= 0) return (false, "Coverage priority harus lebih besar dari 0.");
            if (request.CoveragePercent.HasValue && (request.CoveragePercent.Value < 0 || request.CoveragePercent.Value > 100)) return (false, "Coverage percent harus di antara 0 sampai 100.");
            if (request.CoPaymentPercent.HasValue && (request.CoPaymentPercent.Value < 0 || request.CoPaymentPercent.Value > 100)) return (false, "Co-payment percent harus di antara 0 sampai 100.");

            var amounts = new[] { request.AnnualLimitAmount, request.RemainingLimitAmount, request.UsedLimitAmount, request.RoomLimitPerDayAmount, request.DeductibleAmount, request.CoPaymentAmount, request.EstimatedCoveredAmount, request.EstimatedPatientPayAmount };
            if (amounts.Any(x => x.HasValue && x.Value < 0)) return (false, "Nilai amount penjamin tidak boleh kurang dari 0.");

            if (request.PaymentMethodId.HasValue && request.PaymentMethodId.Value != Guid.Empty)
            {
                var exists = await _dbContext.Set<MstPaymentMethod>().AsNoTracking().AnyAsync(x => x.Id == request.PaymentMethodId.Value && x.IsActive && !x.IsDelete);
                if (!exists) return (false, "Metode pembayaran penjamin tidak valid.");
            }

            if (request.PatientInsuranceId.HasValue && request.PatientInsuranceId.Value != Guid.Empty)
            {
                var exists = await _dbContext.Set<MstPatientInsurance>().AsNoTracking().AnyAsync(x => x.Id == request.PatientInsuranceId.Value && x.PatientId == patientId && x.IsActive && !x.IsDelete);
                if (!exists) return (false, "Asuransi pasien tidak valid atau tidak aktif.");
            }

            if (request.InsuranceProviderId.HasValue && request.InsuranceProviderId.Value != Guid.Empty)
            {
                var exists = await _dbContext.Set<MstInsuranceProvider>().AsNoTracking().AnyAsync(x => x.Id == request.InsuranceProviderId.Value && x.IsActive && !x.IsDelete);
                if (!exists) return (false, "Provider asuransi tidak valid atau tidak aktif.");
            }

            if (request.PatientCompanyGuarantorId.HasValue && request.PatientCompanyGuarantorId.Value != Guid.Empty)
            {
                var exists = await _dbContext.Set<MstPatientCompanyGuarantor>().AsNoTracking().AnyAsync(x => x.Id == request.PatientCompanyGuarantorId.Value && x.PatientId == patientId && x.IsActive && !x.IsDelete);
                if (!exists) return (false, "Company guarantor pasien tidak valid atau tidak aktif.");
            }

            if (request.CompanyGuarantorId.HasValue && request.CompanyGuarantorId.Value != Guid.Empty)
            {
                var exists = await _dbContext.Set<MstCompanyGuarantor>().AsNoTracking().AnyAsync(x => x.Id == request.CompanyGuarantorId.Value && x.IsActive && !x.IsDelete);
                if (!exists) return (false, "Company guarantor tidak valid atau tidak aktif.");
            }

            if (request.PatientMembershipId.HasValue && request.PatientMembershipId.Value != Guid.Empty)
            {
                var exists = await _dbContext.Set<MstPatientMembership>().AsNoTracking().AnyAsync(x => x.Id == request.PatientMembershipId.Value && x.PatientId == patientId && x.IsActive && !x.IsDelete);
                if (!exists) return (false, "Membership pasien tidak valid atau tidak aktif.");
            }

            if (request.GuarantorType == PatientEncounterGuarantorType.Insurance && !request.PatientInsuranceId.HasValue && !request.InsuranceProviderId.HasValue)
            {
                return (false, "Penjamin asuransi wajib memiliki PatientInsuranceId atau InsuranceProviderId.");
            }

            if (request.GuarantorType == PatientEncounterGuarantorType.Company && !request.PatientCompanyGuarantorId.HasValue && !request.CompanyGuarantorId.HasValue)
            {
                return (false, "Penjamin company wajib memiliki PatientCompanyGuarantorId atau CompanyGuarantorId.");
            }

            return (true, null);
        }

        private List<PatientEncounterGuarantorRequest> BuildDefaultGuarantorsIfEmpty(PatientEncounterCreateRequest request)
        {
            if (request.Guarantors.Any()) return request.Guarantors;
            if (request.PaymentType != EncounterPaymentType.Cash) return new List<PatientEncounterGuarantorRequest>();

            return new List<PatientEncounterGuarantorRequest>
            {
                new()
                {
                    GuarantorType = PatientEncounterGuarantorType.PatientCash,
                    GuarantorRole = PatientEncounterGuarantorRole.Primary,
                    GuarantorStatus = PatientEncounterGuarantorStatus.Eligible,
                    CheckMethod = PatientEncounterGuarantorCheckMethod.None,
                    CoveragePriority = 1,
                    IsPrimary = true,
                    PaymentMethodId = request.PaymentMethodId,
                    IsEligibilityRequired = false,
                    IsEligible = true,
                    IsPolicyActive = true,
                    IsAllowExcessPaymentByPatient = true,
                    GuarantorNameSnapshot = "Patient Cash"
                }
            };
        }

        private async Task<TrxPatientEncounterGuarantor> BuildGuarantorEntityAsync(Guid encounterId, Guid patientId, PatientEncounterGuarantorRequest request, DateTime now, Guid actorUserId)
        {
            var entity = new TrxPatientEncounterGuarantor
            {
                Id = Guid.NewGuid(),
                EncounterGuarantorNumber = await GenerateEncounterGuarantorNumberAsync(),
                EncounterId = encounterId,
                PatientId = patientId,
                GuarantorType = request.GuarantorType,
                GuarantorRole = request.GuarantorRole,
                GuarantorStatus = request.GuarantorStatus,
                CheckMethod = request.CheckMethod,
                CoveragePriority = request.CoveragePriority,
                IsPrimary = request.IsPrimary,
                IsActive = true,
                PaymentMethodId = NormalizeNullableGuid(request.PaymentMethodId),
                PatientInsuranceId = NormalizeNullableGuid(request.PatientInsuranceId),
                InsuranceProviderId = NormalizeNullableGuid(request.InsuranceProviderId),
                CompanyGuarantorId = NormalizeNullableGuid(request.CompanyGuarantorId),
                PatientCompanyGuarantorId = NormalizeNullableGuid(request.PatientCompanyGuarantorId),
                PatientMembershipId = NormalizeNullableGuid(request.PatientMembershipId),
                GuarantorNameSnapshot = NormalizeNullableText(request.GuarantorNameSnapshot),
                PolicyNumberSnapshot = NormalizeNullableText(request.PolicyNumberSnapshot),
                CardNumberSnapshot = NormalizeNullableText(request.CardNumberSnapshot),
                MemberNumberSnapshot = NormalizeNullableText(request.MemberNumberSnapshot),
                PlanNameSnapshot = NormalizeNullableText(request.PlanNameSnapshot),
                ClassNameSnapshot = NormalizeNullableText(request.ClassNameSnapshot),
                BenefitPlanCodeSnapshot = NormalizeNullableText(request.BenefitPlanCodeSnapshot),
                EffectiveStartDateSnapshot = request.EffectiveStartDateSnapshot,
                EffectiveEndDateSnapshot = request.EffectiveEndDateSnapshot,
                IsEligibilityRequired = request.IsEligibilityRequired,
                IsEligible = request.IsEligible,
                EligibilityReferenceNumber = NormalizeNullableText(request.EligibilityReferenceNumber),
                EligibilityCheckedAt = request.EligibilityCheckedAt,
                VerificationReferenceNumber = NormalizeNullableText(request.VerificationReferenceNumber),
                VerificationOfficerName = NormalizeNullableText(request.VerificationOfficerName),
                VerificationNote = NormalizeNullableText(request.VerificationNote),
                IsNeedApproval = request.IsNeedApproval,
                IsNeedGuaranteeLetter = request.IsNeedGuaranteeLetter,
                IsNeedReferralLetter = request.IsNeedReferralLetter,
                IsAllowExcessPaymentByPatient = request.IsAllowExcessPaymentByPatient,
                CoveragePercent = request.CoveragePercent,
                AnnualLimitAmount = request.AnnualLimitAmount,
                RemainingLimitAmount = request.RemainingLimitAmount,
                UsedLimitAmount = request.UsedLimitAmount,
                RoomLimitPerDayAmount = request.RoomLimitPerDayAmount,
                DeductibleAmount = request.DeductibleAmount,
                CoPaymentPercent = request.CoPaymentPercent,
                CoPaymentAmount = request.CoPaymentAmount,
                EstimatedCoveredAmount = request.EstimatedCoveredAmount,
                EstimatedPatientPayAmount = request.EstimatedPatientPayAmount,
                IsPolicyActive = request.IsPolicyActive,
                IsPremiumPaid = request.IsPremiumPaid,
                IsCardActive = request.IsCardActive,
                IsInWaitingPeriod = request.IsInWaitingPeriod,
                WaitingPeriodUntilDate = request.WaitingPeriodUntilDate,
                HasSpecialExclusion = request.HasSpecialExclusion,
                SpecialExclusionNote = NormalizeNullableText(request.SpecialExclusionNote),
                HasPreviousClaim = request.HasPreviousClaim,
                PreviousClaimNote = NormalizeNullableText(request.PreviousClaimNote),
                BenefitSnapshotJson = NormalizeNullableText(request.BenefitSnapshotJson),
                ManualCheckResultJson = NormalizeNullableText(request.ManualCheckResultJson),
                Notes = NormalizeNullableText(request.Notes),
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            await FillGuarantorSnapshotAsync(entity);
            return entity;
        }

        private async Task FillGuarantorSnapshotAsync(TrxPatientEncounterGuarantor entity)
        {
            if (entity.GuarantorType == PatientEncounterGuarantorType.PatientCash)
            {
                entity.GuarantorNameSnapshot ??= "Patient Cash";
                return;
            }

            if (entity.PatientInsuranceId.HasValue)
            {
                var data = await _dbContext.Set<MstPatientInsurance>().Include(x => x.InsuranceProvider).AsNoTracking().FirstOrDefaultAsync(x => x.Id == entity.PatientInsuranceId.Value);
                if (data != null)
                {
                    entity.InsuranceProviderId ??= data.InsuranceProviderId;
                    entity.GuarantorNameSnapshot ??= data.InsuranceProvider?.InsuranceProviderName;
                    entity.PolicyNumberSnapshot ??= data.PolicyNumber;
                    entity.CardNumberSnapshot ??= data.CardNumber;
                    entity.MemberNumberSnapshot ??= data.MemberNumber;
                    entity.PlanNameSnapshot ??= data.PlanName;
                    entity.ClassNameSnapshot ??= data.ClassName;
                    entity.BenefitPlanCodeSnapshot ??= data.BenefitPlanCode;
                    entity.EffectiveStartDateSnapshot ??= data.EffectiveStartDate;
                    entity.EffectiveEndDateSnapshot ??= data.EffectiveEndDate;
                    entity.AnnualLimitAmount ??= data.AnnualLimitAmount;
                    entity.RemainingLimitAmount ??= data.RemainingLimitAmount;
                    entity.CoPaymentPercent ??= data.CoPaymentPercent;
                    entity.CoPaymentAmount ??= data.CoPaymentAmount;
                    entity.IsNeedGuaranteeLetter = entity.IsNeedGuaranteeLetter || data.IsNeedGuaranteeLetter;
                    entity.IsNeedReferralLetter = entity.IsNeedReferralLetter || data.IsNeedReferralLetter;
                    entity.IsAllowExcessPaymentByPatient = data.IsAllowExcessPaymentByPatient;
                }
            }

            if (entity.PatientCompanyGuarantorId.HasValue)
            {
                var data = await _dbContext.Set<MstPatientCompanyGuarantor>().Include(x => x.CompanyGuarantor).AsNoTracking().FirstOrDefaultAsync(x => x.Id == entity.PatientCompanyGuarantorId.Value);
                if (data != null)
                {
                    entity.CompanyGuarantorId ??= data.CompanyGuarantorId;
                    entity.GuarantorNameSnapshot ??= data.CompanyGuarantor?.CompanyGuarantorName;
                    entity.MemberNumberSnapshot ??= data.EmployeeNumber;
                    entity.PlanNameSnapshot ??= data.BenefitPlanName;
                    entity.ClassNameSnapshot ??= data.ClassName;
                    entity.BenefitPlanCodeSnapshot ??= data.BenefitPlanCode;
                    entity.EffectiveStartDateSnapshot ??= data.EffectiveStartDate;
                    entity.EffectiveEndDateSnapshot ??= data.EffectiveEndDate;
                    entity.AnnualLimitAmount ??= data.AnnualLimitAmount;
                    entity.RemainingLimitAmount ??= data.RemainingLimitAmount;
                    entity.CoPaymentPercent ??= data.CoPaymentPercent;
                    entity.CoPaymentAmount ??= data.CoPaymentAmount;
                    entity.IsNeedGuaranteeLetter = entity.IsNeedGuaranteeLetter || data.IsNeedGuaranteeLetter;
                    entity.IsAllowExcessPaymentByPatient = data.IsAllowExcessPaymentByPatient;
                }
            }

            if (entity.PatientMembershipId.HasValue)
            {
                var data = await _dbContext.Set<MstPatientMembership>().Include(x => x.MembershipTier).AsNoTracking().FirstOrDefaultAsync(x => x.Id == entity.PatientMembershipId.Value);
                if (data != null)
                {
                    entity.GuarantorNameSnapshot ??= data.MembershipTier?.TierName;
                    entity.MemberNumberSnapshot ??= data.MemberNumber;
                    entity.PlanNameSnapshot ??= data.MembershipTier?.TierName;
                    entity.EffectiveStartDateSnapshot ??= data.JoinDate;
                    entity.EffectiveEndDateSnapshot ??= data.ExpiredDate;
                }
            }

            if (string.IsNullOrWhiteSpace(entity.GuarantorNameSnapshot) && entity.InsuranceProviderId.HasValue)
            {
                entity.GuarantorNameSnapshot = await _dbContext.Set<MstInsuranceProvider>().Where(x => x.Id == entity.InsuranceProviderId.Value).Select(x => x.InsuranceProviderName).FirstOrDefaultAsync();
            }

            if (string.IsNullOrWhiteSpace(entity.GuarantorNameSnapshot) && entity.CompanyGuarantorId.HasValue)
            {
                entity.GuarantorNameSnapshot = await _dbContext.Set<MstCompanyGuarantor>().Where(x => x.Id == entity.CompanyGuarantorId.Value).Select(x => x.CompanyGuarantorName).FirstOrDefaultAsync();
            }

            if (string.IsNullOrWhiteSpace(entity.GuarantorNameSnapshot) && entity.PaymentMethodId.HasValue)
            {
                entity.GuarantorNameSnapshot = await _dbContext.Set<MstPaymentMethod>().Where(x => x.Id == entity.PaymentMethodId.Value).Select(x => x.PaymentMethodName).FirstOrDefaultAsync();
            }
        }

        private static void ApplyEncounterPaymentSummary(TrxPatientEncounter encounter, IEnumerable<TrxPatientEncounterGuarantor> guarantors, DateTime now, Guid actorUserId)
        {
            var activeGuarantors = guarantors.Where(x => x.IsActive && !x.IsDelete && x.GuarantorStatus != PatientEncounterGuarantorStatus.Cancelled).ToList();
            var primary = activeGuarantors.FirstOrDefault(x => x.IsPrimary) ?? activeGuarantors.OrderBy(x => x.CoveragePriority).FirstOrDefault();

            encounter.IsInsurancePatient = activeGuarantors.Any(x => x.GuarantorType == PatientEncounterGuarantorType.Insurance || x.GuarantorType == PatientEncounterGuarantorType.BPJS);
            encounter.IsCompanyPatient = activeGuarantors.Any(x => x.GuarantorType == PatientEncounterGuarantorType.Company);
            encounter.IsMembershipPatient = activeGuarantors.Any(x => x.GuarantorType == PatientEncounterGuarantorType.Membership);
            encounter.IsMixedPayment = activeGuarantors.Select(x => x.GuarantorType).Distinct().Count() > 1;
            encounter.PrimaryGuarantorNameSnapshot = primary?.GuarantorNameSnapshot;
            encounter.PrimaryGuarantorTypeSnapshot = primary?.GuarantorType.ToString();
            encounter.IsEligibilityRequired = activeGuarantors.Any(x => x.IsEligibilityRequired) || encounter.IsEligibilityRequired;
            encounter.IsEligibilityCompleted = activeGuarantors.Any(x => x.IsEligibilityRequired) && activeGuarantors.Where(x => x.IsEligibilityRequired).All(x => x.IsEligible && x.IsVerified);
            encounter.EligibilityReferenceNumber = primary?.EligibilityReferenceNumber ?? encounter.EligibilityReferenceNumber;
            encounter.EligibilityCheckedAt = primary?.EligibilityCheckedAt ?? encounter.EligibilityCheckedAt;
            encounter.UpdateDateTime = now;
            encounter.UpdateBy = actorUserId;
        }

        private async Task CancelQueuesByEncounterAsync(Guid encounterId, DateTime now, Guid actorUserId, string reason)
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
        }

        private async Task<string> GenerateEncounterNumberAsync()
        {
            return await GenerateRunningCodeAsync<TrxPatientEncounter>(selector: x => x.EncounterNumber, prefix: EncounterCodePrefix);
        }

        private async Task<string> GenerateEncounterGuarantorNumberAsync()
        {
            return await GenerateRunningCodeAsync<TrxPatientEncounterGuarantor>(selector: x => x.EncounterGuarantorNumber, prefix: EncounterGuarantorCodePrefix);
        }

        private async Task<string> GenerateRunningCodeAsync<TEntity>(Expression<Func<TEntity, string>> selector, string prefix) where TEntity : class
        {
            var existingCodes = await _dbContext.Set<TEntity>().IgnoreQueryFilters().AsNoTracking().Select(selector).Where(x => x.StartsWith(prefix)).ToListAsync();
            var usedNumbers = existingCodes.Select(x => x.Replace(prefix, string.Empty)).Where(x => int.TryParse(x, out _)).Select(int.Parse).Where(x => x > 0).ToHashSet();
            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber)) nextNumber++;

            return prefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
        }

        private async Task<int> GenerateQueueNumberAsync(DateTime now, Guid serviceUnitId, Guid? clinicId, Guid? doctorId)
        {
            var normalizedClinicId = NormalizeNullableGuid(clinicId);
            var normalizedDoctorId = NormalizeNullableGuid(doctorId);

            return await _dbContext.Set<TrxQueue>().CountAsync(x => x.QueueDate == now.Date && x.ServiceUnitId == serviceUnitId && x.ClinicId == normalizedClinicId && x.DoctorId == normalizedDoctorId && !x.IsDelete) + 1;
        }

        private static string GenerateQueueCode(DateTime now, MstClinic? clinic, int queueNumber)
        {
            var prefix = !string.IsNullOrWhiteSpace(clinic?.ShortName) ? clinic.ShortName.Trim().ToUpperInvariant() : "Q";
            return $"{prefix}-{now:yyyyMMdd}-{queueNumber:D3}";
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> actorIds)
        {
            var ids = actorIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (!ids.Any()) return new Dictionary<Guid, string?>();

            return await _dbContext.Users.AsNoTracking().Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode }).ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static PatientEncounterResponse MapResponse(TrxPatientEncounter entity, IReadOnlyDictionary<Guid, string?> actorNames)
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
                DoctorId = entity.DoctorId,
                DoctorName = entity.Doctor?.FullName,
                DoctorScheduleId = entity.DoctorScheduleId,
                DoctorServiceRuleId = entity.DoctorServiceRuleId,
                PatientClassId = entity.PatientClassId,
                PatientClassName = entity.PatientClass?.PatientClassName,
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
                IsInsurancePatient = entity.IsInsurancePatient,
                IsCompanyPatient = entity.IsCompanyPatient,
                IsMembershipPatient = entity.IsMembershipPatient,
                IsMixedPayment = entity.IsMixedPayment,
                PrimaryGuarantorNameSnapshot = entity.PrimaryGuarantorNameSnapshot,
                PrimaryGuarantorTypeSnapshot = entity.PrimaryGuarantorTypeSnapshot,
                IsEligibilityRequired = entity.IsEligibilityRequired,
                IsEligibilityCompleted = entity.IsEligibilityCompleted,
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

        private static PatientEncounterDetailResponse MapDetailResponse(TrxPatientEncounter entity, IReadOnlyDictionary<Guid, string?> actorNames)
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
                DoctorId = entity.DoctorId,
                DoctorName = entity.Doctor?.FullName,
                DoctorScheduleId = entity.DoctorScheduleId,
                DoctorServiceRuleId = entity.DoctorServiceRuleId,
                PatientClassId = entity.PatientClassId,
                PatientClassName = entity.PatientClass?.PatientClassName,
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
                IsInsurancePatient = entity.IsInsurancePatient,
                IsCompanyPatient = entity.IsCompanyPatient,
                IsMembershipPatient = entity.IsMembershipPatient,
                IsMixedPayment = entity.IsMixedPayment,
                PrimaryGuarantorNameSnapshot = entity.PrimaryGuarantorNameSnapshot,
                PrimaryGuarantorTypeSnapshot = entity.PrimaryGuarantorTypeSnapshot,
                EligibilityReferenceNumber = entity.EligibilityReferenceNumber,
                EligibilityCheckedAt = entity.EligibilityCheckedAt,
                IsEligibilityRequired = entity.IsEligibilityRequired,
                IsEligibilityCompleted = entity.IsEligibilityCompleted,
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
                CancelledByUserName = entity.CancelledByUserId.HasValue ? GetActorName(actorNames, entity.CancelledByUserId.Value) : null,
                CancelReason = entity.CancelReason,
                NoShowAt = entity.NoShowAt,
                NoShowByUserId = entity.NoShowByUserId,
                NoShowByUserName = entity.NoShowByUserId.HasValue ? GetActorName(actorNames, entity.NoShowByUserId.Value) : null,
                NoShowReason = entity.NoShowReason,
                Notes = entity.Notes,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy),
                Guarantors = entity.EncounterGuarantors.Where(g => !g.IsDelete).OrderBy(g => g.CoveragePriority).Select(MapGuarantorResponse).ToList()
            };

            return response;
        }

        private static PatientEncounterOptionResponse MapOptionResponse(TrxPatientEncounter entity)
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
                DoctorId = entity.DoctorId,
                DoctorName = entity.Doctor?.FullName,
                EncounterStatus = entity.EncounterStatus,
                EncounterStatusName = BuildEnumLabel(entity.EncounterStatus),
                EncounterDate = entity.EncounterDate,
                RegisteredAt = entity.RegisteredAt
            };
        }

        private static PatientEncounterGuarantorResponse MapGuarantorResponse(TrxPatientEncounterGuarantor entity)
        {
            return new PatientEncounterGuarantorResponse
            {
                Id = entity.Id,
                EncounterGuarantorNumber = entity.EncounterGuarantorNumber,
                EncounterId = entity.EncounterId,
                EncounterNumber = entity.Encounter?.EncounterNumber ?? string.Empty,
                PatientId = entity.PatientId,
                PatientName = entity.Patient?.FullName ?? string.Empty,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber ?? string.Empty,
                GuarantorType = entity.GuarantorType,
                GuarantorTypeName = BuildEnumLabel(entity.GuarantorType),
                GuarantorRole = entity.GuarantorRole,
                GuarantorRoleName = BuildEnumLabel(entity.GuarantorRole),
                GuarantorStatus = entity.GuarantorStatus,
                GuarantorStatusName = BuildEnumLabel(entity.GuarantorStatus),
                CheckMethod = entity.CheckMethod,
                CheckMethodName = BuildEnumLabel(entity.CheckMethod),
                CoveragePriority = entity.CoveragePriority,
                IsPrimary = entity.IsPrimary,
                PaymentMethodId = entity.PaymentMethodId,
                PaymentMethodName = entity.PaymentMethod?.PaymentMethodName,
                PatientInsuranceId = entity.PatientInsuranceId,
                InsuranceProviderId = entity.InsuranceProviderId,
                InsuranceProviderName = entity.InsuranceProvider?.InsuranceProviderName,
                CompanyGuarantorId = entity.CompanyGuarantorId,
                CompanyGuarantorName = entity.CompanyGuarantor?.CompanyGuarantorName,
                PatientCompanyGuarantorId = entity.PatientCompanyGuarantorId,
                PatientMembershipId = entity.PatientMembershipId,
                GuarantorNameSnapshot = entity.GuarantorNameSnapshot,
                PolicyNumberSnapshot = entity.PolicyNumberSnapshot,
                CardNumberSnapshot = entity.CardNumberSnapshot,
                MemberNumberSnapshot = entity.MemberNumberSnapshot,
                PlanNameSnapshot = entity.PlanNameSnapshot,
                ClassNameSnapshot = entity.ClassNameSnapshot,
                BenefitPlanCodeSnapshot = entity.BenefitPlanCodeSnapshot,
                IsEligibilityRequired = entity.IsEligibilityRequired,
                IsEligible = entity.IsEligible,
                IsVerified = entity.IsVerified,
                VerifiedAt = entity.VerifiedAt,
                EligibilityReferenceNumber = entity.EligibilityReferenceNumber,
                EligibilityCheckedAt = entity.EligibilityCheckedAt,
                IsNeedApproval = entity.IsNeedApproval,
                IsNeedGuaranteeLetter = entity.IsNeedGuaranteeLetter,
                IsNeedReferralLetter = entity.IsNeedReferralLetter,
                IsAllowExcessPaymentByPatient = entity.IsAllowExcessPaymentByPatient,
                CoveragePercent = entity.CoveragePercent,
                AnnualLimitAmount = entity.AnnualLimitAmount,
                RemainingLimitAmount = entity.RemainingLimitAmount,
                EstimatedCoveredAmount = entity.EstimatedCoveredAmount,
                EstimatedPatientPayAmount = entity.EstimatedPatientPayAmount,
                IsPolicyActive = entity.IsPolicyActive,
                IsPremiumPaid = entity.IsPremiumPaid,
                IsCardActive = entity.IsCardActive,
                IsInWaitingPeriod = entity.IsInWaitingPeriod,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy
            };
        }

        private static PatientEncounterGuarantorCreateResponse MapGuarantorCreateResponse(TrxPatientEncounterGuarantor entity)
        {
            return new PatientEncounterGuarantorCreateResponse
            {
                Id = entity.Id,
                EncounterGuarantorNumber = entity.EncounterGuarantorNumber,
                EncounterId = entity.EncounterId,
                GuarantorType = entity.GuarantorType,
                GuarantorTypeName = BuildEnumLabel(entity.GuarantorType),
                GuarantorStatus = entity.GuarantorStatus,
                GuarantorStatusName = BuildEnumLabel(entity.GuarantorStatus),
                CoveragePriority = entity.CoveragePriority,
                IsPrimary = entity.IsPrimary,
                GuarantorNameSnapshot = entity.GuarantorNameSnapshot
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

        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(userIdValue, out var userId) ? userId : Guid.Empty;
        }
    }
}