using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseBedPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.BedResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/beds")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Bed",
        AreaName = "HealthServices",
        ControllerName = "Bed",
        Description = "Health service master data bed",
        SortOrder = 5
    )]
    [Tags("Health Services / Master Data / Bed")]
    public class BedController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public BedController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<BedFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Bed", Description = "Melihat data bed", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Bed", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new BedFilterMetadataResponse
            {
                DefaultFilter = new BedDefaultFilterResponse(),
                SortOptions = new List<BedSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "bedCode", Label = "Kode bed" },
                    new() { Value = "bedName", Label = "Nama bed" },
                    new() { Value = "bedNumber", Label = "Nomor bed" },
                    new() { Value = "roomName", Label = "Nama room" },
                    new() { Value = "serviceUnitName", Label = "Nama service unit" },
                    new() { Value = "patientClassName", Label = "Kelas pasien" },
                    new() { Value = "bedStatus", Label = "Status bed" },
                    new() { Value = "isReservable", Label = "Bisa reservasi" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                BedStatusOptions = BuildEnumOptions<BedStatus>()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Bed.GetFilterMetadata",
                "Mengambil metadata filter bed.",
                result
            );

            return Ok(ApiResponse<BedFilterMetadataResponse>.Ok(
                result,
                "Metadata filter bed berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<BedSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Bed", Description = "Melihat data bed", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Bed", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstBed>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new BedSummaryResponse
            {
                TotalBed = await query.CountAsync(),
                ActiveBed = await query.CountAsync(x => x.IsActive),
                InactiveBed = await query.CountAsync(x => !x.IsActive),
                AvailableBed = await query.CountAsync(x => x.BedStatus == BedStatus.Available),
                ReservableBed = await query.CountAsync(x => x.IsReservable),
                IsolationBed = await query.CountAsync(x => x.IsIsolationBed),
                IntensiveCareBed = await query.CountAsync(x => x.IsIntensiveCareBed),
                OdcBed = await query.CountAsync(x => x.IsOdcBed),
                NewbornBed = await query.CountAsync(x => x.IsForNewborn)
            };

            return Ok(ApiResponse<BedSummaryResponse>.Ok(
                result,
                "Ringkasan bed berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseBedPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Bed", Description = "Melihat data bed", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Bed", "Read")]
        public async Task<IActionResult> GetBeds(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] Guid? roomId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? patientClassId,
            [FromQuery] BedStatus? bedStatus,
            [FromQuery] bool? isForMale,
            [FromQuery] bool? isForFemale,
            [FromQuery] bool? isForNewborn,
            [FromQuery] bool? isIsolationBed,
            [FromQuery] bool? isIntensiveCareBed,
            [FromQuery] bool? isOdcBed,
            [FromQuery] bool? isReservable,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstBed>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.BedCode.ToLower().Contains(keyword) ||
                    x.BedName.ToLower().Contains(keyword) ||
                    (x.BedNumber != null && x.BedNumber.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.Room != null && x.Room.RoomCode.ToLower().Contains(keyword)) ||
                    (x.Room != null && x.Room.RoomName.ToLower().Contains(keyword)) ||
                    (x.Room != null && x.Room.ServiceUnit != null && x.Room.ServiceUnit.ServiceUnitCode.ToLower().Contains(keyword)) ||
                    (x.Room != null && x.Room.ServiceUnit != null && x.Room.ServiceUnit.ServiceUnitName.ToLower().Contains(keyword)) ||
                    (x.Room != null && x.Room.PatientClass != null && x.Room.PatientClass.PatientClassCode.ToLower().Contains(keyword)) ||
                    (x.Room != null && x.Room.PatientClass != null && x.Room.PatientClass.PatientClassName.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (roomId.HasValue && roomId.Value != Guid.Empty)
                query = query.Where(x => x.RoomId == roomId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.Room != null && x.Room.ServiceUnitId == serviceUnitId.Value);

            if (patientClassId.HasValue && patientClassId.Value != Guid.Empty)
                query = query.Where(x => x.Room != null && x.Room.PatientClassId == patientClassId.Value);

            if (bedStatus.HasValue)
                query = query.Where(x => x.BedStatus == bedStatus.Value);

            if (isForMale.HasValue)
                query = query.Where(x => x.IsForMale == isForMale.Value);

            if (isForFemale.HasValue)
                query = query.Where(x => x.IsForFemale == isForFemale.Value);

            if (isForNewborn.HasValue)
                query = query.Where(x => x.IsForNewborn == isForNewborn.Value);

            if (isIsolationBed.HasValue)
                query = query.Where(x => x.IsIsolationBed == isIsolationBed.Value);

            if (isIntensiveCareBed.HasValue)
                query = query.Where(x => x.IsIntensiveCareBed == isIntensiveCareBed.Value);

            if (isOdcBed.HasValue)
                query = query.Where(x => x.IsOdcBed == isOdcBed.Value);

            if (isReservable.HasValue)
                query = query.Where(x => x.IsReservable == isReservable.Value);

            var totalCount = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => ToResponse(x))
                .ToListAsync();

            var result = new ResponseBedPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalCount,
                TotalPage = (int)Math.Ceiling(totalCount / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseBedPagedResult>.Ok(
                result,
                "Data bed berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<BedOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Bed", Description = "Melihat data bed", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Bed", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] Guid? roomId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? patientClassId,
            [FromQuery] BedStatus? bedStatus,
            [FromQuery] bool? isReservable,
            [FromQuery] bool activeOnly = true)
        {
            var query = _dbContext.Set<MstBed>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (activeOnly)
                query = query.Where(x => x.IsActive);

            if (roomId.HasValue && roomId.Value != Guid.Empty)
                query = query.Where(x => x.RoomId == roomId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.Room != null && x.Room.ServiceUnitId == serviceUnitId.Value);

            if (patientClassId.HasValue && patientClassId.Value != Guid.Empty)
                query = query.Where(x => x.Room != null && x.Room.PatientClassId == patientClassId.Value);

            if (bedStatus.HasValue)
                query = query.Where(x => x.BedStatus == bedStatus.Value);

            if (isReservable.HasValue)
                query = query.Where(x => x.IsReservable == isReservable.Value);

            var result = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.BedName)
                .Select(x => new BedOptionResponse
                {
                    Id = x.Id,
                    RoomId = x.RoomId,
                    RoomName = x.Room != null ? x.Room.RoomName : string.Empty,
                    ServiceUnitId = x.Room != null ? x.Room.ServiceUnitId : Guid.Empty,
                    ServiceUnitName = x.Room != null && x.Room.ServiceUnit != null ? x.Room.ServiceUnit.ServiceUnitName : string.Empty,
                    PatientClassId = x.Room != null ? x.Room.PatientClassId : null,
                    PatientClassName = x.Room != null && x.Room.PatientClass != null ? x.Room.PatientClass.PatientClassName : null,
                    BedCode = x.BedCode,
                    BedName = x.BedName,
                    BedNumber = x.BedNumber,
                    BedStatus = x.BedStatus,
                    IsForMale = x.IsForMale,
                    IsForFemale = x.IsForFemale,
                    IsForNewborn = x.IsForNewborn,
                    IsReservable = x.IsReservable
                })
                .ToListAsync();

            return Ok(ApiResponse<List<BedOptionResponse>>.Ok(
                result,
                "Option bed berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BedDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Bed", Description = "Melihat detail bed", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Bed", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _dbContext.Set<MstBed>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new BedDetailResponse
                {
                    Id = x.Id,
                    RoomId = x.RoomId,
                    RoomCode = x.Room != null ? x.Room.RoomCode : string.Empty,
                    RoomName = x.Room != null ? x.Room.RoomName : string.Empty,
                    ServiceUnitId = x.Room != null ? x.Room.ServiceUnitId : Guid.Empty,
                    ServiceUnitCode = x.Room != null && x.Room.ServiceUnit != null ? x.Room.ServiceUnit.ServiceUnitCode : string.Empty,
                    ServiceUnitName = x.Room != null && x.Room.ServiceUnit != null ? x.Room.ServiceUnit.ServiceUnitName : string.Empty,
                    PatientClassId = x.Room != null ? x.Room.PatientClassId : null,
                    PatientClassCode = x.Room != null && x.Room.PatientClass != null ? x.Room.PatientClass.PatientClassCode : null,
                    PatientClassName = x.Room != null && x.Room.PatientClass != null ? x.Room.PatientClass.PatientClassName : null,
                    BedCode = x.BedCode,
                    BedName = x.BedName,
                    BedNumber = x.BedNumber,
                    BedStatus = x.BedStatus,
                    IsForMale = x.IsForMale,
                    IsForFemale = x.IsForFemale,
                    IsForNewborn = x.IsForNewborn,
                    IsIsolationBed = x.IsIsolationBed,
                    IsIntensiveCareBed = x.IsIntensiveCareBed,
                    IsOdcBed = x.IsOdcBed,
                    IsReservable = x.IsReservable,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    Description = x.Description
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Bed tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<BedDetailResponse>.Ok(
                result,
                "Detail bed berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BedCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Bed", Description = "Membuat data bed", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Bed", "Create")]
        public async Task<IActionResult> CreateBed([FromBody] CreateBedRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                roomId: request.RoomId,
                bedCode: request.BedCode,
                bedName: request.BedName,
                bedNumber: request.BedNumber
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data bed tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstBed
            {
                Id = Guid.NewGuid(),
                RoomId = request.RoomId,
                BedCode = request.BedCode.Trim().ToUpperInvariant(),
                BedName = request.BedName.Trim(),
                BedNumber = NormalizeNullableText(request.BedNumber),
                BedStatus = request.BedStatus,
                IsForMale = request.IsForMale,
                IsForFemale = request.IsForFemale,
                IsForNewborn = request.IsForNewborn,
                IsIsolationBed = request.IsIsolationBed,
                IsIntensiveCareBed = request.IsIntensiveCareBed,
                IsOdcBed = request.IsOdcBed,
                IsReservable = request.IsReservable,
                SortOrder = request.SortOrder,
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstBed>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = new BedCreateResponse
            {
                Id = entity.Id,
                RoomId = entity.RoomId,
                BedCode = entity.BedCode,
                BedName = entity.BedName,
                BedNumber = entity.BedNumber,
                BedStatus = entity.BedStatus,
                IsReservable = entity.IsReservable,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<BedCreateResponse>.Ok(
                response,
                "Bed berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Bed", Description = "Mengubah data bed", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Bed", "Update")]
        public async Task<IActionResult> UpdateBed(Guid id, [FromBody] UpdateBedRequest request)
        {
            var entity = await _dbContext.Set<MstBed>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Bed tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                roomId: request.RoomId,
                bedCode: request.BedCode,
                bedName: request.BedName,
                bedNumber: request.BedNumber
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data bed tidak valid."
                ));
            }

            entity.RoomId = request.RoomId;
            entity.BedCode = request.BedCode.Trim().ToUpperInvariant();
            entity.BedName = request.BedName.Trim();
            entity.BedNumber = NormalizeNullableText(request.BedNumber);
            entity.BedStatus = request.BedStatus;
            entity.IsForMale = request.IsForMale;
            entity.IsForFemale = request.IsForFemale;
            entity.IsForNewborn = request.IsForNewborn;
            entity.IsIsolationBed = request.IsIsolationBed;
            entity.IsIntensiveCareBed = request.IsIntensiveCareBed;
            entity.IsOdcBed = request.IsOdcBed;
            entity.IsReservable = request.IsReservable;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Bed berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/activate")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Bed", Description = "Mengaktifkan data bed", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Bed", "Update")]
        public async Task<IActionResult> ActivateBed(Guid id)
        {
            return await SetActiveStatusAsync(id, true, "Bed berhasil diaktifkan.");
        }

        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Bed", Description = "Menonaktifkan data bed", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Bed", "Update")]
        public async Task<IActionResult> DeactivateBed(Guid id)
        {
            return await SetActiveStatusAsync(id, false, "Bed berhasil dinonaktifkan.");
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Bed", Description = "Menghapus data bed", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Bed", "Delete")]
        public async Task<IActionResult> DeleteBed(Guid id)
        {
            var entity = await _dbContext.Set<MstBed>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Bed tidak ditemukan."
                ));
            }

            if (entity.BedStatus != BedStatus.Available)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Bed tidak dapat dihapus karena status bed tidak available. Nonaktifkan saja jika masih diperlukan untuk histori."
                ));
            }

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Bed berhasil dihapus."
            ));
        }

        private async Task<IActionResult> SetActiveStatusAsync(Guid id, bool isActive, string successMessage)
        {
            var entity = await _dbContext.Set<MstBed>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Bed tidak ditemukan."
                ));
            }

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                successMessage
            ));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            Guid roomId,
            string bedCode,
            string bedName,
            string? bedNumber)
        {
            if (roomId == Guid.Empty)
                return (false, "Room wajib dipilih.");

            if (string.IsNullOrWhiteSpace(bedCode))
                return (false, "Kode bed wajib diisi.");

            if (string.IsNullOrWhiteSpace(bedName))
                return (false, "Nama bed wajib diisi.");

            var roomExists = await _dbContext.Set<MstRoom>()
                .AnyAsync(x => x.Id == roomId && x.IsActive && !x.IsDelete);

            if (!roomExists)
                return (false, "Room tidak valid atau tidak aktif.");

            var normalizedCode = bedCode.Trim().ToUpperInvariant();
            var normalizedName = bedName.Trim().ToLower();

            var duplicateCode = await _dbContext.Set<MstBed>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.BedCode.ToUpper() == normalizedCode &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateCode)
                return (false, "Kode bed sudah digunakan.");

            var duplicateNameInRoom = await _dbContext.Set<MstBed>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.RoomId == roomId &&
                    x.BedName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateNameInRoom)
                return (false, "Nama bed pada room tersebut sudah digunakan.");

            if (!string.IsNullOrWhiteSpace(bedNumber))
            {
                var normalizedBedNumber = bedNumber.Trim().ToLower();

                var duplicateBedNumber = await _dbContext.Set<MstBed>()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.RoomId == roomId &&
                        x.BedNumber != null &&
                        x.BedNumber.ToLower() == normalizedBedNumber &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateBedNumber)
                    return (false, "Nomor bed pada room tersebut sudah digunakan.");
            }

            return (true, null);
        }

        private static IQueryable<MstBed> ApplySorting(
            IQueryable<MstBed> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "bedcode" => isDesc
                    ? query.OrderByDescending(x => x.BedCode)
                    : query.OrderBy(x => x.BedCode),

                "bedname" => isDesc
                    ? query.OrderByDescending(x => x.BedName)
                    : query.OrderBy(x => x.BedName),

                "bednumber" => isDesc
                    ? query.OrderByDescending(x => x.BedNumber)
                    : query.OrderBy(x => x.BedNumber),

                "roomname" => isDesc
                    ? query.OrderByDescending(x => x.Room != null ? x.Room.RoomName : "")
                    : query.OrderBy(x => x.Room != null ? x.Room.RoomName : ""),

                "serviceunitname" => isDesc
                    ? query.OrderByDescending(x => x.Room != null && x.Room.ServiceUnit != null ? x.Room.ServiceUnit.ServiceUnitName : "")
                    : query.OrderBy(x => x.Room != null && x.Room.ServiceUnit != null ? x.Room.ServiceUnit.ServiceUnitName : ""),

                "patientclassname" => isDesc
                    ? query.OrderByDescending(x => x.Room != null && x.Room.PatientClass != null ? x.Room.PatientClass.PatientClassName : "")
                    : query.OrderBy(x => x.Room != null && x.Room.PatientClass != null ? x.Room.PatientClass.PatientClassName : ""),

                "bedstatus" => isDesc
                    ? query.OrderByDescending(x => x.BedStatus)
                    : query.OrderBy(x => x.BedStatus),

                "isreservable" => isDesc
                    ? query.OrderByDescending(x => x.IsReservable)
                    : query.OrderBy(x => x.IsReservable),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.BedName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.BedName)
            };
        }

        private static BedResponse ToResponse(MstBed x)
        {
            return new BedResponse
            {
                Id = x.Id,
                RoomId = x.RoomId,
                RoomCode = x.Room != null ? x.Room.RoomCode : string.Empty,
                RoomName = x.Room != null ? x.Room.RoomName : string.Empty,
                ServiceUnitId = x.Room != null ? x.Room.ServiceUnitId : Guid.Empty,
                ServiceUnitCode = x.Room != null && x.Room.ServiceUnit != null ? x.Room.ServiceUnit.ServiceUnitCode : string.Empty,
                ServiceUnitName = x.Room != null && x.Room.ServiceUnit != null ? x.Room.ServiceUnit.ServiceUnitName : string.Empty,
                PatientClassId = x.Room != null ? x.Room.PatientClassId : null,
                PatientClassCode = x.Room != null && x.Room.PatientClass != null ? x.Room.PatientClass.PatientClassCode : null,
                PatientClassName = x.Room != null && x.Room.PatientClass != null ? x.Room.PatientClass.PatientClassName : null,
                BedCode = x.BedCode,
                BedName = x.BedName,
                BedNumber = x.BedNumber,
                BedStatus = x.BedStatus,
                IsForMale = x.IsForMale,
                IsForFemale = x.IsForFemale,
                IsForNewborn = x.IsForNewborn,
                IsIsolationBed = x.IsIsolationBed,
                IsIntensiveCareBed = x.IsIntensiveCareBed,
                IsOdcBed = x.IsOdcBed,
                IsReservable = x.IsReservable,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<BedEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new BedEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = SplitPascalCase(x.ToString())
                })
                .ToList();
        }

        private static string SplitPascalCase(string value)
        {
            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private Guid GetCurrentUserId()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}