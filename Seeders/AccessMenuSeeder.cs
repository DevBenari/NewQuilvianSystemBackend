using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using System.Reflection;

namespace QuilvianSystemBackend.Seeders
{
    public static class AccessMenuSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var actionDescriptorProvider = scope.ServiceProvider.GetRequiredService<IActionDescriptorCollectionProvider>();

            var controllerActions = actionDescriptorProvider
                .ActionDescriptors
                .Items
                .OfType<ControllerActionDescriptor>()
                .ToList();

            foreach (var controllerAction in controllerActions)
            {
                var controllerAttribute = controllerAction
                    .ControllerTypeInfo
                    .GetCustomAttribute<AccessControllerAttribute>();

                if (controllerAttribute == null)
                {
                    continue;
                }

                var actionAttribute = controllerAction
                    .MethodInfo
                    .GetCustomAttribute<AccessActionAttribute>();

                if (actionAttribute == null)
                {
                    continue;
                }

                var module = await EnsureModuleAsync(
                    dbContext,
                    controllerAttribute
                );

                var controller = await EnsureControllerAsync(
                    dbContext,
                    module.Id,
                    controllerAction,
                    controllerAttribute
                );

                await EnsureActionAsync(
                    dbContext,
                    controller.Id,
                    controllerAction,
                    controllerAttribute,
                    actionAttribute
                );
            }

