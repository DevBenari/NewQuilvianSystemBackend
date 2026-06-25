using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponsePatientRelationshipPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs.PatientRelationshipResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/patient-management/master-data/patient-relationships")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PATIENT_MANAGEMENT_MASTER_DATA",
        moduleName: "Health Service Patient Management Master Data",
        displayName: "Patient Relationship",
        AreaName = "HealthServices",
        ControllerName = "PatientRelationship",
        Description = "Health service patient management master data patient relationship",
        SortOrder = 19
    )]
    [Tags("Health Services / Patient Management / Master Data / Patient Relationship")]
    public class PatientRelationshipController : ControllerBase
    {
        private const string LogCategory = "HealthServices.PatientManagement.MasterData";
        private const string KioskReadPolicy = "KioskRead";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PatientRelationshipController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientRelationshipFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Relationship",
            Description = "Melihat metadata filter patient relationship",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("PatientRelationship", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientRelationshipFilterMetadataResponse
            {
                DefaultFilter = new PatientRelationshipDefaultFilterResponse(),
                CustomPeriods = new List<PatientRelationshipCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<PatientRelationshipSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "updateDateTime", Label = "Tanggal diperbarui" },
                    new() { Value = "patientName", Label = "Nama pasien" },
                    new() { Value = "medicalRecordNumber", Label = "Nomor rekam medis" },
                    new() { Value = "relatedPatientName", Label = "Nama pasien terkait" },
                    new() { Value = "relationshipType", Label = "Tipe relasi" },
                    new() { Value = "relatedPersonName", Label = "Nama keluarga atau relasi" },
                    new() { Value = "isPrimary", Label = "Relasi utama" },
                    new() { Value = "isEmergencyContact", Label = "Kontak darurat" },
                    new() { Value = "isResponsiblePerson", Label = "Penanggung jawab" },
                    new() { Value = "isLegalGuardian", Label = "Wali hukum" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                RelationFilters = new List<PatientRelationshipRelationFilterResponse>
                {
                    new()
                    {
                        Value = "patientId",
                        Label = "Patient",
                        Endpoint = "/api/v1/health-services/patient-management/master-data/patients/options"
                    },
                    new()
                    {
                        Value = "relatedPatientId",
                        Label = "Related Patient",
                        Endpoint = "/api/v1/health-services/patient-management/master-data/patients/options"
                    }
                },
                RelationshipTypeOptions = BuildEnumOptions<PatientRelationshipType>(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientRelationship.GetFilterMetadata",
                "Mengambil metadata filter patient relationship.",
                result
            );

            return Ok(ApiResponse<PatientRelationshipFilterMetadataResponse>.Ok(
                result,
                "Metadata filter patient relationship berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<PatientRelationshipSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Relationship",
            Description = "Melihat ringkasan patient relationship",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("PatientRelationship", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstPatientRelationship>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new PatientRelationshipSummaryResponse
            {
                TotalRelationship = await query.CountAsync(),
                ActiveRelationship = await query.CountAsync(x => x.IsActive),
                InactiveRelationship = await query.CountAsync(x => !x.IsActive),
                PrimaryRelationship = await query.CountAsync(x => x.IsPrimary),
                EmergencyContactRelationship = await query.CountAsync(x => x.IsEmergencyContact),
                ResponsiblePersonRelationship = await query.CountAsync(x => x.IsResponsiblePerson),
                LegalGuardianRelationship = await query.CountAsync(x => x.IsLegalGuardian)
            };

            return Ok(ApiResponse<PatientRelationshipSummaryResponse>.Ok(
                result,
                "Ringkasan patient relationship berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientRelationshipPagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Relationship",
            Description = "Melihat data patient relationship",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("PatientRelationship", "Read")]
        public async Task<IActionResult> GetPatientRelationships(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? relatedPatientId,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyRelationFilter(query, patientId, relatedPatientId);
            query = ApplyStandardFilter(query, isActive, search);

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities
                    .SelectMany(x => new[] { x.CreateBy, x.UpdateBy })
                    .Where(x => x != Guid.Empty)
            );

            var items = entities
                .Select(x => MapResponse(x, actorNames))
                .ToList();

            var result = new ResponsePatientRelationshipPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePatientRelationshipPagedResult>.Ok(
                result,
                "Data patient relationship berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<PatientRelationshipOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Relationship",
            Description = "Melihat data pilihan patient relationship",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("PatientRelationship", "Read")]
        public async Task<IActionResult> GetPatientRelationshipOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] Guid? patientId = null,
            [FromQuery] Guid? relatedPatientId = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyRelationFilter(query, patientId, relatedPatientId);
            query = ApplyStandardFilter(query, onlyActive ? true : null, search);

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.IsResponsiblePerson)
                .ThenByDescending(x => x.IsEmergencyContact)
                .ThenBy(x => x.RelationshipType)
                .ThenBy(x => x.RelatedPersonName ?? (x.RelatedPatient != null ? x.RelatedPatient.FullName : string.Empty))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapOptionResponse)
                .ToList();

            var result = new PatientRelationshipOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PatientRelationshipOptionPagedResponse>.Ok(
                result,
                "Data pilihan patient relationship berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientRelationshipDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Patient Relationship",
            Description = "Melihat detail patient relationship",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("PatientRelationship", "Read")]
        public async Task<IActionResult> GetPatientRelationshipById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient relationship tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, actorNames);

            return Ok(ApiResponse<PatientRelationshipDetailResponse>.Ok(
                data,
                "Detail patient relationship berhasil diambil."
            ));
        }

        [HttpPost]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<PatientRelationshipCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Create",
            "Create Patient Relationship",
            Description = "Membuat data patient relationship",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        public async Task<IActionResult> CreatePatientRelationship([FromBody] CreatePatientRelationshipRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                patientId: request.PatientId,
                relatedPatientId: request.RelatedPatientId,
                relationshipType: request.RelationshipType,
                relatedPersonName: request.RelatedPersonName,
                relatedPersonIdentityNumber: request.RelatedPersonIdentityNumber,
                relatedPersonPhoneNumber: request.RelatedPersonPhoneNumber,
                relatedPersonWhatsAppNumber: request.RelatedPersonWhatsAppNumber,
                relatedPersonEmail: request.RelatedPersonEmail,
                isPrimary: request.IsPrimary,
                isEmergencyContact: request.IsEmergencyContact,
                isResponsiblePerson: request.IsResponsiblePerson,
                isLegalGuardian: request.IsLegalGuardian
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data patient relationship tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var normalizedRelatedPatientId = NormalizeNullableGuid(request.RelatedPatientId);

            if (request.IsPrimary)
            {
                await ResetPrimaryAsync(request.PatientId, null, actorUserId, now);
            }

            var entity = new MstPatientRelationship
            {
                Id = Guid.NewGuid(),
                PatientId = request.PatientId,
                RelatedPatientId = normalizedRelatedPatientId,
                RelationshipType = request.RelationshipType,
                RelatedPersonName = NormalizeNullableString(request.RelatedPersonName),
                RelatedPersonIdentityType = NormalizeNullableString(request.RelatedPersonIdentityType),
                RelatedPersonIdentityNumber = NormalizeNullableString(request.RelatedPersonIdentityNumber),
                RelatedPersonPhoneNumber = NormalizeNullableString(request.RelatedPersonPhoneNumber),
                RelatedPersonWhatsAppNumber = NormalizeNullableString(request.RelatedPersonWhatsAppNumber),
                RelatedPersonEmail = NormalizeLowerNullableString(request.RelatedPersonEmail),
                RelatedPersonAddress = NormalizeNullableString(request.RelatedPersonAddress),
                IsPrimary = request.IsPrimary,
                IsEmergencyContact = request.IsEmergencyContact,
                IsResponsiblePerson = request.IsResponsiblePerson,
                IsLegalGuardian = request.IsLegalGuardian,
                Notes = NormalizeNullableString(request.Notes),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstPatientRelationship>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = new PatientRelationshipCreateResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                RelatedPatientId = entity.RelatedPatientId,
                RelationshipType = entity.RelationshipType,
                RelationshipTypeName = BuildEnumLabel(entity.RelationshipType),
                RelatedPersonName = entity.RelatedPersonName,
                IsPrimary = entity.IsPrimary,
                IsEmergencyContact = entity.IsEmergencyContact,
                IsResponsiblePerson = entity.IsResponsiblePerson,
                IsLegalGuardian = entity.IsLegalGuardian,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientRelationship.CreatePatientRelationship",
                "Membuat data patient relationship.",
                response
            );

            return Ok(ApiResponse<PatientRelationshipCreateResponse>.Ok(
                response,
                "Patient relationship berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Patient Relationship",
            Description = "Mengubah data patient relationship",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("PatientRelationship", "Update")]
        public async Task<IActionResult> UpdatePatientRelationship(
            Guid id,
            [FromBody] UpdatePatientRelationshipRequest request)
        {
            var entity = await _dbContext.Set<MstPatientRelationship>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient relationship tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                patientId: request.PatientId,
                relatedPatientId: request.RelatedPatientId,
                relationshipType: request.RelationshipType,
                relatedPersonName: request.RelatedPersonName,
                relatedPersonIdentityNumber: request.RelatedPersonIdentityNumber,
                relatedPersonPhoneNumber: request.RelatedPersonPhoneNumber,
                relatedPersonWhatsAppNumber: request.RelatedPersonWhatsAppNumber,
                relatedPersonEmail: request.RelatedPersonEmail,
                isPrimary: request.IsPrimary,
                isEmergencyContact: request.IsEmergencyContact,
                isResponsiblePerson: request.IsResponsiblePerson,
                isLegalGuardian: request.IsLegalGuardian
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data patient relationship tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (request.IsPrimary)
            {
                await ResetPrimaryAsync(request.PatientId, id, actorUserId, now);
            }

            entity.PatientId = request.PatientId;
            entity.RelatedPatientId = NormalizeNullableGuid(request.RelatedPatientId);
            entity.RelationshipType = request.RelationshipType;
            entity.RelatedPersonName = NormalizeNullableString(request.RelatedPersonName);
            entity.RelatedPersonIdentityType = NormalizeNullableString(request.RelatedPersonIdentityType);
            entity.RelatedPersonIdentityNumber = NormalizeNullableString(request.RelatedPersonIdentityNumber);
            entity.RelatedPersonPhoneNumber = NormalizeNullableString(request.RelatedPersonPhoneNumber);
            entity.RelatedPersonWhatsAppNumber = NormalizeNullableString(request.RelatedPersonWhatsAppNumber);
            entity.RelatedPersonEmail = NormalizeLowerNullableString(request.RelatedPersonEmail);
            entity.RelatedPersonAddress = NormalizeNullableString(request.RelatedPersonAddress);
            entity.IsPrimary = request.IsPrimary;
            entity.IsEmergencyContact = request.IsEmergencyContact;
            entity.IsResponsiblePerson = request.IsResponsiblePerson;
            entity.IsLegalGuardian = request.IsLegalGuardian;
            entity.Notes = NormalizeNullableString(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientRelationship.UpdatePatientRelationship",
                "Mengubah data patient relationship.",
                new
                {
                    entity.Id,
                    entity.PatientId,
                    entity.RelatedPatientId,
                    entity.RelationshipType,
                    entity.IsActive
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient relationship berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Patient Relationship Status",
            Description = "Mengubah status patient relationship",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("PatientRelationship", "Update")]
        public async Task<IActionResult> UpdatePatientRelationshipStatus(
            Guid id,
            [FromBody] UpdatePatientRelationshipStatusRequest request)
        {
            var entity = await _dbContext.Set<MstPatientRelationship>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient relationship tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status patient relationship berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Patient Relationship",
            Description = "Menghapus data patient relationship",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("PatientRelationship", "Delete")]
        public async Task<IActionResult> DeletePatientRelationship(
            Guid id,
            [FromBody] DeletePatientRelationshipRequest? request = null)
        {
            var entity = await _dbContext.Set<MstPatientRelationship>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient relationship tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsPrimary = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (!string.IsNullOrWhiteSpace(request?.DeleteReason))
            {
                entity.Notes = request.DeleteReason.Trim();
            }

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient relationship berhasil dihapus."
            ));
        }

        private IQueryable<MstPatientRelationship> BuildBaseQuery()
        {
            return _dbContext.Set<MstPatientRelationship>()
                .AsNoTracking()
                .Include(x => x.Patient)
                .Include(x => x.RelatedPatient)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstPatientRelationship> ApplyDateFilter(
            IQueryable<MstPatientRelationship> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue)
            {
                var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime < end);
            }

            if (!startDate.HasValue &&
                !endDate.HasValue &&
                !string.IsNullOrWhiteSpace(customPeriod))
            {
                var today = AppDateTimeHelper.OperationalDate();

                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        query = query.Where(x =>
                            x.CreateDateTime >= today &&
                            x.CreateDateTime < today.AddDays(1));
                        break;

                    case "last7days":
                        query = query.Where(x =>
                            x.CreateDateTime >= today.AddDays(-6) &&
                            x.CreateDateTime < today.AddDays(1));
                        break;

                    case "thismonth":
                        var thisMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        query = query.Where(x =>
                            x.CreateDateTime >= thisMonthStart &&
                            x.CreateDateTime < thisMonthStart.AddMonths(1));
                        break;

                    case "lastmonth":
                        var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        var lastMonthStart = currentMonthStart.AddMonths(-1);
                        query = query.Where(x =>
                            x.CreateDateTime >= lastMonthStart &&
                            x.CreateDateTime < currentMonthStart);
                        break;
                }
            }

            return query;
        }

        private static IQueryable<MstPatientRelationship> ApplyRelationFilter(
            IQueryable<MstPatientRelationship> query,
            Guid? patientId,
            Guid? relatedPatientId)
        {
            var normalizedPatientId = NormalizeNullableGuid(patientId);
            if (normalizedPatientId.HasValue)
            {
                query = query.Where(x => x.PatientId == normalizedPatientId.Value);
            }

            var normalizedRelatedPatientId = NormalizeNullableGuid(relatedPatientId);
            if (normalizedRelatedPatientId.HasValue)
            {
                query = query.Where(x => x.RelatedPatientId == normalizedRelatedPatientId.Value);
            }

            return query;
        }

        private static IQueryable<MstPatientRelationship> ApplyStandardFilter(
            IQueryable<MstPatientRelationship> query,
            bool? isActive,
            string? search)
        {
            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                var matchedRelationshipTypes = Enum.GetValues<PatientRelationshipType>()
                    .Where(x =>
                        x.ToString().ToLower().Contains(keyword) ||
                        BuildEnumLabel(x).ToLower().Contains(keyword))
                    .ToList();

                query = query.Where(x =>
                    (x.RelatedPersonName != null && x.RelatedPersonName.ToLower().Contains(keyword)) ||
                    (x.RelatedPersonIdentityType != null && x.RelatedPersonIdentityType.ToLower().Contains(keyword)) ||
                    (x.RelatedPersonIdentityNumber != null && x.RelatedPersonIdentityNumber.ToLower().Contains(keyword)) ||
                    (x.RelatedPersonPhoneNumber != null && x.RelatedPersonPhoneNumber.ToLower().Contains(keyword)) ||
                    (x.RelatedPersonWhatsAppNumber != null && x.RelatedPersonWhatsAppNumber.ToLower().Contains(keyword)) ||
                    (x.RelatedPersonEmail != null && x.RelatedPersonEmail.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.PatientCode.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.RelatedPatient != null && x.RelatedPatient.PatientCode.ToLower().Contains(keyword)) ||
                    (x.RelatedPatient != null && x.RelatedPatient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.RelatedPatient != null && x.RelatedPatient.FullName.ToLower().Contains(keyword)) ||
                    matchedRelationshipTypes.Contains(x.RelationshipType));
            }

            return query;
        }

        private static IOrderedQueryable<MstPatientRelationship> ApplySorting(
            IQueryable<MstPatientRelationship> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase
            );

            return (sortBy ?? "createDateTime").Trim().ToLowerInvariant() switch
            {
                "updatedatetime" => isDescending
                    ? query.OrderByDescending(x => x.UpdateDateTime).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.UpdateDateTime).ThenBy(x => x.CreateDateTime),

                "patientname" => isDescending
                    ? query.OrderByDescending(x => x.Patient != null ? x.Patient.FullName : string.Empty)
                    : query.OrderBy(x => x.Patient != null ? x.Patient.FullName : string.Empty),

                "medicalrecordnumber" => isDescending
                    ? query.OrderByDescending(x => x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty)
                    : query.OrderBy(x => x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty),

                "relatedpatientname" => isDescending
                    ? query.OrderByDescending(x => x.RelatedPatient != null ? x.RelatedPatient.FullName : string.Empty)
                    : query.OrderBy(x => x.RelatedPatient != null ? x.RelatedPatient.FullName : string.Empty),

                "relationshiptype" => isDescending
                    ? query.OrderByDescending(x => x.RelationshipType).ThenBy(x => x.RelatedPersonName)
                    : query.OrderBy(x => x.RelationshipType).ThenBy(x => x.RelatedPersonName),

                "relatedpersonname" => isDescending
                    ? query.OrderByDescending(x => x.RelatedPersonName)
                    : query.OrderBy(x => x.RelatedPersonName),

                "isprimary" => isDescending
                    ? query.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.RelatedPersonName)
                    : query.OrderBy(x => x.IsPrimary).ThenBy(x => x.RelatedPersonName),

                "isemergencycontact" => isDescending
                    ? query.OrderByDescending(x => x.IsEmergencyContact).ThenBy(x => x.RelatedPersonName)
                    : query.OrderBy(x => x.IsEmergencyContact).ThenBy(x => x.RelatedPersonName),

                "isresponsibleperson" => isDescending
                    ? query.OrderByDescending(x => x.IsResponsiblePerson).ThenBy(x => x.RelatedPersonName)
                    : query.OrderBy(x => x.IsResponsiblePerson).ThenBy(x => x.RelatedPersonName),

                "islegalguardian" => isDescending
                    ? query.OrderByDescending(x => x.IsLegalGuardian).ThenBy(x => x.RelatedPersonName)
                    : query.OrderBy(x => x.IsLegalGuardian).ThenBy(x => x.RelatedPersonName),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.RelatedPersonName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.RelatedPersonName),

                _ => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime).ThenByDescending(x => x.RelatedPersonName)
                    : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.RelatedPersonName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            Guid patientId,
            Guid? relatedPatientId,
            PatientRelationshipType relationshipType,
            string? relatedPersonName,
            string? relatedPersonIdentityNumber,
            string? relatedPersonPhoneNumber,
            string? relatedPersonWhatsAppNumber,
            string? relatedPersonEmail,
            bool isPrimary,
            bool isEmergencyContact,
            bool isResponsiblePerson,
            bool isLegalGuardian)
        {
            if (patientId == Guid.Empty)
            {
                return (false, "Pasien wajib dipilih.");
            }

            if (!Enum.IsDefined(typeof(PatientRelationshipType), relationshipType))
            {
                return (false, "Tipe relasi pasien tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (relationshipType == PatientRelationshipType.Unknown)
            {
                return (false, "Tipe relasi pasien wajib dipilih.");
            }

            var patientExists = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == patientId && x.IsActive && !x.IsDelete);

            if (!patientExists)
            {
                return (false, "Pasien tidak valid atau tidak aktif.");
            }

            var normalizedRelatedPatientId = NormalizeNullableGuid(relatedPatientId);

            if (normalizedRelatedPatientId.HasValue)
            {
                if (normalizedRelatedPatientId.Value == patientId)
                {
                    return (false, "Pasien terkait tidak boleh sama dengan pasien utama.");
                }

                var relatedPatientExists = await _dbContext.Set<MstPatient>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == normalizedRelatedPatientId.Value && x.IsActive && !x.IsDelete);

                if (!relatedPatientExists)
                {
                    return (false, "Pasien terkait tidak valid atau tidak aktif.");
                }
            }

            var hasFreeTextRelatedPerson =
                !string.IsNullOrWhiteSpace(relatedPersonName) ||
                !string.IsNullOrWhiteSpace(relatedPersonIdentityNumber) ||
                !string.IsNullOrWhiteSpace(relatedPersonPhoneNumber) ||
                !string.IsNullOrWhiteSpace(relatedPersonWhatsAppNumber);

            if (!normalizedRelatedPatientId.HasValue && !hasFreeTextRelatedPerson)
            {
                return (false, "Isi pasien terkait atau minimal nama, identitas, atau nomor kontak relasi.");
            }

            if (isLegalGuardian && !isResponsiblePerson)
            {
                return (false, "Wali hukum harus ditandai sebagai penanggung jawab.");
            }

            if (isEmergencyContact &&
                string.IsNullOrWhiteSpace(relatedPersonPhoneNumber) &&
                string.IsNullOrWhiteSpace(relatedPersonWhatsAppNumber) &&
                !normalizedRelatedPatientId.HasValue)
            {
                return (false, "Kontak darurat wajib memiliki nomor telepon, WhatsApp, atau terhubung ke pasien terkait.");
            }

            if (!string.IsNullOrWhiteSpace(relatedPersonEmail) && !relatedPersonEmail.Contains('@'))
            {
                return (false, "Format email relasi pasien tidak valid.");
            }

            if (!string.IsNullOrWhiteSpace(relatedPersonIdentityNumber))
            {
                var normalizedIdentityNumber = relatedPersonIdentityNumber.Trim().ToLower();

                var duplicateIdentity = await _dbContext.Set<MstPatientRelationship>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.PatientId == patientId &&
                        x.RelatedPersonIdentityNumber != null &&
                        x.RelatedPersonIdentityNumber.ToLower() == normalizedIdentityNumber &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateIdentity)
                {
                    return (false, "Nomor identitas relasi pasien sudah digunakan pada pasien ini.");
                }
            }

            if (normalizedRelatedPatientId.HasValue)
            {
                var duplicateRelatedPatient = await _dbContext.Set<MstPatientRelationship>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.PatientId == patientId &&
                        x.RelatedPatientId == normalizedRelatedPatientId.Value &&
                        x.RelationshipType == relationshipType &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateRelatedPatient)
                {
                    return (false, "Relasi pasien terkait dengan tipe relasi tersebut sudah ada.");
                }
            }

            return (true, null);
        }

        private async Task ResetPrimaryAsync(
            Guid patientId,
            Guid? excludeId,
            Guid actorUserId,
            DateTime now)
        {
            var query = _dbContext.Set<MstPatientRelationship>()
                .Where(x =>
                    !x.IsDelete &&
                    x.PatientId == patientId &&
                    x.IsPrimary);

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            var existingPrimaryRelationships = await query.ToListAsync();

            foreach (var item in existingPrimaryRelationships)
            {
                item.IsPrimary = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(
            IEnumerable<Guid> actorIds)
        {
            var ids = actorIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (!ids.Any())
            {
                return new Dictionary<Guid, string?>();
            }

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    Name =
                        x.DisplayName ??
                        x.UserName ??
                        x.Email ??
                        x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static PatientRelationshipResponse MapResponse(
            MstPatientRelationship entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new PatientRelationshipResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientCode = entity.Patient?.PatientCode ?? string.Empty,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber ?? string.Empty,
                PatientName = entity.Patient?.FullName ?? string.Empty,
                RelatedPatientId = entity.RelatedPatientId,
                RelatedPatientCode = entity.RelatedPatient?.PatientCode,
                RelatedPatientMedicalRecordNumber = entity.RelatedPatient?.MedicalRecordNumber,
                RelatedPatientName = entity.RelatedPatient?.FullName,
                RelationshipType = entity.RelationshipType,
                RelationshipTypeName = BuildEnumLabel(entity.RelationshipType),
                RelatedPersonName = entity.RelatedPersonName,
                RelatedPersonIdentityType = entity.RelatedPersonIdentityType,
                RelatedPersonIdentityNumber = entity.RelatedPersonIdentityNumber,
                RelatedPersonPhoneNumber = entity.RelatedPersonPhoneNumber,
                RelatedPersonWhatsAppNumber = entity.RelatedPersonWhatsAppNumber,
                RelatedPersonEmail = entity.RelatedPersonEmail,
                IsPrimary = entity.IsPrimary,
                IsEmergencyContact = entity.IsEmergencyContact,
                IsResponsiblePerson = entity.IsResponsiblePerson,
                IsLegalGuardian = entity.IsLegalGuardian,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static PatientRelationshipDetailResponse MapDetailResponse(
            MstPatientRelationship entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = new PatientRelationshipDetailResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientCode = entity.Patient?.PatientCode ?? string.Empty,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber ?? string.Empty,
                PatientName = entity.Patient?.FullName ?? string.Empty,
                RelatedPatientId = entity.RelatedPatientId,
                RelatedPatientCode = entity.RelatedPatient?.PatientCode,
                RelatedPatientMedicalRecordNumber = entity.RelatedPatient?.MedicalRecordNumber,
                RelatedPatientName = entity.RelatedPatient?.FullName,
                RelationshipType = entity.RelationshipType,
                RelationshipTypeName = BuildEnumLabel(entity.RelationshipType),
                RelatedPersonName = entity.RelatedPersonName,
                RelatedPersonIdentityType = entity.RelatedPersonIdentityType,
                RelatedPersonIdentityNumber = entity.RelatedPersonIdentityNumber,
                RelatedPersonPhoneNumber = entity.RelatedPersonPhoneNumber,
                RelatedPersonWhatsAppNumber = entity.RelatedPersonWhatsAppNumber,
                RelatedPersonEmail = entity.RelatedPersonEmail,
                RelatedPersonAddress = entity.RelatedPersonAddress,
                IsPrimary = entity.IsPrimary,
                IsEmergencyContact = entity.IsEmergencyContact,
                IsResponsiblePerson = entity.IsResponsiblePerson,
                IsLegalGuardian = entity.IsLegalGuardian,
                Notes = entity.Notes,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            return response;
        }

        private static PatientRelationshipOptionResponse MapOptionResponse(MstPatientRelationship entity)
        {
            var displayName = entity.RelatedPatient?.FullName ?? entity.RelatedPersonName ?? string.Empty;

            return new PatientRelationshipOptionResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientName = entity.Patient?.FullName ?? string.Empty,
                RelatedPatientId = entity.RelatedPatientId,
                RelatedPatientName = entity.RelatedPatient?.FullName,
                RelationshipType = entity.RelationshipType,
                RelationshipTypeName = BuildEnumLabel(entity.RelationshipType),
                DisplayName = displayName,
                IsPrimary = entity.IsPrimary,
                IsEmergencyContact = entity.IsEmergencyContact,
                IsResponsiblePerson = entity.IsResponsiblePerson,
                IsLegalGuardian = entity.IsLegalGuardian
            };
        }

        private static List<PatientRelationshipEnumOptionResponse> BuildEnumOptions<TEnum>()
            where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new PatientRelationshipEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = BuildEnumLabel(x)
                })
                .ToList();
        }

        private static string BuildEnumLabel<TEnum>(TEnum value)
            where TEnum : Enum
        {
            return SplitPascalCase(value.ToString());
        }

        private static string SplitPascalCase(string value)
        {
            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private static string? GetActorName(
            IReadOnlyDictionary<Guid, string?> actorNames,
            Guid actorId)
        {
            if (actorId == Guid.Empty)
            {
                return null;
            }

            return actorNames.TryGetValue(actorId, out var actorName)
                ? actorName
                : null;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 25;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            return (pageNumber, pageSize);
        }

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string? NormalizeLowerNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToLowerInvariant();
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
            {
                return null;
            }

            return value.Value;
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
