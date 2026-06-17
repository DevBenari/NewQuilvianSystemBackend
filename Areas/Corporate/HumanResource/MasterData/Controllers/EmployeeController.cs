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

using ResponseEmployeePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs.EmployeeResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/employees")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Employee",
        AreaName = "Corporate",
        ControllerName = "Employee",
        Description = "Corporate human resource master data employee",
        SortOrder = 4
    )]
    [Tags("Corporate / Human Resource / Master Data / Employee")]
    public class EmployeeController : ControllerBase
    {
        private const string DefaultUserProfilePhotoPathFallback = "/uploads/default-profile-photos/user.png";
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string HospitalCode = "RSMMC";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public EmployeeController(
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

        private static List<EmployeeDetailTabMetadataResponse> BuildEmployeeDetailTabs()
        {
            return new List<EmployeeDetailTabMetadataResponse>
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
                    Key = "education",
                    Label = "Education",
                    Icon = "education",
                    Endpoint = "/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/educations",
                    IsVisibleInDetail = true,
                    IsVisibleInUpdate = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true,
                    SortOrder = 3
                },
                new()
                {
                    Key = "training",
                    Label = "Training",
                    Icon = "training",
                    Endpoint = "/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/training-records",
                    IsVisibleInDetail = true,
                    IsVisibleInUpdate = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true,
                    SortOrder = 4
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
                    SortOrder = 5
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
                    SortOrder = 6
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
                    SortOrder = 7
                },
                new()
                {
                    Key = "transportAllowance",
                    Label = "Transport",
                    Icon = "transport",
                    Endpoint = "/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/transport-allowance",
                    IsVisibleInDetail = true,
                    IsVisibleInUpdate = true,
                    CanCreate = false,
                    CanUpdate = true,
                    CanDelete = false,
                    SortOrder = 8
                },
                new()
                {
                    Key = "payroll",
                    Label = "Payroll",
                    Icon = "payroll",
                    Endpoint = "/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/payroll-profile",
                    IsVisibleInDetail = true,
                    IsVisibleInUpdate = true,
                    CanCreate = false,
                    CanUpdate = true,
                    CanDelete = false,
                    SortOrder = 9
                },
                new()
                {
                    Key = "tax",
                    Label = "Tax",
                    Icon = "tax",
                    Endpoint = "/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/tax-profile",
                    IsVisibleInDetail = true,
                    IsVisibleInUpdate = true,
                    CanCreate = false,
                    CanUpdate = true,
                    CanDelete = false,
                    SortOrder = 10
                },
                new()
                {
                    Key = "insurance",
                    Label = "Insurance",
                    Icon = "insurance",
                    Endpoint = "/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/insurance-profile",
                    IsVisibleInDetail = true,
                    IsVisibleInUpdate = true,
                    CanCreate = false,
                    CanUpdate = true,
                    CanDelete = false,
                    SortOrder = 11
                },
                new()
                {
                    Key = "attendance",
                    Label = "Attendance",
                    Icon = "attendance",
                    Endpoint = "/api/v1/corporate/human-resource/master-data/employees/{employeeId}/attendance",
                    IsVisibleInDetail = true,
                    IsVisibleInUpdate = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false,
                    SortOrder = 12
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
                    SortOrder = 13
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
                    SortOrder = 14
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
                    SortOrder = 15
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
                    SortOrder = 16
                }
            }
            .OrderBy(x => x.SortOrder)
            .ToList();
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Employee",
            Description = "Melihat data employee",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Employee", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new EmployeeFilterMetadataResponse
            {
                DefaultFilter = new EmployeeDefaultFilterResponse
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
                SortOptions = new List<EmployeeSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "employeeCode", Label = "Kode employee" },
                    new() { Value = "employeeNumber", Label = "Nomor employee" },
                    new() { Value = "fullName", Label = "Nama employee" },
                    new() { Value = "departmentName", Label = "Nama department" },
                    new() { Value = "positionName", Label = "Nama position" },
                    new() { Value = "joinDate", Label = "Tanggal bergabung" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                GenderOptions = BuildEnumOptions<Gender>(),
                ReligionOptions = BuildEnumOptions<Religion>(),
                MaritalStatusOptions = BuildEnumOptions<MaritalStatus>(),
                BloodTypeOptions = BuildEnumOptions<BloodType>(),
                EmployeeStatusOptions = BuildEnumOptions<EmployeeStatus>(),
                ProfessionTypeOptions = BuildEnumOptions<EmployeeProfessionType>(),
                EmploymentTypeOptions = BuildEnumOptions<EmploymentType>(),
                TransportAllowanceModes = new List<string>
                {
                    "None",
                    "FixedMonthly",
                    "DailyAttendance",
                    "NightShift",
                    "MonthlyAndNightShift",
                    "Manual"
                },
                TransportTransactionStatuses = new List<string>
                {
                    "Draft",
                    "Calculated",
                    "Approved",
                    "PostedToPayroll",
                    "Cancelled"
                },
                TransportAllowanceTypes = new List<string>
                {
                    "Regular",
                    "Monthly",
                    "Night",
                    "Adjustment",
                    "Manual"
                },
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateEmployeeFieldMetadata(),
                UpdateFields = BuildUpdateEmployeeFieldMetadata(),
                DetailTabs = BuildEmployeeDetailTabs()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Employee.GetFilterMetadata",
                "Mengambil metadata filter employee.",
                result
            );

            return Ok(ApiResponse<EmployeeFilterMetadataResponse>.Ok(
                result,
                "Metadata filter employee berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Employee",
            Description = "Melihat data employee",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Employee", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var employeeQuery = _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var transportProfileQuery = _dbContext.Set<WfpTransportAllowance>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new EmployeeSummaryResponse
            {
                TotalEmployee = await employeeQuery.CountAsync(),
                ActiveEmployee = await employeeQuery.CountAsync(x => x.IsActive),
                InactiveEmployee = await employeeQuery.CountAsync(x => !x.IsActive),
                EmployeeWithTransportAllowanceProfile = await employeeQuery.CountAsync(x =>
                    x.WorkforceProfileId != Guid.Empty &&
                    transportProfileQuery.Any(t => t.WorkforceProfileId == x.WorkforceProfileId)),
                TransportEligibleEmployee = await employeeQuery.CountAsync(x =>
                    x.WorkforceProfileId != Guid.Empty &&
                    transportProfileQuery.Any(t =>
                        t.WorkforceProfileId == x.WorkforceProfileId &&
                        t.IsEligible &&
                        t.IsActive)),
                NightTransportEligibleEmployee = await employeeQuery.CountAsync(x =>
                    x.WorkforceProfileId != Guid.Empty &&
                    transportProfileQuery.Any(t =>
                        t.WorkforceProfileId == x.WorkforceProfileId &&
                        t.IsNightTransportEligible &&
                        t.IsActive))
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Employee.GetSummary",
                "Mengambil ringkasan data employee.",
                result
            );

            return Ok(ApiResponse<EmployeeSummaryResponse>.Ok(
                result,
                "Ringkasan employee berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseEmployeePagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Employee",
            Description = "Melihat data employee",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Employee", "Read")]
        public async Task<IActionResult> GetEmployees(
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

            var query = _dbContext.Set<MstEmployee>()
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
                    x.EmployeeCode.ToLower().Contains(keyword) ||
                    x.EmployeeNumber.ToLower().Contains(keyword) ||
                    x.FullName.ToLower().Contains(keyword) ||
                    (x.NickName != null && x.NickName.ToLower().Contains(keyword)) ||
                    (x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(keyword)) ||
                    (x.WhatsAppNumber != null && x.WhatsAppNumber.ToLower().Contains(keyword)) ||
                    (x.Email != null && x.Email.ToLower().Contains(keyword)) ||
                    (x.IdentityNumber != null && x.IdentityNumber.ToLower().Contains(keyword)) ||
                    (x.PrimaryDepartment != null && x.PrimaryDepartment.DepartmentCode.ToLower().Contains(keyword)) ||
                    (x.PrimaryDepartment != null && x.PrimaryDepartment.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.PrimaryPosition != null && x.PrimaryPosition.PositionCode.ToLower().Contains(keyword)) ||
                    (x.PrimaryPosition != null && x.PrimaryPosition.PositionName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();
            var defaultEmployeeProfilePhotoPath = GetDefaultUserProfilePhotoPath();

            var items = await ApplyEmployeeSorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new EmployeeResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    EmployeeCode = x.EmployeeCode,
                    EmployeeNumber = x.EmployeeNumber,
                    FullName = x.FullName,
                    NickName = x.NickName,
                    Gender = x.Gender,
                    PhoneNumber = x.PhoneNumber,
                    WhatsAppNumber = x.WhatsAppNumber,
                    Email = x.Email,
                    ProfilePhotoPath = _dbContext.Users
                        .Where(u =>
                            u.EmployeeId == x.Id &&
                            u.UserType == UserType.Employee)
                        .OrderByDescending(u => u.IsActive)
                        .ThenByDescending(u => u.UpdateDateTime ?? u.CreateDateTime)
                        .Select(u => u.ProfilePhotoPath)
                        .FirstOrDefault() ?? defaultEmployeeProfilePhotoPath,

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

                    EmployeeStatus = x.EmployeeStatus,
                    ProfessionType = x.ProfessionType,
                    EmploymentType = x.EmploymentType,
                    GradeLevel = x.GradeLevel,
                    WorkLocation = x.WorkLocation,
                    JoinDate = x.JoinDate,
                    ContractEndDate = x.ContractEndDate,
                    ResignDate = x.ResignDate,
                    HasUserAccount = _dbContext.Users.Any(u =>
                        u.EmployeeId == x.Id &&
                        u.UserType == UserType.Employee),
                    HasTransportAllowanceProfile = x.WorkforceProfileId != Guid.Empty &&
                        _dbContext.Set<WfpTransportAllowance>()
                            .Any(t =>
                                t.WorkforceProfileId == x.WorkforceProfileId &&
                                !t.IsDelete),
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy != Guid.Empty ? x.CreateBy : null,
                    CreateByName = x.CreateBy != Guid.Empty
                        ? _dbContext.Users
                            .Where(u => u.Id == x.CreateBy)
                            .Select(u =>
                                u.DisplayName ??
                                u.UserName ??
                                u.Email ??
                                u.UserCode)
                            .FirstOrDefault()
                        : null
                })
                .ToListAsync();

            foreach (var item in items)
            {
                EnrichEmployeePhotoFields(item);
            }

            var result = new ResponseEmployeePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Employee.GetEmployees",
                "Mengambil data employee.",
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

            return Ok(ApiResponse<ResponseEmployeePagedResult>.Ok(
                result,
                "Data employee berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EmployeeOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Employee",
            Description = "Melihat pilihan employee",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Employee", "Read")]
        public async Task<IActionResult> GetEmployeeOptions(
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

            var query = _dbContext.Set<MstEmployee>()
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
                    x.EmployeeCode.ToLower().Contains(keyword) ||
                    x.EmployeeNumber.ToLower().Contains(keyword) ||
                    x.FullName.ToLower().Contains(keyword) ||
                    (x.PrimaryDepartment != null && x.PrimaryDepartment.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.PrimaryPosition != null && x.PrimaryPosition.PositionName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();
            var defaultEmployeeProfilePhotoPath = GetDefaultUserProfilePhotoPath();

            var items = await query
                .OrderBy(x => x.FullName)
                .ThenBy(x => x.EmployeeCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new EmployeeOptionResponse
                {
                    Id = x.Id,
                    EmployeeCode = x.EmployeeCode,
                    EmployeeNumber = x.EmployeeNumber,
                    FullName = x.FullName,
                    ProfilePhotoPath = _dbContext.Users
                        .Where(u =>
                            u.EmployeeId == x.Id &&
                            u.UserType == UserType.Employee)
                        .OrderByDescending(u => u.IsActive)
                        .ThenByDescending(u => u.UpdateDateTime ?? u.CreateDateTime)
                        .Select(u => u.ProfilePhotoPath)
                        .FirstOrDefault() ?? defaultEmployeeProfilePhotoPath,
                    PrimaryDepartmentId = x.PrimaryDepartmentId,
                    PrimaryDepartmentName = x.PrimaryDepartment != null ? x.PrimaryDepartment.DepartmentName : string.Empty,
                    PrimaryPositionId = x.PrimaryPositionId,
                    PrimaryPositionName = x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : string.Empty
                })
                .ToListAsync();

            foreach (var item in items)
            {
                EnrichEmployeePhotoFields(item);
            }

            var result = new PagedResult<EmployeeOptionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PagedResult<EmployeeOptionResponse>>.Ok(
                result,
                "Data pilihan employee berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Employee",
            Description = "Melihat data employee",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Employee", "Read")]
        public async Task<IActionResult> GetEmployeeById(Guid id)
        {
            var defaultEmployeeProfilePhotoPath = GetDefaultUserProfilePhotoPath();

            var data = await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new EmployeeDetailResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    EmployeeCode = x.EmployeeCode,
                    EmployeeNumber = x.EmployeeNumber,
                    FullName = x.FullName,
                    NickName = x.NickName,
                    Gender = x.Gender,
                    BirthPlace = x.BirthPlace,
                    BirthDate = x.BirthDate,
                    Religion = x.Religion,
                    MaritalStatus = x.MaritalStatus,
                    BloodType = x.BloodType,
                    IdentityType = x.IdentityType,
                    IdentityNumber = x.IdentityNumber,
                    PhoneNumber = x.PhoneNumber,
                    WhatsAppNumber = x.WhatsAppNumber,
                    Email = x.Email,
                    ProfilePhotoPath = _dbContext.Users
                        .Where(u =>
                            u.EmployeeId == x.Id &&
                            u.UserType == UserType.Employee)
                        .OrderByDescending(u => u.IsActive)
                        .ThenByDescending(u => u.UpdateDateTime ?? u.CreateDateTime)
                        .Select(u => u.ProfilePhotoPath)
                        .FirstOrDefault() ?? defaultEmployeeProfilePhotoPath,
                    Address = x.Address,

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

                    EmployeeStatus = x.EmployeeStatus,
                    ProfessionType = x.ProfessionType,
                    EmploymentType = x.EmploymentType,
                    GradeLevel = x.GradeLevel,
                    WorkLocation = x.WorkLocation,
                    JoinDate = x.JoinDate,
                    ProbationEndDate = x.ProbationEndDate,
                    ContractStartDate = x.ContractStartDate,
                    ContractEndDate = x.ContractEndDate,
                    ResignDate = x.ResignDate,
                    ResignReason = x.ResignReason,
                    EmergencyContactName = x.EmergencyContactName,
                    EmergencyContactRelation = x.EmergencyContactRelation,
                    EmergencyContactPhone = x.EmergencyContactPhone,
                    EmergencyContactAddress = x.EmergencyContactAddress,
                    HasTransportAllowanceProfile = x.WorkforceProfileId != Guid.Empty &&
                        _dbContext.Set<WfpTransportAllowance>()
                            .Any(t =>
                                t.WorkforceProfileId == x.WorkforceProfileId &&
                                !t.IsDelete),
                    HasUserAccount = _dbContext.Users.Any(u =>
                        u.EmployeeId == x.Id &&
                        u.UserType == UserType.Employee),
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy != Guid.Empty ? x.CreateBy : null,
                    CreateByName = x.CreateBy != Guid.Empty
                        ? _dbContext.Users
                            .Where(u => u.Id == x.CreateBy)
                            .Select(u =>
                                u.DisplayName ??
                                u.UserName ??
                                u.Email ??
                                u.UserCode)
                            .FirstOrDefault()
                        : null,

                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy != Guid.Empty ? x.UpdateBy : null,
                    UpdateByName = x.UpdateBy != Guid.Empty
                        ? _dbContext.Users
                            .Where(u => u.Id == x.UpdateBy)
                            .Select(u =>
                                u.DisplayName ??
                                u.UserName ??
                                u.Email ??
                                u.UserCode)
                            .FirstOrDefault()
                        : null
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Employee tidak ditemukan."
                ));
            }

            EnrichEmployeePhotoFields(data);
            data.UserAccount = await BuildEmployeeUserAccountCompactResponseAsync(id);
            EnrichEmployeeUserAccountPhotoFields(data.UserAccount);
            data.ChildSummary = await BuildEmployeeChildSummaryAsync(id);

            return Ok(ApiResponse<EmployeeDetailResponse>.Ok(
                data,
                "Detail employee berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmployeeCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Employee",
            Description = "Membuat data employee",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("Employee", "Create")]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
        {
            var requiredValidation = ValidateRequiredEmployeeRequest(request);

            if (!requiredValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    requiredValidation.ErrorMessage ?? "Data wajib employee belum lengkap."
                ));
            }

            if (request.CreateLoginAccount)
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Email wajib diisi jika ingin membuat akun login."
                    ));
                }

                if (request.BirthDate == default)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "BirthDate wajib diisi untuk generate password awal akun login."
                    ));
                }
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

            var validation = await ValidateEmployeeRequestAsync(
                excludeEmployeeId: null,
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
                emergencyContactPhone: request.EmergencyContactPhone,
                email: request.Email
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data employee tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var userId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var workforceProfile = new MstWorkforceProfile
                {
                    Id = Guid.NewGuid(),
                    ProfileCode = await GenerateWorkforceProfileCodeAsync(),
                    UserType = UserType.Employee,
                    DisplayName = request.FullName.Trim(),
                    Email = request.Email.Trim().ToLowerInvariant(),
                    PhoneNumber = NormalizeDigitsOnly(request.PhoneNumber),
                    WhatsAppNumber = NormalizeDigitsOnly(request.WhatsAppNumber),
                    PrimaryDepartmentId = request.PrimaryDepartmentId,
                    PrimaryPositionId = request.PrimaryPositionId,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = userId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<MstWorkforceProfile>().Add(workforceProfile);
                await _dbContext.SaveChangesAsync();

                var entity = new MstEmployee
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfile.Id,
                    EmployeeCode = await GenerateEmployeeCodeAsync(),
                    EmployeeNumber = await GenerateEmployeeNumberAsync(request.JoinDate),
                    FullName = request.FullName.Trim(),
                    NickName = NormalizeNullableText(request.NickName),
                    Gender = request.Gender,
                    BirthPlace = NormalizeNullableText(request.BirthPlace),
                    BirthDate = request.BirthDate,
                    Religion = request.Religion,
                    MaritalStatus = request.MaritalStatus,
                    BloodType = request.BloodType,
                    IdentityType = request.IdentityType.Trim(),
                    IdentityNumber = NormalizeDigitsOnly(request.IdentityNumber) ?? string.Empty,
                    PhoneNumber = NormalizeDigitsOnly(request.PhoneNumber),
                    WhatsAppNumber = NormalizeDigitsOnly(request.WhatsAppNumber),
                    Email = request.Email.Trim().ToLowerInvariant(),
                    Address = NormalizeNullableText(request.Address),
                    CountryId = NormalizeNullableGuid(request.CountryId),
                    ProvinceId = NormalizeNullableGuid(request.ProvinceId),
                    CityId = NormalizeNullableGuid(request.CityId),
                    DistrictId = NormalizeNullableGuid(request.DistrictId),
                    PostalCodeId = NormalizeNullableGuid(request.PostalCodeId),
                    PrimaryDepartmentId = request.PrimaryDepartmentId,
                    PrimaryPositionId = request.PrimaryPositionId,
                    EmployeeStatus = request.EmployeeStatus,
                    ProfessionType = request.ProfessionType,
                    EmploymentType = request.EmploymentType,
                    GradeLevel = NormalizeNullableText(request.GradeLevel),
                    WorkLocation = NormalizeNullableText(request.WorkLocation),
                    JoinDate = request.JoinDate,
                    ProbationEndDate = request.ProbationEndDate,
                    ContractStartDate = request.ContractStartDate,
                    ContractEndDate = request.ContractEndDate,
                    EmergencyContactName = NormalizeNullableText(request.EmergencyContactName),
                    EmergencyContactRelation = NormalizeNullableText(request.EmergencyContactRelation),
                    EmergencyContactPhone = NormalizeDigitsOnly(request.EmergencyContactPhone),
                    EmergencyContactAddress = NormalizeNullableText(request.EmergencyContactAddress),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = userId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<MstEmployee>().Add(entity);
                await _dbContext.SaveChangesAsync();

                var organizationAssignment = new WfpOrganizationAssignment
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfile.Id,
                    DepartmentId = entity.PrimaryDepartmentId,
                    PositionId = entity.PrimaryPositionId,
                    IsPrimary = true,
                    IsActive = true,
                    EffectiveStartDate = entity.JoinDate.Date,
                    EffectiveEndDate = null,
                    Description = "Initial primary organization assignment",
                    CreateDateTime = now,
                    CreateBy = userId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<WfpOrganizationAssignment>().Add(organizationAssignment);
                await _dbContext.SaveChangesAsync();

                EmployeeLoginAccountResponse? loginAccount = null;

                if (request.CreateLoginAccount)
                {
                    var accountResult = await CreateLoginAccountForEmployeeAsync(
                        employee: entity,
                        isFingerprintRegistrationEnabled: request.IsFingerprintRegistrationEnabled,
                        fingerprintRegistrationReason: request.FingerprintRegistrationReason,
                        actorUserId: userId
                    );

                    if (!accountResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();

                        return BadRequest(ApiResponse<object>.Fail(
                            StatusCodes.Status400BadRequest,
                            accountResult.ErrorMessage ?? "Akun login employee gagal dibuat."
                        ));
                    }

                    loginAccount = accountResult.Response;

                    if (loginAccount?.UserId.HasValue == true)
                    {
                        await SyncUserPrimaryOrganizationAsync(
                            userId: loginAccount.UserId.Value,
                            departmentId: entity.PrimaryDepartmentId,
                            positionId: entity.PrimaryPositionId,
                            effectiveStartDate: entity.JoinDate,
                            actorUserId: userId
                        );

                        await _dbContext.SaveChangesAsync();
                    }
                }

                await transaction.CommitAsync();

                var response = new EmployeeCreateResponse
                {
                    Id = entity.Id,
                    WorkforceProfileId = entity.WorkforceProfileId,
                    EmployeeCode = entity.EmployeeCode,
                    EmployeeNumber = entity.EmployeeNumber,
                    FullName = entity.FullName,
                    IsActive = entity.IsActive,
                    LoginAccount = loginAccount
                };

                await _loggerService.InfoAsync(
                    LogCategory,
                    "Employee.CreateEmployee",
                    "Employee berhasil dibuat.",
                    new
                    {
                        entity.Id,
                        entity.WorkforceProfileId,
                        entity.EmployeeCode,
                        entity.EmployeeNumber,
                        entity.FullName,
                        entity.PrimaryDepartmentId,
                        entity.PrimaryPositionId,
                        LoginAccountCreated = loginAccount?.IsCreated ?? false,
                        loginAccount?.UserId,
                        loginAccount?.UserCode
                    }
                );

                return Ok(ApiResponse<EmployeeCreateResponse>.Ok(
                    response,
                    request.CreateLoginAccount
                        ? "Employee dan akun login berhasil dibuat."
                        : "Employee berhasil dibuat."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "Employee.CreateEmployee",
                    "Gagal membuat employee.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat employee."
                    )
                );
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Employee",
            Description = "Mengubah data employee",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Employee", "Update")]
        public async Task<IActionResult> UpdateEmployee(
            Guid id,
            [FromBody] UpdateEmployeeRequest request)
        {
            var entity = await _dbContext.Set<MstEmployee>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Employee tidak ditemukan."
                ));
            }

            var requiredValidation = ValidateRequiredEmployeeRequest(request);

            if (!requiredValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    requiredValidation.ErrorMessage ?? "Data wajib employee belum lengkap."
                ));
            }

            var validation = await ValidateEmployeeRequestAsync(
                excludeEmployeeId: id,
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
                emergencyContactPhone: request.EmergencyContactPhone,
                email: request.Email
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data employee tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var userId = GetCurrentUserId();

            entity.FullName = request.FullName.Trim();
            entity.NickName = NormalizeNullableText(request.NickName);
            entity.Gender = request.Gender;
            entity.BirthPlace = NormalizeNullableText(request.BirthPlace);
            entity.BirthDate = request.BirthDate;
            entity.Religion = request.Religion;
            entity.MaritalStatus = request.MaritalStatus;
            entity.BloodType = request.BloodType;
            entity.IdentityType = request.IdentityType.Trim();
            entity.IdentityNumber = NormalizeDigitsOnly(request.IdentityNumber) ?? string.Empty;
            entity.PhoneNumber = NormalizeDigitsOnly(request.PhoneNumber);
            entity.WhatsAppNumber = NormalizeDigitsOnly(request.WhatsAppNumber);
            entity.Email = request.Email.Trim().ToLowerInvariant();
            entity.Address = NormalizeNullableText(request.Address);
            entity.CountryId = NormalizeNullableGuid(request.CountryId);
            entity.ProvinceId = NormalizeNullableGuid(request.ProvinceId);
            entity.CityId = NormalizeNullableGuid(request.CityId);
            entity.DistrictId = NormalizeNullableGuid(request.DistrictId);
            entity.PostalCodeId = NormalizeNullableGuid(request.PostalCodeId);
            entity.PrimaryDepartmentId = request.PrimaryDepartmentId;
            entity.PrimaryPositionId = request.PrimaryPositionId;
            entity.EmployeeStatus = request.EmployeeStatus;
            entity.ProfessionType = request.ProfessionType;
            entity.EmploymentType = request.EmploymentType;
            entity.GradeLevel = NormalizeNullableText(request.GradeLevel);
            entity.WorkLocation = NormalizeNullableText(request.WorkLocation);
            entity.JoinDate = request.JoinDate;
            entity.ProbationEndDate = request.ProbationEndDate;
            entity.ContractStartDate = request.ContractStartDate;
            entity.ContractEndDate = request.ContractEndDate;
            entity.ResignDate = request.ResignDate;
            entity.ResignReason = NormalizeNullableText(request.ResignReason);
            entity.EmergencyContactName = NormalizeNullableText(request.EmergencyContactName);
            entity.EmergencyContactRelation = NormalizeNullableText(request.EmergencyContactRelation);
            entity.EmergencyContactPhone = NormalizeDigitsOnly(request.EmergencyContactPhone);
            entity.EmergencyContactAddress = NormalizeNullableText(request.EmergencyContactAddress);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = userId;

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
                    workforceProfile.UpdateBy = userId;
                }
            }

            var linkedUser = await _dbContext.Users.FirstOrDefaultAsync(x =>
                x.EmployeeId == entity.Id &&
                x.UserType == UserType.Employee);

            if (linkedUser != null)
            {
                linkedUser.DisplayName = entity.FullName;
                linkedUser.Email = entity.Email;
                linkedUser.UserName = entity.Email;
                linkedUser.PrimaryDepartmentId = entity.PrimaryDepartmentId;
                linkedUser.PrimaryPositionId = entity.PrimaryPositionId;
                linkedUser.WorkforceProfileId = entity.WorkforceProfileId;
                linkedUser.IsActive = entity.IsActive;
                linkedUser.UpdateDateTime = now;
            }

            await EnsureWorkforcePrimaryOrganizationAssignmentAsync(entity, now, userId);

            if (linkedUser != null)
            {
                await SyncUserPrimaryOrganizationAsync(
                    userId: linkedUser.Id,
                    departmentId: entity.PrimaryDepartmentId,
                    positionId: entity.PrimaryPositionId,
                    effectiveStartDate: entity.JoinDate,
                    actorUserId: userId
                );
            }

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Employee.UpdateEmployee",
                "Employee berhasil diperbarui.",
                new
                {
                    entity.Id,
                    entity.WorkforceProfileId,
                    entity.EmployeeCode,
                    entity.EmployeeNumber,
                    entity.FullName,
                    entity.IsActive
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Employee berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Employee",
            Description = "Mengubah data employee",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Employee", "Update")]
        public async Task<IActionResult> UpdateEmployeeStatus(
            Guid id,
            [FromBody] UpdateEmployeeStatusRequest request)
        {
            var entity = await _dbContext.Set<MstEmployee>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Employee tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var userId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.ResignDate = request.ResignDate;
            entity.ResignReason = NormalizeNullableText(request.ResignReason);
            entity.UpdateDateTime = now;
            entity.UpdateBy = userId;

            if (entity.WorkforceProfileId != Guid.Empty)
            {
                var workforceProfile = await _dbContext.Set<MstWorkforceProfile>()
                    .FirstOrDefaultAsync(x => x.Id == entity.WorkforceProfileId && !x.IsDelete);

                if (workforceProfile != null)
                {
                    workforceProfile.IsActive = request.IsActive;
                    workforceProfile.UpdateDateTime = now;
                    workforceProfile.UpdateBy = userId;
                }
            }

            var linkedUser = await _dbContext.Users.FirstOrDefaultAsync(x =>
                x.EmployeeId == id &&
                x.UserType == UserType.Employee);

            if (linkedUser != null)
            {
                linkedUser.IsActive = request.IsActive;
                linkedUser.UpdateDateTime = now;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status employee berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/user-account/geolocation-bypass")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Employee",
            Description = "Mengubah pengaturan geolocation bypass akun employee",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Employee", "Update")]
        public async Task<IActionResult> UpdateEmployeeUserGeolocationBypass(
            Guid id, [FromBody] UpdateEmployeeUserGeolocationBypassRequest request)
        {
            var employeeExists = await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == id && !x.IsDelete);

            if (!employeeExists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Employee tidak ditemukan."
                ));
            }

            var linkedUser = await _dbContext.Users.FirstOrDefaultAsync(x =>
                x.EmployeeId == id &&
                x.UserType == UserType.Employee);

            if (linkedUser == null)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Employee belum memiliki akun login, sehingga geolocation bypass tidak dapat diubah."
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
                "Employee.UpdateEmployeeUserGeolocationBypass",
                "Pengaturan geolocation bypass akun employee berhasil diperbarui.",
                new
                {
                    EmployeeId = id,
                    UserId = linkedUser.Id,
                    linkedUser.UserName,
                    linkedUser.Email,
                    linkedUser.IsGeolocationBypassEnabled,
                    linkedUser.GeolocationBypassUntil,
                    linkedUser.GeolocationBypassReason
                }
            );

            return Ok(ApiResponse<object>.Ok(
                new
                {
                    EmployeeId = id,
                    UserId = linkedUser.Id,
                    linkedUser.IsGeolocationBypassEnabled,
                    linkedUser.GeolocationBypassUntil,
                    linkedUser.GeolocationBypassReason
                },
                request.IsGeolocationBypassEnabled
                    ? "Geolocation bypass akun employee berhasil diaktifkan."
                    : "Geolocation bypass akun employee berhasil dinonaktifkan."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Employee",
            Description = "Menghapus data employee",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("Employee", "Delete")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            var entity = await _dbContext.Set<MstEmployee>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Employee tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var userId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = userId;

            if (entity.WorkforceProfileId != Guid.Empty)
            {
                var workforceProfile = await _dbContext.Set<MstWorkforceProfile>()
                    .FirstOrDefaultAsync(x => x.Id == entity.WorkforceProfileId && !x.IsDelete);

                if (workforceProfile != null)
                {
                    workforceProfile.IsActive = false;
                    workforceProfile.UpdateDateTime = now;
                    workforceProfile.UpdateBy = userId;
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
                    item.DeleteBy = userId;
                }
            }

            var linkedUser = await _dbContext.Users.FirstOrDefaultAsync(x =>
                x.EmployeeId == id &&
                x.UserType == UserType.Employee);

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
                    item.DeleteBy = userId;
                }
            }

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Employee berhasil dihapus."
            ));
        }

        private async Task<string> GenerateEmployeeCodeAsync()
        {
            const string menuCode = "EMP";
            const string prefix = $"{menuCode}-{HospitalCode}-";

            var existingCount = await _dbContext.Set<MstEmployee>()
                .IgnoreQueryFilters()
                .CountAsync(x => x.EmployeeCode.StartsWith(prefix));

            var nextNumber = existingCount + 1;

            while (true)
            {
                var code = $"{prefix}{nextNumber:D5}";

                var exists = await _dbContext.Set<MstEmployee>()
                    .IgnoreQueryFilters()
                    .AnyAsync(x => x.EmployeeCode == code);

                if (!exists)
                {
                    return code;
                }

                nextNumber++;
            }
        }

        private async Task<string> GenerateEmployeeNumberAsync(DateTime joinDate)
        {
            var date = joinDate.Date;
            var dateCode = date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            var prefix = $"{HospitalCode}-{dateCode}";

            var existingCount = await _dbContext.Set<MstEmployee>()
                .IgnoreQueryFilters()
                .CountAsync(x => x.EmployeeNumber.StartsWith(prefix));

            var nextNumber = existingCount + 1;

            while (true)
            {
                var employeeNumber = $"{prefix}{nextNumber.ToString("D3")}";

                var exists = await _dbContext.Set<MstEmployee>()
                    .AnyAsync(x => x.EmployeeNumber == employeeNumber);

                if (!exists)
                {
                    return employeeNumber;
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

        private async Task<EmployeeUserAccountCompactResponse?> BuildEmployeeUserAccountCompactResponseAsync(Guid employeeId)
        {
            var now = DateTime.UtcNow;

            var user = await _dbContext.Users
                .AsNoTracking()
                .Where(x =>
                    x.EmployeeId == employeeId &&
                    x.UserType == UserType.Employee)
                .Select(x => new EmployeeUserAccountCompactResponse
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
                EnrichEmployeeUserAccountPhotoFields(user);
                return user;
            }

            return new EmployeeUserAccountCompactResponse
            {
                IsAvailable = false,
                IsActive = false,
                MustChangePassword = false,
                IsGeolocationBypassEnabled = false,
                IsGeolocationBypassActive = false
            };
        }

        private async Task<EmployeeChildSummaryResponse> BuildEmployeeChildSummaryAsync(Guid employeeId)
        {
            var employee = await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .Where(x => x.Id == employeeId && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.WorkforceProfileId
                })
                .FirstOrDefaultAsync();

            if (employee == null || employee.WorkforceProfileId == Guid.Empty)
            {
                return new EmployeeChildSummaryResponse();
            }

            var workforceProfileId = employee.WorkforceProfileId;

            var organizationQuery = _dbContext.Set<WfpOrganizationAssignment>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            return new EmployeeChildSummaryResponse
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

                PayrollProfileCount = await _dbContext.Set<WfpPayroll>()
                    .AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete),

                TaxProfileCount = await _dbContext.Set<WfpTax>()
                    .AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete),

                InsuranceProfileCount = await _dbContext.Set<WfpInsurance>()
                    .AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete),

                TransportAllowanceProfileCount = await _dbContext.Set<WfpTransportAllowance>()
                    .AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete),

                TransportAllowanceTransactionCount = await _dbContext.Set<WfpTransportAllowanceTransaction>()
                    .AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete),

                LeaveCount = 0,

                ShiftAssignmentCount = await _dbContext.Set<WfpWorkScheduleAssignment>()
                    .AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete),

                AttendanceCount = await _dbContext.Set<EmpAttendance>()
                    .AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete),

                SalaryCount = 0,

                TrainingCount = await _dbContext.Set<WfpTrainingRecord>()
                    .AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete),

                WarningLetterCount = 0,
                PerformanceReviewCount = 0
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateEmployeeRequestAsync(
            Guid? excludeEmployeeId,
            Guid primaryDepartmentId,
            Guid primaryPositionId,
            Guid? countryId,
            Guid? provinceId,
            Guid? cityId,
            Guid? districtId,
            Guid? postalCodeId,
            string? identityNumber,
            string? phoneNumber,
            string? whatsAppNumber,
            string? emergencyContactPhone,
            string? email)
        {
            if (primaryDepartmentId == Guid.Empty)
            {
                return (false, "Primary department wajib dipilih.");
            }

            if (primaryPositionId == Guid.Empty)
            {
                return (false, "Primary position wajib dipilih.");
            }

            var departmentExists = await _dbContext.MstDepartments
                .AsNoTracking()
                .AnyAsync(x => x.Id == primaryDepartmentId && x.IsActive && !x.IsDelete);

            if (!departmentExists)
            {
                return (false, "Primary department tidak valid atau tidak aktif.");
            }

            var positionExists = await _dbContext.MstPositions
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == primaryPositionId &&
                    x.DepartmentId == primaryDepartmentId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!positionExists)
            {
                return (false, "Primary position tidak valid, tidak aktif, atau tidak sesuai department.");
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

            var normalizedIdentityNumber = NormalizeDigitsOnly(identityNumber);
            var normalizedPhoneNumber = NormalizeDigitsOnly(phoneNumber);
            var normalizedWhatsAppNumber = NormalizeDigitsOnly(whatsAppNumber);
            var normalizedEmergencyContactPhone = NormalizeDigitsOnly(emergencyContactPhone);
            var normalizedEmail = NormalizeNullableText(email);

            if (!string.IsNullOrWhiteSpace(normalizedIdentityNumber) &&
                normalizedIdentityNumber.Length != 16)
            {
                return (false, "Nomor identitas harus 16 digit.");
            }

            if (!string.IsNullOrWhiteSpace(normalizedPhoneNumber) &&
                normalizedPhoneNumber.Length > 13)
            {
                return (false, "Nomor telepon maksimal 13 digit.");
            }

            if (!string.IsNullOrWhiteSpace(normalizedWhatsAppNumber) &&
                normalizedWhatsAppNumber.Length > 13)
            {
                return (false, "Nomor WhatsApp maksimal 13 digit.");
            }

            if (!string.IsNullOrWhiteSpace(normalizedEmergencyContactPhone) &&
                normalizedEmergencyContactPhone.Length > 13)
            {
                return (false, "Nomor telepon kontak darurat maksimal 13 digit.");
            }

            if (!string.IsNullOrWhiteSpace(normalizedIdentityNumber))
            {
                var exists = await _dbContext.Set<MstEmployee>()
                    .AnyAsync(x =>
                        x.Id != excludeEmployeeId &&
                        x.IdentityNumber == normalizedIdentityNumber &&
                        !x.IsDelete);

                if (exists)
                {
                    return (false, "Nomor identitas sudah digunakan.");
                }
            }

            if (!string.IsNullOrWhiteSpace(normalizedEmail))
            {
                var emailLower = normalizedEmail.ToLower();

                var exists = await _dbContext.Set<MstEmployee>()
                    .AnyAsync(x =>
                        x.Id != excludeEmployeeId &&
                        x.Email != null &&
                        x.Email.ToLower() == emailLower &&
                        !x.IsDelete);

                if (exists)
                {
                    return (false, "Email sudah digunakan.");
                }
            }

            return (true, null);
        }

        private async Task EnsureWorkforcePrimaryOrganizationAssignmentAsync(
    MstEmployee employee,
    DateTime now,
    Guid actorUserId)
        {
            if (employee.WorkforceProfileId == Guid.Empty)
            {
                return;
            }

            var workforceProfileId = employee.WorkforceProfileId;

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
                    x.DepartmentId == employee.PrimaryDepartmentId &&
                    x.PositionId == employee.PrimaryPositionId);

            if (existing == null)
            {
                existing = new WfpOrganizationAssignment
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    DepartmentId = employee.PrimaryDepartmentId,
                    PositionId = employee.PrimaryPositionId,
                    IsPrimary = true,
                    IsActive = employee.IsActive,
                    EffectiveStartDate = employee.JoinDate.Date,
                    EffectiveEndDate = null,
                    Description = "Primary organization assignment synced from employee update",
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
                existing.IsActive = employee.IsActive;
                existing.EffectiveStartDate = employee.JoinDate.Date;
                existing.EffectiveEndDate = null;
                existing.Description = "Primary organization assignment synced from employee update";
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

        private static IOrderedQueryable<MstEmployee> ApplyEmployeeSorting(
            IQueryable<MstEmployee> query,
            string? sortBy,
            string? sortDirection)
        {
            var field = NormalizeSortBy(sortBy);
            var desc = IsDescending(sortDirection);

            return field switch
            {
                "employeecode" => desc
                    ? query.OrderByDescending(x => x.EmployeeCode).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.EmployeeCode).ThenBy(x => x.FullName),

                "employeenumber" => desc
                    ? query.OrderByDescending(x => x.EmployeeNumber).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.EmployeeNumber).ThenBy(x => x.FullName),

                "fullname" => desc
                    ? query.OrderByDescending(x => x.FullName).ThenBy(x => x.EmployeeCode)
                    : query.OrderBy(x => x.FullName).ThenBy(x => x.EmployeeCode),

                "departmentname" => desc
                    ? query.OrderByDescending(x => x.PrimaryDepartment != null ? x.PrimaryDepartment.DepartmentName : string.Empty).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.PrimaryDepartment != null ? x.PrimaryDepartment.DepartmentName : string.Empty).ThenBy(x => x.FullName),

                "positionname" => desc
                    ? query.OrderByDescending(x => x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : string.Empty).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : string.Empty).ThenBy(x => x.FullName),

                "joindate" => desc
                    ? query.OrderByDescending(x => x.JoinDate).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.JoinDate).ThenBy(x => x.FullName),

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

        private static List<EmployeeCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<EmployeeCustomPeriodOptionResponse>
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

        private static List<EmployeeQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<EmployeeQueryParameterInfoResponse>
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
                    Description = "Filter status aktif employee.",
                    Example = "true"
                },
                new()
                {
                    Name = "search",
                    Type = "string",
                    Description = "Pencarian employee code, employee number, nama, email, phone, department, dan position.",
                    Example = "budi"
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

        private static List<EmployeeFormFieldMetadataResponse> BuildCreateEmployeeFieldMetadata()
        {
            return BuildEmployeeFieldMetadata(isUpdate: false);
        }

        private static List<EmployeeFormFieldMetadataResponse> BuildUpdateEmployeeFieldMetadata()
        {
            var fields = BuildEmployeeFieldMetadata(isUpdate: true);

            fields.Add(new EmployeeFormFieldMetadataResponse
            {
                Name = "isActive",
                Label = "Status Aktif",
                Section = "Status",
                InputType = "boolean",
                IsRequiredOnCreate = false,
                IsRequiredOnUpdate = true,
                RequiredType = "Required",
                Description = "Status aktif employee.",
                Example = "true",
                SortOrder = 900
            });

            fields.Add(new EmployeeFormFieldMetadataResponse
            {
                Name = "resignDate",
                Label = "Tanggal Resign",
                Section = "Status",
                InputType = "date",
                IsRequiredOnCreate = false,
                IsRequiredOnUpdate = false,
                RequiredType = "Optional",
                Description = "Diisi jika employee resign atau dinonaktifkan.",
                Example = "2026-05-07",
                SortOrder = 901
            });

            fields.Add(new EmployeeFormFieldMetadataResponse
            {
                Name = "resignReason",
                Label = "Alasan Resign",
                Section = "Status",
                InputType = "textarea",
                IsRequiredOnCreate = false,
                IsRequiredOnUpdate = false,
                RequiredType = "Optional",
                MaxLength = 250,
                DependsOn = "resignDate",
                Description = "Alasan resign atau nonaktif.",
                Example = "Mengundurkan diri.",
                SortOrder = 902
            });

            return fields.OrderBy(x => x.SortOrder).ToList();
        }

        private static List<EmployeeFormFieldMetadataResponse> BuildEmployeeFieldMetadata(bool isUpdate)
        {
            return new List<EmployeeFormFieldMetadataResponse>
            {
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
                    Description = "Nama lengkap employee.",
                    Example = "Maya Lestari",
                    SortOrder = 1
                },
                new()
                {
                    Name = "nickName",
                    Label = "Nama Panggilan",
                    Section = "Identitas Utama",
                    InputType = "text",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 100,
                    Example = "Maya",
                    SortOrder = 2
                },
                new()
                {
                    Name = "gender",
                    Label = "Jenis Kelamin",
                    Section = "Identitas Utama",
                    InputType = "select",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    OptionsSource = "genderOptions",
                    Example = "2",
                    SortOrder = 3
                },
                new()
                {
                    Name = "birthPlace",
                    Label = "Tempat Lahir",
                    Section = "Identitas Utama",
                    InputType = "text",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 100,
                    Example = "Bandung",
                    SortOrder = 4
                },
                new()
                {
                    Name = "birthDate",
                    Label = "Tanggal Lahir",
                    Section = "Identitas Utama",
                    InputType = "date",
                    IsRequiredOnCreate = true,
                    IsRequiredOnUpdate = true,
                    RequiredType = "Required",
                    Description = "Tanggal lahir wajib diisi. Digunakan juga untuk password awal akun login dengan format ddMMMyyyy.",
                    Example = "2025-01-01",
                    SortOrder = 5
                },
                new()
                {
                    Name = "religion",
                    Label = "Agama",
                    Section = "Identitas Utama",
                    InputType = "select",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    OptionsSource = "religionOptions",
                    Example = "1",
                    SortOrder = 6
                },
                new()
                {
                    Name = "maritalStatus",
                    Label = "Status Pernikahan",
                    Section = "Identitas Utama",
                    InputType = "select",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    OptionsSource = "maritalStatusOptions",
                    Example = "1",
                    SortOrder = 7
                },
                new()
                {
                    Name = "bloodType",
                    Label = "Golongan Darah",
                    Section = "Identitas Utama",
                    InputType = "select",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    OptionsSource = "bloodTypeOptions",
                    Example = "1",
                    SortOrder = 8
                },
                new()
                {
                    Name = "identityType",
                    Label = "Tipe Identitas",
                    Section = "Identitas Legal & Kontak",
                    InputType = "text",
                    IsRequiredOnCreate = true,
                    IsRequiredOnUpdate = true,
                    RequiredType = "Required",
                    MaxLength = 50,
                    Example = "KTP",
                    SortOrder = 101
                },
                new()
                {
                    Name = "identityNumber",
                    Label = "Nomor Identitas",
                    Section = "Identitas Legal & Kontak",
                    InputType = "text",
                    IsRequiredOnCreate = true,
                    IsRequiredOnUpdate = true,
                    RequiredType = "Required",
                    MaxLength = 16,
                    ValidationRule = "digits:16;unique",
                    Description = "Nomor identitas wajib 16 digit dan harus unik.",
                    Example = "3273010101250005",
                    SortOrder = 102
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
                    MaxLength = 13,
                    ValidationRule = "digits;max:13",
                    Description = "Nomor maksimal 13 digit.",
                    SortOrder = 103
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
                    MaxLength = 13,
                    ValidationRule = "digits;max:13",
                    Description = "Nomor maksimal 13 digit.",
                    Example = "0812345678902",
                    SortOrder = 104
                },
                new()
                {
                    Name = "email",
                    Label = "Email",
                    Section = "Identitas Legal & Kontak",
                    InputType = "email",
                    IsRequiredOnCreate = true,
                    IsRequiredOnUpdate = true,
                    RequiredType = "Required",
                    MaxLength = 200,
                    ValidationRule = "email;unique",
                    Description = "Email wajib diisi dan harus unik. Digunakan juga sebagai username login.",
                    Example = "maya@admin.com",
                    SortOrder = 105
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
                    Example = "Jl. Kenanga No. 8",
                    SortOrder = 106
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
                    SortOrder = 201
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
                    Description = "Wajib jika cityId diisi. Harus sesuai countryId.",
                    SortOrder = 202
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
                    Description = "Wajib jika districtId diisi. Harus sesuai provinceId.",
                    SortOrder = 203
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
                    Description = "Wajib jika postalCodeId diisi. Harus sesuai cityId.",
                    SortOrder = 204
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
                    Description = "Jika diisi harus sesuai districtId.",
                    SortOrder = 205
                },
                new()
                {
                    Name = "primaryDepartmentId",
                    Label = "Department Utama",
                    Section = "Organisasi",
                    InputType = "select",
                    IsRequiredOnCreate = true,
                    IsRequiredOnUpdate = true,
                    RequiredType = "Required",
                    OptionsSource = "organization.departments.options",
                    Description = "Department utama wajib dipilih.",
                    SortOrder = 301
                },
                new()
                {
                    Name = "primaryPositionId",
                    Label = "Position Utama",
                    Section = "Organisasi",
                    InputType = "select",
                    IsRequiredOnCreate = true,
                    IsRequiredOnUpdate = true,
                    RequiredType = "Required",
                    DependsOn = "primaryDepartmentId",
                    OptionsSource = "organization.positions.options",
                    Description = "Position utama wajib dipilih dan harus sesuai department.",
                    SortOrder = 302
                },
                new()
                {
                    Name = "employeeStatus",
                    Label = "Status Employee",
                    Section = "Employment",
                    InputType = "select",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    OptionsSource = "employeeStatusOptions",
                    Description = "Default backend adalah Contract.",
                    Example = "1",
                    SortOrder = 401
                },
                new()
                {
                    Name = "professionType",
                    Label = "Tipe Profesi",
                    Section = "Employment",
                    InputType = "select",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    OptionsSource = "professionTypeOptions",
                    Description = "Default backend adalah GeneralStaff.",
                    Example = "1",
                    SortOrder = 402
                },
                new()
                {
                    Name = "employmentType",
                    Label = "Tipe Pekerjaan",
                    Section = "Employment",
                    InputType = "select",
                    IsRequiredOnCreate = true,
                    IsRequiredOnUpdate = true,
                    RequiredType = "Required",
                    OptionsSource = "employmentTypeOptions",
                    Description = "Default backend adalah Contract.",
                    Example = "2",
                    SortOrder = 403
                },
                new()
                {
                    Name = "gradeLevel",
                    Label = "Grade Level",
                    Section = "Employment",
                    InputType = "text",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 50,
                    Example = "Staff",
                    SortOrder = 404
                },
                new()
                {
                    Name = "workLocation",
                    Label = "Lokasi Kerja",
                    Section = "Employment",
                    InputType = "text",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 50,
                    Example = "RSMMC",
                    SortOrder = 405
                },
                new()
                {
                    Name = "joinDate",
                    Label = "Tanggal Bergabung",
                    Section = "Employment",
                    InputType = "date",
                    IsRequiredOnCreate = true,
                    IsRequiredOnUpdate = true,
                    RequiredType = "Required",
                    Example = "2026-05-07",
                    SortOrder = 406
                },
                new()
                {
                    Name = "probationEndDate",
                    Label = "Tanggal Selesai Probation",
                    Section = "Employment",
                    InputType = "date",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    Example = "2026-08-07",
                    SortOrder = 407
                },
                new()
                {
                    Name = "contractStartDate",
                    Label = "Tanggal Mulai Kontrak",
                    Section = "Employment",
                    InputType = "date",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    Example = "2026-05-07",
                    SortOrder = 408
                },
                new()
                {
                    Name = "contractEndDate",
                    Label = "Tanggal Akhir Kontrak",
                    Section = "Employment",
                    InputType = "date",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    Example = "2027-05-06",
                    SortOrder = 409
                },
                new()
                {
                    Name = "emergencyContactName",
                    Label = "Nama Kontak Darurat",
                    Section = "Emergency Contact",
                    InputType = "text",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 200,
                    Example = "Hendra Lestari",
                    SortOrder = 501
                },
                new()
                {
                    Name = "emergencyContactRelation",
                    Label = "Hubungan Kontak Darurat",
                    Section = "Emergency Contact",
                    InputType = "text",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 50,
                    Example = "Ayah",
                    SortOrder = 502
                },
                new()
                {
                    Name = "emergencyContactPhone",
                    Label = "Telepon Kontak Darurat",
                    Section = "Emergency Contact",
                    InputType = "text",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 13,
                    ValidationRule = "digits:13",
                    Example = "0812345678912",
                    SortOrder = 503
                },
                new()
                {
                    Name = "emergencyContactAddress",
                    Label = "Alamat Kontak Darurat",
                    Section = "Emergency Contact",
                    InputType = "textarea",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 500,
                    Example = "Jl. Kenanga No. 8",
                    SortOrder = 504
                }
            }
            .OrderBy(x => x.SortOrder)
            .ToList();
        }

        private static List<EmployeeEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : struct, System.Enum
        {
            return System.Enum.GetValues<TEnum>()
                .Select(x => new EmployeeEnumOptionResponse
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
                "Active" => "Aktif",
                "Inactive" => "Tidak Aktif",
                "Probation" => "Probation",
                "Suspended" => "Suspended",
                "Resigned" => "Resign",
                "Terminated" => "Diberhentikan",
                "Retired" => "Pensiun",
                "ContractEnded" => "Kontrak Berakhir",

                "Permanent" => "Permanent",
                "Contract" => "Kontrak",
                "Internship" => "Magang",
                "PartTime" => "Paruh Waktu",
                "Outsourced" => "Outsourced",
                "DailyWorker" => "Harian",
                "Consultant" => "Konsultan",
                "Volunteer" => "Relawan",

                "GeneralStaff" => "Staff Umum",
                "Nurse" => "Perawat",
                "Midwife" => "Bidan",
                "Pharmacist" => "Apoteker",
                "PharmacyTechnician" => "Tenaga Teknis Kefarmasian",
                "LaboratoryAnalyst" => "Analis Laboratorium",
                "Radiographer" => "Radiografer",
                "Physiotherapist" => "Fisioterapis",
                "Nutritionist" => "Ahli Gizi",
                "MedicalRecordStaff" => "Rekam Medis",
                "BillingStaff" => "Billing",
                "Cashier" => "Kasir",
                "Administrator" => "Administrator",
                "ITStaff" => "IT Staff",
                "FinanceStaff" => "Finance Staff",
                "HRStaff" => "HR Staff",
                "Security" => "Security",
                "CleaningService" => "Cleaning Service",
                "Driver" => "Driver",
                "Maintenance" => "Maintenance",

                "Unknown" => "Tidak Diketahui",
                "Male" => "Laki-laki",
                "Female" => "Perempuan",

                "Islam" => "Islam",
                "ProtestantChristian" => "Kristen Protestan",
                "CatholicChristian" => "Kristen Katolik",
                "Hindu" => "Hindu",
                "Buddhist" => "Buddha",
                "Confucian" => "Konghucu",
                "Other" => "Lainnya",

                "Single" => "Belum Menikah",
                "Married" => "Menikah",
                "Divorced" => "Cerai Hidup",
                "Widowed" => "Cerai Mati",
                "Separated" => "Pisah",

                "APositive" => "A+",
                "ANegative" => "A-",
                "BPositive" => "B+",
                "BNegative" => "B-",
                "ABPositive" => "AB+",
                "ABNegative" => "AB-",
                "OPositive" => "O+",
                "ONegative" => "O-",
                "NotDisclosed" => "Tidak Diinformasikan",
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

        private async Task<(bool IsSuccess, string? ErrorMessage, EmployeeLoginAccountResponse? Response)>
            CreateLoginAccountForEmployeeAsync(
                MstEmployee employee,
                bool isFingerprintRegistrationEnabled,
                string? fingerprintRegistrationReason,
                Guid actorUserId)
        {
            if (employee.Id == Guid.Empty)
            {
                return (false, "EmployeeId tidak valid.", null);
            }

            if (string.IsNullOrWhiteSpace(employee.Email))
            {
                return (false, "Email employee wajib diisi untuk membuat akun login.", null);
            }

            if (employee.BirthDate == default)
            {
                return (false, "BirthDate employee wajib diisi untuk generate password awal.", null);
            }

            var email = employee.Email.Trim().ToLower();

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

            var existingEmployeeAccount = await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(x =>
                    x.EmployeeId == employee.Id &&
                    x.UserType == UserType.Employee);

            if (existingEmployeeAccount)
            {
                return (false, "Employee ini sudah memiliki akun login.", null);
            }

            var initialPassword = GenerateInitialPasswordFromBirthDate(employee.BirthDate);

            if (string.IsNullOrWhiteSpace(initialPassword))
            {
                return (false, "Password awal gagal dibuat dari BirthDate.", null);
            }

            var now = DateTime.UtcNow;

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserCode = await GenerateUserCodeAsync(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = employee.FullName,
                UserType = UserType.Employee,
                WorkforceProfileId = employee.WorkforceProfileId,
                EmployeeId = employee.Id,
                DoctorId = null,
                ExternalUserId = null,
                PrimaryDepartmentId = employee.PrimaryDepartmentId,
                PrimaryPositionId = employee.PrimaryPositionId,
                IsGeolocationBypassEnabled = false,
                GeolocationBypassReason = null,
                GeolocationBypassUntil = null,
                IsActive = employee.IsActive,
                MustChangePassword = true,
                AccessValidUntil = null,
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

            var response = new EmployeeLoginAccountResponse
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
                Message = "Akun login employee berhasil dibuat. Password awal wajib diganti saat login pertama."
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
                    Description = "Synced from employee primary organization assignment",
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
                existing.Description = "Synced from employee primary organization assignment";
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

        private static string GenerateInitialPasswordFromBirthDate(DateTime birthDate)
        {
            return birthDate.ToString("ddMMMyyyy", CultureInfo.InvariantCulture);
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

        private void EnrichEmployeePhotoFields(EmployeeResponse response)
        {
            response.ProfilePhotoPath = ResolveUserProfilePhotoPath(response.ProfilePhotoPath);
            response.ProfilePhotoUrl = BuildPublicFileUrl(response.ProfilePhotoPath);
            response.EmployeePhotoPath = response.ProfilePhotoPath;
            response.EmployeePhotoUrl = response.ProfilePhotoUrl;
        }

        private void EnrichEmployeePhotoFields(EmployeeOptionResponse response)
        {
            response.ProfilePhotoPath = ResolveUserProfilePhotoPath(response.ProfilePhotoPath);
            response.ProfilePhotoUrl = BuildPublicFileUrl(response.ProfilePhotoPath);
            response.EmployeePhotoPath = response.ProfilePhotoPath;
            response.EmployeePhotoUrl = response.ProfilePhotoUrl;
        }

        private void EnrichEmployeeUserAccountPhotoFields(EmployeeUserAccountCompactResponse? response)
        {
            if (response == null)
            {
                return;
            }

            response.ProfilePhotoPath = ResolveUserProfilePhotoPath(response.ProfilePhotoPath);
            response.ProfilePhotoUrl = BuildPublicFileUrl(response.ProfilePhotoPath);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateRequiredEmployeeRequest(CreateEmployeeRequest request)
        {
            return ValidateRequiredEmployeeFields(
                fullName: request.FullName,
                birthDate: request.BirthDate,
                identityType: request.IdentityType,
                identityNumber: request.IdentityNumber,
                email: request.Email,
                primaryDepartmentId: request.PrimaryDepartmentId,
                primaryPositionId: request.PrimaryPositionId,
                employmentType: request.EmploymentType,
                joinDate: request.JoinDate
            );
        }

        private static (bool IsValid, string? ErrorMessage) ValidateRequiredEmployeeRequest(UpdateEmployeeRequest request)
        {
            return ValidateRequiredEmployeeFields(
                fullName: request.FullName,
                birthDate: request.BirthDate,
                identityType: request.IdentityType,
                identityNumber: request.IdentityNumber,
                email: request.Email,
                primaryDepartmentId: request.PrimaryDepartmentId,
                primaryPositionId: request.PrimaryPositionId,
                employmentType: request.EmploymentType,
                joinDate: request.JoinDate
            );
        }

        private static (bool IsValid, string? ErrorMessage) ValidateRequiredEmployeeFields(
    string? fullName,
    DateTime birthDate,
    string? identityType,
    string? identityNumber,
    string? email,
    Guid primaryDepartmentId,
    Guid primaryPositionId,
    EmploymentType employmentType,
    DateTime joinDate)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return (false, "Nama employee wajib diisi.");
            }

            if (birthDate == default)
            {
                return (false, "Tanggal lahir wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(identityType))
            {
                return (false, "Tipe identitas wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(identityNumber))
            {
                return (false, "Nomor identitas wajib diisi.");
            }

            if (NormalizeDigitsOnly(identityNumber)?.Length != 16)
            {
                return (false, "Nomor identitas harus 16 digit.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return (false, "Email wajib diisi.");
            }

            if (primaryDepartmentId == Guid.Empty)
            {
                return (false, "Primary department wajib dipilih.");
            }

            if (primaryPositionId == Guid.Empty)
            {
                return (false, "Primary position wajib dipilih.");
            }

            if (employmentType == EmploymentType.Unknown)
            {
                return (false, "Employment type wajib dipilih.");
            }

            if (joinDate == default)
            {
                return (false, "Tanggal bergabung wajib diisi.");
            }

            return (true, null);
        }
    }
}