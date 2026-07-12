using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseDoctorConsultationPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs.DoctorConsultationResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/doctor-consultations")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Doctor Consultation",
        AreaName = "HealthServices",
        ControllerName = "DoctorConsultation",
        Description = "Pemeriksaan dan konsultasi dokter",
        SortOrder = 2
    )]
    [Tags("Health Services / Clinical Management / Doctor Consultation")]
    public class DoctorConsultationController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Clinical";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly ConsultationValidationService _consultationValidationService;
        private readonly ConsultationFinalizationService _consultationFinalizationService;

        public DoctorConsultationController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            ConsultationValidationService consultationValidationService,
            ConsultationFinalizationService consultationFinalizationService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _consultationValidationService = consultationValidationService;
            _consultationFinalizationService = consultationFinalizationService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<DoctorConsultationFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Doctor Consultation", Description = "Melihat metadata filter konsultasi dokter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorConsultation", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new DoctorConsultationFilterMetadataResponse
            {
                DefaultFilter = new DoctorConsultationDefaultFilterResponse(),
                SortOptions = new List<DoctorConsultationSortOptionResponse>
                {
                    new() { Value = "consultationDateTime", Label = "Tanggal konsultasi" },
                    new() { Value = "consultationNumber", Label = "Nomor konsultasi" },
                    new() { Value = "consultationStatus", Label = "Status konsultasi" },
                    new() { Value = "diagnosisCount", Label = "Jumlah diagnosis" },
                    new() { Value = "procedureCount", Label = "Jumlah tindakan" },
                    new() { Value = "prescriptionCount", Label = "Jumlah resep" },
                    new() { Value = "supportingOrderCount", Label = "Jumlah order penunjang" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ConsultationStatusOptions = BuildEnumOptions<DoctorConsultationStatus>()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "DoctorConsultation.GetFilterMetadata",
                "Mengambil metadata filter konsultasi dokter.",
                result
            );

            return Ok(ApiResponse<DoctorConsultationFilterMetadataResponse>.Ok(
                result,
                "Metadata filter konsultasi dokter berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseDoctorConsultationPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Doctor Consultation", Description = "Melihat data konsultasi dokter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorConsultation", "Read")]
        public async Task<IActionResult> GetConsultations(
            [FromQuery] string? search,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? queueId,
            [FromQuery] Guid? assessmentId,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? doctorId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] DoctorConsultationStatus? consultationStatus,
            [FromQuery] bool? hasPrimaryDiagnosis,
            [FromQuery] bool? hasProcedure,
            [FromQuery] bool? hasPrescription,
            [FromQuery] bool? hasSupportingOrder,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy = "consultationDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery().AsNoTracking();

            query = ApplyFilters(
                query,
                search,
                encounterId,
                queueId,
                assessmentId,
                patientId,
                doctorId,
                serviceUnitId,
                clinicId,
                consultationStatus,
                hasPrimaryDiagnosis,
                hasProcedure,
                hasPrescription,
                hasSupportingOrder,
                startDate,
                endDate
            );

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(ToResponse).ToList();

            return Ok(ApiResponse<ResponseDoctorConsultationPagedResult>.Ok(
                new ResponseDoctorConsultationPagedResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data konsultasi dokter berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DoctorConsultationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Doctor Consultation", Description = "Melihat detail konsultasi dokter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorConsultation", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Konsultasi dokter tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<DoctorConsultationDetailResponse>.Ok(
                ToDetailResponse(entity),
                "Detail konsultasi dokter berhasil diambil."
            ));
        }

        [HttpGet("active-by-queue/{queueId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DoctorConsultationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Active Doctor Consultation", Description = "Melihat konsultasi dokter aktif berdasarkan antrean", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorConsultation", "Read")]
        public async Task<IActionResult> GetActiveByQueue(Guid queueId)
        {
            if (queueId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "QueueId wajib diisi."
                ));
            }

            var entity = await BuildBaseQuery()
                .AsNoTracking()
                .Where(x =>
                    x.QueueId == queueId &&
                    x.IsActive &&
                    x.ConsultationStatus != DoctorConsultationStatus.Cancelled)
                .OrderByDescending(x => x.ConsultationStatus == DoctorConsultationStatus.InProgress)
                .ThenByDescending(x => x.UpdateDateTime ?? x.CreateDateTime)
                .ThenByDescending(x => x.ConsultationDateTime)
                .FirstOrDefaultAsync();

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Konsultasi dokter aktif untuk antrean ini tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<DoctorConsultationDetailResponse>.Ok(
                ToDetailResponse(entity),
                "Konsultasi dokter aktif berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DoctorConsultationCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Doctor Consultation", Description = "Membuat konsultasi dokter", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("DoctorConsultation", "Create")]
        public async Task<IActionResult> CreateConsultation([FromBody] CreateDoctorConsultationRequest request)
        {
            var validation = await ValidateCreateRequestAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data konsultasi dokter tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var queue = await _dbContext.Set<TrxQueue>()
                .Include(x => x.Encounter)
                .FirstAsync(x =>
                    x.Id == request.QueueId &&
                    x.EncounterId == request.EncounterId &&
                    !x.IsDelete);

            var assessment = await ResolveAssessmentAsync(request.EncounterId, request.AssessmentId);

            var vitalSign = BuildVitalSignSnapshot(request, assessment);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var entity = new TrxDoctorConsultation
            {
                Id = Guid.NewGuid(),
                ConsultationNumber = await GenerateConsultationNumberAsync(now),
                EncounterId = queue.EncounterId,
                QueueId = queue.Id,
                AssessmentId = assessment?.Id,
                PatientId = queue.PatientId,
                DoctorId = queue.DoctorId ?? Guid.Empty,
                ServiceUnitId = queue.ServiceUnitId,
                ClinicId = queue.ClinicId,
                ConsultationDateTime = now,
                ConsultationStatus = request.CompleteImmediately
                    ? DoctorConsultationStatus.Completed
                    : DoctorConsultationStatus.InProgress,

                IsVitalSignCopiedFromAssessment = request.IsVitalSignCopiedFromAssessment,
                BloodPressureSystolic = vitalSign.BloodPressureSystolic,
                BloodPressureDiastolic = vitalSign.BloodPressureDiastolic,
                PulseRate = vitalSign.PulseRate,
                RespiratoryRate = vitalSign.RespiratoryRate,
                Temperature = vitalSign.Temperature,
                OxygenSaturation = vitalSign.OxygenSaturation,
                Weight = vitalSign.Weight,
                Height = vitalSign.Height,
                BMI = vitalSign.BMI,

                ChiefComplaint = NormalizeNullableText(request.ChiefComplaint),
                HistoryOfPresentIllness = NormalizeNullableText(request.HistoryOfPresentIllness),
                PhysicalExamination = NormalizeNullableText(request.PhysicalExamination),

                Subjective = NormalizeNullableText(request.Subjective),
                Objective = NormalizeNullableText(request.Objective),
                Assessment = NormalizeNullableText(request.Assessment),
                Plan = NormalizeNullableText(request.Plan),

                DiagnosisText = null,
                PrimaryDiagnosisText = null,
                SecondaryDiagnosisText = null,
                DiagnosisCount = 0,
                HasPrimaryDiagnosis = false,

                ProcedureText = null,
                ProcedureCount = 0,
                HasProcedure = false,

                PrescriptionText = null,
                PrescriptionCount = 0,
                HasPrescription = false,

                SupportingOrderText = null,
                SupportingOrderCount = 0,
                HasSupportingOrder = false,

                MedicalCertificateCount = 0,
                ClinicalDocumentCount = 0,
                ConsentCount = 0,

                ProcedurePlan = NormalizeNullableText(request.ProcedurePlan),
                PrescriptionPlan = NormalizeNullableText(request.PrescriptionPlan),
                SupportingExamPlan = NormalizeNullableText(request.SupportingExamPlan),
                ReferralPlan = NormalizeNullableText(request.ReferralPlan),
                EducationPlan = NormalizeNullableText(request.EducationPlan),

                FollowUpDate = request.FollowUpDate,
                FollowUpNote = NormalizeNullableText(request.FollowUpNote),
                DoctorNote = NormalizeNullableText(request.DoctorNote),

                StartedAt = now,
                StartedByUserId = actorUserId,
                CompletedAt = request.CompleteImmediately ? now : null,
                CompletedByUserId = request.CompleteImmediately ? actorUserId : null,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            NormalizeConsultationData(entity);

            _dbContext.Set<TrxDoctorConsultation>().Add(entity);

            queue.QueueStatus = request.CompleteImmediately
                ? QueueStatus.Completed
                : QueueStatus.InConsultation;

            queue.ConsultationStartedAt ??= now;

            if (request.CompleteImmediately)
            {
                queue.ConsultationCompletedAt = now;
                queue.CompletedAt = now;
                queue.CompletedByUserId = actorUserId;
            }

            queue.UpdateDateTime = now;
            queue.UpdateBy = actorUserId;

            if (queue.Encounter != null)
            {
                queue.Encounter.EncounterStatus = request.CompleteImmediately
                    ? EncounterStatus.ConsultationCompleted
                    : EncounterStatus.InConsultation;

                queue.Encounter.UpdateDateTime = now;
                queue.Encounter.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var response = BuildCreateUpdateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "DoctorConsultation.CreateConsultation",
                "Membuat konsultasi dokter.",
                response
            );

            return Ok(ApiResponse<DoctorConsultationCreateResponse>.Ok(
                response,
                "Konsultasi dokter berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DoctorConsultationUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Doctor Consultation", Description = "Mengubah konsultasi dokter", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DoctorConsultation", "Update")]
        public async Task<IActionResult> UpdateConsultation(Guid id, [FromBody] UpdateDoctorConsultationRequest request)
        {
            var entity = await _dbContext.Set<TrxDoctorConsultation>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Konsultasi dokter tidak ditemukan."
                ));
            }

            if (entity.ConsultationStatus == DoctorConsultationStatus.Completed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Konsultasi yang sudah completed tidak dapat diubah."
                ));
            }

            if (entity.ConsultationStatus == DoctorConsultationStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Konsultasi yang sudah cancelled tidak dapat diubah."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            TrxPatientAssessment? assessment = null;

            if (request.IsVitalSignCopiedFromAssessment)
            {
                assessment = await ResolveAssessmentAsync(entity.EncounterId, entity.AssessmentId);
            }

            var vitalSign = BuildVitalSignSnapshot(request, assessment);

            entity.IsVitalSignCopiedFromAssessment = request.IsVitalSignCopiedFromAssessment;
            entity.BloodPressureSystolic = vitalSign.BloodPressureSystolic;
            entity.BloodPressureDiastolic = vitalSign.BloodPressureDiastolic;
            entity.PulseRate = vitalSign.PulseRate;
            entity.RespiratoryRate = vitalSign.RespiratoryRate;
            entity.Temperature = vitalSign.Temperature;
            entity.OxygenSaturation = vitalSign.OxygenSaturation;
            entity.Weight = vitalSign.Weight;
            entity.Height = vitalSign.Height;
            entity.BMI = vitalSign.BMI;

            entity.ChiefComplaint = NormalizeNullableText(request.ChiefComplaint);
            entity.HistoryOfPresentIllness = NormalizeNullableText(request.HistoryOfPresentIllness);
            entity.PhysicalExamination = NormalizeNullableText(request.PhysicalExamination);

            entity.Subjective = NormalizeNullableText(request.Subjective);
            entity.Objective = NormalizeNullableText(request.Objective);
            entity.Assessment = NormalizeNullableText(request.Assessment);
            entity.Plan = NormalizeNullableText(request.Plan);

            entity.ProcedurePlan = NormalizeNullableText(request.ProcedurePlan);
            entity.PrescriptionPlan = NormalizeNullableText(request.PrescriptionPlan);
            entity.SupportingExamPlan = NormalizeNullableText(request.SupportingExamPlan);
            entity.ReferralPlan = NormalizeNullableText(request.ReferralPlan);
            entity.EducationPlan = NormalizeNullableText(request.EducationPlan);

            entity.FollowUpDate = request.FollowUpDate;
            entity.FollowUpNote = NormalizeNullableText(request.FollowUpNote);
            entity.DoctorNote = NormalizeNullableText(request.DoctorNote);

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            NormalizeConsultationData(entity);

            await _dbContext.SaveChangesAsync();

            var response = BuildUpdateResponse(entity);

            return Ok(ApiResponse<DoctorConsultationUpdateResponse>.Ok(
                response,
                "Konsultasi dokter berhasil diubah."
            ));
        }

        [HttpPatch("{id:guid}/soap")]
        [ProducesResponseType(typeof(ApiResponse<DoctorConsultationSoapUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Autosave Doctor Consultation SOAP", Description = "Menyimpan otomatis SOAP konsultasi dokter tanpa mengubah field lain", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("DoctorConsultation", "Update")]
        public async Task<IActionResult> UpdateSoap(Guid id, [FromBody] UpdateDoctorConsultationSoapRequest request)
        {
            if (request == null)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Payload SOAP wajib diisi."
                ));
            }

            var entity = await _dbContext.Set<TrxDoctorConsultation>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Konsultasi dokter tidak ditemukan."
                ));
            }

            if (entity.ConsultationStatus == DoctorConsultationStatus.Completed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "SOAP pada konsultasi yang sudah completed tidak dapat diubah."
                ));
            }

            if (entity.ConsultationStatus == DoctorConsultationStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "SOAP pada konsultasi yang sudah cancelled tidak dapat diubah."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            ApplySoapPatch(entity, request);

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            NormalizeConsultationData(entity);

            await _dbContext.SaveChangesAsync();

            var response = BuildSoapUpdateResponse(entity, now);

            await _loggerService.InfoAsync(
                LogCategory,
                "DoctorConsultation.UpdateSoap",
                "Menyimpan SOAP konsultasi dokter.",
                response
            );

            return Ok(ApiResponse<DoctorConsultationSoapUpdateResponse>.Ok(
                response,
                "SOAP konsultasi dokter berhasil disimpan."
            ));
        }

        [HttpGet("{id:guid}/finalization-validation")]
        [ProducesResponseType(typeof(ApiResponse<ConsultationFinalizationValidationResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Validate Doctor Consultation Finalization", Description = "Memvalidasi seluruh tab sebelum konsultasi diselesaikan", AccessType = AccessTypes.Read, SortOrder = 4)]
        [AccessPermission("DoctorConsultation", "Read")]
        public async Task<IActionResult> ValidateFinalization(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _consultationValidationService.ValidateAsync(id, cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "DoctorConsultation.ValidateFinalization",
                "Memvalidasi kesiapan finalisasi konsultasi dokter.",
                result
            );

            return Ok(ApiResponse<ConsultationFinalizationValidationResponse>.Ok(
                result,
                result.CanFinalize
                    ? "Konsultasi siap diselesaikan."
                    : "Konsultasi masih memiliki data yang perlu diperbaiki."
            ));
        }

        [HttpPatch("{id:guid}/complete")]
        [ProducesResponseType(typeof(ApiResponse<ConsultationFinalizationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ConsultationFinalizationValidationResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Complete Doctor Consultation", Description = "Memvalidasi dan menyelesaikan seluruh proses konsultasi dokter", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("DoctorConsultation", "Update")]
        public async Task<IActionResult> CompleteConsultation(
            Guid id,
            [FromBody] FinalizeDoctorConsultationRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _consultationFinalizationService.FinalizeAsync(
                id,
                request,
                actorUserId,
                cancellationToken
            );

            if (result.IsConflict)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict,
                    result.ErrorMessage ?? "Data konsultasi telah berubah."
                ));
            }

            if (!result.IsSuccess)
            {
                if (result.Validation != null)
                {
                    return BadRequest(ApiResponse<ConsultationFinalizationValidationResponse>.Ok(
                        result.Validation,
                        result.ErrorMessage ?? "Konsultasi belum dapat diselesaikan."
                    ));
                }

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    result.ErrorMessage ?? "Konsultasi belum dapat diselesaikan."
                ));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "DoctorConsultation.CompleteConsultation",
                "Memvalidasi dan menyelesaikan konsultasi dokter.",
                result.Data
            );

            return Ok(ApiResponse<ConsultationFinalizationResponse>.Ok(
                result.Data!,
                "Konsultasi dokter berhasil diselesaikan dan transaksi klinis telah difinalkan."
            ));
        }

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Cancel Doctor Consultation", Description = "Membatalkan konsultasi dokter", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("DoctorConsultation", "Update")]
        public async Task<IActionResult> CancelConsultation(Guid id, [FromBody] CancelDoctorConsultationRequest request)
        {
            var entity = await _dbContext.Set<TrxDoctorConsultation>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Konsultasi dokter tidak ditemukan."
                ));
            }

            if (entity.ConsultationStatus == DoctorConsultationStatus.Completed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Konsultasi yang sudah completed tidak dapat dibatalkan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.ConsultationStatus = DoctorConsultationStatus.Cancelled;
            entity.CancelledAt = now;
            entity.CancelledByUserId = actorUserId;
            entity.CancelReason = request.CancelReason.Trim();
            entity.IsActive = false;
            entity.IsCancel = true;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Konsultasi dokter berhasil dibatalkan."
            ));
        }

        private IQueryable<TrxDoctorConsultation> BuildBaseQuery()
        {
            return _dbContext.Set<TrxDoctorConsultation>()
                .Include(x => x.Encounter)
                .Include(x => x.Queue)
                .Include(x => x.PatientAssessment)
                .Include(x => x.Patient)
                .Include(x => x.Doctor)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.StartedByUser)
                .Include(x => x.CompletedByUser)
                .Include(x => x.CancelledByUser)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<TrxDoctorConsultation> ApplyFilters(
            IQueryable<TrxDoctorConsultation> query,
            string? search,
            Guid? encounterId,
            Guid? queueId,
            Guid? assessmentId,
            Guid? patientId,
            Guid? doctorId,
            Guid? serviceUnitId,
            Guid? clinicId,
            DoctorConsultationStatus? consultationStatus,
            bool? hasPrimaryDiagnosis,
            bool? hasProcedure,
            bool? hasPrescription,
            bool? hasSupportingOrder,
            DateTime? startDate,
            DateTime? endDate)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ConsultationNumber.ToLower().Contains(keyword) ||
                    (x.Encounter != null && x.Encounter.EncounterNumber.ToLower().Contains(keyword)) ||
                    (x.Queue != null && x.Queue.QueueCode.ToLower().Contains(keyword)) ||
                    (x.PatientAssessment != null && x.PatientAssessment.AssessmentNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.FullName.ToLower().Contains(keyword)) ||
                    (x.ChiefComplaint != null && x.ChiefComplaint.ToLower().Contains(keyword)) ||
                    (x.DiagnosisText != null && x.DiagnosisText.ToLower().Contains(keyword)) ||
                    (x.PrimaryDiagnosisText != null && x.PrimaryDiagnosisText.ToLower().Contains(keyword)) ||
                    (x.SecondaryDiagnosisText != null && x.SecondaryDiagnosisText.ToLower().Contains(keyword)) ||
                    (x.ProcedureText != null && x.ProcedureText.ToLower().Contains(keyword)) ||
                    (x.PrescriptionText != null && x.PrescriptionText.ToLower().Contains(keyword)) ||
                    (x.SupportingOrderText != null && x.SupportingOrderText.ToLower().Contains(keyword)));
            }

            if (encounterId.HasValue && encounterId.Value != Guid.Empty)
                query = query.Where(x => x.EncounterId == encounterId.Value);

            if (queueId.HasValue && queueId.Value != Guid.Empty)
                query = query.Where(x => x.QueueId == queueId.Value);

            if (assessmentId.HasValue && assessmentId.Value != Guid.Empty)
                query = query.Where(x => x.AssessmentId == assessmentId.Value);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (doctorId.HasValue && doctorId.Value != Guid.Empty)
                query = query.Where(x => x.DoctorId == doctorId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
                query = query.Where(x => x.ClinicId == clinicId.Value);

            if (consultationStatus.HasValue)
                query = query.Where(x => x.ConsultationStatus == consultationStatus.Value);

            if (hasPrimaryDiagnosis.HasValue)
                query = query.Where(x => x.HasPrimaryDiagnosis == hasPrimaryDiagnosis.Value);

            if (hasProcedure.HasValue)
                query = query.Where(x => x.HasProcedure == hasProcedure.Value);

            if (hasPrescription.HasValue)
                query = query.Where(x => x.HasPrescription == hasPrescription.Value);

            if (hasSupportingOrder.HasValue)
                query = query.Where(x => x.HasSupportingOrder == hasSupportingOrder.Value);

            if (startDate.HasValue)
                query = query.Where(x => x.ConsultationDateTime >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(x => x.ConsultationDateTime < endDate.Value.Date.AddDays(1));

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(
            CreateDoctorConsultationRequest request)
        {
            var queue = await _dbContext.Set<TrxQueue>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.QueueId &&
                    x.EncounterId == request.EncounterId &&
                    !x.IsDelete);

            if (queue == null)
                return (false, "Antrean tidak ditemukan atau tidak sesuai dengan encounter.");

            if (!queue.IsDoctorRequired)
                return (false, "Antrean ini tidak membutuhkan pemeriksaan dokter.");

            if (!queue.DoctorId.HasValue || queue.DoctorId.Value == Guid.Empty)
                return (false, "Dokter pada antrean belum ditentukan.");

            if (queue.QueueStatus != QueueStatus.CalledByDoctor &&
                queue.QueueStatus != QueueStatus.InConsultation &&
                queue.QueueStatus != QueueStatus.WaitingForDoctor)
            {
                return (false, "Status antrean tidak valid untuk konsultasi dokter.");
            }

            var consultationExists = await _dbContext.Set<TrxDoctorConsultation>()
                .AnyAsync(x =>
                    x.EncounterId == request.EncounterId &&
                    !x.IsDelete);

            if (consultationExists)
                return (false, "Konsultasi dokter untuk encounter ini sudah ada.");

            if (request.AssessmentId.HasValue && request.AssessmentId.Value != Guid.Empty)
            {
                var assessmentExists = await _dbContext.Set<TrxPatientAssessment>()
                    .AnyAsync(x =>
                        x.Id == request.AssessmentId.Value &&
                        x.EncounterId == request.EncounterId &&
                        x.AssessmentStatus == PatientAssessmentStatus.Completed &&
                        !x.IsDelete);

                if (!assessmentExists)
                    return (false, "Assessment pasien tidak valid atau belum completed.");
            }

            return (true, null);
        }

        private async Task<TrxPatientAssessment?> ResolveAssessmentAsync(Guid encounterId, Guid? assessmentId)
        {
            if (assessmentId.HasValue && assessmentId.Value != Guid.Empty)
            {
                return await _dbContext.Set<TrxPatientAssessment>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == assessmentId.Value &&
                        x.EncounterId == encounterId &&
                        !x.IsDelete);
            }

            return await _dbContext.Set<TrxPatientAssessment>()
                .AsNoTracking()
                .Where(x =>
                    x.EncounterId == encounterId &&
                    !x.IsDelete)
                .OrderByDescending(x => x.AssessmentDateTime)
                .FirstOrDefaultAsync();
        }

        private async Task<string> GenerateConsultationNumberAsync(DateTime now)
        {
            var prefix = $"CON-{now:yyyyMMdd}";
            var countToday = await _dbContext.Set<TrxDoctorConsultation>()
                .CountAsync(x => x.ConsultationNumber.StartsWith(prefix));

            return $"{prefix}-{countToday + 1:D5}";
        }

        private static DoctorVitalSignSnapshot BuildVitalSignSnapshot(
            CreateDoctorConsultationRequest request,
            TrxPatientAssessment? assessment)
        {
            if (request.IsVitalSignCopiedFromAssessment && assessment != null)
            {
                return BuildVitalSignFromAssessment(assessment);
            }

            return new DoctorVitalSignSnapshot
            {
                BloodPressureSystolic = request.BloodPressureSystolic,
                BloodPressureDiastolic = request.BloodPressureDiastolic,
                PulseRate = request.PulseRate,
                RespiratoryRate = request.RespiratoryRate,
                Temperature = request.Temperature,
                OxygenSaturation = request.OxygenSaturation,
                Weight = request.Weight,
                Height = request.Height,
                BMI = CalculateBmi(request.Weight, request.Height)
            };
        }

        private static DoctorVitalSignSnapshot BuildVitalSignSnapshot(
            UpdateDoctorConsultationRequest request,
            TrxPatientAssessment? assessment)
        {
            if (request.IsVitalSignCopiedFromAssessment && assessment != null)
            {
                return BuildVitalSignFromAssessment(assessment);
            }

            return new DoctorVitalSignSnapshot
            {
                BloodPressureSystolic = request.BloodPressureSystolic,
                BloodPressureDiastolic = request.BloodPressureDiastolic,
                PulseRate = request.PulseRate,
                RespiratoryRate = request.RespiratoryRate,
                Temperature = request.Temperature,
                OxygenSaturation = request.OxygenSaturation,
                Weight = request.Weight,
                Height = request.Height,
                BMI = CalculateBmi(request.Weight, request.Height)
            };
        }

        private static DoctorVitalSignSnapshot BuildVitalSignFromAssessment(TrxPatientAssessment assessment)
        {
            return new DoctorVitalSignSnapshot
            {
                BloodPressureSystolic = assessment.BloodPressureSystolic,
                BloodPressureDiastolic = assessment.BloodPressureDiastolic,
                PulseRate = assessment.PulseRate,
                RespiratoryRate = assessment.RespiratoryRate,
                Temperature = assessment.Temperature,
                OxygenSaturation = assessment.OxygenSaturation,
                Weight = assessment.Weight,
                Height = assessment.Height,
                BMI = assessment.BMI
            };
        }

        private static decimal? CalculateBmi(decimal? weightKg, decimal? heightCm)
        {
            if (!weightKg.HasValue || !heightCm.HasValue || heightCm.Value <= 0)
                return null;

            var heightMeter = heightCm.Value / 100;
            var bmi = weightKg.Value / (heightMeter * heightMeter);

            return Math.Round(bmi, 2);
        }

        private static void NormalizeConsultationData(TrxDoctorConsultation entity)
        {
            if (entity.DiagnosisCount < 0)
                entity.DiagnosisCount = 0;

            if (entity.ProcedureCount < 0)
                entity.ProcedureCount = 0;

            if (entity.PrescriptionCount < 0)
                entity.PrescriptionCount = 0;

            if (entity.SupportingOrderCount < 0)
                entity.SupportingOrderCount = 0;

            if (entity.MedicalCertificateCount < 0)
                entity.MedicalCertificateCount = 0;

            if (entity.ClinicalDocumentCount < 0)
                entity.ClinicalDocumentCount = 0;

            if (entity.ConsentCount < 0)
                entity.ConsentCount = 0;

            entity.HasPrimaryDiagnosis = entity.DiagnosisCount > 0 && entity.HasPrimaryDiagnosis;
            entity.HasProcedure = entity.ProcedureCount > 0;
            entity.HasPrescription = entity.PrescriptionCount > 0;
            entity.HasSupportingOrder = entity.SupportingOrderCount > 0;

            if (!entity.HasPrimaryDiagnosis)
                entity.PrimaryDiagnosisText = null;

            if (entity.DiagnosisCount == 0)
            {
                entity.DiagnosisText = null;
                entity.SecondaryDiagnosisText = null;
            }

            if (entity.ProcedureCount == 0)
                entity.ProcedureText = null;

            if (entity.PrescriptionCount == 0)
                entity.PrescriptionText = null;

            if (entity.SupportingOrderCount == 0)
                entity.SupportingOrderText = null;
        }

        private static IQueryable<TrxDoctorConsultation> ApplySorting(
            IQueryable<TrxDoctorConsultation> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "consultationDateTime").ToLowerInvariant() switch
            {
                "consultationnumber" => isDesc ? query.OrderByDescending(x => x.ConsultationNumber) : query.OrderBy(x => x.ConsultationNumber),
                "consultationstatus" => isDesc ? query.OrderByDescending(x => x.ConsultationStatus) : query.OrderBy(x => x.ConsultationStatus),
                "diagnosiscount" => isDesc ? query.OrderByDescending(x => x.DiagnosisCount) : query.OrderBy(x => x.DiagnosisCount),
                "procedurecount" => isDesc ? query.OrderByDescending(x => x.ProcedureCount) : query.OrderBy(x => x.ProcedureCount),
                "prescriptioncount" => isDesc ? query.OrderByDescending(x => x.PrescriptionCount) : query.OrderBy(x => x.PrescriptionCount),
                "supportingordercount" => isDesc ? query.OrderByDescending(x => x.SupportingOrderCount) : query.OrderBy(x => x.SupportingOrderCount),
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDesc ? query.OrderByDescending(x => x.ConsultationDateTime) : query.OrderBy(x => x.ConsultationDateTime)
            };
        }

        private static DoctorConsultationResponse ToResponse(TrxDoctorConsultation x)
        {
            return new DoctorConsultationResponse
            {
                Id = x.Id,
                ConsultationNumber = x.ConsultationNumber,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : string.Empty,
                QueueId = x.QueueId,
                QueueCode = x.Queue != null ? x.Queue.QueueCode : string.Empty,
                AssessmentId = x.AssessmentId,
                AssessmentNumber = x.PatientAssessment != null ? x.PatientAssessment.AssessmentNumber : null,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : string.Empty,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                ConsultationDateTime = x.ConsultationDateTime,
                ConsultationStatus = x.ConsultationStatus,
                IsVitalSignCopiedFromAssessment = x.IsVitalSignCopiedFromAssessment,
                ChiefComplaint = x.ChiefComplaint,
                DiagnosisText = x.DiagnosisText,
                PrimaryDiagnosisText = x.PrimaryDiagnosisText,
                SecondaryDiagnosisText = x.SecondaryDiagnosisText,
                DiagnosisCount = x.DiagnosisCount,
                HasPrimaryDiagnosis = x.HasPrimaryDiagnosis,
                ProcedureText = x.ProcedureText,
                ProcedureCount = x.ProcedureCount,
                HasProcedure = x.HasProcedure,
                PrescriptionText = x.PrescriptionText,
                PrescriptionCount = x.PrescriptionCount,
                HasPrescription = x.HasPrescription,
                SupportingOrderText = x.SupportingOrderText,
                SupportingOrderCount = x.SupportingOrderCount,
                HasSupportingOrder = x.HasSupportingOrder,
                MedicalCertificateCount = x.MedicalCertificateCount,
                ClinicalDocumentCount = x.ClinicalDocumentCount,
                ConsentCount = x.ConsentCount,
                StartedAt = x.StartedAt,
                StartedByUserId = x.StartedByUserId,
                StartedByUserName = x.StartedByUser != null ? x.StartedByUser.DisplayName : null,
                CompletedAt = x.CompletedAt,
                CompletedByUserId = x.CompletedByUserId,
                CompletedByUserName = x.CompletedByUser != null ? x.CompletedByUser.DisplayName : null,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static DoctorConsultationDetailResponse ToDetailResponse(TrxDoctorConsultation x)
        {
            return new DoctorConsultationDetailResponse
            {
                Id = x.Id,
                ConsultationNumber = x.ConsultationNumber,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : string.Empty,
                QueueId = x.QueueId,
                QueueCode = x.Queue != null ? x.Queue.QueueCode : string.Empty,
                AssessmentId = x.AssessmentId,
                AssessmentNumber = x.PatientAssessment != null ? x.PatientAssessment.AssessmentNumber : null,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : string.Empty,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                ConsultationDateTime = x.ConsultationDateTime,
                ConsultationStatus = x.ConsultationStatus,
                IsVitalSignCopiedFromAssessment = x.IsVitalSignCopiedFromAssessment,

                BloodPressureSystolic = x.BloodPressureSystolic,
                BloodPressureDiastolic = x.BloodPressureDiastolic,
                PulseRate = x.PulseRate,
                RespiratoryRate = x.RespiratoryRate,
                Temperature = x.Temperature,
                OxygenSaturation = x.OxygenSaturation,
                Weight = x.Weight,
                Height = x.Height,
                BMI = x.BMI,

                ChiefComplaint = x.ChiefComplaint,
                HistoryOfPresentIllness = x.HistoryOfPresentIllness,
                PhysicalExamination = x.PhysicalExamination,

                Subjective = x.Subjective,
                Objective = x.Objective,
                Assessment = x.Assessment,
                Plan = x.Plan,

                DiagnosisText = x.DiagnosisText,
                PrimaryDiagnosisText = x.PrimaryDiagnosisText,
                SecondaryDiagnosisText = x.SecondaryDiagnosisText,
                DiagnosisCount = x.DiagnosisCount,
                HasPrimaryDiagnosis = x.HasPrimaryDiagnosis,

                ProcedureText = x.ProcedureText,
                ProcedureCount = x.ProcedureCount,
                HasProcedure = x.HasProcedure,

                PrescriptionText = x.PrescriptionText,
                PrescriptionCount = x.PrescriptionCount,
                HasPrescription = x.HasPrescription,

                SupportingOrderText = x.SupportingOrderText,
                SupportingOrderCount = x.SupportingOrderCount,
                HasSupportingOrder = x.HasSupportingOrder,

                MedicalCertificateCount = x.MedicalCertificateCount,
                ClinicalDocumentCount = x.ClinicalDocumentCount,
                ConsentCount = x.ConsentCount,

                ProcedurePlan = x.ProcedurePlan,
                PrescriptionPlan = x.PrescriptionPlan,
                SupportingExamPlan = x.SupportingExamPlan,
                ReferralPlan = x.ReferralPlan,
                EducationPlan = x.EducationPlan,

                FollowUpDate = x.FollowUpDate,
                FollowUpNote = x.FollowUpNote,

                StartedAt = x.StartedAt,
                StartedByUserId = x.StartedByUserId,
                StartedByUserName = x.StartedByUser != null ? x.StartedByUser.DisplayName : null,
                CompletedAt = x.CompletedAt,
                CompletedByUserId = x.CompletedByUserId,
                CompletedByUserName = x.CompletedByUser != null ? x.CompletedByUser.DisplayName : null,
                CancelledAt = x.CancelledAt,
                CancelledByUserId = x.CancelledByUserId,
                CancelledByUserName = x.CancelledByUser != null ? x.CancelledByUser.DisplayName : null,
                CancelReason = x.CancelReason,
                DoctorNote = x.DoctorNote,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static DoctorConsultationCreateResponse BuildCreateUpdateResponse(TrxDoctorConsultation entity)
        {
            return new DoctorConsultationCreateResponse
            {
                Id = entity.Id,
                ConsultationNumber = entity.ConsultationNumber,
                EncounterId = entity.EncounterId,
                QueueId = entity.QueueId,
                AssessmentId = entity.AssessmentId,
                ConsultationStatus = entity.ConsultationStatus,
                ConsultationDateTime = entity.ConsultationDateTime,
                StartedAt = entity.StartedAt,
                CompletedAt = entity.CompletedAt,
                IsVitalSignCopiedFromAssessment = entity.IsVitalSignCopiedFromAssessment,
                DiagnosisCount = entity.DiagnosisCount,
                HasPrimaryDiagnosis = entity.HasPrimaryDiagnosis,
                ProcedureCount = entity.ProcedureCount,
                HasProcedure = entity.HasProcedure,
                PrescriptionCount = entity.PrescriptionCount,
                HasPrescription = entity.HasPrescription,
                SupportingOrderCount = entity.SupportingOrderCount,
                HasSupportingOrder = entity.HasSupportingOrder
            };
        }

        private static DoctorConsultationUpdateResponse BuildUpdateResponse(TrxDoctorConsultation entity)
        {
            return new DoctorConsultationUpdateResponse
            {
                Id = entity.Id,
                ConsultationNumber = entity.ConsultationNumber,
                EncounterId = entity.EncounterId,
                QueueId = entity.QueueId,
                AssessmentId = entity.AssessmentId,
                ConsultationStatus = entity.ConsultationStatus,
                ConsultationDateTime = entity.ConsultationDateTime,
                StartedAt = entity.StartedAt,
                CompletedAt = entity.CompletedAt,
                IsVitalSignCopiedFromAssessment = entity.IsVitalSignCopiedFromAssessment,
                DiagnosisCount = entity.DiagnosisCount,
                HasPrimaryDiagnosis = entity.HasPrimaryDiagnosis,
                ProcedureCount = entity.ProcedureCount,
                HasProcedure = entity.HasProcedure,
                PrescriptionCount = entity.PrescriptionCount,
                HasPrescription = entity.HasPrescription,
                SupportingOrderCount = entity.SupportingOrderCount,
                HasSupportingOrder = entity.HasSupportingOrder
            };
        }


        private static void ApplySoapPatch(TrxDoctorConsultation entity, UpdateDoctorConsultationSoapRequest request)
        {
            if (request.Subjective != null)
                entity.Subjective = NormalizeNullableText(request.Subjective);

            if (request.Objective != null)
                entity.Objective = NormalizeNullableText(request.Objective);

            if (request.Assessment != null)
                entity.Assessment = NormalizeNullableText(request.Assessment);

            if (request.Plan != null)
                entity.Plan = NormalizeNullableText(request.Plan);

            if (request.ProcedurePlan != null)
                entity.ProcedurePlan = NormalizeNullableText(request.ProcedurePlan);

            if (request.PrescriptionPlan != null)
                entity.PrescriptionPlan = NormalizeNullableText(request.PrescriptionPlan);

            if (request.SupportingExamPlan != null)
                entity.SupportingExamPlan = NormalizeNullableText(request.SupportingExamPlan);

            if (request.ReferralPlan != null)
                entity.ReferralPlan = NormalizeNullableText(request.ReferralPlan);

            if (request.EducationPlan != null)
                entity.EducationPlan = NormalizeNullableText(request.EducationPlan);

            if (request.ClearFollowUpDate)
            {
                entity.FollowUpDate = null;
            }
            else if (request.FollowUpDate.HasValue)
            {
                entity.FollowUpDate = request.FollowUpDate.Value;
            }

            if (request.FollowUpNote != null)
                entity.FollowUpNote = NormalizeNullableText(request.FollowUpNote);

            if (request.DoctorNote != null)
                entity.DoctorNote = NormalizeNullableText(request.DoctorNote);
        }

        private static DoctorConsultationSoapUpdateResponse BuildSoapUpdateResponse(
            TrxDoctorConsultation entity,
            DateTime savedAt)
        {
            return new DoctorConsultationSoapUpdateResponse
            {
                Id = entity.Id,
                ConsultationNumber = entity.ConsultationNumber,
                EncounterId = entity.EncounterId,
                QueueId = entity.QueueId,
                AssessmentId = entity.AssessmentId,
                ConsultationStatus = entity.ConsultationStatus,
                ConsultationDateTime = entity.ConsultationDateTime,
                StartedAt = entity.StartedAt,
                CompletedAt = entity.CompletedAt,
                IsVitalSignCopiedFromAssessment = entity.IsVitalSignCopiedFromAssessment,
                DiagnosisCount = entity.DiagnosisCount,
                HasPrimaryDiagnosis = entity.HasPrimaryDiagnosis,
                ProcedureCount = entity.ProcedureCount,
                HasProcedure = entity.HasProcedure,
                PrescriptionCount = entity.PrescriptionCount,
                HasPrescription = entity.HasPrescription,
                SupportingOrderCount = entity.SupportingOrderCount,
                HasSupportingOrder = entity.HasSupportingOrder,
                Subjective = entity.Subjective,
                Objective = entity.Objective,
                Assessment = entity.Assessment,
                Plan = entity.Plan,
                ProcedurePlan = entity.ProcedurePlan,
                PrescriptionPlan = entity.PrescriptionPlan,
                SupportingExamPlan = entity.SupportingExamPlan,
                ReferralPlan = entity.ReferralPlan,
                EducationPlan = entity.EducationPlan,
                FollowUpDate = entity.FollowUpDate,
                FollowUpNote = entity.FollowUpNote,
                DoctorNote = entity.DoctorNote,
                SavedAt = savedAt
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<DoctorConsultationEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new DoctorConsultationEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = SplitPascalCase(x.ToString())
                })
                .ToList();
        }

        private static string SplitPascalCase(string value)
        {
            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userId, out var id)
                ? id
                : Guid.Empty;
        }

        private class DoctorVitalSignSnapshot
        {
            public int? BloodPressureSystolic { get; set; }
            public int? BloodPressureDiastolic { get; set; }
            public int? PulseRate { get; set; }
            public int? RespiratoryRate { get; set; }
            public decimal? Temperature { get; set; }
            public decimal? OxygenSaturation { get; set; }
            public decimal? Weight { get; set; }
            public decimal? Height { get; set; }
            public decimal? BMI { get; set; }
        }
    }
}