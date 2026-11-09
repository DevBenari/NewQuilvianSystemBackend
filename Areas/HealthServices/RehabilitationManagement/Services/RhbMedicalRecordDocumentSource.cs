using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services.Sources;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services
{
    /// <summary>
    /// Pembaca dokumen final Rehab Medik untuk berkas rekam medis — <c>BE-RHB-015</c>, INT-RHB-06, RHB-DEC-019
    /// (persetujuan pemilik Rekam Medis, bukti <c>RHB-OQ-012</c>). Pola <c>HmdMedicalRecordDocumentSource</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Hanya membaca. <b>Final menurut Rehab</b>: asesmen, edukasi, sesi, reasesmen, dan ringkasan akhir
    /// berstatus <c>Final</c>; program yang pernah diaktifkan (<c>ActivatedAt</c> terisi — aktivasi adalah tanda
    /// tangannya), termasuk versi yang sudah digantikan, dihentikan, atau selesai. Draft dan yang dibatalkan
    /// tidak pernah dikembalikan.
    /// </para>
    /// <para>
    /// Kepemilikan pasien selalu diperiksa lewat episode (<c>RhbEpisode.PatientId</c>);
    /// <see cref="ReadDetailAsync"/> mengembalikan <c>null</c> untuk dokumen pasien lain. "Pernah diralat" =
    /// baris keutuhan RM punya addendum.
    /// </para>
    /// </remarks>
    public class RhbMedicalRecordDocumentSource : IMedicalRecordDocumentSource
    {
        private const int SummaryMaxLength = 300;

        private readonly ApplicationDbContext _dbContext;

        public RhbMedicalRecordDocumentSource(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string SourceModule => "Rehab Medik";

        public IReadOnlyCollection<ClinicalDocumentKind> Kinds { get; } = RhbDocumentIntegrityGateway.RehabDocumentKinds;

        /// <summary>Baris final bersama semua jenis, sebelum diubah menjadi deskriptor RM.</summary>
        private sealed class FinalRow
        {
            public Guid Id { get; init; }
            public Guid EpisodeId { get; init; }
            public Guid PatientId { get; init; }
            public Guid EncounterId { get; init; }
            public DateTime FinalizedAt { get; init; }
            public DateTime? OccurredAt { get; init; }
            public string? Number { get; init; }
            public string? Discipline { get; init; }
            public int Variant { get; init; }
            public int Variant2 { get; init; }
            public string? Text1 { get; init; }
            public string? Text2 { get; init; }
            public Guid AuthorUserId { get; init; }
        }

        // =====================================================================
        // Daftar
        // =====================================================================

        public async Task<MedicalRecordSourcePage> ReadAsync(MedicalRecordSourceQuery query, CancellationToken cancellationToken)
        {
            var page = new MedicalRecordSourcePage();

            foreach (var kind in Kinds.Where(k => query.Kinds.Contains(k)))
            {
                var rows = Filter(Final(kind, query.PatientId), query.EncounterId, query.StartDate, query.EndDate);
                page.Totals[kind] = await rows.CountAsync(cancellationToken);
                if (query.Limit <= 0)
                    continue;

                var items = await (query.NewestFirst ? rows.OrderByDescending(x => x.FinalizedAt) : rows.OrderBy(x => x.FinalizedAt))
                    .Take(query.Limit)
                    .ToListAsync(cancellationToken);
                var addenda = await AddendumCountsAsync(kind, items.Select(x => x.Id).ToList(), cancellationToken);

                page.Items.AddRange(items.Select(x => new MedicalRecordFinalDocumentDescriptor
                {
                    Kind = kind,
                    DocumentId = x.Id,
                    EncounterId = x.EncounterId,
                    FinalizedAt = x.FinalizedAt,
                    OccurredAt = x.OccurredAt,
                    DocumentNumber = x.Number,
                    Title = Title(kind, x),
                    Summary = Cut(Summary(kind, x)),
                    IsCorrected = addenda.TryGetValue(x.Id, out var count) && count > 0,
                    AuthorUserId = x.AuthorUserId
                }));
            }

            return page;
        }

        // =====================================================================
        // Rincian
        // =====================================================================

        public async Task<MedicalRecordSourceDocumentDetail?> ReadDetailAsync(
            ClinicalDocumentKind kind,
            Guid documentId,
            Guid patientId,
            CancellationToken cancellationToken)
        {
            if (!Kinds.Contains(kind))
                return null;

            var row = await Final(kind, patientId).FirstOrDefaultAsync(x => x.Id == documentId, cancellationToken);
            if (row == null)
                return null;

            var addenda = await AddendumCountsAsync(kind, [row.Id], cancellationToken);
            var sections = new List<MedicalRecordDocumentSectionResponse>();
            Add(sections, "Episode", await _dbContext.RhbEpisodes.AsNoTracking().Where(e => e.Id == row.EpisodeId).Select(e => e.EpisodeNumber).FirstOrDefaultAsync(cancellationToken));
            Add(sections, "Layanan", row.Discipline);

            switch (kind)
            {
                case ClinicalDocumentKind.RehabAssessment:
                    var assessment = await _dbContext.RhbAssessments.AsNoTracking().Where(x => x.Id == documentId).Select(x => new
                    {
                        x.HistoryText, x.PhysicalFunctionalFindingsText, x.RiskAndContraindicationText, x.ProblemOrDiagnosisText,
                        x.PrognosisText, x.ConclusionText, x.PatientGoalText
                    }).FirstAsync(cancellationToken);
                    Add(sections, "Riwayat", assessment.HistoryText);
                    Add(sections, "Temuan fisik dan fungsional", assessment.PhysicalFunctionalFindingsText);
                    Add(sections, "Risiko dan kontraindikasi", assessment.RiskAndContraindicationText);
                    Add(sections, "Masalah / diagnosis", assessment.ProblemOrDiagnosisText);
                    Add(sections, "Prognosis", assessment.PrognosisText);
                    Add(sections, "Kesimpulan", assessment.ConclusionText);
                    Add(sections, "Harapan pasien", assessment.PatientGoalText);
                    var prescriptions = await _dbContext.RhbEpisodeDisciplines.AsNoTracking()
                        .Where(x => x.PrescribedByAssessmentId == documentId && !x.IsDelete)
                        .Select(x => x.Discipline!.DisciplineName + (x.MedicalPrecautionText != null ? " (" + x.MedicalPrecautionText + ")" : ""))
                        .ToListAsync(cancellationToken);
                    Add(sections, "Layanan diresepkan", prescriptions.Count > 0 ? string.Join("; ", prescriptions) : null);
                    Add(sections, "Pengukuran", await MeasurementsAsync(documentId, null, cancellationToken));
                    break;

                case ClinicalDocumentKind.RehabProgram:
                    var program = await _dbContext.RhbPrograms.AsNoTracking().Where(x => x.Id == documentId).Select(x => new
                    {
                        x.ProgramVersion, x.ProgramStatus, x.FrequencyText, x.PlannedSessionCount, x.PrecautionText, x.StartDate,
                        x.EvaluationDueDate, x.ModificationReason, x.StopReason
                    }).FirstAsync(cancellationToken);
                    Add(sections, "Versi", $"v{program.ProgramVersion} — {RhbProgramService.StatusName(program.ProgramStatus)}");
                    Add(sections, "Frekuensi", program.FrequencyText);
                    Add(sections, "Rencana jumlah sesi", program.PlannedSessionCount?.ToString(CultureInfo.InvariantCulture));
                    Add(sections, "Mulai / evaluasi", Join(program.StartDate?.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture), program.EvaluationDueDate?.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture)));
                    Add(sections, "Kewaspadaan", program.PrecautionText);
                    Add(sections, "Alasan modifikasi", program.ModificationReason);
                    Add(sections, "Alasan penghentian", program.StopReason);
                    var interventions = await (
                            from i in _dbContext.RhbProgramInterventions.AsNoTracking()
                            join p in _dbContext.Set<MstProcedure>().AsNoTracking() on i.ProcedureId equals p.Id into procedures
                            from p in procedures.DefaultIfEmpty()
                            where i.ProgramId == documentId && !i.IsDelete
                            select (p != null ? p.ProcedureName : "Tindakan") + ": " + i.ParametersText)
                        .ToListAsync(cancellationToken);
                    Add(sections, "Intervensi", interventions.Count > 0 ? string.Join("; ", interventions) : null);
                    var goals = await _dbContext.RhbProgramGoals.AsNoTracking()
                        .Where(x => x.ProgramId == documentId && !x.IsDelete)
                        .Select(x => x.Goal!.DescriptionText).ToListAsync(cancellationToken);
                    Add(sections, "Tujuan", goals.Count > 0 ? string.Join("; ", goals) : null);
                    break;

                case ClinicalDocumentKind.RehabSession:
                    var session = await _dbContext.RhbSessions.AsNoTracking().Where(x => x.Id == documentId).Select(x => new
                    {
                        x.SessionStatus, x.NotDoneReason, x.CurrentConditionText, x.ContraindicationText, x.StartedAt, x.EndedAt,
                        x.ResponseText, x.ImmediateOutcomeText, x.DeviationType, x.DeviationReasonText, x.ImmediateActionText, x.ContactedPartyText
                    }).FirstAsync(cancellationToken);
                    Add(sections, "Status", Join(RhbSessionService.StatusName(session.SessionStatus),
                        session.NotDoneReason.HasValue ? RhbSessionService.NotDoneReasonName(session.NotDoneReason.Value) : null));
                    Add(sections, "Mulai", Time(session.StartedAt));
                    Add(sections, "Selesai", Time(session.EndedAt));
                    Add(sections, "Kondisi terkini", session.CurrentConditionText);
                    Add(sections, "Kontraindikasi", session.ContraindicationText);
                    var actual = await (
                            from i in _dbContext.RhbSessionInterventions.AsNoTracking()
                            join p in _dbContext.Set<MstProcedure>().AsNoTracking() on i.ProcedureId equals p.Id into procedures
                            from p in procedures.DefaultIfEmpty()
                            where i.SessionId == documentId && !i.IsDelete
                            select (p != null ? p.ProcedureName : "Tindakan") + ": " + i.ActualParametersText)
                        .ToListAsync(cancellationToken);
                    Add(sections, "Tindakan diberikan", actual.Count > 0 ? string.Join("; ", actual) : null);
                    Add(sections, "Respons pasien", session.ResponseText);
                    Add(sections, "Hasil segera / catatan", session.ImmediateOutcomeText);
                    if (session.DeviationType != RehabDeviationType.None)
                    {
                        Add(sections, "Deviasi", Join(RhbSessionService.DeviationName(session.DeviationType), session.DeviationReasonText));
                        Add(sections, "Tindakan segera", session.ImmediateActionText);
                        Add(sections, "Pihak dihubungi", session.ContactedPartyText);
                    }
                    break;

                case ClinicalDocumentKind.RehabProgressReview:
                    var review = await _dbContext.RhbProgressReviews.AsNoTracking().Where(x => x.Id == documentId).Select(x => new
                    {
                        x.ProgressSummaryText, x.BarrierText, x.Decision, x.DecisionReasonText, x.ReferralTargetText, x.InstrumentChangeNote
                    }).FirstAsync(cancellationToken);
                    Add(sections, "Ringkasan kemajuan", review.ProgressSummaryText);
                    Add(sections, "Hambatan", review.BarrierText);
                    Add(sections, "Pengukuran", await MeasurementsAsync(null, documentId, cancellationToken));
                    Add(sections, "Alasan perubahan instrumen", review.InstrumentChangeNote);
                    var goalReviews = await _dbContext.RhbGoalReviews.AsNoTracking()
                        .Where(x => x.ProgressReviewId == documentId && !x.IsDelete)
                        .Select(x => new { x.Goal!.DescriptionText, x.AchievementStatus })
                        .ToListAsync(cancellationToken);
                    Add(sections, "Penilaian tujuan", goalReviews.Count > 0
                        ? string.Join("; ", goalReviews.Select(g => $"{g.DescriptionText}: {RhbGoalService.StatusName(g.AchievementStatus)}"))
                        : null);
                    Add(sections, "Keputusan", review.Decision.HasValue ? Join(RhbProgressReviewService.DecisionName(review.Decision.Value), review.DecisionReasonText) : null);
                    Add(sections, "Tujuan rujukan", review.ReferralTargetText);
                    break;

                case ClinicalDocumentKind.RehabDischargeSummary:
                    var summary = await _dbContext.RhbDischargeSummaries.AsNoTracking().Where(x => x.Id == documentId).Select(x => new
                    {
                        x.ClosureReason, x.MainServicesText, x.GoalAchievementText, x.FinalConditionText, x.FollowUpType,
                        x.FollowUpInstructionText, x.FollowUpTargetText, x.FollowUpDueDate
                    }).FirstAsync(cancellationToken);
                    Add(sections, "Alasan penutupan", summary.ClosureReason.HasValue ? RhbDischargeService.ClosureReasonName(summary.ClosureReason.Value) : null);
                    Add(sections, "Layanan utama", summary.MainServicesText);
                    Add(sections, "Capaian tujuan", summary.GoalAchievementText);
                    Add(sections, "Kondisi akhir", summary.FinalConditionText);
                    Add(sections, "Tindak lanjut", Join(RhbDischargeService.FollowUpName(summary.FollowUpType), summary.FollowUpTargetText,
                        summary.FollowUpDueDate?.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture)));
                    Add(sections, "Instruksi tindak lanjut", summary.FollowUpInstructionText);
                    break;

                case ClinicalDocumentKind.RehabEducation:
                    var education = await _dbContext.RhbEducationRecords.AsNoTracking().Where(x => x.Id == documentId).Select(x => new
                    {
                        x.MaterialText, x.RecipientName, x.RecipientRelation, x.MethodText, x.UnderstandingLevel, x.BarrierText, x.ConsentStatus, x.ConsentNote
                    }).FirstAsync(cancellationToken);
                    Add(sections, "Materi", education.MaterialText);
                    Add(sections, "Penerima", Join(education.RecipientName, education.RecipientRelation));
                    Add(sections, "Metode", education.MethodText);
                    Add(sections, "Pemahaman", RhbEducationService.UnderstandingName(education.UnderstandingLevel));
                    Add(sections, "Hambatan", education.BarrierText);
                    Add(sections, "Persetujuan", Join(RhbEducationService.ConsentName(education.ConsentStatus), education.ConsentNote));
                    break;
            }

            Add(sections, "Disahkan", Time(row.FinalizedAt));
            Add(sections, "Riwayat addendum", addenda.TryGetValue(row.Id, out var count) && count > 0 ? $"{count} addendum" : null);

            return new MedicalRecordSourceDocumentDetail
            {
                EncounterId = row.EncounterId,
                DocumentNumber = row.Number,
                Title = Title(kind, row),
                FinalizedAt = row.FinalizedAt,
                OccurredAt = row.OccurredAt,
                IsCorrected = count > 0,
                Sections = sections
            };
        }

        // =====================================================================
        // Satu-satunya tempat aturan "final" Rehab untuk rekam medis
        // =====================================================================

        private IQueryable<FinalRow> Final(ClinicalDocumentKind kind, Guid patientId) => kind switch
        {
            ClinicalDocumentKind.RehabAssessment => _dbContext.RhbAssessments.AsNoTracking()
                .Where(x => !x.IsDelete && x.RecordStatus == RehabRecordStatus.Final && x.SignedAt != null && x.Episode!.PatientId == patientId)
                .Select(x => new FinalRow
                {
                    Id = x.Id, EpisodeId = x.EpisodeId, PatientId = x.Episode!.PatientId, EncounterId = x.EncounterId,
                    FinalizedAt = x.SignedAt!.Value, OccurredAt = x.AssessedAt,
                    Discipline = x.Discipline != null ? x.Discipline.DisciplineName : null, Variant = (int)x.AssessmentType,
                    Text1 = x.ConclusionText, AuthorUserId = x.AuthorUserId
                }),
            ClinicalDocumentKind.RehabProgram => _dbContext.RhbPrograms.AsNoTracking()
                .Where(x => !x.IsDelete && x.ActivatedAt != null && x.Episode!.PatientId == patientId)
                .Select(x => new FinalRow
                {
                    Id = x.Id, EpisodeId = x.EpisodeId, PatientId = x.Episode!.PatientId, EncounterId = x.AuthoredEncounterId,
                    FinalizedAt = x.ActivatedAt!.Value, Number = "v" + x.ProgramVersion,
                    Discipline = x.Discipline!.DisciplineName, Variant = (int)x.ProgramStatus,
                    Text1 = x.FrequencyText, AuthorUserId = x.AuthorUserId
                }),
            ClinicalDocumentKind.RehabSession => _dbContext.RhbSessions.AsNoTracking()
                .Where(x => !x.IsDelete && x.RecordStatus == RehabRecordStatus.Final && x.SignedAt != null && x.Episode!.PatientId == patientId)
                .Select(x => new FinalRow
                {
                    Id = x.Id, EpisodeId = x.EpisodeId, PatientId = x.Episode!.PatientId, EncounterId = x.EncounterId,
                    FinalizedAt = x.SignedAt!.Value, OccurredAt = x.StartedAt, Number = x.SessionNumber,
                    Discipline = x.Program!.Discipline!.DisciplineName, Variant = (int)x.SessionStatus,
                    Text1 = x.Interventions.Count(i => !i.IsDelete).ToString(), AuthorUserId = x.TherapistUserId
                }),
            ClinicalDocumentKind.RehabProgressReview => _dbContext.RhbProgressReviews.AsNoTracking()
                .Where(x => !x.IsDelete && x.RecordStatus == RehabRecordStatus.Final && x.SignedAt != null && x.Episode!.PatientId == patientId)
                .Select(x => new FinalRow
                {
                    Id = x.Id, EpisodeId = x.EpisodeId, PatientId = x.Episode!.PatientId, EncounterId = x.EncounterId,
                    FinalizedAt = x.SignedAt!.Value, OccurredAt = x.ReviewedAt,
                    Discipline = x.Discipline != null ? x.Discipline.DisciplineName : null, Variant = x.Decision.HasValue ? (int)x.Decision.Value : 0,
                    Text1 = x.DecisionReasonText, AuthorUserId = x.AuthorUserId
                }),
            ClinicalDocumentKind.RehabDischargeSummary => _dbContext.RhbDischargeSummaries.AsNoTracking()
                .Where(x => !x.IsDelete && x.RecordStatus == RehabRecordStatus.Final && x.SignedAt != null && x.Episode!.PatientId == patientId)
                .Select(x => new FinalRow
                {
                    Id = x.Id, EpisodeId = x.EpisodeId, PatientId = x.Episode!.PatientId, EncounterId = x.EncounterId,
                    FinalizedAt = x.SignedAt!.Value, Variant = x.ClosureReason.HasValue ? (int)x.ClosureReason.Value : 0,
                    Variant2 = (int)x.FollowUpType, AuthorUserId = x.AuthorUserId
                }),
            ClinicalDocumentKind.RehabEducation => _dbContext.RhbEducationRecords.AsNoTracking()
                .Where(x => !x.IsDelete && x.RecordStatus == RehabRecordStatus.Final && x.Episode!.PatientId == patientId)
                .Select(x => new FinalRow
                {
                    Id = x.Id, EpisodeId = x.EpisodeId, PatientId = x.Episode!.PatientId, EncounterId = x.EncounterId,
                    // Edukasi tidak menyimpan waktu tanda tangan sendiri; waktunya dari baris keutuhan RM.
                    FinalizedAt = _dbContext.MrcClinicalDocumentIntegrities
                        .Where(k => k.DocumentKind == ClinicalDocumentKind.RehabEducation && k.DocumentId == x.Id && !k.IsDelete)
                        .Select(k => k.SignedAt).FirstOrDefault() ?? x.UpdateDateTime ?? x.EducatedAt,
                    OccurredAt = x.EducatedAt, Variant = (int)x.ConsentStatus,
                    Text1 = x.RecipientRelation, AuthorUserId = x.EducatorUserId
                }),
            _ => Enumerable.Empty<FinalRow>().AsQueryable()
        };

        private static IQueryable<FinalRow> Filter(IQueryable<FinalRow> rows, Guid? encounterId, DateTime? start, DateTime? end)
        {
            if (encounterId.HasValue)
                rows = rows.Where(x => x.EncounterId == encounterId.Value);
            if (start.HasValue)
                rows = rows.Where(x => x.FinalizedAt >= start.Value);
            if (end.HasValue)
                rows = rows.Where(x => x.FinalizedAt <= end.Value);
            return rows;
        }

        private static string Title(ClinicalDocumentKind kind, FinalRow row) => kind switch
        {
            ClinicalDocumentKind.RehabAssessment => row.Variant == (int)RehabAssessmentType.MedicalInitial
                ? "Asesmen Medis Rehab"
                : $"Asesmen Profesi Rehab — {row.Discipline}",
            ClinicalDocumentKind.RehabProgram => $"Program Terapi Rehab — {row.Discipline}",
            ClinicalDocumentKind.RehabSession => $"Sesi Terapi Rehab — {row.Discipline}",
            ClinicalDocumentKind.RehabProgressReview => $"Reasesmen Rehab — {row.Discipline ?? "Medis"}",
            ClinicalDocumentKind.RehabDischargeSummary => "Ringkasan Akhir Rehab",
            ClinicalDocumentKind.RehabEducation => "Edukasi dan Persetujuan Rehab",
            _ => "Dokumen Rehab"
        };

        private static string? Summary(ClinicalDocumentKind kind, FinalRow row) => kind switch
        {
            ClinicalDocumentKind.RehabAssessment => row.Text1,
            ClinicalDocumentKind.RehabProgram => Join(RhbProgramService.StatusName((RehabProgramStatus)row.Variant), row.Text1),
            ClinicalDocumentKind.RehabSession => Join(RhbSessionService.StatusName((RehabSessionStatus)row.Variant), $"{row.Text1} tindakan"),
            ClinicalDocumentKind.RehabProgressReview => row.Variant > 0
                ? Join(RhbProgressReviewService.DecisionName((RehabProgressDecision)row.Variant), row.Text1)
                : row.Text1,
            ClinicalDocumentKind.RehabDischargeSummary => Join(
                row.Variant > 0 ? RhbDischargeService.ClosureReasonName((RehabEpisodeClosureReason)row.Variant) : null,
                RhbDischargeService.FollowUpName((RehabFollowUpType)row.Variant2)),
            ClinicalDocumentKind.RehabEducation => Join(RhbEducationService.ConsentName((RehabConsentStatus)row.Variant), row.Text1),
            _ => null
        };

        private Task<Dictionary<Guid, int>> AddendumCountsAsync(ClinicalDocumentKind kind, List<Guid> ids, CancellationToken cancellationToken) =>
            _dbContext.MrcClinicalDocumentIntegrities.AsNoTracking()
                .Where(k => k.DocumentKind == kind && ids.Contains(k.DocumentId) && !k.IsDelete)
                .GroupBy(k => k.DocumentId)
                .Select(g => new { g.Key, Count = g.Max(k => k.AddendumCount) })
                .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        private async Task<string?> MeasurementsAsync(Guid? assessmentId, Guid? reviewId, CancellationToken cancellationToken)
        {
            var rows = await _dbContext.RhbMeasurements.AsNoTracking()
                .Where(x => !x.IsDelete && (assessmentId != null ? x.AssessmentId == assessmentId : x.ProgressReviewId == reviewId))
                .OrderBy(x => x.MeasuredAt)
                .Select(x => new { x.InstrumentName, x.BodyPartOrDomain, x.NumericValue, x.ValueText, x.Unit })
                .ToListAsync(cancellationToken);
            return rows.Count == 0
                ? null
                : string.Join("; ", rows.Select(x =>
                    $"{x.InstrumentName}{(x.BodyPartOrDomain != null ? " (" + x.BodyPartOrDomain + ")" : "")}: " +
                    $"{x.NumericValue?.ToString(CultureInfo.InvariantCulture) ?? x.ValueText} {x.Unit}".TrimEnd()));
        }

        private static string? Join(params string?[] parts)
        {
            var filled = parts.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            return filled.Count == 0 ? null : string.Join(" — ", filled);
        }

        /// <summary>Waktu tersimpan UTC; ditulis WIB (UTC+7) beserta labelnya, sama dengan pembaca HD.</summary>
        private static string? Time(DateTime? value) =>
            value?.AddHours(7).ToString("dd-MM-yyyy HH:mm 'WIB'", CultureInfo.InvariantCulture);

        private static string? Cut(string? text) =>
            text == null || text.Length <= SummaryMaxLength ? text : text[..SummaryMaxLength];

        private static void Add(List<MedicalRecordDocumentSectionResponse> sections, string label, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                sections.Add(new MedicalRecordDocumentSectionResponse { Label = label, Value = value.Trim() });
        }
    }
}
