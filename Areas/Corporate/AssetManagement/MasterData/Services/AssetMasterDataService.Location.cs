using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;

public sealed partial class AssetMasterDataService
{
    private const string LocationLabel = "Lokasi aset";

    private static readonly Expression<Func<MstAssetLocation, AssetLocationResponse>> LocationProjection = x => new AssetLocationResponse
    {
        Id = x.Id,
        LocationCode = x.LocationCode,
        LocationName = x.LocationName,
        OrganizationUnitId = x.OrganizationUnitId,
        OrganizationUnitName = x.OrganizationUnit!.UnitName,
        BuildingName = x.BuildingName,
        FloorName = x.FloorName,
        RoomName = x.RoomName,
        RoomId = x.RoomId,
        LinkedRoomName = x.Room != null ? x.Room.RoomName : null,
        IsActive = x.IsActive,
        CreateDateTime = x.CreateDateTime,
        CreateBy = x.CreateBy,
        UpdateDateTime = x.UpdateDateTime,
        UpdateBy = x.UpdateBy
    };

    public Task<AssetMasterFilterMetadataResponse> GetLocationMetadataAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(BuildMetadata(
            "Kode Lokasi",
            "Nama Lokasi",
            [new() { Value = "organizationUnit", Label = "Unit Pemilik" }],
            [GuidParameter("organizationUnitId", "Menyaring lokasi milik satu unit organisasi")],
            [],
            [
                Field("locationCode", "Kode Lokasi", "text", true, 1, MaxCodeLength, example: "IGD-RESUS"),
                Field("locationName", "Nama Lokasi", "text", true, 2, MaxNameLength, example: "Ruang Resusitasi IGD"),
                Field("organizationUnitId", "Unit Pemilik", "select", true, 3, optionsSource: "organization-units",
                    description: "Unit organisasi pemilik ruangan"),
                Field("buildingName", "Gedung", "text", false, 4, 100, example: "Gedung A"),
                Field("floorName", "Lantai", "text", false, 5, 50, example: "Lantai 1"),
                Field("roomName", "Ruangan", "text", false, 6, 100, example: "Resusitasi"),
                Field("roomId", "Tautan Ruangan Klinis", "select", false, 7, optionsSource: "rooms",
                    description: "Opsional; menautkan lokasi ke data ruangan klinis tanpa menyalinnya"),
                Field("isActive", "Aktif", "switch", true, 8)
            ]));

    public async Task<AssetLocationSummaryResponse> GetLocationSummaryAsync(CancellationToken cancellationToken = default)
    {
        var query = _context.MstAssetLocations.AsNoTracking().Where(x => !x.IsDelete);
        return new AssetLocationSummaryResponse
        {
            TotalLocation = await query.CountAsync(cancellationToken),
            ActiveLocation = await query.CountAsync(x => x.IsActive, cancellationToken),
            InactiveLocation = await query.CountAsync(x => !x.IsActive, cancellationToken),
            LinkedRoomLocation = await query.CountAsync(x => x.RoomId != null, cancellationToken)
        };
    }

    public Task<PagedResult<AssetLocationResponse>> GetLocationsAsync(AssetMasterPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var (pageNumber, pageSize, descending, sortBy) = NormalizePaging(request);
        var query = _context.MstAssetLocations.AsNoTracking().Where(x => !x.IsDelete);

        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (request.OrganizationUnitId.HasValue) query = query.Where(x => x.OrganizationUnitId == request.OrganizationUnitId.Value);

        var search = NormalizeSearch(request.Search);
        if (search != null)
        {
            query = query.Where(x => x.LocationCode.ToUpper().Contains(search) || x.LocationName.ToUpper().Contains(search));
        }

        query = sortBy switch
        {
            "code" => Sort(query, x => x.LocationCode, descending),
            "isactive" => Sort(query, x => x.IsActive, descending),
            "createdatetime" => Sort(query, x => x.CreateDateTime, descending),
            "organizationunit" => Sort(query, x => x.OrganizationUnit!.UnitName, descending),
            _ => Sort(query, x => x.LocationName, descending)
        };

        return ToPagedAsync(query, pageNumber, pageSize, x => x.Select(LocationProjection), cancellationToken);
    }

    public async Task<List<AssetMasterOptionResponse>> GetLocationOptionsAsync(
        bool onlyActive,
        string? search,
        Guid? organizationUnitId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MstAssetLocations.AsNoTracking().Where(x => !x.IsDelete);
        if (onlyActive) query = query.Where(x => x.IsActive);
        if (organizationUnitId.HasValue) query = query.Where(x => x.OrganizationUnitId == organizationUnitId.Value);

        var keyword = NormalizeSearch(search);
        if (keyword != null)
        {
            query = query.Where(x => x.LocationCode.ToUpper().Contains(keyword) || x.LocationName.ToUpper().Contains(keyword));
        }

        return await query
            .OrderBy(x => x.LocationName)
            .Select(x => new AssetMasterOptionResponse { Id = x.Id, Code = x.LocationCode, Name = x.LocationName, IsActive = x.IsActive })
            .ToListAsync(cancellationToken);
    }

