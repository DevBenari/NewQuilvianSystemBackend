using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using static QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services.OperatingRoomCommandSupport;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;

/// <summary>
/// Pemetaan kamar operasi ke depo farmasi yang menjadi sumber stoknya.
/// </summary>
/// <remarks>
/// Pemetaan ini adalah satu-satunya tempat keputusan "stok siapa yang berkurang" dijawab.
/// Ia dibuat sebagai data supaya rumah sakit dapat berpindah dari Depo Rawat Inap ke Depo OK
/// tanpa perubahan kode maupun desain transaksinya.
/// </remarks>
public sealed class OperatingRoomStockSourceService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;

    public OperatingRoomStockSourceService(ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
    }

    public Task<List<OprStockSourceResponse>> ListAsync(bool includeInactive,
        CancellationToken cancellationToken = default) =>
        _dbContext.OprStockSources.AsNoTracking()
            .Where(x => !x.IsDelete && (includeInactive || x.IsActive))
            .OrderBy(x => x.Room!.RoomName)
            .Select(x => new OprStockSourceResponse
            {
                Id = x.Id,
                RoomId = x.RoomId,
                RoomName = x.Room!.RoomName,
                StorageLocationId = x.StorageLocationId,
                StorageLocationName = x.StorageLocation!.StorageLocationName,
                IsActive = x.IsActive,
                Note = x.Note
            })
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Menetapkan atau memperbarui depo sumber satu kamar.
    /// </summary>
    /// <remarks>
    /// Pemetaan lama dinonaktifkan, bukan dihapus. Pemakaian yang sudah dibukukan harus tetap
    /// dapat dijelaskan dengan pemetaan yang berlaku saat itu.
    /// </remarks>
    public async Task<OprStockSourceResponse> SaveAsync(SaveOprStockSourceRequest request,
        CancellationToken cancellationToken = default)
    {
        var actorUserId = GetUserId(_httpContextAccessor);

        var roomName = await _dbContext.MstRooms.AsNoTracking()
            .Where(x => x.Id == request.RoomId && !x.IsDelete)
            .Select(x => x.RoomName)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Kamar operasi tidak ditemukan.");

        var location = await _dbContext.MstDrugStorageLocations.AsNoTracking()
            .Where(x => x.Id == request.StorageLocationId && !x.IsDelete)
            .Select(x => new
            {
                x.StorageLocationName, x.StorageLocationType, x.IsActive,
                x.IsAllowDispensing, x.IsQuarantineLocation
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Depo farmasi tidak ditemukan.");

        if (!location.IsActive)
            throw new OperatingRoomUnprocessableException("OPR-INV-011",
                "Depo yang dipilih tidak aktif.");

        // Depo sumber harus boleh melayankan obat. Lokasi karantina menyimpan barang yang
        // justru sedang ditahan dari pelayanan; memakainya sebagai sumber akan mengeluarkan
        // barang yang belum dinyatakan layak.
        //
        // Karantina diperiksa dari dua sisi — bendera dan jenis lokasinya — karena keduanya
        // dapat berbeda pada data yang sudah ada: ditemukan lokasi berjenis `Quarantine` yang
        // benderanya justru tidak menyala. Memeriksa satu sisi saja akan meloloskannya.
        var isQuarantine = location.IsQuarantineLocation ||
            string.Equals(location.StorageLocationType, "Quarantine", StringComparison.OrdinalIgnoreCase);

        if (!location.IsAllowDispensing || isQuarantine)
            throw new OperatingRoomUnprocessableException("OPR-INV-012",
                "Depo yang dipilih tidak boleh melayankan obat, sehingga tidak dapat menjadi " +
                "sumber stok kamar operasi.");

        var now = DateTime.UtcNow;

        var existing = await _dbContext.OprStockSources
            .Where(x => x.RoomId == request.RoomId && x.IsActive && !x.IsDelete)
            .ToListAsync(cancellationToken);

        var entity = existing.FirstOrDefault(x => x.StorageLocationId == request.StorageLocationId);

        foreach (var stale in existing.Where(x => x.Id != entity?.Id))
        {
            stale.IsActive = false;
            stale.UpdateDateTime = now;
            stale.UpdateBy = actorUserId;
        }

        if (entity == null)
        {
            entity = new OprStockSource
            {
                RoomId = request.RoomId,
                StorageLocationId = request.StorageLocationId,
                IsActive = request.IsActive,
                Note = Normalize(request.Note),
                CreateDateTime = now,
                CreateBy = actorUserId
            };
            _dbContext.OprStockSources.Add(entity);
        }
        else
        {
            entity.IsActive = request.IsActive;
            entity.Note = Normalize(request.Note);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomStockSource.Save",
            "Menetapkan depo farmasi sumber stok kamar operasi.",
            new
            {
                ActorUserId = actorUserId, entity.Id, entity.RoomId, RoomName = roomName,
                entity.StorageLocationId, location.StorageLocationName, entity.IsActive
            });

        return new OprStockSourceResponse
        {
            Id = entity.Id,
            RoomId = entity.RoomId,
            RoomName = roomName,
            StorageLocationId = entity.StorageLocationId,
            StorageLocationName = location.StorageLocationName,
            IsActive = entity.IsActive,
            Note = entity.Note
        };
    }
}
