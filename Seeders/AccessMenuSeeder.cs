using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using System.Reflection;

namespace QuilvianSystemBackend.Seeders
{
    public static class AccessMenuSeeder
    {
        /// <summary>
        /// Hak akses <b>penanda</b>: pasangan yang dibaca service, bukan yang menempel pada
        /// sebuah endpoint.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Seluruh pasangan hak akses lain lahir dari pemindaian action MVC yang punya
        /// <c>[AccessController]</c> dan <c>[AccessAction]</c>. Sebagian aturan keselamatan
        /// tidak berbentuk begitu: ia menjawab "orang ini dihitung sebagai apa", bukan "orang
        /// ini boleh memanggil endpoint apa", sehingga tidak ada endpoint yang pantas
        /// menampungnya.
        /// </para>
        /// <para>
        /// <b>Tanpa daftar ini, pasangan semacam itu tidak pernah masuk
        /// <c>SysActionAccess</c>.</b> Akibatnya bukan galat yang terlihat, melainkan
        /// <c>403</c> permanen yang <b>tidak dapat diperbaiki dari layar mana pun</b> — karena
        /// baris untuk dicentangnya memang tidak ada. Itulah yang terjadi pada
        /// <c>RadReport : ActAsRadiologist</c> sejak <c>BE-RAD-08</c> sampai penambahan ini.
        /// </para>
        /// <para>
        /// Daftar ini <b>sengaja pendek dan ditulis tangan</b>. Setiap baris di sini adalah
        /// kewenangan yang tidak dapat ditelusuri dari source dengan cara biasa, jadi ia harus
        /// terbaca di satu tempat yang mudah diperiksa saat review.
        /// </para>
        /// </remarks>
        public static readonly PenandaTanpaEndpoint[] PenandaTanpaEndpointYangDidaftarkan =
        [
            // RAD-DEC-015, RAD-PERM-001 bagian 6. Dibaca RadReportService ketika membekukan
            // peran penulis draf dan ketika memutuskan boleh-tidaknya pengesahan sendiri.
            //
            // Sengaja TIDAK digabung dengan RadReport : Validate. "Boleh mencoba mengesahkan"
            // dan "dihitung sebagai dokter radiolog" adalah dua hal berbeda: seorang residen
            // dapat diberi Validate supaya dapat mengesahkan draf radiografer, dan tanpa
            // penanda ini draf yang ia tulis sendiri tetap ditolak. Menggabungkannya menghapus
            // aturan RAD-DEC-003.
            new(
                ModuleCode: "HEALTH_SERVICE_RADIOLOGY_MANAGEMENT",
                ControllerName: "RadReport",
                ActionName: "ActAsRadiologist",
                DisplayName: "Act As Radiologist",
                Description: "Dihitung sebagai dokter radiolog saat menulis dan mengesahkan hasil bacaan",
                AccessType: AccessTypes.Update,
                SortOrder: 7),
        ];

        /// <summary>Satu hak akses penanda yang didaftarkan tanpa endpoint.</summary>
        public sealed record PenandaTanpaEndpoint(
            string ModuleCode,
            string ControllerName,
            string ActionName,
            string DisplayName,
            string Description,
            string AccessType,
            int SortOrder);

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

            var modules = await dbContext.SysApplicationModules.ToListAsync();
            var controllers = await dbContext.SysControllerAccesses.ToListAsync();
            var actions = await dbContext.SysActionAccesses.ToListAsync();
            var modulesByCode = modules.ToDictionary(x => x.ModuleCode, StringComparer.Ordinal);
            var controllersByModuleAndName = controllers.ToDictionary(
                x => BuildControllerKey(x.ModuleId, x.ControllerName),
                StringComparer.Ordinal);
            var actionsByControllerAndName = actions.ToDictionary(
                x => BuildActionKey(x.ControllerAccessId, x.ActionName),
                StringComparer.Ordinal);

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

                var module = EnsureModule(dbContext, modulesByCode, controllerAttribute);

                var controller = EnsureController(
                    dbContext,
                    controllersByModuleAndName,
                    module.Id,
                    controllerAction,
                    controllerAttribute
                );

                EnsureAction(
                    dbContext,
                    actionsByControllerAndName,
                    controller.Id,
                    controllerAction,
                    controllerAttribute,
                    actionAttribute
                );
            }

