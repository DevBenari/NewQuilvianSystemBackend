using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Pengawasan Harian rawat inap: cairan masuk dan keluar, GDS bangsal, observasi harian, jam shift, balance per
    /// shift dan 24 jam, dan ringkasan satu hari — <c>BE-RWI-119</c>, <c>BE-RWI-120</c>, <c>BE-RWI-122</c>,
    /// arsitektur keperawatan 0.4 bagian 11.5.5–11.5.7, state matrix 0.5.0 bagian 5.4.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Entri tidak pernah dihapus.</b> Koreksi menyimpan nilai lama pada tabel revisi dan menaikkan
    /// <c>RevisionNumber</c>; pembatalan menyimpan alasan. Balance hanya menjumlah entri <c>Active</c>.
    /// </para>
    /// <para>
    /// <b>GDS bangsal satu tempat, satuan wajib tanpa bawaan</b> (<c>RWI-DEC-150</c> b, gate <c>G-25</c>). Dosis insulin
    /// dihitung dari angka ini; mg/dL yang terbaca sebagai mmol/L adalah kesalahan berlipat delapan belas.
    /// </para>
    /// <para>
    /// <b>Jam shift tidak pernah menentukan kewenangan</b> (<c>AC-KEP-093</c>, <c>RWI-DEC-100</c>). Shift hanya membagi
    /// jumlah cairan; penjaga tulis tetap penempatan unit dari <see cref="NursingEpisodeWriteGuard"/>.
    /// </para>
    /// <para>
    /// <b>Arah ketergantungan dengan MAR</b> (<c>INT-KEP-10</c>): service ini membaca dosis <c>PhmMedicationAdministration</c>
    /// langsung sebagai data baca, dan <c>MedicationAdministrationService</c> hanya memanggil satu metode penanda
    /// <see cref="FlagLinkedFluidEntryAsync"/>. Tidak ada injeksi balik, sehingga tidak ada siklus dependency.
    /// </para>
    /// </remarks>
    public partial class DailyMonitoringService
    {
        private const string LogCategory = "HealthServices.Clinical.DailyMonitoring";

        public const int FutureToleranceMinutes = 5;
        public const int MaxRangeDays = 31;

        private static readonly TimeSpan RentangBawaan = TimeSpan.FromHours(24);
        private static readonly string[] FormatJam = { "HH:mm", "H:mm", "HH:mm:ss" };

        private readonly ApplicationDbContext _dbContext;
        private readonly NursingEpisodeWriteGuard _writeGuard;
        private readonly InpatientVitalSignService _vitalSignService;
        private readonly LoggerService _loggerService;

        public DailyMonitoringService(
            ApplicationDbContext dbContext,
            NursingEpisodeWriteGuard writeGuard,
            InpatientVitalSignService vitalSignService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _writeGuard = writeGuard;
            _vitalSignService = vitalSignService;
            _loggerService = loggerService;
        }

        // =====================================================================
        // BE-RWI-120 — balance cairan per shift dan 24 jam
        // =====================================================================

        /// <summary>
        /// Balance satu hari. Bila unit (atau bawaan) punya shift, hari dimulai pada jam mulai shift paling awal —
        /// Pagi 07.00 → 07.00 sampai 07.00 esok; tanpa shift, 00.00–24.00 dan tidak ada shift buatan.
        /// </summary>
        public async Task<NursingResult<FluidTotalsResponse>> GetFluidTotalsAsync(
            Guid episodeId,
            DateOnly? date,
            CancellationToken cancellationToken = default)
        {
            var episode = await _dbContext.Set<InpEpisode>().AsNoTracking()
                .Where(x => x.Id == episodeId && !x.IsDelete)
                .Select(x => new { x.Id, x.ServiceUnitId })
                .FirstOrDefaultAsync(cancellationToken);

            if (episode == null)
                return NursingResult<FluidTotalsResponse>.Fail(StatusCodes.Status404NotFound, "Perawatan rawat inap tidak ditemukan.");

            var shifts = await ResolveShiftsAsync(episode.ServiceUnitId, cancellationToken);
            var jendela = BuildDayWindow(date, shifts, DateTime.UtcNow);

            var entri = await _dbContext.Set<CliFluidBalanceEntry>().AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete &&
                            x.EntryStatus == ClinicalMeasurementStatus.Active &&
                            x.EntryDateTime >= jendela.StartUtc && x.EntryDateTime < jendela.EndUtc)
                .Select(x => new { x.Direction, x.VolumeMl, x.EntryDateTime })
                .ToListAsync(cancellationToken);

            FluidTotalLine Hitung(string? kode, string? nama, DateTime mulai, DateTime akhir)
            {
                var dalam = entri.Where(x => x.EntryDateTime >= mulai && x.EntryDateTime < akhir).ToList();
                var masuk = dalam.Where(x => x.Direction == FluidDirection.Intake).Sum(x => x.VolumeMl);
                var keluar = dalam.Where(x => x.Direction == FluidDirection.Output).Sum(x => x.VolumeMl);

                return new FluidTotalLine
                {
                    ShiftCode = kode,
                    ShiftName = nama,
                    StartUtc = mulai,
                    EndUtc = akhir,
                    IntakeMl = masuk,
                    OutputMl = keluar,
                    BalanceMl = masuk - keluar
                };
            }

            var response = new FluidTotalsResponse
            {
                EpisodeId = episodeId,
                Date = jendela.Date,
                WindowStartUtc = jendela.StartUtc,
                WindowEndUtc = jendela.EndUtc,
                ShiftConfigurationMissing = shifts.Count == 0,
                Day = Hitung(null, "24 jam", jendela.StartUtc, jendela.EndUtc)
            };

            foreach (var s in jendela.Shifts)
                response.Shifts.Add(Hitung(s.Code, s.Name, s.StartUtc, s.EndUtc));

            return NursingResult<FluidTotalsResponse>.Ok(response, "Balance cairan berhasil dihitung.");
        }

        // =====================================================================
        // Ringkasan Pengawasan Harian — api-contract 7.5
        // =====================================================================

        public async Task<NursingResult<DailyMonitoringSummaryResponse>> GetSummaryAsync(
            Guid episodeId,
            DateOnly? date,
            CancellationToken cancellationToken = default)
        {
            var totals = await GetFluidTotalsAsync(episodeId, date, cancellationToken);

            if (!totals.IsSuccess)
                return totals.Cast<DailyMonitoringSummaryResponse>();

            var awal = totals.Value!.WindowStartUtc;
            var akhir = totals.Value.WindowEndUtc;

            var tandaVital = await _vitalSignService.GetSeriesAsync(episodeId, awal, akhir.AddTicks(-1), cancellationToken);

            if (!tandaVital.IsSuccess)
                return tandaVital.Cast<DailyMonitoringSummaryResponse>();

            var nyeri = await _dbContext.Set<TrxPatientAssessment>().AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete &&
                            x.AssessmentType == PatientAssessmentType.PainMonitoring &&
                            x.AssessmentStatus == PatientAssessmentStatus.Completed &&
                            x.AssessmentDateTime < akhir)
                .OrderByDescending(x => x.AssessmentDateTime)
                .Select(x => new DailyMonitoringLatestPain
                {
                    AssessmentId = x.Id,
                    PainScale = x.PainScale,
                    PainAssessmentState = x.PainAssessmentState,
                    ClinicalDateTime = x.AssessmentDateTime,
                    PainReassessmentDueAt = x.PainReassessmentDueAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            var gds = await _dbContext.Set<CliBloodGlucoseReading>().AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete && x.ReadingStatus == ClinicalMeasurementStatus.Active &&
                            x.MeasuredAt >= awal && x.MeasuredAt < akhir)
                .OrderBy(x => x.MeasuredAt)
                .ToListAsync(cancellationToken);

            var cairan = await _dbContext.Set<CliFluidBalanceEntry>().AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete && x.EntryStatus == ClinicalMeasurementStatus.Active &&
                            x.EntryDateTime >= awal && x.EntryDateTime < akhir)
                .OrderBy(x => x.EntryDateTime)
                .ToListAsync(cancellationToken);

            var observasi = await _dbContext.Set<CliDailyObservation>().AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete && x.ObservationStatus == ClinicalMeasurementStatus.Active &&
                            x.ObservedAt >= awal && x.ObservedAt < akhir)
                .OrderBy(x => x.ObservedAt)
                .ToListAsync(cancellationToken);

            // VAL-KEP-36c — pengingat dosis diberikan tanpa entri intake. Tidak mewajibkan.
            var tanpaIntake = await _dbContext.Set<PhmMedicationAdministration>().AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete &&
                            x.DoseStatus == MedicationDoseStatus.Administered &&
                            x.AdministeredAt >= awal && x.AdministeredAt < akhir &&
                            !_dbContext.Set<CliFluidBalanceEntry>().Any(e =>
                                e.MedicationAdministrationId == x.Id && !e.IsDelete && e.EntryStatus == ClinicalMeasurementStatus.Active))
                .Join(_dbContext.Set<PhmPrescriptionItem>().AsNoTracking(), d => d.PrescriptionItemId, i => i.Id, (d, i) => new AdministeredDoseWithoutIntakeItem
                {
                    AdministrationId = d.Id,
                    AdministrationNumber = d.AdministrationNumber,
                    DrugName = i.DrugNameSnapshot,
                    ActualDose = d.ActualDose,
                    ActualDoseUnit = d.ActualDoseUnitSnapshot,
                    ActualRoute = d.ActualRouteSnapshot,
                    AdministeredAt = d.AdministeredAt
                })
                .OrderBy(x => x.AdministeredAt)
                .ToListAsync(cancellationToken);

            var shifts = await ResolveShiftsAsync(
                await _dbContext.Set<InpEpisode>().AsNoTracking().Where(x => x.Id == episodeId).Select(x => x.ServiceUnitId).FirstAsync(cancellationToken),
                cancellationToken);

            var response = new DailyMonitoringSummaryResponse
            {
                EpisodeId = episodeId,
                Date = totals.Value.Date,
                WindowStartUtc = awal,
                WindowEndUtc = akhir,
                DayStartsAt = shifts.Count == 0 ? "00:00" : shifts.Min(x => x.StartTime).ToString("HH:mm", CultureInfo.InvariantCulture),
                VitalSigns = tandaVital.Value!,
                LatestPain = nyeri,
                GlucoseReadings = await ToGlucoseResponsesAsync(gds, cancellationToken),
                FluidTotals = totals.Value,
                FluidEntries = await ToFluidResponsesAsync(cairan, cancellationToken),
                Observations = await ToObservationResponsesAsync(observasi, cancellationToken),
                AdministeredDosesWithoutIntake = tanpaIntake
            };

            return NursingResult<DailyMonitoringSummaryResponse>.Ok(response, "Ringkasan Pengawasan Harian berhasil diambil.");
        }

        // =====================================================================
        // Konfigurasi shift — api-contract 7.9, FR-KEP-060
        // =====================================================================

        /// <summary>Shift satu unit; bila unit tidak punya shift sendiri, shift bawaan beserta <c>IsDefault = true</c>.</summary>
        public async Task<NursingShiftSetResponse> GetShiftSetAsync(Guid? serviceUnitId, CancellationToken cancellationToken = default)
        {
            var milikUnit = serviceUnitId.HasValue
                ? await ShiftRowsAsync(serviceUnitId.Value, cancellationToken)
                : new List<CliNursingShift>();

            var dipakai = milikUnit.Count > 0 ? milikUnit : await ShiftRowsAsync(null, cancellationToken);

            var namaUnit = serviceUnitId.HasValue
                ? await _dbContext.Set<MstServiceUnit>().AsNoTracking().Where(x => x.Id == serviceUnitId.Value).Select(x => x.ServiceUnitName).FirstOrDefaultAsync(cancellationToken)
                : null;

            return new NursingShiftSetResponse
            {
                ServiceUnitId = serviceUnitId,
                ServiceUnitName = namaUnit,
                IsDefault = milikUnit.Count == 0,
                IsConfigured = dipakai.Count > 0,
                Shifts = dipakai.OrderBy(x => x.StartTime).Select(x => new NursingShiftResponse
                {
                    Id = x.Id,
                    ShiftCode = x.ShiftCode,
                    ShiftName = x.ShiftName,
                    StartTime = x.StartTime.ToString("HH:mm", CultureInfo.InvariantCulture),
                    EndTime = x.EndTime.ToString("HH:mm", CultureInfo.InvariantCulture)
                }).ToList()
            };
        }

        /// <summary>
        /// Mengganti seluruh shift satu unit atau bawaan. Shift aktif wajib menutup 24 jam tanpa celah dan tanpa tumpang
        /// tindih — <c>VAL-KEP-26a</c>. Contoh ditolak: Pagi 07–14, Siang 14–20, Malam 21–07 → celah 20.00–21.00.
        /// </summary>
        public async Task<NursingResult<NursingShiftSetResponse>> ReplaceShiftSetAsync(
            ReplaceNursingShiftSetRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request.ServiceUnitId.HasValue)
            {
                var unitAda = await _dbContext.Set<MstServiceUnit>().AsNoTracking()
                    .AnyAsync(x => x.Id == request.ServiceUnitId.Value && !x.IsDelete, cancellationToken);

                if (!unitAda)
                    return NursingResult<NursingShiftSetResponse>.Fail(StatusCodes.Status404NotFound, "Unit layanan tidak ditemukan.");
            }

            var baru = new List<(string Code, string Name, TimeOnly Start, TimeOnly End)>();

            foreach (var s in request.Shifts ?? new List<NursingShiftRequest>())
            {
                var kode = s.ShiftCode?.Trim();
                var nama = s.ShiftName?.Trim();

                if (string.IsNullOrEmpty(kode) || kode.Length > 20 || string.IsNullOrEmpty(nama) || nama.Length > 50)
                    return BadRequest<NursingShiftSetResponse>("Kode shift (paling banyak 20 karakter) dan nama shift (paling banyak 50 karakter) wajib diisi.", "SHIFT_FIELDS_REQUIRED");

                if (!TimeOnly.TryParseExact(s.StartTime?.Trim(), FormatJam, CultureInfo.InvariantCulture, DateTimeStyles.None, out var mulai) ||
                    !TimeOnly.TryParseExact(s.EndTime?.Trim(), FormatJam, CultureInfo.InvariantCulture, DateTimeStyles.None, out var selesai))
                    return BadRequest<NursingShiftSetResponse>($"Jam shift {kode} tidak sah. Gunakan format 07:00.", "INVALID_TIME");

                if (baru.Any(x => string.Equals(x.Code, kode, StringComparison.OrdinalIgnoreCase)))
                    return BadRequest<NursingShiftSetResponse>($"Kode shift {kode} tercantum lebih dari sekali.", "DUPLICATE_SHIFT_CODE");

                baru.Add((kode, nama, mulai, selesai));
            }

            if (baru.Count > 0 && !CoversFullDay(baru.Select(x => (x.Start, x.End)).ToList()))
                return BadRequest<NursingShiftSetResponse>("Jam shift harus menutup 24 jam tanpa celah dan tanpa tumpang tindih.", "SHIFT_NOT_COVERING_DAY");

            var now = DateTime.UtcNow;
            var lama = await _dbContext.Set<CliNursingShift>()
                .Where(x => !x.IsDelete && x.ServiceUnitId == request.ServiceUnitId)
                .ToListAsync(cancellationToken);

            foreach (var s in baru)
            {
                var baris = lama.FirstOrDefault(x => string.Equals(x.ShiftCode, s.Code, StringComparison.OrdinalIgnoreCase));

                if (baris == null)
                {
                    _dbContext.Set<CliNursingShift>().Add(new CliNursingShift
                    {
                        Id = Guid.NewGuid(),
                        ServiceUnitId = request.ServiceUnitId,
                        ShiftCode = s.Code,
                        ShiftName = s.Name,
                        StartTime = s.Start,
                        EndTime = s.End,
                        IsActive = true,
                        CreateDateTime = now,
                        CreateBy = actorUserId
                    });
                }
                else
                {
                    baris.ShiftName = s.Name;
                    baris.StartTime = s.Start;
                    baris.EndTime = s.End;
                    baris.IsActive = true;
                    baris.UpdateDateTime = now;
                    baris.UpdateBy = actorUserId;
                }
            }

            foreach (var sisa in lama.Where(x => !baru.Any(b => string.Equals(b.Code, x.ShiftCode, StringComparison.OrdinalIgnoreCase))))
            {
                sisa.IsActive = false;
                sisa.IsDelete = true;
                sisa.DeleteDateTime = now;
                sisa.DeleteBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(LogCategory, "NursingShift.Replace", "Jam shift perawat diganti.",
                new { request.ServiceUnitId, ShiftCount = baru.Count, UpdatedBy = actorUserId });

            return NursingResult<NursingShiftSetResponse>.Ok(await GetShiftSetAsync(request.ServiceUnitId, cancellationToken),
                baru.Count == 0 ? "Shift unit dihapus; unit memakai shift bawaan." : "Jam shift berhasil disimpan.");
        }

        // =====================================================================
        // Pembantu bersama
        // =====================================================================

        private sealed record ShiftWindow(string Code, string Name, DateTime StartUtc, DateTime EndUtc);

        private sealed record DayWindow(DateOnly Date, DateTime StartUtc, DateTime EndUtc, List<ShiftWindow> Shifts);

        private Task<List<CliNursingShift>> ShiftRowsAsync(Guid? serviceUnitId, CancellationToken cancellationToken) =>
            _dbContext.Set<CliNursingShift>().AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.ServiceUnitId == serviceUnitId)
                .ToListAsync(cancellationToken);

        /// <summary>Shift unit, atau bawaan bila unit tidak punya; daftar kosong bila keduanya belum dikonfigurasi.</summary>
        private async Task<List<CliNursingShift>> ResolveShiftsAsync(Guid serviceUnitId, CancellationToken cancellationToken)
        {
            var milikUnit = await ShiftRowsAsync(serviceUnitId, cancellationToken);
            return milikUnit.Count > 0 ? milikUnit : await ShiftRowsAsync(null, cancellationToken);
        }

        private static DayWindow BuildDayWindow(DateOnly? date, List<CliNursingShift> shifts, DateTime nowUtc)
        {
            if (shifts.Count == 0)
            {
                var hari = date ?? HospitalTimeZone.TodayLocal(nowUtc);
                return new DayWindow(hari, HospitalTimeZone.ToUtc(hari, TimeOnly.MinValue), HospitalTimeZone.ToUtc(hari.AddDays(1), TimeOnly.MinValue), new List<ShiftWindow>());
            }

            var urut = shifts.OrderBy(x => x.StartTime).ToList();
            var awalHari = urut[0].StartTime;

            DateOnly tanggal;

            if (date.HasValue)
            {
                tanggal = date.Value;
            }
            else
            {
                // Pukul 03.00 dengan hari shift mulai 07.00 masih termasuk hari shift kemarin.
                var lokal = HospitalTimeZone.ToLocal(nowUtc);
                tanggal = DateOnly.FromDateTime(lokal);

                if (TimeOnly.FromDateTime(lokal) < awalHari)
                    tanggal = tanggal.AddDays(-1);
            }

            var mulaiHari = HospitalTimeZone.ToUtc(tanggal, awalHari);
            var akhirHari = HospitalTimeZone.ToUtc(tanggal.AddDays(1), awalHari);

            var jendela = urut.Select(s =>
            {
                var tanggalMulai = s.StartTime >= awalHari ? tanggal : tanggal.AddDays(1);
                var mulai = HospitalTimeZone.ToUtc(tanggalMulai, s.StartTime);
                return new ShiftWindow(s.ShiftCode, s.ShiftName, mulai, mulai.AddMinutes(DurationMinutes(s.StartTime, s.EndTime)));
            }).ToList();

            return new DayWindow(tanggal, mulaiHari, akhirHari, jendela);
        }

        private static int DurationMinutes(TimeOnly start, TimeOnly end)
        {
            var mulai = start.Hour * 60 + start.Minute;
            var akhir = end.Hour * 60 + end.Minute;
            return (akhir - mulai + 1440) % 1440;
        }

        /// <summary>Menutup 24 jam tanpa celah dan tanpa tumpang tindih — <c>VAL-KEP-26a</c>.</summary>
        internal static bool CoversFullDay(List<(TimeOnly Start, TimeOnly End)> shifts)
        {
            var urut = shifts.OrderBy(x => x.Start).ToList();
            var total = 0;

            for (var i = 0; i < urut.Count; i++)
            {
                var durasi = DurationMinutes(urut[i].Start, urut[i].End);

                if (durasi == 0)
                    return false;

                total += durasi;

                var berikut = urut[(i + 1) % urut.Count].Start;
                var akhir = urut[i].End;

                if (akhir.Hour != berikut.Hour || akhir.Minute != berikut.Minute)
                    return false;
            }

            return total == 1440;
        }

        private async Task<Dictionary<Guid, string>> EmployeeNamesAsync(IEnumerable<Guid> employeeIds, CancellationToken cancellationToken)
        {
            var ids = employeeIds.Distinct().ToList();

            return ids.Count == 0
                ? new Dictionary<Guid, string>()
                : await _dbContext.Set<MstEmployee>().AsNoTracking()
                    .Where(x => ids.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.FullName, cancellationToken);
        }

        private async Task<Dictionary<Guid, string>> UserNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken)
        {
            var ids = userIds.Distinct().ToList();

            return ids.Count == 0
                ? new Dictionary<Guid, string>()
                : await _dbContext.Users.AsNoTracking()
                    .Where(x => ids.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);
        }

        /// <summary>Rentang baca daftar: bawaan 24 jam terakhir, paling panjang 31 hari.</summary>
        private static (DateTime From, DateTime To, string? Error) ResolveRange(DateTime? from, DateTime? to)
        {
            var sampai = to.HasValue ? AsUtc(to.Value) : DateTime.UtcNow;
            var dari = from.HasValue ? AsUtc(from.Value) : sampai - RentangBawaan;

            if (dari > sampai)
                return (dari, sampai, "Waktu awal tidak boleh setelah waktu akhir.");

            if (sampai - dari > TimeSpan.FromDays(MaxRangeDays))
                return (dari, sampai, $"Rentang paling panjang {MaxRangeDays} hari.");

            return (dari, sampai, null);
        }

        /// <summary>Waktu klinis tidak lebih dari 5 menit di masa depan dan tidak sebelum pasien masuk — <c>VAL-KEP-24c</c>.</summary>
        private static bool IsClinicalTimeInvalid(DateTime atUtc, DateTime? admittedAt, DateTime now) =>
            atUtc > now.AddMinutes(FutureToleranceMinutes) || (admittedAt.HasValue && atUtc < admittedAt.Value);

        internal static DateTime AsUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        private static string? Truncate(string? value, int max) =>
            string.IsNullOrWhiteSpace(value) ? null : (value.Trim().Length > max ? value.Trim()[..max] : value.Trim());

        private static string StatusLabel(ClinicalMeasurementStatus status) =>
            status == ClinicalMeasurementStatus.Cancelled ? "Dibatalkan" : "Aktif";

        private static NursingResult<T> BadRequest<T>(string message, string? code = null) =>
            NursingResult<T>.Fail(StatusCodes.Status400BadRequest, message, code);

        private static NursingResult<T> Conflict<T>(string message, string code) =>
            NursingResult<T>.Fail(StatusCodes.Status409Conflict, message, code);

        private static NursingResult<T> Stale<T>() =>
            NursingResult<T>.Fail(StatusCodes.Status409Conflict, "Data sudah diubah pengguna lain. Muat ulang.", "STALE_REVISION");
    }
}
