using System;
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
    /// <remarks>
    /// Remediasi 10 November 2026 (BE-HRD-233, BE-HRD-234, BE-HRD-235):
    /// <list type="bullet">
    /// <item>Skor tahunan yang belum ada <b>tidak lagi</b> diisi angka karangan 82,5 / 85,0 / 88,5. Bila riwayat OPPE
    /// belum cukup dan tidak ada masukan simulasi, rapor dikembalikan dengan penanda <c>IsDataSufficient = false</c>
    /// dan tier <c>InsufficientData</c>.</item>
    /// <item>Penyesuaian tier oleh Ketua Komite Medik dan putusan sidang rekredensialing disimpan di tabel
    /// <c>HrdRecredentialingTierOverride</c> dan <c>HrdRecredentialingVerdict</c>, bukan lagi di memori proses.</item>
    /// </list>
    /// </remarks>
    public class RecredentialingQualityService : IRecredentialingCompositeProvider
    {
        public const string InsufficientDataTier = "InsufficientData";

        private static readonly string[] ValidTiers = { "GreenFastTrack", "YellowClarificationReview", "RedPlenaryInvestigation" };
        private static readonly string[] ValidVerdicts = { "RenewAsIs", "Upgrade", "Restricted", "Revoked" };

        private readonly ApplicationDbContext _context;

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

            // 2. Ambil data historis OPPE (3 rapor terakhir, dari yang paling lama ke yang paling baru) atau pakai input simulasi.
            //    Tahun yang tidak punya rapor DAN tidak diberi nilai simulasi dicatat sebagai data belum cukup — tidak dikarang.
            var historicEvaluations = await _context.TrxOppeStaffEvaluations
                .Where(e => e.WorkforceProfileId == workforceProfileId)
                .OrderByDescending(e => e.CreateDateTime)
                .Take(3)
                .ToListAsync(cancellationToken);

            double? year1 = request.Year1Score ?? (historicEvaluations.Count > 2 ? (double)historicEvaluations[2].FinalCompositeScore : null);
            double? year2 = request.Year2Score ?? (historicEvaluations.Count > 1 ? (double)historicEvaluations[1].FinalCompositeScore : null);
            double? year3 = request.Year3Score ?? (historicEvaluations.Count > 0 ? (double)historicEvaluations[0].FinalCompositeScore : null);

            var missingYears = new List<int>();
            if (year1 is null) missingYears.Add(1);
            if (year2 is null) missingYears.Add(2);
            if (year3 is null) missingYears.Add(3);
            var isDataSufficient = missingYears.Count == 0;

            // 3. Formula Deterministik 25%-35%-40% (VR-OPP-15, AT-HRD-OPP-19) — hanya bila ketiga tahun tersedia.
            // FinalComposite = (0.25 * Y1) + (0.35 * Y2) + (0.40 * Y3)
            double compositeScore = isDataSufficient
                ? Math.Round((0.25 * year1!.Value) + (0.35 * year2!.Value) + (0.40 * year3!.Value), 2)
                : 0d;

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
            if (!isDataSufficient)
            {
                calculatedTier = InsufficientDataTier;
            }
            else if (hasSentinel || hasFailedFppe || compositeScore < 65.0)
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

            // 6. Cek apakah terdapat Diskresi Penyesuaian Tier Aktif (VR-OPP-19) — dibaca dari database, bukan memori proses
            var assignedTier = calculatedTier;
            bool isOverridden = false;
            string? overrideReason = null;
            Guid? overriddenBy = null;
            string? overriddenByName = null;
            DateTime? overriddenAt = null;

            var overrideRecord = await _context.HrdRecredentialingTierOverrides
                .AsNoTracking()
                .Where(o => o.WorkforceProfileId == workforceProfileId && o.IsActive && !o.IsDelete)
                .OrderByDescending(o => o.OverriddenAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (overrideRecord != null)
            {
                assignedTier = overrideRecord.NewTier;
                isOverridden = true;
                overrideReason = overrideRecord.JustificationReason;
                overriddenBy = overrideRecord.ApprovedByCommitteeChairmanId;
                overriddenByName = overrideRecord.ChairmanNameSnapshot;
                overriddenAt = overrideRecord.OverriddenAt;
            }

            var sufficiencyMessage = isDataSufficient
                ? null
                : $"Data belum cukup: rapor OPPE tahun ke-{string.Join(", ", missingYears)} belum tersedia ({historicEvaluations.Count} dari 3 rapor tahunan). Skor komposit dan tier belum dapat dihitung.";

            return new RecredentialingScorecardDto
            {
                WorkforceProfileId = workforceProfileId,
                DoctorName = doctorName,
                DoctorCode = doctorCode,
                Specialization = specialization,
                Year1Score = year1 ?? 0d,
                Year2Score = year2 ?? 0d,
                Year3Score = year3 ?? 0d,
                FinalCompositeScore = compositeScore,
                IsDataSufficient = isDataSufficient,
                AvailableEvaluationYears = 3 - missingYears.Count,
                MissingEvaluationYears = missingYears,
                DataSufficiencyMessage = sufficiencyMessage,
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
            if (!ValidTiers.Contains(request.NewTier))
            {
                throw new ArgumentException($"Tier '{request.NewTier}' tidak sah. Pilihan: GreenFastTrack, YellowClarificationReview, RedPlenaryInvestigation.");
            }

            var chairmanProfile = await _context.Set<MstWorkforceProfile>()
                .FirstOrDefaultAsync(p => p.Id == request.ApprovedByCommitteeChairmanId, cancellationToken);

            // Tier hasil kalkulasi saat ini direkam sebagai pembanding pada jejak audit
            var currentScorecard = await CalculateScorecardAsync(workforceProfileId, new CalculateRecredScorecardRequestDto(), cancellationToken);

            var now = DateTime.UtcNow;

            // 4. Nonaktifkan penyesuaian sebelumnya (riwayat tetap tersimpan), lalu simpan penyesuaian baru
            var previousOverrides = await _context.HrdRecredentialingTierOverrides
                .Where(o => o.WorkforceProfileId == workforceProfileId && o.IsActive && !o.IsDelete)
                .ToListAsync(cancellationToken);
            foreach (var previous in previousOverrides)
            {
                previous.IsActive = false;
                previous.UpdateDateTime = now;
                previous.UpdateBy = request.ApprovedByCommitteeChairmanId;
            }

            _context.HrdRecredentialingTierOverrides.Add(new HrdRecredentialingTierOverride
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                CalculatedTier = currentScorecard.CalculatedTier,
                NewTier = request.NewTier,
                JustificationReason = request.JustificationReason.Trim(),
                ApprovedByCommitteeChairmanId = request.ApprovedByCommitteeChairmanId,
                ChairmanNameSnapshot = chairmanProfile?.DisplayName ?? "Ketua Komite Medik",
                OverriddenAt = now,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = request.ApprovedByCommitteeChairmanId
            });

            await _context.SaveChangesAsync(cancellationToken);

            // Kembalikan scorecard terbarui
            return await GetScorecardAsync(workforceProfileId, cancellationToken);
        }

        public async Task<RecredVerdictResponseDto> IssueVerdictAsync(
            CreateRecredVerdictRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // 1. Validasi 4 Putusan Baku Sidang Rekredensialing (VR-OPP-20)
            if (!ValidVerdicts.Contains(request.Verdict))
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
            }

            // 4. Pembentukan Stempel Digital SHA-256
            var decidedAt = DateTime.UtcNow;
            var payload = $"{request.WorkforceProfileId}|{request.Verdict}|{request.ApprovedByCommitteeChairmanId}|{decidedAt:O}|QUILVIAN-RECRED-VERDICT";
            var signatureHash = ComputeSha256Hex(payload);

            // 5. Simpan putusan ke database (jejak audit akreditasi), satu transaksi dengan pembaruan status dokter
            var verdict = new HrdRecredentialingVerdict
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = request.WorkforceProfileId,
                DoctorCodeSnapshot = doctorCode,
                DoctorNameSnapshot = doctorName,
                Verdict = request.Verdict,
                CommitteeNotes = request.CommitteeNotes,
                ApprovedByCommitteeChairmanId = request.ApprovedByCommitteeChairmanId,
                DecidedAt = decidedAt,
                SignatureHash = signatureHash,
                CreateDateTime = decidedAt,
                CreateBy = request.ApprovedByCommitteeChairmanId
            };
            _context.HrdRecredentialingVerdicts.Add(verdict);

            await _context.SaveChangesAsync(cancellationToken);

            return new RecredVerdictResponseDto
            {
                Id = verdict.Id,
                WorkforceProfileId = verdict.WorkforceProfileId,
                DoctorName = doctorName,
                DoctorCode = doctorCode,
                Verdict = verdict.Verdict,
                CommitteeNotes = verdict.CommitteeNotes ?? string.Empty,
                ApprovedByCommitteeChairmanId = verdict.ApprovedByCommitteeChairmanId,
                DecidedAt = verdict.DecidedAt,
                SignatureHash = verdict.SignatureHash
            };
        }

        private static string ComputeSha256Hex(string input)
        {
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hashBytes);
        }
    }
}
