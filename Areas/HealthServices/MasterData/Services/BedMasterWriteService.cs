using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Security.Claims;
using ResponseBedPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.BedResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;

/// <summary>Writer Bed existing yang dijalankan di bawah transaksi dan guard BedMutationService.</summary>
public sealed class BedMasterWriteService(ApplicationDbContext dbContext, LoggerService loggerService, IHttpContextAccessor http)
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly LoggerService _loggerService = loggerService;
    private HttpContext HttpContext => http.HttpContext ?? throw new InvalidOperationException("HTTP context tidak tersedia.");
    private ClaimsPrincipal User => HttpContext.User;
    private static IActionResult Ok(object value) => new OkObjectResult(value);
    private static IActionResult BadRequest(object value) => new BadRequestObjectResult(value);
    private static IActionResult NotFound(object value) => new NotFoundObjectResult(value);
    private static IActionResult UnprocessableEntity(object value) => new UnprocessableEntityObjectResult(value);
    private const string LogCategory = "HealthServices.MasterData";

    internal async Task<IActionResult> CreateBedAsync(CreateBedRequest request, string bedCode)
        {
            if (_dbContext.Database.CurrentTransaction == null)
                throw new InvalidOperationException("Writer harus dipanggil melalui BedMutationService.");

            // BE-RWI-235: kamar berkunci denah → label "<nomor kamar>-<huruf berikutnya>" dibentuk server,
            // unik per lantai, dengan penanda turunan kelas (backend 15.6).
            var ct = HttpContext.RequestAborted;
            var keyedRoom = await LoadKeyedRoomAsync(request.RoomId, ct);
            var bedName = request.BedName;
            var bedNumber = request.BedNumber;
            if (keyedRoom != null)
            {
                await BedMutationService.LockFloorBedLabelsAsync(_dbContext, keyedRoom.FloorCode!, ct);
                bedNumber = FloorPlanRoomNaming.NextBedLetter(
                    await BedMutationService.UsedBedLettersAsync(_dbContext, keyedRoom.FloorCode!, keyedRoom.RoomNumber!, ct));
                bedName = FloorPlanRoomNaming.BedLabel(keyedRoom.RoomNumber!, bedNumber);
                await EnsureBedLabelUniqueOnFloorAsync(keyedRoom.FloorCode!, bedName, excludeBedId: null, ct);
            }

            var validation = await ValidateCreateUpdateRequestAsync(
                excludeId: null,
                roomId: request.RoomId,
                bedName: bedName,
                bedNumber: bedNumber
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

            await using var transaction = await InpBedTransactionLease.BeginAsync(_dbContext, HttpContext.RequestAborted);

            var codeValidation = await ValidateBedCodeAsync(bedCode);

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
                BedCode = bedCode,
                BedName = bedName.Trim(),
                BedNumber = NormalizeNullableText(bedNumber),
                BedStatus = request.BedStatus,
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

            if (keyedRoom != null)
            {
                // Boks rawat gabung dikirim eksplisit IsForNewborn = true; sisanya turunan kelas kamar.
                FloorPlanRoomNaming.ApplyBedFlags(entity, keyedRoom.PatientClass?.PatientClassType, isCrib: request.IsForNewborn);
            }

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

    internal async Task<IActionResult> UpdateBedAsync(Guid id, UpdateBedRequest request)
        {
            if (_dbContext.Database.CurrentTransaction == null)
                throw new InvalidOperationException("Writer harus dipanggil melalui BedMutationService.");
            var entity = await _dbContext.Set<MstBed>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Bed tidak ditemukan."
                ));
            }

            // BE-RWI-235: ubah label bed pada kamar berkunci; label unik per lantai, huruf bed mengikuti
            // label berpola "<nomor kamar>-<huruf>".
            var ct = HttpContext.RequestAborted;
            var bedNumber = request.BedNumber;
            var keyedRoom = await LoadKeyedRoomAsync(request.RoomId, ct);
            if (keyedRoom != null && !string.IsNullOrWhiteSpace(request.BedName))
            {
                var label = request.BedName.Trim();
                if (entity.RoomId != request.RoomId || !string.Equals(entity.BedName, label, StringComparison.OrdinalIgnoreCase))
                {
                    await BedMutationService.LockFloorBedLabelsAsync(_dbContext, keyedRoom.FloorCode!, ct);
                    await EnsureBedLabelUniqueOnFloorAsync(keyedRoom.FloorCode!, label, excludeBedId: id, ct);
                }
                bedNumber = FloorPlanRoomNaming.LetterFromLabel(label, keyedRoom.RoomNumber) ?? bedNumber;
            }

            var validation = await ValidateCreateUpdateRequestAsync(
                excludeId: id,
                roomId: request.RoomId,
                bedName: request.BedName,
                bedNumber: bedNumber
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
            entity.BedNumber = NormalizeNullableText(bedNumber);
            entity.BedStatus = request.BedStatus;
            entity.IsForNewborn = request.IsForNewborn;
            entity.IsIsolationBed = request.IsIsolationBed;
            entity.IsIntensiveCareBed = request.IsIntensiveCareBed;
            entity.IsOdcBed = request.IsOdcBed;
            entity.IsReservable = request.IsReservable;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive ?? entity.IsActive;
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

    internal async Task<IActionResult> UpdateBedStatusAsync(Guid id, UpdateBedStatusRequest request)
        {
            if (_dbContext.Database.CurrentTransaction == null)
                throw new InvalidOperationException("Writer harus dipanggil melalui BedMutationService.");
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

    internal async Task<IActionResult> UpdateBedAvailabilityAsync(Guid id, UpdateBedAvailabilityRequest request)
        {
            if (_dbContext.Database.CurrentTransaction == null)
                throw new InvalidOperationException("Writer harus dipanggil melalui BedMutationService.");
            var entity = await _dbContext.Set<MstBed>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Bed tidak ditemukan."
                ));
            }

            // RWI-RULE-027 aturan 4. Pesannya sama persis dengan validation matrix bagian 10.
            if (request.BedStatus == BedStatus.Reserved || request.BedStatus == BedStatus.Occupied)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Status Terisi dan Dipesan hanya dapat diubah lewat modul Rawat Inap. " +
                    "Untuk menutup tempat tidur sementara, pakai status Pembersihan, Perbaikan, atau Diblokir."
                ));
            }

            if (request.BedStatus == BedStatus.Unknown)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Status ketersediaan tempat tidur tidak dikenali."
                ));
            }

            // Tempat tidur yang sedang dipakai tidak boleh ditutup dari layar master. Menutupnya
            // tidak memindahkan pasien ke mana pun — ia hanya membuat salinan status berbohong,
            // dan pasien yang masih berbaring di sana menghilang dari papan tempat tidur.
            var adaPenempatanAktif = await _dbContext.Set<InpBedPlacement>()
                .AsNoTracking()
                .AnyAsync(x => x.BedId == id && x.EndDateTime == null && !x.IsDelete);

            if (adaPenempatanAktif)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Tempat tidur ini sedang ditempati pasien rawat inap. " +
                    "Statusnya baru dapat diubah setelah pasien dipindahkan atau kepergiannya dicatat."
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

    internal async Task<IActionResult> DeleteBedAsync(Guid id, DeleteBedRequest? request = null)
        {
            if (_dbContext.Database.CurrentTransaction == null)
                throw new InvalidOperationException("Writer harus dipanggil melalui BedMutationService.");
            var entity = await _dbContext.Set<MstBed>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Bed tidak ditemukan."
                ));
            }

            // BE-RWI-235 (VAL-DNH-10, DNH-MIS-07): bed hanya boleh dihapus bila tidak pernah dirujuk
            // pemesanan maupun penempatan dalam status apa pun; bed beriwayat dinonaktifkan saja.
            var hasUsageHistory =
                await _dbContext.Set<InpBedPlacement>().IgnoreQueryFilters().AsNoTracking().AnyAsync(x => x.BedId == id) ||
                await _dbContext.Set<InpBedReservation>().IgnoreQueryFilters().AsNoTracking().AnyAsync(x => x.BedId == id);

            if (hasUsageHistory)
            {
                throw new InpBedOperationException(422, "BED_HAS_USAGE_HISTORY",
                    "Bed ini pernah dipesan atau ditempati, sehingga tidak dapat dihapus. Nonaktifkan bed agar riwayatnya tetap tersimpan.");
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

    /// <summary>Kamar berkunci denah (berlantai dan bernomor) beserta kelasnya, atau <c>null</c> untuk kamar di luar denah.</summary>
    private async Task<MstRoom?> LoadKeyedRoomAsync(Guid roomId, CancellationToken ct)
    {
        if (roomId == Guid.Empty) return null;
        var room = await _dbContext.Set<MstRoom>().AsNoTracking()
            .Include(x => x.PatientClass)
            .FirstOrDefaultAsync(x => x.Id == roomId && !x.IsDelete, ct);
        return room is { FloorPlanRoomKey: not null, FloorCode: not null } && !string.IsNullOrWhiteSpace(room.RoomNumber) ? room : null;
    }

    /// <summary>INV-DNH-02: label bed unik di antara bed yang belum dihapus pada lantai yang sama.</summary>
    /// <example>Bed "329-C" sudah ada di Lantai 3 → menambah/mengubah label lain menjadi "329-C" ditolak 409.</example>
    private async Task EnsureBedLabelUniqueOnFloorAsync(string floorCode, string label, Guid? excludeBedId, CancellationToken ct)
    {
        var labels = await BedMutationService.FloorBedLabelsAsync(_dbContext, floorCode, excludeBedId, ct);
        if (labels.Contains(label.Trim().ToLowerInvariant()))
            throw new InpBedOperationException(409, "BED_LABEL_DUPLICATE",
                $"Label bed {label.Trim()} sudah dipakai bed lain di lantai yang sama. Pakai label lain.");
    }

    private async Task<(bool IsValid, string? ErrorMessage)> ValidateBedCodeAsync(string bedCode)
        {
            if (string.IsNullOrWhiteSpace(bedCode))
                return (false, "Kode bed otomatis gagal dibuat.");

            var normalizedCode = bedCode.Trim().ToUpperInvariant();

            var duplicateCode = await _dbContext.Set<MstBed>()
                .IgnoreQueryFilters()
                .AnyAsync(x => x.BedCode.ToUpper() == normalizedCode);

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
                IsForNewborn = entity.IsForNewborn,
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
