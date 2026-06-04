using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseRoomPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.RoomResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/rooms")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Room",
        AreaName = "HealthServices",
        ControllerName = "Room",
        Description = "Health service master data room",
        SortOrder = 4
    )]
    [Tags("Health Services / Master Data / Room")]
    public class RoomController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";
        private const string RoomCodePrefix = "RM-RSMMC-";
        private const int RoomCodeDigitLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public RoomController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<RoomFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Room", Description = "Melihat data room", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Room", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new RoomFilterMetadataResponse
            {
                DefaultFilter = new RoomDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<RoomSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "roomCode", Label = "Kode room" },
                    new() { Value = "roomName", Label = "Nama room" },
                    new() { Value = "serviceUnitName", Label = "Nama service unit" },
                    new() { Value = "patientClassName", Label = "Kelas pasien" },
                    new() { Value = "capacity", Label = "Kapasitas" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                RoomTypeOptions = BuildEnumOptions<RoomType>(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Room.GetFilterMetadata",
                "Mengambil metadata filter room.",
                result
            );

            return Ok(ApiResponse<RoomFilterMetadataResponse>.Ok(
                result,
                "Metadata filter room berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<RoomSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Room", Description = "Melihat data room", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Room", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstRoom>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new RoomSummaryResponse
            {
                TotalRoom = await query.CountAsync(),
                ActiveRoom = await query.CountAsync(x => x.IsActive),
                InactiveRoom = await query.CountAsync(x => !x.IsActive),
                AdmissionAvailableRoom = await query.CountAsync(x => x.IsAvailableForAdmission),
                IsolationRoom = await query.CountAsync(x => x.IsIsolationRoom),
                IntensiveCareRoom = await query.CountAsync(x => x.IsIntensiveCare),
                OdcRoom = await query.CountAsync(x => x.IsOdcRoom),
                NewbornRoom = await query.CountAsync(x => x.IsForNewborn)
            };

            return Ok(ApiResponse<RoomSummaryResponse>.Ok(
                result,
                "Ringkasan room berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseRoomPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Room", Description = "Melihat data room", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Room", "Read")]
        public async Task<IActionResult> GetRooms(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? patientClassId,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
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

            var query = _dbContext.Set<MstRoom>()
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

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (patientClassId.HasValue && patientClassId.Value != Guid.Empty)
                query = query.Where(x => x.PatientClassId == patientClassId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.RoomCode.ToLower().Contains(keyword) ||
                    x.RoomName.ToLower().Contains(keyword) ||
                    (x.RoomNumber != null && x.RoomNumber.ToLower().Contains(keyword)) ||
                    (x.LocationName != null && x.LocationName.ToLower().Contains(keyword)) ||
                    (x.FloorName != null && x.FloorName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitCode.ToLower().Contains(keyword)) ||
                    (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitName.ToLower().Contains(keyword)) ||
                    (x.PatientClass != null && x.PatientClass.PatientClassCode.ToLower().Contains(keyword)) ||
                    (x.PatientClass != null && x.PatientClass.PatientClassName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RoomResponse
                {
                    Id = x.Id,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitCode = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitCode : string.Empty,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty,
                    PatientClassId = x.PatientClassId,
                    PatientClassCode = x.PatientClass != null ? x.PatientClass.PatientClassCode : null,
                    PatientClassName = x.PatientClass != null ? x.PatientClass.PatientClassName : null,
                    RoomCode = x.RoomCode,
                    RoomName = x.RoomName,
                    RoomType = x.RoomType,
                    RoomNumber = x.RoomNumber,
                    LocationName = x.LocationName,
                    FloorName = x.FloorName,
                    Capacity = x.Capacity,
                    IsForMale = x.IsForMale,
                    IsForFemale = x.IsForFemale,
                    IsForNewborn = x.IsForNewborn,
                    IsIsolationRoom = x.IsIsolationRoom,
                    IsIntensiveCare = x.IsIntensiveCare,
                    IsOdcRoom = x.IsOdcRoom,
                    IsAvailableForAdmission = x.IsAvailableForAdmission,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new ResponseRoomPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseRoomPagedResult>.Ok(
                result,
                "Data room berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<RoomOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Room", Description = "Melihat data room", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Room", "Read")]
        public async Task<IActionResult> GetRoomOptions(
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? patientClassId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstRoom>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (patientClassId.HasValue && patientClassId.Value != Guid.Empty)
                query = query.Where(x => x.PatientClassId == patientClassId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.RoomCode.ToLower().Contains(keyword) ||
                    x.RoomName.ToLower().Contains(keyword) ||
                    (x.RoomNumber != null && x.RoomNumber.ToLower().Contains(keyword)) ||
                    (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitName.ToLower().Contains(keyword)) ||
                    (x.PatientClass != null && x.PatientClass.PatientClassName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.RoomName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RoomOptionResponse
                {
                    Id = x.Id,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null
                        ? x.ServiceUnit.ServiceUnitName
                        : string.Empty,

                    PatientClassId = x.PatientClassId,
                    PatientClassName = x.PatientClass != null
                        ? x.PatientClass.PatientClassName
                        : null,

                    RoomCode = x.RoomCode,
                    RoomName = x.RoomName,
                    RoomType = x.RoomType,
                    RoomNumber = x.RoomNumber,
                    Capacity = x.Capacity,
                    IsAvailableForAdmission = x.IsAvailableForAdmission
                })
                .ToListAsync();

            var result = new RoomOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<RoomOptionPagedResponse>.Ok(
                result,
                "Data pilihan room berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RoomDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Room", Description = "Melihat data room", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Room", "Read")]
        public async Task<IActionResult> GetRoomById(Guid id)
        {
            var data = await _dbContext.Set<MstRoom>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new RoomDetailResponse
                {
                    Id = x.Id,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitCode = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitCode : string.Empty,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty,
                    PatientClassId = x.PatientClassId,
                    PatientClassCode = x.PatientClass != null ? x.PatientClass.PatientClassCode : null,
                    PatientClassName = x.PatientClass != null ? x.PatientClass.PatientClassName : null,
                    RoomCode = x.RoomCode,
                    RoomName = x.RoomName,
                    RoomType = x.RoomType,
                    RoomNumber = x.RoomNumber,
                    LocationName = x.LocationName,
                    FloorName = x.FloorName,
                    Capacity = x.Capacity,
                    IsForMale = x.IsForMale,
                    IsForFemale = x.IsForFemale,
                    IsForNewborn = x.IsForNewborn,
                    IsIsolationRoom = x.IsIsolationRoom,
                    IsIntensiveCare = x.IsIntensiveCare,
                    IsOdcRoom = x.IsOdcRoom,
                    IsAvailableForAdmission = x.IsAvailableForAdmission,
                    SortOrder = x.SortOrder,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Room tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<RoomDetailResponse>.Ok(
                data,
                "Detail room berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RoomCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Room", Description = "Membuat data room", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Room", "Create")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
        {
            var validation = await ValidateCreateUpdateRequestAsync(
                excludeId: null,
                serviceUnitId: request.ServiceUnitId,
                patientClassId: request.PatientClassId,
                roomName: request.RoomName,
                roomNumber: request.RoomNumber,
                capacity: request.Capacity
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data room tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var generatedRoomCode = await GenerateRoomCodeAsync();
            var codeValidation = await ValidateGeneratedRoomCodeAsync(generatedRoomCode);

            if (!codeValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    codeValidation.ErrorMessage ?? "Kode room otomatis tidak valid."
                ));
            }

            var entity = new MstRoom
            {
                Id = Guid.NewGuid(),
                ServiceUnitId = request.ServiceUnitId,
                PatientClassId = NormalizeNullableGuid(request.PatientClassId),
                RoomCode = generatedRoomCode,
                RoomName = request.RoomName.Trim(),
                RoomType = request.RoomType,
                RoomNumber = NormalizeNullableText(request.RoomNumber),
                LocationName = NormalizeNullableText(request.LocationName),
                FloorName = NormalizeNullableText(request.FloorName),
                Capacity = request.Capacity,
                IsForMale = request.IsForMale,
                IsForFemale = request.IsForFemale,
                IsForNewborn = request.IsForNewborn,
                IsIsolationRoom = request.IsIsolationRoom,
                IsIntensiveCare = request.IsIntensiveCare,
                IsOdcRoom = request.IsOdcRoom,
                IsAvailableForAdmission = request.IsAvailableForAdmission,
                SortOrder = request.SortOrder,
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstRoom>().Add(entity);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var response = new RoomCreateResponse
            {
                Id = entity.Id,
                ServiceUnitId = entity.ServiceUnitId,
                PatientClassId = entity.PatientClassId,
                RoomCode = entity.RoomCode,
                RoomName = entity.RoomName,
                RoomType = entity.RoomType,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Room.CreateRoom",
                "Membuat data room.",
                response
            );

            return Ok(ApiResponse<RoomCreateResponse>.Ok(
                response,
                "Room berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RoomUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Room", Description = "Mengubah data room", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Room", "Update")]
        public async Task<IActionResult> UpdateRoom(Guid id, [FromBody] UpdateRoomRequest request)
        {
            var entity = await _dbContext.Set<MstRoom>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Room tidak ditemukan."
                ));
            }

            var validation = await ValidateCreateUpdateRequestAsync(
                excludeId: id,
                serviceUnitId: request.ServiceUnitId,
                patientClassId: request.PatientClassId,
                roomName: request.RoomName,
                roomNumber: request.RoomNumber,
                capacity: request.Capacity
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data room tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.ServiceUnitId = request.ServiceUnitId;
            entity.PatientClassId = NormalizeNullableGuid(request.PatientClassId);
            entity.RoomName = request.RoomName.Trim();
            entity.RoomType = request.RoomType;
            entity.RoomNumber = NormalizeNullableText(request.RoomNumber);
            entity.LocationName = NormalizeNullableText(request.LocationName);
            entity.FloorName = NormalizeNullableText(request.FloorName);
            entity.Capacity = request.Capacity;
            entity.IsForMale = request.IsForMale;
            entity.IsForFemale = request.IsForFemale;
            entity.IsForNewborn = request.IsForNewborn;
            entity.IsIsolationRoom = request.IsIsolationRoom;
            entity.IsIntensiveCare = request.IsIntensiveCare;
            entity.IsOdcRoom = request.IsOdcRoom;
            entity.IsAvailableForAdmission = request.IsAvailableForAdmission;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = new RoomUpdateResponse
            {
                Id = entity.Id,
                ServiceUnitId = entity.ServiceUnitId,
                PatientClassId = entity.PatientClassId,
                RoomCode = entity.RoomCode,
                RoomName = entity.RoomName,
                RoomType = entity.RoomType,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<RoomUpdateResponse>.Ok(
                response,
                "Room berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Room", Description = "Menghapus data room", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Room", "Delete")]
        public async Task<IActionResult> DeleteRoom(Guid id)
        {
            var entity = await _dbContext.Set<MstRoom>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Room tidak ditemukan."
                ));
            }

            var isUsedByBed = await _dbContext.Set<MstBed>()
                .AnyAsync(x => x.RoomId == id && !x.IsDelete);

            if (isUsedByBed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Room tidak dapat dihapus karena sudah digunakan oleh bed."
                ));
            }

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Room berhasil dihapus."
            ));
        }

        private async Task<string> GenerateRoomCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstRoom>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.RoomCode.StartsWith(RoomCodePrefix))
                .Select(x => x.RoomCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(ExtractRoomCodeNumber)
                .Where(x => x.HasValue && x.Value > 0)
                .Select(x => x!.Value)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return $"{RoomCodePrefix}{nextNumber.ToString().PadLeft(RoomCodeDigitLength, '0')}";
        }

        private static int? ExtractRoomCodeNumber(string roomCode)
        {
            if (string.IsNullOrWhiteSpace(roomCode))
                return null;

            if (!roomCode.StartsWith(RoomCodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var numberText = roomCode[RoomCodePrefix.Length..];

            return int.TryParse(numberText, out var number)
                ? number
                : null;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateUpdateRequestAsync(
            Guid? excludeId,
            Guid serviceUnitId,
            Guid? patientClassId,
            string roomName,
            string? roomNumber,
            int capacity)
        {
            if (serviceUnitId == Guid.Empty)
                return (false, "Service unit wajib dipilih.");

            if (string.IsNullOrWhiteSpace(roomName))
                return (false, "Nama room wajib diisi.");

            if (capacity < 1)
                return (false, "Kapasitas room minimal 1.");

            if (capacity > 999)
                return (false, "Kapasitas room tidak boleh lebih dari 999.");

            var serviceUnitExists = await _dbContext.Set<MstServiceUnit>()
                .AnyAsync(x => x.Id == serviceUnitId && x.IsActive && !x.IsDelete);

            if (!serviceUnitExists)
                return (false, "Service unit tidak valid atau tidak aktif.");

            var normalizedPatientClassId = NormalizeNullableGuid(patientClassId);

            if (normalizedPatientClassId.HasValue)
            {
                var patientClassExists = await _dbContext.Set<MstPatientClass>()
                    .AnyAsync(x => x.Id == normalizedPatientClassId.Value && x.IsActive && !x.IsDelete);

                if (!patientClassExists)
                    return (false, "Patient class tidak valid atau tidak aktif.");
            }

            var normalizedName = roomName.Trim().ToLower();

            var duplicateNameInServiceUnit = await _dbContext.Set<MstRoom>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.ServiceUnitId == serviceUnitId &&
                    x.RoomName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateNameInServiceUnit)
                return (false, "Nama room pada service unit tersebut sudah digunakan.");

            if (!string.IsNullOrWhiteSpace(roomNumber))
            {
                var normalizedRoomNumber = roomNumber.Trim().ToLower();

                var duplicateRoomNumber = await _dbContext.Set<MstRoom>()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.ServiceUnitId == serviceUnitId &&
                        x.RoomNumber != null &&
                        x.RoomNumber.ToLower() == normalizedRoomNumber &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateRoomNumber)
                    return (false, "Nomor room pada service unit tersebut sudah digunakan.");
            }

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateGeneratedRoomCodeAsync(string roomCode)
        {
            if (string.IsNullOrWhiteSpace(roomCode))
                return (false, "Kode room otomatis gagal dibuat.");

            var normalizedCode = roomCode.Trim().ToUpperInvariant();

            var duplicateCode = await _dbContext.Set<MstRoom>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.RoomCode.ToUpper() == normalizedCode);

            if (duplicateCode)
                return (false, "Kode room otomatis sudah digunakan. Silakan ulangi proses create.");

            return (true, null);
        }

        private static IQueryable<MstRoom> ApplySorting(
            IQueryable<MstRoom> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "roomcode" => isDesc
                    ? query.OrderByDescending(x => x.RoomCode)
                    : query.OrderBy(x => x.RoomCode),

                "roomname" => isDesc
                    ? query.OrderByDescending(x => x.RoomName)
                    : query.OrderBy(x => x.RoomName),

                "serviceunitname" => isDesc
                    ? query.OrderByDescending(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : "")
                    : query.OrderBy(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : ""),

                "patientclassname" => isDesc
                    ? query.OrderByDescending(x => x.PatientClass != null ? x.PatientClass.PatientClassName : "")
                    : query.OrderBy(x => x.PatientClass != null ? x.PatientClass.PatientClassName : ""),

                "capacity" => isDesc
                    ? query.OrderByDescending(x => x.Capacity)
                    : query.OrderBy(x => x.Capacity),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.RoomName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.RoomName)
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static (bool IsValid, DateTime? Start, DateTime? EndExclusive, string? ErrorMessage)
            ResolveDateRange(DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            var today = DateTime.UtcNow.Date;

            if (!string.IsNullOrWhiteSpace(customPeriod) &&
                !string.Equals(customPeriod, "custom", StringComparison.OrdinalIgnoreCase))
            {
                return customPeriod.ToLowerInvariant() switch
                {
                    "today" => (true, today, today.AddDays(1), null),
                    "last7days" => (true, today.AddDays(-6), today.AddDays(1), null),
                    "last30days" => (true, today.AddDays(-29), today.AddDays(1), null),
                    "thismonth" => (true, new DateTime(today.Year, today.Month, 1), new DateTime(today.Year, today.Month, 1).AddMonths(1), null),
                    _ => (false, null, null, "Custom period tidak valid.")
                };
            }

            if (startDate.HasValue && endDate.HasValue && startDate.Value.Date > endDate.Value.Date)
            {
                return (false, null, null, "Start date tidak boleh lebih besar dari end date.");
            }

            return (
                true,
                startDate?.Date,
                endDate?.Date.AddDays(1),
                null
            );
        }

        private static List<RoomCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<RoomCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false }
            };
        }

        private static List<RoomEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new RoomEnumOptionResponse
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

        private static List<RoomQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<RoomQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "date", Description = "Filter tanggal mulai berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "date", Description = "Filter tanggal akhir berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Preset periode. Pilihan: custom, today, last7days, last30days, thismonth.", Example = "last7days" },
                new() { Name = "serviceUnitId", Type = "guid", Description = "Relasi table 1. Filter room berdasarkan service unit.", Example = "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                new() { Name = "patientClassId", Type = "guid", Description = "Relasi table 2. Filter room berdasarkan patient class.", Example = "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                new() { Name = "isActive", Type = "bool", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "search", Type = "string", Description = "Cari berdasarkan kode room, nama room, nomor room, lokasi, deskripsi, service unit, atau patient class.", Example = "Ruang 101" },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting.", Example = "sortOrder" },
                new() { Name = "sortDirection", Type = "string", Description = "Arah sorting: asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman.", Example = "25" }
            };
        }

        private static List<RoomFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: false);
        }

        private static List<RoomFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: true);
        }

        private static List<RoomFormFieldMetadataResponse> BuildFieldMetadata(bool isUpdate)
        {
            var fields = new List<RoomFormFieldMetadataResponse>
            {
                new() { Name = "roomCode", Label = "Kode Room", Section = "Basic", InputType = "readonly", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "AutoGenerated", MaxLength = 50, Description = "Digenerate otomatis oleh sistem dengan format RM-RSMMC-00001. Nomor terkecil yang kosong dari data aktif akan dipakai kembali.", Example = "RM-RSMMC-00001", SortOrder = 1 },
                new() { Name = "serviceUnitId", Label = "Service Unit", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "/api/v1/health-services/master-data/service-units/options", SortOrder = 2 },
                new() { Name = "patientClassId", Label = "Patient Class", Section = "Basic", InputType = "select", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "Optional", OptionsSource = "/api/v1/health-services/master-data/patient-classes/options", Description = "Boleh kosong jika room tidak terikat kelas pasien tertentu.", SortOrder = 3 },
                new() { Name = "roomName", Label = "Nama Room", Section = "Basic", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 150, Example = "Ruang Anggrek", SortOrder = 4 },
                new() { Name = "roomType", Label = "Tipe Room", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "roomTypeOptions", SortOrder = 5 },
                new() { Name = "roomNumber", Label = "Nomor Room", Section = "Basic", InputType = "text", MaxLength = 50, Example = "101", SortOrder = 6 },
                new() { Name = "locationName", Label = "Lokasi", Section = "Location", InputType = "text", MaxLength = 100, Example = "Gedung Rawat Inap", SortOrder = 7 },
                new() { Name = "floorName", Label = "Lantai", Section = "Location", InputType = "text", MaxLength = 50, Example = "Lantai 2", SortOrder = 8 },
                new() { Name = "capacity", Label = "Kapasitas", Section = "Capacity", InputType = "number", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", Description = "Jumlah kapasitas tempat tidur/kapasitas layanan di room.", Example = "1", SortOrder = 9 },
                new() { Name = "isForMale", Label = "Untuk Laki-laki", Section = "Rule", InputType = "switch", SortOrder = 10 },
                new() { Name = "isForFemale", Label = "Untuk Perempuan", Section = "Rule", InputType = "switch", SortOrder = 11 },
                new() { Name = "isForNewborn", Label = "Untuk Bayi Baru Lahir", Section = "Rule", InputType = "switch", SortOrder = 12 },
                new() { Name = "isIsolationRoom", Label = "Ruang Isolasi", Section = "Rule", InputType = "switch", SortOrder = 13 },
                new() { Name = "isIntensiveCare", Label = "Intensive Care", Section = "Rule", InputType = "switch", SortOrder = 14 },
                new() { Name = "isOdcRoom", Label = "One Day Care", Section = "Rule", InputType = "switch", SortOrder = 15 },
                new() { Name = "isAvailableForAdmission", Label = "Tersedia Untuk Admission", Section = "Rule", InputType = "switch", SortOrder = 16 },
                new() { Name = "sortOrder", Label = "Urutan", Section = "Display", InputType = "number", SortOrder = 17 },
                new() { Name = "description", Label = "Deskripsi", Section = "Additional", InputType = "textarea", MaxLength = 250, SortOrder = 18 }
            };

            if (isUpdate)
            {
                fields.Add(new RoomFormFieldMetadataResponse
                {
                    Name = "isActive",
                    Label = "Status Aktif",
                    Section = "Status",
                    InputType = "switch",
                    SortOrder = 99
                });
            }

            return fields.OrderBy(x => x.SortOrder).ToList();
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

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
                return null;

            return value.Value;
        }
    }
}
