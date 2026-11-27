using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Services;

/// <summary>
/// CRUD master dokumen wajib paket per metode <b>atau</b> per jenis pengadaan (BE-PBJ-PS-002; PBJ-ASM-007, PBJ-DEC-086).
/// </summary>
/// <remarks>
/// <para>
/// <b>Tepat satu cakupan.</b> Isian yang mengisi metode dan jenis sekaligus, atau tidak satu pun, ditolak 400 sesuai kontrak
/// <c>PBJ-PS-API</c> 1.0. Contoh sah: KAK/TOR × jenis Jasa, atau Dasar HPS × metode Penunjukan Langsung.
/// </para>
/// <para>
/// <b>Syarat aktif.</b> Jenis dokumen dan metode/jenis pengadaannya harus aktif, supaya paket yang memilihnya dapat
/// melengkapi checklist. Satu pasangan jenis dokumen × metode (atau × jenis pengadaan) hanya boleh ada satu di antara data
/// yang belum dihapus (409).
/// </para>
/// <para>
/// Dokumen wajib boleh dihapus kapan pun: paket menyalinnya menjadi checklist saat metode dipilih, sehingga paket yang
/// sudah berjalan tidak terpengaruh.
/// </para>
/// </remarks>
public sealed class ProcurementPackageDocumentRequirementService
{
    public const string MethodScope = "METHOD";
    public const string ProcurementTypeScope = "PROCUREMENT_TYPE";

