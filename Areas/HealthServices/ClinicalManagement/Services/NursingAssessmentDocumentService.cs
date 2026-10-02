using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Dokumen Pengkajian Pasien V2 rawat inap: jawaban instrumen berversi, skor dan pita yang dihitung
    /// server, rujukan tanda vital, dan keadaan nyeri — <c>BE-RWI-109</c>, <c>BE-RWI-110</c>,
    /// <c>BE-RWI-111</c>, state matrix 0.5.0 bagian 5.2.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Berlaku hanya pada dokumen keperawatan rawat inap</b> — pengkajian yang menempel pada episode
    /// dan berjenis Kajian Umum (<c>Initial</c>, <c>Reassessment</c>), <c>FallRisk</c>,
    /// <c>PainMonitoring</c>, <c>EducationAssessment</c>, atau <c>DischargePlanning</c>. Poliklinik,
    /// medical check-up, IGD, dan kajian medis tidak pernah memanggil service ini, sehingga
    /// perhitungan lamanya tidak bergeser (<c>BE-RWI-109</c> kriteria 4).
    /// </para>
    /// <para>
    /// <b>Contoh Resiko Jatuh.</b> Ns. Siti menyimpan Resiko Jatuh Budi 67 tahun dengan "Morse Dewasa v2":
    /// riwayat jatuh Ya (25), diagnosis sekunder Ya (15), alat bantu Tidak ada (0), infus Ya (20), cara
    /// berjalan Normal (0), status mental Sadar (0) → server menghitung 60 → pita Tinggi dari definisi
    /// v2 → <c>FallRiskStatus = HighRisk</c>, <c>FallRiskScore = 60</c>, dan barisnya menyimpan v2
    /// beserta hash definisinya. Tidak ada satu angka batas pun di kode ini.
    /// </para>
    /// <para>
    /// <b>Contoh Kajian Umum.</b> Kondisi Umum menunjuk tanda vital 08.00 lewat <c>VitalSignId</c>. Pukul
    /// 09.00 tanda vital itu dikoreksi (TD 130/80 → 140/85). Detail Kajian Umum membaca baris tanda vital
    /// yang sama, sehingga langsung menampilkan 140/85 — tidak ada salinan yang tertinggal.
    /// </para>
    /// </remarks>
    public class NursingAssessmentDocumentService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ClinicalInstrumentService _instrumentService;

        public NursingAssessmentDocumentService(ApplicationDbContext dbContext, ClinicalInstrumentService instrumentService)
        {
            _dbContext = dbContext;
            _instrumentService = instrumentService;
        }

        /// <summary>Jenis instrumen untuk satu jenis dokumen; <c>null</c> bila bukan dokumen V2.</summary>
        public static ClinicalInstrumentKind? KindFor(PatientAssessmentType type) => type switch
        {
            PatientAssessmentType.Initial or PatientAssessmentType.Reassessment => ClinicalInstrumentKind.GeneralNursingAssessmentForm,
            PatientAssessmentType.FallRisk => ClinicalInstrumentKind.FallRiskScale,
            PatientAssessmentType.PainMonitoring => ClinicalInstrumentKind.PainScale,
            PatientAssessmentType.EducationAssessment => ClinicalInstrumentKind.EducationAssessmentForm,
            PatientAssessmentType.DischargePlanning => ClinicalInstrumentKind.DischargePlanningForm,
            _ => null
        };

        /// <summary><c>true</c> bila pengkajian ini dokumen keperawatan rawat inap V2.</summary>
        public static bool IsInpatientV2(Guid? inpEpisodeId, PatientAssessmentType type) =>
            inpEpisodeId.HasValue && inpEpisodeId.Value != Guid.Empty && KindFor(type).HasValue;

        // =====================================================================
        // Simpan konsep
        // =====================================================================

        /// <summary>
        /// Menerapkan isian V2 pada pengkajian yang sedang dibuat atau diubah — <b>tidak menyimpan</b>;
        /// pemanggil yang memanggil <c>SaveChanges</c> dalam unit kerja yang sama.
        /// </summary>
        public async Task<NursingResult<List<AssessmentInstrumentResultResponse>>> ApplyDraftAsync(
            TrxPatientAssessment entity,
            List<AssessmentInstrumentResponseRequest>? responses,
            Guid? vitalSignId,
            PainAssessmentState painAssessmentState,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var kind = KindFor(entity.AssessmentType)!.Value;
            var now = DateTime.UtcNow;

            if (!Enum.IsDefined(painAssessmentState))
                return Fail(StatusCodes.Status400BadRequest, $"Nilai keadaan nyeri ({(int)painAssessmentState}) tidak dikenal.");

            entity.PainAssessmentState = painAssessmentState;

            // BE-RWI-106 / BR-RWI-007. Kolom HasPain lama diturunkan dari keadaan nyeri, bukan sebaliknya.
            entity.HasPain = painAssessmentState == PainAssessmentState.HasPain;

            // BE-RWI-109 / RWI-FACT-036. Dua centang V2 lama diabaikan pada jalur rawat inap.
            entity.HasFallRisk = false;
            entity.HasAtaxia = false;
            entity.HasPosturalInstability = false;

            // BE-RWI-110 kriteria 3 / VAL-KEP-21d. Hanya Kajian Umum yang menunjuk tanda vital.
            if (kind == ClinicalInstrumentKind.GeneralNursingAssessmentForm)
            {
                if (vitalSignId.HasValue && vitalSignId.Value != Guid.Empty)
                {
                    var tandaVital = await _dbContext.Set<TrxPatientVitalSign>().AsNoTracking()
                        .Where(x => x.Id == vitalSignId.Value && !x.IsDelete)
                        .Select(x => new { x.InpEpisodeId, x.EncounterId, x.PatientId, x.VitalSignStatus })
                        .FirstOrDefaultAsync(cancellationToken);

                    var milikPasien = tandaVital != null &&
                                      tandaVital.PatientId == entity.PatientId &&
                                      (tandaVital.InpEpisodeId == entity.InpEpisodeId ||
                                       (!tandaVital.InpEpisodeId.HasValue && tandaVital.EncounterId == entity.EncounterId));

                    if (!milikPasien ||
                        tandaVital!.VitalSignStatus is PatientVitalSignStatus.Cancelled or PatientVitalSignStatus.EnteredInError)
                    {
                        return Fail(StatusCodes.Status400BadRequest, "Tanda vital yang dipilih bukan milik pasien ini.", "VITAL_SIGN_NOT_OWNED");
                    }

                    entity.VitalSignId = vitalSignId.Value;
                }
                else
                {
                    entity.VitalSignId = null;
                }
            }
            else
            {
                entity.VitalSignId = null;
            }

            var hasil = new List<AssessmentInstrumentResultResponse>();
            var daftar = (responses ?? new()).Where(x => x.InstrumentVersionId != Guid.Empty).ToList();

            if (daftar.Count > 1)
                return Fail(StatusCodes.Status400BadRequest, "Satu dokumen pengkajian hanya memakai satu instrumen.");

            ClinicalInstrumentDefinition? definisiDipakai = null;

            if (daftar.Count == 1)
            {
                var kiriman = daftar[0];
                var berlaku = await _instrumentService.ResolveForPatientAsync(kind, entity.PatientId, now, cancellationToken);

                if (!berlaku.IsSuccess)
                    return berlaku.Cast<List<AssessmentInstrumentResultResponse>>();

                // VAL-KEP-21c. Versi yang dikirim wajib versi yang berlaku saat ini bagi usia pasien.
                if (berlaku.Value!.VersionId != kiriman.InstrumentVersionId)
                {
                    return Fail(StatusCodes.Status409Conflict,
                        "Formulir sudah diganti versi baru. Muat ulang formulir; isian Anda tetap tersimpan di layar.",
                        "INSTRUMENT_VERSION_CHANGED");
                }

                var versi = await _dbContext.Set<CliClinicalInstrumentVersion>().AsNoTracking()
                    .FirstAsync(x => x.Id == kiriman.InstrumentVersionId, cancellationToken);

                var definisi = ClinicalInstrumentDefinitionEngine.Parse(versi.DefinitionJson, out var galat);

                if (definisi == null)
                    return Fail(StatusCodes.Status400BadRequest, galat ?? "Definisi instrumen tidak dapat dibaca.");

                var jawaban = kiriman.Responses ?? new Dictionary<string, JsonElement>();
                var skor = ClinicalInstrumentDefinitionEngine.Score(definisi, jawaban);

                if (skor.Errors.Count > 0)
                    return Fail(StatusCodes.Status400BadRequest, "Pita instrumen tidak dapat dihitung dari jawaban: " + string.Join(" ", skor.Errors));

                definisiDipakai = definisi;

                var baris = await FindResponseRowAsync(entity.Id, cancellationToken);

                if (baris == null)
                {
                    baris = new CliAssessmentInstrumentResponse
                    {
                        Id = Guid.NewGuid(),
                        AssessmentId = entity.Id,
                        CreateDateTime = now,
                        CreateBy = actorUserId
                    };

                    _dbContext.Set<CliAssessmentInstrumentResponse>().Add(baris);
                }
                else
                {
                    baris.UpdateDateTime = now;
                    baris.UpdateBy = actorUserId;
                }

                baris.InstrumentVersionId = versi.Id;
                baris.DefinitionHashSnapshot = versi.DefinitionHash;
                baris.ResponsesJson = JsonSerializer.Serialize(jawaban, ClinicalInstrumentDefinitionEngine.JsonOptions);
                baris.TotalScore = skor.TotalScore;
                baris.BandCode = skor.BandCode;
                baris.BandLabelSnapshot = skor.BandLabel;
                baris.IsAlertBand = skor.IsAlertBand;
                baris.ComputedAt = skor.TotalScore.HasValue ? now : null;

                // BE-RWI-109 kriteria 1. Kategori risiko jatuh diisi dari pita definisi, bukan dari angka di kode.
                if (kind == ClinicalInstrumentKind.FallRiskScale)
                {
                    entity.FallRiskScore = skor.TotalScore.HasValue ? (int)Math.Round(skor.TotalScore.Value, MidpointRounding.AwayFromZero) : null;
                    entity.FallRiskStatus = skor.MappedFallRiskStatus ?? FallRiskStatus.Unknown;
                }

                hasil.Add(ToResult(baris, versi, berlaku.Value.InstrumentName, berlaku.Value.InstrumentKind, versi.VersionNumber));
            }
            else if (kind == ClinicalInstrumentKind.FallRiskScale)
            {
                // Tanpa jawaban instrumen tidak ada skor — "belum dikaji", bukan "tidak berisiko".
                var barisLama = await FindResponseRowAsync(entity.Id, cancellationToken);

                if (barisLama == null)
                {
                    entity.FallRiskScore = null;
                    entity.FallRiskStatus = FallRiskStatus.Unknown;
                }
            }

            // BE-RWI-111 kriteria 3. Waktu kajian ulang nyeri dari interval instrumen nyeri.
            if (kind == ClinicalInstrumentKind.PainScale)
            {
                definisiDipakai ??= await LoadDefinitionForPainAsync(entity, now, cancellationToken);

                entity.PainReassessmentDueAt =
                    painAssessmentState == PainAssessmentState.HasPain && definisiDipakai?.ReassessmentMinutes is int menit
                        ? entity.AssessmentDateTime.AddMinutes(menit)
                        : null;
            }

            return NursingResult<List<AssessmentInstrumentResultResponse>>.Ok(hasil, string.Empty);
        }

        // =====================================================================
        // Selesaikan
        // =====================================================================

        /// <summary>
        /// Penjaga penyelesaian dokumen V2 — <c>VAL-KEP-21a</c>, <c>VAL-KEP-21b</c>, <c>VAL-KEP-22a</c>,
        /// <c>VAL-KEP-22b</c>. Tidak ada daftar isian wajib tetap di sini; seluruhnya dari versi instrumen.
        /// </summary>
        public async Task<NursingResult<bool>> EnsureCanCompleteAsync(TrxPatientAssessment entity, CancellationToken cancellationToken = default)
        {
            var kind = KindFor(entity.AssessmentType)!.Value;

            if (kind == ClinicalInstrumentKind.PainScale)
            {
                if (entity.PainAssessmentState == PainAssessmentState.NotAssessed)
                    return FailBool(StatusCodes.Status422UnprocessableEntity, "Pilih keadaan nyeri: tidak nyeri, nyeri, atau tidak dapat dinilai.", "PAIN_STATE_REQUIRED");

                if (entity.PainAssessmentState == PainAssessmentState.HasPain && !entity.PainScale.HasValue)
                    return FailBool(StatusCodes.Status400BadRequest, "Isi skala nyeri sesuai instrumen yang dipakai.", "PAIN_SCALE_REQUIRED");
            }

            var baris = await FindResponseRowAsync(entity.Id, cancellationToken);
            CliClinicalInstrumentVersion? versi;
            Dictionary<string, JsonElement> jawaban;

            if (baris != null)
            {
                versi = await _dbContext.Set<CliClinicalInstrumentVersion>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == baris.InstrumentVersionId, cancellationToken);
                jawaban = ClinicalInstrumentDefinitionEngine.ParseResponses(baris.ResponsesJson);
            }
            else
            {
                var berlaku = await _instrumentService.ResolveForPatientAsync(kind, entity.PatientId, DateTime.UtcNow, cancellationToken);

                if (!berlaku.IsSuccess)
                {
                    return FailBool(StatusCodes.Status422UnprocessableEntity,
                        (berlaku.ErrorMessage ?? "Instrumen belum tersedia.") + " Dokumen dapat disimpan sebagai konsep.",
                        "INSTRUMENT_NOT_AVAILABLE");
                }

                versi = await _dbContext.Set<CliClinicalInstrumentVersion>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == berlaku.Value!.VersionId, cancellationToken);
                jawaban = new Dictionary<string, JsonElement>();
            }

            if (versi == null)
                return FailBool(StatusCodes.Status422UnprocessableEntity, "Instrumen belum tersedia. Dokumen dapat disimpan sebagai konsep.", "INSTRUMENT_NOT_AVAILABLE");

            if (versi.VersionStatus == ClinicalInstrumentVersionStatus.Retired)
            {
                return FailBool(StatusCodes.Status409Conflict,
                    "Formulir sudah diganti versi baru. Muat ulang formulir; isian Anda tetap tersimpan di layar.",
                    "INSTRUMENT_VERSION_CHANGED");
            }

            // VAL-KEP-21a / RWI-DEC-124 butir 5.
            if (versi.VersionStatus != ClinicalInstrumentVersionStatus.Approved && !_instrumentService.IsDraftAllowedInThisEnvironment)
            {
                return FailBool(StatusCodes.Status422UnprocessableEntity,
                    "Instrumen belum disahkan. Dokumen dapat disimpan sebagai konsep dan diselesaikan setelah pengesahan.",
                    "INSTRUMENT_NOT_APPROVED");
            }

            var definisi = ClinicalInstrumentDefinitionEngine.Parse(versi.DefinitionJson, out var galat);

            if (definisi == null)
                return FailBool(StatusCodes.Status422UnprocessableEntity, galat ?? "Definisi instrumen tidak dapat dibaca.");

            // VAL-KEP-21b — isian wajib versi itu, termasuk isian yang disimpan pada kolom.
            var kosong = ClinicalInstrumentDefinitionEngine.MissingRequiredItems(definisi, jawaban, kolom => IsColumnFilled(entity, kolom));

            // Isian berskor yang belum dijawab tidak pernah dihitung nol.
            var skor = ClinicalInstrumentDefinitionEngine.Score(definisi, jawaban);
            kosong.AddRange(skor.UnansweredScoredItems.Where(x => !kosong.Contains(x)));

            if (kosong.Count > 0)
                return FailBool(StatusCodes.Status422UnprocessableEntity, $"Isian wajib belum lengkap: {string.Join(", ", kosong)}.", "REQUIRED_ITEMS_MISSING");

            if (definisi.Scoring != null && !skor.TotalScore.HasValue)
                return FailBool(StatusCodes.Status422UnprocessableEntity, "Skor instrumen belum dapat dihitung. " + string.Join(" ", skor.Errors), "SCORE_NOT_COMPUTED");

            return NursingResult<bool>.Ok(true, string.Empty);
        }

        // =====================================================================
        // Baca
        // =====================================================================

        public async Task<List<AssessmentInstrumentResultResponse>> GetResultsAsync(Guid assessmentId, CancellationToken cancellationToken = default)
        {
            var baris = await _dbContext.Set<CliAssessmentInstrumentResponse>().AsNoTracking()
                .Where(x => x.AssessmentId == assessmentId && !x.IsDelete)
                .Join(_dbContext.Set<CliClinicalInstrumentVersion>().AsNoTracking(), r => r.InstrumentVersionId, v => v.Id, (r, v) => new { r, v })
                .Join(_dbContext.Set<CliClinicalInstrument>().AsNoTracking(), rv => rv.v.InstrumentId, i => i.Id, (rv, i) => new { rv.r, rv.v, i })
                .ToListAsync(cancellationToken);

            return baris.Select(x => ToResult(x.r, x.v, x.i.Name, x.i.InstrumentKind, x.v.VersionNumber)).ToList();
        }

        public async Task<PatientAssessmentVitalSignReference?> GetVitalSignReferenceAsync(Guid? vitalSignId, CancellationToken cancellationToken = default)
        {
            if (!vitalSignId.HasValue)
                return null;

            return await _dbContext.Set<TrxPatientVitalSign>().AsNoTracking()
                .Where(x => x.Id == vitalSignId.Value)
                .Select(x => new PatientAssessmentVitalSignReference
                {
                    Id = x.Id,
                    ObservationDateTime = x.ObservationDateTime,
                    VitalSignStatus = x.VitalSignStatus,
                    BloodPressureSystolic = x.BloodPressureSystolic,
                    BloodPressureDiastolic = x.BloodPressureDiastolic,
                    PulseRate = x.PulseRate,
                    RespiratoryRate = x.RespiratoryRate,
                    Temperature = x.Temperature,
                    OxygenSaturation = x.OxygenSaturation,
                    ConsciousnessStatus = x.ConsciousnessStatus,
                    UpdateDateTime = x.UpdateDateTime
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>Melengkapi balasan detail dengan hasil instrumen dan tanda vital yang ditunjuk.</summary>
        public async Task EnrichDetailAsync(PatientAssessmentDetailResponse detail, CancellationToken cancellationToken = default)
        {
            detail.InstrumentResults = await GetResultsAsync(detail.Id, cancellationToken);
            detail.ReferencedVitalSign = await GetVitalSignReferenceAsync(detail.VitalSignId, cancellationToken);
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        private async Task<CliAssessmentInstrumentResponse?> FindResponseRowAsync(Guid assessmentId, CancellationToken cancellationToken)
        {
            var lokal = _dbContext.Set<CliAssessmentInstrumentResponse>().Local
                .FirstOrDefault(x => x.AssessmentId == assessmentId && !x.IsDelete);

            return lokal ?? await _dbContext.Set<CliAssessmentInstrumentResponse>()
                .FirstOrDefaultAsync(x => x.AssessmentId == assessmentId && !x.IsDelete, cancellationToken);
        }

        private async Task<ClinicalInstrumentDefinition?> LoadDefinitionForPainAsync(TrxPatientAssessment entity, DateTime now, CancellationToken cancellationToken)
        {
            var baris = await FindResponseRowAsync(entity.Id, cancellationToken);
            string? json = null;

            if (baris != null)
            {
                json = await _dbContext.Set<CliClinicalInstrumentVersion>().AsNoTracking()
                    .Where(x => x.Id == baris.InstrumentVersionId)
                    .Select(x => x.DefinitionJson)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            else
            {
                var berlaku = await _instrumentService.ResolveForPatientAsync(ClinicalInstrumentKind.PainScale, entity.PatientId, now, cancellationToken);

                if (berlaku.IsSuccess)
                    json = berlaku.Value!.Definition.GetRawText();
            }

            return json == null ? null : ClinicalInstrumentDefinitionEngine.Parse(json, out _);
        }

        private static bool IsColumnFilled(TrxPatientAssessment entity, string column)
        {
            var nilai = typeof(TrxPatientAssessment).GetProperty(column)?.GetValue(entity);

            return nilai switch
            {
                null => false,
                string teks => !string.IsNullOrWhiteSpace(teks),
                Enum pilihan => Convert.ToInt32(pilihan) != 0,
                _ => true
            };
        }

        private static AssessmentInstrumentResultResponse ToResult(
            CliAssessmentInstrumentResponse baris,
            CliClinicalInstrumentVersion versi,
            string instrumentName,
            ClinicalInstrumentKind kind,
            int versionNumber)
        {
            using var dokumen = JsonDocument.Parse(string.IsNullOrWhiteSpace(baris.ResponsesJson) ? "{}" : baris.ResponsesJson);

            return new AssessmentInstrumentResultResponse
            {
                Id = baris.Id,
                InstrumentVersionId = baris.InstrumentVersionId,
                InstrumentName = instrumentName,
                InstrumentKind = kind,
                VersionNumber = versionNumber,
                VersionStatus = versi.VersionStatus,
                DefinitionHashSnapshot = baris.DefinitionHashSnapshot,
                Responses = dokumen.RootElement.Clone(),
                TotalScore = baris.TotalScore,
                BandCode = baris.BandCode,
                BandLabel = baris.BandLabelSnapshot,
                IsAlertBand = baris.IsAlertBand,
                ComputedAt = baris.ComputedAt
            };
        }

        private static NursingResult<List<AssessmentInstrumentResultResponse>> Fail(int statusCode, string message, string? code = null) =>
            NursingResult<List<AssessmentInstrumentResultResponse>>.Fail(statusCode, message, code);

        private static NursingResult<bool> FailBool(int statusCode, string message, string? code = null) =>
            NursingResult<bool>.Fail(statusCode, message, code);
    }
}
