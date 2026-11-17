using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed record AssetLabelDocument(byte[] Content, string FileName, int LabelCount);

public sealed class AssetLabelDocumentService
{
    private const int MaxLabels = 200;
    private const int Columns = 3;
    private const float LabelWidthMm = 63.5f;
    private const float LabelHeightMm = 38.1f;
    private const float PageMarginHorizontalMm = 9.5f;
    private const float PageMarginVerticalMm = 15f;
    private const float LabelPaddingMm = 3f;
    private const float QrSizeMm = 24f;
    private const int QrPixelsPerModule = 10;

    private readonly ApplicationDbContext _context;
    private readonly AssetEntityResolver _entityResolver;
    private readonly AssetActorResolver _actorResolver;

    public AssetLabelDocumentService(ApplicationDbContext context, AssetEntityResolver entityResolver, AssetActorResolver actorResolver)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _entityResolver = entityResolver ?? throw new ArgumentNullException(nameof(entityResolver));
        _actorResolver = actorResolver ?? throw new ArgumentNullException(nameof(actorResolver));
    }

    public async Task<AssetLabelDocument> BuildAsync(AssetLabelRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var ids = (request.AssetIds ?? []).Where(x => x != Guid.Empty).Distinct().ToList();
        if (ids.Count is < 1 or > MaxLabels)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, AssetRegisterMessages.LabelBatchSize);
        }

        var actor = await _actorResolver.GetCurrentAsync(cancellationToken);
        if (!actor.IsAuthenticated)
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.OutsideUnit, AssetRegisterMessages.OutsideUnit);
        }

        var entity = await _entityResolver.ResolveAsync(cancellationToken);

        var assets = await _context.AstAssets
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id) && !x.IsDelete && x.LegalEntityId == entity.LegalEntityId)
            .Select(x => new AssetLabelSource(
                x.Id,
                x.InventoryNumber,
                x.AssetName,
                x.AcquisitionDate,
                x.RecordStatus,
                x.CurrentOrganizationUnitId))
            .ToListAsync(cancellationToken);

        if (assets.Count != ids.Count)
        {
            throw AssetManagementException.NotFound(AssetRegisterErrorCodes.NotFound, AssetRegisterMessages.NotFound);
        }

        foreach (var asset in assets)
        {
            actor.EnsureCanAccessUnit(asset.OrganizationUnitId);

            if (asset.RecordStatus != AssetRecordStatus.Terdaftar || string.IsNullOrWhiteSpace(asset.InventoryNumber))
            {
                throw AssetManagementException.Conflict(
                    AssetRegisterErrorCodes.Status,
                    string.Format(
                        AssetRegisterMessages.StatusFormat,
                        AssetRegisterLabels.RecordStatus(asset.RecordStatus),
                        "dicetak labelnya"));
            }
        }

        var labels = assets
            .OrderBy(x => x.InventoryNumber, StringComparer.Ordinal)
            .Select(x => new AssetLabelItem(x.InventoryNumber!, x.AssetName, x.AcquisitionDate.Year, CreateQrPng(x.InventoryNumber!)))
            .ToList();

        var content = Render(labels, entity.LegalEntityName);
        return new AssetLabelDocument(content, $"label-aset-{DateTime.Now:yyyyMMddHHmmss}.pdf", labels.Count);
    }

    public static byte[] CreateQrPng(string inventoryNumber)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(inventoryNumber, QRCodeGenerator.ECCLevel.M);
        using var code = new PngByteQRCode(data);
        return code.GetGraphic(QrPixelsPerModule, true);
    }

    private static byte[] Render(List<AssetLabelItem> labels, string ownerName) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);
                page.MarginHorizontal(PageMarginHorizontalMm, Unit.Millimetre);
                page.MarginVertical(PageMarginVerticalMm, Unit.Millimetre);
                page.DefaultTextStyle(x => x.FontSize(7));

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        for (var i = 0; i < Columns; i++)
                        {
                            columns.ConstantColumn(LabelWidthMm, Unit.Millimetre);
                        }
                    });

                    foreach (var label in labels)
                    {
                        table.Cell()
                            .Height(LabelHeightMm, Unit.Millimetre)
                            .ShowEntire()
                            .Padding(LabelPaddingMm, Unit.Millimetre)
                            .Element(cell => ComposeLabel(cell, label, ownerName));
                    }
                });
            });
        }).GeneratePdf();

    private static void ComposeLabel(IContainer container, AssetLabelItem label, string ownerName) =>
        container.Column(column =>
        {
            column.Spacing(1.5f, Unit.Millimetre);
            column.Item().Row(head =>
            {
                head.RelativeItem().Text(label.InventoryNumber).FontSize(9).Bold();
                head.AutoItem().Text(label.AcquisitionYear.ToString(CultureInfo.InvariantCulture)).FontSize(9);
            });
            column.Item().Row(body =>
            {
                body.ConstantItem(QrSizeMm, Unit.Millimetre).Image(label.QrPng).FitArea();
                body.RelativeItem().PaddingLeft(2.5f, Unit.Millimetre).AlignMiddle().Column(info =>
                {
                    info.Spacing(1, Unit.Millimetre);
                    info.Item().Text(label.AssetName).FontSize(7).ClampLines(3, "…");
                    info.Item().Text(ownerName).FontSize(6).FontColor(Colors.Grey.Darken1).ClampLines(2, "…");
                });
            });
        });

    private sealed record AssetLabelSource(
        Guid Id,
        string? InventoryNumber,
        string AssetName,
        DateOnly AcquisitionDate,
        AssetRecordStatus RecordStatus,
        Guid? OrganizationUnitId);

    private sealed record AssetLabelItem(string InventoryNumber, string AssetName, int AcquisitionYear, byte[] QrPng);
}
