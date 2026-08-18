using Microsoft.AspNetCore.Http;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services
{
    public sealed class WfpCertificationFileStorageService
    {
        public const long MaximumFileSize = 10 * 1024 * 1024;

        private static readonly IReadOnlyDictionary<string, string> AllowedTypes =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [".pdf"] = "application/pdf",
                [".jpg"] = "image/jpeg",
                [".jpeg"] = "image/jpeg",
                [".png"] = "image/png",
                [".doc"] = "application/msword",
                [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                [".xls"] = "application/vnd.ms-excel",
                [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };

        private readonly string _storageRoot;

        public WfpCertificationFileStorageService(IWebHostEnvironment environment)
        {
            _storageRoot = Path.GetFullPath(Path.Combine(
                environment.ContentRootPath, "App_Data", "WfpCertifications"));
            Directory.CreateDirectory(_storageRoot);
        }

        public async Task<StoredCertificationFile> SaveAsync(
            IFormFile file, CancellationToken cancellationToken = default)
        {
            Validate(file, out var extension, out var contentType);
            if (!await HasExpectedSignatureAsync(file, extension, cancellationToken))
                throw new CertificationFileValidationException("Isi file tidak sesuai dengan ekstensi file.");
            var storedName = $"{Guid.NewGuid():N}{extension}";
            var temporaryPath = ResolvePath($".{Guid.NewGuid():N}.tmp");
            var finalPath = ResolvePath(storedName);

            try
            {
                await using (var input = file.OpenReadStream())
                await using (var output = new FileStream(
                    temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                    81920, FileOptions.Asynchronous | FileOptions.SequentialScan))
                {
                    await input.CopyToAsync(output, cancellationToken);
                }

                File.Move(temporaryPath, finalPath, false);
                return new StoredCertificationFile(storedName, contentType, file.Length);
            }
            catch
            {
                TryDelete(temporaryPath);
                throw;
            }
        }

        public string GetPhysicalPath(string storedFileName) => ResolvePath(storedFileName);

        public Task DeleteAsync(string? storedFileName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!string.IsNullOrWhiteSpace(storedFileName))
                TryDelete(ResolvePath(storedFileName));
            return Task.CompletedTask;
        }

        private static void Validate(IFormFile file, out string extension, out string contentType)
        {
            if (file == null || file.Length <= 0)
                throw new CertificationFileValidationException("File sertifikasi kosong.");
            if (file.Length > MaximumFileSize)
                throw new CertificationFileValidationException("Ukuran file sertifikasi maksimal 10 MB.");
            if (string.IsNullOrWhiteSpace(file.FileName) || file.FileName.IndexOf('\0') >= 0)
                throw new CertificationFileValidationException("Nama file sertifikasi tidak valid.");

            extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedTypes.TryGetValue(extension, out var canonicalContentType))
                throw new CertificationFileValidationException("Tipe file sertifikasi tidak diizinkan.");
            contentType = canonicalContentType;
            if (!string.IsNullOrWhiteSpace(file.ContentType) &&
                !string.Equals(file.ContentType, contentType, StringComparison.OrdinalIgnoreCase) &&
                !(extension is ".jpg" or ".jpeg" && file.ContentType.Equals("image/jpg", StringComparison.OrdinalIgnoreCase)))
                throw new CertificationFileValidationException("Tipe konten file sertifikasi tidak sesuai.");
        }

        private static async Task<bool> HasExpectedSignatureAsync(
            IFormFile file, string extension, CancellationToken cancellationToken)
        {
            var signature = extension switch
            {
                ".pdf" => new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D },
                ".jpg" or ".jpeg" => new byte[] { 0xFF, 0xD8, 0xFF },
                ".png" => new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
                ".doc" or ".xls" => new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 },
                ".docx" or ".xlsx" => new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                _ => Array.Empty<byte>()
            };

            await using var stream = file.OpenReadStream();
            var buffer = new byte[signature.Length];
            var read = 0;
            while (read < buffer.Length)
            {
                var count = await stream.ReadAsync(buffer.AsMemory(read), cancellationToken);
                if (count == 0) break;
                read += count;
            }
            return read == signature.Length && buffer.AsSpan().SequenceEqual(signature);
        }

        private string ResolvePath(string fileName)
        {
            var safeName = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(fileName) || !string.Equals(safeName, fileName, StringComparison.Ordinal))
                throw new InvalidOperationException("Storage path tidak valid.");
            var fullPath = Path.GetFullPath(Path.Combine(_storageRoot, safeName));
            var root = _storageRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Storage path tidak valid.");
            return fullPath;
        }

        private static void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    public sealed record StoredCertificationFile(string FilePath, string ContentType, long FileSize);

    public sealed class CertificationFileValidationException : Exception
    {
        public CertificationFileValidationException(string message) : base(message) { }
    }
}
