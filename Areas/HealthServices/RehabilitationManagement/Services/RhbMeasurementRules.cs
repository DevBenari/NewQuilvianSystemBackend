using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services
{
    /// <summary>Penolakan aturan yang belum terikat ke tipe hasil pemanggil.</summary>
    public sealed record RhbRuleFailure(bool IsBusinessRule, string Code, string Message, string[] Fields, object? Details = null)
    {
        public RhbResult<T> ToResult<T>() => (IsBusinessRule
                ? RhbResult<T>.Rule(Code, Message, Details)
                : RhbResult<T>.Invalid(Code, Message, Details))
            .WithFields(Fields);
    }

    /// <summary>Satu pengukuran yang sudah lolos validasi, siap disimpan.</summary>
    public sealed record RhbMeasurementValue(
        RehabMeasurementSource Source,
        Guid? InstrumentVersionId,
        ClinicalInstrumentKind? InstrumentKind,
        string InstrumentName,
        string? BodyPartOrDomain,
        string? AnswersJson,
        decimal? NumericValue,
        string? ValueText,
        string? Unit,
        string? ScoreBandCode,
        DateTime MeasuredAt);

    /// <summary>
    /// Aturan pengukuran bersama asesmen (<c>BE-RHB-009</c>) dan reasesmen (<c>BE-RHB-014</c>) — V-MEA-01,
    /// RHB-DEC-006/017. Dipindahkan dari <c>RhbAssessmentService</c> tanpa mengubah perilaku.
    /// </summary>
    /// <remarks>
    /// Berinstrumen: versi ada, jenisnya terpasang di konfigurasi layanan (atau asesmen medis bila layanan
    /// kosong), disahkan atau lingkungan mengizinkan konsep, dan jawabannya sah menurut definisi. Jawaban boleh
    /// belum lengkap selama draft; kelengkapan dituntut saat tanda tangan. Terstruktur: nama + nilai wajib.
    /// </remarks>
    public class RhbMeasurementRules
    {
        private static readonly TimeSpan ClockSkewTolerance = TimeSpan.FromMinutes(2);
        public const int MaxMeasurements = 50;

        private readonly ApplicationDbContext _dbContext;
        private readonly ClinicalInstrumentService _instrumentService;

        public RhbMeasurementRules(ApplicationDbContext dbContext, ClinicalInstrumentService instrumentService)
        {
            _dbContext = dbContext;
            _instrumentService = instrumentService;
        }

        public async Task<(RhbRuleFailure? Error, RhbMeasurementValue? Value)> ValidateAsync(
            RehabMeasurementRequest input,
            int index,
            Guid? disciplineId,
            DateTime nowUtc,
            CancellationToken cancellationToken)
        {
            var field = $"measurements[{index}]";
            var measuredAt = input.MeasuredAt.HasValue ? RhbServiceSupport.ToUtc(input.MeasuredAt.Value) : nowUtc;
            if (measuredAt > nowUtc + ClockSkewTolerance)
                return (Invalid(RhbErrorCodes.MeasurementInvalid, "Waktu pengukuran tidak boleh di masa depan.", $"{field}.measuredAt"), null);

            var bodyPart = RhbServiceSupport.Normalize(input.BodyPartOrDomain);
            if (bodyPart?.Length > 150)
                return (TooLong($"{field}.bodyPartOrDomain", 150), null);

            if (input.Source == RehabMeasurementSource.Structured)
            {
                var name = RhbServiceSupport.Normalize(input.InstrumentName);
                var valueText = RhbServiceSupport.Normalize(input.ValueText);
                var unit = RhbServiceSupport.Normalize(input.Unit);
                if (name == null || (!input.NumericValue.HasValue && valueText == null))
                    return (Invalid(RhbErrorCodes.MeasurementInvalid, "Pengukuran terstruktur wajib punya nama dan nilai.", $"{field}.instrumentName", $"{field}.numericValue"), null);
                if (name.Length > 150)
                    return (TooLong($"{field}.instrumentName", 150), null);
                if (valueText?.Length > 200)
                    return (TooLong($"{field}.valueText", 200), null);
                if (unit?.Length > 30)
                    return (TooLong($"{field}.unit", 30), null);

                return (null, new RhbMeasurementValue(RehabMeasurementSource.Structured, null, null, name, bodyPart, null,
                    input.NumericValue, valueText, unit, null, measuredAt));
            }

            if (input.Source != RehabMeasurementSource.Instrument)
                return (Invalid(RhbErrorCodes.MeasurementInvalid, "Jenis pengukuran tidak dikenal.", $"{field}.source"), null);

            if (!input.InstrumentVersionId.HasValue)
                return (InstrumentInvalid(field, "Pilih versi instrumen."), null);

            var version = await (
                    from v in _dbContext.Set<CliClinicalInstrumentVersion>().AsNoTracking()
                    join i in _dbContext.Set<CliClinicalInstrument>().AsNoTracking() on v.InstrumentId equals i.Id
                    where v.Id == input.InstrumentVersionId.Value && !v.IsDelete && !i.IsDelete
                    select new { v.VersionStatus, v.DefinitionJson, i.Name, i.InstrumentKind })
                .FirstOrDefaultAsync(cancellationToken);
            if (version == null)
                return (InstrumentInvalid(field, "Versi instrumen tidak ditemukan."), null);

            var configured = await _dbContext.RhbDisciplineInstruments.AsNoTracking()
                .AnyAsync(x => x.DisciplineId == disciplineId && x.InstrumentKind == version.InstrumentKind && x.IsActive && !x.IsDelete, cancellationToken);
            if (!configured)
                return (InstrumentInvalid(field, "Instrumen ini tidak dipasang untuk layanan asesmen ini."), null);

            if (version.VersionStatus != ClinicalInstrumentVersionStatus.Approved &&
                !(version.VersionStatus == ClinicalInstrumentVersionStatus.Draft && _instrumentService.IsDraftAllowedInThisEnvironment))
                return (InstrumentInvalid(field, "Instrumen belum disahkan atau isian tidak sesuai."), null);

            var definition = ClinicalInstrumentDefinitionEngine.Parse(version.DefinitionJson, out _);
            if (definition == null)
                return (InstrumentInvalid(field, "Definisi instrumen tidak sah."), null);

            var answers = input.Answers ?? new Dictionary<string, JsonElement>(StringComparer.Ordinal);
            var score = ClinicalInstrumentDefinitionEngine.Score(definition, answers);
            if (score.Errors.Count > 0)
                return (new RhbRuleFailure(true, RhbErrorCodes.InstrumentNotApproved, "Instrumen belum disahkan atau isian tidak sesuai.", [$"{field}.answers"], score.Errors), null);

            return (null, new RhbMeasurementValue(
                RehabMeasurementSource.Instrument,
                input.InstrumentVersionId,
                version.InstrumentKind,
                version.Name.Length > 150 ? version.Name[..150] : version.Name,
                bodyPart,
                JsonSerializer.Serialize(answers, ClinicalInstrumentDefinitionEngine.JsonOptions),
                score.TotalScore,
                score.BandLabel,
                score.TotalScore.HasValue ? "skor" : null,
                score.BandCode,
                measuredAt));
        }

        /// <summary>
        /// Saat tanda tangan: pengukuran berinstrumen harus lengkap dan versinya masih boleh dipakai (V-MEA-01) —
        /// versi bisa saja dipensiunkan sejak draft disimpan.
        /// </summary>
        public async Task<RhbRuleFailure?> ValidateStoredInstrumentsAsync(IQueryable<RhbMeasurement> measurements, CancellationToken cancellationToken)
        {
            var rows = await (
                    from m in measurements
                    join v in _dbContext.Set<CliClinicalInstrumentVersion>().AsNoTracking() on m.InstrumentVersionId equals v.Id
                    where !m.IsDelete && m.Source == RehabMeasurementSource.Instrument
                    select new { m.InstrumentName, m.AnswersJson, v.VersionStatus, v.DefinitionJson })
                .ToListAsync(cancellationToken);

            var draftAllowed = _instrumentService.IsDraftAllowedInThisEnvironment;
            foreach (var row in rows)
            {
                if (row.VersionStatus != ClinicalInstrumentVersionStatus.Approved &&
                    !(row.VersionStatus == ClinicalInstrumentVersionStatus.Draft && draftAllowed))
                    return new RhbRuleFailure(true, RhbErrorCodes.InstrumentNotApproved, $"Instrumen {row.InstrumentName} belum disahkan atau isian tidak sesuai.", ["measurements"]);

                var definition = ClinicalInstrumentDefinitionEngine.Parse(row.DefinitionJson, out _);
                if (definition == null)
                    return new RhbRuleFailure(true, RhbErrorCodes.InstrumentNotApproved, $"Definisi instrumen {row.InstrumentName} tidak sah.", ["measurements"]);

                var answers = ClinicalInstrumentDefinitionEngine.ParseResponses(row.AnswersJson);
                var score = ClinicalInstrumentDefinitionEngine.Score(definition, answers);
                var missing = ClinicalInstrumentDefinitionEngine.MissingRequiredItems(definition, answers, _ => false);
                var problems = score.Errors.Concat(score.UnansweredScoredItems.Select(x => $"Isian {x} belum dijawab."))
                    .Concat(missing.Select(x => $"Isian wajib {x} belum diisi.")).Distinct().ToList();
                if (problems.Count > 0)
                    return new RhbRuleFailure(true, RhbErrorCodes.InstrumentNotApproved, $"Isian instrumen {row.InstrumentName} belum lengkap.", ["measurements"], problems);
            }

            return null;
        }

        /// <summary>Baris pengukuran untuk asesmen atau reasesmen (tepat satu pemilik, <c>CK_RhbMeasurement_Owner</c>).</summary>
        public static RhbMeasurement ToEntity(RhbMeasurementValue m, Guid? assessmentId, Guid? progressReviewId, Guid actorUserId, DateTime nowUtc) => new()
        {
            AssessmentId = assessmentId,
            ProgressReviewId = progressReviewId,
            Source = m.Source,
            InstrumentVersionId = m.InstrumentVersionId,
            InstrumentKind = m.InstrumentKind,
            InstrumentName = m.InstrumentName,
            BodyPartOrDomain = m.BodyPartOrDomain,
            AnswersJson = m.AnswersJson,
            NumericValue = m.NumericValue,
            ValueText = m.ValueText,
            Unit = m.Unit,
            ScoreBandCode = m.ScoreBandCode,
            MeasuredAt = m.MeasuredAt,
            CreateDateTime = nowUtc,
            CreateBy = actorUserId
        };

        private static RhbRuleFailure Invalid(string code, string message, params string[] fields) => new(false, code, message, fields);

        private static RhbRuleFailure InstrumentInvalid(string field, string message) =>
            new(true, RhbErrorCodes.InstrumentNotApproved, message, [$"{field}.instrumentVersionId"]);

        private static RhbRuleFailure TooLong(string field, int max) =>
            new(false, RhbErrorCodes.FieldTooLong, $"Isian terlalu panjang (maksimal {max} karakter).", [field]);
    }
}
