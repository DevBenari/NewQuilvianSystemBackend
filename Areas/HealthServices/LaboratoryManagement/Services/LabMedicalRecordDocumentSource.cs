using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services.Sources;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Pembaca hasil laboratorium untuk berkas rekam medis (`INT-RM-10a`, `BE-23`).
    ///
    /// Ditulis di modul Laboratorium dengan persetujuan pemiliknya (`RM-DEC-063`), karena
    /// `RM-DEC-038` menetapkan pemilik dokumen yang menerjemahkan id pasien. Laboratorium tidak
    /// menyimpan `PatientId` (`RM-FACT-010`); kunjungan pasien dicari lewat subquery pada
    /// <c>RegPatientEncounters</c> dalam satu perjalanan ke database — pola yang sama dengan
    /// <c>RadReportService.GetByPatientAsync</c>. Nol kolom ditambahkan pada tabel Laboratorium
    /// (`RM-DEC-031`).
    ///
    /// YANG DIBACA. Hanya hasil Patologi Klinik dan Mikrobiologi yang sudah DIRILIS
    /// (<c>ReleasedAt</c> terisi, `RM-DEC-041`). Hasil Patologi Anatomi tidak pernah dibaca:
    /// laporan PA dapat dibuka kembali sesudah rilis (`RM-FACT-020`), sehingga ditahan dari
    /// berkas (`RM-DEC-058`). Karena pembaca ini tidak mengaku memiliki
    /// <see cref="ClinicalDocumentKind.PathologyAnatomyResult"/>, berkas melaporkannya "belum
    /// tersedia".
    ///
    /// PEMERIKSAAN TERTUNDA. Sebaliknya, pemeriksaan PA yang belum dirilis TETAP dilaporkan
    /// tertunda: berkas pasien yang masih menunggu hasil PA memang belum lengkap (`RM-DEC-035`).
    ///
    /// PERNAH DIRALAT selalu <c>false</c>. Hasil non-PA yang sudah divalidasi tidak dapat diubah
    /// (`RM-FACT-019`), dan koreksi hasil terrilis (`S6`) belum dibangun. `S6` tidak boleh
    /// dirilis sebelum penanda ini disambungkan (`RM-DEC-057`).
    ///
    /// Disiplin sebuah pemeriksaan mengikuti aturan Laboratorium sendiri: disiplin order, lalu
    /// disiplin katalog pemeriksaan bagi order lama berdisiplin kosong (`LAB-DEC-048`,
    /// <c>LabExaminationService.ResolveDiscipline</c>).
    /// </summary>
    public class LabMedicalRecordDocumentSource : IMedicalRecordDocumentSource, IMedicalRecordCompletenessProbe
    {
        private const int PanjangRingkasanMaksimal = 300;

        private readonly ApplicationDbContext _dbContext;

        public LabMedicalRecordDocumentSource(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string SourceModule => "Laboratorium";

        public IReadOnlyCollection<ClinicalDocumentKind> Kinds { get; } = [ClinicalDocumentKind.LaboratoryResult];

        public IReadOnlyCollection<CompletenessCriterion> Criteria { get; } = [];

        public IReadOnlyCollection<CompletenessCondition> Conditions { get; } = [];

        public bool ReportsPendingResults => true;

        public async Task<MedicalRecordSourcePage> ReadAsync(
            MedicalRecordSourceQuery query,
            CancellationToken cancellationToken)
        {
            var hasil = new MedicalRecordSourcePage();

            if (!query.Kinds.Contains(ClinicalDocumentKind.LaboratoryResult))
                return hasil;

            var dasar = HasilTerrilisPasien(query.PatientId);

            if (query.EncounterId.HasValue)
                dasar = dasar.Where(x => x.LabOrder!.EncounterId == query.EncounterId.Value);

            if (query.StartDate.HasValue)
                dasar = dasar.Where(x => x.ReleasedAt >= query.StartDate.Value);

            if (query.EndDate.HasValue)
                dasar = dasar.Where(x => x.ReleasedAt <= query.EndDate.Value);

            hasil.Totals[ClinicalDocumentKind.LaboratoryResult] = await dasar.CountAsync(cancellationToken);

            if (query.Limit <= 0)
                return hasil;

            var terurut = query.NewestFirst
                ? dasar.OrderByDescending(x => x.ReleasedAt)
                : dasar.OrderBy(x => x.ReleasedAt);

            var baris = await terurut
                .Take(query.Limit)
                .Select(x => new
                {
                    x.Id,
                    x.LabOrder!.EncounterId,
                    x.LabOrder.OrderNumber,
                    ReleasedAt = x.ReleasedAt!.Value,
                    CollectedAt = x.Specimen != null ? x.Specimen.CollectedAt : null,
                    NamaPemeriksaan = x.ProcedureNameSnapshot ?? x.Procedure!.ProcedureName,
                    x.ResultNumeric,
                    x.ResultUnitSnapshot,
                    Pilihan = x.ResultOption != null ? x.ResultOption.OptionName : null,
                    x.MicrobiologyFinding,
                    x.ResultQualifier,
                    x.ResultEnteredByUserId
                })
                .ToListAsync(cancellationToken);

            hasil.Items = baris
                .Select(x => new MedicalRecordFinalDocumentDescriptor
                {
                    Kind = ClinicalDocumentKind.LaboratoryResult,
                    DocumentId = x.Id,
                    EncounterId = x.EncounterId,
                    FinalizedAt = x.ReleasedAt,
                    OccurredAt = x.CollectedAt,
                    DocumentNumber = x.OrderNumber,
                    Title = x.NamaPemeriksaan,
                    Summary = Potong(TulisHasil(
                        x.ResultNumeric, x.ResultUnitSnapshot, x.Pilihan, x.MicrobiologyFinding, x.ResultQualifier)),
                    IsCorrected = false,
                    IsProvisional = false,
                    AuthorUserId = x.ResultEnteredByUserId
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
            if (kind != ClinicalDocumentKind.LaboratoryResult)
                return null;

            // Kepemilikan pasien diperiksa di query yang sama: id hasil milik pasien lain
            // menghasilkan null, bukan isinya.
            var x = await HasilTerrilisPasien(patientId)
                .Where(e => e.Id == documentId)
                .Select(e => new
                {
                    e.LabOrder!.EncounterId,
                    e.LabOrder.OrderNumber,
                    e.LabOrder.LabReportNumber,
                    ReleasedAt = e.ReleasedAt!.Value,
                    e.ValidatedAt,
                    e.ExaminedAt,
                    CollectedAt = e.Specimen != null ? e.Specimen.CollectedAt : null,
                    NamaPemeriksaan = e.ProcedureNameSnapshot ?? e.Procedure!.ProcedureName,
                    e.ResultNumeric,
                    e.ResultUnitSnapshot,
                    Pilihan = e.ResultOption != null ? e.ResultOption.OptionName : null,
                    e.MicrobiologyFinding,
                    e.ResultQualifier,
                    e.ValidatedByPositionNameSnapshot,
                    e.ReleasedByPositionNameSnapshot
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (x == null)
                return null;

            var bagian = new List<MedicalRecordDocumentSectionResponse>();
            Tambah(bagian, "Pemeriksaan", x.NamaPemeriksaan);
            Tambah(bagian, "Hasil", TulisHasil(x.ResultNumeric, x.ResultUnitSnapshot, x.Pilihan, x.MicrobiologyFinding, x.ResultQualifier));
            Tambah(bagian, "Nomor order", x.OrderNumber);
            Tambah(bagian, "Nomor laporan", x.LabReportNumber);
            Tambah(bagian, "Spesimen diambil", TulisWaktu(x.CollectedAt));
            Tambah(bagian, "Diperiksa", TulisWaktu(x.ExaminedAt));
            Tambah(bagian, "Divalidasi", TulisWaktu(x.ValidatedAt));
            Tambah(bagian, "Jabatan validator", x.ValidatedByPositionNameSnapshot);
            Tambah(bagian, "Dirilis", TulisWaktu(x.ReleasedAt));
            Tambah(bagian, "Jabatan perilis", x.ReleasedByPositionNameSnapshot);

            return new MedicalRecordSourceDocumentDetail
            {
                EncounterId = x.EncounterId,
                DocumentNumber = x.OrderNumber,
                Title = x.NamaPemeriksaan,
                FinalizedAt = x.ReleasedAt,
                OccurredAt = x.CollectedAt,
                IsCorrected = false,
                IsProvisional = false,
                Sections = bagian
            };
        }

        public async Task<MedicalRecordCompletenessProbeResult> ProbeAsync(
            MedicalRecordCompletenessProbeQuery query,
            CancellationToken cancellationToken)
        {
            // Syarat 2 `RM-DEC-045`: pemeriksaan yang dipesan tetapi belum dirilis, TERMASUK
            // Patologi Anatomi. Pemeriksaan batal dan dihapus dari tagihan (Voided) tidak
            // ditunggu hasilnya.
            var tertunda = await _dbContext.LabExaminations
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.ReleasedAt == null
                            && x.ExaminationStatus != LabExaminationStatus.Voided
                            && x.ExaminationStatus != LabExaminationStatus.Cancelled
                            && x.LabOrder != null
                            && !x.LabOrder.IsDelete
                            && x.LabOrder.EncounterId == query.EncounterId)
                .Select(x => new
                {
                    x.Id,
                    NamaPemeriksaan = x.ProcedureNameSnapshot ?? x.Procedure!.ProcedureName,
                    Disiplin = x.LabOrder!.Discipline ?? x.Procedure!.LabDiscipline
                })
                .ToListAsync(cancellationToken);

            return new MedicalRecordCompletenessProbeResult
            {
                PendingResults = tertunda
                    .Select(x => new MedicalRecordPendingResult
                    {
                        Kind = x.Disiplin == LabDiscipline.AnatomicalPathology
                            ? ClinicalDocumentKind.PathologyAnatomyResult
                            : ClinicalDocumentKind.LaboratoryResult,
                        DocumentId = x.Id,
                        Name = x.NamaPemeriksaan
                    })
                    .ToList()
            };
        }

        /// <summary>
        /// Hasil non-PA yang sudah dirilis milik satu pasien. Satu-satunya tempat aturan "final"
        /// dan pengecualian PA ditulis, dipakai daftar maupun rincian.
        /// </summary>
        private IQueryable<LabExamination> HasilTerrilisPasien(Guid patientId)
        {
            var kunjunganPasien = _dbContext.RegPatientEncounters
                .AsNoTracking()
                .Where(x => x.PatientId == patientId && !x.IsDelete)
                .Select(x => x.Id);

            return _dbContext.LabExaminations
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.ReleasedAt != null
                            && x.ExaminationStatus != LabExaminationStatus.Voided
                            && x.ExaminationStatus != LabExaminationStatus.Cancelled
                            && x.LabOrder != null
                            && !x.LabOrder.IsDelete
                            && kunjunganPasien.Contains(x.LabOrder.EncounterId)
                            && (x.LabOrder.Discipline ?? x.Procedure!.LabDiscipline) != LabDiscipline.AnatomicalPathology);
        }

        private static string? TulisHasil(
            decimal? angka,
            string? satuan,
            string? pilihan,
            LabMicrobiologyFinding? temuan,
            LabResultQualifier? kualifikasi)
        {
            var nilai = angka.HasValue
                ? $"{angka.Value.ToString("0.####", CultureInfo.GetCultureInfo("id-ID"))} {satuan}".Trim()
                : pilihan ?? temuan switch
                {
                    LabMicrobiologyFinding.Normal => "Normal",
                    LabMicrobiologyFinding.Positive => "Positif",
                    LabMicrobiologyFinding.Negative => "Negatif",
                    _ => null
                };

            if (nilai != null && kualifikasi == LabResultQualifier.Preliminary)
                nilai += " (Sementara)";

            return nilai;
        }

        /// <summary>
        /// Waktu tersimpan UTC; rincian berkas dibaca petugas di rumah sakit, sehingga ditulis WIB
        /// (UTC+7, tanpa musim panas) beserta labelnya supaya tidak tertukar (`NFR-005`).
        /// </summary>
        private static string? TulisWaktu(DateTime? waktu) =>
            waktu?.AddHours(7).ToString("dd-MM-yyyy HH:mm 'WIB'", CultureInfo.InvariantCulture);

        private static string? Potong(string? teks) =>
            teks == null || teks.Length <= PanjangRingkasanMaksimal ? teks : teks[..PanjangRingkasanMaksimal];

        private static void Tambah(List<MedicalRecordDocumentSectionResponse> bagian, string label, string? nilai)
        {
            if (!string.IsNullOrWhiteSpace(nilai))
                bagian.Add(new MedicalRecordDocumentSectionResponse { Label = label, Value = nilai });
        }
    }
}
