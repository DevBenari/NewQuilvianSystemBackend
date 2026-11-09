using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Implementasi Layanan Rapor Komposit 3 Tahun Rekredensialing & Klasifikasi Tier (INT-HR-75, VR-OPP-15..20)
    /// </summary>
    public class RecredentialingQualityService : IRecredentialingCompositeProvider
    {
        private readonly ApplicationDbContext _context;

        // Runtime storage untuk Tier Override dan Verdict Sidang Rekredensialing
        private static readonly ConcurrentDictionary<Guid, OverrideRecredTierRecord> _tierOverrides = new();
        private static readonly ConcurrentDictionary<Guid, RecredVerdictResponseDto> _verdicts = new();

        public RecredentialingQualityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RecredentialingScorecardDto> GetScorecardAsync(Guid workforceProfileId, CancellationToken cancellationToken = default)
        {
            return await CalculateScorecardAsync(workforceProfileId, new CalculateRecredScorecardRequestDto(), cancellationToken);
        }

        public async Task<RecredentialingScorecardDto> CalculateScorecardAsync(
            Guid workforceProfileId,
            CalculateRecredScorecardRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // 1. Ambil data profil tenaga kerja & dokter
            var staffProfile = await _context.Set<MstWorkforceProfile>()
                .FirstOrDefaultAsync(p => p.Id == workforceProfileId, cancellationToken);

            var doctor = await _context.Set<MstDoctor>()
                .FirstOrDefaultAsync(d => d.WorkforceProfileId == workforceProfileId, cancellationToken);

            var doctorName = doctor?.FullName ?? staffProfile?.DisplayName ?? "Dokter Terkait";
            var doctorCode = doctor?.DoctorCode ?? staffProfile?.ProfileCode ?? "-";
            var specialization = doctor?.SpecialistName ?? "Spesialisasi Klinis";

            // 2. Ambil data historis OPPE atau gunakan input simulasi
            var historicEvaluations = await _context.TrxOppeStaffEvaluations
                .Where(e => e.WorkforceProfileId == workforceProfileId)
                .OrderByDescending(e => e.CreateDateTime)
                .Take(3)
                .ToListAsync(cancellationToken);

            double year1 = request.Year1Score ?? (historicEvaluations.Count > 2 ? (double)historicEvaluations[2].FinalCompositeScore : 82.5);
            double year2 = request.Year2Score ?? (historicEvaluations.Count > 1 ? (double)historicEvaluations[1].FinalCompositeScore : 85.0);
            double year3 = request.Year3Score ?? (historicEvaluations.Count > 0 ? (double)historicEvaluations[0].FinalCompositeScore : 88.5);

            // 3. Formula Deterministik 25%-35%-40% (VR-OPP-15, AT-HRD-OPP-19)
            // FinalComposite = (0.25 * Y1) + (0.35 * Y2) + (0.40 * Y3)
            double compositeScore = Math.Round((0.25 * year1) + (0.35 * year2) + (0.40 * year3), 2);

            // 4. Deteksi Insiden Sentinel dan FPPE (INT-HR-74, VR-OPP-05..07)
            var fppeEngagements = await _context.TrxFppeEngagements
                .Where(e => e.WorkforceProfileId == workforceProfileId)
                .ToListAsync(cancellationToken);

            bool hasSentinel = request.SimulateSentinelIncident || fppeEngagements.Any(e => e.TriggerSource == "SENTINEL_EVENT");
            bool hasEmrDelinquency = request.SimulateEmrDelinquency || fppeEngagements.Any(e => e.TriggerSource == "EMR_DELINQUENCY");
            bool hasFailedFppe = fppeEngagements.Any(e => e.Status == "FAILED_RESTRICTED");

            string fppeSummary = fppeEngagements.Count > 0
                ? $"Total penugasan FPPE: {fppeEngagements.Count} kasus (Sentinel: {(hasSentinel ? 1 : 0)}, Gagal/Restricted: {(hasFailedFppe ? 1 : 0)})."
                : "Nihil riwayat penugasan FPPE dan kasus sentinel selama siklus 3 tahun.";

            // 5. Klasifikasi Eligibilitas 3 Tier Baku (VR-OPP-16, VR-OPP-17, VR-OPP-18, AT-HRD-OPP-20)
            string calculatedTier;
            if (hasSentinel || hasFailedFppe || compositeScore < 65.0)
            {
                // VR-OPP-18: Tier Merah (Skor < 65.0 atau riwayat sentinel / gagal FPPE)
                calculatedTier = "RedPlenaryInvestigation";
            }
            else if (compositeScore >= 80.0 && !hasEmrDelinquency)
            {
                // VR-OPP-16: Tier Hijau (Skor >= 80.0 dan 0 sentinel / 0 kendala)
                calculatedTier = "GreenFastTrack";
            }
            else
            {
                // VR-OPP-17: Tier Kuning (Skor 65.0 - 79.9 atau penundaan EMR)
                calculatedTier = "YellowClarificationReview";
            }

            // 6. Cek apakah terdapat Diskresi Penyesuaian Tier Aktif (VR-OPP-19)
            var assignedTier = calculatedTier;
            bool isOverridden = false;
            string? overrideReason = null;
            Guid? overriddenBy = null;
            string? overriddenByName = null;
            DateTime? overriddenAt = null;

            if (_tierOverrides.TryGetValue(workforceProfileId, out var overrideRecord))
            {
                assignedTier = overrideRecord.NewTier;
                isOverridden = true;
                overrideReason = overrideRecord.Reason;
                overriddenBy = overrideRecord.ApprovedByCommitteeChairmanId;
                overriddenByName = overrideRecord.ChairmanName;
                overriddenAt = overrideRecord.OverriddenAt;
            }

            return new RecredentialingScorecardDto
            {
                WorkforceProfileId = workforceProfileId,
                DoctorName = doctorName,
                DoctorCode = doctorCode,
                Specialization = specialization,
                Year1Score = year1,
                Year2Score = year2,
                Year3Score = year3,
                FinalCompositeScore = compositeScore,
                CalculatedTier = calculatedTier,
                AssignedTier = assignedTier,
                IsOverridden = isOverridden,
                TierOverrideReason = overrideReason,
                OverriddenBy = overriddenBy,
                OverriddenByName = overriddenByName,
                OverriddenAt = overriddenAt,
                HasSentinelIncident = hasSentinel,
                HasEmrDelinquency = hasEmrDelinquency,
                FppeHistorySummary = fppeSummary,
                CalculatedAt = DateTime.UtcNow
            };
        }

        public async Task<RecredentialingScorecardDto> OverrideTierAsync(
            Guid workforceProfileId,
            OverrideRecredTierRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // 1. Validasi Keharusan Catatan Justifikasi Klinis Minimal 20 Karakter (VR-OPP-19, AT-HRD-OPP-21)
            if (string.IsNullOrWhiteSpace(request.JustificationReason) || request.JustificationReason.Trim().Length < 20)
            {
                throw new InvalidOperationException("Perubahan tier rekomendasi rekredensialing wajib melampirkan dasar justifikasi klinis tertulis minimal 20 karakter (VR-OPP-19).");
            }

            // 2. Validasi Otorisasi Ketua Komite Medik
            if (request.ApprovedByCommitteeChairmanId == Guid.Empty)
            {
                throw new InvalidOperationException("Penyesuaian tier kelayakan wajib disetujui secara eksplisit oleh Ketua Komite Medik (VR-OPP-19).");
            }

            // 3. Validasi Tier Baru
            var validTiers = new[] { "GreenFastTrack", "YellowClarificationReview", "RedPlenaryInvestigation" };
            if (!validTiers.Contains(request.NewTier))
            {
                throw new ArgumentException($"Tier '{request.NewTier}' tidak sah. Pilihan: GreenFastTrack, YellowClarificationReview, RedPlenaryInvestigation.");
            }

            var chairmanProfile = await _context.Set<MstWorkforceProfile>()
                .FirstOrDefaultAsync(p => p.Id == request.ApprovedByCommitteeChairmanId, cancellationToken);

            var overrideRecord = new OverrideRecredTierRecord
            {
                WorkforceProfileId = workforceProfileId,
                NewTier = request.NewTier,
                Reason = request.JustificationReason.Trim(),
                ApprovedByCommitteeChairmanId = request.ApprovedByCommitteeChairmanId,
                ChairmanName = chairmanProfile?.DisplayName ?? "Ketua Komite Medik",
                OverriddenAt = DateTime.UtcNow
            };

            _tierOverrides[workforceProfileId] = overrideRecord;

            // Kembalikan scorecard terbarui
            return await GetScorecardAsync(workforceProfileId, cancellationToken);
        }

        public async Task<RecredVerdictResponseDto> IssueVerdictAsync(
            CreateRecredVerdictRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // 1. Validasi 4 Putusan Baku Sidang Rekredensialing (VR-OPP-20)
            var validVerdicts = new[] { "RenewAsIs", "Upgrade", "Restricted", "Revoked" };
            if (!validVerdicts.Contains(request.Verdict))
            {
                throw new ArgumentException($"Putusan '{request.Verdict}' tidak sah. Putusan sidang rekredensialing wajib salah satu dari 4 keputusan baku komite medik: RenewAsIs, Upgrade, Restricted, Revoked (VR-OPP-20).");
            }

            // 2. Validasi Otorisasi Pengesahan Sidang
            if (request.ApprovedByCommitteeChairmanId == Guid.Empty)
            {
                throw new InvalidOperationException("Putusan sidang rekredensialing wajib disahkan oleh Ketua Komite Medik.");
            }

            var staffProfile = await _context.Set<MstWorkforceProfile>()
                .FirstOrDefaultAsync(p => p.Id == request.WorkforceProfileId, cancellationToken);

            var doctor = await _context.Set<MstDoctor>()
                .FirstOrDefaultAsync(d => d.WorkforceProfileId == request.WorkforceProfileId, cancellationToken);

            var doctorName = doctor?.FullName ?? staffProfile?.DisplayName ?? "Dokter Target";
            var doctorCode = doctor?.DoctorCode ?? staffProfile?.ProfileCode ?? "-";

            // 3. Update Status Kewenangan Klinis di Database Lokal
            if (doctor != null)
            {
                switch (request.Verdict)
                {
                    case "RenewAsIs":
                    case "Upgrade":
                        doctor.ClinicalPrivilegeStatus = ClinicalPrivilegeStatus.Active;
                        break;
                    case "Restricted":
                        doctor.ClinicalPrivilegeStatus = ClinicalPrivilegeStatus.Suspended;
                        break;
                    case "Revoked":
                        doctor.ClinicalPrivilegeStatus = ClinicalPrivilegeStatus.Revoked;
                        break;
                }
                doctor.UpdateDateTime = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }

            // 4. Pembentukan Stempel Digital SHA-256
            var decidedAt = DateTime.UtcNow;
            var payload = $"{request.WorkforceProfileId}|{request.Verdict}|{request.ApprovedByCommitteeChairmanId}|{decidedAt:O}|QUILVIAN-RECRED-VERDICT";
            var signatureHash = ComputeSha256Hex(payload);

            var verdictId = Guid.NewGuid();
            var response = new RecredVerdictResponseDto
            {
                Id = verdictId,
                WorkforceProfileId = request.WorkforceProfileId,
                DoctorName = doctorName,
                DoctorCode = doctorCode,
                Verdict = request.Verdict,
                CommitteeNotes = request.CommitteeNotes,
                ApprovedByCommitteeChairmanId = request.ApprovedByCommitteeChairmanId,
                DecidedAt = decidedAt,
                SignatureHash = signatureHash
            };

            _verdicts[verdictId] = response;
            return response;
        }

        private static string ComputeSha256Hex(string input)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hashBytes);
        }

        private class OverrideRecredTierRecord
        {
            public Guid WorkforceProfileId { get; set; }
            public string NewTier { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
            public Guid ApprovedByCommitteeChairmanId { get; set; }
            public string ChairmanName { get; set; } = string.Empty;
            public DateTime OverriddenAt { get; set; }
        }
    }
}
