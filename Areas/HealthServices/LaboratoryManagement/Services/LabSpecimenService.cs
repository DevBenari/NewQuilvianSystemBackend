using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Alur operasional sampel laboratorium dari perencanaan sampai penetapan layak atau tolak.
    ///
    /// Batas kewenangan yang ditegakkan service ini berasal dari <c>RJ-BIL-GATE-DEC-003</c> dan
    /// keputusan author <c>RJ-BIL-OQ-008</c> sampai <c>OQ-011</c>:
    ///
    /// <list type="bullet">
    /// <item>Hanya penetapan layak yang menerbitkan fakta kelayakan tagih. Perencanaan,
    /// pengambilan, dan penerimaan fisik tidak pernah menerbitkan apa pun.</item>
    /// <item>Penolakan tidak menghasilkan tagihan pemeriksaan.</item>
    /// <item>Kelayakan dinilai per sampel, bukan per pesanan, sehingga dua komponen yang layak
    /// tetap dapat ditagih walaupun komponen ketiga ditolak.</item>
    /// <item>Laboratorium tidak memiliki kewenangan finansial apa pun. Tidak ada Paid,
    /// Settlement, PayerApproval, Void, Refund, maupun Reversal di sini.</item>
    /// </list>
    ///
    /// Fakta klinis selalu diterbitkan setelah perubahan klinis tersimpan, tidak pernah di
    /// dalam transaksi yang masih terbuka. Billing yang tidak dapat dihubungi tidak boleh
    /// membatalkan penetapan layak yang secara klinis sudah benar terjadi.
    /// </summary>
    public class LabSpecimenService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        /// <summary>Satuan fakta Lab: satu sampel mewakili satu komponen pemeriksaan.</summary>
        private const string ExaminationUnit = "Pemeriksaan";

        private const int MaxBarcodeAllocationAttempts = 3;

        /// <summary>
        /// Status sampel yang tidak lagi dapat dipindahkan oleh alur operasional.
        /// </summary>
        private static readonly LabSpecimenStatus[] TerminalSpecimenStatuses =
        {
            LabSpecimenStatus.Cancelled,
            LabSpecimenStatus.RecollectionRequired
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly ClinicalMilestoneFactProducer _clinicalMilestoneFactProducer;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabSpecimenService(
            ApplicationDbContext dbContext,
            ClinicalMilestoneFactProducer clinicalMilestoneFactProducer,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _clinicalMilestoneFactProducer = clinicalMilestoneFactProducer;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Merencanakan satu sampel sekaligus satu komponen pemeriksaan pada sebuah pesanan.
        ///
        /// Tarif komponen disalin di sini, bukan pada saat penetapan layak, agar muatan fakta
        /// yang dikirim ke Billing dapat direproduksi persis ketika pengiriman diulang.
        /// </summary>
        public async Task<LabSpecimenActionResult> PlanAsync(
            Guid labOrderId,
            PlanLabSpecimenRequest request,
            CancellationToken cancellationToken = default)
        {
            var order = await LoadOrderAsync(labOrderId, cancellationToken);

            if (order.OrderStatus is LabOrderStatus.Cancelled or LabOrderStatus.Completed)
            {
                throw new InvalidOperationException(
                    $"Pesanan laboratorium berstatus {order.OrderStatus} tidak dapat menerima sampel baru.");
            }

            if (order.OrderStatus == LabOrderStatus.OnHold)
                throw new InvalidOperationException("Pesanan laboratorium sedang ditahan.");

            var procedureId = request.ProcedureId.GetValueOrDefault() == Guid.Empty
                ? order.ProcedureId
                : request.ProcedureId!.Value;

            var procedure = await _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == procedureId &&
                    x.IsLaboratory &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            if (procedure == null)
            {
                throw new ArgumentException(
                    "Procedure komponen pemeriksaan tidak ditemukan, tidak aktif, atau bukan procedure laboratorium.");
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var nextSequence = await _dbContext.TrxLabSpecimens
                .Where(x => x.LabOrderId == order.Id && !x.IsDelete)
                .Select(x => (int?)x.SpecimenSequence)
                .MaxAsync(cancellationToken) ?? 0;

            var tariff = await ResolveTariffAsync(procedureId, now, cancellationToken);

            var specimen = await CreateSpecimenAsync(
                order,
                procedure,
                tariff,
                nextSequence + 1,
                request.SpecimenDescription,
                supersededSpecimenId: null,
                recollectionCause: null,
                recollectionReason: null,
                actorUserId,
                now,
                cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabSpecimen.Plan",
                "Merencanakan sampel laboratorium.",
                new { specimen.Id, specimen.LabOrderId, specimen.SpecimenSequence, ActorUserId = actorUserId });

            return new LabSpecimenActionResult(specimen, null);
        }

        public Task<LabSpecimenActionResult> CollectAsync(
            Guid specimenId,
            CollectLabSpecimenRequest request,
            CancellationToken cancellationToken = default) =>
            MoveOperationalStatusAsync(
                specimenId,
                LabSpecimenStatus.Planned,
                LabSpecimenStatus.Collected,
                "Specimen.Collect",
                request.Note,
                cancellationToken);

        public Task<LabSpecimenActionResult> ReceiveAsync(
            Guid specimenId,
            ReceiveLabSpecimenRequest request,
            CancellationToken cancellationToken = default) =>
            MoveOperationalStatusAsync(
                specimenId,
                LabSpecimenStatus.Collected,
                LabSpecimenStatus.Received,
                "Specimen.Receive",
                request.Note,
                cancellationToken);

        /// <summary>
        /// Menyatakan sampel layak periksa. Inilah satu-satunya titik pada modul Laboratorium
        /// yang menerbitkan fakta kelayakan tagih.
        ///
        /// Pemanggilan ulang terhadap sampel yang sudah layak tidak mengubah keadaan dan
        /// memakai kembali waktu keputusan yang tersimpan, sehingga fakta yang dikirim identik
        /// dan Billing mengenalinya sebagai pengiriman ulang, bukan revisi baru.
        /// </summary>
        public async Task<LabSpecimenActionResult> AcceptAsync(
            Guid specimenId,
            AcceptLabSpecimenRequest request,
            CancellationToken cancellationToken = default)
        {
            var specimen = await LoadSpecimenAsync(specimenId, cancellationToken);
            var order = specimen.LabOrder!;

            EnsureOrderUsable(order);

            var actorUserId = GetCurrentUserId();

            if (specimen.SpecimenStatus == LabSpecimenStatus.Accepted)
            {
                // Pengulangan yang aman: keadaan tidak disentuh, fakta dikirim ulang dengan
                // waktu keputusan yang sama.
                var replay = await EmitChargeEligibilityAsync(specimen, order, actorUserId, cancellationToken);
                return new LabSpecimenActionResult(specimen, replay);
            }

            if (specimen.SpecimenStatus != LabSpecimenStatus.Received)
            {
                throw new InvalidOperationException(
                    $"Sampel berstatus {specimen.SpecimenStatus} tidak dapat dinyatakan layak. " +
                    "Penetapan layak hanya berlaku atas sampel yang sudah diterima laboratorium.");
            }

            var now = DateTime.UtcNow;
            var fromStatus = specimen.SpecimenStatus;

            specimen.SpecimenStatus = LabSpecimenStatus.Accepted;
            specimen.DecidedAt = now;
            specimen.DecidedByUserId = actorUserId;
            specimen.UpdateDateTime = now;
            specimen.UpdateBy = actorUserId;
            specimen.Version++;

            AppendHistory(
                order,
                specimen,
                LabTransitionScope.LabSpecimen,
                "Specimen.Accept",
                fromStatus.ToString(),
                LabSpecimenStatus.Accepted.ToString(),
                reasonCode: null,
                reasonNote: request.Note,
                actorUserId,
                now);

            // Pesanan mengikuti sampel pertama yang dinyatakan layak. Turunan ini dicatat
            // sebagai inferensi pada execution evidence, bukan aturan yang tertulis eksplisit.
            if (order.OrderStatus is LabOrderStatus.Draft or LabOrderStatus.Requested)
            {
                var orderFrom = order.OrderStatus;
                order.OrderStatus = LabOrderStatus.Accepted;
                order.UpdateDateTime = now;
                order.UpdateBy = actorUserId;
                order.Version++;

                AppendHistory(
                    order,
                    specimen: null,
                    LabTransitionScope.LabOrder,
                    "Order.Accept",
                    orderFrom.ToString(),
                    LabOrderStatus.Accepted.ToString(),
                    reasonCode: null,
                    reasonNote: "Mengikuti sampel pertama yang dinyatakan layak.",
                    actorUserId,
                    now);
            }

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabSpecimen.Accept",
                "Menyatakan sampel laboratorium layak periksa.",
                new { specimen.Id, specimen.LabOrderId, ActorUserId = actorUserId });

            var handoff = await EmitChargeEligibilityAsync(specimen, order, actorUserId, cancellationToken);

            return new LabSpecimenActionResult(specimen, handoff);
        }

        /// <summary>
        /// Menolak sampel dengan alasan dari katalog. Penolakan tidak pernah menerbitkan fakta
        /// kelayakan tagih, sehingga tidak ada tagihan pemeriksaan yang terbentuk.
        /// </summary>
        public async Task<LabSpecimenActionResult> RejectAsync(
            Guid specimenId,
            RejectLabSpecimenRequest request,
            CancellationToken cancellationToken = default)
        {
            var specimen = await LoadSpecimenAsync(specimenId, cancellationToken);
            var order = specimen.LabOrder!;

            EnsureOrderUsable(order);

            if (specimen.SpecimenStatus != LabSpecimenStatus.Received)
            {
                throw new InvalidOperationException(
                    $"Sampel berstatus {specimen.SpecimenStatus} tidak dapat ditolak. " +
                    "Penolakan hanya berlaku atas sampel yang sudah diterima laboratorium.");
            }

            var reasonCode = request.ReasonCode?.Trim();
            if (string.IsNullOrWhiteSpace(reasonCode))
                throw new ArgumentException("Kode alasan penolakan wajib diisi.");

            var reason = await _dbContext.MstLabRejectionReasons
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.ReasonCode == reasonCode &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            if (reason == null)
            {
                throw new ArgumentException(
                    "Kode alasan penolakan tidak dikenal atau sudah tidak aktif.");
            }

            var note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

            if (reason.RequiresNote && string.IsNullOrWhiteSpace(note))
            {
                throw new ArgumentException(
                    $"Alasan '{reason.ReasonCode}' mewajibkan catatan tambahan.");
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var fromStatus = specimen.SpecimenStatus;

            specimen.SpecimenStatus = LabSpecimenStatus.Rejected;
            specimen.DecidedAt = now;
            specimen.DecidedByUserId = actorUserId;
            specimen.RejectionReasonId = reason.Id;
            specimen.RejectionReasonCode = reason.ReasonCode;
            specimen.RejectionNote = note;
            specimen.UpdateDateTime = now;
            specimen.UpdateBy = actorUserId;
            specimen.Version++;

            AppendHistory(
                order,
                specimen,
                LabTransitionScope.LabSpecimen,
                "Specimen.Reject",
                fromStatus.ToString(),
                LabSpecimenStatus.Rejected.ToString(),
                reason.ReasonCode,
                note,
                actorUserId,
                now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabSpecimen.Reject",
                "Menolak sampel laboratorium.",
                new
                {
                    specimen.Id,
                    specimen.LabOrderId,
                    reason.ReasonCode,
                    reason.IsInternalHospitalError,
                    ActorUserId = actorUserId
                });

            // Tidak ada fakta yang diterbitkan. Penolakan menghasilkan nol tagihan pemeriksaan.
            return new LabSpecimenActionResult(specimen, null);
        }

        /// <summary>
        /// Meminta pengambilan ulang atas sampel yang ditolak.
        ///
        /// Sampel lama tidak dihapus dan tidak diubah alasannya; ia berpindah ke
        /// <c>RecollectionRequired</c> dan tetap menjadi asal-usul sampel penggantinya. Sampel
        /// baru memperoleh identitas dan barcode baru.
        ///
        /// Sebab pengambilan ulang ikut dibawa ke Billing sebagai keterangan, bukan sebagai
        /// keputusan finansial. Kesalahan internal rumah sakit tidak pernah otomatis menambah
        /// tanggungan pasien di sini.
        /// </summary>
        public async Task<LabSpecimenActionResult> RequestRecollectionAsync(
            Guid specimenId,
            RequestLabRecollectionRequest request,
            CancellationToken cancellationToken = default)
        {
            var specimen = await LoadSpecimenAsync(specimenId, cancellationToken);
            var order = specimen.LabOrder!;

            EnsureOrderUsable(order);

            if (specimen.SpecimenStatus != LabSpecimenStatus.Rejected)
            {
                throw new InvalidOperationException(
                    $"Sampel berstatus {specimen.SpecimenStatus} tidak dapat diambil ulang. " +
                    "Pengambilan ulang hanya berlaku atas sampel yang ditolak.");
            }

            if (request.Cause == null)
                throw new ArgumentException("Sebab pengambilan ulang wajib diisi.");

            var cause = request.Cause.Value;

            if (!Enum.IsDefined(cause))
                throw new ArgumentException("Sebab pengambilan ulang tidak dikenal.");

            var reasonText = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();

            // RJ-BIL-GATE-DEC-003: pengambilan ulang karena kondisi pasien atau sebab eksternal
            // memerlukan alasan dan otorisasi sebelum tagihan baru dipertimbangkan. Kesalahan
            // internal tidak menuntut otorisasi karena akibatnya memang tidak dibebankan pasien.
            if (cause != LabRecollectionCause.InternalHospitalError && string.IsNullOrWhiteSpace(reasonText))
            {
                throw new ArgumentException(
                    "Pengambilan ulang karena kondisi pasien atau sebab eksternal wajib menyertakan alasan.");
            }

            var procedure = await _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == specimen.ProcedureId && !x.IsDelete, cancellationToken);

            if (procedure == null)
                throw new ArgumentException("Procedure komponen pemeriksaan tidak ditemukan.");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var fromStatus = specimen.SpecimenStatus;

            specimen.SpecimenStatus = LabSpecimenStatus.RecollectionRequired;
            specimen.RecollectionCause = cause;
            specimen.RecollectionReason = reasonText;
            specimen.RecollectionAuthorizedByUserId = actorUserId;
            specimen.RecollectionAuthorizedAt = now;
            specimen.UpdateDateTime = now;
            specimen.UpdateBy = actorUserId;
            specimen.Version++;

            AppendHistory(
                order,
                specimen,
                LabTransitionScope.LabSpecimen,
                "Specimen.RequestRecollection",
                fromStatus.ToString(),
                LabSpecimenStatus.RecollectionRequired.ToString(),
                specimen.RejectionReasonCode,
                reasonText,
                actorUserId,
                now);

            var nextSequence = await _dbContext.TrxLabSpecimens
                .Where(x => x.LabOrderId == order.Id && !x.IsDelete)
                .Select(x => (int?)x.SpecimenSequence)
                .MaxAsync(cancellationToken) ?? 0;

            var tariff = await ResolveTariffAsync(specimen.ProcedureId, now, cancellationToken);

            var replacement = await CreateSpecimenAsync(
                order,
                procedure,
                tariff,
                nextSequence + 1,
                specimen.SpecimenDescription,
                supersededSpecimenId: specimen.Id,
                recollectionCause: cause,
                recollectionReason: reasonText,
                actorUserId,
                now,
                cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabSpecimen.RequestRecollection",
                "Meminta pengambilan ulang sampel laboratorium.",
                new
                {
                    RejectedSpecimenId = specimen.Id,
                    ReplacementSpecimenId = replacement.Id,
                    Cause = cause.ToString(),
                    ActorUserId = actorUserId
                });

            return new LabSpecimenActionResult(replacement, null);
        }

        /// <summary>
        /// Menahan sampel sambil mempertahankan status operasional sebelumnya.
        /// </summary>
        public async Task<LabSpecimenActionResult> HoldAsync(
            Guid specimenId,
            HoldLabRequest request,
            CancellationToken cancellationToken = default)
        {
            var specimen = await LoadSpecimenAsync(specimenId, cancellationToken);
            var order = specimen.LabOrder!;

            EnsureOrderUsable(order);

            if (specimen.SpecimenStatus == LabSpecimenStatus.OnHold)
                throw new InvalidOperationException("Sampel sudah ditahan.");

            if (Array.IndexOf(TerminalSpecimenStatuses, specimen.SpecimenStatus) >= 0)
            {
                throw new InvalidOperationException(
                    $"Sampel berstatus {specimen.SpecimenStatus} tidak dapat ditahan.");
            }

            var reason = request.Reason?.Trim();
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Alasan penahanan wajib diisi.");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var fromStatus = specimen.SpecimenStatus;

            specimen.StatusBeforeHold = fromStatus;
            specimen.SpecimenStatus = LabSpecimenStatus.OnHold;
            specimen.UpdateDateTime = now;
            specimen.UpdateBy = actorUserId;
            specimen.Version++;

            AppendHistory(
                order,
                specimen,
                LabTransitionScope.LabSpecimen,
                "Specimen.Hold",
                fromStatus.ToString(),
                LabSpecimenStatus.OnHold.ToString(),
                reasonCode: null,
                reasonNote: reason,
                actorUserId,
                now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            return new LabSpecimenActionResult(specimen, null);
        }

        public async Task<LabSpecimenActionResult> ResumeAsync(
            Guid specimenId,
            ResumeLabRequest request,
            CancellationToken cancellationToken = default)
        {
            var specimen = await LoadSpecimenAsync(specimenId, cancellationToken);
            var order = specimen.LabOrder!;

            EnsureOrderUsable(order);

            if (specimen.SpecimenStatus != LabSpecimenStatus.OnHold)
                throw new InvalidOperationException("Sampel tidak sedang ditahan.");

            var resumeTo = specimen.StatusBeforeHold ?? LabSpecimenStatus.Planned;

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            specimen.SpecimenStatus = resumeTo;
            specimen.StatusBeforeHold = null;
            specimen.UpdateDateTime = now;
            specimen.UpdateBy = actorUserId;
            specimen.Version++;

            AppendHistory(
                order,
                specimen,
                LabTransitionScope.LabSpecimen,
                "Specimen.Resume",
                LabSpecimenStatus.OnHold.ToString(),
                resumeTo.ToString(),
                reasonCode: null,
                reasonNote: string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                actorUserId,
                now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            return new LabSpecimenActionResult(specimen, null);
        }

        /// <summary>
        /// Membatalkan sampel secara klinis.
        ///
        /// Pembatalan klinis bukan pembatalan finansial. Bila sampel belum pernah dinyatakan
        /// layak, tidak ada apa pun yang perlu dikoreksi di Billing. Bila sudah pernah layak,
        /// yang dikirim adalah revisi baru atas fakta yang sama sehingga tagihan lama tetap
        /// utuh dan Billing yang memutuskan koreksinya.
        /// </summary>
        public async Task<LabSpecimenActionResult> CancelAsync(
            Guid specimenId,
            CancelLabSpecimenRequest request,
            CancellationToken cancellationToken = default)
        {
            var specimen = await LoadSpecimenAsync(specimenId, cancellationToken);
            var order = specimen.LabOrder!;

            if (specimen.SpecimenStatus == LabSpecimenStatus.Cancelled)
                throw new InvalidOperationException("Sampel sudah dibatalkan.");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var fromStatus = specimen.SpecimenStatus;
            var reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();

            CancelSpecimenInMemory(order, specimen, reason, actorUserId, now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabSpecimen.Cancel",
                "Membatalkan sampel laboratorium.",
                new { specimen.Id, specimen.LabOrderId, FromStatus = fromStatus.ToString(), ActorUserId = actorUserId });

            var handoff = fromStatus == LabSpecimenStatus.Accepted
                ? await EmitClinicalCancellationAsync(specimen, order, actorUserId, cancellationToken)
                : null;

            return new LabSpecimenActionResult(specimen, handoff);
        }

        public async Task<List<LabSpecimenResponse>> GetByOrderAsync(
            Guid labOrderId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.TrxLabSpecimens
                .AsNoTracking()
                .Where(x => x.LabOrderId == labOrderId && !x.IsDelete)
                .OrderBy(x => x.SpecimenSequence)
                .Select(x => new LabSpecimenResponse
                {
                    Id = x.Id,
                    LabOrderId = x.LabOrderId,
                    ProcedureId = x.ProcedureId,
                    SpecimenBarcode = x.SpecimenBarcode,
                    SpecimenSequence = x.SpecimenSequence,
                    SpecimenDescription = x.SpecimenDescription,
                    SpecimenStatus = x.SpecimenStatus.ToString(),
                    ProcedureCode = x.ProcedureCodeSnapshot,
                    ProcedureName = x.ProcedureNameSnapshot,
                    UnitPrice = x.UnitPriceSnapshot,
                    CollectedAt = x.CollectedAt,
                    ReceivedAt = x.ReceivedAt,
                    DecidedAt = x.DecidedAt,
                    RejectionReasonCode = x.RejectionReasonCode,
                    RejectionNote = x.RejectionNote,
                    SupersededSpecimenId = x.SupersededSpecimenId,
                    RecollectionCause = x.RecollectionCause != null ? x.RecollectionCause.ToString() : null,
                    Version = x.Version
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<LabTransitionHistoryResponse>> GetHistoryAsync(
            Guid labOrderId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.TrxLabTransitionHistories
                .AsNoTracking()
                .Where(x => x.LabOrderId == labOrderId)
                .OrderBy(x => x.OccurredAt)
                .ThenBy(x => x.CreateDateTime)
                .Select(x => new LabTransitionHistoryResponse
                {
                    Id = x.Id,
                    LabOrderId = x.LabOrderId,
                    LabSpecimenId = x.LabSpecimenId,
                    Scope = x.Scope.ToString(),
                    Action = x.Action,
                    FromStatus = x.FromStatus,
                    ToStatus = x.ToStatus,
                    ReasonCode = x.ReasonCode,
                    ReasonNote = x.ReasonNote,
                    ActorUserId = x.ActorUserId,
                    OccurredAt = x.OccurredAt
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<LabRejectionReasonResponse>> GetRejectionReasonsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.MstLabRejectionReasons
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ReasonCode)
                .Select(x => new LabRejectionReasonResponse
                {
                    Id = x.Id,
                    ReasonCode = x.ReasonCode,
                    ReasonName = x.ReasonName,
                    Description = x.Description,
                    IsInternalHospitalError = x.IsInternalHospitalError,
                    RequiresNote = x.RequiresNote,
                    SortOrder = x.SortOrder
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Membatalkan seluruh sampel yang masih berjalan pada satu pesanan, dipakai ketika
        /// pesanannya sendiri dibatalkan. Mengembalikan sampel yang sebelumnya sudah dinyatakan
        /// layak agar pemanggil dapat menerbitkan fakta pembatalannya setelah penyimpanan.
        /// </summary>
        internal async Task<List<TrxLabSpecimen>> CancelAllForOrderInMemoryAsync(
            LabOrder order,
            string? reason,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var specimens = await _dbContext.TrxLabSpecimens
                .Where(x => x.LabOrderId == order.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);

            var previouslyAccepted = new List<TrxLabSpecimen>();

            foreach (var specimen in specimens)
            {
                if (specimen.SpecimenStatus == LabSpecimenStatus.Cancelled)
                    continue;

                if (specimen.SpecimenStatus == LabSpecimenStatus.Accepted)
                    previouslyAccepted.Add(specimen);

                CancelSpecimenInMemory(order, specimen, reason, actorUserId, now);
            }

            return previouslyAccepted;
        }

        internal async Task<ClinicalFactEmissionResult> EmitClinicalCancellationAsync(
            TrxLabSpecimen specimen,
            LabOrder order,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var request = BuildFactRequest(
                specimen,
                order,
                specimen.CancelDateTime ?? DateTime.UtcNow,
                includeTariffSnapshot: false);

            return await _clinicalMilestoneFactProducer.EmitClinicalCancellationAsync(
                request,
                actorUserId,
                cancellationToken);
        }

        private async Task<ClinicalFactEmissionResult> EmitChargeEligibilityAsync(
            TrxLabSpecimen specimen,
            LabOrder order,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var request = BuildFactRequest(
                specimen,
                order,
                specimen.DecidedAt ?? DateTime.UtcNow,
                includeTariffSnapshot: true);

            return await _clinicalMilestoneFactProducer.EmitChargeEligibilityAsync(
                request,
                actorUserId,
                cancellationToken);
        }

        /// <summary>
        /// Menyusun muatan fakta klinis.
        ///
        /// Seluruh nilai berasal dari baris yang sudah tersimpan, bukan dari pembacaan ulang
        /// master data, agar pengiriman ulang menghasilkan muatan yang sama persis. Pembagian
        /// penjamin sengaja tidak disertakan karena kepemilikannya ada pada Billing.
        /// </summary>
        private static ClinicalMilestoneFactRequest BuildFactRequest(
            TrxLabSpecimen specimen,
            LabOrder order,
            DateTime occurredAt,
            bool includeTariffSnapshot)
        {
            return new ClinicalMilestoneFactRequest
            {
                SourceContext = BillingSourceContract.LaboratorySourceContext,
                SourceAggregateId = order.Id,
                SourceItemId = specimen.Id,
                EffectType = BillingSourceContract.LaboratoryChargeEffectType,
                EncounterId = order.EncounterId,
                OccurredAt = occurredAt,
                Quantity = includeTariffSnapshot ? 1m : null,
                Unit = includeTariffSnapshot ? ExaminationUnit : null,
                TariffSnapshot = includeTariffSnapshot
                    ? JsonSerializer.Serialize(new
                    {
                        source = "LaboratorySnapshot",
                        procedureCode = specimen.ProcedureCodeSnapshot,
                        procedureName = specimen.ProcedureNameSnapshot,
                        tariffCode = specimen.TariffCodeSnapshot,
                        unitPrice = specimen.UnitPriceSnapshot
                    })
                    : null,
                RuleSnapshot = JsonSerializer.Serialize(new
                {
                    milestone = includeTariffSnapshot ? "SpecimenAccepted" : "SpecimenCancelled",
                    specimenBarcode = specimen.SpecimenBarcode,
                    specimenSequence = specimen.SpecimenSequence,
                    supersededSpecimenId = specimen.SupersededSpecimenId,
                    recollectionCause = specimen.RecollectionCause?.ToString()
                }),
                CorrelationId = order.Id
            };
        }

        private async Task<TrxLabSpecimen> CreateSpecimenAsync(
            LabOrder order,
            MstProcedure procedure,
            MstTariff? tariff,
            int sequence,
            string? description,
            Guid? supersededSpecimenId,
            LabRecollectionCause? recollectionCause,
            string? recollectionReason,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var specimen = new TrxLabSpecimen
            {
                Id = Guid.NewGuid(),
                LabOrderId = order.Id,
                ProcedureId = procedure.Id,
                SpecimenBarcode = GenerateSpecimenBarcode(),
                SpecimenSequence = sequence,
                SpecimenDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                ProcedureCodeSnapshot = procedure.ProcedureCode,
                ProcedureNameSnapshot = procedure.ProcedureName,
                TariffId = tariff?.Id,
                TariffCodeSnapshot = tariff?.TariffCode,
                UnitPriceSnapshot = tariff?.NormalPrice,
                SpecimenStatus = LabSpecimenStatus.Planned,
                SupersededSpecimenId = supersededSpecimenId,
                RecollectionCause = recollectionCause,
                RecollectionReason = recollectionReason,
                RecollectionAuthorizedByUserId = recollectionCause != null ? actorUserId : null,
                RecollectionAuthorizedAt = recollectionCause != null ? now : null,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.TrxLabSpecimens.Add(specimen);

            AppendHistory(
                order,
                specimen,
                LabTransitionScope.LabSpecimen,
                supersededSpecimenId == null ? "Specimen.Plan" : "Specimen.PlanRecollection",
                fromStatus: null,
                LabSpecimenStatus.Planned.ToString(),
                reasonCode: null,
                reasonNote: recollectionReason,
                actorUserId,
                now);

            for (var attempt = 1; attempt <= MaxBarcodeAllocationAttempts; attempt++)
            {
                try
                {
                    await SaveWithConcurrencyGuardAsync(cancellationToken);
                    return specimen;
                }
                catch (DbUpdateException exception) when (IsUniqueViolation(exception) &&
                                                          attempt < MaxBarcodeAllocationAttempts)
                {
                    // Barcode kembar praktis mustahil karena dibangkitkan dari GUID, tetapi
                    // keunikan tetap ditegakkan database dan tabrakan tetap ditangani daripada
                    // dibiarkan menjadi kegagalan yang tidak jelas sebabnya.
                    //
                    // Hanya barcode-nya yang diganti. Baris sampel dan baris riwayatnya tetap
                    // entity yang sama dan masih berstatus Added, sehingga percobaan ulang tidak
                    // menggandakan riwayat.
                    specimen.SpecimenBarcode = GenerateSpecimenBarcode();
                }
            }

            throw new InvalidOperationException(
                "Gagal mengalokasikan barcode sampel yang unik. Silakan ulangi permintaan.");
        }

        private async Task<LabSpecimenActionResult> MoveOperationalStatusAsync(
            Guid specimenId,
            LabSpecimenStatus expectedFrom,
            LabSpecimenStatus target,
            string action,
            string? note,
            CancellationToken cancellationToken)
        {
            var specimen = await LoadSpecimenAsync(specimenId, cancellationToken);
            var order = specimen.LabOrder!;

            EnsureOrderUsable(order);

            if (specimen.SpecimenStatus != expectedFrom)
            {
                throw new InvalidOperationException(
                    $"Sampel berstatus {specimen.SpecimenStatus} tidak dapat dipindahkan ke {target}. " +
                    $"Tindakan ini hanya berlaku atas sampel berstatus {expectedFrom}.");
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            specimen.SpecimenStatus = target;
            specimen.UpdateDateTime = now;
            specimen.UpdateBy = actorUserId;
            specimen.Version++;

            if (target == LabSpecimenStatus.Collected)
            {
                specimen.CollectedAt = now;
                specimen.CollectedByUserId = actorUserId;
            }
            else if (target == LabSpecimenStatus.Received)
            {
                specimen.ReceivedAt = now;
                specimen.ReceivedByUserId = actorUserId;
            }

            AppendHistory(
                order,
                specimen,
                LabTransitionScope.LabSpecimen,
                action,
                expectedFrom.ToString(),
                target.ToString(),
                reasonCode: null,
                reasonNote: string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
                actorUserId,
                now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            return new LabSpecimenActionResult(specimen, null);
        }

        private void CancelSpecimenInMemory(
            LabOrder order,
            TrxLabSpecimen specimen,
            string? reason,
            Guid actorUserId,
            DateTime now)
        {
            var fromStatus = specimen.SpecimenStatus;

            specimen.SpecimenStatus = LabSpecimenStatus.Cancelled;
            specimen.IsCancel = true;
            specimen.CancelDateTime = now;
            specimen.CancelBy = actorUserId;
            specimen.UpdateDateTime = now;
            specimen.UpdateBy = actorUserId;
            specimen.Version++;

            AppendHistory(
                order,
                specimen,
                LabTransitionScope.LabSpecimen,
                "Specimen.Cancel",
                fromStatus.ToString(),
                LabSpecimenStatus.Cancelled.ToString(),
                reasonCode: null,
                reasonNote: reason,
                actorUserId,
                now);
        }

        /// <summary>
        /// Menambah satu baris riwayat. Dipakai bersama oleh alur pesanan dan alur sampel;
        /// keduanya berbagi <see cref="ApplicationDbContext"/> yang sama dalam satu request,
        /// sehingga riwayat pesanan dan riwayat sampel tersimpan dalam satu penyimpanan atomik.
        /// </summary>
        internal void AppendHistory(
            LabOrder order,
            TrxLabSpecimen? specimen,
            LabTransitionScope scope,
            string action,
            string? fromStatus,
            string toStatus,
            string? reasonCode,
            string? reasonNote,
            Guid actorUserId,
            DateTime occurredAt)
        {
            _dbContext.TrxLabTransitionHistories.Add(new TrxLabTransitionHistory
            {
                Id = Guid.NewGuid(),
                LabOrderId = order.Id,
                LabSpecimenId = specimen?.Id,
                EncounterId = order.EncounterId,
                Scope = scope,
                Action = action,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                ReasonCode = reasonCode,
                ReasonNote = reasonNote,
                ActorUserId = actorUserId,
                OccurredAt = occurredAt,
                CorrelationId = order.Id,
                CreateDateTime = occurredAt,
                CreateBy = actorUserId
            });
        }

        private async Task<LabOrder> LoadOrderAsync(Guid labOrderId, CancellationToken cancellationToken)
        {
            var order = await _dbContext.LabOrders
                .FirstOrDefaultAsync(x => x.Id == labOrderId && !x.IsDelete, cancellationToken);

            if (order == null)
                throw new KeyNotFoundException("Pesanan laboratorium tidak ditemukan.");

            return order;
        }

        private async Task<TrxLabSpecimen> LoadSpecimenAsync(Guid specimenId, CancellationToken cancellationToken)
        {
            var specimen = await _dbContext.TrxLabSpecimens
                .Include(x => x.LabOrder)
                .FirstOrDefaultAsync(x => x.Id == specimenId && !x.IsDelete, cancellationToken);

            if (specimen?.LabOrder == null || specimen.LabOrder.IsDelete)
                throw new KeyNotFoundException("Sampel laboratorium tidak ditemukan.");

            return specimen;
        }

        private static void EnsureOrderUsable(LabOrder order)
        {
            if (order.OrderStatus == LabOrderStatus.Cancelled)
                throw new InvalidOperationException("Pesanan laboratorium sudah dibatalkan.");

            if (order.OrderStatus == LabOrderStatus.OnHold)
                throw new InvalidOperationException("Pesanan laboratorium sedang ditahan.");
        }

        private async Task<MstTariff?> ResolveTariffAsync(
            Guid procedureId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<MstTariff>()
                .AsNoTracking()
                .Where(x =>
                    x.ProcedureId == procedureId &&
                    !x.IsDelete &&
                    (x.EffectiveStartDate == null || x.EffectiveStartDate <= now) &&
                    (x.EffectiveEndDate == null || x.EffectiveEndDate >= now))
                .OrderByDescending(x => x.EffectiveStartDate)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task SaveWithConcurrencyGuardAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new LabConcurrencyException(
                    "Data laboratorium sudah diubah oleh petugas lain. Muat ulang lalu ulangi tindakan Anda.");
            }
        }

        private static bool IsUniqueViolation(DbUpdateException exception) =>
            exception.InnerException is Npgsql.PostgresException { SqlState: "23505" };

        /// <summary>
        /// Barcode operasional tanpa makna: awalan tetap diikuti 32 karakter heksadesimal dari
        /// GUID baru. Tidak memuat identitas pasien atau informasi klinis apa pun.
        /// </summary>
        private static string GenerateSpecimenBarcode() =>
            $"LSP-{Guid.NewGuid():N}".ToUpperInvariant();

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }

    /// <summary>
    /// Hasil satu tindakan operasional beserta ringkasan penyerahan fakta ke Billing bila
    /// tindakan tersebut memang menerbitkan fakta.
    /// </summary>
    public sealed record LabSpecimenActionResult(
        TrxLabSpecimen Specimen,
        ClinicalFactEmissionResult? Handoff);

    /// <summary>
    /// Ditandai terpisah agar controller dapat membalas <c>409 Conflict</c> dan bukan
    /// <c>400 Bad Request</c> ketika dua petugas mengubah data yang sama bersamaan.
    /// </summary>
    public sealed class LabConcurrencyException : Exception
    {
        public LabConcurrencyException(string message) : base(message)
        {
        }
    }
}
