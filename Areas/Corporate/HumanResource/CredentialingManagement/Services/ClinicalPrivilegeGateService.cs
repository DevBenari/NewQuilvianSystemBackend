using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services
{
    public class ClinicalPrivilegeGateService : IClinicalPrivilegeGateService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClinicalPrivilegeGateService> _logger;

        public ClinicalPrivilegeGateService(
            ApplicationDbContext context,
            ILogger<ClinicalPrivilegeGateService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ClinicalPrivilegeGateValidationResultDto> ValidateClinicalGateAsync(
            ClinicalPrivilegeGateValidationRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var profile = await _context.MstWorkforceProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.WorkforceProfileId, cancellationToken);

            if (profile == null)
            {
                return new ClinicalPrivilegeGateValidationResultDto
                {
                    IsAllowed = false,
                    GateDecision = "HardStopBlocked",
                    SupervisionLevel = "Level4_NotPermitted",
                    RequiresCoSignature = false,
                    Message = "Profil tenaga medis/kesehatan tidak ditemukan dalam master pegawai.",
                    LicenseRadarStatus = "Normal",
                    IsLifeSavingOverrideAvailable = false
                };
            }

            var now = DateTime.UtcNow;

            // 1. Cek masa berlaku SIP faskes & status Fail-Closed Radar (INT-HR-71, CRD-B2)
            var activeSip = await _context.WfpCredentialLicenses
                .AsNoTracking()
                .Where(l => l.WorkforceProfileId == request.WorkforceProfileId && l.IsActive && !l.IsRevoked)
                .OrderByDescending(l => l.ExpiredDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (activeSip == null)
            {
                return new ClinicalPrivilegeGateValidationResultDto
                {
                    IsAllowed = false,
                    GateDecision = "ExpiredLicenseBlocked",
                    SupervisionLevel = "Level4_NotPermitted",
                    RequiresCoSignature = false,
                    Message = "Akses ditolak (Fail-Closed). Tenaga kerja tidak memiliki Surat Izin Praktik (SIP) aktif di rumah sakit ini.",
                    LicenseRadarStatus = "ExpiredH0",
                    IsLifeSavingOverrideAvailable = true
                };
            }

            var daysRemaining = activeSip.IsLifetimeStr ? 99999 : (activeSip.ExpiredDate.Date - now.Date).Days;
            if (!activeSip.IsLifetimeStr && (daysRemaining <= 0 || activeSip.ExpiryRadarStatus == "ExpiredH0"))
            {
                return new ClinicalPrivilegeGateValidationResultDto
                {
                    IsAllowed = false,
                    GateDecision = "ExpiredLicenseBlocked",
                    SupervisionLevel = "Level4_NotPermitted",
                    RequiresCoSignature = false,
                    Message = $"Akses layanan klinis diblokir (Fail-Closed H-0). Masa berlaku SIP dokter ({activeSip.LicenseNumber}) telah habis. Segera perpanjang SIP.",
                    LicenseRadarStatus = "ExpiredH0",
                    IsLifeSavingOverrideAvailable = true
                };
            }

            // 2. Cek kewenangan klinis di WfpClinicalPrivilege (INT-HR-69, HRD-DEC-005)
            var privilege = await _context.WfpClinicalPrivileges
                .Include(cp => cp.SupervisorUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(cp => cp.WorkforceProfileId == request.WorkforceProfileId &&
                                           cp.PrivilegeCode.Equals(request.PrivilegeCode, StringComparison.OrdinalIgnoreCase) &&
                                           cp.IsActive, cancellationToken);

            if (privilege == null)
            {
                // HARD-STOP BLOCKED: Tidak memiliki kewenangan klinis dalam SPK-RKK
                return new ClinicalPrivilegeGateValidationResultDto
                {
                    IsAllowed = false,
                    GateDecision = "HardStopBlocked",
                    SupervisionLevel = "Level4_NotPermitted",
                    RequiresCoSignature = false,
                    Message = $"Tindakan medis '{request.PrivilegeCode}' DITOLAK (Hard-Stop). Dokter tidak memiliki kewenangan klinis untuk tindakan ini dalam SPK-RKK yang sah.",
                    LicenseRadarStatus = activeSip.ExpiryRadarStatus,
                    IsLifeSavingOverrideAvailable = true
                };
            }

            // Jika kewenangan klinis berstatus ditangguhkan / kedaluwarsa
            if (privilege.PrivilegeStatus != ClinicalPrivilegeStatus.Active || privilege.IsClinicalServiceBlocked)
            {
                return new ClinicalPrivilegeGateValidationResultDto
                {
                    IsAllowed = false,
                    GateDecision = "HardStopBlocked",
                    SupervisionLevel = "Level4_NotPermitted",
                    RequiresCoSignature = false,
                    Message = $"Kewenangan klinis '{request.PrivilegeCode}' saat ini berstatus {privilege.PrivilegeStatus}. Akses tindakan dinonaktifkan.",
                    LicenseRadarStatus = activeSip.ExpiryRadarStatus,
                    IsLifeSavingOverrideAvailable = true
                };
            }

            // Jika kewenangan klinis membutuhkan supervisi / pendampingan (Level 2/3)
            if (privilege.IsSupervisionRequired)
            {
                return new ClinicalPrivilegeGateValidationResultDto
                {
                    IsAllowed = true,
                    GateDecision = "SupervisionRequired",
                    SupervisionLevel = "Level2_UnderSupervision",
                    RequiresCoSignature = true,
                    AssignedSupervisorUserId = privilege.SupervisorUserId,
                    AssignedSupervisorName = privilege.SupervisorUser?.DisplayName ?? privilege.SupervisorUser?.UserName ?? "Dokter Supervisor",
                    Message = "Tindakan diizinkan di bawah supervisi. Order di EMR SIMRS membutuhkan persetujuan digital (Co-Signature) supervisor dalam 24 jam.",
                    LicenseRadarStatus = activeSip.ExpiryRadarStatus,
                    IsLifeSavingOverrideAvailable = true
                };
            }

            // LEVEL 1: MANDIRI PENUH (FULL INDEPENDENT PRIVILEGE)
            return new ClinicalPrivilegeGateValidationResultDto
            {
                IsAllowed = true,
                GateDecision = "Allowed",
                SupervisionLevel = "Level1_Independent",
                RequiresCoSignature = false,
                AssignedSupervisorUserId = null,
                AssignedSupervisorName = null,
                Message = "Kewenangan klinis mandiri penuh disetujui. Dokter berwenang melakukan tindakan tanpa asistensi.",
                LicenseRadarStatus = activeSip.ExpiryRadarStatus,
                IsLifeSavingOverrideAvailable = true
            };
        }

        public async Task<CoSignatureResultDto> ProcessCoSignatureAsync(
            CoSignatureActionDto request,
            Guid supervisorUserId,
            CancellationToken cancellationToken = default)
        {
            var privilege = await _context.WfpClinicalPrivileges
                .FirstOrDefaultAsync(p => p.Id == request.ClinicalPrivilegeId, cancellationToken);

            if (privilege == null)
            {
                throw new KeyNotFoundException($"Kewenangan klinis dengan ID {request.ClinicalPrivilegeId} tidak ditemukan.");
            }

            var supervisorUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == supervisorUserId, cancellationToken);

            var supervisorName = supervisorUser?.DisplayName ?? supervisorUser?.UserName ?? supervisorUserId.ToString();

            var now = DateTime.UtcNow;

            privilege.SupervisorUserId = supervisorUserId;
            privilege.ApprovalNotes = $"{privilege.ApprovalNotes} | Co-Signature ({request.Decision}): {request.SupervisorNotes} pada {now:g}".Trim();
            privilege.UpdateBy = supervisorUserId;
            privilege.UpdateDateTime = now;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Co-Signature diproses untuk wewenang {Code}: Keputusan {Decision} oleh Supervisor {Supervisor}",
                privilege.PrivilegeCode, request.Decision, supervisorName);

            return new CoSignatureResultDto
            {
                IsSuccess = true,
                Decision = request.Decision,
                PrivilegeId = privilege.Id,
                CoSignedAtUtc = now,
                SupervisorUserName = supervisorName,
                Message = request.Decision == "Approved"
                    ? "Persetujuan digital Co-Signature berhasil dibubuhkan. Order tindakan supervisi disahkan."
                    : "Order tindakan supervisi ditolak oleh dokter supervisor."
            };
        }

        public async Task<EmergencyOverrideLogDto> RecordLifeSavingOverrideAsync(
            ClinicalEmergencyOverrideRequestDto request,
            Guid doctorUserId,
            CancellationToken cancellationToken = default)
        {
            var profile = await _context.MstWorkforceProfiles
                .FirstOrDefaultAsync(p => p.Id == request.WorkforceProfileId, cancellationToken);

            if (profile == null)
            {
                throw new KeyNotFoundException($"Profil dokter dengan ID {request.WorkforceProfileId} tidak ditemukan.");
            }

            var now = DateTime.UtcNow;

            var overrideLog = new TrxClinicalPrivilegeEmergencyOverrideLog
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = request.WorkforceProfileId,
                PatientId = request.PatientId,
                EncounterId = request.EncounterId,
                PrivilegeItemCode = request.PrivilegeItemCode,
                ProcedureName = request.ProcedureName,
                OverrideReason = request.EmergencyJustification.Trim(),
                OverrideAtUtc = now,
                WitnessUserId = request.WitnessUserId,
                WitnessName = request.WitnessName,
                IsReviewedByKmkp = false,
                ReviewedAtUtc = null,
                ReviewedByUserId = null,
                KmkpReviewNotes = null,
                KmkpVerdict = "PendingReview",
                IsActive = true,
                CreateBy = doctorUserId,
                CreateDateTime = now
            };

            _context.TrxClinicalPrivilegeEmergencyOverrideLogs.Add(overrideLog);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "LIFE-SAVING OVERRIDE DI REKAM: Dokter {Doctor} (#{Code}) melakukan tindakan di luar RKK: {Proc} untuk Encounter {Enc}. Alasan: {Reason}. Alert dikirim ke KMKP.",
                profile.DisplayName, profile.ProfileCode, request.ProcedureName, request.EncounterId, request.EmergencyJustification);

            return MapToOverrideDto(overrideLog, profile, null);
        }

        public async Task<EmergencyOverrideLogDto> ReviewEmergencyOverrideByKmkpAsync(
            ReviewEmergencyOverrideDto request,
            Guid kmkpAuditorUserId,
            CancellationToken cancellationToken = default)
        {
            var log = await _context.TrxClinicalPrivilegeEmergencyOverrideLogs
                .Include(l => l.WorkforceProfile)
                .FirstOrDefaultAsync(l => l.Id == request.OverrideLogId, cancellationToken);

            if (log == null)
            {
                throw new KeyNotFoundException($"Log audit override dengan ID {request.OverrideLogId} tidak ditemukan.");
            }

            var auditorUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == kmkpAuditorUserId, cancellationToken);

            var auditorName = auditorUser?.DisplayName ?? auditorUser?.UserName ?? kmkpAuditorUserId.ToString();

            var now = DateTime.UtcNow;

            log.IsReviewedByKmkp = true;
            log.ReviewedAtUtc = now;
            log.ReviewedByUserId = kmkpAuditorUserId;
            log.KmkpVerdict = request.Verdict;
            log.KmkpReviewNotes = request.ReviewNotes.Trim();
            log.UpdateBy = kmkpAuditorUserId;
            log.UpdateDateTime = now;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Audit KMKP Selesai untuk Override #{LogId}: Putusan {Verdict} oleh Auditor {Auditor}",
                log.Id, request.Verdict, auditorName);

            return MapToOverrideDto(log, log.WorkforceProfile, auditorName);
        }

        public async Task<List<EmergencyOverrideLogDto>> GetEmergencyOverrideLogsAsync(
            bool? pendingOnly = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.TrxClinicalPrivilegeEmergencyOverrideLogs
                .Include(l => l.WorkforceProfile)
                .Include(l => l.ReviewedByUser)
                .Where(l => l.IsActive && !l.IsDelete);

            if (pendingOnly.HasValue && pendingOnly.Value)
            {
                query = query.Where(l => !l.IsReviewedByKmkp);
            }

            var logs = await query
                .OrderByDescending(l => l.OverrideAtUtc)
                .ToListAsync(cancellationToken);

            return logs.Select(l => MapToOverrideDto(l, l.WorkforceProfile, l.ReviewedByUser?.DisplayName)).ToList();
        }

        public async Task<TemporaryPrivilegeResultDto> GrantTemporaryPrivilegeAsync(
            GrantTemporaryPrivilegeDto request,
            Guid authorizedByUserId,
            CancellationToken cancellationToken = default)
        {
            var profile = await _context.MstWorkforceProfiles
                .FirstOrDefaultAsync(p => p.Id == request.WorkforceProfileId, cancellationToken);

            if (profile == null)
            {
                throw new KeyNotFoundException($"Profil nakes dengan ID {request.WorkforceProfileId} tidak ditemukan.");
            }

            var catalogs = await _context.MstClinicalPrivilegeCatalogs
                .Where(c => request.PrivilegeCatalogIds.Contains(c.Id))
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;
            var endDate = now.AddDays(request.ValidityDays);
            var isEmergency = request.PrivilegeNature.Equals("Emergency", StringComparison.OrdinalIgnoreCase);

            foreach (var catalog in catalogs)
            {
                var privilege = new WfpClinicalPrivilege
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = profile.Id,
                    ClinicalPrivilegeCatalogId = catalog.Id,
                    PrivilegeCode = catalog.PrivilegeCode,
                    PrivilegeName = catalog.PrivilegeName,
                    PrivilegeType = isEmergency ? ClinicalPrivilegeType.EmergencyPrivilege : ClinicalPrivilegeType.TemporaryPrivilege,
                    ClinicalScope = catalog.Description,
                    EffectiveStartDate = now,
                    EffectiveEndDate = endDate,
                    PrivilegeStatus = ClinicalPrivilegeStatus.Active,
                    IsTemporary = true,
                    IsEmergencyPrivilege = isEmergency,
                    IsSupervisionRequired = catalog.RequiresSupervision,
                    SupervisorUserId = request.SupervisorUserId,
                    IsClinicalServiceBlocked = false,
                    IsSchedulingBlocked = false,
                    GrantedAt = now,
                    GrantedByUserId = authorizedByUserId,
                    ApprovalNotes = $"Kewenangan Sementara Fast-Track ({request.PrivilegeNature}): {request.Reason}",
                    IsActive = true,
                    CreateBy = authorizedByUserId,
                    CreateDateTime = now
                };

                _context.WfpClinicalPrivileges.Add(privilege);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Kewenangan Klinis Sementara ({Nature}) berhasil diberikan untuk nakes {Doctor} sebanyak {Count} tindakan (Berlaku s.d. {End:d}).",
                request.PrivilegeNature, profile.DisplayName, catalogs.Count, endDate);

            return new TemporaryPrivilegeResultDto
            {
                IsSuccess = true,
                WorkforceProfileId = profile.Id,
                DoctorName = profile.DisplayName,
                PrivilegeNature = request.PrivilegeNature,
                EffectiveStartDate = now,
                EffectiveEndDate = endDate,
                TotalPrivilegesGranted = catalogs.Count,
                Message = $"Kewenangan klinis sementara ({request.PrivilegeNature}) berhasil diaktifkan selama {request.ValidityDays} hari."
            };
        }

        private static EmergencyOverrideLogDto MapToOverrideDto(
            TrxClinicalPrivilegeEmergencyOverrideLog log,
            QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models.MstWorkforceProfile? profile,
            string? auditorName)
        {
            return new EmergencyOverrideLogDto
            {
                Id = log.Id,
                WorkforceProfileId = log.WorkforceProfileId,
                DoctorName = profile?.DisplayName ?? "-",
                DoctorCode = profile?.ProfileCode ?? "-",
                PrivilegeItemCode = log.PrivilegeItemCode,
                ProcedureName = log.ProcedureName,
                PatientId = log.PatientId,
                EncounterId = log.EncounterId,
                OverrideReason = log.OverrideReason,
                OverrideAtUtc = log.OverrideAtUtc,
                WitnessName = log.WitnessName,
                IsReviewedByKmkp = log.IsReviewedByKmkp,
                KmkpVerdict = log.KmkpVerdict,
                KmkpReviewNotes = log.KmkpReviewNotes,
                ReviewedAtUtc = log.ReviewedAtUtc,
                ReviewerUserName = auditorName ?? "-"
            };
        }
    }
}
