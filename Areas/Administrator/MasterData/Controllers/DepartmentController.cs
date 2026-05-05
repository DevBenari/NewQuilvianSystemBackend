//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
//using QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs;
//using QuilvianSystemBackend.Attributes;
//using QuilvianSystemBackend.Repositories;
//using QuilvianSystemBackend.Responses;
//using QuilvianSystemBackend.Services.Logging;
//using System.Security.Claims;

//namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Controllers
//{
//    [ApiController]
//    [Authorize]
//    [Route("api/v1/administrator/master-data/departments")]
//    [AccessController(
//        moduleCode: "MASTER_DATA",
//        moduleName: "Master Data",
//        displayName: "Department",
//        AreaName = "Administrator",
//        ControllerName = "Department",
//        Description = "Master data department",
//        SortOrder = 1
//    )]
//    [Tags("Master Data / Department")]
//    public class DepartmentController : ControllerBase
//    {
//        private readonly ApplicationDbContext _dbContext;
//        private readonly LoggerService _loggerService;

//        public DepartmentController(
//            ApplicationDbContext dbContext,
//            LoggerService loggerService)
//        {
//            _dbContext = dbContext;
//            _loggerService = loggerService;
//        }

//        [HttpGet]
//        [ProducesResponseType(typeof(ApiResponse<PagedResult<DepartmentResponse>>), StatusCodes.Status200OK)]
//        [AccessAction(
//            actionName: "Index",
//            displayName: "Lihat Data",
//            Description = "Melihat daftar departemen",
//            SortOrder = 1
//        )]
//        [AccessPermission("Department", "Index")]
//        public async Task<IActionResult> GetAll(
//            [FromQuery] string? search,
//            [FromQuery] bool? isActive,
//            [FromQuery] int pageNumber = 1,
//            [FromQuery] int pageSize = 10)
//        {
//            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
//            pageSize = pageSize <= 0 ? 10 : pageSize;
//            pageSize = pageSize > 100 ? 100 : pageSize;

//            var query = _dbContext.MstDepartments
//                .AsNoTracking()
//                .Where(x => !x.IsDelete);

//            if (!string.IsNullOrWhiteSpace(search))
//            {
//                var keyword = search.Trim().ToLower();

//                query = query.Where(x =>
//                    x.DepartmentCode.ToLower().Contains(keyword) ||
//                    x.DepartmentName.ToLower().Contains(keyword) ||
//                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
//            }

//            if (isActive.HasValue)
//            {
//                query = query.Where(x => x.IsActive == isActive.Value);
//            }

//            var totalData = await query.CountAsync();

//            var items = await query
//                .OrderBy(x => x.DepartmentName)
//                .Skip((pageNumber - 1) * pageSize)
//                .Take(pageSize)
//                .Select(x => new DepartmentResponse
//                {
//                    Id = x.Id,
//                    DepartmentCode = x.DepartmentCode,
//                    DepartmentName = x.DepartmentName,
//                    Description = x.Description,
//                    IsActive = x.IsActive,
//                    CreateDateTime = x.CreateDateTime
//                })
//                .ToListAsync();

//            var result = new PagedResult<DepartmentResponse>
//            {
//                PageNumber = pageNumber,
//                PageSize = pageSize,
//                TotalData = totalData,
//                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
//                Items = items
//            };

//            await _loggerService.InfoAsync(
//                "MasterData",
//                "Department.GetAll",
//                "Mengambil data department.",
//                new
//                {
//                    search,
//                    isActive,
//                    pageNumber,
//                    pageSize,
//                    totalData
//                }
//            );

//            return Ok(ApiResponse<PagedResult<DepartmentResponse>>.Ok(
//                result,
//                "Data department berhasil diambil."
//            ));
//        }

//        [HttpGet("options")]
//        [ProducesResponseType(typeof(ApiResponse<List<DepartmentOptionResponse>>), StatusCodes.Status200OK)]
//        [AccessAction(
//            actionName: "Options",
//            displayName: "Pilihan Data",
//            Description = "Mengambil pilihan department untuk dropdown",
//            SortOrder = 2
//        )]
//        [AccessPermission("Department", "Options")]
//        public async Task<IActionResult> GetOptions()
//        {
//            var data = await _dbContext.MstDepartments
//                .AsNoTracking()
//                .Where(x => x.IsActive && !x.IsDelete)
//                .OrderBy(x => x.DepartmentName)
//                .Select(x => new DepartmentOptionResponse
//                {
//                    Id = x.Id,
//                    DepartmentCode = x.DepartmentCode,
//                    DepartmentName = x.DepartmentName
//                })
//                .ToListAsync();

//            await _loggerService.InfoAsync(
//                "MasterData",
//                "Department.GetOptions",
//                "Mengambil data pilihan department.",
//                new
//                {
//                    TotalData = data.Count
//                }
//            );

//            return Ok(ApiResponse<List<DepartmentOptionResponse>>.Ok(
//                data,
//                "Data pilihan department berhasil diambil."
//            ));
//        }

