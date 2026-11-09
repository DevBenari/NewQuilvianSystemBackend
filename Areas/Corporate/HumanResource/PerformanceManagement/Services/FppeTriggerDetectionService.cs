using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Implementasi Layanan Pendeteksi Ambang Pemicu Kritis FPPE (INT-HR-74)
    /// </summary>
    public class FppeTriggerDetectionService : IFppeTriggerDetectionService
    {
        private readonly ApplicationDbContext _context;
        // In-memory feed cache for incoming real-time events in addition to DB scans
        private static readonly List<FppeTriggerFeedDto> _runtimeFeedCache = new();

        public FppeTriggerDetectionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<FppeTriggerFeedDto>> GetActiveTriggerFeedAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<FppeTriggerFeedDto>();

            // 1. Sertakan feed dinamis dari runtime cache
            lock (_runtimeFeedCache)
            {
                result.AddRange(_runtimeFeedCache.Where(f => f.DetectedAt >= DateTime.UtcNow.AddDays(-30)));
            }

            // 2. Scan dokter aktif untuk mendeteksi potensi staf baru tanpa penugasan FPPE
            var activeDoctors = await _context.Set<MstDoctor>()
                .Include(d => d.WorkforceProfile)
                .Where(d => d.IsActive && d.JoinDate >= DateTime.UtcNow.AddMonths(-3))
                .Take(5)
                .ToListAsync(cancellationToken);

            foreach (var doc in activeDoctors)
            {
                var existingEngagement = await _context.TrxFppeEngagements
                    .AnyAsync(e => e.WorkforceProfileId == doc.WorkforceProfileId && e.TriggerSource == "NEW_STAFF", cancellationToken);

                if (!existingEngagement && !result.Any(r => r.WorkforceProfileId == doc.WorkforceProfileId && r.TriggerType == "NEW_STAFF_ONBOARDING"))
                {
                    result.Add(new FppeTriggerFeedDto
                    {
                        TriggerId = Guid.NewGuid(),
                        WorkforceProfileId = doc.WorkforceProfileId,
                        DoctorName = doc.FullName,
                        DoctorCode = doc.DoctorCode,
                        Specialization = doc.SpecialistName ?? "Spesialis Baru",
                        TriggerType = "NEW_STAFF_ONBOARDING",
                        Severity = "WARNING",
                        IncidentDate = doc.JoinDate ?? DateTime.UtcNow,
                        Description = $"Dokter baru bergabung pada {doc.JoinDate:dd-MMM-yyyy}. Wajib proctoring FPPE minimal 10 kasus / 3 bulan.",
                        MetricEvidence = "Status kredensialing awal selesai; butuh pendampingan proctor senior.",
                        RecommendedAction = "Inisiasi FPPE Staf Baru (VR-OPP-09)",
                        DetectedAt = DateTime.UtcNow
                    });
                }
            }

            // 3. Scan penugasan FPPE yang sedang aktif dengan pemicu bahaya (SENTINEL_EVENT, ABNORMAL_COMPLICATION)
            var activeDangerousEngagements = await _context.TrxFppeEngagements
                .Where(e => e.Status == "ACTIVE" && (e.TriggerSource == "SENTINEL_EVENT" || e.TriggerSource == "ABNORMAL_COMPLICATION"))
                .ToListAsync(cancellationToken);

            foreach (var eng in activeDangerousEngagements)
            {
                if (!result.Any(r => r.WorkforceProfileId == eng.WorkforceProfileId && r.TriggerType == eng.TriggerSource))
                {
                    var staffProfile = await _context.Set<MstWorkforceProfile>()
                        .FirstOrDefaultAsync(p => p.Id == eng.WorkforceProfileId, cancellationToken);

                    result.Add(new FppeTriggerFeedDto
                    {
                        TriggerId = Guid.NewGuid(),
                        WorkforceProfileId = eng.WorkforceProfileId,
                        DoctorName = staffProfile?.DisplayName ?? "Dokter Terkait",
                        DoctorCode = staffProfile?.ProfileCode ?? "-",
                        Specialization = "Klinisi Spesialis",
                        TriggerType = eng.TriggerSource,
                        Severity = eng.TriggerSource == "SENTINEL_EVENT" ? "FATAL" : "ERROR",
                        IncidentDate = eng.StartDate,
                        Description = eng.InitialReasonJustification,
                        MetricEvidence = eng.TriggerSource == "SENTINEL_EVENT" 
                            ? "1 kejadian sentinel keselamatan pasien (KARS STARKES VR-OPP-05)."
                            : "Tingkat komplikasi bedah > 2 standar deviasi selama 3 bulan berturut-turut (VR-OPP-06).",
                        RecommendedAction = "Investigasi Kasus FPPE & Pendampingan Tindakan",
                        DetectedAt = eng.StartDate
                    });
                }
            }

            return result.OrderByDescending(r => r.Severity == "FATAL")
                .ThenByDescending(r => r.DetectedAt)
                .ToList();
        }

        public Task<FppeTriggerFeedDto> ReceiveTriggerEventAsync(FppeTriggerFeedDto feedItem, CancellationToken cancellationToken = default)
        {
            if (feedItem == null) throw new ArgumentNullException(nameof(feedItem));

            if (feedItem.TriggerId == Guid.Empty)
            {
                feedItem.TriggerId = Guid.NewGuid();
            }

            feedItem.DetectedAt = DateTime.UtcNow;

            lock (_runtimeFeedCache)
            {
                _runtimeFeedCache.Add(feedItem);
            }

            return Task.FromResult(feedItem);
        }
    }
}
