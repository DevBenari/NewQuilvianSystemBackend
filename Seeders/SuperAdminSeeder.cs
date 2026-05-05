using Microsoft.AspNetCore.Identity;
using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Seeders
{
    public static class SuperAdminSeeder
    {
        private const string SuperAdminRoleName = "SuperAdmin";
        private const string UserRoleName = "User";

        private const string SuperAdminUserCode = "USR-RSMMC-00000";
        private const string DefaultProfilePhotoFileName = "user.jpeg";

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            var seedEnabled = configuration.GetValue<bool>("SeedSuperAdmin:Enabled");

            if (!seedEnabled)
            {
                Console.WriteLine("[Seeder] SeedSuperAdmin disabled.");
                return;
            }

            await EnsureRoleAsync(
                roleManager,
                SuperAdminRoleName,
                "Full access system administrator",
                true
            );

            await EnsureRoleAsync(
                roleManager,
                UserRoleName,
                "Default application user",
                true
            );

            var username = configuration["SeedSuperAdmin:Username"];
            var email = configuration["SeedSuperAdmin:Email"];
            var password = configuration["SeedSuperAdmin:Password"];
            var fullName = configuration["SeedSuperAdmin:FullName"];

            if (
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password)
            )
            {
                throw new InvalidOperationException("Konfigurasi SeedSuperAdmin belum lengkap di appsettings.json.");
            }

            username = username.Trim();
            email = email.Trim().ToLowerInvariant();

            var defaultPhotoPath = BuildDefaultProfilePhotoUrl(configuration);

            Console.WriteLine($"[Seeder] Checking SuperAdmin username={username}, email={email}");

            var existingUser = await userManager.FindByNameAsync(username);

            if (existingUser == null)
            {
                existingUser = await userManager.FindByEmailAsync(email);
            }

            if (existingUser == null)
            {
                Console.WriteLine("[Seeder] SuperAdmin not found. Creating new SuperAdmin...");

                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),

                    UserCode = SuperAdminUserCode,

                    UserName = username,
                    NormalizedUserName = username.ToUpperInvariant(),

                    Email = email,
                    NormalizedEmail = email.ToUpperInvariant(),
                    EmailConfirmed = true,

                    FullName = string.IsNullOrWhiteSpace(fullName)
                        ? "Super Administrator"
                        : fullName.Trim(),

                    UserType = UserType.SuperAdmin,

                    BirthDate = null,
                    IdentityNumber = null,

                    HospitalId = null,
                    DepartmentId = null,
                    PositionId = null,
                    EmployeeId = null,
                    DoctorId = null,
                    ExternalUserId = null,

                    IsActive = true,
                    MustChangePassword = false,
                    AccessValidUntil = null,

                    ProfilePhotoPath = defaultPhotoPath,

                    CreateDateTime = DateTime.UtcNow
                };

                var createUserResult = await userManager.CreateAsync(user, password);

                if (!createUserResult.Succeeded)
                {
                    var errors = string.Join(", ", createUserResult.Errors.Select(x => x.Description));
                    throw new InvalidOperationException($"Gagal membuat user SuperAdmin: {errors}");
                }

                var addRoleResult = await userManager.AddToRoleAsync(user, SuperAdminRoleName);

                if (!addRoleResult.Succeeded)
                {
                    var errors = string.Join(", ", addRoleResult.Errors.Select(x => x.Description));
                    throw new InvalidOperationException($"Gagal menambahkan role SuperAdmin ke user: {errors}");
                }

                Console.WriteLine("[Seeder] SuperAdmin created successfully.");
                return;
            }

            Console.WriteLine("[Seeder] SuperAdmin already exists. Checking data...");

            var needUpdate = false;

            if (string.IsNullOrWhiteSpace(existingUser.UserCode))
            {
                existingUser.UserCode = SuperAdminUserCode;
                needUpdate = true;
            }

            if (string.IsNullOrWhiteSpace(existingUser.FullName))
            {
                existingUser.FullName = string.IsNullOrWhiteSpace(fullName)
                    ? "Super Administrator"
                    : fullName.Trim();

                needUpdate = true;
            }

            if (existingUser.UserType != UserType.SuperAdmin)
            {
                existingUser.UserType = UserType.SuperAdmin;
                needUpdate = true;
            }

            if (!existingUser.IsActive)
            {
                existingUser.IsActive = true;
                needUpdate = true;
            }

            if (existingUser.MustChangePassword)
            {
                existingUser.MustChangePassword = false;
                needUpdate = true;
            }

            if (string.IsNullOrWhiteSpace(existingUser.ProfilePhotoPath) ||
                existingUser.ProfilePhotoPath.Contains("/uploads/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                existingUser.ProfilePhotoPath = defaultPhotoPath;
                needUpdate = true;
            }

            if (!string.Equals(existingUser.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                existingUser.Email = email;
                existingUser.NormalizedEmail = email.ToUpperInvariant();
                existingUser.EmailConfirmed = true;
                needUpdate = true;
            }

            if (!string.Equals(existingUser.UserName, username, StringComparison.OrdinalIgnoreCase))
            {
                existingUser.UserName = username;
                existingUser.NormalizedUserName = username.ToUpperInvariant();
                needUpdate = true;
            }

            if (needUpdate)
            {
                existingUser.UpdateDateTime = DateTime.UtcNow;

                var updateResult = await userManager.UpdateAsync(existingUser);

                if (!updateResult.Succeeded)
                {
                    var errors = string.Join(", ", updateResult.Errors.Select(x => x.Description));
                    throw new InvalidOperationException($"Gagal update user SuperAdmin: {errors}");
                }

                Console.WriteLine("[Seeder] SuperAdmin updated successfully.");
            }

            var isInSuperAdminRole = await userManager.IsInRoleAsync(existingUser, SuperAdminRoleName);

            if (!isInSuperAdminRole)
            {
                var addRoleResult = await userManager.AddToRoleAsync(existingUser, SuperAdminRoleName);

                if (!addRoleResult.Succeeded)
                {
                    var errors = string.Join(", ", addRoleResult.Errors.Select(x => x.Description));
                    throw new InvalidOperationException($"Gagal menambahkan role SuperAdmin ke existing user: {errors}");
                }

                Console.WriteLine("[Seeder] SuperAdmin role assigned successfully.");
            }
        }

        private static async Task EnsureRoleAsync(
            RoleManager<ApplicationRole> roleManager,
            string roleName,
            string description,
            bool isSystemRole)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);

            if (roleExists)
            {
                return;
            }

            var role = new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
                Description = description,
                IsSystemRole = isSystemRole,
                CreateDateTime = DateTime.UtcNow
            };

            var createRoleResult = await roleManager.CreateAsync(role);

            if (!createRoleResult.Succeeded)
            {
                var errors = string.Join(", ", createRoleResult.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"Gagal membuat role {roleName}: {errors}");
            }
        }

        private static string BuildDefaultProfilePhotoUrl(IConfiguration configuration)
        {
            var uploadUrl = configuration["FileStorage:UploadUrl"]?.Trim();
            var folder = configuration["FileStorage:ProfilePhotoFolder"]?.Trim();

            if (string.IsNullOrWhiteSpace(uploadUrl))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(folder))
            {
                folder = "default-photo";
            }

            uploadUrl = uploadUrl.TrimEnd('/');
            folder = folder.Trim('/');

            if (folder.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
            {
                folder = folder["uploads/".Length..];
            }

            if (folder.StartsWith("upload/", StringComparison.OrdinalIgnoreCase))
            {
                folder = folder["upload/".Length..];
            }

            var baseUrl = uploadUrl;

            if (baseUrl.EndsWith("/upload", StringComparison.OrdinalIgnoreCase))
            {
                baseUrl = baseUrl[..^"/upload".Length];
            }

            if (baseUrl.EndsWith("/uploads", StringComparison.OrdinalIgnoreCase))
            {
                baseUrl = baseUrl[..^"/uploads".Length];
            }

            return $"{baseUrl}/uploads/{folder}/{DefaultProfilePhotoFileName}";
        }
    }
}