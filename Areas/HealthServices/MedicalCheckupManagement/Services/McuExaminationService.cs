using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using E = QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services.McuErrorCodes;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services
{
    /// <summary>
    /// Pemeriksaan MCU — <i>Mulai pemeriksaan</i> (<c>BE-MCU-021</c>), komponen order (<c>BE-MCU-022</c>), dan
    /// batal komponen (<c>BE-MCU-024</c>):
    /// <c>MCU-API-v1</c> grup Mcu Examination, <c>VAL-MCU-34</c>, <c>38</c>, <c>46</c>,
    /// <c>INT-MCU-04</c>..<c>07</c>, <c>MCU-STATE-v1</c> bagian 2.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Gerbang bayar milik Lab</b> (<c>MCU-DEC-004</c>, <c>MCU-DEC-024</c>): status kunjungan dibaca
    /// lewat <c>LabPaymentClearanceRules</c> — peserta tunai wajib lunas, peserta penjamin perusahaan
    /// <i>terjamin</i>. Pesanan Lab/Rad paket baru lahir sesudah gerbang ini, sehingga petugas Lab dan
    /// Radiologi tidak pernah melihat pesanan peserta yang belum membayar.
    /// </para>
    /// <para>
    /// <b>Satu transaksi.</b> Pesanan Lab per disiplin dan pesanan Rad per komponen dibuat di dalam
    /// transaksi yang dibuka di sini, bersama perpindahan komponen dan order. Bila satu pesanan ditolak
    /// modulnya, seluruhnya batal. Dua petugas yang menyimpan bersamaan dijaga token <c>Version</c>
    /// order: yang kalah ikut batal seluruhnya.
    /// </para>
    /// <para>
    /// <b>Komponen paket vs di luar paket</b> (<c>MCU-DEC-036</c>). Komponen paket dipesan lewat jalur
    /// bertanda paket (<c>BE-MCU-020</c>) dan tidak ditagih Lab/Rad. Pemeriksaan tambahan dokter MCU
    /// dipesan lewat jalur biasa Lab/Rad — <b>tanpa</b> tanda — sehingga ditagih satuan seperti biasa,
    /// tetapi tetap dihitung kelengkapan order.
    /// </para>
    /// <para>
    /// <b>Contoh.</b> Budi (tunai) belum membayar → <c>409</c> <i>"Tagihan paket belum lunas (sisa Rp
    /// 350.000)."</i>, nol pesanan. Sesudah lunas → satu pesanan Lab (Darah Lengkap, Gula Darah Puasa)
    /// dan satu pesanan Rad (Thorax), keduanya bertanda paket atas nama dr. Rina; Pemeriksaan Fisik
    /// siap diisi. dr. Rina lalu menambah Kolesterol Total → pesanan Lab kedua tanpa tanda, order
    /// menjadi 5 komponen.
    /// </para>
    /// </remarks>
    public class McuExaminationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly LabOrderService _labOrderService;
        private readonly LabExaminationService _labExaminationService;
        private readonly RadOrderService _radOrderService;
        private readonly McuOrderQueryService _queryService;
        private readonly InpatientClinicalContextService _clinicalContextService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly McuBookingService _bookingService;
        private readonly BillingRefundService _refundService;
        private readonly BillingInvoiceService _invoiceService;
        private readonly EncounterIntakeService _intakeService;

        public McuExaminationService(
            ApplicationDbContext dbContext,
            LabOrderService labOrderService,
            RadOrderService radOrderService,
            McuOrderQueryService queryService,
            InpatientClinicalContextService clinicalContextService,
            IHttpContextAccessor httpContextAccessor,
            McuBookingService bookingService,
            BillingRefundService refundService,
            BillingInvoiceService invoiceService,
            EncounterIntakeService intakeService,
            LabExaminationService labExaminationService)
        {
            _labExaminationService = labExaminationService;
            _bookingService = bookingService;
            _refundService = refundService;
            _invoiceService = invoiceService;
            _intakeService = intakeService;
            _dbContext = dbContext;
            _labOrderService = labOrderService;
            _radOrderService = radOrderService;
            _queryService = queryService;
            _clinicalContextService = clinicalContextService;
            _httpContextAccessor = httpContextAccessor;
        }

        // =====================================================================
        // Mulai pemeriksaan — BE-MCU-021
        // =====================================================================

        public async Task<McuResult<McuOrderDetailResponse>> StartAsync(
            Guid orderId,
            StartMcuExaminationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            if (order == null)
                return McuResult<McuOrderDetailResponse>.NotFound("Order MCU tidak ditemukan.");

            // Kirim ulang aman: pemeriksaan yang sudah dimulai mengembalikan hasil yang sama.
            if (order.ExaminationStartedAt.HasValue)
                return await DetailAsync(order.Id, cancellationToken);

            if (order.Version != request.Version)
                return VersionConflict<McuOrderDetailResponse>();

            var statusBlock = StatusBlockMessage(order.OrderStatus);
            if (statusBlock != null)
                return McuResult<McuOrderDetailResponse>.Conflict(E.InvalidTransition, statusBlock);

            if (!order.EncounterId.HasValue || !order.McuDoctorId.HasValue)
            {
                return McuResult<McuOrderDetailResponse>.Conflict(
                    E.InvalidTransition, "Data check-in order ini tidak lengkap. Hubungi admin MCU.");
            }

            // INT-MCU-04 — VAL-MCU-34
            var encounter = await _dbContext.Set<RegPatientEncounter>().AsNoTracking()
                .Where(x => x.Id == order.EncounterId.Value)
                .Select(x => new { x.Id, x.PaymentType, x.EncounterType })
                .SingleOrDefaultAsync(cancellationToken);

            if (encounter == null)
                return McuResult<McuOrderDetailResponse>.Conflict(E.InvalidTransition, "Kunjungan MCU order ini tidak ditemukan.");

            var clearance = (await LabPaymentClearanceRules.ReadAsync(
                _dbContext, [(encounter.Id, encounter.PaymentType, encounter.EncounterType)], cancellationToken))[encounter.Id];

            if (!clearance.IsCleared)
            {
                return McuResult<McuOrderDetailResponse>.Conflict(
                    E.Val34,
                    clearance.Status == LabPaymentClearanceRules.NotBilled
                        ? "Tagihan paket belum terbit. Hubungi kasir."
                        : $"Tagihan paket belum lunas (sisa {McuPackageService.FormatRupiah(clearance.OutstandingAmount)}).");
            }

            var basis = clearance.Status == LabPaymentClearanceRules.Guaranteed
                ? McuClearanceBasis.Guaranteed
                : McuClearanceBasis.Paid;

            // Komponen paket yang menunggu dikerjakan hari ini. Komponen yang dijadwal ulang saat
            // check-in tetap Rescheduled; komponen di luar paket baru dapat ditambah sesudah order
            // berjalan, sehingga di titik ini semua komponen adalah komponen paket.
            var components = await _dbContext.McuOrderComponents
                .Where(x => x.McuOrderId == order.Id && !x.IsDelete && x.ComponentStatus == McuOrderComponentStatus.Pending)
                .OrderBy(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);

            var incomplete = IncompleteComponent(components);
            if (incomplete != null)
            {
                return McuResult<McuOrderDetailResponse>.Conflict(
                    E.InvalidTransition, $"Komponen {incomplete.ComponentNameSnapshot} belum lengkap datanya. Hubungi admin MCU.");
            }

            var now = DateTime.UtcNow;
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var rejection = await PlaceOrdersAsync(order, encounter.Id, components, actorUserId, now, cancellationToken);
                if (rejection != null)
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                    _dbContext.ChangeTracker.Clear();
                    return McuResult<McuOrderDetailResponse>.Conflict(E.PartnerRejected, rejection);
                }

                order.OrderStatus = McuOrderStatus.InExamination;
                order.ClearanceBasis = basis;
                order.ExaminationStartedAt = now;
                Touch(order, actorUserId, now);
                AddHistory(order.Id, McuOrderStatus.CheckedIn, McuOrderStatus.InExamination, actorUserId, null, actorUserId, now);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                _dbContext.ChangeTracker.Clear();

                // Tekan serentak: pemenangnya sudah memulai — kembalikan hasilnya.
                var started = await _dbContext.McuOrders.AsNoTracking()
                    .AnyAsync(x => x.Id == orderId && x.ExaminationStartedAt != null, cancellationToken);
                return started ? await DetailAsync(orderId, cancellationToken) : VersionConflict<McuOrderDetailResponse>();
            }
            catch (Exception exception) when (PartnerFailure(exception) is { } message)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                _dbContext.ChangeTracker.Clear();
                return McuResult<McuOrderDetailResponse>.Conflict(E.PartnerRejected, message);
            }

            return await DetailAsync(order.Id, cancellationToken);
        }

        // =====================================================================
        // Komponen — BE-MCU-022
        // =====================================================================

        public async Task<McuResult<List<McuOrderComponentResponse>>> GetComponentsAsync(Guid orderId, CancellationToken cancellationToken)
        {
            var exists = await _dbContext.McuOrders.AsNoTracking().AnyAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            if (!exists)
                return McuResult<List<McuOrderComponentResponse>>.NotFound("Order MCU tidak ditemukan.");

            return McuResult<List<McuOrderComponentResponse>>.Ok(await _queryService.GetComponentsAsync(orderId, cancellationToken));
        }

        /// <summary>
        /// Muat ulang status komponen (<c>INT-MCU-07</c>, tanpa polling otomatis): komponen susulan yang
        /// tanggalnya tiba kembali <i>Menunggu</i> lalu dipesan bila pemeriksaan sudah dimulai; komponen
        /// Lab/Rad <i>Dipesan</i> menjadi <i>Selesai</i> bila pesanan Lab <c>Completed</c> atau pesanan Rad
        /// <c>Completed</c> (laporan final dirilis); semua tuntas → order <i>Menunggu Resume</i>.
        /// </summary>
        public async Task<McuResult<List<McuOrderComponentResponse>>> RefreshComponentsAsync(
            Guid orderId,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            if (order == null)
                return McuResult<List<McuOrderComponentResponse>>.NotFound("Order MCU tidak ditemukan.");

            if (order.OrderStatus is not (McuOrderStatus.CheckedIn or McuOrderStatus.InExamination))
                return McuResult<List<McuOrderComponentResponse>>.Ok(await _queryService.GetComponentsAsync(orderId, cancellationToken));

            var components = await _dbContext.McuOrderComponents
                .Where(x => x.McuOrderId == order.Id && !x.IsDelete)
                .OrderBy(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);

            var today = Today();
            var now = DateTime.UtcNow;
            var changed = false;

            // MCU-STATE-v1 2.1: Rescheduled → Pending saat tanggal susulan tiba.
            foreach (var component in components.Where(x => x.ComponentStatus == McuOrderComponentStatus.Rescheduled && x.ScheduledDate <= today))
            {
                component.ComponentStatus = McuOrderComponentStatus.Pending;
                TouchComponent(component, actorUserId, now);
                changed = true;
            }

            // INT-MCU-07 — status selesai dibaca dari modul pelaksana.
            var labIds = components.Where(x => x.ComponentStatus == McuOrderComponentStatus.Ordered && x.LabOrderId.HasValue)
                .Select(x => x.LabOrderId!.Value).Distinct().ToList();
            var labDone = await _dbContext.LabOrders.AsNoTracking()
                .Where(x => labIds.Contains(x.Id) && !x.IsCancel && x.OrderStatus == LabOrderStatus.Completed)
                .Select(x => new { x.Id, x.CompletedAt })
                .ToDictionaryAsync(x => x.Id, x => x.CompletedAt, cancellationToken);

            var radIds = components.Where(x => x.ComponentStatus == McuOrderComponentStatus.Ordered && x.RadOrderId.HasValue)
                .Select(x => x.RadOrderId!.Value).ToList();
            var radDone = await _dbContext.RadOrders.AsNoTracking()
                .Where(x => radIds.Contains(x.Id) && !x.IsCancel && x.OrderStatus == RadOrderStatus.Completed)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            foreach (var component in components.Where(x => x.ComponentStatus == McuOrderComponentStatus.Ordered))
            {
                var completedAt = component.LabOrderId is { } labId && labDone.TryGetValue(labId, out var labAt) ? labAt ?? now
                    : component.RadOrderId is { } radId && radDone.Contains(radId) ? now
                    : (DateTime?)null;

                if (completedAt == null)
                    continue;

                component.ComponentStatus = McuOrderComponentStatus.Completed;
                component.CompletedAt = completedAt;
                TouchComponent(component, actorUserId, now);
                changed = true;
            }

            var started = order.OrderStatus == McuOrderStatus.InExamination;
            var due = started
                ? components.Where(x => x.ComponentStatus == McuOrderComponentStatus.Pending).ToList()
                : [];

            // Komponen terakhir dapat tuntas di luar sinkron (hasil Pemeriksaan MCU diisi), sehingga
            // perpindahan ke Menunggu Resume tetap diperiksa walau tidak ada komponen yang berubah di sini.
            var allResolved = started && components.All(x => McuLabels.IsComponentResolved(x.ComponentStatus));
            if (!changed && due.Count == 0 && !allResolved)
                return McuResult<List<McuOrderComponentResponse>>.Ok(await _queryService.GetComponentsAsync(orderId, cancellationToken));

            var incomplete = IncompleteComponent(due);
            if (incomplete != null)
            {
                return McuResult<List<McuOrderComponentResponse>>.Conflict(
                    E.InvalidTransition, $"Komponen {incomplete.ComponentNameSnapshot} belum lengkap datanya. Hubungi admin MCU.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (due.Count > 0)
                {
                    var rejection = await PlaceOrdersAsync(order, order.EncounterId!.Value, due, actorUserId, now, cancellationToken);
                    if (rejection != null)
                    {
                        await transaction.RollbackAsync(CancellationToken.None);
                        _dbContext.ChangeTracker.Clear();
                        return McuResult<List<McuOrderComponentResponse>>.Conflict(E.PartnerRejected, rejection);
                    }
                }

                // MCU-STATE-v1 1.1: komponen terakhir tuntas → Menunggu Resume (sistem).
                if (allResolved)
                {
                    order.OrderStatus = McuOrderStatus.AwaitingResume;
                    AddHistory(order.Id, McuOrderStatus.InExamination, McuOrderStatus.AwaitingResume, null,
                        "Semua komponen tuntas.", actorUserId, now);
                }

                Touch(order, actorUserId, now);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                _dbContext.ChangeTracker.Clear();
                return VersionConflict<List<McuOrderComponentResponse>>();
            }
            catch (Exception exception) when (PartnerFailure(exception) is { } message)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                _dbContext.ChangeTracker.Clear();
                return McuResult<List<McuOrderComponentResponse>>.Conflict(E.PartnerRejected, message);
            }

            return McuResult<List<McuOrderComponentResponse>>.Ok(await _queryService.GetComponentsAsync(orderId, cancellationToken));
        }

        /// <summary>
        /// Tambah pemeriksaan di luar paket (<c>MCU-DEC-036</c>) — hanya dokter MCU yang ditetapkan
        /// (<c>VAL-MCU-38</c>). Order <i>Menunggu Resume</i> kembali <i>Dalam Pemeriksaan</i>.
        /// </summary>
        public async Task<McuResult<McuOrderComponentResponse>> AddComponentAsync(
            Guid orderId,
            AddMcuComponentRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            if (order == null)
                return McuResult<McuOrderComponentResponse>.NotFound("Order MCU tidak ditemukan.");

            // VAL-MCU-38
            var actorDoctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(
                _httpContextAccessor.HttpContext?.User, actorUserId, cancellationToken);
            if (!actorDoctorId.HasValue || actorDoctorId != order.McuDoctorId)
            {
                return McuResult<McuOrderComponentResponse>.Forbidden(
                    E.Val38, "Hanya dokter MCU yang ditetapkan untuk peserta ini yang dapat melakukan tindakan ini.");
            }

            var fromStatus = order.OrderStatus;
            if (fromStatus is not (McuOrderStatus.InExamination or McuOrderStatus.AwaitingResume))
            {
                return McuResult<McuOrderComponentResponse>.Conflict(
                    E.InvalidTransition,
                    fromStatus is McuOrderStatus.Scheduled or McuOrderStatus.CheckedIn
                        ? "Pemeriksaan belum dimulai."
                        : $"Order berstatus {McuLabels.OrderStatus(fromStatus)} tidak dapat ditambah pemeriksaan.");
            }

            // VAL-MCU-06 — isian sesuai jenis, sama dengan komponen paket.
            string? name = null;
            switch (request.ComponentType)
            {
                case McuComponentType.Laboratory:
                    name = await _dbContext.Set<MstProcedure>().AsNoTracking()
                        .Where(x => x.Id == request.ProcedureId && x.IsLaboratory && x.IsActive && !x.IsDelete)
                        .Select(x => x.ProcedureName)
                        .SingleOrDefaultAsync(cancellationToken);
                    if (name == null)
                        return McuResult<McuOrderComponentResponse>.Invalid(E.Val06, "Komponen Laboratorium wajib memilih prosedur laboratorium.");
                    break;

                case McuComponentType.Radiology:
                    name = await _dbContext.Set<MstProcedure>().AsNoTracking()
                        .Where(x => x.Id == request.ProcedureId && x.IsRadiology && x.IsActive && !x.IsDelete)
                        .Select(x => x.ProcedureName)
                        .SingleOrDefaultAsync(cancellationToken);
                    var modalityActive = request.RadModalityId.HasValue && await _dbContext.Set<MstRadModality>().AsNoTracking()
                        .AnyAsync(x => x.Id == request.RadModalityId.Value && x.IsActive && !x.IsDelete, cancellationToken);
                    if (name == null || !modalityActive)
                        return McuResult<McuOrderComponentResponse>.Invalid(E.Val06, "Komponen Radiologi wajib memilih prosedur dan modalitas.");
                    break;

                case McuComponentType.McuExamination:
                    name = await _dbContext.McuExaminationTypes.AsNoTracking()
                        .Where(x => x.Id == request.ExaminationTypeId && x.IsActive && !x.IsDelete)
                        .Select(x => x.ExaminationTypeName)
                        .SingleOrDefaultAsync(cancellationToken);
                    if (name == null)
                        return McuResult<McuOrderComponentResponse>.Invalid(E.Val06, "Komponen Pemeriksaan MCU wajib memilih jenis pemeriksaan.");
                    break;

                default:
                    return McuResult<McuOrderComponentResponse>.Invalid(E.InvalidRequest, "Jenis komponen tidak dikenal.");
            }

            // Pemeriksaan yang sama tidak dipesan dua kali pada satu order.
            var duplicate = await _dbContext.McuOrderComponents.AsNoTracking()
                .AnyAsync(x => x.McuOrderId == order.Id && !x.IsDelete
                    && x.ComponentStatus != McuOrderComponentStatus.Cancelled
                    && x.ComponentType == request.ComponentType
                    && (request.ComponentType == McuComponentType.McuExamination
                        ? x.ExaminationTypeId == request.ExaminationTypeId
                        : x.ProcedureId == request.ProcedureId), cancellationToken);
            if (duplicate)
                return McuResult<McuOrderComponentResponse>.Conflict(E.Val07, $"{name} sudah ada di order ini.");

            var now = DateTime.UtcNow;
            var component = new McuOrderComponent
            {
                McuOrderId = order.Id,
                PackageComponentId = null,
                ComponentType = request.ComponentType,
                ProcedureId = request.ComponentType == McuComponentType.McuExamination ? null : request.ProcedureId,
                RadModalityId = request.ComponentType == McuComponentType.Radiology ? request.RadModalityId : null,
                ExaminationTypeId = request.ComponentType == McuComponentType.McuExamination ? request.ExaminationTypeId : null,
                ComponentNameSnapshot = name,
                IsPackageCovered = false,
                RefundAmountSnapshot = 0m,
                MedicalFeeAmountSnapshot = 0m,
                ComponentStatus = McuOrderComponentStatus.Pending,
                ScheduledDate = Today(),
                AddedByDoctorId = actorDoctorId,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                _dbContext.McuOrderComponents.Add(component);

                var rejection = await PlaceOrdersAsync(order, order.EncounterId!.Value, [component], actorUserId, now, cancellationToken);
                if (rejection != null)
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                    _dbContext.ChangeTracker.Clear();
                    return McuResult<McuOrderComponentResponse>.Conflict(E.PartnerRejected, rejection);
                }

                // MCU-STATE-v1 1.1: Menunggu Resume → Dalam Pemeriksaan.
                if (fromStatus == McuOrderStatus.AwaitingResume)
                {
                    order.OrderStatus = McuOrderStatus.InExamination;
                    AddHistory(order.Id, fromStatus, McuOrderStatus.InExamination, actorUserId,
                        $"Tambah pemeriksaan di luar paket: {name}.", actorUserId, now);
                }

                Touch(order, actorUserId, now);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                _dbContext.ChangeTracker.Clear();
                return VersionConflict<McuOrderComponentResponse>();
            }
            catch (Exception exception) when (PartnerFailure(exception) is { } message)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                _dbContext.ChangeTracker.Clear();
                return McuResult<McuOrderComponentResponse>.Conflict(E.PartnerRejected, message);
            }

            return await ComponentAsync(order.Id, component.Id, cancellationToken);
        }

        /// <summary>
        /// Pindah tanggal komponen (<c>MCU-DEC-013</c>): komponen yang belum dipesan dan belum diisi
        /// dipindah ke tanggal susulan yang buka. <b>Tidak memakai kuota</b> hari tujuan (<c>MCU-DEC-029</c>).
        /// </summary>
        public async Task<McuResult<McuOrderComponentResponse>> RescheduleComponentAsync(
            Guid orderId,
            Guid componentId,
            RescheduleMcuComponentRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            if (order == null)
                return McuResult<McuOrderComponentResponse>.NotFound("Order MCU tidak ditemukan.");

            var component = await _dbContext.McuOrderComponents
                .SingleOrDefaultAsync(x => x.Id == componentId && x.McuOrderId == orderId && !x.IsDelete, cancellationToken);
            if (component == null)
                return McuResult<McuOrderComponentResponse>.NotFound("Komponen order MCU tidak ditemukan.");

            if (order.Version != request.Version)
                return VersionConflict<McuOrderComponentResponse>();

            if (order.OrderStatus is not (McuOrderStatus.CheckedIn or McuOrderStatus.InExamination))
            {
                return McuResult<McuOrderComponentResponse>.Conflict(
                    E.InvalidTransition, $"Komponen order berstatus {McuLabels.OrderStatus(order.OrderStatus)} tidak dapat dijadwal ulang.");
            }

            var componentBlock = component.ComponentStatus switch
            {
                McuOrderComponentStatus.Completed => "Komponen sudah selesai.",
                McuOrderComponentStatus.Cancelled => "Komponen sudah dibatalkan.",
                McuOrderComponentStatus.Ordered => "Komponen sudah dipesan ke modul pelaksana dan tidak dapat dijadwal ulang.",
                McuOrderComponentStatus.ReadyToFill when await _dbContext.McuExaminationResults.AsNoTracking()
                    .AnyAsync(x => x.McuOrderComponentId == component.Id && !x.IsDelete, cancellationToken)
                    => "Hasil pemeriksaan ini sudah mulai diisi dan tidak dapat dijadwal ulang.",
                _ => null
            };
            if (componentBlock != null)
                return McuResult<McuOrderComponentResponse>.Conflict(E.InvalidTransition, componentBlock);

            var today = Today();
            if (request.ScheduledDate == component.ScheduledDate)
                return McuResult<McuOrderComponentResponse>.Invalid(E.InvalidRequest, "Tanggal baru sama dengan tanggal sekarang.");

            if (request.ScheduledDate <= today)
            {
                return McuResult<McuOrderComponentResponse>.Invalid(
                    E.InvalidRequest, $"Pilih tanggal susulan sesudah hari ini untuk {component.ComponentNameSnapshot}.");
            }

            // Tanggal susulan wajib buka, tanpa memakai kuota (MCU-STATE-v1 2.1, MCU-DEC-029).
            var open = await _dbContext.McuDailySchedules.AsNoTracking()
                .AnyAsync(x => x.ScheduleDate == request.ScheduledDate && !x.IsDelete && x.IsOpen, cancellationToken);
            if (!open)
                return McuResult<McuOrderComponentResponse>.Invalid(E.Val10, $"MCU tutup pada {McuScheduleService.FormatDate(request.ScheduledDate)}.");

            var now = DateTime.UtcNow;
            component.ComponentStatus = McuOrderComponentStatus.Rescheduled;
            component.ScheduledDate = request.ScheduledDate;
            TouchComponent(component, actorUserId, now);
            Touch(order, actorUserId, now);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                _dbContext.ChangeTracker.Clear();
                return VersionConflict<McuOrderComponentResponse>();
            }

            return await ComponentAsync(order.Id, component.Id, cancellationToken);
        }

        // =====================================================================
        // Hasil Pemeriksaan MCU — BE-MCU-023
        // =====================================================================

        public async Task<McuResult<McuExaminationResultResponse>> GetResultAsync(
            Guid orderId,
            Guid componentId,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            var component = order == null
                ? null
                : await _dbContext.McuOrderComponents.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == componentId && x.McuOrderId == orderId && !x.IsDelete, cancellationToken);

            if (order == null || component == null)
                return McuResult<McuExaminationResultResponse>.NotFound("Komponen order MCU tidak ditemukan.");

            if (component.ComponentType != McuComponentType.McuExamination)
                return McuResult<McuExaminationResultResponse>.Invalid(E.InvalidRequest, "Hasil Laboratorium/Radiologi diisi di modulnya.");

            return McuResult<McuExaminationResultResponse>.Ok(await BuildResultAsync(order, component, cancellationToken));
        }

        /// <summary>
        /// Isi/ubah hasil Pemeriksaan MCU (<c>MCU-DEC-007</c>, <c>018</c>, <c>035</c>): butuh skrining selesai
        /// (<c>VAL-MCU-36</c>), angka berformat desimal (<c>VAL-MCU-43</c>), isian wajib saat diselesaikan
        /// (<c>VAL-MCU-42</c>), dan terkunci sesudah resume disetujui (<c>VAL-MCU-44</c>). Tanpa lampiran berkas.
        /// </summary>
        /// <remarks>
        /// Nilai sesudah resume pernah disetujui (buka ulang) menulis <c>McuExaminationResultValueRevision</c>
        /// lebih dulu. Nilai dan kesimpulan tidak pernah masuk log aplikasi.
        /// </remarks>
        public async Task<McuResult<McuExaminationResultResponse>> SaveResultAsync(
            Guid orderId,
            Guid componentId,
            SaveMcuExaminationResultRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            var component = order == null
                ? null
                : await _dbContext.McuOrderComponents
                    .SingleOrDefaultAsync(x => x.Id == componentId && x.McuOrderId == orderId && !x.IsDelete, cancellationToken);

            if (order == null || component == null)
                return McuResult<McuExaminationResultResponse>.NotFound("Komponen order MCU tidak ditemukan.");

            if (component.ComponentType != McuComponentType.McuExamination || !component.ExaminationTypeId.HasValue)
                return McuResult<McuExaminationResultResponse>.Invalid(E.InvalidRequest, "Hasil Laboratorium/Radiologi diisi di modulnya.");

            var block = await ReadOnlyReasonAsync(order, component, cancellationToken);
            if (block != null)
                return McuResult<McuExaminationResultResponse>.Conflict(block.Value.Code, block.Value.Message);

            var conclusion = string.IsNullOrWhiteSpace(request.Conclusion) ? null : request.Conclusion.Trim();
            if (conclusion?.Length > 2000)
                return McuResult<McuExaminationResultResponse>.Invalid(E.InvalidRequest, "Kesimpulan paling banyak 2.000 karakter.");

            var fields = await _dbContext.McuExaminationFields.AsNoTracking()
                .Where(x => x.ExaminationTypeId == component.ExaminationTypeId.Value && !x.IsDelete)
                .OrderBy(x => x.FieldNumber)
                .ToListAsync(cancellationToken);
            var fieldById = fields.ToDictionary(x => x.Id);

            // VAL-MCU-43 dan bentuk isian lain — dinormalkan sebelum disimpan.
            var incoming = new Dictionary<Guid, string?>();
            foreach (var value in request.Values ?? [])
            {
                if (!fieldById.TryGetValue(value.ExaminationFieldId, out var field))
                    return McuResult<McuExaminationResultResponse>.Invalid(E.InvalidRequest, "Isian tidak dikenal untuk pemeriksaan ini.");

                var normalized = NormalizeValue(field, value.Value);
                if (normalized.Error != null)
                    return McuResult<McuExaminationResultResponse>.Invalid(normalized.Code!, normalized.Error);

                incoming[field.Id] = normalized.Value;
            }

            var result = await _dbContext.McuExaminationResults
                .Include(x => x.Values)
                .SingleOrDefaultAsync(x => x.McuOrderComponentId == component.Id && !x.IsDelete, cancellationToken);

            // Komponen yang sudah selesai tetap selesai: setiap simpan ulang diperiksa seperti diselesaikan.
            var completing = request.Complete || component.ComponentStatus == McuOrderComponentStatus.Completed;
            if (completing)
            {
                foreach (var field in fields.Where(x => x.IsRequired))
                {
                    var final = incoming.TryGetValue(field.Id, out var sent)
                        ? sent
                        : result?.Values.FirstOrDefault(v => v.ExaminationFieldId == field.Id && !v.IsDelete)?.ValueText;

                    if (string.IsNullOrEmpty(final))
                        return McuResult<McuExaminationResultResponse>.Invalid(E.Val42, $"Isi {field.FieldLabel}.");
                }
            }

            var now = DateTime.UtcNow;
            var everApproved = await _dbContext.McuResumes.AsNoTracking()
                .AnyAsync(x => x.McuOrderId == order.Id && !x.IsDelete && x.ApprovedAt != null, cancellationToken);
            // Versi resume terakhir yang DISETUJUI dengan nilai lama (data dictionary) — sesudah buka ulang
            // itu versi yang dikoreksi, bukan draf berlaku (BE-MCU-031).
            var resumeVersion = everApproved
                ? await _dbContext.McuResumes.AsNoTracking()
                    .Where(x => x.McuOrderId == order.Id && !x.IsDelete && x.ApprovedAt != null)
                    .MaxAsync(x => x.VersionNumber, cancellationToken)
                : 0;

            if (result == null)
            {
                result = new McuExaminationResult
                {
                    McuOrderComponentId = component.Id,
                    FilledByUserId = actorUserId,
                    FilledAt = now,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };
                _dbContext.McuExaminationResults.Add(result);
            }
            else
            {
                result.FilledByUserId = actorUserId;
                result.FilledAt = now;
                result.UpdateDateTime = now;
                result.UpdateBy = actorUserId;
            }

            result.ConclusionText = conclusion;

            foreach (var (fieldId, text) in incoming)
            {
                var existing = result.Values.FirstOrDefault(v => v.ExaminationFieldId == fieldId && !v.IsDelete);
                if (existing == null)
                {
                    _dbContext.McuExaminationResultValues.Add(new McuExaminationResultValue
                    {
                        ExaminationResultId = result.Id,
                        ExaminationFieldId = fieldId,
                        ValueText = text,
                        CreateDateTime = now,
                        CreateBy = actorUserId
                    });
                    continue;
                }

                if (existing.ValueText == text)
                    continue;

                // Nilai lama dicatat dulu bila resume pernah disetujui (buka ulang).
                if (everApproved)
                {
                    _dbContext.McuExaminationResultValueRevisions.Add(new McuExaminationResultValueRevision
                    {
                        ResultValueId = existing.Id,
                        PreviousValueText = existing.ValueText,
                        ResumeVersionNumber = resumeVersion,
                        ChangedByUserId = actorUserId,
                        ChangedAt = now,
                        CreateDateTime = now,
                        CreateBy = actorUserId
                    });
                }

                existing.ValueText = text;
                existing.UpdateDateTime = now;
                existing.UpdateBy = actorUserId;
            }

            if (completing && component.ComponentStatus != McuOrderComponentStatus.Completed)
            {
                component.ComponentStatus = McuOrderComponentStatus.Completed;
                component.CompletedAt = now;
                component.PerformingDoctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(
                    _httpContextAccessor.HttpContext?.User, actorUserId, cancellationToken);
                TouchComponent(component, actorUserId, now);

                // MCU-STATE-v1 1.1: komponen terakhir tuntas → Menunggu Resume (sistem).
                var othersResolved = await _dbContext.McuOrderComponents.AsNoTracking()
                    .Where(x => x.McuOrderId == order.Id && !x.IsDelete && x.Id != component.Id)
                    .AllAsync(x => x.ComponentStatus == McuOrderComponentStatus.Completed
                        || x.ComponentStatus == McuOrderComponentStatus.Cancelled, cancellationToken);

                if (order.OrderStatus == McuOrderStatus.InExamination && othersResolved)
                {
                    order.OrderStatus = McuOrderStatus.AwaitingResume;
                    AddHistory(order.Id, McuOrderStatus.InExamination, McuOrderStatus.AwaitingResume, null,
                        "Semua komponen tuntas.", actorUserId, now);
                }
            }

            Touch(order, actorUserId, now);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (exception is DbUpdateConcurrencyException || IsUniqueViolation(exception))
            {
                _dbContext.ChangeTracker.Clear();
                return VersionConflict<McuExaminationResultResponse>();
            }

            return McuResult<McuExaminationResultResponse>.Ok(await BuildResultAsync(order, component, cancellationToken));
        }

        private async Task<McuExaminationResultResponse> BuildResultAsync(
            McuOrder order,
            McuOrderComponent component,
            CancellationToken cancellationToken)
        {
            var fields = component.ExaminationTypeId.HasValue
                ? await _dbContext.McuExaminationFields.AsNoTracking()
                    .Where(x => x.ExaminationTypeId == component.ExaminationTypeId.Value && !x.IsDelete)
                    .OrderBy(x => x.FieldNumber)
                    .ToListAsync(cancellationToken)
                : [];

            var result = await _dbContext.McuExaminationResults.AsNoTracking()
                .Where(x => x.McuOrderComponentId == component.Id && !x.IsDelete)
                .Select(x => new
                {
                    x.ConclusionText,
                    x.FilledAt,
                    FilledByName = _dbContext.Users.Where(u => u.Id == x.FilledByUserId).Select(u => u.DisplayName).FirstOrDefault(),
                    Values = x.Values.Where(v => !v.IsDelete).Select(v => new { v.ExaminationFieldId, v.ValueText }).ToList()
                })
                .SingleOrDefaultAsync(cancellationToken);

            var performingDoctorName = component.PerformingDoctorId.HasValue
                ? await _dbContext.Set<MstDoctor>().AsNoTracking()
                    .Where(x => x.Id == component.PerformingDoctorId.Value)
                    .Select(x => x.FullName)
                    .FirstOrDefaultAsync(cancellationToken)
                : null;

            var block = await ReadOnlyReasonAsync(order, component, cancellationToken);

            return new McuExaminationResultResponse
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                ComponentId = component.Id,
                ComponentName = component.ComponentNameSnapshot,
                ExaminationTypeId = component.ExaminationTypeId,
                ComponentStatus = component.ComponentStatus,
                ComponentStatusLabel = McuLabels.ComponentStatus(component.ComponentStatus),
                IsScreeningCompleted = await IsScreeningCompletedAsync(order.EncounterId, cancellationToken),
                ReadOnlyReason = block?.Message,
                Conclusion = result?.ConclusionText,
                FilledByName = result?.FilledByName,
                FilledAt = result?.FilledAt,
                PerformingDoctorName = performingDoctorName,
                Fields = fields.Select(f => new McuExaminationResultFieldResponse
                {
                    ExaminationFieldId = f.Id,
                    FieldNumber = f.FieldNumber,
                    FieldCode = f.FieldCode,
                    FieldLabel = f.FieldLabel,
                    FieldType = f.FieldType,
                    FieldTypeLabel = McuReferenceDataService.FieldTypeLabel(f.FieldType),
                    UnitName = f.UnitName,
                    Options = f.FieldType == McuExaminationFieldType.YesNo ? [YesValue, NoValue] : McuReferenceDataService.SplitOptions(f.OptionValues),
                    IsRequired = f.IsRequired,
                    Value = result?.Values.FirstOrDefault(v => v.ExaminationFieldId == f.Id)?.ValueText
                }).ToList()
            };
        }

        /// <summary>
        /// Alasan hasil tidak dapat diisi, berurutan: kunci resume (<c>VAL-MCU-44</c>), status order dan
        /// komponen, lalu skrining (<c>VAL-MCU-36</c>). <c>null</c> bila dapat diisi.
        /// </summary>
        private async Task<(string Code, string Message)?> ReadOnlyReasonAsync(
            McuOrder order,
            McuOrderComponent component,
            CancellationToken cancellationToken)
        {
            var resumeLocked = order.OrderStatus is McuOrderStatus.DoctorApproved or McuOrderStatus.Verified or McuOrderStatus.Delivered
                || await _dbContext.McuResumes.AsNoTracking()
                    .AnyAsync(x => x.McuOrderId == order.Id && !x.IsDelete && x.IsCurrent
                        && (x.ResumeStatus == McuResumeStatus.Approved || x.ResumeStatus == McuResumeStatus.Verified), cancellationToken);
            if (resumeLocked)
                return (E.Val44, "Hasil terkunci. Minta dokter membuka ulang resume.");

            if (order.OrderStatus is not (McuOrderStatus.InExamination or McuOrderStatus.AwaitingResume))
            {
                return (E.InvalidTransition, order.OrderStatus is McuOrderStatus.Scheduled or McuOrderStatus.CheckedIn
                    ? "Pemeriksaan belum dimulai."
                    : $"Order berstatus {McuLabels.OrderStatus(order.OrderStatus)} tidak dapat diisi hasilnya.");
            }

            var componentBlock = component.ComponentStatus switch
            {
                McuOrderComponentStatus.ReadyToFill or McuOrderComponentStatus.Completed => null,
                McuOrderComponentStatus.Cancelled => "Komponen sudah dibatalkan.",
                McuOrderComponentStatus.Rescheduled => $"Komponen dijadwal ulang ke {McuScheduleService.FormatDate(component.ScheduledDate)}.",
                _ => "Komponen belum siap diisi."
            };
            if (componentBlock != null)
                return (E.InvalidTransition, componentBlock);

            if (!await IsScreeningCompletedAsync(order.EncounterId, cancellationToken))
                return (E.Val36, "Skrining awal peserta belum selesai.");

            return null;
        }

        /// <summary>
        /// Skrining perawat kunjungan MCU selesai bila antreannya mencatat <c>ScreeningCompletedAt</c>
        /// (<c>INT-MCU-11</c>, dibuktikan di devYoga pada <c>BE-MCU-001</c> <c>AT-MCU-RUN-02</c>).
        /// </summary>
        private Task<bool> IsScreeningCompletedAsync(Guid? encounterId, CancellationToken cancellationToken) =>
            encounterId == null
                ? Task.FromResult(false)
                : _dbContext.Set<RegQueue>().AsNoTracking()
                    .AnyAsync(x => x.EncounterId == encounterId.Value && !x.IsDelete && x.ScreeningCompletedAt != null, cancellationToken);

        private const string YesValue = "Ya";
        private const string NoValue = "Tidak";

        private sealed record NormalizedValue(string? Value, string? Code, string? Error);

        /// <summary>Nilai satu isian dinormalkan menurut jenisnya; kosong berarti isian dikosongkan.</summary>
        private static NormalizedValue NormalizeValue(McuExaminationField field, string? raw)
        {
            var text = string.IsNullOrWhiteSpace(raw) ? null : raw.Trim();
            if (text == null)
                return new NormalizedValue(null, null, null);

            switch (field.FieldType)
            {
                case McuExaminationFieldType.Number:
                    return TryNormalizeNumber(text, out var number)
                        ? new NormalizedValue(number, null, null)
                        : new NormalizedValue(null, E.Val43, "Nilai harus berupa angka, contoh 6.4.");

                case McuExaminationFieldType.Option:
                    var options = McuReferenceDataService.SplitOptions(field.OptionValues);
                    var option = options.FirstOrDefault(x => string.Equals(x, text, StringComparison.OrdinalIgnoreCase));
                    return option != null
                        ? new NormalizedValue(option, null, null)
                        : new NormalizedValue(null, E.InvalidRequest, $"Pilih salah satu untuk {field.FieldLabel}: {string.Join(", ", options)}.");

                case McuExaminationFieldType.YesNo:
                    return string.Equals(text, YesValue, StringComparison.OrdinalIgnoreCase) ? new NormalizedValue(YesValue, null, null)
                        : string.Equals(text, NoValue, StringComparison.OrdinalIgnoreCase) ? new NormalizedValue(NoValue, null, null)
                        : new NormalizedValue(null, E.InvalidRequest, $"Pilih Ya atau Tidak untuk {field.FieldLabel}.");

                default:
                    return text.Length <= 500
                        ? new NormalizedValue(text, null, null)
                        : new NormalizedValue(null, E.InvalidRequest, $"{field.FieldLabel} paling banyak 500 karakter.");
            }
        }

        /// <summary>
        /// <c>VAL-MCU-43</c>: <c>6,4</c> dan <c>6.4</c> sama-sama menjadi <c>6.4</c> — koma tidak pernah dibuang
        /// (pelajaran <c>BaseTextField</c> yang mengubah 6,4 menjadi 64). Pemisah ribuan ditolak.
        /// </summary>
        internal static bool TryNormalizeNumber(string text, out string normalized)
        {
            normalized = string.Empty;
            if (text.Count(c => c is ',' or '.') > 1)
                return false;

            if (!decimal.TryParse(text.Replace(',', '.'), System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowDecimalPoint,
                    System.Globalization.CultureInfo.InvariantCulture, out var value))
                return false;

            normalized = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return true;
        }

        /// <summary>Dua simpan pertama serentak pada komponen yang sama bertabrakan di index unik hasil.</summary>
        private static bool IsUniqueViolation(DbUpdateException exception) =>
            (exception.InnerException as PostgresException)?.SqlState == PostgresErrorCodes.UniqueViolation;

        // =====================================================================
        // Batal penuh — BE-MCU-025
        // =====================================================================

        /// <summary>
        /// <c>PATCH orders/{id}/cancel</c>. Order <i>Hadir</i>/<i>Dalam Pemeriksaan</i> dibatalkan penuh di
        /// sini; status lain (termasuk <i>Terjadwal</i>) diteruskan ke batal sebelum datang (<c>BE-MCU-011</c>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Hanya bila belum ada komponen dimulai</b> (<c>VAL-MCU-41</c>, <c>MCU-STATE-v1</c> 2.3): Lab —
        /// pesanan sudah melewati <c>Requested</c>/<c>Confirmed</c> atau ada wadah yang sudah diambil;
        /// Rad — pesanan sudah dikerjakan atau akuisisi dimulai; Pemeriksaan MCU — hasil sudah tersimpan.
        /// </para>
        /// <para>
        /// <b>Satu transaksi</b> (keputusan pemilik modul 2026-10-22): pesanan Lab/Rad dibatalkan, baris paket
        /// dikembalikan utuh — refund diajukan bila sudah dibayar, baris dibatalkan (void) bila belum —
        /// kunjungan beserta antreannya dibatalkan, lalu komponen dan order <i>Dibatalkan</i>. Billing dan
        /// Registrasi ikut transaksi ini; satu langkah gagal membatalkan seluruhnya. Kuota hari itu tidak
        /// dikembalikan — peserta sudah datang (keputusan pemilik modul 2026-10-22).
        /// </para>
        /// </remarks>
        public async Task<McuResult<McuOrderDetailResponse>> CancelOrderAsync(
            Guid orderId,
            CancelMcuOrderRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            if (order == null)
                return McuResult<McuOrderDetailResponse>.NotFound("Order MCU tidak ditemukan.");

            if (order.OrderStatus is not (McuOrderStatus.CheckedIn or McuOrderStatus.InExamination))
            {
                _dbContext.ChangeTracker.Clear();
                return await _bookingService.CancelBeforeArrivalAsync(orderId, request, actorUserId, cancellationToken);
            }

            var reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();
            if (reason == null)
                return McuResult<McuOrderDetailResponse>.Invalid(E.Val23, "Alasan pembatalan wajib diisi.");
            if (reason.Length > 500)
                return McuResult<McuOrderDetailResponse>.Invalid(E.InvalidRequest, "Alasan pembatalan paling banyak 500 karakter.");

            if (order.Version != request.Version)
                return VersionConflict<McuOrderDetailResponse>();

            var components = await _dbContext.McuOrderComponents
                .Where(x => x.McuOrderId == order.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);

            // VAL-MCU-41
            if (await AnyComponentStartedAsync(components, cancellationToken))
            {
                return McuResult<McuOrderDetailResponse>.Conflict(
                    E.Val41, "Sudah ada pemeriksaan yang dimulai. Gunakan pembatalan per komponen.");
            }

            var packageItem = !order.EncounterId.HasValue || !order.PackageInvoiceItemId.HasValue
                ? null
                : await _dbContext.BilInvoiceItems.AsNoTracking()
                    .Where(x => x.Id == order.PackageInvoiceItemId.Value && !x.IsDelete)
                    .Select(x => new { x.Id, x.InvoiceId, x.Status, x.SourceVersion, x.Quantity, x.UnitPrice, InvoiceRowVersion = x.Invoice.RowVersion })
                    .SingleOrDefaultAsync(cancellationToken);

            if (packageItem == null)
            {
                return McuResult<McuOrderDetailResponse>.Conflict(
                    E.InvalidTransition, "Kunjungan atau baris paket order ini tidak ditemukan. Hubungi kasir.");
            }

            var paidAmount = await PaidAmountAsync(packageItem.InvoiceId, cancellationToken);

            var now = DateTime.UtcNow;
            var history = new McuOrderStatusHistory
            {
                McuOrderId = order.Id,
                FromStatus = order.OrderStatus,
                ToStatus = McuOrderStatus.Cancelled,
                ChangedAt = now,
                ChangedByUserId = actorUserId,
                Reason = reason,
                CreateDateTime = now,
                CreateBy = actorUserId
            };
            var partnerReason = $"Batal penuh MCU {order.OrderNumber}: {reason}";
            if (partnerReason.Length > 250)
                partnerReason = partnerReason[..250];

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Pesanan turunan — Lab per pesanan, Rad per pesanan.
                foreach (var labOrderId in components.Where(x => x.LabOrderId.HasValue).Select(x => x.LabOrderId!.Value).Distinct())
                {
                    var cancelled = await _dbContext.LabOrders.AsNoTracking().AnyAsync(x => x.Id == labOrderId && x.IsCancel, cancellationToken);
                    if (!cancelled)
                        await _labOrderService.CancelAsync(labOrderId, new CancelLabOrderRequest { CancelReason = partnerReason }, cancellationToken);
                }

                foreach (var component in components.Where(x => x.RadOrderId.HasValue))
                {
                    var cancelled = await _dbContext.RadOrders.AsNoTracking()
                        .AnyAsync(x => x.Id == component.RadOrderId!.Value && (x.IsCancel || x.OrderStatus == RadOrderStatus.Cancelled), cancellationToken);
                    if (cancelled)
                        continue;

                    var radResult = await _radOrderService.CancelAsync(
                        component.RadOrderId!.Value, new RadOrderTransitionRequest { Reason = partnerReason }, cancellationToken);
                    if (!radResult.IsSuccess)
                    {
                        await transaction.RollbackAsync(CancellationToken.None);
                        _dbContext.ChangeTracker.Clear();
                        return McuResult<McuOrderDetailResponse>.Conflict(
                            E.PartnerRejected, $"Pesanan radiologi {component.ComponentNameSnapshot} tidak dapat dibatalkan: {radResult.ErrorMessage}");
                    }
                }

                // INT-MCU-09 — baris paket dikembalikan utuh, sebelum kunjungan dibatalkan (kalkulasi
                // Billing hanya menghitung kunjungan yang belum batal).
                if (packageItem.Status != BillingInvoiceItemStatuses.Voided)
                {
                    if (paidAmount > 0m)
                    {
                        await _refundService.CreateAsync(
                            new CreateRefundRequest
                            {
                                InvoiceId = packageItem.InvoiceId,
                                RefundCategory = "BILLING",
                                SelectedBillingItemIds = [packageItem.Id],
                                RequestedAmount = packageItem.Quantity * packageItem.UnitPrice,
                                Reason = partnerReason,
                                CorrelationId = order.Id,
                                CausationId = history.Id
                            },
                            order.Id,
                            actorUserId,
                            cancellationToken);
                    }
                    else
                    {
                        await _invoiceService.VoidItemAsync(
                            packageItem.InvoiceId,
                            packageItem.Id,
                            new VoidInvoiceItemRequest
                            {
                                ExpectedRowVersion = packageItem.InvoiceRowVersion,
                                SourceVersion = packageItem.SourceVersion + 1,
                                SourceStatus = BillingInvoiceItemStatuses.Voided,
                                ContractVersion = ContractBillingChargeSourceAdapter.ContractVersion,
                                Reason = partnerReason,
                                CorrelationId = order.Id,
                                CausationId = history.Id
                            },
                            order.Id,
                            actorUserId,
                            cancellationToken);
                    }
                }

                // Kunjungan beserta antreannya.
                await _intakeService.CancelMedicalCheckupAsync(order.EncounterId!.Value, partnerReason, actorUserId, cancellationToken);

                foreach (var component in components.Where(x => x.ComponentStatus != McuOrderComponentStatus.Cancelled))
                {
                    component.ComponentStatus = McuOrderComponentStatus.Cancelled;
                    component.CancelReason = reason;
                    TouchComponent(component, actorUserId, now);
                }

                order.OrderStatus = McuOrderStatus.Cancelled;
                order.CancelReason = reason;
                order.IsCancel = true;
                order.CancelDateTime = now;
                order.CancelBy = actorUserId;
                Touch(order, actorUserId, now);
                _dbContext.McuOrderStatusHistories.Add(history);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                _dbContext.ChangeTracker.Clear();
                return VersionConflict<McuOrderDetailResponse>();
            }
            catch (Exception exception) when (CancelPartnerFailure(exception) is { } message)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                _dbContext.ChangeTracker.Clear();
                return McuResult<McuOrderDetailResponse>.Conflict(E.PartnerRejected, message);
            }

            await _intakeService.PublishDeferredAsync();

            return await DetailAsync(order.Id, cancellationToken);
        }

        /// <summary><c>MCU-STATE-v1</c> 2.3 — arti "komponen sudah dimulai".</summary>
        private async Task<bool> AnyComponentStartedAsync(List<McuOrderComponent> components, CancellationToken cancellationToken)
        {
            if (components.Any(x => x.ComponentStatus == McuOrderComponentStatus.Completed))
                return true;

            var labIds = components.Where(x => x.LabOrderId.HasValue).Select(x => x.LabOrderId!.Value).Distinct().ToList();
            if (labIds.Count > 0)
            {
                // Lab: pesanan melewati Requested/Confirmed, atau wadah sudah diambil (contoh MCU-DEC-021:
                // darah sudah diambil pukul 07.15).
                var labStarted = await _dbContext.LabOrders.AsNoTracking()
                    .AnyAsync(x => labIds.Contains(x.Id) && !x.IsCancel
                        && x.OrderStatus != LabOrderStatus.Requested && x.OrderStatus != LabOrderStatus.Confirmed, cancellationToken)
                    || await _dbContext.LabSpecimens.AsNoTracking()
                        .AnyAsync(x => labIds.Contains(x.LabOrderId) && !x.IsDelete
                            && x.SpecimenStatus != LabSpecimenStatus.Planned && x.SpecimenStatus != LabSpecimenStatus.Cancelled, cancellationToken);
                if (labStarted)
                    return true;
            }

            var radIds = components.Where(x => x.RadOrderId.HasValue).Select(x => x.RadOrderId!.Value).ToList();
            if (radIds.Count > 0)
            {
                // Rad: pesanan sudah dikerjakan (termasuk hasil langsung RAD-DEC-023) atau akuisisi dimulai.
                var radStarted = await _dbContext.RadOrders.AsNoTracking()
                    .AnyAsync(x => radIds.Contains(x.Id) && !x.IsCancel
                        && (x.OrderStatus == RadOrderStatus.InProgress || x.OrderStatus == RadOrderStatus.Completed), cancellationToken)
                    || await _dbContext.RadStudies.AsNoTracking()
                        .AnyAsync(x => radIds.Contains(x.RadOrderId) && !x.IsDelete && x.AcquisitionStartedAt != null, cancellationToken);
                if (radStarted)
                    return true;
            }

            var examinationIds = components.Where(x => x.ComponentType == McuComponentType.McuExamination).Select(x => x.Id).ToList();
            return examinationIds.Count > 0 && await _dbContext.McuExaminationResults.AsNoTracking()
                .AnyAsync(x => examinationIds.Contains(x.McuOrderComponentId) && !x.IsDelete, cancellationToken);
        }

        /// <summary>Jumlah yang sudah dibayar pada invoice — rumus yang sama dengan <c>LabPaymentClearanceRules</c>.</summary>
        private async Task<decimal> PaidAmountAsync(Guid invoiceId, CancellationToken cancellationToken)
        {
            var settlementIds = await _dbContext.BilSettlements.AsNoTracking()
                .Where(x => x.InvoiceId == invoiceId && x.Purpose == BillingSettlementPurposes.InvoicePayment)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            if (settlementIds.Count == 0)
                return 0m;

            return (await _dbContext.BilTenders.AsNoTracking()
                    .Where(x => settlementIds.Contains(x.SettlementId) && x.Status == BillingTenderStatuses.Succeeded)
                    .Select(x => x.Amount)
                    .ToListAsync(cancellationToken))
                .Sum();
        }

        /// <summary>Penolakan Lab, Billing, atau Registrasi saat batal penuh — pesannya diteruskan apa adanya.</summary>
        private static string? CancelPartnerFailure(Exception exception) => exception switch
        {
            BillingRefundException or BillingInvoiceValidationException or BillingInvoiceConflictException
                or EncounterIntakeValidationException or EncounterIntakeConflictException => exception.Message,
            _ => PartnerFailure(exception)
        };

        // =====================================================================
        // Batal komponen — BE-MCU-024
        // =====================================================================

        private const string LabValidatedMessage = "Pemeriksaan Lab sudah divalidasi dan tidak dapat dibatalkan.";

        /// <summary>
        /// <c>PATCH components/{componentId}/cancel</c> — dokter MCU yang ditetapkan (<c>VAL-MCU-38</c>) membatalkan
        /// satu komponen dengan alasan (<c>VAL-MCU-40</c>, <c>MCU-DEC-015</c>); bila sudah dibayar, refund sebesar
        /// nilai pengembaliannya diajukan ke Billing (<c>INT-MCU-09</c>, <c>MCU-DEC-043</c>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Lab per pemeriksaan</b> (<c>INT-MCU-08</c>, <c>ARC-TECH-01</c>). Pemeriksaan yang sudah masuk wadah
        /// dibatalkan lewat <c>LabExaminationService.CancelAsync</c>; permintaan yang belum masuk wadah dicabut lewat
        /// <c>CancelOrderedProcedureAsync</c> (keputusan pemilik Lab 2026-10-23). Bila komponen itu yang terakhir
        /// pada pesanan Lab yang belum diproses, pesanan Lab dibatalkan utuh. Pemeriksaan yang sudah divalidasi
        /// ditolak MCU lebih dulu, karena Lab sendiri hanya menolak yang sudah dirilis. Rad per pesanan.
        /// </para>
        /// <para>
        /// <b>Refund</b> hanya untuk komponen paket bernilai pengembalian &gt; 0 pada invoice yang sudah dibayar;
        /// Billing menolak bila nominalnya melebihi sisa baris paket. Peserta perusahaan yang belum membayar tidak
        /// menerima refund — perlakuan tagihan kolektif belum tersedia di Billing (<c>MCU-COORD-003</c>).
        /// </para>
        /// <para>
        /// <b>Satu transaksi</b>: Lab, Rad, refund, komponen, dan order. Satu langkah ditolak membatalkan seluruhnya.
        /// Komponen terakhir yang tuntas memindah order ke <i>Menunggu Resume</i>.
        /// </para>
        /// <para>
        /// <b>Contoh.</b> Siti (Paket Basic Rp 350.000, lunas) ternyata hamil. dr. Rina membatalkan Thorax dengan
        /// alasan <i>kehamilan</i> → komponen <i>Dibatalkan</i>, pesanan Rad batal, refund Rp 150.000 diajukan.
        /// Audiometri dibatalkan kemudian → refund Rp 50.000; sisa baris paket Rp 150.000.
        /// </para>
        /// </remarks>
        public async Task<McuResult<McuOrderComponentResponse>> CancelComponentAsync(
            Guid orderId,
            Guid componentId,
            CancelMcuComponentRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
            var component = order == null
                ? null
                : await _dbContext.McuOrderComponents.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == componentId && x.McuOrderId == orderId && !x.IsDelete, cancellationToken);

            if (order == null || component == null)
                return McuResult<McuOrderComponentResponse>.NotFound("Komponen order MCU tidak ditemukan.");

            // VAL-MCU-38
            var actorDoctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(
                _httpContextAccessor.HttpContext?.User, actorUserId, cancellationToken);
            if (!actorDoctorId.HasValue || actorDoctorId != order.McuDoctorId)
            {
                return McuResult<McuOrderComponentResponse>.Forbidden(
                    E.Val38, "Hanya dokter MCU yang ditetapkan untuk peserta ini yang dapat melakukan tindakan ini.");
            }

            // VAL-MCU-40
            var reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();
            if (reason == null)
                return McuResult<McuOrderComponentResponse>.Invalid(E.Val40, "Alasan wajib diisi.");
            if (reason.Length > 500)
                return McuResult<McuOrderComponentResponse>.Invalid(E.InvalidRequest, "Alasan pembatalan paling banyak 500 karakter.");

            if (order.Version != request.Version)
                return VersionConflict<McuOrderComponentResponse>();

            if (order.OrderStatus is not (McuOrderStatus.InExamination or McuOrderStatus.AwaitingResume))
            {
                return McuResult<McuOrderComponentResponse>.Conflict(
                    E.InvalidTransition,
                    order.OrderStatus switch
                    {
                        McuOrderStatus.Scheduled or McuOrderStatus.CheckedIn => "Pemeriksaan belum dimulai.",
                        McuOrderStatus.Cancelled => "Order sudah dibatalkan.",
                        _ => $"Order berstatus {McuLabels.OrderStatus(order.OrderStatus)} tidak dapat dibatalkan komponennya."
                    });
            }

            // MCU-STATE-v1 2.2
            var componentBlock = component.ComponentStatus switch
            {
                McuOrderComponentStatus.Completed => "Komponen sudah selesai.",
                McuOrderComponentStatus.Cancelled => "Komponen sudah dibatalkan.",
                _ => null
            };
            if (componentBlock != null)
                return McuResult<McuOrderComponentResponse>.Conflict(E.InvalidTransition, componentBlock);

            // Rencana Lab — dibaca sebelum transaksi, dikerjakan di dalamnya.
            var labOrderId = component.ComponentType == McuComponentType.Laboratory ? component.LabOrderId : null;
            List<Guid> labExaminationIds = [];
            var cancelLabOrder = false;
            var cancelOrderedProcedure = false;

            if (labOrderId.HasValue)
            {
                var examinations = await _dbContext.LabExaminations.AsNoTracking()
                    .Where(x => x.LabOrderId == labOrderId.Value && x.ProcedureId == component.ProcedureId && !x.IsDelete
                        && x.ExaminationStatus != LabExaminationStatus.Cancelled && x.ExaminationStatus != LabExaminationStatus.Voided)
                    .Select(x => new { x.Id, x.ValidatedAt, x.ReleasedAt })
                    .ToListAsync(cancellationToken);

                // Penjaga MCU sendiri: Lab hanya menolak pemeriksaan yang sudah dirilis.
                if (examinations.Any(x => x.ValidatedAt != null || x.ReleasedAt != null))
                    return McuResult<McuOrderComponentResponse>.Conflict(E.InvalidTransition, LabValidatedMessage);

                labExaminationIds = examinations.Select(x => x.Id).ToList();

                var labOrderOpen = await _dbContext.LabOrders.AsNoTracking()
                    .AnyAsync(x => x.Id == labOrderId.Value && !x.IsCancel
                        && (x.OrderStatus == LabOrderStatus.Requested || x.OrderStatus == LabOrderStatus.Confirmed), cancellationToken);
                var lastOnLabOrder = !await _dbContext.McuOrderComponents.AsNoTracking()
                    .AnyAsync(x => x.LabOrderId == labOrderId.Value && x.Id != component.Id && !x.IsDelete
                        && x.ComponentStatus != McuOrderComponentStatus.Cancelled, cancellationToken);

                cancelLabOrder = labOrderOpen && lastOnLabOrder;
                cancelOrderedProcedure = !cancelLabOrder && await _dbContext.LabOrderedProcedures.AsNoTracking()
                    .AnyAsync(x => x.LabOrderId == labOrderId.Value && x.ProcedureId == component.ProcedureId && !x.IsDelete
                        && x.OrderedStatus == LabOrderedProcedureStatus.Ordered, cancellationToken);
            }

            var radOrderId = component.ComponentType == McuComponentType.Radiology && component.RadOrderId is { } radId
                && !await _dbContext.RadOrders.AsNoTracking()
                    .AnyAsync(x => x.Id == radId && (x.IsCancel || x.OrderStatus == RadOrderStatus.Cancelled), cancellationToken)
                ? radId
                : (Guid?)null;

            // INT-MCU-09 — refund hanya bila sudah dibayar.
            var refundAmount = component.IsPackageCovered ? component.RefundAmountSnapshot : 0m;
            Guid? refundInvoiceId = null;
            if (refundAmount > 0m)
            {
                var packageItem = order.PackageInvoiceItemId.HasValue
                    ? await _dbContext.BilInvoiceItems.AsNoTracking()
                        .Where(x => x.Id == order.PackageInvoiceItemId.Value && !x.IsDelete)
                        .Select(x => new { x.InvoiceId, x.Status })
                        .SingleOrDefaultAsync(cancellationToken)
                    : null;

                if (packageItem == null)
                {
                    return McuResult<McuOrderComponentResponse>.Conflict(
                        E.InvalidTransition, "Baris paket order ini tidak ditemukan. Hubungi kasir.");
                }

                if (packageItem.Status != BillingInvoiceItemStatuses.Voided
                    && await PaidAmountAsync(packageItem.InvoiceId, cancellationToken) > 0m)
                {
                    refundInvoiceId = packageItem.InvoiceId;
                }
            }

            var partnerReason = $"Batal komponen MCU {order.OrderNumber} {component.ComponentNameSnapshot}: {reason}";
            if (partnerReason.Length > 250)
                partnerReason = partnerReason[..250];

            var now = DateTime.UtcNow;
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (cancelLabOrder)
                {
                    await _labOrderService.CancelAsync(labOrderId!.Value, new CancelLabOrderRequest { CancelReason = partnerReason }, cancellationToken);
                }
                else
                {
                    foreach (var examinationId in labExaminationIds)
                        await _labExaminationService.CancelAsync(examinationId, new CancelLabExaminationRequest { Reason = partnerReason }, cancellationToken);

                    if (cancelOrderedProcedure)
                        await _labExaminationService.CancelOrderedProcedureAsync(labOrderId!.Value, component.ProcedureId!.Value, partnerReason, cancellationToken);
                }

                if (radOrderId.HasValue)
                {
                    var radResult = await _radOrderService.CancelAsync(
                        radOrderId.Value, new RadOrderTransitionRequest { Reason = partnerReason }, cancellationToken);
                    if (!radResult.IsSuccess)
                    {
                        await transaction.RollbackAsync(CancellationToken.None);
                        _dbContext.ChangeTracker.Clear();
                        return McuResult<McuOrderComponentResponse>.Conflict(
                            E.PartnerRejected, $"Pesanan radiologi {component.ComponentNameSnapshot} tidak dapat dibatalkan: {radResult.ErrorMessage}");
                    }
                }

                Guid? refundCaseId = null;
                if (refundInvoiceId.HasValue)
                {
                    var refund = await _refundService.CreateAsync(
                        new CreateRefundRequest
                        {
                            InvoiceId = refundInvoiceId.Value,
                            RefundCategory = "BILLING",
                            SelectedBillingItemIds = [order.PackageInvoiceItemId!.Value],
                            RequestedAmount = refundAmount,
                            Reason = partnerReason,
                            // Billing menolak CorrelationId yang sudah dipakai case lain, sehingga tiap
                            // komponen membawa korelasinya sendiri; order dicatat sebagai penyebab.
                            CorrelationId = component.Id,
                            CausationId = order.Id
                        },
                        component.Id,
                        actorUserId,
                        cancellationToken);
                    refundCaseId = refund.Id;
                }

                // Lab membersihkan pelacak perubahan saat membaca ulang; order dan komponen dimuat di sini,
                // di dalam transaksi, dan versinya diperiksa sekali lagi.
                _dbContext.ChangeTracker.Clear();
                var trackedOrder = await _dbContext.McuOrders.SingleAsync(x => x.Id == orderId, cancellationToken);
                if (trackedOrder.Version != request.Version)
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                    _dbContext.ChangeTracker.Clear();
                    return VersionConflict<McuOrderComponentResponse>();
                }

                var components = await _dbContext.McuOrderComponents
                    .Where(x => x.McuOrderId == orderId && !x.IsDelete)
                    .ToListAsync(cancellationToken);
                var tracked = components.Single(x => x.Id == componentId);

                tracked.ComponentStatus = McuOrderComponentStatus.Cancelled;
                tracked.CancelReason = reason;
                tracked.CancelledByDoctorId = actorDoctorId;
                tracked.RefundCaseId = refundCaseId;
                TouchComponent(tracked, actorUserId, now);

                // MCU-STATE-v1 1.1: komponen terakhir tuntas → Menunggu Resume (sistem).
                if (trackedOrder.OrderStatus == McuOrderStatus.InExamination
                    && components.All(x => McuLabels.IsComponentResolved(x.ComponentStatus)))
                {
                    trackedOrder.OrderStatus = McuOrderStatus.AwaitingResume;
                    AddHistory(trackedOrder.Id, McuOrderStatus.InExamination, McuOrderStatus.AwaitingResume, null,
                        "Semua komponen tuntas.", actorUserId, now);
                }

                Touch(trackedOrder, actorUserId, now);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                _dbContext.ChangeTracker.Clear();
                return VersionConflict<McuOrderComponentResponse>();
            }
            catch (LabExaminationValidationException)
            {
                // Lab menolak 422 hanya untuk hasil yang sudah dirilis — diterjemahkan ke 409 (MCU-STATE-v1 2.1).
                await transaction.RollbackAsync(CancellationToken.None);
                _dbContext.ChangeTracker.Clear();
                return McuResult<McuOrderComponentResponse>.Conflict(E.InvalidTransition, LabValidatedMessage);
            }
            catch (Exception exception) when (ComponentPartnerFailure(exception) is { } message)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                _dbContext.ChangeTracker.Clear();
                return McuResult<McuOrderComponentResponse>.Conflict(E.PartnerRejected, message);
            }

            return await ComponentAsync(orderId, componentId, cancellationToken);
        }

        /// <summary>Penolakan Lab atau Billing saat batal komponen — pesannya diteruskan apa adanya.</summary>
        private static string? ComponentPartnerFailure(Exception exception) => exception switch
        {
            LabExaminationConflictException or LabConcurrencyException or BillingRefundException => exception.Message,
            _ => PartnerFailure(exception)
        };

        // =====================================================================
        // Pesanan Lab/Rad — INT-MCU-05, 06
        // =====================================================================

        /// <summary>
        /// Memesan komponen <i>Menunggu</i> di dalam transaksi pemanggil: Lab per disiplin, Rad per
        /// komponen, Pemeriksaan MCU langsung <i>Siap diisi</i>. Komponen paket memakai jalur bertanda
        /// paket atas nama dokter MCU; komponen di luar paket memakai jalur biasa Lab/Rad.
        /// Mengembalikan pesan penolakan Radiologi, atau <c>null</c> bila semua berhasil; penolakan Lab
        /// dilempar sebagai exception dan diterjemahkan pemanggil.
        /// </summary>
        private async Task<string?> PlaceOrdersAsync(
            McuOrder order,
            Guid encounterId,
            List<McuOrderComponent> components,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            foreach (var group in components.Where(x => x.ComponentType == McuComponentType.Laboratory).GroupBy(x => x.IsPackageCovered))
            {
                var labRequest = new CreateLabOrderByExaminationsRequest
                {
                    EncounterId = encounterId,
                    Examinations = group.Select(x => x.ProcedureId!.Value).Distinct().ToList()
                };

                var created = group.Key
                    ? await _labOrderService.CreateForMcuAsync(labRequest, order.Id, order.McuDoctorId!.Value, cancellationToken)
                    : await _labOrderService.CreateByExaminationsAsync(labRequest, cancellationToken);

                var createdIds = created.Select(x => x.Id).ToList();
                var labOrderByProcedure = await _dbContext.LabOrderedProcedures.AsNoTracking()
                    .Where(x => createdIds.Contains(x.LabOrderId) && !x.IsDelete)
                    .Select(x => new { x.ProcedureId, x.LabOrderId })
                    .ToDictionaryAsync(x => x.ProcedureId, x => x.LabOrderId, cancellationToken);

                foreach (var component in group)
                {
                    component.LabOrderId = labOrderByProcedure[component.ProcedureId!.Value];
                    component.ComponentStatus = McuOrderComponentStatus.Ordered;
                }
            }

            foreach (var component in components.Where(x => x.ComponentType == McuComponentType.Radiology))
            {
                var radRequest = new CreateRadOrderRequest
                {
                    EncounterId = encounterId,
                    ProcedureId = component.ProcedureId!.Value,
                    ModalityId = component.RadModalityId!.Value,
                    ClinicalIndication = $"Medical Check-Up {order.PackageNameSnapshot} ({order.OrderNumber})"
                };

                var created = component.IsPackageCovered
                    ? await _radOrderService.CreateForMcuAsync(radRequest, order.Id, order.McuDoctorId!.Value, cancellationToken)
                    : await _radOrderService.CreateAsync(radRequest, cancellationToken);

                if (!created.IsSuccess)
                    return $"Pesanan radiologi {component.ComponentNameSnapshot} ditolak: {created.ErrorMessage}";

                component.RadOrderId = created.Value!.Id;
                component.ComponentStatus = McuOrderComponentStatus.Ordered;
            }

            foreach (var component in components)
            {
                if (component.ComponentType == McuComponentType.McuExamination)
                    component.ComponentStatus = McuOrderComponentStatus.ReadyToFill;

                TouchComponent(component, actorUserId, now);
            }

            return null;
        }

        // =====================================================================
        // Penolong
        // =====================================================================

        private static DateOnly Today() => DateOnly.FromDateTime(AppDateTimeHelper.OperationalDate());

        /// <summary>Komponen Lab/Rad yang tidak dapat dipesan karena potretnya tidak lengkap.</summary>
        private static McuOrderComponent? IncompleteComponent(IEnumerable<McuOrderComponent> components) =>
            components.FirstOrDefault(x =>
                (x.ComponentType == McuComponentType.Radiology && (!x.ProcedureId.HasValue || !x.RadModalityId.HasValue))
                || (x.ComponentType == McuComponentType.Laboratory && !x.ProcedureId.HasValue));

        private static void Touch(McuOrder order, Guid actorUserId, DateTime now)
        {
            order.Version++;
            order.UpdateDateTime = now;
            order.UpdateBy = actorUserId;
        }

        private static void TouchComponent(McuOrderComponent component, Guid actorUserId, DateTime now)
        {
            if (component.CreateDateTime == now)
                return;

            component.UpdateDateTime = now;
            component.UpdateBy = actorUserId;
        }

        private void AddHistory(
            Guid orderId,
            McuOrderStatus from,
            McuOrderStatus to,
            Guid? changedByUserId,
            string? reason,
            Guid actorUserId,
            DateTime now) =>
            _dbContext.McuOrderStatusHistories.Add(new McuOrderStatusHistory
            {
                McuOrderId = orderId,
                FromStatus = from,
                ToStatus = to,
                ChangedAt = now,
                ChangedByUserId = changedByUserId,
                Reason = reason,
                CreateDateTime = now,
                CreateBy = actorUserId
            });

        /// <summary><c>MCU-STATE-v1</c> bagian 1.1–1.2 untuk mulai pemeriksaan; <c>null</c> bila order <i>Hadir</i>.</summary>
        private static string? StatusBlockMessage(McuOrderStatus status) => status switch
        {
            McuOrderStatus.CheckedIn => null,
            McuOrderStatus.Scheduled => "Peserta belum check-in.",
            McuOrderStatus.NoShow => "Order sudah Tidak Hadir. Jadwalkan ulang lebih dulu.",
            McuOrderStatus.Cancelled => "Order sudah dibatalkan.",
            _ => $"Order berstatus {McuLabels.OrderStatus(status)} tidak dapat dimulai pemeriksaannya."
        };

        /// <summary>Pesan penolakan Lab yang diteruskan apa adanya dengan kode status kontrak MCU.</summary>
        private static string? PartnerFailure(Exception exception) => exception switch
        {
            LabOrderValidationException or LabOrderConflictException or KeyNotFoundException or ArgumentException => exception.Message,
            _ => null
        };

        private async Task<McuResult<McuOrderDetailResponse>> DetailAsync(Guid orderId, CancellationToken cancellationToken) =>
            McuResult<McuOrderDetailResponse>.Ok((await _queryService.GetOrderAsync(orderId, cancellationToken))!);

        private async Task<McuResult<McuOrderComponentResponse>> ComponentAsync(Guid orderId, Guid componentId, CancellationToken cancellationToken) =>
            McuResult<McuOrderComponentResponse>.Ok(
                (await _queryService.GetComponentsAsync(orderId, cancellationToken)).Single(x => x.Id == componentId));

        private static McuResult<T> VersionConflict<T>() =>
            McuResult<T>.Conflict(E.Val46, "Data order sudah diubah petugas lain. Muat ulang lalu coba lagi.");
    }
}
