using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
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
        [AccessAction("Read", "Read Patient Class", Description = "Melihat metadata filter patient class", AccessType = AccessTypes.Read, SortOrder = 1)]
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
                    new() { Value = "externalClassCode", Label = "Kode eksternal" },
                    new() { Value = "classAlias", Label = "Alias kelas" },
                    new() { Value = "isForOutpatient", Label = "Rawat jalan" },
                    new() { Value = "isForInpatient", Label = "Rawat inap" },
                    new() { Value = "isForEmergency", Label = "IGD" },
                    new() { Value = "isForIntensiveCare", Label = "Intensive care" },
                    new() { Value = "isForNewborn", Label = "Newborn" },
                    new() { Value = "isForRoomCharge", Label = "Biaya kamar" },
                    new() { Value = "isForTariffMapping", Label = "Mapping tarif" },
                    new() { Value = "isDefault", Label = "Default" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                PatientClassTypeOptions = BuildEnumOptions<PatientClassType>(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
                ResetButtonLabel = "Reset"
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
        [AccessAction("Read", "Read Patient Class", Description = "Melihat ringkasan patient class", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientClass", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

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
            [FromQuery] PatientClassType? patientClassType,
            [FromQuery] bool? isForOutpatient,
            [FromQuery] bool? isForInpatient,
            [FromQuery] bool? isForEmergency,
            [FromQuery] bool? isForIntensiveCare,
            [FromQuery] bool? isForNewborn,
            [FromQuery] bool? isForRoomCharge,
            [FromQuery] bool? isForTariffMapping,
            [FromQuery] bool? isDefault,
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

            var query = BuildBaseQuery();
            query = ApplyDateFilter(query, dateRange);
            query = ApplyStandardFilter(
                query,
                search,
                isActive,
                patientClassType,
                isForOutpatient,
                isForInpatient,
                isForEmergency,
                isForIntensiveCare,
                isForNewborn,
                isForRoomCharge,
                isForTariffMapping,
                isDefault
            );

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities.Select(x => x.CreateBy).Where(x => x != Guid.Empty)
            );

            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

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
        [ProducesResponseType(typeof(ApiResponse<PatientClassOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Class", Description = "Melihat data pilihan patient class", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientClass", "Read")]
        public async Task<IActionResult> GetPatientClassOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool? activeOnly = null,
            [FromQuery] PatientClassType? patientClassType = null,
            [FromQuery] bool? isForOutpatient = null,
            [FromQuery] bool? isForInpatient = null,
            [FromQuery] bool? isForEmergency = null,
            [FromQuery] bool? isForIntensiveCare = null,
            [FromQuery] bool? isForNewborn = null,
            [FromQuery] bool? isForRoomCharge = null,
            [FromQuery] bool? isForTariffMapping = null,
            [FromQuery] bool? isDefault = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();
            query = ApplyStandardFilter(
                query,
                search,
                activeOnly ?? onlyActive,
                patientClassType,
                isForOutpatient,
                isForInpatient,
                isForEmergency,
                isForIntensiveCare,
                isForNewborn,
                isForRoomCharge,
                isForTariffMapping,
                isDefault
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.ClassLevel)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.PatientClassName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(MapOptionResponse).ToList();

            var result = new PatientClassOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PatientClassOptionPagedResponse>.Ok(
                result,
                "Data pilihan patient class berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientClassDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Class", Description = "Melihat detail patient class", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientClass", "Read")]
        public async Task<IActionResult> GetPatientClassById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient class tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var result = MapDetailResponse(entity, actorNames);

            return Ok(ApiResponse<PatientClassDetailResponse>.Ok(
                result,
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
            var validation = await ValidateCreateUpdateRequestAsync(null, request, isActive: true);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data patient class tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

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

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = MapCreateResponse(entity, actorNames);

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientClass.CreatePatientClass",
                "Membuat data patient class.",
                result
            );

            return Ok(ApiResponse<PatientClassCreateResponse>.Ok(
                result,
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

            var validation = await ValidateCreateUpdateRequestAsync(id, request, request.IsActive);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data patient class tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            if (request.IsDefault)
            {
                await ResetDefaultPatientClassAsync(actorUserId, now, excludeId: entity.Id);
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
            entity.IsDefault = request.IsActive && request.IsDefault;
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

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = MapUpdateResponse(entity, actorNames);

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientClass.UpdatePatientClass",
                "Mengubah data patient class.",
                result
            );

            return Ok(ApiResponse<PatientClassUpdateResponse>.Ok(
                result,
                "Patient class berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<PatientClassUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Class Status", Description = "Mengubah status patient class", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientClass", "Update")]
        public async Task<IActionResult> UpdatePatientClassStatus(Guid id, [FromBody] UpdatePatientClassStatusRequest request)
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

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            if (!request.IsActive)
            {
                entity.IsDefault = false;
            }

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = MapUpdateResponse(entity, actorNames);

            return Ok(ApiResponse<PatientClassUpdateResponse>.Ok(
                result,
                "Status patient class berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientClassDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Patient Class", Description = "Menghapus data patient class", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("PatientClass", "Delete")]
        public async Task<IActionResult> DeletePatientClass(Guid id, [FromBody] DeletePatientClassRequest? request = null)
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

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsDefault = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (!string.IsNullOrWhiteSpace(request?.DeleteReason))
            {
                entity.Description = request.DeleteReason.Trim();
            }

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = new PatientClassDeleteResponse
            {
                Id = entity.Id,
                PatientClassCode = entity.PatientClassCode,
                PatientClassName = entity.PatientClassName,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : (Guid?)entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientClass.DeletePatientClass",
                "Menghapus data patient class.",
                result
            );

            return Ok(ApiResponse<PatientClassDeleteResponse>.Ok(
                result,
                "Patient class berhasil dihapus."
            ));
        }

        private IQueryable<MstPatientClass> BuildBaseQuery()
        {
            return _dbContext.Set<MstPatientClass>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstPatientClass> ApplyDateFilter(
            IQueryable<MstPatientClass> query,
            DateRangeResolveResult dateRange)
        {
            if (dateRange.Start.HasValue)
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);

            if (dateRange.EndExclusive.HasValue)
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);

            return query;
        }

        private static IQueryable<MstPatientClass> ApplyStandardFilter(
            IQueryable<MstPatientClass> query,
            string? search,
            bool? isActive,
            PatientClassType? patientClassType,
            bool? isForOutpatient,
            bool? isForInpatient,
            bool? isForEmergency,
            bool? isForIntensiveCare,
            bool? isForNewborn,
            bool? isForRoomCharge,
            bool? isForTariffMapping,
            bool? isDefault)
        {
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

            if (isForIntensiveCare.HasValue)
                query = query.Where(x => x.IsForIntensiveCare == isForIntensiveCare.Value);

            if (isForNewborn.HasValue)
                query = query.Where(x => x.IsForNewborn == isForNewborn.Value);

            if (isForRoomCharge.HasValue)
                query = query.Where(x => x.IsForRoomCharge == isForRoomCharge.Value);

            if (isForTariffMapping.HasValue)
                query = query.Where(x => x.IsForTariffMapping == isForTariffMapping.Value);

            if (isDefault.HasValue)
                query = query.Where(x => x.IsDefault == isDefault.Value);

            return query;
        }

        private static IOrderedQueryable<MstPatientClass> ApplySorting(
            IQueryable<MstPatientClass> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "patientclasscode" => isDesc ? query.OrderByDescending(x => x.PatientClassCode) : query.OrderBy(x => x.PatientClassCode),
                "patientclassname" => isDesc ? query.OrderByDescending(x => x.PatientClassName) : query.OrderBy(x => x.PatientClassName),
                "patientclasstype" => isDesc ? query.OrderByDescending(x => x.PatientClassType).ThenBy(x => x.PatientClassName) : query.OrderBy(x => x.PatientClassType).ThenBy(x => x.PatientClassName),
                "externalclasscode" => isDesc ? query.OrderByDescending(x => x.ExternalClassCode).ThenBy(x => x.PatientClassName) : query.OrderBy(x => x.ExternalClassCode).ThenBy(x => x.PatientClassName),
                "classalias" => isDesc ? query.OrderByDescending(x => x.ClassAlias).ThenBy(x => x.PatientClassName) : query.OrderBy(x => x.ClassAlias).ThenBy(x => x.PatientClassName),
                "classlevel" => isDesc ? query.OrderByDescending(x => x.ClassLevel).ThenBy(x => x.PatientClassName) : query.OrderBy(x => x.ClassLevel).ThenBy(x => x.PatientClassName),
                "isforoutpatient" => isDesc ? query.OrderByDescending(x => x.IsForOutpatient).ThenBy(x => x.PatientClassName) : query.OrderBy(x => x.IsForOutpatient).ThenBy(x => x.PatientClassName),
                "isforinpatient" => isDesc ? query.OrderByDescending(x => x.IsForInpatient).ThenBy(x => x.PatientClassName) : query.OrderBy(x => x.IsForInpatient).ThenBy(x => x.PatientClassName),
                "isforemergency" => isDesc ? query.OrderByDescending(x => x.IsForEmergency).ThenBy(x => x.PatientClassName) : query.OrderBy(x => x.IsForEmergency).ThenBy(x => x.PatientClassName),
                "isforintensivecare" => isDesc ? query.OrderByDescending(x => x.IsForIntensiveCare).ThenBy(x => x.PatientClassName) : query.OrderBy(x => x.IsForIntensiveCare).ThenBy(x => x.PatientClassName),
                "isfornewborn" => isDesc ? query.OrderByDescending(x => x.IsForNewborn).ThenBy(x => x.PatientClassName) : query.OrderBy(x => x.IsForNewborn).ThenBy(x => x.PatientClassName),
                "isforroomcharge" => isDesc ? query.OrderByDescending(x => x.IsForRoomCharge).ThenBy(x => x.PatientClassName) : query.OrderBy(x => x.IsForRoomCharge).ThenBy(x => x.PatientClassName),
                "isfortariffmapping" => isDesc ? query.OrderByDescending(x => x.IsForTariffMapping).ThenBy(x => x.PatientClassName) : query.OrderBy(x => x.IsForTariffMapping).ThenBy(x => x.PatientClassName),
                "isdefault" => isDesc ? query.OrderByDescending(x => x.IsDefault).ThenBy(x => x.PatientClassName) : query.OrderBy(x => x.IsDefault).ThenBy(x => x.PatientClassName),
                "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.PatientClassName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.PatientClassName),
                _ => isDesc ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.PatientClassName) : query.OrderBy(x => x.SortOrder).ThenBy(x => x.PatientClassName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateUpdateRequestAsync(
            Guid? excludeId,
            CreatePatientClassRequest request,
            bool isActive)
        {
            if (string.IsNullOrWhiteSpace(request.PatientClassName))
                return (false, "Nama patient class wajib diisi.");

            if (request.ClassLevel < 0)
                return (false, "Level kelas tidak boleh kurang dari 0.");

            if (request.IsDefault && !isActive)
                return (false, "Patient class default harus dalam status aktif.");

            if (request.DefaultDailyRoomRate.HasValue && request.DefaultDailyRoomRate.Value < 0)
                return (false, "Default daily room rate tidak boleh kurang dari 0.");

            if (request.DefaultRegistrationFee.HasValue && request.DefaultRegistrationFee.Value < 0)
                return (false, "Default registration fee tidak boleh kurang dari 0.");

            if (request.DefaultConsultationFee.HasValue && request.DefaultConsultationFee.Value < 0)
                return (false, "Default consultation fee tidak boleh kurang dari 0.");

            var normalizedName = request.PatientClassName.Trim().ToLower();

            var duplicateNameQuery = _dbContext.Set<MstPatientClass>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.PatientClassName.ToLower() == normalizedName);

            if (excludeId.HasValue)
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateNameQuery.AnyAsync())
                return (false, "Nama patient class sudah digunakan.");

            var externalClassCode = NormalizeNullableText(request.ExternalClassCode);
            if (!string.IsNullOrWhiteSpace(externalClassCode))
            {
                var normalizedExternalCode = externalClassCode.ToLower();

                var duplicateExternalCodeQuery = _dbContext.Set<MstPatientClass>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.ExternalClassCode != null &&
                        x.ExternalClassCode.ToLower() == normalizedExternalCode);

                if (excludeId.HasValue)
                    duplicateExternalCodeQuery = duplicateExternalCodeQuery.Where(x => x.Id != excludeId.Value);

                if (await duplicateExternalCodeQuery.AnyAsync())
                    return (false, "External class code sudah digunakan.");
            }

            return (true, null);
        }

        private async Task ResetDefaultPatientClassAsync(Guid actorUserId, DateTime now, Guid? excludeId = null)
        {
            var defaultClasses = await _dbContext.Set<MstPatientClass>()
                .Where(x =>
                    x.IsDefault &&
                    !x.IsDelete &&
                    (!excludeId.HasValue || x.Id != excludeId.Value))
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
                .Where(x => !x.IsDelete && x.PatientClassCode.StartsWith(PatientClassCodePrefix))
                .Select(x => x.PatientClassCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(ExtractPatientClassCodeNumber)
                .Where(x => x.HasValue && x.Value > 0)
                .Select(x => x!.Value)
                .ToHashSet();

            var nextNumber = 1;
            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return PatientClassCodePrefix + nextNumber.ToString().PadLeft(PatientClassCodeDigitLength, '0');
        }

        private static int? ExtractPatientClassCodeNumber(string patientClassCode)
        {
            if (string.IsNullOrWhiteSpace(patientClassCode))
                return null;

            if (!patientClassCode.StartsWith(PatientClassCodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var numberText = patientClassCode[PatientClassCodePrefix.Length..];
            return int.TryParse(numberText, out var number) ? number : null;
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> actorIds)
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
                    Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static PatientClassResponse MapResponse(MstPatientClass entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new PatientClassResponse
            {
                Id = entity.Id,
                PatientClassCode = entity.PatientClassCode,
                PatientClassName = entity.PatientClassName,
                PatientClassType = entity.PatientClassType,
                PatientClassTypeName = BuildEnumLabel(entity.PatientClassType),
                ExternalClassCode = entity.ExternalClassCode,
                ClassAlias = entity.ClassAlias,
                ClassLevel = entity.ClassLevel,
                IsForOutpatient = entity.IsForOutpatient,
                IsForInpatient = entity.IsForInpatient,
                IsForEmergency = entity.IsForEmergency,
                IsForIntensiveCare = entity.IsForIntensiveCare,
                IsForNewborn = entity.IsForNewborn,
                IsForRoomCharge = entity.IsForRoomCharge,
                IsForTariffMapping = entity.IsForTariffMapping,
                IsDefault = entity.IsDefault,
                DefaultDailyRoomRate = entity.DefaultDailyRoomRate,
                DefaultRegistrationFee = entity.DefaultRegistrationFee,
                DefaultConsultationFee = entity.DefaultConsultationFee,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static PatientClassDetailResponse MapDetailResponse(MstPatientClass entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new PatientClassDetailResponse
            {
                Id = entity.Id,
                PatientClassCode = entity.PatientClassCode,
                PatientClassName = entity.PatientClassName,
                PatientClassType = entity.PatientClassType,
                PatientClassTypeName = BuildEnumLabel(entity.PatientClassType),
                ExternalClassCode = entity.ExternalClassCode,
                ClassAlias = entity.ClassAlias,
                ClassLevel = entity.ClassLevel,
                IsForOutpatient = entity.IsForOutpatient,
                IsForInpatient = entity.IsForInpatient,
                IsForEmergency = entity.IsForEmergency,
                IsForIntensiveCare = entity.IsForIntensiveCare,
                IsForNewborn = entity.IsForNewborn,
                IsForRoomCharge = entity.IsForRoomCharge,
                IsForTariffMapping = entity.IsForTariffMapping,
                IsDefault = entity.IsDefault,
                DefaultDailyRoomRate = entity.DefaultDailyRoomRate,
                DefaultRegistrationFee = entity.DefaultRegistrationFee,
                DefaultConsultationFee = entity.DefaultConsultationFee,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                Description = entity.Description,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static PatientClassOptionResponse MapOptionResponse(MstPatientClass entity)
        {
            return new PatientClassOptionResponse
            {
                Id = entity.Id,
                PatientClassCode = entity.PatientClassCode,
                PatientClassName = entity.PatientClassName,
                PatientClassType = entity.PatientClassType,
                PatientClassTypeName = BuildEnumLabel(entity.PatientClassType),
                ExternalClassCode = entity.ExternalClassCode,
                ClassAlias = entity.ClassAlias,
                ClassLevel = entity.ClassLevel,
                IsForOutpatient = entity.IsForOutpatient,
                IsForInpatient = entity.IsForInpatient,
                IsForEmergency = entity.IsForEmergency,
                IsForIntensiveCare = entity.IsForIntensiveCare,
                IsForNewborn = entity.IsForNewborn,
                IsForRoomCharge = entity.IsForRoomCharge,
                IsForTariffMapping = entity.IsForTariffMapping,
                IsDefault = entity.IsDefault,
                SortOrder = entity.SortOrder
            };
        }

        private static PatientClassCreateResponse MapCreateResponse(MstPatientClass entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new PatientClassCreateResponse
            {
                Id = entity.Id,
                PatientClassCode = entity.PatientClassCode,
                PatientClassName = entity.PatientClassName,
                PatientClassType = entity.PatientClassType,
                PatientClassTypeName = BuildEnumLabel(entity.PatientClassType),
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static PatientClassUpdateResponse MapUpdateResponse(MstPatientClass entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new PatientClassUpdateResponse
            {
                Id = entity.Id,
                PatientClassCode = entity.PatientClassCode,
                PatientClassName = entity.PatientClassName,
                PatientClassType = entity.PatientClassType,
                PatientClassTypeName = BuildEnumLabel(entity.PatientClassType),
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> actorNames, Guid actorId)
        {
            if (actorId == Guid.Empty)
            {
                return null;
            }

            return actorNames.TryGetValue(actorId, out var actorName) ? actorName : null;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;
            return (pageNumber, pageSize);
        }

        private static DateRangeResolveResult ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var today = AppDateTimeHelper.OperationalDate();

            if (!string.IsNullOrWhiteSpace(customPeriod) &&
                !string.Equals(customPeriod, "custom", StringComparison.OrdinalIgnoreCase))
            {
                return customPeriod.Trim().ToLowerInvariant() switch
                {
                    "today" => DateRangeResolveResult.Valid(today, today.AddDays(1)),
                    "yesterday" => DateRangeResolveResult.Valid(today.AddDays(-1), today),
                    "last7days" => DateRangeResolveResult.Valid(today.AddDays(-6), today.AddDays(1)),
                    "last30days" => DateRangeResolveResult.Valid(today.AddDays(-29), today.AddDays(1)),
                    "thismonth" => DateRangeResolveResult.Valid(new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                    "lastmonth" => ResolveLastMonth(today),
                    "thisyear" => DateRangeResolveResult.Valid(new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(today.Year + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
                    _ => DateRangeResolveResult.Invalid("Custom period tidak valid.")
                };
            }

            if (startDate.HasValue && endDate.HasValue && startDate.Value.Date > endDate.Value.Date)
            {
                return DateRangeResolveResult.Invalid("Start date tidak boleh lebih besar dari end date.");
            }

            return DateRangeResolveResult.Valid(startDate?.Date, endDate?.Date.AddDays(1));
        }

        private static DateRangeResolveResult ResolveLastMonth(DateTime today)
        {
            var thisMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            return DateRangeResolveResult.Valid(thisMonthStart.AddMonths(-1), thisMonthStart);
        }

        private static List<PatientClassCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<PatientClassCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "yesterday", Label = "Kemarin", Description = "Data yang dibuat kemarin.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thisyear", Label = "Tahun ini", Description = "Data yang dibuat pada tahun berjalan.", UsesStartDate = false, UsesEndDate = false }
            };
        }

        private static List<PatientClassEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new PatientClassEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = BuildEnumLabel(x)
                })
                .ToList();
        }

        private static string BuildEnumLabel<TEnum>(TEnum value) where TEnum : Enum
        {
            return SplitPascalCase(value.ToString());
        }

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private static List<PatientClassQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<PatientClassQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "date", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "date", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Preset periode. Pilihan: custom, today, yesterday, last7days, last30days, thismonth, lastmonth, thisyear.", Example = "thismonth" },
                new() { Name = "patientClassType", Type = "enum", Description = "Filter berdasarkan tipe patient class.", Example = "1" },
                new() { Name = "isActive", Type = "bool", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "isDefault", Type = "bool", Description = "Filter kelas default.", Example = "true" },
                new() { Name = "search", Type = "string", Description = "Pencarian kode, nama, alias, kode eksternal, atau deskripsi.", Example = "VIP" },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting.", Example = "sortOrder" },
                new() { Name = "sortDirection", Type = "string", Description = "asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman.", Example = "25" }
            };
        }

        private static List<PatientClassFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: false);
        }

        private static List<PatientClassFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: true);
        }

        private static List<PatientClassFormFieldMetadataResponse> BuildFieldMetadata(bool isUpdate)
        {
            var fields = new List<PatientClassFormFieldMetadataResponse>
            {
                new() { Name = "patientClassCode", Label = "Kode Kelas Pasien", Section = "Basic", InputType = "readonly", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "AutoGenerated", MaxLength = 50, Description = "Dibuat otomatis oleh sistem dengan format PC-RSMMC-00001.", Example = "PC-RSMMC-00001", SortOrder = 1 },
                new() { Name = "patientClassName", Label = "Nama Kelas Pasien", Section = "Basic", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 150, Example = "VIP", SortOrder = 2 },
                new() { Name = "patientClassType", Label = "Tipe Kelas Pasien", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "patientClassTypeOptions", SortOrder = 3 },
                new() { Name = "externalClassCode", Label = "Kode Kelas Eksternal", Section = "Integration", InputType = "text", MaxLength = 50, Example = "VIP", SortOrder = 4 },
                new() { Name = "classAlias", Label = "Alias Kelas", Section = "Basic", InputType = "text", MaxLength = 100, Example = "VIP A", SortOrder = 5 },
                new() { Name = "classLevel", Label = "Level Kelas", Section = "Basic", InputType = "number", Description = "Semakin kecil biasanya semakin tinggi prioritas urut.", Example = "1", SortOrder = 6 },
                new() { Name = "isForOutpatient", Label = "Untuk Rawat Jalan", Section = "Rule", InputType = "switch", SortOrder = 7 },
                new() { Name = "isForInpatient", Label = "Untuk Rawat Inap", Section = "Rule", InputType = "switch", SortOrder = 8 },
                new() { Name = "isForEmergency", Label = "Untuk IGD", Section = "Rule", InputType = "switch", SortOrder = 9 },
                new() { Name = "isForIntensiveCare", Label = "Untuk Intensive Care", Section = "Rule", InputType = "switch", SortOrder = 10 },
                new() { Name = "isForNewborn", Label = "Untuk Newborn", Section = "Rule", InputType = "switch", SortOrder = 11 },
                new() { Name = "isForRoomCharge", Label = "Untuk Biaya Kamar", Section = "Rule", InputType = "switch", SortOrder = 12 },
                new() { Name = "isForTariffMapping", Label = "Untuk Mapping Tarif", Section = "Rule", InputType = "switch", SortOrder = 13 },
                new() { Name = "isDefault", Label = "Default", Section = "Rule", InputType = "switch", SortOrder = 14 },
                new() { Name = "defaultDailyRoomRate", Label = "Default Tarif Kamar Harian", Section = "Default Tariff", InputType = "number", SortOrder = 15 },
                new() { Name = "defaultRegistrationFee", Label = "Default Biaya Registrasi", Section = "Default Tariff", InputType = "number", SortOrder = 16 },
                new() { Name = "defaultConsultationFee", Label = "Default Biaya Konsultasi", Section = "Default Tariff", InputType = "number", SortOrder = 17 },
                new() { Name = "sortOrder", Label = "Urutan", Section = "Display", InputType = "number", SortOrder = 18 },
                new() { Name = "description", Label = "Deskripsi", Section = "Additional", InputType = "textarea", MaxLength = 250, SortOrder = 19 }
            };

            if (isUpdate)
            {
                fields.Add(new PatientClassFormFieldMetadataResponse
                {
                    Name = "isActive",
                    Label = "Status Aktif",
                    Section = "Status",
                    InputType = "switch",
                    SortOrder = 99
                });
            }

            return fields.OrderBy(x => x.SortOrder).ToList();
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdText, out var userId) ? userId : Guid.Empty;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private sealed class DateRangeResolveResult
        {
            public bool IsValid { get; private set; }
            public string? ErrorMessage { get; private set; }
            public DateTime? Start { get; private set; }
            public DateTime? EndExclusive { get; private set; }

            public static DateRangeResolveResult Valid(DateTime? start, DateTime? endExclusive)
            {
                return new DateRangeResolveResult
                {
                    IsValid = true,
                    Start = start,
                    EndExclusive = endExclusive
                };
            }

            public static DateRangeResolveResult Invalid(string errorMessage)
            {
                return new DateRangeResolveResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}
