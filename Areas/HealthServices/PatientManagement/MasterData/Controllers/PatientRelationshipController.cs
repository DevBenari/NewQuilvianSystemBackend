using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
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
        moduleCode: "HEALTH_SERVICE_PATIENT_MANAGEMENT",
        moduleName: "Health Service Patient Management",
        displayName: "Patient Relationship",
        AreaName = "HealthServices",
        ControllerName = "PatientRelationship",
        Description = "Health service patient management master data patient relationship",
        SortOrder = 3
    )]
    [Tags("Health Services / Patient Management / Patient Relationship")]
    public class PatientRelationshipController : ControllerBase
    {
        private const string LogCategory = "HealthServices.PatientManagement";

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
        [AccessAction("Read", "Read Patient Relationship", Description = "Melihat data patient relationship", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientRelationship", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientRelationshipFilterMetadataResponse
            {
                DefaultFilter = new PatientRelationshipDefaultFilterResponse(),
                SortOptions = new List<PatientRelationshipSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "patientName", Label = "Nama pasien" },
                    new() { Value = "medicalRecordNumber", Label = "Nomor rekam medis" },
                    new() { Value = "relatedPatientName", Label = "Nama pasien terkait" },
                    new() { Value = "relationshipType", Label = "Tipe relasi" },
                    new() { Value = "relatedPersonName", Label = "Nama keluarga/relasi" },
                    new() { Value = "isPrimary", Label = "Relasi utama" },
                    new() { Value = "isEmergencyContact", Label = "Kontak darurat" },
                    new() { Value = "isResponsiblePerson", Label = "Penanggung jawab" },
                    new() { Value = "isLegalGuardian", Label = "Wali hukum" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                RelationshipTypeOptions = BuildEnumOptions<PatientRelationshipType>()
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
        [AccessAction("Read", "Read Patient Relationship", Description = "Melihat data patient relationship", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientRelationship", "Read")]
        public async Task<IActionResult> GetSummary([FromQuery] Guid? patientId)
        {
            var query = _dbContext.Set<MstPatientRelationship>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

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
        [AccessAction("Read", "Read Patient Relationship", Description = "Melihat data patient relationship", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientRelationship", "Read")]
        public async Task<IActionResult> GetPatientRelationships(
            [FromQuery] string? search,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? relatedPatientId,
            [FromQuery] PatientRelationshipType? relationshipType,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isEmergencyContact,
            [FromQuery] bool? isResponsiblePerson,
            [FromQuery] bool? isLegalGuardian,
            [FromQuery] bool? isActive,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstPatientRelationship>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            query = ApplyFilters(
                query,
                search,
                patientId,
                relatedPatientId,
                relationshipType,
                isPrimary,
                isEmergencyContact,
                isResponsiblePerson,
                isLegalGuardian,
                isActive
            );

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PatientRelationshipResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientCode = x.Patient != null ? x.Patient.PatientCode : string.Empty,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    RelatedPatientId = x.RelatedPatientId,
                    RelatedPatientCode = x.RelatedPatient != null ? x.RelatedPatient.PatientCode : null,
                    RelatedPatientMedicalRecordNumber = x.RelatedPatient != null ? x.RelatedPatient.MedicalRecordNumber : null,
                    RelatedPatientName = x.RelatedPatient != null ? x.RelatedPatient.FullName : null,
                    RelationshipType = x.RelationshipType,
                    RelationshipTypeName = x.RelationshipType.ToString(),
                    RelatedPersonName = x.RelatedPersonName,
                    RelatedPersonIdentityType = x.RelatedPersonIdentityType,
                    RelatedPersonIdentityNumber = x.RelatedPersonIdentityNumber,
                    RelatedPersonPhoneNumber = x.RelatedPersonPhoneNumber,
                    RelatedPersonWhatsAppNumber = x.RelatedPersonWhatsAppNumber,
                    RelatedPersonEmail = x.RelatedPersonEmail,
                    IsPrimary = x.IsPrimary,
                    IsEmergencyContact = x.IsEmergencyContact,
                    IsResponsiblePerson = x.IsResponsiblePerson,
                    IsLegalGuardian = x.IsLegalGuardian,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

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
        [ProducesResponseType(typeof(ApiResponse<List<PatientRelationshipOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Relationship", Description = "Melihat data patient relationship", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientRelationship", "Read")]
        public async Task<IActionResult> GetPatientRelationshipOptions(
            [FromQuery] Guid? patientId,
            [FromQuery] PatientRelationshipType? relationshipType,
            [FromQuery] bool? isEmergencyContact,
            [FromQuery] bool? isResponsiblePerson,
            [FromQuery] bool? isLegalGuardian,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<MstPatientRelationship>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (relationshipType.HasValue)
                query = query.Where(x => x.RelationshipType == relationshipType.Value);

            if (isEmergencyContact.HasValue)
                query = query.Where(x => x.IsEmergencyContact == isEmergencyContact.Value);

            if (isResponsiblePerson.HasValue)
                query = query.Where(x => x.IsResponsiblePerson == isResponsiblePerson.Value);

            if (isLegalGuardian.HasValue)
                query = query.Where(x => x.IsLegalGuardian == isLegalGuardian.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    (x.RelatedPersonName != null && x.RelatedPersonName.ToLower().Contains(keyword)) ||
                    (x.RelatedPersonIdentityNumber != null && x.RelatedPersonIdentityNumber.ToLower().Contains(keyword)) ||
                    (x.RelatedPersonPhoneNumber != null && x.RelatedPersonPhoneNumber.ToLower().Contains(keyword)) ||
                    (x.RelatedPersonWhatsAppNumber != null && x.RelatedPersonWhatsAppNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.RelatedPatient != null && x.RelatedPatient.FullName.ToLower().Contains(keyword)) ||
                    (x.RelatedPatient != null && x.RelatedPatient.MedicalRecordNumber.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.IsResponsiblePerson)
                .ThenByDescending(x => x.IsEmergencyContact)
                .ThenBy(x => x.RelationshipType)
                .ThenBy(x => x.RelatedPersonName ?? (x.RelatedPatient != null ? x.RelatedPatient.FullName : string.Empty))
                .Select(x => new PatientRelationshipOptionResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    RelatedPatientId = x.RelatedPatientId,
                    RelatedPatientName = x.RelatedPatient != null ? x.RelatedPatient.FullName : null,
                    RelationshipType = x.RelationshipType,
                    RelationshipTypeName = x.RelationshipType.ToString(),
                    DisplayName = x.RelatedPatient != null
                        ? x.RelatedPatient.FullName
                        : (x.RelatedPersonName ?? string.Empty),
                    IsPrimary = x.IsPrimary,
                    IsEmergencyContact = x.IsEmergencyContact,
                    IsResponsiblePerson = x.IsResponsiblePerson,
                    IsLegalGuardian = x.IsLegalGuardian
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientRelationshipOptionResponse>>.Ok(
                data,
                "Data pilihan patient relationship berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientRelationshipDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Relationship", Description = "Melihat data patient relationship", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientRelationship", "Read")]
        public async Task<IActionResult> GetPatientRelationshipById(Guid id)
        {
            var data = await _dbContext.Set<MstPatientRelationship>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new PatientRelationshipDetailResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientCode = x.Patient != null ? x.Patient.PatientCode : string.Empty,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    RelatedPatientId = x.RelatedPatientId,
                    RelatedPatientCode = x.RelatedPatient != null ? x.RelatedPatient.PatientCode : null,
                    RelatedPatientMedicalRecordNumber = x.RelatedPatient != null ? x.RelatedPatient.MedicalRecordNumber : null,
                    RelatedPatientName = x.RelatedPatient != null ? x.RelatedPatient.FullName : null,
                    RelationshipType = x.RelationshipType,
                    RelationshipTypeName = x.RelationshipType.ToString(),
                    RelatedPersonName = x.RelatedPersonName,
                    RelatedPersonIdentityType = x.RelatedPersonIdentityType,
                    RelatedPersonIdentityNumber = x.RelatedPersonIdentityNumber,
                    RelatedPersonPhoneNumber = x.RelatedPersonPhoneNumber,
                    RelatedPersonWhatsAppNumber = x.RelatedPersonWhatsAppNumber,
                    RelatedPersonEmail = x.RelatedPersonEmail,
                    RelatedPersonAddress = x.RelatedPersonAddress,
                    IsPrimary = x.IsPrimary,
                    IsEmergencyContact = x.IsEmergencyContact,
                    IsResponsiblePerson = x.IsResponsiblePerson,
                    IsLegalGuardian = x.IsLegalGuardian,
                    Notes = x.Notes,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient relationship tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientRelationshipDetailResponse>.Ok(
                data,
                "Detail patient relationship berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientRelationshipCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Patient Relationship", Description = "Membuat data patient relationship", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PatientRelationship", "Create")]
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
                await ResetPrimaryAsync(request.PatientId, null, actorUserId, now);

            var entity = new MstPatientRelationship
            {
                Id = Guid.NewGuid(),
                PatientId = request.PatientId,
                RelatedPatientId = normalizedRelatedPatientId,
                RelationshipType = request.RelationshipType,
                RelatedPersonName = NormalizeNullableText(request.RelatedPersonName),
                RelatedPersonIdentityType = NormalizeNullableText(request.RelatedPersonIdentityType),
                RelatedPersonIdentityNumber = NormalizeNullableText(request.RelatedPersonIdentityNumber),
                RelatedPersonPhoneNumber = NormalizeNullableText(request.RelatedPersonPhoneNumber),
                RelatedPersonWhatsAppNumber = NormalizeNullableText(request.RelatedPersonWhatsAppNumber),
                RelatedPersonEmail = NormalizeNullableText(request.RelatedPersonEmail),
                RelatedPersonAddress = NormalizeNullableText(request.RelatedPersonAddress),
                IsPrimary = request.IsPrimary,
                IsEmergencyContact = request.IsEmergencyContact,
                IsResponsiblePerson = request.IsResponsiblePerson,
                IsLegalGuardian = request.IsLegalGuardian,
                Notes = NormalizeNullableText(request.Notes),
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
                RelatedPersonName = entity.RelatedPersonName,
                IsPrimary = entity.IsPrimary,
                IsEmergencyContact = entity.IsEmergencyContact,
                IsResponsiblePerson = entity.IsResponsiblePerson,
                IsLegalGuardian = entity.IsLegalGuardian,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<PatientRelationshipCreateResponse>.Ok(
                response,
                "Patient relationship berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Relationship", Description = "Mengubah data patient relationship", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientRelationship", "Update")]
        public async Task<IActionResult> UpdatePatientRelationship(Guid id, [FromBody] UpdatePatientRelationshipRequest request)
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
                await ResetPrimaryAsync(request.PatientId, id, actorUserId, now);

            entity.PatientId = request.PatientId;
            entity.RelatedPatientId = NormalizeNullableGuid(request.RelatedPatientId);
            entity.RelationshipType = request.RelationshipType;
            entity.RelatedPersonName = NormalizeNullableText(request.RelatedPersonName);
            entity.RelatedPersonIdentityType = NormalizeNullableText(request.RelatedPersonIdentityType);
            entity.RelatedPersonIdentityNumber = NormalizeNullableText(request.RelatedPersonIdentityNumber);
            entity.RelatedPersonPhoneNumber = NormalizeNullableText(request.RelatedPersonPhoneNumber);
            entity.RelatedPersonWhatsAppNumber = NormalizeNullableText(request.RelatedPersonWhatsAppNumber);
            entity.RelatedPersonEmail = NormalizeNullableText(request.RelatedPersonEmail);
            entity.RelatedPersonAddress = NormalizeNullableText(request.RelatedPersonAddress);
            entity.IsPrimary = request.IsPrimary;
            entity.IsEmergencyContact = request.IsEmergencyContact;
            entity.IsResponsiblePerson = request.IsResponsiblePerson;
            entity.IsLegalGuardian = request.IsLegalGuardian;
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient relationship berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Patient Relationship", Description = "Menghapus data patient relationship", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("PatientRelationship", "Delete")]
        public async Task<IActionResult> DeletePatientRelationship(Guid id)
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

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient relationship berhasil dihapus."
            ));
        }

        private static IQueryable<MstPatientRelationship> ApplyFilters(
            IQueryable<MstPatientRelationship> query,
            string? search,
            Guid? patientId,
            Guid? relatedPatientId,
            PatientRelationshipType? relationshipType,
            bool? isPrimary,
            bool? isEmergencyContact,
            bool? isResponsiblePerson,
            bool? isLegalGuardian,
            bool? isActive)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

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
                    (x.RelatedPatient != null && x.RelatedPatient.FullName.ToLower().Contains(keyword)));
            }

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (relatedPatientId.HasValue && relatedPatientId.Value != Guid.Empty)
                query = query.Where(x => x.RelatedPatientId == relatedPatientId.Value);

            if (relationshipType.HasValue)
                query = query.Where(x => x.RelationshipType == relationshipType.Value);

            if (isPrimary.HasValue)
                query = query.Where(x => x.IsPrimary == isPrimary.Value);

            if (isEmergencyContact.HasValue)
                query = query.Where(x => x.IsEmergencyContact == isEmergencyContact.Value);

            if (isResponsiblePerson.HasValue)
                query = query.Where(x => x.IsResponsiblePerson == isResponsiblePerson.Value);

            if (isLegalGuardian.HasValue)
                query = query.Where(x => x.IsLegalGuardian == isLegalGuardian.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            return query;
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
            bool isPrimary,
            bool isEmergencyContact,
            bool isResponsiblePerson,
            bool isLegalGuardian)
        {
            if (patientId == Guid.Empty)
                return (false, "Pasien wajib dipilih.");

            var patientExists = await _dbContext.Set<MstPatient>()
                .AnyAsync(x => x.Id == patientId && x.IsActive && !x.IsDelete);

            if (!patientExists)
                return (false, "Pasien tidak valid atau tidak aktif.");

            var normalizedRelatedPatientId = NormalizeNullableGuid(relatedPatientId);

            if (normalizedRelatedPatientId.HasValue)
            {
                if (normalizedRelatedPatientId.Value == patientId)
                    return (false, "Pasien terkait tidak boleh sama dengan pasien utama.");

                var relatedPatientExists = await _dbContext.Set<MstPatient>()
                    .AnyAsync(x => x.Id == normalizedRelatedPatientId.Value && x.IsActive && !x.IsDelete);

                if (!relatedPatientExists)
                    return (false, "Pasien terkait tidak valid atau tidak aktif.");
            }

            var hasFreeTextRelatedPerson =
                !string.IsNullOrWhiteSpace(relatedPersonName) ||
                !string.IsNullOrWhiteSpace(relatedPersonIdentityNumber) ||
                !string.IsNullOrWhiteSpace(relatedPersonPhoneNumber) ||
                !string.IsNullOrWhiteSpace(relatedPersonWhatsAppNumber);

            if (!normalizedRelatedPatientId.HasValue && !hasFreeTextRelatedPerson)
                return (false, "Isi pasien terkait atau minimal nama/identitas/nomor kontak relasi.");

            if (relationshipType == PatientRelationshipType.Unknown)
                return (false, "Tipe relasi pasien wajib dipilih.");

            if (isLegalGuardian && !isResponsiblePerson)
                return (false, "Wali hukum sebaiknya juga ditandai sebagai penanggung jawab.");

            if (isEmergencyContact &&
                string.IsNullOrWhiteSpace(relatedPersonPhoneNumber) &&
                string.IsNullOrWhiteSpace(relatedPersonWhatsAppNumber) &&
                !normalizedRelatedPatientId.HasValue)
            {
                return (false, "Kontak darurat wajib memiliki nomor telepon, WhatsApp, atau terhubung ke pasien terkait.");
            }

            if (!string.IsNullOrWhiteSpace(relatedPersonIdentityNumber))
            {
                var normalizedIdentityNumber = relatedPersonIdentityNumber.Trim().ToLower();

                var duplicateIdentity = await _dbContext.Set<MstPatientRelationship>()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.PatientId == patientId &&
                        x.RelatedPersonIdentityNumber != null &&
                        x.RelatedPersonIdentityNumber.ToLower() == normalizedIdentityNumber &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateIdentity)
                    return (false, "Nomor identitas relasi pasien sudah digunakan pada pasien ini.");
            }

            if (normalizedRelatedPatientId.HasValue)
            {
                var duplicateRelatedPatient = await _dbContext.Set<MstPatientRelationship>()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.PatientId == patientId &&
                        x.RelatedPatientId == normalizedRelatedPatientId.Value &&
                        x.RelationshipType == relationshipType &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateRelatedPatient)
                    return (false, "Relasi pasien terkait dengan tipe relasi tersebut sudah ada.");
            }

            return (true, null);
        }

        private async Task ResetPrimaryAsync(Guid patientId, Guid? excludeId, Guid actorUserId, DateTime now)
        {
            var existingPrimaryRelationships = await _dbContext.Set<MstPatientRelationship>()
                .Where(x =>
                    !x.IsDelete &&
                    x.PatientId == patientId &&
                    x.IsPrimary &&
                    (!excludeId.HasValue || x.Id != excludeId.Value))
                .ToListAsync();

            foreach (var item in existingPrimaryRelationships)
            {
                item.IsPrimary = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
        }

        private static IQueryable<MstPatientRelationship> ApplySorting(
            IQueryable<MstPatientRelationship> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "createDateTime").ToLowerInvariant() switch
            {
                "patientname" => isDesc
                    ? query.OrderByDescending(x => x.Patient != null ? x.Patient.FullName : "")
                    : query.OrderBy(x => x.Patient != null ? x.Patient.FullName : ""),

                "medicalrecordnumber" => isDesc
                    ? query.OrderByDescending(x => x.Patient != null ? x.Patient.MedicalRecordNumber : "")
                    : query.OrderBy(x => x.Patient != null ? x.Patient.MedicalRecordNumber : ""),

                "relatedpatientname" => isDesc
                    ? query.OrderByDescending(x => x.RelatedPatient != null ? x.RelatedPatient.FullName : "")
                    : query.OrderBy(x => x.RelatedPatient != null ? x.RelatedPatient.FullName : ""),

                "relationshiptype" => isDesc
                    ? query.OrderByDescending(x => x.RelationshipType)
                    : query.OrderBy(x => x.RelationshipType),

                "relatedpersonname" => isDesc
                    ? query.OrderByDescending(x => x.RelatedPersonName)
                    : query.OrderBy(x => x.RelatedPersonName),

                "isprimary" => isDesc
                    ? query.OrderByDescending(x => x.IsPrimary)
                    : query.OrderBy(x => x.IsPrimary),

                "isemergencycontact" => isDesc
                    ? query.OrderByDescending(x => x.IsEmergencyContact)
                    : query.OrderBy(x => x.IsEmergencyContact),

                "isresponsibleperson" => isDesc
                    ? query.OrderByDescending(x => x.IsResponsiblePerson)
                    : query.OrderBy(x => x.IsResponsiblePerson),

                "islegalguardian" => isDesc
                    ? query.OrderByDescending(x => x.IsLegalGuardian)
                    : query.OrderBy(x => x.IsLegalGuardian),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime)
            };
        }

        private static List<PatientRelationshipEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new PatientRelationshipEnumOptionResponse
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

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private Guid GetCurrentUserId()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
                return null;

            return value.Value;
        }
    }
}