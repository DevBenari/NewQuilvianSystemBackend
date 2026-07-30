using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using System.Security.Cryptography;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public class WorkflowFileStorageService
    {
        private const long DefaultMaximumFileSizeBytes = 20L * 1024L * 1024L;
        private const string DefaultRelativeRootPath = "uploads/human-resource/workflow";

        private static readonly HashSet<string> DefaultAllowedExtensions = new(
            new[]
            {
                ".pdf",
                ".jpg",
                ".jpeg",
                ".png",
                ".webp",
                ".doc",
                ".docx",
                ".xls",
                ".xlsx",
                ".csv",
                ".txt",
                ".zip"
            },
            StringComparer.OrdinalIgnoreCase);

        private static readonly HashSet<string> BlockedExtensions = new(
            new[]
            {
                ".exe",
                ".dll",
                ".com",
                ".bat",
                ".cmd",
                ".ps1",
                ".sh",
                ".js",
                ".mjs",
                ".html",
                ".htm",
                ".php",
                ".asp",
                ".aspx",
                ".jsp",
                ".jar",
                ".msi",
                ".scr"
            },
            StringComparer.OrdinalIgnoreCase);

        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public WorkflowFileStorageService(
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        public long MaximumFileSizeBytes
        {
            get
            {
                var rawValue = _configuration[
                    "HumanResource:WorkflowAttachment:MaximumFileSizeBytes"];

                return long.TryParse(rawValue, out var configuredValue) && configuredValue > 0
                    ? configuredValue
                    : DefaultMaximumFileSizeBytes;
            }
        }

        public IReadOnlyCollection<string> AllowedExtensions =>
            ResolveAllowedExtensions()
                .OrderBy(x => x)
                .ToList();

        public async Task<WorkflowServiceResult<WorkflowAttachmentStoredFileResponse>> SaveAsync(
            Guid workflowInstanceId,
            IFormFile file,
            CancellationToken cancellationToken = default)
        {
            var validationMessage = ValidateFile(file);
            if (validationMessage != null)
            {
                return WorkflowServiceResult<WorkflowAttachmentStoredFileResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    validationMessage);
            }

            var originalFileName = Path.GetFileName(file.FileName).Trim();
            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var now = DateTime.UtcNow;

            var relativeDirectory = Path.Combine(
                ResolveRelativeRootPath(),
                now.Year.ToString("0000"),
                now.Month.ToString("00"),
                workflowInstanceId.ToString("N"));

            var physicalDirectory = ResolvePhysicalPath(relativeDirectory);
            Directory.CreateDirectory(physicalDirectory);

            var relativePath = Path.Combine(relativeDirectory, storedFileName);
            var physicalPath = ResolvePhysicalPath(relativePath);
            var temporaryPath = physicalPath + ".tmp";

            try
            {
                using var sha256 = SHA256.Create();
                await using var inputStream = file.OpenReadStream();
                await using var outputStream = new FileStream(
                    temporaryPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);

                var buffer = new byte[81920];
                long totalWritten = 0;

                while (true)
                {
                    var bytesRead = await inputStream.ReadAsync(
                        buffer.AsMemory(0, buffer.Length),
                        cancellationToken);

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    totalWritten += bytesRead;
                    if (totalWritten > MaximumFileSizeBytes)
                    {
                        throw new InvalidDataException(
                            "Ukuran file melebihi batas maksimum yang diizinkan.");
                    }

                    sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
                    await outputStream.WriteAsync(
                        buffer.AsMemory(0, bytesRead),
                        cancellationToken);
                }

                sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                await outputStream.FlushAsync(cancellationToken);

                File.Move(temporaryPath, physicalPath, false);

                var result = new WorkflowAttachmentStoredFileResponse
                {
                    RelativePath = NormalizeRelativePath(relativePath),
                    Checksum = Convert.ToHexString(sha256.Hash ?? Array.Empty<byte>())
                        .ToLowerInvariant(),
                    FileSizeBytes = totalWritten,
                    ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                        ? "application/octet-stream"
                        : file.ContentType.Trim()
                };

                return WorkflowServiceResult<WorkflowAttachmentStoredFileResponse>.Ok(
                    result,
                    "File workflow berhasil disimpan.");
            }
            catch (InvalidDataException ex)
            {
                DeleteIfExists(temporaryPath);
                DeleteIfExists(physicalPath);

                return WorkflowServiceResult<WorkflowAttachmentStoredFileResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message);
            }
            catch
            {
                DeleteIfExists(temporaryPath);
                DeleteIfExists(physicalPath);
                throw;
            }
        }

        public WorkflowServiceResult<string> ResolveDownloadPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return WorkflowServiceResult<string>.Fail(
                    StatusCodes.Status404NotFound,
                    "Lokasi file workflow tidak tersedia.");
            }

            try
            {
                var physicalPath = ResolvePhysicalPath(relativePath);
                if (!File.Exists(physicalPath))
                {
                    return WorkflowServiceResult<string>.Fail(
                        StatusCodes.Status404NotFound,
                        "File workflow tidak ditemukan pada penyimpanan.");
                }

                return WorkflowServiceResult<string>.Ok(
                    physicalPath,
                    "Lokasi file workflow berhasil ditemukan.");
            }
            catch (InvalidOperationException)
            {
                return WorkflowServiceResult<string>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Lokasi file workflow tidak valid.");
            }
        }

        public Task DeletePhysicalFileAsync(
            string? relativePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return Task.CompletedTask;
            }

            try
            {
                var physicalPath = ResolvePhysicalPath(relativePath);
                DeleteIfExists(physicalPath);
            }
            catch (InvalidOperationException)
            {
                // Metadata sudah di-soft-delete. Path yang tidak valid tidak boleh
                // digunakan untuk menghapus file di luar root penyimpanan workflow.
            }

            return Task.CompletedTask;
        }

        private string? ValidateFile(IFormFile? file)
        {
            if (file == null)
            {
                return "File wajib dilampirkan.";
            }

            if (file.Length <= 0)
            {
                return "File tidak boleh kosong.";
            }

            if (file.Length > MaximumFileSizeBytes)
            {
                return $"Ukuran file melebihi batas maksimum {MaximumFileSizeBytes} byte.";
            }

            var safeFileName = Path.GetFileName(file.FileName);
            if (string.IsNullOrWhiteSpace(safeFileName) || safeFileName.Length > 255)
            {
                return "Nama file tidak valid atau melebihi 255 karakter.";
            }

            var extension = Path.GetExtension(safeFileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                return "File wajib memiliki ekstensi.";
            }

            if (BlockedExtensions.Contains(extension))
            {
                return $"Ekstensi file {extension} tidak diizinkan.";
            }

            if (!ResolveAllowedExtensions().Contains(extension))
            {
                return $"Ekstensi file {extension} tidak termasuk daftar yang diizinkan.";
            }

            return null;
        }

        private HashSet<string> ResolveAllowedExtensions()
        {
            var configuredExtensions = _configuration
                .GetSection("HumanResource:WorkflowAttachment:AllowedExtensions")
                .GetChildren()
                .Select(x => x.Value)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(NormalizeExtension)
                .Where(x => !BlockedExtensions.Contains(x))
                .ToList();

            return configuredExtensions.Count > 0
                ? new HashSet<string>(configuredExtensions, StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(DefaultAllowedExtensions, StringComparer.OrdinalIgnoreCase);
        }

        private string ResolveRelativeRootPath()
        {
            var configuredPath = _configuration[
                "HumanResource:WorkflowAttachment:RelativeRootPath"];

            var path = string.IsNullOrWhiteSpace(configuredPath)
                ? DefaultRelativeRootPath
                : configuredPath.Trim();

            path = path
                .Replace('\\', Path.DirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar)
                .TrimStart(Path.DirectorySeparatorChar);

            return path;
        }

        private string ResolveStorageRoot()
        {
            var webRootPath = string.IsNullOrWhiteSpace(_environment.WebRootPath)
                ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                : _environment.WebRootPath;

            return Path.GetFullPath(webRootPath);
        }

        private string ResolvePhysicalPath(string relativePath)
        {
            var storageRoot = ResolveStorageRoot();
            var normalizedRelativePath = relativePath
                .Replace('\\', Path.DirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar)
                .TrimStart(Path.DirectorySeparatorChar);

            var physicalPath = Path.GetFullPath(
                Path.Combine(storageRoot, normalizedRelativePath));

            var requiredPrefix = storageRoot.EndsWith(Path.DirectorySeparatorChar)
                ? storageRoot
                : storageRoot + Path.DirectorySeparatorChar;

            if (!physicalPath.StartsWith(requiredPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Path file berada di luar root penyimpanan workflow.");
            }

            return physicalPath;
        }

        private static string NormalizeExtension(string? extension)
        {
            var normalized = extension?.Trim().ToLowerInvariant() ?? string.Empty;
            return normalized.StartsWith('.') ? normalized : "." + normalized;
        }

        private static string NormalizeRelativePath(string path)
        {
            return path.Replace(Path.DirectorySeparatorChar, '/');
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
