using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace QuilvianSystemBackend.Repositories
{
    public sealed class ApplicationDbContextFactory
        : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var environment =
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? "Development";

            var basePath = Directory.GetCurrentDirectory();

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(
                    "appsettings.json",
                    optional: false,
                    reloadOnChange: false)
                .AddJsonFile(
                    $"appsettings.{environment}.json",
                    optional: true,
                    reloadOnChange: false);

            if (environment.Equals(
                    "Development",
                    StringComparison.OrdinalIgnoreCase))
            {
                configurationBuilder.AddUserSecrets<ApplicationDbContextFactory>(
                    optional: true);
            }

            configurationBuilder.AddEnvironmentVariables();

            var configuration = configurationBuilder.Build();

            var connectionString =
                configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' tidak ditemukan.");
            }

            var optionsBuilder =
                new DbContextOptionsBuilder<ApplicationDbContext>();

            optionsBuilder.UseNpgsql(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}