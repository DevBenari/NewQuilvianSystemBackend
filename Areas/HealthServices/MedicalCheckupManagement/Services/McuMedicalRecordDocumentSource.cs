using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services.Sources;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services
{
    /// <summary>
    /// Pembaca resume dan hasil pemeriksaan MCU untuk berkas rekam medis (`INT-RM-10j`, `BE-36`).
    ///
    /// Ditulis di modul MCU dengan persetujuan pemiliknya (`RM-DEC-063`). Hanya order yang sudah
    /// tertaut ke pasien (<c>McuOrder.PatientId</c>) yang terbaca; peserta tanpa nomor rekam medis
    /// tidak punya berkas, dan penautannya urusan MCU (`DEC-MCU-004`).
    ///
    /// VERSI YANG TAMPIL (`RM-DEC-065`). Resume MCU berversi: membuka kembali resume yang sudah
    /// diverifikasi menandai versi lama <c>Corrected</c> dan membuat versi baru berstatus
    /// <c>Draft</c> (<c>McuResumeService</c>, buka ulang). Berkas selalu menampilkan versi terakhir
    /// yang PERNAH diverifikasi (<c>VerifiedAt</c> terisi) — status <c>Corrected</c> pada versi itu
    /// berarti "sedang/sudah digantikan", BUKAN "final yang diralat". Selama koreksi berjalan,
    /// versi itu tetap tampil dengan keterangan "sedang dikoreksi". Versi ke-2 ke atas yang
    /// sudah diverifikasi bertanda pernah diralat.
    ///
    /// HASIL PEMERIKSAAN ikut resume (`RM-DEC-066`): tampil bila order punya versi terverifikasi,
    /// dengan waktu final = waktu verifikasi itu. Nilai yang diubah sesudah versi itu direkonstruksi
    /// dari <c>McuExaminationResultValueRevision</c>, sehingga isi koreksi yang belum diverifikasi
    /// tidak bocor. KESIMPULAN hasil ditimpa MCU tanpa riwayat, sehingga selama koreksi berjalan
    /// kesimpulan tidak ditampilkan — lebih baik kosong berketerangan daripada isi yang belum
    /// diverifikasi.
    /// </summary>
    public class McuMedicalRecordDocumentSource : IMedicalRecordDocumentSource, IMedicalRecordCompletenessProbe
    {
        private const int PanjangRingkasanMaksimal = 300;
        private const string KeteranganKoreksi = "Sedang dikoreksi — yang tampil adalah versi terakhir yang sudah diverifikasi.";

        private readonly ApplicationDbContext _dbContext;

        public McuMedicalRecordDocumentSource(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string SourceModule => "MCU";

        public IReadOnlyCollection<ClinicalDocumentKind> Kinds { get; } =
            [ClinicalDocumentKind.McuResume, ClinicalDocumentKind.McuExaminationResult];

        public IReadOnlyCollection<CompletenessCriterion> Criteria { get; } = [CompletenessCriterion.McuResume];

        public IReadOnlyCollection<CompletenessCondition> Conditions { get; } = [];

        public bool ReportsPendingResults => false;

        public async Task<MedicalRecordSourcePage> ReadAsync(
            MedicalRecordSourceQuery query,
            CancellationToken cancellationToken)
        {
            var hasil = new MedicalRecordSourcePage();

            var mintaResume = query.Kinds.Contains(ClinicalDocumentKind.McuResume);
            var mintaHasil = query.Kinds.Contains(ClinicalDocumentKind.McuExaminationResult);
            if (!mintaResume && !mintaHasil)
                return hasil;

            var order = await OrderTerverifikasiAsync(query.PatientId, query.EncounterId, cancellationToken);
            order = order
                .Where(x => !query.StartDate.HasValue || x.Versi.VerifiedAt >= query.StartDate.Value)
                .Where(x => !query.EndDate.HasValue || x.Versi.VerifiedAt <= query.EndDate.Value)
                .ToList();

            var dokumen = new List<MedicalRecordFinalDocumentDescriptor>();

            if (mintaResume)
            {
                hasil.Totals[ClinicalDocumentKind.McuResume] = order.Count;
                dokumen.AddRange(order.Select(x => new MedicalRecordFinalDocumentDescriptor
                {
                    Kind = ClinicalDocumentKind.McuResume,
                    DocumentId = x.Versi.Id,
                    EncounterId = x.EncounterId,
                    FinalizedAt = x.Versi.VerifiedAt,
                    DocumentNumber = x.OrderNumber,
                    Title = $"Resume MCU — {x.PackageName}",
                    Summary = Potong(Gabung(
                        x.SedangDikoreksi ? "Sedang dikoreksi." : null,
                        x.Versi.FitnessCategoryName == null ? null : $"Kelaikan: {x.Versi.FitnessCategoryName}.",
                        x.Versi.ImportantFindingsText)),
                    IsCorrected = x.Versi.VersionNumber > 1,
                    AuthorUserId = x.Versi.VerifiedByUserId
                }));
            }

            if (mintaHasil && order.Count > 0)
            {
                var hasilPemeriksaan = await HasilPemeriksaanAsync(order, null, cancellationToken);
                hasil.Totals[ClinicalDocumentKind.McuExaminationResult] = hasilPemeriksaan.Count;
                dokumen.AddRange(hasilPemeriksaan.Select(x => new MedicalRecordFinalDocumentDescriptor
                {
                    Kind = ClinicalDocumentKind.McuExaminationResult,
                    DocumentId = x.Id,
                    EncounterId = x.Order.EncounterId,
                    FinalizedAt = x.Order.Versi.VerifiedAt,
                    DocumentNumber = x.Order.OrderNumber,
                    Title = x.Nama,
                    Summary = Potong(x.Order.SedangDikoreksi ? "Kesimpulan sedang dikoreksi." : x.Kesimpulan),
                    IsCorrected = x.PernahDiralat
                }));
            }
            else if (mintaHasil)
            {
                hasil.Totals[ClinicalDocumentKind.McuExaminationResult] = 0;
            }

            if (query.Limit <= 0)
                return hasil;

            hasil.Items = dokumen
                .GroupBy(x => x.Kind)
                .SelectMany(g => (query.NewestFirst
                        ? g.OrderByDescending(x => x.FinalizedAt)
                        : g.OrderBy(x => x.FinalizedAt))
                    .Take(query.Limit))
                .ToList();

            return hasil;
        }

        public async Task<MedicalRecordSourceDocumentDetail?> ReadDetailAsync(
            ClinicalDocumentKind kind,
            Guid documentId,
            Guid patientId,
            CancellationToken cancellationToken)
        {
            if (kind == ClinicalDocumentKind.McuResume)
            {
                var order = (await OrderTerverifikasiAsync(patientId, null, cancellationToken))
                    .FirstOrDefault(x => x.Versi.Id == documentId);
                if (order == null)
                    return null;

                var v = order.Versi;
                var bagian = new List<MedicalRecordDocumentSectionResponse>();
                Tambah(bagian, "Keadaan", order.SedangDikoreksi ? KeteranganKoreksi : null);
                Tambah(bagian, "Paket", order.PackageName);
                Tambah(bagian, "Kondisi umum", v.GeneralConditionText);
                Tambah(bagian, "Temuan penting", v.ImportantFindingsText);
                Tambah(bagian, "Saran", v.RecommendationText);
                Tambah(bagian, "Kategori kelaikan", v.FitnessCategoryName);
                Tambah(bagian, "Saran kerja", v.WorkRecommendationText);
                Tambah(bagian, "Nomor order", order.OrderNumber);
                Tambah(bagian, "Versi", v.VersionNumber.ToString(CultureInfo.InvariantCulture));
                Tambah(bagian, "Diverifikasi", TulisWaktu(v.VerifiedAt));

                return new MedicalRecordSourceDocumentDetail
                {
                    EncounterId = order.EncounterId,
                    DocumentNumber = order.OrderNumber,
                    Title = $"Resume MCU — {order.PackageName}",
                    FinalizedAt = v.VerifiedAt,
                    IsCorrected = v.VersionNumber > 1,
                    Sections = bagian
                };
            }

            if (kind == ClinicalDocumentKind.McuExaminationResult)
            {
                var order = await OrderTerverifikasiAsync(patientId, null, cancellationToken);
                var x = (await HasilPemeriksaanAsync(order, documentId, cancellationToken)).FirstOrDefault();
                if (x == null)
                    return null;

                var bagian = new List<MedicalRecordDocumentSectionResponse>();
                Tambah(bagian, "Keadaan", x.Order.SedangDikoreksi ? KeteranganKoreksi : null);
                Tambah(bagian, "Pemeriksaan", x.Nama);
                foreach (var nilai in x.Nilai)
                    Tambah(bagian, nilai.Label, nilai.Teks);
                Tambah(bagian, "Kesimpulan", x.Order.SedangDikoreksi ? "Sedang dikoreksi." : x.Kesimpulan);
                Tambah(bagian, "Nomor order", x.Order.OrderNumber);
                Tambah(bagian, "Diverifikasi", TulisWaktu(x.Order.Versi.VerifiedAt));

                return new MedicalRecordSourceDocumentDetail
                {
                    EncounterId = x.Order.EncounterId,
                    DocumentNumber = x.Order.OrderNumber,
                    Title = x.Nama,
                    FinalizedAt = x.Order.Versi.VerifiedAt,
                    IsCorrected = x.PernahDiralat,
                    Sections = bagian
                };
            }

            return null;
        }

        public async Task<MedicalRecordCompletenessProbeResult> ProbeAsync(
            MedicalRecordCompletenessProbeQuery query,
            CancellationToken cancellationToken)
        {
            // Kriteria `McuResume` terpenuhi bila kunjungan punya resume yang pernah diverifikasi —
            // sama dengan yang tampil di berkas (`RM-DEC-065`).
            var hasil = new MedicalRecordCompletenessProbeResult();
            var order = await OrderTerverifikasiAsync(query.PatientId, query.EncounterId, cancellationToken);
            if (order.Count > 0)
                hasil.SatisfiedCriteria.Add(CompletenessCriterion.McuResume);
            return hasil;
        }

        // =====================================================================
        // Pembacaan
        // =====================================================================

        private sealed record VersiResume(
            Guid Id,
            Guid McuOrderId,
            int VersionNumber,
            DateTime VerifiedAt,
            Guid? VerifiedByUserId,
            string? GeneralConditionText,
            string? ImportantFindingsText,
            string? RecommendationText,
            string? FitnessCategoryName,
            string? WorkRecommendationText);

        private sealed record OrderTerverifikasi(
            Guid Id,
            Guid? EncounterId,
            string OrderNumber,
            string PackageName,
            VersiResume Versi,
            bool SedangDikoreksi);

        private sealed record NilaiHasil(string Label, string? Teks);

        private sealed record HasilTerbaca(
            Guid Id,
            OrderTerverifikasi Order,
            string Nama,
            string? Kesimpulan,
            List<NilaiHasil> Nilai,
            bool PernahDiralat);

        /// <summary>
        /// Order milik pasien beserta versi resume terakhir yang pernah diverifikasi. Order tanpa
        /// versi terverifikasi tidak dikembalikan. Satu-satunya tempat aturan `RM-DEC-065` ditulis.
        /// </summary>
        private async Task<List<OrderTerverifikasi>> OrderTerverifikasiAsync(
            Guid patientId,
            Guid? encounterId,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders
                .AsNoTracking()
                .Where(x => x.PatientId == patientId
                            && !x.IsDelete
                            && x.OrderStatus != McuOrderStatus.Cancelled
                            && (!encounterId.HasValue || x.EncounterId == encounterId.Value))
                .Select(x => new { x.Id, x.EncounterId, x.OrderNumber, x.PackageNameSnapshot })
                .ToListAsync(cancellationToken);

            if (order.Count == 0)
                return [];

            var idOrder = order.Select(x => x.Id).ToList();
            var versi = await _dbContext.McuResumes
                .AsNoTracking()
                .Where(x => idOrder.Contains(x.McuOrderId) && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.McuOrderId,
                    x.VersionNumber,
                    x.VerifiedAt,
                    x.VerifiedByUserId,
                    x.GeneralConditionText,
                    x.ImportantFindingsText,
                    x.RecommendationText,
                    Kelaikan = x.FitnessCategory != null ? x.FitnessCategory.FitnessCategoryName : null,
                    x.WorkRecommendationText
                })
                .ToListAsync(cancellationToken);

            var hasil = new List<OrderTerverifikasi>();
            foreach (var o in order)
            {
                var milikOrder = versi.Where(v => v.McuOrderId == o.Id).ToList();
                var terverifikasi = milikOrder
                    .Where(v => v.VerifiedAt.HasValue)
                    .OrderByDescending(v => v.VersionNumber)
                    .FirstOrDefault();

                if (terverifikasi == null)
                    continue;

                hasil.Add(new OrderTerverifikasi(
                    o.Id,
                    o.EncounterId,
                    o.OrderNumber,
                    o.PackageNameSnapshot,
                    new VersiResume(
                        terverifikasi.Id,
                        o.Id,
                        terverifikasi.VersionNumber,
                        terverifikasi.VerifiedAt!.Value,
                        terverifikasi.VerifiedByUserId,
                        terverifikasi.GeneralConditionText,
                        terverifikasi.ImportantFindingsText,
                        terverifikasi.RecommendationText,
                        terverifikasi.Kelaikan,
                        terverifikasi.WorkRecommendationText),
                    SedangDikoreksi: milikOrder.Any(v => v.VersionNumber > terverifikasi.VersionNumber)));
            }

            return hasil;
        }

        /// <summary>
        /// Hasil pemeriksaan order terverifikasi, dengan nilai sebagaimana pada versi terverifikasi.
        ///
        /// <c>McuExaminationResultValueRevision.ResumeVersionNumber</c> adalah versi yang sedang
        /// dikoreksi saat nilai diubah, dan <c>PreviousValueText</c> nilainya pada versi itu. Maka
        /// nilai pada versi terverifikasi N = nilai lama revisi PERTAMA dengan versi ≥ N, atau nilai
        /// sekarang bila tidak ada. Revisi dengan versi &lt; N berarti nilai pernah diralat lalu
        /// ralatnya diverifikasi.
        /// </summary>
        private async Task<List<HasilTerbaca>> HasilPemeriksaanAsync(
            List<OrderTerverifikasi> order,
            Guid? resultId,
            CancellationToken cancellationToken)
        {
            if (order.Count == 0)
                return [];

            var perOrder = order.ToDictionary(x => x.Id);
            var idOrder = perOrder.Keys.ToList();

            var baris = await _dbContext.McuExaminationResults
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.McuOrderComponent != null
                            && idOrder.Contains(x.McuOrderComponent.McuOrderId)
                            && (!resultId.HasValue || x.Id == resultId.Value))
                .Select(x => new
                {
                    x.Id,
                    OrderId = x.McuOrderComponent!.McuOrderId,
                    Nama = x.McuOrderComponent.ExaminationType != null
                        ? x.McuOrderComponent.ExaminationType.ExaminationTypeName
                        : x.McuOrderComponent.ComponentNameSnapshot,
                    x.ConclusionText,
                    Nilai = x.Values
                        .Where(v => !v.IsDelete)
                        .Select(v => new
                        {
                            Label = v.ExaminationField != null ? v.ExaminationField.FieldLabel : null,
                            Satuan = v.ExaminationField != null ? v.ExaminationField.UnitName : null,
                            Urutan = v.ExaminationField != null ? v.ExaminationField.FieldNumber : 0,
                            v.ValueText,
                            Revisi = v.Revisions
                                .Where(r => !r.IsDelete)
                                .Select(r => new { r.ResumeVersionNumber, r.PreviousValueText, r.ChangedAt })
                                .ToList()
                        })
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            return baris.Select(x =>
            {
                var o = perOrder[x.OrderId];
                var n = o.Versi.VersionNumber;

                var nilai = x.Nilai
                    .OrderBy(v => v.Urutan)
                    .Select(v =>
                    {
                        var sesudahVersi = v.Revisi
                            .Where(r => r.ResumeVersionNumber >= n)
                            .OrderBy(r => r.ChangedAt)
                            .FirstOrDefault();
                        var teks = sesudahVersi != null ? sesudahVersi.PreviousValueText : v.ValueText;
                        return new NilaiHasil(
                            v.Label ?? "Nilai",
                            string.IsNullOrWhiteSpace(teks) ? null : $"{teks} {v.Satuan}".Trim());
                    })
                    .ToList();

                var pernahDiralat = x.Nilai.Any(v => v.Revisi.Any(r => r.ResumeVersionNumber < n));

                return new HasilTerbaca(x.Id, o, x.Nama, x.ConclusionText, nilai, pernahDiralat);
            }).ToList();
        }

        private static string? Gabung(params string?[] bagian)
        {
            var isi = bagian.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            return isi.Count == 0 ? null : string.Join(" ", isi);
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
