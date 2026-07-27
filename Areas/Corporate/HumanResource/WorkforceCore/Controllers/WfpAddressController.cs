using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Globalization;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/addresses")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Workforce Address",
        AreaName = "Corporate",
        ControllerName = "WorkforceAddress",
        Description = "Corporate human resource workforce address",
        SortOrder = 1
    )]
    [Tags("Corporate / Human Resource / Workforce Core / Address")]
    public class WfpAddressController : ControllerBase
    {
        private static readonly HashSet<string> AllowedAddressTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Identity", "Current", "Domicile", "Mailing", "Emergency"
        };

        private const string LogCategory = "Corporate.HumanResource.WorkforceCore";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpAddressController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WfpAddressFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Address", Description = "Melihat metadata filter alamat workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceAddress", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new WfpAddressFilterMetadataResponse
            {
                DefaultFilter = new WfpAddressDefaultFilterResponse(),
                AddressTypeOptions = AllowedAddressTypes
                    .OrderBy(x => x)
                    .Select(x => new WfpAddressStringOptionResponse
                    {
                        Value = x,
                        Label = BuildAddressTypeLabel(x)
                    })
                    .ToList(),
                SortOptions = new List<WfpAddressSortOptionResponse>
                {
                    new() { Value = "isPrimary", Label = "Alamat utama" },
                    new() { Value = "addressType", Label = "Jenis alamat" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal mulai berlaku" },
                    new() { Value = "isVerified", Label = "Status verifikasi" },
                    new() { Value = "isActive", Label = "Status aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpAddressFilterMetadataResponse>.Ok(
                result,
                "Metadata filter alamat workforce berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WfpAddressSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Address", Description = "Melihat ringkasan alamat workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceAddress", "Read")]
        public async Task<IActionResult> GetSummary(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var query = _dbContext.Set<WfpAddress>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var result = new WfpAddressSummaryResponse
            {
                TotalAddress = await query.CountAsync(cancellationToken),
                ActiveAddress = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveAddress = await query.CountAsync(x => !x.IsActive, cancellationToken),
                PrimaryAddress = await query.CountAsync(x => x.IsPrimary, cancellationToken),
                VerifiedAddress = await query.CountAsync(x => x.IsVerified, cancellationToken),
                UnverifiedAddress = await query.CountAsync(x => !x.IsVerified, cancellationToken)
            };

            return Ok(ApiResponse<WfpAddressSummaryResponse>.Ok(
                result,
                "Ringkasan alamat workforce berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WfpAddressResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Address", Description = "Melihat data alamat workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceAddress", "Read")]
        public async Task<IActionResult> GetAddresses(
            Guid workforceProfileId,
            [FromQuery] string? addressType,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "isPrimary",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyFilter(query, addressType, isPrimary, isVerified, isActive, search);

            var totalData = await query.CountAsync(cancellationToken);

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WfpAddressResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    WorkforceDisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    AddressType = x.AddressType,
                    AddressLine = x.AddressLine,
                    CountryId = x.CountryId,
                    CountryCode = x.Country != null ? x.Country.CountryCode : null,
                    CountryName = x.Country != null ? x.Country.CountryName : null,
                    ProvinceId = x.ProvinceId,
                    ProvinceName = x.Province != null ? x.Province.ProvinceName : null,
                    CityId = x.CityId,
                    CityName = x.City != null ? x.City.CityName : null,
                    DistrictId = x.DistrictId,
                    DistrictName = x.District != null ? x.District.DistrictName : null,
                    PostalCodeId = x.PostalCodeId,
                    PostalCode = x.PostalCode != null ? x.PostalCode.PostalCode : null,
                    VillageName = x.VillageName ?? (x.PostalCode != null ? x.PostalCode.VillageName : null),
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    IsPrimary = x.IsPrimary,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    IsVerified = x.IsVerified,
                    IsActive = x.IsActive,
                    Description = x.Description,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.CreateBy)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<WfpAddressResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PagedResult<WfpAddressResponse>>.Ok(
                result,
                "Data alamat workforce berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpAddressDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Address", Description = "Melihat detail alamat workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceAddress", "Read")]
        public async Task<IActionResult> GetAddressById(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var data = await BuildBaseQuery(workforceProfileId)
                .Where(x => x.Id == id)
                .Select(x => new WfpAddressDetailResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    WorkforceDisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    AddressType = x.AddressType,
                    AddressLine = x.AddressLine,
                    CountryId = x.CountryId,
                    CountryCode = x.Country != null ? x.Country.CountryCode : null,
                    CountryName = x.Country != null ? x.Country.CountryName : null,
                    ProvinceId = x.ProvinceId,
                    ProvinceName = x.Province != null ? x.Province.ProvinceName : null,
                    CityId = x.CityId,
                    CityName = x.City != null ? x.City.CityName : null,
                    DistrictId = x.DistrictId,
                    DistrictName = x.District != null ? x.District.DistrictName : null,
                    PostalCodeId = x.PostalCodeId,
                    PostalCode = x.PostalCode != null ? x.PostalCode.PostalCode : null,
                    VillageName = x.VillageName ?? (x.PostalCode != null ? x.PostalCode.VillageName : null),
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    IsPrimary = x.IsPrimary,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    IsVerified = x.IsVerified,
                    IsActive = x.IsActive,
                    Description = x.Description,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.CreateBy)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault(),
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.UpdateBy)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Alamat workforce tidak ditemukan."));
            }

            return Ok(ApiResponse<WfpAddressDetailResponse>.Ok(
                data,
                "Detail alamat workforce berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WfpAddressDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Create", "Create Workforce Address", Description = "Membuat alamat workforce", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceAddress", "Create")]
        public async Task<IActionResult> CreateAddress(
            Guid workforceProfileId,
            [FromBody] CreateWfpAddressRequest request,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data alamat workforce tidak valid."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (request.IsPrimary)
                {
                    await UnsetOtherPrimaryAsync(workforceProfileId, null, now, actorUserId, cancellationToken);
                }

                var entity = new WfpAddress
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    AddressType = NormalizeAddressType(request.AddressType),
                    AddressLine = request.AddressLine.Trim(),
                    CountryId = NormalizeNullableGuid(request.CountryId),
                    ProvinceId = NormalizeNullableGuid(request.ProvinceId),
                    CityId = NormalizeNullableGuid(request.CityId),
                    DistrictId = NormalizeNullableGuid(request.DistrictId),
                    PostalCodeId = NormalizeNullableGuid(request.PostalCodeId),
                    VillageName = NormalizeNullableText(request.VillageName),
                    Latitude = NormalizeCoordinate(request.Latitude),
                    Longitude = NormalizeCoordinate(request.Longitude),
                    IsPrimary = request.IsPrimary,
                    EffectiveStartDate = request.EffectiveStartDate?.Date,
                    EffectiveEndDate = request.EffectiveEndDate?.Date,
                    IsVerified = request.IsVerified,
                    IsActive = request.IsActive,
                    Description = NormalizeNullableText(request.Description),
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<WfpAddress>().Add(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceAddress.CreateAddress",
                    "Membuat alamat workforce.",
                    new { entity.Id, entity.WorkforceProfileId, entity.AddressType, entity.IsPrimary });

                return await GetAddressById(workforceProfileId, entity.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceAddress.CreateAddress",
                    "Gagal membuat alamat workforce.",
                    ex);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat alamat workforce."));
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpAddressDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Address", Description = "Mengubah alamat workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceAddress", "Update")]
        public async Task<IActionResult> UpdateAddress(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpAddressRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpAddress>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Alamat workforce tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data alamat workforce tidak valid."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (request.IsPrimary)
                {
                    await UnsetOtherPrimaryAsync(workforceProfileId, id, now, actorUserId, cancellationToken);
                }

                entity.AddressType = NormalizeAddressType(request.AddressType);
                entity.AddressLine = request.AddressLine.Trim();
                entity.CountryId = NormalizeNullableGuid(request.CountryId);
                entity.ProvinceId = NormalizeNullableGuid(request.ProvinceId);
                entity.CityId = NormalizeNullableGuid(request.CityId);
                entity.DistrictId = NormalizeNullableGuid(request.DistrictId);
                entity.PostalCodeId = NormalizeNullableGuid(request.PostalCodeId);
                entity.VillageName = NormalizeNullableText(request.VillageName);
                entity.Latitude = NormalizeCoordinate(request.Latitude);
                entity.Longitude = NormalizeCoordinate(request.Longitude);
                entity.IsPrimary = request.IsPrimary && request.IsActive;
                entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
                entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
                entity.IsVerified = request.IsVerified;
                entity.IsActive = request.IsActive;
                entity.Description = NormalizeNullableText(request.Description);
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceAddress.UpdateAddress",
                    "Mengubah alamat workforce.",
                    new { entity.Id, entity.WorkforceProfileId, entity.AddressType, entity.IsPrimary, entity.IsActive });

                return await GetAddressById(workforceProfileId, entity.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceAddress.UpdateAddress",
                    "Gagal mengubah alamat workforce.",
                    ex);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat mengubah alamat workforce."));
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Address", Description = "Mengubah status alamat workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceAddress", "Update")]
        public async Task<IActionResult> UpdateAddressStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpAddressStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpAddress>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Alamat workforce tidak ditemukan."));
            }

            if (request.EffectiveEndDate.HasValue &&
                entity.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.Value.Date < entity.EffectiveStartDate.Value.Date)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate."));
            }

            entity.IsActive = request.IsActive;
            entity.IsPrimary = request.IsActive && entity.IsPrimary;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeNullableText(request.Description) ?? entity.Description;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status alamat workforce berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/primary")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Address", Description = "Menetapkan alamat utama workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceAddress", "Update")]
        public async Task<IActionResult> SetPrimaryAddress(
            Guid workforceProfileId,
            Guid id,
            [FromBody] SetWfpAddressPrimaryRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpAddress>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Alamat workforce tidak ditemukan."));
            }

            if (request.IsPrimary && !entity.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alamat tidak aktif tidak dapat dijadikan alamat utama."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (request.IsPrimary)
                {
                    await UnsetOtherPrimaryAsync(workforceProfileId, id, now, actorUserId, cancellationToken);
                }

                entity.IsPrimary = request.IsPrimary;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Ok(ApiResponse<object>.Ok(
                    null,
                    request.IsPrimary
                        ? "Alamat utama workforce berhasil ditetapkan."
                        : "Status alamat utama workforce berhasil dilepas."));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceAddress.SetPrimaryAddress",
                    "Gagal menetapkan alamat utama workforce.",
                    ex);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat menetapkan alamat utama workforce."));
            }
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Address", Description = "Memverifikasi alamat workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceAddress", "Update")]
        public async Task<IActionResult> VerifyAddress(
            Guid workforceProfileId,
            Guid id,
            [FromBody] VerifyWfpAddressRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpAddress>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Alamat workforce tidak ditemukan."));
            }

            entity.IsVerified = request.IsVerified;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                request.IsVerified
                    ? "Alamat workforce berhasil diverifikasi."
                    : "Verifikasi alamat workforce berhasil dibatalkan."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Address", Description = "Menghapus alamat workforce", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WorkforceAddress", "Delete")]
        public async Task<IActionResult> DeleteAddress(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpAddress>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Alamat workforce tidak ditemukan."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsPrimary = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceAddress.DeleteAddress",
                "Menghapus alamat workforce.",
                new { entity.Id, entity.WorkforceProfileId, entity.AddressType });

            return Ok(ApiResponse<object>.Ok(
                null,
                "Alamat workforce berhasil dihapus."));
        }

        private IQueryable<WfpAddress> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpAddress>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Country)
                .Include(x => x.Province)
                .Include(x => x.City)
                .Include(x => x.District)
                .Include(x => x.PostalCode)
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);
        }

        private static IQueryable<WfpAddress> ApplyFilter(
            IQueryable<WfpAddress> query,
            string? addressType,
            bool? isPrimary,
            bool? isVerified,
            bool? isActive,
            string? search)
        {
            if (!string.IsNullOrWhiteSpace(addressType))
            {
                var normalizedType = NormalizeAddressType(addressType);
                query = query.Where(x => x.AddressType == normalizedType);
            }

            if (isPrimary.HasValue)
                query = query.Where(x => x.IsPrimary == isPrimary.Value);

            if (isVerified.HasValue)
                query = query.Where(x => x.IsVerified == isVerified.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.AddressType.ToLower().Contains(keyword) ||
                    x.AddressLine.ToLower().Contains(keyword) ||
                    (x.VillageName != null && x.VillageName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.Country != null && x.Country.CountryName.ToLower().Contains(keyword)) ||
                    (x.Province != null && x.Province.ProvinceName.ToLower().Contains(keyword)) ||
                    (x.City != null && x.City.CityName.ToLower().Contains(keyword)) ||
                    (x.District != null && x.District.DistrictName.ToLower().Contains(keyword)) ||
                    (x.PostalCode != null && x.PostalCode.PostalCode.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpAddress> ApplySorting(
            IQueryable<WfpAddress> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = !string.Equals(
                sortDirection?.Trim(),
                "asc",
                StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "isPrimary").Trim().ToLowerInvariant() switch
            {
                "addresstype" => isDescending
                    ? query.OrderByDescending(x => x.AddressType).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.AddressType).ThenBy(x => x.CreateDateTime),

                "effectivestartdate" => isDescending
                    ? query.OrderByDescending(x => x.EffectiveStartDate).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.EffectiveStartDate).ThenBy(x => x.CreateDateTime),

                "isverified" => isDescending
                    ? query.OrderByDescending(x => x.IsVerified).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.IsVerified).ThenBy(x => x.CreateDateTime),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.CreateDateTime),

                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                _ => isDescending
                    ? query.OrderByDescending(x => x.IsPrimary).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.IsPrimary).ThenBy(x => x.CreateDateTime)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            CreateWfpAddressRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.AddressType))
                return (false, "Jenis alamat wajib diisi.");

            if (!AllowedAddressTypes.Contains(request.AddressType.Trim()))
                return (false, "Jenis alamat tidak valid. Gunakan Identity, Current, Domicile, Mailing, atau Emergency.");

            if (string.IsNullOrWhiteSpace(request.AddressLine))
                return (false, "Alamat lengkap wajib diisi.");

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                return (false, "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate.");
            }

            var coordinateValidation = ValidateCoordinates(request.Latitude, request.Longitude);
            if (!coordinateValidation.IsValid)
                return coordinateValidation;

            return await ValidateRegionAsync(
                request.CountryId,
                request.ProvinceId,
                request.CityId,
                request.DistrictId,
                request.PostalCodeId,
                cancellationToken);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRegionAsync(
            Guid? countryId,
            Guid? provinceId,
            Guid? cityId,
            Guid? districtId,
            Guid? postalCodeId,
            CancellationToken cancellationToken)
        {
            countryId = NormalizeNullableGuid(countryId);
            provinceId = NormalizeNullableGuid(provinceId);
            cityId = NormalizeNullableGuid(cityId);
            districtId = NormalizeNullableGuid(districtId);
            postalCodeId = NormalizeNullableGuid(postalCodeId);

            if (provinceId.HasValue && !countryId.HasValue)
                return (false, "Country wajib dipilih jika province diisi.");

            if (cityId.HasValue && !provinceId.HasValue)
                return (false, "Province wajib dipilih jika city diisi.");

            if (districtId.HasValue && !cityId.HasValue)
                return (false, "City wajib dipilih jika district diisi.");

            if (postalCodeId.HasValue && !districtId.HasValue)
                return (false, "District wajib dipilih jika postal code diisi.");

            if (countryId.HasValue &&
                !await _dbContext.MstCountries.AsNoTracking().AnyAsync(x =>
                    x.Id == countryId.Value && x.IsActive && !x.IsDelete,
                    cancellationToken))
            {
                return (false, "Country tidak valid atau tidak aktif.");
            }

            if (provinceId.HasValue &&
                !await _dbContext.MstProvinces.AsNoTracking().AnyAsync(x =>
                    x.Id == provinceId.Value &&
                    x.CountryId == countryId!.Value &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken))
            {
                return (false, "Province tidak valid, tidak aktif, atau tidak sesuai country.");
            }

            if (cityId.HasValue &&
                !await _dbContext.MstCities.AsNoTracking().AnyAsync(x =>
                    x.Id == cityId.Value &&
                    x.ProvinceId == provinceId!.Value &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken))
            {
                return (false, "City tidak valid, tidak aktif, atau tidak sesuai province.");
            }

            if (districtId.HasValue &&
                !await _dbContext.MstDistricts.AsNoTracking().AnyAsync(x =>
                    x.Id == districtId.Value &&
                    x.CityId == cityId!.Value &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken))
            {
                return (false, "District tidak valid, tidak aktif, atau tidak sesuai city.");
            }

            if (postalCodeId.HasValue &&
                !await _dbContext.MstPostalCodes.AsNoTracking().AnyAsync(x =>
                    x.Id == postalCodeId.Value &&
                    x.DistrictId == districtId!.Value &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken))
            {
                return (false, "Postal code tidak valid, tidak aktif, atau tidak sesuai district.");
            }

            return (true, null);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateCoordinates(
            string? latitude,
            string? longitude)
        {
            if (!string.IsNullOrWhiteSpace(latitude))
            {
                if (!TryParseCoordinate(latitude, out var parsedLatitude) ||
                    parsedLatitude < -90 ||
                    parsedLatitude > 90)
                {
                    return (false, "Latitude harus berupa angka antara -90 dan 90.");
                }
            }

            if (!string.IsNullOrWhiteSpace(longitude))
            {
                if (!TryParseCoordinate(longitude, out var parsedLongitude) ||
                    parsedLongitude < -180 ||
                    parsedLongitude > 180)
                {
                    return (false, "Longitude harus berupa angka antara -180 dan 180.");
                }
            }

            return (true, null);
        }

        private static bool TryParseCoordinate(string value, out decimal coordinate)
        {
            return decimal.TryParse(
                value.Trim().Replace(',', '.'),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out coordinate);
        }

        private async Task UnsetOtherPrimaryAsync(
            Guid workforceProfileId,
            Guid? exceptId,
            DateTime now,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<WfpAddress>()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsPrimary &&
                    x.IsActive &&
                    !x.IsDelete);

            if (exceptId.HasValue)
                query = query.Where(x => x.Id != exceptId.Value);

            var existingPrimaries = await query.ToListAsync(cancellationToken);

            foreach (var item in existingPrimaries)
            {
                item.IsPrimary = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
        }

        private async Task<bool> WorkforceProfileExistsAsync(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            return workforceProfileId != Guid.Empty &&
                   await _dbContext.MstWorkforceProfiles
                       .AsNoTracking()
                       .AnyAsync(x =>
                           x.Id == workforceProfileId &&
                           x.IsActive &&
                           !x.IsDelete,
                           cancellationToken);
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
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

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            return !value.HasValue || value.Value == Guid.Empty
                ? null
                : value.Value;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string? NormalizeCoordinate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value.Trim().Replace(',', '.');
        }

        private static string NormalizeAddressType(string value)
        {
            var selected = AllowedAddressTypes
                .FirstOrDefault(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));

            return selected ?? value.Trim();
        }

        private static string BuildAddressTypeLabel(string value)
        {
            return value switch
            {
                "Identity" => "Alamat Identitas",
                "Current" => "Alamat Saat Ini",
                "Domicile" => "Alamat Domisili",
                "Mailing" => "Alamat Surat-Menyurat",
                "Emergency" => "Alamat Darurat",
                _ => value
            };
        }
    }
}
