using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services
{
    public class CredentialVerificationService : ICredentialVerificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CredentialVerificationService> _logger;

        public CredentialVerificationService(
            ApplicationDbContext context,
            ILogger<CredentialVerificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ProfessionalLicenseDto> RegisterLicenseAsync(
            RegisterProfessionalLicenseDto request,
            Guid registeredByUserId,
            CancellationToken cancellationToken = default)
        {
            var profile = await _context.MstWorkforceProfiles
                .Include(p => p.PrimaryDepartment)
                .Include(p => p.PrimaryPosition)
                .FirstOrDefaultAsync(p => p.Id == request.WorkforceProfileId, cancellationToken);

            if (profile == null)
            {
                throw new KeyNotFoundException($"Profil tenaga kerja dengan ID {request.WorkforceProfileId} tidak ditemukan.");
            }

            var isStr = request.LicenseType.Equals("STR", StringComparison.OrdinalIgnoreCase);
            var isSip = request.LicenseType.Equals("SIP", StringComparison.OrdinalIgnoreCase);

            // Tentukan tanggal kedaluwarsa untuk STR Seumur Hidup
            DateTime expiredDate;
            if (isStr && request.IsLifetimeStr)
            {
                expiredDate = DateTime.SpecifyKind(new DateTime(2099, 12, 31, 23, 59, 59), DateTimeKind.Utc);
            }
            else
            {
                if (!request.ExpiredDate.HasValue)
                {
                    throw new InvalidOperationException("Tanggal kedaluwarsa wajib diisi untuk lisensi non-seumur hidup.");
                }
                expiredDate = DateTime.SpecifyKind(request.ExpiredDate.Value, DateTimeKind.Utc);
            }

            var issueDate = DateTime.SpecifyKind(request.IssueDate, DateTimeKind.Utc);

            // Hitung status radar awal
            var daysRemaining = (expiredDate.Date - DateTime.UtcNow.Date).Days;
            var radarStatus = CalculateRadarStatus(daysRemaining, request.IsLifetimeStr);

            var license = new WfpCredentialLicense
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = request.WorkforceProfileId,
                LicenseType = request.LicenseType.ToUpperInvariant(),
                LicenseNumber = request.LicenseNumber.Trim(),
                Issuer = request.Issuer.Trim(),
                PracticeLocation = request.PracticeLocation ?? "Rumah Sakit Quilvian Utama",
                IssueDate = issueDate,
                ExpiredDate = expiredDate,
                IsLifetimeStr = isStr && request.IsLifetimeStr,
                IsPsvVerified = false,
                PsvVerifiedAt = null,
                PsvVerificationSource = null,
                PsvVerificationUrl = null,
                PsvAuditLogJson = null,
                SipFaskesPracticeNumber = isSip ? (request.SipFaskesPracticeNumber ?? "SIP-1") : null,
                SipFaskesName = isSip ? (request.SipFaskesName ?? "Rumah Sakit Quilvian Utama") : null,
                ExpiryRadarStatus = radarStatus,
                VerificationStatus = CredentialVerificationStatus.PendingVerification,
                IsPrimary = isSip && (request.SipFaskesPracticeNumber == "SIP-1" || string.IsNullOrEmpty(request.SipFaskesPracticeNumber)),
                IsVerified = false,
                VerifiedAt = null,
                VerifiedByUserId = null,
                VerificationNotes = null,
                IsRevoked = false,
                RevokedAt = null,
                RevokedByUserId = null,
                RevocationReason = null,
                BlocksSchedulingWhenInvalid = true,
                BlocksClinicalServiceWhenInvalid = true,
                Description = request.Description,
                IsActive = true,
                CreateBy = registeredByUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _context.WfpCredentialLicenses.Add(license);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Lisensi {LicenseType} #{LicenseNumber} untuk tenaga kerja {ProfileId} berhasil didaftarkan. PSV Pending.",
                license.LicenseType, license.LicenseNumber, profile.Id);

            return MapToDto(license, profile);
        }

        public async Task<PsvVerificationResultDto> PerformPrimarySourceVerificationAsync(
            PsvVerificationRequestDto request,
            Guid auditorUserId,
            CancellationToken cancellationToken = default)
        {
            var license = await _context.WfpCredentialLicenses
                .Include(l => l.WorkforceProfile)
                .FirstOrDefaultAsync(l => l.Id == request.LicenseId, cancellationToken);

            if (license == null)
            {
                throw new KeyNotFoundException($"Lisensi dengan ID {request.LicenseId} tidak ditemukan.");
            }

            var auditorUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == auditorUserId, cancellationToken);

            var auditorName = auditorUser?.UserName ?? auditorUserId.ToString();

            var auditLog = new
            {
                AuditorUserId = auditorUserId,
                AuditorName = auditorName,
                VerifiedAtUtc = DateTime.UtcNow,
                Source = request.PsvVerificationSource,
                Url = request.PsvVerificationUrl,
                IsValidOnSource = request.IsValidOnSource,
                Notes = request.AuditorNotes
            };

            license.PsvAuditLogJson = JsonSerializer.Serialize(auditLog);
            license.PsvVerificationSource = request.PsvVerificationSource;
            license.PsvVerificationUrl = request.PsvVerificationUrl;
            license.UpdateBy = auditorUserId;
            license.UpdateDateTime = DateTime.UtcNow;

            if (request.IsValidOnSource)
            {
                license.IsPsvVerified = true;
                license.PsvVerifiedAt = DateTime.UtcNow;
                license.VerificationStatus = CredentialVerificationStatus.Verified;
                license.IsVerified = true;
                license.VerifiedAt = DateTime.UtcNow;
                license.VerifiedByUserId = auditorUserId;
                license.VerificationNotes = $"PSV diverifikasi valid via {request.PsvVerificationSource}. Catatan: {request.AuditorNotes}";

                _logger.LogInformation(
                    "Audit PSV SUKSES untuk Lisensi {LicenseType} #{LicenseNumber} via {Source} oleh auditor {Auditor}",
                    license.LicenseType, license.LicenseNumber, request.PsvVerificationSource, auditorName);
            }
            else
            {
                license.IsPsvVerified = false;
                license.VerificationStatus = CredentialVerificationStatus.Rejected;
                license.IsVerified = false;
                license.VerificationNotes = $"PSV TIDAK VALID pada {request.PsvVerificationSource}. Alasan: {request.AuditorNotes}";

                _logger.LogWarning(
                    "Audit PSV GAGAL / TIDAK VALID untuk Lisensi {LicenseType} #{LicenseNumber} via {Source} oleh auditor {Auditor}",
                    license.LicenseType, license.LicenseNumber, request.PsvVerificationSource, auditorName);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new PsvVerificationResultDto
            {
                IsSuccess = request.IsValidOnSource,
                Message = request.IsValidOnSource
                    ? "Verifikasi Sumber Primer (PSV) berhasil diverifikasi valid. Dokumen siap diajukan ke komite kredensial."
                    : "Verifikasi Sumber Primer (PSV) ditolak. Dokumen lisensi tidak terdaftar atau tidak sah di portal resmi.",
                LicenseId = license.Id,
                LicenseNumber = license.LicenseNumber,
                LicenseType = license.LicenseType,
                IsPsvVerified = license.IsPsvVerified,
                PsvVerifiedAt = license.PsvVerifiedAt,
                PsvVerificationSource = license.PsvVerificationSource,
                PsvVerificationUrl = license.PsvVerificationUrl,
                AuditorUserName = auditorName
            };
        }

        public async Task<ProfessionalLicenseDto> GetLicenseDetailAsync(
            Guid licenseId,
            CancellationToken cancellationToken = default)
        {
            var license = await _context.WfpCredentialLicenses
                .Include(l => l.WorkforceProfile)
                    .ThenInclude(p => p!.PrimaryDepartment)
                .Include(l => l.WorkforceProfile)
                    .ThenInclude(p => p!.PrimaryPosition)
                .FirstOrDefaultAsync(l => l.Id == licenseId, cancellationToken);

            if (license == null)
            {
                throw new KeyNotFoundException($"Lisensi dengan ID {licenseId} tidak ditemukan.");
            }

            return MapToDto(license, license.WorkforceProfile);
        }

        public async Task<List<ProfessionalLicenseDto>> GetLicensesByEmployeeAsync(
            Guid workforceProfileId,
            CancellationToken cancellationToken = default)
        {
            var profile = await _context.MstWorkforceProfiles
                .Include(p => p.PrimaryDepartment)
                .Include(p => p.PrimaryPosition)
                .FirstOrDefaultAsync(p => p.Id == workforceProfileId, cancellationToken);

            if (profile == null)
            {
                throw new KeyNotFoundException($"Profil tenaga kerja {workforceProfileId} tidak ditemukan.");
            }

            var licenses = await _context.WfpCredentialLicenses
                .Where(l => l.WorkforceProfileId == workforceProfileId)
                .OrderByDescending(l => l.CreateDateTime)
                .ToListAsync(cancellationToken);

            return licenses.Select(l => MapToDto(l, profile)).ToList();
        }

        public async Task<ProfessionalLicenseDto> RenewLicenseAsync(
            Guid licenseId,
            RenewProfessionalLicenseDto request,
            Guid requestedByUserId,
            CancellationToken cancellationToken = default)
        {
            var license = await _context.WfpCredentialLicenses
                .Include(l => l.WorkforceProfile)
                    .ThenInclude(p => p!.PrimaryDepartment)
                .Include(l => l.WorkforceProfile)
                    .ThenInclude(p => p!.PrimaryPosition)
                .FirstOrDefaultAsync(l => l.Id == licenseId, cancellationToken);

            if (license == null)
            {
                throw new KeyNotFoundException($"Lisensi dengan ID {licenseId} tidak ditemukan.");
            }

            if (license.IsLifetimeStr)
            {
                throw new InvalidOperationException("STR Seumur Hidup tidak memerlukan perpanjangan berkala.");
            }

            var newExpiredDate = DateTime.SpecifyKind(request.NewExpiredDate, DateTimeKind.Utc);
            var newIssueDate = DateTime.SpecifyKind(request.NewIssueDate, DateTimeKind.Utc);

            if (newExpiredDate <= newIssueDate)
            {
                throw new InvalidOperationException("Tanggal kedaluwarsa lisensi baru harus lebih besar dari tanggal terbit.");
            }

            var daysRemaining = (newExpiredDate.Date - DateTime.UtcNow.Date).Days;

            license.LicenseNumber = request.NewLicenseNumber.Trim();
            license.IssueDate = newIssueDate;
            license.ExpiredDate = newExpiredDate;
            if (!string.IsNullOrWhiteSpace(request.NewIssuer))
            {
                license.Issuer = request.NewIssuer.Trim();
            }

            // Setelah diperpanjang, status PSV di-reset agar diverifikasi kembali ke portal KKI/Kemenkes
            license.IsPsvVerified = false;
            license.PsvVerifiedAt = null;
            license.VerificationStatus = CredentialVerificationStatus.PendingVerification;
            license.IsVerified = false;
            license.ExpiryRadarStatus = CalculateRadarStatus(daysRemaining, false);
            license.Description = $"{license.Description} | Perpanjangan: {request.RenewalNotes}".Trim();
            license.UpdateBy = requestedByUserId;
            license.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Lisensi #{LicenseNumber} berhasil diperpanjang hingga {ExpiredDate}. PSV status di-reset ke Pending.",
                license.LicenseNumber, license.ExpiredDate);

            return MapToDto(license, license.WorkforceProfile);
        }

        public async Task<List<LicenseRadarAlertDto>> GetLicenseExpiringRadarListAsync(
            string? radarLevel = null,
            CancellationToken cancellationToken = default)
        {
            var today = DateTime.UtcNow.Date;
            var maxThresholdDate = today.AddDays(90);

            var query = _context.WfpCredentialLicenses
                .Include(l => l.WorkforceProfile)
                    .ThenInclude(p => p!.PrimaryDepartment)
                .Include(l => l.WorkforceProfile)
                    .ThenInclude(p => p!.PrimaryPosition)
                .Where(l => l.IsActive && !l.IsRevoked && !l.IsLifetimeStr && l.ExpiredDate <= maxThresholdDate);

            var licenses = await query.ToListAsync(cancellationToken);

            var alerts = licenses.Select(l =>
            {
                var daysRemaining = (l.ExpiredDate.Date - today).Days;
                var currentRadar = CalculateRadarStatus(daysRemaining, false);
                var (actionReq, simrsBlocked, schedBlocked) = GetActionDetails(currentRadar, daysRemaining);

                return new LicenseRadarAlertDto
                {
                    LicenseId = l.Id,
                    WorkforceProfileId = l.WorkforceProfileId,
                    EmployeeName = l.WorkforceProfile?.DisplayName ?? "Nakes Tanpa Nama",
                    EmployeeCode = l.WorkforceProfile?.ProfileCode ?? "-",
                    Profession = l.WorkforceProfile?.PrimaryPosition?.PositionName ?? l.LicenseType,
                    DepartmentName = l.WorkforceProfile?.PrimaryDepartment?.DepartmentName ?? "Unit Rawat",
                    LicenseType = l.LicenseType,
                    LicenseNumber = l.LicenseNumber,
                    ExpiredDate = l.ExpiredDate,
                    DaysRemaining = daysRemaining,
                    RadarLevel = currentRadar,
                    ActionRequired = actionReq,
                    IsSimrsBlocked = simrsBlocked,
                    IsSchedulingBlocked = schedBlocked
                };
            });

            if (!string.IsNullOrWhiteSpace(radarLevel))
            {
                alerts = alerts.Where(a => a.RadarLevel.Equals(radarLevel, StringComparison.OrdinalIgnoreCase));
            }

            return alerts.OrderBy(a => a.DaysRemaining).ToList();
        }

        public async Task<int> ExecuteFailClosedForExpiredLicensesAsync(
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            // Cari lisensi aktif non-seumur-hidup yang tanggal kedaluwarsanya sudah lewat dan belum berstatus ExpiredH0
            var expiredLicenses = await _context.WfpCredentialLicenses
                .Include(l => l.WorkforceProfile)
                .Where(l => l.IsActive && !l.IsRevoked && !l.IsLifetimeStr && l.ExpiredDate < now && l.ExpiryRadarStatus != "ExpiredH0")
                .ToListAsync(cancellationToken);

            if (!expiredLicenses.Any())
            {
                return 0;
            }

            var affectedProfileIds = expiredLicenses.Select(l => l.WorkforceProfileId).Distinct().ToList();

            foreach (var license in expiredLicenses)
            {
                license.ExpiryRadarStatus = "ExpiredH0";
                license.BlocksClinicalServiceWhenInvalid = true;
                license.BlocksSchedulingWhenInvalid = true;
                license.UpdateDateTime = now;
                license.UpdateBy = Guid.Empty;

                _logger.LogWarning(
                    "FAIL-CLOSED: Lisensi {LicenseType} #{LicenseNumber} milik nakes {ProfileId} kedaluwarsa pada {ExpiredDate}. Blokir SIMRS & Roster aktif.",
                    license.LicenseType, license.LicenseNumber, license.WorkforceProfileId, license.ExpiredDate);
            }

            // Tangguhkan kewenangan klinis nakes terkait di WfpClinicalPrivilege jika SIP utama habis
            var clinicalPrivileges = await _context.WfpClinicalPrivileges
                .Where(cp => affectedProfileIds.Contains(cp.WorkforceProfileId) && cp.PrivilegeStatus == ClinicalPrivilegeStatus.Active)
                .ToListAsync(cancellationToken);

            foreach (var cp in clinicalPrivileges)
            {
                cp.PrivilegeStatus = ClinicalPrivilegeStatus.Suspended;
                cp.IsClinicalServiceBlocked = true;
                cp.IsSchedulingBlocked = true;
                cp.SuspensionReason = "Fail-Closed Otomatis: Masa berlaku SIP tenaga medis/kesehatan telah habis (H-0).";
                cp.SuspendedAt = now;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Eksekusi Fail-Closed berhasil memproses {Count} lisensi kedaluwarsa dan menangguhkan kewenangan klinis terkait.",
                expiredLicenses.Count);

            return expiredLicenses.Count;
        }

        private static string CalculateRadarStatus(int daysRemaining, bool isLifetime)
        {
            if (isLifetime) return "Normal";
            if (daysRemaining <= 0) return "ExpiredH0";
            if (daysRemaining <= 7) return "H7";
            if (daysRemaining <= 30) return "H30";
            if (daysRemaining <= 60) return "H60";
            if (daysRemaining <= 90) return "H90";
            return "Normal";
        }

        private static (string ActionRequired, bool SimrsBlocked, bool SchedBlocked) GetActionDetails(string radarLevel, int daysRemaining)
        {
            return radarLevel switch
            {
                "ExpiredH0" => (
                    "FAIL-CLOSED: EMR Order SIMRS & Jadwal Dinas telah diblokir. Nakes dilarang memberikan layanan klinis sampai SIP baru disahkan.",
                    true, true),
                "H7" => (
                    $"DARURAT (H-{daysRemaining}): Eskalasi langsung ke Direktur Pelayanan Medik & HRD. SIP wajib terbit dalam 7 hari.",
                    false, false),
                "H30" => (
                    $"KRITIS (H-{daysRemaining}): Peringatan Sidang Komite Kredensial. Berkas perpanjangan SIP harus sudah masuk rekomendasi SPK-RKK.",
                    false, false),
                "H60" => (
                    $"PERINGATAN UNIT (H-{daysRemaining}): Notifikasi ke Kepala Ruang & Unit Pelayanan. SKP dan kelengkapan PTSP harus segera diajukan.",
                    false, false),
                "H90" => (
                    $"PERINGATAN AWAL ESS (H-{daysRemaining}): Notifikasi ke portal mandiri Nakes untuk menyiapkan perpanjangan SIP 5 tahun.",
                    false, false),
                _ => ("Masa berlaku lisensi aman (> 90 hari).", false, false)
            };
        }

        private static ProfessionalLicenseDto MapToDto(WfpCredentialLicense license, QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models.MstWorkforceProfile? profile)
        {
            var daysRemaining = license.IsLifetimeStr ? 99999 : (license.ExpiredDate.Date - DateTime.UtcNow.Date).Days;

            return new ProfessionalLicenseDto
            {
                Id = license.Id,
                WorkforceProfileId = license.WorkforceProfileId,
                EmployeeName = profile?.DisplayName ?? "-",
                EmployeeNumber = profile?.ProfileCode ?? "-",
                ProfessionName = profile?.PrimaryPosition?.PositionName ?? license.LicenseType,
                LicenseType = license.LicenseType,
                LicenseNumber = license.LicenseNumber,
                Issuer = license.Issuer,
                PracticeLocation = license.PracticeLocation,
                IssueDate = license.IssueDate,
                ExpiredDate = license.ExpiredDate,
                IsLifetimeStr = license.IsLifetimeStr,
                IsPsvVerified = license.IsPsvVerified,
                PsvVerifiedAt = license.PsvVerifiedAt,
                PsvVerificationSource = license.PsvVerificationSource,
                PsvVerificationUrl = license.PsvVerificationUrl,
                SipFaskesPracticeNumber = license.SipFaskesPracticeNumber,
                SipFaskesName = license.SipFaskesName,
                ExpiryRadarStatus = CalculateRadarStatus(daysRemaining, license.IsLifetimeStr),
                DaysRemaining = daysRemaining,
                BlocksClinicalServiceWhenInvalid = license.BlocksClinicalServiceWhenInvalid,
                BlocksSchedulingWhenInvalid = license.BlocksSchedulingWhenInvalid,
                IsRevoked = license.IsRevoked,
                RevocationReason = license.RevocationReason,
                IsActive = license.IsActive
            };
        }
    }
}
