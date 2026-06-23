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
        moduleCode: "HEALTH_SERVICE_PATIENT_MANAGEMENT_MASTER_DATA",
        moduleName: "Health Service Patient Management Master Data",
        displayName: "Patient Emergency Contact",
        AreaName = "HealthServices",
        ControllerName = "PatientEmergencyContact",
        Description = "Health service patient management master data patient emergency contact",
        SortOrder = 4
    )]
    [Tags("Health Services / Patient Management / Master Data / Patient Emergency Contact")]
    public class PatientEmergencyContactController : ControllerBase
    {
        private const string LogCategory = "HealthServices.PatientManagement.MasterData";
        private const string KioskReadPolicy = "KioskRead";

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
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<PatientEmergencyContactFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Emergency Contact",
            Description = "Melihat metadata filter patient emergency contact",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientEmergencyContactFilterMetadataResponse
            {
                DefaultFilter = new PatientEmergencyContactDefaultFilterResponse(),
                CustomPeriods = new List<PatientEmergencyContactCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                RelationFilters = new List<PatientEmergencyContactRelationFilterResponse>
                {
                    new()
                    {
                        Value = "patientId",
                        Label = "Patient",
                        Endpoint = "/api/v1/health-services/patient-management/master-data/patients/options"
                    }
                },
                SortOptions = new List<PatientEmergencyContactSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "updateDateTime", Label = "Tanggal diperbarui" },
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
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ResetButtonLabel = "Reset"
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
        [AccessAction(
            "Read",
            "Read Patient Emergency Contact",
            Description = "Melihat ringkasan patient emergency contact",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("PatientEmergencyContact", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new PatientEmergencyContactSummaryResponse
            {
                TotalEmergencyContact = await query.CountAsync(),
                ActiveEmergencyContact = await query.CountAsync(x => x.IsActive),
                InactiveEmergencyContact = await query.CountAsync(x => !x.IsActive),
                PrimaryEmergencyContact = await query.CountAsync(x => x.IsPrimary),
                ResponsiblePersonContact = await query.CountAsync(x => x.IsResponsiblePerson),
                SameAddressAsPatientContact = await query.CountAsync(x => x.IsSameAddressAsPatient),
                WithPhoneNumberContact = await query.CountAsync(x =>
                    x.PhoneNumber != null &&
                    x.PhoneNumber != string.Empty),
                WithWhatsAppNumberContact = await query.CountAsync(x =>
                    x.WhatsAppNumber != null &&
                    x.WhatsAppNumber != string.Empty)
            };

            return Ok(ApiResponse<PatientEmergencyContactSummaryResponse>.Ok(
                result,
                "Ringkasan patient emergency contact berhasil diambil."
            ));
        }

        [HttpGet]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientEmergencyContactPagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Emergency Contact",
            Description = "Melihat data patient emergency contact",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        public async Task<IActionResult> GetPatientEmergencyContacts(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? patientId,
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
            query = ApplyRelationFilter(query, patientId);
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
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<PatientEmergencyContactOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Emergency Contact",
            Description = "Melihat data pilihan patient emergency contact",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        public async Task<IActionResult> GetPatientEmergencyContactOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] Guid? patientId = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyRelationFilter(query, patientId);
            query = ApplyStandardFilter(
                query,
                onlyActive ? true : null,
                search
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.IsResponsiblePerson)
                .ThenBy(x => x.ContactName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapOptionResponse)
                .ToList();

            var result = new PatientEmergencyContactOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PatientEmergencyContactOptionPagedResponse>.Ok(
                result,
                "Data pilihan patient emergency contact berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientEmergencyContactDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Patient Emergency Contact",
            Description = "Melihat detail patient emergency contact",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("PatientEmergencyContact", "Read")]
        public async Task<IActionResult> GetPatientEmergencyContactById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient emergency contact tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, actorNames);

            NormalizeActorInfo(data);

            return Ok(ApiResponse<PatientEmergencyContactDetailResponse>.Ok(
                data,
                "Detail patient emergency contact berhasil diambil."
            ));
        }

        [HttpPost]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<PatientEmergencyContactCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Create",
            "Create Patient Emergency Contact",
            Description = "Membuat data patient emergency contact",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        public async Task<IActionResult> CreatePatientEmergencyContact(
            [FromBody] CreatePatientEmergencyContactRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                request: request
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

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (request.IsPrimary)
                {
                    await UnsetOtherPrimaryContactAsync(
                        patientId: request.PatientId,
                        exceptId: null,
                        now: now,
                        actorUserId: actorUserId
                    );
                }

                if (request.IsResponsiblePerson)
                {
                    await UnsetOtherResponsiblePersonAsync(
                        patientId: request.PatientId,
                        exceptId: null,
                        now: now,
                        actorUserId: actorUserId
                    );
                }

                var entity = new MstPatientEmergencyContact
                {
                    Id = Guid.NewGuid(),
                    PatientId = request.PatientId,
                    ContactName = request.ContactName.Trim(),
                    Relationship = NormalizeNullableString(request.Relationship),
                    IdentityType = NormalizeNullableString(request.IdentityType),
                    IdentityNumber = NormalizeNullableString(request.IdentityNumber),
                    PhoneNumber = NormalizeNullableString(request.PhoneNumber),
                    WhatsAppNumber = NormalizeNullableString(request.WhatsAppNumber),
                    Email = NormalizeLowerNullableString(request.Email),
                    Address = NormalizeNullableString(request.Address),
                    IsPrimary = request.IsPrimary,
                    IsResponsiblePerson = request.IsResponsiblePerson,
                    IsSameAddressAsPatient = request.IsSameAddressAsPatient,
                    Notes = NormalizeNullableString(request.Notes),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<MstPatientEmergencyContact>().Add(entity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = await BuildCreateResponseAsync(entity.Id);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "PatientEmergencyContact.CreatePatientEmergencyContact",
                    "Membuat data patient emergency contact.",
                    result
                );

                return Ok(ApiResponse<PatientEmergencyContactCreateResponse>.Ok(
                    result,
                    "Patient emergency contact berhasil dibuat."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "PatientEmergencyContact.CreatePatientEmergencyContact",
                    "Gagal membuat data patient emergency contact.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat patient emergency contact."
                    )
                );
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Update",
            "Update Patient Emergency Contact",
            Description = "Mengubah data patient emergency contact",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("PatientEmergencyContact", "Update")]
        public async Task<IActionResult> UpdatePatientEmergencyContact(
            Guid id,
            [FromBody] UpdatePatientEmergencyContactRequest request)
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

            if ((request.IsPrimary || request.IsResponsiblePerson) && !request.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Kontak utama atau penanggung jawab harus aktif."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                request: request
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

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (request.IsPrimary)
                {
                    await UnsetOtherPrimaryContactAsync(
                        patientId: request.PatientId,
                        exceptId: id,
                        now: now,
                        actorUserId: actorUserId
                    );
                }

                if (request.IsResponsiblePerson)
                {
                    await UnsetOtherResponsiblePersonAsync(
                        patientId: request.PatientId,
                        exceptId: id,
                        now: now,
                        actorUserId: actorUserId
                    );
                }

                entity.PatientId = request.PatientId;
                entity.ContactName = request.ContactName.Trim();
                entity.Relationship = NormalizeNullableString(request.Relationship);
                entity.IdentityType = NormalizeNullableString(request.IdentityType);
                entity.IdentityNumber = NormalizeNullableString(request.IdentityNumber);
                entity.PhoneNumber = NormalizeNullableString(request.PhoneNumber);
                entity.WhatsAppNumber = NormalizeNullableString(request.WhatsAppNumber);
                entity.Email = NormalizeLowerNullableString(request.Email);
                entity.Address = NormalizeNullableString(request.Address);
                entity.IsPrimary = request.IsActive ? request.IsPrimary : false;
                entity.IsResponsiblePerson = request.IsActive ? request.IsResponsiblePerson : false;
                entity.IsSameAddressAsPatient = request.IsSameAddressAsPatient;
                entity.Notes = NormalizeNullableString(request.Notes);
                entity.IsActive = request.IsActive;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "PatientEmergencyContact.UpdatePatientEmergencyContact",
                    "Mengubah data patient emergency contact.",
                    new
                    {
                        entity.Id,
                        entity.PatientId,
                        entity.ContactName,
                        entity.IsActive
                    }
                );

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "Patient emergency contact berhasil diperbarui."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "PatientEmergencyContact.UpdatePatientEmergencyContact",
                    "Gagal mengubah data patient emergency contact.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat memperbarui patient emergency contact."
                    )
                );
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Patient Emergency Contact Status",
            Description = "Mengubah status patient emergency contact",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("PatientEmergencyContact", "Update")]
        public async Task<IActionResult> UpdatePatientEmergencyContactStatus(
            Guid id,
            [FromBody] UpdatePatientEmergencyContactStatusRequest request)
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

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;

            if (!request.IsActive)
            {
                entity.IsPrimary = false;
                entity.IsResponsiblePerson = false;
            }

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status patient emergency contact berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Patient Emergency Contact",
            Description = "Menghapus data patient emergency contact",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("PatientEmergencyContact", "Delete")]
        public async Task<IActionResult> DeletePatientEmergencyContact(
            Guid id,
            [FromBody] DeletePatientEmergencyContactRequest? request = null)
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

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsPrimary = false;
            entity.IsResponsiblePerson = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (!string.IsNullOrWhiteSpace(request?.DeleteReason))
            {
                entity.Notes = request.DeleteReason.Trim();
            }

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientEmergencyContact.DeletePatientEmergencyContact",
                "Menghapus data patient emergency contact.",
                new
                {
                    entity.Id,
                    entity.PatientId,
                    entity.ContactName,
                    entity.DeleteDateTime
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient emergency contact berhasil dihapus."
            ));
        }

        private IQueryable<MstPatientEmergencyContact> BuildBaseQuery()
        {
            return _dbContext.Set<MstPatientEmergencyContact>()
                .AsNoTracking()
                .Include(x => x.Patient)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstPatientEmergencyContact> ApplyDateFilter(
            IQueryable<MstPatientEmergencyContact> query,
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
                var today = DateTime.UtcNow.Date;

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
                        var thisMonthStart = new DateTime(
                            today.Year,
                            today.Month,
                            1,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc
                        );

                        query = query.Where(x =>
                            x.CreateDateTime >= thisMonthStart &&
                            x.CreateDateTime < thisMonthStart.AddMonths(1));
                        break;

                    case "lastmonth":
                        var currentMonthStart = new DateTime(
                            today.Year,
                            today.Month,
                            1,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc
                        );

                        var lastMonthStart = currentMonthStart.AddMonths(-1);

                        query = query.Where(x =>
                            x.CreateDateTime >= lastMonthStart &&
                            x.CreateDateTime < currentMonthStart);
                        break;
                }
            }

            return query;
        }

        private static IQueryable<MstPatientEmergencyContact> ApplyRelationFilter(
            IQueryable<MstPatientEmergencyContact> query,
            Guid? patientId)
        {
            var normalizedPatientId = NormalizeNullableGuid(patientId);

            if (normalizedPatientId.HasValue)
            {
                query = query.Where(x => x.PatientId == normalizedPatientId.Value);
            }

            return query;
        }

        private static IQueryable<MstPatientEmergencyContact> ApplyStandardFilter(
            IQueryable<MstPatientEmergencyContact> query,
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

                query = query.Where(x =>
                    x.ContactName.ToLower().Contains(keyword) ||
                    (x.Relationship != null && x.Relationship.ToLower().Contains(keyword)) ||
                    (x.IdentityType != null && x.IdentityType.ToLower().Contains(keyword)) ||
                    (x.IdentityNumber != null && x.IdentityNumber.ToLower().Contains(keyword)) ||
                    (x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(keyword)) ||
                    (x.WhatsAppNumber != null && x.WhatsAppNumber.ToLower().Contains(keyword)) ||
                    (x.Email != null && x.Email.ToLower().Contains(keyword)) ||
                    (x.Address != null && x.Address.ToLower().Contains(keyword)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.PatientCode.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstPatientEmergencyContact> ApplySorting(
            IQueryable<MstPatientEmergencyContact> query,
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

                "contactname" => isDescending
                    ? query.OrderByDescending(x => x.ContactName)
                    : query.OrderBy(x => x.ContactName),

                "relationship" => isDescending
                    ? query.OrderByDescending(x => x.Relationship)
                    : query.OrderBy(x => x.Relationship),

                "phonenumber" => isDescending
                    ? query.OrderByDescending(x => x.PhoneNumber)
                    : query.OrderBy(x => x.PhoneNumber),

                "isprimary" => isDescending
                    ? query.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.ContactName)
                    : query.OrderBy(x => x.IsPrimary).ThenBy(x => x.ContactName),

                "isresponsibleperson" => isDescending
                    ? query.OrderByDescending(x => x.IsResponsiblePerson).ThenBy(x => x.ContactName)
                    : query.OrderBy(x => x.IsResponsiblePerson).ThenBy(x => x.ContactName),

                "issameaddressaspatient" => isDescending
                    ? query.OrderByDescending(x => x.IsSameAddressAsPatient).ThenBy(x => x.ContactName)
                    : query.OrderBy(x => x.IsSameAddressAsPatient).ThenBy(x => x.ContactName),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.ContactName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.ContactName),

                _ => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime).ThenByDescending(x => x.ContactName)
                    : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.ContactName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreatePatientEmergencyContactRequest request)
        {
            if (request.PatientId == Guid.Empty)
            {
                return (false, "Patient wajib dipilih.");
            }

            if (string.IsNullOrWhiteSpace(request.ContactName))
            {
                return (false, "Nama kontak wajib diisi.");
            }

            var patientExists = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == request.PatientId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!patientExists)
            {
                return (false, "Patient tidak valid atau tidak aktif.");
            }

            if (!string.IsNullOrWhiteSpace(request.Email) &&
                !request.Email.Contains('@'))
            {
                return (false, "Format email kontak tidak valid.");
            }

            var normalizedContactName = request.ContactName.Trim().ToLower();
            var normalizedPhoneNumber = NormalizeNullableString(request.PhoneNumber)?.ToLower();
            var normalizedWhatsAppNumber = NormalizeNullableString(request.WhatsAppNumber)?.ToLower();
            var normalizedIdentityType = NormalizeNullableString(request.IdentityType)?.ToLower();
            var normalizedIdentityNumber = NormalizeNullableString(request.IdentityNumber)?.ToLower();

            var duplicateNameQuery = _dbContext.Set<MstPatientEmergencyContact>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.PatientId == request.PatientId &&
                    x.ContactName.ToLower() == normalizedContactName);

            if (excludeId.HasValue)
            {
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateNameQuery.AnyAsync())
            {
                return (false, "Nama kontak darurat pada patient tersebut sudah digunakan.");
            }

            if (!string.IsNullOrWhiteSpace(normalizedPhoneNumber))
            {
                var duplicatePhoneQuery = _dbContext.Set<MstPatientEmergencyContact>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.PatientId == request.PatientId &&
                        x.PhoneNumber != null &&
                        x.PhoneNumber.ToLower() == normalizedPhoneNumber);

                if (excludeId.HasValue)
                {
                    duplicatePhoneQuery = duplicatePhoneQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicatePhoneQuery.AnyAsync())
                {
                    return (false, "Nomor telepon kontak darurat pada patient tersebut sudah digunakan.");
                }
            }

            if (!string.IsNullOrWhiteSpace(normalizedWhatsAppNumber))
            {
                var duplicateWhatsAppQuery = _dbContext.Set<MstPatientEmergencyContact>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.PatientId == request.PatientId &&
                        x.WhatsAppNumber != null &&
                        x.WhatsAppNumber.ToLower() == normalizedWhatsAppNumber);

                if (excludeId.HasValue)
                {
                    duplicateWhatsAppQuery = duplicateWhatsAppQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicateWhatsAppQuery.AnyAsync())
                {
                    return (false, "Nomor WhatsApp kontak darurat pada patient tersebut sudah digunakan.");
                }
            }

            if (!string.IsNullOrWhiteSpace(normalizedIdentityType) &&
                !string.IsNullOrWhiteSpace(normalizedIdentityNumber))
            {
                var duplicateIdentityQuery = _dbContext.Set<MstPatientEmergencyContact>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.PatientId == request.PatientId &&
                        x.IdentityType != null &&
                        x.IdentityNumber != null &&
                        x.IdentityType.ToLower() == normalizedIdentityType &&
                        x.IdentityNumber.ToLower() == normalizedIdentityNumber);

                if (excludeId.HasValue)
                {
                    duplicateIdentityQuery = duplicateIdentityQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicateIdentityQuery.AnyAsync())
                {
                    return (false, "Identitas kontak darurat pada patient tersebut sudah digunakan.");
                }
            }

            return (true, null);
        }

        private async Task UnsetOtherPrimaryContactAsync(
            Guid patientId,
            Guid? exceptId,
            DateTime now,
            Guid actorUserId)
        {
            var query = _dbContext.Set<MstPatientEmergencyContact>()
                .Where(x =>
                    x.PatientId == patientId &&
                    x.IsPrimary &&
                    !x.IsDelete);

            if (exceptId.HasValue)
            {
                query = query.Where(x => x.Id != exceptId.Value);
            }

            var entities = await query.ToListAsync();

            foreach (var entity in entities)
            {
                entity.IsPrimary = false;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;
            }
        }

        private async Task UnsetOtherResponsiblePersonAsync(
            Guid patientId,
            Guid? exceptId,
            DateTime now,
            Guid actorUserId)
        {
            var query = _dbContext.Set<MstPatientEmergencyContact>()
                .Where(x =>
                    x.PatientId == patientId &&
                    x.IsResponsiblePerson &&
                    !x.IsDelete);

            if (exceptId.HasValue)
            {
                query = query.Where(x => x.Id != exceptId.Value);
            }

            var entities = await query.ToListAsync();

            foreach (var entity in entities)
            {
                entity.IsResponsiblePerson = false;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;
            }
        }

        private async Task<PatientEmergencyContactCreateResponse> BuildCreateResponseAsync(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstAsync(x => x.Id == id);

            return new PatientEmergencyContactCreateResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientFullName = entity.Patient?.FullName ?? string.Empty,
                ContactName = entity.ContactName,
                Relationship = entity.Relationship,
                IsPrimary = entity.IsPrimary,
                IsResponsiblePerson = entity.IsResponsiblePerson,
                IsActive = entity.IsActive
            };
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

        private static PatientEmergencyContactResponse MapResponse(
            MstPatientEmergencyContact entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new PatientEmergencyContactResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientCode = entity.Patient?.PatientCode ?? string.Empty,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber ?? string.Empty,
                PatientFullName = entity.Patient?.FullName ?? string.Empty,
                ContactName = entity.ContactName,
                Relationship = entity.Relationship,
                IdentityType = entity.IdentityType,
                IdentityNumber = entity.IdentityNumber,
                PhoneNumber = entity.PhoneNumber,
                WhatsAppNumber = entity.WhatsAppNumber,
                Email = entity.Email,
                Address = entity.Address,
                IsPrimary = entity.IsPrimary,
                IsResponsiblePerson = entity.IsResponsiblePerson,
                IsSameAddressAsPatient = entity.IsSameAddressAsPatient,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static PatientEmergencyContactDetailResponse MapDetailResponse(
            MstPatientEmergencyContact entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = new PatientEmergencyContactDetailResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientCode = entity.Patient?.PatientCode ?? string.Empty,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber ?? string.Empty,
                PatientFullName = entity.Patient?.FullName ?? string.Empty,
                ContactName = entity.ContactName,
                Relationship = entity.Relationship,
                IdentityType = entity.IdentityType,
                IdentityNumber = entity.IdentityNumber,
                PhoneNumber = entity.PhoneNumber,
                WhatsAppNumber = entity.WhatsAppNumber,
                Email = entity.Email,
                Address = entity.Address,
                IsPrimary = entity.IsPrimary,
                IsResponsiblePerson = entity.IsResponsiblePerson,
                IsSameAddressAsPatient = entity.IsSameAddressAsPatient,
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

        private static PatientEmergencyContactOptionResponse MapOptionResponse(
            MstPatientEmergencyContact entity)
        {
            return new PatientEmergencyContactOptionResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientCode = entity.Patient?.PatientCode ?? string.Empty,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber ?? string.Empty,
                PatientFullName = entity.Patient?.FullName ?? string.Empty,
                ContactName = entity.ContactName,
                Relationship = entity.Relationship,
                PhoneNumber = entity.PhoneNumber,
                WhatsAppNumber = entity.WhatsAppNumber,
                IsPrimary = entity.IsPrimary,
                IsResponsiblePerson = entity.IsResponsiblePerson
            };
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

        private static void NormalizeActorInfo(PatientEmergencyContactResponse data)
        {
            if (data.UpdateDateTime.HasValue &&
                data.UpdateDateTime.Value == DateTime.MinValue)
            {
                data.UpdateDateTime = null;
            }

            if (!data.CreateBy.HasValue || data.CreateBy.Value == Guid.Empty)
            {
                data.CreateBy = null;
                data.CreateByName = null;
            }

            if (!data.UpdateBy.HasValue || data.UpdateBy.Value == Guid.Empty)
            {
                data.UpdateBy = null;
                data.UpdateByName = null;
            }
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
