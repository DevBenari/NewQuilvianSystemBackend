using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Responses;
using W = QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services.AssetImportWorkbook;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed partial class AssetImportService
{
    public AssetMasterFilterMetadataResponse GetMetadata() => new()
    {
        DefaultFilter = new AssetMasterDefaultFilterResponse { SortBy = "createDateTime", SortDirection = "desc" },
        SortOptions = [new() { Value = "createDateTime", Label = "Tanggal Unggah" }],
        FlagOptions =
        [
            AssetPlacementService.EnumOptions("status", "Status Batch", Enum.GetValues<ImportBatchStatus>().Where(x => x != ImportBatchStatus.Diunggah), AssetRegisterLabels.ImportBatchStatus)
        ],
        QueryParameters =
        [
            new() { Name = "search", Type = "string", Description = "Mencari pada nomor batch" },
            new() { Name = "status", Type = "integer[]", Description = "Lihat flagOptions status; boleh diulang" },
            new() { Name = "awaitingMyDecision", Type = "boolean", Description = "true = batch yang menunggu persetujuan pengguna ini", Example = "true" },
            new() { Name = "sortDirection", Type = "string", Description = "asc atau desc", Example = "desc" },
            new() { Name = "pageNumber", Type = "integer", Description = "Halaman, mulai 1", Example = "1" },
            new() { Name = "pageSize", Type = "integer", Description = $"Jumlah baris per halaman, maksimal {MaxPageSize}", Example = "25" }
        ]
    };

    public async Task<AssetImportSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var settings = await AssetRegisterSettingReader.ReadAsync(_context, entity.LegalEntityId, cancellationToken);
        var query = _context.AstImportBatches.AsNoTracking().Where(x => !x.IsDelete);
        var counts = await query.GroupBy(x => x.Status).Select(x => new { Status = x.Key, Count = x.Count() }).ToListAsync(cancellationToken);
        int CountOf(ImportBatchStatus status) => counts.FirstOrDefault(x => x.Status == status)?.Count ?? 0;

        return new AssetImportSummaryResponse
        {
            Total = counts.Sum(x => x.Count),
            FailedValidation = CountOf(ImportBatchStatus.GagalValidasi),
            Validated = CountOf(ImportBatchStatus.Tervalidasi),
            AssetHeadApproved = CountOf(ImportBatchStatus.DisetujuiBagianAset),
            Applied = CountOf(ImportBatchStatus.Diterapkan),
            Rejected = CountOf(ImportBatchStatus.Ditolak),
            Cancelled = CountOf(ImportBatchStatus.Dibatalkan),
            AwaitingMyDecision = await AwaitingDecision(query, actor, await GetPermissionsAsync()).CountAsync(cancellationToken),
            CutoverDate = settings.CutoverDate,
            MaxRows = MaxRows
        };
    }

    public async Task<PagedResult<AssetImportBatchResponse>> GetBatchesAsync(AssetImportPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var query = _context.AstImportBatches.AsNoTracking().Where(x => !x.IsDelete);

        var statuses = (request.Status ?? []).Where(x => Enum.IsDefined(x)).Distinct().ToList();
        if (statuses.Count > 0)
        {
            query = query.Where(x => statuses.Contains(x.Status));
        }

        if (request.AwaitingMyDecision == true)
        {
            query = AwaitingDecision(query, actor, await GetPermissionsAsync());
        }

        var keyword = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToUpper();
        if (keyword != null)
        {
            query = query.Where(x => x.ImportNumber.ToUpper().Contains(keyword));
        }

        var ordered = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderBy(x => x.CreateDateTime)
            : query.OrderByDescending(x => x.CreateDateTime);
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize);
        var total = await query.CountAsync(cancellationToken);
        var items = await Project(ordered.ThenBy(x => x.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync(cancellationToken);
        await DescribeAsync(items, actor, cancellationToken);

        return new PagedResult<AssetImportBatchResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    public async Task<AssetImportBatchResponse> GetBatchAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var batch = await Project(_context.AstImportBatches.AsNoTracking().Where(x => x.Id == id && !x.IsDelete))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw BatchNotFound();
        await DescribeAsync([batch], actor, cancellationToken);
        return batch;
    }

    public async Task<PagedResult<AssetImportRowResponse>> GetRowsAsync(Guid id, AssetImportRowPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await RequireActorAsync(cancellationToken);
        if (!await _context.AstImportBatches.AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken))
        {
            throw BatchNotFound();
        }

        var query = _context.AstImportBatchRows.AsNoTracking().Where(x => x.ImportBatchId == id && !x.IsDelete);
        if (request.OnlyErrors == true)
        {
            query = query.Where(x => x.ErrorMessages != null);
        }

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 50 : Math.Min(request.PageSize, MaxPageSize);
        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderBy(x => x.RowNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.RowNumber,
                x.RawJson,
                x.ErrorMessages,
                x.CreatedAssetId,
                InventoryNumber = x.CreatedAsset != null ? x.CreatedAsset.InventoryNumber : null
            })
            .ToListAsync(cancellationToken);

        var amountsVisible = (await GetPermissionsAsync()).CanViewAmount;
        var items = rows.Select(x =>
        {
            var values = JsonSerializer.Deserialize<Dictionary<string, string>>(x.RawJson) ?? new Dictionary<string, string>();
            string? Read(string key) => values.TryGetValue(key, out var value) && value.Length > 0 ? value : null;
            decimal? Amount(string key) => amountsVisible && Read(key) is { } text && W.TryParseAmount(text, out var amount) ? amount : null;
            return new AssetImportRowResponse
            {
                Id = x.Id,
                RowNumber = x.RowNumber,
                GroupCode = Read(W.GroupCode),
                AssetName = Read(W.AssetName),
                UnitCode = Read(W.UnitCode),
                LocationCode = Read(W.LocationCode),
                CustodianNumber = Read(W.CustodianNumber),
                ReadyForUseDate = Read(W.ReadyForUseDate),
                AcquisitionCost = Amount(W.AcquisitionCost),
                AccumulatedDepreciation = Amount(W.AccumulatedDepreciation),
                Errors = string.IsNullOrEmpty(x.ErrorMessages) ? [] : x.ErrorMessages.Split(" | ").ToList(),
                CreatedAssetId = x.CreatedAssetId,
                InventoryNumber = x.InventoryNumber
            };
        }).ToList();

        return new PagedResult<AssetImportRowResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    private static IQueryable<AstImportBatch> AwaitingDecision(IQueryable<AstImportBatch> query, AssetActor actor, ImportPermissions permissions)
    {
        var userId = actor.UserId;
        var canApproveAsset = permissions.CanApproveAsset;
        var canApproveValue = permissions.CanApproveValue;
        return query.Where(x =>
            x.UploadedBy != userId &&
            ((canApproveAsset && x.Status == ImportBatchStatus.Tervalidasi) ||
             (canApproveValue && x.Status == ImportBatchStatus.DisetujuiBagianAset)));
    }

    private static IQueryable<AssetImportBatchResponse> Project(IQueryable<AstImportBatch> query) =>
        query.Select(x => new AssetImportBatchResponse
        {
            Id = x.Id,
            ImportNumber = x.ImportNumber,
            CutoverDate = x.CutoverDate,
            Status = x.Status,
            RowCount = x.RowCount,
            ErrorCount = x.ErrorCount,
            CreatedAssetCount = x.Rows.Count(r => r.CreatedAssetId != null),
            TotalAcquisitionCost = x.TotalAcquisitionCost,
            TotalAccumulatedDepreciation = x.TotalAccumulatedDepreciation,
            File = x.FileAttachment != null
                ? new AssetImportFileResponse { Id = x.FileAttachment.Id, FileName = x.FileAttachment.OriginalFileName, SizeBytes = x.FileAttachment.SizeBytes }
                : null,
            Uploaded = x.UploadedBy != null ? new AssetImportActorResponse { UserId = x.UploadedBy.Value, At = x.UploadedAt!.Value } : null,
            AssetHeadApproved = x.AssetHeadApprovedBy != null ? new AssetImportActorResponse { UserId = x.AssetHeadApprovedBy.Value, At = x.AssetHeadApprovedAt!.Value } : null,
            AccountingApproved = x.AccountingApprovedBy != null ? new AssetImportActorResponse { UserId = x.AccountingApprovedBy.Value, At = x.AccountingApprovedAt!.Value } : null,
            Rejected = x.RejectedBy != null ? new AssetImportActorResponse { UserId = x.RejectedBy.Value, At = x.RejectedAt!.Value } : null,
            DecisionReason = x.DecisionReason,
            RowVersion = x.RowVersion,
            CreateDateTime = x.CreateDateTime
        });

    private async Task DescribeAsync(IReadOnlyCollection<AssetImportBatchResponse> batches, AssetActor actor, CancellationToken cancellationToken)
    {
        var steps = batches
            .SelectMany(x => new[] { x.Uploaded, x.AssetHeadApproved, x.AccountingApproved, x.Rejected })
            .OfType<AssetImportActorResponse>()
            .ToList();
        var userIds = steps.Select(x => x.UserId).Where(x => x != Guid.Empty).Distinct().ToList();
        if (userIds.Count > 0)
        {
            var profiles = await _context.Users
                .AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .Select(x => new { x.Id, x.DisplayName, PositionName = x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : null })
                .ToDictionaryAsync(x => x.Id, cancellationToken);
            foreach (var step in steps)
            {
                if (profiles.TryGetValue(step.UserId, out var profile))
                {
                    step.Name = profile.DisplayName;
                    step.PositionName = profile.PositionName;
                }
            }
        }

        var permissions = await GetPermissionsAsync();
        foreach (var batch in batches)
        {
            batch.StatusName = AssetRegisterLabels.ImportBatchStatus(batch.Status);
            batch.AmountsVisible = permissions.CanViewAmount;
            if (permissions.CanViewAmount)
            {
                batch.TotalNetBookValue = (batch.TotalAcquisitionCost ?? 0m) - (batch.TotalAccumulatedDepreciation ?? 0m);
            }
            else
            {
                batch.TotalAcquisitionCost = null;
                batch.TotalAccumulatedDepreciation = null;
            }

            var isUploader = batch.Uploaded?.UserId == actor.UserId;
            var actions = new List<string>();
            if (batch.Status == ImportBatchStatus.Tervalidasi && permissions.CanApproveAsset && !isUploader)
            {
                actions.AddRange(["approveAsset", "reject"]);
            }

            if (batch.Status == ImportBatchStatus.DisetujuiBagianAset && permissions.CanApproveValue && !isUploader)
            {
                actions.AddRange(["approveValue", "rejectValue"]);
            }

            if ((batch.Status is ImportBatchStatus.Tervalidasi or ImportBatchStatus.DisetujuiBagianAset) && isUploader && permissions.CanUpload)
            {
                actions.Add("cancel");
            }

            if (batch.File != null)
            {
                actions.Add("download");
            }

            batch.IsAwaitingMyDecision = actions.Contains("approveAsset") || actions.Contains("approveValue");
            batch.AvailableActions = actions;
        }
    }
}
