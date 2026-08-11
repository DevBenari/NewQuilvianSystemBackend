using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Services
{
    public class LeaveAdjustmentReasonService
    {
        private const string CodePrefix = "LAR-RSMMC-";
        private const int CodeNumberLength = 5;
        private readonly ApplicationDbContext _dbContext;

        public LeaveAdjustmentReasonService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public LeaveAdjustmentReasonFilterMetadataResponse GetFilterMetadata() => new()
        {
            DefaultFilter = new LeaveAdjustmentReasonDefaultFilterResponse(),
            CustomPeriods = BuildPeriodOptions(),
            SortOptions = new List<LeaveAdjustmentReasonSortOptionResponse>
            {
                new() { Value = "sortOrder", Label = "Urutan" },
                new() { Value = "reasonCode", Label = "Kode alasan" },
                new() { Value = "reasonName", Label = "Nama alasan" },
                new() { Value = "reasonCategory", Label = "Kategori alasan" },
                new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                new() { Value = "isActive", Label = "Status aktif" }
            },
            SortDirections = new List<string> { "asc", "desc" },
            PageSizeOptions = new List<int> { 10, 25, 50, 100 }
        };

        public async Task<LeaveAdjustmentReasonSummaryResponse> GetSummaryAsync()
        {
            var query = BaseQuery();
            return new LeaveAdjustmentReasonSummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive),
                OpeningBalanceAllowedData = await query.CountAsync(x => x.AllowOpeningBalance),
                ApprovalRequiredData = await query.CountAsync(x => x.RequiresApproval)
            };
        }

        public async Task<PagedResult<LeaveAdjustmentReasonResponse>> GetDataAsync(
            DateTime? startDate, DateTime? endDate, string? customPeriod,
            Guid? leaveTypeId, string? reasonCategory, string? allowedDirection, bool? allowOpeningBalance,
            bool? requiresApproval, bool? isActive, string? search, string? sortBy, string? sortDirection,
            int pageNumber, int pageSize)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var query = ApplyFilter(BaseQuery(), leaveTypeId, reasonCategory, allowedDirection, allowOpeningBalance, requiresApproval, isActive, search);
            query = WorkflowMasterDataSupport.ApplyDateFilter(query, startDate, endDate, customPeriod);
            var totalData = await query.CountAsync();
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var actorNames = await GetActorNameMapAsync(entities.Select(x => x.CreateBy));
            return new PagedResult<LeaveAdjustmentReasonResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(x => MapResponse(x, actorNames)).ToList()
            };
        }

        public async Task<LeaveAdjustmentReasonOptionPagedResponse> GetOptionsAsync(
            Guid? leaveTypeId, string? reasonCategory, string? allowedDirection, bool onlyActive,
            string? search, int pageNumber, int pageSize)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var query = ApplyFilter(BaseQuery(), leaveTypeId, reasonCategory, allowedDirection, null, null, onlyActive ? true : null, search);
            var totalData = await query.CountAsync();
            var items = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.ReasonName)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(x => new LeaveAdjustmentReasonOptionResponse
                {
                    Id = x.Id,
                    LeaveTypeId = x.LeaveTypeId,
                    ReasonCode = x.ReasonCode,
                    ReasonName = x.ReasonName,
                    ReasonCategory = x.ReasonCategory,
                    AllowedDirection = x.AllowedDirection,
                    RequiresApproval = x.RequiresApproval
                }).ToListAsync();
            return new LeaveAdjustmentReasonOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<LeaveAdjustmentReasonDetailResponse?> GetByIdAsync(Guid id)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return null;
            var actors = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            var baseResponse = MapResponse(entity, actors);
            return new LeaveAdjustmentReasonDetailResponse
            {
                Id = baseResponse.Id, LeaveTypeId = baseResponse.LeaveTypeId, LeaveTypeCode = baseResponse.LeaveTypeCode, LeaveTypeName = baseResponse.LeaveTypeName,
                ReasonCode = baseResponse.ReasonCode, ReasonName = baseResponse.ReasonName, ReasonCategory = baseResponse.ReasonCategory,
                AllowedDirection = baseResponse.AllowedDirection, AllowOpeningBalance = baseResponse.AllowOpeningBalance,
                AllowManualAdjustment = baseResponse.AllowManualAdjustment, AllowCorrection = baseResponse.AllowCorrection,
                AllowReversal = baseResponse.AllowReversal, MaximumAdjustmentDays = baseResponse.MaximumAdjustmentDays,
                RequiresComment = baseResponse.RequiresComment, RequiresAttachment = baseResponse.RequiresAttachment,
                RequiresApproval = baseResponse.RequiresApproval, ApprovalWorkflowCode = baseResponse.ApprovalWorkflowCode,
                SortOrder = baseResponse.SortOrder, EffectiveStartDate = baseResponse.EffectiveStartDate, EffectiveEndDate = baseResponse.EffectiveEndDate,
                Description = baseResponse.Description, IsActive = baseResponse.IsActive, CreateDateTime = baseResponse.CreateDateTime,
                CreateBy = baseResponse.CreateBy, CreateByName = baseResponse.CreateByName, UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy, UpdateByName = GetActorName(actors, entity.UpdateBy)
            };
        }

        public async Task<LeaveAdjustmentReasonServiceResult<LeaveAdjustmentReasonCreateResponse>> CreateAsync(
            CreateLeaveAdjustmentReasonRequest request, Guid actor)
        {
            var error = await ValidateRequestAsync(null, request);
            if (error != null) return LeaveAdjustmentReasonServiceResult<LeaveAdjustmentReasonCreateResponse>.Invalid(error);
            var entity = new MstLeaveAdjustmentReason
            {
                Id = Guid.NewGuid(), LeaveTypeId = NormalizeGuid(request.LeaveTypeId), ReasonCode = await GenerateCodeAsync(),
                ReasonName = request.ReasonName.Trim(), ReasonCategory = request.ReasonCategory.Trim(), AllowedDirection = request.AllowedDirection.Trim(),
                AllowOpeningBalance = request.AllowOpeningBalance, AllowManualAdjustment = request.AllowManualAdjustment,
                AllowCorrection = request.AllowCorrection, AllowReversal = request.AllowReversal, MaximumAdjustmentDays = request.MaximumAdjustmentDays,
                RequiresComment = request.RequiresComment, RequiresAttachment = request.RequiresAttachment, RequiresApproval = request.RequiresApproval,
                ApprovalWorkflowCode = NormalizeText(request.ApprovalWorkflowCode), SortOrder = request.SortOrder,
                EffectiveStartDate = request.EffectiveStartDate?.Date, EffectiveEndDate = request.EffectiveEndDate?.Date,
                Description = NormalizeText(request.Description), IsActive = true, CreateDateTime = DateTime.UtcNow, CreateBy = actor,
                IsDelete = false, IsCancel = false
            };
            _dbContext.Set<MstLeaveAdjustmentReason>().Add(entity);
            await _dbContext.SaveChangesAsync();
            var actors = await GetActorNameMapAsync(new[] { entity.CreateBy });
            return LeaveAdjustmentReasonServiceResult<LeaveAdjustmentReasonCreateResponse>.Success(new LeaveAdjustmentReasonCreateResponse
            {
                Id = entity.Id, ReasonCode = entity.ReasonCode, ReasonName = entity.ReasonName, ReasonCategory = entity.ReasonCategory,
                IsActive = entity.IsActive, CreateDateTime = entity.CreateDateTime, CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actors, entity.CreateBy)
            });
        }

        public async Task<LeaveAdjustmentReasonServiceResult<LeaveAdjustmentReasonUpdateResponse>> UpdateAsync(
            Guid id, UpdateLeaveAdjustmentReasonRequest request, Guid actor)
        {
            var entity = await _dbContext.Set<MstLeaveAdjustmentReason>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return LeaveAdjustmentReasonServiceResult<LeaveAdjustmentReasonUpdateResponse>.NotFound();
            var error = await ValidateRequestAsync(id, request);
            if (error != null) return LeaveAdjustmentReasonServiceResult<LeaveAdjustmentReasonUpdateResponse>.Invalid(error);
            entity.LeaveTypeId = NormalizeGuid(request.LeaveTypeId); entity.ReasonName = request.ReasonName.Trim();
            entity.ReasonCategory = request.ReasonCategory.Trim(); entity.AllowedDirection = request.AllowedDirection.Trim();
            entity.AllowOpeningBalance = request.AllowOpeningBalance; entity.AllowManualAdjustment = request.AllowManualAdjustment;
            entity.AllowCorrection = request.AllowCorrection; entity.AllowReversal = request.AllowReversal;
            entity.MaximumAdjustmentDays = request.MaximumAdjustmentDays; entity.RequiresComment = request.RequiresComment;
            entity.RequiresAttachment = request.RequiresAttachment; entity.RequiresApproval = request.RequiresApproval;
            entity.ApprovalWorkflowCode = NormalizeText(request.ApprovalWorkflowCode); entity.SortOrder = request.SortOrder;
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date; entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeText(request.Description); entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow; entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync();
            var actors = await GetActorNameMapAsync(new[] { entity.UpdateBy });
            return LeaveAdjustmentReasonServiceResult<LeaveAdjustmentReasonUpdateResponse>.Success(new LeaveAdjustmentReasonUpdateResponse
            {
                Id = entity.Id, ReasonCode = entity.ReasonCode, ReasonName = entity.ReasonName, ReasonCategory = entity.ReasonCategory,
                IsActive = entity.IsActive, UpdateDateTime = entity.UpdateDateTime, UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actors, entity.UpdateBy)
            });
        }

        public async Task<bool> UpdateStatusAsync(Guid id, UpdateLeaveAdjustmentReasonStatusRequest request, Guid actor)
        {
            var entity = await _dbContext.Set<MstLeaveAdjustmentReason>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return false;
            entity.IsActive = request.IsActive; entity.UpdateDateTime = DateTime.UtcNow; entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<LeaveAdjustmentReasonDeleteResponse?> DeleteAsync(Guid id, DeleteLeaveAdjustmentReasonRequest? request, Guid actor)
        {
            var entity = await _dbContext.Set<MstLeaveAdjustmentReason>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return null;
            var now = DateTime.UtcNow;
            entity.IsDelete = true; entity.IsActive = false; entity.DeleteDateTime = now; entity.DeleteBy = actor;
            entity.UpdateDateTime = now; entity.UpdateBy = actor;
            if (!string.IsNullOrWhiteSpace(request?.DeleteReason)) entity.Description = request.DeleteReason.Trim();
            await _dbContext.SaveChangesAsync();
            var actors = await GetActorNameMapAsync(new[] { entity.DeleteBy });
            return new LeaveAdjustmentReasonDeleteResponse
            {
                Id = entity.Id, ReasonCode = entity.ReasonCode, ReasonName = entity.ReasonName,
                DeleteDateTime = entity.DeleteDateTime, DeleteBy = entity.DeleteBy == Guid.Empty ? null : entity.DeleteBy,
                DeleteByName = GetActorName(actors, entity.DeleteBy)
            };
        }

        private IQueryable<MstLeaveAdjustmentReason> BaseQuery() =>
            _dbContext.Set<MstLeaveAdjustmentReason>().AsNoTracking().Include(x => x.LeaveType).Where(x => !x.IsDelete);

        private static IQueryable<MstLeaveAdjustmentReason> ApplyFilter(IQueryable<MstLeaveAdjustmentReason> q, Guid? leaveTypeId, string? category, string? direction, bool? opening, bool? approval, bool? active, string? search)
        {
            if (leaveTypeId.HasValue && leaveTypeId != Guid.Empty) q = q.Where(x => x.LeaveTypeId == leaveTypeId);
            if (!string.IsNullOrWhiteSpace(category)) q = q.Where(x => x.ReasonCategory == category.Trim());
            if (!string.IsNullOrWhiteSpace(direction)) q = q.Where(x => x.AllowedDirection == direction.Trim());
            if (opening.HasValue) q = q.Where(x => x.AllowOpeningBalance == opening);
            if (approval.HasValue) q = q.Where(x => x.RequiresApproval == approval);
            if (active.HasValue) q = q.Where(x => x.IsActive == active);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var k = search.Trim().ToLower();
                q = q.Where(x => x.ReasonCode.ToLower().Contains(k) || x.ReasonName.ToLower().Contains(k) || x.ReasonCategory.ToLower().Contains(k) || (x.Description != null && x.Description.ToLower().Contains(k)));
            }
            return q;
        }

        private static IOrderedQueryable<MstLeaveAdjustmentReason> ApplySorting(IQueryable<MstLeaveAdjustmentReason> q, string? sortBy, string? direction)
        {
            var desc = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "reasoncode" => desc ? q.OrderByDescending(x => x.ReasonCode) : q.OrderBy(x => x.ReasonCode),
                "reasonname" => desc ? q.OrderByDescending(x => x.ReasonName) : q.OrderBy(x => x.ReasonName),
                "reasoncategory" => desc ? q.OrderByDescending(x => x.ReasonCategory) : q.OrderBy(x => x.ReasonCategory),
                "createdatetime" => desc ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime),
                "isactive" => desc ? q.OrderByDescending(x => x.IsActive).ThenBy(x => x.SortOrder) : q.OrderBy(x => x.IsActive).ThenBy(x => x.SortOrder),
                _ => desc ? q.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.ReasonName) : q.OrderBy(x => x.SortOrder).ThenBy(x => x.ReasonName)
            };
        }

        private async Task<string?> ValidateRequestAsync(Guid? excludeId, CreateLeaveAdjustmentReasonRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ReasonName)) return "Nama alasan adjustment wajib diisi.";
            if (string.IsNullOrWhiteSpace(request.ReasonCategory)) return "Kategori alasan wajib diisi.";
            if (string.IsNullOrWhiteSpace(request.AllowedDirection)) return "Arah adjustment wajib diisi.";
            if (request.EffectiveStartDate.HasValue && request.EffectiveEndDate.HasValue && request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date) return "Tanggal selesai efektif tidak boleh sebelum tanggal mulai efektif.";
            if (request.LeaveTypeId.HasValue && request.LeaveTypeId != Guid.Empty && !await _dbContext.Set<MstLeaveType>().AsNoTracking().AnyAsync(x => x.Id == request.LeaveTypeId && x.IsActive && !x.IsDelete)) return "Leave type tidak ditemukan atau tidak aktif.";
            var name = request.ReasonName.Trim().ToLower();
            var normalizedLeaveTypeId = NormalizeGuid(request.LeaveTypeId);
            var duplicate = _dbContext.Set<MstLeaveAdjustmentReason>().AsNoTracking().Where(x => !x.IsDelete && x.LeaveTypeId == normalizedLeaveTypeId && x.ReasonName.ToLower() == name);
            if (excludeId.HasValue) duplicate = duplicate.Where(x => x.Id != excludeId.Value);
            return await duplicate.AnyAsync() ? "Nama alasan adjustment sudah digunakan untuk leave type tersebut." : null;
        }

        private static LeaveAdjustmentReasonResponse MapResponse(MstLeaveAdjustmentReason x, IReadOnlyDictionary<Guid, string?> actors) => new()
        {
            Id = x.Id, LeaveTypeId = x.LeaveTypeId, LeaveTypeCode = x.LeaveType?.LeaveTypeCode, LeaveTypeName = x.LeaveType?.LeaveTypeName,
            ReasonCode = x.ReasonCode, ReasonName = x.ReasonName, ReasonCategory = x.ReasonCategory, AllowedDirection = x.AllowedDirection,
            AllowOpeningBalance = x.AllowOpeningBalance, AllowManualAdjustment = x.AllowManualAdjustment, AllowCorrection = x.AllowCorrection,
            AllowReversal = x.AllowReversal, MaximumAdjustmentDays = x.MaximumAdjustmentDays, RequiresComment = x.RequiresComment,
            RequiresAttachment = x.RequiresAttachment, RequiresApproval = x.RequiresApproval, ApprovalWorkflowCode = x.ApprovalWorkflowCode,
            SortOrder = x.SortOrder, EffectiveStartDate = x.EffectiveStartDate, EffectiveEndDate = x.EffectiveEndDate,
            Description = x.Description, IsActive = x.IsActive, CreateDateTime = x.CreateDateTime,
            CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy, CreateByName = GetActorName(actors, x.CreateBy)
        };

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstLeaveAdjustmentReason>().AsNoTracking()
                .Where(x => !x.IsDelete && x.ReasonCode.StartsWith(CodePrefix)).Select(x => x.ReasonCode).ToListAsync();
            var used = codes.Select(x => x.Replace(CodePrefix, string.Empty)).Where(x => int.TryParse(x, out _)).Select(int.Parse).ToHashSet();
            var next = 1;
            while (used.Contains(next)) next++;
            return CodePrefix + next.ToString().PadLeft(CodeNumberLength, '0');
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> ids)
        {
            var values = ids.Where(x => x != Guid.Empty).Distinct().ToList();
            return await _dbContext.Users.AsNoTracking().Where(x => values.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> map, Guid id) =>
            id == Guid.Empty ? null : map.TryGetValue(id, out var value) ? value : null;

        private static void NormalizePaging(ref int pageNumber, ref int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 25 : Math.Min(pageSize, 100);
        }

        private static Guid? NormalizeGuid(Guid? value) => !value.HasValue || value == Guid.Empty ? null : value;
        private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static List<LeaveAdjustmentReasonCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<LeaveAdjustmentReasonCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }

    public sealed class LeaveAdjustmentReasonServiceResult<T> where T : class
    {
        private LeaveAdjustmentReasonServiceResult(T? data, string? errorMessage, bool isNotFound)
        {
            Data = data;
            ErrorMessage = errorMessage;
            IsNotFound = isNotFound;
        }

        public T? Data { get; }
        public string? ErrorMessage { get; }
        public bool IsNotFound { get; }
        public bool IsSuccess => Data != null;

        public static LeaveAdjustmentReasonServiceResult<T> Success(T data) => new(data, null, false);
        public static LeaveAdjustmentReasonServiceResult<T> Invalid(string errorMessage) => new(null, errorMessage, false);
        public static LeaveAdjustmentReasonServiceResult<T> NotFound() => new(null, null, true);
    }
}
