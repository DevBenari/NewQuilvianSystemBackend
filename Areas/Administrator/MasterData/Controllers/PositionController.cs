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
//    [Route("api/v1/administrator/master-data/positions")]
//    [AccessController(
//        moduleCode: "MASTER_DATA",
//        moduleName: "Master Data",
//        displayName: "Position",
//        AreaName = "Administrator",
//        ControllerName = "Position",
//        Description = "Master data position",
//        SortOrder = 2
//    )]
//    [Tags("Master Data / Position")]
//    public class PositionController : ControllerBase
//    {
//        private readonly ApplicationDbContext _dbContext;
//        private readonly LoggerService _loggerService;

//        public PositionController(
//            ApplicationDbContext dbContext,
//            LoggerService loggerService)
//        {
//            _dbContext = dbContext;
//            _loggerService = loggerService;
//        }

//        [HttpGet]
//        [ProducesResponseType(typeof(ApiResponse<PagedResult<PositionResponse>>), StatusCodes.Status200OK)]
//        [AccessAction(
//            actionName: "Index",
//            displayName: "Lihat Data",
//            Description = "Melihat daftar position",
//            SortOrder = 1
//        )]
//        [AccessPermission("Position", "Index")]
//        public async Task<IActionResult> GetAll(
//            [FromQuery] Guid? departmentId,
//            [FromQuery] string? search,
//            [FromQuery] bool? isActive,
//            [FromQuery] int pageNumber = 1,
//            [FromQuery] int pageSize = 10)
//        {
//            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
//            pageSize = pageSize <= 0 ? 10 : pageSize;
//            pageSize = pageSize > 100 ? 100 : pageSize;

//            var query = _dbContext.MstPositions
//                .AsNoTracking()
//                .Include(x => x.Department)
//                .Where(x => !x.IsDelete);

//            if (departmentId.HasValue)
//            {
//                query = query.Where(x => x.DepartmentId == departmentId.Value);
//            }

//            if (!string.IsNullOrWhiteSpace(search))
//            {
//                var keyword = search.Trim().ToLower();

//                query = query.Where(x =>
//                    x.PositionCode.ToLower().Contains(keyword) ||
//                    x.PositionName.ToLower().Contains(keyword) ||
//                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
//            }

//            if (isActive.HasValue)
//            {
//                query = query.Where(x => x.IsActive == isActive.Value);
//            }

//            var totalData = await query.CountAsync();

//            var items = await query
//                .OrderBy(x => x.Department!.DepartmentName)
//                .ThenBy(x => x.PositionName)
//                .Skip((pageNumber - 1) * pageSize)
//                .Take(pageSize)
//                .Select(x => new PositionResponse
//                {
//                    Id = x.Id,
//                    DepartmentId = x.DepartmentId,
//                    DepartmentName = x.Department != null ? x.Department.DepartmentName : string.Empty,
//                    PositionCode = x.PositionCode,
//                    PositionName = x.PositionName,
//                    Description = x.Description,
//                    IsActive = x.IsActive,
//                    CreateDateTime = x.CreateDateTime
//                })
//                .ToListAsync();

//            var result = new PagedResult<PositionResponse>
//            {
//                PageNumber = pageNumber,
//                PageSize = pageSize,
//                TotalData = totalData,
//                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
//                Items = items
//            };

//            await _loggerService.InfoAsync(
//                "MasterData",
//                "Position.GetAll",
//                "Mengambil data position.",
//                new
//                {
//                    departmentId,
//                    search,
//                    isActive,
//                    pageNumber,
//                    pageSize,
//                    totalData
//                }
//            );

//            return Ok(ApiResponse<PagedResult<PositionResponse>>.Ok(
//                result,
//                "Data position berhasil diambil."
//            ));
//        }