            await dbContext.SaveChangesAsync();
        }

        private static async Task<SysApplicationModule> EnsureModuleAsync(
            ApplicationDbContext dbContext,
            AccessControllerAttribute attribute)
        {
            var module = await dbContext.SysApplicationModules
                .FirstOrDefaultAsync(x => x.ModuleCode == attribute.ModuleCode);

            if (module != null)
            {
                module.ModuleName = attribute.ModuleName;
                module.AreaName = attribute.AreaName;
                module.Description = attribute.Description;
                module.SortOrder = attribute.SortOrder;
                module.IsActive = true;
                module.IsDelete = false;

                return module;
            }

            module = new SysApplicationModule
            {
                Id = Guid.NewGuid(),
                ModuleCode = attribute.ModuleCode,
                ModuleName = attribute.ModuleName,
                AreaName = attribute.AreaName,
                Description = attribute.Description,
                SortOrder = attribute.SortOrder,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                IsDelete = false,
                IsCancel = false
            };

            dbContext.SysApplicationModules.Add(module);

            await dbContext.SaveChangesAsync();

            return module;
        }

        private static async Task<SysControllerAccess> EnsureControllerAsync(
            ApplicationDbContext dbContext,
            Guid moduleId,
            ControllerActionDescriptor controllerAction,
            AccessControllerAttribute attribute)
        {
            var controllerName = string.IsNullOrWhiteSpace(attribute.ControllerName)
                ? controllerAction.ControllerName
                : attribute.ControllerName;

            var controllerRoutePath = BuildControllerRoutePath(controllerAction);

            var controller = await dbContext.SysControllerAccesses
                .FirstOrDefaultAsync(x =>
                    x.ModuleId == moduleId &&
                    x.ControllerName == controllerName);

            if (controller != null)
            {
                controller.DisplayName = attribute.DisplayName;
                controller.RoutePath = controllerRoutePath;
                controller.Description = attribute.Description;
                controller.SortOrder = attribute.SortOrder;
                controller.VisibleInRoleAccess = attribute.VisibleInRoleAccess;
                controller.IsSystemOnly = attribute.IsSystemOnly;
                controller.IsActive = true;
                controller.IsDelete = false;

                return controller;
            }

            controller = new SysControllerAccess
            {
                Id = Guid.NewGuid(),
                ModuleId = moduleId,
                ControllerName = controllerName,
                DisplayName = attribute.DisplayName,
                RoutePath = controllerRoutePath,
                Description = attribute.Description,
                SortOrder = attribute.SortOrder,
                VisibleInRoleAccess = attribute.VisibleInRoleAccess,
                IsSystemOnly = attribute.IsSystemOnly,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                IsDelete = false,
                IsCancel = false
            };

            dbContext.SysControllerAccesses.Add(controller);

            await dbContext.SaveChangesAsync();

            return controller;
        }

        private static async Task EnsureActionAsync(
            ApplicationDbContext dbContext,
            Guid controllerAccessId,
            ControllerActionDescriptor controllerAction,
            AccessControllerAttribute controllerAttribute,
            AccessActionAttribute attribute)
        {
            var actionRoutePath = BuildActionRoutePath(controllerAction);
            var httpMethod = GetHttpMethod(controllerAction);

            var visibleInRoleAccess =
                controllerAttribute.VisibleInRoleAccess &&
                !controllerAttribute.IsSystemOnly &&
                attribute.VisibleInRoleAccess &&
                !attribute.IsSystemOnly;

            var isSystemOnly =
                controllerAttribute.IsSystemOnly ||
                attribute.IsSystemOnly;
            
            var action = await dbContext.SysActionAccesses
                .FirstOrDefaultAsync(x =>
                    x.ControllerAccessId == controllerAccessId &&
                    x.ActionName == attribute.ActionName);

            if (action != null)
            {
                action.DisplayName = attribute.DisplayName;
                action.HttpMethod = httpMethod;
                action.RoutePath = actionRoutePath;
                action.Description = attribute.Description;
                action.SortOrder = attribute.SortOrder;
                action.AccessType = attribute.AccessType;
                action.VisibleInRoleAccess = visibleInRoleAccess;
                action.IsSystemOnly = isSystemOnly;
                action.IsActive = true;
                action.IsDelete = false;

                return;
            }

            action = new SysActionAccess
            {
                Id = Guid.NewGuid(),
                ControllerAccessId = controllerAccessId,
                ActionName = attribute.ActionName,
                DisplayName = attribute.DisplayName,
                HttpMethod = httpMethod,
                RoutePath = actionRoutePath,
                Description = attribute.Description,
                SortOrder = attribute.SortOrder,
                AccessType = attribute.AccessType,
                VisibleInRoleAccess = visibleInRoleAccess,
                IsSystemOnly = isSystemOnly,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                IsDelete = false,
                IsCancel = false
            };

            dbContext.SysActionAccesses.Add(action);

            await dbContext.SaveChangesAsync();
        }

        private static string BuildControllerRoutePath(ControllerActionDescriptor controllerAction)
        {
            var routeAttribute = controllerAction.ControllerTypeInfo
                .GetCustomAttribute<RouteAttribute>();

            var template = routeAttribute?.Template;

            if (string.IsNullOrWhiteSpace(template))
            {
                return $"/api/v1/{controllerAction.ControllerName}";
            }

            template = template
                .Replace("[controller]", controllerAction.ControllerName)
                .Replace("[action]", controllerAction.ActionName);

            if (!template.StartsWith("/"))
            {
                template = "/" + template;
            }

            return template;
        }

        private static string BuildActionRoutePath(ControllerActionDescriptor controllerAction)
        {
            var template = controllerAction.AttributeRouteInfo?.Template;

            if (string.IsNullOrWhiteSpace(template))
            {
                return $"/api/v1/{controllerAction.ControllerName}/{controllerAction.ActionName}";
            }

            template = template
                .Replace("[controller]", controllerAction.ControllerName)
                .Replace("[action]", controllerAction.ActionName);

            if (!template.StartsWith("/"))
            {
                template = "/" + template;
            }

            return template;
        }

        private static string GetHttpMethod(ControllerActionDescriptor controllerAction)
        {
            var httpMethodActionConstraint = controllerAction
                .ActionConstraints?
                .OfType<HttpMethodActionConstraint>()
                .FirstOrDefault();

            var httpMethod = httpMethodActionConstraint?
                .HttpMethods
                .FirstOrDefault();

            return httpMethod ?? "GET";
        }
    }
}