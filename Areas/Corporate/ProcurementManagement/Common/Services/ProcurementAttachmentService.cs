using System.IO.Compression;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services
{
    /// <summary>Isian atau berkas tidak memenuhi aturan (HTTP 422).</summary>
    public sealed class ProcurementAttachmentValidationException(string code, string message) : Exception(message)
    {
        public string Code { get; } = code;
    }

    /// <summary>Bentrok status, misalnya dokumen sudah final (HTTP 409).</summary>
    public sealed class ProcurementAttachmentConflictException(string code, string message) : Exception(message)
    {
        public string Code { get; } = code;
    }

    /// <summary>Lokasi berkas fisik untuk diunduh controller.</summary>
    public sealed record ProcurementAttachmentDownload(string PhysicalPath, string ContentType, string FileName);

    /// <summary>
    /// Lampiran dokumen Pengadaan (BE-PBJ-004, PBJ-DEC-023; <c>validation-matrix.md</c>
    /// PBJ-VAL-080..083).
    /// </summary>
    /// <remarks>
    /// <para><b>Aturan berkas.</b> Ekstensi harus ada di daftar izin <b>dan</b> isi berkas harus
    /// cocok dengan ekstensinya. Contoh: <c>penawaran.pdf</c> yang isinya HTML ditolak
    /// (PBJ-VAL-081), walaupun ekstensinya diizinkan. Ukuran bawaan maksimal 10 MB (PBJ-VAL-082).
    /// Daftar izin, ukuran, dan akar penyimpanan dapat diatur lewat <c>Procurement:Attachment:*</c>.</para>
    /// <para><b>Penyimpanan.</b> Berkas disimpan di luar <c>wwwroot</c> dengan nama acak, sehingga
    /// tidak bisa diambil langsung sebagai berkas statis.</para>
    /// <para><b>Hak akses.</b> Selain izin <c>ProcurementAttachment</c>, pengguna wajib punya hak atas
    /// dokumen induknya lewat <see cref="IProcurementDocumentAccessProvider"/>. Jenis dokumen tanpa
    /// penyedia dijawab 404 (fail-closed).</para>
    /// </remarks>
    public sealed class ProcurementAttachmentService
    {
        private const string LogCategory = "Corporate.ProcurementManagement.Common";
        private const long DefaultMaximumFileSizeBytes = 10L * 1024L * 1024L;
        private const string DefaultStorageRoot = "App_Data/procurement-attachments";

        private static readonly string[] DefaultAllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png", ".docx", ".xlsx"];

        private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "application/pdf",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly ProcurementDocumentHistoryWriter _historyWriter;
        private readonly IReadOnlyList<IProcurementDocumentAccessProvider> _accessProviders;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly LoggerService _loggerService;

        public ProcurementAttachmentService(
            ApplicationDbContext dbContext,
            ProcurementDocumentHistoryWriter historyWriter,
            IEnumerable<IProcurementDocumentAccessProvider> accessProviders,
            IWebHostEnvironment environment,
            IConfiguration configuration,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _historyWriter = historyWriter;
            _accessProviders = accessProviders.ToList();
            _environment = environment;
            _configuration = configuration;
            _loggerService = loggerService;
        }

        public async Task<List<ProcurementAttachmentResponse>> GetAsync(
            ClaimsPrincipal user, ProcurementAttachmentQuery query, CancellationToken cancellationToken)
        {
            var documentType = NormalizeDocumentType(query.DocumentType);
            await RequireAccessAsync(user, documentType, query.DocumentId, ProcurementDocumentAccessLevel.Read, cancellationToken);

            var rows = _dbContext.PrcAttachments.AsNoTracking()
                .Where(x => x.DocumentType == documentType && x.DocumentId == query.DocumentId && !x.IsDelete);
            if (!query.IncludeHistory)
            {
                rows = rows.Where(x => x.IsCurrent && !x.IsCancel);
            }

            return await rows
                .OrderBy(x => x.CreateDateTime).ThenBy(x => x.VersionNo)
                .Select(x => new ProcurementAttachmentResponse
                {
                    Id = x.Id,
                    DocumentType = x.DocumentType,
                    DocumentId = x.DocumentId,
                    AttachmentCategory = x.AttachmentCategory,
                    AttachmentKey = x.AttachmentKey,
                    VersionNo = x.VersionNo,
                    IsCurrent = x.IsCurrent,
                    IsWithdrawn = x.IsCancel,
                    OriginalFileName = x.OriginalFileName,
                    ContentType = x.ContentType,
                    FileSizeBytes = x.FileSizeBytes,
                    UploadedByUserId = x.UploadedByUserId,
                    UploadedByName = _dbContext.Users.Where(u => u.Id == x.UploadedByUserId).Select(u => u.DisplayName).FirstOrDefault(),
                    UploadedAt = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<ProcurementAttachmentResponse> UploadAsync(
            ClaimsPrincipal user, UploadProcurementAttachmentRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var documentType = NormalizeDocumentType(request.DocumentType);
            await RequireAccessAsync(user, documentType, request.DocumentId, ProcurementDocumentAccessLevel.Edit, cancellationToken);

            var file = request.File;
            if (file == null || file.Length == 0)
                throw new ProcurementAttachmentValidationException("PBJ-VAL-080", "Berkas wajib dipilih.");

            var originalFileName = Path.GetFileName(file.FileName ?? string.Empty).Trim();
            if (originalFileName.Length is 0 or > 255)
                throw new ProcurementAttachmentValidationException("PBJ-VAL-080", "Nama berkas tidak valid.");

            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            var allowed = ResolveAllowedExtensions();
            if (string.IsNullOrEmpty(extension) || !allowed.Contains(extension) || !ContentTypes.ContainsKey(extension))
                throw new ProcurementAttachmentValidationException(
                    "PBJ-VAL-080",
                    $"Jenis berkas tidak diizinkan. Gunakan {string.Join(", ", allowed.Select(x => x.TrimStart('.').ToUpperInvariant()))}.");

            var maximum = ResolveMaximumFileSizeBytes();
            if (file.Length > maximum)
                throw new ProcurementAttachmentValidationException(
                    "PBJ-VAL-082", $"Ukuran berkas maksimal {maximum / (1024 * 1024)} MB.");

            await using (var probe = file.OpenReadStream())
            {
                if (!MatchesSignature(probe, extension))
                    throw new ProcurementAttachmentValidationException("PBJ-VAL-081", "Isi berkas tidak sesuai dengan jenisnya.");
            }

            var category = string.IsNullOrWhiteSpace(request.AttachmentCategory) ? null : request.AttachmentCategory.Trim();
            if (category is { Length: > 50 })
                throw new ProcurementAttachmentValidationException("PBJ-VAL-080", "Kategori lampiran maksimal 50 karakter.");

            PrcAttachment? replaced = null;
            if (request.ReplacesAttachmentId.HasValue)
            {
                replaced = await _dbContext.PrcAttachments.FirstOrDefaultAsync(x =>
                        x.Id == request.ReplacesAttachmentId.Value &&
                        x.DocumentType == documentType &&
                        x.DocumentId == request.DocumentId &&
                        !x.IsDelete,
                    cancellationToken)
                    ?? throw new KeyNotFoundException("Lampiran yang akan diganti tidak ditemukan pada dokumen ini.");
                if (!replaced.IsCurrent || replaced.IsCancel)
                    throw new ProcurementAttachmentConflictException(
                        "PBJ-VAL-083", "Lampiran yang akan diganti bukan versi yang berlaku.");
            }

            var now = DateTime.UtcNow;
            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var relativePath = Path.Combine(
                now.Year.ToString("0000"),
                now.Month.ToString("00"),
                documentType,
                storedFileName).Replace('\\', '/');
            var physicalPath = ResolvePhysicalPath(relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);

            string sha256;
            var temporaryPath = physicalPath + ".tmp";
            try
            {
                await using (var input = file.OpenReadStream())
                await using (var output = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true))
                using (var hasher = SHA256.Create())
                {
                    var buffer = new byte[81920];
                    int read;
                    while ((read = await input.ReadAsync(buffer, cancellationToken)) > 0)
                    {
                        hasher.TransformBlock(buffer, 0, read, null, 0);
                        await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                    }

                    hasher.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                    sha256 = Convert.ToHexString(hasher.Hash!).ToLowerInvariant();
                }

                File.Move(temporaryPath, physicalPath);
            }
            catch
            {
                TryDelete(temporaryPath);
                throw;
            }

            var entity = new PrcAttachment
            {
                DocumentType = documentType,
                DocumentId = request.DocumentId,
                AttachmentCategory = category ?? replaced?.AttachmentCategory,
                AttachmentKey = replaced?.AttachmentKey ?? Guid.NewGuid(),
                VersionNo = 1,
                IsCurrent = true,
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName,
                RelativePath = relativePath,
                ContentType = ContentTypes[extension],
                FileSizeBytes = file.Length,
                Sha256 = sha256,
                UploadedByUserId = actorUserId,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            if (replaced != null)
            {
                entity.VersionNo = await _dbContext.PrcAttachments
                    .Where(x => x.AttachmentKey == replaced.AttachmentKey)
                    .MaxAsync(x => x.VersionNo, cancellationToken) + 1;
                replaced.IsCurrent = false;
                replaced.UpdateDateTime = now;
                replaced.UpdateBy = actorUserId;
            }

            _dbContext.PrcAttachments.Add(entity);
            _historyWriter.Record(
                documentType, request.DocumentId, null, ProcurementDocumentHistoryActions.Attach,
                null, null, null,
                new { AttachmentId = entity.Id, entity.AttachmentKey, entity.VersionNo },
                actorUserId, DateTimeOffset.UtcNow);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                // Basis data gagal: berkas fisik dibuang supaya tidak ada berkas yatim.
                TryDelete(physicalPath);
                throw;
            }

            await _loggerService.AuditAsync(LogCategory, "ProcurementAttachment.Upload", "Lampiran Pengadaan ditambahkan.", new
            {
                EntityId = entity.Id,
                entity.DocumentType,
                entity.DocumentId,
                entity.VersionNo,
                ActorUserId = actorUserId
            });

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<ProcurementAttachmentResponse> DeactivateAsync(
            ClaimsPrincipal user, Guid attachmentId, Guid actorUserId, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.PrcAttachments.FirstOrDefaultAsync(x => x.Id == attachmentId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Lampiran tidak ditemukan.");
            await RequireAccessAsync(user, entity.DocumentType, entity.DocumentId, ProcurementDocumentAccessLevel.Edit, cancellationToken);

            if (!entity.IsCurrent || entity.IsCancel)
                throw new ProcurementAttachmentConflictException("PBJ-VAL-083", "Lampiran ini sudah tidak berlaku.");

            var now = DateTime.UtcNow;
            entity.IsCurrent = false;
            entity.IsCancel = true;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            _historyWriter.Record(
                entity.DocumentType, entity.DocumentId, null, ProcurementDocumentHistoryActions.Detach,
                null, null, null,
                new { AttachmentId = entity.Id, entity.AttachmentKey, entity.VersionNo },
                actorUserId, DateTimeOffset.UtcNow);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(LogCategory, "ProcurementAttachment.Deactivate", "Lampiran Pengadaan ditarik.", new
            {
                EntityId = entity.Id,
                entity.DocumentType,
                entity.DocumentId,
                ActorUserId = actorUserId
            });

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<ProcurementAttachmentDownload> GetDownloadAsync(
            ClaimsPrincipal user, Guid attachmentId, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.PrcAttachments.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == attachmentId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Lampiran tidak ditemukan.");
            await RequireAccessAsync(user, entity.DocumentType, entity.DocumentId, ProcurementDocumentAccessLevel.Read, cancellationToken);

            var physicalPath = ResolvePhysicalPath(entity.RelativePath);
            if (!File.Exists(physicalPath))
                throw new KeyNotFoundException("Berkas lampiran tidak ditemukan di penyimpanan.");

            return new ProcurementAttachmentDownload(physicalPath, entity.ContentType, entity.OriginalFileName);
        }

        private async Task<ProcurementAttachmentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            await _dbContext.PrcAttachments.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new ProcurementAttachmentResponse
                {
                    Id = x.Id,
                    DocumentType = x.DocumentType,
                    DocumentId = x.DocumentId,
                    AttachmentCategory = x.AttachmentCategory,
                    AttachmentKey = x.AttachmentKey,
                    VersionNo = x.VersionNo,
                    IsCurrent = x.IsCurrent,
                    IsWithdrawn = x.IsCancel,
                    OriginalFileName = x.OriginalFileName,
                    ContentType = x.ContentType,
                    FileSizeBytes = x.FileSizeBytes,
                    UploadedByUserId = x.UploadedByUserId,
                    UploadedByName = _dbContext.Users.Where(u => u.Id == x.UploadedByUserId).Select(u => u.DisplayName).FirstOrDefault(),
                    UploadedAt = x.CreateDateTime
                })
                .FirstAsync(cancellationToken);

        /// <summary>
        /// Menilai hak atas dokumen induk. Tanpa penyedia, atau hak di bawah <c>Read</c> → 404
        /// (keberadaan dokumen tidak dibocorkan). Hak <c>Read</c> saat butuh <c>Edit</c> → 409
        /// PBJ-VAL-083 (dokumen sudah final atau bukan haknya untuk diubah).
        /// </summary>
        private async Task RequireAccessAsync(
            ClaimsPrincipal user, string documentType, Guid documentId, ProcurementDocumentAccessLevel required, CancellationToken cancellationToken)
        {
            var provider = _accessProviders.FirstOrDefault(x => string.Equals(x.DocumentType, documentType, StringComparison.Ordinal));
            var access = provider == null || documentId == Guid.Empty
                ? ProcurementDocumentAccessLevel.None
                : await provider.GetAccessAsync(user, documentId, cancellationToken);

            if (access == ProcurementDocumentAccessLevel.None)
                throw new KeyNotFoundException("Dokumen tidak ditemukan atau bukan milik unit Anda.");

            if (access < required)
                throw new ProcurementAttachmentConflictException("PBJ-VAL-083", "Dokumen sudah final; lampiran tidak dapat diubah.");
        }

        private static string NormalizeDocumentType(string? documentType)
        {
            var normalized = documentType?.Trim() ?? string.Empty;
            if (!ProcurementDocumentTypes.IsKnown(normalized))
                throw new ProcurementAttachmentValidationException("PBJ-VAL-080", "Jenis dokumen tidak dikenal.");
            return normalized;
        }

        /// <summary>
        /// Mencocokkan tanda tangan berkas dengan ekstensinya. DOCX/XLSX diperiksa sebagai arsip ZIP
        /// yang memuat <c>[Content_Types].xml</c> dan bagian utama dokumennya.
        /// </summary>
        private static bool MatchesSignature(Stream stream, string extension)
        {
            var header = new byte[8];
            var read = stream.Read(header, 0, header.Length);

            switch (extension)
            {
                case ".pdf":
                    return read >= 4 && header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46; // %PDF
                case ".png":
                    return read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
                        && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;
                case ".jpg":
                case ".jpeg":
                    return read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
                case ".docx":
                case ".xlsx":
                    if (read < 4 || header[0] != 0x50 || header[1] != 0x4B || header[2] != 0x03 || header[3] != 0x04)
                        return false;
                    if (!stream.CanSeek) return false;
                    stream.Seek(0, SeekOrigin.Begin);
                    try
                    {
                        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
                        var hasContentTypes = archive.GetEntry("[Content_Types].xml") != null;
                        var mainPart = extension == ".docx" ? "word/document.xml" : "xl/workbook.xml";
                        return hasContentTypes && archive.GetEntry(mainPart) != null;
                    }
                    catch (InvalidDataException)
                    {
                        return false;
                    }
                default:
                    return false;
            }
        }

        private HashSet<string> ResolveAllowedExtensions()
        {
            var configured = _configuration
                .GetSection("Procurement:Attachment:AllowedExtensions")
                .GetChildren()
                .Select(x => x.Value)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!.Trim().StartsWith('.') ? x.Trim().ToLowerInvariant() : "." + x.Trim().ToLowerInvariant())
                // Hanya ekstensi yang punya pemeriksa tanda tangan yang boleh diizinkan.
                .Where(ContentTypes.ContainsKey)
                .ToList();

            return new HashSet<string>(configured.Count > 0 ? configured : DefaultAllowedExtensions, StringComparer.OrdinalIgnoreCase);
        }

        private long ResolveMaximumFileSizeBytes() =>
            long.TryParse(_configuration["Procurement:Attachment:MaximumFileSizeBytes"], out var value) && value > 0
                ? value
                : DefaultMaximumFileSizeBytes;

        private string ResolvePhysicalPath(string relativePath)
        {
            var configuredRoot = _configuration["Procurement:Attachment:StorageRoot"];
            var root = Path.GetFullPath(Path.Combine(
                _environment.ContentRootPath,
                string.IsNullOrWhiteSpace(configuredRoot) ? DefaultStorageRoot : configuredRoot.Trim()));

            var full = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
            if (!full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Jalur lampiran berada di luar akar penyimpanan.");
            return full;
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (IOException)
            {
                // Dibiarkan: berkas yatim tidak membuka akses apa pun karena tidak tercatat.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
