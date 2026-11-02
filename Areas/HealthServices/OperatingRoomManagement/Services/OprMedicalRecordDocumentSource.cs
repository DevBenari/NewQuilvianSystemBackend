using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services.Sources;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;

/// <summary>
/// Pembaca laporan operasi, catatan anestesi, checklist keselamatan, dan catatan kelahiran
/// Kamar Operasi untuk berkas rekam medis (`INT-RM-10e`, `BE-31`).
///
/// Ditulis di modul Kamar Operasi dengan persetujuan pemiliknya (`RM-DEC-071`). Hanya membaca;
/// isi dan siklus dokumen tetap milik Kamar Operasi, termasuk addendum laporan operasi.
///
/// FINAL MENURUT KAMAR OPERASI (A4.10):
/// <list type="bullet">
/// <item>Laporan operasi dan catatan anestesi — final bila berstatus <c>Final</c>, waktu
/// <c>FinalizedAt</c>. Laporan operasi yang punya addendum bertanda pernah diralat.</item>
/// <item>Checklist keselamatan — final bila berstatus <c>Completed</c>, waktu <c>SignedAt</c>.
/// Perubahan checklist final membuat revisi baru; yang tampil hanya revisi selesai terakhir per
/// fase, bertanda pernah diralat bila <c>Revision &gt; 1</c>.</item>
/// <item>Catatan kelahiran — final saat tersimpan, waktu <c>CreateDateTime</c> (`RM-DEC-071`).</item>
/// </list>
///
/// KONDISI DAN KRITERIA KELENGKAPAN: kondisi <c>Surgery</c> ada bila kunjungan punya kasus
/// <c>Completed</c>; kasus <c>Cancelled</c>, <c>Rejected</c>, <c>Postponed</c>, maupun yang belum
/// selesai tidak memicunya. Kriteria dinilai PER KASUS selesai (`RM-DEC-076`): setiap kasus itu
/// wajib punya laporan operasi final, catatan anestesi final, dan checklist ketiga fase (Sign In,
/// Time Out, Sign Out) yang selesai. Dokumen kasus yang batal tidak menutupi kasus lain.
/// </summary>
public class OprMedicalRecordDocumentSource : IMedicalRecordDocumentSource, IMedicalRecordCompletenessProbe
{
    private const int PanjangRingkasanMaksimal = 300;

    private readonly ApplicationDbContext _dbContext;

