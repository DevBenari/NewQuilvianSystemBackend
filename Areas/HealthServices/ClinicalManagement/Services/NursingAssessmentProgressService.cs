using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Progres Pengkajian Pasien lima bagian — <c>BE-RWI-112</c>, <c>FR-KEP-050</c> s.d. <c>FR-KEP-052</c>,
    /// <c>INV-KEP-09</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Dihitung dari keadaan dokumen, bukan dari temuan klinis.</b> Satu dokumen <c>Completed</c> →
    /// ✓ walaupun ada konsep baru; hanya konsep → !; tidak ada dokumen yang tidak dibatalkan → ○.
    /// </para>
    /// <para>
    /// <b>Contoh Budi hari ke-2.</b> Kajian Umum selesai, Resiko Jatuh selesai berpita Tinggi, Monitoring
    /// Nyeri konsep, Edukasi dan Perencanaan Pulang belum ada → ✓ ✓ ! ○ ○ → <c>CompletedCount = 2</c>,
    /// <c>ProgressPercent = 40</c>, dan <b>satu alert terpisah</b> "Risiko Jatuh Tinggi". Resiko Jatuh tetap
    /// ✓ — pengkajiannya lengkap walau hasilnya mengkhawatirkan.
    /// </para>
    /// <para>
    /// <b>Gagal memuat bukan ○.</b> Service ini tidak menangkap galat basis data untuk mengembalikan lima ○;
    /// galat naik apa adanya dan layar menampilkan "Gagal memuat progres pengkajian" (<c>AC-KEP-072</c>).
    /// Hanya episode yang tidak ada yang dijawab <c>null</c> → <c>404</c>.
    /// </para>
    /// </remarks>
    public class NursingAssessmentProgressService
    {
        private static readonly (string Code, string Label, PatientAssessmentType[] Types)[] Bagian =
        {
            ("GENERAL", "Kajian Umum", new[] { PatientAssessmentType.Initial, PatientAssessmentType.Reassessment }),
            ("FALL_RISK", "Resiko Jatuh", new[] { PatientAssessmentType.FallRisk }),
            ("PAIN", "Monitoring Nyeri", new[] { PatientAssessmentType.PainMonitoring }),
            ("EDUCATION", "Assesment Edukasi", new[] { PatientAssessmentType.EducationAssessment }),
            ("DISCHARGE_PLANNING", "Perencanaan Pulang", new[] { PatientAssessmentType.DischargePlanning })
        };

        private readonly ApplicationDbContext _dbContext;

        public NursingAssessmentProgressService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<NursingAssessmentProgressResponse?> GetProgressAsync(Guid episodeId, CancellationToken cancellationToken = default)
        {
            var adaEpisode = await _dbContext.Set<InpEpisode>().AsNoTracking()
                .AnyAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);

            if (!adaEpisode)
                return null;

            var now = DateTime.UtcNow;
            var jenisV2 = Bagian.SelectMany(b => b.Types).ToList();

            var dokumen = await _dbContext.Set<TrxPatientAssessment>().AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete && !x.IsCancel &&
                            x.AssessmentStatus != PatientAssessmentStatus.Cancelled &&
                            jenisV2.Contains(x.AssessmentType))
                .Select(x => new
                {
                    x.Id,
                    x.AssessmentType,
                    x.AssessmentStatus,
                    x.AssessmentDateTime,
                    Author = x.AssessmentByUserId ?? x.CreateBy,
                    x.PainReassessmentDueAt,
                    x.CompletedAt
                })
                .ToListAsync(cancellationToken);

            var penulisIds = dokumen.Select(x => x.Author).Distinct().ToList();
            var namaPenulis = await _dbContext.Users.AsNoTracking()
                .Where(x => penulisIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);

            var response = new NursingAssessmentProgressResponse { EpisodeId = episodeId, CalculatedAt = now };

            foreach (var (code, label, types) in Bagian)
            {
                var milik = dokumen.Where(x => types.Contains(x.AssessmentType)).ToList();
                var terakhir = milik.OrderByDescending(x => x.AssessmentDateTime).FirstOrDefault();

                string state;

                if (milik.Any(x => x.AssessmentStatus == PatientAssessmentStatus.Completed))
                    state = "Completed";
                else if (milik.Count > 0)
                    state = "NeedsAttention";
                else
                    state = "NotFilled";

                var terlambat = false;

                if (code == "PAIN")
                {
                    // VAL-KEP-36d — penanda, tidak mengubah ✓/!/○.
                    var jatuhTempo = milik
                        .Where(x => x.AssessmentStatus == PatientAssessmentStatus.Completed && x.PainReassessmentDueAt.HasValue)
                        .OrderByDescending(x => x.AssessmentDateTime)
                        .FirstOrDefault();

                    terlambat = jatuhTempo != null &&
                                jatuhTempo.PainReassessmentDueAt!.Value < now &&
                                !milik.Any(x => x.Id != jatuhTempo.Id && x.AssessmentDateTime >= jatuhTempo.PainReassessmentDueAt.Value);
                }

                response.Sections.Add(new NursingAssessmentProgressSection
                {
                    SectionCode = code,
                    Label = label,
                    State = state,
                    StateLabel = state switch { "Completed" => "Selesai", "NeedsAttention" => "Perlu perhatian", _ => "Belum diisi" },
                    LastDocumentId = terakhir?.Id,
                    LastClinicalDateTime = terakhir?.AssessmentDateTime,
                    LastAuthorName = terakhir == null ? null : namaPenulis.GetValueOrDefault(terakhir.Author),
                    ReassessmentOverdue = terlambat
                });
            }

            response.CompletedCount = response.Sections.Count(x => x.State == "Completed");
            response.ProgressPercent = response.CompletedCount * 20;

            // FR-KEP-051 — alert dari dokumen selesai terakhir per bagian yang pitanya ber-alert.
            var terakhirSelesai = dokumen
                .Where(x => x.AssessmentStatus == PatientAssessmentStatus.Completed)
                .GroupBy(x => Bagian.First(b => b.Types.Contains(x.AssessmentType)).Code)
                .Select(g => new { SectionCode = g.Key, Doc = g.OrderByDescending(x => x.AssessmentDateTime).First() })
                .ToList();

            var idSelesai = terakhirSelesai.Select(x => x.Doc.Id).ToList();

            var hasilAlert = await _dbContext.Set<CliAssessmentInstrumentResponse>().AsNoTracking()
                .Where(x => x.AssessmentId.HasValue && idSelesai.Contains(x.AssessmentId.Value) && x.IsAlertBand && !x.IsDelete)
                .Join(_dbContext.Set<CliClinicalInstrumentVersion>().AsNoTracking(), r => r.InstrumentVersionId, v => v.Id, (r, v) => new { r, v.InstrumentId })
                .Join(_dbContext.Set<CliClinicalInstrument>().AsNoTracking(), rv => rv.InstrumentId, i => i.Id, (rv, i) => new
                {
                    AssessmentId = rv.r.AssessmentId!.Value,
                    rv.r.BandCode,
                    rv.r.BandLabelSnapshot,
                    rv.r.TotalScore,
                    i.Name
                })
                .ToListAsync(cancellationToken);

            foreach (var alert in hasilAlert)
            {
                var bagian = terakhirSelesai.First(x => x.Doc.Id == alert.AssessmentId);
                var labelBagian = Bagian.First(b => b.Code == bagian.SectionCode).Label;

                response.Alerts.Add(new NursingClinicalAlert
                {
                    SectionCode = bagian.SectionCode,
                    AssessmentId = alert.AssessmentId,
                    InstrumentName = alert.Name,
                    BandCode = alert.BandCode,
                    BandLabel = alert.BandLabelSnapshot,
                    TotalScore = alert.TotalScore,
                    Message = $"{labelBagian} {alert.BandLabelSnapshot}".Trim()
                });
            }

            response.DailyMonitoringLastRecordedAt = await LastDailyMonitoringAsync(episodeId, cancellationToken);

            var evaluasi = await _dbContext.Set<CliCaseManagementEvaluation>().AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete && x.EvaluationStatus != CaseManagementEvaluationStatus.Cancelled)
                .Select(x => (CaseManagementEvaluationStatus?)x.EvaluationStatus)
                .FirstOrDefaultAsync(cancellationToken);

            response.CaseManagementEvaluationStatus = evaluasi switch
            {
                CaseManagementEvaluationStatus.Completed => "Selesai",
                CaseManagementEvaluationStatus.Draft => "Konsep — milik MPP",
                _ => "Belum diisi — milik MPP"
            };

            return response;
        }

        private async Task<DateTime?> LastDailyMonitoringAsync(Guid episodeId, CancellationToken cancellationToken)
        {
            var tandaVital = await _dbContext.Set<TrxPatientVitalSign>().AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete &&
                            x.VitalSignStatus != PatientVitalSignStatus.Cancelled &&
                            x.VitalSignStatus != PatientVitalSignStatus.EnteredInError)
                .MaxAsync(x => (DateTime?)x.ObservationDateTime, cancellationToken);

            var cairan = await _dbContext.Set<CliFluidBalanceEntry>().AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete && x.EntryStatus == ClinicalMeasurementStatus.Active)
                .MaxAsync(x => (DateTime?)x.EntryDateTime, cancellationToken);

            var gula = await _dbContext.Set<CliBloodGlucoseReading>().AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete && x.ReadingStatus == ClinicalMeasurementStatus.Active)
                .MaxAsync(x => (DateTime?)x.MeasuredAt, cancellationToken);

            var observasi = await _dbContext.Set<CliDailyObservation>().AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete && x.ObservationStatus == ClinicalMeasurementStatus.Active)
                .MaxAsync(x => (DateTime?)x.ObservedAt, cancellationToken);

            var terisi = new[] { tandaVital, cairan, gula, observasi }.Where(x => x.HasValue).ToList();

            return terisi.Count == 0 ? null : terisi.Max();
        }
    }
}
