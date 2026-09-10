using Xunit;

namespace QuilvianSystemBackend.BillingTests.Infrastructure
{
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class PostgresIntegrationTestCollection
        : ICollectionFixture<BillingTestDatabaseFixture>
    {
        public const string Name = "Postgres Integration Tests";
    }
}