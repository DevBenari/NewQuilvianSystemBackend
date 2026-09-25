using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Services
{
    /// <summary>
    /// Pemilik master sumber daya unit HD — mesin, station, pengaturan unit, dan butir checklist
    /// Pra-HD (<c>BE-HMD-04</c>, <c>BE-HMD-05</c>). Controller tidak menyentuh
    /// <c>ApplicationDbContext</c> sendiri (<c>QBE-SVC-001</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Status mesin selalu dibaca langsung dari basis data</b>, tidak pernah dari cache. Risiko
    /// yang dijaga: mesin rusak tetap dijadwalkan karena status basi. Setiap perpindahan status
    /// menulis satu baris <see cref="HmdMachineStatusHistory"/> pada <c>SaveChanges</c> yang sama
    /// dengan perubahan statusnya, sehingga tidak ada perpindahan tanpa jejak.
    /// </para>
    /// <para>
    /// <b>Contoh.</b> Teknisi memindahkan mesin <c>M-03</c> dari <c>Ready</c> ke <c>Blocked</c>
    /// dengan alasan "Kebocoran dialisat". Status berubah, dan satu baris riwayat tercatat berisi
    /// status asal, status tujuan, alasan, pelaku, dan waktu server. Tanpa alasan, permintaan
    /// ditolak <c>400 HMD-VAL-091</c>; bila mesin sedang dipakai sesi berjalan, ditolak
    /// <c>409 HMD-VAL-090</c>.
    /// </para>
    /// </remarks>
    public class HmdResourceService
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>Perpindahan status mesin yang sah — <c>state-transition-matrix.md</c> bagian 5.</summary>
        private static readonly HashSet<(HmdMachineStatus From, HmdMachineStatus To)> AllowedMachineTransitions =
        [
            (HmdMachineStatus.Ready, HmdMachineStatus.Blocked),
            (HmdMachineStatus.Ready, HmdMachineStatus.Maintenance),
            (HmdMachineStatus.Ready, HmdMachineStatus.NotEligible),
            (HmdMachineStatus.Blocked, HmdMachineStatus.Ready),
            (HmdMachineStatus.Maintenance, HmdMachineStatus.Ready),
            (HmdMachineStatus.NotEligible, HmdMachineStatus.Ready),
            (HmdMachineStatus.Blocked, HmdMachineStatus.Maintenance),
            (HmdMachineStatus.Maintenance, HmdMachineStatus.Blocked)
        ];

        /// <summary>Perpindahan status station yang sah — <c>state-transition-matrix.md</c> bagian 6.</summary>
        private static readonly HashSet<(HmdStationStatus From, HmdStationStatus To)> AllowedStationTransitions =
        [
            (HmdStationStatus.Available, HmdStationStatus.Blocked),
            (HmdStationStatus.Available, HmdStationStatus.Maintenance),
            (HmdStationStatus.Blocked, HmdStationStatus.Available),
            (HmdStationStatus.Maintenance, HmdStationStatus.Available)
        ];

        /// <summary>Sesi yang belum berakhir; mesin atau station yang dipakainya belum boleh dihapus.</summary>
        private static readonly HmdSessionStatus[] OpenSessionStatuses =
        [
            HmdSessionStatus.Planned,
            HmdSessionStatus.Scheduled,
            HmdSessionStatus.CheckedIn,
            HmdSessionStatus.PreCheck,
            HmdSessionStatus.Held,
            HmdSessionStatus.Ready,
            HmdSessionStatus.InProgress
        ];

        public HmdResourceService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // =================================================================
        // Mesin
        // =================================================================

        public static HmdMachineFilterMetadataResponse BuildMachineFilterMetadata() => new()
        {
            MachineStatusOptions = HmdLabels.Options<HmdMachineStatus>(HmdLabels.MachineStatus),
            DedicatedForOptions = HmdLabels.Options<HmdIsolationRequirement>(HmdLabels.Isolation),
            SortOptions =
            [
                new() { Value = "machineCode", Label = "Kode mesin" },
                new() { Value = "machineName", Label = "Nama mesin" },
                new() { Value = "machineStatus", Label = "Status" },
                new() { Value = "lastStatusChangedAt", Label = "Perubahan status terakhir" },
                new() { Value = "createDateTime", Label = "Tanggal dibuat" }
            ],
            SortDirections = HmdLabels.SortDirections(),
            PageSizeOptions = HmdLabels.PageSizeOptions()
        };

        public async Task<HmdMachineSummaryResponse> GetMachineSummaryAsync(CancellationToken cancellationToken)
        {
            var rows = _dbContext.HmdMachines.AsNoTracking().Where(x => !x.IsDelete);

            return new HmdMachineSummaryResponse
            {
                TotalMachine = await rows.CountAsync(cancellationToken),
                ActiveMachine = await rows.CountAsync(x => x.IsActive, cancellationToken),
                InactiveMachine = await rows.CountAsync(x => !x.IsActive, cancellationToken),
                ReadyMachine = await rows.CountAsync(x => x.MachineStatus == HmdMachineStatus.Ready, cancellationToken),
                BlockedMachine = await rows.CountAsync(x => x.MachineStatus == HmdMachineStatus.Blocked, cancellationToken),
                MaintenanceMachine = await rows.CountAsync(x => x.MachineStatus == HmdMachineStatus.Maintenance, cancellationToken),
                NotEligibleMachine = await rows.CountAsync(x => x.MachineStatus == HmdMachineStatus.NotEligible, cancellationToken),
                DedicatedIsolationMachine = await rows.CountAsync(x => x.DedicatedFor != HmdIsolationRequirement.None, cancellationToken)
            };
        }

        public async Task<PagedResult<HmdMachineResponse>> GetMachinesAsync(
            HmdMachinePagedQuery query,
            CancellationToken cancellationToken)
        {
            var (pageNumber, pageSize) = HmdServiceSupport.NormalizePaging(query.PageNumber, query.PageSize);

            var rows = _dbContext.HmdMachines.AsNoTracking().Where(x => !x.IsDelete);

            if (query.ServiceUnitId.HasValue)
                rows = rows.Where(x => x.ServiceUnitId == query.ServiceUnitId.Value);
            if (query.MachineStatus.HasValue)
                rows = rows.Where(x => x.MachineStatus == query.MachineStatus.Value);
            if (query.DedicatedFor.HasValue)
                rows = rows.Where(x => x.DedicatedFor == query.DedicatedFor.Value);
            if (query.IsSchedulable.HasValue)
                rows = rows.Where(x => x.IsSchedulable == query.IsSchedulable.Value);
            if (query.IsActive.HasValue)
                rows = rows.Where(x => x.IsActive == query.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();
                rows = rows.Where(x =>
                    x.MachineCode.ToLower().Contains(keyword) ||
                    x.MachineName.ToLower().Contains(keyword) ||
                    (x.SerialNumber != null && x.SerialNumber.ToLower().Contains(keyword)));
            }

            var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            rows = (query.SortBy ?? "machineCode").ToLowerInvariant() switch
            {
                "machinename" => descending ? rows.OrderByDescending(x => x.MachineName) : rows.OrderBy(x => x.MachineName),
                "machinestatus" => descending ? rows.OrderByDescending(x => x.MachineStatus) : rows.OrderBy(x => x.MachineStatus),
                "laststatuschangedat" => descending ? rows.OrderByDescending(x => x.LastStatusChangedAt) : rows.OrderBy(x => x.LastStatusChangedAt),
                "createdatetime" => descending ? rows.OrderByDescending(x => x.CreateDateTime) : rows.OrderBy(x => x.CreateDateTime),
                _ => descending ? rows.OrderByDescending(x => x.MachineCode) : rows.OrderBy(x => x.MachineCode)
            };

            var total = await rows.CountAsync(cancellationToken);
            var items = await ProjectMachine(rows.Skip((pageNumber - 1) * pageSize).Take(pageSize))
                .ToListAsync(cancellationToken);

            return new PagedResult<HmdMachineResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = HmdServiceSupport.TotalPage(total, pageSize),
                Items = items.Select(Label).ToList()
            };
        }

        public async Task<PagedResult<HmdMachineOptionResponse>> GetMachineOptionsAsync(
            HmdMachineOptionsQuery query,
            CancellationToken cancellationToken)
        {
            var (pageNumber, pageSize) = HmdServiceSupport.NormalizePaging(query.PageNumber, query.PageSize);

            var rows = _dbContext.HmdMachines.AsNoTracking().Where(x => !x.IsDelete && x.IsActive);

            if (query.OnlySchedulable)
                rows = rows.Where(x => x.IsSchedulable && x.MachineStatus == HmdMachineStatus.Ready);
            if (query.ServiceUnitId.HasValue)
                rows = rows.Where(x => x.ServiceUnitId == query.ServiceUnitId.Value);
            if (query.DedicatedFor.HasValue)
                rows = rows.Where(x => x.DedicatedFor == query.DedicatedFor.Value);
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();
                rows = rows.Where(x => x.MachineCode.ToLower().Contains(keyword) || x.MachineName.ToLower().Contains(keyword));
            }

            rows = rows.OrderBy(x => x.MachineCode).ThenBy(x => x.MachineName);

            var total = await rows.CountAsync(cancellationToken);
            var items = await rows
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new HmdMachineOptionResponse
                {
                    Id = x.Id,
                    MachineCode = x.MachineCode,
                    MachineName = x.MachineName,
                    ServiceUnitId = x.ServiceUnitId,
                    DedicatedFor = x.DedicatedFor
                })
                .ToListAsync(cancellationToken);

            items.ForEach(x => x.DedicatedForName = HmdLabels.Isolation(x.DedicatedFor));

            return new PagedResult<HmdMachineOptionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = HmdServiceSupport.TotalPage(total, pageSize),
                Items = items
            };
        }

        public async Task<HmdMachineResponse?> GetMachineAsync(Guid id, CancellationToken cancellationToken)
        {
            var row = await ProjectMachine(_dbContext.HmdMachines.AsNoTracking().Where(x => x.Id == id && !x.IsDelete))
                .FirstOrDefaultAsync(cancellationToken);

            return row == null ? null : Label(row);
        }

        public async Task<HmdResult<PagedResult<HmdMachineStatusHistoryResponse>>> GetMachineStatusHistoryAsync(
            Guid id,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var exists = await _dbContext.HmdMachines.AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (!exists)
                return HmdResult<PagedResult<HmdMachineStatusHistoryResponse>>.NotFound("Mesin tidak ditemukan atau sudah dihapus.");

            (pageNumber, pageSize) = HmdServiceSupport.NormalizePaging(pageNumber, pageSize);

            var rows = _dbContext.HmdMachineStatusHistories.AsNoTracking()
                .Where(x => x.MachineId == id && !x.IsDelete)
                .OrderByDescending(x => x.ChangedAt);

            var total = await rows.CountAsync(cancellationToken);
            var items = await rows
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new HmdMachineStatusHistoryResponse
                {
                    Id = x.Id,
                    MachineId = x.MachineId,
                    FromStatus = x.FromStatus,
                    ToStatus = x.ToStatus,
                    Reason = x.Reason,
                    ChangedByUserId = x.ChangedByUserId,
                    ChangedByName = _dbContext.Users.Where(u => u.Id == x.ChangedByUserId).Select(u => u.DisplayName).FirstOrDefault(),
                    ChangedAt = x.ChangedAt
                })
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                item.FromStatusName = item.FromStatus.HasValue ? HmdLabels.MachineStatus(item.FromStatus.Value) : null;
                item.ToStatusName = HmdLabels.MachineStatus(item.ToStatus);
            }

            return HmdResult<PagedResult<HmdMachineStatusHistoryResponse>>.Ok(new PagedResult<HmdMachineStatusHistoryResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = HmdServiceSupport.TotalPage(total, pageSize),
                Items = items
            });
        }

        public async Task<HmdResult<HmdMachineResponse>> CreateMachineAsync(
            CreateHmdMachineRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var code = request.MachineCode.Trim();

            if (!await ServiceUnitExistsAsync(request.ServiceUnitId, cancellationToken))
                return HmdResult<HmdMachineResponse>.Invalid(HmdErrorCodes.InvalidRequest, "Unit layanan tidak ditemukan atau tidak aktif.");

            if (await _dbContext.HmdMachines.AnyAsync(x => x.ServiceUnitId == request.ServiceUnitId && x.MachineCode == code, cancellationToken))
                return HmdResult<HmdMachineResponse>.Conflict(HmdErrorCodes.Val092, HmdMessages.Val092);

            var now = DateTime.UtcNow;
            var machine = new HmdMachine
            {
                MachineCode = code,
                MachineName = request.MachineName.Trim(),
                ServiceUnitId = request.ServiceUnitId,
                Manufacturer = HmdServiceSupport.Normalize(request.Manufacturer),
                SerialNumber = HmdServiceSupport.Normalize(request.SerialNumber),
                MachineStatus = HmdMachineStatus.Ready,
                DedicatedFor = request.DedicatedFor,
                IsSchedulable = request.IsSchedulable,
                IsActive = true,
                LastStatusChangedAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.HmdMachines.Add(machine);
            _dbContext.HmdMachineStatusHistories.Add(new HmdMachineStatusHistory
            {
                MachineId = machine.Id,
                FromStatus = null,
                ToStatus = HmdMachineStatus.Ready,
                Reason = "Mesin didaftarkan.",
                ChangedByUserId = actorUserId,
                ChangedAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId
            });

            var saved = await TrySaveAsync(cancellationToken);
            if (saved != null)
                return HmdResult<HmdMachineResponse>.From(saved);

            return HmdResult<HmdMachineResponse>.Created((await GetMachineAsync(machine.Id, cancellationToken))!);
        }

        public async Task<HmdResult<HmdMachineResponse>> UpdateMachineAsync(
            Guid id,
            UpdateHmdMachineRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var machine = await _dbContext.HmdMachines.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (machine == null)
                return HmdResult<HmdMachineResponse>.NotFound("Mesin tidak ditemukan atau sudah dihapus.");

            var code = request.MachineCode.Trim();
            if (!string.Equals(code, machine.MachineCode, StringComparison.Ordinal) &&
                await _dbContext.HmdMachines.AnyAsync(x => x.Id != id && x.ServiceUnitId == machine.ServiceUnitId && x.MachineCode == code, cancellationToken))
            {
                return HmdResult<HmdMachineResponse>.Conflict(HmdErrorCodes.Val092, HmdMessages.Val092);
            }

            machine.MachineCode = code;
            machine.MachineName = request.MachineName.Trim();
            machine.Manufacturer = HmdServiceSupport.Normalize(request.Manufacturer);
            machine.SerialNumber = HmdServiceSupport.Normalize(request.SerialNumber);
            machine.DedicatedFor = request.DedicatedFor;
            machine.IsSchedulable = request.IsSchedulable;
            machine.IsActive = request.IsActive;
            machine.UpdateDateTime = DateTime.UtcNow;
            machine.UpdateBy = actorUserId;
            machine.Version++;

            var saved = await TrySaveAsync(cancellationToken);
            if (saved != null)
                return HmdResult<HmdMachineResponse>.From(saved);

            return HmdResult<HmdMachineResponse>.Ok((await GetMachineAsync(id, cancellationToken))!);
        }

        /// <summary>
        /// Mengubah status laik pakai mesin beserta alasannya, lalu menulis satu baris riwayat.
        /// </summary>
        public async Task<HmdResult<HmdMachineResponse>> ChangeMachineStatusAsync(
            Guid id,
            ChangeHmdMachineStatusRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var reason = HmdServiceSupport.Normalize(request.Reason);
            if (reason == null)
                return HmdResult<HmdMachineResponse>.Invalid(HmdErrorCodes.Val091, HmdMessages.Val091);

            var machine = await _dbContext.HmdMachines.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (machine == null)
                return HmdResult<HmdMachineResponse>.NotFound("Mesin tidak ditemukan atau sudah dihapus.");

            var from = machine.MachineStatus;
            if (!AllowedMachineTransitions.Contains((from, request.ToStatus)))
            {
                return HmdResult<HmdMachineResponse>.Rule(
                    HmdErrorCodes.InvalidTransition,
                    $"Status mesin tidak dapat diubah dari {HmdLabels.MachineStatus(from)} menjadi {HmdLabels.MachineStatus(request.ToStatus)}.");
            }

            // HMD-VAL-090: mesin yang sedang dipakai sesi berjalan tidak boleh dinyatakan tidak
            // siap. Dibaca langsung dari database, bukan dari status yang mungkin sudah basi.
            if (from == HmdMachineStatus.Ready &&
                await _dbContext.HmdSessions.AnyAsync(x => x.MachineId == id && !x.IsDelete && x.SessionStatus == HmdSessionStatus.InProgress, cancellationToken))
            {
                return HmdResult<HmdMachineResponse>.Conflict(HmdErrorCodes.Val090, HmdMessages.Val090);
            }

            var now = DateTime.UtcNow;
            machine.MachineStatus = request.ToStatus;
            machine.LastStatusChangedAt = now;
            machine.UpdateDateTime = now;
            machine.UpdateBy = actorUserId;
            machine.Version++;

            _dbContext.HmdMachineStatusHistories.Add(new HmdMachineStatusHistory
            {
                MachineId = machine.Id,
                FromStatus = from,
                ToStatus = request.ToStatus,
                Reason = reason,
                ChangedByUserId = actorUserId,
                ChangedAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId
            });

            var saved = await TrySaveAsync(cancellationToken);
            if (saved != null)
                return HmdResult<HmdMachineResponse>.From(saved);

            return HmdResult<HmdMachineResponse>.Ok((await GetMachineAsync(id, cancellationToken))!);
        }

        public async Task<HmdResult<bool>> DeleteMachineAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
        {
            var machine = await _dbContext.HmdMachines.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (machine == null)
                return HmdResult<bool>.NotFound("Mesin tidak ditemukan atau sudah dihapus.");

            if (await _dbContext.HmdSessions.AnyAsync(x => x.MachineId == id && !x.IsDelete && OpenSessionStatuses.Contains(x.SessionStatus), cancellationToken))
            {
                return HmdResult<bool>.Invalid(
                    HmdErrorCodes.InvalidRequest,
                    "Mesin tidak dapat dinonaktifkan karena masih dipakai sesi yang belum selesai.");
            }

            var now = DateTime.UtcNow;
            machine.IsDelete = true;
            machine.IsActive = false;
            machine.DeleteDateTime = now;
            machine.DeleteBy = actorUserId;
            machine.Version++;

            var saved = await TrySaveAsync(cancellationToken);
            return saved != null ? HmdResult<bool>.From(saved) : HmdResult<bool>.Ok(true);
        }

        // =================================================================
        // Station
        // =================================================================

        public static HmdStationFilterMetadataResponse BuildStationFilterMetadata() => new()
        {
            StationStatusOptions = HmdLabels.Options<HmdStationStatus>(HmdLabels.StationStatus),
            SortOptions =
            [
                new() { Value = "stationCode", Label = "Kode station" },
                new() { Value = "stationName", Label = "Nama station" },
                new() { Value = "stationStatus", Label = "Status" },
                new() { Value = "createDateTime", Label = "Tanggal dibuat" }
            ],
            SortDirections = HmdLabels.SortDirections(),
            PageSizeOptions = HmdLabels.PageSizeOptions()
        };

        public async Task<HmdStationSummaryResponse> GetStationSummaryAsync(CancellationToken cancellationToken)
        {
            var rows = _dbContext.HmdStations.AsNoTracking().Where(x => !x.IsDelete);

            return new HmdStationSummaryResponse
            {
                TotalStation = await rows.CountAsync(cancellationToken),
                ActiveStation = await rows.CountAsync(x => x.IsActive, cancellationToken),
                InactiveStation = await rows.CountAsync(x => !x.IsActive, cancellationToken),
                AvailableStation = await rows.CountAsync(x => x.StationStatus == HmdStationStatus.Available, cancellationToken),
                BlockedStation = await rows.CountAsync(x => x.StationStatus == HmdStationStatus.Blocked, cancellationToken),
                MaintenanceStation = await rows.CountAsync(x => x.StationStatus == HmdStationStatus.Maintenance, cancellationToken),
                IsolationStation = await rows.CountAsync(x => x.IsIsolationStation, cancellationToken)
            };
        }

        public async Task<PagedResult<HmdStationResponse>> GetStationsAsync(
            HmdStationPagedQuery query,
            CancellationToken cancellationToken)
        {
            var (pageNumber, pageSize) = HmdServiceSupport.NormalizePaging(query.PageNumber, query.PageSize);

            var rows = _dbContext.HmdStations.AsNoTracking().Where(x => !x.IsDelete);

            if (query.ServiceUnitId.HasValue)
                rows = rows.Where(x => x.ServiceUnitId == query.ServiceUnitId.Value);
            if (query.StationStatus.HasValue)
                rows = rows.Where(x => x.StationStatus == query.StationStatus.Value);
            if (query.IsIsolationStation.HasValue)
                rows = rows.Where(x => x.IsIsolationStation == query.IsIsolationStation.Value);
            if (query.IsActive.HasValue)
                rows = rows.Where(x => x.IsActive == query.IsActive.Value);
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();
                rows = rows.Where(x => x.StationCode.ToLower().Contains(keyword) || x.StationName.ToLower().Contains(keyword));
            }

            var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            rows = (query.SortBy ?? "stationCode").ToLowerInvariant() switch
            {
                "stationname" => descending ? rows.OrderByDescending(x => x.StationName) : rows.OrderBy(x => x.StationName),
                "stationstatus" => descending ? rows.OrderByDescending(x => x.StationStatus) : rows.OrderBy(x => x.StationStatus),
                "createdatetime" => descending ? rows.OrderByDescending(x => x.CreateDateTime) : rows.OrderBy(x => x.CreateDateTime),
                _ => descending ? rows.OrderByDescending(x => x.StationCode) : rows.OrderBy(x => x.StationCode)
            };

            var total = await rows.CountAsync(cancellationToken);
            var items = await ProjectStation(rows.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync(cancellationToken);
            items.ForEach(x => x.StationStatusName = HmdLabels.StationStatus(x.StationStatus));

            return new PagedResult<HmdStationResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = HmdServiceSupport.TotalPage(total, pageSize),
                Items = items
            };
        }

        public async Task<PagedResult<HmdStationOptionResponse>> GetStationOptionsAsync(
            HmdStationOptionsQuery query,
            CancellationToken cancellationToken)
        {
            var (pageNumber, pageSize) = HmdServiceSupport.NormalizePaging(query.PageNumber, query.PageSize);

            var rows = _dbContext.HmdStations.AsNoTracking().Where(x => !x.IsDelete && x.IsActive);

            if (query.OnlyAvailable)
                rows = rows.Where(x => x.StationStatus == HmdStationStatus.Available);
            if (query.ServiceUnitId.HasValue)
                rows = rows.Where(x => x.ServiceUnitId == query.ServiceUnitId.Value);
            if (query.IsIsolationStation.HasValue)
                rows = rows.Where(x => x.IsIsolationStation == query.IsIsolationStation.Value);
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();
                rows = rows.Where(x => x.StationCode.ToLower().Contains(keyword) || x.StationName.ToLower().Contains(keyword));
            }

            rows = rows.OrderBy(x => x.StationCode).ThenBy(x => x.StationName);

            var total = await rows.CountAsync(cancellationToken);
            var items = await rows
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new HmdStationOptionResponse
                {
                    Id = x.Id,
                    StationCode = x.StationCode,
                    StationName = x.StationName,
                    ServiceUnitId = x.ServiceUnitId,
                    IsIsolationStation = x.IsIsolationStation
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<HmdStationOptionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = HmdServiceSupport.TotalPage(total, pageSize),
                Items = items
            };
        }

        public async Task<HmdStationResponse?> GetStationAsync(Guid id, CancellationToken cancellationToken)
        {
            var row = await ProjectStation(_dbContext.HmdStations.AsNoTracking().Where(x => x.Id == id && !x.IsDelete))
                .FirstOrDefaultAsync(cancellationToken);

            if (row != null)
                row.StationStatusName = HmdLabels.StationStatus(row.StationStatus);

            return row;
        }

        public async Task<HmdResult<HmdStationResponse>> CreateStationAsync(
            CreateHmdStationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var code = request.StationCode.Trim();

            if (!await ServiceUnitExistsAsync(request.ServiceUnitId, cancellationToken))
                return HmdResult<HmdStationResponse>.Invalid(HmdErrorCodes.InvalidRequest, "Unit layanan tidak ditemukan atau tidak aktif.");

            if (request.RoomId.HasValue && !await RoomExistsAsync(request.RoomId.Value, cancellationToken))
                return HmdResult<HmdStationResponse>.Invalid(HmdErrorCodes.InvalidRequest, "Ruang tidak ditemukan atau tidak aktif.");

            if (await _dbContext.HmdStations.AnyAsync(x => x.ServiceUnitId == request.ServiceUnitId && x.StationCode == code, cancellationToken))
                return HmdResult<HmdStationResponse>.Conflict(HmdErrorCodes.Val092, HmdMessages.Val092);

            var now = DateTime.UtcNow;
            var station = new HmdStation
            {
                StationCode = code,
                StationName = request.StationName.Trim(),
                ServiceUnitId = request.ServiceUnitId,
                RoomId = request.RoomId,
                StationStatus = HmdStationStatus.Available,
                IsIsolationStation = request.IsIsolationStation,
                IsActive = true,
                LastStatusChangedAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.HmdStations.Add(station);

            var saved = await TrySaveAsync(cancellationToken);
            if (saved != null)
                return HmdResult<HmdStationResponse>.From(saved);

            return HmdResult<HmdStationResponse>.Created((await GetStationAsync(station.Id, cancellationToken))!);
        }

        public async Task<HmdResult<HmdStationResponse>> UpdateStationAsync(
            Guid id,
            UpdateHmdStationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var station = await _dbContext.HmdStations.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (station == null)
                return HmdResult<HmdStationResponse>.NotFound("Station tidak ditemukan atau sudah dihapus.");

            if (request.RoomId.HasValue && !await RoomExistsAsync(request.RoomId.Value, cancellationToken))
                return HmdResult<HmdStationResponse>.Invalid(HmdErrorCodes.InvalidRequest, "Ruang tidak ditemukan atau tidak aktif.");

            var code = request.StationCode.Trim();
            if (!string.Equals(code, station.StationCode, StringComparison.Ordinal) &&
                await _dbContext.HmdStations.AnyAsync(x => x.Id != id && x.ServiceUnitId == station.ServiceUnitId && x.StationCode == code, cancellationToken))
            {
                return HmdResult<HmdStationResponse>.Conflict(HmdErrorCodes.Val092, HmdMessages.Val092);
            }

            station.StationCode = code;
            station.StationName = request.StationName.Trim();
            station.RoomId = request.RoomId;
            station.IsIsolationStation = request.IsIsolationStation;
            station.IsActive = request.IsActive;
            station.UpdateDateTime = DateTime.UtcNow;
            station.UpdateBy = actorUserId;
            station.Version++;

            var saved = await TrySaveAsync(cancellationToken);
            if (saved != null)
                return HmdResult<HmdStationResponse>.From(saved);

            return HmdResult<HmdStationResponse>.Ok((await GetStationAsync(id, cancellationToken))!);
        }

        /// <summary>
        /// Mengubah status station. Perpindahan menjadi <c>Maintenance</c> atau <c>Blocked</c> ditolak
        /// bila station sedang dipakai sesi yang berlangsung (<c>BE-HMD-04</c> kriteria 3).
        /// </summary>
        public async Task<HmdResult<HmdStationResponse>> ChangeStationStatusAsync(
            Guid id,
            ChangeHmdStationStatusRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var reason = HmdServiceSupport.Normalize(request.Reason);
            if (reason == null)
                return HmdResult<HmdStationResponse>.Invalid(HmdErrorCodes.Val091, HmdMessages.Val091);

            var station = await _dbContext.HmdStations.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (station == null)
                return HmdResult<HmdStationResponse>.NotFound("Station tidak ditemukan atau sudah dihapus.");

            var from = station.StationStatus;
            if (!AllowedStationTransitions.Contains((from, request.ToStatus)))
            {
                return HmdResult<HmdStationResponse>.Rule(
                    HmdErrorCodes.InvalidTransition,
                    $"Status station tidak dapat diubah dari {HmdLabels.StationStatus(from)} menjadi {HmdLabels.StationStatus(request.ToStatus)}.");
            }

            if (from == HmdStationStatus.Available &&
                await _dbContext.HmdSessions.AnyAsync(x => x.StationId == id && !x.IsDelete && x.SessionStatus == HmdSessionStatus.InProgress, cancellationToken))
            {
                return HmdResult<HmdStationResponse>.Conflict(
                    HmdErrorCodes.Val090,
                    "Station sedang dipakai sesi yang berlangsung dan statusnya belum dapat diubah.");
            }

            var now = DateTime.UtcNow;
            station.StationStatus = request.ToStatus;
            station.StatusReason = reason;
            station.LastStatusChangedAt = now;
            station.UpdateDateTime = now;
            station.UpdateBy = actorUserId;
            station.Version++;

            var saved = await TrySaveAsync(cancellationToken);
            if (saved != null)
                return HmdResult<HmdStationResponse>.From(saved);

            return HmdResult<HmdStationResponse>.Ok((await GetStationAsync(id, cancellationToken))!);
        }

        public async Task<HmdResult<bool>> DeleteStationAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
        {
            var station = await _dbContext.HmdStations.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (station == null)
                return HmdResult<bool>.NotFound("Station tidak ditemukan atau sudah dihapus.");

            if (await _dbContext.HmdSessions.AnyAsync(x => x.StationId == id && !x.IsDelete && OpenSessionStatuses.Contains(x.SessionStatus), cancellationToken))
            {
                return HmdResult<bool>.Invalid(
                    HmdErrorCodes.InvalidRequest,
                    "Station tidak dapat dinonaktifkan karena masih dipakai sesi yang belum selesai.");
            }

            var now = DateTime.UtcNow;
            station.IsDelete = true;
            station.IsActive = false;
            station.DeleteDateTime = now;
            station.DeleteBy = actorUserId;
            station.Version++;

            var saved = await TrySaveAsync(cancellationToken);
            return saved != null ? HmdResult<bool>.From(saved) : HmdResult<bool>.Ok(true);
        }

        // =================================================================
        // Pengaturan unit
        // =================================================================

        public async Task<HmdSettingResponse?> GetSettingAsync(Guid serviceUnitId, CancellationToken cancellationToken) =>
            await _dbContext.HmdSettings.AsNoTracking()
                .Where(x => x.ServiceUnitId == serviceUnitId && !x.IsDelete)
                .Select(x => new HmdSettingResponse
                {
                    Id = x.Id,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    MaxPatientsPerNurse = x.MaxPatientsPerNurse,
                    EnforceNurseRatio = x.EnforceNurseRatio,
                    WaterResultValidityHours = x.WaterResultValidityHours,
                    EnforceCompetencyCheck = x.EnforceCompetencyCheck,
                    AllowMultipleActiveEpisodePerPatient = x.AllowMultipleActiveEpisodePerPatient,
                    SessionStartGraceMinutes = x.SessionStartGraceMinutes,
                    RequireDifferentSigner = x.RequireDifferentSigner,
                    ProcedureId = x.ProcedureId,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : null,
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy
                })
                .FirstOrDefaultAsync(cancellationToken);

        /// <summary>
        /// Memperbarui pengaturan unit HD. Nilainya berlaku seketika pada pemeriksaan berikutnya,
        /// karena setiap alur membaca pengaturan langsung dari basis data — tanpa restart aplikasi.
        /// </summary>
        /// <remarks>
        /// <b>Contoh.</b> Masa berlaku hasil air diubah dari 720 jam menjadi 1440 jam (60 hari).
        /// Lembar kesiapan shift berikutnya menilai hasil air berumur 40 hari sebagai masih
        /// berlaku; sebelum perubahan, hasil yang sama ditolak <c>422 HMD-VAL-102</c>.
        /// Bila unit belum punya baris pengaturan, baris baru dibentuk (PUT idempoten).
        /// </remarks>
        public async Task<HmdResult<HmdSettingResponse>> UpdateSettingAsync(
            Guid serviceUnitId,
            UpdateHmdSettingRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            if (!await ServiceUnitExistsAsync(serviceUnitId, cancellationToken))
                return HmdResult<HmdSettingResponse>.NotFound("Unit layanan tidak ditemukan atau tidak aktif.");

            if (request.ProcedureId.HasValue &&
                !await _dbContext.Set<MstProcedure>().AnyAsync(x => x.Id == request.ProcedureId.Value && !x.IsDelete && x.IsActive, cancellationToken))
            {
                return HmdResult<HmdSettingResponse>.Invalid(HmdErrorCodes.InvalidRequest, "Tindakan yang dipilih tidak ditemukan atau tidak aktif.");
            }

            var now = DateTime.UtcNow;
            var setting = await _dbContext.HmdSettings.FirstOrDefaultAsync(x => x.ServiceUnitId == serviceUnitId && !x.IsDelete, cancellationToken);

            if (setting == null)
            {
                setting = new HmdSetting { ServiceUnitId = serviceUnitId, CreateDateTime = now, CreateBy = actorUserId };
                _dbContext.HmdSettings.Add(setting);
            }
            else
            {
                setting.Version++;
            }

            setting.MaxPatientsPerNurse = request.MaxPatientsPerNurse;
            setting.EnforceNurseRatio = request.EnforceNurseRatio;
            setting.WaterResultValidityHours = request.WaterResultValidityHours;
            setting.EnforceCompetencyCheck = request.EnforceCompetencyCheck;
            setting.AllowMultipleActiveEpisodePerPatient = request.AllowMultipleActiveEpisodePerPatient;
            setting.SessionStartGraceMinutes = request.SessionStartGraceMinutes;
            setting.RequireDifferentSigner = request.RequireDifferentSigner;
            setting.ProcedureId = request.ProcedureId;
            setting.UpdateDateTime = now;
            setting.UpdateBy = actorUserId;

            var saved = await TrySaveAsync(cancellationToken);
            if (saved != null)
                return HmdResult<HmdSettingResponse>.From(saved);

            return HmdResult<HmdSettingResponse>.Ok((await GetSettingAsync(serviceUnitId, cancellationToken))!);
        }

        // =================================================================
        // Butir checklist Pra-HD
        // =================================================================

        public static HmdChecklistItemFilterMetadataResponse BuildChecklistItemFilterMetadata() => new()
        {
            CategoryOptions = HmdLabels.Options<HmdChecklistCategory>(HmdLabels.ChecklistCategory)
        };

        public async Task<HmdChecklistItemSummaryResponse> GetChecklistItemSummaryAsync(CancellationToken cancellationToken)
        {
            var rows = _dbContext.HmdChecklistItems.AsNoTracking().Where(x => !x.IsDelete);

            return new HmdChecklistItemSummaryResponse
            {
                TotalItem = await rows.CountAsync(cancellationToken),
                ActiveItem = await rows.CountAsync(x => x.IsActive, cancellationToken),
                InactiveItem = await rows.CountAsync(x => !x.IsActive, cancellationToken),
                MandatoryItem = await rows.CountAsync(x => x.IsMandatory, cancellationToken),
                OverridableItem = await rows.CountAsync(x => x.IsOverridable, cancellationToken)
            };
        }

        public async Task<List<HmdChecklistItemResponse>> GetChecklistItemsAsync(
            HmdChecklistItemQuery query,
            CancellationToken cancellationToken)
        {
            var rows = _dbContext.HmdChecklistItems.AsNoTracking().Where(x => !x.IsDelete);

            if (query.Category.HasValue)
                rows = rows.Where(x => x.Category == query.Category.Value);
            if (query.IsMandatory.HasValue)
                rows = rows.Where(x => x.IsMandatory == query.IsMandatory.Value);
            if (query.IsOverridable.HasValue)
                rows = rows.Where(x => x.IsOverridable == query.IsOverridable.Value);
            if (query.IsActive.HasValue)
                rows = rows.Where(x => x.IsActive == query.IsActive.Value);
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();
                rows = rows.Where(x => x.ItemCode.ToLower().Contains(keyword) || x.ItemName.ToLower().Contains(keyword));
            }

            var items = await ProjectChecklistItem(rows.OrderBy(x => x.CheckSequence).ThenBy(x => x.ItemCode))
                .ToListAsync(cancellationToken);
            items.ForEach(x => x.CategoryName = HmdLabels.ChecklistCategory(x.Category));
            return items;
        }

        public async Task<List<HmdChecklistItemOptionResponse>> GetChecklistItemOptionsAsync(
            bool onlyActive,
            CancellationToken cancellationToken) =>
            await _dbContext.HmdChecklistItems.AsNoTracking()
                .Where(x => !x.IsDelete && (!onlyActive || x.IsActive))
                .OrderBy(x => x.CheckSequence).ThenBy(x => x.ItemName)
                .Select(x => new HmdChecklistItemOptionResponse
                {
                    Id = x.Id,
                    ItemCode = x.ItemCode,
                    ItemName = x.ItemName,
                    Category = x.Category
                })
                .ToListAsync(cancellationToken);

        public async Task<HmdChecklistItemResponse?> GetChecklistItemAsync(Guid id, CancellationToken cancellationToken)
        {
            var row = await ProjectChecklistItem(_dbContext.HmdChecklistItems.AsNoTracking().Where(x => x.Id == id && !x.IsDelete))
                .FirstOrDefaultAsync(cancellationToken);

            if (row != null)
                row.CategoryName = HmdLabels.ChecklistCategory(row.Category);

            return row;
        }

        /// <summary>
        /// Menambah butir checklist. Butir baru selalu <b>tidak boleh dilewati</b>; nilainya hanya
        /// dapat diubah lewat <see cref="SetChecklistItemOverridableAsync"/> (<c>HMD-ASM-001</c>).
        /// </summary>
        public async Task<HmdResult<HmdChecklistItemResponse>> CreateChecklistItemAsync(
            CreateHmdChecklistItemRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var code = request.ItemCode.Trim().ToUpperInvariant();

            if (await _dbContext.HmdChecklistItems.AnyAsync(x => x.ItemCode == code, cancellationToken))
                return HmdResult<HmdChecklistItemResponse>.Conflict(HmdErrorCodes.Val092, HmdMessages.Val092);

            var now = DateTime.UtcNow;
            var item = new HmdChecklistItem
            {
                ItemCode = code,
                ItemName = request.ItemName.Trim(),
                Category = request.Category,
                IsMandatory = request.IsMandatory,
                IsOverridable = false,
                CheckSequence = request.CheckSequence,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.HmdChecklistItems.Add(item);

            var saved = await TrySaveAsync(cancellationToken);
            if (saved != null)
                return HmdResult<HmdChecklistItemResponse>.From(saved);

            return HmdResult<HmdChecklistItemResponse>.Created((await GetChecklistItemAsync(item.Id, cancellationToken))!);
        }

        public async Task<HmdResult<HmdChecklistItemResponse>> UpdateChecklistItemAsync(
            Guid id,
            UpdateHmdChecklistItemRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var item = await _dbContext.HmdChecklistItems.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (item == null)
                return HmdResult<HmdChecklistItemResponse>.NotFound("Butir checklist tidak ditemukan atau sudah dihapus.");

            item.ItemName = request.ItemName.Trim();
            item.Category = request.Category;
            item.IsMandatory = request.IsMandatory;
            item.CheckSequence = request.CheckSequence;
            item.IsActive = request.IsActive;
            item.UpdateDateTime = DateTime.UtcNow;
            item.UpdateBy = actorUserId;

            var saved = await TrySaveAsync(cancellationToken);
            if (saved != null)
                return HmdResult<HmdChecklistItemResponse>.From(saved);

            return HmdResult<HmdChecklistItemResponse>.Ok((await GetChecklistItemAsync(id, cancellationToken))!);
        }

        /// <summary>
        /// Menetapkan apakah butir boleh dilewati dokter — endpoint yang membuat <c>HMD-ASM-001</c>
        /// dapat dicabut tanpa mengubah kode.
        /// </summary>
        /// <remarks>
        /// Catatan tata kelola klinis wajib diisi dan tersimpan pada butirnya bersama pelaku dan
        /// waktu server, misalnya <c>{ "isOverridable": true, "clinicalGovernanceNote":
        /// "Keputusan Komite Medis No. 42" }</c>. Kewenangannya dijaga butir hak akses
        /// <c>HemodialysisChecklistItem : SetOverridable</c> pada controller; pengguna tanpa butir
        /// itu menerima <c>403</c> sebelum service ini dipanggil.
        /// </remarks>
        public async Task<HmdResult<HmdChecklistItemResponse>> SetChecklistItemOverridableAsync(
            Guid id,
            SetOverridableRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var note = HmdServiceSupport.Normalize(request.ClinicalGovernanceNote);
            if (note == null)
            {
                return HmdResult<HmdChecklistItemResponse>.Invalid(
                    HmdErrorCodes.InvalidRequest,
                    "Catatan keputusan tata kelola klinis wajib diisi ketika mengubah butir yang boleh dilewati.");
            }

            var item = await _dbContext.HmdChecklistItems.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (item == null)
                return HmdResult<HmdChecklistItemResponse>.NotFound("Butir checklist tidak ditemukan atau sudah dihapus.");

            var now = DateTime.UtcNow;
            item.IsOverridable = request.IsOverridable;
            item.OverridableDecisionNote = note;
            item.OverridableDecidedByUserId = actorUserId;
            item.OverridableDecidedAt = now;
            item.UpdateDateTime = now;
            item.UpdateBy = actorUserId;

            var saved = await TrySaveAsync(cancellationToken);
            if (saved != null)
                return HmdResult<HmdChecklistItemResponse>.From(saved);

            return HmdResult<HmdChecklistItemResponse>.Ok((await GetChecklistItemAsync(id, cancellationToken))!);
        }

        // =================================================================
        // Penolong
        // =================================================================

        private Task<bool> ServiceUnitExistsAsync(Guid serviceUnitId, CancellationToken cancellationToken) =>
            _dbContext.Set<MstServiceUnit>().AnyAsync(x => x.Id == serviceUnitId && !x.IsDelete && x.IsActive, cancellationToken);

        private Task<bool> RoomExistsAsync(Guid roomId, CancellationToken cancellationToken) =>
            _dbContext.Set<MstRoom>().AnyAsync(x => x.Id == roomId && !x.IsDelete && x.IsActive, cancellationToken);

        private static IQueryable<HmdMachineResponse> ProjectMachine(IQueryable<HmdMachine> rows) =>
            rows.Select(x => new HmdMachineResponse
            {
                Id = x.Id,
                MachineCode = x.MachineCode,
                MachineName = x.MachineName,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                Manufacturer = x.Manufacturer,
                SerialNumber = x.SerialNumber,
                MachineStatus = x.MachineStatus,
                DedicatedFor = x.DedicatedFor,
                IsSchedulable = x.IsSchedulable,
                IsActive = x.IsActive,
                LastStatusChangedAt = x.LastStatusChangedAt,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy,
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy
            });

        private static HmdMachineResponse Label(HmdMachineResponse row)
        {
            row.MachineStatusName = HmdLabels.MachineStatus(row.MachineStatus);
            row.DedicatedForName = HmdLabels.Isolation(row.DedicatedFor);
            return row;
        }

        private static IQueryable<HmdStationResponse> ProjectStation(IQueryable<HmdStation> rows) =>
            rows.Select(x => new HmdStationResponse
            {
                Id = x.Id,
                StationCode = x.StationCode,
                StationName = x.StationName,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                RoomId = x.RoomId,
                RoomName = x.Room != null ? x.Room.RoomName : null,
                StationStatus = x.StationStatus,
                StatusReason = x.StatusReason,
                LastStatusChangedAt = x.LastStatusChangedAt,
                IsIsolationStation = x.IsIsolationStation,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy,
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy
            });

        private static IQueryable<HmdChecklistItemResponse> ProjectChecklistItem(IQueryable<HmdChecklistItem> rows) =>
            rows.Select(x => new HmdChecklistItemResponse
            {
                Id = x.Id,
                ItemCode = x.ItemCode,
                ItemName = x.ItemName,
                Category = x.Category,
                IsMandatory = x.IsMandatory,
                IsOverridable = x.IsOverridable,
                OverridableDecisionNote = x.OverridableDecisionNote,
                OverridableDecidedByUserId = x.OverridableDecidedByUserId,
                OverridableDecidedAt = x.OverridableDecidedAt,
                CheckSequence = x.CheckSequence,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy
            });

        /// <summary>
        /// Menyimpan perubahan, lalu menerjemahkan bentrok token <c>Version</c> dan pelanggaran
        /// index unik menjadi <c>409</c> alih-alih galat server. <c>null</c> berarti tersimpan.
        /// </summary>
        private async Task<HmdResult<bool>?> TrySaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return null;
            }
            catch (DbUpdateConcurrencyException)
            {
                HmdServiceSupport.DetachFailedEntries(_dbContext);
                return HmdResult<bool>.Conflict(HmdErrorCodes.ConcurrencyConflict, HmdMessages.Val902);
            }
            catch (DbUpdateException exception) when (HmdServiceSupport.IsUniqueViolation(exception))
            {
                HmdServiceSupport.DetachFailedEntries(_dbContext);
                return HmdResult<bool>.Conflict(HmdErrorCodes.Val092, HmdMessages.Val092);
            }
        }
    }
}
