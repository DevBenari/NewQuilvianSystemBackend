using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.HealthServices.EmergencyInstallationManagement;

public class EmergencyDepartureServiceTests
{
    private static (ApplicationDbContext Db, EmergencyDepartureService Service) World()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"igd-departure-{Guid.NewGuid():N}").Options;
        var db = new ApplicationDbContext(options);
        var authority = new EmergencyUnitAuthorityService(db);
        return (db, new EmergencyDepartureService(db, new EmergencyDocumentNumberService(), authority));
    }

    [Theory]
    [InlineData(EmergencyPhysicalStatus.Prepared, EmergencyPhysicalStatus.Departed, true)]
    [InlineData(EmergencyPhysicalStatus.Prepared, EmergencyPhysicalStatus.Cancelled, true)]
    [InlineData(EmergencyPhysicalStatus.Departed, EmergencyPhysicalStatus.Arrived, true)]
    [InlineData(EmergencyPhysicalStatus.Departed, EmergencyPhysicalStatus.Cancelled, true)]
    [InlineData(EmergencyPhysicalStatus.Arrived, EmergencyPhysicalStatus.Cancelled, false)]
    [InlineData(EmergencyPhysicalStatus.Cancelled, EmergencyPhysicalStatus.Departed, false)]
    public void PhysicalTransition_MengikutiKontrak(
        EmergencyPhysicalStatus current, EmergencyPhysicalStatus target, bool expected)
        => Assert.Equal(expected, EmergencyDepartureService.CanTransition(current, target));

    [Theory]
    [InlineData(EmergencyHandoverStatus.Submitted, EmergencyHandoverStatus.Pending, true)]
    [InlineData(EmergencyHandoverStatus.Pending, EmergencyHandoverStatus.Accepted, true)]
    [InlineData(EmergencyHandoverStatus.Pending, EmergencyHandoverStatus.Rejected, true)]
    [InlineData(EmergencyHandoverStatus.Rejected, EmergencyHandoverStatus.Pending, true)]
    [InlineData(EmergencyHandoverStatus.Accepted, EmergencyHandoverStatus.Rejected, false)]
    public void HandoverTransition_MengikutiKontrak(
        EmergencyHandoverStatus current, EmergencyHandoverStatus target, bool expected)
        => Assert.Equal(expected, EmergencyDepartureService.CanTransition(current, target));

    [Fact]
    public async Task Depart_MenambahKejadianDanMemperbaruiStatusDalamSatuSave()
    {
        var (db, service) = World();
        var departure = new EmgDeparture
        {
            Id = Guid.NewGuid(), EmergencyVisitId = Guid.NewGuid(), DepartureNumber = "DEP-1",
            ToServiceUnitId = Guid.NewGuid(), RequestedByUserId = Guid.NewGuid(),
            PhysicalStatus = EmergencyPhysicalStatus.Prepared,
            HandoverStatus = EmergencyHandoverStatus.Submitted,
            CreateDateTime = DateTime.UtcNow
        };
        db.Add(departure);
        await db.SaveChangesAsync();

        var occurredAt = DateTime.UtcNow.AddMinutes(-2);
        var result = await service.DepartAsync(departure.Id,
            new DepartEmergencyDepartureRequest { OccurredAt = occurredAt }, Guid.NewGuid());

        Assert.True(result.Berhasil);
        Assert.Equal(EmergencyPhysicalStatus.Departed, departure.PhysicalStatus);
        var recorded = await db.Set<EmgDepartureEvent>().SingleAsync();
        Assert.Equal(EmergencyDepartureEventType.Departed, recorded.EventType);
        Assert.Equal(occurredAt, recorded.OccurredAt);
        Assert.True(recorded.IsEffective);
    }

    [Fact]
    public async Task Amend_TidakMenghapusKejadianLama()
    {
        var (db, service) = World();
        var departureId = Guid.NewGuid();
        var oldEvent = new EmgDepartureEvent
        {
            Id = Guid.NewGuid(), EmergencyDepartureId = departureId,
            EventType = EmergencyDepartureEventType.Departed,
            OccurredAt = DateTime.UtcNow.AddMinutes(-10), RecordedAt = DateTime.UtcNow.AddMinutes(-9),
            RecordedByUserId = Guid.NewGuid(), IsEffective = true, CreateDateTime = DateTime.UtcNow
        };
        db.Add(oldEvent);
        await db.SaveChangesAsync();

        var result = await service.AmendEventAsync(departureId, oldEvent.Id,
            new AmendDepartureEventRequest
            {
                EventId = oldEvent.Id, OccurredAt = DateTime.UtcNow.AddMinutes(-8), Reason = "Waktu dikoreksi"
            }, Guid.NewGuid());

        Assert.True(result.Berhasil);
        Assert.Equal(2, await db.Set<EmgDepartureEvent>().CountAsync());
        Assert.False(oldEvent.IsEffective);
        Assert.Equal(oldEvent.Id, result.Data!.SupersedesEventId);
    }

    [Fact]
    public void OrderCancel_TanpaAlasan_Ditolak()
    {
        var error = EmergencyDepartureService.ValidateOrderItem(new EmergencyHandoverOrderItemInput
        {
            OrderKind = EmergencyOrderKind.Procedure,
            OrderSource = EmergencyOrderSource.Internal,
            OrderReferenceId = Guid.NewGuid(),
            OrderDescription = "Tindakan",
            Action = EmergencyOrderAction.Cancel
        });

        Assert.Equal("Alasan pembatalan pesanan wajib diisi.", error);
    }

    [Fact]
    public void Closure_MembacaStatusFisikSaja()
    {
        Assert.True(EmergencyDepartureService.KepergianSudahTuntas(EmergencyPhysicalStatus.Arrived));
        Assert.True(EmergencyDepartureService.DokumenBelumFinal(EmergencyHandoverStatus.Pending));
    }
}
