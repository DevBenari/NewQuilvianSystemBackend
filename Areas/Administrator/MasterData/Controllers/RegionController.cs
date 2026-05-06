using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
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
using QuilvianSystemBackend.Models;

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

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public RegionController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        // =========================================================
        // METADATA & SUMMARY
        // =========================================================

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<RegionFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Region",
            Description = "Melihat data region",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Region", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new RegionFilterMetadataResponse
            {
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                CountrySortOptions = BuildCommonSortOptions("countryCode", "countryName"),
                ProvinceSortOptions = BuildCommonSortOptions("provinceCode", "provinceName"),
                CitySortOptions = BuildCommonSortOptions("cityCode", "cityName"),
                DistrictSortOptions = BuildCommonSortOptions("districtCode", "districtName"),
                PostalCodeSortOptions = new List<RegionSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "postalCode", Label = "Kode pos" },
                    new() { Value = "villageName", Label = "Kelurahan/Desa" },
                    new() { Value = "isActive", Label = "Status aktif" }
                }
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
        [AccessAction(
            "Read",
            "Read Region",
            Description = "Melihat data region",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
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

                TotalProvince = await provinces.CountAsync(),
                ActiveProvince = await provinces.CountAsync(x => x.IsActive),

                TotalCity = await cities.CountAsync(),
                ActiveCity = await cities.CountAsync(x => x.IsActive),

                TotalDistrict = await districts.CountAsync(),
                ActiveDistrict = await districts.CountAsync(x => x.IsActive),

                TotalPostalCode = await postalCodes.CountAsync(),
                ActivePostalCode = await postalCodes.CountAsync(x => x.IsActive)
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
        [ProducesResponseType(typeof(ApiResponse<ResponseCountryPagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Region",
            Description = "Melihat data region",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Region", "Read")]
        public async Task<IActionResult> GetCountries(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] string? sortBy = "countryName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.MstCountries
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.CountryCode.ToLower().Contains(keyword) ||
                    x.CountryName.ToLower().Contains(keyword) ||
                    (x.PhoneCode != null && x.PhoneCode.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            var totalData = await query.CountAsync();

            var items = await ApplyCountrySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CountryResponse
                {
                    Id = x.Id,
                    CountryCode = x.CountryCode,
                    CountryName = x.CountryName,
                    PhoneCode = x.PhoneCode,
                    IsDefault = x.IsDefault,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new ResponseCountryPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseCountryPagedResult>.Ok(
                result,
                "Data country berhasil diambil."
            ));
        }

        [HttpGet("countries/options")]
        [ProducesResponseType(typeof(ApiResponse<List<RegionOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Region",
            Description = "Melihat data region",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Region", "Read")]
        public async Task<IActionResult> GetCountryOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.MstCountries
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
            {
                query = query.Where(x => x.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.CountryCode.ToLower().Contains(keyword) ||
                    x.CountryName.ToLower().Contains(keyword));
            }

            var data = await query
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.CountryName)
                .Select(x => new RegionOptionResponse
                {
                    Id = x.Id,
                    Code = x.CountryCode,
                    Name = x.CountryName,
                    ParentId = null,
                    AdditionalInfo = x.PhoneCode
                })
                .ToListAsync();

            return Ok(ApiResponse<List<RegionOptionResponse>>.Ok(
                data,
                "Pilihan country berhasil diambil."
            ));
        }

        [HttpGet("countries/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CountryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Region",
            Description = "Melihat data region",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Region", "Read")]
        public async Task<IActionResult> GetCountryById(Guid id)
        {
            var data = await _dbContext.MstCountries
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new CountryResponse
                {
                    Id = x.Id,
                    CountryCode = x.CountryCode,
                    CountryName = x.CountryName,
                    PhoneCode = x.PhoneCode,
                    IsDefault = x.IsDefault,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Country tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<CountryResponse>.Ok(
                data,
                "Detail country berhasil diambil."
            ));
        }

        [HttpPost("countries")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Region",
            Description = "Membuat data region",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("Region", "Create")]
        public async Task<IActionResult> CreateCountry([FromBody] CreateCountryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CountryCode))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Country code wajib diisi."
                ));
            }

            if (string.IsNullOrWhiteSpace(request.CountryName))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Country name wajib diisi."
                ));
            }

            var code = request.CountryCode.Trim().ToUpper();
            var name = request.CountryName.Trim();

            var exists = await _dbContext.MstCountries.AnyAsync(x =>
                x.CountryCode == code &&
                !x.IsDelete);

            if (exists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Country code sudah digunakan."
                ));
            }

            if (request.IsDefault)
            {
                await ResetDefaultCountryAsync();
            }

            var entity = new MstCountry
            {
                Id = Guid.NewGuid(),
                CountryCode = code,
                CountryName = name,
                PhoneCode = NormalizeNullableText(request.PhoneCode),
                IsDefault = request.IsDefault,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstCountries.Add(entity);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                new { entity.Id, entity.CountryCode, entity.CountryName },
                "Country berhasil dibuat."
            ));
        }

        [HttpPut("countries/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Region",
            Description = "Mengubah data region",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Region", "Update")]
        public async Task<IActionResult> UpdateCountry(
            Guid id,
            [FromBody] UpdateCountryRequest request)
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

            var code = request.CountryCode.Trim().ToUpper();
            var name = request.CountryName.Trim();

            var exists = await _dbContext.MstCountries.AnyAsync(x =>
                x.Id != id &&
                x.CountryCode == code &&
                !x.IsDelete);

            if (exists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Country code sudah digunakan."
                ));
            }

            if (request.IsDefault)
            {
                await ResetDefaultCountryAsync(excludeId: id);
            }

            entity.CountryCode = code;
            entity.CountryName = name;
            entity.PhoneCode = NormalizeNullableText(request.PhoneCode);
            entity.IsDefault = request.IsDefault;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Country berhasil diperbarui."
            ));
        }

        [HttpPatch("countries/{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Region",
            Description = "Mengubah data region",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Region", "Update")]
        public async Task<IActionResult> UpdateCountryStatus(
            Guid id,
            [FromBody] UpdateRegionStatusRequest request)
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
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status country berhasil diperbarui."
            ));
        }

        [HttpDelete("countries/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Region",
            Description = "Menghapus data region",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("Region", "Delete")]
        public async Task<IActionResult> DeleteCountry(Guid id)
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

            SoftDelete(entity);

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Country berhasil dihapus."
            ));
        }

        // =========================================================
        // PROVINCE
        // =========================================================

        [HttpGet("provinces")]
        [ProducesResponseType(typeof(ApiResponse<ResponseProvincePagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Region",
            Description = "Melihat data region",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Region", "Read")]
        public async Task<IActionResult> GetProvinces(
            [FromQuery] Guid? countryId,
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] string? sortBy = "provinceName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.MstProvinces
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (countryId.HasValue && countryId.Value != Guid.Empty)
            {
                query = query.Where(x => x.CountryId == countryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ProvinceCode.ToLower().Contains(keyword) ||
                    x.ProvinceName.ToLower().Contains(keyword) ||
                    (x.Country != null && x.Country.CountryName.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            var totalData = await query.CountAsync();

            var items = await ApplyProvinceSorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ProvinceResponse
                {
                    Id = x.Id,
                    CountryId = x.CountryId,
                    CountryCode = x.Country != null ? x.Country.CountryCode : string.Empty,
                    CountryName = x.Country != null ? x.Country.CountryName : string.Empty,
                    ProvinceCode = x.ProvinceCode,
                    ProvinceName = x.ProvinceName,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new ResponseProvincePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseProvincePagedResult>.Ok(
                result,
                "Data province berhasil diambil."
            ));
        }

        [HttpGet("provinces/options")]
        [ProducesResponseType(typeof(ApiResponse<List<RegionOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Region",
            Description = "Melihat data region",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Region", "Read")]
        public async Task<IActionResult> GetProvinceOptions(
            [FromQuery] Guid countryId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            if (countryId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Country wajib dipilih."
                ));
            }

            var query = _dbContext.MstProvinces
                .AsNoTracking()
                .Where(x => x.CountryId == countryId && !x.IsDelete);

            if (onlyActive)
            {
                query = query.Where(x => x.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ProvinceCode.ToLower().Contains(keyword) ||
                    x.ProvinceName.ToLower().Contains(keyword));
            }

            var data = await query
                .OrderBy(x => x.ProvinceName)
                .Select(x => new RegionOptionResponse
                {
                    Id = x.Id,
                    Code = x.ProvinceCode,
                    Name = x.ProvinceName,
                    ParentId = x.CountryId
                })
                .ToListAsync();

            return Ok(ApiResponse<List<RegionOptionResponse>>.Ok(
                data,
                "Pilihan province berhasil diambil."
            ));
        }

        [HttpPost("provinces")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Region",
            Description = "Membuat data region",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("Region", "Create")]
        public async Task<IActionResult> CreateProvince([FromBody] CreateProvinceRequest request)
        {
            if (!await CountryExistsAsync(request.CountryId))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Country tidak valid atau tidak aktif."
                ));
            }

            var code = request.ProvinceCode.Trim().ToUpper();
            var name = request.ProvinceName.Trim();

            var exists = await _dbContext.MstProvinces.AnyAsync(x =>
                x.CountryId == request.CountryId &&
                x.ProvinceCode == code &&
                !x.IsDelete);

            if (exists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Province code sudah digunakan pada country ini."
                ));
            }

            var entity = new MstProvince
            {
                Id = Guid.NewGuid(),
                CountryId = request.CountryId,
                ProvinceCode = code,
                ProvinceName = name,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstProvinces.Add(entity);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                new { entity.Id, entity.ProvinceCode, entity.ProvinceName },
                "Province berhasil dibuat."
            ));
        }

        [HttpPut("provinces/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Region",
            Description = "Mengubah data region",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Region", "Update")]
        public async Task<IActionResult> UpdateProvince(
            Guid id,
            [FromBody] UpdateProvinceRequest request)
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

            if (!await CountryExistsAsync(request.CountryId))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Country tidak valid atau tidak aktif."
                ));
            }

            var code = request.ProvinceCode.Trim().ToUpper();

            var exists = await _dbContext.MstProvinces.AnyAsync(x =>
                x.Id != id &&
                x.CountryId == request.CountryId &&
                x.ProvinceCode == code &&
                !x.IsDelete);

            if (exists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Province code sudah digunakan pada country ini."
                ));
            }

            entity.CountryId = request.CountryId;
            entity.ProvinceCode = code;
            entity.ProvinceName = request.ProvinceName.Trim();
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Province berhasil diperbarui."));
        }

        [HttpPatch("provinces/{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Region",
            Description = "Mengubah data region",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Region", "Update")]
        public async Task<IActionResult> UpdateProvinceStatus(
            Guid id,
            [FromBody] UpdateRegionStatusRequest request)
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

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Status province berhasil diperbarui."));
        }

        [HttpDelete("provinces/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Region",
            Description = "Menghapus data region",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("Region", "Delete")]
        public async Task<IActionResult> DeleteProvince(Guid id)
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

            SoftDelete(entity);

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Province berhasil dihapus."));
        }

        // =========================================================
        // CITY
        // =========================================================

        [HttpGet("cities/options")]
        [ProducesResponseType(typeof(ApiResponse<List<RegionOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Region",
            Description = "Melihat data region",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Region", "Read")]
        public async Task<IActionResult> GetCityOptions(
            [FromQuery] Guid provinceId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            if (provinceId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Province wajib dipilih."
                ));
            }

            var query = _dbContext.MstCities
                .AsNoTracking()
                .Where(x => x.ProvinceId == provinceId && !x.IsDelete);

            if (onlyActive)
            {
                query = query.Where(x => x.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.CityCode.ToLower().Contains(keyword) ||
                    x.CityName.ToLower().Contains(keyword) ||
                    (x.CityType != null && x.CityType.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderBy(x => x.CityName)
                .Select(x => new RegionOptionResponse
                {
                    Id = x.Id,
                    Code = x.CityCode,
                    Name = x.CityName,
                    ParentId = x.ProvinceId,
                    AdditionalInfo = x.CityType
                })
                .ToListAsync();

            return Ok(ApiResponse<List<RegionOptionResponse>>.Ok(
                data,
                "Pilihan city berhasil diambil."
            ));
        }

        [HttpPost("cities")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Region",
            Description = "Membuat data region",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("Region", "Create")]
        public async Task<IActionResult> CreateCity([FromBody] CreateCityRequest request)
        {
            if (!await ProvinceExistsAsync(request.ProvinceId))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Province tidak valid atau tidak aktif."
                ));
            }

            var code = request.CityCode.Trim().ToUpper();

            var exists = await _dbContext.MstCities.AnyAsync(x =>
                x.ProvinceId == request.ProvinceId &&
                x.CityCode == code &&
                !x.IsDelete);

            if (exists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "City code sudah digunakan pada province ini."
                ));
            }

            var entity = new MstCity
            {
                Id = Guid.NewGuid(),
                ProvinceId = request.ProvinceId,
                CityCode = code,
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

            return Ok(ApiResponse<object>.Ok(
                new { entity.Id, entity.CityCode, entity.CityName },
                "City berhasil dibuat."
            ));
        }

        [HttpPut("cities/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Region",
            Description = "Mengubah data region",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Region", "Update")]
        public async Task<IActionResult> UpdateCity(
            Guid id,
            [FromBody] UpdateCityRequest request)
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

            if (!await ProvinceExistsAsync(request.ProvinceId))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Province tidak valid atau tidak aktif."
                ));
            }

            var code = request.CityCode.Trim().ToUpper();

            var exists = await _dbContext.MstCities.AnyAsync(x =>
                x.Id != id &&
                x.ProvinceId == request.ProvinceId &&
                x.CityCode == code &&
                !x.IsDelete);

            if (exists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "City code sudah digunakan pada province ini."
                ));
            }

            entity.ProvinceId = request.ProvinceId;
            entity.CityCode = code;
            entity.CityName = request.CityName.Trim();
            entity.CityType = NormalizeNullableText(request.CityType);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "City berhasil diperbarui."));
        }

        [HttpPatch("cities/{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Region",
            Description = "Mengubah data region",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Region", "Update")]
        public async Task<IActionResult> UpdateCityStatus(
            Guid id,
            [FromBody] UpdateRegionStatusRequest request)
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

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Status city berhasil diperbarui."));
        }

        [HttpDelete("cities/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Region",
            Description = "Menghapus data region",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("Region", "Delete")]
        public async Task<IActionResult> DeleteCity(Guid id)
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

            SoftDelete(entity);

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "City berhasil dihapus."));
        }

        // =========================================================
        // DISTRICT
        // =========================================================

        [HttpGet("districts/options")]
        [ProducesResponseType(typeof(ApiResponse<List<RegionOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Region",
            Description = "Melihat data region",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Region", "Read")]
        public async Task<IActionResult> GetDistrictOptions(
            [FromQuery] Guid cityId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            if (cityId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "City wajib dipilih."
                ));
            }

            var query = _dbContext.MstDistricts
                .AsNoTracking()
                .Where(x => x.CityId == cityId && !x.IsDelete);

            if (onlyActive)
            {
                query = query.Where(x => x.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.DistrictCode.ToLower().Contains(keyword) ||
                    x.DistrictName.ToLower().Contains(keyword));
            }

            var data = await query
                .OrderBy(x => x.DistrictName)
                .Select(x => new RegionOptionResponse
                {
                    Id = x.Id,
                    Code = x.DistrictCode,
                    Name = x.DistrictName,
                    ParentId = x.CityId
                })
                .ToListAsync();

            return Ok(ApiResponse<List<RegionOptionResponse>>.Ok(
                data,
                "Pilihan district berhasil diambil."
            ));
        }

        [HttpPost("districts")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Region",
            Description = "Membuat data region",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("Region", "Create")]
        public async Task<IActionResult> CreateDistrict([FromBody] CreateDistrictRequest request)
        {
            if (!await CityExistsAsync(request.CityId))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "City tidak valid atau tidak aktif."
                ));
            }

            var code = request.DistrictCode.Trim().ToUpper();

            var exists = await _dbContext.MstDistricts.AnyAsync(x =>
                x.CityId == request.CityId &&
                x.DistrictCode == code &&
                !x.IsDelete);

            if (exists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "District code sudah digunakan pada city ini."
                ));
            }

            var entity = new MstDistrict
            {
                Id = Guid.NewGuid(),
                CityId = request.CityId,
                DistrictCode = code,
                DistrictName = request.DistrictName.Trim(),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstDistricts.Add(entity);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                new { entity.Id, entity.DistrictCode, entity.DistrictName },
                "District berhasil dibuat."
            ));
        }

        [HttpPut("districts/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Region",
            Description = "Mengubah data region",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Region", "Update")]
        public async Task<IActionResult> UpdateDistrict(
            Guid id,
            [FromBody] UpdateDistrictRequest request)
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

            if (!await CityExistsAsync(request.CityId))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "City tidak valid atau tidak aktif."
                ));
            }

            var code = request.DistrictCode.Trim().ToUpper();

            var exists = await _dbContext.MstDistricts.AnyAsync(x =>
                x.Id != id &&
                x.CityId == request.CityId &&
                x.DistrictCode == code &&
                !x.IsDelete);

            if (exists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "District code sudah digunakan pada city ini."
                ));
            }

            entity.CityId = request.CityId;
            entity.DistrictCode = code;
            entity.DistrictName = request.DistrictName.Trim();
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "District berhasil diperbarui."));
        }

        [HttpPatch("districts/{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Region",
            Description = "Mengubah data region",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Region", "Update")]
        public async Task<IActionResult> UpdateDistrictStatus(
            Guid id,
            [FromBody] UpdateRegionStatusRequest request)
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

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Status district berhasil diperbarui."));
        }

        [HttpDelete("districts/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Region",
            Description = "Menghapus data region",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("Region", "Delete")]
        public async Task<IActionResult> DeleteDistrict(Guid id)
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

            SoftDelete(entity);

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "District berhasil dihapus."));
        }

        // =========================================================
        // POSTAL CODE
        // =========================================================

        [HttpGet("postal-codes/options")]
        [ProducesResponseType(typeof(ApiResponse<List<RegionOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Region",
            Description = "Melihat data region",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Region", "Read")]
        public async Task<IActionResult> GetPostalCodeOptions(
            [FromQuery] Guid districtId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            if (districtId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "District wajib dipilih."
                ));
            }

            var query = _dbContext.MstPostalCodes
                .AsNoTracking()
                .Where(x => x.DistrictId == districtId && !x.IsDelete);

            if (onlyActive)
            {
                query = query.Where(x => x.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.PostalCode.ToLower().Contains(keyword) ||
                    (x.VillageName != null && x.VillageName.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderBy(x => x.VillageName)
                .ThenBy(x => x.PostalCode)
                .Select(x => new RegionOptionResponse
                {
                    Id = x.Id,
                    Code = x.PostalCode,
                    Name = x.VillageName ?? x.PostalCode,
                    ParentId = x.DistrictId,
                    AdditionalInfo = x.PostalCode
                })
                .ToListAsync();

            return Ok(ApiResponse<List<RegionOptionResponse>>.Ok(
                data,
                "Pilihan postal code berhasil diambil."
            ));
        }

        [HttpPost("postal-codes")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Region",
            Description = "Membuat data region",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("Region", "Create")]
        public async Task<IActionResult> CreatePostalCode([FromBody] CreatePostalCodeRequest request)
        {
            if (!await DistrictExistsAsync(request.DistrictId))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "District tidak valid atau tidak aktif."
                ));
            }

            var postalCode = request.PostalCode.Trim();

            var exists = await _dbContext.MstPostalCodes.AnyAsync(x =>
                x.DistrictId == request.DistrictId &&
                x.PostalCode == postalCode &&
                x.VillageName == NormalizeNullableText(request.VillageName) &&
                !x.IsDelete);

            if (exists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Postal code dengan village yang sama sudah ada pada district ini."
                ));
            }

            var entity = new MstPostalCode
            {
                Id = Guid.NewGuid(),
                DistrictId = request.DistrictId,
                PostalCode = postalCode,
                VillageName = NormalizeNullableText(request.VillageName),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstPostalCodes.Add(entity);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                new { entity.Id, entity.PostalCode, entity.VillageName },
                "Postal code berhasil dibuat."
            ));
        }

        [HttpPut("postal-codes/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Region",
            Description = "Mengubah data region",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Region", "Update")]
        public async Task<IActionResult> UpdatePostalCode(
            Guid id,
            [FromBody] UpdatePostalCodeRequest request)
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

            if (!await DistrictExistsAsync(request.DistrictId))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "District tidak valid atau tidak aktif."
                ));
            }

            var postalCode = request.PostalCode.Trim();
            var villageName = NormalizeNullableText(request.VillageName);

            var exists = await _dbContext.MstPostalCodes.AnyAsync(x =>
                x.Id != id &&
                x.DistrictId == request.DistrictId &&
                x.PostalCode == postalCode &&
                x.VillageName == villageName &&
                !x.IsDelete);

            if (exists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Postal code dengan village yang sama sudah ada pada district ini."
                ));
            }

            entity.DistrictId = request.DistrictId;
            entity.PostalCode = postalCode;
            entity.VillageName = villageName;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Postal code berhasil diperbarui."));
        }

        [HttpPatch("postal-codes/{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Region",
            Description = "Mengubah data region",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Region", "Update")]
        public async Task<IActionResult> UpdatePostalCodeStatus(
            Guid id,
            [FromBody] UpdateRegionStatusRequest request)
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

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Status postal code berhasil diperbarui."));
        }

        [HttpDelete("postal-codes/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Region",
            Description = "Menghapus data region",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("Region", "Delete")]
        public async Task<IActionResult> DeletePostalCode(Guid id)
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

            SoftDelete(entity);

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Postal code berhasil dihapus."));
        }

        // =========================================================
        // PRIVATE HELPERS
        // =========================================================

        private async Task ResetDefaultCountryAsync(Guid? excludeId = null)
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
                country.UpdateDateTime = DateTime.UtcNow;
                country.UpdateBy = GetCurrentUserId();
            }
        }

        private async Task<bool> CountryExistsAsync(Guid id)
        {
            return await _dbContext.MstCountries
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == id &&
                    x.IsActive &&
                    !x.IsDelete);
        }

        private async Task<bool> ProvinceExistsAsync(Guid id)
        {
            return await _dbContext.MstProvinces
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == id &&
                    x.IsActive &&
                    !x.IsDelete);
        }

        private async Task<bool> CityExistsAsync(Guid id)
        {
            return await _dbContext.MstCities
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == id &&
                    x.IsActive &&
                    !x.IsDelete);
        }

        private async Task<bool> DistrictExistsAsync(Guid id)
        {
            return await _dbContext.MstDistricts
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == id &&
                    x.IsActive &&
                    !x.IsDelete);
        }

        private static List<RegionSortOptionResponse> BuildCommonSortOptions(
            string codeField,
            string nameField)
        {
            return new List<RegionSortOptionResponse>
            {
                new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                new() { Value = codeField, Label = "Kode" },
                new() { Value = nameField, Label = "Nama" },
                new() { Value = "isActive", Label = "Status aktif" }
            };
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
                "countrycode" => desc
                    ? query.OrderByDescending(x => x.CountryCode)
                    : query.OrderBy(x => x.CountryCode),

                "countryname" => desc
                    ? query.OrderByDescending(x => x.CountryName)
                    : query.OrderBy(x => x.CountryName),

                "isactive" => desc
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.CountryName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.CountryName),

                _ => desc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime)
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
                "provincecode" => desc
                    ? query.OrderByDescending(x => x.ProvinceCode)
                    : query.OrderBy(x => x.ProvinceCode),

                "provincename" => desc
                    ? query.OrderByDescending(x => x.ProvinceName)
                    : query.OrderBy(x => x.ProvinceName),

                "isactive" => desc
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.ProvinceName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.ProvinceName),

                _ => desc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime)
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 25 : pageSize;
            pageSize = pageSize > 100 ? 100 : pageSize;

            return (pageNumber, pageSize);
        }

        private static string NormalizeSortBy(string? sortBy)
        {
            return string.IsNullOrWhiteSpace(sortBy)
                ? "createdatetime"
                : sortBy.Trim().Replace("_", string.Empty).ToLower();
        }

        private static bool IsDescending(string? sortDirection)
        {
            return !string.Equals(
                sortDirection?.Trim(),
                "asc",
                StringComparison.OrdinalIgnoreCase
            );
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(userIdText, out var userId))
            {
                return userId;
            }

            return Guid.Empty;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private void SoftDelete(MstCountry entity)
        {
            entity.IsActive = false;
            ApplyDeleteAudit(entity);
        }

        private void SoftDelete(MstProvince entity)
        {
            entity.IsActive = false;
            ApplyDeleteAudit(entity);
        }

        private void SoftDelete(MstCity entity)
        {
            entity.IsActive = false;
            ApplyDeleteAudit(entity);
        }

        private void SoftDelete(MstDistrict entity)
        {
            entity.IsActive = false;
            ApplyDeleteAudit(entity);
        }

        private void SoftDelete(MstPostalCode entity)
        {
            entity.IsActive = false;
            ApplyDeleteAudit(entity);
        }

        private void ApplyDeleteAudit(IdentityModel entity)
        {
            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();
        }
    }
}