    public async Task<AssetLocationResponse> GetLocationAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.MstAssetLocations
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDelete)
            .Select(LocationProjection)
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw NotFound(LocationLabel);

    public async Task<AssetLocationResponse> CreateLocationAsync(AssetLocationUpsertRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = new MstAssetLocation();
        await ApplyLocationAsync(entity, request, null, cancellationToken);
        StampCreate(entity, actorUserId);

        _context.MstAssetLocations.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetLocation.Create", new { entity.Id, entity.LocationCode, entity.OrganizationUnitId, entity.IsActive }, actorUserId);
        return await GetLocationAsync(entity.Id, cancellationToken);
    }

    public async Task<AssetLocationResponse> UpdateLocationAsync(Guid id, AssetLocationUpsertRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await FindLocationAsync(id, cancellationToken);
        await ApplyLocationAsync(entity, request, id, cancellationToken);
        StampUpdate(entity, actorUserId);

        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetLocation.Update", new { entity.Id, entity.LocationCode, entity.OrganizationUnitId, entity.IsActive }, actorUserId);
        return await GetLocationAsync(id, cancellationToken);
    }

    public async Task<AssetLocationResponse> UpdateLocationStatusAsync(Guid id, AssetMasterStatusRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await FindLocationAsync(id, cancellationToken);
        entity.IsActive = request.IsActive;
        StampUpdate(entity, actorUserId);

        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync(request.IsActive ? "AssetLocation.Activate" : "AssetLocation.Deactivate", new { entity.Id, entity.LocationCode }, actorUserId);
        return await GetLocationAsync(id, cancellationToken);
    }

    public async Task<AssetMasterDeleteResponse> DeleteLocationAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var entity = await FindLocationAsync(id, cancellationToken);

        if (await _context.AstAssets.AnyAsync(x => x.CurrentLocationId == id, cancellationToken) ||
            await _context.AstAssetPlacements.AnyAsync(x => x.LocationId == id, cancellationToken) ||
            await _context.AstAssetHandovers.AnyAsync(x => x.TargetLocationId == id, cancellationToken))
        {
            throw InUse("Lokasi ini tidak dapat dihapus karena sudah dipakai aset, riwayat penempatan, atau penyerahan. Nonaktifkan saja bila tidak dipakai lagi.");
        }

        entity.IsActive = false;
        StampDelete(entity, actorUserId);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetLocation.Delete", new { entity.Id, entity.LocationCode }, actorUserId);
        return new AssetMasterDeleteResponse { Id = entity.Id, Code = entity.LocationCode, Name = entity.LocationName, IsDelete = true };
    }

    private async Task ApplyLocationAsync(MstAssetLocation entity, AssetLocationUpsertRequest request, Guid? currentId, CancellationToken cancellationToken)
    {
        var code = RequiredCode(request.LocationCode, "Kode lokasi");
        var name = RequiredText(request.LocationName, MaxNameLength, "Nama lokasi");
        var buildingName = OptionalText(request.BuildingName, 100, "Gedung");
        var floorName = OptionalText(request.FloorName, 50, "Lantai");
        var roomName = OptionalText(request.RoomName, 100, "Ruangan");

        if (request.OrganizationUnitId is not { } organizationUnitId || organizationUnitId == Guid.Empty ||
            !await _context.MstOrganizationUnits.AnyAsync(x => x.Id == organizationUnitId && x.IsActive && !x.IsDelete, cancellationToken))
        {
            throw AssetManagementException.Invalid(AssetMasterErrorCodes.LocationUnit, AssetMasterMessages.LocationUnit);
        }

        var roomId = request.RoomId is { } room && room != Guid.Empty ? room : (Guid?)null;
        if (roomId.HasValue && !await _context.MstRooms.AnyAsync(x => x.Id == roomId.Value && !x.IsDelete, cancellationToken))
        {
            throw AssetManagementException.Invalid(AssetMasterErrorCodes.Reference, "Ruangan klinis yang ditautkan tidak ditemukan.");
        }

        if (await _context.MstAssetLocations.AnyAsync(x => !x.IsDelete && x.Id != (currentId ?? Guid.Empty) && x.LocationCode == code, cancellationToken))
        {
            throw DuplicateCode();
        }

        entity.LocationCode = code;
        entity.LocationName = name;
        entity.OrganizationUnitId = organizationUnitId;
        entity.BuildingName = buildingName;
        entity.FloorName = floorName;
        entity.RoomName = roomName;
        entity.RoomId = roomId;
        entity.IsActive = request.IsActive;
    }

    private async Task<MstAssetLocation> FindLocationAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.MstAssetLocations.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw NotFound(LocationLabel);
}
