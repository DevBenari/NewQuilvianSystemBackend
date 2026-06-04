using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseIdentityScannerProfilePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.IdentityScannerProfileResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/identity-scanner-profiles")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Identity Scanner Profile",
        AreaName = "HealthServices",
        ControllerName = "IdentityScannerProfile",
        Description = "Health service master data identity scanner profile",
        SortOrder = 7
    )]
    [Tags("Health Services / Master Data / Identity Scanner Profile")]
    public class IdentityScannerProfileController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";

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
        [AccessAction("Read", "Read Identity Scanner Profile", Description = "Melihat data identity scanner profile", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("IdentityScannerProfile", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new IdentityScannerProfileFilterMetadataResponse
            {
                DefaultFilter = new IdentityScannerProfileDefaultFilterResponse(),
                SortOptions = new List<IdentityScannerProfileSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "profileCode", Label = "Kode profile" },
                    new() { Value = "profileName", Label = "Nama profile" },
                    new() { Value = "profileType", Label = "Tipe profile" },
                    new() { Value = "scannerVendorName", Label = "Vendor scanner" },
                    new() { Value = "scannerModel", Label = "Model scanner" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ProfileTypeOptions = BuildEnumOptions<IdentityScannerProfileType>()
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
        [AccessAction("Read", "Read Identity Scanner Profile", Description = "Melihat data identity scanner profile", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("IdentityScannerProfile", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstIdentityScannerProfile>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

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
        [AccessAction("Read", "Read Identity Scanner Profile", Description = "Melihat data identity scanner profile", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("IdentityScannerProfile", "Read")]
        public async Task<IActionResult> GetIdentityScannerProfiles(
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

            var query = _dbContext.Set<MstIdentityScannerProfile>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

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
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (profileType.HasValue)
                query = query.Where(x => x.ProfileType == profileType.Value);

            if (isForIdentityCard.HasValue)
                query = query.Where(x => x.IsForIdentityCard == isForIdentityCard.Value);

            if (isForPatientCard.HasValue)
                query = query.Where(x => x.IsForPatientCard == isForPatientCard.Value);

            if (isForMembershipCard.HasValue)
                query = query.Where(x => x.IsForMembershipCard == isForMembershipCard.Value);

            if (isForInsuranceCard.HasValue)
                query = query.Where(x => x.IsForInsuranceCard == isForInsuranceCard.Value);

            if (isOcrEnabled.HasValue)
                query = query.Where(x => x.IsOcrEnabled == isOcrEnabled.Value);

            if (isBarcodeEnabled.HasValue)
                query = query.Where(x => x.IsBarcodeEnabled == isBarcodeEnabled.Value);

            if (isQrEnabled.HasValue)
                query = query.Where(x => x.IsQrEnabled == isQrEnabled.Value);

            if (isManualInputAllowed.HasValue)
                query = query.Where(x => x.IsManualInputAllowed == isManualInputAllowed.Value);

            var totalCount = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => ToResponse(x))
                .ToListAsync();

            var result = new ResponseIdentityScannerProfilePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalCount,
                TotalPage = (int)Math.Ceiling(totalCount / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseIdentityScannerProfilePagedResult>.Ok(
                result,
                "Data identity scanner profile berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<IdentityScannerProfileOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Identity Scanner Profile", Description = "Melihat data pilihan identity scanner profile", AccessType = AccessTypes.Read, SortOrder = 1)]
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

            var query = _dbContext.Set<MstIdentityScannerProfile>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (useOnlyActive)
                query = query.Where(x => x.IsActive);

            if (profileType.HasValue)
                query = query.Where(x => x.ProfileType == profileType.Value);

            if (isForIdentityCard.HasValue)
                query = query.Where(x => x.IsForIdentityCard == isForIdentityCard.Value);

            if (isForPatientCard.HasValue)
                query = query.Where(x => x.IsForPatientCard == isForPatientCard.Value);

            if (isForMembershipCard.HasValue)
                query = query.Where(x => x.IsForMembershipCard == isForMembershipCard.Value);

            if (isForInsuranceCard.HasValue)
                query = query.Where(x => x.IsForInsuranceCard == isForInsuranceCard.Value);

            if (isOcrEnabled.HasValue)
                query = query.Where(x => x.IsOcrEnabled == isOcrEnabled.Value);

            if (isBarcodeEnabled.HasValue)
                query = query.Where(x => x.IsBarcodeEnabled == isBarcodeEnabled.Value);

            if (isQrEnabled.HasValue)
                query = query.Where(x => x.IsQrEnabled == isQrEnabled.Value);

            if (isManualInputAllowed.HasValue)
                query = query.Where(x => x.IsManualInputAllowed == isManualInputAllowed.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ProfileCode.ToLower().Contains(keyword) ||
                    x.ProfileName.ToLower().Contains(keyword) ||
                    (x.ScannerVendorName != null && x.ScannerVendorName.ToLower().Contains(keyword)) ||
                    (x.ScannerModel != null && x.ScannerModel.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ProfileName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new IdentityScannerProfileOptionResponse
                {
                    Id = x.Id,
                    ProfileCode = x.ProfileCode,
                    ProfileName = x.ProfileName,
                    ProfileType = x.ProfileType,
                    IsForIdentityCard = x.IsForIdentityCard,
                    IsForPatientCard = x.IsForPatientCard,
                    IsForMembershipCard = x.IsForMembershipCard,
                    IsForInsuranceCard = x.IsForInsuranceCard,
                    IsOcrEnabled = x.IsOcrEnabled,
                    IsBarcodeEnabled = x.IsBarcodeEnabled,
                    IsQrEnabled = x.IsQrEnabled,
                    IsManualInputAllowed = x.IsManualInputAllowed
                })
                .ToListAsync();

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
        [AccessAction("Read", "Read Identity Scanner Profile", Description = "Melihat detail identity scanner profile", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("IdentityScannerProfile", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _dbContext.Set<MstIdentityScannerProfile>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new IdentityScannerProfileDetailResponse
                {
                    Id = x.Id,
                    ProfileCode = x.ProfileCode,
                    ProfileName = x.ProfileName,
                    ProfileType = x.ProfileType,
                    ScannerVendorName = x.ScannerVendorName,
                    ScannerModel = x.ScannerModel,
                    InputFormat = x.InputFormat,
                    OutputFormat = x.OutputFormat,
                    IdentityNumberRegex = x.IdentityNumberRegex,
                    MemberNumberRegex = x.MemberNumberRegex,
                    CardNumberRegex = x.CardNumberRegex,
                    IdentityNumberFieldName = x.IdentityNumberFieldName,
                    FullNameFieldName = x.FullNameFieldName,
                    BirthDateFieldName = x.BirthDateFieldName,
                    GenderFieldName = x.GenderFieldName,
                    AddressFieldName = x.AddressFieldName,
                    IsForIdentityCard = x.IsForIdentityCard,
                    IsForPatientCard = x.IsForPatientCard,
                    IsForMembershipCard = x.IsForMembershipCard,
                    IsForInsuranceCard = x.IsForInsuranceCard,
                    IsOcrEnabled = x.IsOcrEnabled,
                    IsBarcodeEnabled = x.IsBarcodeEnabled,
                    IsQrEnabled = x.IsQrEnabled,
                    IsManualInputAllowed = x.IsManualInputAllowed,
                    IsAutoCreatePatientAllowed = x.IsAutoCreatePatientAllowed,
                    IsVerificationRequired = x.IsVerificationRequired,
                    SortOrder = x.SortOrder,
                    ConfigurationJson = x.ConfigurationJson,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Identity scanner profile tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<IdentityScannerProfileDetailResponse>.Ok(
                result,
                "Detail identity scanner profile berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<IdentityScannerProfileCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Identity Scanner Profile", Description = "Membuat data identity scanner profile", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("IdentityScannerProfile", "Create")]
        public async Task<IActionResult> CreateIdentityScannerProfile([FromBody] CreateIdentityScannerProfileRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                profileCode: request.ProfileCode,
                profileName: request.ProfileName
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
                ProfileCode = request.ProfileCode.Trim().ToUpperInvariant(),
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

            var response = new IdentityScannerProfileCreateResponse
            {
                Id = entity.Id,
                ProfileCode = entity.ProfileCode,
                ProfileName = entity.ProfileName,
                ProfileType = entity.ProfileType,
                IsForIdentityCard = entity.IsForIdentityCard,
                IsForPatientCard = entity.IsForPatientCard,
                IsForMembershipCard = entity.IsForMembershipCard,
                IsForInsuranceCard = entity.IsForInsuranceCard,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<IdentityScannerProfileCreateResponse>.Ok(
                response,
                "Identity scanner profile berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Identity Scanner Profile", Description = "Mengubah data identity scanner profile", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("IdentityScannerProfile", "Update")]
        public async Task<IActionResult> UpdateIdentityScannerProfile(Guid id, [FromBody] UpdateIdentityScannerProfileRequest request)
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
                profileCode: request.ProfileCode,
                profileName: request.ProfileName
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data identity scanner profile tidak valid."
                ));
            }

            entity.ProfileCode = request.ProfileCode.Trim().ToUpperInvariant();
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
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Identity scanner profile berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/activate")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Identity Scanner Profile", Description = "Mengaktifkan data identity scanner profile", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("IdentityScannerProfile", "Update")]
        public async Task<IActionResult> ActivateIdentityScannerProfile(Guid id)
        {
            return await SetActiveStatusAsync(id, true, "Identity scanner profile berhasil diaktifkan.");
        }

        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Identity Scanner Profile", Description = "Menonaktifkan data identity scanner profile", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("IdentityScannerProfile", "Update")]
        public async Task<IActionResult> DeactivateIdentityScannerProfile(Guid id)
        {
            return await SetActiveStatusAsync(id, false, "Identity scanner profile berhasil dinonaktifkan.");
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Identity Scanner Profile", Description = "Menghapus data identity scanner profile", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("IdentityScannerProfile", "Delete")]
        public async Task<IActionResult> DeleteIdentityScannerProfile(Guid id)
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

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Identity scanner profile berhasil dihapus."
            ));
        }

        private async Task<IActionResult> SetActiveStatusAsync(Guid id, bool isActive, string successMessage)
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

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                successMessage
            ));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string profileCode,
            string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileCode))
                return (false, "Kode identity scanner profile wajib diisi.");

            if (string.IsNullOrWhiteSpace(profileName))
                return (false, "Nama identity scanner profile wajib diisi.");

            var normalizedCode = profileCode.Trim().ToUpperInvariant();
            var normalizedName = profileName.Trim().ToLower();

            var duplicateCode = await _dbContext.Set<MstIdentityScannerProfile>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.ProfileCode.ToUpper() == normalizedCode &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateCode)
                return (false, "Kode identity scanner profile sudah digunakan.");

            var duplicateName = await _dbContext.Set<MstIdentityScannerProfile>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.ProfileName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateName)
                return (false, "Nama identity scanner profile sudah digunakan.");

            return (true, null);
        }

        private static IQueryable<MstIdentityScannerProfile> ApplySorting(
            IQueryable<MstIdentityScannerProfile> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "profilecode" => isDesc
                    ? query.OrderByDescending(x => x.ProfileCode)
                    : query.OrderBy(x => x.ProfileCode),

                "profilename" => isDesc
                    ? query.OrderByDescending(x => x.ProfileName)
                    : query.OrderBy(x => x.ProfileName),

                "profiletype" => isDesc
                    ? query.OrderByDescending(x => x.ProfileType)
                    : query.OrderBy(x => x.ProfileType),

                "scannervendorname" => isDesc
                    ? query.OrderByDescending(x => x.ScannerVendorName)
                    : query.OrderBy(x => x.ScannerVendorName),

                "scannermodel" => isDesc
                    ? query.OrderByDescending(x => x.ScannerModel)
                    : query.OrderBy(x => x.ScannerModel),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.ProfileName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.ProfileName)
            };
        }

        private static IdentityScannerProfileResponse ToResponse(MstIdentityScannerProfile x)
        {
            return new IdentityScannerProfileResponse
            {
                Id = x.Id,
                ProfileCode = x.ProfileCode,
                ProfileName = x.ProfileName,
                ProfileType = x.ProfileType,
                ScannerVendorName = x.ScannerVendorName,
                ScannerModel = x.ScannerModel,
                InputFormat = x.InputFormat,
                OutputFormat = x.OutputFormat,
                IsForIdentityCard = x.IsForIdentityCard,
                IsForPatientCard = x.IsForPatientCard,
                IsForMembershipCard = x.IsForMembershipCard,
                IsForInsuranceCard = x.IsForInsuranceCard,
                IsOcrEnabled = x.IsOcrEnabled,
                IsBarcodeEnabled = x.IsBarcodeEnabled,
                IsQrEnabled = x.IsQrEnabled,
                IsManualInputAllowed = x.IsManualInputAllowed,
                IsAutoCreatePatientAllowed = x.IsAutoCreatePatientAllowed,
                IsVerificationRequired = x.IsVerificationRequired,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
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

        private static string SplitPascalCase(string value)
        {
            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private Guid GetCurrentUserId()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

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
    }
}