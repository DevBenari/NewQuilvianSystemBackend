using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.BillingManagement;

internal static class IsolatedBillingDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"billing-tests-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }
}
