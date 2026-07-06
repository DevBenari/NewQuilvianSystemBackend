using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponsePatientMedicalHistoryPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs.PatientMedicalHistoryResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/patient-medical-histories")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Patient Medical History",
        AreaName = "HealthServices",
        ControllerName = "PatientMedicalHistory",
        Description = "Riwayat penyakit dahulu pasien dan clinical risk alert",
        SortOrder = 6
    )]
    [Tags("Health Services / Clinical Management / Patient Medical History")]
    public class PatientMedicalHistoryController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Clinical";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PatientMedicalHistoryController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientMedicalHistoryFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Medical History", Description = "Melihat metadata filter riwayat penyakit pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientMedicalHistory", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientMedicalHistoryFilterMetadataResponse
            {
                DefaultFilter = new PatientMedicalHistoryDefaultFilterResponse(),
                SortOptions = new List<PatientMedicalHistorySortOptionResponse>
                {
                    new() { Value = "recordedDateTime", Label = "Tanggal pencatatan" },
                    new() { Value = "conditionName", Label = "Nama kondisi" },
                    new() { Value = "conditionCode", Label = "Kode kondisi" },
                    new() { Value = "historyType", Label = "Tipe riwayat" },
                    new() { Value = "historyStatus", Label = "Status riwayat" },
                    new() { Value = "severity", Label = "Tingkat keparahan" },
                    new() { Value = "certainty", Label = "Kepastian" },
                    new() { Value = "isCurrentProblem", Label = "Masalah aktif" },
                    new() { Value = "isChronic", Label = "Kronis" },
                    new() { Value = "isComorbidity", Label = "Komorbid" },
                    new() { Value = "isHighRisk", Label = "Risiko tinggi" },
                    new() { Value = "isVerified", Label = "Terverifikasi" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                HistoryTypeOptions = BuildEnumOptions<PatientMedicalHistoryType>(),
                HistoryStatusOptions = BuildEnumOptions<PatientMedicalHistoryStatus>(),
                SeverityOptions = BuildEnumOptions<PatientMedicalHistorySeverity>(),
                CertaintyOptions = BuildEnumOptions<PatientMedicalHistoryCertainty>()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientMedicalHistory.GetFilterMetadata",
                "Mengambil metadata filter riwayat penyakit pasien.",
                result
            );

            return Ok(ApiResponse<PatientMedicalHistoryFilterMetadataResponse>.Ok(
                result,
                "Metadata filter riwayat penyakit pasien berhasil diambil."
            ));
        }

        [HttpGet("active-alerts")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientMedicalHistoryAlertResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Medical History", Description = "Melihat alert riwayat penyakit aktif pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientMedicalHistory", "Read")]
        public async Task<IActionResult> GetActiveAlerts([FromQuery] Guid patientId)
        {
            if (patientId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "PatientId wajib diisi."
                ));
            }

            var data = await _dbContext.Set<TrxPatientMedicalHistory>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.PatientId == patientId &&
                    x.IsActive &&
                    x.IsAlertEnabled &&
                    x.HistoryStatus != PatientMedicalHistoryStatus.Inactive &&
                    x.HistoryStatus != PatientMedicalHistoryStatus.Resolved &&
                    x.HistoryStatus != PatientMedicalHistoryStatus.EnteredInError &&
                    x.HistoryStatus != PatientMedicalHistoryStatus.Cancelled)
                .OrderByDescending(x => x.IsHighRisk)
                .ThenByDescending(x => x.IsComorbidity)
                .ThenByDescending(x => x.IsChronic)
                .ThenByDescending(x => x.Severity)
                .ThenBy(x => x.ConditionName)
                .Select(x => new PatientMedicalHistoryAlertResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    MedicalHistoryRecordNumber = x.MedicalHistoryRecordNumber,
                    DiagnosisId = x.DiagnosisId,
                    HistoryType = x.HistoryType,
                    HistoryStatus = x.HistoryStatus,
                    Severity = x.Severity,
                    ConditionCode = x.ConditionCode,
                    ConditionName = x.ConditionName,
                    ConditionGroupName = x.ConditionGroupName,
                    IsCurrentProblem = x.IsCurrentProblem,
                    IsChronic = x.IsChronic,
                    IsComorbidity = x.IsComorbidity,
                    IsInfectiousDisease = x.IsInfectiousDisease,
                    IsMentalHealthRelated = x.IsMentalHealthRelated,
                    IsPregnancyRelated = x.IsPregnancyRelated,
                    IsHighRisk = x.IsHighRisk,
                    RiskNote = x.RiskNote,
                    ClinicalNote = x.ClinicalNote
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientMedicalHistoryAlertResponse>>.Ok(
                data,
                "Alert riwayat penyakit aktif pasien berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientMedicalHistoryPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Medical History", Description = "Melihat data riwayat penyakit pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientMedicalHistory", "Read")]
        public async Task<IActionResult> GetMedicalHistories(
            [FromQuery] string? search,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? consultationId,
            [FromQuery] Guid? assessmentId,
            [FromQuery] Guid? doctorId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? diagnosisId,
            [FromQuery] PatientMedicalHistoryType? historyType,
            [FromQuery] PatientMedicalHistoryStatus? historyStatus,
            [FromQuery] PatientMedicalHistorySeverity? severity,
            [FromQuery] PatientMedicalHistoryCertainty? certainty,
            [FromQuery] bool? isFromMasterDiagnosis,
            [FromQuery] bool? isCurrentProblem,
            [FromQuery] bool? isChronic,
            [FromQuery] bool? isComorbidity,
            [FromQuery] bool? isUnderTreatment,
            [FromQuery] bool? isControlled,
            [FromQuery] bool? isInfectiousDisease,
            [FromQuery] bool? isHereditaryRelated,
            [FromQuery] bool? isMentalHealthRelated,
            [FromQuery] bool? isPregnancyRelated,
            [FromQuery] bool? isSurgicalHistory,
            [FromQuery] bool? isHospitalizationHistory,
            [FromQuery] bool? isHighRisk,
            [FromQuery] bool? isAlertEnabled,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isActive,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy = "recordedDateTime",
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
                patientId,
                encounterId,
                consultationId,
                assessmentId,
                doctorId,
                serviceUnitId,
                clinicId,
                diagnosisId,
                historyType,
                historyStatus,
                severity,
                certainty,
                isFromMasterDiagnosis,
                isCurrentProblem,
                isChronic,
                isComorbidity,
                isUnderTreatment,
                isControlled,
                isInfectiousDisease,
                isHereditaryRelated,
                isMentalHealthRelated,
                isPregnancyRelated,
                isSurgicalHistory,
                isHospitalizationHistory,
                isHighRisk,
                isAlertEnabled,
                isVerified,
                isActive,
                startDate,
                endDate
            );

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(ToResponse).ToList();

            var result = new ResponsePatientMedicalHistoryPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePatientMedicalHistoryPagedResult>.Ok(
                result,
                "Data riwayat penyakit pasien berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientMedicalHistoryOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Medical History", Description = "Melihat pilihan riwayat penyakit pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientMedicalHistory", "Read")]
        public async Task<IActionResult> GetMedicalHistoryOptions(
            [FromQuery] Guid? patientId,
            [FromQuery] PatientMedicalHistoryType? historyType,
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool onlyCurrentProblem = false,
            [FromQuery] bool onlyAlertEnabled = false,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<TrxPatientMedicalHistory>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
            {
                query = query.Where(x =>
                    x.IsActive &&
                    x.HistoryStatus != PatientMedicalHistoryStatus.Cancelled &&
                    x.HistoryStatus != PatientMedicalHistoryStatus.EnteredInError);
            }

            if (onlyCurrentProblem)
                query = query.Where(x => x.IsCurrentProblem);

            if (onlyAlertEnabled)
                query = query.Where(x => x.IsAlertEnabled);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (historyType.HasValue)
                query = query.Where(x => x.HistoryType == historyType.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ConditionName.ToLower().Contains(keyword) ||
                    (x.ConditionCode != null && x.ConditionCode.ToLower().Contains(keyword)) ||
                    (x.ConditionGroupName != null && x.ConditionGroupName.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderByDescending(x => x.IsCurrentProblem)
                .ThenByDescending(x => x.IsHighRisk)
                .ThenByDescending(x => x.IsComorbidity)
                .ThenBy(x => x.ConditionName)
                .Take(100)
                .Select(x => new PatientMedicalHistoryOptionResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    MedicalHistoryRecordNumber = x.MedicalHistoryRecordNumber,
                    DiagnosisId = x.DiagnosisId,
                    HistoryType = x.HistoryType,
                    HistoryStatus = x.HistoryStatus,
                    Severity = x.Severity,
                    Certainty = x.Certainty,
                    ConditionCode = x.ConditionCode,
                    ConditionName = x.ConditionName,
                    ConditionGroupName = x.ConditionGroupName,
                    IsCurrentProblem = x.IsCurrentProblem,
                    IsChronic = x.IsChronic,
                    IsComorbidity = x.IsComorbidity,
                    IsHighRisk = x.IsHighRisk,
                    IsAlertEnabled = x.IsAlertEnabled,
                    IsVerified = x.IsVerified
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientMedicalHistoryOptionResponse>>.Ok(
                data,
                "Data pilihan riwayat penyakit pasien berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientMedicalHistoryDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Medical History", Description = "Melihat detail riwayat penyakit pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientMedicalHistory", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Riwayat penyakit pasien tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientMedicalHistoryDetailResponse>.Ok(
                ToDetailResponse(entity),
                "Detail riwayat penyakit pasien berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientMedicalHistoryCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Patient Medical History", Description = "Membuat data riwayat penyakit pasien", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PatientMedicalHistory", "Create")]
        public async Task<IActionResult> CreateMedicalHistory([FromBody] CreatePatientMedicalHistoryRequest request)
        {
            var validation = await ValidateCreateRequestAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data riwayat penyakit pasien tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var context = await ResolveClinicalContextAsync(
                request.PatientId,
                request.EncounterId,
                request.ConsultationId,
                request.AssessmentId,
                request.DoctorId,
                request.ServiceUnitId,
                request.ClinicId
            );

            if (!context.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    context.ErrorMessage ?? "Konteks klinis tidak valid."
                ));
            }

            var historySnapshot = await BuildMedicalHistorySnapshotAsync(
                request.DiagnosisId,
                request.ConditionCode,
                request.ConditionName,
                request.ConditionGroupName,
                request.ConditionMasterType,
                request.IcdVersion
            );

            if (!historySnapshot.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    historySnapshot.ErrorMessage ?? "Riwayat penyakit tidak valid."
                ));
            }

            var duplicateValidation = await ValidateDuplicateMedicalHistoryAsync(
                patientId: request.PatientId,
                diagnosisId: historySnapshot.DiagnosisId,
                historyType: request.HistoryType,
                conditionName: historySnapshot.ConditionName,
                excludeId: null
            );

            if (!duplicateValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    duplicateValidation.ErrorMessage ?? "Data riwayat penyakit pasien duplikat."
                ));
            }

            var entity = new TrxPatientMedicalHistory
            {
                Id = Guid.NewGuid(),
                MedicalHistoryRecordNumber = await GenerateMedicalHistoryRecordNumberAsync(now),
                PatientId = request.PatientId,
                EncounterId = context.EncounterId,
                ConsultationId = context.ConsultationId,
                AssessmentId = context.AssessmentId,
                DoctorId = context.DoctorId,
                ServiceUnitId = context.ServiceUnitId,
                ClinicId = context.ClinicId,
                DiagnosisId = historySnapshot.DiagnosisId,

                HistoryType = request.HistoryType,
                HistoryStatus = request.HistoryStatus,
                Severity = request.Severity,
                Certainty = request.Certainty,
                ConditionCode = historySnapshot.ConditionCode,
                ConditionName = historySnapshot.ConditionName,
                ConditionGroupName = historySnapshot.ConditionGroupName,
                ConditionMasterType = historySnapshot.ConditionMasterType,
                IcdVersion = historySnapshot.IcdVersion,
                IsFromMasterDiagnosis = historySnapshot.IsFromMasterDiagnosis,

                IsCurrentProblem = request.IsCurrentProblem,
                IsChronic = request.IsChronic || historySnapshot.IsChronicDisease,
                IsComorbidity = request.IsComorbidity,
                IsUnderTreatment = request.IsUnderTreatment,
                IsControlled = request.IsControlled,
                IsInfectiousDisease = request.IsInfectiousDisease || historySnapshot.IsInfectiousDisease,
                IsHereditaryRelated = request.IsHereditaryRelated,
                IsMentalHealthRelated = request.IsMentalHealthRelated || historySnapshot.IsMentalHealthRelated,
                IsPregnancyRelated = request.IsPregnancyRelated || historySnapshot.IsPregnancyRelated,
                IsSurgicalHistory = request.IsSurgicalHistory || request.HistoryType == PatientMedicalHistoryType.Surgery,
                IsHospitalizationHistory = request.IsHospitalizationHistory || request.HistoryType == PatientMedicalHistoryType.Hospitalization,
                IsHighRisk = request.IsHighRisk,
                IsAlertEnabled = request.IsAlertEnabled,

                RecordedDateTime = request.RecordedDateTime ?? now,
                OnsetDate = request.OnsetDate,
                OnsetAgeYear = request.OnsetAgeYear,
                DiagnosedDate = request.DiagnosedDate,
                LastTreatmentDate = request.LastTreatmentDate,
                LastControlDate = request.LastControlDate,
                SourceOfInformation = NormalizeNullableText(request.SourceOfInformation),
                TreatmentHistory = NormalizeNullableText(request.TreatmentHistory),
                MedicationHistory = NormalizeNullableText(request.MedicationHistory),
                SurgeryHistory = NormalizeNullableText(request.SurgeryHistory),
                HospitalizationHistory = NormalizeNullableText(request.HospitalizationHistory),
                ComplicationNote = NormalizeNullableText(request.ComplicationNote),
                ClinicalNote = NormalizeNullableText(request.ClinicalNote),
                RiskNote = NormalizeNullableText(request.RiskNote),
                Notes = NormalizeNullableText(request.Notes),

                IsVerified = request.IsVerified,
                VerifiedAt = request.IsVerified ? now : null,
                VerifiedByUserId = request.IsVerified ? actorUserId : null,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            NormalizeMedicalHistoryData(entity);

            _dbContext.Set<TrxPatientMedicalHistory>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = ToCreateUpdateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientMedicalHistory.CreateMedicalHistory",
                "Membuat data riwayat penyakit pasien.",
                response
            );

            return Ok(ApiResponse<PatientMedicalHistoryCreateResponse>.Ok(
                response,
                "Riwayat penyakit pasien berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientMedicalHistoryUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Medical History", Description = "Mengubah data riwayat penyakit pasien", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientMedicalHistory", "Update")]
        public async Task<IActionResult> UpdateMedicalHistory(Guid id, [FromBody] UpdatePatientMedicalHistoryRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientMedicalHistory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Riwayat penyakit pasien tidak ditemukan."
                ));
            }

            if (entity.HistoryStatus == PatientMedicalHistoryStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Riwayat penyakit yang sudah cancelled tidak dapat diubah."
                ));
            }

            var validation = await ValidateUpdateRequestAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data riwayat penyakit pasien tidak valid."
                ));
            }

            var historySnapshot = await BuildMedicalHistorySnapshotAsync(
                request.DiagnosisId,
                request.ConditionCode,
                request.ConditionName,
                request.ConditionGroupName,
                request.ConditionMasterType,
                request.IcdVersion
            );

            if (!historySnapshot.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    historySnapshot.ErrorMessage ?? "Riwayat penyakit tidak valid."
                ));
            }

            var duplicateValidation = await ValidateDuplicateMedicalHistoryAsync(
                patientId: entity.PatientId,
                diagnosisId: historySnapshot.DiagnosisId,
                historyType: request.HistoryType,
                conditionName: historySnapshot.ConditionName,
                excludeId: id
            );

            if (!duplicateValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    duplicateValidation.ErrorMessage ?? "Data riwayat penyakit pasien duplikat."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.DiagnosisId = historySnapshot.DiagnosisId;
            entity.HistoryType = request.HistoryType;
            entity.HistoryStatus = request.HistoryStatus;
            entity.Severity = request.Severity;
            entity.Certainty = request.Certainty;
            entity.ConditionCode = historySnapshot.ConditionCode;
            entity.ConditionName = historySnapshot.ConditionName;
            entity.ConditionGroupName = historySnapshot.ConditionGroupName;
            entity.ConditionMasterType = historySnapshot.ConditionMasterType;
            entity.IcdVersion = historySnapshot.IcdVersion;
            entity.IsFromMasterDiagnosis = historySnapshot.IsFromMasterDiagnosis;

            entity.IsCurrentProblem = request.IsCurrentProblem;
            entity.IsChronic = request.IsChronic || historySnapshot.IsChronicDisease;
            entity.IsComorbidity = request.IsComorbidity;
            entity.IsUnderTreatment = request.IsUnderTreatment;
            entity.IsControlled = request.IsControlled;
            entity.IsInfectiousDisease = request.IsInfectiousDisease || historySnapshot.IsInfectiousDisease;
            entity.IsHereditaryRelated = request.IsHereditaryRelated;
            entity.IsMentalHealthRelated = request.IsMentalHealthRelated || historySnapshot.IsMentalHealthRelated;
            entity.IsPregnancyRelated = request.IsPregnancyRelated || historySnapshot.IsPregnancyRelated;
            entity.IsSurgicalHistory = request.IsSurgicalHistory || request.HistoryType == PatientMedicalHistoryType.Surgery;
            entity.IsHospitalizationHistory = request.IsHospitalizationHistory || request.HistoryType == PatientMedicalHistoryType.Hospitalization;
            entity.IsHighRisk = request.IsHighRisk;
            entity.IsAlertEnabled = request.IsAlertEnabled;

            entity.OnsetDate = request.OnsetDate;
            entity.OnsetAgeYear = request.OnsetAgeYear;
            entity.DiagnosedDate = request.DiagnosedDate;
            entity.LastTreatmentDate = request.LastTreatmentDate;
            entity.LastControlDate = request.LastControlDate;
            entity.SourceOfInformation = NormalizeNullableText(request.SourceOfInformation);
            entity.TreatmentHistory = NormalizeNullableText(request.TreatmentHistory);
            entity.MedicationHistory = NormalizeNullableText(request.MedicationHistory);
            entity.SurgeryHistory = NormalizeNullableText(request.SurgeryHistory);
            entity.HospitalizationHistory = NormalizeNullableText(request.HospitalizationHistory);
            entity.ComplicationNote = NormalizeNullableText(request.ComplicationNote);
            entity.ClinicalNote = NormalizeNullableText(request.ClinicalNote);
            entity.RiskNote = NormalizeNullableText(request.RiskNote);
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            NormalizeMedicalHistoryData(entity);

            await _dbContext.SaveChangesAsync();

            var response = ToUpdateResponse(entity);

            return Ok(ApiResponse<PatientMedicalHistoryUpdateResponse>.Ok(
                response,
                "Riwayat penyakit pasien berhasil diubah."
            ));
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Verify Patient Medical History", Description = "Verifikasi riwayat penyakit pasien", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PatientMedicalHistory", "Update")]
        public async Task<IActionResult> VerifyMedicalHistory(Guid id, [FromBody] VerifyPatientMedicalHistoryRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientMedicalHistory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Riwayat penyakit pasien tidak ditemukan."
                ));
            }

            if (entity.HistoryStatus == PatientMedicalHistoryStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Riwayat penyakit yang sudah cancelled tidak dapat diverifikasi."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsVerified = true;
            entity.VerifiedAt = now;
            entity.VerifiedByUserId = actorUserId;

            if (!string.IsNullOrWhiteSpace(request.ClinicalNote))
                entity.ClinicalNote = request.ClinicalNote.Trim();

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Riwayat penyakit pasien berhasil diverifikasi."
            ));
        }

        [HttpPatch("{id:guid}/resolve")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Resolve Patient Medical History", Description = "Menyelesaikan status riwayat penyakit pasien", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("PatientMedicalHistory", "Update")]
        public async Task<IActionResult> ResolveMedicalHistory(Guid id, [FromBody] ResolvePatientMedicalHistoryRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientMedicalHistory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Riwayat penyakit pasien tidak ditemukan."
                ));
            }

            if (entity.HistoryStatus == PatientMedicalHistoryStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Riwayat penyakit yang sudah cancelled tidak dapat diselesaikan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.HistoryStatus = PatientMedicalHistoryStatus.Resolved;
            entity.IsCurrentProblem = false;
            entity.IsActive = false;
            entity.IsAlertEnabled = false;
            entity.ResolvedAt = now;
            entity.ResolvedByUserId = actorUserId;
            entity.ResolvedReason = request.ResolvedReason.Trim();
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Riwayat penyakit pasien berhasil diselesaikan."
            ));
        }

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Cancel Patient Medical History", Description = "Membatalkan riwayat penyakit pasien", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("PatientMedicalHistory", "Update")]
        public async Task<IActionResult> CancelMedicalHistory(Guid id, [FromBody] CancelPatientMedicalHistoryRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientMedicalHistory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Riwayat penyakit pasien tidak ditemukan."
                ));
            }

            if (entity.HistoryStatus == PatientMedicalHistoryStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Riwayat penyakit pasien sudah cancelled."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.HistoryStatus = PatientMedicalHistoryStatus.Cancelled;
            entity.IsCurrentProblem = false;
            entity.IsActive = false;
            entity.IsAlertEnabled = false;
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
                "Riwayat penyakit pasien berhasil dibatalkan."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Patient Medical History", Description = "Menghapus data riwayat penyakit pasien", AccessType = AccessTypes.Delete, SortOrder = 7)]
        [AccessPermission("PatientMedicalHistory", "Delete")]
        public async Task<IActionResult> DeleteMedicalHistory(Guid id)
        {
            var entity = await _dbContext.Set<TrxPatientMedicalHistory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Riwayat penyakit pasien tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            entity.IsAlertEnabled = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Riwayat penyakit pasien berhasil dihapus."
            ));
        }

        private IQueryable<TrxPatientMedicalHistory> BuildBaseQuery()
        {
            return _dbContext.Set<TrxPatientMedicalHistory>()
                .Include(x => x.Patient)
                .Include(x => x.Encounter)
                .Include(x => x.Consultation)
                .Include(x => x.Assessment)
                .Include(x => x.Doctor)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.VerifiedByUser)
                .Include(x => x.ResolvedByUser)
                .Include(x => x.CancelledByUser)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<TrxPatientMedicalHistory> ApplyFilters(
            IQueryable<TrxPatientMedicalHistory> query,
            string? search,
            Guid? patientId,
            Guid? encounterId,
            Guid? consultationId,
            Guid? assessmentId,
            Guid? doctorId,
            Guid? serviceUnitId,
            Guid? clinicId,
            Guid? diagnosisId,
            PatientMedicalHistoryType? historyType,
            PatientMedicalHistoryStatus? historyStatus,
            PatientMedicalHistorySeverity? severity,
            PatientMedicalHistoryCertainty? certainty,
            bool? isFromMasterDiagnosis,
            bool? isCurrentProblem,
            bool? isChronic,
            bool? isComorbidity,
            bool? isUnderTreatment,
            bool? isControlled,
            bool? isInfectiousDisease,
            bool? isHereditaryRelated,
            bool? isMentalHealthRelated,
            bool? isPregnancyRelated,
            bool? isSurgicalHistory,
            bool? isHospitalizationHistory,
            bool? isHighRisk,
            bool? isAlertEnabled,
            bool? isVerified,
            bool? isActive,
            DateTime? startDate,
            DateTime? endDate)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.MedicalHistoryRecordNumber.ToLower().Contains(keyword) ||
                    x.ConditionName.ToLower().Contains(keyword) ||
                    (x.ConditionCode != null && x.ConditionCode.ToLower().Contains(keyword)) ||
                    (x.ConditionGroupName != null && x.ConditionGroupName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)));
            }

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (encounterId.HasValue && encounterId.Value != Guid.Empty)
                query = query.Where(x => x.EncounterId == encounterId.Value);

            if (consultationId.HasValue && consultationId.Value != Guid.Empty)
                query = query.Where(x => x.ConsultationId == consultationId.Value);

            if (assessmentId.HasValue && assessmentId.Value != Guid.Empty)
                query = query.Where(x => x.AssessmentId == assessmentId.Value);

            if (doctorId.HasValue && doctorId.Value != Guid.Empty)
                query = query.Where(x => x.DoctorId == doctorId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
                query = query.Where(x => x.ClinicId == clinicId.Value);

            if (diagnosisId.HasValue && diagnosisId.Value != Guid.Empty)
                query = query.Where(x => x.DiagnosisId == diagnosisId.Value);

            if (historyType.HasValue)
                query = query.Where(x => x.HistoryType == historyType.Value);

            if (historyStatus.HasValue)
                query = query.Where(x => x.HistoryStatus == historyStatus.Value);

            if (severity.HasValue)
                query = query.Where(x => x.Severity == severity.Value);

            if (certainty.HasValue)
                query = query.Where(x => x.Certainty == certainty.Value);

            if (isFromMasterDiagnosis.HasValue)
                query = query.Where(x => x.IsFromMasterDiagnosis == isFromMasterDiagnosis.Value);

            if (isCurrentProblem.HasValue)
                query = query.Where(x => x.IsCurrentProblem == isCurrentProblem.Value);

            if (isChronic.HasValue)
                query = query.Where(x => x.IsChronic == isChronic.Value);

            if (isComorbidity.HasValue)
                query = query.Where(x => x.IsComorbidity == isComorbidity.Value);

            if (isUnderTreatment.HasValue)
                query = query.Where(x => x.IsUnderTreatment == isUnderTreatment.Value);

            if (isControlled.HasValue)
                query = query.Where(x => x.IsControlled == isControlled.Value);

            if (isInfectiousDisease.HasValue)
                query = query.Where(x => x.IsInfectiousDisease == isInfectiousDisease.Value);

            if (isHereditaryRelated.HasValue)
                query = query.Where(x => x.IsHereditaryRelated == isHereditaryRelated.Value);

            if (isMentalHealthRelated.HasValue)
                query = query.Where(x => x.IsMentalHealthRelated == isMentalHealthRelated.Value);

            if (isPregnancyRelated.HasValue)
                query = query.Where(x => x.IsPregnancyRelated == isPregnancyRelated.Value);

            if (isSurgicalHistory.HasValue)
                query = query.Where(x => x.IsSurgicalHistory == isSurgicalHistory.Value);

            if (isHospitalizationHistory.HasValue)
                query = query.Where(x => x.IsHospitalizationHistory == isHospitalizationHistory.Value);

            if (isHighRisk.HasValue)
                query = query.Where(x => x.IsHighRisk == isHighRisk.Value);

            if (isAlertEnabled.HasValue)
                query = query.Where(x => x.IsAlertEnabled == isAlertEnabled.Value);

            if (isVerified.HasValue)
                query = query.Where(x => x.IsVerified == isVerified.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (startDate.HasValue)
                query = query.Where(x => x.RecordedDateTime >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(x => x.RecordedDateTime < endDate.Value.Date.AddDays(1));

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(CreatePatientMedicalHistoryRequest request)
        {
            if (request.PatientId == Guid.Empty)
                return (false, "PatientId wajib diisi.");

            if (!request.DiagnosisId.HasValue && string.IsNullOrWhiteSpace(request.ConditionName))
                return (false, "ConditionName wajib diisi jika DiagnosisId tidak dipilih.");

            var patientExists = await _dbContext.Set<MstPatient>()
                .AnyAsync(x => x.Id == request.PatientId && !x.IsDelete);

            if (!patientExists)
                return (false, "Pasien tidak ditemukan.");

            return ValidateDateAndAge(request.OnsetDate, request.DiagnosedDate, request.LastTreatmentDate, request.LastControlDate, request.OnsetAgeYear);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateUpdateRequestAsync(UpdatePatientMedicalHistoryRequest request)
        {
            if (!request.DiagnosisId.HasValue && string.IsNullOrWhiteSpace(request.ConditionName))
                return (false, "ConditionName wajib diisi jika DiagnosisId tidak dipilih.");

            return ValidateDateAndAge(request.OnsetDate, request.DiagnosedDate, request.LastTreatmentDate, request.LastControlDate, request.OnsetAgeYear);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateDateAndAge(
            DateTime? onsetDate,
            DateTime? diagnosedDate,
            DateTime? lastTreatmentDate,
            DateTime? lastControlDate,
            int? onsetAgeYear)
        {
            if (onsetAgeYear.HasValue && (onsetAgeYear.Value < 0 || onsetAgeYear.Value > 150))
                return (false, "Usia onset harus berada di antara 0 sampai 150 tahun.");

            if (onsetDate.HasValue && diagnosedDate.HasValue && onsetDate.Value.Date > diagnosedDate.Value.Date)
                return (false, "Tanggal onset tidak boleh lebih besar dari tanggal diagnosis.");

            if (diagnosedDate.HasValue && lastTreatmentDate.HasValue && diagnosedDate.Value.Date > lastTreatmentDate.Value.Date)
                return (false, "Tanggal diagnosis tidak boleh lebih besar dari tanggal terapi terakhir.");

            if (diagnosedDate.HasValue && lastControlDate.HasValue && diagnosedDate.Value.Date > lastControlDate.Value.Date)
                return (false, "Tanggal diagnosis tidak boleh lebih besar dari tanggal kontrol terakhir.");

            return (true, null);
        }

        private async Task<MedicalHistorySnapshotResult> BuildMedicalHistorySnapshotAsync(
            Guid? diagnosisId,
            string? conditionCode,
            string? conditionName,
            string? conditionGroupName,
            string? conditionMasterType,
            string? icdVersion)
        {
            var normalizedDiagnosisId = NormalizeNullableGuid(diagnosisId);

            if (normalizedDiagnosisId.HasValue)
            {
                var diagnosis = await _dbContext.Set<MstDiagnosis>()
                    .AsNoTracking()
                    .Include(x => x.DiagnosisChapter)
                    .FirstOrDefaultAsync(x => x.Id == normalizedDiagnosisId.Value && !x.IsDelete);

                if (diagnosis == null)
                    return MedicalHistorySnapshotResult.Fail("Master diagnosis tidak ditemukan.");

                return new MedicalHistorySnapshotResult
                {
                    IsValid = true,
                    DiagnosisId = diagnosis.Id,
                    ConditionCode = diagnosis.DiagnosisCode,
                    ConditionName = diagnosis.DiagnosisName,
                    ConditionGroupName = diagnosis.DiagnosisChapter != null ? diagnosis.DiagnosisChapter.ChapterName : null,
                    ConditionMasterType = diagnosis.DiagnosisType,
                    IcdVersion = diagnosis.IcdVersion,
                    IsFromMasterDiagnosis = true,
                    IsChronicDisease = false,
                    IsInfectiousDisease = false,
                    IsPregnancyRelated = false,
                    IsMentalHealthRelated = false
                };
            }

            var normalizedName = NormalizeNullableText(conditionName);

            if (string.IsNullOrWhiteSpace(normalizedName))
                return MedicalHistorySnapshotResult.Fail("Nama kondisi wajib diisi.");

            return new MedicalHistorySnapshotResult
            {
                IsValid = true,
                DiagnosisId = null,
                ConditionCode = NormalizeNullableText(conditionCode),
                ConditionName = normalizedName,
                ConditionGroupName = NormalizeNullableText(conditionGroupName),
                ConditionMasterType = NormalizeNullableText(conditionMasterType) ?? "Manual",
                IcdVersion = NormalizeNullableText(icdVersion),
                IsFromMasterDiagnosis = false,
                IsChronicDisease = false,
                IsInfectiousDisease = false,
                IsPregnancyRelated = false,
                IsMentalHealthRelated = false
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateDuplicateMedicalHistoryAsync(
            Guid patientId,
            Guid? diagnosisId,
            PatientMedicalHistoryType historyType,
            string conditionName,
            Guid? excludeId)
        {
            var query = _dbContext.Set<TrxPatientMedicalHistory>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.PatientId == patientId &&
                    x.HistoryStatus != PatientMedicalHistoryStatus.Cancelled &&
                    x.HistoryStatus != PatientMedicalHistoryStatus.EnteredInError);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            if (diagnosisId.HasValue)
            {
                var duplicateByDiagnosis = await query.AnyAsync(x => x.DiagnosisId == diagnosisId.Value);

                if (duplicateByDiagnosis)
                    return (false, "Riwayat penyakit dengan diagnosis yang sama sudah tercatat untuk pasien ini.");
            }

            var normalizedName = conditionName.Trim().ToLower();

            var duplicateByName = await query.AnyAsync(x =>
                x.HistoryType == historyType &&
                x.ConditionName.ToLower() == normalizedName);

            if (duplicateByName)
                return (false, "Riwayat penyakit dengan tipe dan nama kondisi yang sama sudah tercatat untuk pasien ini.");

            return (true, null);
        }

        private async Task<ClinicalContextResult> ResolveClinicalContextAsync(
            Guid patientId,
            Guid? encounterId,
            Guid? consultationId,
            Guid? assessmentId,
            Guid? doctorId,
            Guid? serviceUnitId,
            Guid? clinicId)
        {
            var result = new ClinicalContextResult
            {
                EncounterId = NormalizeNullableGuid(encounterId),
                ConsultationId = NormalizeNullableGuid(consultationId),
                AssessmentId = NormalizeNullableGuid(assessmentId),
                DoctorId = NormalizeNullableGuid(doctorId),
                ServiceUnitId = NormalizeNullableGuid(serviceUnitId),
                ClinicId = NormalizeNullableGuid(clinicId)
            };

            if (result.ConsultationId.HasValue)
            {
                var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.ConsultationId.Value && !x.IsDelete);

                if (consultation == null)
                    return ClinicalContextResult.Fail("Konsultasi dokter tidak ditemukan.");

                if (consultation.PatientId != patientId)
                    return ClinicalContextResult.Fail("Konsultasi dokter tidak sesuai dengan pasien.");

                result.EncounterId = consultation.EncounterId;
                result.AssessmentId = consultation.AssessmentId ?? result.AssessmentId;
                result.DoctorId = consultation.DoctorId;
                result.ServiceUnitId = consultation.ServiceUnitId;
                result.ClinicId = consultation.ClinicId;

                return result.Ok();
            }

            if (result.AssessmentId.HasValue)
            {
                var assessment = await _dbContext.Set<TrxPatientAssessment>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.AssessmentId.Value && !x.IsDelete);

                if (assessment == null)
                    return ClinicalContextResult.Fail("Assessment pasien tidak ditemukan.");

                if (assessment.PatientId != patientId)
                    return ClinicalContextResult.Fail("Assessment pasien tidak sesuai dengan pasien.");

                result.EncounterId = assessment.EncounterId;
                result.DoctorId = assessment.DoctorId ?? result.DoctorId;
                result.ServiceUnitId = assessment.ServiceUnitId;
                result.ClinicId = assessment.ClinicId;

                return result.Ok();
            }

            if (result.EncounterId.HasValue)
            {
                var encounter = await _dbContext.Set<TrxPatientEncounter>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.EncounterId.Value && !x.IsDelete);

                if (encounter == null)
                    return ClinicalContextResult.Fail("Encounter pasien tidak ditemukan.");

                if (encounter.PatientId != patientId)
                    return ClinicalContextResult.Fail("Encounter tidak sesuai dengan pasien.");
            }

            return result.Ok();
        }

        private async Task<string> GenerateMedicalHistoryRecordNumberAsync(DateTime now)
        {
            var prefix = $"PMH-{now:yyyyMMdd}";

            var countToday = await _dbContext.Set<TrxPatientMedicalHistory>()
                .CountAsync(x => x.MedicalHistoryRecordNumber.StartsWith(prefix));

            return $"{prefix}-{countToday + 1:0000}";
        }

        private static IQueryable<TrxPatientMedicalHistory> ApplySorting(
            IQueryable<TrxPatientMedicalHistory> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "recordedDateTime").ToLowerInvariant() switch
            {
                "conditionname" => isDesc ? query.OrderByDescending(x => x.ConditionName) : query.OrderBy(x => x.ConditionName),
                "conditioncode" => isDesc ? query.OrderByDescending(x => x.ConditionCode) : query.OrderBy(x => x.ConditionCode),
                "historytype" => isDesc ? query.OrderByDescending(x => x.HistoryType) : query.OrderBy(x => x.HistoryType),
                "historystatus" => isDesc ? query.OrderByDescending(x => x.HistoryStatus) : query.OrderBy(x => x.HistoryStatus),
                "severity" => isDesc ? query.OrderByDescending(x => x.Severity) : query.OrderBy(x => x.Severity),
                "certainty" => isDesc ? query.OrderByDescending(x => x.Certainty) : query.OrderBy(x => x.Certainty),
                "iscurrentproblem" => isDesc ? query.OrderByDescending(x => x.IsCurrentProblem) : query.OrderBy(x => x.IsCurrentProblem),
                "ischronic" => isDesc ? query.OrderByDescending(x => x.IsChronic) : query.OrderBy(x => x.IsChronic),
                "iscomorbidity" => isDesc ? query.OrderByDescending(x => x.IsComorbidity) : query.OrderBy(x => x.IsComorbidity),
                "ishighrisk" => isDesc ? query.OrderByDescending(x => x.IsHighRisk) : query.OrderBy(x => x.IsHighRisk),
                "isverified" => isDesc ? query.OrderByDescending(x => x.IsVerified) : query.OrderBy(x => x.IsVerified),
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDesc
                    ? query.OrderByDescending(x => x.RecordedDateTime)
                        .ThenByDescending(x => x.IsCurrentProblem)
                        .ThenByDescending(x => x.IsHighRisk)
                    : query.OrderBy(x => x.RecordedDateTime)
                        .ThenByDescending(x => x.IsCurrentProblem)
                        .ThenByDescending(x => x.IsHighRisk)
            };
        }

        private static PatientMedicalHistoryResponse ToResponse(TrxPatientMedicalHistory x)
        {
            return new PatientMedicalHistoryResponse
            {
                Id = x.Id,
                MedicalHistoryRecordNumber = x.MedicalHistoryRecordNumber,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : null,
                ConsultationId = x.ConsultationId,
                ConsultationNumber = x.Consultation != null ? x.Consultation.ConsultationNumber : null,
                AssessmentId = x.AssessmentId,
                AssessmentNumber = x.Assessment != null ? x.Assessment.AssessmentNumber : null,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                DiagnosisId = x.DiagnosisId,
                HistoryType = x.HistoryType,
                HistoryStatus = x.HistoryStatus,
                Severity = x.Severity,
                Certainty = x.Certainty,
                ConditionCode = x.ConditionCode,
                ConditionName = x.ConditionName,
                ConditionGroupName = x.ConditionGroupName,
                ConditionMasterType = x.ConditionMasterType,
                IcdVersion = x.IcdVersion,
                IsFromMasterDiagnosis = x.IsFromMasterDiagnosis,
                IsCurrentProblem = x.IsCurrentProblem,
                IsChronic = x.IsChronic,
                IsComorbidity = x.IsComorbidity,
                IsUnderTreatment = x.IsUnderTreatment,
                IsControlled = x.IsControlled,
                IsInfectiousDisease = x.IsInfectiousDisease,
                IsHereditaryRelated = x.IsHereditaryRelated,
                IsMentalHealthRelated = x.IsMentalHealthRelated,
                IsPregnancyRelated = x.IsPregnancyRelated,
                IsSurgicalHistory = x.IsSurgicalHistory,
                IsHospitalizationHistory = x.IsHospitalizationHistory,
                IsHighRisk = x.IsHighRisk,
                IsAlertEnabled = x.IsAlertEnabled,
                RecordedDateTime = x.RecordedDateTime,
                OnsetDate = x.OnsetDate,
                OnsetAgeYear = x.OnsetAgeYear,
                DiagnosedDate = x.DiagnosedDate,
                LastTreatmentDate = x.LastTreatmentDate,
                LastControlDate = x.LastControlDate,
                SourceOfInformation = x.SourceOfInformation,
                IsVerified = x.IsVerified,
                VerifiedAt = x.VerifiedAt,
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByUserName = x.VerifiedByUser != null ? x.VerifiedByUser.DisplayName : null,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static PatientMedicalHistoryDetailResponse ToDetailResponse(TrxPatientMedicalHistory x)
        {
            return new PatientMedicalHistoryDetailResponse
            {
                Id = x.Id,
                MedicalHistoryRecordNumber = x.MedicalHistoryRecordNumber,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : null,
                ConsultationId = x.ConsultationId,
                ConsultationNumber = x.Consultation != null ? x.Consultation.ConsultationNumber : null,
                AssessmentId = x.AssessmentId,
                AssessmentNumber = x.Assessment != null ? x.Assessment.AssessmentNumber : null,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                DiagnosisId = x.DiagnosisId,
                HistoryType = x.HistoryType,
                HistoryStatus = x.HistoryStatus,
                Severity = x.Severity,
                Certainty = x.Certainty,
                ConditionCode = x.ConditionCode,
                ConditionName = x.ConditionName,
                ConditionGroupName = x.ConditionGroupName,
                ConditionMasterType = x.ConditionMasterType,
                IcdVersion = x.IcdVersion,
                IsFromMasterDiagnosis = x.IsFromMasterDiagnosis,
                IsCurrentProblem = x.IsCurrentProblem,
                IsChronic = x.IsChronic,
                IsComorbidity = x.IsComorbidity,
                IsUnderTreatment = x.IsUnderTreatment,
                IsControlled = x.IsControlled,
                IsInfectiousDisease = x.IsInfectiousDisease,
                IsHereditaryRelated = x.IsHereditaryRelated,
                IsMentalHealthRelated = x.IsMentalHealthRelated,
                IsPregnancyRelated = x.IsPregnancyRelated,
                IsSurgicalHistory = x.IsSurgicalHistory,
                IsHospitalizationHistory = x.IsHospitalizationHistory,
                IsHighRisk = x.IsHighRisk,
                IsAlertEnabled = x.IsAlertEnabled,
                RecordedDateTime = x.RecordedDateTime,
                OnsetDate = x.OnsetDate,
                OnsetAgeYear = x.OnsetAgeYear,
                DiagnosedDate = x.DiagnosedDate,
                LastTreatmentDate = x.LastTreatmentDate,
                LastControlDate = x.LastControlDate,
                SourceOfInformation = x.SourceOfInformation,
                TreatmentHistory = x.TreatmentHistory,
                MedicationHistory = x.MedicationHistory,
                SurgeryHistory = x.SurgeryHistory,
                HospitalizationHistory = x.HospitalizationHistory,
                ComplicationNote = x.ComplicationNote,
                ClinicalNote = x.ClinicalNote,
                RiskNote = x.RiskNote,
                Notes = x.Notes,
                IsVerified = x.IsVerified,
                VerifiedAt = x.VerifiedAt,
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByUserName = x.VerifiedByUser != null ? x.VerifiedByUser.DisplayName : null,
                ResolvedAt = x.ResolvedAt,
                ResolvedByUserId = x.ResolvedByUserId,
                ResolvedByUserName = x.ResolvedByUser != null ? x.ResolvedByUser.DisplayName : null,
                ResolvedReason = x.ResolvedReason,
                CancelledAt = x.CancelledAt,
                CancelledByUserId = x.CancelledByUserId,
                CancelledByUserName = x.CancelledByUser != null ? x.CancelledByUser.DisplayName : null,
                CancelReason = x.CancelReason,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static PatientMedicalHistoryCreateResponse ToCreateUpdateResponse(TrxPatientMedicalHistory x)
        {
            return new PatientMedicalHistoryCreateResponse
            {
                Id = x.Id,
                MedicalHistoryRecordNumber = x.MedicalHistoryRecordNumber,
                PatientId = x.PatientId,
                EncounterId = x.EncounterId,
                ConsultationId = x.ConsultationId,
                AssessmentId = x.AssessmentId,
                DiagnosisId = x.DiagnosisId,
                HistoryType = x.HistoryType,
                HistoryStatus = x.HistoryStatus,
                Severity = x.Severity,
                Certainty = x.Certainty,
                ConditionCode = x.ConditionCode,
                ConditionName = x.ConditionName,
                ConditionGroupName = x.ConditionGroupName,
                ConditionMasterType = x.ConditionMasterType,
                IsFromMasterDiagnosis = x.IsFromMasterDiagnosis,
                IsCurrentProblem = x.IsCurrentProblem,
                IsChronic = x.IsChronic,
                IsComorbidity = x.IsComorbidity,
                IsHighRisk = x.IsHighRisk,
                IsAlertEnabled = x.IsAlertEnabled,
                IsVerified = x.IsVerified
            };
        }

        private static PatientMedicalHistoryUpdateResponse ToUpdateResponse(TrxPatientMedicalHistory x)
        {
            return new PatientMedicalHistoryUpdateResponse
            {
                Id = x.Id,
                MedicalHistoryRecordNumber = x.MedicalHistoryRecordNumber,
                PatientId = x.PatientId,
                EncounterId = x.EncounterId,
                ConsultationId = x.ConsultationId,
                AssessmentId = x.AssessmentId,
                DiagnosisId = x.DiagnosisId,
                HistoryType = x.HistoryType,
                HistoryStatus = x.HistoryStatus,
                Severity = x.Severity,
                Certainty = x.Certainty,
                ConditionCode = x.ConditionCode,
                ConditionName = x.ConditionName,
                ConditionGroupName = x.ConditionGroupName,
                ConditionMasterType = x.ConditionMasterType,
                IsFromMasterDiagnosis = x.IsFromMasterDiagnosis,
                IsCurrentProblem = x.IsCurrentProblem,
                IsChronic = x.IsChronic,
                IsComorbidity = x.IsComorbidity,
                IsHighRisk = x.IsHighRisk,
                IsAlertEnabled = x.IsAlertEnabled,
                IsVerified = x.IsVerified
            };
        }

        private static void NormalizeMedicalHistoryData(TrxPatientMedicalHistory entity)
        {
            if (entity.HistoryType == PatientMedicalHistoryType.Surgery)
                entity.IsSurgicalHistory = true;

            if (entity.HistoryType == PatientMedicalHistoryType.Hospitalization)
                entity.IsHospitalizationHistory = true;

            if (entity.HistoryType == PatientMedicalHistoryType.ChronicDisease)
                entity.IsChronic = true;

            if (entity.HistoryType == PatientMedicalHistoryType.InfectiousDisease)
                entity.IsInfectiousDisease = true;

            if (entity.HistoryType == PatientMedicalHistoryType.Psychiatric)
                entity.IsMentalHealthRelated = true;

            if (entity.HistoryStatus == PatientMedicalHistoryStatus.UnderTreatment)
                entity.IsUnderTreatment = true;

            if (entity.HistoryStatus == PatientMedicalHistoryStatus.Controlled)
                entity.IsControlled = true;

            if (entity.Severity == PatientMedicalHistorySeverity.Critical)
                entity.IsHighRisk = true;

            if (entity.HistoryStatus == PatientMedicalHistoryStatus.Inactive ||
                entity.HistoryStatus == PatientMedicalHistoryStatus.Resolved ||
                entity.HistoryStatus == PatientMedicalHistoryStatus.EnteredInError ||
                entity.HistoryStatus == PatientMedicalHistoryStatus.Cancelled)
            {
                entity.IsCurrentProblem = false;
                entity.IsAlertEnabled = false;
            }

            if (!entity.IsActive)
            {
                entity.IsCurrentProblem = false;
                entity.IsAlertEnabled = false;
            }
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<PatientMedicalHistoryEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new PatientMedicalHistoryEnumOptionResponse
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

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
                return null;

            return value.Value;
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userId, out var id)
                ? id
                : Guid.Empty;
        }

        private class ClinicalContextResult
        {
            public bool IsValid { get; set; }

            public string? ErrorMessage { get; set; }

            public Guid? EncounterId { get; set; }

            public Guid? ConsultationId { get; set; }

            public Guid? AssessmentId { get; set; }

            public Guid? DoctorId { get; set; }

            public Guid? ServiceUnitId { get; set; }

            public Guid? ClinicId { get; set; }

            public ClinicalContextResult Ok()
            {
                IsValid = true;
                return this;
            }

            public static ClinicalContextResult Fail(string errorMessage)
            {
                return new ClinicalContextResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }

        private class MedicalHistorySnapshotResult
        {
            public bool IsValid { get; set; }

            public string? ErrorMessage { get; set; }

            public Guid? DiagnosisId { get; set; }

            public string? ConditionCode { get; set; }

            public string ConditionName { get; set; } = string.Empty;

            public string? ConditionGroupName { get; set; }

            public string ConditionMasterType { get; set; } = "Manual";

            public string? IcdVersion { get; set; }

            public bool IsFromMasterDiagnosis { get; set; }

            public bool IsChronicDisease { get; set; }

            public bool IsInfectiousDisease { get; set; }

            public bool IsPregnancyRelated { get; set; }

            public bool IsMentalHealthRelated { get; set; }

            public static MedicalHistorySnapshotResult Fail(string errorMessage)
            {
                return new MedicalHistorySnapshotResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}
