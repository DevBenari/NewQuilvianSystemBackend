using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed class AssetImportOptions
{
    public const string SectionName = "AssetManagement:Import";

    public int MaxRows { get; set; } = 5000;
}

public sealed partial class AssetImportService
{
    public const string ImportOwnerType = "AstImportBatch";
    public const string PermissionResource = "AssetImport";
    public const string PlacementSourceImport = "IMPOR";
    private const string LogCategory = "Corporate.AssetManagement.Import";
    private const int MaxReasonLength = 500;
    private const int MaxErrorLength = 2000;
    private const int MaxPageSize = 100;

    private readonly ApplicationDbContext _context;
    private readonly AssetActorResolver _actorResolver;
    private readonly AssetEntityResolver _entityResolver;
    private readonly AssetNumberingService _numberingService;
    private readonly AssetHistoryService _historyService;
    private readonly AssetAttachmentService _attachmentService;
    private readonly AssetBookService _bookService;
    private readonly AssetValuationParameterQueryService _parameterQuery;
    private readonly AccessPermissionService _accessPermissionService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;
    private readonly AssetImportOptions _options;
    private ImportPermissions? _permissions;

    public AssetImportService(
        ApplicationDbContext context,
        AssetActorResolver actorResolver,
        AssetEntityResolver entityResolver,
        AssetNumberingService numberingService,
        AssetHistoryService historyService,
        AssetAttachmentService attachmentService,
        AssetBookService bookService,
        AssetValuationParameterQueryService parameterQuery,
        AccessPermissionService accessPermissionService,
        IHttpContextAccessor httpContextAccessor,
        LoggerService loggerService,
        IOptions<AssetImportOptions> options)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _actorResolver = actorResolver ?? throw new ArgumentNullException(nameof(actorResolver));
        _entityResolver = entityResolver ?? throw new ArgumentNullException(nameof(entityResolver));
        _numberingService = numberingService ?? throw new ArgumentNullException(nameof(numberingService));
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        _attachmentService = attachmentService ?? throw new ArgumentNullException(nameof(attachmentService));
        _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
        _parameterQuery = parameterQuery ?? throw new ArgumentNullException(nameof(parameterQuery));
        _accessPermissionService = accessPermissionService ?? throw new ArgumentNullException(nameof(accessPermissionService));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
        _options = options?.Value ?? new AssetImportOptions();
    }

    private int MaxRows => _options.MaxRows > 0 ? _options.MaxRows : 5000;

    public async Task<AssetImportBatchResponse> UploadAsync(AssetImportUploadRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var settings = await AssetRegisterSettingReader.ReadAsync(_context, entity.LegalEntityId, cancellationToken);
        if (settings.CutoverDate is not { } cutoverDate)
        {
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.ImportCutoverMissing, AssetRegisterMessages.ImportCutoverMissing);
        }

        var file = request.File;
        if (file == null || file.Length <= 0)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.AttachmentEmpty, AssetRegisterMessages.AttachmentEmpty);
        }

        if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.ImportTemplate, AssetRegisterMessages.ImportTemplate);
        }

        AssetImportParseResult parsed;
        try
        {
            await using var buffer = new MemoryStream();
            await using (var input = file.OpenReadStream())
            {
                await input.CopyToAsync(buffer, cancellationToken);
            }

            buffer.Position = 0;
            parsed = AssetImportWorkbook.Parse(buffer);
        }
        catch (Exception exception) when (exception is not OperationCanceledException and not AssetManagementException)
        {
            throw new AssetManagementException(StatusCodes.Status400BadRequest, AssetRegisterErrorCodes.ImportTemplate, AssetRegisterMessages.ImportTemplate, exception);
        }

        if (!parsed.HeaderValid)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.ImportTemplate, AssetRegisterMessages.ImportTemplate);
        }

        if (parsed.Rows.Count == 0)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.ImportTemplate, AssetRegisterMessages.ImportEmpty);
        }

        if (parsed.Rows.Count > MaxRows)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.ImportMaxRows, string.Format(System.Globalization.CultureInfo.GetCultureInfo("id-ID"), AssetRegisterMessages.ImportMaxRowsFormat, MaxRows));
        }

        var validation = await ValidateAsync(parsed.Rows, entity.LegalEntityId, cutoverDate, cancellationToken);
        var errorCount = validation.Count(x => x.Errors.Count > 0);

        var created = await InTransactionAsync(async () =>
        {
            var now = DateTime.UtcNow;
            var batch = new AstImportBatch
            {
                CutoverDate = cutoverDate,
                Status = errorCount == 0 ? ImportBatchStatus.Tervalidasi : ImportBatchStatus.GagalValidasi,
                RowCount = validation.Count,
                ErrorCount = errorCount,
                TotalAcquisitionCost = validation.Sum(x => x.AcquisitionCost ?? 0m),
                TotalAccumulatedDepreciation = validation.Sum(x => x.AccumulatedDepreciation ?? 0m),
                UploadedBy = actor.UserId,
                UploadedAt = now,
                CreateBy = actor.UserId,
                CreateDateTime = now
            };

            var attachment = await _attachmentService.SaveAsync(file, ImportOwnerType, batch.Id, actor.UserId, cancellationToken);
            batch.FileAttachmentId = attachment.Id;
            batch.ImportNumber = await _numberingService.AllocateDocumentNumberAsync(AssetDocumentNumberSeries.Import, actor.UserId, cancellationToken);

            foreach (var row in validation)
            {
                batch.Rows.Add(new AstImportBatchRow
                {
                    ImportBatchId = batch.Id,
                    RowNumber = row.RowNumber,
                    RawJson = JsonSerializer.Serialize(row.Source.Values),
                    ErrorMessages = JoinErrors(row.Errors),
                    CreateBy = actor.UserId,
                    CreateDateTime = now
                });
            }

            _context.AstImportBatches.Add(batch);
            await _context.SaveChangesAsync(cancellationToken);
            return (batch.Id, batch.ImportNumber);
        }, cancellationToken);

        await AuditAsync("AssetImport.Upload", $"Batch impor {created.ImportNumber} diunggah oleh {actor.UserId}: {validation.Count} baris, {errorCount} baris salah.");
        return await GetBatchAsync(created.Id, cancellationToken);
    }

    public async Task<AssetImportBatchResponse> ApproveAssetAsync(Guid id, AssetImportDecisionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = OptionalReason(request.Reason);

        var number = await InTransactionAsync(async () =>
        {
            var batch = await LockBatchAsync(id, cancellationToken);
            EnsureStatus(batch, "disetujui Kepala Bagian Aset", ImportBatchStatus.Tervalidasi);
            EnsureRowVersion(batch, request.RowVersion);
            EnsureNotUploader(batch, actor);

            var now = DateTime.UtcNow;
            batch.Status = ImportBatchStatus.DisetujuiBagianAset;
            batch.AssetHeadApprovedBy = actor.UserId;
            batch.AssetHeadApprovedAt = now;
            batch.DecisionReason = reason;
            Touch(batch, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return batch.ImportNumber;
        }, cancellationToken);

        await AuditAsync("AssetImport.ApproveAsset", $"Batch impor {number} disetujui Kepala Bagian Aset {actor.UserId}.");
        return await GetBatchAsync(id, cancellationToken);
    }

    public async Task<AssetImportBatchResponse> ApproveValueAsync(Guid id, AssetImportDecisionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = request.Accept ? OptionalReason(request.Reason) : RequiredReason(request.Reason);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);

        var result = await InTransactionAsync(async () =>
        {
            var batch = await LockBatchAsync(id, cancellationToken);
            EnsureStatus(batch, "diputuskan Akuntansi", ImportBatchStatus.DisetujuiBagianAset);
            EnsureRowVersion(batch, request.RowVersion);
            EnsureNotUploader(batch, actor);

            var now = DateTime.UtcNow;
            batch.DecisionReason = reason;
            if (!request.Accept)
            {
                batch.Status = ImportBatchStatus.Ditolak;
                batch.RejectedBy = actor.UserId;
                batch.RejectedAt = now;
                Touch(batch, actor.UserId, now);
                await _context.SaveChangesAsync(cancellationToken);
                return (batch.ImportNumber, Applied: 0);
            }

            var applied = await ApplyAsync(batch, entity, actor.UserId, now, cancellationToken);
            batch.Status = ImportBatchStatus.Diterapkan;
            batch.AccountingApprovedBy = actor.UserId;
            batch.AccountingApprovedAt = now;
            Touch(batch, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return (batch.ImportNumber, Applied: applied);
        }, cancellationToken);

        await AuditAsync(
            request.Accept ? "AssetImport.Apply" : "AssetImport.RejectValue",
            request.Accept
                ? $"Batch impor {result.ImportNumber} disetujui Akuntansi {actor.UserId} dan diterapkan: {result.Applied} aset."
                : $"Batch impor {result.ImportNumber} ditolak Akuntansi {actor.UserId}.");
        return await GetBatchAsync(id, cancellationToken);
    }

    public async Task<AssetImportBatchResponse> RejectAsync(Guid id, AssetImportDecisionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = RequiredReason(request.Reason);

        var number = await InTransactionAsync(async () =>
        {
            var batch = await LockBatchAsync(id, cancellationToken);
            if (batch.Status == ImportBatchStatus.DisetujuiBagianAset)
            {
                throw AssetManagementException.Conflict(AssetRegisterErrorCodes.ImportRoute, AssetRegisterMessages.ImportRejectAtAccounting);
            }

            EnsureStatus(batch, "ditolak", ImportBatchStatus.Tervalidasi);
            EnsureRowVersion(batch, request.RowVersion);
            EnsureNotUploader(batch, actor);

            var now = DateTime.UtcNow;
            batch.Status = ImportBatchStatus.Ditolak;
            batch.RejectedBy = actor.UserId;
            batch.RejectedAt = now;
            batch.DecisionReason = reason;
            Touch(batch, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return batch.ImportNumber;
        }, cancellationToken);

        await AuditAsync("AssetImport.Reject", $"Batch impor {number} ditolak Kepala Bagian Aset {actor.UserId}.");
        return await GetBatchAsync(id, cancellationToken);
    }

    public async Task<AssetImportBatchResponse> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);

        var number = await InTransactionAsync(async () =>
        {
            var batch = await LockBatchAsync(id, cancellationToken);
            if (batch.UploadedBy != actor.UserId)
            {
                throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.ImportOwner, AssetRegisterMessages.ImportOwner);
            }

            EnsureStatus(batch, "dibatalkan", ImportBatchStatus.Tervalidasi, ImportBatchStatus.DisetujuiBagianAset);

            var now = DateTime.UtcNow;
            batch.Status = ImportBatchStatus.Dibatalkan;
            batch.CancelBy = actor.UserId;
            batch.CancelDateTime = now;
            Touch(batch, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return batch.ImportNumber;
        }, cancellationToken);

        await AuditAsync("AssetImport.Cancel", $"Batch impor {number} dibatalkan pengunggah {actor.UserId}.");
        return await GetBatchAsync(id, cancellationToken);
    }

    public async Task<AssetImportTemplateFile> BuildTemplateAsync(CancellationToken cancellationToken = default)
    {
        await RequireActorAsync(cancellationToken);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var settings = await AssetRegisterSettingReader.ReadAsync(_context, entity.LegalEntityId, cancellationToken);

        var groups = await _context.MstAssetGroups
            .AsNoTracking()
            .Where(x => x.IsActive && !x.IsDelete)
            .OrderBy(x => x.InventoryCode)
            .Select(x => new AssetImportReferenceRow(x.InventoryCode, x.GroupName, x.Classification != null ? x.Classification.ClassificationName : null))
            .ToListAsync(cancellationToken);
        var ownership = await _context.MstAssetOwnershipTypes
            .AsNoTracking()
            .Where(x => x.IsActive && !x.IsDelete)
            .OrderBy(x => x.OwnershipCode)
            .Select(x => new AssetImportReferenceRow(x.OwnershipCode, x.OwnershipName, x.IsCapitalizable ? "Ya" : "Tidak"))
            .ToListAsync(cancellationToken);
        var locations = await _context.MstAssetLocations
            .AsNoTracking()
            .Where(x => x.IsActive && !x.IsDelete)
            .OrderBy(x => x.LocationCode)
            .Select(x => new AssetImportReferenceRow(x.LocationCode, x.LocationName, x.OrganizationUnit != null ? x.OrganizationUnit.UnitCode : null))
            .ToListAsync(cancellationToken);

        var content = AssetImportWorkbook.BuildTemplate(new AssetImportReferences(groups, ownership, locations), settings.CutoverDate);
        return new AssetImportTemplateFile(content, AssetImportWorkbook.TemplateFileName);
    }

    private async Task<int> ApplyAsync(AstImportBatch batch, AssetEntityContext entity, Guid actorUserId, DateTime now, CancellationToken cancellationToken)
    {
        var rows = await _context.AstImportBatchRows
            .Where(x => x.ImportBatchId == batch.Id && !x.IsDelete)
            .OrderBy(x => x.RowNumber)
            .ToListAsync(cancellationToken);
        var parsed = rows
            .Select(x => new AssetImportParsedRow
            {
                RowNumber = x.RowNumber,
                Values = JsonSerializer.Deserialize<Dictionary<string, string>>(x.RawJson) ?? new Dictionary<string, string>()
            })
            .ToList();

        var validation = await ValidateAsync(parsed, entity.LegalEntityId, batch.CutoverDate, cancellationToken);
        var failed = validation.FirstOrDefault(x => x.Errors.Count > 0);
        if (failed != null)
        {
            throw AssetManagementException.Conflict(
                AssetRegisterErrorCodes.ImportRevalidate,
                string.Format(AssetRegisterMessages.ImportRevalidateFormat, failed.RowNumber, failed.Errors[0]));
        }

        await _parameterQuery.GetRequiredParameterAtAsync(entity.LegalEntityId, batch.CutoverDate, cancellationToken);
        var rowById = rows.ToDictionary(x => x.RowNumber);
        foreach (var row in validation)
        {
            var asset = new AstAsset
            {
                LegalEntityId = entity.LegalEntityId,
                HospitalSiteId = entity.HospitalSiteId,
                AssetGroupId = row.GroupId,
                OwnershipTypeId = row.OwnershipTypeId,
                AssetName = row.AssetName,
                Description = row.Description,
                Brand = row.Brand,
                ModelType = row.ModelType,
                SerialNumber = row.SerialNumber,
                Manufacturer = row.Manufacturer,
                DistributionPermitNumber = row.DistributionPermitNumber,
                ManufactureYear = row.ManufactureYear,
                DistributorName = row.DistributorName,
                SupplierName = row.SupplierName,
                AcquisitionType = AssetAcquisitionType.SaldoAwal,
                AcquisitionDate = row.AcquisitionDate,
                ReadyForUseDate = row.ReadyForUseDate,
                RegisteredAcquisitionCost = row.AcquisitionCost!.Value,
                RecordStatus = AssetRecordStatus.Terdaftar,
                Condition = AssetCondition.Aktif,
                ConditionChangedAt = now,
                PlacementState = AssetPlacementState.Ditempatkan,
                CurrentOrganizationUnitId = row.OrganizationUnitId,
                CurrentLocationId = row.LocationId,
                CurrentCustodianEmployeeId = row.CustodianEmployeeId,
                SubmittedBy = batch.UploadedBy,
                SubmittedAt = batch.UploadedAt,
                ApprovedBy = actorUserId,
                ApprovedAt = now,
                SourceImportBatchId = batch.Id,
                CreateBy = actorUserId,
                CreateDateTime = now
            };

            asset.InventoryNumber = await _numberingService.AllocateInventoryNumberAsync(row.InventoryCode, actorUserId, cancellationToken);
            _context.AstAssets.Add(asset);
            _context.AstAssetPlacements.Add(new AstAssetPlacement
            {
                AssetId = asset.Id,
                OrganizationUnitId = row.OrganizationUnitId,
                LocationId = row.LocationId,
                CustodianEmployeeId = row.CustodianEmployeeId,
                SourceType = PlacementSourceImport,
                SourceDocumentId = batch.Id,
                StartAt = now,
                CreateBy = actorUserId,
                CreateDateTime = now
            });

            await _bookService.CreateOpeningBalanceAsync(asset, row.AccumulatedDepreciation!.Value, batch.CutoverDate, actorUserId, cancellationToken);

            _historyService.Append(new AssetHistoryEntry(
                asset.Id,
                AssetHistoryEventType.DiimporSaldoAwal,
                AssetSubModules.Register,
                actorUserId,
                ToValue: row.PlacementLabel,
                SourceDocumentType: ImportOwnerType,
                SourceDocumentId: batch.Id,
                SourceDocumentNumber: batch.ImportNumber,
                Reason: $"Saldo awal per {batch.CutoverDate:dd-MM-yyyy}",
                OccurredAt: now));

            var source = rowById[row.RowNumber];
            source.CreatedAssetId = asset.Id;
            source.UpdateBy = actorUserId;
            source.UpdateDateTime = now;
        }

        return validation.Count;
    }

    private async Task<AstImportBatch> LockBatchAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.AstImportBatches
            .FromSqlInterpolated($"SELECT * FROM public.\"AstImportBatch\" WHERE \"Id\" = {id} AND \"IsDelete\" = false FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw BatchNotFound();

    private static void EnsureNotUploader(AstImportBatch batch, AssetActor actor)
    {
        if (batch.UploadedBy == actor.UserId)
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.ImportSelfApproval, AssetRegisterMessages.ImportSelfApproval);
        }
    }

    private static void EnsureStatus(AstImportBatch batch, string actionLabel, params ImportBatchStatus[] allowed)
    {
        if (!allowed.Contains(batch.Status))
        {
            throw AssetManagementException.Conflict(
                AssetRegisterErrorCodes.ImportStatus,
                string.Format(AssetRegisterMessages.ImportStatusFormat, AssetRegisterLabels.ImportBatchStatus(batch.Status).ToLowerInvariant(), actionLabel));
        }
    }

    private static void EnsureRowVersion(AstImportBatch batch, Guid? rowVersion)
    {
        if (rowVersion is { } expected && expected != Guid.Empty && expected != batch.RowVersion)
        {
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.VersionMismatch, AssetRegisterMessages.VersionMismatch);
        }
    }

    private async Task<AssetActor> RequireActorAsync(CancellationToken cancellationToken)
    {
        var actor = await _actorResolver.GetCurrentAsync(cancellationToken);
        if (!actor.IsAuthenticated)
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.OutsideUnit, AssetRegisterMessages.OutsideUnit);
        }

        return actor;
    }

    private async Task<ImportPermissions> GetPermissionsAsync()
    {
        if (_permissions != null)
        {
            return _permissions;
        }

        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            return _permissions = new ImportPermissions(false, false, false, false);
        }

        return _permissions = new ImportPermissions(
            await _accessPermissionService.HasAccessAsync(user, PermissionResource, "Upload"),
            await _accessPermissionService.HasAccessAsync(user, PermissionResource, "ApproveAsset"),
            await _accessPermissionService.HasAccessAsync(user, PermissionResource, "ApproveValue"),
            await _accessPermissionService.HasAccessAsync(user, AssetActorResolver.ReadAllUnitsResource, "ViewAmount"));
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
        catch (DbUpdateConcurrencyException exception)
        {
            _context.ChangeTracker.Clear();
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.VersionMismatch, AssetRegisterMessages.VersionMismatch, exception);
        }
        catch
        {
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    private static string? JoinErrors(List<string> errors)
    {
        if (errors.Count == 0)
        {
            return null;
        }

        var joined = string.Join(" | ", errors);
        return joined.Length <= MaxErrorLength ? joined : joined[..(MaxErrorLength - 3)] + "...";
    }

    private static string RequiredReason(string? value)
    {
        var reason = value?.Trim();
        if (string.IsNullOrEmpty(reason) || reason.Length > MaxReasonLength)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.ReasonRequired, AssetRegisterMessages.ReasonRequired);
        }

        return reason;
    }

    private static string? OptionalReason(string? value)
    {
        var reason = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (reason is { Length: > MaxReasonLength })
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.ReasonRequired, AssetRegisterMessages.ReasonRequired);
        }

        return reason;
    }

    private static void Touch(AstImportBatch batch, Guid actorUserId, DateTime now)
    {
        batch.UpdateBy = actorUserId;
        batch.UpdateDateTime = now;
        batch.RowVersion = Guid.NewGuid();
    }

    private Task AuditAsync(string action, string message) =>
        _loggerService.AuditAsync(LogCategory, action, message);

    private static AssetManagementException BatchNotFound() =>
        AssetManagementException.NotFound(AssetRegisterErrorCodes.ImportNotFound, AssetRegisterMessages.ImportNotFound);

    private sealed record ImportPermissions(bool CanUpload, bool CanApproveAsset, bool CanApproveValue, bool CanViewAmount);
}

public sealed record AssetImportTemplateFile(byte[] Content, string FileName);
