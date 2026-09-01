using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using QuilvianSystemBackend.Attributes;
using System.Reflection;

namespace QuilvianSystemBackend.Services.Security
{
    /// <summary>
    /// Menurunkan identitas permission kanonik dari atribut endpoint.
    ///
    /// <para><b>Aturan kanonik Phase A0.</b> Identitas sebuah kemampuan adalah pasangan
    /// <c>(resource, action)</c> yang ditulis pada <c>[AccessPermission]</c> — persis nilai yang
    /// dicari <c>AccessPermissionService.HasAccessAsync</c> saat request masuk. Seeder mendaftarkan
    /// pasangan itu apa adanya, sehingga baris registry yang dibuat selalu identik dengan kunci
    /// yang dicari runtime.</para>
    ///
    /// <para>Sebelum A0, seeder menyimpan argumen pertama <c>[AccessAction]</c> sementara filter
    /// mencari argumen kedua <c>[AccessPermission]</c>, dan tidak ada apa pun yang memaksa keduanya
    /// sama. Audit menemukan 89 endpoint yang keduanya berbeda; seluruhnya menolak setiap pengguna
    /// non-SuperAdmin dengan 403 permanen, dan admin tidak punya baris untuk dicentang karena
    /// barisnya memang tidak pernah dibuat.</para>
    ///
    /// <para>Dengan menjadikan <c>[AccessPermission]</c> sebagai sumber identitas, selisih semacam
    /// itu tidak lagi mungkin terjadi. <c>[AccessAction]</c> tetap wajib pada method yang sama,
    /// tetapi perannya kini murni metadata tampilan: nama tampil, deskripsi, urutan, dan
    /// <c>AccessType</c> yang menentukan kolom pada layar Akses Role.</para>
    ///
    /// <para>Aturan ini juga menghormati kontrak yang sudah disetujui — misalnya
    /// <c>opr-permission-v1</c> dan kontrak terkunci Billing — yang memang menamai permission
    /// menurut <i>resource bisnis</i> (<c>BillingRefund</c>, <c>OperatingRoomAnesthesia</c>) dan
    /// bukan menurut nama class controller.</para>
    /// </summary>
    public static class PermissionRegistryDescriptor
    {
        public sealed record ModuleDescriptor(
            string ModuleCode,
            string ModuleName,
            string? AreaName,
            string? Description,
            int SortOrder);

        /// <summary>Satu resource permission. Dipetakan ke satu baris <c>SysControllerAccess</c>.</summary>
        public sealed record ResourceDescriptor(
            string ModuleCode,
            string ResourceName,
            string DisplayName,
            string? Description,
            int SortOrder,
            bool VisibleInRoleAccess,
            bool IsSystemOnly);

        /// <summary>Satu kemampuan. Dipetakan ke satu baris <c>SysActionAccess</c>.</summary>
        public sealed record ActionDescriptorEntry(
            string ModuleCode,
            string ResourceName,
            string ActionName,
            string DisplayName,
            string? Description,
            string AccessType,
            int SortOrder,
            bool VisibleInRoleAccess,
            bool IsSystemOnly,
            string? HttpMethod,
            string? RoutePath);

        /// <summary>Endpoint yang memakai <c>[AccessPermission]</c> tanpa <c>[AccessAction]</c>.</summary>
        public sealed record MetadataGap(
            string ResourceName,
            string ActionName,
            string DeclaringController,
            string MethodName);

        public sealed class RegistrySnapshot
        {
            public List<ModuleDescriptor> Modules { get; } = new();
            public List<ResourceDescriptor> Resources { get; } = new();
            public List<ActionDescriptorEntry> Actions { get; } = new();

            /// <summary>Endpoint terproteksi yang belum punya metadata untuk layar Akses Role.</summary>
            public List<MetadataGap> MetadataGaps { get; } = new();

            /// <summary>Endpoint ber-<c>[AccessAction]</c> yang tidak ditegakkan permission apa pun.</summary>
            public List<MetadataGap> UnenforcedActions { get; } = new();

            public HashSet<string> DeclaredKeys { get; } = new(StringComparer.Ordinal);

            public static string Key(string resourceName, string actionName) =>
                $"{resourceName}|{actionName}";
        }

        private sealed record EndpointFacts(
            AccessControllerAttribute Controller,
            AccessActionAttribute? Action,
            IReadOnlyList<AccessPermissionAttribute> Permissions,
            string DeclaringControllerName,
            string MethodName,
            string? HttpMethod,
            string? RoutePath);

