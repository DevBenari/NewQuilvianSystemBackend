using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    /// <summary>
    /// Berkas dokumen identitas pasien (KTP, paspor, dst.) di penyimpanan privat
    /// (RJ-REG-REV-BE-004, RJ-REG-DEC-008).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Privat.</b> Berkas ditulis ke <c>FileStorage:PrivateRootPath</c> (di luar folder
    /// publik <c>/uploads</c>) dan hanya dapat dibaca lewat endpoint berizin. Kolom
    /// <c>FilePath</c> menyimpan kunci berawalan <see cref="PrivatePathPrefix"/>, bukan URL.
    /// Nama berkas pengguna tidak pernah menjadi lokasi penyimpanan.
    /// </para>
    /// <para>
    /// <b>Format diperiksa dari isi</b> (magic bytes): JPEG, PNG, atau PDF; ukuran maksimal
    /// <c>FileStorage:MaxIdentityDocumentSizeMb</c> (bawaan 5 MB).
    /// </para>
    /// <para>
    /// <b>Tidak menimpa diam-diam.</b> Bila pasien sudah punya dokumen aktif yang memuat berkas,
    /// unggahan baru ditolak <c>409</c> kecuali pemanggil menunjuk dokumen yang diganti dan
    /// memegang <c>PatientIdentityDocument : Update</c>. Dokumen lama dinonaktifkan dan dihapus
    /// lunak (baris dan berkas fisiknya tetap ada), tidak ditimpa.
    /// </para>
    /// </remarks>
    public class PatientIdentityDocumentFileService
    {
        public const string PrivatePathPrefix = "private:";
        private const string FolderName = "identity-documents";
        private const int DefaultMaxSizeMb = 5;
        private const string LogCategory = "HealthServices.PatientManagement.MasterData";
        private const string ResourceName = "PatientIdentityDocument";

        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly AccessPermissionService _accessPermissionService;
        private readonly LoggerService _loggerService;

        public PatientIdentityDocumentFileService(
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            AccessPermissionService accessPermissionService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _environment = environment;
            _accessPermissionService = accessPermissionService;
            _loggerService = loggerService;
        }

        public enum ResultStatus
        {
            Success,
            Invalid,
            NotFound,
            Conflict,
            Forbidden
        }

        public sealed record Result(ResultStatus Status, string Message, Guid? DocumentId = null, Guid? ExistingDocumentId = null);

        public sealed record UploadRequest(
            Guid PatientId,
            string? IdentityType,
            string? IdentityNumber,
            Guid? ReplaceDocumentId,
            string? FileName,
            long Length,
            Func<Stream> OpenReadStream);

        public sealed record DocumentContent(Stream Stream, string ContentType, string FileName);

        public static bool IsPrivatePath(string? filePath) =>
            !string.IsNullOrWhiteSpace(filePath) &&
            filePath.StartsWith(PrivatePathPrefix, StringComparison.Ordinal);

        public async Task<Result> UploadAsync(
            ClaimsPrincipal user,
            UploadRequest request,
            IReadOnlyCollection<string> allowedIdentityTypes,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var identityType = request.IdentityType?.Trim();
            var identityNumber = request.IdentityNumber?.Trim();

            if (request.PatientId == Guid.Empty)
                return new Result(ResultStatus.Invalid, "Pasien wajib dipilih.");

            if (string.IsNullOrWhiteSpace(identityType) ||
                !allowedIdentityTypes.Contains(identityType, StringComparer.OrdinalIgnoreCase))
                return new Result(ResultStatus.Invalid, "Jenis identitas tidak valid.");

            identityType = allowedIdentityTypes.First(x => string.Equals(x, identityType, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(identityNumber) || identityNumber.Length > 100)
                return new Result(ResultStatus.Invalid, "Nomor identitas wajib diisi, maksimal 100 karakter.");

            var maxBytes = (_configuration.GetValue<int?>("FileStorage:MaxIdentityDocumentSizeMb") ?? DefaultMaxSizeMb) * 1024L * 1024L;

            if (request.Length <= 0 || request.Length > maxBytes)
                return new Result(ResultStatus.Invalid, $"Ukuran berkas identitas harus lebih dari 0 dan maksimal {maxBytes / (1024 * 1024)} MB.");

            var patientExists = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.PatientId && !x.IsDelete, cancellationToken);

            if (!patientExists)
                return new Result(ResultStatus.NotFound, "Pasien tidak ditemukan.");

            if (request.ReplaceDocumentId.HasValue &&
                !await _accessPermissionService.HasAccessAsync(user, ResourceName, "Update"))
                return new Result(ResultStatus.Forbidden, "Anda tidak memiliki izin mengganti dokumen identitas.");

            byte[] bytes;

            await using (var input = request.OpenReadStream())
            using (var buffer = new MemoryStream())
            {
                await input.CopyToAsync(buffer, cancellationToken);
                bytes = buffer.ToArray();
            }

            if (bytes.LongLength > maxBytes)
                return new Result(ResultStatus.Invalid, $"Ukuran berkas identitas maksimal {maxBytes / (1024 * 1024)} MB.");

            var detected = DetectContentType(bytes);

            if (detected == null)
                return new Result(ResultStatus.Invalid, "Format berkas identitas harus JPG, PNG, atau PDF.");

            var rootError = TryResolvePrivateRoot(out var privateRoot);

            if (rootError != null)
                return new Result(ResultStatus.Invalid, rootError);

            var now = DateTime.UtcNow;
            var documentId = Guid.NewGuid();
            var patientFolder = request.PatientId.ToString("N");
            var fileName = $"{documentId:N}{detected.Value.Extension}";
            var relativePath = $"{FolderName}/{patientFolder}/{fileName}";
            var physicalPath = Path.Combine(privateRoot, FolderName, patientFolder, fileName);
            var fileWritten = false;
            Guid? replacedId = null;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Dua unggahan bersamaan untuk pasien yang sama tidak boleh sama-sama lolos
                // pemeriksaan "belum ada dokumen".
                if (_dbContext.Database.IsNpgsql())
                {
                    await _dbContext.Database.ExecuteSqlRawAsync(
                        "SELECT pg_advisory_xact_lock(hashtext({0}));",
                        [$"PAT_IDENTITY_DOCUMENT_{request.PatientId:N}"],
                        cancellationToken);
                }

                var activeWithFile = await _dbContext.Set<MstPatientIdentityDocument>()
                    .Where(x => x.PatientId == request.PatientId &&
                                !x.IsDelete &&
                                x.IsActive &&
                                x.FilePath != null &&
                                x.FilePath != string.Empty)
                    .ToListAsync(cancellationToken);

                if (request.ReplaceDocumentId.HasValue)
                {
                    var replaced = activeWithFile.FirstOrDefault(x => x.Id == request.ReplaceDocumentId.Value);

                    if (replaced == null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return new Result(ResultStatus.NotFound, "Dokumen identitas yang akan diganti tidak ditemukan pada pasien ini.");
                    }

                    // Dokumen lama tidak ditimpa: barisnya ditandai dihapus lunak (unique index
                    // PatientId+IdentityType+IdentityNumber hanya berlaku untuk baris hidup) dan
                    // berkas fisiknya tetap tersimpan untuk penelusuran.
                    replaced.IsActive = false;
                    replaced.IsPrimary = false;
                    replaced.IsDelete = true;
                    replaced.DeleteDateTime = now;
                    replaced.DeleteBy = actorUserId;
                    replaced.UpdateDateTime = now;
                    replaced.UpdateBy = actorUserId;
                    replacedId = replaced.Id;
                }
                else if (activeWithFile.Count > 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return new Result(
                        ResultStatus.Conflict,
                        "Pasien sudah memiliki dokumen identitas. Gunakan aksi Ganti untuk menggantinya.",
                        ExistingDocumentId: activeWithFile.OrderByDescending(x => x.IsPrimary).ThenByDescending(x => x.CreateDateTime).First().Id);
                }

                // Dokumen baru menjadi primer, sama dengan aturan create yang sudah ada.
                var primaries = await _dbContext.Set<MstPatientIdentityDocument>()
                    .Where(x => x.PatientId == request.PatientId && !x.IsDelete && x.IsPrimary)
                    .ToListAsync(cancellationToken);

                foreach (var primary in primaries)
                {
                    primary.IsPrimary = false;
                    primary.UpdateDateTime = now;
                    primary.UpdateBy = actorUserId;
                }

                // Simpan pelepasan primer/penggantian lebih dulu supaya unique index primer dan
                // identitas tidak bertabrakan dengan baris baru dalam satu batch.
                await _dbContext.SaveChangesAsync(cancellationToken);

                Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);
                await File.WriteAllBytesAsync(physicalPath, bytes, cancellationToken);
                fileWritten = true;

                _dbContext.Set<MstPatientIdentityDocument>().Add(new MstPatientIdentityDocument
                {
                    Id = documentId,
                    PatientId = request.PatientId,
                    IdentityType = identityType,
                    IdentityNumber = identityNumber,
                    DocumentName = SanitizeFileName(request.FileName, detected.Value.Extension),
                    FilePath = PrivatePathPrefix + relativePath,
                    FileContentType = detected.Value.ContentType,
                    IsPrimary = true,
                    IsVerified = false,
                    IsFromKioskScan = false,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                });

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                // Metadata gagal tersimpan: berkas fisik tidak boleh tertinggal yatim.
                if (fileWritten)
                    TryDelete(physicalPath);

                throw;
            }

            // Tanpa isi berkas maupun nomor identitas.
            await _loggerService.AuditAsync(
                LogCategory,
                replacedId.HasValue ? "PatientIdentityDocument.ReplaceFile" : "PatientIdentityDocument.UploadFile",
                replacedId.HasValue ? "Dokumen identitas pasien diganti." : "Dokumen identitas pasien diunggah.",
                new
                {
                    EntityId = documentId,
                    PatientId = request.PatientId,
                    ReplacedDocumentId = replacedId,
                    IdentityType = identityType,
                    ContentType = detected.Value.ContentType,
                    SizeBytes = bytes.LongLength,
                    ActorUserId = actorUserId
                });

            return new Result(
                ResultStatus.Success,
                replacedId.HasValue ? "Dokumen identitas berhasil diganti." : "Dokumen identitas berhasil diunggah.",
                documentId);
        }

        public async Task<(Result Result, DocumentContent? Content)> OpenAsync(
            Guid documentId,
            CancellationToken cancellationToken = default)
        {
            var document = await _dbContext.Set<MstPatientIdentityDocument>()
                .AsNoTracking()
                .Where(x => x.Id == documentId && !x.IsDelete)
                .Select(x => new { x.PatientId, x.FilePath, x.FileContentType, x.DocumentName })
                .FirstOrDefaultAsync(cancellationToken);

            if (document == null || !IsPrivatePath(document.FilePath))
                return (new Result(ResultStatus.NotFound, "Berkas dokumen identitas tidak ditemukan."), null);

            var rootError = TryResolvePrivateRoot(out var privateRoot);

            if (rootError != null)
                return (new Result(ResultStatus.Invalid, rootError), null);

            // Berkas wajib berada di folder pasien pemilik dokumen; mencegah FilePath yang
            // disunting menunjuk berkas pasien lain atau keluar dari root privat.
            var allowedFolder = Path.GetFullPath(Path.Combine(privateRoot, FolderName, document.PatientId.ToString("N")))
                + Path.DirectorySeparatorChar;
            var relative = document.FilePath![PrivatePathPrefix.Length..];
            var physical = Path.GetFullPath(Path.Combine(privateRoot, relative));

            if (!physical.StartsWith(allowedFolder, StringComparison.OrdinalIgnoreCase) || !File.Exists(physical))
                return (new Result(ResultStatus.NotFound, "Berkas dokumen identitas tidak ditemukan di penyimpanan."), null);

            var contentType = document.FileContentType is "image/jpeg" or "image/png" or "application/pdf"
                ? document.FileContentType
                : "application/octet-stream";

            return (new Result(ResultStatus.Success, "OK"),
                new DocumentContent(File.OpenRead(physical), contentType, document.DocumentName ?? Path.GetFileName(physical)));
        }

        /// <summary>Root privat absolut. Ditolak bila kosong atau berada di dalam folder publik.</summary>
        private string? TryResolvePrivateRoot(out string privateRoot)
        {
            privateRoot = string.Empty;
            var configured = _configuration["FileStorage:PrivateRootPath"];

            if (string.IsNullOrWhiteSpace(configured))
                return "Penyimpanan dokumen identitas belum dikonfigurasi (FileStorage:PrivateRootPath). Hubungi admin.";

            privateRoot = AbsoluteWithSeparator(configured);

            var publicRoot = _configuration["FileStorage:UploadRootPath"];
            var publicCandidates = new List<string>
            {
                AbsoluteWithSeparator(string.IsNullOrWhiteSpace(_environment.WebRootPath)
                    ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                    : _environment.WebRootPath)
            };

            if (!string.IsNullOrWhiteSpace(publicRoot))
                publicCandidates.Add(AbsoluteWithSeparator(publicRoot));

            var root = privateRoot;

            if (publicCandidates.Any(p => root.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                return "Penyimpanan dokumen identitas salah konfigurasi: folder privat berada di dalam folder publik. Hubungi admin.";

            return null;
        }

        private string AbsoluteWithSeparator(string path)
        {
            var full = Path.IsPathRooted(path) ? path : Path.Combine(_environment.ContentRootPath, path);
            full = Path.GetFullPath(full);

            return full.EndsWith(Path.DirectorySeparatorChar) ? full : full + Path.DirectorySeparatorChar;
        }

        private static (string ContentType, string Extension)? DetectContentType(byte[] bytes)
        {
            if (bytes.Length >= 4 && bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46)
                return ("application/pdf", ".pdf");

            if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                return ("image/jpeg", ".jpg");

            if (bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
                bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A)
                return ("image/png", ".png");

            return null;
        }

        /// <summary>Nama tampilan saja (maks 200, kolom <c>DocumentName</c>); tidak pernah dipakai sebagai lokasi.</summary>
        private static string SanitizeFileName(string? fileName, string extension)
        {
            var name = Path.GetFileName(fileName ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(name))
                name = "dokumen-identitas" + extension;

            var invalid = Path.GetInvalidFileNameChars();
            name = new string(name.Select(c => invalid.Contains(c) || char.IsControl(c) || c is '<' or '>' or '"' ? '_' : c).ToArray());

            return name.Length > 200 ? name[^200..] : name;
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
                // Pembersihan terbaik-usaha; kegagalan di sini tidak boleh menutupi galat utama.
            }
        }
    }
}
