using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.BillingManagement;

public sealed class DiscountPolicyServiceTests
{
    [Fact]
    public async Task PromoTotal_IsAutomaticAndTargetsPatientPortion()
    {
        await using var dbContext = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(dbContext);

        var result = await service.CreateAsync(Request(
            "PROMO-TOTAL-2026",
            DiscountPolicyValues.PromoTotal,
            DiscountPolicyValues.PatientPortion), Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.RequiresApproval);
        Assert.Null(result.ApproverRole);
        Assert.Equal(DiscountPolicyValues.PatientPortion, result.TargetComponent);
    }

    [Fact]
    public async Task PromoItem_IsAutomaticAndTargetsInvoiceItem()
    {
        await using var dbContext = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(dbContext);

        var result = await service.CreateAsync(Request(
            "PROMO-ITEM-2026",
            DiscountPolicyValues.PromoItem,
            DiscountPolicyValues.InvoiceItem), Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.RequiresApproval);
        Assert.Equal(DiscountPolicyValues.InvoiceItem, result.TargetComponent);
    }

    [Fact]
    public async Task DoctorPolicy_OnlyTargetsDoctorShareAndRequiresDoctorApproval()
    {
        await using var dbContext = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(dbContext);
        var invalid = Request("DOC-INVALID", DiscountPolicyValues.Doctor, DiscountPolicyValues.PatientPortion);

        await Assert.ThrowsAsync<DiscountPolicyValidationException>(() =>
            service.CreateAsync(invalid, Guid.NewGuid(), CancellationToken.None));

        var valid = Request("DOC-VALID", DiscountPolicyValues.Doctor, DiscountPolicyValues.DoctorShare);
        valid.Value = 100;
        var result = await service.CreateAsync(valid, Guid.NewGuid(), CancellationToken.None);
        Assert.True(result.RequiresApproval);
        Assert.Equal(DiscountPolicyValues.DoctorApprover, result.ApproverRole);
        Assert.Equal(DiscountPolicyValues.DoctorShare, result.TargetComponent);
    }

    [Fact]
    public async Task Create_RejectsOverlappingPeriodForSameTypeAndTarget()
    {
        await using var dbContext = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(dbContext);
        var first = Request("PROMO-A", DiscountPolicyValues.PromoTotal, DiscountPolicyValues.PatientPortion);
        first.EffectiveTo = first.EffectiveFrom.AddDays(10);
        await service.CreateAsync(first, Guid.NewGuid(), CancellationToken.None);

        var second = Request("PROMO-B", DiscountPolicyValues.PromoTotal, DiscountPolicyValues.PatientPortion);
        second.EffectiveFrom = first.EffectiveFrom.AddDays(5);
        second.EffectiveTo = first.EffectiveFrom.AddDays(15);

        await Assert.ThrowsAsync<DiscountPolicyConflictException>(() =>
            service.CreateAsync(second, Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task Update_RejectsHistoricalPolicy()
    {
        await using var dbContext = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(dbContext);
        var request = Request("PROMO-HISTORY", DiscountPolicyValues.PromoItem, DiscountPolicyValues.InvoiceItem);
        request.EffectiveFrom = DateTimeOffset.UtcNow.AddMinutes(-1);
        var created = await service.CreateAsync(request, Guid.NewGuid(), CancellationToken.None);

        var update = new UpdateDiscountPolicyRequest
        {
            Code = created.Code,
            Name = created.Name,
            DiscountType = created.DiscountType,
            TargetComponent = created.TargetComponent,
            ValueType = created.ValueType,
            Value = created.Value,
            EffectiveFrom = created.EffectiveFrom,
            IsActive = true
        };

        await Assert.ThrowsAsync<DiscountPolicyValidationException>(() =>
            service.UpdateAsync(created.Id, update, Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public void MutationEndpoints_RequireExactDiscountPolicyPermissions()
    {
        AssertPermission(nameof(DiscountPoliciesController.Create), "Create");
        AssertPermission(nameof(DiscountPoliciesController.Update), "Update");
        AssertPermission(nameof(DiscountPoliciesController.Deactivate), "Update");
    }

    private static void AssertPermission(string methodName, string action)
    {
        var method = typeof(DiscountPoliciesController).GetMethod(methodName);
        var attribute = method?.GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(attribute);
        var arguments = Assert.IsType<object[]>(attribute!.Arguments);
        Assert.Equal("DiscountPolicy", arguments[0]);
        Assert.Equal(action, arguments[1]);
    }

    private static CreateDiscountPolicyRequest Request(string code, string type, string target) => new()
    {
        Code = code,
        Name = code,
        DiscountType = type,
        TargetComponent = target,
        ValueType = DiscountPolicyValues.Percentage,
        Value = 10,
        Limit = 100_000,
        EffectiveFrom = DateTimeOffset.UtcNow.AddDays(1),
        IsActive = true
    };

    private static DiscountPolicyService CreateService(Repositories.ApplicationDbContext dbContext)
    {
        var logger = new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor());
        return new DiscountPolicyService(dbContext, logger);
    }
}
