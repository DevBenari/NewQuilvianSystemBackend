using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Services
{
    /// <summary>
    /// Master pos beban biaya operasional (<c>Hrd.Expense.Category.Manage</c>, BE-HRD-333).
    /// </summary>
    public class ExpenseCategoryService
    {
        private const string SequenceKey = "HRD_EXPENSE_CATEGORY";
        private const string SequencePrefix = "EXC";

        private readonly ApplicationDbContext _context;
        private readonly NumberSeriesAllocator _numberAllocator;

        public ExpenseCategoryService(ApplicationDbContext context, NumberSeriesAllocator numberAllocator)
        {
            _context = context;
            _numberAllocator = numberAllocator;
        }

        public async Task<ExpenseCategoryFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken ct)
        {
            var types = await _context.MstExpenseCategories.AsNoTracking()
                .Where(x => !x.IsDelete)
                .Select(x => x.CategoryType)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(ct);
            return new ExpenseCategoryFilterMetadataResponse { CategoryTypes = types };
        }

        public async Task<ExpenseCategorySummaryResponse> GetSummaryAsync(CancellationToken ct)
        {
            var query = _context.MstExpenseCategories.AsNoTracking().Where(x => !x.IsDelete);
            var total = await query.CountAsync(ct);
            var active = await query.CountAsync(x => x.IsActive, ct);
            var travel = await query.CountAsync(x => x.IsTravelRelated, ct);
            return new ExpenseCategorySummaryResponse { Total = total, Active = active, Inactive = total - active, TravelRelated = travel };
        }

        public async Task<PagedResult<ExpenseCategoryResponse>> GetListAsync(string? search, string? categoryType, bool? isActive, int pageNumber, int pageSize, CancellationToken ct)
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _context.MstExpenseCategories.AsNoTracking().Include(x => x.ParentExpenseCategory).Where(x => !x.IsDelete);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(x => x.ExpenseCategoryCode.ToLower().Contains(term) || x.ExpenseCategoryName.ToLower().Contains(term));
            }
            if (!string.IsNullOrWhiteSpace(categoryType)) query = query.Where(x => x.CategoryType == categoryType);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

            var total = await query.CountAsync(ct);
            var rows = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.ExpenseCategoryName)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return new PagedResult<ExpenseCategoryResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = rows.Select(Map).ToList()
            };
        }

        public Task<List<ExpenseCategoryOptionResponse>> GetOptionsAsync(CancellationToken ct)
        {
            return _context.MstExpenseCategories.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .OrderBy(x => x.SortOrder).ThenBy(x => x.ExpenseCategoryName)
                .Select(x => new ExpenseCategoryOptionResponse { Id = x.Id, Code = x.ExpenseCategoryCode, Name = x.ExpenseCategoryName })
                .ToListAsync(ct);
        }

        public async Task<ExpenseCategoryResponse> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var row = await _context.MstExpenseCategories.AsNoTracking().Include(x => x.ParentExpenseCategory)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            return row == null ? throw new KeyNotFoundException("Kategori biaya tidak ditemukan.") : Map(row);
        }

        public async Task<ExpenseCategoryResponse> CreateAsync(ExpenseCategoryRequest request, Guid actorUserId, CancellationToken ct)
        {
            await ValidateParentAsync(null, request.ParentExpenseCategoryId, ct);

            var code = string.IsNullOrWhiteSpace(request.ExpenseCategoryCode)
                ? await _numberAllocator.AllocateAsync(new NumberAllocationRequest(
                    SequenceKey: SequenceKey,
                    Prefix: SequencePrefix,
                    ResetPolicy: NumberSeriesResetPolicies.Never,
                    SequenceDigits: 5,
                    ActorUserId: actorUserId,
                    Instant: DateTimeOffset.UtcNow), ct)
                : request.ExpenseCategoryCode.Trim();

            await EnsureCodeUniqueAsync(code, null, ct);

            var entity = new MstExpenseCategory { Id = Guid.NewGuid(), ExpenseCategoryCode = code, IsActive = true, CreateBy = actorUserId, CreateDateTime = DateTime.UtcNow };
            Apply(entity, request);
            _context.MstExpenseCategories.Add(entity);
            await _context.SaveChangesAsync(ct);
            return await GetByIdAsync(entity.Id, ct);
        }

        public async Task<ExpenseCategoryResponse> UpdateAsync(Guid id, ExpenseCategoryRequest request, Guid actorUserId, CancellationToken ct)
        {
            var entity = await FindAsync(id, ct);
            await ValidateParentAsync(id, request.ParentExpenseCategoryId, ct);

            if (!string.IsNullOrWhiteSpace(request.ExpenseCategoryCode))
            {
                var code = request.ExpenseCategoryCode.Trim();
                await EnsureCodeUniqueAsync(code, id, ct);
                entity.ExpenseCategoryCode = code;
            }

            Apply(entity, request);
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            return await GetByIdAsync(id, ct);
        }

        public async Task<ExpenseCategoryResponse> SetStatusAsync(Guid id, bool isActive, Guid actorUserId, CancellationToken ct)
        {
            var entity = await FindAsync(id, ct);
            entity.IsActive = isActive;
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            return await GetByIdAsync(id, ct);
        }

        public async Task DeleteAsync(Guid id, Guid actorUserId, CancellationToken ct)
        {
            var entity = await FindAsync(id, ct);

            var isUsed = await _context.TrxExpenseClaimItems.AsNoTracking().AnyAsync(x => x.ExpenseCategoryId == id && !x.IsDelete, ct)
                || await _context.MstExpenseCategories.AsNoTracking().AnyAsync(x => x.ParentExpenseCategoryId == id && !x.IsDelete, ct);
            if (isUsed)
            {
                throw new ExpenseCategoryConflictException("Kategori biaya masih dipakai klaim atau menjadi induk kategori lain. Nonaktifkan saja bila tidak ingin dipakai lagi.");
            }

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteBy = actorUserId;
            entity.DeleteDateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }

        private async Task<MstExpenseCategory> FindAsync(Guid id, CancellationToken ct)
        {
            return await _context.MstExpenseCategories.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct)
                ?? throw new KeyNotFoundException("Kategori biaya tidak ditemukan.");
        }

        private async Task EnsureCodeUniqueAsync(string code, Guid? exceptId, CancellationToken ct)
        {
            var exists = await _context.MstExpenseCategories.AsNoTracking()
                .AnyAsync(x => x.ExpenseCategoryCode == code && !x.IsDelete && (exceptId == null || x.Id != exceptId), ct);
            if (exists) throw new ExpenseCategoryConflictException($"Kode kategori biaya '{code}' sudah dipakai.");
        }

        private async Task ValidateParentAsync(Guid? selfId, Guid? parentId, CancellationToken ct)
        {
            if (parentId == null) return;
            if (parentId == selfId) throw new ArgumentException("Kategori biaya tidak boleh menjadi induk dirinya sendiri.");
            var exists = await _context.MstExpenseCategories.AsNoTracking().AnyAsync(x => x.Id == parentId && !x.IsDelete, ct);
            if (!exists) throw new ArgumentException("Kategori induk tidak ditemukan.");
        }

        private static void Apply(MstExpenseCategory entity, ExpenseCategoryRequest request)
        {
            entity.ExpenseCategoryName = request.ExpenseCategoryName.Trim();
            entity.ParentExpenseCategoryId = request.ParentExpenseCategoryId;
            entity.CategoryType = request.CategoryType.Trim();
            entity.IsTravelRelated = request.IsTravelRelated;
            entity.IsMedicalBenefitRelated = request.IsMedicalBenefitRelated;
            entity.IsTrainingRelated = request.IsTrainingRelated;
            entity.RequiresReceipt = request.RequiresReceipt;
            entity.AllowWithoutReceipt = request.AllowWithoutReceipt;
            entity.IsReimbursable = request.IsReimbursable;
            entity.IsTaxable = request.IsTaxable;
            entity.RequireCostCenter = request.RequireCostCenter;
            entity.AllowSplitAllocation = request.AllowSplitAllocation;
            entity.DefaultMaximumAmount = request.DefaultMaximumAmount;
            entity.SortOrder = request.SortOrder;
            entity.Description = request.Description;
        }

        private static ExpenseCategoryResponse Map(MstExpenseCategory x) => new()
        {
            Id = x.Id,
            ExpenseCategoryCode = x.ExpenseCategoryCode,
            ExpenseCategoryName = x.ExpenseCategoryName,
            ParentExpenseCategoryId = x.ParentExpenseCategoryId,
            ParentExpenseCategoryName = x.ParentExpenseCategory?.ExpenseCategoryName,
            CategoryType = x.CategoryType,
            IsTravelRelated = x.IsTravelRelated,
            IsMedicalBenefitRelated = x.IsMedicalBenefitRelated,
            IsTrainingRelated = x.IsTrainingRelated,
            RequiresReceipt = x.RequiresReceipt,
            AllowWithoutReceipt = x.AllowWithoutReceipt,
            IsReimbursable = x.IsReimbursable,
            IsTaxable = x.IsTaxable,
            RequireCostCenter = x.RequireCostCenter,
            AllowSplitAllocation = x.AllowSplitAllocation,
            DefaultMaximumAmount = x.DefaultMaximumAmount,
            SortOrder = x.SortOrder,
            Description = x.Description,
            IsActive = x.IsActive,
            CreateDateTime = x.CreateDateTime,
            UpdateDateTime = x.UpdateDateTime
        };
    }
}
