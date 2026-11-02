using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services
{
    /// <summary>Konfigurasi <c>MedicalCheckup</c> untuk tutup hari (<c>BE-MCU-012</c>).</summary>
    public sealed class McuNoShowOptions
    {
        public const string SectionName = "MedicalCheckup";

        /// <summary>
        /// Saklar <c>MedicalCheckup:NoShowEnabled</c>. Bawaan menyala karena <i>Tidak Hadir</i> adalah
        /// aturan bisnis inti (<c>MCU-DEC-014</c>); dimatikan saat menguji modul lain terhadap basis
        /// data bersama, karena pekerjaan ini menulis ke basis data begitu aplikasi menyala.
        /// </summary>
        public bool NoShowEnabled { get; set; } = true;

        /// <summary>
        /// Jam tutup hari <c>MedicalCheckup:NoShowCutoffLocalTime</c>, zona WIB. Bawaan 23.59.
        /// </summary>
        public TimeSpan NoShowCutoffLocalTime { get; set; } = new(23, 59, 0);
    }

    /// <summary>Hasil satu kali tutup hari.</summary>
    public sealed record McuNoShowResult(DateOnly ClosedThroughDate, int MarkedNoShow, int Failures);

    /// <summary>
    /// Tutup hari MCU: order <i>Terjadwal</i> yang tanggal pemeriksaannya sudah ditutup berubah menjadi
    /// <i>Tidak Hadir</i>, dan kursinya dilepas (<c>BE-MCU-012</c>, <c>MCU-DEC-014</c>, <c>MCU-DEC-030</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Contoh.</b> Budi booking 27-10-2026 dan tidak datang. Pukul 23.59 WIB 27 Oktober ordernya
    /// menjadi <i>Tidak Hadir</i>, kuota 27 Oktober kembali 29/30, dan riwayat mencatat perubahan oleh
    /// sistem. Konfirmasi datang lewat WhatsApp tidak menyelamatkan order — hanya check-in fisik yang
    /// menghitung (<c>MCU-DEC-030</c>). Tidak ada kunjungan maupun tagihan yang perlu dibatalkan, karena
    /// keduanya baru lahir saat check-in.
    /// </para>
    /// <para>
    /// <b>Tanggal yang sudah ditutup</b> ditentukan dari jam tutup: sebelum 23.59 WIB, yang ditutup
    /// adalah sampai kemarin; sejak 23.59, sampai hari ini. Server yang mati beberapa hari mengejar
    /// seluruh tanggal yang terlewat pada jalan berikutnya.
    /// </para>
    /// <para>
    /// <b>Idempoten dan per order.</b> Hanya order yang masih <i>Terjadwal</i> yang disentuh, sehingga
    /// jalan kedua tidak mengubah apa pun. Setiap order punya transaksinya sendiri; order yang gagal —
    /// misalnya sedang di-check-in petugas pada detik yang sama — dilewati dan dicoba pada siklus
    /// berikutnya, tanpa menggagalkan order lain.
    /// </para>
    /// </remarks>
    public class McuNoShowService
    {
        public const string SystemReason = "Tutup hari: peserta tidak check-in.";

        private readonly ApplicationDbContext _dbContext;
        private readonly McuScheduleService _scheduleService;

        public McuNoShowService(ApplicationDbContext dbContext, McuScheduleService scheduleService)
        {
            _dbContext = dbContext;
            _scheduleService = scheduleService;
        }

        /// <summary>Tanggal terakhir yang sudah ditutup pada saat <paramref name="nowUtc"/>.</summary>
        public static DateOnly ClosedThroughDate(DateTimeOffset nowUtc, TimeSpan cutoffLocalTime)
        {
            var local = TimeZoneInfo.ConvertTime(nowUtc, BusinessTimeZone);
            var today = DateOnly.FromDateTime(local.DateTime);
            return local.TimeOfDay >= cutoffLocalTime ? today : today.AddDays(-1);
        }

        public async Task<McuNoShowResult> CloseOverdueAsync(
            DateTimeOffset nowUtc,
            TimeSpan cutoffLocalTime,
            CancellationToken cancellationToken)
        {
            var closedThrough = ClosedThroughDate(nowUtc, cutoffLocalTime);

            var orderIds = await _dbContext.McuOrders.AsNoTracking()
                .Where(x => !x.IsDelete
                    && x.OrderStatus == McuOrderStatus.Scheduled
                    && x.ExaminationDate <= closedThrough)
                .OrderBy(x => x.ExaminationDate)
                .ThenBy(x => x.OrderNumber)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            var marked = 0;
            var failures = 0;

            foreach (var orderId in orderIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (await MarkNoShowAsync(orderId, nowUtc.UtcDateTime, cancellationToken))
                        marked++;
                }
                catch (DbUpdateException)
                {
                    failures++;
                }
                finally
                {
                    _dbContext.ChangeTracker.Clear();
                }
            }

            return new McuNoShowResult(closedThrough, marked, failures);
        }

        private async Task<bool> MarkNoShowAsync(Guid orderId, DateTime nowUtc, CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var order = await _dbContext.McuOrders
                .SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);

            // Sudah di-check-in, dijadwal ulang, atau dibatalkan sejak daftar dibaca.
            if (order == null || order.OrderStatus != McuOrderStatus.Scheduled)
                return false;

            var release = await _scheduleService.ReleaseAsync(order.ExaminationDate, Guid.Empty, cancellationToken);
            if (!release.IsSuccess && release.Kind != McuResultKind.NotFound)
                return false;

            order.OrderStatus = McuOrderStatus.NoShow;
            order.NoShowAt = nowUtc;
            order.Version++;
            order.UpdateDateTime = nowUtc;

            _dbContext.McuOrderStatusHistories.Add(new McuOrderStatusHistory
            {
                McuOrderId = order.Id,
                FromStatus = McuOrderStatus.Scheduled,
                ToStatus = McuOrderStatus.NoShow,
                ChangedAt = nowUtc,
                ChangedByUserId = null,
                Reason = SystemReason,
                CreateDateTime = nowUtc
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }

        /// <summary>Zona waktu bisnis <c>Asia/Jakarta</c>, cadangan Windows — sama dengan alokator nomor Platform.</summary>
        public static TimeZoneInfo BusinessTimeZone { get; } = ResolveBusinessTimeZone();

        private static TimeZoneInfo ResolveBusinessTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
        }
    }
}
