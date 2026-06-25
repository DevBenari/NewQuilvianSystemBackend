using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseNurseStationClusterStaffPagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs.NurseStationClusterStaffResponse>;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/administrator/master-data/nurse-station-cluster-staffs")]
    [AccessController(moduleCode: "ADMINISTRATOR_MASTER_DATA", moduleName: "Administrator Master Data", displayName: "Nurse Station Cluster Staff", AreaName = "Administrator", ControllerName = "NurseStationClusterStaff", Description = "Administrator master data nurse station cluster staff", SortOrder = 103)]
    [Tags("Administrator / Master Data / Nurse Station Cluster Staff")]
    public class NurseStationClusterStaffController : ControllerBase
    {
        private const string LogCategory = "Administrator.MasterData";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        public NurseStationClusterStaffController(ApplicationDbContext dbContext, LoggerService loggerService) { _dbContext = dbContext; _loggerService = loggerService; }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<NurseStationClusterStaffFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nurse Station Cluster Staff", Description = "Melihat metadata filter staff cluster", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationClusterStaff", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new NurseStationClusterStaffFilterMetadataResponse
            {
                SortOptions = new List<NurseStationClusterStaffSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" }, new() { Value = "createDateTime", Label = "Tanggal dibuat" }, new() { Value = "clusterName", Label = "Nama cluster" }, new() { Value = "employeeName", Label = "Nama pegawai" }, new() { Value = "isPrimary", Label = "Primary" }, new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
            return Ok(ApiResponse<NurseStationClusterStaffFilterMetadataResponse>.Ok(result, "Metadata filter staff cluster berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<NurseStationClusterStaffSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nurse Station Cluster Staff", Description = "Melihat ringkasan staff cluster", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationClusterStaff", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var q = BuildBaseQuery();
            var result = new NurseStationClusterStaffSummaryResponse { TotalStaff = await q.CountAsync(), ActiveStaff = await q.CountAsync(x => x.IsActive), InactiveStaff = await q.CountAsync(x => !x.IsActive), PrimaryStaff = await q.CountAsync(x => x.IsPrimary), CanCallQueueStaff = await q.CountAsync(x => x.CanCallQueue), CanStartScreeningStaff = await q.CountAsync(x => x.CanStartScreening), CanTransferQueueStaff = await q.CountAsync(x => x.CanTransferQueue) };
            return Ok(ApiResponse<NurseStationClusterStaffSummaryResponse>.Ok(result, "Ringkasan staff cluster berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseNurseStationClusterStaffPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nurse Station Cluster Staff", Description = "Melihat data staff cluster", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationClusterStaff", "Read")]
        public async Task<IActionResult> GetNurseStationClusterStaffs([FromQuery] Guid? nurseStationClusterId, [FromQuery] Guid? employeeId, [FromQuery] Guid? workforceProfileId, [FromQuery] bool? isActive, [FromQuery] bool? isPrimary, [FromQuery] string? search, [FromQuery] string? sortBy = "sortOrder", [FromQuery] string? sortDirection = "asc", [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize); pageNumber = paging.PageNumber; pageSize = paging.PageSize;
            var query = ApplyFilter(BuildBaseQuery(), nurseStationClusterId, employeeId, workforceProfileId, isActive, isPrimary, search);
            var totalData = await query.CountAsync();
            var entities = await ApplySorting(query, sortBy, sortDirection).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var actorNames = await GetActorNameMapAsync(entities.Select(x => x.CreateBy).Where(x => x != Guid.Empty));
            var result = new ResponseNurseStationClusterStaffPagedResult { PageNumber = pageNumber, PageSize = pageSize, TotalData = totalData, TotalPage = (int)Math.Ceiling(totalData / (double)pageSize), Items = entities.Select(x => MapResponse(x, actorNames)).ToList() };
            return Ok(ApiResponse<ResponseNurseStationClusterStaffPagedResult>.Ok(result, "Data staff cluster berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<NurseStationClusterStaffOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nurse Station Cluster Staff", Description = "Melihat data pilihan staff cluster", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationClusterStaff", "Read")]
        public async Task<IActionResult> GetNurseStationClusterStaffOptions([FromQuery] Guid? nurseStationClusterId, [FromQuery] bool onlyActive = true, [FromQuery] string? search = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize); pageNumber = paging.PageNumber; pageSize = paging.PageSize;
            var query = ApplyFilter(BuildBaseQuery(), nurseStationClusterId, null, null, onlyActive ? true : null, null, search);
            var totalData = await query.CountAsync();
            var items = await query.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.SortOrder).ThenBy(x => x.Employee!.FullName).Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(x => MapOptionResponse(x)).ToListAsync();
            return Ok(ApiResponse<NurseStationClusterStaffOptionPagedResponse>.Ok(new NurseStationClusterStaffOptionPagedResponse { PageNumber = pageNumber, PageSize = pageSize, TotalData = totalData, TotalPage = (int)Math.Ceiling(totalData / (double)pageSize), Items = items }, "Data pilihan staff cluster berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<NurseStationClusterStaffDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nurse Station Cluster Staff", Description = "Melihat detail staff cluster", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationClusterStaff", "Read")]
        public async Task<IActionResult> GetNurseStationClusterStaffById(Guid id)
        {
            var entity = await BuildBaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Staff cluster tidak ditemukan."));
            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            return Ok(ApiResponse<NurseStationClusterStaffDetailResponse>.Ok(MapDetailResponse(entity, actorNames), "Detail staff cluster berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<NurseStationClusterStaffCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Nurse Station Cluster Staff", Description = "Membuat staff cluster", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("NurseStationClusterStaff", "Create")]
        public async Task<IActionResult> CreateNurseStationClusterStaff([FromBody] CreateNurseStationClusterStaffRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data staff cluster tidak valid."));
            var employee = await _dbContext.Set<MstEmployee>().AsNoTracking().FirstAsync(x => x.Id == request.EmployeeId);
            var workforceProfileId = request.WorkforceProfileId.HasValue && request.WorkforceProfileId.Value != Guid.Empty ? request.WorkforceProfileId.Value : employee.WorkforceProfileId;
            var now = DateTime.UtcNow; var actorUserId = GetCurrentUserId();
            var entity = new MstNurseStationClusterStaff { Id = Guid.NewGuid(), NurseStationClusterId = request.NurseStationClusterId, EmployeeId = request.EmployeeId, WorkforceProfileId = workforceProfileId, IsPrimary = request.IsPrimary, CanCallQueue = request.CanCallQueue, CanStartScreening = request.CanStartScreening, CanTransferQueue = request.CanTransferQueue, SortOrder = request.SortOrder, Description = NormalizeNullableString(request.Description), IsActive = true, CreateDateTime = now, CreateBy = actorUserId, IsDelete = false, IsCancel = false };
            _dbContext.Set<MstNurseStationClusterStaff>().Add(entity); await _dbContext.SaveChangesAsync();
            var result = new NurseStationClusterStaffCreateResponse { Id = entity.Id, NurseStationClusterId = entity.NurseStationClusterId, EmployeeId = entity.EmployeeId, WorkforceProfileId = entity.WorkforceProfileId, IsActive = entity.IsActive };
            await _loggerService.InfoAsync(LogCategory, "NurseStationClusterStaff.Create", "Membuat staff cluster.", result);
            return Ok(ApiResponse<NurseStationClusterStaffCreateResponse>.Ok(result, "Staff cluster berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Nurse Station Cluster Staff", Description = "Mengubah staff cluster", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("NurseStationClusterStaff", "Update")]
        public async Task<IActionResult> UpdateNurseStationClusterStaff(Guid id, [FromBody] UpdateNurseStationClusterStaffRequest request)
        {
            var entity = await _dbContext.Set<MstNurseStationClusterStaff>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Staff cluster tidak ditemukan."));
            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data staff cluster tidak valid."));
            var employee = await _dbContext.Set<MstEmployee>().AsNoTracking().FirstAsync(x => x.Id == request.EmployeeId);
            entity.NurseStationClusterId = request.NurseStationClusterId; entity.EmployeeId = request.EmployeeId; entity.WorkforceProfileId = request.WorkforceProfileId.HasValue && request.WorkforceProfileId.Value != Guid.Empty ? request.WorkforceProfileId.Value : employee.WorkforceProfileId; entity.IsPrimary = request.IsPrimary; entity.CanCallQueue = request.CanCallQueue; entity.CanStartScreening = request.CanStartScreening; entity.CanTransferQueue = request.CanTransferQueue; entity.SortOrder = request.SortOrder; entity.Description = NormalizeNullableString(request.Description); entity.IsActive = request.IsActive; entity.UpdateDateTime = DateTime.UtcNow; entity.UpdateBy = GetCurrentUserId(); await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Staff cluster berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Nurse Station Cluster Staff Status", Description = "Mengubah status staff cluster", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("NurseStationClusterStaff", "Update")]
        public async Task<IActionResult> UpdateNurseStationClusterStaffStatus(Guid id, [FromBody] UpdateNurseStationClusterStaffStatusRequest request)
        {
            var entity = await _dbContext.Set<MstNurseStationClusterStaff>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Staff cluster tidak ditemukan."));
            entity.IsActive = request.IsActive; entity.UpdateDateTime = DateTime.UtcNow; entity.UpdateBy = GetCurrentUserId(); await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status staff cluster berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Delete", "Delete Nurse Station Cluster Staff", Description = "Menghapus staff cluster", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("NurseStationClusterStaff", "Delete")]
        public async Task<IActionResult> DeleteNurseStationClusterStaff(Guid id, [FromBody] DeleteNurseStationClusterStaffRequest? request = null)
        {
            var entity = await _dbContext.Set<MstNurseStationClusterStaff>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Staff cluster tidak ditemukan."));
            var now = DateTime.UtcNow; var actorUserId = GetCurrentUserId(); entity.IsDelete = true; entity.IsActive = false; entity.DeleteDateTime = now; entity.DeleteBy = actorUserId; entity.UpdateDateTime = now; entity.UpdateBy = actorUserId; if (!string.IsNullOrWhiteSpace(request?.DeleteReason)) entity.Description = request.DeleteReason.Trim(); await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Staff cluster berhasil dihapus."));
        }

        private IQueryable<MstNurseStationClusterStaff> BuildBaseQuery() => _dbContext.Set<MstNurseStationClusterStaff>().AsNoTracking().Include(x => x.NurseStationCluster).Include(x => x.Employee).Include(x => x.WorkforceProfile).Where(x => !x.IsDelete);
        private static IQueryable<MstNurseStationClusterStaff> ApplyFilter(IQueryable<MstNurseStationClusterStaff> q, Guid? clusterId, Guid? employeeId, Guid? workforceProfileId, bool? isActive, bool? isPrimary, string? search) { if (clusterId.HasValue && clusterId.Value != Guid.Empty) q = q.Where(x => x.NurseStationClusterId == clusterId.Value); if (employeeId.HasValue && employeeId.Value != Guid.Empty) q = q.Where(x => x.EmployeeId == employeeId.Value); if (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty) q = q.Where(x => x.WorkforceProfileId == workforceProfileId.Value); if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value); if (isPrimary.HasValue) q = q.Where(x => x.IsPrimary == isPrimary.Value); if (!string.IsNullOrWhiteSpace(search)) { var k = search.Trim().ToLower(); q = q.Where(x => x.NurseStationCluster!.ClusterName.ToLower().Contains(k) || x.NurseStationCluster.ClusterCode.ToLower().Contains(k) || x.Employee!.FullName.ToLower().Contains(k) || x.Employee.EmployeeCode.ToLower().Contains(k) || x.Employee.EmployeeNumber.ToLower().Contains(k) || (x.Description != null && x.Description.ToLower().Contains(k))); } return q; }
        private static IOrderedQueryable<MstNurseStationClusterStaff> ApplySorting(IQueryable<MstNurseStationClusterStaff> q, string? sortBy, string? dir) { var d = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase); return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch { "createdatetime" => d ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime), "clustername" => d ? q.OrderByDescending(x => x.NurseStationCluster!.ClusterName) : q.OrderBy(x => x.NurseStationCluster!.ClusterName), "employeename" => d ? q.OrderByDescending(x => x.Employee!.FullName) : q.OrderBy(x => x.Employee!.FullName), "isprimary" => d ? q.OrderByDescending(x => x.IsPrimary) : q.OrderBy(x => x.IsPrimary), "isactive" => d ? q.OrderByDescending(x => x.IsActive) : q.OrderBy(x => x.IsActive), _ => d ? q.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.Employee!.FullName) : q.OrderBy(x => x.SortOrder).ThenBy(x => x.Employee!.FullName) }; }
        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateNurseStationClusterStaffRequest r) { if (r.NurseStationClusterId == Guid.Empty) return (false, "Cluster wajib diisi."); if (r.EmployeeId == Guid.Empty) return (false, "Pegawai wajib diisi."); if (!await _dbContext.Set<MstNurseStationCluster>().AnyAsync(x => x.Id == r.NurseStationClusterId && !x.IsDelete)) return (false, "Cluster tidak ditemukan."); var emp = await _dbContext.Set<MstEmployee>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == r.EmployeeId && !x.IsDelete); if (emp == null) return (false, "Pegawai tidak ditemukan."); if (r.WorkforceProfileId.HasValue && r.WorkforceProfileId.Value != Guid.Empty && r.WorkforceProfileId.Value != emp.WorkforceProfileId) return (false, "Workforce profile tidak sesuai dengan pegawai."); var dup = _dbContext.Set<MstNurseStationClusterStaff>().Where(x => !x.IsDelete && x.NurseStationClusterId == r.NurseStationClusterId && x.EmployeeId == r.EmployeeId); if (excludeId.HasValue) dup = dup.Where(x => x.Id != excludeId.Value); if (await dup.AnyAsync()) return (false, "Pegawai sudah terdaftar pada cluster tersebut."); return (true, null); }
        private static NurseStationClusterStaffResponse MapResponse(MstNurseStationClusterStaff e, IReadOnlyDictionary<Guid, string?> names) => new() { Id = e.Id, NurseStationClusterId = e.NurseStationClusterId, ClusterCode = e.NurseStationCluster?.ClusterCode, ClusterName = e.NurseStationCluster?.ClusterName, EmployeeId = e.EmployeeId, EmployeeCode = e.Employee?.EmployeeCode, EmployeeNumber = e.Employee?.EmployeeNumber, EmployeeName = e.Employee?.FullName, WorkforceProfileId = e.WorkforceProfileId, WorkforceProfileCode = e.WorkforceProfile?.ProfileCode, WorkforceProfileName = e.WorkforceProfile?.DisplayName, IsPrimary = e.IsPrimary, CanCallQueue = e.CanCallQueue, CanStartScreening = e.CanStartScreening, CanTransferQueue = e.CanTransferQueue, SortOrder = e.SortOrder, IsActive = e.IsActive, CreateDateTime = e.CreateDateTime, CreateBy = e.CreateBy == Guid.Empty ? null : e.CreateBy, CreateByName = GetActorName(names, e.CreateBy) };
        private static NurseStationClusterStaffDetailResponse MapDetailResponse(MstNurseStationClusterStaff e, IReadOnlyDictionary<Guid, string?> names) { var d = new NurseStationClusterStaffDetailResponse(); var b = MapResponse(e, names); foreach (var p in typeof(NurseStationClusterStaffResponse).GetProperties()) p.SetValue(d, p.GetValue(b)); d.Description = e.Description; d.UpdateDateTime = e.UpdateDateTime; d.UpdateBy = e.UpdateBy == Guid.Empty ? null : e.UpdateBy; d.UpdateByName = GetActorName(names, e.UpdateBy); return d; }
        private static NurseStationClusterStaffOptionResponse MapOptionResponse(MstNurseStationClusterStaff e) => new() { Id = e.Id, NurseStationClusterId = e.NurseStationClusterId, ClusterName = e.NurseStationCluster?.ClusterName, EmployeeId = e.EmployeeId, EmployeeCode = e.Employee?.EmployeeCode, EmployeeName = e.Employee?.FullName, IsPrimary = e.IsPrimary, SortOrder = e.SortOrder };
        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> ids) { var list = ids.Where(x => x != Guid.Empty).Distinct().ToList(); if (!list.Any()) return new(); return await _dbContext.Users.AsNoTracking().Where(x => list.Contains(x.Id)).Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode }).ToDictionaryAsync(x => x.Id, x => x.Name); }
        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> names, Guid id) => id == Guid.Empty ? null : names.TryGetValue(id, out var n) ? n : null;
        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize) { if (pageNumber < 1) pageNumber = 1; if (pageSize < 1) pageSize = 25; if (pageSize > 100) pageSize = 100; return (pageNumber, pageSize); }
        private static string? NormalizeNullableString(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private Guid GetCurrentUserId() { var v = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id"); return Guid.TryParse(v, out var id) ? id : Guid.Empty; }
    }
}
