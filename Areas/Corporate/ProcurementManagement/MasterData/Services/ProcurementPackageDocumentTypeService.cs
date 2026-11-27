using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Services;

/// <summary>
/// CRUD master jenis dokumen paket (BE-PBJ-PS-002; PBJ-ASM-007, PBJ-DEC-079). Kode disimpan huruf besar dan unik di
/// antara data yang belum dihapus.
/// </summary>
/// <remarks>
/// <para>
/// <b>Kode = kategori lampiran.</b> Lampiran paket (<c>PBJ_PACKAGE</c>, <c>PBJ_RFQ</c>) menyimpan kode jenis ini sebagai
/// <c>AttachmentCategory</c>, dan checklist paket terpenuhi menurut kode itu. Karena itu kode tidak dapat diganti setelah ada
/// lampiran berkategori kode tersebut (409), dan jenisnya tidak dapat dihapus (400 <c>PBJ-VAL-071</c>).
/// </para>
/// <para>
/// Jenis yang masih diwajibkan dokumen wajib aktif tidak dapat dinonaktifkan (409). Contoh: KAK/TOR diwajibkan untuk jenis
/// Jasa; bila jenisnya nonaktif, paket Jasa tidak bisa melengkapi checklist. Nonaktifkan dokumen wajibnya lebih dulu.
/// </para>
/// </remarks>
public sealed partial class ProcurementPackageDocumentTypeService
{
    private const string LogCategory = "Corporate.ProcurementManagement.MasterData";

    private static readonly string[] PackageAttachmentDocumentTypes = [ProcurementDocumentTypes.Package, ProcurementDocumentTypes.Rfq];