//        [HttpGet("{id:guid}")]
//        [ProducesResponseType(typeof(ApiResponse<DepartmentResponse>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [AccessAction(
//            actionName: "Detail",
//            displayName: "Detail Data",
//            Description = "Melihat detail department",
//            SortOrder = 3
//        )]
//        [AccessPermission("Department", "Detail")]
//        public async Task<IActionResult> GetById(Guid id)
//        {
//            var data = await _dbContext.MstDepartments
//                .AsNoTracking()
//                .Where(x => x.Id == id && !x.IsDelete)
//                .Select(x => new DepartmentResponse
//                {
//                    Id = x.Id,
//                    DepartmentCode = x.DepartmentCode,
//                    DepartmentName = x.DepartmentName,
//                    Description = x.Description,
//                    IsActive = x.IsActive,
//                    CreateDateTime = x.CreateDateTime
//                })
//                .FirstOrDefaultAsync();

//            if (data == null)
//            {
//                await _loggerService.WarningAsync(
//                    "MasterData",
//                    "Department.GetById",
//                    "Department tidak ditemukan.",
//                    new
//                    {
//                        Id = id
//                    }
//                );

//                return NotFound(ApiResponse<object>.Fail(
//                    StatusCodes.Status404NotFound,
//                    "Department tidak ditemukan."
//                ));
//            }

//            await _loggerService.InfoAsync(
//                "MasterData",
//                "Department.GetById",
//                "Mengambil detail department.",
//                new
//                {
//                    Id = id
//                }
//            );

//            return Ok(ApiResponse<DepartmentResponse>.Ok(
//                data,
//                "Detail department berhasil diambil."
//            ));
//        }

//        [HttpPost]
//        [ProducesResponseType(typeof(ApiResponse<DepartmentResponse>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
//        [AccessAction(
//            actionName: "Create",
//            displayName: "Tambah Data",
//            Description = "Membuat department baru",
//            SortOrder = 4
//        )]
//        [AccessPermission("Department", "Create")]
//        public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request)
//        {
//            if (string.IsNullOrWhiteSpace(request.DepartmentName))
//            {
//                await _loggerService.WarningAsync(
//                    "MasterData",
//                    "Department.Create",
//                    "Gagal membuat department. Nama department kosong.",
//                    request
//                );

//                return BadRequest(ApiResponse<object>.Fail(
//                    StatusCodes.Status400BadRequest,
//                    "Nama department wajib diisi."
//                ));
//            }

//            var name = request.DepartmentName.Trim();

//            var nameExists = await _dbContext.MstDepartments
//                .AnyAsync(x =>
//                    x.DepartmentName.ToLower() == name.ToLower() &&
//                    !x.IsDelete);

//            if (nameExists)
//            {
//                await _loggerService.WarningAsync(
//                    "MasterData",
//                    "Department.Create",
//                    "Gagal membuat department. Nama department sudah digunakan.",
//                    new
//                    {
//                        DepartmentName = name
//                    }
//                );

//                return BadRequest(ApiResponse<object>.Fail(
//                    StatusCodes.Status400BadRequest,
//                    "Nama department sudah digunakan."
//                ));
//            }

//            var code = await GenerateDepartmentCodeAsync();

//            var entity = new MstDepartment
//            {
//                Id = Guid.NewGuid(),
//                DepartmentCode = code,
//                DepartmentName = name,
//                Description = request.Description,
//                IsActive = true,
//                CreateDateTime = DateTime.UtcNow,
//                CreateBy = GetCurrentUserId(),
//                IsDelete = false,
//                IsCancel = false
//            };

//            _dbContext.MstDepartments.Add(entity);

//            await _dbContext.SaveChangesAsync();

//            var response = new DepartmentResponse
//            {
//                Id = entity.Id,
//                DepartmentCode = entity.DepartmentCode,
//                DepartmentName = entity.DepartmentName,
//                Description = entity.Description,
//                IsActive = entity.IsActive,
//                CreateDateTime = entity.CreateDateTime
//            };

//            await _loggerService.InfoAsync(
//                "MasterData",
//                "Department.Create",
//                "Department berhasil dibuat.",
//                new
//                {
//                    entity.Id,
//                    entity.DepartmentCode,
//                    entity.DepartmentName,
//                    entity.IsActive
//                }
//            );

//            return Ok(ApiResponse<DepartmentResponse>.Ok(
//                response,
//                "Department berhasil dibuat."
//            ));
//        }

//        [HttpPut("{id:guid}")]
//        [ProducesResponseType(typeof(ApiResponse<DepartmentResponse>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [AccessAction(
//            actionName: "Update",
//            displayName: "Ubah Data",
//            Description = "Mengubah department",
//            SortOrder = 5
//        )]
//        [AccessPermission("Department", "Update")]
//        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentRequest request)
//        {
//            var entity = await _dbContext.MstDepartments
//                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

//            if (entity == null)
//            {
//                await _loggerService.WarningAsync(
//                    "MasterData",
//                    "Department.Update",
//                    "Gagal update department. Department tidak ditemukan.",
//                    new
//                    {
//                        Id = id
//                    }
//                );

//                return NotFound(ApiResponse<object>.Fail(
//                    StatusCodes.Status404NotFound,
//                    "Department tidak ditemukan."
//                ));
//            }

