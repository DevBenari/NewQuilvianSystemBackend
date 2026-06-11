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
        private const string IdentityScannerProfileCodePrefix = "ISP-RSMMC-";
        private const int IdentityScannerProfileCodeDigitLength = 5;

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
            var profileTypeOptions = BuildEnumOptions<IdentityScannerProfileType>();

            var result = new IdentityScannerProfileFilterMetadataResponse
            {
                DefaultFilter = new IdentityScannerProfileDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
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
                    new() { Value = "isForIdentityCard", Label = "Untuk kartu identitas" },
                    new() { Value = "isForPatientCard", Label = "Untuk kartu pasien" },
                    new() { Value = "isForMembershipCard", Label = "Untuk kartu membership" },
                    new() { Value = "isForInsuranceCard", Label = "Untuk kartu asuransi" },
                    new() { Value = "isOcrEnabled", Label = "OCR aktif" },
                    new() { Value = "isBarcodeEnabled", Label = "Barcode aktif" },
                    new() { Value = "isQrEnabled", Label = "QR aktif" },
                    new() { Value = "isManualInputAllowed", Label = "Manual input diizinkan" },
                    new() { Value = "isAutoCreatePatientAllowed", Label = "Auto create pasien diizinkan" },
                    new() { Value = "isVerificationRequired", Label = "Verifikasi wajib" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                EnumOptions = BuildEnumMetadataOptions(profileTypeOptions),
                ProfileTypeOptions = profileTypeOptions,
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
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
                QrEnabledProfile = await query.CountAsync(x => x.IsQrEnabled),
                ManualInputAllowedProfile = await query.CountAsync(x => x.IsManualInputAllowed),
                AutoCreatePatientAllowedProfile = await query.CountAsync(x => x.IsAutoCreatePatientAllowed),
                VerificationRequiredProfile = await query.CountAsync(x => x.IsVerificationRequired)
            };

            return Ok(ApiResponse<IdentityScannerProfileSummaryResponse>.Ok(
                result,
                "Ringkasan identity scanner profile berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseIdentityScannerProfilePagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
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
            [FromQuery] bool? isAutoCreatePatientAllowed,
            [FromQuery] bool? isVerificationRequired,
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
                profileType,
                isForIdentityCard,
                isForPatientCard,
                isForMembershipCard,
                isForInsuranceCard,
                isOcrEnabled,
                isBarcodeEnabled,
                isQrEnabled,
                isManualInputAllowed,
                isAutoCreatePatientAllowed,
                isVerificationRequired
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
            [FromQuery] bool? isAutoCreatePatientAllowed,
            [FromQuery] bool? isVerificationRequired,
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
                isManualInputAllowed,
                isAutoCreatePatientAllowed,
                isVerificationRequired
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

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var generatedProfileCode = await GenerateIdentityScannerProfileCodeAsync();

                var entity = new MstIdentityScannerProfile
                {
                    Id = Guid.NewGuid(),
                    ProfileCode = generatedProfileCode,
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
                await transaction.CommitAsync();

                var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy });

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
                    IsActive = entity.IsActive,
                    CreateDateTime = entity.CreateDateTime,
                    CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                    CreateByName = GetActorName(actorNames, entity.CreateBy)
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
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "IdentityScannerProfile.CreateIdentityScannerProfile",
                    "Gagal membuat data identity scanner profile.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat identity scanner profile."
                    )
                );
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<IdentityScannerProfileUpdateResponse>), StatusCodes.Status200OK)]
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

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });

            var result = new IdentityScannerProfileUpdateResponse
            {
                Id = entity.Id,
                ProfileCode = entity.ProfileCode,
                ProfileName = entity.ProfileName,
                ProfileType = entity.ProfileType,
                ProfileTypeName = BuildProfileTypeLabel(entity.ProfileType),
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "IdentityScannerProfile.UpdateIdentityScannerProfile",
                "Mengubah data identity scanner profile.",
                result
            );

            return Ok(ApiResponse<IdentityScannerProfileUpdateResponse>.Ok(
                result,
                "Identity scanner profile berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<IdentityScannerProfileUpdateResponse>), StatusCodes.Status200OK)]
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

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });

            var result = new IdentityScannerProfileUpdateResponse
            {
                Id = entity.Id,
                ProfileCode = entity.ProfileCode,
                ProfileName = entity.ProfileName,
                ProfileType = entity.ProfileType,
                ProfileTypeName = BuildProfileTypeLabel(entity.ProfileType),
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            return Ok(ApiResponse<IdentityScannerProfileUpdateResponse>.Ok(
                result,
                "Status identity scanner profile berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<IdentityScannerProfileDeleteResponse>), StatusCodes.Status200OK)]
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

            var actorNames = await GetActorNameMapAsync(new[] { entity.DeleteBy });

            var result = new IdentityScannerProfileDeleteResponse
            {
                Id = entity.Id,
                ProfileCode = entity.ProfileCode,
                ProfileName = entity.ProfileName,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : (Guid?)entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "IdentityScannerProfile.DeleteIdentityScannerProfile",
                "Menghapus data identity scanner profile.",
                result
            );

            return Ok(ApiResponse<IdentityScannerProfileDeleteResponse>.Ok(
                result,
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
            bool? isManualInputAllowed,
            bool? isAutoCreatePatientAllowed,
            bool? isVerificationRequired)
        {
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

            if (isAutoCreatePatientAllowed.HasValue)
            {
                query = query.Where(x => x.IsAutoCreatePatientAllowed == isAutoCreatePatientAllowed.Value);
            }

            if (isVerificationRequired.HasValue)
            {
                query = query.Where(x => x.IsVerificationRequired == isVerificationRequired.Value);
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

                "isforidentitycard" => isDescending
                    ? query.OrderByDescending(x => x.IsForIdentityCard).ThenBy(x => x.ProfileName)
                    : query.OrderBy(x => x.IsForIdentityCard).ThenBy(x => x.ProfileName),

                "isforpatientcard" => isDescending
                    ? query.OrderByDescending(x => x.IsForPatientCard).ThenBy(x => x.ProfileName)
                    : query.OrderBy(x => x.IsForPatientCard).ThenBy(x => x.ProfileName),

                "isformembershipcard" => isDescending
                    ? query.OrderByDescending(x => x.IsForMembershipCard).ThenBy(x => x.ProfileName)
                    : query.OrderBy(x => x.IsForMembershipCard).ThenBy(x => x.ProfileName),

                "isforinsurancecard" => isDescending
                    ? query.OrderByDescending(x => x.IsForInsuranceCard).ThenBy(x => x.ProfileName)
                    : query.OrderBy(x => x.IsForInsuranceCard).ThenBy(x => x.ProfileName),

                "isocrenabled" => isDescending
                    ? query.OrderByDescending(x => x.IsOcrEnabled).ThenBy(x => x.ProfileName)
                    : query.OrderBy(x => x.IsOcrEnabled).ThenBy(x => x.ProfileName),

                "isbarcodeenabled" => isDescending
                    ? query.OrderByDescending(x => x.IsBarcodeEnabled).ThenBy(x => x.ProfileName)
                    : query.OrderBy(x => x.IsBarcodeEnabled).ThenBy(x => x.ProfileName),

                "isqrenabled" => isDescending
                    ? query.OrderByDescending(x => x.IsQrEnabled).ThenBy(x => x.ProfileName)
                    : query.OrderBy(x => x.IsQrEnabled).ThenBy(x => x.ProfileName),

                "ismanualinputallowed" => isDescending
                    ? query.OrderByDescending(x => x.IsManualInputAllowed).ThenBy(x => x.ProfileName)
                    : query.OrderBy(x => x.IsManualInputAllowed).ThenBy(x => x.ProfileName),

                "isautocreatepatientallowed" => isDescending
                    ? query.OrderByDescending(x => x.IsAutoCreatePatientAllowed).ThenBy(x => x.ProfileName)
                    : query.OrderBy(x => x.IsAutoCreatePatientAllowed).ThenBy(x => x.ProfileName),

                "isverificationrequired" => isDescending
                    ? query.OrderByDescending(x => x.IsVerificationRequired).ThenBy(x => x.ProfileName)
                    : query.OrderBy(x => x.IsVerificationRequired).ThenBy(x => x.ProfileName),

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

            if (request.SortOrder < 0)
            {
                return (false, "Urutan tidak boleh kurang dari 0.");
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

            var normalizedName = request.ProfileName.Trim().ToLower();

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

        private async Task<string> GenerateIdentityScannerProfileCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstIdentityScannerProfile>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.ProfileCode.StartsWith(IdentityScannerProfileCodePrefix))
                .Select(x => x.ProfileCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(TryExtractIdentityScannerProfileSequenceNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return IdentityScannerProfileCodePrefix + nextNumber.ToString("D" + IdentityScannerProfileCodeDigitLength, CultureInfo.InvariantCulture);
        }

        private static int? TryExtractIdentityScannerProfileSequenceNumber(string profileCode)
        {
            if (string.IsNullOrWhiteSpace(profileCode))
            {
                return null;
            }

            if (!profileCode.StartsWith(IdentityScannerProfileCodePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var numberText = profileCode[IdentityScannerProfileCodePrefix.Length..];

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
                ScannerVendorName = entity.ScannerVendorName,
                ScannerModel = entity.ScannerModel,
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
                SortOrder = entity.SortOrder
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

        private static List<IdentityScannerProfileEnumMetadataResponse> BuildEnumMetadataOptions(
            List<IdentityScannerProfileEnumOptionResponse> profileTypeOptions)
        {
            return new List<IdentityScannerProfileEnumMetadataResponse>
            {
                new()
                {
                    EnumName = nameof(IdentityScannerProfileType),
                    FieldName = "profileType",
                    OptionsSource = "profileTypeOptions",
                    Description = "Enum tipe identity scanner profile untuk field profileType pada create, update, filter, response, dan option.",
                    Options = profileTypeOptions
                        .Select(x => new IdentityScannerProfileEnumMetadataOptionResponse
                        {
                            Value = x.Value,
                            Name = x.Name,
                            Label = x.Label
                        })
                        .ToList()
                }
            };
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

        private static List<IdentityScannerProfileCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<IdentityScannerProfileCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false }
            };
        }

        private static List<IdentityScannerProfileQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<IdentityScannerProfileQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "DateTime?", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "DateTime?", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Filter periode cepat: custom, today, last7days, last30days, thismonth, lastmonth.", Example = "thismonth" },
                new() { Name = "search", Type = "string", Description = "Cari berdasarkan kode, nama profile, tipe, vendor, model, format, field mapping, regex, configuration, atau deskripsi.", Example = "KTP" },
                new() { Name = "profileType", Type = "enum", Description = "Filter berdasarkan tipe scanner profile.", Example = "1" },
                new() { Name = "isActive", Type = "bool", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "isForIdentityCard", Type = "bool", Description = "Filter profile untuk kartu identitas.", Example = "true" },
                new() { Name = "isForPatientCard", Type = "bool", Description = "Filter profile untuk kartu pasien.", Example = "true" },
                new() { Name = "isForMembershipCard", Type = "bool", Description = "Filter profile untuk kartu membership.", Example = "true" },
                new() { Name = "isForInsuranceCard", Type = "bool", Description = "Filter profile untuk kartu asuransi.", Example = "true" },
                new() { Name = "isOcrEnabled", Type = "bool", Description = "Filter OCR aktif.", Example = "true" },
                new() { Name = "isBarcodeEnabled", Type = "bool", Description = "Filter barcode aktif.", Example = "true" },
                new() { Name = "isQrEnabled", Type = "bool", Description = "Filter QR aktif.", Example = "true" },
                new() { Name = "isManualInputAllowed", Type = "bool", Description = "Filter manual input diizinkan.", Example = "true" },
                new() { Name = "isAutoCreatePatientAllowed", Type = "bool", Description = "Filter auto create pasien diizinkan.", Example = "false" },
                new() { Name = "isVerificationRequired", Type = "bool", Description = "Filter verifikasi wajib.", Example = "true" },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting.", Example = "sortOrder" },
                new() { Name = "sortDirection", Type = "string", Description = "Arah sorting: asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman, maksimal 100.", Example = "25" }
            };
        }

        private static List<IdentityScannerProfileFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: false);
        }

        private static List<IdentityScannerProfileFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: true);
        }

        private static List<IdentityScannerProfileFormFieldMetadataResponse> BuildFieldMetadata(bool isUpdate)
        {
            var fields = new List<IdentityScannerProfileFormFieldMetadataResponse>
            {
                new() { Name = "profileCode", Label = "Kode Profile", Section = "Basic", InputType = "readonly", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "AutoGenerated", MaxLength = 50, Description = "Digenerate otomatis oleh sistem dengan format ISP-RSMMC-00001.", Example = "ISP-RSMMC-00001", SortOrder = 1 },
                new() { Name = "profileName", Label = "Nama Profile", Section = "Basic", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 150, Example = "KTP OCR Default", SortOrder = 2 },
                new() { Name = "profileType", Label = "Tipe Profile", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "profileTypeOptions", SortOrder = 3 },
                new() { Name = "scannerVendorName", Label = "Vendor Scanner", Section = "Scanner", InputType = "text", MaxLength = 100, Example = "Zebra", SortOrder = 4 },
                new() { Name = "scannerModel", Label = "Model Scanner", Section = "Scanner", InputType = "text", MaxLength = 100, Example = "DS2208", SortOrder = 5 },
                new() { Name = "inputFormat", Label = "Input Format", Section = "Format", InputType = "text", MaxLength = 100, Example = "Image", SortOrder = 6 },
                new() { Name = "outputFormat", Label = "Output Format", Section = "Format", InputType = "text", MaxLength = 100, Example = "JSON", SortOrder = 7 },
                new() { Name = "identityNumberRegex", Label = "Regex Nomor Identitas", Section = "Regex", InputType = "text", MaxLength = 250, Example = "^[0-9]{16}$", SortOrder = 8 },
                new() { Name = "memberNumberRegex", Label = "Regex Nomor Member", Section = "Regex", InputType = "text", MaxLength = 250, SortOrder = 9 },
                new() { Name = "cardNumberRegex", Label = "Regex Nomor Kartu", Section = "Regex", InputType = "text", MaxLength = 250, SortOrder = 10 },
                new() { Name = "identityNumberFieldName", Label = "Field Nomor Identitas", Section = "Field Mapping", InputType = "text", MaxLength = 100, Example = "nik", SortOrder = 11 },
                new() { Name = "fullNameFieldName", Label = "Field Nama Lengkap", Section = "Field Mapping", InputType = "text", MaxLength = 100, Example = "fullName", SortOrder = 12 },
                new() { Name = "birthDateFieldName", Label = "Field Tanggal Lahir", Section = "Field Mapping", InputType = "text", MaxLength = 100, Example = "birthDate", SortOrder = 13 },
                new() { Name = "genderFieldName", Label = "Field Gender", Section = "Field Mapping", InputType = "text", MaxLength = 100, Example = "gender", SortOrder = 14 },
                new() { Name = "addressFieldName", Label = "Field Alamat", Section = "Field Mapping", InputType = "text", MaxLength = 100, Example = "address", SortOrder = 15 },
                new() { Name = "isForIdentityCard", Label = "Untuk Kartu Identitas", Section = "Usage", InputType = "switch", SortOrder = 16 },
                new() { Name = "isForPatientCard", Label = "Untuk Kartu Pasien", Section = "Usage", InputType = "switch", SortOrder = 17 },
                new() { Name = "isForMembershipCard", Label = "Untuk Kartu Membership", Section = "Usage", InputType = "switch", SortOrder = 18 },
                new() { Name = "isForInsuranceCard", Label = "Untuk Kartu Asuransi", Section = "Usage", InputType = "switch", SortOrder = 19 },
                new() { Name = "isOcrEnabled", Label = "OCR Aktif", Section = "Input Method", InputType = "switch", SortOrder = 20 },
                new() { Name = "isBarcodeEnabled", Label = "Barcode Aktif", Section = "Input Method", InputType = "switch", SortOrder = 21 },
                new() { Name = "isQrEnabled", Label = "QR Aktif", Section = "Input Method", InputType = "switch", SortOrder = 22 },
                new() { Name = "isManualInputAllowed", Label = "Izinkan Manual Input", Section = "Input Method", InputType = "switch", SortOrder = 23 },
                new() { Name = "isAutoCreatePatientAllowed", Label = "Izinkan Auto Create Pasien", Section = "Rule", InputType = "switch", SortOrder = 24 },
                new() { Name = "isVerificationRequired", Label = "Wajib Verifikasi", Section = "Rule", InputType = "switch", SortOrder = 25 },
                new() { Name = "sortOrder", Label = "Urutan", Section = "Display", InputType = "number", SortOrder = 26 },
                new() { Name = "configurationJson", Label = "Configuration JSON", Section = "Configuration", InputType = "textarea", MaxLength = 500, Description = "JSON konfigurasi tambahan. Harus valid jika diisi.", Example = "{\"timeout\":30}", SortOrder = 27 },
                new() { Name = "description", Label = "Deskripsi", Section = "Additional", InputType = "textarea", MaxLength = 250, SortOrder = 28 }
            };

            if (isUpdate)
            {
                fields.Add(new IdentityScannerProfileFormFieldMetadataResponse
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

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
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
