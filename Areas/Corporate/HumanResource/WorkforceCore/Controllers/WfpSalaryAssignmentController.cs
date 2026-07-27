using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using SalaryAssignmentPagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs.WfpSalaryAssignmentResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/salary-assignments")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Workforce Salary Assignment",
        AreaName = "Corporate",
        ControllerName = "WfpSalaryAssignment",
        Description = "Corporate human resource workforce salary assignment",
        SortOrder = 10)]
    [Tags("Corporate / Human Resource / Workforce Core / Salary Assignment")]
    public class WfpSalaryAssignmentController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.WorkforceCore";
        private static readonly string[] PaymentFrequencies = { "Monthly", "Weekly", "Biweekly", "Daily", "Hourly" };
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpSalaryAssignmentController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Workforce Salary Assignment", Description = "Melihat metadata salary assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpSalaryAssignment", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new WfpSalaryAssignmentFilterMetadataResponse
            {
                DefaultFilter = new WfpSalaryAssignmentDefaultFilterResponse(),
                CustomPeriods = BuildPeriods(),
                PaymentFrequencyOptions = PaymentFrequencies.Select(x => new WfpSalaryAssignmentStringOptionResponse { Value = x, Label = x }).ToList(),
                CurrencyOptions = new List<WfpSalaryAssignmentStringOptionResponse>
                {
                    new() { Value = "IDR", Label = "IDR" },
                    new() { Value = "USD", Label = "USD" },
                    new() { Value = "SGD", Label = "SGD" },
                    new() { Value = "EUR", Label = "EUR" }
                },
                SortOptions = new List<WfpSalaryAssignmentStringOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal efektif" },
                    new() { Value = "baseSalary", Label = "Gaji pokok" },
                    new() { Value = "paymentFrequency", Label = "Frekuensi pembayaran" },
                    new() { Value = "isPrimary", Label = "Primary" },
                    new() { Value = "approvedAt", Label = "Tanggal approval" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpSalaryAssignmentFilterMetadataResponse>.Ok(result, "Metadata filter salary assignment berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Workforce Salary Assignment", Description = "Melihat ringkasan salary assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpSalaryAssignment", "Read")]
        public async Task<IActionResult> GetSummary(Guid workforceProfileId, CancellationToken ct)
        {
            if (!await WorkforceExistsAsync(workforceProfileId, ct)) return WorkforceNotFound();

            var query = _dbContext.Set<WfpSalaryAssignment>().AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var result = new WfpSalaryAssignmentSummaryResponse
            {
                TotalData = await query.CountAsync(ct),
                ActiveData = await query.CountAsync(x => x.IsActive, ct),
                InactiveData = await query.CountAsync(x => !x.IsActive, ct),
                PrimaryData = await query.CountAsync(x => x.IsPrimary && x.IsActive, ct),
                ApprovedData = await query.CountAsync(x => x.ApprovedAt.HasValue && x.ApprovedByUserId.HasValue, ct),
                PendingApprovalData = await query.CountAsync(x => !x.ApprovedAt.HasValue || !x.ApprovedByUserId.HasValue, ct)
            };

            return Ok(ApiResponse<WfpSalaryAssignmentSummaryResponse>.Ok(result, "Ringkasan salary assignment berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Workforce Salary Assignment", Description = "Melihat salary assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpSalaryAssignment", "Read")]
        public async Task<IActionResult> GetAll(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? salaryStructureId,
            [FromQuery] Guid? salaryGradeId,
            [FromQuery] string? paymentFrequency,
            [FromQuery] string? currencyCode,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isApproved,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "effectiveStartDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken ct = default)
        {
            if (!await WorkforceExistsAsync(workforceProfileId, ct)) return WorkforceNotFound();
            NormalizePaging(ref pageNumber, ref pageSize);

            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);
            if (!dateRange.IsValid) return BadRequest(ApiResponse<object>.Fail(400, dateRange.ErrorMessage!));

            var query = BuildBaseQuery(workforceProfileId);
            if (dateRange.Start.HasValue) query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            if (dateRange.EndExclusive.HasValue) query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            if (salaryStructureId.HasValue && salaryStructureId.Value != Guid.Empty) query = query.Where(x => x.SalaryStructureId == salaryStructureId.Value);
            if (salaryGradeId.HasValue && salaryGradeId.Value != Guid.Empty) query = query.Where(x => x.SalaryGradeId == salaryGradeId.Value);
            if (!string.IsNullOrWhiteSpace(paymentFrequency)) query = query.Where(x => x.PaymentFrequency == NormalizeFrequency(paymentFrequency));
            if (!string.IsNullOrWhiteSpace(currencyCode)) query = query.Where(x => x.CurrencyCode == currencyCode.Trim().ToUpperInvariant());
            if (isPrimary.HasValue) query = query.Where(x => x.IsPrimary == isPrimary.Value);
            if (isApproved.HasValue)
                query = isApproved.Value
                    ? query.Where(x => x.ApprovedAt.HasValue && x.ApprovedByUserId.HasValue)
                    : query.Where(x => !x.ApprovedAt.HasValue || !x.ApprovedByUserId.HasValue);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.CurrencyCode.ToLower().Contains(keyword) ||
                    x.PaymentFrequency.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.WorkforceProfile != null && (x.WorkforceProfile.ProfileCode.ToLower().Contains(keyword) || x.WorkforceProfile.DisplayName.ToLower().Contains(keyword))));
            }

            var totalData = await query.CountAsync(ct);
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            var actorNames = await GetActorNamesAsync(entities.SelectMany(x => new[] { x.CreateBy, x.UpdateBy, x.ApprovedByUserId ?? Guid.Empty }), ct);
            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

            return Ok(ApiResponse<SalaryAssignmentPagedResult>.Ok(new SalaryAssignmentPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data salary assignment berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Workforce Salary Assignment", Description = "Melihat detail salary assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpSalaryAssignment", "Read")]
        public async Task<IActionResult> GetById(Guid workforceProfileId, Guid id, CancellationToken ct)
        {
            var entity = await BuildBaseQuery(workforceProfileId).FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Salary assignment tidak ditemukan."));
            var actorNames = await GetActorNamesAsync(new[] { entity.CreateBy, entity.UpdateBy, entity.ApprovedByUserId ?? Guid.Empty }, ct);
            return Ok(ApiResponse<WfpSalaryAssignmentResponse>.Ok(MapResponse(entity, actorNames), "Detail salary assignment berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Workforce Salary Assignment", Description = "Membuat salary assignment", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WfpSalaryAssignment", "Create")]
        public async Task<IActionResult> Create(Guid workforceProfileId, [FromBody] CreateWfpSalaryAssignmentRequest request, CancellationToken ct)
        {
            if (!await WorkforceExistsAsync(workforceProfileId, ct)) return WorkforceNotFound();
            var validation = await ValidateRequestAsync(workforceProfileId, null, request, ct);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                if (request.IsPrimary && request.IsActive) await ClearPrimaryAsync(workforceProfileId, null, actor, now, ct);

                var entity = new WfpSalaryAssignment
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    SalaryStructureId = request.SalaryStructureId,
                    SalaryGradeId = request.SalaryGradeId,
                    EmployeeGradeId = NormalizeGuid(request.EmployeeGradeId),
                    PayrollPeriodId = NormalizeGuid(request.PayrollPeriodId),
                    BaseSalary = request.BaseSalary,
                    CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
                    PaymentFrequency = NormalizeFrequency(request.PaymentFrequency),
                    EffectiveStartDate = request.EffectiveStartDate.Date,
                    EffectiveEndDate = request.EffectiveEndDate?.Date,
                    IsPrimary = request.IsPrimary,
                    IsConfidential = request.IsConfidential,
                    IsActive = request.IsActive,
                    Description = Normalize(request.Description),
                    CreateDateTime = now,
                    CreateBy = actor,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<WfpSalaryAssignment>().Add(entity);
                await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                await _loggerService.InfoAsync(LogCategory, "WfpSalaryAssignment.Create", "Salary assignment berhasil dibuat.", new { entity.Id, entity.WorkforceProfileId, entity.BaseSalary, entity.IsPrimary });
                return Ok(ApiResponse<object>.Ok(new { entity.Id }, "Salary assignment berhasil dibuat."));
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Workforce Salary Assignment", Description = "Mengubah salary assignment", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpSalaryAssignment", "Update")]
        public async Task<IActionResult> Update(Guid workforceProfileId, Guid id, [FromBody] UpdateWfpSalaryAssignmentRequest request, CancellationToken ct)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Salary assignment tidak ditemukan."));
            var validation = await ValidateRequestAsync(workforceProfileId, id, request, ct);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                if (request.IsPrimary && request.IsActive) await ClearPrimaryAsync(workforceProfileId, id, actor, now, ct);
                entity.SalaryStructureId = request.SalaryStructureId;
                entity.SalaryGradeId = request.SalaryGradeId;
                entity.EmployeeGradeId = NormalizeGuid(request.EmployeeGradeId);
                entity.PayrollPeriodId = NormalizeGuid(request.PayrollPeriodId);
                entity.BaseSalary = request.BaseSalary;
                entity.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
                entity.PaymentFrequency = NormalizeFrequency(request.PaymentFrequency);
                entity.EffectiveStartDate = request.EffectiveStartDate.Date;
                entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
                entity.IsPrimary = request.IsPrimary;
                entity.IsConfidential = request.IsConfidential;
                entity.IsActive = request.IsActive;
                entity.Description = Normalize(request.Description);
                entity.UpdateDateTime = now;
                entity.UpdateBy = actor;
                await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return Ok(ApiResponse<object>.Ok(null, "Salary assignment berhasil diperbarui."));
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        [HttpPatch("{id:guid}/approval")]
        [AccessAction("Update", "Approve Workforce Salary Assignment", Description = "Melakukan approval salary assignment", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpSalaryAssignment", "Update")]
        public async Task<IActionResult> Approve(Guid workforceProfileId, Guid id, [FromBody] ApproveWfpSalaryAssignmentRequest request, CancellationToken ct)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Salary assignment tidak ditemukan."));
            var actor = GetCurrentUserId();
            var now = DateTime.UtcNow;
            entity.ApprovedByUserId = request.IsApproved ? actor : null;
            entity.ApprovedAt = request.IsApproved ? now : null;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null, request.IsApproved ? "Salary assignment berhasil disetujui." : "Approval salary assignment berhasil dibatalkan."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Workforce Salary Assignment", Description = "Mengubah status salary assignment", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpSalaryAssignment", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid workforceProfileId, Guid id, [FromBody] UpdateWfpSalaryAssignmentStatusRequest request, CancellationToken ct)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Salary assignment tidak ditemukan."));
            entity.IsActive = request.IsActive;
            if (!request.IsActive) entity.IsPrimary = false;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null, "Status salary assignment berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/primary")]
        [AccessAction("Update", "Update Workforce Salary Assignment", Description = "Mengatur primary salary assignment", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpSalaryAssignment", "Update")]
        public async Task<IActionResult> SetPrimary(Guid workforceProfileId, Guid id, [FromBody] SetWfpSalaryAssignmentPrimaryRequest request, CancellationToken ct)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Salary assignment tidak ditemukan."));
            if (request.IsPrimary && !entity.IsActive) return BadRequest(ApiResponse<object>.Fail(400, "Salary assignment tidak aktif tidak dapat dijadikan primary."));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                if (request.IsPrimary) await ClearPrimaryAsync(workforceProfileId, id, actor, now, ct);
                entity.IsPrimary = request.IsPrimary;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actor;
                await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return Ok(ApiResponse<object>.Ok(null, "Primary salary assignment berhasil diperbarui."));
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Workforce Salary Assignment", Description = "Menghapus salary assignment", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WfpSalaryAssignment", "Delete")]
        public async Task<IActionResult> Delete(Guid workforceProfileId, Guid id, CancellationToken ct)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Salary assignment tidak ditemukan."));
            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsPrimary = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null, "Salary assignment berhasil dihapus."));
        }

        private IQueryable<WfpSalaryAssignment> BuildBaseQuery(Guid workforceProfileId) =>
            _dbContext.Set<WfpSalaryAssignment>().AsNoTracking().Include(x => x.WorkforceProfile)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

        private static IOrderedQueryable<WfpSalaryAssignment> ApplySorting(IQueryable<WfpSalaryAssignment> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "effectiveStartDate").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "basesalary" => desc ? query.OrderByDescending(x => x.BaseSalary) : query.OrderBy(x => x.BaseSalary),
                "paymentfrequency" => desc ? query.OrderByDescending(x => x.PaymentFrequency) : query.OrderBy(x => x.PaymentFrequency),
                "isprimary" => desc ? query.OrderByDescending(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate),
                "approvedat" => desc ? query.OrderByDescending(x => x.ApprovedAt) : query.OrderBy(x => x.ApprovedAt),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate),
                _ => desc ? query.OrderByDescending(x => x.EffectiveStartDate).ThenByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.EffectiveStartDate).ThenBy(x => x.CreateDateTime)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid workforceProfileId, Guid? excludeId, CreateWfpSalaryAssignmentRequest request, CancellationToken ct)
        {
            if (request.SalaryStructureId == Guid.Empty) return (false, "Salary structure wajib dipilih.");
            if (request.SalaryGradeId == Guid.Empty) return (false, "Salary grade wajib dipilih.");
            if (request.BaseSalary < 0) return (false, "Base salary tidak boleh negatif.");
            if (string.IsNullOrWhiteSpace(request.CurrencyCode) || request.CurrencyCode.Trim().Length != 3) return (false, "Currency code wajib terdiri dari 3 karakter.");
            if (!PaymentFrequencies.Contains(NormalizeFrequency(request.PaymentFrequency), StringComparer.OrdinalIgnoreCase)) return (false, "Payment frequency tidak valid.");
            if (request.EffectiveStartDate == default) return (false, "Effective start date wajib diisi.");
            if (request.EffectiveEndDate.HasValue && request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Date) return (false, "Effective end date tidak boleh lebih kecil dari effective start date.");
            if (!await ExistsActiveAsync<MstSalaryStructure>(request.SalaryStructureId, ct)) return (false, "Salary structure tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveAsync<MstSalaryGrade>(request.SalaryGradeId, ct)) return (false, "Salary grade tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstEmployeeGrade>(request.EmployeeGradeId, ct)) return (false, "Employee grade tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstPayrollPeriod>(request.PayrollPeriodId, ct)) return (false, "Payroll period tidak ditemukan atau tidak aktif.");

            var overlaps = await _dbContext.Set<WfpSalaryAssignment>().AsNoTracking().AnyAsync(x =>
                x.WorkforceProfileId == workforceProfileId && x.Id != excludeId && !x.IsDelete &&
                x.EffectiveStartDate <= (request.EffectiveEndDate ?? DateTime.MaxValue).Date &&
                (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= request.EffectiveStartDate.Date) &&
                x.SalaryStructureId == request.SalaryStructureId && x.SalaryGradeId == request.SalaryGradeId, ct);
            if (overlaps) return (false, "Periode salary assignment dengan struktur dan grade yang sama beririsan.");
            return (true, null);
        }

        private async Task ClearPrimaryAsync(Guid workforceProfileId, Guid? excludeId, Guid actor, DateTime now, CancellationToken ct)
        {
            var rows = await _dbContext.Set<WfpSalaryAssignment>().Where(x => x.WorkforceProfileId == workforceProfileId && x.IsPrimary && x.IsActive && !x.IsDelete && x.Id != excludeId).ToListAsync(ct);
            foreach (var row in rows) { row.IsPrimary = false; row.UpdateDateTime = now; row.UpdateBy = actor; }
        }

        private async Task<bool> WorkforceExistsAsync(Guid id, CancellationToken ct) =>
            await _dbContext.Set<MstWorkforceProfile>().AsNoTracking().AnyAsync(x => x.Id == id && x.IsActive && !x.IsDelete, ct);

        private IActionResult WorkforceNotFound() => NotFound(ApiResponse<object>.Fail(404, "Workforce profile tidak ditemukan atau sudah tidak aktif."));

        private async Task<WfpSalaryAssignment?> FindEntityAsync(Guid workforceProfileId, Guid id, CancellationToken ct) =>
            await _dbContext.Set<WfpSalaryAssignment>().FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, ct);

        private static WfpSalaryAssignmentResponse MapResponse(WfpSalaryAssignment x, IReadOnlyDictionary<Guid, string?> actorNames) => new()
        {
            Id = x.Id,
            WorkforceProfileId = x.WorkforceProfileId,
            WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
            WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
            SalaryStructureId = x.SalaryStructureId,
            SalaryGradeId = x.SalaryGradeId,
            EmployeeGradeId = x.EmployeeGradeId,
            PayrollPeriodId = x.PayrollPeriodId,
            BaseSalary = x.BaseSalary,
            CurrencyCode = x.CurrencyCode,
            PaymentFrequency = x.PaymentFrequency,
            EffectiveStartDate = x.EffectiveStartDate,
            EffectiveEndDate = x.EffectiveEndDate,
            IsPrimary = x.IsPrimary,
            IsConfidential = x.IsConfidential,
            IsActive = x.IsActive,
            ApprovedByUserId = x.ApprovedByUserId,
            ApprovedByUserName = x.ApprovedByUserId.HasValue ? GetActorName(actorNames, x.ApprovedByUserId.Value) : null,
            ApprovedAt = x.ApprovedAt,
            IsApproved = x.ApprovedAt.HasValue && x.ApprovedByUserId.HasValue,
            Description = x.Description,
            CreateDateTime = x.CreateDateTime,
            CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
            CreateByName = GetActorName(actorNames, x.CreateBy),
            UpdateDateTime = x.UpdateDateTime,
            UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
            UpdateByName = GetActorName(actorNames, x.UpdateBy)
        };

        private async Task<Dictionary<Guid, string?>> GetActorNamesAsync(IEnumerable<Guid> ids, CancellationToken ct)
        {
            var actorIds = ids.Where(x => x != Guid.Empty).Distinct().ToList();
            return await _dbContext.Users.AsNoTracking().Where(x => actorIds.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> map, Guid id) => id == Guid.Empty ? null : map.TryGetValue(id, out var name) ? name : null;
        private Guid GetCurrentUserId() { var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id"); return Guid.TryParse(value, out var id) ? id : Guid.Empty; }
        private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static Guid? NormalizeGuid(Guid? value) => !value.HasValue || value.Value == Guid.Empty ? null : value.Value;
        private static string NormalizeFrequency(string value) => PaymentFrequencies.FirstOrDefault(x => x.Equals(value?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? value?.Trim() ?? string.Empty;
        private static void NormalizePaging(ref int pageNumber, ref int pageSize) { pageNumber = pageNumber <= 0 ? 1 : pageNumber; pageSize = pageSize <= 0 ? 25 : Math.Min(pageSize, 100); }

        private async Task<bool> ExistsActiveAsync<TEntity>(Guid id, CancellationToken ct) where TEntity : IdentityModel =>
            await _dbContext.Set<TEntity>().AsNoTracking().AnyAsync(x => EF.Property<Guid>(x, "Id") == id && EF.Property<bool>(x, "IsActive") && !EF.Property<bool>(x, "IsDelete"), ct);

        private async Task<bool> ExistsActiveIfProvidedAsync<TEntity>(Guid? id, CancellationToken ct) where TEntity : IdentityModel =>
            !id.HasValue || id.Value == Guid.Empty || await ExistsActiveAsync<TEntity>(id.Value, ct);

        private static DateRangeResult ResolveDateRange(DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            if (!string.IsNullOrWhiteSpace(customPeriod) && !customPeriod.Equals("custom", StringComparison.OrdinalIgnoreCase))
            {
                var today = DateTime.UtcNow.Date;
                return customPeriod.Trim().ToLowerInvariant() switch
                {
                    "today" => DateRangeResult.Valid(today, today.AddDays(1)),
                    "last7days" => DateRangeResult.Valid(today.AddDays(-6), today.AddDays(1)),
                    "last30days" => DateRangeResult.Valid(today.AddDays(-29), today.AddDays(1)),
                    "thismonth" => DateRangeResult.Valid(new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                    _ => DateRangeResult.Invalid("Custom period tidak dikenali.")
                };
            }
            DateTime? start = startDate.HasValue ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc) : null;
            DateTime? endExclusive = endDate.HasValue ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc) : null;
            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value) return DateRangeResult.Invalid("StartDate tidak boleh lebih besar atau sama dengan EndDate.");
            return DateRangeResult.Valid(start, endExclusive);
        }

        private static List<WfpSalaryAssignmentStringOptionResponse> BuildPeriods() => new()
        {
            new() { Value = "custom", Label = "Custom" },
            new() { Value = "today", Label = "Hari ini" },
            new() { Value = "last7days", Label = "7 hari terakhir" },
            new() { Value = "last30days", Label = "30 hari terakhir" },
            new() { Value = "thismonth", Label = "Bulan ini" }
        };

        private sealed class DateRangeResult
        {
            public bool IsValid { get; private init; }
            public DateTime? Start { get; private init; }
            public DateTime? EndExclusive { get; private init; }
            public string? ErrorMessage { get; private init; }
            public static DateRangeResult Valid(DateTime? start, DateTime? end) => new() { IsValid = true, Start = start, EndExclusive = end };
            public static DateRangeResult Invalid(string message) => new() { IsValid = false, ErrorMessage = message };
        }
    }
}
