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

using ResponsePatientFamilyHistoryPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs.PatientFamilyHistoryResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/patient-family-histories")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Patient Family History",
        AreaName = "HealthServices",
        ControllerName = "PatientFamilyHistory",
        Description = "Riwayat penyakit keluarga pasien dan hereditary risk alert",
        SortOrder = 7
    )]
    [Tags("Health Services / Clinical Management / Patient Family History")]
    public class PatientFamilyHistoryController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Clinical";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PatientFamilyHistoryController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientFamilyHistoryFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Family History", Description = "Melihat metadata filter riwayat penyakit keluarga pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientFamilyHistory", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientFamilyHistoryFilterMetadataResponse
            {
                DefaultFilter = new PatientFamilyHistoryDefaultFilterResponse(),
                SortOptions = new List<PatientFamilyHistorySortOptionResponse>
                {
                    new() { Value = "recordedDateTime", Label = "Tanggal pencatatan" },
                    new() { Value = "conditionName", Label = "Nama kondisi" },
                    new() { Value = "conditionCode", Label = "Kode kondisi" },
                    new() { Value = "relationshipType", Label = "Hubungan keluarga" },
                    new() { Value = "relationshipSide", Label = "Sisi keluarga" },
                    new() { Value = "familyHistoryStatus", Label = "Status riwayat" },
                    new() { Value = "certainty", Label = "Kepastian" },
                    new() { Value = "riskLevel", Label = "Level risiko" },
                    new() { Value = "isHereditaryDisease", Label = "Herediter" },
                    new() { Value = "isGeneticRisk", Label = "Risiko genetik" },
                    new() { Value = "isHighRisk", Label = "Risiko tinggi" },
                    new() { Value = "isScreeningRecommended", Label = "Butuh screening" },
                    new() { Value = "isVerified", Label = "Terverifikasi" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                RelationshipTypeOptions = BuildEnumOptions<PatientFamilyRelationshipType>(),
                RelationshipSideOptions = BuildEnumOptions<PatientFamilyRelationshipSide>(),
                FamilyHistoryStatusOptions = BuildEnumOptions<PatientFamilyHistoryStatus>(),
                CertaintyOptions = BuildEnumOptions<PatientFamilyHistoryCertainty>(),
                RiskLevelOptions = BuildEnumOptions<PatientFamilyRiskLevel>()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientFamilyHistory.GetFilterMetadata",
                "Mengambil metadata filter riwayat penyakit keluarga pasien.",
                result
            );

            return Ok(ApiResponse<PatientFamilyHistoryFilterMetadataResponse>.Ok(
                result,
                "Metadata filter riwayat penyakit keluarga pasien berhasil diambil."
            ));
        }

        [HttpGet("active-alerts")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientFamilyHistoryAlertResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Family History", Description = "Melihat alert riwayat penyakit keluarga aktif pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientFamilyHistory", "Read")]
        public async Task<IActionResult> GetActiveAlerts([FromQuery] Guid patientId)
        {
            if (patientId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "PatientId wajib diisi."
                ));
            }

            var data = await _dbContext.Set<TrxPatientFamilyHistory>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.PatientId == patientId &&
                    x.IsActive &&
                    x.IsAlertEnabled &&
                    x.FamilyHistoryStatus != PatientFamilyHistoryStatus.Inactive &&
                    x.FamilyHistoryStatus != PatientFamilyHistoryStatus.Resolved &&
                    x.FamilyHistoryStatus != PatientFamilyHistoryStatus.EnteredInError &&
                    x.FamilyHistoryStatus != PatientFamilyHistoryStatus.Cancelled)
                .OrderByDescending(x => x.IsHighRisk)
                .ThenByDescending(x => x.RiskLevel)
                .ThenByDescending(x => x.IsFirstDegreeRelative)
                .ThenByDescending(x => x.IsScreeningRecommended)
                .ThenBy(x => x.ConditionName)
                .Select(x => new PatientFamilyHistoryAlertResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    FamilyHistoryRecordNumber = x.FamilyHistoryRecordNumber,
                    RelationshipType = x.RelationshipType,
                    RelationshipSide = x.RelationshipSide,
                    RelationshipDescription = x.RelationshipDescription,
                    DiagnosisId = x.DiagnosisId,
                    ConditionCode = x.ConditionCode,
                    ConditionName = x.ConditionName,
                    ConditionGroupName = x.ConditionGroupName,
                    RiskLevel = x.RiskLevel,
                    IsFirstDegreeRelative = x.IsFirstDegreeRelative,
                    IsHereditaryDisease = x.IsHereditaryDisease,
                    IsGeneticRisk = x.IsGeneticRisk,
                    IsCancerRelated = x.IsCancerRelated,
                    IsCardiovascularRisk = x.IsCardiovascularRisk,
                    IsMetabolicRisk = x.IsMetabolicRisk,
                    IsHighRisk = x.IsHighRisk,
                    IsScreeningRecommended = x.IsScreeningRecommended,
                    RiskNote = x.RiskNote,
                    ScreeningRecommendation = x.ScreeningRecommendation
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientFamilyHistoryAlertResponse>>.Ok(
                data,
                "Alert riwayat penyakit keluarga aktif pasien berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientFamilyHistoryPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Family History", Description = "Melihat data riwayat penyakit keluarga pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientFamilyHistory", "Read")]
        public async Task<IActionResult> GetFamilyHistories(
            [FromQuery] string? search,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? consultationId,
            [FromQuery] Guid? assessmentId,
            [FromQuery] Guid? doctorId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? familyMemberPatientId,
            [FromQuery] Guid? diagnosisId,
            [FromQuery] PatientFamilyRelationshipType? relationshipType,
            [FromQuery] PatientFamilyRelationshipSide? relationshipSide,
            [FromQuery] PatientFamilyHistoryStatus? familyHistoryStatus,
            [FromQuery] PatientFamilyHistoryCertainty? certainty,
            [FromQuery] PatientFamilyRiskLevel? riskLevel,
            [FromQuery] bool? isFromMasterDiagnosis,
            [FromQuery] bool? isFirstDegreeRelative,
            [FromQuery] bool? isSecondDegreeRelative,
            [FromQuery] bool? isSameHousehold,
            [FromQuery] bool? isHereditaryDisease,
            [FromQuery] bool? isGeneticRisk,
            [FromQuery] bool? isChronicDisease,
            [FromQuery] bool? isCancerRelated,
            [FromQuery] bool? isCardiovascularRisk,
            [FromQuery] bool? isMetabolicRisk,
            [FromQuery] bool? isMentalHealthRelated,
            [FromQuery] bool? isInfectiousDisease,
            [FromQuery] bool? isHighRisk,
            [FromQuery] bool? isScreeningRecommended,
            [FromQuery] bool? isAlertEnabled,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isFamilyMemberDeceased,
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
                familyMemberPatientId,
                diagnosisId,
                relationshipType,
                relationshipSide,
                familyHistoryStatus,
                certainty,
                riskLevel,
                isFromMasterDiagnosis,
                isFirstDegreeRelative,
                isSecondDegreeRelative,
                isSameHousehold,
                isHereditaryDisease,
                isGeneticRisk,
                isChronicDisease,
                isCancerRelated,
                isCardiovascularRisk,
                isMetabolicRisk,
                isMentalHealthRelated,
                isInfectiousDisease,
                isHighRisk,
                isScreeningRecommended,
                isAlertEnabled,
                isVerified,
                isFamilyMemberDeceased,
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

            var result = new ResponsePatientFamilyHistoryPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePatientFamilyHistoryPagedResult>.Ok(
                result,
                "Data riwayat penyakit keluarga pasien berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientFamilyHistoryOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Family History", Description = "Melihat pilihan riwayat penyakit keluarga pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientFamilyHistory", "Read")]
        public async Task<IActionResult> GetFamilyHistoryOptions(
            [FromQuery] Guid? patientId,
            [FromQuery] PatientFamilyRelationshipType? relationshipType,
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool onlyAlertEnabled = false,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<TrxPatientFamilyHistory>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive && x.FamilyHistoryStatus == PatientFamilyHistoryStatus.Active);

            if (onlyAlertEnabled)
                query = query.Where(x => x.IsAlertEnabled);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (relationshipType.HasValue)
                query = query.Where(x => x.RelationshipType == relationshipType.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.FamilyHistoryRecordNumber.ToLower().Contains(keyword) ||
                    x.ConditionName.ToLower().Contains(keyword) ||
                    (x.ConditionCode != null && x.ConditionCode.ToLower().Contains(keyword)) ||
                    (x.ConditionGroupName != null && x.ConditionGroupName.ToLower().Contains(keyword)) ||
                    (x.RelationshipDescription != null && x.RelationshipDescription.ToLower().Contains(keyword)) ||
                    (x.FamilyMemberNameSnapshot != null && x.FamilyMemberNameSnapshot.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderByDescending(x => x.IsHighRisk)
                .ThenByDescending(x => x.RiskLevel)
                .ThenBy(x => x.RelationshipType)
                .ThenBy(x => x.ConditionName)
                .Take(100)
                .Select(x => new PatientFamilyHistoryOptionResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    FamilyHistoryRecordNumber = x.FamilyHistoryRecordNumber,
                    RelationshipType = x.RelationshipType,
                    RelationshipSide = x.RelationshipSide,
                    RelationshipDescription = x.RelationshipDescription,
                    DiagnosisId = x.DiagnosisId,
                    ConditionCode = x.ConditionCode,
                    ConditionName = x.ConditionName,
                    ConditionGroupName = x.ConditionGroupName,
                    FamilyHistoryStatus = x.FamilyHistoryStatus,
                    Certainty = x.Certainty,
                    RiskLevel = x.RiskLevel,
                    IsHereditaryDisease = x.IsHereditaryDisease,
                    IsGeneticRisk = x.IsGeneticRisk,
                    IsHighRisk = x.IsHighRisk,
                    IsScreeningRecommended = x.IsScreeningRecommended,
                    IsAlertEnabled = x.IsAlertEnabled,
                    IsVerified = x.IsVerified
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientFamilyHistoryOptionResponse>>.Ok(
                data,
                "Data pilihan riwayat penyakit keluarga pasien berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientFamilyHistoryDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Family History", Description = "Melihat detail riwayat penyakit keluarga pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientFamilyHistory", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Riwayat penyakit keluarga pasien tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientFamilyHistoryDetailResponse>.Ok(
                ToDetailResponse(entity),
                "Detail riwayat penyakit keluarga pasien berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientFamilyHistoryCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Patient Family History", Description = "Membuat data riwayat penyakit keluarga pasien", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PatientFamilyHistory", "Create")]
        public async Task<IActionResult> CreateFamilyHistory([FromBody] CreatePatientFamilyHistoryRequest request)
        {
            var validation = await ValidateCreateRequestAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data riwayat penyakit keluarga pasien tidak valid."
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

            var conditionSnapshot = await BuildFamilyHistorySnapshotAsync(
                request.DiagnosisId,
                request.ConditionCode,
                request.ConditionName,
                request.ConditionGroupName,
                request.ConditionMasterType,
                request.IcdVersion
            );

            if (!conditionSnapshot.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    conditionSnapshot.ErrorMessage ?? "Data kondisi riwayat keluarga tidak valid."
                ));
            }

            var duplicateValidation = await ValidateDuplicateFamilyHistoryAsync(
                patientId: request.PatientId,
                diagnosisId: conditionSnapshot.DiagnosisId,
                relationshipType: request.RelationshipType,
                relationshipSide: request.RelationshipSide,
                conditionName: conditionSnapshot.ConditionName,
                excludeId: null
            );

            if (!duplicateValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    duplicateValidation.ErrorMessage ?? "Data riwayat penyakit keluarga pasien duplikat."
                ));
            }

            var entity = new TrxPatientFamilyHistory
            {
                Id = Guid.NewGuid(),
                FamilyHistoryRecordNumber = await GenerateFamilyHistoryRecordNumberAsync(now),
                PatientId = request.PatientId,
                EncounterId = context.EncounterId,
                ConsultationId = context.ConsultationId,
                AssessmentId = context.AssessmentId,
                DoctorId = context.DoctorId,
                ServiceUnitId = context.ServiceUnitId,
                ClinicId = context.ClinicId,
                FamilyMemberPatientId = NormalizeNullableGuid(request.FamilyMemberPatientId),
                DiagnosisId = conditionSnapshot.DiagnosisId,

                RelationshipType = request.RelationshipType,
                RelationshipSide = request.RelationshipSide,
                FamilyMemberNameSnapshot = NormalizeNullableText(request.FamilyMemberNameSnapshot),
                RelationshipDescription = NormalizeNullableText(request.RelationshipDescription),
                IsFirstDegreeRelative = request.IsFirstDegreeRelative,
                IsSecondDegreeRelative = request.IsSecondDegreeRelative,
                IsSameHousehold = request.IsSameHousehold,

                ConditionCode = conditionSnapshot.ConditionCode,
                ConditionName = conditionSnapshot.ConditionName,
                ConditionGroupName = conditionSnapshot.ConditionGroupName,
                ConditionMasterType = conditionSnapshot.ConditionMasterType,
                IcdVersion = conditionSnapshot.IcdVersion,
                IsFromMasterDiagnosis = conditionSnapshot.IsFromMasterDiagnosis,

                FamilyHistoryStatus = request.FamilyHistoryStatus,
                Certainty = request.Certainty,
                RiskLevel = request.RiskLevel,
                IsHereditaryDisease = request.IsHereditaryDisease,
                IsGeneticRisk = request.IsGeneticRisk,
                IsChronicDisease = request.IsChronicDisease || conditionSnapshot.IsChronicDisease,
                IsCancerRelated = request.IsCancerRelated,
                IsCardiovascularRisk = request.IsCardiovascularRisk,
                IsMetabolicRisk = request.IsMetabolicRisk,
                IsMentalHealthRelated = request.IsMentalHealthRelated || conditionSnapshot.IsMentalHealthRelated,
                IsInfectiousDisease = request.IsInfectiousDisease || conditionSnapshot.IsInfectiousDisease,
                IsHighRisk = request.IsHighRisk,
                IsScreeningRecommended = request.IsScreeningRecommended,
                IsAlertEnabled = request.IsAlertEnabled,

                RecordedDateTime = request.RecordedDateTime ?? now,
                DiagnosedDate = request.DiagnosedDate,
                AgeAtDiagnosisYear = request.AgeAtDiagnosisYear,
                IsFamilyMemberDeceased = request.IsFamilyMemberDeceased,
                DeathDate = request.DeathDate,
                AgeAtDeathYear = request.AgeAtDeathYear,
                CauseOfDeath = NormalizeNullableText(request.CauseOfDeath),
                SourceOfInformation = NormalizeNullableText(request.SourceOfInformation),
                ClinicalNote = NormalizeNullableText(request.ClinicalNote),
                RiskNote = NormalizeNullableText(request.RiskNote),
                ScreeningRecommendation = NormalizeNullableText(request.ScreeningRecommendation),
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

            NormalizeFamilyHistoryData(entity);

            _dbContext.Set<TrxPatientFamilyHistory>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = ToCreateUpdateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientFamilyHistory.CreateFamilyHistory",
                "Membuat data riwayat penyakit keluarga pasien.",
                response
            );

            return Ok(ApiResponse<PatientFamilyHistoryCreateResponse>.Ok(
                response,
                "Riwayat penyakit keluarga pasien berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientFamilyHistoryUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Family History", Description = "Mengubah data riwayat penyakit keluarga pasien", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientFamilyHistory", "Update")]
        public async Task<IActionResult> UpdateFamilyHistory(Guid id, [FromBody] UpdatePatientFamilyHistoryRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientFamilyHistory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Riwayat penyakit keluarga pasien tidak ditemukan."
                ));
            }

            if (entity.FamilyHistoryStatus == PatientFamilyHistoryStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Riwayat penyakit keluarga yang sudah cancelled tidak dapat diubah."
                ));
            }

            var validation = await ValidateUpdateRequestAsync(entity.PatientId, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data riwayat penyakit keluarga pasien tidak valid."
                ));
            }

            var conditionSnapshot = await BuildFamilyHistorySnapshotAsync(
                request.DiagnosisId,
                request.ConditionCode,
                request.ConditionName,
                request.ConditionGroupName,
                request.ConditionMasterType,
                request.IcdVersion
            );

            if (!conditionSnapshot.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    conditionSnapshot.ErrorMessage ?? "Data kondisi riwayat keluarga tidak valid."
                ));
            }

            var duplicateValidation = await ValidateDuplicateFamilyHistoryAsync(
                patientId: entity.PatientId,
                diagnosisId: conditionSnapshot.DiagnosisId,
                relationshipType: request.RelationshipType,
                relationshipSide: request.RelationshipSide,
                conditionName: conditionSnapshot.ConditionName,
                excludeId: id
            );

            if (!duplicateValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    duplicateValidation.ErrorMessage ?? "Data riwayat penyakit keluarga pasien duplikat."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.FamilyMemberPatientId = NormalizeNullableGuid(request.FamilyMemberPatientId);
            entity.DiagnosisId = conditionSnapshot.DiagnosisId;
            entity.RelationshipType = request.RelationshipType;
            entity.RelationshipSide = request.RelationshipSide;
            entity.FamilyMemberNameSnapshot = NormalizeNullableText(request.FamilyMemberNameSnapshot);
            entity.RelationshipDescription = NormalizeNullableText(request.RelationshipDescription);
            entity.IsFirstDegreeRelative = request.IsFirstDegreeRelative;
            entity.IsSecondDegreeRelative = request.IsSecondDegreeRelative;
            entity.IsSameHousehold = request.IsSameHousehold;
            entity.ConditionCode = conditionSnapshot.ConditionCode;
            entity.ConditionName = conditionSnapshot.ConditionName;
            entity.ConditionGroupName = conditionSnapshot.ConditionGroupName;
            entity.ConditionMasterType = conditionSnapshot.ConditionMasterType;
            entity.IcdVersion = conditionSnapshot.IcdVersion;
            entity.IsFromMasterDiagnosis = conditionSnapshot.IsFromMasterDiagnosis;
            entity.FamilyHistoryStatus = request.FamilyHistoryStatus;
            entity.Certainty = request.Certainty;
            entity.RiskLevel = request.RiskLevel;
            entity.IsHereditaryDisease = request.IsHereditaryDisease;
            entity.IsGeneticRisk = request.IsGeneticRisk;
            entity.IsChronicDisease = request.IsChronicDisease || conditionSnapshot.IsChronicDisease;
            entity.IsCancerRelated = request.IsCancerRelated;
            entity.IsCardiovascularRisk = request.IsCardiovascularRisk;
            entity.IsMetabolicRisk = request.IsMetabolicRisk;
            entity.IsMentalHealthRelated = request.IsMentalHealthRelated || conditionSnapshot.IsMentalHealthRelated;
            entity.IsInfectiousDisease = request.IsInfectiousDisease || conditionSnapshot.IsInfectiousDisease;
            entity.IsHighRisk = request.IsHighRisk;
            entity.IsScreeningRecommended = request.IsScreeningRecommended;
            entity.IsAlertEnabled = request.IsAlertEnabled;
            entity.DiagnosedDate = request.DiagnosedDate;
            entity.AgeAtDiagnosisYear = request.AgeAtDiagnosisYear;
            entity.IsFamilyMemberDeceased = request.IsFamilyMemberDeceased;
            entity.DeathDate = request.DeathDate;
            entity.AgeAtDeathYear = request.AgeAtDeathYear;
            entity.CauseOfDeath = NormalizeNullableText(request.CauseOfDeath);
            entity.SourceOfInformation = NormalizeNullableText(request.SourceOfInformation);
            entity.ClinicalNote = NormalizeNullableText(request.ClinicalNote);
            entity.RiskNote = NormalizeNullableText(request.RiskNote);
            entity.ScreeningRecommendation = NormalizeNullableText(request.ScreeningRecommendation);
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            NormalizeFamilyHistoryData(entity);

            await _dbContext.SaveChangesAsync();

            var response = ToUpdateResponse(entity);

            return Ok(ApiResponse<PatientFamilyHistoryUpdateResponse>.Ok(
                response,
                "Riwayat penyakit keluarga pasien berhasil diubah."
            ));
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Verify Patient Family History", Description = "Verifikasi riwayat penyakit keluarga pasien", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PatientFamilyHistory", "Update")]
        public async Task<IActionResult> VerifyFamilyHistory(Guid id, [FromBody] VerifyPatientFamilyHistoryRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientFamilyHistory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Riwayat penyakit keluarga pasien tidak ditemukan."
                ));
            }

            if (entity.FamilyHistoryStatus == PatientFamilyHistoryStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Riwayat penyakit keluarga yang sudah cancelled tidak dapat diverifikasi."
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
                "Riwayat penyakit keluarga pasien berhasil diverifikasi."
            ));
        }

        [HttpPatch("{id:guid}/resolve")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Resolve Patient Family History", Description = "Menyelesaikan status riwayat penyakit keluarga pasien", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("PatientFamilyHistory", "Update")]
        public async Task<IActionResult> ResolveFamilyHistory(Guid id, [FromBody] ResolvePatientFamilyHistoryRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientFamilyHistory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Riwayat penyakit keluarga pasien tidak ditemukan."
                ));
            }

            if (entity.FamilyHistoryStatus == PatientFamilyHistoryStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Riwayat penyakit keluarga yang sudah cancelled tidak dapat diselesaikan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.FamilyHistoryStatus = PatientFamilyHistoryStatus.Resolved;
            entity.IsActive = false;
            entity.IsAlertEnabled = false;
            entity.IsScreeningRecommended = false;
            entity.ResolvedAt = now;
            entity.ResolvedByUserId = actorUserId;
            entity.ResolvedReason = request.ResolvedReason.Trim();
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Riwayat penyakit keluarga pasien berhasil diselesaikan."
            ));
        }

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Cancel Patient Family History", Description = "Membatalkan riwayat penyakit keluarga pasien", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("PatientFamilyHistory", "Update")]
        public async Task<IActionResult> CancelFamilyHistory(Guid id, [FromBody] CancelPatientFamilyHistoryRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientFamilyHistory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Riwayat penyakit keluarga pasien tidak ditemukan."
                ));
            }

            if (entity.FamilyHistoryStatus == PatientFamilyHistoryStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Riwayat penyakit keluarga pasien sudah cancelled."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.FamilyHistoryStatus = PatientFamilyHistoryStatus.Cancelled;
            entity.IsActive = false;
            entity.IsAlertEnabled = false;
            entity.IsScreeningRecommended = false;
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
                "Riwayat penyakit keluarga pasien berhasil dibatalkan."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Patient Family History", Description = "Menghapus data riwayat penyakit keluarga pasien", AccessType = AccessTypes.Delete, SortOrder = 7)]
        [AccessPermission("PatientFamilyHistory", "Delete")]
        public async Task<IActionResult> DeleteFamilyHistory(Guid id)
        {
            var entity = await _dbContext.Set<TrxPatientFamilyHistory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Riwayat penyakit keluarga pasien tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            entity.IsAlertEnabled = false;
            entity.IsScreeningRecommended = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Riwayat penyakit keluarga pasien berhasil dihapus."
            ));
        }

        private IQueryable<TrxPatientFamilyHistory> BuildBaseQuery()
        {
            return _dbContext.Set<TrxPatientFamilyHistory>()
                .Include(x => x.Patient)
                .Include(x => x.FamilyMemberPatient)
                .Include(x => x.Encounter)
                .Include(x => x.Consultation)
                .Include(x => x.Assessment)
                .Include(x => x.Doctor)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.Diagnosis)
                .Include(x => x.VerifiedByUser)
                .Include(x => x.ResolvedByUser)
                .Include(x => x.CancelledByUser)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<TrxPatientFamilyHistory> ApplyFilters(
            IQueryable<TrxPatientFamilyHistory> query,
            string? search,
            Guid? patientId,
            Guid? encounterId,
            Guid? consultationId,
            Guid? assessmentId,
            Guid? doctorId,
            Guid? serviceUnitId,
            Guid? clinicId,
            Guid? familyMemberPatientId,
            Guid? diagnosisId,
            PatientFamilyRelationshipType? relationshipType,
            PatientFamilyRelationshipSide? relationshipSide,
            PatientFamilyHistoryStatus? familyHistoryStatus,
            PatientFamilyHistoryCertainty? certainty,
            PatientFamilyRiskLevel? riskLevel,
            bool? isFromMasterDiagnosis,
            bool? isFirstDegreeRelative,
            bool? isSecondDegreeRelative,
            bool? isSameHousehold,
            bool? isHereditaryDisease,
            bool? isGeneticRisk,
            bool? isChronicDisease,
            bool? isCancerRelated,
            bool? isCardiovascularRisk,
            bool? isMetabolicRisk,
            bool? isMentalHealthRelated,
            bool? isInfectiousDisease,
            bool? isHighRisk,
            bool? isScreeningRecommended,
            bool? isAlertEnabled,
            bool? isVerified,
            bool? isFamilyMemberDeceased,
            bool? isActive,
            DateTime? startDate,
            DateTime? endDate)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.FamilyHistoryRecordNumber.ToLower().Contains(keyword) ||
                    x.ConditionName.ToLower().Contains(keyword) ||
                    (x.ConditionCode != null && x.ConditionCode.ToLower().Contains(keyword)) ||
                    (x.ConditionGroupName != null && x.ConditionGroupName.ToLower().Contains(keyword)) ||
                    (x.FamilyMemberNameSnapshot != null && x.FamilyMemberNameSnapshot.ToLower().Contains(keyword)) ||
                    (x.RelationshipDescription != null && x.RelationshipDescription.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.FamilyMemberPatient != null && x.FamilyMemberPatient.FullName.ToLower().Contains(keyword)) ||
                    (x.FamilyMemberPatient != null && x.FamilyMemberPatient.MedicalRecordNumber.ToLower().Contains(keyword)));
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

            if (familyMemberPatientId.HasValue && familyMemberPatientId.Value != Guid.Empty)
                query = query.Where(x => x.FamilyMemberPatientId == familyMemberPatientId.Value);

            if (diagnosisId.HasValue && diagnosisId.Value != Guid.Empty)
                query = query.Where(x => x.DiagnosisId == diagnosisId.Value);

            if (relationshipType.HasValue)
                query = query.Where(x => x.RelationshipType == relationshipType.Value);

            if (relationshipSide.HasValue)
                query = query.Where(x => x.RelationshipSide == relationshipSide.Value);

            if (familyHistoryStatus.HasValue)
                query = query.Where(x => x.FamilyHistoryStatus == familyHistoryStatus.Value);

            if (certainty.HasValue)
                query = query.Where(x => x.Certainty == certainty.Value);

            if (riskLevel.HasValue)
                query = query.Where(x => x.RiskLevel == riskLevel.Value);

            if (isFromMasterDiagnosis.HasValue)
                query = query.Where(x => x.IsFromMasterDiagnosis == isFromMasterDiagnosis.Value);

            if (isFirstDegreeRelative.HasValue)
                query = query.Where(x => x.IsFirstDegreeRelative == isFirstDegreeRelative.Value);

            if (isSecondDegreeRelative.HasValue)
                query = query.Where(x => x.IsSecondDegreeRelative == isSecondDegreeRelative.Value);

            if (isSameHousehold.HasValue)
                query = query.Where(x => x.IsSameHousehold == isSameHousehold.Value);

            if (isHereditaryDisease.HasValue)
                query = query.Where(x => x.IsHereditaryDisease == isHereditaryDisease.Value);

            if (isGeneticRisk.HasValue)
                query = query.Where(x => x.IsGeneticRisk == isGeneticRisk.Value);

            if (isChronicDisease.HasValue)
                query = query.Where(x => x.IsChronicDisease == isChronicDisease.Value);

            if (isCancerRelated.HasValue)
                query = query.Where(x => x.IsCancerRelated == isCancerRelated.Value);

            if (isCardiovascularRisk.HasValue)
                query = query.Where(x => x.IsCardiovascularRisk == isCardiovascularRisk.Value);

            if (isMetabolicRisk.HasValue)
                query = query.Where(x => x.IsMetabolicRisk == isMetabolicRisk.Value);

            if (isMentalHealthRelated.HasValue)
                query = query.Where(x => x.IsMentalHealthRelated == isMentalHealthRelated.Value);

            if (isInfectiousDisease.HasValue)
                query = query.Where(x => x.IsInfectiousDisease == isInfectiousDisease.Value);

            if (isHighRisk.HasValue)
                query = query.Where(x => x.IsHighRisk == isHighRisk.Value);

            if (isScreeningRecommended.HasValue)
                query = query.Where(x => x.IsScreeningRecommended == isScreeningRecommended.Value);

            if (isAlertEnabled.HasValue)
                query = query.Where(x => x.IsAlertEnabled == isAlertEnabled.Value);

            if (isVerified.HasValue)
                query = query.Where(x => x.IsVerified == isVerified.Value);

            if (isFamilyMemberDeceased.HasValue)
                query = query.Where(x => x.IsFamilyMemberDeceased == isFamilyMemberDeceased.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (startDate.HasValue)
                query = query.Where(x => x.RecordedDateTime >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(x => x.RecordedDateTime < endDate.Value.Date.AddDays(1));

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(CreatePatientFamilyHistoryRequest request)
        {
            if (request.PatientId == Guid.Empty)
                return (false, "PatientId wajib diisi.");

            if (!request.DiagnosisId.HasValue && string.IsNullOrWhiteSpace(request.ConditionName))
                return (false, "ConditionName wajib diisi jika DiagnosisId tidak dipilih.");

            var patientExists = await _dbContext.Set<MstPatient>()
                .AnyAsync(x => x.Id == request.PatientId && !x.IsDelete);

            if (!patientExists)
                return (false, "Pasien tidak ditemukan.");

            var familyMemberPatientId = NormalizeNullableGuid(request.FamilyMemberPatientId);

            if (familyMemberPatientId.HasValue)
            {
                if (familyMemberPatientId.Value == request.PatientId)
                    return (false, "FamilyMemberPatientId tidak boleh sama dengan PatientId.");

                var familyMemberExists = await _dbContext.Set<MstPatient>()
                    .AnyAsync(x => x.Id == familyMemberPatientId.Value && !x.IsDelete);

                if (!familyMemberExists)
                    return (false, "Pasien anggota keluarga tidak ditemukan.");
            }

            return ValidateDateAndAge(
                request.DiagnosedDate,
                request.AgeAtDiagnosisYear,
                request.IsFamilyMemberDeceased,
                request.DeathDate,
                request.AgeAtDeathYear
            );
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateUpdateRequestAsync(Guid patientId, UpdatePatientFamilyHistoryRequest request)
        {
            if (!request.DiagnosisId.HasValue && string.IsNullOrWhiteSpace(request.ConditionName))
                return (false, "ConditionName wajib diisi jika DiagnosisId tidak dipilih.");

            var familyMemberPatientId = NormalizeNullableGuid(request.FamilyMemberPatientId);

            if (familyMemberPatientId.HasValue)
            {
                if (familyMemberPatientId.Value == patientId)
                    return (false, "FamilyMemberPatientId tidak boleh sama dengan PatientId.");

                var familyMemberExists = await _dbContext.Set<MstPatient>()
                    .AnyAsync(x => x.Id == familyMemberPatientId.Value && !x.IsDelete);

                if (!familyMemberExists)
                    return (false, "Pasien anggota keluarga tidak ditemukan.");
            }

            return ValidateDateAndAge(
                request.DiagnosedDate,
                request.AgeAtDiagnosisYear,
                request.IsFamilyMemberDeceased,
                request.DeathDate,
                request.AgeAtDeathYear
            );
        }

        private static (bool IsValid, string? ErrorMessage) ValidateDateAndAge(
            DateTime? diagnosedDate,
            int? ageAtDiagnosisYear,
            bool isFamilyMemberDeceased,
            DateTime? deathDate,
            int? ageAtDeathYear)
        {
            if (ageAtDiagnosisYear.HasValue && (ageAtDiagnosisYear.Value < 0 || ageAtDiagnosisYear.Value > 150))
                return (false, "Usia saat diagnosis harus berada di antara 0 sampai 150 tahun.");

            if (ageAtDeathYear.HasValue && (ageAtDeathYear.Value < 0 || ageAtDeathYear.Value > 150))
                return (false, "Usia saat meninggal harus berada di antara 0 sampai 150 tahun.");

            if (diagnosedDate.HasValue && deathDate.HasValue && diagnosedDate.Value.Date > deathDate.Value.Date)
                return (false, "Tanggal diagnosis tidak boleh lebih besar dari tanggal meninggal.");

            if (isFamilyMemberDeceased && !deathDate.HasValue && !ageAtDeathYear.HasValue)
                return (false, "Jika anggota keluarga meninggal, isi DeathDate atau AgeAtDeathYear.");

            return (true, null);
        }

        private async Task<FamilyHistorySnapshotResult> BuildFamilyHistorySnapshotAsync(
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
                    .FirstOrDefaultAsync(x => x.Id == normalizedDiagnosisId.Value && !x.IsDelete);

                if (diagnosis == null)
                    return FamilyHistorySnapshotResult.Fail("Master diagnosis tidak ditemukan.");

                return new FamilyHistorySnapshotResult
                {
                    IsValid = true,
                    DiagnosisId = diagnosis.Id,
                    ConditionCode = diagnosis.DiagnosisCode,
                    ConditionName = diagnosis.DiagnosisName,
                    ConditionGroupName = diagnosis.DiagnosisGroupName,
                    ConditionMasterType = "ICD10",
                    IcdVersion = diagnosis.IcdVersion,
                    IsFromMasterDiagnosis = true,
                    IsChronicDisease = diagnosis.IsChronicDisease,
                    IsInfectiousDisease = diagnosis.IsInfectiousDisease,
                    IsMentalHealthRelated = diagnosis.IsMentalHealthRelated
                };
            }

            var normalizedName = NormalizeNullableText(conditionName);

            if (string.IsNullOrWhiteSpace(normalizedName))
                return FamilyHistorySnapshotResult.Fail("Nama kondisi wajib diisi.");

            return new FamilyHistorySnapshotResult
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
                IsMentalHealthRelated = false
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateDuplicateFamilyHistoryAsync(
            Guid patientId,
            Guid? diagnosisId,
            PatientFamilyRelationshipType relationshipType,
            PatientFamilyRelationshipSide relationshipSide,
            string conditionName,
            Guid? excludeId)
        {
            var query = _dbContext.Set<TrxPatientFamilyHistory>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.PatientId == patientId &&
                    x.FamilyHistoryStatus != PatientFamilyHistoryStatus.Cancelled &&
                    x.FamilyHistoryStatus != PatientFamilyHistoryStatus.EnteredInError);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            if (diagnosisId.HasValue)
            {
                var duplicateByDiagnosis = await query.AnyAsync(x =>
                    x.DiagnosisId == diagnosisId.Value &&
                    x.RelationshipType == relationshipType &&
                    x.RelationshipSide == relationshipSide);

                if (duplicateByDiagnosis)
                    return (false, "Riwayat keluarga dengan diagnosis dan hubungan keluarga yang sama sudah tercatat untuk pasien ini.");
            }

            var normalizedName = conditionName.Trim().ToLower();

            var duplicateByName = await query.AnyAsync(x =>
                x.RelationshipType == relationshipType &&
                x.RelationshipSide == relationshipSide &&
                x.ConditionName.ToLower() == normalizedName);

            if (duplicateByName)
                return (false, "Riwayat keluarga dengan hubungan dan nama kondisi yang sama sudah tercatat untuk pasien ini.");

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

        private async Task<string> GenerateFamilyHistoryRecordNumberAsync(DateTime now)
        {
            var prefix = $"PFH-{now:yyyyMMdd}";

            var countToday = await _dbContext.Set<TrxPatientFamilyHistory>()
                .CountAsync(x => x.FamilyHistoryRecordNumber.StartsWith(prefix));

            return $"{prefix}-{countToday + 1:0000}";
        }

        private static IQueryable<TrxPatientFamilyHistory> ApplySorting(
            IQueryable<TrxPatientFamilyHistory> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "recordedDateTime").ToLowerInvariant() switch
            {
                "conditionname" => isDesc ? query.OrderByDescending(x => x.ConditionName) : query.OrderBy(x => x.ConditionName),
                "conditioncode" => isDesc ? query.OrderByDescending(x => x.ConditionCode) : query.OrderBy(x => x.ConditionCode),
                "relationshiptype" => isDesc ? query.OrderByDescending(x => x.RelationshipType) : query.OrderBy(x => x.RelationshipType),
                "relationshipside" => isDesc ? query.OrderByDescending(x => x.RelationshipSide) : query.OrderBy(x => x.RelationshipSide),
                "familyhistorystatus" => isDesc ? query.OrderByDescending(x => x.FamilyHistoryStatus) : query.OrderBy(x => x.FamilyHistoryStatus),
                "certainty" => isDesc ? query.OrderByDescending(x => x.Certainty) : query.OrderBy(x => x.Certainty),
                "risklevel" => isDesc ? query.OrderByDescending(x => x.RiskLevel) : query.OrderBy(x => x.RiskLevel),
                "ishereditarydisease" => isDesc ? query.OrderByDescending(x => x.IsHereditaryDisease) : query.OrderBy(x => x.IsHereditaryDisease),
                "isgeneticrisk" => isDesc ? query.OrderByDescending(x => x.IsGeneticRisk) : query.OrderBy(x => x.IsGeneticRisk),
                "ishighrisk" => isDesc ? query.OrderByDescending(x => x.IsHighRisk) : query.OrderBy(x => x.IsHighRisk),
                "isscreeningrecommended" => isDesc ? query.OrderByDescending(x => x.IsScreeningRecommended) : query.OrderBy(x => x.IsScreeningRecommended),
                "isverified" => isDesc ? query.OrderByDescending(x => x.IsVerified) : query.OrderBy(x => x.IsVerified),
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDesc
                    ? query.OrderByDescending(x => x.RecordedDateTime)
                        .ThenByDescending(x => x.IsHighRisk)
                        .ThenByDescending(x => x.RiskLevel)
                    : query.OrderBy(x => x.RecordedDateTime)
                        .ThenByDescending(x => x.IsHighRisk)
                        .ThenByDescending(x => x.RiskLevel)
            };
        }

        private static PatientFamilyHistoryResponse ToResponse(TrxPatientFamilyHistory x)
        {
            return new PatientFamilyHistoryResponse
            {
                Id = x.Id,
                FamilyHistoryRecordNumber = x.FamilyHistoryRecordNumber,
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
                FamilyMemberPatientId = x.FamilyMemberPatientId,
                FamilyMemberPatientName = x.FamilyMemberPatient != null ? x.FamilyMemberPatient.FullName : null,
                FamilyMemberMedicalRecordNumber = x.FamilyMemberPatient != null ? x.FamilyMemberPatient.MedicalRecordNumber : null,
                DiagnosisId = x.DiagnosisId,
                RelationshipType = x.RelationshipType,
                RelationshipSide = x.RelationshipSide,
                FamilyMemberNameSnapshot = x.FamilyMemberNameSnapshot,
                RelationshipDescription = x.RelationshipDescription,
                IsFirstDegreeRelative = x.IsFirstDegreeRelative,
                IsSecondDegreeRelative = x.IsSecondDegreeRelative,
                IsSameHousehold = x.IsSameHousehold,
                ConditionCode = x.ConditionCode,
                ConditionName = x.ConditionName,
                ConditionGroupName = x.ConditionGroupName,
                ConditionMasterType = x.ConditionMasterType,
                IcdVersion = x.IcdVersion,
                IsFromMasterDiagnosis = x.IsFromMasterDiagnosis,
                FamilyHistoryStatus = x.FamilyHistoryStatus,
                Certainty = x.Certainty,
                RiskLevel = x.RiskLevel,
                IsHereditaryDisease = x.IsHereditaryDisease,
                IsGeneticRisk = x.IsGeneticRisk,
                IsChronicDisease = x.IsChronicDisease,
                IsCancerRelated = x.IsCancerRelated,
                IsCardiovascularRisk = x.IsCardiovascularRisk,
                IsMetabolicRisk = x.IsMetabolicRisk,
                IsMentalHealthRelated = x.IsMentalHealthRelated,
                IsInfectiousDisease = x.IsInfectiousDisease,
                IsHighRisk = x.IsHighRisk,
                IsScreeningRecommended = x.IsScreeningRecommended,
                IsAlertEnabled = x.IsAlertEnabled,
                RecordedDateTime = x.RecordedDateTime,
                DiagnosedDate = x.DiagnosedDate,
                AgeAtDiagnosisYear = x.AgeAtDiagnosisYear,
                IsFamilyMemberDeceased = x.IsFamilyMemberDeceased,
                DeathDate = x.DeathDate,
                AgeAtDeathYear = x.AgeAtDeathYear,
                CauseOfDeath = x.CauseOfDeath,
                SourceOfInformation = x.SourceOfInformation,
                IsVerified = x.IsVerified,
                VerifiedAt = x.VerifiedAt,
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByUserName = x.VerifiedByUser != null ? x.VerifiedByUser.DisplayName : null,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static PatientFamilyHistoryDetailResponse ToDetailResponse(TrxPatientFamilyHistory x)
        {
            return new PatientFamilyHistoryDetailResponse
            {
                Id = x.Id,
                FamilyHistoryRecordNumber = x.FamilyHistoryRecordNumber,
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
                FamilyMemberPatientId = x.FamilyMemberPatientId,
                FamilyMemberPatientName = x.FamilyMemberPatient != null ? x.FamilyMemberPatient.FullName : null,
                FamilyMemberMedicalRecordNumber = x.FamilyMemberPatient != null ? x.FamilyMemberPatient.MedicalRecordNumber : null,
                DiagnosisId = x.DiagnosisId,
                RelationshipType = x.RelationshipType,
                RelationshipSide = x.RelationshipSide,
                FamilyMemberNameSnapshot = x.FamilyMemberNameSnapshot,
                RelationshipDescription = x.RelationshipDescription,
                IsFirstDegreeRelative = x.IsFirstDegreeRelative,
                IsSecondDegreeRelative = x.IsSecondDegreeRelative,
                IsSameHousehold = x.IsSameHousehold,
                ConditionCode = x.ConditionCode,
                ConditionName = x.ConditionName,
                ConditionGroupName = x.ConditionGroupName,
                ConditionMasterType = x.ConditionMasterType,
                IcdVersion = x.IcdVersion,
                IsFromMasterDiagnosis = x.IsFromMasterDiagnosis,
                FamilyHistoryStatus = x.FamilyHistoryStatus,
                Certainty = x.Certainty,
                RiskLevel = x.RiskLevel,
                IsHereditaryDisease = x.IsHereditaryDisease,
                IsGeneticRisk = x.IsGeneticRisk,
                IsChronicDisease = x.IsChronicDisease,
                IsCancerRelated = x.IsCancerRelated,
                IsCardiovascularRisk = x.IsCardiovascularRisk,
                IsMetabolicRisk = x.IsMetabolicRisk,
                IsMentalHealthRelated = x.IsMentalHealthRelated,
                IsInfectiousDisease = x.IsInfectiousDisease,
                IsHighRisk = x.IsHighRisk,
                IsScreeningRecommended = x.IsScreeningRecommended,
                IsAlertEnabled = x.IsAlertEnabled,
                RecordedDateTime = x.RecordedDateTime,
                DiagnosedDate = x.DiagnosedDate,
                AgeAtDiagnosisYear = x.AgeAtDiagnosisYear,
                IsFamilyMemberDeceased = x.IsFamilyMemberDeceased,
                DeathDate = x.DeathDate,
                AgeAtDeathYear = x.AgeAtDeathYear,
                CauseOfDeath = x.CauseOfDeath,
                SourceOfInformation = x.SourceOfInformation,
                ClinicalNote = x.ClinicalNote,
                RiskNote = x.RiskNote,
                ScreeningRecommendation = x.ScreeningRecommendation,
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

        private static PatientFamilyHistoryCreateResponse ToCreateUpdateResponse(TrxPatientFamilyHistory x)
        {
            return new PatientFamilyHistoryCreateResponse
            {
                Id = x.Id,
                FamilyHistoryRecordNumber = x.FamilyHistoryRecordNumber,
                PatientId = x.PatientId,
                EncounterId = x.EncounterId,
                ConsultationId = x.ConsultationId,
                AssessmentId = x.AssessmentId,
                FamilyMemberPatientId = x.FamilyMemberPatientId,
                DiagnosisId = x.DiagnosisId,
                RelationshipType = x.RelationshipType,
                RelationshipSide = x.RelationshipSide,
                RelationshipDescription = x.RelationshipDescription,
                ConditionCode = x.ConditionCode,
                ConditionName = x.ConditionName,
                ConditionGroupName = x.ConditionGroupName,
                ConditionMasterType = x.ConditionMasterType,
                IsFromMasterDiagnosis = x.IsFromMasterDiagnosis,
                FamilyHistoryStatus = x.FamilyHistoryStatus,
                Certainty = x.Certainty,
                RiskLevel = x.RiskLevel,
                IsHereditaryDisease = x.IsHereditaryDisease,
                IsGeneticRisk = x.IsGeneticRisk,
                IsHighRisk = x.IsHighRisk,
                IsScreeningRecommended = x.IsScreeningRecommended,
                IsAlertEnabled = x.IsAlertEnabled,
                IsVerified = x.IsVerified
            };
        }

        private static PatientFamilyHistoryUpdateResponse ToUpdateResponse(TrxPatientFamilyHistory x)
        {
            return new PatientFamilyHistoryUpdateResponse
            {
                Id = x.Id,
                FamilyHistoryRecordNumber = x.FamilyHistoryRecordNumber,
                PatientId = x.PatientId,
                EncounterId = x.EncounterId,
                ConsultationId = x.ConsultationId,
                AssessmentId = x.AssessmentId,
                FamilyMemberPatientId = x.FamilyMemberPatientId,
                DiagnosisId = x.DiagnosisId,
                RelationshipType = x.RelationshipType,
                RelationshipSide = x.RelationshipSide,
                RelationshipDescription = x.RelationshipDescription,
                ConditionCode = x.ConditionCode,
                ConditionName = x.ConditionName,
                ConditionGroupName = x.ConditionGroupName,
                ConditionMasterType = x.ConditionMasterType,
                IsFromMasterDiagnosis = x.IsFromMasterDiagnosis,
                FamilyHistoryStatus = x.FamilyHistoryStatus,
                Certainty = x.Certainty,
                RiskLevel = x.RiskLevel,
                IsHereditaryDisease = x.IsHereditaryDisease,
                IsGeneticRisk = x.IsGeneticRisk,
                IsHighRisk = x.IsHighRisk,
                IsScreeningRecommended = x.IsScreeningRecommended,
                IsAlertEnabled = x.IsAlertEnabled,
                IsVerified = x.IsVerified
            };
        }

        private static void NormalizeFamilyHistoryData(TrxPatientFamilyHistory entity)
        {
            if (entity.RelationshipType == PatientFamilyRelationshipType.Father ||
                entity.RelationshipType == PatientFamilyRelationshipType.Mother ||
                entity.RelationshipType == PatientFamilyRelationshipType.Brother ||
                entity.RelationshipType == PatientFamilyRelationshipType.Sister ||
                entity.RelationshipType == PatientFamilyRelationshipType.Son ||
                entity.RelationshipType == PatientFamilyRelationshipType.Daughter)
            {
                entity.IsFirstDegreeRelative = true;
            }

            if (entity.RelationshipType == PatientFamilyRelationshipType.Grandfather ||
                entity.RelationshipType == PatientFamilyRelationshipType.Grandmother ||
                entity.RelationshipType == PatientFamilyRelationshipType.Uncle ||
                entity.RelationshipType == PatientFamilyRelationshipType.Aunt)
            {
                entity.IsSecondDegreeRelative = true;
            }

            if (entity.IsGeneticRisk)
                entity.IsHereditaryDisease = true;

            if (entity.IsHereditaryDisease || entity.IsCancerRelated || entity.IsCardiovascularRisk || entity.IsMetabolicRisk)
                entity.IsScreeningRecommended = true;

            if (entity.RiskLevel == PatientFamilyRiskLevel.High || entity.RiskLevel == PatientFamilyRiskLevel.VeryHigh)
                entity.IsHighRisk = true;

            if (entity.IsHighRisk || entity.IsScreeningRecommended)
                entity.IsAlertEnabled = true;

            if (entity.FamilyHistoryStatus == PatientFamilyHistoryStatus.Inactive ||
                entity.FamilyHistoryStatus == PatientFamilyHistoryStatus.Resolved ||
                entity.FamilyHistoryStatus == PatientFamilyHistoryStatus.EnteredInError ||
                entity.FamilyHistoryStatus == PatientFamilyHistoryStatus.Cancelled)
            {
                entity.IsAlertEnabled = false;
                entity.IsScreeningRecommended = false;
            }

            if (!entity.IsActive)
            {
                entity.IsAlertEnabled = false;
                entity.IsScreeningRecommended = false;
            }

            if (!entity.IsFamilyMemberDeceased)
            {
                entity.DeathDate = null;
                entity.AgeAtDeathYear = null;
                entity.CauseOfDeath = null;
            }
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<PatientFamilyHistoryEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new PatientFamilyHistoryEnumOptionResponse
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

        private class FamilyHistorySnapshotResult
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

            public bool IsMentalHealthRelated { get; set; }

            public static FamilyHistorySnapshotResult Fail(string errorMessage)
            {
                return new FamilyHistorySnapshotResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}
