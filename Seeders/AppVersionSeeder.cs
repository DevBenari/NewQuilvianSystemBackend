using QuilvianSystemBackend.Services.System;
using QuilvianSystemBackend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace QuilvianSystemBackend.Seeders
{
    public static class AppVersionSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.ExecuteSqlRawAsync(@"
                ALTER TABLE public.""InpEpisode"" ADD COLUMN IF NOT EXISTS ""NewbornAdmissionSource"" character varying(20);
                ALTER TABLE public.""InpEpisode"" ADD COLUMN IF NOT EXISTS ""NewbornCareMode"" character varying(20);
                ALTER TABLE public.""InpEpisode"" ADD COLUMN IF NOT EXISTS ""NewbornCareSource"" character varying(200);
                ALTER TABLE public.""InpEpisode"" ADD COLUMN IF NOT EXISTS ""FamilyBillingPreference"" character varying(20);
                ALTER TABLE public.""InpEpisode"" ADD COLUMN IF NOT EXISTS ""RequiresNeonatalIntensiveCare"" boolean NOT NULL DEFAULT false;
                ALTER TABLE public.""InpEpisode"" ADD COLUMN IF NOT EXISTS ""PlannedDepositAmount"" numeric(18,2);
            ");
            var versionService = scope.ServiceProvider.GetRequiredService<ApplicationVersionService>();
            await versionService.RegisterCurrentVersionAsync();
        }
    }
}
