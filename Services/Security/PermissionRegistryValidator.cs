using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace QuilvianSystemBackend.Services.Security
{
    /// <summary>
    /// Menjaga agar kelas kesalahan yang ditemukan pada audit Phase A0 tidak terulang.
    ///
    /// Sejak A0 identitas permission diambil langsung dari <c>[AccessPermission]</c>, sehingga
    /// kunci yang didaftarkan seeder dan kunci yang dicari runtime tidak lagi bisa berbeda. Yang
    /// masih perlu dijaga adalah tiga hal yang tetap mungkin salah:
    ///
    /// <list type="number">
    /// <item>Endpoint terproteksi tanpa <c>[AccessAction]</c> — kemampuannya tidak punya metadata,
    /// sehingga tidak muncul di layar Akses Role dan tidak dapat diberikan admin. Akibatnya sama
    /// dengan 89 endpoint yang ditemukan audit: 403 permanen.</item>
    /// <item>Satu resource permission terdaftar pada lebih dari satu modul, sehingga pencarian
    /// registry menjadi ambigu.</item>
    /// <item><c>AccessType</c> di luar Read/Create/Update/Delete, yang membuat kemampuannya
    /// tersaring keluar dari layar Akses Role.</item>
    /// </list>
    ///
    /// Di Development dan CI kegagalan menghentikan startup supaya ketahuan sebelum rilis. Di
    /// Production ia hanya mencatat <c>Critical</c>: rumah sakit tidak boleh gagal boot karena satu
    /// anotasi yang salah.
    /// </summary>
    public sealed class PermissionRegistryValidator
    {
        private readonly IActionDescriptorCollectionProvider _actionDescriptorProvider;
        private readonly ILogger<PermissionRegistryValidator> _logger;

        public PermissionRegistryValidator(
            IActionDescriptorCollectionProvider actionDescriptorProvider,
            ILogger<PermissionRegistryValidator> logger)
        {
            _actionDescriptorProvider = actionDescriptorProvider;
            _logger = logger;
        }

        public sealed class ValidationResult
        {
            /// <summary>Endpoint terproteksi yang kemampuannya tidak akan muncul di Akses Role.</summary>
            public List<string> UnregisterableEndpoints { get; } = new();

            public List<string> DuplicateResourceIdentities { get; } = new();
            public List<string> InvalidAccessTypes { get; } = new();

            /// <summary>Informasi saja: endpoint ber-[AccessAction] yang tidak ditegakkan permission.</summary>
            public List<string> UnenforcedEndpoints { get; } = new();

            public int TotalDeclaredKeys { get; init; }
            public int TotalActions { get; init; }

            public bool IsValid =>
                UnregisterableEndpoints.Count == 0 &&
                DuplicateResourceIdentities.Count == 0 &&
                InvalidAccessTypes.Count == 0;
        }

        public ValidationResult Validate() =>
            Validate(PermissionRegistryDescriptor.Build(_actionDescriptorProvider));

        public static ValidationResult Validate(PermissionRegistryDescriptor.RegistrySnapshot snapshot)
        {
            var result = new ValidationResult
            {
                TotalDeclaredKeys = snapshot.DeclaredKeys.Count,
                TotalActions = snapshot.Actions.Count
            };

            foreach (var gap in snapshot.MetadataGaps)
            {
                result.UnregisterableEndpoints.Add(
                    $"{gap.DeclaringController}.{gap.MethodName} memakai " +
                    $"[AccessPermission(\"{gap.ResourceName}\", \"{gap.ActionName}\")] tanpa [AccessAction]. " +
                    "Kemampuannya tidak akan muncul di layar Akses Role, sehingga tidak dapat diberikan " +
                    "kepada siapa pun dan endpoint menolak semua pengguna non-SuperAdmin.");
            }

            foreach (var duplicate in snapshot.Resources
                         .GroupBy(x => x.ResourceName, StringComparer.Ordinal)
                         .Where(g => g.Select(x => x.ModuleCode).Distinct(StringComparer.Ordinal).Count() > 1))
            {
                result.DuplicateResourceIdentities.Add(
                    $"Resource '{duplicate.Key}' terdaftar pada lebih dari satu modul: " +
                    string.Join(", ", duplicate.Select(x => x.ModuleCode).Distinct(StringComparer.Ordinal)));
            }

            foreach (var action in snapshot.Actions
                         .Where(x => !Constants.AccessTypes.AllowedForRoleAccess.Contains(x.AccessType)))
            {
                result.InvalidAccessTypes.Add(
                    $"{action.ResourceName}.{action.ActionName} memakai AccessType '{action.AccessType}' " +
                    "yang tidak ditampilkan layar Akses Role.");
            }

            foreach (var unenforced in snapshot.UnenforcedActions)
            {
                result.UnenforcedEndpoints.Add($"{unenforced.DeclaringController}.{unenforced.MethodName}");
            }

            return result;
        }

        public ValidationResult ValidateAndReport(bool throwOnFailure)
        {
            var result = Validate();

            if (result.UnenforcedEndpoints.Count > 0)
            {
                _logger.LogInformation(
                    "{Count} endpoint memakai [AccessAction] tanpa [AccessPermission]. " +
                    "Endpoint ini tidak ditegakkan matriks Akses Role dan mengandalkan policy lain.",
                    result.UnenforcedEndpoints.Count);
            }

            if (result.IsValid)
            {
                _logger.LogInformation(
                    "Permission registry valid. {KeyCount} identitas kanonik dari {ActionCount} kemampuan terdaftar.",
                    result.TotalDeclaredKeys,
                    result.TotalActions);

                return result;
            }

            foreach (var detail in result.UnregisterableEndpoints
                         .Concat(result.DuplicateResourceIdentities)
                         .Concat(result.InvalidAccessTypes))
            {
                _logger.LogCritical("Permission registry bermasalah: {Detail}", detail);
            }

            var message =
                $"Permission registry tidak konsisten: {result.UnregisterableEndpoints.Count} endpoint tanpa " +
                $"metadata Akses Role, {result.DuplicateResourceIdentities.Count} resource ganda, " +
                $"{result.InvalidAccessTypes.Count} AccessType tidak sah.";

            if (throwOnFailure)
            {
                throw new InvalidOperationException(message);
            }

            _logger.LogCritical("{Message}", message);

            return result;
        }
    }
}
