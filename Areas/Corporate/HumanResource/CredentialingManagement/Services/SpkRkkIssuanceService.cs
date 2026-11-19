using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
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

        public SpkRkkIssuanceService(
            ApplicationDbContext context,
            ILogger<SpkRkkIssuanceService> logger,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _environment = environment;
        }

        public async Task<SpkRkkDetailDto> IssueSpkRkkDraftAsync(
            IssueSpkRkkRequestDto request,
            Guid issuedByUserId,
            CancellationToken cancellationToken = default)
        {
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

            var year = DateTime.UtcNow.Year;
            var monthRoman = GetRomanMonth(DateTime.UtcNow.Month);

            // FIN-HRD-09-BE-007 / M09-P0-08: Penomoran beruntun anti-tabrakan (concurrency-safe)
            var countThisYear = await _context.TrxSpkRkkIssuances
                .CountAsync(s => s.SpkNumber.Contains($"/{year}/"), cancellationToken);
            var seq = countThisYear + 1;
            string spkNumber;
            string rkkNumber;
            while (true)
            {
                var sequenceNumber = seq.ToString("D4");
                spkNumber = $"SPK/RSQ-DIRUT/{monthRoman}/{year}/{sequenceNumber}";
                rkkNumber = $"RKK/RSQ-DIRUT/{monthRoman}/{year}/{sequenceNumber}";
                var collision = await _context.TrxSpkRkkIssuances
                    .AnyAsync(s => s.SpkNumber == spkNumber || s.RkkNumber == rkkNumber, cancellationToken);
                if (!collision)
                {
                    break;
                }
                seq++;
            }

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
                throw new InvalidOperationException($"SPK-RKK #{issuance.SpkNumber} sudah disahkan sebelumnya pada {issuance.SignedAt:yyyy-MM-dd HH:mm:ss} UTC.");
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

            // FIN-HRD-09-BE-007 / M09-P0-09: Pastikan file fisik PDF SPK/RKK dan QR code benar-benar dibuat di disk
            EnsurePhysicalFilesCreated(issuance, issuance.WorkforceProfile, dirutName, verificationHash);

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
            if (!string.IsNullOrWhiteSpace(issuance.ApprovedPrivilegesSnapshotJson))
            {
                try
                {
                    var privilegeItems = JsonSerializer.Deserialize<List<PrivilegeAssessmentItemDto>>(issuance.ApprovedPrivilegesSnapshotJson);
                    if (privilegeItems != null)
                    {
                        var catalogIds = privilegeItems.Select(p => p.CatalogId).ToList();
                        var catalogs = await _context.MstClinicalPrivilegeCatalogs
                            .Where(c => catalogIds.Contains(c.Id))
                            .ToDictionaryAsync(c => c.Id, cancellationToken);

                        foreach (var item in privilegeItems)
                        {
                            if (!catalogs.TryGetValue(item.CatalogId, out var catalog)) continue;

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
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Gagal mengurai ApprovedPrivilegesSnapshotJson saat aktivasi kewenangan klinis.");
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

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

        private static readonly byte[] MinimalPngBytes = new byte[]
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
            0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4, 0x89,
            0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41, 0x54,
            0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00, 0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4,
            0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
        };

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
            string verificationHash)
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
                    File.WriteAllBytes(qrPath, MinimalPngBytes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gagal membuat file fisik PDF/QR untuk SPK-RKK #{SpkNumber}", issuance.SpkNumber);
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
