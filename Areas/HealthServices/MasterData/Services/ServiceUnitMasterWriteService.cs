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
using System.Globalization;
using System.Security.Claims;
using ResponseServiceUnitPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.ServiceUnitResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;

/// <summary>Writer ServiceUnit existing yang dijalankan di bawah transaksi dan guard BedMutationService.</summary>
public sealed class ServiceUnitMasterWriteService(ApplicationDbContext dbContext, LoggerService loggerService, IHttpContextAccessor http)
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly LoggerService _loggerService = loggerService;
    private HttpContext HttpContext => http.HttpContext ?? throw new InvalidOperationException("HTTP context tidak tersedia.");
    private ClaimsPrincipal User => HttpContext.User;
    private static IActionResult Ok(object value) => new OkObjectResult(value);
    private static IActionResult BadRequest(object value) => new BadRequestObjectResult(value);
    private static IActionResult NotFound(object value) => new NotFoundObjectResult(value);
    private const string LogCategory = "HealthServices.MasterData";

    internal async Task<IActionResult> UpdateServiceUnitAsync(
            Guid id,
            UpdateServiceUnitRequest request)
        {
            if (_dbContext.Database.CurrentTransaction == null)
                throw new InvalidOperationException("Writer harus dipanggil melalui BedMutationService.");
            var entity = await _dbContext.Set<MstServiceUnit>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Service unit tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                request: request
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data service unit tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.ServiceUnitName = request.ServiceUnitName.Trim();
            entity.ServiceUnitType = request.ServiceUnitType;
            entity.ShortName = NormalizeNullableText(request.ShortName);
            entity.LocationName = NormalizeNullableText(request.LocationName);
            entity.FloorName = NormalizeNullableText(request.FloorName);
            entity.IsAvailableForRegistration = request.IsAvailableForRegistration;
            entity.IsAvailableForKiosk = request.IsAvailableForKiosk;
            entity.IsAvailableForAppointment = request.IsAvailableForAppointment;
            entity.IsAvailableForBloodOrder = request.IsAvailableForBloodOrder;
            entity.IsQueueRequired = request.IsQueueRequired;
            entity.IsDoctorRequired = request.IsDoctorRequired;
            entity.IsScreeningRequired = request.IsScreeningRequired;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });

            var result = new ServiceUnitUpdateResponse
            {
                Id = entity.Id,
                ServiceUnitCode = entity.ServiceUnitCode,
                ServiceUnitName = entity.ServiceUnitName,
                ServiceUnitType = entity.ServiceUnitType,
                ServiceUnitTypeName = BuildServiceUnitTypeLabel(entity.ServiceUnitType),
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "ServiceUnit.UpdateServiceUnit",
                "Mengubah data service unit.",
                result
            );

            return Ok(ApiResponse<ServiceUnitUpdateResponse>.Ok(
                result,
                "Service unit berhasil diperbarui."
            ));
        }

    internal async Task<IActionResult> UpdateServiceUnitStatusAsync(
            Guid id,
            UpdateServiceUnitStatusRequest request)
        {
            if (_dbContext.Database.CurrentTransaction == null)
                throw new InvalidOperationException("Writer harus dipanggil melalui BedMutationService.");
            var entity = await _dbContext.Set<MstServiceUnit>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Service unit tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });

            var result = new ServiceUnitUpdateResponse
            {
                Id = entity.Id,
                ServiceUnitCode = entity.ServiceUnitCode,
                ServiceUnitName = entity.ServiceUnitName,
                ServiceUnitType = entity.ServiceUnitType,
                ServiceUnitTypeName = BuildServiceUnitTypeLabel(entity.ServiceUnitType),
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            return Ok(ApiResponse<ServiceUnitUpdateResponse>.Ok(
                result,
                "Status service unit berhasil diperbarui."
            ));
        }

    internal async Task<IActionResult> DeleteServiceUnitAsync(
            Guid id,
            DeleteServiceUnitRequest? request = null)
        {
            if (_dbContext.Database.CurrentTransaction == null)
                throw new InvalidOperationException("Writer harus dipanggil melalui BedMutationService.");
            var entity = await _dbContext.Set<MstServiceUnit>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Service unit tidak ditemukan."
                ));
            }

            var isUsedByClinic = await _dbContext.Set<MstClinic>()
                .AnyAsync(x => x.ServiceUnitId == id && !x.IsDelete);

            if (isUsedByClinic)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Service unit tidak dapat dihapus karena sudah digunakan oleh clinic."
                ));
            }

            var isUsedByRoom = await _dbContext.Set<MstRoom>()
                .AsNoTracking()
                .AnyAsync(x => x.ServiceUnitId == id && !x.IsDelete);

            if (isUsedByRoom)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Service unit tidak dapat dihapus karena sudah digunakan oleh room."
                ));
            }

            var isUsedByDoctorSchedule = await _dbContext.Set<MstDoctorSchedule>()
                .AsNoTracking()
                .AnyAsync(x => x.ServiceUnitId == id && !x.IsDelete);

            if (isUsedByDoctorSchedule)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Service unit tidak dapat dihapus karena sudah digunakan oleh jadwal dokter."
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

            var result = new ServiceUnitDeleteResponse
            {
                Id = entity.Id,
                ServiceUnitCode = entity.ServiceUnitCode,
                ServiceUnitName = entity.ServiceUnitName,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : (Guid?)entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "ServiceUnit.DeleteServiceUnit",
                "Menghapus data service unit.",
                result
            );

            return Ok(ApiResponse<ServiceUnitDeleteResponse>.Ok(
                result,
                "Service unit berhasil dihapus."
            ));
        }

    private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateServiceUnitRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ServiceUnitName))
            {
                return (false, "Nama service unit wajib diisi.");
            }

            if (!Enum.IsDefined(typeof(ServiceUnitType), request.ServiceUnitType))
            {
                return (false, "Tipe service unit tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (request.SortOrder < 0)
            {
                return (false, "Urutan tidak boleh kurang dari 0.");
            }

            var normalizedName = request.ServiceUnitName.Trim().ToLower();

            var duplicateNameQuery = _dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ServiceUnitName.ToLower() == normalizedName);

            if (excludeId.HasValue)
            {
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateNameQuery.AnyAsync())
            {
                return (false, "Nama service unit sudah digunakan.");
            }

            var shortName = NormalizeNullableText(request.ShortName);

            if (!string.IsNullOrWhiteSpace(shortName))
            {
                var normalizedShortName = shortName.ToLower();

                var duplicateShortNameQuery = _dbContext.Set<MstServiceUnit>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.ShortName != null &&
                        x.ShortName.ToLower() == normalizedShortName);

                if (excludeId.HasValue)
                {
                    duplicateShortNameQuery = duplicateShortNameQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicateShortNameQuery.AnyAsync())
                {
                    return (false, "Nama singkat service unit sudah digunakan.");
                }
            }

            return (true, null);
        }

    private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(
            IEnumerable<Guid> actorIds)
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

    private static string BuildServiceUnitTypeLabel(ServiceUnitType value)
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

}