            // Dijalankan SETELAH pemindaian, karena baris controllernya lahir dari pemindaian
            // itu. Sebuah penanda tidak dapat didaftarkan pada controller yang belum ada.
            EnsurePenandaTanpaEndpoint(
                dbContext,
                modulesByCode,
                controllersByModuleAndName,
                actionsByControllerAndName);

            await dbContext.SaveChangesAsync();

            await NormalizeEmployeeSelfServiceLegacyEntriesAsync(dbContext);
            await NormalizeEmergencyMasterDataModuleMoveAsync(dbContext);
            await NormalizeLaboratorySpecimenLegacyActionsAsync(dbContext);
            await NormalizeSystemOnlyVisibilityAsync(dbContext);

            await dbContext.SaveChangesAsync();
        }

        private static SysApplicationModule EnsureModule(
            ApplicationDbContext dbContext,
            IDictionary<string, SysApplicationModule> modulesByCode,
            AccessControllerAttribute attribute)
        {
            modulesByCode.TryGetValue(attribute.ModuleCode, out var module);

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
            modulesByCode.Add(module.ModuleCode, module);

            return module;
        }

        private static SysControllerAccess EnsureController(
            ApplicationDbContext dbContext,
            IDictionary<string, SysControllerAccess> controllersByModuleAndName,
            Guid moduleId,
            ControllerActionDescriptor controllerAction,
            AccessControllerAttribute attribute)
        {
            var controllerName = string.IsNullOrWhiteSpace(attribute.ControllerName)
                ? controllerAction.ControllerName
                : attribute.ControllerName;

            var controllerRoutePath = BuildControllerRoutePath(controllerAction);

            var isSystemOnly = attribute.IsSystemOnly;

            var visibleInRoleAccess =
                !isSystemOnly &&
                attribute.VisibleInRoleAccess;

            controllersByModuleAndName.TryGetValue(
                BuildControllerKey(moduleId, controllerName),
                out var controller);

            if (controller != null)
            {
                controller.DisplayName = attribute.DisplayName;
                controller.RoutePath = controllerRoutePath;
                controller.Description = attribute.Description;
                controller.SortOrder = attribute.SortOrder;
                controller.VisibleInRoleAccess = visibleInRoleAccess;
                controller.IsSystemOnly = isSystemOnly;
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
                VisibleInRoleAccess = visibleInRoleAccess,
                IsSystemOnly = isSystemOnly,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                IsDelete = false,
                IsCancel = false
            };

            dbContext.SysControllerAccesses.Add(controller);
            controllersByModuleAndName.Add(
                BuildControllerKey(controller.ModuleId, controller.ControllerName),
                controller);

            return controller;
        }

        private static void EnsureAction(
            ApplicationDbContext dbContext,
            IDictionary<string, SysActionAccess> actionsByControllerAndName,
            Guid controllerAccessId,
            ControllerActionDescriptor controllerAction,
            AccessControllerAttribute controllerAttribute,
            AccessActionAttribute attribute)
        {
            var actionRoutePath = BuildActionRoutePath(controllerAction);
            var httpMethod = GetHttpMethod(controllerAction);

            var isSystemOnly =
                controllerAttribute.IsSystemOnly ||
                attribute.IsSystemOnly;

            var visibleInRoleAccess =
                !isSystemOnly &&
                controllerAttribute.VisibleInRoleAccess &&
                attribute.VisibleInRoleAccess;

            actionsByControllerAndName.TryGetValue(
                BuildActionKey(controllerAccessId, attribute.ActionName),
                out var action);

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
            actionsByControllerAndName.Add(
                BuildActionKey(action.ControllerAccessId, action.ActionName),
                action);
        }

        /// <summary>
        /// Mendaftarkan hak akses penanda supaya dapat dicentang pada layar Akses Role.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Gagal keras bila controllernya tidak ditemukan</b>, dan itu disengaja. Penanda yang
        /// gagal didaftarkan menghasilkan <c>403</c> permanen yang tidak dapat diperbaiki dari
        /// layar mana pun — persis bentuk kegagalan yang membuat method ini ada. Melewatinya
        /// diam-diam berarti mengulangi cacat yang sedang ditutup, kali ini dengan kode yang
        /// terlihat seperti sudah menanganinya.
        /// </para>
        /// <para>
        /// <c>HttpMethod</c> dan <c>RoutePath</c> dibiarkan kosong. Penanda ini memang tidak
        /// punya route, dan mengisinya dengan nilai karangan akan membuat layar Akses Role
        /// menampilkan alamat yang tidak dapat dipanggil siapa pun.
        /// </para>
        /// </remarks>
        private static void EnsurePenandaTanpaEndpoint(
            ApplicationDbContext dbContext,
            IDictionary<string, SysApplicationModule> modulesByCode,
            IDictionary<string, SysControllerAccess> controllersByModuleAndName,
            IDictionary<string, SysActionAccess> actionsByControllerAndName)
        {
            foreach (var penanda in PenandaTanpaEndpointYangDidaftarkan)
            {
                if (!modulesByCode.TryGetValue(penanda.ModuleCode, out var module))
                {
                    throw new InvalidOperationException(
                        $"Hak akses penanda '{penanda.ControllerName} : {penanda.ActionName}' " +
                        $"menunjuk module '{penanda.ModuleCode}' yang tidak terdaftar. Penanda " +
                        "yang gagal didaftarkan menghasilkan 403 permanen yang tidak dapat " +
                        "diperbaiki dari layar mana pun.");
                }

                var controllerKey = BuildControllerKey(module.Id, penanda.ControllerName);

                if (!controllersByModuleAndName.TryGetValue(controllerKey, out var controller))
                {
                    throw new InvalidOperationException(
                        $"Hak akses penanda '{penanda.ControllerName} : {penanda.ActionName}' " +
                        $"menunjuk controller '{penanda.ControllerName}' yang tidak ditemukan. " +
                        "Bila controllernya memang dihapus, hapus juga penandanya dari " +
                        "PenandaTanpaEndpointYangDidaftarkan.");
                }

                var actionKey = BuildActionKey(controller.Id, penanda.ActionName);

                if (actionsByControllerAndName.TryGetValue(actionKey, out var existing))
                {
                    existing.DisplayName = penanda.DisplayName;
                    existing.Description = penanda.Description;
                    existing.AccessType = penanda.AccessType;
                    existing.SortOrder = penanda.SortOrder;
                    existing.HttpMethod = null;
                    existing.RoutePath = null;
                    existing.VisibleInRoleAccess = true;
                    existing.IsSystemOnly = false;
                    existing.IsActive = true;
                    existing.IsDelete = false;

                    continue;
                }

                var action = new SysActionAccess
                {
                    Id = Guid.NewGuid(),
                    ControllerAccessId = controller.Id,
                    ActionName = penanda.ActionName,
                    DisplayName = penanda.DisplayName,
                    HttpMethod = null,
                    RoutePath = null,
                    Description = penanda.Description,
                    SortOrder = penanda.SortOrder,
                    AccessType = penanda.AccessType,
                    VisibleInRoleAccess = true,
                    IsSystemOnly = false,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    IsDelete = false,
                    IsCancel = false,
                };

                dbContext.SysActionAccesses.Add(action);
                actionsByControllerAndName.Add(actionKey, action);
            }
        }

        private static string BuildControllerKey(Guid moduleId, string controllerName) =>
            $"{moduleId:N}:{controllerName}";

        private static string BuildActionKey(Guid controllerAccessId, string actionName) =>
            $"{controllerAccessId:N}:{actionName}";


        private static async Task NormalizeEmployeeSelfServiceLegacyEntriesAsync(
            ApplicationDbContext dbContext)
        {
            var now = DateTime.UtcNow;

            var legacyControllers = await (
                from controller in dbContext.SysControllerAccesses
                join module in dbContext.SysApplicationModules
                    on controller.ModuleId equals module.Id
                where
                    (module.ModuleCode == "HUMAN_RESOURCE_LEAVE" &&
                     (controller.ControllerName == "LeaveRequestSelfService" ||
                      controller.ControllerName == "LeaveCancellationSelfService" ||
                      controller.ControllerName == "LeaveReturnToWorkSelfService")) ||
                    (module.ModuleCode == "HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE" &&
                     controller.ControllerName == "EmployeeProfileChange")
                select controller)
                .ToListAsync();

            foreach (var controller in legacyControllers)
            {
                controller.VisibleInRoleAccess = false;
                controller.IsActive = false;
                controller.IsDelete = true;
                controller.UpdateDateTime = now;

                var actions = await dbContext.SysActionAccesses
                    .Where(x => x.ControllerAccessId == controller.Id)
                    .ToListAsync();

                foreach (var action in actions)
                {
                    action.VisibleInRoleAccess = false;
                    action.IsActive = false;
                    action.IsDelete = true;
                    action.UpdateDateTime = now;
                }
            }

            var attendanceCorrectionController = await (
                from controller in dbContext.SysControllerAccesses
                join module in dbContext.SysApplicationModules
                    on controller.ModuleId equals module.Id
                where
                    module.ModuleCode == "HUMAN_RESOURCE_ATTENDANCE" &&
                    controller.ControllerName == "AttendanceCorrection"
                select controller)
                .FirstOrDefaultAsync();

            if (attendanceCorrectionController != null)
            {
                var personalActionNames = new[]
                {
                    "Create",
                    "Update",
                    "Delete",
                    "Submit",
                    "Cancel"
                };

                var staleActions = await dbContext.SysActionAccesses
                    .Where(x =>
                        x.ControllerAccessId == attendanceCorrectionController.Id &&
                        personalActionNames.Contains(x.ActionName))
                    .ToListAsync();

                foreach (var action in staleActions)
                {
                    action.VisibleInRoleAccess = false;
                    action.IsActive = false;
                    action.IsDelete = true;
                    action.UpdateDateTime = now;
                }
            }
        }

        /// <summary>
        /// Menutup dua aksi lama <c>LabSpecimen</c> yang sudah tidak diperiksa siapa pun.
        /// </summary>
        /// <remarks>
        /// Dahulu seluruh jalur ubah wadah didaftarkan dengan nama aksi <c>Create</c> dan
        /// <c>Update</c>, sementara pemeriksaannya memakai nama yang lebih spesifik —
        /// <c>Plan</c>, <c>Collect</c>, <c>Receive</c>, <c>Accept</c>, <c>Hold</c>, dan
        /// <c>Cancel</c>. Karena nama yang diperiksa tidak pernah terdaftar, seluruh daur hidup
        /// wadah mustahil diberikan kepada siapa pun: admin tidak dapat mencentang aksi yang
        /// tidak ada di katalog, dan <c>HasAccessAsync</c> menjawab tidak untuk aksi yang tidak
        /// ditemukan. Hanya SuperAdmin yang lolos, dan itu pun karena ia memang melewati
        /// pemeriksaan kebijakan.
        ///
        /// Setelah nama pendaftarannya diluruskan, kedua baris lama tetap tertinggal di
        /// database. Keduanya tidak berbahaya — tidak satu pun endpoint memeriksanya — tetapi
        /// <b>menyesatkan pada layar Role Access</b>: labelnya kembar persis dengan baris yang
        /// benar, sehingga admin melihat "Plan Lab Specimen" dan "Cancel Lab Specimen" masing-
        /// masing dua kali, dan yang satu tidak mengendalikan apa pun. Admin yang mencentang
        /// baris yang salah akan mengira sudah memberi kewenangan, padahal petugasnya tetap
        /// ditolak.
        ///
        /// Pembersihan ditaruh di seeder, bukan dikerjakan sekali lewat perintah database,
        /// supaya setiap lingkungan yang pernah menjalankan versi lama ikut rapi dengan
        /// sendirinya. Polanya menyalin penanganan <c>AttendanceCorrection</c> di atas.
        ///
        /// Baris hanya ditandai terhapus, tidak dihapus fisik, mengikuti soft delete yang
        /// dipakai seluruh tabel audit.
        /// </remarks>
        private static async Task NormalizeLaboratorySpecimenLegacyActionsAsync(
            ApplicationDbContext dbContext)
        {
            var now = DateTime.UtcNow;

            var specimenController = await dbContext.SysControllerAccesses
                .FirstOrDefaultAsync(x => x.ControllerName == "LabSpecimen");

            if (specimenController == null)
            {
                return;
            }

            // Kedua nama ini sengaja disebut satu per satu, bukan disimpulkan dari selisih
            // terhadap atribut yang ada. Menyimpulkan otomatis berarti setiap aksi yang
            // kebetulan belum terbaca refleksi ikut dipensiunkan diam-diam.
            var retiredActionNames = new[] { "Create", "Update" };

            var staleActions = await dbContext.SysActionAccesses
                .Where(x =>
                    x.ControllerAccessId == specimenController.Id &&
                    retiredActionNames.Contains(x.ActionName))
                .ToListAsync();

            foreach (var action in staleActions)
            {
                action.VisibleInRoleAccess = false;
                action.IsActive = false;
                action.IsDelete = true;
                action.UpdateDateTime = now;
            }
        }

        /// <summary>
        /// Menutup pendaftaran lama enam controller master IGD yang berpindah modul.
        /// </summary>
        /// <remarks>
        /// Master IGD sebelumnya terdaftar pada modul <c>HEALTH_SERVICE_MASTER_DATA</c> dan
        /// kini pindah ke <c>HEALTH_SERVICE_EMERGENCY_INSTALLATION_MANAGEMENT</c>.
        /// <para>
        /// <see cref="EnsureControllerAsync"/> mencari controller berdasarkan pasangan
        /// modul dan nama, sehingga perpindahan modul membuatnya membuat baris baru alih-alih
        /// memindahkan yang lama. Tanpa penutupan ini, layar Manajemen Role akan menampilkan
        /// keenam master dua kali: satu di bawah Master Data, satu di bawah IGD, dan petugas
        /// tidak punya cara membedakan mana yang menegakkan izin.
        /// </para>
        /// <para>
        /// Baris lama <b>ditutup</b>, bukan dihapus, mengikuti cara
        /// <see cref="NormalizeEmployeeSelfServiceLegacyEntriesAsync"/> menangani perpindahan
        /// serupa. Nol kebijakan pada <c>SysAccessPolicy</c> menunjuk keenamnya saat
        /// perpindahan dikerjakan, sehingga tidak ada izin yang hangus.
        /// </para>
        /// </remarks>
        private static async Task NormalizeEmergencyMasterDataModuleMoveAsync(
            ApplicationDbContext dbContext)
        {
            var now = DateTime.UtcNow;

            var movedControllerNames = new[]
            {
                "EmergencyTriageLevel",
                "EmergencyTriageIndicator",
                "EmergencyArrivalMode",
                "EmergencyCaseType",
                "EmergencyDispositionType",
                "EmergencySetting"
            };

            var legacyControllers = await (
                from controller in dbContext.SysControllerAccesses
                join module in dbContext.SysApplicationModules
                    on controller.ModuleId equals module.Id
                where
                    module.ModuleCode == "HEALTH_SERVICE_MASTER_DATA" &&
                    movedControllerNames.Contains(controller.ControllerName) &&
                    !controller.IsDelete
                select controller)
                .ToListAsync();

            foreach (var controller in legacyControllers)
            {
                controller.VisibleInRoleAccess = false;
                controller.IsActive = false;
                controller.IsDelete = true;
                controller.UpdateDateTime = now;

                var actions = await dbContext.SysActionAccesses
                    .Where(x => x.ControllerAccessId == controller.Id)
                    .ToListAsync();

                foreach (var action in actions)
                {
                    action.VisibleInRoleAccess = false;
                    action.IsActive = false;
                    action.IsDelete = true;
                    action.UpdateDateTime = now;
                }
            }
        }

        private static async Task NormalizeSystemOnlyVisibilityAsync(
            ApplicationDbContext dbContext)
        {
            var systemOnlyControllers = await dbContext.SysControllerAccesses
                .Where(x =>
                    x.IsSystemOnly &&
                    x.VisibleInRoleAccess)
                .ToListAsync();

            foreach (var controller in systemOnlyControllers)
            {
                controller.VisibleInRoleAccess = false;
            }

            var systemOnlyActions = await dbContext.SysActionAccesses
                .Where(x =>
                    x.IsSystemOnly &&
                    x.VisibleInRoleAccess)
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

        private static string BuildControllerRoutePath(
            ControllerActionDescriptor controllerAction)
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

        private static string BuildActionRoutePath(
            ControllerActionDescriptor controllerAction)
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

        private static string GetHttpMethod(
            ControllerActionDescriptor controllerAction)
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
