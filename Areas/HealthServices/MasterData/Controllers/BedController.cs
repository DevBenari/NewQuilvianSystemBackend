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
        [AccessAction("Read", "Read Bed", Description = "Melihat data bed", AccessType = AccessTypes.Read, SortOrder = 1)]
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
                    new() { Value = "isReservable", Label = "Bisa reservasi" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                BedStatusOptions = BuildEnumOptions<BedStatus>(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata()
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
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? roomId,
            [FromQuery] Guid? serviceUnitId,
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

            var query = _dbContext.Set<MstBed>()
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

            if (roomId.HasValue && roomId.Value != Guid.Empty)
                query = query.Where(x => x.RoomId == roomId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.Room != null && x.Room.ServiceUnitId == serviceUnitId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

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

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BedResponse
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
                })
                .ToListAsync();

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
    [FromQuery] bool onlyActive = true,
    [FromQuery] string? search = null,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstBed>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (roomId.HasValue && roomId.Value != Guid.Empty)
                query = query.Where(x => x.RoomId == roomId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.Room != null && x.Room.ServiceUnitId == serviceUnitId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.BedCode.ToLower().Contains(keyword) ||
                    x.BedName.ToLower().Contains(keyword) ||
                    (x.BedNumber != null && x.BedNumber.ToLower().Contains(keyword)) ||
                    (x.Room != null && x.Room.RoomName.ToLower().Contains(keyword)) ||
                    (x.Room != null && x.Room.RoomCode.ToLower().Contains(keyword)) ||
                    (x.Room != null && x.Room.ServiceUnit != null && x.Room.ServiceUnit.ServiceUnitName.ToLower().Contains(keyword)) ||
                    (x.Room != null && x.Room.PatientClass != null && x.Room.PatientClass.PatientClassName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.BedName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BedOptionResponse
                {
                    Id = x.Id,
                    RoomId = x.RoomId,
                    RoomName = x.Room != null ? x.Room.RoomName : string.Empty,

                    ServiceUnitId = x.Room != null ? x.Room.ServiceUnitId : Guid.Empty,
                    ServiceUnitName = x.Room != null && x.Room.ServiceUnit != null
                        ? x.Room.ServiceUnit.ServiceUnitName
                        : string.Empty,

                    PatientClassId = x.Room != null ? x.Room.PatientClassId : null,
                    PatientClassName = x.Room != null && x.Room.PatientClass != null
                        ? x.Room.PatientClass.PatientClassName
                        : null,

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
            var data = await _dbContext.Set<MstBed>()
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

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Bed tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<BedDetailResponse>.Ok(
                data,
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

            await _loggerService.InfoAsync(
                LogCategory,
                "Bed.CreateBed",
                "Membuat data bed.",
                response
            );

            return Ok(ApiResponse<BedCreateResponse>.Ok(
                response,
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

            var response = new BedUpdateResponse
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

            return Ok(ApiResponse<BedUpdateResponse>.Ok(
                response,
                "Bed berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
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

        private async Task<string> GenerateBedCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstBed>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.BedCode.StartsWith(BedCodePrefix))
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

            return $"{BedCodePrefix}{nextNumber.ToString().PadLeft(BedCodeDigitLength, '0')}";
        }

        private static int? ExtractBedCodeNumber(string bedCode)
        {
            if (string.IsNullOrWhiteSpace(bedCode))
                return null;

            if (!bedCode.StartsWith(BedCodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var numberText = bedCode[BedCodePrefix.Length..];

            return int.TryParse(numberText, out var number)
                ? number
                : null;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateUpdateRequestAsync(
            Guid? excludeId,
            Guid roomId,
            string bedName,
            string? bedNumber)
        {
            if (roomId == Guid.Empty)
                return (false, "Room wajib dipilih.");

            if (string.IsNullOrWhiteSpace(bedName))
                return (false, "Nama bed wajib diisi.");

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

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateGeneratedBedCodeAsync(string bedCode)
        {
            if (string.IsNullOrWhiteSpace(bedCode))
                return (false, "Kode bed otomatis gagal dibuat.");

            var normalizedCode = bedCode.Trim().ToUpperInvariant();

            var duplicateCode = await _dbContext.Set<MstBed>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.BedCode.ToUpper() == normalizedCode);

            if (duplicateCode)
                return (false, "Kode bed otomatis sudah digunakan. Silakan ulangi proses create.");

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

        private static List<BedCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<BedCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false }
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
                    Label = SplitPascalCase(x.ToString())
                })
                .ToList();
        }

        private static string SplitPascalCase(string value)
        {
            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private static List<BedQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<BedQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "date", Description = "Filter tanggal mulai berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "date", Description = "Filter tanggal akhir berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Preset periode. Pilihan: custom, today, last7days, last30days, thismonth.", Example = "last7days" },
                new() { Name = "roomId", Type = "guid", Description = "Relasi table 1. Filter bed berdasarkan room.", Example = "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                new() { Name = "serviceUnitId", Type = "guid", Description = "Relasi table 2. Filter bed berdasarkan service unit dari room.", Example = "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                new() { Name = "isActive", Type = "bool", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "search", Type = "string", Description = "Cari berdasarkan kode bed, nama bed, nomor bed, room, service unit, atau patient class.", Example = "Bed 101" },
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
                new() { Name = "bedCode", Label = "Kode Bed", Section = "Basic", InputType = "readonly", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "AutoGenerated", MaxLength = 50, Description = "Digenerate otomatis oleh sistem dengan format BD-RSMMC-00001. Nomor terkecil yang kosong dari data aktif akan dipakai kembali.", Example = "BD-RSMMC-00001", SortOrder = 1 },
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
