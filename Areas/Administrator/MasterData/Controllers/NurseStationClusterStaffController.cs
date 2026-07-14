using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
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
    [AccessController(
        moduleCode: "ADMINISTRATOR_MASTER_DATA",
        moduleName: "Administrator Master Data",
        displayName: "Nurse Station Cluster Staff",
        AreaName = "Administrator",
        ControllerName = "NurseStationClusterStaff",
        Description = "Administrator master data nurse station cluster staff beserta poli yang ditangani",
        SortOrder = 103
    )]
    [Tags("Administrator / Master Data / Nurse Station Cluster Staff")]
    public class NurseStationClusterStaffController : ControllerBase
    {
        private const string LogCategory = "Administrator.MasterData";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public NurseStationClusterStaffController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

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
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "clusterName", Label = "Nama cluster" },
                    new() { Value = "employeeName", Label = "Nama perawat" },
                    new() { Value = "clinicCount", Label = "Jumlah poli" },
                    new() { Value = "isPrimary", Label = "Petugas utama" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<NurseStationClusterStaffFilterMetadataResponse>.Ok(
                result,
                "Metadata filter cluster perawat berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<NurseStationClusterStaffSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nurse Station Cluster Staff", Description = "Melihat ringkasan staff cluster", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationClusterStaff", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQueryWithoutIncludes();
            var staffIds = query.Select(x => x.Id);

            var clinicAssignmentQuery = _dbContext.Set<MstNurseStationClusterStaffClinic>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && staffIds.Contains(x.NurseStationClusterStaffId));

            var result = new NurseStationClusterStaffSummaryResponse
            {
                TotalStaff = await query.CountAsync(),
                ActiveStaff = await query.CountAsync(x => x.IsActive),
                InactiveStaff = await query.CountAsync(x => !x.IsActive),
                PrimaryStaff = await query.CountAsync(x => x.IsPrimary),
                CanCallQueueStaff = await query.CountAsync(x => x.CanCallQueue),
                CanStartScreeningStaff = await query.CountAsync(x => x.CanStartScreening),
                CanTransferQueueStaff = await query.CountAsync(x => x.CanTransferQueue),
                TotalClinicAssignment = await clinicAssignmentQuery.CountAsync(),
                StaffWithoutClinic = await query.CountAsync(x =>
                    !x.StaffClinics.Any(sc => !sc.IsDelete && sc.IsActive))
            };

            return Ok(ApiResponse<NurseStationClusterStaffSummaryResponse>.Ok(
                result,
                "Ringkasan cluster perawat berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseNurseStationClusterStaffPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nurse Station Cluster Staff", Description = "Melihat data staff cluster", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationClusterStaff", "Read")]
        public async Task<IActionResult> GetNurseStationClusterStaffs(
            [FromQuery] Guid? nurseStationClusterId,
            [FromQuery] Guid? employeeId,
            [FromQuery] Guid? workforceProfileId,
            [FromQuery] Guid? clinicId,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isPrimary,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyFilter(
                BuildBaseQuery(),
                nurseStationClusterId,
                employeeId,
                workforceProfileId,
                clinicId,
                isActive,
                isPrimary,
                search);

            var totalData = await query.CountAsync();
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities.Select(x => x.CreateBy).Where(x => x != Guid.Empty));

            var result = new ResponseNurseStationClusterStaffPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(x => MapResponse(x, actorNames)).ToList()
            };

            return Ok(ApiResponse<ResponseNurseStationClusterStaffPagedResult>.Ok(
                result,
                "Data cluster perawat berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<NurseStationClusterStaffOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nurse Station Cluster Staff", Description = "Melihat data pilihan staff cluster", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationClusterStaff", "Read")]
        public async Task<IActionResult> GetNurseStationClusterStaffOptions(
            [FromQuery] Guid? nurseStationClusterId,
            [FromQuery] Guid? clinicId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyFilter(
                BuildBaseQuery(),
                nurseStationClusterId,
                null,
                null,
                clinicId,
                onlyActive ? true : null,
                null,
                search);

            var totalData = await query.CountAsync();
            var entities = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.Employee!.FullName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(MapOptionResponse).ToList();

            return Ok(ApiResponse<NurseStationClusterStaffOptionPagedResponse>.Ok(
                new NurseStationClusterStaffOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan cluster perawat berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<NurseStationClusterStaffDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nurse Station Cluster Staff", Description = "Melihat detail staff cluster", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationClusterStaff", "Read")]
        public async Task<IActionResult> GetNurseStationClusterStaffById(Guid id)
        {
            var entity = await BuildBaseQuery().FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Cluster perawat tidak ditemukan."));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            return Ok(ApiResponse<NurseStationClusterStaffDetailResponse>.Ok(
                MapDetailResponse(entity, actorNames),
                "Detail cluster perawat berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<NurseStationClusterStaffCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Nurse Station Cluster Staff", Description = "Membuat staff cluster beserta poli", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("NurseStationClusterStaff", "Create")]
        public async Task<IActionResult> CreateNurseStationClusterStaff(
            [FromBody] CreateNurseStationClusterStaffRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data cluster perawat tidak valid."));
            }

            var clinicIds = NormalizeClinicIds(request.ClinicIds);
            var employee = await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .FirstAsync(x => x.Id == request.EmployeeId);

            var workforceProfileId = request.WorkforceProfileId.HasValue &&
                                     request.WorkforceProfileId.Value != Guid.Empty
                ? request.WorkforceProfileId.Value
                : employee.WorkforceProfileId;

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var entity = new MstNurseStationClusterStaff
                {
                    Id = Guid.NewGuid(),
                    NurseStationClusterId = request.NurseStationClusterId,
                    EmployeeId = request.EmployeeId,
                    WorkforceProfileId = workforceProfileId,
                    IsPrimary = request.IsPrimary,
                    CanCallQueue = request.CanCallQueue,
                    CanStartScreening = request.CanStartScreening,
                    CanTransferQueue = request.CanTransferQueue,
                    SortOrder = request.SortOrder,
                    Description = NormalizeNullableString(request.Description),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<MstNurseStationClusterStaff>().Add(entity);

                var clinicMappings = BuildStaffClinicMappings(
                    entity.Id,
                    clinicIds,
                    now,
                    actorUserId);

                _dbContext.Set<MstNurseStationClusterStaffClinic>().AddRange(clinicMappings);

                await _dbContext.SaveChangesAsync();
                await ReconcileClusterClinicMappingsAsync(
                    entity.NurseStationClusterId,
                    now,
                    actorUserId);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = new NurseStationClusterStaffCreateResponse
                {
                    Id = entity.Id,
                    NurseStationClusterId = entity.NurseStationClusterId,
                    EmployeeId = entity.EmployeeId,
                    WorkforceProfileId = entity.WorkforceProfileId,
                    ClinicCount = clinicIds.Count,
                    ClinicIds = clinicIds,
                    IsActive = entity.IsActive
                };

                await _loggerService.InfoAsync(
                    LogCategory,
                    "NurseStationClusterStaff.Create",
                    "Membuat cluster perawat beserta mapping poli.",
                    result);

                return Ok(ApiResponse<NurseStationClusterStaffCreateResponse>.Ok(
                    result,
                    "Cluster perawat berhasil dibuat."));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggerService.ErrorAsync(
                    LogCategory,
                    "NurseStationClusterStaff.Create",
                    "Gagal membuat cluster perawat.",
                    ex);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat cluster perawat."));
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Nurse Station Cluster Staff", Description = "Mengubah staff cluster beserta poli", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("NurseStationClusterStaff", "Update")]
        public async Task<IActionResult> UpdateNurseStationClusterStaff(
            Guid id,
            [FromBody] UpdateNurseStationClusterStaffRequest request)
        {
            var entity = await _dbContext.Set<MstNurseStationClusterStaff>()
                .Include(x => x.StaffClinics)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Cluster perawat tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data cluster perawat tidak valid."));
            }

            var employee = await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .FirstAsync(x => x.Id == request.EmployeeId);

            var clinicIds = NormalizeClinicIds(request.ClinicIds);
            var oldClusterId = entity.NurseStationClusterId;
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                entity.NurseStationClusterId = request.NurseStationClusterId;
                entity.EmployeeId = request.EmployeeId;
                entity.WorkforceProfileId = request.WorkforceProfileId.HasValue &&
                                            request.WorkforceProfileId.Value != Guid.Empty
                    ? request.WorkforceProfileId.Value
                    : employee.WorkforceProfileId;
                entity.IsPrimary = request.IsPrimary;
                entity.CanCallQueue = request.CanCallQueue;
                entity.CanStartScreening = request.CanStartScreening;
                entity.CanTransferQueue = request.CanTransferQueue;
                entity.SortOrder = request.SortOrder;
                entity.Description = NormalizeNullableString(request.Description);
                entity.IsActive = request.IsActive;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                SynchronizeStaffClinicMappings(
                    entity,
                    clinicIds,
                    now,
                    actorUserId);

                await _dbContext.SaveChangesAsync();

                await ReconcileClusterClinicMappingsAsync(
                    oldClusterId,
                    now,
                    actorUserId);

                if (oldClusterId != entity.NurseStationClusterId)
                {
                    await ReconcileClusterClinicMappingsAsync(
                        entity.NurseStationClusterId,
                        now,
                        actorUserId);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "NurseStationClusterStaff.Update",
                    "Mengubah cluster perawat beserta mapping poli.",
                    new
                    {
                        entity.Id,
                        entity.NurseStationClusterId,
                        entity.EmployeeId,
                        ClinicIds = clinicIds,
                        entity.IsActive
                    });

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "Cluster perawat berhasil diperbarui."));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggerService.ErrorAsync(
                    LogCategory,
                    "NurseStationClusterStaff.Update",
                    "Gagal memperbarui cluster perawat.",
                    ex);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat memperbarui cluster perawat."));
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Nurse Station Cluster Staff Status", Description = "Mengubah status staff cluster", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("NurseStationClusterStaff", "Update")]
        public async Task<IActionResult> UpdateNurseStationClusterStaffStatus(
            Guid id,
            [FromBody] UpdateNurseStationClusterStaffStatusRequest request)
        {
            var entity = await _dbContext.Set<MstNurseStationClusterStaff>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Cluster perawat tidak ditemukan."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                entity.IsActive = request.IsActive;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync();
                await ReconcileClusterClinicMappingsAsync(
                    entity.NurseStationClusterId,
                    now,
                    actorUserId);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "Status cluster perawat berhasil diperbarui."));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggerService.ErrorAsync(
                    LogCategory,
                    "NurseStationClusterStaff.UpdateStatus",
                    "Gagal memperbarui status cluster perawat.",
                    ex);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat memperbarui status cluster perawat."));
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Delete", "Delete Nurse Station Cluster Staff", Description = "Menghapus staff cluster", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("NurseStationClusterStaff", "Delete")]
        public async Task<IActionResult> DeleteNurseStationClusterStaff(
            Guid id,
            [FromBody] DeleteNurseStationClusterStaffRequest? request = null)
        {
            var entity = await _dbContext.Set<MstNurseStationClusterStaff>()
                .Include(x => x.StaffClinics)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Cluster perawat tidak ditemukan."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                entity.IsDelete = true;
                entity.IsActive = false;
                entity.DeleteDateTime = now;
                entity.DeleteBy = actorUserId;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                if (!string.IsNullOrWhiteSpace(request?.DeleteReason))
                {
                    entity.Description = request.DeleteReason.Trim();
                }

                foreach (var mapping in entity.StaffClinics.Where(x => !x.IsDelete))
                {
                    mapping.IsDelete = true;
                    mapping.IsActive = false;
                    mapping.DeleteDateTime = now;
                    mapping.DeleteBy = actorUserId;
                    mapping.UpdateDateTime = now;
                    mapping.UpdateBy = actorUserId;
                }

                await _dbContext.SaveChangesAsync();
                await ReconcileClusterClinicMappingsAsync(
                    entity.NurseStationClusterId,
                    now,
                    actorUserId);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "Cluster perawat berhasil dihapus."));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggerService.ErrorAsync(
                    LogCategory,
                    "NurseStationClusterStaff.Delete",
                    "Gagal menghapus cluster perawat.",
                    ex);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat menghapus cluster perawat."));
            }
        }

        private IQueryable<MstNurseStationClusterStaff> BuildBaseQueryWithoutIncludes()
        {
            return _dbContext.Set<MstNurseStationClusterStaff>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private IQueryable<MstNurseStationClusterStaff> BuildBaseQuery()
        {
            return _dbContext.Set<MstNurseStationClusterStaff>()
                .AsNoTracking()
                .Include(x => x.NurseStationCluster)
                .Include(x => x.Employee)
                .Include(x => x.WorkforceProfile)
                .Include(x => x.StaffClinics.Where(sc => !sc.IsDelete && sc.IsActive))
                    .ThenInclude(sc => sc.Clinic)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstNurseStationClusterStaff> ApplyFilter(
            IQueryable<MstNurseStationClusterStaff> query,
            Guid? clusterId,
            Guid? employeeId,
            Guid? workforceProfileId,
            Guid? clinicId,
            bool? isActive,
            bool? isPrimary,
            string? search)
        {
            if (clusterId.HasValue && clusterId.Value != Guid.Empty)
            {
                query = query.Where(x => x.NurseStationClusterId == clusterId.Value);
            }

            if (employeeId.HasValue && employeeId.Value != Guid.Empty)
            {
                query = query.Where(x => x.EmployeeId == employeeId.Value);
            }

            if (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty)
            {
                query = query.Where(x => x.WorkforceProfileId == workforceProfileId.Value);
            }

            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
            {
                query = query.Where(x => x.StaffClinics.Any(sc =>
                    !sc.IsDelete &&
                    sc.IsActive &&
                    sc.ClinicId == clinicId.Value));
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (isPrimary.HasValue)
            {
                query = query.Where(x => x.IsPrimary == isPrimary.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.NurseStationCluster!.ClusterName.ToLower().Contains(keyword) ||
                    x.NurseStationCluster.ClusterCode.ToLower().Contains(keyword) ||
                    x.Employee!.FullName.ToLower().Contains(keyword) ||
                    x.Employee.EmployeeCode.ToLower().Contains(keyword) ||
                    x.Employee.EmployeeNumber.ToLower().Contains(keyword) ||
                    x.StaffClinics.Any(sc =>
                        !sc.IsDelete &&
                        sc.IsActive &&
                        sc.Clinic != null &&
                        (sc.Clinic.ClinicName.ToLower().Contains(keyword) ||
                         sc.Clinic.ClinicCode.ToLower().Contains(keyword))) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstNurseStationClusterStaff> ApplySorting(
            IQueryable<MstNurseStationClusterStaff> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),
                "clustername" => desc
                    ? query.OrderByDescending(x => x.NurseStationCluster!.ClusterName)
                    : query.OrderBy(x => x.NurseStationCluster!.ClusterName),
                "employeename" => desc
                    ? query.OrderByDescending(x => x.Employee!.FullName)
                    : query.OrderBy(x => x.Employee!.FullName),
                "cliniccount" => desc
                    ? query.OrderByDescending(x => x.StaffClinics.Count(sc => !sc.IsDelete && sc.IsActive))
                    : query.OrderBy(x => x.StaffClinics.Count(sc => !sc.IsDelete && sc.IsActive)),
                "isprimary" => desc
                    ? query.OrderByDescending(x => x.IsPrimary)
                    : query.OrderBy(x => x.IsPrimary),
                "isactive" => desc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),
                _ => desc
                    ? query.OrderByDescending(x => x.SortOrder)
                        .ThenByDescending(x => x.Employee!.FullName)
                    : query.OrderBy(x => x.SortOrder)
                        .ThenBy(x => x.Employee!.FullName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateNurseStationClusterStaffRequest request)
        {
            if (request.NurseStationClusterId == Guid.Empty)
            {
                return (false, "Cluster wajib diisi.");
            }

            if (request.EmployeeId == Guid.Empty)
            {
                return (false, "Perawat jaga wajib diisi.");
            }

            var clinicIds = NormalizeClinicIds(request.ClinicIds);
            if (!clinicIds.Any())
            {
                return (false, "Minimal satu poli wajib dipilih.");
            }

            if ((request.ClinicIds ?? new List<Guid>()).Any(x => x == Guid.Empty))
            {
                return (false, "Terdapat ID poli yang tidak valid.");
            }

            var clusterExists = await _dbContext.Set<MstNurseStationCluster>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == request.NurseStationClusterId &&
                    !x.IsDelete);

            if (!clusterExists)
            {
                return (false, "Cluster tidak ditemukan.");
            }

            var employee = await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.EmployeeId &&
                    !x.IsDelete &&
                    x.IsActive);

            if (employee == null)
            {
                return (false, "Perawat atau pegawai aktif tidak ditemukan.");
            }

            if (request.WorkforceProfileId.HasValue &&
                request.WorkforceProfileId.Value != Guid.Empty &&
                request.WorkforceProfileId.Value != employee.WorkforceProfileId)
            {
                return (false, "Workforce profile tidak sesuai dengan pegawai.");
            }

            var validClinicIds = await _dbContext.Set<MstClinic>()
                .AsNoTracking()
                .Where(x =>
                    clinicIds.Contains(x.Id) &&
                    !x.IsDelete)
                .Select(x => x.Id)
                .ToListAsync();

            if (validClinicIds.Count != clinicIds.Count)
            {
                return (false, "Terdapat poli yang tidak ditemukan.");
            }

            var duplicateQuery = _dbContext.Set<MstNurseStationClusterStaff>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.NurseStationClusterId == request.NurseStationClusterId &&
                    x.EmployeeId == request.EmployeeId);

            if (excludeId.HasValue)
            {
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateQuery.AnyAsync())
            {
                return (false, "Perawat sudah terdaftar pada cluster tersebut.");
            }

            return (true, null);
        }

        private static List<MstNurseStationClusterStaffClinic> BuildStaffClinicMappings(
            Guid staffId,
            IReadOnlyList<Guid> clinicIds,
            DateTime now,
            Guid actorUserId)
        {
            return clinicIds
                .Select((clinicId, index) => new MstNurseStationClusterStaffClinic
                {
                    Id = Guid.NewGuid(),
                    NurseStationClusterStaffId = staffId,
                    ClinicId = clinicId,
                    SortOrder = index,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                })
                .ToList();
        }

        private void SynchronizeStaffClinicMappings(
            MstNurseStationClusterStaff staff,
            IReadOnlyList<Guid> desiredClinicIds,
            DateTime now,
            Guid actorUserId)
        {
            var desiredSet = desiredClinicIds.ToHashSet();
            var currentActive = staff.StaffClinics
                .Where(x => !x.IsDelete)
                .GroupBy(x => x.ClinicId)
                .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.CreateDateTime).First());

            for (var index = 0; index < desiredClinicIds.Count; index++)
            {
                var clinicId = desiredClinicIds[index];

                if (currentActive.TryGetValue(clinicId, out var existing))
                {
                    existing.IsActive = true;
                    existing.SortOrder = index;
                    existing.UpdateDateTime = now;
                    existing.UpdateBy = actorUserId;
                    continue;
                }

                _dbContext.Set<MstNurseStationClusterStaffClinic>().Add(
                    new MstNurseStationClusterStaffClinic
                    {
                        Id = Guid.NewGuid(),
                        NurseStationClusterStaffId = staff.Id,
                        ClinicId = clinicId,
                        SortOrder = index,
                        IsActive = true,
                        CreateDateTime = now,
                        CreateBy = actorUserId,
                        IsDelete = false,
                        IsCancel = false
                    });
            }

            foreach (var existing in staff.StaffClinics.Where(x =>
                         !x.IsDelete &&
                         !desiredSet.Contains(x.ClinicId)))
            {
                existing.IsDelete = true;
                existing.IsActive = false;
                existing.DeleteDateTime = now;
                existing.DeleteBy = actorUserId;
                existing.UpdateDateTime = now;
                existing.UpdateBy = actorUserId;
            }
        }

        private async Task ReconcileClusterClinicMappingsAsync(
            Guid clusterId,
            DateTime now,
            Guid actorUserId)
        {
            if (clusterId == Guid.Empty)
            {
                return;
            }

            var desiredClinicIds = await _dbContext.Set<MstNurseStationClusterStaffClinic>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.NurseStationClusterStaff != null &&
                    !x.NurseStationClusterStaff.IsDelete &&
                    x.NurseStationClusterStaff.IsActive &&
                    x.NurseStationClusterStaff.NurseStationClusterId == clusterId)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ClinicId)
                .Select(x => x.ClinicId)
                .Distinct()
                .ToListAsync();

            var existingMappings = await _dbContext.Set<MstNurseStationClusterClinic>()
                .Where(x =>
                    x.NurseStationClusterId == clusterId &&
                    !x.IsDelete)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.CreateDateTime)
                .ToListAsync();

            var existingByClinic = existingMappings
                .GroupBy(x => x.ClinicId)
                .ToDictionary(x => x.Key, x => x.First());

            for (var index = 0; index < desiredClinicIds.Count; index++)
            {
                var clinicId = desiredClinicIds[index];

                if (existingByClinic.TryGetValue(clinicId, out var existing))
                {
                    if (!existing.IsActive || existing.SortOrder != index)
                    {
                        existing.IsActive = true;
                        existing.SortOrder = index;
                        existing.UpdateDateTime = now;
                        existing.UpdateBy = actorUserId;
                    }

                    continue;
                }

                var mapping = new MstNurseStationClusterClinic
                {
                    Id = Guid.NewGuid(),
                    NurseStationClusterId = clusterId,
                    ClinicId = clinicId,
                    IsPrimary = false,
                    SortOrder = index,
                    Description = "Dibentuk otomatis dari pengaturan cluster perawat.",
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<MstNurseStationClusterClinic>().Add(mapping);
                existingMappings.Add(mapping);
                existingByClinic[clinicId] = mapping;
            }

            var desiredSet = desiredClinicIds.ToHashSet();

            foreach (var mapping in existingMappings.Where(x =>
                         x.IsActive &&
                         !desiredSet.Contains(x.ClinicId)))
            {
                mapping.IsActive = false;
                mapping.IsPrimary = false;
                mapping.UpdateDateTime = now;
                mapping.UpdateBy = actorUserId;
            }

            var activeMappings = existingMappings
                .Where(x => x.IsActive && desiredSet.Contains(x.ClinicId))
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.CreateDateTime)
                .ToList();

            if (activeMappings.Any())
            {
                var primaryId = activeMappings.FirstOrDefault(x => x.IsPrimary)?.Id
                                ?? activeMappings[0].Id;

                foreach (var mapping in activeMappings)
                {
                    var shouldBePrimary = mapping.Id == primaryId;
                    if (mapping.IsPrimary == shouldBePrimary)
                    {
                        continue;
                    }

                    mapping.IsPrimary = shouldBePrimary;
                    mapping.UpdateDateTime = now;
                    mapping.UpdateBy = actorUserId;
                }
            }
        }

        private static NurseStationClusterStaffResponse MapResponse(
            MstNurseStationClusterStaff entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var clinics = MapClinics(entity);

            return new NurseStationClusterStaffResponse
            {
                Id = entity.Id,
                NurseStationClusterId = entity.NurseStationClusterId,
                ClusterCode = entity.NurseStationCluster?.ClusterCode,
                ClusterName = entity.NurseStationCluster?.ClusterName,
                EmployeeId = entity.EmployeeId,
                EmployeeCode = entity.Employee?.EmployeeCode,
                EmployeeNumber = entity.Employee?.EmployeeNumber,
                EmployeeName = entity.Employee?.FullName,
                WorkforceProfileId = entity.WorkforceProfileId,
                WorkforceProfileCode = entity.WorkforceProfile?.ProfileCode,
                WorkforceProfileName = entity.WorkforceProfile?.DisplayName,
                ClinicCount = clinics.Count,
                ClinicIds = clinics.Select(x => x.ClinicId).ToList(),
                Clinics = clinics,
                IsPrimary = entity.IsPrimary,
                CanCallQueue = entity.CanCallQueue,
                CanStartScreening = entity.CanStartScreening,
                CanTransferQueue = entity.CanTransferQueue,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static NurseStationClusterStaffDetailResponse MapDetailResponse(
            MstNurseStationClusterStaff entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = new NurseStationClusterStaffDetailResponse();
            var baseResponse = MapResponse(entity, actorNames);

            foreach (var property in typeof(NurseStationClusterStaffResponse).GetProperties())
            {
                property.SetValue(response, property.GetValue(baseResponse));
            }

            response.Description = entity.Description;
            response.UpdateDateTime = entity.UpdateDateTime;
            response.UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy;
            response.UpdateByName = GetActorName(actorNames, entity.UpdateBy);

            return response;
        }

        private static NurseStationClusterStaffOptionResponse MapOptionResponse(
            MstNurseStationClusterStaff entity)
        {
            var clinics = MapClinics(entity);

            return new NurseStationClusterStaffOptionResponse
            {
                Id = entity.Id,
                NurseStationClusterId = entity.NurseStationClusterId,
                ClusterName = entity.NurseStationCluster?.ClusterName,
                EmployeeId = entity.EmployeeId,
                EmployeeCode = entity.Employee?.EmployeeCode,
                EmployeeName = entity.Employee?.FullName,
                ClinicCount = clinics.Count,
                ClinicIds = clinics.Select(x => x.ClinicId).ToList(),
                ClinicNames = clinics
                    .Select(x => x.ClinicName)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!)
                    .ToList(),
                IsPrimary = entity.IsPrimary,
                SortOrder = entity.SortOrder
            };
        }

        private static List<NurseStationClusterStaffClinicResponse> MapClinics(
            MstNurseStationClusterStaff entity)
        {
            return entity.StaffClinics
                .Where(x => !x.IsDelete && x.IsActive)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Clinic != null ? x.Clinic.ClinicName : string.Empty)
                .Select(x => new NurseStationClusterStaffClinicResponse
                {
                    Id = x.Id,
                    ClinicId = x.ClinicId,
                    ClinicCode = x.Clinic?.ClinicCode,
                    ClinicName = x.Clinic?.ClinicName,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive
                })
                .ToList();
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(
            IEnumerable<Guid> ids)
        {
            var actorIds = ids
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (!actorIds.Any())
            {
                return new Dictionary<Guid, string?>();
            }

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => actorIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static string? GetActorName(
            IReadOnlyDictionary<Guid, string?> actorNames,
            Guid actorId)
        {
            return actorId == Guid.Empty
                ? null
                : actorNames.TryGetValue(actorId, out var actorName)
                    ? actorName
                    : null;
        }

        private static List<Guid> NormalizeClinicIds(IEnumerable<Guid>? clinicIds)
        {
            return (clinicIds ?? Enumerable.Empty<Guid>())
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();
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

        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                              ?? User.FindFirstValue("user_id");

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
