using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Net;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Services
{
    /// <summary>
    /// Daftar mesin absen yang dikelola Petugas IT (HRD-DEC-212). Mesin yang tersimpan aktif — baik baru didaftarkan,
    /// diubah identitasnya, maupun diaktifkan kembali — melepas scan "Menunggu Pendaftaran Mesin" yang cocok sesudah commit
    /// (INT-HR-103). Kegagalan pelepasan tidak membatalkan penyimpanan mesin.
    /// </summary>
    public class AttendanceDeviceService
    {
        private const string IdentityConflictMessage = "Identitas mesin sudah dipakai mesin lain";

        private static readonly (string Value, string Label)[] DeviceTypes =
        {
            ("Fingerprint", "Sidik jari"),
            ("FaceRecognition", "Pengenalan wajah"),
            ("CardReader", "Pembaca kartu"),
            ("QR", "Kode QR"),
            ("Mobile", "Perangkat mobile"),
            ("Integration", "Integrasi sistem")
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly AttendanceRawLogService _rawLogService;
        private readonly ILogger<AttendanceDeviceService> _logger;

        public AttendanceDeviceService(
            ApplicationDbContext dbContext,
            AttendanceRawLogService rawLogService,
            ILogger<AttendanceDeviceService> logger)
        {
            _dbContext = dbContext;
            _rawLogService = rawLogService;
            _logger = logger;
        }

        public AttendanceDeviceFilterMetadataResponse GetFilterMetadata() => new()
        {
            DeviceTypeOptions = DeviceTypes.Select(x => new AttendanceDeviceOptionResponse { Value = x.Value, Label = x.Label }).ToList(),
            SortOptions = new List<AttendanceDeviceOptionResponse>
            {
                new() { Value = "attendanceDeviceCode", Label = "Kode mesin" },
                new() { Value = "attendanceDeviceName", Label = "Nama mesin" },
                new() { Value = "deviceType", Label = "Jenis mesin" },
                new() { Value = "lastSyncAt", Label = "Sinkron terakhir" },
                new() { Value = "createDateTime", Label = "Tanggal dibuat" }
            },
            SortDirections = new List<string> { "asc", "desc" },
            PageSizeOptions = new List<int> { 10, 25, 50, 100 }
        };

        public async Task<PagedResult<AttendanceDeviceResponse>> GetPagedAsync(
            AttendanceDeviceQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);
            var query = BaseQuery();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.AttendanceDeviceCode.ToLower().Contains(keyword) ||
                    x.AttendanceDeviceName.ToLower().Contains(keyword) ||
                    (x.SerialNumber != null && x.SerialNumber.ToLower().Contains(keyword)) ||
                    (x.ExternalDeviceId != null && x.ExternalDeviceId.ToLower().Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(request.DeviceType)) query = query.Where(x => x.DeviceType == request.DeviceType.Trim());
            if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
            if (request.HospitalSiteId.HasValue) query = query.Where(x => x.HospitalSiteId == request.HospitalSiteId.Value);
            if (request.AttendanceLocationId.HasValue) query = query.Where(x => x.AttendanceLocationId == request.AttendanceLocationId.Value);

            var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "attendancedevicename" => descending ? query.OrderByDescending(x => x.AttendanceDeviceName) : query.OrderBy(x => x.AttendanceDeviceName),
                "devicetype" => descending ? query.OrderByDescending(x => x.DeviceType).ThenBy(x => x.AttendanceDeviceCode) : query.OrderBy(x => x.DeviceType).ThenBy(x => x.AttendanceDeviceCode),
                "lastsyncat" => descending ? query.OrderByDescending(x => x.LastSyncAt) : query.OrderBy(x => x.LastSyncAt),
                "createdatetime" => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => descending ? query.OrderByDescending(x => x.AttendanceDeviceCode) : query.OrderBy(x => x.AttendanceDeviceCode)
            };

            var totalData = await query.CountAsync(cancellationToken);
            var rows = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            var items = await MapAsync(rows, cancellationToken);
            return new PagedResult<AttendanceDeviceResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>Identitas mesin yang mengirim scan tertahan, dikelompokkan per identitas (HRD-DEC-212, HRD-ASM-81).</summary>
        public async Task<List<UnregisteredDeviceResponse>> GetUnregisteredAsync(CancellationToken cancellationToken = default)
        {
            var awaiting = AttendanceValueConstants.RawLogProcessingStatus.AwaitingDeviceRegistration;
            var groups = await _dbContext.Set<HrdAttendanceRawLog>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.ProcessingStatus == awaiting && x.ExternalDeviceId != null)
                .GroupBy(x => x.ExternalDeviceId!)
                .Select(g => new UnregisteredDeviceResponse
                {
                    ExternalDeviceId = g.Key,
                    HeldScanCount = g.Count(),
                    FirstEventAt = g.Min(x => x.EventAt),
                    LastEventAt = g.Max(x => x.EventAt),
                    LastReceivedAt = g.Max(x => x.ReceivedAt)
                })
                .ToListAsync(cancellationToken);

            if (groups.Count > 0)
            {
                var keys = groups.Select(x => x.ExternalDeviceId).ToList();
                var inactive = await _dbContext.Set<MstAttendanceDevice>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && !x.IsActive &&
                        (keys.Contains(x.AttendanceDeviceCode) ||
                         (x.ExternalDeviceId != null && keys.Contains(x.ExternalDeviceId)) ||
                         (x.SerialNumber != null && keys.Contains(x.SerialNumber))))
                    .Select(x => new { x.Id, x.AttendanceDeviceCode, x.ExternalDeviceId, x.SerialNumber })
                    .ToListAsync(cancellationToken);
                foreach (var group in groups)
                {
                    var match = inactive.FirstOrDefault(x =>
                        x.AttendanceDeviceCode == group.ExternalDeviceId || x.ExternalDeviceId == group.ExternalDeviceId || x.SerialNumber == group.ExternalDeviceId);
                    group.InactiveAttendanceDeviceId = match?.Id;
                    group.InactiveAttendanceDeviceCode = match?.AttendanceDeviceCode;
                }
            }

            return groups.OrderByDescending(x => x.LastReceivedAt).ToList();
        }

        public async Task<AttendanceDeviceResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            return entity == null ? null : (await MapAsync(new List<MstAttendanceDevice> { entity }, cancellationToken)).Single();
        }

        public async Task<AttendanceDeviceServiceResult<AttendanceDeviceResponse>> CreateAsync(
            CreateAttendanceDeviceRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var input = Normalize(request);
            var error = await ValidateAsync(input, null, request.IsActive, cancellationToken);
            if (error != null) return AttendanceDeviceServiceResult<AttendanceDeviceResponse>.Fail(error.Value.Status, error.Value.Message);

            var now = DateTime.UtcNow;
            var entity = new MstAttendanceDevice
            {
                Id = Guid.NewGuid(),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };
            Apply(entity, input);
            _dbContext.Set<MstAttendanceDevice>().Add(entity);

            var saveError = await SaveAsync(cancellationToken);
            if (saveError != null) return AttendanceDeviceServiceResult<AttendanceDeviceResponse>.Fail(StatusCodes.Status409Conflict, saveError);

            return await CompleteWriteAsync(entity.Id, entity.IsActive, actorUserId, "Mesin absen berhasil didaftarkan.", cancellationToken);
        }

        public async Task<AttendanceDeviceServiceResult<AttendanceDeviceResponse>> UpdateAsync(
            Guid id,
            UpdateAttendanceDeviceRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstAttendanceDevice>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null) return AttendanceDeviceServiceResult<AttendanceDeviceResponse>.Fail(StatusCodes.Status404NotFound, "Mesin absen tidak ditemukan.");

            var input = Normalize(request);
            var error = await ValidateAsync(input, id, entity.IsActive, cancellationToken);
            if (error != null) return AttendanceDeviceServiceResult<AttendanceDeviceResponse>.Fail(error.Value.Status, error.Value.Message);

            Apply(entity, input);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            var saveError = await SaveAsync(cancellationToken);
            if (saveError != null) return AttendanceDeviceServiceResult<AttendanceDeviceResponse>.Fail(StatusCodes.Status409Conflict, saveError);

            // Identitas mesin aktif bisa berubah menjadi identitas yang selama ini ditahan.
            return await CompleteWriteAsync(entity.Id, entity.IsActive, actorUserId, "Mesin absen berhasil diperbarui.", cancellationToken);
        }

        public async Task<AttendanceDeviceServiceResult<AttendanceDeviceResponse>> UpdateStatusAsync(
            Guid id,
            bool isActive,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstAttendanceDevice>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null) return AttendanceDeviceServiceResult<AttendanceDeviceResponse>.Fail(StatusCodes.Status404NotFound, "Mesin absen tidak ditemukan.");

            if (isActive && !entity.IsActive)
            {
                var keys = IdentityKeys(entity.AttendanceDeviceCode, entity.SerialNumber, entity.ExternalDeviceId);
                if (await IdentityUsedByOtherActiveDeviceAsync(keys, id, cancellationToken))
                {
                    return AttendanceDeviceServiceResult<AttendanceDeviceResponse>.Fail(StatusCodes.Status409Conflict, IdentityConflictMessage);
                }
            }

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await CompleteWriteAsync(
                entity.Id,
                isActive,
                actorUserId,
                isActive ? "Mesin absen berhasil diaktifkan." : "Mesin absen berhasil dinonaktifkan; scan berikutnya dari mesin ini ditahan.",
                cancellationToken);
        }

        public async Task<AttendanceDeviceServiceResult<AttendanceDeviceResponse>> DeleteAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstAttendanceDevice>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null) return AttendanceDeviceServiceResult<AttendanceDeviceResponse>.Fail(StatusCodes.Status404NotFound, "Mesin absen tidak ditemukan.");

            if (await _dbContext.Set<HrdAttendanceRawLog>().AnyAsync(x => x.AttendanceDeviceId == id, cancellationToken))
            {
                return AttendanceDeviceServiceResult<AttendanceDeviceResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Mesin absen sudah punya log scan dan tidak dapat dihapus. Nonaktifkan mesin bila tidak dipakai lagi.");
            }

            if (await _dbContext.Set<HrdAttendanceDeviceIncident>().AnyAsync(x => x.AttendanceDeviceId == id, cancellationToken))
            {
                return AttendanceDeviceServiceResult<AttendanceDeviceResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Mesin absen sudah punya catatan gangguan dan tidak dapat dihapus. Nonaktifkan mesin bila tidak dipakai lagi.");
            }

            var response = (await MapAsync(new List<MstAttendanceDevice> { entity }, cancellationToken)).Single();
            var now = DateTime.UtcNow;
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            response.IsActive = false;
            return AttendanceDeviceServiceResult<AttendanceDeviceResponse>.Ok(response, "Mesin absen berhasil dihapus.");
        }

        private async Task<AttendanceDeviceServiceResult<AttendanceDeviceResponse>> CompleteWriteAsync(
            Guid id,
            bool isActive,
            Guid actorUserId,
            string message,
            CancellationToken cancellationToken)
        {
            AttendanceRawLogReleaseResponse? release = null;
            string? releaseMessage = null;
            if (isActive)
            {
                try
                {
                    var result = await _rawLogService.ReleaseHeldLogsForDeviceAsync(id, actorUserId, cancellationToken);
                    if (result.Success)
                    {
                        release = result.Data;
                    }
                    else
                    {
                        releaseMessage = result.Message;
                    }
                }
                catch (Exception ex)
                {
                    // Pendaftaran sudah tersimpan; scan tetap tertahan dan dapat dilepas ulang dengan mengaktifkan ulang mesin.
                    _logger.LogError(ex, "Pelepasan scan tertahan sesudah menyimpan mesin {AttendanceDeviceId} gagal.", id);
                    releaseMessage = "Pelepasan scan tertahan gagal; scan tetap ditahan dan dapat dipicu ulang.";
                }
            }

            _dbContext.ChangeTracker.Clear();
            var response = await GetByIdAsync(id, cancellationToken);
            response!.HeldScanRelease = release;
            response.HeldScanReleaseMessage = releaseMessage;
            if (release != null && release.HeldLogCount > 0)
            {
                message = $"{message} {release.MatchedCount + release.PendingCount} scan tertahan dilepas.";
            }
            else if (releaseMessage != null)
            {
                message = $"{message} {releaseMessage}";
            }

            return AttendanceDeviceServiceResult<AttendanceDeviceResponse>.Ok(response, message);
        }

        private async Task<string?> SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return null;
            }
            catch (DbUpdateException ex)
            {
                // Indeks unik kode/nomor seri menolak simpan bersamaan yang lolos pemeriksaan awal.
                _logger.LogWarning(ex, "Simpan mesin absen ditolak basis data.");
                _dbContext.ChangeTracker.Clear();
                return IdentityConflictMessage;
            }
        }

        private async Task<(int Status, string Message)?> ValidateAsync(
            DeviceInput input,
            Guid? excludeId,
            bool willBeActive,
            CancellationToken cancellationToken)
        {
            if (input.Code == null) return (StatusCodes.Status400BadRequest, "Kode mesin wajib diisi.");
            if (input.Name == null) return (StatusCodes.Status400BadRequest, "Nama mesin wajib diisi.");
            if (!DeviceTypes.Any(x => x.Value == input.DeviceType)) return (StatusCodes.Status400BadRequest, "Jenis mesin tidak valid.");
            if (input.Port.HasValue && (input.Port < 1 || input.Port > 65535)) return (StatusCodes.Status400BadRequest, "Port harus 1 sampai 65535.");
            if (input.IpAddress != null && !IPAddress.TryParse(input.IpAddress, out _)) return (StatusCodes.Status400BadRequest, "Alamat IP tidak valid.");

            if (input.HospitalSiteId.HasValue &&
                !await _dbContext.MstHospitalSites.AnyAsync(x => x.Id == input.HospitalSiteId && !x.IsDelete, cancellationToken))
            {
                return (StatusCodes.Status400BadRequest, "Rumah sakit tidak ditemukan.");
            }

            if (input.AttendanceLocationId.HasValue)
            {
                var location = await _dbContext.Set<MstAttendanceLocation>()
                    .AsNoTracking()
                    .Where(x => x.Id == input.AttendanceLocationId && !x.IsDelete && x.IsActive)
                    .Select(x => new { x.HospitalSiteId })
                    .FirstOrDefaultAsync(cancellationToken);
                if (location == null) return (StatusCodes.Status400BadRequest, "Lokasi absensi tidak ditemukan atau tidak aktif.");
                if (location.HospitalSiteId.HasValue && input.HospitalSiteId.HasValue && location.HospitalSiteId != input.HospitalSiteId)
                {
                    return (StatusCodes.Status400BadRequest, "Lokasi absensi tidak berada di rumah sakit yang dipilih.");
                }
            }

            // Indeks unik basis data: kode dan nomor seri unik di antara mesin yang belum dihapus (aktif maupun nonaktif).
            var others = _dbContext.Set<MstAttendanceDevice>().AsNoTracking().Where(x => !x.IsDelete && (!excludeId.HasValue || x.Id != excludeId));
            if (await others.AnyAsync(x => x.AttendanceDeviceCode == input.Code, cancellationToken)) return (StatusCodes.Status409Conflict, IdentityConflictMessage);
            if (input.SerialNumber != null && await others.AnyAsync(x => x.SerialNumber == input.SerialNumber, cancellationToken)) return (StatusCodes.Status409Conflict, IdentityConflictMessage);

            // VR-PK-26: kode, nomor seri, atau ID eksternal tidak boleh cocok dengan identitas mana pun milik mesin aktif lain,
            // karena scan dicocokkan lewat ketiganya sekaligus.
            if (willBeActive &&
                await IdentityUsedByOtherActiveDeviceAsync(IdentityKeys(input.Code, input.SerialNumber, input.ExternalDeviceId), excludeId, cancellationToken))
            {
                return (StatusCodes.Status409Conflict, IdentityConflictMessage);
            }

            return null;
        }

        private async Task<bool> IdentityUsedByOtherActiveDeviceAsync(List<string> keys, Guid? excludeId, CancellationToken cancellationToken)
        {
            if (keys.Count == 0) return false;
            return await _dbContext.Set<MstAttendanceDevice>()
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete && x.IsActive && (!excludeId.HasValue || x.Id != excludeId) &&
                    (keys.Contains(x.AttendanceDeviceCode) ||
                     (x.SerialNumber != null && keys.Contains(x.SerialNumber)) ||
                     (x.ExternalDeviceId != null && keys.Contains(x.ExternalDeviceId))),
                    cancellationToken);
        }

        private static List<string> IdentityKeys(params string?[] values) =>
            values.Select(N).Where(x => x != null).Select(x => x!).Distinct().ToList();

        private IQueryable<MstAttendanceDevice> BaseQuery() =>
            _dbContext.Set<MstAttendanceDevice>()
                .AsNoTracking()
                .Include(x => x.AttendanceLocation)
                .Include(x => x.HospitalSite)
                .Where(x => !x.IsDelete);

        private async Task<List<AttendanceDeviceResponse>> MapAsync(List<MstAttendanceDevice> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0) return new List<AttendanceDeviceResponse>();

            var actorIds = rows.Select(x => x.CreateBy).Where(x => x != Guid.Empty).Distinct().ToList();
            var actors = await _dbContext.Users.AsNoTracking()
                .Where(x => actorIds.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var heldCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            var allKeys = rows.SelectMany(x => IdentityKeys(x.AttendanceDeviceCode, x.SerialNumber, x.ExternalDeviceId)).Distinct().ToList();
            if (allKeys.Count > 0)
            {
                var awaiting = AttendanceValueConstants.RawLogProcessingStatus.AwaitingDeviceRegistration;
                heldCounts = await _dbContext.Set<HrdAttendanceRawLog>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.ProcessingStatus == awaiting && x.ExternalDeviceId != null && allKeys.Contains(x.ExternalDeviceId))
                    .GroupBy(x => x.ExternalDeviceId!)
                    .Select(g => new { g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Key, x => x.Count, StringComparer.Ordinal, cancellationToken);
            }

            return rows.Select(x => new AttendanceDeviceResponse
            {
                Id = x.Id,
                AttendanceDeviceCode = x.AttendanceDeviceCode,
                AttendanceDeviceName = x.AttendanceDeviceName,
                DeviceType = x.DeviceType,
                DeviceTypeLabel = DeviceTypes.FirstOrDefault(t => t.Value == x.DeviceType).Label ?? x.DeviceType,
                SerialNumber = x.SerialNumber,
                Manufacturer = x.Manufacturer,
                ModelName = x.ModelName,
                IpAddress = x.IpAddress,
                Port = x.Port,
                MacAddress = x.MacAddress,
                IntegrationProvider = x.IntegrationProvider,
                ExternalDeviceId = x.ExternalDeviceId,
                AttendanceLocationId = x.AttendanceLocationId,
                AttendanceLocationName = x.AttendanceLocation?.AttendanceLocationName,
                HospitalSiteId = x.HospitalSiteId,
                HospitalSiteName = x.HospitalSite?.SiteName,
                LastSyncAt = x.LastSyncAt,
                IsOnline = x.IsOnline,
                IsPrimary = x.IsPrimary,
                Description = x.Description,
                IsActive = x.IsActive,
                HeldScanCount = IdentityKeys(x.AttendanceDeviceCode, x.SerialNumber, x.ExternalDeviceId).Sum(k => heldCounts.GetValueOrDefault(k)),
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = actors.GetValueOrDefault(x.CreateBy),
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy
            }).ToList();
        }

        private static void Apply(MstAttendanceDevice entity, DeviceInput input)
        {
            entity.AttendanceDeviceCode = input.Code!;
            entity.AttendanceDeviceName = input.Name!;
            entity.DeviceType = input.DeviceType;
            entity.SerialNumber = input.SerialNumber;
            entity.Manufacturer = input.Manufacturer;
            entity.ModelName = input.ModelName;
            entity.IpAddress = input.IpAddress;
            entity.Port = input.Port;
            entity.MacAddress = input.MacAddress;
            entity.IntegrationProvider = input.IntegrationProvider;
            entity.ExternalDeviceId = input.ExternalDeviceId;
            entity.AttendanceLocationId = input.AttendanceLocationId;
            entity.HospitalSiteId = input.HospitalSiteId;
            entity.IsPrimary = input.IsPrimary;
            entity.Description = input.Description;
        }

        private static DeviceInput Normalize(CreateAttendanceDeviceRequest r) => new(
            N(r.AttendanceDeviceCode), N(r.AttendanceDeviceName), N(r.DeviceType) ?? string.Empty, N(r.SerialNumber), N(r.Manufacturer), N(r.ModelName),
            N(r.IpAddress), r.Port, N(r.MacAddress), N(r.IntegrationProvider), N(r.ExternalDeviceId), NG(r.AttendanceLocationId), NG(r.HospitalSiteId),
            r.IsPrimary, N(r.Description));

        private static DeviceInput Normalize(UpdateAttendanceDeviceRequest r) => new(
            N(r.AttendanceDeviceCode), N(r.AttendanceDeviceName), N(r.DeviceType) ?? string.Empty, N(r.SerialNumber), N(r.Manufacturer), N(r.ModelName),
            N(r.IpAddress), r.Port, N(r.MacAddress), N(r.IntegrationProvider), N(r.ExternalDeviceId), NG(r.AttendanceLocationId), NG(r.HospitalSiteId),
            r.IsPrimary, N(r.Description));

        private static string? N(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static Guid? NG(Guid? value) => !value.HasValue || value == Guid.Empty ? null : value;

        private sealed record DeviceInput(
            string? Code, string? Name, string DeviceType, string? SerialNumber, string? Manufacturer, string? ModelName,
            string? IpAddress, int? Port, string? MacAddress, string? IntegrationProvider, string? ExternalDeviceId,
            Guid? AttendanceLocationId, Guid? HospitalSiteId, bool IsPrimary, string? Description);
    }

    public class AttendanceDeviceServiceResult<T>
    {
        public bool Success { get; private set; }
        public int StatusCode { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public T? Data { get; private set; }

        public static AttendanceDeviceServiceResult<T> Ok(T data, string message) =>
            new() { Success = true, StatusCode = StatusCodes.Status200OK, Message = message, Data = data };

        public static AttendanceDeviceServiceResult<T> Fail(int statusCode, string message) =>
            new() { Success = false, StatusCode = statusCode, Message = message };
    }
}