    private const string LogCategory = "Corporate.ProcurementManagement.MasterData";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public ProcurementPackageDocumentRequirementService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public async Task<ProcurementPackageDocumentRequirementFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        new()
        {
            SortOptions =
            [
                new() { Value = "documentType", Label = "Jenis dokumen" },
                new() { Value = "scope", Label = "Metode / jenis pengadaan" },
                new() { Value = "isActive", Label = "Status aktif" }
            ],
            ScopeOptions =
            [
                new() { Value = MethodScope, Label = "Per metode pengadaan" },
                new() { Value = ProcurementTypeScope, Label = "Per jenis pengadaan" }
            ],
            DocumentTypeOptions = await _dbContext.MstProcurementPackageDocumentTypes.AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.DocumentTypeName)
                .Select(x => new ProcurementPackageDocumentTypeOptionResponse
                {
                    Id = x.Id, Code = x.DocumentTypeCode, Name = x.DocumentTypeName, Stage = x.Stage,
                    IsPriceSensitive = x.IsPriceSensitive, IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken),
            MethodOptions = await _dbContext.MstProcurementMethods.AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.MinHpsAmount.HasValue).ThenBy(x => x.MinHpsAmount).ThenBy(x => x.MethodName)
                .Select(x => new ProcurementMethodOptionResponse
                {
                    Id = x.Id, Code = x.MethodCode, Name = x.MethodName, MinHpsAmount = x.MinHpsAmount, MaxHpsAmount = x.MaxHpsAmount,
                    MinimumQuotationCount = x.MinimumQuotationCount, RequiresTechnicalEvaluation = x.RequiresTechnicalEvaluation,
                    IsSample = x.IsSample, IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken),
            ProcurementTypeOptions = await _dbContext.MstProcurementTypes.AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.TypeName)
                .Select(x => new ProcurementReferenceOptionResponse { Id = x.Id, Code = x.TypeCode, Name = x.TypeName, IsActive = x.IsActive })
                .ToListAsync(cancellationToken)
        };

    public async Task<ProcurementPackageDocumentRequirementSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementPackageDocumentRequirements.AsNoTracking().Where(x => !x.IsDelete);
        return new ProcurementPackageDocumentRequirementSummaryResponse
        {
            Total = await query.CountAsync(cancellationToken),
            Active = await query.CountAsync(x => x.IsActive, cancellationToken),
            Inactive = await query.CountAsync(x => !x.IsActive, cancellationToken),
            MethodScope = await query.CountAsync(x => x.MethodId != null, cancellationToken),
            ProcurementTypeScope = await query.CountAsync(x => x.ProcurementTypeId != null, cancellationToken)
        };
    }

    public async Task<PagedResult<ProcurementPackageDocumentRequirementResponse>> GetPagedAsync(
        ProcurementPackageDocumentRequirementQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementPackageDocumentRequirements.AsNoTracking().Where(x => !x.IsDelete);
        query = query.WhereCreatedWithin(ProcurementPeriodFilter.Resolve(request.StartDate, request.EndDate, request.CustomPeriod));
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (request.DocumentTypeId.HasValue) query = query.Where(x => x.DocumentTypeId == request.DocumentTypeId.Value);
        if (request.MethodId.HasValue) query = query.Where(x => x.MethodId == request.MethodId.Value);
        if (request.ProcurementTypeId.HasValue) query = query.Where(x => x.ProcurementTypeId == request.ProcurementTypeId.Value);
        var scope = request.Scope?.Trim().ToUpperInvariant();
        if (scope == MethodScope) query = query.Where(x => x.MethodId != null);
        else if (scope == ProcurementTypeScope) query = query.Where(x => x.ProcurementTypeId != null);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x =>
                x.DocumentType!.DocumentTypeCode.ToUpper().Contains(search) || x.DocumentType.DocumentTypeName.ToUpper().Contains(search)
                || (x.Method != null && (x.Method.MethodCode.ToUpper().Contains(search) || x.Method.MethodName.ToUpper().Contains(search)))
                || (x.ProcurementType != null && (x.ProcurementType.TypeCode.ToUpper().Contains(search) || x.ProcurementType.TypeName.ToUpper().Contains(search))));
        }

        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "scope" => descending
                ? query.OrderByDescending(x => x.Method != null ? x.Method.MethodName : x.ProcurementType!.TypeName).ThenBy(x => x.DocumentType!.DocumentTypeName)
                : query.OrderBy(x => x.Method != null ? x.Method.MethodName : x.ProcurementType!.TypeName).ThenBy(x => x.DocumentType!.DocumentTypeName),
            "isactive" => descending
                ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.DocumentType!.DocumentTypeName)
                : query.OrderBy(x => x.IsActive).ThenBy(x => x.DocumentType!.DocumentTypeName),
            _ => descending
                ? query.OrderByDescending(x => x.DocumentType!.DocumentTypeName).ThenBy(x => x.Method != null ? x.Method.MethodName : x.ProcurementType!.TypeName)
                : query.OrderBy(x => x.DocumentType!.DocumentTypeName).ThenBy(x => x.Method != null ? x.Method.MethodName : x.ProcurementType!.TypeName)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await Project(query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProcurementPackageDocumentRequirementResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = ProcurementMasterDataSupport.TotalPage(total, request.PageSize), Items = items
        };
    }

    public async Task<List<ProcurementPackageDocumentRequirementOptionResponse>> GetOptionsAsync(
        bool onlyActive, Guid? methodId, Guid? procurementTypeId, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementPackageDocumentRequirements.AsNoTracking().Where(x => !x.IsDelete);
        if (onlyActive) query = query.Where(x => x.IsActive);
        if (methodId.HasValue) query = query.Where(x => x.MethodId == methodId.Value);
        if (procurementTypeId.HasValue) query = query.Where(x => x.ProcurementTypeId == procurementTypeId.Value);

        return await query
            .OrderBy(x => x.DocumentType!.DocumentTypeName)
            .Select(x => new ProcurementPackageDocumentRequirementOptionResponse
            {
                Id = x.Id, DocumentTypeId = x.DocumentTypeId, DocumentTypeCode = x.DocumentType!.DocumentTypeCode,
                DocumentTypeName = x.DocumentType.DocumentTypeName, Scope = x.MethodId != null ? MethodScope : ProcurementTypeScope,
                MethodId = x.MethodId, ProcurementTypeId = x.ProcurementTypeId, IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ProcurementPackageDocumentRequirementResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await Project(_dbContext.MstProcurementPackageDocumentRequirements.AsNoTracking().Where(x => x.Id == id && !x.IsDelete))
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new KeyNotFoundException("Dokumen wajib paket tidak ditemukan atau sudah dihapus.");

    public async Task<ProcurementPackageDocumentRequirementResponse> CreateAsync(
        UpsertProcurementPackageDocumentRequirementRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        await ValidateAsync(request, null, cancellationToken);
        if (request.IsActive) await EnsureActivatableAsync(request.DocumentTypeId, request.MethodId, request.ProcurementTypeId, cancellationToken);
        var entity = new MstProcurementPackageDocumentRequirement
        {
            DocumentTypeId = request.DocumentTypeId, MethodId = request.MethodId, ProcurementTypeId = request.ProcurementTypeId,
            IsActive = request.IsActive, CreateDateTime = DateTime.UtcNow, CreateBy = actorUserId
        };
        _dbContext.MstProcurementPackageDocumentRequirements.Add(entity);
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementPackageDocumentRequirement.Create", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ProcurementPackageDocumentRequirementResponse> UpdateAsync(
        Guid id, UpsertProcurementPackageDocumentRequirementRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        await ValidateAsync(request, id, cancellationToken);
        if (request.IsActive) await EnsureActivatableAsync(request.DocumentTypeId, request.MethodId, request.ProcurementTypeId, cancellationToken);
        entity.DocumentTypeId = request.DocumentTypeId;
        entity.MethodId = request.MethodId;
        entity.ProcurementTypeId = request.ProcurementTypeId;
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementPackageDocumentRequirement.Update", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ProcurementPackageDocumentRequirementResponse> UpdateStatusAsync(
        Guid id, UpdateProcurementMasterStatusRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (request.IsActive && !entity.IsActive)
            await EnsureActivatableAsync(entity.DocumentTypeId, entity.MethodId, entity.ProcurementTypeId, cancellationToken);
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync(request.IsActive ? "ProcurementPackageDocumentRequirement.Activate" : "ProcurementPackageDocumentRequirement.Deactivate", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ProcurementMasterDeleteResponse> DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.MstProcurementPackageDocumentRequirements
            .Include(x => x.DocumentType).Include(x => x.Method).Include(x => x.ProcurementType)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Dokumen wajib paket tidak ditemukan atau sudah dihapus.");

        entity.IsDelete = true;
        entity.IsActive = false;
        entity.DeleteDateTime = DateTime.UtcNow;
        entity.DeleteBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementPackageDocumentRequirement.Delete", entity, actorUserId);
        return new ProcurementMasterDeleteResponse
        {
            Id = entity.Id,
            Code = entity.DocumentType?.DocumentTypeCode ?? string.Empty,
            Name = $"{entity.DocumentType?.DocumentTypeName} — {entity.Method?.MethodName ?? entity.ProcurementType?.TypeName}",
            IsDelete = true
        };
    }

    private async Task ValidateAsync(UpsertProcurementPackageDocumentRequirementRequest request, Guid? excludedId, CancellationToken cancellationToken)
    {
        var hasMethod = request.MethodId.HasValue && request.MethodId.Value != Guid.Empty;
        var hasType = request.ProcurementTypeId.HasValue && request.ProcurementTypeId.Value != Guid.Empty;
        if (hasMethod == hasType)
            throw new ProcurementMasterBadRequestException(
                "Isi tepat satu: metode pengadaan atau jenis pengadaan. Dokumen wajib berlaku per metode atau per jenis, tidak keduanya.");
        request.MethodId = hasMethod ? request.MethodId : null;
        request.ProcurementTypeId = hasType ? request.ProcurementTypeId : null;

        if (request.DocumentTypeId == Guid.Empty
            || !await _dbContext.MstProcurementPackageDocumentTypes.AnyAsync(x => x.Id == request.DocumentTypeId && !x.IsDelete, cancellationToken))
            throw new ProcurementMasterValidationException("Jenis dokumen wajib dipilih dari jenis dokumen paket yang ada.");
        if (hasMethod && !await _dbContext.MstProcurementMethods.AnyAsync(x => x.Id == request.MethodId && !x.IsDelete, cancellationToken))
            throw new ProcurementMasterValidationException("Metode wajib dipilih dari metode pengadaan yang ada.");
        if (hasType && !await _dbContext.MstProcurementTypes.AnyAsync(x => x.Id == request.ProcurementTypeId && !x.IsDelete, cancellationToken))
            throw new ProcurementMasterValidationException("Jenis pengadaan wajib dipilih dari jenis pengadaan yang ada.");

        if (await _dbContext.MstProcurementPackageDocumentRequirements.AnyAsync(
                x => !x.IsDelete && x.Id != excludedId && x.DocumentTypeId == request.DocumentTypeId
                     && x.MethodId == request.MethodId && x.ProcurementTypeId == request.ProcurementTypeId, cancellationToken))
            throw new ProcurementMasterConflictException(DuplicateMessage);
    }

    private async Task EnsureActivatableAsync(Guid documentTypeId, Guid? methodId, Guid? procurementTypeId, CancellationToken cancellationToken)
    {
        var documentType = await _dbContext.MstProcurementPackageDocumentTypes.AsNoTracking()
            .Where(x => x.Id == documentTypeId && !x.IsDelete)
            .Select(x => new { x.DocumentTypeName, x.IsActive })
            .FirstOrDefaultAsync(cancellationToken);
        if (documentType == null)
            throw new ProcurementMasterValidationException("Jenis dokumen sudah dihapus; dokumen wajib tidak dapat diaktifkan.");
        if (!documentType.IsActive)
            throw new ProcurementMasterValidationException(
                $"Jenis dokumen {documentType.DocumentTypeName} tidak aktif; aktifkan jenis dokumennya lebih dulu.");

        if (methodId.HasValue)
        {
            var method = await _dbContext.MstProcurementMethods.AsNoTracking()
                .Where(x => x.Id == methodId.Value && !x.IsDelete)
                .Select(x => new { x.MethodName, x.IsActive })
                .FirstOrDefaultAsync(cancellationToken);
            if (method == null)
                throw new ProcurementMasterValidationException("Metode sudah dihapus; dokumen wajib tidak dapat diaktifkan.");
            if (!method.IsActive)
                throw new ProcurementMasterValidationException($"Metode {method.MethodName} tidak aktif; aktifkan metodenya lebih dulu.");
        }

        if (procurementTypeId.HasValue)
        {
            var type = await _dbContext.MstProcurementTypes.AsNoTracking()
                .Where(x => x.Id == procurementTypeId.Value && !x.IsDelete)
                .Select(x => new { x.TypeName, x.IsActive })
                .FirstOrDefaultAsync(cancellationToken);
            if (type == null)
                throw new ProcurementMasterValidationException("Jenis pengadaan sudah dihapus; dokumen wajib tidak dapat diaktifkan.");
            if (!type.IsActive)
                throw new ProcurementMasterValidationException($"Jenis pengadaan {type.TypeName} tidak aktif; dokumen wajib hanya dapat diaktifkan untuk jenis aktif.");
        }
    }

    private const string DuplicateMessage = "Jenis dokumen ini sudah tercatat sebagai dokumen wajib untuk metode atau jenis pengadaan tersebut.";

    private IQueryable<ProcurementPackageDocumentRequirementResponse> Project(IQueryable<MstProcurementPackageDocumentRequirement> query) =>
        query.Select(x => new ProcurementPackageDocumentRequirementResponse
        {
            Id = x.Id,
            DocumentTypeId = x.DocumentTypeId, DocumentTypeCode = x.DocumentType!.DocumentTypeCode,
            DocumentTypeName = x.DocumentType.DocumentTypeName, DocumentStage = x.DocumentType.Stage,
            IsPriceSensitive = x.DocumentType.IsPriceSensitive,
            Scope = x.MethodId != null ? MethodScope : ProcurementTypeScope,
            MethodId = x.MethodId, MethodCode = x.Method != null ? x.Method.MethodCode : null,
            MethodName = x.Method != null ? x.Method.MethodName : null,
            ProcurementTypeId = x.ProcurementTypeId, ProcurementTypeCode = x.ProcurementType != null ? x.ProcurementType.TypeCode : null,
            ProcurementTypeName = x.ProcurementType != null ? x.ProcurementType.TypeName : null,
            IsActive = x.IsActive,
            CreateDateTime = x.CreateDateTime, CreateBy = x.CreateBy, UpdateDateTime = x.UpdateDateTime, UpdateBy = x.UpdateBy
        });

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception) when (ProcurementMasterDataSupport.IsUniqueViolation(exception))
        {
            throw new ProcurementMasterConflictException(DuplicateMessage);
        }
    }

    private async Task<MstProcurementPackageDocumentRequirement> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.MstProcurementPackageDocumentRequirements.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Dokumen wajib paket tidak ditemukan atau sudah dihapus.");

    private Task AuditAsync(string action, MstProcurementPackageDocumentRequirement entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan master dokumen wajib paket.", new
        {
            EntityId = entity.Id,
            entity.DocumentTypeId,
            entity.MethodId,
            entity.ProcurementTypeId,
            entity.IsActive,
            ActorUserId = actorUserId
        });
}