    private static readonly IReadOnlyDictionary<string, string> StageLabels = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        [ProcurementStatuses.PackageDocumentStage.Preparation] = "Persiapan",
        [ProcurementStatuses.PackageDocumentStage.Quotation] = "Penawaran",
        [ProcurementStatuses.PackageDocumentStage.Negotiation] = "Negosiasi"
    };

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public ProcurementPackageDocumentTypeService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    /// <summary>Label tahap berbahasa Indonesia. Contoh: <c>QUOTATION</c> → "Penawaran".</summary>
    public static string StageLabelOf(string? stage) =>
        stage != null && StageLabels.TryGetValue(stage, out var label) ? label : stage ?? string.Empty;

    public static List<ProcurementSourcingValueOptionResponse> StageOptions() =>
        StageLabels.Select(x => new ProcurementSourcingValueOptionResponse { Value = x.Key, Label = x.Value }).ToList();

    public Task<ProcurementPackageDocumentTypeFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new ProcurementPackageDocumentTypeFilterMetadataResponse
        {
            SortOptions =
            [
                new() { Value = "name", Label = "Nama" },
                new() { Value = "code", Label = "Kode" },
                new() { Value = "stage", Label = "Tahap" },
                new() { Value = "isActive", Label = "Status aktif" }
            ],
            StageOptions = StageOptions()
        });

    public async Task<ProcurementPackageDocumentTypeSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementPackageDocumentTypes.AsNoTracking().Where(x => !x.IsDelete);
        return new ProcurementPackageDocumentTypeSummaryResponse
        {
            Total = await query.CountAsync(cancellationToken),
            Active = await query.CountAsync(x => x.IsActive, cancellationToken),
            Inactive = await query.CountAsync(x => !x.IsActive, cancellationToken),
            PriceSensitive = await query.CountAsync(x => x.IsPriceSensitive, cancellationToken),
            Preparation = await query.CountAsync(x => x.Stage == ProcurementStatuses.PackageDocumentStage.Preparation, cancellationToken),
            Quotation = await query.CountAsync(x => x.Stage == ProcurementStatuses.PackageDocumentStage.Quotation, cancellationToken),
            Negotiation = await query.CountAsync(x => x.Stage == ProcurementStatuses.PackageDocumentStage.Negotiation, cancellationToken)
        };
    }

    public async Task<PagedResult<ProcurementPackageDocumentTypeResponse>> GetPagedAsync(
        ProcurementPackageDocumentTypeQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementPackageDocumentTypes.AsNoTracking().Where(x => !x.IsDelete);
        query = query.WhereCreatedWithin(ProcurementPeriodFilter.Resolve(request.StartDate, request.EndDate, request.CustomPeriod));
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (request.IsPriceSensitive.HasValue) query = query.Where(x => x.IsPriceSensitive == request.IsPriceSensitive.Value);
        if (!string.IsNullOrWhiteSpace(request.Stage))
        {
            var stage = request.Stage.Trim().ToUpperInvariant();
            query = query.Where(x => x.Stage == stage);
        }
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x => x.DocumentTypeCode.ToUpper().Contains(search) || x.DocumentTypeName.ToUpper().Contains(search));
        }

        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => descending ? query.OrderByDescending(x => x.DocumentTypeCode) : query.OrderBy(x => x.DocumentTypeCode),
            "stage" => descending
                ? query.OrderByDescending(x => x.Stage).ThenBy(x => x.DocumentTypeName)
                : query.OrderBy(x => x.Stage).ThenBy(x => x.DocumentTypeName),
            "isactive" => descending
                ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.DocumentTypeName)
                : query.OrderBy(x => x.IsActive).ThenBy(x => x.DocumentTypeName),
            _ => descending ? query.OrderByDescending(x => x.DocumentTypeName) : query.OrderBy(x => x.DocumentTypeName)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await Project(query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize))
            .ToListAsync(cancellationToken);
        items.ForEach(x => x.StageLabel = StageLabelOf(x.Stage));

        return new PagedResult<ProcurementPackageDocumentTypeResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = ProcurementMasterDataSupport.TotalPage(total, request.PageSize), Items = items
        };
    }

    public async Task<List<ProcurementPackageDocumentTypeOptionResponse>> GetOptionsAsync(
        bool onlyActive, string? stage, string? search, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementPackageDocumentTypes.AsNoTracking().Where(x => !x.IsDelete);
        if (onlyActive) query = query.Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(stage))
        {
            var normalizedStage = stage.Trim().ToUpperInvariant();
            query = query.Where(x => x.Stage == normalizedStage);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToUpper();
            query = query.Where(x => x.DocumentTypeCode.ToUpper().Contains(keyword) || x.DocumentTypeName.ToUpper().Contains(keyword));
        }

        return await query
            .OrderBy(x => x.DocumentTypeName)
            .Select(x => new ProcurementPackageDocumentTypeOptionResponse
            {
                Id = x.Id, Code = x.DocumentTypeCode, Name = x.DocumentTypeName, Stage = x.Stage,
                IsPriceSensitive = x.IsPriceSensitive, IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ProcurementPackageDocumentTypeResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await Project(_dbContext.MstProcurementPackageDocumentTypes.AsNoTracking().Where(x => x.Id == id && !x.IsDelete))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Jenis dokumen paket tidak ditemukan atau sudah dihapus.");
        response.StageLabel = StageLabelOf(response.Stage);
        return response;
    }

    public async Task<ProcurementPackageDocumentTypeResponse> CreateAsync(
        UpsertProcurementPackageDocumentTypeRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var (code, name, stage) = await ValidateAsync(request, null, cancellationToken);
        var entity = new MstProcurementPackageDocumentType
        {
            DocumentTypeCode = code, DocumentTypeName = name, Stage = stage, IsPriceSensitive = request.IsPriceSensitive,
            IsActive = request.IsActive, CreateDateTime = DateTime.UtcNow, CreateBy = actorUserId
        };
        _dbContext.MstProcurementPackageDocumentTypes.Add(entity);
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementPackageDocumentType.Create", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ProcurementPackageDocumentTypeResponse> UpdateAsync(
        Guid id, UpsertProcurementPackageDocumentTypeRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        var (code, name, stage) = await ValidateAsync(request, id, cancellationToken);
        if (!string.Equals(entity.DocumentTypeCode, code, StringComparison.Ordinal) && await IsUsedByAttachmentAsync(entity.DocumentTypeCode, cancellationToken))
            throw new ProcurementMasterConflictException(
                $"Kode {entity.DocumentTypeCode} sudah dipakai lampiran paket sebagai kategori; kode tidak dapat diganti. Buat jenis dokumen baru bila perlu kode lain.");
        if (entity.IsActive && !request.IsActive) await EnsureNotRequiredAsync(entity, cancellationToken);
        entity.DocumentTypeCode = code;
        entity.DocumentTypeName = name;
        entity.Stage = stage;
        entity.IsPriceSensitive = request.IsPriceSensitive;
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementPackageDocumentType.Update", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ProcurementPackageDocumentTypeResponse> UpdateStatusAsync(
        Guid id, UpdateProcurementMasterStatusRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity.IsActive && !request.IsActive) await EnsureNotRequiredAsync(entity, cancellationToken);
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync(request.IsActive ? "ProcurementPackageDocumentType.Activate" : "ProcurementPackageDocumentType.Deactivate", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ProcurementMasterDeleteResponse> DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (await _dbContext.MstProcurementPackageDocumentRequirements.AnyAsync(x => x.DocumentTypeId == id && !x.IsDelete, cancellationToken))
            throw new ProcurementMasterInUseException(
                "Jenis dokumen ini tidak dapat dihapus karena masih tercatat sebagai dokumen wajib paket. Hapus dokumen wajibnya, atau nonaktifkan jenis ini saja.");
        if (await IsUsedByAttachmentAsync(entity.DocumentTypeCode, cancellationToken))
            throw new ProcurementMasterInUseException(
                "Jenis dokumen ini tidak dapat dihapus karena sudah dipakai lampiran paket. Nonaktifkan saja bila tidak dipakai lagi.");

        entity.IsDelete = true;
        entity.IsActive = false;
        entity.DeleteDateTime = DateTime.UtcNow;
        entity.DeleteBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementPackageDocumentType.Delete", entity, actorUserId);
        return new ProcurementMasterDeleteResponse { Id = entity.Id, Code = entity.DocumentTypeCode, Name = entity.DocumentTypeName, IsDelete = true };
    }

    private Task<bool> IsUsedByAttachmentAsync(string code, CancellationToken cancellationToken) =>
        _dbContext.PrcAttachments.AnyAsync(
            x => !x.IsDelete && PackageAttachmentDocumentTypes.Contains(x.DocumentType) && x.AttachmentCategory == code, cancellationToken);

    private async Task EnsureNotRequiredAsync(MstProcurementPackageDocumentType entity, CancellationToken cancellationToken)
    {
        var scopes = await _dbContext.MstProcurementPackageDocumentRequirements.AsNoTracking()
            .Where(x => x.DocumentTypeId == entity.Id && x.IsActive && !x.IsDelete)
            .Select(x => x.Method != null ? x.Method.MethodName : x.ProcurementType!.TypeName)
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
        if (scopes.Count > 0)
            throw new ProcurementMasterConflictException(
                $"Jenis dokumen {entity.DocumentTypeName} masih diwajibkan untuk {string.Join(", ", scopes)}. Nonaktifkan dokumen wajibnya lebih dulu.");
    }

    private async Task<(string Code, string Name, string Stage)> ValidateAsync(
        UpsertProcurementPackageDocumentTypeRequest request, Guid? excludedId, CancellationToken cancellationToken)
    {
        var code = ProcurementMasterDataSupport.Required(request.Code, "Kode", 30).ToUpperInvariant();
        if (!CodePattern().IsMatch(code))
            throw new ProcurementMasterValidationException("Kode hanya boleh berisi huruf, angka, dan garis bawah. Contoh: KAK_TOR.");
        var name = ProcurementMasterDataSupport.Required(request.Name, "Nama", 150);
        var stage = ProcurementMasterDataSupport.Required(request.Stage, "Tahap", 20).ToUpperInvariant();
        if (!ProcurementStatuses.PackageDocumentStage.All.Contains(stage))
            throw new ProcurementMasterValidationException("Tahap harus PREPARATION (Persiapan), QUOTATION (Penawaran), atau NEGOTIATION (Negosiasi).");
        if (await _dbContext.MstProcurementPackageDocumentTypes.AnyAsync(
                x => !x.IsDelete && x.Id != excludedId && x.DocumentTypeCode == code, cancellationToken))
            throw new ProcurementMasterConflictException("Kode sudah dipakai jenis dokumen paket lain. Gunakan kode yang berbeda.");
        return (code, name, stage);
    }

    [GeneratedRegex("^[A-Z0-9_]+$")]
    private static partial Regex CodePattern();

    private IQueryable<ProcurementPackageDocumentTypeResponse> Project(IQueryable<MstProcurementPackageDocumentType> query) =>
        query.Select(x => new ProcurementPackageDocumentTypeResponse
        {
            Id = x.Id, Code = x.DocumentTypeCode, Name = x.DocumentTypeName, Stage = x.Stage, IsPriceSensitive = x.IsPriceSensitive,
            IsActive = x.IsActive,
            ActiveRequirementCount = _dbContext.MstProcurementPackageDocumentRequirements
                .Count(r => r.DocumentTypeId == x.Id && r.IsActive && !r.IsDelete),
            CreateDateTime = x.CreateDateTime, CreateBy = x.CreateBy, UpdateDateTime = x.UpdateDateTime, UpdateBy = x.UpdateBy
        });

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception) when (ProcurementMasterDataSupport.IsUniqueViolation(exception))
        {
            throw new ProcurementMasterConflictException("Kode sudah dipakai jenis dokumen paket lain. Gunakan kode yang berbeda.");
        }
    }

    private async Task<MstProcurementPackageDocumentType> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.MstProcurementPackageDocumentTypes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Jenis dokumen paket tidak ditemukan atau sudah dihapus.");

    private Task AuditAsync(string action, MstProcurementPackageDocumentType entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan master jenis dokumen paket.", new
        {
            EntityId = entity.Id,
            entity.DocumentTypeCode,
            entity.Stage,
            entity.IsPriceSensitive,
            entity.IsActive,
            ActorUserId = actorUserId
        });
}
