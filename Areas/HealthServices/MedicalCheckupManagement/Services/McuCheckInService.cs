using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;
using E = QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services.McuErrorCodes;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services
{
    /// <summary>
    /// Check-in admisi MCU (<c>BE-MCU-013</c>, <c>MCU-API-v1</c> grup Mcu Check In, <c>VAL-MCU-24</c>..<c>30</c>,
    /// <c>INT-MCU-01</c>..<c>03</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Satu transaksi lintas tiga modul</b> (<c>02-backend-architecture.md</c> §2.1): tautkan pasien →
    /// kunjungan <c>MedicalCheckup</c> lewat <c>EncounterIntakeService</c> → baris paket lewat
    /// <c>BillingInvoiceService.AddCatalogChargeAsync</c> → dokter MCU, penandaan persyaratan, order
    /// <i>Hadir</i>, riwayat. Kedua service mitra ikut transaksi yang dibuka di sini; bila satu langkah
    /// gagal, seluruhnya batal — order tetap <i>Terjadwal</i>, tanpa kunjungan maupun baris tagihan yatim.
    /// Notifikasi antrean Registrasi baru dikirim sesudah commit.
    /// </para>
    /// <para>
    /// <b>Contoh.</b> Budi check-in 27-10-2026 → order yang <b>sama</b> menjadi <i>Hadir</i>, satu kunjungan
    /// MCU berantrean skrining, satu baris Paket Basic Rp 350.000, tanpa biaya administrasi. Siti (PT
    /// Contoh) check-in 27-10 saat harga perusahaan Rp 300.000 berlaku → baris Rp 300.000; peserta yang
    /// check-in 5 Januari sesudah harga itu berakhir → Rp 350.000. Bila tarif paket dinonaktifkan sebelum
    /// check-in → <c>409</c> <i>"Tarif paket tidak aktif. Hubungi admin MCU."</i>, order tetap <i>Terjadwal</i>.
    /// </para>
    /// <para>
    /// <b>Kirim ulang aman</b> (<c>VAL-MCU-30</c>). Order yang sudah punya kunjungan langsung
    /// mengembalikan hasil check-in yang sama. Di bawahnya, kunjungan memakai kunci idempotensi
    /// <c>MCU-CHECKIN:{orderId}</c> dan baris paket memakai kunci idempotensi Billing = <c>McuOrder.Id</c>.
    /// </para>
    /// <para>
    /// <b>MCU tidak membuat pasien.</b> Peserta baru didaftarkan lebih dulu lewat Patient Management
    /// dengan isian awal dari identitas sementara, lalu check-in memakai pasien itu.
    /// </para>
    /// </remarks>
    public class McuCheckInService
    {
        public const string IdempotencyPrefix = "MCU-CHECKIN:";

        private readonly ApplicationDbContext _dbContext;
        private readonly EncounterIntakeService _intakeService;
        private readonly BillingInvoiceService _billingService;
        private readonly McuOrderQueryService _queryService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AccessPermissionService _accessPermissionService;
        private readonly LabOrderService _labOrderService;
        private readonly RadOrderService _radOrderService;

        public McuCheckInService(
            ApplicationDbContext dbContext,
            EncounterIntakeService intakeService,
            BillingInvoiceService billingService,
            McuOrderQueryService queryService,
            IHttpContextAccessor httpContextAccessor,
            AccessPermissionService accessPermissionService,
            LabOrderService labOrderService,
            RadOrderService radOrderService)
        {
            _labOrderService = labOrderService;
            _radOrderService = radOrderService;
            _dbContext = dbContext;
            _intakeService = intakeService;
            _billingService = billingService;
            _queryService = queryService;
            _httpContextAccessor = httpContextAccessor;
            _accessPermissionService = accessPermissionService;
        }

        /// <summary>
        /// Seam kewenangan <c>McuOrder : CheckIn</c> untuk <i>walk-in</i> (<c>BE-MCU-014</c>). Endpoint
        /// <i>walk-in</i> hanya dapat memasang satu <c>[AccessPermission]</c> (<c>McuOrder : Create</c>),
        /// sedangkan kontrak meminta keduanya. <c>protected virtual</c> semata agar uji dapat menggantinya,
        /// sama dengan <c>EncounterIntakeService.HasRegistrationAuthorityAsync</c>.
        /// </summary>
        protected virtual async Task<bool> HasCheckInAuthorityAsync(CancellationToken cancellationToken)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return false;

            return await _accessPermissionService.HasAccessAsync(
                user,
                Constants.MedicalCheckupPermissions.Order.Resource,
                Constants.MedicalCheckupPermissions.Order.CheckIn);
        }

        /// <summary>Notifikasi antrean Registrasi yang tertunda; dipanggil pemanggil sesudah transaksinya commit.</summary>
        public Task PublishDeferredAsync() => _intakeService.PublishDeferredAsync();

        // =====================================================================
        // Pencocokan pasien — INT-MCU-01
        // =====================================================================

        public async Task<McuResult<McuPatientMatchResponse>> GetPatientMatchesAsync(Guid orderId, CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.AsNoTracking()
                .Where(x => x.Id == orderId && !x.IsDelete)
                .Select(x => new { x.Id, x.PatientId, x.ParticipantName, x.ParticipantNik, x.ParticipantBirthDate, x.ParticipantPhone })
                .SingleOrDefaultAsync(cancellationToken);

            if (order == null)
                return McuResult<McuPatientMatchResponse>.NotFound("Order MCU tidak ditemukan.");

            var response = new McuPatientMatchResponse
            {
                OrderId = order.Id,
                ParticipantName = order.ParticipantName,
                ParticipantNik = order.ParticipantNik,
                ParticipantBirthDate = order.ParticipantBirthDate,
                ParticipantPhone = order.ParticipantPhone
            };

            if (order.PatientId.HasValue)
            {
                response.Candidates = await CandidatesAsync(x => x.Id == order.PatientId.Value, cancellationToken);
                return McuResult<McuPatientMatchResponse>.Ok(WithStatus(response, McuPatientMatchStatuses.Linked));
            }

            var nik = order.ParticipantNik;
            response.Candidates = nik == null
                ? []
                : await CandidatesAsync(x => x.IdentityNumber == nik, cancellationToken);

            var status = response.Candidates.Count == 0
                ? McuPatientMatchStatuses.NoMatch
                : response.Candidates.Any(x => x.BirthDate == order.ParticipantBirthDate)
                    ? McuPatientMatchStatuses.FullMatch
                    : McuPatientMatchStatuses.NikMatchBirthDateDiffers;

            return McuResult<McuPatientMatchResponse>.Ok(WithStatus(response, status));
        }

        // =====================================================================
        // Pratinjau check-in
        // =====================================================================

        public async Task<McuResult<McuCheckInPreviewResponse>> GetPreviewAsync(Guid orderId, CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.AsNoTracking()
                .Where(x => x.Id == orderId && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNumber,
                    x.Version,
                    x.OrderStatus,
                    x.ExaminationDate,
                    x.PatientId,
                    x.PackageId,
                    x.PackageNameSnapshot,
                    x.CompanyGuarantorId,
                    CompanyName = x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : null,
                    x.EncounterId
                })
                .SingleOrDefaultAsync(cancellationToken);

            if (order == null)
                return McuResult<McuCheckInPreviewResponse>.NotFound("Order MCU tidak ditemukan.");

            var today = Today();
            var tariff = await ResolveTariffAsync(order.PackageId, order.CompanyGuarantorId, today, cancellationToken);

            var duty = await _dbContext.McuDailySchedules.AsNoTracking()
                .Where(x => x.ScheduleDate == order.ExaminationDate && !x.IsDelete && x.DutyMcuDoctorId != null)
                .Select(x => new { x.DutyMcuDoctorId, Name = x.DutyMcuDoctor != null ? x.DutyMcuDoctor.FullName : null })
                .FirstOrDefaultAsync(cancellationToken);

            var preview = new McuCheckInPreviewResponse
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Version = order.Version,
                OrderStatus = order.OrderStatus,
                OrderStatusLabel = McuLabels.OrderStatus(order.OrderStatus),
                ExaminationDate = order.ExaminationDate,
                CheckInBlockedReason = order.EncounterId != null
                    ? "Order ini sudah check-in."
                    : StatusBlockMessage(order.OrderStatus) ?? DateBlockMessage(order.ExaminationDate, today),
                PatientId = order.PatientId,
                PackageName = order.PackageNameSnapshot,
                CompanyGuarantorId = order.CompanyGuarantorId,
                CompanyGuarantorName = order.CompanyName,
                TariffId = tariff.TariffId,
                TariffName = tariff.TariffName,
                Price = tariff.Price,
                PriceSource = tariff.Source,
                PriceSourceLabel = tariff.Source == McuPackageService.PriceSourceCompany ? "Harga perusahaan" : "Harga umum",
                CompanyPriceStartDate = tariff.CompanyStart,
                CompanyPriceEndDate = tariff.CompanyEnd,
                IsTariffActive = tariff.IsActive,
                DefaultMcuDoctorId = duty?.DutyMcuDoctorId,
                DefaultMcuDoctorName = duty?.Name,
                Requirements = await RequirementsAsync(order.Id, cancellationToken)
            };

            if (order.CompanyGuarantorId.HasValue && order.PatientId.HasValue)
            {
                preview.CompanyCards = (await CompanyCardsAsync(order.PatientId.Value, order.CompanyGuarantorId.Value, today, cancellationToken))
                    .Select(x => new McuCompanyCardResponse
                    {
                        PatientCompanyGuarantorId = x.Id,
                        EmployeeNumber = x.EmployeeNumber,
                        EmployeeName = x.EmployeeName,
                        IsPrimary = x.IsPrimary
                    })
                    .ToList();
            }

            return McuResult<McuCheckInPreviewResponse>.Ok(preview);
        }

        // =====================================================================
        // Check-in
        // =====================================================================

        public Task<McuResult<McuOrderDetailResponse>> CheckInAsync(
            Guid orderId,
            McuCheckInRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken) =>
            CheckInCoreAsync(orderId, request, actorUserId, walkIn: false, cancellationToken);

        /// <summary>
        /// Pemeriksaan <i>walk-in</i> yang dapat dilakukan sebelum order ada — kewenangan check-in, dokter,
        /// persyaratan paket, tarif, kartu penjamin — supaya penolakan tidak menghanguskan nomor order.
        /// Kepastiannya tetap milik check-in di dalam transaksi <i>walk-in</i>.
        /// </summary>
        public async Task<McuResult<McuOrderDetailResponse>?> ValidateWalkInAsync(
            Guid packageId,
            Guid? companyGuarantorId,
            Guid patientId,
            McuCheckInRequest request,
            CancellationToken cancellationToken)
        {
            if (!await HasCheckInAuthorityAsync(cancellationToken))
            {
                return McuResult<McuOrderDetailResponse>.Forbidden(
                    E.InvalidRequest, "Anda tidak memiliki hak check-in MCU yang dibutuhkan pendaftaran walk-in.");
            }

            if (!request.McuDoctorId.HasValue)
                return McuResult<McuOrderDetailResponse>.Invalid(E.Val26, "Pilih dokter MCU pemeriksa.");

            var doctorActive = await _dbContext.Set<MstDoctor>().AsNoTracking()
                .AnyAsync(x => x.Id == request.McuDoctorId.Value && x.IsActive && !x.IsDelete, cancellationToken);
            if (!doctorActive)
                return McuResult<McuOrderDetailResponse>.Invalid(E.Val26, "Dokter MCU tidak ditemukan atau tidak aktif.");

            var today = Today();
            var outcomes = await ValidateOutcomesAsync(
                await PackageRequirementsAsync(packageId, cancellationToken), request.RequirementOutcomes ?? [], today, cancellationToken);
            if (outcomes.Error != null)
                return outcomes.Error;

            var tariff = await ResolveTariffAsync(packageId, companyGuarantorId, today, cancellationToken);
            if (!tariff.IsActive || !tariff.TariffId.HasValue)
                return McuResult<McuOrderDetailResponse>.Conflict(E.Val29, "Tarif paket tidak aktif. Hubungi admin MCU.");

            if (companyGuarantorId.HasValue)
            {
                var cards = await CompanyCardsAsync(patientId, companyGuarantorId.Value, today, cancellationToken);
                var hasCard = request.PatientCompanyGuarantorId.HasValue
                    ? cards.Any(x => x.Id == request.PatientCompanyGuarantorId.Value)
                    : cards.Count > 0;
                if (!hasCard)
                    return await CompanyCardMissingAsync(companyGuarantorId.Value, cancellationToken);
            }

            return null;
        }

        /// <summary>
        /// Inti check-in. Membuka transaksinya sendiri, kecuali sudah ada transaksi pemanggil
        /// (<i>walk-in</i>, <c>BE-MCU-014</c>) — maka ia ikut di dalamnya, tidak commit, tidak rollback,
        /// dan tidak mengirim notifikasi tertunda; ketiganya milik pemanggil.
        /// </summary>
        internal async Task<McuResult<McuOrderDetailResponse>> CheckInCoreAsync(
            Guid orderId,
            McuCheckInRequest request,
            Guid actorUserId,
            bool walkIn,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            if (order == null)
                return McuResult<McuOrderDetailResponse>.NotFound("Order MCU tidak ditemukan.");

            // VAL-MCU-30: kirim ulang mengembalikan hasil check-in yang sama.
            if (order.EncounterId.HasValue)
                return await DetailAsync(order.Id, cancellationToken);

            if (order.Version != request.Version)
                return VersionConflict();

            var statusBlock = StatusBlockMessage(order.OrderStatus);
            if (statusBlock != null)
                return McuResult<McuOrderDetailResponse>.Conflict(E.InvalidTransition, statusBlock);

            var today = Today();
            var dateBlock = DateBlockMessage(order.ExaminationDate, today);
            if (dateBlock != null)
                return McuResult<McuOrderDetailResponse>.Conflict(E.Val24, dateBlock);

            // VAL-MCU-26
            if (!request.McuDoctorId.HasValue)
                return McuResult<McuOrderDetailResponse>.Invalid(E.Val26, "Pilih dokter MCU pemeriksa.");

            var doctorName = await _dbContext.Set<MstDoctor>().AsNoTracking()
                .Where(x => x.Id == request.McuDoctorId.Value && x.IsActive && !x.IsDelete)
                .Select(x => x.FullName)
                .SingleOrDefaultAsync(cancellationToken);

            if (doctorName == null)
                return McuResult<McuOrderDetailResponse>.Invalid(E.Val26, "Dokter MCU tidak ditemukan atau tidak aktif.");

            // INT-MCU-01 — VAL-MCU-25
            var match = await ResolvePatientAsync(order, request, actorUserId, cancellationToken);
            if (match.Error != null)
                return match.Error;

            // VAL-MCU-27, 28 dan jadwal ulang komponen
            var requirements = await RequirementsAsync(order.Id, cancellationToken);
            var outcomes = await ValidateOutcomesAsync(requirements, request.RequirementOutcomes ?? [], today, cancellationToken);
            if (outcomes.Error != null)
                return outcomes.Error;

            // VAL-MCU-29 — tarif yang berlaku pada tanggal check-in
            var tariff = await ResolveTariffAsync(order.PackageId, order.CompanyGuarantorId, today, cancellationToken);
            if (!tariff.IsActive || !tariff.TariffId.HasValue)
                return McuResult<McuOrderDetailResponse>.Conflict(E.Val29, "Tarif paket tidak aktif. Hubungi admin MCU.");

            // Penjamin perusahaan — INT-MCU-02
            Guid? companyCardId = null;
            if (order.CompanyGuarantorId.HasValue)
            {
                var cards = await CompanyCardsAsync(match.PatientId, order.CompanyGuarantorId.Value, today, cancellationToken);
                var card = request.PatientCompanyGuarantorId.HasValue
                    ? cards.FirstOrDefault(x => x.Id == request.PatientCompanyGuarantorId.Value)
                    : cards.OrderByDescending(x => x.IsPrimary).FirstOrDefault();

                if (card == null)
                    return await CompanyCardMissingAsync(order.CompanyGuarantorId.Value, cancellationToken);

                companyCardId = card.Id;
            }

            var clinic = await _dbContext.Set<MstClinic>().AsNoTracking()
                .Where(x => x.ClinicType == ClinicType.MedicalCheckup && x.IsActive && !x.IsDelete && x.IsAvailableForRegistration)
                .OrderBy(x => x.ClinicCode)
                .Select(x => new { x.Id, x.ServiceUnitId })
                .FirstOrDefaultAsync(cancellationToken);

            if (clinic == null)
            {
                return McuResult<McuOrderDetailResponse>.Conflict(
                    E.ClinicMissing, "Klinik Medical Check-Up belum tersedia di master data. Hubungi admin.");
            }

            var now = DateTime.UtcNow;
            var history = new McuOrderStatusHistory
            {
                McuOrderId = order.Id,
                // Walk-in lahir langsung Hadir (MCU-STATE-v1: — → Hadir).
                FromStatus = walkIn ? null : McuOrderStatus.Scheduled,
                ToStatus = McuOrderStatus.CheckedIn,
                ChangedAt = now,
                ChangedByUserId = actorUserId,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            var ownsTransaction = _dbContext.Database.CurrentTransaction == null;
            await using var transaction = ownsTransaction
                ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                : null;

            try
            {
                // INT-MCU-02 — kunjungan MedicalCheckup, ikut transaksi ini.
                var encounter = await _intakeService.RegisterAsync(new EncounterIntakeRequest
                {
                    PatientId = match.PatientId,
                    ServiceUnitId = clinic.ServiceUnitId,
                    EncounterType = EncounterType.MedicalCheckup,
                    ClinicId = clinic.Id,
                    DoctorId = request.McuDoctorId.Value,
                    IdempotencyKey = IdempotencyPrefix + order.Id,
                    PaymentType = companyCardId.HasValue ? EncounterPaymentType.CompanyGuarantor : EncounterPaymentType.Cash,
                    PatientCompanyGuarantorId = companyCardId
                }, cancellationToken);

                // INT-MCU-03 — satu baris paket, ikut transaksi ini.
                await _billingService.AddCatalogChargeAsync(
                    new AddCatalogChargeRequest
                    {
                        EncounterId = encounter.EncounterId,
                        TariffId = tariff.TariffId.Value,
                        Quantity = 1,
                        CorrelationId = order.Id,
                        CausationId = history.Id
                    },
                    order.Id,
                    actorUserId,
                    cancellationToken);

                var invoiceItemId = await _dbContext.BilChargeReceipts.AsNoTracking()
                    .Where(x => x.IdempotencyKey == order.Id)
                    .Select(x => (Guid?)x.InvoiceItemId)
                    .FirstOrDefaultAsync(cancellationToken);

                order.PatientId = match.PatientId;
                order.PatientMatchMethod = match.Method;
                order.PatientMatchConfirmedByUserId = match.ConfirmedByUserId ?? order.PatientMatchConfirmedByUserId;
                order.EncounterId = encounter.EncounterId;
                order.CheckInChannel = McuCheckInChannel.Admission;
                order.CheckedInAt = now;
                order.CheckedInByUserId = actorUserId;
                order.McuDoctorId = request.McuDoctorId.Value;
                order.AppliedTariffId = tariff.TariffId.Value;
                order.PackageInvoiceItemId = invoiceItemId;
                order.OrderStatus = McuOrderStatus.CheckedIn;
                order.Version++;
                order.UpdateDateTime = now;
                order.UpdateBy = actorUserId;

                _dbContext.McuOrderStatusHistories.Add(history);
                _dbContext.McuOrderDoctorAssignments.Add(new McuOrderDoctorAssignment
                {
                    McuOrderId = order.Id,
                    DoctorId = request.McuDoctorId.Value,
                    AssignedAt = now,
                    AssignedByUserId = actorUserId,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                await ApplyOutcomesAsync(order.Id, outcomes.Value!, actorUserId, now, cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);

                if (transaction == null)
                    return McuResult<McuOrderDetailResponse>.Ok(new McuOrderDetailResponse { Id = order.Id });

                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException) when (transaction != null)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                _dbContext.ChangeTracker.Clear();

                // Kirim ulang serentak: pemenangnya sudah check-in — kembalikan hasilnya (VAL-MCU-30).
                var winner = await _dbContext.McuOrders.AsNoTracking()
                    .AnyAsync(x => x.Id == orderId && x.EncounterId != null, cancellationToken);
                return winner ? await DetailAsync(orderId, cancellationToken) : VersionConflict();
            }
            catch (Exception exception) when (PartnerFailure(exception) is { } failure)
            {
                // Di dalam transaksi pemanggil, rollback milik pemanggil.
                if (transaction != null)
                    await transaction.RollbackAsync(CancellationToken.None);
                _dbContext.ChangeTracker.Clear();
                return failure;
            }

            await _intakeService.PublishDeferredAsync();

            return await DetailAsync(order.Id, cancellationToken);
        }

        // =====================================================================
        // Pasien
        // =====================================================================

        private sealed record PatientMatch(
            Guid PatientId,
            McuPatientMatchMethod? Method,
            Guid? ConfirmedByUserId,
            McuResult<McuOrderDetailResponse>? Error);

        private async Task<PatientMatch> ResolvePatientAsync(
            McuOrder order,
            McuCheckInRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            static PatientMatch Reject(McuResult<McuOrderDetailResponse> error) => new(Guid.Empty, null, null, error);

            if (order.PatientId.HasValue)
            {
                if (request.PatientId.HasValue && request.PatientId != order.PatientId)
                {
                    return Reject(McuResult<McuOrderDetailResponse>.Invalid(
                        E.InvalidRequest, "Order ini sudah tertaut ke pasien lain sejak booking."));
                }

                return new PatientMatch(order.PatientId.Value, order.PatientMatchMethod ?? McuPatientMatchMethod.ExistingAtBooking, null, null);
            }

            if (!request.PatientId.HasValue)
            {
                return Reject(McuResult<McuOrderDetailResponse>.Invalid(
                    E.InvalidRequest, "Pilih pasien yang cocok atau daftarkan pasien baru lebih dulu."));
            }

            var patient = await _dbContext.Set<MstPatient>().AsNoTracking()
                .Where(x => x.Id == request.PatientId.Value && !x.IsDelete && x.IsActive)
                .Select(x => new { x.Id, x.MedicalRecordNumber, x.IdentityNumber, x.BirthDate, x.CreateDateTime })
                .SingleOrDefaultAsync(cancellationToken);

            if (patient == null)
                return Reject(McuResult<McuOrderDetailResponse>.Invalid(E.InvalidRequest, "Pasien tidak ditemukan atau tidak aktif."));

            var sameNik = patient.IdentityNumber?.Trim() == order.ParticipantNik;
            var sameBirthDate = patient.BirthDate.HasValue
                && DateOnly.FromDateTime(patient.BirthDate.Value) == order.ParticipantBirthDate;

            if (sameNik && sameBirthDate)
            {
                // Pasien yang terdaftar sesudah booking adalah pasien baru untuk peserta ini (MCU-DEC-033).
                var method = patient.CreateDateTime >= order.CreateDateTime
                    ? McuPatientMatchMethod.NewPatientRegistered
                    : McuPatientMatchMethod.NikAndBirthDateMatch;
                return new PatientMatch(patient.Id, method, null, null);
            }

            if (!request.MatchConfirmation)
            {
                var message = sameNik
                    ? $"NIK cocok dengan pasien RM {patient.MedicalRecordNumber} tetapi tanggal lahir berbeda. Periksa KTP lalu konfirmasi."
                    : $"NIK pasien RM {patient.MedicalRecordNumber} berbeda dengan NIK peserta. Periksa KTP lalu konfirmasi.";
                return Reject(McuResult<McuOrderDetailResponse>.Conflict(E.Val25, message));
            }

            return new PatientMatch(patient.Id, McuPatientMatchMethod.ConfirmedByStaff, actorUserId, null);
        }

        private async Task<List<McuPatientCandidateResponse>> CandidatesAsync(
            System.Linq.Expressions.Expression<Func<MstPatient, bool>> predicate,
            CancellationToken cancellationToken) =>
            (await _dbContext.Set<MstPatient>().AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .Where(predicate)
                .OrderBy(x => x.MedicalRecordNumber)
                .Select(x => new { x.Id, x.MedicalRecordNumber, x.FullName, x.IdentityNumber, x.BirthDate })
                .ToListAsync(cancellationToken))
            .Select(x => new McuPatientCandidateResponse
            {
                PatientId = x.Id,
                MedicalRecordNumber = x.MedicalRecordNumber,
                FullName = x.FullName,
                IdentityNumber = x.IdentityNumber,
                BirthDate = x.BirthDate.HasValue ? DateOnly.FromDateTime(x.BirthDate.Value) : null
            })
            .ToList();

        private static McuPatientMatchResponse WithStatus(McuPatientMatchResponse response, string status)
        {
            response.MatchStatus = status;
            response.MatchStatusLabel = status switch
            {
                McuPatientMatchStatuses.Linked => "Sudah tertaut ke pasien",
                McuPatientMatchStatuses.FullMatch => "Cocok NIK dan tanggal lahir",
                McuPatientMatchStatuses.NikMatchBirthDateDiffers => "NIK sama, tanggal lahir berbeda — periksa KTP",
                _ => "Belum terdaftar — daftarkan pasien baru"
            };
            return response;
        }

        // =====================================================================
        // Persyaratan
        // =====================================================================

        private sealed record ValidOutcome(
            McuCheckInRequirementResponse Requirement,
            McuRequirementOutcome Outcome,
            string? Note,
            DateOnly? RescheduledDate);

        private sealed record OutcomeValidation(List<ValidOutcome>? Value, McuResult<McuOrderDetailResponse>? Error);

        private async Task<List<McuCheckInRequirementResponse>> RequirementsAsync(Guid orderId, CancellationToken cancellationToken)
        {
            var components = await _dbContext.McuOrderComponents.AsNoTracking()
                .Where(x => x.McuOrderId == orderId && !x.IsDelete
                    && x.ComponentStatus != McuOrderComponentStatus.Cancelled
                    && x.PackageComponentId != null)
                .OrderBy(x => x.ComponentType)
                .ThenBy(x => x.CreateDateTime)
                .Select(x => new { x.Id, x.PackageComponentId, x.ComponentNameSnapshot })
                .ToListAsync(cancellationToken);

            var packageComponentIds = components.Select(x => x.PackageComponentId!.Value).ToList();
            var requirements = await _dbContext.McuPackageComponentRequirements.AsNoTracking()
                .Where(x => packageComponentIds.Contains(x.PackageComponentId) && !x.IsDelete)
                .OrderBy(x => x.CreateDateTime)
                .Select(x => new { x.Id, x.PackageComponentId, x.RequirementText, x.IsConfirmationRequired })
                .ToListAsync(cancellationToken);

            return components
                .SelectMany(c => requirements
                    .Where(r => r.PackageComponentId == c.PackageComponentId)
                    .Select(r => new McuCheckInRequirementResponse
                    {
                        PackageComponentRequirementId = r.Id,
                        McuOrderComponentId = c.Id,
                        ComponentName = c.ComponentNameSnapshot,
                        RequirementText = r.RequirementText,
                        IsConfirmationRequired = r.IsConfirmationRequired
                    }))
                .ToList();
        }

        private async Task<OutcomeValidation> ValidateOutcomesAsync(
            List<McuCheckInRequirementResponse> requirements,
            List<McuRequirementOutcomeRequest> requested,
            DateOnly today,
            CancellationToken cancellationToken)
        {
            static OutcomeValidation Reject(McuResult<McuOrderDetailResponse> error) => new(null, error);

            var byId = requested
                .GroupBy(x => x.PackageComponentRequirementId)
                .ToDictionary(x => x.Key, x => x.Last());

            var valid = new List<ValidOutcome>();

            foreach (var requirement in requirements)
            {
                if (!byId.TryGetValue(requirement.PackageComponentRequirementId, out var outcome))
                {
                    if (requirement.IsConfirmationRequired)
                    {
                        return Reject(McuResult<McuOrderDetailResponse>.Invalid(
                            E.Val27, $"Tandai persyaratan: {requirement.RequirementText} ({requirement.ComponentName})."));
                    }

                    continue;
                }

                if (!Enum.IsDefined(outcome.Outcome))
                    return Reject(McuResult<McuOrderDetailResponse>.Invalid(E.InvalidRequest, "Hasil penandaan persyaratan tidak dikenal."));

                var note = string.IsNullOrWhiteSpace(outcome.Note) ? null : outcome.Note.Trim();
                if (note?.Length > 500)
                    return Reject(McuResult<McuOrderDetailResponse>.Invalid(E.InvalidRequest, "Catatan persyaratan paling banyak 500 karakter."));

                if (outcome.Outcome == McuRequirementOutcome.ProceedWithNote && note == null)
                {
                    return Reject(McuResult<McuOrderDetailResponse>.Invalid(
                        E.Val28, "Tulis catatan untuk persyaratan yang tidak terpenuhi."));
                }

                DateOnly? rescheduled = null;
                if (outcome.Outcome == McuRequirementOutcome.ComponentRescheduled)
                {
                    if (!outcome.RescheduledDate.HasValue || outcome.RescheduledDate.Value <= today)
                    {
                        return Reject(McuResult<McuOrderDetailResponse>.Invalid(
                            E.InvalidRequest, $"Pilih tanggal susulan sesudah hari ini untuk {requirement.ComponentName}."));
                    }

                    // Tanggal susulan wajib buka, tanpa memakai kuota (MCU-STATE-v1 bagian 2.1).
                    var open = await _dbContext.McuDailySchedules.AsNoTracking()
                        .AnyAsync(x => x.ScheduleDate == outcome.RescheduledDate.Value && !x.IsDelete && x.IsOpen, cancellationToken);
                    if (!open)
                    {
                        return Reject(McuResult<McuOrderDetailResponse>.Invalid(
                            E.Val10, $"MCU tutup pada {McuScheduleService.FormatDate(outcome.RescheduledDate.Value)}."));
                    }

                    rescheduled = outcome.RescheduledDate;
                }

                valid.Add(new ValidOutcome(requirement, outcome.Outcome, note, rescheduled));
            }

            return new OutcomeValidation(valid, null);
        }

        private async Task ApplyOutcomesAsync(
            Guid orderId,
            List<ValidOutcome> outcomes,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            foreach (var outcome in outcomes)
            {
                _dbContext.McuOrderRequirementChecks.Add(new McuOrderRequirementCheck
                {
                    McuOrderId = orderId,
                    McuOrderComponentId = outcome.Requirement.McuOrderComponentId,
                    RequirementTextSnapshot = outcome.Requirement.RequirementText,
                    Outcome = outcome.Outcome,
                    Note = outcome.Note,
                    CheckedByUserId = actorUserId,
                    CheckedAt = now,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
            }

            // Komponen yang persyaratannya tidak terpenuhi dijadwal ulang; komponen lain tetap berjalan.
            var rescheduled = outcomes
                .Where(x => x.RescheduledDate.HasValue)
                .GroupBy(x => x.Requirement.McuOrderComponentId)
                .ToDictionary(x => x.Key, x => x.Max(o => o.RescheduledDate!.Value));

            if (rescheduled.Count == 0)
                return;

            var ids = rescheduled.Keys.ToList();
            var components = await _dbContext.McuOrderComponents
                .Where(x => ids.Contains(x.Id))
                .ToListAsync(cancellationToken);

            foreach (var component in components)
            {
                component.ComponentStatus = McuOrderComponentStatus.Rescheduled;
                component.ScheduledDate = rescheduled[component.Id];
                component.UpdateDateTime = now;
                component.UpdateBy = actorUserId;
            }
        }

        // =====================================================================
        // Tarif dan penjamin
        // =====================================================================

        private sealed record TariffChoice(
            Guid? TariffId,
            string? TariffName,
            decimal? Price,
            string Source,
            DateOnly? CompanyStart,
            DateOnly? CompanyEnd,
            bool IsActive);

        /// <summary>
        /// Harga perusahaan yang berlaku pada <paramref name="date"/>, selain itu tarif umum paket. Tarif
        /// dianggap aktif dengan aturan yang sama dengan <c>AddCatalogChargeAsync</c> Billing, sehingga
        /// <c>VAL-MCU-29</c> terdeteksi sebelum kunjungan dibuat.
        /// </summary>
        private async Task<TariffChoice> ResolveTariffAsync(
            Guid packageId,
            Guid? companyGuarantorId,
            DateOnly date,
            CancellationToken cancellationToken)
        {
            var source = McuPackageService.PriceSourceGeneral;
            Guid? tariffId = null;
            DateOnly? start = null;
            DateOnly? end = null;

            if (companyGuarantorId.HasValue)
            {
                var price = await _dbContext.McuPackageCompanyPrices.AsNoTracking()
                    .Where(x => !x.IsDelete
                        && x.PackageId == packageId
                        && x.CompanyGuarantorId == companyGuarantorId.Value
                        && x.EffectiveStartDate <= date
                        && (x.EffectiveEndDate == null || x.EffectiveEndDate >= date))
                    .Select(x => new { x.TariffId, x.EffectiveStartDate, x.EffectiveEndDate })
                    .FirstOrDefaultAsync(cancellationToken);

                if (price != null)
                {
                    source = McuPackageService.PriceSourceCompany;
                    tariffId = price.TariffId;
                    start = price.EffectiveStartDate;
                    end = price.EffectiveEndDate;
                }
            }

            tariffId ??= await _dbContext.McuPackages.AsNoTracking()
                .Where(x => x.Id == packageId)
                .Select(x => (Guid?)x.GeneralTariffId)
                .FirstOrDefaultAsync(cancellationToken);

            var tariff = tariffId == null
                ? null
                : await _dbContext.Set<MstTariff>().AsNoTracking()
                    .Where(x => x.Id == tariffId.Value)
                    .Select(x => new { x.TariffName, x.NormalPrice, x.IsActive, x.IsDelete, x.IsCancel, x.EffectiveStartDate, x.EffectiveEndDate })
                    .FirstOrDefaultAsync(cancellationToken);

            var now = DateTime.UtcNow;
            var active = tariff != null
                && tariff.IsActive && !tariff.IsDelete && !tariff.IsCancel
                && (tariff.EffectiveStartDate == null || tariff.EffectiveStartDate <= now)
                && (tariff.EffectiveEndDate == null || now < tariff.EffectiveEndDate);

            return new TariffChoice(tariffId, tariff?.TariffName, tariff?.NormalPrice, source, start, end, active);
        }

        private async Task<McuResult<McuOrderDetailResponse>> CompanyCardMissingAsync(Guid companyGuarantorId, CancellationToken cancellationToken)
        {
            var companyName = await _dbContext.Set<Administrator.MasterData.Models.MstCompanyGuarantor>().AsNoTracking()
                .Where(x => x.Id == companyGuarantorId)
                .Select(x => x.CompanyGuarantorName)
                .FirstOrDefaultAsync(cancellationToken);

            return McuResult<McuOrderDetailResponse>.Conflict(
                E.CompanyCardMissing,
                $"Pasien belum punya kartu penjamin {companyName} yang berlaku hari ini. " +
                "Lengkapi data penjamin pasien lalu ulangi check-in.");
        }

        /// <summary>Persyaratan paket sebelum order ada — untuk pemeriksaan awal <i>walk-in</i>.</summary>
        private async Task<List<McuCheckInRequirementResponse>> PackageRequirementsAsync(Guid packageId, CancellationToken cancellationToken)
        {
            var components = await _dbContext.McuPackageComponents.AsNoTracking()
                .Where(x => x.PackageId == packageId && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    Name = x.Procedure != null ? x.Procedure.ProcedureName
                        : x.ExaminationType != null ? x.ExaminationType.ExaminationTypeName
                        : string.Empty
                })
                .ToListAsync(cancellationToken);

            var ids = components.Select(x => x.Id).ToList();
            var requirements = await _dbContext.McuPackageComponentRequirements.AsNoTracking()
                .Where(x => ids.Contains(x.PackageComponentId) && !x.IsDelete)
                .OrderBy(x => x.CreateDateTime)
                .Select(x => new { x.Id, x.PackageComponentId, x.RequirementText, x.IsConfirmationRequired })
                .ToListAsync(cancellationToken);

            return requirements.Select(r => new McuCheckInRequirementResponse
            {
                PackageComponentRequirementId = r.Id,
                ComponentName = components.First(c => c.Id == r.PackageComponentId).Name,
                RequirementText = r.RequirementText,
                IsConfirmationRequired = r.IsConfirmationRequired
            }).ToList();
        }

        private async Task<List<MstPatientCompanyGuarantor>> CompanyCardsAsync(
            Guid patientId,
            Guid companyGuarantorId,
            DateOnly date,
            CancellationToken cancellationToken)
        {
            var day = date.ToDateTime(TimeOnly.MinValue);

            var cards = await _dbContext.Set<MstPatientCompanyGuarantor>().AsNoTracking()
                .Where(x => x.PatientId == patientId
                    && x.CompanyGuarantorId == companyGuarantorId
                    && !x.IsDelete && x.IsActive && x.IsEligible)
                .ToListAsync(cancellationToken);

            return cards
                .Where(x => (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= day)
                    && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= day))
                .ToList();
        }

        // =====================================================================
        // Penolong
        // =====================================================================

        private static DateOnly Today() => DateOnly.FromDateTime(AppDateTimeHelper.OperationalDate());

        /// <summary><c>MCU-STATE-v1</c> bagian 1.2 untuk check-in; <c>null</c> bila order <i>Terjadwal</i>.</summary>
        private static string? StatusBlockMessage(McuOrderStatus status) => status switch
        {
            McuOrderStatus.Scheduled => null,
            McuOrderStatus.NoShow => "Order sudah Tidak Hadir. Jadwalkan ulang lebih dulu.",
            McuOrderStatus.Cancelled => "Order sudah dibatalkan.",
            _ => $"Order berstatus {McuLabels.OrderStatus(status)} tidak dapat di-check-in."
        };

        /// <summary><c>VAL-MCU-24</c>; <c>null</c> bila tanggal pemeriksaan hari ini.</summary>
        private static string? DateBlockMessage(DateOnly examinationDate, DateOnly today) =>
            examinationDate == today
                ? null
                : $"Order ini dijadwalkan {McuScheduleService.FormatDate(examinationDate)}, bukan hari ini.";

        /// <summary>Penolakan modul mitra diteruskan dengan kode status kontrak MCU.</summary>
        private static McuResult<McuOrderDetailResponse>? PartnerFailure(Exception exception) => exception switch
        {
            EncounterIntakeValidationException e => McuResult<McuOrderDetailResponse>.Invalid(E.PartnerRejected, e.Message),
            EncounterIntakeForbiddenException e => McuResult<McuOrderDetailResponse>.Forbidden(E.PartnerRejected, e.Message),
            EncounterIntakeConflictException e => McuResult<McuOrderDetailResponse>.Conflict(E.PartnerRejected, e.Message),
            BillingInvoiceValidationException e => McuResult<McuOrderDetailResponse>.Conflict(E.PartnerRejected, e.Message),
            BillingInvoiceConflictException e => McuResult<McuOrderDetailResponse>.Conflict(E.PartnerRejected, e.Message),
            KeyNotFoundException e => McuResult<McuOrderDetailResponse>.Conflict(E.PartnerRejected, e.Message),
            _ => null
        };

        // =====================================================================
        // Ganti dokter MCU — BE-MCU-032
        // =====================================================================

        /// <summary>
        /// <c>PATCH orders/{id}/mcu-doctor</c> oleh kepala unit MCU (<c>McuOrder : ReassignDoctor</c>) dengan alasan
        /// (<c>MCU-DEC-040</c>, <c>VAL-MCU-40</c>, <c>45</c>). Dokter pengganti mewarisi seluruh tanggung jawab
        /// <c>MCU-DEC-032</c> sejak penggantian: penyusun dan pemberi approve resume, serta penanggung jawab dan
        /// penerima nilai kritis pesanan paket Lab/Rad yang masih terbuka (<c>INT-MCU-06</c>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Satu transaksi: penugasan lama ditutup, penugasan baru beralasan dicatat, order menunjuk dokter baru, lalu
        /// Lab dan Rad memindah pesanan terbukanya. Pesanan yang sudah selesai atau batal tetap atas nama dokter lama.
        /// </para>
        /// <para>
        /// <b>Contoh.</b> dr. Rina pulang pukul 14.00 sebelum resume Siti disusun → kepala unit mengganti ke dr. Hadi
        /// dengan alasan <i>dokter selesai jaga</i>; pesanan Thorax Siti yang masih berjalan kini atas nama dr. Hadi,
        /// dr. Hadi dapat approve, dan dr. Rina ditolak <c>403</c>.
        /// </para>
        /// </remarks>
        public async Task<McuResult<McuOrderDetailResponse>> ReassignDoctorAsync(
            Guid orderId,
            ReassignMcuDoctorRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            if (order == null)
                return McuResult<McuOrderDetailResponse>.NotFound("Order MCU tidak ditemukan.");

            // VAL-MCU-40
            var reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();
            if (reason == null)
                return McuResult<McuOrderDetailResponse>.Invalid(E.Val40, "Alasan wajib diisi.");
            if (reason.Length > 500)
                return McuResult<McuOrderDetailResponse>.Invalid(E.InvalidRequest, "Alasan paling banyak 500 karakter.");

            if (order.Version != request.Version)
                return VersionConflict();

            // VAL-MCU-45
            if (order.OrderStatus is McuOrderStatus.Delivered or McuOrderStatus.Cancelled)
                return McuResult<McuOrderDetailResponse>.Conflict(E.Val45, "Dokter MCU tidak dapat diganti pada order yang sudah selesai.");
            if (order.OrderStatus is McuOrderStatus.Scheduled or McuOrderStatus.NoShow || !order.McuDoctorId.HasValue)
                return McuResult<McuOrderDetailResponse>.Conflict(E.InvalidTransition, "Dokter MCU ditetapkan saat check-in.");

            if (request.DoctorId == order.McuDoctorId)
                return McuResult<McuOrderDetailResponse>.Invalid(E.InvalidRequest, "Dokter pengganti sama dengan dokter MCU sekarang.");

            var doctorActive = await _dbContext.Set<MstDoctor>().AsNoTracking()
                .AnyAsync(x => x.Id == request.DoctorId && x.IsActive && !x.IsDelete, cancellationToken);
            if (!doctorActive)
                return McuResult<McuOrderDetailResponse>.Invalid(E.InvalidRequest, "Dokter pengganti tidak ditemukan atau tidak aktif.");

            var current = await _dbContext.McuOrderDoctorAssignments
                .SingleOrDefaultAsync(x => x.McuOrderId == order.Id && !x.IsDelete && x.EndedAt == null, cancellationToken);

            var partnerReason = $"Ganti dokter MCU {order.OrderNumber}: {reason}";
            if (partnerReason.Length > 500)
                partnerReason = partnerReason[..500];

            var now = DateTime.UtcNow;
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Index unik penugasan berlaku: yang lama ditutup dan disimpan lebih dulu.
                if (current != null)
                {
                    current.EndedAt = now;
                    current.UpdateDateTime = now;
                    current.UpdateBy = actorUserId;
                }

                order.McuDoctorId = request.DoctorId;
                order.Version++;
                order.UpdateDateTime = now;
                order.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);

                _dbContext.McuOrderDoctorAssignments.Add(new McuOrderDoctorAssignment
                {
                    McuOrderId = order.Id,
                    DoctorId = request.DoctorId,
                    AssignedAt = now,
                    AssignedByUserId = actorUserId,
                    Reason = reason,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
                await _dbContext.SaveChangesAsync(cancellationToken);

                // INT-MCU-06 — pesanan paket terbuka berpindah penanggung jawab.
                await _labOrderService.ReassignMcuDoctorAsync(order.Id, request.DoctorId, partnerReason, cancellationToken);
                await _radOrderService.ReassignMcuDoctorAsync(order.Id, request.DoctorId, partnerReason, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (exception is DbUpdateConcurrencyException
                || (exception.InnerException as PostgresException)?.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                _dbContext.ChangeTracker.Clear();
                return VersionConflict();
            }
            catch (Exception exception) when (exception is LabConcurrencyException or RadConcurrencyException)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                _dbContext.ChangeTracker.Clear();
                return McuResult<McuOrderDetailResponse>.Conflict(E.PartnerRejected, exception.Message);
            }

            return await DetailAsync(order.Id, cancellationToken);
        }

        private async Task<McuResult<McuOrderDetailResponse>> DetailAsync(Guid orderId, CancellationToken cancellationToken) =>
            McuResult<McuOrderDetailResponse>.Ok((await _queryService.GetOrderAsync(orderId, cancellationToken))!);

        private static McuResult<McuOrderDetailResponse> VersionConflict() =>
            McuResult<McuOrderDetailResponse>.Conflict(E.Val46, "Data order sudah diubah petugas lain. Muat ulang lalu coba lagi.");
    }
}
