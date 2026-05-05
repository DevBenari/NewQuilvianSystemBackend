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
    [Route("api/v1/administrator/user-management/external-users")]
    [Tags("User Management / External User")]
    [AccessController(
        moduleCode: "USER_MANAGEMENT",
        moduleName: "User Management",
        displayName: "External User Management",
        AreaName = "Administrator",
        ControllerName = "ExternalUser",
        Description = "Pengelolaan data user eksternal",
        SortOrder = 5
    )]
    public class ExternalUserController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public ExternalUserController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ExternalUserResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "Index",
            displayName: "Lihat Data",
            Description = "Melihat daftar user eksternal",
            SortOrder = 1
        )]
        [AccessPermission("ExternalUser", "Index")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int? externalUserType,
            [FromQuery] bool? isActive,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;
            pageSize = pageSize > 100 ? 100 : pageSize;

            var query = _dbContext.MstExternalUsers
                .AsNoTracking()
                .Where(x => !x.IsDelete);

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
                    (x.ExternalStatus != null && x.ExternalStatus.ToLower().Contains(keyword)));
            }

            if (externalUserType.HasValue)
            {
                query = query.Where(x => (int)x.ExternalUserType == externalUserType.Value);
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
                .Select(x => BuildExternalUserResponse(x))
                .ToListAsync();

            var result = new PagedResult<ExternalUserResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            await _loggerService.InfoAsync(
                "UserManagement",
                "ExternalUser.GetAll",
                "Mengambil daftar user eksternal.",
                new
                {
                    search,
                    externalUserType,
                    isActive,
                    pageNumber,
                    pageSize,
                    totalData
                }
            );

            return Ok(ApiResponse<PagedResult<ExternalUserResponse>>.Ok(
                result,
                "Daftar user eksternal berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<ExternalUserOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "Options",
            displayName: "Pilihan Data",
            Description = "Mengambil data pilihan user eksternal",
            SortOrder = 2
        )]
        [AccessPermission("ExternalUser", "Options")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] int? externalUserType,
            [FromQuery] string? search)
        {
            var query = _dbContext.MstExternalUsers
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete);

            if (externalUserType.HasValue)
            {
                query = query.Where(x => (int)x.ExternalUserType == externalUserType.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ExternalCode.ToLower().Contains(keyword) ||
                    x.FullName.ToLower().Contains(keyword) ||
                    (x.CompanyName != null && x.CompanyName.ToLower().Contains(keyword)) ||
                    (x.CompanyCode != null && x.CompanyCode.ToLower().Contains(keyword)) ||
                    (x.Email != null && x.Email.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderBy(x => x.FullName)
                .Take(50)
                .Select(x => new ExternalUserOptionResponse
                {
                    Id = x.Id,
                    ExternalCode = x.ExternalCode,
                    ExternalUserType = x.ExternalUserType,
                    ExternalUserTypeName = x.ExternalUserType.ToString(),
                    FullName = x.FullName,
                    CompanyName = x.CompanyName,
                    CompanyCode = x.CompanyCode,
                    JobTitle = x.JobTitle,
                    ContactPersonName = x.ContactPersonName,
                    Email = x.Email
                })
                .ToListAsync();

            await _loggerService.InfoAsync(
                "UserManagement",
                "ExternalUser.GetOptions",
                "Mengambil pilihan user eksternal.",
                new
                {
                    externalUserType,
                    search,
                    TotalData = data.Count
                }
            );

            return Ok(ApiResponse<List<ExternalUserOptionResponse>>.Ok(
                data,
                "Data pilihan user eksternal berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ExternalUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            actionName: "Detail",
            displayName: "Detail Data",
            Description = "Melihat detail user eksternal",
            SortOrder = 3
        )]
        [AccessPermission("ExternalUser", "Detail")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _dbContext.MstExternalUsers
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => BuildExternalUserResponse(x))
                .FirstOrDefaultAsync();

            if (data == null)
            {
                await _loggerService.WarningAsync(
                    "UserManagement",
                    "ExternalUser.GetById",
                    "User eksternal tidak ditemukan.",
                    new
                    {
                        Id = id
                    }
                );

                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "User eksternal tidak ditemukan."
                ));
            }

            await _loggerService.InfoAsync(
                "UserManagement",
                "ExternalUser.GetById",
                "Mengambil detail user eksternal.",
                new
                {
                    Id = id
                }
            );

            return Ok(ApiResponse<ExternalUserResponse>.Ok(
                data,
                "Detail user eksternal berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ExternalUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            actionName: "Create",
            displayName: "Tambah Data",
            Description = "Membuat user eksternal baru",
            SortOrder = 4
        )]
        [AccessPermission("ExternalUser", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateExternalUserRequest request)
        {
            var validation = await ValidateExternalUserRequestAsync(request, null);

            if (!validation.IsValid)
            {
                await _loggerService.WarningAsync(
                    "UserManagement",
                    "ExternalUser.Create",
                    validation.Message,
                    request
                );

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            var externalCode = await GenerateExternalCodeAsync();

            var entity = new MstExternalUser
            {
                Id = Guid.NewGuid(),
                ExternalCode = externalCode,
                ExternalUserType = request.ExternalUserType,
                FullName = request.FullName.Trim(),
                CompanyName = Normalize(request.CompanyName),
                CompanyCode = Normalize(request.CompanyCode),
                JobTitle = Normalize(request.JobTitle),
                ContactPersonName = Normalize(request.ContactPersonName),
                PhoneNumber = Normalize(request.PhoneNumber),
                WhatsAppNumber = Normalize(request.WhatsAppNumber),
                Email = Normalize(request.Email),
                Address = Normalize(request.Address),
                IdentityNumber = Normalize(request.IdentityNumber),
                TaxNumber = Normalize(request.TaxNumber),
                BusinessLicenseNumber = Normalize(request.BusinessLicenseNumber),
                ExternalStatus = Normalize(request.ExternalStatus),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstExternalUsers.Add(entity);

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                "UserManagement",
                "ExternalUser.Create",
                "User eksternal berhasil dibuat.",
                new
                {
                    entity.Id,
                    entity.ExternalCode,
                    entity.ExternalUserType,
                    entity.FullName,
                    entity.CompanyName,
                    entity.CompanyCode,
                    entity.Email
                }
            );

            var response = await GetExternalUserResponseByIdAsync(entity.Id);

            return Ok(ApiResponse<ExternalUserResponse>.Ok(
                response!,
                "User eksternal berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ExternalUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            actionName: "Update",
            displayName: "Ubah Data",
            Description = "Mengubah data user eksternal",
            SortOrder = 5
        )]
        [AccessPermission("ExternalUser", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExternalUserRequest request)
        {
            var entity = await _dbContext.MstExternalUsers
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                await _loggerService.WarningAsync(
                    "UserManagement",
                    "ExternalUser.Update",
                    "Gagal update user eksternal. User eksternal tidak ditemukan.",
                    new
                    {
                        Id = id
                    }
                );

                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "User eksternal tidak ditemukan."
                ));
            }

            var validation = await ValidateExternalUserRequestAsync(request, id);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            entity.ExternalUserType = request.ExternalUserType;
            entity.FullName = request.FullName.Trim();
            entity.CompanyName = Normalize(request.CompanyName);
            entity.CompanyCode = Normalize(request.CompanyCode);
            entity.JobTitle = Normalize(request.JobTitle);
            entity.ContactPersonName = Normalize(request.ContactPersonName);
            entity.PhoneNumber = Normalize(request.PhoneNumber);
            entity.WhatsAppNumber = Normalize(request.WhatsAppNumber);
            entity.Email = Normalize(request.Email);
            entity.Address = Normalize(request.Address);
            entity.IdentityNumber = Normalize(request.IdentityNumber);
            entity.TaxNumber = Normalize(request.TaxNumber);
            entity.BusinessLicenseNumber = Normalize(request.BusinessLicenseNumber);
            entity.ExternalStatus = Normalize(request.ExternalStatus);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                "UserManagement",
                "ExternalUser.Update",
                "User eksternal berhasil diperbarui.",
                new
                {
                    entity.Id,
                    entity.ExternalCode,
                    entity.ExternalUserType,
                    entity.FullName,
                    entity.CompanyName,
                    entity.CompanyCode,
                    entity.Email,
                    entity.IsActive
                }
            );

            var response = await GetExternalUserResponseByIdAsync(entity.Id);

            return Ok(ApiResponse<ExternalUserResponse>.Ok(
                response!,
                "User eksternal berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            actionName: "Delete",
            displayName: "Hapus Data",
            Description = "Menghapus data user eksternal",
            SortOrder = 6
        )]
        [AccessPermission("ExternalUser", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.MstExternalUsers
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                await _loggerService.WarningAsync(
                    "UserManagement",
                    "ExternalUser.Delete",
                    "Gagal hapus user eksternal. User eksternal tidak ditemukan.",
                    new
                    {
                        Id = id
                    }
                );

                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "User eksternal tidak ditemukan."
                ));
            }

            var userExists = await _dbContext.Users
                .AnyAsync(x => x.ExternalUserId == id);

            if (userExists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "User eksternal tidak bisa dihapus karena sudah terhubung dengan akun user."
                ));
            }

            var hasContracts = await _dbContext.ExtUserContracts
                .AnyAsync(x => x.ExternalUserId == id && !x.IsDelete);

            if (hasContracts)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "User eksternal tidak bisa dihapus karena masih memiliki data kontrak."
                ));
            }

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                "UserManagement",
                "ExternalUser.Delete",
                "User eksternal berhasil dihapus.",
                new
                {
                    entity.Id,
                    entity.ExternalCode,
                    entity.FullName,
                    entity.CompanyName
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "User eksternal berhasil dihapus."
            ));
        }

        private async Task<ExternalUserResponse?> GetExternalUserResponseByIdAsync(Guid id)
        {
            return await _dbContext.MstExternalUsers
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => BuildExternalUserResponse(x))
                .FirstOrDefaultAsync();
        }

        private static ExternalUserResponse BuildExternalUserResponse(MstExternalUser x)
        {
            return new ExternalUserResponse
            {
                Id = x.Id,
                ExternalCode = x.ExternalCode,
                ExternalUserType = x.ExternalUserType,
                ExternalUserTypeName = x.ExternalUserType.ToString(),
                FullName = x.FullName,
                CompanyName = x.CompanyName,
                CompanyCode = x.CompanyCode,
                JobTitle = x.JobTitle,
                ContactPersonName = x.ContactPersonName,
                PhoneNumber = x.PhoneNumber,
                WhatsAppNumber = x.WhatsAppNumber,
                Email = x.Email,
                Address = x.Address,
                IdentityNumber = x.IdentityNumber,
                TaxNumber = x.TaxNumber,
                BusinessLicenseNumber = x.BusinessLicenseNumber,
                ExternalStatus = x.ExternalStatus,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private Task<(bool IsValid, string Message)> ValidateExternalUserRequestAsync(
            CreateExternalUserRequest request,
            Guid? excludeId)
        {
            return ValidateExternalUserDataAsync(
                fullName: request.FullName,
                companyCode: request.CompanyCode,
                email: request.Email,
                identityNumber: request.IdentityNumber,
                taxNumber: request.TaxNumber,
                businessLicenseNumber: request.BusinessLicenseNumber,
                excludeId: excludeId
            );
        }

        private Task<(bool IsValid, string Message)> ValidateExternalUserRequestAsync(
            UpdateExternalUserRequest request,
            Guid? excludeId)
        {
            return ValidateExternalUserDataAsync(
                fullName: request.FullName,
                companyCode: request.CompanyCode,
                email: request.Email,
                identityNumber: request.IdentityNumber,
                taxNumber: request.TaxNumber,
                businessLicenseNumber: request.BusinessLicenseNumber,
                excludeId: excludeId
            );
        }

        private async Task<(bool IsValid, string Message)> ValidateExternalUserDataAsync(
            string fullName,
            string? companyCode,
            string? email,
            string? identityNumber,
            string? taxNumber,
            string? businessLicenseNumber,
            Guid? excludeId)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return (false, "Nama lengkap wajib diisi.");
            }

            if (!string.IsNullOrWhiteSpace(companyCode))
            {
                var value = companyCode.Trim();

                var exists = await _dbContext.MstExternalUsers
                    .AnyAsync(x =>
                        (!excludeId.HasValue || x.Id != excludeId.Value) &&
                        x.CompanyCode == value &&
                        !x.IsDelete);

                if (exists)
                {
                    return (false, "Kode perusahaan sudah digunakan.");
                }
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var value = email.Trim().ToLower();

                var exists = await _dbContext.MstExternalUsers
                    .AnyAsync(x =>
                        (!excludeId.HasValue || x.Id != excludeId.Value) &&
                        x.Email != null &&
                        x.Email.ToLower() == value &&
                        !x.IsDelete);

                if (exists)
                {
                    return (false, "Email user eksternal sudah digunakan.");
                }
            }

            if (!string.IsNullOrWhiteSpace(identityNumber))
            {
                var value = identityNumber.Trim();

                var exists = await _dbContext.MstExternalUsers
                    .AnyAsync(x =>
                        (!excludeId.HasValue || x.Id != excludeId.Value) &&
                        x.IdentityNumber == value &&
                        !x.IsDelete);

                if (exists)
                {
                    return (false, "Nomor identitas sudah digunakan.");
                }
            }

            if (!string.IsNullOrWhiteSpace(taxNumber))
            {
                var value = taxNumber.Trim();

                var exists = await _dbContext.MstExternalUsers
                    .AnyAsync(x =>
                        (!excludeId.HasValue || x.Id != excludeId.Value) &&
                        x.TaxNumber == value &&
                        !x.IsDelete);

                if (exists)
                {
                    return (false, "Nomor pajak sudah digunakan.");
                }
            }

            if (!string.IsNullOrWhiteSpace(businessLicenseNumber))
            {
                var value = businessLicenseNumber.Trim();

                var exists = await _dbContext.MstExternalUsers
                    .AnyAsync(x =>
                        (!excludeId.HasValue || x.Id != excludeId.Value) &&
                        x.BusinessLicenseNumber == value &&
                        !x.IsDelete);

                if (exists)
                {
                    return (false, "Nomor izin usaha sudah digunakan.");
                }
            }

            return (true, string.Empty);
        }

        private async Task<string> GenerateExternalCodeAsync()
        {
            const string prefix = "EXT";

            var totalData = await _dbContext.MstExternalUsers
                .IgnoreQueryFilters()
                .CountAsync();

            var nextNumber = totalData + 1;

            while (true)
            {
                var code = $"{prefix}{nextNumber.ToString("D6")}";

                var exists = await _dbContext.MstExternalUsers
                    .AnyAsync(x => x.ExternalCode == code);

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