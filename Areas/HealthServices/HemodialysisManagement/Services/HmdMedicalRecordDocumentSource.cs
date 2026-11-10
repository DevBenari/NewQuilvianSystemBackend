using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services.Sources;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Services
{
    /// <summary>
    /// Pembaca catatan sesi hemodialisa untuk berkas rekam medis (`INT-RM-10f`, `BE-32`).
    ///
    /// Ditulis di modul Hemodialisa dengan persetujuan pemiliknya (`RM-DEC-072`). Hanya membaca;
    /// isi dan siklus sesi tetap milik Hemodialisa, termasuk addendum catatannya.
    ///
    /// FINAL MENURUT HEMODIALISA (A4.10): sesi berstatus <c>Finalized</c> dengan <c>SignedAt</c>
    /// terisi, waktu <c>SignedAt</c>. Pernah diralat bila baris keutuhannya punya addendum
    /// (<c>AddendumCount &gt; 0</c>).
    ///
    /// KONDISI DAN KRITERIA KELENGKAPAN: kondisi <c>HemodialysisSession</c> ada bila kunjungan punya
    /// sesi yang sudah berakhir secara fisik — <c>Stopped</c>, <c>Completed</c>,
    /// <c>AwaitingFinalization</c>, atau <c>Finalized</c>. Sesi yang dihentikan ikut dihitung karena
    /// cuci darah yang sudah berjalan tidak dapat dianggap tidak pernah terjadi (matriks status
    /// Hemodialisa). <c>HemodialysisSessionSigned</c> terpenuhi bila SETIAP sesi itu punya baris
    /// keutuhan <c>Signed</c> — sama dengan tanda tangan yang mengunci sesinya (`RM-DEC-072`).
    /// </summary>
    public class HmdMedicalRecordDocumentSource : IMedicalRecordDocumentSource, IMedicalRecordCompletenessProbe
    {
        private const int PanjangRingkasanMaksimal = 300;

        /// <summary>Sesi yang sudah berakhir secara fisik; batal dan yang belum dimulai tidak termasuk.</summary>
        private static readonly HmdSessionStatus[] StatusBerakhir =
        [
            HmdSessionStatus.Stopped,
            HmdSessionStatus.Completed,
            HmdSessionStatus.AwaitingFinalization,
            HmdSessionStatus.Finalized
        ];

        private readonly ApplicationDbContext _dbContext;

        public HmdMedicalRecordDocumentSource(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string SourceModule => "Hemodialisa";

        public IReadOnlyCollection<ClinicalDocumentKind> Kinds { get; } = [ClinicalDocumentKind.HemodialysisSession];

        public IReadOnlyCollection<CompletenessCriterion> Criteria { get; } = [CompletenessCriterion.HemodialysisSessionSigned];

        public IReadOnlyCollection<CompletenessCondition> Conditions { get; } = [CompletenessCondition.HemodialysisSession];

        public bool ReportsPendingResults => false;

        public async Task<MedicalRecordSourcePage> ReadAsync(
            MedicalRecordSourceQuery query,
            CancellationToken cancellationToken)
        {
            var hasil = new MedicalRecordSourcePage();
            if (!query.Kinds.Contains(ClinicalDocumentKind.HemodialysisSession))
                return hasil;

            var sesi = SesiFinal(query.PatientId, query.EncounterId, query.StartDate, query.EndDate);
            hasil.Totals[ClinicalDocumentKind.HemodialysisSession] = await sesi.CountAsync(cancellationToken);

            if (query.Limit > 0)
            {
                var baris = await (query.NewestFirst
                        ? sesi.OrderByDescending(x => x.SignedAt)
                        : sesi.OrderBy(x => x.SignedAt))
                    .Take(query.Limit)
                    .Select(x => new
                    {
                        x.Id,
                        x.EncounterId,
                        x.SessionNumber,
                        SignedAt = x.SignedAt!.Value,
                        x.StartedAt,
                        x.Shift,
                        x.ActualDurationMinutes,
                        x.ActualUltrafiltrationMl,
                        x.StopReason,
                        x.SignedByUserId,
                        JumlahAddendum = _dbContext.MrcClinicalDocumentIntegrities
                        .Where(k => k.DocumentKind == ClinicalDocumentKind.HemodialysisSession && k.DocumentId == x.Id && !k.IsDelete)
                        .Select(k => k.AddendumCount)
                        .FirstOrDefault()
                    })
                    .ToListAsync(cancellationToken);

                hasil.Items = baris.Select(x => new MedicalRecordFinalDocumentDescriptor
                {
                    Kind = ClinicalDocumentKind.HemodialysisSession,
                    DocumentId = x.Id,
                    EncounterId = x.EncounterId,
                    FinalizedAt = x.SignedAt,
                    OccurredAt = x.StartedAt,
                    DocumentNumber = x.SessionNumber,
                    Title = "Sesi Hemodialisa",
                    Summary = Potong(Ringkasan(x.Shift, x.ActualDurationMinutes, x.ActualUltrafiltrationMl, x.StopReason)),
                    IsCorrected = x.JumlahAddendum > 0,
                    AuthorUserId = x.SignedByUserId
                }).ToList();
            }

            return hasil;
        }

        public async Task<MedicalRecordSourceDocumentDetail?> ReadDetailAsync(
            ClinicalDocumentKind kind,
            Guid documentId,
            Guid patientId,
            CancellationToken cancellationToken)
        {
            if (kind != ClinicalDocumentKind.HemodialysisSession)
                return null;

            var x = await SesiFinal(patientId, null, null, null)
                .Where(s => s.Id == documentId)
                .Select(s => new
                {
                    s.EncounterId,
                    s.SessionNumber,
                    SignedAt = s.SignedAt!.Value,
                    s.ScheduledDate,
                    s.Shift,
                    s.StartedAt,
                    s.EndedAt,
                    s.ActualDurationMinutes,
                    s.ActualUltrafiltrationMl,
                    s.StopReason,
                    s.StopNote,
                    s.DeviationNote,
                    s.Disposition,
                    s.DispositionNote,
                    s.DocumentedAt,
                    // Subquery, bukan navigation: master dokter yang hilang tidak boleh menyembunyikan sesi final.
                    Dokter = _dbContext.MstDoctors.Where(d => d.Id == s.ResponsibleDoctorId).Select(d => d.FullName).FirstOrDefault(),
                    JumlahAddendum = _dbContext.MrcClinicalDocumentIntegrities
                        .Where(k => k.DocumentKind == ClinicalDocumentKind.HemodialysisSession && k.DocumentId == s.Id && !k.IsDelete)
                        .Select(k => k.AddendumCount)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (x == null)
                return null;

            var bagian = new List<MedicalRecordDocumentSectionResponse>();
            Tambah(bagian, "Jadwal", Gabung(x.ScheduledDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture), $"Shift {HmdLabels.Shift(x.Shift)}"));
            Tambah(bagian, "Dokter penanggung jawab", x.Dokter);
            Tambah(bagian, "Mulai", TulisWaktu(x.StartedAt));
            Tambah(bagian, "Selesai", TulisWaktu(x.EndedAt));
            Tambah(bagian, "Durasi", x.ActualDurationMinutes.HasValue ? $"{x.ActualDurationMinutes} menit" : null);
            Tambah(bagian, "Ultrafiltrasi", x.ActualUltrafiltrationMl.HasValue ? $"{x.ActualUltrafiltrationMl} ml" : null);
            Tambah(bagian, "Dihentikan", x.StopReason.HasValue ? Gabung(HmdLabels.StopReason(x.StopReason.Value), x.StopNote) : null);
            Tambah(bagian, "Penyimpangan dari resep", x.DeviationNote);
            Tambah(bagian, "Disposisi", x.Disposition.HasValue ? Gabung(HmdLabels.Disposition(x.Disposition.Value), x.DispositionNote) : null);
            Tambah(bagian, "Didokumentasikan", TulisWaktu(x.DocumentedAt));
            Tambah(bagian, "Disahkan", TulisWaktu(x.SignedAt));
            Tambah(bagian, "Riwayat addendum", x.JumlahAddendum > 0 ? $"{x.JumlahAddendum} addendum" : null);

            return new MedicalRecordSourceDocumentDetail
            {
                EncounterId = x.EncounterId,
                DocumentNumber = x.SessionNumber,
                Title = "Sesi Hemodialisa",
                FinalizedAt = x.SignedAt,
                OccurredAt = x.StartedAt,
                IsCorrected = x.JumlahAddendum > 0,
                Sections = bagian
            };
        }

        public async Task<MedicalRecordCompletenessProbeResult> ProbeAsync(
            MedicalRecordCompletenessProbeQuery query,
            CancellationToken cancellationToken)
        {
            var hasil = new MedicalRecordCompletenessProbeResult();

            var sesiBerakhir = _dbContext.HmdSessions
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.EncounterId == query.EncounterId
                            && x.Episode != null
                            && x.Episode.PatientId == query.PatientId
                            && StatusBerakhir.Contains(x.SessionStatus));

            // Tanpa sesi berakhir, kriteria tidak dianggap terpenuhi: profil RJ-HD menuntut catatan
            // sesi bertanda tangan, dan kunjungan unit HD yang sesinya batal tidak lolos diam-diam.
            if (!await sesiBerakhir.AnyAsync(cancellationToken))
                return hasil;

            hasil.PresentConditions.Add(CompletenessCondition.HemodialysisSession);

            if (query.Criteria.Contains(CompletenessCriterion.HemodialysisSessionSigned)
                && await sesiBerakhir.AllAsync(s => _dbContext.MrcClinicalDocumentIntegrities.Any(k =>
                    k.DocumentKind == ClinicalDocumentKind.HemodialysisSession
                    && k.DocumentId == s.Id
                    && !k.IsDelete
                    && k.IsActive
                    && k.IntegrityStatus == ClinicalDocumentIntegrityStatus.Signed), cancellationToken))
            {
                hasil.SatisfiedCriteria.Add(CompletenessCriterion.HemodialysisSessionSigned);
            }

            return hasil;
        }

        // =====================================================================
        // Satu-satunya tempat aturan "final" Hemodialisa untuk rekam medis
        // =====================================================================

        private IQueryable<HmdSession> SesiFinal(Guid patientId, Guid? encounterId, DateTime? dari, DateTime? sampai)
        {
            var query = _dbContext.HmdSessions
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.SessionStatus == HmdSessionStatus.Finalized
                            && x.SignedAt != null
                            && x.Episode != null
                            && x.Episode.PatientId == patientId);

            if (encounterId.HasValue)
                query = query.Where(x => x.EncounterId == encounterId.Value);
            if (dari.HasValue)
                query = query.Where(x => x.SignedAt >= dari.Value);
            if (sampai.HasValue)
                query = query.Where(x => x.SignedAt <= sampai.Value);

            return query;
        }

        private static string? Ringkasan(HmdShift shift, int? durasi, int? ultrafiltrasi, HmdSessionStopReason? alasanHenti)
        {
            var bagian = new List<string?> { $"Shift {HmdLabels.Shift(shift)}" };
            if (durasi.HasValue)
                bagian.Add($"{durasi} menit");
            if (ultrafiltrasi.HasValue)
                bagian.Add($"UF {ultrafiltrasi} ml");
            if (alasanHenti.HasValue)
                bagian.Add($"Dihentikan: {HmdLabels.StopReason(alasanHenti.Value)}");
            return string.Join(", ", bagian.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        private static string? Gabung(params string?[] bagian)
        {
            var isi = bagian.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            return isi.Count == 0 ? null : string.Join(" — ", isi);
        }

        /// <summary>Waktu tersimpan UTC; ditulis WIB (UTC+7) beserta labelnya (`NFR-005`).</summary>
        private static string? TulisWaktu(DateTime? waktu) =>
            waktu?.AddHours(7).ToString("dd-MM-yyyy HH:mm 'WIB'", CultureInfo.InvariantCulture);

        private static string? Potong(string? teks) =>
            teks == null || teks.Length <= PanjangRingkasanMaksimal ? teks : teks[..PanjangRingkasanMaksimal];

        private static void Tambah(List<MedicalRecordDocumentSectionResponse> bagian, string label, string? nilai)
        {
            if (!string.IsNullOrWhiteSpace(nilai))
                bagian.Add(new MedicalRecordDocumentSectionResponse { Label = label, Value = nilai.Trim() });
        }
    }
}
