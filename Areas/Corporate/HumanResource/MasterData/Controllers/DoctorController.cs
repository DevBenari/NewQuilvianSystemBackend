using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Attendance.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
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

using ResponseDoctorPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs.DoctorResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/doctors")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Doctor",
        AreaName = "Corporate",
        ControllerName = "Doctor",
        Description = "Corporate human resource master data doctor",
        SortOrder = 5
    )]
    [Tags("Corporate / Human Resource / Master Data / Doctor")]
    public class DoctorController : ControllerBase
    {
        private const string DefaultDoctorProfilePhotoPathFallback = "/uploads/default-profile-photos/dokter.png";
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string HospitalCode = "RSMMC";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public DoctorController(
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

        private static List<DoctorDetailTabMetadataResponse> BuildDoctorDetailTabs()
        {
            return new List<DoctorDetailTabMetadataResponse>
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
                    Key = "credentialLicense",
                    Label = "Credential License",
                    Icon = "license",
                    Endpoint = "/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/credential-licenses",
                    IsVisibleInDetail = true,
                    IsVisibleInUpdate = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true,
                    SortOrder = 2
                },
                new()
                {
                    Key = "clinicalPrivilege",
                    Label = "Clinical Privilege",
                    Icon = "privilege",
                    Endpoint = "/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/clinical-privileges",
                    IsVisibleInDetail = true,
                    IsVisibleInUpdate = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true,
                    SortOrder = 3
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
                    SortOrder = 4
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
                    SortOrder = 5
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
                    SortOrder = 6
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
                    SortOrder = 7
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
                    SortOrder = 8
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
                    SortOrder = 9
                },
                new()
                {
                    Key = "attendance",
                    Label = "Attendance",
                    Icon = "attendance",
                    Endpoint = "/api/v1/corporate/human-resource/master-data/doctors/{doctorId}/attendance",
                    IsVisibleInDetail = true,
                    IsVisibleInUpdate = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false,
                    SortOrder = 10
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
                    SortOrder = 11
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
                    SortOrder = 12
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
                    SortOrder = 13
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
                    SortOrder = 14
                }
            }
            .OrderBy(x => x.SortOrder)
            .ToList();
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<DoctorFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Doctor",
            Description = "Melihat data doctor",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Doctor", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new DoctorFilterMetadataResponse
            {
                DefaultFilter = new DoctorDefaultFilterResponse
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
                SortOptions = new List<DoctorSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "doctorCode", Label = "Kode doctor" },
                    new() { Value = "doctorNumber", Label = "Nomor doctor" },
                    new() { Value = "fullName", Label = "Nama doctor" },
                    new() { Value = "departmentName", Label = "Nama department" },
                    new() { Value = "positionName", Label = "Nama position" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                WorkforceUserTypeOptions = BuildDoctorUserTypeOptions(),
                GenderOptions = BuildEnumOptions<Gender>(),
                ReligionOptions = BuildEnumOptions<Religion>(),
                MaritalStatusOptions = BuildEnumOptions<MaritalStatus>(),
                BloodTypeOptions = BuildEnumOptions<BloodType>(),
                DoctorStatusOptions = BuildEnumOptions<DoctorStatus>(),
                DoctorTypeOptions = BuildEnumOptions<DoctorType>(),
                PracticeTypeOptions = BuildEnumOptions<DoctorPracticeType>(),
                EmploymentTypeOptions = BuildEnumOptions<EmploymentType>(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateDoctorFieldMetadata(),
                UpdateFields = BuildUpdateDoctorFieldMetadata(),
                DetailTabs = BuildDoctorDetailTabs()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Doctor.GetFilterMetadata",
                "Mengambil metadata filter doctor.",
                result
            );

            return Ok(ApiResponse<DoctorFilterMetadataResponse>.Ok(
                result,
                "Metadata filter doctor berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<DoctorSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Doctor",
            Description = "Melihat data doctor",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Doctor", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var doctorQuery = _dbContext.Set<MstDoctor>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new DoctorSummaryResponse
            {
                TotalDoctor = await doctorQuery.CountAsync(),
                ActiveDoctor = await doctorQuery.CountAsync(x => x.IsActive),
                InactiveDoctor = await doctorQuery.CountAsync(x => !x.IsActive),
                AvailableForAppointmentDoctor = await doctorQuery.CountAsync(x => x.IsAvailableForAppointment && x.IsActive),
                PermanentDoctor = await doctorQuery.CountAsync(x => x.WorkforceProfile != null && x.WorkforceProfile.UserType == UserType.PermanentDoctor),
                GuestDoctor = await doctorQuery.CountAsync(x => x.WorkforceProfile != null && x.WorkforceProfile.UserType == UserType.GuestDoctor),
                DoctorWithUserAccount = await doctorQuery.CountAsync(x =>
                    _dbContext.Users.Any(u =>
                        u.DoctorId == x.Id &&
                        (u.UserType == UserType.PermanentDoctor || u.UserType == UserType.GuestDoctor))),
                CredentialingPendingDoctor = await doctorQuery.CountAsync(x => x.DoctorStatus == DoctorStatus.CredentialingPending),
                PrivilegeSuspendedDoctor = await doctorQuery.CountAsync(x => x.DoctorStatus == DoctorStatus.PrivilegeSuspended)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Doctor.GetSummary",
                "Mengambil ringkasan data doctor.",
                result
            );

            return Ok(ApiResponse<DoctorSummaryResponse>.Ok(
                result,
                "Ringkasan doctor berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseDoctorPagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Doctor",
            Description = "Melihat data doctor",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Doctor", "Read")]
        public async Task<IActionResult> GetDoctors(
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

            var query = _dbContext.Set<MstDoctor>()
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
                    x.DoctorCode.ToLower().Contains(keyword) ||
                    x.DoctorNumber.ToLower().Contains(keyword) ||
                    x.FullName.ToLower().Contains(keyword) ||
                    (x.NickName != null && x.NickName.ToLower().Contains(keyword)) ||
                    (x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(keyword)) ||
                    (x.WhatsAppNumber != null && x.WhatsAppNumber.ToLower().Contains(keyword)) ||
                    (x.Email != null && x.Email.ToLower().Contains(keyword)) ||
                    (x.IdentityNumber != null && x.IdentityNumber.ToLower().Contains(keyword)) ||
                    (x.SpecialistName != null && x.SpecialistName.ToLower().Contains(keyword)) ||
                    (x.SubSpecialistName != null && x.SubSpecialistName.ToLower().Contains(keyword)) ||
                    (x.MedicalStaffGroup != null && x.MedicalStaffGroup.ToLower().Contains(keyword)) ||
                    (x.PrimaryDepartment != null && x.PrimaryDepartment.DepartmentCode.ToLower().Contains(keyword)) ||
                    (x.PrimaryDepartment != null && x.PrimaryDepartment.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.PrimaryPosition != null && x.PrimaryPosition.PositionCode.ToLower().Contains(keyword)) ||
                    (x.PrimaryPosition != null && x.PrimaryPosition.PositionName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await ApplyDoctorSorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DoctorResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceUserType = x.WorkforceProfile != null ? x.WorkforceProfile.UserType : UserType.GuestDoctor,
                    DoctorCode = x.DoctorCode,
                    DoctorNumber = x.DoctorNumber,
                    FullName = x.FullName,
                    NickName = x.NickName,
                    Gender = x.Gender,
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

                    DoctorStatus = x.DoctorStatus,
                    DoctorType = x.DoctorType,
                    PracticeType = x.PracticeType,
                    EmploymentType = x.EmploymentType,
                    SpecialistName = x.SpecialistName,
                    SubSpecialistName = x.SubSpecialistName,
                    MedicalStaffGroup = x.MedicalStaffGroup,
                    GradeLevel = x.GradeLevel,
                    WorkLocation = x.WorkLocation,
                    JoinDate = x.JoinDate,
                    ContractEndDate = x.ContractEndDate,
                    ResignDate = x.ResignDate,
                    CredentialingDate = x.CredentialingDate,
                    IsAvailableForAppointment = x.IsAvailableForAppointment,
                    HasUserAccount = _dbContext.Users.Any(u =>
                        u.DoctorId == x.Id &&
                        (u.UserType == UserType.PermanentDoctor || u.UserType == UserType.GuestDoctor)),
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new ResponseDoctorPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Doctor.GetDoctors",
                "Mengambil data doctor.",
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

            return Ok(ApiResponse<ResponseDoctorPagedResult>.Ok(
                result,
                "Data doctor berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<DoctorOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Doctor",
            Description = "Melihat pilihan doctor",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Doctor", "Read")]
        public async Task<IActionResult> GetDoctorOptions(
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

            var query = _dbContext.Set<MstDoctor>()
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
                    x.DoctorCode.ToLower().Contains(keyword) ||
                    x.DoctorNumber.ToLower().Contains(keyword) ||
                    x.FullName.ToLower().Contains(keyword) ||
                    (x.SpecialistName != null && x.SpecialistName.ToLower().Contains(keyword)) ||
                    (x.SubSpecialistName != null && x.SubSpecialistName.ToLower().Contains(keyword)) ||
                    (x.PrimaryDepartment != null && x.PrimaryDepartment.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.PrimaryPosition != null && x.PrimaryPosition.PositionName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.FullName)
                .ThenBy(x => x.DoctorCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DoctorOptionResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceUserType = x.WorkforceProfile != null ? x.WorkforceProfile.UserType : UserType.GuestDoctor,
                    DoctorCode = x.DoctorCode,
                    DoctorNumber = x.DoctorNumber,
                    FullName = x.FullName,
                    SpecialistName = x.SpecialistName,
                    SubSpecialistName = x.SubSpecialistName,
                    PrimaryDepartmentId = x.PrimaryDepartmentId,
                    PrimaryDepartmentName = x.PrimaryDepartment != null ? x.PrimaryDepartment.DepartmentName : string.Empty,
                    PrimaryPositionId = x.PrimaryPositionId,
                    PrimaryPositionName = x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : string.Empty,
                    IsAvailableForAppointment = x.IsAvailableForAppointment
                })
                .ToListAsync();

            var result = new PagedResult<DoctorOptionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PagedResult<DoctorOptionResponse>>.Ok(
                result,
                "Data pilihan doctor berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DoctorDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Doctor",
            Description = "Melihat data doctor",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Doctor", "Read")]
        public async Task<IActionResult> GetDoctorById(Guid id)
        {
            var data = await _dbContext.Set<MstDoctor>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new DoctorDetailResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceUserType = x.WorkforceProfile != null ? x.WorkforceProfile.UserType : UserType.GuestDoctor,
                    DoctorCode = x.DoctorCode,
                    DoctorNumber = x.DoctorNumber,
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

                    DoctorStatus = x.DoctorStatus,
                    DoctorType = x.DoctorType,
                    PracticeType = x.PracticeType,
                    EmploymentType = x.EmploymentType,
                    SpecialistName = x.SpecialistName,
                    SubSpecialistName = x.SubSpecialistName,
                    MedicalStaffGroup = x.MedicalStaffGroup,
                    GradeLevel = x.GradeLevel,
                    WorkLocation = x.WorkLocation,
                    JoinDate = x.JoinDate,
                    ProbationEndDate = x.ProbationEndDate,
                    ContractStartDate = x.ContractStartDate,
                    ContractEndDate = x.ContractEndDate,
                    ResignDate = x.ResignDate,
                    ResignReason = x.ResignReason,
                    CredentialingDate = x.CredentialingDate,
                    IsAvailableForAppointment = x.IsAvailableForAppointment,
                    HasUserAccount = _dbContext.Users.Any(u =>
                        u.DoctorId == x.Id &&
                        (u.UserType == UserType.PermanentDoctor || u.UserType == UserType.GuestDoctor)),
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Doctor tidak ditemukan."
                ));
            }

            data.UserAccount = await BuildDoctorUserAccountCompactResponseAsync(id);
            data.ChildSummary = await BuildDoctorChildSummaryAsync(id);

            return Ok(ApiResponse<DoctorDetailResponse>.Ok(
                data,
                "Detail doctor berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DoctorCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Doctor",
            Description = "Membuat data doctor",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("Doctor", "Create")]
        public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorRequest request)
        {
            var requiredValidation = ValidateRequiredDoctorRequest(request);

            if (!requiredValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    requiredValidation.ErrorMessage ?? "Data wajib doctor belum lengkap."
                ));
            }

            if (!IsDoctorUserType(request.DoctorUserType))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "DoctorUserType hanya boleh PermanentDoctor atau GuestDoctor."
                ));
            }

            if (request.CreateLoginAccount)
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Email wajib diisi jika ingin membuat akun login doctor."
                    ));
                }

                if (!request.BirthDate.HasValue || request.BirthDate.Value == default)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "BirthDate wajib diisi untuk generate password awal akun login doctor."
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

            var validation = await ValidateDoctorRequestAsync(
                excludeDoctorId: null,
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
                email: request.Email
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data doctor tidak valid."
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

                var workforceProfile = new MstWorkforceProfile
                {
                    Id = Guid.NewGuid(),
                    ProfileCode = await GenerateWorkforceProfileCodeAsync(),
                    UserType = request.DoctorUserType,
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

                var entity = new MstDoctor
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfile.Id,
                    DoctorCode = await GenerateDoctorCodeAsync(),
                    DoctorNumber = await GenerateDoctorNumberAsync(request.JoinDate),
                    FullName = request.FullName.Trim(),
                    NickName = NormalizeNullableText(request.NickName),
                    BirthPlace = NormalizeNullableText(request.BirthPlace),
                    BirthDate = request.BirthDate,
                    Gender = request.Gender,
                    Religion = request.Religion,
                    MaritalStatus = request.MaritalStatus,
                    BloodType = request.BloodType,
                    IdentityType = NormalizeNullableText(request.IdentityType),
                    IdentityNumber = NormalizeNullableText(request.IdentityNumber),
                    PhoneNumber = normalizedPhone,
                    WhatsAppNumber = normalizedWhatsApp,
                    Email = normalizedEmail,
                    Address = NormalizeNullableText(request.Address),
                    CountryId = NormalizeNullableGuid(request.CountryId),
                    ProvinceId = NormalizeNullableGuid(request.ProvinceId),
                    CityId = NormalizeNullableGuid(request.CityId),
                    DistrictId = NormalizeNullableGuid(request.DistrictId),
                    PostalCodeId = NormalizeNullableGuid(request.PostalCodeId),
                    PrimaryDepartmentId = primaryDepartmentId,
                    PrimaryPositionId = primaryPositionId,
                    DoctorStatus = request.DoctorStatus,
                    DoctorType = request.DoctorType,
                    PracticeType = request.PracticeType,
                    EmploymentType = request.EmploymentType,
                    SpecialistName = NormalizeNullableText(request.SpecialistName),
                    SubSpecialistName = NormalizeNullableText(request.SubSpecialistName),
                    MedicalStaffGroup = NormalizeNullableText(request.MedicalStaffGroup),
                    GradeLevel = NormalizeNullableText(request.GradeLevel),
                    WorkLocation = NormalizeNullableText(request.WorkLocation),
                    JoinDate = request.JoinDate,
                    ProbationEndDate = request.ProbationEndDate,
                    ContractStartDate = request.ContractStartDate,
                    ContractEndDate = request.ContractEndDate,
                    CredentialingDate = request.CredentialingDate,
                    IsAvailableForAppointment = request.IsAvailableForAppointment,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<MstDoctor>().Add(entity);
                await _dbContext.SaveChangesAsync();

                if (primaryDepartmentId.HasValue && primaryPositionId.HasValue)
                {
                    var organizationAssignment = new WfpOrganizationAssignment
                    {
                        Id = Guid.NewGuid(),
                        WorkforceProfileId = workforceProfile.Id,
                        DepartmentId = primaryDepartmentId.Value,
                        PositionId = primaryPositionId.Value,
                        IsPrimary = true,
                        IsActive = true,
                        EffectiveStartDate = (entity.JoinDate ?? now.Date).Date,
                        EffectiveEndDate = null,
                        Description = "Initial primary organization assignment",
                        CreateDateTime = now,
                        CreateBy = actorUserId,
                        IsDelete = false,
                        IsCancel = false
                    };

                    _dbContext.Set<WfpOrganizationAssignment>().Add(organizationAssignment);
                    await _dbContext.SaveChangesAsync();
                }

                DoctorLoginAccountResponse? loginAccount = null;

                if (request.CreateLoginAccount)
                {
                    var accountResult = await CreateLoginAccountForDoctorAsync(
                        doctor: entity,
                        userType: request.DoctorUserType,
                        isFingerprintRegistrationEnabled: request.IsFingerprintRegistrationEnabled,
                        fingerprintRegistrationReason: request.FingerprintRegistrationReason,                        
                        actorUserId: actorUserId
                    );

                    if (!accountResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();

                        return BadRequest(ApiResponse<object>.Fail(
                            StatusCodes.Status400BadRequest,
                            accountResult.ErrorMessage ?? "Akun login doctor gagal dibuat."
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
                            effectiveStartDate: entity.JoinDate ?? now.Date,
                            actorUserId: actorUserId
                        );

                        await _dbContext.SaveChangesAsync();
                    }
                }

                await transaction.CommitAsync();

                var response = new DoctorCreateResponse
                {
                    Id = entity.Id,
                    WorkforceProfileId = entity.WorkforceProfileId,
                    WorkforceUserType = workforceProfile.UserType,
                    DoctorCode = entity.DoctorCode,
                    DoctorNumber = entity.DoctorNumber,
                    FullName = entity.FullName,
                    IsAvailableForAppointment = entity.IsAvailableForAppointment,
                    IsActive = entity.IsActive,
                    LoginAccount = loginAccount
                };

                await _loggerService.InfoAsync(
                    LogCategory,
                    "Doctor.CreateDoctor",
                    "Doctor berhasil dibuat.",
                    new
                    {
                        entity.Id,
                        entity.WorkforceProfileId,
                        workforceProfile.UserType,
                        entity.DoctorCode,
                        entity.DoctorNumber,
                        entity.FullName,
                        entity.PrimaryDepartmentId,
                        entity.PrimaryPositionId,
                        LoginAccountCreated = loginAccount?.IsCreated ?? false,
                        loginAccount?.UserId,
                        loginAccount?.UserCode
                    }
                );

                return Ok(ApiResponse<DoctorCreateResponse>.Ok(
                    response,
                    request.CreateLoginAccount
                        ? "Doctor dan akun login berhasil dibuat."
                        : "Doctor berhasil dibuat."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "Doctor.CreateDoctor",
                    "Gagal membuat doctor.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat doctor."
                    )
                );
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Doctor",
            Description = "Mengubah data doctor",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Doctor", "Update")]
        public async Task<IActionResult> UpdateDoctor(
            Guid id,
            [FromBody] UpdateDoctorRequest request)
        {
            var entity = await _dbContext.Set<MstDoctor>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Doctor tidak ditemukan."
                ));
            }

            var requiredValidation = ValidateRequiredDoctorRequest(request);

            if (!requiredValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    requiredValidation.ErrorMessage ?? "Data wajib doctor belum lengkap."
                ));
            }

            var validation = await ValidateDoctorRequestAsync(
                excludeDoctorId: id,
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
                email: request.Email
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data doctor tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var normalizedEmail = NormalizeNullableText(request.Email)?.ToLowerInvariant();
            var normalizedPhone = NormalizeDigitsOnly(request.PhoneNumber);
            var normalizedWhatsApp = NormalizeDigitsOnly(request.WhatsAppNumber);
            var primaryDepartmentId = NormalizeNullableGuid(request.PrimaryDepartmentId);
            var primaryPositionId = NormalizeNullableGuid(request.PrimaryPositionId);

            entity.FullName = request.FullName.Trim();
            entity.NickName = NormalizeNullableText(request.NickName);
            entity.BirthPlace = NormalizeNullableText(request.BirthPlace);
            entity.BirthDate = request.BirthDate;
            entity.Gender = request.Gender;
            entity.Religion = request.Religion;
            entity.MaritalStatus = request.MaritalStatus;
            entity.BloodType = request.BloodType;
            entity.IdentityType = NormalizeNullableText(request.IdentityType);
            entity.IdentityNumber = NormalizeNullableText(request.IdentityNumber);
            entity.PhoneNumber = normalizedPhone;
            entity.WhatsAppNumber = normalizedWhatsApp;
            entity.Email = normalizedEmail;
            entity.Address = NormalizeNullableText(request.Address);
            entity.CountryId = NormalizeNullableGuid(request.CountryId);
            entity.ProvinceId = NormalizeNullableGuid(request.ProvinceId);
            entity.CityId = NormalizeNullableGuid(request.CityId);
            entity.DistrictId = NormalizeNullableGuid(request.DistrictId);
            entity.PostalCodeId = NormalizeNullableGuid(request.PostalCodeId);
            entity.PrimaryDepartmentId = primaryDepartmentId;
            entity.PrimaryPositionId = primaryPositionId;
            entity.DoctorStatus = request.DoctorStatus;
            entity.DoctorType = request.DoctorType;
            entity.PracticeType = request.PracticeType;
            entity.EmploymentType = request.EmploymentType;
            entity.SpecialistName = NormalizeNullableText(request.SpecialistName);
            entity.SubSpecialistName = NormalizeNullableText(request.SubSpecialistName);
            entity.MedicalStaffGroup = NormalizeNullableText(request.MedicalStaffGroup);
            entity.GradeLevel = NormalizeNullableText(request.GradeLevel);
            entity.WorkLocation = NormalizeNullableText(request.WorkLocation);
            entity.JoinDate = request.JoinDate;
            entity.ProbationEndDate = request.ProbationEndDate;
            entity.ContractStartDate = request.ContractStartDate;
            entity.ContractEndDate = request.ContractEndDate;
            entity.ResignDate = request.ResignDate;
            entity.ResignReason = NormalizeNullableText(request.ResignReason);
            entity.CredentialingDate = request.CredentialingDate;
            entity.IsAvailableForAppointment = request.IsAvailableForAppointment;
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
                x.DoctorId == entity.Id &&
                (x.UserType == UserType.PermanentDoctor || x.UserType == UserType.GuestDoctor));

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
                    effectiveStartDate: entity.JoinDate ?? now.Date,
                    actorUserId: actorUserId
                );
            }

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Doctor.UpdateDoctor",
                "Doctor berhasil diperbarui.",
                new
                {
                    entity.Id,
                    entity.WorkforceProfileId,
                    entity.DoctorCode,
                    entity.DoctorNumber,
                    entity.FullName,
                    entity.IsActive,
                    entity.IsAvailableForAppointment
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Doctor berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Doctor",
            Description = "Mengubah status doctor",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Doctor", "Update")]
        public async Task<IActionResult> UpdateDoctorStatus(
            Guid id,
            [FromBody] UpdateDoctorStatusRequest request)
        {
            var entity = await _dbContext.Set<MstDoctor>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Doctor tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.DoctorStatus = request.DoctorStatus;
            entity.IsAvailableForAppointment = request.IsActive && request.IsAvailableForAppointment;
            entity.ResignDate = request.ResignDate;
            entity.ResignReason = NormalizeNullableText(request.ResignReason);
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
                x.DoctorId == id &&
                (x.UserType == UserType.PermanentDoctor || x.UserType == UserType.GuestDoctor));

            if (linkedUser != null)
            {
                linkedUser.IsActive = request.IsActive;
                linkedUser.UpdateDateTime = now;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status doctor berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/user-account/geolocation-bypass")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
    "Update",
    "Update Doctor",
    Description = "Mengubah pengaturan geolocation bypass akun doctor",
    AccessType = AccessTypes.Update,
    SortOrder = 3
)]
        [AccessPermission("Doctor", "Update")]
        public async Task<IActionResult> UpdateDoctorUserGeolocationBypass(
    Guid id,
    [FromBody] UpdateDoctorUserGeolocationBypassRequest request)
        {
            var doctorExists = await _dbContext.Set<MstDoctor>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == id && !x.IsDelete);

            if (!doctorExists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Doctor tidak ditemukan."
                ));
            }

            var linkedUser = await _dbContext.Users.FirstOrDefaultAsync(x =>
                x.DoctorId == id &&
                (x.UserType == UserType.PermanentDoctor || x.UserType == UserType.GuestDoctor));

            if (linkedUser == null)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Doctor belum memiliki akun login, sehingga geolocation bypass tidak dapat diubah."
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
                "Doctor.UpdateDoctorUserGeolocationBypass",
                "Pengaturan geolocation bypass akun doctor berhasil diperbarui.",
                new
                {
                    DoctorId = id,
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
                    DoctorId = id,
                    UserId = linkedUser.Id,
                    linkedUser.IsGeolocationBypassEnabled,
                    linkedUser.GeolocationBypassUntil,
                    linkedUser.GeolocationBypassReason
                },
                request.IsGeolocationBypassEnabled
                    ? "Geolocation bypass akun doctor berhasil diaktifkan."
                    : "Geolocation bypass akun doctor berhasil dinonaktifkan."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Doctor",
            Description = "Menghapus data doctor",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("Doctor", "Delete")]
        public async Task<IActionResult> DeleteDoctor(Guid id)
        {
            var entity = await _dbContext.Set<MstDoctor>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Doctor tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsAvailableForAppointment = false;
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
                x.DoctorId == id &&
                (x.UserType == UserType.PermanentDoctor || x.UserType == UserType.GuestDoctor));

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
                "Doctor berhasil dihapus."
            ));
        }

        private async Task<string> GenerateDoctorCodeAsync()
        {
            const string menuCode = "DOC";
            const string prefix = $"{menuCode}-{HospitalCode}-";

            var existingCount = await _dbContext.Set<MstDoctor>()
                .IgnoreQueryFilters()
                .CountAsync(x => x.DoctorCode.StartsWith(prefix));

            var nextNumber = existingCount + 1;

            while (true)
            {
                var code = $"{prefix}{nextNumber:D5}";

                var exists = await _dbContext.Set<MstDoctor>()
                    .IgnoreQueryFilters()
                    .AnyAsync(x => x.DoctorCode == code);

                if (!exists)
                {
                    return code;
                }

                nextNumber++;
            }
        }

        private async Task<string> GenerateDoctorNumberAsync(DateTime? joinDate)
        {
            var date = (joinDate ?? DateTime.UtcNow).Date;
            var dateCode = date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            var prefix = $"{HospitalCode}-DOC-{dateCode}";

            var existingCount = await _dbContext.Set<MstDoctor>()
                .IgnoreQueryFilters()
                .CountAsync(x => x.DoctorNumber.StartsWith(prefix));

            var nextNumber = existingCount + 1;

            while (true)
            {
                var doctorNumber = $"{prefix}{nextNumber.ToString("D3")}";

                var exists = await _dbContext.Set<MstDoctor>()
                    .AnyAsync(x => x.DoctorNumber == doctorNumber);

                if (!exists)
                {
                    return doctorNumber;
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

        private async Task<DoctorUserAccountCompactResponse?> BuildDoctorUserAccountCompactResponseAsync(Guid doctorId)
        {
            var now = DateTime.UtcNow;

            var user = await _dbContext.Users
                .AsNoTracking()
                .Where(x =>
                    x.DoctorId == doctorId &&
                    (x.UserType == UserType.PermanentDoctor || x.UserType == UserType.GuestDoctor))
                .Select(x => new DoctorUserAccountCompactResponse
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

            return user ?? new DoctorUserAccountCompactResponse
            {
                IsAvailable = false,
                IsActive = false,
                MustChangePassword = false,
                IsGeolocationBypassEnabled = false,
                IsGeolocationBypassActive = false
            };
        }

        private async Task<DoctorChildSummaryResponse> BuildDoctorChildSummaryAsync(Guid doctorId)
        {
            var doctor = await _dbContext.Set<MstDoctor>()
                .AsNoTracking()
                .Where(x => x.Id == doctorId && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.WorkforceProfileId
                })
                .FirstOrDefaultAsync();

            if (doctor == null || doctor.WorkforceProfileId == Guid.Empty)
            {
                return new DoctorChildSummaryResponse();
            }

            var workforceProfileId = doctor.WorkforceProfileId;

            var organizationQuery = _dbContext.Set<WfpOrganizationAssignment>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            return new DoctorChildSummaryResponse
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

                EducationCount = await _dbContext.Set<WfpEducation>()
                    .AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete),

                TrainingCount = await _dbContext.Set<WfpTrainingRecord>()
                    .AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete),

                CertificationCount = await _dbContext.Set<WfpCertification>()
                    .AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete),

                CredentialLicenseCount = await _dbContext.Set<WfpCredentialLicense>()
                    .AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete),

                ClinicalPrivilegeCount = 0,
                HealthRecordCount = 0,

                ScheduleAssignmentCount = await _dbContext.Set<WfpWorkScheduleAssignment>()
                    .AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete),

                AttendanceCount = await _dbContext.Set<EmpAttendance>()
                    .AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateDoctorRequestAsync(
            Guid? excludeDoctorId,
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
            string? email)
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
                var exists = await _dbContext.Set<MstDoctor>()
                    .AnyAsync(x =>
                        x.Id != excludeDoctorId &&
                        x.IdentityNumber == normalizedIdentityNumber &&
                        !x.IsDelete);

                if (exists)
                {
                    return (false, "Nomor identitas doctor sudah digunakan.");
                }
            }

            if (!string.IsNullOrWhiteSpace(normalizedEmail))
            {
                var emailLower = normalizedEmail.ToLower();

                var exists = await _dbContext.Set<MstDoctor>()
                    .AnyAsync(x =>
                        x.Id != excludeDoctorId &&
                        x.Email != null &&
                        x.Email.ToLower() == emailLower &&
                        !x.IsDelete);

                if (exists)
                {
                    return (false, "Email doctor sudah digunakan.");
                }
            }

            return (true, null);
        }

        private async Task EnsureWorkforcePrimaryOrganizationAssignmentAsync(
            MstDoctor doctor,
            DateTime now,
            Guid actorUserId)
        {
            if (doctor.WorkforceProfileId == Guid.Empty ||
                !doctor.PrimaryDepartmentId.HasValue ||
                !doctor.PrimaryPositionId.HasValue)
            {
                return;
            }

            var workforceProfileId = doctor.WorkforceProfileId;
            var departmentId = doctor.PrimaryDepartmentId.Value;
            var positionId = doctor.PrimaryPositionId.Value;

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
                    IsActive = doctor.IsActive,
                    EffectiveStartDate = (doctor.JoinDate ?? now.Date).Date,
                    EffectiveEndDate = null,
                    Description = "Primary organization assignment synced from doctor update",
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
                existing.IsActive = doctor.IsActive;
                existing.EffectiveStartDate = (doctor.JoinDate ?? now.Date).Date;
                existing.EffectiveEndDate = null;
                existing.Description = "Primary organization assignment synced from doctor update";
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
            string? customPeriod)
        {
            var period = customPeriod?.Trim().ToLower();
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
                        $"customPeriod '{customPeriod}' tidak valid. Gunakan endpoint filters/metadata untuk melihat daftar customPeriod yang tersedia."
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

        private static IOrderedQueryable<MstDoctor> ApplyDoctorSorting(
            IQueryable<MstDoctor> query,
            string? sortBy,
            string? sortDirection)
        {
            var field = NormalizeSortBy(sortBy);
            var desc = IsDescending(sortDirection);

            return field switch
            {
                "doctorcode" => desc
                    ? query.OrderByDescending(x => x.DoctorCode).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.DoctorCode).ThenBy(x => x.FullName),

                "doctornumber" => desc
                    ? query.OrderByDescending(x => x.DoctorNumber).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.DoctorNumber).ThenBy(x => x.FullName),

                "fullname" => desc
                    ? query.OrderByDescending(x => x.FullName).ThenBy(x => x.DoctorCode)
                    : query.OrderBy(x => x.FullName).ThenBy(x => x.DoctorCode),

                "departmentname" => desc
                    ? query.OrderByDescending(x => x.PrimaryDepartment != null ? x.PrimaryDepartment.DepartmentName : string.Empty).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.PrimaryDepartment != null ? x.PrimaryDepartment.DepartmentName : string.Empty).ThenBy(x => x.FullName),

                "positionname" => desc
                    ? query.OrderByDescending(x => x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : string.Empty).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : string.Empty).ThenBy(x => x.FullName),

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

        private static List<DoctorCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<DoctorCustomPeriodOptionResponse>
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

        private static List<DoctorQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<DoctorQueryParameterInfoResponse>
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
                    Description = "Filter status aktif doctor.",
                    Example = "true"
                },
                new()
                {
                    Name = "search",
                    Type = "string",
                    Description = "Pencarian doctor code, doctor number, nama, email, phone, spesialis, department, dan position.",
                    Example = "andi"
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

        private static List<DoctorFormFieldMetadataResponse> BuildCreateDoctorFieldMetadata()
        {
            return BuildDoctorFieldMetadata(isUpdate: false);
        }

        private static List<DoctorFormFieldMetadataResponse> BuildUpdateDoctorFieldMetadata()
        {
            var fields = BuildDoctorFieldMetadata(isUpdate: true);

            fields.Add(new DoctorFormFieldMetadataResponse
            {
                Name = "isActive",
                Label = "Status Aktif",
                Section = "Status",
                InputType = "boolean",
                IsRequiredOnCreate = false,
                IsRequiredOnUpdate = true,
                RequiredType = "Required",
                Description = "Status aktif doctor.",
                Example = "true",
                SortOrder = 900
            });

            fields.Add(new DoctorFormFieldMetadataResponse
            {
                Name = "resignDate",
                Label = "Tanggal Resign",
                Section = "Status",
                InputType = "date",
                IsRequiredOnCreate = false,
                IsRequiredOnUpdate = false,
                RequiredType = "Optional",
                Description = "Diisi jika doctor resign atau dinonaktifkan.",
                Example = "2026-05-07",
                SortOrder = 901
            });

            fields.Add(new DoctorFormFieldMetadataResponse
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

        private static List<DoctorFormFieldMetadataResponse> BuildDoctorFieldMetadata(bool isUpdate)
        {
            return new List<DoctorFormFieldMetadataResponse>
            {
                new()
                {
                    Name = "doctorUserType",
                    Label = "Jenis User Doctor",
                    Section = "Identitas Utama",
                    InputType = "select",
                    IsRequiredOnCreate = true,
                    IsRequiredOnUpdate = false,
                    RequiredType = isUpdate ? "Readonly" : "Required",
                    OptionsSource = "workforceUserTypeOptions",
                    Description = "Hanya PermanentDoctor atau GuestDoctor.",
                    Example = "PermanentDoctor",
                    SortOrder = 1
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
                    Description = "Nama lengkap doctor.",
                    Example = "dr. Andi Wijaya, Sp.PD",
                    SortOrder = 2
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
                    Example = "dr. Andi",
                    SortOrder = 3
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
                    Example = "1",
                    SortOrder = 4
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
                    SortOrder = 5
                },
                new()
                {
                    Name = "birthDate",
                    Label = "Tanggal Lahir",
                    Section = "Identitas Utama",
                    InputType = "date",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Conditional",
                    Description = "Wajib jika CreateLoginAccount = true karena dipakai untuk password awal.",
                    Example = "1988-01-01",
                    SortOrder = 6
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
                    SortOrder = 7
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
                    SortOrder = 8
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
                    SortOrder = 9
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
                    SortOrder = 101
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
                    MaxLength = 50,
                    ValidationRule = "unique",
                    Example = "3273010101880001",
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
                    MaxLength = 30,
                    ValidationRule = "digits;max:30",
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
                    MaxLength = 30,
                    ValidationRule = "digits;max:30",
                    SortOrder = 104
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
                    Example = "doctor@admin.com",
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
                    SortOrder = 205
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
                    SortOrder = 301
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
                    SortOrder = 302
                },
                new()
                {
                    Name = "doctorStatus",
                    Label = "Status Doctor",
                    Section = "Doctor Profile",
                    InputType = "select",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    OptionsSource = "doctorStatusOptions",
                    Example = "1",
                    SortOrder = 401
                },
                new()
                {
                    Name = "doctorType",
                    Label = "Tipe Doctor",
                    Section = "Doctor Profile",
                    InputType = "select",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    OptionsSource = "doctorTypeOptions",
                    Example = "1",
                    SortOrder = 402
                },
                new()
                {
                    Name = "practiceType",
                    Label = "Tipe Praktik",
                    Section = "Doctor Profile",
                    InputType = "select",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    OptionsSource = "practiceTypeOptions",
                    Example = "1",
                    SortOrder = 403
                },
                new()
                {
                    Name = "employmentType",
                    Label = "Tipe Employment",
                    Section = "Doctor Profile",
                    InputType = "select",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    OptionsSource = "employmentTypeOptions",
                    Example = "2",
                    SortOrder = 404
                },
                new()
                {
                    Name = "specialistName",
                    Label = "Spesialis",
                    Section = "Doctor Profile",
                    InputType = "text",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 100,
                    Example = "Penyakit Dalam",
                    SortOrder = 405
                },
                new()
                {
                    Name = "subSpecialistName",
                    Label = "Sub Spesialis",
                    Section = "Doctor Profile",
                    InputType = "text",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 100,
                    Example = "Konsultan Ginjal Hipertensi",
                    SortOrder = 406
                },
                new()
                {
                    Name = "medicalStaffGroup",
                    Label = "Medical Staff Group",
                    Section = "Doctor Profile",
                    InputType = "text",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 100,
                    Example = "SMF Penyakit Dalam",
                    SortOrder = 407
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
                    SortOrder = 501
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
                    SortOrder = 502
                },
                new()
                {
                    Name = "joinDate",
                    Label = "Tanggal Bergabung",
                    Section = "Employment",
                    InputType = "date",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    Example = "2026-05-07",
                    SortOrder = 503
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
                    SortOrder = 504
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
                    SortOrder = 505
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
                    SortOrder = 506
                },
                new()
                {
                    Name = "credentialingDate",
                    Label = "Tanggal Credentialing",
                    Section = "Credentialing",
                    InputType = "date",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    SortOrder = 601
                },
                new()
                {
                    Name = "isAvailableForAppointment",
                    Label = "Tersedia untuk Appointment",
                    Section = "Appointment",
                    InputType = "boolean",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    Example = "true",
                    SortOrder = 701
                }
            }
            .OrderBy(x => x.SortOrder)
            .ToList();
        }

        private static List<DoctorEnumOptionResponse> BuildDoctorUserTypeOptions()
        {
            return new List<UserType>
                {
                    UserType.PermanentDoctor,
                    UserType.GuestDoctor
                }
                .Select(x => new DoctorEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = BuildEnumLabel(x.ToString())
                })
                .ToList();
        }

        private static List<DoctorEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : struct, System.Enum
        {
            return System.Enum.GetValues<TEnum>()
                .Select(x => new DoctorEnumOptionResponse
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
                "PermanentDoctor" => "Dokter Tetap",
                "GuestDoctor" => "Dokter Tamu",

                "Active" => "Aktif",
                "Inactive" => "Tidak Aktif",
                "OnLeave" => "Cuti",
                "Probation" => "Probation",
                "Suspended" => "Suspended",
                "Resigned" => "Resign",
                "Terminated" => "Diberhentikan",
                "Retired" => "Pensiun",
                "ContractEnded" => "Kontrak Berakhir",
                "CredentialingPending" => "Credentialing Pending",
                "PrivilegeSuspended" => "Privilege Suspended",

                "Permanent" => "Permanent",
                "Contract" => "Kontrak",
                "Internship" => "Magang",
                "PartTime" => "Paruh Waktu",
                "Outsourced" => "Outsourced",
                "DailyWorker" => "Harian",
                "Consultant" => "Konsultan",
                "Volunteer" => "Relawan",

                "GeneralPractitioner" => "Dokter Umum",
                "Specialist" => "Spesialis",
                "SubSpecialist" => "Sub Spesialis",
                "Dentist" => "Dokter Gigi",
                "SpecialistDentist" => "Dokter Gigi Spesialis",
                "Resident" => "Residen",
                "Fellow" => "Fellow",
                "VisitingDoctor" => "Visiting Doctor",

                "FullTime" => "Full Time",
                "Visiting" => "Visiting",
                "OnCall" => "On Call",
                "Telemedicine" => "Telemedicine",
                "Guest" => "Guest",

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

        private static bool IsDoctorUserType(UserType userType)
        {
            return userType == UserType.PermanentDoctor ||
                   userType == UserType.GuestDoctor;
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

        private string GetDefaultDoctorProfilePhotoPath()
        {
            var configuredPath = _configuration["FileStorage:DefaultDoctorProfilePhotoPath"];

            return string.IsNullOrWhiteSpace(configuredPath)
                ? DefaultDoctorProfilePhotoPathFallback
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

        private async Task<(bool IsSuccess, string? ErrorMessage, DoctorLoginAccountResponse? Response)>
        CreateLoginAccountForDoctorAsync(
            MstDoctor doctor,
            UserType userType,
            bool isFingerprintRegistrationEnabled,
            string? fingerprintRegistrationReason,
            Guid actorUserId)
        {
            if (doctor.Id == Guid.Empty)
            {
                return (false, "DoctorId tidak valid.", null);
            }

            if (!IsDoctorUserType(userType))
            {
                return (false, "UserType doctor hanya boleh PermanentDoctor atau GuestDoctor.", null);
            }

            if (string.IsNullOrWhiteSpace(doctor.Email))
            {
                return (false, "Email doctor wajib diisi untuk membuat akun login.", null);
            }

            if (!doctor.BirthDate.HasValue || doctor.BirthDate.Value == default)
            {
                return (false, "BirthDate doctor wajib diisi untuk generate password awal.", null);
            }

            var email = doctor.Email.Trim().ToLowerInvariant();

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

            var existingDoctorAccount = await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(x =>
                    x.DoctorId == doctor.Id &&
                    (x.UserType == UserType.PermanentDoctor || x.UserType == UserType.GuestDoctor));

            if (existingDoctorAccount)
            {
                return (false, "Doctor ini sudah memiliki akun login.", null);
            }

            var initialPassword = GenerateInitialPasswordFromBirthDate(doctor.BirthDate.Value);

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
                DisplayName = doctor.FullName,
                UserType = userType,
                WorkforceProfileId = doctor.WorkforceProfileId,
                EmployeeId = null,
                DoctorId = doctor.Id,
                ExternalUserId = null,
                PrimaryDepartmentId = doctor.PrimaryDepartmentId,
                PrimaryPositionId = doctor.PrimaryPositionId,
                IsGeolocationBypassEnabled = false,
                GeolocationBypassReason = null,
                GeolocationBypassUntil = null,
                IsActive = doctor.IsActive,
                MustChangePassword = true,
                AccessValidUntil = null,
                CreateDateTime = now,
                ProfilePhotoPath = GetDefaultDoctorProfilePhotoPath(),
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

            var response = new DoctorLoginAccountResponse
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
                Message = "Akun login doctor berhasil dibuat. Password awal wajib diganti saat login pertama."
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
                    Description = "Synced from doctor primary organization assignment",
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
                existing.Description = "Synced from doctor primary organization assignment";
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

        private static (bool IsValid, string? ErrorMessage) ValidateRequiredDoctorRequest(CreateDoctorRequest request)
        {
            return ValidateRequiredDoctorFields(
                fullName: request.FullName,
                doctorUserType: request.DoctorUserType,
                createLoginAccount: request.CreateLoginAccount,
                birthDate: request.BirthDate,
                email: request.Email,
                primaryDepartmentId: request.PrimaryDepartmentId,
                primaryPositionId: request.PrimaryPositionId,
                employmentType: request.EmploymentType
            );
        }

        private static (bool IsValid, string? ErrorMessage) ValidateRequiredDoctorRequest(UpdateDoctorRequest request)
        {
            return ValidateRequiredDoctorFields(
                fullName: request.FullName,
                doctorUserType: UserType.PermanentDoctor,
                createLoginAccount: false,
                birthDate: request.BirthDate,
                email: request.Email,
                primaryDepartmentId: request.PrimaryDepartmentId,
                primaryPositionId: request.PrimaryPositionId,
                employmentType: request.EmploymentType
            );
        }

        private static (bool IsValid, string? ErrorMessage) ValidateRequiredDoctorFields(
            string? fullName,
            UserType doctorUserType,
            bool createLoginAccount,
            DateTime? birthDate,
            string? email,
            Guid? primaryDepartmentId,
            Guid? primaryPositionId,
            EmploymentType employmentType)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return (false, "Nama doctor wajib diisi.");
            }

            if (!IsDoctorUserType(doctorUserType))
            {
                return (false, "DoctorUserType hanya boleh PermanentDoctor atau GuestDoctor.");
            }

            if (createLoginAccount && string.IsNullOrWhiteSpace(email))
            {
                return (false, "Email wajib diisi jika ingin membuat akun login doctor.");
            }

            if (createLoginAccount && (!birthDate.HasValue || birthDate.Value == default))
            {
                return (false, "BirthDate wajib diisi jika ingin membuat akun login doctor.");
            }

            if (primaryPositionId.HasValue && primaryPositionId.Value != Guid.Empty &&
                (!primaryDepartmentId.HasValue || primaryDepartmentId.Value == Guid.Empty))
            {
                return (false, "Primary department wajib dipilih jika primary position diisi.");
            }

            if (employmentType == EmploymentType.Unknown)
            {
                return (false, "Employment type wajib dipilih.");
            }

            return (true, null);
        }
    }
}