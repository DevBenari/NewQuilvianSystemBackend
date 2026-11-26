using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services
{
    public class SpkRkkIssuanceService : ISpkRkkIssuanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SpkRkkIssuanceService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;

        private const string SpkSequenceKey = "TRX_SPK_RKK_ISSUANCE";
        private const string SpkSeriesPrefix = "SPK-RKK";
        private const int SpkSequenceDigits = 4;
        private const string PublicVerifyPath = "/api/v1/public/clinical-privileges/verify/";
        private const int QrPixelsPerModule = 8;

        public SpkRkkIssuanceService(
            ApplicationDbContext context,
            ILogger<SpkRkkIssuanceService> logger,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            NumberSeriesAllocator numberSeriesAllocator)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _environment = environment;
            _numberSeriesAllocator = numberSeriesAllocator ?? throw new ArgumentNullException(nameof(numberSeriesAllocator));
        }

        public async Task<SpkRkkDetailDto> IssueSpkRkkDraftAsync(
            IssueSpkRkkRequestDto request,
            Guid issuedByUserId,
            CancellationToken cancellationToken = default)
        {
            // BE-HRD-432: dua penerbitan draf paralel untuk permohonan yang sama diserialkan, sehingga cek
            // "sudah punya SPK-RKK" di bawah tidak dapat dilewati race.
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            await AcquireTransactionLockAsync($"HRD_SPK_RKK_DRAFT_{request.ApplicationId:N}", cancellationToken);

            var application = await _context.TrxCredentialingApplications
                .Include(a => a.WorkforceProfile)
                    .ThenInclude(p => p!.PrimaryDepartment)
                .Include(a => a.WorkforceProfile)
                    .ThenInclude(p => p!.PrimaryPosition)
                .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

            if (application == null)
            {
                throw new KeyNotFoundException($"Permohonan kredensial dengan ID {request.ApplicationId} tidak ditemukan.");
            }

            if (!application.ApplicationStatus.Equals("PlenarySessionConcluded", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Draf SPK-RKK hanya dapat diterbitkan setelah Rapat Pleno Komite Kredensial selesai dilaksanakan (Status: PlenarySessionConcluded).");
            }

            // MSL-16 (19 Nov 2026): permohonan yang ditolak pleno atau sudah punya SPK-RKK tidak boleh diterbitkan drafnya lagi.
            var plenary = CredentialingWorklistQueryService.ReadComplianceSnapshot(application.ComplianceResultJson);
            if (string.Equals(plenary.Decision, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Sidang pleno menolak permohonan ini, sehingga draf SPK-RKK tidak dapat diterbitkan.");
            }

            var hasIssuance = await _context.TrxSpkRkkIssuances
                .AnyAsync(i => i.CredentialingApplicationId == application.Id && i.IsActive && !i.IsDelete && !i.IsRevoked, cancellationToken);
            if (hasIssuance)
            {
                throw new InvalidOperationException("Permohonan ini sudah memiliki SPK-RKK. Buka daftar SPK-RKK untuk melihat atau menandatanganinya.");
            }

            // Ekstrak daftar kewenangan klinis yang disetujui dari BAP Sidang Pleno
            string approvedPrivilegesJson = "[]";
            if (!string.IsNullOrWhiteSpace(application.ComplianceResultJson))
            {
                try
                {
                    using var doc = JsonDocument.Parse(application.ComplianceResultJson);
                    if (doc.RootElement.TryGetProperty("FinalApprovedPrivileges", out var privs))
                    {
                        approvedPrivilegesJson = privs.GetRawText();
                    }
                }
                catch
                {
                    approvedPrivilegesJson = "[]";
                }
            }

            // FIN-HRD-09-BE-007 / M09-P0-08: Snapshot RKK wajib berisi kewenangan klinis yang disetujui
            if (string.IsNullOrWhiteSpace(approvedPrivilegesJson) || approvedPrivilegesJson.Trim() == "[]")
            {
                throw new InvalidOperationException("Draf SPK-RKK tidak dapat diterbitkan karena tidak ada kewenangan klinis yang disetujui dalam BAP Sidang Pleno.");
            }

            // BE-HRD-432 / M09-P0-08: urutan SPK/RKK dari NumberSeriesAllocator (reset tahunan), bukan Count+1.
            // Format nomor tetap SPK|RKK/RSQ-DIRUT/{bulan romawi}/{tahun}/{NNNN}; SPK dan RKK berbagi satu urutan.
            var (year, sequenceNumber) = await AllocateSpkSequenceAsync(issuedByUserId, cancellationToken);
            var monthRoman = GetRomanMonth(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ResolveBusinessTimeZone()).Month);
            var spkNumber = $"SPK/RSQ-DIRUT/{monthRoman}/{year}/{sequenceNumber}";
            var rkkNumber = $"RKK/RSQ-DIRUT/{monthRoman}/{year}/{sequenceNumber}";

            var validFrom = DateTime.SpecifyKind(request.ValidFrom, DateTimeKind.Utc);
            // SIKLUS 3 TAHUN SESUAI CRD-B3
            var validUntil = validFrom.AddYears(3);

            var issuance = new TrxSpkRkkIssuance
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = application.WorkforceProfileId,
                CredentialingApplicationId = application.Id,
                SpkNumber = spkNumber,
                RkkNumber = rkkNumber,
                IssuedAt = DateTime.UtcNow,
                ValidFrom = validFrom,
                ValidUntil = validUntil,
                ClinicalPrivilegeType = request.ClinicalPrivilegeType ?? "Regular",
                DirectorGeneralUserId = null,
                DirectorGeneralName = "Direktur Utama RS Quilvian",
                IsSignedByDirectorGeneral = false,
                SignedAt = null,
                IssuanceStatus = "Draft",
                ApprovedPrivilegesSnapshotJson = approvedPrivilegesJson,
                IsActive = true,
                CreateBy = issuedByUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _context.TrxSpkRkkIssuances.Add(issuance);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Draf SPK-RKK #{SpkNumber} berhasil disusun untuk nakes {ProfileId} (Siklus 3 tahun: {From:d} s.d. {Until:d})",
                issuance.SpkNumber, application.WorkforceProfileId, validFrom, validUntil);

            return MapToDto(issuance, application.WorkforceProfile);
        }

        public async Task<SpkRkkDetailDto> SignSpkRkkByDirectorGeneralAsync(
            SignSpkRkkDto request,
            Guid directorGeneralUserId,
            CancellationToken cancellationToken = default)
        {
            // BE-HRD-432: dua pengesahan paralel tidak boleh menggandakan arsip e-Dossier dan kewenangan klinis.
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            await AcquireTransactionLockAsync($"HRD_SPK_RKK_SIGN_{request.SpkRkkIssuanceId:N}", cancellationToken);

            var issuance = await _context.TrxSpkRkkIssuances
                .Include(i => i.WorkforceProfile)
                    .ThenInclude(p => p!.PrimaryDepartment)
                .Include(i => i.WorkforceProfile)
                    .ThenInclude(p => p!.PrimaryPosition)
                .FirstOrDefaultAsync(i => i.Id == request.SpkRkkIssuanceId, cancellationToken);

            if (issuance == null)
            {
                throw new KeyNotFoundException($"Penerbitan SPK-RKK dengan ID {request.SpkRkkIssuanceId} tidak ditemukan.");
            }

            if (issuance.IsSignedByDirectorGeneral)
            {
                throw new CredentialingConflictException($"SPK-RKK #{issuance.SpkNumber} sudah disahkan sebelumnya pada {issuance.SignedAt:yyyy-MM-dd HH:mm:ss} UTC.");
            }

            if (issuance.IsRevoked || !issuance.IsActive || issuance.IsDelete
                || !string.Equals(issuance.IssuanceStatus, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                throw new CredentialingConflictException($"SPK-RKK #{issuance.SpkNumber} tidak berstatus Draf sehingga tidak dapat disahkan (status: {issuance.IssuanceStatus}).");
            }

            // BE-HRD-432 / M09-P0-09: snapshot kewenangan harus terbaca utuh SEBELUM pengesahan; kegagalan baca tidak
            // boleh berakhir dengan SPK aktif tanpa kewenangan.
            List<PrivilegeAssessmentItemDto> privilegeItems;
            try
            {
                privilegeItems = JsonSerializer.Deserialize<List<PrivilegeAssessmentItemDto>>(issuance.ApprovedPrivilegesSnapshotJson ?? "[]")
                    ?? new List<PrivilegeAssessmentItemDto>();
            }
            catch (JsonException)
            {
                throw new InvalidOperationException($"Snapshot kewenangan klinis SPK-RKK #{issuance.SpkNumber} rusak; pengesahan dibatalkan.");
            }

            if (privilegeItems.Count == 0)
            {
                throw new InvalidOperationException($"SPK-RKK #{issuance.SpkNumber} tidak memuat kewenangan klinis hasil pleno; pengesahan dibatalkan.");
            }

            var snapshotCatalogIds = privilegeItems.Select(p => p.CatalogId).Distinct().ToList();
            var catalogs = await _context.MstClinicalPrivilegeCatalogs
                .Where(c => snapshotCatalogIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, cancellationToken);
            if (catalogs.Count != snapshotCatalogIds.Count)
            {
                throw new InvalidOperationException($"Sebagian katalog kewenangan pada SPK-RKK #{issuance.SpkNumber} tidak ditemukan; pengesahan dibatalkan.");
            }

            var dirutUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == directorGeneralUserId, cancellationToken);

            var dirutName = dirutUser?.DisplayName ?? dirutUser?.UserName ?? "Direktur Utama RS Quilvian";

            var now = DateTime.UtcNow;

            // 1. Buat hash integritas digital SHA-256
            var rawSignaturePayload = $"{issuance.SpkNumber}|{issuance.RkkNumber}|{issuance.WorkforceProfileId}|{issuance.ValidFrom:O}|{issuance.ValidUntil:O}|{directorGeneralUserId}|{now:O}";
            var verificationHash = GenerateSha256(rawSignaturePayload);

            issuance.IsSignedByDirectorGeneral = true;
            issuance.SignedAt = now;
            issuance.DirectorGeneralUserId = directorGeneralUserId;
            issuance.DirectorGeneralName = dirutName;
            issuance.VerificationHash = verificationHash;
            issuance.IssuanceStatus = "Active";
            issuance.SpkPdfFilePath = $"/storage/spk-rkk/{issuance.Id}_SPK.pdf";
            issuance.RkkPdfFilePath = $"/storage/spk-rkk/{issuance.Id}_RKK.pdf";
            issuance.QrCodeImagePath = $"/storage/qr-codes/{verificationHash}.png";
            issuance.UpdateBy = directorGeneralUserId;
            issuance.UpdateDateTime = now;

            // FIN-HRD-09-BE-007 / M09-P0-09: file fisik PDF SPK/RKK dan QR wajib benar-benar terbentuk. Gagal tulis
            // → pengecualian, transaksi tidak di-commit, SPK tetap Draf dan kewenangan tidak diaktifkan.
            EnsurePhysicalFilesCreated(issuance, issuance.WorkforceProfile, dirutName, verificationHash, throwOnFailure: true);

            // 2. Integrasi Auto-Arsip E-Dossier Sub-02 (INT-HR-68)
            var eDossierDoc = new WfpDocument
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = issuance.WorkforceProfileId,
                DocumentType = "SPK_RKK",
                DocumentName = $"Surat Penugasan Klinis & RKK ({issuance.SpkNumber})",
                DocumentNumber = issuance.SpkNumber,
                IssueDate = issuance.ValidFrom,
                ExpiredDate = issuance.ValidUntil,
                IssuingAuthority = $"Direktur Utama RS Quilvian ({dirutName})",
                FilePath = issuance.SpkPdfFilePath,
                FileContentType = "application/pdf",
                IsVerified = true,
                VerifiedAt = now,
                VerifiedByUserId = directorGeneralUserId,
                VerificationNote = $"SPK-RKK disahkan e-Sign Direktur Utama. Hash: {verificationHash[..16]}...",
                Description = $"Surat Penugasan Klinis siklus 3 tahun dan Rincian Kewenangan Klinis sah.",
                IsActive = true,
                CreateBy = directorGeneralUserId,
                CreateDateTime = now
            };

            _context.WfpDocuments.Add(eDossierDoc);
            issuance.EDossierDocumentId = eDossierDoc.Id;

            // 3. Aktivasi Kewenangan Klinis Nakes di WfpClinicalPrivilege (Akses SIMRS & Roster)
            foreach (var item in privilegeItems)
            {
                var catalog = catalogs[item.CatalogId];

                // Jika Level 4 (Not Permitted), blokir layanan
                var isDenied = item.RecommendedSupervisionLevel == SupervisionLevel.Level4_NotPermitted;
                var requiresSupervision = item.RecommendedSupervisionLevel == SupervisionLevel.Level2_UnderSupervision ||
                                          item.RecommendedSupervisionLevel == SupervisionLevel.Level3_Proctored;

                var privilege = new WfpClinicalPrivilege
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = issuance.WorkforceProfileId,
                    ClinicalPrivilegeCatalogId = catalog.Id,
                    PrivilegeCode = catalog.PrivilegeCode,
                    PrivilegeName = catalog.PrivilegeName,
                    PrivilegeType = ClinicalPrivilegeType.CorePrivilege,
                    ClinicalScope = catalog.Description,
                    EffectiveStartDate = issuance.ValidFrom,
                    EffectiveEndDate = issuance.ValidUntil,
                    PrivilegeStatus = isDenied ? ClinicalPrivilegeStatus.Expired : ClinicalPrivilegeStatus.Active,
                    IsSupervisionRequired = requiresSupervision,
                    IsClinicalServiceBlocked = isDenied,
                    IsSchedulingBlocked = isDenied,
                    GrantedAt = now,
                    GrantedByUserId = directorGeneralUserId,
                    ApprovalNotes = $"Disahkan dalam SPK #{issuance.SpkNumber}. Level: {item.RecommendedSupervisionLevel}",
                    IsActive = !isDenied,
                    CreateBy = directorGeneralUserId,
                    CreateDateTime = now
                };

                _context.WfpClinicalPrivileges.Add(privilege);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "PENGESAHAN SUKSES: SPK-RKK #{SpkNumber} berhasil disahkan oleh Direktur Utama ({Dirut}). E-Dossier #{DocId} terarsip.",
                issuance.SpkNumber, dirutName, eDossierDoc.Id);

            return MapToDto(issuance, issuance.WorkforceProfile);
        }

        public async Task<SpkRkkDetailDto> GetSpkRkkDetailAsync(
            Guid spkRkkId,
            CancellationToken cancellationToken = default)
        {
            var issuance = await _context.TrxSpkRkkIssuances
                .Include(i => i.WorkforceProfile)
                    .ThenInclude(p => p!.PrimaryDepartment)
                .Include(i => i.WorkforceProfile)
                    .ThenInclude(p => p!.PrimaryPosition)
                .FirstOrDefaultAsync(i => i.Id == spkRkkId, cancellationToken);

            if (issuance == null)
            {
                throw new KeyNotFoundException($"SPK-RKK dengan ID {spkRkkId} tidak ditemukan.");
            }

            return MapToDto(issuance, issuance.WorkforceProfile);
        }

        public async Task<List<SpkRkkDetailDto>> GetSpkRkkByEmployeeAsync(
            Guid workforceProfileId,
            CancellationToken cancellationToken = default)
        {
            var profile = await _context.MstWorkforceProfiles
                .Include(p => p.PrimaryDepartment)
                .Include(p => p.PrimaryPosition)
                .FirstOrDefaultAsync(p => p.Id == workforceProfileId, cancellationToken);

            if (profile == null)
            {
                throw new KeyNotFoundException($"Profil tenaga kerja dengan ID {workforceProfileId} tidak ditemukan.");
            }

            var issuances = await _context.TrxSpkRkkIssuances
                .Where(i => i.WorkforceProfileId == workforceProfileId)
                .OrderByDescending(i => i.IssuedAt)
                .ToListAsync(cancellationToken);

            return issuances.Select(i => MapToDto(i, profile)).ToList();
        }

        public async Task<(byte[] FileBytes, string FileName, string ContentType)> GetSpkRkkFileAsync(
            Guid spkRkkId,
            string fileType,
            CancellationToken cancellationToken = default)
        {
            var issuance = await _context.TrxSpkRkkIssuances
                .Include(i => i.WorkforceProfile)
                    .ThenInclude(p => p!.PrimaryDepartment)
                .Include(i => i.WorkforceProfile)
                    .ThenInclude(p => p!.PrimaryPosition)
                .FirstOrDefaultAsync(i => i.Id == spkRkkId, cancellationToken);

            if (issuance == null)
            {
                throw new KeyNotFoundException($"SPK-RKK dengan ID {spkRkkId} tidak ditemukan.");
            }

            var isRkk = string.Equals(fileType, "RKK", StringComparison.OrdinalIgnoreCase);
            var fileName = isRkk ? $"{issuance.Id}_RKK.pdf" : $"{issuance.Id}_SPK.pdf";
            var relativePath = isRkk ? issuance.RkkPdfFilePath : issuance.SpkPdfFilePath;

            var physicalPath = ResolvePhysicalFilePath(relativePath);
            if (!string.IsNullOrWhiteSpace(physicalPath) && File.Exists(physicalPath))
            {
                var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken);
                return (bytes, fileName, "application/pdf");
            }

            EnsurePhysicalFilesCreated(issuance, issuance.WorkforceProfile, issuance.DirectorGeneralName ?? "Direktur Utama RS Quilvian", issuance.VerificationHash ?? "DRAFT");
            if (!string.IsNullOrWhiteSpace(physicalPath) && File.Exists(physicalPath))
            {
                var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken);
                return (bytes, fileName, "application/pdf");
            }

            var content = isRkk
                ? BuildRkkText(issuance, issuance.WorkforceProfile)
                : BuildSpkText(issuance, issuance.WorkforceProfile, issuance.DirectorGeneralName ?? "Direktur Utama RS Quilvian");
            var generatedBytes = GeneratePdfBytes(isRkk ? "RINCIAN KEWENANGAN KLINIS (RKK)" : "SURAT PENUGASAN KLINIS (SPK)", content);
            return (generatedBytes, fileName, "application/pdf");
        }

        /// <summary>
        /// BE-HRD-432: gambar QR yang benar-benar dapat dipindai, berisi alamat verifikasi publik
        /// <c>/api/v1/public/clinical-privileges/verify/{hash}</c> (diawali <c>FileStorage:PublicBaseUrl</c> bila diatur).
        /// </summary>
        private byte[] BuildVerificationQrPng(string verificationHash)
        {
            var baseUrl = _configuration?["FileStorage:PublicBaseUrl"]?.Trim().TrimEnd('/') ?? string.Empty;
            var payload = $"{baseUrl}{PublicVerifyPath}{Uri.EscapeDataString(verificationHash)}";
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
            using var code = new PngByteQRCode(data);
            return code.GetGraphic(QrPixelsPerModule, true);
        }

        private string GetPhysicalStorageRoot()
        {
            var configured = _configuration?["FileStorage:UploadRootPath"];
            var root = string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(_environment?.ContentRootPath ?? AppDomain.CurrentDomain.BaseDirectory, "uploads")
                : configured.Trim();

            if (!Path.IsPathRooted(root))
            {
                root = Path.Combine(_environment?.ContentRootPath ?? AppDomain.CurrentDomain.BaseDirectory, root);
            }

            return Path.GetFullPath(root);
        }

        private string ResolvePhysicalFilePath(string? relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl)) return string.Empty;
            var clean = relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var root = GetPhysicalStorageRoot();
            return Path.Combine(root, clean);
        }

        private void EnsurePhysicalFilesCreated(
            TrxSpkRkkIssuance issuance,
            QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models.MstWorkforceProfile? profile,
            string dirutName,
            string verificationHash,
            bool throwOnFailure = false)
        {
            try
            {
                var spkPath = ResolvePhysicalFilePath(issuance.SpkPdfFilePath);
                var rkkPath = ResolvePhysicalFilePath(issuance.RkkPdfFilePath);
                var qrPath = ResolvePhysicalFilePath(issuance.QrCodeImagePath);

                if (!string.IsNullOrWhiteSpace(spkPath))
                {
                    var dir = Path.GetDirectoryName(spkPath);
                    if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
                    var spkContent = BuildSpkText(issuance, profile, dirutName);
                    var spkBytes = GeneratePdfBytes("SURAT PENUGASAN KLINIS (SPK)", spkContent);
                    File.WriteAllBytes(spkPath, spkBytes);
                }

                if (!string.IsNullOrWhiteSpace(rkkPath))
                {
                    var dir = Path.GetDirectoryName(rkkPath);
                    if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
                    var rkkContent = BuildRkkText(issuance, profile);
                    var rkkBytes = GeneratePdfBytes("RINCIAN KEWENANGAN KLINIS (RKK)", rkkContent);
                    File.WriteAllBytes(rkkPath, rkkBytes);
                }

                if (!string.IsNullOrWhiteSpace(qrPath))
                {
                    var dir = Path.GetDirectoryName(qrPath);
                    if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllBytes(qrPath, BuildVerificationQrPng(verificationHash));
                }

                if (throwOnFailure)
                {
                    foreach (var path in new[] { spkPath, rkkPath, qrPath })
                    {
                        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path) || new FileInfo(path).Length == 0)
                        {
                            throw new IOException($"Berkas dokumen SPK-RKK tidak terbentuk: {Path.GetFileName(path)}");
                        }
                    }
                }
            }
            catch (Exception ex) when (!throwOnFailure && ex is IOException or UnauthorizedAccessException)
            {
                _logger.LogWarning(ex, "Gagal membuat file fisik PDF/QR untuk SPK-RKK #{SpkNumber}", issuance.SpkNumber);
            }
            catch (Exception ex) when (throwOnFailure && ex is IOException or UnauthorizedAccessException)
            {
                _logger.LogError(ex, "Gagal membuat file fisik PDF/QR untuk SPK-RKK #{SpkNumber}; pengesahan dibatalkan.", issuance.SpkNumber);
                throw new InvalidOperationException(
                    $"Dokumen SPK-RKK #{issuance.SpkNumber} gagal dibuat di penyimpanan, sehingga pengesahan dibatalkan dan kewenangan tidak diaktifkan. Hubungi admin sistem.");
            }
        }

        private static string BuildSpkText(
            TrxSpkRkkIssuance issuance,
            QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models.MstWorkforceProfile? profile,
            string dirutName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("================================================================================");
            sb.AppendLine("                     RUMAH SAKIT QUILVIAN - SURAT PENUGASAN KLINIS              ");
            sb.AppendLine("================================================================================");
            sb.AppendLine($"Nomor SPK         : {issuance.SpkNumber}");
            sb.AppendLine($"Nomor RKK Terkait : {issuance.RkkNumber}");
            sb.AppendLine($"Tanggal Terbit    : {issuance.IssuedAt:dd MMMM yyyy}");
            sb.AppendLine($"Masa Berlaku      : {issuance.ValidFrom:dd MMMM yyyy} s.d. {issuance.ValidUntil:dd MMMM yyyy} (Siklus 3 Tahun)");
            sb.AppendLine($"Nama Tenaga Kerja : {profile?.DisplayName ?? "-"} ({profile?.ProfileCode ?? "-"})");
            sb.AppendLine($"Jabatan / Unit    : {profile?.PrimaryPosition?.PositionName ?? "Tenaga Medis"} / {profile?.PrimaryDepartment?.DepartmentName ?? "Pelayanan Medis"}");
            sb.AppendLine($"Jenis Wewenang    : {issuance.ClinicalPrivilegeType}");
            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine("KEPUTUSAN DIREKTUR UTAMA:");
            sb.AppendLine("Memberikan penugasan klinis kepada tenaga kesehatan tersebut di atas untuk");
            sb.AppendLine("melakukan tindakan medis di lingkungan Rumah Sakit Quilvian sesuai dengan");
            sb.AppendLine("Rincian Kewenangan Klinis (RKK) terlampir.");
            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine($"Disahkan Secara Sah : {issuance.SignedAt:dd MMMM yyyy HH:mm:ss} UTC");
            sb.AppendLine($"Direktur Utama      : {dirutName}");
            sb.AppendLine($"Digital Hash (QR)   : {issuance.VerificationHash}");
            sb.AppendLine("================================================================================");
            return sb.ToString();
        }

        private static string BuildRkkText(
            TrxSpkRkkIssuance issuance,
            QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models.MstWorkforceProfile? profile)
        {
            var sb = new StringBuilder();
            sb.AppendLine("================================================================================");
            sb.AppendLine("                 RUMAH SAKIT QUILVIAN - RINCIAN KEWENANGAN KLINIS (RKK)         ");
            sb.AppendLine("================================================================================");
            sb.AppendLine($"Nomor RKK         : {issuance.RkkNumber}");
            sb.AppendLine($"Mengacu SPK Nomor : {issuance.SpkNumber}");
            sb.AppendLine($"Nama Tenaga Kerja : {profile?.DisplayName ?? "-"} ({profile?.ProfileCode ?? "-"})");
            sb.AppendLine($"Masa Berlaku      : {issuance.ValidFrom:dd MMMM yyyy} s.d. {issuance.ValidUntil:dd MMMM yyyy}");
            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine("DAFTAR RINCIAN TINDAKAN DAN LEVEL SUPERVISI YANG DISETUJUI:");
            sb.AppendLine("--------------------------------------------------------------------------------");

            if (!string.IsNullOrWhiteSpace(issuance.ApprovedPrivilegesSnapshotJson))
            {
                try
                {
                    var privs = JsonSerializer.Deserialize<List<PrivilegeAssessmentItemDto>>(issuance.ApprovedPrivilegesSnapshotJson);
                    if (privs != null && privs.Count > 0)
                    {
                        var idx = 1;
                        foreach (var item in privs)
                        {
                            var levelDesc = item.RecommendedSupervisionLevel switch
                            {
                                SupervisionLevel.Level1_Independent => "Mandiri (Level 1)",
                                SupervisionLevel.Level2_UnderSupervision => "Di Bawah Supervisi (Level 2)",
                                SupervisionLevel.Level3_Proctored => "Proctored/Didampingi (Level 3)",
                                SupervisionLevel.Level4_NotPermitted => "Tidak Diizinkan (Level 4)",
                                _ => item.RecommendedSupervisionLevel.ToString()
                            };
                            var code = !string.IsNullOrWhiteSpace(item.PrivilegeCode) ? item.PrivilegeCode : item.CatalogId.ToString();
                            var name = !string.IsNullOrWhiteSpace(item.PrivilegeName) ? $" {item.PrivilegeName}" : "";
                            sb.AppendLine($"{idx}. [{code}]{name} - Level: {levelDesc}");
                            idx++;
                        }
                    }
                    else
                    {
                        sb.AppendLine("(Tidak ada butir tindakan spesifik)");
                    }
                }
                catch
                {
                    sb.AppendLine("(Gagal memuat rincian kewenangan snapshot)");
                }
            }
            sb.AppendLine("================================================================================");
            return sb.ToString();
        }

        private async Task<(int Year, string Sequence)> AllocateSpkSequenceAsync(Guid actorUserId, CancellationToken cancellationToken)
        {
            var year = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ResolveBusinessTimeZone()).Year;
            var yearMarker = $"/{year}/";

            // Batas bawah = urutan tertinggi tahun berjalan yang sudah terbit lewat Count+1 sebelum perubahan ini.
            var existing = await _context.TrxSpkRkkIssuances
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(s => s.SpkNumber.Contains(yearMarker))
                .Select(s => s.SpkNumber)
                .ToListAsync(cancellationToken);
            var watermark = existing
                .Select(n => long.TryParse(n[(n.LastIndexOf('/') + 1)..], NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0L)
                .DefaultIfEmpty(0L)
                .Max();

            var allocated = await _numberSeriesAllocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: SpkSequenceKey,
                    Prefix: SpkSeriesPrefix,
                    ResetPolicy: NumberSeriesResetPolicies.Yearly,
                    SequenceDigits: SpkSequenceDigits,
                    ActorUserId: actorUserId,
                    Instant: DateTimeOffset.UtcNow,
                    MinimumValue: watermark),
                cancellationToken);

            // Bentuk keluaran: SPK-RKK-{yyyy}-{NNNN}; segmen terakhir adalah urutan.
            return (year, allocated[(allocated.LastIndexOf('-') + 1)..]);
        }

        private async Task AcquireTransactionLockAsync(string lockKey, CancellationToken cancellationToken)
        {
            if (!_context.Database.IsNpgsql())
            {
                return;
            }

            await _context.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [lockKey],
                cancellationToken);
        }

        private static TimeZoneInfo ResolveBusinessTimeZone()
        {
            foreach (var id in new[] { "Asia/Jakarta", "SE Asia Standard Time" })
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(id);
                }
                catch (TimeZoneNotFoundException)
                {
                }
                catch (InvalidTimeZoneException)
                {
                }
            }

            return TimeZoneInfo.CreateCustomTimeZone("WIB", TimeSpan.FromHours(7), "WIB", "WIB");
        }

        private static byte[] GeneratePdfBytes(string title, string content)
        {
            var sb = new StringBuilder();
            sb.Append("%PDF-1.4\n");

            var obj1Offset = sb.Length;
            sb.Append("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

            var obj2Offset = sb.Length;
            sb.Append("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");

            var escapedContent = content
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)");

            var lines = escapedContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var streamBuilder = new StringBuilder();
            streamBuilder.Append("BT\n/F1 10 Tf\n40 780 Td\n14 TL\n");
            streamBuilder.Append($"({title}) Tj T*\nT*\n");
            foreach (var line in lines)
            {
                var truncated = line.Length > 90 ? line.Substring(0, 90) : line;
                streamBuilder.Append($"({truncated}) Tj T*\n");
            }
            streamBuilder.Append("ET\n");
            var streamBytes = Encoding.ASCII.GetBytes(streamBuilder.ToString());

            var obj3Offset = sb.Length;
            sb.Append($"3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>\nendobj\n");

            var obj4Offset = sb.Length;
            sb.Append($"4 0 obj\n<< /Length {streamBytes.Length} >>\nstream\n");
            sb.Append(streamBuilder);
            sb.Append("endstream\nendobj\n");

            var obj5Offset = sb.Length;
            sb.Append("5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n");

            var xrefOffset = sb.Length;
            sb.Append("xref\n0 6\n0000000000 65535 f \n");
            sb.Append($"{obj1Offset:D10} 00000 n \n");
            sb.Append($"{obj2Offset:D10} 00000 n \n");
            sb.Append($"{obj3Offset:D10} 00000 n \n");
            sb.Append($"{obj4Offset:D10} 00000 n \n");
            sb.Append($"{obj5Offset:D10} 00000 n \n");

            sb.Append($"trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF\n");

            return Encoding.ASCII.GetBytes(sb.ToString());
        }

        private static string GenerateSha256(string raw)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private static string GetRomanMonth(int month)
        {
            return month switch
            {
                1 => "I",
                2 => "II",
                3 => "III",
                4 => "IV",
                5 => "V",
                6 => "VI",
                7 => "VII",
                8 => "VIII",
                9 => "IX",
                10 => "X",
                11 => "XI",
                12 => "XII",
                _ => "I"
            };
        }

        private static SpkRkkDetailDto MapToDto(
            TrxSpkRkkIssuance issuance,
            QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models.MstWorkforceProfile? profile)
        {
            var privileges = new List<PrivilegeAssessmentItemDto>();
            if (!string.IsNullOrWhiteSpace(issuance.ApprovedPrivilegesSnapshotJson))
            {
                try
                {
                    privileges = JsonSerializer.Deserialize<List<PrivilegeAssessmentItemDto>>(issuance.ApprovedPrivilegesSnapshotJson) ?? new List<PrivilegeAssessmentItemDto>();
                }
                catch
                {
                    privileges = new List<PrivilegeAssessmentItemDto>();
                }
            }

            return new SpkRkkDetailDto
            {
                Id = issuance.Id,
                WorkforceProfileId = issuance.WorkforceProfileId,
                EmployeeName = profile?.DisplayName ?? "-",
                EmployeeCode = profile?.ProfileCode ?? "-",
                Profession = profile?.PrimaryPosition?.PositionName ?? "Tenaga Medis",
                CommitteeName = "Komite Kredensial Rumah Sakit",
                SpkNumber = issuance.SpkNumber,
                RkkNumber = issuance.RkkNumber,
                IssuedAt = issuance.IssuedAt,
                ValidFrom = issuance.ValidFrom,
                ValidUntil = issuance.ValidUntil,
                ClinicalPrivilegeType = issuance.ClinicalPrivilegeType,
                DirectorGeneralName = issuance.DirectorGeneralName,
                IsSignedByDirectorGeneral = issuance.IsSignedByDirectorGeneral,
                SignedAt = issuance.SignedAt,
                VerificationHash = issuance.VerificationHash,
                SpkPdfUrl = issuance.SpkPdfFilePath,
                QrCodeUrl = issuance.QrCodeImagePath,
                EDossierDocumentId = issuance.EDossierDocumentId,
                IssuanceStatus = issuance.IssuanceStatus,
                ApprovedPrivileges = privileges
            };
        }
    }
}
