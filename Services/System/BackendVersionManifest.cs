using System.Text.Json;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackend.Services.System
{
    public sealed class BackendVersionManifest
    {
        private static readonly Regex SemanticVersionPattern = new(
            @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public string BackendVersion { get; init; } = string.Empty;

        public string ReleaseName { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public static BackendVersionManifest Load(string contentRootPath)
        {
            var path = Path.Combine(contentRootPath, "version.json");
            if (!File.Exists(path))
            {
                throw new InvalidOperationException(
                    $"Authoritative backend version manifest was not found at '{path}'.");
            }

            BackendVersionManifest? manifest;
            try
            {
                manifest = JsonSerializer.Deserialize<BackendVersionManifest>(
                    File.ReadAllText(path),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
            {
                throw new InvalidOperationException(
                    $"Authoritative backend version manifest at '{path}' could not be read or parsed.",
                    exception);
            }

            if (manifest == null)
            {
                throw new InvalidOperationException(
                    $"Authoritative backend version manifest at '{path}' is empty.");
            }

            manifest = new BackendVersionManifest
            {
                BackendVersion = manifest.BackendVersion?.Trim() ?? string.Empty,
                ReleaseName = manifest.ReleaseName?.Trim() ?? string.Empty,
                Description = manifest.Description?.Trim() ?? string.Empty
            };

            if (!IsValidSemanticVersion(manifest.BackendVersion))
            {
                throw new InvalidOperationException(
                    $"The backendVersion in '{path}' must be a Semantic Version in MAJOR.MINOR.PATCH format.");
            }

            if (string.IsNullOrWhiteSpace(manifest.ReleaseName))
            {
                throw new InvalidOperationException($"The releaseName in '{path}' is required.");
            }

            if (string.IsNullOrWhiteSpace(manifest.Description))
            {
                throw new InvalidOperationException($"The description in '{path}' is required.");
            }

            return manifest;
        }

        public static bool IsValidSemanticVersion(string? value) =>
            !string.IsNullOrWhiteSpace(value) && SemanticVersionPattern.IsMatch(value);
    }
}
