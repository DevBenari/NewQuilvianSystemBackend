using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.UserManagement.DTOs;
using QuilvianSystemBackend.Areas.Administrator.UserManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using UserTypeEnum = QuilvianSystemBackend.Enum.UserType;
using DoctorTypeEnum = QuilvianSystemBackend.Areas.Administrator.UserManagement.Enum.DoctorType;
using EmployeeStatusEnum = QuilvianSystemBackend.Areas.Administrator.UserManagement.Enum.EmployeeStatus;
using ExternalUserTypeEnum = QuilvianSystemBackend.Areas.Administrator.UserManagement.Enum.ExternalUserType;
using GenderEnum = QuilvianSystemBackend.Areas.Administrator.UserManagement.Enum.Gender;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;

namespace QuilvianSystemBackend.Areas.Administrator.UserManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/administrator/user-management/users")]
    [Tags("User Management / User")]
    [AccessController(
        moduleCode: "USER_MANAGEMENT",
        moduleName: "User Management",
        displayName: "User Management",
        AreaName = "Administrator",
        ControllerName = "UserManagement",
        Description = "Pengelolaan user aplikasi",
        SortOrder = 1
    )]
    public class UserManagementController : ControllerBase
    {
        private const string DefaultUserRole = "User";
        private const string UserCodePrefix = "USR-RSMMC";
        private const string DefaultProfilePhotoFileName = "user.jpeg";

        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly LoggerService _loggerService;
        private readonly IConfiguration _configuration;

        public UserManagementController(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            LoggerService loggerService,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _loggerService = loggerService;
            _configuration = configuration;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<UserManagementResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "Index",
            displayName: "Lihat Data",
            Description = "Melihat daftar user",
            SortOrder = 1
        )]
        [AccessPermission("UserManagement", "Index")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] UserTypeEnum? userType,
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? positionId,
            [FromQuery] bool? isActive,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;
            pageSize = pageSize > 100 ? 100 : pageSize;

            var query = _dbContext.Users
                .AsNoTracking()
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Include(x => x.Employee)
                .Include(x => x.Doctor)
                .Include(x => x.ExternalUser)
                .Where(x => x.UserType != UserTypeEnum.SuperAdmin);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.UserCode.ToLower().Contains(keyword) ||
                    (x.UserName != null && x.UserName.ToLower().Contains(keyword)) ||
                    (x.Email != null && x.Email.ToLower().Contains(keyword)) ||
                    x.FullName.ToLower().Contains(keyword) ||
                    (x.IdentityNumber != null && x.IdentityNumber.ToLower().Contains(keyword)) ||
                    (x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(keyword)) ||
                    (x.Department != null && x.Department.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.Position != null && x.Position.PositionName.ToLower().Contains(keyword)) ||
                    (x.Employee != null && x.Employee.FullName.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.FullName.ToLower().Contains(keyword)) ||
                    (x.ExternalUser != null && x.ExternalUser.FullName.ToLower().Contains(keyword)));
            }

            if (userType.HasValue)
            {
                query = query.Where(x => x.UserType == userType.Value);
            }

            if (departmentId.HasValue)
            {
                query = query.Where(x => x.DepartmentId == departmentId.Value);
            }

            if (positionId.HasValue)
            {
                query = query.Where(x => x.PositionId == positionId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            var totalData = await query.CountAsync();

            var users = await query
                .OrderBy(x => x.FullName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = new List<UserManagementResponse>();

            foreach (var user in users)
            {
                items.Add(await BuildUserResponseAsync(user));
            }

            var result = new PagedResult<UserManagementResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PagedResult<UserManagementResponse>>.Ok(
                result,
                "Daftar user berhasil diambil."
            ));
        }

        [HttpGet("create-options")]
        [ProducesResponseType(typeof(ApiResponse<CreateUserOptionsResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "CreateOptions",
            displayName: "Pilihan Form Tambah User",
            Description = "Mengambil pilihan enum dan aturan form pembuatan user",
            SortOrder = 2
        )]
        [AccessPermission("UserManagement", "Create")]
        public IActionResult GetCreateOptions()
        {
            var response = new CreateUserOptionsResponse
            {
                UsernameSource = "Email",
                PasswordRule = "Password otomatis dari tanggal lahir format ddMMMyyyy Indonesia, contoh 18Des1990.",
                DefaultProfilePhotoPath = BuildDefaultProfilePhotoUrl(),

                UserTypes = new List<EnumOptionResponse>
                {
                    ToEnumOption(UserTypeEnum.Employee, "Employee / Karyawan"),
                    ToEnumOption(UserTypeEnum.PermanentDoctor, "Dokter Tetap"),
                    ToEnumOption(UserTypeEnum.GuestDoctor, "Dokter Tamu"),
                    ToEnumOption(UserTypeEnum.ExternalUser, "User Eksternal"),
                    ToEnumOption(UserTypeEnum.Vendor, "Vendor")
                },

                Genders = BuildEnumOptions<GenderEnum>(),

                RequiredFields = new List<string>
                {
                    "fullName",
                    "userType",
                    "birthDate",
                    "identityNumber",
                    "email",
                    "phoneNumber",
                    "departmentId",
                    "positionId"
                },

                OptionalFields = new List<string>
                {
                    "gender",
                    "accessValidUntil",
                    "address"
                },

                AutoGeneratedFields = new List<string>
                {
                    "userCode",
                    "username",
                    "initialPassword",
                    "profilePhotoPath",
                    "employeeId",
                    "doctorId",
                    "externalUserId",
                    "employeeCode",
                    "employeeNumber",
                    "doctorCode",
                    "externalCode"
                }
            };

            return Ok(ApiResponse<CreateUserOptionsResponse>.Ok(
                response,
                "Pilihan form user berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<UserManagementResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            actionName: "Detail",
            displayName: "Detail Data",
            Description = "Melihat detail user",
            SortOrder = 3
        )]
        [AccessPermission("UserManagement", "Detail")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await GetUserWithRelationsAsync(id);

            if (user == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "User tidak ditemukan."
                ));
            }

            var response = await BuildUserResponseAsync(user);

            return Ok(ApiResponse<UserManagementResponse>.Ok(
                response,
                "Detail user berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CreateUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            actionName: "Create",
            displayName: "Tambah Data",
            Description = "Membuat user baru",
            SortOrder = 4
        )]
        [AccessPermission("UserManagement", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    GetModelStateErrorMessage()
                ));
            }

            var validation = await ValidateProvisionCreateRequestAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            var roleExists = await _roleManager.RoleExistsAsync(DefaultUserRole);

            if (!roleExists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Role User belum tersedia."
                ));
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var userCode = await GenerateUserCodeAsync();
                var username = request.Email.Trim().ToLowerInvariant();
                var email = request.Email.Trim().ToLowerInvariant();
                var initialPassword = GeneratePasswordFromBirthDate(request.BirthDate);
                var defaultPhotoPath = BuildDefaultProfilePhotoUrl();

                var profileResult = await ProvisionProfileAsync(request);

                if (!profileResult.IsValid)
                {
                    await transaction.RollbackAsync();

                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        profileResult.Message
                    ));
                }

                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserCode = userCode,

                    UserName = username,
                    NormalizedUserName = username.ToUpperInvariant(),

                    Email = email,
                    NormalizedEmail = email.ToUpperInvariant(),
                    EmailConfirmed = true,

                    PhoneNumber = Normalize(request.PhoneNumber),
                    FullName = request.FullName.Trim(),
                    UserType = request.UserType,

                    BirthDate = request.BirthDate.Date,
                    IdentityNumber = Normalize(request.IdentityNumber),

                    DepartmentId = profileResult.DepartmentId,
                    PositionId = profileResult.PositionId,
                    EmployeeId = profileResult.EmployeeId,
                    DoctorId = profileResult.DoctorId,
                    ExternalUserId = profileResult.ExternalUserId,

                    IsActive = true,
                    MustChangePassword = true,
                    AccessValidUntil = request.AccessValidUntil,
                    ProfilePhotoPath = defaultPhotoPath,
                    CreateDateTime = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user, initialPassword);

                if (!createResult.Succeeded)
                {
                    await transaction.RollbackAsync();

                    var errors = string.Join(", ", createResult.Errors.Select(x => x.Description));

                    await _loggerService.WarningAsync(
                        "UserManagement",
                        "User.Create",
                        "Gagal membuat user.",
                        new
                        {
                            userCode,
                            username,
                            email,
                            Errors = errors
                        }
                    );

                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        errors
                    ));
                }

                var addRoleResult = await _userManager.AddToRoleAsync(user, DefaultUserRole);

                if (!addRoleResult.Succeeded)
                {
                    await transaction.RollbackAsync();

                    var errors = string.Join(", ", addRoleResult.Errors.Select(x => x.Description));

                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        $"Gagal assign role User: {errors}"
                    ));
                }

                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    "UserManagement",
                    "User.Create",
                    "User berhasil dibuat.",
                    new
                    {
                        UserId = user.Id,
                        user.UserCode,
                        Username = user.UserName,
                        Email = user.Email,
                        user.UserType,
                        user.DepartmentId,
                        user.PositionId,
                        user.EmployeeId,
                        user.DoctorId,
                        user.ExternalUserId
                    }
                );

                var savedUser = await GetUserWithRelationsAsync(user.Id);
                var userResponse = await BuildUserResponseAsync(savedUser!);

                var response = new CreateUserResponse
                {
                    User = userResponse,
                    InitialPassword = initialPassword
                };

                return Ok(ApiResponse<CreateUserResponse>.Ok(
                    response,
                    "User berhasil dibuat."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    "UserManagement",
                    "User.Create",
                    "Terjadi error saat membuat user.",
                    ex,
                    new
                    {
                        request.FullName,
                        request.Email,
                        request.UserType
                    }
                );

                var errorMessage = ex.InnerException?.Message ?? ex.Message;

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Terjadi error saat membuat user: {errorMessage}"
                ));
            }
        }

        [HttpPut("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            actionName: "UpdateStatus",
            displayName: "Ubah Status",
            Description = "Mengaktifkan atau menonaktifkan user",
            SortOrder = 5
        )]
        [AccessPermission("UserManagement", "UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] bool isActive)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "User tidak ditemukan."
                ));
            }

            if (user.UserType == UserTypeEnum.SuperAdmin)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Status SuperAdmin tidak boleh diubah dari menu ini."
                ));
            }

            user.IsActive = isActive;
            user.UpdateDateTime = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(x => x.Description));

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    errors
                ));
            }

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status user berhasil diperbarui."
            ));
        }

        [HttpPut("{id:guid}/reset-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            actionName: "ResetPassword",
            displayName: "Reset Password",
            Description = "Reset password user",
            SortOrder = 6
        )]
        [AccessPermission("UserManagement", "ResetPassword")]
        public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetUserPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    GetModelStateErrorMessage()
                ));
            }

            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "User tidak ditemukan."
                ));
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(
                user,
                resetToken,
                request.NewPassword
            );

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(x => x.Description));

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    errors
                ));
            }

            user.MustChangePassword = request.MustChangePassword;
            user.UpdateDateTime = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Password user berhasil direset."
            ));
        }

        private async Task<(bool IsValid, string Message)> ValidateProvisionCreateRequestAsync(CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return (false, "Nama lengkap wajib diisi.");
            }

            if (request.UserType == UserTypeEnum.SuperAdmin)
            {
                return (false, "User SuperAdmin tidak boleh dibuat dari menu ini.");
            }

            if (request.UserType == UserTypeEnum.Patient)
            {
                return (false, "UserType Patient belum didukung dari menu user management ini.");
            }

            if (request.UserType != UserTypeEnum.Employee &&
                request.UserType != UserTypeEnum.PermanentDoctor &&
                request.UserType != UserTypeEnum.GuestDoctor &&
                request.UserType != UserTypeEnum.ExternalUser &&
                request.UserType != UserTypeEnum.Vendor)
            {
                return (false, "UserType tidak valid untuk pembuatan akun dari menu ini.");
            }

            if (request.BirthDate == default)
            {
                return (false, "Tanggal lahir wajib diisi.");
            }

            if (request.BirthDate.Date > DateTime.UtcNow.Date)
            {
                return (false, "Tanggal lahir tidak boleh lebih besar dari tanggal hari ini.");
            }

            if (string.IsNullOrWhiteSpace(request.IdentityNumber))
            {
                return (false, "Nomor identitas wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return (false, "Email wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return (false, "Nomor handphone wajib diisi.");
            }

            if (request.DepartmentId == Guid.Empty)
            {
                return (false, "Department wajib dipilih.");
            }

            if (request.PositionId == Guid.Empty)
            {
                return (false, "Position wajib dipilih.");
            }

            var departmentPositionValidation = await ValidateDepartmentAndPositionAsync(
                request.DepartmentId,
                request.PositionId
            );

            if (!departmentPositionValidation.IsValid)
            {
                return departmentPositionValidation;
            }

            var email = request.Email.Trim().ToLowerInvariant();

            var usernameExists = await _userManager.FindByNameAsync(email);

            if (usernameExists != null)
            {
                return (false, "Username/email sudah digunakan.");
            }

            var emailExists = await _userManager.FindByEmailAsync(email);

            if (emailExists != null)
            {
                return (false, "Email sudah digunakan.");
            }

            var identityNumber = request.IdentityNumber.Trim();

            var identityExists = await _dbContext.Users
                .AnyAsync(x =>
                    x.IdentityNumber != null &&
                    x.IdentityNumber == identityNumber);

            if (identityExists)
            {
                return (false, "Nomor identitas sudah digunakan oleh user lain.");
            }           

            return (true, string.Empty);
        }

        private async Task<UserProfileProvisionResult> ProvisionProfileAsync(CreateUserRequest request)
        {
            return request.UserType switch
            {
                UserTypeEnum.Employee =>
                    await CreateMinimalEmployeeProfileAsync(request),

                UserTypeEnum.PermanentDoctor =>
                    await CreateMinimalDoctorProfileAsync(request, DoctorTypeEnum.PermanentDoctor),

                UserTypeEnum.GuestDoctor =>
                    await CreateMinimalDoctorProfileAsync(request, DoctorTypeEnum.GuestDoctor),

                UserTypeEnum.ExternalUser =>
                    await CreateMinimalExternalProfileAsync(request),

                UserTypeEnum.Vendor =>
                    await CreateMinimalExternalProfileAsync(request),

                _ => UserProfileProvisionResult.Fail("UserType belum didukung untuk pembuatan akun user.")
            };
        }

        private async Task<UserProfileProvisionResult> CreateMinimalEmployeeProfileAsync(CreateUserRequest request)
        {
            var employeeCode = await GenerateEmployeeCodeAsync();
            var employeeNumber = await GenerateEmployeeNumberAsync();

            var employee = new MstEmployee
            {
                Id = Guid.NewGuid(),
                EmployeeCode = employeeCode,
                EmployeeNumber = employeeNumber,
                FullName = request.FullName.Trim(),
                Gender = request.Gender,
                BirthDate = request.BirthDate.Date,
                IdentityNumber = Normalize(request.IdentityNumber),
                Email = Normalize(request.Email),
                PhoneNumber = Normalize(request.PhoneNumber),
                WhatsAppNumber = Normalize(request.PhoneNumber),
                Address = Normalize(request.Address),
                DepartmentId = request.DepartmentId,
                PositionId = request.PositionId,
                EmployeeStatus = EmployeeStatusEnum.Contract,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstEmployees.Add(employee);

            await _dbContext.SaveChangesAsync();

            return UserProfileProvisionResult.Ok(
                departmentId: employee.DepartmentId,
                positionId: employee.PositionId,
                employeeId: employee.Id,
                doctorId: null,
                externalUserId: null
            );
        }

        private async Task<UserProfileProvisionResult> CreateMinimalDoctorProfileAsync(
            CreateUserRequest request,
            DoctorTypeEnum doctorType)
        {
            var doctorCode = await GenerateDoctorCodeAsync();

            var doctor = new MstDoctor
            {
                Id = Guid.NewGuid(),
                DoctorCode = doctorCode,
                FullName = request.FullName.Trim(),
                DoctorType = doctorType,
                Gender = request.Gender,
                BirthDate = request.BirthDate.Date,
                IdentityNumber = Normalize(request.IdentityNumber),
                Email = Normalize(request.Email),
                PhoneNumber = Normalize(request.PhoneNumber),
                WhatsAppNumber = Normalize(request.PhoneNumber),
                Address = Normalize(request.Address),
                DepartmentId = request.DepartmentId,
                PositionId = request.PositionId,
                IsAvailableForAppointment = false,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstDoctors.Add(doctor);

            await _dbContext.SaveChangesAsync();

            return UserProfileProvisionResult.Ok(
                departmentId: request.DepartmentId,
                positionId: request.PositionId,
                employeeId: null,
                doctorId: doctor.Id,
                externalUserId: null
            );
        }

        private async Task<UserProfileProvisionResult> CreateMinimalExternalProfileAsync(CreateUserRequest request)
        {
            var externalCode = await GenerateExternalCodeAsync();

            var externalUserType = request.UserType == UserTypeEnum.Vendor
                ? ExternalUserTypeEnum.Vendor
                : ExternalUserTypeEnum.Other;

            var externalUser = new MstExternalUser
            {
                Id = Guid.NewGuid(),
                ExternalCode = externalCode,
                ExternalUserType = externalUserType,
                FullName = request.FullName.Trim(),
                PhoneNumber = Normalize(request.PhoneNumber),
                WhatsAppNumber = Normalize(request.PhoneNumber),
                Email = Normalize(request.Email),
                Address = Normalize(request.Address),
                IdentityNumber = Normalize(request.IdentityNumber),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstExternalUsers.Add(externalUser);

            await _dbContext.SaveChangesAsync();

            return UserProfileProvisionResult.Ok(
                departmentId: request.DepartmentId,
                positionId: request.PositionId,
                employeeId: null,
                doctorId: null,
                externalUserId: externalUser.Id
            );
        }

        private async Task<string> GenerateUserCodeAsync()
        {
            var totalData = await _dbContext.Users
                .IgnoreQueryFilters()
                .CountAsync();

            var nextNumber = totalData + 1;

            while (true)
            {
                var code = $"{UserCodePrefix}-{nextNumber.ToString("D5")}";

                var exists = await _dbContext.Users
                    .AnyAsync(x => x.UserCode == code);

                if (!exists)
                {
                    return code;
                }

                nextNumber++;
            }
        }

        private async Task<string> GenerateEmployeeCodeAsync()
        {
            const string prefix = "EMP-RSMMC";

            var totalData = await _dbContext.MstEmployees
                .IgnoreQueryFilters()
                .CountAsync();

            var nextNumber = totalData + 1;

            while (true)
            {
                var code = $"{prefix}-{nextNumber.ToString("D5")}";

                var exists = await _dbContext.MstEmployees
                    .AnyAsync(x => x.EmployeeCode == code);

                if (!exists)
                {
                    return code;
                }

                nextNumber++;
            }
        }

        private async Task<string> GenerateEmployeeNumberAsync()
        {
            var now = GetSystemNow();
            var datePart = now.ToString("yyyyMMdd");
            var prefix = $"RSMMC-{datePart}";

            var todayCount = await _dbContext.MstEmployees
                .IgnoreQueryFilters()
                .CountAsync(x =>
                    x.EmployeeNumber != null &&
                    x.EmployeeNumber.StartsWith(prefix));

            var nextNumber = todayCount + 1;

            while (true)
            {
                var employeeNumber = $"{prefix}{nextNumber.ToString("D3")}";

                var exists = await _dbContext.MstEmployees
                    .AnyAsync(x => x.EmployeeNumber == employeeNumber);

                if (!exists)
                {
                    return employeeNumber;
                }

                nextNumber++;
            }
        }

        private async Task<string> GenerateDoctorCodeAsync()
        {
            const string prefix = "DCT-RSMMC";

            var totalData = await _dbContext.MstDoctors
                .IgnoreQueryFilters()
                .CountAsync();

            var nextNumber = totalData + 1;

            while (true)
            {
                var code = $"{prefix}-{nextNumber.ToString("D5")}";

                var exists = await _dbContext.MstDoctors
                    .AnyAsync(x => x.DoctorCode == code);

                if (!exists)
                {
                    return code;
                }

                nextNumber++;
            }
        }

        private async Task<string> GenerateExternalCodeAsync()
        {
            const string prefix = "EXT-RSMMC";

            var totalData = await _dbContext.MstExternalUsers
                .IgnoreQueryFilters()
                .CountAsync();

            var nextNumber = totalData + 1;

            while (true)
            {
                var code = $"{prefix}-{nextNumber.ToString("D5")}";

                var exists = await _dbContext.MstExternalUsers
                    .AnyAsync(x => x.ExternalCode == code);

                if (!exists)
                {
                    return code;
                }

                nextNumber++;
            }
        }

        private static string GeneratePasswordFromBirthDate(DateTime birthDate)
        {
            var monthNames = new[]
            {
                string.Empty,
                "Jan",
                "Feb",
                "Mar",
                "Apr",
                "Mei",
                "Jun",
                "Jul",
                "Agu",
                "Sep",
                "Okt",
                "Nov",
                "Des"
            };

            return $"{birthDate.Day.ToString("D2")}{monthNames[birthDate.Month]}{birthDate.Year}!";
        }

        private string BuildDefaultProfilePhotoUrl()
        {
            var uploadUrl = _configuration["FileStorage:UploadUrl"]?.Trim();
            var folder = _configuration["FileStorage:ProfilePhotoFolder"]?.Trim();

            if (string.IsNullOrWhiteSpace(uploadUrl))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(folder))
            {
                folder = "default-photo";
            }

            uploadUrl = uploadUrl.TrimEnd('/');
            folder = folder.Trim('/');

            if (folder.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
            {
                folder = folder["uploads/".Length..];
            }

            if (folder.StartsWith("upload/", StringComparison.OrdinalIgnoreCase))
            {
                folder = folder["upload/".Length..];
            }

            var baseUrl = uploadUrl;

            if (baseUrl.EndsWith("/upload", StringComparison.OrdinalIgnoreCase))
            {
                baseUrl = baseUrl[..^"/upload".Length];
            }

            if (baseUrl.EndsWith("/uploads", StringComparison.OrdinalIgnoreCase))
            {
                baseUrl = baseUrl[..^"/uploads".Length];
            }

            return $"{baseUrl}/uploads/{folder}/{DefaultProfilePhotoFileName}";
        }

        private async Task<(bool IsValid, string Message)> ValidateDepartmentAndPositionAsync(
            Guid? departmentId,
            Guid? positionId)
        {
            if (!departmentId.HasValue || departmentId.Value == Guid.Empty)
            {
                return (false, "Department wajib dipilih.");
            }

            if (!positionId.HasValue || positionId.Value == Guid.Empty)
            {
                return (false, "Position wajib dipilih.");
            }

            var departmentExists = await _dbContext.MstDepartments
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == departmentId.Value &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!departmentExists)
            {
                return (false, "Department tidak valid atau tidak aktif.");
            }

            var positionExists = await _dbContext.MstPositions
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == positionId.Value &&
                    x.DepartmentId == departmentId.Value &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!positionExists)
            {
                return (false, "Position tidak valid atau tidak sesuai dengan department.");
            }

            return (true, string.Empty);
        }

        private async Task<ApplicationUser?> GetUserWithRelationsAsync(Guid id)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Include(x => x.Employee)
                .Include(x => x.Doctor)
                .Include(x => x.ExternalUser)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        private async Task<UserManagementResponse> BuildUserResponseAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            return new UserManagementResponse
            {
                Id = user.Id,
                UserCode = user.UserCode,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                UserType = user.UserType,
                UserTypeName = user.UserType.ToString(),
                BirthDate = user.BirthDate,
                IdentityNumber = user.IdentityNumber,
                PhoneNumber = user.PhoneNumber,
                DepartmentId = user.DepartmentId,
                DepartmentCode = user.Department?.DepartmentCode,
                DepartmentName = user.Department?.DepartmentName,
                PositionId = user.PositionId,
                PositionCode = user.Position?.PositionCode,
                PositionName = user.Position?.PositionName,
                EmployeeId = user.EmployeeId,
                DoctorId = user.DoctorId,
                ExternalUserId = user.ExternalUserId,
                ProfileCode = GetProfileCode(user),
                ProfileName = GetProfileName(user),
                ProfileType = GetProfileType(user),
                ProfilePhotoPath = user.ProfilePhotoPath,
                Roles = roles.ToList(),
                IsActive = user.IsActive,
                MustChangePassword = user.MustChangePassword,
                LastLoginAt = user.LastLoginAt,
                AccessValidUntil = user.AccessValidUntil,
                CreateDateTime = user.CreateDateTime
            };
        }

        private static string? GetProfileCode(ApplicationUser user)
        {
            if (user.Employee != null)
            {
                return user.Employee.EmployeeCode;
            }

            if (user.Doctor != null)
            {
                return user.Doctor.DoctorCode;
            }

            if (user.ExternalUser != null)
            {
                return user.ExternalUser.ExternalCode;
            }

            return null;
        }

        private static string? GetProfileName(ApplicationUser user)
        {
            if (user.Employee != null)
            {
                return user.Employee.FullName;
            }

            if (user.Doctor != null)
            {
                return user.Doctor.FullName;
            }

            if (user.ExternalUser != null)
            {
                return user.ExternalUser.FullName;
            }

            return null;
        }

        private static string? GetProfileType(ApplicationUser user)
        {
            if (user.Employee != null)
            {
                return "Employee";
            }

            if (user.Doctor != null)
            {
                return "Doctor";
            }

            if (user.ExternalUser != null)
            {
                return "ExternalUser";
            }

            return null;
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private Guid GetCurrentUserId()
        {
            var userIdText = User.FindFirstValue("user_id");

            if (Guid.TryParse(userIdText, out var userId))
            {
                return userId;
            }

            return Guid.Empty;
        }

        private static DateTime GetSystemNow()
        {
            try
            {
                var timezone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timezone);
            }
            catch
            {
                return DateTime.UtcNow.AddHours(7);
            }
        }

        private string GetModelStateErrorMessage()
        {
            return string.Join(", ",
                ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => string.IsNullOrWhiteSpace(x.ErrorMessage)
                        ? "Input tidak valid."
                        : x.ErrorMessage)
                    .ToList()
            );
        }

        private static EnumOptionResponse ToEnumOption<TEnum>(
            TEnum value,
            string? displayName = null)
            where TEnum : struct, System.Enum
        {
            return new EnumOptionResponse
            {
                Value = Convert.ToInt32(value),
                Name = value.ToString(),
                DisplayName = displayName ?? value.ToString()
            };
        }

        private static List<EnumOptionResponse> BuildEnumOptions<TEnum>()
            where TEnum : struct, System.Enum
        {
            return System.Enum.GetValues<TEnum>()
                .Select(x => ToEnumOption(x))
                .ToList();
        }

        private class UserProfileProvisionResult
        {
            public bool IsValid { get; set; }

            public string Message { get; set; } = string.Empty;

            public Guid DepartmentId { get; set; }

            public Guid PositionId { get; set; }

            public Guid? EmployeeId { get; set; }

            public Guid? DoctorId { get; set; }

            public Guid? ExternalUserId { get; set; }

            public static UserProfileProvisionResult Ok(
                Guid departmentId,
                Guid positionId,
                Guid? employeeId,
                Guid? doctorId,
                Guid? externalUserId)
            {
                return new UserProfileProvisionResult
                {
                    IsValid = true,
                    DepartmentId = departmentId,
                    PositionId = positionId,
                    EmployeeId = employeeId,
                    DoctorId = doctorId,
                    ExternalUserId = externalUserId
                };
            }

            public static UserProfileProvisionResult Fail(string message)
            {
                return new UserProfileProvisionResult
                {
                    IsValid = false,
                    Message = message
                };
            }
        }
    }
}