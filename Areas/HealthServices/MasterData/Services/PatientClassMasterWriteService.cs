using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
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
using ResponsePatientClassPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.PatientClassResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;

/// <summary>Writer PatientClass existing yang dijalankan di bawah transaksi dan guard BedMutationService.</summary>
public sealed class PatientClassMasterWriteService(ApplicationDbContext dbContext, LoggerService loggerService, IHttpContextAccessor http)
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly LoggerService _loggerService = loggerService;
    private HttpContext HttpContext => http.HttpContext ?? throw new InvalidOperationException("HTTP context tidak tersedia.");
    private ClaimsPrincipal User => HttpContext.User;
    private static IActionResult Ok(object value) => new OkObjectResult(value);
    private static IActionResult BadRequest(object value) => new BadRequestObjectResult(value);
    private static IActionResult NotFound(object value) => new NotFoundObjectResult(value);
    private const string LogCategory = "HealthServices.MasterData";

    internal async Task<IActionResult> UpdatePatientClassAsync(Guid id, UpdatePatientClassRequest request)
        {
            if (_dbContext.Database.CurrentTransaction == null)
                throw new InvalidOperationException("Writer harus dipanggil melalui BedMutationService.");
            var entity = await _dbContext.Set<MstPatientClass>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient class tidak ditemukan."
                ));
            }

            var validation = await ValidateCreateUpdateRequestAsync(id, request, request.IsActive);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data patient class tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await InpBedTransactionLease.BeginAsync(_dbContext, HttpContext.RequestAborted);

            if (request.IsDefault)
            {
                await ResetDefaultPatientClassAsync(actorUserId, now, excludeId: entity.Id);
            }

            var previousClassName = entity.PatientClassName;
            entity.PatientClassName = request.PatientClassName.Trim();

            // BE-RWI-235 (FR-DNH-016, RWI-DEC-322): nama kamar berkunci mengikuti nama kelas, dalam
            // transaksi yang sama. Kamar dan bed kelas ini sudah dikunci BedMutationService; snapshot
            // lokasi penempatan lama tidak ditulis ulang.
            if (!string.Equals(previousClassName, entity.PatientClassName, StringComparison.Ordinal))
            {
                var keyedRooms = await _dbContext.Set<MstRoom>()
                    .Where(x => x.PatientClassId == entity.Id && !x.IsDelete && x.FloorPlanRoomKey != null)
                    .ToListAsync(HttpContext.RequestAborted);
                foreach (var room in keyedRooms)
                {
                    var renamed = FloorPlanRoomNaming.RoomName(entity.PatientClassName, room.RoomNumber, room.RoomName);
                    if (renamed == room.RoomName) continue;
                    room.RoomName = renamed;
                    room.UpdateDateTime = now;
                    room.UpdateBy = actorUserId;
                }
            }
            entity.PatientClassType = request.PatientClassType;
            entity.ExternalClassCode = NormalizeNullableText(request.ExternalClassCode);
            entity.ClassAlias = NormalizeNullableText(request.ClassAlias);
            entity.ClassLevel = request.ClassLevel;
            entity.IsForOutpatient = request.IsForOutpatient;
            entity.IsForInpatient = request.IsForInpatient;
            entity.IsForEmergency = request.IsForEmergency;
            entity.IsForIntensiveCare = request.IsForIntensiveCare;
            entity.IsForNewborn = request.IsForNewborn;
            entity.IsForRoomCharge = request.IsForRoomCharge;
            entity.IsForTariffMapping = request.IsForTariffMapping;
            entity.IsDefault = request.IsActive && request.IsDefault;
            entity.DefaultDailyRoomRate = request.DefaultDailyRoomRate;
            entity.DefaultRegistrationFee = request.DefaultRegistrationFee;
            entity.DefaultConsultationFee = request.DefaultConsultationFee;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = MapUpdateResponse(entity, actorNames);

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientClass.UpdatePatientClass",
                "Mengubah data patient class.",
                result
            );

            return Ok(ApiResponse<PatientClassUpdateResponse>.Ok(
                result,
                "Patient class berhasil diperbarui."
            ));
        }

    internal async Task<IActionResult> UpdatePatientClassStatusAsync(Guid id, UpdatePatientClassStatusRequest request)
        {
            if (_dbContext.Database.CurrentTransaction == null)
                throw new InvalidOperationException("Writer harus dipanggil melalui BedMutationService.");
            var entity = await _dbContext.Set<MstPatientClass>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient class tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            if (!request.IsActive)
            {
                entity.IsDefault = false;
            }

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = MapUpdateResponse(entity, actorNames);

            return Ok(ApiResponse<PatientClassUpdateResponse>.Ok(
                result,
                "Status patient class berhasil diperbarui."
            ));
        }

    internal async Task<IActionResult> DeletePatientClassAsync(Guid id, DeletePatientClassRequest? request = null)
        {
            if (_dbContext.Database.CurrentTransaction == null)
                throw new InvalidOperationException("Writer harus dipanggil melalui BedMutationService.");
            var entity = await _dbContext.Set<MstPatientClass>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient class tidak ditemukan."
                ));
            }

            if (entity.IsDefault)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Patient class default tidak dapat dihapus."
                ));
            }

            var isUsedByRoom = await _dbContext.Set<MstRoom>()
                .AnyAsync(x => x.PatientClassId == id && !x.IsDelete);

            var isUsedByTariff = await _dbContext.Set<MstTariff>()
                .AnyAsync(x => x.PatientClassId == id && !x.IsDelete);

            if (isUsedByRoom || isUsedByTariff)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Patient class tidak dapat dihapus karena sudah digunakan oleh room atau tariff."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsDefault = false;
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
            var result = new PatientClassDeleteResponse
            {
                Id = entity.Id,
                PatientClassCode = entity.PatientClassCode,
                PatientClassName = entity.PatientClassName,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : (Guid?)entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientClass.DeletePatientClass",
                "Menghapus data patient class.",
                result
            );

            return Ok(ApiResponse<PatientClassDeleteResponse>.Ok(
                result,
                "Patient class berhasil dihapus."
            ));
        }

    private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateUpdateRequestAsync(
            Guid? excludeId,
            CreatePatientClassRequest request,
            bool isActive)
        {
            if (string.IsNullOrWhiteSpace(request.PatientClassName))
                return (false, "Nama patient class wajib diisi.");

            if (request.ClassLevel < 0)
                return (false, "Level kelas tidak boleh kurang dari 0.");

            if (request.IsDefault && !isActive)
                return (false, "Patient class default harus dalam status aktif.");

            if (request.DefaultDailyRoomRate.HasValue && request.DefaultDailyRoomRate.Value < 0)
                return (false, "Default daily room rate tidak boleh kurang dari 0.");

            if (request.DefaultRegistrationFee.HasValue && request.DefaultRegistrationFee.Value < 0)
                return (false, "Default registration fee tidak boleh kurang dari 0.");

            if (request.DefaultConsultationFee.HasValue && request.DefaultConsultationFee.Value < 0)
                return (false, "Default consultation fee tidak boleh kurang dari 0.");

            var normalizedName = request.PatientClassName.Trim().ToLower();

            var duplicateNameQuery = _dbContext.Set<MstPatientClass>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.PatientClassName.ToLower() == normalizedName);

            if (excludeId.HasValue)
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateNameQuery.AnyAsync())
                return (false, "Nama patient class sudah digunakan.");

            var externalClassCode = NormalizeNullableText(request.ExternalClassCode);
            if (!string.IsNullOrWhiteSpace(externalClassCode))
            {
                var normalizedExternalCode = externalClassCode.ToLower();

                var duplicateExternalCodeQuery = _dbContext.Set<MstPatientClass>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.ExternalClassCode != null &&
                        x.ExternalClassCode.ToLower() == normalizedExternalCode);

                if (excludeId.HasValue)
                    duplicateExternalCodeQuery = duplicateExternalCodeQuery.Where(x => x.Id != excludeId.Value);

                if (await duplicateExternalCodeQuery.AnyAsync())
                    return (false, "External class code sudah digunakan.");
            }

            return (true, null);
        }

    private async Task ResetDefaultPatientClassAsync(Guid actorUserId, DateTime now, Guid? excludeId = null)
        {
            var defaultClasses = await _dbContext.Set<MstPatientClass>()
                .Where(x =>
                    x.IsDefault &&
                    !x.IsDelete &&
                    (!excludeId.HasValue || x.Id != excludeId.Value))
                .ToListAsync();

            foreach (var item in defaultClasses)
            {
                item.IsDefault = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
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

    private static PatientClassUpdateResponse MapUpdateResponse(MstPatientClass entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new PatientClassUpdateResponse
            {
                Id = entity.Id,
                PatientClassCode = entity.PatientClassCode,
                PatientClassName = entity.PatientClassName,
                PatientClassType = entity.PatientClassType,
                PatientClassTypeName = BuildEnumLabel(entity.PatientClassType),
                IsDefault = entity.IsDefault,
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

            return Guid.TryParse(userIdText, out var userId) ? userId : Guid.Empty;
        }

    private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

}
