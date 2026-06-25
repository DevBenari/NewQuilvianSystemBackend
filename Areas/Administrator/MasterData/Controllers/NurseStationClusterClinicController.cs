using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseNurseStationClusterClinicPagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs.NurseStationClusterClinicResponse>;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/administrator/master-data/nurse-station-cluster-clinics")]
    [AccessController(moduleCode: "ADMINISTRATOR_MASTER_DATA", moduleName: "Administrator Master Data", displayName: "Nurse Station Cluster Clinic", AreaName = "Administrator", ControllerName = "NurseStationClusterClinic", Description = "Administrator master data nurse station cluster clinic", SortOrder = 102)]
    [Tags("Administrator / Master Data / Nurse Station Cluster Clinic")]
    public class NurseStationClusterClinicController : ControllerBase
    {
        private const string LogCategory = "Administrator.MasterData";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        public NurseStationClusterClinicController(ApplicationDbContext dbContext, LoggerService loggerService) { _dbContext = dbContext; _loggerService = loggerService; }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<NurseStationClusterClinicFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nurse Station Cluster Clinic", Description = "Melihat metadata filter nurse station cluster clinic", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationClusterClinic", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new NurseStationClusterClinicFilterMetadataResponse
            {
                SortOptions = new List<NurseStationClusterClinicSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" }, new() { Value = "createDateTime", Label = "Tanggal dibuat" }, new() { Value = "clusterName", Label = "Nama cluster" }, new() { Value = "clinicName", Label = "Nama poli" }, new() { Value = "isPrimary", Label = "Primary" }, new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
            return Ok(ApiResponse<NurseStationClusterClinicFilterMetadataResponse>.Ok(result, "Metadata filter mapping poli cluster berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<NurseStationClusterClinicSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nurse Station Cluster Clinic", Description = "Melihat ringkasan mapping poli cluster", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationClusterClinic", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var q = BuildBaseQuery();
            var result = new NurseStationClusterClinicSummaryResponse { TotalMapping = await q.CountAsync(), ActiveMapping = await q.CountAsync(x => x.IsActive), InactiveMapping = await q.CountAsync(x => !x.IsActive), PrimaryMapping = await q.CountAsync(x => x.IsPrimary) };
            return Ok(ApiResponse<NurseStationClusterClinicSummaryResponse>.Ok(result, "Ringkasan mapping poli cluster berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseNurseStationClusterClinicPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nurse Station Cluster Clinic", Description = "Melihat data mapping poli cluster", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationClusterClinic", "Read")]
        public async Task<IActionResult> GetNurseStationClusterClinics([FromQuery] Guid? nurseStationClusterId, [FromQuery] Guid? clinicId, [FromQuery] bool? isActive, [FromQuery] bool? isPrimary, [FromQuery] string? search, [FromQuery] string? sortBy = "sortOrder", [FromQuery] string? sortDirection = "asc", [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize); pageNumber = paging.PageNumber; pageSize = paging.PageSize;
            var query = ApplyFilter(BuildBaseQuery(), nurseStationClusterId, clinicId, isActive, isPrimary, search);
            var totalData = await query.CountAsync();
            var entities = await ApplySorting(query, sortBy, sortDirection).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var actorNames = await GetActorNameMapAsync(entities.Select(x => x.CreateBy).Where(x => x != Guid.Empty));
            var result = new ResponseNurseStationClusterClinicPagedResult { PageNumber = pageNumber, PageSize = pageSize, TotalData = totalData, TotalPage = (int)Math.Ceiling(totalData / (double)pageSize), Items = entities.Select(x => MapResponse(x, actorNames)).ToList() };
            return Ok(ApiResponse<ResponseNurseStationClusterClinicPagedResult>.Ok(result, "Data mapping poli cluster berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<NurseStationClusterClinicOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nurse Station Cluster Clinic", Description = "Melihat data pilihan mapping poli cluster", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationClusterClinic", "Read")]
        public async Task<IActionResult> GetNurseStationClusterClinicOptions([FromQuery] Guid? nurseStationClusterId, [FromQuery] bool onlyActive = true, [FromQuery] string? search = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize); pageNumber = paging.PageNumber; pageSize = paging.PageSize;
            var query = ApplyFilter(BuildBaseQuery(), nurseStationClusterId, null, onlyActive ? true : null, null, search);
            var totalData = await query.CountAsync();
            var items = await query.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.SortOrder).ThenBy(x => x.Clinic!.ClinicName).Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(x => MapOptionResponse(x)).ToListAsync();
            return Ok(ApiResponse<NurseStationClusterClinicOptionPagedResponse>.Ok(new NurseStationClusterClinicOptionPagedResponse { PageNumber = pageNumber, PageSize = pageSize, TotalData = totalData, TotalPage = (int)Math.Ceiling(totalData / (double)pageSize), Items = items }, "Data pilihan mapping poli cluster berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<NurseStationClusterClinicDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nurse Station Cluster Clinic", Description = "Melihat detail mapping poli cluster", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationClusterClinic", "Read")]
        public async Task<IActionResult> GetNurseStationClusterClinicById(Guid id)
        {
            var entity = await BuildBaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Mapping poli cluster tidak ditemukan."));
            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            return Ok(ApiResponse<NurseStationClusterClinicDetailResponse>.Ok(MapDetailResponse(entity, actorNames), "Detail mapping poli cluster berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<NurseStationClusterClinicCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Nurse Station Cluster Clinic", Description = "Membuat mapping poli cluster", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("NurseStationClusterClinic", "Create")]
        public async Task<IActionResult> CreateNurseStationClusterClinic([FromBody] CreateNurseStationClusterClinicRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data mapping poli cluster tidak valid."));
            var now = DateTime.UtcNow; var actorUserId = GetCurrentUserId();
            var entity = new MstNurseStationClusterClinic { Id = Guid.NewGuid(), NurseStationClusterId = request.NurseStationClusterId, ClinicId = request.ClinicId, IsPrimary = request.IsPrimary, SortOrder = request.SortOrder, Description = NormalizeNullableString(request.Description), IsActive = true, CreateDateTime = now, CreateBy = actorUserId, IsDelete = false, IsCancel = false };
            _dbContext.Set<MstNurseStationClusterClinic>().Add(entity); await _dbContext.SaveChangesAsync();
            var result = new NurseStationClusterClinicCreateResponse { Id = entity.Id, NurseStationClusterId = entity.NurseStationClusterId, ClinicId = entity.ClinicId, IsActive = entity.IsActive };
            await _loggerService.InfoAsync(LogCategory, "NurseStationClusterClinic.Create", "Membuat mapping poli cluster.", result);
            return Ok(ApiResponse<NurseStationClusterClinicCreateResponse>.Ok(result, "Mapping poli cluster berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Nurse Station Cluster Clinic", Description = "Mengubah mapping poli cluster", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("NurseStationClusterClinic", "Update")]
        public async Task<IActionResult> UpdateNurseStationClusterClinic(Guid id, [FromBody] UpdateNurseStationClusterClinicRequest request)
        {
            var entity = await _dbContext.Set<MstNurseStationClusterClinic>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Mapping poli cluster tidak ditemukan."));
            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data mapping poli cluster tidak valid."));
            entity.NurseStationClusterId = request.NurseStationClusterId; entity.ClinicId = request.ClinicId; entity.IsPrimary = request.IsPrimary; entity.SortOrder = request.SortOrder; entity.Description = NormalizeNullableString(request.Description); entity.IsActive = request.IsActive; entity.UpdateDateTime = DateTime.UtcNow; entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Mapping poli cluster berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Nurse Station Cluster Clinic Status", Description = "Mengubah status mapping poli cluster", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("NurseStationClusterClinic", "Update")]
        public async Task<IActionResult> UpdateNurseStationClusterClinicStatus(Guid id, [FromBody] UpdateNurseStationClusterClinicStatusRequest request)
        {
            var entity = await _dbContext.Set<MstNurseStationClusterClinic>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Mapping poli cluster tidak ditemukan."));
            entity.IsActive = request.IsActive; entity.UpdateDateTime = DateTime.UtcNow; entity.UpdateBy = GetCurrentUserId(); await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status mapping poli cluster berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Delete", "Delete Nurse Station Cluster Clinic", Description = "Menghapus mapping poli cluster", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("NurseStationClusterClinic", "Delete")]
        public async Task<IActionResult> DeleteNurseStationClusterClinic(Guid id, [FromBody] DeleteNurseStationClusterClinicRequest? request = null)
        {
            var entity = await _dbContext.Set<MstNurseStationClusterClinic>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Mapping poli cluster tidak ditemukan."));
            var now = DateTime.UtcNow; var actorUserId = GetCurrentUserId(); entity.IsDelete = true; entity.IsActive = false; entity.DeleteDateTime = now; entity.DeleteBy = actorUserId; entity.UpdateDateTime = now; entity.UpdateBy = actorUserId; if (!string.IsNullOrWhiteSpace(request?.DeleteReason)) entity.Description = request.DeleteReason.Trim(); await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Mapping poli cluster berhasil dihapus."));
        }

        private IQueryable<MstNurseStationClusterClinic> BuildBaseQuery() => _dbContext.Set<MstNurseStationClusterClinic>().AsNoTracking().Include(x => x.NurseStationCluster).Include(x => x.Clinic).Where(x => !x.IsDelete);
        private static IQueryable<MstNurseStationClusterClinic> ApplyFilter(IQueryable<MstNurseStationClusterClinic> q, Guid? clusterId, Guid? clinicId, bool? isActive, bool? isPrimary, string? search) { if (clusterId.HasValue && clusterId.Value != Guid.Empty) q = q.Where(x => x.NurseStationClusterId == clusterId.Value); if (clinicId.HasValue && clinicId.Value != Guid.Empty) q = q.Where(x => x.ClinicId == clinicId.Value); if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value); if (isPrimary.HasValue) q = q.Where(x => x.IsPrimary == isPrimary.Value); if (!string.IsNullOrWhiteSpace(search)) { var k = search.Trim().ToLower(); q = q.Where(x => x.NurseStationCluster!.ClusterName.ToLower().Contains(k) || x.NurseStationCluster.ClusterCode.ToLower().Contains(k) || x.Clinic!.ClinicName.ToLower().Contains(k) || x.Clinic.ClinicCode.ToLower().Contains(k) || (x.Description != null && x.Description.ToLower().Contains(k))); } return q; }
        private static IOrderedQueryable<MstNurseStationClusterClinic> ApplySorting(IQueryable<MstNurseStationClusterClinic> q, string? sortBy, string? dir) { var d = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase); return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch { "createdatetime" => d ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime), "clustername" => d ? q.OrderByDescending(x => x.NurseStationCluster!.ClusterName) : q.OrderBy(x => x.NurseStationCluster!.ClusterName), "clinicname" => d ? q.OrderByDescending(x => x.Clinic!.ClinicName) : q.OrderBy(x => x.Clinic!.ClinicName), "isprimary" => d ? q.OrderByDescending(x => x.IsPrimary) : q.OrderBy(x => x.IsPrimary), "isactive" => d ? q.OrderByDescending(x => x.IsActive) : q.OrderBy(x => x.IsActive), _ => d ? q.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.Clinic!.ClinicName) : q.OrderBy(x => x.SortOrder).ThenBy(x => x.Clinic!.ClinicName) }; }
        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateNurseStationClusterClinicRequest r) { if (r.NurseStationClusterId == Guid.Empty) return (false, "Cluster wajib diisi."); if (r.ClinicId == Guid.Empty) return (false, "Poli wajib diisi."); if (!await _dbContext.Set<MstNurseStationCluster>().AnyAsync(x => x.Id == r.NurseStationClusterId && !x.IsDelete)) return (false, "Cluster tidak ditemukan."); if (!await _dbContext.Set<QuilvianSystemBackend.Areas.HealthServices.MasterData.Models.MstClinic>().AnyAsync(x => x.Id == r.ClinicId && !x.IsDelete)) return (false, "Poli tidak ditemukan."); var dup = _dbContext.Set<MstNurseStationClusterClinic>().Where(x => !x.IsDelete && x.NurseStationClusterId == r.NurseStationClusterId && x.ClinicId == r.ClinicId); if (excludeId.HasValue) dup = dup.Where(x => x.Id != excludeId.Value); if (await dup.AnyAsync()) return (false, "Poli sudah terdaftar pada cluster tersebut."); return (true, null); }
        private static NurseStationClusterClinicResponse MapResponse(MstNurseStationClusterClinic e, IReadOnlyDictionary<Guid, string?> names) => new() { Id = e.Id, NurseStationClusterId = e.NurseStationClusterId, ClusterCode = e.NurseStationCluster?.ClusterCode, ClusterName = e.NurseStationCluster?.ClusterName, ClinicId = e.ClinicId, ClinicCode = e.Clinic?.ClinicCode, ClinicName = e.Clinic?.ClinicName, IsPrimary = e.IsPrimary, SortOrder = e.SortOrder, IsActive = e.IsActive, CreateDateTime = e.CreateDateTime, CreateBy = e.CreateBy == Guid.Empty ? null : e.CreateBy, CreateByName = GetActorName(names, e.CreateBy) };
        private static NurseStationClusterClinicDetailResponse MapDetailResponse(MstNurseStationClusterClinic e, IReadOnlyDictionary<Guid, string?> names) { var d = new NurseStationClusterClinicDetailResponse(); var b = MapResponse(e, names); foreach (var p in typeof(NurseStationClusterClinicResponse).GetProperties()) p.SetValue(d, p.GetValue(b)); d.Description = e.Description; d.UpdateDateTime = e.UpdateDateTime; d.UpdateBy = e.UpdateBy == Guid.Empty ? null : e.UpdateBy; d.UpdateByName = GetActorName(names, e.UpdateBy); return d; }
        private static NurseStationClusterClinicOptionResponse MapOptionResponse(MstNurseStationClusterClinic e) => new() { Id = e.Id, NurseStationClusterId = e.NurseStationClusterId, ClusterName = e.NurseStationCluster?.ClusterName, ClinicId = e.ClinicId, ClinicCode = e.Clinic?.ClinicCode, ClinicName = e.Clinic?.ClinicName, IsPrimary = e.IsPrimary, SortOrder = e.SortOrder };
        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> ids) { var list = ids.Where(x => x != Guid.Empty).Distinct().ToList(); if (!list.Any()) return new(); return await _dbContext.Users.AsNoTracking().Where(x => list.Contains(x.Id)).Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode }).ToDictionaryAsync(x => x.Id, x => x.Name); }
        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> names, Guid id) => id == Guid.Empty ? null : names.TryGetValue(id, out var n) ? n : null;
        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize) { if (pageNumber < 1) pageNumber = 1; if (pageSize < 1) pageSize = 25; if (pageSize > 100) pageSize = 100; return (pageNumber, pageSize); }
        private static string? NormalizeNullableString(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private Guid GetCurrentUserId() { var v = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id"); return Guid.TryParse(v, out var id) ? id : Guid.Empty; }
    }
}
