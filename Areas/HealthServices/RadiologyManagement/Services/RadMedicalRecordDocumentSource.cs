using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services.Sources;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services
{
    /// <summary>
    /// Pembaca laporan bacaan radiologi untuk berkas rekam medis (`INT-RM-10b`, `BE-24`).
    ///
    /// Ditulis di modul Radiologi dengan persetujuan pemiliknya (`RM-DEC-064`).
    ///
    /// DAFTAR memakai <see cref="RadReportService.GetByPatientAsync"/> apa adanya, dipanggil di
    /// dalam proses. Penyaringnya sudah tepat untuk berkas: hanya bacaan yang sudah melewati
    /// perilisan dan benar-benar punya waktu rilis (`FR-RAD-032`), kunjungan pasien dibaca lewat
    /// subquery satu perjalanan. Rute HTTP-nya sengaja TIDAK dipakai: rute itu dijaga
    /// <c>RadReport : Read</c>, sedangkan berkas hanya menuntut <c>MedicalRecord : Read</c>
    /// (`RM-DEC-040`).
    ///
    /// AMANDEMEN TIDAK BOCOR. Selama koreksi sedang disusun, <c>CurrentVersionNumber</c> tetap
    /// menunjuk versi rilis sebelumnya dan baru bergerak saat koreksi dirilis
    /// (<c>RadReportService.cs</c>, bagian penyusunan koreksi). Karena itu kesimpulan yang tampil
    /// selalu milik versi yang sudah dirilis, dan <c>CurrentVersionNumber &gt; 1</c> berarti
    /// laporan itu sudah pernah diralat dan ralatnya sudah dirilis.
    /// </summary>
    public class RadMedicalRecordDocumentSource : IMedicalRecordDocumentSource, IMedicalRecordCompletenessProbe
    {
        private const int PanjangRingkasanMaksimal = 300;

        /// <summary>Sama dengan <c>RadReportService.BelumDirilisStatuses</c>: keadaan koreksi tidak ikut.</summary>
        private static readonly RadReportStatus[] BelumDirilis =
        [
            RadReportStatus.Pending,
            RadReportStatus.Drafted,
            RadReportStatus.Validated
        ];

        /// <summary>Order yang tidak lagi menunggu hasil: belum diajukan, batal, atau ditolak.</summary>
        private static readonly RadOrderStatus[] BukanTertunda =
        [
            RadOrderStatus.Draft,
            RadOrderStatus.Cancelled,
            RadOrderStatus.Rejected
        ];

        private readonly ApplicationDbContext _dbContext;
        private readonly RadReportService _radReportService;

        public RadMedicalRecordDocumentSource(ApplicationDbContext dbContext, RadReportService radReportService)
        {
            _dbContext = dbContext;
            _radReportService = radReportService;
        }

        public string SourceModule => "Radiologi";

        public IReadOnlyCollection<ClinicalDocumentKind> Kinds { get; } = [ClinicalDocumentKind.RadiologyReport];

        public IReadOnlyCollection<CompletenessCriterion> Criteria { get; } = [];

        public IReadOnlyCollection<CompletenessCondition> Conditions { get; } = [];

        public bool ReportsPendingResults => true;

        public async Task<MedicalRecordSourcePage> ReadAsync(
            MedicalRecordSourceQuery query,
            CancellationToken cancellationToken)
        {
            var hasil = new MedicalRecordSourcePage();

            if (!query.Kinds.Contains(ClinicalDocumentKind.RadiologyReport))
                return hasil;

            var laporan = (await _radReportService.GetByPatientAsync(query.PatientId, cancellationToken))
                .Where(x => x.FirstReleasedAt.HasValue)
                .Where(x => !query.EncounterId.HasValue || x.EncounterId == query.EncounterId.Value)
                .Where(x => !query.StartDate.HasValue || x.FirstReleasedAt >= query.StartDate.Value)
                .Where(x => !query.EndDate.HasValue || x.FirstReleasedAt <= query.EndDate.Value)
                .ToList();

            hasil.Totals[ClinicalDocumentKind.RadiologyReport] = laporan.Count;

            if (query.Limit <= 0 || laporan.Count == 0)
                return hasil;

            var terurut = (query.NewestFirst
                    ? laporan.OrderByDescending(x => x.FirstReleasedAt)
                    : laporan.OrderBy(x => x.FirstReleasedAt))
                .Take(query.Limit)
                .ToList();

            // Nama pemeriksaan tidak ada pada baris daftar; diambil sekaligus untuk seluruh baris.
            var idOrder = terurut.Select(x => x.RadOrderId).Distinct().ToList();
            var pemeriksaan = await _dbContext.RadOrders
                .AsNoTracking()
                .Where(x => idOrder.Contains(x.Id))
                .Select(x => new { x.Id, Nama = x.Procedure != null ? x.Procedure.ProcedureName : null })
                .ToDictionaryAsync(x => x.Id, x => x.Nama, cancellationToken);

            hasil.Items = terurut
                .Select(x => new MedicalRecordFinalDocumentDescriptor
                {
                    Kind = ClinicalDocumentKind.RadiologyReport,
                    DocumentId = x.Id,
                    EncounterId = x.EncounterId,
                    FinalizedAt = x.FirstReleasedAt!.Value,
                    DocumentNumber = x.ReportNumber,
                    Title = pemeriksaan.GetValueOrDefault(x.RadOrderId) ?? "Hasil Radiologi",
                    Summary = Potong(x.Impression),
                    IsCorrected = x.CurrentVersionNumber > 1,
                    IsProvisional = false,
                    AuthorUserId = x.AuthorUserId
                })
                .ToList();

            return hasil;
        }

        public async Task<MedicalRecordSourceDocumentDetail?> ReadDetailAsync(
            ClinicalDocumentKind kind,
            Guid documentId,
            Guid patientId,
            CancellationToken cancellationToken)
        {
            if (kind != ClinicalDocumentKind.RadiologyReport)
                return null;

            var kunjunganPasien = KunjunganPasien(patientId);

            // Aturan rilis sama dengan daftar; kepemilikan pasien diperiksa di query yang sama.
            var laporan = await _dbContext.RadReports
                .AsNoTracking()
                .Where(x => x.Id == documentId
                            && !x.IsDelete
                            && !BelumDirilis.Contains(x.ReportStatus)
                            && x.FirstReleasedAt != null
                            && kunjunganPasien.Contains(x.EncounterId))
                .Select(x => new
                {
                    x.EncounterId,
                    x.ReportNumber,
                    x.CurrentVersionNumber,
                    FirstReleasedAt = x.FirstReleasedAt!.Value,
                    x.LastReleasedAt,
                    x.RadOrderId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (laporan == null)
                return null;

            var versi = await _dbContext.RadReportVersions
                .AsNoTracking()
                .Where(x => x.RadReportId == documentId
                            && x.VersionNumber == laporan.CurrentVersionNumber
                            && !x.IsDelete)
                .Select(x => new { x.Findings, x.Impression, x.Recommendation, x.IsAmendment, x.AmendmentReason })
                .FirstOrDefaultAsync(cancellationToken);

            var order = await _dbContext.RadOrders
                .AsNoTracking()
                .Where(x => x.Id == laporan.RadOrderId)
                .Select(x => new { x.OrderNumber, Nama = x.Procedure != null ? x.Procedure.ProcedureName : null })
                .FirstOrDefaultAsync(cancellationToken);

            var bagian = new List<MedicalRecordDocumentSectionResponse>();
            Tambah(bagian, "Pemeriksaan", order?.Nama);
            Tambah(bagian, "Temuan", versi?.Findings);
            Tambah(bagian, "Kesan", versi?.Impression);
            Tambah(bagian, "Saran", versi?.Recommendation);
            Tambah(bagian, "Nomor bacaan", laporan.ReportNumber);
            Tambah(bagian, "Nomor order", order?.OrderNumber);
            Tambah(bagian, "Pertama dirilis", TulisWaktu(laporan.FirstReleasedAt));
            if (laporan.CurrentVersionNumber > 1)
            {
                Tambah(bagian, "Versi berlaku", laporan.CurrentVersionNumber.ToString(CultureInfo.InvariantCulture));
                Tambah(bagian, "Ralat terakhir dirilis", TulisWaktu(laporan.LastReleasedAt));
                Tambah(bagian, "Alasan ralat", versi?.IsAmendment == true ? versi.AmendmentReason : null);
            }

            return new MedicalRecordSourceDocumentDetail
            {
                EncounterId = laporan.EncounterId,
                DocumentNumber = laporan.ReportNumber,
                Title = order?.Nama ?? "Hasil Radiologi",
                FinalizedAt = laporan.FirstReleasedAt,
                IsCorrected = laporan.CurrentVersionNumber > 1,
                IsProvisional = false,
                Sections = bagian
            };
        }

        public async Task<MedicalRecordCompletenessProbeResult> ProbeAsync(
            MedicalRecordCompletenessProbeQuery query,
            CancellationToken cancellationToken)
        {
            // Syarat 2 `RM-DEC-045`: order yang masih berjalan tetapi belum punya bacaan yang
            // dirilis. Order yang belum diajukan, batal, atau ditolak tidak ditunggu hasilnya.
            var tertunda = await _dbContext.RadOrders
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.EncounterId == query.EncounterId
                            && !BukanTertunda.Contains(x.OrderStatus)
                            && !_dbContext.RadReports.Any(r => r.RadOrderId == x.Id
                                                               && !r.IsDelete
                                                               && r.FirstReleasedAt != null
                                                               && !BelumDirilis.Contains(r.ReportStatus)))
                .Select(x => new { x.Id, Nama = x.Procedure != null ? x.Procedure.ProcedureName : null })
                .ToListAsync(cancellationToken);

            return new MedicalRecordCompletenessProbeResult
            {
                PendingResults = tertunda
                    .Select(x => new MedicalRecordPendingResult
                    {
                        Kind = ClinicalDocumentKind.RadiologyReport,
                        DocumentId = x.Id,
                        Name = x.Nama
                    })
                    .ToList()
            };
        }

        private IQueryable<Guid> KunjunganPasien(Guid patientId) => _dbContext.RegPatientEncounters
            .AsNoTracking()
            .Where(x => x.PatientId == patientId && !x.IsDelete)
            .Select(x => x.Id);

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
