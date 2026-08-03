using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using LeaveTypePagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.DTOs.LeaveTypeResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/leave-types")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Leave Type",
        AreaName = "Corporate",
        ControllerName = "LeaveType",
        Description = "Corporate human resource master data leave type",
        SortOrder = 30)]
    [Tags("Corporate / Human Resource / Master Data / Leave and Overtime / Leave Type")]
    public class LeaveTypeController : ControllerBase
    {
        private static readonly HashSet<string> AllowedLeaveCategories = new(StringComparer.OrdinalIgnoreCase)
        {
            "Annual", "Sick", "Maternity", "Paternity", "Marriage",
            "Bereavement", "Unpaid", "Special", "Compensatory", "Other"
        };

        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "LVT-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public LeaveTypeController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<LeaveTypeFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Type", Description = "Melihat metadata filter leave type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveType", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new LeaveTypeFilterMetadataResponse
            {
                DefaultFilter = new LeaveTypeDefaultFilterResponse(),
                LeaveCategoryOptions = AllowedLeaveCategories.OrderBy(x => x)
                    .Select(x => new LeaveTypeStringOptionResponse { Value = x, Label = x }).ToList(),
                SortOptions = new List<LeaveTypeSortOptionResponse>
                {
                    new() { Value = "leaveTypeCode", Label = "Kode jenis cuti" },
                    new() { Value = "leaveTypeName", Label = "Nama jenis cuti" },
                    new() { Value = "leaveCategory", Label = "Kategori cuti" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(LogCategory, "LeaveType.GetFilterMetadata", "Mengambil metadata filter leave type.", result);
            return Ok(ApiResponse<LeaveTypeFilterMetadataResponse>.Ok(result, "Metadata filter leave type berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<LeaveTypeSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Type", Description = "Melihat ringkasan leave type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveType", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BaseQuery();
            var result = new LeaveTypeSummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive),
                PaidLeaveData = await query.CountAsync(x => x.IsPaidLeave),
                BalanceDeductedData = await query.CountAsync(x => x.IsBalanceDeducted)
            };
            return Ok(ApiResponse<LeaveTypeSummaryResponse>.Ok(result, "Ringkasan leave type berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<LeaveTypePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Type", Description = "Melihat data leave type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveType", "Read")]
        public async Task<IActionResult> GetData(
            [FromQuery] string? leaveCategory,
            [FromQuery] bool? isPaidLeave,
            [FromQuery] bool? isBalanceDeducted,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "leaveTypeName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var query = ApplyFilter(BaseQuery(), leaveCategory, isPaidLeave, isBalanceDeducted, isActive, search);
            var totalData = await query.CountAsync();
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var actorNames = await GetActorNameMapAsync(entities.Select(x => x.CreateBy));
            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

            return Ok(ApiResponse<LeaveTypePagedResult>.Ok(new LeaveTypePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data leave type berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<LeaveTypeOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Type", Description = "Melihat pilihan leave type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveType", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? leaveCategory,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var query = ApplyFilter(BaseQuery(), leaveCategory, null, null, onlyActive ? true : null, search);
            var totalData = await query.CountAsync();
            var items = await query.OrderBy(x => x.LeaveCategory).ThenBy(x => x.LeaveTypeName)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(x => new LeaveTypeOptionResponse
                {
                    Id = x.Id,
                    LeaveTypeCode = x.LeaveTypeCode,
                    LeaveTypeName = x.LeaveTypeName,
                    LeaveCategory = x.LeaveCategory,
                    IsPaidLeave = x.IsPaidLeave,
                    IsBalanceDeducted = x.IsBalanceDeducted
                }).ToListAsync();

            return Ok(ApiResponse<LeaveTypeOptionPagedResponse>.Ok(new LeaveTypeOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Pilihan leave type berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LeaveTypeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Leave Type", Description = "Melihat detail leave type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveType", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Leave type tidak ditemukan."));

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            var response = MapResponse(entity, actorNames);
            var result = new LeaveTypeDetailResponse
            {
                Id = response.Id,
                LeaveTypeCode = response.LeaveTypeCode,
                LeaveTypeName = response.LeaveTypeName,
                LeaveCategory = response.LeaveCategory,
                IsPaidLeave = response.IsPaidLeave,
                IsBalanceDeducted = response.IsBalanceDeducted,
                AllowHalfDay = response.AllowHalfDay,
                AllowHourly = response.AllowHourly,
                RequiresAttachment = response.RequiresAttachment,
                RequiresMedicalCertificate = response.RequiresMedicalCertificate,
                AttachmentRequiredAfterDays = response.AttachmentRequiredAfterDays,
                DefaultMinimumNoticeDays = response.DefaultMinimumNoticeDays,
                DefaultMaximumConsecutiveDays = response.DefaultMaximumConsecutiveDays,
                ColorCode = response.ColorCode,
                Description = response.Description,
                IsActive = response.IsActive,
                LeavePolicyCount = response.LeavePolicyCount,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
            return Ok(ApiResponse<LeaveTypeDetailResponse>.Ok(result, "Detail leave type berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LeaveTypeCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Leave Type", Description = "Membuat leave type", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LeaveType", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateLeaveTypeRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Payload leave type wajib diisi."));

            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            var entity = new MstLeaveType
            {
                Id = Guid.NewGuid(),
                LeaveTypeCode = await GenerateCodeAsync(),
                LeaveTypeName = request.LeaveTypeName.Trim(),
                LeaveCategory = NormalizeLeaveCategory(request.LeaveCategory),
                IsPaidLeave = request.IsPaidLeave,
                IsBalanceDeducted = request.IsBalanceDeducted,
                AllowHalfDay = request.AllowHalfDay,
                AllowHourly = request.AllowHourly,
                RequiresAttachment = request.RequiresAttachment,
                RequiresMedicalCertificate = request.RequiresMedicalCertificate,
                AttachmentRequiredAfterDays = request.AttachmentRequiredAfterDays,
                DefaultMinimumNoticeDays = request.DefaultMinimumNoticeDays,
                DefaultMaximumConsecutiveDays = request.DefaultMaximumConsecutiveDays,
                ColorCode = NormalizeText(request.ColorCode),
                Description = NormalizeText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actor,
                IsDelete = false,
                IsCancel = false
            };
            _dbContext.Set<MstLeaveType>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy });
            var response = new LeaveTypeCreateResponse
            {
                Id = entity.Id,
                LeaveTypeCode = entity.LeaveTypeCode,
                LeaveTypeName = entity.LeaveTypeName,
                LeaveCategory = entity.LeaveCategory,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
            await _loggerService.InfoAsync(LogCategory, "LeaveType.Create", "Membuat data leave type.", response);
            return Ok(ApiResponse<LeaveTypeCreateResponse>.Ok(response, "Leave type berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LeaveTypeUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Leave Type", Description = "Mengubah leave type", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LeaveType", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeaveTypeRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Payload leave type wajib diisi."));

            var entity = await _dbContext.Set<MstLeaveType>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Leave type tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            entity.LeaveTypeName = request.LeaveTypeName.Trim();
            entity.LeaveCategory = NormalizeLeaveCategory(request.LeaveCategory);
            entity.IsPaidLeave = request.IsPaidLeave;
            entity.IsBalanceDeducted = request.IsBalanceDeducted;
            entity.AllowHalfDay = request.AllowHalfDay;
            entity.AllowHourly = request.AllowHourly;
            entity.RequiresAttachment = request.RequiresAttachment;
            entity.RequiresMedicalCertificate = request.RequiresMedicalCertificate;
            entity.AttachmentRequiredAfterDays = request.AttachmentRequiredAfterDays;
            entity.DefaultMinimumNoticeDays = request.DefaultMinimumNoticeDays;
            entity.DefaultMaximumConsecutiveDays = request.DefaultMaximumConsecutiveDays;
            entity.ColorCode = NormalizeText(request.ColorCode);
            entity.Description = NormalizeText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });
            var response = new LeaveTypeUpdateResponse
            {
                Id = entity.Id,
                LeaveTypeCode = entity.LeaveTypeCode,
                LeaveTypeName = entity.LeaveTypeName,
                LeaveCategory = entity.LeaveCategory,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
            await _loggerService.InfoAsync(LogCategory, "LeaveType.Update", "Mengubah data leave type.", response);
            return Ok(ApiResponse<LeaveTypeUpdateResponse>.Ok(response, "Leave type berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<LeaveTypeUpdateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Leave Type Status", Description = "Mengubah status leave type", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LeaveType", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateLeaveTypeStatusRequest request)
        {
            var entity = await _dbContext.Set<MstLeaveType>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Leave type tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });
            var response = new LeaveTypeUpdateResponse
            {
                Id = entity.Id,
                LeaveTypeCode = entity.LeaveTypeCode,
                LeaveTypeName = entity.LeaveTypeName,
                LeaveCategory = entity.LeaveCategory,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
            return Ok(ApiResponse<LeaveTypeUpdateResponse>.Ok(response, "Status leave type berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LeaveTypeDeleteResponse>), StatusCodes.Status200OK)]
        [AccessAction("Delete", "Delete Leave Type", Description = "Menghapus leave type", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("LeaveType", "Delete")]
        public async Task<IActionResult> Delete(Guid id, [FromBody] DeleteLeaveTypeRequest? request = null)
        {
            var entity = await _dbContext.Set<MstLeaveType>().Include(x => x.LeavePolicies)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Leave type tidak ditemukan."));
            if (entity.LeavePolicies.Any(x => !x.IsDelete))
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Leave type tidak dapat dihapus karena masih digunakan oleh leave policy."));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            if (!string.IsNullOrWhiteSpace(request?.DeleteReason)) entity.Description = request.DeleteReason.Trim();
            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.DeleteBy });
            var response = new LeaveTypeDeleteResponse
            {
                Id = entity.Id,
                LeaveTypeCode = entity.LeaveTypeCode,
                LeaveTypeName = entity.LeaveTypeName,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
            };
            await _loggerService.InfoAsync(LogCategory, "LeaveType.Delete", "Menghapus data leave type.", response);
            return Ok(ApiResponse<LeaveTypeDeleteResponse>.Ok(response, "Leave type berhasil dihapus."));
        }

        private IQueryable<MstLeaveType> BaseQuery() => _dbContext.Set<MstLeaveType>()
            .AsNoTracking().Include(x => x.LeavePolicies).Where(x => !x.IsDelete);

        private static IQueryable<MstLeaveType> ApplyFilter(IQueryable<MstLeaveType> query, string? category, bool? paid, bool? deducted, bool? active, string? search)
        {
            if (!string.IsNullOrWhiteSpace(category)) query = query.Where(x => x.LeaveCategory == category.Trim());
            if (paid.HasValue) query = query.Where(x => x.IsPaidLeave == paid.Value);
            if (deducted.HasValue) query = query.Where(x => x.IsBalanceDeducted == deducted.Value);
            if (active.HasValue) query = query.Where(x => x.IsActive == active.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x => x.LeaveTypeCode.ToLower().Contains(keyword) || x.LeaveTypeName.ToLower().Contains(keyword) ||
                    x.LeaveCategory.ToLower().Contains(keyword) || (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IOrderedQueryable<MstLeaveType> ApplySorting(IQueryable<MstLeaveType> query, string? sortBy, string? direction)
        {
            var desc = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "leaveTypeName").Trim().ToLowerInvariant() switch
            {
                "leavetypecode" => desc ? query.OrderByDescending(x => x.LeaveTypeCode) : query.OrderBy(x => x.LeaveTypeCode),
                "leavecategory" => desc ? query.OrderByDescending(x => x.LeaveCategory).ThenBy(x => x.LeaveTypeName) : query.OrderBy(x => x.LeaveCategory).ThenBy(x => x.LeaveTypeName),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.LeaveTypeName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.LeaveTypeName),
                _ => desc ? query.OrderByDescending(x => x.LeaveTypeName) : query.OrderBy(x => x.LeaveTypeName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateLeaveTypeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.LeaveTypeName)) return (false, "Nama leave type wajib diisi.");
            if (!AllowedLeaveCategories.Contains(request.LeaveCategory.Trim())) return (false, "Leave category tidak valid.");
            if (request.RequiresAttachment && request.AttachmentRequiredAfterDays.HasValue && request.AttachmentRequiredAfterDays <= 0)
                return (false, "AttachmentRequiredAfterDays harus lebih besar dari nol.");
            var name = request.LeaveTypeName.Trim().ToLower();
            var duplicate = _dbContext.Set<MstLeaveType>().AsNoTracking().Where(x => !x.IsDelete && x.LeaveTypeName.ToLower() == name);
            if (excludeId.HasValue) duplicate = duplicate.Where(x => x.Id != excludeId.Value);
            if (await duplicate.AnyAsync()) return (false, "Nama leave type sudah digunakan.");
            return (true, null);
        }

        private LeaveTypeResponse MapResponse(MstLeaveType x, IReadOnlyDictionary<Guid, string?> actors) => new()
        {
            Id = x.Id,
            LeaveTypeCode = x.LeaveTypeCode,
            LeaveTypeName = x.LeaveTypeName,
            LeaveCategory = x.LeaveCategory,
            IsPaidLeave = x.IsPaidLeave,
            IsBalanceDeducted = x.IsBalanceDeducted,
            AllowHalfDay = x.AllowHalfDay,
            AllowHourly = x.AllowHourly,
            RequiresAttachment = x.RequiresAttachment,
            RequiresMedicalCertificate = x.RequiresMedicalCertificate,
            AttachmentRequiredAfterDays = x.AttachmentRequiredAfterDays,
            DefaultMinimumNoticeDays = x.DefaultMinimumNoticeDays,
            DefaultMaximumConsecutiveDays = x.DefaultMaximumConsecutiveDays,
            ColorCode = x.ColorCode,
            Description = x.Description,
            IsActive = x.IsActive,
            LeavePolicyCount = x.LeavePolicies.Count(y => !y.IsDelete),
            CreateDateTime = x.CreateDateTime,
            CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
            CreateByName = GetActorName(actors, x.CreateBy)
        };

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstLeaveType>().AsNoTracking()
                .Where(x => !x.IsDelete && x.LeaveTypeCode.StartsWith(CodePrefix)).Select(x => x.LeaveTypeCode).ToListAsync();
            var used = codes.Select(x => x.Replace(CodePrefix, string.Empty)).Where(x => int.TryParse(x, out _)).Select(int.Parse).ToHashSet();
            var next = 1; while (used.Contains(next)) next++;
            return CodePrefix + next.ToString().PadLeft(CodeNumberLength, '0');
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> ids)
        {
            var actorIds = ids.Where(x => x != Guid.Empty).Distinct().ToList();
            return await _dbContext.Users.AsNoTracking().Where(x => actorIds.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> actors, Guid id) =>
            id == Guid.Empty ? null : actors.TryGetValue(id, out var name) ? name : null;
        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
        private static void NormalizePaging(ref int pageNumber, ref int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 25 : Math.Min(pageSize, 100);
        }
        private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static string NormalizeLeaveCategory(string value) => AllowedLeaveCategories.First(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
