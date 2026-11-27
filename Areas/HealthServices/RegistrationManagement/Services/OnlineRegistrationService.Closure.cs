using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Penutupan otomatis pendaftaran online (<c>BE-KSK-PO-008</c>): Didaftarkan lewat Jalur Lain saat
    /// pasien mendapat kunjungan Rawat Jalan lewat jalur lain pada tanggal yang sama
    /// (<c>KSK-DEC-045</c>), dan Tidak Datang untuk tanggal kunjungan yang sudah lewat
    /// (<c>KSK-DEC-037</c>). Pola sama dengan Reservasi Prioritas (<c>RJ-PRI-DEC-013</c>, <c>027</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Siapa yang dianggap pasien yang sama.</b> Pendaftaran pasien lama lewat <c>PatientId</c>.
    /// Pendaftaran pasien baru yang belum check-in lewat NIK, dibandingkan <b>persis</b> dengan NIK pada
    /// master pasien kunjungan. Dokumen identitas lain tidak dipakai, supaya pendaftaran pasien lain
    /// tidak ikut tertutup. Aturan yang sama dipakai untuk mengecualikan pendaftaran itu dari hitungan
    /// kuota kunjungan (<see cref="CountClosableOnScheduleAsync"/>), sehingga tidak ada kuota ganda.
    /// </para>
    /// <para>
    /// <b>Tidak Datang idempoten.</b> Hanya menyentuh Menunggu Check-in dengan tanggal sebelum hari ini
    /// (WIB). Menjalankannya ulang tidak menambah riwayat, dan hari yang tertinggal ikut diproses.
    /// </para>
    /// </remarks>
    public partial class OnlineRegistrationService
    {
        private const int ClosureBatchSize = 200;
        private const int ClosureMaximumBatches = 500;

        /// <summary>
        /// Pendaftaran Menunggu Check-in milik pasien kunjungan: <c>PatientId</c> sama, atau pasien baru
        /// (belum check-in) dengan NIK sama persis dengan NIK master.
        /// </summary>
        public static Expression<Func<RegOnlineRegistration, bool>> BelongsToPatient(Guid patientId, string? masterIdentityNumber)
        {
            var nik = string.IsNullOrWhiteSpace(masterIdentityNumber) ? null : masterIdentityNumber.Trim();

            return x =>
                x.PatientId == patientId ||
                (nik != null &&
                 x.PatientId == null &&
                 x.PatientMode == OnlineRegistrationPatientMode.New &&
                 x.IdentityNumber == nik);
        }

        /// <summary>NIK master pasien, atau <c>null</c> bila kosong.</summary>
        public async Task<string?> GetMasterIdentityNumberAsync(Guid patientId, CancellationToken cancellationToken = default)
        {
            var nik = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => x.Id == patientId)
                .Select(x => x.IdentityNumber)
                .FirstOrDefaultAsync(cancellationToken);

            return string.IsNullOrWhiteSpace(nik) ? null : nik.Trim();
        }

        /// <summary>
        /// Kunci pendaftaran online pasien kunjungan (<c>REG_ONLINE_PATIENT_{PatientId}</c> dan, bila
        /// ada, <c>REG_ONLINE_PATIENT_NIK_{NIK}</c>). Dipanggil jalur kunjungan <b>sesudah</b> kunci
        /// kunjungan aktif dan <b>sebelum</b> kunci kuota, sehingga urutannya cocok dengan pengiriman dan
        /// ubah jadwal pendaftaran online (kunci pasien online, lalu kuota) dan tidak saling menunggu.
        /// </summary>
        public async Task LockPatientForEncounterAsync(Guid patientId, CancellationToken cancellationToken = default)
        {
            await _publicService.LockOnlinePatientAsync(patientId, null, cancellationToken);

            var nik = await GetMasterIdentityNumberAsync(patientId, cancellationToken);
            if (nik != null)
                await _publicService.LockOnlinePatientAsync(null, nik, cancellationToken);
        }

        /// <summary>
        /// Jumlah pendaftaran pasien ini pada jadwal dan tanggal itu yang akan ditutup Jalur Lain oleh
        /// kunjungan yang sedang dibuat. Dikurangkan dari hitungan kuota kunjungan.
        /// </summary>
        public async Task<int> CountClosableOnScheduleAsync(
            Guid patientId,
            Guid doctorScheduleId,
            DateTime visitDate,
            CancellationToken cancellationToken = default)
        {
            var nik = await GetMasterIdentityNumberAsync(patientId, cancellationToken);
            var date = ToCalendarDate(visitDate);

            return await _dbContext.RegOnlineRegistrations
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.Status == OnlineRegistrationStatus.AwaitingCheckIn &&
                    x.VisitDate == date &&
                    x.DoctorScheduleId == doctorScheduleId)
                .Where(BelongsToPatient(patientId, nik))
                .CountAsync(cancellationToken);
        }

        /// <summary>
        /// <c>KSK-DEC-045</c>: menutup pendaftaran pasien ini pada tanggal kunjungan sebagai Didaftarkan
        /// lewat Jalur Lain dan menautkannya ke kunjungan. Ikut transaksi kunjungan; tidak menyimpan
        /// sendiri. Kunci sudah diambil lewat <see cref="LockPatientForEncounterAsync"/>.
        /// </summary>
        public async Task<IReadOnlyList<RegOnlineRegistration>> CloseForEncounterAsync(
            Guid patientId,
            DateTime encounterDate,
            Guid encounterId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var nik = await GetMasterIdentityNumberAsync(patientId, cancellationToken);
            var visitDate = ToCalendarDate(encounterDate);

            var open = await _dbContext.RegOnlineRegistrations
                .Where(x =>
                    !x.IsDelete &&
                    x.Status == OnlineRegistrationStatus.AwaitingCheckIn &&
                    x.VisitDate == visitDate)
                .Where(BelongsToPatient(patientId, nik))
                .OrderBy(x => x.SubmittedAt)
                .ToListAsync(cancellationToken);

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var linked = false;

            foreach (var registration in open)
            {
                registration.Status = OnlineRegistrationStatus.RegisteredElsewhere;
                registration.ClosedAt = now;
                registration.CheckInTokenHash = null;
                registration.CheckInTokenExpiresAt = null;

                // EncounterId unik; aturan satu-per-tanggal membuat paling banyak satu pendaftaran di sini.
                if (!linked)
                {
                    registration.EncounterId = encounterId;
                    linked = true;
                }

                Touch(registration, actorUserId, now);

                _dbContext.RegOnlineRegistrationHistories.Add(new RegOnlineRegistrationHistory
                {
                    Id = Guid.NewGuid(),
                    OnlineRegistrationId = registration.Id,
                    EventCode = RegOnlineRegistrationHistory.EventRegisteredElsewhere,
                    FromStatus = OnlineRegistrationStatus.AwaitingCheckIn,
                    ToStatus = OnlineRegistrationStatus.RegisteredElsewhere,
                    Source = OnlineRegistrationEventSource.System,
                    ActorUserId = actorUserId == Guid.Empty ? null : actorUserId,
                    EncounterId = encounterId,
                    OccurredAt = now,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
            }

            return open;
        }

        /// <summary>
        /// <c>KSK-DEC-037</c>: Menunggu Check-in dengan tanggal kunjungan sebelum hari ini (WIB) menjadi
        /// Tidak Datang. Batch 200 baris; batch yang bentrok dengan perubahan petugas dibatalkan utuh dan
        /// dipilih ulang pada putaran berikutnya.
        /// </summary>
        public async Task<int> CloseExpiredAsync(CancellationToken cancellationToken = default)
        {
            var today = DateTime.SpecifyKind(
                TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), _businessTimeZone).Date,
                DateTimeKind.Unspecified);
            var closed = 0;

            for (var batch = 0; batch < ClosureMaximumBatches; batch++)
            {
                var expired = await _dbContext.RegOnlineRegistrations
                    .Where(x => !x.IsDelete && x.Status == OnlineRegistrationStatus.AwaitingCheckIn && x.VisitDate < today)
                    .OrderBy(x => x.VisitDate)
                    .ThenBy(x => x.Id)
                    .Take(ClosureBatchSize)
                    .ToListAsync(cancellationToken);

                if (expired.Count == 0)
                    break;

                var now = _timeProvider.GetUtcNow().UtcDateTime;

                foreach (var registration in expired)
                {
                    registration.Status = OnlineRegistrationStatus.NoShow;
                    registration.ClosedAt = now;
                    registration.CheckInTokenHash = null;
                    registration.CheckInTokenExpiresAt = null;
                    // Ditutup sistem, bukan pengguna.
                    Touch(registration, Guid.Empty, now);

                    _dbContext.RegOnlineRegistrationHistories.Add(new RegOnlineRegistrationHistory
                    {
                        Id = Guid.NewGuid(),
                        OnlineRegistrationId = registration.Id,
                        EventCode = RegOnlineRegistrationHistory.EventNoShow,
                        FromStatus = OnlineRegistrationStatus.AwaitingCheckIn,
                        ToStatus = OnlineRegistrationStatus.NoShow,
                        Source = OnlineRegistrationEventSource.System,
                        ActorUserId = null,
                        ToVisitDate = registration.VisitDate,
                        OccurredAt = now,
                        CreateDateTime = now
                    });
                }

                var transaction = _dbContext.Database.IsRelational()
                    ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                    : null;

                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    if (transaction != null)
                        await transaction.CommitAsync(cancellationToken);
                    closed += expired.Count;
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (transaction != null)
                        await transaction.RollbackAsync(cancellationToken);
                }
                finally
                {
                    if (transaction != null)
                        await transaction.DisposeAsync();
                    _dbContext.ChangeTracker.Clear();
                }

                if (expired.Count < ClosureBatchSize)
                    break;
            }

            if (closed > 0)
                _logger.LogInformation("Penutupan pendaftaran online: {Closed} pendaftaran menjadi Tidak Datang.", closed);

            return closed;
        }
    }
}
