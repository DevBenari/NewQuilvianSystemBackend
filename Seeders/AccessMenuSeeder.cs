using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Seeders
{
    /// <summary>
    /// Mendaftarkan kemampuan aplikasi ke registry Akses Role.
    ///
    /// Seeder ini <b>hanya</b> mengelola tiga tabel registry: <c>SysApplicationModule</c>,
    /// <c>SysControllerAccess</c>, dan <c>SysActionAccess</c>. Ia tidak pernah membuat
    /// <c>SysAccessPolicy</c>. Kemampuan yang baru terdaftar tetap ditolak untuk semua orang
    /// sampai admin memberikannya lewat layar Akses Role.
    ///
    /// Sejak Phase A0 penutupan baris usang dilakukan secara generik. Sebelumnya setiap
    /// perpindahan modul atau penggantian nama menuntut satu fungsi <c>Normalize...</c> baru yang
    /// ditulis tangan, dan yang terlewat tetap tampil di layar Akses Role sebagai kemampuan yang
    /// sebenarnya sudah tidak ada. Audit Phase A0 menemukan 59 baris seperti itu.
    /// </summary>
    public static class AccessMenuSeeder
    {
        public sealed record ReconciliationResult(
            int ModulesUpserted,
            int ControllersUpserted,
            int ActionsUpserted,
            int ModulesClosed,
            int ControllersClosed,
            int ActionsClosed);

        public static async Task<ReconciliationResult> SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var actionDescriptorProvider = scope.ServiceProvider
                .GetRequiredService<Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider>();

            var snapshot = PermissionRegistryDescriptor.Build(actionDescriptorProvider);

            return await ReconcileAsync(dbContext, snapshot);
        }

        /// <summary>
        /// Dipisahkan dari <see cref="SeedAsync"/> supaya dapat diuji tanpa host penuh.
        /// </summary>
        public static async Task<ReconciliationResult> ReconcileAsync(
            ApplicationDbContext dbContext,
            PermissionRegistryDescriptor.RegistrySnapshot snapshot)
        {
            var now = DateTime.UtcNow;

            var modules = await dbContext.SysApplicationModules.ToListAsync();
            var modulesByCode = modules
                .GroupBy(x => x.ModuleCode, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

            var modulesUpserted = 0;
            foreach (var descriptor in snapshot.Modules)
            {
                if (!modulesByCode.TryGetValue(descriptor.ModuleCode, out var module))
                {
                    module = new SysApplicationModule
                    {
                        Id = Guid.NewGuid(),
                        ModuleCode = descriptor.ModuleCode,
                        CreateDateTime = now,
                        IsCancel = false
                    };

                    dbContext.SysApplicationModules.Add(module);
                    modulesByCode.Add(descriptor.ModuleCode, module);
                }

                module.ModuleName = descriptor.ModuleName;
                module.AreaName = descriptor.AreaName;
                module.Description = descriptor.Description;
                module.SortOrder = descriptor.SortOrder;
                module.IsActive = true;
                module.IsDelete = false;
                modulesUpserted++;
            }

            await dbContext.SaveChangesAsync();

            var controllers = await dbContext.SysControllerAccesses.ToListAsync();
            var controllersByKey = controllers
                .GroupBy(x => BuildControllerKey(x.ModuleId, x.ControllerName), StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

            var controllersUpserted = 0;
            foreach (var descriptor in snapshot.Resources)
            {
                var module = modulesByCode[descriptor.ModuleCode];
                var key = BuildControllerKey(module.Id, descriptor.ResourceName);

                if (!controllersByKey.TryGetValue(key, out var controller))
                {
                    controller = new SysControllerAccess
                    {
                        Id = Guid.NewGuid(),
                        ModuleId = module.Id,
                        ControllerName = descriptor.ResourceName,
                        CreateDateTime = now,
                        IsCancel = false
                    };

                    dbContext.SysControllerAccesses.Add(controller);
                    controllersByKey.Add(key, controller);
                }

                controller.DisplayName = descriptor.DisplayName;
                controller.Description = descriptor.Description;
                controller.SortOrder = descriptor.SortOrder;
                controller.VisibleInRoleAccess = descriptor.VisibleInRoleAccess;
                controller.IsSystemOnly = descriptor.IsSystemOnly;
                controller.IsActive = true;
                controller.IsDelete = false;
                controllersUpserted++;
            }

            await dbContext.SaveChangesAsync();

            var actions = await dbContext.SysActionAccesses.ToListAsync();
            var actionsByKey = actions
                .GroupBy(x => BuildActionKey(x.ControllerAccessId, x.ActionName), StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

            var actionsUpserted = 0;
            foreach (var descriptor in snapshot.Actions)
            {
                var module = modulesByCode[descriptor.ModuleCode];
                var controller = controllersByKey[BuildControllerKey(module.Id, descriptor.ResourceName)];
                var key = BuildActionKey(controller.Id, descriptor.ActionName);

                if (!actionsByKey.TryGetValue(key, out var action))
                {
                    action = new SysActionAccess
                    {
                        Id = Guid.NewGuid(),
                        ControllerAccessId = controller.Id,
                        ActionName = descriptor.ActionName,
                        CreateDateTime = now,
                        IsCancel = false
                    };

                    dbContext.SysActionAccesses.Add(action);
                    actionsByKey.Add(key, action);
                }

                action.DisplayName = descriptor.DisplayName;
                action.HttpMethod = descriptor.HttpMethod;
                action.RoutePath = descriptor.RoutePath;
                action.Description = descriptor.Description;
                action.AccessType = descriptor.AccessType;
                action.SortOrder = descriptor.SortOrder;
                action.VisibleInRoleAccess = descriptor.VisibleInRoleAccess;
                action.IsSystemOnly = descriptor.IsSystemOnly;
                action.IsActive = true;
                action.IsDelete = false;
                actionsUpserted++;
            }

            await dbContext.SaveChangesAsync();

            var closed = await CloseRowsAbsentFromSourceAsync(
                dbContext, snapshot, modulesByCode, controllersByKey, now);

            await NormalizeSystemOnlyVisibilityAsync(dbContext);
            await dbContext.SaveChangesAsync();

            return new ReconciliationResult(
                modulesUpserted,
                controllersUpserted,
                actionsUpserted,
                closed.Modules,
                closed.Controllers,
                closed.Actions);
        }

        /// <summary>
        /// Menutup baris registry yang tidak lagi punya deskriptor di source.
        ///
        /// Penutupan memakai lifecycle repository (<c>IsActive=false</c>, <c>IsDelete=true</c>,
        /// <c>VisibleInRoleAccess=false</c>) dan bukan hard delete, supaya sejarah tetap ada dan
        /// referensi <c>SysAccessPolicy</c> lama tidak menggantung. Policy sendiri tidak disentuh:
        /// memindahkannya ke kemampuan baru sama saja memberi hak tanpa keputusan admin.
        /// </summary>
        private static async Task<(int Modules, int Controllers, int Actions)> CloseRowsAbsentFromSourceAsync(
            ApplicationDbContext dbContext,
            PermissionRegistryDescriptor.RegistrySnapshot snapshot,
            IDictionary<string, SysApplicationModule> modulesByCode,
            IDictionary<string, SysControllerAccess> controllersByKey,
            DateTime now)
        {
            var declaredModuleIds = snapshot.Modules
                .Select(x => modulesByCode[x.ModuleCode].Id)
                .ToHashSet();

            var declaredControllerIds = snapshot.Resources
                .Select(x => controllersByKey[BuildControllerKey(modulesByCode[x.ModuleCode].Id, x.ResourceName)].Id)
                .ToHashSet();

            var declaredActionKeys = snapshot.Actions
                .Select(x => BuildActionKey(
                    controllersByKey[BuildControllerKey(modulesByCode[x.ModuleCode].Id, x.ResourceName)].Id,
                    x.ActionName))
                .ToHashSet(StringComparer.Ordinal);

            var actionsClosed = 0;
            var openActions = await dbContext.SysActionAccesses
                .Where(x => x.IsActive || !x.IsDelete)
                .ToListAsync();

            foreach (var action in openActions)
            {
                if (declaredActionKeys.Contains(BuildActionKey(action.ControllerAccessId, action.ActionName)))
                {
                    continue;
                }

                action.IsActive = false;
                action.IsDelete = true;
                action.VisibleInRoleAccess = false;
                action.DeleteDateTime ??= now;
                action.UpdateDateTime = now;
                actionsClosed++;
            }

            var controllersClosed = 0;
            var openControllers = await dbContext.SysControllerAccesses
                .Where(x => x.IsActive || !x.IsDelete)
                .ToListAsync();

            foreach (var controller in openControllers)
            {
                if (declaredControllerIds.Contains(controller.Id))
                {
                    continue;
                }

                controller.IsActive = false;
                controller.IsDelete = true;
                controller.VisibleInRoleAccess = false;
                controller.DeleteDateTime ??= now;
                controller.UpdateDateTime = now;
                controllersClosed++;
            }

            var modulesClosed = 0;
            var openModules = await dbContext.SysApplicationModules
                .Where(x => x.IsActive || !x.IsDelete)
                .ToListAsync();

            foreach (var module in openModules)
            {
                if (declaredModuleIds.Contains(module.Id))
                {
                    continue;
                }

                module.IsActive = false;
                module.IsDelete = true;
                module.DeleteDateTime ??= now;
                module.UpdateDateTime = now;
                modulesClosed++;
            }

            await dbContext.SaveChangesAsync();

            return (modulesClosed, controllersClosed, actionsClosed);
        }

        private static string BuildControllerKey(Guid moduleId, string controllerName) =>
            $"{moduleId:N}:{controllerName}";

        private static string BuildActionKey(Guid controllerAccessId, string actionName) =>
            $"{controllerAccessId:N}:{actionName}";

        private static async Task NormalizeSystemOnlyVisibilityAsync(ApplicationDbContext dbContext)
        {
            var systemOnlyControllers = await dbContext.SysControllerAccesses
                .Where(x => x.IsSystemOnly && x.VisibleInRoleAccess)
                .ToListAsync();

            foreach (var controller in systemOnlyControllers)
            {
                controller.VisibleInRoleAccess = false;
            }

            var systemOnlyActions = await dbContext.SysActionAccesses
                .Where(x => x.IsSystemOnly && x.VisibleInRoleAccess)
                .ToListAsync();

            foreach (var action in systemOnlyActions)
            {
                action.VisibleInRoleAccess = false;
            }

            var actionsUnderSystemOnlyController = await dbContext.SysActionAccesses
                .Where(a =>
                    a.VisibleInRoleAccess &&
                    a.ControllerAccess != null &&
                    a.ControllerAccess.IsSystemOnly)
                .ToListAsync();

            foreach (var action in actionsUnderSystemOnlyController)
            {
                action.VisibleInRoleAccess = false;
                action.IsSystemOnly = true;
            }
        }
    }
}
