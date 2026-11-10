using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services.Sources;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;

/// <summary>
/// Pembaca catatan asuhan gizi untuk berkas rekam medis (`INT-RM-10g`, `BE-33`).
///
/// Ditulis di modul Gizi dengan persetujuan pemiliknya (`RM-DEC-073`). Hanya membaca; order,
/// diagnosis gizi, diet, dan catatan asuhan tetap milik Gizi dan tidak diubah.
///
/// FINAL MENURUT GIZI — ATURAN SEMENTARA (A4.10, `RM-DEC-059`, `RM-DEC-073`): catatan asuhan
/// diisi bertahap selama kunjungan dan tidak punya status final maupun penguncian. Karena itu
/// setiap catatan yang tersimpan tampil dengan waktu final <c>CreateDateTime</c> dan bertanda
/// <c>IsProvisional</c> — berlabel "Dapat berubah" di layar — karena isinya masih dapat diubah
/// tanpa riwayat. Pemilik Gizi dapat menggantinya dengan kontrak final kemudian.
///
/// Gizi tidak menjawab kriteria kelengkapan mana pun (A4.11), sehingga pembaca ini bukan
/// <see cref="IMedicalRecordCompletenessProbe"/>.
/// </summary>
public class GziMedicalRecordDocumentSource : IMedicalRecordDocumentSource
{
    private const int PanjangRingkasanMaksimal = 300;

    private readonly ApplicationDbContext _dbContext;

