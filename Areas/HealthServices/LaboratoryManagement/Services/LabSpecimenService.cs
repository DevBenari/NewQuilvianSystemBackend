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

            // VAL-06.
            if (order.OrderStatus == LabOrderStatus.Cancelled)
            {
                throw new LabSpecimenConflictException(
                    "Pesanan ini sudah dibatalkan, wadah baru tidak dapat ditambahkan.");
            }

            if (order.OrderStatus == LabOrderStatus.Completed)
            {
                throw new LabSpecimenConflictException(
                    "Pesanan laboratorium ini sudah selesai, wadah baru tidak dapat ditambahkan.");
            }

            if (order.OrderStatus == LabOrderStatus.OnHold)
                throw new InvalidOperationException("Pesanan laboratorium sedang ditahan.");

            // Daftar pemeriksaan yang akan ditopang wadah ini. Ruas Examinations adalah jalur
            // utama sejak LAB-DEC-024; ProcedureId dan procedure pesanan dipertahankan sebagai
            // jalur ringkas satu pemeriksaan bagi pemanggil lama.
            var procedureIds = request.Examinations?
                .Where(x => x != Guid.Empty)
                .ToList() ?? new List<Guid>();

            if (procedureIds.Count == 0)
            {
                var tunggal = request.ProcedureId.GetValueOrDefault() == Guid.Empty
                    ? order.ProcedureId
                    : request.ProcedureId!.Value;

                if (tunggal != Guid.Empty)
                    procedureIds.Add(tunggal);
            }

            // VAL-05. Wadah tanpa satu pun pemeriksaan tidak berarti apa-apa: ia bahan yang
            // diambil dari pasien tanpa ada yang akan dikerjakan darinya.
            if (procedureIds.Count == 0)
            {
                throw new LabSpecimenValidationException(
                    "Satu wadah harus memuat sekurang-kurangnya satu pemeriksaan.");
            }

            // VAL-07.
            if (procedureIds.Count != procedureIds.Distinct().Count())
            {
                throw new LabSpecimenValidationException(
                    "Pemeriksaan yang sama tidak boleh dimasukkan dua kali dalam satu wadah.");
            }

            var procedures = await _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .Where(x =>
                    procedureIds.Contains(x.Id) &&
                    x.IsLaboratory &&
                    x.IsActive &&
                    !x.IsDelete)
                .ToListAsync(cancellationToken);

            if (procedures.Count != procedureIds.Count)
            {
                throw new ArgumentException(
                    "Procedure komponen pemeriksaan tidak ditemukan, tidak aktif, atau bukan procedure laboratorium.");
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var nextSequence = await _dbContext.LabSpecimens
                .Where(x => x.LabOrderId == order.Id && !x.IsDelete)
                .Select(x => (int?)x.SpecimenSequence)
                .MaxAsync(cancellationToken) ?? 0;

            // Urutan daftar dipertahankan supaya daftar kerja petugas mengikuti urutan yang
            // dikirim pemanggil — lihat CreateExaminationsAsync.
            var berurutan = procedureIds
                .Select(id => procedures.First(p => p.Id == id))
                .ToList();

            var specimen = await CreateSpecimenAsync(
                order,
                nextSequence + 1,
                request.SpecimenDescription,
                supersededSpecimenId: null,
                recollectionCause: null,
                recollectionReason: null,
                actorUserId,
                now,
                cancellationToken);

            await CreateExaminationsAsync(order, specimen, berurutan, actorUserId, now, cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabSpecimen.Plan",
                "Merencanakan sampel laboratorium.",
                new
                {
                    specimen.Id,
                    specimen.LabOrderId,
                    specimen.SpecimenSequence,
                    ExaminationCount = berurutan.Count,
                    ActorUserId = actorUserId
                });

            return new LabSpecimenActionResult(specimen, null);
        }

        /// <summary>
        /// Membentuk baris pemeriksaan yang ditopang sebuah wadah, masing-masing dengan salinan
        /// tarifnya sendiri.
        ///
        /// Salinan tarif diambil per pemeriksaan, bukan sekali untuk seluruh wadah: hemoglobin
        /// dan leukosit berbeda harganya walaupun berasal dari tabung yang sama.
        /// </summary>
        private async Task CreateExaminationsAsync(
            LabOrder order,
            LabSpecimen specimen,
            IReadOnlyList<MstProcedure> procedures,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            foreach (var procedure in procedures)
            {
                var tariff = await ResolveTariffAsync(procedure.Id, now, cancellationToken);

                _dbContext.LabExaminations.Add(new LabExamination
                {
                    LabOrderId = order.Id,
                    SpecimenId = specimen.Id,
                    ProcedureId = procedure.Id,
                    ProcedureCodeSnapshot = procedure.ProcedureCode,
                    ProcedureNameSnapshot = procedure.ProcedureName,
                    TariffId = tariff?.Id,
                    TariffCodeSnapshot = tariff?.TariffCode,
                    UnitPriceSnapshot = tariff?.NormalPrice,
                    ExaminationStatus = LabExaminationStatus.Ordered,
                    Urgency = LabExaminationUrgency.Routine,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Memindahkan seluruh pemeriksaan yang ditopang sebuah wadah ke satu status.
        ///
        /// Inilah penegakan <c>AC-36</c> dan <c>VAL-13</c>: keputusan atas wadah berlaku untuk
        /// <b>seluruh</b> isinya sekaligus, karena semuanya berasal dari bahan yang sama. Tidak
        /// ada jalur yang memindahkan sebagian saja — pembatalan satu pemeriksaan adalah
        /// tindakan lain, dan tempatnya di <c>LabExaminationService</c>.
        ///
        /// Pemeriksaan yang sudah dibatalkan tersendiri tidak ikut dipindahkan: pembatalannya
        /// keputusan klinis tersendiri yang tidak boleh tertimpa keputusan atas wadah.
        ///
        /// Setiap pemeriksaan yang benar-benar berpindah meninggalkan satu baris riwayat
        /// berlingkup <c>LabExamination</c>, sebagaimana dituntut
        /// <c>contracts/permission-audit-matrix.md</c> bagian 4. Yang sudah berada pada status
        /// tujuan tidak menghasilkan baris apa pun — riwayat mencatat perpindahan, bukan
        /// pemanggilan.
        /// </summary>
        private async Task<int> MoveExaminationsAsync(
            LabOrder order,
            Guid specimenId,
            LabExaminationStatus toStatus,
            string action,
            string? reasonNote,
            DateTime? chargeEligibleAt,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var examinations = await _dbContext.LabExaminations
                .Where(x =>
                    x.SpecimenId == specimenId &&
                    !x.IsDelete &&
                    x.ExaminationStatus != LabExaminationStatus.Cancelled)
                .ToListAsync(cancellationToken);

            foreach (var examination in examinations)
            {
                if (examination.ExaminationStatus == toStatus) continue;

                var statusSebelum = examination.ExaminationStatus;

                examination.ExaminationStatus = toStatus;
                examination.ChargeEligibleAt = chargeEligibleAt ?? examination.ChargeEligibleAt;
                examination.UpdateDateTime = now;
                examination.UpdateBy = actorUserId;
                examination.Version++;

                AppendExaminationHistory(
                    order,
                    examination,
                    action,
                    statusSebelum.ToString(),
                    toStatus.ToString(),
                    reasonNote,
                    actorUserId,
                    now);
            }

            return examinations.Count;
        }

        /// <summary>
        /// Menulis satu baris riwayat berlingkup <c>LabExamination</c>.
        ///
        /// Barisnya menyebut pesanan dan pemeriksaannya, tetapi <b>tidak</b> menyebut wadahnya:
        /// yang berpindah adalah pemeriksaan itu, dan perpindahan wadah yang memicunya sudah
        /// punya barisnya sendiri.
        /// </summary>
        private void AppendExaminationHistory(
            LabOrder order,
            LabExamination examination,
            string action,
            string? fromStatus,
            string toStatus,
            string? reasonNote,
            Guid actorUserId,
            DateTime occurredAt)
        {
            _dbContext.LabTransitionHistories.Add(new LabTransitionHistory
            {
                Id = Guid.NewGuid(),
                LabOrderId = order.Id,
                LabExaminationId = examination.Id,
                EncounterId = order.EncounterId,
                Scope = LabTransitionScope.LabExamination,
                Action = action,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                ReasonNote = reasonNote,
                ActorUserId = actorUserId,
                OccurredAt = occurredAt,
                CorrelationId = order.Id,
                CreateDateTime = occurredAt,
                CreateBy = actorUserId
            });
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

            // VAL-08.
            if (specimen.SpecimenStatus != LabSpecimenStatus.Received)
            {
                throw new LabSpecimenConflictException(
                    "Wadah ini belum tercatat tiba di laboratorium, jadi belum bisa dinyatakan layak.");
            }

            // VAL-09 — aturan empat mata pada tingkat wadah.
            //
            // Ditulis di sini, bukan diserahkan ke konfigurasi permission, karena CAP-16 sudah
            // membuktikan sistem permission yang ada tidak dapat menegakkannya:
            // AccessPermissionService.HasAccessAsync hanya menjawab boleh atau tidak, dan tidak
            // pernah membandingkan siapa pelaku sebelumnya atas baris yang sama.
            //
            // Yang dijaga adalah penilaian mutu bahan. Orang yang mengambil sampel sudah punya
            // kepentingan pada hasilnya dinyatakan layak — bila ia juga yang menilai, tidak ada
            // mata kedua yang memeriksa pekerjaannya.
            if (specimen.CollectedByUserId.HasValue &&
                specimen.CollectedByUserId.Value != Guid.Empty &&
                specimen.CollectedByUserId.Value == actorUserId)
            {
                throw new LabSpecimenForbiddenException(
                    "Petugas yang mengambil sampel tidak boleh menyatakan kelayakannya.");
            }

            var now = DateTime.UtcNow;
            var fromStatus = specimen.SpecimenStatus;

            specimen.SpecimenStatus = LabSpecimenStatus.Accepted;
            specimen.DecidedAt = now;
            specimen.DecidedByUserId = actorUserId;
            specimen.UpdateDateTime = now;
            specimen.UpdateBy = actorUserId;
            specimen.Version++;

            // AC-37: seluruh pemeriksaan yang ditopang wadah ini menjadi layak tagih sekaligus,
            // dengan waktu keputusan yang sama.
            await MoveExaminationsAsync(
                order,
                specimen.Id,
                LabExaminationStatus.ChargeEligible,
                "Examination.ChargeEligible",
                reasonNote: null,
                chargeEligibleAt: now,
                actorUserId,
                now,
                cancellationToken);

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
                throw new LabSpecimenConflictException(
                    $"Wadah berstatus {specimen.SpecimenStatus} tidak dapat ditolak. " +
                    "Penolakan hanya berlaku atas wadah yang sudah diterima laboratorium.");
            }

            // VAL-10.
            var reasonCode = request.ReasonCode?.Trim();
            if (string.IsNullOrWhiteSpace(reasonCode))
                throw new LabSpecimenValidationException("Pilih alasan penolakan lebih dulu.");

            // VAL-11.
            var reason = await _dbContext.MstLabRejectionReasons
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.ReasonCode == reasonCode &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            if (reason == null)
                throw new LabSpecimenValidationException("Alasan penolakan yang dipilih tidak berlaku.");

            var note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

            // VAL-12.
            if (reason.RequiresNote && string.IsNullOrWhiteSpace(note))
            {
                throw new LabSpecimenValidationException(
                    "Alasan ini membutuhkan keterangan tambahan. Mohon isi catatannya.");
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var fromStatus = specimen.SpecimenStatus;

            // AC-36 dan VAL-13: penolakan berlaku untuk SELURUH pemeriksaan pada wadah ini,
            // karena semuanya berasal dari bahan yang sama. Bila bahannya tidak layak, tidak ada
            // satu pun di antaranya yang dapat dikerjakan.
            await MoveExaminationsAsync(
                order,
                specimen.Id,
                LabExaminationStatus.Voided,
                "Examination.Void",
                reasonNote: note,
                chargeEligibleAt: null,
                actorUserId,
                now,
                cancellationToken);

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

            // VAL-14.
            if (request.Cause == null)
                throw new LabSpecimenValidationException("Pilih sebab pengambilan ulang lebih dulu.");

            var cause = request.Cause.Value;

            if (!Enum.IsDefined(cause))
                throw new LabSpecimenValidationException("Sebab pengambilan ulang tidak dikenal.");

            var reasonText = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();

            // RJ-BIL-GATE-DEC-003: pengambilan ulang karena kondisi pasien atau sebab eksternal
            // memerlukan alasan dan otorisasi sebelum tagihan baru dipertimbangkan. Kesalahan
            // internal tidak menuntut otorisasi karena akibatnya memang tidak dibebankan pasien.
            // VAL-15.
            if (cause != LabRecollectionCause.InternalHospitalError && string.IsNullOrWhiteSpace(reasonText))
            {
                throw new LabSpecimenValidationException(
                    "Pengambilan ulang dengan sebab ini membutuhkan alasan tertulis.");
            }

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

            var nextSequence = await _dbContext.LabSpecimens
                .Where(x => x.LabOrderId == order.Id && !x.IsDelete)
                .Select(x => (int?)x.SpecimenSequence)
                .MaxAsync(cancellationToken) ?? 0;

            var replacement = await CreateSpecimenAsync(
                order,
                nextSequence + 1,
                specimen.SpecimenDescription,
                supersededSpecimenId: specimen.Id,
                recollectionCause: cause,
                recollectionReason: reasonText,
                actorUserId,
                now,
                cancellationToken);

            // AC-38. Wadah pengganti menampung SELURUH pemeriksaan wadah lama.
            //
            // Bahannya diambil ulang karena yang lama tidak layak — bukan karena permintaan
            // dokternya berubah. Menyalin hanya satu pemeriksaan akan diam-diam membatalkan
            // sisanya: pasien ditusuk ulang, tabungnya terkumpul, tetapi dua dari tiga
            // pemeriksaan yang diminta tidak pernah dikerjakan dan tidak ada yang menyadarinya.
            //
            // Pemeriksaan yang sudah dibatalkan tersendiri tidak ikut disalin — pembatalannya
            // keputusan klinis yang tetap berlaku pada bahan pengganti.
            var pemeriksaanLama = await _dbContext.LabExaminations
                .AsNoTracking()
                .Where(x =>
                    x.SpecimenId == specimen.Id &&
                    !x.IsDelete &&
                    x.ExaminationStatus != LabExaminationStatus.Cancelled)
                .OrderBy(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);

            var disalin = 0;

            if (pemeriksaanLama.Count > 0)
            {
                var procedureIdsLama = pemeriksaanLama.Select(x => x.ProcedureId).ToList();

                var proceduresLama = await _dbContext.Set<MstProcedure>()
                    .AsNoTracking()
                    .Where(x => procedureIdsLama.Contains(x.Id) && !x.IsDelete)
                    .ToListAsync(cancellationToken);

                // Urutan wadah lama dipertahankan supaya daftar kerja petugas tidak berubah
                // susunannya hanya karena bahannya diambil ulang.
                var berurutan = pemeriksaanLama
                    .Select(x => proceduresLama.FirstOrDefault(p => p.Id == x.ProcedureId))
                    .Where(x => x != null)
                    .Select(x => x!)
                    .ToList();

                if (berurutan.Count > 0)
                {
                    // Tarifnya diambil ulang pada waktu kejadian ini, bukan disalin dari baris
                    // lama: bila tarif berubah di antara keduanya, yang berlaku adalah tarif
                    // saat bahan penggantinya direncanakan.
                    await CreateExaminationsAsync(
                        order, replacement, berurutan, actorUserId, now, cancellationToken);

                    disalin = berurutan.Count;
                }
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "LabSpecimen.RequestRecollection",
                "Meminta pengambilan ulang sampel laboratorium.",
                new
                {
                    RejectedSpecimenId = specimen.Id,
                    ReplacementSpecimenId = replacement.Id,
                    Cause = cause.ToString(),
                    ExaminationCarriedOver = disalin,
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

        /// <summary>
        /// Keterangan bentuk layar wadah. Tidak menyentuh database sama sekali.
        /// </summary>
        public LabSpecimenFilterMetadataResponse GetFilterMetadata() =>
            LabFilterMetadataFactory.LabSpecimen();

        /// <summary>
        /// Rekap wadah pada satu rentang waktu, dihitung dari baris yang belum ditandai
        /// terhapus. Rentangnya memakai waktu wadah direncanakan.
        ///
        /// Tiga angka terakhir — sebab pengambilan ulang — sengaja dipisahkan karena angka
        /// kesalahan internal rumah sakitlah yang dibaca saat menilai apakah biaya pengambilan
        /// ulang boleh dibebankan kepada pasien (<c>LAB-INH-011</c>).
        /// </summary>
        public async Task<LabSpecimenSummaryResponse> GetSummaryAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            var source = _dbContext.LabSpecimens
                .AsNoTracking()
                .Where(x => !x.IsDelete &&
                            x.CreateDateTime >= startDate &&
                            x.CreateDateTime <= endDate);

            var rekap = await source
                .GroupBy(x => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Direncanakan = g.Count(x => x.SpecimenStatus == LabSpecimenStatus.Planned),
                    Diambil = g.Count(x => x.SpecimenStatus == LabSpecimenStatus.Collected),
                    Diterima = g.Count(x => x.SpecimenStatus == LabSpecimenStatus.Received),
                    DinyatakanLayak = g.Count(x => x.SpecimenStatus == LabSpecimenStatus.Accepted),
                    Ditolak = g.Count(x => x.SpecimenStatus == LabSpecimenStatus.Rejected),
                    PerluAmbilUlang = g.Count(x => x.SpecimenStatus == LabSpecimenStatus.RecollectionRequired),
                    Dibatalkan = g.Count(x => x.SpecimenStatus == LabSpecimenStatus.Cancelled),
                    Ditahan = g.Count(x => x.SpecimenStatus == LabSpecimenStatus.OnHold),
                    KesalahanInternal = g.Count(x => x.RecollectionCause == LabRecollectionCause.InternalHospitalError),
                    KondisiPasien = g.Count(x => x.RecollectionCause == LabRecollectionCause.PatientOrSpecimenCondition),
                    SebabEksternal = g.Count(x => x.RecollectionCause == LabRecollectionCause.ExternalCause)
                })
                .FirstOrDefaultAsync(cancellationToken);

            return new LabSpecimenSummaryResponse
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalWadah = rekap?.Total ?? 0,
                Direncanakan = rekap?.Direncanakan ?? 0,
                Diambil = rekap?.Diambil ?? 0,
                Diterima = rekap?.Diterima ?? 0,
                DinyatakanLayak = rekap?.DinyatakanLayak ?? 0,
                Ditolak = rekap?.Ditolak ?? 0,
                PerluAmbilUlang = rekap?.PerluAmbilUlang ?? 0,
                Dibatalkan = rekap?.Dibatalkan ?? 0,
                Ditahan = rekap?.Ditahan ?? 0,
                KesalahanInternalRumahSakit = rekap?.KesalahanInternal ?? 0,
                KondisiPasienAtauSampel = rekap?.KondisiPasien ?? 0,
                SebabEksternal = rekap?.SebabEksternal ?? 0
            };
        }

        public async Task<List<LabSpecimenResponse>> GetByOrderAsync(
            Guid labOrderId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.LabSpecimens
                .AsNoTracking()
                .Where(x => x.LabOrderId == labOrderId && !x.IsDelete)
                .OrderBy(x => x.SpecimenSequence)
                .Select(x => new LabSpecimenResponse
                {
                    Id = x.Id,
                    LabOrderId = x.LabOrderId,
                    SpecimenBarcode = x.SpecimenBarcode,
                    SpecimenSequence = x.SpecimenSequence,
                    SpecimenDescription = x.SpecimenDescription,
                    SpecimenStatus = x.SpecimenStatus.ToString(),
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
            return await _dbContext.LabTransitionHistories
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
        internal async Task<List<LabSpecimen>> CancelAllForOrderInMemoryAsync(
            LabOrder order,
            string? reason,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var specimens = await _dbContext.LabSpecimens
                .Where(x => x.LabOrderId == order.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);

            var previouslyAccepted = new List<LabSpecimen>();

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

        /// <summary>
        /// Membaca pemeriksaan yang ditopang sebuah wadah, sebagai satuan penerbitan fakta.
        ///
        /// Pemeriksaan yang dibatalkan tersendiri tidak ikut: pembatalannya keputusan klinis
        /// yang berarti pemeriksaan itu memang tidak dikerjakan, sehingga tidak ada yang layak
        /// ditagihkan darinya.
        /// </summary>
        private async Task<List<LabExamination>> LoadExaminationsForFactAsync(
            Guid specimenId,
            CancellationToken cancellationToken) =>
            await _dbContext.LabExaminations
                .AsNoTracking()
                .Where(x =>
                    x.SpecimenId == specimenId &&
                    !x.IsDelete &&
                    x.ExaminationStatus != LabExaminationStatus.Cancelled)
                .OrderBy(x => x.CreateDateTime)
                .ThenBy(x => x.ProcedureCodeSnapshot)
                .ToListAsync(cancellationToken);

        internal async Task<LabFactEmission> EmitClinicalCancellationAsync(
            LabSpecimen specimen,
            LabOrder order,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var examinations = await LoadExaminationsForFactAsync(specimen.Id, cancellationToken);
            var occurredAt = specimen.CancelDateTime ?? DateTime.UtcNow;

            // Wadah tanpa baris pemeriksaan berarti data peninggalan sebelum LAB-DEC-024.
            // Faktanya tetap diterbitkan atas wadah supaya jejaknya tidak hilang.
            if (examinations.Count == 0)
            {
                var tunggal = await _clinicalMilestoneFactProducer.EmitClinicalCancellationAsync(
                    BuildFactRequest(specimen, order, occurredAt, includeTariffSnapshot: false),
                    actorUserId,
                    cancellationToken);

                return LabFactEmission.Dari(tunggal);
            }

            var hasil = new List<ClinicalFactEmissionResult>();

            foreach (var examination in examinations)
            {
                hasil.Add(await _clinicalMilestoneFactProducer.EmitClinicalCancellationAsync(
                    BuildFactRequest(specimen, order, examination, occurredAt, includeTariffSnapshot: false),
                    actorUserId,
                    cancellationToken));
            }

            return LabFactEmission.Dari(hasil);
        }

        /// <summary>
        /// Menerbitkan fakta kelayakan tagih — <b>satu untuk setiap pemeriksaan</b> yang
        /// ditopang wadah ini (<c>FR-05.1</c>, <c>AC-37</c>).
        ///
        /// Inilah satuan yang benar. Wadah adalah bahan; yang ditagihkan adalah pemeriksaan yang
        /// dikerjakan darinya. Satu tabung darah ungu yang menopang hemoglobin, leukosit, dan
        /// trombosit menerbitkan tiga fakta dengan salinan tarifnya masing-masing — bukan satu
        /// fakta berharga satu tabung.
        ///
        /// Idempotensinya dijaga <c>SourceItemId</c> yang menunjuk identitas pemeriksaan:
        /// menekan tombol layak dua kali menerbitkan fakta atas pemeriksaan yang sama, dan
        /// producer mengenalinya sebagai pengiriman ulang.
        /// </summary>
        private async Task<LabFactEmission> EmitChargeEligibilityAsync(
            LabSpecimen specimen,
            LabOrder order,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var examinations = await LoadExaminationsForFactAsync(specimen.Id, cancellationToken);
            var occurredAt = specimen.DecidedAt ?? DateTime.UtcNow;

            if (examinations.Count == 0)
            {
                var tunggal = await _clinicalMilestoneFactProducer.EmitChargeEligibilityAsync(
                    BuildFactRequest(specimen, order, occurredAt, includeTariffSnapshot: true),
                    actorUserId,
                    cancellationToken);

                return LabFactEmission.Dari(tunggal);
            }

            var hasil = new List<ClinicalFactEmissionResult>();

            foreach (var examination in examinations)
            {
                hasil.Add(await _clinicalMilestoneFactProducer.EmitChargeEligibilityAsync(
                    BuildFactRequest(specimen, order, examination, occurredAt, includeTariffSnapshot: true),
                    actorUserId,
                    cancellationToken));
            }

            return LabFactEmission.Dari(hasil);
        }

        /// <summary>
        /// Muatan fakta untuk <b>satu pemeriksaan</b>.
        ///
        /// Seluruh nilai berasal dari baris pemeriksaan yang sudah tersimpan, bukan dari
        /// pembacaan ulang master data, agar pengiriman ulang menghasilkan muatan yang sama
        /// persis.
        /// </summary>
        private static ClinicalMilestoneFactRequest BuildFactRequest(
            LabSpecimen specimen,
            LabOrder order,
            LabExamination examination,
            DateTime occurredAt,
            bool includeTariffSnapshot)
        {
            return new ClinicalMilestoneFactRequest
            {
                SourceContext = BillingSourceContract.LaboratorySourceContext,
                SourceAggregateId = order.Id,

                // Inti FR-05.1: satuan fakta adalah pemeriksaan, bukan wadah.
                SourceItemId = examination.Id,

                EffectType = BillingSourceContract.LaboratoryChargeEffectType,
                EncounterId = order.EncounterId,
                OccurredAt = occurredAt,
                Quantity = includeTariffSnapshot ? 1m : null,
                Unit = includeTariffSnapshot ? ExaminationUnit : null,
                TariffSnapshot = includeTariffSnapshot
                    ? JsonSerializer.Serialize(new
                    {
                        source = "LaboratorySnapshot",
                        procedureCode = examination.ProcedureCodeSnapshot,
                        procedureName = examination.ProcedureNameSnapshot,
                        tariffCode = examination.TariffCodeSnapshot,
                        unitPrice = examination.UnitPriceSnapshot
                    })
                    : null,
                RuleSnapshot = JsonSerializer.Serialize(new
                {
                    milestone = includeTariffSnapshot ? "SpecimenAccepted" : "SpecimenCancelled",
                    specimenBarcode = specimen.SpecimenBarcode,
                    examinationId = examination.Id,
                    procedureCode = examination.ProcedureCodeSnapshot
                })
            };
        }

        /// <summary>
        /// Muatan fakta untuk wadah <b>peninggalan</b> yang tidak menopang satu baris pemeriksaan
        /// pun — data yang terbentuk sebelum <c>LAB-DEC-024</c>.
        ///
        /// Sejak <c>BE-LAB-11</c> wadah tidak lagi menyimpan salinan tarif, sehingga jalur ini
        /// menerbitkan fakta <b>tanpa</b> <c>TariffSnapshot</c>. Ia sengaja tidak dihapus:
        /// membiarkan wadah seperti itu tidak menerbitkan apa pun akan menghilangkan jejak
        /// tagihan yang sah. Billing menerima faktanya dan menilai nilainya sendiri.
        /// </summary>
        private static ClinicalMilestoneFactRequest BuildFactRequest(
            LabSpecimen specimen,
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

                // Tidak ada salinan tarif yang dapat disusun: keenam kolomnya sudah pindah ke
                // LabExamination, dan wadah ini justru tidak memiliki satu pun baris pemeriksaan.
                TariffSnapshot = null,
                RuleSnapshot = JsonSerializer.Serialize(new
                {
                    milestone = includeTariffSnapshot ? "SpecimenAccepted" : "SpecimenCancelled",
                    tariffSource = "LegacySpecimenWithoutExamination",
                    specimenBarcode = specimen.SpecimenBarcode,
                    specimenSequence = specimen.SpecimenSequence,
                    supersededSpecimenId = specimen.SupersededSpecimenId,
                    recollectionCause = specimen.RecollectionCause?.ToString()
                }),
                CorrelationId = order.Id
            };
        }

        private async Task<LabSpecimen> CreateSpecimenAsync(
            LabOrder order,
            int sequence,
            string? description,
            Guid? supersededSpecimenId,
            LabRecollectionCause? recollectionCause,
            string? recollectionReason,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var specimen = new LabSpecimen
            {
                Id = Guid.NewGuid(),
                LabOrderId = order.Id,
                SpecimenBarcode = GenerateSpecimenBarcode(),
                SpecimenSequence = sequence,
                SpecimenDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                SpecimenStatus = LabSpecimenStatus.Planned,
                SupersededSpecimenId = supersededSpecimenId,
                RecollectionCause = recollectionCause,
                RecollectionReason = recollectionReason,
                RecollectionAuthorizedByUserId = recollectionCause != null ? actorUserId : null,
                RecollectionAuthorizedAt = recollectionCause != null ? now : null,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.LabSpecimens.Add(specimen);

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
            LabSpecimen specimen,
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
            LabSpecimen? specimen,
            LabTransitionScope scope,
            string action,
            string? fromStatus,
            string toStatus,
            string? reasonCode,
            string? reasonNote,
            Guid actorUserId,
            DateTime occurredAt)
        {
            _dbContext.LabTransitionHistories.Add(new LabTransitionHistory
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

        private async Task<LabSpecimen> LoadSpecimenAsync(Guid specimenId, CancellationToken cancellationToken)
        {
            var specimen = await _dbContext.LabSpecimens
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
        LabSpecimen Specimen,
        LabFactEmission? Handoff);

    /// <summary>
    /// Hasil penerbitan fakta untuk satu keputusan atas wadah.
    ///
    /// Sejak <c>FR-05.1</c>, satu keputusan menerbitkan <b>sebanyak pemeriksaan</b> yang
    /// ditopang wadah itu — bukan satu. Karena itu hasilnya dikumpulkan: <see cref="Perwakilan"/>
    /// dipakai untuk menjawab pemanggil dengan bentuk yang sudah dikenalnya, sementara
    /// <see cref="FactIds"/> membawa seluruh identitas fakta yang benar-benar terbit.
    /// </summary>
    public sealed class LabFactEmission
    {
        private LabFactEmission(ClinicalFactEmissionResult perwakilan, IReadOnlyList<Guid> factIds)
        {
            Perwakilan = perwakilan;
            FactIds = factIds;
        }

        public ClinicalFactEmissionResult Perwakilan { get; }

        public IReadOnlyList<Guid> FactIds { get; }

        public int Count => FactIds.Count;

        // Yang dikumpulkan adalah MilestoneFactId, bukan ClinicalMilestoneFactId.
        //
        // Keduanya berbeda dan perbedaannya menentukan: ClinicalMilestoneFactId adalah identitas
        // BARIS, yang berganti setiap kali sebuah fakta memperoleh versi baru. MilestoneFactId
        // adalah identitas FAKTA itu sendiri, yang bertahan lintas versi. Memakai yang pertama
        // membuat pengiriman ulang tampak seperti fakta baru, dan idempotensi yang sesungguhnya
        // berjalan justru terbaca sebagai pelanggaran.
        public static LabFactEmission Dari(ClinicalFactEmissionResult hasil) =>
            new(hasil, hasil.MilestoneFactId.HasValue
                ? new[] { hasil.MilestoneFactId.Value }
                : Array.Empty<Guid>());

        public static LabFactEmission Dari(IReadOnlyList<ClinicalFactEmissionResult> hasil)
        {
            var ids = hasil
                .Where(x => x.MilestoneFactId.HasValue)
                .Select(x => x.MilestoneFactId!.Value)
                .ToList();

            return new LabFactEmission(hasil[0], ids);
        }
    }

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

    /// <summary>
    /// Pelanggaran aturan isi permintaan wadah. Dipetakan menjadi <c>422</c>.
    ///
    /// Dibedakan dari <see cref="ArgumentException"/> yang tetap menjadi <c>400</c>: matriks
    /// validasi menetapkan kode yang berbeda untuk aturan yang berbeda, dan layar membedakan
    /// keduanya — <c>400</c> berarti permintaannya cacat bentuk, <c>422</c> berarti bentuknya
    /// benar tetapi isinya melanggar aturan bisnis.
    /// </summary>
    public sealed class LabSpecimenValidationException(string message) : Exception(message);

    /// <summary>Bentrokan dengan keadaan yang sudah berjalan. Dipetakan menjadi <c>409</c>.</summary>
    public sealed class LabSpecimenConflictException(string message) : Exception(message);

    /// <summary>Tindakan di luar kewenangan pelakunya. Dipetakan menjadi <c>403</c>.</summary>
    public sealed class LabSpecimenForbiddenException(string message) : Exception(message);
}
