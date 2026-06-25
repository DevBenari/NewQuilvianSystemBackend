using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseWorkforceInsurancePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs.WorkforceInsuranceResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/insurances")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE",
        moduleName: "Human Resource Workforce",
        displayName: "Workforce Insurance",
        AreaName = "Corporate",
        ControllerName = "WorkforceInsurance",
        Description = "Workforce insurance management",
        SortOrder = 30
    )]
    [Tags("Corporate / Human Resource / Workforce / Insurance")]
    public class WorkforceInsuranceController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.Insurance";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceInsuranceController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceInsuranceFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Insurance", Description = "Melihat metadata filter insurance workforce profile", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceInsurance", "Read")]
        public async Task<IActionResult> GetFilterMetadata(Guid workforceProfileId)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var result = new WorkforceInsuranceFilterMetadataResponse
            {
                DefaultFilter = new WorkforceInsuranceDefaultFilterResponse(),
                CustomPeriods = new List<WorkforceInsuranceCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<WorkforceInsuranceSortOptionResponse>
                {
                    new() { Value = "effectiveStartDate", Label = "Tanggal mulai efektif" },
                    new() { Value = "effectiveEndDate", Label = "Tanggal selesai efektif" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "bpjsKesehatanNumber", Label = "Nomor BPJS Kesehatan" },
                    new() { Value = "bpjsKetenagakerjaanNumber", Label = "Nomor BPJS Ketenagakerjaan" },
                    new() { Value = "privateInsuranceProvider", Label = "Provider asuransi private" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                CoverageTypes = new List<WorkforceInsuranceCoverageTypeOptionResponse>
                {
                    new()
                    {
                        Value = "bpjsKesehatan",
                        Label = "BPJS Kesehatan",
                        Description = "Filter data yang mengaktifkan BPJS Kesehatan."
                    },
                    new()
                    {
                        Value = "bpjsKetenagakerjaan",
                        Label = "BPJS Ketenagakerjaan",
                        Description = "Filter data yang mengaktifkan BPJS Ketenagakerjaan."
                    },
                    new()
                    {
                        Value = "privateInsurance",
                        Label = "Private Insurance",
                        Description = "Filter data yang mengaktifkan asuransi private."
                    }
                }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceInsurance.GetFilterMetadata",
                "Mengambil metadata filter insurance workforce.",
                new { workforceProfileId, profile.ProfileCode }
            );

            return Ok(ApiResponse<WorkforceInsuranceFilterMetadataResponse>.Ok(
                result,
                "Metadata filter insurance workforce berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceInsuranceSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Insurance", Description = "Melihat ringkasan insurance workforce profile", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceInsurance", "Read")]
        public async Task<IActionResult> GetSummary(Guid workforceProfileId)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var query = BuildBaseQuery(workforceProfileId);
            var today = AppDateTimeHelper.OperationalDate();

            var result = new WorkforceInsuranceSummaryResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalInsurance = await query.CountAsync(),
                ActiveInsurance = await query.CountAsync(x => x.IsActive),
                InactiveInsurance = await query.CountAsync(x => !x.IsActive),
                ExpiredInsurance = await query.CountAsync(x => x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value < today),
                CurrentlyValidInsurance = await query.CountAsync(x =>
                    x.IsActive &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today) &&
                    (x.IsBpjsKesehatanEnabled || x.IsBpjsKetenagakerjaanEnabled || x.IsPrivateInsuranceEnabled)),
                BpjsKesehatanInsurance = await query.CountAsync(x => x.IsBpjsKesehatanEnabled),
                BpjsKetenagakerjaanInsurance = await query.CountAsync(x => x.IsBpjsKetenagakerjaanEnabled),
                PrivateInsurance = await query.CountAsync(x => x.IsPrivateInsuranceEnabled)
            };

            return Ok(ApiResponse<WorkforceInsuranceSummaryResponse>.Ok(
                result,
                "Ringkasan insurance workforce berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseWorkforceInsurancePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Insurance", Description = "Melihat insurance workforce profile", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceInsurance", "Read")]
        public async Task<IActionResult> GetInsurances(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? coverageType,
            [FromQuery] Guid? privateInsuranceProviderId,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isExpired,
            [FromQuery] bool? isCurrentlyValid,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "effectiveStartDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyStandardFilter(query, coverageType, privateInsuranceProviderId, isActive, isExpired, isCurrentlyValid, search);

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities
                    .Select(x => x.CreateBy)
                    .Where(x => x != Guid.Empty)
            );

            var items = entities
                .Select(x => MapResponse(x, profile, actorNames))
                .ToList();

            var result = new ResponseWorkforceInsurancePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseWorkforceInsurancePagedResult>.Ok(
                result,
                "Data insurance workforce berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceInsuranceOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Insurance", Description = "Melihat pilihan insurance workforce profile", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceInsurance", "Read")]
        public async Task<IActionResult> GetOptions(
            Guid workforceProfileId,
            [FromQuery] string? coverageType,
            [FromQuery] Guid? privateInsuranceProviderId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var profileExists = await ProfileExistsAsync(workforceProfileId);

            if (!profileExists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyStandardFilter(query, coverageType, privateInsuranceProviderId, onlyActive ? true : null, null, null, search);

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.IsActive)
                .ThenByDescending(x => x.EffectiveStartDate)
                .ThenByDescending(x => x.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapOptionResponse)
                .ToList();

            var result = new WorkforceInsuranceOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<WorkforceInsuranceOptionPagedResponse>.Ok(
                result,
                "Data pilihan insurance workforce berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceInsuranceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Insurance", Description = "Melihat detail insurance workforce profile", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceInsurance", "Read")]
        public async Task<IActionResult> GetInsuranceById(Guid workforceProfileId, Guid id)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Insurance workforce tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, profile, actorNames);
            NormalizeAuditFields(data);

            return Ok(ApiResponse<WorkforceInsuranceDetailResponse>.Ok(
                data,
                "Detail insurance workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforceInsuranceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Create", "Create Workforce Insurance", Description = "Menambah insurance workforce profile", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceInsurance", "Create")]
        public async Task<IActionResult> CreateInsurance(
            Guid workforceProfileId,
            [FromBody] CreateWorkforceInsuranceRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var validation = await ValidateInsuranceRequestAsync(
                request.IsBpjsKesehatanEnabled,
                request.BpjsKesehatanNumber,
                request.IsBpjsKetenagakerjaanEnabled,
                request.BpjsKetenagakerjaanNumber,
                request.IsPrivateInsuranceEnabled,
                request.PrivateInsuranceProviderId,
                request.PrivateInsuranceNumber,
                request.EffectiveStartDate,
                request.EffectiveEndDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data insurance tidak valid."
                ));
            }

            var duplicate = await BuildBaseQuery(workforceProfileId)
                .AsNoTracking()
                .AnyAsync();

            if (duplicate)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Insurance workforce profile ini sudah tersedia. Gunakan endpoint update."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var privateInsuranceProviderId = request.IsPrivateInsuranceEnabled
                ? NormalizeNullableGuid(request.PrivateInsuranceProviderId)
                : null;

            var entity = new WfpInsurance
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                IsBpjsKesehatanEnabled = request.IsBpjsKesehatanEnabled,
                BpjsKesehatanNumber = request.IsBpjsKesehatanEnabled ? NormalizeInsuranceNumber(request.BpjsKesehatanNumber) : null,
                IsBpjsKetenagakerjaanEnabled = request.IsBpjsKetenagakerjaanEnabled,
                BpjsKetenagakerjaanNumber = request.IsBpjsKetenagakerjaanEnabled ? NormalizeInsuranceNumber(request.BpjsKetenagakerjaanNumber) : null,
                IsPrivateInsuranceEnabled = request.IsPrivateInsuranceEnabled,
                PrivateInsuranceProviderId = privateInsuranceProviderId,
                PrivateInsuranceNumber = request.IsPrivateInsuranceEnabled ? NormalizeNullableText(request.PrivateInsuranceNumber) : null,
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                Description = NormalizeNullableText(request.Description),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpInsurance>().Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceInsurance.CreateInsurance",
                "Insurance workforce berhasil dibuat.",
                new
                {
                    WorkforceProfileId = workforceProfileId,
                    entity.Id,
                    entity.IsBpjsKesehatanEnabled,
                    entity.IsBpjsKetenagakerjaanEnabled,
                    entity.IsPrivateInsuranceEnabled
                }
            );

            var savedEntity = await BuildBaseQuery(workforceProfileId)
                .FirstAsync(x => x.Id == entity.Id);

            var actorNames = await GetActorNameMapAsync(new[]
            {
                savedEntity.CreateBy,
                savedEntity.UpdateBy
            });

            var data = MapDetailResponse(savedEntity, profile, actorNames);
            NormalizeAuditFields(data);

            return Ok(ApiResponse<WorkforceInsuranceDetailResponse>.Ok(
                data,
                "Insurance workforce berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceInsuranceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Insurance", Description = "Mengubah insurance workforce profile", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceInsurance", "Update")]
        public async Task<IActionResult> UpdateInsurance(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceInsuranceRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpInsurance>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Insurance workforce tidak ditemukan."
                ));
            }

            var validation = await ValidateInsuranceRequestAsync(
                request.IsBpjsKesehatanEnabled,
                request.BpjsKesehatanNumber,
                request.IsBpjsKetenagakerjaanEnabled,
                request.BpjsKetenagakerjaanNumber,
                request.IsPrivateInsuranceEnabled,
                request.PrivateInsuranceProviderId,
                request.PrivateInsuranceNumber,
                request.EffectiveStartDate,
                request.EffectiveEndDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data insurance tidak valid."
                ));
            }

            var privateInsuranceProviderId = request.IsPrivateInsuranceEnabled
                ? NormalizeNullableGuid(request.PrivateInsuranceProviderId)
                : null;

            entity.IsBpjsKesehatanEnabled = request.IsBpjsKesehatanEnabled;
            entity.BpjsKesehatanNumber = request.IsBpjsKesehatanEnabled ? NormalizeInsuranceNumber(request.BpjsKesehatanNumber) : null;
            entity.IsBpjsKetenagakerjaanEnabled = request.IsBpjsKetenagakerjaanEnabled;
            entity.BpjsKetenagakerjaanNumber = request.IsBpjsKetenagakerjaanEnabled ? NormalizeInsuranceNumber(request.BpjsKetenagakerjaanNumber) : null;
            entity.IsPrivateInsuranceEnabled = request.IsPrivateInsuranceEnabled;
            entity.PrivateInsuranceProviderId = privateInsuranceProviderId;
            entity.PrivateInsuranceNumber = request.IsPrivateInsuranceEnabled ? NormalizeNullableText(request.PrivateInsuranceNumber) : null;
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceInsurance.UpdateInsurance",
                "Insurance workforce berhasil diperbarui.",
                new
                {
                    WorkforceProfileId = workforceProfileId,
                    entity.Id,
                    entity.IsBpjsKesehatanEnabled,
                    entity.IsBpjsKetenagakerjaanEnabled,
                    entity.IsPrivateInsuranceEnabled
                }
            );

            var savedEntity = await BuildBaseQuery(workforceProfileId)
                .FirstAsync(x => x.Id == entity.Id);

            var actorNames = await GetActorNameMapAsync(new[]
            {
                savedEntity.CreateBy,
                savedEntity.UpdateBy
            });

            var data = MapDetailResponse(savedEntity, profile, actorNames);
            NormalizeAuditFields(data);

            return Ok(ApiResponse<WorkforceInsuranceDetailResponse>.Ok(
                data,
                "Insurance workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceInsuranceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Insurance", Description = "Mengubah status insurance workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceInsurance", "Update")]
        public async Task<IActionResult> UpdateInsuranceStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceInsuranceStatusRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpInsurance>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Insurance workforce tidak ditemukan."
                ));
            }

            if (request.EffectiveEndDate.HasValue &&
                entity.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.Value.Date < entity.EffectiveStartDate.Value.Date)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate."
                ));
            }

            entity.IsActive = request.IsActive;

            if (request.EffectiveEndDate.HasValue)
            {
                entity.EffectiveEndDate = request.EffectiveEndDate.Value.Date;
            }

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                entity.Description = NormalizeNullableText(request.Description);
            }

            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var savedEntity = await BuildBaseQuery(workforceProfileId)
                .FirstAsync(x => x.Id == entity.Id);

            var actorNames = await GetActorNameMapAsync(new[]
            {
                savedEntity.CreateBy,
                savedEntity.UpdateBy
            });

            var data = MapDetailResponse(savedEntity, profile, actorNames);
            NormalizeAuditFields(data);

            return Ok(ApiResponse<WorkforceInsuranceDetailResponse>.Ok(
                data,
                "Status insurance workforce berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Insurance", Description = "Menghapus insurance workforce", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WorkforceInsurance", "Delete")]
        public async Task<IActionResult> DeleteInsurance(Guid workforceProfileId, Guid id)
        {
            var entity = await _dbContext.Set<WfpInsurance>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Insurance workforce tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Insurance workforce berhasil dihapus."
            ));
        }

        private IQueryable<WfpInsurance> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpInsurance>()
                .AsNoTracking()
                .Include(x => x.PrivateInsuranceProvider)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
        }

        private static IQueryable<WfpInsurance> ApplyDateFilter(
            IQueryable<WfpInsurance> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var today = AppDateTimeHelper.OperationalDate();
            DateTime? effectiveStartDate = startDate?.Date;
            DateTime? effectiveEndDate = endDate?.Date;

            if (!string.IsNullOrWhiteSpace(customPeriod) &&
                !string.Equals(customPeriod, "custom", StringComparison.OrdinalIgnoreCase))
            {
                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        effectiveStartDate = today;
                        effectiveEndDate = today;
                        break;
                    case "last7days":
                        effectiveStartDate = today.AddDays(-6);
                        effectiveEndDate = today;
                        break;
                    case "thismonth":
                        effectiveStartDate = new DateTime(today.Year, today.Month, 1);
                        effectiveEndDate = effectiveStartDate.Value.AddMonths(1).AddDays(-1);
                        break;
                    case "lastmonth":
                        var firstDayThisMonth = new DateTime(today.Year, today.Month, 1);
                        effectiveStartDate = firstDayThisMonth.AddMonths(-1);
                        effectiveEndDate = firstDayThisMonth.AddDays(-1);
                        break;
                }
            }

            if (effectiveStartDate.HasValue)
            {
                query = query.Where(x => x.EffectiveStartDate.HasValue && x.EffectiveStartDate.Value >= effectiveStartDate.Value);
            }

            if (effectiveEndDate.HasValue)
            {
                var endExclusive = effectiveEndDate.Value.AddDays(1);
                query = query.Where(x => x.EffectiveStartDate.HasValue && x.EffectiveStartDate.Value < endExclusive);
            }

            return query;
        }

        private static IQueryable<WfpInsurance> ApplyStandardFilter(
            IQueryable<WfpInsurance> query,
            string? coverageType,
            Guid? privateInsuranceProviderId,
            bool? isActive,
            bool? isExpired,
            bool? isCurrentlyValid,
            string? search)
        {
            var today = AppDateTimeHelper.OperationalDate();

            if (!string.IsNullOrWhiteSpace(coverageType))
            {
                switch (coverageType.Trim().ToLowerInvariant())
                {
                    case "bpjskesehatan":
                    case "bpjs_kesehatan":
                    case "bpjs-kesehatan":
                        query = query.Where(x => x.IsBpjsKesehatanEnabled);
                        break;
                    case "bpjsketenagakerjaan":
                    case "bpjs_ketenagakerjaan":
                    case "bpjs-ketenagakerjaan":
                        query = query.Where(x => x.IsBpjsKetenagakerjaanEnabled);
                        break;
                    case "privateinsurance":
                    case "private_insurance":
                    case "private-insurance":
                        query = query.Where(x => x.IsPrivateInsuranceEnabled);
                        break;
                }
            }

            if (privateInsuranceProviderId.HasValue && privateInsuranceProviderId.Value != Guid.Empty)
            {
                query = query.Where(x => x.PrivateInsuranceProviderId == privateInsuranceProviderId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (isExpired.HasValue)
            {
                query = isExpired.Value
                    ? query.Where(x => x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value < today)
                    : query.Where(x => !x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today);
            }

            if (isCurrentlyValid.HasValue)
            {
                query = isCurrentlyValid.Value
                    ? query.Where(x =>
                        x.IsActive &&
                        (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today) &&
                        (x.IsBpjsKesehatanEnabled || x.IsBpjsKetenagakerjaanEnabled || x.IsPrivateInsuranceEnabled))
                    : query.Where(x =>
                        !x.IsActive ||
                        (x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value < today) ||
                        (!x.IsBpjsKesehatanEnabled && !x.IsBpjsKetenagakerjaanEnabled && !x.IsPrivateInsuranceEnabled));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLowerInvariant();
                var numberKeyword = NormalizeInsuranceNumber(search);

                query = query.Where(x =>
                    (x.PrivateInsuranceProvider != null && x.PrivateInsuranceProvider.InsuranceProviderCode.ToLower().Contains(keyword)) ||
                    (x.PrivateInsuranceProvider != null && x.PrivateInsuranceProvider.InsuranceProviderName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrWhiteSpace(numberKeyword) && x.BpjsKesehatanNumber != null && x.BpjsKesehatanNumber.Contains(numberKeyword)) ||
                    (!string.IsNullOrWhiteSpace(numberKeyword) && x.BpjsKetenagakerjaanNumber != null && x.BpjsKetenagakerjaanNumber.Contains(numberKeyword)) ||
                    (x.PrivateInsuranceNumber != null && x.PrivateInsuranceNumber.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpInsurance> ApplySorting(
            IQueryable<WfpInsurance> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "effectiveStartDate").Trim().ToLowerInvariant() switch
            {
                "effectivestartdate" => isDescending ? query.OrderByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.EffectiveStartDate),
                "effectiveenddate" => isDescending ? query.OrderByDescending(x => x.EffectiveEndDate) : query.OrderBy(x => x.EffectiveEndDate),
                "bpjskesehatannumber" => isDescending ? query.OrderByDescending(x => x.BpjsKesehatanNumber) : query.OrderBy(x => x.BpjsKesehatanNumber),
                "bpjsketenagakerjaannumber" => isDescending ? query.OrderByDescending(x => x.BpjsKetenagakerjaanNumber) : query.OrderBy(x => x.BpjsKetenagakerjaanNumber),
                "privateinsuranceprovider" => isDescending
                    ? query.OrderByDescending(x => x.PrivateInsuranceProvider == null ? string.Empty : x.PrivateInsuranceProvider.InsuranceProviderName)
                    : query.OrderBy(x => x.PrivateInsuranceProvider == null ? string.Empty : x.PrivateInsuranceProvider.InsuranceProviderName),
                "isactive" => isDescending ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate),
                "createdatetime" => isDescending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDescending ? query.OrderByDescending(x => x.EffectiveStartDate).ThenByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.EffectiveStartDate).ThenBy(x => x.CreateDateTime)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateInsuranceRequestAsync(
            bool isBpjsKesehatanEnabled,
            string? bpjsKesehatanNumber,
            bool isBpjsKetenagakerjaanEnabled,
            string? bpjsKetenagakerjaanNumber,
            bool isPrivateInsuranceEnabled,
            Guid? privateInsuranceProviderId,
            string? privateInsuranceNumber,
            DateTime? effectiveStartDate,
            DateTime? effectiveEndDate)
        {
            if (!isBpjsKesehatanEnabled &&
                !isBpjsKetenagakerjaanEnabled &&
                !isPrivateInsuranceEnabled)
            {
                return (false, "Minimal satu jenis insurance harus diaktifkan.");
            }

            if (isBpjsKesehatanEnabled && string.IsNullOrWhiteSpace(bpjsKesehatanNumber))
            {
                return (false, "Nomor BPJS Kesehatan wajib diisi jika BPJS Kesehatan aktif.");
            }

            if (isBpjsKetenagakerjaanEnabled && string.IsNullOrWhiteSpace(bpjsKetenagakerjaanNumber))
            {
                return (false, "Nomor BPJS Ketenagakerjaan wajib diisi jika BPJS Ketenagakerjaan aktif.");
            }

            if (isPrivateInsuranceEnabled)
            {
                if (!privateInsuranceProviderId.HasValue || privateInsuranceProviderId.Value == Guid.Empty)
                {
                    return (false, "PrivateInsuranceProviderId wajib diisi jika private insurance aktif.");
                }

                var providerExists = await _dbContext.Set<MstInsuranceProvider>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == privateInsuranceProviderId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!providerExists)
                {
                    return (false, "Insurance provider tidak valid atau tidak aktif.");
                }

                if (string.IsNullOrWhiteSpace(privateInsuranceNumber))
                {
                    return (false, "Nomor asuransi private wajib diisi jika private insurance aktif.");
                }
            }

            if (isBpjsKesehatanEnabled && NormalizeInsuranceNumber(bpjsKesehatanNumber).Length < 5)
            {
                return (false, "Nomor BPJS Kesehatan minimal 5 digit.");
            }

            if (isBpjsKetenagakerjaanEnabled && NormalizeInsuranceNumber(bpjsKetenagakerjaanNumber).Length < 5)
            {
                return (false, "Nomor BPJS Ketenagakerjaan minimal 5 digit.");
            }

            if (effectiveStartDate.HasValue &&
                effectiveEndDate.HasValue &&
                effectiveEndDate.Value.Date < effectiveStartDate.Value.Date)
            {
                return (false, "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate.");
            }

            return (true, null);
        }

        private WorkforceInsuranceResponse MapResponse(
            WfpInsurance entity,
            MstWorkforceProfile profile,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var today = AppDateTimeHelper.OperationalDate();
            var isExpired = entity.EffectiveEndDate.HasValue && entity.EffectiveEndDate.Value.Date < today;
            var hasCoverage = entity.IsBpjsKesehatanEnabled || entity.IsBpjsKetenagakerjaanEnabled || entity.IsPrivateInsuranceEnabled;
            var providerName = entity.PrivateInsuranceProvider?.InsuranceProviderName;

            return new WorkforceInsuranceResponse
            {
                Id = entity.Id,
                WorkforceProfileId = entity.WorkforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                IsBpjsKesehatanEnabled = entity.IsBpjsKesehatanEnabled,
                BpjsKesehatanNumber = entity.BpjsKesehatanNumber,
                BpjsKesehatanNumberMasked = MaskNumber(entity.BpjsKesehatanNumber),
                IsBpjsKetenagakerjaanEnabled = entity.IsBpjsKetenagakerjaanEnabled,
                BpjsKetenagakerjaanNumber = entity.BpjsKetenagakerjaanNumber,
                BpjsKetenagakerjaanNumberMasked = MaskNumber(entity.BpjsKetenagakerjaanNumber),
                IsPrivateInsuranceEnabled = entity.IsPrivateInsuranceEnabled,
                PrivateInsuranceProviderId = entity.PrivateInsuranceProviderId,
                PrivateInsuranceProviderCode = entity.PrivateInsuranceProvider?.InsuranceProviderCode,
                PrivateInsuranceProviderName = providerName,
                PrivateInsuranceProvider = providerName,
                PrivateInsuranceNumber = entity.PrivateInsuranceNumber,
                PrivateInsuranceNumberMasked = MaskNumber(entity.PrivateInsuranceNumber),
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                IsExpired = isExpired,
                IsCurrentlyValid = entity.IsActive && !isExpired && hasCoverage,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private WorkforceInsuranceDetailResponse MapDetailResponse(
            WfpInsurance entity,
            MstWorkforceProfile profile,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = MapResponse(entity, profile, actorNames);

            return new WorkforceInsuranceDetailResponse
            {
                Id = response.Id,
                WorkforceProfileId = response.WorkforceProfileId,
                ProfileCode = response.ProfileCode,
                DisplayName = response.DisplayName,
                IsBpjsKesehatanEnabled = response.IsBpjsKesehatanEnabled,
                BpjsKesehatanNumber = response.BpjsKesehatanNumber,
                BpjsKesehatanNumberMasked = response.BpjsKesehatanNumberMasked,
                IsBpjsKetenagakerjaanEnabled = response.IsBpjsKetenagakerjaanEnabled,
                BpjsKetenagakerjaanNumber = response.BpjsKetenagakerjaanNumber,
                BpjsKetenagakerjaanNumberMasked = response.BpjsKetenagakerjaanNumberMasked,
                IsPrivateInsuranceEnabled = response.IsPrivateInsuranceEnabled,
                PrivateInsuranceProviderId = response.PrivateInsuranceProviderId,
                PrivateInsuranceProviderCode = response.PrivateInsuranceProviderCode,
                PrivateInsuranceProviderName = response.PrivateInsuranceProviderName,
                PrivateInsuranceProvider = response.PrivateInsuranceProvider,
                PrivateInsuranceNumber = response.PrivateInsuranceNumber,
                PrivateInsuranceNumberMasked = response.PrivateInsuranceNumberMasked,
                EffectiveStartDate = response.EffectiveStartDate,
                EffectiveEndDate = response.EffectiveEndDate,
                IsExpired = response.IsExpired,
                IsCurrentlyValid = response.IsCurrentlyValid,
                Description = response.Description,
                IsActive = response.IsActive,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static WorkforceInsuranceOptionResponse MapOptionResponse(WfpInsurance entity)
        {
            var today = AppDateTimeHelper.OperationalDate();
            var isExpired = entity.EffectiveEndDate.HasValue && entity.EffectiveEndDate.Value.Date < today;
            var hasCoverage = entity.IsBpjsKesehatanEnabled || entity.IsBpjsKetenagakerjaanEnabled || entity.IsPrivateInsuranceEnabled;
            var labels = new List<string>();
            var providerName = entity.PrivateInsuranceProvider?.InsuranceProviderName;

            if (entity.IsBpjsKesehatanEnabled)
            {
                labels.Add("BPJS Kesehatan");
            }

            if (entity.IsBpjsKetenagakerjaanEnabled)
            {
                labels.Add("BPJS Ketenagakerjaan");
            }

            if (entity.IsPrivateInsuranceEnabled)
            {
                labels.Add(string.IsNullOrWhiteSpace(providerName)
                    ? "Private Insurance"
                    : providerName);
            }

            return new WorkforceInsuranceOptionResponse
            {
                Id = entity.Id,
                DisplayLabel = labels.Any() ? string.Join(" + ", labels) : "Insurance",
                IsBpjsKesehatanEnabled = entity.IsBpjsKesehatanEnabled,
                BpjsKesehatanNumberMasked = MaskNumber(entity.BpjsKesehatanNumber),
                IsBpjsKetenagakerjaanEnabled = entity.IsBpjsKetenagakerjaanEnabled,
                BpjsKetenagakerjaanNumberMasked = MaskNumber(entity.BpjsKetenagakerjaanNumber),
                IsPrivateInsuranceEnabled = entity.IsPrivateInsuranceEnabled,
                PrivateInsuranceProviderId = entity.PrivateInsuranceProviderId,
                PrivateInsuranceProviderCode = entity.PrivateInsuranceProvider?.InsuranceProviderCode,
                PrivateInsuranceProviderName = providerName,
                PrivateInsuranceProvider = providerName,
                PrivateInsuranceNumberMasked = MaskNumber(entity.PrivateInsuranceNumber),
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                IsCurrentlyValid = entity.IsActive && !isExpired && hasCoverage,
                IsActive = entity.IsActive
            };
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
                    Name =
                        x.DisplayName ??
                        x.UserName ??
                        x.Email ??
                        x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
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

        private static void NormalizeAuditFields(WorkforceInsuranceDetailResponse data)
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

        private async Task<MstWorkforceProfile?> GetProfileAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);
        }

        private async Task<bool> ProfileExistsAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == workforceProfileId && !x.IsDelete);
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
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

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            return value.HasValue && value.Value != Guid.Empty
                ? value.Value
                : null;
        }

        private static string NormalizeInsuranceNumber(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : new string(value.Where(char.IsDigit).ToArray());
        }

        private static string? MaskNumber(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = NormalizeInsuranceNumber(value);

            if (string.IsNullOrWhiteSpace(normalized))
            {
                return value;
            }

            if (normalized.Length <= 4)
            {
                return normalized;
            }

            var suffix = normalized[^4..];
            var maskedLength = normalized.Length - 4;

            return new string('*', maskedLength) + suffix;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