    public GziMedicalRecordDocumentSource(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string SourceModule => "Gizi";

    public IReadOnlyCollection<ClinicalDocumentKind> Kinds { get; } = [ClinicalDocumentKind.NutritionCareRecord];

    public async Task<MedicalRecordSourcePage> ReadAsync(
        MedicalRecordSourceQuery query,
        CancellationToken cancellationToken)
    {
        var hasil = new MedicalRecordSourcePage();
        if (!query.Kinds.Contains(ClinicalDocumentKind.NutritionCareRecord))
            return hasil;

        var catatan = CatatanTersimpan(query.PatientId, query.EncounterId, query.StartDate, query.EndDate);
        hasil.Totals[ClinicalDocumentKind.NutritionCareRecord] = await catatan.CountAsync(cancellationToken);

        if (query.Limit > 0)
        {
            var baris = await (query.NewestFirst
                    ? catatan.OrderByDescending(x => x.CreateDateTime)
                    : catatan.OrderBy(x => x.CreateDateTime))
                .Take(query.Limit)
                .Select(x => new
                {
                    x.Id,
                    x.NutritionOrder!.EncounterId,
                    x.NutritionOrder.OrderNumber,
                    x.CreateDateTime,
                    x.VisitAt,
                    x.VisitSequence,
                    x.RecordType,
                    DiagnosisUtama = _dbContext.GziNutritionCareRecordDiagnoses
                        .Where(d => d.CareRecordId == x.Id && !d.IsDelete)
                        .OrderByDescending(d => d.IsPrimary)
                        .ThenBy(d => d.SortOrder)
                        .Select(d => d.NutritionDiagnosis!.DiagnosisCode + " " + d.NutritionDiagnosis.DiagnosisName)
                        .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            hasil.Items = baris.Select(x => new MedicalRecordFinalDocumentDescriptor
            {
                Kind = ClinicalDocumentKind.NutritionCareRecord,
                DocumentId = x.Id,
                EncounterId = x.EncounterId,
                FinalizedAt = x.CreateDateTime,
                OccurredAt = x.VisitAt,
                DocumentNumber = x.OrderNumber,
                Title = "Asuhan Gizi",
                Summary = Potong(Gabung($"{NamaJenisKunjungan(x.RecordType)} ke-{x.VisitSequence}", x.DiagnosisUtama)),
                IsProvisional = true
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
        if (kind != ClinicalDocumentKind.NutritionCareRecord)
            return null;

        var x = await CatatanTersimpan(patientId, null, null, null)
            .Where(r => r.Id == documentId)
            .Select(r => new
            {
                r.NutritionOrder!.EncounterId,
                r.NutritionOrder.OrderNumber,
                r.CreateDateTime,
                r.UpdateDateTime,
                r.VisitAt,
                r.VisitSequence,
                r.RecordType,
                r.Weight,
                r.Height,
                r.Bmi,
                r.AssessmentNote,
                r.DiagnosisNote,
                r.InterventionNote,
                r.IntakeRecallNote,
                r.IntakePercent,
                r.EvaluationNote,
                // Subquery, bukan navigation: master yang hilang tidak boleh menyembunyikan catatan.
                Petugas = _dbContext.MstWorkforceProfiles
                    .Where(w => w.Id == r.RecordedByWorkforceId)
                    .Select(w => w.DisplayName)
                    .FirstOrDefault(),
                Diet = _dbContext.GziPatientDiets
                    .Where(d => d.Id == r.PatientDietId)
                    .Select(d => new
                    {
                        Jenis = d.DietType != null ? d.DietType.DietTypeName : null,
                        Bentuk = d.FoodForm != null ? d.FoodForm.FoodFormName : null,
                        d.Instruction
                    })
                    .FirstOrDefault(),
                Diagnosis = _dbContext.GziNutritionCareRecordDiagnoses
                    .Where(d => d.CareRecordId == r.Id && !d.IsDelete)
                    .OrderByDescending(d => d.IsPrimary)
                    .ThenBy(d => d.SortOrder)
                    .Select(d => new
                    {
                        Kode = d.NutritionDiagnosis != null ? d.NutritionDiagnosis.DiagnosisCode : null,
                        Nama = d.NutritionDiagnosis != null ? d.NutritionDiagnosis.DiagnosisName : null,
                        d.IsPrimary,
                        d.Note
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (x == null)
            return null;

        var bagian = new List<MedicalRecordDocumentSectionResponse>();
        Tambah(bagian, "Jenis kunjungan", $"{NamaJenisKunjungan(x.RecordType)} ke-{x.VisitSequence}");
        Tambah(bagian, "Waktu kunjungan", TulisWaktu(x.VisitAt));
        Tambah(bagian, "Petugas gizi", x.Petugas);
        Tambah(bagian, "Berat badan", x.Weight.HasValue ? $"{TulisAngka(x.Weight.Value)} kg" : null);
        Tambah(bagian, "Tinggi badan", x.Height.HasValue ? $"{TulisAngka(x.Height.Value)} cm" : null);
        Tambah(bagian, "IMT", x.Bmi.HasValue ? TulisAngka(x.Bmi.Value) : null);
        Tambah(bagian, "Asesmen", x.AssessmentNote);
        foreach (var d in x.Diagnosis)
        {
            Tambah(bagian, d.IsPrimary ? "Diagnosis gizi utama" : "Diagnosis gizi",
                Gabung(string.Join(" ", new[] { d.Kode, d.Nama }.Where(t => !string.IsNullOrWhiteSpace(t))), d.Note));
        }
        Tambah(bagian, "Catatan diagnosis", x.DiagnosisNote);
        Tambah(bagian, "Intervensi", x.InterventionNote);
        Tambah(bagian, "Diet", x.Diet == null ? null : Gabung(x.Diet.Jenis, x.Diet.Bentuk, x.Diet.Instruction));
        Tambah(bagian, "Recall asupan", x.IntakeRecallNote);
        Tambah(bagian, "Asupan terhadap kebutuhan", x.IntakePercent.HasValue ? $"{x.IntakePercent}%" : null);
        Tambah(bagian, "Monitoring dan evaluasi", x.EvaluationNote);
        Tambah(bagian, "Dicatat", TulisWaktu(x.CreateDateTime));
        Tambah(bagian, "Terakhir diubah", x.UpdateDateTime.HasValue && x.UpdateDateTime != x.CreateDateTime ? TulisWaktu(x.UpdateDateTime) : null);

        return new MedicalRecordSourceDocumentDetail
        {
            EncounterId = x.EncounterId,
            DocumentNumber = x.OrderNumber,
            Title = "Asuhan Gizi",
            FinalizedAt = x.CreateDateTime,
            OccurredAt = x.VisitAt,
            IsProvisional = true,
            Sections = bagian
        };
    }

    // =====================================================================
    // Satu-satunya tempat aturan "tersimpan" Gizi untuk rekam medis
    // =====================================================================

    private IQueryable<GziNutritionCareRecord> CatatanTersimpan(Guid patientId, Guid? encounterId, DateTime? dari, DateTime? sampai)
    {
        var query = _dbContext.GziNutritionCareRecords
            .AsNoTracking()
            .Where(x => !x.IsDelete
                        && x.NutritionOrder != null
                        && !x.NutritionOrder.IsDelete
                        && x.NutritionOrder.PatientId == patientId);

        if (encounterId.HasValue)
            query = query.Where(x => x.NutritionOrder!.EncounterId == encounterId.Value);
        if (dari.HasValue)
            query = query.Where(x => x.CreateDateTime >= dari.Value);
        if (sampai.HasValue)
            query = query.Where(x => x.CreateDateTime <= sampai.Value);

        return query;
    }

    private static string NamaJenisKunjungan(GziCareRecordType jenis) => jenis switch
    {
        GziCareRecordType.Initial => "Asesmen awal",
        GziCareRecordType.FollowUp => "Kunjungan lanjutan",
        _ => "Kunjungan"
    };

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
