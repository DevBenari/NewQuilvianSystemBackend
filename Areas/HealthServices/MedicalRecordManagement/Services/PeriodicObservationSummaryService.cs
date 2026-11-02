using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services.Sources;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services
{
    /// <summary>
    /// Ringkasan pencatatan berkala per hari per jenis untuk linimasa berkas (`RM-DEC-052`, `BE-22`).
    ///
    /// Satu pasien rawat inap dapat menghasilkan ratusan pengukuran gula darah, balans cairan,
    /// dan observasi harian. Menampilkannya satu baris per pengukuran membuat ringkasan pulang dan
    /// hasil lab tenggelam. Layanan ini menggabungkannya menjadi satu baris per jenis per tanggal
    /// WIB (`NFR-005`), dan rinciannya dibuka lewat <see cref="GetDayEntriesAsync"/>.
    ///
    /// Dihitung saat dibaca, tidak disimpan (`RM-DEC-038`). Hanya membaca tabel
    /// `ClinicalManagement`; tidak ada berkas di modul itu yang diubah.
    ///
    /// Didaftarkan sebagai <see cref="IMedicalRecordDocumentSource"/> untuk satu jenis,
    /// <see cref="ClinicalDocumentKind.PeriodicObservationSummary"/>, supaya ikut aturan batas waktu
    /// dan gangguan per sumber yang sama dengan modul lain.
    ///
    /// Untuk drain WSD dan surveilans luka operasi, yang diringkas adalah PENCATATANNYA (bacaan
    /// drain, entri harian surveilans), bukan pemasangan drain atau pembukaan surveilans — yang
    /// berulang setiap hari adalah pencatatannya.
    ///
    /// Teks isian disusun di memori, bukan di SQL: menggabungkan nama enum dan angka desimal di
    /// query tidak diterjemahkan sama oleh setiap penyedia basis data.
    /// </summary>
    public class PeriodicObservationSummaryService : IMedicalRecordDocumentSource
    {
        /// <summary>Selisih WIB terhadap UTC. Indonesia tidak memakai musim panas.</summary>
        private static readonly TimeSpan SelisihWib = TimeSpan.FromHours(7);

        private static readonly CultureInfo Indonesia = CultureInfo.GetCultureInfo("id-ID");

        private readonly ApplicationDbContext _dbContext;

        public PeriodicObservationSummaryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string SourceModule => "Pelayanan Klinis";

        public IReadOnlyCollection<ClinicalDocumentKind> Kinds { get; } = [ClinicalDocumentKind.PeriodicObservationSummary];

        public static string NamaJenis(PeriodicObservationKind jenis) => jenis switch
        {
            PeriodicObservationKind.DailyObservation => "Observasi Harian",
            PeriodicObservationKind.FluidBalance => "Balans Cairan",
            PeriodicObservationKind.BloodGlucose => "Gula Darah",
            PeriodicObservationKind.EquipmentUsage => "Pemakaian Alat",
            PeriodicObservationKind.WsdDrain => "Bacaan Drain WSD",
            PeriodicObservationKind.SurgicalSiteSurveillance => "Surveilans Luka Operasi",
            _ => jenis.ToString()
        };

        public async Task<MedicalRecordSourcePage> ReadAsync(
            MedicalRecordSourceQuery query,
            CancellationToken cancellationToken)
        {
            var hasil = new MedicalRecordSourcePage();

            if (!query.Kinds.Contains(ClinicalDocumentKind.PeriodicObservationSummary))
                return hasil;

            var kelompok = new List<Kelompok>();
            foreach (var jenis in Enum.GetValues<PeriodicObservationKind>())
            {
                var catatan = await CatatanAsync(jenis, query.PatientId, query.EncounterId, cancellationToken);

                kelompok.AddRange(catatan
                    .Where(x => !query.StartDate.HasValue || x.Waktu >= query.StartDate.Value)
                    .Where(x => !query.EndDate.HasValue || x.Waktu <= query.EndDate.Value)
                    .GroupBy(x => (Tanggal: TanggalWib(x.Waktu), x.EncounterId))
                    .Select(g => new Kelompok(jenis, g.Key.Tanggal, g.Key.EncounterId, g.Count(), g.Max(x => x.Waktu))));
            }

            hasil.Totals[ClinicalDocumentKind.PeriodicObservationSummary] = kelompok.Count;

            if (query.Limit <= 0)
                return hasil;

            hasil.Items = (query.NewestFirst
                    ? kelompok.OrderByDescending(x => x.Terakhir)
                    : kelompok.OrderBy(x => x.Terakhir))
                .Take(query.Limit)
                .Select(x => new MedicalRecordFinalDocumentDescriptor
                {
                    Kind = ClinicalDocumentKind.PeriodicObservationSummary,
                    DocumentId = IdRingkasan(query.PatientId, x.Jenis, x.Tanggal, x.EncounterId),
                    EncounterId = x.EncounterId,
                    FinalizedAt = x.Terakhir,
                    Title = $"{NamaJenis(x.Jenis)} — {x.Jumlah} pencatatan",
                    Summary = x.Tanggal.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                    PeriodicSummary = new MedicalRecordPeriodicSummaryResponse
                    {
                        Kind = x.Jenis,
                        KindName = NamaJenis(x.Jenis),
                        Date = x.Tanggal.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        Count = x.Jumlah
                    }
                })
                .ToList();

            return hasil;
        }

        /// <summary>
        /// Ringkasan berkala tidak punya rincian tunggal; rinciannya per hari lewat
        /// <see cref="GetDayEntriesAsync"/>.
        /// </summary>
        public Task<MedicalRecordSourceDocumentDetail?> ReadDetailAsync(
            ClinicalDocumentKind kind,
            Guid documentId,
            Guid patientId,
            CancellationToken cancellationToken) => Task.FromResult<MedicalRecordSourceDocumentDetail?>(null);

        /// <summary>
        /// Rincian satu jenis pada satu tanggal WIB — isi baris ringkasan. Hanya pencatatan yang
        /// tidak dibatalkan, urut waktu.
        /// </summary>
        public async Task<List<PeriodicObservationEntryResponse>> GetDayEntriesAsync(
            Guid patientId,
            PeriodicObservationKind jenis,
            DateOnly tanggalWib,
            CancellationToken cancellationToken = default)
        {
            var catatan = await CatatanAsync(jenis, patientId, null, cancellationToken);

            return catatan
                .Where(x => TanggalWib(x.Waktu) == tanggalWib)
                .OrderBy(x => x.Waktu)
                .Select(x => new PeriodicObservationEntryResponse
                {
                    EntryId = x.Id,
                    EncounterId = x.EncounterId,
                    OccurredAt = x.Waktu,
                    Value = x.Teks,
                    IsCorrected = x.Revisi > 1
                })
                .ToList();
        }

        // =====================================================================

        private sealed record Kelompok(
            PeriodicObservationKind Jenis, DateOnly Tanggal, Guid? EncounterId, int Jumlah, DateTime Terakhir);

        /// <summary>Satu pencatatan aktif beserta teks isiannya yang siap dibaca.</summary>
        private sealed record Catatan(Guid Id, Guid? EncounterId, DateTime Waktu, int Revisi, string Teks);

        /// <summary>Seluruh pencatatan aktif milik pasien untuk satu jenis.</summary>
        private async Task<List<Catatan>> CatatanAsync(
            PeriodicObservationKind jenis,
            Guid patientId,
            Guid? encounterId,
            CancellationToken cancellationToken)
        {
            switch (jenis)
            {
                case PeriodicObservationKind.DailyObservation:
                    return (await _dbContext.CliDailyObservations.AsNoTracking()
                            .Where(x => x.PatientId == patientId && !x.IsDelete && !x.IsCancel
                                        && x.ObservationStatus == ClinicalMeasurementStatus.Active
                                        && (!encounterId.HasValue || x.EncounterId == encounterId.Value))
                            .Select(x => new { x.Id, x.EncounterId, x.ObservedAt, x.RevisionNumber, x.DietIntakePercent, x.MobilizationLevel, x.Note })
                            .ToListAsync(cancellationToken))
                        .Select(x => new Catatan(x.Id, x.EncounterId, x.ObservedAt, x.RevisionNumber,
                            Gabung(
                                x.DietIntakePercent.HasValue ? $"Asupan diet {x.DietIntakePercent}%" : null,
                                $"mobilisasi {x.MobilizationLevel}",
                                x.Note)))
                        .ToList();

                case PeriodicObservationKind.FluidBalance:
                    return (await _dbContext.CliFluidBalanceEntries.AsNoTracking()
                            .Where(x => x.PatientId == patientId && !x.IsDelete && !x.IsCancel
                                        && x.EntryStatus == ClinicalMeasurementStatus.Active
                                        && (!encounterId.HasValue || x.EncounterId == encounterId.Value))
                            .Select(x => new { x.Id, x.EncounterId, x.EntryDateTime, x.RevisionNumber, x.Direction, x.SourceCategory, x.SourceDetail, x.VolumeMl })
                            .ToListAsync(cancellationToken))
                        .Select(x => new Catatan(x.Id, x.EncounterId, x.EntryDateTime, x.RevisionNumber,
                            $"{(x.Direction == FluidDirection.Intake ? "Masuk" : "Keluar")} {x.SourceDetail ?? x.SourceCategory.ToString()} {Angka(x.VolumeMl)} ml"))
                        .ToList();

                case PeriodicObservationKind.BloodGlucose:
                    return (await _dbContext.CliBloodGlucoseReadings.AsNoTracking()
                            .Where(x => x.PatientId == patientId && !x.IsDelete && !x.IsCancel
                                        && x.ReadingStatus == ClinicalMeasurementStatus.Active
                                        && (!encounterId.HasValue || x.EncounterId == encounterId.Value))
                            .Select(x => new { x.Id, x.EncounterId, x.MeasuredAt, x.RevisionNumber, x.GlucoseValue, x.GlucoseUnit, x.Method })
                            .ToListAsync(cancellationToken))
                        .Select(x => new Catatan(x.Id, x.EncounterId, x.MeasuredAt, x.RevisionNumber,
                            $"{Angka(x.GlucoseValue)} {(x.GlucoseUnit == BloodGlucoseUnit.MmolPerL ? "mmol/L" : "mg/dL")} ({NamaMetode(x.Method)})"))
                        .ToList();

                case PeriodicObservationKind.EquipmentUsage:
                    return (await (
                                from x in _dbContext.CliEquipmentUsages.AsNoTracking()
                                join e in _dbContext.MstMedicalEquipments.AsNoTracking() on x.MedicalEquipmentId equals e.Id into alat
                                from e in alat.DefaultIfEmpty()
                                where x.PatientId == patientId && !x.IsDelete && !x.IsCancel
                                      && x.Status != CliEquipmentUsageStatus.Cancelled
                                      && (!encounterId.HasValue || x.EncounterId == encounterId.Value)
                                select new { x.Id, x.EncounterId, x.StartedAt, x.EndedAt, x.RevisionNumber, Nama = e != null ? e.EquipmentName : null })
                            .ToListAsync(cancellationToken))
                        .Select(x => new Catatan(x.Id, x.EncounterId, x.StartedAt, x.RevisionNumber,
                            $"{x.Nama ?? "Alat"} ({(x.EndedAt.HasValue ? "selesai" : "masih terpasang")})"))
                        .ToList();

                case PeriodicObservationKind.WsdDrain:
                    return (await (
                                from r in _dbContext.CliWsdReadings.AsNoTracking()
                                join d in _dbContext.CliWsdDrains.AsNoTracking() on r.WsdDrainId equals d.Id
                                where d.PatientId == patientId && !r.IsDelete && !r.IsCancel
                                      && r.Status == ClinicalMeasurementStatus.Active
                                      && (!encounterId.HasValue || d.EncounterId == encounterId.Value)
                                select new { r.Id, d.EncounterId, r.PeriodEndAt, r.RevisionNumber, d.DrainLabel, r.IncreaseMl, r.CurrentResidualMl })
                            .ToListAsync(cancellationToken))
                        .Select(x => new Catatan(x.Id, x.EncounterId, x.PeriodEndAt, x.RevisionNumber,
                            $"{x.DrainLabel}: kenaikan {Angka(x.IncreaseMl)} ml, residu {Angka(x.CurrentResidualMl)} ml"))
                        .ToList();

                case PeriodicObservationKind.SurgicalSiteSurveillance:
                    // Entri hanya menyimpan tanggal, sehingga waktunya dianggap awal hari WIB itu.
                    return (await (
                                from e in _dbContext.CliSurgicalSiteSurveillanceEntries.AsNoTracking()
                                join s in _dbContext.CliSurgicalSiteSurveillances.AsNoTracking() on e.SurveillanceId equals s.Id
                                where s.PatientId == patientId && !e.IsDelete && !e.IsCancel && !s.IsDelete
                                      && s.Status != CliSurveillanceStatus.Cancelled
                                      && (!encounterId.HasValue || s.EncounterId == encounterId.Value)
                                select new { e.Id, s.EncounterId, e.EntryDate, e.DayNumber, e.RevisionNumber, e.TemperatureMaxCelsiusSnapshot })
                            .ToListAsync(cancellationToken))
                        .Select(x => new Catatan(x.Id, x.EncounterId,
                            x.EntryDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) - SelisihWib,
                            x.RevisionNumber,
                            Gabung(
                                $"Hari ke-{x.DayNumber}",
                                x.TemperatureMaxCelsiusSnapshot.HasValue
                                    ? $"suhu tertinggi {x.TemperatureMaxCelsiusSnapshot.Value.ToString("0.#", Indonesia)} °C"
                                    : null)))
                        .ToList();

                default:
                    return [];
            }
        }

        private static string NamaMetode(BloodGlucoseMethod metode) => metode switch
        {
            BloodGlucoseMethod.WardGlucometer => "glukometer ruangan",
            _ => metode.ToString()
        };

        private static DateOnly TanggalWib(DateTime utc) => DateOnly.FromDateTime(utc + SelisihWib);

        private static string Angka(decimal nilai) => nilai.ToString("0.##", Indonesia);

        private static string Gabung(params string?[] bagian) =>
            string.Join("; ", bagian.Where(x => !string.IsNullOrWhiteSpace(x)));

        /// <summary>Id tetap untuk satu baris ringkasan, supaya layar dapat menandai baris yang sama.</summary>
        private static Guid IdRingkasan(Guid patientId, PeriodicObservationKind jenis, DateOnly tanggal, Guid? encounterId)
        {
            var kunci = $"{patientId:N}|{(int)jenis}|{tanggal:yyyyMMdd}|{encounterId:N}";
            return new Guid(MD5.HashData(Encoding.UTF8.GetBytes(kunci)));
        }
    }
}
