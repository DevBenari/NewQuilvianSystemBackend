using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed class AssetAttachmentService
{
    private const string LogCategory = "Corporate.AssetManagement.Register.Attachment";
    private const string DefaultUploadRootPath = "uploads";
    private const string RelativeRootFolder = "asset-management/attachments";
    private const int MaxOwnerTypeLength = 40;
    private const int MaxFileNameLength = 255;

    private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".com", ".bat", ".cmd", ".ps1", ".sh", ".js", ".mjs",
        ".html", ".htm", ".php", ".asp", ".aspx", ".jsp", ".jar", ".msi", ".scr", ".svg"
    };

    private static readonly IReadOnlyDictionary<string, string> CanonicalMediaTypeByExtension =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "application/pdf",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };

    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly AssetAttachmentOptions _options;
    private readonly LoggerService _loggerService;

    public AssetAttachmentService(
        ApplicationDbContext context,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        IOptions<AssetAttachmentOptions> options,
        LoggerService loggerService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
    }

    public async Task<AstAttachment> SaveAsync(
        IFormFile? file,
        string ownerType,
        Guid ownerId,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ownerType) || ownerType.Length > MaxOwnerTypeLength)
        {
            throw new ArgumentException("Jenis pemilik lampiran wajib diisi, paling panjang 40 karakter.", nameof(ownerType));
        }

        if (ownerId == Guid.Empty)
        {
            throw new ArgumentException("Id pemilik lampiran wajib diisi.", nameof(ownerId));
        }

        if (!_options.IsConfigured)
        {
            throw AssetManagementException.Unavailable(
                AssetRegisterErrorCodes.AttachmentNotConfigured,
                AssetRegisterMessages.AttachmentNotConfigured);
        }

        if (file == null || file.Length <= 0)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.AttachmentEmpty, AssetRegisterMessages.AttachmentEmpty);
        }

        var originalFileName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(originalFileName) || originalFileName.Length > MaxFileNameLength)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.AttachmentType, AssetRegisterMessages.AttachmentType);
        }

        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var mediaType = ResolveMediaType(extension, file.ContentType);

        if (file.Length > _options.MaxFileSizeBytes!.Value)
        {
            throw AssetManagementException.TooLarge(AssetRegisterErrorCodes.AttachmentTooLarge, AssetRegisterMessages.AttachmentTooLarge);
        }

        var now = DateTime.UtcNow;
        var relativePath = $"{RelativeRootFolder}/{now.Year:0000}/{now.Month:00}/{Guid.NewGuid():N}{extension}";
        var physicalPath = ResolvePhysicalPath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);

        string sha256;
        try
        {
            sha256 = await CopyWithHashAsync(file, physicalPath, cancellationToken);
        }
        catch
        {
            DeleteIfExists(physicalPath);
            throw;
        }

        var attachment = new AstAttachment
        {
            OwnerType = ownerType,
            OwnerId = ownerId,
            OriginalFileName = originalFileName,
            ContentType = mediaType,
            SizeBytes = file.Length,
            Sha256 = sha256,
            StoredPath = relativePath,
            CreateBy = actorUserId,
            CreateDateTime = now
        };

        try
        {
            _context.AstAttachments.Add(attachment);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            _context.Entry(attachment).State = EntityState.Detached;
            DeleteIfExists(physicalPath);
            throw;
        }

        await _loggerService.AuditAsync(
            LogCategory,
            "AssetAttachment.Upload",
            $"Lampiran aset berhasil diunggah. AttachmentId={attachment.Id}",
            new
            {
                AttachmentId = attachment.Id,
                attachment.OwnerType,
                attachment.OwnerId,
                attachment.ContentType,
                attachment.SizeBytes,
                UploadedBy = actorUserId
            });

        return attachment;
    }

    public async Task<AstAttachment> GetForOwnerAsync(
        Guid attachmentId,
        string ownerType,
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        var attachment = await _context.AstAttachments
            .AsNoTracking()
            .SingleOrDefaultAsync(x =>
                x.Id == attachmentId &&
                x.OwnerType == ownerType &&
                x.OwnerId == ownerId &&
                !x.IsDelete,
                cancellationToken);

        return attachment ?? throw AssetManagementException.NotFound(
            AssetRegisterErrorCodes.AttachmentNotFound,
            AssetRegisterMessages.AttachmentNotFound);
    }

    public async Task<AssetAttachmentContent> OpenReadAsync(
        Guid attachmentId,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var attachment = await _context.AstAttachments
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == attachmentId && !x.IsDelete, cancellationToken)
            ?? throw AssetManagementException.NotFound(AssetRegisterErrorCodes.AttachmentNotFound, AssetRegisterMessages.AttachmentNotFound);

        string physicalPath;
        try
        {
            physicalPath = ResolvePhysicalPath(attachment.StoredPath);
        }
        catch (InvalidOperationException)
        {
            throw AssetManagementException.NotFound(AssetRegisterErrorCodes.AttachmentNotFound, AssetRegisterMessages.AttachmentNotFound);
        }

        if (!File.Exists(physicalPath))
        {
            throw AssetManagementException.NotFound(AssetRegisterErrorCodes.AttachmentNotFound, AssetRegisterMessages.AttachmentNotFound);
        }

        await _loggerService.AuditAsync(
            LogCategory,
            "AssetAttachment.Download",
            $"Lampiran aset diunduh. AttachmentId={attachment.Id}",
            new { AttachmentId = attachment.Id, attachment.OwnerType, attachment.OwnerId, DownloadedBy = actorUserId });

        var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return new AssetAttachmentContent(attachment.Id, attachment.OwnerType, attachment.OwnerId, stream, attachment.ContentType, attachment.OriginalFileName);
    }

    private string ResolveMediaType(string extension, string? contentType)
    {
        var allowed = _options.AllowedExtensions
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().StartsWith('.') ? x.Trim() : "." + x.Trim());

        if (string.IsNullOrWhiteSpace(extension) ||
            BlockedExtensions.Contains(extension) ||
            !allowed.Contains(extension, StringComparer.OrdinalIgnoreCase) ||
            !CanonicalMediaTypeByExtension.TryGetValue(extension, out var expectedMediaType) ||
            !string.Equals(contentType?.Trim(), expectedMediaType, StringComparison.OrdinalIgnoreCase))
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.AttachmentType, AssetRegisterMessages.AttachmentType);
        }

        return expectedMediaType;
    }

    private static async Task<string> CopyWithHashAsync(IFormFile file, string physicalPath, CancellationToken cancellationToken)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        await using var output = new FileStream(
            physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        await using var input = file.OpenReadStream();

        var buffer = new byte[81920];
        int read;
        while ((read = await input.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
        {
            hash.AppendData(buffer, 0, read);
            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private string ResolvePhysicalPath(string relativePath)
    {
        var root = ResolveUploadRootPath();
        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        var candidate = Path.GetFullPath(Path.Combine(root, normalized.Replace('/', Path.DirectorySeparatorChar)));
        var requiredPrefix = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;

        if (!candidate.StartsWith(requiredPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Jalur berkas berada di luar akar penyimpanan.");
        }

        return candidate;
    }

    private string ResolveUploadRootPath()
    {
        var configured = _configuration["FileStorage:UploadRootPath"];
        var path = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(_environment.ContentRootPath, DefaultUploadRootPath)
            : configured.Trim();

        if (!Path.IsPathRooted(path))
        {
            path = Path.Combine(_environment.ContentRootPath, path);
        }

        return Path.GetFullPath(path);
    }
}

public sealed record AssetAttachmentContent(
    Guid AttachmentId,
    string OwnerType,
    Guid OwnerId,
    Stream Stream,
    string ContentType,
    string FileName);
