using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseCountryPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs.CountryResponse>;

using ResponseProvincePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs.ProvinceResponse>;

using ResponseCityPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs.CityResponse>;

using ResponseDistrictPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs.DistrictResponse>;

using ResponsePostalCodePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs.PostalCodeResponse>;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/administrator/master-data/regions")]
    [AccessController(
        moduleCode: "ADMINISTRATOR_MASTER_DATA",
        moduleName: "Administrator Master Data",
        displayName: "Region",
        AreaName = "Administrator",
        ControllerName = "Region",
        Description = "Master data wilayah negara, provinsi, kota, kecamatan, dan kode pos",
        SortOrder = 1
    )]
    [Tags("Administrator / Master Data / Region")]
    public class RegionController : ControllerBase
    {
        private const string LogCategory = "Administrator.MasterData";
        private const string KioskRegionReadPolicy = "KioskRegionRead";
        private const int CodeNumberLength = 5;
        private const string CountryCodePrefix = "CTR-RSMMC-";
        private const string ProvinceCodePrefix = "PRV-RSMMC-";
        private const string CityCodePrefix = "CTY-RSMMC-";
        private const string DistrictCodePrefix = "DST-RSMMC-";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public RegionController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [Authorize(Policy = KioskRegionReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<RegionFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Region", Description = "Melihat data region", AccessType = AccessTypes.Read, SortOrder = 1)]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new RegionFilterMetadataResponse
            {
                CountryDefaultFilter = BuildDefaultFilter(),
                ProvinceDefaultFilter = BuildDefaultFilter(),
                CityDefaultFilter = BuildDefaultFilter(),
                DistrictDefaultFilter = BuildDefaultFilter(),
                PostalCodeDefaultFilter = BuildDefaultFilter(),
                CustomPeriods = BuildCustomPeriodOptions(),
                CountrySortOptions = new List<RegionSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "countryCode", Label = "Kode country" },
                    new() { Value = "countryName", Label = "Nama country" },
                    new() { Value = "phoneCode", Label = "Kode telepon" },
                    new() { Value = "isDefault", Label = "Default" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                ProvinceSortOptions = new List<RegionSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "provinceCode", Label = "Kode province" },
                    new() { Value = "provinceName", Label = "Nama province" },
                    new() { Value = "countryName", Label = "Nama country" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                CitySortOptions = new List<RegionSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "cityCode", Label = "Kode city" },
                    new() { Value = "cityName", Label = "Nama city" },
                    new() { Value = "cityType", Label = "Tipe city" },
                    new() { Value = "provinceName", Label = "Nama province" },
                    new() { Value = "countryName", Label = "Nama country" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                DistrictSortOptions = new List<RegionSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "districtCode", Label = "Kode district" },
                    new() { Value = "districtName", Label = "Nama district" },
                    new() { Value = "cityName", Label = "Nama city" },
                    new() { Value = "provinceName", Label = "Nama province" },
                    new() { Value = "countryName", Label = "Nama country" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                PostalCodeSortOptions = new List<RegionSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "postalCode", Label = "Kode pos" },
                    new() { Value = "villageName", Label = "Kelurahan/Desa" },
                    new() { Value = "districtName", Label = "Nama district" },
                    new() { Value = "cityName", Label = "Nama city" },
                    new() { Value = "provinceName", Label = "Nama province" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Region.GetFilterMetadata",
                "Mengambil metadata filter region.",
                result
            );

            return Ok(ApiResponse<RegionFilterMetadataResponse>.Ok(
                result,
                "Metadata filter region berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<RegionSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Region", Description = "Melihat data region", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Region", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var countries = _dbContext.MstCountries.AsNoTracking().Where(x => !x.IsDelete);
            var provinces = _dbContext.MstProvinces.AsNoTracking().Where(x => !x.IsDelete);
            var cities = _dbContext.MstCities.AsNoTracking().Where(x => !x.IsDelete);
            var districts = _dbContext.MstDistricts.AsNoTracking().Where(x => !x.IsDelete);
            var postalCodes = _dbContext.MstPostalCodes.AsNoTracking().Where(x => !x.IsDelete);

            var result = new RegionSummaryResponse
            {
                TotalCountry = await countries.CountAsync(),
                ActiveCountry = await countries.CountAsync(x => x.IsActive),
                InactiveCountry = await countries.CountAsync(x => !x.IsActive),
                DefaultCountry = await countries.CountAsync(x => x.IsDefault),

                TotalProvince = await provinces.CountAsync(),
                ActiveProvince = await provinces.CountAsync(x => x.IsActive),
                InactiveProvince = await provinces.CountAsync(x => !x.IsActive),

                TotalCity = await cities.CountAsync(),
                ActiveCity = await cities.CountAsync(x => x.IsActive),
                InactiveCity = await cities.CountAsync(x => !x.IsActive),

                TotalDistrict = await districts.CountAsync(),
                ActiveDistrict = await districts.CountAsync(x => x.IsActive),
                InactiveDistrict = await districts.CountAsync(x => !x.IsActive),

                TotalPostalCode = await postalCodes.CountAsync(),
                ActivePostalCode = await postalCodes.CountAsync(x => x.IsActive),
                InactivePostalCode = await postalCodes.CountAsync(x => !x.IsActive)
            };

            return Ok(ApiResponse<RegionSummaryResponse>.Ok(
                result,
                "Ringkasan region berhasil diambil."
            ));
        }

        // =========================================================
        // COUNTRY
        // =========================================================

        [HttpGet("countries")]
        [Authorize(Policy = KioskRegionReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<ResponseCountryPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Region", Description = "Melihat data country", AccessType = AccessTypes.Read, SortOrder = 1)]
        public async Task<IActionResult> GetCountries(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] bool? isDefault,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
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

            var query = _dbContext.MstCountries
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            query = ApplyDateFilter(query, dateRange);

            if (isDefault.HasValue)
                query = query.Where(x => x.IsDefault == isDefault.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.CountryCode.ToLower().Contains(keyword) ||
                    x.CountryName.ToLower().Contains(keyword) ||
                    (x.PhoneCode != null && x.PhoneCode.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var entities = await ApplyCountrySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities.Select(x => x.CreateBy)
            );

            var result = new ResponseCountryPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(x => MapCountryResponse(x, actorNames)).ToList()
            };

            return Ok(ApiResponse<ResponseCountryPagedResult>.Ok(
                result,
                "Data country berhasil diambil."
            ));
        }

        [HttpGet("countries/options")]
        [Authorize(Policy = KioskRegionReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<RegionOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Region", Description = "Melihat pilihan country", AccessType = AccessTypes.Read, SortOrder = 1)]
        public async Task<IActionResult> GetCountryOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.MstCountries
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.CountryCode.ToLower().Contains(keyword) ||
                    x.CountryName.ToLower().Contains(keyword) ||
                    (x.PhoneCode != null && x.PhoneCode.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.CountryName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RegionOptionResponse
                {
                    Id = x.Id,
                    Code = x.CountryCode,
                    Name = x.CountryName,
                    ParentId = null,
                    ParentCode = null,
                    ParentName = null,
                    AdditionalInfo = x.PhoneCode,
                    IsDefault = x.IsDefault,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return Ok(ApiResponse<RegionOptionPagedResponse>.Ok(
                BuildOptionPagedResponse(pageNumber, pageSize, totalData, items),
                "Pilihan country berhasil diambil."
            ));
        }

        [HttpGet("countries/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CountryDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Region", Description = "Melihat detail country", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Region", "Read")]
        public async Task<IActionResult> GetCountryById(Guid id)
        {
            var entity = await _dbContext.MstCountries
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Country tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            return Ok(ApiResponse<CountryDetailResponse>.Ok(
                MapCountryDetailResponse(entity, actorNames),
                "Detail country berhasil diambil."
            ));
        }

        [HttpPost("countries")]
        [ProducesResponseType(typeof(ApiResponse<CountryDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Region", Description = "Membuat data country", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Region", "Create")]
        public async Task<IActionResult> CreateCountry([FromBody] CreateCountryRequest request)
        {
            var validation = await ValidateCountryRequestAsync(null, request.CountryName, request.PhoneCode);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data country tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (request.IsDefault)
                {
                    await ResetDefaultCountryAsync(null, now, actorUserId);
                }

                var entity = new MstCountry
                {
                    Id = Guid.NewGuid(),
                    CountryCode = await GenerateCountryCodeAsync(),
                    CountryName = request.CountryName.Trim(),
                    PhoneCode = NormalizeNullableText(request.PhoneCode),
                    IsDefault = request.IsDefault,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.MstCountries.Add(entity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "Region.CreateCountry",
                    "Membuat data country.",
                    new { entity.Id, entity.CountryCode, entity.CountryName }
                );

                return await GetCountryById(entity.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "Region.CreateCountry",
                    "Gagal membuat data country.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat country."
                    )
                );
            }
        }

        [HttpPut("countries/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CountryDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Region", Description = "Mengubah data country", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Region", "Update")]
        public async Task<IActionResult> UpdateCountry(Guid id, [FromBody] UpdateCountryRequest request)
        {
            var entity = await _dbContext.MstCountries
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Country tidak ditemukan."
                ));
            }

            if (request.IsDefault && !request.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Country tidak aktif tidak bisa dijadikan default."
                ));
            }

            var validation = await ValidateCountryRequestAsync(id, request.CountryName, request.PhoneCode);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data country tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (request.IsDefault)
                {
                    await ResetDefaultCountryAsync(id, now, actorUserId);
                }

                entity.CountryName = request.CountryName.Trim();
                entity.PhoneCode = NormalizeNullableText(request.PhoneCode);
                entity.IsDefault = request.IsActive && request.IsDefault;
                entity.IsActive = request.IsActive;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "Region.UpdateCountry",
                    "Mengubah data country.",
                    new { entity.Id, entity.CountryCode, entity.CountryName }
                );

                return await GetCountryById(entity.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "Region.UpdateCountry",
                    "Gagal mengubah data country.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat memperbarui country."
                    )
                );
            }
        }

        [HttpPatch("countries/{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<CountryDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Region Status", Description = "Mengubah status country", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("Region", "Update")]
        public async Task<IActionResult> UpdateCountryStatus(Guid id, [FromBody] UpdateRegionStatusRequest request)
        {
            var entity = await _dbContext.MstCountries
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Country tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;

            if (!request.IsActive)
            {
                entity.IsDefault = false;
            }

            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return await GetCountryById(entity.Id);
        }

        [HttpDelete("countries/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Region", Description = "Menghapus data country", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("Region", "Delete")]
        public async Task<IActionResult> DeleteCountry(Guid id, [FromBody] DeleteRegionRequest? request = null)
        {
            var entity = await _dbContext.MstCountries
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Country tidak ditemukan."
                ));
            }

            var hasChild = await _dbContext.MstProvinces
                .AnyAsync(x => x.CountryId == id && !x.IsDelete);

            if (hasChild)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Country tidak dapat dihapus karena masih memiliki province."
                ));
            }

            entity.IsActive = false;
            entity.IsDefault = false;
            ApplyDeleteAudit(entity);

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Region.DeleteCountry",
                "Menghapus data country.",
                new { entity.Id, entity.CountryCode, entity.CountryName, request?.DeleteReason }
            );

            return Ok(ApiResponse<object>.Ok(null, "Country berhasil dihapus."));
        }

        // =========================================================
        // PROVINCE
        // =========================================================

        [HttpGet("provinces")]
        [Authorize(Policy = KioskRegionReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<ResponseProvincePagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Region", Description = "Melihat data province", AccessType = AccessTypes.Read, SortOrder = 1)]
        public async Task<IActionResult> GetProvinces(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? countryId,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
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

            var query = _dbContext.MstProvinces
                .Include(x => x.Country)
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            query = ApplyDateFilter(query, dateRange);

            if (countryId.HasValue && countryId.Value != Guid.Empty)
                query = query.Where(x => x.CountryId == countryId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ProvinceCode.ToLower().Contains(keyword) ||
                    x.ProvinceName.ToLower().Contains(keyword) ||
                    (x.Country != null && x.Country.CountryCode.ToLower().Contains(keyword)) ||
                    (x.Country != null && x.Country.CountryName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var entities = await ApplyProvinceSorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities.Select(x => x.CreateBy)
            );

            var result = new ResponseProvincePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(x => MapProvinceResponse(x, actorNames)).ToList()
            };

            return Ok(ApiResponse<ResponseProvincePagedResult>.Ok(
                result,
                "Data province berhasil diambil."
            ));
        }

        [HttpGet("provinces/options")]
        [Authorize(Policy = KioskRegionReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<RegionOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Region", Description = "Melihat pilihan province", AccessType = AccessTypes.Read, SortOrder = 1)]
        public async Task<IActionResult> GetProvinceOptions(
            [FromQuery] Guid? countryId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.MstProvinces
                .Include(x => x.Country)
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (countryId.HasValue && countryId.Value != Guid.Empty)
                query = query.Where(x => x.CountryId == countryId.Value);

            if (onlyActive)
                query = query.Where(x => x.IsActive && x.Country != null && x.Country.IsActive && !x.Country.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ProvinceCode.ToLower().Contains(keyword) ||
                    x.ProvinceName.ToLower().Contains(keyword) ||
                    (x.Country != null && x.Country.CountryName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.ProvinceName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RegionOptionResponse
                {
                    Id = x.Id,
                    Code = x.ProvinceCode,
                    Name = x.ProvinceName,
                    ParentId = x.CountryId,
                    ParentCode = x.Country != null ? x.Country.CountryCode : null,
                    ParentName = x.Country != null ? x.Country.CountryName : null,
                    AdditionalInfo = null,
                    IsDefault = false,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return Ok(ApiResponse<RegionOptionPagedResponse>.Ok(
                BuildOptionPagedResponse(pageNumber, pageSize, totalData, items),
                "Pilihan province berhasil diambil."
            ));
        }

        [HttpGet("provinces/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ProvinceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Region", Description = "Melihat detail province", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Region", "Read")]
        public async Task<IActionResult> GetProvinceById(Guid id)
        {
            var entity = await _dbContext.MstProvinces
                .Include(x => x.Country)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Province tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            return Ok(ApiResponse<ProvinceDetailResponse>.Ok(
                MapProvinceDetailResponse(entity, actorNames),
                "Detail province berhasil diambil."
            ));
        }

        [HttpPost("provinces")]
        [ProducesResponseType(typeof(ApiResponse<ProvinceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Region", Description = "Membuat data province", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Region", "Create")]
        public async Task<IActionResult> CreateProvince([FromBody] CreateProvinceRequest request)
        {
            var validation = await ValidateProvinceRequestAsync(null, request.CountryId, request.ProvinceName);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data province tidak valid."
                ));
            }

            var entity = new MstProvince
            {
                Id = Guid.NewGuid(),
                CountryId = request.CountryId,
                ProvinceCode = await GenerateProvinceCodeAsync(),
                ProvinceName = request.ProvinceName.Trim(),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstProvinces.Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Region.CreateProvince",
                "Membuat data province.",
                new { entity.Id, entity.ProvinceCode, entity.ProvinceName }
            );

            return await GetProvinceById(entity.Id);
        }

        [HttpPut("provinces/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ProvinceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Region", Description = "Mengubah data province", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Region", "Update")]
        public async Task<IActionResult> UpdateProvince(Guid id, [FromBody] UpdateProvinceRequest request)
        {
            var entity = await _dbContext.MstProvinces
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Province tidak ditemukan."
                ));
            }

            var validation = await ValidateProvinceRequestAsync(id, request.CountryId, request.ProvinceName);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data province tidak valid."
                ));
            }

            entity.CountryId = request.CountryId;
            entity.ProvinceName = request.ProvinceName.Trim();
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Region.UpdateProvince",
                "Mengubah data province.",
                new { entity.Id, entity.ProvinceCode, entity.ProvinceName }
            );

            return await GetProvinceById(entity.Id);
        }

        [HttpPatch("provinces/{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<ProvinceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Region Status", Description = "Mengubah status province", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("Region", "Update")]
        public async Task<IActionResult> UpdateProvinceStatus(Guid id, [FromBody] UpdateRegionStatusRequest request)
        {
            var entity = await _dbContext.MstProvinces
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Province tidak ditemukan."
                ));
            }

            if (request.IsActive && !await CountryExistsAsync(entity.CountryId))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Province tidak bisa aktif ketika country tidak aktif."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return await GetProvinceById(entity.Id);
        }

        [HttpDelete("provinces/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Region", Description = "Menghapus data province", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("Region", "Delete")]
        public async Task<IActionResult> DeleteProvince(Guid id, [FromBody] DeleteRegionRequest? request = null)
        {
            var entity = await _dbContext.MstProvinces
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Province tidak ditemukan."
                ));
            }

            var hasChild = await _dbContext.MstCities
                .AnyAsync(x => x.ProvinceId == id && !x.IsDelete);

            if (hasChild)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Province tidak dapat dihapus karena masih memiliki city."
                ));
            }

            entity.IsActive = false;
            ApplyDeleteAudit(entity);

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Region.DeleteProvince",
                "Menghapus data province.",
                new { entity.Id, entity.ProvinceCode, entity.ProvinceName, request?.DeleteReason }
            );

            return Ok(ApiResponse<object>.Ok(null, "Province berhasil dihapus."));
        }

        // =========================================================
        // CITY
        // =========================================================

        [HttpGet("cities")]
        [Authorize(Policy = KioskRegionReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<ResponseCityPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Region", Description = "Melihat data city", AccessType = AccessTypes.Read, SortOrder = 1)]
        public async Task<IActionResult> GetCities(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? countryId,
            [FromQuery] Guid? provinceId,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
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

            var query = _dbContext.MstCities
                .Include(x => x.Province)
                    .ThenInclude(x => x!.Country)
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            query = ApplyDateFilter(query, dateRange);

            if (countryId.HasValue && countryId.Value != Guid.Empty)
                query = query.Where(x => x.Province != null && x.Province.CountryId == countryId.Value);

            if (provinceId.HasValue && provinceId.Value != Guid.Empty)
                query = query.Where(x => x.ProvinceId == provinceId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.CityCode.ToLower().Contains(keyword) ||
                    x.CityName.ToLower().Contains(keyword) ||
                    (x.CityType != null && x.CityType.ToLower().Contains(keyword)) ||
                    (x.Province != null && x.Province.ProvinceName.ToLower().Contains(keyword)) ||
                    (x.Province != null && x.Province.Country != null && x.Province.Country.CountryName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var entities = await ApplyCitySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities.Select(x => x.CreateBy)
            );

            var result = new ResponseCityPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(x => MapCityResponse(x, actorNames)).ToList()
            };

            return Ok(ApiResponse<ResponseCityPagedResult>.Ok(
                result,
                "Data city berhasil diambil."
            ));
        }

        [HttpGet("cities/options")]
        [Authorize(Policy = KioskRegionReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<RegionOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Region", Description = "Melihat pilihan city", AccessType = AccessTypes.Read, SortOrder = 1)]
        public async Task<IActionResult> GetCityOptions(
            [FromQuery] Guid? countryId,
            [FromQuery] Guid? provinceId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.MstCities
                .Include(x => x.Province)
                    .ThenInclude(x => x!.Country)
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (countryId.HasValue && countryId.Value != Guid.Empty)
                query = query.Where(x => x.Province != null && x.Province.CountryId == countryId.Value);

            if (provinceId.HasValue && provinceId.Value != Guid.Empty)
                query = query.Where(x => x.ProvinceId == provinceId.Value);

            if (onlyActive)
                query = query.Where(x => x.IsActive && x.Province != null && x.Province.IsActive && !x.Province.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.CityCode.ToLower().Contains(keyword) ||
                    x.CityName.ToLower().Contains(keyword) ||
                    (x.CityType != null && x.CityType.ToLower().Contains(keyword)) ||
                    (x.Province != null && x.Province.ProvinceName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.CityName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RegionOptionResponse
                {
                    Id = x.Id,
                    Code = x.CityCode,
                    Name = x.CityName,
                    ParentId = x.ProvinceId,
                    ParentCode = x.Province != null ? x.Province.ProvinceCode : null,
                    ParentName = x.Province != null ? x.Province.ProvinceName : null,
                    AdditionalInfo = x.CityType,
                    IsDefault = false,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return Ok(ApiResponse<RegionOptionPagedResponse>.Ok(
                BuildOptionPagedResponse(pageNumber, pageSize, totalData, items),
                "Pilihan city berhasil diambil."
            ));
        }

        [HttpGet("cities/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CityDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Region", Description = "Melihat detail city", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Region", "Read")]
        public async Task<IActionResult> GetCityById(Guid id)
        {
            var entity = await _dbContext.MstCities
                .Include(x => x.Province)
                    .ThenInclude(x => x!.Country)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "City tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            return Ok(ApiResponse<CityDetailResponse>.Ok(
                MapCityDetailResponse(entity, actorNames),
                "Detail city berhasil diambil."
            ));
        }

        [HttpPost("cities")]
        [ProducesResponseType(typeof(ApiResponse<CityDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Region", Description = "Membuat data city", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Region", "Create")]
        public async Task<IActionResult> CreateCity([FromBody] CreateCityRequest request)
        {
            var validation = await ValidateCityRequestAsync(null, request.ProvinceId, request.CityName, request.CityType);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data city tidak valid."
                ));
            }

            var entity = new MstCity
            {
                Id = Guid.NewGuid(),
                ProvinceId = request.ProvinceId,
                CityCode = await GenerateCityCodeAsync(),
                CityName = request.CityName.Trim(),
                CityType = NormalizeNullableText(request.CityType),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstCities.Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Region.CreateCity",
                "Membuat data city.",
                new { entity.Id, entity.CityCode, entity.CityName }
            );

            return await GetCityById(entity.Id);
        }

        [HttpPut("cities/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CityDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Region", Description = "Mengubah data city", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Region", "Update")]
        public async Task<IActionResult> UpdateCity(Guid id, [FromBody] UpdateCityRequest request)
        {
            var entity = await _dbContext.MstCities
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "City tidak ditemukan."
                ));
            }

            var validation = await ValidateCityRequestAsync(id, request.ProvinceId, request.CityName, request.CityType);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data city tidak valid."
                ));
            }

            entity.ProvinceId = request.ProvinceId;
            entity.CityName = request.CityName.Trim();
            entity.CityType = NormalizeNullableText(request.CityType);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Region.UpdateCity",
                "Mengubah data city.",
                new { entity.Id, entity.CityCode, entity.CityName }
            );

            return await GetCityById(entity.Id);
        }

        [HttpPatch("cities/{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<CityDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Region Status", Description = "Mengubah status city", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("Region", "Update")]
        public async Task<IActionResult> UpdateCityStatus(Guid id, [FromBody] UpdateRegionStatusRequest request)
        {
            var entity = await _dbContext.MstCities
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "City tidak ditemukan."
                ));
            }

            if (request.IsActive && !await ProvinceExistsAsync(entity.ProvinceId))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "City tidak bisa aktif ketika province tidak aktif."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return await GetCityById(entity.Id);
        }

        [HttpDelete("cities/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Region", Description = "Menghapus data city", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("Region", "Delete")]
        public async Task<IActionResult> DeleteCity(Guid id, [FromBody] DeleteRegionRequest? request = null)
        {
            var entity = await _dbContext.MstCities
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "City tidak ditemukan."
                ));
            }

            var hasChild = await _dbContext.MstDistricts
                .AnyAsync(x => x.CityId == id && !x.IsDelete);

            if (hasChild)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "City tidak dapat dihapus karena masih memiliki district."
                ));
            }

            entity.IsActive = false;
            ApplyDeleteAudit(entity);

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Region.DeleteCity",
                "Menghapus data city.",
                new { entity.Id, entity.CityCode, entity.CityName, request?.DeleteReason }
            );

            return Ok(ApiResponse<object>.Ok(null, "City berhasil dihapus."));
        }

        // =========================================================
        // DISTRICT
        // =========================================================

        [HttpGet("districts")]
        [Authorize(Policy = KioskRegionReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<ResponseDistrictPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Region", Description = "Melihat data district", AccessType = AccessTypes.Read, SortOrder = 1)]
        public async Task<IActionResult> GetDistricts(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? countryId,
            [FromQuery] Guid? provinceId,
            [FromQuery] Guid? cityId,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
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

            var query = _dbContext.MstDistricts
                .Include(x => x.City)
                    .ThenInclude(x => x!.Province)
                        .ThenInclude(x => x!.Country)
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            query = ApplyDateFilter(query, dateRange);

            if (countryId.HasValue && countryId.Value != Guid.Empty)
                query = query.Where(x => x.City != null && x.City.Province != null && x.City.Province.CountryId == countryId.Value);

            if (provinceId.HasValue && provinceId.Value != Guid.Empty)
                query = query.Where(x => x.City != null && x.City.ProvinceId == provinceId.Value);

            if (cityId.HasValue && cityId.Value != Guid.Empty)
                query = query.Where(x => x.CityId == cityId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.DistrictCode.ToLower().Contains(keyword) ||
                    x.DistrictName.ToLower().Contains(keyword) ||
                    (x.City != null && x.City.CityName.ToLower().Contains(keyword)) ||
                    (x.City != null && x.City.Province != null && x.City.Province.ProvinceName.ToLower().Contains(keyword)) ||
                    (x.City != null && x.City.Province != null && x.City.Province.Country != null && x.City.Province.Country.CountryName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var entities = await ApplyDistrictSorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities.Select(x => x.CreateBy)
            );

            var result = new ResponseDistrictPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(x => MapDistrictResponse(x, actorNames)).ToList()
            };

            return Ok(ApiResponse<ResponseDistrictPagedResult>.Ok(
                result,
                "Data district berhasil diambil."
            ));
        }

        [HttpGet("districts/options")]
        [Authorize(Policy = KioskRegionReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<RegionOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Region", Description = "Melihat pilihan district", AccessType = AccessTypes.Read, SortOrder = 1)]
        public async Task<IActionResult> GetDistrictOptions(
            [FromQuery] Guid? countryId,
            [FromQuery] Guid? provinceId,
            [FromQuery] Guid? cityId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.MstDistricts
                .Include(x => x.City)
                    .ThenInclude(x => x!.Province)
                        .ThenInclude(x => x!.Country)
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (countryId.HasValue && countryId.Value != Guid.Empty)
                query = query.Where(x => x.City != null && x.City.Province != null && x.City.Province.CountryId == countryId.Value);

            if (provinceId.HasValue && provinceId.Value != Guid.Empty)
                query = query.Where(x => x.City != null && x.City.ProvinceId == provinceId.Value);

            if (cityId.HasValue && cityId.Value != Guid.Empty)
                query = query.Where(x => x.CityId == cityId.Value);

            if (onlyActive)
                query = query.Where(x => x.IsActive && x.City != null && x.City.IsActive && !x.City.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.DistrictCode.ToLower().Contains(keyword) ||
                    x.DistrictName.ToLower().Contains(keyword) ||
                    (x.City != null && x.City.CityName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.DistrictName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RegionOptionResponse
                {
                    Id = x.Id,
                    Code = x.DistrictCode,
                    Name = x.DistrictName,
                    ParentId = x.CityId,
                    ParentCode = x.City != null ? x.City.CityCode : null,
                    ParentName = x.City != null ? x.City.CityName : null,
                    AdditionalInfo = null,
                    IsDefault = false,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return Ok(ApiResponse<RegionOptionPagedResponse>.Ok(
                BuildOptionPagedResponse(pageNumber, pageSize, totalData, items),
                "Pilihan district berhasil diambil."
            ));
        }

        [HttpGet("districts/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DistrictDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Region", Description = "Melihat detail district", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Region", "Read")]
        public async Task<IActionResult> GetDistrictById(Guid id)
        {
            var entity = await _dbContext.MstDistricts
                .Include(x => x.City)
                    .ThenInclude(x => x!.Province)
                        .ThenInclude(x => x!.Country)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "District tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            return Ok(ApiResponse<DistrictDetailResponse>.Ok(
                MapDistrictDetailResponse(entity, actorNames),
                "Detail district berhasil diambil."
            ));
        }

        [HttpPost("districts")]
        [ProducesResponseType(typeof(ApiResponse<DistrictDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Region", Description = "Membuat data district", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Region", "Create")]
        public async Task<IActionResult> CreateDistrict([FromBody] CreateDistrictRequest request)
        {
            var validation = await ValidateDistrictRequestAsync(null, request.CityId, request.DistrictName);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data district tidak valid."
                ));
            }

            var entity = new MstDistrict
            {
                Id = Guid.NewGuid(),
                CityId = request.CityId,
                DistrictCode = await GenerateDistrictCodeAsync(),
                DistrictName = request.DistrictName.Trim(),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstDistricts.Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Region.CreateDistrict",
                "Membuat data district.",
                new { entity.Id, entity.DistrictCode, entity.DistrictName }
            );

            return await GetDistrictById(entity.Id);
        }

        [HttpPut("districts/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DistrictDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Region", Description = "Mengubah data district", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Region", "Update")]
        public async Task<IActionResult> UpdateDistrict(Guid id, [FromBody] UpdateDistrictRequest request)
        {
            var entity = await _dbContext.MstDistricts
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "District tidak ditemukan."
                ));
            }

            var validation = await ValidateDistrictRequestAsync(id, request.CityId, request.DistrictName);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data district tidak valid."
                ));
            }

            entity.CityId = request.CityId;
            entity.DistrictName = request.DistrictName.Trim();
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Region.UpdateDistrict",
                "Mengubah data district.",
                new { entity.Id, entity.DistrictCode, entity.DistrictName }
            );

            return await GetDistrictById(entity.Id);
        }

        [HttpPatch("districts/{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<DistrictDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Region Status", Description = "Mengubah status district", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("Region", "Update")]
        public async Task<IActionResult> UpdateDistrictStatus(Guid id, [FromBody] UpdateRegionStatusRequest request)
        {
            var entity = await _dbContext.MstDistricts
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "District tidak ditemukan."
                ));
            }

            if (request.IsActive && !await CityExistsAsync(entity.CityId))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "District tidak bisa aktif ketika city tidak aktif."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return await GetDistrictById(entity.Id);
        }

        [HttpDelete("districts/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Region", Description = "Menghapus data district", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("Region", "Delete")]
        public async Task<IActionResult> DeleteDistrict(Guid id, [FromBody] DeleteRegionRequest? request = null)
        {
            var entity = await _dbContext.MstDistricts
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "District tidak ditemukan."
                ));
            }

            var hasChild = await _dbContext.MstPostalCodes
                .AnyAsync(x => x.DistrictId == id && !x.IsDelete);

            if (hasChild)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "District tidak dapat dihapus karena masih memiliki postal code."
                ));
            }

            entity.IsActive = false;
            ApplyDeleteAudit(entity);

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Region.DeleteDistrict",
                "Menghapus data district.",
                new { entity.Id, entity.DistrictCode, entity.DistrictName, request?.DeleteReason }
            );

            return Ok(ApiResponse<object>.Ok(null, "District berhasil dihapus."));
        }

        // =========================================================
        // POSTAL CODE
        // =========================================================

        [HttpGet("postal-codes")]
        [Authorize(Policy = KioskRegionReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<ResponsePostalCodePagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Region", Description = "Melihat data postal code", AccessType = AccessTypes.Read, SortOrder = 1)]
        public async Task<IActionResult> GetPostalCodes(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? countryId,
            [FromQuery] Guid? provinceId,
            [FromQuery] Guid? cityId,
            [FromQuery] Guid? districtId,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
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

            var query = _dbContext.MstPostalCodes
                .Include(x => x.District)
                    .ThenInclude(x => x!.City)
                        .ThenInclude(x => x!.Province)
                            .ThenInclude(x => x!.Country)
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            query = ApplyDateFilter(query, dateRange);

            if (countryId.HasValue && countryId.Value != Guid.Empty)
                query = query.Where(x => x.District != null && x.District.City != null && x.District.City.Province != null && x.District.City.Province.CountryId == countryId.Value);

            if (provinceId.HasValue && provinceId.Value != Guid.Empty)
                query = query.Where(x => x.District != null && x.District.City != null && x.District.City.ProvinceId == provinceId.Value);

            if (cityId.HasValue && cityId.Value != Guid.Empty)
                query = query.Where(x => x.District != null && x.District.CityId == cityId.Value);

            if (districtId.HasValue && districtId.Value != Guid.Empty)
                query = query.Where(x => x.DistrictId == districtId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.PostalCode.ToLower().Contains(keyword) ||
                    (x.VillageName != null && x.VillageName.ToLower().Contains(keyword)) ||
                    (x.District != null && x.District.DistrictName.ToLower().Contains(keyword)) ||
                    (x.District != null && x.District.City != null && x.District.City.CityName.ToLower().Contains(keyword)) ||
                    (x.District != null && x.District.City != null && x.District.City.Province != null && x.District.City.Province.ProvinceName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var entities = await ApplyPostalCodeSorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities.Select(x => x.CreateBy)
            );

            var result = new ResponsePostalCodePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(x => MapPostalCodeResponse(x, actorNames)).ToList()
            };

            return Ok(ApiResponse<ResponsePostalCodePagedResult>.Ok(
                result,
                "Data postal code berhasil diambil."
            ));
        }

        [HttpGet("postal-codes/options")]
        [Authorize(Policy = KioskRegionReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<RegionOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Region", Description = "Melihat pilihan postal code", AccessType = AccessTypes.Read, SortOrder = 1)]
        public async Task<IActionResult> GetPostalCodeOptions(
            [FromQuery] Guid? countryId,
            [FromQuery] Guid? provinceId,
            [FromQuery] Guid? cityId,
            [FromQuery] Guid? districtId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.MstPostalCodes
                .Include(x => x.District)
                    .ThenInclude(x => x!.City)
                        .ThenInclude(x => x!.Province)
                            .ThenInclude(x => x!.Country)
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (countryId.HasValue && countryId.Value != Guid.Empty)
                query = query.Where(x => x.District != null && x.District.City != null && x.District.City.Province != null && x.District.City.Province.CountryId == countryId.Value);

            if (provinceId.HasValue && provinceId.Value != Guid.Empty)
                query = query.Where(x => x.District != null && x.District.City != null && x.District.City.ProvinceId == provinceId.Value);

            if (cityId.HasValue && cityId.Value != Guid.Empty)
                query = query.Where(x => x.District != null && x.District.CityId == cityId.Value);

            if (districtId.HasValue && districtId.Value != Guid.Empty)
                query = query.Where(x => x.DistrictId == districtId.Value);

            if (onlyActive)
                query = query.Where(x => x.IsActive && x.District != null && x.District.IsActive && !x.District.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.PostalCode.ToLower().Contains(keyword) ||
                    (x.VillageName != null && x.VillageName.ToLower().Contains(keyword)) ||
                    (x.District != null && x.District.DistrictName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.VillageName)
                .ThenBy(x => x.PostalCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RegionOptionResponse
                {
                    Id = x.Id,
                    Code = x.PostalCode,
                    Name = x.VillageName ?? x.PostalCode,
                    ParentId = x.DistrictId,
                    ParentCode = x.District != null ? x.District.DistrictCode : null,
                    ParentName = x.District != null ? x.District.DistrictName : null,
                    AdditionalInfo = x.PostalCode,
                    IsDefault = false,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return Ok(ApiResponse<RegionOptionPagedResponse>.Ok(
                BuildOptionPagedResponse(pageNumber, pageSize, totalData, items),
                "Pilihan postal code berhasil diambil."
            ));
        }

        [HttpGet("postal-codes/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PostalCodeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Region", Description = "Melihat detail postal code", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Region", "Read")]
        public async Task<IActionResult> GetPostalCodeById(Guid id)
        {
            var entity = await _dbContext.MstPostalCodes
                .Include(x => x.District)
                    .ThenInclude(x => x!.City)
                        .ThenInclude(x => x!.Province)
                            .ThenInclude(x => x!.Country)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Postal code tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            return Ok(ApiResponse<PostalCodeDetailResponse>.Ok(
                MapPostalCodeDetailResponse(entity, actorNames),
                "Detail postal code berhasil diambil."
            ));
        }

        [HttpPost("postal-codes")]
        [ProducesResponseType(typeof(ApiResponse<PostalCodeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Region", Description = "Membuat data postal code", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Region", "Create")]
        public async Task<IActionResult> CreatePostalCode([FromBody] CreatePostalCodeRequest request)
        {
            var validation = await ValidatePostalCodeRequestAsync(null, request.DistrictId, request.PostalCode, request.VillageName);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data postal code tidak valid."
                ));
            }

            var entity = new MstPostalCode
            {
                Id = Guid.NewGuid(),
                DistrictId = request.DistrictId,
                PostalCode = request.PostalCode.Trim(),
                VillageName = NormalizeNullableText(request.VillageName),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstPostalCodes.Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Region.CreatePostalCode",
                "Membuat data postal code.",
                new { entity.Id, entity.PostalCode, entity.VillageName }
            );

            return await GetPostalCodeById(entity.Id);
        }

        [HttpPut("postal-codes/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PostalCodeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Region", Description = "Mengubah data postal code", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Region", "Update")]
        public async Task<IActionResult> UpdatePostalCode(Guid id, [FromBody] UpdatePostalCodeRequest request)
        {
            var entity = await _dbContext.MstPostalCodes
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Postal code tidak ditemukan."
                ));
            }

            var validation = await ValidatePostalCodeRequestAsync(id, request.DistrictId, request.PostalCode, request.VillageName);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data postal code tidak valid."
                ));
            }

            entity.DistrictId = request.DistrictId;
            entity.PostalCode = request.PostalCode.Trim();
            entity.VillageName = NormalizeNullableText(request.VillageName);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Region.UpdatePostalCode",
                "Mengubah data postal code.",
                new { entity.Id, entity.PostalCode, entity.VillageName }
            );

            return await GetPostalCodeById(entity.Id);
        }

        [HttpPatch("postal-codes/{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<PostalCodeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Region Status", Description = "Mengubah status postal code", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("Region", "Update")]
        public async Task<IActionResult> UpdatePostalCodeStatus(Guid id, [FromBody] UpdateRegionStatusRequest request)
        {
            var entity = await _dbContext.MstPostalCodes
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Postal code tidak ditemukan."
                ));
            }

            if (request.IsActive && !await DistrictExistsAsync(entity.DistrictId))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Postal code tidak bisa aktif ketika district tidak aktif."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return await GetPostalCodeById(entity.Id);
        }

        [HttpDelete("postal-codes/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Region", Description = "Menghapus data postal code", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("Region", "Delete")]
        public async Task<IActionResult> DeletePostalCode(Guid id, [FromBody] DeleteRegionRequest? request = null)
        {
            var entity = await _dbContext.MstPostalCodes
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Postal code tidak ditemukan."
                ));
            }

            entity.IsActive = false;
            ApplyDeleteAudit(entity);

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Region.DeletePostalCode",
                "Menghapus data postal code.",
                new { entity.Id, entity.PostalCode, entity.VillageName, request?.DeleteReason }
            );

            return Ok(ApiResponse<object>.Ok(null, "Postal code berhasil dihapus."));
        }

        // =========================================================
        // PRIVATE HELPERS
        // =========================================================

        private static RegionDefaultFilterResponse BuildDefaultFilter()
        {
            return new RegionDefaultFilterResponse
            {
                SortBy = "createDateTime",
                SortDirection = "desc",
                PageNumber = 1,
                PageSize = 25
            };
        }

        private static IQueryable<T> ApplyDateFilter<T>(IQueryable<T> query, DateRangeResolveResult dateRange)
            where T : IdentityModel
        {
            if (dateRange.Start.HasValue)
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);

            if (dateRange.EndExclusive.HasValue)
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);

            return query;
        }

        private async Task ResetDefaultCountryAsync(Guid? excludeId, DateTime now, Guid actorUserId)
        {
            var countries = await _dbContext.MstCountries
                .Where(x =>
                    x.IsDefault &&
                    !x.IsDelete &&
                    (!excludeId.HasValue || x.Id != excludeId.Value))
                .ToListAsync();

            foreach (var country in countries)
            {
                country.IsDefault = false;
                country.UpdateDateTime = now;
                country.UpdateBy = actorUserId;
            }
        }

        private async Task<bool> CountryExistsAsync(Guid id)
        {
            return id != Guid.Empty && await _dbContext.MstCountries
                .AsNoTracking()
                .AnyAsync(x => x.Id == id && x.IsActive && !x.IsDelete);
        }

        private async Task<bool> ProvinceExistsAsync(Guid id)
        {
            return id != Guid.Empty && await _dbContext.MstProvinces
                .Include(x => x.Country)
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == id &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.Country != null &&
                    x.Country.IsActive &&
                    !x.Country.IsDelete);
        }

        private async Task<bool> CityExistsAsync(Guid id)
        {
            return id != Guid.Empty && await _dbContext.MstCities
                .Include(x => x.Province)
                    .ThenInclude(x => x!.Country)
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == id &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.Province != null &&
                    x.Province.IsActive &&
                    !x.Province.IsDelete &&
                    x.Province.Country != null &&
                    x.Province.Country.IsActive &&
                    !x.Province.Country.IsDelete);
        }

        private async Task<bool> DistrictExistsAsync(Guid id)
        {
            return id != Guid.Empty && await _dbContext.MstDistricts
                .Include(x => x.City)
                    .ThenInclude(x => x!.Province)
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == id &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.City != null &&
                    x.City.IsActive &&
                    !x.City.IsDelete &&
                    x.City.Province != null &&
                    x.City.Province.IsActive &&
                    !x.City.Province.IsDelete);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCountryRequestAsync(
            Guid? excludeId,
            string countryName,
            string? phoneCode)
        {
            if (string.IsNullOrWhiteSpace(countryName))
                return (false, "CountryName wajib diisi.");

            var normalizedName = countryName.Trim().ToLower();

            var duplicateName = await _dbContext.MstCountries
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.CountryName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateName)
                return (false, "CountryName sudah digunakan.");

            if (!string.IsNullOrWhiteSpace(phoneCode))
            {
                var normalizedPhoneCode = phoneCode.Trim().ToLower();

                var duplicatePhoneCode = await _dbContext.MstCountries
                    .AsNoTracking()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.PhoneCode != null &&
                        x.PhoneCode.ToLower() == normalizedPhoneCode &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicatePhoneCode)
                    return (false, "PhoneCode sudah digunakan.");
            }

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateProvinceRequestAsync(
            Guid? excludeId,
            Guid countryId,
            string provinceName)
        {
            if (!await CountryExistsAsync(countryId))
                return (false, "Country tidak valid atau tidak aktif.");

            if (string.IsNullOrWhiteSpace(provinceName))
                return (false, "ProvinceName wajib diisi.");

            var normalizedName = provinceName.Trim().ToLower();

            var duplicate = await _dbContext.MstProvinces
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.CountryId == countryId &&
                    x.ProvinceName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicate)
                return (false, "ProvinceName sudah digunakan pada country ini.");

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCityRequestAsync(
            Guid? excludeId,
            Guid provinceId,
            string cityName,
            string? cityType)
        {
            if (!await ProvinceExistsAsync(provinceId))
                return (false, "Province tidak valid atau tidak aktif.");

            if (string.IsNullOrWhiteSpace(cityName))
                return (false, "CityName wajib diisi.");

            var normalizedName = cityName.Trim().ToLower();
            var normalizedCityType = NormalizeNullableText(cityType)?.ToLower() ?? string.Empty;

            var duplicate = await _dbContext.MstCities
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.ProvinceId == provinceId &&
                    x.CityName.ToLower() == normalizedName &&
                    ((x.CityType ?? string.Empty).ToLower() == normalizedCityType) &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicate)
                return (false, "CityName dan CityType sudah digunakan pada province ini.");

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateDistrictRequestAsync(
            Guid? excludeId,
            Guid cityId,
            string districtName)
        {
            if (!await CityExistsAsync(cityId))
                return (false, "City tidak valid atau tidak aktif.");

            if (string.IsNullOrWhiteSpace(districtName))
                return (false, "DistrictName wajib diisi.");

            var normalizedName = districtName.Trim().ToLower();

            var duplicate = await _dbContext.MstDistricts
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.CityId == cityId &&
                    x.DistrictName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicate)
                return (false, "DistrictName sudah digunakan pada city ini.");

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidatePostalCodeRequestAsync(
            Guid? excludeId,
            Guid districtId,
            string postalCode,
            string? villageName)
        {
            if (!await DistrictExistsAsync(districtId))
                return (false, "District tidak valid atau tidak aktif.");

            if (string.IsNullOrWhiteSpace(postalCode))
                return (false, "PostalCode wajib diisi.");

            var normalizedPostalCode = postalCode.Trim().ToLower();
            var normalizedVillageName = NormalizeNullableText(villageName)?.ToLower() ?? string.Empty;

            var duplicate = await _dbContext.MstPostalCodes
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.DistrictId == districtId &&
                    x.PostalCode.ToLower() == normalizedPostalCode &&
                    ((x.VillageName ?? string.Empty).ToLower() == normalizedVillageName) &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicate)
                return (false, "PostalCode dan VillageName sudah digunakan pada district ini.");

            return (true, null);
        }

        private static IOrderedQueryable<MstCountry> ApplyCountrySorting(
            IQueryable<MstCountry> query,
            string? sortBy,
            string? sortDirection)
        {
            var field = NormalizeSortBy(sortBy);
            var desc = IsDescending(sortDirection);

            return field switch
            {
                "countrycode" => desc ? query.OrderByDescending(x => x.CountryCode) : query.OrderBy(x => x.CountryCode),
                "countryname" => desc ? query.OrderByDescending(x => x.CountryName) : query.OrderBy(x => x.CountryName),
                "phonecode" => desc ? query.OrderByDescending(x => x.PhoneCode) : query.OrderBy(x => x.PhoneCode),
                "isdefault" => desc ? query.OrderByDescending(x => x.IsDefault).ThenBy(x => x.CountryName) : query.OrderBy(x => x.IsDefault).ThenBy(x => x.CountryName),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.CountryName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.CountryName),
                _ => desc ? query.OrderByDescending(x => x.CreateDateTime).ThenBy(x => x.CountryName) : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.CountryName)
            };
        }

        private static IOrderedQueryable<MstProvince> ApplyProvinceSorting(
            IQueryable<MstProvince> query,
            string? sortBy,
            string? sortDirection)
        {
            var field = NormalizeSortBy(sortBy);
            var desc = IsDescending(sortDirection);

            return field switch
            {
                "provincecode" => desc ? query.OrderByDescending(x => x.ProvinceCode) : query.OrderBy(x => x.ProvinceCode),
                "provincename" => desc ? query.OrderByDescending(x => x.ProvinceName) : query.OrderBy(x => x.ProvinceName),
                "countryname" => desc ? query.OrderByDescending(x => x.Country != null ? x.Country.CountryName : string.Empty).ThenBy(x => x.ProvinceName) : query.OrderBy(x => x.Country != null ? x.Country.CountryName : string.Empty).ThenBy(x => x.ProvinceName),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.ProvinceName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.ProvinceName),
                _ => desc ? query.OrderByDescending(x => x.CreateDateTime).ThenBy(x => x.ProvinceName) : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.ProvinceName)
            };
        }

        private static IOrderedQueryable<MstCity> ApplyCitySorting(
            IQueryable<MstCity> query,
            string? sortBy,
            string? sortDirection)
        {
            var field = NormalizeSortBy(sortBy);
            var desc = IsDescending(sortDirection);

            return field switch
            {
                "citycode" => desc ? query.OrderByDescending(x => x.CityCode) : query.OrderBy(x => x.CityCode),
                "cityname" => desc ? query.OrderByDescending(x => x.CityName) : query.OrderBy(x => x.CityName),
                "citytype" => desc ? query.OrderByDescending(x => x.CityType).ThenBy(x => x.CityName) : query.OrderBy(x => x.CityType).ThenBy(x => x.CityName),
                "provincename" => desc ? query.OrderByDescending(x => x.Province != null ? x.Province.ProvinceName : string.Empty).ThenBy(x => x.CityName) : query.OrderBy(x => x.Province != null ? x.Province.ProvinceName : string.Empty).ThenBy(x => x.CityName),
                "countryname" => desc ? query.OrderByDescending(x => x.Province != null && x.Province.Country != null ? x.Province.Country.CountryName : string.Empty).ThenBy(x => x.CityName) : query.OrderBy(x => x.Province != null && x.Province.Country != null ? x.Province.Country.CountryName : string.Empty).ThenBy(x => x.CityName),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.CityName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.CityName),
                _ => desc ? query.OrderByDescending(x => x.CreateDateTime).ThenBy(x => x.CityName) : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.CityName)
            };
        }

        private static IOrderedQueryable<MstDistrict> ApplyDistrictSorting(
            IQueryable<MstDistrict> query,
            string? sortBy,
            string? sortDirection)
        {
            var field = NormalizeSortBy(sortBy);
            var desc = IsDescending(sortDirection);

            return field switch
            {
                "districtcode" => desc ? query.OrderByDescending(x => x.DistrictCode) : query.OrderBy(x => x.DistrictCode),
                "districtname" => desc ? query.OrderByDescending(x => x.DistrictName) : query.OrderBy(x => x.DistrictName),
                "cityname" => desc ? query.OrderByDescending(x => x.City != null ? x.City.CityName : string.Empty).ThenBy(x => x.DistrictName) : query.OrderBy(x => x.City != null ? x.City.CityName : string.Empty).ThenBy(x => x.DistrictName),
                "provincename" => desc ? query.OrderByDescending(x => x.City != null && x.City.Province != null ? x.City.Province.ProvinceName : string.Empty).ThenBy(x => x.DistrictName) : query.OrderBy(x => x.City != null && x.City.Province != null ? x.City.Province.ProvinceName : string.Empty).ThenBy(x => x.DistrictName),
                "countryname" => desc ? query.OrderByDescending(x => x.City != null && x.City.Province != null && x.City.Province.Country != null ? x.City.Province.Country.CountryName : string.Empty).ThenBy(x => x.DistrictName) : query.OrderBy(x => x.City != null && x.City.Province != null && x.City.Province.Country != null ? x.City.Province.Country.CountryName : string.Empty).ThenBy(x => x.DistrictName),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.DistrictName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.DistrictName),
                _ => desc ? query.OrderByDescending(x => x.CreateDateTime).ThenBy(x => x.DistrictName) : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.DistrictName)
            };
        }

        private static IOrderedQueryable<MstPostalCode> ApplyPostalCodeSorting(
            IQueryable<MstPostalCode> query,
            string? sortBy,
            string? sortDirection)
        {
            var field = NormalizeSortBy(sortBy);
            var desc = IsDescending(sortDirection);

            return field switch
            {
                "postalcode" => desc ? query.OrderByDescending(x => x.PostalCode) : query.OrderBy(x => x.PostalCode),
                "villagename" => desc ? query.OrderByDescending(x => x.VillageName).ThenBy(x => x.PostalCode) : query.OrderBy(x => x.VillageName).ThenBy(x => x.PostalCode),
                "districtname" => desc ? query.OrderByDescending(x => x.District != null ? x.District.DistrictName : string.Empty).ThenBy(x => x.PostalCode) : query.OrderBy(x => x.District != null ? x.District.DistrictName : string.Empty).ThenBy(x => x.PostalCode),
                "cityname" => desc ? query.OrderByDescending(x => x.District != null && x.District.City != null ? x.District.City.CityName : string.Empty).ThenBy(x => x.PostalCode) : query.OrderBy(x => x.District != null && x.District.City != null ? x.District.City.CityName : string.Empty).ThenBy(x => x.PostalCode),
                "provincename" => desc ? query.OrderByDescending(x => x.District != null && x.District.City != null && x.District.City.Province != null ? x.District.City.Province.ProvinceName : string.Empty).ThenBy(x => x.PostalCode) : query.OrderBy(x => x.District != null && x.District.City != null && x.District.City.Province != null ? x.District.City.Province.ProvinceName : string.Empty).ThenBy(x => x.PostalCode),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.PostalCode) : query.OrderBy(x => x.IsActive).ThenBy(x => x.PostalCode),
                _ => desc ? query.OrderByDescending(x => x.CreateDateTime).ThenBy(x => x.PostalCode) : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.PostalCode)
            };
        }

        private static CountryResponse MapCountryResponse(
            MstCountry entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new CountryResponse
            {
                Id = entity.Id,
                CountryCode = entity.CountryCode,
                CountryName = entity.CountryName,
                PhoneCode = entity.PhoneCode,
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static CountryDetailResponse MapCountryDetailResponse(
            MstCountry entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new CountryDetailResponse
            {
                Id = entity.Id,
                CountryCode = entity.CountryCode,
                CountryName = entity.CountryName,
                PhoneCode = entity.PhoneCode,
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static ProvinceResponse MapProvinceResponse(
            MstProvince entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new ProvinceResponse
            {
                Id = entity.Id,
                CountryId = entity.CountryId,
                CountryCode = entity.Country?.CountryCode ?? string.Empty,
                CountryName = entity.Country?.CountryName ?? string.Empty,
                ProvinceCode = entity.ProvinceCode,
                ProvinceName = entity.ProvinceName,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static ProvinceDetailResponse MapProvinceDetailResponse(
            MstProvince entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new ProvinceDetailResponse
            {
                Id = entity.Id,
                CountryId = entity.CountryId,
                CountryCode = entity.Country?.CountryCode ?? string.Empty,
                CountryName = entity.Country?.CountryName ?? string.Empty,
                ProvinceCode = entity.ProvinceCode,
                ProvinceName = entity.ProvinceName,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static CityResponse MapCityResponse(
            MstCity entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new CityResponse
            {
                Id = entity.Id,
                ProvinceId = entity.ProvinceId,
                ProvinceCode = entity.Province?.ProvinceCode ?? string.Empty,
                ProvinceName = entity.Province?.ProvinceName ?? string.Empty,
                CountryId = entity.Province?.CountryId ?? Guid.Empty,
                CountryCode = entity.Province?.Country?.CountryCode ?? string.Empty,
                CountryName = entity.Province?.Country?.CountryName ?? string.Empty,
                CityCode = entity.CityCode,
                CityName = entity.CityName,
                CityType = entity.CityType,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static CityDetailResponse MapCityDetailResponse(
            MstCity entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new CityDetailResponse
            {
                Id = entity.Id,
                ProvinceId = entity.ProvinceId,
                ProvinceCode = entity.Province?.ProvinceCode ?? string.Empty,
                ProvinceName = entity.Province?.ProvinceName ?? string.Empty,
                CountryId = entity.Province?.CountryId ?? Guid.Empty,
                CountryCode = entity.Province?.Country?.CountryCode ?? string.Empty,
                CountryName = entity.Province?.Country?.CountryName ?? string.Empty,
                CityCode = entity.CityCode,
                CityName = entity.CityName,
                CityType = entity.CityType,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static DistrictResponse MapDistrictResponse(
            MstDistrict entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new DistrictResponse
            {
                Id = entity.Id,
                CityId = entity.CityId,
                CityCode = entity.City?.CityCode ?? string.Empty,
                CityName = entity.City?.CityName ?? string.Empty,
                ProvinceId = entity.City?.ProvinceId ?? Guid.Empty,
                ProvinceCode = entity.City?.Province?.ProvinceCode ?? string.Empty,
                ProvinceName = entity.City?.Province?.ProvinceName ?? string.Empty,
                CountryId = entity.City?.Province?.CountryId ?? Guid.Empty,
                CountryCode = entity.City?.Province?.Country?.CountryCode ?? string.Empty,
                CountryName = entity.City?.Province?.Country?.CountryName ?? string.Empty,
                DistrictCode = entity.DistrictCode,
                DistrictName = entity.DistrictName,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static DistrictDetailResponse MapDistrictDetailResponse(
            MstDistrict entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new DistrictDetailResponse
            {
                Id = entity.Id,
                CityId = entity.CityId,
                CityCode = entity.City?.CityCode ?? string.Empty,
                CityName = entity.City?.CityName ?? string.Empty,
                ProvinceId = entity.City?.ProvinceId ?? Guid.Empty,
                ProvinceCode = entity.City?.Province?.ProvinceCode ?? string.Empty,
                ProvinceName = entity.City?.Province?.ProvinceName ?? string.Empty,
                CountryId = entity.City?.Province?.CountryId ?? Guid.Empty,
                CountryCode = entity.City?.Province?.Country?.CountryCode ?? string.Empty,
                CountryName = entity.City?.Province?.Country?.CountryName ?? string.Empty,
                DistrictCode = entity.DistrictCode,
                DistrictName = entity.DistrictName,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static PostalCodeResponse MapPostalCodeResponse(
            MstPostalCode entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new PostalCodeResponse
            {
                Id = entity.Id,
                DistrictId = entity.DistrictId,
                DistrictCode = entity.District?.DistrictCode ?? string.Empty,
                DistrictName = entity.District?.DistrictName ?? string.Empty,
                CityId = entity.District?.CityId ?? Guid.Empty,
                CityCode = entity.District?.City?.CityCode ?? string.Empty,
                CityName = entity.District?.City?.CityName ?? string.Empty,
                ProvinceId = entity.District?.City?.ProvinceId ?? Guid.Empty,
                ProvinceCode = entity.District?.City?.Province?.ProvinceCode ?? string.Empty,
                ProvinceName = entity.District?.City?.Province?.ProvinceName ?? string.Empty,
                CountryId = entity.District?.City?.Province?.CountryId ?? Guid.Empty,
                CountryCode = entity.District?.City?.Province?.Country?.CountryCode ?? string.Empty,
                CountryName = entity.District?.City?.Province?.Country?.CountryName ?? string.Empty,
                PostalCode = entity.PostalCode,
                VillageName = entity.VillageName,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static PostalCodeDetailResponse MapPostalCodeDetailResponse(
            MstPostalCode entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new PostalCodeDetailResponse
            {
                Id = entity.Id,
                DistrictId = entity.DistrictId,
                DistrictCode = entity.District?.DistrictCode ?? string.Empty,
                DistrictName = entity.District?.DistrictName ?? string.Empty,
                CityId = entity.District?.CityId ?? Guid.Empty,
                CityCode = entity.District?.City?.CityCode ?? string.Empty,
                CityName = entity.District?.City?.CityName ?? string.Empty,
                ProvinceId = entity.District?.City?.ProvinceId ?? Guid.Empty,
                ProvinceCode = entity.District?.City?.Province?.ProvinceCode ?? string.Empty,
                ProvinceName = entity.District?.City?.Province?.ProvinceName ?? string.Empty,
                CountryId = entity.District?.City?.Province?.CountryId ?? Guid.Empty,
                CountryCode = entity.District?.City?.Province?.Country?.CountryCode ?? string.Empty,
                CountryName = entity.District?.City?.Province?.Country?.CountryName ?? string.Empty,
                PostalCode = entity.PostalCode,
                VillageName = entity.VillageName,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private async Task<string> GenerateCountryCodeAsync()
        {
            var existingCodes = await _dbContext.MstCountries
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.CountryCode.StartsWith(CountryCodePrefix))
                .Select(x => x.CountryCode)
                .ToListAsync();

            return GenerateNextCode(existingCodes, CountryCodePrefix);
        }

        private async Task<string> GenerateProvinceCodeAsync()
        {
            var existingCodes = await _dbContext.MstProvinces
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.ProvinceCode.StartsWith(ProvinceCodePrefix))
                .Select(x => x.ProvinceCode)
                .ToListAsync();

            return GenerateNextCode(existingCodes, ProvinceCodePrefix);
        }

        private async Task<string> GenerateCityCodeAsync()
        {
            var existingCodes = await _dbContext.MstCities
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.CityCode.StartsWith(CityCodePrefix))
                .Select(x => x.CityCode)
                .ToListAsync();

            return GenerateNextCode(existingCodes, CityCodePrefix);
        }

        private async Task<string> GenerateDistrictCodeAsync()
        {
            var existingCodes = await _dbContext.MstDistricts
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.DistrictCode.StartsWith(DistrictCodePrefix))
                .Select(x => x.DistrictCode)
                .ToListAsync();

            return GenerateNextCode(existingCodes, DistrictCodePrefix);
        }

        private static string GenerateNextCode(IEnumerable<string> existingCodes, string prefix)
        {
            var usedNumbers = existingCodes
                .Select(x => x.Replace(prefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return prefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
        }

        private static RegionOptionPagedResponse BuildOptionPagedResponse(
            int pageNumber,
            int pageSize,
            int totalData,
            List<RegionOptionResponse> items)
        {
            return new RegionOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 25;

            if (pageSize > 100)
                pageSize = 100;

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
                        start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);

                    if (endDate.HasValue)
                        endExclusive = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);

                    break;

                case "today":
                    start = today;
                    endExclusive = today.AddDays(1);
                    break;

                case "last7days":
                case "last7day":
                    start = today.AddDays(-6);
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
                return DateRangeResolveResult.Invalid("startDate tidak boleh lebih besar atau sama dengan endDate.");

            return DateRangeResolveResult.Valid(start, endExclusive);
        }

        private static List<RegionCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<RegionCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
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

        private static string NormalizeSortBy(string? sortBy)
        {
            return string.IsNullOrWhiteSpace(sortBy)
                ? "createdatetime"
                : sortBy.Trim().Replace("_", string.Empty).ToLowerInvariant();
        }

        private static bool IsDescending(string? sortDirection)
        {
            return string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private void ApplyDeleteAudit(IdentityModel entity)
        {
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
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
