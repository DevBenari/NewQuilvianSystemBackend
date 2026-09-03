using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.OperatingRoomManagement;

public class OperatingRoomMaterialServiceTests
{
    [Fact]
    public async Task RecordAsync_Consumable_AppendsLedgerAndStagesInventoryDelivery()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);

        var result = await service.RecordAsync(ctx.CaseId, ValidUsage("mat-1"));

        Assert.Equal(1, result.Revision);
        Assert.Equal(OprMaterialOutcome.Used, result.Outcome);
        var delivery = await ctx.Context.OprIntegrationDeliveries.SingleAsync();
        Assert.Equal(OperatingRoomIntegrationService.InventoryDestination, delivery.Destination);
        Assert.Equal(OperatingRoomIntegrationService.MaterialMessageType, delivery.MessageType);
        Assert.Equal(OprDeliveryStatus.Pending, delivery.Status);
        Assert.EndsWith(":1", delivery.IdempotencyKey);
    }

    [Fact]
    public async Task RecordAsync_NonPositiveQuantity_RejectsOpr008()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        var request = ValidUsage("mat-zero");
        request.Quantity = 0;

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            service.RecordAsync(ctx.CaseId, request));

        Assert.Equal("OPR008", exception.Code);
    }

    [Fact]
    public async Task RecordAsync_ImplantWithoutBatchOrSerial_RejectsOpr009()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        var request = ValidUsage("mat-implant");
        request.ItemType = OprMaterialItemType.Implant;

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            service.RecordAsync(ctx.CaseId, request));

        Assert.Equal("OPR009", exception.Code);
    }

    [Fact]
    public async Task RecordAsync_ImplantWithSerial_IsAccepted()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        var request = ValidUsage("mat-implant-ok");
        request.ItemType = OprMaterialItemType.Implant;
        request.SerialNumber = "SN-00187";

        var result = await service.RecordAsync(ctx.CaseId, request);

        Assert.Equal("SN-00187", result.SerialNumber);
        Assert.Equal(OprMaterialItemType.Implant, result.ItemType);
    }

    [Fact]
    public async Task RecordAsync_RetryWithSameKey_DoesNotDuplicateUsageOrDelivery()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);

        var request = ValidUsage("mat-retry");
        var first = await service.RecordAsync(ctx.CaseId, request);
        var second = await service.RecordAsync(ctx.CaseId, request);

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, await ctx.Context.OprMaterialUsages.CountAsync());
        Assert.Equal(1, await ctx.Context.OprIntegrationDeliveries.CountAsync());
    }

    [Fact]
    public async Task RecordAsync_SameKeyDifferentPayload_RejectsOpr013()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        var original = ValidUsage("mat-conflict");
        await service.RecordAsync(ctx.CaseId, original);
        // Hanya jumlahnya yang berbeda; itemnya sama, sehingga penolakan benar-benar
        // berasal dari isi permintaan yang tidak cocok.
        var changed = ValidUsage("mat-conflict");
        changed.ExternalItemId = original.ExternalItemId;
        changed.Quantity = 5;

        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            service.RecordAsync(ctx.CaseId, changed));

        Assert.Equal("OPR013", exception.Code);
    }

    [Fact]
    public async Task RecordAsync_CorrectionWithoutReason_RejectsAsUnprocessable()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        var original = await service.RecordAsync(ctx.CaseId, ValidUsage("mat-original"));
        var correction = ValidUsage("mat-correction");
        correction.Outcome = OprMaterialOutcome.Corrected;
        correction.CorrectionOfUsageId = original.Id;

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            service.RecordAsync(ctx.CaseId, correction));

        Assert.Equal("CorrectionReasonRequired", exception.Code);
    }

    [Fact]
    public async Task RecordAsync_Correction_RaisesRevisionAndKeepsOriginal()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        var original = await service.RecordAsync(ctx.CaseId, ValidUsage("mat-base"));

        var correction = ValidUsage("mat-fix");
        correction.Outcome = OprMaterialOutcome.Corrected;
        correction.CorrectionOfUsageId = original.Id;
        correction.CorrectionReason = "Jumlah tercatat dua kali lipat.";
        correction.Quantity = 1;
        var result = await service.RecordAsync(ctx.CaseId, correction);

        Assert.Equal(2, result.Revision);
        Assert.Equal(2, await ctx.Context.OprMaterialUsages.CountAsync());
        var ledger = await service.GetLedgerAsync(ctx.CaseId);
        Assert.Equal(2, ledger!.Entries.Count);
    }

    [Fact]
    public async Task RecordAsync_ByUserOutsideTeam_IsForbidden()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        ctx.ActAs(ctx.OutsiderUserId);

        await Assert.ThrowsAsync<OperatingRoomForbiddenException>(() =>
            service.RecordAsync(ctx.CaseId, ValidUsage("mat-outsider")));
    }

    [Fact]
    public async Task RecordAsync_BeforeStart_RejectsInvalidStateTransition()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.Ready);
        var service = Build(ctx);

        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            service.RecordAsync(ctx.CaseId, ValidUsage("mat-early")));

        Assert.Equal("InvalidStateTransition", exception.Code);
    }

    [Fact]
    public async Task RecordAsync_UnknownItem_IsRecordedAsUnresolved()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);

        var result = await service.RecordAsync(ctx.CaseId, ValidUsage("mat-unknown"));

        Assert.False(result.IsItemResolved);
        var ledger = await service.GetLedgerAsync(ctx.CaseId);
        Assert.Equal(1, ledger!.UnresolvedItemCount);
    }

    [Fact]
    public async Task RecordAsync_KnownActiveDrug_IsResolvedWithName()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        var itemId = await AddDrugAsync(ctx, "Ceftriaxone 1 g", active: true);
        var request = ValidUsage("mat-known");
        request.ExternalItemId = itemId;

        var result = await service.RecordAsync(ctx.CaseId, request);

        Assert.True(result.IsItemResolved);
        Assert.Equal("Ceftriaxone 1 g", result.ItemName);
    }

    [Fact]
    public async Task RecordAsync_InactiveDrug_RejectsAsUnprocessable()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        var itemId = await AddDrugAsync(ctx, "Obat Nonaktif", active: false);
        var request = ValidUsage("mat-inactive");
        request.ExternalItemId = itemId;

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            service.RecordAsync(ctx.CaseId, request));

        Assert.Equal("ItemInactive", exception.Code);
    }

    private static OperatingRoomMaterialService Build(OperatingRoomTestContext ctx) =>
        new(ctx.Context, ctx.Accessor, ctx.Logger,
            new OperatingRoomIntegrationService(ctx.Context, ctx.Accessor, ctx.Logger));

    private static async Task<Guid> AddDrugAsync(OperatingRoomTestContext ctx, string name, bool active)
    {
        var id = Guid.NewGuid();
        ctx.Context.Set<MstDrug>().Add(new MstDrug
        {
            Id = id, DrugCategoryId = Guid.NewGuid(), DrugCode = $"DRG-{id:N}"[..12], DrugName = name,
            IsActive = active
        });
        await ctx.Context.SaveChangesAsync();
        ctx.Context.ChangeTracker.Clear();
        return id;
    }

    private static CreateOprMaterialUsageRequest ValidUsage(string key) => new()
    {
        ExternalItemId = Guid.NewGuid(),
        ItemType = OprMaterialItemType.Consumable,
        Quantity = 2,
        UnitCode = "PCS",
        Outcome = OprMaterialOutcome.Used,
        IdempotencyKey = key
    };
}
