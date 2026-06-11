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
using System.Data;
using System.Globalization;
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
        private const string BankCodePrefix = "BNK-RSMMC-";
        private const int BankCodeDigitLength = 5;

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
            var bankCategoryOptions = BuildBankCategoryOptions();

            var result = new BankFilterMetadataResponse
            {
                DefaultFilter = new BankDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
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
                EnumOptions = BuildEnumMetadataOptions(bankCategoryOptions),
                BankCategoryOptions = bankCategoryOptions,
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
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
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] BankCategory? bankCategory,
            [FromQuery] bool? isDefault,
            [FromQuery] bool? hasClearingCode,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);

            if (!dateRange.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    dateRange.ErrorMessage ?? "Filter tanggal tidak valid."
                ));
            }

            var query = BuildBaseQuery();

            query = ApplyDateFilter(query, dateRange);
            query = ApplyStandardFilter(
                query,
                search,
                isActive,
                bankCategory,
                isDefault,
                hasClearingCode
            );

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
            [FromQuery] bool? activeOnly = null,
            [FromQuery] BankCategory? bankCategory = null,
            [FromQuery] bool? isDefault = null,
            [FromQuery] bool? hasClearingCode = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var useOnlyActive = activeOnly ?? onlyActive;

            var query = BuildBaseQuery();

            query = ApplyStandardFilter(
                query,
                search,
                useOnlyActive ? true : null,
                bankCategory,
                isDefault,
                hasClearingCode
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

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            if (request.IsDefault)
            {
                await UnsetOtherDefaultBanksAsync(
                    exceptId: null,
                    now: now,
                    actorUserId: actorUserId
                );
            }

            var generatedBankCode = await GenerateBankCodeAsync();

            var entity = new MstBank
            {
                Id = Guid.NewGuid(),
                BankCode = generatedBankCode,
                BankName = request.BankName.Trim(),
                BankShortName = NormalizeNullableText(request.BankShortName),
                BankCategory = request.BankCategory,
                ClearingCode = NormalizeUpperNullableText(request.ClearingCode),
                IsDefault = request.IsDefault,
                SortOrder = request.SortOrder,
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstBank>().Add(entity);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy });

            var result = new BankCreateResponse
            {
                Id = entity.Id,
                BankCode = entity.BankCode,
                BankName = entity.BankName,
                BankShortName = entity.BankShortName,
                BankCategory = entity.BankCategory,
                BankCategoryName = BuildBankCategoryLabel(entity.BankCategory),
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
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

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BankUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
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

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            if (request.IsDefault)
            {
                await UnsetOtherDefaultBanksAsync(
                    exceptId: id,
                    now: now,
                    actorUserId: actorUserId
                );
            }

            entity.BankName = request.BankName.Trim();
            entity.BankShortName = NormalizeNullableText(request.BankShortName);
            entity.BankCategory = request.BankCategory;
            entity.ClearingCode = NormalizeUpperNullableText(request.ClearingCode);
            entity.IsDefault = request.IsActive ? request.IsDefault : false;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });

            var result = new BankUpdateResponse
            {
                Id = entity.Id,
                BankCode = entity.BankCode,
                BankName = entity.BankName,
                BankShortName = entity.BankShortName,
                BankCategory = entity.BankCategory,
                BankCategoryName = BuildBankCategoryLabel(entity.BankCategory),
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Bank.UpdateBank",
                "Mengubah data bank.",
                result
            );

            return Ok(ApiResponse<BankUpdateResponse>.Ok(
                result,
                "Bank berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<BankUpdateResponse>), StatusCodes.Status200OK)]
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

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });

            var result = new BankUpdateResponse
            {
                Id = entity.Id,
                BankCode = entity.BankCode,
                BankName = entity.BankName,
                BankShortName = entity.BankShortName,
                BankCategory = entity.BankCategory,
                BankCategoryName = BuildBankCategoryLabel(entity.BankCategory),
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            return Ok(ApiResponse<BankUpdateResponse>.Ok(
                result,
                "Status bank berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BankDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
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

            var isUsedByWorkforceBankAccount = await _dbContext.WfpBankAccounts
                .AnyAsync(x => x.BankId == id && !x.IsDelete);

            if (isUsedByWorkforceBankAccount)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Bank tidak dapat dihapus karena sudah digunakan oleh rekening bank workforce."
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

            var actorNames = await GetActorNameMapAsync(new[] { entity.DeleteBy });

            var result = new BankDeleteResponse
            {
                Id = entity.Id,
                BankCode = entity.BankCode,
                BankName = entity.BankName,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : (Guid?)entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Bank.DeleteBank",
                "Menghapus data bank.",
                result
            );

            return Ok(ApiResponse<BankDeleteResponse>.Ok(
                result,
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
            DateRangeResolveResult dateRange)
        {
            if (dateRange.Start.HasValue)
            {
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            }

            if (dateRange.EndExclusive.HasValue)
            {
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            }

            return query;
        }

        private static IQueryable<MstBank> ApplyStandardFilter(
            IQueryable<MstBank> query,
            string? search,
            bool? isActive,
            BankCategory? bankCategory,
            bool? isDefault,
            bool? hasClearingCode)
        {
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

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (bankCategory.HasValue)
            {
                query = query.Where(x => x.BankCategory == bankCategory.Value);
            }

            if (isDefault.HasValue)
            {
                query = query.Where(x => x.IsDefault == isDefault.Value);
            }

            if (hasClearingCode.HasValue)
            {
                query = hasClearingCode.Value
                    ? query.Where(x => x.ClearingCode != null && x.ClearingCode != string.Empty)
                    : query.Where(x => x.ClearingCode == null || x.ClearingCode == string.Empty);
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
                    ? query.OrderByDescending(x => x.BankShortName).ThenBy(x => x.BankName)
                    : query.OrderBy(x => x.BankShortName).ThenBy(x => x.BankName),

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

            if (request.SortOrder < 0)
            {
                return (false, "Urutan tidak boleh kurang dari 0.");
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

            var shortName = NormalizeNullableText(request.BankShortName);

            if (!string.IsNullOrWhiteSpace(shortName))
            {
                var normalizedShortName = shortName.ToLower();

                var duplicateShortNameQuery = _dbContext.Set<MstBank>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.BankShortName != null &&
                        x.BankShortName.ToLower() == normalizedShortName);

                if (excludeId.HasValue)
                {
                    duplicateShortNameQuery = duplicateShortNameQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicateShortNameQuery.AnyAsync())
                {
                    return (false, "Nama singkat bank sudah digunakan.");
                }
            }

            var clearingCode = NormalizeUpperNullableText(request.ClearingCode);

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
                .Where(x => x.BankCode.StartsWith(BankCodePrefix))
                .Select(x => x.BankCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(TryExtractBankSequenceNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return BankCodePrefix + nextNumber.ToString("D" + BankCodeDigitLength, CultureInfo.InvariantCulture);
        }

        private static int? TryExtractBankSequenceNumber(string bankCode)
        {
            if (string.IsNullOrWhiteSpace(bankCode))
            {
                return null;
            }

            if (!bankCode.StartsWith(BankCodePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var numberText = bankCode[BankCodePrefix.Length..];

            return int.TryParse(numberText, NumberStyles.None, CultureInfo.InvariantCulture, out var number)
                ? number
                : null;
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

        private static DateRangeResolveResult ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var period = customPeriod?.Trim().ToLowerInvariant();
            var today = DateTime.UtcNow.Date;

            DateTime? start = null;
            DateTime? endExclusive = null;

            switch (period)
            {
                case null:
                case "":
                case "custom":
                    if (startDate.HasValue)
                    {
                        start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                    }

                    if (endDate.HasValue)
                    {
                        endExclusive = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                    }

                    break;

                case "today":
                    start = today;
                    endExclusive = today.AddDays(1);
                    break;

                case "last7days":
                    start = today.AddDays(-6);
                    endExclusive = today.AddDays(1);
                    break;

                case "last30days":
                    start = today.AddDays(-29);
                    endExclusive = today.AddDays(1);
                    break;

                case "thismonth":
                    start = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    endExclusive = start.Value.AddMonths(1);
                    break;

                case "lastmonth":
                    var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    start = currentMonthStart.AddMonths(-1);
                    endExclusive = currentMonthStart;
                    break;

                default:
                    return DateRangeResolveResult.Invalid($"customPeriod '{customPeriod}' tidak valid.");
            }

            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value)
            {
                return DateRangeResolveResult.Invalid("startDate tidak boleh lebih besar atau sama dengan endDate.");
            }

            return DateRangeResolveResult.Valid(start, endExclusive);
        }

        private static List<BankCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<BankCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false }
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


        private static List<BankEnumMetadataResponse> BuildEnumMetadataOptions(
            List<BankCategoryOptionResponse> bankCategoryOptions)
        {
            return new List<BankEnumMetadataResponse>
            {
                new()
                {
                    EnumName = nameof(BankCategory),
                    FieldName = "bankCategory",
                    OptionsSource = "bankCategoryOptions",
                    Description = "Enum kategori bank untuk field bankCategory pada create, update, filter, response, dan option.",
                    Options = bankCategoryOptions
                        .Select(x => new BankEnumOptionResponse
                        {
                            Value = x.Value,
                            Name = x.Name,
                            Label = x.Label
                        })
                        .ToList()
                }
            };
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
                _ => SplitPascalCase(value.ToString())
            };
        }

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private static List<BankQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<BankQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "DateTime?", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "DateTime?", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Filter periode cepat: custom, today, last7days, last30days, thismonth, lastmonth.", Example = "thismonth" },
                new() { Name = "search", Type = "string", Description = "Cari berdasarkan kode, nama, nama singkat, kategori, clearing code, atau deskripsi.", Example = "BCA" },
                new() { Name = "isActive", Type = "bool", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "bankCategory", Type = "enum", Description = "Filter berdasarkan kategori bank.", Example = "1" },
                new() { Name = "isDefault", Type = "bool", Description = "Filter bank default.", Example = "true" },
                new() { Name = "hasClearingCode", Type = "bool", Description = "Filter bank yang memiliki clearing code atau tidak.", Example = "true" },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting.", Example = "sortOrder" },
                new() { Name = "sortDirection", Type = "string", Description = "Arah sorting: asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman, maksimal 100.", Example = "25" }
            };
        }

        private static List<BankFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: false);
        }

        private static List<BankFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: true);
        }

        private static List<BankFormFieldMetadataResponse> BuildFieldMetadata(bool isUpdate)
        {
            var fields = new List<BankFormFieldMetadataResponse>
            {
                new() { Name = "bankCode", Label = "Kode Bank", Section = "Basic", InputType = "readonly", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "AutoGenerated", MaxLength = 50, Description = "Digenerate otomatis oleh sistem dengan format BNK-RSMMC-00001.", Example = "BNK-RSMMC-00001", SortOrder = 1 },
                new() { Name = "bankName", Label = "Nama Bank", Section = "Basic", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 200, Example = "Bank Central Asia", SortOrder = 2 },
                new() { Name = "bankShortName", Label = "Nama Singkat", Section = "Basic", InputType = "text", MaxLength = 50, Example = "BCA", SortOrder = 3 },
                new() { Name = "bankCategory", Label = "Kategori Bank", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "bankCategoryOptions", SortOrder = 4 },
                new() { Name = "clearingCode", Label = "Clearing Code", Section = "Banking", InputType = "text", MaxLength = 50, Example = "014", SortOrder = 5 },
                new() { Name = "isDefault", Label = "Bank Default", Section = "Rule", InputType = "switch", SortOrder = 6 },
                new() { Name = "sortOrder", Label = "Urutan", Section = "Display", InputType = "number", SortOrder = 7 },
                new() { Name = "description", Label = "Deskripsi", Section = "Additional", InputType = "textarea", MaxLength = 250, SortOrder = 8 }
            };

            if (isUpdate)
            {
                fields.Add(new BankFormFieldMetadataResponse
                {
                    Name = "isActive",
                    Label = "Status Aktif",
                    Section = "Status",
                    InputType = "switch",
                    SortOrder = 99
                });
            }

            return fields.OrderBy(x => x.SortOrder).ToList();
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

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string? NormalizeUpperNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToUpperInvariant();
        }

        private sealed class DateRangeResolveResult
        {
            public bool IsValid { get; private set; }
            public string? ErrorMessage { get; private set; }
            public DateTime? Start { get; private set; }
            public DateTime? EndExclusive { get; private set; }

            public static DateRangeResolveResult Valid(DateTime? start, DateTime? endExclusive)
            {
                return new DateRangeResolveResult
                {
                    IsValid = true,
                    Start = start,
                    EndExclusive = endExclusive
                };
            }

            public static DateRangeResolveResult Invalid(string errorMessage)
            {
                return new DateRangeResolveResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}
