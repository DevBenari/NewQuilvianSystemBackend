using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseNurseStationClusterPagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs.NurseStationClusterResponse>;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/administrator/master-data/nurse-station-clusters")]
    [AccessController(
        moduleCode: "ADMINISTRATOR_MASTER_DATA",
        moduleName: "Administrator Master Data",
        displayName: "Nurse Station Cluster",
        AreaName = "Administrator",
        ControllerName = "NurseStationCluster",
        Description = "Administrator master data nurse station cluster",
        SortOrder = 101
    )]
    [Tags("Administrator / Master Data / Nurse Station Cluster")]
    public class NurseStationClusterController : ControllerBase
    {
        private const string LogCategory = "Administrator.MasterData";
        private const string CodePrefix = "NSC-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public NurseStationClusterController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<NurseStationClusterFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nurse Station Cluster", Description = "Melihat metadata filter nurse station cluster", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationCluster", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new NurseStationClusterFilterMetadataResponse
            {
                CustomPeriods = BuildCustomPeriods(),
                SortOptions = new List<NurseStationClusterSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "clusterCode", Label = "Kode cluster" },
                    new() { Value = "clusterName", Label = "Nama cluster" },
                    new() { Value = "shortName", Label = "Nama singkat" },
                    new() { Value = "locationName", Label = "Lokasi" },
                    new() { Value = "floorName", Label = "Lantai" },
                    new() { Value = "isDefault", Label = "Default" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(LogCategory, "NurseStationCluster.GetFilterMetadata", "Mengambil metadata filter nurse station cluster.", result);

            return Ok(ApiResponse<NurseStationClusterFilterMetadataResponse>.Ok(result, "Metadata filter nurse station cluster berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<NurseStationClusterSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nurse Station Cluster", Description = "Melihat ringkasan nurse station cluster", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationCluster", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();
            var result = new NurseStationClusterSummaryResponse
            {
                TotalCluster = await query.CountAsync(),
                ActiveCluster = await query.CountAsync(x => x.IsActive),
                InactiveCluster = await query.CountAsync(x => !x.IsActive),
                DefaultCluster = await query.CountAsync(x => x.IsDefault),
                AvailableForRegistrationQueue = await query.CountAsync(x => x.IsAvailableForRegistrationQueue),
                AvailableForScreening = await query.CountAsync(x => x.IsAvailableForScreening),
                AvailableForDisplay = await query.CountAsync(x => x.IsAvailableForDisplay)
            };

            return Ok(ApiResponse<NurseStationClusterSummaryResponse>.Ok(result, "Ringkasan nurse station cluster berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseNurseStationClusterPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nurse Station Cluster", Description = "Melihat data nurse station cluster", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationCluster", "Read")]
        public async Task<IActionResult> GetNurseStationClusters([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? customPeriod, [FromQuery] Guid? serviceUnitId, [FromQuery] bool? isActive, [FromQuery] bool? isDefault, [FromQuery] string? search, [FromQuery] string? sortBy = "sortOrder", [FromQuery] string? sortDirection = "asc", [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyFilter(ApplyDateFilter(BuildBaseQuery(), startDate, endDate, customPeriod), serviceUnitId, isActive, isDefault, search);
            var totalData = await query.CountAsync();
            var entities = await ApplySorting(query, sortBy, sortDirection).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var actorNames = await GetActorNameMapAsync(entities.Select(x => x.CreateBy).Where(x => x != Guid.Empty));

            var result = new ResponseNurseStationClusterPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(x => MapResponse(x, actorNames)).ToList()
            };

            return Ok(ApiResponse<ResponseNurseStationClusterPagedResult>.Ok(result, "Data nurse station cluster berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<NurseStationClusterOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nurse Station Cluster", Description = "Melihat data pilihan nurse station cluster", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationCluster", "Read")]
        public async Task<IActionResult> GetNurseStationClusterOptions([FromQuery] Guid? serviceUnitId, [FromQuery] bool onlyActive = true, [FromQuery] string? search = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;
            var query = ApplyFilter(BuildBaseQuery(), serviceUnitId, onlyActive ? true : null, null, search);
            var totalData = await query.CountAsync();
            var items = await query.OrderByDescending(x => x.IsDefault).ThenBy(x => x.SortOrder).ThenBy(x => x.ClusterName).Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(x => MapOptionResponse(x)).ToListAsync();
            var result = new NurseStationClusterOptionPagedResponse { PageNumber = pageNumber, PageSize = pageSize, TotalData = totalData, TotalPage = (int)Math.Ceiling(totalData / (double)pageSize), Items = items };
            return Ok(ApiResponse<NurseStationClusterOptionPagedResponse>.Ok(result, "Data pilihan nurse station cluster berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<NurseStationClusterDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Nurse Station Cluster", Description = "Melihat detail nurse station cluster", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationCluster", "Read")]
        public async Task<IActionResult> GetNurseStationClusterById(Guid id)
        {
            var entity = await BuildBaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Nurse station cluster tidak ditemukan."));
            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            return Ok(ApiResponse<NurseStationClusterDetailResponse>.Ok(MapDetailResponse(entity, actorNames), "Detail nurse station cluster berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<NurseStationClusterCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Nurse Station Cluster", Description = "Membuat data nurse station cluster", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("NurseStationCluster", "Create")]
        public async Task<IActionResult> CreateNurseStationCluster([FromBody] CreateNurseStationClusterRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data nurse station cluster tidak valid."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                if (request.IsDefault) await UnsetOtherDefaultAsync(null, now, actorUserId);
                var entity = new MstNurseStationCluster
                {
                    Id = Guid.NewGuid(),
                    ServiceUnitId = request.ServiceUnitId,
                    ClusterCode = await GenerateCodeAsync(),
                    ClusterName = request.ClusterName.Trim(),
                    ShortName = NormalizeNullableString(request.ShortName),
                    LocationName = NormalizeNullableString(request.LocationName),
                    FloorName = NormalizeNullableString(request.FloorName),
                    RoomName = NormalizeNullableString(request.RoomName),
                    IsAvailableForRegistrationQueue = request.IsAvailableForRegistrationQueue,
                    IsAvailableForScreening = request.IsAvailableForScreening,
                    IsAvailableForDisplay = request.IsAvailableForDisplay,
                    IsDefault = request.IsDefault,
                    SortOrder = request.SortOrder,
                    Description = NormalizeNullableString(request.Description),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };
                _dbContext.Set<MstNurseStationCluster>().Add(entity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                var result = new NurseStationClusterCreateResponse { Id = entity.Id, ClusterCode = entity.ClusterCode, ClusterName = entity.ClusterName, IsActive = entity.IsActive };
                await _loggerService.InfoAsync(LogCategory, "NurseStationCluster.Create", "Membuat data nurse station cluster.", result);
                return Ok(ApiResponse<NurseStationClusterCreateResponse>.Ok(result, "Nurse station cluster berhasil dibuat."));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggerService.ErrorAsync(LogCategory, "NurseStationCluster.Create", "Gagal membuat data nurse station cluster.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail(StatusCodes.Status500InternalServerError, "Terjadi kesalahan saat membuat nurse station cluster."));
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Nurse Station Cluster", Description = "Mengubah data nurse station cluster", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("NurseStationCluster", "Update")]
        public async Task<IActionResult> UpdateNurseStationCluster(Guid id, [FromBody] UpdateNurseStationClusterRequest request)
        {
            var entity = await _dbContext.Set<MstNurseStationCluster>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Nurse station cluster tidak ditemukan."));
            if (request.IsDefault && !request.IsActive) return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Nurse station cluster default harus aktif."));
            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data nurse station cluster tidak valid."));
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                if (request.IsDefault) await UnsetOtherDefaultAsync(id, now, actorUserId);
                entity.ServiceUnitId = request.ServiceUnitId;
                entity.ClusterName = request.ClusterName.Trim();
                entity.ShortName = NormalizeNullableString(request.ShortName);
                entity.LocationName = NormalizeNullableString(request.LocationName);
                entity.FloorName = NormalizeNullableString(request.FloorName);
                entity.RoomName = NormalizeNullableString(request.RoomName);
                entity.IsAvailableForRegistrationQueue = request.IsAvailableForRegistrationQueue;
                entity.IsAvailableForScreening = request.IsAvailableForScreening;
                entity.IsAvailableForDisplay = request.IsAvailableForDisplay;
                entity.IsDefault = request.IsActive ? request.IsDefault : false;
                entity.SortOrder = request.SortOrder;
                entity.Description = NormalizeNullableString(request.Description);
                entity.IsActive = request.IsActive;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(ApiResponse<object>.Ok(null, "Nurse station cluster berhasil diperbarui."));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggerService.ErrorAsync(LogCategory, "NurseStationCluster.Update", "Gagal mengubah data nurse station cluster.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail(StatusCodes.Status500InternalServerError, "Terjadi kesalahan saat memperbarui nurse station cluster."));
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Nurse Station Cluster Status", Description = "Mengubah status nurse station cluster", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("NurseStationCluster", "Update")]
        public async Task<IActionResult> UpdateNurseStationClusterStatus(Guid id, [FromBody] UpdateNurseStationClusterStatusRequest request)
        {
            var entity = await _dbContext.Set<MstNurseStationCluster>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Nurse station cluster tidak ditemukan."));
            entity.IsActive = request.IsActive;
            if (!request.IsActive) entity.IsDefault = false;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status nurse station cluster berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Delete", "Delete Nurse Station Cluster", Description = "Menghapus data nurse station cluster", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("NurseStationCluster", "Delete")]
        public async Task<IActionResult> DeleteNurseStationCluster(Guid id, [FromBody] DeleteNurseStationClusterRequest? request = null)
        {
            var entity = await _dbContext.Set<MstNurseStationCluster>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Nurse station cluster tidak ditemukan."));
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsDefault = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            if (!string.IsNullOrWhiteSpace(request?.DeleteReason)) entity.Description = request.DeleteReason.Trim();
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Nurse station cluster berhasil dihapus."));
        }

        private IQueryable<MstNurseStationCluster> BuildBaseQuery()
        {
            return _dbContext.Set<MstNurseStationCluster>().AsNoTracking().Include(x => x.ServiceUnit).Where(x => !x.IsDelete);
        }

        private static IQueryable<MstNurseStationCluster> ApplyFilter(IQueryable<MstNurseStationCluster> query, Guid? serviceUnitId, bool? isActive, bool? isDefault, string? search)
        {
            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty) query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (isDefault.HasValue) query = query.Where(x => x.IsDefault == isDefault.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x => x.ClusterCode.ToLower().Contains(keyword) || x.ClusterName.ToLower().Contains(keyword) || (x.ShortName != null && x.ShortName.ToLower().Contains(keyword)) || (x.LocationName != null && x.LocationName.ToLower().Contains(keyword)) || (x.FloorName != null && x.FloorName.ToLower().Contains(keyword)) || (x.RoomName != null && x.RoomName.ToLower().Contains(keyword)) || (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IQueryable<MstNurseStationCluster> ApplyDateFilter(IQueryable<MstNurseStationCluster> query, DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            if (startDate.HasValue) query = query.Where(x => x.CreateDateTime >= DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc));
            if (endDate.HasValue) query = query.Where(x => x.CreateDateTime < DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc));
            if (!startDate.HasValue && !endDate.HasValue && !string.IsNullOrWhiteSpace(customPeriod))
            {
                var today = AppDateTimeHelper.OperationalDate();
                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today": query = query.Where(x => x.CreateDateTime >= today && x.CreateDateTime < today.AddDays(1)); break;
                    case "last7days": query = query.Where(x => x.CreateDateTime >= today.AddDays(-6) && x.CreateDateTime < today.AddDays(1)); break;
                    case "thismonth": var s = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc); query = query.Where(x => x.CreateDateTime >= s && x.CreateDateTime < s.AddMonths(1)); break;
                    case "lastmonth": var c = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc); var l = c.AddMonths(-1); query = query.Where(x => x.CreateDateTime >= l && x.CreateDateTime < c); break;
                }
            }
            return query;
        }

        private static IOrderedQueryable<MstNurseStationCluster> ApplySorting(IQueryable<MstNurseStationCluster> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "clustercode" => desc ? query.OrderByDescending(x => x.ClusterCode) : query.OrderBy(x => x.ClusterCode),
                "clustername" => desc ? query.OrderByDescending(x => x.ClusterName) : query.OrderBy(x => x.ClusterName),
                "shortname" => desc ? query.OrderByDescending(x => x.ShortName) : query.OrderBy(x => x.ShortName),
                "locationname" => desc ? query.OrderByDescending(x => x.LocationName) : query.OrderBy(x => x.LocationName),
                "floorname" => desc ? query.OrderByDescending(x => x.FloorName) : query.OrderBy(x => x.FloorName),
                "isdefault" => desc ? query.OrderByDescending(x => x.IsDefault).ThenBy(x => x.ClusterName) : query.OrderBy(x => x.IsDefault).ThenBy(x => x.ClusterName),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.ClusterName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.ClusterName),
                _ => desc ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.ClusterName) : query.OrderBy(x => x.SortOrder).ThenBy(x => x.ClusterName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateNurseStationClusterRequest request)
        {
            if (request.ServiceUnitId == Guid.Empty) return (false, "Unit layanan wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.ClusterName)) return (false, "Nama cluster wajib diisi.");
            if (!await _dbContext.Set<QuilvianSystemBackend.Areas.HealthServices.MasterData.Models.MstServiceUnit>().AsNoTracking().AnyAsync(x => x.Id == request.ServiceUnitId && !x.IsDelete)) return (false, "Unit layanan tidak ditemukan.");
            var normalizedName = request.ClusterName.Trim().ToLower();
            var duplicateQuery = _dbContext.Set<MstNurseStationCluster>().AsNoTracking().Where(x => !x.IsDelete && x.ServiceUnitId == request.ServiceUnitId && x.ClusterName.ToLower() == normalizedName);
            if (excludeId.HasValue) duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            if (await duplicateQuery.AnyAsync()) return (false, "Nama cluster sudah digunakan pada unit layanan tersebut.");
            return (true, null);
        }

        private async Task UnsetOtherDefaultAsync(Guid? exceptId, DateTime now, Guid actorUserId)
        {
            var query = _dbContext.Set<MstNurseStationCluster>().Where(x => x.IsDefault && !x.IsDelete);
            if (exceptId.HasValue) query = query.Where(x => x.Id != exceptId.Value);
            var entities = await query.ToListAsync();
            foreach (var entity in entities)
            {
                entity.IsDefault = false;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;
            }
        }

        private async Task<string> GenerateCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstNurseStationCluster>().IgnoreQueryFilters().AsNoTracking().Where(x => x.ClusterCode.StartsWith(CodePrefix)).Select(x => x.ClusterCode).ToListAsync();
            var usedNumbers = existingCodes.Select(x => x.Replace(CodePrefix, string.Empty)).Where(x => int.TryParse(x, out _)).Select(int.Parse).Where(x => x > 0).ToHashSet();
            var nextNumber = 1;
            while (usedNumbers.Contains(nextNumber)) nextNumber++;
            return CodePrefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
        }

        private static NurseStationClusterResponse MapResponse(MstNurseStationCluster entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new NurseStationClusterResponse
            {
                Id = entity.Id,
                ServiceUnitId = entity.ServiceUnitId,
                ServiceUnitCode = entity.ServiceUnit?.ServiceUnitCode,
                ServiceUnitName = entity.ServiceUnit?.ServiceUnitName,
                ClusterCode = entity.ClusterCode,
                ClusterName = entity.ClusterName,
                ShortName = entity.ShortName,
                LocationName = entity.LocationName,
                FloorName = entity.FloorName,
                RoomName = entity.RoomName,
                IsAvailableForRegistrationQueue = entity.IsAvailableForRegistrationQueue,
                IsAvailableForScreening = entity.IsAvailableForScreening,
                IsAvailableForDisplay = entity.IsAvailableForDisplay,
                IsDefault = entity.IsDefault,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static NurseStationClusterDetailResponse MapDetailResponse(MstNurseStationCluster entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var data = new NurseStationClusterDetailResponse();
            var baseData = MapResponse(entity, actorNames);
            foreach (var prop in typeof(NurseStationClusterResponse).GetProperties()) prop.SetValue(data, prop.GetValue(baseData));
            data.Description = entity.Description;
            data.UpdateDateTime = entity.UpdateDateTime;
            data.UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy;
            data.UpdateByName = GetActorName(actorNames, entity.UpdateBy);
            return data;
        }

        private static NurseStationClusterOptionResponse MapOptionResponse(MstNurseStationCluster entity)
        {
            return new NurseStationClusterOptionResponse { Id = entity.Id, ServiceUnitId = entity.ServiceUnitId, ServiceUnitCode = entity.ServiceUnit?.ServiceUnitCode, ServiceUnitName = entity.ServiceUnit?.ServiceUnitName, ClusterCode = entity.ClusterCode, ClusterName = entity.ClusterName, ShortName = entity.ShortName, IsDefault = entity.IsDefault, SortOrder = entity.SortOrder };
        }

        private static List<NurseStationClusterCustomPeriodOptionResponse> BuildCustomPeriods()
        {
            return new List<NurseStationClusterCustomPeriodOptionResponse> { new() { Value = "today", Label = "Hari ini" }, new() { Value = "last7days", Label = "7 hari terakhir" }, new() { Value = "thismonth", Label = "Bulan ini" }, new() { Value = "lastmonth", Label = "Bulan lalu" } };
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> actorIds)
        {
            var ids = actorIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (!ids.Any()) return new Dictionary<Guid, string?>();
            return await _dbContext.Users.AsNoTracking().Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode }).ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> actorNames, Guid actorId) => actorId == Guid.Empty ? null : actorNames.TryGetValue(actorId, out var actorName) ? actorName : null;
        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize) { if (pageNumber < 1) pageNumber = 1; if (pageSize < 1) pageSize = 25; if (pageSize > 100) pageSize = 100; return (pageNumber, pageSize); }
        private static string? NormalizeNullableString(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private Guid GetCurrentUserId() { var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id"); return Guid.TryParse(userIdValue, out var userId) ? userId : Guid.Empty; }
    }
}