//        [HttpGet("options")]
//        [ProducesResponseType(typeof(ApiResponse<List<PositionOptionResponse>>), StatusCodes.Status200OK)]
//        [AccessAction(
//            actionName: "Options",
//            displayName: "Pilihan Data",
//            Description = "Mengambil pilihan position berdasarkan department",
//            SortOrder = 2
//        )]
//        [AccessPermission("Position", "Options")]
//        public async Task<IActionResult> GetOptions([FromQuery] Guid departmentId)
//        {
//            if (departmentId == Guid.Empty)
//            {
//                await _loggerService.WarningAsync(
//                    "MasterData",
//                    "Position.GetOptions",
//                    "Gagal mengambil pilihan position. Department kosong."
//                );

//                return BadRequest(ApiResponse<object>.Fail(
//                    StatusCodes.Status400BadRequest,
//                    "Department wajib dipilih."
//                ));
//            }

//            var data = await _dbContext.MstPositions
//                .AsNoTracking()
//                .Where(x =>
//                    x.DepartmentId == departmentId &&
//                    x.IsActive &&
//                    !x.IsDelete)
//                .OrderBy(x => x.PositionName)
//                .Select(x => new PositionOptionResponse
//                {
//                    Id = x.Id,
//                    DepartmentId = x.DepartmentId,
//                    PositionCode = x.PositionCode,
//                    PositionName = x.PositionName
//                })
//                .ToListAsync();

//            await _loggerService.InfoAsync(
//                "MasterData",
//                "Position.GetOptions",
//                "Mengambil data pilihan position.",
//                new
//                {
//                    departmentId,
//                    TotalData = data.Count
//                }
//            );

//            return Ok(ApiResponse<List<PositionOptionResponse>>.Ok(
//                data,
//                "Data pilihan position berhasil diambil."
//            ));
//        }

//        [HttpGet("{id:guid}")]
//        [ProducesResponseType(typeof(ApiResponse<PositionResponse>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [AccessAction(
//            actionName: "Detail",
//            displayName: "Detail Data",
//            Description = "Melihat detail position",
//            SortOrder = 3
//        )]
//        [AccessPermission("Position", "Detail")]
//        public async Task<IActionResult> GetById(Guid id)
//        {
//            var data = await _dbContext.MstPositions
//                .AsNoTracking()
//                .Include(x => x.Department)
//                .Where(x => x.Id == id && !x.IsDelete)
//                .Select(x => new PositionResponse
//                {
//                    Id = x.Id,
//                    DepartmentId = x.DepartmentId,
//                    DepartmentName = x.Department != null ? x.Department.DepartmentName : string.Empty,
//                    PositionCode = x.PositionCode,
//                    PositionName = x.PositionName,
//                    Description = x.Description,
//                    IsActive = x.IsActive,
//                    CreateDateTime = x.CreateDateTime
//                })
//                .FirstOrDefaultAsync();

//            if (data == null)
//            {
//                await _loggerService.WarningAsync(
//                    "MasterData",
//                    "Position.GetById",
//                    "Position tidak ditemukan.",
//                    new
//                    {
//                        Id = id
//                    }
//                );

//                return NotFound(ApiResponse<object>.Fail(
//                    StatusCodes.Status404NotFound,
//                    "Position tidak ditemukan."
//                ));
//            }

//            await _loggerService.InfoAsync(
//                "MasterData",
//                "Position.GetById",
//                "Mengambil detail position.",
//                new
//                {
//                    Id = id
//                }
//            );

//            return Ok(ApiResponse<PositionResponse>.Ok(
//                data,
//                "Detail position berhasil diambil."
//            ));
//        }

//        [HttpPost]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
//        [AccessAction(
//            actionName: "Create",
//            displayName: "Tambah Data",
//            Description = "Membuat position baru",
//            SortOrder = 4
//        )]
//        [AccessPermission("Position", "Create")]
//        public async Task<IActionResult> Create([FromBody] CreatePositionRequest request)
//        {
//            if (request.DepartmentId == Guid.Empty)
//            {
//                await _loggerService.WarningAsync(
//                    "MasterData",
//                    "Position.Create",
//                    "Gagal membuat position. Department kosong.",
//                    request
//                );

//                return BadRequest(ApiResponse<object>.Fail(
//                    StatusCodes.Status400BadRequest,
//                    "Department wajib dipilih."
//                ));
//            }

