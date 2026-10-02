using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    /// <summary>
    /// Hasil aksi penghentian satu butir obat — BE-RWI-100.
    /// </summary>
    public sealed class StopPrescriptionItemResult
    {
        public bool IsSuccess { get; private init; }
        public int StatusCode { get; private init; }
        public string Message { get; private init; } = string.Empty;
        public InpatientPrescriptionItemResponse? Data { get; private init; }

        public static StopPrescriptionItemResult Success(InpatientPrescriptionItemResponse data, string message) => new()
        {
            IsSuccess = true,
            StatusCode = StatusCodes.Status200OK,
            Message = message,
            Data = data
        };

        public static StopPrescriptionItemResult Fail(int statusCode, string message) => new()
        {
            IsSuccess = false,
            StatusCode = statusCode,
            Message = message
        };
    }

    /// <summary>
    /// Periode saring Resep Harian — <c>BE-RWI-099</c>, <c>RWI-DEC-121</c> butir (1).
    /// </summary>
    public enum DailyPrescriptionPeriod
    {
        /// <summary>Tanpa saring periode — seluruh resep episode. Perilaku endpoint sebelum task ini.</summary>
        All = 0,

        /// <summary>Hari ini, pukul 00.00 sampai 24.00 waktu rumah sakit.</summary>
        Today = 1,

        /// <summary>Minggu berjalan, Senin 00.00 sampai Senin berikutnya 00.00 waktu rumah sakit.</summary>
        ThisWeek = 2,

        /// <summary>Bulan berjalan, tanggal 1 pukul 00.00 sampai tanggal 1 bulan berikutnya.</summary>
        ThisMonth = 3,

        /// <summary>Rentang tanggal <c>from</c> sampai <c>to</c>, keduanya termasuk.</summary>
        Range = 4
    }

    /// <summary>
    /// Hasil penyelesaian periode: jendela waktu UTC, atau penolakan beserta kode statusnya.
    /// </summary>
    public sealed record DailyPrescriptionPeriodResult(
        bool IsValid,
        int StatusCode,
        string? ErrorMessage,
        DailyPrescriptionPeriod Period,
        DateTime? FromUtc,
        DateTime? ToUtcExclusive);

    /// <summary>
    /// Pembaca Resep Harian satu episode rawat inap — <c>BE-RWI-099</c>, <c>FR-DOK-086</c>,
    /// api-contract 0.6.0 bagian 12.6.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Resep Harian bukan jenis resep baru</b> (<c>RWI-DEC-121</c> butir 2). Ia adalah daftar
    /// seluruh resep episode yang disaring per periode. Racikan beserta bahannya dan obat pulang
    /// ikut tampil; obat pulang tetap terbedakan lewat <c>PrescriptionOrderType</c>. Butir yang
    /// sudah dihentikan <b>tetap tampil</b> beserta siapa, kapan, dan kenapa.
    /// </para>
    /// <para>
    /// <b>Batas hari dihitung pada waktu rumah sakit (Asia/Jakarta)</b>, bukan UTC. Tanpa itu, resep
    /// yang ditulis pukul 06.00 WIB akan jatuh ke "kemarin" karena di UTC masih pukul 23.00.
    /// </para>
    /// <para>
    /// <b>Contoh.</b> Budi dirawat sejak Senin 1 September. Kamis 4 September dr. Rina memilih
    /// "Minggu Ini": jendela 1 September 00.00 WIB sampai 8 September 00.00 WIB, sehingga resep hari
    /// ke-1 dan hari ke-3 tampil. Memilih "Rentang" dengan <c>from=2026-09-03</c> tanpa <c>to</c>
    /// → <c>422</c>.
    /// </para>
    /// <para>
    /// Service ini hanya membaca. Keadaan pemenuhan resep tidak diubah di sini — <c>RUL-DOK-01</c>.
    /// </para>
    /// </remarks>
    public class InpatientPrescriptionService
    {
        private const string BusinessTimeZoneId = "Asia/Jakarta";
        private const string WindowsBusinessTimeZoneId = "SE Asia Standard Time";

        private readonly ApplicationDbContext _dbContext;
        private readonly MedicationAdministrationService _medicationAdministrationService;
        private readonly SlidingScaleOrderService _slidingScaleOrderService;
        private readonly InpatientClinicalContextService _clinicalContextService;

        public InpatientPrescriptionService(
            ApplicationDbContext dbContext,
            MedicationAdministrationService medicationAdministrationService,
            SlidingScaleOrderService slidingScaleOrderService,
            InpatientClinicalContextService clinicalContextService)
        {
            _dbContext = dbContext;
            _medicationAdministrationService = medicationAdministrationService;
            _slidingScaleOrderService = slidingScaleOrderService;
            _clinicalContextService = clinicalContextService;
        }

        /// <summary>
        /// Menerjemahkan isian periode menjadi jendela waktu UTC.
        /// </summary>
        /// <remarks>
        /// Menerima dua kosakata sekaligus: kosakata kartu roadmap (<c>Today</c>, <c>ThisWeek</c>,
        /// <c>ThisMonth</c>, <c>Range</c>) dan kosakata api-contract 0.6.0 (<c>today</c>,
        /// <c>week</c>, <c>month</c>), tanpa membedakan huruf besar. <c>from</c>/<c>to</c> tanpa
        /// <c>period</c> dibaca sebagai <c>Range</c>. Tanpa keduanya, seluruh resep episode
        /// dikembalikan seperti sebelum task ini, sehingga pemanggil lama tidak berubah hasilnya.
        /// </remarks>
        public static DailyPrescriptionPeriodResult ResolvePeriod(
            string? period,
            DateOnly? from,
            DateOnly? to,
            DateTime nowUtc)
        {
            DailyPrescriptionPeriod jenis;

            if (string.IsNullOrWhiteSpace(period))
            {
                jenis = from.HasValue || to.HasValue
                    ? DailyPrescriptionPeriod.Range
                    : DailyPrescriptionPeriod.All;
            }
            else
            {
                switch (period.Trim().ToLowerInvariant())
                {
                    case "all":
                        jenis = DailyPrescriptionPeriod.All;
                        break;
                    case "today":
                        jenis = DailyPrescriptionPeriod.Today;
                        break;
                    case "week":
                    case "thisweek":
                        jenis = DailyPrescriptionPeriod.ThisWeek;
                        break;
                    case "month":
                    case "thismonth":
                        jenis = DailyPrescriptionPeriod.ThisMonth;
                        break;
                    case "range":
                        jenis = DailyPrescriptionPeriod.Range;
                        break;
                    default:
                        return Gagal(
                            StatusCodes.Status400BadRequest,
                            "Periode tidak dikenal. Gunakan Today, ThisWeek, ThisMonth, atau Range.");
                }
            }

            var zona = BusinessTimeZone();
            var hariIniLokal = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc), zona).Date;

            switch (jenis)
            {
                case DailyPrescriptionPeriod.All:
                    return new DailyPrescriptionPeriodResult(true, StatusCodes.Status200OK, null, jenis, null, null);

                case DailyPrescriptionPeriod.Today:
                    return Jendela(jenis, hariIniLokal, hariIniLokal.AddDays(1), zona);

                case DailyPrescriptionPeriod.ThisWeek:
                {
                    // Senin sebagai awal minggu. DayOfWeek.Sunday bernilai 0, sehingga Minggu
                    // dihitung sebagai hari ketujuh minggu yang sedang berjalan.
                    var selisih = ((int)hariIniLokal.DayOfWeek + 6) % 7;
                    var senin = hariIniLokal.AddDays(-selisih);
                    return Jendela(jenis, senin, senin.AddDays(7), zona);
                }

                case DailyPrescriptionPeriod.ThisMonth:
                {
                    var awalBulan = new DateTime(hariIniLokal.Year, hariIniLokal.Month, 1);
                    return Jendela(jenis, awalBulan, awalBulan.AddMonths(1), zona);
                }

                default:
                {
                    // BE-RWI-099 kriteria 4.
                    if (!from.HasValue || !to.HasValue)
                    {
                        return Gagal(
                            StatusCodes.Status422UnprocessableEntity,
                            "Periode Rentang wajib menyebut tanggal awal dan tanggal akhir.");
                    }

                    if (from.Value > to.Value)
                    {
                        return Gagal(
                            StatusCodes.Status422UnprocessableEntity,
                            "Tanggal awal tidak boleh setelah tanggal akhir.");
                    }

                    var awal = from.Value.ToDateTime(TimeOnly.MinValue);
                    var akhir = to.Value.ToDateTime(TimeOnly.MinValue).AddDays(1);
                    return Jendela(jenis, awal, akhir, zona);
                }
            }
        }

        /// <summary>
        /// Membaca resep episode pada jendela periode, beserta butir, racikan, dan bahan racikan.
        /// </summary>
        public async Task<(int TotalData, List<InpatientPrescriptionListItem> Items)> GetDailyPrescriptionsAsync(
            Guid episodeId,
            DailyPrescriptionPeriodResult period,
            PrescriptionOrderType? orderType,
            int pageNumber,
            int pageSize,
            Func<PhmPrescription, InpatientPrescriptionListItem> mapHeader,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<PhmPrescription>()
                .AsNoTracking()
                .Include(x => x.Encounter)
                .Include(x => x.Consultation)
                .Include(x => x.Patient)
                .Include(x => x.Doctor)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete);

            if (orderType.HasValue)
                query = query.Where(x => x.PrescriptionOrderType == orderType.Value);

            if (period.FromUtc.HasValue)
                query = query.Where(x => x.PrescriptionDateTime >= period.FromUtc.Value);

            if (period.ToUtcExclusive.HasValue)
                query = query.Where(x => x.PrescriptionDateTime < period.ToUtcExclusive.Value);

            var totalData = await query.CountAsync(cancellationToken);

            var headers = await query
                .OrderBy(x => x.PrescriptionDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var prescriptionIds = headers.Select(x => x.Id).ToList();

            var butir = await _dbContext.Set<PhmPrescriptionItem>()
                .AsNoTracking()
                .Include(x => x.StoppedByUser)
                .Where(x => prescriptionIds.Contains(x.PrescriptionId) && !x.IsDelete)
                .OrderBy(x => x.SortOrder)
                .ToListAsync(cancellationToken);

            var racikan = await _dbContext.Set<PhmPrescriptionCompound>()
                .AsNoTracking()
                .Include(x => x.Items)
                .Where(x => prescriptionIds.Contains(x.PrescriptionId) && !x.IsDelete)
                .OrderBy(x => x.SortOrder)
                .ToListAsync(cancellationToken);

            var hasil = new List<InpatientPrescriptionListItem>(headers.Count);

            foreach (var header in headers)
            {
                var baris = mapHeader(header);

                baris.Items = butir
                    .Where(x => x.PrescriptionId == header.Id)
                    .Select(ToItemResponse)
                    .ToList();

                baris.Compounds = racikan
                    .Where(x => x.PrescriptionId == header.Id)
                    .Select(ToCompoundResponse)
                    .ToList();

                hasil.Add(baris);
            }

            return (totalData, hasil);
        }

        /// <summary>
        /// Menghentikan satu butir obat dari Resep Harian — BE-RWI-100, FR-DOK-087, INT-KEP-09, INT-DOK-16.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Aturan bisnis:</b>
        /// 1. Dokter dengan penugasan aktif (VAL-DOK-51); dokter tidak bertugas → 403.
        /// 2. Alasan penghentian wajib diisi (VAL-DOK-51a); kosong → 400.
        /// 3. Butir sudah dihentikan atau resep dibatalkan → 409 (VAL-DOK-51b).
        /// 4. Perawatan sudah ditutup → 422.
        /// 5. Seluruh dosis Due pada MAR setelah waktu henti dibatalkan (CancelDueDosesForItemAsync).
        /// 6. Order sliding scale aktif pada butir insulin ikut dihentikan (StopForPrescriptionItemAsync).
        /// 7. Seluruh langkah dieksekusi dalam satu transaksi atomik (AC-5).
        /// </para>
        /// </remarks>
        public async Task<StopPrescriptionItemResult> StopItemAsync(
            Guid itemId,
            StopPrescriptionItemRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request?.Reason))
            {
                return StopPrescriptionItemResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan penghentian wajib diisi.");
            }

            var item = await _dbContext.Set<PhmPrescriptionItem>()
                .Include(x => x.Prescription)
                .Include(x => x.StoppedByUser)
                .FirstOrDefaultAsync(x => x.Id == itemId && !x.IsDelete, cancellationToken);

            if (item == null || item.Prescription == null || item.Prescription.IsDelete)
            {
                return StopPrescriptionItemResult.Fail(
                    StatusCodes.Status404NotFound,
                    "Item resep tidak ditemukan.");
            }

            if (item.IsStopped)
            {
                return StopPrescriptionItemResult.Fail(
                    StatusCodes.Status409Conflict,
                    "Obat ini sudah dihentikan.");
            }

            if (item.Prescription.PrescriptionStatus == PrescriptionStatus.Cancelled)
            {
                return StopPrescriptionItemResult.Fail(
                    StatusCodes.Status409Conflict,
                    "Resep ini sudah dibatalkan.");
            }

            var episodeId = item.Prescription.InpEpisodeId;
            var nowUtc = DateTime.UtcNow;

            if (episodeId.HasValue && episodeId.Value != Guid.Empty)
            {
                var episode = await _dbContext.Set<InpEpisode>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == episodeId.Value && !x.IsDelete, cancellationToken);

                if (episode != null && (episode.EpisodeStatus == InpEpisodeStatus.Closed || episode.EpisodeStatus == InpEpisodeStatus.Cancelled))
                {
                    return StopPrescriptionItemResult.Fail(
                        StatusCodes.Status422UnprocessableEntity,
                        "Perawatan pasien sudah ditutup.");
                }

                var doctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(user, actorUserId, cancellationToken);
                if (!doctorId.HasValue || doctorId.Value == Guid.Empty)
                {
                    return StopPrescriptionItemResult.Fail(
                        StatusCodes.Status403Forbidden,
                        "Anda tidak sedang bertugas atas pasien ini.");
                }

                var isAssigned = await _clinicalContextService.IsDoctorAssignedAsync(
                    episodeId.Value,
                    doctorId.Value,
                    nowUtc,
                    cancellationToken);

                if (!isAssigned)
                {
                    return StopPrescriptionItemResult.Fail(
                        StatusCodes.Status403Forbidden,
                        "Anda tidak sedang bertugas atas pasien ini.");
                }
            }
            else
            {
                var doctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(user, actorUserId, cancellationToken);
                if (!doctorId.HasValue || (item.Prescription.DoctorId != doctorId.Value && actorUserId != item.Prescription.CreateBy))
                {
                    return StopPrescriptionItemResult.Fail(
                        StatusCodes.Status403Forbidden,
                        "Hanya dokter yang merawat yang dapat menghentikan resep ini.");
                }
            }

            var cleanReason = request.Reason.Trim();
            var stoppedAt = nowUtc;

            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // 1. Tandai butir dihentikan (tidak dihapus dan tidak dibatalkan)
                    item.IsStopped = true;
                    item.StoppedAt = stoppedAt;
                    item.StoppedByUserId = actorUserId;
                    item.StopReason = cleanReason.Length > 500 ? cleanReason[..500] : cleanReason;
                    item.UpdateDateTime = stoppedAt;
                    item.UpdateBy = actorUserId;

                    // 2. Batalkan seluruh dosis Due pada MAR yang dijadwalkan >= waktu henti (INT-KEP-09)
                    await _medicationAdministrationService.CancelDueDosesForItemAsync(
                        item.Id,
                        stoppedAt,
                        actorUserId,
                        MedicationAdministrationService.AlasanResepDihentikan,
                        cancellationToken);

                    // 3. Hentikan order sliding scale jika butir ini memiliki order aktif (INT-DOK-16)
                    await _slidingScaleOrderService.StopForPrescriptionItemAsync(
                        item.Id,
                        cleanReason,
                        actorUserId,
                        stoppedAt,
                        cancellationToken);

                    // 4. Simpan seluruh entitas dalam transaksi yang sama
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });

            if (item.StoppedByUser == null && actorUserId != Guid.Empty)
            {
                item.StoppedByUser = await _dbContext.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == actorUserId, cancellationToken);
            }

            var response = ToItemResponse(item);
            return StopPrescriptionItemResult.Success(response, "Obat berhasil dihentikan.");
        }

        public static InpatientPrescriptionItemResponse ToItemResponse(PhmPrescriptionItem x) => new()
        {
            Id = x.Id,
            PrescriptionId = x.PrescriptionId,
            DrugId = x.DrugId,
            DrugCodeSnapshot = x.DrugCodeSnapshot,
            DrugNameSnapshot = x.DrugNameSnapshot,
            GenericNameSnapshot = x.GenericNameSnapshot,
            DrugFormSnapshot = x.DrugFormSnapshot,
            StrengthSnapshot = x.StrengthSnapshot,
            RouteSnapshot = x.RouteSnapshot,
            IsFormularySnapshot = x.IsFormularySnapshot,
            IsHighAlertSnapshot = x.IsHighAlertSnapshot,
            Dose = x.Dose,
            DoseUnitNameSnapshot = x.DoseUnitNameSnapshot,
            FrequencyCode = x.FrequencyCode,
            FrequencyText = x.FrequencyText,
            IsAsNeeded = x.IsAsNeeded,
            Signa = x.Signa,
            AdministrationInstruction = x.AdministrationInstruction,
            Quantity = x.Quantity,
            DispenseUnitNameSnapshot = x.DispenseUnitNameSnapshot,
            DoseKind = x.DoseKind,
            IsStopped = x.IsStopped,
            StoppedAt = x.StoppedAt,
            StoppedByUserId = x.StoppedByUserId,
            StoppedByName = x.StoppedByUser?.DisplayName,
            StopReason = x.StopReason,
            SortOrder = x.SortOrder
        };

        private static InpatientPrescriptionCompoundResponse ToCompoundResponse(PhmPrescriptionCompound x) => new()
        {
            Id = x.Id,
            PrescriptionId = x.PrescriptionId,
            CompoundName = x.CompoundName,
            CompoundForm = x.CompoundForm,
            TotalPackage = x.TotalPackage,
            PackageUnitNameSnapshot = x.PackageUnitNameSnapshot,
            DosePerUse = x.DosePerUse,
            DoseUnitNameSnapshot = x.DoseUnitNameSnapshot,
            FrequencyText = x.FrequencyText,
            IsAsNeeded = x.IsAsNeeded,
            Signa = x.Signa,
            AdministrationInstruction = x.AdministrationInstruction,
            SortOrder = x.SortOrder,
            Ingredients = (x.Items ?? new List<PhmPrescriptionCompoundItem>())
                .Where(i => !i.IsDelete)
                .OrderBy(i => i.SortOrder)
                .Select(i => new InpatientPrescriptionCompoundIngredientResponse
                {
                    Id = i.Id,
                    DrugId = i.DrugId,
                    DrugNameSnapshot = i.DrugNameSnapshot,
                    StrengthSnapshot = i.StrengthSnapshot,
                    AmountPerPackage = i.AmountPerPackage,
                    TotalQuantity = i.TotalQuantity,
                    QuantityUnitNameSnapshot = i.QuantityUnitNameSnapshot
                })
                .ToList()
        };

        private static DailyPrescriptionPeriodResult Jendela(
            DailyPrescriptionPeriod jenis,
            DateTime awalLokal,
            DateTime akhirLokal,
            TimeZoneInfo zona)
        {
            var awalUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(awalLokal, DateTimeKind.Unspecified), zona);
            var akhirUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(akhirLokal, DateTimeKind.Unspecified), zona);

            return new DailyPrescriptionPeriodResult(true, StatusCodes.Status200OK, null, jenis, awalUtc, akhirUtc);
        }

        private static DailyPrescriptionPeriodResult Gagal(int statusCode, string message)
            => new(false, statusCode, message, DailyPrescriptionPeriod.All, null, null);

        private static TimeZoneInfo BusinessTimeZone()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(BusinessTimeZoneId); }
            catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById(WindowsBusinessTimeZoneId); }
        }
    }
}
