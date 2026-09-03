using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.OperatingRoomManagement;

public class OperatingRoomReportServiceTests
{
    [Fact]
    public async Task GetOperationsAsync_ReturnsScheduleRoomAndActualDuration()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        await SetExecutionWindowAsync(ctx, minutes: 75);
        var service = new OperatingRoomReportService(ctx.Context);

        var result = await service.GetOperationsAsync(new OprReportQuery());

        var row = Assert.Single(result.Items);
        Assert.Equal("OPR-TEST-001", row.CaseNumber);
        Assert.Equal("Pasien Uji", row.PatientName);
        Assert.Equal("OK 1", row.RoomName);
        Assert.Equal(75, row.ActualDurationMinutes);
        Assert.Equal(60, row.EstimatedMinutes);
        Assert.Equal(1, result.TotalData);
    }

    [Fact]
    public async Task GetOperationsAsync_StatusFilter_ExcludesOtherStatuses()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = new OperatingRoomReportService(ctx.Context);

        var matching = await service.GetOperationsAsync(new OprReportQuery { Status = OprCaseStatus.InProgress });
        var other = await service.GetOperationsAsync(new OprReportQuery { Status = OprCaseStatus.Completed });

        Assert.Single(matching.Items);
        Assert.Empty(other.Items);
    }

    [Fact]
    public async Task GetOperationsAsync_Paging_ReportsTotalPageCorrectly()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = new OperatingRoomReportService(ctx.Context);

        var result = await service.GetOperationsAsync(new OprReportQuery { PageNumber = 2, PageSize = 1 });

        Assert.Empty(result.Items);
        Assert.Equal(1, result.TotalData);
        Assert.Equal(1, result.TotalPage);
    }

    [Fact]
    public async Task GetUtilizationAsync_ComputesScheduledActualAndRealization()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        await SetExecutionWindowAsync(ctx, minutes: 30);
        var service = new OperatingRoomReportService(ctx.Context);

        var result = await service.GetUtilizationAsync(new OprUtilizationQuery
        {
            From = DateTime.UtcNow.AddDays(-1), To = DateTime.UtcNow.AddDays(1)
        });

        var room = Assert.Single(result.Rooms);
        Assert.Equal("OK 1", room.RoomName);
        Assert.Equal(1, room.ScheduledCases);
        Assert.Equal(60, room.ScheduledMinutes);
        Assert.Equal(30, room.ActualMinutes);
        Assert.Equal(50.00m, room.RealizationPercent);
        Assert.Equal(1, result.TotalScheduledCases);
    }

    [Fact]
    public async Task GetUtilizationAsync_InvertedRange_RejectsWithArgumentException()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = new OperatingRoomReportService(ctx.Context);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetUtilizationAsync(new OprUtilizationQuery
        {
            From = DateTime.UtcNow, To = DateTime.UtcNow.AddDays(-1)
        }));
    }

    [Fact]
    public async Task GetUtilizationAsync_CountsPostponedCasesFromHistory()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.Scheduled);
        ctx.Context.OprStatusHistories.Add(new OprStatusHistory
        {
            OprCaseId = ctx.CaseId, FromStatus = OprCaseStatus.Scheduled, ToStatus = OprCaseStatus.Postponed,
            Action = "Postpone", ActorUserId = ctx.SurgeonUserId, OccurredAt = DateTime.UtcNow,
            Source = "API:test", CorrelationId = "hist-1"
        });
        await ctx.Context.SaveChangesAsync();
        var service = new OperatingRoomReportService(ctx.Context);

        var result = await service.GetUtilizationAsync(new OprUtilizationQuery
        {
            From = DateTime.UtcNow.AddDays(-1), To = DateTime.UtcNow.AddDays(1)
        });

        Assert.Equal(1, result.PostponedCases);
        Assert.Equal(0, result.CancelledCases);
    }

    [Fact]
    public async Task GetMaterialsAsync_FiltersBySerialNumber()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var material = new OperatingRoomMaterialService(ctx.Context, ctx.Accessor, ctx.Logger,
            new OperatingRoomIntegrationService(ctx.Context, ctx.Accessor, ctx.Logger));
        await material.RecordAsync(ctx.CaseId, new CreateOprMaterialUsageRequest
        {
            ExternalItemId = Guid.NewGuid(), ItemType = OprMaterialItemType.Implant, Quantity = 1,
            UnitCode = "PCS", Outcome = OprMaterialOutcome.Used, SerialNumber = "SN-777",
            IdempotencyKey = "rep-mat-1"
        });
        await material.RecordAsync(ctx.CaseId, new CreateOprMaterialUsageRequest
        {
            ExternalItemId = Guid.NewGuid(), ItemType = OprMaterialItemType.Consumable, Quantity = 3,
            UnitCode = "PCS", Outcome = OprMaterialOutcome.Used, IdempotencyKey = "rep-mat-2"
        });
        var service = new OperatingRoomReportService(ctx.Context);

        var all = await service.GetMaterialsAsync(new OprMaterialReportQuery());
        var filtered = await service.GetMaterialsAsync(new OprMaterialReportQuery { SerialNumber = "SN-777" });

        Assert.Equal(2, all.TotalData);
        var row = Assert.Single(filtered.Items);
        Assert.Equal(OprMaterialItemType.Implant, row.ItemType);
        Assert.Equal("OPR-TEST-001", row.CaseNumber);
    }

    private static async Task SetExecutionWindowAsync(OperatingRoomTestContext ctx, int minutes)
    {
        var record = await ctx.Context.OprExecutionRecords.FirstAsync(x => x.OprCaseId == ctx.CaseId);
        record.FinishedAt = record.StartedAt.AddMinutes(minutes);
        await ctx.Context.SaveChangesAsync();
        ctx.Context.ChangeTracker.Clear();
    }
}
