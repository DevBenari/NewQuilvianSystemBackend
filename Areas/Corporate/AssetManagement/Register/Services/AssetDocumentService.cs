using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed partial class AssetDocumentService
{
    public const string DocumentOwnerType = "AstAssetDocument";
    public const string PermissionResource = "AssetDocument";
    private const string LogCategory = "Corporate.AssetManagement.Document";
    private const int MaxDocumentNumberLength = 100;
    private const int MaxHolderNameLength = 200;
    private const int MaxNotesLength = 500;
    private const int MaxPageSize = 100;

    private readonly ApplicationDbContext _context;
    private readonly AssetActorResolver _actorResolver;
    private readonly AssetEntityResolver _entityResolver;
    private readonly AssetAttachmentService _attachmentService;
    private readonly AccessPermissionService _accessPermissionService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;
    private DocumentPermissions? _permissions;

    public AssetDocumentService(
        ApplicationDbContext context,
        AssetActorResolver actorResolver,
        AssetEntityResolver entityResolver,
        AssetAttachmentService attachmentService,
        AccessPermissionService accessPermissionService,
        IHttpContextAccessor httpContextAccessor,
        LoggerService loggerService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _actorResolver = actorResolver ?? throw new ArgumentNullException(nameof(actorResolver));
        _entityResolver = entityResolver ?? throw new ArgumentNullException(nameof(entityResolver));
        _attachmentService = attachmentService ?? throw new ArgumentNullException(nameof(attachmentService));
        _accessPermissionService = accessPermissionService ?? throw new ArgumentNullException(nameof(accessPermissionService));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
    }

    public async Task<AssetDocumentResponse> CreateAsync(AssetDocumentUpsertRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        if (request.AssetId is not { } assetId || assetId == Guid.Empty || request.DocumentTypeId is not { } documentTypeId || documentTypeId == Guid.Empty)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, AssetRegisterMessages.DocumentRequired);
        }

        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var fields = await ValidateFieldsAsync(request, documentTypeId, cancellationToken);

        var id = await InTransactionAsync(async () =>
        {
            var asset = await LoadAssetAsync(assetId, entity.LegalEntityId, cancellationToken);
            actor.EnsureCanAccessUnit(asset.CurrentOrganizationUnitId);
            EnsureAssetEditable(asset);

            var now = DateTime.UtcNow;
            var document = new AstAssetDocument
            {
                AssetId = asset.Id,
                CreateBy = actor.UserId,
                CreateDateTime = now
            };
            Apply(document, fields);

            if (request.File is { Length: > 0 })
            {
                var attachment = await _attachmentService.SaveAsync(request.File, DocumentOwnerType, document.Id, actor.UserId, cancellationToken);
                document.AttachmentId = attachment.Id;
            }

            _context.AstAssetDocuments.Add(document);
            await _context.SaveChangesAsync(cancellationToken);
            return document.Id;
        }, cancellationToken);

        await AuditAsync("AssetDocument.Create", $"Dokumen kepemilikan {id} ditambahkan pada aset {assetId} oleh {actor.UserId}.");
        return await GetDocumentAsync(id, cancellationToken);
    }

    public async Task<AssetDocumentResponse> UpdateAsync(Guid id, AssetDocumentUpsertRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        if (request.DocumentTypeId is not { } documentTypeId || documentTypeId == Guid.Empty)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, AssetRegisterMessages.DocumentRequired);
        }

        var entity = await _entityResolver.ResolveAsync(cancellationToken);

        await InTransactionAsync(async () =>
        {
            var document = await _context.AstAssetDocuments
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw DocumentNotFound();
            if (request.AssetId is { } assetId && assetId != Guid.Empty && assetId != document.AssetId)
            {
                throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, AssetRegisterMessages.DocumentAssetMismatch);
            }

            var asset = await LoadAssetAsync(document.AssetId, entity.LegalEntityId, cancellationToken);
            actor.EnsureCanAccessUnit(asset.CurrentOrganizationUnitId);
            EnsureAssetEditable(asset);

            var fields = await ValidateFieldsAsync(request, documentTypeId, cancellationToken, document.DocumentTypeId);
            Apply(document, fields);

            if (request.File is { Length: > 0 })
            {
                var attachment = await _attachmentService.SaveAsync(request.File, DocumentOwnerType, document.Id, actor.UserId, cancellationToken);
                document.AttachmentId = attachment.Id;
            }

            document.UpdateBy = actor.UserId;
            document.UpdateDateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return document.Id;
        }, cancellationToken);

        await AuditAsync("AssetDocument.Update", $"Dokumen kepemilikan {id} diubah oleh {actor.UserId}.");
        return await GetDocumentAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);

        var document = await _context.AstAssetDocuments
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw DocumentNotFound();
        var asset = await LoadAssetAsync(document.AssetId, entity.LegalEntityId, cancellationToken);
        actor.EnsureCanAccessUnit(asset.CurrentOrganizationUnitId);

        document.IsDelete = true;
        document.DeleteBy = actor.UserId;
        document.DeleteDateTime = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await AuditAsync("AssetDocument.Delete", $"Dokumen kepemilikan {id} dihapus oleh {actor.UserId}.");
        return true;
    }

    private async Task<DocumentFields> ValidateFieldsAsync(
        AssetDocumentUpsertRequest request,
        Guid documentTypeId,
        CancellationToken cancellationToken,
        Guid? currentDocumentTypeId = null)
    {
        var documentType = await _context.MstAssetDocumentTypes
            .AsNoTracking()
            .Where(x => x.Id == documentTypeId && !x.IsDelete)
            .Select(x => new { x.Id, x.HasExpiry, x.IsActive })
            .FirstOrDefaultAsync(cancellationToken);
        if (documentType == null || (!documentType.IsActive && documentType.Id != currentDocumentTypeId))
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.DocumentTypeInactive, AssetRegisterMessages.DocumentTypeInactive);
        }

        var number = request.DocumentNumber?.Trim();
        if (string.IsNullOrEmpty(number) || (documentType.HasExpiry && request.ExpiryDate == null))
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.DocumentFields, AssetRegisterMessages.DocumentFields);
        }

        var holder = string.IsNullOrWhiteSpace(request.HolderName) ? null : request.HolderName.Trim();
        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        if (number.Length > MaxDocumentNumberLength || holder is { Length: > MaxHolderNameLength } || notes is { Length: > MaxNotesLength })
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.DocumentFields, AssetRegisterMessages.DocumentFieldLength);
        }

        if (request.IssuedDate is { } issued && request.ExpiryDate is { } expiry && issued > expiry)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.DocumentFields, AssetRegisterMessages.DocumentDates);
        }

        return new DocumentFields(documentType.Id, number, request.IssuedDate, request.ExpiryDate, holder, notes);
    }

    private static void Apply(AstAssetDocument document, DocumentFields fields)
    {
        document.DocumentTypeId = fields.DocumentTypeId;
        document.DocumentNumber = fields.DocumentNumber;
        document.IssuedDate = fields.IssuedDate;
        document.ExpiryDate = fields.ExpiryDate;
        document.HolderName = fields.HolderName;
        document.Notes = fields.Notes;
    }

    private static void EnsureAssetEditable(AstAsset asset)
    {
        if (asset.RecordStatus != AssetRecordStatus.Terdaftar)
        {
            throw AssetManagementException.Conflict(
                AssetRegisterErrorCodes.Status,
                string.Format(AssetRegisterMessages.DocumentAssetStatusFormat, AssetRegisterLabels.RecordStatus(asset.RecordStatus).ToLowerInvariant()));
        }
    }

    private async Task<AstAsset> LoadAssetAsync(Guid assetId, Guid legalEntityId, CancellationToken cancellationToken) =>
        await _context.AstAssets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == assetId && !x.IsDelete && x.LegalEntityId == legalEntityId, cancellationToken)
        ?? throw AssetManagementException.NotFound(AssetRegisterErrorCodes.NotFound, AssetRegisterMessages.NotFound);

    private async Task<AssetActor> RequireActorAsync(CancellationToken cancellationToken)
    {
        var actor = await _actorResolver.GetCurrentAsync(cancellationToken);
        if (!actor.IsAuthenticated)
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.OutsideUnit, AssetRegisterMessages.OutsideUnit);
        }

        return actor;
    }

    private async Task<DocumentPermissions> GetPermissionsAsync()
    {
        if (_permissions != null)
        {
            return _permissions;
        }

        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            return _permissions = new DocumentPermissions(false, false);
        }

        var canUpdate = await _accessPermissionService.HasAccessAsync(user, PermissionResource, "Update");
        var canDelete = await _accessPermissionService.HasAccessAsync(user, PermissionResource, "Delete");
        return _permissions = new DocumentPermissions(canUpdate, canDelete);
    }

    private async Task<T> InTransactionAsync<T>(Func<Task<T>> work, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await work();
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    private Task AuditAsync(string action, string message) =>
        _loggerService.AuditAsync(LogCategory, action, message);

    private static AssetManagementException DocumentNotFound() =>
        AssetManagementException.NotFound(AssetRegisterErrorCodes.DocumentNotFound, AssetRegisterMessages.DocumentNotFound);

    private sealed record DocumentFields(Guid DocumentTypeId, string DocumentNumber, DateOnly? IssuedDate, DateOnly? ExpiryDate, string? HolderName, string? Notes);

    private sealed record DocumentPermissions(bool CanUpdate, bool CanDelete);
}