//            if (string.IsNullOrWhiteSpace(request.PositionName))
//            {
//                await _loggerService.WarningAsync(
//                    "MasterData",
//                    "Position.Create",
//                    "Gagal membuat position. Nama position kosong.",
//                    request
//                );

//                return BadRequest(ApiResponse<object>.Fail(
//                    StatusCodes.Status400BadRequest,
//                    "Nama position wajib diisi."
//                ));
//            }

//            var department = await _dbContext.MstDepartments
//                .AsNoTracking()
//                .FirstOrDefaultAsync(x =>
//                    x.Id == request.DepartmentId &&
//                    x.IsActive &&
//                    !x.IsDelete);

//            if (department == null)
//            {
//                await _loggerService.WarningAsync(
//                    "MasterData",
//                    "Position.Create",
//                    "Gagal membuat position. Department tidak valid.",
//                    request
//                );

//                return BadRequest(ApiResponse<object>.Fail(
//                    StatusCodes.Status400BadRequest,
//                    "Department tidak valid atau tidak aktif."
//                ));
//            }

//            var name = request.PositionName.Trim();

//            var nameExists = await _dbContext.MstPositions
//                .AnyAsync(x =>
//                    x.DepartmentId == request.DepartmentId &&
//                    x.PositionName.ToLower() == name.ToLower() &&
//                    !x.IsDelete);

//            if (nameExists)
//            {
//                await _loggerService.WarningAsync(
//                    "MasterData",
//                    "Position.Create",
//                    "Gagal membuat position. Nama position sudah digunakan pada department ini.",
//                    new
//                    {
//                        request.DepartmentId,
//                        PositionName = name
//                    }
//                );

//                return BadRequest(ApiResponse<object>.Fail(
//                    StatusCodes.Status400BadRequest,
//                    "Nama position sudah digunakan pada department ini."
//                ));
//            }

//            var code = await GeneratePositionCodeAsync();

//            var entity = new MstPosition
//            {
//                Id = Guid.NewGuid(),
//                DepartmentId = request.DepartmentId,
//                PositionCode = code,
//                PositionName = name,
//                Description = request.Description,
//                IsActive = true,
//                CreateDateTime = DateTime.UtcNow,
//                CreateBy = GetCurrentUserId(),
//                IsDelete = false,
//                IsCancel = false
//            };

//            _dbContext.MstPositions.Add(entity);

//            await _dbContext.SaveChangesAsync();

//            await _loggerService.InfoAsync(
//                "MasterData",
//                "Position.Create",
//                "Position berhasil dibuat.",
//                new
//                {
//                    entity.Id,
//                    entity.DepartmentId,
//                    DepartmentName = department.DepartmentName,
//                    entity.PositionCode,
//                    entity.PositionName,
//                    entity.IsActive
//                }
//            );

//            return Ok(ApiResponse<object>.Ok(
//                new
//                {
//                    entity.Id,
//                    entity.DepartmentId,
//                    department.DepartmentName,
//                    entity.PositionCode,
//                    entity.PositionName,
//                    entity.Description,
//                    entity.IsActive
//                },
//                "Position berhasil dibuat."
//            ));
//        }

//        [HttpPut("{id:guid}")]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
//        [AccessAction(
//            actionName: "Update",
//            displayName: "Ubah Data",
//            Description = "Mengubah position",
//            SortOrder = 5
//        )]
//        [AccessPermission("Position", "Update")]
//        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePositionRequest request)
//        {
//            var entity = await _dbContext.MstPositions
//                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

//            if (entity == null)
//            {
//                await _loggerService.WarningAsync(
//                    "MasterData",
//                    "Position.Update",
//                    "Gagal update position. Position tidak ditemukan.",
//                    new
//                    {
//                        Id = id
//                    }
//                );

//                return NotFound(ApiResponse<object>.Fail(
//                    StatusCodes.Status404NotFound,
//                    "Position tidak ditemukan."
//                ));
//            }

//            if (request.DepartmentId == Guid.Empty)
//            {
//                return BadRequest(ApiResponse<object>.Fail(
//                    StatusCodes.Status400BadRequest,
//                    "Department wajib dipilih."
//                ));
//            }

