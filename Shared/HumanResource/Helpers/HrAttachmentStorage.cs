using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace QuilvianSystemBackend.Shared.HumanResource.Helpers
{
    /// <summary>Hasil simpan lampiran SDM. <see cref="Key"/> disimpan di kolom URL/lampiran milik fitur.</summary>
    public sealed record HrStoredAttachment(string Key, string FileName, string ContentType, long SizeBytes, string Checksum);

    /// <summary>Berkas siap diunduh: lokasi fisik, jenis konten, dan nama unduhan.</summary>
    public sealed record HrAttachmentFile(string PhysicalPath, string ContentType, string DownloadName, Guid OwnerUserId);

    /// <summary>
    /// Penyimpanan lampiran modul SDM (register masalah MSL-15, keputusan DEC-FER-02 opsi A, 18 November 2026).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Contoh: pegawai mengunggah kuitansi kacamata saat mengajukan restitusi. Berkas disimpan di luar <c>wwwroot</c>
    /// (<c>App_Data/human-resource-attachments</c>), sehingga tidak bisa dibuka tanpa melewati endpoint ber-hak akses.
    /// </para>
    /// <para>
    /// Kunci berkas berbentuk <c>{kategori}/{tahun}/{bulan}/{idAkunPengunggah}/{guid}{ekstensi}</c>. Kategori dibatasi
    /// daftar <see cref="Categories"/>; ID pengunggah ikut di kunci agar endpoint unduh bisa mengizinkan pemilik berkas
    /// tanpa tabel metadata baru. Nama berkas acak 128 bit, jadi tidak bisa ditebak.
    /// </para>
    /// </remarks>
    public class HrAttachmentStorage
    {
        public const long DefaultMaximumFileSizeBytes = 10L * 1024L * 1024L;
        private const string DefaultRelativeRoot = "App_Data/human-resource-attachments";

        /// <summary>Kategori lampiran yang dikenali beserta pemilik fiturnya.</summary>
        public static readonly IReadOnlyDictionary<string, string> Categories = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["restitution-receipt"] = "Nota/kuitansi klaim restitusi",
            ["restitution-medical-resume"] = "Resume medis klaim restitusi",
            ["travel-receipt"] = "Bukti pengeluaran perjalanan dinas",
            ["performance-evidence"] = "Bukti capaian penilaian kinerja",
            ["performance-dispute-evidence"] = "Bukti pendukung sanggahan nilai",
            // MSL-15 tahap 2 (19 Nov 2026)
            ["benefit-claim-document"] = "Dokumen klaim COB benefit",
            ["recruitment-candidate-document"] = "Dokumen kandidat rekrutmen",
            ["recruitment-mcu-certificate"] = "Sertifikat MCU pra-kerja",
            ["attendance-correction-evidence"] = "Bukti koreksi presensi atas nama",
            ["attendance-official-duty"] = "Lampiran dinas luar",
        };

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".jpg", ".jpeg", ".png", ".webp", ".doc", ".docx", ".xls", ".xlsx"
        };

        private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "application/pdf",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp",
            [".doc"] = "application/msword",
            [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            [".xls"] = "application/vnd.ms-excel",
            [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        };

        private static readonly Regex KeyPattern = new(
            @"^(?<category>[a-z-]+)/(?<year>\d{4})/(?<month>\d{2})/(?<owner>[0-9a-f]{32})/(?<file>[0-9a-f]{32}\.[a-z0-9]{2,5})$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public HrAttachmentStorage(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        public long MaximumFileSizeBytes =>
            long.TryParse(_configuration["HumanResource:Attachment:MaximumFileSizeBytes"], out var value) && value > 0
                ? value
                : DefaultMaximumFileSizeBytes;

        /// <summary>Menyimpan berkas. Galat isian (ekstensi, ukuran, kategori) dilempar sebagai <see cref="ArgumentException"/>.</summary>
        public async Task<HrStoredAttachment> SaveAsync(string category, Guid ownerUserId, IFormFile? file, CancellationToken cancellationToken = default)
        {
            if (!Categories.ContainsKey(category ?? string.Empty))
                throw new ArgumentException("Jenis lampiran tidak dikenal.");
            if (ownerUserId == Guid.Empty)
                throw new UnauthorizedAccessException("Identitas pengunggah tidak terbaca. Silakan masuk ulang.");
            if (file == null || file.Length <= 0)
                throw new ArgumentException("Pilih berkas yang akan diunggah.");
            if (file.Length > MaximumFileSizeBytes)
                throw new ArgumentException($"Ukuran berkas melebihi {MaximumFileSizeBytes / (1024 * 1024)} MB.");

            var originalName = Path.GetFileName(file.FileName ?? string.Empty).Trim();
            var extension = Path.GetExtension(originalName).ToLowerInvariant();
            if (string.IsNullOrEmpty(originalName) || originalName.Length > 255)
                throw new ArgumentException("Nama berkas tidak valid.");
            if (!AllowedExtensions.Contains(extension))
                throw new ArgumentException("Jenis berkas tidak diizinkan. Gunakan PDF, gambar (JPG/PNG/WEBP), Word, atau Excel.");

            var now = DateTime.UtcNow;
            var key = $"{category}/{now:yyyy}/{now:MM}/{ownerUserId:N}/{Guid.NewGuid():N}{extension}";
            var physicalPath = ResolvePhysicalPath(key);
            Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);
            var temporaryPath = physicalPath + ".tmp";

            try
            {
                using var sha256 = SHA256.Create();
                await using (var input = file.OpenReadStream())
                await using (var output = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous))
                {
                    var buffer = new byte[81920];
                    long total = 0;
                    int read;
                    while ((read = await input.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
                    {
                        total += read;
                        if (total > MaximumFileSizeBytes)
                            throw new ArgumentException($"Ukuran berkas melebihi {MaximumFileSizeBytes / (1024 * 1024)} MB.");
                        sha256.TransformBlock(buffer, 0, read, null, 0);
                        await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                    }

                    sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                }

                File.Move(temporaryPath, physicalPath, false);
                return new HrStoredAttachment(
                    key,
                    originalName,
                    ContentTypes.TryGetValue(extension, out var contentType) ? contentType : "application/octet-stream",
                    file.Length,
                    Convert.ToHexString(sha256.Hash ?? Array.Empty<byte>()).ToLowerInvariant());
            }
            catch
            {
                TryDelete(temporaryPath);
                TryDelete(physicalPath);
                throw;
            }
        }

        /// <summary>
        /// Membuka berkas berdasarkan kunci. Kunci yang bentuknya salah, kategori berbeda dari <paramref name="expectedCategory"/>,
        /// atau berkas yang tidak ada dijawab <see cref="KeyNotFoundException"/> agar keberadaannya tidak bocor.
        /// </summary>
        public HrAttachmentFile Open(string? key, string expectedCategory)
        {
            var match = KeyPattern.Match(key ?? string.Empty);
            if (!match.Success || !string.Equals(match.Groups["category"].Value, expectedCategory, StringComparison.Ordinal))
                throw new KeyNotFoundException("Lampiran tidak ditemukan.");

            var physicalPath = ResolvePhysicalPath(key!);
            if (!File.Exists(physicalPath))
                throw new KeyNotFoundException("Lampiran tidak ditemukan.");

            var extension = Path.GetExtension(physicalPath);
            var contentType = ContentTypes.TryGetValue(extension, out var type) ? type : "application/octet-stream";
            var owner = Guid.ParseExact(match.Groups["owner"].Value, "N");
            return new HrAttachmentFile(physicalPath, contentType, $"lampiran-{expectedCategory}{extension}", owner);
        }

        /// <summary>Kategori dari kunci, atau null bila bentuk kunci tidak sah.</summary>
        public static string? CategoryOf(string? key)
        {
            var match = KeyPattern.Match(key ?? string.Empty);
            return match.Success ? match.Groups["category"].Value : null;
        }

        /// <summary>ID akun pengunggah dari kunci, atau null bila bentuk kunci tidak sah.</summary>
        public static Guid? OwnerOf(string? key)
        {
            var match = KeyPattern.Match(key ?? string.Empty);
            return match.Success ? Guid.ParseExact(match.Groups["owner"].Value, "N") : null;
        }

        private string ResolvePhysicalPath(string key)
        {
            var configuredRoot = _configuration["HumanResource:Attachment:RelativeRootPath"];
            var relativeRoot = string.IsNullOrWhiteSpace(configuredRoot) ? DefaultRelativeRoot : configuredRoot.Trim();
            var root = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, relativeRoot));
            var physical = Path.GetFullPath(Path.Combine(root, key.Replace('/', Path.DirectorySeparatorChar)));
            var prefix = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
            if (!physical.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                throw new KeyNotFoundException("Lampiran tidak ditemukan.");
            return physical;
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (IOException)
            {
                // Berkas sementara; kegagalan hapus tidak boleh menutupi galat aslinya.
            }
        }
    }
}