    public OprMedicalRecordDocumentSource(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string SourceModule => "Kamar Operasi";

    public IReadOnlyCollection<ClinicalDocumentKind> Kinds { get; } =
    [
        ClinicalDocumentKind.SurgeryReport,
        ClinicalDocumentKind.AnesthesiaRecord,
        ClinicalDocumentKind.SurgicalSafetyChecklist,
        ClinicalDocumentKind.BirthRecord
    ];

    public IReadOnlyCollection<CompletenessCriterion> Criteria { get; } =
    [
        CompletenessCriterion.SurgeryReport,
        CompletenessCriterion.AnesthesiaRecord,
        CompletenessCriterion.SurgicalSafetyChecklist
    ];

    public IReadOnlyCollection<CompletenessCondition> Conditions { get; } = [CompletenessCondition.Surgery];

    public bool ReportsPendingResults => false;

    public async Task<MedicalRecordSourcePage> ReadAsync(
        MedicalRecordSourceQuery query,
        CancellationToken cancellationToken)
    {
        var hasil = new MedicalRecordSourcePage();
        var dokumen = new List<MedicalRecordFinalDocumentDescriptor>();

        if (query.Kinds.Contains(ClinicalDocumentKind.SurgeryReport))
        {
            var laporan = LaporanFinal(query.PatientId, query.EncounterId, query.StartDate, query.EndDate);
            hasil.Totals[ClinicalDocumentKind.SurgeryReport] = await laporan.CountAsync(cancellationToken);

            if (query.Limit > 0)
            {
                var baris = await (query.NewestFirst
                        ? laporan.OrderByDescending(x => x.FinalizedAt)
                        : laporan.OrderBy(x => x.FinalizedAt))
                    .Take(query.Limit)
                    .Select(x => new
                    {
                        x.Id,
                        x.OprCase!.EncounterId,
                        x.OprCase.CaseNumber,
                        FinalizedAt = x.FinalizedAt!.Value,
                        x.PostDiagnosis,
                        PernahDiralat = x.Addenda.Any(a => !a.IsDelete)
                    })
                    .ToListAsync(cancellationToken);

                dokumen.AddRange(baris.Select(x => new MedicalRecordFinalDocumentDescriptor
                {
                    Kind = ClinicalDocumentKind.SurgeryReport,
                    DocumentId = x.Id,
                    EncounterId = x.EncounterId,
                    FinalizedAt = x.FinalizedAt,
                    DocumentNumber = x.CaseNumber,
                    Title = "Laporan Operasi",
                    Summary = Potong(x.PostDiagnosis),
                    IsCorrected = x.PernahDiralat
                }));
            }
        }

        if (query.Kinds.Contains(ClinicalDocumentKind.AnesthesiaRecord))
        {
            var anestesi = AnestesiFinal(query.PatientId, query.EncounterId, query.StartDate, query.EndDate);
            hasil.Totals[ClinicalDocumentKind.AnesthesiaRecord] = await anestesi.CountAsync(cancellationToken);

            if (query.Limit > 0)
            {
                var baris = await (query.NewestFirst
                        ? anestesi.OrderByDescending(x => x.FinalizedAt)
                        : anestesi.OrderBy(x => x.FinalizedAt))
                    .Take(query.Limit)
                    .Select(x => new
                    {
                        x.Id,
                        x.OprCase!.EncounterId,
                        x.OprCase.CaseNumber,
                        FinalizedAt = x.FinalizedAt!.Value,
                        x.Technique
                    })
                    .ToListAsync(cancellationToken);

                dokumen.AddRange(baris.Select(x => new MedicalRecordFinalDocumentDescriptor
                {
                    Kind = ClinicalDocumentKind.AnesthesiaRecord,
                    DocumentId = x.Id,
                    EncounterId = x.EncounterId,
                    FinalizedAt = x.FinalizedAt,
                    DocumentNumber = x.CaseNumber,
                    Title = "Catatan Anestesi",
                    Summary = Potong(x.Technique)
                }));
            }
        }

        if (query.Kinds.Contains(ClinicalDocumentKind.SurgicalSafetyChecklist))
        {
            var checklist = ChecklistFinal(query.PatientId, query.EncounterId, query.StartDate, query.EndDate);
            hasil.Totals[ClinicalDocumentKind.SurgicalSafetyChecklist] = await checklist.CountAsync(cancellationToken);

            if (query.Limit > 0)
            {
                var baris = await (query.NewestFirst
                        ? checklist.OrderByDescending(x => x.SignedAt)
                        : checklist.OrderBy(x => x.SignedAt))
                    .Take(query.Limit)
                    .Select(x => new
                    {
                        x.Id,
                        x.OprCase!.EncounterId,
                        x.OprCase.CaseNumber,
                        SignedAt = x.SignedAt!.Value,
                        x.Phase,
                        x.Revision,
                        x.IsEmergencyBypass,
                        x.SignedByUserId
                    })
                    .ToListAsync(cancellationToken);

                dokumen.AddRange(baris.Select(x => new MedicalRecordFinalDocumentDescriptor
                {
                    Kind = ClinicalDocumentKind.SurgicalSafetyChecklist,
                    DocumentId = x.Id,
                    EncounterId = x.EncounterId,
                    FinalizedAt = x.SignedAt,
                    DocumentNumber = x.CaseNumber,
                    Title = $"Checklist Keselamatan Operasi — {NamaFase(x.Phase)}",
                    Summary = x.IsEmergencyBypass ? "Jalur darurat" : null,
                    IsCorrected = x.Revision > 1,
                    AuthorUserId = x.SignedByUserId
                }));
            }
        }

        if (query.Kinds.Contains(ClinicalDocumentKind.BirthRecord))
        {
            var kelahiran = KelahiranFinal(query.PatientId, query.EncounterId, query.StartDate, query.EndDate);
            hasil.Totals[ClinicalDocumentKind.BirthRecord] = await kelahiran.CountAsync(cancellationToken);

            if (query.Limit > 0)
            {
                var baris = await (query.NewestFirst
                        ? kelahiran.OrderByDescending(x => x.CreateDateTime)
                        : kelahiran.OrderBy(x => x.CreateDateTime))
                    .Take(query.Limit)
                    .Select(x => new
                    {
                        x.Id,
                        x.OprCase!.EncounterId,
                        x.OprCase.CaseNumber,
                        x.CreateDateTime,
                        x.BirthDateTime,
                        x.Gender,
                        x.WeightGrams,
                        x.ApgarScore1Min,
                        x.ApgarScore5Min
                    })
                    .ToListAsync(cancellationToken);

                dokumen.AddRange(baris.Select(x => new MedicalRecordFinalDocumentDescriptor
                {
                    Kind = ClinicalDocumentKind.BirthRecord,
                    DocumentId = x.Id,
                    EncounterId = x.EncounterId,
                    FinalizedAt = x.CreateDateTime,
                    OccurredAt = x.BirthDateTime,
                    DocumentNumber = x.CaseNumber,
                    Title = "Catatan Kelahiran",
                    Summary = $"Bayi {NamaJenisKelamin(x.Gender).ToLowerInvariant()}, {TulisAngka(x.WeightGrams)} g, APGAR {x.ApgarScore1Min}/{x.ApgarScore5Min}"
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
        return kind switch
        {
            ClinicalDocumentKind.SurgeryReport => await RincianLaporanAsync(documentId, patientId, cancellationToken),
            ClinicalDocumentKind.AnesthesiaRecord => await RincianAnestesiAsync(documentId, patientId, cancellationToken),
            ClinicalDocumentKind.SurgicalSafetyChecklist => await RincianChecklistAsync(documentId, patientId, cancellationToken),
            ClinicalDocumentKind.BirthRecord => await RincianKelahiranAsync(documentId, patientId, cancellationToken),
            _ => null
        };
    }

    public async Task<MedicalRecordCompletenessProbeResult> ProbeAsync(
        MedicalRecordCompletenessProbeQuery query,
        CancellationToken cancellationToken)
    {
        var hasil = new MedicalRecordCompletenessProbeResult();

        var kasusSelesai = _dbContext.OprCases
            .AsNoTracking()
            .Where(x => !x.IsDelete
                        && x.PatientId == query.PatientId
                        && x.EncounterId == query.EncounterId
                        && x.Status == OprCaseStatus.Completed);

        // Tanpa kasus selesai, tidak ada kriteria yang dianggap terpenuhi: kondisinya memang tidak
        // ada, dan profil yang (keliru) menuntut kriteria operasi tanpa kondisi tidak lolos diam-diam.
        if (!await kasusSelesai.AnyAsync(cancellationToken))
            return hasil;

        hasil.PresentConditions.Add(CompletenessCondition.Surgery);

        if (query.Criteria.Contains(CompletenessCriterion.SurgeryReport)
            && await kasusSelesai.AllAsync(c => _dbContext.OprExecutionRecords.Any(r =>
                r.OprCaseId == c.Id && !r.IsDelete && r.Status == OprRecordStatus.Final && r.FinalizedAt != null), cancellationToken))
        {
            hasil.SatisfiedCriteria.Add(CompletenessCriterion.SurgeryReport);
        }

        if (query.Criteria.Contains(CompletenessCriterion.AnesthesiaRecord)
            && await kasusSelesai.AllAsync(c => _dbContext.OprAnesthesiaRecords.Any(r =>
                r.OprCaseId == c.Id && !r.IsDelete && r.Status == OprRecordStatus.Final && r.FinalizedAt != null), cancellationToken))
        {
            hasil.SatisfiedCriteria.Add(CompletenessCriterion.AnesthesiaRecord);
        }

        if (query.Criteria.Contains(CompletenessCriterion.SurgicalSafetyChecklist)
            && await kasusSelesai.AllAsync(c =>
                _dbContext.OprSafetyChecklists.Any(s => s.OprCaseId == c.Id && !s.IsDelete && s.Status == OprChecklistStatus.Completed && s.SignedAt != null && s.Phase == OprChecklistPhase.SignIn)
                && _dbContext.OprSafetyChecklists.Any(s => s.OprCaseId == c.Id && !s.IsDelete && s.Status == OprChecklistStatus.Completed && s.SignedAt != null && s.Phase == OprChecklistPhase.TimeOut)
                && _dbContext.OprSafetyChecklists.Any(s => s.OprCaseId == c.Id && !s.IsDelete && s.Status == OprChecklistStatus.Completed && s.SignedAt != null && s.Phase == OprChecklistPhase.SignOut),
                cancellationToken))
        {
            hasil.SatisfiedCriteria.Add(CompletenessCriterion.SurgicalSafetyChecklist);
        }

        return hasil;
    }

    // =====================================================================
    // Rincian
    // =====================================================================

    private async Task<MedicalRecordSourceDocumentDetail?> RincianLaporanAsync(Guid documentId, Guid patientId, CancellationToken cancellationToken)
    {
        var x = await LaporanFinal(patientId, null, null, null)
            .Where(r => r.Id == documentId)
            .Select(r => new
            {
                r.OprCase!.EncounterId,
                r.OprCase.CaseNumber,
                FinalizedAt = r.FinalizedAt!.Value,
                r.PreDiagnosis,
                r.PostDiagnosis,
                r.Findings,
                r.Technique,
                r.Complications,
                r.BloodLossMl,
                r.SpecimenNote,
                r.ImplantDrainNote,
                r.PostPlan,
                r.StartedAt,
                r.FinishedAt,
                // Subquery, bukan navigation: relasi wajib menjadi inner join, dan master dokter yang
                // hilang tidak boleh menyembunyikan laporan final.
                Operator = _dbContext.MstDoctors.Where(d => d.Id == r.OprCase.PrimarySurgeonId).Select(d => d.FullName).FirstOrDefault(),
                Addendum = r.Addenda.Where(a => !a.IsDelete).OrderBy(a => a.AuthoredAt)
                    .Select(a => new { a.AuthoredAt, a.Reason, a.Content }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (x == null)
            return null;

        var bagian = new List<MedicalRecordDocumentSectionResponse>();
        Tambah(bagian, "Operator", x.Operator);
        Tambah(bagian, "Diagnosis pra-operasi", x.PreDiagnosis);
        Tambah(bagian, "Diagnosis pasca-operasi", x.PostDiagnosis);
        Tambah(bagian, "Temuan", x.Findings);
        Tambah(bagian, "Teknik", x.Technique);
        Tambah(bagian, "Komplikasi", x.Complications);
        Tambah(bagian, "Perdarahan", x.BloodLossMl.HasValue ? $"{TulisAngka(x.BloodLossMl.Value)} ml" : null);
        Tambah(bagian, "Spesimen", x.SpecimenNote);
        Tambah(bagian, "Implan dan drain", x.ImplantDrainNote);
        Tambah(bagian, "Rencana pasca-operasi", x.PostPlan);
        Tambah(bagian, "Mulai", TulisWaktu(x.StartedAt));
        Tambah(bagian, "Selesai", TulisWaktu(x.FinishedAt));
        Tambah(bagian, "Difinalkan", TulisWaktu(x.FinalizedAt));
        foreach (var a in x.Addendum)
            Tambah(bagian, "Addendum", Gabung(TulisWaktu(a.AuthoredAt), a.Reason, a.Content));

        return new MedicalRecordSourceDocumentDetail
        {
            EncounterId = x.EncounterId,
            DocumentNumber = x.CaseNumber,
            Title = "Laporan Operasi",
            FinalizedAt = x.FinalizedAt,
            IsCorrected = x.Addendum.Count > 0,
            Sections = bagian
        };
    }

    private async Task<MedicalRecordSourceDocumentDetail?> RincianAnestesiAsync(Guid documentId, Guid patientId, CancellationToken cancellationToken)
    {
        var x = await AnestesiFinal(patientId, null, null, null)
            .Where(r => r.Id == documentId)
            .Select(r => new
            {
                r.OprCase!.EncounterId,
                r.OprCase.CaseNumber,
                FinalizedAt = r.FinalizedAt!.Value,
                r.AssessmentSummary,
                r.Technique,
                r.MedicationFluidSummary,
                r.AirwaySummary,
                r.MonitoringSummary,
                r.EventSummary,
                r.FinalCondition
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (x == null)
            return null;

        var bagian = new List<MedicalRecordDocumentSectionResponse>();
        Tambah(bagian, "Penilaian pra-anestesi", x.AssessmentSummary);
        Tambah(bagian, "Teknik anestesi", x.Technique);
        Tambah(bagian, "Obat dan cairan", x.MedicationFluidSummary);
        Tambah(bagian, "Jalan napas", x.AirwaySummary);
        Tambah(bagian, "Pemantauan", x.MonitoringSummary);
        Tambah(bagian, "Kejadian selama anestesi", x.EventSummary);
        Tambah(bagian, "Kondisi akhir", x.FinalCondition);
        Tambah(bagian, "Difinalkan", TulisWaktu(x.FinalizedAt));

        return new MedicalRecordSourceDocumentDetail
        {
            EncounterId = x.EncounterId,
            DocumentNumber = x.CaseNumber,
            Title = "Catatan Anestesi",
            FinalizedAt = x.FinalizedAt,
            Sections = bagian
        };
    }

    private async Task<MedicalRecordSourceDocumentDetail?> RincianChecklistAsync(Guid documentId, Guid patientId, CancellationToken cancellationToken)
    {
        var x = await ChecklistFinal(patientId, null, null, null)
            .Where(r => r.Id == documentId)
            .Select(r => new
            {
                r.OprCase!.EncounterId,
                r.OprCase.CaseNumber,
                SignedAt = r.SignedAt!.Value,
                r.Phase,
                r.TemplateVersion,
                r.Revision,
                r.ItemsJson,
                r.IsEmergencyBypass,
                r.BypassReason,
                r.CompletedAfterStableAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (x == null)
            return null;

        var bagian = new List<MedicalRecordDocumentSectionResponse>();
        Tambah(bagian, "Fase", NamaFase(x.Phase));
        Tambah(bagian, "Versi templat", x.TemplateVersion);
        Tambah(bagian, "Revisi", x.Revision.ToString(CultureInfo.InvariantCulture));
        foreach (var butir in BacaButir(x.ItemsJson))
            Tambah(bagian, butir.Label, butir.Nilai);
        Tambah(bagian, "Jalur darurat", x.IsEmergencyBypass ? Gabung("Ya", x.BypassReason) : null);
        Tambah(bagian, "Dilengkapi setelah stabil", TulisWaktu(x.CompletedAfterStableAt));
        Tambah(bagian, "Ditandatangani", TulisWaktu(x.SignedAt));

        return new MedicalRecordSourceDocumentDetail
        {
            EncounterId = x.EncounterId,
            DocumentNumber = x.CaseNumber,
            Title = $"Checklist Keselamatan Operasi — {NamaFase(x.Phase)}",
            FinalizedAt = x.SignedAt,
            IsCorrected = x.Revision > 1,
            Sections = bagian
        };
    }

    private async Task<MedicalRecordSourceDocumentDetail?> RincianKelahiranAsync(Guid documentId, Guid patientId, CancellationToken cancellationToken)
    {
        var x = await KelahiranFinal(patientId, null, null, null)
            .Where(r => r.Id == documentId)
            .Select(r => new
            {
                r.OprCase!.EncounterId,
                r.OprCase.CaseNumber,
                r.CreateDateTime,
                r.BirthDateTime,
                r.Gender,
                r.WeightGrams,
                r.LengthCentimeters,
                r.HeadCircumferenceCm,
                r.ApgarScore1Min,
                r.ApgarScore5Min,
                r.ApgarScore10Min,
                r.ResuscitationNote,
                r.ConditionNote
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (x == null)
            return null;

        var apgar = $"{x.ApgarScore1Min} (1 menit), {x.ApgarScore5Min} (5 menit)"
                    + (x.ApgarScore10Min.HasValue ? $", {x.ApgarScore10Min} (10 menit)" : string.Empty);

        var bagian = new List<MedicalRecordDocumentSectionResponse>();
        Tambah(bagian, "Waktu lahir", TulisWaktu(x.BirthDateTime));
        Tambah(bagian, "Jenis kelamin", NamaJenisKelamin(x.Gender));
        Tambah(bagian, "Berat badan", $"{TulisAngka(x.WeightGrams)} g");
        Tambah(bagian, "Panjang badan", $"{TulisAngka(x.LengthCentimeters)} cm");
        Tambah(bagian, "Lingkar kepala", x.HeadCircumferenceCm.HasValue ? $"{TulisAngka(x.HeadCircumferenceCm.Value)} cm" : null);
        Tambah(bagian, "APGAR", apgar);
        Tambah(bagian, "Resusitasi", x.ResuscitationNote);
        Tambah(bagian, "Kondisi bayi", x.ConditionNote);
        Tambah(bagian, "Dicatat", TulisWaktu(x.CreateDateTime));

        return new MedicalRecordSourceDocumentDetail
        {
            EncounterId = x.EncounterId,
            DocumentNumber = x.CaseNumber,
            Title = "Catatan Kelahiran",
            FinalizedAt = x.CreateDateTime,
            OccurredAt = x.BirthDateTime,
            Sections = bagian
        };
    }

    // =====================================================================
    // Satu-satunya tempat aturan "final" Kamar Operasi untuk rekam medis
    // =====================================================================

    private IQueryable<OprExecutionRecord> LaporanFinal(Guid patientId, Guid? encounterId, DateTime? dari, DateTime? sampai)
    {
        var kasus = KasusPasien(patientId, encounterId);
        var query = _dbContext.OprExecutionRecords
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.Status == OprRecordStatus.Final && x.FinalizedAt != null)
            .Where(x => kasus.Contains(x.OprCaseId));

        if (dari.HasValue)
            query = query.Where(x => x.FinalizedAt >= dari.Value);
        if (sampai.HasValue)
            query = query.Where(x => x.FinalizedAt <= sampai.Value);

        return query;
    }

    private IQueryable<OprAnesthesiaRecord> AnestesiFinal(Guid patientId, Guid? encounterId, DateTime? dari, DateTime? sampai)
    {
        var kasus = KasusPasien(patientId, encounterId);
        var query = _dbContext.OprAnesthesiaRecords
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.Status == OprRecordStatus.Final && x.FinalizedAt != null)
            .Where(x => kasus.Contains(x.OprCaseId));

        if (dari.HasValue)
            query = query.Where(x => x.FinalizedAt >= dari.Value);
        if (sampai.HasValue)
            query = query.Where(x => x.FinalizedAt <= sampai.Value);

        return query;
    }

    /// <summary>Revisi selesai terakhir per kasus per fase; revisi selesai yang lebih lama tidak tampil.</summary>
    private IQueryable<OprSafetyChecklist> ChecklistFinal(Guid patientId, Guid? encounterId, DateTime? dari, DateTime? sampai)
    {
        var kasus = KasusPasien(patientId, encounterId);
        var query = _dbContext.OprSafetyChecklists
            .AsNoTracking()
            .Where(x => !x.IsDelete
                        && x.Status == OprChecklistStatus.Completed
                        && x.SignedAt != null
                        && !_dbContext.OprSafetyChecklists.Any(o => o.OprCaseId == x.OprCaseId
                                                                    && o.Phase == x.Phase
                                                                    && !o.IsDelete
                                                                    && o.Status == OprChecklistStatus.Completed
                                                                    && o.Revision > x.Revision))
            .Where(x => kasus.Contains(x.OprCaseId));

        if (dari.HasValue)
            query = query.Where(x => x.SignedAt >= dari.Value);
        if (sampai.HasValue)
            query = query.Where(x => x.SignedAt <= sampai.Value);

        return query;
    }

    private IQueryable<OprBirthEvent> KelahiranFinal(Guid patientId, Guid? encounterId, DateTime? dari, DateTime? sampai)
    {
        var kasus = KasusPasien(patientId, encounterId);
        var query = _dbContext.OprBirthEvents
            .AsNoTracking()
            .Where(x => !x.IsDelete)
            .Where(x => kasus.Contains(x.OprCaseId));

        if (dari.HasValue)
            query = query.Where(x => x.CreateDateTime >= dari.Value);
        if (sampai.HasValue)
            query = query.Where(x => x.CreateDateTime <= sampai.Value);

        return query;
    }

    /// <summary>Jalan ke pasien lewat kasus operasinya (<c>OprCase.PatientId</c>, A4.10).</summary>
    private IQueryable<Guid> KasusPasien(Guid patientId, Guid? encounterId)
    {
        var kasus = _dbContext.OprCases.Where(c => !c.IsDelete && c.PatientId == patientId);
        if (encounterId.HasValue)
            kasus = kasus.Where(c => c.EncounterId == encounterId.Value);
        return kasus.Select(c => c.Id);
    }

    // =====================================================================
    // Penulisan
    // =====================================================================

    public static string NamaFase(OprChecklistPhase fase) => fase switch
    {
        OprChecklistPhase.SignIn => "Sign In",
        OprChecklistPhase.TimeOut => "Time Out",
        OprChecklistPhase.SignOut => "Sign Out",
        _ => "Fase lain"
    };

    private static string NamaJenisKelamin(string? kode) => kode switch
    {
        "Male" => "Laki-laki",
        "Female" => "Perempuan",
        _ => string.IsNullOrWhiteSpace(kode) ? "Tidak tercatat" : kode
    };

    /// <summary>Butir checklist sebagai "Sudah"/"Belum" beserta catatannya; JSON rusak menghasilkan nol butir.</summary>
    private static IEnumerable<(string Label, string Nilai)> BacaButir(string itemsJson)
    {
        var butir = new List<(string, string)>();
        try
        {
            using var dokumen = JsonDocument.Parse(itemsJson);
            if (!dokumen.RootElement.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
                return butir;

            foreach (var item in items.EnumerateArray())
            {
                var label = item.TryGetProperty("label", out var l) ? l.GetString() : null;
                if (string.IsNullOrWhiteSpace(label))
                    continue;
                var dicentang = item.TryGetProperty("isChecked", out var c) && c.ValueKind == JsonValueKind.True;
                var catatan = item.TryGetProperty("note", out var n) && n.ValueKind == JsonValueKind.String ? n.GetString() : null;
                butir.Add((label.Trim(), Gabung(dicentang ? "Sudah" : "Belum", catatan)!));
            }
        }
        catch (JsonException)
        {
            // Isi checklist milik Kamar Operasi; bentuk yang tidak dikenali tidak menggagalkan rincian.
        }

        return butir;
    }

    private static string TulisAngka(decimal nilai) => nilai.ToString("0.##", CultureInfo.InvariantCulture);

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
