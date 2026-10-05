using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Cryptography;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Services;

/// <summary>
/// Unggah dan pembacaan metadata bukti pembayaran langsung (BE-FIN-075, FIN-DES-087, FIN-DES-092,
/// FIN-DEC-139). Menulis metadata ke <see cref="FinTransactionProof"/> (BE-FIN-074) dan berkas fisik
/// di bawah <c>FileStorage:UploadRootPath</c> — konfigurasi dan middleware `UseStaticFiles` yang sudah
/// berjalan (<c>Program.cs</c>). TIDAK memanggil <c>WorkflowFileStorageService</c> milik HR (batas
/// modul MUST NOT dilintasi, FIN-DES-087) — pola path-safety-nya ditulis ulang di sini, mengikuti
/// teknik yang sama: <c>Path.GetFullPath</c> + pemeriksaan prefix akar
/// (lihat juga <c>WfpDocumentController.ResolvePhysicalPath</c>).
///
/// Berkas TIDAK diekspos lewat `UseStaticFiles` publik — satu-satunya jalur unduh adalah
/// <c>DownloadAsync</c> di bawah ini, yang dipanggil controller BERGERBANG
/// `FinanceTransactionProof : Read` (FIN-DES-092 M.3, permission-audit-matrix.md H.3).
///
/// DELAPAN PEMERIKSAAN BERURUT (FIN-DES-092 M.3, FIN-VAL-214..221) — yang pertama gagal
/// menghentikan sisanya:
///   1. Berkas ada dan tidak kosong                                    -> 400 (FIN-VAL-214)
///   2. Nama berkas maksimal 255 karakter                               -> 400 (FIN-VAL-215)
///   3. Ekstensi wajib ada                                              -> 400 (FIN-VAL-216)
///   4. Ekstensi tidak ada pada daftar terlarang (dipakai ulang HR)     -> 400 (FIN-VAL-216)
///   5. Ekstensi ada pada FinanceManagement:TransactionProof:AllowedExtensions -> 400 (FIN-VAL-217)
///   6. Tipe media cocok dengan ekstensinya                             -> 400 (FIN-VAL-218)
///   7a. MaxFileSizeBytes sudah dikonfigurasi (null = fail-closed)      -> 503 (FIN-VAL-220)
///   7b. Ukuran tidak melewati MaxFileSizeBytes                         -> 413 (FIN-VAL-219)
///   8. Jalur simpan hasil penormalan berada di bawah UploadRootPath    -> 400 (FIN-VAL-221, dicatat)
/// </summary>
public sealed class FinanceTransactionProofService
{
    private const string LogCategory = "Corporate.FinanceManagement.Collection.TransactionProof";
    private const string DefaultUploadRootPath = "uploads";
    private const string RelativeRootFolder = "finance/transaction-proofs";

