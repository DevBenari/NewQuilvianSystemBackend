using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponsePatientAssessmentPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs.PatientAssessmentResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/patient-assessments")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Patient Assessment",
        AreaName = "HealthServices",
        ControllerName = "PatientAssessment",
        Description = "Screening awal pasien oleh perawat",
        SortOrder = 1
    )]
    [Tags("Health Services / Clinical Management / Patient Assessment")]
    public class PatientAssessmentController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Clinical";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PatientAssessmentController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientAssessmentPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Assessment", Description = "Melihat data assessment pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientAssessment", "Read")]
        public async Task<IActionResult> GetAssessments(
            [FromQuery] string? search,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? queueId,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? doctorId,
            [FromQuery] PatientAssessmentStatus? assessmentStatus,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy = "assessmentDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<TrxPatientAssessment>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.AssessmentNumber.ToLower().Contains(keyword) ||
                    (x.Encounter != null && x.Encounter.EncounterNumber.ToLower().Contains(keyword)) ||
                    (x.Queue != null && x.Queue.QueueCode.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.ChiefComplaint != null && x.ChiefComplaint.ToLower().Contains(keyword)));
            }

            if (encounterId.HasValue && encounterId.Value != Guid.Empty)
                query = query.Where(x => x.EncounterId == encounterId.Value);

            if (queueId.HasValue && queueId.Value != Guid.Empty)
                query = query.Where(x => x.QueueId == queueId.Value);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
                query = query.Where(x => x.ClinicId == clinicId.Value);

            if (doctorId.HasValue && doctorId.Value != Guid.Empty)
                query = query.Where(x => x.DoctorId == doctorId.Value);

            if (assessmentStatus.HasValue)
                query = query.Where(x => x.AssessmentStatus == assessmentStatus.Value);

            if (startDate.HasValue)
                query = query.Where(x => x.AssessmentDateTime >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(x => x.AssessmentDateTime < endDate.Value.Date.AddDays(1));

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => ToResponse(x))
                .ToListAsync();

            var result = new ResponsePatientAssessmentPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePatientAssessmentPagedResult>.Ok(
                result,
                "Data assessment pasien berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientAssessmentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Assessment", Description = "Melihat detail assessment pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientAssessment", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _dbContext.Set<TrxPatientAssessment>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => ToDetailResponse(x))
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Assessment pasien tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientAssessmentDetailResponse>.Ok(
                result,
                "Detail assessment pasien berhasil diambil."
            ));
        }

        [HttpGet("active-by-encounter/{encounterId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientAssessmentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Assessment", Description = "Melihat draft/assessment aktif berdasarkan encounter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientAssessment", "Read")]
        public async Task<IActionResult> GetActiveByEncounter(Guid encounterId, [FromQuery] Guid? queueId = null)
        {
            var query = _dbContext.Set<TrxPatientAssessment>()
                .AsNoTracking()
                .Where(x =>
                    x.EncounterId == encounterId &&
                    !x.IsDelete &&
                    x.IsActive &&
                    x.AssessmentStatus != PatientAssessmentStatus.Cancelled);

            if (queueId.HasValue && queueId.Value != Guid.Empty)
            {
                query = query.Where(x => x.QueueId == queueId.Value);
            }

            var result = await query
                .OrderByDescending(x => x.AssessmentStatus == PatientAssessmentStatus.InProgress)
                .ThenByDescending(x => x.UpdateDateTime ?? x.CreateDateTime)
                .ThenByDescending(x => x.AssessmentDateTime)
                .Select(x => ToDetailResponse(x))
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Draft/assessment aktif untuk encounter ini tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientAssessmentDetailResponse>.Ok(
                result,
                "Draft/assessment aktif berhasil diambil."
            ));
        }

        [HttpGet("active-by-queue/{queueId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientAssessmentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Assessment", Description = "Melihat draft/assessment aktif berdasarkan antrean", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientAssessment", "Read")]
        public async Task<IActionResult> GetActiveByQueue(Guid queueId)
        {
            var result = await _dbContext.Set<TrxPatientAssessment>()
                .AsNoTracking()
                .Where(x =>
                    x.QueueId == queueId &&
                    !x.IsDelete &&
                    x.IsActive &&
                    x.AssessmentStatus != PatientAssessmentStatus.Cancelled)
                .OrderByDescending(x => x.AssessmentStatus == PatientAssessmentStatus.InProgress)
                .ThenByDescending(x => x.UpdateDateTime ?? x.CreateDateTime)
                .ThenByDescending(x => x.AssessmentDateTime)
                .Select(x => ToDetailResponse(x))
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Draft/assessment aktif untuk antrean ini tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientAssessmentDetailResponse>.Ok(
                result,
                "Draft/assessment aktif berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientAssessmentCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Patient Assessment", Description = "Membuat assessment pasien", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PatientAssessment", "Create")]
        public async Task<IActionResult> CreateAssessment([FromBody] CreatePatientAssessmentRequest request)
        {
            var validation = await ValidateCreateRequestAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data assessment pasien tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var queue = await _dbContext.Set<TrxQueue>()
                .Include(x => x.Encounter)
                .FirstAsync(x => x.Id == request.QueueId && x.EncounterId == request.EncounterId && !x.IsDelete);

            var calculated = CalculateAssessmentValues(request);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var entity = new TrxPatientAssessment
            {
                Id = Guid.NewGuid(),
                AssessmentNumber = await GenerateAssessmentNumberAsync(now),
                EncounterId = queue.EncounterId,
                QueueId = queue.Id,
                PatientId = queue.PatientId,
                ServiceUnitId = queue.ServiceUnitId,
                ClinicId = queue.ClinicId,
                DoctorId = queue.DoctorId,
                AssessmentDateTime = now,
                AssessmentStatus = request.CompleteImmediately
                    ? PatientAssessmentStatus.Completed
                    : PatientAssessmentStatus.InProgress,
                AssessmentByUserId = actorUserId,

                ChiefComplaint = NormalizeNullableText(request.ChiefComplaint),
                CurrentIllnessHistory = NormalizeNullableText(request.CurrentIllnessHistory),
                MedicationHistory = NormalizeNullableText(request.MedicationHistory),

                BloodPressureSystolic = request.BloodPressureSystolic,
                BloodPressureDiastolic = request.BloodPressureDiastolic,
                PulseRate = request.PulseRate,
                IsPulseReadable = request.IsPulseReadable,
                RespiratoryRate = request.RespiratoryRate,
                Temperature = request.Temperature,
                OxygenSaturation = request.OxygenSaturation,
                IsUsingOxygen = request.IsUsingOxygen,
                OxygenSupportType = request.OxygenSupportType,
                OxygenFlowRate = request.OxygenFlowRate,
                OxygenSupportNote = NormalizeNullableText(request.OxygenSupportNote),
                ConsciousnessStatus = request.ConsciousnessStatus,
                Weight = request.Weight,
                Height = request.Height,
                BMI = calculated.BMI,
                MeanArterialPressure = calculated.MeanArterialPressure,
                MapStatus = calculated.MapStatus,
                EarlyWarningScore = calculated.EarlyWarningScore,
                EwsRiskLevel = calculated.EwsRiskLevel,
                EwsMonitoringRecommendation = calculated.EwsMonitoringRecommendation,

                HasPain = request.HasPain,
                PainScale = request.PainScale,
                PainTrigger = NormalizeNullableText(request.PainTrigger),
                PainQuality = NormalizeNullableText(request.PainQuality),
                PainLocation = NormalizeNullableText(request.PainLocation),
                PainFrequency = NormalizeNullableText(request.PainFrequency),
                PainManagement = NormalizeNullableText(request.PainManagement),
                PainNote = NormalizeNullableText(request.PainNote),

                HasHereditaryDisease = request.HasHereditaryDisease,
                HereditaryDiseaseNote = NormalizeNullableText(request.HereditaryDiseaseNote),

                HasAllergy = request.HasAllergy,
                AllergyType = NormalizeNullableText(request.AllergyType),
                AllergyNote = NormalizeNullableText(request.AllergyNote),

                AppetiteStatus = request.AppetiteStatus,
                HasNausea = request.HasNausea,
                HasVomiting = request.HasVomiting,
                NutritionRiskStatus = request.NutritionRiskStatus,
                NutritionRiskScore = request.NutritionRiskScore,
                NutritionNote = NormalizeNullableText(request.NutritionNote),

                HasFallRisk = request.HasFallRisk || request.HasAtaxia || request.HasPosturalInstability,
                HasAtaxia = request.HasAtaxia,
                HasPosturalInstability = request.HasPosturalInstability,
                FallRiskStatus = calculated.FallRiskStatus,
                FallRiskScore = calculated.FallRiskScore,
                FallRiskNote = NormalizeNullableText(request.FallRiskNote),

                FunctionalStatus = request.FunctionalStatus,
                FunctionalNote = NormalizeNullableText(request.FunctionalNote),
                PsychosocialNote = NormalizeNullableText(request.PsychosocialNote),
                EducationNote = NormalizeNullableText(request.EducationNote),
                NurseNote = NormalizeNullableText(request.NurseNote),

                StartedAt = now,
                CompletedAt = request.CompleteImmediately ? now : null,
                CompletedByUserId = request.CompleteImmediately ? actorUserId : null,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            NormalizeAssessmentData(entity);

            _dbContext.Set<TrxPatientAssessment>().Add(entity);

            queue.QueueStatus = request.CompleteImmediately
                ? QueueStatus.WaitingForDoctor
                : QueueStatus.InNurseScreening;

            queue.ScreeningStartedAt ??= now;

            if (request.CompleteImmediately)
                queue.ScreeningCompletedAt = now;

            queue.UpdateDateTime = now;
            queue.UpdateBy = actorUserId;

            if (queue.Encounter != null)
            {
                queue.Encounter.EncounterStatus = request.CompleteImmediately
                    ? EncounterStatus.WaitingForDoctor
                    : EncounterStatus.InNurseScreening;

                queue.Encounter.UpdateDateTime = now;
                queue.Encounter.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var response = new PatientAssessmentCreateResponse
            {
                Id = entity.Id,
                AssessmentNumber = entity.AssessmentNumber,
                EncounterId = entity.EncounterId,
                QueueId = entity.QueueId,
                AssessmentStatus = entity.AssessmentStatus,
                AssessmentDateTime = entity.AssessmentDateTime,
                CompletedAt = entity.CompletedAt,
                BMI = entity.BMI,
                MeanArterialPressure = entity.MeanArterialPressure,
                MapStatus = entity.MapStatus,
                EarlyWarningScore = entity.EarlyWarningScore,
                EwsRiskLevel = entity.EwsRiskLevel,
                EwsMonitoringRecommendation = entity.EwsMonitoringRecommendation
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientAssessment.CreateAssessment",
                "Membuat assessment pasien.",
                response
            );

            return Ok(ApiResponse<PatientAssessmentCreateResponse>.Ok(
                response,
                "Assessment pasien berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Assessment", Description = "Mengubah assessment pasien", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientAssessment", "Update")]
        public async Task<IActionResult> UpdateAssessment(Guid id, [FromBody] UpdatePatientAssessmentRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientAssessment>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Assessment pasien tidak ditemukan."
                ));
            }

            if (entity.AssessmentStatus == PatientAssessmentStatus.Completed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Assessment yang sudah completed tidak dapat diubah."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var calculated = CalculateAssessmentValues(request);

            entity.ChiefComplaint = NormalizeNullableText(request.ChiefComplaint);
            entity.CurrentIllnessHistory = NormalizeNullableText(request.CurrentIllnessHistory);
            entity.MedicationHistory = NormalizeNullableText(request.MedicationHistory);

            entity.BloodPressureSystolic = request.BloodPressureSystolic;
            entity.BloodPressureDiastolic = request.BloodPressureDiastolic;
            entity.PulseRate = request.PulseRate;
            entity.IsPulseReadable = request.IsPulseReadable;
            entity.RespiratoryRate = request.RespiratoryRate;
            entity.Temperature = request.Temperature;
            entity.OxygenSaturation = request.OxygenSaturation;
            entity.IsUsingOxygen = request.IsUsingOxygen;
            entity.OxygenSupportType = request.OxygenSupportType;
            entity.OxygenFlowRate = request.OxygenFlowRate;
            entity.OxygenSupportNote = NormalizeNullableText(request.OxygenSupportNote);
            entity.ConsciousnessStatus = request.ConsciousnessStatus;
            entity.Weight = request.Weight;
            entity.Height = request.Height;
            entity.BMI = calculated.BMI;
            entity.MeanArterialPressure = calculated.MeanArterialPressure;
            entity.MapStatus = calculated.MapStatus;
            entity.EarlyWarningScore = calculated.EarlyWarningScore;
            entity.EwsRiskLevel = calculated.EwsRiskLevel;
            entity.EwsMonitoringRecommendation = calculated.EwsMonitoringRecommendation;

            entity.HasPain = request.HasPain;
            entity.PainScale = request.PainScale;
            entity.PainTrigger = NormalizeNullableText(request.PainTrigger);
            entity.PainQuality = NormalizeNullableText(request.PainQuality);
            entity.PainLocation = NormalizeNullableText(request.PainLocation);
            entity.PainFrequency = NormalizeNullableText(request.PainFrequency);
            entity.PainManagement = NormalizeNullableText(request.PainManagement);
            entity.PainNote = NormalizeNullableText(request.PainNote);

            entity.HasHereditaryDisease = request.HasHereditaryDisease;
            entity.HereditaryDiseaseNote = NormalizeNullableText(request.HereditaryDiseaseNote);

            entity.HasAllergy = request.HasAllergy;
            entity.AllergyType = NormalizeNullableText(request.AllergyType);
            entity.AllergyNote = NormalizeNullableText(request.AllergyNote);

            entity.AppetiteStatus = request.AppetiteStatus;
            entity.HasNausea = request.HasNausea;
            entity.HasVomiting = request.HasVomiting;
            entity.NutritionRiskStatus = request.NutritionRiskStatus;
            entity.NutritionRiskScore = request.NutritionRiskScore;
            entity.NutritionNote = NormalizeNullableText(request.NutritionNote);

            entity.HasFallRisk = request.HasFallRisk || request.HasAtaxia || request.HasPosturalInstability;
            entity.HasAtaxia = request.HasAtaxia;
            entity.HasPosturalInstability = request.HasPosturalInstability;
            entity.FallRiskStatus = calculated.FallRiskStatus;
            entity.FallRiskScore = calculated.FallRiskScore;
            entity.FallRiskNote = NormalizeNullableText(request.FallRiskNote);

            entity.FunctionalStatus = request.FunctionalStatus;
            entity.FunctionalNote = NormalizeNullableText(request.FunctionalNote);
            entity.PsychosocialNote = NormalizeNullableText(request.PsychosocialNote);
            entity.EducationNote = NormalizeNullableText(request.EducationNote);
            entity.NurseNote = NormalizeNullableText(request.NurseNote);

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            NormalizeAssessmentData(entity);

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Assessment pasien berhasil diubah."
            ));
        }

        [HttpPatch("{id:guid}/complete")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Complete Patient Assessment", Description = "Menyelesaikan assessment pasien", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PatientAssessment", "Update")]
        public async Task<IActionResult> CompleteAssessment(Guid id, [FromBody] CompletePatientAssessmentRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientAssessment>()
                .Include(x => x.Queue)
                .Include(x => x.Encounter)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Assessment pasien tidak ditemukan."
                ));
            }

            if (entity.AssessmentStatus == PatientAssessmentStatus.Completed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Assessment pasien sudah completed."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.AssessmentStatus = PatientAssessmentStatus.Completed;
            entity.CompletedAt = now;
            entity.CompletedByUserId = actorUserId;
            entity.NurseNote = NormalizeNullableText(request.NurseNote) ?? entity.NurseNote;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (entity.Queue != null)
            {
                entity.Queue.QueueStatus = entity.Queue.IsDoctorRequired
                    ? QueueStatus.WaitingForDoctor
                    : QueueStatus.Completed;

                entity.Queue.ScreeningCompletedAt = now;
                entity.Queue.UpdateDateTime = now;
                entity.Queue.UpdateBy = actorUserId;
            }

            if (entity.Encounter != null)
            {
                entity.Encounter.EncounterStatus = entity.Queue != null && entity.Queue.IsDoctorRequired
                    ? EncounterStatus.WaitingForDoctor
                    : EncounterStatus.Completed;

                entity.Encounter.UpdateDateTime = now;
                entity.Encounter.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Assessment pasien berhasil diselesaikan."
            ));
        }

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Cancel Patient Assessment", Description = "Membatalkan assessment pasien", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("PatientAssessment", "Update")]
        public async Task<IActionResult> CancelAssessment(Guid id, [FromBody] CancelPatientAssessmentRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientAssessment>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Assessment pasien tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.AssessmentStatus = PatientAssessmentStatus.Cancelled;
            entity.CancelledAt = now;
            entity.CancelledByUserId = actorUserId;
            entity.CancelReason = request.CancelReason.Trim();
            entity.IsCancel = true;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Assessment pasien berhasil dibatalkan."
            ));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(
            CreatePatientAssessmentRequest request)
        {
            var queue = await _dbContext.Set<TrxQueue>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.QueueId &&
                    x.EncounterId == request.EncounterId &&
                    !x.IsDelete);

            if (queue == null)
                return (false, "Antrean tidak ditemukan atau tidak sesuai dengan encounter.");

            if (!queue.IsScreeningRequired)
                return (false, "Antrean ini tidak membutuhkan screening.");

            if (queue.QueueStatus != QueueStatus.CalledByNurse &&
                queue.QueueStatus != QueueStatus.InNurseScreening &&
                queue.QueueStatus != QueueStatus.WaitingForNurse)
            {
                return (false, "Status antrean tidak valid untuk assessment.");
            }

            var assessmentExists = await _dbContext.Set<TrxPatientAssessment>()
                .AnyAsync(x =>
                    x.EncounterId == request.EncounterId &&
                    !x.IsDelete);

            if (assessmentExists)
                return (false, "Assessment untuk encounter ini sudah ada.");

            return (true, null);
        }

        private async Task<string> GenerateAssessmentNumberAsync(DateTime now)
        {
            var prefix = $"ASM-{now:yyyyMMdd}";
            var countToday = await _dbContext.Set<TrxPatientAssessment>()
                .CountAsync(x => x.AssessmentNumber.StartsWith(prefix));

            return $"{prefix}-{countToday + 1:D5}";
        }

        private static CalculatedAssessmentValue CalculateAssessmentValues(CreatePatientAssessmentRequest request)
        {
            var bmi = CalculateBmi(request.Weight, request.Height);
            var map = CalculateMap(request.BloodPressureSystolic, request.BloodPressureDiastolic);
            var mapStatus = CalculateMapStatus(map);
            var ewsScore = CalculateEwsScore(
                request.RespiratoryRate,
                request.OxygenSaturation,
                request.Temperature,
                request.BloodPressureSystolic,
                request.PulseRate,
                request.ConsciousnessStatus);

            var ewsRiskLevel = CalculateEwsRiskLevel(ewsScore);
            var ewsMonitoringRecommendation = GetEwsMonitoringRecommendation(ewsRiskLevel, ewsScore);

            var hasFallRisk = request.HasFallRisk || request.HasAtaxia || request.HasPosturalInstability;
            var fallRiskScore = CalculateFallRiskScore(hasFallRisk, request.HasAtaxia, request.HasPosturalInstability);
            var fallRiskStatus = CalculateFallRiskStatus(hasFallRisk, fallRiskScore);

            return new CalculatedAssessmentValue
            {
                BMI = bmi,
                MeanArterialPressure = map,
                MapStatus = mapStatus,
                EarlyWarningScore = ewsScore,
                EwsRiskLevel = ewsRiskLevel,
                EwsMonitoringRecommendation = ewsMonitoringRecommendation,
                FallRiskScore = fallRiskScore,
                FallRiskStatus = fallRiskStatus
            };
        }

        private static CalculatedAssessmentValue CalculateAssessmentValues(UpdatePatientAssessmentRequest request)
        {
            var bmi = CalculateBmi(request.Weight, request.Height);
            var map = CalculateMap(request.BloodPressureSystolic, request.BloodPressureDiastolic);
            var mapStatus = CalculateMapStatus(map);
            var ewsScore = CalculateEwsScore(
                request.RespiratoryRate,
                request.OxygenSaturation,
                request.Temperature,
                request.BloodPressureSystolic,
                request.PulseRate,
                request.ConsciousnessStatus);

            var ewsRiskLevel = CalculateEwsRiskLevel(ewsScore);
            var ewsMonitoringRecommendation = GetEwsMonitoringRecommendation(ewsRiskLevel, ewsScore);

            var hasFallRisk = request.HasFallRisk || request.HasAtaxia || request.HasPosturalInstability;
            var fallRiskScore = CalculateFallRiskScore(hasFallRisk, request.HasAtaxia, request.HasPosturalInstability);
            var fallRiskStatus = CalculateFallRiskStatus(hasFallRisk, fallRiskScore);

            return new CalculatedAssessmentValue
            {
                BMI = bmi,
                MeanArterialPressure = map,
                MapStatus = mapStatus,
                EarlyWarningScore = ewsScore,
                EwsRiskLevel = ewsRiskLevel,
                EwsMonitoringRecommendation = ewsMonitoringRecommendation,
                FallRiskScore = fallRiskScore,
                FallRiskStatus = fallRiskStatus
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

        private static decimal? CalculateMap(int? systolic, int? diastolic)
        {
            if (!systolic.HasValue || !diastolic.HasValue)
                return null;

            var map = diastolic.Value + ((systolic.Value - diastolic.Value) / 3m);

            return Math.Round(map, 2);
        }

        private static MapStatus CalculateMapStatus(decimal? map)
        {
            if (!map.HasValue)
                return MapStatus.Unknown;

            if (map.Value < 60)
                return MapStatus.Hypotension;

            if (map.Value > 100)
                return MapStatus.Hypertension;

            return MapStatus.Normal;
        }

        private static int? CalculateEwsScore(
            int? respiratoryRate,
            decimal? oxygenSaturation,
            decimal? temperature,
            int? systolicBloodPressure,
            int? pulseRate,
            ConsciousnessStatus consciousnessStatus)
        {
            var hasAnyValue =
                respiratoryRate.HasValue ||
                oxygenSaturation.HasValue ||
                temperature.HasValue ||
                systolicBloodPressure.HasValue ||
                pulseRate.HasValue ||
                consciousnessStatus != ConsciousnessStatus.Unknown;

            if (!hasAnyValue)
                return null;

            var score = 0;

            if (respiratoryRate.HasValue)
            {
                if (respiratoryRate.Value <= 8) score += 3;
                else if (respiratoryRate.Value <= 11) score += 1;
                else if (respiratoryRate.Value <= 20) score += 0;
                else if (respiratoryRate.Value <= 24) score += 2;
                else score += 3;
            }

            if (oxygenSaturation.HasValue)
            {
                if (oxygenSaturation.Value <= 91) score += 3;
                else if (oxygenSaturation.Value <= 93) score += 2;
                else if (oxygenSaturation.Value <= 95) score += 1;
                else score += 0;
            }

            if (temperature.HasValue)
            {
                if (temperature.Value <= 35.0m) score += 3;
                else if (temperature.Value <= 36.0m) score += 1;
                else if (temperature.Value <= 38.0m) score += 0;
                else if (temperature.Value <= 39.0m) score += 1;
                else score += 2;
            }

            if (systolicBloodPressure.HasValue)
            {
                if (systolicBloodPressure.Value <= 90) score += 3;
                else if (systolicBloodPressure.Value <= 100) score += 2;
                else if (systolicBloodPressure.Value <= 110) score += 1;
                else if (systolicBloodPressure.Value <= 219) score += 0;
                else score += 3;
            }

            if (pulseRate.HasValue)
            {
                if (pulseRate.Value <= 40) score += 3;
                else if (pulseRate.Value <= 50) score += 1;
                else if (pulseRate.Value <= 90) score += 0;
                else if (pulseRate.Value <= 110) score += 1;
                else if (pulseRate.Value <= 130) score += 2;
                else score += 3;
            }

            if (consciousnessStatus != ConsciousnessStatus.Unknown &&
                consciousnessStatus != ConsciousnessStatus.ComposMentis)
            {
                score += 3;
            }

            return score;
        }

        private static EwsRiskLevel CalculateEwsRiskLevel(int? ewsScore)
        {
            if (!ewsScore.HasValue)
                return EwsRiskLevel.Unknown;

            if (ewsScore.Value >= 7)
                return EwsRiskLevel.Critical;

            if (ewsScore.Value >= 5)
                return EwsRiskLevel.High;

            if (ewsScore.Value >= 3)
                return EwsRiskLevel.Medium;

            return EwsRiskLevel.Low;
        }

        private static string? GetEwsMonitoringRecommendation(EwsRiskLevel riskLevel, int? ewsScore)
        {
            if (!ewsScore.HasValue)
                return null;

            return riskLevel switch
            {
                EwsRiskLevel.Low => "Monitoring rutin sesuai kondisi klinis pasien.",
                EwsRiskLevel.Medium => "Monitoring ulang tanda vital dan evaluasi klinis berkala.",
                EwsRiskLevel.High => "Monitoring lebih sering dan informasikan dokter penanggung jawab.",
                EwsRiskLevel.Critical => "Pemantauan terus menerus tanda-tanda vital, pertimbangkan eskalasi klinis segera.",
                _ => null
            };
        }

        private static int? CalculateFallRiskScore(bool hasFallRisk, bool hasAtaxia, bool hasPosturalInstability)
        {
            if (!hasFallRisk)
                return null;

            var score = 0;

            if (hasAtaxia)
                score += 1;

            if (hasPosturalInstability)
                score += 1;

            return score;
        }

        private static FallRiskStatus CalculateFallRiskStatus(bool hasFallRisk, int? fallRiskScore)
        {
            if (!hasFallRisk)
                return FallRiskStatus.NoRisk;

            if (!fallRiskScore.HasValue)
                return FallRiskStatus.Unknown;

            if (fallRiskScore.Value >= 2)
                return FallRiskStatus.HighRisk;

            if (fallRiskScore.Value == 1)
                return FallRiskStatus.MediumRisk;

            return FallRiskStatus.LowRisk;
        }

        private static void NormalizeAssessmentData(TrxPatientAssessment entity)
        {
            if (!entity.IsUsingOxygen)
            {
                entity.OxygenSupportType = OxygenSupportType.None;
                entity.OxygenFlowRate = null;
                entity.OxygenSupportNote = null;
            }

            if (!entity.HasPain)
            {
                entity.PainScale = null;
                entity.PainTrigger = null;
                entity.PainQuality = null;
                entity.PainLocation = null;
                entity.PainFrequency = null;
                entity.PainManagement = null;
                entity.PainNote = null;
            }

            if (!entity.HasHereditaryDisease)
            {
                entity.HereditaryDiseaseNote = null;
            }

            if (!entity.HasAllergy)
            {
                entity.AllergyType = null;
                entity.AllergyNote = null;
            }

            if (!entity.HasFallRisk)
            {
                entity.HasAtaxia = false;
                entity.HasPosturalInstability = false;
                entity.FallRiskScore = null;
                entity.FallRiskStatus = FallRiskStatus.NoRisk;
                entity.FallRiskNote = null;
            }
        }

        private static IQueryable<TrxPatientAssessment> ApplySorting(
            IQueryable<TrxPatientAssessment> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "assessmentDateTime").ToLowerInvariant() switch
            {
                "assessmentnumber" => isDesc ? query.OrderByDescending(x => x.AssessmentNumber) : query.OrderBy(x => x.AssessmentNumber),
                "assessmentstatus" => isDesc ? query.OrderByDescending(x => x.AssessmentStatus) : query.OrderBy(x => x.AssessmentStatus),
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDesc ? query.OrderByDescending(x => x.AssessmentDateTime) : query.OrderBy(x => x.AssessmentDateTime)
            };
        }

        private static PatientAssessmentResponse ToResponse(TrxPatientAssessment x)
        {
            return new PatientAssessmentResponse
            {
                Id = x.Id,
                AssessmentNumber = x.AssessmentNumber,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : string.Empty,
                QueueId = x.QueueId,
                QueueCode = x.Queue != null ? x.Queue.QueueCode : string.Empty,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                DoctorId = x.DoctorId,
                DoctorName = x.Queue != null && x.Queue.Doctor != null ? x.Queue.Doctor.FullName : null,
                AssessmentDateTime = x.AssessmentDateTime,
                AssessmentStatus = x.AssessmentStatus,
                AssessmentByUserId = x.AssessmentByUserId,
                AssessmentByUserName = x.AssessmentByUser != null ? x.AssessmentByUser.DisplayName : null,
                ChiefComplaint = x.ChiefComplaint,

                BloodPressureSystolic = x.BloodPressureSystolic,
                BloodPressureDiastolic = x.BloodPressureDiastolic,
                PulseRate = x.PulseRate,
                IsPulseReadable = x.IsPulseReadable,
                RespiratoryRate = x.RespiratoryRate,
                Temperature = x.Temperature,
                OxygenSaturation = x.OxygenSaturation,
                IsUsingOxygen = x.IsUsingOxygen,
                OxygenSupportType = x.OxygenSupportType,
                OxygenFlowRate = x.OxygenFlowRate,
                ConsciousnessStatus = x.ConsciousnessStatus,
                Weight = x.Weight,
                Height = x.Height,
                BMI = x.BMI,
                MeanArterialPressure = x.MeanArterialPressure,
                MapStatus = x.MapStatus,
                EarlyWarningScore = x.EarlyWarningScore,
                EwsRiskLevel = x.EwsRiskLevel,

                HasPain = x.HasPain,
                PainScale = x.PainScale,
                HasHereditaryDisease = x.HasHereditaryDisease,
                HasAllergy = x.HasAllergy,
                AllergyType = x.AllergyType,

                AppetiteStatus = x.AppetiteStatus,
                HasNausea = x.HasNausea,
                HasVomiting = x.HasVomiting,

                HasFallRisk = x.HasFallRisk,
                FallRiskStatus = x.FallRiskStatus,
                NutritionRiskStatus = x.NutritionRiskStatus,
                FunctionalStatus = x.FunctionalStatus,

                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static PatientAssessmentDetailResponse ToDetailResponse(TrxPatientAssessment x)
        {
            return new PatientAssessmentDetailResponse
            {
                Id = x.Id,
                AssessmentNumber = x.AssessmentNumber,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : string.Empty,
                QueueId = x.QueueId,
                QueueCode = x.Queue != null ? x.Queue.QueueCode : string.Empty,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                DoctorId = x.DoctorId,
                DoctorName = x.Queue != null && x.Queue.Doctor != null ? x.Queue.Doctor.FullName : null,
                AssessmentDateTime = x.AssessmentDateTime,
                AssessmentStatus = x.AssessmentStatus,
                AssessmentByUserId = x.AssessmentByUserId,
                AssessmentByUserName = x.AssessmentByUser != null ? x.AssessmentByUser.DisplayName : null,
                ChiefComplaint = x.ChiefComplaint,
                CurrentIllnessHistory = x.CurrentIllnessHistory,
                MedicationHistory = x.MedicationHistory,

                BloodPressureSystolic = x.BloodPressureSystolic,
                BloodPressureDiastolic = x.BloodPressureDiastolic,
                PulseRate = x.PulseRate,
                IsPulseReadable = x.IsPulseReadable,
                RespiratoryRate = x.RespiratoryRate,
                Temperature = x.Temperature,
                OxygenSaturation = x.OxygenSaturation,
                IsUsingOxygen = x.IsUsingOxygen,
                OxygenSupportType = x.OxygenSupportType,
                OxygenFlowRate = x.OxygenFlowRate,
                OxygenSupportNote = x.OxygenSupportNote,
                ConsciousnessStatus = x.ConsciousnessStatus,
                Weight = x.Weight,
                Height = x.Height,
                BMI = x.BMI,
                MeanArterialPressure = x.MeanArterialPressure,
                MapStatus = x.MapStatus,
                EarlyWarningScore = x.EarlyWarningScore,
                EwsRiskLevel = x.EwsRiskLevel,
                EwsMonitoringRecommendation = x.EwsMonitoringRecommendation,

                HasPain = x.HasPain,
                PainScale = x.PainScale,
                PainTrigger = x.PainTrigger,
                PainQuality = x.PainQuality,
                PainLocation = x.PainLocation,
                PainFrequency = x.PainFrequency,
                PainManagement = x.PainManagement,
                PainNote = x.PainNote,

                HasHereditaryDisease = x.HasHereditaryDisease,
                HereditaryDiseaseNote = x.HereditaryDiseaseNote,

                HasAllergy = x.HasAllergy,
                AllergyType = x.AllergyType,
                AllergyNote = x.AllergyNote,

                AppetiteStatus = x.AppetiteStatus,
                HasNausea = x.HasNausea,
                HasVomiting = x.HasVomiting,
                NutritionRiskStatus = x.NutritionRiskStatus,
                NutritionRiskScore = x.NutritionRiskScore,
                NutritionNote = x.NutritionNote,

                HasFallRisk = x.HasFallRisk,
                HasAtaxia = x.HasAtaxia,
                HasPosturalInstability = x.HasPosturalInstability,
                FallRiskStatus = x.FallRiskStatus,
                FallRiskScore = x.FallRiskScore,
                FallRiskNote = x.FallRiskNote,

                FunctionalStatus = x.FunctionalStatus,
                FunctionalNote = x.FunctionalNote,
                PsychosocialNote = x.PsychosocialNote,
                EducationNote = x.EducationNote,
                NurseNote = x.NurseNote,

                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt,
                CompletedByUserId = x.CompletedByUserId,
                CompletedByUserName = x.CompletedByUser != null ? x.CompletedByUser.DisplayName : null,
                CancelledAt = x.CancelledAt,
                CancelledByUserId = x.CancelledByUserId,
                CancelledByUserName = x.CancelledByUser != null ? x.CancelledByUser.DisplayName : null,
                CancelReason = x.CancelReason,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
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

        private class CalculatedAssessmentValue
        {
            public decimal? BMI { get; set; }
            public decimal? MeanArterialPressure { get; set; }
            public MapStatus MapStatus { get; set; }
            public int? EarlyWarningScore { get; set; }
            public EwsRiskLevel EwsRiskLevel { get; set; }
            public string? EwsMonitoringRecommendation { get; set; }
            public int? FallRiskScore { get; set; }
            public FallRiskStatus FallRiskStatus { get; set; }
        }
    }
}