using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Attendance.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Globalization;
using System.Security.Claims;

using ResponseExternalUserPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs.ExternalUserResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/external-users")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "External User",
        AreaName = "Corporate",
        ControllerName = "ExternalUser",
        Description = "Corporate human resource master data external user",
        SortOrder = 6
    )]
    [Tags("Corporate / Human Resource / Master Data / External User")]
    public class ExternalUserController : ControllerBase
    {
        private const string DefaultUserProfilePhotoPathFallback = "/uploads/default-profile-photos/user.png";
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string HospitalCode = "RSMMC";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public ExternalUserController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _userManager = userManager;
            _configuration = configuration;
        }

        private static List<ExternalUserDetailTabMetadataResponse> BuildExternalUserDetailTabs()
        {
            return new List<ExternalUserDetailTabMetadataResponse>
            {
                new()
                {
                    Key = "organization",
                    Label = "Organization",
                    Icon = "organization",
                    Endpoint = "/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/organizations",
                    IsVisibleInDetail = true,
                    IsVisibleInUpdate = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = false,
                    SortOrder = 1
                },
                new()
                {
                    Key = "documents",
                    Label = "Documents",
                    Icon = "document",
                    Endpoint = "/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/documents",
                    IsVisibleInDetail = true,
                    IsVisibleInUpdate = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true,
                    SortOrder = 2
                },
                new()
                {
                    Key = "certification",
                    Label = "Certification",
                    Icon = "certification",
                    Endpoint = "/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/certifications",
                    IsVisibleInDetail = true,
                    IsVisibleInUpdate = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true,
                    SortOrder = 3
                },
                new()
                {
                    Key = "credentialLicense",
                    Label = "Credential License",
                    Icon = "license",
                    Endpoint = "/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/credential-licenses",
                    IsVisibleInDetail = true,
                    IsVisibleInUpdate = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true,
                    SortOrder = 4
                },
                new()
                {
                    Key = "bank",
                    Label = "Bank",
                    Icon = "bank",
                    Endpoint = "/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/bank-accounts",
                    IsVisibleInDetail = true,
                    IsVisibleInUpdate = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true,
                    SortOrder = 5
                },
                new()
                {
                    Key = "schedule",
                    Label = "Schedule",
                    Icon = "schedule",
                    Endpoint = "/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/work-schedules",
                    IsVisibleInDetail = true,
                    IsVisibleInUpdate = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true,
                    SortOrder = 6
                },
                new()
                {
                    Key = "attendance",
                    Label = "Attendance",
                    Icon = "attendance",
                    Endpoint = "/api/v1/corporate/human-resource/master-data/external-users/{externalUserId}/attendance",
                    IsVisibleInDetail = true,
                    IsVisibleInUpdate = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false,
                    SortOrder = 7
                },
                new()
                {
                    Key = "disciplinaryActions",
                    Label = "Disciplinary Actions",
                    Icon = "warning",
                    Endpoint = "/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/disciplinary-actions",
                    IsVisibleInDetail = true,
                    IsVisibleInUpdate = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true,
                    SortOrder = 8
                },
                new()
                {
                    Key = "complianceAlerts",
                    Label = "Compliance Alerts",
                    Icon = "alert",
                    Endpoint = "/api/v1/corporate/human-resource/workforce/compliance-alerts?workforceProfileId={workforceProfileId}",
                    IsVisibleInDetail = true,
                    IsVisibleInUpdate = false,
                    CanCreate = false,
                    CanUpdate = true,
                    CanDelete = false,
                    SortOrder = 9
                },
                new()
                {
                    Key = "scheduleChangeRequests",
                    Label = "Schedule Change Requests",
                    Icon = "calendar-edit",
                    Endpoint = "/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/schedule-change-requests",
                    IsVisibleInDetail = true,
                    IsVisibleInUpdate = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true,
                    SortOrder = 10
                },
                new()
                {
                    Key = "shiftSwapRequests",
                    Label = "Shift Swap Requests",
                    Icon = "swap",
                    Endpoint = "/api/v1/corporate/human-resource/workforce/shift-swap-requests?workforceProfileId={workforceProfileId}",
                    IsVisibleInDetail = true,
                    IsVisibleInUpdate = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true,
                    SortOrder = 11
                }
            }
            .OrderBy(x => x.SortOrder)
            .ToList();
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<ExternalUserFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read External User",
            Description = "Melihat data external user",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("ExternalUser", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new ExternalUserFilterMetadataResponse
            {
                DefaultFilter = new ExternalUserDefaultFilterResponse
                {
                    StartDate = null,
                    EndDate = null,
                    Period = null,
                    DepartmentId = null,
                    PositionId = null,
                    IsActive = null,
                    Search = null,
                    SortBy = "createDateTime",
                    SortDirection = "desc",
                    PageNumber = 1,
                    PageSize = 25
                },
                Periods = BuildCustomPeriodOptions(),
                SortOptions = new List<ExternalUserSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "externalCode", Label = "Kode external user" },
                    new() { Value = "fullName", Label = "Nama external user" },
                    new() { Value = "companyName", Label = "Nama company" },
                    new() { Value = "departmentName", Label = "Nama department" },
                    new() { Value = "positionName", Label = "Nama position" },
                    new() { Value = "accessEndDate", Label = "Tanggal akhir akses" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ExternalUserTypeOptions = BuildEnumOptions<ExternalUserType>(),
                ExternalUserStatusOptions = BuildEnumOptions<ExternalUserStatus>(),
                EngagementTypeOptions = BuildEnumOptions<ExternalEngagementType>(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateExternalUserFieldMetadata(),
                UpdateFields = BuildUpdateExternalUserFieldMetadata(),
                DetailTabs = BuildExternalUserDetailTabs()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "ExternalUser.GetFilterMetadata",
                "Mengambil metadata filter external user.",
                result
            );

            return Ok(ApiResponse<ExternalUserFilterMetadataResponse>.Ok(
                result,
                "Metadata filter external user berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<ExternalUserSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read External User",
            Description = "Melihat data external user",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("ExternalUser", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var now = DateTime.UtcNow;

            var externalUserQuery = _dbContext.Set<MstExternalUser>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new ExternalUserSummaryResponse
            {
                TotalExternalUser = await externalUserQuery.CountAsync(),
                ActiveExternalUser = await externalUserQuery.CountAsync(x => x.IsActive),
                InactiveExternalUser = await externalUserQuery.CountAsync(x => !x.IsActive),
                PendingApprovalExternalUser = await externalUserQuery.CountAsync(x => x.ExternalUserStatus == ExternalUserStatus.PendingApproval),
                SuspendedExternalUser = await externalUserQuery.CountAsync(x => x.ExternalUserStatus == ExternalUserStatus.Suspended),
                AccessExpiredExternalUser = await externalUserQuery.CountAsync(x =>
                    x.ExternalUserStatus == ExternalUserStatus.AccessExpired ||
                    (x.AccessEndDate.HasValue && x.AccessEndDate.Value < now)),
                ExternalUserWithLoginAccount = await externalUserQuery.CountAsync(x =>
                    _dbContext.Users.Any(u =>
                        u.ExternalUserId == x.Id &&
                        u.UserType == UserType.ExternalUser))
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "ExternalUser.GetSummary",
                "Mengambil ringkasan data external user.",
                result
            );

            return Ok(ApiResponse<ExternalUserSummaryResponse>.Ok(
                result,
                "Ringkasan external user berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseExternalUserPagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read External User",
            Description = "Melihat data external user",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("ExternalUser", "Read")]
        public async Task<IActionResult> GetExternalUsers(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? period,
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? positionId,
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

            var dateRange = ResolveDateRange(startDate, endDate, period);

            if (!dateRange.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    dateRange.ErrorMessage ?? "Filter tanggal tidak valid."
                ));
            }

            var query = _dbContext.Set<MstExternalUser>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (dateRange.Start.HasValue)
            {
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            }

            if (dateRange.EndExclusive.HasValue)
            {
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            }

            if (departmentId.HasValue && departmentId.Value != Guid.Empty)
            {
                query = query.Where(x => x.PrimaryDepartmentId == departmentId.Value);
            }

            if (positionId.HasValue && positionId.Value != Guid.Empty)
            {
                query = query.Where(x => x.PrimaryPositionId == positionId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ExternalCode.ToLower().Contains(keyword) ||
                    x.FullName.ToLower().Contains(keyword) ||
                    (x.CompanyName != null && x.CompanyName.ToLower().Contains(keyword)) ||
                    (x.CompanyCode != null && x.CompanyCode.ToLower().Contains(keyword)) ||
                    (x.JobTitle != null && x.JobTitle.ToLower().Contains(keyword)) ||
                    (x.ContactPersonName != null && x.ContactPersonName.ToLower().Contains(keyword)) ||
                    (x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(keyword)) ||
                    (x.WhatsAppNumber != null && x.WhatsAppNumber.ToLower().Contains(keyword)) ||
                    (x.Email != null && x.Email.ToLower().Contains(keyword)) ||
                    (x.IdentityNumber != null && x.IdentityNumber.ToLower().Contains(keyword)) ||
                    (x.TaxNumber != null && x.TaxNumber.ToLower().Contains(keyword)) ||
                    (x.BusinessLicenseNumber != null && x.BusinessLicenseNumber.ToLower().Contains(keyword)) ||
                    (x.PrimaryDepartment != null && x.PrimaryDepartment.DepartmentCode.ToLower().Contains(keyword)) ||
                    (x.PrimaryDepartment != null && x.PrimaryDepartment.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.PrimaryPosition != null && x.PrimaryPosition.PositionCode.ToLower().Contains(keyword)) ||
                    (x.PrimaryPosition != null && x.PrimaryPosition.PositionName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();
            var defaultExternalUserProfilePhotoPath = GetDefaultUserProfilePhotoPath();

            var items = await ApplyExternalUserSorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ExternalUserResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ExternalCode = x.ExternalCode,
                    ExternalUserType = x.ExternalUserType,
                    ExternalUserStatus = x.ExternalUserStatus,
                    EngagementType = x.EngagementType,
                    FullName = x.FullName,
                    ProfilePhotoPath = _dbContext.Users
                        .Where(u =>
                            u.ExternalUserId == x.Id &&
                            u.UserType == UserType.ExternalUser)
                        .OrderByDescending(u => u.IsActive)
                        .ThenByDescending(u => u.UpdateDateTime ?? u.CreateDateTime)
                        .Select(u => u.ProfilePhotoPath)
                        .FirstOrDefault() ?? defaultExternalUserProfilePhotoPath,
                    CompanyName = x.CompanyName,
                    CompanyCode = x.CompanyCode,
                    JobTitle = x.JobTitle,
                    ContactPersonName = x.ContactPersonName,
                    PhoneNumber = x.PhoneNumber,
                    WhatsAppNumber = x.WhatsAppNumber,
                    Email = x.Email,

                    CountryId = x.CountryId,
                    CountryName = x.Country != null ? x.Country.CountryName : null,
                    ProvinceId = x.ProvinceId,
                    ProvinceName = x.Province != null ? x.Province.ProvinceName : null,
                    CityId = x.CityId,
                    CityName = x.City != null ? x.City.CityName : null,
                    DistrictId = x.DistrictId,
                    DistrictName = x.District != null ? x.District.DistrictName : null,
                    PostalCodeId = x.PostalCodeId,
                    PostalCode = x.PostalCode != null ? x.PostalCode.PostalCode : null,
                    VillageName = x.PostalCode != null ? x.PostalCode.VillageName : null,

                    PrimaryDepartmentId = x.PrimaryDepartmentId,
                    PrimaryDepartmentCode = x.PrimaryDepartment != null ? x.PrimaryDepartment.DepartmentCode : string.Empty,
                    PrimaryDepartmentName = x.PrimaryDepartment != null ? x.PrimaryDepartment.DepartmentName : string.Empty,
                    PrimaryPositionId = x.PrimaryPositionId,
                    PrimaryPositionCode = x.PrimaryPosition != null ? x.PrimaryPosition.PositionCode : string.Empty,
                    PrimaryPositionName = x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : string.Empty,

                    WorkLocation = x.WorkLocation,
                    ContractStartDate = x.ContractStartDate,
                    ContractEndDate = x.ContractEndDate,
                    AccessStartDate = x.AccessStartDate,
                    AccessEndDate = x.AccessEndDate,
                    AccessPurpose = x.AccessPurpose,
                    HasUserAccount = _dbContext.Users.Any(u =>
                        u.ExternalUserId == x.Id &&
                        u.UserType == UserType.ExternalUser),
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : (Guid?)x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.CreateBy)
                            .Select(u =>
                                u.DisplayName ??
                                u.UserName ??
                                u.Email ??
                                u.UserCode)
                            .FirstOrDefault()
                })
                .ToListAsync();

            foreach (var item in items)
            {
                EnrichExternalUserPhotoFields(item);
            }

            var result = new ResponseExternalUserPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "ExternalUser.GetExternalUsers",
                "Mengambil data external user.",
                new
                {
                    startDate,
                    endDate,
                    period,
                    AppliedStartDate = dateRange.Start,
                    AppliedEndExclusive = dateRange.EndExclusive,
                    departmentId,
                    positionId,
                    isActive,
                    search,
                    sortBy,
                    sortDirection,
                    pageNumber,
                    pageSize,
                    totalData
                }
            );

            return Ok(ApiResponse<ResponseExternalUserPagedResult>.Ok(
                result,
                "Data external user berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ExternalUserOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read External User",
            Description = "Melihat pilihan external user",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("ExternalUser", "Read")]
        public async Task<IActionResult> GetExternalUserOptions(
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? positionId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstExternalUser>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
            {
                query = query.Where(x => x.IsActive);
            }

            if (departmentId.HasValue && departmentId.Value != Guid.Empty)
            {
                query = query.Where(x => x.PrimaryDepartmentId == departmentId.Value);
            }

            if (positionId.HasValue && positionId.Value != Guid.Empty)
            {
                query = query.Where(x => x.PrimaryPositionId == positionId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ExternalCode.ToLower().Contains(keyword) ||
                    x.FullName.ToLower().Contains(keyword) ||
                    (x.CompanyName != null && x.CompanyName.ToLower().Contains(keyword)) ||
                    (x.CompanyCode != null && x.CompanyCode.ToLower().Contains(keyword)) ||
                    (x.JobTitle != null && x.JobTitle.ToLower().Contains(keyword)) ||
                    (x.Email != null && x.Email.ToLower().Contains(keyword)) ||
                    (x.PrimaryDepartment != null && x.PrimaryDepartment.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.PrimaryPosition != null && x.PrimaryPosition.PositionName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();
            var defaultExternalUserProfilePhotoPath = GetDefaultUserProfilePhotoPath();

            var items = await query
                .OrderBy(x => x.FullName)
                .ThenBy(x => x.ExternalCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ExternalUserOptionResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ExternalCode = x.ExternalCode,
                    ExternalUserType = x.ExternalUserType,
                    ExternalUserStatus = x.ExternalUserStatus,
                    FullName = x.FullName,
                    ProfilePhotoPath = _dbContext.Users
                        .Where(u =>
                            u.ExternalUserId == x.Id &&
                            u.UserType == UserType.ExternalUser)
                        .OrderByDescending(u => u.IsActive)
                        .ThenByDescending(u => u.UpdateDateTime ?? u.CreateDateTime)
                        .Select(u => u.ProfilePhotoPath)
                        .FirstOrDefault() ?? defaultExternalUserProfilePhotoPath,
                    CompanyName = x.CompanyName,
                    JobTitle = x.JobTitle,
                    PrimaryDepartmentId = x.PrimaryDepartmentId,
                    PrimaryDepartmentName = x.PrimaryDepartment != null ? x.PrimaryDepartment.DepartmentName : string.Empty,
                    PrimaryPositionId = x.PrimaryPositionId,
                    PrimaryPositionName = x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : string.Empty
                })
                .ToListAsync();

            foreach (var item in items)
            {
                EnrichExternalUserPhotoFields(item);
            }

            var result = new PagedResult<ExternalUserOptionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PagedResult<ExternalUserOptionResponse>>.Ok(
                result,
                "Data pilihan external user berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ExternalUserDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read External User",
            Description = "Melihat data external user",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("ExternalUser", "Read")]
        public async Task<IActionResult> GetExternalUserById(Guid id)
        {
            var defaultExternalUserProfilePhotoPath = GetDefaultUserProfilePhotoPath();

            var data = await _dbContext.Set<MstExternalUser>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new ExternalUserDetailResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ExternalCode = x.ExternalCode,
                    ExternalUserType = x.ExternalUserType,
                    ExternalUserStatus = x.ExternalUserStatus,
                    EngagementType = x.EngagementType,
                    FullName = x.FullName,
                    ProfilePhotoPath = _dbContext.Users
                        .Where(u =>
                            u.ExternalUserId == x.Id &&
                            u.UserType == UserType.ExternalUser)
                        .OrderByDescending(u => u.IsActive)
                        .ThenByDescending(u => u.UpdateDateTime ?? u.CreateDateTime)
                        .Select(u => u.ProfilePhotoPath)
                        .FirstOrDefault() ?? defaultExternalUserProfilePhotoPath,
                    CompanyName = x.CompanyName,
                    CompanyCode = x.CompanyCode,
                    JobTitle = x.JobTitle,
                    ContactPersonName = x.ContactPersonName,
                    PhoneNumber = x.PhoneNumber,
                    WhatsAppNumber = x.WhatsAppNumber,
                    Email = x.Email,
                    Address = x.Address,
                    IdentityType = x.IdentityType,
                    IdentityNumber = x.IdentityNumber,
                    TaxNumber = x.TaxNumber,
                    BusinessLicenseNumber = x.BusinessLicenseNumber,

                    CountryId = x.CountryId,
                    CountryName = x.Country != null ? x.Country.CountryName : null,
                    ProvinceId = x.ProvinceId,
                    ProvinceName = x.Province != null ? x.Province.ProvinceName : null,
                    CityId = x.CityId,
                    CityName = x.City != null ? x.City.CityName : null,
                    DistrictId = x.DistrictId,
                    DistrictName = x.District != null ? x.District.DistrictName : null,
                    PostalCodeId = x.PostalCodeId,
                    PostalCode = x.PostalCode != null ? x.PostalCode.PostalCode : null,
                    VillageName = x.PostalCode != null ? x.PostalCode.VillageName : null,

                    PrimaryDepartmentId = x.PrimaryDepartmentId,
                    PrimaryDepartmentCode = x.PrimaryDepartment != null ? x.PrimaryDepartment.DepartmentCode : string.Empty,
                    PrimaryDepartmentName = x.PrimaryDepartment != null ? x.PrimaryDepartment.DepartmentName : string.Empty,
                    PrimaryPositionId = x.PrimaryPositionId,
                    PrimaryPositionCode = x.PrimaryPosition != null ? x.PrimaryPosition.PositionCode : string.Empty,
                    PrimaryPositionName = x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : string.Empty,

                    WorkLocation = x.WorkLocation,
                    ContractStartDate = x.ContractStartDate,
                    ContractEndDate = x.ContractEndDate,
                    AccessStartDate = x.AccessStartDate,
                    AccessEndDate = x.AccessEndDate,
                    AccessPurpose = x.AccessPurpose,
                    Description = x.Description,
                    HasUserAccount = _dbContext.Users.Any(u =>
                        u.ExternalUserId == x.Id &&
                        u.UserType == UserType.ExternalUser),
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : (Guid?)x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.CreateBy)
                            .Select(u =>
                                u.DisplayName ??
                                u.UserName ??
                                u.Email ??
                                u.UserCode)
                            .FirstOrDefault(),

                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : (Guid?)x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.UpdateBy)
                            .Select(u =>
                                u.DisplayName ??
                                u.UserName ??
                                u.Email ??
                                u.UserCode)
                            .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "External user tidak ditemukan."
                ));
            }

            EnrichExternalUserPhotoFields(data);
            data.UserAccount = await BuildExternalUserAccountCompactResponseAsync(id);
            EnrichExternalUserAccountPhotoFields(data.UserAccount);
            data.ChildSummary = await BuildExternalUserChildSummaryAsync(id);

            return Ok(ApiResponse<ExternalUserDetailResponse>.Ok(
                data,
                "Detail external user berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ExternalUserCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create External User",
            Description = "Membuat data external user",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("ExternalUser", "Create")]
        public async Task<IActionResult> CreateExternalUser([FromBody] CreateExternalUserRequest request)
        {
            var requiredValidation = ValidateRequiredExternalUserRequest(request);

            if (!requiredValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    requiredValidation.ErrorMessage ?? "Data wajib external user belum lengkap."
                ));
            }

            if (request.CreateLoginAccount && string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Email wajib diisi jika ingin membuat akun login external user."
                ));
            }

            if (!request.CreateLoginAccount && request.IsFingerprintRegistrationEnabled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Fingerprint registration hanya bisa diaktifkan jika akun login dibuat."
                ));
            }

            if (request.IsFingerprintRegistrationEnabled &&
                string.IsNullOrWhiteSpace(request.FingerprintRegistrationReason))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan aktivasi fingerprint wajib diisi."
                ));
            }

            var validation = await ValidateExternalUserRequestAsync(
                excludeExternalUserId: null,
                primaryDepartmentId: request.PrimaryDepartmentId,
                primaryPositionId: request.PrimaryPositionId,
                countryId: request.CountryId,
                provinceId: request.ProvinceId,
                cityId: request.CityId,
                districtId: request.DistrictId,
                postalCodeId: request.PostalCodeId,
                identityNumber: request.IdentityNumber,
                phoneNumber: request.PhoneNumber,
                whatsAppNumber: request.WhatsAppNumber,
                email: request.Email,
                taxNumber: request.TaxNumber,
                businessLicenseNumber: request.BusinessLicenseNumber
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data external user tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var normalizedEmail = NormalizeNullableText(request.Email)?.ToLowerInvariant();
                var normalizedPhone = NormalizeDigitsOnly(request.PhoneNumber);
                var normalizedWhatsApp = NormalizeDigitsOnly(request.WhatsAppNumber);
                var primaryDepartmentId = NormalizeNullableGuid(request.PrimaryDepartmentId);
                var primaryPositionId = NormalizeNullableGuid(request.PrimaryPositionId);

                var accessStartDate = NormalizeNullableDateTimeUtc(request.AccessStartDate);
                var accessEndDate = NormalizeNullableDateTimeUtc(request.AccessEndDate);

                var workforceProfile = new MstWorkforceProfile
                {
                    Id = Guid.NewGuid(),
                    ProfileCode = await GenerateWorkforceProfileCodeAsync(),
                    UserType = UserType.ExternalUser,
                    DisplayName = request.FullName.Trim(),
                    Email = normalizedEmail,
                    PhoneNumber = normalizedPhone,
                    WhatsAppNumber = normalizedWhatsApp,
                    PrimaryDepartmentId = primaryDepartmentId,
                    PrimaryPositionId = primaryPositionId,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<MstWorkforceProfile>().Add(workforceProfile);
                await _dbContext.SaveChangesAsync();

                var entity = new MstExternalUser
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfile.Id,
                    ExternalCode = await GenerateExternalCodeAsync(),
                    ExternalUserType = request.ExternalUserType,
                    ExternalUserStatus = request.ExternalUserStatus,
                    EngagementType = request.EngagementType,
                    FullName = request.FullName.Trim(),
                    CompanyName = NormalizeNullableText(request.CompanyName),
                    CompanyCode = NormalizeNullableText(request.CompanyCode),
                    JobTitle = NormalizeNullableText(request.JobTitle),
                    ContactPersonName = NormalizeNullableText(request.ContactPersonName),
                    PhoneNumber = normalizedPhone,
                    WhatsAppNumber = normalizedWhatsApp,
                    Email = normalizedEmail,
                    Address = NormalizeNullableText(request.Address),
                    CountryId = NormalizeNullableGuid(request.CountryId),
                    ProvinceId = NormalizeNullableGuid(request.ProvinceId),
                    CityId = NormalizeNullableGuid(request.CityId),
                    DistrictId = NormalizeNullableGuid(request.DistrictId),
                    PostalCodeId = NormalizeNullableGuid(request.PostalCodeId),
                    IdentityType = NormalizeNullableText(request.IdentityType),
                    IdentityNumber = NormalizeNullableText(request.IdentityNumber),
                    TaxNumber = NormalizeNullableText(request.TaxNumber),
                    BusinessLicenseNumber = NormalizeNullableText(request.BusinessLicenseNumber),
                    PrimaryDepartmentId = primaryDepartmentId,
                    PrimaryPositionId = primaryPositionId,
                    WorkLocation = NormalizeNullableText(request.WorkLocation),
                    ContractStartDate = request.ContractStartDate?.Date,
                    ContractEndDate = request.ContractEndDate?.Date,
                    AccessStartDate = accessStartDate,
                    AccessEndDate = accessEndDate,
                    AccessPurpose = NormalizeNullableText(request.AccessPurpose),
                    Description = NormalizeNullableText(request.Description),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<MstExternalUser>().Add(entity);
                await _dbContext.SaveChangesAsync();

                if (primaryDepartmentId.HasValue && primaryPositionId.HasValue)
                {
                    var effectiveStartDate = entity.ContractStartDate ?? now.Date;

                    var organizationAssignment = new WfpOrganizationAssignment
                    {
                        Id = Guid.NewGuid(),
                        WorkforceProfileId = workforceProfile.Id,
                        DepartmentId = primaryDepartmentId.Value,
                        PositionId = primaryPositionId.Value,
                        IsPrimary = true,
                        IsActive = true,
                        EffectiveStartDate = effectiveStartDate.Date,
                        EffectiveEndDate = entity.ContractEndDate?.Date,
                        Description = "Initial primary organization assignment",
                        CreateDateTime = now,
                        CreateBy = actorUserId,
                        IsDelete = false,
                        IsCancel = false
                    };

                    _dbContext.Set<WfpOrganizationAssignment>().Add(organizationAssignment);
                    await _dbContext.SaveChangesAsync();
                }

                ExternalUserLoginAccountResponse? loginAccount = null;

                if (request.CreateLoginAccount)
                {
                    var accountResult = await CreateLoginAccountForExternalUserAsync(
                        externalUser: entity,
                        isFingerprintRegistrationEnabled: request.IsFingerprintRegistrationEnabled,
                        fingerprintRegistrationReason: request.FingerprintRegistrationReason,
                        actorUserId: actorUserId
                    );

                    if (!accountResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();

                        return BadRequest(ApiResponse<object>.Fail(
                            StatusCodes.Status400BadRequest,
                            accountResult.ErrorMessage ?? "Akun login external user gagal dibuat."
                        ));
                    }

                    loginAccount = accountResult.Response;

                    if (loginAccount?.UserId.HasValue == true &&
                        primaryDepartmentId.HasValue &&
                        primaryPositionId.HasValue)
                    {
                        await SyncUserPrimaryOrganizationAsync(
                            userId: loginAccount.UserId.Value,
                            departmentId: primaryDepartmentId.Value,
                            positionId: primaryPositionId.Value,
                            effectiveStartDate: entity.ContractStartDate ?? now.Date,
                            actorUserId: actorUserId
                        );

                        await _dbContext.SaveChangesAsync();
                    }
                }

                await transaction.CommitAsync();

                var response = new ExternalUserCreateResponse
                {
                    Id = entity.Id,
                    WorkforceProfileId = entity.WorkforceProfileId,
                    ExternalCode = entity.ExternalCode,
                    ExternalUserType = entity.ExternalUserType,
                    ExternalUserStatus = entity.ExternalUserStatus,
                    FullName = entity.FullName,
                    CompanyName = entity.CompanyName,
                    IsActive = entity.IsActive,
                    LoginAccount = loginAccount
                };

                await _loggerService.InfoAsync(
                    LogCategory,
                    "ExternalUser.CreateExternalUser",
                    "External user berhasil dibuat.",
                    new
                    {
                        entity.Id,
                        entity.WorkforceProfileId,
                        entity.ExternalCode,
                        entity.ExternalUserType,
                        entity.ExternalUserStatus,
                        entity.FullName,
                        entity.CompanyName,
                        entity.PrimaryDepartmentId,
                        entity.PrimaryPositionId,
                        LoginAccountCreated = loginAccount?.IsCreated ?? false,
                        loginAccount?.UserId,
                        loginAccount?.UserCode
                    }
                );

                return Ok(ApiResponse<ExternalUserCreateResponse>.Ok(
                    response,
                    request.CreateLoginAccount
                        ? "External user dan akun login berhasil dibuat."
                        : "External user berhasil dibuat."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "ExternalUser.CreateExternalUser",
                    "Gagal membuat external user.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat external user."
                    )
                );
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update External User",
            Description = "Mengubah data external user",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("ExternalUser", "Update")]
        public async Task<IActionResult> UpdateExternalUser(
            Guid id,
            [FromBody] UpdateExternalUserRequest request)
        {
            var entity = await _dbContext.Set<MstExternalUser>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "External user tidak ditemukan."
                ));
            }

            var requiredValidation = ValidateRequiredExternalUserRequest(request);

            if (!requiredValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    requiredValidation.ErrorMessage ?? "Data wajib external user belum lengkap."
                ));
            }

            var validation = await ValidateExternalUserRequestAsync(
                excludeExternalUserId: id,
                primaryDepartmentId: request.PrimaryDepartmentId,
                primaryPositionId: request.PrimaryPositionId,
                countryId: request.CountryId,
                provinceId: request.ProvinceId,
                cityId: request.CityId,
                districtId: request.DistrictId,
                postalCodeId: request.PostalCodeId,
                identityNumber: request.IdentityNumber,
                phoneNumber: request.PhoneNumber,
                whatsAppNumber: request.WhatsAppNumber,
                email: request.Email,
                taxNumber: request.TaxNumber,
                businessLicenseNumber: request.BusinessLicenseNumber
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data external user tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var normalizedEmail = NormalizeNullableText(request.Email)?.ToLowerInvariant();
            var normalizedPhone = NormalizeDigitsOnly(request.PhoneNumber);
            var normalizedWhatsApp = NormalizeDigitsOnly(request.WhatsAppNumber);
            var primaryDepartmentId = NormalizeNullableGuid(request.PrimaryDepartmentId);
            var primaryPositionId = NormalizeNullableGuid(request.PrimaryPositionId);

            entity.ExternalUserType = request.ExternalUserType;
            entity.ExternalUserStatus = request.ExternalUserStatus;
            entity.EngagementType = request.EngagementType;
            entity.FullName = request.FullName.Trim();
            entity.CompanyName = NormalizeNullableText(request.CompanyName);
            entity.CompanyCode = NormalizeNullableText(request.CompanyCode);
            entity.JobTitle = NormalizeNullableText(request.JobTitle);
            entity.ContactPersonName = NormalizeNullableText(request.ContactPersonName);
            entity.PhoneNumber = normalizedPhone;
            entity.WhatsAppNumber = normalizedWhatsApp;
            entity.Email = normalizedEmail;
            entity.Address = NormalizeNullableText(request.Address);
            entity.CountryId = NormalizeNullableGuid(request.CountryId);
            entity.ProvinceId = NormalizeNullableGuid(request.ProvinceId);
            entity.CityId = NormalizeNullableGuid(request.CityId);
            entity.DistrictId = NormalizeNullableGuid(request.DistrictId);
            entity.PostalCodeId = NormalizeNullableGuid(request.PostalCodeId);
            entity.IdentityType = NormalizeNullableText(request.IdentityType);
            entity.IdentityNumber = NormalizeNullableText(request.IdentityNumber);
            entity.TaxNumber = NormalizeNullableText(request.TaxNumber);
            entity.BusinessLicenseNumber = NormalizeNullableText(request.BusinessLicenseNumber);
            entity.PrimaryDepartmentId = primaryDepartmentId;
            entity.PrimaryPositionId = primaryPositionId;
            entity.WorkLocation = NormalizeNullableText(request.WorkLocation);
            entity.ContractStartDate = request.ContractStartDate?.Date;
            entity.ContractEndDate = request.ContractEndDate?.Date;
            entity.AccessStartDate = NormalizeNullableDateTimeUtc(request.AccessStartDate);
            entity.AccessEndDate = NormalizeNullableDateTimeUtc(request.AccessEndDate);
            entity.AccessPurpose = NormalizeNullableText(request.AccessPurpose);
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (entity.WorkforceProfileId != Guid.Empty)
            {
                var workforceProfile = await _dbContext.Set<MstWorkforceProfile>()
                    .FirstOrDefaultAsync(x => x.Id == entity.WorkforceProfileId && !x.IsDelete);

                if (workforceProfile != null)
                {
                    workforceProfile.DisplayName = entity.FullName;
                    workforceProfile.Email = entity.Email;
                    workforceProfile.PhoneNumber = entity.PhoneNumber;
                    workforceProfile.WhatsAppNumber = entity.WhatsAppNumber;
                    workforceProfile.PrimaryDepartmentId = entity.PrimaryDepartmentId;
                    workforceProfile.PrimaryPositionId = entity.PrimaryPositionId;
                    workforceProfile.IsActive = entity.IsActive;
                    workforceProfile.UpdateDateTime = now;
                    workforceProfile.UpdateBy = actorUserId;
                }
            }

            var linkedUser = await _dbContext.Users.FirstOrDefaultAsync(x =>
                x.ExternalUserId == entity.Id &&
                x.UserType == UserType.ExternalUser);

            if (linkedUser != null)
            {
                linkedUser.DisplayName = entity.FullName;
                linkedUser.Email = entity.Email;

                if (!string.IsNullOrWhiteSpace(entity.Email))
                {
                    linkedUser.UserName = entity.Email;
                    linkedUser.NormalizedUserName = _userManager.NormalizeName(entity.Email);
                    linkedUser.NormalizedEmail = _userManager.NormalizeEmail(entity.Email);
                }

                linkedUser.PrimaryDepartmentId = entity.PrimaryDepartmentId;
                linkedUser.PrimaryPositionId = entity.PrimaryPositionId;
                linkedUser.WorkforceProfileId = entity.WorkforceProfileId;
                linkedUser.AccessValidUntil = entity.AccessEndDate;
                linkedUser.IsActive = entity.IsActive;
                linkedUser.UpdateDateTime = now;
            }

            await EnsureWorkforcePrimaryOrganizationAssignmentAsync(entity, now, actorUserId);

            if (linkedUser != null &&
                entity.PrimaryDepartmentId.HasValue &&
                entity.PrimaryPositionId.HasValue)
            {
                await SyncUserPrimaryOrganizationAsync(
                    userId: linkedUser.Id,
                    departmentId: entity.PrimaryDepartmentId.Value,
                    positionId: entity.PrimaryPositionId.Value,
                    effectiveStartDate: entity.ContractStartDate ?? now.Date,
                    actorUserId: actorUserId
                );
            }

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "ExternalUser.UpdateExternalUser",
                "External user berhasil diperbarui.",
                new
                {
                    entity.Id,
                    entity.WorkforceProfileId,
                    entity.ExternalCode,
                    entity.ExternalUserType,
                    entity.ExternalUserStatus,
                    entity.FullName,
                    entity.CompanyName,
                    entity.IsActive
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "External user berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update External User",
            Description = "Mengubah status external user",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("ExternalUser", "Update")]
        public async Task<IActionResult> UpdateExternalUserStatus(
            Guid id,
            [FromBody] UpdateExternalUserStatusRequest request)
        {
            var entity = await _dbContext.Set<MstExternalUser>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "External user tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.ExternalUserStatus = request.ExternalUserStatus;
            entity.AccessEndDate = NormalizeNullableDateTimeUtc(request.AccessEndDate);
            entity.Description = NormalizeNullableText(request.Description) ?? entity.Description;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (entity.WorkforceProfileId != Guid.Empty)
            {
                var workforceProfile = await _dbContext.Set<MstWorkforceProfile>()
                    .FirstOrDefaultAsync(x => x.Id == entity.WorkforceProfileId && !x.IsDelete);

                if (workforceProfile != null)
                {
                    workforceProfile.IsActive = request.IsActive;
                    workforceProfile.UpdateDateTime = now;
                    workforceProfile.UpdateBy = actorUserId;
                }
            }

            var linkedUser = await _dbContext.Users.FirstOrDefaultAsync(x =>
                x.ExternalUserId == id &&
                x.UserType == UserType.ExternalUser);

            if (linkedUser != null)
            {
                linkedUser.IsActive = request.IsActive;
                linkedUser.AccessValidUntil = entity.AccessEndDate;
                linkedUser.UpdateDateTime = now;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status external user berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/user-account/geolocation-bypass")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
    "Update",
    "Update External User",
    Description = "Mengubah pengaturan geolocation bypass akun external user",
    AccessType = AccessTypes.Update,
    SortOrder = 3
)]
        [AccessPermission("ExternalUser", "Update")]
        public async Task<IActionResult> UpdateExternalUserAccountGeolocationBypass(
    Guid id,
    [FromBody] UpdateExternalUserAccountGeolocationBypassRequest request)
        {
            var externalUser = await _dbContext.Set<MstExternalUser>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.ExternalCode,
                    x.FullName,
                    x.AccessEndDate,
                    x.IsActive
                })
                .FirstOrDefaultAsync();

            if (externalUser == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "External user tidak ditemukan."
                ));
            }

            var linkedUser = await _dbContext.Users.FirstOrDefaultAsync(x =>
                x.ExternalUserId == id &&
                x.UserType == UserType.ExternalUser);

            if (linkedUser == null)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "External user belum memiliki akun login, sehingga geolocation bypass tidak dapat diubah."
                ));
            }

            if (request.IsGeolocationBypassEnabled &&
                request.GeolocationBypassUntil.HasValue &&
                request.GeolocationBypassUntil.Value <= DateTime.UtcNow)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Batas waktu geolocation bypass harus lebih besar dari waktu sekarang."
                ));
            }

            if (request.IsGeolocationBypassEnabled &&
                linkedUser.AccessValidUntil.HasValue &&
                request.GeolocationBypassUntil.HasValue &&
                request.GeolocationBypassUntil.Value > linkedUser.AccessValidUntil.Value)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Batas waktu geolocation bypass tidak boleh melebihi batas akhir akses external user."
                ));
            }

            if (request.IsGeolocationBypassEnabled &&
                string.IsNullOrWhiteSpace(request.GeolocationBypassReason))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan geolocation bypass wajib diisi."
                ));
            }

            var now = DateTime.UtcNow;

            linkedUser.IsGeolocationBypassEnabled = request.IsGeolocationBypassEnabled;

            linkedUser.GeolocationBypassUntil = request.IsGeolocationBypassEnabled
                ? request.GeolocationBypassUntil
                : null;

            linkedUser.GeolocationBypassReason = request.IsGeolocationBypassEnabled
                ? NormalizeNullableText(request.GeolocationBypassReason)
                : null;

            linkedUser.UpdateDateTime = now;

            var updateResult = await _userManager.UpdateAsync(linkedUser);

            if (!updateResult.Succeeded)
            {
                var errors = string.Join("; ", updateResult.Errors.Select(x => x.Description));

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Pengaturan geolocation bypass gagal diperbarui: {errors}"
                ));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "ExternalUser.UpdateExternalUserAccountGeolocationBypass",
                "Pengaturan geolocation bypass akun external user berhasil diperbarui.",
                new
                {
                    ExternalUserId = id,
                    externalUser.ExternalCode,
                    externalUser.FullName,
                    UserId = linkedUser.Id,
                    linkedUser.UserName,
                    linkedUser.Email,
                    linkedUser.IsGeolocationBypassEnabled,
                    linkedUser.GeolocationBypassUntil,
                    linkedUser.GeolocationBypassReason,
                    linkedUser.AccessValidUntil
                }
            );

            return Ok(ApiResponse<object>.Ok(
                new
                {
                    ExternalUserId = id,
                    UserId = linkedUser.Id,
                    linkedUser.IsGeolocationBypassEnabled,
                    linkedUser.GeolocationBypassUntil,
                    linkedUser.GeolocationBypassReason,
                    linkedUser.AccessValidUntil
                },
                request.IsGeolocationBypassEnabled
                    ? "Geolocation bypass akun external user berhasil diaktifkan."
                    : "Geolocation bypass akun external user berhasil dinonaktifkan."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete External User",
            Description = "Menghapus data external user",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("ExternalUser", "Delete")]
        public async Task<IActionResult> DeleteExternalUser(Guid id)
        {
            var entity = await _dbContext.Set<MstExternalUser>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "External user tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.ExternalUserStatus = ExternalUserStatus.Inactive;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;

            if (entity.WorkforceProfileId != Guid.Empty)
            {
                var workforceProfile = await _dbContext.Set<MstWorkforceProfile>()
                    .FirstOrDefaultAsync(x => x.Id == entity.WorkforceProfileId && !x.IsDelete);

                if (workforceProfile != null)
                {
                    workforceProfile.IsActive = false;
                    workforceProfile.UpdateDateTime = now;
                    workforceProfile.UpdateBy = actorUserId;
                }

                var organizationAssignments = await _dbContext.Set<WfpOrganizationAssignment>()
                    .Where(x =>
                        x.WorkforceProfileId == entity.WorkforceProfileId &&
                        !x.IsDelete)
                    .ToListAsync();

                foreach (var item in organizationAssignments)
                {
                    item.IsActive = false;
                    item.IsDelete = true;
                    item.DeleteDateTime = now;
                    item.DeleteBy = actorUserId;
                }
            }

            var linkedUser = await _dbContext.Users.FirstOrDefaultAsync(x =>
                x.ExternalUserId == id &&
                x.UserType == UserType.ExternalUser);

            if (linkedUser != null)
            {
                linkedUser.IsActive = false;
                linkedUser.UpdateDateTime = now;

                var userOrganizations = await _dbContext.ApplicationUserOrganizations
                    .Where(x => x.UserId == linkedUser.Id && !x.IsDelete)
                    .ToListAsync();

                foreach (var item in userOrganizations)
                {
                    item.IsActive = false;
                    item.IsDelete = true;
                    item.DeleteDateTime = now;
                    item.DeleteBy = actorUserId;
                }
            }

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "External user berhasil dihapus."
            ));
        }

        private async Task<string> GenerateExternalCodeAsync()
        {
            const string menuCode = "EXT";
            const string prefix = $"{menuCode}-{HospitalCode}-";

            var existingCount = await _dbContext.Set<MstExternalUser>()
                .IgnoreQueryFilters()
                .CountAsync(x => x.ExternalCode.StartsWith(prefix));

            var nextNumber = existingCount + 1;

            while (true)
            {
                var code = $"{prefix}{nextNumber:D5}";

                var exists = await _dbContext.Set<MstExternalUser>()
                    .IgnoreQueryFilters()
                    .AnyAsync(x => x.ExternalCode == code);

                if (!exists)
                {
                    return code;
                }

                nextNumber++;
            }
        }

        private async Task<string> GenerateWorkforceProfileCodeAsync()
        {
            const string prefix = $"WFP-{HospitalCode}-";

            var totalData = await _dbContext.Set<MstWorkforceProfile>()
                .IgnoreQueryFilters()
                .CountAsync();

            var nextNumber = totalData + 1;

            while (true)
            {
                var code = $"{prefix}{nextNumber.ToString("D5")}";

                var exists = await _dbContext.Set<MstWorkforceProfile>()
                    .AnyAsync(x => x.ProfileCode == code);

                if (!exists)
                {
                    return code;
                }

                nextNumber++;
            }
        }

        private async Task<ExternalUserAccountCompactResponse?> BuildExternalUserAccountCompactResponseAsync(Guid externalUserId)
        {
            var now = DateTime.UtcNow;

            var user = await _dbContext.Users
                .AsNoTracking()
                .Where(x =>
                    x.ExternalUserId == externalUserId &&
                    x.UserType == UserType.ExternalUser)
                .Select(x => new ExternalUserAccountCompactResponse
                {
                    IsAvailable = true,
                    UserId = x.Id,
                    UserCode = x.UserCode,
                    UserName = x.UserName,
                    Email = x.Email,
                    DisplayName = x.DisplayName,
                    UserType = x.UserType,
                    IsActive = x.IsActive,
                    MustChangePassword = x.MustChangePassword,
                    ProfilePhotoPath = x.ProfilePhotoPath,

                    IsFingerprintRegistrationEnabled = x.IsFingerprintRegistrationEnabled,
                    FingerprintRegistrationReason = x.FingerprintRegistrationReason,
                    FingerprintRegistrationEnabledAt = x.FingerprintRegistrationEnabledAt,

                    IsGeolocationBypassEnabled = x.IsGeolocationBypassEnabled,
                    GeolocationBypassReason = x.GeolocationBypassReason,
                    GeolocationBypassUntil = x.GeolocationBypassUntil,
                    IsGeolocationBypassActive =
                        x.IsGeolocationBypassEnabled &&
                        (!x.GeolocationBypassUntil.HasValue || x.GeolocationBypassUntil.Value >= now)
                })
                .FirstOrDefaultAsync();

            if (user != null)
            {
                EnrichExternalUserAccountPhotoFields(user);
                return user;
            }

            return new ExternalUserAccountCompactResponse
            {
                IsAvailable = false,
                IsActive = false,
                MustChangePassword = false,
                IsGeolocationBypassEnabled = false,
                IsGeolocationBypassActive = false
            };
        }

        private async Task<ExternalUserChildSummaryResponse> BuildExternalUserChildSummaryAsync(Guid externalUserId)
        {
            var externalUser = await _dbContext.Set<MstExternalUser>()
                .AsNoTracking()
                .Where(x => x.Id == externalUserId && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.WorkforceProfileId
                })
                .FirstOrDefaultAsync();

            if (externalUser == null || externalUser.WorkforceProfileId == Guid.Empty)
            {
                return new ExternalUserChildSummaryResponse();
            }

            var workforceProfileId = externalUser.WorkforceProfileId;

            var organizationQuery = _dbContext.Set<WfpOrganizationAssignment>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            return new ExternalUserChildSummaryResponse
            {
                OrganizationCount = await organizationQuery.CountAsync(),
                ActiveOrganizationCount = await organizationQuery.CountAsync(x => x.IsActive),
                PrimaryOrganizationCount = await organizationQuery.CountAsync(x => x.IsPrimary && x.IsActive),

                BankAccountCount = await _dbContext.Set<WfpBankAccount>()
                    .AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete),

                DocumentCount = await _dbContext.Set<WfpDocument>()
                    .AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete),

                CertificationCount = await _dbContext.Set<WfpCertification>()
                    .AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete),

                CredentialLicenseCount = await _dbContext.Set<WfpCredentialLicense>()
                    .AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete),

                ScheduleAssignmentCount = await _dbContext.Set<WfpWorkScheduleAssignment>()
                    .AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete),

                AttendanceCount = await _dbContext.Set<EmpAttendance>()
                    .AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateExternalUserRequestAsync(
            Guid? excludeExternalUserId,
            Guid? primaryDepartmentId,
            Guid? primaryPositionId,
            Guid? countryId,
            Guid? provinceId,
            Guid? cityId,
            Guid? districtId,
            Guid? postalCodeId,
            string? identityNumber,
            string? phoneNumber,
            string? whatsAppNumber,
            string? email,
            string? taxNumber,
            string? businessLicenseNumber)
        {
            primaryDepartmentId = NormalizeNullableGuid(primaryDepartmentId);
            primaryPositionId = NormalizeNullableGuid(primaryPositionId);

            if (primaryPositionId.HasValue && !primaryDepartmentId.HasValue)
            {
                return (false, "Primary department wajib dipilih jika primary position diisi.");
            }

            if (primaryDepartmentId.HasValue)
            {
                var departmentExists = await _dbContext.MstDepartments
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == primaryDepartmentId.Value && x.IsActive && !x.IsDelete);

                if (!departmentExists)
                {
                    return (false, "Primary department tidak valid atau tidak aktif.");
                }
            }

            if (primaryPositionId.HasValue)
            {
                var positionExists = await _dbContext.MstPositions
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == primaryPositionId.Value &&
                        x.DepartmentId == primaryDepartmentId!.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!positionExists)
                {
                    return (false, "Primary position tidak valid, tidak aktif, atau tidak sesuai department.");
                }
            }

            var regionValidation = await ValidateRegionAsync(
                countryId,
                provinceId,
                cityId,
                districtId,
                postalCodeId);

            if (!regionValidation.IsValid)
            {
                return regionValidation;
            }

            var normalizedIdentityNumber = NormalizeNullableText(identityNumber);
            var normalizedPhoneNumber = NormalizeDigitsOnly(phoneNumber);
            var normalizedWhatsAppNumber = NormalizeDigitsOnly(whatsAppNumber);
            var normalizedEmail = NormalizeNullableText(email);
            var normalizedTaxNumber = NormalizeNullableText(taxNumber);
            var normalizedBusinessLicenseNumber = NormalizeNullableText(businessLicenseNumber);

            if (!string.IsNullOrWhiteSpace(normalizedPhoneNumber) && normalizedPhoneNumber.Length > 30)
            {
                return (false, "Nomor telepon maksimal 30 digit.");
            }

            if (!string.IsNullOrWhiteSpace(normalizedWhatsAppNumber) && normalizedWhatsAppNumber.Length > 30)
            {
                return (false, "Nomor WhatsApp maksimal 30 digit.");
            }

            if (!string.IsNullOrWhiteSpace(normalizedIdentityNumber))
            {
                var exists = await _dbContext.Set<MstExternalUser>()
                    .AnyAsync(x =>
                        x.Id != excludeExternalUserId &&
                        x.IdentityNumber == normalizedIdentityNumber &&
                        !x.IsDelete);

                if (exists)
                {
                    return (false, "Nomor identitas external user sudah digunakan.");
                }
            }

            if (!string.IsNullOrWhiteSpace(normalizedTaxNumber))
            {
                var exists = await _dbContext.Set<MstExternalUser>()
                    .AnyAsync(x =>
                        x.Id != excludeExternalUserId &&
                        x.TaxNumber == normalizedTaxNumber &&
                        !x.IsDelete);

                if (exists)
                {
                    return (false, "Tax number external user sudah digunakan.");
                }
            }

            if (!string.IsNullOrWhiteSpace(normalizedBusinessLicenseNumber))
            {
                var exists = await _dbContext.Set<MstExternalUser>()
                    .AnyAsync(x =>
                        x.Id != excludeExternalUserId &&
                        x.BusinessLicenseNumber == normalizedBusinessLicenseNumber &&
                        !x.IsDelete);

                if (exists)
                {
                    return (false, "Business license number external user sudah digunakan.");
                }
            }

            if (!string.IsNullOrWhiteSpace(normalizedEmail))
            {
                var emailLower = normalizedEmail.ToLower();

                var exists = await _dbContext.Set<MstExternalUser>()
                    .AnyAsync(x =>
                        x.Id != excludeExternalUserId &&
                        x.Email != null &&
                        x.Email.ToLower() == emailLower &&
                        !x.IsDelete);

                if (exists)
                {
                    return (false, "Email external user sudah digunakan.");
                }
            }

            return (true, null);
        }

        private async Task EnsureWorkforcePrimaryOrganizationAssignmentAsync(
            MstExternalUser externalUser,
            DateTime now,
            Guid actorUserId)
        {
            if (externalUser.WorkforceProfileId == Guid.Empty ||
                !externalUser.PrimaryDepartmentId.HasValue ||
                !externalUser.PrimaryPositionId.HasValue)
            {
                return;
            }

            var workforceProfileId = externalUser.WorkforceProfileId;
            var departmentId = externalUser.PrimaryDepartmentId.Value;
            var positionId = externalUser.PrimaryPositionId.Value;

            var currentPrimaries = await _dbContext.Set<WfpOrganizationAssignment>()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsPrimary &&
                    !x.IsDelete)
                .ToListAsync();

            foreach (var item in currentPrimaries)
            {
                item.IsPrimary = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }

            var existing = await _dbContext.Set<WfpOrganizationAssignment>()
                .FirstOrDefaultAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.DepartmentId == departmentId &&
                    x.PositionId == positionId);

            if (existing == null)
            {
                existing = new WfpOrganizationAssignment
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    DepartmentId = departmentId,
                    PositionId = positionId,
                    IsPrimary = true,
                    IsActive = externalUser.IsActive,
                    EffectiveStartDate = (externalUser.ContractStartDate ?? now.Date).Date,
                    EffectiveEndDate = externalUser.ContractEndDate?.Date,
                    Description = "Primary organization assignment synced from external user update",
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<WfpOrganizationAssignment>().Add(existing);
            }
            else
            {
                existing.IsPrimary = true;
                existing.IsActive = externalUser.IsActive;
                existing.EffectiveStartDate = (externalUser.ContractStartDate ?? now.Date).Date;
                existing.EffectiveEndDate = externalUser.ContractEndDate?.Date;
                existing.Description = "Primary organization assignment synced from external user update";
                existing.IsDelete = false;
                existing.DeleteDateTime = null;
                existing.DeleteBy = Guid.Empty;
                existing.IsCancel = false;
                existing.CancelDateTime = null;
                existing.CancelBy = Guid.Empty;
                existing.UpdateDateTime = now;
                existing.UpdateBy = actorUserId;
            }
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRegionAsync(
            Guid? countryId,
            Guid? provinceId,
            Guid? cityId,
            Guid? districtId,
            Guid? postalCodeId)
        {
            countryId = NormalizeNullableGuid(countryId);
            provinceId = NormalizeNullableGuid(provinceId);
            cityId = NormalizeNullableGuid(cityId);
            districtId = NormalizeNullableGuid(districtId);
            postalCodeId = NormalizeNullableGuid(postalCodeId);

            if (provinceId.HasValue && !countryId.HasValue)
            {
                return (false, "Country wajib dipilih jika province diisi.");
            }

            if (cityId.HasValue && !provinceId.HasValue)
            {
                return (false, "Province wajib dipilih jika city diisi.");
            }

            if (districtId.HasValue && !cityId.HasValue)
            {
                return (false, "City wajib dipilih jika district diisi.");
            }

            if (postalCodeId.HasValue && !districtId.HasValue)
            {
                return (false, "District wajib dipilih jika postal code diisi.");
            }

            if (countryId.HasValue)
            {
                var exists = await _dbContext.MstCountries
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == countryId.Value && x.IsActive && !x.IsDelete);

                if (!exists)
                {
                    return (false, "Country tidak valid atau tidak aktif.");
                }
            }

            if (provinceId.HasValue)
            {
                var exists = await _dbContext.MstProvinces
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == provinceId.Value &&
                        x.CountryId == countryId!.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!exists)
                {
                    return (false, "Province tidak valid, tidak aktif, atau tidak sesuai country.");
                }
            }

            if (cityId.HasValue)
            {
                var exists = await _dbContext.MstCities
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == cityId.Value &&
                        x.ProvinceId == provinceId!.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!exists)
                {
                    return (false, "City tidak valid, tidak aktif, atau tidak sesuai province.");
                }
            }

            if (districtId.HasValue)
            {
                var exists = await _dbContext.MstDistricts
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == districtId.Value &&
                        x.CityId == cityId!.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!exists)
                {
                    return (false, "District tidak valid, tidak aktif, atau tidak sesuai city.");
                }
            }

            if (postalCodeId.HasValue)
            {
                var exists = await _dbContext.MstPostalCodes
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == postalCodeId.Value &&
                        x.DistrictId == districtId!.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!exists)
                {
                    return (false, "Postal code tidak valid, tidak aktif, atau tidak sesuai district.");
                }
            }

            return (true, null);
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 25 : pageSize;
            pageSize = pageSize > 100 ? 100 : pageSize;

            return (pageNumber, pageSize);
        }

        private static DateRangeResolveResult ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? period)
        {
            var selectedPeriod = period?.Trim().ToLower();
            var today = DateTime.UtcNow.Date;

            DateTime? start = null;
            DateTime? endExclusive = null;

            switch (selectedPeriod)
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

                case "yesterday":
                    start = today.AddDays(-1);
                    endExclusive = today;
                    break;

                case "last7days":
                    start = today.AddDays(-6);
                    endExclusive = today.AddDays(1);
                    break;

                case "last30days":
                    start = today.AddDays(-29);
                    endExclusive = today.AddDays(1);
                    break;

                case "last90days":
                    start = today.AddDays(-89);
                    endExclusive = today.AddDays(1);
                    break;

                case "thismonth":
                    start = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    endExclusive = start.Value.AddMonths(1);
                    break;

                case "lastmonth":
                    var thisMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    start = thisMonth.AddMonths(-1);
                    endExclusive = thisMonth;
                    break;

                case "thisyear":
                    start = new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    endExclusive = start.Value.AddYears(1);
                    break;

                default:
                    return DateRangeResolveResult.Invalid(
                        $"period '{period}' tidak valid. Gunakan endpoint filters/metadata untuk melihat daftar period yang tersedia."
                    );
            }

            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value)
            {
                return DateRangeResolveResult.Invalid(
                    "startDate tidak boleh lebih besar atau sama dengan endDate."
                );
            }

            return DateRangeResolveResult.Valid(start, endExclusive);
        }

        private static IOrderedQueryable<MstExternalUser> ApplyExternalUserSorting(
            IQueryable<MstExternalUser> query,
            string? sortBy,
            string? sortDirection)
        {
            var field = NormalizeSortBy(sortBy);
            var desc = IsDescending(sortDirection);

            return field switch
            {
                "externalcode" => desc
                    ? query.OrderByDescending(x => x.ExternalCode).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.ExternalCode).ThenBy(x => x.FullName),

                "fullname" => desc
                    ? query.OrderByDescending(x => x.FullName).ThenBy(x => x.ExternalCode)
                    : query.OrderBy(x => x.FullName).ThenBy(x => x.ExternalCode),

                "companyname" => desc
                    ? query.OrderByDescending(x => x.CompanyName ?? string.Empty).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.CompanyName ?? string.Empty).ThenBy(x => x.FullName),

                "departmentname" => desc
                    ? query.OrderByDescending(x => x.PrimaryDepartment != null ? x.PrimaryDepartment.DepartmentName : string.Empty).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.PrimaryDepartment != null ? x.PrimaryDepartment.DepartmentName : string.Empty).ThenBy(x => x.FullName),

                "positionname" => desc
                    ? query.OrderByDescending(x => x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : string.Empty).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : string.Empty).ThenBy(x => x.FullName),

                "accessenddate" => desc
                    ? query.OrderByDescending(x => x.AccessEndDate).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.AccessEndDate).ThenBy(x => x.FullName),

                "isactive" => desc
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.FullName),

                _ => desc
                    ? query.OrderByDescending(x => x.CreateDateTime).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.FullName)
            };
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

        private static List<ExternalUserCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<ExternalUserCustomPeriodOptionResponse>
            {
                new()
                {
                    Value = "custom",
                    Label = "Custom Date Range",
                    Description = "Frontend mengirim startDate dan/atau endDate manual. Format tanggal: yyyy-MM-dd.",
                    UsesStartDate = true,
                    UsesEndDate = true
                },
                new()
                {
                    Value = "today",
                    Label = "Hari Ini",
                    Description = "Filter data yang dibuat hari ini berdasarkan waktu UTC.",
                    UsesStartDate = false,
                    UsesEndDate = false
                },
                new()
                {
                    Value = "yesterday",
                    Label = "Kemarin",
                    Description = "Filter data yang dibuat kemarin berdasarkan waktu UTC.",
                    UsesStartDate = false,
                    UsesEndDate = false
                },
                new()
                {
                    Value = "last7days",
                    Label = "7 Hari Terakhir",
                    Description = "Filter data dari 7 hari terakhir termasuk hari ini.",
                    UsesStartDate = false,
                    UsesEndDate = false
                },
                new()
                {
                    Value = "last30days",
                    Label = "30 Hari Terakhir",
                    Description = "Filter data dari 30 hari terakhir termasuk hari ini.",
                    UsesStartDate = false,
                    UsesEndDate = false
                },
                new()
                {
                    Value = "last90days",
                    Label = "90 Hari Terakhir",
                    Description = "Filter data dari 90 hari terakhir termasuk hari ini.",
                    UsesStartDate = false,
                    UsesEndDate = false
                },
                new()
                {
                    Value = "thismonth",
                    Label = "Bulan Ini",
                    Description = "Filter data dari tanggal 1 bulan berjalan sampai akhir bulan berjalan.",
                    UsesStartDate = false,
                    UsesEndDate = false
                },
                new()
                {
                    Value = "lastmonth",
                    Label = "Bulan Lalu",
                    Description = "Filter data dari tanggal 1 bulan lalu sampai akhir bulan lalu.",
                    UsesStartDate = false,
                    UsesEndDate = false
                },
                new()
                {
                    Value = "thisyear",
                    Label = "Tahun Ini",
                    Description = "Filter data dari tanggal 1 Januari tahun berjalan sampai akhir tahun berjalan.",
                    UsesStartDate = false,
                    UsesEndDate = false
                }
            };
        }

        private static List<ExternalUserQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<ExternalUserQueryParameterInfoResponse>
            {
                new()
                {
                    Name = "startDate",
                    Type = "date",
                    Description = "Tanggal mulai filter berdasarkan CreateDateTime. Digunakan jika period kosong atau custom.",
                    Example = "2026-01-01"
                },
                new()
                {
                    Name = "endDate",
                    Type = "date",
                    Description = "Tanggal akhir filter berdasarkan CreateDateTime. Digunakan jika period kosong atau custom.",
                    Example = "2026-01-31"
                },
                new()
                {
                    Name = "period",
                    Type = "string",
                    Description = "Pilihan periode cepat. Jika period selain custom diisi, startDate dan endDate akan diabaikan.",
                    Example = "last30days"
                },
                new()
                {
                    Name = "departmentId",
                    Type = "guid",
                    Description = "Relasi filter 1: filter berdasarkan primary department."
                },
                new()
                {
                    Name = "positionId",
                    Type = "guid",
                    Description = "Relasi filter 2: filter berdasarkan primary position."
                },
                new()
                {
                    Name = "isActive",
                    Type = "boolean",
                    Description = "Filter status aktif external user.",
                    Example = "true"
                },
                new()
                {
                    Name = "search",
                    Type = "string",
                    Description = "Pencarian external code, nama, company, job title, email, phone, department, dan position.",
                    Example = "vendor"
                },
                new()
                {
                    Name = "sortBy",
                    Type = "string",
                    Description = "Field sorting. Nilai tersedia dari SortOptions.",
                    Example = "fullName"
                },
                new()
                {
                    Name = "sortDirection",
                    Type = "string",
                    Description = "Arah sorting: asc atau desc.",
                    Example = "asc"
                },
                new()
                {
                    Name = "pageNumber",
                    Type = "integer",
                    Description = "Nomor halaman. Minimal 1.",
                    Example = "1"
                },
                new()
                {
                    Name = "pageSize",
                    Type = "integer",
                    Description = "Jumlah data per halaman. Maksimal 100.",
                    Example = "25"
                }
            };
        }

        private static List<ExternalUserFormFieldMetadataResponse> BuildCreateExternalUserFieldMetadata()
        {
            return BuildExternalUserFieldMetadata(isUpdate: false);
        }

        private static List<ExternalUserFormFieldMetadataResponse> BuildUpdateExternalUserFieldMetadata()
        {
            var fields = BuildExternalUserFieldMetadata(isUpdate: true);

            fields.Add(new ExternalUserFormFieldMetadataResponse
            {
                Name = "isActive",
                Label = "Status Aktif",
                Section = "Status",
                InputType = "boolean",
                IsRequiredOnCreate = false,
                IsRequiredOnUpdate = true,
                RequiredType = "Required",
                Description = "Status aktif external user.",
                Example = "true",
                SortOrder = 900
            });

            return fields.OrderBy(x => x.SortOrder).ToList();
        }

        private static List<ExternalUserFormFieldMetadataResponse> BuildExternalUserFieldMetadata(bool isUpdate)
        {
            return new List<ExternalUserFormFieldMetadataResponse>
            {
                new()
                {
                    Name = "externalUserType",
                    Label = "Tipe External User",
                    Section = "Identitas Utama",
                    InputType = "select",
                    IsRequiredOnCreate = true,
                    IsRequiredOnUpdate = true,
                    RequiredType = "Required",
                    OptionsSource = "externalUserTypeOptions",
                    Example = "1",
                    SortOrder = 1
                },
                new()
                {
                    Name = "externalUserStatus",
                    Label = "Status External User",
                    Section = "Identitas Utama",
                    InputType = "select",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    OptionsSource = "externalUserStatusOptions",
                    Example = "1",
                    SortOrder = 2
                },
                new()
                {
                    Name = "engagementType",
                    Label = "Tipe Engagement",
                    Section = "Identitas Utama",
                    InputType = "select",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    OptionsSource = "engagementTypeOptions",
                    Example = "1",
                    SortOrder = 3
                },
                new()
                {
                    Name = "fullName",
                    Label = "Nama Lengkap",
                    Section = "Identitas Utama",
                    InputType = "text",
                    IsRequiredOnCreate = true,
                    IsRequiredOnUpdate = true,
                    RequiredType = "Required",
                    MaxLength = 200,
                    Description = "Nama lengkap external user.",
                    Example = "Budi Vendor",
                    SortOrder = 4
                },
                new()
                {
                    Name = "companyName",
                    Label = "Nama Company",
                    Section = "Company",
                    InputType = "text",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 200,
                    Example = "PT Vendor Medika",
                    SortOrder = 101
                },
                new()
                {
                    Name = "companyCode",
                    Label = "Kode Company",
                    Section = "Company",
                    InputType = "text",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 100,
                    Example = "VDR-001",
                    SortOrder = 102
                },
                new()
                {
                    Name = "jobTitle",
                    Label = "Job Title",
                    Section = "Company",
                    InputType = "text",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 100,
                    Example = "Teknisi Alat Kesehatan",
                    SortOrder = 103
                },
                new()
                {
                    Name = "contactPersonName",
                    Label = "Contact Person",
                    Section = "Company",
                    InputType = "text",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 200,
                    SortOrder = 104
                },
                new()
                {
                    Name = "identityType",
                    Label = "Tipe Identitas",
                    Section = "Identitas Legal & Kontak",
                    InputType = "text",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 50,
                    Example = "KTP",
                    SortOrder = 201
                },
                new()
                {
                    Name = "identityNumber",
                    Label = "Nomor Identitas",
                    Section = "Identitas Legal & Kontak",
                    InputType = "text",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 100,
                    ValidationRule = "unique",
                    SortOrder = 202
                },
                new()
                {
                    Name = "taxNumber",
                    Label = "NPWP / Tax Number",
                    Section = "Identitas Legal & Kontak",
                    InputType = "text",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 100,
                    ValidationRule = "unique",
                    SortOrder = 203
                },
                new()
                {
                    Name = "businessLicenseNumber",
                    Label = "Nomor Izin Usaha",
                    Section = "Identitas Legal & Kontak",
                    InputType = "text",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 100,
                    ValidationRule = "unique",
                    SortOrder = 204
                },
                new()
                {
                    Name = "phoneNumber",
                    Label = "Nomor Telepon",
                    Section = "Identitas Legal & Kontak",
                    InputType = "text",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 30,
                    ValidationRule = "digits;max:30",
                    SortOrder = 205
                },
                new()
                {
                    Name = "whatsAppNumber",
                    Label = "Nomor WhatsApp",
                    Section = "Identitas Legal & Kontak",
                    InputType = "text",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 30,
                    ValidationRule = "digits;max:30",
                    SortOrder = 206
                },
                new()
                {
                    Name = "email",
                    Label = "Email",
                    Section = "Identitas Legal & Kontak",
                    InputType = "email",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Conditional",
                    MaxLength = 200,
                    ValidationRule = "email;unique",
                    Description = "Wajib jika CreateLoginAccount = true.",
                    SortOrder = 207
                },
                new()
                {
                    Name = "address",
                    Label = "Alamat",
                    Section = "Identitas Legal & Kontak",
                    InputType = "textarea",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 500,
                    SortOrder = 208
                },
                new()
                {
                    Name = "countryId",
                    Label = "Negara",
                    Section = "Region",
                    InputType = "select",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    OptionsSource = "region.countries.options",
                    SortOrder = 301
                },
                new()
                {
                    Name = "provinceId",
                    Label = "Provinsi",
                    Section = "Region",
                    InputType = "select",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Conditional",
                    DependsOn = "countryId",
                    OptionsSource = "region.provinces.options",
                    SortOrder = 302
                },
                new()
                {
                    Name = "cityId",
                    Label = "Kota/Kabupaten",
                    Section = "Region",
                    InputType = "select",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Conditional",
                    DependsOn = "provinceId",
                    OptionsSource = "region.cities.options",
                    SortOrder = 303
                },
                new()
                {
                    Name = "districtId",
                    Label = "Kecamatan",
                    Section = "Region",
                    InputType = "select",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Conditional",
                    DependsOn = "cityId",
                    OptionsSource = "region.districts.options",
                    SortOrder = 304
                },
                new()
                {
                    Name = "postalCodeId",
                    Label = "Kode Pos / Kelurahan",
                    Section = "Region",
                    InputType = "select",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    DependsOn = "districtId",
                    OptionsSource = "region.postal-codes.options",
                    SortOrder = 305
                },
                new()
                {
                    Name = "primaryDepartmentId",
                    Label = "Department Utama",
                    Section = "Organisasi",
                    InputType = "select",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    OptionsSource = "organization.departments.options",
                    SortOrder = 401
                },
                new()
                {
                    Name = "primaryPositionId",
                    Label = "Position Utama",
                    Section = "Organisasi",
                    InputType = "select",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Conditional",
                    DependsOn = "primaryDepartmentId",
                    OptionsSource = "organization.positions.options",
                    Description = "Wajib jika primaryDepartmentId diisi.",
                    SortOrder = 402
                },
                new()
                {
                    Name = "workLocation",
                    Label = "Lokasi Kerja",
                    Section = "Engagement & Access",
                    InputType = "text",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 50,
                    Example = "RSMMC",
                    SortOrder = 501
                },
                new()
                {
                    Name = "contractStartDate",
                    Label = "Tanggal Mulai Kontrak",
                    Section = "Engagement & Access",
                    InputType = "date",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    SortOrder = 502
                },
                new()
                {
                    Name = "contractEndDate",
                    Label = "Tanggal Akhir Kontrak",
                    Section = "Engagement & Access",
                    InputType = "date",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    SortOrder = 503
                },
                new()
                {
                    Name = "accessStartDate",
                    Label = "Mulai Akses",
                    Section = "Engagement & Access",
                    InputType = "datetime",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    Description = "Waktu akses mulai. Backend menyimpan dalam UTC.",
                    SortOrder = 504
                },
                new()
                {
                    Name = "accessEndDate",
                    Label = "Akhir Akses",
                    Section = "Engagement & Access",
                    InputType = "datetime",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    Description = "Waktu akses berakhir. Backend menyimpan dalam UTC.",
                    SortOrder = 505
                },
                new()
                {
                    Name = "accessPurpose",
                    Label = "Tujuan Akses",
                    Section = "Engagement & Access",
                    InputType = "textarea",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 250,
                    SortOrder = 506
                },
                new()
                {
                    Name = "description",
                    Label = "Deskripsi",
                    Section = "Catatan",
                    InputType = "textarea",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 250,
                    SortOrder = 601
                }
            }
            .OrderBy(x => x.SortOrder)
            .ToList();
        }

        private static List<ExternalUserEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : struct, System.Enum
        {
            return System.Enum.GetValues<TEnum>()
                .Select(x => new ExternalUserEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = BuildEnumLabel(x.ToString())
                })
                .ToList();
        }

        private static string BuildEnumLabel(string value)
        {
            return value switch
            {
                "Unknown" => "Tidak Diketahui",
                "Active" => "Aktif",
                "Inactive" => "Tidak Aktif",
                "PendingApproval" => "Menunggu Approval",
                "Suspended" => "Suspended",
                "Blacklisted" => "Blacklisted",
                "ContractEnded" => "Kontrak Berakhir",
                "AccessExpired" => "Akses Expired",

                "Vendor" => "Vendor",
                "Supplier" => "Supplier",
                "Contractor" => "Contractor",
                "OutsourcedStaff" => "Outsourced Staff",
                "Consultant" => "Konsultan",
                "InsurancePartner" => "Partner Asuransi",
                "GovernmentAuditor" => "Auditor Pemerintah",
                "StudentIntern" => "Mahasiswa Magang",
                "Volunteer" => "Relawan",
                "Visitor" => "Visitor",
                "PatientFamilyAccess" => "Akses Keluarga Pasien",
                "Other" => "Lainnya",

                "ContractBased" => "Berbasis Kontrak",
                "ProjectBased" => "Berbasis Project",
                "VisitBased" => "Berbasis Kunjungan",
                "Partnership" => "Partnership",
                "Outsourcing" => "Outsourcing",
                "Audit" => "Audit",
                "Training" => "Training",
                "TemporaryAccess" => "Akses Sementara",
                _ => value
            };
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

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
            {
                return null;
            }

            return value.Value;
        }

        private static string? NormalizeDigitsOnly(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var digits = new string(value.Where(char.IsDigit).ToArray());

            return string.IsNullOrWhiteSpace(digits)
                ? null
                : digits;
        }

        private static DateTime? NormalizeNullableDateTimeUtc(DateTime? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            if (value.Value.Kind == DateTimeKind.Utc)
            {
                return value.Value;
            }

            if (value.Value.Kind == DateTimeKind.Local)
            {
                return value.Value.ToUniversalTime();
            }

            return DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
        }

        private string GetDefaultUserProfilePhotoPath()
        {
            var configuredPath = _configuration["FileStorage:DefaultUserProfilePhotoPath"];

            return string.IsNullOrWhiteSpace(configuredPath)
                ? DefaultUserProfilePhotoPathFallback
                : configuredPath.Trim();
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

        private async Task<(bool IsSuccess, string? ErrorMessage, ExternalUserLoginAccountResponse? Response)>
            CreateLoginAccountForExternalUserAsync(
                MstExternalUser externalUser,
                bool isFingerprintRegistrationEnabled,
                string? fingerprintRegistrationReason,
                Guid actorUserId)
        {
            if (externalUser.Id == Guid.Empty)
            {
                return (false, "ExternalUserId tidak valid.", null);
            }

            if (string.IsNullOrWhiteSpace(externalUser.Email))
            {
                return (false, "Email external user wajib diisi untuk membuat akun login.", null);
            }

            var email = externalUser.Email.Trim().ToLowerInvariant();

            var existingByEmail = await _userManager.FindByEmailAsync(email);

            if (existingByEmail != null)
            {
                return (false, "Email sudah digunakan oleh akun login lain.", null);
            }

            var existingByUserName = await _userManager.FindByNameAsync(email);

            if (existingByUserName != null)
            {
                return (false, "Username sudah digunakan oleh akun login lain.", null);
            }

            var existingExternalUserAccount = await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(x =>
                    x.ExternalUserId == externalUser.Id &&
                    x.UserType == UserType.ExternalUser);

            if (existingExternalUserAccount)
            {
                return (false, "External user ini sudah memiliki akun login.", null);
            }

            var initialPassword = GenerateInitialPasswordForExternalUser();

            if (string.IsNullOrWhiteSpace(initialPassword))
            {
                return (false, "Password awal gagal dibuat.", null);
            }

            var now = DateTime.UtcNow;

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserCode = await GenerateUserCodeAsync(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = externalUser.FullName,
                UserType = UserType.ExternalUser,
                WorkforceProfileId = externalUser.WorkforceProfileId,
                EmployeeId = null,
                DoctorId = null,
                ExternalUserId = externalUser.Id,
                PrimaryDepartmentId = externalUser.PrimaryDepartmentId,
                PrimaryPositionId = externalUser.PrimaryPositionId,
                IsGeolocationBypassEnabled = false,
                GeolocationBypassReason = null,
                GeolocationBypassUntil = null,
                IsActive = externalUser.IsActive,
                MustChangePassword = true,
                AccessValidUntil = externalUser.AccessEndDate,
                CreateDateTime = now,
                ProfilePhotoPath = GetDefaultUserProfilePhotoPath(),
                IsFingerprintRegistrationEnabled = isFingerprintRegistrationEnabled,
                FingerprintRegistrationReason = isFingerprintRegistrationEnabled
                    ? NormalizeNullableText(fingerprintRegistrationReason)
                    : null,
                FingerprintRegistrationEnabledAt = isFingerprintRegistrationEnabled
                    ? now
                    : null,
                FingerprintRegistrationEnabledByUserId = isFingerprintRegistrationEnabled
                    ? actorUserId
                    : null
            };

            var createResult = await _userManager.CreateAsync(user, initialPassword);

            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(x => x.Description));

                return (false, $"Akun login gagal dibuat: {errors}", null);
            }

            var response = new ExternalUserLoginAccountResponse
            {
                IsCreated = true,
                UserId = user.Id,
                UserCode = user.UserCode,
                UserName = user.UserName,
                Email = user.Email,
                DisplayName = user.DisplayName,
                UserType = user.UserType,
                InitialPassword = initialPassword,
                MustChangePassword = user.MustChangePassword,
                ProfilePhotoPath = user.ProfilePhotoPath,
                IsFingerprintRegistrationEnabled = user.IsFingerprintRegistrationEnabled,
                FingerprintRegistrationReason = user.FingerprintRegistrationReason,
                FingerprintRegistrationEnabledAt = user.FingerprintRegistrationEnabledAt,
                Message = "Akun login external user berhasil dibuat. Password awal wajib diganti saat login pertama."
            };

            return (true, null, response);
        }

        private async Task SyncUserPrimaryOrganizationAsync(
            Guid userId,
            Guid departmentId,
            Guid positionId,
            DateTime effectiveStartDate,
            Guid actorUserId)
        {
            var now = DateTime.UtcNow;
            var effectiveStartUtc = DateTime.SpecifyKind(effectiveStartDate.Date, DateTimeKind.Utc);

            var currentPrimaryItems = await _dbContext.ApplicationUserOrganizations
                .Where(x =>
                    x.UserId == userId &&
                    x.IsPrimary &&
                    !x.IsDelete)
                .ToListAsync();

            foreach (var item in currentPrimaryItems)
            {
                item.IsPrimary = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }

            var existing = await _dbContext.ApplicationUserOrganizations
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.DepartmentId == departmentId &&
                    x.PositionId == positionId);

            if (existing == null)
            {
                existing = new ApplicationUserOrganization
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    DepartmentId = departmentId,
                    PositionId = positionId,
                    IsPrimary = true,
                    IsActive = true,
                    EffectiveStartDate = effectiveStartUtc,
                    EffectiveEndDate = null,
                    Description = "Synced from external user primary organization assignment",
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.ApplicationUserOrganizations.Add(existing);
            }
            else
            {
                existing.IsPrimary = true;
                existing.IsActive = true;
                existing.EffectiveStartDate = effectiveStartUtc;
                existing.EffectiveEndDate = null;
                existing.Description = "Synced from external user primary organization assignment";
                existing.IsDelete = false;
                existing.DeleteDateTime = null;
                existing.DeleteBy = Guid.Empty;
                existing.IsCancel = false;
                existing.CancelDateTime = null;
                existing.CancelBy = Guid.Empty;
                existing.UpdateDateTime = now;
                existing.UpdateBy = actorUserId;
            }
        }

        private async Task<string> GenerateUserCodeAsync()
        {
            const string prefix = $"USR-{HospitalCode}-";

            var totalData = await _dbContext.Users
                .IgnoreQueryFilters()
                .CountAsync();

            var nextNumber = totalData + 1;

            while (true)
            {
                var userCode = $"{prefix}{nextNumber.ToString("D5")}";

                var exists = await _dbContext.Users
                    .AnyAsync(x => x.UserCode == userCode);

                if (!exists)
                {
                    return userCode;
                }

                nextNumber++;
            }
        }

        private static string GenerateInitialPasswordForExternalUser()
        {
            var suffix = Guid.NewGuid().ToString("N").Substring(0, 6);
            return $"Ext@{DateTime.UtcNow:yyyyMMdd}{suffix}";
        }


        private string ResolveUserProfilePhotoPath(string? profilePhotoPath)
        {
            return string.IsNullOrWhiteSpace(profilePhotoPath)
                ? GetDefaultUserProfilePhotoPath()
                : profilePhotoPath.Trim();
        }

        private string? BuildPublicFileUrl(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            var normalizedPath = filePath.Trim();

            if (normalizedPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                normalizedPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                normalizedPath.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                return normalizedPath;
            }

            if (!normalizedPath.StartsWith('/'))
            {
                normalizedPath = "/" + normalizedPath;
            }

            var configuredBaseUrl =
                _configuration["FileStorage:PublicBaseUrl"] ??
                _configuration["FileStorage:BaseUrl"] ??
                _configuration["App:PublicBaseUrl"] ??
                _configuration["AppSettings:PublicBaseUrl"];

            var requestBaseUrl = Request?.Host.HasValue == true
                ? $"{Request.Scheme}://{Request.Host.Value}"
                : string.Empty;

            var baseUrl = string.IsNullOrWhiteSpace(configuredBaseUrl)
                ? requestBaseUrl
                : configuredBaseUrl.Trim();

            return string.IsNullOrWhiteSpace(baseUrl)
                ? normalizedPath
                : baseUrl.TrimEnd('/') + normalizedPath;
        }

        private void EnrichExternalUserPhotoFields(ExternalUserResponse response)
        {
            response.ProfilePhotoPath = ResolveUserProfilePhotoPath(response.ProfilePhotoPath);
            response.ProfilePhotoUrl = BuildPublicFileUrl(response.ProfilePhotoPath);
            response.ExternalUserPhotoPath = response.ProfilePhotoPath;
            response.ExternalUserPhotoUrl = response.ProfilePhotoUrl;
        }

        private void EnrichExternalUserPhotoFields(ExternalUserOptionResponse response)
        {
            response.ProfilePhotoPath = ResolveUserProfilePhotoPath(response.ProfilePhotoPath);
            response.ProfilePhotoUrl = BuildPublicFileUrl(response.ProfilePhotoPath);
            response.ExternalUserPhotoPath = response.ProfilePhotoPath;
            response.ExternalUserPhotoUrl = response.ProfilePhotoUrl;
        }

        private void EnrichExternalUserAccountPhotoFields(ExternalUserAccountCompactResponse? response)
        {
            if (response == null)
            {
                return;
            }

            response.ProfilePhotoPath = ResolveUserProfilePhotoPath(response.ProfilePhotoPath);
            response.ProfilePhotoUrl = BuildPublicFileUrl(response.ProfilePhotoPath);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateRequiredExternalUserRequest(CreateExternalUserRequest request)
        {
            return ValidateRequiredExternalUserFields(
                externalUserType: request.ExternalUserType,
                fullName: request.FullName,
                primaryDepartmentId: request.PrimaryDepartmentId,
                primaryPositionId: request.PrimaryPositionId,
                accessStartDate: request.AccessStartDate,
                accessEndDate: request.AccessEndDate,
                contractStartDate: request.ContractStartDate,
                contractEndDate: request.ContractEndDate
            );
        }

        private static (bool IsValid, string? ErrorMessage) ValidateRequiredExternalUserRequest(UpdateExternalUserRequest request)
        {
            return ValidateRequiredExternalUserFields(
                externalUserType: request.ExternalUserType,
                fullName: request.FullName,
                primaryDepartmentId: request.PrimaryDepartmentId,
                primaryPositionId: request.PrimaryPositionId,
                accessStartDate: request.AccessStartDate,
                accessEndDate: request.AccessEndDate,
                contractStartDate: request.ContractStartDate,
                contractEndDate: request.ContractEndDate
            );
        }

        private static (bool IsValid, string? ErrorMessage) ValidateRequiredExternalUserFields(
            ExternalUserType externalUserType,
            string? fullName,
            Guid? primaryDepartmentId,
            Guid? primaryPositionId,
            DateTime? accessStartDate,
            DateTime? accessEndDate,
            DateTime? contractStartDate,
            DateTime? contractEndDate)
        {
            if (externalUserType == ExternalUserType.Unknown)
            {
                return (false, "External user type wajib dipilih.");
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                return (false, "Nama external user wajib diisi.");
            }

            if (primaryPositionId.HasValue && primaryPositionId.Value != Guid.Empty &&
                (!primaryDepartmentId.HasValue || primaryDepartmentId.Value == Guid.Empty))
            {
                return (false, "Primary department wajib dipilih jika primary position diisi.");
            }

            if (accessStartDate.HasValue && accessEndDate.HasValue && accessStartDate.Value >= accessEndDate.Value)
            {
                return (false, "AccessStartDate tidak boleh lebih besar atau sama dengan AccessEndDate.");
            }

            if (contractStartDate.HasValue && contractEndDate.HasValue && contractStartDate.Value.Date > contractEndDate.Value.Date)
            {
                return (false, "ContractStartDate tidak boleh lebih besar dari ContractEndDate.");
            }

            return (true, null);
        }
    }
}