using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed class AssetAttachmentAccessService
{
    private readonly ApplicationDbContext _context;
    private readonly AssetActorResolver _actorResolver;
    private readonly AssetEntityResolver _entityResolver;
    private readonly AssetAttachmentService _attachmentService;
    private readonly AccessPermissionService _accessPermissionService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AssetAttachmentAccessService(
        ApplicationDbContext context,
        AssetActorResolver actorResolver,
        AssetEntityResolver entityResolver,
        AssetAttachmentService attachmentService,
        AccessPermissionService accessPermissionService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _actorResolver = actorResolver ?? throw new ArgumentNullException(nameof(actorResolver));
        _entityResolver = entityResolver ?? throw new ArgumentNullException(nameof(entityResolver));
        _attachmentService = attachmentService ?? throw new ArgumentNullException(nameof(attachmentService));
        _accessPermissionService = accessPermissionService ?? throw new ArgumentNullException(nameof(accessPermissionService));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public async Task<AssetAttachmentContent> OpenAsync(Guid attachmentId, CancellationToken cancellationToken = default)
    {
        var actor = await _actorResolver.GetCurrentAsync(cancellationToken);
        if (!actor.IsAuthenticated)
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.OutsideUnit, AssetRegisterMessages.OutsideUnit);
        }

        var attachment = await _context.AstAttachments
            .AsNoTracking()
            .Where(x => x.Id == attachmentId && !x.IsDelete)
            .Select(x => new { x.Id, x.OwnerType, x.OwnerId })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw AttachmentNotFound();

        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var allowed = attachment.OwnerType switch
        {
            AssetDocumentService.DocumentOwnerType => await CanOpenDocumentAsync(attachment.Id, attachment.OwnerId, actor, entity.LegalEntityId, cancellationToken),
            AssetUtilizationService.UtilizationOwnerType => await CanOpenUtilizationAsync(attachment.Id, attachment.OwnerId, actor, entity.LegalEntityId, cancellationToken),
            AssetConditionService.ConditionReportDocumentType => await CanOpenConditionReportAsync(attachment.Id, attachment.OwnerId, actor, entity.LegalEntityId, cancellationToken),
            _ => (bool?)null
        };

        if (allowed == null)
        {
            throw AttachmentNotFound();
        }

        if (allowed == false)
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.OutsideUnit, AssetRegisterMessages.OutsideUnit);
        }

        return await _attachmentService.OpenReadAsync(attachment.Id, actor.UserId, cancellationToken);
    }

    private async Task<bool?> CanOpenDocumentAsync(Guid attachmentId, Guid documentId, AssetActor actor, Guid legalEntityId, CancellationToken cancellationToken)
    {
        var document = await _context.AstAssetDocuments
            .AsNoTracking()
            .Where(x => x.Id == documentId && x.AttachmentId == attachmentId && !x.IsDelete && x.Asset!.LegalEntityId == legalEntityId)
            .Select(x => new { x.Asset!.CurrentOrganizationUnitId })
            .FirstOrDefaultAsync(cancellationToken);
        if (document == null)
        {
            return null;
        }

        return await HasAccessAsync(AssetDocumentService.PermissionResource) && actor.CanAccessUnit(document.CurrentOrganizationUnitId);
    }

    private async Task<bool?> CanOpenUtilizationAsync(Guid attachmentId, Guid utilizationId, AssetActor actor, Guid legalEntityId, CancellationToken cancellationToken)
    {
        var utilization = await _context.AstUtilizations
            .AsNoTracking()
            .Where(x => x.Id == utilizationId && x.AgreementAttachmentId == attachmentId && !x.IsDelete &&
                x.Items.Any(i => !i.IsDelete && i.Asset!.LegalEntityId == legalEntityId))
            .Select(x => new
            {
                x.CreateBy,
                UnitIds = x.Items.Where(i => !i.IsDelete).Select(i => i.Asset!.CurrentOrganizationUnitId).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (utilization == null)
        {
            return null;
        }

        var visible = actor.CanReadAllUnits || utilization.CreateBy == actor.UserId || utilization.UnitIds.Any(actor.CanAccessUnit);
        return await HasAccessAsync(AssetUtilizationService.PermissionResource) && visible;
    }

    private async Task<bool?> CanOpenConditionReportAsync(Guid attachmentId, Guid reportId, AssetActor actor, Guid legalEntityId, CancellationToken cancellationToken)
    {
        var report = await _context.AstConditionReports
            .AsNoTracking()
            .Where(x => x.Id == reportId && !x.IsDelete && x.Asset!.LegalEntityId == legalEntityId &&
                (x.PhotoAttachmentId == attachmentId || x.LossReportAttachmentId == attachmentId))
            .Select(x => new { x.ReportedBy, x.Asset!.CurrentOrganizationUnitId, x.Asset.CurrentCustodianEmployeeId })
            .FirstOrDefaultAsync(cancellationToken);
        if (report == null)
        {
            return null;
        }

        var visible = actor.CanAccessUnit(report.CurrentOrganizationUnitId) ||
            (actor.EmployeeId is { } employeeId && report.CurrentCustodianEmployeeId == employeeId) ||
            report.ReportedBy == actor.UserId;
        return await HasAccessAsync(AssetConditionService.PermissionResource) && visible;
    }

    private async Task<bool> HasAccessAsync(string resource)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user != null && await _accessPermissionService.HasAccessAsync(user, resource, "Read");
    }

    private static AssetManagementException AttachmentNotFound() =>
        AssetManagementException.NotFound(AssetRegisterErrorCodes.AttachmentNotFound, AssetRegisterMessages.AttachmentNotFound);
}
