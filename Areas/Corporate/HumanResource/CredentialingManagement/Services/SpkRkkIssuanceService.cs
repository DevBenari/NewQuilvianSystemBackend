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

        public SpkRkkIssuanceService(
            ApplicationDbContext context,
            ILogger<SpkRkkIssuanceService> logger)
        {
            _context = context;
            _logger = logger;
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

            var year = DateTime.UtcNow.Year;
            var monthRoman = GetRomanMonth(DateTime.UtcNow.Month);

            var countThisYear = await _context.TrxSpkRkkIssuances
                .CountAsync(s => s.SpkNumber.Contains($"/{year}/"), cancellationToken);
            var sequenceNumber = (countThisYear + 1).ToString("D4");

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
