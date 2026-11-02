using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using E = QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services.McuErrorCodes;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services
{
    /// <summary>
    /// Jadwal harian MCU: hari buka, kuota, pelampauan kuota, dan dokter MCU jaga (<c>BE-MCU-007</c>,
    /// <c>MCU-API-v1</c> grup Mcu Daily Schedule, <c>VAL-MCU-10</c>..<c>15</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kuota adalah titik balapan utama modul.</b> <see cref="ReserveAsync"/> dan
    /// <see cref="ReleaseAsync"/> dipanggil service booking, <i>walk-in</i>, jadwal ulang, dan tutup
    /// hari. Keduanya menyimpan perubahan di dalam transaksi pemanggil, dan dijaga token konkurensi
    /// <c>McuDailySchedule.Version</c>: dua petugas yang mengambil kursi terakhir bersamaan sama-sama
    /// membaca "29 dari 30", tetapi hanya satu <c>UPDATE … WHERE "Version" = n</c> yang mengenai
    /// baris. Yang kalah memuat ulang baris; bila masih ada kursi ia mencoba lagi, bila penuh ia
    /// mendapat <c>VAL-MCU-12</c>.
    /// </para>
    /// <para>
    /// <b>Contoh berangka.</b> 27-10-2026 berkuota 30, sudah 30 terpakai. Booking ke-31 ditolak
    /// <i>"Kuota MCU tanggal 27-10-2026 sudah penuh (30/30)…"</i>. Kepala unit menyetujui pelampauan
    /// 5 beralasan <i>Kontrak tahunan PT Contoh</i>; booking ke-31 sampai ke-35 diterima, ke-36
    /// ditolak dengan <i>(35/35)</i>.
    /// </para>
    /// <para>
    /// <b><c>UsedQuota</c> tidak pernah diubah lewat API jadwal.</b> Satu-satunya jalur ubahnya adalah
    /// pakai/lepas kuota. Mengatur jadwal hanya menolak kuota di bawah yang sudah terpakai
    /// (<c>VAL-MCU-14</c>).
    /// </para>
    /// </remarks>
    public class McuScheduleService
    {
        public const int MaxRangeDays = 92;
        private const int DefaultRangeDays = 30;
        private const int MaxQuotaAttempts = 3;

        private readonly ApplicationDbContext _dbContext;

        public McuScheduleService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // =====================================================================
        // Baca
        // =====================================================================

        public async Task<McuResult<List<McuDailyScheduleResponse>>> GetSchedulesAsync(
            McuDailyScheduleQuery query,
            CancellationToken cancellationToken)
        {
            var from = query.From ?? Today();
            var to = query.To ?? from.AddDays(DefaultRangeDays - 1);

            var rangeError = CheckRange(from, to);
            if (rangeError != null)
                return McuResult<List<McuDailyScheduleResponse>>.Invalid(E.InvalidRequest, rangeError);

            return McuResult<List<McuDailyScheduleResponse>>.Ok(await ReadRangeAsync(from, to, cancellationToken));
        }

        // =====================================================================
        // Atur jadwal
        // =====================================================================

        public async Task<McuResult<List<McuDailyScheduleResponse>>> UpsertRangeAsync(
            UpsertMcuDailyScheduleRangeRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var rangeError = CheckRange(request.From, request.To);
            if (rangeError != null)
                return McuResult<List<McuDailyScheduleResponse>>.Invalid(E.InvalidRequest, rangeError);

            if (request.From < Today())
            {
                return McuResult<List<McuDailyScheduleResponse>>.Invalid(
                    E.InvalidRequest, "Jadwal tidak dapat diatur untuk tanggal yang sudah lewat.");
            }

            if (request.Quota < 0)
                return McuResult<List<McuDailyScheduleResponse>>.Invalid(E.InvalidRequest, "Kuota tidak boleh negatif.");

            if (request.DutyMcuDoctorId.HasValue)
            {
                var doctorValid = await _dbContext.Set<MstDoctor>().AsNoTracking()
                    .AnyAsync(x => x.Id == request.DutyMcuDoctorId.Value && !x.IsDelete && x.IsActive, cancellationToken);

                if (!doctorValid)
                {
                    return McuResult<List<McuDailyScheduleResponse>>.Invalid(
                        E.InvalidRequest, "Dokter MCU jaga tidak ditemukan atau tidak aktif.");
                }
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var existing = await _dbContext.McuDailySchedules
                .Where(x => !x.IsDelete && x.ScheduleDate >= request.From && x.ScheduleDate <= request.To)
                .ToDictionaryAsync(x => x.ScheduleDate, cancellationToken);

            var singleDay = request.From == request.To;
            var now = DateTime.UtcNow;

            for (var date = request.From; date <= request.To; date = date.AddDays(1))
            {
                if (!existing.TryGetValue(date, out var schedule))
                {
                    _dbContext.McuDailySchedules.Add(new McuDailySchedule
                    {
                        ScheduleDate = date,
                        IsOpen = request.IsOpen,
                        Quota = request.Quota,
                        DutyMcuDoctorId = request.DutyMcuDoctorId,
                        CreateDateTime = now,
                        CreateBy = actorUserId
                    });
                    continue;
                }

                if (request.Quota < schedule.UsedQuota)
                {
                    var message = $"Sudah ada {schedule.UsedQuota} peserta terjadwal; kuota tidak boleh di bawah {schedule.UsedQuota}.";
                    return McuResult<List<McuDailyScheduleResponse>>.Conflict(
                        E.Val14, singleDay ? message : $"{FormatDate(date)}: {message}");
                }

                if (!request.IsOpen && schedule.UsedQuota > 0)
                {
                    return McuResult<List<McuDailyScheduleResponse>>.Conflict(
                        E.Val14,
                        $"Sudah ada {schedule.UsedQuota} peserta terjadwal pada {FormatDate(date)}; tanggal ini tidak dapat ditutup.");
                }

                schedule.IsOpen = request.IsOpen;
                schedule.Quota = request.Quota;
                schedule.DutyMcuDoctorId = request.DutyMcuDoctorId;
                schedule.Version++;
                schedule.UpdateDateTime = now;
                schedule.UpdateBy = actorUserId;
            }

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception exception) when (IsConcurrencyOrUniqueConflict(exception))
            {
                return McuResult<List<McuDailyScheduleResponse>>.Conflict(
                    E.ConcurrencyConflict, "Jadwal baru saja diubah petugas lain. Muat ulang lalu coba lagi.");
            }

            await transaction.CommitAsync(cancellationToken);

            return McuResult<List<McuDailyScheduleResponse>>.Ok(await ReadRangeAsync(request.From, request.To, cancellationToken));
        }

        // =====================================================================
        // Pelampauan kuota — kepala unit (MCU-DEC-029)
        // =====================================================================

        public async Task<McuResult<McuDailyScheduleResponse>> ApproveQuotaOverrideAsync(
            DateOnly scheduleDate,
            CreateMcuQuotaOverrideRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            if (request.AdditionalQuota < 1)
                return McuResult<McuDailyScheduleResponse>.Invalid(E.InvalidRequest, "Jumlah pelampauan sekurang-kurangnya 1.");

            var reason = request.Reason?.Trim() ?? string.Empty;
            if (reason.Length == 0)
                return McuResult<McuDailyScheduleResponse>.Invalid(E.Val13, "Alasan pelampauan wajib diisi.");
            if (reason.Length > 500)
                return McuResult<McuDailyScheduleResponse>.Invalid(E.InvalidRequest, "Alasan pelampauan paling banyak 500 karakter.");

            if (scheduleDate < Today())
            {
                return McuResult<McuDailyScheduleResponse>.Invalid(
                    E.InvalidRequest, "Pelampauan kuota tidak dapat disetujui untuk tanggal yang sudah lewat.");
            }

            var now = DateTime.UtcNow;
            McuQuotaOverride? approval = null;

            for (var attempt = 1; attempt <= MaxQuotaAttempts; attempt++)
            {
                var schedule = await FindTrackedAsync(scheduleDate, cancellationToken);
                if (schedule == null || !schedule.IsOpen)
                    return McuResult<McuDailyScheduleResponse>.Conflict(E.Val10, ClosedMessage(scheduleDate));

                if (approval == null)
                {
                    approval = new McuQuotaOverride
                    {
                        DailyScheduleId = schedule.Id,
                        AdditionalQuota = request.AdditionalQuota,
                        Reason = reason,
                        ApprovedByUserId = actorUserId,
                        ApprovedAt = now,
                        CreateDateTime = now,
                        CreateBy = actorUserId
                    };
                    _dbContext.McuQuotaOverrides.Add(approval);
                }

                schedule.ApprovedOverrideQuota += request.AdditionalQuota;
                schedule.Version++;
                schedule.UpdateDateTime = now;
                schedule.UpdateBy = actorUserId;

                if (await TrySaveAsync(schedule, cancellationToken))
                {
                    var result = await ReadRangeAsync(scheduleDate, scheduleDate, cancellationToken);
                    return McuResult<McuDailyScheduleResponse>.Ok(result.Single());
                }
            }

            return McuResult<McuDailyScheduleResponse>.Conflict(
                E.ConcurrencyConflict, "Jadwal baru saja diubah petugas lain. Muat ulang lalu coba lagi.");
        }

        // =====================================================================
        // Pakai / lepas kuota — dipakai service lain di dalam transaksi pemanggil
        // =====================================================================

        /// <summary>
        /// Memakai satu kursi pada tanggal pemeriksaan (<c>VAL-MCU-10</c>, <c>11</c>, <c>12</c>).
        /// </summary>
        /// <remarks>
        /// Menyimpan perubahan sendiri agar balapan terdeteksi di sini, bukan pada simpan pemanggil.
        /// Panggil di dalam transaksi pemanggil: bila langkah sesudahnya gagal, pemakaian kuota ikut
        /// dibatalkan. Perubahan lain yang sudah ditambahkan pemanggil ke konteks ikut tersimpan pada
        /// saat ini — masih di dalam transaksi yang sama. Tanggal lampau (<c>VAL-MCU-15</c>) diperiksa
        /// pemanggil lewat <see cref="CheckNotPast"/>, karena <i>walk-in</i> selalu hari ini.
        /// </remarks>
        public async Task<McuResult<McuQuotaReservation>> ReserveAsync(
            DateOnly scheduleDate,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            for (var attempt = 1; attempt <= MaxQuotaAttempts; attempt++)
            {
                var schedule = await FindTrackedAsync(scheduleDate, cancellationToken);
                if (schedule == null || !schedule.IsOpen)
                    return McuResult<McuQuotaReservation>.Conflict(E.Val10, ClosedMessage(scheduleDate));

                var capacity = schedule.Quota + schedule.ApprovedOverrideQuota;
                if (schedule.UsedQuota >= capacity)
                {
                    // Penuh sejak awal → VAL-MCU-11. Penuh karena kalah balapan → VAL-MCU-12.
                    return attempt == 1
                        ? McuResult<McuQuotaReservation>.Conflict(
                            E.Val11,
                            $"Kuota MCU tanggal {FormatDate(scheduleDate)} sudah penuh ({schedule.UsedQuota}/{capacity}). " +
                            "Minta persetujuan kepala unit untuk melampaui.")
                        : McuResult<McuQuotaReservation>.Conflict(
                            E.Val12, "Kuota baru saja terpakai petugas lain. Muat ulang.");
                }

                schedule.UsedQuota++;
                schedule.Version++;
                schedule.UpdateDateTime = DateTime.UtcNow;
                schedule.UpdateBy = actorUserId;

                if (await TrySaveAsync(schedule, cancellationToken))
                    return McuResult<McuQuotaReservation>.Ok(ToReservation(schedule));
            }

            return McuResult<McuQuotaReservation>.Conflict(E.Val12, "Kuota baru saja terpakai petugas lain. Muat ulang.");
        }

        /// <summary>
        /// Melepas satu kursi — order dibatalkan, dijadwal ulang ke tanggal lain, atau ditutup
        /// <i>Tidak Hadir</i>. Tidak pernah membuat <c>UsedQuota</c> negatif.
        /// </summary>
        public async Task<McuResult<McuQuotaReservation>> ReleaseAsync(
            DateOnly scheduleDate,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            for (var attempt = 1; attempt <= MaxQuotaAttempts; attempt++)
            {
                var schedule = await FindTrackedAsync(scheduleDate, cancellationToken);
                if (schedule == null)
                    return McuResult<McuQuotaReservation>.NotFound($"Jadwal MCU tanggal {FormatDate(scheduleDate)} tidak ditemukan.");

                if (schedule.UsedQuota == 0)
                    return McuResult<McuQuotaReservation>.Ok(ToReservation(schedule));

                schedule.UsedQuota--;
                schedule.Version++;
                schedule.UpdateDateTime = DateTime.UtcNow;
                schedule.UpdateBy = actorUserId;

                if (await TrySaveAsync(schedule, cancellationToken))
                    return McuResult<McuQuotaReservation>.Ok(ToReservation(schedule));
            }

            return McuResult<McuQuotaReservation>.Conflict(
                E.ConcurrencyConflict, "Jadwal baru saja diubah petugas lain. Muat ulang lalu coba lagi.");
        }

        /// <summary>
        /// Pemeriksaan baca saja atas <c>VAL-MCU-10</c>/<c>11</c>, tanpa memakai kursi. Dipakai pemanggil
        /// sebelum menerbitkan nomor order, agar booking yang pasti ditolak tidak menghanguskan nomor.
        /// Kepastiannya tetap milik <see cref="ReserveAsync"/>.
        /// </summary>
        public async Task<McuResult<McuQuotaReservation>> CheckAvailabilityAsync(
            DateOnly scheduleDate,
            CancellationToken cancellationToken)
        {
            var schedule = await _dbContext.McuDailySchedules.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ScheduleDate == scheduleDate && !x.IsDelete, cancellationToken);

            if (schedule == null || !schedule.IsOpen)
                return McuResult<McuQuotaReservation>.Conflict(E.Val10, ClosedMessage(scheduleDate));

            var capacity = schedule.Quota + schedule.ApprovedOverrideQuota;
            if (schedule.UsedQuota >= capacity)
            {
                return McuResult<McuQuotaReservation>.Conflict(
                    E.Val11,
                    $"Kuota MCU tanggal {FormatDate(scheduleDate)} sudah penuh ({schedule.UsedQuota}/{capacity}). " +
                    "Minta persetujuan kepala unit untuk melampaui.");
            }

            return McuResult<McuQuotaReservation>.Ok(ToReservation(schedule));
        }

        /// <summary><c>VAL-MCU-15</c>: pesan bila tanggal pemeriksaan sebelum hari ini (WIB); <c>null</c> bila sah.</summary>
        public static string? CheckNotPast(DateOnly examinationDate) =>
            examinationDate < Today() ? "Tanggal pemeriksaan tidak boleh sebelum hari ini." : null;

        /// <summary>Format tanggal pesan MCU, contoh <c>27-10-2026</c>.</summary>
        public static string FormatDate(DateOnly date) => date.ToString("dd-MM-yyyy");

        // =====================================================================
        // Penolong
        // =====================================================================

        private static DateOnly Today() => DateOnly.FromDateTime(AppDateTimeHelper.OperationalDate());

        private static string ClosedMessage(DateOnly date) => $"MCU tutup pada {FormatDate(date)}.";

        private static string? CheckRange(DateOnly from, DateOnly to)
        {
            if (to < from)
                return "Tanggal akhir tidak boleh sebelum tanggal awal.";
            if (to.DayNumber - from.DayNumber + 1 > MaxRangeDays)
                return $"Rentang tanggal paling panjang {MaxRangeDays} hari.";
            return null;
        }

        private Task<McuDailySchedule?> FindTrackedAsync(DateOnly date, CancellationToken cancellationToken) =>
            _dbContext.McuDailySchedules.FirstOrDefaultAsync(x => x.ScheduleDate == date && !x.IsDelete, cancellationToken);

        /// <summary>
        /// Menyimpan; bila <c>Version</c> sudah dinaikkan pihak lain, memuat ulang baris jadwal dan
        /// mengembalikan <c>false</c> agar pemanggil menilai ulang dengan nilai terbaru.
        /// </summary>
        private async Task<bool> TrySaveAsync(McuDailySchedule schedule, CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                await _dbContext.Entry(schedule).ReloadAsync(cancellationToken);
                return false;
            }
        }

        private static bool IsConcurrencyOrUniqueConflict(Exception exception) =>
            exception is DbUpdateConcurrencyException
            || (exception is DbUpdateException update
                && (update.InnerException as PostgresException)?.SqlState == PostgresErrorCodes.UniqueViolation);

        private static McuQuotaReservation ToReservation(McuDailySchedule schedule) => new()
        {
            DailyScheduleId = schedule.Id,
            ScheduleDate = schedule.ScheduleDate,
            UsedQuota = schedule.UsedQuota,
            Capacity = schedule.Quota + schedule.ApprovedOverrideQuota,
            DutyMcuDoctorId = schedule.DutyMcuDoctorId
        };

        private async Task<List<McuDailyScheduleResponse>> ReadRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken)
        {
            var rows = await _dbContext.McuDailySchedules.AsNoTracking()
                .Where(x => !x.IsDelete && x.ScheduleDate >= from && x.ScheduleDate <= to)
                .Select(x => new
                {
                    x.Id,
                    x.ScheduleDate,
                    x.IsOpen,
                    x.Quota,
                    x.UsedQuota,
                    x.ApprovedOverrideQuota,
                    x.DutyMcuDoctorId,
                    DutyMcuDoctorName = x.DutyMcuDoctor != null ? x.DutyMcuDoctor.FullName : null,
                    x.Version
                })
                .ToDictionaryAsync(x => x.ScheduleDate, cancellationToken);

            var ids = rows.Values.Select(x => x.Id).ToList();
            var overrides = await _dbContext.McuQuotaOverrides.AsNoTracking()
                .Where(x => ids.Contains(x.DailyScheduleId) && !x.IsDelete)
                .OrderBy(x => x.ApprovedAt)
                .Select(x => new
                {
                    x.DailyScheduleId,
                    Response = new McuQuotaOverrideResponse
                    {
                        Id = x.Id,
                        AdditionalQuota = x.AdditionalQuota,
                        Reason = x.Reason,
                        ApprovedByUserId = x.ApprovedByUserId,
                        ApprovedAt = x.ApprovedAt
                    }
                })
                .ToListAsync(cancellationToken);

            var result = new List<McuDailyScheduleResponse>();
            for (var date = from; date <= to; date = date.AddDays(1))
            {
                if (!rows.TryGetValue(date, out var row))
                {
                    result.Add(new McuDailyScheduleResponse { ScheduleDate = date });
                    continue;
                }

                var capacity = row.Quota + row.ApprovedOverrideQuota;
                result.Add(new McuDailyScheduleResponse
                {
                    Id = row.Id,
                    ScheduleDate = date,
                    IsConfigured = true,
                    IsOpen = row.IsOpen,
                    Quota = row.Quota,
                    ApprovedOverrideQuota = row.ApprovedOverrideQuota,
                    Capacity = capacity,
                    UsedQuota = row.UsedQuota,
                    RemainingQuota = row.IsOpen ? Math.Max(0, capacity - row.UsedQuota) : 0,
                    DutyMcuDoctorId = row.DutyMcuDoctorId,
                    DutyMcuDoctorName = row.DutyMcuDoctorName,
                    Version = row.Version,
                    QuotaOverrides = overrides.Where(x => x.DailyScheduleId == row.Id).Select(x => x.Response).ToList()
                });
            }

            return result;
        }
    }
}
