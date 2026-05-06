using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.UserManagement.Enum;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Employee.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseEmployeePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs.EmployeeResponse>;

using ResponseTransportAllowanceTransactionPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs.EmployeeTransportAllowanceTransactionResponse>;

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
        private const string LogCategory = "Corporate.HumanResource.MasterData";

        private static readonly string[] AllowanceModes =
        {
            "None",
            "FixedMonthly",
            "DailyAttendance",
            "NightShift",
            "MonthlyAndNightShift",
            "Manual"
        };

        private static readonly string[] TransportTransactionStatuses =
        {
            "Draft",
            "Calculated",
            "Approved",
            "PostedToPayroll",
            "Cancelled"
        };

        private static readonly string[] TransportAllowanceTypes =
        {
            "Monthly",
            "Daily",
            "Night",
            "Adjustment",
            "Manual"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public EmployeeController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        // =========================================================
        // FILTER METADATA
        // =========================================================

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
                    CustomPeriod = null,
                    Search = null,
                    IsActive = null,
                    DepartmentId = null,
                    PositionId = null,
                    EmployeeStatus = null,
                    ProfessionType = null,
                    HasTransportAllowanceProfile = null,
                    SortBy = "createDateTime",
                    SortDirection = "desc",
                    PageNumber = 1,
                    PageSize = 25
                },
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<EmployeeSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "employeeCode", Label = "Kode employee" },
                    new() { Value = "employeeNumber", Label = "Nomor employee" },
                    new() { Value = "attendanceNumber", Label = "Nomor absensi" },
                    new() { Value = "fullName", Label = "Nama employee" },
                    new() { Value = "departmentName", Label = "Nama department" },
                    new() { Value = "positionName", Label = "Nama position" },
                    new() { Value = "joinDate", Label = "Tanggal bergabung" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                TransportAllowanceModes = AllowanceModes.ToList(),
                TransportTransactionStatuses = TransportTransactionStatuses.ToList(),
                TransportAllowanceTypes = TransportAllowanceTypes.ToList(),
                QueryParameters = new List<EmployeeQueryParameterInfoResponse>
                {
                    new()
                    {
                        Name = "startDate",
                        Type = "date",
                        Required = "No",
                        Description = "Tanggal mulai filter berdasarkan CreateDateTime. Dipakai jika customPeriod kosong atau custom.",
                        Example = "2026-01-01"
                    },
                    new()
                    {
                        Name = "endDate",
                        Type = "date",
                        Required = "No",
                        Description = "Tanggal akhir filter berdasarkan CreateDateTime. Sistem membaca sampai akhir hari endDate.",
                        Example = "2026-01-31"
                    },
                    new()
                    {
                        Name = "customPeriod",
                        Type = "string",
                        Required = "No",
                        Description = "Pilihan periode cepat.",
                        Example = "last30days"
                    },
                    new()
                    {
                        Name = "search",
                        Type = "string",
                        Required = "No",
                        Description = "Pencarian employee code, employee number, attendance number, nama, email, phone, department, position.",
                        Example = "budi"
                    },
                    new()
                    {
                        Name = "departmentId",
                        Type = "guid",
                        Required = "No",
                        Description = "Filter berdasarkan primary department.",
                        Example = "00000000-0000-0000-0000-000000000000"
                    },
                    new()
                    {
                        Name = "positionId",
                        Type = "guid",
                        Required = "No",
                        Description = "Filter berdasarkan primary position.",
                        Example = "00000000-0000-0000-0000-000000000000"
                    },
                    new()
                    {
                        Name = "employeeStatus",
                        Type = "enum",
                        Required = "No",
                        Description = "Filter status employee sesuai enum EmployeeStatus.",
                        Example = "Contract"
                    },
                    new()
                    {
                        Name = "professionType",
                        Type = "enum",
                        Required = "No",
                        Description = "Filter profession type sesuai enum EmployeeProfessionType.",
                        Example = "GeneralStaff"
                    },
                    new()
                    {
                        Name = "hasTransportAllowanceProfile",
                        Type = "boolean",
                        Required = "No",
                        Description = "Filter employee yang sudah/belum punya profile uang transport.",
                        Example = "true"
                    },
                    new()
                    {
                        Name = "sortBy",
                        Type = "string",
                        Required = "No",
                        Description = "Field sorting. Nilai tersedia dari SortOptions.",
                        Example = "fullName"
                    },
                    new()
                    {
                        Name = "sortDirection",
                        Type = "string",
                        Required = "No",
                        Description = "Arah sorting. Nilai: asc atau desc.",
                        Example = "asc"
                    },
                    new()
                    {
                        Name = "pageNumber",
                        Type = "integer",
                        Required = "No",
                        Description = "Nomor halaman. Minimal 1.",
                        Example = "1"
                    },
                    new()
                    {
                        Name = "pageSize",
                        Type = "integer",
                        Required = "No",
                        Description = "Jumlah data per halaman. Maksimal 100.",
                        Example = "25"
                    }
                }
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

        // =========================================================
        // SUMMARY
        // =========================================================

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

            var profileQuery = _dbContext.Set<EmpTransportAllowanceProfile>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new EmployeeSummaryResponse
            {
                TotalEmployee = await employeeQuery.CountAsync(),
                ActiveEmployee = await employeeQuery.CountAsync(x => x.IsActive),
                InactiveEmployee = await employeeQuery.CountAsync(x => !x.IsActive),
                EmployeeWithTransportAllowanceProfile = await profileQuery.CountAsync(),
                TransportEligibleEmployee = await profileQuery.CountAsync(x => x.IsEligible && x.IsActive),
                NightTransportEligibleEmployee = await profileQuery.CountAsync(x => x.IsNightTransportEligible && x.IsActive)
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

        // =========================================================
        // EMPLOYEE
        // =========================================================

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
            [FromQuery] string? customPeriod,
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? positionId,
            [FromQuery] EmployeeStatus? employeeStatus,
            [FromQuery] EmployeeProfessionType? professionType,
            [FromQuery] bool? hasTransportAllowanceProfile,
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

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.EmployeeCode.ToLower().Contains(keyword) ||
                    (x.EmployeeNumber != null && x.EmployeeNumber.ToLower().Contains(keyword)) ||
                    (x.AttendanceNumber != null && x.AttendanceNumber.ToLower().Contains(keyword)) ||
                    x.FullName.ToLower().Contains(keyword) ||
                    (x.NickName != null && x.NickName.ToLower().Contains(keyword)) ||
                    (x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(keyword)) ||
                    (x.WhatsAppNumber != null && x.WhatsAppNumber.ToLower().Contains(keyword)) ||
                    (x.Email != null && x.Email.ToLower().Contains(keyword)) ||
                    (x.PrimaryDepartment != null && x.PrimaryDepartment.DepartmentCode.ToLower().Contains(keyword)) ||
                    (x.PrimaryDepartment != null && x.PrimaryDepartment.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.PrimaryPosition != null && x.PrimaryPosition.PositionCode.ToLower().Contains(keyword)) ||
                    (x.PrimaryPosition != null && x.PrimaryPosition.PositionName.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (departmentId.HasValue && departmentId.Value != Guid.Empty)
            {
                query = query.Where(x => x.PrimaryDepartmentId == departmentId.Value);
            }

            if (positionId.HasValue && positionId.Value != Guid.Empty)
            {
                query = query.Where(x => x.PrimaryPositionId == positionId.Value);
            }

            if (employeeStatus.HasValue)
            {
                query = query.Where(x => x.EmployeeStatus == employeeStatus.Value);
            }

            if (professionType.HasValue)
            {
                query = query.Where(x => x.ProfessionType == professionType.Value);
            }

            if (hasTransportAllowanceProfile.HasValue)
            {
                if (hasTransportAllowanceProfile.Value)
                {
                    query = query.Where(x =>
                        _dbContext.Set<EmpTransportAllowanceProfile>()
                            .Any(t =>
                                t.EmployeeId == x.Id &&
                                !t.IsDelete));
                }
                else
                {
                    query = query.Where(x =>
                        !_dbContext.Set<EmpTransportAllowanceProfile>()
                            .Any(t =>
                                t.EmployeeId == x.Id &&
                                !t.IsDelete));
                }
            }

            var totalData = await query.CountAsync();

            var sortedQuery = ApplyEmployeeSorting(query, sortBy, sortDirection);

            var items = await sortedQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new EmployeeResponse
                {
                    Id = x.Id,
                    EmployeeCode = x.EmployeeCode,
                    EmployeeNumber = x.EmployeeNumber,
                    AttendanceNumber = x.AttendanceNumber,
                    FullName = x.FullName,
                    NickName = x.NickName,
                    Gender = x.Gender,
                    PhoneNumber = x.PhoneNumber,
                    WhatsAppNumber = x.WhatsAppNumber,
                    Email = x.Email,
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
                    HasTransportAllowanceProfile = _dbContext.Set<EmpTransportAllowanceProfile>()
                        .Any(t => t.EmployeeId == x.Id && !t.IsDelete),
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

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
                    customPeriod,
                    AppliedStartDate = dateRange.Start,
                    AppliedEndExclusive = dateRange.EndExclusive,
                    search,
                    isActive,
                    departmentId,
                    positionId,
                    employeeStatus,
                    professionType,
                    hasTransportAllowanceProfile,
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
        [ProducesResponseType(typeof(ApiResponse<List<EmployeeOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Employee",
            Description = "Melihat data employee",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Employee", "Read")]
        public async Task<IActionResult> GetEmployeeOptions(
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? positionId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
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
                    x.FullName.ToLower().Contains(keyword) ||
                    (x.EmployeeNumber != null && x.EmployeeNumber.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderBy(x => x.FullName)
                .Select(x => new EmployeeOptionResponse
                {
                    Id = x.Id,
                    EmployeeCode = x.EmployeeCode,
                    FullName = x.FullName,
                    PrimaryDepartmentId = x.PrimaryDepartmentId,
                    PrimaryDepartmentName = x.PrimaryDepartment != null ? x.PrimaryDepartment.DepartmentName : string.Empty,
                    PrimaryPositionId = x.PrimaryPositionId,
                    PrimaryPositionName = x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : string.Empty
                })
                .ToListAsync();

            return Ok(ApiResponse<List<EmployeeOptionResponse>>.Ok(
                data,
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
            var data = await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new EmployeeDetailResponse
                {
                    Id = x.Id,
                    EmployeeCode = x.EmployeeCode,
                    EmployeeNumber = x.EmployeeNumber,
                    AttendanceNumber = x.AttendanceNumber,
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
                    Province = x.Province,
                    City = x.City,
                    District = x.District,
                    Village = x.Village,
                    PostalCode = x.PostalCode,
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
                    HasTransportAllowanceProfile = _dbContext.Set<EmpTransportAllowanceProfile>()
                        .Any(t => t.EmployeeId == x.Id && !t.IsDelete),
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Employee tidak ditemukan."
                ));
            }

            data.TransportAllowanceProfile = await BuildTransportAllowanceProfileResponseAsync(id);

            return Ok(ApiResponse<EmployeeDetailResponse>.Ok(
                data,
                "Detail employee berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
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
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nama employee wajib diisi."
                ));
            }

            var validation = await ValidateEmployeeRequestAsync(
                excludeEmployeeId: null,
                primaryDepartmentId: request.PrimaryDepartmentId,
                primaryPositionId: request.PrimaryPositionId,
                employeeNumber: request.EmployeeNumber,
                attendanceNumber: request.AttendanceNumber,
                identityNumber: request.IdentityNumber,
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

            var entity = new MstEmployee
            {
                Id = Guid.NewGuid(),
                EmployeeCode = await GenerateEmployeeCodeAsync(),
                EmployeeNumber = NormalizeNullableText(request.EmployeeNumber),
                AttendanceNumber = NormalizeNullableText(request.AttendanceNumber),
                FullName = request.FullName.Trim(),
                NickName = NormalizeNullableText(request.NickName),
                Gender = request.Gender,
                BirthPlace = NormalizeNullableText(request.BirthPlace),
                BirthDate = request.BirthDate,
                Religion = NormalizeNullableText(request.Religion),
                MaritalStatus = NormalizeNullableText(request.MaritalStatus),
                BloodType = NormalizeNullableText(request.BloodType),
                IdentityType = NormalizeNullableText(request.IdentityType),
                IdentityNumber = NormalizeNullableText(request.IdentityNumber),
                PhoneNumber = NormalizeNullableText(request.PhoneNumber),
                WhatsAppNumber = NormalizeNullableText(request.WhatsAppNumber),
                Email = NormalizeNullableText(request.Email),
                Address = NormalizeNullableText(request.Address),
                Province = NormalizeNullableText(request.Province),
                City = NormalizeNullableText(request.City),
                District = NormalizeNullableText(request.District),
                Village = NormalizeNullableText(request.Village),
                PostalCode = NormalizeNullableText(request.PostalCode),
                PrimaryDepartmentId = request.PrimaryDepartmentId,
                PrimaryPositionId = request.PrimaryPositionId,
                EmployeeStatus = request.EmployeeStatus,
                ProfessionType = request.ProfessionType,
                EmploymentType = NormalizeNullableText(request.EmploymentType),
                GradeLevel = NormalizeNullableText(request.GradeLevel),
                WorkLocation = NormalizeNullableText(request.WorkLocation),
                JoinDate = request.JoinDate,
                ProbationEndDate = request.ProbationEndDate,
                ContractStartDate = request.ContractStartDate,
                ContractEndDate = request.ContractEndDate,
                EmergencyContactName = NormalizeNullableText(request.EmergencyContactName),
                EmergencyContactRelation = NormalizeNullableText(request.EmergencyContactRelation),
                EmergencyContactPhone = NormalizeNullableText(request.EmergencyContactPhone),
                EmergencyContactAddress = NormalizeNullableText(request.EmergencyContactAddress),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = userId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstEmployee>().Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Employee.CreateEmployee",
                "Employee berhasil dibuat.",
                new
                {
                    entity.Id,
                    entity.EmployeeCode,
                    entity.FullName,
                    entity.PrimaryDepartmentId,
                    entity.PrimaryPositionId
                }
            );

            return Ok(ApiResponse<object>.Ok(
                new
                {
                    entity.Id,
                    entity.EmployeeCode,
                    entity.FullName,
                    entity.IsActive
                },
                "Employee berhasil dibuat."
            ));
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

            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nama employee wajib diisi."
                ));
            }

            var validation = await ValidateEmployeeRequestAsync(
                excludeEmployeeId: id,
                primaryDepartmentId: request.PrimaryDepartmentId,
                primaryPositionId: request.PrimaryPositionId,
                employeeNumber: request.EmployeeNumber,
                attendanceNumber: request.AttendanceNumber,
                identityNumber: request.IdentityNumber,
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

            entity.EmployeeNumber = NormalizeNullableText(request.EmployeeNumber);
            entity.AttendanceNumber = NormalizeNullableText(request.AttendanceNumber);
            entity.FullName = request.FullName.Trim();
            entity.NickName = NormalizeNullableText(request.NickName);
            entity.Gender = request.Gender;
            entity.BirthPlace = NormalizeNullableText(request.BirthPlace);
            entity.BirthDate = request.BirthDate;
            entity.Religion = NormalizeNullableText(request.Religion);
            entity.MaritalStatus = NormalizeNullableText(request.MaritalStatus);
            entity.BloodType = NormalizeNullableText(request.BloodType);
            entity.IdentityType = NormalizeNullableText(request.IdentityType);
            entity.IdentityNumber = NormalizeNullableText(request.IdentityNumber);
            entity.PhoneNumber = NormalizeNullableText(request.PhoneNumber);
            entity.WhatsAppNumber = NormalizeNullableText(request.WhatsAppNumber);
            entity.Email = NormalizeNullableText(request.Email);
            entity.Address = NormalizeNullableText(request.Address);
            entity.Province = NormalizeNullableText(request.Province);
            entity.City = NormalizeNullableText(request.City);
            entity.District = NormalizeNullableText(request.District);
            entity.Village = NormalizeNullableText(request.Village);
            entity.PostalCode = NormalizeNullableText(request.PostalCode);
            entity.PrimaryDepartmentId = request.PrimaryDepartmentId;
            entity.PrimaryPositionId = request.PrimaryPositionId;
            entity.EmployeeStatus = request.EmployeeStatus;
            entity.ProfessionType = request.ProfessionType;
            entity.EmploymentType = NormalizeNullableText(request.EmploymentType);
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
            entity.EmergencyContactPhone = NormalizeNullableText(request.EmergencyContactPhone);
            entity.EmergencyContactAddress = NormalizeNullableText(request.EmergencyContactAddress);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = userId;

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Employee.UpdateEmployee",
                "Employee berhasil diperbarui.",
                new
                {
                    entity.Id,
                    entity.EmployeeCode,
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

            entity.IsActive = request.IsActive;
            entity.ResignDate = request.ResignDate;
            entity.ResignReason = NormalizeNullableText(request.ResignReason);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status employee berhasil diperbarui."
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

            var profile = await _dbContext.Set<EmpTransportAllowanceProfile>()
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == id &&
                    !x.IsDelete);

            if (profile != null)
            {
                profile.IsActive = false;
                profile.UpdateDateTime = now;
                profile.UpdateBy = userId;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Employee berhasil dihapus."
            ));
        }

        // =========================================================
        // EMPLOYEE TRANSPORT ALLOWANCE
        // =========================================================

        [HttpGet("{employeeId:guid}/transport-allowance")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeTransportAllowanceProfileResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Employee",
            Description = "Melihat data employee",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Employee", "Read")]
        public async Task<IActionResult> GetTransportAllowanceProfile(Guid employeeId)
        {
            var exists = await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == employeeId &&
                    !x.IsDelete);

            if (!exists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Employee tidak ditemukan."
                ));
            }

            var data = await BuildTransportAllowanceProfileResponseAsync(employeeId);

            return Ok(ApiResponse<EmployeeTransportAllowanceProfileResponse>.Ok(
                data,
                data.IsConfigured
                    ? "Profile uang transport employee berhasil diambil."
                    : "Profile uang transport employee belum dibuat."
            ));
        }

        [HttpPut("{employeeId:guid}/transport-allowance")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Employee",
            Description = "Mengubah data employee",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Employee", "Update")]
        public async Task<IActionResult> UpsertTransportAllowanceProfile(
            Guid employeeId,
            [FromBody] UpsertEmployeeTransportAllowanceProfileRequest request)
        {
            var employee = await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == employeeId &&
                    !x.IsDelete);

            if (employee == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Employee tidak ditemukan."
                ));
            }

            var normalizedMode = NormalizeAllowanceMode(request.AllowanceMode);

            if (!AllowanceModes.Contains(normalizedMode))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "AllowanceMode tidak valid."
                ));
            }

            if (request.MonthlyAmount < 0 ||
                request.DailyAmount < 0 ||
                request.NightAmount < 0)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nominal uang transport tidak boleh negatif."
                ));
            }

            var effectiveStartDate = request.EffectiveStartDate?.Date ?? DateTime.UtcNow.Date;
            var effectiveEndDate = request.EffectiveEndDate?.Date;

            if (effectiveEndDate.HasValue && effectiveStartDate > effectiveEndDate.Value)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "EffectiveStartDate tidak boleh lebih besar dari EffectiveEndDate."
                ));
            }

            var now = DateTime.UtcNow;
            var userId = GetCurrentUserId();

            var profile = await _dbContext.Set<EmpTransportAllowanceProfile>()
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == employeeId &&
                    !x.IsDelete);

            if (profile == null)
            {
                profile = new EmpTransportAllowanceProfile
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = employeeId,
                    CreateDateTime = now,
                    CreateBy = userId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<EmpTransportAllowanceProfile>().Add(profile);
            }
            else
            {
                profile.UpdateDateTime = now;
                profile.UpdateBy = userId;
            }

            profile.IsEligible = request.IsEligible;
            profile.IsNightTransportEligible = request.IsNightTransportEligible;
            profile.AllowanceMode = normalizedMode;
            profile.MonthlyAmount = request.MonthlyAmount;
            profile.DailyAmount = request.DailyAmount;
            profile.NightAmount = request.NightAmount;
            profile.IsProrated = request.IsProrated;
            profile.IsTaxable = request.IsTaxable;
            profile.IsPayrollComponent = request.IsPayrollComponent;
            profile.EffectiveStartDate = effectiveStartDate;
            profile.EffectiveEndDate = effectiveEndDate;
            profile.Description = NormalizeNullableText(request.Description);
            profile.IsActive = request.IsActive;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                new
                {
                    profile.Id,
                    profile.EmployeeId,
                    employee.EmployeeCode,
                    employee.FullName,
                    profile.AllowanceMode,
                    profile.MonthlyAmount,
                    profile.DailyAmount,
                    profile.NightAmount,
                    profile.IsEligible,
                    profile.IsNightTransportEligible
                },
                "Profile uang transport employee berhasil disimpan."
            ));
        }

        [HttpGet("{employeeId:guid}/transport-allowance/transactions")]
        [ProducesResponseType(typeof(ApiResponse<ResponseTransportAllowanceTransactionPagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Employee",
            Description = "Melihat data employee",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Employee", "Read")]
        public async Task<IActionResult> GetTransportAllowanceTransactions(
            Guid employeeId,
            [FromQuery] string? periodYearMonth,
            [FromQuery] string? allowanceType,
            [FromQuery] string? transactionStatus,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var exists = await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == employeeId &&
                    !x.IsDelete);

            if (!exists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Employee tidak ditemukan."
                ));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<EmpTransportAllowanceTransaction>()
                .AsNoTracking()
                .Where(x =>
                    x.EmployeeId == employeeId &&
                    !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(periodYearMonth))
            {
                var period = periodYearMonth.Trim();
                query = query.Where(x => x.PeriodYearMonth == period);
            }

            if (!string.IsNullOrWhiteSpace(allowanceType))
            {
                var type = allowanceType.Trim();
                query = query.Where(x => x.AllowanceType == type);
            }

            if (!string.IsNullOrWhiteSpace(transactionStatus))
            {
                var status = transactionStatus.Trim();
                query = query.Where(x => x.TransactionStatus == status);
            }

            if (startDate.HasValue)
            {
                query = query.Where(x => x.TransactionDate >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(x => x.TransactionDate < endDate.Value.Date.AddDays(1));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.TransactionDate)
                .ThenByDescending(x => x.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new EmployeeTransportAllowanceTransactionResponse
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    EmployeeCode = x.Employee != null ? x.Employee.EmployeeCode : string.Empty,
                    EmployeeName = x.Employee != null ? x.Employee.FullName : string.Empty,
                    TransactionDate = x.TransactionDate,
                    PeriodYearMonth = x.PeriodYearMonth,
                    AllowanceType = x.AllowanceType,
                    Amount = x.Amount,
                    IsGeneratedFromAttendance = x.IsGeneratedFromAttendance,
                    IsNightShift = x.IsNightShift,
                    TransactionStatus = x.TransactionStatus,
                    Notes = x.Notes,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new ResponseTransportAllowanceTransactionPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseTransportAllowanceTransactionPagedResult>.Ok(
                result,
                "Data transaksi uang transport employee berhasil diambil."
            ));
        }

        // =========================================================
        // PRIVATE HELPERS
        // =========================================================

        private async Task<string> GenerateEmployeeCodeAsync()
        {
            const string prefix = "EMP";

            var totalData = await _dbContext.Set<MstEmployee>()
                .IgnoreQueryFilters()
                .CountAsync();

            var nextNumber = totalData + 1;

            while (true)
            {
                var code = $"{prefix}{nextNumber.ToString("D6")}";

                var exists = await _dbContext.Set<MstEmployee>()
                    .AnyAsync(x => x.EmployeeCode == code);

                if (!exists)
                {
                    return code;
                }

                nextNumber++;
            }
        }

        private async Task<EmployeeTransportAllowanceProfileResponse> BuildTransportAllowanceProfileResponseAsync(
            Guid employeeId)
        {
            var employee = await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .Where(x => x.Id == employeeId && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.EmployeeCode,
                    x.FullName
                })
                .FirstAsync();

            var profile = await _dbContext.Set<EmpTransportAllowanceProfile>()
                .AsNoTracking()
                .Where(x =>
                    x.EmployeeId == employeeId &&
                    !x.IsDelete)
                .Select(x => new EmployeeTransportAllowanceProfileResponse
                {
                    IsConfigured = true,
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    EmployeeCode = employee.EmployeeCode,
                    EmployeeName = employee.FullName,
                    IsEligible = x.IsEligible,
                    IsNightTransportEligible = x.IsNightTransportEligible,
                    AllowanceMode = x.AllowanceMode,
                    MonthlyAmount = x.MonthlyAmount,
                    DailyAmount = x.DailyAmount,
                    NightAmount = x.NightAmount,
                    IsProrated = x.IsProrated,
                    IsTaxable = x.IsTaxable,
                    IsPayrollComponent = x.IsPayrollComponent,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync();

            if (profile != null)
            {
                return profile;
            }

            return new EmployeeTransportAllowanceProfileResponse
            {
                IsConfigured = false,
                Id = null,
                EmployeeId = employee.Id,
                EmployeeCode = employee.EmployeeCode,
                EmployeeName = employee.FullName,
                IsEligible = false,
                IsNightTransportEligible = false,
                AllowanceMode = "None",
                MonthlyAmount = 0,
                DailyAmount = 0,
                NightAmount = 0,
                IsProrated = true,
                IsTaxable = true,
                IsPayrollComponent = true,
                EffectiveStartDate = null,
                EffectiveEndDate = null,
                IsActive = false
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateEmployeeRequestAsync(
            Guid? excludeEmployeeId,
            Guid primaryDepartmentId,
            Guid primaryPositionId,
            string? employeeNumber,
            string? attendanceNumber,
            string? identityNumber,
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
                .AnyAsync(x =>
                    x.Id == primaryDepartmentId &&
                    x.IsActive &&
                    !x.IsDelete);

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

            var normalizedEmployeeNumber = NormalizeNullableText(employeeNumber);
            var normalizedAttendanceNumber = NormalizeNullableText(attendanceNumber);
            var normalizedIdentityNumber = NormalizeNullableText(identityNumber);
            var normalizedEmail = NormalizeNullableText(email);

            if (!string.IsNullOrWhiteSpace(normalizedEmployeeNumber))
            {
                var exists = await _dbContext.Set<MstEmployee>()
                    .AnyAsync(x =>
                        x.Id != excludeEmployeeId &&
                        x.EmployeeNumber == normalizedEmployeeNumber &&
                        !x.IsDelete);

                if (exists)
                {
                    return (false, "Employee number sudah digunakan.");
                }
            }

            if (!string.IsNullOrWhiteSpace(normalizedAttendanceNumber))
            {
                var exists = await _dbContext.Set<MstEmployee>()
                    .AnyAsync(x =>
                        x.Id != excludeEmployeeId &&
                        x.AttendanceNumber == normalizedAttendanceNumber &&
                        !x.IsDelete);

                if (exists)
                {
                    return (false, "Attendance number sudah digunakan.");
                }
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

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
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

                "attendancenumber" => desc
                    ? query.OrderByDescending(x => x.AttendanceNumber).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.AttendanceNumber).ThenBy(x => x.FullName),

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

        private static string NormalizeAllowanceMode(string? allowanceMode)
        {
            if (string.IsNullOrWhiteSpace(allowanceMode))
            {
                return "None";
            }

            var mode = allowanceMode.Trim();

            return AllowanceModes.FirstOrDefault(x =>
                x.Equals(mode, StringComparison.OrdinalIgnoreCase)) ?? mode;
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

        private sealed class DateRangeResolveResult
        {
            public bool IsValid { get; private set; }

            public string? ErrorMessage { get; private set; }

            public DateTime? Start { get; private set; }

            public DateTime? EndExclusive { get; private set; }

            public static DateRangeResolveResult Valid(
                DateTime? start,
                DateTime? endExclusive)
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