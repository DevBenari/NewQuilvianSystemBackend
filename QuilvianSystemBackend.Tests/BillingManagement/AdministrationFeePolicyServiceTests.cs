using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Tests.BillingManagement;

public sealed class AdministrationFeePolicyServiceTests
{
    [Fact]
    public async Task Create_EnforcesOncePerLocalDayAndNonDiscountable()
    {
        await using var dbContext = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(dbContext);
        var result = await service.CreateAsync(new CreateAdministrationFeePolicyRequest
        {
            Code = "adm-rajal-2026",
            Name = "Administrasi Rawat Jalan",
            ServiceType = AdministrationFeeServiceTypes.Rajal,
            Amount = 25_000,
            Coverable = true,
            EffectiveFrom = DateTimeOffset.UtcNow.AddDays(1),
            IsActive = true
        }, Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.OncePerPatientLocalDay);
        Assert.False(result.Discountable);
        Assert.Equal(10, result.ReplacementPriority);
        Assert.Equal("ADM-RAJAL-2026", result.Code);
        Assert.Equal("Asia/Jakarta", result.BusinessTimeZone);
    }

    [Fact]
    public async Task Create_RejectsOverlappingPeriodForSameServiceType()
    {
        await using var dbContext = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(dbContext);
        var start = DateTimeOffset.UtcNow.AddDays(2);
        await service.CreateAsync(Request("ADM-IGD-A", start, start.AddDays(5)), Guid.NewGuid(), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<AdministrationFeePolicyConflictException>(() =>
            service.CreateAsync(Request("ADM-IGD-B", start.AddDays(4), start.AddDays(8)), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("bertumpang tindih", exception.Message);
    }

    [Fact]
    public async Task Create_RanapUsesOncePerEncounterAndHigherReplacementPriority()
    {
        await using var dbContext = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(dbContext);
        var request = Request("ADM-RANAP-2026", DateTimeOffset.UtcNow.AddDays(1), null);
        request.ServiceType = AdministrationFeeServiceTypes.Ranap;

        var result = await service.CreateAsync(request, Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.OncePerPatientLocalDay);
        Assert.Equal(100, result.ReplacementPriority);
    }

    [Fact]
    public async Task Update_RejectsPolicyThatHasStarted()
    {
        await using var dbContext = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(dbContext);
        var created = await service.CreateAsync(Request("ADM-OTC-CURRENT", DateTimeOffset.UtcNow.AddMinutes(-1), null), Guid.NewGuid(), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<AdministrationFeePolicyValidationException>(() =>
            service.UpdateAsync(created.Id, new UpdateAdministrationFeePolicyRequest
            {
                Code = created.Code,
                Name = created.Name,
                ServiceType = created.ServiceType,
                Amount = created.Amount,
                EffectiveFrom = created.EffectiveFrom,
                IsActive = true
            }, Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("buat versi baru", exception.Message);
    }

    [Fact]
    public void BusinessDate_UsesAsiaJakartaBoundary()
    {
        var firstVisit = new DateTimeOffset(2026, 8, 19, 17, 30, 0, TimeSpan.Zero);
        var secondVisit = new DateTimeOffset(2026, 8, 20, 16, 59, 0, TimeSpan.Zero);
        var nextLocalDay = new DateTimeOffset(2026, 8, 20, 17, 0, 0, TimeSpan.Zero);

        Assert.Equal(AdministrationFeePolicyService.GetBusinessDate(firstVisit), AdministrationFeePolicyService.GetBusinessDate(secondVisit));
        Assert.NotEqual(AdministrationFeePolicyService.GetBusinessDate(secondVisit), AdministrationFeePolicyService.GetBusinessDate(nextLocalDay));
    }

    private static CreateAdministrationFeePolicyRequest Request(string code, DateTimeOffset start, DateTimeOffset? end) => new()
    {
        Code = code,
        Name = code,
        ServiceType = AdministrationFeeServiceTypes.Igd,
        Amount = 50_000,
        EffectiveFrom = start,
        EffectiveTo = end,
        IsActive = true
    };

    private static AdministrationFeePolicyService CreateService(Repositories.ApplicationDbContext dbContext)
    {
        var logger = new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor());
        return new AdministrationFeePolicyService(dbContext, logger);
    }
}
