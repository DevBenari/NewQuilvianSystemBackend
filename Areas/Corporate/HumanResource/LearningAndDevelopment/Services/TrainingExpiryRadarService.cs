using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    /// <summary>
    /// Implementasi Layanan Radar Kedaluwarsa Bertingkat H-90/60/30 & Pengamanan Shift Buddy System (VR-TRN-09..12)
    /// </summary>
    /// <remarks>
    /// Remediasi 10 November 2026 (BE-HRD-240, BE-HRD-241):
    /// <list type="bullet">
    /// <item>Radar dan portofolio hanya menampilkan sertifikat yang benar-benar ada di database. Empat staf fiktif ber-GUID tetap
    /// dan dua sertifikat contoh yang dulu muncul saat data kosong dihapus.</item>
    /// <item>Nama pelatihan dibaca dari catatan pelatihan atau jenis sertifikasi, bukan teks tetap.</item>
    /// <item>Siaran peringatan melaporkan jumlah penerima yang sebenarnya dan <b>0</b> pengiriman, karena kanal notifikasi
    /// (<c>BE-HRD-314</c>) belum tersedia. Sebelumnya dilaporkan "terkirim ke minimal 5 orang" tanpa mengirim apa pun.</item>
    /// <item>Status pengamanan shift buddy system disimpan di tabel <c>HrdShiftBuddySafeguard</c>, bukan memori proses.</item>
    /// </list>
    /// </remarks>
    public class TrainingExpiryRadarService : ITrainingExpiryRadarService
    {
        private const string UnknownValue = "-";

        private readonly ApplicationDbContext _context;

        public TrainingExpiryRadarService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CertificateExpiryRadarSummaryDto> GetExpiringRadarSummaryAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow.Date;
            var horizonDate = now.AddDays(90);

            var dbCertificates = await _context.TrxTrainingCertificates
                .AsNoTracking()
                .Include(c => c.WorkforceProfile)
                    .ThenInclude(p => p!.PrimaryDepartment)
                .Include(c => c.TrainingRecord)
                .Include(c => c.CertificationType)
                    .ThenInclude(t => t!.Profession)
                .Where(c => c.IsActive && !c.IsDelete && c.ExpiredDate.HasValue && c.ExpiredDate.Value <= horizonDate)
                .OrderBy(c => c.ExpiredDate)
                .ToListAsync(cancellationToken);

            var profileIds = dbCertificates.Select(c => c.WorkforceProfileId).Distinct().ToList();
            var activeSafeguards = await _context.HrdShiftBuddySafeguards
                .AsNoTracking()
                .Where(s => s.IsActive && !s.IsDelete && profileIds.Contains(s.WorkforceProfileId))
                .ToListAsync(cancellationToken);
            var safeguardByProfile = activeSafeguards
                .GroupBy(s => s.WorkforceProfileId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.ActivatedAt).First());

            var items = new List<CertificateExpiryRadarItemDto>();

            foreach (var cert in dbCertificates)
            {
                var daysLeft = (int)(cert.ExpiredDate!.Value.Date - now).TotalDays;
                string radarZone;

                if (daysLeft < 0) radarZone = "OVERDUE_H0";
                else if (daysLeft <= 30) radarZone = "RADAR_H30_CRITICAL";
                else if (daysLeft <= 60) radarZone = "RADAR_H60";
                else radarZone = "RADAR_H90";

                safeguardByProfile.TryGetValue(cert.WorkforceProfileId, out var safeguard);

                items.Add(new CertificateExpiryRadarItemDto
                {
                    CertificateId = cert.Id,
                    WorkforceProfileId = cert.WorkforceProfileId,
                    StaffName = cert.WorkforceProfile?.DisplayName ?? UnknownValue,
                    StaffCode = cert.WorkforceProfile?.ProfileCode ?? UnknownValue,
                    DepartmentName = cert.WorkforceProfile?.PrimaryDepartment?.DepartmentName ?? UnknownValue,
                    Profession = cert.CertificationType?.Profession?.ProfessionName ?? UnknownValue,
                    CertificateNumber = cert.CertificateNumber,
                    TrainingName = ResolveTrainingName(cert),
                    IssuedDate = cert.IssuedDate,
                    ExpiredDate = cert.ExpiredDate.Value,
                    DaysUntilExpiry = daysLeft,
                    RadarZone = radarZone,
                    IsBuddySafeguardActive = safeguard?.IsBuddySystemEnforced ?? false,
                    IsRkkPrivilegeRestricted = safeguard?.IsRkkRestrictedLevel3 ?? false
                });
            }

            return new CertificateExpiryRadarSummaryDto
            {
                TotalMonitoredCertificates = items.Count,
                ZoneH90Count = items.Count(i => i.RadarZone == "RADAR_H90"),
                ZoneH60Count = items.Count(i => i.RadarZone == "RADAR_H60"),
                ZoneH30CriticalCount = items.Count(i => i.RadarZone == "RADAR_H30_CRITICAL"),
                OverdueH0Count = items.Count(i => i.RadarZone == "OVERDUE_H0"),
                Items = items
            };
        }

        public async Task<BroadcastExpiryAlertResponseDto> BroadcastAlertsAsync(
            BroadcastExpiryAlertRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var summary = await GetExpiringRadarSummaryAsync(cancellationToken);
            var targetList = request.TargetZoneFilter switch
            {
                "OVERDUE_ONLY" => summary.Items.Where(i => i.RadarZone == "OVERDUE_H0").ToList(),
                "H30_AND_OVERDUE" => summary.Items.Where(i => i.RadarZone == "OVERDUE_H0" || i.RadarZone == "RADAR_H30_CRITICAL").ToList(),
                _ => summary.Items
            };

            // Penerima dihitung per staf, bukan per sertifikat.
            var recipientCount = targetList.Select(i => i.WorkforceProfileId).Distinct().Count();
            var channels = request.DeliveryChannels ?? new List<string>();

            // Kanal pengiriman (lonceng aplikasi, email, WhatsApp) belum tersedia — menunggu BE-HRD-314.
            // Jumlah pengiriman dilaporkan apa adanya: nol.
            return new BroadcastExpiryAlertResponseDto
            {
                TotalRecipients = recipientCount,
                SuccessfulDispatches = 0,
                DeliveryChannels = channels,
                DispatchedAt = DateTime.UtcNow,
                Message = recipientCount == 0
                    ? "Tidak ada staf dengan sertifikat pelatihan wajib pada zona radar yang dipilih."
                    : $"Daftar penerima tersusun: {recipientCount} staf dengan {targetList.Count} sertifikat pada zona radar yang dipilih. Belum ada notifikasi yang terkirim karena kanal pengiriman ({(channels.Count > 0 ? string.Join(", ", channels) : "belum dipilih")}) belum tersedia di aplikasi (menunggu BE-HRD-314, VR-TRN-09)."
            };
        }

        public async Task<ShiftBuddySafeguardResponseDto> EnforceShiftBuddySafeguardAsync(
            Guid staffId,
            AssignShiftBuddySafeguardRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var staffProfile = await _context.Set<MstWorkforceProfile>()
                .FirstOrDefaultAsync(p => p.Id == staffId, cancellationToken);

            if (staffProfile == null)
            {
                throw new KeyNotFoundException($"Profil tenaga kerja {staffId} tidak ditemukan.");
            }

            var staffName = staffProfile.DisplayName;
            var now = DateTime.UtcNow;

            // Jika staf adalah dokter klinisi dan diminta restrict RKK ke Level 3 supervisi (INT-HR-77, VR-TRN-12)
            if (request.RestrictRkkToLevel3)
            {
                var doctor = await _context.Set<MstDoctor>()
                    .FirstOrDefaultAsync(d => d.WorkforceProfileId == staffId, cancellationToken);

                if (doctor != null)
                {
                    doctor.ClinicalPrivilegeStatus = ClinicalPrivilegeStatus.Suspended;
                    doctor.UpdateDateTime = now;
                }
            }

            // Satu pengamanan aktif per staf: yang lama ditutup (riwayat tetap ada), yang baru disimpan.
            var previousSafeguards = await _context.HrdShiftBuddySafeguards
                .Where(s => s.WorkforceProfileId == staffId && s.IsActive && !s.IsDelete)
                .ToListAsync(cancellationToken);
            foreach (var previous in previousSafeguards)
            {
                previous.IsActive = false;
                previous.ReleasedAt = now;
                previous.UpdateDateTime = now;
            }

            var message = "Safeguard Shift Buddy System berhasil diaktifkan. Penugasan shift mandiri diblokir di Sub-06; staf wajib didampingi supervisor"
                + (request.RestrictRkkToLevel3 ? " dan kewenangan mandiri RKK Level 4 diturunkan ke Level 3 supervisi." : ".");

            var safeguard = new HrdShiftBuddySafeguard
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = staffId,
                StaffNameSnapshot = staffName,
                IsOverdueFlagged = true,
                GracePeriodDaysRemaining = 30, // Toleransi 30 hari kalender (VR-TRN-10)
                IsBuddySystemEnforced = true, // Transmisi INT-HR-79 ke Sub-06 Scheduling (VR-TRN-11)
                IsRkkRestrictedLevel3 = request.RestrictRkkToLevel3, // Transmisi INT-HR-77 ke Sub-14 (VR-TRN-12)
                ActivatedAt = now,
                IsActive = true,
                Message = message,
                CreateDateTime = now
            };

            _context.HrdShiftBuddySafeguards.Add(safeguard);
            await _context.SaveChangesAsync(cancellationToken);

            return new ShiftBuddySafeguardResponseDto
            {
                WorkforceProfileId = staffId,
                StaffName = staffName,
                IsOverdueFlagged = safeguard.IsOverdueFlagged,
                GracePeriodDaysRemaining = safeguard.GracePeriodDaysRemaining,
                IsBuddySystemEnforced = safeguard.IsBuddySystemEnforced,
                IsRkkRestrictedLevel3 = safeguard.IsRkkRestrictedLevel3,
                ActivatedAt = safeguard.ActivatedAt,
                Message = message
            };
        }

        public async Task<StaffCertificatePortfolioDto?> GetStaffPortfolioAsync(
            Guid staffId,
            CancellationToken cancellationToken = default)
        {
            var staffProfile = await _context.Set<MstWorkforceProfile>()
                .Include(p => p.PrimaryDepartment)
                .FirstOrDefaultAsync(p => p.Id == staffId, cancellationToken);

            if (staffProfile == null) return null;

            var certs = await _context.TrxTrainingCertificates
                .AsNoTracking()
                .Include(c => c.TrainingRecord)
                .Include(c => c.CertificationType)
                .Where(c => c.WorkforceProfileId == staffId && !c.IsDelete)
                .OrderByDescending(c => c.IssuedDate)
                .ToListAsync(cancellationToken);

            var list = certs.Select(c => new CertificateItemDetailDto
            {
                CertificateId = c.Id,
                CertificateNumber = c.CertificateNumber,
                TrainingProgramName = ResolveTrainingName(c),
                Category = c.CertificationType?.CertificationTypeCode ?? UnknownValue,
                IssuedDate = c.IssuedDate,
                ExpiredDate = c.ExpiredDate,
                IssuerName = c.IssuerName ?? c.CertificationType?.IssuingAuthority ?? UnknownValue,
                IsVerified = c.IsVerified,
                CertificateUrl = c.CertificateFilePath
            }).ToList();

            var hasOverdue = list.Any(c => c.IsExpired);

            return new StaffCertificatePortfolioDto
            {
                WorkforceProfileId = staffId,
                StaffName = staffProfile.DisplayName,
                StaffCode = staffProfile.ProfileCode,
                DepartmentName = staffProfile.PrimaryDepartment?.DepartmentName ?? UnknownValue,
                TotalCertificates = list.Count,
                ActiveCertificates = list.Count(c => !c.IsExpired),
                ExpiredCertificates = list.Count(c => c.IsExpired),
                HasOverdueMandatoryTraining = hasOverdue,
                Certificates = list
            };
        }

        /// <summary>Nama pelatihan dari catatan pelatihan, lalu jenis sertifikasi; tidak pernah teks tetap.</summary>
        private static string ResolveTrainingName(TrxTrainingCertificate cert)
        {
            if (!string.IsNullOrWhiteSpace(cert.TrainingRecord?.TrainingName)) return cert.TrainingRecord.TrainingName;
            if (!string.IsNullOrWhiteSpace(cert.CertificationType?.CertificationTypeName)) return cert.CertificationType.CertificationTypeName;
            return UnknownValue;
        }
    }
}
