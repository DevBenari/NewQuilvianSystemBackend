using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services.Sources;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    /// <summary>
    /// Pembaca triase, observasi, resusitasi, dan disposisi IGD untuk berkas rekam medis
    /// (`INT-RM-10c`, `BE-29`).
    ///
    /// Ditulis di modul IGD dengan persetujuan pemiliknya (`RM-DEC-067`). Hanya membaca; isi dan
    /// siklus dokumen tetap milik IGD. Kunjungan IGD yang belum tertaut ke pasien
    /// (<c>EmgVisit.PatientId</c> kosong, pasien tanpa identitas) tidak punya berkas.
    ///
    /// FINAL MENURUT IGD (A4.10, `RM-DEC-067`):
    /// <list type="bullet">
    /// <item>Triase — <c>Completed</c>, waktu <c>CompletedAt</c>. Triase yang digantikan triase ulang
    /// (<c>Superseded</c>) TETAP tampil sebagai triase yang digantikan, bertanda pernah diralat.</item>
    /// <item>Observasi — <c>Completed</c> atau <c>Escalated</c>, waktu <c>EndedAt</c>.</item>
    /// <item>Resusitasi — <c>Completed</c> atau <c>Stopped</c>, waktu <c>CompletedAt</c>.</item>
    /// <item>Disposisi — <c>Confirmed</c>, waktu <c>ConfirmedAt</c>. <c>Executed</c> adalah status
    /// pelaksanaan sesudah dikonfirmasi, bukan syarat tambahan; disposisi <c>Draft</c> tidak tampil.</item>
    /// </list>
    ///
    /// KRITERIA KELENGKAPAN: <c>EmergencyTriage</c> dipenuhi triase <c>Completed</c>;
    /// <c>EmergencyDisposition</c> dipenuhi disposisi yang sudah dikonfirmasi.
    /// </summary>
    public class EmgMedicalRecordDocumentSource : IMedicalRecordDocumentSource, IMedicalRecordCompletenessProbe
    {
        private const int PanjangRingkasanMaksimal = 300;

        private readonly ApplicationDbContext _dbContext;

        public EmgMedicalRecordDocumentSource(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string SourceModule => "IGD";

        public IReadOnlyCollection<ClinicalDocumentKind> Kinds { get; } =
        [
            ClinicalDocumentKind.EmergencyTriage,
            ClinicalDocumentKind.EmergencyObservation,
            ClinicalDocumentKind.EmergencyResuscitation,
            ClinicalDocumentKind.EmergencyDisposition
        ];

        public IReadOnlyCollection<CompletenessCriterion> Criteria { get; } =
            [CompletenessCriterion.EmergencyTriage, CompletenessCriterion.EmergencyDisposition];

        public IReadOnlyCollection<CompletenessCondition> Conditions { get; } = [];

        public bool ReportsPendingResults => false;

        public async Task<MedicalRecordSourcePage> ReadAsync(
            MedicalRecordSourceQuery query,
            CancellationToken cancellationToken)
        {
            var hasil = new MedicalRecordSourcePage();

            foreach (var jenis in Kinds.Where(query.Kinds.Contains))
            {
                var sumber = Saring(Baris(jenis, query.PatientId, query.EncounterId), query.StartDate, query.EndDate);

                hasil.Totals[jenis] = await sumber.CountAsync(cancellationToken);
                if (query.Limit <= 0)
                    continue;

                var baris = await (query.NewestFirst
                        ? sumber.OrderByDescending(x => x.FinalizedAt)
                        : sumber.OrderBy(x => x.FinalizedAt))
                    .Take(query.Limit)
                    .ToListAsync(cancellationToken);

                hasil.Items.AddRange(baris.Select(x => new MedicalRecordFinalDocumentDescriptor
                {
                    Kind = jenis,
                    DocumentId = x.Id,
                    EncounterId = x.EncounterId,
                    FinalizedAt = x.FinalizedAt,
                    DocumentNumber = x.DocumentNumber,
                    Title = x.Title,
                    Summary = Potong(x.Summary),
                    IsCorrected = x.IsCorrected,
                    AuthorUserId = x.AuthorUserId
                }));
            }

            return hasil;
        }

        public async Task<MedicalRecordSourceDocumentDetail?> ReadDetailAsync(
            ClinicalDocumentKind kind,
            Guid documentId,
            Guid patientId,
            CancellationToken cancellationToken)
        {
            if (!Kinds.Contains(kind))
                return null;

            var ringkas = await Baris(kind, patientId, null).FirstOrDefaultAsync(x => x.Id == documentId, cancellationToken);
            if (ringkas == null)
                return null;

            var bagian = new List<MedicalRecordDocumentSectionResponse>();

            switch (kind)
            {
                case ClinicalDocumentKind.EmergencyTriage:
                {
                    var x = await _dbContext.EmgTriages.AsNoTracking()
                        .Where(t => t.Id == documentId)
                        .Select(t => new
                        {
                            Level = t.TriageLevel != null ? t.TriageLevel.Name : null,
                            t.TriageSystem, t.TriageStatus, t.IsRetriage, t.TriageReason,
                            t.AirwaySummary, t.BreathingSummary, t.CirculationSummary,
                            t.DisabilitySummary, t.ExposureSummary, t.RedFlagSummary, t.Notes
                        })
                        .FirstAsync(cancellationToken);

                    Tambah(bagian, "Keadaan", x.TriageStatus == EmergencyTriageStatus.Superseded ? "Digantikan triase ulang." : null);
                    Tambah(bagian, "Tingkat triase", x.Level);
                    Tambah(bagian, "Sistem triase", x.TriageSystem.ToString());
                    Tambah(bagian, "Triase ulang", x.IsRetriage ? "Ya" : null);
                    Tambah(bagian, "Alasan", x.TriageReason);
                    Tambah(bagian, "Airway", x.AirwaySummary);
                    Tambah(bagian, "Breathing", x.BreathingSummary);
                    Tambah(bagian, "Circulation", x.CirculationSummary);
                    Tambah(bagian, "Disability", x.DisabilitySummary);
                    Tambah(bagian, "Exposure", x.ExposureSummary);
                    Tambah(bagian, "Tanda bahaya", x.RedFlagSummary);
                    Tambah(bagian, "Catatan", x.Notes);
                    break;
                }
                case ClinicalDocumentKind.EmergencyObservation:
                {
                    var x = await _dbContext.EmgObservations.AsNoTracking()
                        .Where(o => o.Id == documentId)
                        .Select(o => new
                        {
                            o.ObservationStatus, o.StartedAt, o.ObservationLocation, o.Indication,
                            o.ObservationPlan, o.CompletionSummary, o.EscalationReason
                        })
                        .FirstAsync(cancellationToken);

                    Tambah(bagian, "Keadaan", x.ObservationStatus == EmergencyObservationStatus.Escalated ? "Dieskalasi" : "Selesai");
                    Tambah(bagian, "Mulai", TulisWaktu(x.StartedAt));
                    Tambah(bagian, "Lokasi", x.ObservationLocation);
                    Tambah(bagian, "Indikasi", x.Indication);
                    Tambah(bagian, "Rencana observasi", x.ObservationPlan);
                    Tambah(bagian, "Ringkasan selesai", x.CompletionSummary);
                    Tambah(bagian, "Alasan eskalasi", x.EscalationReason);
                    break;
                }
                case ClinicalDocumentKind.EmergencyResuscitation:
                {
                    var x = await _dbContext.EmgResuscitations.AsNoTracking()
                        .Where(r => r.Id == documentId)
                        .Select(r => new
                        {
                            r.ResuscitationStatus, r.StartedAt, r.Location, r.TriggerCondition,
                            r.WasCardiopulmonaryResuscitationPerformed, r.ReturnOfSpontaneousCirculationAt, r.DefibrillationCount,
                            r.AirwayManagementSummary, r.BreathingManagementSummary, r.CirculationManagementSummary,
                            r.NeurologicalManagementSummary, r.OutcomeSummary, r.Notes
                        })
                        .FirstAsync(cancellationToken);

                    Tambah(bagian, "Keadaan", x.ResuscitationStatus == EmergencyResuscitationStatus.Stopped ? "Dihentikan" : "Selesai");
                    Tambah(bagian, "Mulai", TulisWaktu(x.StartedAt));
                    Tambah(bagian, "Lokasi", x.Location);
                    Tambah(bagian, "Pemicu", x.TriggerCondition);
                    Tambah(bagian, "RJP dilakukan", x.WasCardiopulmonaryResuscitationPerformed ? "Ya" : "Tidak");
                    Tambah(bagian, "ROSC", TulisWaktu(x.ReturnOfSpontaneousCirculationAt));
                    Tambah(bagian, "Defibrilasi", x.DefibrillationCount > 0 ? $"{x.DefibrillationCount} kali" : null);
                    Tambah(bagian, "Airway", x.AirwayManagementSummary);
                    Tambah(bagian, "Breathing", x.BreathingManagementSummary);
                    Tambah(bagian, "Circulation", x.CirculationManagementSummary);
                    Tambah(bagian, "Neurologis", x.NeurologicalManagementSummary);
                    Tambah(bagian, "Hasil", x.OutcomeSummary);
                    Tambah(bagian, "Catatan", x.Notes);
                    break;
                }
                case ClinicalDocumentKind.EmergencyDisposition:
                {
                    var x = await _dbContext.EmgDispositions.AsNoTracking()
                        .Where(d => d.Id == documentId)
                        .Select(d => new
                        {
                            Jenis = d.DispositionType != null ? d.DispositionType.Name : null,
                            d.DispositionStatus, d.DecidedAt, d.ExecutedAt, d.DestinationFacilityName,
                            Tujuan = d.DestinationServiceUnit != null ? d.DestinationServiceUnit.ServiceUnitName : null,
                            d.ReferralNumber, d.DispositionReason, d.PatientConditionAtDisposition,
                            d.FollowUpInstruction, d.RefusalReason, d.IsPatientDeceased, d.DeathDateTime, d.Notes
                        })
                        .FirstAsync(cancellationToken);

                    Tambah(bagian, "Tindak lanjut", x.Jenis);
                    Tambah(bagian, "Diputuskan", TulisWaktu(x.DecidedAt));
                    Tambah(bagian, "Dilaksanakan", TulisWaktu(x.ExecutedAt));
                    Tambah(bagian, "Tujuan", x.Tujuan ?? x.DestinationFacilityName);
                    Tambah(bagian, "Nomor rujukan", x.ReferralNumber);
                    Tambah(bagian, "Alasan", x.DispositionReason);
                    Tambah(bagian, "Kondisi pasien", x.PatientConditionAtDisposition);
                    Tambah(bagian, "Instruksi tindak lanjut", x.FollowUpInstruction);
                    Tambah(bagian, "Alasan penolakan", x.RefusalReason);
                    Tambah(bagian, "Meninggal", x.IsPatientDeceased ? TulisWaktu(x.DeathDateTime) ?? "Ya" : null);
                    Tambah(bagian, "Catatan", x.Notes);
                    break;
                }
            }

            Tambah(bagian, "Final", TulisWaktu(ringkas.FinalizedAt));

            return new MedicalRecordSourceDocumentDetail
            {
                EncounterId = ringkas.EncounterId,
                DocumentNumber = ringkas.DocumentNumber,
                Title = ringkas.Title,
                FinalizedAt = ringkas.FinalizedAt,
                IsCorrected = ringkas.IsCorrected,
                Sections = bagian
            };
        }

        public async Task<MedicalRecordCompletenessProbeResult> ProbeAsync(
            MedicalRecordCompletenessProbeQuery query,
            CancellationToken cancellationToken)
        {
            var hasil = new MedicalRecordCompletenessProbeResult();

            if (query.Criteria.Contains(CompletenessCriterion.EmergencyTriage)
                && await _dbContext.EmgTriages.AsNoTracking().AnyAsync(x => !x.IsDelete
                    && x.TriageStatus == EmergencyTriageStatus.Completed
                    && x.EmergencyVisit != null && !x.EmergencyVisit.IsDelete
                    && x.EmergencyVisit.EncounterId == query.EncounterId, cancellationToken))
            {
                hasil.SatisfiedCriteria.Add(CompletenessCriterion.EmergencyTriage);
            }

            if (query.Criteria.Contains(CompletenessCriterion.EmergencyDisposition)
                && await _dbContext.EmgDispositions.AsNoTracking().AnyAsync(x => !x.IsDelete
                    && (x.DispositionStatus == EmergencyDispositionStatus.Confirmed || x.DispositionStatus == EmergencyDispositionStatus.Executed)
                    && x.EmergencyVisit != null && !x.EmergencyVisit.IsDelete
                    && x.EmergencyVisit.EncounterId == query.EncounterId, cancellationToken))
            {
                hasil.SatisfiedCriteria.Add(CompletenessCriterion.EmergencyDisposition);
            }

            return hasil;
        }

        // =====================================================================
        // Satu-satunya tempat aturan "final" IGD untuk rekam medis
        // =====================================================================

        private sealed class BarisFinal
        {
            public Guid Id { get; set; }
            public Guid? EncounterId { get; set; }
            public DateTime FinalizedAt { get; set; }
            public string? DocumentNumber { get; set; }
            public string? Title { get; set; }
            public string? Summary { get; set; }
            public bool IsCorrected { get; set; }
            public Guid? AuthorUserId { get; set; }
        }

        private IQueryable<BarisFinal> Baris(ClinicalDocumentKind jenis, Guid patientId, Guid? encounterId) => jenis switch
        {
            ClinicalDocumentKind.EmergencyTriage => _dbContext.EmgTriages.AsNoTracking()
                .Where(x => !x.IsDelete
                            && (x.TriageStatus == EmergencyTriageStatus.Completed || x.TriageStatus == EmergencyTriageStatus.Superseded)
                            && x.CompletedAt != null
                            && x.EmergencyVisit != null && !x.EmergencyVisit.IsDelete
                            && x.EmergencyVisit.PatientId == patientId
                            && (!encounterId.HasValue || x.EmergencyVisit.EncounterId == encounterId.Value))
                .Select(x => new BarisFinal
                {
                    Id = x.Id,
                    EncounterId = x.EmergencyVisit!.EncounterId,
                    FinalizedAt = x.CompletedAt!.Value,
                    DocumentNumber = x.EmergencyVisit.EmergencyVisitNumber,
                    Title = x.TriageLevel != null ? "Triase IGD — " + x.TriageLevel.Name : "Triase IGD",
                    Summary = x.TriageStatus == EmergencyTriageStatus.Superseded
                        ? "Digantikan triase ulang."
                        : x.IsRetriage ? "Triase ulang." : x.TriageReason,
                    IsCorrected = x.TriageStatus == EmergencyTriageStatus.Superseded,
                    AuthorUserId = x.PerformedByUserId
                }),

            ClinicalDocumentKind.EmergencyObservation => _dbContext.EmgObservations.AsNoTracking()
                .Where(x => !x.IsDelete
                            && (x.ObservationStatus == EmergencyObservationStatus.Completed || x.ObservationStatus == EmergencyObservationStatus.Escalated)
                            && x.EndedAt != null
                            && x.EmergencyVisit != null && !x.EmergencyVisit.IsDelete
                            && x.EmergencyVisit.PatientId == patientId
                            && (!encounterId.HasValue || x.EmergencyVisit.EncounterId == encounterId.Value))
                .Select(x => new BarisFinal
                {
                    Id = x.Id,
                    EncounterId = x.EmergencyVisit!.EncounterId,
                    FinalizedAt = x.EndedAt!.Value,
                    DocumentNumber = x.ObservationNumber,
                    Title = "Observasi IGD",
                    Summary = x.ObservationStatus == EmergencyObservationStatus.Escalated
                        ? "Dieskalasi: " + x.EscalationReason
                        : x.CompletionSummary,
                    IsCorrected = false,
                    AuthorUserId = x.ResponsibleNurseUserId
                }),

            ClinicalDocumentKind.EmergencyResuscitation => _dbContext.EmgResuscitations.AsNoTracking()
                .Where(x => !x.IsDelete
                            && (x.ResuscitationStatus == EmergencyResuscitationStatus.Completed || x.ResuscitationStatus == EmergencyResuscitationStatus.Stopped)
                            && x.CompletedAt != null
                            && x.EmergencyVisit != null && !x.EmergencyVisit.IsDelete
                            && x.EmergencyVisit.PatientId == patientId
                            && (!encounterId.HasValue || x.EmergencyVisit.EncounterId == encounterId.Value))
                .Select(x => new BarisFinal
                {
                    Id = x.Id,
                    EncounterId = x.EmergencyVisit!.EncounterId,
                    FinalizedAt = x.CompletedAt!.Value,
                    DocumentNumber = x.ResuscitationNumber,
                    Title = "Resusitasi IGD",
                    Summary = x.ResuscitationStatus == EmergencyResuscitationStatus.Stopped
                        ? "Dihentikan. " + x.OutcomeSummary
                        : x.OutcomeSummary,
                    IsCorrected = false,
                    AuthorUserId = x.RecordedByUserId
                }),

            ClinicalDocumentKind.EmergencyDisposition => _dbContext.EmgDispositions.AsNoTracking()
                .Where(x => !x.IsDelete
                            && (x.DispositionStatus == EmergencyDispositionStatus.Confirmed || x.DispositionStatus == EmergencyDispositionStatus.Executed)
                            && x.EmergencyVisit != null && !x.EmergencyVisit.IsDelete
                            && x.EmergencyVisit.PatientId == patientId
                            && (!encounterId.HasValue || x.EmergencyVisit.EncounterId == encounterId.Value))
                .Select(x => new BarisFinal
                {
                    Id = x.Id,
                    EncounterId = x.EmergencyVisit!.EncounterId,
                    // RM-DEC-067: final pada konfirmasi. Disposisi lama yang langsung dilaksanakan
                    // tanpa waktu konfirmasi memakai waktu keputusannya.
                    FinalizedAt = x.ConfirmedAt ?? x.DecidedAt,
                    DocumentNumber = x.EmergencyVisit.EmergencyVisitNumber,
                    Title = x.DispositionType != null ? "Disposisi IGD — " + x.DispositionType.Name : "Disposisi IGD",
                    Summary = x.DispositionReason,
                    IsCorrected = false,
                    AuthorUserId = x.ConfirmedByUserId
                }),

            _ => Enumerable.Empty<BarisFinal>().AsQueryable()
        };

        private static IQueryable<BarisFinal> Saring(IQueryable<BarisFinal> sumber, DateTime? dari, DateTime? sampai)
        {
            if (dari.HasValue)
                sumber = sumber.Where(x => x.FinalizedAt >= dari.Value);
            if (sampai.HasValue)
                sumber = sumber.Where(x => x.FinalizedAt <= sampai.Value);
            return sumber;
        }

        /// <summary>Waktu tersimpan UTC; ditulis WIB (UTC+7) beserta labelnya (`NFR-005`).</summary>
        private static string? TulisWaktu(DateTime? waktu) =>
            waktu?.AddHours(7).ToString("dd-MM-yyyy HH:mm 'WIB'", CultureInfo.InvariantCulture);

        private static string? Potong(string? teks) =>
            string.IsNullOrWhiteSpace(teks) ? null
            : teks.Length <= PanjangRingkasanMaksimal ? teks : teks[..PanjangRingkasanMaksimal];

        private static void Tambah(List<MedicalRecordDocumentSectionResponse> bagian, string label, string? nilai)
        {
            if (!string.IsNullOrWhiteSpace(nilai))
                bagian.Add(new MedicalRecordDocumentSectionResponse { Label = label, Value = nilai.Trim() });
        }
    }
}
