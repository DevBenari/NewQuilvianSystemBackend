using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponsePatientClassPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.PatientClassResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/patient-classes")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Patient Class",
        AreaName = "HealthServices",
        ControllerName = "PatientClass",
        Description = "Health service master data patient class",
        SortOrder = 3
    )]
    [Tags("Health Services / Master Data / Patient Class")]
    public class PatientClassController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PatientClassController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientClassFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Class", Description = "Melihat data patient class", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientClass", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientClassFilterMetadataResponse
            {
                DefaultFilter = new PatientClassDefaultFilterResponse(),
                SortOptions = new List<PatientClassSortOptionResponse>
                {
                    new() { Value = "classLevel", Label = "Level kelas" },
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "patientClassCode", Label = "Kode kelas pasien" },
                    new() { Value = "patientClassName", Label = "Nama kelas pasien" },
                    new() { Value = "patientClassType", Label = "Tipe kelas pasien" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                PatientClassTypeOptions = BuildEnumOptions<PatientClassType>()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientClass.GetFilterMetadata",
                "Mengambil metadata filter patient class.",
                result
            );

            return Ok(ApiResponse<PatientClassFilterMetadataResponse>.Ok(
                result,
                "Metadata filter patient class berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<PatientClassSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Class", Description = "Melihat data patient class", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientClass", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstPatientClass>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new PatientClassSummaryResponse
            {
                TotalPatientClass = await query.CountAsync(),
                ActivePatientClass = await query.CountAsync(x => x.IsActive),
                InactivePatientClass = await query.CountAsync(x => !x.IsActive),
                OutpatientClass = await query.CountAsync(x => x.IsForOutpatient),
                InpatientClass = await query.CountAsync(x => x.IsForInpatient),
                EmergencyClass = await query.CountAsync(x => x.IsForEmergency),
                RoomChargeClass = await query.CountAsync(x => x.IsForRoomCharge),
                DefaultClass = await query.CountAsync(x => x.IsDefault)
            };

            return Ok(ApiResponse<PatientClassSummaryResponse>.Ok(
                result,
                "Ringkasan patient class berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientClassPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Class", Description = "Melihat data patient class", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientClass", "Read")]
        public async Task<IActionResult> GetPatientClasses(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] PatientClassType? patientClassType,
            [FromQuery] bool? isForOutpatient,
            [FromQuery] bool? isForInpatient,
            [FromQuery] bool? isForEmergency,
            [FromQuery] bool? isForRoomCharge,
            [FromQuery] bool? isDefault,
            [FromQuery] string? sortBy = "classLevel",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstPatientClass>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.PatientClassCode.ToLower().Contains(keyword) ||
                    x.PatientClassName.ToLower().Contains(keyword) ||
                    (x.ExternalClassCode != null && x.ExternalClassCode.ToLower().Contains(keyword)) ||
                    (x.ClassAlias != null && x.ClassAlias.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (patientClassType.HasValue)
                query = query.Where(x => x.PatientClassType == patientClassType.Value);

            if (isForOutpatient.HasValue)
                query = query.Where(x => x.IsForOutpatient == isForOutpatient.Value);

            if (isForInpatient.HasValue)
                query = query.Where(x => x.IsForInpatient == isForInpatient.Value);

            if (isForEmergency.HasValue)
                query = query.Where(x => x.IsForEmergency == isForEmergency.Value);

            if (isForRoomCharge.HasValue)
                query = query.Where(x => x.IsForRoomCharge == isForRoomCharge.Value);

            if (isDefault.HasValue)
                query = query.Where(x => x.IsDefault == isDefault.Value);

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PatientClassResponse
                {
                    Id = x.Id,
                    PatientClassCode = x.PatientClassCode,
                    PatientClassName = x.PatientClassName,
                    PatientClassType = x.PatientClassType,
                    ExternalClassCode = x.ExternalClassCode,
                    ClassAlias = x.ClassAlias,
                    ClassLevel = x.ClassLevel,
                    IsForOutpatient = x.IsForOutpatient,
                    IsForInpatient = x.IsForInpatient,
                    IsForEmergency = x.IsForEmergency,
                    IsForIntensiveCare = x.IsForIntensiveCare,
                    IsForNewborn = x.IsForNewborn,
                    IsForRoomCharge = x.IsForRoomCharge,
                    IsForTariffMapping = x.IsForTariffMapping,
                    IsDefault = x.IsDefault,
                    DefaultDailyRoomRate = x.DefaultDailyRoomRate,
                    DefaultRegistrationFee = x.DefaultRegistrationFee,
                    DefaultConsultationFee = x.DefaultConsultationFee,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new ResponsePatientClassPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePatientClassPagedResult>.Ok(
                result,
                "Data patient class berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientClassOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Class", Description = "Melihat data patient class", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientClass", "Read")]
        public async Task<IActionResult> GetPatientClassOptions(
            [FromQuery] PatientClassType? patientClassType,
            [FromQuery] bool? isForOutpatient,
            [FromQuery] bool? isForInpatient,
            [FromQuery] bool? isForEmergency,
            [FromQuery] bool? isForRoomCharge,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<MstPatientClass>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (patientClassType.HasValue)
                query = query.Where(x => x.PatientClassType == patientClassType.Value);

            if (isForOutpatient.HasValue)
                query = query.Where(x => x.IsForOutpatient == isForOutpatient.Value);

            if (isForInpatient.HasValue)
                query = query.Where(x => x.IsForInpatient == isForInpatient.Value);

            if (isForEmergency.HasValue)
                query = query.Where(x => x.IsForEmergency == isForEmergency.Value);

            if (isForRoomCharge.HasValue)
                query = query.Where(x => x.IsForRoomCharge == isForRoomCharge.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.PatientClassCode.ToLower().Contains(keyword) ||
                    x.PatientClassName.ToLower().Contains(keyword) ||
                    (x.ClassAlias != null && x.ClassAlias.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderBy(x => x.ClassLevel)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.PatientClassName)
                .Select(x => new PatientClassOptionResponse
                {
                    Id = x.Id,
                    PatientClassCode = x.PatientClassCode,
                    PatientClassName = x.PatientClassName,
                    PatientClassType = x.PatientClassType,
                    ClassAlias = x.ClassAlias,
                    ClassLevel = x.ClassLevel,
                    IsForOutpatient = x.IsForOutpatient,
                    IsForInpatient = x.IsForInpatient,
                    IsForEmergency = x.IsForEmergency,
                    IsForRoomCharge = x.IsForRoomCharge,
                    IsDefault = x.IsDefault
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientClassOptionResponse>>.Ok(
                data,
                "Data pilihan patient class berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientClassDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Class", Description = "Melihat data patient class", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientClass", "Read")]
        public async Task<IActionResult> GetPatientClassById(Guid id)
        {
            var data = await _dbContext.Set<MstPatientClass>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new PatientClassDetailResponse
                {
                    Id = x.Id,
                    PatientClassCode = x.PatientClassCode,
                    PatientClassName = x.PatientClassName,
                    PatientClassType = x.PatientClassType,
                    ExternalClassCode = x.ExternalClassCode,
                    ClassAlias = x.ClassAlias,
                    ClassLevel = x.ClassLevel,
                    IsForOutpatient = x.IsForOutpatient,
                    IsForInpatient = x.IsForInpatient,
                    IsForEmergency = x.IsForEmergency,
                    IsForIntensiveCare = x.IsForIntensiveCare,
                    IsForNewborn = x.IsForNewborn,
                    IsForRoomCharge = x.IsForRoomCharge,
                    IsForTariffMapping = x.IsForTariffMapping,
                    IsDefault = x.IsDefault,
                    DefaultDailyRoomRate = x.DefaultDailyRoomRate,
                    DefaultRegistrationFee = x.DefaultRegistrationFee,
                    DefaultConsultationFee = x.DefaultConsultationFee,
                    SortOrder = x.SortOrder,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient class tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientClassDetailResponse>.Ok(
                data,
                "Detail patient class berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientClassCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Patient Class", Description = "Membuat data patient class", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PatientClass", "Create")]
        public async Task<IActionResult> CreatePatientClass([FromBody] CreatePatientClassRequest request)
        {
            var validation = await ValidateRequestAsync(null, request.PatientClassCode, request.PatientClassName);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data patient class tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (request.IsDefault)
            {
                await ResetDefaultPatientClassAsync(actorUserId, now);
            }

            var entity = new MstPatientClass
            {
                Id = Guid.NewGuid(),
                PatientClassCode = request.PatientClassCode.Trim().ToUpperInvariant(),
                PatientClassName = request.PatientClassName.Trim(),
                PatientClassType = request.PatientClassType,
                ExternalClassCode = NormalizeNullableText(request.ExternalClassCode),
                ClassAlias = NormalizeNullableText(request.ClassAlias),
                ClassLevel = request.ClassLevel,
                IsForOutpatient = request.IsForOutpatient,
                IsForInpatient = request.IsForInpatient,
                IsForEmergency = request.IsForEmergency,
                IsForIntensiveCare = request.IsForIntensiveCare,
                IsForNewborn = request.IsForNewborn,
                IsForRoomCharge = request.IsForRoomCharge,
                IsForTariffMapping = request.IsForTariffMapping,
                IsDefault = request.IsDefault,
                DefaultDailyRoomRate = request.DefaultDailyRoomRate,
                DefaultRegistrationFee = request.DefaultRegistrationFee,
                DefaultConsultationFee = request.DefaultConsultationFee,
                SortOrder = request.SortOrder,
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstPatientClass>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = new PatientClassCreateResponse
            {
                Id = entity.Id,
                PatientClassCode = entity.PatientClassCode,
                PatientClassName = entity.PatientClassName,
                PatientClassType = entity.PatientClassType,
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<PatientClassCreateResponse>.Ok(
                response,
                "Patient class berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Class", Description = "Mengubah data patient class", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientClass", "Update")]
        public async Task<IActionResult> UpdatePatientClass(Guid id, [FromBody] UpdatePatientClassRequest request)
        {
            var entity = await _dbContext.Set<MstPatientClass>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient class tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(id, request.PatientClassCode, request.PatientClassName);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data patient class tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (request.IsDefault && !entity.IsDefault)
            {
                await ResetDefaultPatientClassAsync(actorUserId, now);
            }

            entity.PatientClassCode = request.PatientClassCode.Trim().ToUpperInvariant();
            entity.PatientClassName = request.PatientClassName.Trim();
            entity.PatientClassType = request.PatientClassType;
            entity.ExternalClassCode = NormalizeNullableText(request.ExternalClassCode);
            entity.ClassAlias = NormalizeNullableText(request.ClassAlias);
            entity.ClassLevel = request.ClassLevel;
            entity.IsForOutpatient = request.IsForOutpatient;
            entity.IsForInpatient = request.IsForInpatient;
            entity.IsForEmergency = request.IsForEmergency;
            entity.IsForIntensiveCare = request.IsForIntensiveCare;
            entity.IsForNewborn = request.IsForNewborn;
            entity.IsForRoomCharge = request.IsForRoomCharge;
            entity.IsForTariffMapping = request.IsForTariffMapping;
            entity.IsDefault = request.IsDefault;
            entity.DefaultDailyRoomRate = request.DefaultDailyRoomRate;
            entity.DefaultRegistrationFee = request.DefaultRegistrationFee;
            entity.DefaultConsultationFee = request.DefaultConsultationFee;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient class berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Patient Class", Description = "Menghapus data patient class", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("PatientClass", "Delete")]
        public async Task<IActionResult> DeletePatientClass(Guid id)
        {
            var entity = await _dbContext.Set<MstPatientClass>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient class tidak ditemukan."
                ));
            }

            var isUsedByRoom = await _dbContext.Set<MstRoom>()
                .AnyAsync(x => x.PatientClassId == id && !x.IsDelete);

            var isUsedByTariff = await _dbContext.Set<MstTariff>()
                .AnyAsync(x => x.PatientClassId == id && !x.IsDelete);

            if (isUsedByRoom || isUsedByTariff)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Patient class tidak dapat dihapus karena sudah digunakan oleh room atau tariff."
                ));
            }

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient class berhasil dihapus."
            ));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string patientClassCode,
            string patientClassName)
        {
            if (string.IsNullOrWhiteSpace(patientClassCode))
                return (false, "Kode patient class wajib diisi.");

            if (string.IsNullOrWhiteSpace(patientClassName))
                return (false, "Nama patient class wajib diisi.");

            var normalizedCode = patientClassCode.Trim().ToUpperInvariant();
            var normalizedName = patientClassName.Trim().ToLower();

            var duplicateCode = await _dbContext.Set<MstPatientClass>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.PatientClassCode.ToUpper() == normalizedCode &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateCode)
                return (false, "Kode patient class sudah digunakan.");

            var duplicateName = await _dbContext.Set<MstPatientClass>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.PatientClassName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateName)
                return (false, "Nama patient class sudah digunakan.");

            return (true, null);
        }

        private async Task ResetDefaultPatientClassAsync(Guid actorUserId, DateTime now)
        {
            var defaultClasses = await _dbContext.Set<MstPatientClass>()
                .Where(x => x.IsDefault && !x.IsDelete)
                .ToListAsync();

            foreach (var item in defaultClasses)
            {
                item.IsDefault = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
        }

        private static IQueryable<MstPatientClass> ApplySorting(
            IQueryable<MstPatientClass> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "classLevel").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "patientclasscode" => isDesc ? query.OrderByDescending(x => x.PatientClassCode) : query.OrderBy(x => x.PatientClassCode),
                "patientclassname" => isDesc ? query.OrderByDescending(x => x.PatientClassName) : query.OrderBy(x => x.PatientClassName),
                "patientclasstype" => isDesc ? query.OrderByDescending(x => x.PatientClassType) : query.OrderBy(x => x.PatientClassType),
                "sortorder" => isDesc ? query.OrderByDescending(x => x.SortOrder) : query.OrderBy(x => x.SortOrder),
                "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => isDesc
                    ? query.OrderByDescending(x => x.ClassLevel).ThenByDescending(x => x.SortOrder)
                    : query.OrderBy(x => x.ClassLevel).ThenBy(x => x.SortOrder)
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<PatientClassEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new PatientClassEnumOptionResponse
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
    }
}