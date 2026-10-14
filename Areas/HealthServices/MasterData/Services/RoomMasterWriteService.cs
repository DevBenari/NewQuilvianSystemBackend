using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
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

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;

/// <summary>Writer Room existing yang dijalankan di bawah transaksi dan guard BedMutationService.</summary>
public sealed class RoomMasterWriteService(ApplicationDbContext dbContext, LoggerService loggerService, IHttpContextAccessor http)
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly LoggerService _loggerService = loggerService;
    private HttpContext HttpContext => http.HttpContext ?? throw new InvalidOperationException("HTTP context tidak tersedia.");
    private ClaimsPrincipal User => HttpContext.User;
    private static IActionResult Ok(object value) => new OkObjectResult(value);
    private static IActionResult BadRequest(object value) => new BadRequestObjectResult(value);
    private static IActionResult NotFound(object value) => new NotFoundObjectResult(value);
    private const string LogCategory = "HealthServices.MasterData";

    internal async Task<IActionResult> UpdateRoomAsync(Guid id, UpdateRoomRequest request)
        {
            if (_dbContext.Database.CurrentTransaction == null)
                throw new InvalidOperationException("Writer harus dipanggil melalui BedMutationService.");
            var entity = await _dbContext.Set<MstRoom>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Room tidak ditemukan."
                ));
            }

            if (entity.ServiceUnitId != request.ServiceUnitId)
            {
                var isUsedByDoctorSchedule = await _dbContext.Set<MstDoctorSchedule>()
                    .AsNoTracking()
                    .AnyAsync(x => x.RoomId == id && !x.IsDelete);

                var isUsedByEncounter = await _dbContext.Set<RegPatientEncounter>()
                    .AsNoTracking()
                    .AnyAsync(x => x.RoomId == id && !x.IsDelete);

                if (isUsedByDoctorSchedule || isUsedByEncounter)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Service unit room tidak dapat diubah karena room sudah digunakan oleh jadwal dokter atau encounter pasien."
                    ));
                }
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

    internal async Task<IActionResult> UpdateRoomStatusAsync(Guid id, UpdateRoomStatusRequest request)
        {
            if (_dbContext.Database.CurrentTransaction == null)
                throw new InvalidOperationException("Writer harus dipanggil melalui BedMutationService.");
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

    internal async Task<IActionResult> DeleteRoomAsync(Guid id, DeleteRoomRequest? request = null)
        {
            if (_dbContext.Database.CurrentTransaction == null)
                throw new InvalidOperationException("Writer harus dipanggil melalui BedMutationService.");
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

            var isUsedByDoctorSchedule = await _dbContext.Set<MstDoctorSchedule>()
                .AsNoTracking()
                .AnyAsync(x => x.RoomId == id && !x.IsDelete);

            if (isUsedByDoctorSchedule)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Room tidak dapat dihapus karena sudah digunakan oleh jadwal dokter."
                ));
            }

            var isUsedByEncounter = await _dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .AnyAsync(x => x.RoomId == id && !x.IsDelete);

            if (isUsedByEncounter)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Room tidak dapat dihapus karena sudah digunakan oleh encounter pasien."
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

}
