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
using System.Data;
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
        private const string BedCodePrefix = "BD-RSMMC-";
        private const int BedCodeDigitLength = 5;

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
        [AccessAction("Read", "Read Bed", Description = "Melihat metadata filter bed", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Bed", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new BedFilterMetadataResponse
            {
                DefaultFilter = new BedDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
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
                    new() { Value = "isForMale", Label = "Untuk laki-laki" },
                    new() { Value = "isForFemale", Label = "Untuk perempuan" },
                    new() { Value = "isForNewborn", Label = "Untuk bayi" },
                    new() { Value = "isIsolationBed", Label = "Bed isolasi" },
                    new() { Value = "isIntensiveCareBed", Label = "Intensive care" },
                    new() { Value = "isOdcBed", Label = "ODC" },
                    new() { Value = "isReservable", Label = "Bisa reservasi" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                BedStatusOptions = BuildEnumOptions<BedStatus>(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
                ResetButtonLabel = "Reset"
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
        [AccessAction("Read", "Read Bed", Description = "Melihat ringkasan bed", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Bed", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var beds = await BuildBaseQuery().ToListAsync();

            var result = new BedSummaryResponse
            {
                TotalBed = beds.Count,
                ActiveBed = beds.Count(x => x.IsActive),
                InactiveBed = beds.Count(x => !x.IsActive),
                AvailableBed = beds.Count(x => x.BedStatus == BedStatus.Available),
                OccupiedBed = beds.Count(x => string.Equals(x.BedStatus.ToString(), "Occupied", StringComparison.OrdinalIgnoreCase)),
                MaintenanceBed = beds.Count(x => string.Equals(x.BedStatus.ToString(), "Maintenance", StringComparison.OrdinalIgnoreCase)),
                ReservableBed = beds.Count(x => x.IsReservable),
                IsolationBed = beds.Count(x => x.IsIsolationBed),
                IntensiveCareBed = beds.Count(x => x.IsIntensiveCareBed),
                OdcBed = beds.Count(x => x.IsOdcBed),
                NewbornBed = beds.Count(x => x.IsForNewborn),
                MaleBed = beds.Count(x => x.IsForMale),
                FemaleBed = beds.Count(x => x.IsForFemale)
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
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? roomId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? patientClassId,
            [FromQuery] bool? isActive,
            [FromQuery] BedStatus? bedStatus,
            [FromQuery] bool? isForMale,
            [FromQuery] bool? isForFemale,
            [FromQuery] bool? isForNewborn,
            [FromQuery] bool? isIsolationBed,
            [FromQuery] bool? isIntensiveCareBed,
            [FromQuery] bool? isOdcBed,
            [FromQuery] bool? isReservable,
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
                roomId,
                serviceUnitId,
                patientClassId,
                isActive,
                bedStatus,
                isForMale,
                isForFemale,
                isForNewborn,
                isIsolationBed,
                isIntensiveCareBed,
                isOdcBed,
                isReservable,
                search
            );

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities.Select(x => x.CreateBy).Where(x => x != Guid.Empty)
            );

            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

            var result = new ResponseBedPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseBedPagedResult>.Ok(
                result,
                "Data bed berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<BedOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Bed", Description = "Melihat data pilihan bed", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Bed", "Read")]
        public async Task<IActionResult> GetBedOptions(
            [FromQuery] Guid? roomId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? patientClassId,
            [FromQuery] BedStatus? bedStatus,
            [FromQuery] bool? isReservable,
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool? activeOnly = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyStandardFilter(
                query,
                roomId,
                serviceUnitId,
                patientClassId,
                activeOnly ?? onlyActive,
                bedStatus,
                null,
                null,
                null,
                null,
                null,
                null,
                isReservable,
                search
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.BedName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(MapOptionResponse).ToList();

            var result = new BedOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<BedOptionPagedResponse>.Ok(
                result,
                "Data pilihan bed berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BedDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Bed", Description = "Melihat detail bed", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Bed", "Read")]
        public async Task<IActionResult> GetBedById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Bed tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var result = MapDetailResponse(entity, actorNames);

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
            var validation = await ValidateCreateUpdateRequestAsync(
                excludeId: null,
                roomId: request.RoomId,
                bedName: request.BedName,
                bedNumber: request.BedNumber,
                isForMale: request.IsForMale,
                isForFemale: request.IsForFemale,
                isForNewborn: request.IsForNewborn
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

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var generatedBedCode = await GenerateBedCodeAsync();
            var codeValidation = await ValidateGeneratedBedCodeAsync(generatedBedCode);

            if (!codeValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    codeValidation.ErrorMessage ?? "Kode bed otomatis tidak valid."
                ));
            }

            var entity = new MstBed
            {
                Id = Guid.NewGuid(),
                RoomId = request.RoomId,
                BedCode = generatedBedCode,
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
            await transaction.CommitAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = MapCreateResponse(entity, actorNames);

            await _loggerService.InfoAsync(
                LogCategory,
                "Bed.CreateBed",
                "Membuat data bed.",
                result
            );

            return Ok(ApiResponse<BedCreateResponse>.Ok(
                result,
                "Bed berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BedUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
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

            var validation = await ValidateCreateUpdateRequestAsync(
                excludeId: id,
                roomId: request.RoomId,
                bedName: request.BedName,
                bedNumber: request.BedNumber,
                isForMale: request.IsForMale,
                isForFemale: request.IsForFemale,
                isForNewborn: request.IsForNewborn
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

            entity.RoomId = request.RoomId;
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
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = MapUpdateResponse(entity, actorNames);

            await _loggerService.InfoAsync(
                LogCategory,
                "Bed.UpdateBed",
                "Mengubah data bed.",
                result
            );

            return Ok(ApiResponse<BedUpdateResponse>.Ok(
                result,
                "Bed berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<BedUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Bed Status", Description = "Mengubah status aktif bed", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Bed", "Update")]
        public async Task<IActionResult> UpdateBedStatus(Guid id, [FromBody] UpdateBedStatusRequest request)
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

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = MapUpdateResponse(entity, actorNames);

            return Ok(ApiResponse<BedUpdateResponse>.Ok(
                result,
                "Status bed berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/availability")]
        [ProducesResponseType(typeof(ApiResponse<BedUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Bed Availability", Description = "Mengubah status ketersediaan bed", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Bed", "Update")]
        public async Task<IActionResult> UpdateBedAvailability(Guid id, [FromBody] UpdateBedAvailabilityRequest request)
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

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.BedStatus = request.BedStatus;
            entity.Description = NormalizeNullableText(request.Description) ?? entity.Description;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = MapUpdateResponse(entity, actorNames);

            return Ok(ApiResponse<BedUpdateResponse>.Ok(
                result,
                "Status ketersediaan bed berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BedDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Bed", Description = "Menghapus data bed", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Bed", "Delete")]
        public async Task<IActionResult> DeleteBed(Guid id, [FromBody] DeleteBedRequest? request = null)
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

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = new BedDeleteResponse
            {
                Id = entity.Id,
                BedCode = entity.BedCode,
                BedName = entity.BedName,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : (Guid?)entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Bed.DeleteBed",
                "Menghapus data bed.",
                result
            );

            return Ok(ApiResponse<BedDeleteResponse>.Ok(
                result,
                "Bed berhasil dihapus."
            ));
        }

        private IQueryable<MstBed> BuildBaseQuery()
        {
            return _dbContext.Set<MstBed>()
                .AsNoTracking()
                .Include(x => x.Room)
                    .ThenInclude(x => x!.ServiceUnit)
                .Include(x => x.Room)
                    .ThenInclude(x => x!.PatientClass)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstBed> ApplyDateFilter(
            IQueryable<MstBed> query,
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

        private static IQueryable<MstBed> ApplyStandardFilter(
            IQueryable<MstBed> query,
            Guid? roomId,
            Guid? serviceUnitId,
            Guid? patientClassId,
            bool? isActive,
            BedStatus? bedStatus,
            bool? isForMale,
            bool? isForFemale,
            bool? isForNewborn,
            bool? isIsolationBed,
            bool? isIntensiveCareBed,
            bool? isOdcBed,
            bool? isReservable,
            string? search)
        {
            if (roomId.HasValue && roomId.Value != Guid.Empty)
                query = query.Where(x => x.RoomId == roomId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.Room != null && x.Room.ServiceUnitId == serviceUnitId.Value);

            if (patientClassId.HasValue && patientClassId.Value != Guid.Empty)
                query = query.Where(x => x.Room != null && x.Room.PatientClassId == patientClassId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

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

            return query;
        }

        private static IOrderedQueryable<MstBed> ApplySorting(
            IQueryable<MstBed> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "bedcode" => isDesc ? query.OrderByDescending(x => x.BedCode) : query.OrderBy(x => x.BedCode),
                "bedname" => isDesc ? query.OrderByDescending(x => x.BedName) : query.OrderBy(x => x.BedName),
                "bednumber" => isDesc ? query.OrderByDescending(x => x.BedNumber) : query.OrderBy(x => x.BedNumber),
                "roomname" => isDesc ? query.OrderByDescending(x => x.Room != null ? x.Room.RoomName : string.Empty) : query.OrderBy(x => x.Room != null ? x.Room.RoomName : string.Empty),
                "serviceunitname" => isDesc ? query.OrderByDescending(x => x.Room != null && x.Room.ServiceUnit != null ? x.Room.ServiceUnit.ServiceUnitName : string.Empty) : query.OrderBy(x => x.Room != null && x.Room.ServiceUnit != null ? x.Room.ServiceUnit.ServiceUnitName : string.Empty),
                "patientclassname" => isDesc ? query.OrderByDescending(x => x.Room != null && x.Room.PatientClass != null ? x.Room.PatientClass.PatientClassName : string.Empty) : query.OrderBy(x => x.Room != null && x.Room.PatientClass != null ? x.Room.PatientClass.PatientClassName : string.Empty),
                "bedstatus" => isDesc ? query.OrderByDescending(x => x.BedStatus).ThenBy(x => x.BedName) : query.OrderBy(x => x.BedStatus).ThenBy(x => x.BedName),
                "isformale" => isDesc ? query.OrderByDescending(x => x.IsForMale).ThenBy(x => x.BedName) : query.OrderBy(x => x.IsForMale).ThenBy(x => x.BedName),
                "isforfemale" => isDesc ? query.OrderByDescending(x => x.IsForFemale).ThenBy(x => x.BedName) : query.OrderBy(x => x.IsForFemale).ThenBy(x => x.BedName),
                "isfornewborn" => isDesc ? query.OrderByDescending(x => x.IsForNewborn).ThenBy(x => x.BedName) : query.OrderBy(x => x.IsForNewborn).ThenBy(x => x.BedName),
                "isisolationbed" => isDesc ? query.OrderByDescending(x => x.IsIsolationBed).ThenBy(x => x.BedName) : query.OrderBy(x => x.IsIsolationBed).ThenBy(x => x.BedName),
                "isintensivecarebed" => isDesc ? query.OrderByDescending(x => x.IsIntensiveCareBed).ThenBy(x => x.BedName) : query.OrderBy(x => x.IsIntensiveCareBed).ThenBy(x => x.BedName),
                "isodcbed" => isDesc ? query.OrderByDescending(x => x.IsOdcBed).ThenBy(x => x.BedName) : query.OrderBy(x => x.IsOdcBed).ThenBy(x => x.BedName),
                "isreservable" => isDesc ? query.OrderByDescending(x => x.IsReservable).ThenBy(x => x.BedName) : query.OrderBy(x => x.IsReservable).ThenBy(x => x.BedName),
                "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.BedName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.BedName),
                _ => isDesc ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.BedName) : query.OrderBy(x => x.SortOrder).ThenBy(x => x.BedName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateUpdateRequestAsync(
            Guid? excludeId,
            Guid roomId,
            string bedName,
            string? bedNumber,
            bool isForMale,
            bool isForFemale,
            bool isForNewborn)
        {
            if (roomId == Guid.Empty)
                return (false, "Room wajib dipilih.");

            if (string.IsNullOrWhiteSpace(bedName))
                return (false, "Nama bed wajib diisi.");

            if (!isForMale && !isForFemale && !isForNewborn)
                return (false, "Bed minimal harus bisa digunakan untuk laki-laki, perempuan, atau bayi baru lahir.");

            var roomExists = await _dbContext.Set<MstRoom>()
                .AnyAsync(x => x.Id == roomId && x.IsActive && !x.IsDelete);

            if (!roomExists)
                return (false, "Room tidak valid atau tidak aktif.");

            var normalizedName = bedName.Trim().ToLower();

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

        private async Task<string> GenerateBedCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstBed>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.BedCode.StartsWith(BedCodePrefix))
                .Select(x => x.BedCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(ExtractBedCodeNumber)
                .Where(x => x.HasValue && x.Value > 0)
                .Select(x => x!.Value)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return BedCodePrefix + nextNumber.ToString().PadLeft(BedCodeDigitLength, '0');
        }

        private static int? ExtractBedCodeNumber(string bedCode)
        {
            if (string.IsNullOrWhiteSpace(bedCode))
                return null;

            if (!bedCode.StartsWith(BedCodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var numberText = bedCode[BedCodePrefix.Length..];

            return int.TryParse(numberText, out var number) ? number : null;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateGeneratedBedCodeAsync(string bedCode)
        {
            if (string.IsNullOrWhiteSpace(bedCode))
                return (false, "Kode bed otomatis gagal dibuat.");

            var normalizedCode = bedCode.Trim().ToUpperInvariant();

            var duplicateCode = await _dbContext.Set<MstBed>()
                .AnyAsync(x => !x.IsDelete && x.BedCode.ToUpper() == normalizedCode);

            if (duplicateCode)
                return (false, "Kode bed otomatis sudah digunakan. Silakan ulangi proses create.");

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
                    Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static BedResponse MapResponse(MstBed entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new BedResponse
            {
                Id = entity.Id,
                RoomId = entity.RoomId,
                RoomCode = entity.Room?.RoomCode ?? string.Empty,
                RoomName = entity.Room?.RoomName ?? string.Empty,
                ServiceUnitId = entity.Room?.ServiceUnitId ?? Guid.Empty,
                ServiceUnitCode = entity.Room?.ServiceUnit?.ServiceUnitCode ?? string.Empty,
                ServiceUnitName = entity.Room?.ServiceUnit?.ServiceUnitName ?? string.Empty,
                PatientClassId = entity.Room?.PatientClassId,
                PatientClassCode = entity.Room?.PatientClass?.PatientClassCode,
                PatientClassName = entity.Room?.PatientClass?.PatientClassName,
                BedCode = entity.BedCode,
                BedName = entity.BedName,
                BedNumber = entity.BedNumber,
                BedStatus = entity.BedStatus,
                BedStatusName = BuildEnumLabel(entity.BedStatus),
                IsForMale = entity.IsForMale,
                IsForFemale = entity.IsForFemale,
                IsForNewborn = entity.IsForNewborn,
                IsIsolationBed = entity.IsIsolationBed,
                IsIntensiveCareBed = entity.IsIntensiveCareBed,
                IsOdcBed = entity.IsOdcBed,
                IsReservable = entity.IsReservable,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static BedDetailResponse MapDetailResponse(MstBed entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = new BedDetailResponse
            {
                Id = entity.Id,
                RoomId = entity.RoomId,
                RoomCode = entity.Room?.RoomCode ?? string.Empty,
                RoomName = entity.Room?.RoomName ?? string.Empty,
                ServiceUnitId = entity.Room?.ServiceUnitId ?? Guid.Empty,
                ServiceUnitCode = entity.Room?.ServiceUnit?.ServiceUnitCode ?? string.Empty,
                ServiceUnitName = entity.Room?.ServiceUnit?.ServiceUnitName ?? string.Empty,
                PatientClassId = entity.Room?.PatientClassId,
                PatientClassCode = entity.Room?.PatientClass?.PatientClassCode,
                PatientClassName = entity.Room?.PatientClass?.PatientClassName,
                BedCode = entity.BedCode,
                BedName = entity.BedName,
                BedNumber = entity.BedNumber,
                BedStatus = entity.BedStatus,
                BedStatusName = BuildEnumLabel(entity.BedStatus),
                IsForMale = entity.IsForMale,
                IsForFemale = entity.IsForFemale,
                IsForNewborn = entity.IsForNewborn,
                IsIsolationBed = entity.IsIsolationBed,
                IsIntensiveCareBed = entity.IsIntensiveCareBed,
                IsOdcBed = entity.IsOdcBed,
                IsReservable = entity.IsReservable,
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

            return response;
        }

        private static BedOptionResponse MapOptionResponse(MstBed entity)
        {
            return new BedOptionResponse
            {
                Id = entity.Id,
                RoomId = entity.RoomId,
                RoomCode = entity.Room?.RoomCode ?? string.Empty,
                RoomName = entity.Room?.RoomName ?? string.Empty,
                ServiceUnitId = entity.Room?.ServiceUnitId ?? Guid.Empty,
                ServiceUnitCode = entity.Room?.ServiceUnit?.ServiceUnitCode ?? string.Empty,
                ServiceUnitName = entity.Room?.ServiceUnit?.ServiceUnitName ?? string.Empty,
                PatientClassId = entity.Room?.PatientClassId,
                PatientClassCode = entity.Room?.PatientClass?.PatientClassCode,
                PatientClassName = entity.Room?.PatientClass?.PatientClassName,
                BedCode = entity.BedCode,
                BedName = entity.BedName,
                BedNumber = entity.BedNumber,
                BedStatus = entity.BedStatus,
                BedStatusName = BuildEnumLabel(entity.BedStatus),
                IsForMale = entity.IsForMale,
                IsForFemale = entity.IsForFemale,
                IsForNewborn = entity.IsForNewborn,
                IsIsolationBed = entity.IsIsolationBed,
                IsIntensiveCareBed = entity.IsIntensiveCareBed,
                IsOdcBed = entity.IsOdcBed,
                IsReservable = entity.IsReservable,
                SortOrder = entity.SortOrder
            };
        }

        private static BedCreateResponse MapCreateResponse(MstBed entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new BedCreateResponse
            {
                Id = entity.Id,
                RoomId = entity.RoomId,
                BedCode = entity.BedCode,
                BedName = entity.BedName,
                BedNumber = entity.BedNumber,
                BedStatus = entity.BedStatus,
                BedStatusName = BuildEnumLabel(entity.BedStatus),
                IsReservable = entity.IsReservable,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static BedUpdateResponse MapUpdateResponse(MstBed entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new BedUpdateResponse
            {
                Id = entity.Id,
                RoomId = entity.RoomId,
                BedCode = entity.BedCode,
                BedName = entity.BedName,
                BedNumber = entity.BedNumber,
                BedStatus = entity.BedStatus,
                BedStatusName = BuildEnumLabel(entity.BedStatus),
                IsReservable = entity.IsReservable,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> actorNames, Guid actorId)
        {
            if (actorId == Guid.Empty)
            {
                return null;
            }

            return actorNames.TryGetValue(actorId, out var actorName) ? actorName : null;
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
            var today = DateTime.UtcNow.Date;

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

        private static List<BedCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<BedCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false }
            };
        }

        private static List<BedEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new BedEnumOptionResponse
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

        private static List<BedQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<BedQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "date", Description = "Filter tanggal mulai berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "date", Description = "Filter tanggal akhir berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Preset periode. Pilihan: custom, today, last7days, last30days, thismonth, lastmonth.", Example = "last7days" },
                new() { Name = "roomId", Type = "guid", Description = "Filter bed berdasarkan room.", Example = "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                new() { Name = "serviceUnitId", Type = "guid", Description = "Filter bed berdasarkan service unit dari room.", Example = "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                new() { Name = "patientClassId", Type = "guid", Description = "Filter bed berdasarkan patient class dari room.", Example = "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                new() { Name = "bedStatus", Type = "enum", Description = "Filter berdasarkan status bed.", Example = "0" },
                new() { Name = "isActive", Type = "bool", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "search", Type = "string", Description = "Cari berdasarkan kode, nama, nomor bed, room, service unit, patient class, atau deskripsi.", Example = "Bed 101" },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting.", Example = "sortOrder" },
                new() { Name = "sortDirection", Type = "string", Description = "Arah sorting: asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman.", Example = "25" }
            };
        }

        private static List<BedFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: false);
        }

        private static List<BedFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: true);
        }

        private static List<BedFormFieldMetadataResponse> BuildFieldMetadata(bool isUpdate)
        {
            var fields = new List<BedFormFieldMetadataResponse>
            {
                new() { Name = "bedCode", Label = "Kode Bed", Section = "Basic", InputType = "readonly", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "AutoGenerated", MaxLength = 50, Description = "Digenerate otomatis oleh sistem dengan format BD-RSMMC-00001.", Example = "BD-RSMMC-00001", SortOrder = 1 },
                new() { Name = "roomId", Label = "Room", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "/api/v1/health-services/master-data/rooms/options", SortOrder = 2 },
                new() { Name = "bedName", Label = "Nama Bed", Section = "Basic", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 100, Example = "Bed 101-A", SortOrder = 3 },
                new() { Name = "bedNumber", Label = "Nomor Bed", Section = "Basic", InputType = "text", MaxLength = 50, Example = "101-A", SortOrder = 4 },
                new() { Name = "bedStatus", Label = "Status Bed", Section = "Status", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "bedStatusOptions", SortOrder = 5 },
                new() { Name = "isForMale", Label = "Untuk Laki-laki", Section = "Rule", InputType = "switch", SortOrder = 6 },
                new() { Name = "isForFemale", Label = "Untuk Perempuan", Section = "Rule", InputType = "switch", SortOrder = 7 },
                new() { Name = "isForNewborn", Label = "Untuk Bayi Baru Lahir", Section = "Rule", InputType = "switch", SortOrder = 8 },
                new() { Name = "isIsolationBed", Label = "Bed Isolasi", Section = "Rule", InputType = "switch", SortOrder = 9 },
                new() { Name = "isIntensiveCareBed", Label = "Intensive Care", Section = "Rule", InputType = "switch", SortOrder = 10 },
                new() { Name = "isOdcBed", Label = "One Day Care", Section = "Rule", InputType = "switch", SortOrder = 11 },
                new() { Name = "isReservable", Label = "Bisa Reservasi", Section = "Rule", InputType = "switch", SortOrder = 12 },
                new() { Name = "sortOrder", Label = "Urutan", Section = "Display", InputType = "number", SortOrder = 13 },
                new() { Name = "description", Label = "Deskripsi", Section = "Additional", InputType = "textarea", MaxLength = 250, SortOrder = 14 }
            };

            if (isUpdate)
            {
                fields.Add(new BedFormFieldMetadataResponse
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

            return Guid.TryParse(userIdText, out var userId) ? userId : Guid.Empty;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
