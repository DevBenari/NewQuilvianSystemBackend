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
        private const string PatientClassCodePrefix = "PC-RSMMC-";
        private const int PatientClassCodeDigitLength = 5;

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
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<PatientClassSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "classLevel", Label = "Level kelas" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "patientClassCode", Label = "Kode kelas pasien" },
                    new() { Value = "patientClassName", Label = "Nama kelas pasien" },
                    new() { Value = "patientClassType", Label = "Tipe kelas pasien" },
                    new() { Value = "isDefault", Label = "Default" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                PatientClassTypeOptions = BuildEnumOptions<PatientClassType>(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata()
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
                IntensiveCareClass = await query.CountAsync(x => x.IsForIntensiveCare),
                NewbornClass = await query.CountAsync(x => x.IsForNewborn),
                RoomChargeClass = await query.CountAsync(x => x.IsForRoomCharge),
                TariffMappingClass = await query.CountAsync(x => x.IsForTariffMapping),
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
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);

            if (!dateRange.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    dateRange.ErrorMessage ?? "Filter tanggal tidak valid."
                ));
            }

            var query = _dbContext.Set<MstPatientClass>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (dateRange.Start.HasValue)
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);

            if (dateRange.EndExclusive.HasValue)
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);

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
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<MstPatientClass>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.PatientClassCode.ToLower().Contains(keyword) ||
                    x.PatientClassName.ToLower().Contains(keyword) ||
                    (x.ClassAlias != null && x.ClassAlias.ToLower().Contains(keyword)) ||
                    (x.ExternalClassCode != null && x.ExternalClassCode.ToLower().Contains(keyword)));
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
                    IsForIntensiveCare = x.IsForIntensiveCare,
                    IsForNewborn = x.IsForNewborn,
                    IsForRoomCharge = x.IsForRoomCharge,
                    IsForTariffMapping = x.IsForTariffMapping,
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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Patient Class", Description = "Membuat data patient class", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PatientClass", "Create")]
        public async Task<IActionResult> CreatePatientClass([FromBody] CreatePatientClassRequest request)
        {
            var validation = await ValidateCreateRequestAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data patient class tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            if (request.IsDefault)
            {
                await ResetDefaultPatientClassAsync(actorUserId, now);
            }

            var entity = new MstPatientClass
            {
                Id = Guid.NewGuid(),
                PatientClassCode = await GeneratePatientClassCodeAsync(),
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
            await transaction.CommitAsync();

            var response = ToCreateUpdateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientClass.CreatePatientClass",
                "Membuat data patient class.",
                response
            );

            return Ok(ApiResponse<PatientClassCreateResponse>.Ok(
                response,
                "Patient class berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientClassUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
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

            var validation = await ValidateUpdateRequestAsync(id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data patient class tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            if (request.IsDefault && !entity.IsDefault)
            {
                await ResetDefaultPatientClassAsync(actorUserId, now);
            }

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
            await transaction.CommitAsync();

            var response = ToUpdateResponse(entity);

            return Ok(ApiResponse<PatientClassUpdateResponse>.Ok(
                response,
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

            if (entity.IsDefault)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Patient class default tidak dapat dihapus."
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

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(CreatePatientClassRequest request)
        {
            return await ValidateCommonRequestAsync(
                excludeId: null,
                patientClassName: request.PatientClassName,
                classLevel: request.ClassLevel,
                isDefault: request.IsDefault,
                isActive: true,
                defaultDailyRoomRate: request.DefaultDailyRoomRate,
                defaultRegistrationFee: request.DefaultRegistrationFee,
                defaultConsultationFee: request.DefaultConsultationFee
            );
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateUpdateRequestAsync(Guid id, UpdatePatientClassRequest request)
        {
            return await ValidateCommonRequestAsync(
                excludeId: id,
                patientClassName: request.PatientClassName,
                classLevel: request.ClassLevel,
                isDefault: request.IsDefault,
                isActive: request.IsActive,
                defaultDailyRoomRate: request.DefaultDailyRoomRate,
                defaultRegistrationFee: request.DefaultRegistrationFee,
                defaultConsultationFee: request.DefaultConsultationFee
            );
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCommonRequestAsync(
            Guid? excludeId,
            string patientClassName,
            int classLevel,
            bool isDefault,
            bool isActive,
            decimal? defaultDailyRoomRate,
            decimal? defaultRegistrationFee,
            decimal? defaultConsultationFee)
        {
            if (string.IsNullOrWhiteSpace(patientClassName))
                return (false, "Nama patient class wajib diisi.");

            if (patientClassName.Trim().Length > 150)
                return (false, "Nama patient class maksimal 150 karakter.");

            if (classLevel < 0)
                return (false, "Level kelas tidak boleh kurang dari 0.");

            if (isDefault && !isActive)
                return (false, "Patient class default harus dalam status aktif.");

            if (defaultDailyRoomRate.HasValue && defaultDailyRoomRate.Value < 0)
                return (false, "Default daily room rate tidak boleh kurang dari 0.");

            if (defaultRegistrationFee.HasValue && defaultRegistrationFee.Value < 0)
                return (false, "Default registration fee tidak boleh kurang dari 0.");

            if (defaultConsultationFee.HasValue && defaultConsultationFee.Value < 0)
                return (false, "Default consultation fee tidak boleh kurang dari 0.");

            var normalizedName = patientClassName.Trim().ToLower();

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

        private async Task<string> GeneratePatientClassCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstPatientClass>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.PatientClassCode.StartsWith(PatientClassCodePrefix))
                .Select(x => x.PatientClassCode)
                .ToListAsync();

            var usedNumbers = new HashSet<int>();

            foreach (var code in existingCodes)
            {
                if (code.Length <= PatientClassCodePrefix.Length)
                    continue;

                var numberText = code[PatientClassCodePrefix.Length..];

                if (int.TryParse(numberText, out var number) && number > 0)
                    usedNumbers.Add(number);
            }

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            return PatientClassCodePrefix + nextNumber.ToString($"D{PatientClassCodeDigitLength}");
        }

        private static IQueryable<MstPatientClass> ApplySorting(
            IQueryable<MstPatientClass> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "patientclasscode" => isDesc
                    ? query.OrderByDescending(x => x.PatientClassCode)
                    : query.OrderBy(x => x.PatientClassCode),

                "patientclassname" => isDesc
                    ? query.OrderByDescending(x => x.PatientClassName)
                    : query.OrderBy(x => x.PatientClassName),

                "patientclasstype" => isDesc
                    ? query.OrderByDescending(x => x.PatientClassType)
                    : query.OrderBy(x => x.PatientClassType),

                "classlevel" => isDesc
                    ? query.OrderByDescending(x => x.ClassLevel)
                    : query.OrderBy(x => x.ClassLevel),

                "isdefault" => isDesc
                    ? query.OrderByDescending(x => x.IsDefault)
                    : query.OrderBy(x => x.IsDefault),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.PatientClassName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.PatientClassName)
            };
        }

        private static PatientClassCreateResponse ToCreateUpdateResponse(MstPatientClass entity)
        {
            return new PatientClassCreateResponse
            {
                Id = entity.Id,
                PatientClassCode = entity.PatientClassCode,
                PatientClassName = entity.PatientClassName,
                PatientClassType = entity.PatientClassType,
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive
            };
        }

        private static PatientClassUpdateResponse ToUpdateResponse(MstPatientClass entity)
        {
            return new PatientClassUpdateResponse
            {
                Id = entity.Id,
                PatientClassCode = entity.PatientClassCode,
                PatientClassName = entity.PatientClassName,
                PatientClassType = entity.PatientClassType,
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive
            };
        }

        private static DateRangeResult ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var today = DateTime.UtcNow.Date;
            var period = customPeriod?.Trim().ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(period) && period != "custom")
            {
                return period switch
                {
                    "today" => DateRangeResult.Valid(today, today.AddDays(1)),
                    "yesterday" => DateRangeResult.Valid(today.AddDays(-1), today),
                    "last7days" => DateRangeResult.Valid(today.AddDays(-6), today.AddDays(1)),
                    "last30days" => DateRangeResult.Valid(today.AddDays(-29), today.AddDays(1)),
                    "thismonth" => DateRangeResult.Valid(new DateTime(today.Year, today.Month, 1), new DateTime(today.Year, today.Month, 1).AddMonths(1)),
                    "lastmonth" => DateRangeResult.Valid(new DateTime(today.Year, today.Month, 1).AddMonths(-1), new DateTime(today.Year, today.Month, 1)),
                    "thisyear" => DateRangeResult.Valid(new DateTime(today.Year, 1, 1), new DateTime(today.Year + 1, 1, 1)),
                    _ => DateRangeResult.Invalid("Custom period tidak dikenali.")
                };
            }

            var start = startDate?.Date;
            var endExclusive = endDate?.Date.AddDays(1);

            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value)
                return DateRangeResult.Invalid("StartDate tidak boleh lebih besar dari EndDate.");

            return DateRangeResult.Valid(start, endExclusive);
        }

        private static List<PatientClassCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<PatientClassCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "yesterday", Label = "Kemarin", Description = "Data yang dibuat kemarin.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thisMonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastMonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thisYear", Label = "Tahun ini", Description = "Data yang dibuat pada tahun berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true }
            };
        }

        private static List<PatientClassQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<PatientClassQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "date", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "date", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Preset periode. Jika bukan custom, startDate dan endDate diabaikan.", Example = "thisMonth" },
                new() { Name = "isActive", Type = "boolean", Description = "Filter status aktif." },
                new() { Name = "search", Type = "string", Description = "Pencarian kode, nama, alias, kode eksternal, atau deskripsi." },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting." },
                new() { Name = "sortDirection", Type = "string", Description = "asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "integer", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "integer", Description = "Jumlah data per halaman.", Example = "25" }
            };
        }

        private static List<PatientClassFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return new List<PatientClassFormFieldMetadataResponse>
            {
                new() { Name = "patientClassCode", Label = "Kode kelas pasien", DataType = "string", InputType = "text", Required = false, IsReadonly = true, Placeholder = "Auto generated", Description = "Dibuat otomatis oleh sistem dengan format PC-RSMMC-00001." },
                new() { Name = "patientClassName", Label = "Nama kelas pasien", DataType = "string", InputType = "text", Required = true, IsReadonly = false },
                new() { Name = "patientClassType", Label = "Tipe kelas pasien", DataType = "enum", InputType = "select", Required = true, IsReadonly = false },
                new() { Name = "externalClassCode", Label = "Kode kelas eksternal", DataType = "string", InputType = "text", Required = false, IsReadonly = false },
                new() { Name = "classAlias", Label = "Alias kelas", DataType = "string", InputType = "text", Required = false, IsReadonly = false },
                new() { Name = "classLevel", Label = "Level kelas", DataType = "integer", InputType = "number", Required = true, IsReadonly = false },
                new() { Name = "isForOutpatient", Label = "Untuk rawat jalan", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isForInpatient", Label = "Untuk rawat inap", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isForEmergency", Label = "Untuk IGD", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isForIntensiveCare", Label = "Untuk intensive care", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isForNewborn", Label = "Untuk newborn", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isForRoomCharge", Label = "Untuk biaya kamar", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isForTariffMapping", Label = "Untuk mapping tarif", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isDefault", Label = "Default", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "defaultDailyRoomRate", Label = "Default tarif kamar harian", DataType = "decimal", InputType = "number", Required = false, IsReadonly = false },
                new() { Name = "defaultRegistrationFee", Label = "Default biaya registrasi", DataType = "decimal", InputType = "number", Required = false, IsReadonly = false },
                new() { Name = "defaultConsultationFee", Label = "Default biaya konsultasi", DataType = "decimal", InputType = "number", Required = false, IsReadonly = false },
                new() { Name = "sortOrder", Label = "Urutan", DataType = "integer", InputType = "number", Required = false, IsReadonly = false },
                new() { Name = "description", Label = "Deskripsi", DataType = "string", InputType = "textarea", Required = false, IsReadonly = false }
            };
        }

        private static List<PatientClassFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            var fields = BuildCreateFieldMetadata();

            fields.Add(new PatientClassFormFieldMetadataResponse
            {
                Name = "isActive",
                Label = "Status aktif",
                DataType = "boolean",
                InputType = "switch",
                Required = false,
                IsReadonly = false
            });

            return fields;
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

        private sealed class DateRangeResult
        {
            public bool IsValid { get; private set; }
            public DateTime? Start { get; private set; }
            public DateTime? EndExclusive { get; private set; }
            public string? ErrorMessage { get; private set; }

            public static DateRangeResult Valid(DateTime? start, DateTime? endExclusive)
            {
                return new DateRangeResult
                {
                    IsValid = true,
                    Start = start,
                    EndExclusive = endExclusive
                };
            }

            public static DateRangeResult Invalid(string errorMessage)
            {
                return new DateRangeResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}
