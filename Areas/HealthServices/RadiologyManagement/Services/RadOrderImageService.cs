using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services
{
    /// <summary>
    /// Unggah Gambar radiologi — Form Hasil v1 (<c>BE-RAD-22</c>, <c>RAD-EVD-001</c> bagian 7.1).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Pola <c>ReferralDocumentStorageService</c>: berkas ditulis ke <c>FileStorage:PrivateRootPath</c>
    /// yang wajib berada di luar folder publik, jenisnya diperiksa dari isi (JPEG <c>FF D8 FF</c>,
    /// PNG <c>89 50 4E 47…</c>) bukan nama, dan berkas fisik dihapus lagi bila metadata gagal tersimpan.
    /// </para>
    /// <para>
    /// v1 menerima beberapa gambar sekaligus. Batas: <see cref="MaxActiveImages"/> gambar aktif per
    /// pesanan, <c>FileStorage:MaxRadiologyImageSizeMb</c> (bawaan 10 MB) per berkas. Setiap unggah dan
    /// hapus tercatat di <c>RadTransitionHistory</c> beserta pelakunya.
    /// </para>
    /// </remarks>
    public class RadOrderImageService
    {
        public const int MaxActiveImages = 20;
        private const int DefaultMaxSizeMb = 10;
        private const string FolderName = "radiology-images";
        private const string LogCategory = "RadiologyManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public RadOrderImageService(
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        public sealed record UploadFile(string FileName, long Length, Func<Stream> OpenReadStream);

        public sealed record ImageContent(Stream Stream, string ContentType, string FileName);

        public async Task<RadOperationResult<List<RadOrderImageResponse>>> GetListAsync(
            Guid radOrderId,
            CancellationToken cancellationToken = default)
        {
            if (!await _dbContext.RadOrders.AnyAsync(x => x.Id == radOrderId && !x.IsDelete, cancellationToken))
            {
                return RadOperationResult<List<RadOrderImageResponse>>.NotFound(
                    RadErrorCodes.OrderNotFound, "Pesanan radiologi tidak ditemukan.");
            }

            return RadOperationResult<List<RadOrderImageResponse>>.Success(
                await BacaDaftarAsync(radOrderId, cancellationToken));
        }

        public async Task<RadOperationResult<List<RadOrderImageResponse>>> UploadAsync(
            Guid radOrderId,
            Guid? radStudyId,
            IReadOnlyList<UploadFile> files,
            CancellationToken cancellationToken = default)
        {
            if (files.Count == 0)
            {
                return RadOperationResult<List<RadOrderImageResponse>>.Validation(
                    RadErrorCodes.ValidationFailed, "Pilih minimal satu gambar radiologi.");
            }

            var order = await _dbContext.RadOrders
                .FirstOrDefaultAsync(x => x.Id == radOrderId && !x.IsDelete, cancellationToken);

            if (order == null)
            {
                return RadOperationResult<List<RadOrderImageResponse>>.NotFound(
                    RadErrorCodes.OrderNotFound, "Pesanan radiologi tidak ditemukan.");
            }

            if (order.IsCancel || order.OrderStatus is RadOrderStatus.Cancelled or RadOrderStatus.Rejected)
            {
                return RadOperationResult<List<RadOrderImageResponse>>.Conflict(
                    RadErrorCodes.ImageNotChangeable,
                    "Pesanan ini dibatalkan atau ditolak, sehingga gambarnya tidak dapat diubah.");
            }

            if (radStudyId is Guid studyId &&
                !await _dbContext.RadStudies.AnyAsync(s => s.Id == studyId && s.RadOrderId == radOrderId && !s.IsDelete, cancellationToken))
            {
                return RadOperationResult<List<RadOrderImageResponse>>.Validation(
                    RadErrorCodes.StudyNotFound, "Study yang dipilih bukan milik pesanan ini.");
            }

            var aktif = await _dbContext.RadOrderImages
                .CountAsync(x => x.RadOrderId == radOrderId && !x.IsDelete, cancellationToken);
            var maxMb = _configuration.GetValue<int?>("FileStorage:MaxRadiologyImageSizeMb") ?? DefaultMaxSizeMb;
            var maxBytes = maxMb * 1024L * 1024L;

            if (aktif + files.Count > MaxActiveImages || files.Any(f => f.Length <= 0 || f.Length > maxBytes))
            {
                return RadOperationResult<List<RadOrderImageResponse>>.Validation(
                    RadErrorCodes.ImageInvalid,
                    $"Ukuran gambar maksimal {maxMb} MB, paling banyak {MaxActiveImages} gambar per pesanan.");
            }

            var rootError = TryResolvePrivateRoot(out var privateRoot);
            if (rootError != null)
            {
                return RadOperationResult<List<RadOrderImageResponse>>.Validation(RadErrorCodes.ImageInvalid, rootError);
            }

            var folder = Path.Combine(privateRoot, FolderName, radOrderId.ToString("N"));
            Directory.CreateDirectory(folder);

            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;
            var written = new List<string>();
            var nextOrder = await _dbContext.RadOrderImages
                .Where(x => x.RadOrderId == radOrderId)
                .Select(x => (int?)x.SortOrder)
                .MaxAsync(cancellationToken) ?? 0;

            try
            {
                foreach (var file in files)
                {
                    await using var input = file.OpenReadStream();
                    using var buffer = new MemoryStream();
                    await input.CopyToAsync(buffer, cancellationToken);

                    var bytes = buffer.ToArray();
                    var detected = DetectImageType(bytes);

                    if (detected == null)
                    {
                        DeleteFiles(written);
                        return RadOperationResult<List<RadOrderImageResponse>>.Validation(
                            RadErrorCodes.ImageInvalid, "Format gambar harus JPG atau PNG.");
                    }

                    var imageId = Guid.NewGuid();
                    var fileName = $"{imageId:N}{detected.Value.Extension}";
                    var physical = Path.Combine(folder, fileName);

                    await File.WriteAllBytesAsync(physical, bytes, cancellationToken);
                    written.Add(physical);

                    _dbContext.RadOrderImages.Add(new RadOrderImage
                    {
                        Id = imageId,
                        RadOrderId = radOrderId,
                        RadStudyId = radStudyId,
                        OriginalFileName = SanitizeFileName(file.FileName, detected.Value.Extension),
                        ContentType = detected.Value.ContentType,
                        SizeBytes = bytes.LongLength,
                        StoragePath = Path.Combine(FolderName, radOrderId.ToString("N"), fileName).Replace('\\', '/'),
                        SortOrder = ++nextOrder,
                        CreateDateTime = now,
                        CreateBy = actorUserId,
                    });
                }

                AddHistory(order, "Order.ImageAdded", $"{files.Count} gambar diunggah.", actorUserId, now);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                // Metadata gagal tersimpan: berkas fisik yang sudah ditulis tidak boleh tertinggal yatim.
                DeleteFiles(written);
                throw;
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "Order.ImageAdded",
                $"{files.Count} gambar radiologi diunggah ke pesanan {order.OrderNumber}.",
                new { OrderId = radOrderId, ActorUserId = actorUserId });

            return RadOperationResult<List<RadOrderImageResponse>>.Success(
                await BacaDaftarAsync(radOrderId, cancellationToken));
        }

        public async Task<RadOperationResult<ImageContent>> OpenAsync(
            Guid radOrderId,
            Guid imageId,
            CancellationToken cancellationToken = default)
        {
            var image = await _dbContext.RadOrderImages
                .AsNoTracking()
                .Where(x => x.Id == imageId && x.RadOrderId == radOrderId && !x.IsDelete)
                .Select(x => new { x.StoragePath, x.ContentType, x.OriginalFileName })
                .FirstOrDefaultAsync(cancellationToken);

            if (image == null)
            {
                return RadOperationResult<ImageContent>.NotFound(RadErrorCodes.ImageNotFound, "Gambar radiologi tidak ditemukan.");
            }

            var rootError = TryResolvePrivateRoot(out var privateRoot);
            if (rootError != null)
            {
                return RadOperationResult<ImageContent>.Validation(RadErrorCodes.ImageInvalid, rootError);
            }

            var physical = Path.GetFullPath(Path.Combine(privateRoot, image.StoragePath));
            if (!physical.StartsWith(privateRoot, StringComparison.OrdinalIgnoreCase) || !File.Exists(physical))
            {
                return RadOperationResult<ImageContent>.NotFound(
                    RadErrorCodes.ImageNotFound, "Berkas gambar tidak ditemukan di penyimpanan.");
            }

            return RadOperationResult<ImageContent>.Success(
                new ImageContent(File.OpenRead(physical), image.ContentType, image.OriginalFileName));
        }

        /// <summary>Hapus lunak — berkas fisik dibiarkan supaya jejak tetap dapat diperiksa.</summary>
        public async Task<RadOperationResult<List<RadOrderImageResponse>>> DeleteAsync(
            Guid radOrderId,
            Guid imageId,
            CancellationToken cancellationToken = default)
        {
            var image = await _dbContext.RadOrderImages
                .Include(x => x.RadOrder)
                .FirstOrDefaultAsync(x => x.Id == imageId && x.RadOrderId == radOrderId && !x.IsDelete, cancellationToken);

            if (image?.RadOrder == null)
            {
                return RadOperationResult<List<RadOrderImageResponse>>.NotFound(
                    RadErrorCodes.ImageNotFound, "Gambar radiologi tidak ditemukan.");
            }

            var order = image.RadOrder;
            if (order.IsCancel || order.OrderStatus is RadOrderStatus.Cancelled or RadOrderStatus.Rejected)
            {
                return RadOperationResult<List<RadOrderImageResponse>>.Conflict(
                    RadErrorCodes.ImageNotChangeable,
                    "Pesanan ini dibatalkan atau ditolak, sehingga gambarnya tidak dapat diubah.");
            }

            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            image.IsDelete = true;
            image.DeleteDateTime = now;
            image.DeleteBy = actorUserId;

            AddHistory(order, "Order.ImageRemoved", $"Gambar {image.OriginalFileName} dihapus.", actorUserId, now);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "Order.ImageRemoved",
                $"Gambar {image.OriginalFileName} pada pesanan {order.OrderNumber} dihapus.",
                new { OrderId = radOrderId, ActorUserId = actorUserId });

            return RadOperationResult<List<RadOrderImageResponse>>.Success(
                await BacaDaftarAsync(radOrderId, cancellationToken));
        }

        private async Task<List<RadOrderImageResponse>> BacaDaftarAsync(Guid radOrderId, CancellationToken cancellationToken)
        {
            var gambar = await _dbContext.RadOrderImages
                .AsNoTracking()
                .Where(x => x.RadOrderId == radOrderId && !x.IsDelete)
                .OrderBy(x => x.SortOrder)
                .Select(x => new RadOrderImageResponse
                {
                    Id = x.Id,
                    RadOrderId = x.RadOrderId,
                    RadStudyId = x.RadStudyId,
                    OriginalFileName = x.OriginalFileName,
                    ContentType = x.ContentType,
                    SizeBytes = x.SizeBytes,
                    SortOrder = x.SortOrder,
                    UploadedAt = x.CreateDateTime,
                    UploadedByUserId = x.CreateBy,
                })
                .ToListAsync(cancellationToken);

            var userIds = gambar.Select(x => x.UploadedByUserId).Where(x => x != Guid.Empty).Distinct().ToList();
            if (userIds.Count > 0)
            {
                var nama = await _dbContext.Users
                    .AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new { u.Id, Nama = u.DisplayName ?? u.UserName ?? u.UserCode })
                    .ToDictionaryAsync(u => u.Id, u => u.Nama, cancellationToken);

                foreach (var g in gambar) g.UploadedByName = nama.GetValueOrDefault(g.UploadedByUserId);
            }

            return gambar;
        }

        private void AddHistory(RadOrder order, string action, string note, Guid actorUserId, DateTime now)
        {
            var status = order.OrderStatus.ToString();
            _dbContext.RadTransitionHistories.Add(new RadTransitionHistory
            {
                RadOrderId = order.Id,
                EncounterId = order.EncounterId,
                Scope = RadTransitionScope.RadOrder,
                Action = action,
                FromStatus = status,
                ToStatus = status,
                ReasonNote = note,
                ActorUserId = actorUserId,
                OccurredAt = now,
                CreateBy = actorUserId,
                CreateDateTime = now,
            });
        }

        private Guid GetCurrentUserId()
        {
            var raw = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
        }

        /// <summary>Root privat absolut. Ditolak bila kosong atau berada di dalam folder publik.</summary>
        private string? TryResolvePrivateRoot(out string privateRoot)
        {
            privateRoot = string.Empty;
            var configured = _configuration["FileStorage:PrivateRootPath"];

            if (string.IsNullOrWhiteSpace(configured))
                return "Penyimpanan gambar radiologi belum dikonfigurasi (FileStorage:PrivateRootPath). Hubungi admin.";

            privateRoot = AbsoluteWithSeparator(configured);

            var publicCandidates = new List<string>
            {
                AbsoluteWithSeparator(string.IsNullOrWhiteSpace(_environment.WebRootPath)
                    ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                    : _environment.WebRootPath)
            };

            var publicRoot = _configuration["FileStorage:UploadRootPath"];
            if (!string.IsNullOrWhiteSpace(publicRoot)) publicCandidates.Add(AbsoluteWithSeparator(publicRoot));

            var root = privateRoot;
            return publicCandidates.Any(p => root.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                ? "Penyimpanan gambar radiologi salah konfigurasi: folder privat berada di dalam folder publik. Hubungi admin."
                : null;
        }

        private string AbsoluteWithSeparator(string path)
        {
            var full = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(_environment.ContentRootPath, path));
            return full.EndsWith(Path.DirectorySeparatorChar) ? full : full + Path.DirectorySeparatorChar;
        }

        internal static (string ContentType, string Extension)? DetectImageType(byte[] bytes)
        {
            if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                return ("image/jpeg", ".jpg");

            if (bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
                bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A)
                return ("image/png", ".png");

            return null;
        }

        private static string SanitizeFileName(string? fileName, string extension)
        {
            var name = Path.GetFileName(fileName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name)) name = "gambar-radiologi" + extension;

            var invalid = Path.GetInvalidFileNameChars();
            name = new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
            return name.Length > 255 ? name[^255..] : name;
        }

        private static void DeleteFiles(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                try
                {
                    if (File.Exists(path)) File.Delete(path);
                }
                catch
                {
                    // Pembersihan terbaik-usaha; kegagalan di sini tidak boleh menutupi galat utama.
                }
            }
        }
    }
}