    // Pemeriksaan #4 (FIN-DES-092): "Tertanam, dipakai ulang dari preseden" — daftar yang sama
    // persis dengan WorkflowFileStorageService.BlockedExtensions, ditulis ulang di sini karena
    // Finance MUST NOT memanggil kelas unggah milik HR (FIN-DES-087).
    private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".com", ".bat", ".cmd", ".ps1", ".sh", ".js", ".mjs",
        ".html", ".htm", ".php", ".asp", ".aspx", ".jsp", ".jar", ".msi", ".scr"
    };

    // Pemeriksaan #6 (FIN-DES-092): pemetaan ekstensi -> tipe media kanonikal (FIN-DEC-139,
    // erd/data-dictionary.md R14.7). SENGAJA tidak dibuat konfigurabel (BE-FIN-074 bagian 2) —
    // daftarnya sudah diputuskan; konfigurasi hanya mengendalikan ekstensi mana yang DIIZINKAN,
    // bukan tipe media yang dipasangkan dengannya. Ekstensi yang lolos AllowedExtensions tetapi
    // tidak ada pada peta ini TIDAK PERNAH dianggap cocok (fail-closed), karena belum ada tipe
    // media kanonikal yang disetujui untuknya.
    private static readonly IReadOnlyDictionary<string, string> CanonicalMediaTypeByExtension =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "application/pdf",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png"
        };

    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly FinanceTransactionProofOptions _options;
    private readonly LoggerService _loggerService;

    public FinanceTransactionProofService(
        ApplicationDbContext dbContext,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        IOptions<FinanceTransactionProofOptions> options,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _environment = environment;
        _configuration = configuration;
        _options = options.Value;
        _loggerService = loggerService;
    }

    public async Task<FinTransactionProof> UploadAsync(
        IFormFile? file, string proofType, Guid actorUserId, CancellationToken cancellationToken)
    {
        // 1. Berkas ada dan tidak kosong (FIN-VAL-214).
        if (file == null || file.Length <= 0)
        {
            throw new FinanceTransactionProofValidationException(
                "Berkas bukti wajib dilampirkan dan tidak boleh kosong.");
        }

        if (string.IsNullOrWhiteSpace(proofType))
        {
            throw new FinanceTransactionProofValidationException("Jenis bukti wajib diisi.");
        }

        // 2. Nama berkas maksimal 255 karakter (FIN-VAL-215).
        var originalFileName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(originalFileName) || originalFileName.Length > 255)
        {
            throw new FinanceTransactionProofValidationException(
                "Nama berkas terlalu panjang. Maksimal 255 karakter.");
        }

        // 3 & 4. Ekstensi wajib ada, dan tidak ada pada daftar terlarang (FIN-VAL-216).
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || BlockedExtensions.Contains(extension))
        {
            throw new FinanceTransactionProofValidationException(
                "Jenis berkas ini tidak dapat diunggah.");
        }

        // 5. Ekstensi ada pada FinanceManagement:TransactionProof:AllowedExtensions (FIN-VAL-217).
        if (!_options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new FinanceTransactionProofValidationException(
                "Bukti hanya dapat berupa PDF, JPG, JPEG, atau PNG.");
        }

        // 6. Tipe media cocok dengan ekstensinya — bukan hanya ekstensinya (FIN-VAL-218).
        var contentType = file.ContentType?.Trim() ?? string.Empty;
        if (!CanonicalMediaTypeByExtension.TryGetValue(extension, out var expectedMediaType) ||
            !string.Equals(contentType, expectedMediaType, StringComparison.OrdinalIgnoreCase))
        {
            throw new FinanceTransactionProofValidationException(
                "Isi berkas tidak sesuai dengan jenis berkasnya. Unggah ulang berkas aslinya.");
        }

        // 7a. MaxFileSizeBytes MUST sudah dikonfigurasi — fail-closed, bukan tak terbatas (FIN-VAL-220).
        if (_options.MaxFileSizeBytes is not { } maxFileSizeBytes)
        {
            throw new FinanceTransactionProofNotConfiguredException(
                "Unggah bukti belum dapat dipakai: batas ukuran berkas belum dikonfigurasi. Hubungi administrator.");
        }

        // 7b. Ukuran tidak melewati batas (FIN-VAL-219).
        if (file.Length > maxFileSizeBytes)
        {
            throw new FinanceTransactionProofTooLargeException(
                "Ukuran berkas melewati batas yang diizinkan.");
        }

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var now = DateTimeOffset.UtcNow;
        var relativeDirectory = $"{RelativeRootFolder}/{now.Year:0000}/{now.Month:00}";
        var relativePath = $"{relativeDirectory}/{storedFileName}";

        // 8. Jalur simpan hasil penormalan MUST berada di bawah akar penyimpanan (FIN-VAL-221).
        string physicalPath;
        try
        {
            physicalPath = ResolvePhysicalPath(relativePath);
        }
        catch (InvalidOperationException)
        {
            await _loggerService.WarningAsync(
                LogCategory,
                "FinanceTransactionProof.PathTraversalAttempt",
                "Percobaan jalur keluar akar penyimpanan bukti pembayaran ditolak.",
                new { ActorUserId = actorUserId, RequestedRelativePath = relativePath });

            throw new FinanceTransactionProofValidationException("Nama berkas tidak dapat diterima.");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);

        await using (var outputStream = new FileStream(
            physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            81920, FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            await using var inputStream = file.OpenReadStream();
            await inputStream.CopyToAsync(outputStream, cancellationToken);
        }

        var entity = new FinTransactionProof
        {
            ProofType = proofType,
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            RelativePath = relativePath,
            MediaType = expectedMediaType,
            SizeBytes = file.Length,
            UploadedBy = actorUserId,
            UploadedAt = now,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };

        try
        {
            _dbContext.Set<FinTransactionProof>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // FIN-DES-092: berkas yatim MUST NOT tertinggal bila penulisan metadata gagal — satu-
            // satunya penghapusan berkas yang diizinkan di luar jalur manual (bukan pembersihan
            // berkala otomatis yang dilarang FIN-DEC-139).
            DeleteIfExists(physicalPath);
            throw;
        }

        await _loggerService.AuditAsync(
            LogCategory,
            "FinanceTransactionProof.Upload",
            $"Bukti pembayaran berhasil diunggah. ProofId={entity.Id}",
            new
            {
                ProofId = entity.Id,
                entity.ProofType,
                entity.SizeBytes,
                entity.MediaType,
                UploadedBy = actorUserId,
                entity.UploadedAt,
                // H.2: OriginalFileName MUST NOT dicatat utuh bila memuat nama orang — dipotong.
                OriginalFileNameTruncated = Truncate(originalFileName, 40)
            });

        return entity;
    }

    public async Task<TransactionProofResponse> GetMetadataAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Set<FinTransactionProof>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Bukti tidak ditemukan.");

        return Map(entity);
    }

    public async Task<(Stream Stream, string MediaType, string FileName)> DownloadAsync(
        Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Set<FinTransactionProof>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Bukti tidak ditemukan.");

        string physicalPath;
        try
        {
            physicalPath = ResolvePhysicalPath(entity.RelativePath);
        }
        catch (InvalidOperationException)
        {
            throw new KeyNotFoundException("Berkas bukti tidak ditemukan pada penyimpanan.");
        }

        if (!File.Exists(physicalPath))
        {
            throw new KeyNotFoundException("Berkas bukti tidak ditemukan pada penyimpanan.");
        }

        // H.2: setiap unduhan tercatat beserta pengunduhnya. Isi berkas MUST NOT dicatat.
        await _loggerService.AuditAsync(
            LogCategory,
            "FinanceTransactionProof.Download",
            $"Berkas bukti pembayaran diunduh. ProofId={entity.Id}",
            new { ProofId = entity.Id, DownloadedBy = actorUserId });

        var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return (stream, entity.MediaType, entity.OriginalFileName);
    }

    private static TransactionProofResponse Map(FinTransactionProof x) => new()
    {
        Id = x.Id,
        ProofType = x.ProofType,
        OriginalFileName = x.OriginalFileName,
        MediaType = x.MediaType,
        SizeBytes = x.SizeBytes,
        UploadedBy = x.UploadedBy,
        UploadedAt = x.UploadedAt
    };

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    // Pola path-safety yang sama dengan WorkflowFileStorageService.ResolvePhysicalPath /
    // WfpDocumentController.ResolvePhysicalPath — ditulis ulang di sini (FIN-DES-087: Finance
    // MUST NOT memanggil kelas unggah milik HR), bukan disalin lewat pemanggilan lintas modul.
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
