using Microsoft.AspNetCore.Identity;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Services;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Seeders
{
    /// <summary>
    /// Menyiapkan dua role ASP.NET Identity yang dipakai FinanceApprovalAuthorizationService
    /// (BE-FIN-032, FIN-VAL-102) untuk membedakan penyetuju TIER_1 dari TIER_2. Hanya membuat
    /// definisi role — MENUGASKAN staf ke role ini adalah tindakan administratif terpisah
    /// (mis. lewat layar manajemen role/pengguna yang sudah ada), bukan cakupan seeder ini.
    /// </summary>
    public static class FinanceApprovalRoleSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            await EnsureRoleAsync(roleManager, FinanceApprovalRoles.Supervisor,
                "Menyetujui Purchase Order/Purchasing Invoice/Pembayaran Finance bernominal di bawah Rp 50.000.000 (TIER_1)");

            await EnsureRoleAsync(roleManager, FinanceApprovalRoles.Manager,
                "Menyetujui Purchase Order/Purchasing Invoice/Pembayaran Finance pada seluruh jenjang nominal (TIER_1 dan TIER_2)");
        }

        private static async Task EnsureRoleAsync(RoleManager<ApplicationRole> roleManager, string roleName, string description)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                return;
            }

            var role = new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
                Description = description,
                IsSystemRole = false,
                CreateDateTime = DateTime.UtcNow
            };

            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"Gagal membuat role {roleName}: {errors}");
            }
        }
    }
}
