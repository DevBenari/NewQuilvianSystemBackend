using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/transport-allowance-transactions")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_PAYROLL_MANAGEMENT",
        moduleName: "Human Resource Payroll Management",
        displayName: "Transport Allowance Transaction",
        AreaName = "Corporate",
        ControllerName = "TransportAllowanceTransaction",
        Description = "Corporate human resource transport allowance transaction",
        SortOrder = 12)]
    [Tags("Corporate / Human Resource / Payroll Management / Transport Allowance Transaction")]
    public class WfpTransportAllowanceTransactionController : ControllerBase
    {
        private static readonly HashSet<string> AllowedTransactionTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Accrual",
                "Adjustment",
                "Reservation",
                "Payment",
                "Reversal",
                "Expiry"
            };

        private static readonly HashSet<string> AllowedTransactionStatuses =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Draft",
                "Calculated",
                "Approved",
                "Posted",
                "Reversed",
                "Cancelled"
            };

        private const string NumberPrefix = "TAT-RSMMC-";
        private const int NumberLength = 7;
        private const string LogCategory = "Corporate.HumanResource.PayrollManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpTransportAllowanceTransactionController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Transport Allowance Transaction", Description = "Melihat metadata filter transaksi transport allowance", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TransportAllowanceTransaction", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new WfpTransportAllowanceTransactionFilterMetadataResponse
            {
                DefaultFilter = new WfpTransportAllowanceTransactionDefaultFilterResponse(),
                TransactionTypeOptions = AllowedTransactionTypes
                    .OrderBy(x => x)
                    .Select(x => new WfpTransportAllowanceTransactionStringOptionResponse
                    {
                        Value = x,
                        Label = x
                    })
                    .ToList(),
                TransactionStatusOptions = AllowedTransactionStatuses
                    .OrderBy(x => x)
                    .Select(x => new WfpTransportAllowanceTransactionStringOptionResponse
                    {
                        Value = x,
                        Label = x
                    })
                    .ToList(),
                SortOptions = new List<WfpTransportAllowanceTransactionSortOptionResponse>
                {
                    new() { Value = "transactionDate", Label = "Tanggal transaksi" },
                    new() { Value = "transactionNumber", Label = "Nomor transaksi" },
                    new() { Value = "transactionType", Label = "Jenis transaksi" },
                    new() { Value = "transactionStatus", Label = "Status transaksi" },
                    new() { Value = "amount", Label = "Nominal" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpTransportAllowanceTransactionFilterMetadataResponse>.Ok(
                result,
                "Metadata filter transaksi transport allowance berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Transport Allowance Transaction", Description = "Melihat ringkasan transaksi transport allowance", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TransportAllowanceTransaction", "Read")]
        public async Task<IActionResult> GetSummary(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<WfpTransportAllowanceTransaction>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var result = new WfpTransportAllowanceTransactionSummaryResponse
            {
                TotalData = await query.CountAsync(cancellationToken),
                DraftData = await query.CountAsync(x => x.TransactionStatus == "Draft", cancellationToken),
                ApprovedData = await query.CountAsync(x => x.TransactionStatus == "Approved", cancellationToken),
                PostedData = await query.CountAsync(x => x.TransactionStatus == "Posted", cancellationToken),
                ReversedData = await query.CountAsync(x => x.TransactionStatus == "Reversed", cancellationToken),
                CancelledData = await query.CountAsync(x => x.TransactionStatus == "Cancelled", cancellationToken),
                TotalAccrualAmount = await query
                    .Where(x => x.TransactionType == "Accrual")
                    .SumAsync(x => x.Amount, cancellationToken),
                TotalPaymentAmount = await query
                    .Where(x => x.TransactionType == "Payment")
                    .SumAsync(x => x.Amount, cancellationToken),
                TotalAdjustmentAmount = await query
                    .Where(x => x.TransactionType == "Adjustment")
                    .SumAsync(x => x.Amount, cancellationToken)
            };

            return Ok(ApiResponse<WfpTransportAllowanceTransactionSummaryResponse>.Ok(
                result,
                "Ringkasan transaksi transport allowance berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Transport Allowance Transaction", Description = "Melihat transaksi transport allowance", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TransportAllowanceTransaction", "Read")]
        public async Task<IActionResult> GetData(
            Guid workforceProfileId,
            [FromQuery] DateOnly? startDate,
            [FromQuery] DateOnly? endDate,
            [FromQuery] Guid? payrollPeriodId,
            [FromQuery] string? transactionType,
            [FromQuery] string? transactionStatus,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "transactionDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyFilter(
                BuildBaseQuery(workforceProfileId),
                startDate,
                endDate,
                payrollPeriodId,
                transactionType,
                transactionStatus,
                isActive,
                search);

            var totalData = await query.CountAsync(cancellationToken);

            var rows = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<WfpTransportAllowanceTransactionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = rows.Select(MapResponse).ToList()
            };

            return Ok(ApiResponse<PagedResult<WfpTransportAllowanceTransactionResponse>>.Ok(
                result,
                "Data transaksi transport allowance berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Transport Allowance Transaction", Description = "Melihat detail transaksi transport allowance", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TransportAllowanceTransaction", "Read")]
        public async Task<IActionResult> GetById(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Transaksi transport allowance tidak ditemukan."));
            }

            return Ok(ApiResponse<WfpTransportAllowanceTransactionDetailResponse>.Ok(
                MapDetailResponse(entity),
                "Detail transaksi transport allowance berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Transport Allowance Transaction", Description = "Membuat transaksi transport allowance", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("TransportAllowanceTransaction", "Create")]
        public async Task<IActionResult> Create(
            Guid workforceProfileId,
            [FromBody] CreateWfpTransportAllowanceTransactionRequest request,
            CancellationToken cancellationToken)
        {
            var validation = await ValidateRequestAsync(
                workforceProfileId,
                request,
                cancellationToken);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data transaksi transport allowance tidak valid."));
            }

            var entity = new WfpTransportAllowanceTransaction
            {
                Id = Guid.NewGuid(),
                TransportAllowanceId = request.TransportAllowanceId,
                WorkforceProfileId = workforceProfileId,
                PayrollPeriodId = NormalizeGuid(request.PayrollPeriodId),
                PayrollRunEmployeeId = NormalizeGuid(request.PayrollRunEmployeeId),
                AttendanceDailyId = NormalizeGuid(request.AttendanceDailyId),
                TransactionNumber = await GenerateTransactionNumberAsync(cancellationToken),
                TransactionDate = request.TransactionDate,
                TransactionType = NormalizeTransactionType(request.TransactionType),
                TransactionStatus = "Draft",
                Quantity = request.Quantity,
                Rate = request.Rate,
                Amount = request.Amount,
                BalanceAfterTransaction = request.BalanceAfterTransaction,
                SourceType = NormalizeText(request.SourceType),
                SourceId = NormalizeGuid(request.SourceId),
                Description = NormalizeText(request.Description),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpTransportAllowanceTransaction>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "TransportAllowanceTransaction.Create",
                "Membuat transaksi transport allowance.",
                new { entity.Id, entity.TransactionNumber, entity.WorkforceProfileId });

            return await GetById(workforceProfileId, entity.Id, cancellationToken);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Transport Allowance Transaction", Description = "Mengubah transaksi transport allowance", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("TransportAllowanceTransaction", "Update")]
        public async Task<IActionResult> Update(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpTransportAllowanceTransactionRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpTransportAllowanceTransaction>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Transaksi transport allowance tidak ditemukan."));
            }

            if (!string.Equals(entity.TransactionStatus, "Draft", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(entity.TransactionStatus, "Calculated", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Transaksi hanya dapat diubah saat berstatus Draft atau Calculated."));
            }

            var validation = await ValidateRequestAsync(
                workforceProfileId,
                request,
                cancellationToken);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data transaksi transport allowance tidak valid."));
            }

            entity.TransportAllowanceId = request.TransportAllowanceId;
            entity.PayrollPeriodId = NormalizeGuid(request.PayrollPeriodId);
            entity.PayrollRunEmployeeId = NormalizeGuid(request.PayrollRunEmployeeId);
            entity.AttendanceDailyId = NormalizeGuid(request.AttendanceDailyId);
            entity.TransactionDate = request.TransactionDate;
            entity.TransactionType = NormalizeTransactionType(request.TransactionType);
            entity.Quantity = request.Quantity;
            entity.Rate = request.Rate;
            entity.Amount = request.Amount;
            entity.BalanceAfterTransaction = request.BalanceAfterTransaction;
            entity.SourceType = NormalizeText(request.SourceType);
            entity.SourceId = NormalizeGuid(request.SourceId);
            entity.Description = NormalizeText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetById(workforceProfileId, entity.Id, cancellationToken);
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Transport Allowance Transaction Status", Description = "Mengubah status transaksi transport allowance", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("TransportAllowanceTransaction", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpTransportAllowanceTransactionStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpTransportAllowanceTransaction>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Transaksi transport allowance tidak ditemukan."));
            }

            if (!AllowedTransactionStatuses.Contains(request.TransactionStatus.Trim()))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "TransactionStatus tidak valid."));
            }

            var targetStatus = NormalizeTransactionStatus(request.TransactionStatus);

            if (!IsValidStatusTransition(entity.TransactionStatus, targetStatus))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Perubahan status {entity.TransactionStatus} ke {targetStatus} tidak diperbolehkan."));
            }

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            entity.TransactionStatus = targetStatus;
            entity.Description = NormalizeText(request.Description) ?? entity.Description;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            if (targetStatus == "Posted")
            {
                entity.PostedAt = now;
                entity.PostedByUserId = actor;
            }

            if (targetStatus == "Cancelled")
            {
                entity.IsCancel = true;
                entity.CancelDateTime = now;
                entity.CancelBy = actor;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status transaksi transport allowance berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Transport Allowance Transaction", Description = "Menghapus transaksi transport allowance", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("TransportAllowanceTransaction", "Delete")]
        public async Task<IActionResult> Delete(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpTransportAllowanceTransaction>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Transaksi transport allowance tidak ditemukan."));
            }

            if (!string.Equals(entity.TransactionStatus, "Draft", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(entity.TransactionStatus, "Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Transaksi hanya dapat dihapus saat berstatus Draft atau Cancelled."));
            }

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Transaksi transport allowance berhasil dihapus."));
        }

        private IQueryable<WfpTransportAllowanceTransaction> BuildBaseQuery(
            Guid workforceProfileId)
        {
            return _dbContext.Set<WfpTransportAllowanceTransaction>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.PayrollPeriod)
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);
        }

        private static IQueryable<WfpTransportAllowanceTransaction> ApplyFilter(
            IQueryable<WfpTransportAllowanceTransaction> query,
            DateOnly? startDate,
            DateOnly? endDate,
            Guid? payrollPeriodId,
            string? transactionType,
            string? transactionStatus,
            bool? isActive,
            string? search)
        {
            if (startDate.HasValue)
                query = query.Where(x => x.TransactionDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.TransactionDate <= endDate.Value);

            if (payrollPeriodId.HasValue && payrollPeriodId.Value != Guid.Empty)
                query = query.Where(x => x.PayrollPeriodId == payrollPeriodId.Value);

            if (!string.IsNullOrWhiteSpace(transactionType))
                query = query.Where(x => x.TransactionType == transactionType.Trim());

            if (!string.IsNullOrWhiteSpace(transactionStatus))
                query = query.Where(x => x.TransactionStatus == transactionStatus.Trim());

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.TransactionNumber.ToLower().Contains(keyword) ||
                    (x.SourceType != null && x.SourceType.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpTransportAllowanceTransaction> ApplySorting(
            IQueryable<WfpTransportAllowanceTransaction> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "transactionDate").Trim().ToLowerInvariant() switch
            {
                "transactionnumber" => desc
                    ? query.OrderByDescending(x => x.TransactionNumber)
                    : query.OrderBy(x => x.TransactionNumber),
                "transactiontype" => desc
                    ? query.OrderByDescending(x => x.TransactionType)
                    : query.OrderBy(x => x.TransactionType),
                "transactionstatus" => desc
                    ? query.OrderByDescending(x => x.TransactionStatus)
                    : query.OrderBy(x => x.TransactionStatus),
                "amount" => desc
                    ? query.OrderByDescending(x => x.Amount)
                    : query.OrderBy(x => x.Amount),
                "createdatetime" => desc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),
                _ => desc
                    ? query.OrderByDescending(x => x.TransactionDate)
                        .ThenByDescending(x => x.TransactionNumber)
                    : query.OrderBy(x => x.TransactionDate)
                        .ThenBy(x => x.TransactionNumber)
            };
        }

        private WfpTransportAllowanceTransactionResponse MapResponse(
            WfpTransportAllowanceTransaction entity)
        {
            return new WfpTransportAllowanceTransactionResponse
            {
                Id = entity.Id,
                TransportAllowanceId = entity.TransportAllowanceId,
                WorkforceProfileId = entity.WorkforceProfileId,
                WorkforceProfileCode = entity.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = entity.WorkforceProfile?.DisplayName ?? string.Empty,
                PayrollPeriodId = entity.PayrollPeriodId,
                PayrollPeriodCode = entity.PayrollPeriod?.PayrollPeriodCode,
                PayrollPeriodName = entity.PayrollPeriod?.PayrollPeriodName,
                PayrollRunEmployeeId = entity.PayrollRunEmployeeId,
                AttendanceDailyId = entity.AttendanceDailyId,
                TransactionNumber = entity.TransactionNumber,
                TransactionDate = entity.TransactionDate,
                TransactionType = entity.TransactionType,
                TransactionStatus = entity.TransactionStatus,
                Quantity = entity.Quantity,
                Rate = entity.Rate,
                Amount = entity.Amount,
                BalanceAfterTransaction = entity.BalanceAfterTransaction,
                SourceType = entity.SourceType,
                SourceId = entity.SourceId,
                PostedAt = entity.PostedAt,
                PostedByUserId = entity.PostedByUserId,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetUserDisplayName(entity.CreateBy)
            };
        }

        private WfpTransportAllowanceTransactionDetailResponse MapDetailResponse(
            WfpTransportAllowanceTransaction entity)
        {
            var response = MapResponse(entity);

            return new WfpTransportAllowanceTransactionDetailResponse
            {
                Id = response.Id,
                TransportAllowanceId = response.TransportAllowanceId,
                WorkforceProfileId = response.WorkforceProfileId,
                WorkforceProfileCode = response.WorkforceProfileCode,
                WorkforceDisplayName = response.WorkforceDisplayName,
                PayrollPeriodId = response.PayrollPeriodId,
                PayrollPeriodCode = response.PayrollPeriodCode,
                PayrollPeriodName = response.PayrollPeriodName,
                PayrollRunEmployeeId = response.PayrollRunEmployeeId,
                AttendanceDailyId = response.AttendanceDailyId,
                TransactionNumber = response.TransactionNumber,
                TransactionDate = response.TransactionDate,
                TransactionType = response.TransactionType,
                TransactionStatus = response.TransactionStatus,
                Quantity = response.Quantity,
                Rate = response.Rate,
                Amount = response.Amount,
                BalanceAfterTransaction = response.BalanceAfterTransaction,
                SourceType = response.SourceType,
                SourceId = response.SourceId,
                PostedAt = response.PostedAt,
                PostedByUserId = response.PostedByUserId,
                Description = response.Description,
                IsActive = response.IsActive,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetUserDisplayName(entity.UpdateBy)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            CreateWfpTransportAllowanceTransactionRequest request,
            CancellationToken cancellationToken)
        {
            if (request.TransportAllowanceId == Guid.Empty)
                return (false, "Transport allowance wajib dipilih.");

            if (!AllowedTransactionTypes.Contains(request.TransactionType.Trim()))
                return (false, "TransactionType tidak valid.");

            if (!await _dbContext.Set<WfpTransportAllowance>().AnyAsync(x =>
                    x.Id == request.TransportAllowanceId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken))
            {
                return (false, "Transport allowance tidak valid atau tidak sesuai workforce profile.");
            }

            if (request.PayrollPeriodId.HasValue && request.PayrollPeriodId.Value != Guid.Empty &&
                !await _dbContext.Set<MstPayrollPeriod>().AnyAsync(x =>
                    x.Id == request.PayrollPeriodId.Value &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken))
            {
                return (false, "Payroll period tidak ditemukan atau tidak aktif.");
            }

            if (request.PayrollRunEmployeeId.HasValue && request.PayrollRunEmployeeId.Value != Guid.Empty &&
                !await _dbContext.Set<TrxPayrollRunEmployee>().AnyAsync(x =>
                    x.Id == request.PayrollRunEmployeeId.Value &&
                    !x.IsDelete,
                    cancellationToken))
            {
                return (false, "Payroll run employee tidak ditemukan.");
            }

            if (request.AttendanceDailyId.HasValue && request.AttendanceDailyId.Value != Guid.Empty &&
                !await _dbContext.Set<TrxAttendanceDaily>().AnyAsync(x =>
                    x.Id == request.AttendanceDailyId.Value &&
                    !x.IsDelete,
                    cancellationToken))
            {
                return (false, "Attendance daily tidak ditemukan.");
            }

            return (true, null);
        }

        private async Task<string> GenerateTransactionNumberAsync(
            CancellationToken cancellationToken)
        {
            var codes = await _dbContext.Set<WfpTransportAllowanceTransaction>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.TransactionNumber.StartsWith(NumberPrefix))
                .Select(x => x.TransactionNumber)
                .ToListAsync(cancellationToken);

            var used = codes
                .Select(x => x.Replace(NumberPrefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .ToHashSet();

            var next = 1;
            while (used.Contains(next))
                next++;

            return NumberPrefix + next.ToString().PadLeft(NumberLength, '0');
        }

        private static bool IsValidStatusTransition(
            string currentStatus,
            string targetStatus)
        {
            if (currentStatus.Equals(targetStatus, StringComparison.OrdinalIgnoreCase))
                return true;

            return currentStatus switch
            {
                "Draft" => targetStatus is "Calculated" or "Cancelled",
                "Calculated" => targetStatus is "Approved" or "Draft" or "Cancelled",
                "Approved" => targetStatus is "Posted" or "Cancelled",
                "Posted" => targetStatus == "Reversed",
                "Reversed" => false,
                "Cancelled" => false,
                _ => false
            };
        }

        private string? GetUserDisplayName(Guid userId)
        {
            if (userId == Guid.Empty)
                return null;

            return _dbContext.Users
                .Where(x => x.Id == userId)
                .Select(x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode)
                .FirstOrDefault();
        }

        private Guid GetCurrentUserId()
        {
            var value =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(value, out var id)
                ? id
                : Guid.Empty;
        }

        private static Guid? NormalizeGuid(Guid? value)
        {
            return !value.HasValue || value.Value == Guid.Empty
                ? null
                : value.Value;
        }

        private static string? NormalizeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string NormalizeTransactionType(string value)
        {
            return AllowedTransactionTypes.First(x =>
                x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static string NormalizeTransactionStatus(string value)
        {
            return AllowedTransactionStatuses.First(x =>
                x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            return (
                pageNumber < 1 ? 1 : pageNumber,
                pageSize < 1 ? 25 : Math.Min(pageSize, 100));
        }
    }
}
