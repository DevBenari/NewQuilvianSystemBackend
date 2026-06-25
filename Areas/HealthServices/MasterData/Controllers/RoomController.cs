using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
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
        [AccessAction("Read", "Read Room", Description = "Melihat metadata filter room", AccessType = AccessTypes.Read, SortOrder = 1)]
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
                    new() { Value = "roomType", Label = "Tipe room" },
                    new() { Value = "serviceUnitName", Label = "Nama service unit" },
                    new() { Value = "patientClassName", Label = "Kelas pasien" },
                    new() { Value = "roomNumber", Label = "Nomor room" },
                    new() { Value = "locationName", Label = "Lokasi" },
                    new() { Value = "floorName", Label = "Lantai" },
                    new() { Value = "capacity", Label = "Kapasitas" },
                    new() { Value = "isForMale", Label = "Untuk laki-laki" },
                    new() { Value = "isForFemale", Label = "Untuk perempuan" },
                    new() { Value = "isForNewborn", Label = "Untuk bayi" },
                    new() { Value = "isIsolationRoom", Label = "Ruang isolasi" },
                    new() { Value = "isIntensiveCare", Label = "Intensive care" },
                    new() { Value = "isOdcRoom", Label = "ODC" },
                    new() { Value = "isAvailableForAdmission", Label = "Tersedia admission" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                RoomTypeOptions = BuildEnumOptions<RoomType>(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
                ResetButtonLabel = "Reset"
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
        [AccessAction("Read", "Read Room", Description = "Melihat ringkasan room", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Room", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new RoomSummaryResponse
            {
                TotalRoom = await query.CountAsync(),
                ActiveRoom = await query.CountAsync(x => x.IsActive),
                InactiveRoom = await query.CountAsync(x => !x.IsActive),
                AdmissionAvailableRoom = await query.CountAsync(x => x.IsAvailableForAdmission),
                IsolationRoom = await query.CountAsync(x => x.IsIsolationRoom),
                IntensiveCareRoom = await query.CountAsync(x => x.IsIntensiveCare),
                OdcRoom = await query.CountAsync(x => x.IsOdcRoom),
                NewbornRoom = await query.CountAsync(x => x.IsForNewborn),
                MaleRoom = await query.CountAsync(x => x.IsForMale),
                FemaleRoom = await query.CountAsync(x => x.IsForFemale)
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
            [FromQuery] RoomType? roomType,
            [FromQuery] bool? isForMale,
            [FromQuery] bool? isForFemale,
            [FromQuery] bool? isForNewborn,
            [FromQuery] bool? isIsolationRoom,
            [FromQuery] bool? isIntensiveCare,
            [FromQuery] bool? isOdcRoom,
            [FromQuery] bool? isAvailableForAdmission,
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

            var query = BuildBaseQuery();
            query = ApplyDateFilter(query, dateRange);
            query = ApplyStandardFilter(
                query,
                serviceUnitId,
                patientClassId,
                isActive,
                roomType,
                isForMale,
                isForFemale,
                isForNewborn,
                isIsolationRoom,
                isIntensiveCare,
                isOdcRoom,
                isAvailableForAdmission,
                search
            );

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities
                    .Select(x => x.CreateBy)
                    .Where(x => x != Guid.Empty)
            );

            var items = entities
                .Select(x => MapResponse(x, actorNames))
                .ToList();

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
        [AccessAction("Read", "Read Room", Description = "Melihat data pilihan room", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Room", "Read")]
        public async Task<IActionResult> GetRoomOptions(
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? patientClassId,
            [FromQuery] RoomType? roomType,
            [FromQuery] bool? isForMale,
            [FromQuery] bool? isForFemale,
            [FromQuery] bool? isForNewborn,
            [FromQuery] bool? isIsolationRoom,
            [FromQuery] bool? isIntensiveCare,
            [FromQuery] bool? isOdcRoom,
            [FromQuery] bool? isAvailableForAdmission,
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool? activeOnly = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var useOnlyActive = activeOnly ?? onlyActive;

            var query = BuildBaseQuery();
            query = ApplyStandardFilter(
                query,
                serviceUnitId,
                patientClassId,
                useOnlyActive ? true : null,
                roomType,
                isForMale,
                isForFemale,
                isForNewborn,
                isIsolationRoom,
                isIntensiveCare,
                isOdcRoom,
                isAvailableForAdmission,
                search
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.RoomName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapOptionResponse)
                .ToList();

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
        [AccessAction("Read", "Read Room", Description = "Melihat detail room", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Room", "Read")]
        public async Task<IActionResult> GetRoomById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Room tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, actorNames);

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
                roomType: request.RoomType,
                roomNumber: request.RoomNumber,
                capacity: request.Capacity,
                isForMale: request.IsForMale,
                isForFemale: request.IsForFemale,
                isForNewborn: request.IsForNewborn
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

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy });

            var response = new RoomCreateResponse
            {
                Id = entity.Id,
                ServiceUnitId = entity.ServiceUnitId,
                PatientClassId = entity.PatientClassId,
                RoomCode = entity.RoomCode,
                RoomName = entity.RoomName,
                RoomType = entity.RoomType,
                RoomTypeName = BuildEnumLabel(entity.RoomType),
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
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
                roomType: request.RoomType,
                roomNumber: request.RoomNumber,
                capacity: request.Capacity,
                isForMale: request.IsForMale,
                isForFemale: request.IsForFemale,
                isForNewborn: request.IsForNewborn
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

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });

            var response = new RoomUpdateResponse
            {
                Id = entity.Id,
                ServiceUnitId = entity.ServiceUnitId,
                PatientClassId = entity.PatientClassId,
                RoomCode = entity.RoomCode,
                RoomName = entity.RoomName,
                RoomType = entity.RoomType,
                RoomTypeName = BuildEnumLabel(entity.RoomType),
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Room.UpdateRoom",
                "Mengubah data room.",
                response
            );

            return Ok(ApiResponse<RoomUpdateResponse>.Ok(
                response,
                "Room berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<RoomUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Room Status", Description = "Mengubah status room", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Room", "Update")]
        public async Task<IActionResult> UpdateRoomStatus(Guid id, [FromBody] UpdateRoomStatusRequest request)
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

            if (request.IsActive)
            {
                var validation = await ValidateRoomCanBeActivatedAsync(entity.ServiceUnitId, entity.PatientClassId);
                if (!validation.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        validation.ErrorMessage ?? "Room tidak dapat diaktifkan."
                    ));
                }
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });

            var response = new RoomUpdateResponse
            {
                Id = entity.Id,
                ServiceUnitId = entity.ServiceUnitId,
                PatientClassId = entity.PatientClassId,
                RoomCode = entity.RoomCode,
                RoomName = entity.RoomName,
                RoomType = entity.RoomType,
                RoomTypeName = BuildEnumLabel(entity.RoomType),
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            return Ok(ApiResponse<RoomUpdateResponse>.Ok(
                response,
                "Status room berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RoomDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Room", Description = "Menghapus data room", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Room", "Delete")]
        public async Task<IActionResult> DeleteRoom(Guid id, [FromBody] DeleteRoomRequest? request = null)
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

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (!string.IsNullOrWhiteSpace(request?.DeleteReason))
            {
                entity.Description = request.DeleteReason.Trim();
            }

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.DeleteBy });

            var response = new RoomDeleteResponse
            {
                Id = entity.Id,
                RoomCode = entity.RoomCode,
                RoomName = entity.RoomName,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : (Guid?)entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Room.DeleteRoom",
                "Menghapus data room.",
                response
            );

            return Ok(ApiResponse<RoomDeleteResponse>.Ok(
                response,
                "Room berhasil dihapus."
            ));
        }

        private IQueryable<MstRoom> BuildBaseQuery()
        {
            return _dbContext.Set<MstRoom>()
                .AsNoTracking()
                .Include(x => x.ServiceUnit)
                .Include(x => x.PatientClass)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstRoom> ApplyDateFilter(
            IQueryable<MstRoom> query,
            DateRangeResolveResult dateRange)
        {
            if (dateRange.Start.HasValue)
            {
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            }

            if (dateRange.EndExclusive.HasValue)
            {
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            }

            return query;
        }

        private static IQueryable<MstRoom> ApplyStandardFilter(
            IQueryable<MstRoom> query,
            Guid? serviceUnitId,
            Guid? patientClassId,
            bool? isActive,
            RoomType? roomType,
            bool? isForMale,
            bool? isForFemale,
            bool? isForNewborn,
            bool? isIsolationRoom,
            bool? isIntensiveCare,
            bool? isOdcRoom,
            bool? isAvailableForAdmission,
            string? search)
        {
            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
            {
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);
            }

            if (patientClassId.HasValue && patientClassId.Value != Guid.Empty)
            {
                query = query.Where(x => x.PatientClassId == patientClassId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (roomType.HasValue)
            {
                query = query.Where(x => x.RoomType == roomType.Value);
            }

            if (isForMale.HasValue)
            {
                query = query.Where(x => x.IsForMale == isForMale.Value);
            }

            if (isForFemale.HasValue)
            {
                query = query.Where(x => x.IsForFemale == isForFemale.Value);
            }

            if (isForNewborn.HasValue)
            {
                query = query.Where(x => x.IsForNewborn == isForNewborn.Value);
            }

            if (isIsolationRoom.HasValue)
            {
                query = query.Where(x => x.IsIsolationRoom == isIsolationRoom.Value);
            }

            if (isIntensiveCare.HasValue)
            {
                query = query.Where(x => x.IsIntensiveCare == isIntensiveCare.Value);
            }

            if (isOdcRoom.HasValue)
            {
                query = query.Where(x => x.IsOdcRoom == isOdcRoom.Value);
            }

            if (isAvailableForAdmission.HasValue)
            {
                query = query.Where(x => x.IsAvailableForAdmission == isAvailableForAdmission.Value);
            }

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

            return query;
        }

        private static IOrderedQueryable<MstRoom> ApplySorting(
            IQueryable<MstRoom> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
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

                "roomtype" => isDesc
                    ? query.OrderByDescending(x => x.RoomType).ThenBy(x => x.RoomName)
                    : query.OrderBy(x => x.RoomType).ThenBy(x => x.RoomName),

                "serviceunitname" => isDesc
                    ? query.OrderByDescending(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty).ThenBy(x => x.RoomName)
                    : query.OrderBy(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty).ThenBy(x => x.RoomName),

                "patientclassname" => isDesc
                    ? query.OrderByDescending(x => x.PatientClass != null ? x.PatientClass.PatientClassName : string.Empty).ThenBy(x => x.RoomName)
                    : query.OrderBy(x => x.PatientClass != null ? x.PatientClass.PatientClassName : string.Empty).ThenBy(x => x.RoomName),

                "roomnumber" => isDesc
                    ? query.OrderByDescending(x => x.RoomNumber).ThenBy(x => x.RoomName)
                    : query.OrderBy(x => x.RoomNumber).ThenBy(x => x.RoomName),

                "locationname" => isDesc
                    ? query.OrderByDescending(x => x.LocationName).ThenBy(x => x.RoomName)
                    : query.OrderBy(x => x.LocationName).ThenBy(x => x.RoomName),

                "floorname" => isDesc
                    ? query.OrderByDescending(x => x.FloorName).ThenBy(x => x.RoomName)
                    : query.OrderBy(x => x.FloorName).ThenBy(x => x.RoomName),

                "capacity" => isDesc
                    ? query.OrderByDescending(x => x.Capacity).ThenBy(x => x.RoomName)
                    : query.OrderBy(x => x.Capacity).ThenBy(x => x.RoomName),

                "isformale" => isDesc
                    ? query.OrderByDescending(x => x.IsForMale).ThenBy(x => x.RoomName)
                    : query.OrderBy(x => x.IsForMale).ThenBy(x => x.RoomName),

                "isforfemale" => isDesc
                    ? query.OrderByDescending(x => x.IsForFemale).ThenBy(x => x.RoomName)
                    : query.OrderBy(x => x.IsForFemale).ThenBy(x => x.RoomName),

                "isfornewborn" => isDesc
                    ? query.OrderByDescending(x => x.IsForNewborn).ThenBy(x => x.RoomName)
                    : query.OrderBy(x => x.IsForNewborn).ThenBy(x => x.RoomName),

                "isisolationroom" => isDesc
                    ? query.OrderByDescending(x => x.IsIsolationRoom).ThenBy(x => x.RoomName)
                    : query.OrderBy(x => x.IsIsolationRoom).ThenBy(x => x.RoomName),

                "isintensivecare" => isDesc
                    ? query.OrderByDescending(x => x.IsIntensiveCare).ThenBy(x => x.RoomName)
                    : query.OrderBy(x => x.IsIntensiveCare).ThenBy(x => x.RoomName),

                "isodcroom" => isDesc
                    ? query.OrderByDescending(x => x.IsOdcRoom).ThenBy(x => x.RoomName)
                    : query.OrderBy(x => x.IsOdcRoom).ThenBy(x => x.RoomName),

                "isavailableforadmission" => isDesc
                    ? query.OrderByDescending(x => x.IsAvailableForAdmission).ThenBy(x => x.RoomName)
                    : query.OrderBy(x => x.IsAvailableForAdmission).ThenBy(x => x.RoomName),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.RoomName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.RoomName),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.RoomName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.RoomName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateUpdateRequestAsync(
            Guid? excludeId,
            Guid serviceUnitId,
            Guid? patientClassId,
            string roomName,
            RoomType roomType,
            string? roomNumber,
            int capacity,
            bool isForMale,
            bool isForFemale,
            bool isForNewborn)
        {
            if (serviceUnitId == Guid.Empty)
            {
                return (false, "Service unit wajib dipilih.");
            }

            if (string.IsNullOrWhiteSpace(roomName))
            {
                return (false, "Nama room wajib diisi.");
            }

            if (!Enum.IsDefined(typeof(RoomType), roomType))
            {
                return (false, "Tipe room tidak valid.");
            }

            if (capacity < 1)
            {
                return (false, "Kapasitas room minimal 1.");
            }

            if (capacity > 999)
            {
                return (false, "Kapasitas room tidak boleh lebih dari 999.");
            }

            if (!isForMale && !isForFemale && !isForNewborn)
            {
                return (false, "Room harus tersedia minimal untuk laki-laki, perempuan, atau bayi baru lahir.");
            }

            var activationValidation = await ValidateRoomCanBeActivatedAsync(serviceUnitId, NormalizeNullableGuid(patientClassId));
            if (!activationValidation.IsValid)
            {
                return activationValidation;
            }

            var normalizedPatientClassId = NormalizeNullableGuid(patientClassId);
            var normalizedName = roomName.Trim().ToLower();

            var duplicateNameInServiceUnit = await _dbContext.Set<MstRoom>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.ServiceUnitId == serviceUnitId &&
                    x.RoomName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateNameInServiceUnit)
            {
                return (false, "Nama room pada service unit tersebut sudah digunakan.");
            }

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
                {
                    return (false, "Nomor room pada service unit tersebut sudah digunakan.");
                }
            }

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRoomCanBeActivatedAsync(
            Guid serviceUnitId,
            Guid? patientClassId)
        {
            var serviceUnitExists = await _dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == serviceUnitId && x.IsActive && !x.IsDelete);

            if (!serviceUnitExists)
            {
                return (false, "Service unit tidak valid atau tidak aktif.");
            }

            var normalizedPatientClassId = NormalizeNullableGuid(patientClassId);

            if (normalizedPatientClassId.HasValue)
            {
                var patientClassExists = await _dbContext.Set<MstPatientClass>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == normalizedPatientClassId.Value && x.IsActive && !x.IsDelete);

                if (!patientClassExists)
                {
                    return (false, "Patient class tidak valid atau tidak aktif.");
                }
            }

            return (true, null);
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

            return RoomCodePrefix + nextNumber.ToString().PadLeft(RoomCodeDigitLength, '0');
        }

        private static int? ExtractRoomCodeNumber(string roomCode)
        {
            if (string.IsNullOrWhiteSpace(roomCode))
            {
                return null;
            }

            if (!roomCode.StartsWith(RoomCodePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var numberText = roomCode[RoomCodePrefix.Length..];

            return int.TryParse(numberText, out var number)
                ? number
                : null;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateGeneratedRoomCodeAsync(string roomCode)
        {
            if (string.IsNullOrWhiteSpace(roomCode))
            {
                return (false, "Kode room otomatis gagal dibuat.");
            }

            var normalizedCode = roomCode.Trim().ToUpperInvariant();

            var duplicateCode = await _dbContext.Set<MstRoom>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.RoomCode.ToUpper() == normalizedCode);

            if (duplicateCode)
            {
                return (false, "Kode room otomatis sudah digunakan. Silakan ulangi proses create.");
            }

            return (true, null);
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> actorIds)
        {
            var ids = actorIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (!ids.Any())
            {
                return new Dictionary<Guid, string?>();
            }

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    Name =
                        x.DisplayName ??
                        x.UserName ??
                        x.Email ??
                        x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static RoomResponse MapResponse(
            MstRoom entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new RoomResponse
            {
                Id = entity.Id,
                ServiceUnitId = entity.ServiceUnitId,
                ServiceUnitCode = entity.ServiceUnit != null ? entity.ServiceUnit.ServiceUnitCode : string.Empty,
                ServiceUnitName = entity.ServiceUnit != null ? entity.ServiceUnit.ServiceUnitName : string.Empty,
                PatientClassId = entity.PatientClassId,
                PatientClassCode = entity.PatientClass != null ? entity.PatientClass.PatientClassCode : null,
                PatientClassName = entity.PatientClass != null ? entity.PatientClass.PatientClassName : null,
                RoomCode = entity.RoomCode,
                RoomName = entity.RoomName,
                RoomType = entity.RoomType,
                RoomTypeName = BuildEnumLabel(entity.RoomType),
                RoomNumber = entity.RoomNumber,
                LocationName = entity.LocationName,
                FloorName = entity.FloorName,
                Capacity = entity.Capacity,
                IsForMale = entity.IsForMale,
                IsForFemale = entity.IsForFemale,
                IsForNewborn = entity.IsForNewborn,
                IsIsolationRoom = entity.IsIsolationRoom,
                IsIntensiveCare = entity.IsIntensiveCare,
                IsOdcRoom = entity.IsOdcRoom,
                IsAvailableForAdmission = entity.IsAvailableForAdmission,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static RoomDetailResponse MapDetailResponse(
            MstRoom entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new RoomDetailResponse
            {
                Id = entity.Id,
                ServiceUnitId = entity.ServiceUnitId,
                ServiceUnitCode = entity.ServiceUnit != null ? entity.ServiceUnit.ServiceUnitCode : string.Empty,
                ServiceUnitName = entity.ServiceUnit != null ? entity.ServiceUnit.ServiceUnitName : string.Empty,
                PatientClassId = entity.PatientClassId,
                PatientClassCode = entity.PatientClass != null ? entity.PatientClass.PatientClassCode : null,
                PatientClassName = entity.PatientClass != null ? entity.PatientClass.PatientClassName : null,
                RoomCode = entity.RoomCode,
                RoomName = entity.RoomName,
                RoomType = entity.RoomType,
                RoomTypeName = BuildEnumLabel(entity.RoomType),
                RoomNumber = entity.RoomNumber,
                LocationName = entity.LocationName,
                FloorName = entity.FloorName,
                Capacity = entity.Capacity,
                IsForMale = entity.IsForMale,
                IsForFemale = entity.IsForFemale,
                IsForNewborn = entity.IsForNewborn,
                IsIsolationRoom = entity.IsIsolationRoom,
                IsIntensiveCare = entity.IsIntensiveCare,
                IsOdcRoom = entity.IsOdcRoom,
                IsAvailableForAdmission = entity.IsAvailableForAdmission,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                Description = entity.Description,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static RoomOptionResponse MapOptionResponse(MstRoom entity)
        {
            return new RoomOptionResponse
            {
                Id = entity.Id,
                ServiceUnitId = entity.ServiceUnitId,
                ServiceUnitCode = entity.ServiceUnit != null ? entity.ServiceUnit.ServiceUnitCode : string.Empty,
                ServiceUnitName = entity.ServiceUnit != null ? entity.ServiceUnit.ServiceUnitName : string.Empty,
                PatientClassId = entity.PatientClassId,
                PatientClassCode = entity.PatientClass != null ? entity.PatientClass.PatientClassCode : null,
                PatientClassName = entity.PatientClass != null ? entity.PatientClass.PatientClassName : null,
                RoomCode = entity.RoomCode,
                RoomName = entity.RoomName,
                RoomType = entity.RoomType,
                RoomTypeName = BuildEnumLabel(entity.RoomType),
                RoomNumber = entity.RoomNumber,
                Capacity = entity.Capacity,
                IsForMale = entity.IsForMale,
                IsForFemale = entity.IsForFemale,
                IsForNewborn = entity.IsForNewborn,
                IsIsolationRoom = entity.IsIsolationRoom,
                IsIntensiveCare = entity.IsIntensiveCare,
                IsOdcRoom = entity.IsOdcRoom,
                IsAvailableForAdmission = entity.IsAvailableForAdmission,
                SortOrder = entity.SortOrder
            };
        }

        private static string? GetActorName(
            IReadOnlyDictionary<Guid, string?> actorNames,
            Guid actorId)
        {
            if (actorId == Guid.Empty)
            {
                return null;
            }

            return actorNames.TryGetValue(actorId, out var actorName)
                ? actorName
                : null;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static DateRangeResolveResult ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var today = AppDateTimeHelper.OperationalDate();

            if (!string.IsNullOrWhiteSpace(customPeriod) &&
                !string.Equals(customPeriod, "custom", StringComparison.OrdinalIgnoreCase))
            {
                return customPeriod.Trim().ToLowerInvariant() switch
                {
                    "today" => DateRangeResolveResult.Valid(today, today.AddDays(1)),
                    "last7days" => DateRangeResolveResult.Valid(today.AddDays(-6), today.AddDays(1)),
                    "last30days" => DateRangeResolveResult.Valid(today.AddDays(-29), today.AddDays(1)),
                    "thismonth" => DateRangeResolveResult.Valid(new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                    "lastmonth" => ResolveLastMonth(today),
                    _ => DateRangeResolveResult.Invalid("Custom period tidak valid.")
                };
            }

            if (startDate.HasValue && endDate.HasValue && startDate.Value.Date > endDate.Value.Date)
            {
                return DateRangeResolveResult.Invalid("Start date tidak boleh lebih besar dari end date.");
            }

            return DateRangeResolveResult.Valid(startDate?.Date, endDate?.Date.AddDays(1));
        }

        private static DateRangeResolveResult ResolveLastMonth(DateTime today)
        {
            var thisMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            return DateRangeResolveResult.Valid(thisMonthStart.AddMonths(-1), thisMonthStart);
        }

        private static List<RoomCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<RoomCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false }
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
                    Label = BuildEnumLabel(x)
                })
                .ToList();
        }

        private static string BuildEnumLabel<TEnum>(TEnum value) where TEnum : Enum
        {
            return SplitPascalCase(value.ToString());
        }

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private static List<RoomQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<RoomQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "date", Description = "Filter tanggal mulai berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "date", Description = "Filter tanggal akhir berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Preset periode. Pilihan: custom, today, last7days, last30days, thismonth, lastmonth.", Example = "last7days" },
                new() { Name = "serviceUnitId", Type = "guid", Description = "Filter room berdasarkan service unit.", Example = "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                new() { Name = "patientClassId", Type = "guid", Description = "Filter room berdasarkan patient class.", Example = "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                new() { Name = "roomType", Type = "enum", Description = "Filter berdasarkan tipe room.", Example = "1" },
                new() { Name = "isActive", Type = "bool", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "search", Type = "string", Description = "Cari berdasarkan kode, nama, nomor, lokasi, service unit, patient class, atau deskripsi.", Example = "Ruang 101" },
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
                new() { Name = "roomCode", Label = "Kode Room", Section = "Basic", InputType = "readonly", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "AutoGenerated", MaxLength = 50, Description = "Digenerate otomatis oleh sistem dengan format RM-RSMMC-00001.", Example = "RM-RSMMC-00001", SortOrder = 1 },
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
            var userIdText =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

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
            {
                return null;
            }

            return value.Value;
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
    }
}
