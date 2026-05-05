using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.UserManagement.DTOs;
using QuilvianSystemBackend.Areas.Administrator.UserManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Administrator.UserManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/administrator/user-management/employees")]
    [Tags("User Management / Employee")]
    [AccessController(
        moduleCode: "USER_MANAGEMENT",
        moduleName: "User Management",
        displayName: "Employee Management",
        AreaName = "Administrator",
        ControllerName = "Employee",
        Description = "Pengelolaan data karyawan",
        SortOrder = 3
    )]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public EmployeeController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EmployeeResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "Index",
            displayName: "Lihat Data",
            Description = "Melihat daftar karyawan",
            SortOrder = 1
        )]
        [AccessPermission("Employee", "Index")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? positionId,
            [FromQuery] bool? isActive,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;
            pageSize = pageSize > 100 ? 100 : pageSize;

            var query = _dbContext.MstEmployees
                .AsNoTracking()
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Where(x => !x.IsDelete);

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
                    (x.IdentityNumber != null && x.IdentityNumber.ToLower().Contains(keyword)) ||
                    (x.Department != null && x.Department.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.Position != null && x.Position.PositionName.ToLower().Contains(keyword)));
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

            var items = await query
                .OrderBy(x => x.FullName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => BuildEmployeeResponse(x))
                .ToListAsync();

            var result = new PagedResult<EmployeeResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            await _loggerService.InfoAsync(
                "UserManagement",
                "Employee.GetAll",
                "Mengambil daftar karyawan.",
                new
                {
                    search,
                    departmentId,
                    positionId,
                    isActive,
                    pageNumber,
                    pageSize,
                    totalData
                }
            );

            return Ok(ApiResponse<PagedResult<EmployeeResponse>>.Ok(
                result,
                "Daftar karyawan berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<EmployeeOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "Options",
            displayName: "Pilihan Data",
            Description = "Mengambil data pilihan karyawan",
            SortOrder = 2
        )]
        [AccessPermission("Employee", "Options")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? positionId,
            [FromQuery] string? search)
        {
            var query = _dbContext.MstEmployees
                .AsNoTracking()
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Where(x => x.IsActive && !x.IsDelete);

            if (departmentId.HasValue)
            {
                query = query.Where(x => x.DepartmentId == departmentId.Value);
            }

            if (positionId.HasValue)
            {
                query = query.Where(x => x.PositionId == positionId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.EmployeeCode.ToLower().Contains(keyword) ||
                    (x.EmployeeNumber != null && x.EmployeeNumber.ToLower().Contains(keyword)) ||
                    x.FullName.ToLower().Contains(keyword));
            }

            var data = await query
                .OrderBy(x => x.FullName)
                .Take(50)
                .Select(x => new EmployeeOptionResponse
                {
                    Id = x.Id,
                    EmployeeCode = x.EmployeeCode,
                    EmployeeNumber = x.EmployeeNumber,
                    FullName = x.FullName,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : string.Empty,
                    PositionId = x.PositionId,
                    PositionName = x.Position != null ? x.Position.PositionName : string.Empty
                })
                .ToListAsync();

            await _loggerService.InfoAsync(
                "UserManagement",
                "Employee.GetOptions",
                "Mengambil pilihan karyawan.",
                new
                {
                    departmentId,
                    positionId,
                    search,
                    TotalData = data.Count
                }
            );

            return Ok(ApiResponse<List<EmployeeOptionResponse>>.Ok(
                data,
                "Data pilihan karyawan berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            actionName: "Detail",
            displayName: "Detail Data",
            Description = "Melihat detail karyawan",
            SortOrder = 3
        )]
        [AccessPermission("Employee", "Detail")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _dbContext.MstEmployees
                .AsNoTracking()
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => BuildEmployeeResponse(x))
                .FirstOrDefaultAsync();

            if (data == null)
            {
                await _loggerService.WarningAsync(
                    "UserManagement",
                    "Employee.GetById",
                    "Karyawan tidak ditemukan.",
                    new
                    {
                        Id = id
                    }
                );

                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Karyawan tidak ditemukan."
                ));
            }

            await _loggerService.InfoAsync(
                "UserManagement",
                "Employee.GetById",
                "Mengambil detail karyawan.",
                new
                {
                    Id = id
                }
            );

            return Ok(ApiResponse<EmployeeResponse>.Ok(
                data,
                "Detail karyawan berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmployeeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            actionName: "Create",
            displayName: "Tambah Data",
            Description = "Membuat karyawan baru",
            SortOrder = 4
        )]
        [AccessPermission("Employee", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request)
        {
            var validation = await ValidateEmployeeRequestAsync(request, null);

            if (!validation.IsValid)
            {
                await _loggerService.WarningAsync(
                    "UserManagement",
                    "Employee.Create",
                    validation.Message,
                    request
                );

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            var employeeCode = await GenerateEmployeeCodeAsync();

            var entity = new MstEmployee
            {
                Id = Guid.NewGuid(),
                EmployeeCode = employeeCode,
                EmployeeNumber = Normalize(request.EmployeeNumber),
                AttendanceNumber = Normalize(request.AttendanceNumber),
                FullName = request.FullName.Trim(),
                NickName = Normalize(request.NickName),
                Gender = request.Gender,
                BirthPlace = Normalize(request.BirthPlace),
                BirthDate = request.BirthDate,
                Religion = Normalize(request.Religion),
                MaritalStatus = Normalize(request.MaritalStatus),
                BloodType = Normalize(request.BloodType),
                IdentityType = Normalize(request.IdentityType),
                IdentityNumber = Normalize(request.IdentityNumber),
                PhoneNumber = Normalize(request.PhoneNumber),
                WhatsAppNumber = Normalize(request.WhatsAppNumber),
                Email = Normalize(request.Email),
                Address = Normalize(request.Address),
                Province = Normalize(request.Province),
                City = Normalize(request.City),
                District = Normalize(request.District),
                Village = Normalize(request.Village),
                PostalCode = Normalize(request.PostalCode),
                DepartmentId = request.DepartmentId,
                PositionId = request.PositionId,
                EmployeeStatus = request.EmployeeStatus,
                EmploymentType = Normalize(request.EmploymentType),
                GradeLevel = Normalize(request.GradeLevel),
                WorkLocation = Normalize(request.WorkLocation),
                JoinDate = request.JoinDate,
                ProbationEndDate = request.ProbationEndDate,
                ContractStartDate = request.ContractStartDate,
                ContractEndDate = request.ContractEndDate,
                EmergencyContactName = Normalize(request.EmergencyContactName),
                EmergencyContactRelation = Normalize(request.EmergencyContactRelation),
                EmergencyContactPhone = Normalize(request.EmergencyContactPhone),
                EmergencyContactAddress = Normalize(request.EmergencyContactAddress),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstEmployees.Add(entity);

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                "UserManagement",
                "Employee.Create",
                "Karyawan berhasil dibuat.",
                new
                {
                    entity.Id,
                    entity.EmployeeCode,
                    entity.EmployeeNumber,
                    entity.FullName,
                    entity.DepartmentId,
                    entity.PositionId
                }
            );

            var response = await GetEmployeeResponseByIdAsync(entity.Id);

            return Ok(ApiResponse<EmployeeResponse>.Ok(
                response!,
                "Karyawan berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            actionName: "Update",
            displayName: "Ubah Data",
            Description = "Mengubah data karyawan",
            SortOrder = 5
        )]
        [AccessPermission("Employee", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeRequest request)
        {
            var entity = await _dbContext.MstEmployees
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                await _loggerService.WarningAsync(
                    "UserManagement",
                    "Employee.Update",
                    "Gagal update karyawan. Karyawan tidak ditemukan.",
                    new
                    {
                        Id = id
                    }
                );

                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Karyawan tidak ditemukan."
                ));
            }

            var validation = await ValidateEmployeeRequestAsync(request, id);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            entity.EmployeeNumber = Normalize(request.EmployeeNumber);
            entity.AttendanceNumber = Normalize(request.AttendanceNumber);
            entity.FullName = request.FullName.Trim();
            entity.NickName = Normalize(request.NickName);
            entity.Gender = request.Gender;
            entity.BirthPlace = Normalize(request.BirthPlace);
            entity.BirthDate = request.BirthDate;
            entity.Religion = Normalize(request.Religion);
            entity.MaritalStatus = Normalize(request.MaritalStatus);
            entity.BloodType = Normalize(request.BloodType);
            entity.IdentityType = Normalize(request.IdentityType);
            entity.IdentityNumber = Normalize(request.IdentityNumber);
            entity.PhoneNumber = Normalize(request.PhoneNumber);
            entity.WhatsAppNumber = Normalize(request.WhatsAppNumber);
            entity.Email = Normalize(request.Email);
            entity.Address = Normalize(request.Address);
            entity.Province = Normalize(request.Province);
            entity.City = Normalize(request.City);
            entity.District = Normalize(request.District);
            entity.Village = Normalize(request.Village);
            entity.PostalCode = Normalize(request.PostalCode);
            entity.DepartmentId = request.DepartmentId;
            entity.PositionId = request.PositionId;
            entity.EmployeeStatus = request.EmployeeStatus;
            entity.EmploymentType = Normalize(request.EmploymentType);
            entity.GradeLevel = Normalize(request.GradeLevel);
            entity.WorkLocation = Normalize(request.WorkLocation);
            entity.JoinDate = request.JoinDate;
            entity.ProbationEndDate = request.ProbationEndDate;
            entity.ContractStartDate = request.ContractStartDate;
            entity.ContractEndDate = request.ContractEndDate;
            entity.ResignDate = request.ResignDate;
            entity.ResignReason = Normalize(request.ResignReason);
            entity.EmergencyContactName = Normalize(request.EmergencyContactName);
            entity.EmergencyContactRelation = Normalize(request.EmergencyContactRelation);
            entity.EmergencyContactPhone = Normalize(request.EmergencyContactPhone);
            entity.EmergencyContactAddress = Normalize(request.EmergencyContactAddress);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                "UserManagement",
                "Employee.Update",
                "Karyawan berhasil diperbarui.",
                new
                {
                    entity.Id,
                    entity.EmployeeCode,
                    entity.EmployeeNumber,
                    entity.FullName,
                    entity.IsActive
                }
            );

            var response = await GetEmployeeResponseByIdAsync(entity.Id);

            return Ok(ApiResponse<EmployeeResponse>.Ok(
                response!,
                "Karyawan berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            actionName: "Delete",
            displayName: "Hapus Data",
            Description = "Menghapus data karyawan",
            SortOrder = 6
        )]
        [AccessPermission("Employee", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.MstEmployees
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                await _loggerService.WarningAsync(
                    "UserManagement",
                    "Employee.Delete",
                    "Gagal hapus karyawan. Karyawan tidak ditemukan.",
                    new
                    {
                        Id = id
                    }
                );

                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Karyawan tidak ditemukan."
                ));
            }

            var userExists = await _dbContext.Users
                .AnyAsync(x => x.EmployeeId == id);

            if (userExists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Karyawan tidak bisa dihapus karena sudah terhubung dengan akun user."
                ));
            }

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                "UserManagement",
                "Employee.Delete",
                "Karyawan berhasil dihapus.",
                new
                {
                    entity.Id,
                    entity.EmployeeCode,
                    entity.EmployeeNumber,
                    entity.FullName
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Karyawan berhasil dihapus."
            ));
        }

        private async Task<EmployeeResponse?> GetEmployeeResponseByIdAsync(Guid id)
        {
            return await _dbContext.MstEmployees
                .AsNoTracking()
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => BuildEmployeeResponse(x))
                .FirstOrDefaultAsync();
        }

        private static EmployeeResponse BuildEmployeeResponse(MstEmployee x)
        {
            return new EmployeeResponse
            {
                Id = x.Id,
                EmployeeCode = x.EmployeeCode,
                EmployeeNumber = x.EmployeeNumber,
                AttendanceNumber = x.AttendanceNumber,
                FullName = x.FullName,
                NickName = x.NickName,
                Gender = x.Gender,
                GenderName = x.Gender.HasValue ? x.Gender.Value.ToString() : null,
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
                DepartmentId = x.DepartmentId,
                DepartmentCode = x.Department != null ? x.Department.DepartmentCode : string.Empty,
                DepartmentName = x.Department != null ? x.Department.DepartmentName : string.Empty,
                PositionId = x.PositionId,
                PositionCode = x.Position != null ? x.Position.PositionCode : string.Empty,
                PositionName = x.Position != null ? x.Position.PositionName : string.Empty,
                EmployeeStatus = x.EmployeeStatus,
                EmployeeStatusName = x.EmployeeStatus.ToString(),
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
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private Task<(bool IsValid, string Message)> ValidateEmployeeRequestAsync(
    CreateEmployeeRequest request,
    Guid? excludeId)
        {
            return ValidateEmployeeDataAsync(
                fullName: request.FullName,
                departmentId: request.DepartmentId,
                positionId: request.PositionId,
                employeeNumber: request.EmployeeNumber,
                attendanceNumber: request.AttendanceNumber,
                email: request.Email,
                identityNumber: request.IdentityNumber,
                excludeId: excludeId
            );
        }

        private Task<(bool IsValid, string Message)> ValidateEmployeeRequestAsync(
            UpdateEmployeeRequest request,
            Guid? excludeId)
        {
            return ValidateEmployeeDataAsync(
                fullName: request.FullName,
                departmentId: request.DepartmentId,
                positionId: request.PositionId,
                employeeNumber: request.EmployeeNumber,
                attendanceNumber: request.AttendanceNumber,
                email: request.Email,
                identityNumber: request.IdentityNumber,
                excludeId: excludeId
            );
        }

        private async Task<(bool IsValid, string Message)> ValidateEmployeeDataAsync(
            string fullName,
            Guid departmentId,
            Guid positionId,
            string? employeeNumber,
            string? attendanceNumber,
            string? email,
            string? identityNumber,
            Guid? excludeId)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return (false, "Nama lengkap wajib diisi.");
            }

            if (departmentId == Guid.Empty)
            {
                return (false, "Department wajib dipilih.");
            }

            if (positionId == Guid.Empty)
            {
                return (false, "Position wajib dipilih.");
            }

            var departmentExists = await _dbContext.MstDepartments
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == departmentId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!departmentExists)
            {
                return (false, "Department tidak valid atau tidak aktif.");
            }

            var positionExists = await _dbContext.MstPositions
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == positionId &&
                    x.DepartmentId == departmentId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!positionExists)
            {
                return (false, "Position tidak valid atau tidak sesuai dengan department.");
            }

            if (!string.IsNullOrWhiteSpace(employeeNumber))
            {
                var value = employeeNumber.Trim();

                var exists = await _dbContext.MstEmployees
                    .AnyAsync(x =>
                        (!excludeId.HasValue || x.Id != excludeId.Value) &&
                        x.EmployeeNumber == value &&
                        !x.IsDelete);

                if (exists)
                {
                    return (false, "Nomor karyawan sudah digunakan.");
                }
            }

            if (!string.IsNullOrWhiteSpace(attendanceNumber))
            {
                var value = attendanceNumber.Trim();

                var exists = await _dbContext.MstEmployees
                    .AnyAsync(x =>
                        (!excludeId.HasValue || x.Id != excludeId.Value) &&
                        x.AttendanceNumber == value &&
                        !x.IsDelete);

                if (exists)
                {
                    return (false, "Nomor absensi sudah digunakan.");
                }
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var value = email.Trim().ToLower();

                var exists = await _dbContext.MstEmployees
                    .AnyAsync(x =>
                        (!excludeId.HasValue || x.Id != excludeId.Value) &&
                        x.Email != null &&
                        x.Email.ToLower() == value &&
                        !x.IsDelete);

                if (exists)
                {
                    return (false, "Email karyawan sudah digunakan.");
                }
            }

            if (!string.IsNullOrWhiteSpace(identityNumber))
            {
                var value = identityNumber.Trim();

                var exists = await _dbContext.MstEmployees
                    .AnyAsync(x =>
                        (!excludeId.HasValue || x.Id != excludeId.Value) &&
                        x.IdentityNumber == value &&
                        !x.IsDelete);

                if (exists)
                {
                    return (false, "Nomor identitas sudah digunakan.");
                }
            }

            return (true, string.Empty);
        }

        private async Task<string> GenerateEmployeeCodeAsync()
        {
            const string prefix = "EMP";

            var totalData = await _dbContext.MstEmployees
                .IgnoreQueryFilters()
                .CountAsync();

            var nextNumber = totalData + 1;

            while (true)
            {
                var code = $"{prefix}{nextNumber.ToString("D6")}";

                var exists = await _dbContext.MstEmployees
                    .AnyAsync(x => x.EmployeeCode == code);

                if (!exists)
                {
                    return code;
                }

                nextNumber++;
            }
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
    }
}