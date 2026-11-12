using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Implementasi Layanan Pendeteksi Ambang Pemicu Kritis FPPE (INT-HR-74)
    /// </summary>
    /// <remarks>
    /// Remediasi 10 November 2026 (BE-HRD-229): event pemicu yang diterima dari modul lain disimpan di tabel
    /// <c>HrdFppeTriggerEvent</c>, bukan <c>List</c> statis di memori proses yang hilang saat aplikasi di-restart.
    /// Feed aktif menggabungkan event tersimpan 30 hari terakhir dengan hasil pemindaian dokter baru dan penugasan berbahaya.
    /// </remarks>
    public class FppeTriggerDetectionService : IFppeTriggerDetectionService
    {
        private const int ActiveFeedWindowDays = 30;

        private readonly ApplicationDbContext _context;

        public FppeTriggerDetectionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<FppeTriggerFeedDto>> GetActiveTriggerFeedAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<FppeTriggerFeedDto>();
            var windowStart = DateTime.UtcNow.AddDays(-ActiveFeedWindowDays);

            // 1. Sertakan event pemicu yang diterima dan tersimpan di database (30 hari terakhir)
            var storedEvents = await _context.HrdFppeTriggerEvents
                .AsNoTracking()
                .Where(e => !e.IsDelete && e.DetectedAt >= windowStart)
                .OrderByDescending(e => e.DetectedAt)
                .ToListAsync(cancellationToken);

            result.AddRange(storedEvents.Select(ToDto));

            // 2. Scan dokter aktif untuk mendeteksi potensi staf baru tanpa penugasan FPPE
            var activeDoctors = await _context.Set<MstDoctor>()
                .Include(d => d.WorkforceProfile)
                .Where(d => d.IsActive && d.JoinDate >= DateTime.UtcNow.AddMonths(-3))
                .Take(5)
                .ToListAsync(cancellationToken);

            foreach (var doc in activeDoctors)
            {
                var existingEngagement = await _context.HrdFppeEngagements
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
            var activeDangerousEngagements = await _context.HrdFppeEngagements
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

        public async Task<FppeTriggerFeedDto> ReceiveTriggerEventAsync(FppeTriggerFeedDto feedItem, CancellationToken cancellationToken = default)
        {
            if (feedItem == null) throw new ArgumentNullException(nameof(feedItem));
            if (feedItem.WorkforceProfileId == Guid.Empty)
            {
                throw new InvalidOperationException("Event pemicu FPPE wajib menunjuk profil tenaga kerja dokter yang sah.");
            }
            if (string.IsNullOrWhiteSpace(feedItem.TriggerType))
            {
                throw new InvalidOperationException("Jenis pemicu FPPE wajib diisi (SENTINEL_EVENT, ABNORMAL_COMPLICATION, EMR_DELINQUENCY, ETHICAL_DISCIPLINARY, NEW_STAFF_ONBOARDING, atau NEW_PRIVILEGE_REQUEST).");
            }

            var now = DateTime.UtcNow;
            var entity = new HrdFppeTriggerEvent
            {
                Id = feedItem.TriggerId == Guid.Empty ? Guid.NewGuid() : feedItem.TriggerId,
                WorkforceProfileId = feedItem.WorkforceProfileId,
                DoctorNameSnapshot = Truncate(feedItem.DoctorName, 200),
                DoctorCodeSnapshot = Truncate(feedItem.DoctorCode, 50),
                SpecializationSnapshot = Truncate(feedItem.Specialization, 150),
                TriggerType = feedItem.TriggerType.Trim().ToUpperInvariant(),
                Severity = string.IsNullOrWhiteSpace(feedItem.Severity) ? "WARNING" : feedItem.Severity.Trim().ToUpperInvariant(),
                IncidentDate = feedItem.IncidentDate == default ? now : feedItem.IncidentDate,
                Description = Truncate(feedItem.Description, 2000),
                MetricEvidence = Truncate(feedItem.MetricEvidence, 2000),
                RecommendedAction = Truncate(feedItem.RecommendedAction, 500),
                DetectedAt = now,
                CreateDateTime = now
            };

            _context.HrdFppeTriggerEvents.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return ToDto(entity);
        }

        private static FppeTriggerFeedDto ToDto(HrdFppeTriggerEvent e) => new()
        {
            TriggerId = e.Id,
            WorkforceProfileId = e.WorkforceProfileId,
            DoctorName = e.DoctorNameSnapshot ?? string.Empty,
            DoctorCode = e.DoctorCodeSnapshot ?? string.Empty,
            Specialization = e.SpecializationSnapshot ?? string.Empty,
            TriggerType = e.TriggerType,
            Severity = e.Severity,
            IncidentDate = e.IncidentDate,
            Description = e.Description ?? string.Empty,
            MetricEvidence = e.MetricEvidence ?? string.Empty,
            RecommendedAction = e.RecommendedAction ?? string.Empty,
            DetectedAt = e.DetectedAt
        };

        private static string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var trimmed = value.Trim();
            return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
        }
    }
}
