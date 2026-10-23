using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using E = QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services.McuErrorCodes;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services
{
    /// <summary>
    /// Booking MCU, jadwal ulang, dan batal sebelum datang (<c>BE-MCU-011</c>, <c>MCU-API-v1</c> grup
    /// Mcu Order, <c>MCU-STATE-v1</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Booking sama dengan pendaftaran</b> (<c>MCU-DEC-003</c>). Booking hanya membuat satu order
    /// <i>Terjadwal</i> beserta potret komponennya dan memakai satu kursi kuota. <b>Tidak ada</b>
    /// kunjungan, baris tagihan, maupun pasien baru — ketiganya lahir saat check-in. Peserta baru
    /// disimpan sebagai identitas sementara ber-NIK (<c>MCU-DEC-033</c>).
    /// </para>
    /// <para>
    /// <b>Contoh.</b> Budi (belum pasien) dibooking Paket Basic untuk 27-10-2026 → order
    /// <c>MCU-RSMMC-000001</c> <i>Terjadwal</i>, <c>PatientId</c> kosong, kuota 1/30. Booking kedua
    /// Budi pada tanggal yang sama ditolak <i>"Peserta ini sudah terjadwal MCU pada 27-10-2026
    /// (MCU-RSMMC-000001)."</i>. Budi menelepon membatalkan → <i>Dibatalkan</i> beralasan, kuota
    /// kembali 0/30, tanpa refund (<c>MCU-DEC-038</c>).
    /// </para>
    /// <para>
    /// <b>Urutan langkah booking dan alasannya.</b> Seluruh pemeriksaan baca dijalankan lebih dulu —
    /// termasuk sisa kuota — supaya booking yang pasti ditolak tidak menghanguskan nomor. Nomor lalu
    /// diterbitkan <c>NumberSeriesAllocator</c> pada transaksinya sendiri, <b>sebelum</b> transaksi
    /// booking dibuka. Di dalam transaksi, order ganda diperiksa ulang di bawah kunci peserta+tanggal,
    /// lalu kursi dipakai lewat <see cref="McuScheduleService.ReserveAsync"/>. Nomor yang sudah terbit
    /// untuk booking yang kalah balapan hangus — sesuai <c>INV-PLT-002</c>.
    /// </para>
    /// <para>
    /// <b>Log aplikasi tidak memuat NIK, nama, maupun alasan</b>; controller hanya mencatat nomor order,
    /// tanggal, dan sumber.
    /// </para>
    /// </remarks>
    public class McuBookingService
    {
        private static readonly McuOrderStatus[] ActiveStatuses =
        [
            McuOrderStatus.Scheduled,
            McuOrderStatus.CheckedIn,
            McuOrderStatus.InExamination
        ];

        private readonly ApplicationDbContext _dbContext;
        private readonly McuScheduleService _scheduleService;
        private readonly McuOrderNumberService _numberService;
        private readonly McuOrderQueryService _queryService;
        private readonly McuCheckInService _checkInService;

        public McuBookingService(
            ApplicationDbContext dbContext,
            McuScheduleService scheduleService,
            McuOrderNumberService numberService,
            McuOrderQueryService queryService,
            McuCheckInService checkInService)
        {
            _dbContext = dbContext;
            _scheduleService = scheduleService;
            _numberService = numberService;
            _queryService = queryService;
            _checkInService = checkInService;
        }

        // =====================================================================
        // Booking
        // =====================================================================

        public async Task<McuResult<McuOrderDetailResponse>> CreateBookingAsync(
            CreateMcuBookingRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var pastError = McuScheduleService.CheckNotPast(request.ExaminationDate);
            if (pastError != null)
                return McuResult<McuOrderDetailResponse>.Invalid(E.Val15, pastError);

            var participant = await ResolveParticipantAsync(request, cancellationToken);
            if (participant.Error != null)
                return Fail(participant.Error);

            var package = await _dbContext.McuPackages.AsNoTracking()
                .Where(x => x.Id == request.PackageId && !x.IsDelete)
                .Select(x => new { x.Id, x.PackageCode, x.PackageName, x.IsActive })
                .SingleOrDefaultAsync(cancellationToken);

            if (package == null)
                return McuResult<McuOrderDetailResponse>.Invalid(E.InvalidRequest, "Paket MCU tidak ditemukan.");
            if (!package.IsActive)
                return McuResult<McuOrderDetailResponse>.Invalid(E.Val20, "Paket MCU sudah tidak aktif.");

            if (request.CompanyGuarantorId.HasValue)
            {
                var companyError = await CheckCompanyAsync(request.CompanyGuarantorId.Value, request.ExaminationDate, cancellationToken);
                if (companyError != null)
                    return Fail(companyError);
            }

            var duplicate = await FindDuplicateAsync(participant.PatientId, participant.MatchNik, request.ExaminationDate, null, cancellationToken);
            if (duplicate != null)
                return DuplicateConflict(request.ExaminationDate, duplicate);

            var availability = await _scheduleService.CheckAvailabilityAsync(request.ExaminationDate, cancellationToken);
            if (!availability.IsSuccess)
                return McuResult<McuOrderDetailResponse>.Conflict(availability.Code!, availability.Message!);

            var components = await LoadPackageComponentsAsync(package.Id, cancellationToken);

            string orderNumber;
            try
            {
                orderNumber = await _numberService.AllocateOrderNumberAsync(actorUserId, cancellationToken);
            }
            catch (NumberSeriesAllocationException)
            {
                return McuResult<McuOrderDetailResponse>.Conflict(
                    E.NumberAllocationFailed, "Nomor order MCU tidak dapat diterbitkan. Coba lagi.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            await LockParticipantAsync(participant.PatientId, participant.MatchNik, request.ExaminationDate, cancellationToken);

            duplicate = await FindDuplicateAsync(participant.PatientId, participant.MatchNik, request.ExaminationDate, null, cancellationToken);
            if (duplicate != null)
                return DuplicateConflict(request.ExaminationDate, duplicate);

            var reservation = await _scheduleService.ReserveAsync(request.ExaminationDate, actorUserId, cancellationToken);
            if (!reservation.IsSuccess)
                return McuResult<McuOrderDetailResponse>.Conflict(reservation.Code!, reservation.Message!);

            var now = DateTime.UtcNow;
            var order = new McuOrder
            {
                OrderNumber = orderNumber,
                PackageId = package.Id,
                PackageCodeSnapshot = package.PackageCode,
                PackageNameSnapshot = package.PackageName,
                PatientId = participant.PatientId,
                ParticipantName = participant.Name,
                ParticipantNik = participant.StoredNik,
                ParticipantBirthDate = participant.StoredBirthDate,
                ParticipantPhone = participant.Phone,
                PatientMatchMethod = participant.PatientId.HasValue ? McuPatientMatchMethod.ExistingAtBooking : null,
                Source = McuOrderSource.DirectBooking,
                CompanyGuarantorId = request.CompanyGuarantorId,
                ExaminationDate = request.ExaminationDate,
                DailyScheduleId = reservation.Value!.DailyScheduleId,
                OrderStatus = McuOrderStatus.Scheduled,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.McuOrders.Add(order);
            AddOrderComponents(order.Id, components, request.ExaminationDate, actorUserId, now);
            AddHistory(order.Id, null, McuOrderStatus.Scheduled, null, actorUserId, now);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return McuResult<McuOrderDetailResponse>.Ok((await _queryService.GetOrderAsync(order.Id, cancellationToken))!);
        }

        // =====================================================================
        // Walk-in admisi — BE-MCU-014
        // =====================================================================

        /// <summary>
        /// Peserta datang tanpa booking: order lahir langsung <i>Hadir</i>, memakai kuota hari ini
        /// (<c>MCU-DEC-017</c>, <c>MCU-DEC-037</c>), lewat jalur check-in yang sama dengan
        /// <c>BE-MCU-013</c> — semuanya dalam satu transaksi.
        /// </summary>
        /// <remarks>
        /// <b>Contoh.</b> Senin berkuota 30 sudah penuh; Siti datang tanpa booking → <c>409</c> menyebut
        /// (30/30). Kepala unit menyetujui pelampauan; Siti didaftarkan → satu order <i>Hadir</i>, satu
        /// kunjungan, satu baris paket. Bila langkah check-in mana pun gagal, order, kursi, kunjungan, dan
        /// baris tagihan batal bersama.
        /// </remarks>
        public async Task<McuResult<McuOrderDetailResponse>> CreateWalkInAsync(
            CreateMcuWalkInRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var today = DateOnly.FromDateTime(AppDateTimeHelper.OperationalDate());

            var phone = Normalize(request.ParticipantPhone);
            if (phone?.Length > 20)
                return McuResult<McuOrderDetailResponse>.Invalid(E.InvalidRequest, "Nomor HP peserta paling banyak 20 karakter.");

            var patient = await _dbContext.Set<MstPatient>().AsNoTracking()
                .Where(x => x.Id == request.PatientId && !x.IsDelete && x.IsActive)
                .Select(x => new { x.Id, x.FullName, x.IdentityNumber })
                .SingleOrDefaultAsync(cancellationToken);

            if (patient == null)
            {
                return McuResult<McuOrderDetailResponse>.Invalid(
                    E.Val16, "Pilih pasien terdaftar atau daftarkan pasien baru lebih dulu.");
            }

            var package = await _dbContext.McuPackages.AsNoTracking()
                .Where(x => x.Id == request.PackageId && !x.IsDelete)
                .Select(x => new { x.Id, x.PackageCode, x.PackageName, x.IsActive })
                .SingleOrDefaultAsync(cancellationToken);

            if (package == null)
                return McuResult<McuOrderDetailResponse>.Invalid(E.InvalidRequest, "Paket MCU tidak ditemukan.");
            if (!package.IsActive)
                return McuResult<McuOrderDetailResponse>.Invalid(E.Val20, "Paket MCU sudah tidak aktif.");

            if (request.CompanyGuarantorId.HasValue)
            {
                var companyError = await CheckCompanyAsync(request.CompanyGuarantorId.Value, today, cancellationToken);
                if (companyError != null)
                    return Fail(companyError);
            }

            var matchNik = NormalizeNik(patient.IdentityNumber);
            var duplicate = await FindDuplicateAsync(patient.Id, matchNik, today, null, cancellationToken);
            if (duplicate != null)
                return DuplicateConflict(today, duplicate);

            var checkIn = new McuCheckInRequest
            {
                McuDoctorId = request.McuDoctorId,
                PatientCompanyGuarantorId = request.PatientCompanyGuarantorId,
                RequirementOutcomes = request.RequirementOutcomes ?? [],
                Version = 0
            };

            var checkInError = await _checkInService.ValidateWalkInAsync(
                package.Id, request.CompanyGuarantorId, patient.Id, checkIn, cancellationToken);
            if (checkInError != null)
                return checkInError;

            var availability = await _scheduleService.CheckAvailabilityAsync(today, cancellationToken);
            if (!availability.IsSuccess)
                return McuResult<McuOrderDetailResponse>.Conflict(availability.Code!, availability.Message!);

            var components = await LoadPackageComponentsAsync(package.Id, cancellationToken);

            string orderNumber;
            try
            {
                orderNumber = await _numberService.AllocateOrderNumberAsync(actorUserId, cancellationToken);
            }
            catch (NumberSeriesAllocationException)
            {
                return McuResult<McuOrderDetailResponse>.Conflict(
                    E.NumberAllocationFailed, "Nomor order MCU tidak dapat diterbitkan. Coba lagi.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            await LockParticipantAsync(patient.Id, matchNik, today, cancellationToken);

            duplicate = await FindDuplicateAsync(patient.Id, matchNik, today, null, cancellationToken);
            if (duplicate != null)
                return DuplicateConflict(today, duplicate);

            var reservation = await _scheduleService.ReserveAsync(today, actorUserId, cancellationToken);
            if (!reservation.IsSuccess)
                return McuResult<McuOrderDetailResponse>.Conflict(reservation.Code!, reservation.Message!);

            var now = DateTime.UtcNow;
            var order = new McuOrder
            {
                OrderNumber = orderNumber,
                PackageId = package.Id,
                PackageCodeSnapshot = package.PackageCode,
                PackageNameSnapshot = package.PackageName,
                PatientId = patient.Id,
                ParticipantName = patient.FullName,
                ParticipantPhone = phone,
                Source = McuOrderSource.WalkInAdmission,
                CompanyGuarantorId = request.CompanyGuarantorId,
                ExaminationDate = today,
                DailyScheduleId = reservation.Value!.DailyScheduleId,
                // Hanya keadaan sementara di dalam transaksi ini; check-in di bawah memindahkannya ke
                // Hadir dan mencatat satu riwayat "— → Hadir".
                OrderStatus = McuOrderStatus.Scheduled,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.McuOrders.Add(order);
            AddOrderComponents(order.Id, components, today, actorUserId, now);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var result = await _checkInService.CheckInCoreAsync(order.Id, checkIn, actorUserId, walkIn: true, cancellationToken);
            if (!result.IsSuccess)
            {
                // Transaksi dibuang: order, kursi, kunjungan, dan baris tagihan batal bersama.
                _dbContext.ChangeTracker.Clear();
                return result;
            }

            await transaction.CommitAsync(cancellationToken);
            await _checkInService.PublishDeferredAsync();

            return McuResult<McuOrderDetailResponse>.Ok((await _queryService.GetOrderAsync(order.Id, cancellationToken))!);
        }

        // =====================================================================
        // Jadwal ulang
        // =====================================================================

        /// <summary>
        /// <i>Terjadwal</i> atau <i>Tidak Hadir</i> → <i>Terjadwal</i> pada tanggal baru
        /// (<c>MCU-DEC-014</c>). Kursi tanggal lama dilepas bila order masih <i>Terjadwal</i>; order
        /// <i>Tidak Hadir</i> sudah melepas kursinya saat tutup hari.
        /// </summary>
        public async Task<McuResult<McuOrderDetailResponse>> RescheduleAsync(
            Guid orderId,
            RescheduleMcuOrderRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders
                .Include(x => x.Patient)
                .SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);

            if (order == null)
                return McuResult<McuOrderDetailResponse>.NotFound("Order MCU tidak ditemukan.");

            var reason = Normalize(request.Reason);
            if (reason?.Length > 400)
                return McuResult<McuOrderDetailResponse>.Invalid(E.InvalidRequest, "Alasan jadwal ulang paling banyak 400 karakter.");

            if (order.Version != request.Version)
                return VersionConflict();

            if (order.OrderStatus == McuOrderStatus.Cancelled)
                return McuResult<McuOrderDetailResponse>.Conflict(E.InvalidTransition, "Order sudah dibatalkan.");

            if (order.OrderStatus is not (McuOrderStatus.Scheduled or McuOrderStatus.NoShow))
            {
                return McuResult<McuOrderDetailResponse>.Conflict(
                    E.InvalidTransition,
                    $"Order berstatus {McuLabels.OrderStatus(order.OrderStatus)} tidak dapat dijadwal ulang.");
            }

            if (request.NewDate == order.ExaminationDate)
                return McuResult<McuOrderDetailResponse>.Invalid(E.InvalidRequest, "Tanggal baru sama dengan tanggal sekarang.");

            var pastError = McuScheduleService.CheckNotPast(request.NewDate);
            if (pastError != null)
                return McuResult<McuOrderDetailResponse>.Invalid(E.Val15, pastError);

            var matchNik = NormalizeNik(order.ParticipantNik ?? order.Patient?.IdentityNumber);

            var duplicate = await FindDuplicateAsync(order.PatientId, matchNik, request.NewDate, order.Id, cancellationToken);
            if (duplicate != null)
                return DuplicateConflict(request.NewDate, duplicate);

            var availability = await _scheduleService.CheckAvailabilityAsync(request.NewDate, cancellationToken);
            if (!availability.IsSuccess)
                return McuResult<McuOrderDetailResponse>.Conflict(availability.Code!, availability.Message!);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            await LockParticipantAsync(order.PatientId, matchNik, request.NewDate, cancellationToken);

            duplicate = await FindDuplicateAsync(order.PatientId, matchNik, request.NewDate, order.Id, cancellationToken);
            if (duplicate != null)
                return DuplicateConflict(request.NewDate, duplicate);

            var oldDate = order.ExaminationDate;
            var fromStatus = order.OrderStatus;

            if (fromStatus == McuOrderStatus.Scheduled)
            {
                var release = await _scheduleService.ReleaseAsync(oldDate, actorUserId, cancellationToken);
                if (!release.IsSuccess && release.Kind != McuResultKind.NotFound)
                    return McuResult<McuOrderDetailResponse>.Conflict(release.Code!, release.Message!);
            }

            var reservation = await _scheduleService.ReserveAsync(request.NewDate, actorUserId, cancellationToken);
            if (!reservation.IsSuccess)
                return McuResult<McuOrderDetailResponse>.Conflict(reservation.Code!, reservation.Message!);

            var now = DateTime.UtcNow;
            order.ExaminationDate = request.NewDate;
            order.DailyScheduleId = reservation.Value!.DailyScheduleId;
            order.OrderStatus = McuOrderStatus.Scheduled;
            order.ArrivalConfirmedAt = null;
            order.Version++;
            order.UpdateDateTime = now;
            order.UpdateBy = actorUserId;

            var components = await _dbContext.McuOrderComponents
                .Where(x => x.McuOrderId == order.Id && !x.IsDelete
                    && x.ComponentStatus != McuOrderComponentStatus.Cancelled
                    && x.ScheduledDate == oldDate)
                .ToListAsync(cancellationToken);

            foreach (var component in components)
            {
                component.ScheduledDate = request.NewDate;
                component.UpdateDateTime = now;
                component.UpdateBy = actorUserId;
            }

            var historyReason = $"Dijadwal ulang dari {McuScheduleService.FormatDate(oldDate)} ke {McuScheduleService.FormatDate(request.NewDate)}"
                + (reason == null ? "." : $": {reason}");
            AddHistory(order.Id, fromStatus, McuOrderStatus.Scheduled, historyReason, actorUserId, now);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return VersionConflict();
            }

            await transaction.CommitAsync(cancellationToken);

            return McuResult<McuOrderDetailResponse>.Ok((await _queryService.GetOrderAsync(order.Id, cancellationToken))!);
        }

        // =====================================================================
        // Batal sebelum datang
        // =====================================================================

        /// <summary>
        /// <i>Terjadwal</i> → <i>Dibatalkan</i> beralasan; kursi dilepas; tanpa refund karena belum ada
        /// tagihan (<c>MCU-DEC-038</c>). Batal penuh sesudah check-in ada di <c>McuExaminationService.CancelOrderAsync</c> (<c>BE-MCU-025</c>), yang meneruskan status lain ke sini.
        /// </summary>
        public async Task<McuResult<McuOrderDetailResponse>> CancelBeforeArrivalAsync(
            Guid orderId,
            CancelMcuOrderRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders
                .SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);

            if (order == null)
                return McuResult<McuOrderDetailResponse>.NotFound("Order MCU tidak ditemukan.");

            var reason = Normalize(request.Reason);
            if (reason == null)
                return McuResult<McuOrderDetailResponse>.Invalid(E.Val23, "Alasan pembatalan wajib diisi.");
            if (reason.Length > 500)
                return McuResult<McuOrderDetailResponse>.Invalid(E.InvalidRequest, "Alasan pembatalan paling banyak 500 karakter.");

            if (order.Version != request.Version)
                return VersionConflict();

            switch (order.OrderStatus)
            {
                case McuOrderStatus.Scheduled:
                    break;
                case McuOrderStatus.Cancelled:
                    return McuResult<McuOrderDetailResponse>.Conflict(E.InvalidTransition, "Order sudah dibatalkan.");
                case McuOrderStatus.CheckedIn:
                case McuOrderStatus.InExamination:
                    // Dijawab McuExaminationService.CancelOrderAsync (BE-MCU-025) sebelum sampai ke sini.
                    return McuResult<McuOrderDetailResponse>.Conflict(
                        E.InvalidTransition, "Order sudah check-in; gunakan batal penuh.");
                default:
                    return McuResult<McuOrderDetailResponse>.Conflict(
                        E.InvalidTransition,
                        $"Order berstatus {McuLabels.OrderStatus(order.OrderStatus)} tidak dapat dibatalkan.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var release = await _scheduleService.ReleaseAsync(order.ExaminationDate, actorUserId, cancellationToken);
            if (!release.IsSuccess && release.Kind != McuResultKind.NotFound)
                return McuResult<McuOrderDetailResponse>.Conflict(release.Code!, release.Message!);

            var now = DateTime.UtcNow;
            order.OrderStatus = McuOrderStatus.Cancelled;
            order.CancelReason = reason;
            order.IsCancel = true;
            order.CancelDateTime = now;
            order.CancelBy = actorUserId;
            order.Version++;
            order.UpdateDateTime = now;
            order.UpdateBy = actorUserId;

            AddHistory(order.Id, McuOrderStatus.Scheduled, McuOrderStatus.Cancelled, reason, actorUserId, now);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return VersionConflict();
            }

            await transaction.CommitAsync(cancellationToken);

            return McuResult<McuOrderDetailResponse>.Ok((await _queryService.GetOrderAsync(order.Id, cancellationToken))!);
        }

        // =====================================================================
        // Peserta, perusahaan, order ganda
        // =====================================================================

        private sealed record Failure(string Code, string Message);

        /// <param name="MatchNik">NIK untuk pemeriksaan order ganda — dari isian peserta atau data pasien.</param>
        /// <param name="StoredNik">NIK yang disimpan pada order — hanya untuk identitas sementara.</param>
        private sealed record Participant(
            Guid? PatientId,
            string Name,
            string? MatchNik,
            string? StoredNik,
            DateOnly? StoredBirthDate,
            string? Phone,
            Failure? Error);

        private async Task<Participant> ResolveParticipantAsync(CreateMcuBookingRequest request, CancellationToken cancellationToken)
        {
            static Participant Reject(string code, string message) => new(null, string.Empty, null, null, null, null, new Failure(code, message));

            var phone = Normalize(request.ParticipantPhone);
            if (phone?.Length > 20)
                return Reject(E.InvalidRequest, "Nomor HP peserta paling banyak 20 karakter.");

            if (request.PatientId.HasValue)
            {
                var patient = await _dbContext.Set<MstPatient>().AsNoTracking()
                    .Where(x => x.Id == request.PatientId.Value && !x.IsDelete && x.IsActive)
                    .Select(x => new { x.Id, x.FullName, x.IdentityNumber })
                    .SingleOrDefaultAsync(cancellationToken);

                if (patient == null)
                    return Reject(E.InvalidRequest, "Pasien tidak ditemukan atau tidak aktif.");

                return new Participant(patient.Id, patient.FullName, NormalizeNik(patient.IdentityNumber), null, null, phone, null);
            }

            var name = Normalize(request.ParticipantName);
            var nik = NormalizeNik(request.ParticipantNik);

            if (name == null || nik == null || request.ParticipantBirthDate == null)
                return Reject(E.Val16, "Isi data pasien atau nama, NIK, dan tanggal lahir peserta.");

            if (nik.Length != 16 || !nik.All(char.IsAsciiDigit))
                return Reject(E.Val17, "NIK harus 16 digit angka.");

            if (name.Length > 150)
                return Reject(E.InvalidRequest, "Nama peserta paling banyak 150 karakter.");

            if (request.ParticipantBirthDate.Value > DateOnly.FromDateTime(AppDateTimeHelper.OperationalDate()))
                return Reject(E.InvalidRequest, "Tanggal lahir peserta tidak boleh sesudah hari ini.");

            return new Participant(null, name, nik, nik, request.ParticipantBirthDate, phone, null);
        }

        /// <summary><c>VAL-MCU-21</c>: perusahaan aktif dan kerja samanya masih berlaku pada tanggal pemeriksaan.</summary>
        private async Task<Failure?> CheckCompanyAsync(Guid companyId, DateOnly examinationDate, CancellationToken cancellationToken)
        {
            var company = await _dbContext.Set<MstCompanyGuarantor>().AsNoTracking()
                .Where(x => x.Id == companyId && !x.IsDelete)
                .Select(x => new { x.CompanyGuarantorName, x.IsActive, x.ContractEndDate })
                .SingleOrDefaultAsync(cancellationToken);

            if (company == null)
                return new Failure(E.InvalidRequest, "Perusahaan penjamin tidak ditemukan.");

            if (!company.IsActive)
                return new Failure(E.Val21, $"Kerja sama {company.CompanyGuarantorName} tidak aktif.");

            if (company.ContractEndDate.HasValue)
            {
                var end = DateOnly.FromDateTime(company.ContractEndDate.Value);
                if (end < examinationDate)
                    return new Failure(E.Val21, $"Kerja sama {company.CompanyGuarantorName} berakhir {McuScheduleService.FormatDate(end)}.");
            }

            return null;
        }

        /// <summary>
        /// <c>VAL-MCU-18</c>: nomor order aktif (<i>Terjadwal</i>/<i>Hadir</i>/<i>Dalam Pemeriksaan</i>) milik
        /// peserta yang sama — menurut pasien atau NIK — pada tanggal yang sama.
        /// </summary>
        private Task<string?> FindDuplicateAsync(
            Guid? patientId,
            string? nik,
            DateOnly date,
            Guid? excludeOrderId,
            CancellationToken cancellationToken) =>
            _dbContext.McuOrders.AsNoTracking()
                .Where(x => !x.IsDelete
                    && x.ExaminationDate == date
                    && ActiveStatuses.Contains(x.OrderStatus)
                    && x.Id != excludeOrderId
                    && ((patientId != null && x.PatientId == patientId)
                        || (nik != null && (x.ParticipantNik == nik || (x.Patient != null && x.Patient.IdentityNumber == nik)))))
                .OrderBy(x => x.OrderNumber)
                .Select(x => x.OrderNumber)
                .FirstOrDefaultAsync(cancellationToken);

        /// <summary>
        /// Mengantrekan dua booking untuk peserta dan tanggal yang sama, supaya keduanya tidak sama-sama
        /// lolos pemeriksaan order ganda. Kunci berdasar NIK bila ada — pasien dan identitas sementara ber-NIK
        /// sama memakai kunci yang sama.
        /// </summary>
        private async Task LockParticipantAsync(Guid? patientId, string? nik, DateOnly date, CancellationToken cancellationToken)
        {
            if (!_dbContext.Database.IsNpgsql())
                return;

            var identity = nik ?? patientId?.ToString("N") ?? string.Empty;
            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [$"MCU_PARTICIPANT_{date:yyyyMMdd}_{identity}"],
                cancellationToken);
        }

        // =====================================================================
        // Penolong
        // =====================================================================

        private sealed record PackageComponentSnapshot(
            Guid Id,
            McuComponentType ComponentType,
            Guid? ProcedureId,
            Guid? RadModalityId,
            Guid? ExaminationTypeId,
            string Name,
            decimal RefundAmount,
            decimal MedicalFeeAmount,
            DateTime CreateDateTime);

        /// <summary>
        /// Komponen paket yang akan dipotret ke order. Urutannya dibuat pasti: jenis, urutan dibuat di
        /// paket, lalu nama.
        /// </summary>
        private async Task<List<PackageComponentSnapshot>> LoadPackageComponentsAsync(Guid packageId, CancellationToken cancellationToken) =>
            (await _dbContext.McuPackageComponents.AsNoTracking()
                .Where(x => x.PackageId == packageId && !x.IsDelete)
                .Select(x => new PackageComponentSnapshot(
                    x.Id,
                    x.ComponentType,
                    x.ProcedureId,
                    x.RadModalityId,
                    x.ExaminationTypeId,
                    x.Procedure != null ? x.Procedure.ProcedureName
                        : x.ExaminationType != null ? x.ExaminationType.ExaminationTypeName
                        : string.Empty,
                    x.RefundAmount,
                    x.MedicalFeeAmount,
                    x.CreateDateTime))
                .ToListAsync(cancellationToken))
            .OrderBy(x => x.ComponentType)
            .ThenBy(x => x.CreateDateTime)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        /// <summary>Potret komponen paket, semuanya <i>Menunggu</i>; cap waktu berurutan menjaga urutan tampil.</summary>
        private void AddOrderComponents(
            Guid orderId,
            List<PackageComponentSnapshot> components,
            DateOnly scheduledDate,
            Guid actorUserId,
            DateTime now)
        {
            for (var index = 0; index < components.Count; index++)
            {
                var component = components[index];
                _dbContext.McuOrderComponents.Add(new McuOrderComponent
                {
                    McuOrderId = orderId,
                    PackageComponentId = component.Id,
                    ComponentType = component.ComponentType,
                    ProcedureId = component.ProcedureId,
                    RadModalityId = component.RadModalityId,
                    ExaminationTypeId = component.ExaminationTypeId,
                    ComponentNameSnapshot = component.Name,
                    IsPackageCovered = true,
                    RefundAmountSnapshot = component.RefundAmount,
                    MedicalFeeAmountSnapshot = component.MedicalFeeAmount,
                    ComponentStatus = McuOrderComponentStatus.Pending,
                    ScheduledDate = scheduledDate,
                    CreateDateTime = now.AddTicks(index),
                    CreateBy = actorUserId
                });
            }
        }

        private void AddHistory(Guid orderId, McuOrderStatus? from, McuOrderStatus to, string? reason, Guid actorUserId, DateTime now) =>
            _dbContext.McuOrderStatusHistories.Add(new McuOrderStatusHistory
            {
                McuOrderId = orderId,
                FromStatus = from,
                ToStatus = to,
                ChangedAt = now,
                ChangedByUserId = actorUserId,
                Reason = reason,
                CreateDateTime = now,
                CreateBy = actorUserId
            });

        private static McuResult<McuOrderDetailResponse> DuplicateConflict(DateOnly date, string orderNumber) =>
            McuResult<McuOrderDetailResponse>.Conflict(
                E.Val18, $"Peserta ini sudah terjadwal MCU pada {McuScheduleService.FormatDate(date)} ({orderNumber}).");

        private static McuResult<McuOrderDetailResponse> VersionConflict() =>
            McuResult<McuOrderDetailResponse>.Conflict(E.Val46, "Data order sudah diubah petugas lain. Muat ulang lalu coba lagi.");

        private static McuResult<McuOrderDetailResponse> Fail(Failure failure) =>
            McuResult<McuOrderDetailResponse>.Invalid(failure.Code, failure.Message);

        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? NormalizeNik(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
