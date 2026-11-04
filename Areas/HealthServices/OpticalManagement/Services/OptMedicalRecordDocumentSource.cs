using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services.Sources;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services
{
    /// <summary>
    /// Pembaca pemeriksaan refraksi dan resep Optik untuk berkas rekam medis (<c>BE-OPT-013</c>, <c>INT-OPT-09</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Isi</b> (<c>OPT-DEC-024</c>): visus, hasil refraksi, dan resep versi terakhir, termasuk salinan resep luar yang
    /// diberi tanda. Data penjualan — barang, harga, diskon, pembayaran — <b>tidak</b> dibaca sama sekali; kelas ini
    /// tidak menyentuh tabel pesanan.
    /// </para>
    /// <para>
    /// <b>Satu baris per pemeriksaan dan per resep, berisi versi terakhir</b> (<c>OPT-DEC-050</c>). Setiap versi
    /// terdaftar sebagai dokumen keutuhan tersendiri, sehingga <c>DocumentId</c> baris = Id versi terakhir dan
    /// rinciannya menerima Id versi mana pun milik pasien itu; versi lama tampil berketerangan "versi lama".
    /// </para>
    /// <para>
    /// <b>Hanya dokumen final:</b> pemeriksaan berstatus <c>Final</c> (draf dan batal tidak terbaca) dan semua resep
    /// (resep final sejak dibuat). Baca saja, tanpa pelacakan entity, tanpa pemeriksaan izin Optik (<c>RM-DEC-040</c>).
    /// </para>
    /// </remarks>
    public class OptMedicalRecordDocumentSource : IMedicalRecordDocumentSource
    {
        private const int PanjangRingkasanMaksimal = 300;

        private readonly ApplicationDbContext _dbContext;

        public OptMedicalRecordDocumentSource(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string SourceModule => "Optik";

        public IReadOnlyCollection<ClinicalDocumentKind> Kinds { get; } =
            [ClinicalDocumentKind.OpticalRefraction, ClinicalDocumentKind.OpticalPrescription];

        public async Task<MedicalRecordSourcePage> ReadAsync(MedicalRecordSourceQuery query, CancellationToken cancellationToken)
        {
            var hasil = new MedicalRecordSourcePage();
            var dokumen = new List<MedicalRecordFinalDocumentDescriptor>();

            if (query.Kinds.Contains(ClinicalDocumentKind.OpticalRefraction))
            {
                var baris = Saring(await PemeriksaanAsync(query.PatientId, query.EncounterId, cancellationToken), query);
                hasil.Totals[ClinicalDocumentKind.OpticalRefraction] = baris.Count;
                var nama = await NamaAsync(baris.Select(x => x.Terakhir.RevisedByWorkforceProfileId), cancellationToken);
                dokumen.AddRange(baris.Select(x => new MedicalRecordFinalDocumentDescriptor
                {
                    Kind = ClinicalDocumentKind.OpticalRefraction,
                    DocumentId = x.Terakhir.Id,
                    EncounterId = x.Exam.EncounterId,
                    FinalizedAt = x.Waktu,
                    OccurredAt = x.Exam.StartedAt,
                    DocumentNumber = x.Exam.ExamNumber,
                    Title = "Pemeriksaan Refraksi",
                    Summary = Potong(RingkasPemeriksaan(x.Terakhir, nama.GetValueOrDefault(x.Terakhir.RevisedByWorkforceProfileId))),
                    IsCorrected = x.Terakhir.RevisionNo > 1,
                    AuthorUserId = x.Terakhir.RevisionNo == 1 ? x.Exam.FinalizedBy : x.Terakhir.CreateBy,
                }));
            }

            if (query.Kinds.Contains(ClinicalDocumentKind.OpticalPrescription))
            {
                var baris = Saring(await ResepAsync(query.PatientId, query.EncounterId, cancellationToken), query);
                hasil.Totals[ClinicalDocumentKind.OpticalPrescription] = baris.Count;
                var nama = await NamaAsync(baris.Select(x => x.Terakhir.AuthorWorkforceProfileId), cancellationToken);
                dokumen.AddRange(baris.Select(x => new MedicalRecordFinalDocumentDescriptor
                {
                    Kind = ClinicalDocumentKind.OpticalPrescription,
                    DocumentId = x.Terakhir.Id,
                    EncounterId = x.Resep.EncounterId,
                    FinalizedAt = x.Waktu,
                    DocumentNumber = x.Resep.PrescriptionNumber,
                    Title = JudulResep(x.Resep),
                    Summary = Potong(RingkasResep(x.Resep, x.Terakhir, nama.GetValueOrDefault(x.Terakhir.AuthorWorkforceProfileId))),
                    IsCorrected = x.Terakhir.VersionNo > 1,
                    AuthorUserId = x.Terakhir.CreateBy,
                }));
            }

            if (query.Limit <= 0)
                return hasil;

            hasil.Items = dokumen
                .GroupBy(x => x.Kind)
                .SelectMany(g => (query.NewestFirst ? g.OrderByDescending(x => x.FinalizedAt) : g.OrderBy(x => x.FinalizedAt)).Take(query.Limit))
                .ToList();
            return hasil;
        }

        public async Task<MedicalRecordSourceDocumentDetail?> ReadDetailAsync(
            ClinicalDocumentKind kind, Guid documentId, Guid patientId, CancellationToken cancellationToken)
        {
            if (kind == ClinicalDocumentKind.OpticalRefraction)
            {
                var examId = await _dbContext.OptRefractionExamRevisions.AsNoTracking()
                    .Where(r => r.Id == documentId && !r.IsDelete)
                    .Select(r => (Guid?)r.RefractionExamId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (examId is null)
                    return null;

                var x = (await PemeriksaanAsync(patientId, null, cancellationToken, examId)).FirstOrDefault();
                var versi = x?.Semua.FirstOrDefault(r => r.Id == documentId);
                if (x is null || versi is null)
                    return null;

                var nama = await NamaAsync(x.Semua.Select(r => r.RevisedByWorkforceProfileId), cancellationToken);
                var bagian = new List<MedicalRecordDocumentSectionResponse>();
                Tambah(bagian, "Keadaan", versi.Id == x.Terakhir.Id ? null : $"Versi lama. Versi terakhir: {x.Terakhir.RevisionNo}.");
                Tambah(bagian, "Pemeriksa", nama.GetValueOrDefault(versi.RevisedByWorkforceProfileId));
                Tambah(bagian, "Visus jauh tanpa koreksi", Mata(versi.VisusDistanceUncorrectedOd, versi.VisusDistanceUncorrectedOs));
                Tambah(bagian, "Visus jauh dengan koreksi", Mata(versi.VisusDistanceCorrectedOd, versi.VisusDistanceCorrectedOs));
                Tambah(bagian, "Visus dekat tanpa koreksi", Mata(versi.VisusNearUncorrectedOd, versi.VisusNearUncorrectedOs));
                Tambah(bagian, "Visus dekat dengan koreksi", Mata(versi.VisusNearCorrectedOd, versi.VisusNearCorrectedOs));
                Tambah(bagian, "Autorefraktometer", Mata(
                    Ukur(versi.AutorefOdSph, versi.AutorefOdCyl, versi.AutorefOdAxis, null),
                    Ukur(versi.AutorefOsSph, versi.AutorefOsCyl, versi.AutorefOsAxis, null)));
                Tambah(bagian, "Koreksi subjektif", Mata(
                    Ukur(versi.SubjectiveOdSph, versi.SubjectiveOdCyl, versi.SubjectiveOdAxis, versi.SubjectiveOdAdd),
                    Ukur(versi.SubjectiveOsSph, versi.SubjectiveOsCyl, versi.SubjectiveOsAxis, versi.SubjectiveOsAdd)));
                Tambah(bagian, "PD", Pd(versi.PdDistance, versi.PdNear));
                Tambah(bagian, "Keratometri", versi.KeratometryNote);
                Tambah(bagian, "Catatan klinis", versi.ClinicalNote);
                Tambah(bagian, "Versi", $"{versi.RevisionNo} dari {x.Terakhir.RevisionNo}");
                Tambah(bagian, "Alasan revisi", versi.RevisionReason);
                Tambah(bagian, "Riwayat versi", Riwayat(x.Semua.Select(r =>
                    (r.RevisionNo, WaktuVersi(x.Exam, r), nama.GetValueOrDefault(r.RevisedByWorkforceProfileId), r.RevisionReason))));

                return new MedicalRecordSourceDocumentDetail
                {
                    EncounterId = x.Exam.EncounterId,
                    DocumentNumber = x.Exam.ExamNumber,
                    Title = "Pemeriksaan Refraksi",
                    FinalizedAt = WaktuVersi(x.Exam, versi),
                    OccurredAt = x.Exam.StartedAt,
                    IsCorrected = versi.RevisionNo > 1,
                    Sections = bagian,
                };
            }

            if (kind == ClinicalDocumentKind.OpticalPrescription)
            {
                var prescriptionId = await _dbContext.OptPrescriptionVersions.AsNoTracking()
                    .Where(v => v.Id == documentId && !v.IsDelete)
                    .Select(v => (Guid?)v.PrescriptionId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (prescriptionId is null)
                    return null;

                var x = (await ResepAsync(patientId, null, cancellationToken, prescriptionId)).FirstOrDefault();
                var versi = x?.Semua.FirstOrDefault(v => v.Id == documentId);
                if (x is null || versi is null)
                    return null;

                var r = x.Resep;
                var nama = await NamaAsync(x.Semua.Select(v => v.AuthorWorkforceProfileId), cancellationToken);
                var bagian = new List<MedicalRecordDocumentSectionResponse>();
                Tambah(bagian, "Keadaan", versi.Id == x.Terakhir.Id ? null : $"Versi lama. Versi terakhir: {x.Terakhir.VersionNo}.");
                Tambah(bagian, "Jenis", OptLabels.PrescriptionType(r.PrescriptionType));
                Tambah(bagian, "Asal", r.Origin == OptPrescriptionOrigin.External
                    ? $"Resep luar — {r.ExternalAuthorName}, {r.ExternalFacilityName}, {r.IssuedDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture)}"
                    : "Resep internal");
                Tambah(bagian, r.Origin == OptPrescriptionOrigin.External ? "Disalin oleh" : "Penulis", nama.GetValueOrDefault(versi.AuthorWorkforceProfileId));
                Tambah(bagian, "Resep", OptPrescriptionRules.Summary(r.PrescriptionType, versi));
                Tambah(bagian, "Teks asli (sebelum transposisi)", versi.IsTransposed ? versi.OriginalNotationText : null);
                Tambah(bagian, "Konfirmasi ukuran di luar rentang", versi.RangeOverrideReason);
                Tambah(bagian, "Berlaku sampai", r.ValidUntil.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture));
                Tambah(bagian, "Versi", $"{versi.VersionNo} dari {x.Terakhir.VersionNo}");
                Tambah(bagian, "Alasan revisi", versi.RevisionReason);
                Tambah(bagian, "Riwayat versi", Riwayat(x.Semua.Select(v =>
                    (v.VersionNo, v.AuthoredAt, nama.GetValueOrDefault(v.AuthorWorkforceProfileId), v.RevisionReason))));

                return new MedicalRecordSourceDocumentDetail
                {
                    EncounterId = r.EncounterId,
                    DocumentNumber = r.PrescriptionNumber,
                    Title = JudulResep(r),
                    FinalizedAt = versi.AuthoredAt,
                    IsCorrected = versi.VersionNo > 1,
                    Sections = bagian,
                };
            }

            return null;
        }

        // ---------------------------------------------------------------- pembacaan

        private sealed record BarisPemeriksaan(OptRefractionExam Exam, IReadOnlyList<OptRefractionExamRevision> Semua)
        {
            public OptRefractionExamRevision Terakhir => Semua.First(r => r.RevisionNo == Exam.CurrentRevisionNo);
            public DateTime Waktu => WaktuVersi(Exam, Terakhir);
        }

        private sealed record BarisResep(OptPrescription Resep, IReadOnlyList<OptPrescriptionVersion> Semua)
        {
            public OptPrescriptionVersion Terakhir => Semua.First(v => v.VersionNo == Resep.CurrentVersionNo);
            public DateTime Waktu => Terakhir.AuthoredAt;
        }

        private async Task<List<BarisPemeriksaan>> PemeriksaanAsync(Guid patientId, Guid? encounterId, CancellationToken cancellationToken, Guid? examId = null)
        {
            var exams = await _dbContext.OptRefractionExams.AsNoTracking()
                .Where(x => x.PatientId == patientId && x.Status == OptRefractionExamStatus.Final && !x.IsDelete)
                .Where(x => encounterId == null || x.EncounterId == encounterId)
                .Where(x => examId == null || x.Id == examId)
                .ToListAsync(cancellationToken);
            var ids = exams.Select(x => x.Id).ToList();
            var revisions = await _dbContext.OptRefractionExamRevisions.AsNoTracking()
                .Where(r => ids.Contains(r.RefractionExamId) && !r.IsDelete)
                .ToListAsync(cancellationToken);

            return exams
                .Select(x => new BarisPemeriksaan(x, revisions.Where(r => r.RefractionExamId == x.Id).OrderBy(r => r.RevisionNo).ToList()))
                .Where(x => x.Semua.Any(r => r.RevisionNo == x.Exam.CurrentRevisionNo))
                .ToList();
        }

        private async Task<List<BarisResep>> ResepAsync(Guid patientId, Guid? encounterId, CancellationToken cancellationToken, Guid? prescriptionId = null)
        {
            var rows = await _dbContext.OptPrescriptions.AsNoTracking()
                .Where(x => x.PatientId == patientId && !x.IsDelete)
                .Where(x => encounterId == null || x.EncounterId == encounterId)
                .Where(x => prescriptionId == null || x.Id == prescriptionId)
                .ToListAsync(cancellationToken);
            var ids = rows.Select(x => x.Id).ToList();
            var versions = await _dbContext.OptPrescriptionVersions.AsNoTracking()
                .Where(v => ids.Contains(v.PrescriptionId) && !v.IsDelete)
                .ToListAsync(cancellationToken);

            return rows
                .Select(x => new BarisResep(x, versions.Where(v => v.PrescriptionId == x.Id).OrderBy(v => v.VersionNo).ToList()))
                .Where(x => x.Semua.Any(v => v.VersionNo == x.Resep.CurrentVersionNo))
                .ToList();
        }

        private static List<BarisPemeriksaan> Saring(List<BarisPemeriksaan> baris, MedicalRecordSourceQuery q) =>
            baris.Where(x => (!q.StartDate.HasValue || x.Waktu >= q.StartDate.Value) && (!q.EndDate.HasValue || x.Waktu <= q.EndDate.Value)).ToList();

        private static List<BarisResep> Saring(List<BarisResep> baris, MedicalRecordSourceQuery q) =>
            baris.Where(x => (!q.StartDate.HasValue || x.Waktu >= q.StartDate.Value) && (!q.EndDate.HasValue || x.Waktu <= q.EndDate.Value)).ToList();

        private async Task<Dictionary<Guid, string>> NamaAsync(IEnumerable<Guid> workforceProfileIds, CancellationToken cancellationToken)
        {
            var ids = workforceProfileIds.Distinct().ToList();
            return await _dbContext.MstWorkforceProfiles.AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);
        }

        // ---------------------------------------------------------------- penulisan teks

        /// <summary>Versi 1 bertanggal saat final; versi sesudahnya bertanggal saat revisi.</summary>
        private static DateTime WaktuVersi(OptRefractionExam exam, OptRefractionExamRevision r) =>
            r.RevisionNo == 1 && exam.FinalizedAt is { } final ? final : r.RevisedAt;

        private static string JudulResep(OptPrescription r) =>
            (r.PrescriptionType == OptPrescriptionType.ContactLens ? "Resep Lensa Kontak" : "Resep Kacamata")
            + (r.Origin == OptPrescriptionOrigin.External ? " (resep luar)" : string.Empty);

        /// <summary>Contoh: "Visus OD 6/12, OS 6/15; Koreksi OD −1,50 / −0,50 × 180, OS −1,75 (Rina)".</summary>
        private static string RingkasPemeriksaan(OptRefractionExamRevision r, string? pemeriksa)
        {
            var visusOd = r.VisusDistanceUncorrectedOd ?? r.VisusDistanceCorrectedOd;
            var visusOs = r.VisusDistanceUncorrectedOs ?? r.VisusDistanceCorrectedOs;
            var bagian = new List<string>();
            if (visusOd is not null || visusOs is not null)
                bagian.Add($"Visus OD {visusOd ?? "—"}, OS {visusOs ?? "—"}");
            var koreksi = Mata(
                Ukur(r.SubjectiveOdSph, r.SubjectiveOdCyl, r.SubjectiveOdAxis, r.SubjectiveOdAdd),
                Ukur(r.SubjectiveOsSph, r.SubjectiveOsCyl, r.SubjectiveOsAxis, r.SubjectiveOsAdd));
            if (koreksi is not null)
                bagian.Add("Koreksi " + koreksi);
            return string.Join("; ", bagian) + (pemeriksa is null ? string.Empty : $" ({pemeriksa})");
        }

        /// <summary>Contoh: "OD −1,50; OS −1,75; PD 62 (Rina)" atau "Resep luar — Siti, Optik Sinar. OD …".</summary>
        private static string RingkasResep(OptPrescription resep, OptPrescriptionVersion v, string? penulis)
        {
            var awal = resep.Origin == OptPrescriptionOrigin.External ? $"Resep luar — {resep.ExternalAuthorName}, {resep.ExternalFacilityName}. " : string.Empty;
            return awal + OptPrescriptionRules.Summary(resep.PrescriptionType, v) + (penulis is null ? string.Empty : $" ({penulis})");
        }

        private static string? Ukur(decimal? sph, decimal? cyl, int? axis, decimal? add)
        {
            if (sph is null && cyl is null && add is null)
                return null;
            var teks = OptPrescriptionRules.Notation(sph, cyl, axis);
            return add is { } a ? $"{teks} ADD {OptPrescriptionRules.Format(a)}" : teks;
        }

        private static string? Mata(string? od, string? os) =>
            od is null && os is null ? null : $"OD {od ?? "—"}, OS {os ?? "—"}";

        private static string? Pd(decimal? jauh, decimal? dekat)
        {
            static string Mm(decimal x) => x.ToString("0.#", CultureInfo.InvariantCulture).Replace('.', ',') + " mm";
            if (jauh is null && dekat is null) return null;
            return string.Join(", ", new[] { jauh is { } j ? "jauh " + Mm(j) : null, dekat is { } d ? "dekat " + Mm(d) : null }.Where(x => x is not null));
        }

        private static string Riwayat(IEnumerable<(int No, DateTime Waktu, string? Nama, string? Alasan)> versi) =>
            string.Join("; ", versi.Select(v =>
                $"v{v.No} {TulisWaktu(v.Waktu)}{(v.Nama is null ? string.Empty : " " + v.Nama)}{(string.IsNullOrWhiteSpace(v.Alasan) ? string.Empty : $" — {v.Alasan}")}"));

        private static string TulisWaktu(DateTime utc)
        {
            var wib = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta"));
            return wib.ToString("dd-MM-yyyy HH.mm", CultureInfo.InvariantCulture) + " WIB";
        }

        private static void Tambah(List<MedicalRecordDocumentSectionResponse> bagian, string label, string? nilai)
        {
            if (!string.IsNullOrWhiteSpace(nilai))
                bagian.Add(new MedicalRecordDocumentSectionResponse { Label = label, Value = nilai });
        }

        private static string Potong(string teks) =>
            teks.Length <= PanjangRingkasanMaksimal ? teks : teks[..(PanjangRingkasanMaksimal - 1)] + "…";
    }
}
