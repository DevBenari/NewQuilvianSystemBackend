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
    /// <item><b>Endpoint naked</b> — dapat dijangkau pengguna terautentikasi tetapi tidak membawa
    /// <c>[AccessPermission]</c>, <c>[AccessAction]</c>, <c>[AllowAnonymous]</c>, maupun policy
    /// bernama yang disetujui. Ini kelas kesalahan yang paling sulit terlihat: endpoint semacam
    /// itu tidak terdaftar di registry, tidak terhitung pada metrik mana pun, dan karena itu tidak
    /// pernah muncul pada audit — sementara siapa pun yang punya login dapat memanggilnya.
    /// Ditemukan pada <c>BE-SEC-012</c>, ketika 20 endpoint tulis HR ternyata hanya dilindungi
    /// <c>[Authorize]</c> sementara audit drift melaporkan nol metadata gap.</item>
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

            /// <summary>
            /// Endpoint bisnis yang dapat dijangkau pengguna terautentikasi tanpa penegakan apa pun.
            /// Ini kegagalan keras.
            /// </summary>
            public List<string> NakedBusinessEndpoints { get; } = new();

            /// <summary>Utang warisan yang sudah diakui baseline. Dilaporkan, tidak menggagalkan.</summary>
            public List<string> AcknowledgedNakedEndpoints { get; } = new();

            /// <summary>Entri baseline yang endpoint-nya sudah tidak ada. Kegagalan keras.</summary>
            public List<string> StaleNakedBaselineEntries { get; } = new();

            public int TotalDeclaredKeys { get; init; }
            public int TotalActions { get; init; }

            public bool IsValid =>
                UnregisterableEndpoints.Count == 0 &&
                DuplicateResourceIdentities.Count == 0 &&
                InvalidAccessTypes.Count == 0 &&
                NakedBusinessEndpoints.Count == 0 &&
                StaleNakedBaselineEntries.Count == 0;
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

            foreach (var naked in snapshot.NakedEndpoints)
            {
                result.NakedBusinessEndpoints.Add(
                    $"{naked.BaselineKey} [{naked.HttpMethod ?? "?"}] pada modul '{naked.ModuleCode}' " +
                    "dapat dijangkau siapa pun yang punya login: tidak ada [AccessPermission], " +
                    "tidak ada [AllowAnonymous], dan tidak ada policy bernama yang disetujui. " +
                    "Pasang [AccessAction] + [AccessPermission], atau pakai policy yang terdaftar " +
                    "pada AuthorizationPolicies.ApprovedAlternativeAuthorization.");
            }

            foreach (var acknowledged in snapshot.AcknowledgedNakedEndpoints)
            {
                result.AcknowledgedNakedEndpoints.Add(
                    $"{acknowledged.BaselineKey} [{acknowledged.HttpMethod ?? "?"}] pada modul " +
                    $"'{acknowledged.ModuleCode}'");
            }

            foreach (var stale in snapshot.StaleNakedBaselineEntries)
            {
                result.StaleNakedBaselineEntries.Add(
                    $"'{stale}' tercantum pada KnownUnenforcedBusinessEndpoints tetapi endpoint-nya " +
                    "sudah tidak ditemukan. Hapus barisnya bila utangnya memang sudah ditutup.");
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

            if (result.AcknowledgedNakedEndpoints.Count > 0)
            {
                _logger.LogWarning(
                    "{Count} endpoint bisnis masih hanya dilindungi [Authorize] dan sudah tercatat " +
                    "pada baseline warisan. Ini utang otorisasi yang menunggu keputusan pemilik modul, " +
                    "bukan keadaan normal: {Endpoints}",
                    result.AcknowledgedNakedEndpoints.Count,
                    string.Join("; ", result.AcknowledgedNakedEndpoints));
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
                         .Concat(result.InvalidAccessTypes)
                         .Concat(result.NakedBusinessEndpoints)
                         .Concat(result.StaleNakedBaselineEntries))
            {
                _logger.LogCritical("Permission registry bermasalah: {Detail}", detail);
            }

            var message =
                $"Permission registry tidak konsisten: {result.UnregisterableEndpoints.Count} endpoint tanpa " +
                $"metadata Akses Role, {result.DuplicateResourceIdentities.Count} resource ganda, " +
                $"{result.InvalidAccessTypes.Count} AccessType tidak sah, " +
                $"{result.NakedBusinessEndpoints.Count} endpoint bisnis tanpa penegakan otorisasi, " +
                $"{result.StaleNakedBaselineEntries.Count} entri baseline naked yang sudah basi.";

            if (throwOnFailure)
            {
                throw new InvalidOperationException(message);
            }

            _logger.LogCritical("{Message}", message);

            return result;
        }
    }
}
