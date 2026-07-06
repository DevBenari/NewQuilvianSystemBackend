using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponsePatientDiagnosisPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs.PatientDiagnosisResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/patient-diagnoses")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Patient Diagnosis",
        AreaName = "HealthServices",
        ControllerName = "PatientDiagnosis",
        Description = "Diagnosis ICD pasien berdasarkan konsultasi dokter",
        SortOrder = 3
    )]
    [Tags("Health Services / Clinical Management / Patient Diagnosis")]
    public class PatientDiagnosisController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Clinical";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PatientDiagnosisController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientDiagnosisFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Diagnosis", Description = "Melihat metadata filter diagnosis pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientDiagnosis", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientDiagnosisFilterMetadataResponse
            {
                DefaultFilter = new PatientDiagnosisDefaultFilterResponse(),
                SortOptions = new List<PatientDiagnosisSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "diagnosisCode", Label = "Kode diagnosis" },
                    new() { Value = "diagnosisName", Label = "Nama diagnosis" },
                    new() { Value = "diagnosisDateTime", Label = "Tanggal diagnosis" },
                    new() { Value = "diagnosisType", Label = "Tipe diagnosis" },
                    new() { Value = "diagnosisStatus", Label = "Status diagnosis" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                DiagnosisTypeOptions = BuildEnumOptions<PatientDiagnosisType>(),
                DiagnosisStatusOptions = BuildEnumOptions<PatientDiagnosisStatus>()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientDiagnosis.GetFilterMetadata",
                "Mengambil metadata filter diagnosis pasien.",
                result
            );

            return Ok(ApiResponse<PatientDiagnosisFilterMetadataResponse>.Ok(
                result,
                "Metadata filter diagnosis pasien berhasil diambil."
            ));
        }

        [HttpGet("master-options")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientDiagnosisMasterOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Diagnosis", Description = "Melihat pilihan master diagnosis ICD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientDiagnosis", "Read")]
        public async Task<IActionResult> GetMasterDiagnosisOptions(
            [FromQuery] string? search,
            [FromQuery] Guid? diagnosisChapterId,
            [FromQuery] bool onlySelectable = true,
            [FromQuery] bool onlyActive = true,
            [FromQuery] int take = 50)
        {
            if (take <= 0) take = 50;
            if (take > 100) take = 100;

            var query = _dbContext.Set<MstDiagnosis>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (onlySelectable)
                query = query.Where(x => x.IsSelectableForClinicalUse);

            if (diagnosisChapterId.HasValue && diagnosisChapterId.Value != Guid.Empty)
                query = query.Where(x => x.DiagnosisChapterId == diagnosisChapterId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.DiagnosisCode.ToLower().Contains(keyword) ||
                    x.DiagnosisName.ToLower().Contains(keyword));
            }

            var data = await query
                .OrderBy(x => x.DiagnosisCode)
                .Take(take)
                .Select(x => new PatientDiagnosisMasterOptionResponse
                {
                    Id = x.Id,
                    DiagnosisCode = x.DiagnosisCode,
                    DiagnosisName = x.DiagnosisName,
                    DiagnosisType = x.DiagnosisType,
                    IcdVersion = x.IcdVersion,
                    IsSelectableForClinicalUse = x.IsSelectableForClinicalUse,
                    IsPrimaryDiagnosisAllowed = x.IsPrimaryDiagnosisAllowed,
                    IsSecondaryDiagnosisAllowed = x.IsSecondaryDiagnosisAllowed
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientDiagnosisMasterOptionResponse>>.Ok(
                data,
                "Data pilihan master diagnosis berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientDiagnosisPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Diagnosis", Description = "Melihat data diagnosis pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientDiagnosis", "Read")]
        public async Task<IActionResult> GetDiagnoses(
            [FromQuery] string? search,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? consultationId,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? doctorId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? diagnosisId,
            [FromQuery] PatientDiagnosisType? diagnosisType,
            [FromQuery] PatientDiagnosisStatus? diagnosisStatus,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isConfirmed,
            [FromQuery] bool? isFromMasterDiagnosis,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyFilters(
                query,
                search,
                encounterId,
                consultationId,
                patientId,
                doctorId,
                serviceUnitId,
                clinicId,
                diagnosisId,
                diagnosisType,
                diagnosisStatus,
                isPrimary,
                isConfirmed,
                isFromMasterDiagnosis,
                startDate,
                endDate
            );

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(ToResponse).ToList();

            return Ok(ApiResponse<ResponsePatientDiagnosisPagedResult>.Ok(
                new ResponsePatientDiagnosisPagedResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data diagnosis pasien berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientDiagnosisOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Diagnosis", Description = "Melihat pilihan diagnosis pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientDiagnosis", "Read")]
        public async Task<IActionResult> GetDiagnosisOptions(
            [FromQuery] Guid? consultationId,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? patientId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<TrxPatientDiagnosis>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive && x.DiagnosisStatus != PatientDiagnosisStatus.Cancelled);

            if (consultationId.HasValue && consultationId.Value != Guid.Empty)
                query = query.Where(x => x.ConsultationId == consultationId.Value);

            if (encounterId.HasValue && encounterId.Value != Guid.Empty)
                query = query.Where(x => x.EncounterId == encounterId.Value);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.DiagnosisCode.ToLower().Contains(keyword) ||
                    x.DiagnosisName.ToLower().Contains(keyword));
            }

            var data = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.DiagnosisCode)
                .Take(100)
                .Select(x => new PatientDiagnosisOptionResponse
                {
                    Id = x.Id,
                    DiagnosisId = x.DiagnosisId,
                    DiagnosisCode = x.DiagnosisCode,
                    DiagnosisName = x.DiagnosisName,
                    DiagnosisMasterType = x.DiagnosisMasterType,
                    IcdVersion = x.IcdVersion,
                    DiagnosisType = x.DiagnosisType,
                    DiagnosisStatus = x.DiagnosisStatus,
                    IsPrimary = x.IsPrimary,
                    IsConfirmed = x.IsConfirmed,
                    IsFromMasterDiagnosis = x.IsFromMasterDiagnosis
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientDiagnosisOptionResponse>>.Ok(
                data,
                "Data pilihan diagnosis pasien berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientDiagnosisDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Diagnosis", Description = "Melihat detail diagnosis pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientDiagnosis", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Diagnosis pasien tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientDiagnosisDetailResponse>.Ok(
                ToDetailResponse(entity),
                "Detail diagnosis pasien berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientDiagnosisCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Patient Diagnosis", Description = "Membuat diagnosis pasien", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PatientDiagnosis", "Create")]
        public async Task<IActionResult> CreateDiagnosis([FromBody] CreatePatientDiagnosisRequest request)
        {
            var validation = await ValidateCreateRequestAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data diagnosis pasien tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                .FirstAsync(x =>
                    x.Id == request.ConsultationId &&
                    x.EncounterId == request.EncounterId &&
                    !x.IsDelete);

            var diagnosisSnapshot = await BuildDiagnosisSnapshotAsync(
                request.DiagnosisId,
                request.DiagnosisCode,
                request.DiagnosisName,
                request.DiagnosisMasterType,
                request.IcdVersion,
                request.IsPrimary ? PatientDiagnosisType.Primary : request.DiagnosisType
            );

            if (!diagnosisSnapshot.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    diagnosisSnapshot.ErrorMessage ?? "Diagnosis tidak valid."
                ));
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            if (request.IsPrimary)
            {
                await ClearPrimaryDiagnosisAsync(request.ConsultationId, actorUserId, now);
            }

            var entity = new TrxPatientDiagnosis
            {
                Id = Guid.NewGuid(),
                EncounterId = consultation.EncounterId,
                ConsultationId = consultation.Id,
                PatientId = consultation.PatientId,
                DoctorId = consultation.DoctorId,
                ServiceUnitId = consultation.ServiceUnitId,
                ClinicId = consultation.ClinicId,
                DiagnosisId = diagnosisSnapshot.DiagnosisId,
                DiagnosisCode = diagnosisSnapshot.DiagnosisCode,
                DiagnosisName = diagnosisSnapshot.DiagnosisName,
                DiagnosisMasterType = diagnosisSnapshot.DiagnosisMasterType,
                IcdVersion = diagnosisSnapshot.IcdVersion,
                DiagnosisType = request.IsPrimary ? PatientDiagnosisType.Primary : request.DiagnosisType,
                DiagnosisStatus = PatientDiagnosisStatus.Active,
                IsPrimary = request.IsPrimary,
                IsChronic = request.IsChronic,
                IsNewCase = request.IsNewCase,
                IsConfirmed = request.IsConfirmed,
                IsFromMasterDiagnosis = diagnosisSnapshot.IsFromMasterDiagnosis,
                DiagnosisDateTime = now,
                OnsetDate = request.OnsetDate,
                ClinicalNote = NormalizeNullableText(request.ClinicalNote),
                AssessmentNote = NormalizeNullableText(request.AssessmentNote),
                PlanNote = NormalizeNullableText(request.PlanNote),
                DifferentialDiagnosisNote = NormalizeNullableText(request.DifferentialDiagnosisNote),
                SupportingFindingNote = NormalizeNullableText(request.SupportingFindingNote),
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<TrxPatientDiagnosis>().Add(entity);

            await _dbContext.SaveChangesAsync();

            var summary = await UpdateConsultationDiagnosisSummaryAsync(
                consultation.Id,
                actorUserId,
                now
            );

            await transaction.CommitAsync();

            var response = new PatientDiagnosisCreateResponse
            {
                Id = entity.Id,
                EncounterId = entity.EncounterId,
                ConsultationId = entity.ConsultationId,
                DiagnosisId = entity.DiagnosisId,
                DiagnosisCode = entity.DiagnosisCode,
                DiagnosisName = entity.DiagnosisName,
                DiagnosisMasterType = entity.DiagnosisMasterType,
                IcdVersion = entity.IcdVersion,
                DiagnosisType = entity.DiagnosisType,
                DiagnosisStatus = entity.DiagnosisStatus,
                IsPrimary = entity.IsPrimary,
                IsConfirmed = entity.IsConfirmed,
                IsFromMasterDiagnosis = entity.IsFromMasterDiagnosis,
                DiagnosisCount = summary.DiagnosisCount,
                HasPrimaryDiagnosis = summary.HasPrimaryDiagnosis,
                DiagnosisText = summary.DiagnosisText,
                PrimaryDiagnosisText = summary.PrimaryDiagnosisText,
                SecondaryDiagnosisText = summary.SecondaryDiagnosisText
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientDiagnosis.CreateDiagnosis",
                "Membuat diagnosis pasien.",
                response
            );

            return Ok(ApiResponse<PatientDiagnosisCreateResponse>.Ok(
                response,
                "Diagnosis pasien berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientDiagnosisUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Diagnosis", Description = "Mengubah diagnosis pasien", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientDiagnosis", "Update")]
        public async Task<IActionResult> UpdateDiagnosis(Guid id, [FromBody] UpdatePatientDiagnosisRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientDiagnosis>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Diagnosis pasien tidak ditemukan."
                ));
            }

            if (entity.DiagnosisStatus == PatientDiagnosisStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Diagnosis yang sudah cancelled tidak dapat diubah."
                ));
            }

            var diagnosisSnapshot = await BuildDiagnosisSnapshotAsync(
                request.DiagnosisId,
                request.DiagnosisCode,
                request.DiagnosisName,
                request.DiagnosisMasterType,
                request.IcdVersion,
                request.IsPrimary ? PatientDiagnosisType.Primary : request.DiagnosisType
            );

            if (!diagnosisSnapshot.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    diagnosisSnapshot.ErrorMessage ?? "Diagnosis tidak valid."
                ));
            }

            var duplicateValidation = await ValidateDuplicateDiagnosisAsync(
                entity.ConsultationId,
                diagnosisSnapshot.DiagnosisCode,
                diagnosisSnapshot.DiagnosisId,
                id
            );

            if (!duplicateValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    duplicateValidation.ErrorMessage ?? "Diagnosis pasien duplikat."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            if (request.IsPrimary)
            {
                await ClearPrimaryDiagnosisAsync(entity.ConsultationId, actorUserId, now, entity.Id);
            }

            entity.DiagnosisId = diagnosisSnapshot.DiagnosisId;
            entity.DiagnosisCode = diagnosisSnapshot.DiagnosisCode;
            entity.DiagnosisName = diagnosisSnapshot.DiagnosisName;
            entity.DiagnosisMasterType = diagnosisSnapshot.DiagnosisMasterType;
            entity.IcdVersion = diagnosisSnapshot.IcdVersion;
            entity.DiagnosisType = request.IsPrimary ? PatientDiagnosisType.Primary : request.DiagnosisType;
            entity.IsPrimary = request.IsPrimary;
            entity.IsChronic = request.IsChronic;
            entity.IsNewCase = request.IsNewCase;
            entity.IsConfirmed = request.IsConfirmed;
            entity.IsFromMasterDiagnosis = diagnosisSnapshot.IsFromMasterDiagnosis;
            entity.OnsetDate = request.OnsetDate;
            entity.ClinicalNote = NormalizeNullableText(request.ClinicalNote);
            entity.AssessmentNote = NormalizeNullableText(request.AssessmentNote);
            entity.PlanNote = NormalizeNullableText(request.PlanNote);
            entity.DifferentialDiagnosisNote = NormalizeNullableText(request.DifferentialDiagnosisNote);
            entity.SupportingFindingNote = NormalizeNullableText(request.SupportingFindingNote);
            entity.SortOrder = request.SortOrder;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var summary = await UpdateConsultationDiagnosisSummaryAsync(
                entity.ConsultationId,
                actorUserId,
                now
            );

            await transaction.CommitAsync();

            var response = new PatientDiagnosisUpdateResponse
            {
                Id = entity.Id,
                EncounterId = entity.EncounterId,
                ConsultationId = entity.ConsultationId,
                DiagnosisId = entity.DiagnosisId,
                DiagnosisCode = entity.DiagnosisCode,
                DiagnosisName = entity.DiagnosisName,
                DiagnosisMasterType = entity.DiagnosisMasterType,
                IcdVersion = entity.IcdVersion,
                DiagnosisType = entity.DiagnosisType,
                DiagnosisStatus = entity.DiagnosisStatus,
                IsPrimary = entity.IsPrimary,
                IsConfirmed = entity.IsConfirmed,
                IsFromMasterDiagnosis = entity.IsFromMasterDiagnosis,
                DiagnosisCount = summary.DiagnosisCount,
                HasPrimaryDiagnosis = summary.HasPrimaryDiagnosis,
                DiagnosisText = summary.DiagnosisText,
                PrimaryDiagnosisText = summary.PrimaryDiagnosisText,
                SecondaryDiagnosisText = summary.SecondaryDiagnosisText
            };

            return Ok(ApiResponse<PatientDiagnosisUpdateResponse>.Ok(
                response,
                "Diagnosis pasien berhasil diubah."
            ));
        }

        [HttpPatch("{id:guid}/set-primary")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Set Primary Patient Diagnosis", Description = "Menandai diagnosis utama pasien", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PatientDiagnosis", "Update")]
        public async Task<IActionResult> SetPrimary(Guid id, [FromBody] SetPrimaryPatientDiagnosisRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientDiagnosis>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Diagnosis pasien tidak ditemukan."
                ));
            }

            if (entity.DiagnosisStatus == PatientDiagnosisStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Diagnosis yang sudah cancelled tidak dapat dijadikan diagnosis utama."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            if (request.IsPrimary)
            {
                await ClearPrimaryDiagnosisAsync(entity.ConsultationId, actorUserId, now, entity.Id);
                entity.IsPrimary = true;
                entity.DiagnosisType = PatientDiagnosisType.Primary;
            }
            else
            {
                entity.IsPrimary = false;

                if (entity.DiagnosisType == PatientDiagnosisType.Primary)
                    entity.DiagnosisType = PatientDiagnosisType.Secondary;
            }

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await UpdateConsultationDiagnosisSummaryAsync(
                entity.ConsultationId,
                actorUserId,
                now
            );

            await transaction.CommitAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                request.IsPrimary
                    ? "Diagnosis berhasil ditandai sebagai diagnosis utama."
                    : "Diagnosis utama berhasil dilepas."
            ));
        }

        [HttpPatch("{id:guid}/resolve")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Resolve Patient Diagnosis", Description = "Menandai diagnosis pasien sudah resolved", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("PatientDiagnosis", "Update")]
        public async Task<IActionResult> ResolveDiagnosis(Guid id, [FromBody] ResolvePatientDiagnosisRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientDiagnosis>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Diagnosis pasien tidak ditemukan."
                ));
            }

            if (entity.DiagnosisStatus == PatientDiagnosisStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Diagnosis yang sudah cancelled tidak dapat di-resolve."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.DiagnosisStatus = PatientDiagnosisStatus.Resolved;
            entity.ResolvedAt = now;
            entity.ResolvedByUserId = actorUserId;
            entity.ResolvedReason = request.ResolvedReason.Trim();
            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Diagnosis pasien berhasil ditandai resolved."
            ));
        }

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Cancel Patient Diagnosis", Description = "Membatalkan diagnosis pasien", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("PatientDiagnosis", "Update")]
        public async Task<IActionResult> CancelDiagnosis(Guid id, [FromBody] CancelPatientDiagnosisRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientDiagnosis>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Diagnosis pasien tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            entity.DiagnosisStatus = PatientDiagnosisStatus.Cancelled;
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

            await UpdateConsultationDiagnosisSummaryAsync(
                entity.ConsultationId,
                actorUserId,
                now
            );

            await transaction.CommitAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Diagnosis pasien berhasil dibatalkan."
            ));
        }

        private IQueryable<TrxPatientDiagnosis> BuildBaseQuery()
        {
            return _dbContext.Set<TrxPatientDiagnosis>()
                .Include(x => x.Encounter)
                .Include(x => x.Consultation)
                .Include(x => x.Patient)
                .Include(x => x.Doctor)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.Diagnosis)
                .Include(x => x.ResolvedByUser)
                .Include(x => x.CancelledByUser)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<TrxPatientDiagnosis> ApplyFilters(
            IQueryable<TrxPatientDiagnosis> query,
            string? search,
            Guid? encounterId,
            Guid? consultationId,
            Guid? patientId,
            Guid? doctorId,
            Guid? serviceUnitId,
            Guid? clinicId,
            Guid? diagnosisId,
            PatientDiagnosisType? diagnosisType,
            PatientDiagnosisStatus? diagnosisStatus,
            bool? isPrimary,
            bool? isConfirmed,
            bool? isFromMasterDiagnosis,
            DateTime? startDate,
            DateTime? endDate)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.DiagnosisCode.ToLower().Contains(keyword) ||
                    x.DiagnosisName.ToLower().Contains(keyword) ||
                    (x.DiagnosisMasterType != null && x.DiagnosisMasterType.ToLower().Contains(keyword)) ||
                    (x.IcdVersion != null && x.IcdVersion.ToLower().Contains(keyword)) ||
                    (x.Encounter != null && x.Encounter.EncounterNumber.ToLower().Contains(keyword)) ||
                    (x.Consultation != null && x.Consultation.ConsultationNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.FullName.ToLower().Contains(keyword)));
            }

            if (encounterId.HasValue && encounterId.Value != Guid.Empty)
                query = query.Where(x => x.EncounterId == encounterId.Value);

            if (consultationId.HasValue && consultationId.Value != Guid.Empty)
                query = query.Where(x => x.ConsultationId == consultationId.Value);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (doctorId.HasValue && doctorId.Value != Guid.Empty)
                query = query.Where(x => x.DoctorId == doctorId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
                query = query.Where(x => x.ClinicId == clinicId.Value);

            if (diagnosisId.HasValue && diagnosisId.Value != Guid.Empty)
                query = query.Where(x => x.DiagnosisId == diagnosisId.Value);

            if (diagnosisType.HasValue)
                query = query.Where(x => x.DiagnosisType == diagnosisType.Value);

            if (diagnosisStatus.HasValue)
                query = query.Where(x => x.DiagnosisStatus == diagnosisStatus.Value);

            if (isPrimary.HasValue)
                query = query.Where(x => x.IsPrimary == isPrimary.Value);

            if (isConfirmed.HasValue)
                query = query.Where(x => x.IsConfirmed == isConfirmed.Value);

            if (isFromMasterDiagnosis.HasValue)
                query = query.Where(x => x.IsFromMasterDiagnosis == isFromMasterDiagnosis.Value);

            if (startDate.HasValue)
                query = query.Where(x => x.DiagnosisDateTime >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(x => x.DiagnosisDateTime < endDate.Value.Date.AddDays(1));

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(
            CreatePatientDiagnosisRequest request)
        {
            var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.ConsultationId &&
                    x.EncounterId == request.EncounterId &&
                    !x.IsDelete);

            if (consultation == null)
                return (false, "Konsultasi dokter tidak ditemukan atau tidak sesuai encounter.");

            if (consultation.ConsultationStatus == DoctorConsultationStatus.Completed)
                return (false, "Konsultasi yang sudah completed tidak dapat ditambahkan diagnosis.");

            var snapshot = await BuildDiagnosisSnapshotAsync(
                request.DiagnosisId,
                request.DiagnosisCode,
                request.DiagnosisName,
                request.DiagnosisMasterType,
                request.IcdVersion,
                request.IsPrimary ? PatientDiagnosisType.Primary : request.DiagnosisType
            );

            if (!snapshot.IsValid)
                return (false, snapshot.ErrorMessage);

            var duplicateValidation = await ValidateDuplicateDiagnosisAsync(
                request.ConsultationId,
                snapshot.DiagnosisCode,
                snapshot.DiagnosisId,
                excludeId: null
            );

            if (!duplicateValidation.IsValid)
                return duplicateValidation;

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateDuplicateDiagnosisAsync(
            Guid consultationId,
            string diagnosisCode,
            Guid? diagnosisId,
            Guid? excludeId)
        {
            var normalizedCode = diagnosisCode.Trim().ToUpperInvariant();

            var duplicateByCode = await _dbContext.Set<TrxPatientDiagnosis>()
                .AnyAsync(x =>
                    x.ConsultationId == consultationId &&
                    x.DiagnosisCode.ToUpper() == normalizedCode &&
                    !x.IsDelete &&
                    x.DiagnosisStatus != PatientDiagnosisStatus.Cancelled &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateByCode)
                return (false, "Diagnosis dengan kode yang sama sudah ada pada konsultasi ini.");

            if (diagnosisId.HasValue)
            {
                var duplicateByMaster = await _dbContext.Set<TrxPatientDiagnosis>()
                    .AnyAsync(x =>
                        x.ConsultationId == consultationId &&
                        x.DiagnosisId == diagnosisId.Value &&
                        !x.IsDelete &&
                        x.DiagnosisStatus != PatientDiagnosisStatus.Cancelled &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateByMaster)
                    return (false, "Diagnosis master yang sama sudah ada pada konsultasi ini.");
            }

            return (true, null);
        }

        private async Task<DiagnosisSnapshotResult> BuildDiagnosisSnapshotAsync(
            Guid? diagnosisId,
            string? diagnosisCode,
            string? diagnosisName,
            string? diagnosisMasterType,
            string? icdVersion,
            PatientDiagnosisType requestedDiagnosisType)
        {
            var normalizedDiagnosisId = NormalizeNullableGuid(diagnosisId);

            if (normalizedDiagnosisId.HasValue)
            {
                var master = await _dbContext.Set<MstDiagnosis>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == normalizedDiagnosisId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (master == null)
                {
                    return DiagnosisSnapshotResult.Fail("Master diagnosis tidak ditemukan atau tidak aktif.");
                }

                if (!master.IsSelectableForClinicalUse)
                {
                    return DiagnosisSnapshotResult.Fail("Master diagnosis ini tidak dapat dipilih untuk penggunaan klinis.");
                }

                if (requestedDiagnosisType == PatientDiagnosisType.Primary && !master.IsPrimaryDiagnosisAllowed)
                {
                    return DiagnosisSnapshotResult.Fail("Diagnosis ini tidak diizinkan sebagai diagnosis utama.");
                }

                if (requestedDiagnosisType != PatientDiagnosisType.Primary && !master.IsSecondaryDiagnosisAllowed)
                {
                    return DiagnosisSnapshotResult.Fail("Diagnosis ini tidak diizinkan sebagai diagnosis sekunder.");
                }

                return DiagnosisSnapshotResult.Ok(
                    diagnosisId: master.Id,
                    diagnosisCode: master.DiagnosisCode.Trim().ToUpperInvariant(),
                    diagnosisName: master.DiagnosisName.Trim(),
                    diagnosisMasterType: master.DiagnosisType,
                    icdVersion: master.IcdVersion,
                    isFromMasterDiagnosis: true
                );
            }

            if (string.IsNullOrWhiteSpace(diagnosisCode))
                return DiagnosisSnapshotResult.Fail("Kode diagnosis wajib diisi jika tidak memilih master diagnosis.");

            if (string.IsNullOrWhiteSpace(diagnosisName))
                return DiagnosisSnapshotResult.Fail("Nama diagnosis wajib diisi jika tidak memilih master diagnosis.");

            return DiagnosisSnapshotResult.Ok(
                diagnosisId: null,
                diagnosisCode: diagnosisCode.Trim().ToUpperInvariant(),
                diagnosisName: diagnosisName.Trim(),
                diagnosisMasterType: string.IsNullOrWhiteSpace(diagnosisMasterType)
                    ? "Manual"
                    : diagnosisMasterType.Trim(),
                icdVersion: NormalizeNullableText(icdVersion),
                isFromMasterDiagnosis: false
            );
        }

        private async Task ClearPrimaryDiagnosisAsync(
            Guid consultationId,
            Guid actorUserId,
            DateTime now,
            Guid? exceptDiagnosisId = null)
        {
            var query = _dbContext.Set<TrxPatientDiagnosis>()
                .Where(x =>
                    x.ConsultationId == consultationId &&
                    x.IsPrimary &&
                    !x.IsDelete &&
                    x.DiagnosisStatus != PatientDiagnosisStatus.Cancelled);

            if (exceptDiagnosisId.HasValue)
                query = query.Where(x => x.Id != exceptDiagnosisId.Value);

            var existingPrimaryDiagnoses = await query.ToListAsync();

            foreach (var diagnosis in existingPrimaryDiagnoses)
            {
                diagnosis.IsPrimary = false;

                if (diagnosis.DiagnosisType == PatientDiagnosisType.Primary)
                    diagnosis.DiagnosisType = PatientDiagnosisType.Secondary;

                diagnosis.UpdateDateTime = now;
                diagnosis.UpdateBy = actorUserId;
            }
        }

        private async Task<DiagnosisSummaryResult> UpdateConsultationDiagnosisSummaryAsync(
            Guid consultationId,
            Guid actorUserId,
            DateTime now)
        {
            var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                .FirstAsync(x => x.Id == consultationId && !x.IsDelete);

            var diagnoses = await _dbContext.Set<TrxPatientDiagnosis>()
                .AsNoTracking()
                .Where(x =>
                    x.ConsultationId == consultationId &&
                    !x.IsDelete &&
                    x.DiagnosisStatus != PatientDiagnosisStatus.Cancelled)
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.DiagnosisDateTime)
                .ToListAsync();

            var primaryDiagnosis = diagnoses.FirstOrDefault(x => x.IsPrimary);
            var secondaryDiagnoses = diagnoses.Where(x => !x.IsPrimary).ToList();

            var diagnosisText = diagnoses.Count == 0
                ? null
                : string.Join("; ", diagnoses.Select(x => $"{x.DiagnosisCode} - {x.DiagnosisName}"));

            var primaryDiagnosisText = primaryDiagnosis == null
                ? null
                : $"{primaryDiagnosis.DiagnosisCode} - {primaryDiagnosis.DiagnosisName}";

            var secondaryDiagnosisText = secondaryDiagnoses.Count == 0
                ? null
                : string.Join("; ", secondaryDiagnoses.Select(x => $"{x.DiagnosisCode} - {x.DiagnosisName}"));

            consultation.DiagnosisText = diagnosisText;
            consultation.PrimaryDiagnosisText = primaryDiagnosisText;
            consultation.SecondaryDiagnosisText = secondaryDiagnosisText;
            consultation.DiagnosisCount = diagnoses.Count;
            consultation.HasPrimaryDiagnosis = primaryDiagnosis != null;
            consultation.UpdateDateTime = now;
            consultation.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return new DiagnosisSummaryResult
            {
                DiagnosisText = diagnosisText,
                PrimaryDiagnosisText = primaryDiagnosisText,
                SecondaryDiagnosisText = secondaryDiagnosisText,
                DiagnosisCount = diagnoses.Count,
                HasPrimaryDiagnosis = primaryDiagnosis != null
            };
        }

        private static IQueryable<TrxPatientDiagnosis> ApplySorting(
            IQueryable<TrxPatientDiagnosis> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "diagnosiscode" => isDesc ? query.OrderByDescending(x => x.DiagnosisCode) : query.OrderBy(x => x.DiagnosisCode),
                "diagnosisname" => isDesc ? query.OrderByDescending(x => x.DiagnosisName) : query.OrderBy(x => x.DiagnosisName),
                "diagnosisdatetime" => isDesc ? query.OrderByDescending(x => x.DiagnosisDateTime) : query.OrderBy(x => x.DiagnosisDateTime),
                "diagnosistype" => isDesc ? query.OrderByDescending(x => x.DiagnosisType) : query.OrderBy(x => x.DiagnosisType),
                "diagnosisstatus" => isDesc ? query.OrderByDescending(x => x.DiagnosisStatus) : query.OrderBy(x => x.DiagnosisStatus),
                "isprimary" => isDesc ? query.OrderByDescending(x => x.IsPrimary) : query.OrderBy(x => x.IsPrimary),
                "isconfirmed" => isDesc ? query.OrderByDescending(x => x.IsConfirmed) : query.OrderBy(x => x.IsConfirmed),
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.DiagnosisDateTime)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.DiagnosisDateTime)
            };
        }

        private static PatientDiagnosisResponse ToResponse(TrxPatientDiagnosis x)
        {
            return new PatientDiagnosisResponse
            {
                Id = x.Id,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : string.Empty,
                ConsultationId = x.ConsultationId,
                ConsultationNumber = x.Consultation != null ? x.Consultation.ConsultationNumber : string.Empty,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : string.Empty,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                DiagnosisId = x.DiagnosisId,
                DiagnosisCode = x.DiagnosisCode,
                DiagnosisName = x.DiagnosisName,
                DiagnosisMasterType = x.DiagnosisMasterType,
                IcdVersion = x.IcdVersion,
                DiagnosisType = x.DiagnosisType,
                DiagnosisStatus = x.DiagnosisStatus,
                IsPrimary = x.IsPrimary,
                IsChronic = x.IsChronic,
                IsNewCase = x.IsNewCase,
                IsConfirmed = x.IsConfirmed,
                IsFromMasterDiagnosis = x.IsFromMasterDiagnosis,
                DiagnosisDateTime = x.DiagnosisDateTime,
                OnsetDate = x.OnsetDate,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static PatientDiagnosisDetailResponse ToDetailResponse(TrxPatientDiagnosis x)
        {
            return new PatientDiagnosisDetailResponse
            {
                Id = x.Id,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : string.Empty,
                ConsultationId = x.ConsultationId,
                ConsultationNumber = x.Consultation != null ? x.Consultation.ConsultationNumber : string.Empty,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : string.Empty,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                DiagnosisId = x.DiagnosisId,
                DiagnosisCode = x.DiagnosisCode,
                DiagnosisName = x.DiagnosisName,
                DiagnosisMasterType = x.DiagnosisMasterType,
                IcdVersion = x.IcdVersion,
                DiagnosisType = x.DiagnosisType,
                DiagnosisStatus = x.DiagnosisStatus,
                IsPrimary = x.IsPrimary,
                IsChronic = x.IsChronic,
                IsNewCase = x.IsNewCase,
                IsConfirmed = x.IsConfirmed,
                IsFromMasterDiagnosis = x.IsFromMasterDiagnosis,
                DiagnosisDateTime = x.DiagnosisDateTime,
                OnsetDate = x.OnsetDate,
                SortOrder = x.SortOrder,
                ClinicalNote = x.ClinicalNote,
                AssessmentNote = x.AssessmentNote,
                PlanNote = x.PlanNote,
                DifferentialDiagnosisNote = x.DifferentialDiagnosisNote,
                SupportingFindingNote = x.SupportingFindingNote,
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

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<PatientDiagnosisEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new PatientDiagnosisEnumOptionResponse
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

        private class DiagnosisSummaryResult
        {
            public string? DiagnosisText { get; set; }
            public string? PrimaryDiagnosisText { get; set; }
            public string? SecondaryDiagnosisText { get; set; }
            public int DiagnosisCount { get; set; }
            public bool HasPrimaryDiagnosis { get; set; }
        }

        private class DiagnosisSnapshotResult
        {
            public bool IsValid { get; set; }
            public string? ErrorMessage { get; set; }
            public Guid? DiagnosisId { get; set; }
            public string DiagnosisCode { get; set; } = string.Empty;
            public string DiagnosisName { get; set; } = string.Empty;
            public string DiagnosisMasterType { get; set; } = string.Empty;
            public string? IcdVersion { get; set; }
            public bool IsFromMasterDiagnosis { get; set; }
            public static DiagnosisSnapshotResult Ok(
                Guid? diagnosisId,
                string diagnosisCode,
                string diagnosisName,
                string diagnosisMasterType,
                string? icdVersion,
                bool isFromMasterDiagnosis)
            {
                return new DiagnosisSnapshotResult
                {
                    IsValid = true,
                    DiagnosisId = diagnosisId,
                    DiagnosisCode = diagnosisCode,
                    DiagnosisName = diagnosisName,
                    DiagnosisMasterType = diagnosisMasterType,
                    IcdVersion = icdVersion,
                    IsFromMasterDiagnosis = isFromMasterDiagnosis
                };
            }

            public static DiagnosisSnapshotResult Fail(string errorMessage)
            {
                return new DiagnosisSnapshotResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}