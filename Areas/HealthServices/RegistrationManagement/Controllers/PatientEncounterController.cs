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
        Description = "Transaksi kunjungan pasien rawat jalan dan penjamin kunjungan",
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

        // =========================================================
        // ENCOUNTER
        // =========================================================

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientEncounterCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Patient Encounter", Description = "Membuat transaksi kunjungan pasien beserta penjamin", AccessType = AccessTypes.Create, SortOrder = 1)]
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

            var normalizedGuarantorRequests = BuildDefaultGuarantorsIfEmpty(request);
            var paymentSummary = BuildPaymentSummary(request.PaymentType, normalizedGuarantorRequests);

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

                IsInsurancePatient = paymentSummary.IsInsurancePatient,
                IsCompanyPatient = paymentSummary.IsCompanyPatient,
                IsMembershipPatient = paymentSummary.IsMembershipPatient,
                IsMixedPayment = paymentSummary.IsMixedPayment,
                PrimaryGuarantorNameSnapshot = NormalizeNullableText(paymentSummary.PrimaryGuarantorNameSnapshot),
                PrimaryGuarantorTypeSnapshot = NormalizeNullableText(paymentSummary.PrimaryGuarantorTypeSnapshot),
                EligibilityReferenceNumber = NormalizeNullableText(request.EligibilityReferenceNumber),
                EligibilityCheckedAt = request.EligibilityCheckedAt,
                IsEligibilityRequired = request.IsEligibilityRequired || paymentSummary.IsEligibilityRequired,
                IsEligibilityCompleted = request.IsEligibilityCompleted || paymentSummary.IsEligibilityCompleted,

                IsReferral = request.IsReferral,
                ReferralNumber = NormalizeNullableText(request.ReferralNumber),
                IsReferralRequired = request.IsReferralRequired,
                IsReferralVerified = request.IsReferralVerified,

                IsNewPatient = request.IsNewPatient,
                IsFromKiosk = request.IsFromKiosk,
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

            var guarantorEntities = new List<TrxPatientEncounterGuarantor>();

            foreach (var guarantorRequest in normalizedGuarantorRequests.OrderBy(x => x.CoveragePriority))
            {
                var guarantor = await BuildGuarantorEntityAsync(
                    encounter.Id,
                    request.PatientId,
                    guarantorRequest,
                    now,
                    actorUserId
                );

                guarantorEntities.Add(guarantor);
                _dbContext.Set<TrxPatientEncounterGuarantor>().Add(guarantor);
            }

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
                IsQueueRequired = isQueueRequired,
                GuarantorCount = guarantorEntities.Count,
                Guarantors = guarantorEntities.Select(ToGuarantorCreateResponse).ToList()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientEncounter.CreateEncounter",
                "Membuat transaksi kunjungan pasien beserta penjamin.",
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
            [FromQuery] EncounterPaymentType? paymentType,
            [FromQuery] bool? isInsurancePatient,
            [FromQuery] bool? isCompanyPatient,
            [FromQuery] bool? isEligibilityRequired,
            [FromQuery] bool? isEligibilityCompleted,
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

            var query = BuildEncounterBaseQuery().AsNoTracking();

            query = ApplyEncounterFilters(
                query,
                search,
                patientId,
                serviceUnitId,
                clinicId,
                doctorId,
                encounterStatus,
                encounterType,
                paymentType,
                isInsurancePatient,
                isCompanyPatient,
                isEligibilityRequired,
                isEligibilityCompleted,
                startDate,
                endDate
            );

            var totalData = await query.CountAsync();

            var entities = await ApplyEncounterSorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(ToEncounterResponse).ToList();

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
            var entity = await BuildEncounterBaseQuery()
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
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kunjungan pasien tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientEncounterDetailResponse>.Ok(
                ToEncounterDetailResponse(entity),
                "Detail kunjungan pasien berhasil diambil."
            ));
        }

        // =========================================================
        // GUARANTOR CHILD ENDPOINTS
        // =========================================================

        [HttpGet("{encounterId:guid}/guarantors")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientEncounterGuarantorResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Encounter Guarantor", Description = "Melihat penjamin kunjungan pasien", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("PatientEncounter", "Read")]
        public async Task<IActionResult> GetGuarantors(Guid encounterId)
        {
            var encounterExists = await _dbContext.Set<TrxPatientEncounter>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == encounterId && !x.IsDelete);

            if (!encounterExists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kunjungan pasien tidak ditemukan."
                ));
            }

            var entities = await BuildGuarantorBaseQuery()
                .AsNoTracking()
                .Where(x => x.EncounterId == encounterId && !x.IsDelete)
                .OrderBy(x => x.CoveragePriority)
                .ThenByDescending(x => x.IsPrimary)
                .ToListAsync();

            var data = entities.Select(ToGuarantorResponse).ToList();

            return Ok(ApiResponse<List<PatientEncounterGuarantorResponse>>.Ok(
                data,
                "Data penjamin kunjungan pasien berhasil diambil."
            ));
        }

        [HttpGet("{encounterId:guid}/guarantors/{guarantorId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientEncounterGuarantorDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Encounter Guarantor", Description = "Melihat detail penjamin kunjungan pasien", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("PatientEncounter", "Read")]
        public async Task<IActionResult> GetGuarantorById(Guid encounterId, Guid guarantorId)
        {
            var entity = await BuildGuarantorBaseQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == guarantorId && x.EncounterId == encounterId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Penjamin kunjungan pasien tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientEncounterGuarantorDetailResponse>.Ok(
                ToGuarantorDetailResponse(entity),
                "Detail penjamin kunjungan pasien berhasil diambil."
            ));
        }

        [HttpPost("{encounterId:guid}/guarantors")]
        [ProducesResponseType(typeof(ApiResponse<PatientEncounterGuarantorCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Patient Encounter Guarantor", Description = "Menambah penjamin kunjungan pasien", AccessType = AccessTypes.Create, SortOrder = 4)]
        [AccessPermission("PatientEncounter", "Create")]
        public async Task<IActionResult> CreateGuarantor(Guid encounterId, [FromBody] PatientEncounterGuarantorRequest request)
        {
            var encounter = await _dbContext.Set<TrxPatientEncounter>()
                .FirstOrDefaultAsync(x => x.Id == encounterId && !x.IsDelete);

            if (encounter == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kunjungan pasien tidak ditemukan."
                ));
            }

            var validation = await ValidateGuarantorRequestAsync(encounter.PatientId, encounterId, request, null);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data penjamin kunjungan pasien tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var entity = await BuildGuarantorEntityAsync(encounterId, encounter.PatientId, request, now, actorUserId);

            _dbContext.Set<TrxPatientEncounterGuarantor>().Add(entity);

            await RebuildEncounterPaymentSummaryAsync(encounter, entity, now, actorUserId);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(ApiResponse<PatientEncounterGuarantorCreateResponse>.Ok(
                ToGuarantorCreateResponse(entity),
                "Penjamin kunjungan pasien berhasil dibuat."
            ));
        }

        [HttpPut("{encounterId:guid}/guarantors/{guarantorId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientEncounterGuarantorUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Encounter Guarantor", Description = "Mengubah penjamin kunjungan pasien", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("PatientEncounter", "Update")]
        public async Task<IActionResult> UpdateGuarantor(Guid encounterId, Guid guarantorId, [FromBody] PatientEncounterGuarantorRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientEncounterGuarantor>()
                .Include(x => x.Encounter)
                .FirstOrDefaultAsync(x => x.Id == guarantorId && x.EncounterId == encounterId && !x.IsDelete);

            if (entity == null || entity.Encounter == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Penjamin kunjungan pasien tidak ditemukan."
                ));
            }

            if (entity.GuarantorStatus == PatientEncounterGuarantorStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Penjamin yang sudah cancelled tidak dapat diubah."
                ));
            }

            var validation = await ValidateGuarantorRequestAsync(entity.PatientId, encounterId, request, guarantorId);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data penjamin kunjungan pasien tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            MapGuarantorRequestToEntity(entity, request, now, actorUserId);

            await RebuildEncounterPaymentSummaryAsync(entity.Encounter, null, now, actorUserId);

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<PatientEncounterGuarantorUpdateResponse>.Ok(
                ToGuarantorUpdateResponse(entity),
                "Penjamin kunjungan pasien berhasil diubah."
            ));
        }

        [HttpPatch("{encounterId:guid}/guarantors/{guarantorId:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Verify Patient Encounter Guarantor", Description = "Verifikasi penjamin kunjungan pasien", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("PatientEncounter", "Update")]
        public async Task<IActionResult> VerifyGuarantor(Guid encounterId, Guid guarantorId, [FromBody] VerifyPatientEncounterGuarantorRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientEncounterGuarantor>()
                .Include(x => x.Encounter)
                .FirstOrDefaultAsync(x => x.Id == guarantorId && x.EncounterId == encounterId && !x.IsDelete);

            if (entity == null || entity.Encounter == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Penjamin kunjungan pasien tidak ditemukan."
                ));
            }

            if (entity.GuarantorStatus == PatientEncounterGuarantorStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Penjamin yang sudah cancelled tidak dapat diverifikasi."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.GuarantorStatus = request.GuarantorStatus;
            entity.CheckMethod = request.CheckMethod;
            entity.IsEligible = request.IsEligible;
            entity.IsVerified = true;
            entity.VerifiedAt = now;
            entity.VerifiedByUserId = actorUserId;
            entity.IsPolicyActive = request.IsPolicyActive;
            entity.IsPremiumPaid = request.IsPremiumPaid;
            entity.IsCardActive = request.IsCardActive;
            entity.EligibilityReferenceNumber = NormalizeNullableText(request.EligibilityReferenceNumber);
            entity.EligibilityCheckedAt = now;
            entity.VerificationReferenceNumber = NormalizeNullableText(request.VerificationReferenceNumber);
            entity.VerificationOfficerName = NormalizeNullableText(request.VerificationOfficerName);
            entity.VerificationNote = NormalizeNullableText(request.VerificationNote);
            entity.RemainingLimitAmount = request.RemainingLimitAmount;
            entity.UsedLimitAmount = request.UsedLimitAmount;
            entity.IsInWaitingPeriod = request.IsInWaitingPeriod;
            entity.WaitingPeriodUntilDate = request.WaitingPeriodUntilDate;
            entity.HasSpecialExclusion = request.HasSpecialExclusion;
            entity.SpecialExclusionNote = NormalizeNullableText(request.SpecialExclusionNote);
            entity.IsNeedApproval = request.IsNeedApproval;
            entity.IsNeedGuaranteeLetter = request.IsNeedGuaranteeLetter;
            entity.IsNeedReferralLetter = request.IsNeedReferralLetter;
            entity.ManualCheckResultJson = NormalizeNullableText(request.ManualCheckResultJson);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (entity.IsPrimary)
            {
                entity.Encounter.EligibilityReferenceNumber = entity.EligibilityReferenceNumber;
                entity.Encounter.EligibilityCheckedAt = entity.EligibilityCheckedAt;
                entity.Encounter.IsEligibilityCompleted = entity.IsEligible && entity.IsVerified;
                entity.Encounter.UpdateDateTime = now;
                entity.Encounter.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Penjamin kunjungan pasien berhasil diverifikasi."));
        }

        [HttpPatch("{encounterId:guid}/guarantors/{guarantorId:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Cancel Patient Encounter Guarantor", Description = "Membatalkan penjamin kunjungan pasien", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("PatientEncounter", "Update")]
        public async Task<IActionResult> CancelGuarantor(Guid encounterId, Guid guarantorId, [FromBody] CancelPatientEncounterGuarantorRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientEncounterGuarantor>()
                .Include(x => x.Encounter)
                .FirstOrDefaultAsync(x => x.Id == guarantorId && x.EncounterId == encounterId && !x.IsDelete);

            if (entity == null || entity.Encounter == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Penjamin kunjungan pasien tidak ditemukan."
                ));
            }

            if (entity.GuarantorStatus == PatientEncounterGuarantorStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Penjamin kunjungan pasien sudah cancelled."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.GuarantorStatus = PatientEncounterGuarantorStatus.Cancelled;
            entity.IsActive = false;
            entity.CancelledAt = now;
            entity.CancelledByUserId = actorUserId;
            entity.CancelReason = request.CancelReason.Trim();
            entity.IsCancel = true;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await RebuildEncounterPaymentSummaryAsync(entity.Encounter, null, now, actorUserId);

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Penjamin kunjungan pasien berhasil dibatalkan."));
        }

        [HttpDelete("{encounterId:guid}/guarantors/{guarantorId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Patient Encounter Guarantor", Description = "Menghapus penjamin kunjungan pasien", AccessType = AccessTypes.Delete, SortOrder = 8)]
        [AccessPermission("PatientEncounter", "Delete")]
        public async Task<IActionResult> DeleteGuarantor(Guid encounterId, Guid guarantorId)
        {
            var entity = await _dbContext.Set<TrxPatientEncounterGuarantor>()
                .Include(x => x.Encounter)
                .FirstOrDefaultAsync(x => x.Id == guarantorId && x.EncounterId == encounterId && !x.IsDelete);

            if (entity == null || entity.Encounter == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Penjamin kunjungan pasien tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await RebuildEncounterPaymentSummaryAsync(entity.Encounter, null, now, actorUserId);

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Penjamin kunjungan pasien berhasil dihapus."));
        }

        // =========================================================
        // QUERY HELPERS
        // =========================================================

        private IQueryable<TrxPatientEncounter> BuildEncounterBaseQuery()
        {
            return _dbContext.Set<TrxPatientEncounter>()
                .Include(x => x.Patient)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.Doctor)
                .Include(x => x.PatientClass)
                .Include(x => x.PaymentMethod)
                .Where(x => !x.IsDelete);
        }

        private IQueryable<TrxPatientEncounterGuarantor> BuildGuarantorBaseQuery()
        {
            return _dbContext.Set<TrxPatientEncounterGuarantor>()
                .Include(x => x.Encounter)
                .Include(x => x.Patient)
                .Include(x => x.PaymentMethod)
                .Include(x => x.InsuranceProvider)
                .Include(x => x.CompanyGuarantor)
                .Include(x => x.VerifiedByUser)
                .Include(x => x.CancelledByUser)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<TrxPatientEncounter> ApplyEncounterFilters(
            IQueryable<TrxPatientEncounter> query,
            string? search,
            Guid? patientId,
            Guid? serviceUnitId,
            Guid? clinicId,
            Guid? doctorId,
            EncounterStatus? encounterStatus,
            EncounterType? encounterType,
            EncounterPaymentType? paymentType,
            bool? isInsurancePatient,
            bool? isCompanyPatient,
            bool? isEligibilityRequired,
            bool? isEligibilityCompleted,
            DateTime? startDate,
            DateTime? endDate)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.EncounterNumber.ToLower().Contains(keyword) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Clinic != null && x.Clinic.ClinicName.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.FullName.ToLower().Contains(keyword)) ||
                    (x.PrimaryGuarantorNameSnapshot != null && x.PrimaryGuarantorNameSnapshot.ToLower().Contains(keyword)));
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

            if (paymentType.HasValue)
                query = query.Where(x => x.PaymentType == paymentType.Value);

            if (isInsurancePatient.HasValue)
                query = query.Where(x => x.IsInsurancePatient == isInsurancePatient.Value);

            if (isCompanyPatient.HasValue)
                query = query.Where(x => x.IsCompanyPatient == isCompanyPatient.Value);

            if (isEligibilityRequired.HasValue)
                query = query.Where(x => x.IsEligibilityRequired == isEligibilityRequired.Value);

            if (isEligibilityCompleted.HasValue)
                query = query.Where(x => x.IsEligibilityCompleted == isEligibilityCompleted.Value);

            if (startDate.HasValue)
                query = query.Where(x => x.EncounterDate >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(x => x.EncounterDate < endDate.Value.Date.AddDays(1));

            return query;
        }

        private static IQueryable<TrxPatientEncounter> ApplyEncounterSorting(
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
                "paymenttype" => isDesc ? query.OrderByDescending(x => x.PaymentType) : query.OrderBy(x => x.PaymentType),
                "primaryguarantor" => isDesc ? query.OrderByDescending(x => x.PrimaryGuarantorNameSnapshot) : query.OrderBy(x => x.PrimaryGuarantorNameSnapshot),
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDesc ? query.OrderByDescending(x => x.RegisteredAt) : query.OrderBy(x => x.RegisteredAt)
            };
        }

        // =========================================================
        // VALIDATION
        // =========================================================

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(PatientEncounterCreateRequest request)
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
                    .AnyAsync(x => x.Id == request.DoctorScheduleId.Value && x.IsActive && !x.IsDelete);

                if (!scheduleExists)
                    return (false, "Jadwal dokter tidak valid atau tidak aktif.");
            }

            if (request.PaymentMethodId.HasValue && request.PaymentMethodId.Value != Guid.Empty)
            {
                var paymentMethodExists = await _dbContext.Set<MstPaymentMethod>()
                    .AnyAsync(x => x.Id == request.PaymentMethodId.Value && x.IsActive && !x.IsDelete && x.IsAvailableForRegistration);

                if (!paymentMethodExists)
                    return (false, "Metode pembayaran tidak valid atau tidak tersedia untuk registrasi.");
            }

            if (request.KioskScanSessionId.HasValue && request.KioskScanSessionId.Value != Guid.Empty)
            {
                var scanSessionExists = await _dbContext.Set<TrxKioskScanSession>()
                    .AnyAsync(x => x.Id == request.KioskScanSessionId.Value && !x.IsDelete && !x.IsUsedForRegistration);

                if (!scanSessionExists)
                    return (false, "Kiosk scan session tidak valid atau sudah digunakan.");
            }

            var guarantors = BuildDefaultGuarantorsIfEmpty(request);

            if (!guarantors.Any())
                return (false, "Minimal satu penjamin kunjungan wajib diisi.");

            if (guarantors.Count(x => x.IsPrimary) != 1)
                return (false, "Harus ada tepat satu penjamin utama.");

            var duplicatePriority = guarantors
                .GroupBy(x => x.CoveragePriority)
                .Any(x => x.Count() > 1);

            if (duplicatePriority)
                return (false, "Coverage priority penjamin tidak boleh duplikat.");

            foreach (var guarantor in guarantors)
            {
                var guarantorValidation = await ValidateGuarantorRequestAsync(request.PatientId, null, guarantor, null);

                if (!guarantorValidation.IsValid)
                    return guarantorValidation;
            }

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateGuarantorRequestAsync(
            Guid patientId,
            Guid? encounterId,
            PatientEncounterGuarantorRequest request,
            Guid? excludeGuarantorId)
        {
            if (request.CoveragePriority <= 0)
                return (false, "Coverage priority harus lebih besar dari 0.");

            if (request.PaymentMethodId.HasValue && request.PaymentMethodId.Value != Guid.Empty)
            {
                var paymentMethodExists = await _dbContext.Set<MstPaymentMethod>()
                    .AnyAsync(x => x.Id == request.PaymentMethodId.Value && x.IsActive && !x.IsDelete);

                if (!paymentMethodExists)
                    return (false, "Metode pembayaran penjamin tidak valid.");
            }

            if (request.PatientInsuranceId.HasValue && request.PatientInsuranceId.Value != Guid.Empty)
            {
                var patientInsuranceExists = await _dbContext.Set<MstPatientInsurance>()
                    .AnyAsync(x => x.Id == request.PatientInsuranceId.Value && x.PatientId == patientId && x.IsActive && !x.IsDelete);

                if (!patientInsuranceExists)
                    return (false, "Asuransi pasien tidak valid atau tidak aktif.");
            }

            if (request.InsuranceProviderId.HasValue && request.InsuranceProviderId.Value != Guid.Empty)
            {
                var insuranceProviderExists = await _dbContext.Set<MstInsuranceProvider>()
                    .AnyAsync(x => x.Id == request.InsuranceProviderId.Value && x.IsActive && !x.IsDelete);

                if (!insuranceProviderExists)
                    return (false, "Provider asuransi tidak valid atau tidak aktif.");
            }

            if (request.PatientCompanyGuarantorId.HasValue && request.PatientCompanyGuarantorId.Value != Guid.Empty)
            {
                var patientCompanyGuarantorExists = await _dbContext.Set<MstPatientCompanyGuarantor>()
                    .AnyAsync(x => x.Id == request.PatientCompanyGuarantorId.Value && x.PatientId == patientId && x.IsActive && !x.IsDelete);

                if (!patientCompanyGuarantorExists)
                    return (false, "Company guarantor pasien tidak valid atau tidak aktif.");
            }

            if (request.CompanyGuarantorId.HasValue && request.CompanyGuarantorId.Value != Guid.Empty)
            {
                var companyGuarantorExists = await _dbContext.Set<MstCompanyGuarantor>()
                    .AnyAsync(x => x.Id == request.CompanyGuarantorId.Value && x.IsActive && !x.IsDelete);

                if (!companyGuarantorExists)
                    return (false, "Company guarantor tidak valid atau tidak aktif.");
            }

            if (request.PatientMembershipId.HasValue && request.PatientMembershipId.Value != Guid.Empty)
            {
                var membershipExists = await _dbContext.Set<MstPatientMembership>()
                    .AnyAsync(x => x.Id == request.PatientMembershipId.Value && x.PatientId == patientId && x.IsActive && !x.IsDelete);

                if (!membershipExists)
                    return (false, "Membership pasien tidak valid atau tidak aktif.");
            }

            if (request.GuarantorType == PatientEncounterGuarantorType.Insurance &&
                !request.PatientInsuranceId.HasValue &&
                !request.InsuranceProviderId.HasValue)
            {
                return (false, "Penjamin asuransi wajib memiliki PatientInsuranceId atau InsuranceProviderId.");
            }

            if (request.GuarantorType == PatientEncounterGuarantorType.Company &&
                !request.PatientCompanyGuarantorId.HasValue &&
                !request.CompanyGuarantorId.HasValue)
            {
                return (false, "Penjamin company wajib memiliki PatientCompanyGuarantorId atau CompanyGuarantorId.");
            }

            if (encounterId.HasValue && encounterId.Value != Guid.Empty)
            {
                if (request.IsPrimary)
                {
                    var primaryExists = await _dbContext.Set<TrxPatientEncounterGuarantor>()
                        .AnyAsync(x =>
                            x.EncounterId == encounterId.Value &&
                            x.IsPrimary &&
                            !x.IsDelete &&
                            (!excludeGuarantorId.HasValue || x.Id != excludeGuarantorId.Value));

                    if (primaryExists)
                        return (false, "Penjamin utama untuk encounter ini sudah ada.");
                }

                var priorityExists = await _dbContext.Set<TrxPatientEncounterGuarantor>()
                    .AnyAsync(x =>
                        x.EncounterId == encounterId.Value &&
                        x.CoveragePriority == request.CoveragePriority &&
                        !x.IsDelete &&
                        (!excludeGuarantorId.HasValue || x.Id != excludeGuarantorId.Value));

                if (priorityExists)
                    return (false, "Coverage priority penjamin sudah digunakan pada encounter ini.");
            }

            return (true, null);
        }

        // =========================================================
        // BUILD / MAP HELPERS
        // =========================================================

        private List<PatientEncounterGuarantorRequest> BuildDefaultGuarantorsIfEmpty(PatientEncounterCreateRequest request)
        {
            if (request.Guarantors.Any())
                return request.Guarantors;

            return new List<PatientEncounterGuarantorRequest>
            {
                new()
                {
                    GuarantorType = request.PaymentType switch
                    {
                        EncounterPaymentType.Insurance => PatientEncounterGuarantorType.Insurance,
                        EncounterPaymentType.Company => PatientEncounterGuarantorType.Company,
                        _ => PatientEncounterGuarantorType.PatientCash
                    },
                    GuarantorRole = PatientEncounterGuarantorRole.Primary,
                    GuarantorStatus = request.PaymentType == EncounterPaymentType.Cash
                        ? PatientEncounterGuarantorStatus.Eligible
                        : PatientEncounterGuarantorStatus.PendingVerification,
                    CheckMethod = PatientEncounterGuarantorCheckMethod.None,
                    CoveragePriority = 1,
                    IsPrimary = true,
                    PaymentMethodId = request.PaymentMethodId,
                    IsEligibilityRequired = request.PaymentType != EncounterPaymentType.Cash,
                    IsEligible = request.PaymentType == EncounterPaymentType.Cash,
                    IsPolicyActive = request.PaymentType == EncounterPaymentType.Cash,
                    IsAllowExcessPaymentByPatient = true,
                    EligibilityReferenceNumber = request.EligibilityReferenceNumber,
                    EligibilityCheckedAt = request.EligibilityCheckedAt,
                    GuarantorNameSnapshot = request.PaymentType == EncounterPaymentType.Cash ? "Patient Cash" : null
                }
            };
        }

        private async Task<TrxPatientEncounterGuarantor> BuildGuarantorEntityAsync(
            Guid encounterId,
            Guid patientId,
            PatientEncounterGuarantorRequest request,
            DateTime now,
            Guid actorUserId)
        {
            var entity = new TrxPatientEncounterGuarantor
            {
                Id = Guid.NewGuid(),
                EncounterGuarantorNumber = await GenerateEncounterGuarantorNumberAsync(now),
                EncounterId = encounterId,
                PatientId = patientId,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            MapGuarantorRequestToEntity(entity, request, now, actorUserId, isCreate: true);
            await FillGuarantorNameSnapshotAsync(entity);

            return entity;
        }

        private void MapGuarantorRequestToEntity(
            TrxPatientEncounterGuarantor entity,
            PatientEncounterGuarantorRequest request,
            DateTime now,
            Guid actorUserId,
            bool isCreate = false)
        {
            entity.GuarantorType = request.GuarantorType;
            entity.GuarantorRole = request.GuarantorRole;
            entity.GuarantorStatus = request.GuarantorStatus;
            entity.CheckMethod = request.CheckMethod;
            entity.CoveragePriority = request.CoveragePriority;
            entity.IsPrimary = request.IsPrimary;
            entity.PaymentMethodId = NormalizeNullableGuid(request.PaymentMethodId);
            entity.PatientInsuranceId = NormalizeNullableGuid(request.PatientInsuranceId);
            entity.InsuranceProviderId = NormalizeNullableGuid(request.InsuranceProviderId);
            entity.CompanyGuarantorId = NormalizeNullableGuid(request.CompanyGuarantorId);
            entity.PatientCompanyGuarantorId = NormalizeNullableGuid(request.PatientCompanyGuarantorId);
            entity.PatientMembershipId = NormalizeNullableGuid(request.PatientMembershipId);

            entity.GuarantorNameSnapshot = NormalizeNullableText(request.GuarantorNameSnapshot);
            entity.PolicyNumberSnapshot = NormalizeNullableText(request.PolicyNumberSnapshot);
            entity.CardNumberSnapshot = NormalizeNullableText(request.CardNumberSnapshot);
            entity.MemberNumberSnapshot = NormalizeNullableText(request.MemberNumberSnapshot);
            entity.PlanNameSnapshot = NormalizeNullableText(request.PlanNameSnapshot);
            entity.ClassNameSnapshot = NormalizeNullableText(request.ClassNameSnapshot);
            entity.BenefitPlanCodeSnapshot = NormalizeNullableText(request.BenefitPlanCodeSnapshot);
            entity.EffectiveStartDateSnapshot = request.EffectiveStartDateSnapshot;
            entity.EffectiveEndDateSnapshot = request.EffectiveEndDateSnapshot;

            entity.IsEligibilityRequired = request.IsEligibilityRequired;
            entity.IsEligible = request.IsEligible;
            entity.EligibilityReferenceNumber = NormalizeNullableText(request.EligibilityReferenceNumber);
            entity.EligibilityCheckedAt = request.EligibilityCheckedAt;
            entity.VerificationReferenceNumber = NormalizeNullableText(request.VerificationReferenceNumber);
            entity.VerificationOfficerName = NormalizeNullableText(request.VerificationOfficerName);
            entity.VerificationNote = NormalizeNullableText(request.VerificationNote);
            entity.IsNeedApproval = request.IsNeedApproval;
            entity.IsNeedGuaranteeLetter = request.IsNeedGuaranteeLetter;
            entity.IsNeedReferralLetter = request.IsNeedReferralLetter;
            entity.IsAllowExcessPaymentByPatient = request.IsAllowExcessPaymentByPatient;

            entity.CoveragePercent = request.CoveragePercent;
            entity.AnnualLimitAmount = request.AnnualLimitAmount;
            entity.RemainingLimitAmount = request.RemainingLimitAmount;
            entity.UsedLimitAmount = request.UsedLimitAmount;
            entity.RoomLimitPerDayAmount = request.RoomLimitPerDayAmount;
            entity.DeductibleAmount = request.DeductibleAmount;
            entity.CoPaymentPercent = request.CoPaymentPercent;
            entity.CoPaymentAmount = request.CoPaymentAmount;
            entity.EstimatedCoveredAmount = request.EstimatedCoveredAmount;
            entity.EstimatedPatientPayAmount = request.EstimatedPatientPayAmount;

            entity.IsPolicyActive = request.IsPolicyActive;
            entity.IsPremiumPaid = request.IsPremiumPaid;
            entity.IsCardActive = request.IsCardActive;
            entity.IsInWaitingPeriod = request.IsInWaitingPeriod;
            entity.WaitingPeriodUntilDate = request.WaitingPeriodUntilDate;
            entity.HasSpecialExclusion = request.HasSpecialExclusion;
            entity.SpecialExclusionNote = NormalizeNullableText(request.SpecialExclusionNote);
            entity.HasPreviousClaim = request.HasPreviousClaim;
            entity.PreviousClaimNote = NormalizeNullableText(request.PreviousClaimNote);
            entity.BenefitSnapshotJson = NormalizeNullableText(request.BenefitSnapshotJson);
            entity.ManualCheckResultJson = NormalizeNullableText(request.ManualCheckResultJson);
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsActive = true;

            if (!isCreate)
            {
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;
            }
        }

        private async Task FillGuarantorNameSnapshotAsync(TrxPatientEncounterGuarantor entity)
        {
            if (!string.IsNullOrWhiteSpace(entity.GuarantorNameSnapshot))
                return;

            if (entity.GuarantorType == PatientEncounterGuarantorType.PatientCash)
            {
                entity.GuarantorNameSnapshot = "Patient Cash";
                return;
            }

            if (entity.InsuranceProviderId.HasValue)
            {
                entity.GuarantorNameSnapshot = await _dbContext.Set<MstInsuranceProvider>()
                    .Where(x => x.Id == entity.InsuranceProviderId.Value)
                    .Select(x => x.InsuranceProviderName)
                    .FirstOrDefaultAsync();
            }

            if (string.IsNullOrWhiteSpace(entity.GuarantorNameSnapshot) && entity.CompanyGuarantorId.HasValue)
            {
                entity.GuarantorNameSnapshot = await _dbContext.Set<MstCompanyGuarantor>()
                    .Where(x => x.Id == entity.CompanyGuarantorId.Value)
                    .Select(x => x.CompanyGuarantorName)
                    .FirstOrDefaultAsync();
            }

            if (string.IsNullOrWhiteSpace(entity.GuarantorNameSnapshot) && entity.PaymentMethodId.HasValue)
            {
                entity.GuarantorNameSnapshot = await _dbContext.Set<MstPaymentMethod>()
                    .Where(x => x.Id == entity.PaymentMethodId.Value)
                    .Select(x => x.PaymentMethodName)
                    .FirstOrDefaultAsync();
            }
        }

        private static (
            bool IsInsurancePatient,
            bool IsCompanyPatient,
            bool IsMembershipPatient,
            bool IsMixedPayment,
            bool IsEligibilityRequired,
            bool IsEligibilityCompleted,
            string? PrimaryGuarantorNameSnapshot,
            string? PrimaryGuarantorTypeSnapshot) BuildPaymentSummary(
                EncounterPaymentType paymentType,
                List<PatientEncounterGuarantorRequest> guarantors)
        {
            var activeGuarantors = guarantors.Where(x => x.GuarantorStatus != PatientEncounterGuarantorStatus.Cancelled).ToList();
            var primary = activeGuarantors.FirstOrDefault(x => x.IsPrimary) ?? activeGuarantors.OrderBy(x => x.CoveragePriority).FirstOrDefault();

            return (
                IsInsurancePatient: activeGuarantors.Any(x => x.GuarantorType == PatientEncounterGuarantorType.Insurance || x.GuarantorType == PatientEncounterGuarantorType.BPJS),
                IsCompanyPatient: activeGuarantors.Any(x => x.GuarantorType == PatientEncounterGuarantorType.Company),
                IsMembershipPatient: activeGuarantors.Any(x => x.GuarantorType == PatientEncounterGuarantorType.Membership),
                IsMixedPayment: activeGuarantors.Select(x => x.GuarantorType).Distinct().Count() > 1,
                IsEligibilityRequired: activeGuarantors.Any(x => x.IsEligibilityRequired),
                IsEligibilityCompleted: activeGuarantors.Where(x => x.IsEligibilityRequired).All(x => x.IsEligible) && activeGuarantors.Any(x => x.IsEligibilityRequired),
                PrimaryGuarantorNameSnapshot: primary?.GuarantorNameSnapshot,
                PrimaryGuarantorTypeSnapshot: primary?.GuarantorType.ToString()
            );
        }

        private async Task RebuildEncounterPaymentSummaryAsync(
            TrxPatientEncounter encounter,
            TrxPatientEncounterGuarantor? newlyAddedGuarantor,
            DateTime now,
            Guid actorUserId)
        {
            var guarantors = await _dbContext.Set<TrxPatientEncounterGuarantor>()
                .Where(x => x.EncounterId == encounter.Id && !x.IsDelete && x.IsActive && x.GuarantorStatus != PatientEncounterGuarantorStatus.Cancelled)
                .ToListAsync();

            if (newlyAddedGuarantor != null && !guarantors.Any(x => x.Id == newlyAddedGuarantor.Id))
                guarantors.Add(newlyAddedGuarantor);

            var primary = guarantors.FirstOrDefault(x => x.IsPrimary) ?? guarantors.OrderBy(x => x.CoveragePriority).FirstOrDefault();

            encounter.IsInsurancePatient = guarantors.Any(x => x.GuarantorType == PatientEncounterGuarantorType.Insurance || x.GuarantorType == PatientEncounterGuarantorType.BPJS);
            encounter.IsCompanyPatient = guarantors.Any(x => x.GuarantorType == PatientEncounterGuarantorType.Company);
            encounter.IsMembershipPatient = guarantors.Any(x => x.GuarantorType == PatientEncounterGuarantorType.Membership);
            encounter.IsMixedPayment = guarantors.Select(x => x.GuarantorType).Distinct().Count() > 1;
            encounter.PrimaryGuarantorNameSnapshot = primary?.GuarantorNameSnapshot;
            encounter.PrimaryGuarantorTypeSnapshot = primary?.GuarantorType.ToString();
            encounter.IsEligibilityRequired = guarantors.Any(x => x.IsEligibilityRequired);
            encounter.IsEligibilityCompleted = guarantors.Where(x => x.IsEligibilityRequired).All(x => x.IsEligible) && guarantors.Any(x => x.IsEligibilityRequired);
            encounter.UpdateDateTime = now;
            encounter.UpdateBy = actorUserId;
        }

        // =========================================================
        // RESPONSE MAPPING
        // =========================================================

        private static PatientEncounterResponse ToEncounterResponse(TrxPatientEncounter x)
        {
            return new PatientEncounterResponse
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
                IsInsurancePatient = x.IsInsurancePatient,
                IsCompanyPatient = x.IsCompanyPatient,
                IsMembershipPatient = x.IsMembershipPatient,
                IsMixedPayment = x.IsMixedPayment,
                PrimaryGuarantorNameSnapshot = x.PrimaryGuarantorNameSnapshot,
                PrimaryGuarantorTypeSnapshot = x.PrimaryGuarantorTypeSnapshot,
                IsEligibilityRequired = x.IsEligibilityRequired,
                IsEligibilityCompleted = x.IsEligibilityCompleted,
                IsReferral = x.IsReferral,
                IsScreeningRequired = x.IsScreeningRequired,
                IsQueueRequired = x.IsQueueRequired,
                IsDoctorRequired = x.IsDoctorRequired,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static PatientEncounterDetailResponse ToEncounterDetailResponse(TrxPatientEncounter x)
        {
            var response = new PatientEncounterDetailResponse
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
                KioskScanSessionId = x.KioskScanSessionId,
                EncounterType = x.EncounterType,
                VisitType = x.VisitType,
                RegistrationSource = x.RegistrationSource,
                PaymentType = x.PaymentType,
                EncounterStatus = x.EncounterStatus,
                EncounterDate = x.EncounterDate,
                RegisteredAt = x.RegisteredAt,
                ChiefComplaint = x.ChiefComplaint,
                IsInsurancePatient = x.IsInsurancePatient,
                IsCompanyPatient = x.IsCompanyPatient,
                IsMembershipPatient = x.IsMembershipPatient,
                IsMixedPayment = x.IsMixedPayment,
                PrimaryGuarantorNameSnapshot = x.PrimaryGuarantorNameSnapshot,
                PrimaryGuarantorTypeSnapshot = x.PrimaryGuarantorTypeSnapshot,
                EligibilityReferenceNumber = x.EligibilityReferenceNumber,
                EligibilityCheckedAt = x.EligibilityCheckedAt,
                IsEligibilityRequired = x.IsEligibilityRequired,
                IsEligibilityCompleted = x.IsEligibilityCompleted,
                IsReferral = x.IsReferral,
                ReferralNumber = x.ReferralNumber,
                IsReferralRequired = x.IsReferralRequired,
                IsReferralVerified = x.IsReferralVerified,
                CheckedInAt = x.CheckedInAt,
                CompletedAt = x.CompletedAt,
                NoShowAt = x.NoShowAt,
                NoShowReason = x.NoShowReason,
                CancelledAt = x.CancelledAt,
                CancelReason = x.CancelReason,
                Notes = x.Notes,
                IsScreeningRequired = x.IsScreeningRequired,
                IsQueueRequired = x.IsQueueRequired,
                IsDoctorRequired = x.IsDoctorRequired,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                Guarantors = x.EncounterGuarantors
                    .Where(g => !g.IsDelete)
                    .OrderBy(g => g.CoveragePriority)
                    .Select(ToGuarantorResponse)
                    .ToList()
            };

            return response;
        }

        private static PatientEncounterGuarantorResponse ToGuarantorResponse(TrxPatientEncounterGuarantor x)
        {
            return new PatientEncounterGuarantorResponse
            {
                Id = x.Id,
                EncounterGuarantorNumber = x.EncounterGuarantorNumber,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : string.Empty,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                GuarantorType = x.GuarantorType,
                GuarantorRole = x.GuarantorRole,
                GuarantorStatus = x.GuarantorStatus,
                CheckMethod = x.CheckMethod,
                CoveragePriority = x.CoveragePriority,
                IsPrimary = x.IsPrimary,
                PaymentMethodId = x.PaymentMethodId,
                PaymentMethodName = x.PaymentMethod != null ? x.PaymentMethod.PaymentMethodName : null,
                PatientInsuranceId = x.PatientInsuranceId,
                InsuranceProviderId = x.InsuranceProviderId,
                InsuranceProviderName = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : null,
                CompanyGuarantorId = x.CompanyGuarantorId,
                CompanyGuarantorName = x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : null,
                PatientCompanyGuarantorId = x.PatientCompanyGuarantorId,
                PatientMembershipId = x.PatientMembershipId,
                GuarantorNameSnapshot = x.GuarantorNameSnapshot,
                PolicyNumberSnapshot = x.PolicyNumberSnapshot,
                CardNumberSnapshot = x.CardNumberSnapshot,
                MemberNumberSnapshot = x.MemberNumberSnapshot,
                PlanNameSnapshot = x.PlanNameSnapshot,
                ClassNameSnapshot = x.ClassNameSnapshot,
                BenefitPlanCodeSnapshot = x.BenefitPlanCodeSnapshot,
                IsEligibilityRequired = x.IsEligibilityRequired,
                IsEligible = x.IsEligible,
                IsVerified = x.IsVerified,
                VerifiedAt = x.VerifiedAt,
                EligibilityReferenceNumber = x.EligibilityReferenceNumber,
                EligibilityCheckedAt = x.EligibilityCheckedAt,
                IsNeedApproval = x.IsNeedApproval,
                IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                IsNeedReferralLetter = x.IsNeedReferralLetter,
                IsAllowExcessPaymentByPatient = x.IsAllowExcessPaymentByPatient,
                CoveragePercent = x.CoveragePercent,
                AnnualLimitAmount = x.AnnualLimitAmount,
                RemainingLimitAmount = x.RemainingLimitAmount,
                EstimatedCoveredAmount = x.EstimatedCoveredAmount,
                EstimatedPatientPayAmount = x.EstimatedPatientPayAmount,
                IsPolicyActive = x.IsPolicyActive,
                IsPremiumPaid = x.IsPremiumPaid,
                IsCardActive = x.IsCardActive,
                IsInWaitingPeriod = x.IsInWaitingPeriod,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static PatientEncounterGuarantorDetailResponse ToGuarantorDetailResponse(TrxPatientEncounterGuarantor x)
        {
            var response = new PatientEncounterGuarantorDetailResponse
            {
                Id = x.Id,
                EncounterGuarantorNumber = x.EncounterGuarantorNumber,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : string.Empty,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                GuarantorType = x.GuarantorType,
                GuarantorRole = x.GuarantorRole,
                GuarantorStatus = x.GuarantorStatus,
                CheckMethod = x.CheckMethod,
                CoveragePriority = x.CoveragePriority,
                IsPrimary = x.IsPrimary,
                PaymentMethodId = x.PaymentMethodId,
                PaymentMethodName = x.PaymentMethod != null ? x.PaymentMethod.PaymentMethodName : null,
                PatientInsuranceId = x.PatientInsuranceId,
                InsuranceProviderId = x.InsuranceProviderId,
                InsuranceProviderName = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : null,
                CompanyGuarantorId = x.CompanyGuarantorId,
                CompanyGuarantorName = x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : null,
                PatientCompanyGuarantorId = x.PatientCompanyGuarantorId,
                PatientMembershipId = x.PatientMembershipId,
                GuarantorNameSnapshot = x.GuarantorNameSnapshot,
                PolicyNumberSnapshot = x.PolicyNumberSnapshot,
                CardNumberSnapshot = x.CardNumberSnapshot,
                MemberNumberSnapshot = x.MemberNumberSnapshot,
                PlanNameSnapshot = x.PlanNameSnapshot,
                ClassNameSnapshot = x.ClassNameSnapshot,
                BenefitPlanCodeSnapshot = x.BenefitPlanCodeSnapshot,
                EffectiveStartDateSnapshot = x.EffectiveStartDateSnapshot,
                EffectiveEndDateSnapshot = x.EffectiveEndDateSnapshot,
                IsEligibilityRequired = x.IsEligibilityRequired,
                IsEligible = x.IsEligible,
                IsVerified = x.IsVerified,
                VerifiedAt = x.VerifiedAt,
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByUserName = x.VerifiedByUser != null ? x.VerifiedByUser.DisplayName : null,
                EligibilityReferenceNumber = x.EligibilityReferenceNumber,
                EligibilityCheckedAt = x.EligibilityCheckedAt,
                VerificationReferenceNumber = x.VerificationReferenceNumber,
                VerificationOfficerName = x.VerificationOfficerName,
                VerificationNote = x.VerificationNote,
                IsNeedApproval = x.IsNeedApproval,
                IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                IsNeedReferralLetter = x.IsNeedReferralLetter,
                IsAllowExcessPaymentByPatient = x.IsAllowExcessPaymentByPatient,
                CoveragePercent = x.CoveragePercent,
                AnnualLimitAmount = x.AnnualLimitAmount,
                RemainingLimitAmount = x.RemainingLimitAmount,
                UsedLimitAmount = x.UsedLimitAmount,
                RoomLimitPerDayAmount = x.RoomLimitPerDayAmount,
                DeductibleAmount = x.DeductibleAmount,
                CoPaymentPercent = x.CoPaymentPercent,
                CoPaymentAmount = x.CoPaymentAmount,
                EstimatedCoveredAmount = x.EstimatedCoveredAmount,
                EstimatedPatientPayAmount = x.EstimatedPatientPayAmount,
                IsPolicyActive = x.IsPolicyActive,
                IsPremiumPaid = x.IsPremiumPaid,
                IsCardActive = x.IsCardActive,
                IsInWaitingPeriod = x.IsInWaitingPeriod,
                WaitingPeriodUntilDate = x.WaitingPeriodUntilDate,
                HasSpecialExclusion = x.HasSpecialExclusion,
                SpecialExclusionNote = x.SpecialExclusionNote,
                HasPreviousClaim = x.HasPreviousClaim,
                PreviousClaimNote = x.PreviousClaimNote,
                BenefitSnapshotJson = x.BenefitSnapshotJson,
                ManualCheckResultJson = x.ManualCheckResultJson,
                CancelledAt = x.CancelledAt,
                CancelledByUserId = x.CancelledByUserId,
                CancelledByUserName = x.CancelledByUser != null ? x.CancelledByUser.DisplayName : null,
                CancelReason = x.CancelReason,
                Notes = x.Notes,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };

            return response;
        }

        private static PatientEncounterGuarantorCreateResponse ToGuarantorCreateResponse(TrxPatientEncounterGuarantor x)
        {
            return new PatientEncounterGuarantorCreateResponse
            {
                Id = x.Id,
                EncounterGuarantorNumber = x.EncounterGuarantorNumber,
                EncounterId = x.EncounterId,
                GuarantorType = x.GuarantorType,
                GuarantorStatus = x.GuarantorStatus,
                CoveragePriority = x.CoveragePriority,
                IsPrimary = x.IsPrimary,
                GuarantorNameSnapshot = x.GuarantorNameSnapshot
            };
        }

        private static PatientEncounterGuarantorUpdateResponse ToGuarantorUpdateResponse(TrxPatientEncounterGuarantor x)
        {
            return new PatientEncounterGuarantorUpdateResponse
            {
                Id = x.Id,
                EncounterGuarantorNumber = x.EncounterGuarantorNumber,
                EncounterId = x.EncounterId,
                GuarantorType = x.GuarantorType,
                GuarantorStatus = x.GuarantorStatus,
                CoveragePriority = x.CoveragePriority,
                IsPrimary = x.IsPrimary,
                GuarantorNameSnapshot = x.GuarantorNameSnapshot,
                IsEligible = x.IsEligible,
                IsVerified = x.IsVerified,
                IsActive = x.IsActive
            };
        }

        // =========================================================
        // NUMBERING / UTILITIES
        // =========================================================

        private async Task<string> GenerateEncounterNumberAsync(DateTime now)
        {
            var prefix = $"ENC-{now:yyyyMMdd}";
            var countToday = await _dbContext.Set<TrxPatientEncounter>()
                .CountAsync(x => x.EncounterNumber.StartsWith(prefix));

            return $"{prefix}-{countToday + 1:D5}";
        }

        private async Task<string> GenerateEncounterGuarantorNumberAsync(DateTime now)
        {
            var prefix = $"EGT-{now:yyyyMMdd}";
            var countToday = await _dbContext.Set<TrxPatientEncounterGuarantor>()
                .CountAsync(x => x.EncounterGuarantorNumber.StartsWith(prefix));

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
