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

using ResponsePatientAllergyPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs.PatientAllergyResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical/patient-allergies")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Patient Allergy",
        AreaName = "HealthServices",
        ControllerName = "PatientAllergy",
        Description = "Riwayat alergi pasien dan patient safety alert",
        SortOrder = 5
    )]
    [Tags("Health Services / Clinical / Patient Allergy")]
    public class PatientAllergyController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Clinical";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PatientAllergyController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientAllergyFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Allergy", Description = "Melihat metadata filter alergi pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientAllergy", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientAllergyFilterMetadataResponse
            {
                DefaultFilter = new PatientAllergyDefaultFilterResponse(),
                SortOptions = new List<PatientAllergySortOptionResponse>
                {
                    new() { Value = "reportedDateTime", Label = "Tanggal laporan" },
                    new() { Value = "allergenName", Label = "Nama alergen" },
                    new() { Value = "allergyCategory", Label = "Kategori alergi" },
                    new() { Value = "severity", Label = "Tingkat keparahan" },
                    new() { Value = "certainty", Label = "Kepastian" },
                    new() { Value = "allergyStatus", Label = "Status alergi" },
                    new() { Value = "isHighRisk", Label = "Risiko tinggi" },
                    new() { Value = "isLifeThreatening", Label = "Mengancam jiwa" },
                    new() { Value = "isVerified", Label = "Terverifikasi" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                AllergyCategoryOptions = BuildEnumOptions<PatientAllergyCategory>(),
                SeverityOptions = BuildEnumOptions<PatientAllergySeverity>(),
                CertaintyOptions = BuildEnumOptions<PatientAllergyCertainty>(),
                AllergyStatusOptions = BuildEnumOptions<PatientAllergyStatus>()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientAllergy.GetFilterMetadata",
                "Mengambil metadata filter alergi pasien.",
                result
            );

            return Ok(ApiResponse<PatientAllergyFilterMetadataResponse>.Ok(
                result,
                "Metadata filter alergi pasien berhasil diambil."
            ));
        }

        [HttpGet("active-alerts")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientAllergyAlertResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Allergy", Description = "Melihat alert alergi aktif pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientAllergy", "Read")]
        public async Task<IActionResult> GetActiveAlerts([FromQuery] Guid patientId)
        {
            if (patientId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "PatientId wajib diisi."
                ));
            }

            var data = await _dbContext.Set<TrxPatientAllergy>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.PatientId == patientId &&
                    x.IsActive &&
                    x.IsAlertEnabled &&
                    x.AllergyStatus == PatientAllergyStatus.Active)
                .OrderByDescending(x => x.IsLifeThreatening)
                .ThenByDescending(x => x.IsHighRisk)
                .ThenByDescending(x => x.Severity)
                .ThenBy(x => x.AllergenName)
                .Select(x => new PatientAllergyAlertResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    AllergyRecordNumber = x.AllergyRecordNumber,
                    AllergyCategory = x.AllergyCategory,
                    DrugId = x.DrugId,
                    AllergenCode = x.AllergenCode,
                    AllergenName = x.AllergenName,
                    AllergenGroupName = x.AllergenGroupName,
                    ReactionType = x.ReactionType,
                    ReactionDescription = x.ReactionDescription,
                    Severity = x.Severity,
                    Certainty = x.Certainty,
                    IsHighRisk = x.IsHighRisk,
                    IsLifeThreatening = x.IsLifeThreatening,
                    PatientSafetyNote = x.PatientSafetyNote
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientAllergyAlertResponse>>.Ok(
                data,
                "Alert alergi aktif pasien berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientAllergyPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Allergy", Description = "Melihat data alergi pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientAllergy", "Read")]
        public async Task<IActionResult> GetAllergies(
            [FromQuery] string? search,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? consultationId,
            [FromQuery] Guid? assessmentId,
            [FromQuery] Guid? doctorId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? drugId,
            [FromQuery] PatientAllergyCategory? allergyCategory,
            [FromQuery] PatientAllergySeverity? severity,
            [FromQuery] PatientAllergyCertainty? certainty,
            [FromQuery] PatientAllergyStatus? allergyStatus,
            [FromQuery] bool? isHighRisk,
            [FromQuery] bool? isLifeThreatening,
            [FromQuery] bool? isAlertEnabled,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isActive,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy = "reportedDateTime",
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
                drugId,
                allergyCategory,
                severity,
                certainty,
                allergyStatus,
                isHighRisk,
                isLifeThreatening,
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

            var result = new ResponsePatientAllergyPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePatientAllergyPagedResult>.Ok(
                result,
                "Data alergi pasien berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientAllergyOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Allergy", Description = "Melihat pilihan alergi pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientAllergy", "Read")]
        public async Task<IActionResult> GetAllergyOptions(
            [FromQuery] Guid? patientId,
            [FromQuery] PatientAllergyCategory? allergyCategory,
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool onlyAlertEnabled = false,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<TrxPatientAllergy>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive && x.AllergyStatus == PatientAllergyStatus.Active);

            if (onlyAlertEnabled)
                query = query.Where(x => x.IsAlertEnabled);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (allergyCategory.HasValue)
                query = query.Where(x => x.AllergyCategory == allergyCategory.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.AllergenName.ToLower().Contains(keyword) ||
                    (x.AllergenCode != null && x.AllergenCode.ToLower().Contains(keyword)) ||
                    (x.AllergenGroupName != null && x.AllergenGroupName.ToLower().Contains(keyword)) ||
                    (x.ReactionType != null && x.ReactionType.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderByDescending(x => x.IsLifeThreatening)
                .ThenByDescending(x => x.IsHighRisk)
                .ThenBy(x => x.AllergenName)
                .Take(100)
                .Select(x => new PatientAllergyOptionResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    AllergyRecordNumber = x.AllergyRecordNumber,
                    AllergyCategory = x.AllergyCategory,
                    AllergenName = x.AllergenName,
                    AllergenGroupName = x.AllergenGroupName,
                    ReactionType = x.ReactionType,
                    Severity = x.Severity,
                    Certainty = x.Certainty,
                    AllergyStatus = x.AllergyStatus,
                    IsHighRisk = x.IsHighRisk,
                    IsLifeThreatening = x.IsLifeThreatening,
                    IsAlertEnabled = x.IsAlertEnabled,
                    IsVerified = x.IsVerified
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientAllergyOptionResponse>>.Ok(
                data,
                "Data pilihan alergi pasien berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientAllergyDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Allergy", Description = "Melihat detail alergi pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientAllergy", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Alergi pasien tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientAllergyDetailResponse>.Ok(
                ToDetailResponse(entity),
                "Detail alergi pasien berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientAllergyCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Patient Allergy", Description = "Membuat data alergi pasien", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PatientAllergy", "Create")]
        public async Task<IActionResult> CreateAllergy([FromBody] CreatePatientAllergyRequest request)
        {
            var validation = await ValidateCreateRequestAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data alergi pasien tidak valid."
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

            var duplicateValidation = await ValidateDuplicateAllergyAsync(
                patientId: request.PatientId,
                drugId: NormalizeNullableGuid(request.DrugId),
                allergyCategory: request.AllergyCategory,
                allergenName: request.AllergenName,
                excludeId: null
            );

            if (!duplicateValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    duplicateValidation.ErrorMessage ?? "Data alergi pasien duplikat."
                ));
            }

            var entity = new TrxPatientAllergy
            {
                Id = Guid.NewGuid(),
                AllergyRecordNumber = await GenerateAllergyRecordNumberAsync(now),
                PatientId = request.PatientId,
                EncounterId = context.EncounterId,
                ConsultationId = context.ConsultationId,
                AssessmentId = context.AssessmentId,
                DoctorId = context.DoctorId,
                ServiceUnitId = context.ServiceUnitId,
                ClinicId = context.ClinicId,
                DrugId = NormalizeNullableGuid(request.DrugId),

                AllergyCategory = request.AllergyCategory,
                AllergenCode = NormalizeNullableText(request.AllergenCode),
                AllergenName = request.AllergenName.Trim(),
                AllergenGroupName = NormalizeNullableText(request.AllergenGroupName),
                ReactionType = NormalizeNullableText(request.ReactionType),
                ReactionDescription = NormalizeNullableText(request.ReactionDescription),
                Severity = request.Severity,
                Certainty = request.Certainty,
                AllergyStatus = PatientAllergyStatus.Active,
                FirstReactionDate = request.FirstReactionDate,
                LastReactionDate = request.LastReactionDate,
                ReportedDateTime = request.ReportedDateTime ?? now,
                SourceOfInformation = NormalizeNullableText(request.SourceOfInformation),

                IsHighRisk = request.IsHighRisk,
                IsLifeThreatening = request.IsLifeThreatening,
                IsAlertEnabled = request.IsAlertEnabled,
                PatientSafetyNote = NormalizeNullableText(request.PatientSafetyNote),

                IsVerified = request.IsVerified,
                VerifiedAt = request.IsVerified ? now : null,
                VerifiedByUserId = request.IsVerified ? actorUserId : null,

                ClinicalNote = NormalizeNullableText(request.ClinicalNote),
                Notes = NormalizeNullableText(request.Notes),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            NormalizeAllergyData(entity);

            _dbContext.Set<TrxPatientAllergy>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = ToCreateUpdateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientAllergy.CreateAllergy",
                "Membuat data alergi pasien.",
                response
            );

            return Ok(ApiResponse<PatientAllergyCreateResponse>.Ok(
                response,
                "Alergi pasien berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientAllergyUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Allergy", Description = "Mengubah data alergi pasien", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientAllergy", "Update")]
        public async Task<IActionResult> UpdateAllergy(Guid id, [FromBody] UpdatePatientAllergyRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientAllergy>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Alergi pasien tidak ditemukan."
                ));
            }

            if (entity.AllergyStatus == PatientAllergyStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alergi yang sudah cancelled tidak dapat diubah."
                ));
            }

            var validation = await ValidateUpdateRequestAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data alergi pasien tidak valid."
                ));
            }

            var duplicateValidation = await ValidateDuplicateAllergyAsync(
                patientId: entity.PatientId,
                drugId: NormalizeNullableGuid(request.DrugId),
                allergyCategory: request.AllergyCategory,
                allergenName: request.AllergenName,
                excludeId: id
            );

            if (!duplicateValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    duplicateValidation.ErrorMessage ?? "Data alergi pasien duplikat."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.DrugId = NormalizeNullableGuid(request.DrugId);
            entity.AllergyCategory = request.AllergyCategory;
            entity.AllergenCode = NormalizeNullableText(request.AllergenCode);
            entity.AllergenName = request.AllergenName.Trim();
            entity.AllergenGroupName = NormalizeNullableText(request.AllergenGroupName);
            entity.ReactionType = NormalizeNullableText(request.ReactionType);
            entity.ReactionDescription = NormalizeNullableText(request.ReactionDescription);
            entity.Severity = request.Severity;
            entity.Certainty = request.Certainty;
            entity.AllergyStatus = request.AllergyStatus;
            entity.FirstReactionDate = request.FirstReactionDate;
            entity.LastReactionDate = request.LastReactionDate;
            entity.SourceOfInformation = NormalizeNullableText(request.SourceOfInformation);
            entity.IsHighRisk = request.IsHighRisk;
            entity.IsLifeThreatening = request.IsLifeThreatening;
            entity.IsAlertEnabled = request.IsAlertEnabled;
            entity.PatientSafetyNote = NormalizeNullableText(request.PatientSafetyNote);
            entity.ClinicalNote = NormalizeNullableText(request.ClinicalNote);
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            NormalizeAllergyData(entity);

            await _dbContext.SaveChangesAsync();

            var response = ToUpdateResponse(entity);

            return Ok(ApiResponse<PatientAllergyUpdateResponse>.Ok(
                response,
                "Alergi pasien berhasil diubah."
            ));
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Verify Patient Allergy", Description = "Verifikasi alergi pasien", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PatientAllergy", "Update")]
        public async Task<IActionResult> VerifyAllergy(Guid id, [FromBody] VerifyPatientAllergyRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientAllergy>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Alergi pasien tidak ditemukan."
                ));
            }

            if (entity.AllergyStatus == PatientAllergyStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alergi yang sudah cancelled tidak dapat diverifikasi."
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
                "Alergi pasien berhasil diverifikasi."
            ));
        }

        [HttpPatch("{id:guid}/resolve")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Resolve Patient Allergy", Description = "Menyelesaikan status alergi pasien", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("PatientAllergy", "Update")]
        public async Task<IActionResult> ResolveAllergy(Guid id, [FromBody] ResolvePatientAllergyRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientAllergy>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Alergi pasien tidak ditemukan."
                ));
            }

            if (entity.AllergyStatus == PatientAllergyStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alergi yang sudah cancelled tidak dapat diselesaikan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.AllergyStatus = PatientAllergyStatus.Resolved;
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
                "Alergi pasien berhasil diselesaikan."
            ));
        }

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Cancel Patient Allergy", Description = "Membatalkan alergi pasien", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("PatientAllergy", "Update")]
        public async Task<IActionResult> CancelAllergy(Guid id, [FromBody] CancelPatientAllergyRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientAllergy>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Alergi pasien tidak ditemukan."
                ));
            }

            if (entity.AllergyStatus == PatientAllergyStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alergi pasien sudah cancelled."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.AllergyStatus = PatientAllergyStatus.Cancelled;
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
                "Alergi pasien berhasil dibatalkan."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Patient Allergy", Description = "Menghapus data alergi pasien", AccessType = AccessTypes.Delete, SortOrder = 7)]
        [AccessPermission("PatientAllergy", "Delete")]
        public async Task<IActionResult> DeleteAllergy(Guid id)
        {
            var entity = await _dbContext.Set<TrxPatientAllergy>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Alergi pasien tidak ditemukan."
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
                "Alergi pasien berhasil dihapus."
            ));
        }

        private IQueryable<TrxPatientAllergy> BuildBaseQuery()
        {
            return _dbContext.Set<TrxPatientAllergy>()
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

        private static IQueryable<TrxPatientAllergy> ApplyFilters(
            IQueryable<TrxPatientAllergy> query,
            string? search,
            Guid? patientId,
            Guid? encounterId,
            Guid? consultationId,
            Guid? assessmentId,
            Guid? doctorId,
            Guid? serviceUnitId,
            Guid? clinicId,
            Guid? drugId,
            PatientAllergyCategory? allergyCategory,
            PatientAllergySeverity? severity,
            PatientAllergyCertainty? certainty,
            PatientAllergyStatus? allergyStatus,
            bool? isHighRisk,
            bool? isLifeThreatening,
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
                    x.AllergyRecordNumber.ToLower().Contains(keyword) ||
                    x.AllergenName.ToLower().Contains(keyword) ||
                    (x.AllergenCode != null && x.AllergenCode.ToLower().Contains(keyword)) ||
                    (x.AllergenGroupName != null && x.AllergenGroupName.ToLower().Contains(keyword)) ||
                    (x.ReactionType != null && x.ReactionType.ToLower().Contains(keyword)) ||
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

            if (drugId.HasValue && drugId.Value != Guid.Empty)
                query = query.Where(x => x.DrugId == drugId.Value);

            if (allergyCategory.HasValue)
                query = query.Where(x => x.AllergyCategory == allergyCategory.Value);

            if (severity.HasValue)
                query = query.Where(x => x.Severity == severity.Value);

            if (certainty.HasValue)
                query = query.Where(x => x.Certainty == certainty.Value);

            if (allergyStatus.HasValue)
                query = query.Where(x => x.AllergyStatus == allergyStatus.Value);

            if (isHighRisk.HasValue)
                query = query.Where(x => x.IsHighRisk == isHighRisk.Value);

            if (isLifeThreatening.HasValue)
                query = query.Where(x => x.IsLifeThreatening == isLifeThreatening.Value);

            if (isAlertEnabled.HasValue)
                query = query.Where(x => x.IsAlertEnabled == isAlertEnabled.Value);

            if (isVerified.HasValue)
                query = query.Where(x => x.IsVerified == isVerified.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (startDate.HasValue)
                query = query.Where(x => x.ReportedDateTime >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(x => x.ReportedDateTime < endDate.Value.Date.AddDays(1));

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(CreatePatientAllergyRequest request)
        {
            if (request.PatientId == Guid.Empty)
                return (false, "PatientId wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.AllergenName))
                return (false, "Nama alergen wajib diisi.");

            var patientExists = await _dbContext.Set<MstPatient>()
                .AnyAsync(x => x.Id == request.PatientId && !x.IsDelete);

            if (!patientExists)
                return (false, "Pasien tidak ditemukan.");

            var normalizedDrugId = NormalizeNullableGuid(request.DrugId);

            if (normalizedDrugId.HasValue)
            {
                var drugExists = await _dbContext.Set<MstDrug>()
                    .AnyAsync(x => x.Id == normalizedDrugId.Value && !x.IsDelete);

                if (!drugExists)
                    return (false, "Master obat tidak ditemukan.");
            }

            if (request.FirstReactionDate.HasValue &&
                request.LastReactionDate.HasValue &&
                request.FirstReactionDate.Value.Date > request.LastReactionDate.Value.Date)
            {
                return (false, "Tanggal reaksi pertama tidak boleh lebih besar dari tanggal reaksi terakhir.");
            }

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateUpdateRequestAsync(UpdatePatientAllergyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AllergenName))
                return (false, "Nama alergen wajib diisi.");

            var normalizedDrugId = NormalizeNullableGuid(request.DrugId);

            if (normalizedDrugId.HasValue)
            {
                var drugExists = await _dbContext.Set<MstDrug>()
                    .AnyAsync(x => x.Id == normalizedDrugId.Value && !x.IsDelete);

                if (!drugExists)
                    return (false, "Master obat tidak ditemukan.");
            }

            if (request.FirstReactionDate.HasValue &&
                request.LastReactionDate.HasValue &&
                request.FirstReactionDate.Value.Date > request.LastReactionDate.Value.Date)
            {
                return (false, "Tanggal reaksi pertama tidak boleh lebih besar dari tanggal reaksi terakhir.");
            }

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateDuplicateAllergyAsync(
            Guid patientId,
            Guid? drugId,
            PatientAllergyCategory allergyCategory,
            string allergenName,
            Guid? excludeId)
        {
            var normalizedName = allergenName.Trim().ToLower();

            var query = _dbContext.Set<TrxPatientAllergy>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.PatientId == patientId &&
                    x.AllergyStatus != PatientAllergyStatus.Cancelled);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            if (drugId.HasValue)
            {
                var duplicateByDrug = await query.AnyAsync(x => x.DrugId == drugId.Value);

                if (duplicateByDrug)
                    return (false, "Alergi obat yang sama sudah tercatat untuk pasien ini.");
            }

            var duplicateByName = await query.AnyAsync(x =>
                x.AllergyCategory == allergyCategory &&
                x.AllergenName.ToLower() == normalizedName);

            if (duplicateByName)
                return (false, "Alergi dengan kategori dan nama alergen yang sama sudah tercatat untuk pasien ini.");

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

        private async Task<string> GenerateAllergyRecordNumberAsync(DateTime now)
        {
            var prefix = $"ALG-{now:yyyyMMdd}";

            var countToday = await _dbContext.Set<TrxPatientAllergy>()
                .CountAsync(x => x.AllergyRecordNumber.StartsWith(prefix));

            return $"{prefix}-{countToday + 1:0000}";
        }

        private static IQueryable<TrxPatientAllergy> ApplySorting(
            IQueryable<TrxPatientAllergy> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "reportedDateTime").ToLowerInvariant() switch
            {
                "allergenname" => isDesc ? query.OrderByDescending(x => x.AllergenName) : query.OrderBy(x => x.AllergenName),
                "allergycategory" => isDesc ? query.OrderByDescending(x => x.AllergyCategory) : query.OrderBy(x => x.AllergyCategory),
                "severity" => isDesc ? query.OrderByDescending(x => x.Severity) : query.OrderBy(x => x.Severity),
                "certainty" => isDesc ? query.OrderByDescending(x => x.Certainty) : query.OrderBy(x => x.Certainty),
                "allergystatus" => isDesc ? query.OrderByDescending(x => x.AllergyStatus) : query.OrderBy(x => x.AllergyStatus),
                "ishighrisk" => isDesc ? query.OrderByDescending(x => x.IsHighRisk) : query.OrderBy(x => x.IsHighRisk),
                "islifethreatening" => isDesc ? query.OrderByDescending(x => x.IsLifeThreatening) : query.OrderBy(x => x.IsLifeThreatening),
                "isverified" => isDesc ? query.OrderByDescending(x => x.IsVerified) : query.OrderBy(x => x.IsVerified),
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDesc
                    ? query.OrderByDescending(x => x.ReportedDateTime)
                        .ThenByDescending(x => x.IsLifeThreatening)
                        .ThenByDescending(x => x.IsHighRisk)
                    : query.OrderBy(x => x.ReportedDateTime)
                        .ThenByDescending(x => x.IsLifeThreatening)
                        .ThenByDescending(x => x.IsHighRisk)
            };
        }

        private static PatientAllergyResponse ToResponse(TrxPatientAllergy x)
        {
            return new PatientAllergyResponse
            {
                Id = x.Id,
                AllergyRecordNumber = x.AllergyRecordNumber,
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
                DrugId = x.DrugId,
                AllergyCategory = x.AllergyCategory,
                AllergenCode = x.AllergenCode,
                AllergenName = x.AllergenName,
                AllergenGroupName = x.AllergenGroupName,
                ReactionType = x.ReactionType,
                Severity = x.Severity,
                Certainty = x.Certainty,
                AllergyStatus = x.AllergyStatus,
                FirstReactionDate = x.FirstReactionDate,
                LastReactionDate = x.LastReactionDate,
                ReportedDateTime = x.ReportedDateTime,
                SourceOfInformation = x.SourceOfInformation,
                IsHighRisk = x.IsHighRisk,
                IsLifeThreatening = x.IsLifeThreatening,
                IsAlertEnabled = x.IsAlertEnabled,
                IsVerified = x.IsVerified,
                VerifiedAt = x.VerifiedAt,
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByUserName = x.VerifiedByUser != null ? x.VerifiedByUser.DisplayName : null,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static PatientAllergyDetailResponse ToDetailResponse(TrxPatientAllergy x)
        {
            return new PatientAllergyDetailResponse
            {
                Id = x.Id,
                AllergyRecordNumber = x.AllergyRecordNumber,
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
                DrugId = x.DrugId,
                AllergyCategory = x.AllergyCategory,
                AllergenCode = x.AllergenCode,
                AllergenName = x.AllergenName,
                AllergenGroupName = x.AllergenGroupName,
                ReactionType = x.ReactionType,
                ReactionDescription = x.ReactionDescription,
                Severity = x.Severity,
                Certainty = x.Certainty,
                AllergyStatus = x.AllergyStatus,
                FirstReactionDate = x.FirstReactionDate,
                LastReactionDate = x.LastReactionDate,
                ReportedDateTime = x.ReportedDateTime,
                SourceOfInformation = x.SourceOfInformation,
                IsHighRisk = x.IsHighRisk,
                IsLifeThreatening = x.IsLifeThreatening,
                IsAlertEnabled = x.IsAlertEnabled,
                PatientSafetyNote = x.PatientSafetyNote,
                IsVerified = x.IsVerified,
                VerifiedAt = x.VerifiedAt,
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByUserName = x.VerifiedByUser != null ? x.VerifiedByUser.DisplayName : null,
                ClinicalNote = x.ClinicalNote,
                Notes = x.Notes,
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

        private static PatientAllergyCreateResponse ToCreateUpdateResponse(TrxPatientAllergy x)
        {
            return new PatientAllergyCreateResponse
            {
                Id = x.Id,
                AllergyRecordNumber = x.AllergyRecordNumber,
                PatientId = x.PatientId,
                EncounterId = x.EncounterId,
                ConsultationId = x.ConsultationId,
                AssessmentId = x.AssessmentId,
                DrugId = x.DrugId,
                AllergyCategory = x.AllergyCategory,
                AllergenName = x.AllergenName,
                AllergenGroupName = x.AllergenGroupName,
                ReactionType = x.ReactionType,
                Severity = x.Severity,
                Certainty = x.Certainty,
                AllergyStatus = x.AllergyStatus,
                IsHighRisk = x.IsHighRisk,
                IsLifeThreatening = x.IsLifeThreatening,
                IsAlertEnabled = x.IsAlertEnabled,
                IsVerified = x.IsVerified
            };
        }

        private static PatientAllergyUpdateResponse ToUpdateResponse(TrxPatientAllergy x)
        {
            return new PatientAllergyUpdateResponse
            {
                Id = x.Id,
                AllergyRecordNumber = x.AllergyRecordNumber,
                PatientId = x.PatientId,
                EncounterId = x.EncounterId,
                ConsultationId = x.ConsultationId,
                AssessmentId = x.AssessmentId,
                DrugId = x.DrugId,
                AllergyCategory = x.AllergyCategory,
                AllergenName = x.AllergenName,
                AllergenGroupName = x.AllergenGroupName,
                ReactionType = x.ReactionType,
                Severity = x.Severity,
                Certainty = x.Certainty,
                AllergyStatus = x.AllergyStatus,
                IsHighRisk = x.IsHighRisk,
                IsLifeThreatening = x.IsLifeThreatening,
                IsAlertEnabled = x.IsAlertEnabled,
                IsVerified = x.IsVerified
            };
        }

        private static void NormalizeAllergyData(TrxPatientAllergy entity)
        {
            if (entity.AllergyStatus == PatientAllergyStatus.Resolved ||
                entity.AllergyStatus == PatientAllergyStatus.Inactive ||
                entity.AllergyStatus == PatientAllergyStatus.Cancelled ||
                entity.AllergyStatus == PatientAllergyStatus.EnteredInError)
            {
                entity.IsAlertEnabled = false;
            }

            if (entity.IsLifeThreatening)
            {
                entity.IsHighRisk = true;
                entity.Severity = PatientAllergySeverity.LifeThreatening;
            }

            if (!entity.IsActive)
                entity.IsAlertEnabled = false;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<PatientAllergyEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new PatientAllergyEnumOptionResponse
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
    }
}