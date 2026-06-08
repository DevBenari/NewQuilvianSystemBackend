using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseBankPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs.BankResponse>;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/administrator/master-data/banks")]
    [AccessController(
        moduleCode: "ADMINISTRATOR_MASTER_DATA",
        moduleName: "Administrator Master Data",
        displayName: "Bank",
        AreaName = "Administrator",
        ControllerName = "Bank",
        Description = "Administrator master data bank",
        SortOrder = 6
    )]
    [Tags("Administrator / Master Data / Bank")]
    public class BankController : ControllerBase
    {
        private const string LogCategory = "Administrator.MasterData";
        private const string CodePrefix = "BNK-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public BankController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<BankFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Bank",
            Description = "Melihat metadata filter bank",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Bank", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new BankFilterMetadataResponse
            {
                DefaultFilter = new BankDefaultFilterResponse(),
                CustomPeriods = new List<BankCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<BankSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "bankCode", Label = "Kode bank" },
                    new() { Value = "bankName", Label = "Nama bank" },
                    new() { Value = "bankShortName", Label = "Nama singkat bank" },
                    new() { Value = "bankCategory", Label = "Kategori bank" },
                    new() { Value = "clearingCode", Label = "Clearing code" },
                    new() { Value = "isDefault", Label = "Default" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                BankCategoryOptions = BuildBankCategoryOptions(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Bank.GetFilterMetadata",
                "Mengambil metadata filter bank.",
                result
            );

            return Ok(ApiResponse<BankFilterMetadataResponse>.Ok(
                result,
                "Metadata filter bank berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<BankSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Bank",
            Description = "Melihat ringkasan bank",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Bank", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new BankSummaryResponse
            {
                TotalBank = await query.CountAsync(),
                ActiveBank = await query.CountAsync(x => x.IsActive),
                InactiveBank = await query.CountAsync(x => !x.IsActive),
                DefaultBank = await query.CountAsync(x => x.IsDefault),
                CommercialBank = await query.CountAsync(x => x.BankCategory == BankCategory.Commercial),
                SyariahBank = await query.CountAsync(x => x.BankCategory == BankCategory.Syariah),
                DigitalBank = await query.CountAsync(x => x.BankCategory == BankCategory.DigitalBank),
                RuralBank = await query.CountAsync(x => x.BankCategory == BankCategory.RuralBank),
                OtherBank = await query.CountAsync(x => x.BankCategory == BankCategory.Other),
                WithClearingCodeBank = await query.CountAsync(x =>
                    x.ClearingCode != null &&
                    x.ClearingCode != string.Empty)
            };

            return Ok(ApiResponse<BankSummaryResponse>.Ok(
                result,
                "Ringkasan bank berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseBankPagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Bank",
            Description = "Melihat data bank",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Bank", "Read")]
        public async Task<IActionResult> GetBanks(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyStandardFilter(query, isActive, search);

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities
                    .Select(x => x.CreateBy)
                    .Where(x => x != Guid.Empty)
            );

            var items = entities
                .Select(x => MapResponse(x, actorNames))
                .ToList();

            var result = new ResponseBankPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseBankPagedResult>.Ok(
                result,
                "Data bank berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<BankOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Bank",
            Description = "Melihat data pilihan bank",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Bank", "Read")]
        public async Task<IActionResult> GetBankOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyStandardFilter(
                query,
                onlyActive ? true : null,
                search
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.BankName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapOptionResponse)
                .ToList();

            var result = new BankOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<BankOptionPagedResponse>.Ok(
                result,
                "Data pilihan bank berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BankDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Bank",
            Description = "Melihat detail bank",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Bank", "Read")]
        public async Task<IActionResult> GetBankById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Bank tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, actorNames);

            if (data.UpdateDateTime.HasValue &&
                data.UpdateDateTime.Value == DateTime.MinValue)
            {
                data.UpdateDateTime = null;
            }

            if (!data.CreateBy.HasValue || data.CreateBy.Value == Guid.Empty)
            {
                data.CreateBy = null;
                data.CreateByName = null;
            }

            if (!data.UpdateBy.HasValue || data.UpdateBy.Value == Guid.Empty)
            {
                data.UpdateBy = null;
                data.UpdateByName = null;
            }

            return Ok(ApiResponse<BankDetailResponse>.Ok(
                data,
                "Detail bank berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BankCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Create",
            "Create Bank",
            Description = "Membuat data bank",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("Bank", "Create")]
        public async Task<IActionResult> CreateBank([FromBody] CreateBankRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                request: request
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data bank tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (request.IsDefault)
                {
                    await UnsetOtherDefaultBanksAsync(
                        exceptId: null,
                        now: now,
                        actorUserId: actorUserId
                    );
                }

                var entity = new MstBank
                {
                    Id = Guid.NewGuid(),
                    BankCode = await GenerateBankCodeAsync(),
                    BankName = request.BankName.Trim(),
                    BankShortName = NormalizeNullableString(request.BankShortName),
                    BankCategory = request.BankCategory,
                    ClearingCode = NormalizeUpperNullableString(request.ClearingCode),
                    IsDefault = request.IsDefault,
                    SortOrder = request.SortOrder,
                    Description = NormalizeNullableString(request.Description),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<MstBank>().Add(entity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = new BankCreateResponse
                {
                    Id = entity.Id,
                    BankCode = entity.BankCode,
                    BankName = entity.BankName,
                    BankShortName = entity.BankShortName,
                    IsActive = entity.IsActive
                };

                await _loggerService.InfoAsync(
                    LogCategory,
                    "Bank.CreateBank",
                    "Membuat data bank.",
                    result
                );

                return Ok(ApiResponse<BankCreateResponse>.Ok(
                    result,
                    "Bank berhasil dibuat."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "Bank.CreateBank",
                    "Gagal membuat data bank.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat bank."
                    )
                );
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Update",
            "Update Bank",
            Description = "Mengubah data bank",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Bank", "Update")]
        public async Task<IActionResult> UpdateBank(
            Guid id,
            [FromBody] UpdateBankRequest request)
        {
            var entity = await _dbContext.Set<MstBank>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Bank tidak ditemukan."
                ));
            }

            if (request.IsDefault && !request.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Bank default harus aktif."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                request: request
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data bank tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (request.IsDefault)
                {
                    await UnsetOtherDefaultBanksAsync(
                        exceptId: id,
                        now: now,
                        actorUserId: actorUserId
                    );
                }

                entity.BankName = request.BankName.Trim();
                entity.BankShortName = NormalizeNullableString(request.BankShortName);
                entity.BankCategory = request.BankCategory;
                entity.ClearingCode = NormalizeUpperNullableString(request.ClearingCode);
                entity.IsDefault = request.IsActive ? request.IsDefault : false;
                entity.SortOrder = request.SortOrder;
                entity.Description = NormalizeNullableString(request.Description);
                entity.IsActive = request.IsActive;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "Bank.UpdateBank",
                    "Mengubah data bank.",
                    new
                    {
                        entity.Id,
                        entity.BankCode,
                        entity.BankName,
                        entity.BankCategory,
                        entity.IsActive
                    }
                );

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "Bank berhasil diperbarui."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "Bank.UpdateBank",
                    "Gagal mengubah data bank.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat memperbarui bank."
                    )
                );
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Bank Status",
            Description = "Mengubah status bank",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Bank", "Update")]
        public async Task<IActionResult> UpdateBankStatus(
            Guid id,
            [FromBody] UpdateBankStatusRequest request)
        {
            var entity = await _dbContext.Set<MstBank>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Bank tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;

            if (!request.IsActive)
            {
                entity.IsDefault = false;
            }

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status bank berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Bank",
            Description = "Menghapus data bank",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("Bank", "Delete")]
        public async Task<IActionResult> DeleteBank(
            Guid id,
            [FromBody] DeleteBankRequest? request = null)
        {
            var entity = await _dbContext.Set<MstBank>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Bank tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsDefault = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (!string.IsNullOrWhiteSpace(request?.DeleteReason))
            {
                entity.Description = request.DeleteReason.Trim();
            }

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Bank.DeleteBank",
                "Menghapus data bank.",
                new
                {
                    entity.Id,
                    entity.BankCode,
                    entity.BankName,
                    entity.DeleteDateTime
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Bank berhasil dihapus."
            ));
        }

        private IQueryable<MstBank> BuildBaseQuery()
        {
            return _dbContext.Set<MstBank>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstBank> ApplyDateFilter(
            IQueryable<MstBank> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue)
            {
                var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime < end);
            }

            if (!startDate.HasValue &&
                !endDate.HasValue &&
                !string.IsNullOrWhiteSpace(customPeriod))
            {
                var today = DateTime.UtcNow.Date;

                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        query = query.Where(x =>
                            x.CreateDateTime >= today &&
                            x.CreateDateTime < today.AddDays(1));
                        break;

                    case "last7days":
                        query = query.Where(x =>
                            x.CreateDateTime >= today.AddDays(-6) &&
                            x.CreateDateTime < today.AddDays(1));
                        break;

                    case "thismonth":
                        var thisMonthStart = new DateTime(
                            today.Year,
                            today.Month,
                            1,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc
                        );

                        query = query.Where(x =>
                            x.CreateDateTime >= thisMonthStart &&
                            x.CreateDateTime < thisMonthStart.AddMonths(1));
                        break;

                    case "lastmonth":
                        var currentMonthStart = new DateTime(
                            today.Year,
                            today.Month,
                            1,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc
                        );

                        var lastMonthStart = currentMonthStart.AddMonths(-1);

                        query = query.Where(x =>
                            x.CreateDateTime >= lastMonthStart &&
                            x.CreateDateTime < currentMonthStart);
                        break;
                }
            }

            return query;
        }

        private static IQueryable<MstBank> ApplyStandardFilter(
            IQueryable<MstBank> query,
            bool? isActive,
            string? search)
        {
            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                var matchedCategories = Enum.GetValues<BankCategory>()
                    .Where(x =>
                        x.ToString().ToLower().Contains(keyword) ||
                        BuildBankCategoryLabel(x).ToLower().Contains(keyword))
                    .ToList();

                query = query.Where(x =>
                    x.BankCode.ToLower().Contains(keyword) ||
                    x.BankName.ToLower().Contains(keyword) ||
                    (x.BankShortName != null && x.BankShortName.ToLower().Contains(keyword)) ||
                    (x.ClearingCode != null && x.ClearingCode.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    matchedCategories.Contains(x.BankCategory));
            }

            return query;
        }

        private static IOrderedQueryable<MstBank> ApplySorting(
            IQueryable<MstBank> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase
            );

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "bankcode" => isDescending
                    ? query.OrderByDescending(x => x.BankCode)
                    : query.OrderBy(x => x.BankCode),

                "bankname" => isDescending
                    ? query.OrderByDescending(x => x.BankName)
                    : query.OrderBy(x => x.BankName),

                "bankshortname" => isDescending
                    ? query.OrderByDescending(x => x.BankShortName)
                    : query.OrderBy(x => x.BankShortName),

                "bankcategory" => isDescending
                    ? query.OrderByDescending(x => x.BankCategory).ThenBy(x => x.BankName)
                    : query.OrderBy(x => x.BankCategory).ThenBy(x => x.BankName),

                "clearingcode" => isDescending
                    ? query.OrderByDescending(x => x.ClearingCode).ThenBy(x => x.BankName)
                    : query.OrderBy(x => x.ClearingCode).ThenBy(x => x.BankName),

                "isdefault" => isDescending
                    ? query.OrderByDescending(x => x.IsDefault).ThenBy(x => x.BankName)
                    : query.OrderBy(x => x.IsDefault).ThenBy(x => x.BankName),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.BankName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.BankName),

                _ => isDescending
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.BankName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.BankName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateBankRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.BankName))
            {
                return (false, "Nama bank wajib diisi.");
            }

            if (!Enum.IsDefined(typeof(BankCategory), request.BankCategory))
            {
                return (false, "Kategori bank tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            var normalizedName = request.BankName.Trim().ToLower();

            var duplicateNameQuery = _dbContext.Set<MstBank>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.BankName.ToLower() == normalizedName);

            if (excludeId.HasValue)
            {
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateNameQuery.AnyAsync())
            {
                return (false, "Nama bank sudah digunakan.");
            }

            var clearingCode = NormalizeUpperNullableString(request.ClearingCode);

            if (!string.IsNullOrWhiteSpace(clearingCode))
            {
                var duplicateClearingQuery = _dbContext.Set<MstBank>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.ClearingCode != null &&
                        x.ClearingCode.ToLower() == clearingCode.ToLower());

                if (excludeId.HasValue)
                {
                    duplicateClearingQuery = duplicateClearingQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicateClearingQuery.AnyAsync())
                {
                    return (false, "Clearing code sudah digunakan.");
                }
            }

            return (true, null);
        }

        private async Task UnsetOtherDefaultBanksAsync(
            Guid? exceptId,
            DateTime now,
            Guid actorUserId)
        {
            var query = _dbContext.Set<MstBank>()
                .Where(x =>
                    x.IsDefault &&
                    !x.IsDelete);

            if (exceptId.HasValue)
            {
                query = query.Where(x => x.Id != exceptId.Value);
            }

            var banks = await query.ToListAsync();

            foreach (var bank in banks)
            {
                bank.IsDefault = false;
                bank.UpdateDateTime = now;
                bank.UpdateBy = actorUserId;
            }
        }

        private async Task<string> GenerateBankCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstBank>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.BankCode.StartsWith(CodePrefix))
                .Select(x => x.BankCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(x => x.Replace(CodePrefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return CodePrefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(
            IEnumerable<Guid> actorIds)
        {
            var ids = actorIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (!ids.Any())
            {
                return new Dictionary<Guid, string?>();
            }

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    Name =
                        x.DisplayName ??
                        x.UserName ??
                        x.Email ??
                        x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static BankResponse MapResponse(
            MstBank entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new BankResponse
            {
                Id = entity.Id,
                BankCode = entity.BankCode,
                BankName = entity.BankName,
                BankShortName = entity.BankShortName,
                BankCategory = entity.BankCategory,
                BankCategoryName = BuildBankCategoryLabel(entity.BankCategory),
                ClearingCode = entity.ClearingCode,
                IsDefault = entity.IsDefault,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static BankDetailResponse MapDetailResponse(
            MstBank entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new BankDetailResponse
            {
                Id = entity.Id,
                BankCode = entity.BankCode,
                BankName = entity.BankName,
                BankShortName = entity.BankShortName,
                BankCategory = entity.BankCategory,
                BankCategoryName = BuildBankCategoryLabel(entity.BankCategory),
                ClearingCode = entity.ClearingCode,
                IsDefault = entity.IsDefault,
                SortOrder = entity.SortOrder,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static BankOptionResponse MapOptionResponse(MstBank entity)
        {
            return new BankOptionResponse
            {
                Id = entity.Id,
                BankCode = entity.BankCode,
                BankName = entity.BankName,
                BankShortName = entity.BankShortName,
                BankCategory = entity.BankCategory,
                BankCategoryName = BuildBankCategoryLabel(entity.BankCategory),
                ClearingCode = entity.ClearingCode,
                IsDefault = entity.IsDefault,
                SortOrder = entity.SortOrder
            };
        }

        private static List<BankCategoryOptionResponse> BuildBankCategoryOptions()
        {
            return Enum.GetValues<BankCategory>()
                .Select(x => new BankCategoryOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = BuildBankCategoryLabel(x)
                })
                .ToList();
        }

        private static string BuildBankCategoryLabel(BankCategory value)
        {
            return value switch
            {
                BankCategory.Commercial => "Commercial",
                BankCategory.Syariah => "Syariah",
                BankCategory.DigitalBank => "Digital Bank",
                BankCategory.RuralBank => "Rural Bank",
                BankCategory.Other => "Other",
                _ => value.ToString()
            };
        }

        private static string? GetActorName(
            IReadOnlyDictionary<Guid, string?> actorNames,
            Guid actorId)
        {
            if (actorId == Guid.Empty)
            {
                return null;
            }

            return actorNames.TryGetValue(actorId, out var actorName)
                ? actorName
                : null;
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

        private static string? NormalizeUpperNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToUpperInvariant();
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