//            if (string.IsNullOrWhiteSpace(request.DepartmentName))
//            {
//                return BadRequest(ApiResponse<object>.Fail(
//                    StatusCodes.Status400BadRequest,
//                    "Nama department wajib diisi."
//                ));
//            }

//            var name = request.DepartmentName.Trim();

//            var nameExists = await _dbContext.MstDepartments
//                .AnyAsync(x =>
//                    x.Id != id &&
//                    x.DepartmentName.ToLower() == name.ToLower() &&
//                    !x.IsDelete);

//            if (nameExists)
//            {
//                await _loggerService.WarningAsync(
//                    "MasterData",
//                    "Department.Update",
//                    "Gagal update department. Nama department sudah digunakan.",
//                    new
//                    {
//                        Id = id,
//                        DepartmentName = name
//                    }
//                );

//                return BadRequest(ApiResponse<object>.Fail(
//                    StatusCodes.Status400BadRequest,
//                    "Nama department sudah digunakan."
//                ));
//            }

//            entity.DepartmentName = name;
//            entity.Description = request.Description;
//            entity.IsActive = request.IsActive;
//            entity.UpdateDateTime = DateTime.UtcNow;
//            entity.UpdateBy = GetCurrentUserId();

//            await _dbContext.SaveChangesAsync();

//            var response = new DepartmentResponse
//            {
//                Id = entity.Id,
//                DepartmentCode = entity.DepartmentCode,
//                DepartmentName = entity.DepartmentName,
//                Description = entity.Description,
//                IsActive = entity.IsActive,
//                CreateDateTime = entity.CreateDateTime
//            };

//            await _loggerService.InfoAsync(
//                "MasterData",
//                "Department.Update",
//                "Department berhasil diperbarui.",
//                new
//                {
//                    entity.Id,
//                    entity.DepartmentCode,
//                    entity.DepartmentName,
//                    entity.IsActive
//                }
//            );

//            return Ok(ApiResponse<DepartmentResponse>.Ok(
//                response,
//                "Department berhasil diperbarui."
//            ));
//        }

//        [HttpDelete("{id:guid}")]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [AccessAction(
//            actionName: "Delete",
//            displayName: "Hapus Data",
//            Description = "Menghapus department",
//            SortOrder = 6
//        )]
//        [AccessPermission("Department", "Delete")]
//        public async Task<IActionResult> Delete(Guid id)
//        {
//            var entity = await _dbContext.MstDepartments
//                .Include(x => x.Positions)
//                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

//            if (entity == null)
//            {
//                await _loggerService.WarningAsync(
//                    "MasterData",
//                    "Department.Delete",
//                    "Gagal hapus department. Department tidak ditemukan.",
//                    new
//                    {
//                        Id = id
//                    }
//                );

//                return NotFound(ApiResponse<object>.Fail(
//                    StatusCodes.Status404NotFound,
//                    "Department tidak ditemukan."
//                ));
//            }

//            var hasActivePositions = entity.Positions.Any(x => !x.IsDelete);

//            if (hasActivePositions)
//            {
//                await _loggerService.WarningAsync(
//                    "MasterData",
//                    "Department.Delete",
//                    "Gagal hapus department. Masih memiliki position.",
//                    new
//                    {
//                        entity.Id,
//                        entity.DepartmentCode,
//                        entity.DepartmentName
//                    }
//                );

//                return BadRequest(ApiResponse<object>.Fail(
//                    StatusCodes.Status400BadRequest,
//                    "Department tidak bisa dihapus karena masih memiliki position."
//                ));
//            }

//            entity.IsDelete = true;
//            entity.IsActive = false;
//            entity.DeleteDateTime = DateTime.UtcNow;
//            entity.DeleteBy = GetCurrentUserId();

//            await _dbContext.SaveChangesAsync();

//            await _loggerService.InfoAsync(
//                "MasterData",
//                "Department.Delete",
//                "Department berhasil dihapus.",
//                new
//                {
//                    entity.Id,
//                    entity.DepartmentCode,
//                    entity.DepartmentName
//                }
//            );

//            return Ok(ApiResponse<object>.Ok(
//                null,
//                "Department berhasil dihapus."
//            ));
//        }

//        private async Task<string> GenerateDepartmentCodeAsync()
//        {
//            const string prefix = "DPT";

//            var totalData = await _dbContext.MstDepartments
//                .IgnoreQueryFilters()
//                .CountAsync();

//            var nextNumber = totalData + 1;

//            while (true)
//            {
//                var code = $"{prefix}{nextNumber.ToString("D6")}";

//                var exists = await _dbContext.MstDepartments
//                    .AnyAsync(x => x.DepartmentCode == code);

//                if (!exists)
//                {
//                    return code;
//                }

//                nextNumber++;
//            }
//        }

//        private Guid GetCurrentUserId()
//        {
//            var userIdText = User.FindFirstValue("user_id");

//            if (Guid.TryParse(userIdText, out var userId))
//            {
//                return userId;
//            }

//            return Guid.Empty;
//        }
//    }
//}