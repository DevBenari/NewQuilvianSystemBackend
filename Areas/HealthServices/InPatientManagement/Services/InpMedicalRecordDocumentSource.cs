using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services.Sources;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Pembaca ringkasan pulang dan dokumen admisi rawat inap untuk berkas rekam medis
    /// (`INT-RM-10d`, `BE-30`).
    ///
    /// Ditulis di modul Rawat Inap dengan persetujuan pemiliknya (`RM-DEC-069`). Hanya membaca;
    /// isi dan siklus dokumen tetap milik Rawat Inap, termasuk data revisi ringkasan pulang.
    ///
    /// FINAL MENURUT RAWAT INAP (A4.10):
    /// <list type="bullet">
    /// <item>Ringkasan pulang — final bila <c>SignedAt</c> terisi. Pernah diralat bila punya baris
    /// <c>InpDischargeSummaryRevision</c> (sesi koreksi).</item>
    /// <item>Dokumen admisi — final bila berstatus <c>Completed</c>, waktu <c>CompletedAt</c>.
    /// Versi yang menggantikan versi lama (<c>PreviousVersionId</c> terisi) bertanda pernah diralat.
    /// Versi lama berstatus <c>Superseded</c> tidak tampil.</item>
    /// </list>
    ///
    /// KRITERIA KELENGKAPAN: <c>InpatientGeneralConsent</c> hanya dipenuhi dokumen admisi berjenis
    /// <c>GeneralConsent</c> yang <c>Completed</c> — persetujuan umum Registrasi TIDAK menggantikannya
    /// (`RM-DEC-056`). <c>DischargeSummary</c> dipenuhi ringkasan pulang bertanda tangan.
    /// </summary>
    public class InpMedicalRecordDocumentSource : IMedicalRecordDocumentSource, IMedicalRecordCompletenessProbe
    {
        private const int PanjangRingkasanMaksimal = 300;

        private readonly ApplicationDbContext _dbContext;

        public InpMedicalRecordDocumentSource(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string SourceModule => "Rawat Inap";

        public IReadOnlyCollection<ClinicalDocumentKind> Kinds { get; } =
            [ClinicalDocumentKind.DischargeSummary, ClinicalDocumentKind.AdmissionDocument];

        public IReadOnlyCollection<CompletenessCriterion> Criteria { get; } =
            [CompletenessCriterion.InpatientGeneralConsent, CompletenessCriterion.DischargeSummary];

        public IReadOnlyCollection<CompletenessCondition> Conditions { get; } = [];

        public bool ReportsPendingResults => false;

        public async Task<MedicalRecordSourcePage> ReadAsync(
            MedicalRecordSourceQuery query,
            CancellationToken cancellationToken)
        {
            var hasil = new MedicalRecordSourcePage();
            var dokumen = new List<MedicalRecordFinalDocumentDescriptor>();

            if (query.Kinds.Contains(ClinicalDocumentKind.DischargeSummary))
            {
                var ringkasan = RingkasanFinal(query.PatientId, query.EncounterId, query.StartDate, query.EndDate);
                hasil.Totals[ClinicalDocumentKind.DischargeSummary] = await ringkasan.CountAsync(cancellationToken);

                if (query.Limit > 0)
                {
                    var baris = await (query.NewestFirst
                            ? ringkasan.OrderByDescending(x => x.SignedAt)
                            : ringkasan.OrderBy(x => x.SignedAt))
                        .Take(query.Limit)
                        .Select(x => new
                        {
                            x.Id,
                            x.Episode!.EncounterId,
                            x.Episode.EpisodeNumber,
                            SignedAt = x.SignedAt!.Value,
                            x.PrimaryDiagnosisText,
                            PernahDiralat = _dbContext.InpDischargeSummaryRevisions.Any(r => r.DischargeSummaryId == x.Id && !r.IsDelete)
                        })
                        .ToListAsync(cancellationToken);

                    dokumen.AddRange(baris.Select(x => new MedicalRecordFinalDocumentDescriptor
                    {
                        Kind = ClinicalDocumentKind.DischargeSummary,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        FinalizedAt = x.SignedAt,
                        DocumentNumber = x.EpisodeNumber,
                        Title = "Ringkasan Pulang",
                        Summary = Potong(x.PrimaryDiagnosisText),
                        IsCorrected = x.PernahDiralat
                    }));
                }
            }

            if (query.Kinds.Contains(ClinicalDocumentKind.AdmissionDocument))
            {
                var admisi = DokumenAdmisiFinal(query.PatientId, query.EncounterId, query.StartDate, query.EndDate);
                hasil.Totals[ClinicalDocumentKind.AdmissionDocument] = await admisi.CountAsync(cancellationToken);

                if (query.Limit > 0)
                {
                    var baris = await (query.NewestFirst
                            ? admisi.OrderByDescending(x => x.CompletedAt)
                            : admisi.OrderBy(x => x.CompletedAt))
                        .Take(query.Limit)
                        .Select(x => new
                        {
                            x.Id,
                            EncounterId = x.Episode != null ? (Guid?)x.Episode.EncounterId : null,
                            EpisodeNumber = x.Episode != null ? x.Episode.EpisodeNumber : null,
                            CompletedAt = x.CompletedAt!.Value,
                            x.DocumentType,
                            x.VersionNo,
                            x.PreviousVersionId
                        })
                        .ToListAsync(cancellationToken);

                    dokumen.AddRange(baris.Select(x => new MedicalRecordFinalDocumentDescriptor
                    {
                        Kind = ClinicalDocumentKind.AdmissionDocument,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        FinalizedAt = x.CompletedAt,
                        DocumentNumber = x.EpisodeNumber,
                        Title = NamaDokumenAdmisi(x.DocumentType),
                        Summary = x.VersionNo > 1 ? $"Versi {x.VersionNo}" : null,
                        IsCorrected = x.PreviousVersionId != null
                    }));
                }
            }

            hasil.Items = dokumen;
            return hasil;
        }

        public async Task<MedicalRecordSourceDocumentDetail?> ReadDetailAsync(
            ClinicalDocumentKind kind,
            Guid documentId,
            Guid patientId,
            CancellationToken cancellationToken)
        {
            if (kind == ClinicalDocumentKind.DischargeSummary)
            {
                var x = await RingkasanFinal(patientId, null, null, null)
                    .Where(r => r.Id == documentId)
                    .Select(r => new
                    {
                        r.Episode!.EncounterId,
                        r.Episode.EpisodeNumber,
                        SignedAt = r.SignedAt!.Value,
                        r.PrimaryDiagnosisText,
                        r.SecondaryDiagnosisText,
                        r.ProcedureSummary,
                        r.ClinicalSummary,
                        r.ImportantFindingsSummary,
                        r.DischargeConditionNote,
                        r.DischargeMedicationNote,
                        r.EducationSummary,
                        r.FollowUpInstruction,
                        r.ReferralDestination,
                        Dokter = r.SignedByDoctor != null ? r.SignedByDoctor.FullName : null,
                        Revisi = _dbContext.InpDischargeSummaryRevisions.Count(v => v.DischargeSummaryId == r.Id && !v.IsDelete)
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (x == null)
                    return null;

                var bagian = new List<MedicalRecordDocumentSectionResponse>();
                Tambah(bagian, "Diagnosis utama", x.PrimaryDiagnosisText);
                Tambah(bagian, "Diagnosis sekunder", x.SecondaryDiagnosisText);
                Tambah(bagian, "Tindakan", x.ProcedureSummary);
                Tambah(bagian, "Ringkasan klinis", x.ClinicalSummary);
                Tambah(bagian, "Pemeriksaan penting", x.ImportantFindingsSummary);
                Tambah(bagian, "Kondisi saat pulang", x.DischargeConditionNote);
                Tambah(bagian, "Obat pulang", x.DischargeMedicationNote);
                Tambah(bagian, "Edukasi", x.EducationSummary);
                Tambah(bagian, "Instruksi tindak lanjut", x.FollowUpInstruction);
                Tambah(bagian, "Rujukan", x.ReferralDestination);
                Tambah(bagian, "Ditandatangani", Gabung(TulisWaktu(x.SignedAt), x.Dokter));
                Tambah(bagian, "Riwayat koreksi", x.Revisi > 0 ? $"{x.Revisi} kali dikoreksi" : null);

                return new MedicalRecordSourceDocumentDetail
                {
                    EncounterId = x.EncounterId,
                    DocumentNumber = x.EpisodeNumber,
                    Title = "Ringkasan Pulang",
                    FinalizedAt = x.SignedAt,
                    IsCorrected = x.Revisi > 0,
                    Sections = bagian
                };
            }

            if (kind == ClinicalDocumentKind.AdmissionDocument)
            {
                var x = await DokumenAdmisiFinal(patientId, null, null, null)
                    .Where(d => d.Id == documentId)
                    .Select(d => new
                    {
                        EncounterId = d.Episode != null ? (Guid?)d.Episode.EncounterId : null,
                        EpisodeNumber = d.Episode != null ? d.Episode.EpisodeNumber : null,
                        CompletedAt = d.CompletedAt!.Value,
                        d.DocumentType,
                        d.VersionNo,
                        d.PreviousVersionId,
                        d.CorrectionReason,
                        d.StatementDate,
                        d.SigningCity,
                        d.Note
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (x == null)
                    return null;

                var bagian = new List<MedicalRecordDocumentSectionResponse>();
                Tambah(bagian, "Jenis dokumen", NamaDokumenAdmisi(x.DocumentType));
                Tambah(bagian, "Tanggal pernyataan", x.StatementDate?.AddHours(7).ToString("dd-MM-yyyy", CultureInfo.InvariantCulture));
                Tambah(bagian, "Tempat penandatanganan", x.SigningCity);
                Tambah(bagian, "Catatan", x.Note);
                Tambah(bagian, "Versi", x.VersionNo.ToString(CultureInfo.InvariantCulture));
                Tambah(bagian, "Alasan koreksi", x.CorrectionReason);
                Tambah(bagian, "Selesai", TulisWaktu(x.CompletedAt));

                return new MedicalRecordSourceDocumentDetail
                {
                    EncounterId = x.EncounterId,
                    DocumentNumber = x.EpisodeNumber,
                    Title = NamaDokumenAdmisi(x.DocumentType),
                    FinalizedAt = x.CompletedAt,
                    IsCorrected = x.PreviousVersionId != null,
                    Sections = bagian
                };
            }

            return null;
        }

        public async Task<MedicalRecordCompletenessProbeResult> ProbeAsync(
            MedicalRecordCompletenessProbeQuery query,
            CancellationToken cancellationToken)
        {
            var hasil = new MedicalRecordCompletenessProbeResult();

            if (query.Criteria.Contains(CompletenessCriterion.InpatientGeneralConsent)
                && await DokumenAdmisiFinal(query.PatientId, query.EncounterId, null, null)
                    .AnyAsync(x => x.DocumentType == InpAdmissionDocumentType.GeneralConsent, cancellationToken))
            {
                hasil.SatisfiedCriteria.Add(CompletenessCriterion.InpatientGeneralConsent);
            }

            if (query.Criteria.Contains(CompletenessCriterion.DischargeSummary)
                && await RingkasanFinal(query.PatientId, query.EncounterId, null, null).AnyAsync(cancellationToken))
            {
                hasil.SatisfiedCriteria.Add(CompletenessCriterion.DischargeSummary);
            }

            return hasil;
        }

        // =====================================================================
        // Satu-satunya tempat aturan "final" Rawat Inap untuk rekam medis
        // =====================================================================

        private IQueryable<Models.InpDischargeSummary> RingkasanFinal(
            Guid patientId,
            Guid? encounterId,
            DateTime? dari,
            DateTime? sampai)
        {
            var query = _dbContext.InpDischargeSummaries
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.IsActive
                            && x.SignedAt != null
                            && x.Episode != null
                            && !x.Episode.IsDelete
                            && x.Episode.PatientId == patientId);

            if (encounterId.HasValue)
                query = query.Where(x => x.Episode!.EncounterId == encounterId.Value);
            if (dari.HasValue)
                query = query.Where(x => x.SignedAt >= dari.Value);
            if (sampai.HasValue)
                query = query.Where(x => x.SignedAt <= sampai.Value);

            return query;
        }

        private IQueryable<Models.InpAdmissionDocument> DokumenAdmisiFinal(
            Guid patientId,
            Guid? encounterId,
            DateTime? dari,
            DateTime? sampai)
        {
            var query = _dbContext.InpAdmissionDocuments
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.PatientId == patientId
                            && x.Status == InpAdmissionDocumentStatus.Completed
                            && x.CompletedAt != null);

            if (encounterId.HasValue)
                query = query.Where(x => x.Episode != null && x.Episode.EncounterId == encounterId.Value);
            if (dari.HasValue)
                query = query.Where(x => x.CompletedAt >= dari.Value);
            if (sampai.HasValue)
                query = query.Where(x => x.CompletedAt <= sampai.Value);

            return query;
        }

        public static string NamaDokumenAdmisi(InpAdmissionDocumentType jenis) => jenis switch
        {
            InpAdmissionDocumentType.NewPatientHandover => "Serah Terima Pasien Baru",
            InpAdmissionDocumentType.PrivacyRequest => "Permintaan Privasi",
            InpAdmissionDocumentType.BeliefValues => "Nilai dan Kepercayaan",
            InpAdmissionDocumentType.CostDifferenceStatement => "Pernyataan Selisih Biaya",
            InpAdmissionDocumentType.DepositSettlementStatement => "Pernyataan Deposit",
            InpAdmissionDocumentType.CostEstimate => "Estimasi Biaya",
            InpAdmissionDocumentType.GeneralConsent => "Persetujuan Umum Rawat Inap",
            _ => "Dokumen Admisi"
        };

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
