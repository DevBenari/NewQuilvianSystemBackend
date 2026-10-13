using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Surat rujukan privat (<c>RJ-DOC-REV-BE-019</c>, <c>RJ-DOC-DEC-081</c>, <c>082</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Privat, bukan publik.</b> Berkas ditulis ke <c>FileStorage:PrivateRootPath</c> yang
    /// <b>wajib</b> berada di luar <c>FileStorage:UploadRootPath</c> (folder yang dilayani static
    /// files). Surat rujukan memuat diagnosa, jadi hanya dapat dibaca lewat endpoint berizin.
    /// Bila root privat kosong atau berada di dalam folder publik, unggah ditolak.
    /// </para>
    /// <para>
    /// <b>Format diperiksa dari isi</b> (magic bytes), bukan nama: PDF (<c>%PDF</c>), JPEG
    /// (<c>FF D8 FF</c>), PNG (<c>89 50 4E 47</c>). Maksimal 5 MB per berkas dan 10 berkas aktif.
    /// </para>
    /// <para>
    /// Setiap unggah/hapus menghitung ulang kelengkapan rujukan dan menulis revisi
    /// <c>DocumentAdded</c>/<c>DocumentRemoved</c>, atau <c>Completed</c> bila unggah itu
    /// melengkapinya.
    /// </para>
    /// </remarks>
    public class ReferralDocumentStorageService
    {
        public const string Pm12 = "RJ-VAL-PM-12: Format berkas harus PDF, JPG, atau PNG.";
        public const string Pm13 = "RJ-VAL-PM-13: Ukuran berkas maksimal 5 MB, paling banyak 10 berkas.";
        public const string Pm14 = "RJ-VAL-PM-14: Surat rujukan tidak dapat diunggah dari Kiosk untuk kunjungan ini. Silakan hubungi petugas.";

        public const int MaxActiveDocuments = 10;
        private const int DefaultMaxSizeMb = 5;
        private const string FolderName = "referral-documents";
        private static readonly TimeSpan KioskUploadWindow = TimeSpan.FromMinutes(30);

        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly EncounterReferralService _referralService;

        public ReferralDocumentStorageService(
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            EncounterReferralService referralService)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _environment = environment;
            _referralService = referralService;
        }

        public sealed record UploadFile(string FileName, long Length, Func<Stream> OpenReadStream);

        public sealed record DocumentContent(Stream Stream, string ContentType, string FileName);

        public async Task<EncounterReferralResult> UploadAsync(
            Guid encounterId,
            IReadOnlyList<UploadFile> files,
            bool fromKiosk,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (files.Count == 0)
                return EncounterReferralResult.Invalid("Pilih minimal satu berkas surat rujukan.");

            var encounter = await _dbContext.Set<RegPatientEncounter>()
                .FirstOrDefaultAsync(x => x.Id == encounterId && !x.IsDelete, cancellationToken);

            if (encounter == null)
                return EncounterReferralResult.NotFound("Kunjungan tidak ditemukan.");

            if (fromKiosk &&
                (!encounter.IsFromKiosk ||
                 encounter.CreateDateTime < DateTime.UtcNow.Subtract(KioskUploadWindow) ||
                 encounter.EncounterStatus > EncounterStatus.WaitingForDoctor ||
                 encounter.IsCancel))
            {
                return EncounterReferralResult.Forbidden(Pm14);
            }

            if (EncounterReferralService.IsLocked(encounter.EncounterStatus, encounter.IsCancel))
                return EncounterReferralResult.Conflict(EncounterReferralService.Pm09);

            var referral = await _dbContext.Set<RegEncounterReferral>()
                .FirstOrDefaultAsync(x => x.PatientEncounterId == encounterId && !x.IsDelete, cancellationToken);

            if (referral == null)
                return EncounterReferralResult.Invalid("Rincian rujukan belum ada. Simpan rincian rujukan lebih dulu.");

            var activeCount = await _referralService.CountActiveDocumentsAsync(referral.Id, cancellationToken);
            var maxBytes = (_configuration.GetValue<int?>("FileStorage:MaxReferralDocumentSizeMb") ?? DefaultMaxSizeMb) * 1024L * 1024L;

            if (activeCount + files.Count > MaxActiveDocuments || files.Any(f => f.Length <= 0 || f.Length > maxBytes))
                return EncounterReferralResult.Invalid(Pm13);

            var rootError = TryResolvePrivateRoot(out var privateRoot);

            if (rootError != null)
                return EncounterReferralResult.Invalid(rootError);

            var folder = Path.Combine(privateRoot, FolderName, encounterId.ToString("N"));
            Directory.CreateDirectory(folder);

            var written = new List<string>();
            var added = new List<object>();
            var now = DateTime.UtcNow;
            var wasComplete = referral.IsComplete;
            var nextOrder = await _dbContext.Set<RegEncounterReferralDocument>()
                .Where(x => x.EncounterReferralId == referral.Id)
                .Select(x => (int?)x.PageOrder)
                .MaxAsync(cancellationToken) ?? 0;

            try
            {
                foreach (var file in files)
                {
                    await using var input = file.OpenReadStream();
                    using var buffer = new MemoryStream();
                    await input.CopyToAsync(buffer, cancellationToken);

                    var bytes = buffer.ToArray();
                    var detected = DetectContentType(bytes);

                    if (detected == null)
                    {
                        DeleteFiles(written);
                        return EncounterReferralResult.Invalid(Pm12);
                    }

                    var documentId = Guid.NewGuid();
                    var fileName = $"{documentId:N}{detected.Value.Extension}";
                    var physical = Path.Combine(folder, fileName);

                    await File.WriteAllBytesAsync(physical, bytes, cancellationToken);
                    written.Add(physical);

                    nextOrder++;

                    _dbContext.Set<RegEncounterReferralDocument>().Add(new RegEncounterReferralDocument
                    {
                        Id = documentId,
                        EncounterReferralId = referral.Id,
                        OriginalFileName = SanitizeFileName(file.FileName, detected.Value.Extension),
                        ContentType = detected.Value.ContentType,
                        SizeBytes = bytes.LongLength,
                        StoragePath = Path.Combine(FolderName, encounterId.ToString("N"), fileName).Replace('\\', '/'),
                        PageOrder = nextOrder,
                        CreateDateTime = now,
                        CreateBy = actorUserId
                    });

                    added.Add(new { documentId, pageOrder = nextOrder, detected.Value.ContentType, sizeBytes = bytes.LongLength });
                }

                EncounterReferralService.ApplyCompletion(referral, encounter, activeCount + files.Count, now);
                referral.RowVersion = Guid.NewGuid();
                referral.UpdateDateTime = now;
                referral.UpdateBy = actorUserId;

                _referralService.AddRevision(
                    referral.Id,
                    !wasComplete && referral.IsComplete ? ReferralRevisionType.Completed : ReferralRevisionType.DocumentAdded,
                    null,
                    new { documents = added },
                    actorUserId,
                    now);

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                // Metadata gagal tersimpan: berkas fisik yang sudah ditulis tidak boleh tertinggal yatim.
                DeleteFiles(written);
                throw;
            }

            return EncounterReferralResult.Ok($"{files.Count} berkas surat rujukan berhasil diunggah.");
        }

        public async Task<(EncounterReferralResult Result, DocumentContent? Content)> OpenAsync(
            Guid encounterId,
            Guid documentId,
            CancellationToken cancellationToken = default)
        {
            var document = await _dbContext.Set<RegEncounterReferralDocument>()
                .AsNoTracking()
                .Where(x => x.Id == documentId &&
                            !x.IsDelete &&
                            x.EncounterReferral != null &&
                            x.EncounterReferral.PatientEncounterId == encounterId)
                .Select(x => new { x.StoragePath, x.ContentType, x.OriginalFileName })
                .FirstOrDefaultAsync(cancellationToken);

            if (document == null)
                return (EncounterReferralResult.NotFound("Surat rujukan tidak ditemukan."), null);

            var rootError = TryResolvePrivateRoot(out var privateRoot);

            if (rootError != null)
                return (EncounterReferralResult.Invalid(rootError), null);

            var physical = Path.GetFullPath(Path.Combine(privateRoot, document.StoragePath));

            if (!physical.StartsWith(privateRoot, StringComparison.OrdinalIgnoreCase) || !File.Exists(physical))
                return (EncounterReferralResult.NotFound("Berkas surat rujukan tidak ditemukan di penyimpanan."), null);

            return (EncounterReferralResult.Ok("OK"),
                new DocumentContent(File.OpenRead(physical), document.ContentType, document.OriginalFileName));
        }

        public async Task<EncounterReferralResult> DeleteAsync(
            Guid encounterId,
            Guid documentId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var encounter = await _dbContext.Set<RegPatientEncounter>()
                .FirstOrDefaultAsync(x => x.Id == encounterId && !x.IsDelete, cancellationToken);

            if (encounter == null)
                return EncounterReferralResult.NotFound("Kunjungan tidak ditemukan.");

            if (EncounterReferralService.IsLocked(encounter.EncounterStatus, encounter.IsCancel))
                return EncounterReferralResult.Conflict(EncounterReferralService.Pm09);

            var document = await _dbContext.Set<RegEncounterReferralDocument>()
                .Include(x => x.EncounterReferral)
                .FirstOrDefaultAsync(x => x.Id == documentId &&
                                          !x.IsDelete &&
                                          x.EncounterReferral != null &&
                                          x.EncounterReferral.PatientEncounterId == encounterId,
                    cancellationToken);

            if (document == null)
                return EncounterReferralResult.NotFound("Surat rujukan tidak ditemukan.");

            var referral = document.EncounterReferral!;
            var now = DateTime.UtcNow;

            document.IsDelete = true;
            document.DeleteDateTime = now;
            document.DeleteBy = actorUserId;

            var activeCount = await _referralService.CountActiveDocumentsAsync(referral.Id, cancellationToken) - 1;

            EncounterReferralService.ApplyCompletion(referral, encounter, activeCount, now);
            referral.RowVersion = Guid.NewGuid();
            referral.UpdateDateTime = now;
            referral.UpdateBy = actorUserId;

            _referralService.AddRevision(
                referral.Id,
                ReferralRevisionType.DocumentRemoved,
                new { documentId, document.PageOrder },
                null,
                actorUserId,
                now);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return EncounterReferralResult.Ok("Surat rujukan berhasil dihapus.");
        }

        /// <summary>
        /// Root privat absolut. Ditolak bila kosong atau berada di dalam folder publik.
        /// </summary>
        private string? TryResolvePrivateRoot(out string privateRoot)
        {
            privateRoot = string.Empty;
            var configured = _configuration["FileStorage:PrivateRootPath"];

            if (string.IsNullOrWhiteSpace(configured))
                return "Penyimpanan surat rujukan belum dikonfigurasi (FileStorage:PrivateRootPath). Hubungi admin.";

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
                return "Penyimpanan surat rujukan salah konfigurasi: folder privat berada di dalam folder publik. Hubungi admin.";

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

        private static string SanitizeFileName(string? fileName, string extension)
        {
            var name = Path.GetFileName(fileName ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(name))
                name = "surat-rujukan" + extension;

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
}
