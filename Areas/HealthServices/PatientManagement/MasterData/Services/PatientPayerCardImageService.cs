using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using ImageSharpImage = SixLabors.ImageSharp.Image;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    /// <summary>
    /// Menyimpan foto kartu penjamin pasien hasil scan (RJ-DOC-DEC-041). Polanya sama dengan foto
    /// scan identitas kiosk pada PatientController: base64 ditulis sebagai file pada storage
    /// FileStorage, lalu path publiknya dicatat pada data penjamin.
    /// </summary>
    public sealed class PatientPayerCardImageService
    {
        private const string CardFolderName = "patient-payer-cards";
        private const string DefaultPublicRequestPath = "/uploads";
        private const int DefaultMaxCardImageSizeMb = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public PatientPayerCardImageService(
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _environment = environment;
        }

        /// <summary>
        /// Memvalidasi dan menulis gambar kartu. Melempar <see cref="PatientPayerCardImageValidationException"/>
        /// bila base64 rusak, bukan gambar, atau melebihi batas ukuran.
        /// </summary>
        public async Task<StoredPatientPayerCardImage> SaveAsync(
            Guid patientId,
            string payerKind,
            string cardImageBase64,
            CancellationToken cancellationToken = default)
        {
            var fileBytes = DecodeBase64(cardImageBase64);

            var maxSizeMb = _configuration.GetValue<int?>("FileStorage:MaxPayerCardImageSizeMb")
                ?? DefaultMaxCardImageSizeMb;

            if (fileBytes.Length > maxSizeMb * 1024 * 1024)
            {
                throw new PatientPayerCardImageValidationException(
                    $"Ukuran gambar kartu melebihi batas {maxSizeMb} MB.");
            }

            Image<Rgba32> image;

            try
            {
                image = ImageSharpImage.Load<Rgba32>(fileBytes);
            }
            catch (Exception ex) when (ex is UnknownImageFormatException or InvalidImageContentException)
            {
                throw new PatientPayerCardImageValidationException(
                    "Gambar kartu tidak dapat dibaca. Pastikan hasil scan berupa gambar JPG atau PNG.");
            }

            using (image)
            {
                var (rootPath, publicRequestPath) = GetFileStoragePaths();
                var relativeFolder = Path.Combine(CardFolderName, patientId.ToString("N"));
                var absoluteFolder = Path.Combine(rootPath, relativeFolder);

                Directory.CreateDirectory(absoluteFolder);

                var fileName = $"{payerKind}-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}.jpg";
                var physicalPath = Path.Combine(absoluteFolder, fileName);

                await image.SaveAsJpegAsync(
                    physicalPath,
                    new JpegEncoder { Quality = 90 },
                    cancellationToken);

                var publicPath = "/" + string.Join(
                    "/",
                    new[] { publicRequestPath, relativeFolder.Replace("\\", "/"), fileName }
                        .Select(x => x.Trim('/'))
                        .Where(x => !string.IsNullOrWhiteSpace(x)));

                return new StoredPatientPayerCardImage(publicPath, physicalPath);
            }
        }

        /// <summary>
        /// Mengganti gambar kartu asuransi pasien yang sudah ada. Mengembalikan null bila
        /// asuransi tidak ditemukan.
        /// </summary>
        public async Task<PatientPayerCardImageResponse?> UpdateInsuranceCardImageAsync(
            Guid patientInsuranceId,
            string cardImageBase64,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstPatientInsurance>()
                .FirstOrDefaultAsync(x => x.Id == patientInsuranceId && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return null;
            }

            var stored = await SaveAsync(entity.PatientId, "insurance", cardImageBase64, cancellationToken);

            try
            {
                entity.CardImagePath = stored.PublicPath;
                entity.UpdateDateTime = DateTime.UtcNow;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                DeleteFile(stored.PhysicalPath);
                throw;
            }

            return new PatientPayerCardImageResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                CardImagePath = entity.CardImagePath
            };
        }

        /// <summary>
        /// Mengganti gambar kartu penjamin perusahaan pasien yang sudah ada (RJ-DOC-REV-BE-014).
        /// Mengembalikan null bila penjamin tidak ditemukan.
        /// </summary>
        public async Task<PatientPayerCardImageResponse?> UpdateCompanyGuarantorCardImageAsync(
            Guid patientCompanyGuarantorId,
            string cardImageBase64,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstPatientCompanyGuarantor>()
                .FirstOrDefaultAsync(x => x.Id == patientCompanyGuarantorId && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return null;
            }

            var stored = await SaveAsync(entity.PatientId, "company", cardImageBase64, cancellationToken);

            try
            {
                entity.CardImagePath = stored.PublicPath;
                entity.UpdateDateTime = DateTime.UtcNow;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                DeleteFile(stored.PhysicalPath);
                throw;
            }

            return new PatientPayerCardImageResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                CardImagePath = entity.CardImagePath
            };
        }

        public static void DeleteFile(string? physicalPath)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(physicalPath) && File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        private static byte[] DecodeBase64(string? value)
        {
            var text = (value ?? string.Empty).Trim();
            var commaIndex = text.IndexOf(',');

            if (text.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && commaIndex >= 0)
            {
                text = text[(commaIndex + 1)..].Trim();
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new PatientPayerCardImageValidationException("Gambar kartu wajib diisi.");
            }

            try
            {
                return Convert.FromBase64String(text);
            }
            catch (FormatException)
            {
                throw new PatientPayerCardImageValidationException("Format base64 gambar kartu tidak valid.");
            }
        }

        // Sama dengan resolusi storage pada Program.cs (UseStaticFiles) agar file yang ditulis
        // tersaji pada PublicRequestPath.
        private (string RootPath, string PublicRequestPath) GetFileStoragePaths()
        {
            var publicRequestPath = (_configuration["FileStorage:PublicRequestPath"] ?? DefaultPublicRequestPath)
                .Replace("\\", "/")
                .Trim();

            if (!publicRequestPath.StartsWith('/'))
            {
                publicRequestPath = "/" + publicRequestPath;
            }

            publicRequestPath = publicRequestPath.TrimEnd('/');

            var configuredRoot = _configuration["FileStorage:UploadRootPath"];

            if (!string.IsNullOrWhiteSpace(configuredRoot))
            {
                var rootPath = configuredRoot.Trim();

                if (!Path.IsPathRooted(rootPath))
                {
                    rootPath = Path.Combine(_environment.ContentRootPath, rootPath);
                }

                return (Path.GetFullPath(rootPath), publicRequestPath);
            }

            var webRootPath = string.IsNullOrWhiteSpace(_environment.WebRootPath)
                ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                : _environment.WebRootPath;

            return (Path.Combine(webRootPath, publicRequestPath.TrimStart('/')), publicRequestPath);
        }
    }

    public sealed record StoredPatientPayerCardImage(string PublicPath, string PhysicalPath);

    public sealed class PatientPayerCardImageValidationException : Exception
    {
        public PatientPayerCardImageValidationException(string message) : base(message) { }
    }
}