        public static RegistrySnapshot Build(IActionDescriptorCollectionProvider provider)
        {
            var endpoints = provider
                .ActionDescriptors
                .Items
                .OfType<ControllerActionDescriptor>()
                .Select(descriptor =>
                {
                    var controllerAttribute = descriptor.ControllerTypeInfo
                        .GetCustomAttribute<AccessControllerAttribute>();

                    if (controllerAttribute == null)
                    {
                        return null;
                    }

                    return new EndpointFacts(
                        controllerAttribute,
                        descriptor.MethodInfo.GetCustomAttribute<AccessActionAttribute>(),
                        descriptor.MethodInfo.GetCustomAttributes<AccessPermissionAttribute>().ToList(),
                        ResolveControllerName(controllerAttribute, descriptor.ControllerName),
                        descriptor.MethodInfo.Name,
                        GetHttpMethod(descriptor),
                        BuildActionRoutePath(descriptor));
                })
                .Where(x => x != null)
                .Select(x => x!)
                .ToList();

            return BuildCore(endpoints);
        }

        /// <summary>
        /// Membangun snapshot langsung dari assembly, tanpa host MVC. Dipakai test invarian supaya
        /// registry yang diuji benar-benar diturunkan dari atribut yang sama dengan seeder.
        /// </summary>
        public static RegistrySnapshot BuildFromAssembly(Assembly assembly)
        {
            var endpoints = new List<EndpointFacts>();

            var controllerTypes = assembly
                .GetTypes()
                .Where(x => x is { IsClass: true, IsAbstract: false } &&
                            x.GetCustomAttribute<AccessControllerAttribute>() != null);

            foreach (var controllerType in controllerTypes)
            {
                var controllerAttribute = controllerType.GetCustomAttribute<AccessControllerAttribute>()!;
                var controllerName = ResolveControllerName(
                    controllerAttribute,
                    controllerType.Name.Replace("Controller", string.Empty));

                foreach (var method in controllerType.GetMethods(
                             BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    var actionAttribute = method.GetCustomAttribute<AccessActionAttribute>();
                    var permissions = method.GetCustomAttributes<AccessPermissionAttribute>().ToList();

                    if (actionAttribute == null && permissions.Count == 0)
                    {
                        continue;
                    }

                    endpoints.Add(new EndpointFacts(
                        controllerAttribute,
                        actionAttribute,
                        permissions,
                        controllerName,
                        method.Name,
                        null,
                        null));
                }
            }

            return BuildCore(endpoints);
        }

        private static RegistrySnapshot BuildCore(IReadOnlyList<EndpointFacts> endpoints)
        {
            var snapshot = new RegistrySnapshot();

            var moduleSeen = new HashSet<string>(StringComparer.Ordinal);
            var resourceSeen = new HashSet<string>(StringComparer.Ordinal);
            var actionSeen = new HashSet<string>(StringComparer.Ordinal);
            var allUsages = new List<MetadataGap>();

            foreach (var endpoint in endpoints)
            {
                var controllerAttribute = endpoint.Controller;

                if (moduleSeen.Add(controllerAttribute.ModuleCode))
                {
                    snapshot.Modules.Add(new ModuleDescriptor(
                        controllerAttribute.ModuleCode,
                        controllerAttribute.ModuleName,
                        controllerAttribute.AreaName,
                        controllerAttribute.Description,
                        controllerAttribute.SortOrder));
                }

                var controllerIsSystemOnly = controllerAttribute.IsSystemOnly;
                var controllerVisible = !controllerIsSystemOnly && controllerAttribute.VisibleInRoleAccess;

                // Endpoint ber-[AccessAction] tanpa [AccessPermission] tidak ditegakkan matriks
                // Akses Role — biasanya karena dilindungi policy perangkat seperti KioskRead.
                // Ia tetap didaftarkan memakai nama controller-nya, persis seperti perilaku sebelum
                // Phase A0, karena endpoint saudaranya bisa jadi menegakkan kunci yang sama.
                // Mencabut pendaftaran ini akan mematikan endpoint yang hari ini berfungsi.
                if (endpoint.Permissions.Count == 0)
                {
                    if (endpoint.Action != null)
                    {
                        snapshot.UnenforcedActions.Add(new MetadataGap(
                            endpoint.DeclaringControllerName,
                            endpoint.Action.ActionName,
                            endpoint.DeclaringControllerName,
                            endpoint.MethodName));

                        RegisterAction(
                            snapshot, resourceSeen, actionSeen, controllerAttribute, endpoint,
                            endpoint.DeclaringControllerName, endpoint.Action.ActionName,
                            controllerIsSystemOnly, controllerVisible);
                    }

                    continue;
                }

                foreach (var permission in endpoint.Permissions)
                {
                    if (permission.Arguments is not { Length: 2 } ||
                        permission.Arguments[0] is not string resourceName ||
                        permission.Arguments[1] is not string actionName ||
                        string.IsNullOrWhiteSpace(resourceName) ||
                        string.IsNullOrWhiteSpace(actionName))
                    {
                        continue;
                    }

                    // Endpoint alias yang mendelegasikan ke endpoint lain sengaja tidak mengulang
                    // [AccessAction]; kemampuannya sudah didaftarkan endpoint aslinya. Kelengkapan
                    // metadata karena itu diperiksa per kunci, bukan per method — lihat pass kedua.
                    allUsages.Add(new MetadataGap(
                        resourceName, actionName, endpoint.DeclaringControllerName, endpoint.MethodName));

                    if (endpoint.Action == null)
                    {
                        continue;
                    }

                    RegisterAction(
                        snapshot, resourceSeen, actionSeen, controllerAttribute, endpoint,
                        resourceName, actionName, controllerIsSystemOnly, controllerVisible);
                }
            }

            // Pass kedua: sebuah kunci runtime baru dianggap bermasalah bila TIDAK ADA satu pun
            // endpoint yang memberinya metadata. Endpoint alias yang mendelegasikan ke endpoint
            // lain karena itu tidak dilaporkan selama kunci yang sama sudah terdaftar.
            foreach (var usage in allUsages)
            {
                if (!snapshot.DeclaredKeys.Contains(RegistrySnapshot.Key(usage.ResourceName, usage.ActionName)))
                {
                    snapshot.MetadataGaps.Add(usage);
                }
            }

            return snapshot;
        }

        private static void RegisterAction(
            RegistrySnapshot snapshot,
            HashSet<string> resourceSeen,
            HashSet<string> actionSeen,
            AccessControllerAttribute controllerAttribute,
            EndpointFacts endpoint,
            string resourceName,
            string actionName,
            bool controllerIsSystemOnly,
            bool controllerVisible)
        {
            var action = endpoint.Action!;
            var isSystemOnly = controllerIsSystemOnly || action.IsSystemOnly;
            var visible = !isSystemOnly && controllerVisible && action.VisibleInRoleAccess;

            var resourceKey = $"{controllerAttribute.ModuleCode}|{resourceName}";
            if (resourceSeen.Add(resourceKey))
            {
                // Resource yang namanya sama dengan controller memakai DisplayName controller.
                // Resource turunan memakai namanya sendiri agar tetap terbaca di layar Akses Role.
                var displayName =
                    string.Equals(resourceName, endpoint.DeclaringControllerName, StringComparison.Ordinal)
                        ? controllerAttribute.DisplayName
                        : resourceName;

                snapshot.Resources.Add(new ResourceDescriptor(
                    controllerAttribute.ModuleCode,
                    resourceName,
                    displayName,
                    controllerAttribute.Description,
                    controllerAttribute.SortOrder,
                    visible,
                    isSystemOnly));
            }

            var actionKey = $"{controllerAttribute.ModuleCode}|{resourceName}|{actionName}";
            if (actionSeen.Add(actionKey))
            {
                snapshot.Actions.Add(new ActionDescriptorEntry(
                    controllerAttribute.ModuleCode,
                    resourceName,
                    actionName,
                    action.DisplayName,
                    action.Description,
                    action.AccessType,
                    action.SortOrder,
                    visible,
                    isSystemOnly,
                    endpoint.HttpMethod,
                    endpoint.RoutePath));
            }

            snapshot.DeclaredKeys.Add(RegistrySnapshot.Key(resourceName, actionName));
        }

        private static string ResolveControllerName(AccessControllerAttribute attribute, string fallback) =>
            string.IsNullOrWhiteSpace(attribute.ControllerName) ? fallback : attribute.ControllerName!;

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

            return template.StartsWith("/") ? template : "/" + template;
        }

        private static string GetHttpMethod(ControllerActionDescriptor controllerAction)
        {
            var constraint = controllerAction
                .ActionConstraints?
                .OfType<Microsoft.AspNetCore.Mvc.ActionConstraints.HttpMethodActionConstraint>()
                .FirstOrDefault();

            return constraint?.HttpMethods.FirstOrDefault() ?? "GET";
        }
    }
}
