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

            var routePath = BuildRoutePath(controllerAction);

            var controller = await dbContext.SysControllerAccesses
                .FirstOrDefaultAsync(x =>
                    x.ModuleId == moduleId &&
                    x.ControllerName == controllerName);

            if (controller != null)
            {
                controller.DisplayName = attribute.DisplayName;
                controller.RoutePath = GetControllerRoute(routePath);
                controller.Description = attribute.Description;
                controller.SortOrder = attribute.SortOrder;
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
                RoutePath = GetControllerRoute(routePath),
                Description = attribute.Description,
                SortOrder = attribute.SortOrder,
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
            AccessActionAttribute attribute)
        {
            var routePath = BuildRoutePath(controllerAction);
            var httpMethod = GetHttpMethod(controllerAction);

            var action = await dbContext.SysActionAccesses
                .FirstOrDefaultAsync(x =>
                    x.ControllerAccessId == controllerAccessId &&
                    x.ActionName == attribute.ActionName);

            if (action != null)
            {
                action.DisplayName = attribute.DisplayName;
                action.HttpMethod = httpMethod;
                action.RoutePath = routePath;
                action.Description = attribute.Description;
                action.SortOrder = attribute.SortOrder;
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
                RoutePath = routePath,
                Description = attribute.Description,
                SortOrder = attribute.SortOrder,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                IsDelete = false,
                IsCancel = false
            };

            dbContext.SysActionAccesses.Add(action);

            await dbContext.SaveChangesAsync();
        }

        private static string BuildRoutePath(ControllerActionDescriptor controllerAction)
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

        private static string GetControllerRoute(string actionRoute)
        {
            var route = actionRoute;

            var parameterIndex = route.IndexOf("/{", StringComparison.OrdinalIgnoreCase);

            if (parameterIndex >= 0)
            {
                route = route[..parameterIndex];
            }

            var knownActionSegments = new[]
            {
                "/index",
                "/create",
                "/detail",
                "/update",
                "/delete",
                "/edit",
                "/view",
                "/print",
                "/export",
                "/import",
                "/approve",
                "/cancel"
            };

            foreach (var segment in knownActionSegments)
            {
                if (route.EndsWith(segment, StringComparison.OrdinalIgnoreCase))
                {
                    route = route[..^segment.Length];
                    break;
                }
            }

            return route;
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