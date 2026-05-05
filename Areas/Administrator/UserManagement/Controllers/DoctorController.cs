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
    [Route("api/v1/administrator/user-management/doctors")]
    [Tags("User Management / Doctor")]
    [AccessController(
        moduleCode: "USER_MANAGEMENT",
        moduleName: "User Management",
        displayName: "Doctor Management",
        AreaName = "Administrator",
        ControllerName = "Doctor",
        Description = "Pengelolaan data dokter",
        SortOrder = 4
    )]
    public class DoctorController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public DoctorController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<DoctorResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "Index",
            displayName: "Lihat Data",
            Description = "Melihat daftar dokter",
            SortOrder = 1
        )]
        [AccessPermission("Doctor", "Index")]
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

            var query = _dbContext.MstDoctors
                .AsNoTracking()
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.DoctorCode.ToLower().Contains(keyword) ||
                    x.FullName.ToLower().Contains(keyword) ||
                    (x.IdentityNumber != null && x.IdentityNumber.ToLower().Contains(keyword)) ||
                    (x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(keyword)) ||
                    (x.WhatsAppNumber != null && x.WhatsAppNumber.ToLower().Contains(keyword)) ||
                    (x.Email != null && x.Email.ToLower().Contains(keyword)) ||
                    (x.SpecialistName != null && x.SpecialistName.ToLower().Contains(keyword)) ||
                    (x.SubSpecialistName != null && x.SubSpecialistName.ToLower().Contains(keyword)) ||
                    (x.MedicalStaffGroup != null && x.MedicalStaffGroup.ToLower().Contains(keyword)) ||
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
                .Select(x => BuildDoctorResponse(x))
                .ToListAsync();

            var result = new PagedResult<DoctorResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            await _loggerService.InfoAsync(
                "UserManagement",
                "Doctor.GetAll",
                "Mengambil daftar dokter.",
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

            return Ok(ApiResponse<PagedResult<DoctorResponse>>.Ok(
                result,
                "Daftar dokter berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<DoctorOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "Options",
            displayName: "Pilihan Data",
            Description = "Mengambil data pilihan dokter",
            SortOrder = 2
        )]
        [AccessPermission("Doctor", "Options")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? positionId,
            [FromQuery] string? search)
        {
            var query = _dbContext.MstDoctors
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
                    x.DoctorCode.ToLower().Contains(keyword) ||
                    x.FullName.ToLower().Contains(keyword) ||
                    (x.SpecialistName != null && x.SpecialistName.ToLower().Contains(keyword)) ||
                    (x.SubSpecialistName != null && x.SubSpecialistName.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderBy(x => x.FullName)
                .Take(50)
                .Select(x => new DoctorOptionResponse
                {
                    Id = x.Id,
                    DoctorCode = x.DoctorCode,
                    FullName = x.FullName,
                    DoctorType = x.DoctorType,
                    DoctorTypeName = x.DoctorType.ToString(),
                    SpecialistName = x.SpecialistName,
                    SubSpecialistName = x.SubSpecialistName,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                    PositionId = x.PositionId,
                    PositionName = x.Position != null ? x.Position.PositionName : null
                })
                .ToListAsync();

            await _loggerService.InfoAsync(
                "UserManagement",
                "Doctor.GetOptions",
                "Mengambil pilihan dokter.",
                new
                {
                    departmentId,
                    positionId,
                    search,
                    TotalData = data.Count
                }
            );

            return Ok(ApiResponse<List<DoctorOptionResponse>>.Ok(
                data,
                "Data pilihan dokter berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DoctorResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            actionName: "Detail",
            displayName: "Detail Data",
            Description = "Melihat detail dokter",
            SortOrder = 3
        )]
        [AccessPermission("Doctor", "Detail")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _dbContext.MstDoctors
                .AsNoTracking()
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => BuildDoctorResponse(x))
                .FirstOrDefaultAsync();

            if (data == null)
            {
                await _loggerService.WarningAsync(
                    "UserManagement",
                    "Doctor.GetById",
                    "Dokter tidak ditemukan.",
                    new
                    {
                        Id = id
                    }
                );

                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Dokter tidak ditemukan."
                ));
            }

            await _loggerService.InfoAsync(
                "UserManagement",
                "Doctor.GetById",
                "Mengambil detail dokter.",
                new
                {
                    Id = id
                }
            );

            return Ok(ApiResponse<DoctorResponse>.Ok(
                data,
                "Detail dokter berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DoctorResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            actionName: "Create",
            displayName: "Tambah Data",
            Description = "Membuat dokter baru",
            SortOrder = 4
        )]
        [AccessPermission("Doctor", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateDoctorRequest request)
        {
            var validation = await ValidateDoctorRequestAsync(request, null);

            if (!validation.IsValid)
            {
                await _loggerService.WarningAsync(
                    "UserManagement",
                    "Doctor.Create",
                    validation.Message,
                    request
                );

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            var doctorCode = await GenerateDoctorCodeAsync();

            var entity = new MstDoctor
            {
                Id = Guid.NewGuid(),
                DoctorCode = doctorCode,
                FullName = request.FullName.Trim(),
                DoctorType = request.DoctorType,
                Gender = request.Gender,
                BirthPlace = Normalize(request.BirthPlace),
                BirthDate = request.BirthDate,
                IdentityType = Normalize(request.IdentityType),
                IdentityNumber = Normalize(request.IdentityNumber),
                PhoneNumber = Normalize(request.PhoneNumber),
                WhatsAppNumber = Normalize(request.WhatsAppNumber),
                Email = Normalize(request.Email),
                Address = Normalize(request.Address),
                SpecialistName = Normalize(request.SpecialistName),
                SubSpecialistName = Normalize(request.SubSpecialistName),
                MedicalStaffGroup = Normalize(request.MedicalStaffGroup),
                DepartmentId = request.DepartmentId,
                PositionId = request.PositionId,
                JoinDate = request.JoinDate,
                ContractStartDate = request.ContractStartDate,
                ContractEndDate = request.ContractEndDate,
                IsAvailableForAppointment = request.IsAvailableForAppointment,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstDoctors.Add(entity);

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                "UserManagement",
                "Doctor.Create",
                "Dokter berhasil dibuat.",
                new
                {
                    entity.Id,
                    entity.DoctorCode,
                    entity.FullName,
                    entity.DoctorType,
                    entity.SpecialistName,
                    entity.DepartmentId,
                    entity.PositionId
                }
            );

            var response = await GetDoctorResponseByIdAsync(entity.Id);

            return Ok(ApiResponse<DoctorResponse>.Ok(
                response!,
                "Dokter berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DoctorResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            actionName: "Update",
            displayName: "Ubah Data",
            Description = "Mengubah data dokter",
            SortOrder = 5
        )]
        [AccessPermission("Doctor", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDoctorRequest request)
        {
            var entity = await _dbContext.MstDoctors
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                await _loggerService.WarningAsync(
                    "UserManagement",
                    "Doctor.Update",
                    "Gagal update dokter. Dokter tidak ditemukan.",
                    new
                    {
                        Id = id
                    }
                );

                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Dokter tidak ditemukan."
                ));
            }

            var validation = await ValidateDoctorRequestAsync(request, id);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            entity.FullName = request.FullName.Trim();
            entity.DoctorType = request.DoctorType;
            entity.Gender = request.Gender;
            entity.BirthPlace = Normalize(request.BirthPlace);
            entity.BirthDate = request.BirthDate;
            entity.IdentityType = Normalize(request.IdentityType);
            entity.IdentityNumber = Normalize(request.IdentityNumber);
            entity.PhoneNumber = Normalize(request.PhoneNumber);
            entity.WhatsAppNumber = Normalize(request.WhatsAppNumber);
            entity.Email = Normalize(request.Email);
            entity.Address = Normalize(request.Address);
            entity.SpecialistName = Normalize(request.SpecialistName);
            entity.SubSpecialistName = Normalize(request.SubSpecialistName);
            entity.MedicalStaffGroup = Normalize(request.MedicalStaffGroup);
            entity.DepartmentId = request.DepartmentId;
            entity.PositionId = request.PositionId;
            entity.JoinDate = request.JoinDate;
            entity.ContractStartDate = request.ContractStartDate;
            entity.ContractEndDate = request.ContractEndDate;
            entity.IsAvailableForAppointment = request.IsAvailableForAppointment;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                "UserManagement",
                "Doctor.Update",
                "Dokter berhasil diperbarui.",
                new
                {
                    entity.Id,
                    entity.DoctorCode,
                    entity.FullName,
                    entity.DoctorType,
                    entity.IsActive
                }
            );

            var response = await GetDoctorResponseByIdAsync(entity.Id);

            return Ok(ApiResponse<DoctorResponse>.Ok(
                response!,
                "Dokter berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            actionName: "Delete",
            displayName: "Hapus Data",
            Description = "Menghapus data dokter",
            SortOrder = 6
        )]
        [AccessPermission("Doctor", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.MstDoctors
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                await _loggerService.WarningAsync(
                    "UserManagement",
                    "Doctor.Delete",
                    "Gagal hapus dokter. Dokter tidak ditemukan.",
                    new
                    {
                        Id = id
                    }
                );

                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Dokter tidak ditemukan."
                ));
            }

            var userExists = await _dbContext.Users
                .AnyAsync(x => x.DoctorId == id);

            if (userExists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Dokter tidak bisa dihapus karena sudah terhubung dengan akun user."
                ));
            }

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                "UserManagement",
                "Doctor.Delete",
                "Dokter berhasil dihapus.",
                new
                {
                    entity.Id,
                    entity.DoctorCode,
                    entity.FullName
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Dokter berhasil dihapus."
            ));
        }

        private async Task<DoctorResponse?> GetDoctorResponseByIdAsync(Guid id)
        {
            return await _dbContext.MstDoctors
                .AsNoTracking()
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => BuildDoctorResponse(x))
                .FirstOrDefaultAsync();
        }

        private static DoctorResponse BuildDoctorResponse(MstDoctor x)
        {
            return new DoctorResponse
            {
                Id = x.Id,
                DoctorCode = x.DoctorCode,
                FullName = x.FullName,
                DoctorType = x.DoctorType,
                DoctorTypeName = x.DoctorType.ToString(),
                Gender = x.Gender,
                GenderName = x.Gender.HasValue ? x.Gender.Value.ToString() : null,
                BirthPlace = x.BirthPlace,
                BirthDate = x.BirthDate,
                IdentityType = x.IdentityType,
                IdentityNumber = x.IdentityNumber,
                PhoneNumber = x.PhoneNumber,
                WhatsAppNumber = x.WhatsAppNumber,
                Email = x.Email,
                Address = x.Address,
                SpecialistName = x.SpecialistName,
                SubSpecialistName = x.SubSpecialistName,
                MedicalStaffGroup = x.MedicalStaffGroup,
                DepartmentId = x.DepartmentId,
                DepartmentCode = x.Department != null ? x.Department.DepartmentCode : null,
                DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                PositionId = x.PositionId,
                PositionCode = x.Position != null ? x.Position.PositionCode : null,
                PositionName = x.Position != null ? x.Position.PositionName : null,
                JoinDate = x.JoinDate,
                ContractStartDate = x.ContractStartDate,
                ContractEndDate = x.ContractEndDate,
                IsAvailableForAppointment = x.IsAvailableForAppointment,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private Task<(bool IsValid, string Message)> ValidateDoctorRequestAsync(
            CreateDoctorRequest request,
            Guid? excludeId)
        {
            return ValidateDoctorDataAsync(
                fullName: request.FullName,
                departmentId: request.DepartmentId,
                positionId: request.PositionId,
                email: request.Email,
                identityNumber: request.IdentityNumber,
                contractStartDate: request.ContractStartDate,
                contractEndDate: request.ContractEndDate,
                excludeId: excludeId
            );
        }

        private Task<(bool IsValid, string Message)> ValidateDoctorRequestAsync(
            UpdateDoctorRequest request,
            Guid? excludeId)
        {
            return ValidateDoctorDataAsync(
                fullName: request.FullName,
                departmentId: request.DepartmentId,
                positionId: request.PositionId,
                email: request.Email,
                identityNumber: request.IdentityNumber,
                contractStartDate: request.ContractStartDate,
                contractEndDate: request.ContractEndDate,
                excludeId: excludeId
            );
        }

        private async Task<(bool IsValid, string Message)> ValidateDoctorDataAsync(
            string fullName,
            Guid? departmentId,
            Guid? positionId,
            string? email,
            string? identityNumber,
            DateTime? contractStartDate,
            DateTime? contractEndDate,
            Guid? excludeId)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return (false, "Nama lengkap wajib diisi.");
            }

            if (contractStartDate.HasValue && contractEndDate.HasValue &&
                contractEndDate.Value < contractStartDate.Value)
            {
                return (false, "Tanggal akhir kontrak tidak boleh lebih kecil dari tanggal mulai kontrak.");
            }

            if (!departmentId.HasValue && positionId.HasValue)
            {
                return (false, "Department wajib dipilih jika position dipilih.");
            }

            if (departmentId.HasValue && !positionId.HasValue)
            {
                return (false, "Position wajib dipilih jika department dipilih.");
            }

            if (departmentId.HasValue && departmentId.Value == Guid.Empty)
            {
                return (false, "Department tidak valid.");
            }

            if (positionId.HasValue && positionId.Value == Guid.Empty)
            {
                return (false, "Position tidak valid.");
            }

            if (departmentId.HasValue && positionId.HasValue)
            {
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
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var value = email.Trim().ToLower();

                var exists = await _dbContext.MstDoctors
                    .AnyAsync(x =>
                        (!excludeId.HasValue || x.Id != excludeId.Value) &&
                        x.Email != null &&
                        x.Email.ToLower() == value &&
                        !x.IsDelete);

                if (exists)
                {
                    return (false, "Email dokter sudah digunakan.");
                }
            }

            if (!string.IsNullOrWhiteSpace(identityNumber))
            {
                var value = identityNumber.Trim();

                var exists = await _dbContext.MstDoctors
                    .AnyAsync(x =>
                        (!excludeId.HasValue || x.Id != excludeId.Value) &&
                        x.IdentityNumber == value &&
                        !x.IsDelete);

                if (exists)
                {
                    return (false, "Nomor identitas dokter sudah digunakan.");
                }
            }

            return (true, string.Empty);
        }

        private async Task<string> GenerateDoctorCodeAsync()
        {
            const string prefix = "DOC";

            var totalData = await _dbContext.MstDoctors
                .IgnoreQueryFilters()
                .CountAsync();

            var nextNumber = totalData + 1;

            while (true)
            {
                var code = $"{prefix}{nextNumber.ToString("D6")}";

                var exists = await _dbContext.MstDoctors
                    .AnyAsync(x => x.DoctorCode == code);

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