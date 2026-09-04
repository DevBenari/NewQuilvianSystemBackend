using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.BillingManagement;

public sealed class TaxAndRoomChargePolicyServiceTests
{
    [Fact]
    public void Tax_UsesPostItemDiscountBasisAndConfiguredRounding()
    {
        var tax = TaxRuleService.CalculateTax(100_000m, 10_000m, 11m, TaxRuleValues.HalfUp, 2);
        Assert.Equal(9_900m, tax);

        var halfUp = TaxRuleService.CalculateTax(1m, 0m, 50m, TaxRuleValues.HalfUp, 0);
        var halfEven = TaxRuleService.CalculateTax(1m, 0m, 50m, TaxRuleValues.HalfEven, 0);
        Assert.Equal(1m, halfUp);
        Assert.Equal(0m, halfEven);
    }

    [Fact]
    public async Task TaxRule_RejectsCategoryPeriodOverlapAndHistoricalMutation()
    {
        await using var dbContext = IsolatedBillingDbContextFactory.Create();
        var service = CreateTaxService(dbContext);
        var start = DateTimeOffset.UtcNow.AddDays(1);
        await service.CreateAsync(TaxRequest("TAX-A", start, start.AddDays(5)), Guid.NewGuid(), CancellationToken.None);
        await Assert.ThrowsAsync<TaxRuleConflictException>(() => service.CreateAsync(
            TaxRequest("TAX-B", start.AddDays(4), start.AddDays(7)), Guid.NewGuid(), CancellationToken.None));

        var historical = TaxRequest("TAX-HISTORY", DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1));
        historical.TaxableCategory = "HISTORICAL";
        var created = await service.CreateAsync(historical, Guid.NewGuid(), CancellationToken.None);
        await Assert.ThrowsAsync<TaxRuleValidationException>(() => service.UpdateAsync(created.Id, new UpdateTaxRuleRequest
        {
            Code = created.Code, Name = created.Name, TaxableCategory = created.TaxableCategory, Rate = created.Rate,
            RoundingMode = created.RoundingMode, AllocationRule = created.AllocationRule,
            EffectiveFrom = created.EffectiveFrom, EffectiveTo = created.EffectiveTo, IsActive = true
        }, Guid.NewGuid(), CancellationToken.None));
    }

    [Theory]
    [InlineData(1, "CEILING_PERIOD", "1")]
    [InlineData(1440, "CEILING_PERIOD", "1")]
    [InlineData(1441, "CEILING_PERIOD", "2")]
    [InlineData(2160, "PROPORTIONAL", "1.5")]
    [InlineData(2879, "WHOLE_PERIODS", "1")]
    public void RoomCharge_RespectsMinimumPeriodAndRemainderMode(int occupied, string mode, string expected)
    {
        var units = RoomChargePolicyService.CalculateChargeUnits(occupied, 1440, 1440, mode);
        Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture), units);
    }

    [Fact]
    public async Task RoomPolicy_RejectsOverlapAndInvalidMinimum()
    {
        await using var dbContext = IsolatedBillingDbContextFactory.Create();
        var service = CreateRoomService(dbContext);
        var start = DateTimeOffset.UtcNow.AddDays(1);
        await service.CreateAsync(RoomRequest("ROOM-A", start, start.AddDays(5)), Guid.NewGuid(), CancellationToken.None);
        await Assert.ThrowsAsync<RoomChargePolicyConflictException>(() => service.CreateAsync(
            RoomRequest("ROOM-B", start.AddDays(4), start.AddDays(7)), Guid.NewGuid(), CancellationToken.None));
        var invalid = RoomRequest("ROOM-C", start.AddDays(7), null);
        invalid.MinimumMinutes = 60;
        await Assert.ThrowsAsync<RoomChargePolicyValidationException>(() => service.CreateAsync(invalid, Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task InactiveDraft_MayOverlapActivePolicy()
    {
        await using var dbContext = IsolatedBillingDbContextFactory.Create();
        var taxService = CreateTaxService(dbContext);
        var roomService = CreateRoomService(dbContext);
        var start = DateTimeOffset.UtcNow.AddDays(1);
        await taxService.CreateAsync(TaxRequest("TAX-ACTIVE", start, null), Guid.NewGuid(), CancellationToken.None);
        var taxDraft = TaxRequest("TAX-DRAFT", start.AddDays(1), null);
        taxDraft.IsActive = false;
        await taxService.CreateAsync(taxDraft, Guid.NewGuid(), CancellationToken.None);

        await roomService.CreateAsync(RoomRequest("ROOM-ACTIVE", start, null), Guid.NewGuid(), CancellationToken.None);
        var roomDraft = RoomRequest("ROOM-DRAFT", start.AddDays(1), null);
        roomDraft.IsActive = false;
        await roomService.CreateAsync(roomDraft, Guid.NewGuid(), CancellationToken.None);
    }

    [Fact]
    public async Task TaxRule_ActivateReactivatesAndRejectsWhenAnotherActiveRuleOverlaps()
    {
        await using var dbContext = IsolatedBillingDbContextFactory.Create();
        var service = CreateTaxService(dbContext);
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var created = await service.CreateAsync(TaxRequest("TAX-REACT", start, start.AddDays(5)), Guid.NewGuid(), CancellationToken.None);
        await service.DeactivateAsync(created.Id, new DeactivatePolicyRequest { Reason = "Diganti" }, Guid.NewGuid(), CancellationToken.None);

        var reactivated = await service.ActivateAsync(created.Id, Guid.NewGuid(), CancellationToken.None);
        Assert.True(reactivated.IsActive);

        await service.DeactivateAsync(created.Id, new DeactivatePolicyRequest { Reason = "Diganti lagi" }, Guid.NewGuid(), CancellationToken.None);
        dbContext.MstTaxRules.Add(new Areas.HealthServices.BillingManagement.MasterData.Models.MstTaxRule
        {
            Code = "TAX-NEW",
            Name = "TAX-NEW",
            TaxableCategory = "SERVICE",
            Rate = 11m,
            RoundingMode = TaxRuleValues.HalfUp,
            AllocationRule = TaxRuleValues.Proportional,
            EffectiveFrom = start.AddDays(2),
            EffectiveTo = start.AddDays(8),
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = Guid.NewGuid()
        });
        await dbContext.SaveChangesAsync(CancellationToken.None);

        await Assert.ThrowsAsync<TaxRuleConflictException>(() =>
            service.ActivateAsync(created.Id, Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task TaxRule_DeleteRejectsWhileActiveAndSoftDeletesWhenInactive()
    {
        await using var dbContext = IsolatedBillingDbContextFactory.Create();
        var service = CreateTaxService(dbContext);
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var created = await service.CreateAsync(TaxRequest("TAX-DELETE", start, null), Guid.NewGuid(), CancellationToken.None);

        await Assert.ThrowsAsync<TaxRuleValidationException>(() =>
            service.DeleteAsync(created.Id, Guid.NewGuid(), CancellationToken.None));

        await service.DeactivateAsync(created.Id, new DeactivatePolicyRequest { Reason = "Tidak dipakai" }, Guid.NewGuid(), CancellationToken.None);
        var deleted = await service.DeleteAsync(created.Id, Guid.NewGuid(), CancellationToken.None);
        Assert.True(deleted.IsDelete);
    }

    [Fact]
    public async Task RoomPolicy_ActivateReactivatesAndRejectsWhenAnotherActivePolicyOverlaps()
    {
        await using var dbContext = IsolatedBillingDbContextFactory.Create();
        var service = CreateRoomService(dbContext);
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var created = await service.CreateAsync(RoomRequest("ROOM-REACT", start, start.AddDays(5)), Guid.NewGuid(), CancellationToken.None);
        await service.DeactivateAsync(created.Id, new DeactivatePolicyRequest { Reason = "Diganti" }, Guid.NewGuid(), CancellationToken.None);

        var reactivated = await service.ActivateAsync(created.Id, Guid.NewGuid(), CancellationToken.None);
        Assert.True(reactivated.IsActive);

        await service.DeactivateAsync(created.Id, new DeactivatePolicyRequest { Reason = "Diganti lagi" }, Guid.NewGuid(), CancellationToken.None);
        dbContext.MstRoomChargePolicies.Add(new Areas.HealthServices.BillingManagement.MasterData.Models.MstRoomChargePolicy
        {
            Code = "ROOM-NEW",
            Name = "ROOM-NEW",
            MinimumMinutes = 1440,
            PeriodMinutes = 1440,
            RemainderRounding = RoomChargePolicyValues.CeilingPeriod,
            TariffMoment = RoomChargePolicyValues.PeriodStart,
            LeaveRule = RoomChargePolicyValues.IncludeLeave,
            EffectiveFrom = start.AddDays(2),
            EffectiveTo = start.AddDays(8),
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = Guid.NewGuid()
        });
        await dbContext.SaveChangesAsync(CancellationToken.None);

        await Assert.ThrowsAsync<RoomChargePolicyConflictException>(() =>
            service.ActivateAsync(created.Id, Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task RoomPolicy_DeleteRejectsWhileActiveAndSoftDeletesWhenInactive()
    {
        await using var dbContext = IsolatedBillingDbContextFactory.Create();
        var service = CreateRoomService(dbContext);
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var created = await service.CreateAsync(RoomRequest("ROOM-DELETE", start, null), Guid.NewGuid(), CancellationToken.None);

        await Assert.ThrowsAsync<RoomChargePolicyValidationException>(() =>
            service.DeleteAsync(created.Id, Guid.NewGuid(), CancellationToken.None));

        await service.DeactivateAsync(created.Id, new DeactivatePolicyRequest { Reason = "Tidak dipakai" }, Guid.NewGuid(), CancellationToken.None);
        var deleted = await service.DeleteAsync(created.Id, Guid.NewGuid(), CancellationToken.None);
        Assert.True(deleted.IsDelete);
    }

    [Fact]
    public void MutationEndpoints_RequireExactPermissions()
    {
        AssertPermission(typeof(TaxRulesController), nameof(TaxRulesController.Create), "TaxRule", "Create");
        AssertPermission(typeof(TaxRulesController), nameof(TaxRulesController.Update), "TaxRule", "Update");
        AssertPermission(typeof(TaxRulesController), nameof(TaxRulesController.Deactivate), "TaxRule", "Update");
        AssertPermission(typeof(TaxRulesController), nameof(TaxRulesController.Activate), "TaxRule", "Update");
        AssertPermission(typeof(TaxRulesController), nameof(TaxRulesController.Delete), "TaxRule", "Delete");
        AssertPermission(typeof(RoomChargePoliciesController), nameof(RoomChargePoliciesController.Create), "RoomChargePolicy", "Create");
        AssertPermission(typeof(RoomChargePoliciesController), nameof(RoomChargePoliciesController.Update), "RoomChargePolicy", "Update");
        AssertPermission(typeof(RoomChargePoliciesController), nameof(RoomChargePoliciesController.Deactivate), "RoomChargePolicy", "Update");
        AssertPermission(typeof(RoomChargePoliciesController), nameof(RoomChargePoliciesController.Activate), "RoomChargePolicy", "Update");
        AssertPermission(typeof(RoomChargePoliciesController), nameof(RoomChargePoliciesController.Delete), "RoomChargePolicy", "Delete");
    }

    private static CreateTaxRuleRequest TaxRequest(string code, DateTimeOffset start, DateTimeOffset? end) => new()
    {
        Code = code, Name = code, TaxableCategory = "SERVICE", Rate = 11m, RoundingMode = TaxRuleValues.HalfUp,
        AllocationRule = TaxRuleValues.Proportional, EffectiveFrom = start, EffectiveTo = end, IsActive = true
    };
    private static CreateRoomChargePolicyRequest RoomRequest(string code, DateTimeOffset start, DateTimeOffset? end) => new()
    {
        Code = code, Name = code, MinimumMinutes = 1440, PeriodMinutes = 1440,
        RemainderRounding = RoomChargePolicyValues.CeilingPeriod, TariffMoment = RoomChargePolicyValues.PeriodStart,
        LeaveRule = RoomChargePolicyValues.IncludeLeave, EffectiveFrom = start, EffectiveTo = end, IsActive = true
    };
    private static TaxRuleService CreateTaxService(Repositories.ApplicationDbContext dbContext) =>
        new(dbContext, new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor()));
    private static RoomChargePolicyService CreateRoomService(Repositories.ApplicationDbContext dbContext) =>
        new(dbContext, new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor()));
    private static void AssertPermission(Type controller, string methodName, string resource, string action)
    {
        var attribute = controller.GetMethod(methodName)?.GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(attribute);
        var arguments = Assert.IsType<object[]>(attribute!.Arguments);
        Assert.Equal(resource, arguments[0]);
        Assert.Equal(action, arguments[1]);
    }
}