//            if (string.IsNullOrWhiteSpace(request.PositionName))
//            {
//                return BadRequest(ApiResponse<object>.Fail(
//                    StatusCodes.Status400BadRequest,
//                    "Nama position wajib diisi."
//                ));
//            }

//            var department = await _dbContext.MstDepartments
//                .AsNoTracking()
//                .FirstOrDefaultAsync(x =>
//                    x.Id == request.DepartmentId &&
//                    x.IsActive &&
//                    !x.IsDelete);

//            if (department == null)
//            {
//                return BadRequest(ApiResponse<object>.Fail(
//                    StatusCodes.Status400BadRequest,
//                    "Department tidak valid atau tidak aktif."
//                ));
//            }

//            var name = request.PositionName.Trim();

//            var nameExists = await _dbContext.MstPositions
//                .AnyAsync(x =>
//                    x.Id != id &&
//                    x.DepartmentId == request.DepartmentId &&
//                    x.PositionName.ToLower() == name.ToLower() &&
//                    !x.IsDelete);

//            if (nameExists)
//            {
//                return BadRequest(ApiResponse<object>.Fail(
//                    StatusCodes.Status400BadRequest,
//                    "Nama position sudah digunakan pada department ini."
//                ));
//            }

//            entity.DepartmentId = request.DepartmentId;
//            entity.PositionName = name;
//            entity.Description = request.Description;
//            entity.IsActive = request.IsActive;
//            entity.UpdateDateTime = DateTime.UtcNow;
//            entity.UpdateBy = GetCurrentUserId();

//            await _dbContext.SaveChangesAsync();

//            await _loggerService.InfoAsync(
//                "MasterData",
//                "Position.Update",
//                "Position berhasil diperbarui.",
//                new
//                {
//                    entity.Id,
//                    entity.DepartmentId,
//                    DepartmentName = department.DepartmentName,
//                    entity.PositionCode,
//                    entity.PositionName,
//                    entity.IsActive
//                }
//            );

//            return Ok(ApiResponse<object>.Ok(
//                null,
//                "Position berhasil diperbarui."
//            ));
//        }

//        [HttpDelete("{id:guid}")]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
//        [AccessAction(
//            actionName: "Delete",
//            displayName: "Hapus Data",
//            Description = "Menghapus position",
//            SortOrder = 6
//        )]
//        [AccessPermission("Position", "Delete")]
//        public async Task<IActionResult> Delete(Guid id)
//        {
//            var entity = await _dbContext.MstPositions
//                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

//            if (entity == null)
//            {
//                await _loggerService.WarningAsync(
//                    "MasterData",
//                    "Position.Delete",
//                    "Gagal hapus position. Position tidak ditemukan.",
//                    new
//                    {
//                        Id = id
//                    }
//                );

//                return NotFound(ApiResponse<object>.Fail(
//                    StatusCodes.Status404NotFound,
//                    "Position tidak ditemukan."
//                ));
//            }

//            entity.IsDelete = true;
//            entity.IsActive = false;
//            entity.DeleteDateTime = DateTime.UtcNow;
//            entity.DeleteBy = GetCurrentUserId();

//            await _dbContext.SaveChangesAsync();

//            await _loggerService.InfoAsync(
//                "MasterData",
//                "Position.Delete",
//                "Position berhasil dihapus.",
//                new
//                {
//                    entity.Id,
//                    entity.DepartmentId,
//                    entity.PositionCode,
//                    entity.PositionName
//                }
//            );

//            return Ok(ApiResponse<object>.Ok(
//                null,
//                "Position berhasil dihapus."
//            ));
//        }

//        private async Task<string> GeneratePositionCodeAsync()
//        {
//            const string prefix = "POS";

//            var totalData = await _dbContext.MstPositions
//                .IgnoreQueryFilters()
//                .CountAsync();

//            var nextNumber = totalData + 1;

//            while (true)
//            {
//                var code = $"{prefix}{nextNumber.ToString("D6")}";

//                var exists = await _dbContext.MstPositions
//                    .AnyAsync(x => x.PositionCode == code);

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