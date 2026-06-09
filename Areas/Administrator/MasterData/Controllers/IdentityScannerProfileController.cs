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
using System.Text.Json;
using System.Text.RegularExpressions;

using ResponseIdentityScannerProfilePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs.IdentityScannerProfileResponse>;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/administrator/master-data/identity-scanner-profiles")]
    [AccessController(
        moduleCode: "ADMINISTRATOR_MASTER_DATA",
        moduleName: "Administrator Master Data",
        displayName: "Identity Scanner Profile",
        AreaName = "Administrator",
        ControllerName = "IdentityScannerProfile",
        Description = "Administrator master data identity scanner profile",
        SortOrder = 7
    )]
    [Tags("Administrator / Master Data / Identity Scanner Profile")]
    public class IdentityScannerProfileController : ControllerBase
    {
        private const string LogCategory = "Administrator.MasterData";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public IdentityScannerProfileController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<IdentityScannerProfileFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Identity Scanner Profile",
            Description = "Melihat metadata filter identity scanner profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("IdentityScannerProfile", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new IdentityScannerProfileFilterMetadataResponse
            {
                DefaultFilter = new IdentityScannerProfileDefaultFilterResponse(),
                CustomPeriods = new List<IdentityScannerProfileCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<IdentityScannerProfileSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "profileCode", Label = "Kode profile" },
                    new() { Value = "profileName", Label = "Nama profile" },
                    new() { Value = "profileType", Label = "Tipe profile" },
                    new() { Value = "scannerVendorName", Label = "Vendor scanner" },
                    new() { Value = "scannerModel", Label = "Model scanner" },
                    new() { Value = "inputFormat", Label = "Input format" },
                    new() { Value = "outputFormat", Label = "Output format" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ProfileTypeOptions = BuildEnumOptions<IdentityScannerProfileType>(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "IdentityScannerProfile.GetFilterMetadata",
                "Mengambil metadata filter identity scanner profile.",
                result
            );

            return Ok(ApiResponse<IdentityScannerProfileFilterMetadataResponse>.Ok(
                result,
                "Metadata filter identity scanner profile berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<IdentityScannerProfileSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Identity Scanner Profile",
            Description = "Melihat ringkasan identity scanner profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("IdentityScannerProfile", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new IdentityScannerProfileSummaryResponse
            {
                TotalProfile = await query.CountAsync(),
                ActiveProfile = await query.CountAsync(x => x.IsActive),
                InactiveProfile = await query.CountAsync(x => !x.IsActive),
                IdentityCardProfile = await query.CountAsync(x => x.IsForIdentityCard),
                PatientCardProfile = await query.CountAsync(x => x.IsForPatientCard),
                MembershipCardProfile = await query.CountAsync(x => x.IsForMembershipCard),
                InsuranceCardProfile = await query.CountAsync(x => x.IsForInsuranceCard),
                OcrEnabledProfile = await query.CountAsync(x => x.IsOcrEnabled),
                BarcodeEnabledProfile = await query.CountAsync(x => x.IsBarcodeEnabled),
                QrEnabledProfile = await query.CountAsync(x => x.IsQrEnabled)
            };

            return Ok(ApiResponse<IdentityScannerProfileSummaryResponse>.Ok(
                result,
                "Ringkasan identity scanner profile berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseIdentityScannerProfilePagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Identity Scanner Profile",
            Description = "Melihat data identity scanner profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("IdentityScannerProfile", "Read")]
        public async Task<IActionResult> GetIdentityScannerProfiles(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] IdentityScannerProfileType? profileType,
            [FromQuery] bool? isForIdentityCard,
            [FromQuery] bool? isForPatientCard,
            [FromQuery] bool? isForMembershipCard,
            [FromQuery] bool? isForInsuranceCard,
            [FromQuery] bool? isOcrEnabled,
            [FromQuery] bool? isBarcodeEnabled,
            [FromQuery] bool? isQrEnabled,
            [FromQuery] bool? isManualInputAllowed,
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
            query = ApplyStandardFilter(
                query,
                search,
                isActive,
                profileType,
                isForIdentityCard,
                isForPatientCard,
                isForMembershipCard,
                isForInsuranceCard,
                isOcrEnabled,
                isBarcodeEnabled,
                isQrEnabled,
                isManualInputAllowed
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

            var result = new ResponseIdentityScannerProfilePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseIdentityScannerProfilePagedResult>.Ok(
                result,
                "Data identity scanner profile berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<IdentityScannerProfileOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Identity Scanner Profile",
            Description = "Melihat data pilihan identity scanner profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("IdentityScannerProfile", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] IdentityScannerProfileType? profileType,
            [FromQuery] bool? isForIdentityCard,
            [FromQuery] bool? isForPatientCard,
            [FromQuery] bool? isForMembershipCard,
            [FromQuery] bool? isForInsuranceCard,
            [FromQuery] bool? isOcrEnabled,
            [FromQuery] bool? isBarcodeEnabled,
            [FromQuery] bool? isQrEnabled,
            [FromQuery] bool? isManualInputAllowed,
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool? activeOnly = null,
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
                profileType,
                isForIdentityCard,
                isForPatientCard,
                isForMembershipCard,
                isForInsuranceCard,
                isOcrEnabled,
                isBarcodeEnabled,
                isQrEnabled,
                isManualInputAllowed
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ProfileName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapOptionResponse)
                .ToList();

            var result = new IdentityScannerProfileOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<IdentityScannerProfileOptionPagedResponse>.Ok(
                result,
                "Data pilihan identity scanner profile berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<IdentityScannerProfileDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Identity Scanner Profile",
            Description = "Melihat detail identity scanner profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("IdentityScannerProfile", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Identity scanner profile tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var result = MapDetailResponse(entity, actorNames);

            if (result.UpdateDateTime.HasValue &&
                result.UpdateDateTime.Value == DateTime.MinValue)
            {
                result.UpdateDateTime = null;
            }

            if (!result.CreateBy.HasValue || result.CreateBy.Value == Guid.Empty)
            {
                result.CreateBy = null;
                result.CreateByName = null;
            }

            if (!result.UpdateBy.HasValue || result.UpdateBy.Value == Guid.Empty)
            {
                result.UpdateBy = null;
                result.UpdateByName = null;
            }

            return Ok(ApiResponse<IdentityScannerProfileDetailResponse>.Ok(
                result,
                "Detail identity scanner profile berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<IdentityScannerProfileCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Create",
            "Create Identity Scanner Profile",
            Description = "Membuat data identity scanner profile",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("IdentityScannerProfile", "Create")]
        public async Task<IActionResult> CreateIdentityScannerProfile([FromBody] CreateIdentityScannerProfileRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                request: request
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data identity scanner profile tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstIdentityScannerProfile
            {
                Id = Guid.NewGuid(),
                ProfileCode = NormalizeCode(request.ProfileCode),
                ProfileName = request.ProfileName.Trim(),
                ProfileType = request.ProfileType,
                ScannerVendorName = NormalizeNullableText(request.ScannerVendorName),
                ScannerModel = NormalizeNullableText(request.ScannerModel),
                InputFormat = NormalizeNullableText(request.InputFormat),
                OutputFormat = NormalizeNullableText(request.OutputFormat),
                IdentityNumberRegex = NormalizeNullableText(request.IdentityNumberRegex),
                MemberNumberRegex = NormalizeNullableText(request.MemberNumberRegex),
                CardNumberRegex = NormalizeNullableText(request.CardNumberRegex),
                IdentityNumberFieldName = NormalizeNullableText(request.IdentityNumberFieldName),
                FullNameFieldName = NormalizeNullableText(request.FullNameFieldName),
                BirthDateFieldName = NormalizeNullableText(request.BirthDateFieldName),
                GenderFieldName = NormalizeNullableText(request.GenderFieldName),
                AddressFieldName = NormalizeNullableText(request.AddressFieldName),
                IsForIdentityCard = request.IsForIdentityCard,
                IsForPatientCard = request.IsForPatientCard,
                IsForMembershipCard = request.IsForMembershipCard,
                IsForInsuranceCard = request.IsForInsuranceCard,
                IsOcrEnabled = request.IsOcrEnabled,
                IsBarcodeEnabled = request.IsBarcodeEnabled,
                IsQrEnabled = request.IsQrEnabled,
                IsManualInputAllowed = request.IsManualInputAllowed,
                IsAutoCreatePatientAllowed = request.IsAutoCreatePatientAllowed,
                IsVerificationRequired = request.IsVerificationRequired,
                SortOrder = request.SortOrder,
                ConfigurationJson = NormalizeNullableText(request.ConfigurationJson),
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstIdentityScannerProfile>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new IdentityScannerProfileCreateResponse
            {
                Id = entity.Id,
                ProfileCode = entity.ProfileCode,
                ProfileName = entity.ProfileName,
                ProfileType = entity.ProfileType,
                ProfileTypeName = BuildProfileTypeLabel(entity.ProfileType),
                IsForIdentityCard = entity.IsForIdentityCard,
                IsForPatientCard = entity.IsForPatientCard,
                IsForMembershipCard = entity.IsForMembershipCard,
                IsForInsuranceCard = entity.IsForInsuranceCard,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "IdentityScannerProfile.CreateIdentityScannerProfile",
                "Membuat data identity scanner profile.",
                result
            );

            return Ok(ApiResponse<IdentityScannerProfileCreateResponse>.Ok(
                result,
                "Identity scanner profile berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Identity Scanner Profile",
            Description = "Mengubah data identity scanner profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("IdentityScannerProfile", "Update")]
        public async Task<IActionResult> UpdateIdentityScannerProfile(
            Guid id,
            [FromBody] UpdateIdentityScannerProfileRequest request)
        {
            var entity = await _dbContext.Set<MstIdentityScannerProfile>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Identity scanner profile tidak ditemukan."
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
                    validation.ErrorMessage ?? "Data identity scanner profile tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.ProfileCode = NormalizeCode(request.ProfileCode);
            entity.ProfileName = request.ProfileName.Trim();
            entity.ProfileType = request.ProfileType;
            entity.ScannerVendorName = NormalizeNullableText(request.ScannerVendorName);
            entity.ScannerModel = NormalizeNullableText(request.ScannerModel);
            entity.InputFormat = NormalizeNullableText(request.InputFormat);
            entity.OutputFormat = NormalizeNullableText(request.OutputFormat);
            entity.IdentityNumberRegex = NormalizeNullableText(request.IdentityNumberRegex);
            entity.MemberNumberRegex = NormalizeNullableText(request.MemberNumberRegex);
            entity.CardNumberRegex = NormalizeNullableText(request.CardNumberRegex);
            entity.IdentityNumberFieldName = NormalizeNullableText(request.IdentityNumberFieldName);
            entity.FullNameFieldName = NormalizeNullableText(request.FullNameFieldName);
            entity.BirthDateFieldName = NormalizeNullableText(request.BirthDateFieldName);
            entity.GenderFieldName = NormalizeNullableText(request.GenderFieldName);
            entity.AddressFieldName = NormalizeNullableText(request.AddressFieldName);
            entity.IsForIdentityCard = request.IsForIdentityCard;
            entity.IsForPatientCard = request.IsForPatientCard;
            entity.IsForMembershipCard = request.IsForMembershipCard;
            entity.IsForInsuranceCard = request.IsForInsuranceCard;
            entity.IsOcrEnabled = request.IsOcrEnabled;
            entity.IsBarcodeEnabled = request.IsBarcodeEnabled;
            entity.IsQrEnabled = request.IsQrEnabled;
            entity.IsManualInputAllowed = request.IsManualInputAllowed;
            entity.IsAutoCreatePatientAllowed = request.IsAutoCreatePatientAllowed;
            entity.IsVerificationRequired = request.IsVerificationRequired;
            entity.SortOrder = request.SortOrder;
            entity.ConfigurationJson = NormalizeNullableText(request.ConfigurationJson);
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "IdentityScannerProfile.UpdateIdentityScannerProfile",
                "Mengubah data identity scanner profile.",
                new
                {
                    entity.Id,
                    entity.ProfileCode,
                    entity.ProfileName,
                    entity.ProfileType,
                    entity.IsActive
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Identity scanner profile berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Identity Scanner Profile Status",
            Description = "Mengubah status identity scanner profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("IdentityScannerProfile", "Update")]
        public async Task<IActionResult> UpdateIdentityScannerProfileStatus(
            Guid id,
            [FromBody] UpdateIdentityScannerProfileStatusRequest request)
        {
            var entity = await _dbContext.Set<MstIdentityScannerProfile>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Identity scanner profile tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status identity scanner profile berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Identity Scanner Profile",
            Description = "Menghapus data identity scanner profile",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("IdentityScannerProfile", "Delete")]
        public async Task<IActionResult> DeleteIdentityScannerProfile(
            Guid id,
            [FromBody] DeleteIdentityScannerProfileRequest? request = null)
        {
            var entity = await _dbContext.Set<MstIdentityScannerProfile>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Identity scanner profile tidak ditemukan."
                ));
            }

            var isUsedByDevice = await _dbContext.Set<MstKioskDevice>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.DefaultScannerProfileId == id);

            if (isUsedByDevice)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Identity scanner profile tidak dapat dihapus karena masih digunakan oleh kiosk device."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
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
                "IdentityScannerProfile.DeleteIdentityScannerProfile",
                "Menghapus data identity scanner profile.",
                new
                {
                    entity.Id,
                    entity.ProfileCode,
                    entity.ProfileName,
                    entity.DeleteDateTime
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Identity scanner profile berhasil dihapus."
            ));
        }

        private IQueryable<MstIdentityScannerProfile> BuildBaseQuery()
        {
            return _dbContext.Set<MstIdentityScannerProfile>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstIdentityScannerProfile> ApplyDateFilter(
            IQueryable<MstIdentityScannerProfile> query,
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

        private static IQueryable<MstIdentityScannerProfile> ApplyStandardFilter(
            IQueryable<MstIdentityScannerProfile> query,
            string? search,
            bool? isActive,
            IdentityScannerProfileType? profileType,
            bool? isForIdentityCard,
            bool? isForPatientCard,
            bool? isForMembershipCard,
            bool? isForInsuranceCard,
            bool? isOcrEnabled,
            bool? isBarcodeEnabled,
            bool? isQrEnabled,
            bool? isManualInputAllowed)
        {
            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (profileType.HasValue)
            {
                query = query.Where(x => x.ProfileType == profileType.Value);
            }

            if (isForIdentityCard.HasValue)
            {
                query = query.Where(x => x.IsForIdentityCard == isForIdentityCard.Value);
            }

            if (isForPatientCard.HasValue)
            {
                query = query.Where(x => x.IsForPatientCard == isForPatientCard.Value);
            }

            if (isForMembershipCard.HasValue)
            {
                query = query.Where(x => x.IsForMembershipCard == isForMembershipCard.Value);
            }

            if (isForInsuranceCard.HasValue)
            {
                query = query.Where(x => x.IsForInsuranceCard == isForInsuranceCard.Value);
            }

            if (isOcrEnabled.HasValue)
            {
                query = query.Where(x => x.IsOcrEnabled == isOcrEnabled.Value);
            }

            if (isBarcodeEnabled.HasValue)
            {
                query = query.Where(x => x.IsBarcodeEnabled == isBarcodeEnabled.Value);
            }

            if (isQrEnabled.HasValue)
            {
                query = query.Where(x => x.IsQrEnabled == isQrEnabled.Value);
            }

            if (isManualInputAllowed.HasValue)
            {
                query = query.Where(x => x.IsManualInputAllowed == isManualInputAllowed.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                var matchedProfileTypes = Enum.GetValues<IdentityScannerProfileType>()
                    .Where(x =>
                        x.ToString().ToLower().Contains(keyword) ||
                        BuildProfileTypeLabel(x).ToLower().Contains(keyword))
                    .ToList();

                query = query.Where(x =>
                    x.ProfileCode.ToLower().Contains(keyword) ||
                    x.ProfileName.ToLower().Contains(keyword) ||
                    (x.ScannerVendorName != null && x.ScannerVendorName.ToLower().Contains(keyword)) ||
                    (x.ScannerModel != null && x.ScannerModel.ToLower().Contains(keyword)) ||
                    (x.InputFormat != null && x.InputFormat.ToLower().Contains(keyword)) ||
                    (x.OutputFormat != null && x.OutputFormat.ToLower().Contains(keyword)) ||
                    (x.IdentityNumberRegex != null && x.IdentityNumberRegex.ToLower().Contains(keyword)) ||
                    (x.MemberNumberRegex != null && x.MemberNumberRegex.ToLower().Contains(keyword)) ||
                    (x.CardNumberRegex != null && x.CardNumberRegex.ToLower().Contains(keyword)) ||
                    (x.IdentityNumberFieldName != null && x.IdentityNumberFieldName.ToLower().Contains(keyword)) ||
                    (x.FullNameFieldName != null && x.FullNameFieldName.ToLower().Contains(keyword)) ||
                    (x.BirthDateFieldName != null && x.BirthDateFieldName.ToLower().Contains(keyword)) ||
                    (x.GenderFieldName != null && x.GenderFieldName.ToLower().Contains(keyword)) ||
                    (x.AddressFieldName != null && x.AddressFieldName.ToLower().Contains(keyword)) ||
                    (x.ConfigurationJson != null && x.ConfigurationJson.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    matchedProfileTypes.Contains(x.ProfileType));
            }

            return query;
        }

        private static IOrderedQueryable<MstIdentityScannerProfile> ApplySorting(
            IQueryable<MstIdentityScannerProfile> query,
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

                "profilecode" => isDescending
                    ? query.OrderByDescending(x => x.ProfileCode)
                    : query.OrderBy(x => x.ProfileCode),

                "profilename" => isDescending
                    ? query.OrderByDescending(x => x.ProfileName)
                    : query.OrderBy(x => x.ProfileName),

                "profiletype" => isDescending
                    ? query.OrderByDescending(x => x.ProfileType).ThenBy(x => x.ProfileName)
                    : query.OrderBy(x => x.ProfileType).ThenBy(x => x.ProfileName),

                "scannervendorname" => isDescending
                    ? query.OrderByDescending(x => x.ScannerVendorName).ThenBy(x => x.ProfileName)
                    : query.OrderBy(x => x.ScannerVendorName).ThenBy(x => x.ProfileName),

                "scannermodel" => isDescending
                    ? query.OrderByDescending(x => x.ScannerModel).ThenBy(x => x.ProfileName)
                    : query.OrderBy(x => x.ScannerModel).ThenBy(x => x.ProfileName),

                "inputformat" => isDescending
                    ? query.OrderByDescending(x => x.InputFormat).ThenBy(x => x.ProfileName)
                    : query.OrderBy(x => x.InputFormat).ThenBy(x => x.ProfileName),

                "outputformat" => isDescending
                    ? query.OrderByDescending(x => x.OutputFormat).ThenBy(x => x.ProfileName)
                    : query.OrderBy(x => x.OutputFormat).ThenBy(x => x.ProfileName),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.ProfileName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.ProfileName),

                _ => isDescending
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.ProfileName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.ProfileName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateIdentityScannerProfileRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ProfileCode))
            {
                return (false, "Kode identity scanner profile wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(request.ProfileName))
            {
                return (false, "Nama identity scanner profile wajib diisi.");
            }

            if (!Enum.IsDefined(typeof(IdentityScannerProfileType), request.ProfileType))
            {
                return (false, "Tipe identity scanner profile tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (!request.IsManualInputAllowed &&
                !request.IsOcrEnabled &&
                !request.IsBarcodeEnabled &&
                !request.IsQrEnabled)
            {
                return (false, "Minimal satu metode input harus aktif: manual input, OCR, barcode, atau QR.");
            }

            var regexValidation = ValidateRegexFields(request);

            if (!regexValidation.IsValid)
            {
                return regexValidation;
            }

            if (!string.IsNullOrWhiteSpace(request.ConfigurationJson))
            {
                try
                {
                    using var _ = JsonDocument.Parse(request.ConfigurationJson);
                }
                catch (JsonException)
                {
                    return (false, "Configuration JSON tidak valid.");
                }
            }

            var normalizedCode = NormalizeCode(request.ProfileCode);
            var normalizedName = request.ProfileName.Trim().ToLower();

            var duplicateCodeQuery = _dbContext.Set<MstIdentityScannerProfile>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ProfileCode.ToUpper() == normalizedCode);

            if (excludeId.HasValue)
            {
                duplicateCodeQuery = duplicateCodeQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateCodeQuery.AnyAsync())
            {
                return (false, "Kode identity scanner profile sudah digunakan.");
            }

            var duplicateNameQuery = _dbContext.Set<MstIdentityScannerProfile>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ProfileName.ToLower() == normalizedName);

            if (excludeId.HasValue)
            {
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateNameQuery.AnyAsync())
            {
                return (false, "Nama identity scanner profile sudah digunakan.");
            }

            return (true, null);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateRegexFields(
            CreateIdentityScannerProfileRequest request)
        {
            var regexFields = new Dictionary<string, string?>
            {
                { "Identity number regex", request.IdentityNumberRegex },
                { "Member number regex", request.MemberNumberRegex },
                { "Card number regex", request.CardNumberRegex }
            };

            foreach (var regexField in regexFields)
            {
                if (string.IsNullOrWhiteSpace(regexField.Value))
                {
                    continue;
                }

                try
                {
                    _ = new Regex(regexField.Value, RegexOptions.None, TimeSpan.FromSeconds(2));
                }
                catch (ArgumentException)
                {
                    return (false, $"{regexField.Key} tidak valid.");
                }
            }

            return (true, null);
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

        private static IdentityScannerProfileResponse MapResponse(
            MstIdentityScannerProfile entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new IdentityScannerProfileResponse
            {
                Id = entity.Id,
                ProfileCode = entity.ProfileCode,
                ProfileName = entity.ProfileName,
                ProfileType = entity.ProfileType,
                ProfileTypeName = BuildProfileTypeLabel(entity.ProfileType),
                ScannerVendorName = entity.ScannerVendorName,
                ScannerModel = entity.ScannerModel,
                InputFormat = entity.InputFormat,
                OutputFormat = entity.OutputFormat,
                IsForIdentityCard = entity.IsForIdentityCard,
                IsForPatientCard = entity.IsForPatientCard,
                IsForMembershipCard = entity.IsForMembershipCard,
                IsForInsuranceCard = entity.IsForInsuranceCard,
                IsOcrEnabled = entity.IsOcrEnabled,
                IsBarcodeEnabled = entity.IsBarcodeEnabled,
                IsQrEnabled = entity.IsQrEnabled,
                IsManualInputAllowed = entity.IsManualInputAllowed,
                IsAutoCreatePatientAllowed = entity.IsAutoCreatePatientAllowed,
                IsVerificationRequired = entity.IsVerificationRequired,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static IdentityScannerProfileDetailResponse MapDetailResponse(
            MstIdentityScannerProfile entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new IdentityScannerProfileDetailResponse
            {
                Id = entity.Id,
                ProfileCode = entity.ProfileCode,
                ProfileName = entity.ProfileName,
                ProfileType = entity.ProfileType,
                ProfileTypeName = BuildProfileTypeLabel(entity.ProfileType),
                ScannerVendorName = entity.ScannerVendorName,
                ScannerModel = entity.ScannerModel,
                InputFormat = entity.InputFormat,
                OutputFormat = entity.OutputFormat,
                IdentityNumberRegex = entity.IdentityNumberRegex,
                MemberNumberRegex = entity.MemberNumberRegex,
                CardNumberRegex = entity.CardNumberRegex,
                IdentityNumberFieldName = entity.IdentityNumberFieldName,
                FullNameFieldName = entity.FullNameFieldName,
                BirthDateFieldName = entity.BirthDateFieldName,
                GenderFieldName = entity.GenderFieldName,
                AddressFieldName = entity.AddressFieldName,
                IsForIdentityCard = entity.IsForIdentityCard,
                IsForPatientCard = entity.IsForPatientCard,
                IsForMembershipCard = entity.IsForMembershipCard,
                IsForInsuranceCard = entity.IsForInsuranceCard,
                IsOcrEnabled = entity.IsOcrEnabled,
                IsBarcodeEnabled = entity.IsBarcodeEnabled,
                IsQrEnabled = entity.IsQrEnabled,
                IsManualInputAllowed = entity.IsManualInputAllowed,
                IsAutoCreatePatientAllowed = entity.IsAutoCreatePatientAllowed,
                IsVerificationRequired = entity.IsVerificationRequired,
                SortOrder = entity.SortOrder,
                ConfigurationJson = entity.ConfigurationJson,
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

        private static IdentityScannerProfileOptionResponse MapOptionResponse(
            MstIdentityScannerProfile entity)
        {
            return new IdentityScannerProfileOptionResponse
            {
                Id = entity.Id,
                ProfileCode = entity.ProfileCode,
                ProfileName = entity.ProfileName,
                ProfileType = entity.ProfileType,
                ProfileTypeName = BuildProfileTypeLabel(entity.ProfileType),
                IsForIdentityCard = entity.IsForIdentityCard,
                IsForPatientCard = entity.IsForPatientCard,
                IsForMembershipCard = entity.IsForMembershipCard,
                IsForInsuranceCard = entity.IsForInsuranceCard,
                IsOcrEnabled = entity.IsOcrEnabled,
                IsBarcodeEnabled = entity.IsBarcodeEnabled,
                IsQrEnabled = entity.IsQrEnabled,
                IsManualInputAllowed = entity.IsManualInputAllowed
            };
        }

        private static List<IdentityScannerProfileEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new IdentityScannerProfileEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = SplitPascalCase(x.ToString())
                })
                .ToList();
        }

        private static string BuildProfileTypeLabel(IdentityScannerProfileType value)
        {
            return SplitPascalCase(value.ToString());
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

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private static string NormalizeCode(string value)
        {
            return value.Trim().ToUpperInvariant();
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
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
