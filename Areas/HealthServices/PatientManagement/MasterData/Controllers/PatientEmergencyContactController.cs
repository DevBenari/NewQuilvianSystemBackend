using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponsePatientEmergencyContactPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs.PatientEmergencyContactResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/patient-management/master-data/patient-emergency-contacts")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PATIENT_MANAGEMENT",
        moduleName: "Health Service Patient Management",
        displayName: "Patient Emergency Contact",
        AreaName = "HealthServices",
        ControllerName = "PatientEmergencyContact",
        Description = "Health service patient management master data patient emergency contact",
        SortOrder = 4
    )]
    [Tags("Health Services / Patient Management / Patient Emergency Contact")]
    public class PatientEmergencyContactController : ControllerBase
    {
        private const string LogCategory = "HealthServices.PatientManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PatientEmergencyContactController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientEmergencyContactFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Emergency Contact", Description = "Melihat data patient emergency contact", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientEmergencyContact", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientEmergencyContactFilterMetadataResponse
            {
                DefaultFilter = new PatientEmergencyContactDefaultFilterResponse(),
                SortOptions = new List<PatientEmergencyContactSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "patientName", Label = "Nama pasien" },
                    new() { Value = "medicalRecordNumber", Label = "Nomor rekam medis" },
                    new() { Value = "contactName", Label = "Nama kontak" },
                    new() { Value = "relationship", Label = "Hubungan" },
                    new() { Value = "phoneNumber", Label = "Nomor telepon" },
                    new() { Value = "isPrimary", Label = "Kontak utama" },
                    new() { Value = "isResponsiblePerson", Label = "Penanggung jawab" },
                    new() { Value = "isSameAddressAsPatient", Label = "Alamat sama dengan pasien" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientEmergencyContact.GetFilterMetadata",
                "Mengambil metadata filter patient emergency contact.",
                result
            );

            return Ok(ApiResponse<PatientEmergencyContactFilterMetadataResponse>.Ok(
                result,
                "Metadata filter patient emergency contact berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<PatientEmergencyContactSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Emergency Contact", Description = "Melihat data patient emergency contact", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientEmergencyContact", "Read")]
        public async Task<IActionResult> GetSummary([FromQuery] Guid? patientId)
        {
            var query = _dbContext.Set<MstPatientEmergencyContact>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            var result = new PatientEmergencyContactSummaryResponse
            {
                TotalEmergencyContact = await query.CountAsync(),
                ActiveEmergencyContact = await query.CountAsync(x => x.IsActive),
                InactiveEmergencyContact = await query.CountAsync(x => !x.IsActive),
                PrimaryEmergencyContact = await query.CountAsync(x => x.IsPrimary),
                ResponsiblePersonContact = await query.CountAsync(x => x.IsResponsiblePerson),
                SameAddressAsPatientContact = await query.CountAsync(x => x.IsSameAddressAsPatient)
            };

            return Ok(ApiResponse<PatientEmergencyContactSummaryResponse>.Ok(
                result,
                "Ringkasan patient emergency contact berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientEmergencyContactPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Emergency Contact", Description = "Melihat data patient emergency contact", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientEmergencyContact", "Read")]
        public async Task<IActionResult> GetPatientEmergencyContacts(
            [FromQuery] string? search,
            [FromQuery] Guid? patientId,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isResponsiblePerson,
            [FromQuery] bool? isSameAddressAsPatient,
            [FromQuery] bool? isActive,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstPatientEmergencyContact>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            query = ApplyFilters(
                query,
                search,
                patientId,
                isPrimary,
                isResponsiblePerson,
                isSameAddressAsPatient,
                isActive
            );

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PatientEmergencyContactResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientCode = x.Patient != null ? x.Patient.PatientCode : string.Empty,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                    PatientFullName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    ContactName = x.ContactName,
                    Relationship = x.Relationship,
                    IdentityType = x.IdentityType,
                    IdentityNumber = x.IdentityNumber,
                    PhoneNumber = x.PhoneNumber,
                    WhatsAppNumber = x.WhatsAppNumber,
                    Email = x.Email,
                    Address = x.Address,
                    IsPrimary = x.IsPrimary,
                    IsResponsiblePerson = x.IsResponsiblePerson,
                    IsSameAddressAsPatient = x.IsSameAddressAsPatient,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new ResponsePatientEmergencyContactPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePatientEmergencyContactPagedResult>.Ok(
                result,
                "Data patient emergency contact berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientEmergencyContactOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Emergency Contact", Description = "Melihat data patient emergency contact", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientEmergencyContact", "Read")]
        public async Task<IActionResult> GetPatientEmergencyContactOptions(
            [FromQuery] Guid? patientId,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isResponsiblePerson,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<MstPatientEmergencyContact>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (isPrimary.HasValue)
                query = query.Where(x => x.IsPrimary == isPrimary.Value);

            if (isResponsiblePerson.HasValue)
                query = query.Where(x => x.IsResponsiblePerson == isResponsiblePerson.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ContactName.ToLower().Contains(keyword) ||
                    (x.Relationship != null && x.Relationship.ToLower().Contains(keyword)) ||
                    (x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(keyword)) ||
                    (x.WhatsAppNumber != null && x.WhatsAppNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.IsResponsiblePerson)
                .ThenBy(x => x.ContactName)
                .Select(x => new PatientEmergencyContactOptionResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientFullName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    ContactName = x.ContactName,
                    Relationship = x.Relationship,
                    PhoneNumber = x.PhoneNumber,
                    WhatsAppNumber = x.WhatsAppNumber,
                    IsPrimary = x.IsPrimary,
                    IsResponsiblePerson = x.IsResponsiblePerson
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientEmergencyContactOptionResponse>>.Ok(
                data,
                "Data pilihan patient emergency contact berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientEmergencyContactDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Emergency Contact", Description = "Melihat data patient emergency contact", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientEmergencyContact", "Read")]
        public async Task<IActionResult> GetPatientEmergencyContactById(Guid id)
        {
            var data = await _dbContext.Set<MstPatientEmergencyContact>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new PatientEmergencyContactDetailResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientCode = x.Patient != null ? x.Patient.PatientCode : string.Empty,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                    PatientFullName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    ContactName = x.ContactName,
                    Relationship = x.Relationship,
                    IdentityType = x.IdentityType,
                    IdentityNumber = x.IdentityNumber,
                    PhoneNumber = x.PhoneNumber,
                    WhatsAppNumber = x.WhatsAppNumber,
                    Email = x.Email,
                    Address = x.Address,
                    IsPrimary = x.IsPrimary,
                    IsResponsiblePerson = x.IsResponsiblePerson,
                    IsSameAddressAsPatient = x.IsSameAddressAsPatient,
                    Notes = x.Notes,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient emergency contact tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientEmergencyContactDetailResponse>.Ok(
                data,
                "Detail patient emergency contact berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientEmergencyContactCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Patient Emergency Contact", Description = "Membuat data patient emergency contact", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PatientEmergencyContact", "Create")]
        public async Task<IActionResult> CreatePatientEmergencyContact([FromBody] CreatePatientEmergencyContactRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                patientId: request.PatientId,
                contactName: request.ContactName,
                phoneNumber: request.PhoneNumber,
                whatsAppNumber: request.WhatsAppNumber,
                email: request.Email,
                identityType: request.IdentityType,
                identityNumber: request.IdentityNumber
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data patient emergency contact tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (request.IsPrimary)
                await ResetPrimaryContactAsync(request.PatientId, null, actorUserId, now);

            if (request.IsResponsiblePerson)
                await ResetResponsiblePersonAsync(request.PatientId, null, actorUserId, now);

            var entity = new MstPatientEmergencyContact
            {
                Id = Guid.NewGuid(),
                PatientId = request.PatientId,
                ContactName = request.ContactName.Trim(),
                Relationship = NormalizeNullableText(request.Relationship),
                IdentityType = NormalizeNullableText(request.IdentityType),
                IdentityNumber = NormalizeNullableText(request.IdentityNumber),
                PhoneNumber = NormalizeNullableText(request.PhoneNumber),
                WhatsAppNumber = NormalizeNullableText(request.WhatsAppNumber),
                Email = NormalizeNullableEmail(request.Email),
                Address = NormalizeNullableText(request.Address),
                IsPrimary = request.IsPrimary,
                IsResponsiblePerson = request.IsResponsiblePerson,
                IsSameAddressAsPatient = request.IsSameAddressAsPatient,
                Notes = NormalizeNullableText(request.Notes),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstPatientEmergencyContact>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = new PatientEmergencyContactCreateResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                ContactName = entity.ContactName,
                Relationship = entity.Relationship,
                IsPrimary = entity.IsPrimary,
                IsResponsiblePerson = entity.IsResponsiblePerson,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<PatientEmergencyContactCreateResponse>.Ok(
                response,
                "Patient emergency contact berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Emergency Contact", Description = "Mengubah data patient emergency contact", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientEmergencyContact", "Update")]
        public async Task<IActionResult> UpdatePatientEmergencyContact(Guid id, [FromBody] UpdatePatientEmergencyContactRequest request)
        {
            var entity = await _dbContext.Set<MstPatientEmergencyContact>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient emergency contact tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                patientId: request.PatientId,
                contactName: request.ContactName,
                phoneNumber: request.PhoneNumber,
                whatsAppNumber: request.WhatsAppNumber,
                email: request.Email,
                identityType: request.IdentityType,
                identityNumber: request.IdentityNumber
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data patient emergency contact tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (request.IsPrimary)
                await ResetPrimaryContactAsync(request.PatientId, id, actorUserId, now);

            if (request.IsResponsiblePerson)
                await ResetResponsiblePersonAsync(request.PatientId, id, actorUserId, now);

            entity.PatientId = request.PatientId;
            entity.ContactName = request.ContactName.Trim();
            entity.Relationship = NormalizeNullableText(request.Relationship);
            entity.IdentityType = NormalizeNullableText(request.IdentityType);
            entity.IdentityNumber = NormalizeNullableText(request.IdentityNumber);
            entity.PhoneNumber = NormalizeNullableText(request.PhoneNumber);
            entity.WhatsAppNumber = NormalizeNullableText(request.WhatsAppNumber);
            entity.Email = NormalizeNullableEmail(request.Email);
            entity.Address = NormalizeNullableText(request.Address);
            entity.IsPrimary = request.IsPrimary;
            entity.IsResponsiblePerson = request.IsResponsiblePerson;
            entity.IsSameAddressAsPatient = request.IsSameAddressAsPatient;
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient emergency contact berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Patient Emergency Contact", Description = "Menghapus data patient emergency contact", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("PatientEmergencyContact", "Delete")]
        public async Task<IActionResult> DeletePatientEmergencyContact(Guid id)
        {
            var entity = await _dbContext.Set<MstPatientEmergencyContact>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient emergency contact tidak ditemukan."
                ));
            }

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsPrimary = false;
            entity.IsResponsiblePerson = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient emergency contact berhasil dihapus."
            ));
        }

        private static IQueryable<MstPatientEmergencyContact> ApplyFilters(
            IQueryable<MstPatientEmergencyContact> query,
            string? search,
            Guid? patientId,
            bool? isPrimary,
            bool? isResponsiblePerson,
            bool? isSameAddressAsPatient,
            bool? isActive)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ContactName.ToLower().Contains(keyword) ||
                    (x.Relationship != null && x.Relationship.ToLower().Contains(keyword)) ||
                    (x.IdentityType != null && x.IdentityType.ToLower().Contains(keyword)) ||
                    (x.IdentityNumber != null && x.IdentityNumber.ToLower().Contains(keyword)) ||
                    (x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(keyword)) ||
                    (x.WhatsAppNumber != null && x.WhatsAppNumber.ToLower().Contains(keyword)) ||
                    (x.Email != null && x.Email.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.PatientCode.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)));
            }

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (isPrimary.HasValue)
                query = query.Where(x => x.IsPrimary == isPrimary.Value);

            if (isResponsiblePerson.HasValue)
                query = query.Where(x => x.IsResponsiblePerson == isResponsiblePerson.Value);

            if (isSameAddressAsPatient.HasValue)
                query = query.Where(x => x.IsSameAddressAsPatient == isSameAddressAsPatient.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            Guid patientId,
            string contactName,
            string? phoneNumber,
            string? whatsAppNumber,
            string? email,
            string? identityType,
            string? identityNumber)
        {
            if (patientId == Guid.Empty)
                return (false, "Pasien wajib dipilih.");

            if (string.IsNullOrWhiteSpace(contactName))
                return (false, "Nama kontak wajib diisi.");

            var patientExists = await _dbContext.Set<MstPatient>()
                .AnyAsync(x => x.Id == patientId && x.IsActive && !x.IsDelete);

            if (!patientExists)
                return (false, "Pasien tidak valid atau tidak aktif.");

            if (!string.IsNullOrWhiteSpace(email) && !email.Contains('@'))
                return (false, "Format email kontak tidak valid.");

            var normalizedContactName = contactName.Trim().ToLower();
            var normalizedPhoneNumber = NormalizeNullableText(phoneNumber)?.ToLower();
            var normalizedWhatsAppNumber = NormalizeNullableText(whatsAppNumber)?.ToLower();
            var normalizedIdentityType = NormalizeNullableText(identityType)?.ToLower();
            var normalizedIdentityNumber = NormalizeNullableText(identityNumber)?.ToLower();

            var duplicateContactName = await _dbContext.Set<MstPatientEmergencyContact>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.PatientId == patientId &&
                    x.ContactName.ToLower() == normalizedContactName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateContactName)
                return (false, "Nama kontak darurat pada pasien tersebut sudah digunakan.");

            if (!string.IsNullOrWhiteSpace(normalizedPhoneNumber))
            {
                var duplicatePhone = await _dbContext.Set<MstPatientEmergencyContact>()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.PatientId == patientId &&
                        x.PhoneNumber != null &&
                        x.PhoneNumber.ToLower() == normalizedPhoneNumber &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicatePhone)
                    return (false, "Nomor telepon kontak darurat pada pasien tersebut sudah digunakan.");
            }

            if (!string.IsNullOrWhiteSpace(normalizedWhatsAppNumber))
            {
                var duplicateWhatsApp = await _dbContext.Set<MstPatientEmergencyContact>()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.PatientId == patientId &&
                        x.WhatsAppNumber != null &&
                        x.WhatsAppNumber.ToLower() == normalizedWhatsAppNumber &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateWhatsApp)
                    return (false, "Nomor WhatsApp kontak darurat pada pasien tersebut sudah digunakan.");
            }

            if (!string.IsNullOrWhiteSpace(normalizedIdentityType) &&
                !string.IsNullOrWhiteSpace(normalizedIdentityNumber))
            {
                var duplicateIdentity = await _dbContext.Set<MstPatientEmergencyContact>()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.PatientId == patientId &&
                        x.IdentityType != null &&
                        x.IdentityNumber != null &&
                        x.IdentityType.ToLower() == normalizedIdentityType &&
                        x.IdentityNumber.ToLower() == normalizedIdentityNumber &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateIdentity)
                    return (false, "Identitas kontak darurat pada pasien tersebut sudah digunakan.");
            }

            return (true, null);
        }

        private async Task ResetPrimaryContactAsync(
            Guid patientId,
            Guid? excludeId,
            Guid actorUserId,
            DateTime now)
        {
            var query = _dbContext.Set<MstPatientEmergencyContact>()
                .Where(x =>
                    x.PatientId == patientId &&
                    x.IsPrimary &&
                    !x.IsDelete);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            var existingPrimary = await query.ToListAsync();

            foreach (var item in existingPrimary)
            {
                item.IsPrimary = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
        }

        private async Task ResetResponsiblePersonAsync(
            Guid patientId,
            Guid? excludeId,
            Guid actorUserId,
            DateTime now)
        {
            var query = _dbContext.Set<MstPatientEmergencyContact>()
                .Where(x =>
                    x.PatientId == patientId &&
                    x.IsResponsiblePerson &&
                    !x.IsDelete);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            var existingResponsiblePersons = await query.ToListAsync();

            foreach (var item in existingResponsiblePersons)
            {
                item.IsResponsiblePerson = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
        }

        private static IQueryable<MstPatientEmergencyContact> ApplySorting(
            IQueryable<MstPatientEmergencyContact> query,
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

                "contactname" => isDesc
                    ? query.OrderByDescending(x => x.ContactName)
                    : query.OrderBy(x => x.ContactName),

                "relationship" => isDesc
                    ? query.OrderByDescending(x => x.Relationship)
                    : query.OrderBy(x => x.Relationship),

                "phonenumber" => isDesc
                    ? query.OrderByDescending(x => x.PhoneNumber)
                    : query.OrderBy(x => x.PhoneNumber),

                "isprimary" => isDesc
                    ? query.OrderByDescending(x => x.IsPrimary)
                    : query.OrderBy(x => x.IsPrimary),

                "isresponsibleperson" => isDesc
                    ? query.OrderByDescending(x => x.IsResponsiblePerson)
                    : query.OrderBy(x => x.IsResponsiblePerson),

                "issameaddressaspatient" => isDesc
                    ? query.OrderByDescending(x => x.IsSameAddressAsPatient)
                    : query.OrderBy(x => x.IsSameAddressAsPatient),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime)
            };
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

        private static string? NormalizeNullableEmail(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToLowerInvariant();
        }
    }
}
