using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services.Sources;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    /// <summary>
    /// Pembaca resep, pemberian obat, dan rekonsiliasi obat Farmasi untuk berkas rekam medis
    /// (`INT-RM-10i`, `BE-35`).
    ///
    /// Ditulis di modul Farmasi dengan persetujuan pemiliknya (`RM-DEC-075`). Hanya membaca;
    /// pembacaan ini tidak mengubah stok, dispensing, peresepan, administrasi obat, maupun tagihan.
    ///
    /// FINAL MENURUT FARMASI (A4.10):
    /// <list type="bullet">
    /// <item>Resep — berstatus <c>Submitted</c>, waktu <c>SubmittedAt</c>. Resep <c>Draft</c> dan
    /// yang dibatalkan tidak tampil.</item>
    /// <item>Pemberian obat — berstatus <c>Administered</c>, waktu <c>AdministeredAt</c>. Pernah
    /// diralat bila punya baris <c>PhmMedicationAdministrationRevision</c>.</item>
    /// <item>Rekonsiliasi obat — satu baris per obat bawaan yang sudah diputuskan dokter. Yang
    /// tampil keputusan terakhir (<c>SequenceNumber</c> tertinggi), waktu <c>DecidedAt</c>-nya.
    /// Keputusan yang menggantikan keputusan sebelumnya bertanda pernah diralat.</item>
    /// </list>
    ///
    /// Farmasi tidak menjawab kriteria kelengkapan (A4.11), sehingga pembaca ini bukan
    /// <see cref="IMedicalRecordCompletenessProbe"/>.
    /// </summary>
    public class PhmMedicalRecordDocumentSource : IMedicalRecordDocumentSource
    {
        private const int PanjangRingkasanMaksimal = 300;
        private const int ObatDalamRingkasan = 3;

        private readonly ApplicationDbContext _dbContext;

        public PhmMedicalRecordDocumentSource(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string SourceModule => "Farmasi";

        public IReadOnlyCollection<ClinicalDocumentKind> Kinds { get; } =
        [
            ClinicalDocumentKind.Prescription,
            ClinicalDocumentKind.MedicationAdministration,
            ClinicalDocumentKind.MedicationReconciliation
        ];

        public async Task<MedicalRecordSourcePage> ReadAsync(
            MedicalRecordSourceQuery query,
            CancellationToken cancellationToken)
        {
            var hasil = new MedicalRecordSourcePage();
            var dokumen = new List<MedicalRecordFinalDocumentDescriptor>();

            if (query.Kinds.Contains(ClinicalDocumentKind.Prescription))
            {
                var resep = ResepFinal(query.PatientId, query.EncounterId, query.StartDate, query.EndDate);
                hasil.Totals[ClinicalDocumentKind.Prescription] = await resep.CountAsync(cancellationToken);

                if (query.Limit > 0)
                {
                    var baris = await (query.NewestFirst
                            ? resep.OrderByDescending(x => x.SubmittedAt)
                            : resep.OrderBy(x => x.SubmittedAt))
                        .Take(query.Limit)
                        .Select(x => new
                        {
                            x.Id,
                            x.EncounterId,
                            x.PrescriptionNumber,
                            SubmittedAt = x.SubmittedAt!.Value,
                            x.PrescriptionDateTime,
                            x.SubmittedByUserId,
                            Obat = _dbContext.PhmPrescriptionItems
                                .Where(i => i.PrescriptionId == x.Id && !i.IsDelete)
                                .OrderBy(i => i.CreateDateTime)
                                .Select(i => i.DrugNameSnapshot)
                                .Take(ObatDalamRingkasan)
                                .ToList(),
                            Racikan = _dbContext.PhmPrescriptionCompounds
                                .Where(c => c.PrescriptionId == x.Id && !c.IsDelete)
                                .OrderBy(c => c.CreateDateTime)
                                .Select(c => c.CompoundName)
                                .Take(ObatDalamRingkasan)
                                .ToList(),
                            JumlahButir = _dbContext.PhmPrescriptionItems.Count(i => i.PrescriptionId == x.Id && !i.IsDelete)
                                          + _dbContext.PhmPrescriptionCompounds.Count(c => c.PrescriptionId == x.Id && !c.IsDelete)
                        })
                        .ToListAsync(cancellationToken);

                    dokumen.AddRange(baris.Select(x => new MedicalRecordFinalDocumentDescriptor
                    {
                        Kind = ClinicalDocumentKind.Prescription,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        FinalizedAt = x.SubmittedAt,
                        OccurredAt = x.PrescriptionDateTime,
                        DocumentNumber = x.PrescriptionNumber,
                        Title = "Resep",
                        Summary = Potong(RingkasanResep(x.JumlahButir, x.Obat.Concat(x.Racikan).ToList())),
                        AuthorUserId = x.SubmittedByUserId
                    }));
                }
            }

            if (query.Kinds.Contains(ClinicalDocumentKind.MedicationAdministration))
            {
                var pemberian = PemberianFinal(query.PatientId, query.EncounterId, query.StartDate, query.EndDate);
                hasil.Totals[ClinicalDocumentKind.MedicationAdministration] = await pemberian.CountAsync(cancellationToken);

                if (query.Limit > 0)
                {
                    var baris = await (query.NewestFirst
                            ? pemberian.OrderByDescending(x => x.AdministeredAt)
                            : pemberian.OrderBy(x => x.AdministeredAt))
                        .Take(query.Limit)
                        .Select(x => new
                        {
                            x.Id,
                            x.EncounterId,
                            x.AdministrationNumber,
                            AdministeredAt = x.AdministeredAt!.Value,
                            x.ActualDose,
                            x.ActualDoseUnitSnapshot,
                            x.ActualRouteSnapshot,
                            x.RecordedByUserId,
                            Obat = _dbContext.PhmPrescriptionItems.Where(i => i.Id == x.PrescriptionItemId).Select(i => i.DrugNameSnapshot).FirstOrDefault()
                                   ?? _dbContext.MstDrugs.Where(d => d.Id == x.DrugId).Select(d => d.DrugName).FirstOrDefault(),
                            PernahDiralat = _dbContext.PhmMedicationAdministrationRevisions.Any(r => r.AdministrationId == x.Id && !r.IsDelete)
                        })
                        .ToListAsync(cancellationToken);

                    dokumen.AddRange(baris.Select(x => new MedicalRecordFinalDocumentDescriptor
                    {
                        Kind = ClinicalDocumentKind.MedicationAdministration,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        FinalizedAt = x.AdministeredAt,
                        DocumentNumber = x.AdministrationNumber,
                        Title = "Pemberian Obat",
                        Summary = Potong(Gabung(x.Obat, Dosis(x.ActualDose, x.ActualDoseUnitSnapshot), x.ActualRouteSnapshot)),
                        IsCorrected = x.PernahDiralat,
                        AuthorUserId = x.RecordedByUserId
                    }));
                }
            }

            if (query.Kinds.Contains(ClinicalDocumentKind.MedicationReconciliation))
            {
                var rekonsiliasi = KeputusanTerakhir(query.PatientId, query.EncounterId, query.StartDate, query.EndDate);
                hasil.Totals[ClinicalDocumentKind.MedicationReconciliation] = await rekonsiliasi.CountAsync(cancellationToken);

                if (query.Limit > 0)
                {
                    var baris = await (query.NewestFirst
                            ? rekonsiliasi.OrderByDescending(x => x.DecidedAt)
                            : rekonsiliasi.OrderBy(x => x.DecidedAt))
                        .Take(query.Limit)
                        .Select(x => new
                        {
                            x.ReconciliationItemId,
                            x.ReconciliationItem!.EncounterId,
                            x.ReconciliationItem.DrugNameSnapshot,
                            x.DecidedAt,
                            x.DecisionType,
                            x.SequenceNumber,
                            x.DecidedByUserId
                        })
                        .ToListAsync(cancellationToken);

                    dokumen.AddRange(baris.Select(x => new MedicalRecordFinalDocumentDescriptor
                    {
                        Kind = ClinicalDocumentKind.MedicationReconciliation,
                        DocumentId = x.ReconciliationItemId,
                        EncounterId = x.EncounterId,
                        FinalizedAt = x.DecidedAt,
                        Title = "Rekonsiliasi Obat",
                        Summary = Potong(Gabung(x.DrugNameSnapshot, NamaKeputusan(x.DecisionType))),
                        IsCorrected = x.SequenceNumber > 1,
                        AuthorUserId = x.DecidedByUserId
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
                ClinicalDocumentKind.Prescription => await RincianResepAsync(documentId, patientId, cancellationToken),
                ClinicalDocumentKind.MedicationAdministration => await RincianPemberianAsync(documentId, patientId, cancellationToken),
                ClinicalDocumentKind.MedicationReconciliation => await RincianRekonsiliasiAsync(documentId, patientId, cancellationToken),
                _ => null
            };
        }

        // =====================================================================
        // Rincian
        // =====================================================================

        private async Task<MedicalRecordSourceDocumentDetail?> RincianResepAsync(Guid documentId, Guid patientId, CancellationToken cancellationToken)
        {
            var x = await ResepFinal(patientId, null, null, null)
                .Where(r => r.Id == documentId)
                .Select(r => new
                {
                    r.EncounterId,
                    r.PrescriptionNumber,
                    SubmittedAt = r.SubmittedAt!.Value,
                    r.PrescriptionDateTime,
                    r.DispensedAt,
                    r.ClinicalNote,
                    r.DoctorInstruction,
                    // Subquery, bukan navigation: master dokter yang hilang tidak boleh menyembunyikan resep.
                    Dokter = _dbContext.MstDoctors.Where(d => d.Id == r.DoctorId).Select(d => d.FullName).FirstOrDefault(),
                    Butir = _dbContext.PhmPrescriptionItems
                        .Where(i => i.PrescriptionId == r.Id && !i.IsDelete)
                        .OrderBy(i => i.CreateDateTime)
                        .Select(i => new
                        {
                            i.DrugNameSnapshot,
                            i.StrengthSnapshot,
                            i.Dose,
                            i.DoseUnitSymbolSnapshot,
                            i.DoseUnitNameSnapshot,
                            i.FrequencyText,
                            i.Signa,
                            i.RouteSnapshot,
                            i.Quantity,
                            i.DispenseUnitNameSnapshot
                        })
                        .ToList(),
                    Racikan = _dbContext.PhmPrescriptionCompounds
                        .Where(c => c.PrescriptionId == r.Id && !c.IsDelete)
                        .OrderBy(c => c.CreateDateTime)
                        .Select(c => new { c.CompoundName, c.CompoundForm, c.FrequencyText, c.TotalPackage, c.PackageUnitNameSnapshot })
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (x == null)
                return null;

            var bagian = new List<MedicalRecordDocumentSectionResponse>();
            Tambah(bagian, "Dokter", x.Dokter);
            Tambah(bagian, "Tanggal resep", TulisWaktu(x.PrescriptionDateTime));
            foreach (var i in x.Butir)
            {
                Tambah(bagian, "Obat", Gabung(
                    string.Join(" ", new[] { i.DrugNameSnapshot, i.StrengthSnapshot }.Where(t => !string.IsNullOrWhiteSpace(t))),
                    Dosis(i.Dose, i.DoseUnitSymbolSnapshot ?? i.DoseUnitNameSnapshot),
                    i.Signa ?? i.FrequencyText,
                    i.RouteSnapshot,
                    $"Jumlah {TulisAngka(i.Quantity)}{(string.IsNullOrWhiteSpace(i.DispenseUnitNameSnapshot) ? string.Empty : " " + i.DispenseUnitNameSnapshot)}"));
            }
            foreach (var c in x.Racikan)
            {
                Tambah(bagian, "Racikan", Gabung(
                    c.CompoundName,
                    c.CompoundForm,
                    c.FrequencyText,
                    $"{TulisAngka(c.TotalPackage)}{(string.IsNullOrWhiteSpace(c.PackageUnitNameSnapshot) ? string.Empty : " " + c.PackageUnitNameSnapshot)}"));
            }
            Tambah(bagian, "Instruksi dokter", x.DoctorInstruction);
            Tambah(bagian, "Catatan klinis", x.ClinicalNote);
            Tambah(bagian, "Diajukan", TulisWaktu(x.SubmittedAt));
            Tambah(bagian, "Diserahkan", TulisWaktu(x.DispensedAt));

            return new MedicalRecordSourceDocumentDetail
            {
                EncounterId = x.EncounterId,
                DocumentNumber = x.PrescriptionNumber,
                Title = "Resep",
                FinalizedAt = x.SubmittedAt,
                OccurredAt = x.PrescriptionDateTime,
                Sections = bagian
            };
        }

        private async Task<MedicalRecordSourceDocumentDetail?> RincianPemberianAsync(Guid documentId, Guid patientId, CancellationToken cancellationToken)
        {
            var x = await PemberianFinal(patientId, null, null, null)
                .Where(a => a.Id == documentId)
                .Select(a => new
                {
                    a.EncounterId,
                    a.AdministrationNumber,
                    AdministeredAt = a.AdministeredAt!.Value,
                    a.ScheduledAt,
                    a.PlannedDose,
                    a.PlannedDoseUnitSnapshot,
                    a.ActualDose,
                    a.ActualDoseUnitSnapshot,
                    a.ActualRouteSnapshot,
                    a.DeviationNote,
                    a.IsHighAlertSnapshot,
                    a.DoubleCheckedAt,
                    a.PrnIndication,
                    a.PrnEvaluationNote,
                    a.RecordedAt,
                    Obat = _dbContext.PhmPrescriptionItems.Where(i => i.Id == a.PrescriptionItemId).Select(i => i.DrugNameSnapshot).FirstOrDefault()
                                   ?? _dbContext.MstDrugs.Where(d => d.Id == a.DrugId).Select(d => d.DrugName).FirstOrDefault(),
                    Ralat = _dbContext.PhmMedicationAdministrationRevisions
                        .Where(r => r.AdministrationId == a.Id && !r.IsDelete)
                        .OrderBy(r => r.RevisionNumber)
                        .Select(r => new { r.CorrectedAt, r.CorrectionReason })
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (x == null)
                return null;

            var bagian = new List<MedicalRecordDocumentSectionResponse>();
            Tambah(bagian, "Obat", x.Obat);
            Tambah(bagian, "Dosis direncanakan", Dosis(x.PlannedDose, x.PlannedDoseUnitSnapshot));
            Tambah(bagian, "Dosis diberikan", Dosis(x.ActualDose, x.ActualDoseUnitSnapshot));
            Tambah(bagian, "Rute", x.ActualRouteSnapshot);
            Tambah(bagian, "Jadwal", TulisWaktu(x.ScheduledAt));
            Tambah(bagian, "Diberikan", TulisWaktu(x.AdministeredAt));
            Tambah(bagian, "Obat kewaspadaan tinggi", x.IsHighAlertSnapshot ? Gabung("Ya", x.DoubleCheckedAt.HasValue ? $"dicek ganda {TulisWaktu(x.DoubleCheckedAt)}" : null) : null);
            Tambah(bagian, "Indikasi bila perlu", x.PrnIndication);
            Tambah(bagian, "Evaluasi bila perlu", x.PrnEvaluationNote);
            Tambah(bagian, "Penyimpangan", x.DeviationNote);
            Tambah(bagian, "Dicatat", TulisWaktu(x.RecordedAt));
            foreach (var r in x.Ralat)
                Tambah(bagian, "Ralat", Gabung(TulisWaktu(r.CorrectedAt), r.CorrectionReason));

            return new MedicalRecordSourceDocumentDetail
            {
                EncounterId = x.EncounterId,
                DocumentNumber = x.AdministrationNumber,
                Title = "Pemberian Obat",
                FinalizedAt = x.AdministeredAt,
                IsCorrected = x.Ralat.Count > 0,
                Sections = bagian
            };
        }

        private async Task<MedicalRecordSourceDocumentDetail?> RincianRekonsiliasiAsync(Guid documentId, Guid patientId, CancellationToken cancellationToken)
        {
            var x = await KeputusanTerakhir(patientId, null, null, null)
                .Where(d => d.ReconciliationItemId == documentId)
                .Select(d => new
                {
                    d.ReconciliationItem!.EncounterId,
                    d.ReconciliationItem.DrugNameSnapshot,
                    d.ReconciliationItem.Dose,
                    d.ReconciliationItem.DrugFormSnapshot,
                    d.ReconciliationItem.FrequencyText,
                    d.ReconciliationItem.Route,
                    d.ReconciliationItem.RecordedAt,
                    d.DecidedAt,
                    d.DecisionType,
                    d.DecisionNote,
                    d.SequenceNumber,
                    Dokter = _dbContext.MstDoctors.Where(m => m.Id == d.DecidedByDoctorId).Select(m => m.FullName).FirstOrDefault()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (x == null)
                return null;

            var bagian = new List<MedicalRecordDocumentSectionResponse>();
            Tambah(bagian, "Obat bawaan", Gabung(x.DrugNameSnapshot, x.DrugFormSnapshot));
            Tambah(bagian, "Dosis", x.Dose.HasValue ? TulisAngka(x.Dose.Value) : null);
            Tambah(bagian, "Frekuensi", x.FrequencyText);
            Tambah(bagian, "Rute", NamaRute(x.Route));
            Tambah(bagian, "Dicatat", TulisWaktu(x.RecordedAt));
            Tambah(bagian, "Keputusan", NamaKeputusan(x.DecisionType));
            Tambah(bagian, "Catatan keputusan", x.DecisionNote);
            Tambah(bagian, "Diputuskan", Gabung(TulisWaktu(x.DecidedAt), x.Dokter));
            Tambah(bagian, "Riwayat keputusan", x.SequenceNumber > 1 ? $"Keputusan ke-{x.SequenceNumber}" : null);

            return new MedicalRecordSourceDocumentDetail
            {
                EncounterId = x.EncounterId,
                Title = "Rekonsiliasi Obat",
                FinalizedAt = x.DecidedAt,
                IsCorrected = x.SequenceNumber > 1,
                Sections = bagian
            };
        }

        // =====================================================================
        // Satu-satunya tempat aturan "final" Farmasi untuk rekam medis
        // =====================================================================

        private IQueryable<PhmPrescription> ResepFinal(Guid patientId, Guid? encounterId, DateTime? dari, DateTime? sampai)
        {
            var query = _dbContext.PhmPrescriptions
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.PatientId == patientId
                            && x.PrescriptionStatus == PrescriptionStatus.Submitted
                            && x.SubmittedAt != null);

            if (encounterId.HasValue)
                query = query.Where(x => x.EncounterId == encounterId.Value);
            if (dari.HasValue)
                query = query.Where(x => x.SubmittedAt >= dari.Value);
            if (sampai.HasValue)
                query = query.Where(x => x.SubmittedAt <= sampai.Value);

            return query;
        }

        private IQueryable<PhmMedicationAdministration> PemberianFinal(Guid patientId, Guid? encounterId, DateTime? dari, DateTime? sampai)
        {
            var query = _dbContext.PhmMedicationAdministrations
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.PatientId == patientId
                            && x.DoseStatus == MedicationDoseStatus.Administered
                            && x.AdministeredAt != null);

            if (encounterId.HasValue)
                query = query.Where(x => x.EncounterId == encounterId.Value);
            if (dari.HasValue)
                query = query.Where(x => x.AdministeredAt >= dari.Value);
            if (sampai.HasValue)
                query = query.Where(x => x.AdministeredAt <= sampai.Value);

            return query;
        }

        /// <summary>
        /// Keputusan terakhir per obat bawaan; obat yang belum diputuskan atau salah catat
        /// (dinonaktifkan perawat) tidak tampil.
        /// </summary>
        private IQueryable<PhmMedicationReconciliationDecision> KeputusanTerakhir(Guid patientId, Guid? encounterId, DateTime? dari, DateTime? sampai)
        {
            var query = _dbContext.PhmMedicationReconciliationDecisions
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.DecisionType != ReconciliationDecisionType.Pending
                            && x.ReconciliationItem != null
                            && !x.ReconciliationItem.IsDelete
                            && x.ReconciliationItem.IsActive
                            && x.ReconciliationItem.PatientId == patientId
                            && !_dbContext.PhmMedicationReconciliationDecisions.Any(o => o.ReconciliationItemId == x.ReconciliationItemId
                                                                                        && !o.IsDelete
                                                                                        && o.SequenceNumber > x.SequenceNumber));

            if (encounterId.HasValue)
                query = query.Where(x => x.ReconciliationItem!.EncounterId == encounterId.Value);
            if (dari.HasValue)
                query = query.Where(x => x.DecidedAt >= dari.Value);
            if (sampai.HasValue)
                query = query.Where(x => x.DecidedAt <= sampai.Value);

            return query;
        }

        // =====================================================================
        // Penulisan
        // =====================================================================

        private static string RingkasanResep(int jumlahButir, IReadOnlyList<string> nama)
        {
            var daftar = string.Join(", ", nama.Where(x => !string.IsNullOrWhiteSpace(x)).Take(ObatDalamRingkasan));
            var sisa = jumlahButir - Math.Min(nama.Count, ObatDalamRingkasan);
            return $"{jumlahButir} butir" + (daftar.Length > 0 ? $": {daftar}" : string.Empty) + (sisa > 0 ? $", dan {sisa} lainnya" : string.Empty);
        }

        private static string NamaKeputusan(ReconciliationDecisionType keputusan) => keputusan switch
        {
            ReconciliationDecisionType.ContinueSame => "Dilanjutkan tanpa perubahan",
            ReconciliationDecisionType.ContinueModified => "Dilanjutkan dengan perubahan",
            ReconciliationDecisionType.Stopped => "Dihentikan",
            _ => "Belum diputuskan"
        };

        private static string NamaRute(HomeMedicationRoute rute) => rute switch
        {
            HomeMedicationRoute.Oral => "Oral",
            HomeMedicationRoute.Injection => "Injeksi",
            HomeMedicationRoute.Topical => "Topikal",
            HomeMedicationRoute.Inhalation => "Inhalasi",
            HomeMedicationRoute.Rectal => "Rektal",
            HomeMedicationRoute.Infusion => "Infus",
            _ => "Lainnya"
        };

        private static string? Dosis(decimal? jumlah, string? satuan) =>
            jumlah.HasValue ? string.Join(" ", new[] { TulisAngka(jumlah.Value), satuan }.Where(t => !string.IsNullOrWhiteSpace(t))) : null;

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
}
