using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services.Sources;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services
{
    /// <summary>
    /// Pembaca pemeriksaan golongan darah dan pemberitahuan reaksi transfusi Bank Darah untuk
    /// berkas rekam medis (`INT-RM-10h`, `BE-34`).
    ///
    /// Ditulis di modul Bank Darah dengan persetujuan pemilik proses BDRS (`RM-DEC-074`). Hanya
    /// membaca; pembacaan ini TIDAK menggantikan uji kecocokan, otorisasi transfusi, maupun
    /// status stok kantong, dan tidak pernah dipakai untuk keputusan klinis.
    ///
    /// FINAL MENURUT BANK DARAH (A4.10):
    /// <list type="bullet">
    /// <item>Golongan darah — pemeriksaan berstatus <c>Validated</c>, waktu <c>ValidatedAt</c>.
    /// Pemeriksaan ini tidak terikat kunjungan, sehingga tampil sebagai rekam longitudinal
    /// TANPA kunjungan; tidak ada relasi kunjungan fiktif. Pernah diralat bila pemeriksaan ini
    /// menjadi dasar penyelesaian perbedaan (<c>BbkBloodGroupConflictResolution</c>).</item>
    /// <item>Pemberitahuan reaksi transfusi — berstatus <c>Acknowledged</c>, waktu
    /// <c>AcknowledgedAt</c>; waktu reaksi sebagai waktu kejadian.</item>
    /// </list>
    ///
    /// KESELAMATAN: hasil tervalidasi tidak selalu golongan darah yang berlaku (`BD-XINV-04`).
    /// Setiap baris golongan darah karena itu membawa keterangan berlaku/ditahan/tidak berlaku
    /// dari penanda <c>IsValidResult</c> dan <c>IsConflictHeld</c> milik Bank Darah, supaya
    /// hasil lama tidak terbaca sebagai golongan darah sah pasien.
    ///
    /// Bank Darah tidak menjawab kriteria kelengkapan (A4.11; kondisi transfusi dijawab generik),
    /// sehingga pembaca ini bukan <see cref="IMedicalRecordCompletenessProbe"/>.
    /// </summary>
    public class BbkMedicalRecordDocumentSource : IMedicalRecordDocumentSource
    {
        private const int PanjangRingkasanMaksimal = 300;

        private readonly ApplicationDbContext _dbContext;

        public BbkMedicalRecordDocumentSource(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string SourceModule => "Bank Darah";

        public IReadOnlyCollection<ClinicalDocumentKind> Kinds { get; } =
            [ClinicalDocumentKind.BloodGroupExam, ClinicalDocumentKind.TransfusionReactionNotice];

        public async Task<MedicalRecordSourcePage> ReadAsync(
            MedicalRecordSourceQuery query,
            CancellationToken cancellationToken)
        {
            var hasil = new MedicalRecordSourcePage();
            var dokumen = new List<MedicalRecordFinalDocumentDescriptor>();

            if (query.Kinds.Contains(ClinicalDocumentKind.BloodGroupExam))
            {
                var pemeriksaan = GolonganDarahFinal(query.PatientId, query.EncounterId, query.StartDate, query.EndDate);
                hasil.Totals[ClinicalDocumentKind.BloodGroupExam] = await pemeriksaan.CountAsync(cancellationToken);

                if (query.Limit > 0)
                {
                    var baris = await (query.NewestFirst
                            ? pemeriksaan.OrderByDescending(x => x.ValidatedAt)
                            : pemeriksaan.OrderBy(x => x.ValidatedAt))
                        .Take(query.Limit)
                        .Select(x => new
                        {
                            x.Id,
                            ValidatedAt = x.ValidatedAt!.Value,
                            x.AboRhesusResult,
                            x.IsValidResult,
                            x.IsConflictHeld,
                            x.ValidatedByUserId,
                            PernahDiralat = _dbContext.BbkBloodGroupConflictResolutions.Any(r => r.ResolvingExamId == x.Id && !r.IsDelete)
                        })
                        .ToListAsync(cancellationToken);

                    dokumen.AddRange(baris.Select(x => new MedicalRecordFinalDocumentDescriptor
                    {
                        Kind = ClinicalDocumentKind.BloodGroupExam,
                        DocumentId = x.Id,
                        EncounterId = null,
                        FinalizedAt = x.ValidatedAt,
                        Title = "Golongan Darah",
                        Summary = Potong(Gabung(NamaGolongan(x.AboRhesusResult), KeteranganBerlaku(x.IsValidResult, x.IsConflictHeld))),
                        IsCorrected = x.PernahDiralat,
                        AuthorUserId = x.ValidatedByUserId
                    }));
                }
            }

            if (query.Kinds.Contains(ClinicalDocumentKind.TransfusionReactionNotice))
            {
                var reaksi = ReaksiFinal(query.PatientId, query.EncounterId, query.StartDate, query.EndDate);
                hasil.Totals[ClinicalDocumentKind.TransfusionReactionNotice] = await reaksi.CountAsync(cancellationToken);

                if (query.Limit > 0)
                {
                    var baris = await (query.NewestFirst
                            ? reaksi.OrderByDescending(x => x.AcknowledgedAt)
                            : reaksi.OrderBy(x => x.AcknowledgedAt))
                        .Take(query.Limit)
                        .Select(x => new
                        {
                            x.Id,
                            x.EncounterId,
                            AcknowledgedAt = x.AcknowledgedAt!.Value,
                            x.OccurredAt,
                            x.ReactionSummarySnapshot,
                            x.AcknowledgedByUserId,
                            NomorKantong = _dbContext.BbkBloodUnits.Where(u => u.Id == x.BloodUnitId).Select(u => u.PmiBagNumber).FirstOrDefault()
                        })
                        .ToListAsync(cancellationToken);

                    dokumen.AddRange(baris.Select(x => new MedicalRecordFinalDocumentDescriptor
                    {
                        Kind = ClinicalDocumentKind.TransfusionReactionNotice,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        FinalizedAt = x.AcknowledgedAt,
                        OccurredAt = x.OccurredAt,
                        DocumentNumber = x.NomorKantong,
                        Title = "Pemberitahuan Reaksi Transfusi",
                        Summary = Potong(x.ReactionSummarySnapshot),
                        AuthorUserId = x.AcknowledgedByUserId
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
            if (kind == ClinicalDocumentKind.BloodGroupExam)
            {
                var x = await GolonganDarahFinal(patientId, null, null, null)
                    .Where(e => e.Id == documentId)
                    .Select(e => new
                    {
                        ValidatedAt = e.ValidatedAt!.Value,
                        e.ExaminedAt,
                        e.AboRhesusResult,
                        e.IsValidResult,
                        e.IsConflictHeld,
                        Sampel = _dbContext.BbkBloodGroupSamples
                            .Where(s => s.BloodGroupExamId == e.Id && !s.IsDelete)
                            .OrderBy(s => s.TakenAt)
                            .Select(s => new { s.SampleIdentifier, s.TakenAt })
                            .ToList(),
                        Penyelesaian = _dbContext.BbkBloodGroupConflictResolutions
                            .Where(r => r.ResolvingExamId == e.Id && !r.IsDelete)
                            .OrderBy(r => r.ResolvedAt)
                            .Select(r => new
                            {
                                r.ResolvedAt,
                                r.ReasonCode,
                                Alasan = _dbContext.MstBloodBankReasons
                                    .Where(m => m.ReasonCode == r.ReasonCode && !m.IsDelete)
                                    .Select(m => m.ReasonText)
                                    .FirstOrDefault()
                            })
                            .ToList()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (x == null)
                    return null;

                var bagian = new List<MedicalRecordDocumentSectionResponse>();
                Tambah(bagian, "Golongan darah ABO/Rhesus", NamaGolongan(x.AboRhesusResult));
                Tambah(bagian, "Keberlakuan", KeteranganBerlaku(x.IsValidResult, x.IsConflictHeld));
                foreach (var s in x.Sampel)
                    Tambah(bagian, "Sampel", Gabung(s.SampleIdentifier, TulisWaktu(s.TakenAt)));
                Tambah(bagian, "Diperiksa", TulisWaktu(x.ExaminedAt));
                Tambah(bagian, "Divalidasi", TulisWaktu(x.ValidatedAt));
                foreach (var r in x.Penyelesaian)
                    Tambah(bagian, "Penyelesaian perbedaan", Gabung(TulisWaktu(r.ResolvedAt), r.Alasan ?? r.ReasonCode));

                return new MedicalRecordSourceDocumentDetail
                {
                    EncounterId = null,
                    Title = "Golongan Darah",
                    FinalizedAt = x.ValidatedAt,
                    IsCorrected = x.Penyelesaian.Count > 0,
                    Sections = bagian
                };
            }

            if (kind == ClinicalDocumentKind.TransfusionReactionNotice)
            {
                var x = await ReaksiFinal(patientId, null, null, null)
                    .Where(n => n.Id == documentId)
                    .Select(n => new
                    {
                        n.EncounterId,
                        AcknowledgedAt = n.AcknowledgedAt!.Value,
                        n.OccurredAt,
                        n.ReceivedAt,
                        n.ReactionSummarySnapshot,
                        n.AcknowledgeNote,
                        NomorKantong = _dbContext.BbkBloodUnits.Where(u => u.Id == n.BloodUnitId).Select(u => u.PmiBagNumber).FirstOrDefault()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (x == null)
                    return null;

                var bagian = new List<MedicalRecordDocumentSectionResponse>();
                Tambah(bagian, "Ringkasan reaksi", x.ReactionSummarySnapshot);
                Tambah(bagian, "Waktu reaksi", TulisWaktu(x.OccurredAt));
                Tambah(bagian, "Kantong darah", x.NomorKantong);
                Tambah(bagian, "Diterima Bank Darah", TulisWaktu(x.ReceivedAt));
                Tambah(bagian, "Ditindaklanjuti", TulisWaktu(x.AcknowledgedAt));
                Tambah(bagian, "Catatan Bank Darah", x.AcknowledgeNote);

                return new MedicalRecordSourceDocumentDetail
                {
                    EncounterId = x.EncounterId,
                    DocumentNumber = x.NomorKantong,
                    Title = "Pemberitahuan Reaksi Transfusi",
                    FinalizedAt = x.AcknowledgedAt,
                    OccurredAt = x.OccurredAt,
                    Sections = bagian
                };
            }

            return null;
        }

        // =====================================================================
        // Satu-satunya tempat aturan "final" Bank Darah untuk rekam medis
        // =====================================================================

        private IQueryable<BbkBloodGroupExam> GolonganDarahFinal(Guid patientId, Guid? encounterId, DateTime? dari, DateTime? sampai)
        {
            // Pemeriksaan golongan darah tidak terikat kunjungan: penyaring kunjungan tidak
            // pernah cocok, alih-alih mengarang kunjungan untuknya.
            if (encounterId.HasValue)
                return _dbContext.BbkBloodGroupExams.AsNoTracking().Where(_ => false);

            var query = _dbContext.BbkBloodGroupExams
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.PatientId == patientId
                            && x.ExamStatus == BbkBloodGroupExamStatus.Validated
                            && x.ValidatedAt != null);

            if (dari.HasValue)
                query = query.Where(x => x.ValidatedAt >= dari.Value);
            if (sampai.HasValue)
                query = query.Where(x => x.ValidatedAt <= sampai.Value);

            return query;
        }

        private IQueryable<BbkTransfusionReactionNotice> ReaksiFinal(Guid patientId, Guid? encounterId, DateTime? dari, DateTime? sampai)
        {
            var query = _dbContext.BbkTransfusionReactionNotices
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.PatientId == patientId
                            && x.Status == BbkReactionNoticeStatus.Acknowledged
                            && x.AcknowledgedAt != null);

            if (encounterId.HasValue)
                query = query.Where(x => x.EncounterId == encounterId.Value);
            if (dari.HasValue)
                query = query.Where(x => x.AcknowledgedAt >= dari.Value);
            if (sampai.HasValue)
                query = query.Where(x => x.AcknowledgedAt <= sampai.Value);

            return query;
        }

        /// <summary>Keterangan dari penanda Bank Darah; rekam medis tidak menyimpulkan sendiri.</summary>
        private static string KeteranganBerlaku(bool isValidResult, bool isConflictHeld) =>
            isConflictHeld ? "Ditahan: bertentangan dengan hasil tervalidasi lain"
            : isValidResult ? "Golongan darah yang berlaku"
            : "Tidak berlaku lagi";

        /// <summary>Sama dengan label <c>BbkBloodGroupExamService</c>, yang bersifat privat.</summary>
        private static string? NamaGolongan(BloodType? nilai) => nilai switch
        {
            null => null,
            BloodType.APositive => "A Positif",
            BloodType.ANegative => "A Negatif",
            BloodType.BPositive => "B Positif",
            BloodType.BNegative => "B Negatif",
            BloodType.ABPositive => "AB Positif",
            BloodType.ABNegative => "AB Negatif",
            BloodType.OPositive => "O Positif",
            BloodType.ONegative => "O Negatif",
            BloodType.Unknown => "Tidak diketahui",
            BloodType.NotDisclosed => "Tidak diinformasikan",
            _ => nilai.ToString()
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
