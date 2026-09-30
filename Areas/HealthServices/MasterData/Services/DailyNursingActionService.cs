using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services
{
    /// <summary>
    /// Layanan pengelolaan Master Data Tindakan Harian Keperawatan Rawat Inap (Daily Nursing Actions).
    /// </summary>
    public class DailyNursingActionService
    {
        private readonly ApplicationDbContext _dbContext;

        public DailyNursingActionService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DailyNursingActionSummaryResponse> GetSummaryAsync(CancellationToken ct = default)
        {
            var total = await _dbContext.MstDailyNursingActions.CountAsync(x => !x.IsDelete, ct);
            var active = await _dbContext.MstDailyNursingActions.CountAsync(x => !x.IsDelete && x.IsActive, ct);
            var categories = await _dbContext.MstDailyNursingActions
                .Where(x => !x.IsDelete)
                .Select(x => x.Category)
                .Distinct()
                .CountAsync(ct);

            return new DailyNursingActionSummaryResponse
            {
                TotalCount = total,
                ActiveCount = active,
                InactiveCount = total - active,
                CategoryCount = categories
            };
        }

        public async Task<PagedResult<DailyNursingActionListItemDto>> GetPagedListAsync(
            string? search,
            string? category,
            bool? isActive,
            int page = 1,
            int perPage = 25,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 25;
            if (perPage > 100) perPage = 100;

            var query = _dbContext.MstDailyNursingActions
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(x =>
                    x.ActionCode.ToLower().Contains(s) ||
                    x.ActionName.ToLower().Contains(s) ||
                    (x.DefaultNotes != null && x.DefaultNotes.ToLower().Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                var c = category.Trim();
                query = query.Where(x => x.Category == c);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            var totalData = await query.CountAsync(ct);

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ActionName)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .Select(x => new DailyNursingActionListItemDto
                {
                    Id = x.Id,
                    ActionCode = x.ActionCode,
                    ActionName = x.ActionName,
                    Category = x.Category,
                    DefaultNotes = x.DefaultNotes,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                })
                .ToListAsync(ct);

            return new PagedResult<DailyNursingActionListItemDto>
            {
                PageNumber = page,
                PageSize = perPage,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)perPage),
                Items = items
            };
        }

        public async Task<List<DailyNursingActionActiveItemDto>> GetActiveListAsync(
            string? category = null,
            CancellationToken ct = default)
        {
            var query = _dbContext.MstDailyNursingActions
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive);

            if (!string.IsNullOrWhiteSpace(category))
            {
                var c = category.Trim();
                query = query.Where(x => x.Category == c);
            }

            return await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ActionName)
                .Select(x => new DailyNursingActionActiveItemDto
                {
                    Id = x.Id,
                    ActionCode = x.ActionCode,
                    ActionName = x.ActionName,
                    Category = x.Category,
                    DefaultNotes = x.DefaultNotes,
                    SortOrder = x.SortOrder
                })
                .ToListAsync(ct);
        }

        public async Task<DailyNursingActionDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContext.MstDailyNursingActions
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new DailyNursingActionDetailDto
                {
                    Id = x.Id,
                    ActionCode = x.ActionCode,
                    ActionName = x.ActionName,
                    Category = x.Category,
                    DefaultNotes = x.DefaultNotes,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime,
                    CreateBy = x.CreateBy,
                    UpdateBy = x.UpdateBy
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<(bool Success, string? ErrorMessage, DailyNursingActionDetailDto? Result)> CreateAsync(
            CreateDailyNursingActionRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var code = request.ActionCode.Trim().ToUpper();
            var exists = await _dbContext.MstDailyNursingActions
                .AnyAsync(x => x.ActionCode.ToUpper() == code && !x.IsDelete, ct);

            if (exists)
            {
                return (false, $"Kode tindakan '{code}' sudah digunakan.", null);
            }

            var now = DateTime.UtcNow;
            var entity = new MstDailyNursingAction
            {
                Id = Guid.NewGuid(),
                ActionCode = code,
                ActionName = request.ActionName.Trim(),
                Category = request.Category.Trim(),
                DefaultNotes = string.IsNullOrWhiteSpace(request.DefaultNotes) ? null : request.DefaultNotes.Trim(),
                SortOrder = request.SortOrder,
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false
            };

            _dbContext.MstDailyNursingActions.Add(entity);
            await _dbContext.SaveChangesAsync(ct);

            var result = new DailyNursingActionDetailDto
            {
                Id = entity.Id,
                ActionCode = entity.ActionCode,
                ActionName = entity.ActionName,
                Category = entity.Category,
                DefaultNotes = entity.DefaultNotes,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy
            };

            return (true, null, result);
        }

        public async Task<(bool Success, string? ErrorMessage, DailyNursingActionDetailDto? Result)> UpdateAsync(
            Guid id,
            UpdateDailyNursingActionRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var entity = await _dbContext.MstDailyNursingActions
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (entity == null)
            {
                return (false, "Master tindakan harian tidak ditemukan.", null);
            }

            entity.ActionName = request.ActionName.Trim();
            entity.Category = request.Category.Trim();
            entity.DefaultNotes = string.IsNullOrWhiteSpace(request.DefaultNotes) ? null : request.DefaultNotes.Trim();
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(ct);

            var result = new DailyNursingActionDetailDto
            {
                Id = entity.Id,
                ActionCode = entity.ActionCode,
                ActionName = entity.ActionName,
                Category = entity.Category,
                DefaultNotes = entity.DefaultNotes,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                UpdateDateTime = entity.UpdateDateTime,
                CreateBy = entity.CreateBy,
                UpdateBy = entity.UpdateBy
            };

            return (true, null, result);
        }

        public async Task<(bool Success, string? ErrorMessage)> ToggleActiveAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var entity = await _dbContext.MstDailyNursingActions
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (entity == null)
            {
                return (false, "Master tindakan harian tidak ditemukan.");
            }

            entity.IsActive = !entity.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(ct);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var entity = await _dbContext.MstDailyNursingActions
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (entity == null)
            {
                return (false, "Master tindakan harian tidak ditemukan.");
            }

            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = actorUserId;

            await _dbContext.SaveChangesAsync(ct);
            return (true, null);
        }
    }
}
