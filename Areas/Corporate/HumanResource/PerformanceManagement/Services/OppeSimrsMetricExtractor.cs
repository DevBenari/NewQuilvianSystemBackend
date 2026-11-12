using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Hasil penarikan satu indikator kuantitatif OPPE dari data pelayanan (INT-HR-73).
    /// </summary>
    /// <param name="ActualValue">Nilai terukur; kosong bila sumber datanya tidak tersedia.</param>
    /// <param name="IsDataAvailable">Benar bila ada data pelayanan nyata yang menjadi dasar nilai.</param>
    /// <param name="SourceDescription">Keterangan sumber yang dibaca manusia, dicatat sebagai catatan evaluator.</param>
    /// <param name="EvidenceJson">Bukti mentah (jumlah baris, rentang periode) untuk audit.</param>
    public sealed record OppeMetricExtractionResult(decimal? ActualValue, bool IsDataAvailable, string SourceDescription, string EvidenceJson);

    /// <summary>
    /// Penarik data indikator kuantitatif OPPE langsung dari tabel pelayanan SIMRS yang berjalan di aplikasi ini
    /// (BE-HRD-225, INT-HR-73; remediasi Gelombang 2, 10 November 2026).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Keputusan sumber data.</b> SIMRS tidak berada di sistem terpisah; registrasi, visite, resume pulang, resep,
    /// dan surveilans infeksi luka operasi semuanya tabel di aplikasi ini. Karena itu indikator dihitung dari tabel-tabel
    /// itu per dokter (<c>MstDoctor</c> yang terhubung ke profil tenaga kerja) dalam rentang periode evaluasi:
    /// </para>
    /// <list type="bullet">
    /// <item><c>SIMRS_VOLUME</c>: jumlah kunjungan pasien (<c>RegPatientEncounter</c>) yang ditangani dokter, di luar
    /// draf, batal, dan tidak hadir; ditambah bukti jumlah visite dokter (<c>CliPhysicianVisit</c>).</item>
    /// <item><c>SIMRS_EMR_COMPLETION</c>: persentase resume pulang (<c>InpDischargeSummary</c>) yang ditandatangani dokter
    /// dalam 1x24 jam sejak keputusan pulang episode rawat inap.</item>
    /// <item><c>SIMRS_FORMULARY</c>: persentase butir resep dokter yang sesuai formularium (<c>IsFormularySnapshot</c>).</item>
    /// <item><c>SIMRS_SSI</c> / <c>SIMRS_IDO</c>: persentase kasus operasi dokter bedah yang berujung infeksi daerah operasi
    /// menurut surveilans (<c>CliSurgicalSiteSurveillance</c> dengan infeksi nosokomial tercatat).</item>
    /// </list>
    /// <para>
    /// <b>Yang tidak dikarang.</b> Insiden keselamatan pasien (<c>KMKP_INCIDENT</c>) dan kepatuhan clinical pathway belum
    /// punya tabel sumber di aplikasi; indikator bertipe itu, dan tipe lain yang tidak dikenal, dilaporkan sebagai data
    /// tidak tersedia dan tidak ikut dinilai. Sebelum remediasi, semua tipe diisi konstanta (120; 92,5; 0; 88).
    /// </para>
    /// </remarks>
    public static class OppeSimrsMetricExtractor
    {
        public const string SourceVolume = "SIMRS_VOLUME";
        public const string SourceEmrCompletion = "SIMRS_EMR_COMPLETION";
        public const string SourceFormulary = "SIMRS_FORMULARY";
        public const string SourceSurgicalSiteInfection = "SIMRS_SSI";
        public const string SourceSurgicalSiteInfectionAlias = "SIMRS_IDO";
        public const string SourceKmkpIncident = "KMKP_INCIDENT";

        public static async Task<OppeMetricExtractionResult> ExtractAsync(
            ApplicationDbContext context,
            string dataSourceType,
            Guid workforceProfileId,
            DateTime periodStart,
            DateTime periodEnd,
            CancellationToken cancellationToken)
        {
            var doctorId = await context.MstDoctors
                .AsNoTracking()
                .Where(d => d.WorkforceProfileId == workforceProfileId && !d.IsDelete)
                .Select(d => (Guid?)d.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var periodStartUtc = DateTime.SpecifyKind(periodStart.Date, DateTimeKind.Utc);
            var periodEndUtc = DateTime.SpecifyKind(periodEnd.Date.AddDays(1), DateTimeKind.Utc);

            if (doctorId == null)
            {
                return Unavailable(dataSourceType, "Profil tenaga kerja tidak terhubung ke data dokter (MstDoctor); indikator pelayanan tidak dapat ditarik.", periodStartUtc, periodEndUtc);
            }

            var normalized = (dataSourceType ?? string.Empty).Trim().ToUpperInvariant();
            switch (normalized)
            {
                case SourceVolume:
                    return await ExtractVolumeAsync(context, doctorId.Value, periodStartUtc, periodEndUtc, cancellationToken);
                case SourceEmrCompletion:
                    return await ExtractEmrCompletionAsync(context, doctorId.Value, periodStartUtc, periodEndUtc, cancellationToken);
                case SourceFormulary:
                    return await ExtractFormularyComplianceAsync(context, doctorId.Value, periodStartUtc, periodEndUtc, cancellationToken);
                case SourceSurgicalSiteInfection:
                case SourceSurgicalSiteInfectionAlias:
                    return await ExtractSurgicalSiteInfectionAsync(context, doctorId.Value, periodStartUtc, periodEndUtc, cancellationToken);
                case SourceKmkpIncident:
                    return Unavailable(normalized, "Aplikasi belum memiliki tabel insiden keselamatan pasien (KMKP); indikator menunggu sumber data.", periodStartUtc, periodEndUtc);
                default:
                    return Unavailable(normalized, $"Tipe sumber '{dataSourceType}' belum memiliki penarik data pelayanan; indikator tidak dinilai.", periodStartUtc, periodEndUtc);
            }
        }

        private static async Task<OppeMetricExtractionResult> ExtractVolumeAsync(ApplicationDbContext context, Guid doctorId, DateTime start, DateTime end, CancellationToken ct)
        {
            var encounterCount = await context.RegPatientEncounters
                .AsNoTracking()
                .CountAsync(e => e.DoctorId == doctorId && !e.IsDelete
                                 && e.EncounterDate >= start && e.EncounterDate < end
                                 && e.EncounterStatus != EncounterStatus.Draft
                                 && e.EncounterStatus != EncounterStatus.Cancelled
                                 && e.EncounterStatus != EncounterStatus.NoShow, ct);

            var visitCount = await context.CliPhysicianVisits
                .AsNoTracking()
                .CountAsync(v => v.DoctorId == doctorId && !v.IsDelete
                                 && v.VisitDateTime >= start && v.VisitDateTime < end
                                 && v.VisitStatus != PhysicianVisitStatus.Cancelled, ct);

            var total = encounterCount + visitCount;
            var evidence = JsonSerializer.Serialize(new { source = SourceVolume, doctorId, periodStart = start, periodEnd = end, encounterCount, visitCount });

            if (total == 0)
            {
                return new OppeMetricExtractionResult(null, false, "Tidak ada kunjungan pasien maupun visite yang tercatat atas nama dokter pada periode ini.", evidence);
            }

            return new OppeMetricExtractionResult(total, true, $"Volume pelayanan: {encounterCount} kunjungan rawat jalan/IGD dan {visitCount} visite rawat inap pada periode evaluasi.", evidence);
        }

        private static async Task<OppeMetricExtractionResult> ExtractEmrCompletionAsync(ApplicationDbContext context, Guid doctorId, DateTime start, DateTime end, CancellationToken ct)
        {
            var summaries = await context.InpDischargeSummaries
                .AsNoTracking()
                .Where(s => s.SignedByDoctorId == doctorId && !s.IsDelete && s.SignedAt != null
                            && s.SignedAt >= start && s.SignedAt < end
                            && s.Episode != null && s.Episode.DischargeDecidedAt != null)
                .Select(s => new { SignedAt = s.SignedAt!.Value, DischargeDecidedAt = s.Episode!.DischargeDecidedAt!.Value })
                .ToListAsync(ct);

            var total = summaries.Count;
            var onTime = summaries.Count(s => s.SignedAt <= s.DischargeDecidedAt.AddHours(24));
            var evidence = JsonSerializer.Serialize(new { source = SourceEmrCompletion, doctorId, periodStart = start, periodEnd = end, totalSummaries = total, signedWithin24Hours = onTime });

            if (total == 0)
            {
                return new OppeMetricExtractionResult(null, false, "Tidak ada resume pulang yang ditandatangani dokter pada periode ini.", evidence);
            }

            var percentage = Math.Round(onTime * 100m / total, 2);
            return new OppeMetricExtractionResult(percentage, true, $"Kelengkapan resume pulang 1x24 jam: {onTime} dari {total} resume ditandatangani tepat waktu.", evidence);
        }

        private static async Task<OppeMetricExtractionResult> ExtractFormularyComplianceAsync(ApplicationDbContext context, Guid doctorId, DateTime start, DateTime end, CancellationToken ct)
        {
            var prescriptionIds = context.PhmPrescriptions
                .AsNoTracking()
                .Where(p => p.DoctorId == doctorId && !p.IsDelete
                            && p.PrescriptionDateTime >= start && p.PrescriptionDateTime < end
                            && p.PrescriptionStatus != PrescriptionStatus.Draft
                            && p.PrescriptionStatus != PrescriptionStatus.Cancelled)
                .Select(p => p.Id);

            var items = await context.PhmPrescriptionItems
                .AsNoTracking()
                .Where(i => prescriptionIds.Contains(i.PrescriptionId) && !i.IsDelete)
                .Select(i => i.IsFormularySnapshot)
                .ToListAsync(ct);

            var total = items.Count;
            var compliant = items.Count(x => x);
            var evidence = JsonSerializer.Serialize(new { source = SourceFormulary, doctorId, periodStart = start, periodEnd = end, totalItems = total, formularyItems = compliant });

            if (total == 0)
            {
                return new OppeMetricExtractionResult(null, false, "Tidak ada butir resep dokter yang tercatat pada periode ini.", evidence);
            }

            var percentage = Math.Round(compliant * 100m / total, 2);
            return new OppeMetricExtractionResult(percentage, true, $"Kepatuhan formularium: {compliant} dari {total} butir resep sesuai formularium rumah sakit.", evidence);
        }

        private static async Task<OppeMetricExtractionResult> ExtractSurgicalSiteInfectionAsync(ApplicationDbContext context, Guid doctorId, DateTime start, DateTime end, CancellationToken ct)
        {
            var caseIds = await context.OprCases
                .AsNoTracking()
                .Where(c => c.PrimarySurgeonId == doctorId && !c.IsDelete
                            && c.RequestedAt >= start && c.RequestedAt < end)
                .Select(c => c.Id)
                .ToListAsync(ct);

            var total = caseIds.Count;
            var infected = total == 0
                ? 0
                : await context.CliSurgicalSiteSurveillances
                    .AsNoTracking()
                    .CountAsync(s => caseIds.Contains(s.OprCaseId) && !s.IsDelete && s.NosocomialInfectionId != null, ct);

            var evidence = JsonSerializer.Serialize(new { source = SourceSurgicalSiteInfection, doctorId, periodStart = start, periodEnd = end, surgicalCases = total, surgicalSiteInfections = infected });

            if (total == 0)
            {
                return new OppeMetricExtractionResult(null, false, "Tidak ada kasus operasi dengan dokter sebagai operator utama pada periode ini.", evidence);
            }

            var percentage = Math.Round(infected * 100m / total, 2);
            return new OppeMetricExtractionResult(percentage, true, $"Tingkat infeksi daerah operasi: {infected} dari {total} kasus operasi tercatat infeksi pada surveilans.", evidence);
        }

        private static OppeMetricExtractionResult Unavailable(string source, string reason, DateTime start, DateTime end)
        {
            var evidence = JsonSerializer.Serialize(new { source, periodStart = start, periodEnd = end, available = false, reason });
            return new OppeMetricExtractionResult(null, false, reason, evidence);
        }
    }
}
