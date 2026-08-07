using QuilvianSystemBackend.Services.System;

namespace QuilvianSystemBackend.Seeders
{
    public static class AppVersionSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var versionService = scope.ServiceProvider.GetRequiredService<ApplicationVersionService>();
            await versionService.RegisterCurrentBuildAsync();
        }
    }
}
