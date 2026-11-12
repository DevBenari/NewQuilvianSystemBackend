using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class HrdLifecycleAttachmentService : IHrdLifecycleAttachmentService
    {
        private const long MaxFileSizeBytes = 10L * 1024L * 1024L; // 10 MB (VR-ONB-09)
        private readonly IWebHostEnvironment _environment;

        private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
        private static readonly string[] AllowedContentTypes = { "application/pdf", "image/jpeg", "image/png" };

        public HrdLifecycleAttachmentService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<(bool IsSuccess, string? ErrorMessage, string? StoragePath, string? FileName, string? ContentType, long? FileSize)> ValidateAndSaveAttachmentAsync(
            IFormFile file,
            string subFolder,
            CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
            {
                return (false, "Berkas tidak boleh kosong (VR-ONB-09).", null, null, null, null);
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return (false, "Ukuran berkas melebihi batas maksimum 10 MB (VR-ONB-09).", null, null, null, null);
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (Array.IndexOf(AllowedExtensions, ext) < 0)
            {
                return (false, "Format berkas tidak diizinkan. Hanya PDF, JPG, atau PNG yang diterima (VR-ONB-09).", null, null, null, null);
            }

            var rootPath = _environment.ContentRootPath ?? AppContext.BaseDirectory;
            var relativeDir = Path.Combine("uploads", "human-resource", "lifecycle", subFolder);
            var absoluteDir = Path.Combine(rootPath, relativeDir);

            if (!Directory.Exists(absoluteDir))
            {
                Directory.CreateDirectory(absoluteDir);
            }

            var storedFileName = $"{Guid.NewGuid():N}{ext}";
            var absoluteFilePath = Path.Combine(absoluteDir, storedFileName);
            var relativeFilePath = Path.Combine(relativeDir, storedFileName).Replace('\\', '/');

            await using (var fileStream = new FileStream(absoluteFilePath, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(fileStream, cancellationToken);
            }

            var contentType = !string.IsNullOrWhiteSpace(file.ContentType) ? file.ContentType : "application/octet-stream";
            return (true, null, relativeFilePath, file.FileName, contentType, file.Length);
        }

        public Task<(bool Exists, Stream? Stream, string? ContentType, string? FileName)> GetAttachmentStreamAsync(
            string storagePath,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(storagePath))
            {
                return Task.FromResult((false, (Stream?)null, (string?)null, (string?)null));
            }

            var rootPath = _environment.ContentRootPath ?? AppContext.BaseDirectory;
            var absoluteFilePath = Path.Combine(rootPath, storagePath.Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(absoluteFilePath))
            {
                return Task.FromResult((false, (Stream?)null, (string?)null, (string?)null));
            }

            var ext = Path.GetExtension(absoluteFilePath).ToLowerInvariant();
            var contentType = ext switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            Stream stream = new FileStream(absoluteFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult((true, (Stream?)stream, (string?)contentType, (string?)Path.GetFileName(absoluteFilePath)));
        }
    }
}
