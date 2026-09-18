using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;
using System.Globalization;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    /// <summary>
    /// Pelaksanaan protokol sliding scale insulin oleh perawat — <c>BE-RWI-123</c>, <c>FR-KEP-075</c>, <c>FR-KEP-076</c>,
    /// <c>INT-KEP-11</c>, <c>VAL-KEP-27</c> s.d. <c>VAL-KEP-29</c>, <c>RWI-DEC-148</c>, arsitektur 0.4 bagian 11.5.12.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Task paling berbahaya pada roadmap keperawatan.</b> Salah pada langkah mana pun berarti dosis insulin yang salah.
    /// Tiga penjagaan ditegakkan sekaligus: pelaksanaan <b>ditolak</b> tanpa order aktif; dosis <b>hanya</b> dihitung dari GDS
    /// bangsal yang <b>satuannya sama</b> dengan protokol — tanpa konversi; dan GDS, dosis MAR, serta catatan pelaksanaan
    /// tersimpan dalam <b>satu transaksi idempoten</b>.
    /// </para>
    /// <para>
    /// <b>Contoh.</b> Order Budi v2 disesuaikan separuh. Pukul 11.00 perawat mengetik GDS 280 mg/dL → pratinjau rentang
    /// 250–300, 3 unit. Simpan → satu GDS, satu dosis MAR 3 unit, satu pelaksanaan. Insulin high-alert → dosis menunggu cek
    /// ganda; pelaksanaan tetap <c>Recorded</c> karena perhitungannya sudah terjadi. Tombol Simpan tertekan dua kali dengan
    /// <c>Idempotency-Key</c> yang sama → pelaksanaan dan dosis yang sama dikembalikan, bukan dosis kedua.
    /// </para>
    /// <para>
    /// <b>Gerbang produksi.</b> Pemakaian sliding scale disetujui (<c>RWI-DEC-155</c>), tetapi nama pengesah isi protokol
    /// belum ada (<c>RWI-OQ-097</c>). Selama itu tidak ada versi template yang dapat disahkan, sehingga tidak ada order aktif
    /// dan service ini menolak setiap pelaksanaan dengan <c>VAL-KEP-27</c>.
    /// </para>
    /// </remarks>
    public class SlidingScaleExecutionService
    {
        private const string LogCategory = "HealthServices.Pharmacy.SlidingScaleExecution";
        private const string SequenceKey = "PHM_SLIDING_SCALE_EXECUTION";
        private const string NumberPrefix = "SSX";

        public const string AlasanRentangNol = "GDS di bawah rentang pemberian";
        public const string PesanGagalSimpan = "Pencatatan sliding scale gagal disimpan. Belum ada yang tercatat; ulangi.";

        private readonly ApplicationDbContext _dbContext;
        private readonly NursingEpisodeWriteGuard _writeGuard;
        private readonly DailyMonitoringService _dailyMonitoringService;
        private readonly MedicationAdministrationService _medicationAdministrationService;
        private readonly AccessPermissionService _accessPermissionService;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly LoggerService _loggerService;

        public SlidingScaleExecutionService(
            ApplicationDbContext dbContext,
            NursingEpisodeWriteGuard writeGuard,
            DailyMonitoringService dailyMonitoringService,
            MedicationAdministrationService medicationAdministrationService,
            AccessPermissionService accessPermissionService,
            NumberSeriesAllocator numberSeriesAllocator,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _writeGuard = writeGuard;
            _dailyMonitoringService = dailyMonitoringService;
            _medicationAdministrationService = medicationAdministrationService;
            _accessPermissionService = accessPermissionService;
            _numberSeriesAllocator = numberSeriesAllocator;
            _loggerService = loggerService;
        }

        private sealed class OrderContext
        {
            public PhmSlidingScaleOrder Order { get; init; } = null!;

            public PhmSlidingScaleOrderVersion Version { get; init; } = null!;

            public BloodGlucoseUnit ProtocolUnit { get; init; }

            public List<PhmSlidingScaleRange> Ranges { get; init; } = new();

            public PhmPrescriptionItem Item { get; init; } = null!;
        }

        // =====================================================================
        // Pratinjau — tanpa menyimpan
        // =====================================================================

        public async Task<NursingResult<SlidingScalePreviewResponse>> PreviewAsync(
            PreviewSlidingScaleExecutionRequest request,
            CancellationToken cancellationToken = default)
        {
            var konteks = await LoadActiveOrderAsync<SlidingScalePreviewResponse>(request.OrderId, cancellationToken);

            if (konteks.Gagal != null)
                return konteks.Gagal;

            var order = konteks.Nilai!;
            decimal nilai;
            BloodGlucoseUnit satuan;

            if (request.BloodGlucoseReadingId.HasValue)
            {
                var gds = await LoadWardReadingAsync<SlidingScalePreviewResponse>(request.BloodGlucoseReadingId.Value, order.Order.InpEpisodeId, cancellationToken);

                if (gds.Gagal != null)
                    return gds.Gagal;

                nilai = gds.Nilai!.GlucoseValue;
                satuan = gds.Nilai.GlucoseUnit;
            }
            else
            {
                if (!request.GlucoseUnit.HasValue || !Enum.IsDefined(request.GlucoseUnit.Value))
                    return BadRequest<SlidingScalePreviewResponse>("Pilih satuan gula darah: mg/dL atau mmol/L.", "GLUCOSE_UNIT_REQUIRED");

                if (!request.GlucoseValue.HasValue)
                    return BadRequest<SlidingScalePreviewResponse>("Isi nilai gula darah.", "GLUCOSE_VALUE_REQUIRED");

                nilai = request.GlucoseValue.Value;
                satuan = request.GlucoseUnit.Value;
            }

            var hitung = Compute<SlidingScalePreviewResponse>(order, nilai, satuan);

            if (hitung.Gagal != null)
                return hitung.Gagal;

            var rentang = hitung.Nilai!;

            return NursingResult<SlidingScalePreviewResponse>.Ok(new SlidingScalePreviewResponse
            {
                OrderId = order.Order.Id,
                OrderNumber = order.Order.OrderNumber,
                OrderVersionId = order.Version.Id,
                OrderVersionNumber = order.Version.VersionNumber,
                IsOrderAdjusted = order.Version.IsAdjusted,
                ProtocolGlucoseUnit = order.ProtocolUnit,
                ProtocolGlucoseUnitLabel = DailyMonitoringService.GlucoseUnitLabel(order.ProtocolUnit),
                GlucoseValue = nilai,
                GlucoseUnit = satuan,
                BloodGlucoseReadingId = request.BloodGlucoseReadingId,
                MatchedRange = new SlidingScaleRangeResponse
                {
                    Id = rentang.Id,
                    LowerBoundInclusive = rentang.LowerBoundInclusive,
                    UpperBoundExclusive = rentang.UpperBoundExclusive,
                    DoseUnits = rentang.DoseUnits,
                    InstructionText = rentang.InstructionText,
                    RequiresPhysicianNotification = rentang.RequiresPhysicianNotification,
                    SortOrder = rentang.SortOrder
                },
                MatchedRangeLabel = RangeLabel(rentang),
                ComputedDoseUnits = rentang.DoseUnits,
                IsZeroDose = rentang.DoseUnits == 0,
                InstructionText = rentang.InstructionText,
                RequiresPhysicianNotification = rentang.RequiresPhysicianNotification,
                IsHighAlert = order.Item.IsHighAlertSnapshot,
                PrescriptionItemId = order.Item.Id,
                DrugName = order.Item.DrugNameSnapshot,
                Route = order.Item.RouteSnapshot
            }, "Pratinjau dosis sliding scale berhasil dihitung.");
        }

        // =====================================================================
        // Pelaksanaan — satu transaksi
        // =====================================================================

        public async Task<NursingResult<SlidingScaleExecutionResponse>> ExecuteAsync(
            CreateSlidingScaleExecutionRequest request,
            string? idempotencyKey,
            ClaimsPrincipal user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            // VAL-KEP-29e — kunci wajib: tombol ganda tidak boleh memberi insulin dua kali.
            var kunci = NursingEpisodeWriteGuard.NormalizeIdempotencyKey(idempotencyKey ?? request.IdempotencyKey);

            if (kunci == null)
                return BadRequest<SlidingScaleExecutionResponse>("Permintaan tidak dapat diproses. Muat ulang halaman.", "IDEMPOTENCY_KEY_REQUIRED");

            var ulang = await FindReplayAsync(kunci, actorUserId, cancellationToken);

            if (ulang != null)
                return ulang;

            // Kewenangan ganda — api-contract 7.12: butir pelaksanaan diperiksa [AccessPermission], butir pencatatan MAR di sini.
            if (!await _accessPermissionService.HasAccessAsync(user, "MedicationAdministration", "Create"))
                return NursingResult<SlidingScaleExecutionResponse>.Fail(StatusCodes.Status403Forbidden,
                    "Anda tidak berwenang mencatat pemberian obat.", "MEDICATION_ADMINISTRATION_CREATE_REQUIRED");

            var konteksOrder = await LoadActiveOrderAsync<SlidingScaleExecutionResponse>(request.OrderId, cancellationToken);

            if (konteksOrder.Gagal != null)
                return konteksOrder.Gagal;

            var order = konteksOrder.Nilai!;

            var penjaga = await _writeGuard.EnsureCanWriteAsync(order.Order.InpEpisodeId, user, actorUserId, cancellationToken);

            if (!penjaga.IsSuccess)
                return penjaga.Cast<SlidingScaleExecutionResponse>();

            var konteks = penjaga.Value!;

            // VAL-KEP-29c
            if (!request.ExpectedOrderVersionNumber.HasValue || request.ExpectedOrderVersionNumber.Value != order.Order.CurrentVersionNumber)
                return Conflict<SlidingScaleExecutionResponse>("Dokter sudah menyesuaikan protokol. Periksa dosis baru sebelum memberi.", "ORDER_VERSION_CHANGED");

            var adaBaru = request.NewReading != null;

            if (adaBaru == request.BloodGlucoseReadingId.HasValue)
                return BadRequest<SlidingScaleExecutionResponse>("Pilih salah satu: GDS baru, atau GDS bangsal yang sudah tercatat.", "READING_SOURCE_INVALID");

            decimal nilai;
            BloodGlucoseUnit satuan;

            if (adaBaru)
            {
                if (!request.NewReading!.GlucoseUnit.HasValue || !Enum.IsDefined(request.NewReading.GlucoseUnit.Value))
                    return BadRequest<SlidingScaleExecutionResponse>("Pilih satuan gula darah: mg/dL atau mmol/L.", "GLUCOSE_UNIT_REQUIRED");

                if (!request.NewReading.GlucoseValue.HasValue)
                    return BadRequest<SlidingScaleExecutionResponse>("Isi nilai gula darah.", "GLUCOSE_VALUE_REQUIRED");

                nilai = request.NewReading.GlucoseValue.Value;
                satuan = request.NewReading.GlucoseUnit.Value;
            }
            else
            {
                var gds = await LoadWardReadingAsync<SlidingScaleExecutionResponse>(request.BloodGlucoseReadingId!.Value, order.Order.InpEpisodeId, cancellationToken);

                if (gds.Gagal != null)
                    return gds.Gagal;

                nilai = gds.Nilai!.GlucoseValue;
                satuan = gds.Nilai.GlucoseUnit;
            }

            // VAL-KEP-28/29a dan pencocokan rentang dari versi order yang berlaku.
            var hitung = Compute<SlidingScaleExecutionResponse>(order, nilai, satuan);

            if (hitung.Gagal != null)
                return hitung.Gagal;

            var rentang = hitung.Nilai!;
            var dosisHitung = rentang.DoseUnits;
            var dosisAktual = request.ActualDoseUnits ?? dosisHitung;

            if (dosisAktual < 0)
                return BadRequest<SlidingScaleExecutionResponse>("Dosis aktual tidak boleh negatif.", "ACTUAL_DOSE_NEGATIVE");

            var pengecualian = dosisAktual != dosisHitung;

            // VAL-KEP-29d
            if (pengecualian && string.IsNullOrWhiteSpace(request.ExceptionReason))
                return BadRequest<SlidingScaleExecutionResponse>("Dosis berbeda dari hitungan protokol. Isi alasan pengecualian.", "EXCEPTION_REASON_REQUIRED");

            var now = DateTime.UtcNow;
            var diberikan = dosisAktual > 0;
            var diberikanPada = request.AdministeredAt.HasValue ? MedicationAdministrationService.AsUtc(request.AdministeredAt.Value) : now;
            var rute = string.IsNullOrWhiteSpace(request.ActualRoute) ? order.Item.RouteSnapshot : request.ActualRoute.Trim();

            if (diberikan)
            {
                if (string.IsNullOrWhiteSpace(rute))
                    return BadRequest<SlidingScaleExecutionResponse>("Isi dosis, rute, dan waktu pemberian.", "ADMINISTRATION_FIELDS_REQUIRED");

                if (diberikanPada > now.AddMinutes(MedicationAdministrationService.FutureToleranceMinutes))
                    return BadRequest<SlidingScaleExecutionResponse>("Waktu pemberian tidak boleh di masa depan.", "ADMINISTERED_AT_IN_FUTURE");
            }

            string nomorPelaksanaan;
            string? nomorDosis = null;

            try
            {
                nomorPelaksanaan = await _numberSeriesAllocator.AllocateAsync(
                    new NumberAllocationRequest(SequenceKey, NumberPrefix, NumberSeriesResetPolicies.Daily, 4, actorUserId, DateTimeOffset.UtcNow),
                    cancellationToken);

                if (!request.DoseSlotAdministrationId.HasValue)
                    nomorDosis = await _medicationAdministrationService.AllocateNumberAsync(actorUserId, cancellationToken);
            }
            catch (NumberSeriesAllocationException)
            {
                return NursingResult<SlidingScaleExecutionResponse>.Fail(StatusCodes.Status422UnprocessableEntity, PesanGagalSimpan, "NUMBER_ALLOCATION_FAILED");
            }

            await using var transaksi = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Langkah 2 — GDS baru ditulis sekali lewat pemiliknya, atau GDS terpilih dikunci dan diperiksa ulang.
                Guid gdsId;

                if (adaBaru)
                {
                    var dibuat = _dailyMonitoringService.AddGlucoseReading(
                        konteks, request.NewReading!.MeasuredAt, request.NewReading.GlucoseValue, request.NewReading.GlucoseUnit, null, actorUserId);

                    if (!dibuat.IsSuccess)
                        return dibuat.Cast<SlidingScaleExecutionResponse>();

                    gdsId = dibuat.Value!.Id;
                }
                else
                {
                    var terkunci = await _dbContext.Set<CliBloodGlucoseReading>()
                        .FromSqlInterpolated($@"SELECT * FROM public.""CliBloodGlucoseReading"" WHERE ""Id"" = {request.BloodGlucoseReadingId!.Value} AND ""IsDelete"" = false FOR UPDATE")
                        .AsNoTracking()
                        .FirstOrDefaultAsync(cancellationToken);

                    if (terkunci == null || terkunci.ReadingStatus != ClinicalMeasurementStatus.Active ||
                        terkunci.GlucoseValue != nilai || terkunci.GlucoseUnit != satuan)
                        return Conflict<SlidingScaleExecutionResponse>("GDS berubah saat disimpan. Muat ulang dan periksa dosis.", "READING_CHANGED");

                    var dipakai = await ReadingUsedAtAsync(terkunci.Id, cancellationToken);

                    if (dipakai.HasValue)
                        return Conflict<SlidingScaleExecutionResponse>(
                            $"Gula darah ini sudah dipakai pukul {HospitalTimeZone.ToLocal(dipakai.Value):HH.mm}. Ukur ulang untuk pemberian berikutnya.",
                            "READING_ALREADY_USED");

                    gdsId = terkunci.Id;
                }

                // Langkah 5 — dosis MAR SlidingScale: slot Due bila dikirim, jika tidak baris baru.
                PhmMedicationAdministration dosis;

                if (request.DoseSlotAdministrationId.HasValue)
                {
                    var slot = await _dbContext.Set<PhmMedicationAdministration>()
                        .FromSqlInterpolated($@"SELECT * FROM public.""PhmMedicationAdministration"" WHERE ""Id"" = {request.DoseSlotAdministrationId.Value} AND ""IsDelete"" = false FOR UPDATE")
                        .FirstOrDefaultAsync(cancellationToken);

                    if (slot == null || slot.PrescriptionItemId != order.Item.Id || slot.InpEpisodeId != konteks.EpisodeId ||
                        slot.DoseStatus != MedicationDoseStatus.Due || slot.DoubleCheckStatus == MedicationDoubleCheckStatus.Pending)
                        return Conflict<SlidingScaleExecutionResponse>("Slot dosis ini tidak dapat dipakai untuk pelaksanaan sliding scale.", "DOSE_SLOT_INVALID");

                    dosis = slot;
                }
                else
                {
                    dosis = new PhmMedicationAdministration
                    {
                        Id = Guid.NewGuid(),
                        AdministrationNumber = nomorDosis!,
                        PrescriptionId = order.Order.PrescriptionId,
                        PrescriptionItemId = order.Item.Id,
                        EncounterId = konteks.EncounterId,
                        InpEpisodeId = konteks.EpisodeId,
                        PatientId = konteks.PatientId,
                        DrugId = order.Item.DrugId,
                        DoseSource = MedicationDoseSource.SlidingScale,
                        ScheduledAt = null,
                        IsHighAlertSnapshot = order.Item.IsHighAlertSnapshot,
                        RevisionNumber = 0,
                        CreateDateTime = now,
                        CreateBy = actorUserId
                    };

                    _dbContext.Set<PhmMedicationAdministration>().Add(dosis);
                }

                dosis.PlannedDose = dosisHitung;
                dosis.PlannedDoseUnitSnapshot = "unit";
                dosis.RecordedByEmployeeId = konteks.ActorEmployeeId;
                dosis.RecordedByUserId = actorUserId;
                dosis.RecordedAt = now;
                dosis.DeviationNote = pengecualian ? Truncate(request.ExceptionReason, 500) : null;
                dosis.DoubleCheckedByEmployeeId = null;
                dosis.DoubleCheckedByUserId = null;
                dosis.DoubleCheckedAt = null;
                dosis.DoubleCheckNote = null;
                dosis.UpdateDateTime = request.DoseSlotAdministrationId.HasValue ? now : null;
                dosis.UpdateBy = request.DoseSlotAdministrationId.HasValue ? actorUserId : Guid.Empty;

                if (diberikan)
                {
                    dosis.ActualDose = dosisAktual;
                    dosis.ActualDoseUnitSnapshot = "unit";
                    dosis.ActualRouteSnapshot = Truncate(rute, 100);
                    dosis.AdministeredAt = diberikanPada;
                    dosis.StatusReason = null;

                    // INT-KEP-11 — insulin high-alert menunggu cek ganda; pelaksanaan tetap Recorded.
                    dosis.DoseStatus = order.Item.IsHighAlertSnapshot ? MedicationDoseStatus.Due : MedicationDoseStatus.Administered;
                    dosis.DoubleCheckStatus = order.Item.IsHighAlertSnapshot ? MedicationDoubleCheckStatus.Pending : MedicationDoubleCheckStatus.NotRequired;
                }
                else
                {
                    // FR-KEP-076, usulan G-22 — rentang 0 unit (atau pengecualian 0 unit) mencatat Held beralasan.
                    dosis.ActualDose = null;
                    dosis.ActualDoseUnitSnapshot = null;
                    dosis.ActualRouteSnapshot = null;
                    dosis.AdministeredAt = null;
                    dosis.DoseStatus = MedicationDoseStatus.Held;
                    dosis.DoubleCheckStatus = MedicationDoubleCheckStatus.NotRequired;
                    dosis.StatusReason = dosisHitung == 0 ? AlasanRentangNol : Truncate(request.ExceptionReason, 500);
                }

                // Langkah 6 — catatan pelaksanaan.
                var pelaksanaan = new PhmSlidingScaleExecution
                {
                    Id = Guid.NewGuid(),
                    ExecutionNumber = nomorPelaksanaan,
                    OrderId = order.Order.Id,
                    OrderVersionId = order.Version.Id,
                    InpEpisodeId = konteks.EpisodeId,
                    BloodGlucoseReadingId = gdsId,
                    GlucoseValueSnapshot = nilai,
                    GlucoseUnitSnapshot = satuan,
                    MatchedRangeId = rentang.Id,
                    ComputedDoseUnits = dosisHitung,
                    MedicationAdministrationId = dosis.Id,
                    IsException = pengecualian,
                    ExceptionReason = pengecualian ? Truncate(request.ExceptionReason, 500) : null,
                    ReadingCorrectedAfterExecution = false,
                    ExecutedByEmployeeId = konteks.ActorEmployeeId,
                    ExecutedByUserId = actorUserId,
                    ExecutedAt = now,
                    ExecutionStatus = SlidingScaleExecutionStatus.Recorded,
                    IdempotencyKey = kunci,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

                _dbContext.Set<PhmSlidingScaleExecution>().Add(pelaksanaan);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaksi.CommitAsync(cancellationToken);

                // ExceptionReason sensitif — tidak masuk payload logger.
                await _loggerService.InfoAsync(LogCategory, "SlidingScaleExecution.Execute", "Perawat mencatat pelaksanaan sliding scale.",
                    new
                    {
                        pelaksanaan.Id,
                        pelaksanaan.ExecutionNumber,
                        pelaksanaan.OrderId,
                        OrderVersionNumber = order.Version.VersionNumber,
                        pelaksanaan.BloodGlucoseReadingId,
                        pelaksanaan.ComputedDoseUnits,
                        ActualDoseUnits = dosisAktual,
                        pelaksanaan.IsException,
                        MedicationAdministrationId = dosis.Id,
                        dosis.DoseStatus,
                        dosis.DoubleCheckStatus,
                        ExecutedBy = actorUserId
                    });

                return NursingResult<SlidingScaleExecutionResponse>.Ok(
                    await ToResponseAsync(pelaksanaan.Id, actorUserId, false, cancellationToken),
                    dosis.DoubleCheckStatus == MedicationDoubleCheckStatus.Pending
                        ? "Pelaksanaan sliding scale tercatat. Dosis insulin menunggu cek ganda perawat kedua."
                        : "Pelaksanaan sliding scale berhasil dicatat.",
                    StatusCodes.Status201Created);
            }
            catch (DbUpdateException)
            {
                await transaksi.RollbackAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();

                // Kiriman ulang serentak berkunci sama menang lebih dulu → kembalikan hasilnya.
                var pemenang = await FindReplayAsync(kunci, actorUserId, cancellationToken);

                if (pemenang != null)
                    return pemenang;

                if (request.BloodGlucoseReadingId.HasValue && (await ReadingUsedAtAsync(request.BloodGlucoseReadingId.Value, cancellationToken)).HasValue)
                    return Conflict<SlidingScaleExecutionResponse>("Gula darah ini sudah dipakai pelaksanaan lain. Ukur ulang untuk pemberian berikutnya.", "READING_ALREADY_USED");

                return NursingResult<SlidingScaleExecutionResponse>.Fail(StatusCodes.Status409Conflict, PesanGagalSimpan, "EXECUTION_NOT_SAVED");
            }
        }

        // =====================================================================
        // Baca
        // =====================================================================

        public async Task<NursingResult<List<SlidingScaleExecutionListItem>>> GetByEpisodeAsync(
            Guid episodeId,
            DateTime? from,
            DateTime? to,
            CancellationToken cancellationToken = default)
        {
            var sampai = to.HasValue ? MedicationAdministrationService.AsUtc(to.Value) : DateTime.UtcNow;
            var dari = from.HasValue ? MedicationAdministrationService.AsUtc(from.Value) : sampai.AddDays(-7);

            if (dari > sampai)
                return BadRequest<List<SlidingScaleExecutionListItem>>("Waktu awal tidak boleh setelah waktu akhir.");

            var ids = await _dbContext.Set<PhmSlidingScaleExecution>().AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete && x.ExecutedAt >= dari && x.ExecutedAt <= sampai)
                .OrderByDescending(x => x.ExecutedAt)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            var hasil = new List<SlidingScaleExecutionListItem>();

            foreach (var id in ids)
                hasil.Add(await ToResponseAsync(id, null, false, cancellationToken, includeAdministration: false));

            return NursingResult<List<SlidingScaleExecutionListItem>>.Ok(hasil, "Riwayat pelaksanaan sliding scale berhasil diambil.");
        }

        public async Task<NursingResult<SlidingScaleExecutionResponse>> GetAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var ada = await _dbContext.Set<PhmSlidingScaleExecution>().AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            return ada
                ? NursingResult<SlidingScaleExecutionResponse>.Ok(await ToResponseAsync(id, actorUserId, false, cancellationToken), "Detail pelaksanaan sliding scale berhasil diambil.")
                : NursingResult<SlidingScaleExecutionResponse>.Fail(StatusCodes.Status404NotFound, "Pelaksanaan sliding scale tidak ditemukan.");
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        /// <summary>
        /// Order aktif beserta versi berlaku, satuan protokol, rentang, dan butir resep. <c>VAL-KEP-27</c>: order tidak aktif,
        /// butir dihentikan, atau butir bukan dosis skala → <c>409</c>.
        /// </summary>
        private async Task<(OrderContext? Nilai, NursingResult<T>? Gagal)> LoadActiveOrderAsync<T>(Guid orderId, CancellationToken cancellationToken)
        {
            var order = await _dbContext.Set<PhmSlidingScaleOrder>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);

            if (order == null)
                return (null, NursingResult<T>.Fail(StatusCodes.Status404NotFound, "Protokol sliding scale tidak ditemukan."));

            var item = await _dbContext.Set<PhmPrescriptionItem>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == order.PrescriptionItemId && !x.IsDelete, cancellationToken);

            if (order.OrderStatus != SlidingScaleOrderStatus.Active || !order.IsActive || item == null || item.IsStopped ||
                item.DoseKind != PrescriptionDoseKind.SlidingScale)
                return (null, Conflict<T>("Tidak ada protokol sliding scale aktif untuk pasien ini. Hubungi dokter.", "SLIDING_SCALE_ORDER_NOT_ACTIVE"));

            var versi = await _dbContext.Set<PhmSlidingScaleOrderVersion>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.OrderId == order.Id && !x.IsDelete && x.VersionNumber == order.CurrentVersionNumber, cancellationToken);

            if (versi == null)
                return (null, Conflict<T>("Tidak ada protokol sliding scale aktif untuk pasien ini. Hubungi dokter.", "SLIDING_SCALE_ORDER_NOT_ACTIVE"));

            var satuan = await _dbContext.Set<PhmSlidingScaleTemplateVersion>().AsNoTracking()
                .Where(x => x.Id == versi.TemplateVersionId)
                .Select(x => (BloodGlucoseUnit?)x.GlucoseUnit)
                .FirstOrDefaultAsync(cancellationToken);

            if (!satuan.HasValue)
                return (null, Conflict<T>("Satuan protokol sliding scale tidak terbaca. Hubungi dokter.", "PROTOCOL_UNIT_MISSING"));

            var rentang = await _dbContext.Set<PhmSlidingScaleRange>().AsNoTracking()
                .Where(x => x.OrderVersionId == versi.Id && !x.IsDelete)
                .OrderBy(x => x.LowerBoundInclusive)
                .ToListAsync(cancellationToken);

            return (new OrderContext { Order = order, Version = versi, ProtocolUnit = satuan.Value, Ranges = rentang, Item = item }, null);
        }

        /// <summary><c>VAL-KEP-28</c> — hanya GDS bangsal aktif milik episode yang sama; <c>VAL-KEP-29b</c> — belum dipakai.</summary>
        private async Task<(CliBloodGlucoseReading? Nilai, NursingResult<T>? Gagal)> LoadWardReadingAsync<T>(Guid readingId, Guid episodeId, CancellationToken cancellationToken)
        {
            var gds = await _dbContext.Set<CliBloodGlucoseReading>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == readingId && !x.IsDelete, cancellationToken);

            if (gds == null || gds.ReadingStatus != ClinicalMeasurementStatus.Active || gds.InpEpisodeId != episodeId || gds.Method != BloodGlucoseMethod.WardGlucometer)
                return (null, NursingResult<T>.Fail(StatusCodes.Status422UnprocessableEntity, "Dosis insulin hanya dihitung dari GDS bangsal yang dicatat perawat.", "WARD_READING_REQUIRED"));

            var dipakai = await ReadingUsedAtAsync(gds.Id, cancellationToken);

            if (dipakai.HasValue)
                return (null, Conflict<T>($"Gula darah ini sudah dipakai pukul {HospitalTimeZone.ToLocal(dipakai.Value):HH.mm}. Ukur ulang untuk pemberian berikutnya.", "READING_ALREADY_USED"));

            return (gds, null);
        }

        /// <summary><c>VAL-KEP-29a</c> satuan sama tanpa konversi, lalu rentang <c>[bawah, atas)</c> yang memuat nilai.</summary>
        private static (PhmSlidingScaleRange? Nilai, NursingResult<T>? Gagal) Compute<T>(OrderContext order, decimal value, BloodGlucoseUnit unit)
        {
            if (unit != order.ProtocolUnit)
                return (null, Conflict<T>(
                    $"Satuan gula darah ({DailyMonitoringService.GlucoseUnitLabel(unit)}) berbeda dari satuan protokol ({DailyMonitoringService.GlucoseUnitLabel(order.ProtocolUnit)}). Periksa ulang; sistem tidak mengonversi.",
                    "GLUCOSE_UNIT_MISMATCH"));

            var cocok = order.Ranges
                .Where(r => (!r.LowerBoundInclusive.HasValue || value >= r.LowerBoundInclusive.Value) &&
                            (!r.UpperBoundExclusive.HasValue || value < r.UpperBoundExclusive.Value))
                .ToList();

            if (cocok.Count != 1)
                return (null, NursingResult<T>.Fail(StatusCodes.Status422UnprocessableEntity,
                    $"GDS {value.ToString("0.##", CultureInfo.GetCultureInfo("id-ID"))} {DailyMonitoringService.GlucoseUnitLabel(unit)} tidak masuk tepat satu rentang protokol. Hubungi dokter.",
                    "NO_MATCHING_RANGE"));

            return (cocok[0], null);
        }

        private async Task<DateTime?> ReadingUsedAtAsync(Guid readingId, CancellationToken cancellationToken) =>
            await _dbContext.Set<PhmSlidingScaleExecution>().AsNoTracking()
                .Where(x => x.BloodGlucoseReadingId == readingId && x.ExecutionStatus == SlidingScaleExecutionStatus.Recorded && !x.IsDelete)
                .Select(x => (DateTime?)x.ExecutedAt)
                .FirstOrDefaultAsync(cancellationToken);

        private async Task<NursingResult<SlidingScaleExecutionResponse>?> FindReplayAsync(string kunci, Guid actorUserId, CancellationToken cancellationToken)
        {
            var id = await _dbContext.Set<PhmSlidingScaleExecution>().AsNoTracking()
                .Where(x => x.IdempotencyKey == kunci && !x.IsDelete)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (!id.HasValue)
                return null;

            return NursingResult<SlidingScaleExecutionResponse>.Ok(
                await ToResponseAsync(id.Value, actorUserId, true, cancellationToken),
                "Pelaksanaan sliding scale ini sudah tercatat sebelumnya.",
                isReplay: true);
        }

        private async Task<SlidingScaleExecutionResponse> ToResponseAsync(
            Guid id,
            Guid? actorUserId,
            bool isReplay,
            CancellationToken cancellationToken,
            bool includeAdministration = true)
        {
            var x = await _dbContext.Set<PhmSlidingScaleExecution>().AsNoTracking().FirstAsync(e => e.Id == id, cancellationToken);

            var order = await _dbContext.Set<PhmSlidingScaleOrder>().AsNoTracking()
                .Where(o => o.Id == x.OrderId).Select(o => new { o.OrderNumber }).FirstOrDefaultAsync(cancellationToken);

            var versi = await _dbContext.Set<PhmSlidingScaleOrderVersion>().AsNoTracking()
                .Where(v => v.Id == x.OrderVersionId).Select(v => (int?)v.VersionNumber).FirstOrDefaultAsync(cancellationToken);

            var rentang = await _dbContext.Set<PhmSlidingScaleRange>().AsNoTracking().FirstOrDefaultAsync(r => r.Id == x.MatchedRangeId, cancellationToken);

            var dosis = await _dbContext.Set<PhmMedicationAdministration>().AsNoTracking()
                .Where(d => d.Id == x.MedicationAdministrationId)
                .Select(d => new { d.DoseStatus, d.DoubleCheckStatus, d.ActualDose })
                .FirstOrDefaultAsync(cancellationToken);

            var pelaksana = await _dbContext.Set<MstEmployee>().AsNoTracking()
                .Where(e => e.Id == x.ExecutedByEmployeeId).Select(e => e.FullName).FirstOrDefaultAsync(cancellationToken);

            var response = new SlidingScaleExecutionResponse
            {
                Id = x.Id,
                ExecutionNumber = x.ExecutionNumber,
                OrderId = x.OrderId,
                OrderNumber = order?.OrderNumber,
                OrderVersionNumber = versi,
                InpEpisodeId = x.InpEpisodeId,
                ExecutedAt = x.ExecutedAt,
                GlucoseValueSnapshot = x.GlucoseValueSnapshot,
                GlucoseUnitSnapshot = x.GlucoseUnitSnapshot,
                GlucoseUnitLabel = DailyMonitoringService.GlucoseUnitLabel(x.GlucoseUnitSnapshot),
                MatchedRangeLabel = rentang == null ? null : RangeLabel(rentang),
                ComputedDoseUnits = x.ComputedDoseUnits,
                ActualDoseUnits = dosis?.ActualDose,
                IsException = x.IsException,
                ExecutedByEmployeeId = x.ExecutedByEmployeeId,
                ExecutedByName = pelaksana,
                ExecutionStatus = x.ExecutionStatus,
                ExecutionStatusLabel = x.ExecutionStatus == SlidingScaleExecutionStatus.Cancelled ? "Dibatalkan" : "Tercatat",
                ReadingCorrectedAfterExecution = x.ReadingCorrectedAfterExecution,
                MedicationAdministrationId = x.MedicationAdministrationId,
                DoseStatus = dosis?.DoseStatus ?? MedicationDoseStatus.Due,
                DoubleCheckStatus = dosis?.DoubleCheckStatus ?? MedicationDoubleCheckStatus.NotRequired,
                DoseStatusLabel = dosis == null
                    ? string.Empty
                    : dosis.DoseStatus == MedicationDoseStatus.Due && dosis.DoubleCheckStatus == MedicationDoubleCheckStatus.Pending
                        ? "Menunggu cek ganda"
                        : MedicationAdministrationService.DoseStatusLabel(dosis.DoseStatus),
                OrderVersionId = x.OrderVersionId,
                BloodGlucoseReadingId = x.BloodGlucoseReadingId,
                MatchedRangeId = x.MatchedRangeId,
                InstructionText = rentang?.InstructionText,
                RequiresPhysicianNotification = rentang?.RequiresPhysicianNotification ?? false,
                ExceptionReason = x.ExceptionReason,
                ExecutedByUserId = x.ExecutedByUserId,
                IsReplay = isReplay
            };

            if (includeAdministration && actorUserId.HasValue)
            {
                var detail = await _medicationAdministrationService.GetAsync(x.MedicationAdministrationId, actorUserId.Value, cancellationToken);
                response.Administration = detail.Value;
            }

            return response;
        }

        private static string RangeLabel(PhmSlidingScaleRange r) =>
            SlidingScaleRangeValidator.Label(new SlidingScaleRangeRequest
            {
                LowerBoundInclusive = r.LowerBoundInclusive,
                UpperBoundExclusive = r.UpperBoundExclusive,
                DoseUnits = r.DoseUnits
            });

        private static string? Truncate(string? value, int max) =>
            string.IsNullOrWhiteSpace(value) ? null : (value.Trim().Length > max ? value.Trim()[..max] : value.Trim());

        private static NursingResult<T> BadRequest<T>(string message, string? code = null) =>
            NursingResult<T>.Fail(StatusCodes.Status400BadRequest, message, code);

        private static NursingResult<T> Conflict<T>(string message, string code) =>
            NursingResult<T>.Fail(StatusCodes.Status409Conflict, message, code);
    }
}
